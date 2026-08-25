# Full admin host mutation gate 2026-05-28 0.42.57

evidence_id: `full-admin-host-mutation-gate-2026-05-28-04257-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate-public-boundary-current-evidence-rollup`
version: `0.42.57-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260528-04257`
batch_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04257`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260528-04257`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260528-04257`
full_gate_msi_sha256: `809eacb97a49aeaa32fc0ea3dce8ac5bdeb7c66b8b4502352519a338a512847e`
full_gate_payload_aggregate_sha256: `7a34468d3a59c2da182835a03f440f22df9e70f31ff062dc625530a9143ef94d`
clean_package_msi_sha256: `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`
provenance_commit: `16cc0d6b592d7f2f9ead14c41d8f4ad0e1f28b76`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Batch 결과

| step | 결과 | duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `PASS` | `97221 ms` |
| `os-mutation-gate` | `PASS` | `11079 ms` |

Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 통과했다. 최종 service state는
`Running`, boot time은 `2026-05-27T16:47:53.5+09:00`로 unchanged, VM smoke cleanup 후
`remaining_pcv_vms=[]`로 닫혔다. OS mutation gate는 final firewall rule count `0`,
Event Log source absent, trust store root/publisher restored 상태를 확인했다.

## 산출물 해시

| 산출물 | SHA-256 |
| --- | --- |
| MSI | `809eacb97a49aeaa32fc0ea3dce8ac5bdeb7c66b8b4502352519a338a512847e` |
| Payload aggregate | `7a34468d3a59c2da182835a03f440f22df9e70f31ff062dc625530a9143ef94d` |
| Host EXE | `9434e1d8d2d3d52928ab14227581a67dcb7352b6e9a00a6df4e0a55a29c2dc6d` |
| PCVCLI EXE | `2ef327140aa2a43e1ea236f44c217705dd79a33f561a10c29f773e921b17e20c` |
| PCVTUI EXE | `0bd58072b1a7a596524ab9cfde8f2336380a318941d44f5499dc4ffdc0bf39ef` |

## 경계

이 fullgate는 `0.42.57-admin-smoke` operational anchor를 0.42.56에서 0.42.57로 승격한다.
Public trusted signing, external stable publication, winget public submission claim은 계속 범위 밖이다.
