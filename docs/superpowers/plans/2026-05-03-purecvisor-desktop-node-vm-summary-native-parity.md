# VM Summary Native Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** VM이 존재하는 환경에서도 `GET /api/v1/vms`와 `GET /api/v1/vms/{id}`가 CPU, startup memory, generation, checkpoint count 최소 summary parity를 native WMI 경로에서 보존하게 만든다.

**Architecture:** `DesktopNodeHyperVWmiVmProvider`가 `Msvm_ComputerSystem` identity/state를 기준으로 current `Msvm_VirtualSystemSettingData`, `Msvm_ProcessorSettingData`, `Msvm_MemorySettingData`, `Msvm_SnapshotOfVirtualSystem`를 읽어 `DesktopNodeHyperVVmInfo`를 구성한다. 이 slice 당시 product route owner와 fallback policy는 유지하며, native summary field가 하나라도 불완전하면 기존 `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE` helper fallback guard가 계속 동작했다.

**Tech Stack:** C#/.NET 10 Windows target, `System.Management`, xUnit, Markdown docs.

---

> 현행화 메모: 이 문서는 VM summary parity를 넓힌 code-level slice의 당시 fallback-preserving 범위를 보존한다. 현재 product path는 후속 read-route helper fallback removal slice 이후 `PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE` 같은 native parity failure를 helper fallback 없이 반환한다. 이 code-level parity evidence는 installed non-mutating rerun 전에는 GA-ready gate closure 근거로 계산하지 않는다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료
- mutation 범위: code-level WMI read adapter와 unit/contract verification만 수행한다.
- 관리자 opt-in: 이 plan에서는 실제 VM 생성/시작/중지/삭제, checkpoint 생성/삭제, service/MSI/firewall/Event Log/trust-store mutation을 실행하지 않는다.

## Files

- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
  - Add testable WMI VM summary mapping record and mapper.
  - Extend `DesktopNodeHyperVWmiVmProvider` to read processor count, startup memory, generation, notes marker, and checkpoint count from WMI associations.
  - Keep route fallback guard unchanged.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Add RED tests for summary mapping and WMI class/association constants.
- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
  - Clarify that this slice narrows `vm.list` summary parity fallback but does not remove transition helper fallback or GA gate.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `README.md`, `docs/DEVELOPER_INDEX.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`, `follower.md`
  - Record current `0.27.5-admin-smoke` checkpoint list evidence and this summary parity code-level follow-up.

## Tasks

### Task 1: RED - WMI summary mapper contract

- [x] **Step 1: Write failing tests**

Add these tests near the existing WMI VM provider tests in `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`:

```csharp
[Fact]
public void WmiVmProviderMapsSummaryFieldsForNativeParity()
{
    var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
        Id: "alpha",
        Name: "alpha",
        EnabledState: 2,
        ProcessorCount: 2,
        StartupMemoryQuantity: 4096,
        StartupMemoryQuantityUnits: "byte*2^20",
        GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
        CheckpointCount: 1,
        Notes: "managed-by=purecvisor-desktop-node"));

    Assert.Equal("alpha", vm.Id);
    Assert.Equal("alpha", vm.Name);
    Assert.Equal("running", vm.State);
    Assert.Equal(2, vm.Cpu.Count);
    Assert.Equal(4096, vm.Memory.StartupMb);
    Assert.Null(vm.Memory.AssignedMb);
    Assert.False(vm.Memory.Dynamic);
    Assert.Equal(2, vm.Generation);
    Assert.Equal(1, vm.Checkpoints.Count);
    Assert.Equal("vmconnect", vm.Console.Type);
    Assert.True(vm.Console.AvailableLocal);
    Assert.True(vm.ManagedByPurecvisor);
}

[Fact]
public void WmiVmProviderDeclinesUnknownSummaryUnits()
{
    var vm = DesktopNodeHyperVWmiVmProvider.MapSummary(new DesktopNodeHyperVWmiVmSummary(
        Id: "alpha",
        Name: "alpha",
        EnabledState: 3,
        ProcessorCount: 1,
        StartupMemoryQuantity: 1024,
        StartupMemoryQuantityUnits: "unknown-unit",
        GenerationSubtype: "Microsoft:Hyper-V:SubType:2",
        CheckpointCount: 0,
        Notes: null));

    Assert.Equal("stopped", vm.State);
    Assert.Equal(1, vm.Cpu.Count);
    Assert.Null(vm.Memory.StartupMb);
    Assert.False(vm.ManagedByPurecvisor);
}

[Fact]
public void WmiVmProviderUsesCurrentSettingAndResourceAssociations()
{
    Assert.Equal("Msvm_VirtualSystemSettingData", DesktopNodeHyperVWmiVmProvider.VirtualSystemSettingClass);
    Assert.Equal("Msvm_SettingsDefineState", DesktopNodeHyperVWmiVmProvider.SettingsDefineStateAssociationClass);
    Assert.Equal("Msvm_VirtualSystemSettingDataComponent", DesktopNodeHyperVWmiVmProvider.SettingDataComponentAssociationClass);
    Assert.Equal("Msvm_ProcessorSettingData", DesktopNodeHyperVWmiVmProvider.ProcessorSettingClass);
    Assert.Equal("Msvm_MemorySettingData", DesktopNodeHyperVWmiVmProvider.MemorySettingClass);
    Assert.Equal("Msvm_SnapshotOfVirtualSystem", DesktopNodeHyperVWmiVmProvider.SnapshotAssociationClass);
}
```

- [x] **Step 2: Verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider"
```

Observed RED: compilation failed because `DesktopNodeHyperVWmiVmSummary`, `MapSummary`, and the WMI association constants were not implemented yet.

### Task 2: GREEN - minimal WMI summary extraction

- [x] **Step 1: Implement mapper and WMI association reads**

Add:

```csharp
public sealed record DesktopNodeHyperVWmiVmSummary(
    string Id,
    string Name,
    object? EnabledState,
    object? ProcessorCount,
    object? StartupMemoryQuantity,
    string? StartupMemoryQuantityUnits,
    string? GenerationSubtype,
    int? CheckpointCount,
    string? Notes);
```

Implement `DesktopNodeHyperVWmiVmProvider.MapSummary(...)` so it maps:

- `EnabledState`: `2 -> running`, `3 -> stopped`, existing state map for paused/saved/starting/stopping.
- `ProcessorCount`: integer count from `Msvm_ProcessorSettingData.VirtualQuantity`.
- `StartupMemoryQuantity + StartupMemoryQuantityUnits`: `byte*2^20` or MB-like units to MiB, byte-like units to MiB, unknown units to `null`.
- `GenerationSubtype`: suffix `:1` to generation 1 and suffix `:2` to generation 2.
- `CheckpointCount`: native checkpoint count.
- `Notes`: `managed-by=purecvisor-desktop-node` marker to `managed_by_purecvisor = true`.

Update `GetVms()` to keep `CimQuery` unchanged and read summary fields through these associations:

```csharp
vm.GetRelated(VirtualSystemSettingClass, SettingsDefineStateAssociationClass, null, null, "SettingData", "ManagedElement", false, null)
setting.GetRelated(ProcessorSettingClass, SettingDataComponentAssociationClass, null, null, "PartComponent", "GroupComponent", false, null)
setting.GetRelated(MemorySettingClass, SettingDataComponentAssociationClass, null, null, "PartComponent", "GroupComponent", false, null)
vm.GetRelated(VirtualSystemSettingClass, SnapshotAssociationClass, null, null, "Dependent", "Antecedent", false, null)
```

If an association is missing or unreadable, return `null` for that field and let the existing helper fallback guard decide.

- [x] **Step 2: Run focused tests**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider|VmList|VmDetail|CheckpointList"
```

Observed GREEN: 19 selected tests passed.

### Task 3: Docs and verification

- [x] **Step 1: Update docs**

Update docs to say:

- `0.27.5-admin-smoke` installed non-mutating checkpoint list evidence exists.
- `artifacts/installed-vm-create-checkpoint-list-20260503-122705` and `artifacts/installed-checkpoint-lifecycle-cleanup-20260503-124330` are explicit admin opt-in evidence for the temporary VM/checkpoint lifecycle used to validate current installed routes and cleanup.
- This new slice narrows native `vm.list` summary fallback by adding WMI CPU/startup memory/generation/checkpoint count mapping, but does not remove transition helper fallback or mark GA-ready.

- [x] **Step 2: Run verification**

Run:

```powershell
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Observed: all verification commands passed.

- [x] **Step 3: Commit and push**

Run:

```powershell
git add src docs README.md follower.md
git commit -m "Expand native VM summary parity"
git push
```

## Completion Evidence

- RED:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider"` failed before implementation with missing `DesktopNodeHyperVWmiVmSummary`, `MapSummary`, and WMI association constants.
- Focused GREEN:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider|VmList|VmDetail|CheckpointList"` passed: 19 passed, 0 failed.
- Full verification:
  - `dotnet test src\DesktopNode.sln` passed: DesktopNode.Contracts.Tests 4, DesktopNode.Api.Tests 51, DesktopNode.Service.Tests 10, DesktopNode.Runtime.Tests 16, DesktopNode.Host.Tests 14 all passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed: 17 passed, 0 failed.
  - `git diff --check` passed.
- Commit:
  - Commit command: `git commit -m "Expand native VM summary parity"`
