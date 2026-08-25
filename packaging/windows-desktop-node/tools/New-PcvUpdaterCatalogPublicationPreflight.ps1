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
    [string]$PublicCatalogUri,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [ValidateNotNullOrEmpty()]
    [string]$DownloadRoot = '%ProgramData%\PureCVisor\desktop-node\updates',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_UPDATER_CATALOG_PUBLICATION_PREFLIGHT_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the publication preflight descriptor.'
}

function Get-PcvPropertyValue {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        throw "PCV_UPDATER_CATALOG_PUBLICATION_INVALID: missing $Path.$Name"
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
    throw "PCV_UPDATER_CATALOG_PUBLICATION_CATALOG_NOT_FOUND: $catalogFull"
}

Assert-PcvHttpsUri -Value $PublicCatalogUri -Code 'PCV_UPDATER_CATALOG_PUBLICATION_PUBLIC_URI_UNTRUSTED'

$catalog = Get-Content -Raw -LiteralPath $catalogFull | ConvertFrom-Json
$schemaVersion = Get-PcvPropertyValue -Object $catalog -Name 'schema_version' -Path 'catalog'
if ([string]$schemaVersion -ne '1') {
    throw "PCV_UPDATER_CATALOG_PUBLICATION_SCHEMA_UNSUPPORTED: $schemaVersion"
}

$product = Get-PcvPropertyValue -Object $catalog -Name 'product' -Path 'catalog'
$publication = Get-PcvPropertyValue -Object $catalog -Name 'publication' -Path 'catalog'
$channels = @(Get-PcvPropertyValue -Object $catalog -Name 'channels' -Path 'catalog')

$publicSigning = Get-PcvPropertyValue -Object $publication -Name 'public_trusted_signing' -Path 'catalog.publication'
$externalPublication = Get-PcvPropertyValue -Object $publication -Name 'external_stable_publication' -Path 'catalog.publication'
$catalogPublication = Get-PcvPropertyValue -Object $publication -Name 'catalog_publication' -Path 'catalog.publication'

if ($publicSigning -ne 'not-claimed' -or $externalPublication -ne 'not-claimed' -or $catalogPublication -ne 'not-published') {
    throw 'PCV_UPDATER_CATALOG_PUBLICATION_CLAIMED: preflight only accepts not-claimed and not-published catalog descriptors.'
}

$selectedChannel = $channels | Where-Object {
    $name = Get-PcvPropertyValue -Object $_ -Name 'name' -Path 'catalog.channels[]'
    $name -eq $Channel
} | Select-Object -First 1

if ($null -eq $selectedChannel) {
    throw "PCV_UPDATER_CATALOG_PUBLICATION_CHANNEL_NOT_FOUND: $Channel"
}

$selectedVersion = Get-PcvPropertyValue -Object $selectedChannel -Name 'version' -Path 'catalog.channels[]'
$packageUri = Get-PcvPropertyValue -Object $selectedChannel -Name 'package_uri' -Path 'catalog.channels[]'
$sha256 = Get-PcvPropertyValue -Object $selectedChannel -Name 'sha256' -Path 'catalog.channels[]'

Assert-PcvHttpsUri -Value $packageUri -Code 'PCV_UPDATER_CATALOG_PUBLICATION_PACKAGE_URI_UNTRUSTED'

if ($sha256 -notmatch '^[A-Fa-f0-9]{64}$') {
    throw 'PCV_UPDATER_CATALOG_PUBLICATION_PACKAGE_SHA256_INVALID'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$catalogPreviewRoot = Join-Path $artifactRootFull 'catalog'
$previewCatalogPath = Join-Path $catalogPreviewRoot 'purecvisor-desktop-node-catalog.publication-preview.json'
New-Item -ItemType Directory -Force -Path $catalogPreviewRoot | Out-Null

$previewCatalog = [ordered]@{
    schema_version = 1
    product = $product
    publication = [ordered]@{
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        catalog_publication = 'not-published'
    }
    channels = @(
        [ordered]@{
            name = Get-PcvPropertyValue -Object $selectedChannel -Name 'name' -Path 'catalog.channels[]'
            version = $selectedVersion
            package_uri = $packageUri
            sha256 = $sha256
            release_channel = if ($selectedChannel.PSObject.Properties['release_channel']) { $selectedChannel.release_channel } else { $Channel }
            signing_mode = if ($selectedChannel.PSObject.Properties['signing_mode']) { $selectedChannel.signing_mode } else { 'RequireSigned' }
        }
    )
}

$previewCatalog | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $previewCatalogPath -Encoding utf8
$previewShaPath = Join-Path $catalogPreviewRoot 'purecvisor-desktop-node-catalog.publication-preview.sha256'
$previewSha256 = (Get-FileHash -LiteralPath $previewCatalogPath -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath $previewShaPath -Value $previewSha256 -Encoding utf8

$publicationChecks = @(
    [ordered]@{ name = 'catalog-schema-v1'; status = 'pass' },
    [ordered]@{ name = 'selected-channel-present'; status = 'pass' },
    [ordered]@{ name = 'catalog-uri-https'; status = 'pass' },
    [ordered]@{ name = 'package-uri-https'; status = 'pass' },
    [ordered]@{ name = 'package-sha256-present'; status = 'pass' },
    [ordered]@{ name = 'public-claim-not-made'; status = 'pass' },
    [ordered]@{ name = 'publication-not-executed'; status = 'pass' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'updater-catalog-publication-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    catalog_publication = 'not-published'
    catalog_path = $catalogFull
    channel = $Channel
    public_catalog_uri = $PublicCatalogUri
    preview_catalog_path = $previewCatalogPath
    preview_catalog_sha256_path = $previewShaPath
    preview_catalog_sha256 = $previewSha256
    selected_channel = [ordered]@{
        name = $Channel
        version = $selectedVersion
        package_uri = $packageUri
        sha256 = $sha256
    }
    publication_checks = @($publicationChecks)
    command_plan = [ordered]@{
        update = [ordered]@{
            command = 'Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri <public-catalog-uri> -UpdateChannel <channel> -DownloadRoot <download-root>'
            update_catalog_uri = $PublicCatalogUri
            update_channel = $Channel
            download_root = $DownloadRoot
            dry_run_only = $true
            actual_execution = 'not-run'
        }
        publication = [ordered]@{
            upload = 'not-run'
            external_stable_publication = 'not-claimed'
            public_trusted_signing = 'not-claimed'
        }
    }
    source_catalog = [ordered]@{
        schema_version = $schemaVersion
        product = $product
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
