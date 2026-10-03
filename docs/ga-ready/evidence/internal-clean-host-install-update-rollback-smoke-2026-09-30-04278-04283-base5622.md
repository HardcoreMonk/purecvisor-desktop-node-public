# Clean-host install/update/rollback `0.42.78-admin-smoke` -> `0.42.83-admin-smoke` (base `20348.5622`)

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-09-30-04278-04283-base5622`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20260930-04278-04283-base5622`
summary_sha256: `d0aef005f59a375d547aa726dd919751751a186146639463572b3b8365393381`
vm_name: `pcv-cleanhost-20260930-base5622`
base_vhd_file: `20348.5622-20260930.vhd`
base_vhd_sha256: `88caf8aae8e51b0756e674f26c175e7953250f4bf4c00004d48b1312546a521e`
base_vhd_evidence: `clean-host-base-vhd-offline-refresh-2026-09-30-5622`
baseline_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
update_package_sha256: `e9da97140ffe54cd609694474dfc6687ced26097222df2a65b397ee3605f2365`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
guest_reboot_performed: `false`
smoke_vm_removed_on_success: `true`
current_base_set: `20348.5622-20260930.vhd`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`(`538077e` 이후)다. 2026-09-30 `02:19:14Z`에 시작해 `02:21:28Z`에 끝났다(134초).

```text
-BaseVhdPath <image-cache>/windows-server-2022-eval-vhd/20348.5622-20260930.vhd
-BaselineMsiPath artifacts/admin-smoke-package-20260925-04278/PureCVisorDesktopNode-0.42.78-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20260929-04283/PureCVisorDesktopNode-0.42.83-admin-smoke-update.zip
-VmName pcv-cleanhost-20260930-base5622 -BaselineVersion 0.42.78-admin-smoke -TargetVersion 0.42.83-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

입력 package pair와 옵션은 0.42.83 run(`internal-clean-host-install-update-rollback-smoke-2026-09-29-04278-04283`)과 같다. 다른 것은 base VHD뿐이다. guest 비밀번호는 runner 기본값을 썼고 기록하지 않았다(`password_recorded=false`).

| 단계 | 결과 |
| --- | --- |
| summary `ok` | `true` |
| `internal_clean_host_install_update_rollback_smoke` | `pass` |
| `base_vhd_source` / `base_vhd_ubr` / `base_vhd_kb` | `explicit` / `5622` / `KB5122882` |
| PowerShell Direct | `ok`, 2회 시도, 자동 복구 없음(`automatic_recovery_performed=false`) |
| `pre_update_os.ubr` | `5622`(base UBR과 같음) |
| Windows Update | `update_count=0`, 재부팅 없음(`reboot_performed=false`), `post_update_os.ubr=5622` |
| install / catalog update / rollback exit | `0` / `0` / `0` |
| baseline / updated / final manifest | `0.42.78-admin-smoke` / `0.42.83-admin-smoke` / `0.42.78-admin-smoke` |
| final Web Console | HTTP `200` |
| final service | `PureCVisorDesktopNode` Running / Auto |
| rollback 후 failed root | 있음 |
| blocker | `none` |

Windows Update 검색은 제목 패턴에 맞지 않는 항목 4개를 건너뛰었다. 이 동작은 0.42.83 run과 같다. 4개는 .NET Framework 누적/미리보기, MSRT, Defender 정의다.

성공 후 runner가 전용 VM과 differencing 디스크를 지웠다. Hyper-V가 만든 빈 VM 폴더 구조(`Snapshots`, `Virtual Machines`)는 `%ProgramData%\PureCVisor\desktop-node\clean-host-vms\pcv-cleanhost-20260930-base5622` 아래에 남았다. 이전 run들의 폴더도 같은 위치에 남아 있다. 보존 VM `pcv-guest-installed-04253-r1`은 Off 그대로다.

## 0.42.83 run과 비교

| 항목 | 0.42.83 run(base `20348.169`) | 이 run(base `20348.5622`) |
| --- | --- | --- |
| 전체 시간 | 46분(2766초) | 134초 |
| `pre_update_os.ubr` | `169` | `5622` |
| 설치한 LCU | `KB5122882` 1개 | 없음(`update_count=0`) |
| guest 재부팅 | 있음 | 없음 |
| PowerShell Direct 시도 | 26회, 무응답 자동 복구 1회 | 2회, 자동 복구 없음 |
| 제품 install/update/rollback | exit `0` | exit `0` |

설계 문서 §4의 첫 갱신 뒤 확인 조건을 모두 만족했다.
- `pre_update_os.ubr`가 새 base UBR과 같다.
- `update_count`가 `0`이다.
- 재부팅과 무응답 복구가 없다.
- 전체 시간을 비교했다.

"최신 LCU가 적용된 clean host에서 install/update/rollback"이라는 기존 주장도 유지된다. guest는 2026-09 LCU(UBR `5622`)가 적용된 상태에서 시작했고, 이 run 시점에 제목 패턴에 맞는 새 LCU는 없었다.

## current base 지정

PASS 뒤 `New-PcvCleanHostBaseVhd.ps1 -SetCurrentBasePath … -Execute`로 `current-base.json`을 썼다(`pcv-clean-host-current-base-v1`).
- `base_file=20348.5622-20260930.vhd`
- `previous_base_file=null`
- `prune_candidates=[]`

지정 전에 도구가 base SHA-256을 sidecar와 다시 대조했다. 이제 runner를 `-BaseVhdPath` 없이 실행하면 이 base를 쓴다(`base_vhd_source=current-base`). pair orchestrator처럼 `-BaseVhdPath`를 명시하는 호출은 명시한 경로를 그대로 쓴다.

## Nonclaims

- 이 기록은 전용 clean-host의 MSI 설치, catalog update, rollback만 증명한다.
- operational current(`0.42.83-admin-smoke`)와 `docs/ga-ready/current-evidence.json`을 바꾸지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
