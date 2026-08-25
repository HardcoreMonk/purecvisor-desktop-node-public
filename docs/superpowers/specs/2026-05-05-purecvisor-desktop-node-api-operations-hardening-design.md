# PureCVisor Desktop Node API Operations Hardening 설계

## 목적

이 문서는 P2 Web UI polish 이후 남은 route-wide API Operations Hardening 후속을 별도 구현 단위로 분리한다.

대상은 내부 전용 Windows/Hyper-V Desktop Node Local API다. 이 설계는 public CORS expansion, Single Edge REST surface copy, public metrics, public trusted signing, 외부 stable publication을 제품 범위로 가져오지 않는다.

## 상태 마커

```text
DESKTOP_NODE_API_OPERATIONS_HARDENING: implemented-request-job-correlation-and-error-shape
```

이 설계는 ADR이 아니다. 구현 증거는 `a7b3b33 Add API request job correlation`와 `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-api-operations-hardening.md`를 따른다. 이 증거는 내부 개발 검증이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

## 범위

### 채택

- 모든 Local API JSON response에 `request_id`를 추가한다.
- `DesktopNodeApiRequest`는 optional request id를 받을 수 있고, 없으면 request processor가 `req-<guid>` 형식으로 생성한다.
- Queued job 생성 response와 job snapshot에는 `request_id`와 `correlation_id`를 보존한다.
- retry job은 새 `request_id`를 갖고, 원본 job id는 기존 `retry_of`로 보존한다.
- failure body는 기존 `ok`, `operation`, `data`, `error` shape를 유지하면서 `request_id`만 추가한다.
- `GET /api/v1/jobs`는 server-side activity snapshot으로서 request/correlation field를 반환한다.
- Web Console Activity는 request/correlation id가 있으면 표시하고, 없으면 기존 job id 중심 표시로 fallback한다.

### 제외

- HTTP server header plumbing 전체 재설계
- public CORS 확대
- unauthenticated metrics endpoint
- public Prometheus
- Single Edge JWT/RBAC/user DB 이식
- Event Log source registration, firewall/trust-store/LAN/MSI/service mutation
- `jobs.json` schema migration apply
- config migration apply
- public trusted signing 또는 외부 stable publication claim

## 호환성 원칙

- 기존 client가 `request_id`를 무시해도 동작해야 한다.
- 기존 `DesktopNodeApiRequest(method, path, body)` 생성자는 계속 source-compatible해야 한다.
- 기존 job store v1을 읽을 수 있어야 한다. 과거 job에 `request_id`가 없으면 null로 다루고, `correlation_id`가 없으면 기존 job id를 fallback으로 사용한다.
- 새 저장 job에는 request/correlation field를 포함한다. 이는 schema migration apply가 아니며, job store version을 올리지 않는다.
- unsupported future schema blocked diagnostics는 기존 `PCV_JOB_STORE_SCHEMA_UNSUPPORTED` no-mutation contract를 유지한다.

## 데이터 흐름

1. `Handle()` 진입 시 request id를 해석한다.
2. request id가 없으면 runtime이 `req-` prefix의 guid 기반 id를 만든다.
3. JSON response 공통 serializer가 `request_id`를 top-level JSON field로 넣는다.
4. `CreateJob()`은 현재 request id를 job의 `request_id`로 저장한다.
5. job 생성 응답의 `data.request_id`는 해당 job을 만든 API request id와 같다.
6. `ProcessOneQueuedJob()`은 provider mutation을 새로 만들지 않고 기존 queued job을 처리한다.
7. job result/failure는 기존 shape를 유지한다.
8. Retry job은 새 `request_id`를 갖고 원본 job의 `correlation_id` chain을 이어받는다.
9. Web Console Activity는 job row detail에 `request_id` 또는 `correlation_id`를 표시한다.

## 오류 처리

- malformed route id, missing body, invalid JSON, route not found, job not found 같은 API processor failure는 모두 top-level `request_id`를 포함한다.
- native adapter structured failure는 기존 `error.code/message/detail/retryable`를 유지한다.
- response shape 변경은 additive-only다.
- request id 자체가 비어 있거나 공백이면 새 id를 생성한다.

## 검증 기준

구현 시 기본 검증:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

문서만 변경할 때:

```powershell
git diff --check
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

이 후속은 실제 Hyper-V, service/MSI, firewall, Event Log, trust-store, LAN, update/rollback, config/job store migration apply를 실행하지 않는다.

## 완료 기준

- API success/failure body가 top-level `request_id`를 포함한다.
- queued job data가 request/correlation id를 보존한다.
- Web Console Activity가 request/correlation id를 선택적으로 표시한다.
- 기존 `ok/operation/data/error` contract와 old job store v1 load가 깨지지 않는다.
- public trusted signing, 외부 stable publication, public metrics를 완료 조건으로 주장하지 않는다.
