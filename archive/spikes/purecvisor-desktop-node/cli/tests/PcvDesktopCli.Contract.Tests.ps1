Describe 'PcvDesktopCli contract' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopCli.psm1'
        Import-Module $ModulePath -Force
        Import-Module (Join-Path (Split-Path -Parent $Root) 'service\PcvDesktopService.psm1') -Force
    }

    BeforeEach {
        $script:Requests = @()
        $script:Transport = {
            param(
                [string]$Method,
                [string]$Path,
                [AllowNull()]$Body,
                [string]$ApiBaseUrl,
                [string]$ApiToken
            )

            $script:Requests += [pscustomobject]@{
                method = $Method
                path = $Path
                body = $Body
                api_base_url = $ApiBaseUrl
                api_token = $ApiToken
            }

            [ordered]@{
                ok = $true
                operation = 'test.transport'
                data = [ordered]@{
                    method = $Method
                    path = $Path
                    body = $Body
                }
                error = $null
            }
        }
    }

    It 'routes host status to GET /api/v1/host/status' {
        $result = Invoke-PcvDesktopCli -Arguments @('host', 'status', '--json') -Transport $script:Transport

        $result.exit_code | Should -Be 0
        $script:Requests.Count | Should -Be 1
        $script:Requests[0].method | Should -Be 'GET'
        $script:Requests[0].path | Should -Be '/api/v1/host/status'
        ($result.stdout | ConvertFrom-Json).ok | Should -BeTrue
    }

    It 'routes runtime policy to GET /api/v1/runtime/policy' {
        $result = Invoke-PcvDesktopCli -Arguments @('runtime', 'policy', '--json') -Transport $script:Transport

        $result.exit_code | Should -Be 0
        $script:Requests.Count | Should -Be 1
        $script:Requests[0].method | Should -Be 'GET'
        $script:Requests[0].path | Should -Be '/api/v1/runtime/policy'
        ($result.stdout | ConvertFrom-Json).ok | Should -BeTrue
    }

    It 'routes VM list and detail commands' {
        Invoke-PcvDesktopCli -Arguments @('vm', 'list', '--json') -Transport $script:Transport | Out-Null
        Invoke-PcvDesktopCli -Arguments @('vm', 'get', 'ubuntu-lab-01', '--json') -Transport $script:Transport | Out-Null

        $script:Requests[0].method | Should -Be 'GET'
        $script:Requests[0].path | Should -Be '/api/v1/vms'
        $script:Requests[1].method | Should -Be 'GET'
        $script:Requests[1].path | Should -Be '/api/v1/vms/ubuntu-lab-01'
    }

    It 'builds VM create JSON bodies from command options' {
        Invoke-PcvDesktopCli -Arguments @(
            'vm', 'create',
            '--name', 'ubuntu-lab-01',
            '--iso', 'D:\isos\ubuntu.iso',
            '--cpu', '2',
            '--memory-mb', '4096',
            '--disk-gb', '40',
            '--vm-root', 'D:\PureCVisor\VMs',
            '--generation', '2',
            '--json'
        ) -Transport $script:Transport | Out-Null

        $script:Requests.Count | Should -Be 1
        $script:Requests[0].method | Should -Be 'POST'
        $script:Requests[0].path | Should -Be '/api/v1/vms'
        $body = $script:Requests[0].body | ConvertFrom-Json
        $body.name | Should -Be 'ubuntu-lab-01'
        $body.iso_path | Should -Be 'D:\isos\ubuntu.iso'
        $body.cpu | Should -Be 2
        $body.memory_mb | Should -Be 4096
        $body.disk_gb | Should -Be 40
        $body.vm_root | Should -Be 'D:\PureCVisor\VMs'
        $body.generation | Should -Be 2
    }

    It 'routes lifecycle commands to queued job endpoints' {
        $cases = @(
            @{ args = @('vm', 'start', 'ubuntu-lab-01', '--json'); path = '/api/v1/vms/ubuntu-lab-01/start' },
            @{ args = @('vm', 'stop', 'ubuntu-lab-01', '--json'); path = '/api/v1/vms/ubuntu-lab-01/shutdown' },
            @{ args = @('vm', 'poweroff', 'ubuntu-lab-01', '--json'); path = '/api/v1/vms/ubuntu-lab-01/poweroff' }
        )

        foreach ($case in $cases) {
            Invoke-PcvDesktopCli -Arguments $case.args -Transport $script:Transport | Out-Null
        }

        $script:Requests.Count | Should -Be 3
        $script:Requests[0].method | Should -Be 'POST'
        $script:Requests[0].path | Should -Be $cases[0].path
        $script:Requests[1].path | Should -Be $cases[1].path
        $script:Requests[2].path | Should -Be $cases[2].path
    }

    It 'routes checkpoint commands to Phase 4 endpoints' {
        Invoke-PcvDesktopCli -Arguments @('vm', 'checkpoint', 'list', 'ubuntu-lab-01', '--json') -Transport $script:Transport | Out-Null
        Invoke-PcvDesktopCli -Arguments @('vm', 'checkpoint', 'create', 'ubuntu-lab-01', '--name', 'before-upgrade', '--json') -Transport $script:Transport | Out-Null
        Invoke-PcvDesktopCli -Arguments @('vm', 'checkpoint', 'restore', 'ubuntu-lab-01', 'before-upgrade', '--json') -Transport $script:Transport | Out-Null
        Invoke-PcvDesktopCli -Arguments @('vm', 'checkpoint', 'delete', 'ubuntu-lab-01', 'before-upgrade', '--json') -Transport $script:Transport | Out-Null

        $script:Requests[0].method | Should -Be 'GET'
        $script:Requests[0].path | Should -Be '/api/v1/vms/ubuntu-lab-01/checkpoints'
        $script:Requests[1].method | Should -Be 'POST'
        $script:Requests[1].path | Should -Be '/api/v1/vms/ubuntu-lab-01/checkpoints'
        ($script:Requests[1].body | ConvertFrom-Json).name | Should -Be 'before-upgrade'
        $script:Requests[2].method | Should -Be 'POST'
        $script:Requests[2].path | Should -Be '/api/v1/vms/ubuntu-lab-01/checkpoints/before-upgrade/restore'
        $script:Requests[3].method | Should -Be 'DELETE'
        $script:Requests[3].path | Should -Be '/api/v1/vms/ubuntu-lab-01/checkpoints/before-upgrade'
    }

    It 'routes job commands to existing job endpoints' {
        Invoke-PcvDesktopCli -Arguments @('job', 'get', 'job-123', '--json') -Transport $script:Transport | Out-Null
        Invoke-PcvDesktopCli -Arguments @('job', 'cancel', 'job-123', '--json') -Transport $script:Transport | Out-Null
        Invoke-PcvDesktopCli -Arguments @('job', 'retry', 'job-123', '--json') -Transport $script:Transport | Out-Null

        $script:Requests[0].method | Should -Be 'GET'
        $script:Requests[0].path | Should -Be '/api/v1/jobs/job-123'
        $script:Requests[1].method | Should -Be 'POST'
        $script:Requests[1].path | Should -Be '/api/v1/jobs/job-123/cancel'
        $script:Requests[2].method | Should -Be 'POST'
        $script:Requests[2].path | Should -Be '/api/v1/jobs/job-123/retry'
    }

    It 'passes custom API base URL and bearer token to the transport' {
        Invoke-PcvDesktopCli -Arguments @(
            '--api', 'http://127.0.0.1:8888',
            '--token', 'secret',
            'host', 'status',
            '--json'
        ) -Transport $script:Transport | Out-Null

        $script:Requests[0].api_base_url | Should -Be 'http://127.0.0.1:8888'
        $script:Requests[0].api_token | Should -Be 'secret'
    }

    It 'reads a bearer token from a token file' {
        $tokenFile = New-TemporaryFile
        try {
            Set-Content -LiteralPath $tokenFile.FullName -Value "token-from-file`r`n" -Encoding UTF8

            Invoke-PcvDesktopCli -Arguments @(
                '--api', 'http://127.0.0.1:8888',
                '--token-file', $tokenFile.FullName,
                'host', 'status',
                '--json'
            ) -Transport $script:Transport | Out-Null

            $script:Requests[0].api_base_url | Should -Be 'http://127.0.0.1:8888'
            $script:Requests[0].api_token | Should -Be 'token-from-file'
        }
        finally {
            Remove-Item -LiteralPath $tokenFile.FullName -Force -ErrorAction SilentlyContinue
        }
    }

    It 'reads a bearer token from a protected token file' {
        $tokenFile = Join-Path $TestDrive 'api-token.dpapi.json'
        New-PcvDesktopServiceProtectedTokenFile `
            -Path $tokenFile `
            -Token 'token-from-protected-file' `
            -InvokeProcess {
                [ordered]@{ exit_code = 0; stdout = 'processed'; stderr = '' }
            } | Out-Null

        Invoke-PcvDesktopCli -Arguments @(
            '--api', 'http://127.0.0.1:8888',
            '--protected-token-file', $tokenFile,
            'host', 'status',
            '--json'
        ) -Transport $script:Transport | Out-Null

        $script:Requests[0].api_base_url | Should -Be 'http://127.0.0.1:8888'
        $script:Requests[0].api_token | Should -Be 'token-from-protected-file'
    }

    It 'rejects ambiguous CLI token sources' {
        $tokenFile = New-TemporaryFile
        try {
            Set-Content -LiteralPath $tokenFile.FullName -Value 'token-from-file' -Encoding UTF8 -NoNewline
            $protectedTokenFile = Join-Path $TestDrive 'conflict-token.dpapi.json'

            $result = Invoke-PcvDesktopCli -Arguments @(
                '--token', 'inline-token',
                '--token-file', $tokenFile.FullName,
                'host', 'status'
            ) -Transport $script:Transport

            $result.exit_code | Should -Be 2
            $result.stderr | Should -Match 'token'
            $result.stderr | Should -Match 'token-file'
            $script:Requests.Count | Should -Be 0

            $protectedResult = Invoke-PcvDesktopCli -Arguments @(
                '--token-file', $tokenFile.FullName,
                '--protected-token-file', $protectedTokenFile,
                'host', 'status'
            ) -Transport $script:Transport

            $protectedResult.exit_code | Should -Be 2
            $protectedResult.stderr | Should -Match 'protected-token-file'
            $script:Requests.Count | Should -Be 0
        }
        finally {
            Remove-Item -LiteralPath $tokenFile.FullName -Force -ErrorAction SilentlyContinue
        }
    }

    It 'rejects missing or empty token files' {
        $missingPath = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString() + '.txt')
        $missing = Invoke-PcvDesktopCli -Arguments @('--token-file', $missingPath, 'host', 'status') -Transport $script:Transport
        $missing.exit_code | Should -Be 2
        $missing.stderr | Should -Match 'not found'

        $emptyFile = New-TemporaryFile
        try {
            Set-Content -LiteralPath $emptyFile.FullName -Value " `r`n" -Encoding UTF8
            $empty = Invoke-PcvDesktopCli -Arguments @('--token-file', $emptyFile.FullName, 'host', 'status') -Transport $script:Transport
            $empty.exit_code | Should -Be 2
            $empty.stderr | Should -Match 'empty'
        }
        finally {
            Remove-Item -LiteralPath $emptyFile.FullName -Force -ErrorAction SilentlyContinue
        }

        $script:Requests.Count | Should -Be 0
    }

    It 'returns a non-zero exit code for incomplete commands' {
        $result = Invoke-PcvDesktopCli -Arguments @('vm', 'create', '--name', 'missing-fields') -Transport $script:Transport

        $result.exit_code | Should -Be 2
        $result.stderr | Should -Match 'Missing required option'
        $script:Requests.Count | Should -Be 0
    }
}
