# Admin smoke package `0.42.74-admin-smoke` (2026-08-20)

evidence_id: `admin-smoke-package-2026-08-20-04274`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.74-admin-smoke`
source_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
artifact_root: `artifacts/admin-smoke-package-20260820-04274`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
clean_package_payload_aggregate_sha256: `c55cd17d14fed521252e6fee1bf08c828410339b23172fadb01dbd19f7d2578e`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `328de2af97a8ba2c132bb0a5de15504bf602233b24a2ce687c2a83f4b10335f9`
cli_sha256: `21b22cdaa9640ea8b63a031e4a815da1f583a60ca3c6e8486595bdc4a5eb07b0`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-20T13:42:08.9382675Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.74-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

local `main` HEAD `adc04673` (`Merge branch 'feat/service-plan-p0-attach'`)에서 첫 clean
package를 만들었다. 이 package는 `0.42.73-admin-smoke` 이후 SERVICE_PLAN P0 product
payload(media attach, checkpoint restore reconcile, Hyper-V Saved save/resume-saved,
managed import)를 포함하며, 후속 fullgate와
`0.42.73-admin-smoke -> 0.42.74-admin-smoke` package-pair campaign의 target이다.

| 구간 | 값 |
| --- | --- |
| baseline package | `0.42.73-admin-smoke` / `b84441f0` / MSI `03244819d1…` |
| build HEAD | `adc04673` (SERVICE_PLAN P0 merge) |
| release channel | `admin-smoke` |
| MSI product version | `0.42.74` |
| payload source range | `b84441f0750a9f77fd0588a86912dbdb68b94f0c..adc04673b569ef9b587371fdb23bc11ceb14e2e2` |
| non-test product payload paths | `33` |

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 (실측 = provenance = sidecar) | `f4d0fcb7…` 일치 |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.74-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

product wrapper SHA-256은 04273과 같다. Host/CLI/web payload hash는 P0 구현으로
달라졌다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| full admin host mutation | `full-admin-host-mutation-gate-20260820-04274` / `PASS` |
| installed current-card | `installed-operator-surface-current-card-2026-08-20-04274` / `PASS` |
| actual-VM functional | `functional-correctness-actual-host-validation-2026-08-20-04274` / `PASS` |
| actual-VM P0 | `service-plan-p0-actual-vm-2026-08-20-04274` / `FAIL` (`vm.save` WMI `32775`) |
| manual-admin package pair | `manual-admin-campaign-descriptor-20260820-04273-04274-closed` / `PASS` |
| `docs/ga-ready/current-evidence.json` | 승격 `0.42.74-admin-smoke` |

## Nonclaims

- 이 문서의 package build 자체는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational current는 `0.42.74-admin-smoke`다. P0 `vm.save` actual-VM FAIL는 열린 결함이다.
- operational fullgate MSI는 같은 source commit의 별도 빌드이며 clean MSI와 hash가 다르다.
- publication descriptor의 Burn은 `not-built`, MSIX는 `not-built`, winget은
  `not-generated`, catalog는 `not-published`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
