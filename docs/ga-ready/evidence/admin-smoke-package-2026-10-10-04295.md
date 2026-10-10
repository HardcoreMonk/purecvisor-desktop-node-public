# Admin smoke package `0.42.95-admin-smoke` (2026-10-10)

evidence_id: `admin-smoke-package-2026-10-10-04295`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.95-admin-smoke`
source_commit: `b401d949a0d17eece428aaef83330e9e5f20ed59`
release_train: `0.42.95-admin-smoke`
train_carriages: `PR #78` (변경 `1b04ff46`, hyperv), `PR #79` (변경 `04ffe117`, web), `PR #81` (변경 `4e109e70`, web), `PR #84` (변경 `3d50f14b`, installer, merge `f84d46a`); payload `f84d46a` (PR #84 merge)
artifact_root: `artifacts/admin-smoke-package-20261010-04295`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `50680d3c757596e8fce85b3cbb41a7228e8a1ced244678d620fe912f371fab24`
clean_package_payload_aggregate_sha256: `53a58bded4c5eec6f6e430fd4dfd8b69f904b4773c7f45841e224cc3c830381f`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `6f58816923c179d9373e3ba9e1e19867146667a9ed60740e34fd4d5042c33b21`
cli_sha256: `5e9138bb625f3550a4b4e23bb4967cdbe03eb2a76c339d1b290bbae60af07225`
update_zip_sha256: `32350c7422f365a8c15a4a5b1fb5e4cd6a05650e475230b6be49f62926d09887`
payload_file_count: `32`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-10T14:19:06.0172474Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.95`의 package다. train `0.42.94`가 pair Burn bucket에서 정차(BL-0017: ZIP 제품 Update가 먼저 만든 web 하위 디렉터리를 MSI 제거가 남김)한 뒤 수정 PR #84(생성 `WebPayload.wxs`의 디렉터리별 `RemoveFolder`, `web`·INSTALLFOLDER 제거)를 merge하고 2026-10-10 승인 2로 연 campaign `train-04295-20261010`의 train이며 queue 78(`vm.create` inventory readback 대기, BL-0014), 79(VM 상세 submit 버튼 click 위임 수정, BL-0016), 81(Single Edge 프론트엔드 구조 차용 ADR-0018), 84(RemoveFolder 수정)를 싣는다. payload 파일은 32개. build 39초.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.95-admin-smoke -MsiProductVersion 0.42.95 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261010-04295
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 32개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.94 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.95-admin-smoke+b401d949a0d17eece428aaef83330e9e5f20ed59` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `39`초) |
| provenance commit | `b401d949a0d17eece428aaef83330e9e5f20ed59` (HEAD와 같음) |
| product-manifest version | `0.42.95-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.95` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `vm.create`(inventory readback, BL-0014), `web.console.shell`(Single Edge 셸 브라우저 시연), `checkpoint.schedule.set`(S3 브라우저 예약 저장 재시연, BL-0016), `burn.lifecycle`(pair Burn bucket PASS, BL-0017) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.93-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
