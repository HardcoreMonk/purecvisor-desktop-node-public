# Admin-smoke 패키지 2026-05-26 0.42.45

evidence_id: `admin-smoke-package-2026-05-26-04245`
result: `PASS`
scope: `internal-admin-smoke-console-access-card-productization`
version: `0.42.45-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260526-04245`
installed_console_access_smoke: `artifacts/installed-console-access-smoke-20260526-04245/summary.json`
installed_account_login_smoke: `artifacts/installed-account-login-smoke-20260526-04245/summary.json`
target_backed_novnc_streaming_smoke: `artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245/summary.json`
msi_sha256: `376218a0ee394e124f019e0e49a25718077585bac48f09c951da845bd96087bf`
payload_aggregate_sha256: `3c1f9c9ab17144301976b9996d709c611a99122beb1296b457bf6444e2c6787a`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `4b477b776036453741a0594af6167cec07a5b4fd2ea51aff3d66f2b31b54111a`
cli_sha256: `ab5cce549c5b3f327a193569e74f911601101a47850426218412e0da73004f05`
tui_sha256: `8266cce3c696f789587bdc08b635681ce2607466024ef856ad343b0cc079f68c`
provenance_commit: `76c77a86bbb72e415b1968169c16f1638b76fa56`
build_utc: `2026-05-25T15:34:41.2486992Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 console access card와 noVNC handoff projection을 설치본 Web/CLI/TUI에
반영한 `0.42.45-admin-smoke` package 기록이다. `pcvcli vm console`, `pcvcli vm vnc`,
`pcvtui --smoke-once vm`, account login, target-backed noVNC streaming smoke를
package follow-up evidence로 연결한다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.45-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260526-04245 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- MSI update install: `artifacts/installed-msi-update-20260526-04245/summary.json`, `PASS`, installed version `0.42.45-admin-smoke`
- 설치본 console access smoke: `artifacts/installed-console-access-smoke-20260526-04245/summary.json`, `PASS`
- 설치본 account login/browser smoke: `artifacts/installed-account-login-smoke-20260526-04245/summary.json`, `PASS`
- Target-backed noVNC streaming smoke: `artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245/summary.json`, `PASS`

## 경계

이 package build는 internal admin-smoke evidence다. Full admin host mutation과
manual-admin package-pair closure는 각각
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04245-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04244-04245.md`가 current
anchor로 소유한다. Public trusted signing, public stable installer URL, winget submission,
외부 stable publication은 주장하지 않는다.
