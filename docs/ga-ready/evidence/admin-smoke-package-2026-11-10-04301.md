# Admin smoke package `0.43.1-admin-smoke` (2026-11-10)

evidence_id: `admin-smoke-package-2026-11-10-04301`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.1-admin-smoke`
source_commit: `0be6561e0a22b5163a7214e9de9b5ebb889778cc`
release_train: `0.43.1-admin-smoke`
train_carriages: `PR #134` (변경 `1e06c13c`, web); payload: PR #134 merge
artifact_root: `artifacts/admin-smoke-package-20261110-04301`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `37c3163fa024703c3e285d6c8f2343422437b9db933e78a08df4acce88b120f1`
clean_package_payload_aggregate_sha256: `97b42b7495666c0674486980e5c097c53f2cd49d238737c83f87f1d2ec4d79e4`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `2a2eb01fdf8fc092584bc1777b196b9936d2579e4c4858402b6e48dcb48a0b64`
cli_sha256: `003cb4d45447d3558da7eb1369742a48898427ea93fbec9a6e57543834149ec5`
update_zip_sha256: `a504d2304544b1bf9ba468d5256d5e51951be9ae5fd7e56a45ed437f2e7c077e`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-10T07:42:52.1570755Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.0-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.1`의 package다. backlog counts 수정 campaign `backlog-counts-20261110`이 queue에 쌓은 Web Console 변경 한 행을 싣는다(campaign `train-04301-20261110`). 적재는 134(BL-0036 좁은 폭 셸 검색 버튼 이름, BL-0037 좁은 VM 목록의 전체 이름·상태 점·템플릿 배지, BL-0038 삭제 확인 gate 대기, BL-0039 고대비 테마 token 적용과 mockup 테마 퇴역, BL-0040 첫 방문 `runtime/policy` 401 제거, BL-0041 진단 서비스 값 문구)다. payload 파일은 `0.43.0`과 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 51초. pair baseline은 train `0.43.0` Task 1 package다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.1-admin-smoke -MsiProductVersion 0.43.1 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261110-04301
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.0 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.1-admin-smoke+0be6561e0a22b5163a7214e9de9b5ebb889778cc` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `51`초) |
| provenance commit | `0be6561e0a22b5163a7214e9de9b5ebb889778cc` (HEAD와 같음) |
| product-manifest version | `0.43.1-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.1` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 Web Console 화면(queue 134, 승인의 `probe(Web Console 화면)`) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.0-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
