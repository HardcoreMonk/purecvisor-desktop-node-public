# Manual-admin campaign readiness 2026-05-26 0.42.48 -> 0.42.49 blocked

evidence_id: `manual-admin-campaign-2026-05-26-04248-04249-readiness-blocked`
result: `BLOCKED_READINESS`
scope: `manual-admin-package-pair-readiness-04248-to-04249`
baseline_version: `0.42.48-admin-smoke`
target_version: `0.42.49-admin-smoke`
artifact_root: `artifacts/manual-admin-campaign-20260526-04248-04249/manual-admin-rebaseline-readiness`
summary: `artifacts/manual-admin-campaign-20260526-04248-04249/manual-admin-rebaseline-readiness/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04249-hostmutation.md`
current_installed_version: `0.42.49-admin-smoke`
package_pair_input_status: `blocked-by-installed-baseline-version-mismatch`
requested_version_status: `blocked-by-installed-version-mismatch`
safe_execution_boundary: `current-version-rebaseline-or-dedicated-clean-host`
target_msi_sha256: `322bddcb89b05a882ed323429bcfce29f6a856701b801925b53c37423de0a6e2`
current_msi_present: `false`
target_msi_present: `true`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

`0.42.48-admin-smoke -> 0.42.49-admin-smoke` manual-admin package-pair readiness는 현재 host에서
닫지 않았다. 이유는 full admin host mutation gate가 이미 `0.42.49-admin-smoke`를 설치한 뒤
readiness를 실행했기 때문에, package-pair baseline으로 요구한 `0.42.48-admin-smoke` 설치 상태를
만족하지 못했기 때문이다.

## Readiness 결과

| 항목 | 값 |
| --- | --- |
| plan_only | `true` |
| actual_execution | `not-run` |
| installed_version | `0.42.49-admin-smoke` |
| installed_version_matches_requested | `false` |
| package_pair_input_status | `blocked-by-installed-baseline-version-mismatch` |
| current_msi_path | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04249/PureCVisorDesktopNode-0.42.48-admin-smoke-windows-x64.msi` |
| current_msi_present | `false` |
| target_msi_path | `artifacts/admin-smoke-package-20260526-04249/PureCVisorDesktopNode-0.42.49-admin-smoke-windows-x64.msi` |
| target_msi_sha256 | `322bddcb89b05a882ed323429bcfce29f6a856701b801925b53c37423de0a6e2` |

## 판정

이 blocker는 0.42.49 package/fullgate/current-card 실패가 아니다. 설치본 current-card와 full
admin host mutation은 0.42.49 기준 PASS다. 다만 manual-admin package-pair closure는 "현재
설치본이 baseline version이어야 한다"는 안전 조건을 만족해야 하므로, dedicated clean host에
0.42.48 baseline을 설치하거나 현재 host를 0.42.48로 되돌린 뒤 다시 실행해야 한다.

최신 closed manual-admin package-pair evidence는 계속
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md` /
`manual-admin-campaign-descriptor-20260526-04247-04248-closed`다.
