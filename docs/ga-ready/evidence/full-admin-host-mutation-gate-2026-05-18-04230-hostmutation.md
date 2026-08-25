# Full admin host mutation gate 2026-05-18 0.42.30

evidence_id: `full-admin-host-mutation-gate-2026-05-18-04230-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.30-admin-smoke`
manual_admin_package_pair: `0.42.29-admin-smoke -> 0.42.30-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260518-04230`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04230`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260518-04230`
host_mutation_performed: `true`
full_gate_msi_sha256: `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`
payload_aggregate_sha256: `0fddc06c7ced0239ea04a89fd90cc0c152a64688904e0f58b97c3fcd5368a28c`
provenance_commit: `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.30-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## Artifact

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04230/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04230/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260518-04230/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230/PureCVisorDesktopNode-0.42.30-admin-smoke-windows-x64.msi` |
| Installed current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | final service `PureCVisorDesktopNode` `Running`/`Auto`, process id `21124`, `remaining_pcv_vms=[]`, service path에 `--batch-evidence-root` 포함 |
| Package build inside route | `pass` | route package root `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`, build UTC `2026-05-18T09:47:12.6538779Z` |
| OS mutation gate | `pass` | firewall enable/remove, LAN listener, Event Log register/remove, internal trust-store install/remove/restore |
| LAN listener | `pass` | `http://[redacted-private-endpoint]:7777/` |
| Final service state | `pass` | service `Running`, start type `Auto` |
| Final cleanup | `pass` | firewall final count `0`, Event Log source absent, boot time unchanged |

## Trust-store / Boundary

OS gate는 internal trust-store Root/TrustedPublisher install/remove/restore를 확인했다.
Final trust-store 상태는 root/publisher present `true`, root thumbprint
`E49CD75AF53CCF7FA73C97E47443096A4507FB7E`, publisher thumbprint
`8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`였다.

이 evidence는 관리자 opt-in host mutation evidence지만 internal admin-smoke 범위다.
Public trusted signing은 `excluded`, 외부 stable publication은 `not-claimed`다.
