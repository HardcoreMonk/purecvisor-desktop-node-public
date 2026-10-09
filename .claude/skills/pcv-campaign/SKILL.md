---
name: pcv-campaign
description: PureCVisor public 저장소의 열린 campaign을 연속 실행한다(pcv-campaign-runner-v2). active-campaign.json의 task_queue를 next_task부터 task마다 그 차선(task_lanes, 기본 Lane 1)의 checkpoint 하나로 실행하고, commit_policy·push_allowed·merge_policy가 허용하는 만큼 로컬 commit, PR, green CI 뒤 merge까지 같은 턴에서 이어 간다. 큐가 끝나거나 campaign이 없으면 pcvverify completion으로 완료 기준(v3: C1, S1~S4, C5, C6)을 판정하고, autopilot 정책이 paused가 아니면 정책 안 갭을 다음 campaign으로 연쇄한다. 사용자가 계속/재개/후속 조치/후속 작업/다음 단계/캠페인 진행이라고 하거나 /pcv-campaign을 부르면 사용한다.
---

# PCV campaign runner (v2)

단일 진실: public 저장소 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-campaign-runner-v2-design.md`
(`pcv-campaign-runner-v2`). 이 skill은 그 루프를 실행 순서로 옮긴 것이다. 두 문서가 다르면 spec이 우선한다.

- 저장소: 이 저장소(`purecvisor-desktop-node-public`, 이하 `$pub`). shell cwd가 곧 `$pub`이므로 `git -C $pub`는 `git`과 같다.
- campaign: `$pub/docs/ga-ready/active-campaign.json`
- 한도: `$pub/config/agent-execution-circuit-breaker.json` `lanes.<L>`. Lane 1 30분·18회, Lane 2 45분·12회,
  Lane 3 30분·12회. 70%(Lane 1 21분·13번째, Lane 2 32분·9번째)에 수치로 진행 보고.
- 관련 skill: campaign 열기 `pcv-campaign-open`, push/PR/merge `pcv-ship`.

## 0. run 시작

1. `git -C $pub status --short`가 비어 있지 않으면 시작하지 않는다(`dirty-tree-at-start`). 변경 목록을 보고하고 끝낸다.
2. campaign을 읽는다. `status`가 `open`이 아니거나 `next_task`가 없으면 러너 대상이 아니다.
   - 사용자가 직전 보고의 다음 승인 번호로 답했으면 `pcv-campaign-open`으로 간다.
   - `$pub/config/completion-autopilot-policy.json`이 있으면 §6 completion 판정으로 간다. 정책 `status=paused`(ADR-0017)면
     판정만 돌려 보고하고 새 campaign을 열지 않는다(`autopilot-paused`).
   - 아니면 CLAUDE.md의 one-bounded-checkpoint 규칙을 따른다.
3. 권한 필드를 읽는다. 없는 필드는 기본값이다.

   | 필드 | 기본값 |
   | --- | --- |
   | `allowed_lanes` | `["0","1"]` |
   | `mutation_allowed`, `current_write_allowed`, `push_allowed` | `false` |
   | `merge_policy` | `none` |
   | `task_lanes` | 모든 task `"1"` |
   | `task_not_before` | 모든 task 즉시 실행 가능 |
   | `commit_policy` | `approval_locator`가 비면 `ask-per-commit` |

4. `checkpoint_limit`과 남은 큐로 이번 run의 checkpoint 수 N을 정한다.
5. branch: commit은 `main`이 아닌 작업 branch(`lane<L>/<주제>-<yyyymmdd>`)에 쌓는다. merge 전인 PR branch가
   있으면 그 위에 이어 쌓고, merge된 뒤의 새 branch는 `origin/main`에서 만든다. merge 안 된 branch 위에 다른
   branch를 쌓지 않는다.

## 1. checkpoint 루프 (task 하나 = 그 차선의 checkpoint 하나)

1. 실행할 task를 고른다. `next_task`의 `task_not_before`가 오늘보다 뒤면 건너뛰고 큐에서 그 뒤의 실행 가능한
   task를 먼저 돈다. 남은 task가 모두 미래 `not_before`면 §6 carry-over로 간다.
   plan(`plan` 필드)에서 `## 사용자 결정`, `## Global Constraints`, 그 task의 `## Task N:` 절을 읽는다.
   수정/생성 파일, 검증 명령, host mutation 범위, push/PR/merge 여부를 뽑는다.
2. 차선 L 검사(spec §2.2). 다음이면 `approval-required-mutation`으로 멈춘다.
   - L이 `allowed_lanes`에 없다.
   - L=2인데 `mutation_allowed≠true`, 또는 task가 `사용자 결정` 표 밖의 host mutation을 요구한다.
   - L=3인데 `current_write_allowed≠true`.
3. 시작 계약 공개: `checkpoint k/N`, task id, `lane=L`과 그 예산, 허용 파일, host mutation 범위(L=2),
   검증 명령, push/merge 여부, 범위 밖 발견은 `report-only`.
4. plan 절대로 구현·실행한다. plan의 추정치는 착수 시 실측으로 다시 확인한다.
   - Lane 2: 도구에 `-PlanOnly`/`-WhatIf`/`-DryRun`이 있으면 먼저 돌린다. guest 비밀번호와 token은 실행 경계에서
     만들고 명령줄, summary, evidence에 남기지 않는다. FAIL verdict면 current에 쓰지 않고 멈춘다.
   - Lane 3: `current-evidence.json`은 이 차선에서만 쓴다.
5. focused 검증. red를 checkpoint 안에서 못 고치면 commit하지 않고 멈춘다(`verification-red`).
   같은 원인 실패 3회면 멈춘다.
6. green이면 같은 변경 묶음에:
   - plan의 task checkbox를 `- [x]`로 바꾸고 `실행 기록(<date>): ...` 단락을 붙인다(수치, 시각, SHA).
   - campaign `completed_tasks`에 task id를 더하고 `next_task`를 큐의 아직 끝나지 않은 첫 id로 옮긴다.
   - 이 task의 범위 밖 발견은 `$pub/docs/ga-ready/backlog.json`에 행으로 더한다(`pcv-backlog-v1`: id `BL-<다음 네 자리>`,
     `found_on`, `source`(plan과 task), `summary`, `classification="undecided"`, `basis=null`, `out_of_scope_ref=null`,
     `needs_design`, `status="open"`, `closed_by=null`). 분류는 사용자 결정이므로 러너가 바꾸지 않는다.
   - 마지막 task면 spec §2.4로 닫는다: `status=closed`, `next_task=null`, `next_step` 결과 요약,
     `next_approval_required`에 다음 승인 문장(차선, host mutation 범위, push/PR/merge를 한 문장에).
     닫는 PR이 merge되면 §6 판정과 연쇄로 간다.
7. commit: `local-commit-per-task`면 로컬 commit 하나. 제목에 task id(예: `docs: train phase 3c Task 7 ...`),
   끝에 system이 지정한 Co-Authored-By 줄. `ask-per-commit`이면 여기서 멈추고 승인을 묻는다.
8. task 본문이 push/PR/merge를 적었으면 `pcv-ship`을 따른다. `push_allowed=false`면 멈춘다.
   `merge_policy=none`이면 PR만 열고 merge를 `next_approval_required`에 적는다.
9. 진행 보고 1~3줄(task id, lane, 결과, commit SHA, PR/merge)만 쓰고 턴을 끝내지 않는다. 1번으로 돌아간다.

## 2. 종료 조건

하나라도 생기면 멈추고 최종 보고를 한다. 우회하지 않는다.

- 큐 소진, `checkpoint_limit` 도달
- 차선 예산(시간, batch), 같은 원인 실패 3회, focused 검증 red, Lane 2 FAIL, Required CI fail
- 승인 밖 작업: 차선 검사 실패, `push_allowed=false`인데 push/PR 필요, merge가 필요한데 다음 task가 그 변경을
  `main`에서 요구함
- `new-design-required`, 새 `Add-Type`/`P/Invoke`/native ACL/installer handoff
- 권한 거부(auto mode classifier 포함)
- `deadline-wait`: 남은 task가 모두 미래 `not_before`이고 §6 carry-over로 새 campaign을 열 수 없음
- `no-progress`: §6 판정의 갭 id 집합이 직전 판정과 같고 그 사이 merge가 없음
- §6 판정 갭이 모두 정책 밖(`user-decision`, `new-design`, `scenario`)이거나 판정 exit `0`
- `autopilot-paused`: 정책 `status=paused`라 §6 판정 뒤 연쇄하지 않음

범위 밖 발견은 종료 조건이 아니다. backlog 행으로 쓰고 최종 보고 `report-only`에 그 id를 적는다.

## 3. 최종 보고

- 완료 task별 차선, commit SHA, PR 번호와 merge commit
- 종료 조건과 남은 `next_task`
- host mutation 요약(무엇을 바꿨고 끝 상태가 무엇인지)
- `report-only` 발견
- `lane=`, `working_authority=source_head`, `current_evidence_written=false|true`
- `$pub` git status(clean 여부)와 현재 branch
- 다음 승인: `next_approval_required`를 번호 목록으로 보여 준다. 사용자는 번호로 답한다(`pcv-campaign-open`).
- 마지막 줄은 `/goal` 평가기가 읽을 증거 줄이다. 값은 직전 명령 출력에서 옮긴다. 진행 보고(1~3줄)에도 같은 줄을 붙인다.

      goal-evidence: campaign=<id> status=<open|closed> next_task=<id|null> completed=<n>/<N> head=<sha> pr=<#n|none> pr_state=<OPEN|MERGED|none> merge=<sha|none> main=<sha> tree=<clean|dirty> completion=<met>/7|none stop=<none|원인> approval_needed=<none|번호 목록>

  `completion=`은 이 run에서 마지막으로 돌린 `pcvverify completion` 요약 줄의 `met` 값이다. 돌리지 않았으면 `none`.

## 4. 여러 턴 실행

상태는 campaign 파일과 git log가 소유한다. `/pcv-campaign`(또는 `계속`)으로 다시 부르면 `next_task`부터
이어진다. 무인 실행은 `/loop /pcv-campaign` 또는 `/goal`(조건은 `pcv-goal`이 만든다). `/goal`이 걸려 있으면
CI 대기를 foreground로 하고, 정지 조건은 `goal-evidence:`의 `stop=`으로 보고한다.

## 5. 알려진 함정

- `policy-boundaries`(PolicyBoundary) 시험은 dirty tree에서 `cutover-worktree=dirty`로 실패한다. 솔루션 전체와
  Required CI shard는 commit 뒤 clean HEAD에서 돈다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다. Delivery inventory와 migration manifest가 깨진다. 시험은 Delivery C#
  계약으로 쓴다.
- Web 문구·binding을 바꾸면 pin된 다섯 곳과 web Pester를 함께 바꾼다.
- 같은 version MSI fullgate를 다시 돌리면 ARP 중복이 남을 수 있다. 사후 검사에서 같은 version ARP 1개를 확인한다.
- shard artifact root는 저장소 `artifacts/` 아래에 둔다.

## 6. completion 판정과 연쇄 (pcv-completion-autopilot-v1)

단일 진실: `$pub/docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-autopilot-design.md`.
완료 기준: `$pub/docs/adr/0017-scenario-delivery-completion.md`(v3: C1, S1~S4, C5, C6). v2 설계는 역사 기록이다.

1. 판정은 clean `main` HEAD에서만 한다(`git -C $pub switch main && git -C $pub pull --ff-only`). 날짜는 `yyyymmdd`.

       gh run list --branch main --event push --limit 30 --json databaseId,workflowName,status,conclusion,headSha,createdAt > artifacts/completion/<yyyymmdd>/main-runs.json
       dotnet run --project src/DesktopNode.Verification -c Release -- completion --ci-runs artifacts/completion/<yyyymmdd>/main-runs.json --output artifacts/completion/<yyyymmdd>/result.json

   마지막 줄 `completion: complete=<bool> met=<n>/7 gaps=<n> head=<sha>`와 `gap ...` 줄을 진행 보고에 옮긴다.
   `hygiene ...`, `hygiene-gap ...` 줄(v2의 C2·C3·C4·C7)은 보고만 하고 연쇄 대상이 아니다.
2. exit `0`: 완료다. 판정 결과를 인용한 감사 문서 `docs/project-status-audit-<yyyy-mm-dd>.md`를 Lane 1 task로 쓰고 `pcv-ship`으로
   merge한 뒤 멈춘다. 완료는 이 경우에만 적는다.
3. exit `2`: 입력 오류다. `verification-red`로 멈춘다.
4. exit `1`: 정책 `status=paused`면 갭을 연쇄하지 않고 `autopilot-paused`로 최종 보고한다. 아니면 갭을 정책
   (`config/completion-autopilot-policy.json` `gap_kinds`)으로 나눈다.
   - `ci-wait`: head의 run이 끝날 때까지 `gh run watch`로 기다린 뒤 1번부터 다시 판정한다(같은 head에서 한 번만).
   - `auto=true` 갭(`lane1-fix`, `train-departure`, `lane2-probe`, `deadline-wait`)이 있으면 `pcv-campaign-open` completion
     모드로 같은 턴에 새 campaign을 열고 §1 루프를 잇는다.
   - `auto=false` 갭(`user-decision`, `new-design`, `scenario`)은 `next_approval_required`에 번호 문장으로 둔다. `scenario` 갭은
     그 시나리오 단계 campaign 승인 문장이다. auto 갭이 없으면
     최종 보고로 멈춘다.
   - 갭 id 집합이 직전 판정(직전 campaign `generated_from`의 결과)과 같고 그 사이 merge가 없으면 `no-progress`로 멈춘다.
5. carry-over: 열린 campaign의 남은 task가 모두 미래 `task_not_before`면, 판정을 돌려 auto 갭이 있을 때만 새 campaign을
   열고 그 task들을 옮긴다. 옛 campaign은 같은 opening 변경에서 `status=closed`, `next_task=null`,
   `carried_tasks={"<task>": "<새 id>"}`로 닫는다. auto 갭이 없으면 옛 campaign을 열어 둔 채 `deadline-wait`로 멈춘다.
6. 정책은 승인 4의 범위만 연다. 정책이 연 campaign도 항상 멈추는 조건(FAIL, `new-design`, backlog `undecided` 분류, 새
   `Add-Type`/`P/Invoke`/native ACL/installer handoff, 영구 범위 밖, 보존 VM 변경, 정책에 없는 갭 종류)에서 멈춘다.
