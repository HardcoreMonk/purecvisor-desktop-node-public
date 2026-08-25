# 전체 관리자 Host Mutation Gate - 2026-05-16 0.42.21

```text
evidence_id: full-admin-host-mutation-gate-2026-05-16-04221-hostmutation
result: PASS
version: 0.42.21-admin-smoke
batch_id: full-admin-host-mutation-gate-20260516-04221
host_mutation_performed: true
dry_run: false
batch_evidence.status: available
runtime_api_registry_bridge_contract: runtime-api-diagnostics-ops-summary-registry-bridge-v2
public_trusted_signing: excluded
external_stable_publication: not-claimed
```

이 evidence는 `0.42.21-admin-smoke` 전체 관리자 host mutation gate를 닫는다.
Batch Supervisor가 Service/MSI/Hyper-V route parity와 OS mutation gate를
elevated `-AllowHostMutation` 범위에서 실행했고, installed current-card smoke가
같은 batch를 최신 operational evidence로 표시함을 확인했다.

## Provenance

| 항목 | 값 |
| --- | --- |
| batch root | `artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04221` |
| route parity artifact root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04221` |
| OS mutation artifact root | `artifacts/os-mutation-gates-batch-profile-20260516-04221` |
| installed current-card artifact root | `artifacts/installed-current-card-20260516-04221-fullgate` |
| clean package root | `artifacts/admin-smoke-package-20260516-04221` |
| clean package MSI SHA-256 | `d97ca81fffec9fc07ca6bb1d7094f48102e815fbc1f0104d61a06e0b99675b7b` |
| full-gate MSI SHA-256 | `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551` |
| provenance commit | `3b8c48deb4c31675f6fce46c320703f23c27c131` |
| signing mode | `AllowUnsignedDev` |

## Batch Result

Batch summary는 `ok=true`, `status=completed`, `total_steps=2`,
`executed_steps=2`다.

| Step | 결과 |
| --- | --- |
| `service-msi-hyperv-admin-smoke` | `ok=true`, exit `0`, attempt `1` |
| `os-mutation-gate` | `ok=true`, exit `0`, attempt `1` |

## Installed Current Card

| 항목 | 값 |
| --- | --- |
| summary | `artifacts/installed-current-card-20260516-04221-fullgate/summary.json` |
| installed runtime version | `0.42.21-admin-smoke` |
| service | `Running` |
| Web Console | HTTP `200` |
| `/pcv-config.js` | HTTP `200` |
| unauthenticated runtime policy | HTTP `401` |
| batch evidence status | `available` |
| latest batch id | `full-admin-host-mutation-gate-20260516-04221` |
| latest release MSI SHA-256 | `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551` |
| route evidence status | `available` |
| OS mutation evidence status | `available` |
| runtime API registry bridge | `runtime-api-diagnostics-ops-summary-registry-bridge-v2` |

## 결정

`0.42.21-admin-smoke` full admin host mutation gate는 PASS다. 최신 operational
current-card anchor는 `full-admin-host-mutation-gate-20260516-04221`이다. 이 evidence는
internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부
stable publication evidence가 아니다.

