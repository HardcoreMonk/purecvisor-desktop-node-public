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
    [string]$SourceVm = '',
    [string]$TargetVm = '',

    [ValidateRange(1, 3600)]
    [int]$JobTimeoutSeconds = 180,
    [ValidateRange(1, 1800)]
    [int]$CommandTimeoutSeconds = 120,

    [switch]$DryRun,
    [Parameter(DontShow)][scriptblock]$RuntimeAdapter,
    [Parameter(DontShow)][scriptblock]$SummaryWriter
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

    if ($Name -notmatch '^pcv-p1-clone-[A-Za-z0-9][A-Za-z0-9._-]{5,60}$' -or
        $Name -notlike "pcv-p1-clone-$VersionTag-*") {
        throw "PCV_P1_CLONE_VM_NAME_INVALID|$Name"
    }
}

function Assert-DedicatedVmRoot {
    param([Parameter(Mandatory)][string]$Path)

    $full = Get-AbsolutePath -Path $Path
    $volumeRoot = [System.IO.Path]::GetPathRoot($full)
    $relative = [System.IO.Path]::GetRelativePath($volumeRoot, $full)
    $segments = @($relative -split '[\\/]' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($full -eq $volumeRoot -or $segments.Count -lt 2) {
        throw "PCV_P1_CLONE_CLEANUP_ROOT_INVALID|$full"
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
        throw "PCV_P1_CLONE_CLEANUP_ROOT_INVALID|root=$rootFull|candidate=$candidateFull"
    }
    return $candidateFull
}

$versionTag = (($Version.Split('-')[0]) -replace '[^0-9A-Za-z]', '').ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($versionTag)) {
    throw 'PCV_P1_CLONE_VERSION_INVALID'
}
if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path (Get-Location).Path "artifacts/service-plan-p1-clone-actual-vm-$versionTag"
}
if ([string]::IsNullOrWhiteSpace($VmRoot)) {
    $VmRoot = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-service-plan-p1-clone/$versionTag"
}

$artifactRootFull = Get-AbsolutePath -Path $ArtifactRoot
$vmRootFull = Assert-DedicatedVmRoot -Path $VmRoot
$campaignKey = Get-ShortHash -Value "$Version|$artifactRootFull"
if ([string]::IsNullOrWhiteSpace($SourceVm)) {
    $SourceVm = "pcv-p1-clone-$versionTag-$campaignKey-src"
}
if ([string]::IsNullOrWhiteSpace($TargetVm)) {
    $TargetVm = "pcv-p1-clone-$versionTag-$campaignKey-dst"
}
Assert-VmName -Name $SourceVm -VersionTag $versionTag
Assert-VmName -Name $TargetVm -VersionTag $versionTag
if ($SourceVm -eq $TargetVm) {
    throw 'PCV_P1_CLONE_VM_NAME_INVALID|source-and-target-must-differ'
}
$sourceVmRootFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull $SourceVm)
$targetVmRootFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull $TargetVm)

New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$summaryTempPath = Join-Path $artifactRootFull 'summary.json.tmp'
$startedAt = (Get-Date).ToUniversalTime()
$script:Steps = [System.Collections.Generic.List[object]]::new()
$script:VmRecords = [System.Collections.Generic.List[object]]::new()
$script:PcvCli = Join-Path (Get-AbsolutePath -Path $ProductRoot) 'pcvcli.exe'

$plannedSlices = @('source_create', 'preview_mismatch', 'preview_ok', 'clone_ok', 'cleanup')
$summary = [ordered]@{
    schema_version = 'pcv-service-plan-p1-clone-actual-vm-summary/v1'
    scope = 'service-plan-p1-clone-actual-vm'
    version = $Version
    ok = $false
    overall_verdict = 'NOT_RUN'
    actual_execution = 'not-started'
    artifact_root_resolved = $artifactRootFull
    vm_root_resolved = $vmRootFull
    product_root_resolved = (Get-AbsolutePath -Path $ProductRoot)
    iso_path_resolved = if ([string]::IsNullOrWhiteSpace($IsoPath)) { $null } else { Get-AbsolutePath -Path $IsoPath }
    installed_manifest_version = $null
    installed_cli_sha256 = $null
    source_vm = $SourceVm
    source_vm_id = $null
    target_vm = $TargetVm
    target_vm_id = $null
    slice_verdicts = [ordered]@{
        source_create = 'NOT_RUN'
        preview_mismatch = 'NOT_RUN'
        preview_ok = 'NOT_RUN'
        clone_ok = 'NOT_RUN'
        cleanup = 'NOT_RUN'
    }
    queued_jobs = [ordered]@{}
    hyperv_state_after_create = $null
    product_state_after_create = $null
    hyperv_state_after_clone_target = $null
    product_state_after_clone_target = $null
    hyperv_state_after_clone_source = $null
    product_state_after_clone_source = $null
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

    if (Test-SecretMaterial -Text $Message) { return 'PCV_P1_CLONE_SECRET_OBSERVED' }
    $match = [regex]::Match([string]$Message, '\bPCV_[A-Z0-9_]+\b')
    if ($match.Success) { return $match.Value }
    return 'PCV_P1_CLONE_INTERNAL_FAILURE'
}

function Set-SecretObserved {
    $summary.secret_observed = $true
    $summary.ok = $false
    $summary.overall_verdict = 'FAIL'
    $summary.error = 'PCV_P1_CLONE_SECRET_OBSERVED'
}

function Write-AtomicSummary {
    try {
        $summary.steps = $script:Steps.ToArray()
        $json = $summary | ConvertTo-Json -Depth 32
        if (Test-SecretMaterial -Text $json) {
            $summary.secret_observed = $true
            throw 'PCV_P1_CLONE_SECRET_OBSERVED_IN_SUMMARY'
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
        throw 'PCV_P1_CLONE_SUMMARY_WRITE_FAILED'
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
        throw "PCV_P1_CLONE_RUNTIME_ADAPTER_NOT_CONFIGURED|$Operation"
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
        throw "PCV_P1_CLONE_VM_ROOT_ALREADY_EXISTS|kind=$Kind|path=$Path"
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
            throw "PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$($installed.version)"
        }
        if (-not [bool]$installed.iso_exists) { throw 'PCV_P1_CLONE_ISO_NOT_FOUND' }
        return
    }
    $manifestPath = Join-Path $summary.product_root_resolved 'product-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "PCV_P1_CLONE_INSTALLED_MANIFEST_MISSING|$manifestPath"
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
        throw "PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$installedVersion"
    }
    if (-not (Test-Path -LiteralPath $script:PcvCli -PathType Leaf)) {
        throw "PCV_P1_CLONE_CLI_NOT_FOUND|$script:PcvCli"
    }
    $summary.installed_cli_sha256 = (Get-FileHash -LiteralPath $script:PcvCli -Algorithm SHA256).Hash.ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($summary.iso_path_resolved) -or
        -not (Test-Path -LiteralPath $summary.iso_path_resolved -PathType Leaf)) {
        throw "PCV_P1_CLONE_ISO_NOT_FOUND|$($summary.iso_path_resolved)"
    }
}

function Assert-ServiceAvailable {
    if ($null -ne $RuntimeAdapter) {
        if ([string](Invoke-RuntimeOperation -Operation 'service-state') -ne 'Running') {
            throw 'PCV_P1_CLONE_SERVICE_LOST'
        }
        return
    }
    $service = Get-Service -Name 'PureCVisorDesktopNode' -ErrorAction SilentlyContinue
    if ($null -eq $service -or [string]$service.Status -ne 'Running') {
        throw 'PCV_P1_CLONE_SERVICE_LOST'
    }
}

function Assert-VmAbsent {
    param([Parameter(Mandatory)][string]$Name)

    if (@(Get-PcvVmByName -Name $Name -Purpose 'preflight').Count -ne 0) {
        throw "PCV_P1_CLONE_VM_ALREADY_EXISTS|$Name"
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

function Get-CliProblemCode {
    param(
        $Payload,
        [string]$Stderr = ''
    )
    foreach ($candidate in @(
        (Get-ObjectPropertyValue -InputObject (Get-ObjectPropertyValue -InputObject $Payload -Name 'error') -Name 'code'),
        (Get-ObjectPropertyValue -InputObject $Payload -Name 'code')
    )) {
        if ([string]$candidate -match '^PCV_[A-Z0-9_]+$') {
            return [string]$candidate
        }
    }
    if ([string]$Stderr -match '(?m)^code=(PCV_[A-Z0-9_]+)') {
        return $Matches[1]
    }
    if ([string]$Stderr -match '\b(PCV_[A-Z0-9_]+)\b') {
        return $Matches[1]
    }
    return $null
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
            throw "PCV_P1_CLONE_COMMAND_START_FAILED|$StepName"
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($CommandTimeoutSeconds * 1000)) {
            try { $process.Kill($true) } catch { }
            throw "PCV_P1_CLONE_COMMAND_TIMEOUT|$StepName"
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
        $cliErrorCode = Get-CliProblemCode -Payload $payload -Stderr $stderr
        if ([string]::IsNullOrWhiteSpace($cliErrorCode)) {
            $cliErrorCode = 'PCV_P1_CLONE_COMMAND_FAILED'
        }
        throw "$cliErrorCode|$StepName|exit=$exitCode"
    }
    return [pscustomobject]@{
        ExitCode = $exitCode
        Json = $payload
        Stderr = $stderr
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
    throw "PCV_P1_CLONE_JOB_TIMEOUT|$StepName|job=$JobId"
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
        throw "PCV_P1_CLONE_JOB_ID_MISSING|$StepName"
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
        throw 'PCV_P1_CLONE_SECRET_OBSERVED'
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
        $summary.queued_jobs[$StepName].status = if ($failureCode -eq 'PCV_P1_CLONE_JOB_TIMEOUT') { 'timed_out' } else { 'poll_error' }
        $summary.queued_jobs[$StepName].polling_status = if ($failureCode -eq 'PCV_P1_CLONE_JOB_TIMEOUT') { 'timeout' } else { 'error' }
        $summary.queued_jobs[$StepName].error_code = $failureCode
        Write-AtomicSummary
        throw $failureCode
    }
    $status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
    if ($status -notin @('succeeded', 'failed', 'canceled')) { $status = 'invalid-terminal-status' }
    $errorObject = Get-ObjectPropertyValue -InputObject $job -Name 'error'
    $errorCode = [string](Get-ObjectPropertyValue -InputObject $errorObject -Name 'code')
    if (-not [string]::IsNullOrEmpty($errorCode) -and $errorCode -notmatch '^PCV_[A-Z0-9_]+$') {
        $errorCode = 'PCV_P1_CLONE_REMOTE_ERROR_REDACTED'
    }
    $summary.queued_jobs[$StepName].status = $status
    $summary.queued_jobs[$StepName].polling_status = 'terminal'
    $summary.queued_jobs[$StepName].terminal = $true
    $summary.queued_jobs[$StepName].error_code = $errorCode
    if (-not $DeferTerminalSummaryWrite.IsPresent) { Write-AtomicSummary }
    if ($status -ne 'succeeded' -and -not $AllowFailure.IsPresent) {
        throw "PCV_P1_CLONE_JOB_FAILED|$StepName|job=$jobId|status=$status"
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
            throw "PCV_P1_CLONE_CLEANUP_ROOT_INVALID|observed-vm-outside-reserved-root"
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
    if ($Record.kind -eq 'source') { $summary.source_vm_id = $Record.id }
    else { $summary.target_vm_id = $Record.id }
    $Record.identity_status = 'authoritative'
    $Record.identity_blocker = $false
    Write-AtomicSummary
    return $Record
}

function Assert-SlicePassed {
    param([Parameter(Mandatory)][string]$Slice)

    if ([string]$summary.slice_verdicts[$Slice] -ne 'PASS') {
        throw "PCV_P1_CLONE_SLICE_FAILED|$Slice"
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

function Resolve-CreatedVm {
    param(
        [Parameter(Mandatory)]$Record,
        [Parameter(Mandatory)]$Job,
        [Parameter(Mandatory)][string]$Name
    )

    $createdVm = $null
    $jobResult = Get-ObjectPropertyValue -InputObject $Job -Name 'result'
    $jobVmIdText = [string](Get-ObjectPropertyValue -InputObject $Job -Name 'vm_id')
    if ([string]::IsNullOrWhiteSpace($jobVmIdText)) {
        $jobVmIdText = [string](Get-ObjectPropertyValue -InputObject $jobResult -Name 'vm_id')
    }
    $jobVmId = [Guid]::Empty
    if ([Guid]::TryParse($jobVmIdText, [ref]$jobVmId)) {
        $createdVm = Get-PcvVmById -Id $jobVmId -Record $Record
    }
    if ($null -eq $createdVm) {
        $createdRows = @(Get-PcvVmByName -Name $Name -Purpose 'authoritative-create')
        if ($createdRows.Count -eq 1) { $createdVm = $createdRows[0] }
        else {
            $Record.identity_status = 'orphan-blocker'
            $Record.identity_blocker = $true
            Write-AtomicSummary
            throw "PCV_P1_CLONE_STATE_MISMATCH|created-vm-cardinality=$($createdRows.Count)|name=$Name"
        }
    }
    return Set-VmAuthoritativeIdentity -Record $Record -Vm $createdVm
}

function Get-Int64PropertyValue {
    param(
        $InputObject,
        [Parameter(Mandatory)][string]$Name
    )

    $raw = Get-ObjectPropertyValue -InputObject $InputObject -Name $Name
    if ($null -eq $raw -or [string]::IsNullOrWhiteSpace([string]$raw)) {
        return $null
    }
    try { return [int64]$raw } catch { return $null }
}

function Test-ManagedMarker {
    param(
        $Vm,
        $ProductData
    )

    $managed = Get-ObjectPropertyValue -InputObject $ProductData -Name 'managed_by_purecvisor'
    if ($null -ne $managed) {
        return [bool]$managed
    }
    $notes = if ($null -eq $Vm -or $null -eq $Vm.PSObject.Properties['Notes']) { '' } else { [string]$Vm.Notes }
    return $notes -match 'managed-by=purecvisor-desktop-node'
}

function Invoke-SourceCreateSlice {
    param([Parameter(Mandatory)]$Record)

    $create = Start-PcvCliJob -StepName 'vm-create' -Arguments @(
        'vm', 'create', '--name', $SourceVm, '--iso', $summary.iso_path_resolved,
        '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '1', '--vm-root', $vmRootFull) -DeferTerminalSummaryWrite
    if ([string](Get-ObjectPropertyValue -InputObject $create -Name 'status') -ne 'succeeded') {
        throw 'PCV_P1_CLONE_STATE_MISMATCH|create'
    }
    $Record = Resolve-CreatedVm -Record $Record -Job $create -Name $SourceVm
    $id = [Guid]$Record.id
    $hypervOff = Wait-HyperVState -Id $id -Expected 'Off' -Phase 'after-create'
    $productOff = Get-ProductVmState -OperatorId $SourceVm -Phase 'after-create'
    $summary.hyperv_state_after_create = $hypervOff
    $summary.product_state_after_create = $productOff
    $summary.readbacks.source_create = [ordered]@{
        hyperv = $hypervOff
        product = $productOff
        started = $false
    }
    Write-AtomicSummary
    if ($hypervOff -ne 'Off' -or $productOff -ne 'off') {
        throw "PCV_P1_CLONE_STATE_MISMATCH|create|hyperv=$hypervOff|product=$productOff"
    }
}

function Invoke-PreviewMismatchSlice {
    $unconfirmed = Invoke-PcvCliJson -StepName 'vm-clone-unconfirmed' -AllowFailure -Arguments @(
        'vm', 'clone', $SourceVm, '--name', $TargetVm)
    $code = Get-CliProblemCode -Payload $unconfirmed.Json -Stderr $unconfirmed.Stderr
    $targetPresent = @(Get-PcvVmByName -Name $TargetVm -Purpose 'preview-mismatch').Count -ne 0
    $targetDisk = Join-Path $targetVmRootFull 'disk0.vhdx'
    $diskExists = Test-PcvPath -Path $targetDisk
    $summary.readbacks.preview_mismatch = [ordered]@{
        exit_code = $unconfirmed.ExitCode
        code = $code
        target_absent = (-not $targetPresent)
        target_disk_absent = (-not $diskExists)
    }
    Write-AtomicSummary
    if ($unconfirmed.ExitCode -eq 0 -or
        $code -ne 'PCV_CLI_CONFIRMATION_REQUIRED' -or
        $targetPresent -or
        $diskExists) {
        throw "PCV_P1_CLONE_STATE_MISMATCH|preview-mismatch|exit=$($unconfirmed.ExitCode)|code=$code"
    }
}

function Invoke-PreviewOkSlice {
    $preview = Invoke-PcvCliJson -StepName 'vm-clone-preview' -Arguments @(
        'vm', 'clone', $SourceVm, '--name', $TargetVm, '--dry-run')
    $data = Get-ObjectPropertyValue -InputObject $preview.Json -Name 'data'
    if ($null -eq $data) { $data = $preview.Json }
    $plannedBytes = Get-Int64PropertyValue -InputObject $data -Name 'planned_copy_bytes'
    if ($null -eq $plannedBytes) {
        $plannedBytes = Get-Int64PropertyValue -InputObject $preview.Json -Name 'planned_copy_bytes'
    }
    $targetPresent = @(Get-PcvVmByName -Name $TargetVm -Purpose 'preview-ok').Count -ne 0
    $targetDisk = Join-Path $targetVmRootFull 'disk0.vhdx'
    $diskExists = Test-PcvPath -Path $targetDisk
    $summary.readbacks.preview_ok = [ordered]@{
        planned_copy_bytes = $plannedBytes
        target_absent = (-not $targetPresent)
        target_disk_absent = (-not $diskExists)
    }
    Write-AtomicSummary
    if ($null -eq $plannedBytes -or $plannedBytes -le 0 -or $targetPresent -or $diskExists) {
        throw "PCV_P1_CLONE_STATE_MISMATCH|preview-ok|planned_copy_bytes=$plannedBytes"
    }
}

function Invoke-CloneOkSlice {
    param(
        [Parameter(Mandatory)]$SourceRecord,
        [Parameter(Mandatory)]$TargetRecord
    )

    $clone = Start-PcvCliJob -StepName 'vm-clone' -Arguments @(
        'vm', 'clone', $SourceVm, '--name', $TargetVm, '--yes')
    if ([string](Get-ObjectPropertyValue -InputObject $clone -Name 'status') -ne 'succeeded') {
        throw 'PCV_P1_CLONE_STATE_MISMATCH|clone'
    }
    $TargetRecord = Resolve-CreatedVm -Record $TargetRecord -Job $clone -Name $TargetVm
    $TargetRecord.root_owned_by_run = $true
    $targetState = Get-ProductVmState -OperatorId $TargetVm -Phase 'after-clone'
    $sourceState = Get-ProductVmState -OperatorId $SourceVm -Phase 'after-clone-source'
    $targetHyperV = Wait-HyperVState -Id ([Guid]$TargetRecord.id) -Expected 'Off' -Phase 'after-clone'
    $sourceHyperV = Wait-HyperVState -Id ([Guid]$SourceRecord.id) -Expected 'Off' -Phase 'after-clone-source'
    $summary.product_state_after_clone_target = $targetState
    $summary.product_state_after_clone_source = $sourceState
    $summary.hyperv_state_after_clone_target = $targetHyperV
    $summary.hyperv_state_after_clone_source = $sourceHyperV
    $targetVm = Get-PcvVmById -Id ([Guid]$TargetRecord.id) -Record $TargetRecord
    $targetGet = Invoke-PcvCliJson -StepName 'vm-get-target' -Arguments @('vm', 'get', $TargetVm)
    $targetData = Get-ObjectPropertyValue -InputObject $targetGet.Json -Name 'data'
    $managed = Test-ManagedMarker -Vm $targetVm -ProductData $targetData
    $targetDisk = Join-Path $targetVmRootFull 'disk0.vhdx'
    $sourceDisk = Join-Path $sourceVmRootFull 'disk0.vhdx'
    $targetDiskPresent = if ($null -eq $RuntimeAdapter) { Test-Path -LiteralPath $targetDisk -PathType Leaf } else { $true }
    $sourceDiskPresent = if ($null -eq $RuntimeAdapter) { Test-Path -LiteralPath $sourceDisk -PathType Leaf } else { $true }
    $summary.readbacks.clone_ok = [ordered]@{
        target_product = $targetState
        source_product = $sourceState
        target_hyperv = $targetHyperV
        source_hyperv = $sourceHyperV
        managed = $managed
        target_disk = $targetDisk
        source_disk_present = $sourceDiskPresent
        target_disk_present = $targetDiskPresent
    }
    Write-AtomicSummary
    if ($targetState -ne 'off' -or $sourceState -ne 'off' -or
        $targetHyperV -ne 'Off' -or $sourceHyperV -ne 'Off' -or
        -not $managed -or -not $targetDiskPresent -or -not $sourceDiskPresent) {
        throw "PCV_P1_CLONE_STATE_MISMATCH|clone|target=$targetState|source=$sourceState|managed=$managed"
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
        throw "PCV_P1_CLONE_CLEANUP_IDENTITY_DRIFT|phase=$Phase|missing-recorded-id"
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
        throw "PCV_P1_CLONE_CLEANUP_IDENTITY_DRIFT|phase=$Phase"
    }
    return $current
}

function Invoke-ExactCleanup {
    $summary.cleanup.attempted = $true
    $cleanupErrors = [System.Collections.Generic.List[string]]::new()
    $ordered = [System.Collections.Generic.List[object]]::new()
    foreach ($kind in @('target', 'source')) {
        foreach ($record in $script:VmRecords) {
            if ([string]$record.kind -eq $kind) {
                $ordered.Add($record) | Out-Null
            }
        }
    }
    foreach ($record in $ordered) {
        try {
            $record.root = Assert-ValidatedChildPath -Root $vmRootFull -Candidate $record.root
            $sameName = @(Get-PcvVmByName -Name $record.name -Purpose 'cleanup-observation')
            if ([string]::IsNullOrWhiteSpace([string]$record.id)) {
                if ($sameName.Count -eq 0) {
                    $record.removed = $true
                    $record.root_removed = -not (Test-PcvPath -Path $record.root)
                    continue
                }
                $record.identity_status = 'orphan-blocker'
                $record.identity_blocker = $true
                $record.error = 'PCV_P1_CLONE_CLEANUP_ID_MISMATCH'
                $cleanupErrors.Add('PCV_P1_CLONE_CLEANUP_ID_MISMATCH') | Out-Null
                continue
            }
            $recordedId = [Guid]$record.id
            $different = @($sameName | Where-Object { [Guid]$_.Id -ne $recordedId })
            $collisionCode = $null
            if ($different.Count -gt 0) {
                $record.same_name_different_id_blocked = $true
                $summary.cleanup.same_name_different_id_blocked = $true
                $collisionCode = 'PCV_P1_CLONE_CLEANUP_ID_MISMATCH'
            }

            $current = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-product-delete' -AllowAbsent
            if ($null -ne $current) {
                $record.product_delete_attempted = $true
                $stepName = if ($record.kind -eq 'target') { 'vm-delete-target' } else { 'vm-delete-source' }
                try {
                    $delete = Start-PcvCliJob -StepName $stepName -Arguments @(
                        'vm', 'delete', $record.name, '--yes') -AllowFailure
                    if ([string](Get-ObjectPropertyValue -InputObject $delete -Name 'status') -ne 'succeeded') {
                        throw 'product-delete-failed'
                    }
                }
                catch { }
            }
            $record.removed = $null -eq (Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'after-delete' -AllowAbsent)
            if (-not $record.removed) {
                throw "PCV_P1_CLONE_CLEANUP_ID_MISMATCH|remaining=$recordedId"
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
                    throw "PCV_P1_CLONE_CLEANUP_IDENTITY_DRIFT|phase=before-root-removal|recorded-id-present"
                }
                Remove-PcvDirectory -Path $record.root
            }
            $record.root_removed = -not (Test-PcvPath -Path $record.root)
            if (-not $record.root_removed) {
                throw "PCV_P1_CLONE_CLEANUP_ROOT_INVALID|remaining=$($record.root)"
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
    Assert-VmAbsent -Name $SourceVm
    Assert-VmAbsent -Name $TargetVm
    Assert-PcvPathAbsent -Path $sourceVmRootFull -Kind 'source'
    Assert-PcvPathAbsent -Path $targetVmRootFull -Kind 'target'
    Write-AtomicSummary

    $sourceRecord = New-VmOwnershipRecord -Kind 'source' -Name $SourceVm -ExpectedRoot $sourceVmRootFull
    $targetRecord = New-VmOwnershipRecord -Kind 'target' -Name $TargetVm -ExpectedRoot $targetVmRootFull
    $summary.host_mutation_performed = $true
    $summary.actual_execution = 'installed-cli-and-hyperv'
    if (-not (Test-PcvPath -Path $vmRootFull)) { New-PcvDirectory -Path $vmRootFull }
    New-PcvDirectory -Path $sourceRecord.root
    $sourceRecord.root_owned_by_run = $true
    Write-AtomicSummary
    Invoke-TrackedSlice -Slice 'source_create' -Action { Invoke-SourceCreateSlice -Record $sourceRecord }
    Assert-SlicePassed -Slice 'source_create'
    Invoke-TrackedSlice -Slice 'preview_mismatch' -Action { Invoke-PreviewMismatchSlice }
    Assert-SlicePassed -Slice 'preview_mismatch'
    Invoke-TrackedSlice -Slice 'preview_ok' -Action { Invoke-PreviewOkSlice }
    Assert-SlicePassed -Slice 'preview_ok'
    Invoke-TrackedSlice -Slice 'clone_ok' -Action { Invoke-CloneOkSlice -SourceRecord $sourceRecord -TargetRecord $targetRecord }
    Assert-SlicePassed -Slice 'clone_ok'
}
catch {
    $runError = Get-SafeFailureCode -Message $_.Exception.Message
    $summary.error = $runError
}
finally {
    $cleanupOk = $false
    try {
        Invoke-TrackedSlice -Slice 'cleanup' -Action {
            if (-not (Invoke-ExactCleanup)) {
                throw "PCV_P1_CLONE_CLEANUP_FAILED|$($summary.cleanup.error)"
            }
        }
        Assert-SlicePassed -Slice 'cleanup'
        $cleanupOk = $true
    }
    catch {
        $summary.cleanup.attempted = $true
        if ([string]$summary.cleanup.verdict -ne 'FAIL') {
            $summary.cleanup.verdict = 'FAIL'
        }
        $cleanupCode = Get-SafeFailureCode -Message $_.Exception.Message
        if ([string]::IsNullOrWhiteSpace([string]$summary.cleanup.error)) {
            $summary.cleanup.error = $cleanupCode
        }
        $runError = if ($null -eq $runError) { $cleanupCode } else { "$runError; $cleanupCode" }
    }
    try {
        Assert-ServiceAvailable
    }
    catch {
        $runError = if ($null -eq $runError) { 'PCV_P1_CLONE_SERVICE_LOST' } else { "$runError; PCV_P1_CLONE_SERVICE_LOST" }
    }
    $applicableSlices = @('source_create', 'preview_mismatch', 'preview_ok', 'clone_ok', 'cleanup')
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
    throw "PCV_P1_CLONE_FAILED|$($summary.error)"
}
