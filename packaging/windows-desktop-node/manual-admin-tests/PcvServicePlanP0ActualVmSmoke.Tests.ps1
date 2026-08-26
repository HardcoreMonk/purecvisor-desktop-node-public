Set-StrictMode -Version Latest

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
}
