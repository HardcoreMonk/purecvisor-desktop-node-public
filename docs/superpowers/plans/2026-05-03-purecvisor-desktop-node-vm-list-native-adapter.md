# .NET Native VM List Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `GET /api/v1/vms`를 C# native Hyper-V read adapter 우선 경로로 전환한다. 이 slice 당시 helper fallback은 GA-ready blocker로 유지했다.

**Architecture:** 이 slice 당시 `DesktopNodeApiRequestProcessor`는 `vm.list`에서 `IDesktopNodeHyperVNativeAdapter`를 먼저 호출하고, native VM provider가 helper JSON contract의 identity/state와 summary field 최소 조건을 보존하지 못하면 기존 PowerShell helper로 fallback했다. 이번 slice는 list route만 승격하고 `GET /api/v1/vms/{id}` detail route는 기존 helper 소유로 남겨 별도 field parity slice에서 다뤘다.

**Tech Stack:** C#/.NET 10 Windows target, `System.Management`, xUnit, Pester 5.

---

> 2026-05-03 후속 `vm-detail-native-adapter` slice에서 `GET /api/v1/vms/{id}`도 native `vm.list` handled 결과를 먼저 사용하는 경로로 전환됐다. 이 문서는 `GET /api/v1/vms` list route 전환 slice의 당시 범위와 evidence를 보존한다.
>
> 현재 상태: 후속 read-route helper fallback removal slice 이후 `GET /api/v1/vms`와 detail/checkpoint list read route는 native parity failure를 PowerShell helper fallback 없이 structured failure로 반환한다. 후속 VM power-state 및 checkpoint mutation native adapter slices 이후 VM start/poweroff와 checkpoint create/restore/delete도 C# WMI adapter로 실행된다.

## 상태

- 작성 기준: 2026-05-03
- 구현 상태: 완료
- mutation 범위: code-level WMI read adapter와 unit/contract verification만 수행한다.
- 자동 reboot: 실행하지 않는다.
- 관리자 opt-in: 실제 Hyper-V VM 생성, service install/start/stop/delete, MSI install/repair/uninstall은 이 plan에서 실행하지 않는다.

## Files

- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
- Modify: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`
- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- Modify: `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-dotnet-native-network-inventory-adapter.md`
- Modify: `follower.md`

## Tasks

### Task 1: RED - vm.list native route contract

- [x] Add xUnit tests proving `GET /api/v1/vms` can be served by `DesktopNodeHyperVNativeAdapter`, provider failures return structured retryable native errors, incomplete identity/state or summary parity returns `handled=false`, and `GET /api/v1/vms/{id}` still uses the helper during this slice.
- [x] Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "VmList|VmDetail"
```

Observed RED: FAIL because VM provider records/interfaces were not implemented yet, then FAIL because summary parity guard did not decline incomplete native data.

### Task 2: GREEN - native VM provider and route scope guard

- [x] Add `IDesktopNodeHyperVVmProvider` and VM list response records in `DesktopNodeHyperVNativeAdapter.cs`.
- [x] Add WMI-backed `DesktopNodeHyperVWmiVmProvider` using `root\virtualization\v2:Msvm_ComputerSystem` for VM name/state and conservative helper-compatible fields.
- [x] Update `DesktopNodeHyperVNativeAdapter.TryInvoke` to handle `vm.list` with `handled=true` only when every VM has non-empty `id`, `name`, `state`, and required summary parity fields.
- [x] Update `DesktopNodeApiRequestProcessor` so `/api/v1/vms/{id}` keeps using the helper-backed `vm.list` until a separate detail parity slice.
- [x] Run the same filtered xUnit command and confirm PASS.

### Task 3: Contract/docs alignment

- [x] Change `/api/v1/vms` default owner in `ApiHandlerAdapterContract` to `dotnet-native-adapter`.
- [x] Extend runtime policy native core reason to `host.status,network.inventory,vm.list`.
- [x] Update GA-ready route matrix so `GET /api/v1/vms` has `current_owner = dotnet-native`, `fallback_policy = transition-helper`, and `promotion_state = transition-helper`.
- [x] Update follow-up docs to mark `vm.list` native-first code-level slice as completed and keep detail/checkpoint/mutation routes as helper-backed follow-ups.

### Task 4: Verification

- [x] Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "VmList|VmDetail|NetworkInventory|HostStatus"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter "RuntimePolicy"
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Observed: PASS. `dotnet test src\DesktopNode.sln`, Desktop Node root Pester boundary suite, and `git diff --check` passed. Any installed/service/MSI/Hyper-V live smoke remains explicit admin opt-in follow-up evidence.
