# Desktop Node 완료 기준 autopilot (판정, backlog, 정책, 연쇄)

- Design-ID: `pcv-completion-autopilot-v1`
- 작성일: `2026-10-07`
- 문서 상태: `accepted` (2026-10-07 사용자 승인 `1,2,3,4,5`의 1~4. 4는 "Lane 2/3 모두 자동"으로 다시 확인)
- 변경 등급: M (검증 도구 명령 추가, campaign 필드 추가. 제품 payload 변경 없음)
- 입력: 완료 정의 v2 `pcv-project-completion-definition-v2`, 러너 `pcv-campaign-runner-v2`, release train `docs/DEVELOPMENT_PROCEDURE.md` §10,
  단일 PR train `pcv-single-pr-train-v1`
- host/VM/service/package mutation: `false` (이 설계의 구현. 정책이 여는 다음 campaign은 §2.5 범위)

## 1. 문제

완료 정의 v2 §1과 같다. 판정이 사람 표였고, `report-only` 발견이 저장되지 않았으며, 남은 일을 campaign task로 바꾸는 규칙이
없어 사용자가 매번 승인 번호로 campaign을 열어야 했다. 기한 대기 task 하나가 큐 전체를 막기도 했다.

## 2. 결정

### 2.1 판정 명령

    gh run list --branch main --event push --limit 30 --json databaseId,workflowName,status,conclusion,headSha,createdAt > artifacts/completion/<yyyymmdd>/main-runs.json
    dotnet run --project src/DesktopNode.Verification -c Release -- completion --ci-runs artifacts/completion/<yyyymmdd>/main-runs.json [--head <sha>] [--today <yyyy-mm-dd>] [--output <path>]

- 읽기 전용이다. 입력은 `config/project-completion-criteria.json`, `docs/ga-ready/current-evidence.json`,
  `docs/ga-ready/release-train.json`, `config/desktop-node-feature-evidence-ledger.json`, `docs/ga-ready/backlog.json`,
  `--ci-runs` 파일이다. 네트워크를 쓰지 않는다. CI 결과는 호출자가 `gh`로 받아 파일로 넘긴다.
- `--head` 기본값은 저장소 `HEAD`, `--today` 기본값은 로컬 날짜다. 시험은 둘 다 명시한다.
- `--output`이 있으면 결과 계약 `pcv-project-completion-result-v1` JSON을 그 경로(저장소 `artifacts/` 아래)에 쓴다. stdout에는
  항상 조건별 줄과 마지막 요약 한 줄을 쓴다.

      completion: complete=<true|false> met=<n>/7 gaps=<n> head=<sha>

- exit `0`: 완료(C1~C7 모두 충족). exit `1`: 갭이 있음. exit `2`: 입력 오류(파일 없음, 계약 불일치, 형식 오류).

결과 형식:

```json
{
  "schema_version": 1,
  "contract": "pcv-project-completion-result-v1",
  "definition": "pcv-project-completion-definition-v2",
  "head_sha": "<sha>",
  "evaluated_on": "<yyyy-mm-dd>",
  "complete": false,
  "met_count": 4,
  "condition_count": 7,
  "conditions": [{ "id": "C2", "met": false, "detail": "queue has 1 row" }],
  "gaps": [{ "id": "C2-queue", "condition": "C2", "kind": "train-departure", "lane": "2", "summary": "...", "refs": ["pr:59"], "not_before": null }]
}
```

### 2.2 조건별 판정과 갭 종류

갭 종류는 일곱 가지다: `train-departure`, `lane2-probe`, `lane1-fix`, `new-design`, `deadline-wait`, `user-decision`, `ci-wait`.

| 조건 | 충족 규칙 | 미충족일 때 갭 |
| --- | --- | --- |
| C1 | `--head`의 `Development Gates` push run이 `completed`·`success` | run 없음이나 진행 중: `ci-wait`. 실패: `lane1-fix` |
| C2 | `current-evidence.json` `current.version` = `release-train.json` `operational_current`, 그 version의 train `status=promoted`, `queue` 행 `0` | `queue` 행 있음: `train-departure`(refs는 PR 번호, 요약에 Lane 2 probe 기능군). version 불일치: `user-decision`. 마지막 train이 `promoted`가 아님: `train-departure` |
| C3 | ledger의 `candidate_required=true` feature가 모두 `current.verdict=pass`이고 `current-evidence.json` `feature_qualification.promotion_eligible=true`, `blockers` 빈 목록 | feature마다 `lane2-probe` |
| C4 | criteria `service_plan_items` `15`개가 각각 `evidence_ids`(모두 `docs/ga-ready/evidence/<id>.md`로 존재) 또는 `waiver`(존재하는 결정 문서 경로)를 가진다 | 항목마다 `lane2-probe`. 둘 다 없거나 파일이 없으면 그 항목 |
| C5 | `--head`의 `Development Gates`와 `Public Boundary Contract` push run이 모두 `success`이고, criteria `deadline_risks`에 `status=open` 행이 없다 | run 없음이나 진행 중: `ci-wait`. 실패: `lane1-fix`. 열린 위험: 기한 전이면 `deadline-wait`(`not_before`=기한), 기한 당일이나 뒤면 `lane1-fix`(확인하고 위험 행을 닫거나 대응) |
| C6 | criteria `permanent_out_of_scope`가 v2 §3의 다섯 항목을 가진다 | 형식 오류(exit `2`) |
| C7 | backlog에 `status=open`이면서 `classification`이 `counts`이거나 `undecided`인 행이 없다. `out-of-scope` 행의 `out_of_scope_ref`가 criteria `permanent_out_of_scope` id가 아니면 `undecided`로 센다 | `undecided`: `user-decision`. `counts`: `needs_design=true`면 `new-design`, 아니면 `lane1-fix` |

`lane` 값은 갭을 닫는 첫 task의 차선이다. `train-departure`는 `2`, `lane2-probe`는 `2`, 나머지는 `1`, `user-decision`은 `0`이다.

### 2.3 criteria 계약 (`pcv-project-completion-criteria-v1`)

```json
{
  "schema_version": 1,
  "contract": "pcv-project-completion-criteria-v1",
  "definition": "pcv-project-completion-definition-v2",
  "workflows": { "c1": ["Development Gates"], "c5": ["Development Gates", "Public Boundary Contract"] },
  "service_plan_items": [{ "id": "P0-1", "title": "미디어 재장착", "evidence_ids": ["service-plan-p0-actual-vm-2026-08-27-04275"], "waiver": null }],
  "deadline_risks": [{ "id": "ubuntu-26-runner", "summary": "...", "deadline": "2026-10-19", "status": "open", "closed_by": null }],
  "permanent_out_of_scope": [{ "id": "public-trusted-signing", "summary": "..." }]
}
```

- `service_plan_items` 값은 감사 2026-10-06 §3과 v1 §4 보정의 evidence id를 옮긴다. P1-9는 `waiver`에 v1 문서 경로를 적는다.
- 위험을 닫을 때는 `status=closed`, `closed_by`에 run id나 PR을 적는다.

### 2.4 backlog 계약 (`pcv-backlog-v1`)

```json
{
  "schema_version": 1,
  "contract": "pcv-backlog-v1",
  "rows": [{
    "id": "BL-0001", "found_on": "2026-10-07", "source": "<plan task 또는 설계 절>", "summary": "...",
    "classification": "undecided", "basis": null, "out_of_scope_ref": null, "needs_design": true,
    "status": "open", "closed_by": null
  }]
}
```

- id는 `BL-` 뒤 네 자리 순번이다. 지운 행의 번호를 다시 쓰지 않는다. 행을 지우지 않고 `status=closed`로 닫는다.
- 러너는 `report-only` 발견을 그 task의 commit에 `undecided` 행으로 쓴다. 분류(`counts`, `out-of-scope`)는 사용자 결정이고
  `basis`에 결정 날짜와 승인 문장을 적는다.

### 2.5 autopilot 정책 (`pcv-completion-autopilot-policy-v1`)

`config/completion-autopilot-policy.json`이 갭 종류마다 사용자 승인 없이 열 수 있는 권한을 적는다. 값은 2026-10-07 승인
4("Lane 2/3 모두 자동")다.

| 갭 종류 | 자동 | 차선 | host mutation | Lane 3 | push, PR, merge |
| --- | --- | --- | --- | --- | --- |
| `lane1-fix` | 예 | 0, 1 | 없음 | 아니요 | 예, green CI 뒤 merge |
| `deadline-wait` | 예 | 0, 1 | 없음 | 아니요 | 예, green CI 뒤 merge |
| `ci-wait` | 예 | 0 | 없음 | 아니요 | 아니요 |
| `train-departure` | 예 | 0, 1, 2, 3 | train 표준 범위: MSI 설치·repair·제거·`REMOVE_DATA`, service, pair 여섯 bucket, fullgate route parity VM, fullgate `os-mutation-gate`(firewall 규칙, Event Log source, LAN listener, 2026-10-08 추가 승인), probe VM 생성·삭제 | 예 | 예, green CI 뒤 merge |
| `lane2-probe` | 예 | 0, 1, 2 | probe VM 생성·삭제, service 중지·시작 | 아니요 | 예, green CI 뒤 merge |
| `new-design` | 아니요 | | | | |
| `user-decision` | 아니요 | | | | |

2026-10-08 추가 승인(`completion-20261007` Task 4 정지 보고의 1)으로 `os-mutation-gate`를 `train-departure` 범위에 더했다. LAN prefix는 실행 값으로만 쓰고 저장소에 남기지 않으며, 끝 상태는 PureCVisor firewall 규칙 `0`이다.

항상 멈추는 조건: FAIL(train 정차 포함), `new-design`, backlog `undecided` 행의 분류, 새 `Add-Type`/`P/Invoke`/native ACL/installer
handoff, 영구 범위 밖 항목, 보존 VM `pcv-guest-installed-04253-r1` 변경, 정책에 없는 갭 종류.

- 정책 파일의 `approval_locator`에 승인 원문을 둔다. 정책을 바꾸거나 넓히려면 새 사용자 승인 문장이 필요하다. 철회는 해당 종류를
  `auto=false`로 바꾸거나 파일을 지우는 것이다. 러너는 판정 때마다 정책을 다시 읽는다.
- runner v2 §3("campaign은 없는 승인을 만들지 않는다")과의 관계: 생성 campaign의 권한은 그 campaign에 들어간 갭 종류의 정책
  행을 합친 것이고, `approval_locator`는 정책 `approval_locator`를 그대로 인용한다. 정책은 사람이 승인한 문장을 기계가 읽는
  형식으로 옮긴 것이므로 runner v2 §5의 "승인 문장의 자동 해석"이 아니다.

### 2.6 갭 → campaign (completion 모드)

판정 결과의 갭을 다음 순서로 task로 바꾼다. 하나의 판정에서 하나의 campaign을 만든다.

1. `user-decision`, `new-design`: task로 만들지 않는다. `next_approval_required`에 번호 문장으로 둔다. 다른 갭이 이것에 의존하지 않으면
   나머지는 진행한다.
2. `ci-wait`: task로 만들지 않는다. 러너가 CI 완료를 기다려 다시 판정한다.
3. `lane1-fix`: 갭마다 Lane 1 task 하나(구현, focused 검증, 로컬 commit)와 종료 task(push, PR, green CI 뒤 merge). product payload를
   바꾸면 §10 대기열 규칙대로 `queue` 행을 같은 PR에 더한다.
4. `train-departure`: `queue`(3에서 더한 행 포함)를 싣는 train task 묶음. 직전 train plan(`docs/superpowers/plans/*-train-*.md`)의 task
   목록을 따른다(package, pair, fullgate, current-card, 적재 행별 Lane 2 probe, Lane 3, 단일 PR train ship). version은 `operational_current`의
   다음 patch다. 출발 승인 문구 칸(§10)은 정책 행과 판정 결과로 채운다.
5. `lane2-probe`: 갭마다 Lane 2 실행 task와 기록 task.
6. `deadline-wait`: `task_not_before`가 붙은 Lane 1 확인 task.
7. 마지막 task: clean `main` HEAD에서 판정을 다시 돌린다. exit `0`이면 날짜별 감사 문서 `docs/project-status-audit-<date>.md`에 결과를
   인용해 완료를 적고 push, PR, green CI 뒤 merge한다. exit `1`이면 결과를 기록하고 §2.7 연쇄로 넘어간다.

생성 campaign id는 `completion-<yyyymmdd>`(같은 날 둘째부터 `-r2`), plan은
`docs/superpowers/plans/<yyyy-mm-dd>-purecvisor-desktop-node-completion-<yyyymmdd>.md`다. 사용자 결정 표의 승인 원문은 정책
`approval_locator`이고, 표에는 갭 id와 정책 행을 적는다.

### 2.7 runner v2에 더하는 것

campaign 필드는 더하기만 한다(runner v2 §2.1).

| 필드 | 뜻 | 없을 때 |
| --- | --- | --- |
| `task_not_before` | task id → `yyyy-mm-dd`. 그 날짜 전에는 그 task를 건너뛴다 | 모든 task 즉시 |
| `carried_tasks` | 닫을 때 다음 campaign으로 옮긴 task id → 새 campaign id | 빈 객체 |
| `generated_from` | completion 모드로 열렸으면 판정 결과 경로와 head SHA | 없음 |

- 건너뛰기: 다음 task의 `not_before`가 미래면 큐에서 그 뒤의 실행 가능한 task를 먼저 돈다. 남은 task가 모두 미래 `not_before`면,
  같은 턴에 completion 모드로 새 campaign을 열 수 있을 때 그 task들을 새 campaign으로 옮기고(`carried_tasks`) 지금 campaign을 닫는다.
  열 수 없으면 `deadline-wait`로 멈추고 campaign은 열어 둔다. `stop_on`에 `deadline-wait`를 더한다.
- backlog: `report-only` 발견을 그 task commit에 backlog 행으로 쓰고, 최종 보고의 `report-only`에 backlog id를 적는다.
- 닫을 때 연쇄: campaign을 닫는 merge 뒤 clean `main`에서 판정을 돌린다. exit `0`이면 연쇄를 끝낸다. exit `1`이면 정책 안 갭으로
  같은 턴에 새 campaign을 열고 루프를 잇는다. 정책 밖 갭만 남으면 `next_approval_required`에 번호로 두고 멈춘다.
- 무진전 정지: 직전 판정과 갭 id 집합이 같고 그 사이 merge가 없으면 `no-progress`로 멈춘다. 같은 갭으로 campaign을 두 번 연속 열지
  않는다.
- 회로 차단기: checkpoint마다 차선 예산, 같은 원인 3회 실패, red, FAIL 정지는 그대로다. 연쇄는 예산을 넘기지 않는다.

### 2.8 `/goal`

`pcv-goal`의 완료 템플릿은 "`pcvverify completion`이 clean `main` HEAD에서 exit `0`이면 완료. 정책 밖 갭, FAIL, red, 한도,
`deadline-wait`, `no-progress`, 권한 거부에서는 멈추고 `goal-evidence:`의 `stop=`으로 보고"다. goal은 승인을 만들지 않는다.

## 3. 시험

- `DesktopNode.Verification.Tests`에 fixture 시험을 둔다. 조건마다 충족과 미충족 갭 종류, exit code, 입력 오류(exit `2`)를 본다.
- 저장소 실제 파일은 구조 계약(criteria, backlog, 정책의 계약 이름과 필수 필드, C4 항목 `15`개, evidence 파일 존재)만 본다. 저장소
  상태가 바뀌면 깨지는 판정 결과 snapshot 시험은 두지 않는다. 실제 판정 결과는 실행 기록에 적는다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다. Required CI shard 배정은 기존 Verification 시험과 같다.

## 4. 파일

| 파일 | 역할 |
| --- | --- |
| `config/project-completion-criteria.json` | 판정 출처, C4 항목, 기한 위험, 영구 범위 밖 |
| `docs/ga-ready/backlog.json` | `report-only` 발견과 분류 |
| `config/completion-autopilot-policy.json` | 갭 종류별 자동 권한(승인 4) |
| `src/DesktopNode.Verification` `completion` 명령 | 판정 |
| `docs/DEVELOPMENT_PROCEDURE.md` §1.4, §9 | 판정 명령과 연쇄 규칙 줄 |
| private `.claude/skills/pcv-campaign`, `pcv-campaign-open`, `pcv-goal` | 러너, completion 모드, goal 템플릿 |

## 5. 비목표

- 판정 명령은 완료를 선언하지 않는다. 완료 기록은 exit `0` 결과를 인용한 감사 문서다.
- Required CI blocking gate로 넣지 않는다. train과 train 사이에는 C2가 정상적으로 미충족이다.
- 정책 밖 권한, 회로 차단기 한도 변경, 실패 checkpoint 자동 되돌리기.
- public trusted signing과 external stable publication을 주장하지 않는다.
