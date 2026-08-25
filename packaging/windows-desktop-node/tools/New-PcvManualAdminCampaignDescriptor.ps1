[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("manual-admin-campaign-descriptor-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$CampaignArtifactRoot = '',
    [string]$BaselineVersion = '',
    [string]$TargetVersion = '',
    [string]$ReadinessSummaryPath = '',
    [string]$ProductUpdateSummaryPath = '',
    [string]$ProductRollbackSummaryPath = '',
    [string]$CleanHostSummaryPath = '',
    [string]$BurnLifecycleSummaryPath = '',
    [string]$MsixLifecycleSummaryPath = '',
    [string]$InstalledRuntimeOpsSummaryPath = '',
    [string]$DescriptorBatchId = '',
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly) {
    throw 'PCV_MANUAL_ADMIN_CAMPAIGN_DESCRIPTOR_PLAN_ONLY_REQUIRED|Manual-admin campaign descriptor only reads local evidence. Pass -PlanOnly.'
}

function Resolve-PcvPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ''
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Read-PcvJsonFile {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Write-PcvJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    $Value | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Test-PcvEvidencePass {
    param($Summary)

    if ($null -eq $Summary) {
        return $false
    }

    if ($Summary.PSObject.Properties.Name -contains 'ok' -and [bool]$Summary.ok) {
        return $true
    }

    if ($Summary.PSObject.Properties.Name -contains 'exit_code' -and [int]$Summary.exit_code -eq 0 -and
        $Summary.PSObject.Properties.Name -contains 'output' -and
        $null -ne $Summary.output -and
        $Summary.output.PSObject.Properties.Name -contains 'ok' -and
        [bool]$Summary.output.ok) {
        return $true
    }

    if ($Summary.PSObject.Properties.Name -contains 'internal_clean_host_install_update_rollback_smoke' -and
        [string]::Equals([string]$Summary.internal_clean_host_install_update_rollback_smoke, 'pass', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    if ($Summary.PSObject.Properties.Name -contains 'msix' -and [string]$Summary.msix -match 'pass') {
        return $true
    }

    if ($Summary.PSObject.Properties.Name -contains 'blocker' -and
        [string]::Equals([string]$Summary.blocker, 'none', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    return $false
}

function New-PcvEvidenceResult {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$Path,
        [string]$Expected = 'ok-true'
    )

    $fullPath = Resolve-PcvPath -Path $Path
    $summary = Read-PcvJsonFile -Path $fullPath
    $present = $null -ne $summary
    $passed = Test-PcvEvidencePass -Summary $summary
    $status = if (-not $present) {
        'missing'
    }
    elseif ($passed) {
        'pass'
    }
    else {
        'not-pass'
    }

    $projection = $null
    if ($present) {
        $projection = [ordered]@{
            ok = if ($summary.PSObject.Properties.Name -contains 'ok') { [bool]$summary.ok } else { $null }
            scope = if ($summary.PSObject.Properties.Name -contains 'scope') { [string]$summary.scope } else { $null }
            blocker = if ($summary.PSObject.Properties.Name -contains 'blocker') { [string]$summary.blocker } else { $null }
            msix = if ($summary.PSObject.Properties.Name -contains 'msix') { [string]$summary.msix } else { $null }
            final_manifest_version = if ($summary.PSObject.Properties.Name -contains 'final_manifest_version') { [string]$summary.final_manifest_version } else { $null }
            package_pair_input_status = if ($summary.PSObject.Properties.Name -contains 'package_pair_input_status') { [string]$summary.package_pair_input_status } else { $null }
        }
    }

    [ordered]@{
        id = $Id
        status = $status
        expected = $Expected
        path = $fullPath
        present = $present
        projection = $projection
    }
}

function Get-PcvPackagingReleaseNextTrigger {
    param(
        [string]$BaselineVersion,
        [string]$TargetVersion
    )

    if ([string]::Equals($BaselineVersion, '0.42.18-admin-smoke', [System.StringComparison]::OrdinalIgnoreCase) -and
        -not [string]::IsNullOrWhiteSpace($TargetVersion)) {
        return 'product-payload-change-after-04218-fullgate'
    }

    if ([string]::Equals($BaselineVersion, '0.42.20-admin-smoke', [System.StringComparison]::OrdinalIgnoreCase) -and
        -not [string]::IsNullOrWhiteSpace($TargetVersion)) {
        return 'product-payload-change-after-04220-fullgate'
    }

    return 'manual-admin-package-pair-evidence'
}

function Get-PcvDescriptorBatchId {
    param(
        [string]$DescriptorBatchId,
        [string]$ArtifactRoot
    )

    if (-not [string]::IsNullOrWhiteSpace($DescriptorBatchId)) {
        return $DescriptorBatchId
    }

    $leaf = Split-Path -Leaf $ArtifactRoot
    if ($leaf -like 'manual-admin-campaign-descriptor-*') {
        return $leaf
    }

    return $null
}

$artifactRootFull = Resolve-PcvPath -Path $ArtifactRoot
$campaignRootFull = Resolve-PcvPath -Path $CampaignArtifactRoot
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$productUpdatePathFull = Resolve-PcvPath -Path $ProductUpdateSummaryPath
$productRollbackPathFull = Resolve-PcvPath -Path $ProductRollbackSummaryPath
$productUpdate = Read-PcvJsonFile -Path $productUpdatePathFull
$productRollback = Read-PcvJsonFile -Path $productRollbackPathFull
$productLifecyclePassed = (Test-PcvEvidencePass -Summary $productUpdate) -and (Test-PcvEvidencePass -Summary $productRollback)
$productLifecycleStatus = if ([string]::IsNullOrWhiteSpace($productUpdatePathFull) -or [string]::IsNullOrWhiteSpace($productRollbackPathFull)) {
    'missing'
}
elseif ($productLifecyclePassed) {
    'pass'
}
else {
    'not-pass'
}

$runnerResults = @(
    (New-PcvEvidenceResult -Id 'manual-admin-readiness' -Path $ReadinessSummaryPath -Expected 'package-pair-ready'),
    [ordered]@{
        id = 'installed-product-update-rollback'
        status = $productLifecycleStatus
        expected = 'update-pass-and-rollback-pass'
        update_summary_path = $productUpdatePathFull
        rollback_summary_path = $productRollbackPathFull
        present = (-not [string]::IsNullOrWhiteSpace($productUpdatePathFull)) -and (-not [string]::IsNullOrWhiteSpace($productRollbackPathFull)) -and ($null -ne $productUpdate) -and ($null -ne $productRollback)
        projection = [ordered]@{
            update_ok = if ($null -ne $productUpdate -and $productUpdate.PSObject.Properties.Name -contains 'ok') { [bool]$productUpdate.ok } elseif ($null -ne $productUpdate -and $productUpdate.PSObject.Properties.Name -contains 'output' -and $productUpdate.output.PSObject.Properties.Name -contains 'ok') { [bool]$productUpdate.output.ok } else { $null }
            rollback_ok = if ($null -ne $productRollback -and $productRollback.PSObject.Properties.Name -contains 'ok') { [bool]$productRollback.ok } elseif ($null -ne $productRollback -and $productRollback.PSObject.Properties.Name -contains 'output' -and $productRollback.output.PSObject.Properties.Name -contains 'ok') { [bool]$productRollback.output.ok } else { $null }
            update_action = if ($null -ne $productUpdate -and $productUpdate.PSObject.Properties.Name -contains 'action') { [string]$productUpdate.action } elseif ($null -ne $productUpdate -and $productUpdate.PSObject.Properties.Name -contains 'output' -and $productUpdate.output.PSObject.Properties.Name -contains 'action') { [string]$productUpdate.output.action } else { $null }
            rollback_action = if ($null -ne $productRollback -and $productRollback.PSObject.Properties.Name -contains 'action') { [string]$productRollback.action } elseif ($null -ne $productRollback -and $productRollback.PSObject.Properties.Name -contains 'output' -and $productRollback.output.PSObject.Properties.Name -contains 'action') { [string]$productRollback.output.action } else { $null }
        }
    },
    (New-PcvEvidenceResult -Id 'clean-host-install-update-rollback' -Path $CleanHostSummaryPath -Expected 'clean-host-pass'),
    (New-PcvEvidenceResult -Id 'burn-install-repair-remove' -Path $BurnLifecycleSummaryPath -Expected 'burn-pass'),
    (New-PcvEvidenceResult -Id 'msix-build-install-update-remove' -Path $MsixLifecycleSummaryPath -Expected 'msix-pass'),
    (New-PcvEvidenceResult -Id 'installed-runtime-ops-summary' -Path $InstalledRuntimeOpsSummaryPath -Expected 'ops-summary-json-pass')
)

$missing = @($runnerResults | Where-Object { $_.status -eq 'missing' })
$notPass = @($runnerResults | Where-Object { $_.status -eq 'not-pass' })
$overallStatus = if ($missing.Count -gt 0) {
    'blocked-by-missing-evidence'
}
elseif ($notPass.Count -gt 0) {
    'partial-pass-with-failed-evidence'
}
else {
    'pass'
}

$packagingReleaseNextTrigger = Get-PcvPackagingReleaseNextTrigger -BaselineVersion $BaselineVersion -TargetVersion $TargetVersion
$descriptorSchemaVersion = 2
$manualAdminDescriptorGenerationContract = 'manual-admin-descriptor-generation-contract-v2'
$descriptorContractKey = $manualAdminDescriptorGenerationContract
$resolvedDescriptorBatchId = Get-PcvDescriptorBatchId -DescriptorBatchId $DescriptorBatchId -ArtifactRoot $artifactRootFull
$requiredCodeContracts = @(
    'runtime-api-diagnostics-ops-summary-registry-bridge-v2',
    'hyperv-wmi-provider-callsite-drift-guard-v1',
    'host-ops-dryrun-mutation-reason-code-v1',
    'manual-admin-descriptor-generation-contract-v2'
)
$nextProductPayloadCandidateStatus = if ([string]::Equals($BaselineVersion, '0.42.20-admin-smoke', [System.StringComparison]::OrdinalIgnoreCase) -and
    -not [string]::IsNullOrWhiteSpace($TargetVersion)) {
    'candidate-selected-awaiting-package-build'
}
elseif ($packagingReleaseNextTrigger -like 'product-payload-change-*') {
    'pending-product-payload-change'
}
else {
    'manual-admin-package-pair-evidence'
}
$releaseCandidate = [ordered]@{
    source_version_anchor = $BaselineVersion
    next_candidate_version = $TargetVersion
    packaging_release_next_trigger = $packagingReleaseNextTrigger
    next_product_payload_candidate_status = $nextProductPayloadCandidateStatus
    manual_admin_descriptor_generation_contract = $manualAdminDescriptorGenerationContract
    required_code_contracts = $requiredCodeContracts
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
}

$descriptorPath = Join-Path $artifactRootFull 'manual-admin-campaign.descriptor.json'
$descriptor = [ordered]@{
    schema_version = 1
    descriptor_schema_version = $descriptorSchemaVersion
    descriptor_contract_key = $descriptorContractKey
    descriptor_batch_id = $resolvedDescriptorBatchId
    scope = 'manual-admin-campaign-descriptor'
    plan_only = $true
    actual_execution = 'not-run'
    host_mutation_performed = $false
    generated_utc = (Get-Date).ToUniversalTime().ToString('o')
    campaign_artifact_root = $campaignRootFull
    baseline_version = $BaselineVersion
    target_version = $TargetVersion
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    overall_status = $overallStatus
    packaging_release_next_trigger = $packagingReleaseNextTrigger
    manual_admin_descriptor_generation_contract = $manualAdminDescriptorGenerationContract
    next_product_payload_package_build_trigger = $packagingReleaseNextTrigger
    release_candidate = $releaseCandidate
    runner_results = $runnerResults
}

Write-PcvJsonFile -Path $descriptorPath -Value $descriptor

$summary = [ordered]@{
    schema_version = 1
    descriptor_schema_version = $descriptorSchemaVersion
    descriptor_contract_key = $descriptorContractKey
    descriptor_batch_id = $resolvedDescriptorBatchId
    ok = [string]::Equals($overallStatus, 'pass', [System.StringComparison]::OrdinalIgnoreCase)
    scope = 'manual-admin-campaign-descriptor'
    plan_only = $true
    actual_execution = 'not-run'
    host_mutation_performed = $false
    baseline_version = $BaselineVersion
    target_version = $TargetVersion
    campaign_artifact_root = $campaignRootFull
    descriptor_path = $descriptorPath
    overall_status = $overallStatus
    runner_count = @($runnerResults).Count
    missing_count = $missing.Count
    not_pass_count = $notPass.Count
    runner_results = $runnerResults
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    packaging_release_next_trigger = $packagingReleaseNextTrigger
    manual_admin_descriptor_generation_contract = $manualAdminDescriptorGenerationContract
    next_product_payload_package_build_trigger = $packagingReleaseNextTrigger
    release_candidate = $releaseCandidate
}

Write-PcvJsonFile -Path (Join-Path $artifactRootFull 'summary.json') -Value $summary
$summary | ConvertTo-Json -Depth 32
