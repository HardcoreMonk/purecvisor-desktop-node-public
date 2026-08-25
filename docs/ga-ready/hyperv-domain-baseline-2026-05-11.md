# Hyper-V Domain 기준선 - 2026-05-11

source_project: `src/DesktopNode.HyperV`
target_project: `src/DesktopNode.HyperV`
behavior_change_allowed: false
host_mutation_default: explicit-admin-opt-in-only
domain_catalog: `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs`
wmi_provider_catalog: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs`
provider_file_split: `src/DesktopNode.HyperV/DesktopNodeHyperVWmi*Provider.cs`

## 현재 Route 소유권

- `GET /api/v1/host/status`: native Hyper-V host status provider가 소유한다.
- `GET /api/v1/network/inventory`: native WMI switch provider가 소유한다.
- `GET /api/v1/vms`: native WMI VM provider가 소유한다.
- `GET /api/v1/vms/{vmId}`: native VM inventory result가 소유한다.
- `GET /api/v1/vms/{vmId}/checkpoints`: native checkpoint provider가 소유한다.
- VM lifecycle queued mutation: native WMI lifecycle provider가 소유한다.
- Checkpoint lifecycle queued mutation: native WMI snapshot provider가 소유한다.

## Domain Catalog 계약

- read operation: `host.status`, `network.inventory`, `vm.list`, `checkpoint.list`
- mutation operation: `vm.create`, `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart`, `vm.delete`, `checkpoint.create`, `checkpoint.restore`, `checkpoint.delete`
- adapter dispatch는 `DesktopNodeHyperVDomain.Catalog`를 먼저 조회한 뒤 provider boundary로 위임한다.
- catalog 밖 operation은 `PCV_NATIVE_ROUTE_NOT_HANDLED`로 거절한다.
- `DesktopNodeHyperVAdapterDispatchCatalog`는 domain kind, domain, provider boundary, handler를 함께 검증하며 drift는 `PCV_NATIVE_DISPATCH_PROVIDER_BOUNDARY_DRIFT`로 거절한다.

## 0.42.18 이후 Dispatch Catalog 세부 계약

`docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`는
`hyperv_dispatch_catalog_contract=vm-checkpoint-network-fixed`로 VM/checkpoint/network
세부 domain을 문서 계약으로 고정한다.

| Domain | Operation | Handler | Provider boundary |
| --- | --- | --- | --- |
| network | `network.inventory` | `NetworkInventory` | `switch-provider` |
| VM inventory | `vm.list` | `VmList` | `vm-provider` |
| VM create | `vm.create` | `VmCreate` | `vm-create-provider` |
| VM power state | `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart` | `VmPowerState` | `vm-power-state-provider` |
| VM delete | `vm.delete` | `VmDelete` | `vm-delete-provider` |
| checkpoint read | `checkpoint.list` | `CheckpointList` | `checkpoint-provider` |
| checkpoint mutation | `checkpoint.create`, `checkpoint.restore`, `checkpoint.delete` | `CheckpointMutation` | `checkpoint-mutation-provider` |

## WMI Provider Catalog 계약

- `host-status-provider`: `IDesktopNodeHyperVHostStatusProvider` / `DesktopNodeHyperVNativeHostStatusProvider`
- `switch-provider`: `IDesktopNodeHyperVSwitchProvider` / `DesktopNodeHyperVWmiSwitchProvider`
- `vm-provider`: `IDesktopNodeHyperVVmProvider` / `DesktopNodeHyperVWmiVmProvider`
- `checkpoint-provider`: `IDesktopNodeHyperVCheckpointProvider` / `DesktopNodeHyperVWmiCheckpointProvider`
- `vm-create-provider`: `IDesktopNodeHyperVVmCreateProvider` / `DesktopNodeHyperVWmiVmCreateProvider`
- `vm-power-state-provider`: `IDesktopNodeHyperVVmPowerStateProvider` / `DesktopNodeHyperVWmiVmPowerStateProvider`
- `vm-delete-provider`: `IDesktopNodeHyperVVmDeleteProvider` / `DesktopNodeHyperVWmiVmDeleteProvider`
- `checkpoint-mutation-provider`: `IDesktopNodeHyperVCheckpointMutationProvider` / `DesktopNodeHyperVWmiCheckpointMutationProvider`

## 분리 순서

1. Interface와 record 이동 상태를 유지한다.
2. Native adapter orchestration은 `DesktopNode.HyperV` project에서 유지한다.
3. Domain catalog로 read/mutation/provider boundary를 고정한다.
4. WMI provider implementation은 `DesktopNodeHyperVWmiProviderCatalog` provider boundary coverage test를 유지한 상태에서 provider별 파일로 분리한다.
5. API response contract는 변경하지 않는다.
