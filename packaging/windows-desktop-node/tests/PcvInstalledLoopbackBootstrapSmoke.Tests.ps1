Set-StrictMode -Version Latest

Describe 'Installed loopback bootstrap smoke runner' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:ScriptPath = Join-Path $script:RepoRoot 'packaging\windows-desktop-node\tools\Invoke-PcvInstalledLoopbackBootstrapSmoke.ps1'
    }

    It 'ships a redacted installed loopback session and Chromium bootstrap runner' {
        Test-Path -LiteralPath $script:ScriptPath | Should -BeTrue
        $content = Get-Content -Raw -LiteralPath $script:ScriptPath
        $content | Should -Match '/api/v1/auth/loopback-session'
        $content | Should -Match '/api/v1/auth/session'
        $content | Should -Match '/api/v1/runtime/policy'
        $content | Should -Match '/pcv-config.js'
        $content | Should -Match 'pcvDesktopAccountSession\.v1'
        $content | Should -Match 'token_value_observed = \$false'
        $content | Should -Match 'host_mutation_performed = \$false'
        $content | Should -Match 'public_trusted_signing'
        $content | Should -Match 'external_stable_publication'
        $content | Should -Not -Match 'Authorization header value'
    }
}
