# SERVICE_PLAN P2 Off VM export-import actual-VM 2026-09-28 `0.42.81`

evidence_id: `service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04281`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-export-import-candidate`
version: `0.42.81-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04281`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family export-import`
runner_commit: `1add41d`
artifact_root: `artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04281`
artifact_summary: `artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04281/summary.json`
summary_sha256: `c2086ccb84fd22efab4e5cb63b1bac073c7db88248cfc6728ec432cf8f869dec`
runner_sha256: `b81ebb4cd201ce070ee4834e17adc3588108bb5610487142d44e8f2daefedfa8` (실행 시 working tree)
installed_cli_sha256: `554960a18fd35bd10d85700b36d6e787ff684e922137d911132cb4d3e543f939`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04281-exp`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.81-admin-smoke`에서 P2-13 export/import 기능군 한 프로브를 새 artifact root와 새 VM root로
실행했다. `overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`, 잔여 `pcv-p2-*` VM `0`,
VmRoot 비움. current-evidence와 feature ledger는 바꾸지 않았다.

선행 FAIL 세 건(`0.42.78` Off 어휘, `0.42.79` `.vmgs` 거절, `0.42.80` 소스 디스크 공유)이 모두 닫혔다.

| 항목 | 값 |
| --- | --- |
| 소스 VM | `pcv-p2-offvm-04281-94a957a2-exp` / `4c8259b0-6020-4732-8f95-e8a5e187d241` |
| import VM | `pcv-p2-offvm-04281-94a957a2-imp` / `17c6423a-4f8e-43ca-8e34-6ce147b7d422` |
| create | `job-92288ffa05414869963208ddf732849d` `succeeded` |
| export | `job-a8549854228649788788253346887dcd` `succeeded` |
| import | `job-fda791155fab4ce29413979c14939602` `succeeded` |
| delete import → 소스 | `job-e25227ff7d784367a97c01064ae91635`, `job-84683acb674a43308f35637b157c5b22` `succeeded` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2, checkpoint `0` | `PASS` |
| `export_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, 디렉터리 없음 | `PASS` |
| `export_path_not_allowed` | exit `1`, `PCV_VM_EXPORT_PATH_NOT_ALLOWED`, 바깥 디렉터리 없음 | `PASS` |
| `export_preview` | `dry_run=true`, `host_mutation_performed=false`, 디렉터리 없음 | `PASS` |
| `export` | `.vmcx` `1`, `.vhdx` `1`, `.vmgs` `1`(Gen2 guest state), 소스 Off | `PASS` |
| `import_preview` | `dry_run`, `generate_new_id`, `apply_managed_marker` true, 대상 없음 | `PASS` |
| `import` | 새 identity, 구성 `VmRoot\...-imp`, 디스크 `VmRoot\...-imp\disk0.vhdx`, 소스 디스크 공유 `0`, package VHDX 유지 `1`, managed, 양쪽 Off | `PASS` |
| `cleanup` | import VM 다음 소스 제품 delete, 디렉터리 삭제 전 VHD 참조 `0` 확인, export root 삭제 | `PASS` |

## Nonclaims

- import VM guest 부팅, OVF/TPM 거절 경로는 이 run에서 실행하지 않았다(`nonclaims` 필드).
- operational current는 `0.42.78-admin-smoke` 그대로다. Lane 3, feature ledger pass를 하지 않았다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
