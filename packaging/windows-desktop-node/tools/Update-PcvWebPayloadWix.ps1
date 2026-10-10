#requires -Version 7.0
<#
.SYNOPSIS
Generates packaging/windows-desktop-node/installer/WebPayload.wxs from web/payload-manifest.json (ADR-0018).

.DESCRIPTION
The web root of the Single Edge frontend structure ships many files (vendor fonts and icons, PWA manifest and
service worker, samples, guide content). Product.wxs keeps explicit components only for the manifest "core" files;
every other manifest entry becomes one Component/File pair in the generated WebPayload.wxs fragment, with nested
Directory elements under DesktopNodeWebFolder for subdirectories. Component GUIDs are auto-generated ("*"), which
WiX derives from the install path, so the fragment is stable across runs.

-Check compares the committed fragment with the generated text and exits 1 when stale. -Apply writes it.
Output is one JSON object, like the other packaging tools.
#>
[CmdletBinding()]
param(
    [switch]$Check,
    [switch]$Apply,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Check -eq $Apply) {
    [pscustomobject]@{ schema_version = 1; ok = $false; error = 'PCV_WEB_PAYLOAD_WIX_USAGE|Pass exactly one of -Check or -Apply.' } | ConvertTo-Json -Compress
    exit 2
}

$manifestPath = Join-Path $RepositoryRoot 'web\payload-manifest.json'
$fragmentPath = Join-Path $RepositoryRoot 'packaging\windows-desktop-node\installer\WebPayload.wxs'
$contract = 'pcv-web-payload-manifest-v1'

function ConvertTo-PcvWixId {
    param([string]$Prefix, [string]$RelativePath)
    $sanitized = [regex]::Replace($RelativePath, '[^A-Za-z0-9_]', '_')
    $id = "$Prefix$sanitized"
    if ($id.Length -gt 64) {
        $sha = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($RelativePath))
        $tail = ([System.Convert]::ToHexString($sha)).Substring(0, 8)
        $id = $id.Substring(0, 55) + '_' + $tail
    }
    return $id
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ([string]$manifest.contract -ne $contract) {
    [pscustomobject]@{ schema_version = 1; ok = $false; error = "PCV_WEB_PAYLOAD_MANIFEST_INVALID|web/payload-manifest.json contract must be $contract." } | ConvertTo-Json -Compress
    exit 1
}

$core = @($manifest.core | ForEach-Object { [string]$_ })
$files = @($manifest.files | ForEach-Object { [string]$_ })
foreach ($entry in ($core + $files)) {
    if ($entry -notmatch '^[A-Za-z0-9][A-Za-z0-9._/-]*$' -or $entry.Contains('..') -or $entry.StartsWith('/')) {
        [pscustomobject]@{ schema_version = 1; ok = $false; error = "PCV_WEB_PAYLOAD_MANIFEST_INVALID|Invalid payload entry '$entry'." } | ConvertTo-Json -Compress
        exit 1
    }
}
$duplicates = @(($core + $files) | Group-Object | Where-Object { $_.Count -gt 1 } | ForEach-Object { $_.Name })
if ($duplicates.Count -gt 0) {
    [pscustomobject]@{ schema_version = 1; ok = $false; error = "PCV_WEB_PAYLOAD_MANIFEST_INVALID|Duplicate payload entries: $($duplicates -join ', ')." } | ConvertTo-Json -Compress
    exit 1
}

$generated = @($files | Sort-Object -Culture 'en-US' -CaseSensitive)

# Directory tree for subdirectories.
$directoryIds = [ordered]@{}
foreach ($entry in $generated) {
    $segments = $entry.Split('/')
    for ($depth = 1; $depth -lt $segments.Count; $depth++) {
        $dirPath = ($segments[0..($depth - 1)] -join '/')
        if (-not $directoryIds.Contains($dirPath)) {
            $directoryIds[$dirPath] = ConvertTo-PcvWixId -Prefix 'DesktopNodeWebDir_' -RelativePath $dirPath
        }
    }
}

function Write-PcvWixDirectory {
    param([string]$Parent, [int]$Indent, [System.Collections.Generic.List[string]]$Lines)
    $children = @($directoryIds.Keys | Where-Object {
        $segments = $_.Split('/')
        $parentPath = if ($segments.Count -gt 1) { ($segments[0..($segments.Count - 2)] -join '/') } else { '' }
        $parentPath -eq $Parent
    } | Sort-Object -Culture 'en-US' -CaseSensitive)
    foreach ($child in $children) {
        $name = $child.Split('/')[-1]
        $pad = ' ' * $Indent
        $Lines.Add("$pad<Directory Id=`"$($directoryIds[$child])`" Name=`"$name`">")
        Write-PcvWixDirectory -Parent $child -Indent ($Indent + 2) -Lines $Lines
        $Lines.Add("$pad</Directory>")
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('<?xml version="1.0" encoding="UTF-8"?>')
$lines.Add('<!-- Generated from web/payload-manifest.json by packaging/windows-desktop-node/tools/Update-PcvWebPayloadWix.ps1 (ADR-0018). Do not edit; run the tool with -Apply. -->')
$lines.Add('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
$lines.Add('  <Fragment>')
if ($directoryIds.Count -gt 0) {
    $lines.Add('    <DirectoryRef Id="DesktopNodeWebFolder">')
    Write-PcvWixDirectory -Parent '' -Indent 6 -Lines $lines
    $lines.Add('    </DirectoryRef>')
}
$lines.Add('    <ComponentGroup Id="DesktopNodeWebPayloadComponents">')
foreach ($entry in $generated) {
    $segments = $entry.Split('/')
    $directoryId = if ($segments.Count -gt 1) { $directoryIds[($segments[0..($segments.Count - 2)] -join '/')] } else { 'DesktopNodeWebFolder' }
    $id = ConvertTo-PcvWixId -Prefix 'DesktopNodeWebPayload_' -RelativePath $entry
    $source = '$(var.PayloadRoot)\web\' + $entry.Replace('/', '\')
    $lines.Add("      <Component Id=`"${id}Component`" Directory=`"$directoryId`" Guid=`"*`">")
    $lines.Add("        <File Id=`"$id`" Source=`"$source`" KeyPath=`"yes`" />")
    $lines.Add('      </Component>')
}
$lines.Add('    </ComponentGroup>')
$lines.Add('  </Fragment>')
$lines.Add('</Wix>')
$text = ($lines -join "`n") + "`n"

$current = if (Test-Path -LiteralPath $fragmentPath -PathType Leaf) { Get-Content -LiteralPath $fragmentPath -Raw } else { $null }
$status = if ($current -eq $text) { 'current' } else { 'stale' }

if ($Apply) {
    if ($status -ne 'current') {
        [System.IO.File]::WriteAllText($fragmentPath, $text, [System.Text.UTF8Encoding]::new($false))
    }
    [pscustomobject]@{ schema_version = 1; ok = $true; mode = 'apply'; status = $status; core_count = $core.Count; generated_count = $generated.Count; directory_count = $directoryIds.Count; fragment = 'packaging/windows-desktop-node/installer/WebPayload.wxs' } | ConvertTo-Json -Compress
    exit 0
}

$ok = $status -eq 'current'
[pscustomobject]@{ schema_version = 1; ok = $ok; mode = 'check'; status = $status; core_count = $core.Count; generated_count = $generated.Count; directory_count = $directoryIds.Count; fragment = 'packaging/windows-desktop-node/installer/WebPayload.wxs' } | ConvertTo-Json -Compress
exit $(if ($ok) { 0 } else { 1 })
