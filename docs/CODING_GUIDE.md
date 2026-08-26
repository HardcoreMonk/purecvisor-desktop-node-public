# PureCVisor Desktop Node 통합 코딩 가이드

- 작성 기준: `2026-08-03`
- 마지막 갱신: `2026-08-26`
- 문서 상태: `current-derived-guide`
- 적용 저장소: Windows 전용 `purecvisor-desktop-node`
- 현재 실행 계획 진입점: `docs/DEVELOPER_INDEX.md`
- distribution boundary: `internal-private-network-only`
- public claims: `public_trusted_signing=false`, `external_stable_publication=false`
- 이 문서 작성의 host mutation: `false`

이 문서는 현재 계획, 적용 ADR, 검증 정책, 프로젝트 설정과 코드 관행을 한곳에서 읽기 위한 파생 가이드다.
새 설계 결정을 만들거나 canonical owner를 대체하지 않는다. 이 문서와 canonical owner가 충돌하면
canonical owner가 우선하며, 이 문서를 같은 변경에서 갱신한다.

## 1. 규칙 상태 읽는 법

| 표기 | 의미 |
|---|---|
| `현재 적용` | 지금 작성·검토하는 변경이 지켜야 하는 제품, 계약, 보안 또는 검증 규칙 |
| `자동 강제` | compiler, test, generator, CI 또는 Git 설정이 위반을 자동 탐지하는 규칙 |
| `현재 관행` | 현재 코드에서 일관되게 관찰되지만 전용 lint/analyzer로 전부 강제되지는 않는 작성 방식 |
| `향후 게이트` | 승인된 계획의 목표지만 선행 카드, ADR, 검증 또는 activation 전에는 현재 완료 규칙으로 주장할 수 없는 항목 |
| `범위 외` | 현재 계획에서 도입하거나 되살리면 안 되는 표면 |
| `역사적 참고` | 이전 phase/evidence의 당시 상태이며 현재 구현 권한이나 operational current가 아닌 항목 |

`현재 적용`이 항상 `자동 강제`라는 뜻은 아니다. 자동 검사가 없는 현재 적용 규칙은 코드 검토와
변경 evidence로 지켜야 한다.

## 2. 정보별 권위와 충돌 처리

전체 문서를 날짜순으로 섞어 해석하지 않는다. 정보 종류별 canonical owner를 읽는다.

| 정보 | canonical owner | 해석 규칙 |
|---|---|---|
| 저장소 작업·제품 경계 | 루트 `AGENTS.md` | Windows Desktop Node 범위와 로컬 작업 지침을 따른다. |
| operational current | `docs/ga-ready/current-evidence.json`과 JSON이 가리키는 evidence | version, operational MSI, payload, provenance, public claim을 exact tuple로 읽는다. 설치 또는 source PASS만으로 승격하지 않는다. |
| 적용 설계 결정 | `docs/ADR_INDEX.md`의 적용 목록과 해당 `docs/adr/*.md` | candidate, superseded, historical, `closed-not-adopted` 문서는 현재 구현 권한이 아니다. |
| 변경 등급 | `docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md` | 자동 최소 S/M/L 분류가 작업자가 요청한 낮은 등급보다 우선한다. |
| 검증 범위 | `docs/DEVELOPMENT_VERIFICATION_POLICY.md` | 변경 표면별 suite와 관리자 opt-in 경계를 따른다. |
| 현재 계획 포인터 | `docs/DEVELOPER_INDEX.md` | 현재 실행 계획을 찾는 진입점이며 operational tuple이나 ADR을 덮어쓰지 않는다. |
| 현재 실행 작업 | `docs/DEVELOPER_INDEX.md`의 최신 dated section | Wave A~E와 Required CI cutover가 완료됐다. Required CI는 `dotnet`, `web`, `delivery`, `installer-policy` 네 context이며 ledger 62 files / 627 contracts가 모두 CI PASS다. 다른 historical 계획을 암묵적으로 재활성화하지 않는다. |
| 미래 완료 제어 | Luna stable design과 2026-08-03 successor | materialization과 activation 전에는 설계/controller 정의다. |
| 주간 projection | 2026-08-03 1주 단위 서비스 개발 명세 | non-authoritative forecast이며 DAG, 승인 또는 mutable state owner가 아니다. |

충돌과 노후 문서는 다음과 같이 처리한다.

- `현재 적용`: 복사본의 `latest`/`current` 문구나 파일 날짜로 사실을 추론하지 않고 위 owner를 직접 읽는다.
- `현재 적용`: 중복 문서가 owner와 다르면 `stale`, `historical`, `supporting`, `closed-not-adopted` 중 하나로
  분류하고 owner link를 둔다. 값을 조용히 합치거나 평균하지 않는다.
- `현재 적용`: historical evidence는 현재 값에 맞춰 다시 쓰지 않는다. current-facing index 또는 파생 요약만
  정정한다.
- `현재 적용`: route 수처럼 snapshot 숫자가 다르면 authoritative source와 contract test로 재측정한다.
  현재 source snapshot은 55개지만 문서의 오래된 54개 행을 계약으로 사용하지 않으며, 숫자를 새 코드에
  중복 하드코딩하지 않는다.
- `현재 적용`: 계획 전환은 새 파일의 존재나 날짜가 아니라 materialization/activation 조건으로 판정한다.
- `현재 적용`: `WSD-B001`은 successor v3에서 resolved다.
- `향후 게이트`: `WSD-B002`는 bootstrap과 Luna Max routing amendment만 resolved다. 사용되지 않은 dbac
  materialization 승인은 stale이며, 이 파생 문서 merge 뒤 exact fresh-main 승인과 Luna selector
  resolution/callability 없이 `LC-001`을 시작하지 않는다. LC-026 effective-current attestation 전에는 제품
  카드를 시작하지 않는다.

## 3. 제품·언어·플랫폼 경계

### 3.1 현재 적용 스택

- Windows Desktop Node만 제품 범위다.
- 제품 core와 backend는 C# / .NET 10이다. Windows 연동 프로젝트는 `net10.0-windows`, 중립
  Contracts/Runtime/Service 프로젝트는 `net10.0`을 유지한다.
- Web Console source, build와 browser runtime은 TypeScript가 소유한다.
- C#/.NET `DesktopNode.Verification`과 Node가 현재 비관리자 Required CI를 소유한다. PowerShell 7과
  Pester는 legacy parity, packaging build 또는 승인된 manual/admin evidence runner에만 남는다.
  제품 runtime, deployment 또는 admin operation owner는 C# native 경계다.
- 활성 운영자 표면은 Web Console과 PCVCLI뿐이며 TUI는 absent다.
- 코드의 `native adapter`는 현재 C# `System.Management`/WMI 경계를 뜻하며 C++ 구현을 뜻하지 않는다.
- 현재 distribution은 내부 사설망 전용이다. `AllowUnsignedDev`/`LocalTest` evidence는 public trusted release
  또는 external stable publication evidence가 아니다.

### 3.2 범위 외

- Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime과 Single Edge 표면을 추가하지 않는다.
- TUI source, package, smoke 또는 운영자 경로를 재도입하지 않는다.
- C++23, CMake, 별도 native runtime prerequisite 또는 C++ interop 계층을 현재 제품·installer payload에
  추가하지 않는다.
- C++23 재검토에는 별도 ADR/design, 계획 revision과 사용자 승인이 필요하며 C# 구조 개선 또는
  ASP.NET Core 전환 PR에 섞지 않는다.
- 현재 Runtime이 소유하는 job state, schema, retry/cancel/recovery와 supervision을 C++ 도입 근거로
  중복 구현하지 않는다.
- public trusted signing, Winget, public stable URL과 external stable publication은 `blocked_out_of_scope`다.
  단순 실행 승인으로 열 수 없고 별도 ADR, program 범위 변경과 release approval이 모두 필요하다.

### 3.3 작성 시점 상태 snapshot

다음 값은 가이드 작성 시점 참고값일 뿐 canonical owner를 대체하지 않는다. 작업 시작 시 §2의 owner에서 다시
읽는다.

| 층 | 2026-08-03 snapshot |
|---|---|
| operational current | `0.42.65-admin-smoke` |
| installed non-promoted candidate | `0.42.68-admin-smoke` |
| current source route snapshot | 55; activation 시 source에서 재측정 |
| ASP.NET Core/successor | production 미적용; gated Wave 6/controller-definition |

## 4. C# 아키텍처와 소유권

### 4.1 프로젝트 경계

| 프로젝트/표면 | 소유 책임 |
|---|---|
| `DesktopNode.Contracts` | platform-neutral contract, DTO와 policy projection |
| `DesktopNode.Runtime` | job state transition, queue, durable store, cancellation/recovery |
| `DesktopNode.Api` | 호환 API façade, authoritative route registry, auth/session/RBAC, diagnostics와 query owner |
| `DesktopNode.HyperV` | domain/provider/dispatch와 WMI execution seam |
| `DesktopNode.Host` | composition root, Windows Service lifetime, HTTP transport와 admission; task tracking/supervision 작업 owner |
| `DesktopNode.Cli` | PCVCLI command/exit/output 호환 표면 |
| `DesktopNode.Service` | 새 production dependency를 추가하지 않는다. Wave 7에서 historical scaffold의 병합 또는 제거를 판정한다. |

- `현재 적용`: façade는 catalog lookup, dispatch와 public result/error 경계를 유지하고 실제 정책은 깊은 owner가
  소유한다. callback만 전달하는 얕은 wrapper는 삭제 테스트 후 `deepen`, `merge`, `delete` 중 하나를 택한다.
- `현재 적용`: diagnostics, auth/session/RBAC, ops query와 job runtime을 façade callback으로 되돌리지 않는다.
- `현재 적용`: owner 전용 test는 해당 owner 프로젝트에 두고 API route/status/JSON façade test는
  `DesktopNode.Api.Tests`에 둔다.
- `현재 적용`: composition root를 제외한 역방향 project reference와 transport별 application owner 복제를
  추가하지 않는다.
- `향후 게이트`: Hyper-V canonical registry, Host Ops family owner, evidence reader와 compiled architecture
  ratchet의 미완료 항목을 완료된 구조로 가정하지 않는다.

### 4.2 C# 작성 규칙

`자동 강제`되는 현재 project 설정은 `Nullable=enable`과 `ImplicitUsings=enable`이다. 모든 active project는
.NET 10 target을 사용한다.

새 코드와 수정 코드는 다음 현재 관행을 우선한다.

- file-scoped namespace, public type/member의 PascalCase, interface의 `I` prefix를 사용한다.
- private field는 camelCase를 사용하고 이름 충돌 시 `this.field`로 구분한다.
- 정책/값 객체에는 sealed class 또는 sealed record를 우선한다.
- 비동기 API에는 `Async` suffix를 사용하고 `CancellationToken`은 마지막 인자로 전달한다.
- library/owner 경계의 await는 기존 패턴에 맞춰 `ConfigureAwait(false)`를 사용한다. cleanup과 task 관찰에
  필요한 `CancellationToken.None`을 기계적으로 금지하지 않는다.
- constructor/interface seam과 `CreateDefault`/`CreateWithDependencies` 조립 패턴을 사용하고 내부 모든 객체를
  DI container 등록으로 옮기지 않는다.
- test는 xUnit `[Fact]`, 동작을 설명하는 PascalCase 이름, recording fake와 명시적 failure injection을 우선한다.

현재 루트 `.editorconfig`, `Directory.Build.props`, 명시적 `LangVersion`, warnings-as-errors와 analyzer package는
없다. 따라서 위 스타일을 자동 강제된 규칙으로 과장하지 않는다.

## 5. API·CLI·오류 호환성

- `현재 적용`: method/path, path normalization/decode, query 처리, auth/RBAC, exact status/body/content type,
  JSON key, product header, request ID, CLI exit code와 `PCV_*` 오류 계약을 보존한다.
- `현재 적용`: `ApiHandlerAdapterContract.CreateDefault()`와 관련 contract test가 route catalog의 source다.
  route 수를 Controllers, middleware, Web client 또는 문서 상수로 다시 소유하지 않는다.
- `현재 적용`: route/schema/public API version 또는 path/query 버그 수정은 owner 이동, lifetime 또는 transport
  변경 PR과 분리한다.
- `현재 적용`: 오류는 기존 structured `PCV_*` code, message, detail, retryable, recommended action과
  `{ok, operation, data, error}` envelope 의미를 유지한다.
- `현재 적용`: exception은 redaction한 뒤 request ID/correlation과 연결하고 credential, bearer token, JWT,
  `Authorization`, certificate private key/PFX/password를 log, fixture, DOM 또는 evidence에 남기지 않는다.
- `현재 적용`: diagnostics path containment, redaction, retention과 download header 계약을 owner 이동 전후로
  검증한다.

## 6. lifetime·동시성·영속성

- `현재 적용`: mutation queue consumer는 항상 하나이며 processor 진입의 직렬화 의미를 유지한다.
  ADR-0012의 bounded concurrent-read 대안은 `closed-not-adopted`다.
- `현재 적용`: 새 변경은 durable job을 client disconnect에 결합하거나 commit된 job을 연결 종료로 취소하는
  의미를 도입하면 안 된다. 명시적 job cancel, service stop 또는 reconciliation owner를 유지한다.
- `현재 적용`: uncertain mutation을 다른 transport/backend에서 자동 replay하거나 fallback 재실행하지 않는다.
- `현재 적용`: production 기본 lifetime과 transport는 계속 legacy `HttpListener` 경로다.
- `향후 게이트`: disconnect-before/after durable commit 경계, timeout/cancellation 뒤 late commit 0건, listener/
  worker/noVNC의 모든 child task·exception 관찰은 LT-001..LT-007과 installed 검증 전 완료로 주장하지 않는다.
- `향후 게이트`: `tracked_async_serialized`는 code-ready opt-in이며 active 32/waiting 64 admission, body read와
  per-context task 생성 전 `503 PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED`와 `Retry-After`, request/noVNC task
  tracking과 drain 계약을 가진다.
  installed load/shutdown/account/noVNC parity와 관찰이 닫히기 전 production 기본값 또는 완료로 표현하지 않는다.
- `향후 게이트`: shutdown은 새 admission 차단, request/body/noVNC drain, mutation worker와 reconciliation 확인,
  terminal persistence 확인, server dispose 순서와 service health/fault propagation을 검증해야 한다.

## 7. TypeScript Web Console

- `자동 강제`: `web/tsconfig.json`은 ES2022, ESM/Bundler, `strict`, `noEmit`,
  `verbatimModuleSyntax`, casing 검사를 사용한다.
- `자동 강제`: `npm test --prefix web`은 typecheck, served asset freshness와 frontend batch를 검사하고
  `npm run verify:parity --prefix web`은 static/browser parity를 검사한다.
- `현재 적용`: `web/src/served-app.ts`와 `web/src/served/**`가 source이며 `web/app.js`는 생성물이다.
  `web/app.js`를 직접 수정하지 않고 `npm run build:served --prefix web`로 갱신한다. TypeScript source는 ESM
  설정으로 검사하지만 served bundle은 현재 classic global-script output이다.
- `현재 적용`: TypeScript type은 server contract mirror이며 API source of truth가 아니다.
- `현재 적용`: token/JWT/Authorization 값과 private credential을 source, fixture, DOM, console 또는 artifact에
  기록하지 않는다.
- `현재 관행`: `served-app.ts`에는 `@ts-nocheck`, served module에는 명시적 `any`가 남아 있다. `strict=true`를
  Web 전체의 no-any 보장으로 표현하지 않으며 새 예외 확장은 별도 근거와 test를 남긴다.
- `향후 게이트`: ASP.NET Core는 기존 build output을 정적 제공할 뿐 TypeScript source/build/browser runtime을
  대체하지 않는다. 설치 host에 Node.js runtime 의존성을 추가하지 않는다.

## 8. PowerShell·packaging 작성 경계

- 제품 runtime/request/deployment/admin operation 경로에 generic PowerShell helper fallback이나 제품 실행
  dependency를 다시 추가하지 않는다. PowerShell은 비필수 legacy parity, repo-side build 또는 승인된
  manual/admin evidence runner 경계에만 둔다.
- 승인된 Guest Execution PowerShell Direct provider는 별도 security boundary이며 generic fallback 허가가 아니다.
- 새 또는 수정하는 script/module은 기존 패턴에 맞춰 `Set-StrictMode -Version Latest`,
  `$ErrorActionPreference = 'Stop'`, `[CmdletBinding()]`, typed/validated parameter, `-LiteralPath`와
  `Verb-PcvNoun` 이름을 우선한다.
- machine-readable output은 stable snake_case key와 명시적 상태를 사용하고 secret 원문을 포함하지 않는다.
- module은 필요한 public function만 `Export-ModuleMember`로 내보낸다.
- PowerShell formatting/naming은 PSScriptAnalyzer로 자동 강제되지 않는다. Pester PASS를 style lint PASS로
  해석하지 않는다.
- package build, install, service/firewall/TLS/trust-store/Event Log과 Hyper-V mutation runner는 서로 다른
  권한과 evidence 경계다.

## 9. Hyper-V와 Host Ops 안전 규칙

- `현재 적용`: public adapter, 34-operation snapshot, Host Ops 9-family/22-action snapshot, structured `PCV_*`
  error와 single mutation worker를 승인 없이 바꾸지 않는다. 실제 수는 activation/rebaseline 때 source에서
  다시 측정한다.
- `현재 적용`: 현재 parsing/validation/provider/result mapping과 façade behavior를 보존하며 owner 이동을
  동작 변경과 섞지 않는다.
- `현재 적용`: fake WMI test PASS를 actual provider 또는 actual-VM PASS로 주장하지 않는다.
- `현재 적용`: actual-VM/WMI 의미 변경은 operation별 pre-state, post-state, readback, cleanup과 failure evidence를
  요구한다.
- `현재 적용`: Host Ops action은 approval, dry-run, rollback diagnostics, redaction과 exact target 소유권 검사를
  보존한다.
- `현재 적용`: old/new implementation의 shadow mutation, 이중 실행과 uncertain retry를 금지한다.
- `향후 게이트`: Hyper-V operation을 한 canonical registry에서 Domain/Dispatch/Provider/API로 projection하고
  parsing/validation/provider/result mapping을 domain owner로 옮겨 façade를 dispatch/error boundary로 완결한다.
  Read-only provider부터 fake 가능한 WMI seam을 적용하며 Wave 4 완료 전 이 구조가 이미 완결됐다고 주장하지
  않는다.
- `향후 게이트`: Host Ops 9개 family는 각각 독립 rollback 가능한 commit에서 callback-free owner로 이동한다.

## 10. ASP.NET Core 도입 규칙

### 10.1 현재 상태

- `현재 적용`: production transport는 `System.Net.HttpListener`다.
- `향후 게이트`: ASP.NET Core 도입은 승인된 필수 Wave 6이지만 Wave 5A lifetime 완결, ADR-0014,
  transport parity와 package/installed gate 전에는 production 기본 경로가 아니다.
- `향후 게이트`: HTTP.sys는 1차 권장 후보이고 Kestrel은 비교 후보다. ADR-0014 전에는 selected server,
  endpoint/TLS owner 또는 server-specific production/package branch를 확정하지 않는다.
- `향후 게이트`: Wave 5A 완료 전에는 mutation-disabled fixture, 임시 data root와 동적 loopback port를 쓰는
  비제품 compatibility spike만 허용한다.

### 10.2 구현 불변조건

ASP.NET Core 구현 카드가 활성화된 뒤에도 다음을 지킨다.

- `service-action`은 `WebApplication` builder 생성 전에 분기하며 server, endpoint configuration 또는 hosted
  mutation worker를 만들지 않는다.
- 하나의 `WebApplication`/`WindowsServiceLifetime` composition root를 사용하고 nested host를 만들지 않는다.
- process 시작 전에 legacy 또는 ASP.NET Core transport 하나만 선택한다. 같은 process/product port 동시 bind,
  request mirror, automatic fallback/replay를 금지한다.
- API/Web listener 중 하나의 bind나 processor 생성이 실패하면 이미 열린 listener를 정리하고 service fault로
  전파한다.
- ASP.NET Core는 `/api` terminal transport adapter만 소유한다. Controllers/Minimal API endpoint 목록으로
  authoritative route catalog를 복제하지 않는다.
- adapter는 method, normalized path, raw body, request ID, client identity와 Authorization을 기존 request
  contract로 한 번만 mapping한다.
- API processor, job store/runtime, auth/revoke, rate limiter, admission과 mutation worker를 transport별로
  복제하지 않는다.
- 기존 response의 exact status/body/content type/product header를 전달한다. 자동 `ProblemDetails`, JSON
  재직렬화, redirect와 response compression을 제품 API에 적용하지 않는다.
- framework가 추가하는 `Date`, `Server`, connection/transfer header는 product header와 분리해 parity를
  판정한다.
- `RequestAborted`는 기존 request/job lifetime 분리를 재사용하고 endpoint별 mutation worker CTS를 만들지 않는다.
- raw 제품 argv를 `WebApplication.CreateBuilder(args)`에 넘기지 않고 검증된 `DesktopNodeHostOptions`만 endpoint를
  소유한다. Forwarded Headers는 별도 proxy/security 결정 없이 활성화하지 않는다.
- 명시적 `127.0.0.1:7777`, `127.0.0.1:80`과 승인된 LAN IP만 사용한다. wildcard, 암묵적
  `localhost:5000`, `--urls`, `ASPNETCORE_URLS`, `HTTP_PORTS`/`HTTPS_PORTS`, appsettings endpoint override를
  제품 binding source로 사용하지 않는다.
- endpoint branch는 loopback static terminal → Web-only rejection → OPTIONS → noVNC auth/Origin →
  service-token/account-ready pre-gate → non-loopback static terminal → normal API admission/body cap →
  processor-owned auth/RBAC/rate-limit/timeout 순서를 보존한다. CORS를 global static middleware로 확대하지
  않고 포화 상태에서도 OPTIONS 204와 기존
  auth-versus-body-cap 우선순위를 유지한다.
- known-length와 chunked body 모두 bounded admission 뒤 streaming cap+1 byte에서 중단하고 제품 소유
  `413 PCV_REQUEST_BODY_TOO_LARGE`를 반환한다. Framework generic 413이 제품 계약보다 먼저 응답하지 않게 한다.
- TypeScript build output만 static 제공하고 GET-only, `/`→`index.html`, no directory browsing/no implicit SPA
  fallback, web-root containment와 payload hash 계약을 보존한다. Content root는 `AppContext.BaseDirectory` 또는
  명시적 web root로 해석하고 Windows Service의 `C:\Windows\System32` current directory에 의존하지 않는다.
- IIS/IIS Express, Razor/MVC/Blazor, ASP.NET Identity, Entity Framework와 설치 Node.js runtime을 제품
  prerequisite로 추가하지 않는다.
- noVNC는 CORS가 아닌 별도 Origin/auth/RBAC/WebSocket 계약을 사용한다. missing/invalid Origin 최종 정책은
  legacy-first L/Release 결정 전 임의 기본값을 선택하지 않는다.

### 10.3 rollout

허용 순서는 `legacy_default → aspnet_opt_in → aspnet_default_legacy_retained → aspnet_only`다. Transport 추가,
기본값 전환, legacy 제거, package 승격과 관찰을 서로 다른 변경/gate로 둔다. ASP.NET Core parity는
TestServer만으로 닫지 않고 selected server의 실제 동적 loopback socket, static/noVNC/browser, self-contained
publish와 installed service를 각각 검증한다.

Payload가 바뀌면 진행 중 observation attempt를 immutable history에 `restarted`로 남기고 새 tuple/attempt로
7일을 다시 계산한다. Close에는 현재 attempt 7×24시간 이상, 최소 8 sample, 인접 gap 26시간 이하와 P0/P1
regression 0건이 필요하며 historical/restarted 시간을 합산하지 않는다.

## 11. 변경 분류·승인·완료 표현

### 11.1 최소 변경 등급

| 변경 | 최소 등급/레인 |
|---|---|
| 한 owner 내부의 비계약 구현·test | `S / Fast` 후보 |
| API/CLI/Web 계약 표면, cross-module owner 이동, 일반 packaging | `M / Full` 이상 |
| auth/JWT/security, lifetime/cancellation 의미, HTTP transport/TLS, installer, current evidence, WMI 의미, host mutation | `L / Release` |

실제 등급은 `Resolve-PcvDevelopmentChangeTier` 결과를 사용한다. 불분명한 경로는 검증 레인을 `Full`로
올리며, plan/card가 자동 최소 등급을 낮출 수 없다.

별도 승인된 successor control materialization의 LC 카드부터 모든 successor card에 적용하는 모델
routing은 S/M `gpt-5.6-luna`/`max`, S/Fast와 M/Full, L/Release
`gpt-5.6-sol`/`high|ultra`다. 관찰된 UI label `Luna Max (gpt-5/6-luna, max)`의 slash alias는
card `execution_model`이 아니다. LC-001 전에 alias가 canonical `gpt-5.6-luna`로 resolve되는 durable
evidence를 남기며 불일치는 `blocked/model_identifier_unresolved`다. Luna가 unavailable하면 bootstrap부터
Sol/Terra로 자동 대체하지 않고 `blocked/model_unavailable`로 중단한다. 제품 카드 실행은 LC-026
effective-current attestation 뒤에만 허용되며, 이 routing은 그 전에 현재 predecessor의 제품 실행 권한을
바꾸지 않는다.

### 11.2 승인 경계

- source/code 작업 승인, Git/PR 승인과 문서 승인은 package build/install 또는 host mutation 승인이 아니다.
- package build, 설치, service/HTTP/TLS/firewall/trust-store, Hyper-V actual VM과 lifecycle rollback은
  각각 필요한 명시 승인을 확인한다.
- public trusted signing, Winget과 external stable publication은 현재 승인 category가 아니라
  `blocked_out_of_scope`다. 별도 ADR/program 범위 변경과 release approval 전에는 실행하지 않는다.
- 승인된 exact HEAD, artifact hash, command, host/port/VM target 또는 rollback 절차가 달라지면 재승인한다.
- runner가 승인 artifact를 내부에서 다시 build하거나 uncertain mutation을 자동 재실행하지 않는다.

### 11.3 상태 표현

다음 상태를 서로 바꾸어 쓰지 않는다.

- source/test가 통과한 `code_complete` 또는 `code_ready_operational_pending`
- 만들어졌지만 승격되지 않은 package candidate
- 설치되어 smoke가 통과한 installed non-promoted candidate
- `current-evidence.json`이 소유하는 operational current
- package/installed/operational promotion complete

Evidence는 삭제하지 않고 `current`, `historical`, `supporting`, `closed-not-adopted`로 재분류한다. 문서에는
source/test commit, actual-host 여부, `host_mutation_performed`, public claims와 rollback 경계를 정직하게 기록한다.

## 12. 검증 가이드

변경 전 owner와 contract를 정하고 focused characterization/RED test를 먼저 둔 뒤 전체 regression과 등급별
lane을 실행한다.

| 변경 표면 | 기본 검증 |
|---|---|
| C# owner | 변경 중 focused test project, clean committed HEAD에서 Required CI `dotnet` shard |
| Web/TypeScript | `npm test --prefix web`, `npm run verify:parity --prefix web`; 생성물을 바꿀 때 build/freshness 확인 |
| Required CI | `DesktopNode.Verification`의 `dotnet`, `web`, `delivery`, `installer-policy` 네 shard; ledger 62 files / 627 contracts 전체 `cutover / local pass / CI pass` |
| Web required entrypoint | `npm run test:required --prefix web` |
| PowerShell/Pester 표면 | 비필수 legacy parity 또는 승인된 manual/admin 검증에만 관련 suite 실행 |
| current evidence | `delivery`/`installer-policy` shard와 canonical current-evidence owner 검증; mutation runner는 별도 관리자 승인 |
| 문서·모든 변경 | 실제 changed-path 분류, 필요한 root boundary test, `git diff --check` |
| ASP.NET Core | legacy/ASP.NET parity, TestServer, selected-server 실제 socket, static/noVNC/browser와 publish/installed gate |
| Hyper-V/Host Ops | fake/focused suite; 별도 승인된 경우에만 actual-host/actual-VM runner |

Canonical non-admin lane runner는 `DesktopNode.Verification`이다. 변경 중에는 다음 pre-commit
검증으로 Release 산출물과 Web dependency를 준비하고 focused regression을 확인한다.

```text
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
npm ci --prefix web
npm run test:required --prefix web
git diff --check
```

전체 solution test의 `policy-boundaries`는 활성 cutover 계약상 clean committed HEAD를 요구한다.
변경 중에는 영향 범위의 focused test만 실행한다. Clean committed HEAD에서 전체 solution test는
`dotnet` shard가, Installer 필터와 clean-worktree policy boundary는 `installer-policy` shard가 검증한다.

`installer-policy` shard는 cutover 경계 때문에 clean committed HEAD를 요구한다. 커밋 후
`git status --short` 출력이 비어 있는 상태에서 Required CI exact four를 모두 실행한다. `web`
shard가 `npm run test:required`를 포함하므로 별도로 다시 실행하지 않는다.

```text
git status --short
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/local-dotnet --shard dotnet
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/local-web --shard web
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/local-delivery --shard delivery
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/local-installer-policy --shard installer-policy
```

위 `.ps1` 값은 shard 선택용 changed-path 데이터이며 PowerShell process 호출이 아니다.

`--plan-only`는 suite 선택 확인일 뿐 PASS evidence가 아니다. Runner를 실제 실행했을 때만 해당 lane 결과를
주장한다.

## 13. 현재 자동 강제와 공백

### 13.1 자동 강제

- GitHub Development Gates는 .NET SDK `10.0.x` Release solution test를 실행한다.
- Node 24에서 npm clean install, TypeScript test와 static/browser parity를 실행한다.
- Development Gates는 `DesktopNode.Verification`의 정확한 네 shard를 실행하며 Required CI의
  Pester 및 비관리자 PowerShell process invocation은 각각 `0`이다.
- current evidence generated 문서가 canonical JSON과 일치하는지 검사한다.
- served TypeScript source와 committed `web/app.js`의 byte freshness/parity를 검사한다.
- 대형 모듈 라인 수 라쳇을 강제한다. `packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`이
  `fixtures/module-size-ratchet.json`의 `max_lines`를 상한으로 검사하고, 모듈이 `slack_lines`(`50`)
  이상 줄면 상한을 실제 값으로 낮추도록 요구한다. 라쳇은 한 방향으로만 움직이므로 순증은 실패한다.
- `.gitattributes`는 기본 LF, `.ps1`/`.psm1` CRLF 등 파일별 EOL을 정규화한다.

이 자동 검사는 pull request, `main` push, manual dispatch에서 동작한다. Public `main` branch
protection은 `strict=true`, admin enforcement와 exact four required contexts를 강제한다.
`merge_group` trigger는 구성된 것으로 주장하지 않는다.

### 13.2 정책상 필수지만 일반 PR CI에서 항상 직접 실행되지는 않음

- changed-path S/M/L 분류에 따른 실제 Fast/Full/Release runner
- `git diff --check`
- 변경 유형별 actual-host 또는 installed gate
- package/current-evidence promotion과 관찰
- Wave B의 별도 `test:web-contracts`와 `verify:web-contract-negative-parity` command는 현재
  `npm run test:required --prefix web`과 Required CI `web` shard의 supporting 검증이다.

Development Gates는 plan-only가 아니라 실제 네 shard를 실행한다. 로컬 `--plan-only` 결과는 suite
선택 확인일 뿐 실제 Fast/Full/Release lane PASS와 혼동하지 않는다.

### 13.3 아직 자동 강제되지 않음

- root `.editorconfig`, 공통 MSBuild props/targets와 중앙 package policy
- 명시적 C# language version, warnings-as-errors, analyzer/code-style gate
- ESLint/Prettier/Biome와 no-explicit-any gate
- PSScriptAnalyzer
- repository-wide formatter clean gate

`향후 게이트`: QG-110 이후 shared nullable/analyzer/warnings, compiled architecture rule, touched-project
line/branch coverage `0.0%p` 하락 방지와 hotspot ratchet을 도입한다. 구현 전에는 현재 CI 규칙으로 표현하지
않는다. 여기서 말하는 ratchet은 coverage/hotspot 대상이며, 이미 구현된 §13.1의 모듈 라인 수
라쳇과는 다른 항목이다.

## 14. PR 체크리스트

- [ ] canonical owner와 현재 실행 계획을 확인했다.
- [ ] Windows Desktop Node/Web Console/PCVCLI 경계를 지켰고 TUI, Linux runtime, C++23을 추가하지 않았다.
- [ ] 변경 책임이 한 owner에 있고 얕은 callback wrapper나 역방향 dependency를 늘리지 않았다.
- [ ] route/JSON/auth/error/CLI/request-id 계약 변경 여부를 명시하고 구조·transport 변경과 섞지 않았다.
- [ ] durable job, single mutation consumer, cancellation, replay 금지 불변조건을 지켰다.
- [ ] generated asset은 source에서 재생성했고 secret/redaction 경계를 확인했다.
- [ ] 실제 changed-path 자동 등급과 필요한 focused/전체/lane 검증을 실행했다.
- [ ] code/package/installed/operational 상태와 `host_mutation_performed`를 구분했다.
- [ ] package/install/service/TLS/Hyper-V mutation이 있으면 별도 명시 승인과 rollback/evidence를 확인했다.
- [ ] 적용 ADR, plan activation 또는 계약 owner 변경 시 이 가이드의 상태 태그와 근거를 갱신했다.

## 15. 알려진 미결정과 갱신 trigger

현재 조용한 기본값 선택을 금지하는 항목은 다음과 같다.

- `WSD-B001`: successor v3에서 resolved; §5.2/§19 SW-01 seed와 projection digest 유지
- `WSD-B002`: bootstrap/Max routing amendment resolved, fresh-main materialization 승인과
  selector resolution, materialization/activation/attestation 미완료
- Wave 5A 전체 lifetime/installed load·shutdown·account/noVNC 검증과 legacy mode 제거
- account-ready service bearer 해석의 기존 문서/구현 불일치와 별도 security policy 결정
- ADR-0014 selected server, endpoint/TLS owner와 rollout 세부 결정
- legacy-first noVNC allowed/invalid/missing Origin 정책
- activation-time route/Hyper-V operation/Host Ops action source rebaseline

다음 사건이 발생하면 이 문서를 같은 변경에서 갱신한다.

- ADR-0014 적용 또는 다른 applied ADR이 이 문서 경계를 supersede
- Wave 5A `code_complete`, ASP.NET Core rollout 단계 또는 operational current 변경
- successor materialization/activation과 current plan pointer 변경
- route/CLI/Web contract digest 또는 활성 운영자 표면 변경
- QG-110 analyzer/coverage/architecture gate 적용
- 별도 ADR을 통한 C++23 재검토 승인

## 16. 핵심 근거 문서

- `docs/ADR_INDEX.md`
- `docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/ga-ready/current-evidence.json`
- `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-improvement.md`
- `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-luna-completion-control-design.md`
- `docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md`
- `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-weekly-service-development-spec.md`
