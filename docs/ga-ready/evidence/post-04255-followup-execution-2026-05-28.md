# 0.42.55 후속 1-2-3-4-5-6 실행 증거

evidence_id: `post-04255-followup-execution-2026-05-28`
followup_sequence: `1-2-3-4-5-6`
scope: `post-04255-manual-admin-rebaseline-and-operator-surface-rerun`
status: `pass-with-package-pair-not-opened`
created_at: `2026-05-28T20:20:00+09:00`
installed_version: `0.42.55-admin-smoke`
manual_admin_rebaseline_artifact_root: `artifacts/manual-admin-campaign-20260528-04255-next/rebaseline-readiness`
account_novnc_followup_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup.md`
package_pair_decision: `not-opened-no-next-product-payload-target`
next_package_pair_candidate: `0.42.55-admin-smoke -> next-admin-smoke-required`
guest_execution_contract_status: `carried-forward-0.42.55-provider-running-cancel`
hyperv_qos_mutation_status: `carried-forward-0.42.48-web-tui-direct-control`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 요약

사용자 승인 `1-2-3-4-5-6`에 따라 0.42.55 이후 후속 항목을 점검했다. 이번 실행은
새 제품 코드 payload를 만들지 않았으므로 `0.42.56-admin-smoke` package build 또는
manual-admin package-pair를 열지 않는다. 대신 현재 설치본 `0.42.55-admin-smoke` 기준
rebaseline readiness와 account/noVNC installed smoke를 재실행하고, 다음 package-pair
후보를 `0.42.55-admin-smoke -> next-admin-smoke-required`로 재정의했다.

## 1. Manual-admin rebaseline

`New-PcvManualAdminRebaselineReadiness.ps1`는 의도적으로 `-PlanOnly`만 허용한다. 따라서
현재 host를 downgrade하거나 dedicated clean-host를 임의 생성하지 않고, 현재 설치본 기준
readiness descriptor를 남겼다.

| 항목 | 값 |
| --- | --- |
| artifact root | `artifacts/manual-admin-campaign-20260528-04255-next/rebaseline-readiness` |
| summary | `artifacts/manual-admin-campaign-20260528-04255-next/rebaseline-readiness/summary.json` |
| installed version | `0.42.55-admin-smoke` |
| requested baseline | `0.42.55-admin-smoke` |
| installed version matches requested | `true` |
| package pair mode | `false` |
| package pair input status | `not-requested` |
| target version | `next-admin-smoke-required` |
| host mutation performed | `false` |

`credential-manager`와 `event-log` bucket은 `ready-with-current-version-override`다. Burn/MSIX/MSI,
update/rollback, clean-host bucket은 다음 target package가 생긴 뒤 current baseline/target
package-pair로 열어야 한다.

## 2. 다음 package-pair 후보

이전 04250->04254 후보는 현재 host가 이미 0.42.55까지 올라간 뒤에는 historical blocked
candidate로만 보존한다. 새 후보는 아래와 같다.

```text
manual_admin_next_package_pair_candidate: 0.42.55-admin-smoke -> next-admin-smoke-required
manual_admin_next_package_pair_status: not-opened-no-next-product-payload-target
manual_admin_next_package_pair_safe_boundary: current-version-rebaseline-or-dedicated-clean-host
```

다음 target package가 확정되기 전까지 package-pair PASS를 주장하지 않는다.

## 3-5. Guest Execution, Hyper-V QoS, Web/TUI direct control

이번 실행에서 새 code payload를 추가하지 않았다. 아래 구현은 이미 닫힌 evidence를
current contract로 carry-forward한다.

| 항목 | 기준 |
| --- | --- |
| Guest Execution / Guest Channel | `docs/adr/0009-guest-execution-security-boundary.md`; 0.42.55 actual credentialed guest-exec PASS |
| Running guest execution cancel | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md` |
| Hyper-V QoS mutation | `docs/adr/0008-hyperv-qos-mutation-policy.md`; 0.42.47 installed actual VM smoke, 0.42.48 Web/TUI direct control closure |
| Web/TUI direct control | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md` |

## 6. Account/noVNC installed rerun

`0.42.55-admin-smoke` 설치본 기준 account login/browser QA와 target-backed noVNC streaming을
재실행했다. 상세 증거는
`docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup.md`가
소유한다.

## 경계

이 evidence는 internal admin-smoke 운영 증거다. Public trusted signing, winget public
submission, public stable installer URL, 외부 stable publication은 주장하지 않는다.
