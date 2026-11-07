# Admin smoke package `0.42.97-admin-smoke` (2026-11-08)

evidence_id: `admin-smoke-package-2026-11-08-04297`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.97-admin-smoke`
source_commit: `3a8da146e1d1d39bad2b6d5ff04b008deed8bdc8`
release_train: `0.42.97-admin-smoke`
train_carriages: `PR #94` (변경 `271ab040`, host), `PR #95` (변경 `08c50e08`, host), `PR #97` (변경 `13fc25dd`, web), `PR #98` (변경 `ab9b6704`, web), `PR #99` (변경 `8b0ae171`, web), `PR #100` (변경 `16da5810`, web), `PR #101` (변경 `f0db33e4`, web), `PR #102` (변경 `eb0bdcf7`, web), `PR #103` (변경 `289d98ea`, web), `PR #104` (변경 `213bfe36`, api), `PR #105` (변경 `8403d47d`, web), `PR #106` (변경 `3d2fd89e`, web), `PR #107` (변경 `449a62ce`, web); payload: PR #107 merge
artifact_root: `artifacts/admin-smoke-package-20261108-04297`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `cc7127b63ef909f0e91378ac0e4e7987597ea1cb4e5dda225669c82f3fc7e6d7`
clean_package_payload_aggregate_sha256: `e2a0fd6b6be2764e4313d238a2450ce61f8d8062600d6932da0c1b20481c3641`
product_wrapper_sha256: `a2ec149aa9634ec8276f799996390424fddebed67bfaf88dae05a933f63d4103`
service_host_sha256: `13a3c966b7fead1b5fab82a6e15d083b850a832a1246d4e1bfa6d94e79a21c64`
cli_sha256: `ebc7a4e3125491c332f7e5cc6b5a4d739984bbfd713b94fd75a06878690810fe`
update_zip_sha256: `fdb2dce5a80d132c72321b0c352d49a55f1b440586133dacb324e3f08e5e4723`
payload_file_count: `29`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-11-07T20:30:36.8499100Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.97`의 package다. 2026-11-08 사용자 지시(새 Web Console 화면을 설치본에 먼저 반영)와 승인 `1,2,3,4`로 S4 판정보다 먼저 출발한 campaign `train-04297-20261108`의 train이며 queue 94·95(`configure-installed`의 LAN 옵션과 실행 중 service 재시작), 97~103·105~107(Web Console 재설계, 옛 콘솔 퇴역, 브라우저 품질 게이트), 104(계정 비밀번호 재설정 API)를 싣는다. 옛 콘솔 파일 3개(`web/app.js`, `web/styles.css`, `web/index.legacy.html`)가 빠져 payload 파일은 29개. build 40초. pair baseline은 같은 날 `0.42.96` payload source `87a7deb`에서 다시 빌드한 package다(재클론으로 원래 artifact가 없음).

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.97-admin-smoke -MsiProductVersion 0.42.97 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261108-04297
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 29개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.96 ZIP과 다르다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.97-admin-smoke+3a8da146e1d1d39bad2b6d5ff04b008deed8bdc8` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `40`초) |
| provenance commit | `3a8da146e1d1d39bad2b6d5ff04b008deed8bdc8` (HEAD와 같음) |
| product-manifest version | `0.42.97-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.97` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `host.service.configure-lan`(S4 계정 복원 뒤 LAN 재적용, PR #94·#95), `web.console.vm-detail`·`web.console.frame-view`·`web.console.checkpoints`·`web.console.vm-create`·`web.console.shell-views`·`web.console.confirm-copy`(설치본 새 셸 브라우저 확인), `api.account.password`(계정 비밀번호 재설정) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.96-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
