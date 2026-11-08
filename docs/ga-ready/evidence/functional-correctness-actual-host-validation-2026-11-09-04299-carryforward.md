# Functional correctness actual-host validation 2026-11-09 `0.42.99` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-11-09-04299-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.99-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92 (functional-correctness-actual-host-validation-2026-10-08-04292-carryforward) -> 0.42.93 (functional-correctness-actual-host-validation-2026-10-08-04293-carryforward) -> 0.42.96 (functional-correctness-actual-host-validation-2026-10-11-04296-carryforward) -> 0.42.98 (functional-correctness-actual-host-validation-2026-11-08-04298-carryforward) -> 0.42.99`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.99-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.99-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.42.98 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.98` (`functional-correctness-actual-host-validation-2026-11-08-04298-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.99` PASS (`full-admin-host-mutation-gate-20261109-04299`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-11-09-04299`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04299 product payload 변경 | in-app 안내서 갱신(PR #113), console 입력 제한기 clock seam(PR #115, BL-0011), Web Console 수정 일곱 건과 native VM 목록 일시 공백 재읽기·켜진 VM 복원 거절(PR #116, backlog B2). 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.98` fullgate provenance `2dfe2ac` 뒤 제품 변경은 release train `0.42.99`에 실은 PR #113, #115, #116이다. 변경 commit은 `154d16cc`, `0575b859`, `5f6c1f0c`이고 payload commit `f7426d1`에서 도달한다. 그 뒤 `main` 변경은 train 기록 PR #118(문서)뿐이고 MSI payload는 바뀌지 않았다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| in-app 안내서가 새 Web Console 구조를 설명하고, console 입력 제한기가 hardening clock으로 창을 재며, 삭제 중 VM 목록이 일시 요약 공백에 실패하지 않고, 켜진 VM의 checkpoint 복원이 전용 코드로 거절되며, Web Console의 예약 입력·포커스·표시·테마·문구가 고쳐진다 | `154d16cc`, `0575b859`, `5f6c1f0c` | `lane2-backlog-b2-probe-2026-11-09-04299` PASS(삭제 중 목록 실패 0회, 복원 거절 `PCV_CHECKPOINT_RESTORE_VM_RUNNING`), pair clean-host bucket PASS(guest `0.42.98` → `0.42.99` 제품 Update) |

이 변경들은 Web Console 표시·문구, native VM 목록 읽기, checkpoint 복원 사전 거절, 입력 제한기 시계를 바꾸며 feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
