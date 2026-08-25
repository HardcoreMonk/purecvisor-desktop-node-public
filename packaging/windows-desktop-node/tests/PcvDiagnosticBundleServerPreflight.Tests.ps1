Set-StrictMode -Version Latest

Describe 'PcvDiagnosticBundleServerPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvDiagnosticBundleServerPreflight.ps1'

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory = $true)][string]$ArtifactRoot,
                [string]$ApiRoute = '/api/v1/diagnostics/bundles'
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -ServiceName 'PureCVisorDesktopNode' `
                -DiagnosticsRoot '%ProgramData%\PureCVisor\desktop-node\diagnostics' `
                -ApiRoute $ApiRoute `
                -DownloadRouteTemplate '/api/v1/diagnostics/bundles/{bundle_id}/download' `
                -RetentionDays 14 `
                -MaxBundleCount 50 `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating diagnostic bundle server-side summary' {
        $artifactRoot = Join-Path $TestDrive 'diagnostic-bundle-server-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'diagnostic-bundle-server-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.diagnostic_bundle_server_generation | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.diagnostic_bundle_api_action | Should -Be 'not-run'
        $summary.diagnostic_bundle_archive_created | Should -BeFalse
        $summary.diagnostic_bundle_download_served | Should -BeFalse
        $summary.diagnostic_bundle_redaction_status | Should -Be 'not-run'
        $summary.diagnostic_bundle_authz_status | Should -Be 'not-run'
        $summary.diagnostic_bundle_retention_status | Should -Be 'not-run'
        $summary.wrapper_collect_diagnostics_execution | Should -Be 'not-run'
    }

    It 'records the exact diagnostic bundle server preflight check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'diagnostic-bundle-checks')

        @($summary.diagnostic_checks | ForEach-Object { $_.name }) | Should -Be @(
            'service-name-present',
            'diagnostics-root-recorded',
            'api-route-recorded',
            'download-route-recorded',
            'authz-policy-recorded',
            'archive-creation-not-executed',
            'download-serving-not-executed',
            'redaction-not-executed',
            'retention-not-executed',
            'wrapper-execution-not-delegated',
            'host-mutation-not-executed'
        )
    }

    It 'writes a server-side diagnostic bundle plan preview without archive or download execution' {
        $artifactRoot = Join-Path $TestDrive 'diagnostic-bundle-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.diagnostic_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.diagnostic_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'diagnostic-bundle-server-preflight'
        $plan.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.diagnostics_root | Should -Be '%ProgramData%\PureCVisor\desktop-node\diagnostics'
        $plan.api_route | Should -Be '/api/v1/diagnostics/bundles'
        $plan.download_route_template | Should -Be '/api/v1/diagnostics/bundles/{bundle_id}/download'
        $plan.authz_policy | Should -Be 'bearer-token-required'
        $plan.redaction_policy | Should -Be 'token-and-host-path-redaction-required'
        $plan.archive_status | Should -Be 'not-run'
        $plan.download_status | Should -Be 'not-run'
        $plan.retention_status | Should -Be 'not-run'
        $plan.planned_operations | Should -Contain 'validate-bearer-authorization'
        $plan.planned_operations | Should -Contain 'request-diagnostic-bundle-generation'
        $plan.planned_operations | Should -Contain 'write-server-side-bundle-archive'
        $plan.planned_operations | Should -Contain 'serve-diagnostic-bundle-download'
        $plan.planned_operations | Should -Contain 'apply-bundle-retention-policy'
        $plan.planned_operations | Should -Contain 'record-bundle-audit-metadata'
    }

    It 'requires plan-only mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -ServiceName 'PureCVisorDesktopNode' `
            -DiagnosticsRoot '%ProgramData%\PureCVisor\desktop-node\diagnostics' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects routes outside the Local API namespace' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'bad-route') `
            -ServiceName 'PureCVisorDesktopNode' `
            -DiagnosticsRoot '%ProgramData%\PureCVisor\desktop-node\diagnostics' `
            -ApiRoute '/admin/diagnostics/bundles' `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, archive creation, wrapper execution, or service command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|Compress-Archive|ZipFile|System\.IO\.Compression|Invoke-PcvDesktopNodeProduct|CollectDiagnostics|Start-Process'
    }
}
