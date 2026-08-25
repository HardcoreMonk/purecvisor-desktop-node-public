# Full admin host mutation gate 2026-05-17 0.42.29

evidence_id: `full-admin-host-mutation-gate-2026-05-17-04229-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.29-admin-smoke`
manual_admin_package_pair: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
manual_admin_update_zip_sha256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
batch_id: `full-admin-host-mutation-gate-20260517-04229`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04229`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260517-04229`
host_mutation_performed: `true`
full_gate_msi_sha256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
clean_package_msi_sha256: `2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77`
route_payload_aggregate_sha256: `703a5677ea00f24d397e56a25e2f94ae40794671d0870221f80df1bbbb928a3f`
provenance_commit: `d306712ad671c8a00d5c560765b8952e24a07502`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.29-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## Artifact

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04229/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04229/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260517-04229/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229/PureCVisorDesktopNode-0.42.29-admin-smoke-windows-x64.msi` |
| Installed current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04229.md` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | final service `PureCVisorDesktopNode` `Running`/`Auto`, process id `23996`, `remaining_pcv_vms=[]`, GPU snapshots `22` |
| Package build inside route | `pass` | route package root `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229`, build UTC `2026-05-17T10:07:24.2228993Z` |
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
