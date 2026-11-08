# Admin smoke package `0.42.99-admin-smoke` (2026-11-09)

evidence_id: `admin-smoke-package-2026-11-09-04299`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.99-admin-smoke`
source_commit: `4931d6d59c45a0775f443aa87ea5fe9208467203`
release_train: `0.42.99-admin-smoke`
train_carriages: `PR #113` (변경 `154d16cc`, web), `PR #115` (변경 `0575b859`, api), `PR #116` (변경 `5f6c1f0c`, web); payload: PR #117 merge
artifact_root: `artifacts/admin-smoke-package-20261109-04299`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `7946f464245edf9f7ab84685add805fa7656274a8511525fade9a2c0c6842af0`
clean_package_payload_aggregate_sha256: `e03dcb32faa37db01b8c4900151e0b51b7859fcbb3bb31e1f3f6e69099ea9c57`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `57c9bf7e8f883c5d5d557b405c558fb7cfddf18ea42abacbfeabc971beec78b8`
cli_sha256: `26a3afce158e8c2126bed74ca05f9b3f2ef35fc7f888a6ad907f656bb5784456`
update_zip_sha256: `aec1eb05f07c64e31e8ba4f8cdcd55f3c220af621055a6193a84e17d51498e84`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-08T19:25:01.8596334Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.99`의 package다. 완료 판정(2026-11-08) 뒤 campaign `backlog-burn-20261109`가 backlog `undecided` 18건을 모두 닫으면서 queue에 쌓은 제품 변경 세 행을 싣는다(campaign `train-04299-20261109`). 적재는 113(in-app 안내서), 115(console 입력 제한기 clock seam, BL-0011), 116(Web Console 수정 일곱 건과 native VM 목록 일시 공백 재읽기, BL-0024·0026·0027·0031·0032·0033·0034)이다. payload 파일은 `0.42.98`과 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 51초. pair baseline은 train `0.42.98` Task 1 package다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.99-admin-smoke -MsiProductVersion 0.42.99 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261109-04299
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.98 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.99-admin-smoke+4931d6d59c45a0775f443aa87ea5fe9208467203` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `51`초) |
| provenance commit | `4931d6d59c45a0775f443aa87ea5fe9208467203` (HEAD와 같음) |
| product-manifest version | `0.42.99-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.99` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `hyperv.vm-list.delete-window`(삭제 중 VM 목록, BL-0034), `api.checkpoint.restore-power`(켜진 VM 복원 거절, BL-0033) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.98-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
