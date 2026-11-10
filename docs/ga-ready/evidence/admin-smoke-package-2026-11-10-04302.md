# Admin smoke package `0.43.2-admin-smoke` (2026-11-10)

evidence_id: `admin-smoke-package-2026-11-10-04302`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.2-admin-smoke`
source_commit: `b45e8215db21ecd952da38d4c0ebbdbc194a4d89`
release_train: `0.43.2-admin-smoke`
train_carriages: `PR #138` (변경 `424780d8`, api-web); payload: PR #138 merge
artifact_root: `artifacts/admin-smoke-package-20261110-04302`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `fa6afbce618568a95bb941c34ded38c188d7a8c221bfc26810d3ac14111b1c0c`
clean_package_payload_aggregate_sha256: `35ff7e2ea42152980b60adf8b63b8cade7cdff1aa5100820fe0c8b5e02d2d226`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `815254ede087b8001ca1e92b0b27a6f202f50d1f4bf6732fe406b16b1a1fbeb8`
cli_sha256: `5ca79c1a78939b208ce295b989673108c43cf79b41970a5a5d63ac9fce3dd6ad`
update_zip_sha256: `c9854f1c22dafc98b4f04e7f2e0af3d926d10021453b5f957a7da509f66b61b4`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-10T11:35:13.7841670Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.1-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.2`의 package다. BL-0042 수정 campaign `bl0042-version-badge-20261110`이 queue에 쌓은 변경 한 행을 싣는다(campaign `train-04302-20261110`). 적재는 138(BL-0042: runtime policy 응답의 `product_version`을 Host가 자기 informational version으로 채우고, Web Console version 표시가 `v<version>`과 title의 전체 값을 보인다)이다. payload 파일은 `0.43.1`과 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 47초. pair baseline은 train `0.43.1` Task 1 package다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.2-admin-smoke -MsiProductVersion 0.43.2 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261110-04302
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.1 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.2-admin-smoke+b45e8215db21ecd952da38d4c0ebbdbc194a4d89` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `47`초) |
| provenance commit | `b45e8215db21ecd952da38d4c0ebbdbc194a4d89` (HEAD와 같음) |
| product-manifest version | `0.43.2-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.2` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 Web Console 화면과 runtime policy `product_version`(queue 138, 승인의 `probe(Web Console 화면)`) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.1-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
