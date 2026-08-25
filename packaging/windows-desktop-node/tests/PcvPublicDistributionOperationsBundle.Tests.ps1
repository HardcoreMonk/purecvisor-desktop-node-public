Set-StrictMode -Version Latest

Describe 'PcvPublicDistributionOperationsBundle execution contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPublicDistributionOperationsBundle.ps1'

        function Invoke-TestPublicDistributionOperationsBundle {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -Version '0.39.2' `
                -InstallerUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.2-windows-x64.msi' `
                -UpdatePackageUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.2-windows-x64.update.zip' `
                -PublicCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' `
                -MsiSha256 ('B' * 64) `
                -UpdatePackageSha256 ('C' * 64) `
                -ServiceName 'PureCVisorDesktopNode' `
                -ProtectedTokenPath '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json' `
                -DiagnosticsRoot '%ProgramData%\PureCVisor\desktop-node\diagnostics' `
                -PreserveBranch 'codex/diagnostic-bundle-api-action,codex/diagnostic-bundle-listener-evidence,codex/diagnostic-bundle-product-wrapper-evidence,codex/full-admin-host-mutation-0389-evidence' `
                -AllowLocalDescriptorWrite | Out-Null

            $LASTEXITCODE | Should -Be 0
            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'executes and collects the non-mutating public distribution operations preflight bundle' {
        $artifactRoot = Join-Path $TestDrive 'public-distribution-ops-execution-bundle'

        $summary = Invoke-TestPublicDistributionOperationsBundle -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        Test-Path -LiteralPath $summary.execution_bundle_path | Should -BeTrue
        Test-Path -LiteralPath $summary.follow_up_work_items_path | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'public-distribution-ops-execution-bundle'
        $summary.public_distribution_ops_execution_bundle | Should -Be 'code-level-nonmutating-bundle-pass'
        $summary.actual_execution | Should -Be 'local-preflight-bundle-executed'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.mutates_host | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
    }

    It 'records all requested distribution and operations component steps' {
        $summary = Invoke-TestPublicDistributionOperationsBundle -ArtifactRoot (Join-Path $TestDrive 'public-distribution-ops-components')

        @($summary.component_steps | ForEach-Object { $_.name }) | Should -Be @(
            'public-distribution-descriptor',
            'public-distribution-readiness',
            'burn-bootstrapper-preflight',
            'msix-packaging-feasibility-preflight',
            'winget-manifest-compliance-preflight',
            'updater-catalog-publication-preflight',
            'public-signed-update-rollback-smoke-preflight',
            'windows-credential-manager-transition-preflight',
            'windows-event-log-provider-transition-preflight',
            'builtin-tls-certificate-lifecycle-preflight',
            'service-token-rotation-revoke-preflight',
            'timeout-rate-limit-hardening-preflight',
            'diagnostic-bundle-server-preflight'
        )

        foreach ($step in @($summary.component_steps)) {
            $step.ok | Should -BeTrue
            $step.host_mutation_performed | Should -BeFalse
            $step.public_trusted_signing | Should -Be 'not-claimed'
            $step.external_stable_publication | Should -Be 'not-claimed'
            Test-Path -LiteralPath $step.summary_path | Should -BeTrue
        }
    }

    It 'preserves the requested legacy follow-up branches without deletion' {
        $summary = Invoke-TestPublicDistributionOperationsBundle -ArtifactRoot (Join-Path $TestDrive 'public-distribution-ops-branches')

        $summary.branch_preservation.status | Should -Be 'preserved'
        @($summary.branch_preservation.branches) | Should -Contain 'codex/diagnostic-bundle-api-action'
        @($summary.branch_preservation.branches) | Should -Contain 'codex/diagnostic-bundle-listener-evidence'
        @($summary.branch_preservation.branches) | Should -Contain 'codex/diagnostic-bundle-product-wrapper-evidence'
        @($summary.branch_preservation.branches) | Should -Contain 'codex/full-admin-host-mutation-0389-evidence'
    }

    It 'keeps real public release, credential, event log, TLS, token, and clean-host mutation blocked' {
        $summary = Invoke-TestPublicDistributionOperationsBundle -ArtifactRoot (Join-Path $TestDrive 'public-distribution-ops-claims')

        $summary.component_statuses.catalog_publication | Should -Be 'not-published'
        $summary.component_statuses.winget_submission | Should -Be 'not-submitted'
        $summary.component_statuses.public_signed_update_rollback_smoke | Should -Be 'blocked-by-public-signing-and-publication'
        $summary.component_statuses.clean_host_smoke_status | Should -Be 'not-run'
        $summary.component_statuses.credential_manager_mutation | Should -Be 'not-run'
        $summary.component_statuses.event_log_provider_mutation | Should -Be 'not-run'
        $summary.component_statuses.tls_certificate_mutation | Should -Be 'not-run'
        $summary.component_statuses.service_token_mutation | Should -Be 'not-run'
        $summary.component_statuses.timeout_rate_limit_hardening | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.command_plan.safety.external_publication | Should -Be 'blocked-until-public-signing-and-publication'
        $summary.command_plan.safety.host_mutation | Should -Be 'not-run'
    }

    It 'requires an explicit local descriptor write opt-in' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'missing-opt-in') `
            -Version '0.39.2' `
            -InstallerUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.2-windows-x64.msi' `
            -UpdatePackageUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.2-windows-x64.update.zip' `
            -PublicCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' `
            -MsiSha256 ('B' * 64) `
            -UpdatePackageSha256 ('C' * 64) 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, external submission, or public publication command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|netsh\s+http|New-EventLog|Register-EventSource'
    }
}
