Set-StrictMode -Version Latest

Describe 'PcvServiceTokenRotationRevokePreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1'

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory = $true)][string]$ArtifactRoot,
                [string]$ProtectedTokenPath = '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json',
                [string]$RotationMode = 'rotate-and-revoke-old-token'
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -ServiceName 'PureCVisorDesktopNode' `
                -ProtectedTokenPath $ProtectedTokenPath `
                -CurrentTokenStorage 'dpapi-local-machine-protected-file' `
                -RotationMode $RotationMode `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating service token rotation revoke summary' {
        $artifactRoot = Join-Path $TestDrive 'service-token-rotation-revoke-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'service-token-rotation-revoke-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.service_token_rotation_revoke | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.service_token_mutation | Should -Be 'not-run'
        $summary.service_token_value_observed | Should -BeFalse
        $summary.new_token_value_created | Should -BeFalse
        $summary.service_reload_status | Should -Be 'not-run'
        $summary.old_token_rejection_status | Should -Be 'not-run'
        $summary.token_rotation_audit_status | Should -Be 'not-run'
    }

    It 'records the exact service token rotation revoke check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'service-token-checks')

        @($summary.rotation_checks | ForEach-Object { $_.name }) | Should -Be @(
            'service-name-present',
            'current-token-storage-recorded',
            'protected-token-path-recorded',
            'rotation-mode-recorded',
            'token-value-not-read',
            'new-token-not-generated',
            'protected-token-write-not-executed',
            'service-reload-not-executed',
            'old-token-rejection-not-executed',
            'audit-record-not-written',
            'host-mutation-not-executed'
        )
    }

    It 'writes a rotation revoke plan preview without token generation or writes' {
        $artifactRoot = Join-Path $TestDrive 'service-token-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.rotation_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.rotation_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'service-token-rotation-revoke-preflight'
        $plan.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.protected_token_path | Should -Be '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json'
        $plan.current_token_storage | Should -Be 'dpapi-local-machine-protected-file'
        $plan.rotation_mode | Should -Be 'rotate-and-revoke-old-token'
        $plan.service_token_value_observed | Should -BeFalse
        $plan.new_token_value_created | Should -BeFalse
        $plan.protected_token_write_status | Should -Be 'not-run'
        $plan.planned_operations | Should -Contain 'inspect-current-token-storage-policy'
        $plan.planned_operations | Should -Contain 'plan-new-protected-token-record'
        $plan.planned_operations | Should -Contain 'replace-protected-token-record'
        $plan.planned_operations | Should -Contain 'reload-service-token-policy'
        $plan.planned_operations | Should -Contain 'verify-old-token-rejected'
        $plan.planned_operations | Should -Contain 'write-token-rotation-audit-record'
        $plan.planned_operations | Should -Contain 'rollback-to-previous-protected-token'
    }

    It 'requires plan-only mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -ServiceName 'PureCVisorDesktopNode' `
            -ProtectedTokenPath '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects an unsupported rotation mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'bad-mode') `
            -ServiceName 'PureCVisorDesktopNode' `
            -ProtectedTokenPath '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json' `
            -RotationMode 'rotate-without-revoke' `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, token generation, token write, or service reload command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|New-Guid|RNGCryptoServiceProvider|RandomNumberGenerator|ConvertTo-SecureString|Protect-CmsMessage|CryptProtectData|icacls|Set-Acl'
    }
}
