# Internal clean-host install/update/rollback `0.42.98-admin-smoke` -> `0.42.99-admin-smoke` (2026-11-09)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-11-09-04298-04299`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261109-04298-04299/clean-host-windows-update`
summary_sha256: `b6a047d7fe36591c45c4f610a23a65b5876cf26f5d8845db797cf7d2d50d90b0`
vm_name: `pcv-manual-admin-campaign-20261109-04298-04299`
base_vhd_source: `explicit`
base_vhd_file: `20348.5761-20261108.vhd`
baseline_msi_sha256: `9a26d4cee9c4360cb8a04ce520b57de4d2e99c3e7b64997d8b4567e7c04733b0`
update_package_sha256: `aec1eb05f07c64e31e8ba4f8cdcd55f3c220af621055a6193a84e17d51498e84`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.99` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 147초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261109-04298-04299/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261108-04298/PureCVisorDesktopNode-0.42.98-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261109-04299/PureCVisorDesktopNode-0.42.99-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261109-04298-04299 -BaselineVersion 0.42.98-admin-smoke -TargetVersion 0.42.99-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261109-04298-04299/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5761-20261108.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5761`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.98-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.99-admin-smoke` |
| rollback | exit `0`, manifest `0.42.98-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.99-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.99-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
