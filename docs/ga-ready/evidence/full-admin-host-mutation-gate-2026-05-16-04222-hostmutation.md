# 전체 관리자 Host Mutation Gate - 2026-05-16 0.42.22

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04222-hostmutation
result: PASS
version: 0.42.22-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04222
host_mutation_performed: true
dry_run: false
batch_evidence.status: available
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
runtime_api_registry_bridge_route_count: 4
public_trusted_signing: excluded
external_stable_publication: not-claimed
```

이 evidence는 `0.42.22-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Batch Supervisor가 Service/MSI/Hyper-V route parity와 OS mutation gate를 elevated
`-AllowHostMutation` 범위에서 실행했고, installed current-card smoke가 같은 batch를
최신 operational evidence로 표시함을 확인했다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04222` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04222` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260516-04222` |
| installed current-card artifact root | `artifacts/installed-current-card-20260516-04222-fullgate` |
| clean package root | `artifacts/admin-smoke-package-20260516-04222` |
| clean package MSI SHA-256 | `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3` |
| full-gate MSI SHA-256 | `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c` |
| provenance commit | `8a38995cc25a888f64473e9a2869740949ad6b24` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`,
`executed_steps=2`다.

| Step | 결과 |
| --- | --- |
| `service-msi-hyperv-admin-smoke` | `ok=true`, exit `0`, attempt `1`, duration `212665ms` |
| `os-mutation-gate` | `ok=true`, exit `0`, attempt `1`, duration `11107ms` |

## Installed Current Card

| 항목 | 값 |
| --- | --- |
| summary | `artifacts/installed-current-card-20260516-04222-fullgate/summary.json` |
| installed runtime version | `0.42.22-admin-smoke` |
| service | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260516-04222` |
| latest release MSI SHA-256 | `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c` |
| route evidence status | `available` |
| OS mutation evidence status | `available` |
| final firewall rule count | `0` |
| final Event Log source present | `false` |
| final trust root/publisher present | `true` / `true` |
| runtime API registry bridge | `runtime-api-diagnostics-ops-summary-registry-bridge-v2` |
| route detail count | `4` |

## 결정

`0.42.22-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260516-04222`이다. 이 evidence는
internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부
stable publication evidence가 아니다.

