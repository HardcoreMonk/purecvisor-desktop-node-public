# C# architecture improvement Wave 0 baseline evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `M / Full`
- Evidence type: code-level baseline and test characterization
- Implementation status: `code_complete` (Wave 0)
- Promotion status: `promotion_not_triggered`
- Product source changed: `true` (behavior-preserving injection seams)
- Product behavior changed: `false`
- Host mutation performed: `false`
- Package/current operational evidence anchor changed: `false`
- Operational anchor: `0.42.65-admin-smoke` carry-forward
- Actual-VM/admin smoke: not run; not required for this code-level `M / Full` slice
- Public trusted signing: `false`
- External stable publication: `false`

This record closes Wave 0, including its test-safety batches and the separate behavior-preserving product-source seam slice. It does not claim that the registered job durability, timeout lifetime, shutdown supervision, or service-health gaps are fixed. Current publish-before-save, signal-before-save and timeout-task lifetime traces remain executable characterization assigned to Wave 2A or Wave 5A. This work does not introduce ASP.NET Core or change the current `HttpListener` product transport.

The audit base commit contains the pre-change 591-test state; it is not presented as the provenance commit for the 611-test source state. The tracked quality fixture identifies the current .NET inputs separately with `pcv-dotnet-source-snapshot/v1`, SHA-256 `fba5a4a6fc1b90fc82bc51222fe47baf6efca67c74552d3ef1c213fba4fcf301`, 128 files, and zero deleted-path records.

## Implemented baseline

### API route contract

- `ApiHandlerAdapterContract.CreateDefault()` remains the authoritative source for 54 routes.
- A complete canonical projection now covers method, route template, family, permission, mutation stance, auth policy, owner, and operation name.
- Canonical SHA-256: `6bc921a3daf8b1ff699c5a6ff9170643f3abf6b604a7c600daf1264851336603`.
- Mutation stance distribution: read-only 22, product operation 10, queued mutation 22.
- Route family count: 13.

### Hyper-V test ownership

- `HyperVDomainContractTests` moved from `DesktopNode.Api.Tests` to `DesktopNode.HyperV.Tests` without product changes.
- Moved test cases: 35 before, 35 after.
- Project count effect of the move alone: API 278 to 243, Hyper-V 8 to 43; combined total remains 286.
- The moved suite pins 34 unique operation names and the domain, dispatch handler, provider boundary, telemetry, and error-contract projections.
- A separate API route snapshot Fact raises the final API project count to 244.
- The remaining direct native adapter tests moved as 38 methods/49 cases to `DesktopNodeHyperVNativeAdapterTests`.
- The remaining direct WMI provider tests moved as 22 methods/30 cases to `DesktopNodeHyperVWmiProviderTests`.
- The ownership move changes API 244 to 165 and Hyper-V 43 to 122 while preserving all 79 cases; the old API `Native*`/`Wmi*` owner now discovers zero cases.

### Job store golden and filesystem isolation

- `job-store-characterization-v1.json` now owns executable v1/v2 load projection, persisted-running interruption/rewrite, stored FIFO dispatch, and terminal-retention writer shape expectations.
- Four new API tests verify v1/v2 equivalent projection, running recovery queue exclusion, FIFO provider order, and 503 terminal jobs pruning to the newest 500 while preserving the active job.
- `IDesktopNodeApiJobStore`, the physical JSON file adapter, a job-only clock, and route/linked-job cancellation scopes are internal injectable boundaries; public `CreateDefault(jobStorePath:)` remains compatible.
- Recording store characterization pins create → attempted save, linked scope → running save → provider → terminal save, deterministic job/saved timestamps, queue snapshots, running signal-before-save, route timeout token identity, and worker-owner cancellation propagation.
- `BackgroundWorkerMarksJobFailedWhenStartSaveFailsWithoutInvokingNativeMutation` now uses a fail-on-second-write recording store and proves memory `failed`, last durable `queued`, attempted `running`, and native invoke 0 without filesystem timing.
- `JobStoreSaveUsesAtomicTempReplaceAndCleansStaleTemp` remains a real GUID filesystem contract for the physical `.tmp` replace behavior.
- `BatchEvidenceSummaryReader` uses an internal targeted file-access boundary for existence, attributes, reads, listings, and last-write metadata while keeping path policy, discovery, and projection in the reader.
- Evidence path/reparse/sort/error tests use a recording file-access fake; actual symlink/reparse containment and the named CWD integration probe remain real filesystem tests.
- Every test that creates filesystem state uses a per-test GUID sandbox/root. The fixed non-I/O path-policy probe also carries a GUID suffix.
- The one unavoidable process-global current-directory probe moved to `BatchEvidenceSummaryReaderTests` and an explicitly named `Batch evidence CWD isolation` collection with `DisableParallelization=true`.
- Both `Directory.SetCurrentDirectory` calls are outside the test body in the per-test constructor/dispose scope. Wave 7 owns removal after configured child-root/path resolution no longer depends on process CWD.

### Host Ops and HTTP transport characterization

- Host Ops catalog guards pin 9 unique operation families and 22 unique actions.
- Existing mutation-boundary and explicit firewall/trust-store approval tests remain active.
- `http-transport-contract-v1.json` records the current `System.Net.HttpListener` behavior without creating a duplicate route catalog.
- Ten executable Host tests cover RawTarget/path projection, query and trailing-slash handling, case and percent encoding, exact 404/405 responses, API/Web port separation, static/config assets, OPTIONS/CORS, service-token/account precedence, known/unknown-length body limits, product headers, and noVNC handshake/proxy/close behavior.
- Host test count increases from 150 to 160.

### Fault and migration audit

- The gap registry records six required scenarios as current evidence or static trace, expected safe result, owning wave, RED test, GREEN boundary, and removal condition.
- Current classifications are intentionally not all safe: job create/save ghost, transition save divergence, timeout late overlap, non-object store startup, untracked HTTP/noVNC shutdown tasks, and unobserved listener/worker health faults remain assigned to Wave 2A or Wave 5A.
- The machine-readable migration manifest records the completed Hyper-V domain, direct Native, direct WMI, and serialized CWD test moves plus the remaining planned owner migrations.
- Only a migration with `status=completed` may authorize a removed test; a `planned` entry is rejected even when its replacement already exists.
- Hyper-V direct ownership is checked against the current C# sources rather than only the tracked quality snapshot: API old-owner Native/WMI cases remain zero, while Hyper-V Native and WMI stay 49 and 30 cases respectively.
- Private reflection is now 0 occurrences: the reparse guard and sort-time tests call the internal policy/file-access seam directly. Inventory retains one named serialized process-global CWD test with two lifecycle calls, three production source-text checks, and zero remaining direct `Native*`/`Wmi*` ownership candidates in the API test file.

## Quality baseline

Raw final Wave 0 results are stored under the ignored artifact root `artifacts/dotnet-quality-wave0-seams-final-20260802`:

- TRX files: 7
- Cobertura files: 14 physical copies, 7 unique project results; each collector result and its TRX attachment copy have identical SHA-256
- .NET SDK: `10.0.302`
- `coverlet.collector`: `6.0.4`
- Tests: 611 total, 611 executed, 611 passed, 0 failed, 0 skipped
- Weighted line coverage: `48.833445%`
- Weighted branch coverage: `39.917355%`

| Test project | Total | Skip | Line | Branch |
|---|---:|---:|---:|---:|
| `DesktopNode.Api.Tests` | 174 | 0 | 51.55% | 45.77% |
| `DesktopNode.Cli.Tests` | 112 | 0 | 83.09% | 66.35% |
| `DesktopNode.Contracts.Tests` | 15 | 0 | 92.57% | 71.59% |
| `DesktopNode.Host.Tests` | 160 | 0 | 43.71% | 32.13% |
| `DesktopNode.HyperV.Tests` | 122 | 0 | 37.83% | 32.42% |
| `DesktopNode.Runtime.Tests` | 17 | 0 | 91.67% | 75.00% |
| `DesktopNode.Service.Tests` | 11 | 0 | 100.00% | 100.00% |

The tracked `csharp-architecture-quality-baseline.json` contains the unrounded project values, fully qualified discovered test IDs, audit base commit, and deterministic .NET source snapshot. The ratchet rejects aggregate test-count decline, skip increase, per-project line or branch decline beyond `0.0%p`, toolchain drift, invalid result files, path escape or reparse-point traversal, duplicate projects, planned-only migration removal, and unmapped test removal. A wildcard migration is accepted only when a single suffix substitution derives exactly one distinct live replacement per removed test. Capture and ratchet path guards reject junction traversal before recursive cleanup, artifact reads, or baseline writes.

The pre-change audit baseline was 591 tests and skip 0. The final Wave 0 total is 611: the prior 606 safety baseline plus four job store/clock/cancellation characterization Facts and one evidence metadata failure Fact.

## Verification

| Verification | Result |
|---|---|
| Initial `dotnet test src/DesktopNode.sln -c Release --no-restore` | PASS, 591/591, skip 0 |
| Hyper-V ownership move focused suite | PASS, 43/43; moved discovery 35/35 |
| HTTP transport characterization, three repeated focused runs | PASS, 10/10 on every run |
| API test project, three repeated runs | PASS, 244/244 on every run |
| First-batch final `dotnet test src/DesktopNode.sln -c Release --no-restore` | PASS, 602/602, skip 0 |
| Follow-up direct Native/WMI ownership | PASS, Hyper-V 79/79; API old-owner 0 |
| Job store golden characterization | PASS, 4/4 |
| CWD serialized owner characterization | PASS, 1/1 |
| Follow-up API project, three repeated runs | PASS, 169/169 on every run |
| Follow-up full solution | PASS, 606/606, skip 0 |
| Gap registry Pester | PASS, 8/8 |
| Quality tooling Pester | PASS, 20/20 |
| Quality `-WriteBaseline` then normal ratchet | PASS, 606 total, skip 0, line 48.584556%, branch 39.818048% |
| `dotnet format --verify-no-changes --include` for all nine changed C# test files | PASS |
| Development verification `M / Full` | PASS: .NET, Web npm/parity, packaging Pester, installer Pester, Web Pester, diff check, current-evidence check |
| `git diff --check` | PASS |

Wave 0 product-source closure verification:

| Verification | Result |
|---|---|
| Job/store/clock/cancellation focused characterization | PASS, 6/6 |
| Batch evidence focused characterization | PASS, 12/12 |
| API project, three repeated runs | PASS, 174/174 on every run |
| Final full solution | PASS, 611/611, skip 0 |
| Gap registry Pester after fixture update | PASS, 8/8 |
| Quality tooling Pester | PASS, 20/20 |
| Quality ratchet against prior baseline | PASS, test count +5, line +0.248889%p, branch +0.099307%p |
| Final quality `-WriteBaseline` then normal ratchet | PASS, 611 total, skip 0, line 48.833445%, branch 39.917355% |
| Scoped format check excluding the pre-existing processor whitespace region | PASS for the other eight changed/new C# files |
| Development verification `M / Full` | PASS, seven selected suites, `ok=true` |
| `git diff --check` | PASS |

Final Wave 0 verification summary: `artifacts/development-verification-csharp-architecture-wave0-seams-final-r3/summary.json`. The effective lane remained `Full`, change tier remained `M` (`api-cli-web-contract`, `packaging-contract`, `cross-module-change`), and all seven selected suites passed.

The repository-wide format command is not claimed as a clean baseline: it reports pre-existing whitespace findings in the large route block of `DesktopNodeApiRequestProcessor.cs`, outside this slice's edited regions. The scoped format check passes for the other eight changed/new C# files; the processor seam hunks are manually constrained and `git diff --check` passes without reformatting unrelated code.

## Wave 0 closure and next boundary

- Wave 0 is `code_complete`; no package candidate was produced, so promotion is `promotion_not_triggered`.
- Wave 1 may now move job runtime and diagnostics ownership while keeping `CreateDefault`/`Handle` as compatibility façades.
- Job durability ordering changes remain Wave 2A, timeout/async lifetime changes remain Wave 5A, and configured-root/path projector ownership plus the serialized CWD exception remain Wave 7.
- The three production source-text guards remain assigned to their later owner waves.

The GA-ready evidence index, current-evidence ledger/JSON/schema, package anchor, installed current-card, signing claim, and publication claim were intentionally left unchanged.
