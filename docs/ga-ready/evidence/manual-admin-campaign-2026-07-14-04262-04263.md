# Manual-admin campaign 2026-07-14 0.42.62 -> 0.42.63 BLOCKED

evidence_id: `manual-admin-campaign-2026-07-14-04262-04263-blocked`
result: `BLOCKED`
scope: `manual-admin-package-pair-readiness`
baseline_version: `0.42.62-admin-smoke`
target_version: `0.42.63-admin-smoke`
historical_current_closed_pair_at_campaign: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
current_closed_pair_compatibility_alias_semantics: `historical-at-campaign-time-not-current-operational-authority`
current_closed_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
blocker: `blocked-by-installed-baseline-version-mismatch`
readiness_exit_code: `0`
readiness_artifact_id: `manual-admin-campaign-20260714-04262-04263/readiness`
readiness_summary_locator: `artifacts/manual-admin-campaign-20260714-04262-04263/readiness/summary.json`
readiness_summary_public_tree_status: `private-or-local-artifact-not-tracked-in-public-source`
installed_version: `0.42.63-admin-smoke`
host_mutation_performed_by_readiness: `false`
next_action: `run-on-dedicated-0.42.62-baseline-host-with-approved-burn-msix-runners`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Readiness 판정

`New-PcvManualAdminRebaselineReadiness.ps1 -PlanOnly`은 exit `0`, `ok=true`,
`actual_execution=not-run`을 반환했다. 그러나 campaign의 package-pair input status는
`blocked-by-installed-baseline-version-mismatch`다. 요청 baseline은
`0.42.62-admin-smoke`인데 현재 설치 manifest의 실제 version은 이미
`0.42.63-admin-smoke`이므로 이 host에서 baseline-to-target lifecycle을 시작할 수 없다.

Readiness는 host mutation을 수행하지 않았다. Host downshift, update/rollback, clean-host,
Burn/MSIX lifecycle runner 실행도 하지 않았으며 runner JSON이나 closed descriptor를 합성하지
않았다.

## Historical current anchor at execution time

이 campaign 실행 당시 `0.42.63-admin-smoke` package/fullgate/CLI-Web installed current-card가
operational anchor였다. 이 follow-up은 package-pair closure가 아니므로 당시 closed
manual-admin package-pair는 `0.42.58-admin-smoke -> 0.42.59-admin-smoke` /
`manual-admin-campaign-descriptor-20260529-04258-04259-closed`였다. 현재 operational authority는
`docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의 생성 블록이 소유한다.

## 다음 action

`run-on-dedicated-0.42.62-baseline-host-with-approved-burn-msix-runners`: 승인된 dedicated host를
`0.42.62-admin-smoke` baseline으로 준비한 뒤 실제 package pair로 clean-host, update/rollback,
Burn, MSIX 및 descriptor closure를 별도 campaign에서 실행한다.
