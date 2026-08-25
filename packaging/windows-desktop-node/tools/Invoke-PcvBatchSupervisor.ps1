param(
    [Parameter(Mandatory)][string]$ManifestPath,
    [switch]$DryRun,
    [switch]$Resume,
    [switch]$AllowHostMutation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvBatchSupervisor.psm1'
Import-Module $modulePath -Force

try {
    $manifest = Get-Content -Raw -LiteralPath $ManifestPath -ErrorAction Stop | ConvertFrom-Json -Depth 32 -ErrorAction Stop
    $result = Invoke-PcvBatchSupervisor `
        -Manifest $manifest `
        -DryRun:$DryRun `
        -Resume:$Resume `
        -AllowHostMutation:$AllowHostMutation
    $result | ConvertTo-Json -Depth 32
    if ([bool]$result.ok) {
        exit 0
    }
    exit 1
}
catch {
    [ordered]@{
        ok = $false
        error = [string]$_
    } | ConvertTo-Json -Depth 8
    exit 1
}
