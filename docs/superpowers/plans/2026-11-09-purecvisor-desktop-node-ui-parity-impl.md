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

## Task 2: U2 VM 목록·상세 골격

- [ ] 설계 4.1절. 목록·골격을 새 모듈 `web/src/modules/vm-list.ts`(`modules.json`에서 `vm` 앞)로 나누고, 고르지 않으면 전체 폭 표, 고르면 왼쪽 260px 목록과 넓은 상세로 바꾼다(`web/index.html:291-347`, `web/style.css:5212-5225`). 선택 카드를 지우고 이름은 상세 헤더 h2 하나다. `HN.pagehead`(제목, 설명, `+ 새 VM`). hash `#/vms/<vm-id>/<tab>`(`core.ts:351-354`)와 뒤로 가기, 없는 VM 처리. 빈 상태 CTA와 읽기 실패(`components.emptyState`·`loadError`). 콘솔 탭 맞춤 배율이 넓어진 폭을 쓰는지 확인. 600px 이하는 목록과 상세를 하나씩. 설계 7절의 VM 골격 계약·시험 8곳을 같은 PR에서 고친다. 공통 검증, 가짜 API 캡처(1440, 768, 390)를 원본 캡처와 비교, release-train queue 행. push, PR, green CI 뒤 merge.

## Task 3: U3 VM 행 메뉴, 정렬, 일괄 조작

- [ ] 설계 4.1절. 행 `⋯` 메뉴와 우클릭(ContextMenu 키, Shift+F10)을 ADR-0019 더 보기 메뉴와 같은 항목으로, 상태에 맞지 않는 항목은 끈다. 열 머리 정렬과 방향 반전, j/k 이동. 체크박스와 선택 막대(시작, 종료, 삭제, 선택 해제), 일괄 삭제 확인 모달(대상 이름 목록, 개수 입력 전 버튼 꺼짐), VM마다 job 하나와 자리 진행 줄. node 시험: 메뉴 키보드, 개수 입력 gate, job 수. 공통 검증, release-train queue 행. push, PR, green CI 뒤 merge.

## Task 4: U4 화면 렌더 등록표와 네트워크

- [ ] 설계 4.2절. `nav.ts`에 `PCV.views.register(name, { render(mount, ctx) })`를 두고 옛 `renderActiveView()`가 등록된 화면을 건너뛰게 한다. 첫 화면으로 네트워크를 옮긴다: `HN.pagehead`, `filterBar` 종류 chip(스위치 `type` 값별)과 개수, `createDataTable`(이름, 종류, 연결 VM 수, 어댑터, 상태 pill), 768px 이하 카드, 빈 상태와 읽기 실패, 하드코딩 패널 색을 token으로. 첫 단계에서 `HN.*` 반환 형식을 확인해 domsafe ceiling을 올리지 않는 경로를 정하고 실행 기록에 적는다. node 시험: 등록된 화면 건너뜀, 빈 상태와 읽기 실패 문구 구분. 공통 검증, release-train queue 행. push, PR, green CI 뒤 merge.

## Task 5: U5 작업과 이벤트

- [ ] 설계 4.2절. 작업: 상태 chip과 종류 select, `createDataTable`(접수 시각, 종류, 대상, 상태 pill, 걸린 시간, 오류 요약), 정렬·검색·CSV, 다음 페이지 버튼(C-11). 이벤트: 심각도별 개수 카드와 표, 작업 목록을 다시 그리는 활동 패널(`ops.ts:328-361`)을 지우고 작업 화면 링크로. `ops.ts`의 영어 고정 문구를 i18n 키로(C-03). 공통 검증, release-train queue 행. push, PR, green CI 뒤 merge.

## Task 6: U6 대시보드

- [ ] 설계 4.2절. `web/index.html:228-286`의 hero, 고정 메모, 빠른 작업 6칸, "운영 콕핏" 머리를 지우고 pagehead, KPI 카드(VM 실행 중·꺼짐·템플릿, 진행 중 job, 최근 24시간 실패 job, Hyper-V 준비 상태), 워크로드 표(행을 누르면 VM 상세), 경고 피드(실패 job과 ops 요약 incident)로 다시 그린다. 사용률 게이지와 기간 선택은 넣지 않는다. 공통 검증, release-train queue 행. push, PR, green CI 뒤 merge.

## Task 7: U7 진단, 증적, 계정

- [ ] 설계 4.2절. 진단: 두 열 카드(Hyper-V 준비 상태와 권장 조치, service·listener, 현재 세션, 진단 번들), registry bridge·Host Ops bucket 표·경계 칩은 접힌 "고급" 절로(C-16). 증적: 운영자 값 카드, release 엔지니어링 값은 접힌 절로(C-18). 계정: `createDataTable`과 역할 pill, 생성 모달 유지(C-13). `monitor.ts`의 영어 고정 문구를 i18n 키로. 공통 검증, release-train queue 행. push, PR, green CI 뒤 merge.

## Task 8: U8 옛 렌더러 퇴역, 품질 게이트, 닫기

- [ ] 모든 화면이 등록표로 옮겨졌는지 확인하고 옛 `render()`(`monitor.ts:904-938`), `renderActiveView()`, `index.html` 정적 섹션, 옛 `#command-palette`, 쓰이지 않는 스와이프 코드(`nav.ts:659-693`), 남은 하드코딩 색을 지운다. `web/DESIGN.md` 기본 테마 설명(`:79-81`, `:84-100`)을 BL-0026대로 하나로 고친다. axe 위반 0 기준선, browser fixture, 화면 캡처 기준선(`config/browser-ui-gates-baseline.json`)을 맞춘다. 감사 61건의 처리 결과(고침, 넣지 않음, API 설계로 넘김)를 plan 실행 기록에 표로 남긴다. campaign을 닫고 `next_approval_required`에 release train 출발 문장을 둔다. 공통 검증과 clean HEAD 종료 검증. push, PR, green CI 뒤 merge, `main` push run green 확인.

## Nonclaims

- 이 campaign은 operational current를 바꾸지 않는다. 설치본 화면은 release train(따로 승인) 뒤에 바뀐다.
- 화면 동작은 node 시험, 브라우저 gate, 가짜 API 캡처로만 확인한다. 설치본 동작은 train 뒤 시연 기록으로만 주장한다.
- 사용률 metrics, ISO 목록, 계정 역할·삭제·재활성화를 구현한다고 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
