# Admin smoke package `0.43.4-admin-smoke` (2026-11-11)

evidence_id: `admin-smoke-package-2026-11-11-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.4-admin-smoke`
source_commit: `54a817c4ed0a8ab14056405310a998571c99d578`
release_train: `0.43.4-admin-smoke`
train_carriages: `PR #146` (변경 `08589a5f`, web); payload: PR #146 merge
artifact_root: `artifacts/admin-smoke-package-20261111-04304`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `c24ef8317c838d659834b7e77446625d9377e5774f9057fedc813318a739fccb`
clean_package_payload_aggregate_sha256: `29ba7ece25684f58b11c2e1ba4122569047494a814ef1a4ab49dee7c8959fb3d`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `c61ec11b18eb2060ccf6e61011210949cb9ff6fc9343d9fabf1cc02d43718e8f`
cli_sha256: `9899c3dbb34161526401bf86f25fa0dacdf8ae77d90ea2c3a7d0d46906344948`
update_zip_sha256: `92630abe393ea54fa3acb0c8a775e03d18fc8c27d0aeb1c4d8b1bc81f3cdb2cc`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-10T18:18:32.4087570Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.4`의 package다. BL-0044 수정 campaign `bl0044-alerts-loading-20261111`이 queue에 쌓은 변경 한 행을 싣는다(campaign `train-04304-20261111`). 적재는 146(BL-0044: 첫 갱신이 진행 중이면 대시보드 "경고" 카드가 "운영 요약을 읽지 못했습니다" 대신 "운영 요약을 읽고 있습니다."를 보인다)이다. payload 파일은 `0.43.3`과 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 47초. pair baseline은 train `0.43.3` Task 1 package다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.4-admin-smoke -MsiProductVersion 0.43.4 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04304
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.3 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.4-admin-smoke+54a817c4ed0a8ab14056405310a998571c99d578` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `47`초) |
| provenance commit | `54a817c4ed0a8ab14056405310a998571c99d578` (HEAD와 같음) |
| product-manifest version | `0.43.4-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.4` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 Web Console 대시보드 "경고" 카드의 첫 갱신 중 문구(queue 146, 승인의 `probe(Web Console 화면)`) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.3-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
