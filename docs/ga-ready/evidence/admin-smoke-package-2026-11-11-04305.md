# Admin smoke package `0.43.5-admin-smoke` (2026-11-11)

evidence_id: `admin-smoke-package-2026-11-11-04305`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.5-admin-smoke`
source_commit: `884d442ef0a30079b70061ab0b0cae82c1de4e9b`
release_train: `0.43.5-admin-smoke`
train_carriages: `PR #152` (변경 `ff40acd7`, web), `PR #153` (변경 `25709198`, hyperv+web); payload: PR #153 merge
artifact_root: `artifacts/admin-smoke-package-20261111-04305`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `5f4699097d267ef3bddda081d0351f5f14ae286803d1447ce1eeeba5ef727d8d`
clean_package_payload_aggregate_sha256: `b5151f72c9ebfa5c7cb462627ec4bb3964dac79bad8bfe34c0647c2a565b1f8d`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `6cd9bf08ad096a333d44de0bce7beefd4e3ca40a25144501c9a05029885c71b2`
cli_sha256: `dc07340a85ebe8965379b759a1bd6390867a6a7671657c5aa768dc85433b483a`
update_zip_sha256: `a302528b383a9a29f81eb2506fdd5b26b8db751a6a4d61b2c0217c00c0ac6e4b`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-11T06:20:51.4791893Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.5`의 package다. 제로베이스 감사 후속 campaign `zero-base-followup-20261111`이 queue에 쌓은 변경 두 행을 싣는다(campaign `train-04305-20261111`). 적재는 152(대시보드 첫 로드: 첫 갱신 한 번, 대시보드 단계 먼저 동시에)와 153(BL-0046·BL-0047: 브라우저 콘솔 입력을 VM별 도착 순서로, 키보드 경로 캐시, 텍스트는 글자마다)이다. payload 파일은 `0.43.4`와 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 46초. pair baseline은 train `0.43.4` Task 1 package다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.5-admin-smoke -MsiProductVersion 0.43.5 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04305
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.4 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.5-admin-smoke+884d442ef0a30079b70061ab0b0cae82c1de4e9b` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `46`초) |
| provenance commit | `884d442ef0a30079b70061ab0b0cae82c1de4e9b` (HEAD와 같음) |
| product-manifest version | `0.43.5-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.5` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 대시보드 첫 로드와 브라우저 콘솔 입력(queue 152·153, 승인의 probe: 키 하나 시간, 20자 두 칸 입력 일치, 텍스트 폼) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.4-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
