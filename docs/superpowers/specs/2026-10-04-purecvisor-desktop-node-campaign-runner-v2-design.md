# Desktop Node 캠페인 러너 v2 (차선별 task와 PR 연쇄)

- Design-ID: `pcv-campaign-runner-v2`
- 작성일: `2026-10-04`
- 문서 상태: `implemented-in-docs`
- 선행 설계: `pcv-campaign-runner-v1` (`docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-campaign-runner-design.md`)
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`
- public trusted signing: `false`
- external stable publication: `false`

> 실행 라우터는 `docs/DEVELOPMENT_PROCEDURE.md`이고, 한도와 종료 동작의 단일 진실은
> `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`와 `config/agent-execution-circuit-breaker.json`이다.
> 이 문서는 v1 루프를 Lane 2/3 task와 push/PR/merge까지 넓히고, campaign을 닫고 여는 형식을
> 정한다. 여기서 바꾸지 않은 v1 규칙은 그대로다.

## 1. 문제

v1은 Lane 0/1 task만 연쇄하고 push/PR에서 멈춘다. 2026-10-04 release train 설계
(`docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-release-train-design.md` §4.7)는 출발
승인 하나가 package부터 Lane 3 merge까지 덮게 했고, 실제 campaign도 그렇게 열렸다.

| campaign | `allowed_lanes` | `mutation_allowed` | `current_write_allowed` | `push_allowed` |
| --- | --- | --- | --- | --- |
| `train-04289-20261004` | `0,1,2,3` | `true` | `true` | `true` |
| `train-pair-orchestrator-3b-20261004` | `0,1,2` | `true` | `false` | `true` |
| `train-pair-orchestrator-3b-r2-20261004` | `0,1,2` | `true` | `false` | `true` |

v1 문서는 `push_allowed`를 `false` 고정값으로 적고 Lane 2/3 연쇄를 비목표로 두었으므로 문서와
실행이 어긋난다. 그 밖에 세 가지가 빠져 있다.

1. task마다 차선이 기계적으로 적혀 있지 않다. Lane 2 task에 Lane 1 예산(30분, 18회)을 쓰기 쉽다.
2. merge 권한이 `approval_locator` 문장 안에만 있다. 러너가 문장을 해석해야 한다.
3. 큐가 끝나도 `status=open`, `next_task=null`로 남고, 다음에 받을 승인 목록의 형식이 없다.
   사용자가 번호로 답하면(예: `1,2,3`) 그 문장을 plan과 campaign으로 옮기는 절차가 문서에 없다.

## 2. 결정

### 2.1 campaign 필드

`contract`는 `pcv-active-campaign-v1`을 유지한다. 필드는 더하기만 하고, 없으면 기본값을 쓴다.

| 필드 | v2 의미 | 없을 때 |
| --- | --- | --- |
| `push_allowed` | `approval_locator`의 사용자 승인이 branch push와 PR을 명시할 때만 `true` | `false` |
| `merge_policy` | `none` 또는 `after-green-ci`. 승인이 merge를 명시할 때만 `after-green-ci` | `none` |
| `task_lanes` | task id → 차선(`"0"`~`"3"`) | 모든 task가 Lane 1 |
| `next_approval_required` | 큐가 끝나거나 멈췄을 때 다음에 받을 승인 문장 배열(번호 순) | 빈 목록 |

`allowed_lanes`, `mutation_allowed`, `current_write_allowed`, `commit_policy`, `checkpoint_limit`,
`approval_locator`의 의미는 continuity 설계와 v1 그대로다.

### 2.2 task 차선 검사

러너는 task를 열기 전에 차선 L을 정하고 다음을 확인한다. 하나라도 맞지 않으면
`approval-required-mutation`으로 멈춘다.

| 차선 | 조건 |
| --- | --- |
| 0, 1 | L이 `allowed_lanes`에 있다 |
| 2 | L이 `allowed_lanes`에 있고 `mutation_allowed=true`. host mutation은 plan `사용자 결정` 표의 범위 안 |
| 3 | L이 `allowed_lanes`에 있고 `current_write_allowed=true` |

checkpoint 예산은 `config/agent-execution-circuit-breaker.json`의 `lanes.<L>`이다. 시작 계약에 차선과
그 예산을 적는다. Lane 2 `overall_verdict=FAIL`이나 bucket FAIL은 focused 검증 red와 같게 다룬다.
current에 쓰지 않고 멈춘다(`DEVELOPMENT_PROCEDURE.md` §9).

### 2.3 push, PR, merge

plan task 본문이 push/PR/merge를 적은 경우에만 연다.

1. `push_allowed=true`가 아니면 멈춘다.
2. clean HEAD에서 plan의 종료 검증을 돈다. 작업 branch 이름은 `lane<L>/<주제>-<yyyymmdd>`다.
3. branch push, PR 생성. PR 본문은 요약, 검증, 경계(host mutation, `current-evidence.json` 쓰기 여부)를 적는다.
4. `merge_policy=after-green-ci`면 Required CI가 끝날 때까지 기다린다. 모두 pass면 확인한 head SHA로만
   merge하고 로컬 `main`을 fast-forward한다. 하나라도 fail이면 merge하지 않고 멈춘다.
5. `merge_policy=none`이면 PR을 연 채 두고 merge를 `next_approval_required`에 적는다. 다음 task가 그
   변경이 `main`에 있어야 하면 멈춘다.

CI 대기는 background로 두고 완료 알림을 받는다. 대기 자체는 tool batch 하나로 센다.

### 2.4 닫기

큐가 끝나면 마지막 task와 같은 commit에서 `status=closed`, `next_task=null`, `next_step`에 결과 요약,
`next_approval_required`에 다음 승인 문장을 쓴다. 최종 보고는 같은 목록을 번호로 보여 준다. 각 문장은
차선, host mutation 범위, push/PR/merge 여부를 하나씩 적는다.

### 2.5 열기

사용자가 다음 승인에 답하면(번호, release train 출발 문구, 또는 명시한 작업 묶음) 같은 턴에서 campaign을 연다.

1. 지금 campaign이 `status=open`이고 `next_task`가 남아 있으면 열지 않는다. 교체할지 사용자에게 묻는다.
2. 번호 답이면 직전 `next_approval_required`(또는 최종 보고)의 그 번호 문장을 승인 원문으로 쓴다.
3. plan 문서를 만든다. `## 사용자 결정 (<date>)`에 승인 원문과 번호별 범위 표, `## Global Constraints`에
   보존 대상·비밀 값 경계·차선별 한도, task마다 `## Task N: <제목>`과 checkbox 한 줄, `## Nonclaims`를 둔다.
   task 하나는 그 차선 checkpoint 하나에 들어가게 자른다.
4. campaign 파일을 새로 쓴다. 승인 문장에 없는 권한은 `false`, `none`, `["0","1"]`로 둔다. 승인을
   추정하지 않는다. 직전 id는 `closed_predecessor`에 적는다.
5. 새 campaign의 `commit_policy`로 `docs: open <campaign id>` commit을 만들고 같은 턴에서 러너를 시작한다.

### 2.6 최종 보고

v1 §2.5에 더해 다음을 적는다.

- task별 차선, PR 번호와 merge commit
- host mutation 요약(무엇을 바꿨고 끝 상태가 무엇인지)
- `next_approval_required` 번호 목록

## 3. 유지하는 금지

- campaign은 없는 승인을 만들지 않는다. 권한 필드는 `approval_locator`의 사용자 문장이 명시한 범위만 연다.
- 한 checkpoint는 한 차선만 소유한다. 차선을 바꾸면 새 시작 계약을 공개한다.
- 회로 차단기 한도 변경 금지, 같은 checkpoint 예산 소급 확장 금지.
- FAIL은 current를 쓰지 못한다. `current_evidence_written=true`는 Lane 3 task에서만 나온다.
- `new-design-required`, 새 `Add-Type`/`P/Invoke`/native ACL/installer handoff, 권한 거부에서 멈춘다.

## 4. 파일

| 파일 | 역할 |
| --- | --- |
| `docs/ga-ready/active-campaign.json` | campaign 상태, 권한, task 큐 |
| `docs/DEVELOPMENT_PROCEDURE.md` §1.4 | 실행 라우터에서 러너로 연결 |
| `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` | checkpoint 연쇄와 차선 예산 |
| plan 문서(`plan` 필드) | task, 허용 파일, 검증 명령, checkbox, 사용자 결정 표 |
| 에이전트 skill(이 저장소 밖) | Claude Code `pcv-campaign`(러너), `pcv-campaign-open`(열기), `pcv-ship`(push/PR/merge) |

## 5. 비목표

- 승인 문장의 자동 해석. 사람이 준 문장을 필드로 옮길 때 명시되지 않은 권한은 열지 않는다.
- 회로 차단기 한도 변경, 실패 checkpoint의 자동 되돌리기
- public trusted signing, external stable publication
