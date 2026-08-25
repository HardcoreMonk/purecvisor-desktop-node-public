Set-StrictMode -Version Latest

# docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md went 11 versions and 69 days stale
# without any gate noticing. The existing PcvAdminSmokeEvidenceDocs assertions could not catch
# it: they match unanchored substrings, so `previous_04256_current_manual_admin_package_pair`
# satisfies a `current_manual_admin_package_pair` pattern and every historical value keeps the
# document green forever.
#
# This suite reads only the FIRST line-anchored occurrence of each current_* field and compares
# it against docs/ga-ready/current-evidence.json, which is the canonical owner. Values are
# derived from that record, never pinned, so a legitimate anchor promotion updates both sides
# instead of failing here for the wrong reason.

Describe 'manual-admin campaign descriptor currency' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:DescriptorPath = Join-Path $script:RepoRoot 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $script:EvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.json'
        $script:Record = Get-Content -Raw -LiteralPath $script:EvidencePath | ConvertFrom-Json
        $script:DescriptorLines = @(Get-Content -LiteralPath $script:DescriptorPath)

        function Get-PcvDescriptorFieldMatch {
            param([Parameter(Mandatory)][string]$Field)

            $pattern = '^' + [regex]::Escape($Field) + ':\s*`([^`]*)`\s*$'
            @($script:DescriptorLines | Where-Object { $_ -match $pattern } | ForEach-Object {
                if ($_ -match $pattern) { $Matches[1] }
            })
        }

        function Get-PcvDescriptorField {
            param([Parameter(Mandatory)][string]$Field)

            # @() at the call site, not just inside the helper: PowerShell unwraps a
            # single-element array on function output, and $values[0] on a bare string
            # returns its first character instead of the value.
            $values = @(Get-PcvDescriptorFieldMatch -Field $Field)
            if ($values.Count -eq 0) { return $null }
            $values[0]
        }
    }

    It 'declares each current field exactly once at line start' {
        # A second unprefixed occurrence would make "the current value" ambiguous and reopen the
        # substring-matching hole this suite exists to close.
        $fields = @(
            'descriptor_id'
            'updated_at'
            'current_status'
            'current_manual_admin_package_pair'
            'current_manual_admin_descriptor_batch_manifest'
            'current_manual_admin_campaign'
            'current_installed_operator_surface_current_card_evidence'
            'current_full_admin_host_mutation_gate'
            'current_full_admin_host_mutation_batch'
            'current_full_admin_host_mutation_payload_aggregate_sha256'
            'current_full_admin_host_mutation_operational_msi_sha256'
            'current_full_admin_host_mutation_provenance_commit'
        )

        foreach ($field in $fields) {
            $occurrences = @(Get-PcvDescriptorFieldMatch -Field $field)
            $occurrences.Count | Should -Be 1 -Because "$field must be declared once, unprefixed"
        }
    }

    It 'agrees with the canonical manual-admin closure' {
        $expectedPair = '{0} -> {1}' -f `
            $script:Record.manual_admin.latest_closed_baseline, `
            $script:Record.manual_admin.latest_closed_target

        Get-PcvDescriptorField -Field 'current_manual_admin_package_pair' |
            Should -BeExactly $expectedPair
        Get-PcvDescriptorField -Field 'current_manual_admin_descriptor_batch_manifest' |
            Should -BeExactly $script:Record.manual_admin.latest_closed_descriptor
    }

    It 'agrees with the canonical anchor gate and installed evidence' {
        Get-PcvDescriptorField -Field 'current_full_admin_host_mutation_gate' |
            Should -BeExactly $script:Record.current.fullgate_evidence
        Get-PcvDescriptorField -Field 'current_full_admin_host_mutation_batch' |
            Should -BeExactly $script:Record.current.fullgate_batch
        Get-PcvDescriptorField -Field 'latest_full_admin_gate_batch' |
            Should -BeExactly $script:Record.current.fullgate_batch
        Get-PcvDescriptorField -Field 'current_installed_operator_surface_current_card_evidence' |
            Should -BeExactly $script:Record.current.installed_evidence
    }

    It 'agrees with the canonical anchor hashes and provenance' {
        Get-PcvDescriptorField -Field 'current_full_admin_host_mutation_payload_aggregate_sha256' |
            Should -BeExactly $script:Record.current.payload_sha256
        Get-PcvDescriptorField -Field 'current_full_admin_host_mutation_operational_msi_sha256' |
            Should -BeExactly $script:Record.current.operational_msi_sha256
        Get-PcvDescriptorField -Field 'current_full_admin_host_mutation_provenance_commit' |
            Should -BeExactly $script:Record.current.provenance_commit
    }

    It 'keeps every referenced evidence document on disk' {
        $referenceFields = @(
            'current_manual_admin_campaign'
            'current_installed_operator_surface_current_card_evidence'
            'current_full_admin_host_mutation_gate'
            'current_public_boundary_main_push_evidence'
            'current_installed_account_novnc_evidence'
        )

        foreach ($field in $referenceFields) {
            $relativePath = Get-PcvDescriptorField -Field $field
            $relativePath | Should -Not -BeNullOrEmpty -Because "$field must be present"
            (Join-Path $script:RepoRoot $relativePath) | Should -Exist -Because $field
        }
    }

    It 'demotes rather than deletes the superseded 04259 values' {
        # The document's stated rule is that closing a campaign renames the outgoing current_*
        # fields to previous_<version>_* instead of dropping them. Without this, each update
        # silently erases the predecessor it was supposed to preserve.
        Get-PcvDescriptorField -Field 'previous_04259_current_manual_admin_package_pair' |
            Should -BeExactly '0.42.58-admin-smoke -> 0.42.59-admin-smoke'
        Get-PcvDescriptorField -Field 'previous_04259_current_full_admin_host_mutation_batch' |
            Should -BeExactly 'full-admin-host-mutation-gate-20260529-04259'
        Get-PcvDescriptorField -Field 'previous_04259_descriptor_id' |
            Should -BeExactly 'manual-admin-next-campaign-descriptor-2026-05-29-04259-public-boundary-docs-maintenance-postpush'
    }
}
