# Desktop Node Web Console: Single Edge 프론트엔드 구조 차용 설계

- Design-ID: `pcv-single-edge-frontend-structure-v1`
- 작성일: `2026-10-10`
- 문서 상태: `approved-2026-10-10` (사용자 승인 `1,2,3,4`의 1·2)
- 선행 설계: `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3a-web-console-design.md`, `web/DESIGN.md` "Single UI Clone Mapping"
- 결정: `docs/adr/0018-single-edge-frontend-structure-adoption.md`
- 제품 payload 변경: `true` (`web/` 정적 자산 전체, Host 정적 서빙, installer 파일 inventory)
- host/VM/service/package mutation: `false` (Lane 1만; 설치본 반영은 뒤의 train)
- public trusted signing: `false`
- external stable publication: `false`

> 실행 계획은 `docs/superpowers/plans/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure.md`(campaign
> `single-edge-frontend-structure-20261010`)다. 분석 원문은 2026-10-10 Claude Doc "Single Edge 프론트엔드 구조 차용 분석"이다.

## 1. 목적과 결론

사용자 요청은 "`https://github.com/HardcoreMonk/purecvisor`(Single Edge)의 프론트엔드 구조를 그대로 차용"이다. Single Edge
`ui/`와 Desktop Node `web/`은 모두 "순서표대로 이어 붙인 classic script 하나"라는 같은 빌드 모델이고 시각 셸·Supanova 토큰은
이미 이식돼 있다. 따라서 셸·공통 계층·빌드 파이프라인을 그대로 가져오고, Desktop Node 도메인 코드를 그 구조 위에 다시 얹는다.
Linux 도메인 모듈은 저장소 경계(ADR-0006, `docs/CODING_GUIDE.md` 3.2)상 제외한다. `ui/`를 통째로 복사해 API만 바꾸는 방식은
route 계약이 달라 약 24,000줄을 손봐야 하므로 택하지 않는다.

## 2. 출처

- `HardcoreMonk/purecvisor` `main` `ed147de`(2.0.0 source, 2026-10-08 push, Apache-2.0). `ui/` 63개 파일. 같은 저작자의
  공개 저장소라 코드를 가져올 수 있고, 가져온 파일은 머리에 `Ported from purecvisor ui/<path> (Apache-2.0, same author)`를
  적는다.
- vendor 자산 라이선스는 `THIRD_PARTY_NOTICES.md`에 적는다: Pretendard(SIL OFL 1.1), Coolicons 1.2.2 subset(CC BY 4.0),
  Chart.js 4.4.4(MIT). qrcode(TOTP용)와 noVNC(Desktop Node는 자체 frame route)는 가져오지 않는다.

## 3. 파일 배치표

| Single Edge `ui/` | Desktop Node `web/` (이 설계) | 비고 |
| --- | --- | --- |
| `index.html` | `web/index.html` | 로그인 페이지 `#login-page` + 앱 셸 `#app`(`shell-sidebar`, `shell-topbar`, `main.content.shell-content`), CSP meta, 테마 부트스트랩 inline script, manifest·icon 링크 |
| `app.js` | `web/src/bootstrap.ts` → bundle 끝 | 전역 상태, `window.PCV.state/config/auth`, 에디션 게이팅(Desktop Node 에디션: Linux 전용 nav 숨김) |
| `modules/*.js` | `web/src/modules/*.ts` | `window.PCV` IIFE, `@ts-nocheck`, ES export 없음 |
| `app.bundle.js` | `web/app.bundle.js` | `build-served-asset.mjs` 출력, `PCV_UI_SOURCE_SHA1` 배너 |
| `i18n.js` | `web/i18n.js` | `PCV.I18N` ko·en, bundle과 별개로 먼저 로드 |
| `style.css` | `web/style.css` | Single Edge 원문 base + Desktop Node selector 병합, Linux 전용 selector 제거 |
| `sw.js`, `manifest.json`, `offline.html`, `icon-192.png`, `icon-512.png` | 같은 이름 | PWA(승인 2). push 코드 제외 |
| `vendor/pretendard/`, `vendor/coolicons/`, `vendor/chart.umd.min.js` | `web/vendor/` 같은 배치 | 라이선스 고지 |
| `samples/*.html` | `web/samples/` | design-system-preview, supanova-preview. 기존 `web/mockups/`는 그대로 |
| `docs.html`, `guide.html`, `guide-content.md` | 같은 이름 | 문서 포털(승인 2), 내용은 `docs/USER_GUIDE.md` 발췌 |
| `maintenance.html`, `maintenance-status.json` | 가져오지 않음 | Desktop Node service에 maintenance 모드 없음 |
| Makefile `ui-bundle`, `scripts/bundle-ui.sh` | `web/scripts/build-served-asset.mjs` + `web/src/modules.json`(`pcv-web-module-order-v1`) | 모듈 순서표, 누락 검사, `PCV_UI_SOURCE_SHA1` 배너, `sw.js` 캐시 이름 bump |
| `eslint.config.js`, `scripts/domsafe_ratchet.py` | `web/eslint.config.js`, `web/scripts/domsafe-ratchet.mjs` | `npm run lint`, `npm run lint:domsafe` |

옛 구조 `web/src/served/*.ts`, `web/src/served-app.ts`, `web/app.js`, `web/styles.css`는 Task 16에서 제거한다. 그때까지 두
구조가 함께 빌드된다.

## 4. 모듈 순서표

bundle 순서는 Single Edge `UI_MODULES`와 같은 원칙(의존이 앞)이다. 순서표에 없는 `web/src/modules/*.ts`가 있으면 빌드가
실패한다.

| 순서 | 모듈 | 계층 | 출처 / 옛 part |
| --- | --- | --- | --- |
| 1 | `endpoints` | 공통 | Single Edge `endpoints.js` 형식, 내용은 옛 `routes.ts` route registry |
| 2 | `api` | 공통 | Single Edge `api.js` 구조, Desktop Node auth(loopback session, account login/refresh/logout/session/rbac, 브라우저 token), 401/403 재로그인 |
| 3 | `events` | 공통 | 옛 `job-polling.ts`(WebSocket 대신 polling) |
| 4 | `ui` | 공통 | Single Edge `ui.js` |
| 5 | `uxlib` | 공통 | Single Edge `uxlib.js` + 옛 `table.ts`, `summary.ts` 헬퍼 |
| 6 | `filter-state` | 공통 | Single Edge |
| 7 | `theme` | 공통 | Single Edge, allowlist = supanova 계열 + Desktop Node `contrast` |
| 8 | `modal-core`, `modal` | 공통 | Single Edge |
| 9 | `nav` | 공통 | Single Edge, nav 모델 = Desktop Node views 7개 |
| 10 | `shell` | 공통 | Single Edge, 옛 `render-shell.ts` 대체 |
| 11 | `mobile` | 공통 | Single Edge |
| 12 | `charts`, `metrics` | 공통 | Single Edge(Chart.js) |
| 13 | `vm` | 도메인 | 옛 `render-inventory.ts`, `render-vm-detail.ts` 일부 |
| 14 | `vm-lifecycle` | 도메인 | 옛 `actions.ts`, `mutate.ts`: 생성·clone·template·lifecycle·rename·삭제·manage |
| 15 | `vm-checkpoint` | 도메인 | 옛 `vm-detail-extensions.ts`, `render-qos.ts`, `mutate.ts`: checkpoint·schedule·export/import·QoS·network/device |
| 16 | `vm-console` | 도메인 | 옛 `render-console.ts`, `vm-detail-extensions.ts` frame/keyboard |
| 17 | `vm-guest` | 도메인 | guest exec/channel/file |
| 18 | `accounts` | 도메인 | 로그인 페이지, 계정 생성, session, refresh, logout |
| 19 | `security` | 도메인 | RBAC chips, token rotation, diagnostics 번들(옛 `rbac.ts`, `render-ops.ts` 일부) |
| 20 | `monitor` | 도메인 | dashboard, monitoring signals(옛 `render-panels.ts`, `render-monitoring.ts`) |
| 21 | `ops` | 도메인 | jobs, activity/event center, evidence, troubleshooting(옛 `render-jobs.ts`, `render-activity.ts`, `evidence.ts`, `errors.ts`) |
| 22 | `help` | 공통 | Single Edge `help.js`, 문서 포털 |
| 23 | `bootstrap` | 부트스트랩 | `web/src/bootstrap.ts`(옛 `served-app.ts` 이벤트 바인딩 포함). `web/src/app.ts`는 정적 parity scaffold라 이름을 바꾸지 않는다 |

`web/src/served/state.ts`, `types.ts`, `errors.ts`의 공용 타입·상태는 `web/src/modules/state.ts`로 옮겨 1번보다 앞에 둔다.

## 5. 네임스페이스와 코드 규칙

- 모든 모듈은 `window.PCV = window.PCV || {}; (function (PCV) { ... })(window.PCV);` 꼴이고 공개 API는 `PCV.<module>`에만
  둔다. 전역 함수 노출(`window.navigateTo` 등)은 Single Edge가 `index.html` inline 핸들러에서 쓰는 것만 같은 이름으로 둔다.
- 이벤트는 셸 수준 위임 하나로 받는다. form 안 `type="submit"` 버튼은 click 위임이 건드리지 않는다(BL-0016 규칙,
  `web/node-tests/vm-detail-submit-guard.test.mjs`).
- DOM 쓰기는 `innerHTML` 직접 대입 수를 ratchet(`lint:domsafe`)으로 묶고 `escapeHtml`을 유지한다.
- TypeScript는 `@ts-nocheck` part로 두되 `tsc --noEmit`은 계속 돈다. 가져온 파일의 Single Edge 전용 분기(container,
  storage, OVN, cluster)는 제거하고 제거 목록을 spec 부록에 적는다.

## 6. 공통 계층과 제외 목록

가져오는 공통 모듈: shell, nav, theme, modal-core, modal, ui, uxlib, mobile, filter-state, charts, metrics, help, i18n 구조,
api 구조, endpoints 형식. 제외: container, vpc, storage, network(OVN/OVS 부분), selfhealing, totp, push, security의 Linux
부분, advanced. Desktop Node에만 있는 화면(evidence, troubleshooting, jobs 상세)은 `ops` 모듈로 둔다.

## 7. Host와 packaging 변경

- Host 정적 서빙(`DesktopNodeHostApplication.StaticAuth.cs`): content-type에 `.woff2`, `.woff`, `.ttf`, `.svg`, `.json`,
  `.webmanifest`, `.png`, `.ico`, `.md`, `.map` 추가, web root 아래 `vendor/`, `samples/` 하위 경로 서빙(경로 정규화와
  root 밖 거부), `sw.js` root scope. 소스 `web/src/`는 서빙하지 않는다.
- packaging: MSI/wrapper의 web 파일 inventory를 새 파일과 하위 디렉터리로 갱신한다. 새 packaging `*.Tests.ps1`은 만들지 않는다.
- Local API route는 바꾸지 않는다. WebSocket route와 Linux route를 넣지 않는다.

## 8. 계약 재기준선 목록

| 계약 | 바뀌는 것 |
| --- | --- |
| `web/contracts/web-static-contracts.mjs` `root-assets`, `visual-shell`, `workbench-frame`, `served-source-parts` 등 | 파일 이름(`app.bundle.js`, `style.css`), part 목록, 셸 selector |
| `web.static.single-edge-isolation` | 유지: Single Edge 트리 경로 import 금지(`../../ui/`). 복사된 파일은 `web/` 안에 있어 위반이 아니다 |
| `web.static.design-boundary` | 유지: `web/DESIGN.md`에 Linux route 없음 |
| static parity 스냅샷(`npm run generate:parity`), feature surface ledger, browser fixture | 새 bundle 기준으로 재생성 |
| `web/tests/PcvDesktopWeb.Static.Tests.ps1` 줄 범위 pin | `METADATA_LEDGER` 줄 범위 갱신 |
| Delivery 계약 pin(`Update-PcvContractSpecPins.ps1`) | AGENTS/PUBLIC_RELEASE_BOUNDARY 문장 변경 시 |
| S1~S3 시나리오 스크립트·node 테스트 | 경로와 selector |

## 9. 검증 전략

- task마다 `npm test --prefix web`(feature surfaces, tsc, served check, batches), 해당 node 테스트, `git diff --check`.
- Host 변경 task는 `dotnet test src/DesktopNode.Host.Tests -c Release`, packaging task는 Pester 두 종과 Delivery.
- 마지막(Task 17)은 clean HEAD에서 PR gate, `npm run test:required`, Pester 네 종, evidence -Check, public-source-safety.
- 브라우저 확인은 설치본 반영 뒤 train의 S3 재시연·S4 campaign에서 한다. 이 campaign은 정적·단위 검증까지다.

## 10. 비주장

- operational current `0.42.93-admin-smoke`는 그대로다. 설치본 반영은 train 0.42.94다.
- Linux Single Edge runtime·route·화면을 추가하지 않는다. public trusted signing과 external stable publication을 주장하지 않는다.
