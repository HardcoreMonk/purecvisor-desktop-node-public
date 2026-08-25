# Full admin host mutation gate 2026-07-16 0.42.65

evidence_id: `full-admin-host-mutation-gate-2026-07-16-04265-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.65-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260716-04265`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260716-04265`
execution_shape: `direct-elevated-route-parity-plus-os-mutation-script-pair`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260716-04265`
routeparity_summary: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260716-04265/summary.json`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260716-04265`
os_mutation_summary: `artifacts/os-mutation-gates-batch-profile-20260716-04265/summary.json`
clean_package_msi_sha256: `5709edb0d5f265393c8690c212dd6d1f61873f7cbbaa110b1654a2e380e6b748`
clean_package_payload_aggregate_sha256: `3b4fefb3c03c1a70ba804e959931bdec0ee36923139a84602e85be69e96e251a`
full_gate_msi_sha256: `9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30`
full_gate_payload_aggregate_sha256: `5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28`
provenance_commit: `4855947fe0199cedc978e8b40ffb45e96ced6876`
route_steps: `4/4`
os_mutation_steps: `11/11`
executed_steps: `2/2`
host_mutation_performed: `true`
boot_time_unchanged: `true`
service_final_state: `Running/Automatic`
remaining_test_vm_count: `0`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS buckets

| Bucket | 결과 | 근거 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `PASS` | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260716-04265/summary.json` |
| OS mutation gate | `PASS` | `artifacts/os-mutation-gates-batch-profile-20260716-04265/summary.json` |

Batch는 두 step을 각각 첫 attempt, exit `0`으로 완료했다. Route parity summary는 `ok=true`이며
package build, service action, MSI lifecycle, 설치본 Hyper-V API route를 모두 완료했다. Service는
`Running`/`Auto`, boot time은 unchanged이고 남은 `pcv-spike-*` test VM은 없다.

OS mutation summary도 `ok=true`이며 실제 preferred physical LAN 주소
`http://[redacted-private-endpoint]:7777/`에서 firewall, LAN listener, Event Log, trust-store lifecycle을
완료했다. Final state에서 firewall rule count는 `0`, 임시 Event Log source는 없고 기존 internal
trust root/publisher는 복원됐으며 service는 `Running`/`Auto`다.

Clean package provenance는 `AllowUnsignedDev`/`LocalTest`, operational full-gate provenance는
`AllowUnsignedDev`/`Unspecified`다. 모두 internal admin-smoke 범위이며 public trusted signing,
외부 stable publication, 별도 manual-admin package-pair campaign을 주장하지 않는다.
