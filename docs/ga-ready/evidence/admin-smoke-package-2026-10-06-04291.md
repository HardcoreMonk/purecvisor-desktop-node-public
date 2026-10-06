# Admin smoke package `0.42.91-admin-smoke` (2026-10-06)

evidence_id: `admin-smoke-package-2026-10-06-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.91-admin-smoke`
source_commit: `59cd8b6cc1d95c9b4f3a566354361b3efd6076c5`
release_train: `0.42.91-admin-smoke`
train_carriages: `PR #53` (변경 `c8be5bc`, merge `05f42a2`)
artifact_root: `artifacts/admin-smoke-package-20261006-04291`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `bdef7609de3667298325d162641578a85e191ed31075cc6238e8e0f79fbfc12f`
clean_package_payload_aggregate_sha256: `f4503dfcbde2b708c7cd0bc25ad89b69ab454935be3f92ac327a159c2f5c5cac`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `cf506bfa6401a72aa5df9f1e887243ecc73b2190e86e0bc16a80a82c454bd0ca`
cli_sha256: `0e9e4b8df411f03d2d4b72dbab55a255c2758470e0f5448df2f2616e4315b058`
update_zip_sha256: `f31ae34ca27932892fc6aad9255122b88bc8fa15a4518bab63e6a4eca6a43568`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-06T10:39:40.0750092Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.91`의 package다. 완료 probe가 0.42.90 설치본에서 찾은 template lock 결함 수정(PR #53)을 싣는다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.91-admin-smoke -MsiProductVersion 0.42.91 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261006-04291
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.90 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.91-admin-smoke+59cd8b6cc1d95c9b4f3a566354361b3efd6076c5` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `42`초) |
| provenance commit | `59cd8b6cc1d95c9b4f3a566354361b3efd6076c5` (HEAD와 같음) |
| product-manifest version | `0.42.91-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.91` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `vm.template.lock`(완료 probe P1-6·P1-7과 함께) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.90-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
