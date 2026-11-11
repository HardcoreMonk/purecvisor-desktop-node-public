# Internal clean-host install/update/rollback `0.43.5-admin-smoke` -> `0.43.6-admin-smoke` (2026-11-11)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-11-11-04305-04306`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04305-04306/clean-host-windows-update`
summary_sha256: `d3b2d6a3e862ccc006c85c1610bd9afc253ec576da5f187bbd4c4afb02ac2c8b`
vm_name: `pcv-manual-admin-campaign-20261111-04305-04306`
base_vhd_source: `explicit`
base_vhd_file: `20348.5761-20261108.vhd`
baseline_msi_sha256: `5f4699097d267ef3bddda081d0351f5f14ae286803d1447ce1eeeba5ef727d8d`
update_package_sha256: `048ec1bb33b893fd1587cb60859c27215be9b7e65f15322b2c072a878c805f1e`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.6` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 2166초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261111-04305-04306/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261111-04305/PureCVisorDesktopNode-0.43.5-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261111-04306/PureCVisorDesktopNode-0.43.6-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261111-04305-04306 -BaselineVersion 0.43.5-admin-smoke -TargetVersion 0.43.6-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261111-04305-04306/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5761-20261108.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5761`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `1`개, 재부팅 있음 |
| baseline MSI 설치 | exit `0`, manifest `0.43.5-admin-smoke` |
| catalog update | exit `0`, manifest `0.43.6-admin-smoke` |
| rollback | exit `0`, manifest `0.43.5-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.43.6-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.43.6-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
