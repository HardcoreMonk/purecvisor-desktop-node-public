# Admin-smoke 패키지 2026-05-26 0.42.49

evidence_id: `admin-smoke-package-2026-05-26-04249`
result: `PASS`
scope: `internal-admin-smoke-guest-execution-policy-api-preview-disabled-boundary`
version: `0.42.49-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260526-04249`
msi_sha256: `322bddcb89b05a882ed323429bcfce29f6a856701b801925b53c37423de0a6e2`
payload_aggregate_sha256: `e348a46ad635b61347688750162de100914ad991dd255d10892d319872f19d10`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `2eba16c110ba3dbf5c8aa836feb7859898c4c4eba20b63b58c1b7a6d43ebde65`
cli_sha256: `2d0ad7c87685ae1b18e42e1d1a82afb7c897a44dd7c59a26dad791c945883575`
tui_sha256: `90358357a28664cee79f882d02b052c34729948ccd83115879818bb9251d5c8a`
provenance_commit: `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`
build_utc: `2026-05-26T12:42:31.8838540Z`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 ADR-0009 Guest Execution security boundary의 첫 product payload인
runtime policy block, disabled API route, problem code catalog, credential reference
resolver, redaction engine, audit writer skeleton을 설치본 package로 승격한 기록이다. 실제
guest command execution/provider mutation은 아직 열지 않는다.

## Operational full-gate package 구분

Full admin host mutation gate는 같은 commit에서 route parity artifact 안에서 MSI를 다시
빌드했고, 실제 설치와 current-card smoke는 아래 operational full-gate package를 기준으로
통과했다.

| 항목 | 값 |
| --- | --- |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04249` |
| operational MSI SHA-256 | `465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32` |
| operational payload aggregate SHA-256 | `d49e70c1e291dd28040821fcb659222f4ff524b9c7353994f5e5447ec08610c5` |
| operational build UTC | `2026-05-26T12:43:35.7012867Z` |
| operational signing trust model | `Unspecified` |

Clean package와 operational full-gate package는 같은 provenance commit
`4e08d8020f74d4f452e6e0ff3dba0d9602073a43`에서 생성됐지만 MSI container hash는 빌드 시각과
Wix output 차이로 다르다. Current installed evidence는 operational full-gate MSI hash를
release anchor로 사용한다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.49-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260526-04249 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`: exit `0`
- Full admin host mutation gate: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04249/summary.json`, `PASS`
- 설치본 Web/TUI/CLI current-card smoke:
  `artifacts/installed-operator-surface-current-card-20260526-04249/summary.json`, `PASS`
- Guest execution disabled boundary: runtime policy `guest_execution.enabled=false`,
  `PCV_GUEST_EXEC_DISABLED`, secret/credential-ref echo 없음.
- Manual-admin 04248->04249 readiness:
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04248-04249-readiness-blocked.md`,
  `blocked-by-installed-baseline-version-mismatch`

## 경계

이 package build는 internal admin-smoke evidence다. `0.42.48-admin-smoke ->
0.42.49-admin-smoke` manual-admin package-pair는 현재 host가 이미 0.42.49로 올라간 뒤
readiness를 실행해 baseline mismatch로 blocked 처리했다. 최신 closed manual-admin
package-pair는 계속 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
