# Admin smoke package `0.42.73-admin-smoke` (2026-08-14)

evidence_id: `admin-smoke-package-2026-08-14-04273`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.73-admin-smoke`
source_commit: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
artifact_root: `artifacts/admin-smoke-package-20260814-04273`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`
clean_package_payload_aggregate_sha256: `bbe2bfde532260eab7bd80de13e4e13350ae6553e4ef6a4037faa6e650359660`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `a437a78b7198cb04d588e8b80688a522b3497fe5b8cdddc41d6f3483e197e9e2`
cli_sha256: `b8a7374e843999d2979ba5181d18fb91909a375ef0482b840cb942c253b40bc2`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-13T15:26:26.9302611Z`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

origin/main HEAD `b84441f0`에서 첫 clean package를 만들었다. 이 package는
`0.42.72-admin-smoke` 이후 Web loopback session bootstrap, login `409`
`PCV_ACCOUNT_AUTH_NOT_CONFIGURED`, in-process Chromium Host gate, PR #189 diagnostics
list를 포함하며, 후속 fullgate와 `0.42.72-admin-smoke -> 0.42.73-admin-smoke`
package-pair campaign의 target이다.

| 구간 | 값 |
| --- | --- |
| baseline package | `0.42.72-admin-smoke` / `02428fab` / MSI `142a9e3d8a…` |
| build HEAD | `b84441f0` (loopback bootstrap browser-gate merge) |
| release channel | `admin-smoke` |
| MSI product version | `0.42.73` |

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 (실측 = provenance = sidecar) | `03244819…` 일치 |
| provenance commit == HEAD | `true` |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| full admin host mutation | `full-admin-host-mutation-gate-20260814-04273` / `PASS` |
| manual-admin package pair | `manual-admin-campaign-descriptor-20260814-04272-04273-closed` / `PASS` |
| actual-VM functional | `10/10` / `PASS` |
| installed current-card | `promoted-current`; Web `2/2` HTTP `200`, service `Running/Automatic`, TUI 부재 |

## Nonclaims

- 이 문서의 package build 자체는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational fullgate MSI는 같은 source commit의 별도 빌드이며 clean MSI와 hash가 다르다.
- publication descriptor의 Burn은 `not-built`, MSIX는 `not-built`, winget은
  `not-generated`, catalog는 `not-published`다.
- canonical `current-evidence.json` 승격은 같은 날 ledger update가 소유한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
