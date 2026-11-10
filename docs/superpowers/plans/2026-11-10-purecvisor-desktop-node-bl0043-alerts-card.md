# BL-0043 대시보드 경고 카드 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Web Console 대시보드 "경고" 카드가 `ops.summary`를 받은 뒤에는 "운영 요약을 읽지 못했습니다"를 보이지 않게 한다(backlog BL-0043). 운영 위생 C7을 다시 `open_counts=0, undecided=0`으로 만든다. 제품 변경은 release-train queue 행으로 쌓고, 설치본 반영은 따로 승인받는 다음 train에서 한다.

**Architecture:** 근거는 backlog 행 BL-0043(train `0.43.2` Task 6 probe, `docs/ga-ready/evidence/lane2-bl0042-probe-2026-11-10-04302.md`)이다. 카드는 `view-dashboard.ts` `alertsUnavailable()`이 `state.summaryError`가 있거나 `hasRefreshedOperation('ops.summary')`가 `false`면 "unavailable"을 그린다. probe에서는 `ops/summary` 요청 세 개가 화면 갱신 중 취소(`ERR_ABORTED`)된 뒤 네 번째가 `200`으로 왔고, `hasRefreshedOperation('ops.summary')`는 `true`였다. 유력한 원인은 `core.ts` `loadOpsSummary()`다. 이 함수는 오류를 스스로 잡기 때문에, 새 `refreshAll()`이 이전 요청을 취소하면 이전 호출의 catch가 `state.summaryError`에 `PCV_REQUEST_ABORTED`를 쓰고 `state.opsSummary`를 비운다. `refreshAll()`은 단계 오류의 `PCV_REQUEST_ABORTED`만 거르고, 이 경로는 거르지 않는다. Task 1이 재현 시험으로 원인을 먼저 확인한다. branch는 `lane1/bl0043-alerts-card-20261110` 하나이고 PR도 하나다. grok(Herdr `grok-status`)이 구현하고 Claude가 task마다 리뷰한다.

**Tech Stack:** TypeScript Web Console(`npm run build:served --prefix web`), node 시험(`web/node-tests`), browser-ui-gates, web Pester, `gh`

## 사용자 결정 (2026-11-10)

승인 원문: `"고칠 대상으로 지정"` (`train-04302-20261110` 마무리 뒤 보고의 다음 승인 1 "BL-0043 분류: … 고칠 대상(`counts`)인지 범위 밖(`out-of-scope`)인지 정해 주세요"에 대한 답). 답은 BL-0043을 `counts`로 분류한다는 뜻이다.

| 항목 | 범위 |
| --- | --- |
| 1 | BL-0043을 `counts`로 분류한다(이 opening commit의 `backlog.json`) |
| 2 | 고치는 일은 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 연다. BL-0042 때(`"1, 2 수정"`)와 같은 Lane 0/1 수정 campaign이다. 구현은 grok, 리뷰는 Claude. task마다 로컬 commit |
| 3 | push, PR 하나, Required CI 다섯 개 green 뒤 merge(`merge_policy=after-green-ci`, 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`). merge 뒤 main push run 확인 |
| 4 | 제품 변경은 `release-train.json` queue 행 하나로 쌓는다. release train 출발은 하지 않는다(따로 승인) |
| 5 | host mutation 없음. 설치본, service, VM을 바꾸지 않는다. `current-evidence.json`을 쓰지 않는다 |
| 설계 | 권장안 자동 승인으로 정한다. 취소된(`PCV_REQUEST_ABORTED`) 요청은 화면 상태를 쓰지 않는다. 새 갱신이 이전 갱신을 대신하므로 이전 호출은 오류도 데이터도 남기지 않는다. 같은 모양으로 오류를 스스로 잡는 `core.ts` 읽기 함수가 있으면 같은 원인이므로 함께 고친다. Task 1 재현에서 원인이 다르면 grok이 보고하고 Claude가 BL-0043 범위 안에서 다시 정한다 |

## Global Constraints

- 고치는 범위는 BL-0043과 같은 원인뿐이다. 그 밖의 발견은 backlog `undecided` 행으로 쓴다.
- 새 route, 새 권한, 새 `Add-Type`/P/Invoke/native ACL을 만들지 않는다. 서버 코드는 바꾸지 않는다.
- Web 규칙(BL-0042와 같다):
  - `web/src`를 고친 뒤 `npm run build:served --prefix web`를 돌린다. `web/app.bundle.js`와 `web/sw.js` CACHE_NAME은 생성물이다.
  - 문구는 바꾸지 않는다. 바꿔야 하면 i18n 키(ko 기본, en 같은 키)로 쓴다. 색은 기존 token만 쓴다. 계약·Pester·browser fixture가 붙잡은 markup과 문자열은 같은 강도로 옮긴다.
  - axe violation 0. `--prune`은 쓰지 않는다.
- grok 지시는 task마다 약 20분 분량으로 쓰고, 마감 시각을 적는다. grok은 commit·push하지 않고 `docs/**`를 고치지 않는다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 재현 시험과 수정 (Web)

- [x] 먼저 원인을 재현한다. node 시험으로 (가) 이전 갱신의 `loadOpsSummary`가 취소되고 새 갱신의 `loadOpsSummary`가 성공하는 순서(취소 거부가 성공보다 늦게 오는 순서 포함), (나) 그 뒤 대시보드 "경고" 카드 상태를 본다. 고치기 전에는 이 시험이 `unavailable`로 실패해야 한다. browser gate mock 화면에서도 같은 문구가 보이는 이유를 확인한다(mock 응답, 갱신 순서, 다시 그리기). 그다음 고친다. 취소된 요청은 `state.summaryError`와 `state.opsSummary`를 바꾸지 않는다. 같은 모양의 `core.ts` 읽기 함수도 같은 규칙을 따른다.
  - 시험: 위 재현 시험(고친 뒤 `empty` 또는 목록), 진짜 실패(예 `500`)는 계속 `unavailable`, 기존 `data-alerts-state` 시험 유지.
  - 검증: `npm run build:served --prefix web`, `npm run test:required --prefix web`, web Pester, `git diff --check`. gate mock 대시보드 1440과 768 캡처에서 "경고" 카드가 "운영 요약을 읽지 못했습니다"를 보이지 않는다. 로컬 commit.

실행 기록(2026-11-10): grok이 23:38~23:55에 구현했고 Claude가 리뷰했다.
- 원인은 가설과 같다. `refreshAll()`이 새 갱신을 시작하며 이전 controller를 abort하면, 이전 `loadOpsSummary()`가 스스로 잡은 `PCV_REQUEST_ABORTED`로 `state.summaryError`를 쓰고 `state.opsSummary`를 비웠다. 이 거부가 새 갱신의 성공보다 늦게 오면 카드가 `unavailable`로 남는다. `hasRefreshedOperation()`은 `summaryError`를 보지 않으므로 probe의 `true`와도 맞는다.
- gate mock도 같은 원인이다. 시작 때 `init()`의 `refreshAll()`과 `restoreSession()` → `_finishLogin()` → `loadAll()`의 두 번째 갱신이 겹쳐 첫 갱신이 취소됐다. mock은 바꾸지 않았다.
- 고치기 전 재현 시험 `BL-0043 a late aborted loadOpsSummary leaves the alerts card available`가 `'unavailable' !== 'empty'`로 실패했다.
- 수정(`web/src/modules/core.ts`):
  - 새 함수 `isSupersededRequest`는 넘겨받은 signal이 aborted이거나 오류 코드가 `PCV_REQUEST_ABORTED`이면 참이다.
  - `loadOpsSummary`와 같은 모양으로 오류를 스스로 잡는 `loadNetworkInventory`, `loadAccountSession`, `loadAccountDirectory`, `loadConsoleCapabilities`, `loadServerJobs`, `loadDiagnosticBundleList`가 이 규칙을 따른다. 취소면 오류도 데이터도 쓰지 않고 돌아간다. 오류는 시작할 때가 아니라 성공한 뒤에 지운다.
  - 진짜 실패(HTTP `500`)는 지금처럼 오류를 쓴다. `loadHost`·`loadVms`·`loadRuntimePolicy`는 throw하므로 `refreshAll()`의 기존 필터가 처리한다. 바꾸지 않았다.
- node 시험 3건 추가: 늦은 abort 뒤 카드 `empty`, 이미 abort된 signal은 API를 부르지 않음, HTTP `500`이면 카드 `unavailable`.
- browser gate 기준선(`web/config/browser-ui-gates-baseline.json`)에 axe incomplete 8개를 더했다(`--write-baseline`, `--prune` 없음, 1597→1605). 수정 뒤 대시보드가 `경고 없음`을 그리고 mock 요약 데이터가 활동·문제 해결 화면에 들어오면서 새로 생긴 항목이다. violation은 0이다.
- 결과: `npm run test:required --prefix web` exit 0(node 399/399, browser gate `ok=true` assertions 121 failed 0 violations 0), web Pester 50/50, `git diff --check` clean. `web/sw.js` CACHE_NAME은 `pcv-ui-v7e9cc024`다.
- 화면: gate mock 대시보드 1440×900과 768×1024(`artifacts/bl0043-alerts/`, gitignore)에서 "경고" 카드 본문이 `경고 없음`(`data-alerts-state="empty"`)이다. "운영 요약을 읽지 못했습니다"는 없다.
- grok이 적은 범위 밖 관찰 두 건은 결함이 아니라 backlog 행으로 쓰지 않았다. 폴링 `runPollTick`의 signal 없는 `loadOpsSummary()`는 진짜 실패를 쓰는 경로다. `loadNextDiagnosticBundlePage`는 사용자가 누르는 다음 페이지 경로라 갱신 취소와 무관하다.

## Task 2: 종료 검증과 PR

- [x] backlog BL-0043을 닫고(`status=closed`, `closed_by`), `release-train.json` queue 행을 더하고, campaign을 닫는다.
  - 종료 검증(clean HEAD): `dotnet build src/DesktopNode.sln -c Release`, Required CI 네 shard 로컬 실행, Pester 네 종, `Invoke-PcvPrGate.ps1` `ok=true`, `git diff --check origin/main...HEAD`.
  - push, PR, Required CI 다섯 개 green 뒤 `--match-head-commit` merge. merge 뒤 main push run을 기다린다. red면 멈추고 보고한다.

실행 기록(2026-11-11):
- 종료 검증(clean HEAD `3d1ce59`): Release build 성공. Required CI 네 shard(`dotnet`, `web`, `delivery`, `installer-policy`) 로컬 실행 `ok=true`. dotnet shard 시험 3,078/3,078(Service 11, Contracts 201, Cli 186, Runtime 129, HyperV 288, Delivery 785, Host 246, Api 545, Verification 687). Pester 실패 0: packaging 528, installer 49, web 50, manual-admin 140.
- backlog BL-0043을 닫았다(`closed_by` PR #142). 운영 위생 C7은 `open_counts=0, undecided=0`이 된다.
- `release-train.json` queue에 PR #142 행 하나(`area=web`, `risk_tier=M`, `lane2_probe=null`)를 더했다. 설치본 반영은 다음 release train(따로 승인)에서 한다.
- campaign을 닫고 다음 승인으로 release train `0.43.3-admin-smoke` 출발 문장을 남겼다. PR #142의 CI·merge·main run 결과는 최종 보고에 적는다.

## Nonclaims

- operational current는 `0.43.2-admin-smoke` 그대로다. 설치본 카드는 다음 train 뒤에 바뀐다.
- public trusted signing과 external stable publication을 주장하지 않는다.
