Set-StrictMode -Version Latest

BeforeAll {
function New-P1CloneBehaviorRuntime {
    param([scriptblock]$Configure)

    $sourceId = [guid]'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
    $targetId = [guid]'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'
    $state = [ordered]@{
        Operations = [System.Collections.Generic.List[object]]::new()
        InstalledVersion = '0.42.76-admin-smoke'
        ServiceState = 'Running'
        SourceId = $sourceId
        TargetId = $targetId
        SourceName = 'pcv-p1-clone-04276-behavior-src'
        TargetName = 'pcv-p1-clone-04276-behavior-dst'
        VmRoot = $null
        PendingDeleteName = $null
        ProductPowerState = 'off'
        Vms = @{}
        ExistingRoots = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    }
    if ($null -ne $Configure) { & $Configure $state }

    $adapter = {
        param([string]$Operation, [hashtable]$Payload)

        $state.Operations.Add([pscustomobject]@{ operation = $Operation; input = $Payload }) | Out-Null
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
            'create-directory' {
                $path = [string]$Payload.path
                if ($state.ExistingRoots.Contains($path)) {
                    throw "simulated-directory-already-exists|$path"
                }
                $state.ExistingRoots.Add($path) | Out-Null
                return $true
            }
            'path-exists' {
                return $state.ExistingRoots.Contains([string]$Payload.path)
            }
            'remove-directory' {
                $path = [string]$Payload.path
                $state.ExistingRoots.Remove($path) | Out-Null
                return $true
            }
            'vm-by-name' {
                $name = [string]$Payload.name
                if ($state.Vms.ContainsKey($name)) {
                    return ,($state.Vms[$name])
                }
                return @()
            }
            'vm-by-id' {
                $id = [guid]$Payload.id
                foreach ($vm in @($state.Vms.Values)) {
                    if ([guid]$vm.Id -eq $id) { return $vm }
                }
                return $null
            }
            'invoke-cli' {
                $step = [string]$Payload.step
                $arguments = @($Payload.arguments)
                if ($step -eq 'vm-create') {
                    for ($index = 0; $index -lt $arguments.Count; $index++) {
                        if ($arguments[$index] -eq '--name' -and ($index + 1) -lt $arguments.Count) {
                            $state.SourceName = [string]$arguments[$index + 1]
                        }
                        if ($arguments[$index] -eq '--vm-root' -and ($index + 1) -lt $arguments.Count) {
                            $state.VmRoot = [string]$arguments[$index + 1]
                        }
                    }
                }
                if ($step -in @('vm-clone', 'vm-clone-preview', 'vm-clone-unconfirmed')) {
                    for ($index = 0; $index -lt $arguments.Count; $index++) {
                        if ($arguments[$index] -eq '--name' -and ($index + 1) -lt $arguments.Count) {
                            $state.TargetName = [string]$arguments[$index + 1]
                        }
                    }
                }
                if ($step -like 'vm-delete-*' -and $arguments.Count -ge 3) {
                    $state.PendingDeleteName = [string]$arguments[2]
                }
                if ($step -eq 'vm-clone-unconfirmed') {
                    return [pscustomobject]@{
                        exit_code = 1
                        stdout = ''
                        stderr = "code=PCV_CLI_CONFIRMATION_REQUIRED`nmessage=confirmation required"
                    }
                }
                if ($step -eq 'vm-clone-preview') {
                    return [pscustomobject]@{
                        exit_code = 0
                        stdout = (@{ data = @{ planned_copy_bytes = 1048576 } } | ConvertTo-Json -Compress)
                        stderr = ''
                    }
                }
                if ($step -in @('vm-get-state', 'vm-get-target')) {
                    $requested = if ($arguments.Count -ge 3) { [string]$arguments[2] } else { '' }
                    return [pscustomobject]@{
                        exit_code = 0
                        stdout = (@{
                            data = @{
                                name = $requested
                                state = [string]$state.ProductPowerState
                                managed_by_purecvisor = $true
                            }
                        } | ConvertTo-Json -Compress)
                        stderr = ''
                    }
                }
                return [pscustomobject]@{
                    exit_code = 0
                    stdout = (@{
                        data = @{
                            job_id = "job-$step"
                            status = 'queued'
                        }
                    } | ConvertTo-Json -Compress)
                    stderr = ''
                }
            }
            'wait-job' {
                $step = [string]$Payload.step
                switch ($step) {
                    'vm-create' {
                        $sourceRoot = Join-Path $state.VmRoot $state.SourceName
                        $state.ExistingRoots.Add((Join-Path $sourceRoot 'disk0.vhdx')) | Out-Null
                        $state.Vms[$state.SourceName] = [pscustomobject]@{
                            Id = $state.SourceId
                            Name = $state.SourceName
                            Path = $sourceRoot
                            State = 'Off'
                            Notes = 'managed-by=purecvisor-desktop-node'
                        }
                        return [pscustomobject]@{
                            status = 'succeeded'
                            vm_id = $state.SourceId.ToString('D')
                        }
                    }
                    'vm-clone' {
                        $targetRoot = Join-Path $state.VmRoot $state.TargetName
                        $state.ExistingRoots.Add((Join-Path $targetRoot 'disk0.vhdx')) | Out-Null
                        $state.Vms[$state.TargetName] = [pscustomobject]@{
                            Id = $state.TargetId
                            Name = $state.TargetName
                            Path = $targetRoot
                            State = 'Off'
                            Notes = 'managed-by=purecvisor-desktop-node'
                        }
                        return [pscustomobject]@{
                            status = 'succeeded'
                            vm_id = $state.TargetId.ToString('D')
                        }
                    }
                    { $_ -like 'vm-delete-*' } {
                        $name = [string]$state.PendingDeleteName
                        if (-not [string]::IsNullOrWhiteSpace($name) -and $state.Vms.ContainsKey($name)) {
                            $state.Vms.Remove($name)
                        }
                        if (-not [string]::IsNullOrWhiteSpace($name) -and -not [string]::IsNullOrWhiteSpace([string]$state.VmRoot)) {
                            $removedRoot = Join-Path $state.VmRoot $name
                            $state.ExistingRoots.Remove($removedRoot) | Out-Null
                            $state.ExistingRoots.Remove((Join-Path $removedRoot 'disk0.vhdx')) | Out-Null
                        }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    default {
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                }
            }
            'wait-hyperv-state' { return [string]$Payload.expected }
            default { throw "PCV_P1_CLONE_TEST_ADAPTER_OPERATION_MISSING|$Operation" }
        }
    }.GetNewClosure()

    return [pscustomobject]@{ State = $state; Adapter = $adapter }
}

function Invoke-P1CloneBehaviorScenario {
    param(
        [Parameter(Mandatory)][string]$Name,
        [scriptblock]$Configure
    )

    $runtime = New-P1CloneBehaviorRuntime -Configure $Configure
    $artifactRoot = Join-Path $TestDrive $Name
    $parameters = @{
        Version = '0.42.76-admin-smoke'
        ArtifactRoot = $artifactRoot
        ProductRoot = (Join-Path $TestDrive 'mock-product')
        IsoPath = (Join-Path $TestDrive 'mock.iso')
        VmRoot = (Join-Path $TestDrive "vm-root/$Name")
        SourceVm = 'pcv-p1-clone-04276-behavior-src'
        TargetVm = 'pcv-p1-clone-04276-behavior-dst'
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
        SummaryPath = $summaryPath
    }
}
}

Describe 'SERVICE_PLAN P1 clone actual-VM runner contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:RunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1'
    }

    It 'emits a deterministic non-mutating dry-run summary without installed product access' {
        $artifactRoot = Join-Path $TestDrive 'p1-clone-plan'
        $vmRoot = Join-Path $TestDrive 'dedicated-vm-root/clone'
        $result = & $script:RunnerPath `
            -Version '0.42.76-admin-smoke' `
            -ArtifactRoot $artifactRoot `
            -ProductRoot (Join-Path $TestDrive 'product-not-installed') `
            -IsoPath (Join-Path $TestDrive 'media-not-present.iso') `
            -VmRoot $vmRoot `
            -DryRun

        $summary = Get-Content -LiteralPath (Join-Path $artifactRoot 'summary.json') -Raw |
            ConvertFrom-Json -Depth 100
        $summary.ok | Should -BeTrue
        $summary.overall_verdict | Should -Be 'NOT_RUN'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.secret_observed | Should -BeFalse
        $summary.actual_execution | Should -Be 'dry-run-no-installed-cli-or-hyperv'
        @($summary.plan | ForEach-Object slice) | Should -Be @(
            'source_create', 'preview_mismatch', 'preview_ok', 'clone_ok', 'cleanup')
        @($result).Count | Should -Be 1
    }

    It 'fails an installed-version mismatch before any VM-root or CLI mutation' {
        $run = Invoke-P1CloneBehaviorScenario -Name 'version-mismatch' -Configure {
            param($state)
            $state.InstalledVersion = '0.42.75-admin-smoke'
        }

        $run.Error.Exception.Message | Should -Match 'PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH'
        $run.Summary.error | Should -Match 'PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.host_mutation_performed | Should -BeFalse
        @($run.State.Operations.operation) | Should -Not -Contain 'create-directory'
        @($run.State.Operations.operation) | Should -Not -Contain 'invoke-cli'
    }

    It 'queues source create with the product-minimum 8 GB disk' {
        $run = Invoke-P1CloneBehaviorScenario -Name 'source-create-disk-gb'

        $create = @($run.State.Operations | Where-Object {
            $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'vm-create'
        })
        $create.Count | Should -Be 1
        $arguments = @($create[0].input.arguments)
        $diskIndex = [array]::IndexOf($arguments, '--disk-gb')
        $diskIndex | Should -BeGreaterThan -1
        $arguments[$diskIndex + 1] | Should -Be '8'
        $run.Summary.slice_verdicts.source_create | Should -Be 'PASS'
    }

    It 'passes dedicated --vm-root on clone preview and confirmed clone' {
        $run = Invoke-P1CloneBehaviorScenario -Name 'clone-vm-root'

        $preview = @($run.State.Operations | Where-Object {
            $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'vm-clone-preview'
        })
        $clone = @($run.State.Operations | Where-Object {
            $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'vm-clone'
        })
        $preview.Count | Should -Be 1
        $clone.Count | Should -Be 1
        $previewArgs = @($preview[0].input.arguments)
        $cloneArgs = @($clone[0].input.arguments)
        $previewRootIndex = [array]::IndexOf($previewArgs, '--vm-root')
        $cloneRootIndex = [array]::IndexOf($cloneArgs, '--vm-root')
        $previewRootIndex | Should -BeGreaterThan -1
        $cloneRootIndex | Should -BeGreaterThan -1
        $previewArgs[$previewRootIndex + 1] | Should -Be $run.Summary.vm_root_resolved
        $cloneArgs[$cloneRootIndex + 1] | Should -Be $run.Summary.vm_root_resolved
        $run.Summary.slice_verdicts.clone_ok | Should -Be 'PASS'
    }

    It 'treats installed product stopped as Off after source create' {
        $run = Invoke-P1CloneBehaviorScenario -Name 'source-create-product-stopped' -Configure {
            param($state)
            $state.ProductPowerState = 'stopped'
        }

        $run.Summary.slice_verdicts.source_create | Should -Be 'PASS'
        $run.Summary.product_state_after_create | Should -Be 'stopped'
        $run.Summary.hyperv_state_after_create | Should -Be 'Off'
        $run.Summary.error | Should -BeNullOrEmpty
    }

    It 'records clone confirmation-required mismatch through the adapter without Hyper-V' {
        $run = Invoke-P1CloneBehaviorScenario -Name 'preview-mismatch-confirmation'

        $run.Summary.slice_verdicts.source_create | Should -Be 'PASS'
        $run.Summary.slice_verdicts.preview_mismatch | Should -Be 'PASS'
        $run.Summary.readbacks.preview_mismatch.exit_code | Should -Be 1
        $run.Summary.readbacks.preview_mismatch.code | Should -Be 'PCV_CLI_CONFIRMATION_REQUIRED'
        $run.Summary.readbacks.preview_mismatch.target_absent | Should -BeTrue
        $run.Summary.readbacks.preview_mismatch.target_disk_absent | Should -BeTrue
        $unconfirmed = @($run.State.Operations | Where-Object {
            $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'vm-clone-unconfirmed'
        })
        $unconfirmed.Count | Should -Be 1
        [string]$unconfirmed[0].input.arguments[0] | Should -Be 'vm'
        [string]$unconfirmed[0].input.arguments[1] | Should -Be 'clone'
        @($unconfirmed[0].input.arguments) | Should -Not -Contain '--yes'
        @($unconfirmed[0].input.arguments) | Should -Not -Contain '--dry-run'
        @($run.State.Operations.operation) | Should -Not -Contain 'Get-VM'
    }
}
