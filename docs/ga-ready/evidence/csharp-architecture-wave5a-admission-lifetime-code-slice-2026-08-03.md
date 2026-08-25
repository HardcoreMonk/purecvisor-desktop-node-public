# C# 구조 개선 Wave 5A bounded admission/lifetime code slice

- 날짜: 2026-08-03
- 상태: `code_ready_operational_pending`
- 범위: 기존 `HttpListener` 기본 경로를 보존한 명시적 `tracked_async_serialized` opt-in slice
- host mutation: `false`
- installed service mutation: `false`
- ASP.NET Core package/framework reference: `not_changed`
- public trusted signing: `false`
- external stable publication: `false`

## 적용 내용

`DesktopNodeHostOptions`가 다음 host-owned 설정을 소유한다.

- `--request-lifetime-mode legacy|tracked_async_serialized` (기본값 `legacy`)
- `--request-admission-active` (기본값 `32`)
- `--request-admission-waiting` (기본값 `64`)

`tracked_async_serialized`를 선택하면 request body read 전에 bounded admission을 수행한다. active와
waiting capacity가 모두 소진되면 body를 읽지 않고 HTTP `503`과
`PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED`, `Retry-After`를 반환한다. static Web Console 요청과
OPTIONS/auth precedence는 기존 위치를 유지하고, noVNC request도 tracked task/admission 범위에 포함한다.

tracked request task는 host lifetime owner가 보관하고 shutdown 시 cancellation → listener stop →
tracked task drain 순서로 관찰한다. 기존 `legacy` listener에는 admission owner를 생성하지 않으므로
현재 설치본의 80/7777 HTTP.sys 계약은 바뀌지 않는다.

## 검증

| 검증 | 결과 |
|---|---|
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release --no-restore` | PASS, 186/186, skip 0 |
| `dotnet test src/DesktopNode.sln -c Release --no-restore` | PASS, 815/815, skip 0 |
| active/waiting admission unit tests | PASS |
| in-process tracked admission HTTP 503/Retry-After test | PASS |
| default legacy transport behavior | 기존 Host/transport tests PASS로 재확인 |

## 남은 Wave 5A 책임

이 slice는 Wave 5A 전체 완료가 아니다. end-to-end processor async 전환, client disconnect와 durable
enqueue commit 분리, 명시적 serialization gate, overload Web/PCVCLI parity, listener/worker fault
supervision, metrics/queue-age observation, installed load/shutdown/account/noVNC smoke는 후속 작업으로
남아 있다. ADR-0012 API read concurrency policy도 아직 작성/종결되지 않았다.

## ADR 번호 충돌 감사

현재 `docs/adr/0013-job-store-single-writer-transaction-lease.md`는 이미 적용 중인 job-store
결정이다. 따라서 ASP.NET Core server/rollout 결정은 계획에서 `ADR-0014`로 예약했으며, 기존 ADR-0013
번호를 재사용하지 않는다.

