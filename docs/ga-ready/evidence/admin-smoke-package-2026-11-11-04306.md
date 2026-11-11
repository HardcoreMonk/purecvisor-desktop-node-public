# Admin smoke package `0.43.6-admin-smoke` (2026-11-11)

evidence_id: `admin-smoke-package-2026-11-11-04306`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.43.6-admin-smoke`
source_commit: `5758b54a230d52519437cbec7d15b58a4662419e`
release_train: `0.43.6-admin-smoke`
train_carriages: `PR #152` (변경 `ff40acd7`, web), `PR #153` (변경 `25709198`, hyperv+web), `PR #157` (변경 `39976fd2`, api+hyperv+web); payload: PR #157 merge
artifact_root: `artifacts/admin-smoke-package-20261111-04306`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `59106d65ad70f57ab4cb3d9bd5b5f7cf6dde9f333af76d66ce44d9ba00dd98bd`
clean_package_payload_aggregate_sha256: `7960b364a05ba83c92ab206a78858fb321adb03a785c62807e7301d9d5089119`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `2ff7bdb4b29927cfe667b994a29e68b3c242963df8e5b8ace3a8914ced60ba5c`
cli_sha256: `98151a1c22a7e3afe3a0cb43cccaae576dfb6a87df5de60a2cd8409fc3925647`
update_zip_sha256: `048ec1bb33b893fd1587cb60859c27215be9b7e65f15322b2c072a878c805f1e`
payload_file_count: `31`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-11T09:27:46.9719117Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.43.6`의 package다. queue 세 행을 싣는다(campaign `train-04306-20261111`). 적재는 152(대시보드 첫 로드), 153(BL-0046·BL-0047: 콘솔 입력 순서, 키보드 경로 캐시, 텍스트 글자마다), 157(BL-0048·BL-0049: 콘솔 frame·input을 전역 API lock 밖에서, loopback 세션 하나, 키마다 VM 하나 상태)이다. payload 파일은 `0.43.5`와 같은 31개다(`web/app.js`·`web/styles.css` 호환 파일 포함, BL-0030 fixture). build 49초. pair baseline은 정차한 train `0.43.5` Task 1 package이고, operational current는 `0.43.4`다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.43.6-admin-smoke -MsiProductVersion 0.43.6 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261111-04306
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 31개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.43.5 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.43.6-admin-smoke+5758b54a230d52519437cbec7d15b58a4662419e` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `49`초) |
| provenance commit | `5758b54a230d52519437cbec7d15b58a4662419e` (HEAD와 같음) |
| product-manifest version | `0.43.6-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.43.6` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, 설치본 대시보드 첫 로드와 브라우저 콘솔 입력(queue 152·153·157, 승인의 probe: loopback 세션·ops/summary 횟수와 취소, frame 중 키 하나 시간, 20자 두 칸 입력 일치, 텍스트 폼) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.43.4-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
