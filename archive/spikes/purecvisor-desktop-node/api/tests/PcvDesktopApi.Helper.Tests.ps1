Describe 'PcvDesktopApi helper process invocation' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    It 'sends operation params through stdin and parses compact JSON stdout' {
        $helperPath = Join-Path $TestDrive 'echo-helper.ps1'
        @'
$raw = [Console]::In.ReadToEnd()
$request = $raw | ConvertFrom-Json -Depth 20
[ordered]@{
    ok = $true
    operation = $request.operation
    data = [ordered]@{
        echoed_name = $request.params.name
    }
    error = $null
} | ConvertTo-Json -Depth 20 -Compress
exit 0
'@ | Set-Content -LiteralPath $helperPath -Encoding UTF8

        $result = Invoke-PcvHyperVHelper `
            -Operation 'host.status' `
            -Params @{ name = 'alpha' } `
            -HelperScriptPath $helperPath `
            -TimeoutSec 5

        $result.ok | Should -BeTrue
        $result.operation | Should -Be 'host.status'
        $result.data.echoed_name | Should -Be 'alpha'
        $result.error | Should -BeNullOrEmpty
    }

    It 'returns PCV_HELPER_EXIT_FAILED when the helper exits non-zero' {
        $helperPath = Join-Path $TestDrive 'failing-helper.ps1'
        @'
[Console]::Error.WriteLine('simulated helper failure')
exit 17
'@ | Set-Content -LiteralPath $helperPath -Encoding UTF8

        $result = Invoke-PcvHyperVHelper `
            -Operation 'vm.list' `
            -Params @{} `
            -HelperScriptPath $helperPath `
            -TimeoutSec 5

        $result.ok | Should -BeFalse
        $result.operation | Should -Be 'vm.list'
        $result.error.code | Should -Be 'PCV_HELPER_EXIT_FAILED'
        $result.error.detail | Should -Match 'exit code 17'
        $result.error.detail | Should -Match 'simulated helper failure'
    }

    It 'returns PCV_HELPER_INVALID_JSON when helper stdout is not JSON' {
        $helperPath = Join-Path $TestDrive 'bad-json-helper.ps1'
        @'
Write-Output 'not-json'
exit 0
'@ | Set-Content -LiteralPath $helperPath -Encoding UTF8

        $result = Invoke-PcvHyperVHelper `
            -Operation 'host.status' `
            -Params @{} `
            -HelperScriptPath $helperPath `
            -TimeoutSec 5

        $result.ok | Should -BeFalse
        $result.operation | Should -Be 'host.status'
        $result.error.code | Should -Be 'PCV_HELPER_INVALID_JSON'
        $result.error.retryable | Should -BeFalse
    }
}
