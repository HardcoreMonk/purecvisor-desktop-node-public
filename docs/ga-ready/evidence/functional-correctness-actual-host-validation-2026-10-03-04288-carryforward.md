# Functional correctness actual-host validation 2026-10-03 `0.42.88` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-10-03-04288-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.88-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86 (functional-correctness-actual-host-validation-2026-10-02-04286-carryforward) -> 0.42.87 (functional-correctness-actual-host-validation-2026-10-03-04287-carryforward) -> 0.42.88`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.88-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77부터 0.42.87 승격까지와 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.87` (`functional-correctness-actual-host-validation-2026-10-03-04287-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.88` PASS (`full-admin-host-mutation-gate-20261003-04288`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-10-03-04288`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04288 product payload 변경 | managed delete 디스크 정리. 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.87` fullgate provenance `8ade930` 뒤 제품 경로 변경은 managed delete 디스크 정리 하나다. clean package source `ff62e59`와 fullgate provenance `47ff198`에 들어 있고 PR #31 merge `d27086f`에서 도달한다.

| 변경 | commit | 근거 |
| --- | --- | --- |
| `vm delete`가 VM 전용 `ConfigurationDataRoot` 안 미참조 디스크 이미지와 빈 디렉터리를 지우고 `storage_cleanup`을 남긴다 | `96e8570` | `lane2-managed-delete-actual-vm-2026-10-03-04288` PASS(checkpoint 있는 probe VM, 손 정리 없음), fullgate route smoke의 delete 결과 |

이 변경은 feature evidence ledger의 candidate를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
