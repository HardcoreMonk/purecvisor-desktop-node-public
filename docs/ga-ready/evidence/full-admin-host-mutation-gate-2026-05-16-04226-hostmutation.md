# Full Admin Host Mutation Gate 2026-05-16 0.42.26 PASS

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04226-hostmutation
result: PASS
scope: full-admin-host-mutation-gate
version: 0.42.26-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04226
artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04226
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260516-04226
host_mutation_performed: true
full_gate_msi_sha256: f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7
provenance_commit: d6500c01c972cbc7ca1e290e51120181ceea1501
signing_mode: AllowUnsignedDev
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

이 evidence는 `0.42.26-admin-smoke` full admin host mutation gate를 기록한다. Batch
Supervisor `FullAdminHostMutationGate` profile로 Service/MSI/Hyper-V route parity와
OS mutation gate를 실제 host mutation으로 실행했다.

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04226/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04226/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260516-04226/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226/PureCVisorDesktopNode-0.42.26-admin-smoke-windows-x64.msi` |
| full-gate MSI SHA-256 | `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7` |
| provenance commit | `d6500c01c972cbc7ca1e290e51120181ceea1501` |
| LAN prefix smoke | `http://[redacted-private-endpoint]:7777/` |

## 결과

| Gate | 결과 |
| --- | --- |
| Service/MSI/Hyper-V route parity | `PASS` |
| MSI lifecycle smoke | `PASS` |
| installed Hyper-V API route smoke | `PASS` |
| config migration apply while service running | `PASS` |
| Event Log register/remove | `PASS` |
| firewall enable/remove | `PASS` |
| LAN listener IP smoke | `PASS` |
| internal trust store install/remove/restore | `PASS` |

Final state는 service `Running`, firewall rule count `0`, Event Log source absent,
internal Root/TrustedPublisher cert present, boot time unchanged, `remaining_pcv_vms=[]`다.

## 결정

이 run이 최신 operational full admin host mutation anchor다. Installed Web/TUI/CLI
current-card 확인은
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md`가
소유한다. 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는
외부 stable publication evidence가 아니다.
