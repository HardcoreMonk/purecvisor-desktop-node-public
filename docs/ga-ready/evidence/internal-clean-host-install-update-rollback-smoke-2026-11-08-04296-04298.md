# Internal clean-host install/update/rollback `0.42.96-admin-smoke` -> `0.42.98-admin-smoke` (2026-11-08)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-11-08-04296-04298`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261108-04296-04298/clean-host-windows-update`
summary_sha256: `3246b64d154a27b054f6ed6023122f03f83ad6a22790987d303b4a86bbef84e1`
vm_name: `pcv-manual-admin-campaign-20261108-04296-04298`
base_vhd_source: `explicit`
base_vhd_file: `20348.5761-20261108.vhd`
baseline_msi_sha256: `3fcdada1cc5f390e8398ce424ba8ee6fb051ede845044c6a5b6bc6f04a021933`
update_package_sha256: `c18bc3de426cc031cff4acc939038bd045513e4e401c8b61dbd3adb371777494`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.98` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 143초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261108-04296-04298/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261108-04296r/PureCVisorDesktopNode-0.42.96-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261108-04298/PureCVisorDesktopNode-0.42.98-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261108-04296-04298 -BaselineVersion 0.42.96-admin-smoke -TargetVersion 0.42.98-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261108-04296-04298/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5761-20261108.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5761`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.96-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.98-admin-smoke` |
| rollback | exit `0`, manifest `0.42.96-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.98-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.98-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
