# Auto Non-Mutating Regression Batch Evidence

evidence_id: auto-nonmutating-regression-batch-2026-05-09
created_at: 2026-05-09T00:55:39+09:00
scope: non-mutating-regression-batch
result: PASS
artifact_root: artifacts/batch-runs/auto-nonmutating-regression-20260509-005232
batch_id: auto-nonmutating-regression-20260509-005232
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
execution_status: pass

This evidence records the operator-approved automatic non-mutating regression batch started on 2026-05-09.

The batch used `PcvBatchSupervisor.psm1` with five read-only/regression steps. It did not run MSI install/repair/uninstall, Hyper-V mutation, firewall mutation, trust-store mutation, LAN binding, Event Log registration, service token mutation, or reboot-capable commands.

## Artifacts

- Batch artifact root: `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232`
- Manifest: `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232/manifest.json`
- Resolved manifest: `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232/batch-manifest.resolved.json`
- Summary: `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232/summary.json`
- Heartbeat: `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232/heartbeat.jsonl`
- Step results: `artifacts/batch-runs/auto-nonmutating-regression-20260509-005232/step-results`

## Batch Summary

- Batch id: `auto-nonmutating-regression-20260509-005232`
- Result: `ok=true`
- Status: `completed`
- Total steps: `5`
- Executed steps: `5`
- Failed step id: `null`
- Timed out steps: `0`

## Step Results

| Step | Result | Exit | Timeout | Duration |
| --- | --- | --- | --- | --- |
| `packaging-regression` | PASS | `0` | `false` | `127988 ms` |
| `installer-regression` | PASS | `0` | `false` | `27149 ms` |
| `web-regression` | PASS | `0` | `false` | `10028 ms` |
| `dotnet-solution-tests` | PASS | `0` | `false` | `16120 ms` |
| `git-diff-check` | PASS | `0` | `false` | `5016 ms` |

## Verification Covered

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed`: `248/248` passed
- `Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed`: `41/41` passed
- `Invoke-Pester -Path 'web/tests' -Output Detailed`: `31/31` passed
- `npm test --prefix web`: passed
- `npm run verify:parity --prefix web`: passed
- `node --check web/app.js`: passed
- `dotnet test src/DesktopNode.sln`: passed
  - `DesktopNode.Service.Tests`: `11/11` passed
  - `DesktopNode.Contracts.Tests`: `5/5` passed
  - `DesktopNode.Runtime.Tests`: `16/16` passed
  - `DesktopNode.Host.Tests`: `64/64` passed
  - `DesktopNode.Api.Tests`: `131/131` passed
- `git diff --check`: passed

## Boundary

This batch is verification evidence only. It does not claim:

- Service/MSI/Hyper-V route parity host mutation
- MSI package lifecycle mutation
- Update/rollback mutation
- Firewall mutation
- Trust-store mutation
- LAN listener mutation
- Event Log source mutation
- Public trusted signing
- External stable publication

Those scopes remain owned by their dedicated admin-smoke or preflight evidence documents.
