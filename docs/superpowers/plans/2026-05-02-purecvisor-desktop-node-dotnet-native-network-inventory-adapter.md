# .NET Native Network Inventory Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** PowerShell helper process에 의존하던 첫 read-only route인 `GET /api/v1/network/inventory`를 C# native adapter 우선 경로로 전환한다.

후속 slice에서 `GET /api/v1/host/status`도 C# registry/WMI/service/admin native adapter로 전환됐고, 2026-05-03 후속 slice에서 `GET /api/v1/vms`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints`도 native read route로 전환됐다. 이후 read-route helper fallback removal slice가 Tier 1 read route의 PowerShell helper fallback을 제거했으므로 현재 product request path는 native structured success/failure를 직접 반환한다. 후속 VM power-state 및 checkpoint mutation slices 이후 runtime policy의 native core reason은 operation 기준으로 `host.status,network.inventory,vm.list,checkpoint.list,vm.start,vm.poweroff,checkpoint.create,checkpoint.restore,checkpoint.delete`로 확장됐다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 `IDesktopNodeHyperVNativeAdapter`를 먼저 호출하고, adapter가 맡지 않거나 기존 helper contract parity를 보존하지 못하는 operation은 기존 `IDesktopNodeHyperVHelper`로 fallback한다. 첫 native slice는 `network.inventory`만 대상으로 하며, native provider가 switch type, `allow_management_os`, external adapter description을 모두 보존할 수 있을 때만 JSON contract shape를 직접 반환한다. Windows WMI `root\virtualization\v2:Msvm_VirtualEthernetSwitch` 기본 조회가 이 topology parity를 완성하지 못하면 PowerShell helper `Get-VMSwitch` 경로가 응답을 소유한다.

**Tech Stack:** C#/.NET 10 Windows target, `System.Management`, xUnit, Pester 5.

---

## 현행화 메모

이 문서의 task/evidence 본문은 `network.inventory` 첫 native adapter slice의 당시 범위를 보존한다. 현재 상태는 후속 read-route helper fallback removal slice 이후 `network.inventory`, `vm.list`, VM detail, checkpoint list가 PowerShell helper fallback 없이 native structured failure를 반환하고, 후속 VM power-state/checkpoint/native lifecycle/delete adapter slices 이후 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete가 C# WMI adapter로 실행된다. Native VM create product path는 Hyper-V Generation 2만 지원하고 native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다.

## 상태

- 작성 기준: 2026-05-02
- 구현 상태: 완료
- mutation 범위: code-level 구현은 WMI 조회와 unit/contract verification만 수행했다. 이후 관리자 opt-in으로 service/MSI/Hyper-V route mutation smoke를 재실행했다.
- 자동 reboot: 실행하지 않음.
- parity guard: 2026-05-02 리뷰 수정 당시 incomplete switch type, 누락된 `allow_management_os`, external adapter description 누락 시 native adapter가 `handled=false`를 반환하고 helper로 fallback하도록 보강했다. 현재 product path는 후속 fallback removal slice 이후 같은 조건을 `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE` structured failure로 반환하고 helper로 재시도하지 않는다.

## 완료 항목

- [x] `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs` 추가
- [x] `network.inventory`는 C# native adapter가 우선 처리
- [x] native provider가 switch topology parity를 보존하지 못하면 당시 PowerShell helper `network.inventory`로 fallback하도록 구현
- [x] `vm.list`는 후속 slice에서 C# native read route로 전환
- [x] 후속 slice에서 VM detail과 checkpoint list도 native read route로 전환하고, VM start/poweroff와 checkpoint create/restore/delete는 native mutation adapter로 전환
- [x] `ApiHandlerAdapterContract`에서 `/api/v1/network/inventory` owner를 `dotnet-native-adapter`로 표시
- [x] `RuntimePolicyContract`를 `dotnet-native-read-plus-hyperv-helper-process` / `native-read-routes-with-helper-process-mutation` 상태로 갱신
- [x] product diagnostic self-audit가 기존 helper-only contract와 새 hybrid contract를 모두 유효로 인식하도록 갱신
- [x] `DesktopNode.Api`와 `DesktopNode.Api.Tests` target framework를 Windows 전용 `net10.0-windows`로 명시

## 검증

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "NetworkInventory|VmListRouteFallsBack"
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "NetworkInventoryRouteFallsBackToPowerShellHelperWhenNativeTopologyIncomplete|NativeNetworkInventoryAdapterDeclinesIncompleteSwitchTopology|NativeNetworkInventoryAdapterDeclinesMissingManagementOsParityField|ProcessorSerializesConcurrentHandleCalls"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter "RuntimePolicySerializesPhase24JobRuntimeContract|RuntimePolicyDeclaresNativeReadRouteStart"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -FullName '*Phase 24 runtime policy contract*' -Output Detailed"
dotnet test src\DesktopNode.sln
```

추가 live-read 확인:

- 컴파일된 `DesktopNode.Api.dll`에서 `DesktopNodeHyperVNativeAdapter.CreateDefault()`를 직접 호출했다.
- `network.inventory` 결과는 `handled=true`, `ok=true`, `source=hyperv`, `mutating=false`, `Default Switch` 포함으로 확인했다.
- 리뷰 수정 당시 기본 WMI provider가 helper contract parity field를 보존하지 못하는 host에서는 `handled=false` 후 PowerShell helper fallback을 사용했다. 현재 product path는 후속 fallback removal slice 이후 helper 재시도 없이 native structured failure를 반환한다. Native direct response evidence는 완전한 provider shape가 주입된 code-level contract로 유지한다.

설치본 admin-smoke evidence:

- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`
- version: `0.26.8-admin-smoke`
- provenance git commit: `b23030efb2cc305925ea3765d5c8a341e40069a9`
- MSI SHA-256: `50694850b3ff3bd199025f950fc69802bb01066474acc71c8ea275f026235e71`
- service-action, MSI lifecycle, installed .NET Host Hyper-V API route smoke: PASS
- installed `network.inventory`: `source=hyperv`, `mutating=false`, `Default Switch` 반환
- final service: `Running`
- boot time: unchanged
- `pcv-spike-*` VM leftovers: none

리뷰 수정 후 설치본 admin-smoke evidence:

- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`
- version: `0.26.9-admin-smoke`
- provenance git commit: `352aa256b77109ea9104602aebd424c627db11ed`
- MSI SHA-256: `d517baee2149d9dfcf6bd34d77b4f9de8663fd7e416558c1ba0ffb3de16788e3`
- native provider topology parity가 불완전한 host에서 helper fallback으로 `Default Switch`, `type=internal`, `allow_management_os=true`를 반환
- service-action, MSI lifecycle, installed .NET Host Hyper-V API route smoke: PASS
- final service: `Running`
- boot time: unchanged
- `pcv-spike-*` VM leftovers: none

`host.status` 후속 native adapter 설치본 admin-smoke evidence:

- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`
- version: `0.27.1-admin-smoke`
- provenance git commit: `7120ef58b924cfdf664f868b857fb91537bf6be9`
- MSI SHA-256: `9e6c57ef852df2df7794598fd0193141ad4f95f7ec365c565453a6fc05b9c48f`
- installed `host.status`: `supported=true`, Windows 10 Pro for Workstations `25H2`, admin elevated, Hyper-V enabled, VMMS running, Default Switch present
- installed `network.inventory`: `source=hyperv`, `mutating=false`, `Default Switch`, `type=internal`, `allow_management_os=true`
- service-action, MSI lifecycle, installed .NET Host Hyper-V API route smoke: PASS
- final service: `Running`
- boot time: unchanged
- `pcv-spike-*` VM leftovers: none

## 후속

- `host.status`, `vm.list`, VM detail, checkpoint list는 후속 slice에서 C# native read adapter 경로로 전환됐고 read-route helper fallback은 제거됐다.
- VM summary/storage/network parity 확장은 code-level slice로 추가됐으며, installed non-mutating rerun 전까지 GA-ready gate closure 근거로 계산하지 않는다.
- mutation route native 전환은 checkpoint create/delete가 `0.28.3-admin-smoke`로 먼저 닫혔고 checkpoint restore는 `0.28.6-admin-smoke`로 installed evidence를 추가했다. VM create/start/restart/poweroff/delete와 checkpoint create/restore/delete는 후속 `0.30.1-admin-smoke` installed evidence에서 native adapter product path로 확인됐다. VM delete는 managed delete `action=delete`, repeat delete `action=absent`, unmanaged guard block까지 확인됐다. Installer-ISO VM의 shutdown integration unavailable case는 `PCV_VM_SHUTDOWN_NOT_AVAILABLE`으로 확인됐고, `artifacts/guest-shutdown-windows-smoke-20260503-222750`은 Windows Server 2022 Evaluation guest에서 successful installed guest shutdown evidence를 추가했다.
- 새 native adapter가 포함된 첫 MSI install/repair/uninstall/Hyper-V route smoke는 `0.26.8-admin-smoke`로 닫혔고, 리뷰 수정 후 parity fallback evidence는 `0.26.9-admin-smoke`로 다시 확인했다. `host.status` native adapter slice는 `0.27.1-admin-smoke`, checkpoint create/delete native mutation adapter slice는 `0.28.3-admin-smoke`, checkpoint restore native mutation adapter slice는 `0.28.6-admin-smoke`로 설치본 evidence를 다시 수집했다.
