# Web Console 본문 원본 정합 구현 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

> 이 계획은 campaign `ui-parity-design-20261109` Task 3이 초안으로 썼고, 2026-11-09 사용자 승인으로 campaign `ui-parity-20261109`가 열었다.

**Goal:** ADR-0020과 설계 `pcv-web-content-parity-v1`대로 Web Console 본문 화면을 원본 `fd754e2`의 골격과 부품으로 다시 그린다. 지금 틀린 동작 9건을 먼저 고치고, VM 화면을 목록·상세 골격으로 바꾼 뒤, 데이터 화면을 화면별 렌더 등록표로 옮기고, 마지막에 옛 렌더러를 지운다.

**Architecture:** 근거는 감사 `docs/web-console-upstream-parity-audit-2026-11-09.md`(발견 61건)와 설계 `docs/superpowers/specs/2026-11-09-purecvisor-desktop-node-web-console-upstream-parity-design.md`다. ADR-0018 구조와 ADR-0019 정보 구조·상호작용 규칙은 유지한다. 단계 U1~U8이 task 하나씩이고 task마다 Lane 1 PR 하나다. branch는 `lane1/ui-parity-<단계>-<yyyymmdd>`이고, merge된 뒤의 새 branch는 `origin/main`에서 만든다. Web 파일은 MSI payload라 PR마다 `docs/ga-ready/release-train.json` `queue` 행을 더한다.

**Tech Stack:** TypeScript Web Console(`web/src/modules/*.ts`, `web/src/modules.json`, `npm run build:served --prefix web`), node 시험(`web/node-tests`), 정적 계약(`web/contracts`), Playwright·axe(`npm run browser:ui-gates`), web Pester(`web/tests`), Delivery 계약 시험

## 사용자 결정 (2026-11-09)

승인 원문: `UI 개편 구현 campaign 열기와 ADR-0020 채택 승인` (campaign `ui-parity-design-20261109` `next_approval_required` 1에 대한 답)

| 항목 | 범위 |
| --- | --- |
| 1 | ADR-0020을 채택한다. opening commit에서 상태를 채택으로 바꾸고 `docs/ADR_INDEX.md` 적용 표로 옮겼다 |
| 2 | Task 1~8(U1~U8)을 Lane 1로 구현한다. grok 구현, Claude 리뷰 |
| 3 | task마다 push, PR, Required CI 다섯 check green 뒤 merge(2026-11-09 상시 merge 승인) |
| 4 | host mutation 없음. release train 출발과 설치본 시연은 이 campaign 밖이고 따로 승인한다 |

## Global Constraints

- operational current는 `0.42.99-admin-smoke`다. 이 campaign은 `current-evidence.json`을 쓰지 않는다. 보존 VM `pcv-guest-installed-04253-r1`, template `pcv-it-s2-source`, BL-0028 부모 VHD, 설치된 service와 계정은 건드리지 않는다.
- Linux runtime 화면·코드(KVM/libvirt/LXC/ZFS/OVS/OVN, noVNC, WebSocket)는 가져오지 않는다.
- 새 Local API route를 더하지 않는다. 사용률 metrics, ISO 목록, 계정 역할·삭제·재활성화는 설계 4.3절대로 따로 설계한다. task가 route를 요구하면 멈추고 보고한다(`new-design-required`).
- 새 서드파티 자산(폰트, 아이콘, 라이브러리)을 넣지 않는다.
- `web/app.bundle.js`, `web/sw.js`는 `npm run build:served --prefix web`로만 만든다.
- ratchet을 올리지 않는다: `web/src/modules/vm.ts` 줄 수 상한, `lint:domsafe` ceiling. 올려야 하면 멈추고 보고한다.
- 새 문구는 i18n 키로 두고 ko를 기본, en을 같은 키로 맞춘다.
- 계약·시험이 붙잡는 id를 바꾸면 같은 PR에서 계약을 고친다. Web 문구·binding을 바꾸면 pin된 다섯 곳과 web Pester를 함께 고친다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다.
- public 문서·PR에는 사용자 홈 경로, LAN 사설 IP, 호스트명, token·password 값을 적지 않는다. 캡처는 가짜 API 데이터로만 찍는다.
- 역할: grok(Herdr pane)이 구현하고, Claude가 diff·시험·캡처를 확인한 뒤 commit한다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

공통 검증(task마다):

```powershell
npm run build:served --prefix web
npm run test:required --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
dotnet test src/DesktopNode.Delivery.Tests -c Release
git diff --check
```

마지막 task는 clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`와 PR gate(`packaging/windows-desktop-node/tools/Invoke-PcvPrGate.ps1`)를 더 돌린다.

## Task 1: U1 지금 틀린 동작과 작은 정리

- [x] 설계 4.4절. B-11 `mobile.ts:219,242,328-329`를 `PCV.vm.queueVmLifecycle`·`queueVmDelete`로 바꾸고 `mobile.ts`가 부르는 `PCV.vm.*`가 export(`vm.ts:2738`)에 있는지 node 시험으로 고정한다. B-07 로그인 노드 칸은 새 route 없이 Web Console version과 접속 주소만 보인다. B-08 TLS pill에 scheme을 보인다. B-09 Ctrl+K가 새 팔레트를 연다. B-10 단축키 Ctrl+K, `/`, `?`, Esc를 등록하거나 도움말에서 뺀다. B-12·C-19 로그아웃(`api.ts:112-126`)에서 열린 modal을 닫는다. B-14 경고 영역(`monitor.ts:697-715`)과 진단(`index.html:459-465`)의 로그인 폼을 지우고 "다시 로그인"이 오버레이를 연다. B-15 도움말 카탈로그, B-18 사이드바 `aria-label`, C-17 화면 제목, A-23 요약 탭 원시 필드 라벨. 공통 검증, release-train queue 행. push, PR, green CI 뒤 merge.

실행 기록(2026-11-09): checkpoint 1은 grok 작업이 37분 걸려 Lane 1 예산 30분을 넘어 멈췄다(회로 개방, 변경은 commit하지 않고 보관). 사용자 승인 `U1 checkpoint 2 승인`으로 이어 갔다. 고친 것: B-11 모바일 전원·삭제를 `queueVmLifecycle`·`queueVmDelete`로 바꾸고 모바일 일괄 삭제 UI는 숨김(U3 공통 모달 뒤 되살림), B-07 로그인 칸은 새 route 없이 접속 주소와 이미 읽은 version만, B-08 TLS pill에 scheme, B-09 Ctrl+K와 검색 버튼이 새 팔레트를 염(옛 `#command-palette`는 U8까지 숨김), B-10 단축키는 Ctrl+K·`/`·`?`·Esc만 등록·안내, B-12·C-19 로그아웃에서 modal·팔레트·메뉴를 닫음, B-14 경고 영역과 진단의 로그인 폼을 지우고 "다시 로그인" 버튼, B-15 도움말 카탈로그에 비밀번호 route 2개와 계정 화면 view 값, C-17 화면 제목(작업, 증적, 진단), A-23 요약 readback 라벨과 Linux 전용 필드 숨김. B-18은 바꿀 것이 없었다: 사이드바 nav에는 이미 `copy.shell.nav` 라벨이 있고, 감사가 가리킨 `shell.ts:152`는 검색 버튼이었다(라벨을 붙이면 axe `label-content-name-mismatch`가 나서 되돌렸다). 상한: `vm.ts` 2,739→2,734줄(ratchet 상한을 내리고 policy spec pin을 `Update-PcvContractSpecPins.ps1 -Apply`로 갱신), domsafe 38→37. axe 기준선은 지운 로그인 요소 때문에 이름이 바뀐 선택자 27개를 더했다(`--prune` 하지 않음). 검증: `npm run test:required --prefix web`에서 node 계약 369/369, parity, fixture 통과. browser gate는 첫 실행이 알려진 `vm-detail-summary` bgGradient flake 6건으로 실패했고 재실행 `ok=true`(violations 0). web Pester 50/50, module-size ratchet 3/3, `dotnet test src/DesktopNode.Delivery.Tests -c Release` 785/785, `git diff --check` 통과. host mutation 없음.

task 크기(2026-11-09 사용자 승인 `U1 checkpoint 2 승인`): Task 2부터는 한 task가 Lane 1 checkpoint 하나(30분, grok 작업 시간 포함)에 들어가도록 grok 지시 하나(항목 3~4개, 20분 안쪽, 멈춤 시각 포함)로 자른다. Task 2를 시작할 때 Task 2~8을 `Task Na`, `Task Nb`로 나누는 plan·campaign 변경을 먼저 commit한다.

## 단계와 task 나누기 (2026-11-09)

사용자 승인 `U1 checkpoint 2 승인`에 따라 Task 2부터는 task 하나가 Lane 1 checkpoint 하나(30분, grok 작업 시간 포함)에 들어가게 나눴다. task마다 grok 지시 하나(20분 안쪽, 멈춤 시각 포함)와 Claude 리뷰·검증이다. 같은 단계의 task는 한 branch(`lane1/ui-parity-u<단계>-<yyyymmdd>`)에 로컬 commit으로 쌓고, 단계의 마지막 task가 release-train queue 행, push, PR, green CI 뒤 merge를 한다. 그래서 PR과 queue 행은 단계마다 하나다.

## Task 2a: U2 `vm-list.ts` 분리

- [x] 동작을 바꾸지 않고 VM 목록 쪽 함수(목록 렌더, 필터, 상태 칩, 정렬, 템플릿 목록 등)를 새 모듈 `web/src/modules/vm-list.ts`(`modules.json`에서 `vm` 앞)로 옮긴다. `PCV.vm` export 이름과 동작은 그대로다. `vm.ts` 줄 수 상한을 실제 줄 수로 낮추고 새 모듈을 ratchet 규칙대로 등록하며, policy spec pin을 `Update-PcvContractSpecPins.ps1 -Apply`로 갱신한다. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): grok이 14분(10:48–11:01)에 옮겼다. 옮긴 함수는 필터(`vmQueryText`, `matchesVmQuery`, `matchesVmStateChip`, `matchesVmFilter`), 정렬(`getVmUpdatedValue`, `compareVms`), 행 주 조작(`vmListPrimaryButton`), 상태 칩(`paintVmChips`), 템플릿 목록(`templateRowButton`, `renderTemplates`), 목록 렌더(`vmScalarText`, `vmCpuText`, `vmMemoryText`, `renderVms`)다. `modules.json`에서 `vm-list`는 `vm` 바로 앞이다(쓰는 값은 `core`가 window에 둔 것이고, `vmPowerKind`만 `vm.ts`가 `window.vmPowerKind`로 공개해 호출 시점에 읽는다). Claude 확인: `vm.ts`에서 지운 줄과 `vm-list.ts` 줄을 비교해 새 줄은 모듈 감싸기와 export 나누기뿐이고, 다른 모듈이 이름으로 직접 부르는 옮긴 함수는 `renderVms` 하나이며 window에 공개돼 있다. 줄 수: `vm.ts` 2,734→2,538(ratchet 상한도 2,538, spec pin 갱신), `vm-list.ts` 205(ratchet 계약은 목록에 있는 파일만 재고 모듈 수 31을 고정하므로 등록하지 않음). 고친 계약: `web-static-contracts.mjs`와 negative 시험의 `compareVms` 경로, `bundle-load.test.mjs` 네임스페이스. domsafe 37 그대로. 검증(Claude 재실행): `npm run test:required --prefix web` exit 0(browser gate `ok=true`, violations 0), web Pester 50/50, module-size ratchet 3/3, Delivery 785/785, `git diff --check` 통과. host mutation 없음.

## Task 2b: U2 목록·상세 골격

- [x] 설계 4.1절의 두 상태. 고르지 않으면 전체 폭 표, 고르면 왼쪽 260px 목록과 넓은 상세(`web/index.html:291-347`, `web/style.css:5212-5225`). 선택 카드를 지우고 이름은 상세 헤더 h2 하나다. `HN.pagehead`(제목, 설명, `+ 새 VM`). 콘솔 탭 맞춤 배율이 넓어진 폭을 쓰는지 확인한다. 설계 7절의 VM 골격 계약·시험 8곳을 고친다. 공통 검증, 가짜 API 캡처(1440, 768, 390)를 원본 캡처와 비교. 로컬 commit.

실행 기록(2026-11-09): checkpoint 4(11:05~11:26)에서 grok이 골격을 구현했지만 `bootstrap.ts` 512줄(상한 499)과 browser gate assertion 10건 실패로 멈췄다. 원인은 더 보기 메뉴가 Esc를 먼저 처리(`preventDefault`)해 메뉴를 닫은 뒤, `bootstrap.ts` 스냅샷이 아무것도 열려 있지 않다고 보고 VM 선택을 해제해 상세와 메뉴가 사라진 것이다. "U2b checkpoint 2 승인" 뒤 checkpoint 5(15:33~15:46)에서 Esc 선택 해제를 `vm-list.ts` 모듈 로드 때 등록하는 document keydown으로 옮기고, `defaultPrevented`이거나 메뉴·대화상자·확인 창 안에서 누른 Esc는 건너뛰게 했다. `bootstrap.ts` 497줄(HEAD 그대로), `vm.ts` 2,538→2,508줄(slack 50 안이라 ratchet 그대로), `vm-list.ts` 205→241줄, domsafe 천장 37→35, axe 기준선 1,906→2,054(+148, `--prune` 없음, stale 551 경고). 검증: `npm run build:served`, `npm run test:required` exit 0(web-contracts 370 pass, browser gate assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean. 가짜 API 1280 캡처(`vms`, `vm-detail-summary`, `vm-detail-console`)에서 두 상태, 고른 행 표시, 이름 한 번, 넓어진 상세·콘솔 폭을 확인했다. 콘솔 맞춤 배율은 이미 stage 폭으로 계산해 `vm-console.ts`는 고치지 않았다. 768·390 캡처 비교와 260px 목록에서 상태·템플릿 배지와 VM 이름이 글자 중간에서 줄바꿈되는 문제(`Faile d`, `Runni ng`)는 Task 2c로 넘겼다.

## Task 2c: U2 바로 가는 링크, 빈 상태, 목록 줄바꿈

- [x] hash `#/vms/<vm-id>/<tab>`(`core.ts:351-354`)와 뒤로 가기, 없는 VM 처리. 빈 상태 CTA와 읽기 실패(`components.emptyState`·`loadError`). Task 2b에서 넘어온 260px 목록의 배지·이름 줄바꿈 수정. node 시험. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 6(15:42~16:06). grok이 `vm-list.ts`에 다음을 넣었다. hash 바로 가는 링크(`#/vms/<id>/<tab>`. 목록에서 상세로 갈 때 `pushState` 한 번, 탭 변경은 `replaceState`, 뒤로 가기·닫기·Esc는 `#/vms`, 없는 id는 목록과 toast `vm.list.missing`). 빈 상태 `emptyState`와 CTA `open-create-vm`, 필터 빈 상태의 필터 지우기, 읽기 실패 `loadError`와 다시 시도. 260px 목록 배지 `nowrap`, 이름 말줄임(`title`). 리뷰에서 결함 하나를 찾았다. `render()`(`monitor.ts:872-882`)가 모든 화면에서 `renderVms()`를 부르므로, VM을 고른 채 다른 화면으로 가면 hash가 VM 경로로 바뀐다. Claude가 `syncVmListHash`에 `state.activeView !== 'vms'` 가드 한 줄을 더했다. `vm-list.ts` 241→542줄(ratchet 미등록), `vm.ts` 2,508, `bootstrap.ts` 497, domsafe 천장 35→34, axe 기준선 2,054→2,118(+64, `td[title]` 선택자, `--prune` 없음). 검증: `npm run test:required` exit 0(browser gate assertions 121 failed 0 violations 0), web Pester와 ratchet Pester 53/53, Delivery 785/785, `git diff --check` clean. 가드 회귀 node 시험, 600px 이하 확인, 768·390 캡처 비교는 Task 2d로 넘겼다.

## Task 2d: U2 캡처 비교와 PR

- [x] 가짜 API 캡처(1440, 768, 390)를 원본 캡처와 비교하고, 600px 이하에서 목록과 상세가 하나씩 보이는지 확인한다. VM을 고른 채 다른 화면으로 가도 hash가 그 화면을 따르는지 node 시험을 더한다. 공통 검증, release-train queue 행. push, PR(U2 묶음), green CI 뒤 merge.

실행 기록(2026-11-09): checkpoint 7(16:07~). grok이 다음을 넣었다. `shell-views.test.mjs` hash 시험에 VM을 고른 채 `#/dashboard`로 `render()`해도 hash가 그대로이고 `pushState`/`replaceState`가 불리지 않는 경우. `verify-browser-ui-gates.mjs`에 `--viewport <W>x<H>`(없으면 1280×720 그대로). 캡처 결과는 다음과 같다. 1440×900: 두 상태, 고른 행, 이름 한 번이 맞다. 원본(`upstream-shell-vm-summary-1440`)과 달리 머리와 도구 줄이 카드 안에 있고, 검색이 목록 열이 아니라 위 도구 줄에 있다. 260px 목록은 세 열이라 이름이 `pcv-de...`로 잘린다(BL-0037). 768×1024: 한 열, 고르면 상세만 보인다. 390×844: 데스크톱 셸 대신 `mobile.ts` 모바일 화면이 떠서, 600px 이하는 모바일 화면이 맡는다. 이 크기의 gate assertion 4건 실패는 그 전환 때문이다. 768에서 axe violations 34건은 모두 셸 `.shell-search`의 `aria-command-name`이다(좁은 화면에서 글자가 숨어 이름이 없음, BL-0036, U2 밖). 기본 1280 gate는 `test:required` exit 0(web-contracts 372 pass, assertions 121 failed 0 violations 0), web Pester 50/50, `git diff --check` clean. clean HEAD에서 Delivery 785/785와 `Invoke-PcvPrGate.ps1` ok 뒤 push, PR #124.

## Task 3a: U3 행 메뉴

- [x] 행 `⋯` 메뉴와 우클릭(ContextMenu 키, Shift+F10 포함)이 ADR-0019 더 보기 메뉴와 같은 항목을 연다. 상태에 맞지 않는 항목은 끈다. 행 주 조작 하나는 그대로다. node 시험: 메뉴 키보드(열기, 화살표, Esc). 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 1(16:47~17:12)에서 grok이 행 `⋯` 버튼, 우클릭, ContextMenu 키·Shift+F10, 상세 더 보기와 같은 항목과 끄는 규칙 helper(`vmMoreMenuModel`, `vmMoreMenuItemsHtml`)를 넣었다. 하지만 context 압축이 두 번 일어나 마지막 검증 전에 멈췄다(diff 보존). 권장안 자동 승인으로 checkpoint 2(17:12~17:15)에서 Claude가 검증과 리뷰를 했다. 주기적 `render()`에도 열린 행 메뉴를 새 버튼에 붙여 유지하는 `keepVmRowMenuAfterRender`를 더했다(상세 더 보기와 같은 동작, VM이 목록에서 빠지면 닫힘). 첫 `test:required` 실패 5건은 checkpoint 3(17:15~17:31)에서 grok이 고쳤다. 상세 복제 버튼이 IIFE 지역 `openVmCloneModal`을 직접 불러 행 메뉴와 시험 stub이 닿지 않던 것을 `window.openVmCloneModal`로 바꿨다. 그 대화상자가 남아 실패하던 logout 시험이 풀렸고, 로그아웃(`_closeOperatorSurfaces`)이 행 메뉴도 닫는다. 메뉴 항목 계약 검사 위치를 `vm-list.ts`로 옮겼다(정규식 그대로). `vm.ts` 2,508→2,499줄, `vm-list.ts` 542→833줄, domsafe 34, axe 기준선 2,118→2,126(+8, `⋯` 버튼 nonBmp, `--prune` 없음). 검증: `npm run test:required` exit 0(web-contracts 377 pass, browser gate assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean.

## Task 3b: U3 정렬과 j/k

- [x] 열 머리를 누르면 그 열로 정렬하고 다시 누르면 방향을 바꾼다. 목록에 포커스가 있으면 j/k로 행을 옮긴다. node 시험. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 4(17:32~17:49). grok이 다음을 넣었다. 정렬할 수 있는 머리(이름, 상태, 세대, vCPU, 메모리, 표시)를 `<th aria-sort>` 안 `button[data-vm-sort]`로 바꿨다. 다른 열은 기본 방향(updated만 desc), 같은 열은 방향 전환이다. `state.vmSortDir`을 더했다. `compareVms`는 방향을 반영하고, 세대·vCPU·메모리는 숫자로 비교하며(없는 값은 끝), 동점은 이름 asc다. `#vm-sort`에 세대, vCPU, 메모리, 표시 옵션을 더해 머리와 맞췄다(라벨은 기존 `vm.list.col.*`). `#vm-table` 안 j/k는 보이는 다음·이전 행을 행 클릭과 같은 경로로 고르고 그 행에 포커스를 둔다. 끝에서 돌지 않고, 수정 키, 입력 요소, 메뉴·대화상자에서는 무시한다. `bootstrap.ts`와 `vm.ts`는 바꾸지 않았다. `vm-list.ts` 833→972줄, domsafe 34, axe 기준선 2,126→2,196(+70, 정렬 버튼 pseudoContent·방향 기호 nonBmp, `--prune` 없음). 검증: `npm run test:required` exit 0(web-contracts 379 pass, browser gate assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean. grok의 첫 gate 실행에서 `delete-confirm/accept-deletes` assertion이 한 번 실패했고, 뒤의 세 번은 통과했다(BL-0038).

## Task 3c: U3 선택과 일괄 시작·종료

- [x] 표 첫 열 체크박스와 선택 막대(선택 n개, 시작, 종료, 선택 해제). VM마다 job 하나와 자리 진행 줄. node 시험: job 수. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 5(17:50~18:13). grok이 다음을 넣었다. 표 첫 열에 행 체크박스(`{이름} 선택`)와 머리의 보이는 VM 모두 선택(일부면 `indeterminate`)을 두었다. 체크해도 상세는 열리지 않는다. `state.vmChecked`를 두고, 목록에 없는 id는 다시 그릴 때 지운다. 하나 이상 고르면 선택 막대(선택 n개, 시작, 종료, 선택 해제, 건너뜀 n)가 나온다. 시작은 꺼진 VM만, 종료는 확인 창 한 번 뒤 실행 중 VM만 대상이고, 템플릿 잠금·대기 중 VM은 건너뛴다. VM마다 `queueVmAction`을 한 번 부르고 `setVmActionPending`, `pcvTrackJob`, `startPolling`, `clearVmActionPending`을 단일 동작과 같이 쓴다. 막대 아래 VM마다 진행 줄을 두고 `trackJob`으로 갱신한다. 260px 상세 목록에서 숨기는 열을 1(체크박스), 4, 5, 6, 8로 옮겼다(1열에 남은 ellipsis 선택자는 숨은 열이라 효과가 없다). i18n 키 7개(ko·en)를 더했다. `vm-list.ts` 972→1,163줄, `core.ts` 1,584줄, `vm.ts`·`bootstrap.ts`는 바꾸지 않았다. domsafe 34, axe 기준선 2,196→2,256(+60, 열 이동으로 바뀐 선택자, `--prune` 없음). grok은 18:10에 마지막 `build:served` 전에 멈췄고, Claude가 다시 만들어 검증했다. 검증: `npm run test:required`에서 node 380 pass, browser gate 기준선 기록 뒤 `ok=true`(assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean.

## Task 3d: U3 일괄 삭제

- [x] 일괄 삭제 확인 모달(대상 이름 목록, 고른 개수를 입력해야 버튼이 켜짐)과 선택 막대 삭제 버튼. 모바일 일괄 삭제를 이 모달로 되살린다. node 시험: 개수 gate, VM마다 job 하나. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 6(18:14~18:29). grok이 `vm-list.ts`에 `queueVmBulkDelete(vmIds)`를 넣고 `PCV.vm`으로 내보냈다. 선택 막대에 위험 표시 삭제 버튼을 두었다. 실행 중, 템플릿 잠금, 대기 중 VM은 대상에서 빼고 그 수를 따로 보여 준다. 대상 0이거나 `operate`가 없으면 버튼이 꺼진다. 확인은 `confirmDialog` 한 번(`danger`, `typedName`=대상 개수, 설명에 대상 이름 목록)이다. 수락하면 VM마다 `desktopApi.deleteVm`을 한 번 부르고 `pcvTrackJob`, pending, `startPolling`, 진행 줄은 Task 3c와 같다. U1에서 지운 모바일 체크 선택과 일괄 삭제(`m-vm-bulk-delete`)를 되살렸고, 삭제는 같은 `PCV.vm.queueVmBulkDelete`만 부른다. i18n 키 4개(ko·en)를 더했다. `vm-list.ts` 1,163→1,233줄, `mobile.ts` 393줄, `vm.ts`·`bootstrap.ts`·CSS는 바꾸지 않았다. domsafe 34. 검증: `npm run test:required` exit 0(node 381 pass, browser gate `ok=true` assertions 121 failed 0 violations 0, 기준선 변화 없음), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean. PR은 Task 3e로 나눴다.

## Task 3e: U3 PR

- [x] clean HEAD 종료 검증(`npm run test:required`, `Invoke-PcvPrGate.ps1`), release-train queue 행. push, PR(U3 묶음, Task 3a~3d), green CI 뒤 merge, merge 뒤 `main` run 확인.

실행 기록(2026-11-09): checkpoint 7(18:29~). clean HEAD `06d958af`에서 `npm run test:required` exit 0(node 381 pass, browser gate assertions 121 failed 0 violations 0), `Invoke-PcvPrGate.ps1` ok(build, dotnet-test-release, module-size-ratchet), Delivery 785/785, `git diff --check origin/main...HEAD` clean. push 뒤 PR #125, release-train queue 행(merge_commit `06d958af`).

## Task 4a: U4 화면 렌더 등록표

- [x] 설계 4.2절. `nav.ts`에 `PCV.views.register(name, { render(mount, ctx) })`를 두고 옛 `renderActiveView()`가 등록된 화면을 건너뛰게 한다. 주기 새로 고침은 활성 화면의 `render`만 부른다. `HN.*` 반환 형식을 확인해 domsafe ceiling을 올리지 않는 경로를 정하고 실행 기록에 적는다. 화면은 아직 옮기지 않는다. node 시험: 등록된 화면 건너뜀. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 8(18:40~18:48). grok이 `nav.ts`에 `PCV.views`(`register`, `isRegistered`, `get`, 시험용 `unregister`, `renderActive(reason)`)를 넣었다. `register`는 nav route 이름과 `render` 함수만 받고, 같은 이름은 교체한다. `renderActive`는 활성 화면이 등록돼 있을 때만 그 `render(mount, ctx)` 하나를 부른다. `ctx`는 `{ reason, generation, isCurrent }`이고, `isCurrent`는 이동했거나 활성 화면이 바뀌면 false다. `render`가 throw하면 그 mount에만 `loadError`를 붙인다. mount 규칙: `#cb .app-view[data-view="<name>"]` 안의 `[data-view-mount]`, 없으면 섹션 자체다. 기존 자식은 지우지 않는다. `_renderContentPaint`는 섹션을 켠 뒤 `renderActive('navigate')`를 부르고, 같은 페인트 안에서 옛 `render()`가 다시 부르지 않게 막는다(`navPaintActive`). `monitor.ts` `render()`는 옛 그리기 뒤에 `renderActive('refresh')`를 부르고, `renderActiveView()`는 등록된 섹션의 표시를 nav에 맡긴다. 옛 그리기 함수 guard는 `pcvLegacyViewSkipped(name)`으로 두고, 각 화면을 옮길 때 건다. `HN.*`(`ui.ts`)는 `PCV.uxlib.el`로 DOM 노드를 돌려주므로, 등록 화면은 `HN`·`uxlib.el`로 노드를 만들어 mount에 붙이는 경로로 domsafe 천장(34)을 올리지 않는다. 등록된 제품 화면은 아직 없다. 검증: `npm run test:required` exit 0(node 382 pass, browser gate assertions 121 failed 0 violations 0, 기준선 변화 없음), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean.

## Task 4b: U4 네트워크

- [x] 네트워크를 등록표로 옮긴다: `HN.pagehead`, `filterBar` 종류 chip(스위치 `type` 값별)과 개수, `createDataTable`(이름, 종류, 연결 VM 수, 어댑터, 상태 pill), 768px 이하 카드, 빈 상태와 읽기 실패(문구 구분 node 시험), 하드코딩 패널 색을 token으로. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 9(18:48~19:11)에서 grok이 새 모듈 `web/src/modules/view-network.ts`(`PCV.views.register('network')`, 214줄)를 만들었다. 내용은 종류 chip(전체와 type별 개수), `createDataTable`(이름, 종류, 연결 VM 수, 어댑터, 상태 pill. 연결 VM 수는 `state.vms` 어댑터 switch로 VM 단위로 셈), skeleton, `loadError`(기존 parity 안내 재사용), `emptyState`, 경계 절(읽기 전용, 스위치 생성은 service만, NAT 제외 chip)이다. 옛 `vm.ts` `renderNetworkInventory`와 전용 helper를 지웠고, 768px 이하 카드와 token 색을 넣었다. 계약이 잡지 않던 요약 카드(source, 스위치 수, 기본 수)와 관리 OS 열은 뺐다. 검증 전에 한도에 걸려, 권장안 자동 승인으로 checkpoint 10(19:11~19:33)에서 이어 했다. Claude는 `vm.ts` ratchet을 2,538→2,389로 낮추고 pin을 갱신했으며(`Update-PcvContractSpecPins.ps1 -Apply`), domsafe 천장을 34→33으로 낮췄다. grok은 node 시험을 더하고 첫 `test:required` 실패를 고쳤다. 네트워크 머리와 검색은 VM 화면처럼 `index.html`의 정적 pagehead와 원래 `#network-filter` markup으로 두어 계약(`search-event-table`, i18n BL-0024)을 그대로 지켰다. 빈 상태에 `network-empty-state` class를 붙였다. fixture에서 canvas가 없을 때 `updateFavicon`이 멈추던 것(`getContext`)에 guard를 넣었다. `network-inventory:panel` 계약의 창은 pagehead가 들어가 240→800자로 넓혔다(mount 제거 결함 주입은 그대로 실패). axe 기준선 2,256→2,314(+58, `--prune` 없음). 검증: `npm run test:required` exit 0(node 383 pass, browser gate assertions 121 failed 0 violations 0), web Pester와 ratchet Pester 53/53, Delivery 785/785, `git diff --check` clean. PR은 Task 4c로 나눴다.

## Task 4c: U4 PR

- [x] clean HEAD 종료 검증(`npm run test:required`, `Invoke-PcvPrGate.ps1`), release-train queue 행. push, PR(U4 묶음, Task 4a~4b), green CI 뒤 merge, merge 뒤 `main` run 확인.

실행 기록(2026-11-09): checkpoint 11(19:33~). clean HEAD `a0178508`에서 `npm run test:required` exit 0(node 383 pass, browser gate assertions 121 failed 0 violations 0), `Invoke-PcvPrGate.ps1` ok(build, dotnet-test-release, module-size-ratchet), `git diff --check origin/main...HEAD` clean. push 뒤 PR #126, release-train queue 행(merge_commit `a0178508`).

## Task 5a: U5 작업

- [x] 작업 화면을 등록표로 옮긴다: 상태 chip과 종류 select, `createDataTable`(접수 시각, 종류, 대상, 상태 pill, 걸린 시간, 오류 요약), 정렬·검색·CSV, 다음 페이지 버튼(C-11), 영어 고정 문구를 i18n 키로. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 12(19:42~20:03). grok이 새 모듈 `web/src/modules/view-jobs.ts`(326줄, `register('jobs')`)를 만들었다. mount는 `#jobs-panel.jobs-panel[data-view-mount]`이며, id와 class를 유지해 bootstrap 위임 click이 그대로 동작한다. mount에는 다음을 그린다. 상태 chip(전체, 대기, 진행 중, 성공, 실패, 취소, 개수 포함. `state.jobStatusFilter`와 `#job-status-filter`가 같은 값), 종류 select(`state.jobOperationFilter`), `createDataTable`(접수 시각, 종류, 대상, 상태 pill, 걸린 시간, 오류 요약, 조작. 정렬과 CSV를 켜고, 검색은 기존 `#job-filter`). 조작은 cancel·retry·reconcile 버튼으로, `PCV.uxlib.el`과 기존 `data-action`·`data-job-id`·`data-job-cancel-scope`를 쓰고 `operate`가 없으면 꺼진다. 그 밖에 경계 요약 `job-edge-summary`, 빈 상태 두 가지와 읽기 실패의 서로 다른 문구, 다음 페이지 버튼(`load-next-jobs` → `loadNextJobPage`, C-11)이 있다. 영어 고정 문구는 i18n 키(ko·en)로 바꿨다. 정적 pagehead와 `#job-filter`·`#job-status-filter`·`#job-sort`·`#clear-job-history`는 `index.html`에 남겼다. `createDataTable`은 `searchable` 없이 `exportable`만 켜도 CSV 버튼을 낸다. 옛 `ops.ts` `renderJobs`는 `pcvLegacyViewSkipped('jobs')`로 막았다. 본문과 문자열 helper는 활동 화면과 guest-exec 계약이 붙잡아 U8까지 남긴다. domsafe 33(변화 없음), ratchet 변화 없음, axe 기준선 2,314→2,346(+32, `--prune` 없음). 검증: `npm run test:required` exit 0(node 384 pass, browser gate assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean.

## Task 5b: U5 이벤트

- [x] 이벤트 화면을 등록표로 옮긴다: 심각도별 개수 카드와 표. 작업 목록을 다시 그리는 활동 패널(`ops.ts:328-361`)을 지우고 작업 화면 링크로 바꾼다(C-10). `ops.ts`의 남은 영어 고정 문구를 i18n 키로(C-03). 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 13(20:03~20:26). grok이 새 모듈 `web/src/modules/view-events.ts`(167줄, `register('activity')`)를 만들었다. mount `#event-center-panel[data-view-mount]`에 다음을 그린다: 심각도 개수 카드(`event-severity-lane` 유지), 심각도 chip(`state.eventSeverityFilter`), `createDataTable`(시각, 심각도, 출처, 메시지, 정렬, CSV), 읽는 중·요약 읽기 실패·빈 목록의 서로 다른 문구. 제목 `copy.activity.title`은 이벤트/Events다. `#activity-panel`은 작업 목록을 다시 그리지 않고 "작업 기록은 작업 화면에서 봅니다"와 `#/jobs` 링크만 보인다(C-10). 활동 패널에서 빠지는 `Pagination`, `max_terminal_jobs`, `request_id`는 작업 화면 경계 줄로 옮겼다. `ops.ts`의 사건 카드, 이벤트 빈 문구, 토큰 안내, 증적 상태 영어 문구를 i18n으로 옮겼다(C-03). `renderJobs`와 대시보드 활동 영어는 계약·U6 범위라 남겼다. 이벤트 화면에는 원래 다음 페이지가 없었다(최대 12행). 옛 `renderEventCenter`는 `pcvLegacyViewSkipped('activity')`로 막았다. domsafe 천장 33→31. Claude 리뷰에서 결함을 하나 찾았다. `createDataTable` id가 `dt-`+난수라 axe 선택자가 실행마다 바뀌어 gate가 기준선과 맞지 않았다(활동 화면 incomplete 6건이 매번 새로 나옴). `cfg.tableId`를 받게 하고 세 화면에 고정 id(`dt-events`, `dt-jobs`, `dt-network`)를 주었다. 난수 id로 남은 기준선 항목은 stale 경고로만 남고, U8에서 정리한다. axe 기준선 2,346→2,414(`--prune` 없음). 검증: `npm run test:required` exit 0(node 385 pass, browser gate assertions 121 failed 0 violations 0, 두 번 연속 `ok=true`), web Pester와 ratchet Pester 53/53, Delivery 785/785(grok 실행), `git diff --check` clean. PR은 Task 5c로 나눴다.

## Task 5c: U5 PR

- [x] clean HEAD 종료 검증(`npm run test:required`, `Invoke-PcvPrGate.ps1`), release-train queue 행. push, PR(U5 묶음, Task 5a~5b), green CI 뒤 merge, merge 뒤 `main` run 확인.

실행 기록(2026-11-09): checkpoint 14(20:27~). clean HEAD `43bba65a`에서 `npm run test:required` node 385 pass. browser gate 첫 실행은 알려진 `vm-detail-summary` bgGradient flake였고, 한 번 다시 돌려 `ok=true`(assertions 121 failed 0 violations 0)였다. `Invoke-PcvPrGate.ps1` ok, Delivery 785/785, `git diff --check origin/main...HEAD` clean. push 뒤 PR #127, release-train queue 행(merge_commit `43bba65a`).

## Task 6a: U6 대시보드 머리와 KPI

- [x] `web/index.html:228-286`의 hero, 고정 메모, 빠른 작업 6칸, "운영 콕핏" 머리를 지운다. 대시보드를 등록표로 옮겨 pagehead(주 버튼 `+ 새 VM`)와 KPI 카드(VM 실행 중·꺼짐·템플릿, 진행 중 job, 최근 24시간 실패 job, Hyper-V 준비 상태)를 그린다. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 15(20:37~20:58)에서 grok이 새 모듈 `web/src/modules/view-dashboard.ts`(168줄, `register('dashboard')`)를 만들었다. `#dashboard-kpi[data-view-mount]`에 KPI 카드 6개를 그린다. VM 실행 중·꺼짐·템플릿은 `state.vms`·`vmPowerKind`·`isTemplateLockedVm`으로 세며, 템플릿은 실행/꺼짐에서 빠진다. 진행 중 job은 queued와 running을 센다. 최근 24시간 실패 job은 `created_at` 기준이다. Hyper-V 준비 상태는 준비, 미준비, 알 수 없음과 짧은 이유로 보인다. VM 목록을 아직 읽지 못했으면 "-"와 읽는 중으로 보이고 꾸민 값은 없다. 정적 pagehead에는 `#open-create-vm`(`+ 새 VM`)과 보조 링크를 두었다. hero, 운영 메모, pills, 빠른 작업 6칸, 두 번째 "운영 콕핏" 머리, `renderHeroChips`를 지웠다. 붙잡힌 패널(`ops-summary-panel`, `priority-panel`, `dashboard-activity-panel`, `metric-grid`, `host-details`, `monitoring`, `monitoring-panel`)은 남겼다. 새 시험이 실패한 채 한도에 걸렸다. 권장안 자동 승인으로 checkpoint 16(20:58~21:02)과 17(21:02~21:07)에서 Claude가 이어서 고쳤다. 시험의 가짜 DOM이 `<a>`의 class 선택자를 맞추지 못해 카드 6개 중 1개만 셌으므로, mount 자식으로 셌다. 렌더 결과는 원래 맞았다. 빠른 작업 칸에만 있던 `data-view-link`(network, activity, troubleshooting)는 pagehead 보조 링크(작업, 네트워크, 이벤트, 진단)로 옮겼다. `dashboard` 링크 검사는 계약(`ops-cockpit:dashboard-link`)과 Pester 모두에서 실제 위치인 셸 nav 정의(`shell.ts` `id: 'dashboard'`)를 보게 했다. 대시보드 mount가 `index.html`에서 먼저 나와서, network panel 음성 시험의 결함 주입은 네트워크 섹션 mount(`<div data-view-mount`)를 가리키게 했다. domsafe 31(변화 없음), axe 기준선 2,414→2,502(+88, `--prune` 없음). 검증: `npm run test:required`(node 386 pass, 기준선 기록 뒤 gate 두 번 `ok=true`, assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean.

## Task 6b: U6 워크로드 표와 경고 피드

- [x] 워크로드 표(상위 VM, 행을 누르면 VM 상세)와 경고 피드(실패 job과 ops 요약 incident)를 더한다. 사용률 게이지와 기간 선택은 넣지 않는다. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 18(21:07~21:26). grok이 `#dashboard-kpi` 아래 `.dashboard-grid`에 정적 컨테이너 `#dashboard-workload`와 `#dashboard-alerts`를 두고(`data-view-mount`는 더하지 않음), 같은 `renderDashboard`가 그리게 했다. 워크로드는 `createDataTable`(`tableId` `dt-dashboard-workload`)로, 실행 중 먼저·이름 순 상위 8개다. 이름 칸은 `#/vms/<id>/summary` 링크다. 목록을 못 읽었으면 읽는 중, VM이 없으면 빈 상태와 `+ 새 VM`이다. 경고 피드는 최대 8개로, 최근 24시간 실패 job과 `state.opsSummary`의 errors·signals incident(심각도 pill, 제목, 권장 조치 한 줄)를 최신순으로 보인다. "경고 없음"과 "운영 요약을 읽지 못했습니다"는 다른 문구이고, 요약을 읽지 못하면 incident만 숨긴다. 넓은 화면은 2열, 900px 이하는 1열이고 기존 token만 쓴다. 사용률 게이지와 기간 선택은 넣지 않았다. domsafe 31(변화 없음), axe 기준선 2,502→2,554(+52, `--prune` 없음). 검증: `npm run test:required` exit 0(node 387 pass, browser gate assertions 121 failed 0 violations 0), web Pester 50/50, ratchet Pester 3/3, Delivery 785/785, `git diff --check` clean. PR은 Task 6c로 나눴다.

## Task 6c: U6 PR

- [x] clean HEAD 종료 검증(`npm run test:required`, `Invoke-PcvPrGate.ps1`), release-train queue 행. push, PR(U6 묶음, Task 6a~6b), green CI 뒤 merge, merge 뒤 `main` run 확인.

실행 기록(2026-11-09): checkpoint 19(21:27~). clean HEAD `bdc5874e`에서 `npm run test:required` node 387 pass. browser gate 첫 실행은 알려진 `vm-detail-summary` bgGradient flake였고, 한 번 다시 돌려 `ok=true`였다. `Invoke-PcvPrGate.ps1` ok, `git diff --check origin/main...HEAD` clean. push 뒤 PR #128, release-train queue 행(merge_commit `bdc5874e`).

## Task 7a: U7 진단

- [x] 진단을 등록표로 옮겨 두 열 카드(Hyper-V 준비 상태와 권장 조치, service·listener, 현재 세션, 진단 번들)로 그린다. registry bridge, Host Ops bucket 표, 경계 칩은 접힌 "고급" 절로(C-16). `monitor.ts`의 진단 쪽 영어 고정 문구를 i18n 키로. 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 20(21:36~22:03). grok이 새 모듈 `web/src/modules/view-troubleshooting.ts`(223줄, `register('troubleshooting')`)를 만들었다. mount는 `#troubleshooting-cards.trouble-grid[data-view-mount]` 하나로, 네트워크 mount 뒤에 둔다. 두 열 카드 넷(Hyper-V 준비 상태와 권장 조치, service·listener, 현재 세션의 사용자·역할·로그아웃(로그인 폼 없음), 진단 번들)을 `PCV.uxlib.el`로 그린다. registry bridge, Host Ops lifecycle bucket 표, 경계 칩은 닫힌 `<details id="troubleshooting-advanced">`로 옮겼다(C-16). 붙잡힌 패널(`incident-panel`, `relogin-overlay`, `account-console-panel`, `token-rotation-panel`, `diagnostics-panel`, `beta-followup-panel`, `troubleshooting-panel`)은 접힌 절 밖에 남겼다. 옛 `renderAccountSession`, `renderDiagnosticsBundle`(`monitor.ts`), `renderTroubleshooting`(`vm-console.ts`)은 등록된 동안 카드와 고급 절을 다시 그리지 않는다. 번들 버튼·사실, 배치 증적, 오류 코드 목록은 fixture가 그 패널 문자열로 붙잡고 있어 원래 패널에 남겼다. 고급 절과 카드 문구는 i18n 키(ko·en)로 바꿨다. 진단 패널에 남은 영어 리터럴(`Download latest`, `Retry action`, Mode/Redaction/Latest bundle 등)은 계약·fixture가 붙잡아 U8 정리로 넘긴다. grok은 21:58에 build 전에 멈췄고, Claude가 build·검증·기준선 기록을 했다. domsafe 31(변화 없음), axe 기준선 2,554→2,586(+32, 진단 화면, `--prune` 없음). 검증: `npm run test:required`(node 388 pass, 기준선 기록 뒤 gate 두 번 `ok=true`, assertions 121 failed 0 violations 0), web Pester와 ratchet Pester 53/53, Delivery 785/785, `git diff --check` clean.

## Task 7b: U7 증적과 계정

- [x] 증적: 운영자 값 카드, release 엔지니어링 값은 접힌 절로(C-18). 계정: `createDataTable`과 역할 pill, 생성 모달 유지(C-13). 공통 검증. 로컬 commit.

실행 기록(2026-11-09): checkpoint 21(22:03~22:23). grok이 두 모듈을 만들었다. `view-evidence.ts`(134줄, `register('evidence')`)는 `#evidence-cards`에 운영 카드 3장(설치 version은 batch evidence `latest.release.version`, operational current는 full admin `version` 또는 `batch_id`, 마지막 검증 시각은 `generated_at` 등. 모르면 "-")을 그린다. GPU snapshot, Public boundary head, signing·service·route/MSI 등 release 엔지니어링 값은 닫힌 `<details id="evidence-release">`로 옮겼다(C-18). `view-accounts.ts`(110줄, `register('accounts')`)는 역할 chip(전체와 역할별 개수)과 `createDataTable`(`tableId` `dt-accounts`: 사용자 이름, 역할 pill, 사용 여부, 기존 `data-action` 행 조작)을 그린다. 생성·재설정 모달(ADR-0019)과 마지막 admin 오류 상자는 그대로다(C-13). 계정 객체에 마지막 로그인 필드가 없어 그 열은 만들지 않았다. Claude 리뷰: 계약이 붙잡은 옛 `#evidence-panel`이 GPU·boundary 값을 펼친 채 보여 C-18이 덜 이뤄졌으므로, 그 패널을 접힌 절 안으로 옮겼다(id와 문자열은 그대로). 기준선 기록 때 그 실행의 알려진 `vm-detail-summary` bgGradient incomplete 6건도 함께 기록됐다(정책은 다시 돌리기). 그 flake가 gate를 깨지 않게 될 뿐이며 U8 기준선 정리에서 다시 본다. domsafe 31(변화 없음), axe 기준선 2,586→2,626(`--prune` 없음). 검증: `npm run test:required`(node 389 pass, 기준선 기록 뒤 gate 두 번 `ok=true`, assertions 121 failed 0 violations 0), web Pester와 ratchet Pester 53/53, Delivery 785/785, `git diff --check` clean. PR은 Task 7c로 나눴다.

## Task 7c: U7 PR

- [x] clean HEAD 종료 검증(`npm run test:required`, `Invoke-PcvPrGate.ps1`), release-train queue 행. push, PR(U7 묶음, Task 7a~7b), green CI 뒤 merge, merge 뒤 `main` run 확인.

실행 기록(2026-11-09): checkpoint 22(22:24~). clean HEAD `388bf658`에서 `npm run test:required` exit 0(node 389 pass, browser gate assertions 121 failed 0 violations 0), `Invoke-PcvPrGate.ps1` ok, `git diff --check origin/main...HEAD` clean. push 뒤 PR #129, release-train queue 행(merge_commit `388bf658`).

## Task 8a: U8 옛 렌더러 퇴역

- [ ] 모든 화면이 등록표로 옮겨졌는지 확인하고 옛 `render()`(`monitor.ts:904-938`), `renderActiveView()`, `index.html` 정적 섹션, 옛 `#command-palette`, 쓰이지 않는 스와이프 코드(`nav.ts:659-693`)를 지운다. 공통 검증. 로컬 commit.

## Task 8b: U8 품질 게이트와 닫기, PR

- [ ] 남은 하드코딩 색을 지우고 `web/DESIGN.md` 기본 테마 설명(`:79-81`, `:84-100`)을 BL-0026대로 하나로 고친다. axe 위반 0 기준선(필요하면 `--prune`), browser fixture, 화면 캡처 기준선을 맞춘다. 감사 61건의 처리 결과(고침, 넣지 않음, API 설계로 넘김)를 plan 실행 기록에 표로 남긴다. campaign을 닫고 `next_approval_required`에 release train 출발 문장을 둔다. 공통 검증과 clean HEAD 종료 검증(`dotnet test src/DesktopNode.sln -c Release`, PR gate). release-train queue 행. push, PR(U8 묶음), green CI 뒤 merge, `main` push run green 확인.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 설치본 화면은 release train(따로 승인) 뒤에 바뀐다.
- 화면 동작은 node 시험, 브라우저 gate, 가짜 API 캡처로만 확인한다. 설치본 동작은 train 뒤 시연 기록으로만 주장한다.
- 사용률 metrics, ISO 목록, 계정 역할·삭제·재활성화를 구현한다고 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
