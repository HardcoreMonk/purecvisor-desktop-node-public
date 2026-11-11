# Admin smoke package `0.43.7-admin-smoke` (2026-11-11)

evidence_id: `admin-smoke-package-2026-11-11-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.7-admin-smoke`
source_commit: `74b956256c585c032536a117281dadc128d9794a`
release_train: `0.43.7-admin-smoke`
train_carriages: `PR #152` (변경 `ff40acd7`, web), `PR #153` (변경 `25709198`, hyperv+web), `PR #157` (변경 `39976fd2`, api+hyperv+web), `PR #159` (변경 `04b1a91b`, api+web); payload: PR #160 merge (제품 소스는 PR #159 merge와 같음)
artifact_root: `artifacts/admin-smoke-package-20261111-04307`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `a808cda4d54eae4caf012a8d7b012663b6caa46f683741599150dae7e0223a31`
clean_package_payload_aggregate_sha256: `33e39bd2191fc825d62815f296b9e11b4a2b2f052125665e77d427d504a4f7ef`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `0fff0d6bc245e3b642a80639b91c488a2d65b73d94589ba08cba1f8d3db19f1f`
cli_sha256: `b7f93e88f475858df0a9fc6e9b3b3af503963b04474849a20e11635894f2fa2b`
update_zip_sha256: `fcc5182ac1a2e44b8ea09a5e462f1b7c6ec2b2cba4a04c9fb10b43774dbafe7d`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-11T12:45:02.5464360Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.7`의 package다. queue 네 행을 싣는다(campaign `train-04307-20261111`). 적재는 152(대시보드 첫 로드), 153(BL-0046·BL-0047: 콘솔 입력 순서, 키보드 경로 캐시, 텍스트 글자마다), 157(BL-0048·BL-0049: 콘솔 frame·input을 전역 API lock 밖에서, loopback 세션 하나, 키마다 VM 하나 상태), 159(BL-0050·BL-0051: 콘솔 route는 전역 요청 한도에서 빠짐, 첫 service worker claim reload 없음)이다. payload 파일은 `0.43.6`과 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 47초. pair baseline은 정차한 train `0.43.6` Task 1 package이고, operational current는 `0.43.4`다. 같은 제품 소스의 개발 build는 dev probe `lane2-devprobe-2026-11-11-04307`에서 먼저 확인했다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.7-admin-smoke -MsiProductVersion 0.43.7 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04307
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.6 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.7-admin-smoke+74b956256c585c032536a117281dadc128d9794a` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `47`초) |
| provenance commit | `74b956256c585c032536a117281dadc128d9794a` (HEAD와 같음) |
| product-manifest version | `0.43.7-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.7` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 대시보드 첫 로드와 브라우저 콘솔 키 입력(queue 152·153·157·159, 승인의 probe: ops/summary 횟수와 취소, 목록 시각, 키 요청 시간과 거절 건수, 20자 두 칸 일치. 텍스트 보내기 폼은 BL-0047로 따로 다룬다) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.4-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
