# Timeout and Rate-Limit Hardening Preflight Evidence - 2026-05-08

evidence_id: timeout-rate-limit-hardening-preflight-2026-05-08
scope: timeout-rate-limit-hardening-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
timeout_rate_limit_hardening: blocked-by-no-mutation-preflight
route_timeout_policy: not-applied
request_limit_policy: not-applied
retry_semantics_status: not-run
ui_api_error_contract_status: not-run
load_test_status: not-run
server_config_mutation: not-run

## 요약

이 slice는 ADR-0005의 timeout/rate-limit hardening row를 실제 server config mutation 또는 middleware 적용 전 plan-only preflight로 고정한다. `New-PcvTimeoutRateLimitHardeningPreflight.ps1`는 서비스명, Local API route prefix, route timeout target, request limit target, retry-after target, UI/API error contract, hardening check 목록을 `summary.json`과 Timeout and rate-limit hardening plan preview에 기록한다.

이 도구는 server config mutation, timeout middleware enablement, request rate-limit middleware enablement, retry semantics change, UI/API error behavior verification, load test execution, service/MSI/firewall/trust-store/LAN/update mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. 실제 middleware/config 적용과 UI/API/load-test evidence가 닫히기 전까지 `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`, `route_timeout_policy: not-applied`, `request_limit_policy: not-applied`, `retry_semantics_status: not-run`, `ui_api_error_contract_status: not-run`, `load_test_status: not-run`, `server_config_mutation: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1 -ArtifactRoot 'artifacts/timeout-rate-limit-hardening-preflight-20260508-dryrun' -ServiceName 'PureCVisorDesktopNode' -ApiRoutePrefix '/api/v1/' -RouteTimeoutSeconds 30 -RequestLimitPerMinute 120 -BurstLimit 20 -RetryAfterSeconds 15 -ErrorContract 'problem-details-json' -PlanOnly
```

## Contract

```text
scope: timeout-rate-limit-hardening-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
timeout_rate_limit_hardening: blocked-by-no-mutation-preflight
route_timeout_policy: not-applied
request_limit_policy: not-applied
retry_semantics_status: not-run
ui_api_error_contract_status: not-run
load_test_status: not-run
server_config_mutation: not-run
hardening_checks:
  service-name-present
  api-route-prefix-recorded
  timeout-policy-recorded
  request-limit-policy-recorded
  retry-semantics-recorded
  ui-api-error-contract-recorded
  server-config-not-mutated
  middleware-not-enabled
  load-test-not-executed
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1`는 `New-PcvTimeoutRateLimitHardeningPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 timeout/rate-limit hardening preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- Dry-run artifact root: `artifacts/timeout-rate-limit-hardening-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=timeout-rate-limit-hardening-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `timeout_rate_limit_hardening=blocked-by-no-mutation-preflight`, `route_timeout_policy=not-applied`, `request_limit_policy=not-applied`, `retry_semantics_status=not-run`, `ui_api_error_contract_status=not-run`, `load_test_status=not-run`, `server_config_mutation=not-run`.

이 GREEN은 timeout/rate-limit hardening plan preview와 blocker descriptor만 확인한다. Server config mutation, middleware enablement, retry semantics change, UI/API error behavior verification, load test execution, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
