# Manual-admin package-pair closure 2026-05-28 0.42.50 to 0.42.54 blocked after fullgate

evidence_id: `manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-fullgate`
result: `BLOCKED_BY_MISSING_DEDICATED_BASELINE_HOST`
scope: `manual-admin-04250-to-04254-package-pair-closure-after-04254-fullgate`
baseline_version: `0.42.50-admin-smoke`
target_version: `0.42.54-admin-smoke`
readiness_summary: `artifacts/manual-admin-campaign-20260528-04250-04254/manual-admin-rebaseline-readiness-after-fullgate/summary.json`
plan_preview: `artifacts/manual-admin-campaign-20260528-04250-04254/manual-admin-rebaseline-readiness-after-fullgate/manual-admin-rebaseline.plan-preview.json`
baseline_package_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260527-04250`
target_package_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260528-04254`
baseline_msi_sha256: `99c8e4adf8959de3da3d5a9a1157cd1ea2f9580eb16cf4ba1a9738013a376d6b`
target_msi_sha256: `937ac686aa782a69dc41d06d8694a020cf4a78b45cf7a6674e85593cce3c4cb1`
installed_version: `0.42.54-admin-smoke`
package_pair_input_status: `blocked-by-installed-baseline-version-mismatch`
safe_execution_boundary: `current-version-rebaseline-or-dedicated-clean-host`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 `
  -ArtifactRoot artifacts/manual-admin-campaign-20260528-04250-04254/manual-admin-rebaseline-readiness-after-fullgate `
  -BaselineVersion 0.42.50-admin-smoke `
  -TargetVersion 0.42.54-admin-smoke `
  -RouteParityArtifactRoot artifacts/routeparity-service-msi-hyperv-batch-profile-20260527-04250 `
  -TargetPackageArtifactRoot artifacts/routeparity-service-msi-hyperv-batch-profile-20260528-04254 `
  -PlanOnly
```

## 판정

0.42.50 baseline artifact와 0.42.54 fullgate target artifact는 모두 존재한다. 그러나 현재 host는
이미 `0.42.54-admin-smoke` 설치본이므로 baseline `0.42.50-admin-smoke` 조건을 만족하지
않는다. 따라서 active host를 downgrade하지 않고 package-pair closure를 PASS로 승격하지
않는다. 다음 실행은 dedicated 0.42.50 baseline host 또는 통제된 rebaseline campaign에서만
열 수 있다.
