# Functional correctness actual-host validation 2026-10-02 `0.42.86` (carry-forward)

evidence_id: `functional-correctness-actual-host-validation-2026-10-02-04286-carryforward`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.86-admin-smoke`
carry_forward_from: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-27-04275.md`
carry_forward_chain: `0.42.75 -> 0.42.77 (functional-correctness-actual-host-validation-2026-09-20-04277-carryforward) -> 0.42.78 (functional-correctness-actual-host-validation-2026-09-27-04278-carryforward) -> 0.42.83 (functional-correctness-actual-host-validation-2026-09-29-04283-carryforward) -> 0.42.84 (functional-correctness-actual-host-validation-2026-09-30-04284-carryforward) -> 0.42.86`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
host_mutation_performed: `false`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.86-admin-smoke` Lane 3는 QoS/disk functional suite를 이 날짜에 다시 돌리지 않았다. 0.42.77, 0.42.78, 0.42.83, 0.42.84 승격과 같은 방식으로, 아래 근거를 들어 `0.42.75` functional PASS를 이어 받는다. feature evidence ledger `config/desktop-node-feature-evidence-ledger.json`의 마지막 변경은 `bb15f66`이고, 그 뒤 내용은 바뀌지 않았다.

| 근거 | 값 |
| --- | --- |
| predecessor functional | `0.42.75` PASS (`functional-correctness-actual-host-validation-2026-08-27-04275`) |
| 이전 carry-forward | `0.42.84` (`functional-correctness-actual-host-validation-2026-09-30-04284-carryforward`) |
| fullgate Hyper-V installed route smoke | `0.42.86` PASS (`full-admin-host-mutation-gate-20261002-04286`) |
| installed current-card | CLI/Web PASS (`installed-operator-surface-current-card-2026-10-02-04286`) |
| feature evidence ledger | candidate `4`개(`pcv.checkpoint.restore`, `pcv.vm.managed-import`, `pcv.vm.media-attach`, `pcv.vm.saved-lifecycle`)는 `0.42.75` actual-VM PASS다 |
| 04286 product payload 변경 | DVD media eject와 빈 drive attach. 아래 표 참고. QoS/disk resize 계약은 04275 functional이 소유한다 |

`0.42.84` fullgate provenance `ee90e0e` 뒤 제품 경로 변경은 media fix `fb95de1`이다. 이 수정은 clean package source `1c488b6`에 들어 있고, fullgate provenance `b807803`에서도 도달한다. `0.42.85-admin-smoke` package에는 `fb95de1`이 없다.

| 변경 | commit | actual-VM 근거 |
| --- | --- | --- |
| `vm.eject`는 media가 있으면 `RemoveResourceSettings`, 없으면 WMI를 호출하지 않는다. 빈 drive의 `vm.attach`는 media SASD를 `AddResourceSettings`로 추가한다 | `fb95de1` | `lane2-vm-media-eject-attach-2026-10-02-04286` PASS. probe VM `pcv-wo-media-1002`는 삭제했다. 그 프로브의 Host/CLI hash는 clean package와 같고, 이후 fullgate operational hash와는 다르다 |

이 기능은 feature evidence ledger의 candidate `pcv.vm.media-attach`를 다시 열지 않는다. ADR-0015 blocker가 아니다. 이 carry-forward는 QoS/disk functional 10/10을 다시 측정하지 않았다.

## Nonclaims

- 이 문서는 actual-host functional suite를 다시 돌린 evidence가 아니다.
- 이 문서를 쓰는 동안 호스트 설치본을 바꾸는 mutation을 하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
