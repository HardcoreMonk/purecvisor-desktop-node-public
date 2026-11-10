# Admin smoke package `0.43.3-admin-smoke` (2026-11-11)

evidence_id: `admin-smoke-package-2026-11-11-04303`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.3-admin-smoke`
source_commit: `e1e1b8b47c68a4d54af4d1fe8e85b9a3c2355eeb`
release_train: `0.43.3-admin-smoke`
train_carriages: `PR #142` (변경 `3d1ce598`, web); payload: PR #142 merge
artifact_root: `artifacts/admin-smoke-package-20261111-04303`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `66546521fb0fea092de8d0fd390153cc337108d9c17d5622015b748d901de060`
clean_package_payload_aggregate_sha256: `ef0c85dba321261bb6900caedc49fa95b1f58f488cd4aae3740115c5d159cd09`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `78533536771113d74085f7f1b33675a22b19ed785bf56df70891edf23021c6aa`
cli_sha256: `8bd705ccee328ea8b4ae7d3cd2791652f23795680fb913818d3fe42bebab3c83`
update_zip_sha256: `c431ba7573540753ffb9ba4071d824581781bbb3af60be9d344020d4fcede26f`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-10T15:38:40.1350932Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.3`의 package다. BL-0043 수정 campaign `bl0043-alerts-card-20261110`이 queue에 쌓은 변경 한 행을 싣는다(campaign `train-04303-20261111`). 적재는 142(BL-0043: 새 갱신에 밀려 취소된 요청이 Web Console 상태를 쓰지 않아, 대시보드 "경고" 카드가 운영 요약을 받은 뒤 "운영 요약을 읽지 못했습니다"를 보이지 않는다)이다. payload 파일은 `0.43.2`와 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 48초. pair baseline은 train `0.43.2` Task 1 package다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.3-admin-smoke -MsiProductVersion 0.43.3 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04303
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.2 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.3-admin-smoke+e1e1b8b47c68a4d54af4d1fe8e85b9a3c2355eeb` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `48`초) |
| provenance commit | `e1e1b8b47c68a4d54af4d1fe8e85b9a3c2355eeb` (HEAD와 같음) |
| product-manifest version | `0.43.3-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.3` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 Web Console 대시보드 "경고" 카드(queue 142, 승인의 `probe(Web Console 화면)`) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.2-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
