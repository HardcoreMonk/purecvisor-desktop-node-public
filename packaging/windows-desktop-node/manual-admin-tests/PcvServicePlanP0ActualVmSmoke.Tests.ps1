Set-StrictMode -Version Latest

BeforeAll {
function New-P0BehaviorRuntime {
    param([scriptblock]$Configure)

    $managedId = [guid]'11111111-1111-1111-1111-111111111111'
    $foreignId = [guid]'22222222-2222-2222-2222-222222222222'
    $collisionId = [guid]'33333333-3333-3333-3333-333333333333'
    $state = [ordered]@{
        Operations = [System.Collections.Generic.List[object]]::new()
        InstalledVersion = '0.42.75-admin-smoke'
        ServiceState = 'Running'
        ManagedId = $managedId
        ForeignId = $foreignId
        CollisionId = $collisionId
        ManagedExists = $false
        ForeignExists = $false
        ManagedState = 'Off'
        ForeignState = 'Off'
        ForeignNotes = ''
        SaveHyperVState = 'Saved'
        SaveProductState = 'saved'
        ResumeHyperVState = 'Running'
        ResumeProductState = 'running'
        DvdHostResource = $null
        CheckpointCurrentCount = 1
        JobOutcomes = @{}
        JobStderr = @{}
        RequireLowLevelCli = $false
        ServiceLossAfterEnqueue = $null
        LastEnqueuedStep = $null
        CreateCompleted = $false
        FailSummaryOnceAfterCreate = $false
        SummaryFailureInjected = $false
        PreexistingManagedRoot = $false
        PreexistingForeignRoot = $false
        PreexistingContents = @{ managed = $true; foreign = $true }
        CleanupIdentityDrift = $false
        LifecycleComplete = $false
        CollisionOnCleanup = $false
        CleanupRootFailure = $false
        VmGetNotFound = $false
        ProductDeleteVmIds = [System.Collections.Generic.List[string]]::new()
        NativeStopVmIds = [System.Collections.Generic.List[string]]::new()
        RemovedVmIds = [System.Collections.Generic.List[string]]::new()
        RemovedRoots = [System.Collections.Generic.List[string]]::new()
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
                $leaf = Split-Path -Path $path -Leaf
                $preexistingKind = if ($leaf -like '*managed' -and $state.PreexistingManagedRoot) {
                    'managed'
                }
                elseif ($leaf -like '*foreign' -and $state.PreexistingForeignRoot) {
                    'foreign'
                }
                else { $null }
                if ($null -ne $preexistingKind -or $state.ExistingRoots.Contains($path)) {
                    if ($null -ne $preexistingKind) { $state.PreexistingContents[$preexistingKind] = $false }
                    throw "simulated-directory-already-exists|$path"
                }
                $state.ExistingRoots.Add($path) | Out-Null
                return $true
            }
            'vm-by-name' {
                if ($Payload.purpose -eq 'preflight') { return @() }
                if ($Payload.purpose -eq 'cleanup-observation' -and $state.CollisionOnCleanup) {
                    return ,([pscustomobject]@{
                        Id = $state.CollisionId
                        Name = $Payload.name
                        Path = (Join-Path $Payload.vm_root 'foreign-collision')
                        State = 'Off'
                        Notes = ''
                    })
                }
                if ($Payload.name -like '*managed' -and $state.ManagedExists) {
                    $managedPath = Join-Path $Payload.vm_root $Payload.name
                    $state.ExistingRoots.Add($managedPath) | Out-Null
                    return ,([pscustomobject]@{
                        Id = $state.ManagedId
                        Name = $Payload.name
                        Path = $managedPath
                        State = $state.ManagedState
                        Notes = ''
                    })
                }
                if ($Payload.name -like '*foreign' -and $state.ForeignExists) {
                    $foreignPath = Join-Path $Payload.vm_root $Payload.name
                    $state.ExistingRoots.Add($foreignPath) | Out-Null
                    return ,([pscustomobject]@{
                        Id = $state.ForeignId
                        Name = $Payload.name
                        Path = $foreignPath
                        State = $state.ForeignState
                        Notes = $state.ForeignNotes
                    })
                }
                return @()
            }
            'vm-by-id' {
                $id = [guid]$Payload.id
                if ($id -eq $state.ManagedId -and $state.ManagedExists) {
                    $currentName = $Payload.name
                    $currentPath = Join-Path $Payload.vm_root $Payload.name
                    if ($state.CleanupIdentityDrift -and $state.LifecycleComplete) {
                        $currentName = "$($Payload.name)-renamed"
                        $currentPath = Join-Path $Payload.vm_root 'drifted-managed-root'
                    }
                    return [pscustomobject]@{
                        Id = $state.ManagedId
                        Name = $currentName
                        Path = $currentPath
                        State = $state.ManagedState
                        Notes = ''
                    }
                }
                if ($id -eq $state.ForeignId -and $state.ForeignExists) {
                    return [pscustomobject]@{
                        Id = $state.ForeignId
                        Name = $Payload.name
                        Path = (Join-Path $Payload.vm_root $Payload.name)
                        State = $state.ForeignState
                        Notes = $state.ForeignNotes
                    }
                }
                return $null
            }
            'invoke-cli' {
                $step = [string]$Payload.step
                $arguments = @($Payload.arguments)
                $state.LastEnqueuedStep = $step
                if ($step -like 'cleanup-delete-*') {
                    $state.ProductDeleteVmIds.Add([string]$arguments[2]) | Out-Null
                }
                if ($step -eq 'vm-get-state') {
                    $requested = [string]$arguments[2]
                    if ($state.VmGetNotFound -or $requested -ne 'pcv-p0-04275-behavior-managed') {
                        return [pscustomobject]@{
                            exit_code = 1
                            stdout = ''
                            stderr = "code=PCV_VM_NOT_FOUND`nmessage=VM not found"
                        }
                    }
                    $productState = if ($state.ManagedState -eq 'Saved') {
                        $state.SaveProductState
                    }
                    else {
                        $state.ResumeProductState
                    }
                    return [pscustomobject]@{
                        exit_code = 0
                        stdout = (@{ data = @{ name = $requested; state = $productState } } | ConvertTo-Json -Compress)
                        stderr = ''
                    }
                }
                if ([string]$state.ServiceLossAfterEnqueue -eq $step) {
                    $state.ServiceState = 'Stopped'
                }
                return [pscustomobject]@{
                    exit_code = 0
                    stdout = (@{
                        data = @{
                            job_id = "job-$step"
                            status = 'queued'
                        }
                    } | ConvertTo-Json -Compress)
                    stderr = [string]$state.JobStderr[$step]
                }
            }
            'enqueue-job' {
                if ($state.RequireLowLevelCli) {
                    throw 'PCV_P0_LOW_LEVEL_CLI_REQUIRED'
                }
                $step = [string]$Payload.step
                return [pscustomobject]@{
                    job_id = "job-$step"
                    status = 'queued'
                    stderr = [string]$state.JobStderr[$step]
                }
            }
            'wait-job' {
                $step = [string]$Payload.step
                $outcome = $state.JobOutcomes[$step]
                if ($outcome -eq 'timeout') { throw "PCV_P0_JOB_TIMEOUT|$step" }
                if ([string]::IsNullOrWhiteSpace([string]$outcome)) { $outcome = 'succeeded' }
                if ($outcome -eq 'succeeded') {
                    switch ($step) {
                        'vm-create' {
                            if ($state.PreexistingManagedRoot) { $state.PreexistingContents.managed = $false }
                            $state.ManagedExists = $true
                            $state.CreateCompleted = $true
                        }
                        'vm-start' { $state.ManagedState = 'Running' }
                        'vm-save' { $state.ManagedState = $state.SaveHyperVState }
                        'vm-resume-saved' {
                            $state.ManagedState = $state.ResumeHyperVState
                            $state.LifecycleComplete = $true
                        }
                        'vm-poweroff' { $state.ManagedState = 'Off' }
                        'vm-manage' { $state.ForeignNotes = 'managed-by=purecvisor-desktop-node' }
                        'managed-delete' { $state.ForeignExists = $false }
                        'cleanup-delete-managed' { $state.ManagedExists = $false }
                        'cleanup-delete-foreign' { $state.ForeignExists = $false }
                    }
                }
                $errorCode = if ($outcome -eq 'failed' -and $step -eq 'unmanaged-delete') {
                    'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
                }
                else { $null }
                return [pscustomobject]@{
                    status = $outcome
                    error = if ($null -eq $errorCode) { $null } else { [pscustomobject]@{ code = $errorCode } }
                }
            }
            'wait-hyperv-state' {
                if ($Payload.expected -eq 'Saved') { return $state.SaveHyperVState }
                if ($Payload.expected -eq 'Off') { return 'Off' }
                if ($Payload.phase -eq 'after-resume') { return $state.ResumeHyperVState }
                return 'Running'
            }
            'product-vm-state' {
                if ($Payload.phase -eq 'after-save') { return $state.SaveProductState }
                return $state.ResumeProductState
            }
            'dvd-readback' {
                $path = if ($null -eq $state.DvdHostResource) { $Payload.iso } else { $state.DvdHostResource }
                return [pscustomobject]@{ HostResource = @($path); Path = $path }
            }
            'checkpoint-list' {
                $rows = @()
                for ($index = 0; $index -lt $state.CheckpointCurrentCount; $index++) {
                    $rows += [pscustomobject]@{ name = $Payload.name; is_current = $true }
                }
                return $rows
            }
            'create-foreign-vm' {
                $state.ForeignExists = $true
                return [pscustomobject]@{
                    Id = $state.ForeignId
                    Name = $Payload.name
                    Path = (Join-Path $Payload.vm_root $Payload.name)
                    State = 'Off'
                    Notes = ''
                }
            }
            'stop-vm' {
                $state.NativeStopVmIds.Add([string]$Payload.id) | Out-Null
                return $true
            }
            'remove-vm' {
                $id = [guid]$Payload.id
                $state.RemovedVmIds.Add($id.ToString('D')) | Out-Null
                if ($id -eq $state.ManagedId) { $state.ManagedExists = $false }
                if ($id -eq $state.ForeignId) { $state.ForeignExists = $false }
                return $true
            }
            'remove-directory' {
                if ($state.CleanupRootFailure) { throw 'simulated-cleanup-root-failure' }
                $leaf = Split-Path -Path ([string]$Payload.path) -Leaf
                if ($leaf -like '*managed' -and $state.PreexistingManagedRoot) {
                    $state.PreexistingContents.managed = $false
                }
                if ($leaf -like '*foreign' -and $state.PreexistingForeignRoot) {
                    $state.PreexistingContents.foreign = $false
                }
                $state.RemovedRoots.Add([string]$Payload.path) | Out-Null
                $state.ExistingRoots.Remove([string]$Payload.path) | Out-Null
                return $true
            }
            'path-exists' {
                $path = [string]$Payload.path
                $leaf = Split-Path -Path $path -Leaf
                if (($leaf -like '*managed' -and $state.PreexistingManagedRoot) -or
                    ($leaf -like '*foreign' -and $state.PreexistingForeignRoot)) {
                    $state.ExistingRoots.Add($path) | Out-Null
                    return $true
                }
                return $state.ExistingRoots.Contains($path)
            }
            default { throw "PCV_P0_TEST_ADAPTER_OPERATION_MISSING|$Operation" }
        }
    }.GetNewClosure()

    return [pscustomobject]@{ State = $state; Adapter = $adapter }
}

function Invoke-P0BehaviorScenario {
    param(
        [Parameter(Mandatory)][string]$Name,
        [ValidateSet('SavedOnly', 'Full')][string]$Mode = 'SavedOnly',
        [scriptblock]$Configure,
        [scriptblock]$SummaryWriter
    )

    $runtime = New-P0BehaviorRuntime -Configure $Configure
    if ($null -eq $SummaryWriter -and $runtime.State.FailSummaryOnceAfterCreate) {
        $writerState = $runtime.State
        $SummaryWriter = {
            param($summaryPath, $temporaryPath, $json)
            if ($writerState.CreateCompleted -and -not $writerState.SummaryFailureInjected) {
                $writerState.SummaryFailureInjected = $true
                throw 'simulated-post-create-summary-write-failure'
            }
            [System.IO.File]::WriteAllText($temporaryPath, $json, [System.Text.UTF8Encoding]::new($false))
            Move-Item -LiteralPath $temporaryPath -Destination $summaryPath -Force
        }.GetNewClosure()
    }
    $artifactRoot = Join-Path $TestDrive $Name
    $parameters = @{
        Version = '0.42.75-admin-smoke'
        ArtifactRoot = $artifactRoot
        ProductRoot = (Join-Path $TestDrive 'mock-product')
        IsoPath = (Join-Path $TestDrive 'mock.iso')
        VmRoot = (Join-Path $TestDrive "vm-root/$Name")
        ManagedVm = 'pcv-p0-04275-behavior-managed'
        ForeignVm = 'pcv-p0-04275-behavior-foreign'
        CheckpointName = 'p0-behavior-restore'
        Mode = $Mode
        RuntimeAdapter = $runtime.Adapter
    }
    if ($null -ne $SummaryWriter) { $parameters.SummaryWriter = $SummaryWriter }

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

Describe 'SERVICE_PLAN P0 formal actual-VM runner contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:RunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1'
    }

    It 'publishes validated SavedOnly Full and DryRun inputs' {
        $script:RunnerPath | Should -Exist
        $source = Get-Content -LiteralPath $script:RunnerPath -Raw

        $source | Should -Match '(?s)\[Parameter\(Mandatory\)\].*?\[string\]\$Version'
        $source | Should -Match "\[ValidateSet\('SavedOnly', 'Full'\)\]"
        foreach ($parameter in @(
            'ArtifactRoot', 'ProductRoot', 'IsoPath', 'VmRoot', 'ManagedVm', 'ForeignVm',
            'CheckpointName', 'JobTimeoutSeconds', 'CommandTimeoutSeconds', 'DryRun')) {
            $source | Should -Match ([regex]::Escape("`$$parameter"))
        }
        $source | Should -Match 'PCV_P0_INSTALLED_VERSION_MISMATCH'
        $source | Should -Match 'PCV_P0_VM_NAME_INVALID'
        $source | Should -Match 'PCV_P0_VM_ALREADY_EXISTS'
        $source | Should -Match 'artifact_root_resolved'
        $source | Should -Match 'vm_root_resolved'
    }

    It 'pins SavedOnly state and Full slice postconditions with fail-stop ordering' {
        $source = Get-Content -LiteralPath $script:RunnerPath -Raw

        foreach ($token in @(
            "'vm-create'", "'vm-start'", "'vm-save'", "'vm-resume-saved'",
            "'Saved'", "'saved'", "'Running'", "'Paused'",
            "'media_attach'", "'checkpoint_restore'", "'saved_lifecycle'", "'managed_import'",
            'HostResource', 'is_current', 'PCV_VM_NOT_MANAGED_BY_PURECVISOR',
            'managed-by=purecvisor-desktop-node', 'Assert-SlicePassed')) {
            $source | Should -Match ([regex]::Escape($token))
        }
        $source.IndexOf("'saved_lifecycle'", [System.StringComparison]::Ordinal) |
            Should -BeLessThan $source.IndexOf("'media_attach'", [System.StringComparison]::Ordinal)
    }

    It 'pins exact identity cleanup atomic summary and redaction fail-closed guards' {
        $source = Get-Content -LiteralPath $script:RunnerPath -Raw

        foreach ($token in @(
            'installed_cli_sha256', 'managed_vm_id', 'foreign_vm_id', 'queued_jobs',
            'hyperv_state_after_save', 'product_state_after_save', 'cleanup',
            'host_mutation_performed', 'secret_observed', 'started_at', 'completed_at',
            'PCV_P0_CLEANUP_ID_MISMATCH', 'PCV_P0_CLEANUP_ROOT_INVALID',
            'PCV_P0_SUMMARY_WRITE_FAILED', 'PCV_P0_SERVICE_LOST',
            'summary.json.tmp', 'Move-Item -LiteralPath', 'Get-VM -Id')) {
            $source | Should -Match ([regex]::Escape($token))
        }
        $source | Should -Not -Match 'Remove-VM\s+-Name'
        $source | Should -Not -Match 'Get-VM\s*\|\s*Where-Object.*Remove-VM'
        $source | Should -Not -Match '(?i)bearer\s+[A-Za-z0-9._~+/-]+=*'
    }

    It 'emits a deterministic non-mutating dry-run summary without installed product access' {
        $artifactRoot = Join-Path $TestDrive 'actual-vm-plan'
        $vmRoot = Join-Path $TestDrive 'dedicated-vm-root'
        $result = & $script:RunnerPath `
            -Version '0.42.75-admin-smoke' `
            -ArtifactRoot $artifactRoot `
            -ProductRoot (Join-Path $TestDrive 'product-not-installed') `
            -IsoPath (Join-Path $TestDrive 'media-not-present.iso') `
            -VmRoot $vmRoot `
            -ManagedVm 'pcv-p0-04275-static-managed' `
            -ForeignVm 'pcv-p0-04275-static-foreign' `
            -CheckpointName 'p0-static-restore' `
            -Mode Full `
            -DryRun

        $summary = Get-Content -LiteralPath (Join-Path $artifactRoot 'summary.json') -Raw |
            ConvertFrom-Json -Depth 100
        $summary.ok | Should -BeTrue
        $summary.overall_verdict | Should -Be 'NOT_RUN'
        $summary.mode | Should -Be 'Full'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.secret_observed | Should -BeFalse
        $summary.actual_execution | Should -Be 'dry-run-no-installed-cli-or-hyperv'
        @($summary.plan | ForEach-Object slice) | Should -Be @(
            'saved_lifecycle', 'media_attach', 'checkpoint_restore', 'managed_import', 'cleanup')
        @($result).Count | Should -Be 1
    }

    It 'fails an installed-version mismatch before any VM-root or job mutation' {
        $run = Invoke-P0BehaviorScenario -Name 'version-mismatch' -Configure {
            param($state)
            $state.InstalledVersion = '0.42.74-admin-smoke'
        }

        $run.Error.Exception.Message | Should -Match 'PCV_P0_INSTALLED_VERSION_MISMATCH'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.host_mutation_performed | Should -BeFalse
        @($run.State.Operations.operation) | Should -Not -Contain 'create-directory'
        @($run.State.Operations.operation) | Should -Not -Contain 'enqueue-job'
    }

    It 'blocks nonempty exact managed and foreign roots before mutation and preserves their contents' -Tag 'cleanup-safety' {
        foreach ($kind in @('managed', 'foreign')) {
            $run = Invoke-P0BehaviorScenario -Name "preexisting-$kind-root" -Configure {
                param($state)
                if ($kind -eq 'managed') { $state.PreexistingManagedRoot = $true }
                else { $state.PreexistingForeignRoot = $true }
            }

            $run.Summary.overall_verdict | Should -Be 'FAIL'
            $run.Summary.error | Should -Match 'PCV_P0_VM_ROOT_ALREADY_EXISTS'
            $run.Summary.host_mutation_performed | Should -BeFalse
            $run.State.PreexistingContents[$kind] | Should -BeTrue
            @($run.State.ProductDeleteVmIds).Count | Should -Be 0
            @($run.State.NativeStopVmIds).Count | Should -Be 0
            @($run.State.RemovedVmIds).Count | Should -Be 0
            @($run.State.RemovedRoots).Count | Should -Be 0
            @($run.State.Operations.operation) | Should -Not -Contain 'create-directory'
            @($run.State.Operations.operation) | Should -Not -Contain 'invoke-cli'
        }
    }

    It 'blocks cleanup when exact-ID identity drifts and performs no cleanup mutation' -Tag 'cleanup-safety' {
        $run = Invoke-P0BehaviorScenario -Name 'cleanup-identity-drift' -Configure {
            param($state)
            $state.CleanupIdentityDrift = $true
        }

        $run.Summary.managed_vm_id | Should -Be $run.State.ManagedId.ToString('D')
        $run.Summary.cleanup.verdict | Should -Be 'FAIL'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.cleanup.error | Should -Match 'PCV_P0_CLEANUP_IDENTITY_DRIFT'
        @($run.State.ProductDeleteVmIds).Count | Should -Be 0
        @($run.State.NativeStopVmIds).Count | Should -Be 0
        @($run.State.RemovedVmIds).Count | Should -Be 0
        @($run.State.RemovedRoots).Count | Should -Be 0
        $managedRecord = @($run.Summary.cleanup.records | Where-Object kind -eq 'managed')[0]
        $managedRecord.identity_blocker | Should -BeTrue
        $run.State.ExistingRoots.Contains([string]$managedRecord.root) | Should -BeTrue
    }

    It 'preserves PCV_VM_NOT_FOUND from vm get instead of PCV_P0_COMMAND_FAILED' {
        $run = Invoke-P0BehaviorScenario -Name 'get-not-found' -Configure {
            param($state)
            $state.VmGetNotFound = $true
        }
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.error | Should -Be 'PCV_VM_NOT_FOUND'
        $run.Summary.error | Should -Not -Be 'PCV_P0_COMMAND_FAILED'
        $run.Summary.queued_jobs.'vm-save'.status | Should -Be 'succeeded'
        $run.Summary.slice_verdicts.saved_lifecycle | Should -Be 'FAIL'
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
    }

    It 'marks saved_lifecycle FAIL and runs exact cleanup on save readback mismatch' {
        $run = Invoke-P0BehaviorScenario -Name 'save-mismatch' -Mode Full -Configure {
            param($state)
            $state.SaveHyperVState = 'Paused'
        }

        $run.Summary.slice_verdicts.saved_lifecycle | Should -Be 'FAIL'
        $run.Summary.slice_verdicts.media_attach | Should -Be 'NOT_RUN'
        $run.Summary.slice_verdicts.checkpoint_restore | Should -Be 'NOT_RUN'
        $run.Summary.cleanup.attempted | Should -BeTrue
        $run.Summary.overall_verdict | Should -Be 'FAIL'
    }

    It 'fails saved_lifecycle when resume product readback is not running' {
        $run = Invoke-P0BehaviorScenario -Name 'resume-product-mismatch' -Configure {
            param($state)
            $state.ResumeProductState = 'saved'
        }

        $run.Summary.hyperv_state_after_resume | Should -Be 'Running'
        $run.Summary.product_state_after_resume | Should -Be 'saved'
        $run.Summary.slice_verdicts.saved_lifecycle | Should -Be 'FAIL'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
    }

    It 'fail-stops Full after media_attach and leaves later slices not run' {
        $run = Invoke-P0BehaviorScenario -Name 'full-fail-stop' -Mode Full -Configure {
            param($state)
            $state.DvdHostResource = 'C:\wrong\other.iso'
        }

        $run.Summary.slice_verdicts.saved_lifecycle | Should -Be 'PASS'
        $run.Summary.slice_verdicts.media_attach | Should -Be 'FAIL'
        $run.Summary.slice_verdicts.checkpoint_restore | Should -Be 'NOT_RUN'
        $run.Summary.slice_verdicts.managed_import | Should -Be 'NOT_RUN'
        @($run.State.Operations.input.step) | Should -Not -Contain 'checkpoint-create'
        @($run.State.Operations.operation) | Should -Not -Contain 'create-foreign-vm'
    }

    It 'retains queued job identity and timeout status when polling times out' {
        $run = Invoke-P0BehaviorScenario -Name 'job-timeout' -Configure {
            param($state)
            $state.JobOutcomes['vm-save'] = 'timeout'
        }

        $run.Summary.queued_jobs.'vm-save'.job_id | Should -Be 'job-vm-save'
        $run.Summary.queued_jobs.'vm-save'.initial_status | Should -Be 'queued'
        $run.Summary.queued_jobs.'vm-save'.status | Should -Be 'timed_out'
        $run.Summary.queued_jobs.'vm-save'.polling_status | Should -Be 'timeout'
        $run.Summary.slice_verdicts.saved_lifecycle | Should -Be 'FAIL'
    }

    It 'retains enqueued job identity when service is lost immediately after CLI enqueue' -Tag 'p1-retention' {
        $run = Invoke-P0BehaviorScenario -Name 'post-enqueue-service-loss' -Configure {
            param($state)
            $state.RequireLowLevelCli = $true
            $state.ServiceLossAfterEnqueue = 'vm-create'
        }

        $run.Summary.queued_jobs.'vm-create'.job_id | Should -Be 'job-vm-create'
        $run.Summary.queued_jobs.'vm-create'.initial_status | Should -Be 'queued'
        $run.Summary.queued_jobs.'vm-create'.status | Should -Be 'queued'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.error | Should -Match 'PCV_P0_SERVICE_LOST'
    }

    It 'records post-create identity before summary failure and exact-cleans that ID' -Tag 'p1-retention' {
        $run = Invoke-P0BehaviorScenario -Name 'post-create-summary-failure' -Configure {
            param($state)
            $state.RequireLowLevelCli = $true
            $state.FailSummaryOnceAfterCreate = $true
            $state.JobOutcomes['cleanup-delete-managed'] = 'failed'
        }

        $run.State.SummaryFailureInjected | Should -BeTrue
        $run.Summary.managed_vm_id | Should -Be $run.State.ManagedId.ToString('D')
        @($run.State.RemovedVmIds) | Should -Contain $run.State.ManagedId.ToString('D')
        @($run.State.RemovedVmIds) | Should -Not -Contain $run.State.ForeignId.ToString('D')
        @($run.State.RemovedVmIds) | Should -Not -Contain $run.State.CollisionId.ToString('D')
        $managedRecord = @($run.Summary.cleanup.records | Where-Object kind -eq 'managed')[0]
        @($run.State.RemovedRoots).Count | Should -Be 1
        @($run.State.RemovedRoots) | Should -Contain $managedRecord.root
        $managedRecord.native_fallback_used | Should -BeTrue
        $managedRecord.root_removed | Should -BeTrue
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.error | Should -Match 'PCV_P0_SUMMARY_WRITE_FAILED'
    }

    It 'records a same-name collision but still cleans only the exact owned ID' {
        $run = Invoke-P0BehaviorScenario -Name 'cleanup-collision' -Configure {
            param($state)
            $state.CollisionOnCleanup = $true
            $state.JobOutcomes['cleanup-delete-managed'] = 'failed'
        }

        $run.Summary.cleanup.same_name_different_id_blocked | Should -BeTrue
        $run.Summary.cleanup.verdict | Should -Be 'FAIL'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        @($run.State.RemovedVmIds) | Should -Contain $run.State.ManagedId.ToString('D')
        @($run.State.RemovedVmIds) | Should -Not -Contain $run.State.CollisionId.ToString('D')
    }

    It 'makes cleanup failure fail the overall result' {
        $run = Invoke-P0BehaviorScenario -Name 'cleanup-failure' -Configure {
            param($state)
            $state.CleanupRootFailure = $true
        }

        $run.Summary.cleanup.verdict | Should -Be 'FAIL'
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.ok | Should -BeFalse
    }

    It 'fails closed when the atomic summary writer fails and never publishes PASS' {
        $writerState = @{ Count = 0 }
        $writer = {
            param($summaryPath, $temporaryPath, $json)
            $writerState.Count++
            if ($writerState.Count -ge 3) { throw 'simulated-summary-write-failure' }
            [System.IO.File]::WriteAllText($temporaryPath, $json, [System.Text.UTF8Encoding]::new($false))
            Move-Item -LiteralPath $temporaryPath -Destination $summaryPath -Force
        }.GetNewClosure()
        $run = Invoke-P0BehaviorScenario -Name 'summary-write-failure' -SummaryWriter $writer

        $run.Error.Exception.Message | Should -Match 'PCV_P0_SUMMARY_WRITE_FAILED'
        if ($null -ne $run.Summary) {
            $run.Summary.ok | Should -BeFalse
            $run.Summary.overall_verdict | Should -Not -Be 'PASS'
        }
    }

    It 'detects and redacts process secrets without persisting their values' {
        $secret = 'Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJwMH0.signature-value'
        $run = Invoke-P0BehaviorScenario -Name 'secret-redaction' -Configure {
            param($state)
            $state.JobStderr['vm-save'] = $secret
        }

        $run.Summary.secret_observed | Should -BeTrue
        $run.Summary.overall_verdict | Should -Be 'FAIL'
        (Get-Content -LiteralPath $run.SummaryPath -Raw) | Should -Not -Match ([regex]::Escape($secret))
        (Get-Content -LiteralPath $run.SummaryPath -Raw) | Should -Not -Match 'signature-value'
    }
}
