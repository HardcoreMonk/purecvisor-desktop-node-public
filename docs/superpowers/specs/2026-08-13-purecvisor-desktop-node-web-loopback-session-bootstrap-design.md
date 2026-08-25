# PureCVisor Desktop Node Web Loopback Session Bootstrap 설계

- Design-ID: `purecvisor-desktop-node-web-loopback-session-bootstrap-v1`
- 작성 기준: `2026-08-13`, `Asia/Seoul`
- 문서 상태: `approved-design`
- 승인 locator: `User-Approval: web-loopback-session-bootstrap-20260813`
- 기준 커밋: `88cc6df5c8afce9176c1d96ffac7a26c014e6830` (`origin/main`)
- 운영 앵커: `0.42.72-admin-smoke`
- 선행 평가: `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` §8 P0-1, P0-3
- 선행 설계: `docs/superpowers/specs/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness-design.md` (가짜 상태 표시는 닫힘, bootstrap/401 fan-out은 비목표로 남김)
- 기존 account 계약: `docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-account-rbac-jwt-console.md`
- 이 설계가 수행하는 host mutation: `false`
- 변경 등급: `M` (`api-cli-web-contract`)
- 최소 검증 레인: `Full`
- public trusted signing: `false`
- external stable publication: `false`

## 1. 문제

기본 설치는 이미 인증 뼈대를 갖추고 있다. 그러나 Web Console의 설치 직후 경로가 닫혀 있다.

| 사실 | 소유 |
| --- | --- |
| `configure-installed`가 `accounts.json`과 `jwt-signing-key.txt`를 만든다 | `DesktopNodeHostServiceAction.EnsureAccountAuthBootstrapFiles` |
| `accounts.json`은 계정 0개, `bootstrap_state=no-default-account` | 같은 함수, Host 테스트가 고정 |
| `DesktopNodeAccountAuthOptions.Ready`는 계정이 1개 이상일 때만 true | `DesktopNodeAccountAuth.cs` |
| Ready가 false면 Host `Authorize()`는 service bearer와 문자 단위로 같은 값만 통과시킨다 | `DesktopNodeHostApplication.StaticAuth.cs` |
| JWT 검증도 Ready가 아니면 `PCV_ACCOUNT_AUTH_NOT_CONFIGURED` | `ValidateAccessToken` |
| `/pcv-config.js`는 `apiBaseUrl`만 넣는다 | `WriteWebConfigScriptAsync` |
| Web은 로그인 폼과 browser token 입력, `sessionStorage` 복원을 이미 가진다 | `web/src/served/state.ts`, `render-panels.ts` |
| `refreshAll()`은 자격 증명 없이 보호 route 11단계를 `Promise.allSettled`로 동시에 호출한다 | `web/src/served/job-polling.ts` |

설치본 기본 경로에는 계정이 없으므로 로그인 폼은 `409 PCV_ACCOUNT_AUTH_NOT_CONFIGURED`로 실패한다.
service token은 DPAPI LocalMachine 파일에만 있고 Web/HTML에 주입하지 않는다. 운영자가 token을
직접 붙여넣기 전에는 보호 API가 전부 `401 PCV_AUTH_REQUIRED`다. CLI는 같은 파일을 읽을 수 있어
이미 정상 경로다.

2026-08-05 진실성 slice는 이 상태에서 가짜 `Connected`/`VM: 3/3`을 제거했다. 인증 자체와 401
fan-out은 닫지 않았다.

## 2. 목표와 비목표

### 목표

- 기본 설치, loopback Web Console은 service token을 HTML/`pcv-config.js`에 넣지 않고 첫 로드에서
  짧은 수명 session을 받아 보호 API를 호출할 수 있다.
- 401이 나기 전에 fan-out을 멈추고, 자격 증명이 없으면 단일 auth gate만 보여 준다.
- 계정이 구성되면 loopback session 발급을 닫고 기존 `POST /api/v1/auth/login`이 유일한 Web
  계정 경로가 된다.
- LAN과 비-loopback remote는 기존처럼 service token 또는 계정 JWT가 필요하다.

### 비목표

- 설치 시 기본 account/비밀번호 생성
- service token을 static config, HTML, `pcv-config.js`에 넣기
- loopback이면 bearer 없이 전 API 개방
- Playwright 설치본 E2E required gate (다음 slice)
- `0.42.73` package/fullgate/manual-admin campaign
- PCVCLI `auth loopback-session` 명령
- ASP.NET Core transport, Luna card, TUI 복원
- 시각 디자인/레이아웃 개편
- public trusted signing, 외부 배포

## 3. 검토한 접근

| 접근 | 판정 |
| --- | --- |
| A. loopback one-time session (짧은 JWT) | **채택.** 기존 `jwt-signing-key.txt`와 Web `sessionStorage`를 재사용한다. 변경 등급 M. |
| B. 설치 시 기본 account | 거부. MSI/configure, 비밀번호, 복구 표면이 L이고 campaign을 강제한다. |
| C. loopback remote면 token 없이 API 개방 | 거부. 같은 기기의 모든 로컬 프로세스가 Local API를 쓰게 된다. |

사용자 승인: `User-Approval: web-loopback-session-bootstrap-20260813`.

## 4. 결정

### 4.1 새 route는 loopback + 계정 미구성에서만 발급한다

`POST /api/v1/auth/loopback-session`

- HTTP method/path는 이 한 쌍만 허용한다. GET으로 발급하지 않는다.
- Host listener와 API handler가 **둘 다** remote loopback을 검사한다. `X-Forwarded-For`와
  `X-Real-IP`는 보지 않는다.
- remote loopback 판정: `HttpListenerRequest.RemoteEndPoint.Address`가 `127.0.0.1`, `::1`,
  또는 IPv4-mapped `::ffff:127.0.0.1`이다. listener prefix가 LAN이어도 remote가 loopback이면
  허용한다. remote가 loopback이 아니면 `403 PCV_LOOPBACK_SESSION_NOT_LOOPBACK`.
- `DesktopNodeAccountAuthOptions.Ready == true`(계정 1개 이상)이면
  `409 PCV_LOOPBACK_SESSION_DISABLED`. 권고 action: 기존 login을 사용한다.
- signing key 파일이 없거나 비면 `409 PCV_ACCOUNT_AUTH_SIGNING_KEY_EMPTY`. 기본
  `configure-installed` 경로에서는 발생하지 않아야 한다.
- 요청 body는 없다. body가 있어도 무시하고 계정/비밀번호를 받지 않는다.
- 성공 응답 envelope는 기존 login과 같은 token pair다. `applyAccountSessionPayload()`를 재사용한다.

성공 `data` 필수 키:

- `access_token`, `refresh_token`, `token_type=Bearer`
- `expires_in`, `refresh_expires_in`, `access_expires_at`, `refresh_expires_at`
- `session.username=loopback-session`
- `session.role=operator`
- `session.subject=loopback-session`
- `grant_type=loopback_session`

TTL은 기존 account JWT와 같다. access 15분, refresh 8시간. 새 시계를 만들지 않는다.

### 4.2 JWT는 계정을 만들지 않고 메모리 principal만 쓴다

`accounts.json`에 사용자를 쓰지 않는다. `bootstrap_state=no-default-account`를 유지한다.

발급 시 메모리 전용 principal:

- `id` / `username` / `sub`: `loopback-session`
- `role`: `operator`
- `display_name`: `Loopback session`
- `permissions`: 기존 `operator` permission set
- access `typ`: `loopback_access`
- refresh `typ`: `loopback_refresh`

`typ=access`/`refresh` 계정 토큰과 섞지 않는다. 계정이 생긴 뒤 남은 loopback token은 Host가
거절한다.

`IssueToken` 경로와 HS256, issuer, audience는 기존 `DesktopNodeAccountAuthService`를 재사용한다.
signing key가 있을 때만 발급한다. `Ready` 조건은 발급/검증에서 분리한다.

- `Ready`: 계정 로그인/RBAC takeover (기존 의미 유지)
- `CanIssueLoopbackSession`: `Enabled && SigningKey` 비어 있지 않음 && `!Ready`

### 4.3 Host Authorize를 loopback JWT와 발급 route만 연다

현재 `accountAuthReady == false`이면 Host는 service token exact match만 통과시킨다. 이 설계는
그 분기를 다음으로 바꾼다.

| 조건 | 결과 |
| --- | --- |
| static Web asset, `/pcv-config.js` | 기존처럼 통과. token 없음 |
| `POST /api/v1/auth/loopback-session` 이고 remote loopback 이고 `!Ready` | bearer 없이 통과 |
| `POST /api/v1/auth/refresh` 이고 `!Ready` | bearer 없이 통과. handler가 refresh token을 검증한다 |
| `POST /api/v1/auth/login` 또는 `/logout` | 기존처럼 bearer 없이 통과. login은 Ready가 아니면 409 |
| `GET /api/v1/runtime/policy` | **계속 token/session 필요.** 발급 allowlist에 넣지 않는다 |
| bearer가 service token과 일치 | 기존처럼 통과 |
| bearer가 유효한 `loopback_access` 이고 `!Ready` 이고 remote loopback | 통과 |
| bearer가 `loopback_access` 이고 remote가 loopback이 아님 | `403 PCV_LOOPBACK_SESSION_NOT_LOOPBACK` |
| `Ready == true` 이고 bearer가 `loopback_*` | `401 PCV_LOOPBACK_SESSION_DISABLED` (계정 경로로 전환) |
| 그 외 누락/불일치 bearer | 기존 `401 PCV_AUTH_REQUIRED` / `403 PCV_AUTH_FORBIDDEN` |

`Ready == true`일 때 기존 동작은 유지한다. 계정 JWT는 API `AuthSessionHandler.Authorize`가
검사하고, Host는 account bootstrap path와 JWT를 API로 넘긴다.

API `AuthSessionHandler.Authorize`는 `!Ready`일 때 지금처럼 RBAC를 적용하지 않는다. loopback
session의 권한은 기본 설치의 service token과 같다. `session.role=operator`는 Web 표시용이다.
`Ready`가 된 뒤에는 기존 RBAC가 권위다.

### 4.4 Remote loopback은 request 필드로 전달한다

`DesktopNodeApiRequest`에 `bool RemoteIsLoopback = false`를 추가한다. Host가
`RemoteEndPoint`로 채워 넣는다. `ClientIdentity` 문자열 파싱에 의존하지 않는다.

API 테스트는 이 필드를 직접 넣는다. 필드를 생략하면 false이며 발급은 거절된다.

### 4.5 Refresh는 같은 loopback 경계에서만 된다

`POST /api/v1/auth/refresh`는 `typ=loopback_refresh`를 받을 수 있다.

- `!Ready`이고 signing key가 있고 remote loopback일 때만 새 `loopback_access`/`loopback_refresh`를 준다
- remote가 loopback이 아니면 `403 PCV_LOOPBACK_SESSION_NOT_LOOPBACK`
- `Ready`이면 기존처럼 계정 refresh만 유효하다. loopback refresh는 실패한다
- 계정 refresh와 같이 사용된 refresh `jti`는 revoke한다

### 4.6 Web은 자격 증명 없이 보호 route를 호출하지 않는다

초기화 순서:

1. 기존 `loadAccountSessionFromStorage()`
2. `ensureLoopbackSession()`
3. `refreshAll()`

`ensureLoopbackSession()` 규칙:

- `state.authAccessToken` 또는 `state.apiToken`이 있으면 no-op
- `window.location.hostname`이 `127.0.0.1`, `localhost`, `[::1]`, `::1`가 아니면 API를 호출하지
  않는다. LAN Web에서 발급을 시도하지 않는다
- loopback이면 `POST /api/v1/auth/loopback-session`을 `skipAuth: true`로 한 번 호출한다
- 성공하면 `applyAccountSessionPayload()` → 기존 `sessionStorage` 키
  `pcvDesktopAccountSession.v1`에 저장한다
- `409 PCV_LOOPBACK_SESSION_DISABLED`이면 login gate
- 그 외 실패는 auth gate + `state.authError`

`refreshAll()` 규칙:

- 시작 전 `ensureLoopbackSession()`이 끝나야 한다
- `authAccessToken`과 `apiToken`이 모두 비면 보호 step을 시작하지 않는다.
  `connectionState='auth'`, `lastRefreshedAt=Date.now()`, `partialFailures=[]`
- 자격 증명이 있으면 기존 step을 실행한다. 어느 step이든 `isAuthError`면 남은 step을
  시작하지 않는다. 이미 떠난 in-flight는 abort한다
- `Promise.allSettled`로 자격 증명 없는 11-way fan-out을 다시 만들지 않는다

Auth gate 표시:

- 새 페이지를 만들지 않는다
- `connectionState==='auth'`이고 세션/browser token이 없으면 현재 뷰 상단에 기존
  `#account-login-form`과 browser token 입력을 보여 준다
- copy는 새 마케팅 문구를 만들지 않는다. problem code와 기존 Troubleshooting 안내를 재사용한다
- Clear browser token / logout는 기존처럼 session만 지운다. 다음 로드에서 loopback이면 다시
  발급을 시도한다

`/pcv-config.js`는 계속 `{ apiBaseUrl }`만 freeze한다.

### 4.7 Runtime policy와 route catalog

`CreateRuntimePolicy`가 `!Ready`이고 signing key가 있으면 다음을 추가한다.

- `loopback_session_available: true`
- `grant_types`에 `loopback_session` 포함

`Ready`이면 `loopback_session_available: false`이고 grant에서 뺀다.

`ApiHandlerAdapterContract`에 한 줄을 추가한다.

- `POST /api/v1/auth/loopback-session` / `CreateLoopbackSession` / family `auth` /
  `NoBearerTokenRequired` / `requiredPermission: null`

기존 login/refresh/logout/session/rbac 계약을 바꾸지 않는다.

### 4.8 문제 코드

| code | HTTP | 의미 |
| --- | ---: | --- |
| `PCV_LOOPBACK_SESSION_NOT_LOOPBACK` | 403 | remote가 loopback이 아님 |
| `PCV_LOOPBACK_SESSION_DISABLED` | 409 | 계정이 구성되어 이 경로가 닫힘 |
| `PCV_ACCOUNT_AUTH_SIGNING_KEY_EMPTY` | 409 | signing key 없음 (기존 메시지 재사용 가능) |
| `PCV_AUTH_REQUIRED` | 401 | 기존. 자격 증명 없음 |
| `PCV_ACCOUNT_AUTH_NOT_CONFIGURED` | 409 | 기존. 계정 login을 계정 없이 호출 |

새 code를 기존 `PCV_AUTH_*`에 섞어 재해석하지 않는다.

## 5. 변경 지점

| 파일 | 변경 |
| --- | --- |
| `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` | `DesktopNodeApiRequest.RemoteIsLoopback` |
| `src/DesktopNode.Api/DesktopNodeAccountAuth.cs` | loopback 발급/검증/refresh, `CanIssueLoopbackSession`, runtime policy flag |
| `src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs` | `POST /api/v1/auth/loopback-session`, refresh의 loopback 분기 |
| `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` | route catalog 1행 |
| `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs` | 필요 시 auth family match만. 별도 family를 만들지 않는다 |
| `src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs` | Authorize allowlist와 loopback JWT 수락 |
| `src/DesktopNode.Host/DesktopNodeHostApplication.Request.cs` | `RemoteIsLoopback` 전달 |
| `src/DesktopNode.Api.Tests/**`, `src/DesktopNode.Host.Tests/**` | 아래 테스트 |
| `web/src/served/routes.ts` | route descriptor 1행 |
| `web/src/served/api-client.ts` | `createLoopbackSession()` |
| `web/src/served/job-polling.ts` 또는 `actions.ts` | `ensureLoopbackSession`, `refreshAll` 게이트 |
| `web/src/served/render-*.ts` | auth gate를 기존 form으로 노출 |
| `web/app.js` | `build-served-asset.mjs --write` 생성물만 |
| `web/tests/**`, `web/scripts/verify-browser-fixture.mjs` | 미인증 fan-out 금지, loopback 성공 경로 |
| `docs/CLI_COMMAND_USAGE.md` 등 | **CLI 명령은 추가하지 않는다.** 계약 문서에 Web-only 경계를 한 줄로 적는다 |

## 6. 테스트 계약

구현 PR은 RED → GREEN이다. skip으로 기대를 숨기지 않는다.

Host / API:

- loopback + `!Ready` + signing key: `POST /api/v1/auth/loopback-session` → 200, token pair,
  `grant_type=loopback_session`, `typ`이 `loopback_access`/`loopback_refresh`
- 같은 토큰으로 `GET /api/v1/host/status` → 200
- `RemoteIsLoopback=false`: 403 `PCV_LOOPBACK_SESSION_NOT_LOOPBACK`, 발급 0
- 계정 1개를 넣은 Ready 구성: 409 `PCV_LOOPBACK_SESSION_DISABLED`
- Ready 전환 후 기존 loopback access로 보호 route: 401/409, service token과 계정 login은 통과
- loopback refresh는 loopback에서만 새 pair를 준다
- `/pcv-config.js` 본문에 token/protected_token/access_token 문자열이 없다
- `accounts.json` 발급 전후 account 수 0, `bootstrap_state` 불변

Web fixture / static:

- 자격 증명 없이 `refreshAll`이 보호 route를 호출하지 않는다
- loopback hostname에서 bootstrap 성공 후 `refreshAll`이 보호 route를 호출한다
- 비-loopback hostname에서 bootstrap POST를 호출하지 않는다
- `index.html`/`pcv-config` 생성 경로에 service token이 없다
- 기존 미인증 `—` 표시 가드는 유지한다

검증 명령 (구현 PR):

```powershell
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
Invoke-Pester -Path web/tests -Output Detailed
& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath @('src/DesktopNode.Api/DesktopNodeAccountAuth.cs','web/src/served/job-polling.ts')
git diff --check
```

설치본 mutation, MSI, service 재시작은 이 slice에서 실행하지 않는다.

## 7. 완료 조건과 다음 slice

이 설계의 구현이 끝나려면:

- 위 테스트가 비상승 계정에서 PASS
- 기본 설치와 동일한 `no-default-account` fixture에서 loopback Web이 로그인 폼 비밀번호 없이
  host/vm/ops read를 받는다
- 401 fan-out이 0
- current-evidence, MSI SHA, package pair를 이 PR이 바꾸지 않는다

다음 slice (이 설계 밖):

1. 설치본 Chromium E2E를 required gate에 추가
2. 그 결과가 source로 닫힌 뒤에만 `0.42.72 -> 0.42.73` campaign을 연다. PR #189 diagnostics
   list가 그때까지 merge되어 있으면 같은 payload에 넣는다

## 8. 위험과 함정

- Host allowlist만 열고 API 검증을 빼면 테스트 더블이 Host를 우회해 발급할 수 있다. 두 층을
  모두 잠근다.
- `Ready`를 “signing key만 있으면 true”로 바꾸면 빈 계정 설치에서 login이 열리고 기존 409
  계약이 깨진다. `Ready` 의미를 바꾸지 않는다.
- loopback JWT를 `typ=access`로 발급하면 계정 전환 후 검증 경로가 섞인다. `loopback_access`를
  쓴다.
- Web이 hostname만 보고 LAN API에 bootstrap을 치면 403이 난다. hostname이 loopback이 아니면
  POST 자체를 하지 않는다.
- `ClientIdentity` 문자열 파싱은 spoof/포맷 위험이 있다. `RemoteIsLoopback` bool만 믿는다.
- 이 문서는 operational current를 승격하지 않는다.
