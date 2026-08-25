# Current evidence generation code-level evidence — 2026-07-16

evidence_id: `current-evidence-generation-code-level-2026-07-16`
result: `CODE_LEVEL_PASS`
canonical_contract: `pcv-current-evidence-v1`
canonical_record: `docs/ga-ready/current-evidence.json`
canonical_schema: `docs/ga-ready/current-evidence.schema.json`
active_operator_surfaces: `web,cli`
tui_present: `false`

## 판정

`docs/ga-ready/current-evidence.json` 한 곳이 현재 operational pointer와 hash를 소유한다.
`Update-PcvCurrentEvidenceDocs.ps1`가 아래 6개 문서의 bounded marker block을 결정적으로
생성하며 `-Check`는 stale 상태에서 쓰기 없이 실패한다.

- `AGENTS.md`
- `docs/ga-ready/EVIDENCE_INDEX.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `packaging/windows-desktop-node/README.md`

각 문서에는 begin/end marker가 정확히 하나씩 있다. 2026-07-13 TUI predecessor 및 그 이전
historical section은 marker 밖에 유지했다. `CURRENT_EVIDENCE_LEDGER.md`의 기존 key/table은
schema-specific 소비자 호환을 위해 read-only legacy 영역으로 보존하고, 최상단 generated
block을 현재 기준으로 사용한다.

## 자동 검증

- generator update 1회 후 `-Check` 2회 연속 PASS.
- 두 `-Check` 전후 6개 target SHA-256이 동일해 write가 없음을 확인.
- current evidence generation, S/M/L selector/runner focused Pester: 17/17 PASS.
- Admin smoke current/historical evidence 및 canonical ownership 회귀: 88/88 PASS.
- Development Gates workflow 계약: 1/1 PASS.
- 위 focused 계약의 최종 합산 재검증: 106/106 PASS.
- Full lane의 비변경 7개 suite가 모두 PASS했고 최근 잔여 `dotnet` process는 0개다.

| Full suite | 결과 | 시간 |
| --- | --- | ---: |
| `dotnet` | PASS | 29.373초 |
| `web-npm` | PASS | 5.581초 |
| `packaging-pester` | PASS | 122.217초 |
| `installer-pester` | PASS | 14.413초 |
| `web-pester` | PASS | 5.230초 |
| `git-diff-check` | PASS | 0.093초 |
| `current-evidence-check` | PASS | 0.766초 |

합계 suite duration은 177.673초다. CI의 Windows `packaging-pester` job은 checkout 직후
generator `-Check`를 실제 실행하고, Full PlanOnly 계약은 7개 suite를 계획한다.

## S/M/L 연결

`Resolve-PcvDevelopmentChangeTier`는 installer lifecycle, host mutation, security policy,
current evidence anchor, public boundary, signing/publication을 `L`로 강제한다. API/CLI/Web
contract와 일반 packaging 변경은 최소 `M`이다. Unknown 경로는 근거 없는 `L` 주장을 만들지
않고 verification lane만 `Full`로 올린다. 상세 정책은
`docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`가 소유한다.

## 운영 경계

- current operational anchor: `0.42.64-admin-smoke`
- fullgate batch: `full-admin-host-mutation-gate-20260715-04264`
- provenance commit: `a0491e39992093b9ad506619cfacb1675939d6a3`
- `host_mutation_performed=false`
- `package_build_performed=false`
- `installed_product_changed=false`
- `public_trusted_signing=false`
- `external_stable_publication=false`

이 evidence는 generator, 문서 소유권, 변경 분류와 비변경 개발 게이트의 code-level
검증이다. 새 package, 설치본, 실제 VM mutation, trusted public signing 또는 외부 stable
publication evidence가 아니다.
