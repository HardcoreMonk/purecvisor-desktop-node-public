BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:TrustScript = Join-Path $script:InstallerRoot 'New-PcvInternalCodeSigningTrust.ps1'
}

Describe 'Desktop Node internal code-signing trust bootstrap contract' {
    It 'exposes a dry-run plan for CurrentUser signing and LocalMachine trust' {
        $output = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:TrustScript `
            -SigningStoreScope CurrentUser `
            -TrustStoreScope LocalMachine `
            -PublicCertificateOutputRoot (Join-Path $TestDrive 'public-certs') `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $output.dry_run | Should -BeTrue
        $output.plan.signing_store_scope | Should -Be 'CurrentUser'
        $output.plan.trust_store_scope | Should -Be 'LocalMachine'
        $output.plan.local_machine_admin_required | Should -BeTrue
        $output.plan.signing_trust_model | Should -Be 'InternalEnterprise'
        $output.plan.secrets_recorded | Should -BeFalse
    }

    It 'does not require administrator privileges for LocalMachine dry-run planning' {
        $jsonText = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:TrustScript `
            -SigningStoreScope LocalMachine `
            -TrustStoreScope LocalMachine `
            -DryRun

        $LASTEXITCODE | Should -Be 0
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeTrue
        $output.plan.local_machine_admin_required | Should -BeTrue
    }

    It 'records only public certificate output paths and build arguments' {
        $scriptText = Get-Content -Raw -LiteralPath $script:TrustScript

        $scriptText | Should -Match 'Export-Certificate'
        $scriptText | Should -Match 'Import-Certificate'
        $scriptText | Should -Match 'TrustedPublisher'
        $scriptText | Should -Match 'InternalEnterprise'
        $scriptText | Should -Match 'CertificateThumbprint'
        $scriptText | Should -Not -Match 'Export-PfxCertificate'
        $scriptText | Should -Not -Match 'PFX password'
    }

    It 'documents dry-run and admin opt-in boundaries for internal RequireSigned release gates' {
        $installerReadme = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'README.md')
        $adr = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot '..\..\..\docs\adr\0003-internal-trusted-signing-policy.md')
        $policy = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot '..\..\..\docs\DEVELOPMENT_VERIFICATION_POLICY.md')
        $docs = $installerReadme + "`n" + $adr + "`n" + $policy

        $installerReadme | Should -Match 'Internal RequireSigned gate runbook'
        $installerReadme | Should -Match 'New-PcvInternalCodeSigningTrust\.ps1.+-DryRun'
        $installerReadme | Should -Match 'Dry-run은 LocalMachine trust import를 실행하지 않는다'
        $installerReadme | Should -Match 'SigningMode RequireSigned'
        $installerReadme | Should -Match 'SigningTrustModel InternalEnterprise'
        $docs | Should -Match '관리자 opt-in'
        $docs | Should -Match 'private key/PFX/password'
        $docs | Should -Match 'public trusted signing 또는 외부 stable publication'
    }
}
