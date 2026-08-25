# Full admin host mutation gate 2026-05-27 0.42.50

evidence_id: `full-admin-host-mutation-gate-2026-05-27-04250-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate-guest-execution-preview-api-cli`
version: `0.42.50-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260527-04250`
batch_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260527-04250`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260527-04250`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260527-04250`
full_gate_msi_sha256: `99c8e4adf8959de3da3d5a9a1157cd1ea2f9580eb16cf4ba1a9738013a376d6b`
full_gate_payload_aggregate_sha256: `119b38c811a3a34529da17c65edab8f992f5e21c725b9d3d83459e44d7dd9ed9`
provenance_commit: `d42ff7fddc67cbcebbfcbbec3342278511edafb3`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Batch 결과

| step | 결과 | duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `PASS` | `91205 ms` |
| `os-mutation-gate` | `PASS` | `11071 ms` |

`ops summary` current evidence는 `full-admin-host-mutation-gate-20260527-04250`를 최신
full admin host mutation anchor로 노출했다. 최종 service state는 `Running`이며, VM smoke
cleanup 후 `remaining_pcv_vms=[]`로 닫혔다.

## 주의

Full gate는 internal admin-smoke host mutation evidence다. Public trusted signing,
external stable publication, winget public submission claim은 여전히 범위 밖이다.
