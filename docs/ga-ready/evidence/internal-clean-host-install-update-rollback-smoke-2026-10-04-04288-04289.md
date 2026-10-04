# Internal clean-host install/update/rollback `0.42.88-admin-smoke` -> `0.42.89-admin-smoke` (2026-10-04)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-04-04288-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20261004-04288-04289`
summary_sha256: `e16eea3d1d47efca7de09b078a77882bfac09641e5e511c56929607924108714`
vm_name: `pcv-cleanhost-20261004-04289`
base_vhd_source: `current-base`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `81ef85273cb9bbf9d813d4c3cce40f88c22e5d596c40906dab8766e4cab79b64`
update_package_sha256: `b852641609601f77cccf015969e3135212f35562e5dc694dd690fb59ca505f71`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.89` pair의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 exit `0`, 약 156초다.

```text
-ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20261004-04288-04289
-BaselineMsiPath artifacts/admin-smoke-package-20261003-04288/PureCVisorDesktopNode-0.42.88-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261004-04289/PureCVisorDesktopNode-0.42.89-admin-smoke-update.zip
-VmName pcv-cleanhost-20261004-04289 -BaselineVersion 0.42.88-admin-smoke -TargetVersion 0.42.89-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

base는 `current-base.json`이 골랐다(UBR `5622`). guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.88-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.89-admin-smoke` |
| rollback | exit `0`, manifest `0.42.88-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.89-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.88-admin-smoke`로 남았다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
