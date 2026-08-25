# Runtime Policy Dispatch Boundary Contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 이 slice 당시 `GET /api/v1/runtime/policy`가 native read adapter probe 범위와 queued mutation helper-direct dispatch boundary를 machine-readable contract로 노출하게 한다.

**Architecture:** `RuntimePolicyContract`의 `job_runtime.dispatch` object에 `native_probe_operations`와 `mutation_dispatch` 필드를 추가했다. 값은 당시 .NET Host request processor 구현과 일치해야 했다: native probe는 read candidate operation(`host.status`, `network.inventory`, `vm.list`, `checkpoint.list`)에만 허용되고 queued mutation execution은 PowerShell Hyper-V helper process direct boundary였다. 이 slice는 DTO/xUnit/docs만 바꾸며 실제 VM, checkpoint, service, MSI, firewall, Event Log mutation은 실행하지 않았다.

**Tech Stack:** C#/.NET 10, xUnit, Markdown docs, git diff guard.

---

> 현행화 메모: 이 문서는 runtime policy가 `helper-process-direct` mutation dispatch를 처음 노출한 당시 범위를 보존한다. 후속 checkpoint, VM power-state, VM lifecycle, VM delete native adapter slices 이후 현재 runtime policy는 `native_mutation_operations=[vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete]`와 `mutation_dispatch=native-vm-create-lifecycle-delete-checkpoint-mutation`을 보고한다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료
- 관리자 opt-in: 필요 없음. 실제 VM create/start/poweroff/delete, checkpoint create/delete/restore, service/MSI/firewall/Event Log/trust-store mutation은 실행하지 않는다.
- GA 상태: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 유지. `transition-helper`와 `explicit-admin-opt-in` gate는 닫지 않는다.

## Files

- Modify: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`
  - Runtime policy dispatch object가 native probe operation enum/list와 mutation helper-direct dispatch marker를 직렬화하는지 RED/GREEN으로 고정한다.
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs`
  - `JobRuntimeDispatchPolicy`에 `NativeProbeOperations`와 `MutationDispatch` 필드를 추가한다.
- Modify docs:
  - `follower.md`
  - `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md`
  - `spikes/purecvisor-desktop-node/api/README.md`

## Tasks

### Task 1: RED - runtime policy exposes dispatch boundary

- [x] **Step 1: Add failing xUnit expectations**

Update `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs` with a focused test:

```csharp
[Fact]
public void RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary()
{
    var policy = RuntimePolicyContract.CreateDefault();

    var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
    using var document = JsonDocument.Parse(json);
    var dispatch = document.RootElement
        .GetProperty("data")
        .GetProperty("job_runtime")
        .GetProperty("dispatch");

    Assert.Equal(
        new[] { "host.status", "network.inventory", "vm.list", "checkpoint.list" },
        dispatch.GetProperty("native_probe_operations").EnumerateArray().Select(item => item.GetString()).ToArray());
    Assert.Equal("helper-process-direct", dispatch.GetProperty("mutation_dispatch").GetString());
}
```

- [x] **Step 2: Verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary
```

Expected: FAIL because `native_probe_operations` and `mutation_dispatch` are not implemented yet.

### Task 2: GREEN - add dispatch contract fields

- [x] **Step 1: Update DTO**

Update `src/DesktopNode.Contracts/RuntimePolicy.cs`:

```csharp
public sealed record JobRuntimeDispatchPolicy(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("helper_boundary")] string HelperBoundary,
    [property: JsonPropertyName("native_probe_operations")] IReadOnlyList<string> NativeProbeOperations,
    [property: JsonPropertyName("mutation_dispatch")] string MutationDispatch);
```

Set default values in `RuntimePolicyContract.CreateDefault()`:

```csharp
NativeProbeOperations:
[
    "host.status",
    "network.inventory",
    "vm.list",
    "checkpoint.list"
],
MutationDispatch: "helper-process-direct"
```

- [x] **Step 2: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary
```

Expected: PASS.

### Task 3: Docs and verification

- [x] **Step 1: Update docs**

Record that Phase 25 .NET Host runtime policy now reports:

- `dispatch.native_probe_operations = ["host.status","network.inventory","vm.list","checkpoint.list"]`
- `dispatch.mutation_dispatch = "helper-process-direct"`
- queued mutation execution remains PowerShell Hyper-V helper process boundary
- GA state remains keep-spike and real host mutation remains explicit admin opt-in

- [x] **Step 2: Run verification**

Run:

```powershell
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter RuntimePolicyGetReturnsManagedContractBody
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: all pass.

- [x] **Step 3: Commit and push**

Run:

```powershell
git add src/DesktopNode.Contracts src/DesktopNode.Contracts.Tests docs spikes/purecvisor-desktop-node/api/README.md follower.md
git commit -m "Expose runtime dispatch boundary contract"
git push
```

## Completion Evidence

- RED:
- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary` failed as expected before implementation. Observed failure: `KeyNotFoundException` for missing `native_probe_operations`.
- GREEN:
- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary` passed after adding dispatch boundary fields. Result: 1 passed, 0 failed.
- Full verification:
- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyDeclaresNativeProbeOperationsAndMutationDispatchBoundary` passed. Result: 1 passed, 0 failed.
- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj` passed. Result: 5 passed, 0 failed.
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter RuntimePolicyGetReturnsManagedContractBody` passed. Result: 1 passed, 0 failed.
- `dotnet test src\DesktopNode.sln` passed. Result: 103 passed, 0 failed across Contracts, Runtime, Api, Service, and Host test projects.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed. Result: 17 passed, 0 failed.
- `git diff --check` passed with no whitespace errors.
- Commit:
- 이 plan 문서를 포함한 구현 slice commit으로 push한다. 최종 commit hash는 git history와 세션 결과를 따른다.
