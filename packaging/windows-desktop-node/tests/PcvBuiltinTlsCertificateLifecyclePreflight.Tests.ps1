Set-StrictMode -Version Latest

Describe 'PcvBuiltinTlsCertificateLifecyclePreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1'

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory = $true)][string]$ArtifactRoot,
                [string]$CertificateSubject = 'CN=PureCVisor Desktop Node Local API',
                [string]$HttpsBindPrefix = 'https://127.0.0.1:7443/'
            )

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -ServiceName 'PureCVisorDesktopNode' `
                -CertificateSubject $CertificateSubject `
                -HttpsBindPrefix $HttpsBindPrefix `
                -CurrentTlsMode 'external-terminator-or-none' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating built-in TLS certificate lifecycle summary' {
        $artifactRoot = Join-Path $TestDrive 'builtin-tls-certificate-lifecycle-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'builtin-tls-certificate-lifecycle-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.tls_certificate_lifecycle | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.tls_certificate_mutation | Should -Be 'not-run'
        $summary.private_key_material_created | Should -BeFalse
        $summary.trust_store_mutation | Should -Be 'not-run'
        $summary.lan_binding_mutation | Should -Be 'not-run'
    }

    It 'records the exact built-in TLS lifecycle check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'tls-checks')

        @($summary.lifecycle_checks | ForEach-Object { $_.name }) | Should -Be @(
            'service-name-present',
            'certificate-subject-present',
            'https-bind-prefix-recorded',
            'current-tls-mode-recorded',
            'target-tls-mode-recorded',
            'private-key-not-created',
            'certificate-import-not-executed',
            'trust-store-mutation-not-executed',
            'lan-binding-not-executed',
            'host-mutation-not-executed'
        )
    }

    It 'writes a lifecycle plan preview without creating certificate material' {
        $artifactRoot = Join-Path $TestDrive 'tls-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.lifecycle_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.lifecycle_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'builtin-tls-certificate-lifecycle-preflight'
        $plan.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.certificate_subject | Should -Be 'CN=PureCVisor Desktop Node Local API'
        $plan.https_bind_prefix | Should -Be 'https://127.0.0.1:7443/'
        $plan.current_tls_mode | Should -Be 'external-terminator-or-none'
        $plan.target_tls_mode | Should -Be 'built-in-service-certificate'
        $plan.private_key_material_created | Should -BeFalse
        $plan.certificate_import_status | Should -Be 'not-run'
        $plan.planned_operations | Should -Contain 'inspect-current-tls-policy'
        $plan.planned_operations | Should -Contain 'generate-service-certificate'
        $plan.planned_operations | Should -Contain 'plan-certificate-private-key-storage'
        $plan.planned_operations | Should -Contain 'bind-https-listener'
        $plan.planned_operations | Should -Contain 'install-trust-anchor-if-approved'
        $plan.planned_operations | Should -Contain 'rotate-certificate'
        $plan.planned_operations | Should -Contain 'remove-certificate-and-binding'
    }

    It 'requires plan-only mode' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -ServiceName 'PureCVisorDesktopNode' `
            -CertificateSubject 'CN=PureCVisor Desktop Node Local API' `
            -HttpsBindPrefix 'https://127.0.0.1:7443/' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a non-HTTPS bind prefix' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'bad-prefix') `
            -ServiceName 'PureCVisorDesktopNode' `
            -CertificateSubject 'CN=PureCVisor Desktop Node Local API' `
            -HttpsBindPrefix 'http://127.0.0.1:7443/' `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, certificate creation, trust-store, or TLS binding command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|New-SelfSignedCertificate|Import-Certificate|Import-PfxCertificate|Export-PfxCertificate|certutil|netsh\s+http|Cert:'
    }
}
