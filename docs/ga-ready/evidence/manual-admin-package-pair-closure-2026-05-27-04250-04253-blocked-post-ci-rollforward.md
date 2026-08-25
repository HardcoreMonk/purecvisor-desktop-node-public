# Manual-admin Package-pair Closure 2026-05-27 0.42.50 -> 0.42.53 Blocked Post CI Roll-forward

evidence_id: `manual-admin-package-pair-closure-2026-05-27-04250-04253-blocked-post-ci-rollforward`
result: `BLOCKED_BY_MISSING_DEDICATED_BASELINE_HOST`
scope: `manual-admin-04250-to-04253-package-pair-closure-attempt`
baseline_version: `0.42.50-admin-smoke`
target_version: `0.42.53-admin-smoke`
installed_version: `0.42.53-admin-smoke`
plan_only: `true`
actual_execution: `not-run`
host_mutation_performed: `false`
readiness_summary: `artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness-post-ci-26518952796/summary.json`
plan_preview: `artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness-post-ci-26518952796/manual-admin-rebaseline.plan-preview.json`
package_pair_input_status: `blocked-by-installed-baseline-version-mismatch`
safe_execution_boundary: `current-version-rebaseline-or-dedicated-clean-host`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 `
  -ArtifactRoot artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness-post-ci-26518952796 `
  -BaselineVersion 0.42.50-admin-smoke `
  -TargetVersion 0.42.53-admin-smoke `
  -RouteParityArtifactRoot artifacts/routeparity-service-msi-hyperv-batch-profile-20260527-04250 `
  -TargetPackageArtifactRoot artifacts/admin-smoke-package-20260527-04253 `
  -PlanOnly
```

## 결과

- 현재 설치본은 이미 `0.42.53-admin-smoke`다.
- 요청한 package pair baseline은 `0.42.50-admin-smoke`이므로 현재 host는 baseline 조건을 만족하지 않는다.
- readiness summary는 `package_pair_input_status=blocked-by-installed-baseline-version-mismatch`를 반환했다.
- 0.42.50 current MSI와 0.42.53 target MSI artifact는 모두 존재한다.

## 판정

0.42.50 -> 0.42.53 manual-admin package-pair closure는 현재 host에서 PASS로 승격하지 않는다.
active host를 임의 downgrade하지 않고 dedicated 0.42.50 baseline host 또는 통제된 rebaseline campaign을
요구하는 blocked evidence로 유지한다.
