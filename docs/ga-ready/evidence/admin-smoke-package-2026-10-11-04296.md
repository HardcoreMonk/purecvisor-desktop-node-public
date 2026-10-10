# Admin smoke package `0.42.96-admin-smoke` (2026-10-11)

evidence_id: `admin-smoke-package-2026-10-11-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.96-admin-smoke`
source_commit: `e07113c5f555715955856897c687c7c8843db1dc`
release_train: `0.42.96-admin-smoke`
train_carriages: `PR #78` (변경 `1b04ff46`, hyperv), `PR #79` (변경 `04ffe117`, web), `PR #81` (변경 `4e109e70`, web), `PR #84` (변경 `3d50f14b`, installer), `PR #86` (변경 `179bd198`, web); payload: PR #86 merge
artifact_root: `artifacts/admin-smoke-package-20261011-04296`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `326b867a161ffa5038f4b3cecd8405ca0999cff00875c4a9fb8f5ac92f3cdeed`
clean_package_payload_aggregate_sha256: `b76c53108e93b37f6d8ca9236f51a1aee5ea252f67c9ffc1ca68883983449104`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `db179cda0f34b42adf5c342de4d2363dd3160713b5176142bec989797d185c23`
cli_sha256: `f8a6856fcfcc410623382cb08f0a78c8f69eadeca02cc4a06546480c36bdc689`
update_zip_sha256: `e4324e2420724dc4383b142debb600465dee84d415243183f138e602530c6a94`
payload_file_count: `32`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-10T16:52:56.3848655Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.96`의 package다. train `0.42.94`(pair Burn bucket, BL-0017)와 `0.42.95`(Lane 2 `web.console.shell` probe, BL-0019)가 정차한 뒤 수정 PR #84(생성 `WebPayload.wxs`의 디렉터리별 `RemoveFolder`)와 #86(Single Edge nav가 보이는 section의 주인, 옛 view 상태 동기화, 도움말 전용 section)을 merge하고 2026-10-11 승인 2로 연 campaign `train-04296-20261011`의 train이며 queue 78(`vm.create` inventory readback 대기, BL-0014), 79(VM 상세 submit 버튼 click 위임 수정, BL-0016), 81(Single Edge 프론트엔드 구조 차용 ADR-0018), 84, 86을 싣는다. payload 파일은 32개. build 42초.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.96-admin-smoke -MsiProductVersion 0.42.96 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261011-04296
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 32개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.95 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.96-admin-smoke+e07113c5f555715955856897c687c7c8843db1dc` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `42`초) |
| provenance commit | `e07113c5f555715955856897c687c7c8843db1dc` (HEAD와 같음) |
| product-manifest version | `0.42.96-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.96` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `vm.create`(inventory readback, BL-0014), `web.console.shell`(Single Edge 셸 화면 전환과 브라우저 시연, BL-0019), `checkpoint.schedule.set`(S3 브라우저 예약 저장과 예약 실행, BL-0016), `burn.lifecycle`(pair Burn bucket PASS, BL-0017) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.93-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
