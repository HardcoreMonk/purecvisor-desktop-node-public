[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("manual-admin-rebaseline-readiness-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$Version = '0.41.2-admin-smoke',
    [string]$BaselineVersion = '',
    [string]$TargetVersion = '',
    [string]$InstalledManifestPath = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json',
    [string]$RouteParityArtifactRoot = 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412',
    [string]$TargetPackageArtifactRoot = '',
    [string]$CampaignId = '',
    [string]$BaselineReservationPath = '',
    # Lets a test pin the identity that is hashed into the reservation fingerprint instead of
    # reading the live machine name. Never used to weaken the match, only to make it reproducible.
    [string]$HostIdentityOverride = '',
    [datetimeoffset]$Now = [datetimeoffset]::UtcNow,
    # Declares that the caller intends to run the campaign for real. Readiness stays non-mutating
    # either way; this only decides whether a matching reservation must already exist.
    [switch]$ForActualExecution,
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly) {
    throw 'PCV_MANUAL_ADMIN_REBASELINE_PLAN_ONLY_REQUIRED|Manual-admin rebaseline readiness only writes a local descriptor. Pass -PlanOnly.'
}

function Resolve-PcvReadablePath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Write-PcvJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth 24 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Read-PcvJsonFile {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-PcvSha256FromSidecar {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    $text = Get-Content -Raw -LiteralPath $Path
    $match = [regex]::Match($text, '(?i)\b[a-f0-9]{64}\b')
    if ($match.Success) {
        return $match.Value.ToLowerInvariant()
    }

    $null
}

$artifactRootFull = Resolve-PcvReadablePath -Path $ArtifactRoot
$routeRootFull = Resolve-PcvReadablePath -Path $RouteParityArtifactRoot
$targetPackageRootFull = if ([string]::IsNullOrWhiteSpace($TargetPackageArtifactRoot)) { $null } else { Resolve-PcvReadablePath -Path $TargetPackageArtifactRoot }
$installedManifestFull = Resolve-PcvReadablePath -Path $InstalledManifestPath
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

$baselineVersionEffective = if ([string]::IsNullOrWhiteSpace($BaselineVersion)) { $Version } else { $BaselineVersion }
$packagePairMode = -not [string]::IsNullOrWhiteSpace($TargetVersion)
$runnerVersion = if ($packagePairMode) { $TargetVersion } else { $Version }

$installedManifest = Read-PcvJsonFile -Path $installedManifestFull
$installedVersion = if ($null -ne $installedManifest -and $installedManifest.PSObject.Properties.Name -contains 'version') {
    [string]$installedManifest.version
}
else {
    'unknown'
}
$installedVersionMatchesRequested = [string]::Equals($installedVersion, $baselineVersionEffective, [System.StringComparison]::OrdinalIgnoreCase)
$requestedVersionStatus = if ($installedVersionMatchesRequested) { 'matches-installed-version' } else { 'blocked-by-installed-version-mismatch' }
$currentOverrideStatus = if ($installedVersionMatchesRequested) { 'ready-with-current-version-override' } else { 'blocked-by-installed-version-mismatch' }

$currentMsiPath = Join-Path $routeRootFull "PureCVisorDesktopNode-$baselineVersionEffective-windows-x64.msi"
$currentMsiShaPath = Join-Path $routeRootFull "PureCVisorDesktopNode-$baselineVersionEffective-windows-x64.msi.sha256"
$currentPublicationPath = Join-Path $routeRootFull "PureCVisorDesktopNode-$baselineVersionEffective-windows-x64.publication.json"
$currentPayloadRoot = Join-Path $routeRootFull 'payload'
$currentMsiPresent = Test-Path -LiteralPath $currentMsiPath -PathType Leaf
$currentPayloadPresent = Test-Path -LiteralPath $currentPayloadRoot -PathType Container
$currentPublicationPresent = Test-Path -LiteralPath $currentPublicationPath -PathType Leaf
$currentMsiSha256 = Get-PcvSha256FromSidecar -Path $currentMsiShaPath
if ([string]::IsNullOrWhiteSpace($currentMsiSha256) -and $currentMsiPresent) {
    $currentMsiSha256 = (Get-FileHash -LiteralPath $currentMsiPath -Algorithm SHA256).Hash.ToLowerInvariant()
}

$targetMsiPath = $null
$targetMsiShaPath = $null
$targetPublicationPath = $null
$targetPayloadRoot = $null
$targetMsiPresent = $false
$targetPayloadPresent = $false
$targetPublicationPresent = $false
$targetMsiSha256 = $null
if ($packagePairMode -and -not [string]::IsNullOrWhiteSpace($targetPackageRootFull)) {
    $targetMsiPath = Join-Path $targetPackageRootFull "PureCVisorDesktopNode-$TargetVersion-windows-x64.msi"
    $targetMsiShaPath = Join-Path $targetPackageRootFull "PureCVisorDesktopNode-$TargetVersion-windows-x64.msi.sha256"
    $targetPublicationPath = Join-Path $targetPackageRootFull "PureCVisorDesktopNode-$TargetVersion-windows-x64.publication.json"
    $targetPayloadRoot = Join-Path $targetPackageRootFull 'payload'
    $targetMsiPresent = Test-Path -LiteralPath $targetMsiPath -PathType Leaf
    $targetPayloadPresent = Test-Path -LiteralPath $targetPayloadRoot -PathType Container
    $targetPublicationPresent = Test-Path -LiteralPath $targetPublicationPath -PathType Leaf
    $targetMsiSha256 = Get-PcvSha256FromSidecar -Path $targetMsiShaPath
    if ([string]::IsNullOrWhiteSpace($targetMsiSha256) -and $targetMsiPresent) {
        $targetMsiSha256 = (Get-FileHash -LiteralPath $targetMsiPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$packagePairInputStatus = if (-not $packagePairMode) {
    'not-requested'
}
elseif ([string]::Equals($baselineVersionEffective, $TargetVersion, [System.StringComparison]::OrdinalIgnoreCase)) {
    'blocked-by-baseline-target-version-match'
}
elseif (-not $installedVersionMatchesRequested) {
    'blocked-by-installed-baseline-version-mismatch'
}
elseif ([string]::IsNullOrWhiteSpace($targetPackageRootFull) -or -not ($targetMsiPresent -and $targetPayloadPresent -and $targetPublicationPresent)) {
    'blocked-by-missing-target-package-artifact'
}
else {
    'ready-current-baseline-target-package-pair'
}

$historicalRunnerDefaults = @(
    [ordered]@{
        runner = 'Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1'
        historical_default = '0.39.4-admin-smoke'
        canonical_evidence_version = '0.39.5-admin-smoke'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'run with explicit -Version after current package check'
    },
    [ordered]@{
        runner = 'Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1'
        historical_default = '0.39.6-admin-smoke'
        canonical_evidence_version = '0.39.6-admin-smoke'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'run with explicit -Version after current package check'
    },
    [ordered]@{
        runner = 'Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'
        historical_default = 'baseline 0.38.7-rc.1 target 0.39.1-admin-smoke'
        canonical_evidence_version = 'baseline 0.39.6-admin-smoke target 0.39.7-admin-smoke'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'dedicated clean host with current baseline and target package pair'
    },
    [ordered]@{
        runner = 'burn-bootstrapper-lifecycle-runner'
        historical_default = '0.39.1-admin-smoke'
        canonical_evidence_version = '0.39.1-admin-smoke'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'regenerate bundle lifecycle runner from current MSI'
    },
    [ordered]@{
        runner = 'msix-package-lifecycle-runner'
        historical_default = '0.39.0.0 to 0.39.1.0'
        canonical_evidence_version = '0.39.0.0 to 0.39.1.0'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'regenerate smoke package identity from current payload'
    },
    [ordered]@{
        runner = 'product-update-rollback-mutation'
        historical_default = '0.38.6-admin-smoke to 0.38.8-admin-smoke'
        canonical_evidence_version = '0.38.8-admin-smoke'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'build current baseline and target update package pair'
    },
    [ordered]@{
        runner = 'msi-update-package-apply'
        historical_default = '0.39.1-admin-smoke'
        canonical_evidence_version = '0.39.1-admin-smoke'
        current_version = $runnerVersion
        status = 'blocked-until-rebaseline'
        rebaseline_path = 'build current MSI/update package artifact set'
    }
)

$campaignBuckets = @(
    [ordered]@{
        id = 'credential-manager'
        status = $currentOverrideStatus
        runner_command = "pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1 -ArtifactRoot artifacts/windows-credential-manager-default-transition-installed-<run-id> -Version $runnerVersion"
        requires_operator_opt_in = $true
    },
    [ordered]@{
        id = 'event-log'
        status = $currentOverrideStatus
        runner_command = "pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1 -ArtifactRoot artifacts/windows-event-log-default-transition-installed-<run-id> -Version $runnerVersion"
        requires_operator_opt_in = $true
    },
    [ordered]@{
        id = 'burn-msix-msi'
        status = if ($currentMsiPresent -and $currentPayloadPresent -and $currentPublicationPresent) { 'requires-current-lifecycle-runner-generation' } else { 'blocked-by-missing-current-package-artifact' }
        current_msi_path = $currentMsiPath
        current_payload_root = $currentPayloadRoot
        current_publication_descriptor = $currentPublicationPath
        requires_operator_opt_in = $true
    },
    [ordered]@{
        id = 'update-rollback'
        status = 'requires-current-baseline-target-package-pair'
        current_baseline_version = $baselineVersionEffective
        target_version = if ($packagePairMode) { $TargetVersion } else { 'next-admin-smoke-required' }
        requires_operator_opt_in = $true
    },
    [ordered]@{
        id = 'clean-host'
        status = 'requires-dedicated-host-current-package-pair'
        current_baseline_version = $baselineVersionEffective
        target_version = if ($packagePairMode) { $TargetVersion } else { 'next-admin-smoke-required' }
        requires_operator_opt_in = $true
    }
)

$planPreviewPath = Join-Path $artifactRootFull 'manual-admin-rebaseline.plan-preview.json'
$planPreview = [ordered]@{
    schema_version = 1
    scope = 'manual-admin-rebaseline-readiness'
    version = $Version
    baseline_version = $baselineVersionEffective
    target_version = if ($packagePairMode) { $TargetVersion } else { $null }
    package_pair_input_status = $packagePairInputStatus
    mixed_version_input_policy = 'reject-baseline-target-match-or-installed-baseline-mismatch'
    installed_version = $installedVersion
    installed_version_matches_requested = $installedVersionMatchesRequested
    requested_version_status = $requestedVersionStatus
    current_package = [ordered]@{
        routeparity_artifact_root = $routeRootFull
        msi_path = $currentMsiPath
        msi_sha256 = $currentMsiSha256
        msi_present = $currentMsiPresent
        payload_root = $currentPayloadRoot
        payload_present = $currentPayloadPresent
        publication_descriptor = $currentPublicationPath
        publication_descriptor_present = $currentPublicationPresent
    }
    target_package = if ($packagePairMode) {
        [ordered]@{
            artifact_root = $targetPackageRootFull
            msi_path = $targetMsiPath
            msi_sha256 = $targetMsiSha256
            msi_present = $targetMsiPresent
            payload_root = $targetPayloadRoot
            payload_present = $targetPayloadPresent
            publication_descriptor = $targetPublicationPath
            publication_descriptor_present = $targetPublicationPresent
        }
    }
    else {
        $null
    }
    campaign_buckets = $campaignBuckets
    historical_runner_defaults = $historicalRunnerDefaults
    direct_historical_default_execution = 'blocked'
    safe_execution_boundary = 'current-version-rebaseline-or-dedicated-clean-host'
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
}
Write-PcvJsonFile -Path $planPreviewPath -Value $planPreview

# A dedicated baseline host is the blocker that has held manual-admin closure since
# 2026-05-29, so actual execution requires a reservation that already matches this campaign,
# version pair and host. Readiness itself never mutates the host, and never consumes the
# reservation: it only reports whether execution would be eligible.
# Not a neutral 'not requested': readiness always reports that a reservation is the
# precondition for actual execution, so a reader cannot mistake absence for permission.
$reservationStatus = 'reservation-required-before-actual-execution'
$actualExecutionEligible = $false
if ($ForActualExecution) {
    Import-Module (Join-Path $PSScriptRoot 'PcvManualAdminBaselineReservation.psm1') -Force
    # Must match New-PcvManualAdminBaselineReservation.ps1 exactly. That tool is the one that
    # mints the fingerprint, so any other composition here makes every real reservation fail
    # with RESERVATION_MISMATCH even when the host is correct.
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
    [void](Test-PcvManualAdminBaselineReservation `
        -ReservationPath $BaselineReservationPath `
        -ExpectedCampaignId $CampaignId `
        -ExpectedBaselineVersion $baselineVersionEffective `
        -ExpectedTargetVersion $TargetVersion `
        -ExpectedHostFingerprint (Get-PcvManualAdminBaselineSha256 -Value $hostIdentity) `
        -Now $Now)
    $reservationStatus = 'reserved-and-matched'
    $actualExecutionEligible = $true
}

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'manual-admin-rebaseline-readiness'
    plan_only = $true
    campaign_id = if ([string]::IsNullOrWhiteSpace($CampaignId)) { $null } else { $CampaignId }
    reservation_status = $reservationStatus
    baseline_reservation_path = if ([string]::IsNullOrWhiteSpace($BaselineReservationPath)) { $null } else { $BaselineReservationPath }
    actual_execution_eligible = $actualExecutionEligible
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    version = $Version
    baseline_version = $baselineVersionEffective
    target_version = if ($packagePairMode) { $TargetVersion } else { $null }
    package_pair_mode = $packagePairMode
    package_pair_input_status = $packagePairInputStatus
    mixed_version_input_policy = 'reject-baseline-target-match-or-installed-baseline-mismatch'
    installed_manifest_path = $installedManifestFull
    installed_version = $installedVersion
    installed_version_matches_requested = $installedVersionMatchesRequested
    requested_version_status = $requestedVersionStatus
    routeparity_artifact_root = $routeRootFull
    current_msi_path = $currentMsiPath
    current_msi_sha256 = $currentMsiSha256
    current_msi_present = $currentMsiPresent
    current_payload_root = $currentPayloadRoot
    current_payload_present = $currentPayloadPresent
    current_publication_descriptor = $currentPublicationPath
    current_publication_descriptor_present = $currentPublicationPresent
    target_package_artifact_root = $targetPackageRootFull
    target_msi_path = $targetMsiPath
    target_msi_sha256 = $targetMsiSha256
    target_msi_present = $targetMsiPresent
    target_payload_root = $targetPayloadRoot
    target_payload_present = $targetPayloadPresent
    target_publication_descriptor = $targetPublicationPath
    target_publication_descriptor_present = $targetPublicationPresent
    rebaseline_required = $true
    direct_historical_default_execution = 'blocked'
    safe_execution_boundary = 'current-version-rebaseline-or-dedicated-clean-host'
    plan_preview_path = $planPreviewPath
    campaign_buckets = $campaignBuckets
    historical_runner_defaults = $historicalRunnerDefaults
}

Write-PcvJsonFile -Path (Join-Path $artifactRootFull 'summary.json') -Value $summary
$summary | ConvertTo-Json -Depth 24
