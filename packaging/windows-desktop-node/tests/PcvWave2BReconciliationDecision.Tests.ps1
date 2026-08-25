Set-StrictMode -Version Latest

Describe 'C# architecture Wave 2B operation reconciliation decision' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:FixturePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2b-reconciliation.json'
        $script:Fixture = Get-Content -LiteralPath $script:FixturePath -Raw | ConvertFrom-Json -Depth 100
        $script:ExpectedMutationOperations = @(
            'vm.create', 'vm.start', 'vm.shutdown', 'vm.poweroff', 'vm.restart', 'vm.pause', 'vm.resume',
            'vm.rename', 'vm.eject', 'vm.limit', 'vm.set-memory', 'vm.set-vcpu', 'vm.disk-resize',
            'vm.qos.storage.set', 'vm.qos.network.set', 'vm.guest.exec', 'vm.guest.channel.verify',
            'vm.guest.channel.ensure', 'vm.delete', 'checkpoint.create', 'checkpoint.restore', 'checkpoint.delete'
        )
        $script:RequiredFamilyIds = @('vm-create', 'vm-delete', 'vm-rename', 'vm-qos', 'checkpoint')
    }

    It 'publishes a versioned non-mutating decision fixture' {
        $script:FixturePath | Should -Exist
        $script:Fixture.schema_version | Should -Be 'pcv-job-reconciliation-decision/v1'
        $script:Fixture.decision_id | Should -Be 'wave2b-operation-reconciliation-v1'
        $script:Fixture.wave | Should -Be 'Wave 2B'
        $script:Fixture.status | Should -Be 'code_complete'
        $script:Fixture.implementation_change | Should -BeFalse
        $script:Fixture.product_behavior_changed | Should -BeFalse
        $script:Fixture.host_mutation_performed | Should -BeFalse
        $script:Fixture.hyperv_mutation_performed | Should -BeFalse
        $script:Fixture.actual_vm_validation_performed | Should -BeFalse
        $script:Fixture.public_trusted_signing | Should -BeFalse
        $script:Fixture.external_stable_publication | Should -BeFalse
    }

    It 'covers every one of the 22 current mutation operations exactly once' {
        $actual = @($script:Fixture.operation_families | ForEach-Object { $_.operations } | ForEach-Object { $_ })
        $actual.Count | Should -Be 22
        @($actual | Sort-Object -Unique).Count | Should -Be 22
        foreach ($operation in $script:ExpectedMutationOperations) {
            $actual | Should -Contain $operation
        }
    }

    It 'requires expected state readback timeout and operator action for every family' {
        foreach ($family in @($script:Fixture.operation_families)) {
            $family.family_id | Should -Not -BeNullOrEmpty
            @($family.operations).Count | Should -BeGreaterThan 0
            @($family.expected_before).Count | Should -BeGreaterThan 0
            @($family.expected_after).Count | Should -BeGreaterThan 0
            $family.idempotency_basis | Should -Not -BeNullOrEmpty
            @($family.readback_sources).Count | Should -BeGreaterThan 0
            $family.readback_sufficient | Should -Not -BeNullOrEmpty
            $family.automatic_reconciliation | Should -Not -BeNullOrEmpty
            $family.timeout_policy | Should -Not -BeNullOrEmpty
            $family.operator_action | Should -Not -BeNullOrEmpty
            @($family.future_2c_gate).Count | Should -BeGreaterThan 0
        }
    }

    It 'keeps the required create delete rename QoS and checkpoint families explicit' {
        $familyIds = @($script:Fixture.operation_families | ForEach-Object family_id)
        foreach ($familyId in $script:RequiredFamilyIds) {
            $familyIds | Should -Contain $familyId
        }

        $qos = @($script:Fixture.operation_families | Where-Object family_id -EQ 'vm-qos')[0]
        @($qos.operations) | Should -Be @('vm.qos.storage.set', 'vm.qos.network.set')
        @($qos.readback_sources) | Should -Contain 'vm.blkio-get'
        @($qos.readback_sources) | Should -Contain 'vm.bandwidth'
        $qos.readback_sufficient | Should -Be 'false-for-persisted-running'
        $qos.automatic_reconciliation | Should -Be 'disabled'
    }

    It 'keeps persisted-running recovery fail-closed and Guest Execution outside Wave 2B' {
        $recovery = $script:Fixture.running_recovery
        $recovery.current_projection_status | Should -Be 'failed'
        $recovery.error_code | Should -Be 'PCV_JOB_INTERRUPTED'
        $recovery.retryable | Should -BeFalse
        $recovery.automatic_retry | Should -BeFalse

        $guest = @($script:Fixture.operation_families | Where-Object family_id -EQ 'guest-execution')[0]
        $guest.reconciliation_class | Should -Be 'deferred-excluded'
        $guest.automatic_reconciliation | Should -Be 'excluded-from-wave-2b'
        $guest.timeout_policy | Should -Match 'ADR-0009'
    }

    It 'does not introduce a new timeout or a public reconciliation error contract' {
        $script:Fixture.timeout_policy.route_timeout_default_seconds | Should -Be 30
        @($script:Fixture.timeout_policy.route_timeout_configured_range_seconds) | Should -Be @(1, 3600)
        $script:Fixture.timeout_policy.new_reconciliation_timeout_introduced | Should -BeFalse
        $json = Get-Content -LiteralPath $script:FixturePath -Raw
        $json | Should -Not -Match 'PCV_JOB_RECONCILIATION_REQUIRED'
    }
}
