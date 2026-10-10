# web.console.shell Single Edge 셸 시연 (2026-10-10)

- 시나리오: 해당 없음(release train `0.42.95-admin-smoke` 적재 queue 81, ADR-0018 Single Edge 프론트엔드 구조의 Lane 2 probe). 기준은 `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-train-04295.md` Task 6.
- 기준: ADR-0018, `docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`.
- 이 기록은 이 호스트 설치본 `0.42.95-admin-smoke+b9898cf`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. 다른 시나리오는 주장하지 않는다.
- 판정: **FAIL**. 셸 로드·loopback 세션·자산·PWA·도움말·문서 포털은 통과했지만 사이드바와 hash 화면 전환이 dashboard에 머문다(BL-0019). train `0.42.95`는 이 결함으로 정차했다.

## 설치본

- 설치본 `0.42.95-admin-smoke+b9898cf7a125e4dadbc4b29ae235f46d8c914a01`(train 0.42.95 fullgate가 남긴 build, 기준 commit `b9898cf`), package 모드 `AllowUnsignedDev` / `LocalTest`, 시연 뒤 Rollback 없음(읽기만).
- service Running/Automatic, Web `http://127.0.0.1/` 200, 보존 VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## 실행

- 명령: Playwright(Chromium) MCP로 `http://127.0.0.1/`을 열고 `page.evaluate`로 상태를 읽었다. 스크립트 파일 없음. token 입력 없음.
- 결과:

| 확인 | 결과 |
| --- | --- |
| 셸 로드 | `#login-page` 숨김, `#app` inert 해제, auth gate 없음, sessionStorage 세션(access_token) 존재. 상단 바 `loopback-session`. PASS |
| 사이드바 | 운영 대시보드, 워크로드/가상 머신, 인프라/네트워크, 관제/작업·이벤트 센터·증적, 시스템/진단과 계정, 도움말 8 항목 렌더. PASS |
| VM 목록 | `#vm-table`에 보존 VM `pcv-guest-installed-04253-r1`, `pcv-it-s2-source` 2행. PASS |
| 도움말 | `navigateTo('helppage')` 뒤 배너 "PureCVisor Desktop Node — Local API 레퍼런스", namespace 7, route 행 65. PASS |
| 자산 | `manifest.json` 200 `application/json`, `sw.js` 200 `application/javascript`, `offline.html`·`docs.html`·`guide.html`·`index.legacy.html` 200 `text/html`, `guide-content.md` 200 `text/markdown`, `app.bundle.js`·`i18n.js` 200, `style.css` 200 `text/css`, `/src/bootstrap.ts` 404. PASS |
| PWA | service worker registration scope `http://127.0.0.1/`, `activated`. PASS |
| 화면 전환 | 사이드바 "가상 머신" 클릭: breadcrumb `워크로드 / 가상 머신`, vms section `.active`이지만 `hidden` 그대로(0×0), dashboard section 계속 표시. `#/vms` hash와 `navigateTo('jobs')`도 같음(jobs section에서 `hidden` 제거 직후 재설정되는 mutation 관찰). **FAIL** |
| 문서 포털 | `docs.html` landing(hero "하나의 Windows 호스트, 하나의 제어면.", 검색 입력, reader 장 링크 10, h2 10), `#2-웹-콘솔`로 reader 전환(breadcrumb "2. 웹 콘솔"). PASS |
| 콘솔 오류 | `GET /api/v1/jobs/job-bbae84f5…` 404 1건: 이 브라우저 localStorage의 옛 tracked job id(이전 설치본)라 제품 결함 아님. dashboard는 `PCV_PARTIAL_REFRESH_DEGRADED`로 표시. 참고 |

- 원인: `web/src/modules/nav.ts` `_renderContentPaint`가 `#cb .app-view`를 토글한 뒤 옛 `render()`를 부르고, 옛 `web/src/modules/monitor.ts` `renderActiveView()`가 옛 `state.activeView`(기본 dashboard) 기준으로 `.app-view` 전부를 다시 토글한다. 도움말 route만 `PCV.help.render`라 정상이다. Host Chromium 시험(`ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste`)은 dashboard 렌더와 세션만 확인해 잡지 못했다.
- 수정 방향(backlog BL-0019): route 실행 전 `setActiveView(viewId)`로 옛 상태를 맞추거나 옛 `renderActiveView`가 `PCV.nav` 현재 tab을 따르게 하고, bundle-load·Host Chromium 시험에 화면 전환 뒤 section 가시성 검사를 더한다.

## 화면 캡처

캡처는 `docs/ga-ready/demo/web-console-shell-20261010/` 아래 PNG(1280×800 CSS 픽셀, 500 KB 이하)다. 사용자 홈 경로·LAN IP·호스트명·token은 화면에 없다(본문 텍스트 검색으로 확인).

- `docs/ga-ready/demo/web-console-shell-20261010/01-shell-dashboard.png`: Single Edge 셸(사이드바, 상단 바 `loopback-session`, 상태 바)과 운영 대시보드.
- `docs/ga-ready/demo/web-console-shell-20261010/02-help-catalog.png`: 도움말 카탈로그(Local API 레퍼런스).
- `docs/ga-ready/demo/web-console-shell-20261010/03-vms-click-stays-dashboard.png`: "가상 머신" 클릭 뒤 breadcrumb은 바뀌었지만 dashboard가 남아 있는 결함 화면.
- `docs/ga-ready/demo/web-console-shell-20261010/04-docs-portal.png`: `docs.html` 문서 포털 landing.
- `docs/ga-ready/demo/web-console-shell-20261010/05-docs-reader.png`: `docs.html#2-웹-콘솔` reader(장 목록 10, breadcrumb "2. 웹 콘솔").

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |
