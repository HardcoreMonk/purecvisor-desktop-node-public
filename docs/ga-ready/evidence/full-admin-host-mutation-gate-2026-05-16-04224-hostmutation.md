# 전체 관리자 Host Mutation Gate - 2026-05-16 0.42.24

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04224-hostmutation
result: PASS
version: 0.42.24-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04224
host_mutation_performed: true
dry_run: false
batch_evidence.status: available
runtime_api_current_evidence_contract: runtime-api-current-evidence-rollup-v1
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_route_count: 4
public_trusted_signing: excluded
external_stable_publication: not-claimed
```

이 evidence는 `0.42.24-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Batch Supervisor가 Service/MSI/Hyper-V route parity와 OS mutation gate를 elevated
`-AllowHostMutation` 범위에서 실행했고, 설치본 Web/TUI/CLI current-card가
`current_evidence` rollup과 같은 batch를 최신 operational evidence로 표시함을 확인했다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04224` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04224` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260516-04224` |
| installed operator current-card artifact root | `artifacts/installed-operator-surface-current-card-20260516-04224` |
| closed package root | `artifacts/admin-smoke-package-20260516-04224` |
| closed package MSI SHA-256 | `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e` |
| closed package provenance commit | `b974d6b541423f2e4160f726f96155b16f105e9d` |
| full-gate MSI SHA-256 | `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826` |
| full-gate provenance commit | `b974d6b541423f2e4160f726f96155b16f105e9d` |
| payload aggregate SHA-256 | `7879e953f499b9e51af2efd44fda14e69dbb566695495a268281f12a9e9140b3` |
| product wrapper SHA-256 | `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f` |
| service host SHA-256 | `e396223284a78816b78d36d4fc4509ebee33ae22dd8e08d491e1ab7f4a915447` |
| CLI SHA-256 | `5f10ba11b07672ba8bf1d2d122a0e11949d9dbe07b6d86aca134349489028477` |
| TUI SHA-256 | `b4f2354848fcd5ad1459afcb745de764bad8c9da3ae9afee8e82a85a751aa1a4` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`,
`executed_steps=2`다.

| Step | 결과 |
| --- | --- |
| `service-msi-hyperv-admin-smoke` | `ok=true`, exit `0`, attempt `1`, duration `199976ms` |
| `os-mutation-gate` | `ok=true`, exit `0`, attempt `1`, duration `11074ms` |

Service/MSI/Hyper-V route parity는 build, service action smoke, MSI lifecycle,
installed .NET host Hyper-V API route smoke를 PASS로 확인했다. Unmanaged VM delete
guard는 destructive delete를 차단하고 cleanup 후 `remaining_pcv_vms=[]`로 종료했다.

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
| summary | `artifacts/installed-operator-surface-current-card-20260516-04224/summary.json` |
| installed runtime version | `0.42.24-admin-smoke` |
| service | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401` / `PCV_AUTH_REQUIRED` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260516-04224` |
| current evidence contract | `runtime-api-current-evidence-rollup-v1` |
| latest release MSI SHA-256 | `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826` |
| route evidence status | `available` |
| OS mutation evidence status | `available` |
| runtime API registry bridge | `runtime-api-diagnostics-ops-summary-registry-bridge-v2` |
| route detail count | `4` |
| TUI redaction | token/password value observed `false` |

## 결정

`0.42.24-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260516-04224`이며,
Runtime/API `current_evidence` rollup은 해당 batch를 `available`로 표시한다. 이
evidence는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing
또는 외부 stable publication evidence가 아니다.
