# Admin smoke probe-vehicle package `0.42.82-admin-smoke` (2026-09-28)

evidence_id: `admin-smoke-package-2026-09-28-04282`
result: `PASS`
evidence_scope: `internal-admin-smoke-probe-vehicle`
version: `0.42.82-admin-smoke`
source_commit: `77346bfc9c34312e290cc2d42272e19c48ab25f0`
artifact_root: `artifacts/admin-smoke-package-20260928-04282`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `6078f250fe0dc52fdc4e6a77cb8f14b80e881df80e179a6961851665d0019d81`
clean_package_payload_aggregate_sha256: `3d0e0917fa53522b1babd1c3c0bc314cb18a4abe440025e5505c1f3122310d26`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `501befba4208de3104dadc3584591ac4701626ad3b3d0a28f540399cacc0e994`
cli_sha256: `71728be6269bb0448e299e6db44b1dd8f86bed182b17fc18a7200c63f285bad2`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-28T11:49:37.9419423Z`
host_mutation_performed: `true` (MSI 업그레이드만)
package_installed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

`0.42.81`에서 guest file copy가 guest 부모 디렉터리 부재로 실패했다
(`service-plan-p1-guest-file-actual-vm-2026-09-28-04281`). 부모 디렉터리 생성 수정(`77346bf`)을 담은 probe-vehicle을
만들어 설치했다. 이 HEAD는 PR #15(후속 결함: packaging Pester, External switch 분류, route DVD guard)를 포함한다.
operational current 승격이 아니다.

승인: `User-Approval: 2026-09-28 0.42.82 빌드 + 설치 + 재실행`.

## 빌드

`packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.82-admin-smoke -MsiProductVersion 0.42.82
-SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`, clean HEAD `77346bf`, provenance commit == HEAD, payload 8개.

## 설치 (0.42.81 → 0.42.82)

`msiexec /i <msi> REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*v
artifacts/admin-smoke-package-20260928-04282/install-upgrade-04281-to-04282.log`

| 항목 | 설치 전 | 설치 후 |
| --- | --- | --- |
| ARP `PureCVisor Desktop Node` | `{96A2ABA0-EFAB-417B-B4EB-227070371B6C}` `0.42.81` | `{3027FF28-52F9-4328-BC5F-244AA2A31BD4}` `0.42.82` (1개) |
| product manifest | `0.42.81-admin-smoke` | `0.42.82-admin-smoke` |
| service | Running | Running, Automatic |
| msiexec exit | | `0` |
| 설치본 `pcvcli.exe` / `DesktopNode.Host.exe` SHA-256 | | build와 같음 |
| log `Won't Overwrite` / `another client exists` | | `0` / `0` |
| loopback Web `/` / API 비인증 | | `200` / `401` |

## Nonclaims

- operational current는 `0.42.78-admin-smoke`다. current-evidence, fullgate, pair는 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
