[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublicationDescriptorPath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$MsiUrl,

    [ValidateNotNullOrEmpty()]
    [string]$BundleName = 'PureCVisor Desktop Node',

    [ValidateNotNullOrEmpty()]
    [string]$Manufacturer = 'PureCVisor',

    [ValidateNotNullOrEmpty()]
    [string]$BundleUpgradeCode = '{8F455BB4-640E-47A2-A982-338C7A6318B5}',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_BURN_BOOTSTRAPPER_PREFLIGHT_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the Burn bootstrapper preflight descriptor.'
}

function Get-PcvPropertyValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        throw "PCV_BURN_BOOTSTRAPPER_PREFLIGHT_INVALID: missing $Path.$Name"
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

function ConvertTo-PcvXmlAttribute {
    param(
        [Parameter(Mandatory = $true)][string]$Value
    )

    [System.Security.SecurityElement]::Escape($Value)
}

$descriptorFull = [System.IO.Path]::GetFullPath($PublicationDescriptorPath)
if (-not (Test-Path -LiteralPath $descriptorFull)) {
    throw "PCV_BURN_BOOTSTRAPPER_PREFLIGHT_DESCRIPTOR_NOT_FOUND: $descriptorFull"
}

Assert-PcvHttpsUri -Value $MsiUrl -Code 'PCV_BURN_BOOTSTRAPPER_PREFLIGHT_MSI_URI_UNTRUSTED'

$parsedUpgradeCode = [guid]::Empty
if (-not [guid]::TryParse($BundleUpgradeCode, [ref]$parsedUpgradeCode)) {
    throw "PCV_BURN_BOOTSTRAPPER_PREFLIGHT_UPGRADE_CODE_INVALID: $BundleUpgradeCode"
}

$descriptor = Get-Content -Raw -LiteralPath $descriptorFull | ConvertFrom-Json
$descriptorSchema = Get-PcvPropertyValue -Object $descriptor -Name 'schema_version' -Path 'descriptor'
if ([string]$descriptorSchema -ne '1') {
    throw "PCV_BURN_BOOTSTRAPPER_PREFLIGHT_DESCRIPTOR_SCHEMA_UNSUPPORTED: $descriptorSchema"
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
$burnBootstrapper = Get-PcvPropertyValue -Object $publication -Name 'burn_bootstrapper' -Path 'descriptor.publication'

if ($msiSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'PCV_BURN_BOOTSTRAPPER_PREFLIGHT_MSI_SHA256_INVALID'
}

if ($publicSigning -ne 'not-claimed' -or $externalPublication -ne 'not-claimed' -or $burnBootstrapper -ne 'not-built') {
    throw 'PCV_BURN_BOOTSTRAPPER_PREFLIGHT_CLAIMED: preflight only accepts not-claimed and not-built publication descriptors.'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$bundleRoot = Join-Path $artifactRootFull 'burn'
$bundlePreviewPath = Join-Path $bundleRoot 'PureCVisorDesktopNode.Bundle.preview.wxs'
New-Item -ItemType Directory -Force -Path $bundleRoot | Out-Null

$bundleUpgradeCodeText = ('{' + $parsedUpgradeCode.ToString().ToUpperInvariant() + '}')
$bundleNameEscaped = ConvertTo-PcvXmlAttribute -Value $BundleName
$manufacturerEscaped = ConvertTo-PcvXmlAttribute -Value $Manufacturer
$productVersionEscaped = ConvertTo-PcvXmlAttribute -Value ([string]$productVersion)
$msiUrlEscaped = ConvertTo-PcvXmlAttribute -Value $MsiUrl

$previewLines = @(
    '<?xml version="1.0" encoding="utf-8"?>',
    '<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs" xmlns:bal="http://wixtoolset.org/schemas/v4/wxs/bal">',
    "  <Bundle Name=""$bundleNameEscaped"" Manufacturer=""$manufacturerEscaped"" Version=""$productVersionEscaped"" UpgradeCode=""$bundleUpgradeCodeText"">",
    '    <BootstrapperApplication>',
    '      <bal:WixStandardBootstrapperApplication Theme="hyperlinkLicense" LicenseUrl="" />',
    '    </BootstrapperApplication>',
    '    <Chain>',
    "      <MsiPackage Id=""PureCVisorDesktopNodeMsi"" SourceFile=""`$(var.PureCVisorDesktopNodeMsi)"" DownloadUrl=""$msiUrlEscaped"" Compressed=""no"" Vital=""yes"" Permanent=""no"" />",
    '    </Chain>',
    '  </Bundle>',
    '</Wix>'
)

Set-Content -LiteralPath $bundlePreviewPath -Value $previewLines -Encoding utf8

$burnChecks = @(
    [ordered]@{ name = 'publication-descriptor-schema-v1'; status = 'pass' },
    [ordered]@{ name = 'msi-url-https'; status = 'pass' },
    [ordered]@{ name = 'msi-sha256-present'; status = 'pass' },
    [ordered]@{ name = 'bundle-upgrade-code-valid'; status = 'pass' },
    [ordered]@{ name = 'wix-burn-authoring-preview-written'; status = 'pass' },
    [ordered]@{ name = 'public-claim-not-made'; status = 'pass' },
    [ordered]@{ name = 'bundle-build-not-executed'; status = 'pass' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'burn-bootstrapper-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    burn_bootstrapper = 'not-built'
    publication_descriptor_path = $descriptorFull
    bundle_authoring_preview_path = $bundlePreviewPath
    bundle_upgrade_code = $bundleUpgradeCodeText
    product = [ordered]@{
        name = $productName
        version = $productVersion
        release_channel = $releaseChannel
    }
    chain = [ordered]@{
        msi_url = $MsiUrl
        msi_sha256 = $msiSha256
        package_version = $productVersion
        signing_mode = $signingMode
        signing_trust_model = $signingTrustModel
        payload_hash_binding = 'preview-only'
        repair_remove_evidence = 'required-before-pass'
    }
    burn_checks = @($burnChecks)
    command_plan = [ordered]@{
        wix = [ordered]@{
            authoring_preview = $bundlePreviewPath
            build_command = 'wix build <bundle-authoring-preview.wxs> -ext WixToolset.BootstrapperApplications.wixext -o <bundle.exe>'
            build_status = 'not-run'
            output = 'not-built'
        }
        lifecycle = [ordered]@{
            install_repair_remove = 'required-before-pass'
            public_signed_update_rollback = 'separate-gate'
        }
        publication = [ordered]@{
            public_trusted_signing = 'not-claimed'
            external_stable_publication = 'not-claimed'
            burn_bootstrapper = 'not-built'
        }
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 12
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
