# API Host Job Hardening Installed Evidence - 2026-05-11

evidence_id: api-host-job-hardening-installed-evidence-2026-05-11
scope: api-host-job-hardening-installed-smoke
result: PASS_AFTER_REBASELINE
build_artifact_root: artifacts/api-host-job-hardening-rebaseline-20260511-033429
smoke_artifact_root: artifacts/api-host-job-hardening-installed-evidence-20260511-033429-rebaseline-pass
previous_blocked_artifact_root: artifacts/api-host-job-hardening-installed-evidence-20260511-022747
actual_execution: installed-payload-update-service-repair-and-installed-listener-readonly-http-smoke
service_name: PureCVisorDesktopNode
api_base_uri: http://127.0.0.1:7777
host_mutation_performed_by_rebaseline: true
host_mutation_performed_by_smoke_runner: false
token_value_observed: false
password_value_observed: false
refresh_token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

초기 installed smoke는 request body cap이 auth route logic보다 먼저 동작하지 않아 `BLOCKED`였다. 직접 원인은 설치본 service `PathName`에 `--max-request-body-bytes`가 없고, 설치 payload가 현재 HEAD hardening과 불일치한 상태였기 때문이다.

후속 rebaseline에서 현재 HEAD payload를 `0.41.7-admin-smoke`로 빌드하고, product update와 native service `repair-installed`를 실행했다. 이후 설치본 `DesktopNode.Host.exe` hash가 build provenance와 일치했고, service `PathName`에 `--api-token-credential-target`와 `--max-request-body-bytes 1048576`가 함께 존재함을 확인했다.

재실행 smoke 결과는 `PASS`다. Oversized `POST /api/v1/auth/login`은 계정 미구성 auth route logic보다 먼저 `413 PCV_REQUEST_BODY_TOO_LARGE application/problem+json`으로 끊겼고, runtime/jobs/diagnostics/console read route와 missing job cancel contract도 기대값을 만족했다.

## Rebaseline 근거

| 항목 | 관찰값 | 판정 |
| --- | --- | --- |
| build version | `0.41.7-admin-smoke` | PASS |
| build provenance commit | `49260c88cdb34843d063ffb45536172b870944ec` | PASS |
| MSI SHA-256 | `498267be0ae684f6afa3cbf2d0fc5efcf8c91c08836b88c762a3f66b685f78f5` | recorded |
| payload aggregate SHA-256 | `2f39b58a41b971b68cc8be4724893cb39d8c47c7e6f0e4cac5223644079a5f2d` | recorded |
| service host SHA-256 | `bdcfc536e1f50d291459af584259964155aeb7a11ef7aff05ad44b76d7920d34` | PASS |
| installed host SHA-256 | `bdcfc536e1f50d291459af584259964155aeb7a11ef7aff05ad44b76d7920d34` | PASS |
| installed manifest version | `0.41.7-admin-smoke` | PASS |
| product update | `0.41.5-admin-smoke` -> `0.41.7-admin-smoke`, rollback attempted `false` | PASS |
| native service repair | `repair-installed`, service owner verified `true`, status `running` | PASS |

설치 서비스 최종 `PathName` 확인:

```text
"C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe" listen ... --api-token-credential-target "PureCVisor/PureCVisorDesktopNode/api-token" ... --route-timeout-seconds 30 --request-limit-per-minute 120 --request-burst-limit 20 --retry-after-seconds 15 --max-request-body-bytes 1048576
```

Credential Manager token source는 유지됐고, protected file token source로 되돌아가지 않았다.

## Installed Smoke Result

| Probe | Expected | Observed | Result |
| --- | --- | --- | --- |
| `POST /api/v1/auth/login` oversized body | `413`, `PCV_REQUEST_BODY_TOO_LARGE`, `application/problem+json` | `413`, `PCV_REQUEST_BODY_TOO_LARGE`, `application/problem+json` | PASS |
| `GET /api/v1/runtime/policy` | `200` | `200` | PASS |
| `GET /api/v1/jobs?limit=1&offset=0` | `200` | `200` | PASS |
| `GET /api/v1/diagnostics/bundles?limit=1&offset=0` | `200` | `200` | PASS |
| `GET /api/v1/console/capabilities` | `200` | `200` | PASS |
| `POST /api/v1/jobs/pcv-installed-hardening-missing-job/cancel` | non-mutating `404/401/403` with expected job/auth code | `404`, `PCV_JOB_NOT_FOUND` | PASS |

Worker responsiveness:

| Route | Duration |
| --- | --- |
| runtime policy | `38 ms` |
| jobs | `20 ms` |
| diagnostics | `3 ms` |
| console capabilities | `2 ms` |
| threshold | `20000 ms` |
| observed_nonblocking | `true` |

Route timeout remains `not-run-installed-smoke-has-no-controlled-slow-native-route`. Rate-limit probe remains `not-run-by-default-controlled-load-probe`. This evidence does not claim installed 504 route-timeout execution or installed external rate-limit load execution.

## Previous Blocker

초기 실행 artifact `artifacts/api-host-job-hardening-installed-evidence-20260511-022747`의 핵심 BLOCKED 값:

```text
ok=false
body_cap.status_code=409
body_cap.error_code=PCV_ACCOUNT_AUTH_NOT_CONFIGURED
body_cap.expected_error_code=PCV_REQUEST_BODY_TOO_LARGE
service PathName did not include --max-request-body-bytes
```

후속 focused code-level test는 body cap이 account auth 미구성 route보다 먼저 적용됨을 확인했다. 따라서 blocker는 code-level priority 결함이 아니라 설치 payload/config rebaseline 미완료로 판정했다.

## Runner Follow-up

rebaseline 직후 첫 installed smoke는 실제 API 응답이 `413 application/problem+json`이었지만, PowerShell 7 `Invoke-WebRequest -SkipHttpErrorCheck`의 byte-array `Content`를 runner가 문자열로 잘못 변환해 `PCV_REQUEST_BODY_TOO_LARGE`를 추출하지 못했다. Runner를 UTF-8 decode하도록 고치고 Pester guard를 추가한 뒤 같은 설치본 대상으로 재실행해 PASS를 얻었다.

이 runner 보정은 smoke evidence 파싱 문제를 수정한 것이며 API/server contract 자체를 완화하지 않았다.

## Verification

실행 및 확인 명령:

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1 -Output Detailed
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostServiceActionTests"
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1 -Output Detailed
git diff --check
```

Observed result:

```text
PcvDesktopNodeProduct.Plan.Tests.ps1: 24 passed, 0 failed
DesktopNodeHostServiceActionTests: 57 passed, 0 failed
PcvApiHostJobHardeningInstalledSmoke.Tests.ps1: 8 passed, 0 failed
git diff --check: clean, line-ending normalization warnings only
```

Canonical artifacts:

```powershell
Get-Content -Raw artifacts/api-host-job-hardening-rebaseline-20260511-033429/PureCVisorDesktopNode-0.41.7-admin-smoke-windows-x64.provenance.json
Get-Content -Raw artifacts/api-host-job-hardening-rebaseline-20260511-033429/product-update-result.json
Get-Content -Raw artifacts/api-host-job-hardening-rebaseline-20260511-033429/service-repair-result.json
Get-Content -Raw artifacts/api-host-job-hardening-installed-evidence-20260511-033429-rebaseline-pass/summary.json
```

Key final summary values:

```text
ok=true
actual_execution=installed-listener-readonly-http-smoke
bearer_token_source=environment-variable
token_value_observed=false
host_mutation_performed=false
body_cap.status_code=413
body_cap.error_code=PCV_REQUEST_BODY_TOO_LARGE
body_cap.expected_content_type_observed=true
runtime_policy.status_code=200
job_readability.status_code=200
diagnostics_readability.status_code=200
console_capabilities.status_code=200
job_cancellation.status_code=404
job_cancellation.error_code=PCV_JOB_NOT_FOUND
worker_responsiveness.observed_nonblocking=true
```

## Boundary

이 evidence는 Windows Desktop Node installed payload rebaseline, native service repair, installed listener HTTP smoke 결과다. Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime code나 public distribution gate를 도입하지 않는다.

이 evidence는 public trusted signing, trusted timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed install/update/rollback readiness를 주장하지 않는다. Internal-only service boundary와 ADR-0006 private network distribution boundary를 유지한다.
