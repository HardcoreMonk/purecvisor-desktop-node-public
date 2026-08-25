# Full Admin Host Mutation Gate Evidence - 2026-05-06 0.38.0

evidence_id: full-admin-host-mutation-gate-2026-05-06-0380
created_at: 2026-05-06T00:16:43+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260506-001432-0380
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260506-001432-0380
version: 0.38.0-admin-smoke
source_commit_sha: 267fe6afa0480ebc3b03431490bc37fa251261ae
msi_sha256: b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9
signing_mode: AllowUnsignedDev
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public trusted signing: excluded
external stable publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass
batch_retry_status: retry-not-needed

## 범위

사용자 opt-in 범위에서 Batch Supervisor full admin host mutation gate를 실행했다. 이 gate는 Service/MSI/Hyper-V route parity와 firewall, LAN, Event Log, ADR-0003 internal trust-store OS mutation gate를 같은 batch manifest에서 실행했다.

이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-001432-0380`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Heartbeat lines: `30`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `final_attempt=1`, `duration_ms=120322`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `final_attempt=1`, `duration_ms=10021`

## Service, MSI, Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380`
- MSI: `PureCVisorDesktopNode-0.38.0-admin-smoke-windows-x64.msi`
- MSI SHA-256: `b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9`
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

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260506-001432-0380`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source absent
- Firewall: owned rule enable pass 후 remove pass, final rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/` runtime policy, `/`, `/index.html`, `/app.js` all HTTP `200`
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Boot time unchanged: pass

## 이전 0.37.0 대비

`0.37.0-admin-smoke`에서 관측한 first-attempt MSI repair `1603` recovered transient는 이번 `0.38.0-admin-smoke`에서 재발하지 않았다. Batch Supervisor retry contract는 route parity step에 `retry_count=1`을 유지했지만 실제 실행은 `attempt_count=1`로 완료했다.

## 판정

`0.38.0-admin-smoke` full admin host mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
