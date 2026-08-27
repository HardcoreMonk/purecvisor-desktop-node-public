#requires -Version 7.0

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [string]$ArtifactRoot = '',
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$IsoPath = '',
    [string]$VmRoot = '',
    [string]$ManagedVm = '',
    [string]$ForeignVm = '',
    [string]$CheckpointName = 'p0-restore',

    [ValidateSet('SavedOnly', 'Full')]
    [string]$Mode = 'SavedOnly',

    [ValidateRange(1, 3600)]
    [int]$JobTimeoutSeconds = 180,

    [ValidateRange(1, 1800)]
    [int]$CommandTimeoutSeconds = 120,

    [switch]$DryRun,

    [Parameter(DontShow)]
    [scriptblock]$RuntimeAdapter,

    [Parameter(DontShow)]
    [scriptblock]$SummaryWriter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-AbsolutePath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }
    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function Get-ShortHash {
    param([Parameter(Mandatory)][string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
    return ([Convert]::ToHexString($hash).Substring(0, 8)).ToLowerInvariant()
}

function Assert-VmName {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$VersionTag
    )

    if ($Name -notmatch '^pcv-p0-[A-Za-z0-9][A-Za-z0-9._-]{5,60}$' -or
        $Name -notlike "pcv-p0-$VersionTag-*") {
        throw "PCV_P0_VM_NAME_INVALID|$Name"
    }
}

function Assert-DedicatedVmRoot {
    param([Parameter(Mandatory)][string]$Path)

    $full = Get-AbsolutePath -Path $Path
    $volumeRoot = [System.IO.Path]::GetPathRoot($full)
    $relative = [System.IO.Path]::GetRelativePath($volumeRoot, $full)
    $segments = @($relative -split '[\\/]' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($full -eq $volumeRoot -or $segments.Count -lt 2) {
        throw "PCV_P0_CLEANUP_ROOT_INVALID|$full"
    }
    return $full.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
}

function Assert-ValidatedChildPath {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][string]$Candidate
    )

    $rootFull = (Get-AbsolutePath -Path $Root).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar)
    $candidateFull = Get-AbsolutePath -Path $Candidate
    $comparison = if ($IsWindows) {
        [System.StringComparison]::OrdinalIgnoreCase
    }
    else {
        [System.StringComparison]::Ordinal
    }
    if ($candidateFull.Equals($rootFull, $comparison) -or
        -not $candidateFull.StartsWith($rootFull + [System.IO.Path]::DirectorySeparatorChar, $comparison)) {
        throw "PCV_P0_CLEANUP_ROOT_INVALID|root=$rootFull|candidate=$candidateFull"
    }
    return $candidateFull
}

$versionTag = (($Version.Split('-')[0]) -replace '[^0-9A-Za-z]', '').ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($versionTag)) {
    throw 'PCV_P0_VERSION_INVALID'
}
if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path (Get-Location).Path "artifacts/service-plan-p0-actual-vm-$versionTag"
}
if ([string]::IsNullOrWhiteSpace($VmRoot)) {
    $VmRoot = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-service-plan-p0/$versionTag"
}

$artifactRootFull = Get-AbsolutePath -Path $ArtifactRoot
$vmRootFull = Assert-DedicatedVmRoot -Path $VmRoot
$campaignKey = Get-ShortHash -Value "$Version|$artifactRootFull"
if ([string]::IsNullOrWhiteSpace($ManagedVm)) {
    $ManagedVm = "pcv-p0-$versionTag-$campaignKey-managed"
}
if ([string]::IsNullOrWhiteSpace($ForeignVm)) {
    $ForeignVm = "pcv-p0-$versionTag-$campaignKey-foreign"
}
Assert-VmName -Name $ManagedVm -VersionTag $versionTag
Assert-VmName -Name $ForeignVm -VersionTag $versionTag
if ($ManagedVm -eq $ForeignVm) {
    throw 'PCV_P0_VM_NAME_INVALID|managed-and-foreign-must-differ'
}
$managedVmRootFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull $ManagedVm)
$foreignVmRootFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull $ForeignVm)
if ([string]::IsNullOrWhiteSpace($CheckpointName) -or $CheckpointName.IndexOfAny([char[]]'*?[]') -ge 0) {
    throw 'PCV_P0_CHECKPOINT_NAME_INVALID'
}

New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$summaryTempPath = Join-Path $artifactRootFull 'summary.json.tmp'
$startedAt = (Get-Date).ToUniversalTime()
$script:Steps = [System.Collections.Generic.List[object]]::new()
$script:VmRecords = [System.Collections.Generic.List[object]]::new()
$script:PcvCli = Join-Path (Get-AbsolutePath -Path $ProductRoot) 'pcvcli.exe'

$plannedSlices = if ($Mode -eq 'Full') {
    @('saved_lifecycle', 'media_attach', 'checkpoint_restore', 'managed_import', 'cleanup')
}
else {
    @('saved_lifecycle', 'cleanup')
}
$summary = [ordered]@{
    schema_version = 'pcv-service-plan-p0-actual-vm-summary/v1'
    scope = 'service-plan-p0-actual-vm'
    version = $Version
    mode = $Mode
    ok = $false
    overall_verdict = 'NOT_RUN'
    actual_execution = 'not-started'
    artifact_root_resolved = $artifactRootFull
    vm_root_resolved = $vmRootFull
    product_root_resolved = (Get-AbsolutePath -Path $ProductRoot)
    iso_path_resolved = if ([string]::IsNullOrWhiteSpace($IsoPath)) { $null } else { Get-AbsolutePath -Path $IsoPath }
    installed_manifest_version = $null
    installed_cli_sha256 = $null
    managed_vm = $ManagedVm
    managed_vm_id = $null
    foreign_vm = $ForeignVm
    foreign_vm_id = $null
    checkpoint_name = $CheckpointName
    slice_verdicts = [ordered]@{
        saved_lifecycle = 'NOT_RUN'
        media_attach = if ($Mode -eq 'Full') { 'NOT_RUN' } else { 'NOT_APPLICABLE' }
        checkpoint_restore = if ($Mode -eq 'Full') { 'NOT_RUN' } else { 'NOT_APPLICABLE' }
        managed_import = if ($Mode -eq 'Full') { 'NOT_RUN' } else { 'NOT_APPLICABLE' }
    }
    queued_jobs = [ordered]@{}
    hyperv_state_after_save = $null
    product_state_after_save = $null
    hyperv_state_after_resume = $null
    product_state_after_resume = $null
    readbacks = [ordered]@{}
    cleanup = [ordered]@{
        attempted = $false
        verdict = 'NOT_RUN'
        native_fallback_used = $false
        same_name_different_id_blocked = $false
        records = @()
        error = $null
    }
    plan = @($plannedSlices | ForEach-Object { [ordered]@{ slice = $_; mutates_host = ($_ -ne 'cleanup') } })
    steps = @()
    host_mutation_performed = $false
    secret_observed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    error = $null
    started_at = $startedAt.ToString('o')
    completed_at = $null
}

function Test-SecretMaterial {
    param([AllowNull()][string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return $false }
    $patterns = @(
        '(?i)\bbearer\s+[A-Za-z0-9._~+/=-]{6,}',
        '\beyJ[A-Za-z0-9_-]{6,}\.eyJ[A-Za-z0-9_-]{6,}\.[A-Za-z0-9_-]{6,}',
        '(?i)\b(?:token|password|secret)(?:_value)?\b\s*[:=]\s*["'']?(?!false\b|null\b|not-claimed\b|not-observed\b)[^\s,"'']{6,}'
    )
    return @($patterns | Where-Object { $Text -match $_ }).Count -gt 0
}

function Get-SafeFailureCode {
    param([AllowNull()][string]$Message)

    if (Test-SecretMaterial -Text $Message) { return 'PCV_P0_SECRET_OBSERVED' }
    $match = [regex]::Match([string]$Message, '\bPCV_[A-Z0-9_]+\b')
    if ($match.Success) { return $match.Value }
    return 'PCV_P0_INTERNAL_FAILURE'
}

function Set-SecretObserved {
    $summary.secret_observed = $true
    $summary.ok = $false
    $summary.overall_verdict = 'FAIL'
    $summary.error = 'PCV_P0_SECRET_OBSERVED'
}

function Write-AtomicSummary {
    try {
        $summary.steps = $script:Steps.ToArray()
        $json = $summary | ConvertTo-Json -Depth 32
        if (Test-SecretMaterial -Text $json) {
            $summary.secret_observed = $true
            throw 'PCV_P0_SECRET_OBSERVED_IN_SUMMARY'
        }
        if ($null -ne $SummaryWriter) {
            & $SummaryWriter $summaryPath $summaryTempPath $json
        }
        else {
            [System.IO.File]::WriteAllText(
                $summaryTempPath,
                $json,
                [System.Text.UTF8Encoding]::new($false))
            Move-Item -LiteralPath $summaryTempPath -Destination $summaryPath -Force
        }
    }
    catch {
        try {
            if (Test-Path -LiteralPath $summaryTempPath -PathType Leaf) {
                Remove-Item -LiteralPath $summaryTempPath -Force
            }
        }
        catch { }
        throw 'PCV_P0_SUMMARY_WRITE_FAILED'
    }
}

Write-AtomicSummary

if ($DryRun.IsPresent) {
    $summary.ok = $true
    $summary.overall_verdict = 'NOT_RUN'
    $summary.actual_execution = 'dry-run-no-installed-cli-or-hyperv'
    $summary.completed_at = (Get-Date).ToUniversalTime().ToString('o')
    Write-AtomicSummary
    return [pscustomobject]$summary
}

function Invoke-RuntimeOperation {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Alias('Input')][hashtable]$RuntimePayload = @{}
    )

    if ($null -eq $RuntimeAdapter) {
        throw "PCV_P0_RUNTIME_ADAPTER_NOT_CONFIGURED|$Operation"
    }
    return & $RuntimeAdapter $Operation $RuntimePayload
}

function Get-PcvVmByName {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Purpose
    )

    if ($null -ne $RuntimeAdapter) {
        return @(Invoke-RuntimeOperation -Operation 'vm-by-name' -Input @{
            name = $Name
            purpose = $Purpose
            vm_root = $vmRootFull
        })
    }
    return @(Get-VM -Name $Name -ErrorAction SilentlyContinue)
}

function Get-PcvVmById {
    param(
        [Parameter(Mandatory)][Guid]$Id,
        [Parameter(Mandatory)]$Record
    )

    if ($null -ne $RuntimeAdapter) {
        return Invoke-RuntimeOperation -Operation 'vm-by-id' -Input @{
            id = $Id.ToString('D')
            name = [string]$Record.name
            vm_root = $vmRootFull
        }
    }
    return Get-VM -Id $Id -ErrorAction SilentlyContinue
}

function New-PcvDirectory {
    param([Parameter(Mandatory)][string]$Path)

    if ($null -ne $RuntimeAdapter) {
        Invoke-RuntimeOperation -Operation 'create-directory' -Input @{ path = $Path } | Out-Null
        return
    }
    New-Item -ItemType Directory -Path $Path -ErrorAction Stop | Out-Null
}

function Test-PcvPath {
    param([Parameter(Mandatory)][string]$Path)

    if ($null -ne $RuntimeAdapter) {
        return [bool](Invoke-RuntimeOperation -Operation 'path-exists' -Input @{ path = $Path })
    }
    return Test-Path -LiteralPath $Path
}

function Assert-PcvPathAbsent {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Kind
    )

    if (Test-PcvPath -Path $Path) {
        throw "PCV_P0_VM_ROOT_ALREADY_EXISTS|kind=$Kind|path=$Path"
    }
}

function Remove-PcvDirectory {
    param([Parameter(Mandatory)][string]$Path)

    if ($null -ne $RuntimeAdapter) {
        Invoke-RuntimeOperation -Operation 'remove-directory' -Input @{ path = $Path } | Out-Null
        return
    }
    [System.IO.Directory]::Delete($Path, $true)
}

function Assert-InstalledProduct {
    if ($null -ne $RuntimeAdapter) {
        $installed = Invoke-RuntimeOperation -Operation 'installed-product'
        $summary.installed_manifest_version = [string]$installed.version
        $summary.installed_cli_sha256 = [string]$installed.cli_sha256
        $script:PcvCli = [string]$installed.cli_path
        if ([string]$installed.version -cne $Version) {
            throw "PCV_P0_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$($installed.version)"
        }
        if (-not [bool]$installed.iso_exists) { throw 'PCV_P0_ISO_NOT_FOUND' }
        return
    }
    $manifestPath = Join-Path $summary.product_root_resolved 'product-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "PCV_P0_INSTALLED_MANIFEST_MISSING|$manifestPath"
    }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json -Depth 32
    $installedVersion = if ($null -ne $manifest.PSObject.Properties['version']) {
        [string]$manifest.version
    }
    elseif ($null -ne $manifest.PSObject.Properties['product'] -and
        $null -ne $manifest.product.PSObject.Properties['version']) {
        [string]$manifest.product.version
    }
    else { '' }
    $summary.installed_manifest_version = $installedVersion
    if ($installedVersion -cne $Version) {
        throw "PCV_P0_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$installedVersion"
    }
    if (-not (Test-Path -LiteralPath $script:PcvCli -PathType Leaf)) {
        throw "PCV_P0_CLI_NOT_FOUND|$script:PcvCli"
    }
    $summary.installed_cli_sha256 = (Get-FileHash -LiteralPath $script:PcvCli -Algorithm SHA256).Hash.ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($summary.iso_path_resolved) -or
        -not (Test-Path -LiteralPath $summary.iso_path_resolved -PathType Leaf)) {
        throw "PCV_P0_ISO_NOT_FOUND|$($summary.iso_path_resolved)"
    }
}

function Assert-ServiceAvailable {
    if ($null -ne $RuntimeAdapter) {
        if ([string](Invoke-RuntimeOperation -Operation 'service-state') -ne 'Running') {
            throw 'PCV_P0_SERVICE_LOST'
        }
        return
    }
    $service = Get-Service -Name 'PureCVisorDesktopNode' -ErrorAction SilentlyContinue
    if ($null -eq $service -or [string]$service.Status -ne 'Running') {
        throw 'PCV_P0_SERVICE_LOST'
    }
}

function Assert-VmAbsent {
    param([Parameter(Mandatory)][string]$Name)

    if (@(Get-PcvVmByName -Name $Name -Purpose 'preflight').Count -ne 0) {
        throw "PCV_P0_VM_ALREADY_EXISTS|$Name"
    }
}

function Get-ObjectPropertyValue {
    param(
        $InputObject,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $InputObject -or $null -eq $InputObject.PSObject.Properties[$Name]) {
        return $null
    }
    return $InputObject.$Name
}

function Invoke-PcvCliJson {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    Assert-ServiceAvailable
    if ($null -ne $RuntimeAdapter) {
        $external = Invoke-RuntimeOperation -Operation 'invoke-cli' -Input @{
            step = $StepName
            arguments = @($Arguments)
            allow_failure = $AllowFailure.IsPresent
            timeout_seconds = $CommandTimeoutSeconds
        }
        $exitCode = [int](Get-ObjectPropertyValue -InputObject $external -Name 'exit_code')
        $stdout = [string](Get-ObjectPropertyValue -InputObject $external -Name 'stdout')
        $stderr = [string](Get-ObjectPropertyValue -InputObject $external -Name 'stderr')
    }
    else {
        $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = $script:PcvCli
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        $startInfo.ArgumentList.Add('--json')
        foreach ($argument in $Arguments) {
            $startInfo.ArgumentList.Add($argument)
        }
        $process = [System.Diagnostics.Process]::Start($startInfo)
        if ($null -eq $process) {
            throw "PCV_P0_COMMAND_START_FAILED|$StepName"
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($CommandTimeoutSeconds * 1000)) {
            try { $process.Kill($true) } catch { }
            throw "PCV_P0_COMMAND_TIMEOUT|$StepName"
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        $exitCode = [int]$process.ExitCode
    }
    $secretObserved = (Test-SecretMaterial -Text $stdout) -or (Test-SecretMaterial -Text $stderr)
    $payload = $null
    if (-not $secretObserved -and -not [string]::IsNullOrWhiteSpace($stdout)) {
        try { $payload = $stdout | ConvertFrom-Json -Depth 64 } catch { }
    }
    $script:Steps.Add([pscustomobject][ordered]@{
        step = $StepName
        exit_code = $exitCode
        status = if ($exitCode -eq 0) { 'completed' } else { 'failed' }
        at = (Get-Date).ToUniversalTime().ToString('o')
    }) | Out-Null
    if ($secretObserved) { Set-SecretObserved }
    if ($exitCode -ne 0 -and -not $AllowFailure.IsPresent) {
        throw "PCV_P0_COMMAND_FAILED|$StepName|exit=$exitCode"
    }
    return [pscustomobject]@{
        ExitCode = $exitCode
        Json = $payload
        SecretObserved = $secretObserved
    }
}

function Wait-PcvJobTerminal {
    param(
        [Parameter(Mandatory)][string]$JobId,
        [Parameter(Mandatory)][string]$StepName
    )

    $deadline = (Get-Date).AddSeconds($JobTimeoutSeconds)
    do {
        $result = Invoke-PcvCliJson -StepName "$StepName-job-get" -Arguments @('job', 'get', $JobId) -AllowFailure
        $data = Get-ObjectPropertyValue -InputObject $result.Json -Name 'data'
        $status = [string](Get-ObjectPropertyValue -InputObject $data -Name 'status')
        if ($status -in @('succeeded', 'failed', 'canceled')) {
            return $data
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    throw "PCV_P0_JOB_TIMEOUT|$StepName|job=$JobId"
}

function Start-PcvCliJob {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure,
        [switch]$DeferTerminalSummaryWrite
    )

    $created = Invoke-PcvCliJson -StepName $StepName -Arguments $Arguments -AllowFailure:$AllowFailure
    $data = Get-ObjectPropertyValue -InputObject $created.Json -Name 'data'
    $jobId = [string](Get-ObjectPropertyValue -InputObject $data -Name 'job_id')
    $initialStatus = [string](Get-ObjectPropertyValue -InputObject $data -Name 'status')
    $secretObserved = [bool]$created.SecretObserved
    if ($jobId -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{2,127}$') {
        if ($secretObserved) { Set-SecretObserved }
        throw "PCV_P0_JOB_ID_MISSING|$StepName"
    }
    if ([string]::IsNullOrWhiteSpace($initialStatus)) { $initialStatus = 'queued' }
    $summary.queued_jobs[$StepName] = [ordered]@{
        job_id = $jobId
        initial_status = $initialStatus
        status = $initialStatus
        polling_status = 'pending'
        terminal = $false
        error_code = $null
    }
    Write-AtomicSummary
    Assert-ServiceAvailable
    if ($secretObserved) {
        Set-SecretObserved
        $summary.queued_jobs[$StepName].status = 'secret_observed'
        $summary.queued_jobs[$StepName].polling_status = 'blocked'
        Write-AtomicSummary
        throw 'PCV_P0_SECRET_OBSERVED'
    }
    $summary.queued_jobs[$StepName].polling_status = 'polling'
    Write-AtomicSummary
    try {
        $job = if ($null -ne $RuntimeAdapter) {
            Invoke-RuntimeOperation -Operation 'wait-job' -Input @{
                step = $StepName
                job_id = $jobId
                timeout_seconds = $JobTimeoutSeconds
            }
        }
        else {
            Wait-PcvJobTerminal -JobId $jobId -StepName $StepName
        }
    }
    catch {
        $failureCode = Get-SafeFailureCode -Message $_.Exception.Message
        $summary.queued_jobs[$StepName].status = if ($failureCode -eq 'PCV_P0_JOB_TIMEOUT') { 'timed_out' } else { 'poll_error' }
        $summary.queued_jobs[$StepName].polling_status = if ($failureCode -eq 'PCV_P0_JOB_TIMEOUT') { 'timeout' } else { 'error' }
        $summary.queued_jobs[$StepName].error_code = $failureCode
        Write-AtomicSummary
        throw $failureCode
    }
    $status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
    if ($status -notin @('succeeded', 'failed', 'canceled')) { $status = 'invalid-terminal-status' }
    $errorObject = Get-ObjectPropertyValue -InputObject $job -Name 'error'
    $errorCode = [string](Get-ObjectPropertyValue -InputObject $errorObject -Name 'code')
    if (-not [string]::IsNullOrEmpty($errorCode) -and $errorCode -notmatch '^PCV_[A-Z0-9_]+$') {
        $errorCode = 'PCV_P0_REMOTE_ERROR_REDACTED'
    }
    $summary.queued_jobs[$StepName].status = $status
    $summary.queued_jobs[$StepName].polling_status = 'terminal'
    $summary.queued_jobs[$StepName].terminal = $true
    $summary.queued_jobs[$StepName].error_code = $errorCode
    if (-not $DeferTerminalSummaryWrite.IsPresent) { Write-AtomicSummary }
    if ($status -ne 'succeeded' -and -not $AllowFailure.IsPresent) {
        throw "PCV_P0_JOB_FAILED|$StepName|job=$JobId|status=$status"
    }
    return $job
}

function Wait-HyperVState {
    param(
        [Parameter(Mandatory)][Guid]$Id,
        [Parameter(Mandatory)][string]$Expected,
        [Parameter(Mandatory)][string]$Phase,
        [int]$TimeoutSeconds = 60
    )

    if ($null -ne $RuntimeAdapter) {
        return [string](Invoke-RuntimeOperation -Operation 'wait-hyperv-state' -Input @{
            id = $Id.ToString('D')
            expected = $Expected
            phase = $Phase
            timeout_seconds = $TimeoutSeconds
        })
    }
    $record = $script:VmRecords | Where-Object { $_.id -eq $Id.ToString('D') } | Select-Object -First 1
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $vm = Get-PcvVmById -Id $Id -Record $record
        if ($null -ne $vm -and [string]$vm.State -eq $Expected) {
            return [string]$vm.State
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    $final = Get-PcvVmById -Id $Id -Record $record
    return if ($null -eq $final) { $null } else { [string]$final.State }
}

function Get-ProductVmState {
    param(
        [Parameter(Mandatory)][string]$OperatorId,
        [Parameter(Mandatory)][string]$Phase
    )

    $result = Invoke-PcvCliJson -StepName 'vm-get-state' -Arguments @('vm', 'get', $OperatorId)
    $data = Get-ObjectPropertyValue -InputObject $result.Json -Name 'data'
    $state = Get-ObjectPropertyValue -InputObject $data -Name 'state'
    if ($null -eq $state) {
        $state = Get-ObjectPropertyValue -InputObject $data -Name 'power_state'
    }
    return ([string]$state).ToLowerInvariant()
}

function New-VmOwnershipRecord {
    param(
        [Parameter(Mandatory)][string]$Kind,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$ExpectedRoot
    )

    $recordedRoot = Assert-ValidatedChildPath -Root $vmRootFull -Candidate $ExpectedRoot
    $record = [pscustomobject][ordered]@{
        kind = $Kind
        name = $Name
        id = $null
        root = $recordedRoot
        root_owned_by_run = $false
        observed_id = $null
        observed_path = $null
        identity_status = 'reserved-before-mutation'
        identity_blocker = $false
        product_delete_attempted = $false
        native_fallback_used = $false
        removed = $false
        root_removed = $false
        same_name_different_id_blocked = $false
        error = $null
    }
    $script:VmRecords.Add($record) | Out-Null
    Write-AtomicSummary
    return $record
}

function Set-VmAuthoritativeIdentity {
    param(
        [Parameter(Mandatory)]$Record,
        [Parameter(Mandatory)]$Vm
    )

    try {
        $observedId = ([Guid]$Vm.Id).ToString('D')
        $observedPath = Get-AbsolutePath -Path ([string]$Vm.Path)
        $Record.observed_id = $observedId
        $Record.observed_path = $observedPath
        $reservedRoot = Get-AbsolutePath -Path ([string]$Record.root)
        $comparison = if ($IsWindows) {
            [System.StringComparison]::OrdinalIgnoreCase
        }
        else {
            [System.StringComparison]::Ordinal
        }
        $withinReservedRoot = $observedPath.Equals($reservedRoot, $comparison) -or
            $observedPath.StartsWith($reservedRoot + [System.IO.Path]::DirectorySeparatorChar, $comparison)
        if (-not $withinReservedRoot) {
            throw "PCV_P0_CLEANUP_ROOT_INVALID|observed-vm-outside-reserved-root"
        }
    }
    catch {
        $Record.identity_status = 'blocker'
        $Record.identity_blocker = $true
        $Record.error = Get-SafeFailureCode -Message $_.Exception.Message
        Write-AtomicSummary
        throw
    }
    $Record.id = $observedId
    if ($Record.kind -eq 'managed') { $summary.managed_vm_id = $Record.id }
    else { $summary.foreign_vm_id = $Record.id }
    $Record.identity_status = 'authoritative'
    $Record.identity_blocker = $false
    Write-AtomicSummary
    return $Record
}

function Assert-SlicePassed {
    param([Parameter(Mandatory)][string]$Slice)

    if ([string]$summary.slice_verdicts[$Slice] -ne 'PASS') {
        throw "PCV_P0_SLICE_FAILED|$Slice"
    }
}

function Invoke-TrackedSlice {
    param(
        [Parameter(Mandatory)][string]$Slice,
        [Parameter(Mandatory)][scriptblock]$Action
    )

    $summary.slice_verdicts[$Slice] = 'RUNNING'
    Write-AtomicSummary
    try {
        & $Action
        $summary.slice_verdicts[$Slice] = 'PASS'
        Write-AtomicSummary
    }
    catch {
        $summary.slice_verdicts[$Slice] = 'FAIL'
        Write-AtomicSummary
        throw
    }
}

function Invoke-SavedLifecycle {
    param([Parameter(Mandatory)]$Record)

    $create = Start-PcvCliJob -StepName 'vm-create' -Arguments @(
        'vm', 'create', '--name', $ManagedVm, '--iso', $summary.iso_path_resolved,
        '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '8', '--vm-root', $vmRootFull) -DeferTerminalSummaryWrite
    if ([string](Get-ObjectPropertyValue -InputObject $create -Name 'status') -ne 'succeeded') {
        throw 'PCV_P0_STATE_MISMATCH|create'
    }
    $createdVm = $null
    $jobResult = Get-ObjectPropertyValue -InputObject $create -Name 'result'
    $jobVmIdText = [string](Get-ObjectPropertyValue -InputObject $create -Name 'vm_id')
    if ([string]::IsNullOrWhiteSpace($jobVmIdText)) {
        $jobVmIdText = [string](Get-ObjectPropertyValue -InputObject $jobResult -Name 'vm_id')
    }
    $jobVmId = [Guid]::Empty
    if ([Guid]::TryParse($jobVmIdText, [ref]$jobVmId)) {
        $createdVm = Get-PcvVmById -Id $jobVmId -Record $Record
    }
    if ($null -eq $createdVm) {
        $createdRows = @(Get-PcvVmByName -Name $ManagedVm -Purpose 'authoritative-create')
        if ($createdRows.Count -eq 1) { $createdVm = $createdRows[0] }
        else {
            $Record.identity_status = 'orphan-blocker'
            $Record.identity_blocker = $true
            Write-AtomicSummary
            throw "PCV_P0_STATE_MISMATCH|created-vm-cardinality=$($createdRows.Count)"
        }
    }
    $Record = Set-VmAuthoritativeIdentity -Record $Record -Vm $createdVm
    $id = [Guid]$Record.id

    $start = Start-PcvCliJob -StepName 'vm-start' -Arguments @('vm', 'start', $Record.id)
    $running = Wait-HyperVState -Id $id -Expected 'Running' -Phase 'after-start'
    if ([string](Get-ObjectPropertyValue -InputObject $start -Name 'status') -ne 'succeeded' -or $running -ne 'Running') {
        throw "PCV_P0_STATE_MISMATCH|start|hyperv=$running"
    }

    $save = Start-PcvCliJob -StepName 'vm-save' -Arguments @('vm', 'save', $Record.id)
    $hypervSaved = Wait-HyperVState -Id $id -Expected 'Saved' -Phase 'after-save'
    $productSaved = Get-ProductVmState -OperatorId $ManagedVm -Phase 'after-save'
    $summary.hyperv_state_after_save = $hypervSaved
    $summary.product_state_after_save = $productSaved
    $summary.readbacks.saved_not_paused = ($hypervSaved -ne 'Paused')
    Write-AtomicSummary
    if ([string](Get-ObjectPropertyValue -InputObject $save -Name 'status') -ne 'succeeded' -or
        $hypervSaved -ne 'Saved' -or $hypervSaved -eq 'Paused' -or $productSaved -ne 'saved') {
        throw "PCV_P0_STATE_MISMATCH|save|hyperv=$hypervSaved|product=$productSaved"
    }

    $resume = Start-PcvCliJob -StepName 'vm-resume-saved' -Arguments @('vm', 'resume-saved', $Record.id)
    $hypervRunning = Wait-HyperVState -Id $id -Expected 'Running' -Phase 'after-resume'
    $productStateAfterResume = Get-ProductVmState -OperatorId $ManagedVm -Phase 'after-resume'
    $summary.hyperv_state_after_resume = $hypervRunning
    $summary.product_state_after_resume = $productStateAfterResume
    if ([string](Get-ObjectPropertyValue -InputObject $resume -Name 'status') -ne 'succeeded' -or
        $hypervRunning -ne 'Running' -or $productStateAfterResume -ne 'running') {
        throw "PCV_P0_STATE_MISMATCH|resume|hyperv=$hypervRunning|product=$productStateAfterResume"
    }
}

function Invoke-MediaAttachSlice {
    $record = $script:VmRecords | Where-Object kind -eq 'managed' | Select-Object -First 1
    $job = Start-PcvCliJob -StepName 'vm-attach' -Arguments @('vm', 'attach', $record.id, '--iso', $summary.iso_path_resolved)
    $dvd = if ($null -ne $RuntimeAdapter) {
        Invoke-RuntimeOperation -Operation 'dvd-readback' -Input @{
            id = $record.id
            iso = $summary.iso_path_resolved
        }
    }
    else {
        Get-VMDvdDrive -VMId ([Guid]$record.id) -ErrorAction Stop | Select-Object -First 1
    }
    $hostResource = if ($null -ne $dvd.PSObject.Properties['HostResource']) {
        [string](@($dvd.HostResource) | Select-Object -First 1)
    }
    else { [string]$dvd.Path }
    $summary.readbacks.media_attach = [ordered]@{
        HostResource = $hostResource
        iso = $summary.iso_path_resolved
    }
    $matches = -not [string]::IsNullOrWhiteSpace($hostResource) -and
        (Get-AbsolutePath -Path $hostResource).Equals(
            $summary.iso_path_resolved,
            [System.StringComparison]::OrdinalIgnoreCase)
    if ([string](Get-ObjectPropertyValue -InputObject $job -Name 'status') -ne 'succeeded' -or -not $matches) {
        throw "PCV_P0_STATE_MISMATCH|media-attach|HostResource=$hostResource"
    }
}

function Invoke-CheckpointRestoreSlice {
    $record = $script:VmRecords | Where-Object kind -eq 'managed' | Select-Object -First 1
    Start-PcvCliJob -StepName 'checkpoint-create' -Arguments @(
        'vm', 'checkpoint', 'create', $record.id, '--name', $CheckpointName) | Out-Null
    $restore = Start-PcvCliJob -StepName 'checkpoint-restore' -Arguments @(
        'vm', 'checkpoint', 'restore', $record.id, $CheckpointName)
    $rows = if ($null -ne $RuntimeAdapter) {
        @(Invoke-RuntimeOperation -Operation 'checkpoint-list' -Input @{
            id = $record.id
            name = $CheckpointName
        })
    }
    else {
        $listed = Invoke-PcvCliJson -StepName 'checkpoint-list-after-restore' -Arguments @(
            'vm', 'checkpoint', 'list', $record.id)
        @((Get-ObjectPropertyValue -InputObject $listed.Json -Name 'data'))
    }
    $current = @($rows | Where-Object {
        $name = Get-ObjectPropertyValue -InputObject $_ -Name 'name'
        if ($null -eq $name) { $name = Get-ObjectPropertyValue -InputObject $_ -Name 'checkpoint_name' }
        [string]$name -eq $CheckpointName -and
            [bool](Get-ObjectPropertyValue -InputObject $_ -Name 'is_current')
    })
    $summary.readbacks.checkpoint_restore = [ordered]@{
        checkpoint = $CheckpointName
        is_current = ($current.Count -eq 1)
        current_row_count = $current.Count
    }
    if ([string](Get-ObjectPropertyValue -InputObject $restore -Name 'status') -ne 'succeeded' -or
        $current.Count -ne 1) {
        throw "PCV_P0_STATE_MISMATCH|checkpoint-restore|current-count=$($current.Count)"
    }
}

function Invoke-ManagedImportSlice {
    $foreignRoot = $foreignVmRootFull
    $record = New-VmOwnershipRecord -Kind 'foreign' -Name $ForeignVm -ExpectedRoot $foreignRoot
    New-PcvDirectory -Path $foreignRoot
    $record.root_owned_by_run = $true
    Write-AtomicSummary
    $vhdPath = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $foreignRoot 'disk0.vhdx')
    if ($null -ne $RuntimeAdapter) {
        $foreignVmResult = Invoke-RuntimeOperation -Operation 'create-foreign-vm' -Input @{
            name = $ForeignVm
            vm_root = $vmRootFull
            root = $foreignRoot
            vhd_path = $vhdPath
        }
    }
    else {
        New-VHD -Path $vhdPath -SizeBytes 1GB -Dynamic | Out-Null
        New-VM -Name $ForeignVm -Generation 2 -MemoryStartupBytes 512MB -VHDPath $vhdPath -Path $foreignRoot | Out-Null
        $foreignRows = @(Get-PcvVmByName -Name $ForeignVm -Purpose 'authoritative-create')
        if ($foreignRows.Count -ne 1) {
            $record.identity_status = 'orphan-blocker'
            $record.identity_blocker = $true
            Write-AtomicSummary
            throw "PCV_P0_STATE_MISMATCH|foreign-vm-cardinality=$($foreignRows.Count)"
        }
        $foreignVmResult = $foreignRows[0]
    }
    $record = Set-VmAuthoritativeIdentity -Record $record -Vm $foreignVmResult

    $rejected = Start-PcvCliJob -StepName 'unmanaged-delete' -Arguments @(
        'vm', 'delete', $record.name, '--yes') -AllowFailure
    $rejectStatus = [string](Get-ObjectPropertyValue -InputObject $rejected -Name 'status')
    $rejectError = Get-ObjectPropertyValue -InputObject $rejected -Name 'error'
    $rejectCode = [string](Get-ObjectPropertyValue -InputObject $rejectError -Name 'code')
    $stillPresent = $null -ne (Get-PcvVmById -Id ([Guid]$record.id) -Record $record)
    if ($rejectStatus -ne 'failed' -or $rejectCode -ne 'PCV_VM_NOT_MANAGED_BY_PURECVISOR' -or -not $stillPresent) {
        throw "PCV_P0_STATE_MISMATCH|unmanaged-delete|status=$rejectStatus|code=$rejectCode"
    }

    $managed = Start-PcvCliJob -StepName 'vm-manage' -Arguments @('vm', 'manage', $record.id, '--yes')
    $managedVm = Get-PcvVmById -Id ([Guid]$record.id) -Record $record
    $markerPresent = [string]$managedVm.Notes -match 'managed-by=purecvisor-desktop-node'
    $deleted = Start-PcvCliJob -StepName 'managed-delete' -Arguments @('vm', 'delete', $record.name, '--yes')
    $gone = $null -eq (Get-PcvVmById -Id ([Guid]$record.id) -Record $record)
    $summary.readbacks.managed_import = [ordered]@{
        unmanaged_delete_rejected = $true
        manage_marker_present = $markerPresent
        managed_delete_absent = $gone
    }
    if ([string](Get-ObjectPropertyValue -InputObject $managed -Name 'status') -ne 'succeeded' -or
        -not $markerPresent -or
        [string](Get-ObjectPropertyValue -InputObject $deleted -Name 'status') -ne 'succeeded' -or
        -not $gone) {
        throw 'PCV_P0_STATE_MISMATCH|managed-import'
    }
}

function Get-ValidatedCleanupVm {
    param(
        [Parameter(Mandatory)]$Record,
        [Parameter(Mandatory)][Guid]$RecordedId,
        [Parameter(Mandatory)][string]$Phase,
        [switch]$AllowAbsent
    )

    $current = Get-PcvVmById -Id $RecordedId -Record $Record
    if ($null -eq $current) {
        if ($AllowAbsent.IsPresent) { return $null }
        $Record.identity_status = 'cleanup-blocker'
        $Record.identity_blocker = $true
        throw "PCV_P0_CLEANUP_IDENTITY_DRIFT|phase=$Phase|missing-recorded-id"
    }

    try {
        $currentId = ([Guid]$current.Id).ToString('D')
        $currentName = [string]$current.Name
        $currentPath = Get-AbsolutePath -Path ([string]$current.Path)
        $recordedPath = Get-AbsolutePath -Path ([string]$Record.observed_path)
        $reservedRoot = Get-AbsolutePath -Path ([string]$Record.root)
        $comparison = if ($IsWindows) {
            [System.StringComparison]::OrdinalIgnoreCase
        }
        else {
            [System.StringComparison]::Ordinal
        }
        $matches = $currentId -eq $RecordedId.ToString('D') -and
            $currentName.Equals([string]$Record.name, $comparison) -and
            $currentPath.Equals($recordedPath, $comparison) -and
            ($currentPath.Equals($reservedRoot, $comparison) -or
                $currentPath.StartsWith($reservedRoot + [System.IO.Path]::DirectorySeparatorChar, $comparison))
        if (-not $matches) { throw 'identity-mismatch' }
    }
    catch {
        $Record.identity_status = 'cleanup-blocker'
        $Record.identity_blocker = $true
        throw "PCV_P0_CLEANUP_IDENTITY_DRIFT|phase=$Phase"
    }
    return $current
}

function Invoke-ExactCleanup {
    $summary.cleanup.attempted = $true
    $cleanupErrors = [System.Collections.Generic.List[string]]::new()
    foreach ($record in $script:VmRecords) {
        try {
            $record.root = Assert-ValidatedChildPath -Root $vmRootFull -Candidate $record.root
            $sameName = @(Get-PcvVmByName -Name $record.name -Purpose 'cleanup-observation')
            if ([string]::IsNullOrWhiteSpace([string]$record.id)) {
                $record.identity_status = 'orphan-blocker'
                $record.identity_blocker = $true
                $record.error = 'PCV_P0_CLEANUP_ID_MISMATCH'
                $cleanupErrors.Add('PCV_P0_CLEANUP_ID_MISMATCH') | Out-Null
                continue
            }
            $recordedId = [Guid]$record.id
            $different = @($sameName | Where-Object { [Guid]$_.Id -ne $recordedId })
            $collisionCode = $null
            if ($different.Count -gt 0) {
                $record.same_name_different_id_blocked = $true
                $summary.cleanup.same_name_different_id_blocked = $true
                $collisionCode = 'PCV_P0_CLEANUP_ID_MISMATCH'
            }

            $current = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-product-delete' -AllowAbsent
            if ($null -ne $current) {
                $record.product_delete_attempted = $true
                try {
                    $delete = Start-PcvCliJob -StepName "cleanup-delete-$($record.kind)" -Arguments @(
                        'vm', 'delete', $record.name, '--yes') -AllowFailure
                    if ([string](Get-ObjectPropertyValue -InputObject $delete -Name 'status') -ne 'succeeded') {
                        throw 'product-delete-failed'
                    }
                }
                catch { }
                $current = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-native-fallback' -AllowAbsent
                if ($null -ne $current) {
                    $record.native_fallback_used = $true
                    $summary.cleanup.native_fallback_used = $true
                    if ([string]$current.State -ne 'Off') {
                        $current = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-native-stop'
                        if ($null -ne $RuntimeAdapter) {
                            Invoke-RuntimeOperation -Operation 'stop-vm' -Input @{ id = $record.id } | Out-Null
                        }
                        else {
                            Stop-VM -VM $current -TurnOff -Force -ErrorAction Stop
                        }
                    }
                    $current = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-native-delete' -AllowAbsent
                    if ($null -ne $current) {
                        if ($null -ne $RuntimeAdapter) {
                            Invoke-RuntimeOperation -Operation 'remove-vm' -Input @{ id = $record.id } | Out-Null
                        }
                        else {
                            Remove-VM -VM $current -Force -ErrorAction Stop
                        }
                    }
                }
            }
            $record.removed = $null -eq (Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'after-delete' -AllowAbsent)
            if (-not $record.removed) {
                throw "PCV_P0_CLEANUP_ID_MISMATCH|remaining=$recordedId"
            }
            $comparison = if ($IsWindows) { [System.StringComparison]::OrdinalIgnoreCase } else { [System.StringComparison]::Ordinal }
            $collisionOwnsRecordedRoot = @($different | Where-Object {
                $candidate = Get-AbsolutePath -Path ([string]$_.Path)
                $candidate.Equals($record.root, $comparison) -or
                    $candidate.StartsWith($record.root + [System.IO.Path]::DirectorySeparatorChar, $comparison)
            }).Count -gt 0
            if ($record.root_owned_by_run -and -not $collisionOwnsRecordedRoot -and (Test-PcvPath -Path $record.root)) {
                $beforeRootRemoval = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-root-removal' -AllowAbsent
                if ($null -ne $beforeRootRemoval) {
                    throw "PCV_P0_CLEANUP_IDENTITY_DRIFT|phase=before-root-removal|recorded-id-present"
                }
                Remove-PcvDirectory -Path $record.root
            }
            $record.root_removed = -not (Test-PcvPath -Path $record.root)
            if (-not $record.root_removed) {
                throw "PCV_P0_CLEANUP_ROOT_INVALID|remaining=$($record.root)"
            }
            if ($null -ne $collisionCode) {
                $record.error = $collisionCode
                $cleanupErrors.Add($collisionCode) | Out-Null
            }
        }
        catch {
            $safeCode = Get-SafeFailureCode -Message $_.Exception.Message
            $record.error = $safeCode
            $cleanupErrors.Add($safeCode) | Out-Null
        }
    }
    $summary.cleanup.records = @($script:VmRecords)
    if ($cleanupErrors.Count -gt 0) {
        $summary.cleanup.verdict = 'FAIL'
        $summary.cleanup.error = @($cleanupErrors | Select-Object -Unique) -join '; '
        return $false
    }
    $summary.cleanup.verdict = 'PASS'
    return $true
}

$runError = $null
try {
    Assert-InstalledProduct
    Assert-ServiceAvailable
    Assert-VmAbsent -Name $ManagedVm
    Assert-VmAbsent -Name $ForeignVm
    Assert-PcvPathAbsent -Path $managedVmRootFull -Kind 'managed'
    Assert-PcvPathAbsent -Path $foreignVmRootFull -Kind 'foreign'
    Write-AtomicSummary

    $managedRecord = New-VmOwnershipRecord -Kind 'managed' -Name $ManagedVm -ExpectedRoot $managedVmRootFull
    $summary.host_mutation_performed = $true
    $summary.actual_execution = 'installed-cli-and-hyperv'
    if (-not (Test-PcvPath -Path $vmRootFull)) { New-PcvDirectory -Path $vmRootFull }
    New-PcvDirectory -Path $managedRecord.root
    $managedRecord.root_owned_by_run = $true
    Write-AtomicSummary
    Invoke-TrackedSlice -Slice 'saved_lifecycle' -Action { Invoke-SavedLifecycle -Record $managedRecord }
    Assert-SlicePassed -Slice 'saved_lifecycle'

    if ($Mode -eq 'Full') {
        Invoke-TrackedSlice -Slice 'media_attach' -Action { Invoke-MediaAttachSlice }
        Assert-SlicePassed -Slice 'media_attach'
        Invoke-TrackedSlice -Slice 'checkpoint_restore' -Action { Invoke-CheckpointRestoreSlice }
        Assert-SlicePassed -Slice 'checkpoint_restore'
        Invoke-TrackedSlice -Slice 'managed_import' -Action { Invoke-ManagedImportSlice }
        Assert-SlicePassed -Slice 'managed_import'
    }
}
catch {
    $runError = Get-SafeFailureCode -Message $_.Exception.Message
    $summary.error = $runError
}
finally {
    $cleanupOk = $false
    try {
        $cleanupOk = Invoke-ExactCleanup
    }
    catch {
        $summary.cleanup.attempted = $true
        $summary.cleanup.verdict = 'FAIL'
        $cleanupCode = Get-SafeFailureCode -Message $_.Exception.Message
        $summary.cleanup.error = $cleanupCode
        $runError = if ($null -eq $runError) { $cleanupCode } else { "$runError; $cleanupCode" }
    }
    try {
        Assert-ServiceAvailable
    }
    catch {
        $runError = if ($null -eq $runError) { 'PCV_P0_SERVICE_LOST' } else { "$runError; PCV_P0_SERVICE_LOST" }
    }
    $applicableSlices = @('saved_lifecycle')
    if ($Mode -eq 'Full') {
        $applicableSlices += @('media_attach', 'checkpoint_restore', 'managed_import')
    }
    $slicesOk = @($applicableSlices | Where-Object { $summary.slice_verdicts[$_] -ne 'PASS' }).Count -eq 0
    $summary.ok = ($null -eq $runError -and $cleanupOk -and $slicesOk -and -not [bool]$summary.secret_observed)
    $summary.overall_verdict = if ($summary.ok) { 'PASS' } else { 'FAIL' }
    if ($null -ne $runError) { $summary.error = $runError }
    $summary.completed_at = (Get-Date).ToUniversalTime().ToString('o')
    Write-AtomicSummary
}

$result = [pscustomobject]$summary
$result
if (-not $summary.ok) {
    throw "PCV_P0_FAILED|$($summary.error)"
}
