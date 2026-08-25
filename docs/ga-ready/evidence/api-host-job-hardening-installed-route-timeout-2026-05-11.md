# API Host Job Hardening 설치본 Route-Timeout 근거 - 2026-05-11

evidence_id: api-host-job-hardening-installed-route-timeout-2026-05-11
scope: api-host-job-hardening-installed-controlled-route-timeout
result: PASS
build_artifact_root: artifacts/api-host-job-hardening-route-timeout-rebaseline-20260511-040309
canonical_smoke_artifact_root: artifacts/api-host-job-hardening-installed-route-timeout-20260511-040654
invalid_first_attempt_artifact_root: artifacts/api-host-job-hardening-installed-route-timeout-20260511-040525
service_name: PureCVisorDesktopNode
api_base_uri: http://127.0.0.1:7777
campaign_host_mutation_performed: true
smoke_runner_host_mutation_performed: false
token_value_observed: false
password_value_observed: false
refresh_token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## 요약

`0.41.8-admin-smoke` payload를 현재 HEAD `2e086eb88bbef629145618b0791da7f5db487257`에서 빌드하고, 설치본 product update와 service-action repair 경로로 installed listener를 재구성했다. Controlled route-timeout probe는 기본 비활성 상태이며, 이번 installed smoke 동안에만 `--route-timeout-seconds 1` 및 `--controlled-route-timeout-probe-delay-ms 2500`로 임시 활성화했다.

결과는 `PASS`다. 설치 listener에서 `GET /api/v1/runtime/route-timeout-probe`가 `504 PCV_ROUTE_TIMEOUT`으로 종료됐고, `application/problem+json` 및 `Retry-After: 15`가 함께 관찰됐다. 이후 service-action repair로 기본 PathName을 복원했고, 복원 후 probe route는 `404 PCV_ROUTE_NOT_FOUND`로 닫힌 것을 확인했다.

## 빌드 및 설치 근거

| 항목 | 관찰값 | 판정 |
| --- | --- | --- |
| version | `0.41.8-admin-smoke` | PASS |
| git commit | `2e086eb88bbef629145618b0791da7f5db487257` | PASS |
| MSI SHA-256 | `342d2e3e864d5feb5f7be14fa6eb2cacd56b482320b928076bf5f27e4c1a207d` | recorded |
| payload aggregate SHA-256 | `735c9562ea3f738b704e3349c93adab0e0ddbd36b15ccfa9b8d3f95c99932d9a` | recorded |
| DesktopNode.Host.exe SHA-256 | `cef487e94bc082b490bfbf2a02545afaaddd2c5c475f0680c9b41c35cec87a32` | PASS |
| signing mode | `AllowUnsignedDev` | internal smoke only |
| public trusted signing | `not-claimed` | PASS |
| external stable publication | `not-claimed` | PASS |

Product update는 `0.41.7-admin-smoke -> 0.41.8-admin-smoke`로 완료됐다. Update result는 `ok=true`, health probe는 `http://127.0.0.1:7777/api/v1/runtime/policy`에서 HTTP `200`을 기록했다.

## 통제 Route-Timeout Probe 설정

임시 repair 실행 결과:

```text
service-action repair-installed: Ok=true
actual service status: running
actual BinaryPathName contains --api-token-credential-target
actual BinaryPathName contains --route-timeout-seconds 1
actual BinaryPathName contains --controlled-route-timeout-probe-delay-ms 2500
actual BinaryPathName contains --max-request-body-bytes 1048576
```

주의할 점은 plan preview의 token source가 protected file로 보일 수 있으나, 실제 `Service.BinaryPathName`은 기존 Credential Manager target을 보존했다. 이는 repair-installed가 현재 SCM PathName의 `--api-token-credential-target`을 읽어 실제 configure 단계에 다시 적용했기 때문이다.

## 설치본 Smoke 관찰값

Canonical summary:

```powershell
Get-Content -Raw artifacts/api-host-job-hardening-installed-route-timeout-20260511-040654/summary.json
```

| Probe | 관찰값 | 판정 |
| --- | --- | --- |
| overall | `ok=true` | PASS |
| bearer token source | `environment-variable` | PASS |
| token value observed | `false` | PASS |
| body cap | `413 PCV_REQUEST_BODY_TOO_LARGE application/problem+json` | PASS |
| route timeout path | `GET /api/v1/runtime/route-timeout-probe` | PASS |
| route timeout status | `504` | PASS |
| route timeout error code | `PCV_ROUTE_TIMEOUT` | PASS |
| route timeout content type | `application/problem+json` | PASS |
| route timeout Retry-After | `15` | PASS |
| route timeout duration | `1018ms` | recorded |
| runtime policy | `200` | PASS |
| jobs read | `200` | PASS |
| diagnostics read | `200` | PASS |
| console capabilities read | `200` | PASS |
| missing job cancel | `404 PCV_JOB_NOT_FOUND` | PASS |
| worker responsiveness | `observed_nonblocking=true` | PASS |
| before/after service PID | `13440 -> 13440` | PASS |

첫 번째 installed smoke attempt `artifacts/api-host-job-hardening-installed-route-timeout-20260511-040525`는 product failure가 아니라 runner 호출 실수로 폐기했다. protected token reader가 반환한 object에서 `.token` 값을 꺼내지 않고 환경변수에 object string을 넣어 HTTP `403 PCV_AUTH_FORBIDDEN`이 발생했다. Canonical PASS는 같은 설치 상태에서 token 값을 출력하지 않고 환경변수에만 올바르게 주입한 `20260511-040654` 실행이다.

## 복원 검증

임시 probe 검증 후 service-action repair를 다시 실행해 기본 hardening PathName으로 복원했다.

```text
restore repair: Ok=true
final service state: Running
final service start mode: Auto
final service process_id: 34304
final PathName has --route-timeout-seconds 30: true
final PathName has --max-request-body-bytes 1048576: true
final PathName has --api-token-credential-target: true
final PathName has --controlled-route-timeout-probe-delay-ms: false
runtime policy after restore: 200
route-timeout probe after restore: 404 PCV_ROUTE_NOT_FOUND
```

복원 검증 artifact:

```powershell
Get-Content -Raw artifacts/api-host-job-hardening-route-timeout-rebaseline-20260511-040309/final-restore-verification.json
```

핵심 값:

```text
ok=true
service.has_controlled_route_timeout_probe=false
service.has_route_timeout_30=true
service.has_max_request_body_bytes=true
service.has_credential_target=true
runtime_status=200
controlled_probe_after_restore_status=404
controlled_probe_after_restore_code=PCV_ROUTE_NOT_FOUND
token_value_observed=false
```

## 검증 명령

코드 수준 및 runner gate:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiHardeningRequestProcessorTests --no-restore -m:1
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostApplicationTests|DesktopNodeHostServiceActionTests" --no-restore -m:1
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1 -Output Detailed
git diff --check
```

설치본 실행:

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.41.8-admin-smoke -OutputRoot artifacts/api-host-job-hardening-route-timeout-rebaseline-20260511-040309 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/api-host-job-hardening-route-timeout-rebaseline-20260511-040309/payload -Version 0.41.8-admin-smoke
DesktopNode.Host.exe service-action repair-installed --route-timeout-seconds 1 --controlled-route-timeout-probe-delay-ms 2500
packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -RunRouteTimeoutProbe
DesktopNode.Host.exe service-action repair-installed
```

## 경계

이 evidence는 Windows Desktop Node installed listener의 controlled route-timeout probe 결과다. Product payload update와 service-action repair/start/stop은 수행했으나, firewall, trust-store, Hyper-V VM, Event Log provider mutation은 수행하지 않았다. Smoke runner 자체는 HTTP 요청만 보냈고 `host_mutation_performed=false`로 기록했다.

`PCV_ROUTE_TIMEOUT` PASS는 installed listener의 route deadline 및 cancellation token 경계가 동작했음을 보여준다. 이 evidence는 native WMI 작업의 강제 abort를 주장하지 않으며, `wmi_abort_claim=not-claimed`를 유지한다.

이 evidence는 public trusted signing, trusted timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed install/update/rollback readiness를 주장하지 않는다. Internal-only service boundary와 ADR-0006 private network distribution boundary를 유지한다.
