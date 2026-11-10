# BL-0044 대시보드 경고 카드 읽는 중 문구 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 첫 방문의 첫 갱신이 끝나기 전에는 대시보드 "경고" 카드가 "운영 요약을 읽지 못했습니다" 대신 읽는 중 문구를 보이게 한다(backlog BL-0044). 운영 위생 C7을 다시 `open_counts=0, undecided=0`으로 만든다. 제품 변경은 release-train queue 행으로 쌓고, 설치본 반영은 따로 승인받는 다음 train에서 한다.

**Architecture:** 근거는 backlog 행 BL-0044(train `0.43.3` Task 6 probe, `docs/ga-ready/evidence/lane2-bl0043-probe-2026-11-11-04303.md`)다. 첫 방문에서 0.9초부터 첫 갱신이 끝나는 9.6~10.7초까지 카드가 "운영 요약을 읽지 못했습니다"(`data-alerts-state="unavailable"`)를 보였다. `view-dashboard.ts` `alertsUnavailable()`은 `state.lastRefreshedAt === null`이면 `hasRefreshedOperation('ops.summary')`가 `false`라 unavailable로 그리고, 읽는 중 상태가 따로 없다. 카드에 읽는 중 상태를 하나 더한다. branch는 `lane1/bl0044-alerts-loading-20261111` 하나이고 PR도 하나다. grok(Herdr `grok-status`)이 구현하고 Claude가 리뷰한다.

**Tech Stack:** TypeScript Web Console(`npm run build:served --prefix web`), `web/i18n.js`, node 시험(`web/node-tests`), browser-ui-gates, web Pester, `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"1, 2 문구 수정"` (`train-04303-20261111` 마무리 보고의 다음 승인 1·2에 대한 답). 2번 문장은 "backlog 분류: BL-0044(설치본 대시보드 "경고" 카드가 첫 방문의 첫 갱신이 끝나기 전 약 9초 동안, 운영 요약을 기다리는 중에도 "운영 요약을 읽지 못했습니다"를 보인다)를 counts 또는 out-of-scope로 정한다."이고, 답 `문구 수정`은 `counts`로 분류하고 그 문구를 고친다는 뜻이다. 1번(train 계정 백업 삭제)과 분류는 PR #145 문서 기록으로 끝났다.

| 항목 | 범위 |
| --- | --- |
| 1 | Lane 0/1. BL-0044 구현(grok)과 리뷰(Claude). task마다 로컬 commit |
| 2 | push, PR 하나, Required CI 다섯 개 green 뒤 merge(`merge_policy=after-green-ci`, 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`). merge 뒤 main push run 확인 |
| 3 | 제품 변경은 `release-train.json` queue 행 하나로 쌓는다. release train 출발은 하지 않는다(따로 승인) |
| 4 | host mutation 없음. 설치본, service, VM을 바꾸지 않는다. `current-evidence.json`을 쓰지 않는다 |
| 설계 | 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 정한다. 첫 갱신이 진행 중이고(`state.loading`) 아직 끝난 적이 없으며(`state.lastRefreshedAt === null`) 요약 오류가 없으면 카드는 `data-alerts-state="loading"`과 새 i18n 키 `copy.dashboard.alerts.loading`(ko `운영 요약을 읽고 있습니다.`, en `Reading the ops summary.`)을 보인다. 갱신이 끝났거나 실패하면 지금 규칙(`unavailable`, `empty`, 목록)을 그대로 쓴다. 첫 갱신이 통째로 실패해 `lastRefreshedAt`이 비어도 `state.loading`이 끝나면 `unavailable`로 돌아가야 한다 |

## Global Constraints

- 고치는 범위는 BL-0044뿐이다. 그 밖의 발견은 backlog `undecided` 행으로 쓴다.
- 새 route, 새 권한, 새 `Add-Type`/P/Invoke/native ACL을 만들지 않는다. 서버 코드는 바꾸지 않는다.
- Web 규칙:
  - `web/src`를 고친 뒤 `npm run build:served --prefix web`를 돌린다. `web/app.bundle.js`와 `web/sw.js` CACHE_NAME은 생성물이다.
  - 새 문구는 i18n 키(ko 기본, en 같은 키)로 쓴다. 색은 기존 token만 쓴다. 계약·Pester·browser fixture가 붙잡은 markup과 문자열은 같은 강도로 옮긴다.
  - axe violation 0. `--prune`은 쓰지 않는다.
- grok 지시는 약 20분 분량으로 쓰고, 마감 시각을 적는다. grok은 commit·push하지 않고 `docs/**`를 고치지 않는다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 읽는 중 상태와 문구 (Web)

- [x] `view-dashboard.ts`의 경고 카드에 읽는 중 상태를 더한다. 조건과 문구는 위 `설계` 행이다. `i18n.js` ko/en에 `copy.dashboard.alerts.loading`을 더한다. 갱신이 끝나면 카드가 다시 그려지는지(지금처럼 `refreshAll()` 끝의 `render()`) 확인한다.
  - 시험: node 시험으로 (가) 첫 갱신 진행 중이면 `loading`과 새 문구, (나) 첫 갱신이 끝나면 `empty` 또는 목록, (다) 첫 갱신이 통째로 실패하면(`state.loading=false`, `lastRefreshedAt=null`) `unavailable`, (라) `summaryError`가 있으면 진행 중이어도 `unavailable`. 기존 `data-alerts-state` 시험과 BL-0043 시험 유지. i18n ko/en 같은 키 시험 통과.
  - 검증: `npm run build:served --prefix web`, `npm run test:required --prefix web`, web Pester, `git diff --check`. gate mock 대시보드 1440과 768 캡처를 본다. 로컬 commit.

실행 기록(2026-11-11): grok이 02:04~02:19에 구현했고 Claude가 리뷰했다.
- `view-dashboard.ts`: 새 함수 `alertsLoading()`은 `state.loading === true`, `state.lastRefreshedAt === null`, `summaryError` 없음일 때 참이다. `renderAlerts()`는 body를 비운 뒤 읽는 중이면 `<p role="status" data-alerts-state="loading">`에 `copy.dashboard.alerts.loading`만 그리고 돌아간다. 그 밖에는 지금 규칙(`unavailable`, `empty`, 목록)이다.
- `i18n.js`: `copy.dashboard.alerts.loading` ko `운영 요약을 읽고 있습니다.`, en `Reading the ops summary.`
- 다시 그리기: `refreshAll()`이 시작할 때 `loading=true`로 `render()`하고, `finally`에서 `loading=false`로 다시 `render()`한다. `render()`는 활성 view(대시보드)를 다시 그린다. 첫 갱신이 통째로 실패하면 `lastRefreshedAt`이 비어 있어도 `loading`이 꺼진 뒤 `unavailable`이다. bootstrap의 첫 `render()`는 `refreshAll()` 바로 앞에서 같은 동기 흐름으로 이어지므로 `unavailable`이 화면에 보이지 않는다(리뷰 확인).
- node 시험 1건(네 경우: 진행 중 `loading`, 끝나면 `empty`, 통째 실패 `unavailable`, 진행 중이어도 `summaryError`면 `unavailable`) 추가.
- 결과: `npm run test:required --prefix web` exit 0(node 400/400, browser gate `ok=true` assertions 121 failed 0 violations 0, 기준선 변화 없음), web Pester 50/50, `git diff --check` clean. `web/sw.js` CACHE_NAME은 `pcv-ui-ve4b62dac`다.
- 화면: gate mock 대시보드 1440×900과 768×1024(`artifacts/bl0044-alerts/`, gitignore)에서 갱신 뒤 카드 본문은 `경고 없음`이다. gate는 갱신이 끝난 뒤 대시보드를 찍으므로 읽는 중 문구는 node 시험이 확인한다. 설치본의 읽는 중 화면은 다음 train probe에서 본다.

## Task 2: 종료 검증과 PR

- [x] backlog BL-0044를 닫고(`status=closed`, `closed_by`), `release-train.json` queue 행을 더하고, campaign을 닫는다.
  - 종료 검증(clean HEAD): `dotnet build src/DesktopNode.sln -c Release`, Required CI 네 shard 로컬 실행, Pester 네 종, `Invoke-PcvPrGate.ps1` `ok=true`, `git diff --check origin/main...HEAD`.
  - push, PR, Required CI 다섯 개 green 뒤 `--match-head-commit` merge. merge 뒤 main push run을 기다린다. red면 멈추고 보고한다.

실행 기록(2026-11-11):
- 종료 검증(clean HEAD `08589a5`): Release build 성공. Required CI 네 shard(`dotnet`, `web`, `delivery`, `installer-policy`) 로컬 실행 `ok=true`. dotnet shard 시험 3,080/3,080(Service 11, Contracts 201, Cli 186, Runtime 129, HyperV 288, Delivery 785, Host 246, Api 545, Verification 689). Pester 실패 0: packaging 528, installer 49, web 50, manual-admin 140.
- backlog BL-0044를 닫았다(`closed_by` PR #146). 운영 위생 C7은 `open_counts=0, undecided=0`이 된다.
- `release-train.json` queue에 PR #146 행 하나(`area=web`, `risk_tier=M`, `lane2_probe=null`)를 더했다. 설치본 반영은 다음 release train(따로 승인)에서 한다.
- campaign을 닫고 다음 승인으로 release train `0.43.4-admin-smoke` 출발 문장을 남겼다. PR #146의 CI·merge·main run 결과는 최종 보고에 적는다.

## Nonclaims

- operational current는 `0.43.3-admin-smoke` 그대로다. 설치본 카드는 다음 train 뒤에 바뀐다.
- public trusted signing과 external stable publication을 주장하지 않는다.
