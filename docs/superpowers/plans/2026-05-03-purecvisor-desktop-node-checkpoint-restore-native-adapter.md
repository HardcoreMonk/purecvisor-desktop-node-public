# Checkpoint Restore Native Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development and superpowers:executing-plans. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** `checkpoint.restore` queued job execution에서 PowerShell Hyper-V helper fallback을 제거하고 C# native WMI adapter가 직접 처리하게 한다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 기존 queued job contract와 job store shape를 유지한다. Worker dispatch만 `checkpoint.restore`까지 `DesktopNodeHyperVNativeAdapter`로 확장하고, 이 slice 당시 VM lifecycle과 VM create는 기존 `IDesktopNodeHyperVHelper` execution boundary에 남긴다. Native mutation provider는 `root\virtualization\v2`의 `Msvm_VirtualSystemSnapshotService.ApplySnapshot`을 사용한다. [Microsoft Learn의 ApplySnapshot 정의](https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/applysnapshot-msvm-virtualsystemsnapshotservice)는 `CIM_VirtualSystemSettingData REF Snapshot`과 async `Job`을 받으므로, provider는 checkpoint snapshot object path를 `Snapshot` parameter로 넘기고 기존 `WaitForMethodResult` path로 완료/실패/timeout을 처리한다.

**Tech Stack:** C#/.NET 10, xUnit, PowerShell/Pester diagnostics contract, Markdown route matrix.

---

## Status

- Date: 2026-05-03
- Implementation status: code-level and installed admin-smoke completed
- Admin opt-in: 사용자 요청으로 installed checkpoint restore mutation smoke를 실행했다. Evidence는 `artifacts/routeparity-service-msi-hyperv-restore-mutation-20260503-0286`에 기록됐다.
- GA state: checkpoint restore row는 code-level 및 installed admin-smoke evidence 기준 `current-native` product path로 이동한다. 후속 VM power-state/native lifecycle/delete slices 이후 VM create/start/shutdown/poweroff/restart/delete도 `current-native` product path다. `0.30.1-admin-smoke` installed mutation evidence가 VM create/start/restart/poweroff/delete와 checkpoint mutation route를 갱신했다.

## Scope

포함:

- `checkpoint.restore` queued job worker dispatch를 native adapter로 전환
- WMI snapshot restore provider 추가
- route owner/runtime policy/docs 갱신
- display-name parity: VM/checkpoint 이름 공백 허용 유지

제외:

- VM lifecycle start/shutdown/poweroff/restart
- VM create/delete
- Hyper-V helper archive/removal
- public trusted signing 또는 GA 승격

## Tasks

### Task 1: RED - queued checkpoint restore must not call helper

- [x] `QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe`에서 checkpoint restore를 제거한다.
- [x] `QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper`에 checkpoint restore route를 추가한다.
- [x] `NativeCheckpointMutationAdapterRestoresCheckpointWithNativeProvider`와 WMI `ApplySnapshot` method constant guard를 추가한다.
- [x] RED 확인:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper|NativeCheckpointMutationAdapterRestoresCheckpointWithNativeProvider|ApiHandlerAdapterContractTests"
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "WmiCheckpointMutationProviderUsesApplySnapshotForRestore|NativeCheckpointMutationAdapterRestoresCheckpointWithNativeProvider"
```

Observed before implementation: worker restore route가 helper를 호출했고, restore route owner가 `dotnet-request-processor-powershell-helper`였으며, native adapter가 `checkpoint.restore`를 handled하지 않았다. WMI method constant guard는 `ApplySnapshotMethod` symbol missing으로 컴파일 실패했다.

### Task 2: GREEN - native checkpoint restore provider

- [x] `DesktopNodeApiRequestProcessor.IsNativeOperationCandidate`에 `checkpoint.restore`를 추가한다.
- [x] `DesktopNodeHyperVNativeAdapter.TryInvoke`가 `checkpoint.restore`를 checkpoint mutation path로 처리하게 한다.
- [x] `DesktopNodeHyperVWmiCheckpointMutationProvider`가 `ApplySnapshot`을 호출하게 한다.
- [x] `ApiHandlerAdapterContract`에서 checkpoint restore queued mutation owner를 `dotnet-native-adapter`로 변경한다.
- [x] `RuntimePolicyContract`에 `native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`와 native core reason을 갱신한다.

Focused GREEN:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutHelper|NativeCheckpointMutationAdapterRestoresCheckpointWithNativeProvider|WmiCheckpointMutationProviderUsesApplySnapshotForRestore|ApiHandlerAdapterContractTests"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicy
```

Result: API focused 12 passed, 0 failed. Contracts focused 5 passed, 0 failed.

### Task 3: Docs and verification

- [x] Route matrix checkpoint restore row를 `dotnet-native` / `fallback_policy = none` / `promotion_state = current-native`로 갱신한다.
- [x] High-level docs는 checkpoint restore native code-level slice와 installed smoke gap을 분리한다.
- [x] Full verification을 session completion에서 수행했다. Commit/push는 이 문서 갱신 후 수행한다.

Verification result:

```powershell
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
git diff --check
```

Result: .NET solution 116 passed, packaging Pester 100 passed, root docs Pester 17 passed, legacy Local API Pester 97 passed, `git diff --check` exit 0.

### Task 4: Admin smoke - installed checkpoint restore mutation

- [x] `Invoke-PcvRouteParityMutationSmoke.ps1`에 installed `checkpoint.restore` route smoke를 추가했다.
- [x] Running VM에 바로 `ApplySnapshot`을 호출하면 Hyper-V WMI가 `Invalid State (32775)`를 반환함을 확인했다. Microsoft Learn의 `ApplySnapshot` return value 정의도 32775를 Invalid State로 분류한다.
- [x] 최소 안정 조건을 `vm.poweroff-before-restore`로 고정하고 installed full mutation smoke를 재실행했다.

Admin-smoke command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1' -Version '0.28.6-admin-smoke' -IsoPath 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso' -ArtifactRoot 'artifacts\routeparity-service-msi-hyperv-restore-mutation-20260503-0286' -JobTimeoutSeconds 300
```

Result:

- `summary.json`: `ok=true`, boot time unchanged, final service `Running`/`Auto`, `remaining_pcv_vms=[]`.
- `hyperv-api-route-smoke.json`: installed `checkpoint.create`, `vm.poweroff`, `checkpoint.restore`, and `checkpoint.delete` jobs succeeded. Restore result used `{ vm_name, name, action=restore }`.
- `runtime-policy.json`: `native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`.
- MSI SHA-256: `1c14c6ceadde8f1cea2189f1942e913c457524a4aeb10995472126ad560b8d0b`.

## Notes

- `checkpoint.restore`는 reversible mutation이지만 VM runtime state를 바꾸므로 installed smoke는 explicit admin opt-in evidence로만 닫는다. Current installed evidence는 `0.28.6-admin-smoke`다.
- 이 slice 자체는 VM lifecycle/VM create transition과 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 현재 결정을 바꾸지 않는다. 후속 VM lifecycle native slice 이후 current served Hyper-V mutation route는 native product path로 이동했다.
