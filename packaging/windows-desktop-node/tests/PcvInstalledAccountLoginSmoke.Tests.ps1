Set-StrictMode -Version Latest

Describe 'Installed account login smoke runner' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ScriptPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1'
    }

    It 'ships an installed account login smoke runner with redacted evidence fields' {
        Test-Path -LiteralPath $script:ScriptPath | Should -BeTrue
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match 'POST /api/v1/auth/login|/api/v1/auth/login'
        $content | Should -Match 'GET /api/v1/auth/session|/api/v1/auth/session'
        $content | Should -Match 'GET /api/v1/auth/rbac|/api/v1/auth/rbac'
        $content | Should -Match '/api/v1/console/capabilities'
        $content | Should -Match 'account_file_backup'
        $content | Should -Match 'jwt_signing_key_backup'
        $content | Should -Match 'acl_sddl'
        $content | Should -Match 'temporary_write_grant_status'
        $content | Should -Match 'temporary_takeown_status'
        $content | Should -Match 'acl_restore_status'
        $content | Should -Match 'takeown\.exe'
        $content | Should -Match 'icacls\.exe'
        $content | Should -Match 'Grant-TemporaryFileWriteAccess'
        $content | Should -Match 'Restore-SavedFileAcl'
        $content | Should -Match 'RunBrowserQa'
        $content | Should -Match 'capture-installed-listener-qa\.mjs'
        $content | Should -Match 'PCV_BROWSER_QA_TOKEN'
        $content | Should -Match 'PCV_BROWSER_QA_ACCOUNT_USERNAME'
        $content | Should -Match 'PCV_BROWSER_QA_ACCOUNT_PASSWORD'
        $content | Should -Match 'browser_qa'
        $content | Should -Match 'token_value_observed\s*=\s*\$false|token_value_observed'
        $content | Should -Match 'password_value_observed\s*=\s*\$false|password_value_observed'
        $content | Should -Match 'public_trusted_signing'
        $content | Should -Match 'external_stable_publication'
    }
}
