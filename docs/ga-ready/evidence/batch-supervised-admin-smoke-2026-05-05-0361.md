# Batch-Supervised Admin Smoke Evidence - 2026-05-05 0.36.1

evidence_id: batch-supervised-admin-smoke-2026-05-05-0361
created_at: 2026-05-05T20:12:18+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361
version: 0.36.1-admin-smoke
msi_provenance_commit_sha: 2a080d80a3394218aee6e1f68fc64cf9f347bf86
msi_sha256: 6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307
signing_mode: AllowUnsignedDev
public_trusted_signing: excluded
external_stable_publication: not-claimed
latest_os_mutation_gate: 0.35.7-admin-smoke
latest_os_mutation_gate_artifact_root: artifacts/os-mutation-gates-20260505-180434-0357-rerun
machine_readable_json_created: no

## 범위

이 evidence는 Batch Supervisor가 감싼 Service/MSI/Hyper-V route parity admin smoke다. Firewall, LAN bearer exposure, Event Log source register/remove, ADR-0003 internal trust-store install/remove/restore OS gate는 이 실행에서 rerun하지 않았다. 최신 OS mutation gate는 계속 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다.

MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`이다. Batch Supervisor tooling과 docs 변경은 repo-local development runner/evidence closure 변경이며 MSI payload provenance로 해석하지 않는다.

## Batch Supervisor 결과

- Summary: `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`
- Step: `ok=true`, `timed_out=false`, `exit_code=0`, `duration_ms=115258`
- Heartbeat lines: `25`
- Artifact: `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`

## Service/MSI/Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`
- Installed DisplayVersion: `0.36.1`
- Final service: `PureCVisorDesktopNode` `Running`, startup `Automatic`
- Boot time: unchanged
- Remaining PureCVisor smoke VMs: `[]`
- MSI payload file count: `7`
- Host status route: pass
- Network inventory route: pass
- VM lifecycle routes: create/start/restart/poweroff/delete pass
- Checkpoint routes: create/restore/delete pass
- Expected structured failure: installer ISO `vm.shutdown` returned `PCV_VM_SHUTDOWN_NOT_AVAILABLE`
- Delete guard: managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete blocked with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`

## 판정

`0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity evidence는 pass다. 이 pass는 `AllowUnsignedDev` 내부 관리자 opt-in smoke evidence이며 public trusted signing, external stable publication, firewall/LAN/Event Log/trust-store OS mutation gate evidence가 아니다.
