# Timeout and Rate-Limit Hardening Route Timeout Code-Level Evidence - 2026-05-08

evidence_id: timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08
scope: timeout-rate-limit-hardening-route-timeout-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs
actual_execution: code-level-tests
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
timeout_rate_limit_hardening: partial-code-level-route-and-request-limit
route_timeout_policy: code-level-applied
request_limit_policy: code-level-applied
retry_semantics_status: retry-after-problem-details-code-level
ui_api_error_contract_status: problem-details-json-code-level
load_test_status: not-run
server_config_mutation: not-run

## 요약

이 slice는 ADR-0005 timeout/rate-limit hardening row 중 route timeout response deadline을 code-level actual path로 추가했다.

- `DesktopNodeApiRequestProcessor`는 `/api/v1/` GET/read route handling을 `DesktopNodeApiHardeningOptions.RouteTimeoutSeconds` deadline 안에서 완료해야 한다.
- Deadline을 넘으면 HTTP 504, `application/problem+json`, `PCV_ROUTE_TIMEOUT`, `Gateway Timeout`, `route_timeout_seconds`, `retry_after_seconds`, `request_id`를 반환한다.
- Timeout response도 `Retry-After` header를 포함하므로 request-limit 초과 응답의 retry semantics와 같은 UI/API error contract family를 사용한다.
- 기존 request limit evidence의 HTTP 429, `PCV_RATE_LIMIT_EXCEEDED`, `Retry-After`, `application/problem+json` contract는 유지된다.

## 범위

이 evidence는 code-level Local API response deadline evidence다. 다음 항목은 완료로 주장하지 않는다.

- mutation-route cancellation
- native adapter cooperative cancellation
- load test execution
- server config mutation 또는 installed service config migration
- public trusted signing
- external stable publication
- host mutation, MSI/firewall/trust-store/LAN/Event Log/update mutation

## 검증

TDD RED:

- `ApiHardeningRequestProcessorTests.RouteTimeoutReturnsProblemDetailsWhenNativeRouteExceedsDeadline`는 2초 native route가 deadline을 넘어도 기존 processor가 HTTP 200을 반환해 실패했다.

GREEN:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~RouteTimeoutReturnsProblemDetailsWhenNativeRouteExceedsDeadline"
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore
```

결과:

- Focused route timeout test: PASS, 1 test.
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore`: PASS, 129 tests.
- `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore`: PASS, 63 tests.

이 GREEN은 code-level route response deadline과 problem-details/Retry-After contract만 확인한다. Load test, server config mutation, installed service mutation, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
