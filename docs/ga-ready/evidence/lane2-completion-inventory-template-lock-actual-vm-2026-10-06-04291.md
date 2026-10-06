# Lane 2 inventory 시각·메모와 template lock 확인 `0.42.91-admin-smoke` (2026-10-06)

evidence_id: `lane2-completion-inventory-template-lock-actual-vm-2026-10-06-04291`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
family: `vm.template.lock` (SERVICE_PLAN P1-7), `vm.list` 시각·메모 (P1-6)
design: `docs/superpowers/specs/2026-09-20-purecvisor-desktop-node-p1-template-lock-design.md`, `docs/superpowers/specs/2026-09-20-purecvisor-desktop-node-p1-inventory-timestamps-design.md`
version: `0.42.91-admin-smoke`
installed_product_version: `0.42.91-admin-smoke+990a4b2713f6d51dca416b476308a0bf92296155`
release_train: `0.42.91-admin-smoke`
source_fix: `c8be5bc` (PR #53 merge `05f42a2`)
artifact_root: `artifacts/lane2-completion-inventory-template-lock-20261006-04291`
probe_summary_sha256: `d605ce6b63a69fd7e8257ea69627aaee374e385c36f35ad1404ddb972dd7f863`
vm_created: `true` (`pcv-probe-c4-inv`, 끝에 삭제)
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

release train `0.42.91`에 실은 PR #53(Hyper-V Notes를 원소 하나로 쓰기)의 Lane 2 probe이자 완료 정의 C4의 P1-6, P1-7 확인이다.
fullgate 설치본 `pcvcli.exe`로 probe VM 하나를 만들어 `2026-10-06T11:02:14Z`부터 `11:02:58Z`까지 실행했다. 보존 VM은 Notes readback만 했다.

0.42.90 설치본에서 같은 probe는 P1-7에서 FAIL했다(`docs/superpowers/plans/2026-10-06-purecvisor-desktop-node-completion-probes.md` Task 1 정차 기록).
lock job은 `succeeded`였지만 Hyper-V Notes에 `template-lock=true`가 남지 않았다. 원인은 여러 줄 Notes를 여러 원소로 쓴 것이었고 PR #53이 고쳤다.

## 결과

| 단계 | 기대 | 관측 |
| --- | --- | --- |
| 보존 VM Notes readback | 운영자 메모, managed marker 줄 없음 | 메모 있음, marker 없음 |
| create | job `succeeded` | `succeeded` |
| Off inventory | `created_at` 있음, `last_powered_on` 없음, `notes` 없음, managed | 그대로 |
| Running inventory | `last_powered_on` 있음, `created_at` 그대로 | 그대로 |
| 다시 Off | `last_powered_on` 없음 | 그대로 |
| template-lock | job `succeeded`, `template_lock=true`, `notes`에 marker 줄 없음 | 그대로 |
| 잠긴 상태 rename | `PCV_VM_TEMPLATE_LOCKED` | 거절 |
| 잠긴 상태 set-memory | `PCV_VM_TEMPLATE_LOCKED` | 거절 |
| 잠긴 상태 start | 허용 | `succeeded` |
| 잠긴 상태 poweroff | `PCV_VM_TEMPLATE_LOCKED` | 거절 |
| template-unlock | `template_lock` 없음 | 그대로 |
| unlock 뒤 poweroff, rename | 허용 | 둘 다 `succeeded` |
| 정리 | probe VM `0`, 보존 VM 그대로 | 그대로 |
| service | Running/Automatic, Web `200` | 그대로 |

`16`단계 모두 PASS다.

## Nonclaims

- probe는 승격 근거가 아니다. operational current는 이 문서를 쓸 때 `0.42.90-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
