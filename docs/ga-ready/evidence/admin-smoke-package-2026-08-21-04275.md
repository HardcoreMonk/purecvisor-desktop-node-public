# Admin smoke package `0.42.75-admin-smoke` (2026-08-21)

evidence_id: `admin-smoke-package-2026-08-21-04275`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.75-admin-smoke`
source_commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`
artifact_root: `artifacts/admin-smoke-package-20260821-04275`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`
clean_package_payload_aggregate_sha256: `3c33a35b21eb9cdd2b24156cc98afe2268f82f3ca32c7dd6a03882a262afdd2c`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `f8fd9147b9a2fd8ab51cf5c8a5aedea6c06bbfcd581b37dbc218680e6b780580`
cli_sha256: `7e2b99bc0eda1fb11dcaac40b24b829581de7167d79552e0c48c40decdf1211d`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-20T16:13:14.9575809Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

origin/main HEAD `dbe1b48` (`fix(hyperv): request CIM Offline for vm.save`)에서 첫 clean
package를 만들었다. 이 package는 `0.42.74-admin-smoke` 이후 `vm.save` RequestedState를
CIM Offline `6`으로 바꾸고 EnabledState `6`/`32769`를 `saved`로 매핑하는 product
payload를 포함하며, 후속 fullgate와
`0.42.74-admin-smoke -> 0.42.75-admin-smoke` package-pair campaign의 target이다.

| 구간 | 값 |
| --- | --- |
| baseline package | `0.42.74-admin-smoke` / `adc04673` / MSI `f4d0fcb75b…` |
| build HEAD | `dbe1b48` (`vm.save` CIM Offline) |
| release channel | `admin-smoke` |
| MSI product version | `0.42.75` |
| payload source range | `adc04673b569ef9b587371fdb23bc11ceb14e2e2..dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4` |
| non-test product payload paths | `2` |

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 (실측 = provenance = sidecar) | `3d3ee255…` 일치 |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.75-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

product wrapper SHA-256은 04273/04274와 같다. Host/CLI hash는 `vm.save` 요청값 변경으로
달라졌다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| full admin host mutation | `full-admin-host-mutation-gate-20260821-04275` / `PASS` |
| installed current-card | `installed-operator-surface-current-card-2026-08-27-04275` / `PASS` |
| actual-VM functional | `functional-correctness-actual-host-validation-2026-08-27-04275` / `PASS` |
| actual-VM P0 | `service-plan-p0-actual-vm-2026-08-27-04275` / `PASS` |
| manual-admin package pair | `manual-admin-campaign-descriptor-20260827-04274-04275` / `PASS` |
| `docs/ga-ready/current-evidence.json` | 승격 `0.42.75-admin-smoke` |

## Nonclaims

- 이 문서의 package build 자체는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational current는 `0.42.75-admin-smoke`다. 04274 P0 `vm.save` FAIL는 historical
  predecessor다.
- operational fullgate MSI는 같은 source commit의 별도 빌드이며 clean MSI와 hash가 다르다.
- publication descriptor의 Burn/MSIX는 후속 `0.42.74 -> 0.42.75` campaign이 소유한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
