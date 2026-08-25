# PureCVisor Desktop Node Phase 2H Worker Pool Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a bounded worker-pool tick contract to the Desktop Node Local API spike so one listener loop can drain more than one queued job.

**Architecture:** Phase 2H keeps the deterministic PowerShell spike model and does not introduce runspaces or background threads. It adds `Invoke-PcvApiWorkerPoolTick -WorkerCount <n>`, which repeatedly calls the existing single-job worker tick up to the configured bound, preserving FIFO order, persistence saves, cancel/retry semantics, and existing `WorkerCount = 1` behavior.

**Tech Stack:** PowerShell 7 module, `HttpListener` local API spike, Pester v5 tests, in-memory FIFO queue, optional JSON job persistence.

---

## Completion Status

Phase 2H is complete. Verification evidence is recorded below.

- RED verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.WorkerPool.Tests.ps1' -Output Detailed"` initially reported 0 passed, 5 failed because `Invoke-PcvApiWorkerPoolTick` did not exist.
- Worker-pool GREEN verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.WorkerPool.Tests.ps1' -Output Detailed"` reported 5 passed, 0 failed.
- API suite verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 46 passed, 0 failed.
- Hyper-V non-integration verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"` reports 41 passed, 0 failed, 1 NotRun.
- Diff verification: `git diff --check` exits 0 with only repository line-ending warnings.

## Scope

Included:
- `Invoke-PcvApiWorkerPoolTick -WorkerCount <n>`
- `Start-PcvDesktopApi -WorkerCount <n>`
- `Invoke-PcvDesktopApi.ps1 -WorkerCount <n>`
- listener loop runs bounded worker-pool ticks after sending each HTTP response
- `WorkerCount = 1` preserves the existing single-job tick behavior
- `WorkerCount > 1` processes up to N queued jobs per pool tick in FIFO order
- pool tick returns `processed`, `processed_count`, `jobs`, and `remaining_queue`
- persisted job stores are updated by each processed job through the existing worker tick path

Excluded:
- PowerShell runspace/thread parallel execution
- interrupting running helper processes
- automatic retry policy/backoff
- worker service lifecycle management
- LAN binding
- Windows service installation

## File Map

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`: add `Invoke-PcvApiWorkerPoolTick`, `WorkerCount` parameters, listener worker-pool wiring, and export.
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`: add `-WorkerCount` parameter.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.WorkerPool.Tests.ps1`: add RED tests for bounded multi-job processing, empty queues, single-worker compatibility, and persistence.
- `spikes/purecvisor-desktop-node/api/README.md`: document `-WorkerCount`, worker-pool behavior, exclusions, and updated API test count.
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`: update Phase 2H status and roadmap.
- `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `ui/guide-content.md`: update Phase 2H references.
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2h-worker-pool.md`: record completion evidence.

## Tasks

### Task 1: RED Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.WorkerPool.Tests.ps1`

- [x] **Step 1: Write worker-pool tests**

Create tests that call `Invoke-PcvApiWorkerPoolTick -WorkerCount ...` and assert:
- `WorkerCount 2` processes exactly two queued jobs in FIFO order and leaves the third queued
- `WorkerCount 10` processes all queued jobs when fewer jobs are available
- empty queues return `processed = false`, `processed_count = 0`, and no jobs
- `WorkerCount 1` processes one job and leaves the next queued
- persisted JSON store reflects completed jobs and an empty queue after a pool tick drains queued jobs

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.WorkerPool.Tests.ps1' -Output Detailed"
```

Expected before implementation: failures because `Invoke-PcvApiWorkerPoolTick` does not exist and listener/runner do not expose `-WorkerCount`.

### Task 2: Worker Pool Implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`

- [x] **Step 1: Add pool tick function**

Add `Invoke-PcvApiWorkerPoolTick`:
- validates `WorkerCount` with `[ValidateRange(1, 64)]`
- calls `Invoke-PcvApiWorkerTick` up to `WorkerCount` times
- stops early when the queue is empty
- returns `[ordered]@{ processed; processed_count; jobs; remaining_queue }`

- [x] **Step 2: Wire listener and runner**

Add `[ValidateRange(1, 64)][int]$WorkerCount = 1` to `Start-PcvDesktopApi` and `Invoke-PcvDesktopApi.ps1`. Replace the listener's one-job tick with `Invoke-PcvApiWorkerPoolTick -WorkerCount $WorkerCount`.

- [x] **Step 3: Export the pool function**

Add `Invoke-PcvApiWorkerPoolTick` to `Export-ModuleMember`.

- [x] **Step 4: Run GREEN verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.WorkerPool.Tests.ps1' -Output Detailed"
```

Expected after implementation: 5 passed, 0 failed.

### Task 3: Docs and Final Verification

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/GUIDE.md`
- Modify: `ui/guide-content.md`
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2h-worker-pool.md`

- [x] **Step 1: Update docs**

Document Phase 2H status, `-WorkerCount`, bounded worker-pool behavior, exclusions, and updated API test count.

- [x] **Step 2: Run final verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected:
- API suite: 46 passed, 0 failed
- Hyper-V non-integration suite: 41 passed, 0 failed, 1 NotRun
- diff check exits 0 with only line-ending warnings

- [x] **Step 3: Commit and push**

Commit:

```powershell
git add -- <phase-2h-files>
git commit -m "feat: add Desktop Node worker pool tick"
git push origin main
```

## Self-Review

- Spec coverage: covers bounded worker-pool processing, listener/runner wiring, persistence, docs, and verification.
- Placeholder scan: no placeholder language remains.
- Type consistency: the new parameter/function names are consistently `WorkerCount` and `Invoke-PcvApiWorkerPoolTick`.
