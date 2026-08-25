# Admin smoke package `0.42.72-admin-smoke` (2026-08-09)

evidence_id: `admin-smoke-package-2026-08-09-04272`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.72-admin-smoke`
source_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
artifact_root: `artifacts/admin-smoke-package-20260809-04272`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22`
clean_package_payload_aggregate_sha256: `39475ad14a9bbd48ecf41c24bac5e42b391535783276cd5ed4d960af276962f0`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `c989fa5db901a7e64bd9b5040024804b0c3a3ee9a3ad138a94a06007d7ef86b3`
cli_sha256: `c7fac8d2f671596878ae58808b79028d7a2951dee371c5f371984a1d23f2d60c`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-09T08:12:20.4939539Z`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

PR #186 merge head `02428fab`에서 첫 clean package를 만들었다. 이 package는
`0.42.71-admin-smoke` 이후의 token replace hardening과 Host/Runtime/Hyper-V 경계의 partial
분할을 포함하며, 후속 fullgate와 `0.42.71 -> 0.42.72` package-pair campaign의 target이다.

| 구간 | 값 |
| --- | --- |
| baseline package | `0.42.71-admin-smoke` / `80f69f31` / MSI `ebb621ada4…` |
| build HEAD | `02428fab` (PR #186 merge commit) |
| release channel | `admin-smoke` |
| MSI product version | `0.42.72` |

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 (실측 = provenance = sidecar) | `142a9e3d…` 일치 |
| provenance commit == HEAD | `true` |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| full admin host mutation | `full-admin-host-mutation-gate-20260809-04272` / `PASS` |
| manual-admin package pair | `0.42.71 -> 0.42.72` / descriptor `6/0/0` |
| actual-VM functional | QoS, shrink guard, 11 GiB expansion, cleanup `PASS` |
| installed current-card | CLI `3/3`, Web `2/2`, service `Running/Automatic` |

## Nonclaims

- 이 문서의 package build 자체는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational fullgate MSI는 같은 source commit의 별도 빌드이며 clean MSI와 hash가 다르다.
- publication descriptor의 Burn은 `not-built`, MSIX는 `not-built`, winget은
  `not-generated`, catalog는 `not-published`다. 이는 publication descriptor 범위이며,
  별도 manual-admin campaign의 Burn/MSIX lifecycle `PASS`와 모순되지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
