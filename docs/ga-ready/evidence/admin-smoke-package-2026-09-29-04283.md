# Admin smoke package `0.42.83-admin-smoke` (2026-09-29)

evidence_id: `admin-smoke-package-2026-09-29-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.83-admin-smoke`
source_commit: `bcda14f0f02c256acc125dafbd43da3b4a93b3d0`
artifact_root: `artifacts/admin-smoke-package-20260929-04283`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524`
clean_package_payload_aggregate_sha256: `76d9ae9cfcc13fbffb999ad0b447d8d24ac31ec31fd14df20a59bbe681ca8e15`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `8d8622fb42c9b6254b8a7e6849a38136dbbf0d31b767f8201885e351f9a47209`
cli_sha256: `c217df9f3124f97c8c248843740988c7d9bd87a778c7afc24c6f130e42324df4`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-29T13:10:54.3582936Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

`lane3/04283-promotion-20260929` HEAD `bcda14f`에서 `0.42.78-admin-smoke -> 0.42.83-admin-smoke`
pair의 target package를 만들었다. 이 HEAD는 `origin/main` `c7b8813`에 Lane 3 계획 commit 하나를 더한 것이다.
`c7b8813`은 PR #17 merge이고, 그 아래에 PR #16과 #15가 있다.

`0.42.78` 빌드 source `0de176f` 이후 product payload에 들어간 주요 변경은 다음과 같다.
- P1-8 guest file 부모 디렉터리 생성(`77346bf`)
- External switch 분류와 route DVD guard(PR #15)

기준 operational current는 `0.42.78-admin-smoke`다. 이 호스트의 설치본은 probe `0.42.82-admin-smoke`다. 이 빌드에서는 설치, 서비스 교체, Hyper-V mutation을 하지 않았다.

| 구간 | 값 |
| --- | --- |
| baseline current | `0.42.78-admin-smoke` |
| installed product manifest | `0.42.82-admin-smoke` (probe) |
| build HEAD | `bcda14f0f02c256acc125dafbd43da3b4a93b3d0` |
| release channel | `admin-smoke` |
| MSI product version | `0.42.83` |

빌드 명령:

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.83-admin-smoke -MsiProductVersion 0.42.83 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20260929-04283
```

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` |
| MSI SHA-256 | `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524` |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.83-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

payload는 `8`개 파일이다. `DesktopNode.Host.exe`, `pcvcli.exe`, `Invoke-PcvDesktopNodeProduct.ps1`, `PcvDesktopNodeProduct.psm1`, `product-manifest.json`과 `web/` 아래 파일들이다.

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| 설치본 update/rollback (`0.42.78` → `0.42.83`) | `not-run` |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-built` |
| MSIX build/install/update/remove | `not-built` |
| `0.42.78-admin-smoke -> 0.42.83-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.78-admin-smoke` |

## Nonclaims

- 이 package build는 설치, 서비스, Hyper-V 또는 OS mutation을 하지 않았다.
- operational current는 `0.42.78-admin-smoke`다. 이 MSI를 current로 승격하지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
