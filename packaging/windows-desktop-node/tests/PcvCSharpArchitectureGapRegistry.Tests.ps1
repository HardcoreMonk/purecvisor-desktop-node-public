Set-StrictMode -Version Latest

Describe 'C# architecture Wave 0 gap registry' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ManifestPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json'
        $script:QualityBaselinePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json'
        $script:RegistryPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md'
        $script:Manifest = Get-Content -LiteralPath $script:ManifestPath -Raw | ConvertFrom-Json -Depth 100
        $script:QualityBaseline = Get-Content -LiteralPath $script:QualityBaselinePath -Raw | ConvertFrom-Json -Depth 100
        $script:Registry = Get-Content -LiteralPath $script:RegistryPath -Raw
        $script:RequiredGapIds = @('W0-FI-01', 'W0-FI-02', 'W0-FI-03', 'W0-FI-04', 'W0-FI-05', 'W0-FI-06')

        function Get-LiveXunitSourceInventory {
            param(
                [Parameter(Mandatory)]
                [string]$Path
            )

            $source = Get-Content -LiteralPath $Path -Raw
            $attributedMethodPattern = '(?ms)(?<attributes>(?:^[ \t]*\[[^\r\n]+\][ \t]*\r?\n)+)^[ \t]*public[ \t]+(?:(?:async[ \t]+)?(?:void|Task(?:<[^>\r\n]+>)?))[ \t]+(?<name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*\('
            $attributedMethods = @([regex]::Matches($source, $attributedMethodPattern) |
                Where-Object { $_.Groups['attributes'].Value -match '(?m)^[ \t]*\[(?:Fact|Theory)\][ \t]*\r?$' })
            $testMethodNames = @($attributedMethods | ForEach-Object { $_.Groups['name'].Value })
            $methodCaseCounts = @{}
            foreach ($method in $attributedMethods) {
                $attributes = $method.Groups['attributes'].Value
                $methodName = $method.Groups['name'].Value
                $methodCaseCounts[$methodName] = if ($attributes -match '(?m)^[ \t]*\[Fact\][ \t]*\r?$') {
                    1
                }
                else {
                    [regex]::Matches($attributes, '(?m)^[ \t]*\[InlineData\(').Count
                }
            }

            [pscustomobject]@{
                test_method_names = $testMethodNames
                method_case_counts = $methodCaseCounts
                fact_count = [regex]::Matches($source, '(?m)^[ \t]*\[Fact\][ \t]*\r?$').Count
                theory_count = [regex]::Matches($source, '(?m)^[ \t]*\[Theory\][ \t]*\r?$').Count
                inline_data_count = [regex]::Matches($source, '(?m)^[ \t]*\[InlineData\(').Count
                case_count =
                    [regex]::Matches($source, '(?m)^[ \t]*\[Fact\][ \t]*\r?$').Count +
                    [regex]::Matches($source, '(?m)^[ \t]*\[InlineData\(').Count
            }
        }
    }

    It 'publishes a parseable versioned manifest with non-mutating audit boundaries' {
        $script:ManifestPath | Should -Exist
        $script:RegistryPath | Should -Exist
        $script:Manifest.schema_version | Should -Be 'pcv-dotnet-test-migrations/v1'
        $script:Manifest.document_schema_version | Should -Be 1
        $script:Manifest.manifest_id | Should -Be 'csharp-architecture-test-migration-v1'
        $script:Manifest.audit_base_commit | Should -Be '2e98ff4f2df250c36700e86ace0db46ef0aca420'
        $script:Manifest.quality_baseline_path | Should -Be 'packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json'
        $script:Manifest.quality_baseline_source_snapshot_field | Should -Be 'source_snapshot'
        $script:QualityBaseline.audit_base_commit | Should -Be $script:Manifest.audit_base_commit
        $script:QualityBaseline.source_snapshot.schema_version | Should -Be 'pcv-dotnet-source-snapshot/v1'
        $script:QualityBaseline.source_snapshot.algorithm | Should -Be 'sha256'
        $script:QualityBaseline.source_snapshot.sha256 | Should -Match '^[0-9a-f]{64}$'
        $script:Manifest.host_mutation_performed | Should -BeFalse
        $script:Manifest.public_trusted_signing | Should -BeFalse
        $script:Manifest.external_stable_publication | Should -BeFalse
    }

    It 'contains exactly the six mandatory fault scenarios and links each one from the registry' {
        @($script:Manifest.required_fault_gap_ids) | Should -Be $script:RequiredGapIds
        $actualGapIds = @($script:Manifest.fault_scenarios | ForEach-Object { $_.gap_id })
        $actualGapIds | Should -Be $script:RequiredGapIds

        foreach ($gapId in $script:RequiredGapIds) {
            $script:Registry | Should -Match ([regex]::Escape($gapId))
        }
    }

    It 'records reproduction trace expected safety owner RED GREEN and characterization closure for every fault' {
        foreach ($scenario in @($script:Manifest.fault_scenarios)) {
            $scenario.title | Should -Not -BeNullOrEmpty
            $scenario.current_status | Should -Not -BeNullOrEmpty
            $scenario.reproduction_condition | Should -Not -BeNullOrEmpty
            @($scenario.current_trace).Count | Should -BeGreaterThan 0
            @($scenario.existing_evidence).Count | Should -BeGreaterThan 0
            $scenario.expected_safe_result | Should -Not -BeNullOrEmpty
            $scenario.owner_wave | Should -Match '^Wave '
            $scenario.red.owner_project | Should -Not -BeNullOrEmpty
            @($scenario.red.test_ids).Count | Should -BeGreaterThan 0
            $scenario.red.fails_on_baseline_because | Should -Not -BeNullOrEmpty
            $scenario.green.implementation_boundary | Should -Not -BeNullOrEmpty
            @($scenario.green.required_assertions).Count | Should -BeGreaterThan 0
            $scenario.temporary_characterization.status | Should -Not -BeNullOrEmpty
            $scenario.temporary_characterization.replacement_wave | Should -Match '^Wave '
            $scenario.temporary_characterization.replacement_test_id | Should -Not -BeNullOrEmpty
            $scenario.temporary_characterization.removal_condition | Should -Not -BeNullOrEmpty
        }
    }

    It 'keeps migration IDs unique and every migration maps old coverage to an owner and replacement' {
        $migrations = @($script:Manifest.migrations)
        $migrationIds = @($migrations | ForEach-Object { $_.migration_id })

        @($migrationIds | Sort-Object -Unique).Count | Should -Be $migrationIds.Count
        foreach ($migration in $migrations) {
            $migration.status | Should -BeIn @('completed', 'planned')
            $migration.old_test_id | Should -Not -BeNullOrEmpty
            $migration.replacement_test_id | Should -Not -BeNullOrEmpty
            $migration.owner | Should -Not -BeNullOrEmpty
            $migration.coverage_boundary | Should -Not -BeNullOrEmpty
            $migration.reason | Should -Not -BeNullOrEmpty
            $migration.removal_condition | Should -Not -BeNullOrEmpty
        }
    }

    It 'records the completed Hyper-V domain ownership move without losing its 35 cases' {
        $migration = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-HYPERV-DOMAIN-001')
        $migration.Count | Should -Be 1
        $migration[0].status | Should -Be 'completed'
        $migration[0].old_test_id | Should -Be 'DesktopNode.Api.Tests.HyperVDomainContractTests.*'
        $migration[0].replacement_test_id | Should -Be 'DesktopNode.HyperV.Tests.HyperVDomainContractTests.*'
        $migration[0].owner | Should -Be 'DesktopNode.HyperV.Tests'
        $migration[0].case_count_before | Should -Be 35
        $migration[0].case_count_after | Should -Be 39

        $oldPath = Join-Path $script:RepoRoot $migration[0].old_source_path
        $replacementPath = Join-Path $script:RepoRoot $migration[0].replacement_source_path
        $oldPath | Should -Not -Exist
        $replacementPath | Should -Exist

        $replacementLive = Get-LiveXunitSourceInventory -Path $replacementPath
        $replacementLive.case_count | Should -Be $migration[0].case_count_after
        $replacementLive.case_count | Should -Be 39
    }

    It 'matches the current private-reflection and process-global CWD occurrence inventory' {
        $testSources = @(Get-ChildItem -LiteralPath (Join-Path $script:RepoRoot 'src') -Recurse -File -Filter '*.cs' |
            Where-Object { $_.FullName -match '[\\/]DesktopNode\.[^\\/]+\.Tests[\\/]' })

        $privateReflectionMatches = @($testSources |
            Select-String -Pattern 'BindingFlags\.NonPublic')
        $cwdMutationMatches = @($testSources |
            Select-String -Pattern 'Directory\.SetCurrentDirectory\(')

        $privateReflectionMatches.Count | Should -Be $script:Manifest.inventory.private_reflection.current_occurrence_count
        $cwdMutationMatches.Count | Should -Be $script:Manifest.inventory.cwd_mutation.current_occurrence_count
        @($script:Manifest.inventory.private_reflection.entries).Count | Should -Be 0
        @($script:Manifest.inventory.cwd_mutation.entries).Count | Should -Be 1

        $cwdEntry = @($script:Manifest.inventory.cwd_mutation.entries)[0]
        $cwdEntry.test_id | Should -Be 'DesktopNode.Api.Tests.BatchEvidenceSummaryReaderTests.RelativeChildEvidenceIsIgnoredWithoutConfiguredChildRoot'
        $cwdEntry.source_path | Should -Be 'src/DesktopNode.Api.Tests/BatchEvidenceSummaryReaderTests.cs'
        $cwdEntry.occurrence_count | Should -Be 2
        $cwdEntry.named_non_parallel_collection | Should -BeTrue

        $allowlistedCwdSourcePath = (Resolve-Path (Join-Path $script:RepoRoot $cwdEntry.source_path)).Path
        foreach ($cwdMutationMatch in $cwdMutationMatches) {
            [System.IO.Path]::GetFullPath($cwdMutationMatch.Path) | Should -Be $allowlistedCwdSourcePath
        }

        $allowlistedCwdSource = Get-Content -LiteralPath $allowlistedCwdSourcePath -Raw
        $allowlistedCwdSource | Should -Match '\[CollectionDefinition\("Batch evidence CWD isolation",\s*DisableParallelization\s*=\s*true\)\]'
        $allowlistedCwdSource | Should -Match '\[Collection\("Batch evidence CWD isolation"\)\]'
        $allowlistedCwdSource | Should -Match 'Wave 7'

        $cwdMigration = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-BATCH-CWD-006')
        $cwdMigration.Count | Should -Be 1
        $cwdMigration[0].status | Should -Be 'completed'
        $cwdMigration[0].case_count_before | Should -Be 1
        $cwdMigration[0].case_count_after | Should -Be 1

        foreach ($entry in @($script:Manifest.inventory.private_reflection.entries) + @($script:Manifest.inventory.cwd_mutation.entries)) {
            $sourcePath = Join-Path $script:RepoRoot $entry.source_path
            $sourcePath | Should -Exist
            $testMethod = ($entry.test_id -split '\.')[-1]
            (Get-Content -LiteralPath $sourcePath -Raw) | Should -Match ([regex]::Escape($testMethod))
        }
    }

    It 'inventories source-text checks and remaining ownership candidates with migration links' {
        $sourceChecks = @($script:Manifest.inventory.production_source_text_checks.entries)
        $sourceChecks.Count | Should -Be $script:Manifest.inventory.production_source_text_checks.current_test_count
        $sourceChecks.Count | Should -Be 2

        $opsMigration = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-SOURCE-OPS-007')
        $opsMigration.Count | Should -Be 1
        $opsMigration[0].status | Should -Be 'completed'
        $opsMigration[0].replacement_test_id | Should -Be 'DesktopNode.Api.Tests.ApiArchitectureOwnershipTests.OpsSummaryProjectionUsesDedicatedOwner'
        @($sourceChecks.migration_id) | Should -Not -Contain 'TM-SOURCE-OPS-007'

        $migrationIds = @($script:Manifest.migrations | ForEach-Object { $_.migration_id })
        foreach ($entry in $sourceChecks) {
            (Join-Path $script:RepoRoot $entry.source_path) | Should -Exist
            $migrationIds | Should -Contain $entry.migration_id
            foreach ($productPath in @($entry.inspected_product_paths)) {
                (Join-Path $script:RepoRoot $productPath) | Should -Exist
            }
        }

        $ownership = @($script:Manifest.inventory.test_ownership_candidates)
        $ownership.Count | Should -BeGreaterThan 0
        $directHyperV = @($ownership | Where-Object {
            $_.PSObject.Properties.Name -contains 'migration_ids' -and
                @($_.migration_ids) -contains 'TM-HYPERV-DIRECT-NATIVE-002A'
        })
        $directHyperV.Count | Should -Be 1
        @($directHyperV[0].migration_ids) | Should -Be @(
            'TM-HYPERV-DIRECT-NATIVE-002A',
            'TM-HYPERV-DIRECT-WMI-002B'
        )
        foreach ($migrationId in @($directHyperV[0].migration_ids)) {
            $migrationIds | Should -Contain $migrationId
        }

        $nativeMigration = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-HYPERV-DIRECT-NATIVE-002A')
        $wmiMigration = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-HYPERV-DIRECT-WMI-002B')
        $nativeMigration.Count | Should -Be 1
        $wmiMigration.Count | Should -Be 1

        $apiSourcePath = Join-Path $script:RepoRoot $nativeMigration[0].old_source_path
        $nativeSourcePath = Join-Path $script:RepoRoot $nativeMigration[0].replacement_source_path
        $wmiSourcePath = Join-Path $script:RepoRoot $wmiMigration[0].replacement_source_path
        $apiLive = Get-LiveXunitSourceInventory -Path $apiSourcePath
        $nativeLive = Get-LiveXunitSourceInventory -Path $nativeSourcePath
        $wmiLive = Get-LiveXunitSourceInventory -Path $wmiSourcePath

        $oldApiOwnerMethods = @($apiLive.test_method_names | Where-Object {
            $_ -like 'Native*' -or $_ -like 'Wmi*'
        })
        $nativeOwnerMethods = @($nativeLive.test_method_names | Where-Object { $_ -like 'Native*' })
        $wmiOwnerMethods = @($wmiLive.test_method_names | Where-Object { $_ -like 'Wmi*' })

        $oldApiOwnerMethods.Count | Should -Be 0
        $nativeOwnerMethods.Count | Should -Be 42
        $wmiOwnerMethods.Count | Should -Be 25
        $nativeLive.case_count | Should -Be 58
        $wmiLive.case_count | Should -Be 33
        ($nativeLive.case_count + $wmiLive.case_count) | Should -Be $directHyperV[0].observed_case_count
        @($ownership | Where-Object migration_id -EQ 'TM-HYPERV-DOMAIN-001').Count | Should -Be 1
    }

    It 'records the exact Runtime job owner inventory and active durability replacements' {
        $ownership = @($script:Manifest.inventory.test_ownership_candidates)
        $jobOwner = @($ownership | Where-Object owner_wave -EQ 'Wave 1A')
        $jobOwner.Count | Should -Be 1
        $jobOwner[0].status | Should -Be 'completed-additive-runtime-owner'
        $jobOwner[0].current_owner | Should -Be 'DesktopNode.Runtime.Tests'
        $jobOwner[0].integration_companion_owner | Should -Be 'DesktopNode.Api.Tests'
        $jobOwner[0].PSObject.Properties.Name | Should -Not -Contain 'test_id_pattern'

        $methodCount = 0
        $caseCount = 0
        foreach ($ownerSource in @($jobOwner[0].runtime_owner_sources)) {
            $sourcePath = Join-Path $script:RepoRoot $ownerSource.source_path
            $sourcePath | Should -Exist
            $live = Get-LiveXunitSourceInventory -Path $sourcePath
            $live.test_method_names.Count | Should -Be $ownerSource.observed_method_count
            $live.case_count | Should -Be $ownerSource.observed_case_count
            $methodCount += $live.test_method_names.Count
            $caseCount += $live.case_count
        }
        $methodCount | Should -Be $jobOwner[0].observed_method_count
        $caseCount | Should -Be $jobOwner[0].observed_case_count
        $methodCount | Should -Be 57
        $caseCount | Should -Be 79

        $guardMethods = @()
        foreach ($guardPath in @(
            'src/DesktopNode.Api.Tests/ApiArchitectureOwnershipTests.cs',
            'src/DesktopNode.Runtime.Tests/RuntimeArchitectureOwnershipTests.cs'
        )) {
            $guardMethods += (Get-LiveXunitSourceInventory -Path (Join-Path $script:RepoRoot $guardPath)).test_method_names
        }
        foreach ($guardId in @($jobOwner[0].ownership_guard_test_ids)) {
            $guardMethods | Should -Contain (($guardId -split '\.')[-1])
        }
        @($jobOwner[0].ownership_guard_test_ids).Count | Should -Be $jobOwner[0].ownership_guard_case_count
        $jobOwner[0].ownership_guard_case_count | Should -Be 4

        $apiCompanionCases = 0
        foreach ($companion in @($jobOwner[0].retained_api_companions)) {
            $live = Get-LiveXunitSourceInventory -Path (Join-Path $script:RepoRoot $companion.source_path)
            @($companion.test_ids).Count | Should -Be $companion.observed_case_count
            foreach ($testId in @($companion.test_ids)) {
                $live.test_method_names | Should -Contain (($testId -split '\.')[-1])
            }
            $apiCompanionCases += $companion.observed_case_count
        }
        $apiCompanionCases | Should -Be $jobOwner[0].observed_api_companion_case_count
        $apiCompanionCases | Should -Be 23

        $migrationIds = @($script:Manifest.migrations | ForEach-Object migration_id)
        foreach ($migrationId in @($jobOwner[0].future_hardening_migration_ids)) {
            $migrationIds | Should -Contain $migrationId
            @($script:Manifest.migrations | Where-Object migration_id -EQ $migrationId)[0].status | Should -Be 'completed'
        }
        @($script:Manifest.migrations | Where-Object { $_.migration_id -like 'TM-JOB-*' -and $_.status -eq 'completed' }).Count | Should -Be 4

        $createSave = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-JOB-CREATE-SAVE-010')
        $createSave.Count | Should -Be 1
        $createSave[0].status | Should -Be 'completed'
        $createSave[0].old_test_id | Should -Be 'DesktopNode.Runtime.Tests.DesktopNodeJobRuntimeTests.CreateSaveFailurePreservesCurrentPublishBeforeCommitOrder'
        $createSave[0].replacement_test_id | Should -Be 'DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests.CreateSaveFailureDoesNotPublishMemoryOrQueueGhost'
        $createSave[0].case_count_before | Should -Be 1
        $createSave[0].case_count_after | Should -Be 1

        $apiCreateSave = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-API-JOB-CREATE-SAVE-013')
        $apiCreateSave.Count | Should -Be 1
        $apiCreateSave[0].status | Should -Be 'completed'
        $apiCreateSave[0].old_test_id | Should -Be 'DesktopNode.Api.Tests.ApiJobStoreFailureCharacterizationTests.CreateSaveFailureCurrentlyPublishesBeforeDurableCommit'
        $apiCreateSave[0].replacement_test_id | Should -Be 'DesktopNode.Api.Tests.ApiJobStoreFailureCharacterizationTests.CreateSaveFailureDoesNotReturn202OrInvokeNativeMutation'
        $apiCreateSave[0].case_count_before | Should -Be 1
        $apiCreateSave[0].case_count_after | Should -Be 1

        $startSave = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-JOB-START-SAVE-003')
        $startSave.Count | Should -Be 1
        $startSave[0].status | Should -Be 'completed'
        $startSave[0].old_test_id | Should -Be 'DesktopNode.Runtime.Tests.DesktopNodeJobRuntimeTests.StartSaveFailureMarksOnlyMemoryFailedAndDoesNotRegisterCancellation'
        $startSave[0].old_source_path | Should -Be 'src/DesktopNode.Runtime.Tests/DesktopNodeJobRuntimeTests.cs'
        $startSave[0].replacement_test_id | Should -Be 'DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests.StartSaveFailureKeepsRecoverableMeaning'
        $startSave[0].replacement_test_id | Should -Not -Match '\*'
        $startSave[0].case_count_before | Should -Be 1
        $startSave[0].case_count_after | Should -Be 1

        $runningCancel = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-JOB-RUNNING-CANCEL-011')
        $runningCancel.Count | Should -Be 1
        $runningCancel[0].status | Should -Be 'completed'
        $runningCancel[0].replacement_test_id | Should -Be 'DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests.RunningCancelPersistsRequestBeforeProviderSignalOutsideStateLock'

        $nonObject = @($script:Manifest.migrations | Where-Object migration_id -EQ 'TM-JOB-NONOBJECT-012')
        $nonObject.Count | Should -Be 1
        $nonObject[0].status | Should -Be 'completed'
        $nonObject[0].case_count_before | Should -Be 6
        $nonObject[0].case_count_after | Should -Be 7
    }

    It 'links safe W0-FI-01 FI-02 and FI-04 to their live Runtime owners' {
        $runtimeLive = Get-LiveXunitSourceInventory -Path (Join-Path $script:RepoRoot 'src/DesktopNode.Runtime.Tests/DesktopNodeJobRuntimeTests.cs')
        $durabilityLive = Get-LiveXunitSourceInventory -Path (Join-Path $script:RepoRoot 'src/DesktopNode.Runtime.Tests/JobRuntimeDurabilityTests.cs')
        $physicalStoreLive = Get-LiveXunitSourceInventory -Path (Join-Path $script:RepoRoot 'src/DesktopNode.Runtime.Tests/JsonFileDesktopNodeJobStoreTests.cs')

        $fi01 = @($script:Manifest.fault_scenarios | Where-Object gap_id -EQ 'W0-FI-01')[0]
        $fi01.current_status | Should -Be 'safe-create-commit-protocol-with-physical-restart-guard-under-single-product-runtime-owner'
        $fi01.temporary_characterization.status | Should -Be 'replaced-safe-create-commit-protocol'
        $fi01.temporary_characterization.replacement_test_id | Should -Be 'DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests.CreateSaveFailureDoesNotPublishMemoryOrQueueGhost'
        $runtimeLive.test_method_names | Should -Not -Contain 'CreateSaveFailurePreservesCurrentPublishBeforeCommitOrder'
        $durabilityLive.test_method_names | Should -Contain 'CreateSaveFailureDoesNotPublishMemoryOrQueueGhost'
        foreach ($physicalTest in @(
            'WriterFlushesUniqueTempAndPendingGuardBeforeReplacingPrimary',
            'PreReplaceFailureReportsNotCommittedAndPreservesPreviousPrimary',
            'PendingCommitPublicationFailureIsNotCommittedAndLeavesNoAuthoritativeGuard',
            'PostReplaceFailureWithMatchingPrimaryIsReconciledAsCommitted',
            'IndeterminatePostReplaceFailureBlocksDispatchUntilRestartReconcilesPrimary',
            'PendingPreReplaceCommitBlocksUntilRestartConfirmsPreviousPrimary',
            'InvalidPendingCommitBlocksWithoutLoadingOrMutatingPrimary',
            'InaccessiblePrimaryStartsStructuredBlockedWithoutTreatingItAsMissing',
            'StartupNeverPromotesOrphanUniqueTemp',
            'DurableWriterPreserves04265CompatibleV1AndV2Shape'
        )) {
            $physicalStoreLive.test_method_names | Should -Contain $physicalTest
        }
        (@($fi01.current_trace) -join ' ') | Should -Match 'src/DesktopNode.Runtime/DesktopNodeJobRuntime.cs'
        (@($fi01.current_trace) -join ' ') | Should -Match 'src/DesktopNode.Runtime/JsonFileDesktopNodeJobStore.cs'

        $fi02 = @($script:Manifest.fault_scenarios | Where-Object gap_id -EQ 'W0-FI-02')[0]
        $fi02.current_status | Should -Be 'safe-persist-before-publish-with-terminal-uncertainty-block'
        $fi02.temporary_characterization.status | Should -Be 'replaced-safe-transition-commit-protocol'
        $fi02.temporary_characterization.test_id | Should -Be 'DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests.StartSaveFailureKeepsRecoverableMeaning'
        @($fi02.temporary_characterization.additional_test_ids) | Should -Contain 'DesktopNode.Runtime.Tests.JobRuntimeDurabilityTests.RunningCancelPersistsRequestBeforeProviderSignalOutsideStateLock'
        $runtimeLive.test_method_names | Should -Not -Contain 'StartSaveFailureMarksOnlyMemoryFailedAndDoesNotRegisterCancellation'
        $runtimeLive.test_method_names | Should -Not -Contain 'RunningCancelSignalsBeforeSavingCurrentCancellationRequest'
        $durabilityLive.test_method_names | Should -Contain 'StartSaveFailureKeepsRecoverableMeaning'
        $durabilityLive.test_method_names | Should -Contain 'TransitionSaveFailureKeepsRecoverableMeaning'
        $durabilityLive.test_method_names | Should -Contain 'RunningCancelPersistsRequestBeforeProviderSignalOutsideStateLock'

        $fi04 = @($script:Manifest.fault_scenarios | Where-Object gap_id -EQ 'W0-FI-04')[0]
        $fi04.current_status | Should -Be 'safe-typed-semantic-block-without-rewrite'
        $fi04.temporary_characterization.status | Should -Be 'replaced-safe-typed-load-result'
        $fi04.temporary_characterization.test_id | Should -Be 'DesktopNode.Runtime.Tests.DesktopNodeJobRuntimeTests.MalformedOrNonObjectRootStartsInStructuredBlockedState'
        $fi04.temporary_characterization.case_count | Should -Be 7
        $runtimeLive.method_case_counts['MalformedOrNonObjectRootStartsInStructuredBlockedState'] | Should -Be 7
        $runtimeLive.test_method_names | Should -Not -Contain 'ValidNonObjectRootPreservesCurrentUnstructuredStartupFailure'
        (@($fi04.current_trace) -join ' ') | Should -Match 'src/DesktopNode.Runtime/DesktopNodeJobRuntime.cs'
    }

    It 'detects reintroduced Native and Wmi tests from live source instead of a baseline snapshot' {
        $fixturePath = Join-Path $TestDrive 'ApiRuntimePolicyRequestProcessorTests.cs'
        $fixtureSource = @'
public sealed class ApiRuntimePolicyRequestProcessorTests
{
    [Fact]
    public void NativeAdapterReturnsToTheOldOwner() { }

    [Fact]
    public void WmiProviderReturnsToTheOldOwner() { }

    [Fact]
    public void ApiRouteRemainsWithTheApiOwner() { }
}
'@
        $fixtureSource = $fixtureSource -replace "`r?`n", "`r`n"
        Set-Content -LiteralPath $fixturePath -Value $fixtureSource -Encoding utf8 -NoNewline

        $live = Get-LiveXunitSourceInventory -Path $fixturePath
        $oldOwnerMethods = @($live.test_method_names | Where-Object {
            $_ -like 'Native*' -or $_ -like 'Wmi*'
        })

        $oldOwnerMethods | Should -Be @(
            'NativeAdapterReturnsToTheOldOwner',
            'WmiProviderReturnsToTheOldOwner'
        )
        $live.case_count | Should -Be 3
    }
}
