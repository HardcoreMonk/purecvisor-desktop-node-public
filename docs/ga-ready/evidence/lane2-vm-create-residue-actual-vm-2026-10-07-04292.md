# Lane 2 끊긴 `vm.create` 잔여물 회수 확인 `0.42.92-admin-smoke` (2026-10-08)

evidence_id: `lane2-vm-create-residue-actual-vm-2026-10-07-04292`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
family: `vm.create` 잔여물 표식·회수·거절, `job.reconcile` `vm.create` not-applied 안내 (release train `0.42.92` 적재 PR #59)
design: `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-interrupted-create-residue-design.md` (`pcv-interrupted-create-residue-v1`)
version: `0.42.92-admin-smoke`
installed_product_version: `0.42.92-admin-smoke+b51b8cf804288121abcd8ea3724714cd7f9fc9f6`
release_train: `0.42.92-admin-smoke`
artifact_root: `artifacts/lane2-vm-create-residue-20261007-04292-r2` (이전 시도 `-04292` 보존)
probe_summary_sha256: `92feaa49c1650ad9dde147ccf7b5f2f41bcb366b2f4a4250a5b813fcac82c189`
vm_created: `true` (`pcv-probe-c2-residue`, 끝에 삭제)
host_mutation_performed: `true` (probe VM 생성·삭제, 서비스 프로세스 종료와 재시작, VM root 아래 probe 이름 폴더와 빈 파일)
secret_observed: `false`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 방법

`pcvcli vm create pcv-probe-c2-residue`를 큐에 넣고, VM 폴더에 `.pcv-create-pending.json`과 `disk0.vhdx`가 생긴 순간 `DesktopNode.Host`
프로세스를 강제 종료한 뒤 서비스를 다시 올렸다. 그다음 `job reconcile`, 같은 이름 `vm create`, 표식 없는 폴더(`pcv-probe-c2-nomarker`,
빈 `disk0.vhdx`)에 대한 `vm create`, managed `vm delete`를 차례로 실행하고 파일 시스템과 `vm list`로 따로 확인했다.

## 결과 (r2)

| 확인 | 기대 | 관측 |
| --- | --- | --- |
| 끊김 | VM 등록 전 `PCV_JOB_INTERRUPTED`, 잔여물과 표식 | 첫 시도에 `PCV_JOB_INTERRUPTED`, VM 행 `0`, `disk0.vhdx` `4194304` B, `.pcv-create-pending.json` `283` B |
| 표식 | `pcv-vm-create-pending/v1`, 필드 `7`개, 비밀 값 없음 | `schema`, `vm_name`, `vhd_file`, `directory_created`(`true`), `owner_process_id`, `owner_process_start_utc`, `created_utc` |
| reconcile | readback만, `not-applied`, 위치와 회수 경로 안내 | `PCV_JOB_RECONCILIATION_REQUIRED` `not-applied`, hint `A create interrupted before VM registration can leave a disk in …`, 파일 그대로 |
| 같은 이름 create | 표식 있는 잔여물 회수 뒤 성공 | `succeeded`, 단계 `Remove interrupted create residue`, `recovered_residue`, managed VM, 표식 삭제 |
| 표식 없는 폴더 | 지우지 않고 거절 | `PCV_VHD_ALREADY_EXISTS` detail `no-create-marker`, 파일 유지, VM 없음 |
| managed delete | 폴더 없음 | `succeeded`, 폴더 없음, VM 행 `0` |

r2는 `18`초 걸렸다. 0.42.91 probe(`lane2-completion-family-reconcile-actual-vm-2026-10-06-04291`)에서 같은 끊김 뒤 다음 create가
`PCV_VHD_ALREADY_EXISTS`로 막혀 손으로 지워야 했던 결함이 0.42.92 설치본에서 닫혔다.

## 시도 기록

- r1: 다섯 확인과 정리를 모두 돌았고 판정도 r2와 같았다(artifact의 job JSON). 마지막 summary 계산에서 스크립트 결함(판정이 아닌
  `residue_files` 항목을 판정 목록에 넣음)으로 멈춰 `summary.json`을 쓰지 못했다.
- r2: 그 항목을 판정 밖으로 옮겨 다시 돌렸다.

## 끝 상태

probe VM과 VM 폴더 `0`, 보존 VM `pcv-guest-installed-04253-r1` stopped(시작과 같음), service Running/Automatic, Web `200`.

## Nonclaims

- probe는 승격 근거가 아니다. `pcv.vm.create`는 feature evidence ledger 후보가 아니다.
- `DefineSystem` 뒤 장치 연결 전에 끊긴 create의 reconcile 판정(backlog `BL-0001`)은 이 probe가 다루지 않는다.
- Hyper-V exactly-once와 reconcile 완전성을 주장하지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
