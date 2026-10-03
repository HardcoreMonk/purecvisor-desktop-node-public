# Internal clean-host install/update/rollback `0.42.86-admin-smoke` -> `0.42.87-admin-smoke` (2026-10-03)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-03-04286-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20261003-04286-04287`
summary_sha256: `c24a19e7c51a9f9858e9327a3a9827251e1141f15199ee5782e3e4be42285c56`
vm_name: `pcv-cleanhost-20261003-04287`
base_vhd_source: `current-base`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6`
update_package_sha256: `28dc8af80d334dd8e3bec8acaf826a8d9033e5c66ffddf553956955fb07d2de3`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`다. exit `0`, 약 135초다.

```text
-ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20261003-04286-04287
-BaselineMsiPath artifacts/admin-smoke-package-20261002-04286/PureCVisorDesktopNode-0.42.86-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261003-04287/PureCVisorDesktopNode-0.42.87-admin-smoke-update.zip
-VmName pcv-cleanhost-20261003-04287 -BaselineVersion 0.42.86-admin-smoke -TargetVersion 0.42.87-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

base는 `current-base.json`이 골랐다(UBR `5622`, `KB5122882`). guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다. update ZIP은 Task 2의 `0.42.87` payload 8개 파일로 만든 것이고 MSI는 다시 빌드하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.86-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.87-admin-smoke` |
| rollback | exit `0`, manifest `0.42.86-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.87-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.86-admin-smoke`로 남았다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다. guest 설치는 baseline `0.42.86` MSI라 같은 version 재설치(A)는 이 bucket이 증명하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
