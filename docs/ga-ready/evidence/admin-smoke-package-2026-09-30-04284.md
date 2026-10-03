# Admin smoke package `0.42.84-admin-smoke` (2026-09-30)

evidence_id: `admin-smoke-package-2026-09-30-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.84-admin-smoke`
source_commit: `aab0bc1c38cf6badb9f5b31bfbeae99c26bd4063`
artifact_root: `artifacts/admin-smoke-package-20260930-04284`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87`
clean_package_payload_aggregate_sha256: `9d8c92c5646c0d8ab7e7516602169295dddf66b89ed73679156a15709090d7f5`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `12512b67e887f8241d9cad5e8312647dd19131a0ba59949f2f8c5afa093caa3d`
cli_sha256: `78273f6d1a483f0fe6e0a2b8a8f30907a7027aa0f25e3ae514d36d482ee09c2b`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-09-30T05:36:37.3244082Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

PR #22 branch `feat/development-completion-20260930`의 HEAD `aab0bc1`에서 `0.42.83-admin-smoke -> 0.42.84-admin-smoke` pair의 target package를 만들었다.

`0.42.83` 빌드 source `6846248` 이후 product payload에 들어간 변경은 개발 완료 campaign(`development-completion-20260930`)이다.
- 중단된 job reconcile 대상 확대와 비대상 분류(Task 1-6)
- Web Console binding 16개(Task 7-12)

기준 operational current와 이 호스트의 설치본은 모두 `0.42.83-admin-smoke`다. 이 빌드에서는 설치, 서비스 교체, Hyper-V mutation을 하지 않았다.

빌드 명령:

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.84-admin-smoke -MsiProductVersion 0.42.84 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20260930-04284
```

## 빌드 결과

| item | result |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `43`초) |
| MSI SHA-256 | `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87` |
| provenance commit == HEAD | `true` |
| product-manifest version | `0.42.84-admin-smoke` |
| TUI payload | 없음 (ADR-0011 CLI/Web-only) |

## Package chain 연결

| 후속 검증 | 결과 |
| --- | --- |
| 설치본 update/rollback (`0.42.83` → `0.42.84`) | `not-run` |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-built` |
| MSIX build/install/update/remove | `not-built` |
| `0.42.83-admin-smoke -> 0.42.84-admin-smoke` pair | `not-closed` |
