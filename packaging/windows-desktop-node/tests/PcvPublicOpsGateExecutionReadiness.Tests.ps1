Set-StrictMode -Version Latest

Describe 'PcvPublicOpsGateExecutionReadiness contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1'

        function New-TestCatalog {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$PackageUri = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.2-windows-x64.update.zip'
            )

            $catalog = [ordered]@{
                schema_version = 1
                product = [ordered]@{
                    id = 'PureCVisor.DesktopNode'
                    name = 'PureCVisor Desktop Node'
                }
                publication = [ordered]@{
                    public_trusted_signing = 'not-claimed'
                    external_stable_publication = 'not-claimed'
                    catalog_publication = 'not-published'
                }
                channels = @(
                    [ordered]@{
                        name = 'stable'
                        version = '0.39.2'
                        package_uri = $PackageUri
                        sha256 = ('A' * 64)
                        release_channel = 'stable'
                    }
                )
            }

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            $catalog | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding utf8
        }
    }

    It 'requires explicit local evidence write opt-in' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'missing-opt-in') `
            -Version '0.39.2' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'records the six remaining requested gates as blocked or pending without public claims' {
        $artifactRoot = Join-Path $TestDrive 'execution-readiness-blocked'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -Version '0.39.2' `
            -AllowLocalEvidenceWrite | Out-Null

        $LASTEXITCODE | Should -Be 0
        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json

        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'public-ops-gate-execution-readiness'
        $summary.actual_execution | Should -Be 'local-execution-readiness-descriptor-written'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_release | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'blocked-by-missing-upload-endpoint-and-credential'
        $summary.winget_submission | Should -Be 'blocked-by-missing-public-installer-url-or-submission-token'
        $summary.clean_host_public_signed_install_update_rollback_smoke | Should -Be 'blocked-by-missing-clean-host-runner-or-public-publication'
        $summary.credential_manager_system_context_proof | Should -Be 'blocked-by-missing-system-context-proof'
        $summary.tls_certificate_lifecycle | Should -Be 'blocked-by-missing-local-tls-lifecycle-opt-in'
        $summary.event_log_hardening | Should -Be 'provider-pass-default-writer-repair-remove-volume-guard-pending'
        @($summary.gates | ForEach-Object { $_.id }) | Should -Be @(
            'external-stable-publication-catalog-upload',
            'winget-submission',
            'clean-host-public-signed-install-update-rollback',
            'windows-credential-manager-service-default-transition',
            'built-in-tls-certificate-lifecycle',
            'windows-event-log-provider-hardening'
        )
    }

    It 'stages catalog/package locally and runs a non-mutating TLS certificate lifecycle slice when opted in' {
        $artifactRoot = Join-Path $TestDrive 'execution-readiness-local'
        $inputRoot = Join-Path $TestDrive 'inputs'
        $catalogPath = Join-Path $inputRoot 'catalog.json'
        $packagePath = Join-Path $inputRoot 'package.zip'
        $stagingRoot = Join-Path $TestDrive 'staging'
        New-TestCatalog -Path $catalogPath -PackageUri 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.2-windows-x64.update.zip'
        Set-Content -LiteralPath $packagePath -Value 'test package bytes' -Encoding utf8

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -Version '0.39.2' `
            -CatalogPath $catalogPath `
            -PackagePath $packagePath `
            -LocalPublicationRoot $stagingRoot `
            -AllowLocalPublicationStaging `
            -RunLocalTlsLifecycle `
            -AllowLocalEvidenceWrite | Out-Null

        $LASTEXITCODE | Should -Be 0
        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $tls = Get-Content -Raw -LiteralPath $summary.tls_lifecycle_path | ConvertFrom-Json

        $summary.catalog_publication | Should -Be 'local-staging-pass-external-not-claimed'
        $summary.external_stable_publication | Should -Be 'local-staging-pass-external-not-claimed'
        $summary.tls_certificate_lifecycle | Should -Be 'partial-code-level-cert-generate-rotate-delete-pass'
        Test-Path -LiteralPath $summary.local_publication.catalog_staged_path | Should -BeTrue
        Test-Path -LiteralPath $summary.local_publication.package_staged_path | Should -BeTrue
        Test-Path -LiteralPath $summary.local_publication.catalog_sha256_path | Should -BeTrue
        $tls.certificate_generation | Should -Be 'code-level-pass'
        $tls.rotation | Should -Be 'code-level-pass'
        $tls.binding | Should -Be 'not-run'
        $tls.private_key_material_written | Should -BeFalse
        Test-Path -LiteralPath $tls.initial_public_certificate_path | Should -BeTrue
        Test-Path -LiteralPath $tls.rotated_public_certificate_path | Should -BeTrue
    }

    It 'imports a SYSTEM-context credential proof artifact when one is supplied' {
        $artifactRoot = Join-Path $TestDrive 'execution-readiness-system-proof'
        $proofPath = Join-Path $TestDrive 'system-proof.json'
        [ordered]@{
            schema_version = 1
            identity = 'NT AUTHORITY\SYSTEM'
            credential_target = 'PureCVisor/PureCVisorDesktopNode/api-token'
            credential_write_status = 'pass'
            credential_read_status = 'pass'
            credential_delete_status = 'pass'
            token_value_observed = $false
            service_reload_status = 'restarted'
            old_source_rejection_status = 'old-source-rejected'
            rollback_diagnostics_status = 'written'
        } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $proofPath -Encoding utf8

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -Version '0.39.2' `
            -CredentialSystemProofPath $proofPath `
            -AllowLocalEvidenceWrite | Out-Null

        $LASTEXITCODE | Should -Be 0
        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json

        $summary.credential_manager_system_context_proof | Should -Be 'system-context-proof-import-pass'
        $summary.service_credential_manager_default_transition | Should -Be 'system-context-proof-import-pass'
        $summary.credential_manager_proof.identity | Should -Be 'NT AUTHORITY\SYSTEM'
        $summary.credential_manager_proof.token_value_observed | Should -BeFalse
    }

    It 'does not contain direct public submission, clean-host execution, or host mutation command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'winget\s+submit|wingetcreate\s+submit|gh\s+release\s+upload|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|netsh\s+http|Import-PfxCertificate|Export-PfxCertificate|Start-Service|Stop-Service|Restart-Service'
    }
}
