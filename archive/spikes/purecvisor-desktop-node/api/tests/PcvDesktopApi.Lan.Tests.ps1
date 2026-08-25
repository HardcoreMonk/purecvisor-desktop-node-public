Describe 'PcvDesktopApi LAN mode hardening' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    It 'keeps loopback prefixes accepted without LAN opt-in' {
        $policy = Assert-PcvApiPrefix -Prefix 'http://127.0.0.1:7777/'

        $policy.exposure | Should -Be 'loopback'
        $policy.host | Should -Be '127.0.0.1'
        $policy.port | Should -Be 7777
        $policy.auth_required | Should -BeFalse
    }

    It 'requires explicit LAN opt-in for non-loopback prefixes' {
        { Assert-PcvApiPrefix -Prefix 'http://0.0.0.0:7777/' } |
            Should -Throw -ExpectedMessage '*PCV_PREFIX_NOT_LOOPBACK*'
    }

    It 'requires a bearer token when LAN mode is enabled' {
        { Assert-PcvApiPrefix -Prefix 'http://0.0.0.0:7777/' -AllowLan } |
            Should -Throw -ExpectedMessage '*PCV_LAN_TOKEN_REQUIRED*'
    }

    It 'accepts non-loopback prefixes only with LAN opt-in and a bearer token' {
        $policy = Assert-PcvApiPrefix `
            -Prefix 'http://0.0.0.0:7777/' `
            -AllowLan `
            -ApiToken '0123456789abcdef'

        $policy.exposure | Should -Be 'lan'
        $policy.host | Should -Be '0.0.0.0'
        $policy.port | Should -Be 7777
        $policy.auth_required | Should -BeTrue
    }

    It 'writes JSONL API event records when an event log path is configured' {
        $eventLogPath = Join-Path $TestDrive 'pcv-api-events.jsonl'

        $result = Write-PcvApiEvent `
            -Path $eventLogPath `
            -EventName 'api.listener.start' `
            -Data ([ordered]@{
                prefix = 'http://0.0.0.0:7777/'
                exposure = 'lan'
                auth_required = $true
            })

        $result.ok | Should -BeTrue
        $record = Get-Content -LiteralPath $eventLogPath -Raw | ConvertFrom-Json
        $record.event | Should -Be 'api.listener.start'
        $record.data.prefix | Should -Be 'http://0.0.0.0:7777/'
        $record.data.exposure | Should -Be 'lan'
        $record.data.auth_required | Should -BeTrue
        $record.timestamp | Should -Not -BeNullOrEmpty
    }

    It 'builds a scoped Windows Firewall rule command from the listener port' {
        $command = New-PcvFirewallRuleCommand `
            -Prefix 'http://0.0.0.0:7777/' `
            -RuleName 'PureCVisor Desktop Node API'

        $command.file_name | Should -Be 'netsh.exe'
        $command.arguments | Should -Contain 'advfirewall'
        $command.arguments | Should -Contain 'firewall'
        $command.arguments | Should -Contain 'add'
        $command.arguments | Should -Contain 'rule'
        $command.arguments | Should -Contain 'name=PureCVisor Desktop Node API'
        $command.arguments | Should -Contain 'dir=in'
        $command.arguments | Should -Contain 'action=allow'
        $command.arguments | Should -Contain 'protocol=TCP'
        $command.arguments | Should -Contain 'localport=7777'
        $command.arguments | Should -Contain 'profile=private'
        $command.arguments | Should -Contain 'enable=yes'
    }

    It 'invokes Windows Firewall rule management through an injectable process runner' {
        $script:FirewallCalls = @()
        $runner = {
            param(
                [string]$FileName,
                [string[]]$Arguments
            )

            $script:FirewallCalls += [pscustomobject]@{
                file_name = $FileName
                arguments = $Arguments
            }

            [ordered]@{
                exit_code = 0
                stdout = 'Ok.'
                stderr = ''
            }
        }

        $result = Invoke-PcvFirewallRuleEnsure `
            -Prefix 'http://0.0.0.0:7777/' `
            -RuleName 'PureCVisor Desktop Node API' `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        $script:FirewallCalls.Count | Should -Be 2
        $script:FirewallCalls[0].arguments | Should -Contain 'delete'
        $script:FirewallCalls[1].arguments | Should -Contain 'add'
        $script:FirewallCalls[1].arguments | Should -Contain 'localport=7777'
    }

    It 'keeps non-loopback static assets behind bearer auth by policy' {
        $policy = Get-PcvApiRuntimePolicy -TokenStorage 'dpapi-local-machine' -CurrentExposure 'lan'

        $policy.network.current_exposure | Should -Be 'lan'
        $policy.network.static_asset_auth.non_loopback | Should -Be 'bearer-required'
        $policy.network.static_asset_auth.loopback | Should -Be 'unauthenticated-static-only'
        $policy.network.tls.required_for_lan | Should -BeTrue
    }
}
