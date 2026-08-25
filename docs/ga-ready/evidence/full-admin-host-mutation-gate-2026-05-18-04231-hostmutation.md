# Full admin host mutation gate 2026-05-18 0.42.31

evidence_id: `full-admin-host-mutation-gate-2026-05-18-04231-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.31-admin-smoke`
manual_admin_package_pair: `0.42.30-admin-smoke -> 0.42.31-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260518-04231`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04231`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260518-04231`
host_mutation_performed: `true`
full_gate_msi_sha256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`
payload_aggregate_sha256: `cea7d1f798e6f0889cf0cd02da049dc7d7b0131e8df51a768c12e02ea76c22f4`
provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.31-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## Artifact

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04231/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260518-04231/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260518-04231/summary.json` |
| Hyper-V route smoke | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231/hyperv-api-route-smoke.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231/PureCVisorDesktopNode-0.42.31-admin-smoke-windows-x64.msi` |
| Installed current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04231.md` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | final service `PureCVisorDesktopNode` `Running`/`Auto`, `remaining_pcv_vms=[]`, service path에 `--batch-evidence-root` 포함 |
| Package build inside route | `pass` | route package root `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`, build UTC `2026-05-18T13:10:10.3926985Z` |
| Hyper-V actual VM mutation | `pass-with-expected-shutdown-unavailable` | `pcv-spike-api-6e1e2414` create/start/restart/checkpoint/poweroff/restore/delete PASS, guest shutdown은 integration service 부재로 retryable `PCV_VM_SHUTDOWN_NOT_AVAILABLE` |
| OS mutation gate | `pass` | firewall enable/remove, LAN listener, Event Log register/remove, internal trust-store install/remove/restore |
| LAN listener | `pass` | `http://[redacted-private-endpoint]:7777/` |
| Final service state | `pass` | service `Running`, start type `Auto` |
| Final cleanup | `pass` | firewall final count `0`, Event Log source absent, boot time unchanged |

## Hyper-V Route Smoke

`hyperv-api-route-smoke.json`는 실제 Hyper-V VM `pcv-spike-api-6e1e2414`를 생성해
`vm.create`, `vm.start`, `vm.restart`, `checkpoint.create`, `vm.poweroff`,
`checkpoint.restore`, `checkpoint.delete`, `vm.delete`, repeat delete absent handling을
검증했다. Linux ISO 부팅 상태에서는 Hyper-V guest shutdown integration service가
없어 `vm.shutdown`이 retryable `PCV_VM_SHUTDOWN_NOT_AVAILABLE`로 실패했고, smoke는
이 상태를 기록한 뒤 `vm.poweroff`로 stop/restore/delete cleanup을 완료했다.
Unmanaged VM delete는 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단되어 destructive
boundary도 확인됐다.

## Trust-store / Boundary

OS gate는 internal trust-store Root/TrustedPublisher install/remove/restore를 확인했다.
Final trust-store 상태는 root/publisher present `true`, root thumbprint
`E49CD75AF53CCF7FA73C97E47443096A4507FB7E`, publisher thumbprint
`8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`였다.

이 evidence는 관리자 opt-in host mutation evidence지만 internal admin-smoke 범위다.
Public trusted signing은 `excluded`, 외부 stable publication은 `not-claimed`다.
