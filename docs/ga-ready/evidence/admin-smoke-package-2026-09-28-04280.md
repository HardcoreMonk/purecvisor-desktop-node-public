# Admin smoke probe-vehicle package `0.42.80-admin-smoke` (2026-09-28)

evidence_id: `admin-smoke-package-2026-09-28-04280`
result: `PASS`
evidence_scope: `internal-admin-smoke-probe-vehicle`
version: `0.42.80-admin-smoke`
source_commit: `017c008e0efbe69b2f17d0ff631b6f5ef653be1a`
artifact_root: `artifacts/admin-smoke-package-20260928-04280`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `e28678f2b4884a06c0a2246c9f2e39009035782c61b3a08e684edd04b2bf7ba6`
clean_package_payload_aggregate_sha256: `6c1d102a564ecc020353606eab50dac02c73ba2900a7c60c239a60f35e79ea4d`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `4521dd194dbb523c5654f2bec5efc32fd2daa2576cc74181190cbeebd3f1325e`
cli_sha256: `d952915543e064e2d31a6e472072a64676ed0988d6974473bdfe0bd57e7a5273`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-27T16:38:09Z`
host_mutation_performed: `true` (MSI 업그레이드만)
package_installed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

`0.42.79` probe에서 P2-13 import가 `.vmgs` 파일로 자기 Gen2 export를 거절하고, P2-14 network connect가
Private switch 때문에 `network.inventory` topology 오류로 실패했다
(`service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04279-r2`,
`service-plan-p2-offvm-network-connect-actual-vm-2026-09-28-04279`). 두 수정(`017c008`)을 담은 probe-vehicle을
만들어 설치했다. operational current 승격이 아니다.

승인: `User-Approval: 2026-09-28 import guard 결정(.vmgs 파일 검사 제거, TPM 검사 유지), 0.42.80 probe 빌드·설치`.

## 빌드

`packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.80-admin-smoke -MsiProductVersion 0.42.80
-SigningMode AllowUnsignedDev -SigningTrustModel LocalTest`, clean HEAD `017c008`, provenance commit == HEAD.
payload 8개 구성은 `admin-smoke-package-2026-09-28-04279`와 같다.

## 설치 (0.42.79 → 0.42.80)

`msiexec /i <msi> REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*v
artifacts/admin-smoke-package-20260928-04280/install-upgrade-04279-to-04280.log`

| 항목 | 설치 전 | 설치 후 |
| --- | --- | --- |
| ARP `PureCVisor Desktop Node` | `{958C2D0D-ACAB-4C90-9AD2-C6A43E252537}` `0.42.79` | `{98D823C8-30FC-4D6B-8BA8-001217100FC3}` `0.42.80` (1개) |
| product manifest | `0.42.79-admin-smoke` | `0.42.80-admin-smoke` |
| service | Running | Running, Automatic |
| msiexec exit | | `0` |
| 설치본 `pcvcli.exe` SHA-256 | | `d9529155…`(build와 같음) |
| 설치본 `DesktopNode.Host.exe` SHA-256 | | `4521dd19…`(build와 같음) |
| log `Won't Overwrite` / `another client exists` | | `0` / `0` |
| loopback Web `/` | | `200` |
| API 비인증 `/api/v1/vms` | | `401` |

## Nonclaims

- operational current는 `0.42.78-admin-smoke`다. `current-evidence.json`, AGENTS generated 블록을 바꾸지 않았다.
- fullgate, manual-admin pair, clean-host update/rollback, Burn/MSIX는 실행하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
