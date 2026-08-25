# PureCVisor Desktop Node Phase 2E Job Control Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add minimal job control to the Desktop Node Local API spike: queued job cancellation and failed job retry.

**Architecture:** Phase 2E extends `spikes/purecvisor-desktop-node/api/` with job-control routes that mutate the existing in-memory/persisted job store. Cancellation is limited to queued jobs because the current synchronous worker cannot interrupt a running Hyper-V helper process. Retry creates a new queued job that references the failed source job, preserving failure history.

**Tech Stack:** PowerShell 7 module, `HttpListener` local API spike, JSON job store, Pester v5 tests.

---

## Completion Status

Phase 2E is complete and ready for follow-up Phase 2F planning.

- Commit status: implementation complete in the working tree; commit and push remain
- RED verification: `PcvDesktopApi.JobControl.Tests.ps1` initially reported 0 passed, 6 failed because job control routes returned 404
- GREEN verification: `PcvDesktopApi.JobControl.Tests.ps1` reported 6 passed, 0 failed
- API suite verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 29 passed, 0 failed
- Hyper-V non-integration verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"` reports 41 passed, 0 failed, 1 NotRun
- Diff check: `git diff --check` exits 0 with only CRLF normalization warnings
- Remaining Phase 2 scope: parallel worker pools, auth, LAN mode, service install, static Web Console serving

## Scope

Included:
- `POST /api/v1/jobs/{job_id}/cancel`
- `POST /api/v1/jobs/{job_id}/retry`
- queue removal for canceled jobs
- persisted JSON update for cancellation and retry
- API docs and roadmap updates

Excluded:
- interrupting a running helper process
- automatic retry policy/backoff
- parallel worker pools
- authentication, LAN mode, Windows service installation, static Web Console serving

## File Map

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`: add job control helpers and route handling.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1`: add RED tests for cancel/retry behavior.
- `spikes/purecvisor-desktop-node/api/README.md`: document new endpoints and updated test count.
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`: update current status and roadmap.
- `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `ui/guide-content.md`: update Phase 2E references.

## Tasks

### Task 1: RED Tests

- [x] **Step 1: Add job-control tests**

Create `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1` with tests for:
- canceling a queued job
- rejecting cancellation for completed jobs
- retrying a failed job as a new queued job
- rejecting retry for non-failed jobs
- persisting cancel/retry mutations when `-JobStorePath` is supplied

- [x] **Step 2: Run the new test file and verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1' -Output Detailed"
```

Expected before implementation: failures because the routes do not exist.

### Task 2: GREEN Implementation

- [x] **Step 1: Add job control helpers**

Add helpers in `PcvDesktopApi.psm1`:
- `Remove-PcvApiJobFromQueue`
- `Cancel-PcvApiJob`
- `Retry-PcvApiJob`

- [x] **Step 2: Route POST job control endpoints**

Handle:
- `POST /api/v1/jobs/{job_id}/cancel`
- `POST /api/v1/jobs/{job_id}/retry`

Persist mutations through `Save-PcvApiJobStore -Path $JobStorePath`.

- [x] **Step 3: Run new tests and full API suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected after implementation: all API tests pass.

### Task 3: Docs and Verification

- [x] **Step 1: Update docs**

Document Phase 2E endpoints, status, exclusions, and test count.

- [x] **Step 2: Run final verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

- [ ] **Step 3: Commit and push**

Commit:

```powershell
git add -- <phase-2e-files>
git commit -m "feat: add Desktop Node job controls"
git push origin main
```
