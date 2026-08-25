# Manual-admin campaign 2026-07-13 0.42.59 to 0.42.62

result: `BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED`
blocker: `blocked-by-installed-baseline-version-mismatch-and-no-configured-dedicated-host`
baseline_version: `0.42.59-admin-smoke`
target_version: `0.42.62-admin-smoke`
current_installed_version: `0.42.62-admin-smoke`
plan_only: `true`
plan_only_readiness_result: `blocked-by-installed-baseline-version-mismatch`
readiness_summary_path: `artifacts/manual-admin-campaign-20260713-04259-04262/manual-admin-rebaseline-readiness/summary.json`
campaign_summary_path: `artifacts/manual-admin-campaign-20260713-04259-04262/summary.json`
dedicated_baseline_host_configured: `false`
protected_credential_reference_configured: `false`
host_mutation_performed: `false`
manual_admin_current_closure_changed: `false`
current_closed_manual_admin_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
additional_package_candidate_opened: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

현재 host의 설치본은 이미 `0.42.62-admin-smoke`다. non-mutating readiness를
`0.42.59-admin-smoke -> 0.42.62-admin-smoke` package-pair와 `-PlanOnly`로 실행한 결과
`blocked-by-installed-baseline-version-mismatch`가 반환됐다. 현재 host를 0.42.59로
downgrade하지 않았다.

process environment에 `PCV_MANUAL_ADMIN_BASELINE_HOST`와
`PCV_MANUAL_ADMIN_CREDENTIAL_REF`가 모두 구성되지 않았다. 따라서 installed
update/rollback, clean-host Windows Update, Burn, MSIX, runtime ops, descriptor v2를 실행하지
않았고 이 package-pair를 PASS 또는 current closure로 승격하지 않는다.

## Immutable input 및 기계적 준비 결과

| input | present | SHA-256 / count |
| --- | --- | --- |
| `PureCVisorDesktopNode-0.42.59-admin-smoke-windows-x64.msi` | `true` | `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585` |
| `PureCVisorDesktopNode-0.42.62-admin-smoke-windows-x64.msi` | `true` | `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533` |
| Windows Server 2022 evaluation base VHD | `true` | input presence only |
| `PureCVisorDesktopNode-0.42.59-admin-smoke-baseline-align.zip` | `true` | `05951af066f0080c9c111de7e104fc8a9418812b68ca0fb246a573d89b6e44fb`; payload files `9` |
| `PureCVisorDesktopNode-0.42.62-admin-smoke-update.zip` | `true` | `3a0e91f95d8759a34434b79525a775d60c9f7c6e82263dada79b13d30abed755`; payload files `9` |

입력 MSI와 VHD는 repository root의 기존 ignore artifact를 read-only로 사용했고, 두 ZIP과
PlanOnly 결과는 이 worktree의 campaign artifact에 생성했다. credential 값은 읽거나
command line, artifact, 문서에 기록하지 않았다.

이 blocker evidence는 `0.42.58-admin-smoke -> 0.42.59-admin-smoke` manual-admin closure를
대체하지 않으며 `0.42.63-admin-smoke` package candidate를 열지 않는다. public trusted
signing 또는 외부 stable publication evidence가 아니다.
