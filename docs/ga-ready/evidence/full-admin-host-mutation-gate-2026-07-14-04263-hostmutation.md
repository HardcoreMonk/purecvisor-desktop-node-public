# Full admin host mutation gate 2026-07-14 0.42.63

evidence_id: `full-admin-host-mutation-gate-2026-07-14-04263-hostmutation`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.63-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260714-04263`
batch_evidence_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260714-04263`
execution_shape: `direct-elevated-route-parity-plus-os-mutation-script-pair`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260714-04263`
routeparity_summary: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260714-04263/summary.json`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260714-04263`
os_mutation_summary: `artifacts/os-mutation-gates-batch-profile-20260714-04263/summary.json`
clean_package_msi_sha256: `d2f2fff7fb400647135d96449f36704af2d080e1a6a97a551354290cdf1a6f04`
clean_package_payload_aggregate_sha256: `19f80f3e0b849d180a3e62461742a8a2ab7371e632dbfecfc8fad28bf59721f4`
full_gate_msi_sha256: `6a520e52042bdca5d55b73a4614aa0ebddaf54d576ddf60739146c2ad6784589`
full_gate_payload_aggregate_sha256: `be53d348199ee7fab95b3b4148d805d81aa98f80aa330cc376e94216e6db210e`
provenance_commit: `9a020dec285d4fbbfe161ca2d31242f305cde572`
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
| Service/MSI/Hyper-V route | `PASS` | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260714-04263/summary.json` |
| OS mutation gate | `PASS` | `artifacts/os-mutation-gates-batch-profile-20260714-04263/summary.json` |

Route parity summary는 `ok=true`이며 package build, service action, MSI lifecycle, 설치본
Hyper-V API route를 모두 완료했다. Service는 `Running`/`Auto`, boot time은 unchanged이고
남은 `pcv-spike-*` test VM은 없다.

OS mutation summary도 `ok=true`이며 실제 preferred physical LAN 주소
`http://[redacted-private-endpoint]:7777/`에서 firewall, LAN listener, Event Log, trust-store lifecycle을
완료했다. Final state에서 firewall rule count는 `0`, 임시 Event Log source는 없고 기존
internal trust root/publisher는 복원됐으며 service는 `Running`/`Auto`다.

이 evidence는 `AllowUnsignedDev`/`LocalTest` internal admin-smoke host mutation evidence다.
Public trusted signing 또는 외부 stable publication을 주장하지 않으며 manual-admin
package-pair campaign은 이 gate에서 실행하지 않았다.
