# SERVICE_PLAN P2 Off VM network-connect actual-VM 2026-09-28 `0.42.81`

evidence_id: `service-plan-p2-offvm-network-connect-actual-vm-2026-09-28-04281`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-network-connect-candidate`
version: `0.42.81-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04281`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family network-connect`
runner_commit: `e3c6d97`
artifact_root: `artifacts/service-plan-p2-offvm-network-connect-actual-vm-20260928-04281`
artifact_summary: `artifacts/service-plan-p2-offvm-network-connect-actual-vm-20260928-04281/summary.json`
summary_sha256: `c3b80ad2ecbbd9b4f878509b5c0060ede8a130be730ae3ee8002b2dc9ac98d11`
runner_sha256: `b81ebb4cd201ce070ee4834e17adc3588108bb5610487142d44e8f2daefedfa8` (실행 시 working tree)
installed_cli_sha256: `554960a18fd35bd10d85700b36d6e787ff684e922137d911132cb4d3e543f939`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04281-net`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.81-admin-smoke`에서 P2-14 VM network connect 기능군 한 프로브를 새 artifact root와 새 VM root로
실행했다. `overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`, 잔여 `pcv-p2-*` VM `0`,
`pcv-*` switch `0`, VmRoot 비움. current-evidence와 feature ledger는 바꾸지 않았다.

선행 FAIL(`0.42.79`, Private switch가 있으면 `network.inventory` topology 실패)이 닫혔다.

| 항목 | 값 |
| --- | --- |
| VM | `pcv-p2-offvm-04281-f92c84f5-net` / `3b6df4d1-a6b2-4cd6-a311-fdd02eac3c68` |
| switch | `pcv-p2-offvm-04281-f92c84f5-sw` (Private) / `6B91E530-7B08-45F1-A7CD-56B6CE9287AD` |
| create | `job-0f59fca177f34bbdbca64803870aca19` `succeeded` |
| connect | `job-fdcd0761c44e45c2acacf4eb8952fb6c` `succeeded` |
| delete | `job-6a55a4a7e626445f9ec545c9f0ee6c1b` `succeeded` |
| Host `switch-create` / `switch-remove` | exit `0` / `0` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2, NIC `1`(Default Switch) | `PASS` |
| `switch_create` | `Ok=true`, `ProductOwned=true`, `AllowManagementOs=false`, WMI switch `1`개 | `PASS` |
| `connect_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, 연결 `Default Switch` 그대로 | `PASS` |
| `connect_switch_missing` | exit `1`, `PCV_NETWORK_SWITCH_NOT_FOUND`, job 없음 | `PASS` |
| `connect` | job succeeded, WMI 연결과 제품 network 모두 전용 switch 하나, VM Off | `PASS` |
| `cleanup` | 제품 delete, 디렉터리 삭제 전 VHD 참조 `0` 확인, 그 뒤 Host `switch-remove`, WMI switch `0` | `PASS` |

## Nonclaims

- Private switch 위 guest 트래픽, Internal/External switch는 검증하지 않았다(`nonclaims` 필드).
- operational current는 `0.42.78-admin-smoke` 그대로다. Lane 3, feature ledger pass를 하지 않았다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
