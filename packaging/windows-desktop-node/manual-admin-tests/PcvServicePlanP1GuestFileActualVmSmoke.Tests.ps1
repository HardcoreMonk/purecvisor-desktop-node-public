Set-StrictMode -Version Latest

BeforeAll {
function New-P1GuestFileBehaviorRuntime {
    param([scriptblock]$Configure)

    $state = [ordered]@{
        Operations = [System.Collections.Generic.List[object]]::new()
        InstalledVersion = '0.42.81-admin-smoke'
        VmId = [guid]'eeeeeeee-1111-2222-3333-444444444444'
        Notes = "guest_family=windows`npersistent_policy=keep-until-next-evidence-cycle"
        PowerState = 'Off'
        HeartbeatOk = $true
        GuestPrefixPresent = $true
        GuestFileExists = $false
        ShutdownWorks = $true
        PayloadSha = ('ab' * 32)
        Paths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    }
    $state.Paths.Add('C:\ProgramData\PureCVisor\desktop-node\guest-credentials\pcv-guest-installed-04253-r1.dpapi') | Out-Null
    $state.Paths.Add('C:\ProgramData\PureCVisor\desktop-node\guest-files') | Out-Null
    if ($null -ne $Configure) { & $Configure $state }

    # GetNewClosure() cannot see functions defined in BeforeAll, so the digest helper is captured as a variable.
    $lineDigest = {
        param([string]$Line)
        $bytes = [System.Text.Encoding]::UTF8.GetBytes("$Line`r`n")
        [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    }
    $adapter = {
        param([string]$Operation, [hashtable]$Payload)

        $state.Operations.Add([pscustomobject]@{ operation = $Operation; input = $Payload }) | Out-Null
        $guestResult = {
            param([string]$Line)
            [pscustomobject]@{
                status = 'succeeded'
                result = [pscustomobject]@{
                    data = [pscustomobject]@{ exit_code = 0; stdout_digest = (& $lineDigest $Line); stdout_byte_count = ([System.Text.Encoding]::UTF8.GetByteCount($Line) + 2) }
                }
            }
        }
        switch ($Operation) {
            'installed-product' {
                return [pscustomobject]@{ version = $state.InstalledVersion; cli_path = 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe'; cli_sha256 = ('a' * 64) }
            }
            'service-state' { return 'Running' }
            'vm-by-name' { return ,([pscustomobject]@{ Id = $state.VmId; Name = [string]$Payload.name; Notes = $state.Notes }) }
            'path-exists' { return $state.Paths.Contains([string]$Payload.path) }
            'vm-state' { return $state.PowerState }
            'wait-vm-state' { return $state.PowerState }
            'wait-heartbeat' { return [bool]$state.HeartbeatOk }
            'create-staging' {
                $state.Paths.Add([string]$Payload.directory) | Out-Null
                return [pscustomobject]@{ payload_sha256 = $state.PayloadSha; payload_length = [long]$Payload.payload_bytes; oversize_length = [long]$Payload.oversize_bytes }
            }
            'remove-directory' {
                $state.Paths.Remove([string]$Payload.path) | Out-Null
                return $true
            }
            'invoke-cli' {
                $step = [string]$Payload.step
                $rejected = { param([string]$Code) [pscustomobject]@{ exit_code = 1; stdout = (@{ ok = $false; error = @{ code = $Code } } | ConvertTo-Json -Compress); stderr = '' } }
                switch ($step) {
                    'guest-file-preview-outside' { return & $rejected 'PCV_GUEST_FILE_PATH_NOT_ALLOWED' }
                    'guest-file-preview-oversize' { return & $rejected 'PCV_GUEST_FILE_SIZE_LIMIT' }
                    'guest-file-unconfirmed' { return [pscustomobject]@{ exit_code = 2; stdout = ''; stderr = 'PCV_CLI_USAGE|Use: vm guest-file ...' } }
                    'guest-file-preview' {
                        return [pscustomobject]@{ exit_code = 0; stdout = (@{ data = @{ size_bytes = 1048576; copied = $false } } | ConvertTo-Json -Compress); stderr = '' }
                    }
                    default {
                        return [pscustomobject]@{ exit_code = 0; stdout = (@{ data = @{ job_id = "job-$step"; status = 'queued' } } | ConvertTo-Json -Compress); stderr = '' }
                    }
                }
            }
            'wait-job' {
                $step = [string]$Payload.step
                if ($step -like 'guest-channel-verify-*') {
                    return [pscustomobject]@{ status = 'succeeded'; result = [pscustomobject]@{ data = [pscustomobject]@{ transport = 'windows-powershell-direct' } } }
                }
                switch ($step) {
                    'vm-start' { $state.PowerState = 'Running'; return [pscustomobject]@{ status = 'succeeded' } }
                    'vm-shutdown' {
                        if ($state.ShutdownWorks) { $state.PowerState = 'Off' }
                        return [pscustomobject]@{ status = 'succeeded' }
                    }
                    'vm-poweroff' { $state.PowerState = 'Off'; return [pscustomobject]@{ status = 'succeeded' } }
                    'guest-prefix-probe' { return & $guestResult ([string]$state.GuestPrefixPresent) }
                    'guest-file-probe-after-preview' { return & $guestResult ([string]$state.GuestFileExists) }
                    'guest-file-copy' {
                        if (-not $state.GuestPrefixPresent) {
                            return [pscustomobject]@{ status = 'failed'; error = [pscustomobject]@{ code = 'PCV_GUEST_FILE_COPY_FAILED' } }
                        }
                        $state.GuestFileExists = $true
                        return [pscustomobject]@{ status = 'succeeded'; result = [pscustomobject]@{ data = [pscustomobject]@{ copied = $true; size_bytes = 1048576 } } }
                    }
                    'guest-file-hash' { return & $guestResult (([string]$state.PayloadSha).ToUpperInvariant()) }
                    'guest-file-remove' { $state.GuestFileExists = $false; return & $guestResult 'False' }
                    default { return [pscustomobject]@{ status = 'succeeded' } }
                }
            }
            default { throw "PCV_P1_GUESTFILE_TEST_ADAPTER_OPERATION_MISSING|$Operation" }
        }
    }.GetNewClosure()

    return [pscustomobject]@{ State = $state; Adapter = $adapter }
}

function Invoke-P1GuestFileScenario {
    param(
        [Parameter(Mandatory)][string]$Name,
        [scriptblock]$Configure
    )

    $runtime = New-P1GuestFileBehaviorRuntime -Configure $Configure
    $artifactRoot = Join-Path $TestDrive $Name
    $caught = $null
    try {
        & $script:RunnerPath -Version '0.42.81-admin-smoke' -ArtifactRoot $artifactRoot -RuntimeAdapter $runtime.Adapter | Out-Null
    }
    catch { $caught = $_ }
    $summaryPath = Join-Path $artifactRoot 'summary.json'
    $summary = if (Test-Path -LiteralPath $summaryPath) { Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json -Depth 100 } else { $null }
    return [pscustomobject]@{ Error = $caught; Summary = $summary; State = $runtime.State }
}

function Get-CliSteps {
    param($Run)

    return @($Run.State.Operations | Where-Object { $_.operation -eq 'invoke-cli' } | ForEach-Object { [string]$_.input.step })
}
}

Describe 'SERVICE_PLAN P1 guest file actual-VM runner contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:RunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1GuestFileActualVmSmoke.ps1'
    }

    It 'emits a non-mutating dry-run plan' {
        $artifactRoot = Join-Path $TestDrive 'plan'
        $result = & $script:RunnerPath -Version '0.42.81-admin-smoke' -ArtifactRoot $artifactRoot -DryRun

        $summary = Get-Content -LiteralPath (Join-Path $artifactRoot 'summary.json') -Raw | ConvertFrom-Json -Depth 100
        $summary.ok | Should -BeTrue
        $summary.overall_verdict | Should -Be 'NOT_RUN'
        $summary.host_mutation_performed | Should -BeFalse
        @($summary.plan | ForEach-Object slice) | Should -Be @(
            'preflight', 'vm_start', 'channel_verify', 'guest_prefix_probe', 'host_staging',
            'preview_path_not_allowed', 'preview_size_limit', 'preview_ok', 'copy_confirm_required',
            'copy', 'guest_readback', 'cleanup')
        $summary.guest_file_path | Should -Match '^C:\\Users\\Public\\PureCVisor\\pcv-p1-guestfile-04281-[0-9a-f]{8}\.bin$'
        @($result).Count | Should -Be 1
    }

    It 'passes the full probe and restores the recorded power state' {
        $run = Invoke-P1GuestFileScenario -Name 'pass'

        $run.Error | Should -BeNullOrEmpty
        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.power.before | Should -Be 'Off'
        $run.Summary.power.started_by_run | Should -BeTrue
        $run.Summary.power.after | Should -Be 'Off'
        $run.Summary.power.restored | Should -BeTrue
        $run.Summary.power.stop_method | Should -Be 'shutdown'
        $run.Summary.guest_prefix_present_before | Should -BeTrue
        $run.Summary.readbacks.preview_path_not_allowed.code | Should -Be 'PCV_GUEST_FILE_PATH_NOT_ALLOWED'
        $run.Summary.readbacks.preview_size_limit.code | Should -Be 'PCV_GUEST_FILE_SIZE_LIMIT'
        $run.Summary.readbacks.copy_confirm_required.code | Should -Be 'PCV_CLI_USAGE'
        $run.Summary.readbacks.guest_readback.hash_matches_payload | Should -BeTrue
        $run.Summary.cleanup.guest_file_removed | Should -BeTrue
        $run.Summary.cleanup.host_staging_removed | Should -BeTrue
        $run.State.GuestFileExists | Should -BeFalse
        $run.State.Paths.Contains($run.Summary.host_staging_dir) | Should -BeFalse
        $exec = @($run.State.Operations | Where-Object { $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'guest-file-hash' })
        $arguments = @($exec[0].input.arguments)
        $arguments[0..1] | Should -Be @('vm', 'guest-exec')
        $arguments | Should -Contain '--credential-ref'
        $arguments[[array]::IndexOf($arguments, '--') + 1] | Should -Be 'powershell.exe'
        $copy = @($run.State.Operations | Where-Object { $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'guest-file-copy' })
        @($copy[0].input.arguments) | Should -Contain '--yes'
        @((@($run.State.Operations | Where-Object { $_.operation -eq 'invoke-cli' -and [string]$_.input.step -eq 'guest-file-unconfirmed' }))[0].input.arguments) | Should -Not -Contain '--yes'
    }

    It 'records a missing guest prefix and fails the copy without creating it' {
        $run = Invoke-P1GuestFileScenario -Name 'prefix-missing' -Configure {
            param($state)
            $state.GuestPrefixPresent = $false
        }

        $run.Summary.overall_verdict | Should -Be 'FAIL'
        $run.Summary.guest_prefix_present_before | Should -BeFalse
        $run.Summary.slice_verdicts.copy | Should -Be 'FAIL'
        $run.Summary.queued_jobs.'guest-file-copy'.error_code | Should -Be 'PCV_GUEST_FILE_COPY_FAILED'
        $run.Summary.cleanup.verdict | Should -Be 'PASS'
        $run.Summary.power.restored | Should -BeTrue
        @(Get-CliSteps $run) | Should -Not -Contain 'guest-prefix-create'
    }

    It 'refuses a VM without the persistent keep policy before any mutation' {
        $run = Invoke-P1GuestFileScenario -Name 'not-persistent' -Configure {
            param($state)
            $state.Notes = 'guest_family=windows'
        }

        $run.Summary.error | Should -Match 'PCV_P1_GUESTFILE_VM_NOT_PERSISTENT_TARGET'
        $run.Summary.host_mutation_performed | Should -BeFalse
        @(Get-CliSteps $run) | Should -Not -Contain 'vm-start'
    }

    It 'leaves an already running guest running' {
        $run = Invoke-P1GuestFileScenario -Name 'already-running' -Configure {
            param($state)
            $state.PowerState = 'Running'
        }

        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.power.started_by_run | Should -BeFalse
        $run.Summary.power.after | Should -Be 'Running'
        @(Get-CliSteps $run) | Should -Not -Contain 'vm-start'
        @(Get-CliSteps $run) | Should -Not -Contain 'vm-shutdown'
    }

    It 'falls back to product poweroff when guest shutdown does not stop the VM' {
        $run = Invoke-P1GuestFileScenario -Name 'shutdown-stuck' -Configure {
            param($state)
            $state.ShutdownWorks = $false
        }

        $run.Summary.overall_verdict | Should -Be 'PASS'
        $run.Summary.power.stop_method | Should -Be 'poweroff'
        $run.Summary.power.after | Should -Be 'Off'
        @(Get-CliSteps $run) | Should -Contain 'vm-poweroff'
    }
}
