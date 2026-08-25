Set-StrictMode -Version Latest

Describe 'PcvPublicOpsFinalFollowupAttempt evidence contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1'
    }

    It 'requires an explicit local evidence write opt-in' {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'missing-opt-in') `
            -Version '0.39.1-admin-smoke' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'records all seven requested public operations follow-up items without making public claims' {
        $artifactRoot = Join-Path $TestDrive 'public-ops-final-followup-attempt'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -Version '0.39.1-admin-smoke' `
            -AllowLocalEvidenceWrite | Out-Null

        $LASTEXITCODE | Should -Be 0

        $summaryPath = Join-Path $artifactRoot 'summary.json'
        $itemsPath = Join-Path $artifactRoot 'remaining-follow-up-items.json'

        Test-Path -LiteralPath $summaryPath | Should -BeTrue
        Test-Path -LiteralPath $itemsPath | Should -BeTrue

        $summary = Get-Content -Raw -LiteralPath $summaryPath | ConvertFrom-Json
        $items = @(Get-Content -Raw -LiteralPath $itemsPath | ConvertFrom-Json)

        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'public-ops-final-followup-attempt'
        $summary.actual_execution | Should -Be 'local-final-followup-prerequisite-scan-executed'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_release | Should -Be 'not-claimed'
        $summary.public_trusted_signing | Should -Be 'blocked-by-missing-public-signing-material'
        $summary.timestamp_evidence | Should -Be 'blocked-by-missing-public-signing-cert-and-timestamp-url'
        $summary.external_stable_publication | Should -Be 'blocked-by-missing-upload-endpoint-and-credentials'
        $summary.catalog_publication | Should -Be 'not-uploaded'
        $summary.winget_submission | Should -Be 'blocked-by-no-public-signed-stable-installer-and-public-url'
        $summary.clean_host_public_signed_install_update_rollback_smoke | Should -Be 'blocked-by-public-signing-publication-and-clean-host'
        $summary.credential_manager_transition | Should -Be 'capability-pass-service-transition-blocked'
        $summary.service_credential_manager_default_transition | Should -Be 'blocked-by-service-account-context'
        $summary.tls_certificate_lifecycle | Should -Be 'blocked-by-no-mutation-preflight'
        $summary.event_log_provider_transition | Should -Be 'installed-provider-register-write-pass'
        $summary.event_log_hardening | Should -Be 'provider-pass-default-writer-repair-remove-volume-guard-pending'

        @($items.id) | Should -Be @(
            '1-public-trusted-signing-timestamp',
            '2-external-stable-publication-catalog-upload',
            '3-winget-submission',
            '4-clean-host-public-signed-install-update-rollback',
            '5-windows-credential-manager-service-default-transition',
            '6-built-in-tls-certificate-lifecycle',
            '7-windows-event-log-provider-hardening'
        )

        foreach ($item in $items) {
            $item.public_claim | Should -Be 'not-claimed'
            [string]::IsNullOrWhiteSpace([string]$item.next_required_evidence) | Should -BeFalse
        }
    }

    It 'does not contain host mutation, external submission, or publication command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|git\s+push|gh\s+release\s+upload|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|netsh\s+http|New-EventLog|Register-EventSource|Invoke-WebRequest|Invoke-RestMethod'
    }
}
