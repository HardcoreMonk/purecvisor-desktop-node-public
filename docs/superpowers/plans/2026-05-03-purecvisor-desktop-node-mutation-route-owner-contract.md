# Mutation Route Owner Contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Served Hyper-V mutation routes의 current owner contract를 실제 구현처럼 .NET request processor queue + PowerShell helper execution boundary로 좁힌다.

**Architecture:** 이 slice 당시 `ApiHandlerAdapterContract`가 aggregate lifecycle 후보 대신 실제 served mutation route를 나열하고, queued mutation route의 `DefaultOwner`를 `dotnet-request-processor-powershell-helper`로 기록했다. 이 slice는 contract/docs/xUnit만 바꾸며 native mutation adapter 구현, helper fallback 제거, Hyper-V/MSI/service mutation은 하지 않았다.

**Tech Stack:** C#/.NET 10, xUnit, Markdown docs, git diff guard.

---

> 현행화 메모: 이 문서는 served mutation route owner를 처음 `dotnet-request-processor-powershell-helper`로 좁힌 contract slice의 당시 범위를 보존한다. 후속 checkpoint mutation, VM power-state, VM lifecycle/delete native adapter slices 이후 current served Hyper-V mutation route인 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete row는 `dotnet-native` product execution으로 전환됐다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료, 푸시 완료
- 관리자 opt-in: 필요 없음. 실제 VM create/start/poweroff/delete, checkpoint create/delete/restore, service/MSI/firewall/Event Log/trust-store mutation은 실행하지 않는다.
- GA 상태: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 유지. `transition-helper`와 `explicit-admin-opt-in` gate는 닫지 않는다.

## Files

- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
  - Add `dotnet-request-processor-powershell-helper` current owner constant.
  - Replace aggregate `/api/v1/vms/{vmId}/lifecycle/{action}` contract row with actual served lifecycle routes.
  - Add served `POST /api/v1/vms`, checkpoint restore, and checkpoint delete mutation rows.
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
  - RED/GREEN guard for actual served mutation routes and current owner.
  - Guard that aggregate lifecycle pseudo route is not exposed as a served route contract.
- Modify docs:
  - `follower.md`
  - `docs/DEVELOPER_INDEX.md`
  - `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
  - relevant Phase 25 plan/spec docs

## Tasks

### Task 1: RED - served mutation route owner contract

- [x] **Step 1: Add failing xUnit expectations**

Update `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` so the default contract requires:

```csharp
AssertRoute(routes[("POST", "/api/v1/vms")], "POST", "QueueCreateVm", MutationStance.QueuedMutation);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/start")], "POST", "QueueStartVm", MutationStance.QueuedMutation);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/shutdown")], "POST", "QueueShutdownVm", MutationStance.QueuedMutation);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/poweroff")], "POST", "QueuePowerOffVm", MutationStance.QueuedMutation);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/restart")], "POST", "QueueRestartVm", MutationStance.QueuedMutation);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/checkpoints")], "POST", "QueueCreateVmCheckpoint", MutationStance.QueuedMutation);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore")], "POST", "QueueRestoreVmCheckpoint", MutationStance.QueuedMutation);
AssertRoute(routes[("DELETE", "/api/v1/vms/{vmId}/checkpoints/{checkpointId}")], "DELETE", "QueueDeleteVmCheckpoint", MutationStance.QueuedMutation);
```

Also require every queued mutation route to have:

```csharp
Assert.Equal("dotnet-request-processor-powershell-helper", route.DefaultOwner);
```

And require:

```csharp
Assert.DoesNotContain(contract.Routes, route => route.RouteTemplate == "/api/v1/vms/{vmId}/lifecycle/{action}");
```

- [x] **Step 2: Verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests
```

Expected: FAIL because the served mutation rows and queued mutation owner are not implemented yet.

### Task 2: GREEN - contract implementation

- [x] **Step 1: Update contract rows**

Update `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`:

```csharp
private const string DotNetRequestProcessorPowerShellHelperOwner = "dotnet-request-processor-powershell-helper";
```

Then list served queued mutation routes:

```csharp
QueuedMutation("/api/v1/vms", "QueueCreateVm"),
QueuedMutation("/api/v1/vms/{vmId}/checkpoints", "QueueCreateVmCheckpoint"),
QueuedMutation("/api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore", "QueueRestoreVmCheckpoint"),
QueuedMutation("/api/v1/vms/{vmId}/checkpoints/{checkpointId}", "QueueDeleteVmCheckpoint", method: "DELETE"),
QueuedMutation("/api/v1/vms/{vmId}/start", "QueueStartVm"),
QueuedMutation("/api/v1/vms/{vmId}/shutdown", "QueueShutdownVm"),
QueuedMutation("/api/v1/vms/{vmId}/poweroff", "QueuePowerOffVm"),
QueuedMutation("/api/v1/vms/{vmId}/restart", "QueueRestartVm")
```

Queued mutation `DefaultOwner` must be `dotnet-request-processor-powershell-helper`.

- [x] **Step 2: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests
```

Expected: PASS.

### Task 3: Docs and verification

- [x] **Step 1: Update docs**

Record that mutation route contract now distinguishes:

- route/job queue owner: .NET request processor
- mutation execution boundary: PowerShell Hyper-V helper process
- GA state: still transition helper, explicit admin opt-in required

- [x] **Step 2: Run verification**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: all pass.

- [x] **Step 3: Commit and push**

Run:

```powershell
git add src/DesktopNode.Api src/DesktopNode.Api.Tests docs follower.md
git commit -m "Clarify mutation route owner contract"
git push
```

## Completion Evidence

- RED:
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests` failed as expected before implementation. Observed failures included missing `(POST, /api/v1/vms)` and queued mutation owner mismatch (`powershell-local-api` vs `dotnet-request-processor-powershell-helper`).
- GREEN:
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests` passed after adding the served mutation rows and `dotnet-request-processor-powershell-helper` owner contract. Result: 4 passed, 0 failed.
- Full verification:
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests` passed. Result: 4 passed, 0 failed.
- `dotnet test src\DesktopNode.sln` passed. Result: 97 passed, 0 failed across Contracts, Service, Runtime, Api, and Host test projects.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed. Result: 17 passed, 0 failed.
- `git diff --check` passed with no whitespace errors.
- Commit:
- `f0a796a Clarify mutation route owner contract`
