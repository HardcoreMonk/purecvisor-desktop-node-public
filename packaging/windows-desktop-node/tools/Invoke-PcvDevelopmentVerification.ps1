[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Fast', 'Full', 'Release')]
    [string]$Lane,

    [Parameter(Mandatory)]
    [ValidateSet('S', 'M', 'L')]
    [string]$ChangeTier,

    [string]$BaseRef = 'origin/main',

    [string[]]$ChangedPath = @(),

    [Parameter(Mandatory)]
    [string]$ArtifactRoot,

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$modulePath = Join-Path $PSScriptRoot 'PcvDevelopmentVerificationRunner.psm1'
Import-Module $modulePath -Force

try {
    $resolvedPaths = @($ChangedPath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($resolvedPaths.Count -eq 0) {
        $resolvedPaths = @(
            & git -C $repoRoot diff --name-only "$BaseRef...HEAD" |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
        if ($LASTEXITCODE -ne 0) {
            throw "PCV_DEVELOPMENT_VERIFICATION_GIT_DIFF_FAILED|base_ref=$BaseRef"
        }
    }
    if ($resolvedPaths.Count -eq 0) {
        throw 'PCV_DEVELOPMENT_VERIFICATION_NO_CHANGED_PATHS'
    }

    $result = Invoke-PcvDevelopmentVerification `
        -Lane $Lane `
        -ChangeTier $ChangeTier `
        -ChangedPath $resolvedPaths `
        -WorkingDirectory $repoRoot `
        -PlanOnly:$PlanOnly

    $resolvedArtifactRoot = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
        $ArtifactRoot
    }
    else {
        Join-Path $repoRoot $ArtifactRoot
    }
    New-Item -ItemType Directory -Path $resolvedArtifactRoot -Force | Out-Null
    $summaryPath = Join-Path $resolvedArtifactRoot 'summary.json'
    $result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
    Write-Output $resolvedArtifactRoot
    if (-not [bool]$result.ok) {
        exit 1
    }
}
catch {
    [pscustomobject]([ordered]@{
        schema_version = 1
        ok = $false
        error = [string]$_
    }) | ConvertTo-Json -Depth 6 -Compress
    exit 1
}
