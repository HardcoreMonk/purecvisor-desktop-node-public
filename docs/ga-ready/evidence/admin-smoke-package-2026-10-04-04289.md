# Admin smoke package `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `admin-smoke-package-2026-10-04-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.89-admin-smoke`
source_commit: `b463903153a6ffe75b438e673c231e78d4c99909`
release_train: `0.42.89-admin-smoke`
train_carriages: `PR #34` (변경 `0c95852`, merge `d6711f3`)
artifact_root: `artifacts/admin-smoke-package-20261004-04289`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `e4574861a06537aacf41f16e75138e9f8cc9c5d4c8f0df7d7e977bd58e0b7391`
clean_package_payload_aggregate_sha256: `01bbfae597ecad5bf214fa68252c2e1099bfea877850cc716b9a41c3c53e76ca`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `16cbf3bb92aa80500eedab69df9ead29c3dd7ecfc02210408de484654d23bb76`
cli_sha256: `f71778c918f5a737df2221cc7ae2958e47ec647d81ead49f5094ba8c5ff66c86`
update_zip_sha256: `b852641609601f77cccf015969e3135212f35562e5dc694dd690fb59ca505f71`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-04T02:09:37.6951388Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.89`의 첫 package다. branch `train/04289-20261004`의 clean HEAD `b463903`(`origin/main` `d6711f3` 위 출발 commit)에서 `0.42.89-admin-smoke`를 만들었다. `0.42.88` build(`ff62e59`) 뒤 product 변경은 train에 실은 PR #34 하나다. `pcvcli --json`이 오류를 stdout에 JSON envelope 하나로 쓴다(stderr와 exit code는 그대로).

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.89-admin-smoke -MsiProductVersion 0.42.89 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261004-04289
```

update ZIP은 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 직접 만들었다(항목 구성은 0.42.88 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.89-admin-smoke+b463903153a6ffe75b438e673c231e78d4c99909` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `38`초) |
| provenance commit | `b463903153a6ffe75b438e673c231e78d4c99909` (HEAD와 같음) |
| product-manifest version | `0.42.89-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.89` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `cli.json-errors` Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.88-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
