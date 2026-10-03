# SERVICE_PLAN P2 Off VM export-import actual-VM 2026-09-28 `0.42.79` r2

evidence_id: `service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04279-r2`
result: `FAIL`
failure_class: `product-defect-import-rejects-own-gen2-export`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-export-import-candidate`
version: `0.42.79-admin-smoke` (probe vehicle, `admin-smoke-package-2026-09-28-04279`)
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family export-import`
runner_base_commit: `cd259a5` (`.vmgs` 기록 전용 수정은 이 evidence와 같은 commit)
artifact_root: `artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04279-r2`
artifact_summary: `artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04279-r2/summary.json`
summary_sha256: `75edd97151d13705fb611d86ec69a9bae8e4da6b7a0d5e8ecbe537974fd26f34`
runner_sha256: `a30e3f4ee0d8628240eda2ae507db42d9765d135d9b1c95237d0846b9c7347b1` (실행 시 working tree)
installed_cli_sha256: `ab9686e4e381e6c0f110bf422786dd20fe880300c704f89163bb281a3bf5ee5f`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04279-exp-r2`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`overall_verdict=FAIL`, `cleanup.verdict=PASS`, `secret_observed=false`, 잔여 `pcv-p2-*` VM `0`, VmRoot 비움.
Off 어휘 수정(`9402774`)은 설치본에서 동작한다: export 확인·allowlist 거절·preview·export job이 모두 PASS다.
실패는 import job이다. 제품이 자기 export package를 import하지 못한다.

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2, checkpoint `0` | `PASS` |
| `export_confirm_required` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, 디렉터리 없음 | `PASS` |
| `export_path_not_allowed` | exit `1`, `PCV_VM_EXPORT_PATH_NOT_ALLOWED`, 바깥 디렉터리 없음 | `PASS` |
| `export_preview` | `dry_run=true`, `host_mutation_performed=false`, 디렉터리 없음 | `PASS` |
| `export` | `job-55281e619c4d4d189f80ca9053840055` succeeded, `.vmcx` `1`, `.vhdx` `1`, `.vmgs` `1`, 소스 Off | `PASS` |
| `import_preview` | `dry_run`, `generate_new_id`, `apply_managed_marker` true, 대상 없음 | `PASS` |
| `import` | `job-7c3172102681469c8d1f8b328225b7c7` failed `PCV_VM_IMPORT_SECURITY_FEATURES_UNSUPPORTED` | `FAIL` |
| `cleanup` | 소스 제품 delete(`job-c4795d97d7c147788ff08cee3f1ef68e`), import VM 없음, export root 삭제 | `PASS` |

소스 VM: `pcv-p2-offvm-04279-75df6cda-exp` / `57aac742-8455-4ff8-8143-4bff66991ae0`.

## 원인

`DesktopNodeHyperVWmiVmImportProvider.Invoke`는 package 안에 `.vmgs` 파일이 있으면 곧바로
`PCV_VM_IMPORT_SECURITY_FEATURES_UNSUPPORTED`로 거절한다. Hyper-V는 구성 version 8 이상 Generation 2 VM의
guest state(UEFI 등)를 TPM 유무와 관계없이 `.vmgs`에 두고 export에 포함한다. 제품 export 정책은 TPM/key
protector/shielded VM을 이미 거절하므로 이 `.vmgs`에는 vTPM 키가 없다. 설계
(`docs/superpowers/specs/2026-09-21-purecvisor-desktop-node-p2-hyperv-export-import-design.md`)의 의도는 “TPM, key
protector, shielded가 있으면 거절, vTPM 키와 `.vmgs` 키 자료를 내보내지 않음”인데, 구현은 `.vmgs` 파일 존재
자체를 TPM 신호로 쓴다. import provider는 planned VM의 `Msvm_SecuritySettingData`(TPM, shielding, key protector)
검사도 따로 한다.

## 선행 r1 (`artifacts/service-plan-p2-offvm-export-import-actual-vm-20260928-04279`)

r1은 `export` slice에서 `PCV_P2_OFFVM_STATE_MISMATCH`로 멈췄다. export job은 succeeded였고 runner가 `.vmgs`
`0`을 요구한 잘못된 가정이 원인이다. cleanup PASS, 잔여 VM `0`. runner는 `.vmgs` 수를 기록만 하도록 고쳤다.

## Nonclaims

- export/import 왕복 PASS를 주장하지 않는다. `pcv.vm.managed-import` Lane 2는 FAIL이다.
- 수정은 보안 guard 변경이라 설계 결정 뒤 별도 Lane 1 checkpoint에서 한다.
- operational current는 `0.42.78-admin-smoke` 그대로다. public trusted signing, external stable publication을 주장하지 않는다.
