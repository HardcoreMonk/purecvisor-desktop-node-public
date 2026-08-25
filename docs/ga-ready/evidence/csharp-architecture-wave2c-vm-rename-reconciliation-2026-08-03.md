# C# architecture Wave 2C `vm.rename` reconciliation evidence

- evidence status: `PASS` (code-level only)
- decision: `wave2c-vm-rename-reconciliation-v1`
- date: `2026-08-03`
- change tier: `L / Release`
- current operational anchor: `0.42.65-admin-smoke` (carry-forward)
- package candidate created: `false`
- promotion: `promotion_not_triggered`
- host mutation performed: `false`
- Hyper-V mutation performed: `false`
- actual VM validation performed: `false`
- public trusted signing: `false`
- external stable publication: `false`

## Implemented contract

The slice adds durable `pcv-vm-rename-reconciliation/v1` baseline metadata to queued
`vm.rename` jobs and an additive `POST /api/v1/jobs/{jobId}/reconcile` route. The reconcile
route is `ReconcileJob`, `jobs` family, `operate` permission and product-operation stance.
It calls only provider `vm.list` readback. A unique target row with a matching pre-state
fingerprint commits the existing job to `succeeded` with `action=reconciled`; all uncertain,
missing-baseline or unavailable-readback cases return `409 PCV_JOB_RECONCILIATION_REQUIRED`
and leave the job `failed`.

Runtime observations `job-reconciled` and `job-reconciliation-required` are retained in the
existing recent-event/diagnostics/ops-summary bridge. No new public job status was introduced.
Web Console and PCVCLI expose the same `operate`-gated action and route.

## Verification

| check | result |
|---|---|
| `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --no-restore --filter FullyQualifiedName~JobRuntimeReconciliationTests` | PASS (2/2) |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore --filter FullyQualifiedName~VmRename` | PASS (5/5) |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore --filter FullyQualifiedName~ApiHandlerAdapterContractTests` | PASS (13/13) |
| `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter FullyQualifiedName~DesktopNodeCliCommandCatalogTests` | PASS (68/68) |
| `npm test --prefix web` | PASS |
| `npm run verify:parity --prefix web` | PASS |
| `dotnet test src/DesktopNode.sln --no-restore --configuration Release` | PASS (801 tests, skip 0) |
| `PcvWave2CVmRenameReconciliation.Tests.ps1` | PASS (code-level fixture) |

The existing 55-route API snapshot and `http-transport-contract-v1` route count were updated
for the intentional additive reconcile route. Web `app.js` was regenerated from TypeScript and
static/browser parity remained green.

## Explicit non-claims

This evidence does not claim that an actual Hyper-V rename, host mutation, installed service
smoke, MSI/package lifecycle, full-admin gate or public release signing was executed. A separate
administrator-approved operational plan is required before actual VM reconciliation or package
promotion.
