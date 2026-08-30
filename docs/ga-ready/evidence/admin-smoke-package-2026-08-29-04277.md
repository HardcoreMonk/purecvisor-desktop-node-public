# Admin smoke package `0.42.77-admin-smoke` (2026-08-29)

evidence_id: `admin-smoke-package-2026-08-29-04277`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.77-admin-smoke`
source_commit: `04b3c9ff1fb146db42a3a08a5d8566075b7bb3a6`
artifact_root: `artifacts/admin-smoke-package-20260829-04277`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `d03eedaf12d344ccd2d74c87237aa8d920ea3474be498c7fe91bfa4394984957`
clean_package_payload_aggregate_sha256: `370a267f7c9fdec1d89c9a1890af4941c688d25b9cad634d45de3774b5e4b99c`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `d5588f0311be7ec8ef5daae352600e2f54a052c9191b2fedfe5b6bb154556902`
cli_sha256: `51e924c490b54a55195e9d675174dcfbcbcb3eccff758e596d6dfb2cb77f36f3`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-08-28T15:59:32.4586362Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

local `main` HEAD `04b3c9f` (`fix: pin P1 clone disk, Off state, and vm-root`)에서 P1 clone
Lane 2 프로브용 probe-vehicle package를 만들었다. 이 package는 operational current가
아니다. `docs/ga-ready/current-evidence.json`과 AGENTS.md generated current 블록은
`0.42.75-admin-smoke`로 유지한다.

04276 설치본 clone은 기본 `D:\PureCVisor\VMs`에 대상을 만들어 전용 VmRoot 계약이
FAIL였다. 04277 Host/CLI는 clone `--vm-root` / body `vm_root`를 넣는다. runner는 MSI
payload에 들어가지 않는다.

| 구간 | 값 |
| --- | --- |
| baseline current | `0.42.75-admin-smoke` |
| installed at build | `0.42.76-admin-smoke` |
| origin/main at build | `aee39b9460463a34086582a84b0669d8e03875d3` |
| build HEAD | `04b3c9ff1fb146db42a3a08a5d8566075b7bb3a6` |
| release channel | `admin-smoke` |
| MSI product version | `0.42.77` |

빌드는 `packaging/windows-desktop-node/installer/build.ps1`가 Host/CLI를
`-p:Version=0.42.77`, `-p:InformationalVersion=0.42.77-admin-smoke`로 self-contained
publish한 뒤 WiX MSI를 만들었다. 설치, 서비스, Hyper-V mutation은 없다.

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 (실측 = provenance = sidecar) | `d03eedaf…` 일치 |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.77-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

payload 구성은 `DesktopNode.Host.exe`, `pcvcli.exe`,
`Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`,
`product-manifest.json`, `web/app.js`, `web/index.html`, `web/styles.css`의 8개다.

product wrapper SHA-256은 04275/04276과 같다. Host/CLI hash는 clone `vm_root` 계약과
`0.42.77-admin-smoke` InformationalVersion으로 달라졌다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| 설치 (`0.42.76` → `0.42.77` Update) | `not-run` (checkpoint C, 관리자 opt-in) |
| clone family actual-VM | `not-run` (checkpoint D, 새 artifact root) |
| full admin host mutation | `not-run` |
| `0.42.75-admin-smoke -> 0.42.77-admin-smoke` pair | `not-opened` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.75-admin-smoke` |

## Nonclaims

- 이 문서의 package build 자체는 설치, 서비스, Hyper-V 또는 OS mutation을 수행하지 않았다.
- operational current는 `0.42.75-admin-smoke`다. 이 MSI를 current로 승격하지 않았다.
- 04276 r3 FAIL를 pass로 재해석하지 않는다.
- publication descriptor의 Burn/MSIX는 `not-built`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
