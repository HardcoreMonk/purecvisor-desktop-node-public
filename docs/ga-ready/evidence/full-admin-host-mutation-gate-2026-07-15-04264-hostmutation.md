# Full admin host mutation gate 2026-07-15 0.42.64

evidence_id: `full-admin-host-mutation-gate-2026-07-15-04264-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.64-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260715-04264`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260715-04264`
execution_shape: `direct-elevated-route-parity-plus-os-mutation-script-pair`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260715-04264`
routeparity_summary: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260715-04264/summary.json`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260715-04264`
os_mutation_summary: `artifacts/os-mutation-gates-batch-profile-20260715-04264/summary.json`
clean_package_msi_sha256: `8ba9714995d153e97a84c90afcf01b3ab1a612a166089e764b7046aae46c1cb7`
clean_package_payload_aggregate_sha256: `d3070394a44d09d34b78a3c06b4e7f99a5bc266ba91306ae41dd1bacf611487f`
full_gate_msi_sha256: `540f5c5fc8bc78a7c07f950cf9c39002491e69308dc264112a42ad0b510f50bf`
full_gate_payload_aggregate_sha256: `d02aec33be7d8f12348e242336604bb453b63b5b0d2cc139f6ced1ef15287cc0`
provenance_commit: `a0491e39992093b9ad506619cfacb1675939d6a3`
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
| Service/MSI/Hyper-V route | `PASS` | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260715-04264/summary.json` |
| OS mutation gate | `PASS` | `artifacts/os-mutation-gates-batch-profile-20260715-04264/summary.json` |

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
