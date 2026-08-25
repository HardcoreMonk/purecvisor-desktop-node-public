Set-StrictMode -Version Latest

Describe 'PcvWindowsCredentialManagerTransitionPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1'

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory = $true)][string]$ArtifactRoot,
                [string]$CredentialTarget = 'PureCVisor/DesktopNode/LocalApiToken',
                [string]$CurrentTokenStorage = 'dpapi-local-machine-protected-file'
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -ServiceName 'PureCVisorDesktopNode' `
                -CredentialTarget $CredentialTarget `
                -CurrentTokenStorage $CurrentTokenStorage `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating Windows Credential Manager transition summary' {
        $artifactRoot = Join-Path $TestDrive 'windows-credential-manager-transition-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'windows-credential-manager-transition-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.credential_manager_transition | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.credential_manager_mutation | Should -Be 'not-run'
        $summary.token_value_observed | Should -BeFalse
        $summary.service_name | Should -Be 'PureCVisorDesktopNode'
        $summary.credential_target | Should -Be 'PureCVisor/DesktopNode/LocalApiToken'
    }

    It 'records the exact Credential Manager transition check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'credential-manager-checks')

        @($summary.transition_checks | ForEach-Object { $_.name }) | Should -Be @(
            'service-name-present',
            'credential-target-present',
            'current-token-storage-recorded',
            'target-token-storage-recorded',
            'token-value-not-read',
            'credential-write-not-executed',
            'credential-delete-not-executed',
            'rollback-diagnostics-required',
            'service-reload-required',
            'host-mutation-not-executed'
        )
    }

    It 'writes a transition plan preview without reading or writing token values' {
        $artifactRoot = Join-Path $TestDrive 'credential-manager-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.transition_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.transition_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'windows-credential-manager-transition-preflight'
        $plan.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.credential_target | Should -Be 'PureCVisor/DesktopNode/LocalApiToken'
        $plan.current_token_storage | Should -Be 'dpapi-local-machine-protected-file'
        $plan.target_token_storage | Should -Be 'windows-credential-manager'
        $plan.migration_status | Should -Be 'not-run'
        $plan.rollback_diagnostics | Should -Be 'required-before-pass'
        $plan.token_value_observed | Should -BeFalse
        $plan.planned_operations | Should -Contain 'read-existing-protected-token-metadata'
        $plan.planned_operations | Should -Contain 'write-credential-target'
        $plan.planned_operations | Should -Contain 'reload-service-token'
        $plan.planned_operations | Should -Contain 'verify-old-token-rejected'
        $plan.planned_operations | Should -Contain 'rollback-to-protected-file'
    }

    It 'requires plan-only mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -ServiceName 'PureCVisorDesktopNode' `
            -CredentialTarget 'PureCVisor/DesktopNode/LocalApiToken' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a credential target with control characters' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'bad-target') `
            -ServiceName 'PureCVisorDesktopNode' `
            -CredentialTarget "PureCVisor`nDesktopNode" `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, service restart, or credential mutation command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|cmdkey|New-StoredCredential|Remove-StoredCredential|Get-StoredCredential|CredWrite|CredDelete|WriteCredential'
    }
}
