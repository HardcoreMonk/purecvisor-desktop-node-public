# Checkpoint List Native Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `GET /api/v1/vms/{id}/checkpoints`를 native-first read route로 전환한다. 이 slice 당시 native VM/checkpoint inventory가 parity를 보존하지 못할 때만 기존 PowerShell helper로 fallback했다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 이미 `checkpoint.list`를 `InvokeHyperVOperation`으로 호출하므로 processor 변경은 최소화했다. 이 slice 당시 `DesktopNodeHyperVNativeAdapter`가 `checkpoint.list`를 처리하되, VM inventory가 비어 있거나 VM identity가 불완전하면 `handled=false`로 helper fallback하고, authoritative VM inventory에서 VM이 없으면 `PCV_VM_NOT_FOUND`를 반환했다.

**Tech Stack:** C#/.NET 10 Windows target, `System.Management`, xUnit, PowerShell/Pester 문서 검증.

---

> 현행화 메모: 이 문서는 checkpoint list native-first 전환 slice의 당시 fallback-preserving 범위를 보존한다. 후속 read-route helper fallback removal slice 이후 `GET /api/v1/vms/{id}/checkpoints`는 native VM inventory/checkpoint parity failure를 helper 재시도 없이 structured failure로 반환한다. 후속 installed non-mutating smoke는 `artifacts/installed-nonmutating-checkpoint-list-20260503-121824`에 기록됐다.

## File Structure

- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
  - Add `IDesktopNodeHyperVCheckpointProvider` and `DesktopNodeHyperVCheckpointInfo`.
  - Add `checkpoint.list` handling with VM existence guard, empty checkpoint list support, structured failures, and helper fallback on incomplete native inventory.
  - Add `DesktopNodeHyperVWmiCheckpointProvider` using `Msvm_SnapshotOfVirtualSystem` relationship to read checkpoint names and creation time.
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
  - Promote `/api/v1/vms/{vmId}/checkpoints` default owner to `dotnet-native-adapter`.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Add RED/GREEN tests for native checkpoint list mapping, helper fallback, missing VM, empty VM inventory, and WMI association constant.
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
  - Update route owner expectation for `/api/v1/vms/{vmId}/checkpoints`.
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs`, `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`
  - Extend operation-level `native_core.reason` to include `checkpoint.list`.
- Modify: high-level docs and GA-ready matrix
  - 이 slice 당시 checkpoint list를 native-first code-level read route로 기록하되 `fallback_policy = transition-helper`와 installed non-mutating evidence follow-up gate를 보존했다. 후속 read-route helper fallback removal slice 이후 현재 route matrix는 `fallback_policy = none`, `promotion_state = current-native`다.

## Task 1: RED - Native Checkpoint List Contract

- [x] **Step 1: Write failing tests**

Add tests equivalent to:

```csharp
[Fact]
public void NativeCheckpointListAdapterMapsProviderResult()
{
    using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
    var adapter = new DesktopNodeHyperVNativeAdapter(
        new RecordingHyperVSwitchProvider([]),
        new RecordingHyperVVmProvider([CompleteVm("alpha")]),
        new RecordingHyperVCheckpointProvider(
        [
            new DesktopNodeHyperVCheckpointInfo("before-upgrade", "alpha", "2026-05-03T00:00:00.0000000Z")
        ]));

    var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, out var result);

    Assert.True(handled);
    Assert.True(result.Ok);
    Assert.Equal("checkpoint.list", result.Operation);
    Assert.Equal("before-upgrade", result.Data!.Value[0].GetProperty("name").GetString());
}
```

Add failure/guard tests:

```csharp
[Fact]
public void NativeCheckpointListAdapterReturnsNotFoundWhenVmInventoryIsAuthoritative()
{
    using var parameters = JsonDocument.Parse("""{"vm_name":"beta"}""");
    var adapter = new DesktopNodeHyperVNativeAdapter(
        new RecordingHyperVSwitchProvider([]),
        new RecordingHyperVVmProvider([CompleteVm("alpha")]),
        new RecordingHyperVCheckpointProvider([]));

    var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, out var result);

    Assert.True(handled);
    Assert.False(result.Ok);
    Assert.Equal("PCV_VM_NOT_FOUND", result.Error!.Code);
}

[Fact]
public void NativeCheckpointListAdapterDeclinesEmptyVmInventoryDuringTransition()
{
    using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
    var adapter = new DesktopNodeHyperVNativeAdapter(
        new RecordingHyperVSwitchProvider([]),
        new RecordingHyperVVmProvider([]),
        new RecordingHyperVCheckpointProvider([]));

    var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, out var result);

    Assert.False(handled);
    Assert.Equal("PCV_NATIVE_CHECKPOINT_LIST_VM_INVENTORY_EMPTY", result.Error!.Code);
}
```

- [x] **Step 2: Verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "CheckpointList|ApiHandlerAdapterContract|RuntimePolicyDeclaresNativeReadRouteStart"
```

Expected: compilation or assertions fail because native checkpoint provider types/route ownership/runtime reason are not implemented yet.

## Task 2: GREEN - Native Adapter and Contract

- [x] **Step 1: Implement minimal native checkpoint list path**

Implement:

```csharp
public interface IDesktopNodeHyperVCheckpointProvider
{
    IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName);
}

public sealed record DesktopNodeHyperVCheckpointInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vm_name")] string VmName,
    [property: JsonPropertyName("created_at")] string? CreatedAt);
```

Add `checkpoint.list` handling in `TryInvoke`, VM inventory guards, checkpoint parity guard, WMI association provider, and owner/runtime policy updates.

- [x] **Step 2: Run focused tests**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "CheckpointList|ApiHandlerAdapterContract"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter "RuntimePolicyDeclaresNativeReadRouteStart"
```

Expected: all selected tests pass.

## Task 3: Docs and Full Verification

- [x] **Step 1: Update docs**

Update native read markers, `ROUTE_PROMOTION_MATRIX`, follow-up queue, ADR index/current ADR notes, release boundary, developer index, and verification policy. 이 slice 당시 GA wording은 conservative하게 유지해 checkpoint list를 native-first code-level로 두고 installed non-mutating evidence 전까지 transition-helper fallback을 보존했다. 후속 read-route helper fallback removal slice 이후 현재 route matrix는 helper retry 없이 native structured success/failure를 반환한다.

- [x] **Step 2: Run verification**

Run:

```powershell
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: all pass.

- [x] **Step 3: Commit**

Run:

```powershell
git add src docs AGENTS.md README.md follower.md
git commit -m "Add native checkpoint list adapter"
git push
```

## 완료 증거

- RED 확인:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "CheckpointList|ApiHandlerAdapterContract"`
  - `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter "RuntimePolicyDeclaresNativeReadRouteStart"`
  - 구현 전 예상 실패를 확인했다. 실패 원인은 native checkpoint provider type/route owner 부재와 runtime policy reason 미갱신이었다.
- Focused GREEN 확인:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "CheckpointList|ApiHandlerAdapterContract"`: 선택된 10개 테스트 통과.
  - `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter "RuntimePolicyDeclaresNativeReadRouteStart"`: 선택된 1개 테스트 통과.
- 전체 검증:
  - `dotnet test src\DesktopNode.sln`: 전체 .NET test project 통과.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 17개 테스트 통과.
  - `git diff --check`: 통과.

이 slice는 code-level/xUnit 및 문서 evidence 범위다. 설치본 non-mutating checkpoint list smoke는 후속 관리자 opt-in gate로 남긴다.
