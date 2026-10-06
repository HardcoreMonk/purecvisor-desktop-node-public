# Lane 2 probe: conditional reconcile of interrupted jobs per family (SERVICE_PLAN P1-10). Each family's mutation is queued,
# the service process is killed while the job runs, and job reconcile must agree with an independent readback.
# Host mutation: a probe VM is created, started, restarted, shut down, powered off, QoS-limited and deleted, and the
# PureCVisor service process is killed and started again.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$EvidenceId,
    [Parameter(Mandatory)][string]$IsoPath,
    [string]$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..')),
    [string]$ProbePrefix = 'pcv-probe-lane2-',
    [string]$PreservedVm = 'pcv-guest-installed-04253-r1',
    [string]$VmRoot = 'D:\PureCVisor\VMs',
    [string]$CliPath = 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe',
    [switch]$PlanOnly
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'PcvLane2ProbeCommon.ps1')

$root = Resolve-PcvLane2ProbePath $RepoRoot $ArtifactRoot
$iso = Resolve-PcvLane2ProbePath $RepoRoot $IsoPath
$vm = "${ProbePrefix}rc"
$plan = [ordered]@{
    probe = 'family-reconcile'
    service_plan_items = @('P1-10')
    evidence_id = $EvidenceId
    artifact_root = $root
    iso = $iso
    probe_vms = @($vm)
    preserved_vm = $PreservedVm
    families = @('vm.create', 'vm.restart', 'vm.shutdown', 'vm.qos.storage.set', 'vm.qos.network.set')
    host_mutation_scope = 'probe VM create/start/restart/shutdown/poweroff/QoS/delete and service process kill plus restart'
}
if ($PlanOnly) {
    Write-PcvLane2ProbePlan $plan
    return
}

Initialize-PcvLane2Probe -ArtifactRoot $root -CliPath $CliPath
$results = [System.Collections.Generic.List[object]]::new()

function Get-PcvProbeRows([string]$Name) { return @(Get-PcvProbeVmRows | Where-Object { (Get-PcvField $_ 'name') -eq $Name }) }

function Wait-PcvProbeDone([string]$Id) {
    if (-not $Id) { return $null }
    $deadline = (Get-Date).AddMinutes(4)
    do {
        Start-Sleep -Milliseconds 700
        $job = Get-PcvProbeJob $Id
    } while ($job -and (Get-PcvField $job 'status') -in @('queued', 'running') -and (Get-Date) -lt $deadline)
    return $job
}

function Restart-PcvProbeHost {
    Stop-Process -Name 'DesktopNode.Host' -Force -ErrorAction SilentlyContinue
    $deadline = (Get-Date).AddSeconds(90)
    do {
        Start-Sleep -Seconds 2
        if ((Get-Service PureCVisorDesktopNode).Status -eq 'Stopped') { Start-Service PureCVisorDesktopNode }
        $ok = $false
        try { $ok = (Invoke-WebRequest http://127.0.0.1/ -UseBasicParsing -TimeoutSec 5).StatusCode -eq 200 } catch { }
        if ($ok) { $ok = (& $script:PcvProbeCli --json job list --limit 1 2>&1 | Out-String) -match '"ok":\s*true' }
    } while (-not $ok -and (Get-Date) -lt $deadline)
    return $ok
}

# Queue the mutation, kill the host while the job is running, restart, and return the interrupted job id.
function Invoke-PcvInterruptedMutation([string]$Label, [string[]]$CliArgs, [scriptblock]$Reset) {
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        $queued = Invoke-PcvProbeCli "$Label-queue-$attempt" $CliArgs
        $id = Get-PcvProbeJobId $queued
        if (-not $id) { return [pscustomobject]@{ outcome = 'not-queued'; job_id = $null; attempts = $attempt; error = (Get-PcvProbeError $queued) } }
        $sawRunning = $false
        $deadline = (Get-Date).AddSeconds(60)
        do {
            $job = Get-PcvProbeJob $id
            if ($job -and (Get-PcvField $job 'status') -eq 'running') { $sawRunning = $true; break }
            if ($job -and (Get-PcvField $job 'status') -notin @('queued', 'running')) { break }
            Start-Sleep -Milliseconds 50
        } while ((Get-Date) -lt $deadline)
        if ($sawRunning) {
            $up = Restart-PcvProbeHost
            $after = Get-PcvProbeJob $id
            if ($after -and (Get-PcvField $after 'status') -eq 'failed' -and (Get-PcvField (Get-PcvField $after 'error') 'code') -eq 'PCV_JOB_INTERRUPTED') {
                return [pscustomobject]@{ outcome = 'interrupted'; job_id = $id; attempts = $attempt; host_back = $up }
            }
            if (-not $up) { return [pscustomobject]@{ outcome = 'host-not-back'; job_id = $id; attempts = $attempt } }
        }
        $done = Wait-PcvProbeDone $id
        [System.IO.File]::WriteAllText((Join-Path $root "$Label-attempt-$attempt-finished.txt"), "status=$(Get-PcvField $done 'status')")
        & $Reset
    }
    return [pscustomobject]@{ outcome = 'finished-before-interrupt'; job_id = $null; attempts = 3 }
}

function Add-PcvReconcileJudgement([string]$Family, $Interrupt, [scriptblock]$Applied) {
    if ($Interrupt.outcome -ne 'interrupted') {
        $results.Add([ordered]@{ family = $Family; outcome = $Interrupt.outcome; attempts = $Interrupt.attempts; verdict = 'not-measured' })
        return
    }
    $actual = & $Applied
    $reconcile = Invoke-PcvProbeCli "$Family-reconcile" @('job', 'reconcile', $Interrupt.job_id)
    $ok = [bool](Get-PcvField $reconcile.json 'ok')
    $final = Get-PcvProbeJob $Interrupt.job_id
    $consistent = ($actual.applied -and $ok) -or ((-not $actual.applied) -and (-not $ok))
    $results.Add([ordered]@{
        family = $Family; outcome = 'interrupted'; attempts = $Interrupt.attempts; job_id = $Interrupt.job_id; actual = $actual
        reconcile_ok = $ok; reconcile_error = (Get-PcvProbeError $reconcile); job_status_after = (Get-PcvField $final 'status')
        verdict = $(if ($consistent) { 'pass' } else { 'fail' })
    })
}

$keepBefore = (Get-PcvProbeRows $PreservedVm) | Select-Object -First 1

# vm.create
$createArgs = @('vm', 'create', '--name', $vm, '--iso', $iso, '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '8')
$resetCreate = {
    foreach ($row in @(Get-PcvProbeRows $vm)) {
        if (Get-PcvField $row 'managed_by_purecvisor') { $null = Wait-PcvProbeDone (Get-PcvProbeJobId (Invoke-PcvProbeCli 'reset-delete' @('vm', 'delete', $vm, '--yes'))) }
    }
    if (-not (Get-PcvProbeRows $vm)) { return }
    Get-VM -Name $vm -ErrorAction SilentlyContinue | Remove-VM -Force
}
$ci = Invoke-PcvInterruptedMutation 'create' $createArgs $resetCreate
Add-PcvReconcileJudgement 'vm.create' $ci {
    $rows = @(Get-PcvProbeRows $vm)
    [ordered]@{ rows = $rows.Count; managed = @($rows | Where-Object { Get-PcvField $_ 'managed_by_purecvisor' }).Count; applied = ($rows.Count -eq 1 -and [bool](Get-PcvField $rows[0] 'managed_by_purecvisor')) }
}
# An interrupted create can leave disk0.vhdx behind (design pcv-interrupted-create-residue-v1); record and remove it.
$orphanDir = Join-Path $VmRoot $vm
if (@(Get-PcvProbeRows $vm).Count -eq 0 -and (Test-Path -LiteralPath $orphanDir)) {
    $orphanFiles = @(Get-ChildItem -LiteralPath $orphanDir -Recurse -File | ForEach-Object { "$($_.Name) $($_.Length)" })
    Remove-Item -LiteralPath $orphanDir -Recurse -Force
    $results[$results.Count - 1]['orphan_files_removed_after_not_applied'] = $orphanFiles
}
if (-not (Get-PcvProbeRows $vm)) { $null = Wait-PcvProbeDone (Get-PcvProbeJobId (Invoke-PcvProbeCli 'ensure-create' $createArgs)) }
if (-not (@(Get-PcvProbeRows $vm) | Where-Object { Get-PcvField $_ 'managed_by_purecvisor' })) { throw 'probe VM not available as managed; stop before power families' }

# vm.restart (needs Running)
$null = Wait-PcvProbeDone (Get-PcvProbeJobId (Invoke-PcvProbeCli 'start-for-restart' @('vm', 'start', $vm)))
$baseline = Get-PcvField ((@(Get-PcvProbeRows $vm))[0]) 'last_powered_on'
$ri = Invoke-PcvInterruptedMutation 'restart' @('vm', 'restart', $vm) { Start-Sleep -Seconds 1 }
Add-PcvReconcileJudgement 'vm.restart' $ri {
    $row = (@(Get-PcvProbeRows $vm))[0]
    $lastPoweredOn = Get-PcvField $row 'last_powered_on'
    [ordered]@{ state = (Get-PcvField $row 'state'); baseline = $baseline; last_powered_on = $lastPoweredOn; applied = ((Get-PcvField $row 'state') -eq 'running' -and $lastPoweredOn -and $lastPoweredOn -ne $baseline) }
}

# vm.shutdown (guest shutdown; the probe VM has no guest OS)
if ((Get-PcvField ((@(Get-PcvProbeRows $vm))[0]) 'state') -ne 'running') { $null = Wait-PcvProbeDone (Get-PcvProbeJobId (Invoke-PcvProbeCli 'start-for-shutdown' @('vm', 'start', $vm))) }
$si = Invoke-PcvInterruptedMutation 'shutdown' @('vm', 'shutdown', $vm) {
    if ((Get-PcvField ((@(Get-PcvProbeRows $vm))[0]) 'state') -ne 'running') { $null = Wait-PcvProbeDone (Get-PcvProbeJobId (Invoke-PcvProbeCli 'restart-for-shutdown' @('vm', 'start', $vm))) }
}
Add-PcvReconcileJudgement 'vm.shutdown' $si {
    $row = (@(Get-PcvProbeRows $vm))[0]
    [ordered]@{ state = (Get-PcvField $row 'state'); applied = ((Get-PcvField $row 'state') -in @('stopped', 'off')) }
}
$null = Wait-PcvProbeDone (Get-PcvProbeJobId (Invoke-PcvProbeCli 'poweroff-for-qos' @('vm', 'poweroff', $vm)))

function Get-PcvReadbackExcerpt([string]$Text) { $flat = $Text -replace '\s+', ' '; return $flat.Substring(0, [Math]::Min(400, $flat.Length)) }

# QoS storage
$blk = Invoke-PcvProbeCli 'blkio-get' @('vm', 'blkio-get', $vm)
$disk = @(Get-PcvField $blk.json 'data') | ForEach-Object { if (Get-PcvField $_ 'disks') { $_.disks } else { $_ } } | Select-Object -First 1
$diskId = foreach ($name in 'id', 'disk', 'path', 'name') { $value = Get-PcvField $disk $name; if ($value) { [string]$value; break } }
if ($diskId) {
    $iops = 300
    $qi = Invoke-PcvInterruptedMutation 'qos-storage' @('vm', 'blkio-set', $vm, '--disk', $diskId, '--maximum-iops', "$iops", '--yes') { $script:iops += 100 }
    Add-PcvReconcileJudgement 'vm.qos.storage.set' $qi {
        $after = Invoke-PcvProbeCli 'blkio-get-after' @('vm', 'blkio-get', $vm)
        [ordered]@{ readback = (Get-PcvReadbackExcerpt $after.text); applied = ($after.text -match "`"maximum_iops`"\s*:\s*$iops\b") }
    }
} else {
    $results.Add([ordered]@{ family = 'vm.qos.storage.set'; outcome = 'no-disk-readback'; verdict = 'not-measured' })
}

# QoS network
$bandwidth = Invoke-PcvProbeCli 'bandwidth-get' @('vm', 'bandwidth', $vm)
$adapter = @(Get-PcvField $bandwidth.json 'data') | ForEach-Object { if (Get-PcvField $_ 'adapters') { $_.adapters } else { $_ } } | Select-Object -First 1
$adapterId = foreach ($name in 'id', 'adapter', 'name') { $value = Get-PcvField $adapter $name; if ($value) { [string]$value; break } }
if ($adapterId) {
    $kbps = 20000
    $ni = Invoke-PcvInterruptedMutation 'qos-network' @('vm', 'bandwidth-set', $vm, '--adapter', $adapterId, '--maximum-kbps', "$kbps", '--yes') { $script:kbps += 5000 }
    Add-PcvReconcileJudgement 'vm.qos.network.set' $ni {
        $after = Invoke-PcvProbeCli 'bandwidth-get-after' @('vm', 'bandwidth', $vm)
        [ordered]@{ readback = (Get-PcvReadbackExcerpt $after.text); applied = ($after.text -match "`"maximum_kbps`"\s*:\s*$kbps\b") }
    }
} else {
    $results.Add([ordered]@{ family = 'vm.qos.network.set'; outcome = 'no-adapter-readback'; verdict = 'not-measured' })
}

$probeLeft = Remove-PcvProbeVms -Prefix $ProbePrefix -VmRoot $VmRoot
$keepAfter = (Get-PcvProbeRows $PreservedVm) | Select-Object -First 1
$serviceState = Get-PcvProbeServiceState

Write-PcvProbeSummary ([ordered]@{
    schema_version = 1
    probe = $plan.probe
    evidence_id = $EvidenceId
    installed_version = (Get-Item (Join-Path (Split-Path $CliPath) 'DesktopNode.Host.exe')).VersionInfo.ProductVersion
    families = $results
    pass = -not ($results | Where-Object { $_.verdict -eq 'fail' })
    measured = @($results | Where-Object { $_.verdict -eq 'pass' }).Count
    not_measured = @($results | Where-Object { $_.verdict -eq 'not-measured' }).Count
    end_state = [ordered]@{ probe_vms = $probeLeft; preserved_vm = (Get-PcvField $keepAfter 'state'); preserved_vm_before = (Get-PcvField $keepBefore 'state'); service = $serviceState.service; web = $serviceState.web }
    host_mutation_performed = $true
    host_mutation_scope = $plan.host_mutation_scope
})
