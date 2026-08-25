Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-PcvManualAdminBaselineSha256 {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|host-identity-empty'
    }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
        ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function ConvertTo-PcvManualAdminVersion {
    param(
        [Parameter(Mandatory)][string]$Value,
        [Parameter(Mandatory)][string]$Field
    )

    $match = [regex]::Match($Value, '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)-admin-smoke$')
    if (-not $match.Success) {
        throw "PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|$Field|invalid-version"
    }
    [version]::new(
        [int]$match.Groups['major'].Value,
        [int]$match.Groups['minor'].Value,
        [int]$match.Groups['patch'].Value)
}

function Assert-PcvManualAdminBaselineReservationRecord {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Record)

    $required = @(
        'schema_version', 'contract', 'reservation_id', 'campaign_id', 'baseline_version',
        'target_version', 'host_fingerprint_sha256', 'reservation_kind', 'resource_reference',
        'installed_version_at_reservation', 'created_at', 'expires_at', 'status'
    )
    $properties = @($Record.PSObject.Properties.Name)
    foreach ($name in $required) {
        if ($name -notin $properties) {
            throw "PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|$name|missing"
        }
    }
    $unexpected = @($properties | Where-Object { $_ -notin $required })
    if ($unexpected.Count -gt 0) {
        throw "PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|unexpected|$($unexpected -join ',')"
    }
    if ([int]$Record.schema_version -ne 1 -or
        [string]$Record.contract -ne 'pcv-manual-admin-baseline-reservation-v1') {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|contract'
    }
    $reservationId = [guid]::Empty
    if (-not [guid]::TryParse([string]$Record.reservation_id, [ref]$reservationId)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|reservation_id'
    }
    foreach ($field in @('campaign_id', 'resource_reference')) {
        if ([string]::IsNullOrWhiteSpace([string]$Record.$field)) {
            throw "PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|$field|empty"
        }
    }
    $baseline = ConvertTo-PcvManualAdminVersion -Value ([string]$Record.baseline_version) -Field 'baseline_version'
    $target = ConvertTo-PcvManualAdminVersion -Value ([string]$Record.target_version) -Field 'target_version'
    [void](ConvertTo-PcvManualAdminVersion `
        -Value ([string]$Record.installed_version_at_reservation) `
        -Field 'installed_version_at_reservation')
    if ($target -le $baseline) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_VERSION_ORDER_INVALID'
    }
    if ([string]$Record.host_fingerprint_sha256 -notmatch '^[0-9a-f]{64}$') {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|host_fingerprint_sha256'
    }
    if ([string]$Record.reservation_kind -notin @('dedicated-host', 'hyperv-checkpoint')) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|reservation_kind'
    }
    if ([string]$Record.status -notin @('reserved', 'consumed', 'released')) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|status'
    }
    $created = [datetimeoffset]::MinValue
    $expires = [datetimeoffset]::MinValue
    if (-not [datetimeoffset]::TryParse([string]$Record.created_at, [ref]$created) -or
        -not [datetimeoffset]::TryParse([string]$Record.expires_at, [ref]$expires)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|timestamp'
    }
    if ($expires -le $created) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_EXPIRY_INVALID'
    }
    $Record
}

function New-PcvManualAdminBaselineReservationRecord {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$CampaignId,
        [Parameter(Mandatory)][string]$BaselineVersion,
        [Parameter(Mandatory)][string]$TargetVersion,
        [Parameter(Mandatory)][string]$InstalledVersion,
        [Parameter(Mandatory)][string]$HostIdentity,
        [Parameter(Mandatory)]
        [ValidateSet('dedicated-host', 'hyperv-checkpoint')]
        [string]$ReservationKind,
        [Parameter(Mandatory)][string]$ResourceReference,
        [Parameter(Mandatory)][datetimeoffset]$Now,
        [Parameter(Mandatory)][datetimeoffset]$ExpiresAt
    )

    if ([string]::IsNullOrWhiteSpace($CampaignId)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|campaign_id|empty'
    }
    if ([string]::IsNullOrWhiteSpace($ResourceReference)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_RESOURCE_INVALID'
    }
    $baseline = ConvertTo-PcvManualAdminVersion -Value $BaselineVersion -Field 'baseline_version'
    $target = ConvertTo-PcvManualAdminVersion -Value $TargetVersion -Field 'target_version'
    [void](ConvertTo-PcvManualAdminVersion -Value $InstalledVersion -Field 'installed_version')
    if (-not [string]::Equals($InstalledVersion, $BaselineVersion, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_VERSION_MISMATCH'
    }
    if ($target -le $baseline) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_VERSION_ORDER_INVALID'
    }
    if ($ExpiresAt -le $Now) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_EXPIRY_INVALID'
    }

    $record = [pscustomobject]([ordered]@{
        schema_version = 1
        contract = 'pcv-manual-admin-baseline-reservation-v1'
        reservation_id = [guid]::NewGuid().ToString()
        campaign_id = $CampaignId.Trim()
        baseline_version = $BaselineVersion
        target_version = $TargetVersion
        host_fingerprint_sha256 = Get-PcvManualAdminBaselineSha256 -Value $HostIdentity
        reservation_kind = $ReservationKind
        resource_reference = $ResourceReference.Trim()
        installed_version_at_reservation = $InstalledVersion
        created_at = $Now.ToUniversalTime().ToString('o')
        expires_at = $ExpiresAt.ToUniversalTime().ToString('o')
        status = 'reserved'
    })
    Assert-PcvManualAdminBaselineReservationRecord -Record $record
}

function Read-PcvManualAdminBaselineReservation {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$ReservationPath)

    if (-not (Test-Path -LiteralPath $ReservationPath -PathType Leaf)) {
        throw "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_REQUIRED|$ReservationPath"
    }
    try {
        $record = Get-Content -Raw -LiteralPath $ReservationPath | ConvertFrom-Json
    }
    catch {
        throw "PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|reservation|unreadable"
    }
    Assert-PcvManualAdminBaselineReservationRecord -Record $record
}

function Test-PcvManualAdminBaselineReservation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ReservationPath,
        [Parameter(Mandatory)][string]$ExpectedCampaignId,
        [Parameter(Mandatory)][string]$ExpectedBaselineVersion,
        [Parameter(Mandatory)][string]$ExpectedTargetVersion,
        [Parameter(Mandatory)][string]$ExpectedHostFingerprint,
        [Parameter(Mandatory)][datetimeoffset]$Now
    )

    $record = Read-PcvManualAdminBaselineReservation -ReservationPath $ReservationPath

    # A consumed or released reservation is checked before expiry so a reused record reports
    # why it cannot be reused rather than how old it is.
    $sidecar = Join-Path (Split-Path -Parent $ReservationPath) 'reservation-consumed.json'
    if ([string]$record.status -ne 'reserved' -or (Test-Path -LiteralPath $sidecar -PathType Leaf)) {
        throw "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_CONSUMED|$($record.reservation_id)"
    }

    $mismatches = @()
    if (-not [string]::Equals([string]$record.campaign_id, $ExpectedCampaignId, [StringComparison]::Ordinal)) {
        $mismatches += 'campaign_id'
    }
    if (-not [string]::Equals([string]$record.baseline_version, $ExpectedBaselineVersion, [StringComparison]::OrdinalIgnoreCase)) {
        $mismatches += 'baseline_version'
    }
    if (-not [string]::Equals([string]$record.target_version, $ExpectedTargetVersion, [StringComparison]::OrdinalIgnoreCase)) {
        $mismatches += 'target_version'
    }
    if (-not [string]::Equals([string]$record.host_fingerprint_sha256, $ExpectedHostFingerprint, [StringComparison]::OrdinalIgnoreCase)) {
        $mismatches += 'host_fingerprint_sha256'
    }
    if ($mismatches.Count -gt 0) {
        throw "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_MISMATCH|$($mismatches -join ',')"
    }

    $expires = [datetimeoffset]::Parse([string]$record.expires_at)
    if ($Now -gt $expires) {
        throw "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_EXPIRED|$($record.expires_at)"
    }

    $record
}

function Set-PcvManualAdminBaselineReservationState {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ReservationPath,
        [Parameter(Mandatory)]
        [ValidateSet('consumed', 'released')]
        [string]$State,
        [Parameter(Mandatory)][datetimeoffset]$Now,
        [Parameter(Mandatory)][string]$Reason
    )

    if ([string]::IsNullOrWhiteSpace($Reason)) {
        throw 'PCV_MANUAL_ADMIN_BASELINE_SCHEMA_INVALID|reason|empty'
    }
    $record = Read-PcvManualAdminBaselineReservation -ReservationPath $ReservationPath

    # The reservation itself is append-only evidence: the transition is recorded in a sidecar
    # so the original record stays byte-identical and a replay cannot be hidden by a rewrite.
    $sidecarPath = Join-Path (Split-Path -Parent $ReservationPath) 'reservation-consumed.json'
    if (Test-Path -LiteralPath $sidecarPath -PathType Leaf) {
        throw "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_CONSUMED|$($record.reservation_id)"
    }

    $sidecar = [pscustomobject]([ordered]@{
        schema_version = 1
        contract = 'pcv-manual-admin-baseline-reservation-state-v1'
        reservation_id = [string]$record.reservation_id
        campaign_id = [string]$record.campaign_id
        baseline_version = [string]$record.baseline_version
        target_version = [string]$record.target_version
        status = $State
        reason = $Reason.Trim()
        recorded_at = $Now.ToUniversalTime().ToString('o')
    })
    $sidecar | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $sidecarPath -Encoding utf8
    $sidecar
}

Export-ModuleMember -Function `
    Assert-PcvManualAdminBaselineReservationRecord, `
    Get-PcvManualAdminBaselineSha256, `
    New-PcvManualAdminBaselineReservationRecord, `
    Read-PcvManualAdminBaselineReservation, `
    Test-PcvManualAdminBaselineReservation, `
    Set-PcvManualAdminBaselineReservationState
