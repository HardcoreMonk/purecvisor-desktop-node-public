# C# architecture improvement Wave 2A job create pre-ack durability evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Decision: `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`
- Gap registry: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Decision commit: `1c69ac2afa2b452e620cb4c830d2941167ddc07d`
- Verification source commit: `6a4735ef419f584dd1d4c223a90e837073a5744f`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `L / Release`
- Evidence type: code-level persistence/restart behavior change for `W0-FI-01` pre-acknowledgement failure only
- Implementation status: `code_complete` (`W0-FI-01` pre-ack slice)
- Remaining `W0-FI-01` status: `partial-safe`; physical post-replace indeterminate outcome is open
- Promotion status: `promotion_not_triggered`
- Product source changed: `true`
- Product behavior changed: `true`
- `host_mutation_performed`: `false`
- Package build performed: `false`
- Installed product changed: `false`
- Actual-VM/admin smoke: not run; no package candidate was produced
- Package/current operational evidence anchor changed: `false`
- Operational anchor: `0.42.65-admin-smoke` / `full-admin-host-mutation-gate-20260716-04265`
- Operational MSI SHA-256: `9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30`
- Operational payload aggregate SHA-256: `5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28`
- Operational provenance commit: `4855947fe0199cedc978e8b40ffb45e96ced6876`
- `public_trusted_signing`: `false`
- `external_stable_publication`: `false`

This record does not introduce ASP.NET Core. `System.Net.HttpListener`, Web Console, PCVCLI,
TypeScript static assets and the internal/private-network product boundary remain unchanged.

## Decision and scoped failure semantics

Decision commit `1c69ac2a` keeps the JSON store and schema v1/v2 compatibility. It fixes the
following order for create mutations:

1. calculate candidate jobs, queue and retention count without changing live state;
2. serialize the candidate with the established redaction and JSON shape;
3. require the store write to return normally;
4. publish the candidate dictionary/queue references and return HTTP success.

The physical writer remains the existing fixed-temp/move implementation. Normal return means an
acknowledged snapshot commit only; unique temp, `FileStream.Flush(true)`, candidate exact-byte/SHA
readback and typed `committed/not_committed/indeterminate` outcome are separate work. Therefore this
record does not claim power-loss durability, exactly-once behavior or safety after an exception that
occurs after primary replacement.

## RED and GREEN evidence

The two desired safety tests were run against the predecessor before implementation:

- Runtime RED found one queued memory job after the first store write threw.
- API RED observed raw `IOException` propagation instead of a structured response.

Implementation commit `6a4735ef` replaces the unsafe order:

- `DesktopNodeJobRuntime.CreateUnsafe` builds candidate jobs/queue and only swaps live references
  after `WriteCreateCandidateSnapshotUnsafe` returns normally.
- An injected pre-ack write failure leaves an empty state empty and preserves an already acknowledged
  job across memory, durable snapshot and restart. The rejected candidate cannot be dequeued.
- `DesktopNodeJobStoreWriteException` owns typed `PCV_JOB_STORE_SAVE_FAILED` diagnostics. The API
  maps it to HTTP 503 while retaining request ID and hiding the inner exception/path.
- The API companion proves HTTP 202 count 0, list count 0, worker `processed=false` and native adapter
  invoke count 0 for the empty-state injected failure.
- Store implementations are prohibited from re-entering mutating Runtime commands while a write is
  in progress; read-only test observation remains allowed.

The Runtime unsafe Fact and API unsafe companion were replaced 1:1 by completed migrations
`TM-JOB-CREATE-SAVE-010` and `TM-API-JOB-CREATE-SAVE-013`. Start-save, running-cancel and non-object
startup migrations remain planned.

## Verification

The deterministic .NET source snapshot for `6a4735ef` is
`6015164fa807879b4b6ffbcb49e8530fbcfa7e4673393d4eaf707d74c7dd0b12`. Raw TRX and Cobertura
results are under `artifacts/dotnet-quality-wave2a-create-preack-20260802-final`.

| Verification | Result |
|---|---|
| Solution build | PASS, warnings 0, errors 0, `-warnaserror` |
| Runtime durability owner | PASS, 42/42, skip 0 |
| Focused API `Job|Worker|Store|Cancel|Retry|Recovery` | PASS, 93/93 |
| API tests | PASS, 220/220; three repeated final-state runs |
| Full .NET solution | PASS, 684/684, skip 0 |
| Quality capture/ratchet | PASS, 684 total, skip 0, line `51.518699%`, branch `41.780458%`, mapped removed tests 2 |
| API project coverage | PASS, line `5109/9283` (`55.036087%`), branch `1690/3531` (`47.861796%`) |
| Runtime project coverage | PASS, line `733/781` (`93.854033%`), branch `218/262` (`83.206107%`) |
| Gap-registry Pester | PASS, 10/10 |
| Quality-tool Pester | PASS, 20/20 |
| Job-hardening installed-smoke contract Pester | PASS, 10/10 |
| Job-hardening dry-run | PASS, `ok=true`, `actual_execution=dry-run-no-http`, `host_mutation_performed=false` |
| Development verification PlanOnly | PASS, Release/L, 7 suites selected |
| Development verification | PASS, Release/L, 7/7 suites, `ok=true` |
| `git diff --check` | PASS |

The dry-run summary is
`artifacts/api-host-job-hardening-wave2a-create-dryrun/summary.json`. The Release/L summary is
`artifacts/development-verification-csharp-job-create-preack-wave2a-final/summary.json`; classification
reasons are `packaging-contract`, `api-cli-web-contract` and `cross-module-change`. It covers the
decision commit paths and supersedes the decision record's M/Full verification requirement; no
standalone Full/M artifact was retained before the L behavior slice. Release is a non-mutating
development preflight and does not authorize package, installed-product or actual-VM execution.

Independent Runtime, test-ledger and release-boundary reviews found no P0. Their commit-blocking
finding was an over-broad closure claim for post-replace exceptions; the registry, decision and this
record now scope completion to pre-acknowledgement failure and leave the physical indeterminate case
open. The existing-state preservation and exact 1:1 migration guards were added in response.

## Closure and next boundary

- `W0-FI-01` pre-acknowledgement create failure is `code_complete`; full `W0-FI-01` remains
  `partial-safe` until physical writer and indeterminate outcome work is complete.
- `W0-FI-02` start/cancel/complete divergence and `W0-FI-04` malformed/non-object startup remain
  unchanged and pending.
- No package candidate was produced, so promotion is `promotion_not_triggered`. Installed listener,
  CLI/Web current-card and actual VM evidence are not refreshed.
- `0.42.65-admin-smoke`, Web Console/PCVCLI, TypeScript static assets, signing and publication claims
  carry forward unchanged.
- `docs/ga-ready/EVIDENCE_INDEX.md`, current-evidence ledger/JSON/schema, control-plane index,
  verification policy, package/fullgate/current-card evidence and ADR index are intentionally unchanged.
