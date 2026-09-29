#requires -Version 7.0

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [string]$ArtifactRoot = '',
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$VmName = 'pcv-guest-installed-04253-r1',
    [string]$CredentialRef = 'dpapi:C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi',
    [string]$HostFilesRoot = 'C:\ProgramData\PureCVisor\desktop-node\guest-files',
    [string]$GuestPrefix = 'C:\Users\Public\PureCVisor',

    [ValidateRange(1, 67108864)]
    [int]$PayloadBytes = 1048576,
    [ValidateRange(1, 3600)]
    [int]$JobTimeoutSeconds = 300,
    [ValidateRange(1, 1800)]
    [int]$CommandTimeoutSeconds = 120,
    [ValidateRange(30, 3600)]
    [int]$BootTimeoutSeconds = 600,
    [ValidateRange(10, 600)]
    [int]$GuestTimeoutSeconds = 120,

    [switch]$DryRun,
    [Parameter(DontShow)][scriptblock]$RuntimeAdapter,
    [Parameter(DontShow)][scriptblock]$SummaryWriter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$PersistentPolicyMarker = 'persistent_policy=keep-until-next-evidence-cycle'
$GuestFileMaxBytes = 64L * 1024L * 1024L

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

function Get-TextDigest {
    param([AllowEmptyString()][string]$Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Get-LineDigests {
    param([Parameter(Mandatory)][string]$Line)

    # Guest output passes through Out-String, so accept the line with CRLF, LF, or no terminator.
    return @((Get-TextDigest "$Line`r`n"), (Get-TextDigest "$Line`n"), (Get-TextDigest $Line))
}

$versionTag = (($Version.Split('-')[0]) -replace '[^0-9A-Za-z]', '').ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($versionTag)) {
    throw 'PCV_P1_GUESTFILE_VERSION_INVALID'
}
if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path (Get-Location).Path "artifacts/service-plan-p1-guest-file-actual-vm-$versionTag"
}
if ($CredentialRef -notmatch '^(dpapi|wincred):\S+$') {
    throw 'PCV_P1_GUESTFILE_CREDENTIAL_REF_INVALID'
}
if ($GuestPrefix -notmatch '^[A-Za-z]:\\[^*?"<>|]+$') {
    throw "PCV_P1_GUESTFILE_GUEST_PREFIX_INVALID|$GuestPrefix"
}

$artifactRootFull = Get-AbsolutePath -Path $ArtifactRoot
$hostFilesRootFull = (Get-AbsolutePath -Path $HostFilesRoot).TrimEnd('\')
$runKey = Get-ShortHash -Value "$Version|$artifactRootFull"
$runName = "pcv-p1-guestfile-$versionTag-$runKey"
$stagingDirFull = Join-Path $hostFilesRootFull $runName
$payloadPath = Join-Path $stagingDirFull "$runName.bin"
$oversizePath = Join-Path $stagingDirFull "$runName-oversize.bin"
$guestFilePath = "$($GuestPrefix.TrimEnd('\'))\$runName.bin"
$outsideGuestPath = "C:\Windows\Temp\$runName.bin"

$plannedSlices = @(
    'preflight', 'vm_start', 'channel_verify', 'guest_prefix_probe', 'host_staging',
    'preview_path_not_allowed', 'preview_size_limit', 'preview_ok', 'copy_confirm_required',
    'copy', 'guest_readback', 'cleanup')

New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$summaryTempPath = Join-Path $artifactRootFull 'summary.json.tmp'
$script:Steps = [System.Collections.Generic.List[object]]::new()
$script:PcvCli = Join-Path (Get-AbsolutePath -Path $ProductRoot) 'pcvcli.exe'
$script:VmId = $null
$script:ChannelReady = $false
$script:CopyAttempted = $false
$script:StagingCreated = $false

$sliceVerdicts = [ordered]@{}
foreach ($slice in $plannedSlices) { $sliceVerdicts[$slice] = 'NOT_RUN' }
$summary = [ordered]@{
    schema_version = 'pcv-service-plan-p1-guest-file-actual-vm-summary/v1'
    scope = 'service-plan-p1-guest-file-actual-vm'
    version = $Version
    ok = $false
    overall_verdict = 'NOT_RUN'
    actual_execution = 'not-started'
    artifact_root_resolved = $artifactRootFull
    product_root_resolved = (Get-AbsolutePath -Path $ProductRoot)
    installed_manifest_version = $null
    installed_cli_sha256 = $null
    vm_name = $VmName
    vm_id = $null
    credential_ref = $CredentialRef
    host_staging_dir = $stagingDirFull
    guest_file_path = $guestFilePath
    payload_bytes = $PayloadBytes
    payload_sha256 = $null
    power = [ordered]@{ before = $null; started_by_run = $false; after = $null; restored = $null; stop_method = $null }
    guest_prefix_present_before = $null
    slice_verdicts = $sliceVerdicts
    queued_jobs = [ordered]@{}
    readbacks = [ordered]@{}
    cleanup = [ordered]@{ attempted = $false; verdict = 'NOT_RUN'; guest_file_removed = $null; host_staging_removed = $null; error = $null }
    plan = @($plannedSlices | ForEach-Object { [ordered]@{ slice = $_; mutates_host = ($_ -notin @('preflight', 'guest_prefix_probe')) } })
    nonclaims = @('guest-to-host-pull-not-exercised', 'directory-copy-not-exercised', 'linux-guest-not-exercised')
    steps = @()
    host_mutation_performed = $false
    secret_observed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    error = $null
    started_at = (Get-Date).ToUniversalTime().ToString('o')
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

    if (Test-SecretMaterial -Text $Message) { return 'PCV_P1_GUESTFILE_SECRET_OBSERVED' }
    $match = [regex]::Match([string]$Message, '\bPCV_[A-Z0-9_]+\b')
    if ($match.Success) { return $match.Value }
    return 'PCV_P1_GUESTFILE_INTERNAL_FAILURE'
}

function Set-SecretObserved {
    $summary.secret_observed = $true
    $summary.ok = $false
    $summary.overall_verdict = 'FAIL'
    $summary.error = 'PCV_P1_GUESTFILE_SECRET_OBSERVED'
}

function Write-AtomicSummary {
    try {
        $summary.steps = $script:Steps.ToArray()
        $json = $summary | ConvertTo-Json -Depth 32
        if (Test-SecretMaterial -Text $json) {
            $summary.secret_observed = $true
            throw 'PCV_P1_GUESTFILE_SECRET_OBSERVED_IN_SUMMARY'
        }
        if ($null -ne $SummaryWriter) {
            & $SummaryWriter $summaryPath $summaryTempPath $json
        }
        else {
            [System.IO.File]::WriteAllText($summaryTempPath, $json, [System.Text.UTF8Encoding]::new($false))
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
        throw 'PCV_P1_GUESTFILE_SUMMARY_WRITE_FAILED'
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
        throw "PCV_P1_GUESTFILE_RUNTIME_ADAPTER_NOT_CONFIGURED|$Operation"
    }
    return & $RuntimeAdapter $Operation $RuntimePayload
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

function Test-PcvPath {
    param([Parameter(Mandatory)][string]$Path)

    if ($null -ne $RuntimeAdapter) {
        return [bool](Invoke-RuntimeOperation -Operation 'path-exists' -Input @{ path = $Path })
    }
    return Test-Path -LiteralPath $Path
}

function Assert-InstalledProduct {
    if ($null -ne $RuntimeAdapter) {
        $installed = Invoke-RuntimeOperation -Operation 'installed-product'
        $summary.installed_manifest_version = [string]$installed.version
        $summary.installed_cli_sha256 = [string]$installed.cli_sha256
        $script:PcvCli = [string]$installed.cli_path
    }
    else {
        $manifestPath = Join-Path $summary.product_root_resolved 'product-manifest.json'
        if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
            throw "PCV_P1_GUESTFILE_INSTALLED_MANIFEST_MISSING|$manifestPath"
        }
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json -Depth 32
        $summary.installed_manifest_version = [string](Get-ObjectPropertyValue -InputObject $manifest -Name 'version')
        if (-not (Test-Path -LiteralPath $script:PcvCli -PathType Leaf)) {
            throw "PCV_P1_GUESTFILE_CLI_NOT_FOUND|$script:PcvCli"
        }
        $summary.installed_cli_sha256 = (Get-FileHash -LiteralPath $script:PcvCli -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    if ([string]$summary.installed_manifest_version -cne $Version) {
        throw "PCV_P1_GUESTFILE_INSTALLED_VERSION_MISMATCH|expected=$Version|actual=$($summary.installed_manifest_version)"
    }
}

function Assert-ServiceAvailable {
    $state = if ($null -ne $RuntimeAdapter) {
        [string](Invoke-RuntimeOperation -Operation 'service-state')
    }
    else {
        $service = Get-Service -Name 'PureCVisorDesktopNode' -ErrorAction SilentlyContinue
        if ($null -eq $service) { 'Missing' } else { [string]$service.Status }
    }
    if ($state -ne 'Running') {
        throw 'PCV_P1_GUESTFILE_SERVICE_LOST'
    }
}

function Get-TargetVm {
    if ($null -ne $RuntimeAdapter) {
        return @(Invoke-RuntimeOperation -Operation 'vm-by-name' -Input @{ name = $VmName })
    }
    return @(Get-VM -Name $VmName -ErrorAction SilentlyContinue | ForEach-Object {
        [pscustomobject]@{ Id = $_.Id; Name = $_.Name; Notes = [string]$_.Notes }
    })
}

# Hyper-V PowerShell cmdlets cache VM objects per process, so power state is read from WMI each time.
function Get-HyperVPowerState {
    if ($null -ne $RuntimeAdapter) {
        return [string](Invoke-RuntimeOperation -Operation 'vm-state' -Input @{ id = $script:VmId })
    }
    $vm = Get-CimInstance -Namespace 'root\virtualization\v2' -ClassName Msvm_ComputerSystem -Filter "Name='$($script:VmId)'"
    if ($null -eq $vm) { return 'Missing' }
    switch ([int]$vm.EnabledState) {
        2 { return 'Running' }
        3 { return 'Off' }
        6 { return 'Saved' }
        9 { return 'Paused' }
        default { return "EnabledState-$($vm.EnabledState)" }
    }
}

function Wait-HyperVPowerState {
    param(
        [Parameter(Mandatory)][string]$Expected,
        [Parameter(Mandatory)][int]$TimeoutSeconds
    )

    if ($null -ne $RuntimeAdapter) {
        return [string](Invoke-RuntimeOperation -Operation 'wait-vm-state' -Input @{ id = $script:VmId; expected = $Expected; timeout_seconds = $TimeoutSeconds })
    }
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $state = Get-HyperVPowerState
        if ($state -eq $Expected) { return $state }
        Start-Sleep -Seconds 3
    } while ((Get-Date) -lt $deadline)
    return Get-HyperVPowerState
}

function Wait-GuestHeartbeat {
    if ($null -ne $RuntimeAdapter) {
        return [bool](Invoke-RuntimeOperation -Operation 'wait-heartbeat' -Input @{ id = $script:VmId; timeout_seconds = $BootTimeoutSeconds })
    }
    $deadline = (Get-Date).AddSeconds($BootTimeoutSeconds)
    do {
        $vm = Get-CimInstance -Namespace 'root\virtualization\v2' -ClassName Msvm_ComputerSystem -Filter "Name='$($script:VmId)'"
        $heartbeat = if ($null -eq $vm) { $null } else {
            Get-CimAssociatedInstance -InputObject $vm -ResultClassName Msvm_HeartbeatComponent | Select-Object -First 1
        }
        if ($null -ne $heartbeat -and @($heartbeat.OperationalStatus) -contains [uint16]2) { return $true }
        Start-Sleep -Seconds 5
    } while ((Get-Date) -lt $deadline)
    return $false
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
        foreach ($argument in $Arguments) { $startInfo.ArgumentList.Add($argument) }
        $process = [System.Diagnostics.Process]::Start($startInfo)
        if ($null -eq $process) { throw "PCV_P1_GUESTFILE_COMMAND_START_FAILED|$StepName" }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($CommandTimeoutSeconds * 1000)) {
            try { $process.Kill($true) } catch { }
            throw "PCV_P1_GUESTFILE_COMMAND_TIMEOUT|$StepName"
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
        $code = Get-CliProblemCode -Payload $payload -Stderr $stderr
        if ([string]::IsNullOrWhiteSpace($code)) { $code = 'PCV_P1_GUESTFILE_COMMAND_FAILED' }
        throw "$code|$StepName|exit=$exitCode"
    }
    return [pscustomobject]@{ ExitCode = $exitCode; Json = $payload; Stderr = $stderr; SecretObserved = $secretObserved }
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
        if ([string](Get-ObjectPropertyValue -InputObject $data -Name 'status') -in @('succeeded', 'failed', 'canceled')) {
            return $data
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    throw "PCV_P1_GUESTFILE_JOB_TIMEOUT|$StepName|job=$JobId"
}

function Start-PcvCliJob {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    $created = Invoke-PcvCliJson -StepName $StepName -Arguments $Arguments -AllowFailure:$AllowFailure
    $jobId = Get-CliJobId -Created $created
    if ($null -eq $jobId) {
        throw "PCV_P1_GUESTFILE_JOB_ID_MISSING|$StepName"
    }
    $summary.queued_jobs[$StepName] = [ordered]@{ job_id = $jobId; status = 'queued'; error_code = $null }
    Write-AtomicSummary
    $job = if ($null -ne $RuntimeAdapter) {
        Invoke-RuntimeOperation -Operation 'wait-job' -Input @{ step = $StepName; job_id = $jobId; timeout_seconds = $JobTimeoutSeconds }
    }
    else {
        Wait-PcvJobTerminal -JobId $jobId -StepName $StepName
    }
    $status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
    $errorCode = [string](Get-ObjectPropertyValue -InputObject (Get-ObjectPropertyValue -InputObject $job -Name 'error') -Name 'code')
    if (-not [string]::IsNullOrEmpty($errorCode) -and $errorCode -notmatch '^PCV_[A-Z0-9_]+$') {
        $errorCode = 'PCV_P1_GUESTFILE_REMOTE_ERROR_REDACTED'
    }
    $summary.queued_jobs[$StepName].status = $status
    $summary.queued_jobs[$StepName].error_code = $errorCode
    Write-AtomicSummary
    if ($status -ne 'succeeded' -and -not $AllowFailure.IsPresent) {
        throw "PCV_P1_GUESTFILE_JOB_FAILED|$StepName|job=$jobId|status=$status|code=$errorCode"
    }
    return $job
}

function Get-JobResultData {
    param($Job)

    return Get-ObjectPropertyValue -InputObject (Get-ObjectPropertyValue -InputObject $Job -Name 'result') -Name 'data'
}

function Invoke-GuestCommand {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string]$Script
    )

    $job = Start-PcvCliJob -StepName $StepName -AllowFailure -Arguments @(
        'vm', 'guest-exec', $VmName, '--credential-ref', $CredentialRef, '--timeout-sec', "$GuestTimeoutSeconds",
        '--', 'powershell.exe', '-NoProfile', '-NonInteractive', '-Command', $Script)
    $data = Get-JobResultData -Job $job
    return [pscustomobject]@{
        Status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
        ExitCode = Get-ObjectPropertyValue -InputObject $data -Name 'exit_code'
        StdoutDigest = [string](Get-ObjectPropertyValue -InputObject $data -Name 'stdout_digest')
        StdoutByteCount = Get-ObjectPropertyValue -InputObject $data -Name 'stdout_byte_count'
    }
}

function Get-GuestBoolean {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string]$Script
    )

    $result = Invoke-GuestCommand -StepName $StepName -Script $Script
    if ($result.Status -ne 'succeeded' -or [int]$result.ExitCode -ne 0) { return $null }
    if ((Get-LineDigests -Line 'True') -contains $result.StdoutDigest) { return $true }
    if ((Get-LineDigests -Line 'False') -contains $result.StdoutDigest) { return $false }
    return $null
}

function ConvertTo-GuestLiteral {
    param([Parameter(Mandatory)][string]$Value)

    return "'" + $Value.Replace("'", "''") + "'"
}

function Assert-SlicePassed {
    param([Parameter(Mandatory)][string]$Slice)

    if ([string]$summary.slice_verdicts[$Slice] -ne 'PASS') {
        throw "PCV_P1_GUESTFILE_SLICE_FAILED|$Slice"
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

function Invoke-PreflightSlice {
    Assert-InstalledProduct
    Assert-ServiceAvailable
    $vms = @(Get-TargetVm)
    if ($vms.Count -ne 1) {
        throw "PCV_P1_GUESTFILE_VM_CARDINALITY|count=$($vms.Count)"
    }
    if ([string]$vms[0].Notes -notmatch [regex]::Escape($PersistentPolicyMarker)) {
        throw 'PCV_P1_GUESTFILE_VM_NOT_PERSISTENT_TARGET'
    }
    $script:VmId = ([Guid]$vms[0].Id).ToString('D')
    $summary.vm_id = $script:VmId
    $credentialPath = $CredentialRef -replace '^dpapi:', ''
    $credentialPresent = $CredentialRef.StartsWith('wincred:', [System.StringComparison]::Ordinal) -or (Test-PcvPath -Path $credentialPath)
    $hostRootPresent = Test-PcvPath -Path $hostFilesRootFull
    $stagingAbsent = -not (Test-PcvPath -Path $stagingDirFull)
    $summary.power.before = Get-HyperVPowerState
    $summary.readbacks.preflight = [ordered]@{
        vm_id = $script:VmId
        persistent_policy = $true
        credential_present = $credentialPresent
        host_root_present = $hostRootPresent
        staging_absent = $stagingAbsent
        power_before = $summary.power.before
    }
    Write-AtomicSummary
    if (-not $credentialPresent -or -not $hostRootPresent -or -not $stagingAbsent -or
        $summary.power.before -notin @('Off', 'Running')) {
        throw "PCV_P1_GUESTFILE_PREFLIGHT_BLOCKED|power=$($summary.power.before)"
    }
}

function Invoke-VmStartSlice {
    if ($summary.power.before -eq 'Off') {
        $summary.host_mutation_performed = $true
        $summary.actual_execution = 'installed-cli-and-hyperv'
        $summary.power.started_by_run = $true
        Write-AtomicSummary
        $null = Start-PcvCliJob -StepName 'vm-start' -Arguments @('vm', 'start', $VmName)
    }
    $running = Wait-HyperVPowerState -Expected 'Running' -TimeoutSeconds $BootTimeoutSeconds
    $heartbeat = if ($running -eq 'Running') { Wait-GuestHeartbeat } else { $false }
    $summary.readbacks.vm_start = [ordered]@{ started_by_run = $summary.power.started_by_run; power = $running; heartbeat_ok = $heartbeat }
    Write-AtomicSummary
    if ($running -ne 'Running' -or -not $heartbeat) {
        throw "PCV_P1_GUESTFILE_STATE_MISMATCH|vm-start|power=$running|heartbeat=$heartbeat"
    }
}

function Invoke-ChannelVerifySlice {
    $attempts = [System.Collections.Generic.List[object]]::new()
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        $job = Start-PcvCliJob -StepName "guest-channel-verify-$attempt" -AllowFailure -Arguments @(
            'vm', 'guest-agent-ensure-channel', $VmName, '--verify', '--credential-ref', $CredentialRef,
            '--timeout-sec', "$GuestTimeoutSeconds")
        $data = Get-JobResultData -Job $job
        $status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
        $attempts.Add([ordered]@{ attempt = $attempt; status = $status; transport = Get-ObjectPropertyValue -InputObject $data -Name 'transport' }) | Out-Null
        if ($status -eq 'succeeded') {
            $script:ChannelReady = $true
            break
        }
        if ($attempt -lt 3) {
            if ($null -eq $RuntimeAdapter) { Start-Sleep -Seconds 20 }
        }
    }
    $summary.readbacks.channel_verify = [ordered]@{ attempts = $attempts.ToArray(); ready = $script:ChannelReady }
    Write-AtomicSummary
    if (-not $script:ChannelReady) {
        throw 'PCV_P1_GUESTFILE_STATE_MISMATCH|channel-verify'
    }
}

function Invoke-GuestPrefixProbeSlice {
    $present = Get-GuestBoolean -StepName 'guest-prefix-probe' -Script "Test-Path -LiteralPath $(ConvertTo-GuestLiteral $GuestPrefix)"
    $summary.guest_prefix_present_before = $present
    $summary.readbacks.guest_prefix_probe = [ordered]@{ prefix = $GuestPrefix; present = $present }
    Write-AtomicSummary
    if ($null -eq $present) {
        throw 'PCV_P1_GUESTFILE_STATE_MISMATCH|guest-prefix-probe'
    }
}

function Invoke-HostStagingSlice {
    $staging = if ($null -ne $RuntimeAdapter) {
        Invoke-RuntimeOperation -Operation 'create-staging' -Input @{
            directory = $stagingDirFull
            payload_path = $payloadPath
            payload_bytes = $PayloadBytes
            oversize_path = $oversizePath
            oversize_bytes = $GuestFileMaxBytes + 1
        }
    }
    else {
        New-Item -ItemType Directory -Path $stagingDirFull -ErrorAction Stop | Out-Null
        $script:StagingCreated = $true
        $bytes = [byte[]]::new($PayloadBytes)
        [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
        [System.IO.File]::WriteAllBytes($payloadPath, $bytes)
        $stream = [System.IO.File]::Open($oversizePath, [System.IO.FileMode]::CreateNew)
        try { $stream.SetLength($GuestFileMaxBytes + 1) } finally { $stream.Dispose() }
        [pscustomobject]@{
            payload_sha256 = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
            payload_length = (Get-Item -LiteralPath $payloadPath).Length
            oversize_length = (Get-Item -LiteralPath $oversizePath).Length
        }
    }
    $script:StagingCreated = $true
    $summary.host_mutation_performed = $true
    $summary.payload_sha256 = ([string]$staging.payload_sha256).ToUpperInvariant()
    $summary.readbacks.host_staging = [ordered]@{
        payload_length = [long]$staging.payload_length
        oversize_length = [long]$staging.oversize_length
    }
    Write-AtomicSummary
    if ([long]$staging.payload_length -ne $PayloadBytes -or [long]$staging.oversize_length -le $GuestFileMaxBytes) {
        throw 'PCV_P1_GUESTFILE_STATE_MISMATCH|host-staging'
    }
}

function Invoke-GuestFilePreview {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string]$HostPath,
        [Parameter(Mandatory)][string]$GuestPath,
        [string[]]$Mode = @('--dry-run')
    )

    return Invoke-PcvCliJson -StepName $StepName -AllowFailure -Arguments (@(
        'vm', 'guest-file', $VmName, '--host-path', $HostPath, '--guest-path', $GuestPath,
        '--credential-ref', $CredentialRef, '--timeout-sec', "$GuestTimeoutSeconds") + $Mode)
}

function Assert-Rejected {
    param(
        [Parameter(Mandatory)][string]$Slice,
        [Parameter(Mandatory)]$Result,
        [Parameter(Mandatory)][string[]]$ExpectedCodes
    )

    $code = Get-CliProblemCode -Payload $Result.Json -Stderr $Result.Stderr
    $jobId = Get-CliJobId -Created $Result
    $summary.readbacks[$Slice] = [ordered]@{ exit_code = $Result.ExitCode; code = $code; job_id = $jobId }
    Write-AtomicSummary
    if ($Result.ExitCode -eq 0 -or $null -ne $jobId -or $code -notin $ExpectedCodes) {
        throw "PCV_P1_GUESTFILE_STATE_MISMATCH|$Slice|exit=$($Result.ExitCode)|code=$code"
    }
}

function Invoke-PreviewOkSlice {
    $preview = Invoke-GuestFilePreview -StepName 'guest-file-preview' -HostPath $payloadPath -GuestPath $guestFilePath
    $data = Get-ObjectPropertyValue -InputObject $preview.Json -Name 'data'
    $present = Get-GuestBoolean -StepName 'guest-file-probe-after-preview' -Script "Test-Path -LiteralPath $(ConvertTo-GuestLiteral $guestFilePath)"
    $summary.readbacks.preview_ok = [ordered]@{
        exit_code = $preview.ExitCode
        job_id = Get-CliJobId -Created $preview
        size_bytes = Get-ObjectPropertyValue -InputObject $data -Name 'size_bytes'
        copied = Get-ObjectPropertyValue -InputObject $data -Name 'copied'
        guest_file_present = $present
    }
    Write-AtomicSummary
    if ($preview.ExitCode -ne 0 -or $null -ne (Get-CliJobId -Created $preview) -or
        [long](Get-ObjectPropertyValue -InputObject $data -Name 'size_bytes') -ne $PayloadBytes -or
        [bool](Get-ObjectPropertyValue -InputObject $data -Name 'copied') -or $present -ne $false) {
        throw "PCV_P1_GUESTFILE_STATE_MISMATCH|preview-ok|exit=$($preview.ExitCode)|guest_file=$present"
    }
}

function Invoke-CopySlice {
    $script:CopyAttempted = $true
    Write-AtomicSummary
    $job = Start-PcvCliJob -StepName 'guest-file-copy' -Arguments @(
        'vm', 'guest-file', $VmName, '--host-path', $payloadPath, '--guest-path', $guestFilePath,
        '--credential-ref', $CredentialRef, '--timeout-sec', "$GuestTimeoutSeconds", '--yes')
    $data = Get-JobResultData -Job $job
    $summary.readbacks.copy = [ordered]@{
        copied = Get-ObjectPropertyValue -InputObject $data -Name 'copied'
        size_bytes = Get-ObjectPropertyValue -InputObject $data -Name 'size_bytes'
    }
    Write-AtomicSummary
    if (-not [bool](Get-ObjectPropertyValue -InputObject $data -Name 'copied')) {
        throw 'PCV_P1_GUESTFILE_STATE_MISMATCH|copy'
    }
}

function Invoke-GuestReadbackSlice {
    $result = Invoke-GuestCommand -StepName 'guest-file-hash' -Script "(Get-FileHash -Algorithm SHA256 -LiteralPath $(ConvertTo-GuestLiteral $guestFilePath)).Hash"
    $matched = (Get-LineDigests -Line $summary.payload_sha256) -contains $result.StdoutDigest
    $summary.readbacks.guest_readback = [ordered]@{
        status = $result.Status
        exit_code = $result.ExitCode
        stdout_byte_count = $result.StdoutByteCount
        hash_matches_payload = $matched
    }
    Write-AtomicSummary
    if ($result.Status -ne 'succeeded' -or [int]$result.ExitCode -ne 0 -or -not $matched) {
        throw "PCV_P1_GUESTFILE_STATE_MISMATCH|guest-readback|matched=$matched"
    }
}

function Invoke-Cleanup {
    $summary.cleanup.attempted = $true
    $errors = [System.Collections.Generic.List[string]]::new()
    if ($script:CopyAttempted -and $script:ChannelReady) {
        try {
            $literal = ConvertTo-GuestLiteral $guestFilePath
            $present = Get-GuestBoolean -StepName 'guest-file-remove' -Script "if (Test-Path -LiteralPath $literal) { Remove-Item -LiteralPath $literal -ErrorAction Stop }; Test-Path -LiteralPath $literal"
            $summary.cleanup.guest_file_removed = ($present -eq $false)
            if ($present -ne $false) { $errors.Add('PCV_P1_GUESTFILE_CLEANUP_GUEST_FILE_REMAINS') | Out-Null }
        }
        catch {
            $summary.cleanup.guest_file_removed = $false
            $errors.Add((Get-SafeFailureCode -Message $_.Exception.Message)) | Out-Null
        }
    }
    elseif ($script:CopyAttempted) {
        $summary.cleanup.guest_file_removed = $false
        $errors.Add('PCV_P1_GUESTFILE_CLEANUP_GUEST_UNREACHABLE') | Out-Null
    }
    if ($script:StagingCreated -or (Test-PcvPath -Path $stagingDirFull)) {
        try {
            if ($null -ne $RuntimeAdapter) {
                Invoke-RuntimeOperation -Operation 'remove-directory' -Input @{ path = $stagingDirFull } | Out-Null
            }
            elseif (Test-Path -LiteralPath $stagingDirFull) {
                [System.IO.Directory]::Delete($stagingDirFull, $true)
            }
            $summary.cleanup.host_staging_removed = -not (Test-PcvPath -Path $stagingDirFull)
            if (-not $summary.cleanup.host_staging_removed) { $errors.Add('PCV_P1_GUESTFILE_CLEANUP_STAGING_REMAINS') | Out-Null }
        }
        catch {
            $summary.cleanup.host_staging_removed = $false
            $errors.Add((Get-SafeFailureCode -Message $_.Exception.Message)) | Out-Null
        }
    }
    if ($summary.power.started_by_run) {
        try {
            $summary.power.stop_method = 'shutdown'
            $null = Start-PcvCliJob -StepName 'vm-shutdown' -AllowFailure -Arguments @('vm', 'shutdown', $VmName)
            $after = Wait-HyperVPowerState -Expected 'Off' -TimeoutSeconds 300
            if ($after -ne 'Off') {
                $summary.power.stop_method = 'poweroff'
                $null = Start-PcvCliJob -StepName 'vm-poweroff' -AllowFailure -Arguments @('vm', 'poweroff', $VmName)
                $after = Wait-HyperVPowerState -Expected 'Off' -TimeoutSeconds 120
            }
            $summary.power.after = $after
        }
        catch {
            $summary.power.after = Get-HyperVPowerState
            $errors.Add((Get-SafeFailureCode -Message $_.Exception.Message)) | Out-Null
        }
    }
    elseif ($null -ne $script:VmId) {
        $summary.power.after = Get-HyperVPowerState
    }
    if ($null -ne $summary.power.before -and $null -ne $summary.power.after) {
        $summary.power.restored = ($summary.power.after -eq $summary.power.before)
        if (-not $summary.power.restored) { $errors.Add('PCV_P1_GUESTFILE_CLEANUP_POWER_NOT_RESTORED') | Out-Null }
    }
    if ($errors.Count -gt 0) {
        $summary.cleanup.verdict = 'FAIL'
        $summary.cleanup.error = @($errors | Select-Object -Unique) -join '; '
        return $false
    }
    $summary.cleanup.verdict = 'PASS'
    return $true
}

$sliceActions = [ordered]@{
    preflight = { Invoke-PreflightSlice }
    vm_start = { Invoke-VmStartSlice }
    channel_verify = { Invoke-ChannelVerifySlice }
    guest_prefix_probe = { Invoke-GuestPrefixProbeSlice }
    host_staging = { Invoke-HostStagingSlice }
    preview_path_not_allowed = {
        Assert-Rejected -Slice 'preview_path_not_allowed' -ExpectedCodes @('PCV_GUEST_FILE_PATH_NOT_ALLOWED') `
            -Result (Invoke-GuestFilePreview -StepName 'guest-file-preview-outside' -HostPath $payloadPath -GuestPath $outsideGuestPath)
    }
    preview_size_limit = {
        Assert-Rejected -Slice 'preview_size_limit' -ExpectedCodes @('PCV_GUEST_FILE_SIZE_LIMIT') `
            -Result (Invoke-GuestFilePreview -StepName 'guest-file-preview-oversize' -HostPath $oversizePath -GuestPath $guestFilePath)
    }
    preview_ok = { Invoke-PreviewOkSlice }
    copy_confirm_required = {
        Assert-Rejected -Slice 'copy_confirm_required' -ExpectedCodes @('PCV_CLI_CONFIRMATION_REQUIRED', 'PCV_CLI_USAGE') `
            -Result (Invoke-GuestFilePreview -StepName 'guest-file-unconfirmed' -HostPath $payloadPath -GuestPath $guestFilePath -Mode @())
    }
    copy = { Invoke-CopySlice }
    guest_readback = { Invoke-GuestReadbackSlice }
}

$runError = $null
try {
    foreach ($slice in $sliceActions.Keys) {
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
            if (-not (Invoke-Cleanup)) {
                throw "PCV_P1_GUESTFILE_CLEANUP_FAILED|$($summary.cleanup.error)"
            }
        }
        $cleanupOk = $true
    }
    catch {
        if ([string]$summary.cleanup.verdict -ne 'FAIL') { $summary.cleanup.verdict = 'FAIL' }
        $cleanupCode = Get-SafeFailureCode -Message $_.Exception.Message
        $runError = if ($null -eq $runError) { $cleanupCode } else { "$runError; $cleanupCode" }
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
    throw "PCV_P1_GUESTFILE_FAILED|$($summary.error)"
}
