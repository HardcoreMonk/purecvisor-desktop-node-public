# PureCVisor Desktop Node Phase 2B Job API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first job API contract to the Desktop Node Local API spike so VM creation requests return a job id and expose stored job results.

**Architecture:** Phase 2B extends the isolated `spikes/purecvisor-desktop-node/api/` PowerShell module. The module keeps an in-memory job store, routes `POST /api/v1/vms` to `vm.create`, executes the Phase 1 helper through the existing helper bridge, and exposes `GET /api/v1/jobs/{job_id}` for status/result inspection. This remains a deterministic spike; a true background queue can replace the inline executor in a later phase without changing the API shape.

**Tech Stack:** PowerShell 7, Pester 5, in-memory hashtable job store, Phase 1 `Invoke-PcvHyperV.ps1` JSON stdin contract.

---

## Completion Status

Phase 2B is complete and ready for follow-up Phase 2C planning.

- Implemented path: `spikes/purecvisor-desktop-node/api/`
- Implemented endpoints: `POST /api/v1/vms`, `GET /api/v1/jobs/{job_id}`
- Implemented states: `queued`, `running`, `succeeded`, `failed`
- Implemented execution model: deterministic inline helper execution with persisted in-memory final job state
- Verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 14 passed, 0 failed
- Remaining Phase 2 scope: persisted jobs, cancellation, retry, background worker pools, auth, LAN mode, service install, static Web Console serving

---

## Scope Check

Phase 2B intentionally implements only the first job contract increment:

- `POST /api/v1/vms`
- `GET /api/v1/jobs/{job_id}`
- in-memory job store
- VM create payload JSON validation
- helper success/failure persisted to the job record

It does not implement persisted jobs, cancellation, retry, background worker pools, auth, LAN mode, Windows service installation, static Web Console serving, or lifecycle/checkpoint mutation endpoints.

## File Structure

```text
spikes/purecvisor-desktop-node/api/
  PcvDesktopApi.psm1
  README.md
  examples/
    vm-create.http.txt
  tests/
    PcvDesktopApi.Job.Tests.ps1
```

Responsibilities:

- `PcvDesktopApi.psm1`: job store helpers, `POST /api/v1/vms`, `GET /api/v1/jobs/{job_id}`, JSON body validation.
- `PcvDesktopApi.Job.Tests.ps1`: TDD coverage for accepted create jobs, job lookup, helper failure persistence, invalid JSON, missing body, and unknown job ids.
- `README.md`: Phase 2B endpoint and verification instructions.

## Tasks

### Task 1: Job API Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1`

- [ ] **Step 1: Write failing tests**

Create Pester tests that assert:

- `POST /api/v1/vms` returns `202` with `data.job_id` and `data.status`
- the helper receives operation `vm.create` and the request body params
- `GET /api/v1/jobs/{job_id}` returns the stored succeeded job with helper result
- helper failure is stored as a failed job
- invalid JSON returns `400` with `PCV_INVALID_JSON`
- missing body returns `400` with `PCV_REQUEST_BODY_MISSING`
- unknown job ids return `404` with `PCV_JOB_NOT_FOUND`

- [ ] **Step 2: Run tests and verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1' -Output Detailed"
```

Expected: fail because Phase 2B job helpers and routes do not exist yet.

### Task 2: Job Store And Routes

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [ ] **Step 1: Add job store helpers**

Add `Clear-PcvApiJobStore`, `New-PcvApiJob`, `Get-PcvApiJob`, `Set-PcvApiJobRunning`, `Set-PcvApiJobCompleted`, and `Convert-PcvJobToApiData`.

- [ ] **Step 2: Add JSON request parsing**

Add a parser that rejects empty bodies and invalid JSON before calling the helper.

- [ ] **Step 3: Route POST /api/v1/vms**

Create a job, run `vm.create` through the helper, store `succeeded` or `failed`, and return `202`.

- [ ] **Step 4: Route GET /api/v1/jobs/{job_id}**

Return stored job data or `PCV_JOB_NOT_FOUND`.

- [ ] **Step 5: Run API test suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: all Phase 2A and Phase 2B API tests pass.

### Task 3: Docs And Verification

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Create: `spikes/purecvisor-desktop-node/api/examples/vm-create.http.txt`
- Modify: `README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`

- [ ] **Step 1: Document Phase 2B endpoints**

Add `POST /api/v1/vms`, `GET /api/v1/jobs/{job_id}`, response examples, and exclusions.

- [ ] **Step 2: Update repository docs**

Reference Phase 2B in the root README, developer index, verification policy, and Desktop Node roadmap.

- [ ] **Step 3: Run full verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected: API tests pass, Hyper-V non-integration suite remains 41 passed / 0 failed / 1 NotRun, and `git diff --check` passes.
