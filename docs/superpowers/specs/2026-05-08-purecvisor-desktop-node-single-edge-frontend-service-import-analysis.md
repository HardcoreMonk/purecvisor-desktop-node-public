# PureCVisor Desktop Node Single Edge Frontend Service Import Analysis

작성 기준: 2026-05-08

## 목적

`D:\data\projects\codex-zone\purecvisor-single`의 Linux Single Edge Web UI를
`DESIGN.md` 기준으로 분석하고, Windows Desktop Node 저장소에 가져올 수 있는
범위와 그대로 가져오면 안 되는 범위를 분리한다.

이 문서는 active product Web Console을 즉시 교체하는 구현 문서가 아니다. 현재
Desktop Node 저장소는 Windows Desktop Node 전용이며, Linux `purecvisor-single`,
`purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime 화면 또는 adapter를 제품
경로에 추가하지 않는다는 저장소 경계를 유지한다.

## 분석한 소스

Source repository:

```text
D:\data\projects\codex-zone\purecvisor-single
```

확인한 주요 파일:

- `DESIGN.md`
- `AGENTS.md`
- `docs/adr/0013-frontend-iife-module-scope.md`
- `docs/adr/0016-supanova-theme-reduction.md`
- `scripts/bundle-ui.sh`
- `ui/index.html`
- `ui/style.css`
- `ui/app.js`
- `ui/app.bundle.js`
- `ui/modules/endpoints.js`
- `ui/modules/api.js`
- `ui/modules/*.js`

Desktop Node 비교 대상:

- `web/index.html`
- `web/styles.css`
- `web/app.js`
- `web/src/served-app.ts`
- `web/src/app.ts`
- `web/scripts/build-served-asset.mjs`
- `web/scripts/verify-static-parity.mjs`
- `web/scripts/verify-browser-fixture.mjs`
- `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- `src/DesktopNode.Host/DesktopNodeHostApplication.cs`

## Single Edge UI 구조 요약

Single Edge UI는 Vanilla JS SPA이며 `DESIGN.md`의 적용 범위는 다음이다.

```text
ui/index.html
ui/style.css
ui/modules/*.js
ui/guide.html
ui/guide-content.md
ui/samples/*.html
```

프론트엔드 구조:

- `window.PCV` 아래 IIFE module namespace를 둔다.
- `ui/modules/endpoints.js`의 `EP` registry가 API route를 중앙 관리한다.
- `scripts/bundle-ui.sh`가 module order를 검증하고 `ui/app.bundle.js`를 concat한다.
- `ui/index.html`은 `/ui/` base href, local vendor assets, `i18n.js`,
  `app.bundle.js`, `app.js`를 로드한다.
- 테마는 Supanova 계열 token을 중심으로 운영 콘솔 밀도, table/card/button/modal
  state를 정의한다.

가져올 가치가 있는 부분:

- Supanova dark operation-console visual language
- `PCV.*` namespace module discipline
- endpoint registry를 통한 route single source
- local vendored assets preference
- sanitizer helper와 `innerHTML` 제한 원칙
- table/card/button/modal density rules
- static preview/sample 검증 문화

## Desktop Node 현재 Web Console 계약

Desktop Node Web Console은 repo-root `web/**`가 제품 Web asset owner다.

현재 정적 asset 계약:

- served files: `/index.html`, `/styles.css`, `/app.js`
- source owner: `web/src/served-app.ts`
- build: `npm run build:served --prefix web`
- freshness check: `npm run check:served --prefix web`
- parity check: `npm run verify:parity --prefix web`
- browser fixture: `npm run browser:fixture --prefix web`

Desktop Node Local API route surface:

- `/api/v1/host/status`
- `/api/v1/runtime/policy`
- `/api/v1/ops/summary`
- `/api/v1/network/inventory`
- `/api/v1/vms`
- `/api/v1/vms/{id}`
- `/api/v1/vms/{id}/start`
- `/api/v1/vms/{id}/shutdown`
- `/api/v1/vms/{id}/poweroff`
- `/api/v1/vms/{id}/restart`
- `/api/v1/vms/{id}/checkpoints`
- `/api/v1/jobs`
- `/api/v1/jobs/{id}`
- `/api/v1/jobs/{id}/cancel`
- `/api/v1/jobs/{id}/retry`

Host static serving:

- `DesktopNodeHostApplication` serves static files from configured `WebRootPath`.
- Loopback static requests can be unauthenticated.
- Non-loopback static requests require the same bearer token gate as API requests
  unless token is absent.
- Static content type support is currently `.html`, `.js`, `.css`, and fallback
  `application/octet-stream`.

## As-Is Active Import 충돌

Single Edge `ui/**`를 Desktop Node active `web/**`로 그대로 덮어쓰면 다음 문제가
발생한다.

1. 저장소 경계 위반

   Single UI source에는 KVM/libvirt/LXC/ZFS/OVS/OVN, Linux service, `purecvisorsd`,
   `journalctl`, `/var/lib/libvirt`, ZFS dataset, LXC path, OVN/OVS route와 copy가
   포함된다. Desktop Node 저장소의 active product surface에는 이 Linux runtime
   범위를 추가하지 않는다.

2. API contract 불일치

   Single UI endpoint registry는 `/auth/token`, `/auth/refresh`, `/ws/events`,
   `/containers`, `/storage/*`, `/networks`, `/ovn/*`, `/vms/{id}/snapshot/*`,
   `/vms/{id}/stop` 등을 전제로 한다. Desktop Node는 protected bearer token
   input과 Hyper-V route set을 사용하며, auth/login/refresh/WebSocket/containers/
   ZFS/OVN route를 제공하지 않는다.

3. Static serving path 불일치

   Single UI는 `<base href="/ui/">`와 `/ui/*` asset path를 전제로 한다. Desktop
   Node 제품 Web root는 `/index.html`, `/styles.css`, `/app.js` contract다.
   `/ui/vendor/*`, font, icon, service worker, manifest를 활성화하려면 host
   content-type, cache, packaging payload, MSI payload contract를 별도 변경해야 한다.

4. 검증 계약 회귀

   `web/tests/PcvDesktopWeb.Static.Tests.ps1`는 active Web Console이 Linux/KVM/ZFS/
   OVS/OVN/purecvisorsd 문구와 host mutation command를 포함하지 않도록 검증한다.
   As-is copy는 이 guard를 실패시킨다.

5. 운영 UX 불일치

   Single UI login은 username/password -> JWT 흐름이다. Desktop Node 운영 UX는
   installed service protected token file과 Web Console bearer token 입력/clear
   흐름이다. 사용자에게 `admin/password` login 화면을 그대로 노출하면 현재
   운영 가이드와 맞지 않는다.

## 허용 가능한 이식 계약

As-is active replacement는 금지한다. 대신 다음 순서로 가져온다.

### Slice A: Design Contract Import

Desktop Node 전용 `web/DESIGN.md`를 추가한다. Single Edge `DESIGN.md`의 시각
규칙을 가져오되 다음을 Windows Desktop Node 기준으로 치환한다.

- Single Edge/KVM/LXC/ZFS/OVN 운영 문맥 제거
- Hyper-V, Windows service, MSI, diagnostics, job store, protected token 운영
  문맥으로 변경
- `web/index.html`, `web/styles.css`, `web/src/served-app.ts`, `web/tests/**`를
  적용 범위로 지정
- public trusted signing/external stable publication claim 금지 유지

### Slice B: Supanova Visual Port

`web/styles.css`를 Supanova token 기반 dark operation console로 단계적으로 바꾼다.
HTML id, route, JS action handler, TypeScript served asset owner는 유지한다.

금지:

- Single UI의 Linux module copy
- `/ui/` base path 전환
- auth/login/refresh/WebSocket flow 도입
- container/storage/OVN screens 도입

### Slice C: Endpoint Registry Port

Single UI의 `EP` registry 패턴을 Desktop Node route set에 맞춘
`web/src/desktop-endpoints.ts` 또는 `served-app.ts` 내부 registry로 도입한다.
route literal 분산을 줄이되, route 목록은 Desktop Node Local API만 포함한다.

### Slice D: Component Pattern Port

Single UI의 table/card/button/modal density와 sanitizer pattern을 Desktop Node
화면에 맞춰 이식한다.

대상:

- VM table/detail/action row
- Jobs/activity table
- Evidence dashboard
- Troubleshooting diagnostics/token panels
- Network inventory read-only table

### Slice E: Optional Vendor Asset Port

Pretendard, Coolicons 같은 local vendor asset은 license와 payload size를 확인한 뒤
필요할 때만 별도 slice로 가져온다. Host content type과 packaging payload 검증을
함께 갱신한다.

## 권장하지 않는 방식

다음은 실행하지 않는다.

- `purecvisor-single/ui/**`를 `web/**`에 그대로 복사해서 active console로 교체
- `web/index.html`을 `/ui/` base href로 변경
- Desktop Node에 `/api/v1/auth/*` login/refresh compatibility route 추가
- Desktop Node에 container, ZFS, OVN, OVS, KVM, libvirt 화면 추가
- Single UI `scripts/bundle-ui.sh`의 `/usr/local/share/purecvisor/ui` deploy path 도입
- Linux command/help copy를 Desktop Node Web Console에 노출

## 검증 기준

Design/visual-only slice:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

Host static serving 또는 vendor asset content type이 바뀌는 slice:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Guard scan:

```powershell
Select-String -Path web/index.html,web/styles.css,web/src/*.ts -Pattern 'journalctl','libvirt','KVM','ZFS','OVS','OVN','purecvisorsd','/containers','/storage','/auth/token','/auth/refresh','/ws/events'
```

Expected: no matches in active Desktop Node product Web Console unless the match is
inside this analysis document or a deliberate negative test.

## 결론

Single Edge frontend service를 active Desktop Node Web Console로 "그대로" 가져오는
것은 현재 저장소 경계와 API/static parity 계약을 깨뜨린다.

가져올 수 있는 것은 디자인 규격, Supanova visual system, frontend module discipline,
endpoint registry pattern, component density pattern이다. 가져오면 안 되는 것은
Linux Single Edge runtime surface와 auth/WebSocket/API endpoint set이다.

따라서 후속 구현은 `web/DESIGN.md` 추가와 Supanova visual port부터 시작하고,
Desktop Node API route와 TypeScript served asset owner를 유지하는 방식으로 진행한다.
