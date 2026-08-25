# Manual-admin Baseline Host Prep 2026-05-27 0.42.50 -> 0.42.53 Blocked

evidence_id: `manual-admin-baseline-host-prep-2026-05-27-04250-04253-blocked`
result: `BLOCKED_BY_CURRENT_HOST_ALREADY_TARGET_VERSION`
scope: `manual-admin-04250-to-04253-baseline-host-prep`
baseline_version: `0.42.50-admin-smoke`
target_version: `0.42.53-admin-smoke`
installed_version: `0.42.53-admin-smoke`
plan_only: `true`
actual_execution: `not-run`
host_mutation_performed: `false`
readiness_summary: `artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness-post-public-boundary/summary.json`
plan_preview: `artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness-post-public-boundary/manual-admin-rebaseline.plan-preview.json`
current_msi_present: `true`
current_msi_sha256: `99c8e4adf8959de3da3d5a9a1157cd1ea2f9580eb16cf4ba1a9738013a376d6b`
target_msi_present: `true`
target_msi_sha256: `39df998c061d9dcecbbc21a966f9ffb495f27502922f2057bd5defc93c9a19ea`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 `
  -ArtifactRoot artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness-post-public-boundary `
  -BaselineVersion 0.42.50-admin-smoke `
  -TargetVersion 0.42.53-admin-smoke `
  -RouteParityArtifactRoot artifacts/routeparity-service-msi-hyperv-batch-profile-20260527-04250 `
  -TargetPackageArtifactRoot artifacts/admin-smoke-package-20260527-04253 `
  -PlanOnly
```

## 결과

- `package_pair_input_status`: `blocked-by-installed-baseline-version-mismatch`.
- 현재 설치본은 `0.42.53-admin-smoke`이므로 baseline `0.42.50-admin-smoke` 조건을 만족하지 않는다.
- 0.42.50 MSI와 0.42.53 MSI artifact는 모두 존재한다.
- 안전 실행 경계는 `current-version-rebaseline-or-dedicated-clean-host`다.

## 판정

현재 host를 강제 downgrade하지 않고 dedicated baseline host 준비를 요구하는 blocked evidence로 닫는다.
04250 -> 04253 manual-admin package-pair closure를 PASS로 승격하려면 0.42.50 설치 상태의 dedicated
host 또는 통제된 downgrade/update/restore campaign이 필요하다.
