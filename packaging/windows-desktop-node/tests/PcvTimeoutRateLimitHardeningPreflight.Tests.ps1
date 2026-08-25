Set-StrictMode -Version Latest

Describe 'PcvTimeoutRateLimitHardeningPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1'

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory = $true)][string]$ArtifactRoot,
                [string]$ApiRoutePrefix = '/api/v1/'
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -ServiceName 'PureCVisorDesktopNode' `
                -ApiRoutePrefix $ApiRoutePrefix `
                -RouteTimeoutSeconds 30 `
                -RequestLimitPerMinute 120 `
                -BurstLimit 20 `
                -RetryAfterSeconds 15 `
                -ErrorContract 'problem-details-json' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating timeout and rate-limit hardening summary' {
        $artifactRoot = Join-Path $TestDrive 'timeout-rate-limit-hardening-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'timeout-rate-limit-hardening-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.timeout_rate_limit_hardening | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.route_timeout_policy | Should -Be 'not-applied'
        $summary.request_limit_policy | Should -Be 'not-applied'
        $summary.retry_semantics_status | Should -Be 'not-run'
        $summary.ui_api_error_contract_status | Should -Be 'not-run'
        $summary.load_test_status | Should -Be 'not-run'
        $summary.server_config_mutation | Should -Be 'not-run'
    }

    It 'records the exact timeout and rate-limit preflight check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'timeout-rate-limit-checks')

        @($summary.hardening_checks | ForEach-Object { $_.name }) | Should -Be @(
            'service-name-present',
            'api-route-prefix-recorded',
            'timeout-policy-recorded',
            'request-limit-policy-recorded',
            'retry-semantics-recorded',
            'ui-api-error-contract-recorded',
            'server-config-not-mutated',
            'middleware-not-enabled',
            'load-test-not-executed',
            'host-mutation-not-executed'
        )
    }

    It 'writes a timeout and rate-limit hardening plan preview without applying middleware or load tests' {
        $artifactRoot = Join-Path $TestDrive 'timeout-rate-limit-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.hardening_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.hardening_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'timeout-rate-limit-hardening-preflight'
        $plan.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.api_route_prefix | Should -Be '/api/v1/'
        $plan.route_timeout_seconds | Should -Be 30
        $plan.request_limit_per_minute | Should -Be 120
        $plan.burst_limit | Should -Be 20
        $plan.retry_after_seconds | Should -Be 15
        $plan.error_contract | Should -Be 'problem-details-json'
        $plan.middleware_status | Should -Be 'not-enabled'
        $plan.load_test_status | Should -Be 'not-run'
        $plan.server_config_status | Should -Be 'not-mutated'
        $plan.planned_operations | Should -Contain 'configure-route-timeout-middleware'
        $plan.planned_operations | Should -Contain 'configure-request-rate-limit-middleware'
        $plan.planned_operations | Should -Contain 'emit-retry-after-contract'
        $plan.planned_operations | Should -Contain 'map-timeout-rate-limit-errors'
        $plan.planned_operations | Should -Contain 'verify-ui-api-error-contract'
        $plan.planned_operations | Should -Contain 'run-rate-limit-load-test'
        $plan.planned_operations | Should -Contain 'record-operational-metrics'
    }

    It 'requires plan-only mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -ServiceName 'PureCVisorDesktopNode' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects routes outside the Local API namespace' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'bad-route-prefix') `
            -ServiceName 'PureCVisorDesktopNode' `
            -ApiRoutePrefix '/admin/' `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, service command, HTTP execution, or load generation text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|Invoke-WebRequest|Invoke-RestMethod|Start-Process|wrk|bombardier|hey\.exe'
    }
}
