# Runtime/Core 경계 기준선 - 2026-05-11

source_decision: ga-ready-product-runtime
distribution_boundary: internal-private-network-only
host_mutation_default: explicit-admin-opt-in-only
contract_owner: `src/DesktopNode.Contracts/RuntimePolicy.cs`
runtime_route_helper: `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs`
runtime_handler_split: `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`

## Runtime 소유 Route 계열

- runtime policy: `GET /api/v1/runtime/policy`
- jobs: `GET /api/v1/jobs`, `GET /api/v1/jobs/{jobId}`, `POST /api/v1/jobs/{jobId}/cancel`, `POST /api/v1/jobs/{jobId}/retry`
- diagnostics: `GET /api/v1/diagnostics/bundles`, `POST /api/v1/diagnostics/bundles`, `GET /api/v1/diagnostics/bundles/{bundleId}/download`
- auth/session/RBAC: `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`, `GET /api/v1/auth/session`, `GET /api/v1/auth/rbac`
- console handoff metadata: `GET /api/v1/console/capabilities`, `GET /api/v1/vms/{vmId}/console`
- ops summary: `GET /api/v1/ops/summary`

## Runtime/Core 계약

- API route family는 `ApiHandlerAdapterContract.RouteFamily`와 `DesktopNodeApiRuntimeRoutes`가 소유한다.
- API route RBAC permission은 `ApiHandlerAdapterContract.RequiredPermission`이 additive contract로 노출한다. `GET /api/v1/runtime/policy`는 account auth bootstrap 전에도 읽을 수 있으므로 `null`이며, job 조회와 ops summary는 `read`, job cancel/retry는 `operate`, diagnostic bundle read/create는 각각 `diagnostics.read`/`diagnostics.create`를 사용한다.
- request processor dispatch는 `DesktopNodeApiAuthSessionHandler`, `DesktopNodeApiJobRuntimeHandler`, `DesktopNodeApiDiagnosticsHandler`, `DesktopNodeApiConsoleHandler`, `DesktopNodeApiOpsSummaryHandler`를 거쳐 auth/session, jobs, diagnostics, console, ops-summary 계열을 분리한다.
- auth/session contract는 `runtime_core.auth_session`에서 owner, route, token storage, session storage, loopback/non-loopback boundary를 노출한다.
- job runtime contract는 `runtime_core.job_runtime`에서 `DesktopNode.Runtime`, JSON snapshot store, bounded worker, native queued mutation boundary를 노출한다.
- diagnostics contract는 `runtime_core.diagnostics`에서 bundle route, configured diagnostics root, redaction required, retention/pagination boundary를 노출한다.

## 0.42.18 이후 API Route Evidence Bridge

`docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`는
`runtime_api_diagnostics_bridge=route-family-evidence-linked`로 API route family와
운영 evidence를 묶는다. `DesktopNodeApiRuntimeRoutes`는 route classifier를 소유하고,
`ApiHandlerAdapterContract.RouteFamily`는 운영 문서가 참조하는 route family 이름을
고정한다.

| Family | Routes | Evidence owner |
| --- | --- | --- |
| jobs | `GET /api/v1/jobs`, `POST /api/v1/jobs/{jobId}/cancel`, `POST /api/v1/jobs/{jobId}/retry` | job runtime snapshot/worker/queued mutation contract |
| diagnostics | `GET /api/v1/diagnostics/bundles`, `POST /api/v1/diagnostics/bundles`, download route | diagnostics root, redaction, retention/pagination contract |
| ops-summary | `GET /api/v1/ops/summary` | installed current-card latest operational evidence selector |
| console | `GET /api/v1/console/capabilities`, `GET /api/v1/vms/{vmId}/console` | Web Console/TUI/CLI operator journey handoff |

## Native Adapter 소유 Route 계열

- host status: `GET /api/v1/host/status`
- network inventory: `GET /api/v1/network/inventory`
- VM read: `GET /api/v1/vms`, `GET /api/v1/vms/{vmId}`
- checkpoint read: `GET /api/v1/vms/{vmId}/checkpoints`
- VM/checkpoint queued mutations: `POST /api/v1/vms`, VM lifecycle routes, checkpoint create/restore/delete routes, `DELETE /api/v1/vms/{vmId}`

## 비목표

- 이 workstream에서는 Hyper-V WMI provider 이동을 하지 않는다.
- 이 workstream에서는 Windows service/firewall/Event Log/trust store mutation 이동을 하지 않는다.
- Public distribution은 다시 열지 않는다.
