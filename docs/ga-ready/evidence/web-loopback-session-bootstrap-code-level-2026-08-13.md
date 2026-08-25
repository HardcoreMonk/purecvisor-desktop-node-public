# Web loopback session bootstrap code-level PASS (2026-08-13)

evidence_id: `web-loopback-session-bootstrap-code-level-2026-08-13`
result: `CODE_LEVEL_PASS`
Design-ID: `purecvisor-desktop-node-web-loopback-session-bootstrap-v1`
approval_locator: `User-Approval: web-loopback-session-bootstrap-20260813`
spec: `docs/superpowers/specs/2026-08-13-purecvisor-desktop-node-web-loopback-session-bootstrap-design.md`
plan: `docs/superpowers/plans/2026-08-13-purecvisor-desktop-node-web-loopback-session-bootstrap.md`
change_tier: `M`
verification_lane: `Full`
operational_current_changed: `false`
host_mutation_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

기본 `no-default-account` 설치의 loopback Web Console은 service token을 HTML/`pcv-config.js`에
넣지 않고 `POST /api/v1/auth/loopback-session`으로 짧은 JWT를 받아 보호 API를 호출한다.
자격 증명이 없으면 `refreshAll()`이 보호 route 11-way fan-out을 시작하지 않는다.
이 기록은 code-level 범위다. 설치본 E2E, 다음 package campaign, operational current
승격은 주장하지 않는다.

## 계약

- Route: `POST /api/v1/auth/loopback-session` / `CreateLoopbackSession` / family `auth` /
  `NoBearerTokenRequired`. catalog count `56`.
- 성공 envelope: `grant_type=loopback_session`, `typ=loopback_access` / `loopback_refresh`,
  `session.username=loopback-session`, `session.role=operator`.
- `accounts.json`은 발급 전후 account 수 `0`, `bootstrap_state=no-default-account`를 유지한다.
- `/pcv-config.js`는 `{ apiBaseUrl }`만 freeze한다. `access_token` / `protected_token` /
  service token 문자열이 없다.
- `Ready` 의미는 바꾸지 않는다. 계정 1개 이상이면 발급을 닫는다.

### 문제 코드

| code | HTTP | 의미 |
| --- | ---: | --- |
| `PCV_LOOPBACK_SESSION_NOT_LOOPBACK` | 403 | remote가 loopback이 아님 |
| `PCV_LOOPBACK_SESSION_DISABLED` | 409 발급 / 401 Host Ready reject | 계정이 구성되어 이 경로가 닫힘 |
| `PCV_ACCOUNT_AUTH_SIGNING_KEY_EMPTY` | 409 | signing key 없음 |
| `PCV_AUTH_REQUIRED` | 401 | 기존. 자격 증명 없음 |
| `PCV_ACCOUNT_AUTH_NOT_CONFIGURED` | 409 | 기존. 계정 login을 계정 없이 호출 |

401 fan-out은 닫혔다. 빈 세션에서 `refreshAll()`은 보호 step을 시작하지 않고
`connectionState='auth'`만 남긴다. loopback hostname이면 먼저
`ensureLoopbackSession()`이 `skipAuth: true`로 한 번 POST한다.

## 검증 결과

아래는 이 문서 작성 시점에 실제 실행해 관측한 결과다.

### 개별 필수 명령 (fixture `route_count` 55→56 이후)

| 검증 | 결과 | 시간 |
| --- | --- | ---: |
| `dotnet test src/DesktopNode.sln --nologo` | PASS `871/871` (Api 259, Host 204, HyperV 137, Runtime 126, Cli 113, Contracts 21, Service 11) | 25.869초 |
| `npm test --prefix web` | PASS (`served app.js is current`, `5 batches, 25 work items`) | 2.191초 |
| `npm run verify:parity --prefix web` | PASS (`static parity verification passed`, `browser fixture verification passed`) | 2.361초 |
| `npm run browser:fixture --prefix web` | PASS (`browser fixture verification passed`) | 0.911초 |
| `node --check web/app.js` | PASS | 0.049초 |
| `Invoke-Pester -Path web/tests -Output Detailed` | PASS `49/49` | 3.696초 |
| `git diff --check` | PASS (출력 없음) | 0.177초 |

최초 `dotnet test`는 `DesktopNodeHttpTransportContractTests.FixtureConnectsHostCharacterizationToAuthoritativeRouteManifest`가
fixture `route_count` `55` vs catalog `56`으로 실패했다. 승인된 catalog increment에 맞춰
`packaging/windows-desktop-node/tests/fixtures/http-transport-contract-v1.json`의 count만
`56`으로 고쳤다. 제품 동작은 바꾸지 않았다.

### Full lane

```powershell
& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath @(
    'src/DesktopNode.Api/DesktopNodeAccountAuth.cs',
    'src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs',
    'web/src/served/job-polling.ts') `
  -ArtifactRoot artifacts/development-verification-full-loopback-session-20260813-r2
```

`summary.json` `ok=true`, `change_tier=M`, `tier_reasons=api-cli-web-contract,cross-module-change`.

| Full suite | 결과 | 시간 |
| --- | --- | ---: |
| `dotnet` | PASS `871/871` | 24.274초 |
| `web-npm` | PASS | 4.531초 |
| `packaging-pester` | PASS `493/493` | 114.901초 |
| `installer-pester` | PASS `49/49` | 9.204초 |
| `web-pester` | PASS `49/49` | 3.721초 |
| `git-diff-check` | PASS | 0.045초 |
| `current-evidence-check` | PASS `8/8 current` | 0.604초 |

합계 suite duration은 157.631초다.

최초 Full lane은 `packaging-pester`에서
`src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` 실측 `496` / ceiling `495`로 실패했다.
라쳇을 올리지 않고 `RemoteIsLoopback`을 `Authorization`과 같은 줄의 마지막 파라미터로 흡수해
파일을 `495`줄에 유지했다. 재실행 r2가 PASS다.

## 게이트 정렬 (제품 동작 아님)

- Transport fixture `route_count` `55` → `56`. Task 1 catalog increment leftover.
- Request processor 라인 수 `496` → `495`. 모듈 분리나 ceiling 인상 없이 파라미터 한 줄을 합쳤다.

## 의도적으로 남긴 항목

- `!Ready`에서 service token 없는 `POST /api/v1/auth/login`은 Host `401`이다. transport
  `general-auth-before-known-length-cap` body-cap fixture가 body 읽기 전 `401`을 요구한다.
  첫 사용 성공 경로는 loopback-session이다.
- SDD ledger의 deferred minor는 이 slice에서 고치지 않았다.

## Nonclaims

- 설치본 Chromium/Playwright E2E를 required gate에 넣지 않았다.
- 다음 package / fullgate / manual-admin campaign을 열지 않았다.
- `docs/ga-ready/current-evidence.json`과 generated current block을 바꾸지 않았다.
- operational current는 `0.42.72-admin-smoke` 그대로다. `operational_current_changed=false`.
- PCVCLI `auth loopback-session` 명령을 추가하지 않았다. loopback-session은 Web-only다.
- public trusted signing과 외부 stable publication을 주장하지 않는다.
- host mutation, MSI, service 재시작, Hyper-V/VM mutation을 실행하지 않았다.
