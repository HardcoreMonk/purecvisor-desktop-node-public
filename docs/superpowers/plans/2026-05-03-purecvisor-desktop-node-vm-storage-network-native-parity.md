# VM Storage Network Native Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `GET /api/v1/vms`와 `GET /api/v1/vms/{id}` native WMI path가 VM storage path/attached와 network switch/mode fields를 best-effort로 보존하게 만든다.

**Architecture:** `DesktopNodeHyperVWmiVmProvider`의 summary record에 storage/network raw summary rows를 추가하고, `MapSummary`가 helper JSON contract의 `storage[]`와 `network[]` field shape로 변환한다. WMI extraction은 current `Msvm_VirtualSystemSettingData`에서 `Msvm_StorageAllocationSettingData`, `Msvm_EthernetPortAllocationSettingData` association을 읽되, 알 수 없는 size는 `null`로 둔다. 이 slice 당시 existing helper fallback guard와 route owner 상태는 바꾸지 않았다.

**Tech Stack:** C#/.NET 10 Windows target, `System.Management`, xUnit, Markdown docs.

---

> 현행화 메모: 이 문서는 VM storage/network parity를 넓힌 code-level slice의 당시 fallback-preserving 범위를 보존한다. 현재 product path는 후속 read-route helper fallback removal slice 이후 VM list/detail native parity failure를 helper fallback 없이 structured failure로 반환한다. 이 code-level parity evidence는 installed non-mutating rerun 전에는 GA-ready gate closure 근거로 계산하지 않는다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료
- mutation 범위: code-level WMI read adapter와 unit/contract verification만 수행한다.
- 관리자 opt-in: 이 plan에서는 실제 VM 생성/시작/중지/삭제, checkpoint 생성/삭제, service/MSI/firewall/Event Log/trust-store mutation을 실행하지 않는다.

## Files

- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
  - Add raw WMI storage/network summary records.
  - Extend `DesktopNodeHyperVWmiVmSummary` with `Storage` and `Network` rows.
  - Map storage VHD/VHDX paths to `DesktopNodeHyperVVmDiskInfo`.
  - Map network switch names to `DesktopNodeHyperVVmNetworkInfo`.
  - Add WMI class constants for storage and ethernet allocation setting data.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Add RED tests for storage/network mapping and WMI constants.
- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `README.md`, `docs/DEVELOPER_INDEX.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`, `follower.md`
  - Record this slice as code-level storage/network parity narrowing, without removing transition-helper fallback or changing GA state.

## Tasks

### Task 1: RED - storage/network mapper contract

- [x] **Step 1: Write failing tests**

Add these tests near the existing WMI VM provider tests in `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`:

```csharp
[Fact]
public void WmiVmProviderMapsStorageAndNetworkFieldsForNativeParity()
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
        Notes: "managed-by=purecvisor-desktop-node",
        Storage:
        [
            new DesktopNodeHyperVWmiVmStorageSummary("D:\\VMs\\alpha\\disk0.vhdx", Attached: true)
        ],
        Network:
        [
            new DesktopNodeHyperVWmiVmNetworkSummary("Default Switch")
        ]));

    var disk = Assert.Single(vm.Storage);
    Assert.Equal("vhdx", disk.Kind);
    Assert.Equal("D:\\VMs\\alpha\\disk0.vhdx", disk.Path);
    Assert.Null(disk.SizeGb);
    Assert.True(disk.Attached);

    var network = Assert.Single(vm.Network);
    Assert.Equal("Default Switch", network.Switch);
    Assert.Equal("default-switch", network.Mode);
}

[Fact]
public void WmiVmProviderUsesStorageAndNetworkAssociationClasses()
{
    Assert.Equal("Msvm_StorageAllocationSettingData", DesktopNodeHyperVWmiVmProvider.StorageSettingClass);
    Assert.Equal("Msvm_EthernetPortAllocationSettingData", DesktopNodeHyperVWmiVmProvider.EthernetPortAllocationSettingClass);
}
```

- [x] **Step 2: Verify RED**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider"
```

Observed RED: compilation failed because `DesktopNodeHyperVWmiVmStorageSummary`, `DesktopNodeHyperVWmiVmNetworkSummary`, `Storage`, `Network`, `StorageSettingClass`, and `EthernetPortAllocationSettingClass` were not implemented yet.

### Task 2: GREEN - storage/network mapping and WMI extraction

- [x] **Step 1: Implement mapper and extraction**

Add:

```csharp
public sealed record DesktopNodeHyperVWmiVmStorageSummary(string? Path, bool Attached);
public sealed record DesktopNodeHyperVWmiVmNetworkSummary(string? SwitchName);
```

Extend `DesktopNodeHyperVWmiVmSummary` with:

```csharp
IReadOnlyList<DesktopNodeHyperVWmiVmStorageSummary>? Storage = null,
IReadOnlyList<DesktopNodeHyperVWmiVmNetworkSummary>? Network = null
```

Implement mapping:

- Storage rows with `.vhd` or `.vhdx` path become `DesktopNodeHyperVVmDiskInfo("vhdx", path, null, attached)`.
- Storage rows with empty path or non-VHD extension are ignored.
- Network rows with non-empty switch name become `DesktopNodeHyperVVmNetworkInfo(switchName, "default-switch")` for `Default Switch`, otherwise `"hyperv-switch"`.

Update WMI extraction:

```csharp
setting.GetRelated(StorageSettingClass, SettingDataComponentAssociationClass, null, null, "PartComponent", "GroupComponent", false, null)
setting.GetRelated(EthernetPortAllocationSettingClass, SettingDataComponentAssociationClass, null, null, "PartComponent", "GroupComponent", false, null)
```

Read storage path from first usable `HostResource` entry. Read network switch from `LastKnownSwitchName`, then first usable `Connection` entry. On WMI errors return empty lists.

- [x] **Step 2: Run focused tests**

Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider|VmList|VmDetail"
```

Observed GREEN: 15 selected tests passed.

### Task 3: Docs and verification

- [x] **Step 1: Update docs**

Update docs to say this slice adds code-level WMI storage/network mapping for native `vm.list`, while `transition-helper` fallback and installed evidence requirements remain open.

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
git add src docs README.md follower.md AGENTS.md
git commit -m "Add native VM storage network mapping"
git push
```

## Completion Evidence

- RED:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider"` failed before implementation with missing storage/network raw summary types, summary parameters, and WMI class constants.
- Focused GREEN:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiVmProvider|VmList|VmDetail"` passed: 15 passed, 0 failed.
- Full verification:
  - `dotnet test src\DesktopNode.sln` passed: DesktopNode.Contracts.Tests 4, DesktopNode.Runtime.Tests 16, DesktopNode.Api.Tests 53, DesktopNode.Service.Tests 10, DesktopNode.Host.Tests 14 all passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed: 17 passed, 0 failed.
  - `git diff --check` passed.
- Commit:
  - Commit command: `git commit -m "Add native VM storage network mapping"`
