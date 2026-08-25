# PureCVisor Desktop Node Phase 2F Static Web Console Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add optional static Web Console file serving to the Desktop Node Local API spike without changing the loopback-only API boundary.

**Architecture:** Phase 2F adds an optional `-WebRootPath` to the Local API daemon. Existing `/api/v1/...` routes keep precedence, while non-API `GET` requests resolve to files under the configured web root. Path traversal and web-root escape attempts return structured JSON errors instead of reading arbitrary host files.

**Tech Stack:** PowerShell 7 module, `HttpListener` local API spike, Pester v5 tests, static file content-type mapping.

---

## Completion Status

Phase 2F is complete. Verification evidence is recorded below.

- RED verification: `PcvDesktopApi.Static.Tests.ps1` initially reported 0 passed, 6 failed because `Invoke-PcvApiRequest` had no `-WebRootPath` parameter.
- GREEN verification: `PcvDesktopApi.Static.Tests.ps1` reported 6 passed, 0 failed.
- API suite verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` reports 35 passed, 0 failed.
- Hyper-V non-integration verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"` reports 41 passed, 0 failed, 1 NotRun.
- Diff check: `git diff --check` exits 0 with only CRLF normalization warnings.
- Remaining Phase 2 scope: parallel worker pools, auth, LAN mode, service install.

## Scope

Included:
- `Start-PcvDesktopApi -WebRootPath <path>`
- `Invoke-PcvDesktopApi.ps1 -WebRootPath <path>`
- `Invoke-PcvApiRequest -WebRootPath <path>` static route testing seam
- `GET /` maps to `<webroot>/index.html`
- `GET /ui/app.js` maps to `<webroot>/ui/app.js`
- directory request maps to `index.html` inside that directory
- content types for `.html`, `.css`, `.js`, `.json`, `.svg`, `.png`, `.ico`, and unknown files
- traversal/web-root escape protection

Excluded:
- Web Console application UI implementation
- authentication, LAN mode, Windows service installation
- parallel worker pools
- binary streaming optimizations beyond byte response support in the listener

## File Map

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`: add static path resolution, content-type mapping, static response construction, and listener byte response support.
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`: add `-WebRootPath` parameter.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1`: add RED tests for static serving and path safety.
- `spikes/purecvisor-desktop-node/api/README.md`: document `-WebRootPath`, static routes, exclusions, and updated test count.
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`: update Phase 2F status and roadmap.
- `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `ui/guide-content.md`: update Phase 2F references.

## Tasks

### Task 1: RED Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1`

- [x] **Step 1: Write static serving tests**

Create tests that call `Invoke-PcvApiRequest -WebRootPath $TestDriveRoot` and assert:
- `GET /` returns `index.html` with `text/html; charset=utf-8`
- `GET /ui/app.js?cache=1` returns nested JavaScript with `application/javascript; charset=utf-8`
- `GET /ui/` returns `ui/index.html`
- `GET /missing.css` returns `404` with `PCV_STATIC_FILE_NOT_FOUND`
- `GET /ui/%2e%2e/secret.txt` returns `403` with `PCV_STATIC_PATH_FORBIDDEN`
- `GET /api/v1/vms` still routes to the helper when `-WebRootPath` is supplied

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1' -Output Detailed"
```

Expected before implementation: failures because `Invoke-PcvApiRequest` has no `-WebRootPath` parameter and no static route handling.

### Task 2: Static File Implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`

- [x] **Step 1: Add helper functions**

Add:
- `Get-PcvStaticContentType`
- `Test-PcvPathInsideRoot`
- `Resolve-PcvStaticFilePath`
- `New-PcvStaticFileResponse`

- [x] **Step 2: Wire request routing**

Add `[string]$WebRootPath` to `Invoke-PcvApiRequest`. If the request is `GET`, the route is not under `/api/`, and `-WebRootPath` is supplied, return a static file response or a structured static error.

- [x] **Step 3: Wire listener and runner**

Add `[string]$WebRootPath` to `Start-PcvDesktopApi` and `Invoke-PcvDesktopApi.ps1`. Pass it through to `Invoke-PcvApiRequest`. Update the listener to write `body_bytes` when present, otherwise write UTF-8 JSON/text from `body`.

- [x] **Step 4: Run GREEN verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Static.Tests.ps1' -Output Detailed"
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
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase2f-static-web-console.md`

- [x] **Step 1: Update docs**

Document Phase 2F status, `-WebRootPath`, static route behavior, path safety, and updated API test count.

- [x] **Step 2: Run final verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected:
- API suite: 35 passed, 0 failed
- Hyper-V non-integration suite: 41 passed, 0 failed, 1 NotRun
- diff check exits 0

- [x] **Step 3: Commit and push**

Commit:

```powershell
git add -- <phase-2f-files>
git commit -m "feat: serve Desktop Node static console"
git push origin main
```

## Self-Review

- Spec coverage: covers optional web root, route precedence, directory index, missing files, traversal protection, runner parameter, docs, and verification.
- Placeholder scan: no placeholder language remains.
- Type consistency: the new parameter name is consistently `WebRootPath`; response byte support is consistently `body_bytes`.
