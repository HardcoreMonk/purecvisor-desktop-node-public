# PureCVisor Desktop Node Phase 2D Persisted Jobs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persist Desktop Node Local API jobs to a JSON file so queued and completed jobs survive process restarts.

**Architecture:** Phase 2D extends `spikes/purecvisor-desktop-node/api/` with an optional `-JobStorePath`. The API module serializes the job map and FIFO queue to JSON whenever jobs are created or completed, and `Initialize-PcvApiJobStore` loads that JSON back into memory. Corrupt job store files are quarantined and the process starts with an empty store instead of crashing.

**Tech Stack:** PowerShell 7, Pester 5, JSON file persistence, in-memory hashtable job store, in-memory FIFO queue.

---

## Completion Status

Phase 2D is complete and ready for follow-up Phase 2E planning.

- Implemented path: `spikes/purecvisor-desktop-node/api/`
- Implemented behavior: JSON job store save/load and queue restoration
- Implemented safety: corrupt store quarantine with empty-store recovery
- Verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 23 passed, 0 failed
- Remaining Phase 2 scope: cancellation, retry, parallel workers, auth, LAN mode, service install, static Web Console serving

---

## Scope Check

Phase 2D intentionally implements only JSON file persistence:

- optional `-JobStorePath` for request handling, worker ticks, and listener startup
- job creation persists queued jobs
- worker completion persists final state
- API startup can load previous jobs and queued work
- corrupt store files are moved aside and a new empty store is used

It does not implement cancellation, retry, parallel workers, auth, LAN mode, Windows service installation, static Web Console serving, or database-backed persistence.

## File Structure

```text
spikes/purecvisor-desktop-node/api/
  PcvDesktopApi.psm1
  Invoke-PcvDesktopApi.ps1
  README.md
  tests/
    PcvDesktopApi.Persistence.Tests.ps1
```

Responsibilities:

- `PcvDesktopApi.psm1`: JSON job store load/save/quarantine, `-JobStorePath` propagation.
- `Invoke-PcvDesktopApi.ps1`: accepts `-JobStorePath` and initializes the store before listening.
- `PcvDesktopApi.Persistence.Tests.ps1`: verifies persistence, reload, worker completion save, queued job recovery, and corrupt file quarantine.
- `README.md`: documents the persistence option and remaining exclusions.

## Tasks

### Task 1: Persistence Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Persistence.Tests.ps1`

- [ ] **Step 1: Write failing persistence tests**

Create Pester tests that assert:

- `POST /api/v1/vms -JobStorePath <path>` writes a JSON store file containing the queued job
- `Initialize-PcvApiJobStore -Path <path>` loads a previous job so `GET /api/v1/jobs/{job_id}` can return it
- `Invoke-PcvApiWorkerTick -JobStorePath <path>` persists `succeeded` job state
- queued jobs loaded from disk are processed by the worker after restart
- corrupt JSON is quarantined and a new empty store is used

- [ ] **Step 2: Run tests and verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Persistence.Tests.ps1' -Output Detailed"
```

Expected: fail because persistence functions and `-JobStorePath` parameters do not exist yet.

### Task 2: Persistence Implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`

- [ ] **Step 1: Add store helpers**

Add `Initialize-PcvApiJobStore`, `Save-PcvApiJobStore`, `Get-PcvApiJobStoreSnapshot`, and queue serialization helpers.

- [ ] **Step 2: Save after mutations**

Save after job creation, worker state changes, and completion when `-JobStorePath` is supplied.

- [ ] **Step 3: Load on startup**

Runner accepts `-JobStorePath`, initializes the job store, and passes the path to request handling and worker ticks.

- [ ] **Step 4: Run API tests**

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

- [ ] **Step 1: Document Phase 2D behavior**

Document `-JobStorePath`, JSON persistence, corrupt file quarantine, and remaining exclusions.

- [ ] **Step 2: Update repository docs**

Reference Phase 2D in the root README, developer index, verification policy, public boundary, and Desktop Node roadmap.

- [ ] **Step 3: Run full verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected: API tests pass, Hyper-V non-integration suite remains 41 passed / 0 failed / 1 NotRun, and `git diff --check` passes.
