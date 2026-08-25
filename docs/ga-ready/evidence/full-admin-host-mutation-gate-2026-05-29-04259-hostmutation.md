# Full admin host mutation gate 2026-05-29 0.42.59

evidence_id: `full-admin-host-mutation-gate-2026-05-29-04259-hostmutation`
result: `PASS`
scope: `internal-full-admin-host-mutation-gate`
version: `0.42.59-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260529-04259`
batch_summary: `artifacts/batch-runs/full-admin-host-mutation-gate-20260529-04259/summary.json`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260529-04259`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260529-04259`
clean_package_msi_sha256: `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`
full_gate_msi_sha256: `dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`
provenance_commit: `63d57feba605f82dabd44a96ed50a4d622f6310a`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| Service/MSI/Hyper-V admin smoke | `PASS` | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260529-04259/summary.json` |
| OS mutation gate | `PASS` | `artifacts/os-mutation-gates-batch-profile-20260529-04259/summary.json` |
| Batch supervisor | `PASS` | `artifacts/batch-runs/full-admin-host-mutation-gate-20260529-04259/summary.json` |

Service/MSI/Hyper-V step은 MSI install/repair/remove/preserve/remove-data, installed
Hyper-V route parity smoke, cleanup을 PASS로 닫았다. OS mutation gate는 Event Log,
firewall, trust store, LAN listener, service final state를 확인했고 boot time은 변경되지
않았다.

이 evidence는 internal admin-smoke host mutation evidence이며 public trusted signing 또는
외부 stable publication을 주장하지 않는다.
