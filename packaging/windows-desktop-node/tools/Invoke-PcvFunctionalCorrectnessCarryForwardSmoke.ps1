# Re-runs the three functional-correctness items that anchor documents have carried forward from
# 0.42.65: network QoS Kbps -> bps conversion, the disk shrink guard, and disk expansion. The
# 0.42.69 and 0.42.70 anchors both recorded them as carry-forward because no committed runner
# reproduced them; this script is that runner.
[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ('functional-correctness-carryforward-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [Parameter(Mandatory)][string]$Version,
    [string]$IsoPath = 'D:\Downloads\ubuntu-26.04-live-server-amd64.iso',
    [string]$VmRoot = (Join-Path $env:TEMP 'pcv-functional-correctness-carryforward'),
    [string]$VmName = ('pcv-fc-cf-' + ([guid]::NewGuid().ToString('N').Substring(0, 8))),
    [string]$AdapterTarget = 'adapter0',
    [int]$MaximumKbps = 2048,
    [int]$MinimumKbps = 0,
    [int]$BaseDiskGb = 10,
    [int]$ShrinkDiskGb = 9,
    [int]$ExpandDiskGb = 11,
    [int]$CpuCount = 2,
    [int]$MemoryMb = 2048,
    [int]$JobTimeoutSeconds = 300,
    [int]$CommandTimeoutSeconds = 180,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
$vmRootFull = [System.IO.Path]::GetFullPath($VmRoot)
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$steps = New-Object System.Collections.Generic.List[object]
$started = Get-Date

$summary = [ordered]@{
    schema_version = 1
    ok = $false
    scope = 'functional-correctness-carry-forward'
    dry_run = $DryRun.IsPresent
    version = $Version
    vm_name = $VmName
    qos = [ordered]@{}
    disk = [ordered]@{}
    cleanup = [ordered]@{ attempted = $false; vm_removed = $false; vm_root_removed = $false; error = $null }
    host_mutation_performed = $false
    secret_observed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    error = $null
    started_at = $started.ToString('o')
    completed_at = $null
}

function Save-Summary {
    # ToArray(), not @($steps): on this pwsh 7 / .NET 10 host the array subexpression around a
    # List[object] throws "Argument types do not match". Sibling runners use ToArray() for the same
    # field, so this matches them rather than working around it locally.
    $summary['steps'] = $steps.ToArray()
    $summary | ConvertTo-Json -Depth 24 | Set-Content -LiteralPath $summaryPath -Encoding utf8
}

function Invoke-PcvCliJson {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure
    )

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $script:PcvCliExe
    foreach ($argument in (@('--json') + $Arguments)) { $startInfo.ArgumentList.Add($argument) }
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false

    $process = [System.Diagnostics.Process]::Start($startInfo)
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($CommandTimeoutSeconds * 1000)) {
        try { $process.Kill($true) } catch { }
        throw "PCV_FC_CARRYFORWARD_COMMAND_TIMEOUT|$StepName"
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()

    $json = $null
    if (-not [string]::IsNullOrWhiteSpace($stdout)) {
        try { $json = $stdout | ConvertFrom-Json } catch { $json = $null }
    }

    $steps.Add([pscustomobject]@{
        step = $StepName
        command = 'pcvcli --json ' + ($Arguments -join ' ')
        exit_code = [int]$process.ExitCode
        ok = [bool]($process.ExitCode -eq 0)
        stderr = $stderr.Trim()
        at = (Get-Date).ToString('o')
    }) | Out-Null
    Save-Summary

    if ($process.ExitCode -ne 0 -and -not $AllowFailure) {
        throw "PCV_FC_CARRYFORWARD_COMMAND_FAILED|$StepName|exit=$($process.ExitCode)|$($stderr.Trim())"
    }

    [pscustomobject]@{ ExitCode = [int]$process.ExitCode; Json = $json; Stdout = $stdout; Stderr = $stderr }
}

# Terminal job states are reported, never thrown, because two of the three checks in this runner
# expect a job to fail: the shrink guard is a PASS only when the product refuses the request.
function Wait-PcvJobTerminal {
    param(
        [Parameter(Mandatory)][string]$JobId,
        [Parameter(Mandatory)][string]$StepName
    )

    $deadline = (Get-Date).AddSeconds($JobTimeoutSeconds)
    do {
        Start-Sleep -Seconds 2
        $result = Invoke-PcvCliJson -StepName "$StepName-job-get" -Arguments @('job', 'get', $JobId) -AllowFailure
        $status = if ($null -ne $result.Json) { [string]$result.Json.data.status } else { '' }
        if ($status -in @('succeeded', 'failed', 'canceled')) { return $result.Json.data }
    } while ((Get-Date) -lt $deadline)

    throw "PCV_FC_CARRYFORWARD_JOB_TIMEOUT|$StepName|job=$JobId"
}

function Start-PcvCliJob {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $created = Invoke-PcvCliJson -StepName $StepName -Arguments $Arguments -AllowFailure
    if ($null -eq $created.Json -or $null -eq $created.Json.data) {
        throw "PCV_FC_CARRYFORWARD_JOB_NOT_QUEUED|$StepName|$($created.Stderr.Trim())"
    }
    $jobId = [string]$created.Json.data.job_id
    if ([string]::IsNullOrWhiteSpace($jobId)) {
        throw "PCV_FC_CARRYFORWARD_JOB_ID_MISSING|$StepName"
    }
    Wait-PcvJobTerminal -JobId $jobId -StepName $StepName
}

function Get-PcvVhdxLengthBytes {
    param([Parameter(Mandatory)][string]$VmName)

    $drive = Get-VMHardDiskDrive -VMName $VmName -ErrorAction Stop | Select-Object -First 1
    if ($null -eq $drive) { throw "PCV_FC_CARRYFORWARD_VHD_NOT_FOUND|$VmName" }
    [pscustomobject]@{
        Path = $drive.Path
        SizeBytes = (Get-VHD -Path $drive.Path -ErrorAction Stop).Size
    }
}

try {
    $script:PcvCliExe = Join-Path $ProductRoot 'pcvcli.exe'
    if (-not (Test-Path -LiteralPath $script:PcvCliExe -PathType Leaf)) {
        $command = Get-Command pcvcli.exe -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -eq $command) { throw 'PCV_FC_CARRYFORWARD_CLI_NOT_FOUND' }
        $script:PcvCliExe = $command.Source
    }
    if (-not (Test-Path -LiteralPath $IsoPath -PathType Leaf)) {
        throw "PCV_FC_CARRYFORWARD_ISO_NOT_FOUND|$IsoPath"
    }

    $summary.pcvcli_path = $script:PcvCliExe
    $summary.iso_path = $IsoPath
    $summary.vm_root = $vmRootFull

    if ($DryRun.IsPresent) {
        $summary.plan = @(
            "vm create $VmName --iso $IsoPath --cpu $CpuCount --memory-mb $MemoryMb --disk-gb $BaseDiskGb",
            "vm bandwidth-set $VmName --adapter $AdapterTarget --maximum-kbps $MaximumKbps --minimum-kbps $MinimumKbps --yes",
            "vm disk-resize $VmName $ShrinkDiskGb  (expect PCV_VM_DISK_SHRINK_NOT_SUPPORTED)",
            "vm disk-resize $VmName $ExpandDiskGb  (expect succeeded)",
            "vm delete $VmName --yes"
        )
        $summary.ok = $true
        $summary.completed_at = (Get-Date).ToString('o')
        Save-Summary
        $summary | ConvertTo-Json -Depth 24
        return
    }

    New-Item -ItemType Directory -Path $vmRootFull -Force | Out-Null
    $summary.host_mutation_performed = $true

    $createJob = Start-PcvCliJob -StepName 'vm-create' -Arguments @(
        'vm', 'create', '--name', $VmName, '--iso', $IsoPath,
        '--cpu', "$CpuCount", '--memory-mb', "$MemoryMb", '--disk-gb', "$BaseDiskGb",
        '--vm-root', $vmRootFull)
    if ([string]$createJob.status -ne 'succeeded') {
        throw "PCV_FC_CARRYFORWARD_CREATE_FAILED|status=$($createJob.status)"
    }
    $summary.create_job_id = [string]$createJob.job_id

    # --- Carry-forward item 1: network QoS Kbps -> bps conversion -------------------------------
    $qosJob = Start-PcvCliJob -StepName 'vm-bandwidth-set' -Arguments @(
        'vm', 'bandwidth-set', $VmName, '--adapter', $AdapterTarget,
        '--maximum-kbps', "$MaximumKbps", '--minimum-kbps', "$MinimumKbps", '--yes')

    $adapter = Get-VMNetworkAdapter -VMName $VmName -ErrorAction Stop | Select-Object -First 1
    $observedBps = if ($null -ne $adapter -and $null -ne $adapter.BandwidthSetting) {
        [int64]$adapter.BandwidthSetting.MaximumBandwidth
    }
    else { $null }

    $summary.qos = [ordered]@{
        job_id = [string]$qosJob.job_id
        status = [string]$qosJob.status
        target = $AdapterTarget
        requested_maximum_kbps = $MaximumKbps
        requested_minimum_kbps = $MinimumKbps
        wmi_maximum_bandwidth_bps_observed = $observedBps
        expected_bps = [int64]$MaximumKbps * 1000
        conversion = "$MaximumKbps Kbps -> $([string]::Format('{0:N0}', [int64]$MaximumKbps * 1000)) bps"
        verdict = if ([string]$qosJob.status -eq 'succeeded' -and $observedBps -eq ([int64]$MaximumKbps * 1000)) { 'PASS' } else { 'FAIL' }
    }
    Save-Summary

    # --- Carry-forward items 2 and 3: disk shrink guard and expansion ---------------------------
    $before = Get-PcvVhdxLengthBytes -VmName $VmName
    $shrinkJob = Start-PcvCliJob -StepName 'vm-disk-resize-shrink' -Arguments @('vm', 'disk-resize', $VmName, "$ShrinkDiskGb")
    $afterShrink = Get-PcvVhdxLengthBytes -VmName $VmName
    $shrinkCode = if ($null -ne $shrinkJob.PSObject.Properties['error'] -and $null -ne $shrinkJob.error) {
        [string]$shrinkJob.error.code
    }
    else { '' }

    $expandJob = Start-PcvCliJob -StepName 'vm-disk-resize-expand' -Arguments @('vm', 'disk-resize', $VmName, "$ExpandDiskGb")
    $afterExpand = Get-PcvVhdxLengthBytes -VmName $VmName

    $summary.disk = [ordered]@{
        vhd_path = $before.Path
        before_bytes = $before.SizeBytes
        shrink_requested_gb = $ShrinkDiskGb
        shrink_job_id = [string]$shrinkJob.job_id
        shrink_status = [string]$shrinkJob.status
        shrink_error_code = $shrinkCode
        after_shrink_bytes = $afterShrink.SizeBytes
        shrink_guard_verdict = if ([string]$shrinkJob.status -eq 'failed' -and $afterShrink.SizeBytes -eq $before.SizeBytes) { 'PASS' } else { 'FAIL' }
        expand_requested_gb = $ExpandDiskGb
        expand_job_id = [string]$expandJob.job_id
        expand_status = [string]$expandJob.status
        after_expand_bytes = $afterExpand.SizeBytes
        expand_verdict = if ([string]$expandJob.status -eq 'succeeded' -and $afterExpand.SizeBytes -gt $before.SizeBytes) { 'PASS' } else { 'FAIL' }
    }
    Save-Summary

    $summary.ok = (
        $summary.qos.verdict -eq 'PASS' -and
        $summary.disk.shrink_guard_verdict -eq 'PASS' -and
        $summary.disk.expand_verdict -eq 'PASS')
}
catch {
    $summary.error = $_.Exception.Message
}
finally {
    if (-not $DryRun.IsPresent) {
        $summary.cleanup.attempted = $true
        try {
            if (Get-VM -Name $VmName -ErrorAction SilentlyContinue) {
                Invoke-PcvCliJson -StepName 'vm-delete' -Arguments @('vm', 'delete', $VmName, '--yes') -AllowFailure | Out-Null
                Start-Sleep -Seconds 5
            }
            if (Get-VM -Name $VmName -ErrorAction SilentlyContinue) {
                Stop-VM -Name $VmName -TurnOff -Force -ErrorAction SilentlyContinue
                Remove-VM -Name $VmName -Force -ErrorAction SilentlyContinue
            }
            $summary.cleanup.vm_removed = -not [bool](Get-VM -Name $VmName -ErrorAction SilentlyContinue)

            $vmFolder = Join-Path $vmRootFull $VmName
            if (Test-Path -LiteralPath $vmFolder) { Remove-Item -LiteralPath $vmFolder -Recurse -Force -ErrorAction SilentlyContinue }
            $summary.cleanup.vm_root_removed = -not (Test-Path -LiteralPath $vmFolder)
        }
        catch {
            $summary.cleanup.error = $_.Exception.Message
        }
    }

    $summary.completed_at = (Get-Date).ToString('o')
    if ($null -ne $summary.error) { $summary.ok = $false }
    Save-Summary
}

$summary | ConvertTo-Json -Depth 24
if (-not $summary.ok) { exit 1 }
