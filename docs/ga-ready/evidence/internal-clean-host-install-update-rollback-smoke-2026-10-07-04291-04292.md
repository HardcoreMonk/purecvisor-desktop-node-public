# Internal clean-host install/update/rollback `0.42.91-admin-smoke` -> `0.42.92-admin-smoke` (2026-10-07)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-07-04291-04292`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261007-04291-04292/clean-host-windows-update`
summary_sha256: `30a11b40ca064b50b6afc764c33badca9b616485a6fe335af5eca23b224b6e09`
vm_name: `pcv-manual-admin-campaign-20261007-04291-04292`
base_vhd_source: `explicit`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `bdef7609de3667298325d162641578a85e191ed31075cc6238e8e0f79fbfc12f`
update_package_sha256: `fcaf13dc3118371eab79013f203e5da2ba1e078401e6b2b331801a334ad57102`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.92` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 142초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261007-04291-04292/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261006-04291/PureCVisorDesktopNode-0.42.91-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261007-04292/PureCVisorDesktopNode-0.42.92-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261007-04291-04292 -BaselineVersion 0.42.91-admin-smoke -TargetVersion 0.42.92-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261007-04291-04292/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5622-20260930.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5622`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.91-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.92-admin-smoke` |
| rollback | exit `0`, manifest `0.42.91-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.92-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.92-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
