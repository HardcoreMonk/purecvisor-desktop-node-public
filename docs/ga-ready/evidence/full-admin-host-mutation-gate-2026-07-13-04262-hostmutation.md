# Full admin host mutation gate 2026-07-13 0.42.62

evidence_id: `full-admin-host-mutation-gate-2026-07-13-04262-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.62-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260713-04262`
batch_summary: `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04262/summary.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260713-04262`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260713-04262`
clean_package_msi_sha256: `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`
clean_package_payload_aggregate_sha256: `0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e`
full_gate_msi_sha256: `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`
full_gate_payload_aggregate_sha256: `ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`
provenance_commit: `7f71f0a518c5b592f233373522d36b5401c3f1df`
batch_status: `completed`
batch_ok: `true`
executed_steps: `2/2`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## PASS buckets

| Bucket | 결과 | 근거 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `PASS` | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260713-04262/summary.json` |
| OS mutation gate | `PASS` | `artifacts/os-mutation-gates-batch-profile-20260713-04262/summary.json` |
| Batch supervisor | `PASS` | `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04262/summary.json` |

Route step은 package build, service action, MSI lifecycle, 설치본 Hyper-V API route를 모두
완료했다. OS mutation step은 실제 실행됐고 Event Log, firewall, trust store, LAN listener
lifecycle을 완료했다. Batch summary는 `ok=true`, `status=completed`, `executed_steps=2`를
기록하며 두 step exit code는 모두 `0`이다.

Final route/OS state에서 boot time은 unchanged, service는 `Running`/`Auto`, remaining
`pcv-*` VM은 빈 배열이다. OS cleanup 후 firewall rule count는 `0`, 임시 Event Log source는
존재하지 않는다.

이 evidence는 internal admin-smoke host mutation evidence이며 public trusted signing 또는 외부
stable publication을 주장하지 않는다. Manual-admin package-pair campaign은 이 gate에서
실행하지 않았다.
