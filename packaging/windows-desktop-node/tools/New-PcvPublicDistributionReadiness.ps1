[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublicationDescriptorPath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [ValidateNotNullOrEmpty()]
    [string]$InstallerUrl,

    [ValidatePattern('^[A-Fa-f0-9]{64}$')]
    [string]$InstallerSha256,

    [ValidateSet('Unknown', 'AzureArtifactSigning', 'TrustedRootProgramCA', 'OVCertificate', 'EVCertificate', 'MicrosoftStoreMSIX', 'SignPathFoundation')]
    [string]$SigningProvider = 'Unknown',

    [ValidateNotNullOrEmpty()]
    [string]$ReleaseApproval = 'missing',

    [ValidateNotNullOrEmpty()]
    [string]$PackageIdentifier = 'PureCVisor.DesktopNode',

    [ValidateNotNullOrEmpty()]
    [string]$Publisher = 'PureCVisor',

    [ValidateNotNullOrEmpty()]
    [string]$PackageName = 'PureCVisor Desktop Node',

    [ValidateNotNullOrEmpty()]
    [string]$PackageLocale = 'en-US',

    [ValidateNotNullOrEmpty()]
    [string]$License = 'Proprietary',

    [ValidateNotNullOrEmpty()]
    [string]$ShortDescription = 'Local Windows Desktop Node management service.',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_PUBLIC_DISTRIBUTION_READINESS_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the readiness preflight descriptor.'
}

function Get-PcvPropertyValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        throw "PCV_PUBLIC_DISTRIBUTION_DESCRIPTOR_INVALID: missing $Path.$Name"
    }

    $property.Value
}

function New-PcvWingetManifestPreview {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$PackageIdentifier,
        [Parameter(Mandatory = $true)][string]$PackageVersion,
        [Parameter(Mandatory = $true)][string]$PackageLocale,
        [Parameter(Mandatory = $true)][string]$Publisher,
        [Parameter(Mandatory = $true)][string]$PackageName,
        [Parameter(Mandatory = $true)][string]$License,
        [Parameter(Mandatory = $true)][string]$ShortDescription,
        [Parameter(Mandatory = $true)][string]$InstallerUrl,
        [Parameter(Mandatory = $true)][string]$InstallerSha256
    )

    $lines = @(
        '# yaml-language-server: $schema=https://aka.ms/winget-manifest.singleton.1.12.0.schema.json',
        "PackageIdentifier: $PackageIdentifier",
        "PackageVersion: $PackageVersion",
        "PackageLocale: $PackageLocale",
        "Publisher: $Publisher",
        "PackageName: $PackageName",
        "License: $License",
        "ShortDescription: $ShortDescription",
        'Installers:',
        '  - Architecture: x64',
        '    InstallerType: msi',
        "    InstallerUrl: $InstallerUrl",
        "    InstallerSha256: $InstallerSha256",
        'ManifestType: singleton',
        'ManifestVersion: 1.12.0'
    )

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    Set-Content -LiteralPath $Path -Value $lines -Encoding utf8
}

$publicationDescriptorFull = [System.IO.Path]::GetFullPath($PublicationDescriptorPath)
if (-not (Test-Path -LiteralPath $publicationDescriptorFull)) {
    throw "PCV_PUBLIC_DISTRIBUTION_DESCRIPTOR_NOT_FOUND: $publicationDescriptorFull"
}

$descriptor = Get-Content -Raw -LiteralPath $publicationDescriptorFull | ConvertFrom-Json
$descriptorSchema = Get-PcvPropertyValue -Object $descriptor -Name 'schema_version' -Path 'descriptor'
if ($descriptorSchema -ne '1') {
    throw "PCV_PUBLIC_DISTRIBUTION_DESCRIPTOR_SCHEMA_UNSUPPORTED: $descriptorSchema"
}

$product = Get-PcvPropertyValue -Object $descriptor -Name 'product' -Path 'descriptor'
$artifact = Get-PcvPropertyValue -Object $descriptor -Name 'artifact' -Path 'descriptor'
$publication = Get-PcvPropertyValue -Object $descriptor -Name 'publication' -Path 'descriptor'
$productVersion = Get-PcvPropertyValue -Object $product -Name 'version' -Path 'descriptor.product'
$artifactSha256 = Get-PcvPropertyValue -Object $artifact -Name 'msi_sha256' -Path 'descriptor.artifact'
$descriptorPublicSigning = Get-PcvPropertyValue -Object $publication -Name 'public_trusted_signing' -Path 'descriptor.publication'
$descriptorExternalPublication = Get-PcvPropertyValue -Object $publication -Name 'external_stable_publication' -Path 'descriptor.publication'

if ($descriptorPublicSigning -ne 'not-claimed' -or $descriptorExternalPublication -ne 'not-claimed') {
    throw 'PCV_PUBLIC_DISTRIBUTION_DESCRIPTOR_CLAIMED: readiness preflight only accepts not-claimed publication descriptors.'
}

$effectiveInstallerSha256 = if ([string]::IsNullOrWhiteSpace($InstallerSha256)) {
    $artifactSha256
} else {
    $InstallerSha256
}

if ($effectiveInstallerSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'PCV_PUBLIC_DISTRIBUTION_INSTALLER_SHA256_INVALID'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$wingetRoot = Join-Path $artifactRootFull 'winget'
$wingetManifestPreviewPath = Join-Path $wingetRoot "$PackageIdentifier.yaml"
$hasInstallerUrl = -not [string]::IsNullOrWhiteSpace($InstallerUrl)

if ($hasInstallerUrl) {
    New-PcvWingetManifestPreview `
        -Path $wingetManifestPreviewPath `
        -PackageIdentifier $PackageIdentifier `
        -PackageVersion $productVersion `
        -PackageLocale $PackageLocale `
        -Publisher $Publisher `
        -PackageName $PackageName `
        -License $License `
        -ShortDescription $ShortDescription `
        -InstallerUrl $InstallerUrl `
        -InstallerSha256 $effectiveInstallerSha256
}

$hasSigningInputs = ($SigningProvider -ne 'Unknown' -and $ReleaseApproval -ne 'missing')
$gates = @(
    [ordered]@{
        name = 'public-signing-inputs'
        status = if ($hasSigningInputs) { 'input-present' } else { 'missing-input' }
        public_claim = 'not-claimed'
    },
    [ordered]@{
        name = 'winget-manifest-preview'
        status = if ($hasInstallerUrl) { 'preview-written' } else { 'missing-installer-url' }
        public_claim = 'not-claimed'
    },
    [ordered]@{
        name = 'winget-validation-command'
        status = 'manual-validation-required'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        name = 'winget-submission-plan'
        status = 'not-submitted'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        name = 'msix-service-packaging-feasibility'
        status = 'blocked-by-service-packaging-design'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        name = 'public-publication-blocker'
        status = 'public-release-not-claimed'
        public_claim = 'not-claimed'
    }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'public-distribution-readiness-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    publication_descriptor_path = $publicationDescriptorFull
    winget_manifest_preview_path = if ($hasInstallerUrl) { $wingetManifestPreviewPath } else { $null }
    inputs = [ordered]@{
        package_identifier = $PackageIdentifier
        package_version = $productVersion
        installer_url = $InstallerUrl
        installer_sha256 = $effectiveInstallerSha256
        signing_provider = $SigningProvider
        release_approval = $ReleaseApproval
    }
    gates = $gates
    command_plan = [ordered]@{
        winget = [ordered]@{
            validate_command = 'winget validate <manifest-preview-folder>'
            submission = 'not-submitted'
            repository = 'https://github.com/microsoft/winget-pkgs'
        }
        msix = [ordered]@{
            status = 'blocked-by-service-packaging-design'
            reason = 'Desktop Node installs and operates a Windows service; MSIX service packaging requires separate package design and OS support evidence.'
        }
        signing = [ordered]@{
            provider = $SigningProvider
            public_trusted_signing = 'not-claimed'
            external_stable_publication = 'not-claimed'
        }
    }
    source_publication_descriptor = [ordered]@{
        schema_version = $descriptorSchema
        artifact_sha256 = $artifactSha256
        publication_mode = Get-PcvPropertyValue -Object $publication -Name 'mode' -Path 'descriptor.publication'
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 12
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
