# Mutation Dispatch Helper Boundary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 이 slice 당시 queued Hyper-V mutation job 실행 경로가 native read adapter를 probe하지 않고 PowerShell Hyper-V helper execution boundary로 바로 dispatch되도록 고정한다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 read-only native candidate operation에만 `IDesktopNodeHyperVNativeAdapter`를 먼저 시도했다. 이 slice 당시 `vm.create`, VM lifecycle, checkpoint create/restore/delete 같은 queued mutation operation은 .NET request processor queue가 job을 소유하되 execution은 `IDesktopNodeHyperVHelper`로 직접 위임했다. 이 slice는 xUnit/code/docs만 바꾸며 실제 VM, checkpoint, service, MSI, firewall, Event Log mutation은 실행하지 않았다.

**Tech Stack:** C#/.NET 10, xUnit, Markdown docs, git diff guard.

---

> 현행화 메모: 이 문서는 queued mutation이 native read adapter를 probe하지 않고 helper로 직접 dispatch되도록 고정한 당시 범위를 보존한다. 후속 checkpoint mutation, VM power-state, VM lifecycle/delete native adapter slices 이후 current served Hyper-V mutation route인 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete는 native adapter로 직접 dispatch된다. PowerShell helper는 current served product fallback이 아니라 component/regression 검증 경계로 남는다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료
- 관리자 opt-in: 필요 없음. 실제 VM create/start/poweroff/delete, checkpoint create/delete/restore, service/MSI/firewall/Event Log/trust-store mutation은 실행하지 않는다.
- GA 상태: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 유지. `transition-helper`와 `explicit-admin-opt-in` gate는 닫지 않는다.

## Files

- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Queued mutation worker가 native read adapter를 probe하지 않고 helper로 직접 dispatch하는 RED/GREEN guard를 추가한다.
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - `InvokeHyperVOperation`에서 native adapter 선시도를 read-only native candidate operation으로 제한한다.
- Modify docs:
  - `follower.md`
  - `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md`

## Tasks

### Task 1: RED - queued mutation dispatch must skip native read adapter

- [x] **Step 1: Add failing xUnit expectations**

Update `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs` with a theory that queues served mutation routes, processes one queued job, and requires:

```csharp
Assert.Empty(nativeCalls);
Assert.Single(helperCalls);
Assert.Equal(expectedOperation, helperCalls[0].Operation);
```

The route examples must include `POST /api/v1/vms`, lifecycle route, checkpoint create, checkpoint restore, and checkpoint delete.

- [x] **Step 2: Verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe
```

Expected: FAIL because `ProcessOneQueuedJob()` currently calls `nativeAdapter.TryInvoke()` before falling back to the helper for queued mutation operations.

### Task 2: GREEN - restrict native adapter probe to read candidates

- [x] **Step 1: Update request processor dispatch**

Update `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` so `InvokeHyperVOperation` only probes `nativeAdapter` for:

```text
host.status
network.inventory
vm.list
checkpoint.list
```

All other operations must call `helper.Invoke(operation, parameters)` directly.

- [x] **Step 2: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe
```

Expected: PASS.

### Task 3: Docs and verification

- [x] **Step 1: Update docs**

Record that queued mutation execution now skips the native read adapter probe:

- queue owner: .NET request processor
- execution boundary: PowerShell Hyper-V helper process
- native adapter scope: read-only native candidates only
- GA state: still transition helper, explicit admin opt-in required for real host mutation

- [x] **Step 2: Run verification**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: all pass.

- [x] **Step 3: Commit and push**

Run:

```powershell
git add src/DesktopNode.Api src/DesktopNode.Api.Tests docs follower.md
git commit -m "Route mutation jobs directly to helper boundary"
git push
```

## Completion Evidence

- RED:
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe` failed as expected before implementation. Result: 5 failed, 0 passed. Observed failure: `nativeCalls` contained `vm.create`, `vm.start`, `checkpoint.create`, `checkpoint.restore`, and `checkpoint.delete`.
- GREEN:
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe` passed after limiting native adapter probe to read candidates. Result: 5 passed, 0 failed.
- Full verification:
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe` passed. Result: 5 passed, 0 failed.
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj` passed. Result: 58 passed, 0 failed.
- `dotnet test src\DesktopNode.sln` passed. Result: 102 passed, 0 failed across Contracts, Runtime, Api, Service, and Host test projects.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed. Result: 17 passed, 0 failed.
- `git diff --check` passed with no whitespace errors.
- Commit:
- 이 plan 문서를 포함한 구현 slice commit으로 push한다. 최종 commit hash는 git history와 세션 결과를 따른다.
