# Internal clean-host install/update/rollback `0.42.87-admin-smoke` -> `0.42.88-admin-smoke` (2026-10-03)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-03-04287-04288`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20261003-04287-04288`
summary_sha256: `d982dd74be31e8a579907c703e796324e364a00cff2a9d954ae882203f4bde31`
vm_name: `pcv-cleanhost-20261003-04288`
base_vhd_source: `current-base`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `a0041c9f70c6e9003950bb672c74be4d41a8ad2e06fe690481d63266f0dbbcc1`
update_package_sha256: `7051e735aff0466925ec1912103dce57dad42f6f67c452cfca4026de244cb751`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`다. exit `0`, 약 137초다.

```text
-ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20261003-04287-04288
-BaselineMsiPath artifacts/admin-smoke-package-20261003-04287/PureCVisorDesktopNode-0.42.87-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261003-04288/PureCVisorDesktopNode-0.42.88-admin-smoke-update.zip
-VmName pcv-cleanhost-20261003-04288 -BaselineVersion 0.42.87-admin-smoke -TargetVersion 0.42.88-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

base는 `current-base.json`이 골랐다(UBR `5622`). guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.87-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.88-admin-smoke` |
| rollback | exit `0`, manifest `0.42.87-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.88-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.87-admin-smoke`로 남았다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
