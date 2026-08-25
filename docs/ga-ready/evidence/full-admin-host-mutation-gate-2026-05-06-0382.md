# Full Admin Host Mutation Gate Evidence - 2026-05-06 0.38.2

evidence_id: full-admin-host-mutation-gate-2026-05-06-0382
created_at: 2026-05-06T14:57:18+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260506-145506-0382
version: 0.38.2-admin-smoke
source_commit_sha: d05d395e96d5d8d83b4cc4310c2b8ef11253041c
msi_sha256: 4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252
signing_mode: AllowUnsignedDev
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public trusted signing: excluded
external stable publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass
batch_retry_status: retry-not-needed

## 범위

사용자 opt-in 범위에서 Batch Supervisor full admin host mutation gate를 재실행했다. 이 gate는 Service/MSI/Hyper-V route parity와 firewall, LAN, Event Log, ADR-0003 internal trust-store OS mutation gate를 같은 batch manifest에서 실행했다.

이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Heartbeat lines: `24`
- GPU snapshot lines: `18`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `final_attempt=1`, `duration_ms=108824`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `final_attempt=1`, `duration_ms=11071`

## Service, MSI, Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382`
- MSI: `PureCVisorDesktopNode-0.38.2-admin-smoke-windows-x64.msi`
- MSI SHA-256: `4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252`
- MSI provenance commit: `d05d395e96d5d8d83b4cc4310c2b8ef11253041c`
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
- Final proof: service `Running`, startup `Automatic`, boot time unchanged, `pcv-spike-*` VM count `0`

## Firewall, LAN, Event Log, Trust Store 결과

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260506-145506-0382`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source absent
- Firewall: owned rule enable pass 후 remove pass, final rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/` runtime policy, `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js` all HTTP `200`
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Boot time unchanged: pass

## 이전 0.38.1 대비

`0.38.1-admin-smoke` 이후 같은 full admin host mutation gate를 `0.38.2-admin-smoke` MSI로 재실행했다. Batch Supervisor retry contract는 route parity step에 `retry_count=1`을 유지했지만 실제 실행은 `attempt_count=1`로 완료했고 timeout은 발생하지 않았다.

## 판정

`0.38.2-admin-smoke` full admin host mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
