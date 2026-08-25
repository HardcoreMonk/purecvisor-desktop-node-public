# PureCVisor Desktop Node Web DESIGN.md

이 문서는 Windows Desktop Node Web Console의 시각/프론트엔드 규격이다. 제품
운영 절차는 `docs/OPERATIONS_GUIDE.md`, 사용자 절차는 `docs/USER_GUIDE.md`,
검증 기준은 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

이 규격은 `D:\data\projects\codex-zone\purecvisor-single\DESIGN.md`의 Supanova
운영 콘솔 원칙을 Desktop Node 경계에 맞춰 가져온 것이다. Single Edge runtime
화면, Linux route, Linux service path를 active Desktop Node Web Console에 직접
가져오지 않는다.

## 적용 범위

- `web/index.html`
- `web/styles.css`
- `web/src/served-app.ts`
- `web/src/app.ts`
- `web/src/view-model.ts`
- `web/src/user-visible-fixtures.ts`
- `web/scripts/*.mjs`
- `web/tests/*.ps1`

## 제품 문맥

Desktop Node Web Console은 Windows Hyper-V 단일 host 운영자가 반복적으로 보는
installed service console이다. 첫 화면은 marketing이나 landing page가 아니라
현재 service/API/VM/job/diagnostic 상태를 빠르게 확인하고 조작하는 작업면이어야
한다.

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

`purecvisor-single`의 Supanova 운영 콘솔 UI/UX는 화면 구조와 조작 감각만 이식한다.
Desktop Node는 Windows Desktop Node Local API와 Hyper-V VM 운영 화면만 활성화한다.
Linux runtime screens are excluded.

| Single UI surface | Desktop Node clone target | Active Desktop Node boundary |
|---|---|---|
| Shell frame | `single-clone-shell`, glass topbar, rail, sidebar, workspace tabbar | Static `web/index.html` shell, no imported Single Edge runtime route |
| Top menu | Korean menu bar commands for dashboard, refresh, VM workbench, network, activity, evidence, troubleshooting | Calls existing browser handlers only |
| Operator session | SUPANOVA theme selector, Korean/language selector, global search, command palette trigger, Live viewer state, API base, browser token input, account login/refresh/logout boundary | Clears browser session/token/JWT state only; command palette uses Windows-local view/action set |
| Activity rail | compact icon rail for assets, evidence, network, activity, settings | Navigates Desktop Node views with `data-view-link` |
| asset explorer | VM asset tabs, search, selected VM row, status/CPU/memory columns, empty state | Windows VM assets only; non-VM runtime tabs remain absent or inactive |
| Workspace tabs | Dashboard, VM Assets, Network, Jobs, Activity, Evidence, Troubleshooting | Browser hash views backed by Desktop Node Local API route names |
| Dashboard | hero, operation memo, summary pills, quick actions, ops cockpit, monitoring signals | Host status, runtime policy, VM/job/evidence state only |
| Activity/Event Center | severity lane, problem-details events, evidence issues, active/failed job rollup | Read-only browser synthesis; no WebSocket route or host mutation command |
| Account/RBAC | username/password login, JWT refresh, logout, session role, permission chips | Desktop Node `/api/v1/auth/login`, `/api/v1/auth/refresh`, `/api/v1/auth/logout`, `/api/v1/auth/session`, `/api/v1/auth/rbac`; password/JWT values not rendered |
| Console | noVNC/console affordance | Desktop Node `/api/v1/console/capabilities` and `/api/v1/vms/{id}/console`; noVNC remains disabled until a Windows VNC/WebSocket bridge is configured, Hyper-V `vmconnect` handoff is shown |
| status bar | connection state, selected asset, API latency/evidence labels | Browser-rendered status; no host mutation command text |

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

- topbar: service/API/token/refresh controls
- sidebar: view navigation
- main: active view content
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
- Optional service bearer token UX는 현재 `apiToken` input과 `Clear` flow를 유지한다.
- Account auth UX는 `/api/v1/auth/login`, `/api/v1/auth/refresh`,
  `/api/v1/auth/session`, `/api/v1/auth/rbac`만 사용한다.
- JWT/password 값은 DOM, logs, diagnostics, static fixture에 렌더링하지 않는다.
- RBAC disabled/pending state는 frontend hint이고, authoritative enforcement는
  Local API가 담당한다.
- Console UX는 `/api/v1/console/capabilities`와 `/api/v1/vms/{id}/console`만
  사용한다. noVNC bridge가 없으면 `not_configured` 상태를 표시하고 Hyper-V
  `vmconnect` handoff를 안내한다.
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
  running cancel affordance 설치본 표시와 actual credentialed guest-exec를 current-card로 재확인했다. ADR-0010이 닫히기 전까지
  account/noVNC target config mutation은 열지 않는다.
- Job/Activity row의 running guest execution cancel affordance는 일반 job cancel과 구분해
  `Cancel running guest exec` label과 `running-guest-execution` scope를 표시한다. 이 UI는
  0.42.55 package/current-card에서 설치본으로 승격됐다.
- WebSocket event flow를 추가하지 않는다.
- `web/src/served-app.ts`가 served `web/app.js`의 source owner다.
- `web/app.js`는 직접 편집하지 않고 `npm run build:served --prefix web`로 생성한다.
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

Visual/static Web Console 변경 후 기본 검증:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

2026-08-25 Wave B local completion checkpoint에서는 아래 검사를 사용한다.

```text
npm run test:web-contracts --prefix web
npm run verify:web-contract-negative-parity --prefix web
node web/scripts/verify-verification-migration-manifest.mjs --require-web-local-pass
```

현재 범위는 legacy metadata/verifier와 positive projection 각각 `50/50`, focused Node unit
`199/199`, controlled negative parity failed `1`/skipped `49`, migration manifest `62`행이다. Web
행만 `mapped`/local pass/CI pending이며 Task 13 full completion audit까지 PASS했다. Required CI
dual-run과 cutover는 pending이므로 위 명령은 legacy Web Pester 또는 required CI를 대체하지 않는다.

## Static parity snapshot policy

- `web/src/served/**`와 `web/src/served-app.ts`가 served asset source of truth다.
- `web/app.js`는 직접 편집하지 않고 `npm run build:served --prefix web`로만 갱신한다.
- route literal, fixture-visible copy, browser-visible DOM id/action이 바뀌면
  `npm run generate:parity --prefix web`로 `web/generated/parity/static-asset-parity.manifest.json`
  snapshot을 갱신한다.
- release gate는 `npm test --prefix web`와 `npm run verify:parity --prefix web`가 같은
  generated asset과 parity snapshot을 보고 있음을 확인해야 한다.

Host static serving, content type, packaging payload가 바뀌면:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

## Porting Order

1. Keep Desktop Node route/API contract fixed.
2. Add or update design tokens in `web/styles.css`.
3. Port one component family at a time.
4. Regenerate `web/app.js` only from `web/src/served-app.ts`.
5. Run static parity and browser fixture checks.
6. Only then consider vendor assets or host static content-type changes.
