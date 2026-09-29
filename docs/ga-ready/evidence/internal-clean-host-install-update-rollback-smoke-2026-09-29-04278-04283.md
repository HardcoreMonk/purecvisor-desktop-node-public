# Clean-host install/update/rollback `0.42.78-admin-smoke` -> `0.42.83-admin-smoke`

evidence_id: `internal-clean-host-install-update-rollback-smoke-2026-09-29-04278-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/internal-clean-host-install-update-rollback-smoke-20260929-04278-04283`
summary_sha256: `08277b8bb6553abf65eed91af89eedddd97f58a96585686ae93e0820e44d8dba`
vm_name: `pcv-cleanhost-20260929-04283`
baseline_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
update_package_sha256: `e9da97140ffe54cd609694474dfc6687ced26097222df2a65b397ee3605f2365`
target_provenance_commit: `bcda14f0f02c256acc125dafbd43da3b4a93b3d0`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
guest_reboot_performed: `true`
smoke_vm_removed_on_success: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 실행

runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`다. 2026-09-29 `13:21:49Z`에 시작해 `14:08:06Z`에 끝났다(46분). 사용자 승인으로 이 task의 경과 한도는 180분이었다.

```text
-BaselineMsiPath artifacts/admin-smoke-package-20260925-04278/PureCVisorDesktopNode-0.42.78-admin-smoke-windows-x64.msi
-UpdatePackagePath artifacts/admin-smoke-package-20260929-04283/PureCVisorDesktopNode-0.42.83-admin-smoke-update.zip
-VmName pcv-cleanhost-20260929-04283 -BaselineVersion 0.42.78-admin-smoke -TargetVersion 0.42.83-admin-smoke
-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess
```

guest 비밀번호는 runner 기본값을 썼고 command line과 summary에 남기지 않았다(`guest_password_recorded=false`).

update package는 `0.42.83` payload 폴더 내용 8개 항목의 ZIP이다. 0.42.78 ZIP과 같은 구조이고, `admin-smoke-package-20260929-04283` artifact root에 있다.

전용 Generation 1 VM을 Windows Server 2022 Evaluation 기준 VHD(`20348.169`)의 differencing 디스크로 만들고 `Default Switch`에 연결했다.

| 단계 | 결과 |
| --- | --- |
| summary `ok` | `true` |
| `internal_clean_host_install_update_rollback_smoke` | `pass` |
| Windows Update | `2026-09 Cumulative Update … (KB5122882)` 1개. UBR `169 → 5622` |
| 재부팅 뒤 PowerShell Direct | `ok`, 26회 시도, heartbeat 무응답 자동 복구 실행(`automatic_recovery_performed=true`) |
| catalog update exit | `0` |
| rollback exit | `0` |
| baseline manifest | `0.42.78-admin-smoke` |
| updated manifest | `0.42.83-admin-smoke` |
| final manifest | `0.42.78-admin-smoke` |
| final Web Console | HTTP `200` |
| final service | `PureCVisorDesktopNode` Running / Auto |
| rollback 후 failed root | 있음 |
| blocker | `none` |

성공 후 전용 VM, differencing 디스크, VM 폴더(`D:\PureCVisor\clean-host-vms\pcv-cleanhost-20260929-04283`)를 제거했다. 보존 VM `pcv-guest-installed-04253-r1`은 Off 그대로다.

## 관측: 대기 시간

실행 시간의 대부분은 오래된 base(`20348.169`, 2021-08)에 최신 LCU를 설치하고 재부팅하는 데 들었다. 재부팅 뒤 heartbeat 무응답 복구까지 기다린 시간도 여기에 들어간다. 다운로드 시간은 이보다 작다. 개선 방안(base VHD 오프라인 갱신)은 이 캠페인 범위 밖이며 별도로 제안한다.

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| Burn install/repair/remove | `not-run` |
| MSIX build/install/update/remove | `not-run` |
| manual-admin campaign descriptor | `not-generated` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `0.42.78-admin-smoke -> 0.42.83-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.78-admin-smoke` |

## Nonclaims

- 이 기록은 전용 clean-host의 MSI 설치, catalog update, rollback만 증명한다.
- operational current를 `0.42.83-admin-smoke`로 올리지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
