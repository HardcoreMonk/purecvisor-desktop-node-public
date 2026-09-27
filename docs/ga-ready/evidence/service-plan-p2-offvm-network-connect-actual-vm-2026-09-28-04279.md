# SERVICE_PLAN P2 Off VM network-connect actual-VM 2026-09-28 `0.42.79`

evidence_id: `service-plan-p2-offvm-network-connect-actual-vm-2026-09-28-04279`
result: `FAIL`
failure_class: `product-defect-private-switch-breaks-network-inventory`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-network-connect-candidate`
version: `0.42.79-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04279`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family network-connect`
runner_commit: `82861ce`
artifact_root: `artifacts/service-plan-p2-offvm-network-connect-actual-vm-20260928-04279`
artifact_summary: `artifacts/service-plan-p2-offvm-network-connect-actual-vm-20260928-04279/summary.json`
summary_sha256: `dc50ab8167772ccce2595a0fc5baa7663bfb75fd7609eff180623501a9b15cbb`
runner_sha256: `a30e3f4ee0d8628240eda2ae507db42d9765d135d9b1c95237d0846b9c7347b1` (실행 시 working tree)
installed_cli_sha256: `ab9686e4e381e6c0f110bf422786dd20fe880300c704f89163bb281a3bf5ee5f`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04279-net`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`overall_verdict=FAIL`, `cleanup.verdict=PASS`, `secret_observed=false`. 잔여 `pcv-p2-*` VM `0`, `pcv-*` switch `0`,
VmRoot 비움. 승인: `User-Approval: 2026-09-28 network-connect 지금 실행`.

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2, NIC `1`(Default Switch) | `PASS` |
| `switch_create` | Host `service-action switch-create --switch-type private` exit `0`, `Ok=true`, `ProductOwned=true`, `AllowManagementOs=false`, WMI switch `1`개 `3925902A-2A30-40A9-B64F-3DCBDC972785` | `PASS` |
| `connect_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, 연결 `Default Switch` 그대로 | `PASS` |
| `connect_switch_missing` | exit `1`, `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`(기대 `PCV_NETWORK_SWITCH_NOT_FOUND`) | `FAIL` |
| `connect` | 실행 안 함 | `NOT_RUN` |
| `cleanup` | 제품 delete(`job-2b4156432ed04844a90d6fb2bac78ac4`), VM 디렉터리 삭제, 그 뒤 Host `switch-remove` exit `0`, WMI switch `0` | `PASS` |

VM: `pcv-p2-offvm-04279-c7f1a842-net` / `35351f6b-7276-49e9-9417-a7897ea357ae`, switch
`pcv-p2-offvm-04279-c7f1a842-sw`.

## 원인

`DesktopNodeHyperVWmiSwitchProvider.MapSwitch`는 Default Switch와 host management port가 있는 Internal switch만
분류한다. management port도 external binding도 없는 Private switch는 `Type="unknown"`,
`AllowManagementOs=null`이 되고, `HasCompleteSwitchTopology`가 false라서 `network.inventory` 전체가
`PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`로 실패한다. VM network connect와 NIC 추가 route는 먼저
`network.inventory`를 읽으므로 Private switch가 하나라도 있으면 host 전체에서 둘 다 실패한다. 제품
`switch-create`와 `NetworkChangePolicy`는 Private(`allow_management_os=false`)를 지원한다.

External switch도 같은 분기로 `unknown`이 된다(report-only, 이 호스트에는 External switch가 없다).

## Nonclaims

- network connect 성공 경로 PASS를 주장하지 않는다. P2-14 Lane 2는 FAIL이다.
- Host service-action switch create/remove(Private)는 설치본에서 정상 동작했다. 이것은 P2-14 connect PASS가 아니다.
- operational current는 `0.42.78-admin-smoke` 그대로다. public trusted signing, external stable publication을 주장하지 않는다.
