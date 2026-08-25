# Manual-admin campaign readiness blocked 2026-05-27 0.42.50 -> 0.42.53

evidence_id: `manual-admin-campaign-2026-05-27-04250-04253-readiness-blocked`
result: `BLOCKED_READINESS`
scope: `manual-admin-package-pair-readiness`
baseline_version: `0.42.50-admin-smoke`
target_version: `0.42.53-admin-smoke`
artifact_root: `artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness`
summary: `artifacts/manual-admin-campaign-20260527-04250-04253/manual-admin-rebaseline-readiness/summary.json`
target_package_artifact_root: `artifacts/admin-smoke-package-20260527-04253`
target_msi_sha256: `39df998c061d9dcecbbc21a966f9ffb495f27502922f2057bd5defc93c9a19ea`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 차단 사유

Readiness는 `blocked-by-installed-baseline-version-mismatch`로 닫혔다. 현재 host는 이미
`0.42.53-admin-smoke` 설치 상태이고, 요청한 package pair baseline은
`0.42.50-admin-smoke`다. 따라서 update/rollback, clean-host, Burn/MSIX package-pair
campaign은 dedicated clean host 또는 0.42.50 rebaseline host에서 다시 시작해야 한다.

이 blocker는 `0.42.53` package build, full admin host mutation gate, installed
Web/TUI/CLI current-card 실패가 아니다. 혼합 버전 package-pair evidence를 만들지 않기 위한
입력 보호 장치다.

## 재개 조건

1. Dedicated host에 `0.42.50-admin-smoke` baseline을 설치한다.
2. `0.42.50-admin-smoke` current package artifact와 `0.42.53-admin-smoke` target package
   artifact를 같은 manual-admin descriptor에 연결한다.
3. Windows Update clean-host, update/rollback, Burn, MSIX, installed runtime ops summary를
   descriptor 기준으로 재실행한다.

