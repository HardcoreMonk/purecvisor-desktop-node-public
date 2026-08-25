# Full admin host mutation gate 2026-05-27 0.42.53

evidence_id: `full-admin-host-mutation-gate-2026-05-27-04253-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate-guest-execution-provider-direct-control`
version: `0.42.53-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260527-04253`
batch_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260527-04253`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260527-04253`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260527-04253`
full_gate_msi_sha256: `14eb351000d3f6324edde5d785040667a5ddbea952cea1e20183a28882b9c669`
full_gate_payload_aggregate_sha256: `da633431d611acb8e762cb25d1e4c9530ba87887fa6fd92ba4216b70b8ce4ff4`
provenance_commit: `cc774b257d6cd772c3a890266aca62aa8ab8eadc`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Batch 결과

| step | 결과 | duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `PASS` | `217234 ms` |
| `os-mutation-gate` | `PASS` | `11096 ms` |

Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 통과했다. 최종 service state는
`Running`, boot time은 unchanged, VM smoke cleanup 후 `remaining_pcv_vms=[]`로 닫혔다.
OS mutation gate는 final firewall rule count `0`, Event Log source absent, trust store
root/publisher restored 상태를 확인했다.

## 주의

Full gate는 internal admin-smoke host mutation evidence다. Public trusted signing,
external stable publication, winget public submission claim은 여전히 범위 밖이다.

