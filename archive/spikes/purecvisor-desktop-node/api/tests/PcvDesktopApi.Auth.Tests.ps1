Describe 'PcvDesktopApi optional API token gate' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
        Import-Module (Join-Path (Split-Path -Parent $Root) 'service\PcvDesktopService.psm1') -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:HelperCalls = @()
        $script:WebRootPath = Join-Path $TestDrive 'www'
        New-Item -ItemType Directory -Path $script:WebRootPath -Force | Out-Null
        Set-Content `
            -LiteralPath (Join-Path $script:WebRootPath 'index.html') `
            -Value '<!doctype html><title>PureCVisor Desktop Node</title>' `
            -Encoding UTF8 `
            -NoNewline

        $script:Helper = {
            param(
                [string]$Operation,
                [AllowNull()]$Params,
                [string]$HelperScriptPath,
                [int]$TimeoutSec
            )

            $script:HelperCalls += [pscustomobject]@{
                operation = $Operation
                params = $Params
                helper_script_path = $HelperScriptPath
                timeout_sec = $TimeoutSec
            }

            $data = [ordered]@{ marker = $Operation }
            if ($Operation -eq 'vm.list') {
                $data = @(
                    [ordered]@{
                        id = 'ubuntu-lab-01'
                        name = 'ubuntu-lab-01'
                        state = 'running'
                    }
                )
            }

            [ordered]@{
                ok = $true
                operation = $Operation
                data = $data
                error = $null
            }
        }
    }

    It 'returns 401 when ApiToken is configured and Authorization is missing' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'api.auth'
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'returns 401 when Authorization is not a bearer token' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Basic abc123' }

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'returns 403 when bearer token is wrong' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Bearer wrong' }

        $response.status | Should -Be 403
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'api.auth'
        $json.error.code | Should -Be 'PCV_AUTH_FORBIDDEN'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'allows API helper routes when bearer token matches' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Bearer secret' }

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $json.data[0].name | Should -Be 'ubuntu-lab-01'
        $script:HelperCalls.Count | Should -Be 1
    }

    It 'allows static file serving when bearer token matches' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Bearer secret' }

        $response.status | Should -Be 200
        $response.headers['Content-Type'] | Should -Be 'text/html; charset=utf-8'
        $response.body | Should -Be '<!doctype html><title>PureCVisor Desktop Node</title>'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'keeps existing localhost behavior when ApiToken is omitted' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $script:HelperCalls.Count | Should -Be 1
    }

    It 'requires bearer token for VM detail routes when ApiToken is configured' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/ubuntu-lab-01' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'api.auth'
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'allows VM detail routes when bearer token matches' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/ubuntu-lab-01' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Bearer secret' }

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.get'
        $json.data.name | Should -Be 'ubuntu-lab-01'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.list'
    }

    It 'requires bearer token for lifecycle job routes when ApiToken is configured' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'api.auth'
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'allows lifecycle job routes when bearer token matches' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Bearer secret' }

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'job.create'
        $json.data.operation | Should -Be 'vm.start'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'reads an API token from a token file for bearer auth' {
        $tokenPath = Join-Path $TestDrive 'api-token.txt'
        Set-Content -LiteralPath $tokenPath -Value "file-secret`r`n" -Encoding UTF8 -NoNewline

        $resolved = Resolve-PcvApiToken -ApiTokenFile $tokenPath
        $resolved.source | Should -Be 'file'
        $resolved.value | Should -Be 'file-secret'
        $resolved.path | Should -Be $tokenPath

        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken $resolved.value `
            -Headers @{ Authorization = 'Bearer file-secret' }

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $script:HelperCalls.Count | Should -Be 1
    }

    It 'reads an API token from a protected DPAPI token file for bearer auth' {
        $tokenPath = Join-Path $TestDrive 'api-token.dpapi.json'
        $runner = {
            [ordered]@{
                exit_code = 0
                stdout = 'processed'
                stderr = ''
            }
        }
        New-PcvDesktopServiceProtectedTokenFile `
            -Path $tokenPath `
            -Token 'protected-file-secret' `
            -InvokeProcess $runner | Out-Null

        $resolved = Resolve-PcvApiToken -ApiTokenProtectedFile $tokenPath
        $resolved.source | Should -Be 'protected_file'
        $resolved.storage | Should -Be 'dpapi-local-machine'
        $resolved.value | Should -Be 'protected-file-secret'
        $resolved.path | Should -Be $tokenPath

        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken $resolved.value `
            -Headers @{ Authorization = 'Bearer protected-file-secret' }

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $script:HelperCalls.Count | Should -Be 1
    }

    It 'rejects ambiguous API token sources' {
        $tokenPath = Join-Path $TestDrive 'api-token.txt'
        Set-Content -LiteralPath $tokenPath -Value 'file-secret' -Encoding UTF8 -NoNewline

        { Resolve-PcvApiToken -ApiToken 'inline-secret' -ApiTokenFile $tokenPath } |
            Should -Throw -ExpectedMessage '*PCV_API_TOKEN_CONFLICT*'
    }

    It 'rejects ambiguous protected API token sources' {
        $tokenPath = Join-Path $TestDrive 'api-token.txt'
        $protectedPath = Join-Path $TestDrive 'api-token.dpapi.json'
        Set-Content -LiteralPath $tokenPath -Value 'file-secret' -Encoding UTF8 -NoNewline

        { Resolve-PcvApiToken -ApiTokenFile $tokenPath -ApiTokenProtectedFile $protectedPath } |
            Should -Throw -ExpectedMessage '*PCV_API_TOKEN_CONFLICT*'
    }

    It 'rejects empty API token files' {
        $tokenPath = Join-Path $TestDrive 'api-token.txt'
        Set-Content -LiteralPath $tokenPath -Value " `r`n" -Encoding UTF8 -NoNewline

        { Resolve-PcvApiToken -ApiTokenFile $tokenPath } |
            Should -Throw -ExpectedMessage '*PCV_API_TOKEN_FILE_EMPTY*'
    }

    It 'reports protected token storage in the runtime policy without exposing token material' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/runtime/policy' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'runtime-secret' `
            -ApiTokenStorage 'dpapi-local-machine' `
            -Headers @{ Authorization = 'Bearer runtime-secret' }

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.data.auth.token_storage | Should -Be 'dpapi-local-machine'
        ($json | ConvertTo-Json -Depth 20) | Should -Not -Match 'runtime-secret'
    }
}
