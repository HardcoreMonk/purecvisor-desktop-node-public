Set-StrictMode -Version Latest

Describe 'PcvOsMutationGateSmoke plan-only contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1'

        function Invoke-TestPlanOnlyOsGate {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot,
                [Parameter(Mandatory)][string]$RouteParityArtifactRoot
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -Version '0.36.2-admin-smoke' `
                -RouteParityArtifactRoot $RouteParityArtifactRoot `
                -ArtifactRoot $ArtifactRoot `
                -LanPrefix <# public-safety: synthetic-rfc1918 #> 'http://192.168.1.17:7777/' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0
            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates summary.json in plan-only mode' {
        $artifactRoot = Join-Path $TestDrive 'os-gate-plan'
        $routeRoot = Join-Path $TestDrive 'routeparity'

        $summary = Invoke-TestPlanOnlyOsGate -ArtifactRoot $artifactRoot -RouteParityArtifactRoot $routeRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.plan_only | Should -BeTrue
    }

    It 'records the exact OS gate step names in plan-only mode' {
        $summary = Invoke-TestPlanOnlyOsGate `
            -ArtifactRoot (Join-Path $TestDrive 'os-gate-steps') `
            -RouteParityArtifactRoot (Join-Path $TestDrive 'routeparity')

        @($summary.steps | ForEach-Object { $_.name }) | Should -Be @(
            'preflight',
            'config-migration-apply-service-running',
            'eventlog-register',
            'eventlog-remove',
            'firewall-enable',
            'lan-listener-ip-smoke',
            'firewall-remove',
            'export-existing-internal-trust-certs',
            'trust-store-install-existing',
            'trust-store-remove-existing',
            'trust-store-restore-existing'
        )
    }

    It 'records evidence classification anchors in plan-only summary' {
        $routeRoot = Join-Path $TestDrive 'routeparity'
        $summary = Invoke-TestPlanOnlyOsGate `
            -ArtifactRoot (Join-Path $TestDrive 'os-gate-classification') `
            -RouteParityArtifactRoot $routeRoot

        $summary.version | Should -Be '0.36.2-admin-smoke'
        $summary.routeparity_artifact | Should -Be ([System.IO.Path]::GetFullPath($routeRoot))
        $summary.lan_prefix | Should -Be 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
        $summary.public_trusted_signing | Should -Be 'excluded'
        $summary.external_stable_publication | Should -Be 'not-claimed'
    }

    It 'plans bearer-required LAN probes for runtime policy and static web assets' {
        $summary = Invoke-TestPlanOnlyOsGate `
            -ArtifactRoot (Join-Path $TestDrive 'os-gate-lan') `
            -RouteParityArtifactRoot (Join-Path $TestDrive 'routeparity')

        @($summary.command_plan.lan_probes | ForEach-Object { $_.path }) | Should -Be @(
            '/api/v1/runtime/policy',
            '/',
            '/index.html',
            '/app.js'
        )
        @($summary.command_plan.lan_probes | Where-Object { $_.auth -ne 'bearer-required' }).Count | Should -Be 0
    }

    It 'states that plan-only did not perform host mutation' {
        $summary = Invoke-TestPlanOnlyOsGate `
            -ArtifactRoot (Join-Path $TestDrive 'os-gate-no-mutation') `
            -RouteParityArtifactRoot (Join-Path $TestDrive 'routeparity')

        $summary.mutates_host | Should -BeFalse
        $summary.host_mutation_performed | Should -BeFalse
        $summary.actual_execution | Should -Be 'not-run'
    }

    It 'does not contain reboot or scheduled task command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe'
    }
}
