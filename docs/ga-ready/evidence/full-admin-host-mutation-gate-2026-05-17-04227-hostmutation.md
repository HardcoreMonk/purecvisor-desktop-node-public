# Full admin host mutation gate 2026-05-17 0.42.27

evidence_id: `full-admin-host-mutation-gate-2026-05-17-04227-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.27-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260517-04227`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04227`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260517-04227`
host_mutation_performed: `true`
full_gate_msi_sha256: `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`
clean_package_msi_sha256: `0084d6ded5723ceb378c0805b9e9369e6626460bd6185d98e0a1028050f6be4a`
provenance_commit: `69aba3eb3ff08c843f1a481818ddc86eac2f019b`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.27-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 elevated `-AllowHostMutation`으로 Service/MSI/Hyper-V route smoke와
OS mutation gate를 모두 실행했고 `ok=true`, `status=completed`,
`executed_steps=2`로 종료했다.

## Batch

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04227/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260517-04227/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260517-04227/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227/PureCVisorDesktopNode-0.42.27-admin-smoke-windows-x64.msi` |

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
| Installed current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04227.md` |

이 gate는 internal admin-smoke 검증이며 public trusted signing 또는 외부 stable
publication evidence가 아니다.
