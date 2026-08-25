Set-StrictMode -Version Latest

Describe 'C# architecture Wave 2C checkpoint.create reconciliation' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:FixturePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json'
        $script:Fixture = Get-Content -LiteralPath $script:FixturePath -Raw | ConvertFrom-Json -Depth 100
    }

    It 'publishes a code-level, read-only-baseline, non-host-mutating decision fixture' {
        $script:Fixture.schema_version | Should -Be 'pcv-checkpoint-create-reconciliation/v1'
        $script:Fixture.wave | Should -Be 'Wave 2C'
        $script:Fixture.operation | Should -Be 'checkpoint.create'
        $script:Fixture.status | Should -Be 'code_complete'
        $script:Fixture.implementation_change | Should -BeTrue
        $script:Fixture.product_behavior_changed | Should -BeTrue
        $script:Fixture.host_mutation_performed | Should -BeFalse
        $script:Fixture.hyperv_mutation_performed | Should -BeFalse
        $script:Fixture.actual_vm_validation_performed | Should -BeFalse
        $script:Fixture.package_candidate_created | Should -BeFalse
        $script:Fixture.promotion_not_triggered | Should -BeTrue
        $script:Fixture.current_operational_anchor | Should -Be '0.42.65-admin-smoke'
    }

    It 'requires absent pre-state and fails closed for existing or ambiguous rows' {
        $script:Fixture.baseline_capture.source | Should -Be 'checkpoint.list'
        $script:Fixture.baseline_capture.read_only | Should -BeTrue
        @($script:Fixture.baseline_capture.capture_statuses) | Should -Contain 'unavailable'
        @($script:Fixture.reconcile_classifications) | Should -Contain 'not-applied'
        @($script:Fixture.reconcile_classifications) | Should -Contain 'ambiguous-duplicate-checkpoint-names'
        $script:Fixture.required_manual_mapping.http_status | Should -Be 409
        $script:Fixture.required_manual_mapping.error_code | Should -Be 'PCV_JOB_RECONCILIATION_REQUIRED'
        $script:Fixture.required_manual_mapping.job_status_unchanged | Should -Be 'failed'
    }

    It 'pins additive route and operator parity without enabling restore reconciliation' {
        $script:Fixture.route.template | Should -Be '/api/v1/jobs/{jobId}/reconcile'
        $script:Fixture.route.operation_name | Should -Be 'ReconcileJob'
        $script:Fixture.route.required_permission | Should -Be 'operate'
        $script:Fixture.route.mutation_stance | Should -Be 'ProductOperation'
        $script:Fixture.operator_surface_parity.web_action | Should -Be 'Reconcile checkpoint'
        $script:Fixture.operator_surface_parity.cli_command | Should -Be 'job reconcile <job_id>'
        $script:Fixture.operator_surface_parity.rbac_permission | Should -Be 'operate'
        @($script:Fixture.excluded_operations) | Should -Contain 'checkpoint.restore'
    }

    It 'keeps actual VM smoke and package promotion out of this code slice' {
        $script:Fixture.verification_scope.actual_vm_smoke | Should -Be 'NOT_RUN_BY_DESIGN'
        $script:Fixture.verification_scope.package_build | Should -Be 'NOT_RUN_BY_DESIGN'
        $script:Fixture.public_boundary.public_trusted_signing | Should -BeFalse
        $script:Fixture.public_boundary.external_stable_publication | Should -BeFalse
    }
}
