[CmdletBinding()]
param(
    [string]$RepoRoot,
    [string]$LedgerPath,
    [string]$DocPath,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# 계약 'pcv-feature-ledger-doc-v1': docs/FEATURE_IMPLEMENTATION_LEDGER.md 의 "Feature ID 요약" 표를
# config/desktop-node-feature-surface-ledger.json 에서 생성한다(2026-10-09 감사 §9, ADR-0017 §2.4). 마커 사이만 바꾸고
# 나머지 본문은 손대지 않는다. -Check 는 아무것도 쓰지 않고 현재 표가 계약과 같은지만 대조한다(stale 이면 exit 1).
# Required CI 는 PowerShell 을 돌리지 않으므로 같은 렌더링을 Delivery 시험 PcvFeatureLedgerDocContractTests 가 대조한다.

$script:Contract = 'pcv-feature-ledger-doc-v1'
$script:BeginMarker = '<!-- BEGIN GENERATED FEATURE ID SUMMARY -->'
$script:EndMarker = '<!-- END GENERATED FEATURE ID SUMMARY -->'

function Get-PcvFeatureLedgerSurfaceCount {
    param(
        [Parameter(Mandatory)][System.Text.Json.JsonElement]$Feature,
        [Parameter(Mandatory)][string]$Surface
    )

    $present = 0
    $excluded = 0
    foreach ($route in $Feature.GetProperty('routes').EnumerateArray()) {
        foreach ($name in $route.GetProperty('present_surfaces').EnumerateArray()) {
            if ($name.GetString() -ceq $Surface) { $present++ }
        }
        foreach ($exclusion in $route.GetProperty('excluded_surfaces').EnumerateArray()) {
            if ($exclusion.GetProperty('surface').GetString() -ceq $Surface) { $excluded++ }
        }
    }
    '{0} present / {1} excluded' -f $present, $excluded
}

function ConvertTo-PcvFeatureLedgerTable {
    param([Parameter(Mandatory)][System.Text.Json.JsonDocument]$Ledger)

    $root = $Ledger.RootElement
    if ($root.GetProperty('contract').GetString() -cne 'pcv-feature-surface-ledger-v1') {
        throw 'PCV_FEATURE_LEDGER_DOC_INVALID|contract'
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add($script:BeginMarker)
    $lines.Add('| Feature ID | Title | Routes | Web | CLI |')
    $lines.Add('|---|---|---:|---:|---:|')
    foreach ($feature in $root.GetProperty('features').EnumerateArray()) {
        $id = $feature.GetProperty('feature_id').GetString()
        $row = '| <a id="{0}"></a>`{1}` | {2} | {3} | {4} | {5} |' -f `
            $id.Replace('.', '-'),
            $id,
            $feature.GetProperty('title').GetString(),
            $feature.GetProperty('routes').GetArrayLength(),
            (Get-PcvFeatureLedgerSurfaceCount -Feature $feature -Surface 'web'),
            (Get-PcvFeatureLedgerSurfaceCount -Feature $feature -Surface 'cli')
        $lines.Add($row)
    }
    $lines.Add($script:EndMarker)
    $lines -join "`n"
}

function Update-PcvFeatureLedgerDocument {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Block,
        [switch]$Check
    )

    $original = Get-Content -Raw -LiteralPath $Path
    $normalized = $original.Replace("`r`n", "`n")
    if ([regex]::Matches($normalized, [regex]::Escape($script:BeginMarker)).Count -ne 1 -or
        [regex]::Matches($normalized, [regex]::Escape($script:EndMarker)).Count -ne 1) {
        throw "PCV_FEATURE_LEDGER_DOC_MARKERS_INVALID|$Path"
    }

    $beginIndex = $normalized.IndexOf($script:BeginMarker, [System.StringComparison]::Ordinal)
    $endIndex = $normalized.IndexOf($script:EndMarker, [System.StringComparison]::Ordinal)
    if ($beginIndex -lt 0 -or $endIndex -le $beginIndex) {
        throw "PCV_FEATURE_LEDGER_DOC_MARKERS_INVALID|$Path"
    }
    $expected = $normalized.Substring(0, $beginIndex) + $Block + $normalized.Substring($endIndex + $script:EndMarker.Length)

    if ($expected -ceq $normalized) {
        return [pscustomobject]@{ path = $Path; status = 'current' }
    }
    if ($Check) {
        throw "PCV_FEATURE_LEDGER_DOC_STALE|$Path"
    }

    $newline = if ($original.Contains("`r`n")) { "`r`n" } else { "`n" }
    $temporaryPath = "$Path.tmp"
    try {
        [System.IO.File]::WriteAllText($temporaryPath, $expected.Replace("`n", $newline), [System.Text.UTF8Encoding]::new($false))
        Move-Item -LiteralPath $temporaryPath -Destination $Path -Force
    }
    finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }
    }
    [pscustomobject]@{ path = $Path; status = 'updated' }
}

function Resolve-PcvFeatureLedgerPath {
    param(
        [Parameter(Mandatory)][string]$Root,
        [string]$Value,
        [Parameter(Mandatory)][string]$Default
    )

    if ([string]::IsNullOrWhiteSpace($Value)) { return (Join-Path $Root $Default) }
    if ([System.IO.Path]::IsPathRooted($Value)) { return $Value }
    Join-Path $Root $Value
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        $resolvedRepoRoot = if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
            (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        }
        else {
            (Resolve-Path -LiteralPath $RepoRoot).Path
        }
        $resolvedLedgerPath = Resolve-PcvFeatureLedgerPath -Root $resolvedRepoRoot -Value $LedgerPath -Default 'config/desktop-node-feature-surface-ledger.json'
        $resolvedDocPath = Resolve-PcvFeatureLedgerPath -Root $resolvedRepoRoot -Value $DocPath -Default 'docs/FEATURE_IMPLEMENTATION_LEDGER.md'

        $ledger = [System.Text.Json.JsonDocument]::Parse([System.IO.File]::ReadAllText($resolvedLedgerPath))
        try {
            $block = ConvertTo-PcvFeatureLedgerTable -Ledger $ledger
        }
        finally {
            $ledger.Dispose()
        }
        $result = Update-PcvFeatureLedgerDocument -Path $resolvedDocPath -Block $block -Check:$Check
        [pscustomobject]([ordered]@{
            schema_version = 1
            contract = $script:Contract
            ok = $true
            check = [bool]$Check
            source = $resolvedLedgerPath
            target = $resolvedDocPath
            status = $result.status
        }) | ConvertTo-Json -Depth 4 -Compress
    }
    catch {
        [pscustomobject]([ordered]@{
            schema_version = 1
            contract = $script:Contract
            ok = $false
            check = [bool]$Check
            error = [string]$_
        }) | ConvertTo-Json -Depth 4 -Compress
        exit 1
    }
}
