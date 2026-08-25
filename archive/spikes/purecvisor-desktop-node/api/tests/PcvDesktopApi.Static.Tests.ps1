Describe 'PcvDesktopApi static Web Console serving' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:HelperCalls = @()
        $script:WebRootPath = Join-Path $TestDrive 'www'
        $script:UiPath = Join-Path $script:WebRootPath 'ui'
        New-Item -ItemType Directory -Path $script:UiPath -Force | Out-Null

        Set-Content `
            -LiteralPath (Join-Path $script:WebRootPath 'index.html') `
            -Value '<!doctype html><title>PureCVisor Desktop Node</title>' `
            -Encoding UTF8 `
            -NoNewline
        Set-Content `
            -LiteralPath (Join-Path $script:UiPath 'index.html') `
            -Value '<!doctype html><title>PureCVisor UI</title>' `
            -Encoding UTF8 `
            -NoNewline
        Set-Content `
            -LiteralPath (Join-Path $script:UiPath 'app.js') `
            -Value 'window.PCV_DESKTOP = true;' `
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

            [ordered]@{
                ok = $true
                operation = $Operation
                data = [ordered]@{ marker = $Operation }
                error = $null
            }
        }
    }

    It 'serves index.html for GET / when WebRootPath is supplied' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath

        $response.status | Should -Be 200
        $response.headers['Content-Type'] | Should -Be 'text/html; charset=utf-8'
        $response.body | Should -Be '<!doctype html><title>PureCVisor Desktop Node</title>'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'serves nested JavaScript assets while ignoring query strings' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/ui/app.js?cache=1' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath

        $response.status | Should -Be 200
        $response.headers['Content-Type'] | Should -Be 'application/javascript; charset=utf-8'
        $response.body | Should -Be 'window.PCV_DESKTOP = true;'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'serves index.html inside requested directories' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/ui/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath

        $response.status | Should -Be 200
        $response.headers['Content-Type'] | Should -Be 'text/html; charset=utf-8'
        $response.body | Should -Be '<!doctype html><title>PureCVisor UI</title>'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'returns a structured 404 for missing static files' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/missing.css' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath

        $response.status | Should -Be 404
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'static.file'
        $json.error.code | Should -Be 'PCV_STATIC_FILE_NOT_FOUND'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'blocks path traversal attempts before reading files' {
        Set-Content `
            -LiteralPath (Join-Path $script:WebRootPath 'secret.txt') `
            -Value 'secret' `
            -Encoding UTF8 `
            -NoNewline

        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/ui/%2e%2e/secret.txt' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath

        $response.status | Should -Be 403
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'static.file'
        $json.error.code | Should -Be 'PCV_STATIC_PATH_FORBIDDEN'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'keeps API routes ahead of static file serving' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $json.data.marker | Should -Be 'vm.list'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.list'
    }

    It 'serves static assets without bearer token when loopback static bypass is enabled' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'required-api-token' `
            -AllowUnauthenticatedStatic

        $response.status | Should -Be 200
        $response.body | Should -Be '<!doctype html><title>PureCVisor Desktop Node</title>'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'still requires bearer token for API routes when loopback static bypass is enabled' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/runtime/policy' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'required-api-token' `
            -AllowUnauthenticatedStatic

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
    }

    It 'requires bearer token for static assets when static bypass is disabled' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -WebRootPath $script:WebRootPath `
            -ApiToken 'required-api-token'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
    }
}
