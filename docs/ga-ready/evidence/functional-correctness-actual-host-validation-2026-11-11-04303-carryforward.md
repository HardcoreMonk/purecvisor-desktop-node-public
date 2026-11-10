# Functional correctness actual-host validation 2026-11-11 `0.43.3` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-11-11-04303-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.3-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92 (functional-correctness-actual-host-validation-2026-10-08-04292-carryforward) -> 0.42.93 (functional-correctness-actual-host-validation-2026-10-08-04293-carryforward) -> 0.42.96 (functional-correctness-actual-host-validation-2026-10-11-04296-carryforward) -> 0.42.98 (functional-correctness-actual-host-validation-2026-11-08-04298-carryforward) -> 0.42.99 (functional-correctness-actual-host-validation-2026-11-09-04299-carryforward) -> 0.43.0 (functional-correctness-actual-host-validation-2026-11-10-04300-carryforward) -> 0.43.1 (functional-correctness-actual-host-validation-2026-11-10-04301-carryforward) -> 0.43.2 (functional-correctness-actual-host-validation-2026-11-10-04302-carryforward) -> 0.43.3`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.43.3-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.43.2 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.43.2` (`functional-correctness-actual-host-validation-2026-11-10-04302-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.43.3` PASS (`full-admin-host-mutation-gate-20261111-04303`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-11-11-04303`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04303 product payload 변경 | BL-0043 대시보드 "경고" 카드(PR #142). 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.43.2` fullgate provenance `2f19778` 뒤 제품 변경은 release train `0.43.3`에 실은 PR #142다. 변경 commit은 `3d1ce598`이고 payload commit `69675e0c`에서 도달한다. 그 뒤 train branch 변경은 train 기록과 Lane 3 문서뿐이고 MSI payload는 바뀌지 않았다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| 새 갱신에 밀려 취소된 요청이 Web Console 상태(오류와 데이터)를 쓰지 않는다. 읽기 함수 일곱 개가 이 규칙을 따르고, 대시보드 "경고" 카드가 운영 요약을 받은 뒤 "운영 요약을 읽지 못했습니다"를 보이지 않는다 | `3d1ce598` | `lane2-bl0043-probe-2026-11-11-04303` PASS(내주는 파일 5개가 payload와 같음, 1440·768에서 새 context 세 번씩 모두 첫 갱신 뒤 카드가 목록으로 바뀜, version 표시 `v0.43.3-admin-smoke`, console error 0, HTTP 4xx 0), pair clean-host bucket PASS(guest `0.43.2` → `0.43.3` 제품 Update) |

이 변경은 Web Console의 읽기 함수가 취소된 요청의 결과를 버리게 한다. API route, 권한, 서버 코드, Hyper-V adapter를 바꾸지 않는다. feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
