# PureCVisor Desktop Node VM Delete Native Adapter

**Goal:** `DELETE /api/v1/vms/{id}`를 served current route로 승격하고, queued `vm.delete` job execution을 PowerShell helper 없이 C# WMI native adapter로 처리한다.

**Architecture:** `DesktopNodeApiRequestProcessor`는 기존 job queue/job store shape를 유지한다. Route는 `DELETE /api/v1/vms/{id}`를 `vm.delete` job으로 enqueue하고 worker는 `DesktopNodeHyperVNativeAdapter`를 호출한다. Native adapter는 VM inventory에서 id/name을 찾고, `managed-by=purecvisor-desktop-node` marker가 없는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다. Missing VM은 idempotent delete contract로 `action=absent` success를 반환한다. 실제 WMI provider는 `Msvm_VirtualSystemManagementService.DestroySystem`을 호출하되, code-level tests는 injectable provider만 사용한다.

## Scope

- `DELETE /api/v1/vms/{id}` route contract
- queued `vm.delete` job creation and worker dispatch
- native adapter validation and provider handoff
- runtime policy `native_mutation_operations` and mutation dispatch marker update
- GA-ready route matrix current-route 후보 반영

## Out of Scope

- VHD/config directory data deletion
- firewall/Event Log/trust-store mutation
- public trusted signing 또는 GA 승격

## Tasks

- [x] RED: route contract/test에 `DELETE /api/v1/vms/{id}`와 `vm.delete` native dispatch 기대값 추가.
- [x] GREEN: request processor가 VM delete route를 queued job으로 만들고 worker가 native adapter로 실행하도록 구현.
- [x] GREEN: native adapter가 managed marker guard, missing VM idempotency, invalid name guard를 enforced contract로 제공.
- [x] GREEN: WMI provider에 `DestroySystem` product path를 추가하되 tests는 fake provider만 사용.
- [x] GREEN: runtime policy와 diagnostics self-audit fixture를 `vm.delete` 포함 boundary로 갱신.
- [x] Docs: GA-ready route matrix와 개발 문서에 code-level evidence 및 installed destructive smoke evidence 반영.
- [x] Admin smoke: `0.30.1-admin-smoke` 설치본 service-action, MSI lifecycle, Hyper-V API route smoke에서 managed VM delete, repeat absent, unmanaged guard block, cleanup/no-reboot evidence 확인.

## Verification

- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "ApiHandlerAdapterContractTests|QueuedVmDelete|NativeVmDelete|WmiVmDelete|MutationRoutesQueueJobsWithoutInvokingHelper"`: PASS.
- `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter RuntimePolicy`: PASS.

## Installed Evidence

- `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301/summary.json`: `ok=true`, version `0.30.1-admin-smoke`, final service `Running`, boot time unchanged, `remaining_pcv_vms=[]`.
- `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301/hyperv-api-route-smoke.json`: managed VM delete job `succeeded` with `action=delete`, repeat delete job `succeeded` with `action=absent`, unmanaged VM delete job `failed` with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, unmanaged VM remained after block and was removed by scoped cleanup.
- `artifacts/routeparity-service-msi-hyperv-vm-delete-mutation-20260503-0301/msi-lifecycle-smoke.json`: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`; no reboot initiated.
