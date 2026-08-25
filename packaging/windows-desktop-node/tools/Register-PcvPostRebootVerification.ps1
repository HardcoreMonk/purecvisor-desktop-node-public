param(
    [Parameter(Mandatory)][string]$PhaseId,
    [string]$RepoRoot = '',
    [Parameter(Mandatory)][string]$EvidenceDir,
    [string]$Profile = 'ProductStatus',
    [string]$TaskName = '',
    [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
    [string]$PrincipalMode = 'LocalSystemAtStartup',
    [string[]]$ContinuationProfiles = @(),
    [switch]$RequiresUserProfile,
    [switch]$RequiresNetworkDrive,
    [switch]$RequiresSigningMaterial,
    [switch]$DryRun,
    [switch]$Reboot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvPostRebootVerification.psm1'
Import-Module $modulePath -Force

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../..') -ErrorAction Stop).Path
}

if ([string]::IsNullOrWhiteSpace($TaskName)) {
    $TaskName = "PureCVisorDesktopNode-PostRebootVerification-$PhaseId"
}

if ($Reboot) {
    throw 'PCV_POST_REBOOT_AUTO_REBOOT_DISABLED|Automatic reboot is disabled for this workflow.|Register the post-reboot verification task, then reboot manually when ready.'
}

$result = Initialize-PcvPostRebootVerification `
    -PhaseId $PhaseId `
    -RepoRoot $RepoRoot `
    -EvidenceDir $EvidenceDir `
    -Profile $Profile `
    -TaskName $TaskName `
    -PrincipalMode $PrincipalMode `
    -ContinuationProfiles $ContinuationProfiles `
    -RequiresUserProfile:$RequiresUserProfile `
    -RequiresNetworkDrive:$RequiresNetworkDrive `
    -RequiresSigningMaterial:$RequiresSigningMaterial `
    -DryRun:$DryRun

$result | ConvertTo-Json -Depth 32
