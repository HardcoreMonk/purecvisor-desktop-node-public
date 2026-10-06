# Lane 2 probe: VM inventory timestamps and notes (SERVICE_PLAN P1-6) and template lock (P1-7) on the installed product.
# Host mutation: one or two probe VMs are created, started, powered off, locked, unlocked, renamed and deleted.
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
$vm = "${ProbePrefix}inv"
$vm2 = "${ProbePrefix}inv2"
$plan = [ordered]@{
    probe = 'inventory-template-lock'
    service_plan_items = @('P1-6', 'P1-7')
    evidence_id = $EvidenceId
    artifact_root = $root
    iso = $iso
    probe_vms = @($vm, $vm2)
    preserved_vm = $PreservedVm
    steps = @('preserved-vm-before', 'preserved-vm-notes-readback', 'create', 'inventory-off', 'inventory-running',
        'inventory-off-again', 'lock', 'locked-rename', 'locked-set-memory', 'locked-start', 'locked-poweroff', 'unlock',
        'unlocked-mutations', 'cleanup', 'service')
    host_mutation_scope = 'probe VM create, start, poweroff, template lock/unlock, rename, delete'
}
if ($PlanOnly) {
    Write-PcvLane2ProbePlan $plan
    return
}

Initialize-PcvLane2Probe -ArtifactRoot $root -CliPath $CliPath

$keep = Get-PcvProbeVm $PreservedVm
$keepNotes = [string](Get-PcvField $keep 'notes')
Add-PcvProbeStep 'preserved-vm-before' 'preserved VM present' ([ordered]@{ state = (Get-PcvField $keep 'state') }) ($null -ne $keep)
Add-PcvProbeStep 'preserved-vm-notes-readback' 'operator notes without managed marker' `
    ([ordered]@{ notes_has_marker = ($keepNotes -match 'managed-by='); notes_present = -not [string]::IsNullOrEmpty($keepNotes) }) `
    ((-not ($keepNotes -match 'managed-by=')) -and -not [string]::IsNullOrEmpty($keepNotes))

$c = Wait-PcvProbeJob 'create' (Invoke-PcvProbeCli 'vm-create' @('vm', 'create', '--name', $vm, '--iso', $iso, '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '8'))
Add-PcvProbeStep 'create' 'job succeeded' $c ($c.status -eq 'succeeded')
$v = Get-PcvProbeVm $vm
Add-PcvProbeStep 'inventory-off' 'created_at present, last_powered_on absent, notes absent, managed' `
    ([ordered]@{ created_at = (Get-PcvField $v 'created_at'); last_powered_on = (Get-PcvField $v 'last_powered_on'); notes = (Get-PcvField $v 'notes'); managed = (Get-PcvField $v 'managed_by_purecvisor'); state = (Get-PcvField $v 'state') }) `
    (($null -ne (Get-PcvField $v 'created_at')) -and ($null -eq (Get-PcvField $v 'last_powered_on')) -and [string]::IsNullOrEmpty([string](Get-PcvField $v 'notes')) -and [bool](Get-PcvField $v 'managed_by_purecvisor'))
$created = Get-PcvField $v 'created_at'

$s = Wait-PcvProbeJob 'start' (Invoke-PcvProbeCli 'vm-start' @('vm', 'start', $vm))
$v = Get-PcvProbeVm $vm
Add-PcvProbeStep 'inventory-running' 'start succeeded, last_powered_on present, created_at unchanged' `
    ([ordered]@{ job = $s.status; state = (Get-PcvField $v 'state'); last_powered_on = (Get-PcvField $v 'last_powered_on'); created_at = (Get-PcvField $v 'created_at') }) `
    (($s.status -eq 'succeeded') -and ($null -ne (Get-PcvField $v 'last_powered_on')) -and ((Get-PcvField $v 'created_at') -eq $created))

$p = Wait-PcvProbeJob 'poweroff' (Invoke-PcvProbeCli 'vm-poweroff' @('vm', 'poweroff', $vm))
$v = Get-PcvProbeVm $vm
Add-PcvProbeStep 'inventory-off-again' 'poweroff succeeded, last_powered_on absent while off' `
    ([ordered]@{ job = $p.status; state = (Get-PcvField $v 'state'); last_powered_on = (Get-PcvField $v 'last_powered_on') }) `
    (($p.status -eq 'succeeded') -and ($null -eq (Get-PcvField $v 'last_powered_on')))

$l = Wait-PcvProbeJob 'lock' (Invoke-PcvProbeCli 'vm-template-lock' @('vm', 'template-lock', $vm, '--yes'))
$v = Get-PcvProbeVm $vm
Add-PcvProbeStep 'lock' 'lock succeeded, template_lock true, notes still without marker' `
    ([ordered]@{ job = $l.status; template_lock = (Get-PcvField $v 'template_lock'); notes = (Get-PcvField $v 'notes') }) `
    (($l.status -eq 'succeeded') -and ((Get-PcvField $v 'template_lock') -eq $true) -and [string]::IsNullOrEmpty([string](Get-PcvField $v 'notes')))

foreach ($deny in @(@('rename', @('vm', 'rename', $vm, $vm2)), @('set-memory', @('vm', 'set-memory', $vm, '2048')))) {
    $r = Invoke-PcvProbeCli ('locked-' + $deny[0]) $deny[1]
    $w = Wait-PcvProbeJob ('locked-' + $deny[0]) $r
    $text = "$($w.error) $(Get-PcvProbeError $r)"
    Add-PcvProbeStep ('locked-' + $deny[0]) 'rejected with PCV_VM_TEMPLATE_LOCKED' ([ordered]@{ queued_status = $w.status; error = $text.Trim() }) ($text -match 'PCV_VM_TEMPLATE_LOCKED')
}

$s2 = Wait-PcvProbeJob 'locked-start' (Invoke-PcvProbeCli 'vm-start-locked' @('vm', 'start', $vm))
Add-PcvProbeStep 'locked-start' 'start allowed on template' ([ordered]@{ job = $s2.status }) ($s2.status -eq 'succeeded')
$r = Invoke-PcvProbeCli 'locked-poweroff' @('vm', 'poweroff', $vm)
$w = Wait-PcvProbeJob 'locked-poweroff' $r
$text = "$($w.error) $(Get-PcvProbeError $r)"
Add-PcvProbeStep 'locked-poweroff' 'rejected with PCV_VM_TEMPLATE_LOCKED' ([ordered]@{ queued_status = $w.status; error = $text.Trim() }) ($text -match 'PCV_VM_TEMPLATE_LOCKED')

$u = Wait-PcvProbeJob 'unlock' (Invoke-PcvProbeCli 'vm-template-unlock' @('vm', 'template-unlock', $vm, '--yes'))
$v = Get-PcvProbeVm $vm
Add-PcvProbeStep 'unlock' 'unlock succeeded, template_lock absent' ([ordered]@{ job = $u.status; template_lock = (Get-PcvField $v 'template_lock') }) `
    (($u.status -eq 'succeeded') -and ($null -eq (Get-PcvField $v 'template_lock')))
$p2 = Wait-PcvProbeJob 'unlocked-poweroff' (Invoke-PcvProbeCli 'vm-poweroff-unlocked' @('vm', 'poweroff', $vm))
$rn = Wait-PcvProbeJob 'unlocked-rename' (Invoke-PcvProbeCli 'vm-rename-unlocked' @('vm', 'rename', $vm, $vm2))
Add-PcvProbeStep 'unlocked-mutations' 'poweroff and rename allowed after unlock' ([ordered]@{ poweroff = $p2.status; rename = $rn.status }) `
    (($p2.status -eq 'succeeded') -and ($rn.status -eq 'succeeded'))

$hypervLeft = Remove-PcvProbeVms -Prefix $ProbePrefix -VmRoot $VmRoot
$listedLeft = @(Get-PcvProbeVmRows 'vm-list-after' | Where-Object { (Get-PcvField $_ 'name') -like "$ProbePrefix*" }).Count
$keepAfter = Get-PcvProbeVm $PreservedVm
Add-PcvProbeStep 'cleanup' 'no probe VM left, preserved VM unchanged' `
    ([ordered]@{ hyperv_probe_left = $hypervLeft; listed_probe_left = $listedLeft; preserved_state = (Get-PcvField $keepAfter 'state') }) `
    (($hypervLeft -eq 0) -and ($listedLeft -eq 0) -and ((Get-PcvField $keepAfter 'state') -eq (Get-PcvField $keep 'state')))

$serviceState = Get-PcvProbeServiceState
Add-PcvProbeStep 'service' 'Running/Automatic, Web 200' $serviceState $serviceState.pass

Write-PcvProbeSummary ([ordered]@{
    schema_version = 1
    probe = $plan.probe
    evidence_id = $EvidenceId
    installed_version = (Get-Item (Join-Path (Split-Path $CliPath) 'DesktopNode.Host.exe')).VersionInfo.ProductVersion
    pass = -not ($script:PcvProbeSteps | Where-Object { -not $_.pass })
    steps = $script:PcvProbeSteps
    host_mutation_performed = $true
    host_mutation_scope = $plan.host_mutation_scope
})
