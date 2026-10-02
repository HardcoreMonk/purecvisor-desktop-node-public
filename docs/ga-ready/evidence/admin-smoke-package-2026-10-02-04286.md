# Admin smoke package `0.42.86-admin-smoke` (2026-10-02)

evidence_id: `admin-smoke-package-2026-10-02-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.86-admin-smoke`
source_commit: `1c488b62bc68783f4673bb4f441f65d5de5c561c`
contains_media_fix: `fb95de1b6349a3d10cfadd814f9e50bbbc72126a`
artifact_root: `artifacts/admin-smoke-package-20261002-04286`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6`
clean_package_payload_aggregate_sha256: `afa5cb95c4268f7118c52c554e3061b738c01cd3d4a3b4c75af04d094ee61484`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `55a1c1a80faa4328afcb88a487b64a8cddf603d53d75988d18a33ccd14e8df9b`
cli_sha256: `fd01b161a142e7e2d92ada0a1800e4a413bce1f0946ac7695a73a6ca1d6d5bbd`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-02T05:50:45.1479176Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

PR #26 병합 커밋 `1c488b6`에서 `0.42.86-admin-smoke`를 만들었다. 이 커밋은 `fb95de1`의 DVD 미디어 수정을 포함한다. `vm.eject`는 media SASD에 `RemoveResourceSettings`를 호출하고, 빈 drive의 `vm.attach`는 drive RASD 아래에 media를 `AddResourceSettings`로 추가한다.

이 빌드는 `0.42.85-admin-smoke`를 다시 만들지 않는다. `0.42.85` pair evidence는 PR #25에 있다. 패키지를 만들 때는 그 PR이 아직 `main`에 없었고, 병합 커밋 `b807803`은 문서와 campaign 기록이라 이 MSI의 provenance가 아니다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.86-admin-smoke -MsiProductVersion 0.42.86 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261002-04286
```

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`) |
| MSI SHA-256 | `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6` |
| payload aggregate SHA-256 | `afa5cb95c4268f7118c52c554e3061b738c01cd3d4a3b4c75af04d094ee61484` |
| provenance commit | `1c488b62bc68783f4673bb4f441f65d5de5c561c` |
| product-manifest version | `0.42.86-admin-smoke` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

| 후속 검증 | 결과 |
| --- | --- |
| 설치 | `not-run` |
| 설치본 update/rollback | `not-run` |
| clean-host | `not-run` |
| Burn install/repair/remove | `not-built` |
| MSIX build/install/update/remove | `not-built` |
| `0.42.85-admin-smoke -> 0.42.86-admin-smoke` pair | `not-closed` |
| Lane 2 eject/빈 drive probe | `not-run` |
| fullgate | `not-run` |
| installed current-card | `not-run` |
| current-evidence 쓰기 | `not-run` |

## Nonclaims

- operational current는 `0.42.84-admin-smoke`다. 이 패키지는 `package_candidate`다.
- 이 호스트 설치본은 이 빌드로 바뀌지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
