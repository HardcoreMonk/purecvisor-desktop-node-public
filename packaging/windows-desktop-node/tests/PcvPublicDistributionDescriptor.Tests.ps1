Set-StrictMode -Version Latest

Describe 'PcvPublicDistributionDescriptor dry-run contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPublicDistributionDescriptor.ps1'

        function Invoke-TestPublicDistributionDescriptor {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -Version '0.39.0-public-candidate' `
                -ArtifactRoot $ArtifactRoot `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0
            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a no-mutation dry-run summary' {
        $artifactRoot = Join-Path $TestDrive 'public-distribution-descriptor'

        $summary = Invoke-TestPublicDistributionDescriptor -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.version | Should -Be '0.39.0-public-candidate'
        $summary.scope | Should -Be 'public-distribution-operations-expansion-candidate'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
    }

    It 'records the exact public distribution and operations gate names' {
        $summary = Invoke-TestPublicDistributionDescriptor -ArtifactRoot (Join-Path $TestDrive 'public-distribution-gates')

        @($summary.gates | ForEach-Object { $_.name }) | Should -Be @(
            'public-signing-preflight',
            'burn-bootstrapper-plan',
            'msix-feasibility-plan',
            'winget-manifest-plan',
            'updater-catalog-publication-plan',
            'public-signed-update-rollback-smoke-plan',
            'credential-manager-transition-plan',
            'eventlog-provider-default-plan',
            'tls-certificate-lifecycle-plan',
            'token-rotation-mutation-plan',
            'diagnostics-server-action-plan',
            'timeout-rate-limit-hardening-plan'
        )
    }

    It 'keeps public release claims explicitly unclaimed' {
        $summary = Invoke-TestPublicDistributionDescriptor -ArtifactRoot (Join-Path $TestDrive 'public-distribution-claims')

        $summary.mutates_host | Should -BeFalse
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.public_release | Should -Be 'not-claimed'
    }

    It 'records required inputs before any public publication claim' {
        $summary = Invoke-TestPublicDistributionDescriptor -ArtifactRoot (Join-Path $TestDrive 'public-distribution-inputs')

        @($summary.command_plan.public_distribution.inputs_required) | Should -Be @(
            'public_trusted_signing_provider',
            'release_approval',
            'publication_target',
            'installer_url',
            'installer_sha256'
        )
    }

    It 'records operations expansion gates separately from publication gates' {
        $summary = Invoke-TestPublicDistributionDescriptor -ArtifactRoot (Join-Path $TestDrive 'public-distribution-ops')

        @($summary.command_plan.operations_expansion.gates) | Should -Contain 'windows-credential-manager-transition'
        @($summary.command_plan.operations_expansion.gates) | Should -Contain 'eventlog-provider-default'
        @($summary.command_plan.operations_expansion.gates) | Should -Contain 'built-in-tls-certificate-lifecycle'
    }

    It 'does not contain host mutation or publication command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove'
    }
}
