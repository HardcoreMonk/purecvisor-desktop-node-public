# Full admin host mutation gate 2026-05-25 0.42.44

evidence_id: `full-admin-host-mutation-gate-2026-05-25-04244-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.44-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260525-04244-r2`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260525-04244-r2`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260525-04244-r2`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260525-04244-r2`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04244.md`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260525-04243-04244-closed`
operational_full_gate_msi_sha256: `bd1f45b62c683571fe238d8b570642d4f5818bd0b3f3c2e8d9a587841028e701`
operational_full_gate_payload_aggregate_sha256: `3bbac62cea3c1e6651367ca8f66bcc49633d398743445325abadc63a35192847`
provenance_commit: `c7c7b0c9d4ea0b0296bc3ba423beb8eb7ac865e2`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

`full-admin-host-mutation-gate-20260525-04244-r2`는 `0.42.44-admin-smoke` 설치본 payload를
전체 관리자 host mutation gate로 승격한 current evidence다. Batch Supervisor는
`service-msi-hyperv-admin-smoke`와 `os-mutation-gate` 두 step을 실행했고 둘 다 exit `0`으로
완료했다.

초기 `full-admin-host-mutation-gate-20260525-04244` attempt는 MSI repair custom action에서
정지되어 summary를 만들지 못했다. 원인은 `repair-installed`가 이미 동일 binary path로
`Running`인 owned service에도 stop/configure/start를 반복하던 idempotence 결함이었다.
`c7c7b0c9d4ea0b0296bc3ba423beb8eb7ac865e2`가 동일 path/running service에서는 SCM mutation을
skip하도록 수정했고, `DesktopNodeHostServiceActionTests` focused test `102/102` PASS 후 r2
gate를 다시 실행했다.

## 실행 요약

| 항목 | 결과 |
| --- | --- |
| batch status | `completed`, `ok=true`, `executed_steps=2` |
| service/MSI/Hyper-V step | exit `0`, duration `97209 ms`, GPU snapshot `15` |
| OS mutation gate step | exit `0`, duration `11113 ms`, GPU snapshot `1` |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260525-04244-r2` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260525-04244-r2` |
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
