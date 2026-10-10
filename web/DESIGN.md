# PureCVisor Desktop Node Web DESIGN.md

이 문서는 Windows Desktop Node Web Console의 시각/프론트엔드 규격이다. 제품
운영 절차는 `docs/OPERATIONS_GUIDE.md`, 사용자 절차는 `docs/USER_GUIDE.md`,
검증 기준은 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

이 규격은 별도 Single Edge/Supanova predecessor의 운영 콘솔 원칙을 Desktop Node
경계에 맞춰 가져온 provenance를 보존한다. 특정 private checkout의 절대 경로를 요구하지
않는다. Single Edge runtime 화면, Linux route, Linux service path를 active Desktop Node
Web Console에 직접 가져오지 않는다.

## 적용 범위

- `web/index.html` (Single Edge 셸: 로그인 페이지 + 앱 셸, 2026-10-10 Task 16a부터 `/`에서 서빙)
- `web/app.bundle.js` (생성물: `web/src/modules.json` 순서표 + `web/src/modules/*.ts` + `web/src/bootstrap.ts`)
- `web/style.css`, `web/i18n.js`, `web/sw.js`, `web/manifest.json`, `web/offline.html`
- `web/docs.html`, `web/guide.html`, `web/guide-content.md` (문서 포털)
- `web/vendor/*` (Pretendard, Coolicons, Chart.js; 라이선스는 `THIRD_PARTY_NOTICES.md`)
- `web/index.legacy.html`, `web/styles.css`, `web/app.js`, `web/src/served-app.ts`, `web/src/served/*.ts` (옛 콘솔; 옛 static contracts·web Pester·parity가 이 이름으로 읽고, 제거는 별도 campaign)
- `web/src/app.ts`, `web/src/view-model.ts`, `web/src/user-visible-fixtures.ts` (static parity scaffold)
- `web/scripts/*.mjs`
- `web/node-tests/*.mjs`
- `web/tests/*.ps1` (legacy/manual parity residue; Required CI 아님)

## 제품 문맥

Desktop Node Web Console은 Windows Hyper-V 단일 host 운영자가 반복적으로 보는
installed service console이다. 첫 화면은 marketing이나 landing page가 아니라
현재 service/API/VM/job/diagnostic 상태를 빠르게 확인하고 조작하는 작업면이어야
한다.

현재 운영 제품은 `0.42.74-admin-smoke`다. Web Console과 PCVCLI가 active operator
surface이고 TUI는 absent다. 최신 닫힌 manual-admin package-pair는
`0.42.73-admin-smoke -> 0.42.74-admin-smoke`이며 feature qualification은
`promotion_eligible=false`다.

운영자가 먼저 보는 정보:

- Service/API connection state
- Host readiness and runtime policy
- VM inventory and lifecycle job state
- Selected VM QoS/guest readback state
- Network inventory read-only state
- Batch/admin-smoke evidence health
- Diagnostic bundle and token handoff boundary
- Account/RBAC/JWT session state and Windows console capability

기본 installed surface는 Web Console `http://127.0.0.1/`, Local API
`http://127.0.0.1:7777/api/v1/...` 분리다. Static Web listener는
`/pcv-config.js`를 먼저 제공해 browser API base URL을 주입하고, API listener는
해당 Web origin만 CORS로 허용한다. HTTPS/443 built-in binding은 아직 active
Desktop Node Web Console 기본값이 아니다.

## Single UI Clone Mapping

2026-10-10 ADR-0018부터 Single Edge(`HardcoreMonk/purecvisor` `ui/`, Apache-2.0, 같은 저자)의 프론트엔드 **구조**를 그대로
차용한다: 파일 배치, `window.PCV` 네임스페이스 IIFE 모듈, 로그인 페이지 + 셸, 공통 계층, 빌드 파이프라인, PWA, 문서 포털.
도메인 화면은 Windows Desktop Node Local API 위에 다시 얹는다. Supanova 테마 토큰과 조작 감각은 유지한다.
Linux runtime screens are excluded.

| Single Edge `ui/` | Desktop Node `web/` | 경계 |
|---|---|---|
| `index.html` 로그인 페이지 + 셸(`#login-page`, `#app`, `shell-sidebar`, `shell-topbar`, `shell-statusbar`, `main.content > #cb`) | `web/index.html` | TOTP 없음, loopback 세션 자동 발급, Desktop Node 화면은 `#cb` 안의 정적 `.app-view` 섹션 |
| `app.bundle.js` (Makefile `UI_MODULES` concat → esbuild) | `web/app.bundle.js` ← `web/src/modules.json`(`pcv-web-module-order-v1`) 모듈 23개 + `bootstrap.ts` | 순서표 누락 검사, `PCV_UI_SOURCE_SHA1` 배너, `sw.js` CACHE_NAME bump |
| 공통 계층 shell, nav, theme, modal-core, modal, ui, uxlib, filter-state, metrics, charts, mobile, prefs, help | `web/src/modules/<같은 이름>.ts` | nav route는 Desktop Node 화면(dashboard, vms, network, jobs, activity, evidence, troubleshooting, helppage)만 |
| 도메인 모듈(vm, 컨테이너, 스토리지, 네트워크 가상화, monitor 등) | `core`, `desktop-api`, `endpoints`, `api`, `events`, `vm`, `vm-console`, `ops`, `monitor`, `accounts` | Linux route 없음. `endpoints.ts`가 Desktop Node route만 조립하고 WebSocket 대신 job polling |
| `i18n.js` | `web/i18n.js` | Desktop Node 키 추가, Linux 전용 키 제거(Task 16b), `_L(ko, en)` |
| `style.css` | `web/style.css` | Supanova 토큰 유지, Desktop Node 블록 추가, Linux 전용 selector 제거(Task 16b) |
| `sw.js`, `manifest.json`, `offline.html` | 같은 이름, root scope | Web Push 제외, `/pcv-config.js`·`/api/`·다른 origin 비캐시 |
| `docs.html`, `guide.html`, `guide-content.md`, `help.js` | 같은 이름 + `web/src/modules/help.ts` | Swagger 제외, 가이드는 `docs/USER_GUIDE.md` 발췌, 카탈로그는 Desktop Node Local API |
| eslint, `domsafe_ratchet.py` | `web/eslint.config.js`, `web/scripts/domsafe-ratchet.mjs` | ceiling 82 |
| 옛 Desktop Node 콘솔(단일 `app.js`, 연결 폼 셸) | `web/index.legacy.html` + `web/app.js` + `web/styles.css` | 옛 static contracts·web Pester·parity·browser fixture가 이 이름으로 읽는다. 제거와 재기준선은 train 뒤 별도 campaign |

셸 안에서 Desktop Node가 유지하는 조작면: asset explorer(VM 표, 검색, 선택 VM 상세), status bar(연결 상태, 세션, 폴링),
command palette(`Ctrl+K`), 환경설정(`Ctrl+P`: 테마, 언어, Local API 주소, 브라우저 token), 키보드 도움말(`?`), 도움말 카탈로그와 문서 포털.
옛 콘솔 표(2026-08 Supanova clone 표)는 git history에 남는다.

## Visual Theme

기본 인상은 dark operation console이다. 장식보다 판독성, 상태 식별, 정보 밀도,
빠른 비교를 우선한다.

권장 token:

| Token | Default | Role |
|---|---:|---|
| `--bg` | `#0a0f1a` | page background |
| `--bg2` | `#0f1525` | shell, toolbar, sidebar |
| `--bg3` | `#141c2e` | rows, fields, compact panels |
| `--bg-panel` | `rgba(15,21,37,.72)` | card/panel surface |
| `--border` | `#1e293b` | divider |
| `--border-panel` | `rgba(255,255,255,.08)` | panel hairline |
| `--fg` | `#e0f0ff` | primary text |
| `--fg2` | `#8895b5` | secondary text |
| `--accent` | `#22d3ee` | focus, selected, primary action |
| `--green` | `#34d399` | running, healthy, success |
| `--yellow` | `#fbbf24` | warning, pending |
| `--red` | `#f43f5e` | error, destructive, blocked |

규칙:

- 한 화면을 accent hue 하나로만 채우지 않는다.
- 상태색의 의미는 고정한다.
- focus ring은 제거하지 않는다.
- discrete orb, bokeh blob, decorative hero gradient를 추가하지 않는다.
- product object는 실제 운영 데이터로 보여준다. 추상 illustration으로 대체하지 않는다.

## Typography

- 기본 font stack은 Windows와 browser 기본을 우선하되, 숫자/IP/path/job id는
  monospace helper를 사용한다.
- viewport width로 font size를 scaling하지 않는다.
- letter spacing은 기본 0을 유지한다.
- panel/card heading은 작고 단단하게 둔다.
- hero-scale type은 이 console에서 사용하지 않는다.

## Layout

Desktop layout:

- topbar: breadcrumb, global search(`Ctrl+K`), polling 상태, 알림 센터, 환경설정(`Ctrl+P`: 테마, 언어, Local API 주소, 브라우저 token), 세션 사용자와 로그아웃
- statusbar: VM 실행, 작업, Critical, 호스트, 증적 요약 버튼
- sidebar: `운영 대시보드`와 `워크로드`·`인프라`·`관제`·`시스템`·`도움말` 묶음(view id `dashboard`, `vms`, `network`, `jobs`, `activity`, `evidence`, `troubleshooting`, `helppage`)
- main: `#cb` 안의 정적 `.app-view` section 중 nav가 고른 하나
- dashboard: ops summary, priority signals, recent activity, metrics
- troubleshooting: account session, RBAC permission state, console capability, token/diagnostic handoff

Responsive layout:

- `<= 1024px`: sidebar width를 줄이고 grid를 2열 이하로 접는다.
- `<= 768px`: 단일 column, action row wrap, table overflow 또는 card-mobile 전환.
- `<= 480px`: button label overflow를 먼저 확인하고, 필요 시 action을 menu로 묶는다.

고정 형식 UI에는 `minmax`, stable button size, stable table columns, stable dialog
width를 사용해 hover/loading/error state가 layout shift를 만들지 않게 한다.

## Components

### Buttons

- `.btn` 또는 기존 `button` style은 normal, hover, focus, disabled, loading state를
  구분해야 한다.
- destructive action은 red semantic state와 confirm copy를 함께 둔다.
- loading state는 button width를 흔들지 않는다.
- token 값, secret, protected token file content를 button/copy에 렌더링하지 않는다.

### Cards and Panels

- 카드 안에 floating card를 중첩하지 않는다.
- 반복 항목, metric, modal 내부 section처럼 frame이 실제로 필요한 곳에만 card를 쓴다.
- card radius는 최대 8px 기준으로 유지한다.
- status badge와 action row 위치를 화면별로 일관되게 둔다.

### Tables

- VM, job, evidence, network inventory처럼 행 비교가 중요한 데이터는 table을 우선한다.
- header는 짧게, cell은 값 중심으로 둔다.
- 긴 설명문은 table cell이 아니라 detail panel이나 troubleshooting row로 보낸다.
- numeric/status columns는 monospace 또는 고정 폭을 사용한다.
- row action은 오른쪽 끝에 짧은 button group으로 묶는다.

### Modals

- destructive confirm은 plain text와 명확한 대상 이름을 사용한다.
- VM/checkpoint mutation은 job 결과와 tracked activity로 이어져야 한다.
- modal footer가 내용에 밀려 사라지지 않도록 max-height와 overflow를 둔다.
- Esc/backdrop/focus behavior를 바꿀 때는 browser fixture 또는 real browser check를
  함께 갱신한다.

## API and State Rules

- API route는 Desktop Node Local API만 사용한다.
- Route coverage is mirrored in `DESKTOP_NODE_ROUTE_COVERAGE` so command/search
  UX can expose Windows-local routes without adding Linux route literals.
- API base URL은 listener-provided `window.PCV_DESKTOP_NODE_CONFIG.apiBaseUrl`
  값을 우선하고, 없을 때만 현재 Web origin으로 fallback한다.
- API response handling uses explicit unwrap helpers for envelope/list shapes
  while preserving problem-details normalization.
- Optional service bearer token UX는 환경설정(`Ctrl+P`)의 브라우저 token 입력과 `token 지우기`다. 옛 콘솔은 `apiToken` input과 `Clear` flow를 유지한다.
- Account auth UX는 `/api/v1/auth/login`, `/api/v1/auth/refresh`,
  `/api/v1/auth/session`, `/api/v1/auth/rbac`만 사용한다.
- JWT/password 값은 DOM, logs, diagnostics, static fixture에 렌더링하지 않는다.
- RBAC disabled/pending state는 frontend hint이고, authoritative enforcement는
  Local API가 담당한다.
- Console UX는 `/api/v1/console/capabilities`와 `/api/v1/vms/{id}/console`만
  사용한다. noVNC bridge가 없으면 `not_configured` 상태를 표시하고 Hyper-V
  `vmconnect` handoff를 안내한다. Console Access Card는 `enabled`/`status`/
  `reason_code` readback만 유지한다. target host/port 입력, 저장 버튼, LAN 토글은
  열지 않으며 configure는 CLI/API `pcvcli console novnc-target`이다.
- Selected VM QoS/guest readback UX는 `/api/v1/vms/{id}/blkio`,
  `/api/v1/vms/{id}/bandwidth`, `/api/v1/vms/{id}/guest-agent/status`,
  `/api/v1/vms/{id}/guest-agent/ping`을 readback으로 사용한다.
- Selected VM QoS direct control UX는 ADR-0008 manual-admin closure 이후
  `/api/v1/vms/{id}/qos/storage/preview`, `/api/v1/vms/{id}/qos/storage`,
  `/api/v1/vms/{id}/qos/network/preview`, `/api/v1/vms/{id}/qos/network`만 사용한다.
  Preview는 mutation route를 호출하지 않고, apply는 `operate` permission과 명시 확인을
  요구한다.
  CLI counterpart는 `pcvcli vm blkio-set`, `pcvcli vm bandwidth-set`이며 Web copy는
  ADR-0008 QoS만 지원 완료로 표시한다.
- ADR-0009 security boundary contract는 `0.42.53-admin-smoke`에서 provider/direct-control
  payload로 열렸다. Guest Execution UI와 guest channel 생성 UI는 raw secret을 받지 않고
  protected credential reference, confirmation guard, queued provider route만 사용한다. 실제
  Windows guest credentialed execution smoke는 PASS했고, running interrupt policy는
  `0.42.54-admin-smoke` 설치본 long-running cancel smoke로 PASS했다. `0.42.55-admin-smoke`는
  running cancel affordance 설치본 표시와 actual credentialed guest-exec를 current-card로 재확인했다. noVNC target
  set/clear는 API/CLI queued mutation으로 열렸고, Web Console은 save form을 열지 않는다.
  ADR-0010 적용 파일 승격은 별도 named step이다.
- Job/Activity row의 running guest execution cancel affordance는 일반 job cancel과 구분해
  `Cancel running guest exec` label과 `running-guest-execution` scope를 표시한다. 이 UI는
  0.42.55 package/current-card에서 설치본으로 승격됐다.
- WebSocket event flow를 추가하지 않는다.
- 새 셸 source는 `web/src/modules/**`와 `web/src/bootstrap.ts`이고, 순서표 `web/src/modules.json`이 `web/app.bundle.js`의 결합 순서를 정한다. 옛 콘솔 `web/app.js`의 source owner는 `web/src/served-app.ts`다.
- 생성물(`web/app.bundle.js`, `web/app.js`, `web/sw.js` CACHE_NAME)은 직접 편집하지 않고 `npm run build:served --prefix web`로 만든다.
- 화면 전환은 `nav.ts`가 소유한다. 보이는 `#cb .app-view` section은 `PCV.nav.activeView()`이고, 옛 렌더러(`setActiveView`, `monitor.ts` `renderActiveView`)는 이 값을 따른다. `#/<view>` hash도 같은 경로로 연다(BL-0019).
- form 안의 submit 버튼 click은 위임 핸들러가 다시 렌더하지 않고 submit 핸들러에 맡긴다. click 처리 중 재렌더는 form을 떼어 내 브라우저가 submit을 취소한다(BL-0016).
- route literal이 늘어나면 TypeScript contract mirror와 parity manifest를 함께 갱신한다.
- `innerHTML`을 쓸 때는 HTML escape/sanitizer helper를 거친다.

## Boundary Rules

Active Desktop Node Web Console에 다음을 추가하지 않는다.

- Non-Windows runtime screens
- External CDN font/icon/script fetch
- Linux noVNC/WebSocket backend import
- Service worker or manifest registration without host/package evidence
- Direct host mutation command text
- Token value examples
- Public trusted signing or external stable publication claim
- Built-in HTTPS/443 binding claim before TLS binding/trust evidence

## Validation

현재 Required CI Web 검증은 `.github/workflows/development-gates.yml`의 `web` .NET verifier
shard가 소유한다. final `main` `6e2bdb93ce308b632c929e2c17f5550ac3845401`, run
`32904006595`에서 exact contexts `dotnet`, `web`, `delivery`, `installer-policy`가 PASS했다.
로컬 Web 검증의 단일 npm entry point는 다음과 같다.

```text
npm ci --prefix web
npm run test:required --prefix web
git diff --check
```

아래 Pester 명령은 legacy/manual parity용이다. pwsh 기반 Public Boundary run
`32904006619`도 non-required transition residue이며 Required CI를 대체하지 않는다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

## 2026-08-25 historical Wave B local checkpoint

당시 local completion checkpoint에서는 아래 검사를 사용했다.

```text
npm run test:web-contracts --prefix web
npm run verify:web-contract-negative-parity --prefix web
node web/scripts/verify-verification-migration-manifest.mjs --require-web-local-pass
```

당시 범위는 legacy metadata/verifier와 positive projection 각각 `50/50`, focused Node unit
`199/199`, controlled negative parity failed `1`/skipped `49`, migration manifest `62`행이었다.
Web 행만 `mapped`/local pass/CI pending이며 Task 13 full completion audit까지 PASS했다.
Required CI dual-run과 cutover가 pending이던 historical predecessor 기록이다. 현재는 위 final
`main`/run의 exact four Required CI contexts가 cutover closure를 소유한다.

## Static parity snapshot policy

- `web/src/modules/**`·`web/src/bootstrap.ts`(새 셸)와 `web/src/served/**`·`web/src/served-app.ts`(옛 콘솔)가 served asset source of truth다.
- `web/app.bundle.js`와 `web/app.js`는 직접 편집하지 않고 `npm run build:served --prefix web`로만 갱신한다.
- route literal, fixture-visible copy, browser-visible DOM id/action이 바뀌면
  `npm run generate:parity --prefix web`로 `web/generated/parity/static-asset-parity.manifest.json`
  snapshot을 갱신한다.
- release gate는 `npm test --prefix web`와 `npm run verify:parity --prefix web`가 같은
  generated asset과 parity snapshot을 보고 있음을 확인해야 한다.

Host static serving, content type, packaging payload가 바뀌면:

현재 required 경로는 `dotnet`/`web`/`delivery`/`installer-policy` verifier shards와
`npm run test:required --prefix web`이다. 아래 Pester는 legacy/manual packaging parity이며
Required CI가 아니다.

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

## Porting Order

1. Keep Desktop Node route/API contract fixed.
2. Add or update design tokens in `web/style.css` (옛 콘솔은 `web/styles.css`).
3. Port one component family at a time.
4. Regenerate `web/app.bundle.js` only from `web/src/modules.json` and `web/src/modules/**` (옛 `web/app.js`는 `web/src/served-app.ts`에서만).
5. Run static parity and browser fixture checks.
6. Only then consider vendor assets or host static content-type changes.
