# Functional correctness actual-host validation 2026-09-29 `0.42.83` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-09-29-04283-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.83-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.83-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 재실행하지 않았다. 0.42.77과 0.42.78 승격과 같은 방식으로, 아래를 근거로 `0.42.75` functional PASS를 carry-forward한다. 2026-09-29에 artifact summary SHA-256이 위 값과 같음을 다시 확인했다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.78` (`functional-correctness-actual-host-validation-2026-09-27-04278-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.83` PASS (`full-admin-host-mutation-gate-20260929-04283`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-09-29-04283`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)가 `0.42.75` actual-VM PASS다. ledger는 `bb15f66` 뒤로 바뀌지 않았다 |
| 04283 product payload 변경 | 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.78` build source `0de176f` 뒤 제품 경로 변경은 네 가지다. 각 변경의 actual-VM 근거는 다음과 같다.

| 변경 | commit | actual-VM 근거 |
| --- | --- | --- |
| P1-8 guest file 부모 디렉터리 생성 | `77346bf` | `service-plan-p1-guest-file-actual-vm-2026-09-28-04282` PASS (probe `0.42.82`) |
| P2 Off-VM export/import와 network connect 수정 | `9402774`, `017c008`, `a3dba76` | `service-plan-p2-offvm-*-04281` PASS (probe `0.42.81`) |
| External switch 분류, route DVD guard | `59e94d5`, `3945f88` | 단위 테스트만. 이 호스트에 External switch가 없다 |
| 라쳇 복구 순수 이동 | `5ec4f23`~`cbb1103`, `38f3eec` | 동작 변경 없음 |

위 기능들은 feature evidence ledger의 candidate가 아니므로 ADR-0015 blocker가 아니다. 이 carry-forward는 그 기능들의 current actual-VM PASS나 functional 10/10을 주장하지 않는다.

## Nonclaims

- 이 문서는 새 actual-host functional suite 재실행 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
