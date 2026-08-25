[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CampaignId,
    [Parameter(Mandatory)][string]$BaselineVersion,
    [Parameter(Mandatory)][string]$TargetVersion,
    [Parameter(Mandatory)]
    [ValidateSet('dedicated-host', 'hyperv-checkpoint')]
    [string]$ReservationKind,
    [Parameter(Mandatory)][string]$ResourceReference,
    [string]$InstalledManifestPath = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json',
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("manual-admin-baseline-reservation-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [ValidateRange(1, 720)][int]$ExpiryHours = 48,
    [string]$HostIdentityOverride = '',
    [datetimeoffset]$Now = [datetimeoffset]::UtcNow,
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvManualAdminBaselineReservation.psm1'
Import-Module $modulePath -Force

function Resolve-PcvReservationPath {
    param([Parameter(Mandatory)][string]$Path)
    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetFullPath($Path)
    }
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

$manifestPath = Resolve-PcvReservationPath -Path $InstalledManifestPath
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw 'PCV_MANUAL_ADMIN_BASELINE_INSTALLED_MANIFEST_INVALID|missing'
}
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
if ('version' -notin @($manifest.PSObject.Properties.Name) -or
    [string]::IsNullOrWhiteSpace([string]$manifest.version)) {
    throw 'PCV_MANUAL_ADMIN_BASELINE_INSTALLED_MANIFEST_INVALID|version'
}

$hostIdentity = $HostIdentityOverride
if ([string]::IsNullOrWhiteSpace($hostIdentity)) {
    try {
        $machineGuid = [string](Get-ItemPropertyValue `
            -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Cryptography' `
            -Name 'MachineGuid' `
            -ErrorAction Stop)
        $hostIdentity = "$machineGuid|$env:COMPUTERNAME"
    }
    catch {
        throw 'PCV_MANUAL_ADMIN_BASELINE_HOST_IDENTITY_UNAVAILABLE'
    }
}

$record = New-PcvManualAdminBaselineReservationRecord `
    -CampaignId $CampaignId `
    -BaselineVersion $BaselineVersion `
    -TargetVersion $TargetVersion `
    -InstalledVersion ([string]$manifest.version) `
    -HostIdentity $hostIdentity `
    -ReservationKind $ReservationKind `
    -ResourceReference $ResourceReference `
    -Now $Now `
    -ExpiresAt $Now.AddHours($ExpiryHours)

if (-not $PlanOnly) {
    $artifactPath = Resolve-PcvReservationPath -Path $ArtifactRoot
    $reservationPath = Join-Path $artifactPath 'reservation.json'
    if (Test-Path -LiteralPath $reservationPath) {
        throw "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_EXISTS|$reservationPath"
    }
    New-Item -ItemType Directory -Path $artifactPath -Force | Out-Null
    $temporaryPath = Join-Path $artifactPath ("reservation.json.tmp-" + [guid]::NewGuid().ToString('N'))
    try {
        $json = $record | ConvertTo-Json -Depth 8
        [IO.File]::WriteAllText($temporaryPath, $json + "`n", [Text.UTF8Encoding]::new($false))
        $roundTrip = Get-Content -Raw -LiteralPath $temporaryPath | ConvertFrom-Json
        [void](Assert-PcvManualAdminBaselineReservationRecord -Record $roundTrip)
        [IO.File]::Move($temporaryPath, $reservationPath, $false)
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
}

$record | ConvertTo-Json -Depth 8
