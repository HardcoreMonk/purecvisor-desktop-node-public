# Admin smoke package `0.42.92-admin-smoke` (2026-10-07)

evidence_id: `admin-smoke-package-2026-10-07-04292`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.92-admin-smoke`
source_commit: `e25095029463943ad75b166f95718a77f7661467`
release_train: `0.42.92-admin-smoke`
train_carriages: `PR #59` (변경 `ee90474`, merge `3f55831`)
artifact_root: `artifacts/admin-smoke-package-20261007-04292`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `dc79fdd166f0882a31e726e43909e0ab5b3dda09142de162c72dfa9821b104ec`
clean_package_payload_aggregate_sha256: `efbbbb16756d88a4c5e3d4b5ca746ff03db94b6fd41c99f91734741479153cff`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `ede0af77a7e8eeb8bddbadb230d050256ca8e3b01f13c915f03a563548be5b04`
cli_sha256: `052993a3fb52ee67476c6fe06ef2fae6ba560a4abea2b1d9c279833db62ddd67`
update_zip_sha256: `fcaf13dc3118371eab79013f203e5da2ba1e078401e6b2b331801a334ad57102`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-07T14:21:03.0191697Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.92`의 package다. completion autopilot(`pcv-completion-autopilot-v1`)이 판정 갭 `C2-queue`로 연 campaign `completion-20261007`의 train이며, 끊긴 `vm.create` 잔여물 회수(PR #59)를 싣는다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.92-admin-smoke -MsiProductVersion 0.42.92 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261007-04292
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.91 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.92-admin-smoke+e25095029463943ad75b166f95718a77f7661467` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `36`초) |
| provenance commit | `e25095029463943ad75b166f95718a77f7661467` (HEAD와 같음) |
| product-manifest version | `0.42.92-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.92` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `vm.create`(끊긴 create 잔여물 표식·회수·거절) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.91-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
