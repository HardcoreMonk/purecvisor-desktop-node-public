# PureCVisor Desktop Node API/Host Job Hardening Installed Evidence Design

작성 기준: 2026-05-11

## 목적

Windows Desktop Node Local API와 Host listener의 운영 안정성을 강화한다. 대상은
`worker 분리`, `cooperative cancellation`, `job policy 단일화`,
`request body cap`, `출력 설명 문구 가독성 강화`다.

이번 작업은 code-level hardening만으로 끝내지 않는다. 최종 완료 기준은 설치본
서비스에서 관리자 opt-in smoke evidence까지 남기는 것이다. 다만 public trusted
signing, winget public submission, external stable publication, clean-host public
signed smoke는 ADR-0006 기준 계속 out-of-scope다.

## 현재 문제 판단

현재 `DesktopNodeHostApplication`은 API request 처리 후 `processor.ProcessWorkerPool()`
을 호출한다. `DesktopNodeApiRequestProcessor`는 내부 `sync` lock 아래에서 queued job을
처리하고, native Hyper-V WMI provider는 job 완료를 polling한다. 이 구조는 작은 테스트
범위에서는 단순하지만, 긴 Hyper-V mutation이 들어오면 read route 응답성과 service stop
경계가 약해질 수 있다.

또한 request body read는 명시적인 크기 상한을 갖지 않는다. LAN 또는 account-auth
구성에서 큰 body가 들어오면 API processor 이전 단계에서 메모리 압박이 발생할 수 있다.

Job state transition은 `DesktopNode.Runtime.JobStateTransitionPolicy`와
`DesktopNodeApiRequestProcessor` 내부 string 상태 변경이 병존한다. 현재 테스트는
통과하지만, cancel/retry/recovery 규칙이 장기적으로 drift될 위험이 있다.

오류 출력은 machine-readable field 중심으로 안정적이지만, 운영자 관점에서는 "무슨 일이
일어났는지", "현재 어떤 상태인지", "다음에 무엇을 해야 하는지"가 더 명확해야 한다.

## 설계 원칙

- Windows Desktop Node 전용 경계를 유지한다.
- Active product runtime은 `.NET Host + C# native Hyper-V adapter` 경계를 유지한다.
- PowerShell helper fallback, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime
  코드를 추가하지 않는다.
- 기존 API JSON contract를 깨지 않는다. 새 사람이 읽는 field는 additive로만 추가한다.
- 설치본 evidence는 internal/private network 제품 evidence로 기록하고 public release
  evidence로 주장하지 않는다.
- Hyper-V에 이미 전달된 host mutation의 강제 abort 또는 rollback은 이번 범위에서
  주장하지 않는다.

## 접근 방식

권장 접근은 두 code-level slice와 하나의 installed admin evidence slice다.

1. Request body cap과 job policy 단일화를 먼저 적용한다.
2. Background worker 분리와 cooperative cancellation을 적용한다.
3. 설치본 service smoke로 listener, job, timeout/cancellation, account/noVNC/diagnostics
   regression evidence를 남긴다.

이 순서는 실패 원인 분리를 쉽게 하면서도 관리자 host mutation evidence 실행 횟수를
과도하게 늘리지 않는다.

## Component Design

### Host Listener

`DesktopNode.Host`는 HTTP listener, static web, noVNC bridge, request body cap,
request-level cancellation을 소유한다.

`DesktopNodeHostOptions`에는 body cap 설정을 추가한다.

- 기본값: 1 MiB
- 허용 범위: 1 KiB 이상 64 MiB 이하
- CLI option 후보: `--max-request-body-bytes`
- service-action 기본 service plan에도 같은 값을 명시한다.

`DesktopNodeHostApplication`은 API request body를 읽기 전에 `ContentLength64`를
검사한다. 길이를 알 수 없는 stream은 bounded reader로 읽고, cap을 넘는 순간 더 읽지
않고 `413`을 반환한다. 정적 파일과 noVNC WebSocket bridge에는 이 cap을 적용하지 않는다.

### API Request Processor

`DesktopNode.Api`는 route handling, auth/RBAC, job store, job queue, worker orchestration을
소유한다. HTTP 요청은 mutation job enqueue까지만 수행하고 즉시 `202`를 반환한다.

현재 `Handle()`의 전역 직렬화 lock은 상태 변경 보호에는 유지하되, 긴 native operation이
그 lock 아래에서 실행되지 않도록 worker 실행 경계를 분리한다. Job dequeue와 상태 변경은
짧은 lock 안에서 수행하고, native operation은 lock 밖에서 실행한다. 완료 후 다시 lock을
잡고 결과를 저장한다.

### Job Runtime Policy

`DesktopNode.Runtime.JobStateTransitionPolicy`를 job 상태 전이의 단일 진실로 둔다.

`DesktopNodeApiJob`의 JSON contract는 기존 string `status`를 유지한다. 단, 상태를 바꾸는
코드는 string 값을 직접 대입하지 않고 다음 adapter를 거친다.

- string status를 `JobStatus` enum으로 변환한다.
- `Start`, `Complete`, `Cancel`, `Retry`, `RecoverPersistedRunningJob` 정책을 호출한다.
- 결과를 다시 string status와 API error payload로 변환한다.

이 방식은 기존 JSON payload 호환성을 유지하면서 policy drift를 차단한다.

### Background Worker

Host service start 시 processor worker loop를 시작하고, service stop 시 cancellation을
전달한다. Worker는 같은 `DesktopNode.Host.exe` process 안에서 동작한다.

기본 worker count는 1로 둔다. Hyper-V mutation ordering과 host mutation safety가 더
중요하므로 이번 설계에서는 parallel mutation worker를 도입하지 않는다.

Worker loop는 다음 규칙을 따른다.

- service cancellation이 요청되면 새 job 시작을 중단한다.
- 이미 running인 job은 provider cancellation token을 전달받는다.
- provider가 cancellation을 관찰하면 job은 structured cancellation failure로 저장된다.
- WMI가 이미 host mutation을 시작한 경우 강제 rollback을 주장하지 않는다.

### Cooperative Cancellation

`IDesktopNodeHyperVNativeAdapter.TryInvoke`와 provider interfaces에는 `CancellationToken`을
전달하는 overload 또는 signature 확장을 추가한다. 기존 테스트 fixture는 새 token 인자를
받도록 갱신한다.

WMI wait loop는 polling 사이에 token을 확인한다. Token이 취소되면 다음 error code 후보를
사용한다.

- worker/job cancellation: `PCV_JOB_CANCELED`
- native wait cancellation: `PCV_NATIVE_OPERATION_CANCELED`
- route response deadline: `PCV_ROUTE_TIMEOUT`

세 코드는 서로 다르게 유지한다. Timeout은 HTTP 응답 deadline 문제이고, cancellation은
worker/service lifecycle 문제다.

## 출력 설명 문구 가독성

API error payload의 machine-readable field는 유지한다.

- `code`
- `operation`
- `status`
- `request_id`
- `retryable`

사람이 읽는 field는 아래 원칙을 따른다.

- `message`: 한 문장으로 관찰된 문제를 설명한다.
- `detail`: 왜 발생했는지와 현재 경계를 설명한다.
- `recommended_action`: 운영자가 다음에 취할 행동을 제안한다.

`recommended_action`은 additive optional field다. 기존 클라이언트는 이 field를 무시해도
동작해야 한다.

예시:

```text
message: The Local API route timed out before the response deadline.
detail: The native Hyper-V operation may still be running until WMI returns.
recommended_action: Check the job status, then retry after the Retry-After interval.
```

한국어 evidence 문서에는 같은 의미를 다음 구조로 기록한다.

```text
판단: Local API 응답 deadline을 초과했다.
관찰: native Hyper-V WMI 작업은 백그라운드에서 계속 완료될 수 있다.
권장 조치: job 상태를 확인하고 Retry-After 이후 재시도한다.
```

CLI/TUI 출력은 토큰, 비밀번호, `Authorization` header, protected token blob, 민감 경로를
계속 redaction한다.

## Installed Evidence Design

설치본 evidence는 새 문서로 남긴다.

후보 경로:

```text
docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md
artifacts/api-host-job-hardening-installed-evidence-20260511
```

Evidence는 한국어 기준으로 작성한다. 코드 식별자, API route, error code, command,
file path는 원문을 유지한다.

검증 항목:

- body cap smoke: cap 초과 API request는 `413`을 반환하고 service는 계속 running이다.
- normal body smoke: 정상 크기 request는 기존 route contract를 유지한다.
- long job responsiveness smoke: queued mutation 처리 중 `GET /api/v1/runtime/policy`와
  `GET /api/v1/jobs`가 block되지 않는다.
- cancellation boundary smoke: service/worker cancellation 요청이 structured evidence로
  남는다.
- timeout boundary smoke: route deadline 초과는 `PCV_ROUTE_TIMEOUT`과 사람이 읽기 쉬운
  설명 field를 반환한다.
- regression smoke: account login, diagnostics bundle create/download, noVNC configured
  path, Web/API port split이 기존 evidence 기준을 유지한다.

Evidence 문서는 다음 경계를 명시한다.

- `host_mutation_performed`
- `public_trusted_signing: out-of-scope` 또는 `not-claimed`
- `external_stable_publication: out-of-scope` 또는 `not-claimed`
- `cooperative_cancellation_scope`
- `wmi_abort_claim: not-claimed`

## Test Strategy

### xUnit

필수 테스트:

- `DesktopNode.Host.Tests`
  - body cap 초과 `413`
  - unknown length body bounded read
  - 기존 token-protected diagnostics route 회귀 없음
- `DesktopNode.Api.Tests`
  - enqueue는 빠르게 `202` 반환
  - background worker가 queued job 처리
  - running job 중 read route가 worker lock에 막히지 않음
  - timeout/cancellation error에 `recommended_action` 포함
- `DesktopNode.Runtime.Tests`
  - API adapter가 `JobStateTransitionPolicy`와 같은 cancel/retry/start/complete 결과를 사용
  - invalid persisted status는 structured failure 또는 safe recovery로 처리

### Pester / Web

변경이 service plan, packaging manifest, Web/CLI/TUI error 표시 계약에 닿으면 다음을
검증한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Web 변경이 있으면 다음도 수행한다.

```powershell
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
```

공통 검증:

```powershell
dotnet test src/DesktopNode.sln
git diff --check
```

## Rollout

1. Code-level slice 1: body cap과 job policy 단일화.
2. Code-level slice 2: background worker와 cooperative cancellation.
3. Output readability slice: API/CLI/TUI/evidence message 정리.
4. Packaging/docs slice: service plan, verification policy, evidence ledger 연결.
5. Installed admin evidence slice: 설치본 service smoke 실행과 evidence 문서 작성.

각 slice는 독립 테스트를 통과한 뒤 다음 단계로 넘어간다.

## Risk and Mitigation

- Risk: worker 분리 중 job store write race가 생길 수 있다.
  - Mitigation: job state mutation과 store save는 processor lock 아래에서만 수행한다.
- Risk: WMI cancellation을 사용자가 강제 rollback으로 오해할 수 있다.
  - Mitigation: evidence와 API detail에 WMI abort/rollback을 claim하지 않는다고 명시한다.
- Risk: body cap이 diagnostics create 같은 정상 route를 막을 수 있다.
  - Mitigation: 기본 cap은 1 MiB로 두고, service option으로 조정 가능하게 한다.
- Risk: status string과 enum adapter에서 unknown status가 발생할 수 있다.
  - Mitigation: unknown persisted status는 safe structured failure 또는 blocked diagnostics로 처리한다.
- Risk: 사람이 읽는 출력 문구 변경이 snapshot tests를 깨뜨릴 수 있다.
  - Mitigation: machine-readable code 중심 assertion은 유지하고, 새 field는 additive로 검증한다.

## Approval State

사용자가 승인한 결정:

- 접근안 B: 두 단계 code-level hardening 후 installed admin evidence.
- 문서 본문은 한국어 기준으로 작성한다.
- API field, command, route, code identifier는 기존 원문 계약을 유지한다.
- 출력 설명 문구 가독성 강화를 작업 범위에 포함한다.
