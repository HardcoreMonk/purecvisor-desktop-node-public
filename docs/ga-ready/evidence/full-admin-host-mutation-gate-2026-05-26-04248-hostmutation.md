# Full admin host mutation gate 2026-05-26 0.42.48

evidence_id: `full-admin-host-mutation-gate-2026-05-26-04248-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.48-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260526-04248`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04248`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04248`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260526-04248`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04247-04248-closed`
operational_full_gate_msi_sha256: `a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf`
operational_full_gate_payload_aggregate_sha256: `2a14e47bf3fd48b17755ce901ec02b924ba9246ecbe91414f952428ca376d92f`
clean_package_msi_sha256: `a0014960979ed23cec8d882cddd22baaaf9435a71287bdc133a79ff0b381338c`
provenance_commit: `46e745efc698a06e4b065a19c3f07217e821155e`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

`full-admin-host-mutation-gate-20260526-04248`는 Phase 3 Web/TUI QoS direct control payload를
전체 관리자 host mutation gate로 승격한 evidence다. Batch Supervisor는
`service-msi-hyperv-admin-smoke`와 `os-mutation-gate` 두 step을 실행했고 둘 다 exit `0`으로
완료했다.

## 실행 요약

| 항목 | 결과 |
| --- | --- |
| batch status | `completed`, `ok=true`, `executed_steps=2` |
| service/MSI/Hyper-V step | exit `0`, duration `193895 ms`, GPU snapshot `31`, retry count `1` |
| OS mutation gate step | exit `0`, duration `11084 ms`, GPU snapshot `1` |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04248` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260526-04248` |
| final service | `PureCVisorDesktopNode`, `Running`, `Auto` |
| remaining PureCVisor VM | `[]` |
| boot time | unchanged |

## 확인된 경계

Route parity smoke는 MSI build, service-action smoke, MSI install/repair/remove lifecycle,
설치본 Hyper-V API route smoke를 통과했다. OS mutation gate는 config migration,
firewall enable/remove, LAN listener IP smoke, Event Log register/remove, internal
trust-store install/remove/restore를 통과했다.

최종 host state는 service `Running`, firewall rule count `0`, Event Log source absent,
internal Root/TrustedPublisher certificate restored 상태다. Installed listener의
`ops summary` release card는 `0.42.48-admin-smoke`,
`msi_sha256=a573c716caa6246536e141af8f839eab093df551aeaf80d06589d05de6248edf`,
`git_commit=46e745efc698a06e4b065a19c3f07217e821155e`를 노출했다.

이 evidence는 internal admin-smoke host mutation evidence이며 public trusted signing 또는
외부 stable publication evidence가 아니다. `0.42.47-admin-smoke ->
0.42.48-admin-smoke` manual-admin package-pair closure는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`에서 PASS로 닫혔다.
