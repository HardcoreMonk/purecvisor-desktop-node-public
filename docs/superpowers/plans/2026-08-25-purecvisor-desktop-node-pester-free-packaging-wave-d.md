# Pester-free Packaging Wave D Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development and superpowers:executing-plans to implement this plan task-by-task. Use superpowers:requesting-code-review after each batch. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Packaging legacy Pester 55파일/528계약을 read-only C# delivery 계약으로 1:1 이전하고, current-evidence check를 managed C# handler로 활성화하여 ledger 전체 62파일/627계약을 local parity PASS 상태로 만든다.

**Architecture:** Wave C의 `DesktopNode.Delivery.Tests` metadata/parser/ledger를 그대로 확장한다. 한 legacy 파일은 한 C# fixture owner를 유지하고, 90계약과 61계약 파일은 독립 batch로 격리한다. 공통 JSON/XML/Markdown/source helpers는 첫 실제 소비자와 함께 추가한다. Current evidence verification은 `DesktopNode.Verification`의 write-free managed handler가 canonical JSON, schema invariants, referenced evidence, generated blocks를 직접 비교한다.

**Tech Stack:** C# / .NET 10, xUnit 2.9.3, `System.Text.Json`, `System.Xml.Linq`, regular expressions, Node.js 24 ledger verifier, Pester 5.7.1 reference-only dual run, Visual Studio 2026 또는 `dotnet` CLI.

---

## Preconditions and hard boundaries

- Wave C checkpoint is complete on `codex/pester-free-verification-cutover` in `HardcoreMonk/purecvisor-desktop-node-public`: Installer `49/49` and Web `50/50` local PASS; Packaging `528` unmapped; all CI parity pending.
- The original private archive and its dirty main remain read-only; pushes and PR checks target only the new authority.
- Exact Packaging inventory is 55 files / 528 literal contracts. A count/name/order drift stops the plan before porting.
- Legacy Pester assertion names, counts, execution behavior, and runnability remain unchanged through the
  shadow commit. Approved public-safety-only comments and runtime-built synthetic material in the sanitized
  root are the baseline and are not reverted. This plan does not perform required-CI cutover.
- Replacement code does not spawn PowerShell/Pester or any host mutation executable.
- Static installed-smoke/preflight tests verify descriptor/source/evidence shape only. They do not install, start services, alter firewall/certificates/credentials/Event Log, reboot, or create a VM.
- Each batch has at most eight legacy files; D1 and D3 are intentionally single-file batches.
- Each batch follows metadata RED → typed implementation GREEN → deterministic negative fixture → legacy reference GREEN → full Delivery GREEN → fixed-diff review → commit.
- A contract is not marked local PASS until its C# method and corresponding legacy `It` both pass in the same worktree state.
- `public_trusted_signing=false`, `external_stable_publication=false`, package candidate created `false`, host mutation performed `false` throughout.

## Common file map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Delivery.Tests/Infrastructure/JsonContract.cs` | Strict typed JSON/object/property helpers. |
| `src/DesktopNode.Delivery.Tests/Infrastructure/XmlContract.cs` | Namespace-aware XML/WiX cardinality helpers. |
| `src/DesktopNode.Delivery.Tests/Infrastructure/MarkdownContract.cs` | Ordered heading/table/key-value parsing. |
| `src/DesktopNode.Delivery.Tests/Infrastructure/SourceContract.cs` | Exact token/block/command-array checks without shell execution. |
| `src/DesktopNode.Delivery.Tests/Infrastructure/TemporaryContractTree.cs` | Contained, disposable negative fixtures. |
| `src/DesktopNode.Delivery.Tests/Delivery/DeliveryNegativeParityTests.cs` | Batch-specific mutation fixtures; category `VerificationInfrastructure`. |
| `src/DesktopNode.Verification/CurrentEvidenceVerifier.cs` | Canonical current-evidence read-only validation/projection. |
| `src/DesktopNode.Verification/ManagedSuiteRunner.cs` | Exact managed-handler dispatch; unavailable handlers fail closed. |
| `src/DesktopNode.Verification.Tests/CurrentEvidenceVerifierTests.cs` | Positive/negative current-evidence parity and no-write proof. |
| `src/DesktopNode.Verification.Tests/ManagedSuiteRunnerTests.cs` | Handler dispatch, timeout/cancel, stable failures. |
| `docs/ga-ready/evidence/pester-free-packaging-wave-d-2026-08-25.md` | Batch results, hashes, counts and non-mutation claims. |

## Exact ten-batch ledger

### D1 — canonical admin-smoke evidence, 1 file / 90 contracts

| Legacy file | C# owner |
| --- | --- |
| `PcvAdminSmokeEvidenceDocs.Tests.ps1` | `Delivery/Evidence/PcvAdminSmokeEvidenceDocsContractTests.cs` |

### D2 — current/promotion/package evidence, 5 files / 42 contracts

| Legacy file | Count | C# owner under `Delivery/Evidence/` |
| --- | ---: | --- |
| `Pcv04273PromotionEvidence.Tests.ps1` | 7 | `Pcv04273PromotionEvidenceContractTests.cs` |
| `Pcv04274PackageEvidence.Tests.ps1` | 11 | `Pcv04274PackageEvidenceContractTests.cs` |
| `PcvCurrentEvidenceGeneration.Tests.ps1` | 12 | `PcvCurrentEvidenceGenerationContractTests.cs` |
| `PcvFeatureEvidencePromotion.Tests.ps1` | 7 | `PcvFeatureEvidencePromotionContractTests.cs` |
| `PcvJobStore04265ReaderCompatibility.Tests.ps1` | 5 | `PcvJobStore04265ReaderCompatibilityContractTests.cs` |

### D3 — product invocation, 1 file / 61 contracts

| Legacy file | C# owner |
| --- | --- |
| `PcvDesktopNodeProduct.Invoke.Tests.ps1` | `Delivery/Product/PcvDesktopNodeProductInvokeContractTests.cs` |

### D4 — product descriptors, 3 files / 58 contracts

| Legacy file | Count | C# owner under `Delivery/Product/` |
| --- | ---: | --- |
| `PcvDesktopNodeProduct.Diagnostics.Tests.ps1` | 16 | `PcvDesktopNodeProductDiagnosticsContractTests.cs` |
| `PcvDesktopNodeProduct.Manifest.Tests.ps1` | 16 | `PcvDesktopNodeProductManifestContractTests.cs` |
| `PcvDesktopNodeProduct.Plan.Tests.ps1` | 26 | `PcvDesktopNodeProductPlanContractTests.cs` |

### D5 — development verification policy, 8 files / 51 contracts

| Legacy file | Count | C# owner under `Delivery/Verification/` |
| --- | ---: | --- |
| `PcvAgentExecutionCircuitBreaker.Tests.ps1` | 3 | `PcvAgentExecutionCircuitBreakerContractTests.cs` |
| `PcvCSharpArchitectureGapRegistry.Tests.ps1` | 10 | `PcvCSharpArchitectureGapRegistryContractTests.cs` |
| `PcvDevelopmentGateWorkflow.Tests.ps1` | 1 | `PcvDevelopmentGateWorkflowContractTests.cs` |
| `PcvDevelopmentVerification.Tests.ps1` | 9 | `PcvDevelopmentVerificationContractTests.cs` |
| `PcvDevelopmentVerificationExecution.Tests.ps1` | 3 | `PcvDevelopmentVerificationExecutionContractTests.cs` |
| `PcvDotNetQualityTools.Tests.ps1` | 20 | `PcvDotNetQualityToolsContractTests.cs` |
| `PcvModuleSizeRatchet.Tests.ps1` | 3 | `PcvModuleSizeRatchetContractTests.cs` |
| `PcvStrictCollection.Tests.ps1` | 2 | `PcvStrictCollectionContractTests.cs` |

### D6 — orchestration and timeout boundaries, 5 files / 44 contracts

| Legacy file | Count | C# owner under `Delivery/Orchestration/` |
| --- | ---: | --- |
| `PcvBatchSupervisor.Tests.ps1` | 28 | `PcvBatchSupervisorContractTests.cs` |
| `PcvCiTriggerContract.Tests.ps1` | 2 | `PcvCiTriggerContractTests.cs` |
| `PcvConfigJobStoreMigrationApplySmoke.Tests.ps1` | 5 | `PcvConfigJobStoreMigrationApplySmokeContractTests.cs` |
| `PcvRunnerArtifactRootContract.Tests.ps1` | 3 | `PcvRunnerArtifactRootContractTests.cs` |
| `PcvTimeoutRateLimitHardeningPreflight.Tests.ps1` | 6 | `PcvTimeoutRateLimitHardeningPreflightContractTests.cs` |

### D7 — package/public-distribution preflight, 8 files / 52 contracts

| Legacy file | Count | C# owner under `Delivery/Preflight/` |
| --- | ---: | --- |
| `PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1` | 6 | `PcvBuiltinTlsCertificateLifecyclePreflightContractTests.cs` |
| `PcvBurnBootstrapperPreflight.Tests.ps1` | 8 | `PcvBurnBootstrapperPreflightContractTests.cs` |
| `PcvDiagnosticBundleServerPreflight.Tests.ps1` | 6 | `PcvDiagnosticBundleServerPreflightContractTests.cs` |
| `PcvMsixPackagingFeasibilityPreflight.Tests.ps1` | 6 | `PcvMsixPackagingFeasibilityPreflightContractTests.cs` |
| `PcvPublicDistributionDescriptor.Tests.ps1` | 6 | `PcvPublicDistributionDescriptorContractTests.cs` |
| `PcvPublicDistributionOperationsBundle.Tests.ps1` | 6 | `PcvPublicDistributionOperationsBundleContractTests.cs` |
| `PcvPublicDistributionReadiness.Tests.ps1` | 6 | `PcvPublicDistributionReadinessContractTests.cs` |
| `PcvUpdaterCatalogPublicationPreflight.Tests.ps1` | 8 | `PcvUpdaterCatalogPublicationPreflightContractTests.cs` |

### D8 — manual-admin/public-ops readiness, 8 files / 45 contracts

| Legacy file | Count | C# owner under `Delivery/ManualAdmin/` |
| --- | ---: | --- |
| `PcvManualAdminBaselineReservation.Tests.ps1` | 3 | `PcvManualAdminBaselineReservationContractTests.cs` |
| `PcvManualAdminCampaignDescriptor.Tests.ps1` | 5 | `PcvManualAdminCampaignDescriptorContractTests.cs` |
| `PcvManualAdminDescriptorCurrency.Tests.ps1` | 6 | `PcvManualAdminDescriptorCurrencyContractTests.cs` |
| `PcvManualAdminRebaselineReadiness.Tests.ps1` | 10 | `PcvManualAdminRebaselineReadinessContractTests.cs` |
| `PcvPublicOpsFinalFollowupAttempt.Tests.ps1` | 3 | `PcvPublicOpsFinalFollowupAttemptContractTests.cs` |
| `PcvPublicOpsGateExecutionReadiness.Tests.ps1` | 5 | `PcvPublicOpsGateExecutionReadinessContractTests.cs` |
| `PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1` | 7 | `PcvPublicSignedUpdateRollbackSmokePreflightContractTests.cs` |
| `PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1` | 6 | `PcvWindowsCredentialManagerTransitionPreflightContractTests.cs` |

### D9 — installed-smoke descriptors, 8 files / 31 contracts

| Legacy file | Count | C# owner under `Delivery/Installed/` |
| --- | ---: | --- |
| `PcvApiHostJobHardeningInstalledSmoke.Tests.ps1` | 10 | `PcvApiHostJobHardeningInstalledSmokeContractTests.cs` |
| `PcvInstalledAccountLoginSmoke.Tests.ps1` | 1 | `PcvInstalledAccountLoginSmokeContractTests.cs` |
| `PcvInstalledLoopbackBootstrapSmoke.Tests.ps1` | 1 | `PcvInstalledLoopbackBootstrapSmokeContractTests.cs` |
| `PcvInstalledNoVncSmoke.Tests.ps1` | 1 | `PcvInstalledNoVncSmokeContractTests.cs` |
| `PcvInternalHttpsTlsLifecycleSmoke.Tests.ps1` | 1 | `PcvInternalHttpsTlsLifecycleSmokeContractTests.cs` |
| `PcvOsMutationGateSmoke.Tests.ps1` | 6 | `PcvOsMutationGateSmokeContractTests.cs` |
| `PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1` | 5 | `PcvServicePlanP0CheckpointRestoreReconciliationContractTests.cs` |
| `PcvServiceTokenRotationRevokePreflight.Tests.ps1` | 6 | `PcvServiceTokenRotationRevokePreflightContractTests.cs` |

### D10 — reconciliation/lifecycle policy, 8 files / 54 contracts

| Legacy file | Count | C# owner |
| --- | ---: | --- |
| `PcvPostRebootVerification.Tests.ps1` | 21 | `Delivery/Reconciliation/PcvPostRebootVerificationContractTests.cs` |
| `PcvWave2BReconciliationDecision.Tests.ps1` | 6 | `Delivery/Reconciliation/PcvWave2BReconciliationDecisionContractTests.cs` |
| `PcvWave2CCheckpointCreateReconciliation.Tests.ps1` | 4 | `Delivery/Reconciliation/PcvWave2CCheckpointCreateReconciliationContractTests.cs` |
| `PcvWave2CVmDeleteReconciliation.Tests.ps1` | 4 | `Delivery/Reconciliation/PcvWave2CVmDeleteReconciliationContractTests.cs` |
| `PcvWave2CVmRenameReconciliation.Tests.ps1` | 4 | `Delivery/Reconciliation/PcvWave2CVmRenameReconciliationContractTests.cs` |
| `PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1` | 2 | `Delivery/Reconciliation/PcvWindowsEventLogDefaultTransitionSmokeContractTests.cs` |
| `PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1` | 6 | `Delivery/Reconciliation/PcvWindowsEventLogProviderTransitionPreflightContractTests.cs` |
| `PcvWingetManifestCompliancePreflight.Tests.ps1` | 7 | `Delivery/Preflight/PcvWingetManifestCompliancePreflightContractTests.cs` |

Total: `90+42+61+58+51+44+52+45+31+54 = 528`.

## Per-batch execution protocol

For every D1-D10 task:

1. Add exact `PcvLegacyContract` facts before manifest regeneration.
2. Run ledger `--check`; expect drift equal to that batch's contract count (RED).
3. Port every assertion with typed readers and the exact legacy semantics.
4. Add at least one positive and one mutated negative fixture per structured responsibility; a batch with multiple parsers covers each parser branch.
5. Run the batch's C# facts, then all Category=Delivery tests.
6. Run only the listed legacy files under Pester 5.7.1; expected count equals the batch count, failed/skipped/not-run `0`.
7. Regenerate manifest, set batch rows mapped/local pass with the Wave D evidence locator, keep CI pending, and run both Node/C# ledger validators.
8. Run `git diff --check`, fixed-diff review, and commit once.

Use this invocation shape after assigning `$batchPaths` to the exact legacy paths listed by the current task:

```powershell
$configuration = New-PesterConfiguration
$configuration.Run.Path = $batchPaths
$configuration.Run.PassThru = $true
$configuration.Output.Verbosity = 'Detailed'
$result = Invoke-Pester -Configuration $configuration
if ($result.FailedCount -ne 0 -or $result.SkippedCount -ne 0 -or $result.NotRunCount -ne 0) { throw 'legacy batch failed' }
```

Before invocation, print only the repository-relative `$batchPaths` and assert its count equals the current batch file count; the evidence records that exact array.

## Task 1: Add shared typed helpers and Wave D evidence skeleton

**Files:** common helper files from the file map; `DeliveryNegativeParityTests.cs`; Wave D evidence document

- [ ] **Step 1: Write RED helper tests for strict JSON/XML/Markdown/source parsing**

Require duplicate JSON keys, wrong JSON type, XML namespace drift, duplicate XML element, Markdown heading/table reorder, source token in comment/string, path escape, symlink, malformed UTF-8, and inconsistent newline policy to fail with error code `PCV_DELIVERY_CONTRACT_INVALID`, a repository-relative owner, and one fixed detail from `duplicate-json-key`, `json-type`, `xml-namespace`, `xml-cardinality`, `markdown-order`, `source-token-context`, `path-containment`, `symlink`, `utf8`, or `newline-policy`.

- [ ] **Step 2: Implement only helpers consumed by D1**

Do not add abstractions for later batches until their first use. Helpers accept a contained fixture root so negative tests never edit repository files.

- [ ] **Step 3: Create the evidence skeleton**

List all ten batches and exact counts with `not-run`, ledger starting state Packaging unmapped `528`, CI pending, host mutation false, public signing/publication false.

- [ ] **Step 4: Run infrastructure tests and commit**

Commit message: `test: add delivery contract fixture helpers`.

## Task 2: D1 — port canonical admin-smoke evidence 90/90

**Files:** `Delivery/Evidence/PcvAdminSmokeEvidenceDocsContractTests.cs`, negative fixtures, manifest, Wave D evidence

- [ ] **Step 1: Add exact facts 001-090 and confirm ledger RED `90`**
- [ ] **Step 2: Port every current/historical evidence anchor, hash, provenance, claim, generated-block, and predecessor-boundary assertion with ordered Markdown/JSON parsing**
- [ ] **Step 3: Add mutations for stale current anchor, wrong SHA length/case, missing predecessor label, and false public-signing claim**
- [ ] **Step 4: Run replacement `90/90`, legacy `90/90`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port admin smoke evidence contracts`**

## Task 3: D2 — port evidence/promotion 42/42 and activate current-evidence handler

**Files:** five D2 C# owners; negative fixtures; manifest/evidence; `CurrentEvidenceVerifier.cs`; `ManagedSuiteRunner.cs`; corresponding Verification tests; `Program.cs`

- [ ] **Step 1: Add exact facts and confirm ledger RED `42`**

- [ ] **Step 2: Write managed-handler RED tests**

Require canonical current evidence PASS; malformed/root-extra/case-drift/contradictory qualification/stale generated block/missing reference/wrong digest/path escape failures; no source/target writes on success or failure; cancellation; unknown handler fail-closed; absolute paths redacted.

- [ ] **Step 3: Implement pure current-evidence validation**

`CurrentEvidenceVerifier.Verify(repositoryRoot)` reads `docs/ga-ready/current-evidence.json` and schema invariants, validates the feature qualification and referenced evidence files, renders the expected generated block in memory, and compares every current-facing generated block. It never calls the legacy updater or a child process.

- [ ] **Step 4: Implement exact managed dispatch**

`ManagedSuiteRunner` dispatches `current-evidence-check` to the verifier and returns a coherent `SuiteExecutionRecord`. Until Wave E, `policy-boundaries` returns `ParityUnmapped`. Wire this runner in `Program.Main`; keep activation non-active so ordinary actual execution remains gated.

- [ ] **Step 5: Port all five legacy fixtures and negative branches**

The 12 current-evidence contracts call the pure verifier/renderer directly; blocked candidate tests assert in-memory rejection before any filesystem write. Do not reproduce the legacy test's `pwsh -File` child invocation.

- [ ] **Step 6: Run replacement `42/42`, legacy `42/42`, managed-handler tests, all Delivery, and ledger validators**
- [ ] **Step 7: Review and commit with `test: port current and promotion evidence contracts`**

## Task 4: D3 — port product invocation 61/61

**Files:** `Delivery/Product/PcvDesktopNodeProductInvokeContractTests.cs`, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `61`**
- [ ] **Step 2: Port command-plan/argument-array/route/error/redaction/timeout/cancellation contracts as static or pure-fixture checks; never invoke the product script**
- [ ] **Step 3: Add argument injection, missing route, duplicate action, unredacted token, and mutation-command negative fixtures**
- [ ] **Step 4: Run replacement `61/61`, legacy `61/61`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port product invocation contracts`**

## Task 5: D4 — port product descriptor contracts 58/58

**Files:** three D4 owners, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `58`**
- [ ] **Step 2: Port diagnostics, manifest, and plan JSON/XML/source contracts with exact cardinality, order, defaults, action families, and forbidden mutation boundary**
- [ ] **Step 3: Add missing/duplicate manifest entry, invalid plan path, and diagnostics leakage mutations**
- [ ] **Step 4: Run replacement `58/58`, legacy `58/58`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port product descriptor contracts`**

## Task 6: D5 — port development-verification policy 51/51

**Files:** eight D5 owners, negative fixtures, manifest/evidence; related verification policy tests when ownership overlaps

- [ ] **Step 1: Add exact facts and confirm ledger RED `51`**
- [ ] **Step 2: Port circuit-breaker config, architecture registry, current shadow workflow, suite catalog/runner policy, quality tool pins, module-size ratchet, and strict collection semantics**
- [ ] **Step 3: Parse workflow structurally only in Wave E; in D5 preserve the legacy workflow contract through a contained source model and mark the owner as a cutover-guard consumer**
- [ ] **Step 4: Add weakened-workflow, forbidden executable, widened module threshold, unknown tool version, and duplicate suite mutations**
- [ ] **Step 5: Run replacement `51/51`, legacy `51/51`, all Delivery, and ledger validators**
- [ ] **Step 6: Review and commit with `test: port development verification policy contracts`**

## Task 7: D6 — port orchestration/timeout boundaries 44/44

**Files:** five D6 owners, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `44`**
- [ ] **Step 2: Port batch ordering, one-terminal-row, retry/timeout/rate-limit, artifact containment, CI trigger, and migration-plan shape contracts without starting a batch**
- [ ] **Step 3: Add double-terminal, timeout overflow, escaping artifact root, duplicate trigger, and mutation-enabled plan fixtures**
- [ ] **Step 4: Run replacement `44/44`, legacy `44/44`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port delivery orchestration contracts`**

## Task 8: D7 — port public/package preflight 52/52

**Files:** eight D7 owners, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `52`**
- [ ] **Step 2: Port TLS/Burn/diagnostic/MSIX/distribution/updater descriptor and readiness checks as read-only schema/source verification**
- [ ] **Step 3: Preserve every blocked/not-run/non-publication state; do not upgrade it to PASS because source is now public**
- [ ] **Step 4: Add missing blocker, false published state, unsafe credential field, wrong package channel, and executable mutation fixtures**
- [ ] **Step 5: Run replacement `52/52`, legacy `52/52`, all Delivery, and ledger validators**
- [ ] **Step 6: Review and commit with `test: port package distribution preflight contracts`**

## Task 9: D8 — port manual-admin/public-ops readiness 45/45

**Files:** eight D8 owners, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `45`**
- [ ] **Step 2: Port baseline reservation, campaign descriptor/currency, rebaseline, public-ops, update/rollback, and Credential Manager transition shape contracts**
- [ ] **Step 3: Add stale descriptor, mismatched package pair, missing blocker, secret-valued field, and false external publication fixtures**
- [ ] **Step 4: Run replacement `45/45`, legacy `45/45`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port manual admin readiness contracts`**

## Task 10: D9 — port installed-smoke descriptor boundaries 31/31

**Files:** eight D9 owners, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `31`**
- [ ] **Step 2: Port installed-smoke evidence/schema/source contracts without contacting an installed listener, service, certificate store, credential store, Event Log, firewall, or VM provider**
- [ ] **Step 3: Add host-mutation-true, token-observed, missing cleanup, wrong route result, and fabricated installed-PASS fixtures**
- [ ] **Step 4: Run replacement `31/31`, legacy `31/31`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port installed smoke descriptor contracts`**

## Task 11: D10 — port reconciliation/lifecycle policy 54/54

**Files:** eight D10 owners, negative fixtures, manifest/evidence

- [ ] **Step 1: Add exact facts and confirm ledger RED `54`**
- [ ] **Step 2: Port post-reboot, decision/checkpoint/delete/rename reconciliation, Event Log transition, and winget manifest compliance contracts as descriptor/state-machine checks**
- [ ] **Step 3: Add illegal transition, stale checkpoint, missing cleanup, reordered lifecycle, Event Log mutation, and invalid winget field fixtures**
- [ ] **Step 4: Run replacement `54/54`, legacy `54/54`, all Delivery, and ledger validators**
- [ ] **Step 5: Review and commit with `test: port reconciliation lifecycle contracts`**

## Task 12: Close all local mapping and prepare shadow-ready inputs

**Files:**
- Modify: `config/development-verification-suites.json`
- Modify: `config/development-verification-suites.schema.json`
- Modify: `src/DesktopNode.Verification/VerificationCatalog.cs`
- Modify: related Verification catalog/summary/architecture tests
- Finalize: Wave D evidence
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [ ] **Step 1: Advance only mapped suite states**

Set `delivery-contracts=mapped` and `evidence-check=mapped`; preserve `installer-contracts=mapped`, Web mapped state, and non-active catalog activation. Do not mark CI parity pass or cutover.

- [ ] **Step 2: Require exact local ledger closure**

```text
files=62/62
contracts=627/627
web_mapped_local_pass=50
installer_mapped_local_pass=49
delivery_mapped_local_pass=528
unmapped=0
ci_pending=627
duplicate=0
missing=0
order_drift=0
```

- [ ] **Step 3: Run the complete replacement set**

```powershell
dotnet restore src/DesktopNode.sln
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter Category=Delivery --no-restore --nologo
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --no-restore --nologo
dotnet test src/DesktopNode.sln -c Release --no-restore --nologo
npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web
npm run test:web-contracts --prefix web
npm run check:verification-migration-manifest --prefix web
git diff --check
```

Expected: Delivery category exactly `528/528`, Installer `49/49`, Web contracts `50/50`, no skipped replacement.

- [ ] **Step 4: Run all 55 Packaging legacy files once**

Invoke Pester 5.7.1 on `packaging/windows-desktop-node/tests`. Expected `528/528`, failed/skipped/not-run `0`. Record result digest and duration. Do not execute any separately opt-in admin smoke runner.

- [ ] **Step 5: Verify the managed current-evidence handler directly**

Unit/integration invoke the handler with the physical repository, then with contained stale/malformed fixtures. Expected canonical PASS, each invalid fixture stable failure, writes `0`, child processes `0`.

- [ ] **Step 6: Run independent fixed-diff review**

Review all D1-D10 commits against Wave C checkpoint. Require unported assertion `0`, metadata/manifest mismatch `0`, process/mutation boundary finding `0`, P0/P1 `0`, required workflow changes `0`.

- [ ] **Step 7: Finalize evidence and commit**

Record every batch SHA/count/duration, aggregate replacement and legacy hashes, managed-handler result, manifest closure, review findings, and explicit non-claims. Commit message: `docs: record Packaging Wave D parity`.

- [ ] **Step 8: Push the draft branch to the new authority and observe current protected CI**

Push without force to `HardcoreMonk/purecvisor-desktop-node-public`. Keep the PR draft. Existing required jobs still run the legacy workflow; their success is not yet the same-SHA shadow evidence required by Wave E.

## Completion checkpoint

- Packaging replacement `528/528`, Installer `49/49`, Web `50/50`; replacement skipped `0`.
- Legacy Packaging `528/528` and Installer/Web retained; no legacy file deleted or weakened.
- Manifest local mapping `627/627`, unmapped/missing/duplicate/order drift `0`, CI parity pending `627`.
- `current-evidence-check` is a write-free C# managed handler with positive/negative proof.
- Required CI job identities and branch protection are unchanged.
- Host/service/MSI/VM mutation `0`; public signing/stable binary publication still false.
- Stop and execute the Wave E shadow/cutover plan.

## Failure and rollback

- Revert only the failing batch commit; do not reset the shared branch or delete legacy tests.
- A large fixture may be split internally, but its 1:1 metadata and legacy owner remain one file boundary.
- Any legacy/reference mismatch blocks later batches even if the aggregate count is correct.
- Any current-evidence checker write or child process is a P1 architecture finding and blocks shadow CI.
- A batch that exceeds the circuit-breaker/checkpoint budget stops with exact remaining files; it is not marked partial PASS.
