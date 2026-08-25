# Manual-admin campaign readiness blocked 2026-05-27 0.42.49 -> 0.42.50

evidence_id: `manual-admin-campaign-2026-05-27-04249-04250-readiness-blocked`
result: `BLOCKED_READINESS`
scope: `manual-admin-package-pair-readiness`
baseline_version: `0.42.49-admin-smoke`
target_version: `0.42.50-admin-smoke`
artifact_root: `artifacts/manual-admin-campaign-20260527-04249-04250/manual-admin-rebaseline-readiness`
summary: `artifacts/manual-admin-campaign-20260527-04249-04250/manual-admin-rebaseline-readiness/summary.json`
target_package_artifact_root: `artifacts/admin-smoke-package-20260527-04250`
target_msi_sha256: `782f4417a5ad9ab0d1a4875bcf94c6473d0163340cd316d3cd715257c302072a`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 차단 사유

Readiness는 `blocked-by-installed-baseline-version-mismatch`로 닫혔다. 현재 host는 이미
`0.42.50-admin-smoke` 설치 상태이고, 요청한 package pair baseline은
`0.42.49-admin-smoke`다. 따라서 update/rollback, clean-host, Burn/MSIX package-pair
campaign은 dedicated clean host 또는 0.42.49 rebaseline host에서 다시 시작해야 한다.

또한 readiness는 current baseline package artifact가 route parity artifact root에 없음을
감지했다. 이 상태에서 historical runner default를 강제로 실행하면 혼합 버전 package-pair
evidence가 되므로 실행하지 않는다.

## 재개 조건

1. Dedicated host에 `0.42.49-admin-smoke` baseline을 설치한다.
2. `0.42.49-admin-smoke` current package artifact와 `0.42.50-admin-smoke` target package
   artifact를 같은 manual-admin descriptor에 연결한다.
3. Windows Update clean-host, update/rollback, Burn, MSIX, installed runtime ops summary를
   descriptor 기준으로 재실행한다.
