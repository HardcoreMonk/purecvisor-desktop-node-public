# Internal clean-host install/update/rollback `0.42.85-admin-smoke` -> `0.42.86-admin-smoke` (2026-10-02)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-02-04285-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20261002-04285-04286`
summary_sha256: `a5b43b4f4fa389b4ae8ce9e96673d4e416bb95b7b58bd803286cf6599cf53e2e`
vm_name: `pcv-cleanhost-20261002-04286`
base_vhd_source: `current-base`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `cba74683e6ae9f7e85e5f625f8e9b9f221ae27c8ffacf5912ce2861f1ef82828`
update_package_sha256: `5255453cd66336f5cf94ffc7489712ffdcf6f17ff14d93f92fd5af2c56f9a357`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`다. exit `0`, 약 172초다.

```text
-ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20261002-04285-04286
-BaselineMsiPath artifacts/admin-smoke-package-20261001-04285/PureCVisorDesktopNode-0.42.85-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261002-04286/PureCVisorDesktopNode-0.42.86-admin-smoke-update.zip
-VmName pcv-cleanhost-20261002-04286 -BaselineVersion 0.42.85-admin-smoke -TargetVersion 0.42.86-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

base는 `current-base.json`이 골랐다(UBR `5622`). guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다. update ZIP은 기존 `0.42.86` payload 8개 파일로 만들었고 MSI는 다시 빌드하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`, 자동 복구 없음) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.85-admin-smoke`, Web `200` |
| catalog update | exit `0`, manifest `0.42.86-admin-smoke` |
| rollback | exit `0`, manifest `0.42.85-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.86-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.85-admin-smoke`로 남았다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
