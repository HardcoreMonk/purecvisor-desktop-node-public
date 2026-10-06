# Lane 2 probe: noVNC target preview, set and clear (SERVICE_PLAN P2-11), restored to the starting state.
# Host mutation: the loopback noVNC target file is written, cleared and removed again when it did not exist before.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$EvidenceId,
    [string]$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..')),
    [string]$NoVncTargetFile = 'C:\ProgramData\PureCVisor\desktop-node\novnc-target.json',
    [string]$CliPath = 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe',
    [switch]$PlanOnly
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'PcvLane2ProbeCommon.ps1')

$root = Resolve-PcvLane2ProbePath $RepoRoot $ArtifactRoot
$targetHost = '127.0.0.1'
$targetPort = '5901'
$plan = [ordered]@{
    probe = 'novnc-target'
    service_plan_items = @('P2-11')
    evidence_id = $EvidenceId
    artifact_root = $root
    target = "${targetHost}:$targetPort"
    target_file = $NoVncTargetFile
    steps = @('novnc-before', 'novnc-preview', 'novnc-set', 'novnc-clear', 'novnc-restore', 'service')
    host_mutation_scope = 'noVNC target set, clear, and file restored to the starting state'
}
if ($PlanOnly) {
    Write-PcvLane2ProbePlan $plan
    return
}

Initialize-PcvLane2Probe -ArtifactRoot $root -CliPath $CliPath

function Read-PcvNoVncTarget {
    if (Test-Path -LiteralPath $NoVncTargetFile) { return Get-Content -LiteralPath $NoVncTargetFile -Raw | ConvertFrom-Json -Depth 16 }
    return $null
}

$fileBefore = Test-Path -LiteralPath $NoVncTargetFile
Add-PcvProbeStep 'novnc-before' 'record starting state' ([ordered]@{ file_present = $fileBefore }) $true
$preview = Invoke-PcvProbeCli 'novnc-preview' @('console', 'novnc-target', 'preview', '--host', $targetHost, '--port', $targetPort, '--reason', 'lane2 probe')
Add-PcvProbeStep 'novnc-preview' 'dry-run ok, no file written' `
    ([ordered]@{ exit = $preview.exit; ok = (Get-PcvField $preview.json 'ok'); error = (Get-PcvField $preview.json 'error'); file_present = (Test-Path -LiteralPath $NoVncTargetFile) }) `
    (($preview.exit -eq 0) -and ((Get-PcvField $preview.json 'ok') -eq $true) -and ((Test-Path -LiteralPath $NoVncTargetFile) -eq $fileBefore))

$set = Invoke-PcvProbeCli 'novnc-set' @('console', 'novnc-target', 'set', '--host', $targetHost, '--port', $targetPort, '--reason', 'lane2 probe', '--yes')
$setJob = Wait-PcvProbeJob 'novnc-set' $set -TimeoutMinutes 3
$setFile = Read-PcvNoVncTarget
Add-PcvProbeStep 'novnc-set' "set succeeded, file holds loopback ${targetHost}:$targetPort enabled" `
    ([ordered]@{ result = $setJob.status; error = (Get-PcvField $set.json 'error'); host = (Get-PcvField $setFile 'host'); port = (Get-PcvField $setFile 'port'); enabled = (Get-PcvField $setFile 'enabled') }) `
    (($setJob.status -in @('immediate-ok', 'succeeded')) -and ((Get-PcvField $setFile 'host') -eq $targetHost) -and ([string](Get-PcvField $setFile 'port') -eq $targetPort) -and ((Get-PcvField $setFile 'enabled') -ne $false))

$clear = Invoke-PcvProbeCli 'novnc-clear' @('console', 'novnc-target', 'clear', '--yes')
$clearJob = Wait-PcvProbeJob 'novnc-clear' $clear -TimeoutMinutes 3
$clearFile = Read-PcvNoVncTarget
Add-PcvProbeStep 'novnc-clear' 'clear succeeded, file holds enabled=false' `
    ([ordered]@{ result = $clearJob.status; error = (Get-PcvField $clear.json 'error'); enabled = (Get-PcvField $clearFile 'enabled') }) `
    (($clearJob.status -in @('immediate-ok', 'succeeded')) -and ((Get-PcvField $clearFile 'enabled') -eq $false))

if (-not $fileBefore -and (Test-Path -LiteralPath $NoVncTargetFile)) { Remove-Item -LiteralPath $NoVncTargetFile }
Add-PcvProbeStep 'novnc-restore' 'file state equals the starting state (PathName has no noVNC target)' `
    ([ordered]@{ file_present = (Test-Path -LiteralPath $NoVncTargetFile) }) ((Test-Path -LiteralPath $NoVncTargetFile) -eq $fileBefore)

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
    secret_recorded = $false
})
