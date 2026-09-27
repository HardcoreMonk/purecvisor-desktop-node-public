#requires -Version 7.0

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [Parameter(Mandatory)]
    [ValidateSet('device-add', 'checkpoint-schedule', 'export-import')]
    [string]$Family,

    [string]$ArtifactRoot = '',
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$IsoPath = '',
    [string]$VmRoot = '',
    [string]$VmName = '',
    [string]$ImportVmName = '',
    [string]$SwitchName = 'Default Switch',

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

    if ($Name -notmatch '^pcv-p2-offvm-[A-Za-z0-9][A-Za-z0-9._-]{5,60}$' -or
        $Name -notlike "pcv-p2-offvm-$VersionTag-*") {
        throw "PCV_P2_OFFVM_VM_NAME_INVALID|$Name"
    }
}

function Assert-DedicatedVmRoot {
    param([Parameter(Mandatory)][string]$Path)

    $full = Get-AbsolutePath -Path $Path
    $volumeRoot = [System.IO.Path]::GetPathRoot($full)
    $relative = [System.IO.Path]::GetRelativePath($volumeRoot, $full)
    $segments = @($relative -split '[\\/]' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($full -eq $volumeRoot -or $segments.Count -lt 2) {
        throw "PCV_P2_OFFVM_CLEANUP_ROOT_INVALID|$full"
    }
    return $full.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
}

function Get-PathComparison {
    if ($IsWindows) { return [System.StringComparison]::OrdinalIgnoreCase }
    return [System.StringComparison]::Ordinal
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
    $comparison = Get-PathComparison
    if ($candidateFull.Equals($rootFull, $comparison) -or
        -not $candidateFull.StartsWith($rootFull + [System.IO.Path]::DirectorySeparatorChar, $comparison)) {
        throw "PCV_P2_OFFVM_CLEANUP_ROOT_INVALID|root=$rootFull|candidate=$candidateFull"
    }
    return $candidateFull
}

$versionTag = (($Version.Split('-')[0]) -replace '[^0-9A-Za-z]', '').ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($versionTag)) {
    throw 'PCV_P2_OFFVM_VERSION_INVALID'
}
if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path (Get-Location).Path "artifacts/service-plan-p2-offvm-$Family-actual-vm-$versionTag"
}
if ([string]::IsNullOrWhiteSpace($VmRoot)) {
    $VmRoot = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-service-plan-p2-offvm/$versionTag"
}

$artifactRootFull = Get-AbsolutePath -Path $ArtifactRoot
$vmRootFull = Assert-DedicatedVmRoot -Path $VmRoot
$campaignKey = Get-ShortHash -Value "$Version|$Family|$artifactRootFull"
$familyTag = switch ($Family) {
    'device-add' { 'dev' }
    'checkpoint-schedule' { 'sched' }
    default { 'exp' }
}
if ([string]::IsNullOrWhiteSpace($VmName)) {
    $VmName = "pcv-p2-offvm-$versionTag-$campaignKey-$familyTag"
}
Assert-VmName -Name $VmName -VersionTag $versionTag
$vmOwnRootFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull $VmName)
if ([string]::IsNullOrWhiteSpace($ImportVmName)) {
    $ImportVmName = "pcv-p2-offvm-$versionTag-$campaignKey-imp"
}
Assert-VmName -Name $ImportVmName -VersionTag $versionTag
if ($ImportVmName -eq $VmName) {
    throw 'PCV_P2_OFFVM_VM_NAME_INVALID|source-and-import-must-differ'
}
$exportRootFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull 'exports')
$exportDirFull = Assert-ValidatedChildPath -Root $exportRootFull -Candidate (Join-Path $exportRootFull $VmName)
$outsideDirFull = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull "outside-$VmName")

$ScheduleIntervalMinutes = 60
$ScheduleRetentionMax = 2
$InvalidIntervalMinutes = 30

$familySlices = [ordered]@{
    'device-add' = @('source_create', 'nic_confirm_required', 'nic_add', 'nic_limit', 'dvd_guard', 'cleanup')
    'checkpoint-schedule' = @('source_create', 'schedule_preview', 'schedule_interval_invalid', 'schedule_set', 'schedule_clear', 'cleanup')
    'export-import' = @('source_create', 'export_confirm_required', 'export_path_not_allowed', 'export_preview', 'export', 'import_preview', 'import', 'cleanup')
}
$familyNonclaims = [ordered]@{
    'device-add' = @('dvd-positive-add-not-reachable-product-create-attaches-dvd', 'nic-guest-link-not-observed')
    'checkpoint-schedule' = @('due-tick-not-observed-min-interval-60-minutes', 'retention-delete-not-observed')
    'export-import' = @('imported-vm-guest-boot-not-observed', 'ovf-and-tpm-rejection-not-exercised')
}
$plannedSlices = @($familySlices[$Family])

New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$summaryTempPath = Join-Path $artifactRootFull 'summary.json.tmp'
$startedAt = (Get-Date).ToUniversalTime()
$script:Steps = [System.Collections.Generic.List[object]]::new()
$script:VmRecords = [System.Collections.Generic.List[object]]::new()
$script:VmRecord = $null
$script:ImportRecord = $null
$script:ScheduleEnabled = $false
$script:PcvCli = Join-Path (Get-AbsolutePath -Path $ProductRoot) 'pcvcli.exe'

$sliceVerdicts = [ordered]@{}
foreach ($slice in $plannedSlices) { $sliceVerdicts[$slice] = 'NOT_RUN' }
$summary = [ordered]@{
    schema_version = 'pcv-service-plan-p2-offvm-actual-vm-summary/v1'
    scope = 'service-plan-p2-offvm-actual-vm'
    family = $Family
    version = $Version
    ok = $false
    overall_verdict = 'NOT_RUN'
    actual_execution = 'not-started'
    artifact_root_resolved = $artifactRootFull
    vm_root_resolved = $vmRootFull
    product_root_resolved = (Get-AbsolutePath -Path $ProductRoot)
    iso_path_resolved = if ([string]::IsNullOrWhiteSpace($IsoPath)) { $null } else { Get-AbsolutePath -Path $IsoPath }
    switch_name = if ($Family -eq 'device-add') { $SwitchName } else { $null }
    installed_manifest_version = $null
    installed_cli_sha256 = $null
    vm_name = $VmName
    vm_id = $null
    import_vm_name = if ($Family -eq 'export-import') { $ImportVmName } else { $null }
    import_vm_id = $null
    export_directory = if ($Family -eq 'export-import') { $exportDirFull } else { $null }
    slice_verdicts = $sliceVerdicts
    queued_jobs = [ordered]@{}
    readbacks = [ordered]@{}
    cleanup = [ordered]@{
        attempted = $false
        verdict = 'NOT_RUN'
        native_fallback_used = $false
        same_name_different_id_blocked = $false
        schedule_cleared = $null
        records = @()
        error = $null
    }
    plan = @($plannedSlices | ForEach-Object { [ordered]@{ slice = $_; mutates_host = ($_ -ne 'cleanup') } })
    nonclaims = @($familyNonclaims[$Family])
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

    if (Test-SecretMaterial -Text $Message) { return 'PCV_P2_OFFVM_SECRET_OBSERVED' }
    $match = [regex]::Match([string]$Message, '\bPCV_[A-Z0-9_]+\b')
    if ($match.Success) { return $match.Value }
    return 'PCV_P2_OFFVM_INTERNAL_FAILURE'
}

function Set-SecretObserved {
    $summary.secret_observed = $true
    $summary.ok = $false
    $summary.overall_verdict = 'FAIL'
    $summary.error = 'PCV_P2_OFFVM_SECRET_OBSERVED'
}

function Write-AtomicSummary {
    try {
        $summary.steps = $script:Steps.ToArray()
        $json = $summary | ConvertTo-Json -Depth 32
        if (Test-SecretMaterial -Text $json) {
            $summary.secret_observed = $true
            throw 'PCV_P2_OFFVM_SECRET_OBSERVED_IN_SUMMARY'
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
        throw 'PCV_P2_OFFVM_SUMMARY_WRITE_FAILED'
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
        throw "PCV_P2_OFFVM_RUNTIME_ADAPTER_NOT_CONFIGURED|$Operation"
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
        throw "PCV_P2_OFFVM_VM_ROOT_ALREADY_EXISTS|kind=$Kind|path=$Path"
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
            throw "PCV_P2_OFFVM_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$($installed.version)"
        }
        if (-not [bool]$installed.iso_exists) { throw 'PCV_P2_OFFVM_ISO_NOT_FOUND' }
        return
    }
    $manifestPath = Join-Path $summary.product_root_resolved 'product-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "PCV_P2_OFFVM_INSTALLED_MANIFEST_MISSING|$manifestPath"
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
        throw "PCV_P2_OFFVM_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$installedVersion"
    }
    if (-not (Test-Path -LiteralPath $script:PcvCli -PathType Leaf)) {
        throw "PCV_P2_OFFVM_CLI_NOT_FOUND|$script:PcvCli"
    }
    $summary.installed_cli_sha256 = (Get-FileHash -LiteralPath $script:PcvCli -Algorithm SHA256).Hash.ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($summary.iso_path_resolved) -or
        -not (Test-Path -LiteralPath $summary.iso_path_resolved -PathType Leaf)) {
        throw "PCV_P2_OFFVM_ISO_NOT_FOUND|$($summary.iso_path_resolved)"
    }
}

function Assert-ServiceAvailable {
    if ($null -ne $RuntimeAdapter) {
        if ([string](Invoke-RuntimeOperation -Operation 'service-state') -ne 'Running') {
            throw 'PCV_P2_OFFVM_SERVICE_LOST'
        }
        return
    }
    $service = Get-Service -Name 'PureCVisorDesktopNode' -ErrorAction SilentlyContinue
    if ($null -eq $service -or [string]$service.Status -ne 'Running') {
        throw 'PCV_P2_OFFVM_SERVICE_LOST'
    }
}

function Assert-SwitchPresent {
    $present = if ($null -ne $RuntimeAdapter) {
        [bool](Invoke-RuntimeOperation -Operation 'switch-exists' -Input @{ name = $SwitchName })
    }
    else {
        $null -ne (Get-VMSwitch -Name $SwitchName -ErrorAction SilentlyContinue)
    }
    if (-not $present) {
        throw "PCV_P2_OFFVM_SWITCH_NOT_FOUND|$SwitchName"
    }
}

function Assert-VmAbsent {
    param([Parameter(Mandatory)][string]$Name)

    if (@(Get-PcvVmByName -Name $Name -Purpose 'preflight').Count -ne 0) {
        throw "PCV_P2_OFFVM_VM_ALREADY_EXISTS|$Name"
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

function Get-ItemCount {
    param($Value)

    if ($null -eq $Value) { return 0 }
    return @($Value).Count
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
            throw "PCV_P2_OFFVM_COMMAND_START_FAILED|$StepName"
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($CommandTimeoutSeconds * 1000)) {
            try { $process.Kill($true) } catch { }
            throw "PCV_P2_OFFVM_COMMAND_TIMEOUT|$StepName"
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
            $cliErrorCode = 'PCV_P2_OFFVM_COMMAND_FAILED'
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

function Get-CliJobId {
    param($Created)

    $data = Get-ObjectPropertyValue -InputObject $Created.Json -Name 'data'
    $jobId = [string](Get-ObjectPropertyValue -InputObject $data -Name 'job_id')
    if ($jobId -match '^[A-Za-z0-9][A-Za-z0-9._-]{2,127}$') { return $jobId }
    return $null
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
    throw "PCV_P2_OFFVM_JOB_TIMEOUT|$StepName|job=$JobId"
}

function Complete-PcvCliJob {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)]$Created,
        [switch]$AllowFailure,
        [switch]$DeferTerminalSummaryWrite
    )

    $data = Get-ObjectPropertyValue -InputObject $Created.Json -Name 'data'
    $jobId = Get-CliJobId -Created $Created
    $initialStatus = [string](Get-ObjectPropertyValue -InputObject $data -Name 'status')
    $secretObserved = [bool]$Created.SecretObserved
    if ($null -eq $jobId) {
        if ($secretObserved) { Set-SecretObserved }
        throw "PCV_P2_OFFVM_JOB_ID_MISSING|$StepName"
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
        throw 'PCV_P2_OFFVM_SECRET_OBSERVED'
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
        $summary.queued_jobs[$StepName].status = if ($failureCode -eq 'PCV_P2_OFFVM_JOB_TIMEOUT') { 'timed_out' } else { 'poll_error' }
        $summary.queued_jobs[$StepName].polling_status = if ($failureCode -eq 'PCV_P2_OFFVM_JOB_TIMEOUT') { 'timeout' } else { 'error' }
        $summary.queued_jobs[$StepName].error_code = $failureCode
        Write-AtomicSummary
        throw $failureCode
    }
    $status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
    if ($status -notin @('succeeded', 'failed', 'canceled')) { $status = 'invalid-terminal-status' }
    $errorObject = Get-ObjectPropertyValue -InputObject $job -Name 'error'
    $errorCode = [string](Get-ObjectPropertyValue -InputObject $errorObject -Name 'code')
    if (-not [string]::IsNullOrEmpty($errorCode) -and $errorCode -notmatch '^PCV_[A-Z0-9_]+$') {
        $errorCode = 'PCV_P2_OFFVM_REMOTE_ERROR_REDACTED'
    }
    $summary.queued_jobs[$StepName].status = $status
    $summary.queued_jobs[$StepName].polling_status = 'terminal'
    $summary.queued_jobs[$StepName].terminal = $true
    $summary.queued_jobs[$StepName].error_code = $errorCode
    if (-not $DeferTerminalSummaryWrite.IsPresent) { Write-AtomicSummary }
    if ($status -ne 'succeeded' -and -not $AllowFailure.IsPresent) {
        throw "PCV_P2_OFFVM_JOB_FAILED|$StepName|job=$jobId|status=$status"
    }
    return $job
}

function Start-PcvCliJob {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure,
        [switch]$DeferTerminalSummaryWrite
    )

    $created = Invoke-PcvCliJson -StepName $StepName -Arguments $Arguments -AllowFailure:$AllowFailure
    return Complete-PcvCliJob -StepName $StepName -Created $created -AllowFailure:$AllowFailure -DeferTerminalSummaryWrite:$DeferTerminalSummaryWrite
}

function Wait-HyperVState {
    param(
        [Parameter(Mandatory)][Guid]$Id,
        [Parameter(Mandatory)][string]$Expected,
        [Parameter(Mandatory)][string]$Phase,
        [int]$TimeoutSeconds = 60,
        $Record = $script:VmRecord
    )

    if ($null -ne $RuntimeAdapter) {
        return [string](Invoke-RuntimeOperation -Operation 'wait-hyperv-state' -Input @{
            id = $Id.ToString('D')
            expected = $Expected
            phase = $Phase
            timeout_seconds = $TimeoutSeconds
        })
    }
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $vm = Get-PcvVmById -Id $Id -Record $Record
        if ($null -ne $vm -and [string]$vm.State -eq $Expected) {
            return [string]$vm.State
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    $final = Get-PcvVmById -Id $Id -Record $Record
    if ($null -eq $final) { return $null }
    return [string]$final.State
}

function Get-HyperVDeviceReadback {
    param([Parameter(Mandatory)][Guid]$Id)

    if ($null -ne $RuntimeAdapter) {
        return Invoke-RuntimeOperation -Operation 'vm-devices' -Input @{
            id = $Id.ToString('D')
            name = $VmName
        }
    }
    # Hyper-V PowerShell cmdlets cache VM devices per process and miss devices the service adds,
    # so device readback queries the Hyper-V WMI provider directly.
    $namespace = 'root\virtualization\v2'
    $vm = Get-CimInstance -Namespace $namespace -ClassName Msvm_ComputerSystem -Filter "Name='$($Id.ToString('D'))'"
    if ($null -eq $vm) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|hyperv-vm-missing|$($Id.ToString('D'))"
    }
    $realized = @(Get-CimAssociatedInstance -InputObject $vm -ResultClassName Msvm_VirtualSystemSettingData |
        Where-Object { [string](Get-CimPropertyValue -Instance $_ -Name 'VirtualSystemType') -eq 'Microsoft:Hyper-V:System:Realized' })
    if ($realized.Count -ne 1) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|hyperv-realized-settings=$($realized.Count)"
    }
    $parts = @(Get-CimAssociatedInstance -InputObject $realized[0] -Association Msvm_VirtualSystemSettingDataComponent)
    $ports = @($parts | Where-Object { $_.CimClass.CimClassName -eq 'Msvm_SyntheticEthernetPortSettingData' })
    $connections = @($parts | Where-Object { $_.CimClass.CimClassName -eq 'Msvm_EthernetPortAllocationSettingData' })
    $dvdDrives = @($parts | Where-Object {
        [string](Get-CimPropertyValue -Instance $_ -Name 'ResourceSubType') -eq 'Microsoft:Hyper-V:Synthetic DVD Drive'
    })
    $snapshots = @(Get-CimAssociatedInstance -InputObject $vm -Association Msvm_SnapshotOfVirtualSystem)
    return [pscustomobject][ordered]@{
        readback_source = 'hyperv-wmi-root-virtualization-v2'
        nic_count = $ports.Count
        nic_switches = @($connections | ForEach-Object { [string](Get-CimPropertyValue -Instance $_ -Name 'LastKnownSwitchName') })
        dvd_count = $dvdDrives.Count
        checkpoint_count = $snapshots.Count
    }
}

function Get-CimPropertyValue {
    param(
        [Parameter(Mandatory)]$Instance,
        [Parameter(Mandatory)][string]$Name
    )

    $property = $Instance.CimInstanceProperties[$Name]
    if ($null -eq $property) { return $null }
    return $property.Value
}

function Get-ProductVmData {
    param([Parameter(Mandatory)][string]$Phase)

    $result = Invoke-PcvCliJson -StepName "vm-get-$Phase" -Arguments @('vm', 'get', $VmName)
    return Get-ObjectPropertyValue -InputObject $result.Json -Name 'data'
}

function Get-ProductState {
    param($Data)

    $state = Get-ObjectPropertyValue -InputObject $Data -Name 'state'
    if ($null -eq $state) {
        $state = Get-ObjectPropertyValue -InputObject $Data -Name 'power_state'
    }
    return ([string]$state).ToLowerInvariant()
}

function Test-PcvProductOff {
    param([AllowNull()][string]$State)

    return ([string]$State).ToLowerInvariant() -in @('off', 'stopped')
}

function New-VmOwnershipRecord {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$ExpectedRoot,
        [ValidateSet('vm', 'import')][string]$Kind = 'vm'
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
        $comparison = Get-PathComparison
        $withinReservedRoot = $observedPath.Equals($reservedRoot, $comparison) -or
            $observedPath.StartsWith($reservedRoot + [System.IO.Path]::DirectorySeparatorChar, $comparison)
        if (-not $withinReservedRoot) {
            throw 'PCV_P2_OFFVM_CLEANUP_ROOT_INVALID|observed-vm-outside-reserved-root'
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
    if ($Record.kind -eq 'import') { $summary.import_vm_id = $Record.id }
    else { $summary.vm_id = $Record.id }
    $Record.identity_status = 'authoritative'
    $Record.identity_blocker = $false
    Write-AtomicSummary
    return $Record
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
            throw "PCV_P2_OFFVM_STATE_MISMATCH|created-vm-cardinality=$($createdRows.Count)|name=$Name"
        }
    }
    return Set-VmAuthoritativeIdentity -Record $Record -Vm $createdVm
}

function Assert-SlicePassed {
    param([Parameter(Mandatory)][string]$Slice)

    if ([string]$summary.slice_verdicts[$Slice] -ne 'PASS') {
        throw "PCV_P2_OFFVM_SLICE_FAILED|$Slice"
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

function Invoke-SourceCreateSlice {
    $create = Start-PcvCliJob -StepName 'vm-create' -Arguments @(
        'vm', 'create', '--name', $VmName, '--iso', $summary.iso_path_resolved,
        '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '8', '--vm-root', $vmRootFull) -DeferTerminalSummaryWrite
    if ([string](Get-ObjectPropertyValue -InputObject $create -Name 'status') -ne 'succeeded') {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|create'
    }
    $record = Resolve-CreatedVm -Record $script:VmRecord -Job $create -Name $VmName
    $id = [Guid]$record.id
    $hypervOff = Wait-HyperVState -Id $id -Expected 'Off' -Phase 'after-create'
    $data = Get-ProductVmData -Phase 'after-create'
    $productState = Get-ProductState -Data $data
    $managed = [bool](Get-ObjectPropertyValue -InputObject $data -Name 'managed_by_purecvisor')
    $generation = [int](Get-ObjectPropertyValue -InputObject $data -Name 'generation')
    $devices = Get-HyperVDeviceReadback -Id $id
    $summary.readbacks.source_create = [ordered]@{
        hyperv = $hypervOff
        product = $productState
        managed = $managed
        generation = $generation
        nic_count = [int]$devices.nic_count
        dvd_count = [int]$devices.dvd_count
        checkpoint_count = [int]$devices.checkpoint_count
        started = $false
    }
    Write-AtomicSummary
    $familyPreState = if ($Family -eq 'device-add') {
        [int]$devices.nic_count -eq 1 -and [int]$devices.dvd_count -eq 1
    }
    else {
        [int]$devices.checkpoint_count -eq 0
    }
    if ($hypervOff -ne 'Off' -or -not (Test-PcvProductOff $productState) -or -not $managed -or
        $generation -ne 2 -or -not $familyPreState) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|create|hyperv=$hypervOff|product=$productState|managed=$managed"
    }
}

function Invoke-NicConfirmRequiredSlice {
    $unconfirmed = Invoke-PcvCliJson -StepName 'vm-device-add-nic-unconfirmed' -AllowFailure -Arguments @(
        'vm', 'device', 'add', $VmName, '--kind', 'nic', '--switch', $SwitchName)
    $code = Get-CliProblemCode -Payload $unconfirmed.Json -Stderr $unconfirmed.Stderr
    $devices = Get-HyperVDeviceReadback -Id ([Guid]$script:VmRecord.id)
    $summary.readbacks.nic_confirm_required = [ordered]@{
        exit_code = $unconfirmed.ExitCode
        code = $code
        job_id = Get-CliJobId -Created $unconfirmed
        nic_count = [int]$devices.nic_count
    }
    Write-AtomicSummary
    if ($unconfirmed.ExitCode -eq 0 -or $code -ne 'PCV_CLI_CONFIRMATION_REQUIRED' -or [int]$devices.nic_count -ne 1) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|nic-confirm-required|exit=$($unconfirmed.ExitCode)|code=$code"
    }
}

function Invoke-NicAddSlice {
    $add = Start-PcvCliJob -StepName 'vm-device-add-nic' -Arguments @(
        'vm', 'device', 'add', $VmName, '--kind', 'nic', '--switch', $SwitchName, '--yes')
    if ([string](Get-ObjectPropertyValue -InputObject $add -Name 'status') -ne 'succeeded') {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|nic-add'
    }
    $id = [Guid]$script:VmRecord.id
    $hyperv = Wait-HyperVState -Id $id -Expected 'Off' -Phase 'after-nic-add'
    $devices = Get-HyperVDeviceReadback -Id $id
    $data = Get-ProductVmData -Phase 'after-nic-add'
    $productState = Get-ProductState -Data $data
    $productNics = Get-ItemCount -Value (Get-ObjectPropertyValue -InputObject $data -Name 'network')
    $onSwitch = @($devices.nic_switches | Where-Object { [string]$_ -eq $SwitchName }).Count
    $summary.readbacks.nic_add = [ordered]@{
        hyperv = $hyperv
        product = $productState
        hyperv_nic_count = [int]$devices.nic_count
        product_nic_count = $productNics
        nics_on_switch = $onSwitch
    }
    Write-AtomicSummary
    if ($hyperv -ne 'Off' -or -not (Test-PcvProductOff $productState) -or
        [int]$devices.nic_count -ne 2 -or $productNics -ne 2 -or $onSwitch -ne 2) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|nic-add|hyperv_nics=$($devices.nic_count)|product_nics=$productNics"
    }
}

function Invoke-NicLimitSlice {
    $limit = Invoke-PcvCliJson -StepName 'vm-device-add-nic-limit' -AllowFailure -Arguments @(
        'vm', 'device', 'add', $VmName, '--kind', 'nic', '--switch', $SwitchName, '--yes')
    $code = Get-CliProblemCode -Payload $limit.Json -Stderr $limit.Stderr
    $jobId = Get-CliJobId -Created $limit
    if ($limit.ExitCode -eq 0 -and $null -ne $jobId) {
        $null = Complete-PcvCliJob -StepName 'vm-device-add-nic-limit' -Created $limit -AllowFailure
    }
    $devices = Get-HyperVDeviceReadback -Id ([Guid]$script:VmRecord.id)
    $summary.readbacks.nic_limit = [ordered]@{
        exit_code = $limit.ExitCode
        code = $code
        job_id = $jobId
        nic_count = [int]$devices.nic_count
    }
    Write-AtomicSummary
    if ($limit.ExitCode -eq 0 -or $null -ne $jobId -or $code -ne 'PCV_VM_DEVICE_LIMIT' -or [int]$devices.nic_count -ne 2) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|nic-limit|exit=$($limit.ExitCode)|code=$code"
    }
}

function Invoke-DvdGuardSlice {
    $dvd = Invoke-PcvCliJson -StepName 'vm-device-add-dvd' -AllowFailure -Arguments @(
        'vm', 'device', 'add', $VmName, '--kind', 'dvd', '--yes')
    $code = Get-CliProblemCode -Payload $dvd.Json -Stderr $dvd.Stderr
    $jobId = Get-CliJobId -Created $dvd
    $rejectedBy = 'route'
    $jobStatus = $null
    if ($dvd.ExitCode -eq 0 -and $null -ne $jobId) {
        $rejectedBy = 'job'
        $job = Complete-PcvCliJob -StepName 'vm-device-add-dvd' -Created $dvd -AllowFailure
        $jobStatus = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
        $code = [string]$summary.queued_jobs['vm-device-add-dvd'].error_code
    }
    $devices = Get-HyperVDeviceReadback -Id ([Guid]$script:VmRecord.id)
    $summary.readbacks.dvd_guard = [ordered]@{
        exit_code = $dvd.ExitCode
        rejected_by = $rejectedBy
        job_id = $jobId
        job_status = $jobStatus
        code = $code
        dvd_count = [int]$devices.dvd_count
    }
    Write-AtomicSummary
    $rejected = ($rejectedBy -eq 'route' -and $dvd.ExitCode -ne 0) -or
        ($rejectedBy -eq 'job' -and $jobStatus -eq 'failed')
    if (-not $rejected -or $code -ne 'PCV_VM_DEVICE_ALREADY_PRESENT' -or [int]$devices.dvd_count -ne 1) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|dvd-guard|rejected_by=$rejectedBy|code=$code|dvd=$($devices.dvd_count)"
    }
}

function Get-ScheduleReadback {
    param([Parameter(Mandatory)][string]$Phase)

    $data = Get-ProductVmData -Phase $Phase
    $schedule = Get-ObjectPropertyValue -InputObject $data -Name 'checkpoint_schedule'
    $devices = Get-HyperVDeviceReadback -Id ([Guid]$script:VmRecord.id)
    return [ordered]@{
        product = Get-ProductState -Data $data
        enabled = [bool](Get-ObjectPropertyValue -InputObject $schedule -Name 'enabled')
        status = [string](Get-ObjectPropertyValue -InputObject $schedule -Name 'status')
        interval_minutes = Get-ObjectPropertyValue -InputObject $schedule -Name 'interval_minutes'
        retention_max = Get-ObjectPropertyValue -InputObject $schedule -Name 'retention_max'
        hyperv_checkpoint_count = [int]$devices.checkpoint_count
    }
}

function Invoke-SchedulePreviewSlice {
    $preview = Invoke-PcvCliJson -StepName 'vm-checkpoint-schedule-preview' -Arguments @(
        'vm', 'checkpoint', 'schedule', 'preview', $VmName,
        '--interval-minutes', "$ScheduleIntervalMinutes", '--retention-max', "$ScheduleRetentionMax")
    $data = Get-ObjectPropertyValue -InputObject $preview.Json -Name 'data'
    $jobId = Get-CliJobId -Created $preview
    $after = Get-ScheduleReadback -Phase 'after-schedule-preview'
    $summary.readbacks.schedule_preview = [ordered]@{
        dry_run = Get-ObjectPropertyValue -InputObject $data -Name 'dry_run'
        interval_minutes = Get-ObjectPropertyValue -InputObject $data -Name 'interval_minutes'
        retention_max = Get-ObjectPropertyValue -InputObject $data -Name 'retention_max'
        job_id = $jobId
        after = $after
    }
    Write-AtomicSummary
    if ($null -ne $jobId -or
        [int](Get-ObjectPropertyValue -InputObject $data -Name 'interval_minutes') -ne $ScheduleIntervalMinutes -or
        [int](Get-ObjectPropertyValue -InputObject $data -Name 'retention_max') -ne $ScheduleRetentionMax -or
        $after.enabled -or $after.hyperv_checkpoint_count -ne 0) {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|schedule-preview'
    }
}

function Invoke-ScheduleIntervalInvalidSlice {
    $invalid = Invoke-PcvCliJson -StepName 'vm-checkpoint-schedule-set-invalid' -AllowFailure -Arguments @(
        'vm', 'checkpoint', 'schedule', 'set', $VmName,
        '--interval-minutes', "$InvalidIntervalMinutes", '--retention-max', "$ScheduleRetentionMax", '--yes')
    $code = Get-CliProblemCode -Payload $invalid.Json -Stderr $invalid.Stderr
    $jobId = Get-CliJobId -Created $invalid
    if ($invalid.ExitCode -eq 0 -and $null -ne $jobId) {
        $null = Complete-PcvCliJob -StepName 'vm-checkpoint-schedule-set-invalid' -Created $invalid -AllowFailure
        $script:ScheduleEnabled = $true
    }
    $after = Get-ScheduleReadback -Phase 'after-schedule-invalid'
    $summary.readbacks.schedule_interval_invalid = [ordered]@{
        exit_code = $invalid.ExitCode
        code = $code
        job_id = $jobId
        after = $after
    }
    Write-AtomicSummary
    if ($invalid.ExitCode -eq 0 -or $null -ne $jobId -or
        $code -ne 'PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID' -or $after.enabled) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|schedule-interval-invalid|exit=$($invalid.ExitCode)|code=$code"
    }
}

function Invoke-ScheduleSetSlice {
    $script:ScheduleEnabled = $true
    $set = Start-PcvCliJob -StepName 'vm-checkpoint-schedule-set' -Arguments @(
        'vm', 'checkpoint', 'schedule', 'set', $VmName,
        '--interval-minutes', "$ScheduleIntervalMinutes", '--retention-max', "$ScheduleRetentionMax", '--yes')
    if ([string](Get-ObjectPropertyValue -InputObject $set -Name 'status') -ne 'succeeded') {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|schedule-set'
    }
    $after = Get-ScheduleReadback -Phase 'after-schedule-set'
    $summary.readbacks.schedule_set = $after
    Write-AtomicSummary
    if (-not $after.enabled -or
        [int]$after.interval_minutes -ne $ScheduleIntervalMinutes -or
        [int]$after.retention_max -ne $ScheduleRetentionMax -or
        -not (Test-PcvProductOff $after.product) -or
        $after.hyperv_checkpoint_count -ne 0) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|schedule-set|enabled=$($after.enabled)"
    }
}

function Invoke-ScheduleClearSlice {
    $clear = Start-PcvCliJob -StepName 'vm-checkpoint-schedule-clear' -Arguments @(
        'vm', 'checkpoint', 'schedule', 'clear', $VmName, '--yes')
    if ([string](Get-ObjectPropertyValue -InputObject $clear -Name 'status') -ne 'succeeded') {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|schedule-clear'
    }
    $after = Get-ScheduleReadback -Phase 'after-schedule-clear'
    $summary.readbacks.schedule_clear = $after
    Write-AtomicSummary
    if ($after.enabled -or $after.hyperv_checkpoint_count -ne 0) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|schedule-clear|enabled=$($after.enabled)"
    }
    $script:ScheduleEnabled = $false
}

function Get-ExportPackageReadback {
    param([Parameter(Mandatory)][string]$Path)

    if ($null -ne $RuntimeAdapter) {
        return Invoke-RuntimeOperation -Operation 'export-package' -Input @{ path = $Path }
    }
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return [pscustomobject][ordered]@{ exists = $false; vmcx_count = 0; vhdx_count = 0; vmgs_count = 0 }
    }
    $vmcx = @(Get-ChildItem -LiteralPath $Path -File -Filter '*.vmcx')
    $virtualMachines = Join-Path $Path 'Virtual Machines'
    if (Test-Path -LiteralPath $virtualMachines -PathType Container) {
        $vmcx += @(Get-ChildItem -LiteralPath $virtualMachines -File -Filter '*.vmcx')
    }
    $files = @(Get-ChildItem -LiteralPath $Path -File -Recurse)
    return [pscustomobject][ordered]@{
        exists = $true
        vmcx_count = $vmcx.Count
        vhdx_count = @($files | Where-Object { $_.Extension -in @('.vhdx', '.vhd') }).Count
        vmgs_count = @($files | Where-Object { $_.Extension -eq '.vmgs' }).Count
    }
}

function Invoke-ExportConfirmRequiredSlice {
    $unconfirmed = Invoke-PcvCliJson -StepName 'vm-export-unconfirmed' -AllowFailure -Arguments @(
        'vm', 'export', $VmName, '--directory', $exportDirFull, '--allowed-root', $exportRootFull)
    $code = Get-CliProblemCode -Payload $unconfirmed.Json -Stderr $unconfirmed.Stderr
    $jobId = Get-CliJobId -Created $unconfirmed
    $exportDirPresent = Test-PcvPath -Path $exportDirFull
    $summary.readbacks.export_confirm_required = [ordered]@{
        exit_code = $unconfirmed.ExitCode
        code = $code
        job_id = $jobId
        export_dir_absent = (-not $exportDirPresent)
    }
    Write-AtomicSummary
    if ($unconfirmed.ExitCode -eq 0 -or $null -ne $jobId -or $code -ne 'PCV_CLI_CONFIRMATION_REQUIRED' -or $exportDirPresent) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|export-confirm-required|exit=$($unconfirmed.ExitCode)|code=$code"
    }
}

function Invoke-ExportPathNotAllowedSlice {
    $outside = Invoke-PcvCliJson -StepName 'vm-export-preview-outside' -AllowFailure -Arguments @(
        'vm', 'export', 'preview', $VmName, '--directory', $outsideDirFull, '--allowed-root', $exportRootFull)
    $code = Get-CliProblemCode -Payload $outside.Json -Stderr $outside.Stderr
    $outsidePresent = Test-PcvPath -Path $outsideDirFull
    $summary.readbacks.export_path_not_allowed = [ordered]@{
        exit_code = $outside.ExitCode
        code = $code
        outside_dir_absent = (-not $outsidePresent)
    }
    Write-AtomicSummary
    if ($outside.ExitCode -eq 0 -or $code -ne 'PCV_VM_EXPORT_PATH_NOT_ALLOWED' -or $outsidePresent) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|export-path-not-allowed|exit=$($outside.ExitCode)|code=$code"
    }
}

function Invoke-ExportPreviewSlice {
    $preview = Invoke-PcvCliJson -StepName 'vm-export-preview' -Arguments @(
        'vm', 'export', 'preview', $VmName, '--directory', $exportDirFull, '--allowed-root', $exportRootFull)
    $data = Get-ObjectPropertyValue -InputObject $preview.Json -Name 'data'
    $jobId = Get-CliJobId -Created $preview
    $exportDirPresent = Test-PcvPath -Path $exportDirFull
    $summary.readbacks.export_preview = [ordered]@{
        dry_run = Get-ObjectPropertyValue -InputObject $data -Name 'dry_run'
        host_mutation_performed = Get-ObjectPropertyValue -InputObject $data -Name 'host_mutation_performed'
        job_id = $jobId
        export_dir_absent = (-not $exportDirPresent)
    }
    Write-AtomicSummary
    if ($null -ne $jobId -or -not [bool](Get-ObjectPropertyValue -InputObject $data -Name 'dry_run') -or
        [bool](Get-ObjectPropertyValue -InputObject $data -Name 'host_mutation_performed') -or $exportDirPresent) {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|export-preview'
    }
}

function Invoke-ExportSlice {
    $export = Start-PcvCliJob -StepName 'vm-export' -Arguments @(
        'vm', 'export', $VmName, '--directory', $exportDirFull, '--allowed-root', $exportRootFull, '--yes')
    if ([string](Get-ObjectPropertyValue -InputObject $export -Name 'status') -ne 'succeeded') {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|export'
    }
    $package = Get-ExportPackageReadback -Path $exportDirFull
    $sourceHyperV = Wait-HyperVState -Id ([Guid]$script:VmRecord.id) -Expected 'Off' -Phase 'after-export'
    $sourceProduct = Get-ProductState -Data (Get-ProductVmData -Phase 'after-export')
    $summary.readbacks.export = [ordered]@{
        package_exists = [bool]$package.exists
        vmcx_count = [int]$package.vmcx_count
        vhdx_count = [int]$package.vhdx_count
        vmgs_count = [int]$package.vmgs_count
        source_hyperv = $sourceHyperV
        source_product = $sourceProduct
    }
    Write-AtomicSummary
    if (-not [bool]$package.exists -or [int]$package.vmcx_count -ne 1 -or [int]$package.vhdx_count -lt 1 -or
        [int]$package.vmgs_count -ne 0 -or $sourceHyperV -ne 'Off' -or -not (Test-PcvProductOff $sourceProduct)) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|export|vmcx=$($package.vmcx_count)|vhdx=$($package.vhdx_count)"
    }
}

function Invoke-ImportPreviewSlice {
    $preview = Invoke-PcvCliJson -StepName 'vm-import-preview' -Arguments @(
        'vm', 'import', 'preview', '--name', $ImportVmName, '--directory', $exportDirFull,
        '--allowed-root', $exportRootFull, '--has-vmcx')
    $data = Get-ObjectPropertyValue -InputObject $preview.Json -Name 'data'
    $jobId = Get-CliJobId -Created $preview
    $targetPresent = @(Get-PcvVmByName -Name $ImportVmName -Purpose 'import-preview').Count -ne 0
    $summary.readbacks.import_preview = [ordered]@{
        dry_run = Get-ObjectPropertyValue -InputObject $data -Name 'dry_run'
        generate_new_id = Get-ObjectPropertyValue -InputObject $data -Name 'generate_new_id'
        apply_managed_marker = Get-ObjectPropertyValue -InputObject $data -Name 'apply_managed_marker'
        job_id = $jobId
        target_absent = (-not $targetPresent)
    }
    Write-AtomicSummary
    if ($null -ne $jobId -or $targetPresent -or
        -not [bool](Get-ObjectPropertyValue -InputObject $data -Name 'dry_run') -or
        -not [bool](Get-ObjectPropertyValue -InputObject $data -Name 'generate_new_id') -or
        -not [bool](Get-ObjectPropertyValue -InputObject $data -Name 'apply_managed_marker')) {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|import-preview'
    }
}

function Invoke-ImportSlice {
    $import = Start-PcvCliJob -StepName 'vm-import' -Arguments @(
        'vm', 'import', '--name', $ImportVmName, '--directory', $exportDirFull,
        '--allowed-root', $exportRootFull, '--yes') -DeferTerminalSummaryWrite
    if ([string](Get-ObjectPropertyValue -InputObject $import -Name 'status') -ne 'succeeded') {
        throw 'PCV_P2_OFFVM_STATE_MISMATCH|import'
    }
    $record = Resolve-CreatedVm -Record $script:ImportRecord -Job $import -Name $ImportVmName
    $importId = [Guid]$record.id
    $sourceId = [Guid]$script:VmRecord.id
    $importHyperV = Wait-HyperVState -Id $importId -Expected 'Off' -Phase 'after-import' -Record $record
    $importGet = Invoke-PcvCliJson -StepName 'vm-get-import' -Arguments @('vm', 'get', $ImportVmName)
    $importData = Get-ObjectPropertyValue -InputObject $importGet.Json -Name 'data'
    $importProduct = Get-ProductState -Data $importData
    $managed = [bool](Get-ObjectPropertyValue -InputObject $importData -Name 'managed_by_purecvisor')
    $sourceHyperV = Wait-HyperVState -Id $sourceId -Expected 'Off' -Phase 'after-import-source'
    $sourceProduct = Get-ProductState -Data (Get-ProductVmData -Phase 'after-import-source')
    $summary.readbacks.import = [ordered]@{
        import_vm_id = $record.id
        new_identity = ($importId -ne $sourceId)
        import_path = $record.observed_path
        import_hyperv = $importHyperV
        import_product = $importProduct
        managed = $managed
        source_hyperv = $sourceHyperV
        source_product = $sourceProduct
    }
    Write-AtomicSummary
    if ($importId -eq $sourceId -or $importHyperV -ne 'Off' -or -not (Test-PcvProductOff $importProduct) -or
        -not $managed -or $sourceHyperV -ne 'Off' -or -not (Test-PcvProductOff $sourceProduct)) {
        throw "PCV_P2_OFFVM_STATE_MISMATCH|import|new_identity=$($importId -ne $sourceId)|managed=$managed"
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
        throw "PCV_P2_OFFVM_CLEANUP_IDENTITY_DRIFT|phase=$Phase|missing-recorded-id"
    }

    try {
        $currentId = ([Guid]$current.Id).ToString('D')
        $currentName = [string]$current.Name
        $currentPath = Get-AbsolutePath -Path ([string]$current.Path)
        $recordedPath = Get-AbsolutePath -Path ([string]$Record.observed_path)
        $reservedRoot = Get-AbsolutePath -Path ([string]$Record.root)
        $comparison = Get-PathComparison
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
        throw "PCV_P2_OFFVM_CLEANUP_IDENTITY_DRIFT|phase=$Phase"
    }
    return $current
}

function Invoke-ScheduleCleanup {
    if (-not $script:ScheduleEnabled -or $null -eq $script:VmRecord -or [string]::IsNullOrWhiteSpace([string]$script:VmRecord.id)) {
        return
    }
    try {
        $clear = Start-PcvCliJob -StepName 'vm-checkpoint-schedule-clear-cleanup' -AllowFailure -Arguments @(
            'vm', 'checkpoint', 'schedule', 'clear', $VmName, '--yes')
        $summary.cleanup.schedule_cleared = [string](Get-ObjectPropertyValue -InputObject $clear -Name 'status') -eq 'succeeded'
    }
    catch {
        $summary.cleanup.schedule_cleared = $false
    }
    if ($summary.cleanup.schedule_cleared) { $script:ScheduleEnabled = $false }
}

function Invoke-ExactCleanup {
    $summary.cleanup.attempted = $true
    $cleanupErrors = [System.Collections.Generic.List[string]]::new()
    Invoke-ScheduleCleanup
    if ($script:ScheduleEnabled) {
        $cleanupErrors.Add('PCV_P2_OFFVM_CLEANUP_SCHEDULE_NOT_CLEARED') | Out-Null
    }
    $ordered = @($script:VmRecords | Where-Object { $_.kind -eq 'import' }) +
        @($script:VmRecords | Where-Object { $_.kind -ne 'import' })
    foreach ($record in $ordered) {
        try {
            $record.root = Assert-ValidatedChildPath -Root $vmRootFull -Candidate $record.root
            $sameName = @(Get-PcvVmByName -Name $record.name -Purpose 'cleanup-observation')
            if ([string]::IsNullOrWhiteSpace([string]$record.id)) {
                if ($sameName.Count -eq 0) {
                    if ($record.root_owned_by_run -and (Test-PcvPath -Path $record.root)) {
                        Remove-PcvDirectory -Path $record.root
                    }
                    $record.removed = $true
                    $record.root_removed = -not (Test-PcvPath -Path $record.root)
                    continue
                }
                $record.identity_status = 'orphan-blocker'
                $record.identity_blocker = $true
                $record.error = 'PCV_P2_OFFVM_CLEANUP_ID_MISMATCH'
                $cleanupErrors.Add('PCV_P2_OFFVM_CLEANUP_ID_MISMATCH') | Out-Null
                continue
            }
            $recordedId = [Guid]$record.id
            $different = @($sameName | Where-Object { [Guid]$_.Id -ne $recordedId })
            $collisionCode = $null
            if ($different.Count -gt 0) {
                $record.same_name_different_id_blocked = $true
                $summary.cleanup.same_name_different_id_blocked = $true
                $collisionCode = 'PCV_P2_OFFVM_CLEANUP_ID_MISMATCH'
            }

            $current = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-product-delete' -AllowAbsent
            if ($null -ne $current) {
                $record.product_delete_attempted = $true
                $deleteStep = if ($record.kind -eq 'import') { 'vm-delete-import' } else { 'vm-delete' }
                try {
                    $delete = Start-PcvCliJob -StepName $deleteStep -AllowFailure -Arguments @(
                        'vm', 'delete', $record.name, '--yes')
                    if ([string](Get-ObjectPropertyValue -InputObject $delete -Name 'status') -ne 'succeeded') {
                        throw 'product-delete-failed'
                    }
                }
                catch { }
            }
            $record.removed = $null -eq (Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'after-delete' -AllowAbsent)
            if (-not $record.removed) {
                throw "PCV_P2_OFFVM_CLEANUP_ID_MISMATCH|remaining=$recordedId"
            }
            $comparison = Get-PathComparison
            $collisionOwnsRecordedRoot = @($different | Where-Object {
                $candidate = Get-AbsolutePath -Path ([string]$_.Path)
                $candidate.Equals($record.root, $comparison) -or
                    $candidate.StartsWith($record.root + [System.IO.Path]::DirectorySeparatorChar, $comparison)
            }).Count -gt 0
            if ($record.root_owned_by_run -and -not $collisionOwnsRecordedRoot -and (Test-PcvPath -Path $record.root)) {
                $beforeRootRemoval = Get-ValidatedCleanupVm -Record $record -RecordedId $recordedId -Phase 'before-root-removal' -AllowAbsent
                if ($null -ne $beforeRootRemoval) {
                    throw 'PCV_P2_OFFVM_CLEANUP_IDENTITY_DRIFT|phase=before-root-removal|recorded-id-present'
                }
                Remove-PcvDirectory -Path $record.root
            }
            $record.root_removed = -not (Test-PcvPath -Path $record.root)
            if (-not $record.root_removed) {
                throw "PCV_P2_OFFVM_CLEANUP_ROOT_INVALID|remaining=$($record.root)"
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
    if ($Family -eq 'export-import') {
        try {
            if (Test-PcvPath -Path $outsideDirFull) { Remove-PcvDirectory -Path $outsideDirFull }
        }
        catch {
            $cleanupErrors.Add('PCV_P2_OFFVM_CLEANUP_ROOT_INVALID') | Out-Null
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

$sliceActions = @{
    source_create = { Invoke-SourceCreateSlice }
    nic_confirm_required = { Invoke-NicConfirmRequiredSlice }
    nic_add = { Invoke-NicAddSlice }
    nic_limit = { Invoke-NicLimitSlice }
    dvd_guard = { Invoke-DvdGuardSlice }
    schedule_preview = { Invoke-SchedulePreviewSlice }
    schedule_interval_invalid = { Invoke-ScheduleIntervalInvalidSlice }
    schedule_set = { Invoke-ScheduleSetSlice }
    schedule_clear = { Invoke-ScheduleClearSlice }
    export_confirm_required = { Invoke-ExportConfirmRequiredSlice }
    export_path_not_allowed = { Invoke-ExportPathNotAllowedSlice }
    export_preview = { Invoke-ExportPreviewSlice }
    export = { Invoke-ExportSlice }
    import_preview = { Invoke-ImportPreviewSlice }
    import = { Invoke-ImportSlice }
}

$runError = $null
try {
    Assert-InstalledProduct
    Assert-ServiceAvailable
    if ($Family -eq 'device-add') { Assert-SwitchPresent }
    Assert-VmAbsent -Name $VmName
    Assert-PcvPathAbsent -Path $vmOwnRootFull -Kind 'vm'
    if ($Family -eq 'export-import') {
        Assert-VmAbsent -Name $ImportVmName
        Assert-PcvPathAbsent -Path $exportRootFull -Kind 'export'
        Assert-PcvPathAbsent -Path $outsideDirFull -Kind 'outside'
    }
    Write-AtomicSummary

    $script:VmRecord = New-VmOwnershipRecord -Name $VmName -ExpectedRoot $vmOwnRootFull
    if ($Family -eq 'export-import') {
        # Hyper-V import registers the exported package in place, so the imported VM lives under the export root.
        $script:ImportRecord = New-VmOwnershipRecord -Kind 'import' -Name $ImportVmName -ExpectedRoot $exportRootFull
    }
    $summary.host_mutation_performed = $true
    $summary.actual_execution = 'installed-cli-and-hyperv'
    if (-not (Test-PcvPath -Path $vmRootFull)) { New-PcvDirectory -Path $vmRootFull }
    New-PcvDirectory -Path $script:VmRecord.root
    $script:VmRecord.root_owned_by_run = $true
    if ($null -ne $script:ImportRecord) {
        New-PcvDirectory -Path $script:ImportRecord.root
        $script:ImportRecord.root_owned_by_run = $true
    }
    Write-AtomicSummary
    foreach ($slice in $plannedSlices) {
        if ($slice -eq 'cleanup') { continue }
        Invoke-TrackedSlice -Slice $slice -Action $sliceActions[$slice]
        Assert-SlicePassed -Slice $slice
    }
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
                throw "PCV_P2_OFFVM_CLEANUP_FAILED|$($summary.cleanup.error)"
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
        $runError = if ($null -eq $runError) { 'PCV_P2_OFFVM_SERVICE_LOST' } else { "$runError; PCV_P2_OFFVM_SERVICE_LOST" }
    }
    $slicesOk = @($plannedSlices | Where-Object { $summary.slice_verdicts[$_] -ne 'PASS' }).Count -eq 0
    $summary.ok = ($null -eq $runError -and $cleanupOk -and $slicesOk -and -not [bool]$summary.secret_observed)
    $summary.overall_verdict = if ($summary.ok) { 'PASS' } else { 'FAIL' }
    if ($null -ne $runError) { $summary.error = $runError }
    $summary.completed_at = (Get-Date).ToUniversalTime().ToString('o')
    Write-AtomicSummary
}

$result = [pscustomobject]$summary
$result
if (-not $summary.ok) {
    throw "PCV_P2_OFFVM_FAILED|$($summary.error)"
}
