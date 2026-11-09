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

- [ ] 설계 4.1절의 두 상태. 고르지 않으면 전체 폭 표, 고르면 왼쪽 260px 목록과 넓은 상세(`web/index.html:291-347`, `web/style.css:5212-5225`). 선택 카드를 지우고 이름은 상세 헤더 h2 하나다. `HN.pagehead`(제목, 설명, `+ 새 VM`). 콘솔 탭 맞춤 배율이 넓어진 폭을 쓰는지 확인한다. 설계 7절의 VM 골격 계약·시험 8곳을 고친다. 공통 검증, 가짜 API 캡처(1440, 768, 390)를 원본 캡처와 비교. 로컬 commit.

## Task 2c: U2 바로 가는 링크, 빈 상태, 좁은 화면, PR

- [ ] hash `#/vms/<vm-id>/<tab>`(`core.ts:351-354`)와 뒤로 가기, 없는 VM 처리. 빈 상태 CTA와 읽기 실패(`components.emptyState`·`loadError`). 600px 이하는 목록과 상세를 하나씩. node 시험. 공통 검증, release-train queue 행. push, PR(U2 묶음), green CI 뒤 merge.

## Task 3a: U3 행 메뉴

- [ ] 행 `⋯` 메뉴와 우클릭(ContextMenu 키, Shift+F10 포함)이 ADR-0019 더 보기 메뉴와 같은 항목을 연다. 상태에 맞지 않는 항목은 끈다. 행 주 조작 하나는 그대로다. node 시험: 메뉴 키보드(열기, 화살표, Esc). 공통 검증. 로컬 commit.

## Task 3b: U3 정렬과 j/k

- [ ] 열 머리를 누르면 그 열로 정렬하고 다시 누르면 방향을 바꾼다. 목록에 포커스가 있으면 j/k로 행을 옮긴다. node 시험. 공통 검증. 로컬 commit.

## Task 3c: U3 선택과 일괄 시작·종료

- [ ] 표 첫 열 체크박스와 선택 막대(선택 n개, 시작, 종료, 선택 해제). VM마다 job 하나와 자리 진행 줄. node 시험: job 수. 공통 검증. 로컬 commit.

## Task 3d: U3 일괄 삭제, PR

- [ ] 일괄 삭제 확인 모달(대상 이름 목록, 고른 개수를 입력해야 버튼이 켜짐)과 선택 막대 삭제 버튼. 모바일 일괄 삭제를 이 모달로 되살린다. node 시험: 개수 gate, VM마다 job 하나. 공통 검증, release-train queue 행. push, PR(U3 묶음), green CI 뒤 merge.

## Task 4a: U4 화면 렌더 등록표

- [ ] 설계 4.2절. `nav.ts`에 `PCV.views.register(name, { render(mount, ctx) })`를 두고 옛 `renderActiveView()`가 등록된 화면을 건너뛰게 한다. 주기 새로 고침은 활성 화면의 `render`만 부른다. `HN.*` 반환 형식을 확인해 domsafe ceiling을 올리지 않는 경로를 정하고 실행 기록에 적는다. 화면은 아직 옮기지 않는다. node 시험: 등록된 화면 건너뜀. 공통 검증. 로컬 commit.

## Task 4b: U4 네트워크, PR

- [ ] 네트워크를 등록표로 옮긴다: `HN.pagehead`, `filterBar` 종류 chip(스위치 `type` 값별)과 개수, `createDataTable`(이름, 종류, 연결 VM 수, 어댑터, 상태 pill), 768px 이하 카드, 빈 상태와 읽기 실패(문구 구분 node 시험), 하드코딩 패널 색을 token으로. 공통 검증, release-train queue 행. push, PR(U4 묶음), green CI 뒤 merge.

## Task 5a: U5 작업

- [ ] 작업 화면을 등록표로 옮긴다: 상태 chip과 종류 select, `createDataTable`(접수 시각, 종류, 대상, 상태 pill, 걸린 시간, 오류 요약), 정렬·검색·CSV, 다음 페이지 버튼(C-11), 영어 고정 문구를 i18n 키로. 공통 검증. 로컬 commit.

## Task 5b: U5 이벤트, PR

- [ ] 이벤트 화면을 등록표로 옮긴다: 심각도별 개수 카드와 표. 작업 목록을 다시 그리는 활동 패널(`ops.ts:328-361`)을 지우고 작업 화면 링크로 바꾼다(C-10). `ops.ts`의 남은 영어 고정 문구를 i18n 키로(C-03). 공통 검증, release-train queue 행. push, PR(U5 묶음), green CI 뒤 merge.

## Task 6a: U6 대시보드 머리와 KPI

- [ ] `web/index.html:228-286`의 hero, 고정 메모, 빠른 작업 6칸, "운영 콕핏" 머리를 지운다. 대시보드를 등록표로 옮겨 pagehead(주 버튼 `+ 새 VM`)와 KPI 카드(VM 실행 중·꺼짐·템플릿, 진행 중 job, 최근 24시간 실패 job, Hyper-V 준비 상태)를 그린다. 공통 검증. 로컬 commit.

## Task 6b: U6 워크로드 표와 경고 피드, PR

- [ ] 워크로드 표(상위 VM, 행을 누르면 VM 상세)와 경고 피드(실패 job과 ops 요약 incident)를 더한다. 사용률 게이지와 기간 선택은 넣지 않는다. 공통 검증, release-train queue 행. push, PR(U6 묶음), green CI 뒤 merge.

## Task 7a: U7 진단

- [ ] 진단을 등록표로 옮겨 두 열 카드(Hyper-V 준비 상태와 권장 조치, service·listener, 현재 세션, 진단 번들)로 그린다. registry bridge, Host Ops bucket 표, 경계 칩은 접힌 "고급" 절로(C-16). `monitor.ts`의 진단 쪽 영어 고정 문구를 i18n 키로. 공통 검증. 로컬 commit.

## Task 7b: U7 증적과 계정, PR

- [ ] 증적: 운영자 값 카드, release 엔지니어링 값은 접힌 절로(C-18). 계정: `createDataTable`과 역할 pill, 생성 모달 유지(C-13). 공통 검증, release-train queue 행. push, PR(U7 묶음), green CI 뒤 merge.

## Task 8a: U8 옛 렌더러 퇴역

- [ ] 모든 화면이 등록표로 옮겨졌는지 확인하고 옛 `render()`(`monitor.ts:904-938`), `renderActiveView()`, `index.html` 정적 섹션, 옛 `#command-palette`, 쓰이지 않는 스와이프 코드(`nav.ts:659-693`)를 지운다. 공통 검증. 로컬 commit.

## Task 8b: U8 품질 게이트와 닫기, PR

- [ ] 남은 하드코딩 색을 지우고 `web/DESIGN.md` 기본 테마 설명(`:79-81`, `:84-100`)을 BL-0026대로 하나로 고친다. axe 위반 0 기준선(필요하면 `--prune`), browser fixture, 화면 캡처 기준선을 맞춘다. 감사 61건의 처리 결과(고침, 넣지 않음, API 설계로 넘김)를 plan 실행 기록에 표로 남긴다. campaign을 닫고 `next_approval_required`에 release train 출발 문장을 둔다. 공통 검증과 clean HEAD 종료 검증(`dotnet test src/DesktopNode.sln -c Release`, PR gate). release-train queue 행. push, PR(U8 묶음), green CI 뒤 merge, `main` push run green 확인.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 설치본 화면은 release train(따로 승인) 뒤에 바뀐다.
- 화면 동작은 node 시험, 브라우저 gate, 가짜 API 캡처로만 확인한다. 설치본 동작은 train 뒤 시연 기록으로만 주장한다.
- 사용률 metrics, ISO 목록, 계정 역할·삭제·재활성화를 구현한다고 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
