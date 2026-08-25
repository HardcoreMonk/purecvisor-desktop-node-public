# Installed Account Login And noVNC Bridge Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for closure tracking.

**Goal:** Add an installed account login smoke runner, a Windows Desktop Node noVNC WebSocket-to-VNC TCP bridge, and GA evidence follow-up docs without adding Linux runtime code.

**Architecture:** Keep Account/RBAC/JWT authoritative in `DesktopNode.Api`, keep listener/WebSocket transport in `DesktopNode.Host`, and keep installed smoke execution in `packaging/windows-desktop-node/tools`. noVNC is opt-in: it is enabled only when an explicit VNC target host/port is configured, defaults to loopback-only target protection, and exposes `/api/v1/console/novnc/{vm_id}` as a WebSocket bridge path.

**Tech Stack:** C#/.NET 10 `HttpListener` WebSocket support, TCP `NetworkStream`, xUnit, PowerShell/Pester, existing GA evidence docs.

---

## Closure Synchronization - 2026-05-10

This plan is synchronized as installed smoke/code-level closed. The noVNC bridge contract, Host options, listener WebSocket-to-VNC TCP proxy, installed account login smoke runner, target-backed noVNC installed streaming runner, Pester guard, matrix, ledger, and evidence docs exist. The installed account login smoke passed in `artifacts/installed-account-login-smoke-20260510-0410-final`, and target-backed noVNC installed streaming passed in `artifacts/target-backed-novnc-installed-streaming-smoke-20260510-0411`.

The RED/FAIL expectations below are retained as historical TDD checkpoints. Current focused verification is:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiAccountAuthRequestProcessorTests
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostApplicationTests"
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1 -Output Detailed
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-smoke-20260510-0410-final
git diff --check
```

Observed closure state: installed account login smoke PASS, noVNC bridge code-level PASS, target-backed noVNC installed streaming PASS, and `host_mutation_performed=true` for the temporary account/JWT file replacement plus service restart smoke. The original files, protected ACLs, and noVNC temporary service `PathName` mutation were restored.

Canonical closure evidence:

- `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`
- `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`

### Task 1: noVNC Bridge Contract Tests

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiAccountAuthRequestProcessorTests.cs`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`

- [x] **Step 1: Write failing API/Host tests**

Add tests that assert:
- `DesktopNodeConsoleOptions(NoVncEnabled: true, NoVncWebSocketPath: "/api/v1/console/novnc/{vm_id}")` makes console capabilities show a WebSocket bridge.
- VM console session substitutes `{vm_id}` into the noVNC WebSocket path.
- host options parse `--novnc-target-host`, `--novnc-target-port`, and optional `--novnc-websocket-path`.
- host options reject non-loopback noVNC targets without `--allow-lan`.
- installed host WebSocket bridge proxies bytes between `ClientWebSocket` and a loopback TCP echo target.

- [x] **Step 2: Run RED tests**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiAccountAuthRequestProcessorTests
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostApplicationTests"
```

Historical RED expectation: fail because noVNC bridge options and WebSocket transport were not implemented.

### Task 2: noVNC Bridge Implementation

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeAccountAuth.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs`

- [x] **Step 1: Implement minimal bridge**

Add opt-in host options for noVNC target host/port and path, pass `DesktopNodeConsoleOptions` to the API processor, and implement `HttpListener` WebSocket proxying to a configured TCP VNC target.

- [x] **Step 2: Run GREEN tests**

Run the same API/Host focused tests and keep them green.

### Task 3: Installed Account Login Smoke Runner

**Files:**
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1`
- Create: `packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1`

- [x] **Step 1: Write failing Pester tests**

Assert the smoke runner exists and records: account file backup/restore, JWT signing key backup/restore, service restart, `POST /api/v1/auth/login`, `GET /api/v1/auth/session`, `GET /api/v1/auth/rbac`, console capabilities, token redaction, host mutation status, and public claim fields.

- [x] **Step 2: Implement smoke runner**

The runner must create a temporary operator account, restart the installed service, verify account login/session/RBAC/console routes, then restore original account/JWT files and restart the service. It must not print passwords, access tokens, refresh tokens, or service bearer tokens.

### Task 4: Evidence Follow-Up

**Files:**
- Create: `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- Modify: `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- Modify: `docs/OPERATIONS_GUIDE.md`
- Modify: `README.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [x] **Step 1: Add documentation guard**

Add Pester coverage for the new evidence doc, matrix row, ledger record, and public-claim guardrails.

- [x] **Step 2: Update docs**

Record code-level noVNC bridge support, installed account login smoke runner availability, actual execution status, host mutation status, token redaction, and explicit public trusted signing/external stable publication non-claims.

### Task 5: Final Verification

**Files:**
- Verify all modified files.

- [x] **Step 1: Run focused verification**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiAccountAuthRequestProcessorTests
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostApplicationTests"
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1 -Output Detailed
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
git diff --check
```

- [x] **Step 2: Commit and push**

Commit the completed feature branch and push it for PR review.
