# Internal clean-host install/update/rollback `0.43.3-admin-smoke` -> `0.43.4-admin-smoke` (2026-11-11)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-11-11-04303-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04303-04304/clean-host-windows-update`
summary_sha256: `acff399c04df35d2369feef8789bb9dabeb1fa47ec6b05f4898f883374e392be`
vm_name: `pcv-manual-admin-campaign-20261111-04303-04304`
base_vhd_source: `explicit`
base_vhd_file: `20348.5761-20261108.vhd`
baseline_msi_sha256: `66546521fb0fea092de8d0fd390153cc337108d9c17d5622015b748d901de060`
update_package_sha256: `92630abe393ea54fa3acb0c8a775e03d18fc8c27d0aeb1c4d8b1bc81f3cdb2cc`
host_mutation_performed: `true`
guest_product_mutation_performed: `true`
token_value_observed: `false`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

release train `0.43.4` pair orchestrator의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 1122초 걸렸다.

```text
-ArtifactRoot artifacts/manual-admin-campaign-20261111-04303-04304/clean-host-windows-update
-BaselineMsiPath artifacts/admin-smoke-package-20261111-04303/PureCVisorDesktopNode-0.43.3-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20261111-04304/PureCVisorDesktopNode-0.43.4-admin-smoke-update.zip
-VmName pcv-manual-admin-campaign-20261111-04303-04304 -BaselineVersion 0.43.3-admin-smoke -TargetVersion 0.43.4-admin-smoke
-VmRoot artifacts/manual-admin-campaign-20261111-04303-04304/clean-host-windows-update/vm -BaseVhdPath <image-cache>/20348.5761-20261108.vhd -VMSwitchName Default Switch
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure
```

base VHD는 pair orchestrator가 `-BaseVhdPath`로 넘겼다(UBR `5761`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.

## 결과

| 단계 | 결과 |
| --- | --- |
| PowerShell Direct | `ok` (시도 `2`) |
| Windows Update | 대상 `1`개, 재부팅 있음 |
| baseline MSI 설치 | exit `0`, manifest `0.43.3-admin-smoke` |
| catalog update | exit `0`, manifest `0.43.4-admin-smoke` |
| rollback | exit `0`, manifest `0.43.3-admin-smoke`, service Running/Auto, Web `200` |
| failed root | 있음(`0.43.4-admin-smoke`) |
| VM 정리 | 삭제됨 |
| blocker | `none` |

이 호스트의 설치본은 `0.43.4-admin-smoke`로 남았다. VM은 `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off다.

## Nonclaims

- 전용 clean-host VM 안의 내부 smoke다.
- public trusted signing과 external stable publication을 주장하지 않는다.
