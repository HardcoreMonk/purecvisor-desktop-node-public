# Functional correctness actual-host validation 2026-10-11 `0.42.96` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-10-11-04296-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.96-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92 (functional-correctness-actual-host-validation-2026-10-08-04292-carryforward) -> 0.42.93 (functional-correctness-actual-host-validation-2026-10-08-04293-carryforward) -> 0.42.96`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.96-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.42.93 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.93` (`functional-correctness-actual-host-validation-2026-10-08-04293-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.96` PASS (`full-admin-host-mutation-gate-20261011-04296`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-10-11-04296`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04296 product payload 변경 | vm.create readback 대기(BL-0014), VM 상세 submit 위임(BL-0016), Single Edge 프론트엔드 구조(ADR-0018)와 셸 화면 전환(BL-0019), WebPayload `RemoveFolder`(BL-0017). 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.93` fullgate provenance `818d00f` 뒤 제품 변경은 release train `0.42.96`에 실은 PR #78, #79, #81, #84, #86이다(train `0.42.94`와 `0.42.95`는 정차해 승격하지 않았다). 변경 commit은 `1b04ff46`, `04ffe117`, `4e109e70`, `3d50f14b`, `179bd198`이고 payload commit `87a7deb`에서 도달한다. 그 뒤 `main` 변경은 문서, SHA pin, `pcvverify` 수정(PR #87)이고 MSI payload는 바뀌지 않았다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| create 직후 inventory readback이 거절되지 않고, VM 상세 폼 submit이 취소되지 않으며, Web Console이 Single Edge 셸(로그인 페이지+앱 셸, 화면 전환, 도움말·문서 포털·PWA)로 바뀌고, MSI 제거가 web 하위 폴더를 남기지 않는다 | `1b04ff46`, `04ffe117`, `4e109e70`, `179bd198`, `3d50f14b` | `lane2-vm-create-readback-actual-vm-2026-10-11-04296` PASS, `web-console-shell-demo-2026-10-11` PASS, `s3-checkpoint-demo-2026-10-11-04296` PASS(브라우저 예약 저장과 예약 실행), pair Burn bucket PASS |

이 변경들은 vm.create readback 대기, Web Console 표시·구조, installer 폴더 제거를 바꾸며 feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
