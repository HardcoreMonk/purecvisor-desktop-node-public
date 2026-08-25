Describe 'Hyper-V network inventory' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $script:ModulePath = Join-Path $Root 'PcvHyperV.psm1'
        $script:Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'
        $script:CreatedPlaceholders = @()
        foreach ($commandName in @('Get-VMSwitch')) {
            if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
                Set-Item -Path "Function:global:$commandName" -Value { }
                $script:CreatedPlaceholders += $commandName
            }
        }

        $script:OriginalPSModulePath = $env:PSModulePath
        $script:StubModuleRoot = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-hyperv-network-test-$([guid]::NewGuid())"
        $stubModuleDir = Join-Path $script:StubModuleRoot 'Hyper-V'
        New-Item -ItemType Directory -Path $stubModuleDir -Force | Out-Null
        @'
function Get-VMSwitch {
    @(
        [pscustomobject]@{
            Name = 'Default Switch'
            SwitchType = 'Internal'
            AllowManagementOS = $true
            NetAdapterInterfaceDescription = $null
        },
        [pscustomobject]@{
            Name = 'lab-external'
            SwitchType = 'External'
            AllowManagementOS = $true
            NetAdapterInterfaceDescription = 'Intel(R) Ethernet'
        }
    )
}

Export-ModuleMember -Function Get-VMSwitch
'@ | Set-Content -Path (Join-Path $stubModuleDir 'Hyper-V.psm1') -Encoding UTF8
        $env:PSModulePath = "$script:StubModuleRoot$([System.IO.Path]::PathSeparator)$env:PSModulePath"
    }

    AfterAll {
        foreach ($commandName in $script:CreatedPlaceholders) {
            Remove-Item -Path "Function:global:$commandName" -ErrorAction SilentlyContinue
        }

        $env:PSModulePath = $script:OriginalPSModulePath
        Remove-Item -LiteralPath $script:StubModuleRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    BeforeEach {
        Import-Module $script:ModulePath -Force

        Mock Get-VMSwitch {
            @(
                [pscustomobject]@{
                    Name = 'Default Switch'
                    SwitchType = 'Internal'
                    AllowManagementOS = $true
                    NetAdapterInterfaceDescription = $null
                },
                [pscustomobject]@{
                    Name = 'lab-external'
                    SwitchType = 'External'
                    AllowManagementOS = $true
                    NetAdapterInterfaceDescription = 'Intel(R) Ethernet'
                }
            )
        } -ModuleName PcvHyperV
    }

    It 'maps Hyper-V switches into a read-only network inventory contract' {
        $inventory = Get-PcvNetworkInventory

        $inventory.source | Should -Be 'hyperv'
        $inventory.mutating | Should -BeFalse
        $inventory.switches.Count | Should -Be 2
        $inventory.switches[0].name | Should -Be 'Default Switch'
        $inventory.switches[0].type | Should -Be 'internal'
        $inventory.switches[0].is_default | Should -BeTrue
        $inventory.switches[0].allow_management_os | Should -BeTrue
        $inventory.switches[1].name | Should -Be 'lab-external'
        $inventory.switches[1].type | Should -Be 'external'
        $inventory.switches[1].net_adapter_interface_description | Should -Be 'Intel(R) Ethernet'
    }

    It 'returns structured network.inventory failure when switch inventory fails' {
        Mock Get-VMSwitch { throw 'Get-VMSwitch unavailable' } -ModuleName PcvHyperV

        $result = Invoke-PcvOperation -Request ([pscustomobject]@{
            operation = 'network.inventory'
            params = @{}
        })

        $result.ok | Should -BeFalse
        $result.operation | Should -Be 'network.inventory'
        $result.error.code | Should -Be 'PCV_NETWORK_INVENTORY_FAILED'
        $result.error.retryable | Should -BeTrue
    }

    It 'dispatches network.inventory through the runner' {
        $payload = @{ operation = 'network.inventory'; params = @{} } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:Runner | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'network.inventory'
        $json.data.source | Should -Be 'hyperv'
        $json.data.mutating | Should -BeFalse
        $json.data.switches[0].name | Should -Be 'Default Switch'
        $json.data.switches[1].type | Should -Be 'external'
    }
}
