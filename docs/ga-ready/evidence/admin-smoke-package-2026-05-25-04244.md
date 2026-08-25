# Admin-smoke 패키지 2026-05-25 0.42.44

evidence_id: `admin-smoke-package-2026-05-25-04244`
result: `PASS`
scope: `internal-admin-smoke-pcvcli-readonly-surface-rendering`
version: `0.42.44-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260525-04244`
installed_cli_readonly_surface_smoke: `docs/ga-ready/evidence/installed-cli-readonly-surface-smoke-2026-05-25-04244.md`
msi_sha256: `eb9b6232a7c61431e2289850eecaba1c9a1d92bc93b88ce8eb4bd6f2ed3e8fe2`
payload_aggregate_sha256: `debe36f469dd4f9782f056854142ff7392a62298962d1d4b9835cd14c3758f38`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `948b6d4db1efe707aae99f36ae1b86f1a304fca69ae961b6fc3e26f9fabf3269`
cli_sha256: `b047eaff46eabc9d4f2ad9f6446f3f9d269c9de9bc24c9b54b0894937f3b7441`
tui_sha256: `58d3999a308cd3d02b3b9596556435655bb2bcd55f19a0d9001397cc2dd1d33a`
provenance_commit: `9e96ffd423addfb0de139b1dfde0f8fc555c7566`
build_utc: `2026-05-25T06:15:47.6350354Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PCVCLI read-only operator surface인 `runtime policy`, `ops summary`, `network inventory`, `network list`가 `ok=True | operation=...` 요약 대신 실제 운영 데이터를 table로 출력하도록 반영한 package 기록이다.

## 검증

- `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore`: `92/92` PASS
- `git diff --check`: PASS
- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.44-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260525-04244 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- 설치본 direct/REPL smoke: `artifacts/installed-cli-readonly-surface-smoke-20260525-04244/summary.json`, PASS

## 경계

이 package build는 internal admin-smoke evidence다. 이 package smoke 이후
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-25-04244-hostmutation.md`와
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-25-04243-04244.md`가 full admin host
mutation 및 manual-admin package-pair closure를 current anchor로 승격했다. Public trusted
signing, public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
