# Full Admin Host Mutation Gate Evidence - 2026-05-05 0.37.0

evidence_id: full-admin-host-mutation-gate-2026-05-05-0370
created_at: 2026-05-05T23:26:05+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260505-231654-0370
version: 0.37.0-admin-smoke
source_commit_sha: 485b1a7338fb2b682c3964c858ccc13c322950d7
msi_sha256: f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95
signing_mode: AllowUnsignedDev
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public trusted signing: excluded
external stable publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass
transient_recovery_status: pass-with-resume

## 범위

사용자 opt-in 범위에서 Batch Supervisor full admin host mutation gate를 실행했다. 이 gate는 Service/MSI/Hyper-V route parity와 firewall, LAN, Event Log, ADR-0003 internal trust-store OS mutation gate를 같은 batch manifest에서 실행했다.

이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `duration_ms=60127`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `duration_ms=10029`

## Service, MSI, Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`
- MSI: `PureCVisorDesktopNode-0.37.0-admin-smoke-windows-x64.msi`
- MSI SHA-256: `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`
- MSI signing mode: `AllowUnsignedDev`
- MSI lifecycle: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`
- Service-action smoke: pass
- Installed Hyper-V API route smoke: pass
- Host status route: pass
- Network inventory route: pass
- VM lifecycle routes: create, start, restart, poweroff, delete pass
- Checkpoint routes: create, restore, delete pass
- Expected structured failure: installer ISO `vm.shutdown` returned `PCV_VM_SHUTDOWN_NOT_AVAILABLE`
- Delete guard: managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete blocked with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`
- Final proof: service `Running`, startup `Automatic`, boot time unchanged, `remaining_pcv_vms=[]`

## Firewall, LAN, Event Log, Trust Store 결과

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source absent
- Firewall: owned rule enable pass 후 remove pass, final rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/` smoke pass
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Boot time unchanged: pass

## Transient MSI Repair Disposition

첫 batch 실행은 `service-msi-hyperv-admin-smoke`의 MSI repair 단계에서 `PCV_SMOKE_MSI_STEP_FAILED|repair exited 1603.`으로 실패했다. 실패 직후 서비스는 수동 복구로 `Running` 상태와 loopback Web root `HTTP 200`을 회복했다.

동일한 `DesktopNode.Host.exe service-action repair-installed` 직접 실행은 exit `0`이었다. 같은 MSI의 manual repair도 exit `0`이었다. 이후 Batch Supervisor `-Resume`으로 동일 manifest를 재개했고 Service/MSI/Hyper-V step과 OS mutation gate step 모두 exit `0`으로 완료했다.

따라서 최종 `0.37.0-admin-smoke` gate 판정은 pass이며, 최초 repair `1603`은 recovered transient evidence로 기록한다. 이 transient는 다음 hardening batch에서 retry/backoff와 partial evidence persistence 대상으로 다룬다.

## 판정

`0.37.0-admin-smoke` full admin host mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
