# Functional correctness actual-host validation 2026-11-10 `0.43.2` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-11-10-04302-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.2-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92 (functional-correctness-actual-host-validation-2026-10-08-04292-carryforward) -> 0.42.93 (functional-correctness-actual-host-validation-2026-10-08-04293-carryforward) -> 0.42.96 (functional-correctness-actual-host-validation-2026-10-11-04296-carryforward) -> 0.42.98 (functional-correctness-actual-host-validation-2026-11-08-04298-carryforward) -> 0.42.99 (functional-correctness-actual-host-validation-2026-11-09-04299-carryforward) -> 0.43.0 (functional-correctness-actual-host-validation-2026-11-10-04300-carryforward) -> 0.43.1 (functional-correctness-actual-host-validation-2026-11-10-04301-carryforward) -> 0.43.2`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.43.2-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.43.1 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.43.1` (`functional-correctness-actual-host-validation-2026-11-10-04301-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.43.2` PASS (`full-admin-host-mutation-gate-20261110-04302`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-11-10-04302`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04302 product payload 변경 | BL-0042 설치 version 표시(PR #138). 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.43.1` fullgate provenance `143a55b` 뒤 제품 변경은 release train `0.43.2`에 실은 PR #138이다. 변경 commit은 `692895cf`, `424780d8`이고 payload commit `00b10b66`에서 도달한다. 그 뒤 train branch 변경은 train 기록과 Lane 3 문서뿐이고 MSI payload는 바뀌지 않았다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| runtime policy 응답의 `product_version`(Host가 자기 informational version을 넘김), Web Console version 표시 `v<version>`과 title의 전체 값, "✓ 최신 버전" 표시 제거 | `692895cf`, `424780d8` | `lane2-bl0042-probe-2026-11-10-04302` PASS(내주는 파일 5개가 payload와 같음, `runtime/policy`·`ops.summary`의 `product_version`이 설치본 Host ProductVersion과 같음, badge `v0.43.2-admin-smoke`를 1440·768에서 확인, console error 0, HTTP 4xx 0), pair clean-host bucket PASS(guest `0.43.1` → `0.43.2` 제품 Update) |

이 변경은 runtime policy 응답에 null 허용 필드 하나를 더하고 Web Console 표시를 바꾼다. API route, 권한, Hyper-V adapter를 바꾸지 않는다. feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
