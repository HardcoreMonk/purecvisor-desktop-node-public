Describe 'PcvWindowsEventLogDefaultTransitionSmoke contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1'
    }

    It 'runs an installed MSI LocalSystem Event Log default transition smoke' {
        Test-Path -LiteralPath $script:EntryPoint -PathType Leaf | Should -BeTrue
        $scriptText = Get-Content -LiteralPath $script:EntryPoint -Raw

        $scriptText | Should -Match 'windows-event-log-default-transition-installed'
        $scriptText | Should -Match 'eventlog-default-transition\.json'
        $scriptText | Should -Match 'eventlog-default-transition'
        $scriptText | Should -Match 'msiexec\.exe'
        $scriptText | Should -Match 'installer/build\.ps1'
        $scriptText | Should -Match 'Get-WinEvent'
        $scriptText | Should -Match '--event-log-writer'
        $scriptText | Should -Match 'PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_PATH_MISMATCH'
        $scriptText | Should -Match 'PCV_EVENTLOG_DEFAULT_SMOKE_WRITER_MISMATCH'
    }

    It 'uses the native host action instead of direct PowerShell Event Log mutation cmdlets' {
        $scriptText = Get-Content -LiteralPath $script:EntryPoint -Raw

        $scriptText | Should -Not -Match 'New-EventLog|Remove-EventLog|Write-EventLog|New-ItemProperty|Set-ItemProperty|Remove-ItemProperty'
        $scriptText | Should -Not -Match '--api-token\s+'
    }
}
