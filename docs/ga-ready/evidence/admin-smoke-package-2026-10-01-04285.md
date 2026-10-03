# Admin smoke package `0.42.85-admin-smoke` (2026-10-01)

evidence_id: `admin-smoke-package-2026-10-01-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.85-admin-smoke`
source_commit: `f5b6d10fad87543cc5e17c83794fa3e29d7af406`
artifact_root: `artifacts/admin-smoke-package-20261001-04285`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `cba74683e6ae9f7e85e5f625f8e9b9f221ae27c8ffacf5912ce2861f1ef82828`
clean_package_payload_aggregate_sha256: `796faba68eeeb1864f7f877debe8b134d43fb1a4c9ce026891b83fe6f61cb54b`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `e1c0cb894b0335f03556950100f2378692e5c188e5130973fd0baee6294e30d4`
cli_sha256: `2e7e513025500f35863a12f2d92f0d68dc7bbc6ecff8859c3d11867f82fd08a6`
update_package_sha256: `74c8eeb29e3b927fc2f447fa7bea40879ebf8ce84ee6f723c12f429799cca9c4`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-30T15:33:26.1164346Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

branch `lane2/04285-package-pair-20261001`의 HEAD `f5b6d10`에서 `0.42.84-admin-smoke -> 0.42.85-admin-smoke` pair의 target package를 만들었다. 이 HEAD는 `origin/main` `5b84a4e`(PR #24 merge)에 campaign 개설 commit 하나를 더한 것이다.

`0.42.84` fullgate build `ee90e0e` 뒤 product payload에 들어간 변경은 PR #24다.
- `vm.list` `dvd_media`
- 내부 read operation `vm.disk.inspect`
- `vm.attach`/`vm.eject` reconcile과 disk-resize 판정 전환
- Web VM detail `DVD Media` 행

기준 operational current와 이 호스트 설치본은 모두 `0.42.84-admin-smoke`다. 이 빌드에서는 설치, 서비스 교체, Hyper-V mutation을 하지 않았다.

빌드 명령:

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.85-admin-smoke -MsiProductVersion 0.42.85 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261001-04285
```

update ZIP은 build가 만들지 않는다. 0.42.84와 같은 구조로 만들었다. payload `8`개 파일을 root에 두고, `/` 구분자와 deflate로 압축했다.

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `38`초) |
| MSI SHA-256 | `cba74683e6ae9f7e85e5f625f8e9b9f221ae27c8ffacf5912ce2861f1ef82828` |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.85-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| 설치본 update/rollback (`0.42.84` → `0.42.85`) | `not-run` |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-built` |
| MSIX build/install/update/remove | `not-built` |
| `0.42.84-admin-smoke -> 0.42.85-admin-smoke` pair | `not-closed` |
