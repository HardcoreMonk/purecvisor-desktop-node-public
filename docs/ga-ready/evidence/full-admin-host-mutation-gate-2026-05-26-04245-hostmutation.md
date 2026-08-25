# Full admin host mutation gate 2026-05-26 0.42.45

evidence_id: `full-admin-host-mutation-gate-2026-05-26-04245-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.45-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260526-04245`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04245`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04245`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260526-04245`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04245.md`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04244-04245-closed`
operational_full_gate_msi_sha256: `379fc96a63d853deb3fb57fa44231479a3785a6f9ca58bf8c924d96410bc3246`
operational_full_gate_payload_aggregate_sha256: `d0568f69ac061815d06b1a41c819594da7cbb6c577dced2382945ae4502498a3`
provenance_commit: `76c77a86bbb72e415b1968169c16f1638b76fa56`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

`full-admin-host-mutation-gate-20260526-04245`는 `0.42.45-admin-smoke` 설치본 payload를
전체 관리자 host mutation gate로 승격한 current evidence다. Batch Supervisor는
`service-msi-hyperv-admin-smoke`와 `os-mutation-gate` 두 step을 실행했고 둘 다 exit `0`으로
완료했다.

## 실행 요약

| 항목 | 결과 |
| --- | --- |
| batch status | `completed`, `ok=true`, `executed_steps=2` |
| service/MSI/Hyper-V step | exit `0`, duration `91122 ms`, GPU snapshot `14` |
| OS mutation gate step | exit `0`, duration `11083 ms`, GPU snapshot `1` |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04245` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260526-04245` |
| final service | `PureCVisorDesktopNode`, `Running`, `Auto` |
| remaining PureCVisor VM | `[]` |
| boot time | unchanged |

## 확인된 경계

Route parity smoke는 MSI build, service-action smoke, MSI install/repair/remove lifecycle,
설치본 Hyper-V API route smoke를 통과했다. OS mutation gate는 config migration,
firewall enable/remove, LAN listener IP smoke, Event Log register/remove, internal
trust-store install/remove/restore를 통과했고 최종 firewall rule count는 `0`, Event Log
source는 absent, internal Root/TrustedPublisher certificate는 restore 상태로 확인됐다.

이 evidence는 internal admin-smoke host mutation evidence이며 public trusted signing 또는
외부 stable publication evidence가 아니다.
