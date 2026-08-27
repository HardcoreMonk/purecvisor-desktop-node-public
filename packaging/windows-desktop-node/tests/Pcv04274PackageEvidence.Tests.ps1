Set-StrictMode -Version Latest

Describe '0.42.75 SERVICE_PLAN P0 promotion evidence contract' {
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

    It 'pins the canonical current evidence record to the exact 0.42.75 tuple' {
        $record = Get-RepoText -RelativePath 'docs/ga-ready/current-evidence.json' | ConvertFrom-Json

        $record.schema_version | Should -Be 1
        $record.contract | Should -BeExactly 'pcv-current-evidence-v1'
        $record.current.version | Should -BeExactly '0.42.75-admin-smoke'
        (@($record.current.operator_surfaces) -join ',') | Should -BeExactly 'web,cli'
        $record.current.tui_present | Should -BeFalse
        $record.current.package_evidence | Should -BeExactly 'docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md'
        $record.current.fullgate_batch | Should -BeExactly 'full-admin-host-mutation-gate-20260821-04275'
        $record.current.fullgate_evidence | Should -BeExactly 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md'
        $record.current.functional_evidence | Should -BeExactly 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md'
        $record.current.installed_evidence | Should -BeExactly 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md'
        $record.current.clean_msi_sha256 | Should -BeExactly '3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6'
        $record.current.operational_msi_sha256 | Should -BeExactly 'd5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b'
        $record.current.payload_sha256 | Should -BeExactly 'b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7'
        $record.current.provenance_commit | Should -BeExactly 'dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4'
        $record.manual_admin.latest_closed_baseline | Should -BeExactly '0.42.74-admin-smoke'
        $record.manual_admin.latest_closed_target | Should -BeExactly '0.42.75-admin-smoke'
        $record.manual_admin.latest_closed_descriptor | Should -BeExactly 'manual-admin-campaign-descriptor-20260827-04274-04275'
        $record.feature_qualification.promotion_eligible | Should -BeTrue
        @($record.feature_qualification.blockers).Count | Should -Be 0
        $record.claims.public_trusted_signing | Should -BeFalse
        $record.claims.external_stable_publication | Should -BeFalse
    }

    It 'records the clean 0.42.75 package as current' {
        $relativePath = 'docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md'
        $package = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $package -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $package -Name 'version' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $package -Name 'source_commit' -Value 'dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4'
        Assert-MetadataValue -Content $package -Name 'artifact_root' -Value 'artifacts/admin-smoke-package-20260821-04275'
        Assert-MetadataValue -Content $package -Name 'signing_mode' -Value 'AllowUnsignedDev'
        Assert-MetadataValue -Content $package -Name 'signing_trust_model' -Value 'LocalTest'
        Assert-MetadataValue -Content $package -Name 'clean_package_msi_sha256' -Value '3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6'
        Assert-MetadataValue -Content $package -Name 'clean_package_payload_aggregate_sha256' -Value '3c33a35b21eb9cdd2b24156cc98afe2268f82f3ca32c7dd6a03882a262afdd2c'
        Assert-MetadataValue -Content $package -Name 'payload_file_count' -Value '8'
        Assert-MetadataValue -Content $package -Name 'host_mutation_performed' -Value 'false'
        Assert-MetadataValue -Content $package -Name 'package_installed' -Value 'false'
        Assert-MetadataValue -Content $package -Name 'canonical_current_evidence' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $package -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $package -Name 'public_trusted_signing' -Value 'not-claimed'
        Assert-MetadataValue -Content $package -Name 'external_stable_publication' -Value 'not-claimed'

        $package | Should -Match 'full admin host mutation'
        $package | Should -Match 'manual-admin-campaign-descriptor-20260827-04274-04275'
    }

    It 'closes the 0.42.74 -> 0.42.75 pair as current and opens the next not-opened pair' {
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'

        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_package_pair' -Value '0.42.74-admin-smoke -> 0.42.75-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'latest_manual_admin_candidate_status' -Value 'pass-closed'
        Assert-MetadataValue -Content $descriptor -Name 'latest_manual_admin_candidate_package_pair' -Value '0.42.74-admin-smoke -> 0.42.75-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'next_manual_admin_package_pair_candidate' -Value '0.42.75-admin-smoke -> next-admin-smoke-required'
        Assert-MetadataValue -Content $descriptor -Name 'next_manual_admin_package_pair_candidate_status' -Value 'not-opened-awaiting-next-product-payload'
        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_update_package_sha256' -Value 'ecae6e9fc7f2f3c49e12a7fec5b4e6d7ca0ce8ba017adf7970cb516a7b5e15df'
        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_descriptor_batch_manifest' -Value 'manual-admin-campaign-descriptor-20260827-04274-04275'
        Assert-MetadataValue -Content $descriptor -Name 'current_manual_admin_target_msi_sha256' -Value '3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6'
        Assert-MetadataValue -Content $descriptor -Name 'current_full_admin_host_mutation_provenance_commit' -Value 'dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_package_candidate_decision' -Value 'docs-only-04275-promotion-retains-0.42.75-admin-smoke'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_evidence' -Value 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-27-04275-promotion-postpush-pass.md'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_run_id' -Value '33064087018'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_job_id' -Value '98489770067'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_head_sha' -Value '7cdd56bf0ff3ded2b9541cd242bd1d68905c0e66'
        Assert-MetadataValue -Content $descriptor -Name 'current_public_boundary_main_push_product_payload_change_detected' -Value 'false'
    }

    It 'indexes 0.42.75 as generated current and keeps the 04274 save defect historical' {
        $index = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $control = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'

        $index | Should -Match 'docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md'
        $index | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md'
        $index | Should -Match 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md'
        $index | Should -Match 'canonical current는 `0\.42\.75-admin-smoke`다'
        $control | Should -Match 'docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md'
        $control | Should -Match 'operational current는 `0\.42\.75-admin-smoke`다'
        $ledger | Should -Match '\|\s*`manual-admin-package-pair-next`\s*\|\s*`not-opened-awaiting-next-product-payload`,\s*`0\.42\.75-admin-smoke -> next-admin-smoke-required`\s*\|'
        $ledger | Should -Match '\|\s*`service-plan-p0-save-historical-defect`\s*\|\s*`fail-historical`'
        $index | Should -Match 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md'
        $index | Should -Match 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md'
        $index | Should -Match 'docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-27-04275.md'
        $index | Should -Match 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-27-04275-promotion-postpush-pass.md'
        $ledger | Should -Match '\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.75-admin-smoke`\s*\|'
        $ledger | Should -Match '\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.75-admin-smoke`\s*\|'
        $ledger | Should -Match '\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.75-admin-smoke`\s*\|'
    }

    It 'records the 0.42.75 fullgate PASS as current' {
        $relativePath = 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md'
        $fullgate = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $fullgate -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $fullgate -Name 'version' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $fullgate -Name 'batch_id' -Value 'full-admin-host-mutation-gate-20260821-04275'
        Assert-MetadataValue -Content $fullgate -Name 'operational_fullgate_msi_sha256' -Value 'd5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b'
        Assert-MetadataValue -Content $fullgate -Name 'operational_fullgate_payload_aggregate_sha256' -Value 'b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7'
        Assert-MetadataValue -Content $fullgate -Name 'provenance_commit' -Value 'dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4'
        Assert-MetadataValue -Content $fullgate -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $fullgate -Name 'canonical_current_evidence' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $fullgate -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $fullgate -Name 'public_trusted_signing' -Value 'excluded'
        Assert-MetadataValue -Content $fullgate -Name 'external_stable_publication' -Value 'not-claimed'

        $fullgate | Should -Match 'pcv-spike-api-8f5c8162'
        $fullgate | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
        $fullgate | Should -Match 'remaining_pcv_vms=\[\]'
    }

    It 'promotes the 0.42.75 installed current-card with carried-forward token evidence' {
        $relativePath = 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md'
        $card = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $card -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $card -Name 'version' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $card -Name 'tui_present' -Value 'false'
        Assert-MetadataValue -Content $card -Name 'summary_sha256' -Value '3c0378fc0046e328b5637e5872d349920b01bd53a671567fa947e643538f6ce6'
        Assert-MetadataValue -Content $card -Name 'cli_exit_zero_count' -Value '3'
        Assert-MetadataValue -Content $card -Name 'web_http_200_count' -Value '2'
        Assert-MetadataValue -Content $card -Name 'secret_observed' -Value 'false'
        Assert-MetadataValue -Content $card -Name 'host_mutation_performed' -Value 'false'
        Assert-MetadataValue -Content $card -Name 'promotion_ledger_status' -Value 'promoted-current'
        Assert-MetadataValue -Content $card -Name 'canonical_current_evidence' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $card -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $card -Name 'latest_manual_admin_package_pair' -Value '0.42.74-admin-smoke -> 0.42.75-admin-smoke'
        Assert-MetadataValue -Content $card -Name 'token_rotation_evidence' -Value 'docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'
        Assert-MetadataValue -Content $card -Name 'token_rotation_r4_summary_sha256' -Value '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        Assert-MetadataValue -Content $card -Name 'token_rotation_status' -Value 'carry-forward-no-token-payload-change-after-04272'
        Assert-MetadataValue -Content $card -Name 'public_trusted_signing' -Value 'not-claimed'
        Assert-MetadataValue -Content $card -Name 'external_stable_publication' -Value 'not-claimed'
    }

    It 'records the 0.42.75 functional actual-VM PASS as current' {
        $relativePath = 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md'
        $functional = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $functional -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $functional -Name 'summary_sha256' -Value 'a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5'
        Assert-MetadataValue -Content $functional -Name 'vm_name' -Value 'pcv-fc-cf-04275'
        Assert-MetadataValue -Content $functional -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $functional -Name 'canonical_current_evidence' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $functional -Name 'public_trusted_signing' -Value 'not-claimed'
    }

    It 'keeps the 0.42.74 P0 actual-VM save failure as historical after 04275 promotion' {
        $relativePath = 'docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md'
        $p0 = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $p0 -Name 'result' -Value 'FAIL'
        Assert-MetadataValue -Content $p0 -Name 'summary_sha256' -Value '11d8d1b34d6e6ff49e2ebb81bc234d20b7eab9f1299baa36ce8daac9c9b14e5d'
        Assert-MetadataValue -Content $p0 -Name 'host_mutation_performed' -Value 'true'
        $p0 | Should -Match '32775'
        $p0 | Should -Match 'RequestedState `6`'
        $p0 | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
        $p0 | Should -Match '열린 결함'
        Assert-MetadataValue -Content $p0 -Name 'public_trusted_signing' -Value 'not-claimed'

        $currentP0 = Get-RepoText -RelativePath 'docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-27-04275.md'
        Assert-MetadataValue -Content $currentP0 -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $currentP0 -Name 'canonical_current_evidence' -Value '0.42.75-admin-smoke'
    }

    It 'records the 0.42.74 -> 0.42.75 manual-admin pair PASS as current' {
        $relativePath = 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md'
        $pair = Get-RepoText -RelativePath $relativePath

        Assert-MetadataValue -Content $pair -Name 'result' -Value 'PASS'
        Assert-MetadataValue -Content $pair -Name 'baseline_version' -Value '0.42.74-admin-smoke'
        Assert-MetadataValue -Content $pair -Name 'target_version' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $pair -Name 'descriptor_batch_id' -Value 'manual-admin-campaign-descriptor-20260827-04274-04275'
        Assert-MetadataValue -Content $pair -Name 'target_msi_sha256' -Value '3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6'
        Assert-MetadataValue -Content $pair -Name 'update_zip_sha256' -Value 'ecae6e9fc7f2f3c49e12a7fec5b4e6d7ca0ce8ba017adf7970cb516a7b5e15df'
        Assert-MetadataValue -Content $pair -Name 'host_mutation_performed' -Value 'true'
        Assert-MetadataValue -Content $pair -Name 'canonical_current_evidence' -Value '0.42.75-admin-smoke'
        Assert-MetadataValue -Content $pair -Name 'canonical_current_changed' -Value 'true'
        Assert-MetadataValue -Content $pair -Name 'public_trusted_signing' -Value 'not-claimed'
        $pair | Should -Match 'runner_count=6'
        $pair | Should -Match 'KB5120242'
    }

    It 'links the closed package pair exactly from the manual-admin descriptor' {
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $expected = [ordered]@{
            current_manual_admin_package_pair = '0.42.74-admin-smoke -> 0.42.75-admin-smoke'
            current_manual_admin_campaign = 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md'
            current_manual_admin_campaign_root = 'artifacts/manual-admin-campaign-20260827-04274-04275'
            current_manual_admin_target_package_root = 'artifacts/admin-smoke-package-20260821-04275'
            current_manual_admin_target_msi_sha256 = '3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6'
            current_manual_admin_update_package_sha256 = 'ecae6e9fc7f2f3c49e12a7fec5b4e6d7ca0ce8ba017adf7970cb516a7b5e15df'
            current_manual_admin_descriptor_batch_manifest = 'manual-admin-campaign-descriptor-20260827-04274-04275'
            current_manual_admin_descriptor_summary = 'artifacts/manual-admin-campaign-20260827-04274-04275/manual-admin-campaign-descriptor/summary.json'
            current_installed_operator_surface_current_card_evidence = 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md'
            latest_manual_admin_candidate_package_pair = '0.42.74-admin-smoke -> 0.42.75-admin-smoke'
            latest_manual_admin_candidate_campaign = 'docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md'
            latest_manual_admin_candidate_descriptor_batch_manifest = 'manual-admin-campaign-descriptor-20260827-04274-04275'
            latest_manual_admin_candidate_status = 'pass-closed'
            current_full_admin_host_mutation_gate = 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md'
            current_full_admin_host_mutation_batch = 'full-admin-host-mutation-gate-20260821-04275'
            current_full_admin_host_mutation_payload_aggregate_sha256 = 'b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7'
            current_full_admin_host_mutation_operational_msi_sha256 = 'd5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b'
            current_full_admin_host_mutation_provenance_commit = 'dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4'
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

        $currentAnchor | Should -Match '\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.75-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`manual-admin-package-pair-current`\s*\|\s*`pass`,\s*`0\.42\.74-admin-smoke -> 0\.42\.75-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.75-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`latest-product-payload-smoke`\s*\|\s*`pass`,\s*package\s*`0\.42\.75-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`functional-correctness-actual-host-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.75-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.75-admin-smoke`\s*\|'
        $currentAnchor | Should -Match '\|\s*`service-plan-p0-save-historical-defect`\s*\|\s*`fail-historical`'
        $currentAnchor | Should -Match '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/admin-smoke-package-2026-08-21-04275.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-21-04275-hostmutation.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-27-04275.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/manual-admin-campaign-2026-08-27-04274-04275.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md'))
        $currentAnchor | Should -Match ([regex]::Escape('docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-27-04275.md'))

        $index = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $index | Should -Match 'manual-admin-campaign-descriptor-20260827-04274-04275'
        $index | Should -Match '285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136'
    }
}
