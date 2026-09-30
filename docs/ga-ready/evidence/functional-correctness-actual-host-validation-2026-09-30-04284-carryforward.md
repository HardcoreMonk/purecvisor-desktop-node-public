# Functional correctness actual-host validation 2026-09-30 `0.42.84` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-09-30-04284-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.84-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.84-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77, 0.42.78, 0.42.83 승격과 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. 2026-09-30에 artifact summary SHA-256이 위 값과 같은 것을 다시 확인했다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.83` (`functional-correctness-actual-host-validation-2026-09-29-04283-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.84` PASS (`full-admin-host-mutation-gate-20260930-04284`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-09-30-04284`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다. ledger는 `bb15f66` 뒤로 바뀌지 않았다 |
| 04284 product payload 변경 | 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.83` build source `6846248` 뒤 제품 경로 변경은 개발 완료 campaign(`development-completion-20260930`, PR #22) 하나다.

| 변경 | commit | actual-VM 근거 |
| --- | --- | --- |
| 중단 job reconcile 대상 확대(전원, 일시정지·저장, checkpoint 삭제, 자원, template lock, switch 연결, manage)와 비대상 분류 | `c48fac4`~`55d053b` | `lane2-development-completion-actual-vm-2026-09-30-04284` PASS. `vm.start` interrupt 뒤 reconcile `succeeded`, 비대상 이유 반환 |
| Web Console binding 16개(pause/resume, rename, telemetry, schedule, export/import, switch 연결, 장치 추가, guest preview) | `a995ba5`~`d6c6a4e` | 같은 probe PASS. route 실행과 설치본 `app.js` binding 표식 10개 |
| surface 완료 계약 테스트 | `5850f28` | 동작 변경 없음 |

위 기능들은 feature evidence ledger의 candidate가 아니므로 ADR-0015 blocker가 아니다. 이 carry-forward는 그 기능들의 functional 10/10을 주장하지 않는다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
