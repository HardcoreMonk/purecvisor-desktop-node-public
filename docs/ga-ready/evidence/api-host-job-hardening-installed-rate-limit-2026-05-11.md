# API Host Job Hardening Installed Rate-Limit Evidence - 2026-05-11

evidence_id: api-host-job-hardening-installed-rate-limit-2026-05-11
scope: api-host-job-hardening-installed-rate-limit-opt-in
result: PASS
artifact_root: artifacts/api-host-job-hardening-installed-rate-limit-20260511-0340
related_rebaseline_evidence: docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md
actual_execution: installed-listener-controlled-load-probe
service_name: PureCVisorDesktopNode
api_base_uri: http://127.0.0.1:7777
host_mutation_performed: false
token_value_observed: false
password_value_observed: false
refresh_token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

`0.41.7-admin-smoke` installed payload rebaseline PASS 이후, 같은 설치 listener를 대상으로 rate-limit opt-in probe를 실행했다. 실행은 `Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -RunRateLimitProbe` 경로를 사용했고, token은 process environment variable로만 전달했다.

결과는 `PASS`다. Controlled load probe에서 `429 PCV_RATE_LIMIT_EXCEEDED`가 관찰됐고, `Retry-After: 15`와 `application/problem+json` content type이 함께 확인됐다.

## Observed Result

| 항목 | 관찰값 | 판정 |
| --- | --- | --- |
| probe status | `executed-controlled-load-probe` | PASS |
| request count | `136` | recorded |
| observed `200` | `135` | recorded |
| observed `429` | `1` | PASS |
| first `429` path | `GET /api/v1/runtime/policy` | PASS |
| first `429` error code | `PCV_RATE_LIMIT_EXCEEDED` | PASS |
| first `429` content type | `application/problem+json` | PASS |
| first `429` `Retry-After` | `15` | PASS |
| unexpected status codes | `[]` | PASS |
| expected content type observed | `true` | PASS |
| expected retry-after observed | `true` | PASS |

Smoke runner의 전체 pass gate도 유지됐다.

| Probe | Observed | Result |
| --- | --- | --- |
| oversized body cap | `413 PCV_REQUEST_BODY_TOO_LARGE application/problem+json` | PASS |
| runtime policy before rate probe | `200` | PASS |
| jobs read | `200` | PASS |
| diagnostics read | `200` | PASS |
| console capabilities read | `200` | PASS |
| missing job cancel | `404 PCV_JOB_NOT_FOUND` | PASS |
| worker responsiveness | `observed_nonblocking=true` | PASS |

## Service Stability

Service snapshot:

```text
before_service.state=Running
before_service.process_id=23668
after_service.state=Running
after_service.process_id=23668
host_mutation_performed=false
```

별도 확인에서도 `PureCVisorDesktopNode`는 `Running`, `StartMode=Auto`, `ProcessId=23668` 상태를 유지했다.

## Runner Gate 보강

이번 opt-in 검증 전에 smoke runner gate를 강화했다. 기존 gate는 opt-in rate-limit probe에서 `429`와 `PCV_RATE_LIMIT_EXCEEDED`만 pass 조건으로 보았으나, 이번 evidence 목적에 맞게 아래 항목도 pass 조건에 포함했다.

```text
rate_limit.first_429.content_type like application/problem+json*
rate_limit.first_429.retry_after is present
```

따라서 이 evidence의 `PASS`는 단순 429 발생이 아니라 problem-details 및 retry guidance contract까지 포함한다.

## Verification

실행 및 확인 명령:

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1 -Output Detailed
packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -RunRateLimitProbe
git diff --check
```

Observed result:

```text
PcvApiHostJobHardeningInstalledSmoke.Tests.ps1: 9 passed, 0 failed
installed rate-limit opt-in smoke: ok=true
git diff --check: clean, line-ending normalization warnings only
```

Canonical artifact:

```powershell
Get-Content -Raw artifacts/api-host-job-hardening-installed-rate-limit-20260511-0340/summary.json
```

Key summary values:

```text
ok=true
actual_execution=installed-listener-readonly-http-smoke
rate_limit.status=executed-controlled-load-probe
rate_limit.request_count=136
rate_limit.observed_200=135
rate_limit.observed_429=1
rate_limit.first_429.status_code=429
rate_limit.first_429.error_code=PCV_RATE_LIMIT_EXCEEDED
rate_limit.first_429.content_type=application/problem+json
rate_limit.first_429.retry_after=15
rate_limit.expected_content_type_observed=true
rate_limit.expected_retry_after_observed=true
host_mutation_performed=false
token_value_observed=false
```

## Boundary

이 evidence는 Windows Desktop Node installed listener의 controlled rate-limit opt-in probe 결과다. 설치 payload update, service repair, firewall, trust-store, Hyper-V, Event Log provider mutation을 수행하지 않았다.

이 evidence는 public trusted signing, trusted timestamp, external stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed install/update/rollback readiness를 주장하지 않는다. Internal-only service boundary와 ADR-0006 private network distribution boundary를 유지한다.
