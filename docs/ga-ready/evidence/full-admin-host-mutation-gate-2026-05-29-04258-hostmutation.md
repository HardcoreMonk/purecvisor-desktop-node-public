# Full admin host mutation gate 2026-05-29 0.42.58

evidence_id: `full-admin-host-mutation-gate-2026-05-29-04258-hostmutation`
result: `PASS`
scope: `internal-full-admin-host-mutation-gate`
version: `0.42.58-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260529-04258`
batch_summary: `artifacts/batch-runs/full-admin-host-mutation-gate-20260529-04258/summary.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260529-04258`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260529-04258`
clean_package_msi_sha256: `6ae889eeb1b7134fab9618941748528f6260727abbc8ff36eee301b59dff6c0b`
full_gate_msi_sha256: `7e0aef503b3f56eb116d5931c9560a3dcd2c4ba347f1eb24e4b505b28e6c2845`
provenance_commit: `96182b440b35c17183802ad323a123ff6e4b6730`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| Service/MSI/Hyper-V admin smoke | `PASS` | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260529-04258/summary.json` |
| OS mutation gate | `PASS` | `artifacts/os-mutation-gates-batch-profile-20260529-04258/summary.json` |
| Batch supervisor | `PASS` | `artifacts/batch-runs/full-admin-host-mutation-gate-20260529-04258/summary.json` |

Service/MSI/Hyper-V step은 MSI install/repair/remove/preserve/remove-data, installed
Hyper-V route parity smoke, cleanup을 PASS로 닫았다. OS mutation gate는 Event Log,
firewall, trust store, LAN listener, service final state를 확인했고 boot time은 변경되지
않았다.

이 evidence는 internal admin-smoke host mutation evidence이며 public trusted signing 또는
외부 stable publication을 주장하지 않는다.
