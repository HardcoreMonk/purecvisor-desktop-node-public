# Internal clean-host install/update/rollback `0.42.95-admin-smoke` -> `0.42.96-admin-smoke` (2026-10-11)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-10-11-04295-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261011-04295-04296/clean-host-windows-update`
summary_sha256: `ec6e9409a9873919d921d08bf608974b70dc72605a4fdd5be56f1420dbd21066`
vm_name: `pcv-manual-admin-campaign-20261011-04295-04296`
base_vhd_source: `explicit`
base_vhd_file: `20348.5622-20260930.vhd`
baseline_msi_sha256: `50680d3c757596e8fce85b3cbb41a7228e8a1ced244678d620fe912f371fab24`
update_package_sha256: `e4324e2420724dc4383b142debb600465dee84d415243183f138e602530c6a94`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.42.96` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 142초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261011-04295-04296/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261010-04295/PureCVisorDesktopNode-0.42.95-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261011-04296/PureCVisorDesktopNode-0.42.96-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261011-04295-04296 -BaselineVersion 0.42.95-admin-smoke -TargetVersion 0.42.96-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261011-04295-04296/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5622-20260930.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5622`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.42.95-admin-smoke` |
| catalog update | exit `0`, manifest `0.42.96-admin-smoke` |
| rollback | exit `0`, manifest `0.42.95-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.42.96-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.42.96-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
