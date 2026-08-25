# PureCVisor Desktop Node Phase 2C Worker Queue Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move VM create job execution out of the request path by adding an in-memory worker queue contract to the Desktop Node Local API spike.

**Architecture:** Phase 2C extends `spikes/purecvisor-desktop-node/api/` with a FIFO job queue and worker tick. `POST /api/v1/vms` creates a queued job and returns immediately without invoking the Hyper-V helper. `Invoke-PcvApiWorkerTick` processes one queued job at a time, transitions it through `running`, and stores the final helper result as `succeeded` or `failed`.

**Tech Stack:** PowerShell 7, Pester 5, in-memory hashtable job store, in-memory FIFO queue, Phase 1 `Invoke-PcvHyperV.ps1` JSON stdin contract.

---

## Completion Status

Phase 2C is complete and ready for follow-up Phase 2D planning.

- Implemented path: `spikes/purecvisor-desktop-node/api/`
- Implemented behavior: request path enqueue, worker path helper execution
- Implemented worker contract: `Invoke-PcvApiWorkerTick` processes one FIFO job per tick
- Verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 18 passed, 0 failed
- Remaining Phase 2 scope: persisted jobs, cancellation, retry, parallel worker pools, auth, LAN mode, service install, static Web Console serving

---

## Scope Check

Phase 2C intentionally implements only the first worker queue increment:

- `POST /api/v1/vms` returns a queued job without calling the helper
- FIFO in-memory job queue
- one-job-at-a-time worker tick
- success/failure result persistence in the existing in-memory job record
- listener loop may opportunistically tick the worker after sending a response

It does not implement persisted jobs, cancellation, retry, parallel worker pools, auth, LAN mode, Windows service installation, static Web Console serving, or lifecycle/checkpoint mutation endpoints.

## File Structure

```text
spikes/purecvisor-desktop-node/api/
  PcvDesktopApi.psm1
  README.md
  tests/
    PcvDesktopApi.Job.Tests.ps1
    PcvDesktopApi.Worker.Tests.ps1
```

Responsibilities:

- `PcvDesktopApi.psm1`: enqueue jobs, worker tick, FIFO queue state, request path no longer executes helper.
- `PcvDesktopApi.Job.Tests.ps1`: assert POST returns queued and GET initially shows queued.
- `PcvDesktopApi.Worker.Tests.ps1`: assert worker success, failure, FIFO ordering, and empty queue behavior.
- `README.md`: Phase 2C worker behavior and remaining exclusions.

## Tasks

### Task 1: Worker Queue Tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1`
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1`

- [ ] **Step 1: Update POST job tests**

Change POST expectations so `POST /api/v1/vms` returns `queued` and does not call the helper during the request.

- [ ] **Step 2: Add worker tests**

Create Pester tests that assert:

- `Invoke-PcvApiWorkerTick` processes one queued job and stores a succeeded result
- helper failures are stored as failed jobs
- multiple queued jobs process FIFO
- ticking an empty queue returns `processed = false`

- [ ] **Step 3: Run tests and verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1' -Output Detailed"
```

Expected: fail because Phase 2B still executes helper inline and worker tick does not exist.

### Task 2: Queue And Worker Implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [ ] **Step 1: Add queue state**

Add a script-scope FIFO queue and reset it in `Clear-PcvApiJobStore`.

- [ ] **Step 2: Enqueue POST jobs**

Change `POST /api/v1/vms` to create a queued job, enqueue it, and return `202` without helper execution.

- [ ] **Step 3: Add worker tick**

Add `Invoke-PcvApiWorkerTick` that dequeues one queued job, sets it running, invokes the helper, stores completion, and returns a compact worker result.

- [ ] **Step 4: Keep listener responsive enough for the spike**

After sending an HTTP response, let `Start-PcvDesktopApi` opportunistically run one worker tick unless disabled in a future test hook. The client receives `202` before helper execution starts.

- [ ] **Step 5: Run API test suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: all API tests pass.

### Task 3: Docs And Verification

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`

- [ ] **Step 1: Document Phase 2C behavior**

Document that POST returns queued immediately, worker execution is in-memory, and persistence/retry/cancel remain future work.

- [ ] **Step 2: Update repository docs**

Reference Phase 2C in the root README, developer index, verification policy, public boundary, and Desktop Node roadmap.

- [ ] **Step 3: Run full verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected: API tests pass, Hyper-V non-integration suite remains 41 passed / 0 failed / 1 NotRun, and `git diff --check` passes.
