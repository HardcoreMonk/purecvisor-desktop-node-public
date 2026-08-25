# Timeout and Rate-Limit Hardening Server Config Code-Level Evidence - 2026-05-08

evidence_id: timeout-rate-limit-hardening-server-config-code-level-2026-05-08
scope: timeout-rate-limit-hardening-server-config-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1
actual_execution: code-level-tests
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config
route_timeout_policy: code-level-applied
request_limit_policy: code-level-applied
retry_semantics_status: retry-after-problem-details-code-level
ui_api_error_contract_status: problem-details-json-code-level
load_test_status: not-run
server_config_mutation: code-level-product-and-native-service-plan-applied

## 요약

이 slice는 ADR-0005 timeout/rate-limit hardening row 중 product wrapper service plan config 경로를 code-level로 연결했다.

- `New-PcvDesktopNodeProductPlan -Action Install`의 `service.config.binary_path`는 `DesktopNode.Host.exe listen`에 `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, `--retry-after-seconds 15`를 포함한다.
- 같은 값은 `service.hardening` descriptor에도 기록되어 plan consumer와 evidence 문서가 동일한 기본값을 본다.
- 후속 `diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`는 `DesktopNode.Host.exe service-action configure-installed|repair-installed` native SCM config도 동일한 hardening 인자를 `DesktopNodeWindowsServiceConfiguration.BinaryPathName`에 싣도록 맞췄다.
- 기존 Local API request limit HTTP 429, route timeout HTTP 504, `Retry-After`, `application/problem+json` contract는 유지된다.

## 범위

이 evidence는 제품 plan contract evidence다. 다음 항목은 완료로 주장하지 않는다.

- installed service config mutation 또는 `sc.exe config` 실행
- service stop/start, reload, repair, MSI lifecycle 실행
- load test execution
- host mutation, MSI/firewall/trust-store/LAN/Event Log/update mutation
- public trusted signing
- external stable publication

## 검증

TDD RED:

- `PcvDesktopNodeProduct.Plan.Tests.ps1`의 install product plan test는 `service.hardening`과 hardening command-line arguments가 없어서 실패했다.

GREEN:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed -FullName '*install product plan*'"
```

결과:

- Focused product plan test: PASS, 1 test.

추가 검증:

- `dotnet test src\DesktopNode.sln`: PASS, 224 tests.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: PASS, 243 tests.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"`: PASS, 33 tests.
- `git diff --check`: PASS.

이 GREEN은 service plan binary path/descriptor contract만 확인한다. Load test, installed service mutation, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
