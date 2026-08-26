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

    [switch]$DryRun
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

function Write-AtomicSummary {
    try {
        if ([bool]$summary.secret_observed) {
            throw 'PCV_P0_SECRET_OBSERVED'
        }
        $summary.steps = $script:Steps.ToArray()
        $json = $summary | ConvertTo-Json -Depth 32
        if ($json -match '(?i)authorization\s*[:=]\s*bearer\s+\S+') {
            $summary.secret_observed = $true
            throw 'PCV_P0_SECRET_OBSERVED'
        }
        [System.IO.File]::WriteAllText(
            $summaryTempPath,
            $json,
            [System.Text.UTF8Encoding]::new($false))
        Move-Item -LiteralPath $summaryTempPath -Destination $summaryPath -Force
    }
    catch {
        try {
            if (Test-Path -LiteralPath $summaryTempPath -PathType Leaf) {
                Remove-Item -LiteralPath $summaryTempPath -Force
            }
        }
        catch { }
        if ($_.Exception.Message -like 'PCV_P0_SECRET_OBSERVED*') {
            throw
        }
        throw "PCV_P0_SUMMARY_WRITE_FAILED|$($_.Exception.Message)"
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

function Assert-InstalledProduct {
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
    $service = Get-Service -Name 'PureCVisorDesktopNode' -ErrorAction SilentlyContinue
    if ($null -eq $service -or [string]$service.Status -ne 'Running') {
        throw 'PCV_P0_SERVICE_LOST'
    }
}

function Assert-VmAbsent {
    param([Parameter(Mandatory)][string]$Name)

    if ($null -ne (Get-VM -Name $Name -ErrorAction SilentlyContinue)) {
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
    $payload = $null
    if (-not [string]::IsNullOrWhiteSpace($stdout)) {
        try { $payload = $stdout | ConvertFrom-Json -Depth 64 } catch { }
    }
    $script:Steps.Add([pscustomobject][ordered]@{
        step = $StepName
        exit_code = [int]$process.ExitCode
        status = if ($process.ExitCode -eq 0) { 'completed' } else { 'failed' }
        at = (Get-Date).ToUniversalTime().ToString('o')
    }) | Out-Null
    Write-AtomicSummary
    Assert-ServiceAvailable
    if ($process.ExitCode -ne 0 -and -not $AllowFailure.IsPresent) {
        throw "PCV_P0_COMMAND_FAILED|$StepName|exit=$($process.ExitCode)|$($stderr.Trim())"
    }
    return [pscustomobject]@{
        ExitCode = [int]$process.ExitCode
        Json = $payload
        Stderr = $stderr
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
        [switch]$AllowFailure
    )

    $created = Invoke-PcvCliJson -StepName $StepName -Arguments $Arguments -AllowFailure:$AllowFailure
    $data = Get-ObjectPropertyValue -InputObject $created.Json -Name 'data'
    $jobId = [string](Get-ObjectPropertyValue -InputObject $data -Name 'job_id')
    if ([string]::IsNullOrWhiteSpace($jobId)) {
        throw "PCV_P0_JOB_ID_MISSING|$StepName"
    }
    $job = Wait-PcvJobTerminal -JobId $jobId -StepName $StepName
    $status = [string](Get-ObjectPropertyValue -InputObject $job -Name 'status')
    $errorObject = Get-ObjectPropertyValue -InputObject $job -Name 'error'
    $summary.queued_jobs[$StepName] = [ordered]@{
        job_id = $jobId
        status = $status
        error_code = [string](Get-ObjectPropertyValue -InputObject $errorObject -Name 'code')
    }
    Write-AtomicSummary
    if ($status -ne 'succeeded' -and -not $AllowFailure.IsPresent) {
        throw "PCV_P0_JOB_FAILED|$StepName|job=$JobId|status=$status"
    }
    return $job
}

function Wait-HyperVState {
    param(
        [Parameter(Mandatory)][Guid]$Id,
        [Parameter(Mandatory)][string]$Expected,
        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $vm = Get-VM -Id $Id -ErrorAction SilentlyContinue
        if ($null -ne $vm -and [string]$vm.State -eq $Expected) {
            return [string]$vm.State
        }
        Start-Sleep -Seconds 2
    } while ((Get-Date) -lt $deadline)
    $final = Get-VM -Id $Id -ErrorAction SilentlyContinue
    return if ($null -eq $final) { $null } else { [string]$final.State }
}

function Get-ProductVmState {
    param([Parameter(Mandatory)][string]$Id)

    $result = Invoke-PcvCliJson -StepName 'vm-get-state' -Arguments @('vm', 'get', $Id)
    $data = Get-ObjectPropertyValue -InputObject $result.Json -Name 'data'
    $state = Get-ObjectPropertyValue -InputObject $data -Name 'state'
    if ($null -eq $state) {
        $state = Get-ObjectPropertyValue -InputObject $data -Name 'power_state'
    }
    return ([string]$state).ToLowerInvariant()
}

function Register-VmRecord {
    param(
        [Parameter(Mandatory)][string]$Kind,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)]$Vm
    )

    $recordedRoot = Assert-ValidatedChildPath -Root $vmRootFull -Candidate ([string]$Vm.Path)
    $record = [pscustomobject][ordered]@{
        kind = $Kind
        name = $Name
        id = ([Guid]$Vm.Id).ToString('D')
        root = $recordedRoot
        product_delete_attempted = $false
        native_fallback_used = $false
        removed = $false
        root_removed = $false
        same_name_different_id_blocked = $false
        error = $null
    }
    $script:VmRecords.Add($record) | Out-Null
    if ($Kind -eq 'managed') {
        $summary.managed_vm_id = $record.id
    }
    else {
        $summary.foreign_vm_id = $record.id
    }
    Write-AtomicSummary
    return $record
}

function Assert-SlicePassed {
    param([Parameter(Mandatory)][string]$Slice)

    if ([string]$summary.slice_verdicts[$Slice] -ne 'PASS') {
        throw "PCV_P0_SLICE_FAILED|$Slice"
    }
}

function Invoke-SavedLifecycle {
    $create = Start-PcvCliJob -StepName 'vm-create' -Arguments @(
        'vm', 'create', '--name', $ManagedVm, '--iso', $summary.iso_path_resolved,
        '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '8', '--vm-root', $vmRootFull)
    if ([string](Get-ObjectPropertyValue -InputObject $create -Name 'status') -ne 'succeeded') {
        throw 'PCV_P0_STATE_MISMATCH|create'
    }
    $createdRows = @(Get-VM -Name $ManagedVm -ErrorAction SilentlyContinue)
    if ($createdRows.Count -ne 1) {
        throw "PCV_P0_STATE_MISMATCH|created-vm-cardinality=$($createdRows.Count)"
    }
    $record = Register-VmRecord -Kind 'managed' -Name $ManagedVm -Vm $createdRows[0]
    $id = [Guid]$record.id

    $start = Start-PcvCliJob -StepName 'vm-start' -Arguments @('vm', 'start', $record.id)
    $running = Wait-HyperVState -Id $id -Expected 'Running'
    if ([string](Get-ObjectPropertyValue -InputObject $start -Name 'status') -ne 'succeeded' -or $running -ne 'Running') {
        throw "PCV_P0_STATE_MISMATCH|start|hyperv=$running"
    }

    $save = Start-PcvCliJob -StepName 'vm-save' -Arguments @('vm', 'save', $record.id)
    $hypervSaved = Wait-HyperVState -Id $id -Expected 'Saved'
    $productSaved = Get-ProductVmState -Id $record.id
    $summary.hyperv_state_after_save = $hypervSaved
    $summary.product_state_after_save = $productSaved
    $summary.readbacks.saved_not_paused = ($hypervSaved -ne 'Paused')
    Write-AtomicSummary
    if ([string](Get-ObjectPropertyValue -InputObject $save -Name 'status') -ne 'succeeded' -or
        $hypervSaved -ne 'Saved' -or $hypervSaved -eq 'Paused' -or $productSaved -ne 'saved') {
        throw "PCV_P0_STATE_MISMATCH|save|hyperv=$hypervSaved|product=$productSaved"
    }

    $resume = Start-PcvCliJob -StepName 'vm-resume-saved' -Arguments @('vm', 'resume-saved', $record.id)
    $hypervRunning = Wait-HyperVState -Id $id -Expected 'Running'
    $productRunning = Get-ProductVmState -Id $record.id
    $summary.hyperv_state_after_resume = $hypervRunning
    $summary.product_state_after_resume = $productRunning
    if ([string](Get-ObjectPropertyValue -InputObject $resume -Name 'status') -ne 'succeeded' -or
        $hypervRunning -ne 'Running') {
        throw "PCV_P0_STATE_MISMATCH|resume|hyperv=$hypervRunning|product=$productRunning"
    }
    $summary.slice_verdicts.saved_lifecycle = 'PASS'
    Write-AtomicSummary
}

function Invoke-MediaAttachSlice {
    $record = $script:VmRecords | Where-Object kind -eq 'managed' | Select-Object -First 1
    $job = Start-PcvCliJob -StepName 'vm-attach' -Arguments @('vm', 'attach', $record.id, '--iso', $summary.iso_path_resolved)
    $dvd = Get-VMDvdDrive -VMId ([Guid]$record.id) -ErrorAction Stop | Select-Object -First 1
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
    $summary.slice_verdicts.media_attach = 'PASS'
    Write-AtomicSummary
}

function Invoke-CheckpointRestoreSlice {
    $record = $script:VmRecords | Where-Object kind -eq 'managed' | Select-Object -First 1
    Start-PcvCliJob -StepName 'checkpoint-create' -Arguments @(
        'vm', 'checkpoint', 'create', $record.id, '--name', $CheckpointName) | Out-Null
    $restore = Start-PcvCliJob -StepName 'checkpoint-restore' -Arguments @(
        'vm', 'checkpoint', 'restore', $record.id, $CheckpointName)
    $listed = Invoke-PcvCliJson -StepName 'checkpoint-list-after-restore' -Arguments @(
        'vm', 'checkpoint', 'list', $record.id)
    $rows = @((Get-ObjectPropertyValue -InputObject $listed.Json -Name 'data'))
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
    $summary.slice_verdicts.checkpoint_restore = 'PASS'
    Write-AtomicSummary
}

function Invoke-ManagedImportSlice {
    $foreignRoot = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $vmRootFull $ForeignVm)
    New-Item -ItemType Directory -Path $foreignRoot -Force | Out-Null
    $vhdPath = Assert-ValidatedChildPath -Root $vmRootFull -Candidate (Join-Path $foreignRoot 'disk0.vhdx')
    New-VHD -Path $vhdPath -SizeBytes 1GB -Dynamic | Out-Null
    New-VM -Name $ForeignVm -Generation 2 -MemoryStartupBytes 512MB -VHDPath $vhdPath -Path $foreignRoot | Out-Null
    $foreignRows = @(Get-VM -Name $ForeignVm -ErrorAction SilentlyContinue)
    if ($foreignRows.Count -ne 1) {
        throw "PCV_P0_STATE_MISMATCH|foreign-vm-cardinality=$($foreignRows.Count)"
    }
    $record = Register-VmRecord -Kind 'foreign' -Name $ForeignVm -Vm $foreignRows[0]

    $rejected = Start-PcvCliJob -StepName 'unmanaged-delete' -Arguments @(
        'vm', 'delete', $record.id, '--yes') -AllowFailure
    $rejectStatus = [string](Get-ObjectPropertyValue -InputObject $rejected -Name 'status')
    $rejectError = Get-ObjectPropertyValue -InputObject $rejected -Name 'error'
    $rejectCode = [string](Get-ObjectPropertyValue -InputObject $rejectError -Name 'code')
    $stillPresent = $null -ne (Get-VM -Id ([Guid]$record.id) -ErrorAction SilentlyContinue)
    if ($rejectStatus -ne 'failed' -or $rejectCode -ne 'PCV_VM_NOT_MANAGED_BY_PURECVISOR' -or -not $stillPresent) {
        throw "PCV_P0_STATE_MISMATCH|unmanaged-delete|status=$rejectStatus|code=$rejectCode"
    }

    $managed = Start-PcvCliJob -StepName 'vm-manage' -Arguments @('vm', 'manage', $record.id, '--yes')
    $managedVm = Get-VM -Id ([Guid]$record.id) -ErrorAction Stop
    $markerPresent = [string]$managedVm.Notes -match 'managed-by=purecvisor-desktop-node'
    $deleted = Start-PcvCliJob -StepName 'managed-delete' -Arguments @('vm', 'delete', $record.id, '--yes')
    $gone = $null -eq (Get-VM -Id ([Guid]$record.id) -ErrorAction SilentlyContinue)
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
    $summary.slice_verdicts.managed_import = 'PASS'
    Write-AtomicSummary
}

function Invoke-ExactCleanup {
    $summary.cleanup.attempted = $true
    $cleanupErrors = [System.Collections.Generic.List[string]]::new()
    foreach ($record in $script:VmRecords) {
        try {
            $recordedId = [Guid]$record.id
            $record.root = Assert-ValidatedChildPath -Root $vmRootFull -Candidate $record.root
            $sameName = @(Get-VM -Name $record.name -ErrorAction SilentlyContinue)
            $different = @($sameName | Where-Object { [Guid]$_.Id -ne $recordedId })
            if ($different.Count -gt 0) {
                $record.same_name_different_id_blocked = $true
                $summary.cleanup.same_name_different_id_blocked = $true
                throw "PCV_P0_CLEANUP_ID_MISMATCH|name=$($record.name)|recorded=$recordedId"
            }

            $current = Get-VM -Id $recordedId -ErrorAction SilentlyContinue
            if ($null -ne $current) {
                $record.product_delete_attempted = $true
                try {
                    $delete = Start-PcvCliJob -StepName "cleanup-delete-$($record.kind)" -Arguments @(
                        'vm', 'delete', $record.id, '--yes') -AllowFailure
                    if ([string](Get-ObjectPropertyValue -InputObject $delete -Name 'status') -ne 'succeeded') {
                        throw 'product-delete-failed'
                    }
                }
                catch { }
                $current = Get-VM -Id $recordedId -ErrorAction SilentlyContinue
                if ($null -ne $current) {
                    if ([Guid]$current.Id -ne $recordedId) {
                        throw "PCV_P0_CLEANUP_ID_MISMATCH|recorded=$recordedId|actual=$($current.Id)"
                    }
                    $record.native_fallback_used = $true
                    $summary.cleanup.native_fallback_used = $true
                    if ([string]$current.State -ne 'Off') {
                        Stop-VM -VM $current -TurnOff -Force -ErrorAction Stop
                    }
                    $current = Get-VM -Id $recordedId -ErrorAction Stop
                    Remove-VM -VM $current -Force -ErrorAction Stop
                }
            }
            $record.removed = $null -eq (Get-VM -Id $recordedId -ErrorAction SilentlyContinue)
            if (-not $record.removed) {
                throw "PCV_P0_CLEANUP_ID_MISMATCH|remaining=$recordedId"
            }
            if (Test-Path -LiteralPath $record.root) {
                [System.IO.Directory]::Delete($record.root, $true)
            }
            $record.root_removed = -not (Test-Path -LiteralPath $record.root)
            if (-not $record.root_removed) {
                throw "PCV_P0_CLEANUP_ROOT_INVALID|remaining=$($record.root)"
            }
        }
        catch {
            $record.error = $_.Exception.Message
            $cleanupErrors.Add($_.Exception.Message) | Out-Null
        }
    }
    $summary.cleanup.records = @($script:VmRecords)
    if ($cleanupErrors.Count -gt 0) {
        $summary.cleanup.verdict = 'FAIL'
        $summary.cleanup.error = $cleanupErrors -join '; '
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
    Write-AtomicSummary

    $summary.host_mutation_performed = $true
    $summary.actual_execution = 'installed-cli-and-hyperv'
    New-Item -ItemType Directory -Path $vmRootFull -Force | Out-Null
    Invoke-SavedLifecycle
    Assert-SlicePassed -Slice 'saved_lifecycle'

    if ($Mode -eq 'Full') {
        Invoke-MediaAttachSlice
        Assert-SlicePassed -Slice 'media_attach'
        Invoke-CheckpointRestoreSlice
        Assert-SlicePassed -Slice 'checkpoint_restore'
        Invoke-ManagedImportSlice
        Assert-SlicePassed -Slice 'managed_import'
    }
}
catch {
    $runError = $_.Exception.Message
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
        $summary.cleanup.error = $_.Exception.Message
        $runError = if ($null -eq $runError) { $_.Exception.Message } else { "$runError; $($_.Exception.Message)" }
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
