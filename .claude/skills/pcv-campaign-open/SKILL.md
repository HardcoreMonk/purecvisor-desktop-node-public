---
name: pcv-campaign-open
description: 사용자 승인으로 PureCVisor public 저장소에 새 campaign을 연다(pcv-campaign-runner-v2 §2.5). 직전 최종 보고의 다음 승인 번호(예 "1,2,3"), release train 출발 승인, 사용자가 명시한 작업 묶음, 또는 pcvverify completion 판정의 정책 안 갭(completion 모드)을 plan 문서와 active-campaign.json으로 옮기고, opening commit 뒤 같은 턴에서 pcv-campaign 루프로 넘긴다. 사용자가 다음 승인 번호로 답하거나 "캠페인 열어", "train 출발", /pcv-campaign-open을 부르면 사용한다.
---

# PCV campaign open

단일 진실: `$pub/docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-campaign-runner-v2-design.md` §2.1, §2.5.
release train 출발 승인 문구는 `$pub/docs/DEVELOPMENT_PROCEDURE.md` §10과 train 설계 §4.7이 정한다.
`$pub` = 이 저장소 루트(`purecvisor-desktop-node-public`). shell cwd가 곧 `$pub`이다. private 저장소 `purecvisor-desktop-node`는 2026-10-09부터 read-only archive다.

## 1. 확인 (Lane 0)

1. `git -C $pub status --short`가 비어 있어야 한다. 아니면 멈추고 보고한다.
2. `git -C $pub switch main && git -C $pub pull --ff-only`.
3. 지금 campaign이 `status=open`이고 `next_task`가 남아 있으면 열지 않는다. 남은 task를 보여 주고 교체할지 묻는다.
   예외: completion 모드(§5)에서 남은 task가 모두 미래 `task_not_before`면 carry-over로 연다.
4. 승인 원문을 정한다. 번호 답이면 직전 campaign `next_approval_required`(없으면 직전 최종 보고)의 그 번호
   문장을 그대로 쓴다. 번호가 가리키는 문장이 없거나 둘 이상으로 읽히면 묻는다.
5. 승인 문장에서 권한을 뽑는다. 문장에 없는 권한은 열지 않는다.

   | 승인 문장에 명시됨 | 필드 |
   | --- | --- |
   | 기본(Lane 1 개발) | `allowed_lanes=["0","1"]` |
   | Lane 2 host mutation(제품 Update/Rollback, VM 생성·삭제, MSI, Burn, MSIX, fullgate 등)과 그 범위 | `"2"` 추가, `mutation_allowed=true` |
   | Lane 3, `current-evidence.json` 쓰기, operational 승격 | `"3"` 추가, `current_write_allowed=true` |
   | branch push, PR | `push_allowed=true` |
   | green CI 뒤 merge | `merge_policy="after-green-ci"` |

   `commit_policy`는 `local-commit-per-task`다(2026-09-27 사용자 승인). host mutation 범위는 plan
   `사용자 결정` 표에 승인 문장 그대로 적는다.

## 2. plan 문서

경로: `$pub/docs/superpowers/plans/<yyyy-mm-dd>-purecvisor-desktop-node-<주제>.md`. 직전 plan
(`git -C $pub log -1 --format=%H -- docs/superpowers/plans`가 가리키는 문서)의 형식을 따른다. 본문은 한국어.

```markdown
# <제목> Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** <한두 문장>

**Architecture:** <설계·evidence 근거, branch 단위>

**Tech Stack:** <도구>

## 사용자 결정 (<yyyy-mm-dd>)

승인 원문: `<원문>` (<무엇에 대한 답인지>)

| 항목 | 범위 |
| --- | --- |
| 1 | <차선, host mutation, commit/PR/merge> |

## Global Constraints

- <보존 대상(VM, evidence, artifact)과 새 산출물 이름 규칙>
- <비밀 값 경계: guest 인증 정보·token은 실행 경계에서 만들고 기록하지 않는다>
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 1: <제목>

- [ ] <무엇을, 검증 명령, commit/PR/merge를 한 문장으로>

## Nonclaims

- <operational current 변화 여부>
- public trusted signing과 external stable publication을 주장하지 않는다.
```

task 자르기:
- task 하나가 그 차선 checkpoint 하나(Lane 1 18회, Lane 2·3 12회)에 들어가야 한다.
- Lane 2는 실행, 복구, 기록을 따로 나눈다(예: orchestrator 실행 / fullgate 복구 / current-card / facts).
- 마지막 task는 clean HEAD 종료 검증과, 승인이 있으면 push·PR·green CI 뒤 merge다.

## 3. campaign 파일

`$pub/docs/ga-ready/active-campaign.json`을 새로 쓴다. 직전 id는 `closed_predecessor`.

```json
{
  "schema_version": 1,
  "contract": "pcv-active-campaign-v1",
  "status": "open",
  "id": "<주제>-<yyyymmdd>",
  "intent": "<짧은 intent>",
  "working_authority": "source_head",
  "allowed_lanes": ["0", "1"],
  "mutation_allowed": false,
  "current_write_allowed": false,
  "next_step": "Run Task 1 from <plan 경로>, then the queue in order.",
  "plan": "<plan 경로>",
  "task_queue": ["Task 1"],
  "task_lanes": {},
  "next_task": "Task 1",
  "completed_tasks": [],
  "commit_policy": "local-commit-per-task",
  "push_allowed": false,
  "merge_policy": "none",
  "checkpoint_limit": 1,
  "approval_locator": "User-Approval: 2026-09-27 campaign runner commit policy local-commit-per-task; <yyyy-mm-dd> \"<원문>\" (<번호별 요약>)",
  "next_approval_required": [],
  "stop_on": ["same-cause-fail-3", "budget", "checkpoint-limit", "verification-red", "approval-required-mutation", "new-design-required", "permission-denied", "dirty-tree-at-start"],
  "out_of_scope_is_not_next_step": true,
  "closed_predecessor": "<직전 campaign id>"
}
```

- `task_lanes`에는 Lane 1이 아닌 task만 적는다(예: `{"Task 2": "2", "Task 7": "3"}`).
- `checkpoint_limit`은 task 수다.
- 기존 파일의 들여쓰기(2칸, 배열은 한 줄에 한 값)를 따른다.

## 4. commit과 넘기기

1. `git -C $pub switch -c lane<첫 task 차선>/<주제>-<yyyymmdd>`.
2. 필요하면 `docs/DOCUMENTATION_INDEX.md`에 plan 줄을 더한다(직전 opening commit과 같은 위치).
3. `git diff --check` 뒤 commit `docs: open <campaign id>`(본문에 승인 요약, 끝에 system 지정 Co-Authored-By 줄).
4. 같은 턴에서 `pcv-campaign` 루프를 `checkpoint 1/N`으로 시작한다.

## 5. completion 모드 (pcv-completion-autopilot-v1 §2.6)

`pcv-campaign` §6이 `pcvverify completion` exit `1` 결과로 부를 때 쓴다. 사용자 번호 답 대신 정책 승인이 원문이다.
정책 `status=paused`(ADR-0017)면 이 모드로 열지 않는다. `scenario` 갭은 `auto=false`라 task로 만들지 않는다.
설계: `$pub/docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-autopilot-design.md`.

1. 입력: 판정 결과 `artifacts/completion/<yyyymmdd>/result.json`(head SHA, 갭 목록)과
   `$pub/config/completion-autopilot-policy.json`. 정책에 없는 갭 종류가 있으면 열지 않고 멈춘다.
2. 갭을 정책 `auto`로 나눈다. `auto=false`(`user-decision`, `new-design`)는 task로 만들지 않고 새 campaign
   `next_approval_required`에 번호 문장으로 둔다. `ci-wait`는 task로 만들지 않는다.
3. task 순서와 템플릿:
   1. `lane1-fix`: 갭마다 Lane 1 구현 task. product payload를 바꾸면 같은 PR에 `release-train.json` `queue` 행을 더한다.
      끝에 종료 검증과 push·PR·green CI 뒤 merge task 하나.
   2. `train-departure`: 직전 train plan(`$pub/docs/superpowers/plans/*-train-*.md` 중 최신)의 task 목록을 그대로 따른다
      (package, pair, fullgate, current-card, 적재 행별 Lane 2 probe, Lane 3, 단일 PR train ship). version은 판정 요약의
      다음 patch(예 `0.42.92-admin-smoke`). 출발 승인 문구 칸(DEVELOPMENT_PROCEDURE §10)은 정책 행 `mutation_scope`와
      판정 갭에서 채운다.
   3. `lane2-probe`: 갭마다 Lane 2 실행 task와 기록 task.
   4. `deadline-wait`: 갭마다 Lane 1 확인 task, `task_not_before`에 갭 `not_before`.
   5. 마지막: clean `main` 판정 task(`pcv-campaign` §6). exit `0`이면 감사 문서로 완료를 적는다.
4. campaign 필드: id `completion-<yyyymmdd>`(같은 날 둘째부터 `-r2`), plan
   `docs/superpowers/plans/<yyyy-mm-dd>-purecvisor-desktop-node-completion-<yyyymmdd>.md`.
   권한은 들어간 갭 종류의 정책 행을 합친 것이다: `allowed_lanes` 합집합, Lane 2가 있으면 `mutation_allowed=true`,
   `current_write_allowed`는 행 중 하나라도 `true`면 `true`, `push_allowed`, `merge_policy`는 행 값. `task_lanes`,
   `task_not_before`를 채운다. `generated_from`에 `{"result": "<result.json 경로>", "head": "<sha>", "gaps": [<갭 id>]}`.
   `approval_locator`는 `User-Approval: 2026-09-27 campaign runner commit policy local-commit-per-task; ` 뒤에 정책
   `approval_locator`를 그대로 붙인다. 열린 ADR-0016 standing approval 문장이 직전 campaign locator에 있으면 그대로 옮긴다.
5. plan `## 사용자 결정` 표: 승인 원문은 정책 `approval_locator`, 행마다 갭 id, 갭 종류, 정책 행(차선, `mutation_scope`,
   Lane 3, push/merge).
6. carry-over: 옮겨 오는 task는 원래 plan 문장과 `task_not_before`를 그대로 가져온다. 옛 campaign은 같은 opening 변경에서
   `status=closed`, `next_task=null`, `carried_tasks`로 닫고, 옛 plan의 그 task 아래에 이관 줄을 붙인다.
7. commit과 넘기기는 §4와 같다. branch는 첫 task 차선의 `lane<L>/completion-<yyyymmdd>`다.
