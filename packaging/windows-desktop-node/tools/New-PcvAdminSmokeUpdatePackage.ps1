[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PackageRoot,
    [Parameter(Mandatory)][string]$Version,
    [string]$OutputRoot = '',
    [ValidateRange(0, 86400)][int]$BuildSeconds = -1
)

# Builds the admin-smoke update ZIP, its update catalog, and package-facts.json from one built package root.
# Reads the MSI read-only; installs nothing and changes no service or host state.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-PcvSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Read-PcvJson([string]$Path) { Get-Content -LiteralPath $Path -Raw -Encoding utf8 | ConvertFrom-Json -Depth 64 }
function Write-PcvJson([string]$Path, $Value) {
    [System.IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 32) + "`n", [System.Text.UTF8Encoding]::new($false))
}
function Invoke-PcvComMember($Target, [string]$Name, [string]$Kind, [object[]]$Arguments) {
    # Windows Installer automation rejects PSObject-wrapped arguments (DISP_E_TYPEMISMATCH): pass base objects or null.
    $plain = if ($Arguments.Count -eq 0) { $null } else { [object[]]@($Arguments | ForEach-Object { $_.psobject.BaseObject }) }
    $Target.GetType().InvokeMember($Name, [System.Reflection.BindingFlags]$Kind, $null, $Target, $plain)
}

if ($Version -notmatch '^(?<product>\d+\.\d+\.\d+)-admin-smoke$') { throw "PCV_UPDATE_PACKAGE_VERSION_INVALID|$Version" }
$msiProductVersion = $Matches.product
$root = (Resolve-Path -LiteralPath $PackageRoot -ErrorAction SilentlyContinue)
if ($null -eq $root) { throw "PCV_UPDATE_PACKAGE_ROOT_MISSING|$PackageRoot" }
$root = $root.Path
$out = if ($OutputRoot) { [System.IO.Path]::GetFullPath($OutputRoot) } else { $root }
$prefix = "PureCVisorDesktopNode-$Version"
$payloadRoot = Join-Path $root 'payload'
$msiPath = Join-Path $root "$prefix-windows-x64.msi"
$provenancePath = Join-Path $root "$prefix-windows-x64.provenance.json"
foreach ($required in @($payloadRoot, $msiPath, $provenancePath, (Join-Path $payloadRoot 'product-manifest.json'))) {
    if (-not (Test-Path -LiteralPath $required)) { throw "PCV_UPDATE_PACKAGE_INPUT_MISSING|$required" }
}

$manifest = Read-PcvJson (Join-Path $payloadRoot 'product-manifest.json')
$provenance = Read-PcvJson $provenancePath
if ([string]$manifest.version -ne $Version) { throw "PCV_UPDATE_PACKAGE_MANIFEST_MISMATCH|$($manifest.version)" }
$msiSha256 = Get-PcvSha256 $msiPath
if ($msiSha256 -ne [string]$provenance.msi.sha256) { throw 'PCV_UPDATE_PACKAGE_MSI_HASH_MISMATCH' }

$zipPath = Join-Path $out "$prefix-update.zip"
$catalogPath = Join-Path $out "$prefix-update-catalog.json"
$factsPath = Join-Path $out 'package-facts.json'
foreach ($output in @($zipPath, $catalogPath, $factsPath)) {
    if (Test-Path -LiteralPath $output) { throw "PCV_UPDATE_PACKAGE_OUTPUT_EXISTS|$output" }
}

# MSI Upgrade table and RemoveExistingProducts sequence, read-only (open mode 0).
$installer = New-Object -ComObject WindowsInstaller.Installer
$database = Invoke-PcvComMember $installer 'OpenDatabase' 'InvokeMethod' @($msiPath, 0)
$upgradeRows = @()
$view = Invoke-PcvComMember $database 'OpenView' 'InvokeMethod' @('SELECT VersionMin, VersionMax, Attributes FROM Upgrade')
[void](Invoke-PcvComMember $view 'Execute' 'InvokeMethod' @())
while ($null -ne ($record = Invoke-PcvComMember $view 'Fetch' 'InvokeMethod' @())) {
    $upgradeRows += [ordered]@{
        version_min = [string](Invoke-PcvComMember $record 'StringData' 'GetProperty' @(1))
        version_max = [string](Invoke-PcvComMember $record 'StringData' 'GetProperty' @(2))
        attributes = [int](Invoke-PcvComMember $record 'IntegerData' 'GetProperty' @(3))
    }
}
[void](Invoke-PcvComMember $view 'Close' 'InvokeMethod' @())
$view = Invoke-PcvComMember $database 'OpenView' 'InvokeMethod' @("SELECT Sequence FROM InstallExecuteSequence WHERE Action='RemoveExistingProducts'")
[void](Invoke-PcvComMember $view 'Execute' 'InvokeMethod' @())
$record = Invoke-PcvComMember $view 'Fetch' 'InvokeMethod' @()
$removeExistingProducts = if ($null -eq $record) { $null } else { [int](Invoke-PcvComMember $record 'IntegerData' 'GetProperty' @(1)) }
[void](Invoke-PcvComMember $view 'Close' 'InvokeMethod' @())
if (-not @($upgradeRows | Where-Object { $_.version_max -eq $msiProductVersion }).Count -or $null -eq $removeExistingProducts) {
    throw 'PCV_UPDATE_PACKAGE_UPGRADE_TABLE_INVALID'
}

# Update ZIP: payload files at the root, sorted, deflate, '/' separators.
New-Item -ItemType Directory -Force -Path $out | Out-Null
$files = @(Get-ChildItem -LiteralPath $payloadRoot -Recurse -File | ForEach-Object {
        [pscustomobject]@{ full = $_.FullName; entry = [System.IO.Path]::GetRelativePath($payloadRoot, $_.FullName).Replace('\', '/') }
    } | Sort-Object { $_.entry.ToLowerInvariant() })
$archive = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in $files) {
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $file.full, $file.entry, [System.IO.Compression.CompressionLevel]::Optimal)
    }
} finally {
    $archive.Dispose()
}
$zipSha256 = Get-PcvSha256 $zipPath

$catalog = [ordered]@{
    schema_version = 1
    product = 'PureCVisor Desktop Node'
    generated_utc = (Get-Date).ToUniversalTime().ToString('o')
    publication = [ordered]@{ public_trusted_signing = 'not-claimed'; external_stable_publication = 'not-claimed' }
    channels = @(
        [ordered]@{
            name = 'admin-smoke'; channel = 'admin-smoke'; release_channel = 'admin-smoke'; version = $Version
            signing_mode = 'AllowUnsignedDev'; source_uri = ([uri]$zipPath).AbsoluteUri; expected_sha256 = $zipSha256
        }
    )
}
Write-PcvJson $catalogPath $catalog

$facts = [ordered]@{
    schema_version = 1
    contract = 'pcv-admin-smoke-package-facts-v1'
    version = $Version
    msi_product_version = $msiProductVersion
    provenance_commit = [string]$provenance.git_commit
    build_utc = [string]$provenance.build_utc
    build_seconds = if ($BuildSeconds -ge 0) { $BuildSeconds } else { $null }
    wix_version = [string]$provenance.wix.version
    msi_sha256 = $msiSha256
    payload_aggregate_sha256 = [string]$provenance.payload.aggregate_sha256
    payload_file_count = [int]$provenance.payload.file_count
    product_wrapper_sha256 = [string]$provenance.payload.product_wrapper_sha256
    service_host_sha256 = [string]$provenance.service_host.sha256
    cli_sha256 = [string]$provenance.cli.sha256
    host_product_version = (Get-Item -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe')).VersionInfo.ProductVersion
    cli_product_version = (Get-Item -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe')).VersionInfo.ProductVersion
    manifest_version = [string]$manifest.version
    update_zip_sha256 = $zipSha256
    update_zip_entries = @($files | ForEach-Object entry)
    update_catalog_sha256 = Get-PcvSha256 $catalogPath
    msi_upgrade_rows = $upgradeRows
    remove_existing_products_sequence = $removeExistingProducts
    host_mutation_performed = $false
}
Write-PcvJson $factsPath $facts

[ordered]@{
    ok = $true
    version = $Version
    update_zip = $zipPath
    update_zip_sha256 = $zipSha256
    update_catalog = $catalogPath
    package_facts = $factsPath
    entry_count = $files.Count
    host_mutation_performed = $false
} | ConvertTo-Json -Depth 8
