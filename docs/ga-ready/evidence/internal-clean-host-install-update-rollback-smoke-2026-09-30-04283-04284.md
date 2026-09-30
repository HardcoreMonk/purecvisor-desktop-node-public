# Internal clean-host install/update/rollback `0.42.83-admin-smoke` -> `0.42.84-admin-smoke` (2026-09-30)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-09-30-04283-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20260930-04283-04284`
summary_sha256: `b14d09bce859738b39c5b160186ab4dffe7e2be9e56268df84ed1b5a26f11629`
vm_name: `pcv-cleanhost-20260930-04284`
base_vhd_source: `current-base`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524`
update_package_sha256: `09b22e1c1d7a186e955e65309f46690a8a794be0f419800e09a574f5470edb90`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`다. 2026-09-30 `05:41:15Z`에 시작해 `05:43:25Z`에 끝났다(약 130초, exit `0`).

```text
-ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20260930-04283-04284
-BaselineMsiPath artifacts/admin-smoke-package-20260929-04283/PureCVisorDesktopNode-0.42.83-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20260930-04284/PureCVisorDesktopNode-0.42.84-admin-smoke-update.zip
-VmName pcv-cleanhost-20260930-04284 -BaselineVersion 0.42.83-admin-smoke -TargetVersion 0.42.84-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

`-BaseVhdPath` 없이 실행했다. 그래서 base는 `current-base.json`이 골랐다(`base_vhd_source=current-base`, UBR `5622`, `KB5122882`). update ZIP은 0.42.83 ZIP과 같은 구조로 만들었다. `0.42.84` payload `8`개 파일을 root에 두고 deflate로 압축했다. guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다(`password_recorded=false`).

## 결과

| 단계 | 결과 |
| --- | --- |
| unattend 주입, PowerShell Direct | `ok` (시도 `2`, 자동 복구 없음) |
| Windows Update | 대상 `0`개, UBR `5622` 유지, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.83-admin-smoke`, service Running/Auto, Web `200` |
| catalog update | exit `0`, manifest `0.42.84-admin-smoke`, service Running, Web `200` |
| rollback | exit `0`, manifest `0.42.83-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.84-admin-smoke`) |
| VM 정리 | 삭제됨(`-RemoveVmOnSuccess`) |
| blocker | `none` |

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다. 이 호스트의 설치본은 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
