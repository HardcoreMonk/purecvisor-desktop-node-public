[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublicationDescriptorPath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [ValidateNotNullOrEmpty()]
    [string]$PackageIdentityName = 'PureCVisor.DesktopNode',

    [ValidateNotNullOrEmpty()]
    [string]$Publisher = 'CN=PureCVisor',

    [ValidateNotNullOrEmpty()]
    [string]$PublisherDisplayName = 'PureCVisor',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_MSIX_PACKAGING_FEASIBILITY_PREFLIGHT_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the MSIX feasibility preflight descriptor.'
}

function Get-PcvPropertyValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        throw "PCV_MSIX_PACKAGING_FEASIBILITY_INVALID: missing $Path.$Name"
    }

    $property.Value
}

function ConvertTo-PcvXmlAttribute {
    param(
        [Parameter(Mandatory = $true)][string]$Value
    )

    [System.Security.SecurityElement]::Escape($Value)
}

function ConvertTo-PcvMsixVersion {
    param(
        [Parameter(Mandatory = $true)][string]$Version
    )

    $parts = @($Version -split '\.')
    if ($parts.Count -lt 3 -or $parts.Count -gt 4) {
        throw "PCV_MSIX_PACKAGING_FEASIBILITY_VERSION_INVALID: $Version"
    }

    $normalized = @()
    foreach ($part in $parts) {
        if ($part -notmatch '^\d+$') {
            throw "PCV_MSIX_PACKAGING_FEASIBILITY_VERSION_INVALID: $Version"
        }
        $normalized += $part
    }

    while ($normalized.Count -lt 4) {
        $normalized += '0'
    }

    $normalized -join '.'
}

$descriptorFull = [System.IO.Path]::GetFullPath($PublicationDescriptorPath)
if (-not (Test-Path -LiteralPath $descriptorFull)) {
    throw "PCV_MSIX_PACKAGING_FEASIBILITY_DESCRIPTOR_NOT_FOUND: $descriptorFull"
}

$descriptor = Get-Content -Raw -LiteralPath $descriptorFull | ConvertFrom-Json
$descriptorSchema = Get-PcvPropertyValue -Object $descriptor -Name 'schema_version' -Path 'descriptor'
if ([string]$descriptorSchema -ne '1') {
    throw "PCV_MSIX_PACKAGING_FEASIBILITY_DESCRIPTOR_SCHEMA_UNSUPPORTED: $descriptorSchema"
}

$product = Get-PcvPropertyValue -Object $descriptor -Name 'product' -Path 'descriptor'
$artifact = Get-PcvPropertyValue -Object $descriptor -Name 'artifact' -Path 'descriptor'
$publication = Get-PcvPropertyValue -Object $descriptor -Name 'publication' -Path 'descriptor'

$productName = Get-PcvPropertyValue -Object $product -Name 'name' -Path 'descriptor.product'
$productVersion = Get-PcvPropertyValue -Object $product -Name 'version' -Path 'descriptor.product'
$releaseChannel = Get-PcvPropertyValue -Object $product -Name 'release_channel' -Path 'descriptor.product'
$msiSha256 = Get-PcvPropertyValue -Object $artifact -Name 'msi_sha256' -Path 'descriptor.artifact'
$signingMode = Get-PcvPropertyValue -Object $artifact -Name 'signing_mode' -Path 'descriptor.artifact'
$signingTrustModel = Get-PcvPropertyValue -Object $artifact -Name 'signing_trust_model' -Path 'descriptor.artifact'
$publicSigning = Get-PcvPropertyValue -Object $publication -Name 'public_trusted_signing' -Path 'descriptor.publication'
$externalPublication = Get-PcvPropertyValue -Object $publication -Name 'external_stable_publication' -Path 'descriptor.publication'
$msixState = Get-PcvPropertyValue -Object $publication -Name 'msix' -Path 'descriptor.publication'

if ($msiSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'PCV_MSIX_PACKAGING_FEASIBILITY_MSI_SHA256_INVALID'
}

if ($publicSigning -ne 'not-claimed' -or $externalPublication -ne 'not-claimed' -or $msixState -ne 'not-built') {
    throw 'PCV_MSIX_PACKAGING_FEASIBILITY_CLAIMED: preflight only accepts not-claimed and not-built publication descriptors.'
}

$msixVersion = ConvertTo-PcvMsixVersion -Version ([string]$productVersion)
$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$msixRoot = Join-Path $artifactRootFull 'msix'
$manifestPreviewPath = Join-Path $msixRoot 'AppxManifest.preview.xml'
New-Item -ItemType Directory -Force -Path $msixRoot | Out-Null

$packageIdentityEscaped = ConvertTo-PcvXmlAttribute -Value $PackageIdentityName
$publisherEscaped = ConvertTo-PcvXmlAttribute -Value $Publisher
$publisherDisplayNameEscaped = ConvertTo-PcvXmlAttribute -Value $PublisherDisplayName
$productNameEscaped = ConvertTo-PcvXmlAttribute -Value ([string]$productName)

$manifestLines = @(
    '<?xml version="1.0" encoding="utf-8"?>',
    '<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" IgnorableNamespaces="uap rescap">',
    "  <Identity Name=""$packageIdentityEscaped"" Publisher=""$publisherEscaped"" Version=""$msixVersion"" ProcessorArchitecture=""x64"" />",
    '  <Properties>',
    "    <DisplayName>$productNameEscaped</DisplayName>",
    "    <PublisherDisplayName>$publisherDisplayNameEscaped</PublisherDisplayName>",
    '    <Logo>Assets\StoreLogo.png</Logo>',
    '  </Properties>',
    '  <Dependencies>',
    '    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.26100.0" />',
    '  </Dependencies>',
    '  <Applications />',
    '</Package>'
)

Set-Content -LiteralPath $manifestPreviewPath -Value $manifestLines -Encoding utf8

$msixChecks = @(
    [ordered]@{ name = 'publication-descriptor-schema-v1'; status = 'pass' },
    [ordered]@{ name = 'package-identity-preview-written'; status = 'pass' },
    [ordered]@{ name = 'service-packaging-design-required'; status = 'blocked' },
    [ordered]@{ name = 'install-update-remove-evidence-required'; status = 'blocked' },
    [ordered]@{ name = 'capability-boundary-required'; status = 'blocked' },
    [ordered]@{ name = 'public-claim-not-made'; status = 'pass' },
    [ordered]@{ name = 'msix-build-not-executed'; status = 'pass' }
)

$requiredBeforePass = @(
    'service-install-start-stop-design',
    'appxmanifest-capability-boundary',
    'install-update-remove-evidence',
    'public-signing-decision'
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'msix-packaging-feasibility-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    msix = 'feasibility-blocked-by-service-packaging-design'
    publication_descriptor_path = $descriptorFull
    package_manifest_preview_path = $manifestPreviewPath
    product = [ordered]@{
        name = $productName
        version = $productVersion
        msix_version = $msixVersion
        release_channel = $releaseChannel
    }
    package_identity = [ordered]@{
        name = $PackageIdentityName
        publisher = $Publisher
        publisher_display_name = $PublisherDisplayName
    }
    source_artifact = [ordered]@{
        msi_sha256 = $msiSha256
        signing_mode = $signingMode
        signing_trust_model = $signingTrustModel
    }
    feasibility = [ordered]@{
        status = 'blocked-by-service-packaging-design'
        required_before_pass = @($requiredBeforePass)
        msix_build = 'not-run'
        host_mutation = 'not-run'
    }
    msix_checks = @($msixChecks)
    command_plan = [ordered]@{
        msix = [ordered]@{
            package_manifest_preview = $manifestPreviewPath
            build_status = 'not-run'
            output = 'not-built'
        }
        lifecycle = [ordered]@{
            install_update_remove = 'required-before-pass'
            capability_boundary = 'required-before-pass'
        }
        publication = [ordered]@{
            public_trusted_signing = 'not-claimed'
            external_stable_publication = 'not-claimed'
            msix = 'feasibility-blocked-by-service-packaging-design'
        }
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 12
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
