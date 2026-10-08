# Admin smoke package `0.42.93-admin-smoke` (2026-10-08)

evidence_id: `admin-smoke-package-2026-10-08-04293`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.93-admin-smoke`
source_commit: `41421d8bf3272dbdbb8dcf384bcf1ff3f329726a`
release_train: `0.42.93-admin-smoke`
train_carriages: `PR #68` (변경 `92146da` QoS `mutation_supported`, `65c376d` `vm.create` reconcile, merge `d9e7d03`, payload `56e7cd0`)
artifact_root: `artifacts/admin-smoke-package-20261008-04293`
signing_mode: `AllowUnsignedDev`
signing_trust_model: `LocalTest`
clean_package_msi_sha256: `13d7f0d476828f865b0d4aca7331a2dcd1a10a8dbe157b9d6a93dc2217f9fb49`
clean_package_payload_aggregate_sha256: `2a071cd2c6298e289b940a37ef124a1c3d3502fd9a476033174251884a0f2cf7`
product_wrapper_sha256: `8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3`
service_host_sha256: `6fcd3af260d14b21a88352cf2fe8568c9c91df561072ab28da34688439e50661`
cli_sha256: `fed6d209af02f0f8185c8f674dd3badf38860aefaf8e9db3d36d9239c69e13af`
update_zip_sha256: `97e4ec6a6376fea970cd2f084ec4352d062bb00cb42e2fdfc0f08d1cb7dfd181`
payload_file_count: `8`
wix_version: `5.0.2+aa65968c`
build_utc: `2026-10-08T05:42:52.3292771Z`
host_mutation_performed: `false`
package_installed: `false`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 배경

release train `0.42.93`의 package다. completion autopilot(`pcv-completion-autopilot-v1`)이 판정 갭 `C2-queue`로 연 campaign `completion-20261008`의 train이며, PR #68의 `vm.qos` readback과 `vm.create` reconcile을 싣는다.

빌드 중 설치, 서비스 교체, Hyper-V 변경은 하지 않았다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.93-admin-smoke -MsiProductVersion 0.42.93 `
  -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261008-04293
```

update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload 8개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다(항목 구성은 0.42.92 ZIP과 같다).

## 빌드 결과

| 항목 | 결과 |
| --- | --- |
| self-contained `DesktopNode.Host.exe` publish | `PASS`, ProductVersion `0.42.93-admin-smoke+41421d8bf3272dbdbb8dcf384bcf1ff3f329726a` |
| self-contained `pcvcli.exe` publish | `PASS`, ProductVersion가 Host와 같다 |
| MSI build (WiX 5.0.2) | `PASS` (exit `0`, `35`초) |
| provenance commit | `41421d8bf3272dbdbb8dcf384bcf1ff3f329726a` (HEAD와 같음) |
| product-manifest version | `0.42.93-admin-smoke` |
| MSI Upgrade 행 | `VersionMax=0.42.93` 포함(Attributes `513`), `RemoveExistingProducts` `1401` |
| TUI payload | 없음 |

## 아직 하지 않은 검증

pair, fullgate, current-card, `vm.qos`(readback `mutation_supported`), `vm.create reconcile`(디스크·ISO·Default Switch) Lane 2 probe, current-evidence 쓰기는 `not-run`이다.

## Nonclaims

- operational current는 `0.42.92-admin-smoke`다. 이 패키지는 `package_candidate`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
