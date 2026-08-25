# Checkpoint Mutation Native Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development and superpowers:executing-plans. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `checkpoint.create`와 `checkpoint.delete` queued job execution에서 PowerShell Hyper-V helper fallback을 제거하고 C# native WMI adapter가 직접 처리하게 한다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 기존 queued job contract와 job store shape를 유지한다. Worker dispatch만 `checkpoint.create`/`checkpoint.delete`에 한해 `DesktopNodeHyperVNativeAdapter`로 보내고, `checkpoint.restore`, VM lifecycle, VM create는 기존 `IDesktopNodeHyperVHelper` execution boundary에 남긴다. Native mutation provider는 `root\virtualization\v2`의 `Msvm_VirtualSystemSnapshotService.CreateSnapshot`/`DestroySnapshot`와 `Msvm_VirtualSystemManagementService.ModifySystemSettings`를 사용한다.

**Tech Stack:** C#/.NET 10, xUnit, PowerShell/Pester diagnostics contract, Markdown route matrix.

---

> 현행화 메모: 이 문서는 checkpoint create/delete native adapter slice의 당시 범위를 보존한다. 후속 `checkpoint-restore-native-adapter` slice 이후 `checkpoint.restore`도 C# WMI `ApplySnapshot` product path로 전환됐고, 후속 VM power-state/native lifecycle/delete slices 이후 VM create/start/shutdown/poweroff/restart/delete도 C# WMI adapter product path로 전환됐다. `0.30.1-admin-smoke` installed evidence는 VM create/start/restart/poweroff/delete와 checkpoint create/restore/delete native mutation path를 확인했으며, VM delete managed `action=delete`, repeat `action=absent`, unmanaged guard block, installer-ISO shutdown integration unavailable `PCV_VM_SHUTDOWN_NOT_AVAILABLE`을 확인했다. `artifacts/guest-shutdown-windows-smoke-20260503-222750`은 Windows Server 2022 Evaluation guest의 successful installed guest shutdown evidence를 추가했다.

## Status

- Date: 2026-05-03
- Implementation status: completed
- Admin opt-in: code-level/xUnit/Pester 검증에는 필요 없음. Installed service/MSI/Hyper-V mutation smoke는 사용자 승인 범위에서 별도 실행한다.
- GA state: 이 slice 당시 checkpoint create/delete row만 `current-native` 후보로 이동했다. 후속 restore, VM power-state, VM lifecycle/delete slices 이후 현재 checkpoint create/restore/delete와 VM create/start/shutdown/poweroff/restart/delete row는 `current-native` product path다. `0.30.1-admin-smoke` installed mutation evidence가 VM create/start/restart/poweroff/delete와 checkpoint mutation route를 갱신했다.

## Scope

- 포함:
  - `checkpoint.create` queued job worker dispatch를 native adapter로 전환
  - `checkpoint.delete` queued job worker dispatch를 native adapter로 전환
  - WMI snapshot create/delete/rename provider 추가
  - route owner/runtime policy/docs 갱신
  - display-name parity: VM/checkpoint 이름 공백 허용
- 제외:
  - `checkpoint.restore`
  - `vm.create`
  - VM lifecycle start/shutdown/poweroff/restart
  - VM delete future route
  - Hyper-V helper archive/removal
  - public trusted signing 또는 GA 승격

## Tasks

### Task 1: RED - queued checkpoint mutation must not call helper

- [x] 이 slice 당시 `QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe`에서 checkpoint create/delete를 제거하고 restore/VM mutation은 helper direct로 유지했다. 후속 restore slice 이후 현재 checkpoint restore도 native adapter로 dispatch된다.
- [x] `QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper`를 추가해 checkpoint create/delete queued job이 helper를 호출하지 않고 native adapter를 호출하는지 검증한다.
- [x] `NativeCheckpointMutationAdapterMapsProviderResult`, missing checkpoint name rejection, display-name space 허용 테스트를 추가한다.
- [x] RED 확인:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper|NativeCheckpointMutationAdapterMapsProviderResult|NativeCheckpointMutationAdapterRejectsMissingCheckpointName"
```

Observed before implementation: 컴파일 실패로 `IDesktopNodeHyperVCheckpointMutationProvider`와 `DesktopNodeHyperVCheckpointMutationInfo`가 없음을 확인했다. Provider 초안 후에는 queued job이 helper로 dispatch되어 expected failure를 확인했다.

### Task 2: GREEN - native checkpoint mutation provider

- [x] `IDesktopNodeHyperVCheckpointMutationProvider`와 `DesktopNodeHyperVCheckpointMutationInfo` 추가.
- [x] `DesktopNodeHyperVWmiCheckpointMutationProvider` 추가.
- [x] `DesktopNodeApiRequestProcessor.InvokeHyperVOperation`에서 `checkpoint.create`/`checkpoint.delete`를 native operation candidate로 추가.
- [x] VM/checkpoint display name validator가 공백을 허용하고 control/slash/backslash/trim 오류만 차단하도록 수정.
- [x] Focused xUnit PASS:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper|NativeCheckpointMutationAdapter|ApiHandlerAdapterContractTests"
```

Result: 11 passed.

### Task 3: Runtime policy, diagnostics, and docs

- [x] `ApiHandlerAdapterContract`에서 checkpoint create/delete queued mutation owner를 `dotnet-native-adapter`로 변경.
- [x] `RuntimePolicyContract`에 `native_mutation_operations=[checkpoint.create,checkpoint.delete]`와 `mutation_dispatch=native-checkpoint-mutation-plus-helper-process-remainder` 추가.
- [x] Product diagnostics self-audit가 새 hybrid boundary를 유효 contract로 인식하도록 갱신.
- [x] `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/DEVELOPER_INDEX.md`, release/verification/readme 문서를 갱신.

### Task 4: Verification and admin smoke

- [x] Focused/full xUnit, Pester, npm/web, `git diff --check` 실행.
- [x] 사용자 승인 범위의 installed service/MSI/Hyper-V mutation smoke를 재실행해 checkpoint create/delete native path를 확인.
- [x] completion evidence를 이 문서에 추가.

## Completion Evidence

- RED:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper|NativeCheckpointMutationAdapterMapsProviderResult|NativeCheckpointMutationAdapterRejectsMissingCheckpointName"` failed before implementation because native checkpoint mutation provider symbols did not exist.
  - Provider 초안 후 같은 route worker test는 checkpoint create/delete가 helper로 dispatch되어 expected failure를 보였다.
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter NativeCheckpointMutationAdapterAllowsHyperVNamesWithSpaces` failed before validator 수정 because Hyper-V display names with spaces were rejected.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"` failed before diagnostics update because the new hybrid runtime policy boundary was not accepted by self-audit.
- GREEN/focused:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper|NativeCheckpointMutationAdapter|ApiHandlerAdapterContractTests"` passed.
  - `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicy` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"` passed.
- Full verification:
  - `dotnet test src\DesktopNode.sln` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"` passed.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"` passed with gated integration test not run.
  - `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"` passed.
  - `npm test --prefix web` passed.
  - `npm run verify:parity --prefix web` passed.
  - `node --check web/app.js` passed.
  - `git diff --check` passed with line-ending normalization warnings only.
- Admin opt-in installed mutation smoke:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1' -Version '0.28.3-admin-smoke' -IsoPath 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso' -ArtifactRoot 'artifacts\routeparity-service-msi-hyperv-mutation-20260503-161247-0283'
```

- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283/summary.json`: `ok=true`, `boot_time_unchanged=true`, `remaining_pcv_vms=[]`, final service `Running`/`Auto`.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283/hyperv-api-route-smoke.json`: `checkpoint.create` and `checkpoint.delete` jobs succeeded through installed .NET Host. Create result used `{ vm_name, name }`; delete result used `{ vm_name, name, action=delete }`.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-161247-0283/runtime-policy.json`: installed runtime policy reported `helper_boundary=dotnet-native-read-checkpoint-mutation-plus-hyperv-helper-process`, `native_mutation_operations=[checkpoint.create,checkpoint.delete]`, and `mutation_dispatch=native-checkpoint-mutation-plus-helper-process-remainder`.

## Notes

- Native create path는 snapshot 생성 후 `ElementName`을 요청 checkpoint 이름으로 rename한다. Hyper-V WMI `CreateSnapshot` input의 `SnapshotSettings.ElementName`만으로 이름이 보존되지 않는 host가 있어 `ModifySystemSettings`를 별도 단계로 둔다.
- Native create/delete provider는 구조적 실패를 helper fallback 없이 반환한다. `checkpoint.restore`는 이 slice에서 건드리지 않는다.
