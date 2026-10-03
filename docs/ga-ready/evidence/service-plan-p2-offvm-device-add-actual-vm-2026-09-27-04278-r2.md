# SERVICE_PLAN P2 Off VM device-add actual-VM 2026-09-27 `0.42.78` r2

evidence_id: `service-plan-p2-offvm-device-add-actual-vm-2026-09-27-04278-r2`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-device-add-candidate`
version: `0.42.78-admin-smoke`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family device-add`
runner_base_commit: `37ad586` (Hyper-V readback 수정은 이 evidence와 같은 commit)
artifact_root: `artifacts/service-plan-p2-offvm-device-add-actual-vm-20260927-04278-r2`
artifact_summary: `artifacts/service-plan-p2-offvm-device-add-actual-vm-20260927-04278-r2/summary.json`
summary_sha256: `9a00cc88bc3f2c4c05c6e6ef0620d110ade4fdfaccd8ded63a420ea39f13ffe4`
runner_sha256: `14401aa74eca9fce11a8a89cd0ca91a70ab32dc93b6f371c3549bb10b67b0b8f` (실행 시 working tree)
installed_cli_sha256: `cdf68bfa72f5d56b21fdddec6af1cac6ef674fecfcc27982bcecd0eebf1eaba1`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04278-dev-r2`
switch: `Default Switch`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.78-admin-smoke`에서 P2-15 device-add 기능군 한 프로브를 새 artifact root와 새 VM root로 실행했다.
`overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`다. current-evidence와 feature ledger는
바꾸지 않았다. 승인: `User-Approval: 2026-09-27 host mutation ok, Off VM 기능군부터`.

| 항목 | 값 |
| --- | --- |
| VM | `pcv-p2-offvm-04278-aede21d3-dev` / `7fe9c97e-481c-4b79-a393-5d8c798766a8` |
| create | `job-6887a1295efb4c92a870eac2e4aaa510` `succeeded` |
| NIC add | `job-9b5d658d70144573b34cad374ab4f146` `succeeded` |
| DVD add | `job-baac1424cd2a4ee0878788dee2045622` `failed` `PCV_VM_DEVICE_ALREADY_PRESENT` |
| delete | `job-0183651a604c45fbb10e372d3c433f2b` `succeeded` |
| 잔여 `pcv-p2-*` VM | `0` |
| VM 디렉터리 | 삭제됨 |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2, NIC `1`, DVD `1`, checkpoint `0` | `PASS` |
| `nic_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, job 없음, NIC `1` | `PASS` |
| `nic_add` | job succeeded, Hyper-V NIC `2`, 제품 network `2`, 두 연결 모두 `Default Switch`, VM Off | `PASS` |
| `nic_limit` | exit `1`, `PCV_VM_DEVICE_LIMIT`, job 없음, NIC `2` | `PASS` |
| `dvd_guard` | route는 job을 받고 native가 `PCV_VM_DEVICE_ALREADY_PRESENT`로 job 실패, DVD `1` | `PASS` |
| `cleanup` | 제품 delete, native fallback 없음, VM 디렉터리 삭제 | `PASS` |

Hyper-V readback은 `root\virtualization\v2` WMI(`Msvm_SyntheticEthernetPortSettingData`,
`Msvm_EthernetPortAllocationSettingData`, DVD drive `ResourceSubType`, `Msvm_SnapshotOfVirtualSystem`)에서 읽었다.

## 선행 r1 FAIL (`artifacts/service-plan-p2-offvm-device-add-actual-vm-20260927-04278`)

r1은 `nic_add`에서 `PCV_P2_OFFVM_STATE_MISMATCH`로 멈췄다(`hyperv_nic_count=1`, `product_nic_count=2`). cleanup은
PASS였다. 일회용 VM 두 대(`pcv-p2-nicdiag-04278-a1`, `-a2`, 둘 다 제품 delete로 정리) 진단 결과 NIC 추가는 WMI에
port `2`, 연결 `2`를 정확히 만들었다. 같은 PowerShell process에서 앞서 VM을 읽은 `Get-VMNetworkAdapter`만 `1`을
돌려줬고 새 process에서는 `2`였다. Hyper-V PowerShell module이 process 안에서 VM device를 cache하는 runner
readback 결함이며 제품 결함이 아니다. runner는 device readback을 WMI 직접 조회로 바꿨고 계약 테스트가 cmdlet
재사용을 막는다.

## report-only

- 제품 inventory `storage`에 ISO DVD drive가 나오지 않아 route DVD guard(`MaxDvdCount=1`)가 DVD를 `0`으로 본다.
  DVD 추가 요청은 `400` 대신 job으로 들어간 뒤 native가 거절한다. 거절은 되지만 층이 다르다.

## Nonclaims

- DVD 추가 성공 경로: 제품 create가 ISO DVD를 붙이므로 제품 VM으로 도달할 수 없다.
- 추가 NIC의 guest link: VM을 시작하지 않았다.
- operational current는 `0.42.78-admin-smoke` 그대로다. Lane 3, feature ledger pass를 하지 않았다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
