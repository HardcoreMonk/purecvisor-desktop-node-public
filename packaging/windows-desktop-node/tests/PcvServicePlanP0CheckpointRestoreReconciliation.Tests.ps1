Set-StrictMode -Version Latest

Describe 'Service Plan P0-2 checkpoint.restore reconciliation' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:FixturePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/service-plan-p0-checkpoint-restore-reconciliation.json'
        $script:Fixture = Get-Content -LiteralPath $script:FixturePath -Raw | ConvertFrom-Json -Depth 100
        $script:Wave2CFixturePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json'
        $script:Wave2CFixture = Get-Content -LiteralPath $script:Wave2CFixturePath -Raw | ConvertFrom-Json -Depth 100
    }

    It 'publishes a code-level, read-only-baseline, non-host-mutating restore decision fixture' {
        $script:Fixture.schema_version | Should -Be 'pcv-checkpoint-restore-reconciliation/v1'
        $script:Fixture.wave | Should -Be 'P0-2'
        $script:Fixture.operation | Should -Be 'checkpoint.restore'
        $script:Fixture.status | Should -Be 'code_complete'
        $script:Fixture.implementation_change | Should -BeTrue
        $script:Fixture.product_behavior_changed | Should -BeTrue
        $script:Fixture.host_mutation_performed | Should -BeFalse
        $script:Fixture.hyperv_mutation_performed | Should -BeFalse
        $script:Fixture.actual_vm_validation_performed | Should -BeFalse
        $script:Fixture.package_candidate_created | Should -BeFalse
        $script:Fixture.promotion_not_triggered | Should -BeTrue
        $script:Fixture.current_operational_anchor | Should -Be '0.42.73-admin-smoke'
    }

    It 'requires current=true postcondition and fails closed for presence-only or unreadable current' {
        $script:Fixture.baseline_capture.source | Should -Be 'checkpoint.list'
        $script:Fixture.baseline_capture.read_only | Should -BeTrue
        @($script:Fixture.baseline_capture.capture_statuses) | Should -Contain 'unavailable'
        $script:Fixture.success_requires.requested_row_count | Should -Be 1
        $script:Fixture.success_requires.is_current | Should -BeTrue
        $script:Fixture.success_requires.presence_only | Should -BeFalse
        @($script:Fixture.reconcile_classifications) | Should -Contain 'not-applied'
        @($script:Fixture.reconcile_classifications) | Should -Contain 'current-unavailable'
        @($script:Fixture.reconcile_classifications) | Should -Contain 'ambiguous-duplicate-checkpoint-names'
        @($script:Fixture.reconcile_classifications) | Should -Contain 'readback-unavailable'
        @($script:Fixture.reconcile_classifications) | Should -Contain 'baseline-unavailable'
        $script:Fixture.required_manual_mapping.http_status | Should -Be 409
        $script:Fixture.required_manual_mapping.error_code | Should -Be 'PCV_JOB_RECONCILIATION_REQUIRED'
        $script:Fixture.required_manual_mapping.job_status_unchanged | Should -Be 'failed'
        $script:Fixture.required_manual_mapping.retryable | Should -BeFalse
        $script:Fixture.confirmed_terminal_mapping.provider_mutation_called | Should -BeFalse
        @($script:Fixture.confirmed_terminal_mapping.provider_operations) | Should -Be @('checkpoint.list')
    }

    It 'pins additive route and restore operator parity without a new HTTP route' {
        $script:Fixture.route.template | Should -Be '/api/v1/jobs/{jobId}/reconcile'
        $script:Fixture.route.operation_name | Should -Be 'ReconcileJob'
        $script:Fixture.route.required_permission | Should -Be 'operate'
        $script:Fixture.route.mutation_stance | Should -Be 'ProductOperation'
        $script:Fixture.operator_surface_parity.web_action | Should -Be 'Reconcile restore'
        $script:Fixture.operator_surface_parity.cli_command | Should -Be 'job reconcile <job_id>'
        $script:Fixture.operator_surface_parity.rbac_permission | Should -Be 'operate'
        @($script:Fixture.excluded_operations) | Should -Not -Contain 'checkpoint.restore'
        @($script:Fixture.excluded_operations) | Should -Contain 'checkpoint.delete'
    }

    It 'leaves the Wave 2C create fixture excluding restore' {
        @($script:Wave2CFixture.excluded_operations) | Should -Contain 'checkpoint.restore'
        $script:Wave2CFixture.operation | Should -Be 'checkpoint.create'
        $script:Wave2CFixture.schema_version | Should -Be 'pcv-checkpoint-create-reconciliation/v1'
    }

    It 'keeps actual VM smoke and package promotion out of this code slice' {
        $script:Fixture.verification_scope.actual_vm_smoke | Should -Be 'NOT_RUN_BY_DESIGN'
        $script:Fixture.verification_scope.package_build | Should -Be 'NOT_RUN_BY_DESIGN'
        $script:Fixture.public_boundary.public_trusted_signing | Should -BeFalse
        $script:Fixture.public_boundary.external_stable_publication | Should -BeFalse
    }
}
