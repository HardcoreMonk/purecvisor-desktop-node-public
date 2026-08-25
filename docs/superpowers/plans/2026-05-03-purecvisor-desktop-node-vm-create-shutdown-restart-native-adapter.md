# Desktop Node VM create/shutdown/restart native adapter

**Goal:** 남은 served VM lifecycle mutation route인 `vm.create`, `vm.shutdown`, `vm.restart`를 PowerShell Hyper-V helper execution boundary에서 C# native WMI adapter product path로 옮긴다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 기존 queued job contract와 JSON job store shape를 유지한다. Worker dispatch는 `vm.create`, `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart`, checkpoint create/restore/delete를 모두 `DesktopNodeHyperVNativeAdapter`로 보낸다. 후속 VM delete slice 이후 current worker dispatch는 `vm.delete`도 같은 native adapter product path로 보낸다. `vm.restart`는 `root\virtualization\v2:Msvm_ComputerSystem.RequestStateChange(11)` hard reset을 사용한다. `vm.shutdown`은 guest graceful shutdown 의미를 유지하기 위해 `Msvm_ShutdownComponent.InitiateShutdown(false, reason)`을 사용하며, guest shutdown integration이 준비되지 않은 VM은 structured `PCV_VM_SHUTDOWN_NOT_AVAILABLE` 실패로 둔다. `vm.create`는 `Msvm_ImageManagementService.CreateVirtualHardDisk`, `Msvm_VirtualSystemManagementService.DefineSystem`, `ModifyResourceSettings`, `AddResourceSettings`로 Generation 2 VM, VHDX, ISO, Default Switch attachment를 구성한다. Gen2 controller/NIC resource는 빈 WMI class instance가 아니라 `Msvm_ResourcePool`의 default RASD를 기준으로 추가한다.

**Scope boundary:**

- 관리자 opt-in: 실제 Hyper-V VM create/shutdown/restart smoke는 explicit admin opt-in evidence에서만 aggregate gate 근거로 쓴다.
- Native create support: 이번 product path는 Hyper-V Generation 2만 지원한다. Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환한다.
- GA state: current served Hyper-V mutation route는 `dotnet-native`/`fallback_policy = none` product path가 됐지만, public trusted/stable signing과 GA 승격 판단은 계속 별도다.
- Existing PowerShell Hyper-V helper는 component/regression 기준으로 남지만 current served product request path fallback으로 사용하지 않는다.

## Route and Policy Changes

- `POST /api/v1/vms` -> queued `vm.create` -> native VM create provider.
- `POST /api/v1/vms/{id}/shutdown` -> queued `vm.shutdown` -> native `Msvm_ShutdownComponent.InitiateShutdown`.
- `POST /api/v1/vms/{id}/restart` -> queued `vm.restart` -> native `RequestStateChange(11)`.
- Runtime policy:
  - 당시 slice: `helper_boundary = dotnet-native-read-vm-create-lifecycle-checkpoint-mutation`
  - 당시 slice: `native_mutation_operations = [vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,checkpoint.create,checkpoint.restore,checkpoint.delete]`
  - 당시 slice: `mutation_dispatch = native-vm-create-lifecycle-checkpoint-mutation`
  - 당시 slice: `host_mutation = native-read-routes-vm-create-lifecycle-and-checkpoint-mutation`
  - 후속 VM delete slice 이후 current runtime policy는 `vm.delete`를 포함하며 `mutation_dispatch = native-vm-create-lifecycle-delete-checkpoint-mutation`을 보고한다.

## Verification

- [x] RED: `vm.create` queued worker still dispatched to helper before native create provider existed.
- [x] GREEN: `vm.create` queued worker dispatches to native adapter without helper.
- [x] GREEN: native VM create adapter maps provider result and rejects invalid create params before provider mutation.
- [x] GREEN: native VM create rejects Generation 1 before provider mutation.
- [x] GREEN: native VM create provider constants match observed Hyper-V Generation 2 SCSI/NIC resource shape.
- [x] GREEN: direct loopback `DesktopNode.Host.dll listen` smoke created a Generation 2 VM, attached VHDX/ISO, attached `Default Switch`, and cleaned up the VM/root.
- [x] GREEN: `vm.shutdown` and `vm.restart` dispatch to native power-state adapter without helper.
- [x] GREEN: direct loopback lifecycle smoke proved `vm.start` and `vm.restart` on a created VM; initial `RequestStateChange(10)` mapping failed with unsupported request and was corrected to reset state `11`.
- [x] GREEN: direct loopback lifecycle smoke proved installer-ISO VM `vm.shutdown` returns structured `PCV_VM_SHUTDOWN_NOT_AVAILABLE` instead of a generic WMI failure when guest shutdown integration is unavailable.
- [x] GREEN: runtime policy contract reports the new native mutation operation list and dispatch marker.
- [x] Full xUnit/Pester/npm verification.
- [x] Installed explicit admin opt-in VM create/start/restart/poweroff/checkpoint mutation smoke with installer-ISO shutdown unavailable contract.
- [x] Installed explicit admin opt-in successful guest shutdown smoke with a guest OS that has Hyper-V shutdown integration service running.

## Evidence Notes

- Code-level verification proves queue dispatch, parameter validation, runtime policy shape, and injected provider behavior.
- Direct native create smoke root cause: `DefineSystem` with empty resource settings does not provide the same Gen2 device surface as `New-VM`; storage/NIC resources must be added from observed/default Hyper-V WMI RASD shape. The smoke verified final Hyper-V state through `Get-VM`, `Get-VMNetworkAdapter`, `Get-VMDvdDrive`, and `Get-VMHardDiskDrive`.
- Direct lifecycle smoke root cause: Hyper-V v2 `RequestStateChange(10)` is not a reliable running-VM restart primitive for this product path; `RequestStateChange(11)` is the reset primitive. `vm.shutdown` is guest integration dependent; an installer-ISO VM returns `Msvm_ShutdownComponent.InitiateShutdown` `32768`, now surfaced as `PCV_VM_SHUTDOWN_NOT_AVAILABLE`.
- Installed mutation evidence from `0.28.8-admin-smoke` remains valid only as historical context for VM start/poweroff and checkpoint create/restore/delete.
- Fresh installed evidence: `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301/summary.json` reports `ok = true`, `version = 0.30.1-admin-smoke`, unchanged boot time, final service `PureCVisorDesktopNode` running, and no remaining `pcv-*` smoke VMs.
- Fresh route evidence: `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301/hyperv-api-route-smoke.json` records `vm.create`, `vm.start`, `vm.restart`, `vm.poweroff`, `vm.delete`, checkpoint create/restore/delete success, `Default Switch` attachment, installer-ISO `vm.shutdown` structured `PCV_VM_SHUTDOWN_NOT_AVAILABLE`, repeat delete `action=absent`, and unmanaged guard block.
- Successful guest shutdown evidence: `artifacts/guest-shutdown-windows-smoke-20260503-222750/summary.json` reports `ok = true` for installed Local API `POST /api/v1/vms/{id}/shutdown` against a Microsoft Windows Server 2022 Evaluation VHD guest. The smoke verified VHD length `10208214528`, SHA-256 `588355586a3b99f1d47cee02f4861680a7e1bcb353582fbe7da11e2988e7562f`, Generation 1 differencing VM, automatic checkpoints disabled, shutdown integration ready, job `status=succeeded`, final VM `Off`, and no remaining smoke VM or ProgramData root.
- Non-closing diagnostic: Ubuntu 24.04 Azure cloud VHD diagnostics showed Hyper-V shutdown integration `Ok` but did not reach `Off` after WMI `InitiateShutdown(false)`, `InitiateShutdown(true)`, or Hyper-V default graceful stop. That diagnostic is retained only to explain why the successful gate uses the Windows Server evaluation guest.
