Set-StrictMode -Version Latest

Describe 'PcvManualAdminCampaignDescriptor contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'

        function Write-TestJson {
            param(
                [Parameter(Mandatory)][string]$Path,
                [Parameter(Mandatory)]$Value
            )

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            $Value | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding utf8
        }

        function New-TestEvidenceSet {
            param([Parameter(Mandatory)][string]$Root)

            Write-TestJson -Path (Join-Path $Root 'readiness/summary.json') -Value ([ordered]@{
                ok = $true
                scope = 'manual-admin-rebaseline-readiness'
                package_pair_input_status = 'ready-current-baseline-target-package-pair'
            })
            Write-TestJson -Path (Join-Path $Root 'update.json') -Value ([ordered]@{ exit_code = 0; output = [ordered]@{ ok = $true; action = 'Update' } })
            Write-TestJson -Path (Join-Path $Root 'rollback.json') -Value ([ordered]@{ exit_code = 0; output = [ordered]@{ ok = $true; action = 'Rollback' } })
            Write-TestJson -Path (Join-Path $Root 'clean-host/summary.json') -Value ([ordered]@{
                ok = $true
                internal_clean_host_install_update_rollback_smoke = 'pass'
                final_manifest_version = '0.42.5-admin-smoke'
            })
            Write-TestJson -Path (Join-Path $Root 'burn/summary.json') -Value ([ordered]@{ ok = $true; blocker = 'none' })
            Write-TestJson -Path (Join-Path $Root 'msix/summary.json') -Value ([ordered]@{
                ok = $true
                msix = 'build-install-update-remove-pass-internal-smoke'
            })
            Write-TestJson -Path (Join-Path $Root 'ops/summary.json') -Value ([ordered]@{
                ok = $true
                scope = 'installed-runtime-ops-summary-capture'
            })
        }
    }

    It 'writes a plan-only descriptor that ties manual-admin runner evidence together' {
        $root = Join-Path $TestDrive 'campaign'
        $artifactRoot = Join-Path $TestDrive 'descriptor'
        New-TestEvidenceSet -Root $root

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -CampaignArtifactRoot $root `
            -BaselineVersion '0.42.5-admin-smoke' `
            -TargetVersion '0.42.6-admin-smoke' `
            -ReadinessSummaryPath (Join-Path $root 'readiness/summary.json') `
            -ProductUpdateSummaryPath (Join-Path $root 'update.json') `
            -ProductRollbackSummaryPath (Join-Path $root 'rollback.json') `
            -CleanHostSummaryPath (Join-Path $root 'clean-host/summary.json') `
            -BurnLifecycleSummaryPath (Join-Path $root 'burn/summary.json') `
            -MsixLifecycleSummaryPath (Join-Path $root 'msix/summary.json') `
            -InstalledRuntimeOpsSummaryPath (Join-Path $root 'ops/summary.json') `
            -DescriptorBatchId 'manual-admin-campaign-descriptor-test-closed' `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $descriptor = Get-Content -Raw -LiteralPath $summary.descriptor_path | ConvertFrom-Json
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'manual-admin-campaign-descriptor'
        $summary.descriptor_schema_version | Should -Be 2
        $summary.descriptor_contract_key | Should -Be 'manual-admin-descriptor-generation-contract-v2'
        $summary.descriptor_batch_id | Should -Be 'manual-admin-campaign-descriptor-test-closed'
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.overall_status | Should -Be 'pass'
        $summary.runner_count | Should -Be 6
        $descriptor.descriptor_schema_version | Should -Be 2
        $descriptor.descriptor_contract_key | Should -Be 'manual-admin-descriptor-generation-contract-v2'
        $descriptor.descriptor_batch_id | Should -Be 'manual-admin-campaign-descriptor-test-closed'
        @($summary.runner_results | ForEach-Object { $_.id }) | Should -Be @(
            'manual-admin-readiness',
            'installed-product-update-rollback',
            'clean-host-install-update-rollback',
            'burn-install-repair-remove',
            'msix-build-install-update-remove',
            'installed-runtime-ops-summary'
        )
        Test-Path -LiteralPath $summary.descriptor_path | Should -BeTrue
    }

    It 'blocks the descriptor when a required evidence summary is missing' {
        $root = Join-Path $TestDrive 'campaign-missing'
        $artifactRoot = Join-Path $TestDrive 'descriptor-missing'
        New-TestEvidenceSet -Root $root

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -CampaignArtifactRoot $root `
            -BaselineVersion '0.42.5-admin-smoke' `
            -TargetVersion '0.42.6-admin-smoke' `
            -ReadinessSummaryPath (Join-Path $root 'readiness/summary.json') `
            -ProductUpdateSummaryPath (Join-Path $root 'update.json') `
            -ProductRollbackSummaryPath (Join-Path $root 'rollback.json') `
            -CleanHostSummaryPath (Join-Path $root 'clean-host/summary.json') `
            -BurnLifecycleSummaryPath (Join-Path $root 'missing-burn/summary.json') `
            -MsixLifecycleSummaryPath (Join-Path $root 'msix/summary.json') `
            -InstalledRuntimeOpsSummaryPath (Join-Path $root 'ops/summary.json') `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.ok | Should -BeFalse
        $summary.overall_status | Should -Be 'blocked-by-missing-evidence'
        ($summary.runner_results | Where-Object id -eq 'burn-install-repair-remove').status | Should -Be 'missing'
    }

    It 'records the post-04218 next product payload trigger for the 0.42.19 descriptor candidate' {
        $root = Join-Path $TestDrive 'campaign-04219'
        $artifactRoot = Join-Path $TestDrive 'descriptor-04219'
        New-TestEvidenceSet -Root $root

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -CampaignArtifactRoot $root `
            -BaselineVersion '0.42.18-admin-smoke' `
            -TargetVersion '0.42.19-admin-smoke' `
            -ReadinessSummaryPath (Join-Path $root 'readiness/summary.json') `
            -ProductUpdateSummaryPath (Join-Path $root 'update.json') `
            -ProductRollbackSummaryPath (Join-Path $root 'rollback.json') `
            -CleanHostSummaryPath (Join-Path $root 'clean-host/summary.json') `
            -BurnLifecycleSummaryPath (Join-Path $root 'burn/summary.json') `
            -MsixLifecycleSummaryPath (Join-Path $root 'msix/summary.json') `
            -InstalledRuntimeOpsSummaryPath (Join-Path $root 'ops/summary.json') `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $descriptor = Get-Content -Raw -LiteralPath $summary.descriptor_path | ConvertFrom-Json

        $summary.packaging_release_next_trigger | Should -Be 'product-payload-change-after-04218-fullgate'
        $summary.release_candidate.source_version_anchor | Should -Be '0.42.18-admin-smoke'
        $summary.release_candidate.next_candidate_version | Should -Be '0.42.19-admin-smoke'
        $summary.release_candidate.public_trusted_signing | Should -Be 'not-claimed'
        $summary.release_candidate.external_stable_publication | Should -Be 'not-claimed'
        $descriptor.packaging_release_next_trigger | Should -Be 'product-payload-change-after-04218-fullgate'
        $descriptor.release_candidate.next_candidate_version | Should -Be '0.42.19-admin-smoke'
    }

    It 'records the post-04220 next product payload trigger for the next descriptor candidate' {
        $root = Join-Path $TestDrive 'campaign-04221'
        $artifactRoot = Join-Path $TestDrive 'descriptor-04221'
        New-TestEvidenceSet -Root $root

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -CampaignArtifactRoot $root `
            -BaselineVersion '0.42.20-admin-smoke' `
            -TargetVersion '0.42.21-admin-smoke' `
            -ReadinessSummaryPath (Join-Path $root 'readiness/summary.json') `
            -ProductUpdateSummaryPath (Join-Path $root 'update.json') `
            -ProductRollbackSummaryPath (Join-Path $root 'rollback.json') `
            -CleanHostSummaryPath (Join-Path $root 'clean-host/summary.json') `
            -BurnLifecycleSummaryPath (Join-Path $root 'burn/summary.json') `
            -MsixLifecycleSummaryPath (Join-Path $root 'msix/summary.json') `
            -InstalledRuntimeOpsSummaryPath (Join-Path $root 'ops/summary.json') `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $descriptor = Get-Content -Raw -LiteralPath $summary.descriptor_path | ConvertFrom-Json

        $summary.packaging_release_next_trigger | Should -Be 'product-payload-change-after-04220-fullgate'
        $summary.release_candidate.source_version_anchor | Should -Be '0.42.20-admin-smoke'
        $summary.release_candidate.next_candidate_version | Should -Be '0.42.21-admin-smoke'
        $summary.release_candidate.public_trusted_signing | Should -Be 'not-claimed'
        $summary.release_candidate.external_stable_publication | Should -Be 'not-claimed'
        $descriptor.packaging_release_next_trigger | Should -Be 'product-payload-change-after-04220-fullgate'
        $descriptor.release_candidate.next_candidate_version | Should -Be '0.42.21-admin-smoke'
        $summary.manual_admin_descriptor_generation_contract | Should -Be 'manual-admin-descriptor-generation-contract-v2'
        $summary.next_product_payload_package_build_trigger | Should -Be 'product-payload-change-after-04220-fullgate'
        $summary.release_candidate.next_product_payload_candidate_status | Should -Be 'candidate-selected-awaiting-package-build'
        $summary.release_candidate.manual_admin_descriptor_generation_contract | Should -Be 'manual-admin-descriptor-generation-contract-v2'
        @($summary.release_candidate.required_code_contracts) | Should -Contain 'runtime-api-diagnostics-ops-summary-registry-bridge-v2'
        @($summary.release_candidate.required_code_contracts) | Should -Contain 'hyperv-wmi-provider-callsite-drift-guard-v1'
        @($summary.release_candidate.required_code_contracts) | Should -Contain 'host-ops-dryrun-mutation-reason-code-v1'
        $descriptor.release_candidate.next_product_payload_candidate_status | Should -Be 'candidate-selected-awaiting-package-build'
        @($descriptor.release_candidate.required_code_contracts) | Should -Contain 'manual-admin-descriptor-generation-contract-v2'
    }

    It 'requires plan-only mode and contains no host mutation commands' {
        $root = Join-Path $TestDrive 'campaign-nonplan'
        New-TestEvidenceSet -Root $root

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'descriptor-nonplan') `
            -CampaignArtifactRoot $root 2>$null | Out-Null
        $LASTEXITCODE | Should -Not -Be 0

        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint
        $scriptText | Should -Not -Match 'msiexec|Start-Service|Stop-Service|Restart-Service|sc\.exe|New-VM|Remove-VM|Add-AppxPackage|Remove-AppxPackage|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|cmdkey|CredWrite|CredDelete'
    }
}
