# Admin smoke probe-vehicle package `0.42.79-admin-smoke` (2026-09-28)

evidence_id: `admin-smoke-package-2026-09-28-04279`
result: `PASS`
evidence_scope: `internal-admin-smoke-probe-vehicle`
version: `0.42.79-admin-smoke`
source_commit: `424b51512732468ac10bc9ae2df1e81ab93501cd`
artifact_root: `artifacts/admin-smoke-package-20260928-04279`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `1cc71d5b77c21b052a71c8bc6402726fce18c73b54e7fca3ed5e9ef80c1903de`
clean_package_payload_aggregate_sha256: `739bfdd1bf22e1e1c0345dd12b2192ccebcd98e0b5efdf132a43898b07bda306`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `9d5246bd41c290c61020461b72a94c991db147b55be9279b105003647fb9c5eb`
cli_sha256: `ab9686e4e381e6c0f110bf422786dd20fe880300c704f89163bb281a3bf5ee5f`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-27T16:18:26.0770239Z`
host_mutation_performed: `true` (MSI upgrade만)
package_installed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

설치본 `0.42.78`은 Off VM을 `stopped`로 보고하는 inventory와 `"Off"`만 받는 export/import, VM network
connect 정책이 어긋나 두 기능군을 Off VM에서도 거절한다. 수정(`9402774`, `VmPowerStates.IsOff`)이 든 branch
`lane2/p2-offvm-20260927` HEAD `424b515`로 probe-vehicle package를 만들어 설치했다. 목적은 P2-13, P2-14 Lane 2
actual-VM 검증이다. operational current 승격이 아니다.

승인: `User-Approval: 2026-09-28 Task 7 빌드 + 설치 승인`.

## 빌드

`packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.79-admin-smoke -MsiProductVersion 0.42.79
-SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`. clean HEAD에서 실행했고 provenance commit이 HEAD와 같다.
Host/CLI는 self-contained publish다. payload는 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`, `product-manifest.json`, `web/app.js`,
`web/index.html`, `web/styles.css` 8개다.

## 설치 (0.42.78 → 0.42.79)

`msiexec /i <msi> REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*v
artifacts/admin-smoke-package-20260928-04279/install-upgrade-04278-to-04279.log`

| 항목 | 설치 전 | 설치 후 |
| --- | --- | --- |
| ARP `PureCVisor Desktop Node` | `{2A2AA89B-DAB2-4810-B19F-B1D5C53D6945}` `0.42.78` | `{958C2D0D-ACAB-4C90-9AD2-C6A43E252537}` `0.42.79` (1개) |
| product manifest | `0.42.78-admin-smoke` | `0.42.79-admin-smoke` |
| service | Running | Running, Automatic |
| msiexec exit | | `0` |
| 설치본 `pcvcli.exe` SHA-256 | | `ab9686e4…`(build와 같음) |
| 설치본 `DesktopNode.Host.exe` SHA-256 | | `9d5246bd…`(build와 같음) |
| log `Won't Overwrite` / `another client exists` | | `0` / `0` |
| loopback Web `/` | | `200` |
| API 비인증 `/api/v1/vms` | | `401` |

MajorUpgrade가 이전 ProductCode를 지웠고 파일이 교체됐다. 같은 version 재빌드 문제
(`docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md`)는 새 version이라
생기지 않았다.

## Nonclaims

- operational current는 `0.42.78-admin-smoke`다. `current-evidence.json`, AGENTS generated 블록을 바꾸지 않았다.
- fullgate, `0.42.78 -> 0.42.79` manual-admin pair, clean-host update/rollback, Burn/MSIX는 실행하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
