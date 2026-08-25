# PureCVisor Desktop Node Phase 2A Local API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first Local API daemon spike for PureCVisor Desktop Node, exposing localhost HTTP endpoints that call the Phase 1 Hyper-V helper contract.

**Architecture:** The spike stays isolated under `spikes/purecvisor-desktop-node/api/`. A PowerShell module owns route dispatch, helper process invocation, JSON response mapping, and loopback prefix validation. A thin runner script starts an `HttpListener` on `127.0.0.1` and delegates every request to the module.

**Tech Stack:** PowerShell 7, `System.Net.HttpListener`, Pester 5, Phase 1 `Invoke-PcvHyperV.ps1` JSON stdin contract.

---

## Completion Status

Phase 2A is complete and ready for follow-up Phase 2B planning.

- Implemented path: `spikes/purecvisor-desktop-node/api/`
- Implemented endpoints: `GET /api/v1/host/status`, `GET /api/v1/vms`
- Implemented guard: loopback-only listener prefixes
- Implemented backend bridge: Phase 1 helper process invocation through JSON stdin/stdout
- Verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 9 passed, 0 failed
- Remaining Phase 2 scope: job queue, long-running `POST /api/v1/vms`, auth, LAN mode, service install, static Web Console serving

---

## Scope Check

Phase 2A intentionally implements only the first daemon increment:

- `GET /api/v1/host/status`
- `GET /api/v1/vms`
- loopback-only HTTP listener guard
- helper execution through JSON stdin
- structured API success/failure responses

It does not implement auth, LAN mode, Windows service installation, static Web Console serving, CLI, persistent job queue, or long-running `POST /api/v1/vms`.

## File Structure

```text
spikes/purecvisor-desktop-node/api/
  README.md
  Invoke-PcvDesktopApi.ps1
  PcvDesktopApi.psm1
  examples/
    host-status.http.txt
    vm-list.http.txt
  tests/
    PcvDesktopApi.Contract.Tests.ps1
    PcvDesktopApi.Helper.Tests.ps1
```

Responsibilities:

- `PcvDesktopApi.psm1`: route dispatch, response helpers, loopback prefix validation, helper process invocation.
- `Invoke-PcvDesktopApi.ps1`: starts the local `HttpListener` and writes JSON responses.
- `tests/*.Tests.ps1`: non-network contract tests for routing, helper invocation, error mapping, and loopback guard.
- `README.md`: run and verification instructions.

## Tasks

### Task 1: Contract Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`

- [ ] **Step 1: Write failing routing tests**

Create tests that import `PcvDesktopApi.psm1`, call `Invoke-PcvApiRequest`, and assert:

- `GET /api/v1/host/status` calls helper operation `host.status`
- `GET /api/v1/vms` calls helper operation `vm.list`
- unsupported routes return HTTP 404 with `PCV_ROUTE_NOT_FOUND`
- unsupported methods return HTTP 405 with `PCV_METHOD_NOT_ALLOWED`
- non-loopback prefixes are rejected by `Assert-PcvLoopbackPrefix`

- [ ] **Step 2: Run tests and verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1' -Output Detailed"
```

Expected: fail because `PcvDesktopApi.psm1` does not exist yet.

### Task 2: Helper Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Helper.Tests.ps1`

- [ ] **Step 1: Write failing helper process tests**

Create tests that write temporary helper scripts in `$TestDrive` and assert:

- `Invoke-PcvHyperVHelper` sends `{operation, params}` JSON through stdin and parses compact JSON stdout
- non-zero helper exit returns `PCV_HELPER_EXIT_FAILED`
- non-JSON helper stdout returns `PCV_HELPER_INVALID_JSON`

- [ ] **Step 2: Run tests and verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Helper.Tests.ps1' -Output Detailed"
```

Expected: fail because `Invoke-PcvHyperVHelper` does not exist yet.

### Task 3: Local API Module

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [ ] **Step 1: Implement response helpers**

Add `New-PcvApiError`, `New-PcvApiResponse`, `ConvertTo-PcvApiJson`, and helper error mapping.

- [ ] **Step 2: Implement helper invocation**

Add `Invoke-PcvHyperVHelper` using `System.Diagnostics.ProcessStartInfo` with `pwsh -NoProfile -ExecutionPolicy Bypass -File <helper>`, redirected stdin/stdout/stderr, timeout, and JSON parsing.

- [ ] **Step 3: Implement route dispatch**

Add `Invoke-PcvApiRequest` for `GET /api/v1/host/status`, `GET /api/v1/vms`, 404, and 405.

- [ ] **Step 4: Implement loopback guard**

Add `Assert-PcvLoopbackPrefix` and only allow `http://127.0.0.1:<port>/`, `http://localhost:<port>/`, or `http://[::1]:<port>/`.

- [ ] **Step 5: Run contract and helper tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: all Phase 2A API tests pass.

### Task 4: Runner And Docs

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`
- Create: `spikes/purecvisor-desktop-node/api/README.md`
- Create: `spikes/purecvisor-desktop-node/api/examples/host-status.http.txt`
- Create: `spikes/purecvisor-desktop-node/api/examples/vm-list.http.txt`
- Modify: `README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`

- [ ] **Step 1: Add runner script**

The runner imports `PcvDesktopApi.psm1`, validates the prefix, starts `HttpListener`, reads request bodies, calls `Invoke-PcvApiRequest`, and writes JSON responses.

- [ ] **Step 2: Add README and examples**

Document start command, curl examples, Pester command, loopback-only boundary, and Phase 2A exclusions.

- [ ] **Step 3: Update repository docs**

Reference the Phase 2A API spike in the root README, developer index, verification policy, and Desktop Node design roadmap.

- [ ] **Step 4: Run full verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected: API tests pass, Hyper-V non-integration suite remains 41 passed / 0 failed / 1 NotRun, and `git diff --check` passes.
