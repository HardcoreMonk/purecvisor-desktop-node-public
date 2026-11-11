# Functional correctness actual-host validation 2026-11-12 `0.43.7` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-11-12-04307-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.7-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92 (functional-correctness-actual-host-validation-2026-10-08-04292-carryforward) -> 0.42.93 (functional-correctness-actual-host-validation-2026-10-08-04293-carryforward) -> 0.42.96 (functional-correctness-actual-host-validation-2026-10-11-04296-carryforward) -> 0.42.98 (functional-correctness-actual-host-validation-2026-11-08-04298-carryforward) -> 0.42.99 (functional-correctness-actual-host-validation-2026-11-09-04299-carryforward) -> 0.43.0 (functional-correctness-actual-host-validation-2026-11-10-04300-carryforward) -> 0.43.1 (functional-correctness-actual-host-validation-2026-11-10-04301-carryforward) -> 0.43.2 (functional-correctness-actual-host-validation-2026-11-10-04302-carryforward) -> 0.43.3 (functional-correctness-actual-host-validation-2026-11-11-04303-carryforward) -> 0.43.4 (functional-correctness-actual-host-validation-2026-11-11-04304-carryforward) -> 0.43.7`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.7-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.43.7-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.43.4 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.43.4` (`functional-correctness-actual-host-validation-2026-11-11-04304-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.43.7` PASS (`full-admin-host-mutation-gate-20261111-04307`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-11-11-04307`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04307 product payload 변경 | 대시보드 첫 로드(PR #152), 콘솔 입력 순서·키보드 경로 캐시(PR #153), 콘솔 route 전역 lock 밖·loopback 세션 하나(PR #157), 콘솔 route 전역 요청 한도 제외·첫 service worker claim reload 없음(PR #159). 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.43.4` fullgate provenance `7b929ec` 뒤 제품 변경은 release train `0.43.7`에 실은 PR #152, #153, #157, #159다(train `0.43.5`·`0.43.6`은 probe FAIL로 정차했다). 변경 commit은 `ff40acd7`, `25709198`, `39976fd2`, `04b1a91b`이고 payload commit `bf5c214`에서 도달한다. 그 뒤 train branch 변경은 train 기록과 Lane 3 문서뿐이고 MSI payload는 바뀌지 않았다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| 대시보드 첫 로드는 갱신 하나로 돈다(PR #152). 같은 VM 콘솔 입력은 도착 순서로 처리하고 키보드 경로를 VM마다 기억한다(PR #153). 콘솔 frame·input은 전역 Local API lock 밖에서 처리하고 한 페이지는 loopback 세션을 한 번만 요청한다(PR #157). 콘솔 route는 전역 요청 한도에서 빠지고(VM별 한도는 그대로), 첫 service worker claim에서 reload하지 않는다(PR #159) | `ff40acd7`, `25709198`, `39976fd2`, `04b1a91b` | `lane2-dashboard-console-probe-2026-11-11-04307` PASS(내주는 파일 5개가 payload와 같음, 1440·768에서 새 context 세 번씩 `ops/summary` 1회·취소 0회·문서 로드 1회, 4.9~5.7초에 목록, 콘솔 키 입력 188건 중앙값 176ms·거절 0건, 20자 두 칸 일치, version 표시 `v0.43.7-admin-smoke`, console error 0, HTTP 4xx 0), pair clean-host bucket PASS(guest `0.43.6` → `0.43.7` 제품 Update) |

이 변경은 Web Console 첫 로드와 콘솔 입력 경로, Local API의 콘솔 route 처리, Hyper-V 콘솔 키보드 호출 순서를 바꾼다. API route, 권한, 저장 형식은 바꾸지 않는다. QoS/disk functional 경로를 바꾸지 않으므로 feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다. 텍스트 보내기 폼(BL-0047)은 열린 결함으로 남는다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
