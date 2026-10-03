# Internal clean-host install/update/rollback `0.42.84-admin-smoke` -> `0.42.85-admin-smoke` (2026-10-01)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-01-04284-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20261001-04284-04285`
summary_sha256: `d8b2cde5df0d53f227cfb4494a111333ac9f527474cce6e6e8ae637490630d8d`
vm_name: `pcv-cleanhost-20261001-04285`
base_vhd_source: `current-base`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87`
update_package_sha256: `74c8eeb29e3b927fc2f447fa7bea40879ebf8ce84ee6f723c12f429799cca9c4`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`다. `2026-09-30T15:35:32Z`(KST 10-01 00:35)에 시작해 `15:37:46Z`에 끝났다(약 134초, exit `0`).

```text
-ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20261001-04284-04285
-BaselineMsiPath artifacts/admin-smoke-package-20260930-04284/PureCVisorDesktopNode-0.42.84-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261001-04285/PureCVisorDesktopNode-0.42.85-admin-smoke-update.zip
-VmName pcv-cleanhost-20261001-04285 -BaselineVersion 0.42.84-admin-smoke -TargetVersion 0.42.85-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

base는 `current-base.json`이 골랐다(UBR `5622`). guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`, 자동 복구 없음) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.84-admin-smoke`, Web `200` |
| catalog update | exit `0`, manifest `0.42.85-admin-smoke`, Web `200` |
| rollback | exit `0`, manifest `0.42.84-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.85-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다. 이 호스트의 설치본은 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
