[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$CatalogPath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Channel,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [ValidateNotNullOrEmpty()]
    [string]$BaselineVersion = '0.38.8',

    [ValidateNotNullOrEmpty()]
    [string]$CleanHostProfile = 'clean-windows-hyperv-public-smoke',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the smoke preflight descriptor.'
}

function Get-PcvPropertyValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        throw "PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_INVALID: missing $Path.$Name"
    }

    $property.Value
}

function Assert-PcvHttpsUri {
    param(
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$Code
    )

    $uri = $null
    if (-not [System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$uri)) {
        throw $Code
    }

    if ($uri.Scheme -ne 'https') {
        throw $Code
    }
}

$catalogFull = [System.IO.Path]::GetFullPath($CatalogPath)
if (-not (Test-Path -LiteralPath $catalogFull)) {
    throw "PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_CATALOG_NOT_FOUND: $catalogFull"
}

$catalog = Get-Content -Raw -LiteralPath $catalogFull | ConvertFrom-Json
$schemaVersion = Get-PcvPropertyValue -Object $catalog -Name 'schema_version' -Path 'catalog'
if ([string]$schemaVersion -ne '1') {
    throw "PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_SCHEMA_UNSUPPORTED: $schemaVersion"
}

$publication = Get-PcvPropertyValue -Object $catalog -Name 'publication' -Path 'catalog'
$channels = @(Get-PcvPropertyValue -Object $catalog -Name 'channels' -Path 'catalog')
$publicSigning = Get-PcvPropertyValue -Object $publication -Name 'public_trusted_signing' -Path 'catalog.publication'
$externalPublication = Get-PcvPropertyValue -Object $publication -Name 'external_stable_publication' -Path 'catalog.publication'
$catalogPublication = Get-PcvPropertyValue -Object $publication -Name 'catalog_publication' -Path 'catalog.publication'

if ($publicSigning -ne 'not-claimed' -or $externalPublication -ne 'not-claimed') {
    throw 'PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_PUBLIC_CLAIM_REQUIRES_EVIDENCE_IMPORT'
}

if ($catalogPublication -ne 'not-published') {
    throw 'PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_CATALOG_PUBLICATION_REQUIRES_EVIDENCE_IMPORT'
}

$selectedChannel = $channels | Where-Object {
    $name = Get-PcvPropertyValue -Object $_ -Name 'name' -Path 'catalog.channels[]'
    $name -eq $Channel
} | Select-Object -First 1

if ($null -eq $selectedChannel) {
    throw "PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_CHANNEL_NOT_FOUND: $Channel"
}

$targetVersion = Get-PcvPropertyValue -Object $selectedChannel -Name 'version' -Path 'catalog.channels[]'
$packageUri = Get-PcvPropertyValue -Object $selectedChannel -Name 'package_uri' -Path 'catalog.channels[]'
$packageSha256 = Get-PcvPropertyValue -Object $selectedChannel -Name 'sha256' -Path 'catalog.channels[]'
$rollbackCompatibleFrom = if ($selectedChannel.PSObject.Properties['rollback_compatible_from']) {
    [string]$selectedChannel.rollback_compatible_from
} else {
    $BaselineVersion
}

Assert-PcvHttpsUri -Value $packageUri -Code 'PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_PACKAGE_URI_UNTRUSTED'

if ($packageSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_PACKAGE_SHA256_INVALID'
}

if ([string]::IsNullOrWhiteSpace($BaselineVersion)) {
    throw 'PCV_PUBLIC_SIGNED_UPDATE_ROLLBACK_PREFLIGHT_BASELINE_VERSION_REQUIRED'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$smokePlanPath = Join-Path $artifactRootFull 'public-signed-update-rollback-smoke.plan-preview.json'
$requiredEvidence = @(
    'public-signed-install',
    'public-signed-update',
    'rollback-final-state',
    'clean-host-health'
)

$smokePlan = [ordered]@{
    schema_version = 1
    scope = 'public-signed-update-rollback-smoke-preflight'
    clean_host_profile = $CleanHostProfile
    baseline_version = $BaselineVersion
    target_version = $targetVersion
    rollback_compatible_from = $rollbackCompatibleFrom
    smoke_status = 'not-run'
    required_evidence = $requiredEvidence
    update = [ordered]@{
        channel = $Channel
        package_uri = $packageUri
        sha256 = $packageSha256
    }
    planned_observations = @(
        'fresh-install-health',
        'target-update-health',
        'rollback-health',
        'final-service-running',
        'no-public-claim-without-signing-and-publication-evidence'
    )
}

$smokePlan | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $smokePlanPath -Encoding utf8

$preflightChecks = @(
    [ordered]@{ name = 'catalog-schema-v1'; status = 'pass' },
    [ordered]@{ name = 'selected-channel-present'; status = 'pass' },
    [ordered]@{ name = 'package-uri-https'; status = 'pass' },
    [ordered]@{ name = 'package-sha256-present'; status = 'pass' },
    [ordered]@{ name = 'baseline-version-present'; status = 'pass' },
    [ordered]@{ name = 'clean-host-profile-recorded'; status = 'pass' },
    [ordered]@{ name = 'public-trusted-signing-required'; status = 'blocked' },
    [ordered]@{ name = 'external-stable-publication-required'; status = 'blocked' },
    [ordered]@{ name = 'signed-update-rollback-smoke-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'public-signed-update-rollback-smoke-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    public_signed_update_rollback_smoke = 'blocked-by-public-signing-and-publication'
    clean_host_smoke_status = 'not-run'
    catalog_path = $catalogFull
    channel = $Channel
    baseline_version = $BaselineVersion
    target_version = $targetVersion
    rollback_compatible_from = $rollbackCompatibleFrom
    clean_host_profile = $CleanHostProfile
    smoke_plan_path = $smokePlanPath
    selected_channel = [ordered]@{
        name = $Channel
        version = $targetVersion
        package_uri = $packageUri
        sha256 = $packageSha256
    }
    preflight_checks = @($preflightChecks)
    blockers = @(
        'public trusted signing evidence is not claimed',
        'external stable publication evidence is not claimed',
        'clean-host public signed install/update/rollback smoke has not run'
    )
    source_catalog = [ordered]@{
        schema_version = $schemaVersion
        publication = [ordered]@{
            public_trusted_signing = $publicSigning
            external_stable_publication = $externalPublication
            catalog_publication = $catalogPublication
        }
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 12
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
