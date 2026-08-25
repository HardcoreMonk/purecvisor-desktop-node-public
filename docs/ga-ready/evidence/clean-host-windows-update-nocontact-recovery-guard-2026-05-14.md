# Clean-host Windows Update NoContact Recovery Guard - 2026-05-14

```text
evidence_id: clean-host-windows-update-nocontact-recovery-guard-2026-05-14
scope: internal-clean-host-runner-windows-update-reboot-recovery-guard
result: CODE_LEVEL_PASS
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed
```

이 evidence는 `Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`의 Windows
Update reboot 대기 경로를 code-level로 보강한 기록이다. 2026-05-14
`0.42.11-admin-smoke -> 0.42.12-admin-smoke` manual-admin campaign에서 Windows
Update reboot 이후 dedicated VM heartbeat가 `NoContact`, CPU가 idle 상태로 장시간
멈춰 operator가 `Stop-VM -TurnOff -Force; Start-VM`을 수동 수행했다. 해당 사건은
`artifacts/manual-admin-campaign-20260514-04211-04212/clean-host-updated-os/manual-forced-restart-after-windows-update-hang.json`
에 보존한다.

이번 변경은 그 수동 복구를 runner contract로 승격한다. 실제 clean-host VM,
Windows Update, MSI install/update/rollback, host mutation은 이 evidence에서
실행하지 않았다.

## 적용 내용

- `Wait-PcvPowerShellDirect`가 Hyper-V VM 상태 snapshot을 같이 남긴다.
- `Get-PcvVmRecoverySnapshot`은 `state`, `heartbeat`, `cpu_usage`, `uptime_seconds`,
  `memory_assigned`를 기록한다.
- `Test-PcvNoContactIdleVm`은 VM이 `Running`, heartbeat `NoContact`, CPU `0-1`인
  상태를 recovery 후보로 판정한다.
- Windows Update reboot 이후 PowerShell Direct가 돌아오지 않고
  `NoContact` + idle 상태가 `WindowsUpdateNoContactRecoverySeconds` 이상 지속되면
  runner가 한 번만 `Stop-VM -TurnOff -Force; Start-VM`을 수행한다.
- recovery action은 `post-windows-update-heartbeat-no-contact-cpu-idle` reason,
  threshold, before/after VM snapshot, `automatic_recovery_performed`,
  `recovery_actions`로 summary에 남긴다.
- 기존 timeout fallback은 유지하되, timeout 이후 forced restart는
  `timeout_forced_restart_performed`와
  `timeout_forced_restart_reason=post-reboot-powershell-direct-timeout`로 구분한다.

## Runner 옵션

| 옵션 | 기본값 | 의미 |
| --- | --- | --- |
| `-WindowsUpdateNoContactRecoverySeconds` | `900` | Windows Update reboot 후 `NoContact` + idle 상태가 지속되어야 하는 시간 |
| `-DisableWindowsUpdateNoContactRecovery` | off | 자동 recovery guard를 끈다 |
| `-WindowsUpdateRebootTimeoutSeconds` | `1800` | 전체 PowerShell Direct reboot 복구 대기 시간 |

`clean-host-runner-plan.json`에는
`windows_update_no_contact_recovery_enabled`와
`windows_update_no_contact_recovery_seconds`를 기록한다.

## 운영 경계

- 이 변경은 repository code-level hardening이다.
- manual-admin clean-host campaign 실행은 여전히 `MANUAL-ADMIN`이며 elevated operator
  opt-in이 필요하다.
- 다음 product payload 변경이 생기면 `0.42.12-admin-smoke -> 0.42.13-admin-smoke`
  package-pair 후보에서 이 runner guard가 적용된다.
- Public trusted signing, winget, external stable publication, public clean-host release
  claim은 추가하지 않는다.
