# Full admin host mutation 게이트 2026-05-20 0.42.39

evidence_id: `full-admin-host-mutation-gate-2026-05-20-04239-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.39-admin-smoke`
manual_admin_latest_closed_package_pair: `0.42.38-admin-smoke -> 0.42.39-admin-smoke`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04238-04239-closed`
batch_id: `full-admin-host-mutation-gate-20260520-04239`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04239`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04239`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260520-04239`
host_mutation_performed: `true`
full_gate_msi_sha256: `8ccf24a0a304b82dfcb0039c92149806539cf74977014bc3468c589e4ddf624f`
payload_aggregate_sha256: `cd2d820c66e6f28df8a740207c7182ab744d5d984fc3bfc6a009a35da95c0869`
provenance_commit: `6fd931baf3de77435d0d11b92424cf6657ea4515`
build_utc: `2026-05-20T13:34:42.0857967Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.39-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## PASS 버킷

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | step `service-msi-hyperv-admin-smoke`, exit `0`, duration `145069 ms` |
| OS mutation gate | `pass` | step `os-mutation-gate`, exit `0`, duration `11073 ms` |
| Batch supervisor | `pass` | total `2`, executed `2`, failed step 없음 |
| Package | `pass` | MSI SHA-256 `8ccf24a0a304b82dfcb0039c92149806539cf74977014bc3468c589e4ddf624f` |

## ADR-0007 Route 확인

Route parity smoke는 실제 Hyper-V VM에 대해 `vm.limit` mutation을 실행했고,
`vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status`, `vm.guest-ping` readback을
PASS로 기록했다. Readback payload는 Hyper-V semantics를 노출하며 Linux cgroup
QoS 또는 qemu guest agent 호환 claim은 하지 않는다.

## Host 상태

OS mutation gate는 firewall enable/remove, LAN listener smoke, Event Log register/remove,
internal trust-store install/remove/restore를 완료했다. 최종 상태는 service
`PureCVisorDesktopNode` `Running`/`Auto`, firewall final count `0`, Event Log source
absent, trust Root/TrustedPublisher present, boot time unchanged다.

## 경계

이 evidence는 관리자 opt-in host mutation evidence지만 internal admin-smoke 범위다.
Public trusted signing은 `excluded`, 외부 stable publication은 `not-claimed`다.
