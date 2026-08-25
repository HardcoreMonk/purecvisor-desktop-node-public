# Admin-smoke 패키지 2026-05-25 0.42.43

evidence_id: `admin-smoke-package-2026-05-25-04243`
result: `PASS`
scope: `internal-admin-smoke-pcvcli-usage-error-trim`
version: `0.42.43-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260525-04243`
installed_cli_usage_trim_smoke: `docs/ga-ready/evidence/installed-cli-usage-trim-smoke-2026-05-25-04243.md`
msi_sha256: `38be93dd0d944e3657ea6fea2f3e0f922ab4577c09d57183b5be299de90297b1`
payload_aggregate_sha256: `95ba31a501bbf7e3cbb2ba103feb9638e0d01ebdfea922237ddbb15cea0c25f7`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `4e01e6c0308236038f6f6d3dc2fb1569589dde1bf3b09d523f48aa8bf7b4e0da`
cli_sha256: `bb8d7da802cc29b5b2d28340a91d961be444afec53303901cf674ce0c79a38ed`
tui_sha256: `5164ef8eaea7ae9210cd5cffb434cccbcd0076af77ebbce4a56c946ca05e4ade`
provenance_commit: `93131de2bfab5fccfc2761538ead0460d3e7d85d`
build_utc: `2026-05-25T06:04:22.3781319Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PCVCLI command-specific usage error가 전체 `Usage:` block을 함께 출력하던 문제를 `0.42.43-admin-smoke` package로 반영한 기록이다. `--help` / `help`의 명시 help 출력은 유지하고, 인자 누락 같은 일반 command 오류에서는 `PCV_CLI_USAGE|Use: ...` 한 줄만 출력한다.

## 검증

- `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore`: `89/89` PASS
- `git diff --check`: PASS
- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.43-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260525-04243 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- 설치본 update/usage-trim smoke: `artifacts/installed-cli-usage-trim-smoke-20260525-04243/summary.json`, PASS

## 경계

이 package build는 internal admin-smoke evidence다. Full admin host mutation gate와 manual-admin package-pair closure는 이번 package smoke에서 재실행하지 않았고, 현재 operational anchor는 `0.42.41-admin-smoke` / `0.42.40-admin-smoke -> 0.42.41-admin-smoke`를 유지한다. Public trusted signing, public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
