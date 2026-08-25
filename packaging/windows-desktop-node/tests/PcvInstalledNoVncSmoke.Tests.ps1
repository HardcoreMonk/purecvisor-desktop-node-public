Describe 'Installed target-backed noVNC smoke contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:NoVncScript = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1'
    }

    It 'ships a target-backed noVNC installed streaming smoke runner with restore and redaction fields' {
        $script:NoVncScript | Should -Exist
        $text = Get-Content -Raw -LiteralPath $script:NoVncScript

        $text | Should -Match 'TcpListener'
        $text | Should -Match 'ClientWebSocket'
        $text | Should -Match 'sc\.exe config'
        $text | Should -Match 'Join-Path \(Split-Path -Parent \$PSScriptRoot\) ''PcvDesktopNodeProduct\.psm1'''
        $text | Should -Match '--novnc-target-host'
        $text | Should -Match '--novnc-target-port'
        $text | Should -Match 'path_name_restored'
        $text | Should -Match 'target_backed_novnc_installed_streaming_smoke'
        $text | Should -Match 'target_frame_sha256'
        $text | Should -Match 'echoed_frame_sha256'
        $text | Should -Match 'token_value_observed\s*=\s*\$false'
        $text | Should -Match 'password_value_observed\s*=\s*\$false'
        $text | Should -Match "public_trusted_signing = 'not-claimed'"
        $text | Should -Match "external_stable_publication = 'not-claimed'"
        $text | Should -Not -Match 'pcvtui\.exe|PCV_TUI_'
    }
}
