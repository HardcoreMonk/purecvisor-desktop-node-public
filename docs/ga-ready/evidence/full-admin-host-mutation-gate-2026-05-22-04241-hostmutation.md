# Full admin host mutation gate 2026-05-22 0.42.41

evidence_id: `full-admin-host-mutation-gate-2026-05-22-04241-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.41-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260522-04241`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260522-04241`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260522-04241`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260522-04241`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-22-04241.md`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260522-04240-04241-closed`
operational_full_gate_msi_sha256: `e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb`
operational_full_gate_payload_aggregate_sha256: `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`
provenance_commit: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

`full-admin-host-mutation-gate-20260522-04241`는 `0.42.41-admin-smoke` 설치본 payload를
전체 관리자 host mutation gate로 재확인했다. Batch Supervisor는
`service-msi-hyperv-admin-smoke`와 `os-mutation-gate` 두 step을 실행했고 둘 다 exit `0`으로
완료했다.

## 실행 요약

| 항목 | 결과 |
| --- | --- |
| batch status | `completed`, `ok=true`, `executed_steps=2` |
| service/MSI/Hyper-V step | exit `0`, duration `200437 ms`, GPU snapshot `32` |
| OS mutation gate step | exit `0`, duration `11083 ms`, GPU snapshot `1` |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260522-04241` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260522-04241` |
| final service | `PureCVisorDesktopNode`, `Running`, `Auto` |
| remaining PureCVisor VM | `[]` |
| boot time | unchanged |

## 확인된 경계

Route parity smoke는 실제 Hyper-V VM과 설치본 service/API route를 사용한다. OS mutation
gate는 firewall, LAN listener, Event Log source, internal trust-store install/remove/restore
범위를 확인한다. 이 evidence는 internal admin-smoke host mutation evidence이며 public
trusted signing 또는 외부 stable publication evidence가 아니다.
