Describe 'PcvDesktopService packaging contract' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopService.psm1'
        $script:Entrypoint = Join-Path $Root 'Invoke-PcvDesktopService.ps1'
        Import-Module $ModulePath -Force
    }

    It 'builds a loopback service config that launches the Local API listener' {
        $apiScript = 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1'
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath $apiScript `
            -Prefix 'http://127.0.0.1:7777/' `
            -JobStorePath 'D:\PureCVisor\desktop-node\jobs.json' `
            -WebRootPath 'D:\repo\web' `
            -EventLogPath 'D:\PureCVisor\desktop-node\events.jsonl' `
            -WorkerCount 4

        $config.service_name | Should -Be 'PureCVisorDesktopNode'
        $config.binary_path | Should -Match 'pwsh.exe'
        $config.binary_path | Should -Not -Match '^"pwsh\.exe"'
        $config.binary_path | Should -Match ([regex]::Escape($apiScript))
        $config.binary_path | Should -Match '-Prefix "http://127.0.0.1:7777/"'
        $config.binary_path | Should -Match '-JobStorePath "D:\\PureCVisor\\desktop-node\\jobs.json"'
        $config.binary_path | Should -Match '-WebRootPath "D:\\repo\\web"'
        $config.binary_path | Should -Match '-EventLogPath "D:\\PureCVisor\\desktop-node\\events.jsonl"'
        $config.binary_path | Should -Match '-WorkerCount 4'
        $config.exposure | Should -Be 'loopback'
    }

    It 'requires an API token when a service config enables LAN mode' {
        { New-PcvDesktopServiceConfig `
                -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
                -Prefix 'http://0.0.0.0:7777/' `
                -AllowLan } |
            Should -Throw -ExpectedMessage '*PCV_SERVICE_LAN_TOKEN_REQUIRED*'
    }

    It 'passes LAN and firewall listener options into the service binary path' {
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
            -Prefix 'http://0.0.0.0:7777/' `
            -AllowLan `
            -ApiToken 'replace-with-a-long-token' `
            -EnsureFirewallRule `
            -FirewallRuleName 'PureCVisor Desktop Node API' `
            -FirewallProfile private

        $config.exposure | Should -Be 'lan'
        $config.binary_path | Should -Match '-AllowLan'
        $config.binary_path | Should -Match '-ApiToken "replace-with-a-long-token"'
        $config.binary_path | Should -Match '-EnsureFirewallRule'
        $config.binary_path | Should -Match '-FirewallRuleName "PureCVisor Desktop Node API"'
        $config.binary_path | Should -Match '-FirewallProfile "private"'
    }

    It 'rejects service install commands that would persist an inline API token' {
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
            -Prefix 'http://0.0.0.0:7777/' `
            -AllowLan `
            -ApiToken 'inline-secret'

        { New-PcvDesktopServiceCommand -Config $config -Action Install } |
            Should -Throw -ExpectedMessage '*PCV_SERVICE_INLINE_TOKEN_INSTALL_FORBIDDEN*'
    }

    It 'passes LAN service auth through an API token file without exposing a token value' {
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
            -Prefix 'http://0.0.0.0:7777/' `
            -AllowLan `
            -ApiTokenFile 'D:\PureCVisor\desktop-node\api-token.txt'

        $config.exposure | Should -Be 'lan'
        $config.auth_required | Should -BeTrue
        $config.api_token_source | Should -Be 'file'
        $config.binary_path | Should -Match '-AllowLan'
        $config.binary_path | Should -Match '-ApiTokenFile "D:\\PureCVisor\\desktop-node\\api-token.txt"'
        $config.binary_path | Should -Not -Match '-ApiToken "'
    }

    It 'rejects ambiguous service API token sources' {
        { New-PcvDesktopServiceConfig `
                -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
                -Prefix 'http://0.0.0.0:7777/' `
                -AllowLan `
                -ApiToken 'inline-secret' `
                -ApiTokenFile 'D:\PureCVisor\desktop-node\api-token.txt' } |
            Should -Throw -ExpectedMessage '*PCV_SERVICE_TOKEN_SOURCE_CONFLICT*'
    }

    It 'prepares a generated token file and ACL runner without returning the token value' {
        $tokenPath = Join-Path $TestDrive 'api-token.txt'
        $script:AclCalls = @()
        $runner = {
            param(
                [string]$FileName,
                [string[]]$Arguments
            )

            $script:AclCalls += [pscustomobject]@{
                file_name = $FileName
                arguments = $Arguments
            }

            [ordered]@{
                exit_code = 0
                stdout = 'processed'
                stderr = ''
            }
        }

        $result = New-PcvDesktopServiceTokenFile `
            -Path $tokenPath `
            -ServiceAccount 'LocalSystem' `
            -InvokeProcess $runner

        $writtenToken = Get-Content -LiteralPath $tokenPath -Raw
        $writtenToken | Should -Not -BeNullOrEmpty
        $writtenToken.Length | Should -BeGreaterThan 32
        $result.ok | Should -BeTrue
        $result.path | Should -Be $tokenPath
        $result.token_length | Should -Be $writtenToken.Length
        @($result.Keys) | Should -Not -Contain 'token'
        $script:AclCalls.Count | Should -Be 2
        $script:AclCalls[0].file_name | Should -Be 'icacls.exe'
        $script:AclCalls[0].arguments | Should -Contain '/inheritance:r'
        $script:AclCalls[1].arguments | Should -Contain 'BUILTIN\Administrators:R'
        $script:AclCalls[1].arguments | Should -Contain 'NT AUTHORITY\SYSTEM:R'
    }

    It 'refuses to overwrite an existing token file unless Force is explicit' {
        $tokenPath = Join-Path $TestDrive 'api-token.txt'
        Set-Content -LiteralPath $tokenPath -Value 'existing-token' -Encoding UTF8 -NoNewline
        $runner = {
            [ordered]@{
                exit_code = 0
                stdout = 'processed'
                stderr = ''
            }
        }

        { New-PcvDesktopServiceTokenFile `
                -Path $tokenPath `
                -Token 'replacement-token' `
                -InvokeProcess $runner } |
            Should -Throw -ExpectedMessage '*PCV_SERVICE_TOKEN_FILE_EXISTS*'

        $result = New-PcvDesktopServiceTokenFile `
            -Path $tokenPath `
            -Token 'replacement-token' `
            -Force `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        Get-Content -LiteralPath $tokenPath -Raw | Should -Be 'replacement-token'
    }

    It 'writes and reads a DPAPI LocalMachine protected token file without returning the token from prepare' {
        $tokenPath = Join-Path $TestDrive 'api-token.dpapi.json'
        $script:AclCalls = @()
        $runner = {
            param(
                [string]$FileName,
                [string[]]$Arguments
            )

            $script:AclCalls += [pscustomobject]@{
                file_name = $FileName
                arguments = $Arguments
            }

            [ordered]@{
                exit_code = 0
                stdout = 'processed'
                stderr = ''
            }
        }

        $result = New-PcvDesktopServiceProtectedTokenFile `
            -Path $tokenPath `
            -Token 'protected-secret' `
            -ServiceAccount 'LocalSystem' `
            -InvokeProcess $runner
        $json = Get-Content -LiteralPath $tokenPath -Raw | ConvertFrom-Json
        $read = Read-PcvDesktopServiceProtectedTokenFile -Path $tokenPath

        $result.ok | Should -BeTrue
        $result.path | Should -Be $tokenPath
        $result.storage | Should -Be 'dpapi-local-machine'
        $result.token_length | Should -Be 'protected-secret'.Length
        @($result.Keys) | Should -Not -Contain 'token'
        $json.schema_version | Should -Be 1
        $json.storage | Should -Be 'dpapi-local-machine'
        $json.scope | Should -Be 'LocalMachine'
        $json.protected_token | Should -Not -BeNullOrEmpty
        $json.protected_token | Should -Not -Match 'protected-secret'
        $json.token_sha256 | Should -Match '^[a-f0-9]{64}$'
        $read.ok | Should -BeTrue
        $read.token | Should -Be 'protected-secret'
        $read.storage | Should -Be 'dpapi-local-machine'
        $script:AclCalls.Count | Should -Be 2
        $script:AclCalls[1].arguments | Should -Contain 'NT AUTHORITY\SYSTEM:R'
    }

    It 'writes and reads a DPAPI LocalMachine protected token file under Windows PowerShell' {
        $tokenPath = Join-Path $TestDrive 'api-token-windows-powershell.dpapi.json'
        $scriptPath = Join-Path $TestDrive 'prepare-protected-token.ps1'
        $escapedModulePath = $ModulePath -replace "'", "''"
        $escapedTokenPath = $tokenPath -replace "'", "''"
        Set-Content -LiteralPath $scriptPath -Encoding UTF8 -Value @"
`$ErrorActionPreference = 'Stop'
Import-Module '$escapedModulePath' -Force
`$runner = {
    [ordered]@{
        exit_code = 0
        stdout = 'processed'
        stderr = ''
    }
}
`$result = New-PcvDesktopServiceProtectedTokenFile -Path '$escapedTokenPath' -Token 'windows-powershell-secret' -InvokeProcess `$runner
`$read = Read-PcvDesktopServiceProtectedTokenFile -Path '$escapedTokenPath'
[ordered]@{
    result = `$result
    read = `$read
} | ConvertTo-Json -Depth 16
"@

        $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.result.ok | Should -BeTrue
        $json.result.storage | Should -Be 'dpapi-local-machine'
        $json.read.ok | Should -BeTrue
        $json.read.token | Should -Be 'windows-powershell-secret'
    }

    It 'rejects ambiguous protected service token sources' {
        { New-PcvDesktopServiceConfig `
                -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
                -Prefix 'http://127.0.0.1:7777/' `
                -ApiTokenFile 'D:\PureCVisor\desktop-node\api-token.txt' `
                -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json' } |
            Should -Throw -ExpectedMessage '*PCV_SERVICE_TOKEN_SOURCE_CONFLICT*'
    }

    It 'passes protected token file source into the service binary path without a raw token file' {
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
            -Prefix 'http://127.0.0.1:7777/' `
            -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json'

        $config.auth_required | Should -BeTrue
        $config.api_token_source | Should -Be 'protected_file'
        $config.binary_path | Should -Match '-ApiTokenProtectedFile "D:\\PureCVisor\\desktop-node\\api-token.dpapi.json"'
        $config.binary_path | Should -Not -Match '-ApiTokenFile "'
        $config.binary_path | Should -Not -Match '-ApiToken "'
    }

    It 'builds token file ACL commands for a custom service account' {
        $commands = New-PcvTokenFileAclCommand `
            -Path 'D:\PureCVisor\desktop-node\api-token.txt' `
            -ServiceAccount '.\pcvsvc'

        $commands.Count | Should -Be 2
        $commands[0].file_name | Should -Be 'icacls.exe'
        $commands[0].arguments | Should -Be @('D:\PureCVisor\desktop-node\api-token.txt', '/inheritance:r')
        $commands[1].arguments | Should -Contain '/grant:r'
        $commands[1].arguments | Should -Contain 'BUILTIN\Administrators:R'
        $commands[1].arguments | Should -Contain '.\pcvsvc:R'
    }

    It 'adds an explicit service account to the install command' {
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1' `
            -ServiceAccount 'LocalSystem'

        $config.service_account | Should -Be 'LocalSystem'

        $install = New-PcvDesktopServiceCommand -Config $config -Action Install
        $install[0].arguments | Should -Contain 'obj='
        $install[0].arguments | Should -Contain 'LocalSystem'
    }

    It 'builds scoped sc.exe commands for install, status, stop, and uninstall' {
        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1'

        $install = New-PcvDesktopServiceCommand -Config $config -Action Install
        $install[0].file_name | Should -Be 'sc.exe'
        $install[0].arguments | Should -Contain 'create'
        $install[0].arguments | Should -Contain 'PureCVisorDesktopNode'
        $install[0].arguments | Should -Contain 'binPath='
        $install[0].arguments | Should -Contain $config.binary_path
        $install[1].arguments | Should -Contain 'description'
        $install[2].arguments | Should -Contain 'failure'

        $status = New-PcvDesktopServiceCommand -Config $config -Action Status
        $status[0].arguments | Should -Be @('query', 'PureCVisorDesktopNode')

        $stop = New-PcvDesktopServiceCommand -Config $config -Action Stop
        $stop[0].arguments | Should -Be @('stop', 'PureCVisorDesktopNode')

        $uninstall = New-PcvDesktopServiceCommand -Config $config -Action Uninstall
        $uninstall[0].arguments | Should -Be @('delete', 'PureCVisorDesktopNode')
    }

    It 'invokes service install commands through an injectable process runner' {
        $script:ServiceCalls = @()
        $runner = {
            param(
                [string]$FileName,
                [string[]]$Arguments
            )

            $script:ServiceCalls += [pscustomobject]@{
                file_name = $FileName
                arguments = $Arguments
            }

            [ordered]@{
                exit_code = 0
                stdout = 'OK'
                stderr = ''
            }
        }

        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1'

        $result = Invoke-PcvDesktopServiceCommand `
            -Config $config `
            -Action Install `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'install'
        $script:ServiceCalls.Count | Should -Be 3
        $script:ServiceCalls[0].arguments | Should -Contain 'create'
        $script:ServiceCalls[1].arguments | Should -Contain 'description'
        $script:ServiceCalls[2].arguments | Should -Contain 'failure'
    }

    It 'stops service command execution when a mutating step fails' {
        $script:ServiceCalls = @()
        $runner = {
            param(
                [string]$FileName,
                [string[]]$Arguments
            )

            $script:ServiceCalls += [pscustomobject]@{
                file_name = $FileName
                arguments = $Arguments
            }

            [ordered]@{
                exit_code = 5
                stdout = ''
                stderr = 'Access is denied.'
            }
        }

        $config = New-PcvDesktopServiceConfig `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1'

        $result = Invoke-PcvDesktopServiceCommand `
            -Config $config `
            -Action Install `
            -InvokeProcess $runner

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_SERVICE_COMMAND_FAILED'
        $script:ServiceCalls.Count | Should -Be 1
    }

    It 'resolves the default pwsh path through the service entrypoint config action' {
        $output = & pwsh -NoProfile -File $script:Entrypoint `
            -Action Config `
            -ApiScriptPath 'D:\repo\archive\spikes\purecvisor-desktop-node\api\Invoke-PcvDesktopApi.ps1'
        $json = $output | ConvertFrom-Json

        $json.binary_path | Should -Match 'pwsh.exe'
        $json.binary_path | Should -Not -Match '^"pwsh\.exe"'
    }

    It 'falls back to an absolute Program Files pwsh path when command lookup is unavailable' {
        $previousProgramFiles = $env:ProgramFiles
        $fakePwsh = Join-Path $TestDrive 'PowerShell\7\pwsh.exe'
        New-Item -ItemType Directory -Path (Split-Path -Parent $fakePwsh) -Force | Out-Null
        Set-Content -LiteralPath $fakePwsh -Value 'fake-pwsh' -NoNewline
        $env:ProgramFiles = $TestDrive
        Mock -CommandName Get-Command -ModuleName PcvDesktopService -MockWith { @() }

        try {
            Resolve-PcvDesktopServicePwshPath -SearchRoots @($TestDrive) | Should -Be $fakePwsh
        }
        finally {
            $env:ProgramFiles = $previousProgramFiles
        }
    }

    It 'falls back to Program Files pwsh locations when pwsh is missing from PATH' {
        $programFiles = Join-Path $TestDrive 'Program Files'
        $storeDir = Join-Path $programFiles 'WindowsApps\Microsoft.PowerShell_7.6.1.0_x64__8wekyb3d8bbwe'
        New-Item -ItemType Directory -Path $storeDir -Force | Out-Null
        $storePwsh = Join-Path $storeDir 'pwsh.exe'
        Set-Content -LiteralPath $storePwsh -Value 'fake-pwsh' -NoNewline

        $resolved = Resolve-PcvDesktopServicePwshPath `
            -CommandName 'definitely-missing-pwsh.exe' `
            -SearchRoots @($programFiles)

        $resolved | Should -Be $storePwsh
        $resolved | Should -Not -Be 'definitely-missing-pwsh.exe'
    }

    It 'exposes prepare, rotate, and revoke protected token actions through the service entrypoint' {
        $tokenPath = Join-Path $TestDrive 'entrypoint-token.dpapi.json'
        $prepare = & pwsh -NoProfile -File $script:Entrypoint `
            -Action PrepareProtectedTokenFile `
            -ApiTokenProtectedFile $tokenPath `
            -TokenValue 'entrypoint-secret' `
            -WhatIf
        $prepared = $prepare | ConvertFrom-Json

        $rotate = & pwsh -NoProfile -File $script:Entrypoint `
            -Action RotateProtectedTokenFile `
            -ApiTokenProtectedFile $tokenPath `
            -TokenValue 'rotated-secret' `
            -WhatIf
        $rotated = $rotate | ConvertFrom-Json

        $revoke = & pwsh -NoProfile -File $script:Entrypoint `
            -Action RevokeProtectedTokenFile `
            -ApiTokenProtectedFile $tokenPath `
            -WhatIf
        $revoked = $revoke | ConvertFrom-Json

        $prepared.ok | Should -BeTrue
        $prepared.action | Should -Be 'prepareprotectedtokenfile'
        $prepared.storage | Should -Be 'dpapi-local-machine'
        @($prepared.commands).Count | Should -Be 2
        $rotated.ok | Should -BeTrue
        $rotated.action | Should -Be 'rotateprotectedtokenfile'
        $rotated.storage | Should -Be 'dpapi-local-machine'
        @($rotated.commands).Count | Should -Be 2
        $revoked.ok | Should -BeTrue
        $revoked.action | Should -Be 'revokeprotectedtokenfile'
        $revoked.storage | Should -Be 'dpapi-local-machine'
        Test-Path -LiteralPath $tokenPath | Should -BeFalse
    }
}
