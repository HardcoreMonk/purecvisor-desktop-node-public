Set-StrictMode -Version Latest

Describe '0.42.74 SERVICE_PLAN P0 promotion evidence contract' {
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
    }

    It 'pins the canonical current evidence record to the exact 0.42.74 tuple' {
        $record = Get-RepoText -RelativePath 'docs/ga-ready/current-evidence.json' | ConvertFrom-Json

        $record.schema_version | Should -Be 1
        $record.contract | Should -BeExactly 'pcv-current-evidence-v1'
        $record.current.version | Should -BeExactly '0.42.74-admin-smoke'
        (@($record.current.operator_surfaces) -join ',') | Should -BeExactly 'web,cli'
        $record.current.tui_present | Should -BeFalse
        $record.current.package_evidence | Should -BeExactly 'docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md'
        $record.current.fullgate_batch | Should -BeExactly 'full-admin-host-mutation-gate-20260820-04274'
        $record.current.fullgate_evidence | Should -BeExactly 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md'
        $record.current.functional_evidence | Should -BeExactly 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md'
        $record.current.installed_evidence | Should -BeExactly 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md'
        $record.current.clean_msi_sha256 | Should -BeExactly 'f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e'
        $record.current.operational_msi_sha256 | Should -BeExactly '2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787'
        $record.current.payload_sha256 | Should -BeExactly 'c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d'
        $record.current.provenance_commit | Should -BeExactly 'adc04673b569ef9b587371fdb23bc11ceb14e2e2'
        $record.manual_admin.latest_closed_baseline | Should -BeExactly '0.42.73-admin-smoke'
        $record.manual_admin.latest_closed_target | Should -BeExactly '0.42.74-admin-smoke'
        $record.manual_admin.latest_closed_descriptor | Should -BeExactly 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
        $record.claims.public_trusted_signing | Should -BeFalse
        $record.claims.external_stable_publication | Should -BeFalse
    }

    It 'records the clean 0.42.74 package as current' {
        $relativePath = 'docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md'
        $package = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $package -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $package -Name 'version' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $package -Name 'source_commit' -Value 'adc04673b569ef9b587371fdb23bc11ceb14e2e2'
        Assert-MetadataValue -Content $package -Name 'artifact_root' -Value 'artifacts/admin-smoke-package-20260820-04274'
        Assert-MetadataValue -Content $package -Name 'signing_mode' -Value 'AllowUnsignedDev'
        Assert-MetadataValue -Content $package -Name 'signing_trust_model' -Value 'LocalTest'
        Assert-MetadataValue -Content $package -Name 'clean_package_msi_sha256' -Value 'f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e'
        Assert-MetadataValue -Content $package -Name 'clean_package_payload_aggregate_sha256' -Value 'c55cd17d14fed521252e6fee1bf08c828410339b23172fadb01dbd19f7d2578e'
        Assert-MetadataValue -Content $package -Name 'payload_file_count' -Value '8'
        Assert-MetadataValue -Content $package -Name 'host_mutation_performed' -Value 'false'
        Assert-MetadataValue -Content $package -Name 'package_installed' -Value 'false'
        Assert-MetadataValue -Content $package -Name 'canonical_current_evidence' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $package -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $package -Name 'public_trusted_signing' -Value 'not-claimed'
        Assert-MetadataValue -Content $package -Name 'external_stable_publication' -Value 'not-claimed'

        $package | Should -Match 'full admin host mutation'
        $package | Should -Match 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
    }

    It 'closes the 0.42.73 -> 0.42.74 pair as current and opens the next not-opened pair' {
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'

        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_package_pair' -Value '0.42.73-admin-smoke -> 0.42.74-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'latest_manual_admin_candidate_status' -Value 'pass-closed'
        Assert-MetadataValue -Content $descriptor -Name 'latest_manual_admin_candidate_package_pair' -Value '0.42.73-admin-smoke -> 0.42.74-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'next_manual_admin_package_pair_candidate' -Value '0.42.74-admin-smoke -> next-admin-smoke-required'
        Assert-MetadataValue -Content $descriptor -Name 'next_manual_admin_package_pair_candidate_status' -Value 'not-opened-awaiting-next-product-payload'
        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_update_package_sha256' -Value 'cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1'
        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_descriptor_batch_manifest' -Value 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_target_msi_sha256' -Value 'f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e'
        Assert-MetadataValue -Content $descriptor -Name 'current_full_admin_host_mutation_provenance_commit' -Value 'adc04673b569ef9b587371fdb23bc11ceb14e2e2'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_package_candidate_decision' -Value 'landed-already-validated-as-0.42.74-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_evidence' -Value 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-21-04274-p0-landing-pass.md'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_run_id' -Value '32388996125'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_job_id' -Value '96490306459'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_head_sha' -Value '5f9cecfd5507e7e5dd726601aae3760e4e1b558c'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_product_payload_change_detected' -Value 'true'
    }

    It 'indexes 0.42.74 as generated current and keeps the save defect visible' {
        $index = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $control = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'

        $index | Should -Match 'docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md'
        $index | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md'
        $index | Should -Match 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md'
        $index | Should -Match 'canonical current는 `0\.42\.74-admin-smoke`다'
        $control | Should -Match 'docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md'
        $control | Should -Match 'operational current는 `0\.42\.74-admin-smoke`다'
        $ledger | Should -Match '\|\s*`manual-admin-package-pair-next`\s*\|\s*`not-opened-awaiting-next-product-payload`,\s*`0\.42\.74-admin-smoke -> next-admin-smoke-required`\s*\|'
        $ledger | Should -Match '\|\s*`service-plan-p0-save-open-defect`\s*\|\s*`fail-open`'
        $index | Should -Match 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md'
        $index | Should -Match 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md'
        $index | Should -Match 'docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md'
        $ledger | Should -Match '\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.74-admin-smoke`\s*\|'
        $ledger | Should -Match '\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.74-admin-smoke`\s*\|'
        $ledger | Should -Match '\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.74-admin-smoke`\s*\|'
    }

    It 'records the 0.42.74 fullgate PASS as current' {
        $relativePath = 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md'
        $fullgate = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $fullgate -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $fullgate -Name 'version' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $fullgate -Name 'batch_id' -Value 'full-admin-host-mutation-gate-20260820-04274'
        Assert-MetadataValue -Content $fullgate -Name 'operational_fullgate_msi_sha256' -Value '2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787'
        Assert-MetadataValue -Content $fullgate -Name 'operational_fullgate_payload_aggregate_sha256' -Value 'c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d'
        Assert-MetadataValue -Content $fullgate -Name 'provenance_commit' -Value 'adc04673b569ef9b587371fdb23bc11ceb14e2e2'
        Assert-MetadataValue -Content $fullgate -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $fullgate -Name 'canonical_current_evidence' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $fullgate -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $fullgate -Name 'public_trusted_signing' -Value 'excluded'
        Assert-MetadataValue -Content $fullgate -Name 'external_stable_publication' -Value 'not-claimed'

        $fullgate | Should -Match 'pcv-spike-api-79522716'
        $fullgate | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
        $fullgate | Should -Match 'remaining_pcv_vms=\[\]'
    }

    It 'promotes the 0.42.74 installed current-card with carried-forward token evidence' {
        $relativePath = 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md'
        $card = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $card -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $card -Name 'version' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $card -Name 'tui_present' -Value 'false'
        Assert-MetadataValue -Content $card -Name 'summary_sha256' -Value '531fc614da5edb0e11994b021383491ccb8830115d59fb211c6c330f5b25f8c8'
        Assert-MetadataValue -Content $card -Name 'cli_exit_zero_count' -Value '3'
        Assert-MetadataValue -Content $card -Name 'web_http_200_count' -Value '2'
        Assert-MetadataValue -Content $card -Name 'secret_observed' -Value 'false'
        Assert-MetadataValue -Content $card -Name 'host_mutation_performed' -Value 'false'
        Assert-MetadataValue -Content $card -Name 'promotion_ledger_status' -Value 'promoted-current'
        Assert-MetadataValue -Content $card -Name 'canonical_current_evidence' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $card -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $card -Name 'latest_manual_admin_package_pair' -Value '0.42.73-admin-smoke -> 0.42.74-admin-smoke'
        Assert-MetadataValue -Content $card -Name 'token_rotation_evidence' -Value 'docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'
        Assert-MetadataValue -Content $card -Name 'token_rotation_r4_summary_sha256' -Value '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        Assert-MetadataValue -Content $card -Name 'token_rotation_status' -Value 'carry-forward-no-token-payload-change-after-04272'
        Assert-MetadataValue -Content $card -Name 'public_trusted_signing' -Value 'not-claimed'
        Assert-MetadataValue -Content $card -Name 'external_stable_publication' -Value 'not-claimed'
    }

    It 'records the 0.42.74 functional actual-VM PASS as current' {
        $relativePath = 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md'
        $functional = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $functional -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $functional -Name 'summary_sha256' -Value '5395286b74ca7dabd3edccbb63c0b006c32999a4c350559e8b90ddb1ea1fb4b8'
        Assert-MetadataValue -Content $functional -Name 'vm_name' -Value 'pcv-fc-cf-04274'
        Assert-MetadataValue -Content $functional -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $functional -Name 'canonical_current_evidence' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $functional -Name 'public_trusted_signing' -Value 'not-claimed'
    }

    It 'keeps the 0.42.74 P0 actual-VM save failure as an open defect after promotion' {
        $relativePath = 'docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md'
        $p0 = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $p0 -Name 'result' -Value 'FAIL'
        Assert-MetadataValue -Content $p0 -Name 'summary_sha256' -Value '11d8d1b34d6e6ff49e2ebb81bc234d20b7eab9f1299baa36ce8daac9c9b14e5d'
        Assert-MetadataValue -Content $p0 -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $p0 -Name 'canonical_current_evidence' -Value '0.42.74-admin-smoke'
        $p0 | Should -Match '32775'
        $p0 | Should -Match 'RequestedState `6`'
        $p0 | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
        $p0 | Should -Match '열린 결함'
        Assert-MetadataValue -Content $p0 -Name 'public_trusted_signing' -Value 'not-claimed'
    }

    It 'records the 0.42.73 -> 0.42.74 manual-admin pair PASS as current' {
        $relativePath = 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md'
        $pair = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $pair -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $pair -Name 'baseline_version' -Value '0.42.73-admin-smoke'
        Assert-MetadataValue -Content $pair -Name 'target_version' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $pair -Name 'descriptor_batch_id' -Value 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
        Assert-MetadataValue -Content $pair -Name 'target_msi_sha256' -Value 'f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e'
        Assert-MetadataValue -Content $pair -Name 'update_zip_sha256' -Value 'cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1'
        Assert-MetadataValue -Content $pair -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $pair -Name 'canonical_current_evidence' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $pair -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $pair -Name 'public_trusted_signing' -Value 'not-claimed'
        $pair | Should -Match 'runner_count=6'
        $pair | Should -Match 'KB5120242'
    }

    It 'links the closed package pair exactly from the manual-admin descriptor' {
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $expected = [ordered]@{
            current_manual_admin_package_pair = '0.42.73-admin-smoke -> 0.42.74-admin-smoke'
            current_manual_admin_campaign = 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md'
            current_manual_admin_campaign_root = 'artifacts/manual-admin-campaign-20260820-04273-04274'
            current_manual_admin_target_package_root = 'artifacts/admin-smoke-package-20260820-04274'
            current_manual_admin_target_msi_sha256 = 'f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e'
            current_manual_admin_update_package_sha256 = 'cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1'
            current_manual_admin_descriptor_batch_manifest = 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
            current_manual_admin_descriptor_summary = 'artifacts/manual-admin-campaign-20260820-04273-04274/manual-admin-campaign-descriptor/summary.json'
            current_installed_operator_surface_current_card_evidence = 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md'
            latest_manual_admin_candidate_package_pair = '0.42.73-admin-smoke -> 0.42.74-admin-smoke'
            latest_manual_admin_candidate_campaign = 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md'
            latest_manual_admin_candidate_descriptor_batch_manifest = 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
            latest_manual_admin_candidate_status = 'pass-closed'
            current_full_admin_host_mutation_gate = 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md'
            current_full_admin_host_mutation_batch = 'full-admin-host-mutation-gate-20260820-04274'
            current_full_admin_host_mutation_payload_aggregate_sha256 = 'c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d'
            current_full_admin_host_mutation_operational_msi_sha256 = '2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787'
            current_full_admin_host_mutation_provenance_commit = 'adc04673b569ef9b587371fdb23bc11ceb14e2e2'
        }

        foreach ($entry in $expected.GetEnumerator()) {
            Assert-MetadataValue -Content $descriptor -Name $entry.Key -Value $entry.Value
        }
    }

    It 'links the exact promotion chain from the current ledger and evidence index' {
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $currentAnchorMatch = [regex]::Match(
            $ledger,
            '(?s)## 현재 Anchor\s*(?<body>.*?)(?=\r?\n## |\z)')
        $currentAnchorMatch.Success | Should -BeTrue
        $currentAnchor = $currentAnchorMatch.Groups['body'].Value

        $currentAnchor | Should -Match '\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.74-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`manual-admin-package-pair-current`\s*\|\s*`pass`,\s*`0\.42\.73-admin-smoke -> 0\.42\.74-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.74-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`latest-product-payload-smoke`\s*\|\s*`pass`,\s*package\s*`0\.42\.74-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`functional-correctness-actual-host-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.74-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.74-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`service-plan-p0-save-open-defect`\s*\|\s*`fail-open`'
        $currentAnchor | Should -Match '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md'))

        $index = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $index | Should -Match 'manual-admin-campaign-descriptor-20260820-04273-04274-closed'
        $index | Should -Match '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
    }
}
