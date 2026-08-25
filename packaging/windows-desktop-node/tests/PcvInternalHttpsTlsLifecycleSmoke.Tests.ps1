Describe 'PcvInternalHttpsTlsLifecycleSmoke contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1'
    }

    It 'fails before TLS binding mutation when the installed token source does not match the declared protected-file baseline' {
        Test-Path -LiteralPath $script:EntryPoint -PathType Leaf | Should -BeTrue
        $scriptText = Get-Content -LiteralPath $script:EntryPoint -Raw

        $scriptText | Should -Match 'Assert-ProtectedFileTokenSource'
        $scriptText | Should -Match 'PCV_TLS_SMOKE_TOKEN_SOURCE_MISMATCH'
        $scriptText | Should -Match 'PCV_TLS_SMOKE_TOKEN_SOURCE_PATH_MISMATCH'
        $scriptText | Should -Match '--api-token-protected-file'
        $scriptText | Should -Match '--api-token-credential-target'
    }
}
