# Admin smoke package `0.42.90-admin-smoke` (2026-10-05)

evidence_id: `admin-smoke-package-2026-10-05-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.90-admin-smoke`
source_commit: `0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c`
release_train: `0.42.90-admin-smoke`
train_carriages: `PR #48` (변경 `b433c50`, merge `a66a8cd`)
artifact_root: `artifacts/admin-smoke-package-20261005-04290`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `54277baafea5be820572c082a874b75c23f55ca3fab0c374cd2b6ca357012146`
clean_package_payload_aggregate_sha256: `e6cec1d059367fc0003f74c417230d24e4d49d35c99dda428c3f40c9ca085fed`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `b7b3dcb7c59a20bcb1a57b4ac3e6bbd58ba84cdf1262a5f4414feee2105d6cf8`
cli_sha256: `7f42aca3c16118a0b3bf041d7ae6c93424fcc0e29662288b6325c1bc5000c0e4`
update_zip_sha256: `1c91de5c16ae73f624deb3802d25a42cf0c66053b126714c5b2208075e1a3abb`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `10/05/2026 12:07:30`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.90`의 package다. 3c 이후 첫 train이며 update ZIP, catalog, `package-facts.json`은 `New-PcvAdminSmokeUpdatePackage.ps1`이 만들었다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.90-admin-smoke -MsiProductVersion 0.42.90 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261005-04290
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다.

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.90-admin-smoke+0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `44`초) |
| provenance commit | `0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c` (HEAD와 같음) |
| product-manifest version | `0.42.90-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.90` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 없음(실행 값 변경 없음) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.89-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
