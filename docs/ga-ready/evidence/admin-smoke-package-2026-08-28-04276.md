# Admin smoke package `0.42.76-admin-smoke` (2026-08-28)

evidence_id: `admin-smoke-package-2026-08-28-04276`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.76-admin-smoke`
source_commit: `fb08d41528d43e2514de4e7ffd98a68840db7de3`
artifact_root: `artifacts/admin-smoke-package-20260828-04276`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `8d3b2453323abd7800a393ae980b1b318a71886e7a2c157b30c97c135391809b`
clean_package_payload_aggregate_sha256: `6aca4b4adf55824dd880dbfd833db4c50feb140e23836f30dacea3acd9f99fc5`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `2fbe04d9b9a3bf62dbe72882bddbcaf675fe5f6942f8f33b04863b523d33cfc4`
cli_sha256: `7fc2a92fcc3becceea90c0996afcbcdef863c6485542bd147760d738b3bbe77f`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-28T13:33:18.8017653Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

local `main` HEAD `fb08d41` (`test: add P1 clone actual-VM DryRun pester`)에서 P1-5 clone
Lane 2 프로브용 probe-vehicle package를 만들었다. 이 package는 operational current가
아니다. `docs/ga-ready/current-evidence.json`과 AGENTS.md generated current 블록은
`0.42.75-admin-smoke`로 유지한다.

P1 clone product payload는 이미 origin/main `aee39b9` (PR #8)에 있다. 04275 clean
package provenance `dbe1b48`는 이 public clone에 없고, 04276 MSI의 Host/CLI hash는
04275와 다르다. HEAD는 origin/main보다 `5`커밋 앞이며 그 구간은 Lane 2 runner/DryRun
계약과 스펙·계획이다. runner 스크립트는 MSI payload에 들어가지 않는다.

| 구간 | 값 |
| --- | --- |
| baseline current | `0.42.75-admin-smoke` / MSI `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6` |
| origin/main at build | `aee39b9460463a34086582a84b0669d8e03875d3` |
| build HEAD | `fb08d41528d43e2514de4e7ffd98a68840db7de3` |
| release channel | `admin-smoke` |
| MSI product version | `0.42.76` |
| payload source range vs origin/main | `aee39b94..fb08d415` |
| non-test product payload paths vs origin/main | `1` (`packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1`, MSI 밖) |

빌드는 `packaging/windows-desktop-node/installer/build.ps1`가 Host/CLI를
`-p:Version=0.42.76`, `-p:InformationalVersion=0.42.76-admin-smoke`로 self-contained
publish한 뒤 WiX MSI를 만들었다. 설치, 서비스, Hyper-V mutation은 없다.

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 (실측 = provenance = sidecar) | `8d3b2453…` 일치 |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.76-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

product wrapper SHA-256은 04275와 같다. Host/CLI hash는 P1 clone payload와
`0.42.76-admin-smoke` InformationalVersion으로 달라졌다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| 설치 (`0.42.75` → `0.42.76` MSI apply) | `not-run` (checkpoint C, 관리자 opt-in) |
| clone family actual-VM | `not-run` (checkpoint D, 관리자 opt-in) |
| full admin host mutation | `not-run` |
| `0.42.75-admin-smoke -> 0.42.76-admin-smoke` pair | `not-opened` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.75-admin-smoke` |

## Nonclaims

- 이 문서의 package build 자체는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational current는 `0.42.75-admin-smoke`다. 이 MSI를 current로 승격하지 않았다.
- DryRun runner PASS는 Lane 2 clone PASS가 아니다.
- publication descriptor의 Burn/MSIX는 `not-built`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
