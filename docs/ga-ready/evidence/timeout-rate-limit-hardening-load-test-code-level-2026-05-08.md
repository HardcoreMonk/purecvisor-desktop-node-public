# Timeout and Rate-Limit Hardening Load Test Code-Level Evidence - 2026-05-08

evidence_id: timeout-rate-limit-hardening-load-test-code-level-2026-05-08
scope: timeout-rate-limit-hardening-load-test-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs
actual_execution: code-level-tests
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load
route_timeout_policy: code-level-applied
request_limit_policy: code-level-applied
retry_semantics_status: retry-after-problem-details-code-level
ui_api_error_contract_status: problem-details-json-code-level
load_test_status: code-level-inprocess-pass
server_config_mutation: code-level-product-and-native-service-plan-applied

## 요약

이 slice는 ADR-0005 timeout/rate-limit hardening row 중 load evidence를 code-level in-process test로 추가했다.

- `ApiHardeningRequestProcessorTests.RequestRateLimitInProcessLoadKeepsSuccessBudgetAndProblemDetailsStable`는 같은 client identity로 64개 `/api/v1/runtime/policy` request를 병렬 실행한다.
- Test profile은 `RequestLimitPerMinute=16`, `BurstLimit=4`, `RetryAfterSeconds=9`다.
- 기대 결과는 HTTP 200 `20`, HTTP 429 `44`, unexpected status `0`이다.
- 모든 HTTP 429 응답은 `application/problem+json` family의 `PCV_RATE_LIMIT_EXCEEDED`, `retry_after_seconds=9`, `operation=rate.limit` contract를 유지해야 한다.

## 범위

이 evidence는 code-level in-process load evidence다. 다음 항목은 완료로 주장하지 않는다.

- installed listener load test
- external load generator 또는 networked HTTP benchmark
- installed service config mutation, service stop/start, service reload
- host mutation, MSI/firewall/trust-store/LAN/Event Log/update mutation
- public trusted signing
- external stable publication

## 검증

TDD RED:

- `RequestRateLimitInProcessLoadKeepsSuccessBudgetAndProblemDetailsStable`는 처음에 `RunInProcessHardeningLoad` helper가 없어 compile failure로 실패했다.

GREEN:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~RequestRateLimitInProcessLoadKeepsSuccessBudgetAndProblemDetailsStable"
```

결과:

- Focused in-process load evidence test: PASS, 1 test.

이 GREEN은 request processor in-process load contract만 확인한다. Installed listener load, external load generator, installed service mutation, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
