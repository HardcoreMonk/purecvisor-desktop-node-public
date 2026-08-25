Set-StrictMode -Version Latest

Describe 'PcvWindowsEventLogProviderTransitionPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1'

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory = $true)][string]$ArtifactRoot,
                [string]$ProviderName = 'PureCVisor Desktop Node',
                [string]$LogName = 'Application'
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -ServiceName 'PureCVisorDesktopNode' `
                -ProviderName $ProviderName `
                -LogName $LogName `
                -CurrentWriter 'jsonl-first-eventlog-opt-in' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating Windows Event Log provider transition summary' {
        $artifactRoot = Join-Path $TestDrive 'windows-event-log-provider-transition-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'windows-event-log-provider-transition-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.event_log_provider_transition | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.event_log_provider_mutation | Should -Be 'not-run'
        $summary.event_log_write_status | Should -Be 'not-run'
        $summary.provider_name | Should -Be 'PureCVisor Desktop Node'
        $summary.log_name | Should -Be 'Application'
    }

    It 'records the exact Event Log provider transition check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'eventlog-checks')

        @($summary.transition_checks | ForEach-Object { $_.name }) | Should -Be @(
            'service-name-present',
            'provider-name-present',
            'log-name-present',
            'current-writer-recorded',
            'target-writer-recorded',
            'provider-registration-not-executed',
            'provider-removal-not-executed',
            'event-write-not-executed',
            'retention-volume-guard-required',
            'host-mutation-not-executed'
        )
    }

    It 'writes a provider transition plan preview without registry or event writes' {
        $artifactRoot = Join-Path $TestDrive 'eventlog-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.transition_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.transition_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'windows-event-log-provider-transition-preflight'
        $plan.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.provider_name | Should -Be 'PureCVisor Desktop Node'
        $plan.log_name | Should -Be 'Application'
        $plan.current_writer | Should -Be 'jsonl-first-eventlog-opt-in'
        $plan.target_writer | Should -Be 'default-windows-event-log-provider'
        $plan.provider_registration_status | Should -Be 'not-run'
        $plan.event_write_status | Should -Be 'not-run'
        $plan.planned_operations | Should -Contain 'inspect-current-jsonl-writer-policy'
        $plan.planned_operations | Should -Contain 'register-event-log-provider'
        $plan.planned_operations | Should -Contain 'switch-default-writer-to-event-log'
        $plan.planned_operations | Should -Contain 'verify-provider-write-and-query'
        $plan.planned_operations | Should -Contain 'remove-provider-on-uninstall-or-rollback'
        $plan.planned_operations | Should -Contain 'enforce-log-volume-guard'
    }

    It 'requires plan-only mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -ServiceName 'PureCVisorDesktopNode' `
            -ProviderName 'PureCVisor Desktop Node' `
            -LogName 'Application' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a provider name with control characters' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'bad-provider') `
            -ServiceName 'PureCVisorDesktopNode' `
            -ProviderName "PureCVisor`nDesktopNode" `
            -LogName 'Application' `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, registry provider, or event write command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|New-EventLog|Remove-EventLog|Write-EventLog|wevtutil|eventcreate|New-ItemProperty|Set-ItemProperty|Remove-ItemProperty|HKLM:'
    }
}
