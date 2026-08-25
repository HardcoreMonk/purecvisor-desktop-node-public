# C# architecture improvement Wave 1D ops dispatch owner evidence (2026-08-02)

## Evidence boundary

- Plan: `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- Audit base commit: `2e98ff4f2df250c36700e86ace0db46ef0aca420`
- Wave 1D predecessor commit: `e5a3dd0d3001991b3d56415a8ab21f2c6aa21728`
- Verification source commit: `894a14030faed92e93f6902aecb5f84ae4982a3b`
- Working branch: `codex/csharp-aspnet-core-improvement`
- Change classification: `M / Full`
- Evidence type: code-level behavior-preserving ops-summary query/dispatch ownership move
- Implementation status: `code_complete` (Wave 1D ops dispatch only)
- Promotion status: `promotion_not_triggered`
- Product source changed: `true`
- Product behavior changed: `false`
- `host_mutation_performed`: `false`
- Package build performed: `false`
- Installed product changed: `false`
- Actual-VM/admin smoke: not run; this slice produced no package candidate
- Package/current operational evidence anchor changed: `false`
- Operational anchor: `0.42.65-admin-smoke` / `full-admin-host-mutation-gate-20260716-04265`
- Operational MSI SHA-256: `9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30`
- Operational payload aggregate SHA-256: `5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28`
- Operational provenance commit: `4855947fe0199cedc978e8b40ffb45e96ced6876`
- `public_trusted_signing`: `false`
- `external_stable_publication`: `false`

This record closes the Wave 1D ops dispatch slice only. It does not introduce ASP.NET Core; the
product HTTP transport remains `System.Net.HttpListener`. Web Console and PCVCLI remain the active
operator surfaces, and the TypeScript Web Console build/runtime boundary is unchanged.

## Characterization and delete-test decision

Commit `b4f7d4d` first fixed the pre-move route contract:

- native reads occur in the order `host.status` then `vm.list`;
- both operations receive `{}` and the route timeout cancellation token;
- the response remains HTTP 200, `application/json`, `ops.summary`, and the outer processor attaches
  `request_id`;
- the top-level and data JSON key sets, runtime policy, VM/job counts and unconfigured evidence
  projection remain stable;
- a non-ops route performs no ops native query.

The delete test found two shallow layers: a static one-callback route wrapper in
`DesktopNodeApiRuntimeCoreHandlers.cs` and a custom-delegate `DesktopNodeApiOpsSummaryDataBuilder`
that only invoked two reads before forwarding to the pure builder. Commit `894a140` selected
`merge + deepen`:

- `DesktopNodeApiOpsSummaryHandler` is now a sealed instance route/envelope owner;
- `IDesktopNodeApiOpsSummaryQuery` is the explicit test/query seam;
- `DesktopNodeApiOpsSummaryQuery` directly owns job snapshot ordering, runtime-policy snapshot,
  sequential host/VM reads and evidence-reader invocation without `Func<>`, custom delegate or
  processor back-reference;
- `DesktopNodeApiRequestProcessor` retains one `private readonly` handler and one
  `TryHandle(method, path, cancellationToken)` dispatch call;
- the former static ops wrapper, custom operation delegate, data builder and processor
  `BuildOpsSummary` helper are deleted;
- `DesktopNodeApiOpsSummaryBuilder` remains the deep, pure projection owner;
- `BatchEvidenceSummaryReader` internals, path containment, selection and redaction are unchanged
  and remain Wave 7 scope.

The same deletion audit classified the remaining job and console callback wrappers as `deepen`
candidates. The job follow-up should own API route/status/envelope behavior above the existing
Runtime state owner; the console follow-up should own options, route-ID decode and capability/session
projection. They were not mixed into this ops-specific commit.

## Direct owner and compiled ownership guards

The owner fixture proves:

- a nonmatching route reads the query zero times;
- a matching route reads it exactly once and passes the exact cancellation token;
- the direct handler response has no `request_id`, leaving that concern to the processor façade;
- the production query preserves job projection, runtime policy, diagnostics root, `{}` native
  parameters and ordered reads;
- native host/VM failures remain a successful degraded ops response with `data.errors` and warning
  signals;
- evidence `not_configured`, `available`, `degraded`, `missing` and `unavailable` states preserve
  the current-evidence status and established signal tone.

`ApiArchitectureOwnershipTests.OpsSummaryProjectionUsesDedicatedOwner` reads compiled metadata and
IL. It proves that the processor has exactly one readonly ops owner, no evidence-reader field,
`BuildOpsSummary` method or direct projection/evidence call; the handler and query are sealed,
callback-free and have no processor reference; the handler calls the query seam and pure builder;
and the retired data-builder/delegate types are absent. It does not use private reflection or
production source-text inspection.

The removed source-text test is mapped to that exact compiled guard by completed migration
`TM-SOURCE-OPS-007`. The live production source-text inventory therefore decreases from three to
two without an unmapped test removal. Batch-evidence path/sort/CWD test migrations remain assigned
to Wave 7.

## Preserved behavior and ordering

The following behavior is unchanged:

1. request ID normalization, global `sync` serialization and rate limit run first;
2. GET route timeout creates the cancellation token used by both native reads;
3. auth route handling and account authorization run before the job-store pre-gate;
4. unsupported future job-store schema blocks the ops route before query/native/evidence reads;
5. the existing job, diagnostics, queued mutation, runtime-policy and console dispatch order remains
   ahead of ops dispatch;
6. jobs are ordered by `UpdatedAt` descending then `CreatedAt` descending;
7. runtime policy is captured before `host.status`, then `vm.list`, then batch evidence is read;
8. native failures or missing/malformed/degraded evidence do not change the route from HTTP 200;
9. response JSON keys, 54-route registry, `PCV_*` errors, timeout/rate-limit behavior, JSON job-store
   schema and single mutation worker are unchanged.

There is no shadow execution, parallel host/VM query, automatic retry or fallback PowerShell
runtime path.

## Verification

The deterministic .NET source snapshot for source commit `894a140` is
`f658b3e12fbedd67044b7633dc83a928574783ffc02d9ee67bf8c3bcc2eccbcf`. Raw TRX and Cobertura
results are stored under
`artifacts/dotnet-quality-wave1d-ops-dispatch-owner-20260802-final`.

| Verification | Result |
|---|---|
| Solution build | PASS, warnings 0, errors 0, `-warnaserror` |
| Ops characterization/owner/architecture tests | PASS, 12/12 |
| API tests | PASS, 220/220; three repeated final-state runs |
| Runtime tests | PASS, 42/42, skip 0 |
| Contracts tests | PASS, 15/15, skip 0 |
| Host tests | PASS, 162/162 through full solution |
| Full .NET solution | PASS, 684/684, skip 0 |
| Quality capture/ratchet | PASS, 684 total, skip 0, line `51.410248%`, branch `41.696238%`, mapped removed test 1 |
| API project coverage | PASS, line `5068/9243` (`54.830683%`), branch `1687/3529` (`47.803910%`) |
| Ops handler/query file coverage | PASS, line `79/79` (`100%`), branch `7/8` (`87.5%`) |
| Ops owner plus pure projection coverage | PASS, line `397/417` (`95.203837%`), branch `128/172` (`74.418605%`) |
| Gap-registry Pester | PASS, 10/10 |
| 54-route/API JSON/request serialization regression | PASS through API, Host and full verification |
| Independent implementation/test/release audits | PASS; remaining commit-blocking findings 0 |
| Development verification | PASS, Full/M, 7/7 suites, `ok=true` |
| `git diff --check` | PASS |

The Full/M summary is
`artifacts/development-verification-csharp-ops-dispatch-owner-wave1d-20260802-final/summary.json`.
It uses the actual committed paths since Wave 1C closure and passes .NET, Web npm, packaging Pester,
installer Pester, Web Pester, diff and current-evidence checks. This is a non-mutating development
preflight, not package, installed-product or actual-VM evidence.

The current C# inventory is 81 product files / 24,972 physical LOC and 44 test files / 19,255
physical LOC. `DesktopNodeApiRequestProcessor.cs` is 2,407 lines, the new query/handler file is 152
lines, and the retained pure projection builder is 500 lines.

## Closure and next boundary

- Wave 1D ops dispatch is `code_complete`; no package candidate was produced, so promotion is
  `promotion_not_triggered`.
- `0.42.65-admin-smoke`, Web Console/PCVCLI, TypeScript static assets and the internal/private
  network boundary carry forward unchanged.
- Installed CLI/Web ops-summary/current-card evidence is not refreshed by this source-only record.
  A package candidate containing this API payload, or a later ops-summary/batch-evidence/current-card
  contract change, is the stale rerun trigger.
- Wave 2A job durability/recovery is the next planned development wave. Job/console callback-free
  owner slices remain separate, and evidence-reader internals remain Wave 7 scope.
- `docs/ga-ready/EVIDENCE_INDEX.md`, current-evidence ledger/JSON/schema, control-plane index, ADR
  index, verification policy, package/fullgate/current-card evidence, signing claim and publication
  claim are intentionally unchanged.
