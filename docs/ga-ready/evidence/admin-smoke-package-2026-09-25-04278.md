# Admin smoke package `0.42.78-admin-smoke` (2026-09-25)

evidence_id: `admin-smoke-package-2026-09-25-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.78-admin-smoke`
source_commit: `e098e0a55333afe7eccd9150c5ef9ca14578cc40`
artifact_root: `artifacts/admin-smoke-package-20260925-04278`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
clean_package_payload_aggregate_sha256: `999f7106d6c63594f9e13d0d17dcfa97364f5b40ea4b86e28342776ef7b16ac1`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `f2314360f9053d7b90fe6445a0c741aab4cf97d1eea82a02d477e98b0f340f6b`
cli_sha256: `9222ef938873bf5e05e12f8b5af9be30cf0e2f72602309d12dcaf8669748ade7`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-24T15:51:05.1068288Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

`feat/p1-9-account-crud` HEAD `e098e0a`에서 P2-14 스위치 service-action과 P2-15
`vm.nic.add` / `vm.dvd.add` payload의 package candidate를 만들었다. 기준 operational
current는 `0.42.77-admin-smoke`다. 이 빌드는 `0.42.77-admin-smoke -> 0.42.78-admin-smoke`
pair의 target MSI만 봉인한다.

이 호스트의 product manifest는 `0.42.77-admin-smoke`이고, MSI DisplayVersion은
`0.42.75`다. 설치, 서비스 교체, Hyper-V mutation은 없다.

| 구간 | 값 |
| --- | --- |
| baseline current | `0.42.77-admin-smoke` |
| installed product manifest | `0.42.77-admin-smoke` |
| installed MSI DisplayVersion | `0.42.75` |
| build HEAD | `e098e0a55333afe7eccd9150c5ef9ca14578cc40` |
| release channel | `admin-smoke` |
| MSI product version | `0.42.78` |

빌드는 `packaging/windows-desktop-node/installer/build.ps1`가 Host/CLI를
`-p:Version=0.42.78`, `-p:InformationalVersion=0.42.78-admin-smoke`로 self-contained
publish한 뒤 WiX MSI를 만들었다.

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 | `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d` |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.78-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| 설치본 update/rollback (`0.42.77` → `0.42.78`) | `not-run` |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-built` |
| MSIX build/install/update/remove | `not-built` |
| `0.42.77-admin-smoke -> 0.42.78-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.77-admin-smoke` |

## Nonclaims

- 이 package build는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational current는 `0.42.77-admin-smoke`다. 이 MSI를 current로 승격하지 않았다.
- publication descriptor의 Burn/MSIX는 `not-built`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
