# C# architecture improvement Wave 1A job runtime owner evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Verification source commit: `a7a87c7152ddfeb4393dc113791ff78bfec81eda`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `M / Full`
- Evidence type: code-level behavior-preserving ownership move
- Implementation status: `code_complete` (Wave 1A only; Wave 1B/1C/1D remain open)
- Promotion status: `promotion_not_triggered`
- Product source changed: `true`
- Product behavior changed: `false`
- Host mutation performed: `false`
- Package build performed: `false`
- Installed product changed: `false`
- Package/current operational evidence anchor changed: `false`
- Operational anchor: `0.42.65-admin-smoke` carry-forward
- Actual-VM/admin smoke: not run; not required for this code-level `M / Full` slice
- Public trusted signing: `false`
- External stable publication: `false`

This record closes Wave 1A only. It moves job state ownership out of
`DesktopNodeApiRequestProcessor` while preserving the public API façade, current global request
serialization, single mutation worker, JSON job-store schema and established route/JSON/`PCV_*`
contracts. It does not introduce ASP.NET Core; the product HTTP transport remains the existing
`HttpListener` path. Wave 1B diagnostics, Wave 1C auth/session/RBAC and Wave 1D ops dispatch are not
claimed complete by this evidence.

## Implemented ownership move

### Explicit request context

- Commit `f57b4fc753469af279d9dc12fe761d2780c483ee` normalizes a request ID once in
  `DesktopNodeApiRequestProcessor.Handle` and passes it explicitly through request/job creation.
- Request ID and correlation ID use `DesktopNodeJobRequestContext`; the processor does not use
  ambient `AsyncLocal` request state.
- The response request-ID header/body projection, generated request IDs and job/store request IDs
  remain aligned with existing API golden tests.

### Runtime owner

- Commit `9bfdb1a24c36745068e0156ca984092a76616549` adds
  `DesktopNode.Runtime.DesktopNodeJobRuntime` as the concrete owner of the job dictionary, FIFO
  queue, running cancellation registrations, retention counters, schema/load block and JSON
  load/save/recovery lifecycle.
- `DesktopNodeJobRuntime` owns immutable job snapshots and neutral command/completion outcomes.
  Runtime references the approved `DesktopNode.Contracts` boundary and does not reference API or
  Hyper-V assemblies.
- The physical JSON store and job clock interfaces moved to Runtime. API cancellation scopes remain
  API-owned so route timeout and linked worker cancellation retain their existing lifetime.
- Guest-execution dispatch keeps the raw credential reference in same-process execution parameters,
  while snapshots and durable JSON retain the existing redacted reference/hash projection.
- v1/v2 reads, unsupported-future no-mutation, persisted-running recovery, FIFO dispatch, terminal
  retention and current unsafe save/cancel call order remain executable Runtime tests.

### API façade switch

- Commit `dfa15e39c65bc80c8c7a0081489148a964a9d486` changes the processor to hold one
  `DesktopNodeJobRuntime` field and removes its former job dictionary, queue, store/load-save fields
  and nested mutable job types.
- `CreateDefault` and `Handle` remain compatible public façades. Job create/get/cancel/retry,
  list/delete-status projection and ops-summary job rows delegate to Runtime snapshots.
- Hyper-V results cross the Runtime boundary as a neutral full-envelope `JsonElement` plus
  `JobError`; Runtime does not gain a Hyper-V reference.
- Worker ordering remains `TryStart` under the existing global `sync`, provider execution without
  the global/state lock, `DetachRunningCancellation` under a first global acquisition and
  `Complete` under a second distinct global acquisition. Runtime never acquires the API global lock.
- `DesktopNodeApiJobRuntimeHandler` remains an API-only route parser and owns no job state. Wrapper
  evaluation for the remaining Wave 1B/1C/1D slices stays open.

### Compiled ownership and failure-boundary tests

- Commit `f116de10b3232a98db1b585996d3ee09c6c0932e` adds compiled metadata guards proving that
  the API processor no longer contains the retired store/clock/job containers and that the Runtime
  assembly references only approved product contracts.
- Commit `a7a87c7152ddfeb4393dc113791ff78bfec81eda` closes the Runtime owner failure-branch
  coverage needed by the no-regression quality ratchet.
- Owner-only store/clock/state/cancellation tests run in `DesktopNode.Runtime.Tests`; API route,
  status, JSON, request-timeout and linked-token façade tests remain in `DesktopNode.Api.Tests`.
  No API route/HTTP test was deleted or moved in this slice.

The current C# inventory is 79 product files / 24,672 physical LOC and 39 test files / 17,254
physical LOC. `DesktopNodeApiRequestProcessor.cs` decreased from the Wave 0 value of 3,477 lines to
2,972 lines while the new Runtime owner contains the moved policy and persistence body.

## Preserved behavior and deferred safety work

Wave 1A is an ownership move, not the Wave 2 durability behavior change. The following unsafe
traces and executable characterizations intentionally remain unchanged:

- create/save failure can publish the current in-memory job/queue before durable commit;
- start/complete save failure can leave current memory/disk meanings different;
- running cancellation records memory state and signals the provider before its save completes;
- malformed JSON syntax is quarantined, while a syntactically valid non-object root retains the
  currently characterized unstructured startup failure;
- a timed-out GET inner task lifetime remains assigned to Wave 5A;
- single mutation worker and current global request serialization remain active.

Wave 2A owns durable save-before-publish, transition/recovery integrity and cancellation ordering.
Wave 5A owns async timeout, late-task and service child-task lifetime changes. This evidence does not
claim either follow-up is fixed.

## Verification

The deterministic .NET source snapshot for the verified source commit is
`df3ed37fc056eb97f5bdf7e37f0799df213dfbb9566342cfac3d57a8d18211be` across 136 files.
Raw quality results are stored under
`artifacts/dotnet-quality-wave1a-runtime-owner-20260802-r4`.

| Verification | Result |
|---|---|
| Solution build | PASS, warnings 0, errors 0 |
| API tests | PASS, 179/179; three repeated runs |
| Runtime tests | PASS, 42/42, skip 0 |
| Full .NET solution | PASS, 642/642, skip 0 |
| Quality capture/ratchet | PASS, 642 total, skip 0, line `50.322384%`, branch `40.897689%` |
| Runtime owner scoped coverage | PASS, line `700/749` (`93.457944%`), branch `215/260` (`82.692308%`) |
| 54-route/API JSON and request serialization regression | PASS through API and Full verification |
| Development verification | PASS, Full/M, `ok=true` |
| `git diff --check` | PASS |

The Full/M summary is
`artifacts/development-verification-csharp-runtime-core-wave1a-20260802-final/summary.json`. Its effective
lane is `Full`, change tier is `M`, and all selected .NET, Web npm/parity, packaging Pester,
installer Pester, Web Pester, diff and current-evidence suites passed. The current-evidence check
also confirms that this code-level slice did not change the generated operational anchors.

## Closure and next boundary

- Wave 1A is `code_complete`; no package candidate was produced, so promotion is
  `promotion_not_triggered`.
- `0.42.65-admin-smoke`, Web Console/PCVCLI, TypeScript static assets and the internal/private
  network boundary carry forward unchanged.
- Wave 1B diagnostics, Wave 1C auth/session/RBAC and Wave 1D ops dispatch remain unchecked and must
  be implemented and verified in their separately classified slices.
- Wave 2A may now use the Runtime owner as the location for its RED/GREEN durability work, but none
  of the Wave 2 behavior changes are part of this evidence.

The GA-ready evidence index, current-evidence ledger/JSON/schema, ADR index, package/fullgate/current
card, signing claim and publication claim were intentionally left unchanged.
