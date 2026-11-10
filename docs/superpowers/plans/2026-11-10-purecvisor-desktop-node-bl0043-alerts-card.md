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

- [ ] 먼저 원인을 재현한다. node 시험으로 (가) 이전 갱신의 `loadOpsSummary`가 취소되고 새 갱신의 `loadOpsSummary`가 성공하는 순서(취소 거부가 성공보다 늦게 오는 순서 포함), (나) 그 뒤 대시보드 "경고" 카드 상태를 본다. 고치기 전에는 이 시험이 `unavailable`로 실패해야 한다. browser gate mock 화면에서도 같은 문구가 보이는 이유를 확인한다(mock 응답, 갱신 순서, 다시 그리기). 그다음 고친다. 취소된 요청은 `state.summaryError`와 `state.opsSummary`를 바꾸지 않는다. 같은 모양의 `core.ts` 읽기 함수도 같은 규칙을 따른다.
  - 시험: 위 재현 시험(고친 뒤 `empty` 또는 목록), 진짜 실패(예 `500`)는 계속 `unavailable`, 기존 `data-alerts-state` 시험 유지.
  - 검증: `npm run build:served --prefix web`, `npm run test:required --prefix web`, web Pester, `git diff --check`. gate mock 대시보드 1440과 768 캡처에서 "경고" 카드가 "운영 요약을 읽지 못했습니다"를 보이지 않는다. 로컬 commit.

## Task 2: 종료 검증과 PR

- [ ] backlog BL-0043을 닫고(`status=closed`, `closed_by`), `release-train.json` queue 행을 더하고, campaign을 닫는다.
  - 종료 검증(clean HEAD): `dotnet build src/DesktopNode.sln -c Release`, Required CI 네 shard 로컬 실행, Pester 네 종, `Invoke-PcvPrGate.ps1` `ok=true`, `git diff --check origin/main...HEAD`.
  - push, PR, Required CI 다섯 개 green 뒤 `--match-head-commit` merge. merge 뒤 main push run을 기다린다. red면 멈추고 보고한다.

## Nonclaims

- operational current는 `0.43.2-admin-smoke` 그대로다. 설치본 카드는 다음 train 뒤에 바뀐다.
- public trusted signing과 external stable publication을 주장하지 않는다.
