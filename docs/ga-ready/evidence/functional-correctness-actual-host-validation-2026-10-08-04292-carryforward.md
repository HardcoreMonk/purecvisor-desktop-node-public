# Functional correctness actual-host validation 2026-10-08 `0.42.92` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-10-08-04292-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.92-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88 (functional-correctness-actual-host-validation-2026-10-03-04288-carryforward) -> 0.42.89 (functional-correctness-actual-host-validation-2026-10-04-04289-carryforward) -> 0.42.90 (functional-correctness-actual-host-validation-2026-10-05-04290-carryforward) -> 0.42.91 (functional-correctness-actual-host-validation-2026-10-06-04291-carryforward) -> 0.42.92`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.92-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.42.91 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.91` (`functional-correctness-actual-host-validation-2026-10-06-04291-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.92` PASS (`full-admin-host-mutation-gate-20261007-04292`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-10-07-04292`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04292 product payload 변경 | 끊긴 `vm.create` 잔여물 표식·회수와 reconcile 안내. 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.91` fullgate provenance `990a4b2` 뒤 제품 payload 경로(`src/DesktopNode.HyperV`, `src/DesktopNode.Api`) 변경은 release train `0.42.92`에 실은 PR #59 하나다(파일 `8`개). 같은 기간의 나머지 `src`·`config` 변경은 시험, 검증 도구, 완료 판정 설정이며 payload에 들어가지 않는다. clean package source `e250950`와 fullgate provenance `b51b8cf`에 들어 있고 PR #62 merge `34b2b16`에서 도달한다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| 끊긴 `vm.create`가 남긴 `disk0.vhdx`를 표식 확인 뒤 같은 이름 create가 회수하고, reconcile not-applied 안내에 위치와 회수 경로를 붙인다 | `ee90474` | `lane2-vm-create-residue-actual-vm-2026-10-07-04292` PASS(설치본, probe VM) |

이 변경은 VM 폴더의 create 잔여물 처리와 reconcile 안내 문구만 바꾸며 feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
