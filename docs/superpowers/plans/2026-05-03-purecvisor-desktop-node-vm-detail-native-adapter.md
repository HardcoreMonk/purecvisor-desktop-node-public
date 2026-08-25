# VM Detail Native Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `GET /api/v1/vms/{id}`를 `vm.list` native 결과로 먼저 조회하게 전환한다. 이 slice 당시 native adapter가 parity 부족으로 route를 거절할 때만 기존 PowerShell helper로 fallback했다.

**Architecture:** detail route는 별도 native operation을 만들지 않고 기존 `vm.list` inventory 결과에서 id/name을 matching해 `vm.get` 응답으로 감싼다. 이 slice 당시 `DesktopNodeHyperVNativeAdapter`의 identity/state/summary parity guard가 incomplete 또는 empty result를 `handled=false`로 유지하므로, processor는 `InvokeHyperVOperation("vm.list", {})`만 사용해 기존 helper fallback을 보존했다.

**Tech Stack:** C#/.NET 10 Windows target, xUnit, PowerShell/Pester 문서 검증.

---

> 현행화 메모: 이 문서는 VM detail native-first 전환 slice의 당시 fallback-preserving 범위를 보존한다. 2026-05-03 read-route helper fallback removal slice 이후 VM detail은 native inventory success/failure를 helper 재시도 없이 반환한다. Checkpoint create/restore/delete는 후속 checkpoint mutation native adapter slices에서 C# WMI adapter product path로 전환됐다.

## File Structure

- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - `GET /api/v1/vms/{id}` detail route에서 직접 `helper.Invoke("vm.list", {})`를 호출하던 코드를 `InvokeHyperVOperation("vm.list", {})`로 바꾼다.
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
  - `/api/v1/vms/{vmId}` default owner를 `dotnet-native-adapter`로 승격한다.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - native handled inventory로 detail route가 helper 없이 응답하는 테스트를 추가한다.
  - native inventory가 route를 처리하지 않을 때 helper fallback이 유지되는 테스트로 기존 slice guard를 교체한다.
  - native handled inventory에 VM이 없으면 helper로 재시도하지 않고 `PCV_VM_NOT_FOUND`를 반환하는 테스트를 추가한다.
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
  - `/api/v1/vms/{vmId}` owner 기대값을 `dotnet-native-adapter`로 갱신한다.
- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/ADR_INDEX.md`, `docs/adr/*.md`, `AGENTS.md`, `follower.md`
  - native read scope와 helper fallback guard 문구에 VM detail route를 반영한다.

## Task 1: RED - VM Detail Native-First Contract

- [x] **Step 1: Write failing tests**

Add tests equivalent to:

```csharp
[Fact]
public void VmDetailRouteUsesNativeVmListBeforePowerShellHelperWhenNativeComplete()
{
    var helperCalls = new List<DesktopNodeHyperVHelperCall>();
    var nativeCalls = new List<string>();
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        helper: new RecordingHyperVHelper(helperCalls, """
        {"ok":true,"operation":"vm.list","data":[{"id":"helper","name":"helper","state":"stopped"}],"error":null}
        """),
        nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "vm.list", """
        {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running"}],"error":null}
        """));

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/alpha"));

    Assert.Equal(200, response.StatusCode);
    Assert.Empty(helperCalls);
    Assert.Single(nativeCalls);
    Assert.Equal("vm.list", nativeCalls[0]);
}
```

Replace the old helper-only detail slice test with fallback and not-found tests:

```csharp
[Fact]
public void VmDetailRouteFallsBackToHelperWhenNativeVmListDeclinesRoute()
{
    var helperCalls = new List<DesktopNodeHyperVHelperCall>();
    var nativeCalls = new List<string>();
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        helper: new RecordingHyperVHelper(helperCalls, """
        {"ok":true,"operation":"vm.list","data":[{"id":"beta","name":"beta","state":"running"}],"error":null}
        """),
        nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, handledOperation: null, responseJson: null));

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/beta"));

    Assert.Equal(200, response.StatusCode);
    Assert.Single(nativeCalls);
    Assert.Single(helperCalls);
}

[Fact]
public void VmDetailRouteReturnsNotFoundFromNativeInventoryWithoutHelperRetryWhenNativeComplete()
{
    var helperCalls = new List<DesktopNodeHyperVHelperCall>();
    var nativeCalls = new List<string>();
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        helper: new RecordingHyperVHelper(helperCalls, """
        {"ok":true,"operation":"vm.list","data":[{"id":"beta","name":"beta","state":"running"}],"error":null}
        """),
        nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "vm.list", """
        {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running"}],"error":null}
        """));

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/beta"));

    Assert.Equal(404, response.StatusCode);
    Assert.Empty(helperCalls);
    Assert.Contains("PCV_VM_NOT_FOUND", response.Body, StringComparison.Ordinal);
}
```

- [x] **Step 2: Run tests to verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "VmDetail"
```

Expected: native-first and not-found tests fail because detail route still calls the helper directly.

## Task 2: GREEN - Processor and Contract Ownership

- [x] **Step 1: Write minimal implementation**

Change `DesktopNodeApiRequestProcessor` detail route from:

```csharp
var helperResult = helper.Invoke("vm.list", EmptyObject());
```

to:

```csharp
var helperResult = InvokeHyperVOperation("vm.list", EmptyObject());
```

Change `ApiHandlerAdapterContract.CreateDefault()` from:

```csharp
ReadOnly("/api/v1/vms/{vmId}", "GetVm"),
```

to:

```csharp
NativeReadOnly("/api/v1/vms/{vmId}", "GetVm"),
```

- [x] **Step 2: Run focused tests**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "VmDetail|ApiHandlerAdapterContract"
```

Expected: all selected tests pass.

## Task 3: Docs and Verification

- [x] **Step 1: Update docs**

Document that `host.status`, `network.inventory`, `vm.list`, and `GET /api/v1/vms/{id}` are native-first code-level read routes. Keep GA/release wording conservative: native detail route still inherits `vm.list` parity guards and installed non-mutating evidence is a follow-up gate.

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
git commit -m "Add native VM detail route fallback"
git push
```

## Completion Evidence

- RED: `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "VmDetail|DefaultContractKeepsPowerShellLocalApiAsDefaultOwner"` failed as expected before implementation.
- GREEN focused: `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "VmDetail|ApiHandlerAdapterContract"` passed.
- Full .NET: `dotnet test src\DesktopNode.sln` passed.
- Root documentation suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed.
- Whitespace: `git diff --check` passed.
