# Internal clean-host install/update/rollback `0.43.6-admin-smoke` -> `0.43.7-admin-smoke` (2026-11-11)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-11-11-04306-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/clean-host-windows-update`
summary_sha256: `c898e14ceb523ff4e1335fb041c2f3f923d4e4049de8f2c3fb6877b6650d79db`
vm_name: `pcv-manual-admin-campaign-20261111-04306-04307-r2`
base_vhd_source: `explicit`
base_vhd_file: `20348.5761-20261108.vhd`
baseline_msi_sha256: `59106d65ad70f57ab4cb3d9bd5b5f7cf6dde9f333af76d66ce44d9ba00dd98bd`
update_package_sha256: `fcc5182ac1a2e44b8ea09a5e462f1b7c6ec2b2cba4a04c9fb10b43774dbafe7d`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.7` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 887초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261111-04306-04307-r2/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261111-04306/PureCVisorDesktopNode-0.43.6-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261111-04307/PureCVisorDesktopNode-0.43.7-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261111-04306-04307-r2 -BaselineVersion 0.43.6-admin-smoke -TargetVersion 0.43.7-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261111-04306-04307-r2/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5761-20261108.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5761`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `0`개, 재부팅 없음 |
| baseline MSI 설치 | exit `0`, manifest `0.43.6-admin-smoke` |
| catalog update | exit `0`, manifest `0.43.7-admin-smoke` |
| rollback | exit `0`, manifest `0.43.6-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.43.7-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.43.7-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
