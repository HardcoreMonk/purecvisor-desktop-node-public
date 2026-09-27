# Functional correctness actual-host validation 2026-09-27 `0.42.78` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-09-27-04278-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.78-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.78-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 재실행하지 않았다.
사용자 결정(2026-09-27)에 따라 아래를 근거로 `0.42.75` functional PASS를 carry-forward한다.
2026-09-27에 artifact summary SHA-256이 위 값과 같음을 다시 확인했다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.77` (`functional-correctness-actual-host-validation-2026-09-20-04277-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.78` PASS (`full-admin-host-mutation-gate-20260927-04278-r2`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-09-27-04278`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)가 `0.42.75` actual-VM PASS이며 ledger는 `bb15f66` 뒤로 바뀌지 않았다 |
| 04278 product payload | P1-8~P2-15(guest file, account create/disable, noVNC target, QoS/job reconcile, checkpoint schedule, Hyper-V export/import, switch service action, NIC/DVD add). QoS/disk resize 계약은 04275 functional이 소유 |

P1-8~P2-15 기능은 feature evidence ledger의 candidate가 아니라 ADR-0015 blocker가 아니다.
이 carry-forward는 그 기능들의 actual-VM PASS나 functional 10/10을 주장하지 않는다.

## Nonclaims

- 이 문서는 새 actual-host functional suite 재실행 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 수행하지 않았다.
- public trusted signing / external stable publication을 주장하지 않는다.
