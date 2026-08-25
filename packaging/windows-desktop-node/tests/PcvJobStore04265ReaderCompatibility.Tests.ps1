Set-StrictMode -Version Latest

$readerCompatibilityRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$readerCompatibilityFrozenHostPath = Join-Path $readerCompatibilityRepoRoot 'artifacts/admin-smoke-package-20260716-04265/host-publish/DesktopNode.Host.exe'
$readerCompatibilityCanRunFrozenHost = $IsWindows -and (
    Test-Path -LiteralPath $readerCompatibilityFrozenHostPath -PathType Leaf)

Describe 'Frozen 0.42.65 job-store reader compatibility runner' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:RunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvJobStore04265ReaderCompatibility.ps1'
        $script:WriterProgramPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/fixtures/PcvJobStoreFixtureWriter/Program.cs'
        $script:FrozenHostPath = Join-Path $script:RepoRoot 'artifacts/admin-smoke-package-20260716-04265/host-publish/DesktopNode.Host.exe'
    }

    It 'pins the frozen host by artifact path, SHA-256, and exact ProductVersion' {
        $script:RunnerPath | Should -Exist
        $content = Get-Content -Raw -LiteralPath $script:RunnerPath

        $content | Should -Match ([regex]::Escape('artifacts/admin-smoke-package-20260716-04265/host-publish/DesktopNode.Host.exe'))
        $content | Should -Match '95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83'
        $content | Should -Match ([regex]::Escape('0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876'))
        $content | Should -Match 'PCV_04265_READER_FROZEN_HOST_HASH_MISMATCH'
        $content | Should -Match 'PCV_04265_READER_FROZEN_HOST_VERSION_MISMATCH'
        $script:WriterProgramPath | Should -Exist
        $writerContent = Get-Content -Raw -LiteralPath $script:WriterProgramPath
        ($content + $writerContent) | Should -Not -Match 'PCV_JOB_CANCELLED'
    }

    It 'keeps the runner isolated from service, installer, admin, and Hyper-V mutation commands' {
        $content = Get-Content -Raw -LiteralPath $script:RunnerPath

        $content | Should -Match "request_scope\s*=\s*'GET /api/v1/jobs only'"
        $content | Should -Match 'native_operation_requests\s*=\s*0'
        $content | Should -Match 'host_mutation_performed\s*=\s*\$false'
        $content | Should -Not -Match 'msiexec|Start-Service|Stop-Service|Restart-Service|New-Service|Remove-Service|sc\.exe|Get-VM|New-VM|Set-VM|Remove-VM|Checkpoint-VM|Restore-VMSnapshot|New-NetFirewallRule|Remove-NetFirewallRule|netsh\s+http'
    }

    It 'dry-runs current-writer terminal and FIFO queue schemas plus backup restore without a listener' `
        -Skip:(-not $readerCompatibilityCanRunFrozenHost) {
        $artifactRoot = Join-Path $TestDrive 'reader-compatibility-dryrun'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:RunnerPath `
            -ArtifactRoot $artifactRoot `
            -FrozenHostPath $script:FrozenHostPath `
            -DryRun | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.ok | Should -BeTrue
        $summary.actual_execution | Should -Be 'dry-run-pinned-binary-no-listener'
        $summary.frozen_host.pin_verified | Should -BeTrue
        $summary.frozen_host.observed_sha256 | Should -Be '95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83'
        $summary.frozen_host.observed_product_version | Should -Be '0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876'
        $summary.fixture_plans.Count | Should -Be 2
        @($summary.fixture_plans.schema_version) | Should -Be @(1, 2)
        @($summary.fixture_plans | ForEach-Object { $_.generated_by_current_writer }) | Should -Not -Contain $false
        @($summary.fixture_plans | ForEach-Object { $_.queue_count }) | Should -Be @(2, 2)
        @($summary.fixture_plans | ForEach-Object { $_.passes_planned.Count }) | Should -Be @(4, 4)
        $summary.current_writer.manual_snapshot_assembly | Should -BeFalse
        $summary.backup_restore_planned | Should -BeTrue
        $summary.admin_required | Should -BeFalse
        $summary.host_mutation_performed | Should -BeFalse
    }

    It 'rejects an unpinned host before launching a listener' {
        $artifactRoot = Join-Path $TestDrive 'reader-compatibility-bad-host'
        $fakeHost = Join-Path $TestDrive 'DesktopNode.Host.exe'
        [IO.File]::WriteAllText($fakeHost, 'not-the-frozen-host', [Text.UTF8Encoding]::new($false))

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:RunnerPath `
            -ArtifactRoot $artifactRoot `
            -FrozenHostPath $fakeHost `
            -DryRun | Out-Null
        $LASTEXITCODE | Should -Be 1

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.ok | Should -BeFalse
        $summary.error.message | Should -Match '^PCV_04265_READER_FROZEN_HOST_HASH_MISMATCH\|'
        $summary.host_mutation_performed | Should -BeFalse
    }

    It 'reads current-writer v1/v2 terminal and FIFO queue stores before and after restore without changing bytes' `
        -Skip:(-not $readerCompatibilityCanRunFrozenHost) {
        $artifactRoot = Join-Path $TestDrive 'reader-compatibility-actual'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:RunnerPath `
            -ArtifactRoot $artifactRoot `
            -FrozenHostPath $script:FrozenHostPath `
            -StartupTimeoutSeconds 30 | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.ok | Should -BeTrue
        $summary.actual_execution | Should -Be 'frozen-04265-binary-high-loopback-reader'
        $summary.frozen_host.pin_verified | Should -BeTrue
        $summary.frozen_host.observed_product_version | Should -Be '0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876'
        $summary.pass_count | Should -Be 8
        $summary.backup_restore_performed | Should -BeTrue
        $summary.jobs_json_hash_unchanged | Should -BeTrue
        $summary.native_operation_requests | Should -Be 0
        $summary.hyperv_routes_invoked | Should -BeFalse
        $summary.service_mutation_performed | Should -BeFalse
        $summary.admin_required | Should -BeFalse
        $summary.host_mutation_performed | Should -BeFalse
        $summary.request_scope | Should -Be 'GET /api/v1/jobs only'

        @($summary.scenarios.schema_version) | Should -Be @(1, 2)
        foreach ($scenario in @($summary.scenarios)) {
            $scenario.ok | Should -BeTrue
            $scenario.generated_by_current_writer | Should -BeTrue
            $scenario.terminal_only | Should -BeFalse
            $scenario.queue_count | Should -Be 2
            $scenario.job_count | Should -Be 3
            $scenario.queue_fifo_observed | Should -BeTrue
            $scenario.backup_restore_performed | Should -BeTrue
            $scenario.jobs_json_hash_unchanged | Should -BeTrue
            $scenario.terminal_source_sha256 | Should -Be $scenario.terminal_backup_sha256
            $scenario.terminal_source_sha256 | Should -Be $scenario.terminal_restored_sha256
            $scenario.terminal_source_sha256 | Should -Be $scenario.terminal_final_sha256
            $scenario.queue_source_sha256 | Should -Be $scenario.queue_backup_sha256
            $scenario.queue_source_sha256 | Should -Be $scenario.queue_restored_sha256
            $scenario.queue_source_sha256 | Should -Be $scenario.queue_final_sha256
            $scenario.passes.Count | Should -Be 4

            foreach ($pass in @($scenario.passes)) {
                $pass.ok | Should -BeTrue
                $pass.http_status | Should -Be 200
                $pass.listener_host | Should -Be '127.0.0.1'
                $pass.listener_port | Should -BeGreaterOrEqual 49152
                $pass.request_path | Should -Be '/api/v1/jobs'
                $pass.job_store_hash_unchanged | Should -BeTrue
                $pass.projection.ok | Should -BeTrue
                if ($pass.queue_probe) {
                    $pass.pass_kind | Should -Be 'queue-fifo-readonly-probe'
                    $pass.projection.job_count | Should -Be 2
                    $pass.projection.fifo_selection_observed | Should -BeTrue
                    $pass.projection.provider_dispatch_prevented_by_store_guard | Should -BeTrue
                    $pass.projection.selected_job_error_code | Should -Be 'PCV_JOB_STORE_SAVE_FAILED'
                    $pass.projection.selection_mode | Should -BeIn @(
                        'first-failed-second-queued',
                        'both-failed-in-fifo-timestamp-order')
                    @($pass.projection.attempted_job_ids)[0] | Should -Be $scenario.expected_queue[0]
                }
                else {
                    $pass.pass_kind | Should -Be 'terminal-reader'
                    $pass.projection.job_count | Should -Be 3
                    @($pass.projection.statuses) | Should -Be @('canceled', 'failed', 'succeeded')
                }
                $pass.native_operation_requests | Should -Be 0
                $pass.hyperv_routes_invoked | Should -BeFalse
                $pass.response_path | Should -Exist
            }
        }
    }
}
