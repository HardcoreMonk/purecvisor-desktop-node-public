# C# architecture improvement Wave 2A physical job-store durability evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Decision: `docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`
- Gap registry: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Predecessor pre-ack implementation: `6a4735ef419f584dd1d4c223a90e837073a5744f`
- Physical implementation commit: `4d3a0d9782ee5e40fc35df51f44a36bf04a15034`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `L / Release`
- Evidence type: code-level persistence/restart behavior and product lifecycle guard change
- Implementation status: `code_complete` (`W0-FI-01` create protocol under one product SCM Runtime owner)
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

This record does not introduce ASP.NET Core. `System.Net.HttpListener`, TypeScript Web Console,
PCVCLI and the internal/private-network product boundary remain unchanged.

## RED and GREEN

The physical baseline RED occupied fixed `jobs.json.tmp` with a directory. The predecessor writer
used that fixed staging name and failed with `UnauthorizedAccessException`; it had no durable
restart-readable record for a failure after primary replacement.

Implementation commit `4d3a0d9` makes the physical outcome explicit:

1. write UTF-8 candidate bytes to `jobs.json.tmp.<GUID-N>` with exclusive `CreateNew` and
   `FileStream.Flush(true)`;
2. write candidate/previous length and SHA-256 identity to
   `jobs.json.commit-pending.tmp.<GUID-N>`, flush it and rename it to fixed
   `jobs.json.commit-pending`;
3. replace the primary only after the fixed marker exists;
4. compare exact primary bytes/identity and return typed `Committed`, `NotCommitted` or
   `Indeterminate`;
5. publish create memory/queue/HTTP success only for `Committed`;
6. retain unresolved marker/load block and prevent job mutation/dispatch until current-binary
   restart reconciliation succeeds.

Pre-marker publication failure is `NotCommitted`. Candidate match after an injected post-replace
failure is reconciled as `Committed`; previous match is `NotCommitted`; inaccessible/malformed or
identity-mismatched state remains blocked. Primary/marker access failure is not treated as absence.
Unique temp, legacy fixed temp and backup are never promoted at startup.

`IDesktopNodeJobStore.WriteSnapshot` changed from `void` to a typed result. This is a source/binary
interface change, but the seam is internal to this repository's Runtime/API/tests and is not an
externally published stable API. External implementation compatibility is not claimed.
`PCV_JOB_STORE_LOAD_FAILED` is an additive startup/read diagnostic in the existing HTTP 409 error
envelope; route shape and retry semantics are unchanged.

## Restart and product lifecycle guard

- `DesktopNodeJobRuntime` returns redacted HTTP 503 for an indeterminate create, blocks later job
  routes and returns worker/native invocation count 0.
- Startup reads the fixed marker and removes it only when primary matches candidate or previous
  length/SHA-256. Invalid or inaccessible state produces a structured load block.
- Native Host `job-store-migration-apply` checks service stopped and marker absence before backup or
  rewrite. Preserve-data native removal performs the same check before deleting the current service.
- Product Update checks after stop+wait and before backup. Every automatic/explicit restore performs
  stop+wait and an immediate marker recheck before restoring an older product root.
- Preserve-data `RemoveInstalled` and `Uninstall` stop+wait, then block before service/product removal
  if a marker exists or cannot be inspected.
- Explicit RemoveData removes only exact `jobs.json`, fixed marker, legacy `jobs.json.tmp` and the two
  GUID-N temp patterns. Near-miss filenames remain untouched.

The operator procedure is in `docs/OPERATIONS_GUIDE.md`. It prohibits marker deletion/editing,
orphan promotion, stale backup restore, blind request resubmit and marker-unaware old-binary start.

## Verification

The deterministic .NET source snapshot for `4d3a0d9` is
`5c2f30e21f1497b7d5b9373a2ed148651061a30f7c82fa44853dd354f3f75aa2`. Raw TRX and Cobertura
results are under `artifacts/dotnet-quality-wave2a-physical-writer-20260802-final`.

| Verification | Result |
|---|---|
| Solution build | PASS, warnings 0, errors 0, `-warnaserror` |
| Runtime durability owner | PASS, 55/55, skip 0 |
| Physical Json store owner | PASS, 12 methods/14 cases |
| Focused API `Job|Worker|Store|Cancel|Retry|Recovery` | PASS, 94/94 |
| API tests | PASS, 221/221; three repeated final-state runs |
| Host tests | PASS, 164/164 |
| Full .NET solution | PASS, 700/700, skip 0 |
| Product Plan/Invoke Pester | PASS, 26/26 + 61/61 = 87/87 |
| Quality capture/ratchet | PASS, 700 total, skip 0, line `51.492417%`, branch `41.561001%`, mapped removed tests 2 |
| API project coverage | PASS, line `5271/9590` (`54.963504%`), branch `1722/3653` (`47.139338%`) |
| Runtime project coverage | PASS, line `1001/1088` (`92.003676%`), branch `311/384` (`80.989583%`) |
| Host project coverage | PASS, line `6310/14160` (`44.562147%`), branch `1703/5288` (`32.204992%`) |
| Gap-registry Pester | PASS, 10/10 |
| Quality-tool Pester | PASS, 20/20 |
| Job-hardening installed-smoke contract Pester | PASS, 10/10 |
| Job-hardening dry-run | PASS, `ok=true`, `actual_execution=dry-run-no-http`, `host_mutation_performed=false` |
| Development verification PlanOnly | PASS, Release/L, 7 suites selected |
| Development verification | PASS, Release/L, 7/7 suites, `ok=true` |
| `git diff --check` | PASS |

The dry-run summary is
`artifacts/api-host-job-hardening-wave2a-physical-writer-dryrun/summary.json`. The Release/L summary is
`artifacts/development-verification-csharp-job-physical-writer-wave2a-final/summary.json`; classification
reasons are `current-evidence-anchor`, `packaging-contract` and `cross-module-change`. All seven selected
suites passed. Release is a non-mutating development preflight; it does not authorize package,
installed-product or actual-VM execution.

## Closure and remaining boundary

- `W0-FI-01` create/save failure is `code_complete` only under the product's one SCM Runtime owner.
- A static per-call lock is not a lifetime lease. Two Runtime instances can load the same old state
  and sequentially overwrite a previously acknowledged job. Path-scoped lifetime lease or revision/CAS
  remains required before the plan's single-writer item can close.
- `W0-FI-02` start/cancel/complete divergence and `W0-FI-04` malformed/non-object/semantic integrity
  remain unchanged and pending.
- The compatibility test is shape-level. No actual frozen 0.42.65 binary reader was executed, so
  downgrade reader and rollback semantic closure remain open.
- No directory-fsync/controller-cache/power-loss or Hyper-V exactly-once claim is made.
- No package candidate was produced. Installed listener, Web/CLI current-card and actual VM evidence
  were not refreshed; `0.42.65-admin-smoke` remains the operational anchor.
