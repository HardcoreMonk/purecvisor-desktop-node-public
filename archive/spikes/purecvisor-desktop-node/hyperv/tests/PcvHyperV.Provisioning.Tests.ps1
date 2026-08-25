Describe 'New-PcvVmFromIso' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $script:ModulePath = Join-Path $Root 'PcvHyperV.psm1'
        $script:OriginalFunctions = @{}
        $script:PlaceholderCommandNames = @()
        $placeholders = @{
            'Get-VM' = { param([string]$Name, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'New-VHD' = { param([string]$Path, [Int64]$SizeBytes, [switch]$Dynamic, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'New-VM' = { param([string]$Name, [int]$Generation, [Int64]$MemoryStartupBytes, [string]$VHDPath, [string]$Path, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Set-VM' = { param([string]$Name, [string]$Notes, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Set-VMProcessor' = { param([string]$VMName, [int]$Count, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Add-VMDvdDrive' = { param([string]$VMName, [string]$Path, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Set-VMDvdDrive' = { param([string]$VMName, [string]$Path, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Get-VMDvdDrive' = { param([string]$VMName, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Connect-VMNetworkAdapter' = { param([string]$VMName, [string]$SwitchName, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Set-VMFirmware' = { param([string]$VMName, $FirstBootDevice, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Stop-VM' = { param([string]$Name, [switch]$TurnOff, [switch]$Force, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Remove-VM' = { param([string]$Name, [switch]$Force, [System.Management.Automation.ActionPreference]$ErrorAction) }
        }
        foreach ($commandName in $placeholders.Keys) {
            $existingFunction = Get-Item -Path "Function:global:$commandName" -ErrorAction SilentlyContinue
            if ($existingFunction) {
                $script:OriginalFunctions[$commandName] = $existingFunction.ScriptBlock
            }
            Set-Item -Path "Function:global:$commandName" -Value $placeholders[$commandName]
            $script:PlaceholderCommandNames += $commandName
        }
    }

    AfterAll {
        foreach ($commandName in $script:PlaceholderCommandNames) {
            if ($script:OriginalFunctions.ContainsKey($commandName)) {
                Set-Item -Path "Function:global:$commandName" -Value $script:OriginalFunctions[$commandName]
            }
            else {
                Remove-Item -Path "Function:global:$commandName" -ErrorAction SilentlyContinue
            }
        }
    }

    BeforeEach {
        Import-Module $script:ModulePath -Force
        Mock Get-PcvHostStatus {
            [ordered]@{
                supported = $true
                reasons = @()
                hyperv = [ordered]@{
                    feature_enabled = $true
                    vmms_running = $true
                    default_switch_present = $true
                }
                admin = [ordered]@{ elevated = $true }
                windows = [ordered]@{ edition = 'Pro'; caption = 'Windows 11 Pro'; version = '23H2' }
            }
        } -ModuleName PcvHyperV
        Mock Test-Path {
            param([string]$LiteralPath, [string]$PathType)
            return ($LiteralPath -like '*.iso')
        } -ModuleName PcvHyperV
        Mock Get-VM { throw 'not found' } -ModuleName PcvHyperV
        Mock New-Item {
            [pscustomobject]@{ FullName = $Path }
        } -ModuleName PcvHyperV
        Mock New-VHD {
            [pscustomobject]@{ Path = $Path; SizeBytes = $SizeBytes }
        } -ModuleName PcvHyperV
        Mock New-VM {
            [pscustomobject]@{ Name = $Name }
        } -ModuleName PcvHyperV
        Mock Set-VM {} -ModuleName PcvHyperV
        Mock Set-VMProcessor {} -ModuleName PcvHyperV
        Mock Add-VMDvdDrive {} -ModuleName PcvHyperV
        Mock Set-VMDvdDrive {} -ModuleName PcvHyperV
        Mock Get-VMDvdDrive {
            [pscustomobject]@{ VMName = $VMName; Path = 'D:\iso\ubuntu.iso' }
        } -ModuleName PcvHyperV
        Mock Connect-VMNetworkAdapter {} -ModuleName PcvHyperV
        Mock Set-VMFirmware {} -ModuleName PcvHyperV
        Mock Stop-VM {} -ModuleName PcvHyperV
        Mock Remove-VM {} -ModuleName PcvHyperV
        Mock Remove-Item {} -ModuleName PcvHyperV
    }

    It 'rejects invalid VM names before calling Hyper-V cmdlets' {
        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'bad name!'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_NAME_INVALID'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'returns structured failure for missing create params' {
        $result = Invoke-PcvOperation -Request ([pscustomobject]@{
            operation = 'vm.create'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CREATE_PARAMS_INVALID'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'returns structured failure for malformed numeric create params' {
        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 'two'
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CREATE_PARAMS_INVALID'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'returns structured failure when VM lookup fails unexpectedly' {
        Mock Get-VM { throw 'Hyper-V service unavailable' } -ModuleName PcvHyperV

        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_LOOKUP_FAILED'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'treats ObjectNotFound VM lookup errors as missing and creates the VM' {
        Mock Get-VM {
            $exception = [System.Management.Automation.ItemNotFoundException]::new('Hyper-V was unable to find a virtual machine named ubuntu-lab-01.')
            $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                $exception,
                'VirtualMachineNotFound',
                [System.Management.Automation.ErrorCategory]::ObjectNotFound,
                'ubuntu-lab-01'
            )
            throw $errorRecord
        } -ModuleName PcvHyperV

        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeTrue -Because ($result | ConvertTo-Json -Depth 8 -Compress)
        Should -Invoke New-VM -Times 1 -ModuleName PcvHyperV
    }

    It 'treats localized Hyper-V missing VM lookup errors as missing' {
        Mock Get-VM {
            $exception = [System.InvalidOperationException]::new('Hyper-V가 이름이 "ubuntu-lab-01"인 가상 컴퓨터를 찾을 수 없습니다.')
            $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                $exception,
                'InvalidParameter,Microsoft.HyperV.PowerShell.Commands.GetVM',
                [System.Management.Automation.ErrorCategory]::InvalidArgument,
                'ubuntu-lab-01'
            )
            throw $errorRecord
        } -ModuleName PcvHyperV

        $result = Test-PcvVmExists -Name 'ubuntu-lab-01'

        $result.exists | Should -BeFalse
        $result.error | Should -BeNullOrEmpty
    }

    It 'returns structured failure for unexpected VM lookup error categories' {
        Mock Get-VM {
            $exception = [System.InvalidOperationException]::new('Hyper-V service unavailable')
            $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                $exception,
                'VmmsUnavailable',
                [System.Management.Automation.ErrorCategory]::ResourceUnavailable,
                'ubuntu-lab-01'
            )
            throw $errorRecord
        } -ModuleName PcvHyperV

        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_LOOKUP_FAILED'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'creates a Generation 2 VM from an ISO using Default Switch' {
        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeTrue -Because ($result | ConvertTo-Json -Depth 8 -Compress)
        $result.data.name | Should -Be 'ubuntu-lab-01'
        $result.data.steps | Should -Contain 'Create VHDX'
        $result.data.steps | Should -Contain 'Create Hyper-V VM'
        $result.data.steps | Should -Contain 'Attach Default Switch'
        Should -Invoke New-VHD -Times 1 -ModuleName PcvHyperV
        Should -Invoke New-VM -Times 1 -ModuleName PcvHyperV
        Should -Invoke Add-VMDvdDrive -Times 1 -ModuleName PcvHyperV
        Should -Invoke Connect-VMNetworkAdapter -Times 1 -ModuleName PcvHyperV
    }

    It 'returns structured failure when VHD creation emits a non-terminating error' {
        Mock New-VHD {
            [CmdletBinding()]
            param([System.Management.Automation.ActionPreference]$ErrorAction)
            Write-Error 'VHD create failed'
        } -ModuleName PcvHyperV

        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_CREATE_FAILED'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
        Should -Invoke Remove-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'cleans up VM and disk artifacts after mid-provisioning failure' {
        Mock Set-VMProcessor {
            [CmdletBinding()]
            param([System.Management.Automation.ActionPreference]$ErrorAction)
            Write-Error 'CPU update failed'
        } -ModuleName PcvHyperV

        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_CREATE_FAILED'
        Should -Invoke Stop-VM -Times 1 -ModuleName PcvHyperV
        Should -Invoke Remove-VM -Times 1 -ModuleName PcvHyperV
        Should -Invoke Remove-Item -Times 1 -ModuleName PcvHyperV
    }
}
