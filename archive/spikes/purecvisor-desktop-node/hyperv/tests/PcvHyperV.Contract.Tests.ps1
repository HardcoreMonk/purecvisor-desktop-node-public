Describe 'Invoke-PcvHyperV contract' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'
    }

    It 'returns PCV_INPUT_MISSING when no input is provided' {
        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'unknown'
        $json.error.code | Should -Be 'PCV_INPUT_MISSING'
        $json.error.retryable | Should -BeFalse
    }

    It 'rejects operations outside the allowlist' {
        $payload = @{ operation = 'shell.exec'; params = @{ command = 'Get-Process' } } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'shell.exec'
        $json.error.code | Should -Be 'PCV_OPERATION_NOT_ALLOWED'
        $json.error.retryable | Should -BeFalse
    }

    It 'returns a successful host.status response shape' {
        $payload = @{ operation = 'host.status'; params = @{} } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'host.status'
        $json.error | Should -BeNullOrEmpty
        $json.data.hyperv | Should -Not -BeNullOrEmpty
        $json.data.admin | Should -Not -BeNullOrEmpty
    }

    It 'accepts an input file path' {
        $inputPath = Join-Path $TestDrive 'host-status.json'
        @{ operation = 'host.status'; params = @{} } | ConvertTo-Json -Depth 8 | Set-Content -Path $inputPath -Encoding UTF8

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner -InputPath $inputPath | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'host.status'
    }
}

Describe 'Hyper-V integration test safety' {
    It 'does not gate cleanup on successful vm.create only' {
        $integrationPath = Join-Path $PSScriptRoot 'PcvHyperV.Integration.Tests.ps1'
        $source = Get-Content -LiteralPath $integrationPath -Raw

        $source | Should -Match '\$script:VmNameReserved'
        $source | Should -Not -Match '\$script:VmCreated'
    }
}
