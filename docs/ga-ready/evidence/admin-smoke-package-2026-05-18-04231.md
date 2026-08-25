# Admin-smoke package 2026-05-18 0.42.31

evidence_id: `admin-smoke-package-2026-05-18-04231`
result: `PASS`
scope: `internal-admin-smoke-package-build`
version: `0.42.31-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260518-04231`
package_build_decision: `executed-pcvcli-interactive-shell-and-linux-cli-parity`
msi_sha256: `173c1e1487e1b032c11ca528d83c5bb4ede77b7fec747a082cd79f2b7b6317ee`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`
operational_full_gate_msi_sha256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`
operational_full_gate_payload_aggregate_sha256: `cea7d1f798e6f0889cf0cd02da049dc7d7b0131e8df51a768c12e02ea76c22f4`
operational_full_gate_provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
payload_aggregate_sha256: `47c97ee43fb28e1757544ee947cd146efa88112789263047e91e705a8e6640e4`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `615a2241751c5ede497416b72ab95594a28c03fce08a886b90c302beaf2c5418`
cli_sha256: `d6e221bb143f928e0c95abb97e84ab8174353113131e4f5866b6e0d30580a06f`
tui_sha256: `461e6bcd27d7a45fe1d2a1963d488fec907df64a2bbcb28c0906a3c96ff7e848`
provenance_commit: `068c4d93cf7ab203983427e8999c64d1fcbfb873`
build_utc: `2026-05-18T12:54:41.7755829Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 PCVCLI Linux CLI parity alias와 interactive shell이 포함된
`0.42.31-admin-smoke` package build 기록이다. Build는 `AllowUnsignedDev` 내부
admin-smoke 범위이며 public trusted signing, winget 제출, public stable installer
URL, 외부 stable publication은 주장하지 않는다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260518-04231/PureCVisorDesktopNode-0.42.31-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/admin-smoke-package-20260518-04231/PureCVisorDesktopNode-0.42.31-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260518-04231/PureCVisorDesktopNode-0.42.31-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `173c1e1487e1b032c11ca528d83c5bb4ede77b7fec747a082cd79f2b7b6317ee` |
| provenance commit | `068c4d93cf7ab203983427e8999c64d1fcbfb873` |
| payload file version | `1.42.31.0` |

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.31-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260518-04231 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified`: exit `0`
- `dotnet publish` service host: exit `0`
- `dotnet publish` `pcvcli.exe`: exit `0`
- `dotnet publish` `pcvtui.exe`: exit `0`
- `wix build`: exit `0`

## 경계

이 package build 자체는 full admin host mutation gate 또는 manual-admin package-pair
closure가 아니다. 설치본 PCVCLI interactive shell smoke는
`docs/ga-ready/evidence/installed-pcvcli-interactive-shell-smoke-2026-05-18-04231.md`가
소유한다.

이후 full admin host mutation gate에서 같은 `0.42.31-admin-smoke` version을
operational package로 재빌드했다. Current installed/package anchor는
`artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`의 MSI SHA-256
`c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`와 provenance commit
`fc8cc284b7824172b8bf035858fb86b21bd26e5d`가 소유한다.
