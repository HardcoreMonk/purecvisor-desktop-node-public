# C# architecture Wave 2C `vm.delete` reconciliation evidence

- evidence status: `PASS` (code-level only)
- decision: `wave2c-vm-delete-reconciliation-v1`
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

Queued `vm.delete` jobs now store durable `pcv-vm-delete-reconciliation/v1` metadata from a
read-only `vm.list` preflight. Capture requires exactly one named row, the
`managed_by_purecvisor=true` marker and a stable VM id; unavailable ownership or identity is
recorded without changing the existing `202` enqueue semantics.

The additive `POST /api/v1/jobs/{jobId}/reconcile` route accepts only failed interrupted
`vm.delete` jobs. It calls provider `vm.list` readback and never calls `vm.delete`. A captured
managed before-state plus an absent target row commits the existing job to `succeeded` with
`action=reconciled`. A remaining same-id row, recreated identity, unmanaged collision, duplicate
name or unavailable readback returns `409 PCV_JOB_RECONCILIATION_REQUIRED` and leaves the job
`failed`.

Runtime observations `job-reconciled` and `job-reconciliation-required` remain in the existing
recent-event/diagnostics/ops-summary bridge. No new public job status was introduced. Web Console
shows `Reconcile delete` under `operate`; PCVCLI continues to use `job reconcile <job_id>`.

## Verification

| check | result |
|---|---|
| `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --no-restore --configuration Release --filter FullyQualifiedName~JobRuntimeReconciliationTests` | PASS (4/4) |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore --configuration Release --filter FullyQualifiedName~VmDelete` | PASS (5/5) |
| `npm test --prefix web` | PASS |
| `Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -PassThru` | PASS (48/48) |
| `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1 -PassThru` | PASS (4/4) |
| `dotnet test src/DesktopNode.sln --no-restore --configuration Release` | PASS (806 tests, skip 0) |
| `npm run verify:parity --prefix web` | PASS (static parity + browser fixture) |
| `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1 -PassThru` | PASS (4/4, regression) |

The existing 55-route registry remains authoritative. The Web served asset was regenerated from
TypeScript and static/browser parity remains green. No operational package candidate is opened by
this slice.

## Explicit non-claims

This evidence does not claim that an actual Hyper-V delete, host mutation, installed service smoke,
MSI/package lifecycle, full-admin gate or public release signing was executed. A separate
administrator-approved operational plan is required before actual VM reconciliation or package
promotion.
