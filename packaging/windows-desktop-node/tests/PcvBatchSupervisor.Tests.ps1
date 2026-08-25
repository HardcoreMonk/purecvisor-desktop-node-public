Set-StrictMode -Version Latest

Describe 'PcvBatchSupervisor v1 contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1'

        function New-PcvFakeBatchStream {
            param([string]$Text)
            $stream = [pscustomobject]@{ Text = $Text }
            $stream | Add-Member -MemberType ScriptMethod -Name ReadToEndAsync -Value {
                [System.Threading.Tasks.Task[string]]::FromResult([string]$this.Text)
            }
            $stream
        }

        function New-PcvFakeBatchProcess {
            param(
                [int]$ExitCode = 0,
                [string]$Stdout = '',
                [string]$Stderr = ''
            )
            $process = [pscustomobject]@{
                StartInfo = $null
                HasExited = $false
                ExitCode = $ExitCode
                StandardOutput = New-PcvFakeBatchStream -Text $Stdout
                StandardError = New-PcvFakeBatchStream -Text $Stderr
                Killed = $false
                Disposed = $false
            }
            $process | Add-Member -MemberType ScriptMethod -Name Start -Value { $true }
            $process | Add-Member -MemberType ScriptMethod -Name Kill -Value {
                param([bool]$EntireProcessTree)
                $this.Killed = $true
                $this.HasExited = $true
            }
            $process | Add-Member -MemberType ScriptMethod -Name WaitForExit -Value {
                param([int]$Milliseconds)
                $true
            }
            $process | Add-Member -MemberType ScriptMethod -Name Dispose -Value {
                $this.Disposed = $true
            }
            $process
        }
    }

    BeforeEach {
        Import-Module $script:ModulePath -Force
    }

    It 'builds a non-mutating packaging regression manifest' {
        $artifactRoot = Join-Path $TestDrive 'batch-packaging'

        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'packaging-regression' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Profile PackagingRegression

        $manifest.schema_version | Should -Be 1
        $manifest.batch_id | Should -Be 'packaging-regression'
        $manifest.gpu_snapshot_interval_seconds | Should -Be 5
        $manifest.steps.Count | Should -Be 4
        @($manifest.steps | ForEach-Object { $_.id }) | Should -Contain 'public-boundary-ci-required'
        @($manifest.steps | Where-Object { $_.requires_admin -or $_.mutates_host }).Count | Should -Be 0
        ($manifest.steps | ConvertTo-Json -Depth 12) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
    }

    It 'writes dry-run artifacts without executing commands' {
        $artifactRoot = Join-Path $TestDrive 'batch-dry-run'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'dry-run' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'echo-secret' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Output should-not-run') `
                    -TimeoutSeconds 30)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun

        $result.ok | Should -BeTrue
        $result.dry_run | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'batch-manifest.resolved.dry-run.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.dry-run.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'batch-manifest.resolved.json') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $artifactRoot 'current-step.json') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results') | Should -BeFalse
    }

    It 'keeps a completed real-run summary intact across a later dry-run of the same artifact root' {
        $artifactRoot = Join-Path $TestDrive 'batch-dry-run-preserve'
        New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
        $realSummaryPath = Join-Path $artifactRoot 'summary.json'
        $realSummary = @{ schema_version = 1; ok = $true; dry_run = $false; status = 'completed'; batch_id = 'real-run' } | ConvertTo-Json -Depth 4
        Set-Content -LiteralPath $realSummaryPath -Value $realSummary -Encoding UTF8
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'real-run' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'echo-noop' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Output should-not-run') `
                    -TimeoutSeconds 30)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun

        $result.dry_run | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.dry-run.json') | Should -BeTrue
        $preserved = Get-Content -Raw -LiteralPath $realSummaryPath | ConvertFrom-Json
        $preserved.dry_run | Should -BeFalse
        $preserved.batch_id | Should -Be 'real-run'
        $preserved.status | Should -Be 'completed'
    }

    It 'rejects host mutation steps without explicit allowance' {
        $artifactRoot = Join-Path $TestDrive 'batch-mutation-block'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'mutation-block' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'msi-install' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'msiexec.exe' `
                    -Arguments @('/i', 'fake.msi') `
                    -TimeoutSeconds 30 `
                    -RequiresAdmin $true `
                    -MutatesHost $true)
            )

        {
            Invoke-PcvBatchSupervisor -Manifest $manifest
        } | Should -Throw -ExpectedMessage '*PCV_BATCH_HOST_MUTATION_APPROVAL_REQUIRED*'

        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results') | Should -BeFalse
    }

    It 'rejects automatic reboot capable commands even when host mutation is allowed' {
        $artifactRoot = Join-Path $TestDrive 'batch-reboot-block'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'reboot-block' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'reboot' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Restart-Computer') `
                    -TimeoutSeconds 30 `
                    -RequiresAdmin $true `
                    -MutatesHost $true)
            )

        {
            Invoke-PcvBatchSupervisor -Manifest $manifest -AllowHostMutation
        } | Should -Throw -ExpectedMessage '*PCV_BATCH_REBOOT_COMMAND_FORBIDDEN*'
    }

    It 'times out a hanging process and records heartbeat plus failed summary' {
        $artifactRoot = Join-Path $TestDrive 'batch-timeout'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'timeout' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -HeartbeatIntervalSeconds 1 `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'sleep-too-long' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Start-Sleep -Seconds 10') `
                    -TimeoutSeconds 1)
            )

        $script:FakeNow = [datetime]'2026-07-16T00:00:00Z'
        $script:FakeProcess = New-PcvFakeBatchProcess
        $factory = { param($StartInfo) $script:FakeProcess.StartInfo = $StartInfo; $script:FakeProcess }
        $now = { $script:FakeNow }
        $wait = {
            param([int]$Milliseconds)
            $script:FakeNow = $script:FakeNow.AddMilliseconds($Milliseconds)
        }

        $result = Invoke-PcvBatchSupervisor `
            -Manifest $manifest `
            -ProcessFactory $factory `
            -NowProvider $now `
            -WaitAction $wait

        $result.ok | Should -BeFalse
        $result.status | Should -Be 'failed'
        $result.failed_step_id | Should -Be 'sleep-too-long'
        $stepResult = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'step-results/001-sleep-too-long.json') | ConvertFrom-Json
        $stepResult.timed_out | Should -BeTrue
        $stepResult.exit_code | Should -BeNullOrEmpty
        Get-Content -LiteralPath (Join-Path $artifactRoot 'heartbeat.jsonl') | Should -Match 'sleep-too-long'
    }

    It 'records GPU adapter and process counter snapshots while a step runs' {
        $artifactRoot = Join-Path $TestDrive 'batch-gpu-snapshots'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'gpu-snapshots' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -HeartbeatIntervalSeconds 1 `
            -GpuSnapshotIntervalSeconds 1 `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'short-running-step' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Start-Sleep -Seconds 2; Write-Output done') `
                    -TimeoutSeconds 30)
            )

        $script:FakeNow = [datetime]'2026-07-16T00:00:00Z'
        $script:FakeProcess = New-PcvFakeBatchProcess -Stdout 'done'
        $script:WaitCount = 0
        $factory = { param($StartInfo) $script:FakeProcess.StartInfo = $StartInfo; $script:FakeProcess }
        $now = { $script:FakeNow }
        $wait = {
            param([int]$Milliseconds)
            $script:WaitCount++
            $script:FakeNow = $script:FakeNow.AddMilliseconds($Milliseconds)
            if ($script:WaitCount -ge 2) { $script:FakeProcess.HasExited = $true }
        }

        $result = Invoke-PcvBatchSupervisor `
            -Manifest $manifest `
            -ProcessFactory $factory `
            -NowProvider $now `
            -WaitAction $wait

        $result.ok | Should -BeTrue
        $snapshotPath = Join-Path $artifactRoot 'gpu-snapshots.jsonl'
        Test-Path -LiteralPath $snapshotPath | Should -BeTrue
        $snapshots = @(Get-Content -LiteralPath $snapshotPath | ForEach-Object { $_ | ConvertFrom-Json })
        $snapshots.Count | Should -BeGreaterThan 0
        $snapshots[0].schema_version | Should -Be 1
        $snapshots[0].batch_id | Should -Be 'gpu-snapshots'
        $snapshots[0].step_id | Should -Be 'short-running-step'
        $snapshots[0].interval_seconds | Should -Be 1
        $snapshots[0].PSObject.Properties.Name | Should -Contain 'adapter_memory'
        $snapshots[0].PSObject.Properties.Name | Should -Contain 'process_memory'
        if ($snapshots[0].status -eq 'collected') {
            @($snapshots[0].adapter_memory) | Should -Not -BeNullOrEmpty
        }

        $stepResult = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'step-results/001-short-running-step.json') | ConvertFrom-Json
        $stepResult.gpu_snapshot_count | Should -BeGreaterThan 0
        $stepResult.gpu_snapshot_path | Should -Match 'gpu-snapshots\.jsonl'
    }

    It 'resumes by skipping successful matching command fingerprints' {
        $artifactRoot = Join-Path $TestDrive 'batch-resume'
        $marker = Join-Path $TestDrive 'marker.txt'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'resume' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'write-once' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', "Add-Content -LiteralPath '$marker' -Value once") `
                    -TimeoutSeconds 30),
                (New-PcvBatchSupervisorStep `
                    -Id 'write-twice' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', "Add-Content -LiteralPath '$marker' -Value twice") `
                    -TimeoutSeconds 30)
            )

        $script:FakeNow = [datetime]'2026-07-16T00:00:00Z'
        $script:FactoryCount = 0
        $script:ActiveProcess = $null
        $factory = {
            param($StartInfo)
            $script:FactoryCount++
            $script:ActiveProcess = New-PcvFakeBatchProcess -Stdout 'written'
            $script:ActiveProcess.StartInfo = $StartInfo
            $script:ActiveProcess
        }
        $now = { $script:FakeNow }
        $wait = {
            param([int]$Milliseconds)
            $script:FakeNow = $script:FakeNow.AddMilliseconds($Milliseconds)
            $script:ActiveProcess.HasExited = $true
        }

        $first = Invoke-PcvBatchSupervisor -Manifest $manifest -ProcessFactory $factory -NowProvider $now -WaitAction $wait
        $second = Invoke-PcvBatchSupervisor -Manifest $manifest -Resume -ProcessFactory $factory -NowProvider $now -WaitAction $wait

        $first.ok | Should -BeTrue
        $second.ok | Should -BeTrue
        $script:FactoryCount | Should -Be 2
        $second.skipped_steps | Should -Contain 'write-once'
        $second.skipped_steps | Should -Contain 'write-twice'
    }

    It 'redacts tokens and known paths from arguments and captured output' {
        $artifactRoot = Join-Path $TestDrive 'batch-redaction'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'redaction' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -PathRedactions @{
                $script:RepoRoot = '[REPO_ROOT]'
                'C:\ProgramData\PureCVisor\desktop-node' = '[DATA_ROOT]'
            } `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'print-secret' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Output "Authorization: Bearer abc.def.secret"; Write-Output "C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json"') `
                    -TimeoutSeconds 30)
            )

        $secretOutput = "Authorization: Bearer abc.def.secret`nC:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json"
        $script:FakeNow = [datetime]'2026-07-16T00:00:00Z'
        $script:FakeProcess = New-PcvFakeBatchProcess -Stdout $secretOutput
        $factory = { param($StartInfo) $script:FakeProcess.StartInfo = $StartInfo; $script:FakeProcess }
        $now = { $script:FakeNow }
        $wait = {
            param([int]$Milliseconds)
            $script:FakeNow = $script:FakeNow.AddMilliseconds($Milliseconds)
            $script:FakeProcess.HasExited = $true
        }

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -ProcessFactory $factory -NowProvider $now -WaitAction $wait

        $result.ok | Should -BeTrue
        $text = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'step-results/001-print-secret.json')
        $text | Should -Match 'Bearer \[REDACTED\]'
        $text | Should -Match '\[DATA_ROOT\]'
        $text | Should -Not -Match 'abc\.def\.secret|api-token\.dpapi\.json'

        ConvertTo-PcvBatchRedactedText -Text 'redacts bearer tokens inside diagnostic strings' -PathRedactions @{} |
            Should -Be 'redacts bearer tokens inside diagnostic strings'
    }

    It 'runs the CLI entrypoint from a manifest file' {
        $artifactRoot = Join-Path $TestDrive 'batch-entrypoint'
        $manifestPath = Join-Path $TestDrive 'manifest.json'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'entrypoint' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'echo' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Output entrypoint-ok') `
                    -TimeoutSeconds 30)
            )
        $manifest | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
        $entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1'

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $entrypoint -ManifestPath $manifestPath | ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $json.ok | Should -BeTrue
        $json.batch_id | Should -Be 'entrypoint'
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
    }

    It 'records a real process-start failure without waiting' {
        $artifactRoot = Join-Path $TestDrive 'batch-process-start-failure'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'process-start-failure' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'missing-command' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pcv-command-that-does-not-exist.exe' `
                    -Arguments @('--probe') `
                    -TimeoutSeconds 30)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest

        $result.ok | Should -BeFalse
        $result.results[0].exit_code | Should -Be -1
        $result.results[0].timed_out | Should -BeFalse
        $result.results[0].start_failure | Should -Not -BeNullOrEmpty
    }

    It 'builds WebRegression profile without host mutation commands' {
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'web-regression' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'web-regression') `
            -Profile WebRegression

        $manifest.steps.Count | Should -Be 4
        @($manifest.steps | ForEach-Object { $_.id }) | Should -Contain 'web-pester'
        @($manifest.steps | ForEach-Object { $_.id }) | Should -Contain 'web-verify-parity'
        $npmCommand = if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
            'npm.cmd'
        }
        else {
            'npm'
        }
        @($manifest.steps | Where-Object { $_.id -in @('web-npm-test', 'web-verify-parity') } | ForEach-Object { Split-Path -Leaf ([string]$_.file_name) }) |
            Should -Be @($npmCommand, $npmCommand)
        ($manifest.steps | ConvertTo-Json -Depth 12) | Should -Not -Match 'msiexec|New-VM|Remove-VM|New-NetFirewallRule|Restart-Computer'
    }

    It 'requires the public boundary guard in packaging regression manifests' {
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'packaging-regression-public-boundary' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'packaging-regression-public-boundary') `
            -Profile PackagingRegression

        $step = $manifest.steps | Where-Object { $_.id -eq 'public-boundary-ci-required' } | Select-Object -First 1

        $step | Should -Not -BeNullOrEmpty
        $step.requires_admin | Should -BeFalse
        $step.mutates_host | Should -BeFalse
        $step.arguments | Should -Contain 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1'
        ($step.arguments -join ' ') | Should -Match 'PUBLIC_BOUNDARY_CI_CONTRACT|public-boundary-ci-required|Post 0.42.18 follow-up execution'
    }

    It 'saves a generated manifest file' {
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'save-manifest' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'save-manifest') `
            -Profile PackagingRegression

        $save = Save-PcvBatchSupervisorManifest -Manifest $manifest -Path (Join-Path $TestDrive 'manifest.json')

        $save.ok | Should -BeTrue
        Test-Path -LiteralPath $save.path | Should -BeTrue
        $saved = Get-Content -Raw -LiteralPath $save.path | ConvertFrom-Json
        $saved.batch_id | Should -Be 'save-manifest'
    }

    It 'builds ServiceMsiHyperVAdminSmoke as one guarded admin mutation step' {
        $routeArtifact = Join-Path $TestDrive 'routeparity-artifact'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'service-msi-hyperv-admin-smoke' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'batch-service-msi-hyperv') `
            -Profile ServiceMsiHyperVAdminSmoke `
            -ProfileOptions @{
                version = '0.36.2-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = $routeArtifact
            }

        $manifest.steps.Count | Should -Be 1
        $step = $manifest.steps[0]
        $step.id | Should -Be 'service-msi-hyperv-admin-smoke'
        $step.requires_admin | Should -BeTrue
        $step.mutates_host | Should -BeTrue
        $step.timeout_seconds | Should -Be 3600
        $step.arguments | Should -Contain 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1'
        $step.arguments | Should -Contain '-Version'
        $step.arguments | Should -Contain '0.36.2-admin-smoke'
        $step.arguments | Should -Contain '-IsoPath'
        $step.arguments | Should -Contain 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
        $step.arguments | Should -Contain '-ArtifactRoot'
        $step.arguments | Should -Contain ([System.IO.Path]::GetFullPath($routeArtifact))
    }

    It 'passes explicit batch evidence root into ServiceMsiHyperVAdminSmoke route runner' {
        $routeArtifact = Join-Path $TestDrive 'routeparity-artifact'
        $batchEvidenceRoot = Join-Path $TestDrive 'batch-runs'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'service-msi-hyperv-admin-smoke-batch-root' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'batch-service-msi-hyperv-batch-root') `
            -Profile ServiceMsiHyperVAdminSmoke `
            -ProfileOptions @{
                version = '0.36.2-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = $routeArtifact
                batch_evidence_root = $batchEvidenceRoot
            }

        $step = $manifest.steps[0]

        $step.arguments | Should -Contain '-BatchEvidenceRoot'
        $step.arguments | Should -Contain ([System.IO.Path]::GetFullPath($batchEvidenceRoot))
        ($step.arguments | ConvertTo-Json -Depth 12) | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|Register-ScheduledTask'
    }

    It 'builds OsMutationGate as one guarded admin mutation step' {
        $routeArtifact = Join-Path $TestDrive 'routeparity-artifact'
        $osArtifact = Join-Path $TestDrive 'os-gate-artifact'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'os-mutation-gate' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'batch-os-gate') `
            -Profile OsMutationGate `
            -ProfileOptions @{
                version = '0.36.2-admin-smoke'
                routeparity_artifact_root = $routeArtifact
                os_gate_artifact_root = $osArtifact
                lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
            }

        $manifest.steps.Count | Should -Be 1
        $step = $manifest.steps[0]
        $step.id | Should -Be 'os-mutation-gate'
        $step.requires_admin | Should -BeTrue
        $step.mutates_host | Should -BeTrue
        $step.timeout_seconds | Should -Be 1800
        $step.arguments | Should -Contain 'packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1'
        $step.arguments | Should -Contain '-Version'
        $step.arguments | Should -Contain '0.36.2-admin-smoke'
        $step.arguments | Should -Contain '-RouteParityArtifactRoot'
        $step.arguments | Should -Contain ([System.IO.Path]::GetFullPath($routeArtifact))
        $step.arguments | Should -Contain '-ArtifactRoot'
        $step.arguments | Should -Contain ([System.IO.Path]::GetFullPath($osArtifact))
        $step.arguments | Should -Contain '-LanPrefix'
        $step.arguments | Should -Contain 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
    }

    It 'builds ManualAdminCampaignDescriptor as one non-mutating descriptor step' {
        $campaignArtifact = Join-Path $TestDrive 'manual-admin-campaign'
        $descriptorArtifact = Join-Path $campaignArtifact 'manual-admin-campaign-descriptor'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'manual-admin-campaign-descriptor' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'batch-manual-admin-descriptor') `
            -Profile ManualAdminCampaignDescriptor `
            -ProfileOptions @{
                descriptor_artifact_root = $descriptorArtifact
                campaign_artifact_root = $campaignArtifact
                baseline_version = '0.42.5-admin-smoke'
                target_version = '0.42.6-admin-smoke'
                readiness_summary_path = (Join-Path $campaignArtifact 'manual-admin-rebaseline-readiness-r2/summary.json')
                product_update_summary_path = (Join-Path $campaignArtifact 'lifecycle/product-update-rollback/update-0425-to-0426-summary.json')
                product_rollback_summary_path = (Join-Path $campaignArtifact 'lifecycle/product-update-rollback/rollback-0426-to-0425-summary.json')
                clean_host_summary_path = (Join-Path $campaignArtifact 'clean-host/summary.json')
                burn_lifecycle_summary_path = (Join-Path $campaignArtifact 'burn-bootstrapper-lifecycle-r2/burn-lifecycle-summary.json')
                msix_lifecycle_summary_path = 'artifacts/msix-package-lifecycle-smoke-20260512-0425-0426/summary.json'
                installed_runtime_ops_summary_path = (Join-Path $campaignArtifact 'installed-runtime-ops-summary/summary.json')
            }

        $manifest.steps.Count | Should -Be 1
        $step = $manifest.steps[0]
        $step.id | Should -Be 'manual-admin-campaign-descriptor'
        $step.requires_admin | Should -BeFalse
        $step.mutates_host | Should -BeFalse
        $step.timeout_seconds | Should -Be 300
        $step.arguments | Should -Contain 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'
        $step.arguments | Should -Contain '-PlanOnly'
        $step.arguments | Should -Contain '-BaselineVersion'
        $step.arguments | Should -Contain '0.42.5-admin-smoke'
        $step.arguments | Should -Contain '-TargetVersion'
        $step.arguments | Should -Contain '0.42.6-admin-smoke'
        $step.arguments | Should -Contain '-DescriptorBatchId'
        $step.arguments | Should -Contain 'manual-admin-campaign-descriptor'
        $step.arguments | Should -Contain ([System.IO.Path]::GetFullPath($descriptorArtifact))
        $step.arguments | Should -Contain ([System.IO.Path]::GetFullPath((Join-Path $campaignArtifact 'clean-host/summary.json')))
        ($manifest.steps | ConvertTo-Json -Depth 12) | Should -Not -Match 'msiexec|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Restart-Computer'
    }

    It 'writes the next manual-admin descriptor batch manifest without host mutation steps' {
        $scriptPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1'
        $artifactRoot = Join-Path $TestDrive 'manual-admin-descriptor-batch'
        $manifestPath = Join-Path $artifactRoot 'manifest.json'

        $scriptPath | Should -Exist

        $result = & $scriptPath `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -ManifestPath $manifestPath `
            -PassThru

        $result.ok | Should -BeTrue
        $result.profile | Should -Be 'ManualAdminCampaignDescriptor'
        $result.host_mutation_performed | Should -BeFalse
        $manifestPath | Should -Exist

        $manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
        $manifest.batch_id | Should -Be 'manual-admin-campaign-descriptor-20260512-0425-0426'
        $manifest.steps.Count | Should -Be 1
        $step = $manifest.steps[0]
        $step.id | Should -Be 'manual-admin-campaign-descriptor'
        $step.requires_admin | Should -BeFalse
        $step.mutates_host | Should -BeFalse
        $step.arguments | Should -Contain 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'
        $step.arguments | Should -Contain '-PlanOnly'
        $step.arguments | Should -Contain '-DescriptorBatchId'
        $step.arguments | Should -Contain 'manual-admin-campaign-descriptor-20260512-0425-0426'
        $step.arguments | Should -Contain '0.42.5-admin-smoke'
        $step.arguments | Should -Contain '0.42.6-admin-smoke'
        ($step.arguments -join ' ') | Should -Match 'manual-admin-campaign-20260512-0425-0426'
        ($manifest.steps | ConvertTo-Json -Depth 12) | Should -Not -Match 'msiexec|Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|New-NetFirewallRule|Restart-Computer'

        $dryRun = Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun
        $dryRun.ok | Should -BeTrue
        $dryRun.total_steps | Should -Be 1
        $dryRun.dry_run | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results') | Should -BeFalse
    }

    It 'builds FullAdminHostMutationGate as ordered route parity then OS gate steps' {
        $routeArtifact = Join-Path $TestDrive 'routeparity-artifact'
        $osArtifact = Join-Path $TestDrive 'os-gate-artifact'
        $batchEvidenceRoot = Join-Path $TestDrive 'batch-runs'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'full-admin-host-mutation-gate' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'batch-full-admin') `
            -Profile FullAdminHostMutationGate `
            -ProfileOptions @{
                version = '0.36.2-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = $routeArtifact
                os_gate_artifact_root = $osArtifact
                lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
                batch_evidence_root = $batchEvidenceRoot
            }

        $manifest.steps.Count | Should -Be 2
        @($manifest.steps | ForEach-Object { $_.id }) | Should -Be @('service-msi-hyperv-admin-smoke', 'os-mutation-gate')
        @($manifest.steps | Where-Object { $_.requires_admin -and $_.mutates_host }).Count | Should -Be 2
        $manifest.steps[0].arguments | Should -Contain '-BatchEvidenceRoot'
        $manifest.steps[0].arguments | Should -Contain ([System.IO.Path]::GetFullPath($batchEvidenceRoot))
        $manifest.steps[1].arguments | Should -Not -Contain '-BatchEvidenceRoot'
    }

    It 'rejects all admin profiles without explicit host mutation allowance' {
        foreach ($profile in @('ServiceMsiHyperVAdminSmoke', 'OsMutationGate', 'FullAdminHostMutationGate')) {
            $routeArtifact = Join-Path $TestDrive "$profile-route"
            $osArtifact = Join-Path $TestDrive "$profile-os"
            $manifest = New-PcvBatchSupervisorManifest `
                -BatchId "$profile-blocked" `
                -RepoRoot $script:RepoRoot `
                -ArtifactRoot (Join-Path $TestDrive "$profile-batch") `
                -Profile $profile `
                -ProfileOptions @{
                    version = '0.36.2-admin-smoke'
                    iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                    routeparity_artifact_root = $routeArtifact
                    os_gate_artifact_root = $osArtifact
                    lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
                }

            {
                Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun -IsAdministrator $true
            } | Should -Throw -ExpectedMessage '*PCV_BATCH_HOST_MUTATION_APPROVAL_REQUIRED*'
        }
    }

    It 'rejects all admin profiles from a non-elevated shell even with host mutation allowance' {
        foreach ($profile in @('ServiceMsiHyperVAdminSmoke', 'OsMutationGate', 'FullAdminHostMutationGate')) {
            $routeArtifact = Join-Path $TestDrive "$profile-route"
            $osArtifact = Join-Path $TestDrive "$profile-os"
            $manifest = New-PcvBatchSupervisorManifest `
                -BatchId "$profile-admin-blocked" `
                -RepoRoot $script:RepoRoot `
                -ArtifactRoot (Join-Path $TestDrive "$profile-admin-batch") `
                -Profile $profile `
                -ProfileOptions @{
                    version = '0.36.2-admin-smoke'
                    iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                    routeparity_artifact_root = $routeArtifact
                    os_gate_artifact_root = $osArtifact
                    lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
                }

            {
                Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun -AllowHostMutation -IsAdministrator $false
            } | Should -Throw -ExpectedMessage '*PCV_BATCH_ADMIN_REQUIRED*'
        }
    }

    It 'dry-runs an admin profile with explicit approval without creating step results' {
        $artifactRoot = Join-Path $TestDrive 'admin-profile-dry-run'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'admin-profile-dry-run' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Profile OsMutationGate `
            -ProfileOptions @{
                version = '0.36.2-admin-smoke'
                routeparity_artifact_root = (Join-Path $TestDrive 'routeparity-artifact')
                os_gate_artifact_root = (Join-Path $TestDrive 'os-gate-artifact')
                lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
            }

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun -AllowHostMutation -IsAdministrator $true

        $result.ok | Should -BeTrue
        $result.dry_run | Should -BeTrue
        $result.total_steps | Should -Be 1
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.dry-run.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results') | Should -BeFalse
    }

    It 'keeps generated admin profile arguments free of reboot and scheduled-task commands' {
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'admin-profile-command-denylist' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'admin-profile-command-denylist') `
            -Profile FullAdminHostMutationGate `
            -ProfileOptions @{
                version = '0.36.2-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = (Join-Path $TestDrive 'routeparity-artifact')
                os_gate_artifact_root = (Join-Path $TestDrive 'os-gate-artifact')
                lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
            }

        ($manifest.steps | ConvertTo-Json -Depth 12) |
            Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|Register-ScheduledTask'
    }

    It 'retries a failed step when retry_count is configured and preserves each attempt' {
        $artifactRoot = Join-Path $TestDrive 'batch-retry'
        $marker = Join-Path $TestDrive 'attempt-count.txt'
        $markerLiteral = $marker.Replace("'", "''")
        $command = @"
`$path = '$markerLiteral'
`$count = 0
if (Test-Path -LiteralPath `$path) {
    `$count = [int](Get-Content -LiteralPath `$path -Raw)
}
`$count++
Set-Content -LiteralPath `$path -Value `$count -NoNewline
if (`$count -lt 2) {
    Write-Error 'transient failure'
    exit 9
}
Write-Output 'recovered'
"@
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'retry-success' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -HeartbeatIntervalSeconds 1 `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'flaky-step' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', $command) `
                    -TimeoutSeconds 30 `
                    -RetryCount 1)
            )

        $script:FakeNow = [datetime]'2026-07-16T00:00:00Z'
        $script:FactoryCount = 0
        $script:ActiveProcess = $null
        $factory = {
            param($StartInfo)
            $script:FactoryCount++
            $exitCode = if ($script:FactoryCount -eq 1) { 9 } else { 0 }
            $stdout = if ($exitCode -eq 0) { 'recovered' } else { '' }
            $stderr = if ($exitCode -eq 0) { '' } else { 'transient failure' }
            $script:ActiveProcess = New-PcvFakeBatchProcess -ExitCode $exitCode -Stdout $stdout -Stderr $stderr
            $script:ActiveProcess.StartInfo = $StartInfo
            $script:ActiveProcess
        }
        $now = { $script:FakeNow }
        $wait = {
            param([int]$Milliseconds)
            $script:FakeNow = $script:FakeNow.AddMilliseconds($Milliseconds)
            $script:ActiveProcess.HasExited = $true
        }

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -ProcessFactory $factory -NowProvider $now -WaitAction $wait

        $result.ok | Should -BeTrue
        $result.results.Count | Should -Be 1
        $result.results[0].retry_count | Should -Be 1
        $result.results[0].attempt_count | Should -Be 2
        $result.results[0].final_attempt | Should -Be 2
        $result.results[0].attempts[0].ok | Should -BeFalse
        $result.results[0].attempts[0].exit_code | Should -Be 9
        $result.results[0].attempts[1].ok | Should -BeTrue
        $result.results[0].attempts[1].exit_code | Should -Be 0
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-flaky-step.attempt-01.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-flaky-step.attempt-02.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-flaky-step.json') | Should -BeTrue
        Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'heartbeat.jsonl') | Should -Match '"status":"retrying"'
    }

    It 'fails after retry_count is exhausted and points resume at the failed step' {
        $artifactRoot = Join-Path $TestDrive 'batch-retry-exhausted'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'retry-exhausted' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'always-fails' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Error "hard failure"; exit 7') `
                    -TimeoutSeconds 30 `
                    -RetryCount 1)
            )

        $script:FakeNow = [datetime]'2026-07-16T00:00:00Z'
        $script:ActiveProcess = $null
        $factory = {
            param($StartInfo)
            $script:ActiveProcess = New-PcvFakeBatchProcess -ExitCode 7 -Stderr 'hard failure'
            $script:ActiveProcess.StartInfo = $StartInfo
            $script:ActiveProcess
        }
        $now = { $script:FakeNow }
        $wait = {
            param([int]$Milliseconds)
            $script:FakeNow = $script:FakeNow.AddMilliseconds($Milliseconds)
            $script:ActiveProcess.HasExited = $true
        }

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -ProcessFactory $factory -NowProvider $now -WaitAction $wait

        $result.ok | Should -BeFalse
        $result.status | Should -Be 'failed'
        $result.failed_step_id | Should -Be 'always-fails'
        $result.next_resume_step_id | Should -Be 'always-fails'
        $result.results[0].retry_count | Should -Be 1
        $result.results[0].attempt_count | Should -Be 2
        $result.results[0].exit_code | Should -Be 7
        $result.results[0].ok | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-always-fails.attempt-01.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-always-fails.attempt-02.json') | Should -BeTrue
        $current = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'current-step.json') | ConvertFrom-Json
        $current.status | Should -Be 'failed'
        $current.failed_step_id | Should -Be 'always-fails'
    }

    It 'sets retry_count defaults for host-mutating admin profile steps' {
        $routeArtifact = Join-Path $TestDrive 'routeparity-artifact'
        $osArtifact = Join-Path $TestDrive 'os-gate-artifact'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'admin-profile-retry-defaults' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'admin-profile-retry-defaults') `
            -Profile FullAdminHostMutationGate `
            -ProfileOptions @{
                version = '0.37.0-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = $routeArtifact
                os_gate_artifact_root = $osArtifact
                lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
            }

        $routeStep = $manifest.steps | Where-Object { $_.id -eq 'service-msi-hyperv-admin-smoke' } | Select-Object -First 1
        $osStep = $manifest.steps | Where-Object { $_.id -eq 'os-mutation-gate' } | Select-Object -First 1
        $routeStep.retry_count | Should -Be 1
        $osStep.retry_count | Should -Be 0

        $override = New-PcvBatchSupervisorManifest `
            -BatchId 'admin-profile-retry-overrides' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'admin-profile-retry-overrides') `
            -Profile FullAdminHostMutationGate `
            -ProfileOptions @{
                version = '0.37.0-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = $routeArtifact
                os_gate_artifact_root = $osArtifact
                lan_prefix = 'http://192.168.1.17:7777/' # public-safety: synthetic-rfc1918
                service_msi_hyperv_retry_count = 2
                os_gate_retry_count = 1
            }

        ($override.steps | Where-Object { $_.id -eq 'service-msi-hyperv-admin-smoke' } | Select-Object -First 1).retry_count | Should -Be 2
        ($override.steps | Where-Object { $_.id -eq 'os-mutation-gate' } | Select-Object -First 1).retry_count | Should -Be 1
    }

    It 'requires profile options for admin profiles' {
        {
            New-PcvBatchSupervisorManifest `
                -BatchId 'missing-profile-options' `
                -RepoRoot $script:RepoRoot `
                -ArtifactRoot (Join-Path $TestDrive 'missing-profile-options') `
                -Profile OsMutationGate `
                -ProfileOptions @{
                    version = '0.36.2-admin-smoke'
                }
        } | Should -Throw -ExpectedMessage '*PCV_BATCH_PROFILE_OPTION_REQUIRED*'
    }
}
