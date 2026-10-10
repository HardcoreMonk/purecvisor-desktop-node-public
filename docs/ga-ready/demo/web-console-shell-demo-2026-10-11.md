# web.console.shell Single Edge 셸 시연 (2026-10-11)

- 시나리오: 해당 없음(release train `0.42.96-admin-smoke` 적재 queue 81·86, ADR-0018 Single Edge 프론트엔드 구조와 BL-0019 화면 전환 수정의 Lane 2 probe). 기준은 `docs/superpowers/plans/2026-10-11-purecvisor-desktop-node-train-04296.md` Task 6.
- 기준: ADR-0018, `docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`. 직전 시연 `docs/ga-ready/demo/web-console-shell-demo-2026-10-10.md`(설치본 `0.42.95`, 화면 전환 FAIL로 train 정차)의 재시연이다.
- 이 기록은 이 호스트 설치본 `0.42.96-admin-smoke`의 시연이다. 승격 근거가 아니고, public trusted signing이나 external stable
  publication을 주장하지 않는다. 다른 시나리오는 주장하지 않는다.
- 판정: **PASS**.

## 설치본

- 설치본 `0.42.96-admin-smoke+9ac8eb3fcf38989dc4dbd66351fc9c7c858e2d63`(train 0.42.96 fullgate build, 기준 commit `9ac8eb3`), package 모드 `AllowUnsignedDev` / `LocalTest`, 시연 뒤 Rollback 없음(읽기만).
- service Running/Automatic, Web `http://127.0.0.1/` 200, 보존 VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## 실행

- 명령: Playwright(Chromium) MCP로 `http://127.0.0.1/`을 열고 `page.evaluate`로 상태를 읽었다. 스크립트 파일 없음. token 입력 없음. 첫 방문에서 새 service worker가 제어를 넘겨받는 설계된 1회 reload가 끝나도록 한 번 reload한 뒤 확인했다.
- 결과:

| 확인 | 결과 |
| --- | --- |
| 셸 로드 | `#login-page` `display:none`, `#app` inert 아님, auth gate 없음, sessionStorage 세션(access_token) 존재, 상단 바 `loopback-session`. PASS |
| 사이드바 8 화면 | 운영 대시보드·가상 머신·네트워크·작업·이벤트 센터·증적·진단과 계정·도움말 클릭마다 그 section만 보이고 `PCV.nav.activeView()`가 같다. PASS |
| hash와 polling | `#/jobs`로 jobs 표시, 35초 polling 재렌더 뒤에도 jobs 유지. PASS |
| VM 목록 | `#vm-table`에 `pcv-guest-installed-04253-r1`, `pcv-it-s2-source` 2행. PASS |
| 도움말 | 배너 "PureCVisor Desktop Node — Local API 레퍼런스", namespace 7, route 행 65, 전용 `helppage` section. 뒤이어 진단과 계정 화면은 `-panel` 7개 그대로(help banner 없음). PASS |
| 문서 포털 | `docs.html#2-웹-콘솔` reader 표시, 장 목록 10, breadcrumb "2. 웹 콘솔". PASS |
| 자산 | `manifest.json` 200 `application/json`, `sw.js`·`app.bundle.js`·`i18n.js` 200 `application/javascript`, `offline.html`·`docs.html`·`guide.html`·`index.legacy.html` 200 `text/html`, `guide-content.md` 200 `text/markdown`, `style.css` 200 `text/css`, `/src/bootstrap.ts` 404. PASS |
| PWA | service worker `activated`, scope `http://127.0.0.1/`, bundle `PCV_UI_SOURCE_SHA1` `15e0c8ef`. PASS |
| 화면 노출 | 본문에 호스트명·LAN IP·사용자 홈 경로 없음 |
| 콘솔 오류 | 첫 로드의 loopback 세션 전 `GET /runtime/policy` 401(세션 뒤 회복), 이 브라우저 localStorage의 옛 tracked job id `GET /jobs/<id>` 404, 자산 검사가 일부러 요청한 `/src/bootstrap.ts` 404. 제품 결함 아님 |

## 화면 캡처

캡처는 `docs/ga-ready/demo/web-console-shell-20261011/` 아래 PNG다. 1280×800 CSS 픽셀로 찍었고 500 KB 이하 규칙에 맞춰 01·03은 800×500, 02는 960×600으로 축소했다(04는 원본). 사용자 홈 경로·LAN IP·호스트명·token은 화면에 없다.

- `docs/ga-ready/demo/web-console-shell-20261011/01-shell-dashboard.png`: Single Edge 셸(사이드바, 상단 바 `loopback-session`, 상태 바)과 운영 대시보드.
- `docs/ga-ready/demo/web-console-shell-20261011/02-vms.png`: 사이드바 "가상 머신" 클릭 뒤 VM 화면.
- `docs/ga-ready/demo/web-console-shell-20261011/03-help-catalog.png`: 도움말 카탈로그(Local API 레퍼런스).
- `docs/ga-ready/demo/web-console-shell-20261011/04-docs-reader.png`: 문서 포털 reader(2. 웹 콘솔).

## 확인

| 항목 | 값 |
| --- | --- |
| 확인자 | |
| 확인 일시 | |
