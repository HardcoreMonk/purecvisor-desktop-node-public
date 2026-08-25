# C# architecture Wave 2C `checkpoint.create` reconciliation evidence

- evidence status: `PASS` (code-level only)
- decision: `wave2c-checkpoint-create-reconciliation-v1`
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

Queued `checkpoint.create` jobs now store durable `pcv-checkpoint-create-reconciliation/v1`
metadata from a read-only `checkpoint.list` preflight. A requested checkpoint name must be absent
for the scoped VM to produce `capture_status=captured`; an existing or ambiguous name and any
unavailable readback remain queued with `capture_status=unavailable`.

The existing `POST /api/v1/jobs/{jobId}/reconcile` route accepts only failed interrupted
`checkpoint.create` jobs with a captured absent pre-state. It calls provider `checkpoint.list` and
never calls `checkpoint.create`. Exactly one matching VM/name row commits the existing job to
`succeeded` with `action=reconciled`; zero, duplicate, missing-baseline and unavailable-readback
cases return `409 PCV_JOB_RECONCILIATION_REQUIRED` and leave the job `failed`.

`checkpoint.restore` remains excluded because checkpoint presence cannot prove restored VM data or
state. Runtime observations remain in the recent-event/diagnostics/ops-summary bridge. Web Console
shows `Reconcile checkpoint` under `operate`; PCVCLI continues to use `job reconcile <job_id>`.

## Verification

| check | result |
|---|---|
| `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --no-restore --configuration Release --filter FullyQualifiedName~JobRuntimeReconciliationTests` | PASS (6/6) |
| `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore --configuration Release --filter FullyQualifiedName~CheckpointCreate` | PASS (2/2) |
| `dotnet test src/DesktopNode.sln --no-restore --configuration Release` | PASS (810/810, skipped 0) |
| `npm test --prefix web` | PASS |
| `npm run verify:parity --prefix web` | PASS (served asset, static parity, browser fixture) |
| `Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -PassThru` | PASS (48/48) |
| `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1 -PassThru` | PASS (4/4) |
| `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1,packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1 -PassThru` | PASS (8/8) |

The existing 55-route registry remains authoritative. The Web served asset was regenerated from
TypeScript and static/browser parity remains green. No operational package candidate is opened by
this slice.

## Explicit non-claims

This evidence does not claim that an actual Hyper-V checkpoint create/delete/restore, host mutation,
installed service smoke, MSI/package lifecycle, full-admin gate or public release signing was
executed. A separate administrator-approved operational plan is required before actual VM
reconciliation or package promotion.
