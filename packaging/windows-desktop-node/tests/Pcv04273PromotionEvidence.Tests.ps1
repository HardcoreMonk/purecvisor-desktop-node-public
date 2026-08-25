Set-StrictMode -Version Latest

Describe '0.42.73 predecessor promotion evidence contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path

        function Get-RepoText {
            param([Parameter(Mandatory)] [string] $RelativePath)

            Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot $RelativePath)
        }

        function Assert-MetadataValue {
            param(
                [Parameter(Mandatory)] [string] $Content,
                [Parameter(Mandatory)] [string] $Name,
                [Parameter(Mandatory)] [string] $Value
            )

            $pattern = '(?m)^' + [regex]::Escape($Name) + ':\s*`' +
                [regex]::Escape($Value) + '`\s*$'
            $Content | Should -Match $pattern -Because "$Name must stay pinned to $Value"
        }

        $script:EvidenceDocuments = [ordered]@{
            'docs/ga-ready/evidence/admin-smoke-package-2026-08-14-04273.md' = 'not-claimed'
            'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-14-04273-hostmutation.md' = 'excluded'
            'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-14-04273.md' = 'not-claimed'
            'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md' = 'not-claimed'
            'docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md' = 'not-claimed'
            'docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md' = 'not-claimed'
            'docs/ga-ready/evidence/operational-credential-rebootstrap-recovery-r2-2026-08-09-04272.md' = 'not-claimed'
            'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md' = 'not-claimed'
        }
    }

    It 'keeps the exact eight repository-owned promotion evidence documents at PASS with bounded nonclaims' {
        $script:EvidenceDocuments.Count | Should -Be 8

        foreach ($entry in $script:EvidenceDocuments.GetEnumerator()) {
            $path = Join-Path $script:RepoRoot $entry.Key
            $path | Should -Exist -Because $entry.Key

            $content = Get-Content -Raw -LiteralPath $path
            Assert-MetadataValue -Content $content -Name 'result' -Value 'PASS'
            Assert-MetadataValue -Content $content -Name 'public_trusted_signing' -Value $entry.Value
            Assert-MetadataValue -Content $content -Name 'external_stable_publication' -Value 'not-claimed'
        }
    }

    It 'demotes the 0.42.73 tuple instead of deleting it' {
        $record = Get-RepoText -RelativePath 'docs/ga-ready/current-evidence.json' | ConvertFrom-Json
        $record.current.version | Should -Not -BeExactly '0.42.73-admin-smoke'

        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_manual_admin_package_pair' -Value '0.42.72-admin-smoke -> 0.42.73-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_manual_admin_campaign' -Value 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_manual_admin_descriptor_batch_manifest' -Value 'manual-admin-campaign-descriptor-20260814-04272-04273-closed'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_full_admin_host_mutation_batch' -Value 'full-admin-host-mutation-gate-20260814-04273'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_full_admin_host_mutation_operational_msi_sha256' -Value '3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_full_admin_host_mutation_payload_aggregate_sha256' -Value 'a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_full_admin_host_mutation_provenance_commit' -Value 'b84441f0750a9f77fd0588a86912dbdb68b94f0c'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_installed_operator_surface_current_card_evidence' -Value 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md'
    }

    It 'binds the final token claim to the exact R4 runner and summary evidence' {
        $token = Get-RepoText -RelativePath 'docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'

        Assert-MetadataValue -Content $token -Name 'r4_runner_raw_sha256' -Value 'c6e138a008315bc2b75b76eb51a202cb75163cd37b961e4a9dfb5f14c2b98414'
        Assert-MetadataValue -Content $token -Name 'r4_runner_contract_sha256' -Value '259547e6eb82d66f172f7bf5f02d9171af1a6b84bcf2d9f8680780b7eb0b424f'
        Assert-MetadataValue -Content $token -Name 'final_summary_sha256' -Value '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        Assert-MetadataValue -Content $token -Name 'current_claim_eligible' -Value 'true'
        Assert-MetadataValue -Content $token -Name 'historical_retry2_host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $token -Name 'read_only_reconciliation_host_mutation_performed' -Value 'false'
        Assert-MetadataValue -Content $token -Name 'host_mutation_performed' -Value 'false'
        Assert-MetadataValue -Content $token -Name 'token_value_recorded' -Value 'false'

        $token | Should -Match '\|\s*classification\s*\|\s*`native-rotation-succeeded-verifier-false-negative-reconciled`\s*\|'
        $token | Should -Match '\|\s*direct auth readback\s*\|\s*old token HTTP `403`, new token HTTP `200`\s*\|'
        $token | Should -Match '\|\s*secret scan\s*\|\s*findings `0`, read failures `0`, raw values recorded `false`\s*\|'
    }

    It 'keeps the 0.42.73 current-card as a historical promoted record with carried-forward token evidence' {
        $card = Get-RepoText -RelativePath 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md'

        Assert-MetadataValue -Content $card -Name 'promotion_ledger_status' -Value 'promoted-current'
        Assert-MetadataValue -Content $card -Name 'token_rotation_evidence' -Value 'docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'
        Assert-MetadataValue -Content $card -Name 'token_rotation_r4_summary' -Value 'artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json'
        Assert-MetadataValue -Content $card -Name 'token_rotation_r4_summary_sha256' -Value '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        Assert-MetadataValue -Content $card -Name 'token_rotation_status' -Value 'carry-forward-no-token-payload-change-after-04272'
        Assert-MetadataValue -Content $card -Name 'latest_manual_admin_package_pair' -Value '0.42.72-admin-smoke -> 0.42.73-admin-smoke'
    }

    It 'contains no provisional promotion marker in the final promotion records' {
        $promotionRecords = @(
            'docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md',
            'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md',
            'docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md',
            'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-09-pr186-postmerge-pass.md',
            'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md',
            'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md'
        )
        $staleMarker = '(?-i:PENDING)|(?i:\bdeferred\b|\bdraft\b|\bR2(?:\s+|-)final\b)'

        foreach ($relativePath in $promotionRecords) {
            (Get-RepoText -RelativePath $relativePath) |
                Should -Not -Match $staleMarker -Because "$relativePath is a final promotion record"
        }
    }

    It 'records the 0.42.73 promotion main push without opening another package candidate' {
        $relativePath = 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md'
        $postMerge = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $postMerge -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $postMerge -Name 'scope' -Value 'post-04273-promotion-main-push'
        Assert-MetadataValue -Content $postMerge -Name 'run_id' -Value '31737488576'
        Assert-MetadataValue -Content $postMerge -Name 'job_id' -Value '94572517694'
        Assert-MetadataValue -Content $postMerge -Name 'head_sha' -Value '291435e374efef7f9639b820ac197c11e2c7e8a4'
        Assert-MetadataValue -Content $postMerge -Name 'development_gates_run_id' -Value '31737488562'
        Assert-MetadataValue -Content $postMerge -Name 'product_payload_change_detected' -Value 'false'
        Assert-MetadataValue -Content $postMerge -Name 'changed_path_count' -Value '17'
        Assert-MetadataValue -Content $postMerge -Name 'product_payload_path_count' -Value '0'
        Assert-MetadataValue -Content $postMerge -Name 'current_version_anchor' -Value '0.42.73-admin-smoke'
        Assert-MetadataValue -Content $postMerge -Name 'additional_package_candidate_opened' -Value 'false'
        Assert-MetadataValue -Content $postMerge -Name 'package_candidate_decision' -Value 'docs-only-followup-retains-0.42.73-admin-smoke'
        Assert-MetadataValue -Content $postMerge -Name 'public_trusted_signing' -Value 'not-claimed'
        Assert-MetadataValue -Content $postMerge -Name 'external_stable_publication' -Value 'not-claimed'

        foreach ($job in @(
                @('web-tests', '94572517696'),
                @('dotnet-tests', '94572517725'),
                @('packaging-pester', '94572517728'),
                @('installer-web-pester', '94572517741')
            )) {
            $postMerge | Should -Match ('\|\s*`' + [regex]::Escape($job[0]) +
                '`\s*\|\s*`' + [regex]::Escape($job[1]) + '`\s*\|\s*`success`\s*\|')
        }

        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_public_boundary_main_push_evidence' -Value $relativePath
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_public_boundary_main_push_run_id' -Value '31737488576'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_public_boundary_main_push_job_id' -Value '94572517694'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_public_boundary_main_push_head_sha' -Value '291435e374efef7f9639b820ac197c11e2c7e8a4'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_public_boundary_main_push_product_payload_change_detected' -Value 'false'
        Assert-MetadataValue -Content $descriptor -Name 'previous_04273_current_public_boundary_main_push_package_candidate_decision' -Value 'docs-only-followup-retains-0.42.73-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'previous_pr187_current_public_boundary_main_push_evidence' -Value 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        Assert-MetadataValue -Content $ledger -Name 'previous_04273_current_public_boundary_main_push_evidence' -Value $relativePath
        Assert-MetadataValue -Content $ledger -Name 'previous_04273_current_public_boundary_main_push_run_id' -Value '31737488576'
        Assert-MetadataValue -Content $ledger -Name 'previous_04273_current_public_boundary_main_push_job_id' -Value '94572517694'
        Assert-MetadataValue -Content $ledger -Name 'previous_04273_current_public_boundary_main_push_head_sha' -Value '291435e374efef7f9639b820ac197c11e2c7e8a4'
        Assert-MetadataValue -Content $ledger -Name 'previous_04273_current_public_boundary_product_payload_change_detected' -Value 'false'
        Assert-MetadataValue -Content $ledger -Name 'previous_04273_current_public_boundary_package_candidate_decision' -Value 'docs-only-followup-retains-0.42.73-admin-smoke'
        Assert-MetadataValue -Content $ledger -Name 'previous_pr187_public_boundary_main_push_evidence' -Value 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md'
        $ledger | Should -Match '\|\s*`public-boundary-04273-promotion-predecessor`\s*\|\s*`pass`, 0\.42\.73 promotion main push\s*\|'
        $ledger | Should -Match '\|\s*`public-boundary-pr187-predecessor`\s*\|'

        $index = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $index | Should -Match ([regex]::Escape($relativePath))
        $index | Should -Match 'product payload 경로는 `0`개'

        $postMerge | Should -Not -Match '0\.42\.74'
        $descriptor | Should -Match 'docs-only-followup-retains-0\.42\.73-admin-smoke'
        $ledger | Should -Match 'docs-only-followup-retains-0\.42\.73-admin-smoke'
    }

    It 'keeps the 0.42.73 promotion chain discoverable from the evidence index' {
        $index = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        foreach ($relativePath in $script:EvidenceDocuments.Keys) {
            $index | Should -Match ([regex]::Escape($relativePath)) `
                -Because 'the evidence index must keep every 0.42.73 promotion record discoverable'
        }
        $index | Should -Match 'manual-admin-campaign-descriptor-20260814-04272-04273-closed'
        $index | Should -Match '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $ledger | Should -Match '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        $ledger | Should -Match ([regex]::Escape('docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'))
        $ledger | Should -Match ([regex]::Escape('docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md'))
    }
}
