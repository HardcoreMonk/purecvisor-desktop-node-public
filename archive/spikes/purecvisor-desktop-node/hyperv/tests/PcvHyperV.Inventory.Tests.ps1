Describe 'VM inventory' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $script:ModulePath = Join-Path $Root 'PcvHyperV.psm1'
        $script:Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'
        $script:CreatedPlaceholders = @()
        foreach ($commandName in @('Get-VM', 'Get-VMHardDiskDrive', 'Get-VHD', 'Get-VMNetworkAdapter', 'Get-VMSnapshot')) {
            if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
                Set-Item -Path "Function:global:$commandName" -Value { }
                $script:CreatedPlaceholders += $commandName
            }
        }

        $script:OriginalPSModulePath = $env:PSModulePath
        $script:StubModuleRoot = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-hyperv-test-$([guid]::NewGuid())"
        $stubModuleDir = Join-Path $script:StubModuleRoot 'Hyper-V'
        New-Item -ItemType Directory -Path $stubModuleDir -Force | Out-Null
        @'
function Get-VM {
    @(
        [pscustomobject]@{
            Name = 'ubuntu-lab-01'
            State = 'Running'
            ProcessorCount = 2
            MemoryStartup = 4294967296
            MemoryAssigned = 2147483648
            Generation = 2
            Uptime = [timespan]::FromMinutes(12)
            Notes = 'managed-by=purecvisor-desktop-node'
        }
    )
}

function Get-VMHardDiskDrive {
    @(
        [pscustomobject]@{ Path = 'D:\PureCVisor\VMs\ubuntu-lab-01\disk0.vhdx' }
    )
}

function Get-VHD {
    [pscustomobject]@{ Size = 42949672960 }
}

function Get-VMNetworkAdapter {
    @(
        [pscustomobject]@{ SwitchName = 'Default Switch' }
    )
}

function Get-VMSnapshot {
    @(
        [pscustomobject]@{ Name = 'before-upgrade' }
    )
}

Export-ModuleMember -Function Get-VM, Get-VMHardDiskDrive, Get-VHD, Get-VMNetworkAdapter, Get-VMSnapshot
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

        Mock Get-VM {
            @(
                [pscustomobject]@{
                    Name = 'ubuntu-lab-01'
                    State = 'Running'
                    ProcessorCount = 2
                    MemoryStartup = 4294967296
                    MemoryAssigned = 2147483648
                    Generation = 2
                    Uptime = [timespan]::FromMinutes(12)
                    Notes = 'managed-by=purecvisor-desktop-node'
                }
            )
        } -ModuleName PcvHyperV
        Mock Get-VMHardDiskDrive {
            @(
                [pscustomobject]@{ Path = 'D:\PureCVisor\VMs\ubuntu-lab-01\disk0.vhdx' }
            )
        } -ModuleName PcvHyperV
        Mock Get-VHD {
            [pscustomobject]@{ Size = 42949672960 }
        } -ModuleName PcvHyperV
        Mock Get-VMNetworkAdapter {
            @(
                [pscustomobject]@{ SwitchName = 'Default Switch' }
            )
        } -ModuleName PcvHyperV
        Mock Get-VMSnapshot {
            @(
                [pscustomobject]@{ Name = 'before-upgrade' }
            )
        } -ModuleName PcvHyperV
    }

    It 'maps Hyper-V VMs into the PureCVisor VM model' {
        $list = Get-PcvVmList

        $list.Count | Should -Be 1
        $vm = $list[0]
        $vm.id | Should -Be 'ubuntu-lab-01'
        $vm.name | Should -Be 'ubuntu-lab-01'
        $vm.platform | Should -Be 'hyperv'
        $vm.guest_family | Should -Be 'linux'
        $vm.state | Should -Be 'running'
        $vm.cpu.count | Should -Be 2
        $vm.memory.startup_mb | Should -Be 4096
        $vm.memory.assigned_mb | Should -Be 2048
        $vm.generation | Should -Be 2
        $vm.storage[0].size_gb | Should -Be 40
        $vm.storage[0].attached | Should -BeTrue
        $vm.network[0].switch | Should -Be 'Default Switch'
        $vm.checkpoints.count | Should -Be 1
        $vm.managed_by_purecvisor | Should -BeTrue
    }

    It 'keeps the VM when disk inventory throws' {
        Mock Get-VMHardDiskDrive { throw 'Disk inventory unavailable' } -ModuleName PcvHyperV

        $list = Get-PcvVmList

        $list.Count | Should -Be 1
        $vm = $list[0]
        $vm.name | Should -Be 'ubuntu-lab-01'
        $vm.storage.Count | Should -Be 0
    }

    It 'returns structured vm.list failure when Hyper-V inventory fails' {
        Mock Get-VM { throw 'Get-VM unavailable' } -ModuleName PcvHyperV

        $result = Invoke-PcvOperation -Request ([pscustomobject]@{
            operation = 'vm.list'
            params = @{}
        })

        $result.ok | Should -BeFalse
        $result.operation | Should -Be 'vm.list'
        $result.error.code | Should -Be 'PCV_VM_LIST_FAILED'
        $result.error.retryable | Should -BeTrue
    }

    It 'dispatches vm.list through the runner' {
        $payload = @{ operation = 'vm.list'; params = @{} } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:Runner | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $vms = @($json.data)
        $vms.Count | Should -Be 1
        $vms[0].name | Should -Be 'ubuntu-lab-01'
        $vms[0].platform | Should -Be 'hyperv'
        $vms[0].state | Should -Be 'running'
        $vms[0].memory.assigned_mb | Should -Be 2048
        $vms[0].generation | Should -Be 2
        $vms[0].storage[0].attached | Should -BeTrue
    }
}
