# Timeout and Rate-Limit Hardening Code-Level Evidence - 2026-05-08

evidence_id: timeout-rate-limit-hardening-code-level-2026-05-08
scope: timeout-rate-limit-hardening-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs, src/DesktopNode.Host/DesktopNodeHostApplication.cs, src/DesktopNode.Host/DesktopNodeHostOptions.cs
actual_execution: code-level-tests
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
timeout_rate_limit_hardening: partial-code-level-request-limit
route_timeout_policy: not-applied
request_limit_policy: code-level-applied
retry_semantics_status: retry-after-problem-details-code-level
ui_api_error_contract_status: problem-details-json-code-level
load_test_status: not-run
server_config_mutation: not-run

## 요약

이 slice는 ADR-0005 timeout/rate-limit hardening row 중 request rate-limit와 Retry-After/problem-details error contract를 code-level actual path로 구현했다.

- `DesktopNodeApiRequestProcessor`는 `ClientIdentity`가 있는 `/api/v1/` 요청에 대해 1분 sliding window request limit을 적용한다.
- `DesktopNodeApiResponse`는 additive `Headers` contract를 갖고, rate-limit 초과 시 `Retry-After`를 반환한다.
- Rate-limit 초과 응답은 `application/problem+json`, HTTP 429, `PCV_RATE_LIMIT_EXCEEDED`, `retry_after_seconds`, `request_id`를 포함한다.
- `DesktopNodeHostApplication`은 remote IP를 client identity로 전달하고 API response header를 HTTP response에 반영한다.
- `DesktopNodeHostOptions`는 `--request-limit-per-minute`, `--request-burst-limit`, `--retry-after-seconds`, `--route-timeout-seconds` parse contract를 갖는다.

## 범위

이 evidence는 code-level API/Host request limit contract evidence다. 다음 항목은 완료로 주장하지 않는다.

- route timeout enforcement
- load test execution
- server config mutation 또는 installed service config migration
- public trusted signing
- external stable publication
- host mutation, MSI/firewall/trust-store/LAN/Event Log/update mutation

## 검증

TDD RED:

- `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`는 `DesktopNodeApiHardeningOptions`, response headers, client identity rate-limit path 부재로 실패했다.
- `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`와 `DesktopNodeHostApplicationTests.cs`는 hardening options/HTTP Retry-After 반영 부재로 실패했다.

GREEN:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore
```

결과:

- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore`: PASS, 128 tests.
- `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore`: PASS, 63 tests.

이 GREEN은 code-level request rate-limit/Retry-After/problem-details contract만 확인한다. Route timeout enforcement, load test, installed service mutation, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
