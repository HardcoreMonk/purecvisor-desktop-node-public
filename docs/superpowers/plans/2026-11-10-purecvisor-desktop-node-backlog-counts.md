# backlog counts 6건 수정 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-11-10에 `counts`로 분류한 backlog BL-0036~BL-0041을 Lane 1로 고쳐 닫는다. 운영 위생 C7을 다시 `open_counts=0, undecided=0`으로 만든다. 제품(Web Console) 변경은 release-train queue 행으로 쌓고, 설치본 반영은 따로 승인받는 다음 train에서 한다.

**Architecture:** 근거는 backlog 행 BL-0036~0041이다(`docs/ga-ready/backlog.json`). BL-0036~0039는 `ui-parity-20261109` U8, BL-0040·0041은 train `0.43.0` Task 6 probe에서 찾았다. ADR-0020 Web Console 구조(`web/src/modules/*.ts`, 렌더 등록표)를 바꾸지 않고 고친다. branch는 `lane1/backlog-counts-20261110` 하나이고 PR도 하나다. grok(Herdr `grok-status`)이 구현하고 Claude가 task마다 리뷰한다.

**Tech Stack:** TypeScript Web Console(`npm run build:served --prefix web`), node 시험(`web/node-tests`), browser gate(`web/scripts/verify-browser-ui-gates.mjs`, axe), web Pester, `gh`

## 사용자 결정 (2026-11-10)

승인 원문: `"1"` (`train-04300-20261110` 마무리 보고의 다음 승인 1번에 대한 답). 그 문장은 다음과 같다. "backlog counts 수정 campaign 열기: BL-0036(768 폭 검색 버튼 접근 가능한 이름), BL-0037(좁은 VM 목록 이름 잘림), BL-0038(삭제 확인 gate flake), BL-0039(고대비·mockup 테마 핵심 색, 살릴지 뺄지 설계 포함), BL-0040(첫 방문 runtime/policy 401), BL-0041(진단 서비스 값 표시)을 Lane 1로 고친다. grok 구현, Claude 리뷰. push, PR, green CI 뒤 merge. 제품 변경은 release-train queue 행으로 쌓는다. host mutation 없음. release train 출발은 따로 승인."

| 항목 | 범위 |
| --- | --- |
| 1 | Lane 0/1. BL-0036~0041 구현(grok)과 리뷰(Claude). task마다 로컬 commit |
| 2 | push, PR 하나, Required CI 다섯 개 green 뒤 merge(`merge_policy=after-green-ci`, 2026-11-09 상시 승인과 같다). merge 뒤 main push run 확인 |
| 3 | 제품 변경은 `release-train.json` queue 행 하나로 쌓는다. release train 출발은 하지 않는다(따로 승인) |
| 4 | host mutation 없음. 설치본, service, VM을 바꾸지 않는다. `current-evidence.json`을 쓰지 않는다 |
| BL-0039 설계 | 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 정한다. 고대비 테마(`supanova-hicontrast`)는 접근성 선택지라 살리고, 핵심 token이 실제로 바뀌게 고친다. mockup 테마(`supanova-mockup`)는 기본 `supanova-cyan`과 겹치는 시연용이라 선택 목록에서 빼고, 저장된 값은 `supanova-cyan`으로 옮긴다. Task 4 착수 때 브라우저에서 현상을 먼저 확인하고, 정적 분석과 다르면 멈추고 다시 정한다 |

## Global Constraints

- 고치는 범위는 BL-0036~0041뿐이다. 그 밖의 발견은 backlog `undecided` 행으로 쓴다.
- Web 규칙(직전 campaign `ui-parity-20261109`에서 굳은 것):
  - `web/src`를 고친 뒤 `npm run build:served --prefix web`를 돌린다. `web/app.bundle.js`와 `web/sw.js` CACHE_NAME은 생성물이다.
  - `web/src/bootstrap.ts`는 499줄을 넘기지 않는다(지금 478). `web/src/modules/vm.ts` ratchet 2,389를 넘기지 않는다. domsafe 천장 27을 올리지 않는다(줄면 `--update`).
  - 문구는 i18n 키(ko 기본, en 같은 키)로 쓴다. 색은 기존 token만 쓴다.
  - 계약·Pester·browser fixture가 붙잡은 markup과 문자열은 같은 강도로 옮긴다(`web/contracts/web-static-contracts.mjs`, `web/tests/PcvDesktopWeb.Static.Tests.ps1`, 음성 시험, `web/scripts/verify-browser-ui-gates.mjs`).
  - axe: violation 0. assertion이 모두 통과하고 incomplete만 늘면 `--write-baseline`. `--prune`은 쓰지 않는다. flake key 6개는 지우지 않는다.
  - 화면을 바꾸는 task는 1440과 768 캡처를 보고 리뷰한다(시험만 보지 않는다).
- grok 지시는 task마다 약 20분 분량으로 쓰고, 구현 마감 시각과 보고 시각을 적는다. grok은 commit·push하지 않고 `docs/**`를 고치지 않는다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다). 다음 task를 미리 준비하는 일도 지금 checkpoint 예산에 들어간다.

## Task 1: 작은 수정 세 건 (BL-0036, BL-0041, BL-0038)

- [x] BL-0036: 셸 검색 버튼(`web/src/modules/shell.ts` `.shell-search`, `role=button`)에 좁은 화면에서도 남는 접근 가능한 이름(`aria-label`, i18n)을 준다. node 시험으로 이름이 있는지 본다.
- BL-0041: 진단 "서비스와 수신기" 카드의 서비스 값이 boolean 그대로(`true`) 나오지 않게 i18n 상태 문구로 바꾼다. 값이 없으면 지금처럼 `-`다.
- BL-0038: browser gate `delete-confirm/accept-deletes`가 고정 `sleep(400)` 대신 DELETE 요청이 mock에 기록될 때까지(상한 있음) 기다리게 한다.
- 검증: `npm run build:served --prefix web`, `npm run test:required --prefix web`, web Pester, module-size ratchet Pester, `git diff --check`. 로컬 commit.

실행 기록(2026-11-10): grok이 08:10~08:23에 구현했고 Claude가 리뷰했다.
- BL-0036: grok은 `aria-label="검색Ctrl+K"`(공백 없음)로 axe를 맞췄다. 리뷰에서 원인 쪽을 고쳤다.
  - 900px 이하 CSS가 `.shell-search-ph`를 `display:none`으로 숨겨 이름이 사라졌다. 그 글자를 시각적으로만 숨기게(sr-only 속성) 바꾸고 `aria-label`은 뺐다.
  - 이름은 보이는 글자 "검색"과 같다. `kbd`는 `aria-hidden`이다. `aria-keyshortcuts="Control+K"`를 더했다.
  - 768×1024 gate 실행: violations 0, assertions 121 통과, `aria-command-name`·label mismatch 0건. 기준선이 1280용이라 incomplete 차이로 `ok=false`인 것은 직전과 같다.
- BL-0041: 진단 카드의 서비스·수신기 값이 boolean이면 `copy.trouble.service.running`/`stopped`(실행 중/멈춤, Running/Stopped)로 보인다. 문자열과 `-`는 그대로다.
- BL-0038: gate `delete-confirm/accept-deletes`가 고정 400ms 대신 DELETE 요청이 기록될 때까지 150ms 간격으로 최대 5초 기다린다. 단언과 이름은 같다.
- 검증: `npm run test:required --prefix web` exit 0(node 391 pass, domsafe 27, browser gate `ok=true` assertions 121 failed 0 violations 0), web Pester와 ratchet 53/53, `git diff --check` clean.

## Task 2: 첫 방문 runtime/policy 401 (BL-0040)

- [x] 새 브라우저(저장된 token 없음)로 loopback 주소를 열 때, 첫 보호 API 요청 전에 loopback 세션을 먼저 만들어 `401`이 나지 않게 한다. 위치는 `web/src/modules/api.ts`(`_createLoopbackSession`, `_recoverSession`)와 첫 refresh 흐름이다.
  - 계정이 있는 설치본(loopback 세션 `409`)에서는 지금처럼 로그인 상자로 간다.
  - 비 loopback 주소에서는 바뀌지 않는다.
  - node 시험: token 없는 첫 refresh에서 `auth/loopback-session`이 보호 API보다 먼저 불리는지, `409`이면 로그인 상자로 가는지 본다.
- 검증은 Task 1과 같다. 로컬 commit.

실행 기록(2026-11-10): Claude가 원인을 찾았고, grok이 08:27~08:32에 구현했으며 Claude가 리뷰했다.
- 원인: `web/index.html` `_pcvInitUI()`가 DOMContentLoaded에 `updateVersionBadge()`(`nav.ts`)를 부른다. 이 함수가 loopback 세션이 생기기 전 빈 Bearer로 `GET /api/v1/runtime/policy`를 보냈다. `refreshAll()`은 이미 세션을 먼저 만든다.
- 수정:
  - `updateVersionBadge()`는 token이 없으면 공유 `ensureLoopbackSession()`을 기다린 뒤 읽는다. 세션이 열리지 않으면(계정 모드 `409`, 비 loopback) 부르지 않고 badge를 숨긴다.
  - `_finishLogin()` 끝에서 badge를 다시 그린다(계정 로그인 뒤 version 표시).
  - 첫 방문에는 token을 가진 policy 읽기가 두 번 일어날 수 있다. 리뷰에서 받아들였다.
- 시험: `bundle-load.test.mjs`에 둘을 더했다. loopback 성공이면 `auth/loopback-session`이 `runtime/policy`보다 먼저이고 Bearer가 있다. `409`이면 policy 요청이 없다.
- 검증: `npm run test:required --prefix web` exit 0(node 393 pass, browser gate `ok=true` assertions 121 failed 0 violations 0), web Pester와 ratchet 53/53, `git diff --check` clean.

## Task 3: 좁은 VM 목록 이름 (BL-0037)

- [x] VM을 골라 목록이 260px로 줄었을 때 이름이 잘리지 않게 한다. 그 상태에서는 상태·표시 열을 숨기고, 이름 칸에 작은 상태 점과 템플릿 배지를 붙인다. 넓은 목록(선택 전)은 바꾸지 않는다.
  - 위치는 `web/src/modules/vm-list.ts`와 `web/style.css`의 목록·상세 배치 규칙이다.
  - node 시험과 1440·768 캡처로 확인한다.
- 검증은 Task 1과 같고, 캡처를 본다. 로컬 commit.

실행 기록(2026-11-10): grok이 08:34~08:48에 구현했고 Claude가 리뷰했다.
- 수정: 이름 칸을 `<span class="pcv-vm-list-name">` 이름과 `<span class="pcv-vm-compact-meta">`(상태 점 `aria-hidden`, `sr-only` 상태 글자, 템플릿 배지)로 나눴다.
  - 넓은 목록에서는 meta를 숨긴다.
  - `.vm-workspace.is-detail`에서는 meta를 보이고 3열(상태)·7열(표시)을 숨긴다. 이름은 남은 폭을 쓰고 말줄임한다.
  - 점 색은 `--green`/`--red`/`--yellow`/`--fg3`이다.
- 리뷰 수정: grok은 목록 span에 `pcv-vm-name`을 썼다. 그 class는 상세 제목 `h2.pcv-vm-name`, 그 CSS, QA 스크립트 2개(`capture-destructive-lifecycle-ui-qa.mjs`, `capture-installed-listener-qa.mjs`)가 쓴다. 그래서 목록 class를 `pcv-vm-list-name`으로 바꾸고 gate `detailReady` 선택자를 원래대로 되돌렸다.
- axe 기준선: grok의 기준선 쓰기를 되돌리고 다시 썼다. 1519→1597, 추가 78, 제거 0, `--prune` 없음. 추가는 모두 color-contrast `bgGradient`·`pseudoContent`(새 이름 span과 점)다.
- 캡처: `artifacts/backlog-counts-bl0037-captures/1440x900/vm-detail-summary.png`. VM을 고른 260px 목록에 이름 전체, 상태 점, 템플릿 배지가 보인다. 768 폭에서는 기존 900px 규칙으로 목록이 숨는다(바꾸지 않음).
- 검증: `npm run test:required --prefix web` exit 0(node 394 pass, domsafe 27, browser gate `ok=true` assertions 121 failed 0 violations 0), web Pester와 ratchet 53/53, `git diff --check` clean.

## Task 4: 고대비·mockup 테마 (BL-0039)

- [ ] 먼저 브라우저에서 고대비를 골랐을 때 `--bg`, `--fg`, `--accent`가 바뀌지 않는지 확인한다.
  - 확인되면 고대비 테마 선택자가 Part 2 `:root` token보다 우선하게 고친다(예: `:root[data-theme="supanova-hicontrast"]`). 고대비 화면에서 axe violation이 0인지 본다.
  - mockup은 `theme.ts` 목록, `index.html` 허용 목록, CSS 블록에서 빼고, 저장된 `supanova-mockup`은 `supanova-cyan`으로 옮긴다.
  - `web/DESIGN.md` 테마 표를 고친다(Claude).
- 검증은 Task 1과 같고, 1440 캡처(기본·고대비)를 본다. 로컬 commit.

## Task 5: 종료 검증과 PR

- [ ] clean HEAD에서 다음을 돌린다.
  - `dotnet test src/DesktopNode.sln -c Release`
  - `npm run test:required --prefix web`
  - `Invoke-PcvPrGate.ps1`
  - `git diff --check origin/main...HEAD`
- 그다음 push와 PR을 한다.
- docs commit 하나를 더한다.
  - `release-train.json` queue 행(제품 변경 요약, 변경 commit)
  - backlog BL-0036~0041 `status=closed`, `closed_by`
  - campaign 닫기
- Required CI 다섯 개 green 뒤 merge하고 main push run을 확인한다.
- clean `main`에서 `pcvverify completion`으로 C7이 `open_counts=0, undecided=0`인지 본다.

## Nonclaims

- operational current는 `0.43.0-admin-smoke` 그대로다. 설치본 반영은 다음 release train(따로 승인)이다.
- public trusted signing과 external stable publication을 주장하지 않는다.
