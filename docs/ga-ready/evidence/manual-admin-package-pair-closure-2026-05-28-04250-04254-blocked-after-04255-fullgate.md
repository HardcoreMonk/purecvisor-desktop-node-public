# Manual-admin package-pair closure 2026-05-28 0.42.50 to 0.42.54 blocked after 0.42.55 fullgate

evidence_id: `manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-04255-fullgate`
result: `BLOCKED_BY_INSTALLED_BASELINE_VERSION_MISMATCH`
scope: `manual-admin-rebaseline-readiness-plan-only`
baseline_version: `0.42.50-admin-smoke`
target_version: `0.42.54-admin-smoke`
installed_version: `0.42.55-admin-smoke`
artifact_root: `artifacts/manual-admin-campaign-20260528-04250-04254/manual-admin-rebaseline-readiness-after-04255-fullgate`
summary: `artifacts/manual-admin-campaign-20260528-04250-04254/manual-admin-rebaseline-readiness-after-04255-fullgate/summary.json`
plan_only: `true`
actual_execution: `not-run`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.50-admin-smoke -> 0.42.54-admin-smoke` package-pair baseline/target artifact는 모두 존재한다.
하지만 현재 host installed manifest가 이미 `0.42.55-admin-smoke`로 승격되어 baseline
`0.42.50-admin-smoke` 설치 조건을 만족하지 못한다. 따라서 downgrade 없이 campaign을 실행하지
않고 `blocked-by-installed-baseline-version-mismatch`로 닫는다.

| 항목 | 결과 |
| --- | --- |
| Current MSI present | `true`, `99c8e4adf8959de3da3d5a9a1157cd1ea2f9580eb16cf4ba1a9738013a376d6b` |
| Target MSI present | `true`, `937ac686aa782a69dc41d06d8694a020cf4a78b45cf7a6674e85593cce3c4cb1` |
| Installed version matches baseline | `false`, installed `0.42.55-admin-smoke` |
| Safe execution boundary | `current-version-rebaseline-or-dedicated-clean-host` |

## 후속

이 package-pair를 실제로 닫으려면 dedicated clean host에 `0.42.50-admin-smoke` baseline을 설치한
뒤 `0.42.54-admin-smoke` target으로 update/rollback, clean-host, Burn/MSIX, installed runtime ops
summary evidence를 새로 수집해야 한다. 현재 0.42.55 host에서 직접 실행하지 않는다.
