# Single Edge 프론트엔드 구조 차용 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `HardcoreMonk/purecvisor`(Single Edge) `ui/`의 프론트엔드 구조(파일 배치, `window.PCV` 네임스페이스 모듈, 로그인 페이지 + 셸, 공통 계층 shell/nav/theme/modal/ui/uxlib/mobile/i18n/api, PWA, help 포털, vendor 자산, 빌드·lint 파이프라인)를 Desktop Node Web Console `web/`에 그대로 옮기고, Desktop Node 도메인(Hyper-V VM, checkpoint, console, guest, 계정, jobs/activity/evidence/troubleshooting)을 그 구조 위에 다시 얹는다. Linux 도메인 모듈(container, vpc, storage, OVN network, selfhealing, totp, push, security의 Linux 부분)은 제외한다.

**Architecture:** 분석 근거는 2026-10-10 Claude Doc "Single Edge 프론트엔드 구조 차용 분석"과 이 저장소 `web/DESIGN.md` "Single UI Clone Mapping"이다. Single Edge `ui/`는 프레임워크 없는 Vanilla JS로 `index.html`(로그인 페이지 + 앱 셸) / `app.js`(부트스트랩) / `i18n.js` / `modules/*.js`(27개, `window.PCV` IIFE) / `style.css` / `sw.js`+`manifest.json`+`offline.html` / `vendor/` / `samples/` / `docs.html`+`guide.html`이고, Makefile `UI_MODULES` 순서로 concat → esbuild → `app.bundle.js`다. Desktop Node `web/`은 TypeScript part를 `build-served-asset.mjs`가 순서대로 이어 붙여 classic script 하나를 만드는 같은 빌드 모델이라, 소스는 TypeScript(`@ts-nocheck` part)로 두고 파일 배치·네임스페이스·공통 모듈 코드를 가져온다. 설계 spec `pcv-single-edge-frontend-structure-v1`과 ADR-0018이 Task 1에서 경계를 정한다. branch는 `lane1/single-edge-frontend-structure-20261010` 하나, PR 하나(Task 17)다.

**Tech Stack:** TypeScript(`tsc --noEmit`), Node.js(`build-served-asset.mjs`, `node --test`, eslint, domsafe ratchet node 이식), C#/.NET 10(Host 정적 서빙), WiX/packaging Pester, PowerShell 7 + Pester 5, git/gh

## 사용자 결정 (2026-10-10)

승인 원문: `1,2,3,4` (2026-10-10 "Single Edge 프론트엔드 구조 차용 분석" 보고의 승인 1~4에 대한 답. 요청 원문: "'https://github.com/HardcoreMonk/purecvisor' 내가 제공하는 오픈소스에 접근, 분석하고 프론트엔드 구조를 그대로 차용해 줘")

| 항목 | 범위 |
| --- | --- |
| 1 | B안: 셸·공통 계층·빌드 파이프라인을 그대로 차용하고 도메인은 Desktop Node Local API로 다시 얹는 campaign `single-edge-frontend-structure-20261010`을 연다. Lane 0/1만, host mutation 없음. branch push·PR, green CI 뒤 merge(`merge_policy=after-green-ci`). |
| 2 | 범위에 PWA(Task 15)와 help 포털(Task 14의 `docs.html`/`guide.html`)을 포함한다. |
| 3 | S4 campaign과 train 0.42.94는 이 campaign 뒤로 미룬다. 그 train이 BL-0016 수정(queue 79)과 새 프론트엔드를 한 번에 싣고, 그 설치본에서 S3 브라우저 예약 저장 재시연과 S4를 한다. train 출발 승인 문구는 이 campaign이 닫힐 때 `next_approval_required`로 묻는다(`docs/DEVELOPMENT_PROCEDURE.md` §10). |
| 4 | 분석을 Claude Doc 보고서로 남긴다(2026-10-10 작성). |
| 이관 | `s3-checkpoint-20261010` Task 4(C5 runner 확인, `not_before` 2026-10-19)를 Task 18로 옮긴다. 문장과 push, PR, green CI 뒤 merge는 원래 승인 그대로다. |

## Global Constraints

- Lane 0/1만 쓴다. 설치본, service, 방화벽, VM을 건드리지 않는다. `current-evidence.json`과 `release-train.json`의 `trains`는 바꾸지 않는다(queue 행은 Task 17 merge 뒤).
- Desktop Node Local API route/계약은 고정한다(`web/DESIGN.md` Porting Order 1). 새 route, WebSocket route(`/ws/events`), Linux route(`/containers`, `/storage`, `/ovn`, `/networks`, `/auth/token`)를 넣지 않는다. loopback 기본값(`http://127.0.0.1/`, `:7777`)은 그대로다.
- Single Edge에서 가져온 파일은 머리에 출처 주석(`Ported from purecvisor ui/<path> (Apache-2.0, same author)`)을 적고, vendor 자산은 `THIRD_PARTY_NOTICES.md`에 라이선스(Pretendard OFL-1.1, Coolicons CC BY 4.0, Chart.js MIT)를 적는다. qrcode, noVNC는 가져오지 않는다.
- 공개 저장소 내용 규칙(AGENTS.md): 사용자 홈 경로, LAN 사설 IP, 호스트명, token, private 저장소 URL을 적지 않는다. Single Edge 저장소 URL은 적어도 된다(사용자 공개 저장소).
- 옛 구조(`web/src/served/*.ts`, `web/app.js`, `web/styles.css`)는 Task 16에서 제거할 때까지 빌드·계약을 깨지 않게 둔다. 중간 task에서 `npm test --prefix web`이 red가 되면 그 task 안에서 고치거나 멈춘다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다). 한 task가 한도를 넘으면 멈추고 나눠서 보고한다. Lane 2·3는 열지 않는다.

## Task 1: 설계 spec, ADR-0018, 경계 문서, 라이선스 고지

- [x] `docs/superpowers/specs/2026-10-10-purecvisor-desktop-node-single-edge-frontend-structure-design.md`(`pcv-single-edge-frontend-structure-v1`)를 쓴다: 파일 배치표(Single Edge `ui/` ↔ Desktop Node `web/`), 모듈 순서표(`UI_MODULES` 대응, `web/src/modules/<name>.ts`), `window.PCV` 네임스페이스 규칙, 공통/도메인 계층, 제외 목록, 계약 재기준선 목록, 검증 전략. `docs/adr/0018-single-edge-frontend-structure-adoption.md`(상태 채택: Single Edge `ui/` 구조와 공통 모듈 코드 차용 허용, Linux runtime·route·표면 금지 유지, vendor 라이선스 고지, `single-edge-isolation` 계약은 경로 import 금지로 유지)를 쓰고 `docs/ADR_INDEX.md`에 더한다. `docs/CODING_GUIDE.md` 3.2와 `docs/PUBLIC_RELEASE_BOUNDARY.md`의 "Single Edge 표면" 문장을 "Linux Single Edge runtime·route·화면은 추가하지 않고, 프론트엔드 구조·공통 모듈은 ADR-0018로 차용한다"로 좁힌다. `THIRD_PARTY_NOTICES.md`를 만들고 `docs/DOCUMENTATION_INDEX.md`에 spec·ADR 줄을 더한다. 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`(계약 spec pin이 깨지면 `Update-PcvContractSpecPins.ps1 -Apply`), `git diff --check`. 로컬 commit.

실행 기록(2026-10-10): 설계 spec `pcv-single-edge-frontend-structure-v1`(파일 배치표 13행, 모듈 순서표 23개, 네임스페이스·이벤트 위임 규칙, 제외 목록, Host·packaging 변경, 계약 재기준선 7종, 검증 전략)과 ADR-0018(채택, 결정 마커 4개)을 썼다. `docs/ADR_INDEX.md`에 2026-10-10 현재 기준 절과 적용 결정 행, `docs/DOCUMENTATION_INDEX.md`에 ADR·spec 줄을 더했다. `docs/CODING_GUIDE.md` 3.2와 `docs/PUBLIC_RELEASE_BOUNDARY.md`의 Single Edge 문장을 "Linux route·화면 제외, 구조·공통 모듈은 ADR-0018로 차용"으로 좁혔다. `THIRD_PARTY_NOTICES.md`를 만들었다(Pretendard OFL-1.1, Coolicons 1.2.2 CC BY 4.0, Chart.js 4.4.4 MIT, Single Edge 공통 모듈 Apache-2.0). 검증: `Update-PcvContractSpecPins.ps1 -Apply` ok(changed 0), Delivery 779/779(Release), `test:public-source-safety`는 BL-0015 기존 2건만, `git diff --check` 통과. host mutation 없음.

## Task 2: 빌드 파이프라인

- [ ] `web/scripts/build-served-asset.mjs`를 Single Edge `ui-bundle` 모델로 바꾼다: 모듈 순서표를 `web/src/modules.json`(또는 스크립트 상수)에 두고 `web/src/modules/*.ts` 전부가 순서표에 있는지 검사(누락이면 실패), `web/src/app.ts`(부트스트랩)를 마지막에 붙이며, 출력은 `web/app.bundle.js`에 `const PCV_UI_SOURCE_SHA1='…'` 배너를 단다. `--check`는 출력 동일성을 본다. `web/package.json`에 `lint`(eslint, `eslint.config.js` Single Edge 설정 이식)와 `lint:domsafe`(`scripts/domsafe_ratchet.py`를 `web/scripts/domsafe-ratchet.mjs`로 이식, `innerHTML` 직접 대입 수 ratchet)를 더하고 `npm test`에 잇는다. 이 task에서는 옛 part도 같은 파이프라인으로 묶여 `app.js`와 `app.bundle.js`가 모두 만들어지게 둔다. 검증: `npm run build:served`, `npm test --prefix web`, `node --check web/app.bundle.js`, `git diff --check`. 로컬 commit.

## Task 3: Host 정적 서빙 확장

- [ ] `src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs`의 content-type 표에 `.woff2`, `.woff`, `.ttf`, `.svg`, `.json`, `.webmanifest`, `.png`, `.ico`, `.md`, `.map`를 더하고, web root 아래 하위 디렉터리(`vendor/`, `samples/`, `modules/` 소스는 제외)의 파일을 경로 정규화 검사(`..` 금지, root 밖 금지) 뒤 서빙한다. `sw.js`는 root scope로 서빙한다. Host 테스트(`src/DesktopNode.Host.Tests`)에 content-type·하위 경로·경로 탈출 거부 시험을 더한다. 검증: `dotnet test src/DesktopNode.Host.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 4: packaging·installer 파일 inventory

- [ ] 제품 wrapper와 MSI가 web root 전체(새 파일과 하위 디렉터리)를 싣도록 `packaging/windows-desktop-node/`의 web 파일 목록(WiX source, payload manifest, 관련 Pester 기대값)을 갱신한다. 새 packaging `*.Tests.ps1`은 만들지 않고 기존 시험의 기대값만 바꾼다. 검증: `Invoke-Pester packaging/windows-desktop-node/tests`, `Invoke-Pester packaging/windows-desktop-node/installer/tests`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 5: vendor 자산과 style.css

- [ ] `web/vendor/pretendard/`(css + woff2 6개), `web/vendor/coolicons/`(svg sprite + README), `web/vendor/chart.umd.min.js`를 Single Edge에서 가져오고 `THIRD_PARTY_NOTICES.md`와 맞춘다. `web/style.css`를 Single Edge `ui/style.css` 원문 base로 만들고 Desktop Node 전용 selector(`web/styles.css`에만 있는 것)를 병합하며 Linux 전용 selector는 제거 목록으로 적는다. `web/samples/`에 `design-system-preview.html`, `supanova-preview.html`을 가져온다(`web/mockups/`는 그대로). 검증: `npm test --prefix web`(static parity는 Task 16까지 옛 파일 기준), 파일 크기 합 기록, `git diff --check`. 로컬 commit.

## Task 6: index.html 로그인 페이지와 셸

- [ ] `web/index.html`을 Single Edge `ui/index.html` 구조(CSP meta, 테마 부트스트랩 inline script, `#login-page` 로그인 페이지, `#app` 셸: `shell-sidebar`, `shell-topbar`, `main.content.shell-content`, manifest·icon 링크, 스크립트 순서 `vendor/chart.umd.min.js` → `i18n.js` → `app.bundle.js`)로 다시 쓰고, Desktop Node 콘텐츠 섹션(dashboard, vms, network, jobs, activity, evidence, troubleshooting)과 폼 id를 콘텐츠 영역으로 옮긴다. 아이콘 `icon-192.png`, `icon-512.png`를 Desktop Node용으로 만든다. 검증: `npm test --prefix web`, 브라우저 수동 확인 대신 `node --test` DOM 정적 검사(id 존재), `git diff --check`. 로컬 commit.

## Task 7: 공통 모듈 1 (shell, nav, theme, modal)

- [ ] `web/src/modules/shell.ts`, `nav.ts`, `theme.ts`, `modal-core.ts`, `modal.ts`를 Single Edge 원문에서 가져와(`window.PCV` IIFE 유지, `@ts-nocheck`) Desktop Node nav 모델(views 7개, `data-nav`), 테마 allowlist(supanova 계열 + Desktop Node `contrast`), modal 호출부에 맞춘다. 옛 `render-shell.ts`의 topbar·rail·sidebar 렌더를 이 모듈로 대체하는 전환 지점을 적는다. 검증: `npm test --prefix web`, `node --check web/app.bundle.js`, `git diff --check`. 로컬 commit.

## Task 8: 공통 모듈 2 (ui, uxlib, mobile, filter-state, charts, metrics, i18n)

- [ ] `ui.ts`, `uxlib.ts`, `mobile.ts`, `filter-state.ts`, `charts.ts`(Chart.js), `metrics.ts`를 가져오고, `web/i18n.js`(Single Edge `I18N` 구조, ko·en 사전은 Desktop Node 문구)를 만든다. 옛 `table.ts`, `summary.ts`의 헬퍼를 `uxlib`/`ui`로 흡수한다. 검증: `npm test --prefix web`, `node --test`(i18n 키 ko·en 쌍 검사), `git diff --check`. 로컬 commit.

## Task 9: api, endpoints, events

- [ ] `api.ts`를 Single Edge `api.js` 구조로 만들되 Desktop Node auth(loopback session, account login/refresh/logout/session/rbac, 브라우저 token)만 쓰고, 403/401에서 stale token을 비우고 세션을 다시 만드는 재로그인 경로(S3 시연에서 본 문제)를 넣는다. `endpoints.ts`는 옛 `routes.ts`의 route registry를 Single Edge `endpoints` 형식으로 옮긴다. WebSocket 대신 옛 `job-polling.ts`를 `events.ts`(polling adapter)로 옮긴다. 검증: `npm test --prefix web`, `npm run test:web-contracts --prefix web`(route registry 계약), `git diff --check`. 로컬 commit.

## Task 10: 도메인 vm, vm-lifecycle 1

- [ ] `vm.ts`(목록, 상세, asset explorer, 선택)와 `vm-lifecycle.ts`의 생성·clone·template lock/unlock·start/stop/pause/save/restart·rename·삭제·manage를 옛 `render-inventory.ts`, `render-vm-detail.ts`, `actions.ts`, `mutate.ts`에서 옮긴다. submit 버튼 guard(BL-0016)는 이벤트 위임 규칙으로 유지한다(`web/node-tests/vm-detail-submit-guard.test.mjs` 경로 갱신). 검증: `npm test --prefix web`, `node --test web/node-tests/vm-detail-submit-guard.test.mjs`, `git diff --check`. 로컬 commit.

## Task 11: 도메인 vm-lifecycle 2

- [ ] checkpoint(생성·복원·삭제·schedule preview/set/clear), export/import, QoS(storage/network), network connect/device add를 옛 `vm-detail-extensions.ts`, `render-qos.ts`, `mutate.ts`에서 `vm-lifecycle.ts`(또는 `vm-checkpoint.ts`로 분리)로 옮긴다. 검증: `npm test --prefix web`, `node --test web/node-tests/s3-checkpoint-scenario.test.mjs`, `git diff --check`. 로컬 commit.

## Task 12: 도메인 vm-console, vm-guest

- [ ] `vm-console.ts`(frame 폴링·canvas·키 입력·text·Ctrl+Alt+Del, Single Edge `vm-console.js` 구조 + Desktop Node frame route)와 `vm-guest.ts`(guest exec/channel/file)를 옛 `render-console.ts`, `vm-detail-extensions.ts`에서 옮긴다. 검증: `npm test --prefix web`, `node --test web/node-tests/s1-*`(있으면), `git diff --check`. 로컬 commit.

## Task 13: 도메인 accounts, security, monitor

- [ ] `accounts.ts`(로그인 페이지·계정 생성·session·refresh·logout, Single Edge `accounts.js` 구조), `security.ts`(RBAC chips, token rotation, diagnostics 번들, 옛 `rbac.ts`·`render-ops.ts` 일부), `monitor.ts`(dashboard hero·summary·ops cockpit·monitoring signals, 옛 `render-panels.ts`·`render-monitoring.ts`)를 만든다. 검증: `npm test --prefix web`, `git diff --check`. 로컬 commit.

## Task 14: 도메인 ops와 help 포털

- [ ] `ops.ts`(jobs, activity/event center, evidence, troubleshooting: 옛 `render-jobs.ts`, `render-activity.ts`, `evidence.ts`, `errors.ts`)와 `help.ts`를 만들고 `web/docs.html`, `web/guide.html`, `web/guide-content.md`(Desktop Node 운영 가이드 요약, `docs/USER_GUIDE.md`에서 발췌)를 Single Edge 문서 포털 구조로 만든다. 검증: `npm test --prefix web`, `git diff --check`. 로컬 commit.

## Task 15: PWA

- [ ] `web/manifest.json`, `web/sw.js`(Single Edge 전략: 프리캐시 입력 목록, `CACHE_NAME` 빌드 시 bump, `CLEAR_CACHE` 메시지, push 관련 코드는 제외), `web/offline.html`을 만들고 빌드 파이프라인에 `sw.js` 캐시 이름 bump를 넣는다. Host가 `manifest.json`·`sw.js`를 올바른 content-type으로 주는지 Host 테스트에 더한다. 검증: `npm test --prefix web`, `dotnet test src/DesktopNode.Host.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 16: 계약 재기준선과 옛 구조 제거

- [ ] `web/src/served/*.ts`, `web/src/served-app.ts`, `web/app.js`, `web/styles.css`를 제거하고 `web/contracts/web-static-contracts.mjs`(`root-assets`, `visual-shell`, `workbench-frame`, `served-source-parts` 등)와 `web/tests/PcvDesktopWeb.Static.Tests.ps1`, feature surface ledger, static parity 스냅샷(`npm run generate:parity`), browser fixture, `web/node-tests/*`(S1~S3 시나리오 테스트 경로), Delivery 계약 pin, `web/DESIGN.md`(Single UI Clone Mapping을 구조 차용 표로 갱신)를 새 구조로 다시 기준선 잡는다. 검증: `npm run test:required --prefix web`, `Invoke-Pester web/tests`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 17: 종료 검증, push, PR, merge

- [ ] clean HEAD에서 `dotnet build src/DesktopNode.sln -c Release`, PR gate(`Invoke-PcvPrGate.ps1`), `npm run test:required --prefix web`, Pester 네 종(packaging, installer, web, manual-admin PR gate), `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `npm run test:public-source-safety --prefix web`(BL-0015 기존 2건 외 0), `git diff --check origin/main...HEAD`. push, PR, green CI 뒤 merge(`pcv-ship`), merge 뒤 main run green 확인, 다음 branch 첫 commit에서 release-train queue 행(area web, risk M, lane2_probe `web.console.shell`)을 더한다. 닫을 때 `next_approval_required`에 train 0.42.94 출발 승인 문구(queue 78·79·이 PR, package build, pair host mutation 범위, S3 재시연·S4 campaign)를 적는다.

## Task 18: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `s3-checkpoint-20261010` Task 4(그 전 `audit-green-lean-20261009` Task 9, `s2-template-clone-20261009` Task 2, `s1-installed-20261008` Task 11, `adr17-s1-console-20261008` Task 14)다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.93-admin-smoke` 그대로다. 설치본 반영과 S3 재시연·S4는 이 campaign 뒤 train 0.42.94에서 한다.
- Linux Single Edge runtime·route·화면을 추가하지 않는다. Web Console은 Desktop Node Local API와 Hyper-V 화면만 활성화한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
