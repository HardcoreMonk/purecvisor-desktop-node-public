# Admin smoke package `0.42.88-admin-smoke` (2026-10-03)

evidence_id: `admin-smoke-package-2026-10-03-04288`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.88-admin-smoke`
source_commit: `ff62e596a949202d905699504cd12ce3c646dc0c`
contains_managed_delete_storage_cleanup: `96e8570` (PR #30 merge `ed4a4fa`)
artifact_root: `artifacts/admin-smoke-package-20261003-04288`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `81ef85273cb9bbf9d813d4c3cce40f88c22e5d596c40906dab8766e4cab79b64`
clean_package_payload_aggregate_sha256: `4377cc2519ab7e014c049697ecd5414891bc71a278b748bd670a8a3f0f870af6`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `f797af948a2e6bb15adbe7c04a0bd64474d9510b721381425a8d9d04f9b002cd`
cli_sha256: `91e43cbeae534c0e0dcc2324b3c2499f7aa160a01c61ca09114203ab967fae40`
update_zip_sha256: `7051e735aff0466925ec1912103dce57dad42f6f67c452cfca4026de244cb751`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-03T11:44:28.3549805Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

branch `lane2/04288-package-pair-20261003`의 clean HEAD `ff62e59`(`origin/main` `ed4a4fa` 위 campaign 개설 commit)에서 `0.42.88-admin-smoke`를 만들었다. `0.42.87` build(`8ade930`) 뒤 product 변경은 managed delete 디스크 정리(`96e8570`) 하나다. `vm delete`가 VM 전용 `ConfigurationDataRoot` 안의 미참조 디스크 이미지와 빈 디렉터리를 지우고 job 결과에 `storage_cleanup`을 남긴다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.88-admin-smoke -MsiProductVersion 0.42.88 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261003-04288
```

update ZIP은 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 직접 만들었다(항목 구성은 0.42.87 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.88-admin-smoke+ff62e596a949202d905699504cd12ce3c646dc0c` |
| self-contained `pcvcli.exe` publish | `PASS` |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `41`초) |
| provenance commit | `ff62e596a949202d905699504cd12ce3c646dc0c` (HEAD와 같음) |
| product-manifest version | `0.42.88-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.88` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, managed delete Lane 2 확인, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.87-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
