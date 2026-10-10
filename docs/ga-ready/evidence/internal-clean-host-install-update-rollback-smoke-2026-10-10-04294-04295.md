# Internal clean-host install/update/rollback `0.42.94-admin-smoke` -> `0.42.95-admin-smoke` (2026-10-10)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-10-04294-04295`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261010-04294-04295/clean-host-windows-update`
summary_sha256: `d32042d15cdd81185692c8434a47d6da2ae929d999bb9ad0e5fdc2972770fc4f`
vm_name: `pcv-manual-admin-campaign-20261010-04294-04295`
base_vhd_source: `explicit`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `a9a4ec7700b84535b5ce7af27738560c575e76b031ebf1e5dab9685b73431096`
update_package_sha256: `32350c7422f365a8c15a4a5b1fb5e4cd6a05650e475230b6be49f62926d09887`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.95` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 166초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261010-04294-04295/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261010-04294/PureCVisorDesktopNode-0.42.94-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261010-04295/PureCVisorDesktopNode-0.42.95-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261010-04294-04295 -BaselineVersion 0.42.94-admin-smoke -TargetVersion 0.42.95-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261010-04294-04295/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5622-20260930.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5622`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.94-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.95-admin-smoke` |
| rollback | exit `0`, manifest `0.42.94-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.95-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.95-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
