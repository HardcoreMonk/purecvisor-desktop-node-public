# Full admin host mutation gate 2026-05-17 0.42.28

evidence_id: `full-admin-host-mutation-gate-2026-05-17-04228-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.28-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260517-04228`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04228`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04228`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260517-04228`
host_mutation_performed: `true`
full_gate_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
clean_package_msi_sha256: `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`
provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
signing_mode: `AllowUnsignedDev`
host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.28-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 elevated `-AllowHostMutation`으로 Service/MSI/Hyper-V route smoke와
OS mutation gate를 모두 실행했고 `ok=true`, `status=completed`,
`executed_steps=2`로 종료했다.

## Batch

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04228/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04228/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04228/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260517-04228/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04228/PureCVisorDesktopNode-0.42.28-admin-smoke-windows-x64.msi` |

## 확인 결과

| Gate | 결과 |
| --- | --- |
| Batch Supervisor | `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2` |
| Service/MSI/Hyper-V route smoke | `exit_code=0`, final service `Running`, boot time unchanged |
| OS mutation gate | `exit_code=0`, `host_mutation_performed=true` |
| LAN listener | `http://[redacted-private-endpoint]:7777/` smoke PASS |
| Firewall cleanup | final firewall rule count `0` |
| Event Log cleanup | final source present `false` |
| Trust store | root/publisher present after restore `true` |
| Installed current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04228.md` |
| Host Ops Web diagnostics table | `host-ops-web-diagnostics-bucket-table-v1` |

이 gate는 internal admin-smoke 검증이며 public trusted signing 또는 외부 stable
publication evidence가 아니다.
