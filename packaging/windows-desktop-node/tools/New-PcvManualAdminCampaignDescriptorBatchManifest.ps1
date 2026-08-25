[CmdletBinding()]
param(
    [string]$RepoRoot = (Get-Location).Path,
    [string]$BatchId = 'manual-admin-campaign-descriptor-20260512-0425-0426',
    [string]$ArtifactRoot = '',
    [string]$ManifestPath = '',
    [string]$CampaignArtifactRoot = 'artifacts/manual-admin-campaign-20260512-0425-0426',
    [string]$DescriptorArtifactRoot = '',
    [string]$BaselineVersion = '0.42.5-admin-smoke',
    [string]$TargetVersion = '0.42.6-admin-smoke',
    [string]$ReadinessSummaryPath = '',
    [string]$ProductUpdateSummaryPath = '',
    [string]$ProductRollbackSummaryPath = '',
    [string]$CleanHostSummaryPath = '',
    [string]$BurnLifecycleSummaryPath = '',
    [string]$MsixLifecycleSummaryPath = 'artifacts/msix-package-lifecycle-smoke-20260512-0425-0426/summary.json',
    [string]$InstalledRuntimeOpsSummaryPath = '',
    [switch]$PassThru
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvBatchSupervisor.psm1'
Import-Module $modulePath -Force

if ([string]::IsNullOrWhiteSpace($ArtifactRoot)) {
    $ArtifactRoot = Join-Path 'artifacts/batch-runs' $BatchId
}
if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $ArtifactRoot 'manifest.json'
}
if ([string]::IsNullOrWhiteSpace($DescriptorArtifactRoot)) {
    $DescriptorArtifactRoot = Join-Path $CampaignArtifactRoot 'manual-admin-campaign-descriptor'
}
if ([string]::IsNullOrWhiteSpace($ReadinessSummaryPath)) {
    $ReadinessSummaryPath = Join-Path $CampaignArtifactRoot 'manual-admin-rebaseline-readiness-r2/summary.json'
}
if ([string]::IsNullOrWhiteSpace($ProductUpdateSummaryPath)) {
    $ProductUpdateSummaryPath = Join-Path $CampaignArtifactRoot 'lifecycle/product-update-rollback/update-0425-to-0426-summary.json'
}
if ([string]::IsNullOrWhiteSpace($ProductRollbackSummaryPath)) {
    $ProductRollbackSummaryPath = Join-Path $CampaignArtifactRoot 'lifecycle/product-update-rollback/rollback-0426-to-0425-summary.json'
}
if ([string]::IsNullOrWhiteSpace($CleanHostSummaryPath)) {
    $CleanHostSummaryPath = Join-Path $CampaignArtifactRoot 'clean-host/summary.json'
}
if ([string]::IsNullOrWhiteSpace($BurnLifecycleSummaryPath)) {
    $BurnLifecycleSummaryPath = Join-Path $CampaignArtifactRoot 'burn-bootstrapper-lifecycle-r2/burn-lifecycle-summary.json'
}
if ([string]::IsNullOrWhiteSpace($InstalledRuntimeOpsSummaryPath)) {
    $InstalledRuntimeOpsSummaryPath = Join-Path $CampaignArtifactRoot 'installed-runtime-ops-summary/summary.json'
}

$profileOptions = [ordered]@{
    descriptor_artifact_root = $DescriptorArtifactRoot
    campaign_artifact_root = $CampaignArtifactRoot
    baseline_version = $BaselineVersion
    target_version = $TargetVersion
    readiness_summary_path = $ReadinessSummaryPath
    product_update_summary_path = $ProductUpdateSummaryPath
    product_rollback_summary_path = $ProductRollbackSummaryPath
    clean_host_summary_path = $CleanHostSummaryPath
    burn_lifecycle_summary_path = $BurnLifecycleSummaryPath
    msix_lifecycle_summary_path = $MsixLifecycleSummaryPath
    installed_runtime_ops_summary_path = $InstalledRuntimeOpsSummaryPath
}

$manifest = New-PcvBatchSupervisorManifest `
    -BatchId $BatchId `
    -RepoRoot $RepoRoot `
    -ArtifactRoot $ArtifactRoot `
    -Profile ManualAdminCampaignDescriptor `
    -ProfileOptions $profileOptions

$saved = Save-PcvBatchSupervisorManifest -Manifest $manifest -Path $ManifestPath

if ($PassThru) {
    [pscustomobject]([ordered]@{
        ok = $true
        profile = 'ManualAdminCampaignDescriptor'
        batch_id = $manifest.batch_id
        manifest_path = $saved.path
        host_mutation_performed = $false
        requires_admin = $false
        mutates_host = $false
    })
}
