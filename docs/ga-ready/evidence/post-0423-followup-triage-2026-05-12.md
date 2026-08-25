# Post-0423 후속 개발 Triage - 2026-05-12

```text
evidence_id: post-0423-followup-triage-2026-05-12
scope: follow-up-items-1-2-3-4
source_baseline: origin/main@d6d0caae6e71531774c55b90048f0194fd7f2e14
baseline_full_admin_evidence: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md
baseline_version: 0.42.3-admin-smoke
target_version: 0.42.4-admin-smoke
manual_admin_0423_rerun_decision: required-next-package-pair
package_pair_rebaseline_plan: 0.42.3-admin-smoke-to-0.42.4-admin-smoke
public_distribution_drift: none-observed
host_mutation_performed: false
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
```

## 판정

`0.42.3-admin-smoke` full admin host mutation gate는 당시 최신 PASS였지만, 최신
MANUAL-ADMIN 1-2-3-4 묶음 evidence는
`manual-admin-campaign-2026-05-11-0420-0421`의 `0.42.0-admin-smoke` to
`0.42.1-admin-smoke` 기준이다. 따라서 0.42.3 이후 operator access, noVNC/TUI,
internal service hardening, Lifecycle/Packaging 묶음은 새 package pair로 다시
닫아야 한다.

이번 문서는 실행 전 triage다. 새 host mutation은 수행하지 않았다. 다음 실행은
`0.42.3-admin-smoke`를 baseline으로 두고 `0.42.4-admin-smoke` target package를
같은 source lineage에서 만든 뒤 MANUAL-ADMIN으로 수행한다.

사후 업데이트: 이 triage는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`에서
실행 evidence로 이어졌다. Full admin gate, Operator Access, Internal Service
Hardening, installed update/rollback은 PASS였고 dedicated clean-host는
`0.42.3-admin-smoke` baseline MSI custom action sequence blocker로 보류됐다.

사후 업데이트: 2026-05-12 이후 최신 full admin host mutation claim은
`0.42.7-admin-smoke` / `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md`가
소유한다. 이 post-0423 triage는 historical planning record로 보존한다.
따라서 이 triage의 `required-next-package-pair` 결론은 실행 완료가 아니라
`partial-pass-clean-host-blocked` 상태로 전환됐다.

## 1. 0.42.3 기준 Manual-admin Rerun 여부

결론: rerun 필요.

이유는 네 가지다.

- `0.42.3-admin-smoke`는 Hyper-V WMI helper 추출과 Ops summary data builder 분리
  이후의 최신 full host gate다.
- 최신 MANUAL-ADMIN 1-2-3-4 evidence는 `0.42.0 -> 0.42.1` package pair이며,
  0.42.3 source boundary를 포함하지 않는다.
- installed account login, target-backed noVNC streaming, TUI smoke, TLS,
  Credential Manager, Event Log, service token rotation/revoke는 service path,
  token source, listener, provider state를 바꾸므로 `MANUAL-ADMIN`을 유지해야 한다.
- Lifecycle/Packaging은 baseline/target package pair가 필요하다. target package가
  아직 `0.42.4-admin-smoke`로 확정되지 않았으므로 즉시 실행 대신 package-pair
  rebaseline 계획을 먼저 고정한다.

## 2. ADR-0006 Package-pair Rebaseline 계획

다음 ADR-0006 internal private distribution rebaseline은 아래 pair를 사용한다.

| 항목 | 값 |
| --- | --- |
| baseline | `0.42.3-admin-smoke` |
| target | `0.42.4-admin-smoke` |
| baseline evidence | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md` |
| baseline MSI SHA-256 | `31ea6df1ff11cbaa9a9681b083cb5d1f61bc87ecd49db52c4e60e7a141cb229d` |
| source boundary | `d6d0caae6e71531774c55b90048f0194fd7f2e14` 이후 follow-up branch |
| target package root | `artifacts/lifecycle-packaging-rebaseline-<stamp>-0423-0424` |
| required execution | installed update/rollback, internal clean-host install/update/rollback, Burn lifecycle, MSIX lifecycle, MSI/update package apply |
| execution class | `MANUAL-ADMIN` |

실행 순서:

1. `0.42.4-admin-smoke` MSI, update ZIP, publication descriptor, provenance를 생성한다.
2. baseline `0.42.3-admin-smoke` 설치 상태와 Web/API auth boundary를 snapshot한다.
3. installed update/rollback으로 `0.42.3 -> 0.42.4 -> 0.42.3`을 검증한다.
4. dedicated clean-host에서 install/update/rollback을 검증한다.
5. Burn/MSIX/MSI-update package smoke를 같은 package pair로 실행한다.
6. 결과를 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`
   또는 stamp가 포함된 동등한 evidence에 기록한다.

## 3. Public Distribution Drift 점검

ADR-0005 P2 재정리와 0.42.3 descriptor를 기준으로 drift는 발견되지 않았다.

| Public gate | 현재 상태 | 판단 |
| --- | --- | --- |
| Public trusted signing/timestamp | `out-of-scope` / `not-claimed` | 0.42.3은 `AllowUnsignedDev` admin-smoke다. |
| External stable publication/catalog upload | `out-of-scope` / `not-claimed` | publication descriptor는 internal artifact descriptor다. |
| Winget submission | `not-submitted` | offline validation history만 보존한다. |
| Public signed clean-host install/update/rollback | `out-of-scope` | ADR-0006 internal clean-host smoke와 구분한다. |

따라서 public release claim은 변경하지 않는다. 새 manual-admin rebaseline도 internal
private network evidence로만 해석한다.

## 4. 다음 Slice 선정

merged main 기준 다음 implementation slice는 세 갈래로 쪼갠다.

| 순서 | Slice | 산출물 |
| --- | --- | --- |
| 1 | Packaging / Manual-admin Campaign Orchestrator | `0.42.3 -> 0.42.4` package pair build, update ZIP, publication descriptor, unified manual-admin runner descriptor |
| 2 | Host Ops Evidence Runner Alignment | Credential Manager, Event Log, TLS, service token runner가 0.42.3 baseline service path와 token source를 preflight에서 검증하도록 문서/테스트 보강 |
| 3 | Runtime/Core Operator Contract | Ops summary builder와 diagnostics/auth boundary가 installed campaign summary에 필요한 fields를 안정적으로 제공하는지 code-level/test evidence 추가 |

상세 implementation plan은
`docs/superpowers/plans/2026-05-12-purecvisor-desktop-node-post-0423-followup-slices.md`
가 소유한다.

## 다음 후속 작업 목록

1. `0.42.4-admin-smoke` package pair input 생성.
2. 0.42.3 baseline installed state snapshot runner 작성 또는 기존 runner parameter 고정.
3. MANUAL-ADMIN 1-2-3-4 통합 실행.
4. 0423-0424 evidence와 ADR-0006 matrix 최신화.
