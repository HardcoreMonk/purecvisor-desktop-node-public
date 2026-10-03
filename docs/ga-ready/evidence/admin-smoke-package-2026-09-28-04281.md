# Admin smoke probe-vehicle package `0.42.81-admin-smoke` (2026-09-28)

evidence_id: `admin-smoke-package-2026-09-28-04281`
result: `PASS`
evidence_scope: `internal-admin-smoke-probe-vehicle`
version: `0.42.81-admin-smoke`
source_commit: `0c2f84b59d01008b6da8526aeeb74d55373abd0c`
artifact_root: `artifacts/admin-smoke-package-20260928-04281`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `abf50b21d80cec9cb90bec81a9773c6012212be2a813ee72d1d926c40f9604f1`
clean_package_payload_aggregate_sha256: `c3195bd2eb9f24d3d243b1ff4ee9881fb9a15af71e7008c1313593897b5c40ff`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `5ee7d502555f751ac37b33bfe85246d51e3644b2f52d5300dab0835df4345ec1`
cli_sha256: `554960a18fd35bd10d85700b36d6e787ff684e922137d911132cb4d3e543f939`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-27T17:07:05.4748279Z`
host_mutation_performed: `true` (MSI 업그레이드만)
package_installed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

`0.42.80` probe에서 import가 소스 VM 디스크를 가리켰다
(`service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04280`). import를 전용 VM root 복사로 고친
`a3dba76`과 runner import 검증·root 삭제 guard `0c2f84b`를 담은 probe-vehicle을 만들어 설치했다. operational
current 승격이 아니다.

승인: `User-Approval: 2026-09-28 import 저장소 결정(전용 root로 복사), 0.42.81 probe 빌드·설치`.

## 빌드

`packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.81-admin-smoke -MsiProductVersion 0.42.81
-SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`, clean HEAD `0c2f84b`, provenance commit == HEAD.
payload 8개 구성은 `admin-smoke-package-2026-09-28-04280`과 같다.

## 설치 (0.42.80 → 0.42.81)

`msiexec /i <msi> REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*v
artifacts/admin-smoke-package-20260928-04281/install-upgrade-04280-to-04281.log`

| 항목 | 설치 전 | 설치 후 |
| --- | --- | --- |
| ARP `PureCVisor Desktop Node` | `{98D823C8-30FC-4D6B-8BA8-001217100FC3}` `0.42.80` | `{96A2ABA0-EFAB-417B-B4EB-227070371B6C}` `0.42.81` (1개) |
| product manifest | `0.42.80-admin-smoke` | `0.42.81-admin-smoke` |
| service | Running | Running, Automatic |
| msiexec exit | | `0` |
| 설치본 `pcvcli.exe` SHA-256 | | `554960a1…`(build와 같음) |
| 설치본 `DesktopNode.Host.exe` SHA-256 | | `5ee7d502…`(build와 같음) |
| log `Won't Overwrite` / `another client exists` | | `0` / `0` |
| loopback Web `/` | | `200` |
| API 비인증 `/api/v1/vms` | | `401` |

## Nonclaims

- operational current는 `0.42.78-admin-smoke`다. `current-evidence.json`, AGENTS generated 블록을 바꾸지 않았다.
- fullgate, manual-admin pair, clean-host update/rollback, Burn/MSIX는 실행하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
