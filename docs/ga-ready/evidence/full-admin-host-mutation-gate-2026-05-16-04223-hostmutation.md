# 전체 관리자 Host Mutation Gate - 2026-05-16 0.42.23

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04223-hostmutation
result: PASS
version: 0.42.23-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04223
host_mutation_performed: true
dry_run: false
batch_evidence.status: available
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_route_count: 4
public_trusted_signing: excluded
external_stable_publication: not-claimed
```

이 evidence는 `0.42.23-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Batch Supervisor가 Service/MSI/Hyper-V route parity와 OS mutation gate를 elevated
`-AllowHostMutation` 범위에서 실행했고, 설치본 Web/TUI/CLI current-card가 같은 batch를
최신 operational evidence로 표시함을 확인했다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04223` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04223` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260516-04223` |
| installed operator current-card artifact root | `artifacts/installed-operator-surface-current-card-20260516-04223` |
| closed package root | `artifacts/admin-smoke-package-20260516-04223` |
| closed package MSI SHA-256 | `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406` |
| closed package provenance commit | `676b4177b10dc80209969066857bab6008ff2473` |
| full-gate MSI SHA-256 | `ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932` |
| full-gate provenance commit | `d11a096086326004f27facd9612c2296ded15a4b` |
| payload aggregate SHA-256 | `27cad8d21bdc9dd30e4831bc22649295e5158086b2c648e0736d610fd5f8ffbe` |
| product wrapper SHA-256 | `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f` |
| service host SHA-256 | `aa92e49e627a589f161cf10f862dd1813025c66a27565b2e97ce3da58a38eea5` |
| CLI SHA-256 | `f6943eda45a5fecc1ef3404a9a96f192d1fae40fed56c10e5e7df287f835a0ad` |
| TUI SHA-256 | `1205692d2cde20581e16b23e7de527797dc167291b293af549580076ed4347bf` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`,
`executed_steps=2`다.

| Step | 결과 |
| --- | --- |
| `service-msi-hyperv-admin-smoke` | `ok=true`, exit `0`, attempt `1`, duration `248829ms` |
| `os-mutation-gate` | `ok=true`, exit `0`, attempt `1`, duration `11091ms` |

Service/MSI/Hyper-V route parity는 VM create/start/restart/poweroff/delete와
checkpoint create/restore/delete를 PASS로 확인했다. Unmanaged VM delete guard는
`PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 destructive delete를 차단했고, cleanup 단계에서
foreign VM/path를 제거했다.

## OS Mutation Gate

| 항목 | 값 |
| --- | --- |
| actual execution | `completed` |
| LAN prefix | `http://[redacted-private-endpoint]:7777/` |
| boot time unchanged | `true` |
| final service state | `Running` |
| final firewall rule count | `0` |
| final Event Log source present | `false` |
| final trust root/publisher present | `true` / `true` |
| root thumbprint | `E49CD75AF53CCF7FA73C97E47443096A4507FB7E` |
| publisher thumbprint | `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` |

OS mutation gate는 config migration apply, Event Log register/remove, firewall
enable/LAN smoke/remove, internal trust-store export/install/remove/restore를 PASS로
확인했다.

## Installed Current Card

| 항목 | 값 |
| --- | --- |
| summary | `artifacts/installed-operator-surface-current-card-20260516-04223/summary.json` |
| installed runtime version | `0.42.23-admin-smoke` |
| service | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260516-04223` |
| latest release MSI SHA-256 | `ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932` |
| route evidence status | `available` |
| OS mutation evidence status | `available` |
| runtime API registry bridge | `runtime-api-diagnostics-ops-summary-registry-bridge-v2` |
| route detail count | `4` |
| TUI redaction | `redaction active` |

## 결정

`0.42.23-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260516-04223`이다. 이 evidence는
internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부
stable publication evidence가 아니다.
