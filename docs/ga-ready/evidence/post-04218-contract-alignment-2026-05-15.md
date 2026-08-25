# Post-04218 계약 정렬 - 2026-05-15

```text
evidence_id: post-04218-contract-alignment-2026-05-15
result: PASS
source_version_anchor: 0.42.18-admin-smoke
source_full_admin_gate: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md
source_package_pair: docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md
actual_execution: docs-and-contract-regression
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
runtime_api_diagnostics_bridge: route-family-evidence-linked
hyperv_dispatch_catalog_contract: vm-checkpoint-network-fixed
host_ops_lifecycle_buckets: service-eventlog-firewall-truststore-data-root-separated
packaging_release_next_trigger: pending-next-product-payload-after-04218-fullgate
operator_surface_journey_alignment: web-console-tui-cli-current-card
public_boundary_preserved: adr-0005-closed-adr-0006-internal-only
```

이 evidence는 `0.42.18-admin-smoke` package-pair와 full admin host mutation PASS
이후의 후속 개발 slice 1-2-3-4-5-6을 문서 계약으로 고정한다. 새 host mutation,
clean-host VM 실행, MSI install/update/rollback, firewall/Event Log/trust-store 변경,
public signing 또는 external publication은 실행하지 않았다.

## 1. Runtime/Core

Runtime/Core 후속은 API route family, auth/session, job runtime, diagnostics,
ops summary를 같은 evidence contract에 묶는다.

| Route family | 대표 route | Owner | Evidence anchor |
| --- | --- | --- | --- |
| runtime policy | `GET /api/v1/runtime/policy` | `DesktopNodeApiRequestProcessor` / `RuntimePolicy` | `runtime_api_diagnostics_bridge=route-family-evidence-linked` |
| jobs | `GET /api/v1/jobs`, `POST /api/v1/jobs/{jobId}/cancel`, `POST /api/v1/jobs/{jobId}/retry` | `DesktopNodeApiJobRuntimeHandler` | job runtime snapshot/worker/queued mutation contract |
| diagnostics | `GET /api/v1/diagnostics/bundles`, `POST /api/v1/diagnostics/bundles`, download route | `DesktopNodeApiDiagnosticsHandler` | diagnostics root, redaction, retention/pagination contract |
| ops summary | `GET /api/v1/ops/summary` | `DesktopNodeApiOpsSummaryDataBuilder` | installed current-card operational evidence selector |

## 2. Hyper-V Domain

Hyper-V provider dispatch는 `DesktopNodeHyperVAdapterDispatchCatalog`와
`DesktopNodeHyperVDomain`이 함께 소유한다. VM/checkpoint/network 세부 domain은
다음 handler/provider boundary로 고정한다.

| Domain | Operation | Handler | Provider boundary |
| --- | --- | --- | --- |
| network | `network.inventory` | `NetworkInventory` | `switch-provider` |
| VM inventory | `vm.list` | `VmList` | `vm-provider` |
| VM lifecycle | `vm.create`, `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart`, `vm.delete` | `VmCreate`, `VmPowerState`, `VmDelete` | `vm-create-provider`, `vm-power-state-provider`, `vm-delete-provider` |
| checkpoint | `checkpoint.list`, `checkpoint.create`, `checkpoint.restore`, `checkpoint.delete` | `CheckpointList`, `CheckpointMutation` | `checkpoint-provider`, `checkpoint-mutation-provider` |

## 3. Host Ops

Host Ops 후속은 `DesktopNodeHostOpsCatalog`의 operation family를 lifecycle smoke
bucket으로 분리한다. `service-action`은 service lifecycle 전체를 소유하지 않고,
Event Log, firewall, trust store, Credential Manager, data-root lifecycle은 각각
독립 family로 남는다.

| Bucket | Operation family | 대표 operation |
| --- | --- | --- |
| service-action lifecycle | `service-lifecycle` | `configure-installed`, `repair-installed`, `remove-installed` |
| Event Log | `event-log` | `eventlog-repair`, `eventlog-write-test`, `eventlog-default-transition` |
| firewall | `firewall` | `firewall-enable`, `firewall-remove` |
| trust store | `trust-store` | `trust-store-install`, `trust-store-remove` |
| data-root lifecycle | `data-root` | `data-root-remove` |

## 4. Packaging/Release

`0.42.18-admin-smoke`는 self-contained payload 복구와 package-pair/full-gate PASS를
이미 소유한다. 다음 package-pair는 `pending-next-product-payload-after-04218-fullgate`
후보로만 열린다. 새 Runtime/Core, Hyper-V, Host Ops, Packaging, Operator Surface
product payload 변경이 없으면 package build, clean-host campaign, full admin host
mutation gate를 열지 않는다.

## 5. Operator Surfaces

운영자 여정은 Web Console, TUI, CLI를 같은 installed Local API operator path로
정렬한다.

| Surface | 역할 | Current evidence rule |
| --- | --- | --- |
| Web Console | Dashboard/Evidence current-card, diagnostics, operator handoff | `batch_evidence.status=available`, latest batch는 full admin operational evidence |
| TUI | `pcvtui.exe --smoke-once runtime` runtime 조회 | service health가 아니라 Local API/token/route response로 판단 |
| CLI | package/update/diagnostics/operator command 실행 | token value를 출력하지 않고 protected token file 또는 redacted artifact를 사용 |

## 6. Public Boundary

ADR-0005 public distribution candidate는 계속 `closed-not-adopted`이고, ADR-0006
내부 사설망 전용 배포가 현재 적용 boundary다. 이 evidence는 internal
`AllowUnsignedDev` admin-smoke 및 repository documentation regression evidence이며,
public trusted signing, public stable installer URL, winget submission, external stable
publication, public clean-host signed install/update/rollback을 주장하지 않는다.
