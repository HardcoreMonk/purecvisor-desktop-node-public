[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("internal-clean-host-install-update-rollback-readiness-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$CleanHostRunnerPath,
    [string]$InternalCatalogPath,
    [string]$BaselineVersion = '0.38.8-admin-smoke',
    [string]$TargetVersion = '0.39.1-admin-smoke'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

function Test-ExistingFile {
    param([string]$Path)
    -not [string]::IsNullOrWhiteSpace($Path) -and (Test-Path -LiteralPath $Path -PathType Leaf)
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding UTF8
}

$runnerPresent = Test-ExistingFile -Path $CleanHostRunnerPath
$catalogPresent = Test-ExistingFile -Path $InternalCatalogPath
$status = if ($runnerPresent) {
    'ready-for-internal-clean-host-run-not-run'
}
else {
    'blocked-by-missing-clean-host-runner'
}

$plan = [ordered]@{
    scope = 'internal-clean-host-install-update-rollback-readiness'
    baseline_version = $BaselineVersion
    target_version = $TargetVersion
    clean_host_runner_path = if ([string]::IsNullOrWhiteSpace($CleanHostRunnerPath)) { $null } else { $CleanHostRunnerPath }
    clean_host_runner_present = $runnerPresent
    internal_catalog_path = if ([string]::IsNullOrWhiteSpace($InternalCatalogPath)) { $null } else { $InternalCatalogPath }
    internal_catalog_present = $catalogPresent
    required_steps = @(
        'provision-clean-windows-host-or-vm',
        'install-internal-signed-msi',
        'read-internal-updater-catalog-channel',
        'apply-update-package',
        'verify-service-health-and-web-console',
        'rollback-to-baseline',
        'verify-final-service-health',
        'capture-no-public-release-claim-and-token-redaction'
    )
}

$summary = [ordered]@{
    ok = $runnerPresent
    scope = 'internal-clean-host-install-update-rollback-readiness'
    actual_execution = 'local-internal-clean-host-prerequisite-scan'
    artifact_root = $artifactRootFull
    host_mutation_performed = $false
    internal_clean_host_install_update_rollback_smoke = $status
    clean_host_runner_present = $runnerPresent
    internal_catalog_present = $catalogPresent
    baseline_version = $BaselineVersion
    target_version = $TargetVersion
    public_trusted_signing = 'out-of-scope'
    external_stable_publication = 'out-of-scope'
    winget_submission = 'out-of-scope'
    public_release = 'not-claimed'
    blocker = if ($runnerPresent) { 'clean-host runner supplied; execution intentionally not run by readiness descriptor' } else { 'missing internal clean-host runner path' }
    next_required_evidence = 'internal clean host install, internal updater catalog update, rollback, final service health, and no public release claim'
    plan = $plan
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$planPath = Join-Path $artifactRootFull 'internal-clean-host-plan.json'
Write-JsonFile -Path $summaryPath -Value $summary
Write-JsonFile -Path $planPath -Value $plan

$summary
