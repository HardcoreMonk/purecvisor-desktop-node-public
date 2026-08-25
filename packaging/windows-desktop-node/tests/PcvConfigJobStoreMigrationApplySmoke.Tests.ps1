Set-StrictMode -Version Latest

Describe 'PcvConfigJobStoreMigrationApplySmoke plan-only contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvConfigJobStoreMigrationApplySmoke.ps1'

        function Invoke-TestPlanOnlyMigrationApplySmoke {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -Version '0.38.5-admin-smoke' `
                -ArtifactRoot $ArtifactRoot `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0
            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates summary.json in plan-only mode' {
        $artifactRoot = Join-Path $TestDrive 'migration-apply-plan'

        $summary = Invoke-TestPlanOnlyMigrationApplySmoke -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
    }

    It 'records the exact installed migration apply step names in plan-only mode' {
        $summary = Invoke-TestPlanOnlyMigrationApplySmoke -ArtifactRoot (Join-Path $TestDrive 'migration-apply-steps')

        @($summary.steps | ForEach-Object { $_.name }) | Should -Be @(
            'preflight',
            'build-current-admin-smoke-msi',
            'install-current-msi',
            'stop-installed-service-for-migration',
            'seed-installed-job-store-v1',
            'config-migration-apply-installed',
            'job-store-migration-apply-installed',
            'start-installed-service-after-migration',
            'post-migration-api-read',
            'final-state'
        )
    }

    It 'records supported migration plan identities in the command plan' {
        $summary = Invoke-TestPlanOnlyMigrationApplySmoke -ArtifactRoot (Join-Path $TestDrive 'migration-apply-command-plan')

        $summary.command_plan.config_migration.arguments | Should -Contain '--migration-plan-id'
        $summary.command_plan.config_migration.arguments | Should -Contain 'product-config-v1-to-v2'
        $summary.command_plan.config_migration.arguments | Should -Contain '--migration-plan-version'
        $summary.command_plan.config_migration.arguments | Should -Contain '1'
        $summary.command_plan.job_store_migration.arguments | Should -Contain '--migration-plan-id'
        $summary.command_plan.job_store_migration.arguments | Should -Contain 'job-store-v1-to-v2'
        $summary.command_plan.job_store_migration.arguments | Should -Contain '--migration-plan-version'
        $summary.command_plan.job_store_migration.arguments | Should -Contain '1'
    }

    It 'states that plan-only did not perform host mutation' {
        $summary = Invoke-TestPlanOnlyMigrationApplySmoke -ArtifactRoot (Join-Path $TestDrive 'migration-apply-no-mutation')

        $summary.mutates_host | Should -BeFalse
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'excluded'
        $summary.external_stable_publication | Should -Be 'not-claimed'
    }

    It 'does not contain reboot, scheduler, firewall, trust-store, or Hyper-V mutation command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|New-NetFirewallRule|Remove-NetFirewallRule|New-VM|Remove-VM|trust-store-install|trust-store-remove'
    }
}
