# PureCVisor Desktop Node VM Power-State Native Adapter Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `vm.start`와 `vm.poweroff` queued job execution에서 PowerShell Hyper-V helper fallback을 제거하고 C# native WMI adapter가 직접 처리하게 한다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 기존 queued job contract와 job store shape를 유지한다. Worker dispatch만 `vm.start`와 `vm.poweroff`에 한해 `DesktopNodeHyperVNativeAdapter`로 보내고, `vm.shutdown`, `vm.restart`, `vm.create`는 기존 `IDesktopNodeHyperVHelper` execution boundary에 남긴다. Native provider는 `root\virtualization\v2`의 `Msvm_ComputerSystem.RequestStateChange`를 사용한다. Microsoft Learn의 [`RequestStateChange` 정의](https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/requeststatechange-msvm-computersystem)에 맞춰 `vm.start`는 `RequestedState = 2` (`Enabled`), `vm.poweroff`는 `RequestedState = 3` (`Disabled`)를 호출하고 async `Job` completion을 기존 WMI wait path로 처리한다.

**Tech Stack:** C#/.NET 10, xUnit, PowerShell/Pester diagnostics contract, Markdown route matrix.

---

## Status

- 작성 기준: 2026-05-03
- Implementation status: code-level and installed admin-smoke completed
- Admin opt-in: 사용자 요청으로 installed service/MSI/Hyper-V mutation smoke를 실행했다. Evidence는 `artifacts/routeparity-service-msi-hyperv-vm-power-state-mutation-20260503-0288`에 기록됐다.
- GA state: `POST /api/v1/vms/{id}/start`와 `POST /api/v1/vms/{id}/poweroff` row는 code-level 및 installed admin-smoke evidence 기준 `current-native` product path로 이동한다. 후속 VM lifecycle/delete native slice 이후 VM create/shutdown/restart/delete도 `current-native` product path다. `0.30.1-admin-smoke` installed mutation evidence가 VM create/start/restart/poweroff/delete와 checkpoint mutation route를 갱신했다.

## Scope

포함:

- `vm.start`
- `vm.poweroff`
- queued job creation, job result shape, job store compatibility
- runtime policy native mutation operation list
- route owner contract
- product diagnostics self-audit contract

제외:

- `vm.shutdown`
- `vm.restart`
- `vm.create`
- VM delete future route
- Hyper-V helper archive/removal
- GA 제품 런타임 승격 또는 stable release 발행

## Tasks

- [x] RED: queued VM power-state worker가 helper를 호출하지 않고 native adapter를 호출해야 한다는 xUnit 추가.
- [x] RED: native adapter가 provider result를 `{ name, action }` payload로 map해야 한다는 xUnit 추가.
- [x] RED: WMI provider가 `RequestStateChange`, `Enabled=2`, `Disabled=3` constants를 고정한다는 xUnit 추가.
- [x] RED: runtime policy가 VM power-state native mutation을 노출해야 한다는 contract test 갱신.
- [x] GREEN: `IDesktopNodeHyperVVmPowerStateProvider`와 `DesktopNodeHyperVWmiVmPowerStateProvider` 추가.
- [x] GREEN: `DesktopNodeHyperVNativeAdapter`가 `vm.start`/`vm.poweroff`를 처리하고 missing/invalid VM name을 structured failure로 반환.
- [x] GREEN: `DesktopNodeApiRequestProcessor` native candidate list에 `vm.start`/`vm.poweroff` 추가.
- [x] GREEN: `ApiHandlerAdapterContract`에서 start/poweroff queued mutation owner를 `dotnet-native-adapter`로 변경.
- [x] GREEN: `RuntimePolicyContract`와 product diagnostics self-audit를 새 hybrid contract로 갱신.
- [x] `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, ADR index/current ADR notes, release boundary, developer index, verification policy, README 문서를 갱신.

## Verification

TDD RED:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedVmPowerStateWorkerDispatchesToNativeAdapterWithoutHelper|NativeVmPowerStateAdapterMapsProviderResult|NativeVmPowerStateAdapterRejectsMissingVmName|WmiVmPowerStateProviderUsesRequestStateChangeConstants|QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe|ApiHandlerAdapterContractTests"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyContractTests
```

결과: expected failure. 새 interface/type/runtime policy 값이 아직 없어서 실패했다.

Focused GREEN:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "QueuedVmPowerStateWorkerDispatchesToNativeAdapterWithoutHelper|NativeVmPowerStateAdapterMapsProviderResult|NativeVmPowerStateAdapterRejectsMissingVmName|WmiVmPowerStateProviderUsesRequestStateChangeConstants|QueuedMutationWorkerDispatchesDirectlyToHelperWithoutNativeAdapterProbe|ApiHandlerAdapterContractTests"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter RuntimePolicyContractTests
```

결과: API focused suite PASS, 14 passed. Runtime policy contract suite PASS, 5 passed.

Full verification:

```powershell
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

결과: 모두 PASS. `git diff --check`는 line-ending normalization warning만 출력하고 exit 0을 반환했다.

## Installed Evidence

- `artifacts/routeparity-service-msi-hyperv-vm-power-state-mutation-20260503-0288/summary.json`: `ok=true`, boot time unchanged, final service `Running`/`Auto`, `remaining_pcv_vms=[]`.
- `artifacts/routeparity-service-msi-hyperv-vm-power-state-mutation-20260503-0288/hyperv-api-route-smoke.json`: installed `vm.start`, `vm.poweroff`, `checkpoint.create`, `checkpoint.restore`, and `checkpoint.delete` jobs succeeded. Start result used `{ name, action=start }`; poweroff result used `{ name, action=poweroff }`; restore result used `{ vm_name, name, action=restore }`.
- `artifacts/routeparity-service-msi-hyperv-vm-power-state-mutation-20260503-0288/runtime-policy-check.json`: installed runtime policy reported `helper_boundary=dotnet-native-read-vm-power-state-checkpoint-mutation-plus-hyperv-helper-process`, `native_mutation_operations=[vm.start,vm.poweroff,checkpoint.create,checkpoint.restore,checkpoint.delete]`, and `mutation_dispatch=native-vm-power-state-checkpoint-mutation-plus-helper-process-remainder`.
- MSI SHA-256: `74d18c9351f939f70717647124588005577ad8ffa3b6c2eda32060f0a4ae63d7`.

## Notes

- `vm.poweroff`는 graceful guest shutdown이 아니라 Hyper-V computer system state를 `Disabled`로 바꾸는 native power-state operation이다.
- `vm.shutdown`은 guest integration/graceful shutdown semantics가 필요하므로 별도 slice로 남긴다.
- `vm.restart`는 stop-start sequencing, intermediate failure recovery, idempotency가 필요하므로 별도 slice로 남긴다.
- `vm.create`는 persistent/destructive setup, storage/network cleanup, rollback evidence가 필요하므로 별도 slice로 남긴다.
- 이 slice는 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 현재 결정을 바꾸지 않는다.
