# PureCVisor Desktop Node Phase 2G API Token Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an optional bearer-token gate to the Desktop Node Local API spike before any future LAN binding work.

**Architecture:** Phase 2G adds an optional `-ApiToken` to the Local API daemon. When unset, localhost development behavior remains unchanged. When set, every request handled by `Invoke-PcvApiRequest`, including `/api/v1/...` and static Web Console files, must include `Authorization: Bearer <token>` before route handling, helper execution, job mutation, or static file reads.

**Tech Stack:** PowerShell 7 module, `HttpListener` local API spike, Pester v5 tests.

---

## Completion Status

Phase 2G is complete. Verification evidence is recorded below.

- RED verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"` initially reported 5 failed, 1 passed before implementation because `Invoke-PcvApiRequest` did not yet expose `-ApiToken`/`-Headers` and had no auth gate.
- Auth-only GREEN verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"` reported 6 passed, 0 failed.
- API suite verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 41 passed, 0 failed.
- Hyper-V non-integration verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"` reports 41 passed, 0 failed, 1 NotRun.
- Diff verification: `git diff --check` exits 0 with only repository line-ending warnings.

## Scope

Included:
- `Start-PcvDesktopApi -ApiToken <token>`
- `Invoke-PcvDesktopApi.ps1 -ApiToken <token>`
- `Invoke-PcvApiRequest -ApiToken <token> -Headers <headers>` testing seam
- `401 PCV_AUTH_REQUIRED` when token auth is enabled and `Authorization` is missing or malformed
- `403 PCV_AUTH_FORBIDDEN` when a bearer token is present but wrong
- no auth requirement when `-ApiToken` is empty or omitted
- auth before helper execution, job mutation, and static file reads

Excluded:
- token generation/storage
- multiple tokens or user accounts
- CORS/OPTIONS behavior
- LAN binding
- Windows service installation
- parallel worker pools

## File Map

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`: add auth helpers, `Headers`/`ApiToken` params, listener header capture, and route gating.
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`: add `-ApiToken` parameter.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`: add RED tests for token behavior.
- `spikes/purecvisor-desktop-node/api/README.md`: document `-ApiToken`, required Authorization header, exclusions, and updated test count.
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`: update Phase 2G status and roadmap.
- `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `ui/guide-content.md`: update Phase 2G references.

## Tasks

### Task 1: RED Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`

- [x] **Step 1: Write auth tests**

Create tests that call `Invoke-PcvApiRequest -ApiToken 'secret' -Headers ...` and assert:
- missing `Authorization` returns `401 PCV_AUTH_REQUIRED` and does not call the helper
- malformed `Authorization` returns `401 PCV_AUTH_REQUIRED`
- wrong bearer token returns `403 PCV_AUTH_FORBIDDEN`
- correct bearer token allows API helper route execution
- correct bearer token allows static file serving
- omitted `-ApiToken` keeps existing unauthenticated localhost behavior

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"
```

Expected before implementation: failures because `Invoke-PcvApiRequest` has no `-ApiToken` or `-Headers` parameter and no auth gate.

### Task 2: Auth Implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`

- [x] **Step 1: Add auth helpers**

Add:
- `Get-PcvRequestHeader`
- `Test-PcvBearerToken`

- [x] **Step 2: Gate request handling**

Add `[object]$Headers` and `[string]$ApiToken` to `Invoke-PcvApiRequest`. If `-ApiToken` is non-empty, validate `Authorization` before routing. Return structured auth errors before helper execution, job mutation, or static file reads.

- [x] **Step 3: Wire listener and runner**

Add `[string]$ApiToken` to `Start-PcvDesktopApi` and `Invoke-PcvDesktopApi.ps1`. Convert `HttpListenerRequest.Headers` into a hashtable and pass it to `Invoke-PcvApiRequest`.

- [x] **Step 4: Run GREEN verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"
```

Expected after implementation: 6 passed, 0 failed.

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
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2g-api-token.md`

- [x] **Step 1: Update docs**

Document Phase 2G status, `-ApiToken`, `Authorization: Bearer`, error codes, and updated API test count.

- [x] **Step 2: Run final verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected:
- API suite: 41 passed, 0 failed
- Hyper-V non-integration suite: 41 passed, 0 failed, 1 NotRun
- diff check exits 0 with only line-ending warnings

- [x] **Step 3: Commit and push**

Commit:

```powershell
git add -- <phase-2g-files>
git commit -m "feat: add Desktop Node API token gate"
git push origin main
```

## Self-Review

- Spec coverage: covers optional token behavior, API/static gating, no-token backwards compatibility, listener plumbing, docs, and verification.
- Placeholder scan: no placeholder language remains.
- Type consistency: the new parameter names are consistently `ApiToken` and `Headers`.
