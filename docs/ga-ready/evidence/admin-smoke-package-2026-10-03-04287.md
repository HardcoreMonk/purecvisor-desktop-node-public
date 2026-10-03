# Admin smoke package `0.42.87-admin-smoke` (2026-10-03)

evidence_id: `admin-smoke-package-2026-10-03-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.87-admin-smoke`
source_commit: `8d940dab1e2aae1ad1e4cac5def45eb26a4343ae`
contains_reconcile_wording_fix: `5b5738e`
contains_same_version_upgrade: `8d940da`
artifact_root: `artifacts/admin-smoke-package-20261003-04287`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `a0041c9f70c6e9003950bb672c74be4d41a8ad2e06fe690481d63266f0dbbcc1`
clean_package_payload_aggregate_sha256: `7c3393887f4cce4a923ba932b716b9f0878d2e9924c6ece64188913fcb0b42a2`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `f6916d3f674f3525586c25b30d4c088e3ec32d1990e21ad8904e3de448058adc`
cli_sha256: `3c681733cd7d11087d41a58a540b512f92ead344d846212c5a2fc02fa677fca7`
update_zip_sha256: `28dc8af80d334dd8e3bec8acaf826a8d9033e5c66ffddf553956955fb07d2de3`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-03T07:09:55.3213072Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

PR #28 branch `docs/project-status-audit-20261003`의 clean HEAD `8d940da`에서 `0.42.87-admin-smoke`를 만들었다. PR #28은 아직 merge 전이다. 이 build는 `0.42.86` build(`b807803`) 뒤의 두 product 변경을 담는다.

- reconcile 안내 문구(`5b5738e`): 비대상 operation의 안내가 `rename` 대신 `mutation`을 말한다.
- 같은 version 재설치 A(`8d940da`): `MajorUpgrade AllowSameVersionUpgrades="yes"`, `Schedule="afterInstallValidate"`.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.87-admin-smoke -MsiProductVersion 0.42.87 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261003-04287
```

update ZIP은 build가 만들지 않으므로 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 직접 만들었다
(`PureCVisorDesktopNode-0.42.87-admin-smoke-update.zip`, 항목 구성은 0.42.86 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.87-admin-smoke+8d940dab1e2aae1ad1e4cac5def45eb26a4343ae` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `40`초) |
| MSI SHA-256 | `a0041c9f70c6e9003950bb672c74be4d41a8ad2e06fe690481d63266f0dbbcc1` |
| payload aggregate SHA-256 | `7c3393887f4cce4a923ba932b716b9f0878d2e9924c6ece64188913fcb0b42a2` |
| provenance commit | `8d940dab1e2aae1ad1e4cac5def45eb26a4343ae` (HEAD와 같음) |
| product-manifest version | `0.42.87-admin-smoke` |
| TUI payload | 없음 |

## MSI Upgrade 테이블 (읽기 전용 확인)

| MSI | upgrade 행 | downgrade 감지 행 | `RemoveExistingProducts` |
| --- | --- | --- | --- |
| `0.42.87` | `VersionMax=0.42.87`, Attributes `513`(`VersionMaxInclusive` 포함) | `VersionMin=0.42.87`, `OnlyDetect` | `1401` (`InstallValidate` `1400` 바로 뒤) |
| `0.42.86` (비교) | `VersionMax=0.42.86`, Attributes `1`(같은 version 미포함) | `VersionMin=0.42.86`, `OnlyDetect` | `1401` |

같은 version의 다른 ProductCode가 0.42.87 MSI의 major upgrade 대상이고, 새 파일을 쓰기 전에 지워진다.
같은 version은 downgrade로 감지되지 않는다. 실제 설치 동작은 Task 4 fullgate가 실증한다.

## 아직 하지 않은 검증

| 후속 검증 | 결과 |
| --- | --- |
| 설치 | `not-run` |
| 설치본 update/rollback | `not-run` |
| clean-host | `not-run` |
| Burn install/repair/remove | `not-built` |
| MSIX build/install/update/remove | `not-built` |
| `0.42.86-admin-smoke -> 0.42.87-admin-smoke` pair | `not-closed` |
| fullgate와 같은 version 재설치 실증 | `not-run` |
| installed current-card | `not-run` |
| reconcile 문구 Lane 2 확인 | `not-run` |
| current-evidence 쓰기 | `not-run` |

## Nonclaims

- operational current는 `0.42.86-admin-smoke`다. 이 패키지는 `package_candidate`다.
- 이 호스트 설치본은 이 빌드로 바뀌지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
