# Admin-smoke 패키지 2026-05-25 0.42.42

evidence_id: `admin-smoke-package-2026-05-25-04242`
result: `PASS`
scope: `internal-admin-smoke-pcvcli-top-level-snapshot-removal`
version: `0.42.42-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260525-04242`
installed_operator_surface_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-25-04242.md`
msi_sha256: `d92e4c8bc8ee47da4a4c3b64d381725b3a1971b41ee41c9c24ba0a5f65a73582`
payload_aggregate_sha256: `ad5ca2730ea932f08d72541b33b04cfb611ed6ca055f459b8988b48b74737c88`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `6f9fa101f5e26dbadaa72640294e283d7d29cebdbef153af46b6a01b914c93a7`
cli_sha256: `cdcb004d9ca9d0b6890a7a66a60f5b331258fef9d94b8155854bca5de2f2bd6c`
tui_sha256: `ad38ce09e48a8fb6a878a81edb1e121300945292674815ed2532087c741a886c`
provenance_commit: `37632159aaf0c9445c9b712f11f1dfee1a6f9c4f`
build_utc: `2026-05-25T05:29:08.1890499Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PCVCLI 최상위 `snapshot list|create|rollback|delete` command group 제거를 설치 가능한 `0.42.42-admin-smoke` package로 반영한 기록이다. `pcvcli vm checkpoint ...`와 `pcvcli vm snapshot ...` alias는 유지한다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.42-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260525-04242 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore`: `88/88` PASS
- 설치본 update/current-card smoke: `artifacts/installed-operator-surface-current-card-20260525-04242/summary.json`, PASS

## 경계

이 package build는 internal admin-smoke evidence다. Full admin host mutation gate와 manual-admin package-pair closure는 이번 package smoke에서 재실행하지 않았고, 현재 operational anchor는 `0.42.41-admin-smoke` / `0.42.40-admin-smoke -> 0.42.41-admin-smoke`를 유지한다. Public trusted signing, public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
