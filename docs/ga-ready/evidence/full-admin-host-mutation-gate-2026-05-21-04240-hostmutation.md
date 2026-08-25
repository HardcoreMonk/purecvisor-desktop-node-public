# Full admin host mutation 게이트 2026-05-21 0.42.40

evidence_id: `full-admin-host-mutation-gate-2026-05-21-04240-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.40-admin-smoke`
manual_admin_latest_closed_package_pair: `0.42.39-admin-smoke -> 0.42.40-admin-smoke`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260521-04239-04240-closed`
batch_id: `full-admin-host-mutation-gate-20260521-04240`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260521-04240`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260521-04240`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260521-04240`
host_mutation_performed: `true`
full_gate_msi_sha256: `eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41`
payload_aggregate_sha256: `cd49f061dfd0e2e5afe45cd34befcfb28e02bbd9038eff1fbaef34f8c9616ea5`
provenance_commit: `adb7b8c77ff60b64c5ac4d840e2bdfac62a3793a`
build_utc: `2026-05-21T12:07:07.4648275Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.40-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## PASS 버킷

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | step `service-msi-hyperv-admin-smoke`, exit `0`, duration `163571 ms` |
| OS mutation gate | `pass` | step `os-mutation-gate`, exit `0`, duration `11068 ms` |
| Batch supervisor | `pass` | total `2`, executed `2`, failed step 없음 |
| Package | `pass` | MSI SHA-256 `eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41` |

## Operator Surface 확인

Route parity smoke는 실제 Hyper-V VM과 설치본 service/API route를 사용한다. 이번 04240
package는 Web/TUI QoS/guest readback product payload를 포함하므로, full gate package의
payload aggregate SHA-256 `cd49f061dfd0e2e5afe45cd34befcfb28e02bbd9038eff1fbaef34f8c9616ea5`를
04240 operational anchor로 둔다.

## Host 상태

OS mutation gate는 firewall enable/remove, LAN listener smoke, Event Log register/remove,
internal trust-store install/remove/restore를 완료했다. 최종 상태는 service
`PureCVisorDesktopNode` `Running`/`Auto`, firewall final count `0`, Event Log source
absent, trust Root/TrustedPublisher present, boot time unchanged다.

## 경계

이 evidence는 관리자 opt-in host mutation evidence지만 internal admin-smoke 범위다.
Public trusted signing은 `excluded`, 외부 stable publication은 `not-claimed`다.
