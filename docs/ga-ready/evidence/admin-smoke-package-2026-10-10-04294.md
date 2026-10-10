# Admin smoke package `0.42.94-admin-smoke` (2026-10-10)

evidence_id: `admin-smoke-package-2026-10-10-04294`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.94-admin-smoke`
source_commit: `59af872e559d3debc6c0a2b0e0cb56014a50291a`
release_train: `0.42.94-admin-smoke`
train_carriages: `PR #78` (변경 `1b04ff46`, hyperv), `PR #79` (변경 `04ffe117`, web), `PR #81` (변경 `4e109e70`, web, merge `4e109e7`); payload `a054db3` (PR #82 merge)
artifact_root: `artifacts/admin-smoke-package-20261010-04294`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `a9a4ec7700b84535b5ce7af27738560c575e76b031ebf1e5dab9685b73431096`
clean_package_payload_aggregate_sha256: `2e1b20b78cad161979227a79057531be5b0a9f5ba369f04ce6485b3f9b6a932b`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `11b16940504a253433cc33479ab7be0656d6409d6652af1d88f2a8c6a003f3ee`
cli_sha256: `596b1acd03531b46ab4a6611c394649447beaaac0f9ac433478620f2eff7206b`
update_zip_sha256: `f649c5d613069fa042666a288c993168c06838aeff9cf39d3ea329e772f40018`
payload_file_count: `32`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-10T11:34:23.0989417Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.94`의 package다. 2026-10-10 승인 1로 연 campaign `train-04294-20261010`의 train이며 queue 78(`vm.create` inventory readback 대기와 EnabledState 전이 어휘, BL-0014), 79(VM 상세 click 위임이 form 안 submit 버튼을 건너뛰게 수정, BL-0016), 81(Single Edge 프론트엔드 구조 차용 ADR-0018: 로그인 페이지+앱 셸 `index.html`을 `/`에서 서빙, `app.bundle.js`, help 카탈로그·문서 포털·PWA, 옛 콘솔은 `index.legacy.html` 유지)을 싣는다. web 자산이 늘어 payload 파일은 8개에서 32개가 됐다. build 42초.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.94-admin-smoke -MsiProductVersion 0.42.94 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261010-04294
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 32개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.93 ZIP과 다르다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.94-admin-smoke+59af872e559d3debc6c0a2b0e0cb56014a50291a` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `42`초) |
| provenance commit | `59af872e559d3debc6c0a2b0e0cb56014a50291a` (HEAD와 같음) |
| product-manifest version | `0.42.94-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.94` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `vm.create`(inventory readback, BL-0014), `web.console.shell`(Single Edge 셸 브라우저 시연), `checkpoint.schedule.set`(S3 브라우저 예약 저장 재시연, BL-0016) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.93-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
