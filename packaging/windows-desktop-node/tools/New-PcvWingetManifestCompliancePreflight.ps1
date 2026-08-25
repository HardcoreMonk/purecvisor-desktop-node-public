[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ManifestPath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_WINGET_MANIFEST_COMPLIANCE_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the compliance preflight descriptor.'
}

function ConvertFrom-PcvWingetSingletonManifest {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    $root = [ordered]@{}
    $installer = [ordered]@{}
    $insideInstallers = $false

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $trimmed = $line.Trim()
        if ($trimmed.StartsWith('#')) {
            continue
        }

        if ($line -match '^\s*Installers:\s*$') {
            $insideInstallers = $true
            continue
        }

        if ($insideInstallers -and $line -match '^\s*-\s+([^:]+):\s*(.*)$') {
            $installer[$Matches[1].Trim()] = ConvertTo-PcvManifestScalar -Value $Matches[2]
            continue
        }

        if ($insideInstallers -and $line -match '^\s+([^:]+):\s*(.*)$') {
            $installer[$Matches[1].Trim()] = ConvertTo-PcvManifestScalar -Value $Matches[2]
            continue
        }

        if ($line -match '^([^:]+):\s*(.*)$') {
            $insideInstallers = $false
            $root[$Matches[1].Trim()] = ConvertTo-PcvManifestScalar -Value $Matches[2]
            continue
        }

        throw "PCV_WINGET_MANIFEST_UNSUPPORTED_LINE: $line"
    }

    [ordered]@{
        root = $root
        installer = $installer
    }
}

function ConvertTo-PcvManifestScalar {
    param(
        [AllowEmptyString()][string]$Value
    )

    $trimmed = $Value.Trim()
    if (($trimmed.StartsWith('"') -and $trimmed.EndsWith('"')) -or ($trimmed.StartsWith("'") -and $trimmed.EndsWith("'"))) {
        return $trimmed.Substring(1, $trimmed.Length - 2)
    }

    $trimmed
}

function Get-PcvRequiredMapValue {
    param(
        [Parameter(Mandatory = $true)]$Map,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if (-not $Map.Contains($Name)) {
        throw "PCV_WINGET_MANIFEST_FIELD_MISSING: $Name"
    }

    $value = [string]$Map[$Name]
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "PCV_WINGET_MANIFEST_FIELD_EMPTY: $Name"
    }

    $value
}

$manifestFull = [System.IO.Path]::GetFullPath($ManifestPath)
if (-not (Test-Path -LiteralPath $manifestFull)) {
    throw "PCV_WINGET_MANIFEST_NOT_FOUND: $manifestFull"
}

$parsed = ConvertFrom-PcvWingetSingletonManifest -Path $manifestFull
$root = $parsed.root
$installer = $parsed.installer

$packageIdentifier = Get-PcvRequiredMapValue -Map $root -Name 'PackageIdentifier'
$packageVersion = Get-PcvRequiredMapValue -Map $root -Name 'PackageVersion'
$packageLocale = Get-PcvRequiredMapValue -Map $root -Name 'PackageLocale'
$publisher = Get-PcvRequiredMapValue -Map $root -Name 'Publisher'
$packageName = Get-PcvRequiredMapValue -Map $root -Name 'PackageName'
$license = Get-PcvRequiredMapValue -Map $root -Name 'License'
$shortDescription = Get-PcvRequiredMapValue -Map $root -Name 'ShortDescription'
$manifestType = Get-PcvRequiredMapValue -Map $root -Name 'ManifestType'
$manifestVersion = Get-PcvRequiredMapValue -Map $root -Name 'ManifestVersion'
$architecture = Get-PcvRequiredMapValue -Map $installer -Name 'Architecture'
$installerType = Get-PcvRequiredMapValue -Map $installer -Name 'InstallerType'
$installerUrl = Get-PcvRequiredMapValue -Map $installer -Name 'InstallerUrl'
$installerSha256 = Get-PcvRequiredMapValue -Map $installer -Name 'InstallerSha256'

if ($manifestType -ne 'singleton') {
    throw "PCV_WINGET_MANIFEST_TYPE_UNSUPPORTED: $manifestType"
}

if ($manifestVersion -ne '1.12.0') {
    throw "PCV_WINGET_MANIFEST_VERSION_UNSUPPORTED: $manifestVersion"
}

if ($packageIdentifier -notmatch '^[A-Za-z0-9][A-Za-z0-9.-]+$') {
    throw "PCV_WINGET_PACKAGE_IDENTIFIER_INVALID: $packageIdentifier"
}

if ($packageVersion -notmatch '^\d+\.\d+\.\d+(\.\d+)?([-.+][A-Za-z0-9.-]+)?$') {
    throw "PCV_WINGET_PACKAGE_VERSION_INVALID: $packageVersion"
}

if ($installerUrl -notmatch '^https://') {
    throw "PCV_WINGET_INSTALLER_URL_NOT_HTTPS: $installerUrl"
}

if ($installerSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'PCV_WINGET_INSTALLER_SHA256_INVALID'
}

if ($installerType -ne 'msi') {
    throw "PCV_WINGET_INSTALLER_TYPE_UNSUPPORTED: $installerType"
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$normalized = [ordered]@{
    package_identifier = $packageIdentifier
    package_version = $packageVersion
    package_locale = $packageLocale
    publisher = $publisher
    package_name = $packageName
    license = $license
    short_description = $shortDescription
    manifest_type = $manifestType
    manifest_version = $manifestVersion
    installer = [ordered]@{
        architecture = $architecture
        installer_type = $installerType
        installer_url = $installerUrl
        installer_sha256 = $installerSha256
    }
}

$normalizedManifestPath = Join-Path $artifactRootFull 'winget-manifest.normalized.json'
$normalized | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $normalizedManifestPath -Encoding utf8

$complianceChecks = @(
    [ordered]@{ name = 'manifest-file-present'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'singleton-manifest-type'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'manifest-version-supported'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'package-identifier-present'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'package-version-winget-compatible'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'installer-url-https'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'installer-sha256-valid'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'installer-type-msi'; status = 'pass'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'winget-cli-validation-not-executed'; status = 'not-run'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'winget-submission-not-executed'; status = 'not-submitted'; public_claim = 'not-claimed' },
    [ordered]@{ name = 'public-claim-not-made'; status = 'not-claimed'; public_claim = 'not-claimed' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'winget-manifest-compliance-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    winget_submission = 'not-submitted'
    validation_status = 'offline-compliance-pass'
    manifest_path = $manifestFull
    normalized_manifest_path = $normalizedManifestPath
    compliance_checks = $complianceChecks
    package = [ordered]@{
        identifier = $packageIdentifier
        version = $packageVersion
        locale = $packageLocale
        publisher = $publisher
        name = $packageName
        license = $license
    }
    installer = [ordered]@{
        architecture = $architecture
        installer_type = $installerType
        installer_url = $installerUrl
        installer_sha256 = $installerSha256
    }
    command_plan = [ordered]@{
        winget_cli = [ordered]@{
            validation_status = 'not-run'
            submission_status = 'not-submitted'
        }
        publication = [ordered]@{
            public_trusted_signing = 'not-claimed'
            external_stable_publication = 'not-claimed'
        }
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 12
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
