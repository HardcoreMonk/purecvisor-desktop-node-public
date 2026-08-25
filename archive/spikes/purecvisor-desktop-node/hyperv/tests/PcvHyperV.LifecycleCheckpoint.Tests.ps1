Describe 'VM lifecycle and checkpoints' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $script:ModulePath = Join-Path $Root 'PcvHyperV.psm1'
        $script:OriginalFunctions = @{}
        $script:PlaceholderCommandNames = @()
        $placeholders = @{
            'Get-VM' = { param([string]$Name) }
            'Start-VM' = { param([string]$Name, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Stop-VM' = { param([string]$Name, [switch]$Shutdown, [switch]$TurnOff, [switch]$Force, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Restart-VM' = { param([string]$Name, [switch]$Force, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Checkpoint-VM' = { param([string]$Name, [string]$SnapshotName, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Get-VMSnapshot' = { param([string]$VMName) }
            'Restore-VMSnapshot' = { param([string]$VMName, [string]$Name, [switch]$Confirm, [System.Management.Automation.ActionPreference]$ErrorAction) }
            'Remove-VMSnapshot' = { param([string]$VMName, [string]$Name, [switch]$Confirm, [System.Management.Automation.ActionPreference]$ErrorAction) }
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
        Mock Get-VM {
            [pscustomobject]@{ Name = $Name; State = 'Off' }
        } -ModuleName PcvHyperV
        Mock Start-VM {} -ModuleName PcvHyperV
        Mock Stop-VM {} -ModuleName PcvHyperV
        Mock Restart-VM {} -ModuleName PcvHyperV
        Mock Checkpoint-VM {
            [pscustomobject]@{ Name = $SnapshotName; VMName = $Name }
        } -ModuleName PcvHyperV
        Mock Get-VMSnapshot {
            @(
                [pscustomobject]@{ Name = 'before-upgrade'; VMName = $VMName; CreationTime = [datetime]'2026-04-24T00:00:00Z' }
            )
        } -ModuleName PcvHyperV
        Mock Start-Sleep {} -ModuleName PcvHyperV
        Mock Restore-VMSnapshot {} -ModuleName PcvHyperV
        Mock Remove-VMSnapshot {} -ModuleName PcvHyperV
    }

    It 'uses terminating errors for mutating Hyper-V calls' {
        $moduleSource = Get-Content -Path $script:ModulePath -Raw

        $moduleSource | Should -Match 'Start-VM\s+-Name\s+\$name\s+-ErrorAction\s+Stop'
        $moduleSource | Should -Match 'Stop-VM\s+-Name\s+\$name\s+-Shutdown:\$true\s+-ErrorAction\s+Stop'
        $moduleSource | Should -Match 'Stop-VM\s+-Name\s+\$name\s+-TurnOff\s+-Force\s+-ErrorAction\s+Stop'
        $moduleSource | Should -Match 'Restart-VM\s+-Name\s+\$name\s+-Force\s+-ErrorAction\s+Stop'
        $moduleSource | Should -Match 'Checkpoint-VM\s+-Name\s+\$vmName\s+-SnapshotName\s+\$checkpointName\s+-ErrorAction\s+Stop'
        $moduleSource | Should -Match 'Restore-VMSnapshot\s+-VMName\s+\$vmName\s+-Name\s+\$checkpointName\s+-Confirm:\$false\s+-ErrorAction\s+Stop'
        $moduleSource | Should -Match 'Remove-VMSnapshot\s+-VMName\s+\$vmName\s+-Name\s+\$checkpointName\s+-Confirm:\$false\s+-ErrorAction\s+Stop'
    }

    It 'starts a VM' {
        $result = Invoke-PcvVmLifecycle -Operation 'vm.start' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeTrue
        $result.data.action | Should -Be 'start'
        Should -Invoke Start-VM -Times 1 -ModuleName PcvHyperV
    }

    It 'performs a graceful shutdown' {
        $result = Invoke-PcvVmLifecycle -Operation 'vm.shutdown' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeTrue
        $result.data.action | Should -Be 'shutdown'
        Should -Invoke Stop-VM -ParameterFilter { $Shutdown -eq $true } -Times 1 -ModuleName PcvHyperV
    }

    It 'creates a named checkpoint' {
        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.create' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
            checkpoint_name = 'before-upgrade'
        })

        $result.ok | Should -BeTrue
        $result.data.name | Should -Be 'before-upgrade'
        Should -Invoke Checkpoint-VM -Times 1 -ModuleName PcvHyperV
    }

    It 'returns a retryable failure when a created checkpoint is not visible after creation' {
        Mock Get-VMSnapshot {
            @(
                [pscustomobject]@{ Name = 'other-checkpoint'; VMName = $VMName; CreationTime = [datetime]'2026-04-24T00:00:00Z' }
            )
        } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.create' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
            checkpoint_name = 'before-upgrade'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_NOT_VISIBLE'
        $result.error.retryable | Should -BeTrue
        Should -Invoke Checkpoint-VM -Times 1 -ModuleName PcvHyperV
        Should -Invoke Get-VMSnapshot -Times 3 -ModuleName PcvHyperV
        Should -Invoke Start-Sleep -Times 2 -ModuleName PcvHyperV
    }

    It 'retries checkpoint visibility before returning create success' {
        $script:CheckpointVisibilityAttempts = 0
        Mock Get-VMSnapshot {
            $script:CheckpointVisibilityAttempts += 1
            if ($script:CheckpointVisibilityAttempts -eq 1) {
                return @()
            }

            @(
                [pscustomobject]@{ Name = 'before-upgrade'; VMName = $VMName; CreationTime = [datetime]'2026-04-24T00:00:00Z' }
            )
        } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.create' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
            checkpoint_name = 'before-upgrade'
        })

        $result.ok | Should -BeTrue
        $result.data.name | Should -Be 'before-upgrade'
        Should -Invoke Checkpoint-VM -Times 1 -ModuleName PcvHyperV
        Should -Invoke Get-VMSnapshot -Times 2 -ModuleName PcvHyperV
        Should -Invoke Start-Sleep -Times 1 -ModuleName PcvHyperV
    }

    It 'lists checkpoints' {
        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.list' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeTrue
        $result.data[0].name | Should -Be 'before-upgrade'
    }

    It 'dispatches missing lifecycle params through the runner as structured failure' {
        $result = Invoke-PcvOperation -Request ([pscustomobject]@{
            operation = 'vm.start'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_LIFECYCLE_PARAMS_INVALID'
        Should -Invoke Start-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'dispatches null checkpoint params through the runner as structured failure' {
        $result = Invoke-PcvOperation -Request ([pscustomobject]@{
            operation = 'checkpoint.list'
            params = $null
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_PARAMS_INVALID'
        Should -Invoke Get-VMSnapshot -Times 0 -ModuleName PcvHyperV
    }

    It 'returns lifecycle failure when start emits a non-terminating error' {
        Mock Start-VM {
            [CmdletBinding()]
            param([System.Management.Automation.ActionPreference]$ErrorAction)
            Write-Error 'Start failed'
        } -ModuleName PcvHyperV

        $result = Invoke-PcvVmLifecycle -Operation 'vm.start' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_LIFECYCLE_FAILED'
    }

    It 'returns checkpoint failure when create emits a non-terminating error' {
        Mock Checkpoint-VM {
            [CmdletBinding()]
            param([System.Management.Automation.ActionPreference]$ErrorAction)
            Write-Error 'Checkpoint failed'
        } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.create' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
            checkpoint_name = 'before-upgrade'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_FAILED'
    }

    It 'rejects missing checkpoint names for restore before calling Hyper-V' {
        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.restore' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_NAME_INVALID'
        Should -Invoke Restore-VMSnapshot -Times 0 -ModuleName PcvHyperV
    }

    It 'rejects invalid checkpoint names for delete before calling Hyper-V' {
        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.delete' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
            checkpoint_name = 'bad name!'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_NAME_INVALID'
        Should -Invoke Remove-VMSnapshot -Times 0 -ModuleName PcvHyperV
    }

    It 'returns not found when VM lookup is ObjectNotFound' {
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

        $result = Invoke-PcvVmLifecycle -Operation 'vm.start' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_NOT_FOUND'
        Should -Invoke Start-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'returns checkpoint failure when checkpoint listing fails' {
        Mock Get-VMSnapshot { throw 'Snapshot service unavailable' } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.list' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_FAILED'
    }

    It 'returns checkpoint failure when checkpoint listing emits a non-terminating error' {
        Mock Get-VMSnapshot {
            [CmdletBinding()]
            param()
            Write-Error 'Snapshot service unavailable'
        } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.list' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_CHECKPOINT_FAILED'
    }

    It 'allows true empty checkpoint lists' {
        Mock Get-VMSnapshot { @() } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.list' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeTrue
        $result.data.Count | Should -Be 0
    }

    It 'returns null created_at when CreationTime is missing or null' {
        Mock Get-VMSnapshot {
            @(
                [pscustomobject]@{ Name = 'missing-created-at'; VMName = $VMName },
                [pscustomobject]@{ Name = 'null-created-at'; VMName = $VMName; CreationTime = $null }
            )
        } -ModuleName PcvHyperV

        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.list' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeTrue
        $result.data[0].created_at | Should -BeNullOrEmpty
        $result.data[1].created_at | Should -BeNullOrEmpty
    }

    It 'returns lookup failure when VM lookup fails unexpectedly' {
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

        $result = Invoke-PcvVmLifecycle -Operation 'vm.start' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_LOOKUP_FAILED'
        Should -Invoke Start-VM -Times 0 -ModuleName PcvHyperV
    }
}
