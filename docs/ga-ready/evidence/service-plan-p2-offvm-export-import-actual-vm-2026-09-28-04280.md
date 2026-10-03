# SERVICE_PLAN P2 Off VM export-import actual-VM 2026-09-28 `0.42.80`

evidence_id: `service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04280`
result: `FAIL`
failure_class: `product-defect-import-keeps-source-disk-paths`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-export-import-candidate`
version: `0.42.80-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04280`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family export-import`
runner_commit: `92cb141`
artifact_root: `artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04280`
artifact_summary: `artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04280/summary.json`
summary_sha256: `fbc1fa2009cbd76ced28063445d151db1f65f688db9d5cf02594b9937cc0ba13`
runner_sha256: `a30e3f4ee0d8628240eda2ae507db42d9765d135d9b1c95237d0846b9c7347b1` (실행 시 working tree)
installed_cli_sha256: `d952915543e064e2d31a6e472072a64676ed0988d6974473bdfe0bd57e7a5273`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04280-exp`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`overall_verdict=FAIL`, `cleanup.verdict=FAIL`(runner), `secret_observed=false`. runner가 import VM identity를 예약
root 밖이라 거절해 정리하지 않았고, 그 VM은 아래 절차로 수동 정리했다. 최종 잔여 `pcv-p2-*` VM `0`, VmRoot 삭제.

`0.42.79` 결함 두 가지 중 `.vmgs` 거절은 `0.42.80`에서 풀렸다: import job
`job-a5a74d7a81404681a5104c8c435a37d7`이 succeeded다. 그러나 import된 VM의 저장소가 package가 아니다.

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2 | `PASS` |
| `export_confirm_required` | `PCV_CLI_CONFIRMATION_REQUIRED`, 디렉터리 없음 | `PASS` |
| `export_path_not_allowed` | `PCV_VM_EXPORT_PATH_NOT_ALLOWED`, 바깥 디렉터리 없음 | `PASS` |
| `export_preview` | `dry_run=true`, 디렉터리 없음 | `PASS` |
| `export` | `job-ae748245c9b744ceb4e6027409229cc8` succeeded, `.vmcx` `1`, `.vhdx` `1`, `.vmgs` `1` | `PASS` |
| `import_preview` | `dry_run`, `generate_new_id`, `apply_managed_marker` true | `PASS` |
| `import` | job succeeded, 새 VM `b2e4b617-282a-4d86-8c6f-57f5665beb01`, 구성 경로가 예약 root 밖 → `orphan-blocker` | `FAIL` |
| `cleanup` | 소스 VM 제품 delete·디렉터리 삭제 PASS, import VM은 identity 미확정이라 삭제 거절(`PCV_P2_OFFVM_CLEANUP_ID_MISMATCH`) | `FAIL` |

## 제품 결함: import가 소스 디스크 경로를 그대로 쓴다

import 직후 Hyper-V 관측(읽기 전용):

| 항목 | 값 |
| --- | --- |
| import VM | `pcv-p2-offvm-04280-b9298a91-imp` / `b2e4b617-282a-4d86-8c6f-57f5665beb01`, Off, managed marker |
| 구성 위치 | `C:\ProgramData\Microsoft\Windows\Hyper-V` (Hyper-V 기본, package 아님) |
| 디스크 | `D:\data\pcv-p2-offvm-04280-exp\pcv-p2-offvm-04280-b9298a91-exp\disk0.vhdx` = **소스 VM 디스크** |
| snapshot 위치 | 소스 VM 디렉터리 |
| package 디스크 | `exports\...\Virtual Hard Disks\disk0.vhdx`는 어떤 VM도 참조하지 않음 |

`DesktopNodeHyperVWmiVmImportProvider`는 `ImportSystemDefinition`으로 planned VM을 만든 뒤 이름과 marker만 바꾸고
`RealizePlannedSystem`한다. planned VM의 VHD 경로, 구성·snapshot 위치를 package나 전용 root로 옮기지 않는다.
그래서 소스가 남아 있으면 두 VM이 같은 VHDX를 가리킨다(데이터 손상 위험). 소스가 지워지면 import VM 디스크가
사라진다. 이 run에서는 runner가 소스 디렉터리를 지운 뒤라 import VM 디스크 참조가 끊겼다(테스트 VM에 한함).

## 수동 정리 (2026-09-28)

1. `Get-VM -Name pcv-p2-offvm-04280-b9298a91-imp`가 정확히 1개, Id `b2e4b617-…`(summary `observed_id`와 같음),
   managed marker 확인.
2. `pcvcli vm delete pcv-p2-offvm-04280-b9298a91-imp --yes` → `job-acd6d4f881b7418c90436700c174608c` succeeded.
3. Id로 VM 없음, Hyper-V 기본 위치 `.vmcx` 없음 확인 뒤 VmRoot `D:\data\pcv-p2-offvm-04280-exp` 삭제.
4. 잔여 `pcv-p2-*` VM `0`.

## report-only

- runner는 identity를 확정하지 못한 VM이 참조하는 소스 디렉터리를 지웠다. 소스 root 삭제 전에 다른 VM의 디스크
  참조를 확인하는 guard가 있으면 더 안전하다.

## Nonclaims

- export/import 왕복 PASS를 주장하지 않는다. `pcv.vm.managed-import` Lane 2는 FAIL이다.
- import 저장소 처리 방식(package 디스크 제자리 사용 또는 전용 root로 복사)은 설계 결정 뒤 수정한다.
- operational current는 `0.42.78-admin-smoke` 그대로다. public trusted signing, external stable publication을 주장하지 않는다.
