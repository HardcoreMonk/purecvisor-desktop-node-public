Set-StrictMode -Version Latest

BeforeAll {
function New-P2OffVmBehaviorRuntime {
    param([scriptblock]$Configure)

    $state = [ordered]@{
        Operations = [System.Collections.Generic.List[object]]::new()
        InstalledVersion = '0.42.78-admin-smoke'
        ServiceState = 'Running'
        SwitchPresent = $true
        VmId = [guid]'cccccccc-cccc-cccc-cccc-cccccccccccc'
        VmName = $null
        VmRoot = $null
        ProductPowerState = 'stopped'
        NicSwitches = [System.Collections.Generic.List[string]]::new()
        DvdCount = 1
        CheckpointCount = 0
        DvdMode = 'job'
        ClearFails = $false
        ImportId = [guid]'dddddddd-dddd-dddd-dddd-dddddddddddd'
        ImportName = $null
        ExportDir = $null
        ExportVmcxCount = 1
        Exported = $false
        PendingDeleteName = $null
        Schedule = [ordered]@{ enabled = $false; interval_minutes = $null; retention_max = $null }
        Vms = @{}
        ExistingRoots = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    }
    if ($null -ne $Configure) { & $Configure $state }

    $adapter = {
        param([string]$Operation, [hashtable]$Payload)

        $state.Operations.Add([pscustomobject]@{ operation = $Operation; input = $Payload }) | Out-Null
        $queued = {
            param([string]$Step)
            [pscustomobject]@{
                exit_code = 0
                stdout = (@{ data = @{ job_id = "job-$Step"; status = 'queued' } } | ConvertTo-Json -Compress)
                stderr = ''
            }
        }
        $rejected = {
            param([string]$Code)
            [pscustomobject]@{
                exit_code = 1
                stdout = (@{ ok = $false; data = $null; error = @{ code = $Code } } | ConvertTo-Json -Compress)
                stderr = ''
            }
        }
        switch ($Operation) {
            'installed-product' {
                return [pscustomobject]@{
                    version = $state.InstalledVersion
                    cli_path = 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe'
                    cli_sha256 = ('a' * 64)
                    iso_exists = $true
                }
            }
            'service-state' { return $state.ServiceState }
            'switch-exists' { return [bool]$state.SwitchPresent }
            'create-directory' {
                $path = [string]$Payload.path
                if ($state.ExistingRoots.Contains($path)) { throw "simulated-directory-already-exists|$path" }
                $state.ExistingRoots.Add($path) | Out-Null
                return $true
            }
            'path-exists' { return $state.ExistingRoots.Contains([string]$Payload.path) }
            'remove-directory' {
                $state.ExistingRoots.Remove([string]$Payload.path) | Out-Null
                return $true
            }
            'vm-by-name' {
                $name = [string]$Payload.name
                if ($state.Vms.ContainsKey($name)) { return ,($state.Vms[$name]) }
                return @()
            }
            'vm-by-id' {
                foreach ($vm in @($state.Vms.Values)) {
                    if ([guid]$vm.Id -eq [guid]$Payload.id) { return $vm }
                }
                return $null
            }
            'export-package' {
                return [pscustomobject]@{
                    exists = [bool]$state.Exported
                    vmcx_count = if ($state.Exported) { $state.ExportVmcxCount } else { 0 }
                    vhdx_count = if ($state.Exported) { 1 } else { 0 }
                    vmgs_count = 0
                }
            }
            'vm-devices' {
                return [pscustomobject]@{
                    nic_count = $state.NicSwitches.Count
                    nic_switches = @($state.NicSwitches)
                    dvd_count = $state.DvdCount
                    checkpoint_count = $state.CheckpointCount
                }
            }
            'invoke-cli' {
                $step = [string]$Payload.step
                $arguments = @($Payload.arguments)
                if ($step -eq 'vm-create') {
                    for ($index = 0; $index -lt $arguments.Count - 1; $index++) {
                        if ($arguments[$index] -eq '--name') { $state.VmName = [string]$arguments[$index + 1] }
                        if ($arguments[$index] -eq '--vm-root') { $state.VmRoot = [string]$arguments[$index + 1] }
                    }
                    return & $queued $step
                }
                for ($index = 0; $index -lt $arguments.Count - 1; $index++) {
                    if ($arguments[$index] -eq '--directory' -and $step -eq 'vm-export') { $state.ExportDir = [string]$arguments[$index + 1] }
                    if ($arguments[$index] -eq '--name' -and $step -eq 'vm-import') { $state.ImportName = [string]$arguments[$index + 1] }
                }
                if ($step -like 'vm-delete*') {
                    $state.PendingDeleteName = [string]$arguments[2]
                    return & $queued $step
                }
                if ($step -like 'vm-get-*') {
                    return [pscustomobject]@{
                        exit_code = 0
                        stdout = (@{
                            data = @{
                                name = [string]$arguments[2]
                                state = [string]$state.ProductPowerState
                                generation = 2
                                managed_by_purecvisor = $true
                                network = @($state.NicSwitches | ForEach-Object { @{ switch = $_ } })
                                checkpoint_schedule = $state.Schedule
                            }
                        } | ConvertTo-Json -Compress -Depth 8)
                        stderr = ''
                    }
                }
                switch ($step) {
                    'vm-device-add-nic-unconfirmed' {
                        return [pscustomobject]@{
                            exit_code = 2
                            stdout = ''
                            stderr = "code=PCV_CLI_CONFIRMATION_REQUIRED`nmessage=confirmation required"
                        }
                    }
                    'vm-device-add-nic-limit' {
                        if ($state.NicSwitches.Count -ge 2) { return & $rejected 'PCV_VM_DEVICE_LIMIT' }
                        return & $queued $step
                    }
                    'vm-device-add-dvd' {
                        if ($state.DvdMode -eq 'route') { return & $rejected 'PCV_VM_DEVICE_ALREADY_PRESENT' }
                        return & $queued $step
                    }
                    'vm-checkpoint-schedule-preview' {
                        return [pscustomobject]@{
                            exit_code = 0
                            stdout = (@{ data = @{ dry_run = $true; interval_minutes = [int]$arguments[6]; retention_max = [int]$arguments[8] } } | ConvertTo-Json -Compress)
                            stderr = ''
                        }
                    }
                    'vm-checkpoint-schedule-set-invalid' { return & $rejected 'PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID' }
                    'vm-export-unconfirmed' {
                        return [pscustomobject]@{
                            exit_code = 2
                            stdout = ''
                            stderr = "code=PCV_CLI_CONFIRMATION_REQUIRED`nmessage=confirmation required"
                        }
                    }
                    'vm-export-preview-outside' { return & $rejected 'PCV_VM_EXPORT_PATH_NOT_ALLOWED' }
                    'vm-export-preview' {
                        return [pscustomobject]@{
                            exit_code = 0
                            stdout = (@{ data = @{ dry_run = $true; host_mutation_performed = $false } } | ConvertTo-Json -Compress)
                            stderr = ''
                        }
                    }
                    'vm-import-preview' {
                        return [pscustomobject]@{
                            exit_code = 0
                            stdout = (@{ data = @{ dry_run = $true; generate_new_id = $true; apply_managed_marker = $true } } | ConvertTo-Json -Compress)
                            stderr = ''
                        }
                    }
                    'vm-checkpoint-schedule-set' {
                        $state.Schedule.pending_interval = [int]$arguments[6]
                        $state.Schedule.pending_retention = [int]$arguments[8]
                        return & $queued $step
                    }
                    default { return & $queued $step }
                }
            }
            'wait-job' {
                switch ([string]$Payload.step) {
                    'vm-create' {
                        $root = Join-Path $state.VmRoot $state.VmName
                        $state.Vms[$state.VmName] = [pscustomobject]@{
                            Id = $state.VmId
                            Name = $state.VmName
                            Path = $root
                            State = 'Off'
                        }
                        $state.NicSwitches.Add('Default Switch') | Out-Null
                        return [pscustomobject]@{ status = 'succeeded'; vm_id = $state.VmId.ToString('D') }
                    }
                    'vm-device-add-nic' {
                        $state.NicSwitches.Add('Default Switch') | Out-Null
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    'vm-device-add-dvd' {
                        return [pscustomobject]@{ status = 'failed'; error = [pscustomobject]@{ code = 'PCV_VM_DEVICE_ALREADY_PRESENT' } }
                    }
                    'vm-checkpoint-schedule-set' {
                        $state.Schedule = [ordered]@{
                            enabled = $true
                            interval_minutes = $state.Schedule.pending_interval
                            retention_max = $state.Schedule.pending_retention
                        }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    'vm-checkpoint-schedule-clear' {
                        if ($state.ClearFails) { return [pscustomobject]@{ status = 'failed' } }
                        $state.Schedule = [ordered]@{ enabled = $false; interval_minutes = $null; retention_max = $null }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    'vm-checkpoint-schedule-clear-cleanup' {
                        $state.Schedule = [ordered]@{ enabled = $false; interval_minutes = $null; retention_max = $null }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    'vm-export' {
                        $state.Exported = $true
                        $state.ExistingRoots.Add([string]$state.ExportDir) | Out-Null
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    'vm-import' {
                        $state.Vms[$state.ImportName] = [pscustomobject]@{
                            Id = $state.ImportId
                            Name = $state.ImportName
                            Path = [string]$state.ExportDir
                            State = 'Off'
                        }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    { $_ -like 'vm-delete*' } {
                        $name = [string]$state.PendingDeleteName
                        if ($state.Vms.ContainsKey($name)) { $state.Vms.Remove($name) }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    default { return [pscustomobject]@{ status = 'succeeded' } }
                }
            }
            'wait-hyperv-state' { return [string]$Payload.expected }
            default { throw "PCV_P2_OFFVM_TEST_ADAPTER_OPERATION_MISSING|$Operation" }
        }
    }.GetNewClosure()

    return [pscustomobject]@{ State = $state; Adapter = $adapter }
}

function Invoke-P2OffVmBehaviorScenario {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Family,
        [scriptblock]$Configure
    )

    $runtime = New-P2OffVmBehaviorRuntime -Configure $Configure
    $artifactRoot = Join-Path $TestDrive $Name
    $parameters = @{
        Version = '0.42.78-admin-smoke'
        Family = $Family
        ArtifactRoot = $artifactRoot
        ProductRoot = (Join-Path $TestDrive 'mock-product')
        IsoPath = (Join-Path $TestDrive 'mock.iso')
        VmRoot = (Join-Path $TestDrive "vm-root/$Name")
        RuntimeAdapter = $runtime.Adapter
    }

    $caught = $null
    try { & $script:RunnerPath @parameters | Out-Null }
    catch { $caught = $_ }
    $summaryPath = Join-Path $artifactRoot 'summary.json'
    $summary = if (Test-Path -LiteralPath $summaryPath) {
        Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json -Depth 100
    }
    else { $null }
    return [pscustomobject]@{
        Error = $caught
        Summary = $summary
        State = $runtime.State
    }
}

function Get-CliStep {
    param($Run, [string]$Step)

    return @($Run.State.Operations | Where-Object {
        $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq $Step
    })
}
}

Describe 'SERVICE_PLAN P2 Off-VM actual-VM runner contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:RunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1'
    }

    It 'emits a non-mutating dry-run plan for <family>' -ForEach @(
        @{ family = 'device-add'; slices = @('source_create', 'nic_confirm_required', 'nic_add', 'nic_limit', 'dvd_guard', 'cleanup') }
        @{ family = 'checkpoint-schedule'; slices = @('source_create', 'schedule_preview', 'schedule_interval_invalid', 'schedule_set', 'schedule_clear', 'cleanup') }
        @{ family = 'export-import'; slices = @('source_create', 'export_confirm_required', 'export_path_not_allowed', 'export_preview', 'export', 'import_preview', 'import', 'cleanup') }
    ) {
        $artifactRoot = Join-Path $TestDrive "plan-$family"
        $result = & $script:RunnerPath `
            -Version '0.42.78-admin-smoke' `
            -Family $family `
            -ArtifactRoot $artifactRoot `
            -ProductRoot (Join-Path $TestDrive 'product-not-installed') `
            -VmRoot (Join-Path $TestDrive 'dedicated-vm-root/p2') `
            -DryRun

        $summary = Get-Content -LiteralPath (Join-Path $artifactRoot 'summary.json') -Raw | ConvertFrom-Json -Depth 100
        $summary.ok | Should -BeTrue
        $summary.overall_verdict | Should -Be 'NOT_RUN'
        $summary.family | Should -Be $family
        $summary.host_mutation_performed | Should -BeFalse
        $summary.actual_execution | Should -Be 'dry-run-no-installed-cli-or-hyperv'
        $summary.vm_name | Should -Match '^pcv-p2-offvm-0.42.78-|^pcv-p2-offvm-04278-'
        @($summary.plan | ForEach-Object slice) | Should -Be $slices
        @($summary.nonclaims).Count | Should -BeGreaterThan 0
        @($result).Count | Should -Be 1
    }

    It 'fails an installed-version mismatch before any VM-root or CLI mutation' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'version-mismatch' -Family 'device-add' -Configure {
            param($state)
            $state.InstalledVersion = '0.42.77-admin-smoke'
        }

        $run.Summary.error | Should -Match 'PCV_P2_OFFVM_INSTALLED_VERSION_MISMATCH'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.host_mutation_performed | Should -BeFalse
        @($run.State.Operations.operation) | Should -Not -Contain 'create-directory'
        @($run.State.Operations.operation) | Should -Not -Contain 'invoke-cli'
    }

    It 'fails a missing switch before any device-add mutation' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'switch-missing' -Family 'device-add' -Configure {
            param($state)
            $state.SwitchPresent = $false
        }

        $run.Summary.error | Should -Match 'PCV_P2_OFFVM_SWITCH_NOT_FOUND'
        $run.Summary.host_mutation_performed | Should -BeFalse
        @($run.State.Operations.operation) | Should -Not -Contain 'invoke-cli'
    }

    It 'passes device-add with NIC add, NIC limit, DVD job guard, and exact cleanup' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'device-add-pass' -Family 'device-add'

        $run.Error | Should -BeNullOrEmpty
        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.readbacks.source_create.product | Should -Be 'stopped'
        $run.Summary.readbacks.nic_confirm_required.code | Should -Be 'PCV_CLI_CONFIRMATION_REQUIRED'
        $run.Summary.readbacks.nic_add.hyperv_nic_count | Should -Be 2
        $run.Summary.readbacks.nic_add.product_nic_count | Should -Be 2
        $run.Summary.readbacks.nic_limit.code | Should -Be 'PCV_VM_DEVICE_LIMIT'
        $run.Summary.readbacks.dvd_guard.rejected_by | Should -Be 'job'
        $run.Summary.readbacks.dvd_guard.code | Should -Be 'PCV_VM_DEVICE_ALREADY_PRESENT'
        $run.Summary.readbacks.dvd_guard.dvd_count | Should -Be 1
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        $run.State.Vms.Count | Should -Be 0
        @((Get-CliStep $run 'vm-device-add-nic-unconfirmed')[0].input.arguments) | Should -Not -Contain '--yes'
        (Get-CliStep $run 'vm-delete')[0].input.arguments[2] | Should -Be $run.Summary.vm_name
    }

    It 'accepts a route-level DVD guard rejection' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'device-add-dvd-route' -Family 'device-add' -Configure {
            param($state)
            $state.DvdMode = 'route'
        }

        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.readbacks.dvd_guard.rejected_by | Should -Be 'route'
    }

    It 'passes checkpoint-schedule preview, invalid interval, set, clear, and cleanup' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'schedule-pass' -Family 'checkpoint-schedule'

        $run.Error | Should -BeNullOrEmpty
        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.readbacks.schedule_preview.after.enabled | Should -BeFalse
        $run.Summary.readbacks.schedule_interval_invalid.code | Should -Be 'PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID'
        $run.Summary.readbacks.schedule_set.enabled | Should -BeTrue
        $run.Summary.readbacks.schedule_set.interval_minutes | Should -Be 60
        $run.Summary.readbacks.schedule_set.retention_max | Should -Be 2
        $run.Summary.readbacks.schedule_clear.enabled | Should -BeFalse
        $run.Summary.cleanup.schedule_cleared | Should -BeNullOrEmpty
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        @(Get-CliStep $run 'vm-device-add-nic').Count | Should -Be 0
    }

    It 'passes export-import with a new identity and deletes the import before the source' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'export-import-pass' -Family 'export-import'

        $run.Error | Should -BeNullOrEmpty
        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.readbacks.export_confirm_required.code | Should -Be 'PCV_CLI_CONFIRMATION_REQUIRED'
        $run.Summary.readbacks.export_path_not_allowed.code | Should -Be 'PCV_VM_EXPORT_PATH_NOT_ALLOWED'
        $run.Summary.readbacks.export_preview.export_dir_absent | Should -BeTrue
        $run.Summary.readbacks.export.vmcx_count | Should -Be 1
        $run.Summary.readbacks.import_preview.target_absent | Should -BeTrue
        $run.Summary.readbacks.import.new_identity | Should -BeTrue
        $run.Summary.import_vm_id | Should -Be 'dddddddd-dddd-dddd-dddd-dddddddddddd'
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        $run.State.Vms.Count | Should -Be 0
        $run.State.ExistingRoots.Contains((Join-Path $run.Summary.vm_root_resolved 'exports')) | Should -BeFalse
        $deletes = @($run.State.Operations | Where-Object {
            $_.operation -eq 'invoke-cli' -and [string]$_.input.step -like 'vm-delete*'
        } | ForEach-Object { [string]$_.input.step })
        $deletes | Should -Be @('vm-delete-import', 'vm-delete')
        foreach ($step in @('vm-export', 'vm-export-preview', 'vm-import', 'vm-import-preview')) {
            $arguments = @((Get-CliStep $run $step)[0].input.arguments)
            $arguments[[array]::IndexOf($arguments, '--allowed-root') + 1] | Should -Be (Join-Path $run.Summary.vm_root_resolved 'exports')
        }
        @((Get-CliStep $run 'vm-export-unconfirmed')[0].input.arguments) | Should -Not -Contain '--yes'
    }

    It 'fails export when the package has no single vmcx and still cleans up' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'export-no-vmcx' -Family 'export-import' -Configure {
            param($state)
            $state.ExportVmcxCount = 0
        }

        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.slice_verdicts.export | Should -Be 'FAIL'
        $run.Summary.slice_verdicts.import | Should -Be 'NOT_RUN'
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        $run.State.Vms.Count | Should -Be 0
        @(Get-CliStep $run 'vm-delete-import').Count | Should -Be 0
    }

    It 'clears a left-over schedule during cleanup when the clear slice fails' {
        $run = Invoke-P2OffVmBehaviorScenario -Name 'schedule-clear-fails' -Family 'checkpoint-schedule' -Configure {
            param($state)
            $state.ClearFails = $true
        }

        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.slice_verdicts.schedule_clear | Should -Be 'FAIL'
        $run.Summary.cleanup.schedule_cleared | Should -BeTrue
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        $run.State.Schedule.enabled | Should -BeFalse
        $run.State.Vms.Count | Should -Be 0
    }
}
