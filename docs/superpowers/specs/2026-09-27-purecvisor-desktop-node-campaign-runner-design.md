# Desktop Node 캠페인 러너 (Task 큐 연속 실행)

- Design-ID: `pcv-campaign-runner-v1`
- 작성일: `2026-09-27`
- 문서 상태: `implemented-in-docs`
- 선행 설계: `pcv-campaign-continuity-procedure-v1` (`docs/superpowers/specs/2026-09-20-purecvisor-desktop-node-campaign-continuity-design.md`)
- 제품 payload 변경: `false`
- host/VM/service/package mutation: `false`
- public trusted signing: `false`
- external stable publication: `false`

> 실행 라우터는 계속 `docs/DEVELOPMENT_PROCEDURE.md`이고, 한도와 종료 동작의 단일 진실은
> `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`다. 이 문서는 열린 campaign이 **여러 checkpoint를
> 사람 개입 없이 이어 가는 방법**만 정한다.

## 1. 문제

campaign continuity v1은 재승인을 없앴지만 실제 실행은 여전히 checkpoint마다 멈춘다.

1. `next_step`이 문자열 하나라서, 한 일이 끝나면 다음 일을 사람에게 다시 묻는다.
2. commit이 `next_step`에 적혀 있지 않으면 task마다 commit 승인에서 멈춘다.
3. 세션 지침이 campaign 규칙을 모르면 `계속`을 `one-bounded-checkpoint`로 읽는다.
4. checkpoint가 green으로 끝나도 에이전트가 턴을 끝내고 다음 지시를 기다린다.

2026-09-27 모듈 크기 라쳇 복구 계획(task `7`개)이 이 네 가지에 모두 걸린다.

## 2. 결정

### 2.1 campaign 필드 추가

`docs/ga-ready/active-campaign.json`에 다음 필드를 더한다. `contract`는
`pcv-active-campaign-v1`을 유지하며 기존 필드의 의미는 바꾸지 않는다.

| 필드 | 의미 |
| --- | --- |
| `plan` | task heading(`## Task N:`)과 checkbox를 가진 plan 문서 경로 |
| `task_queue` | 실행 순서대로 나열한 task id 배열 |
| `next_task` | 다음에 실행할 task id. 완료할 때마다 전진한다 |
| `completed_tasks` | 완료한 task id 배열. commit은 자기 SHA를 담을 수 없으므로 SHA는 git log가 소유하고 commit 제목에 task id를 넣는다 |
| `commit_policy` | `local-commit-per-task` 또는 `ask-per-commit` |
| `push_allowed` | `false`. push/PR은 항상 별도 승인이다 |
| `checkpoint_limit` | 한 번의 run에서 연쇄할 checkpoint 상한 |
| `approval_locator` | `commit_policy`를 준 사용자 승인 근거 |

`commit_policy=local-commit-per-task`는 AGENT_EXECUTION_CIRCUIT_BREAKER의 "commit은 campaign이
`next_step`으로 적었을 때 연다" 규칙을 task 단위로 적은 것이다. 새 승인을 만들지 않는다.
`approval_locator`가 없거나 비어 있으면 `ask-per-commit`으로 취급한다.

### 2.2 러너 루프

task 하나가 Lane 1 checkpoint 하나다. 한 턴 안에서 다음을 반복한다.

1. **Lane 0 읽기.** `git status --short`, `git rev-parse HEAD`, campaign `status`와
   `next_task`, plan의 해당 task 절을 읽는다.
2. **시작 계약 공개.** `checkpoint k/checkpoint_limit`, task id, 허용 파일(plan task의
   수정/생성 목록), 완료 검증 명령, 회로 차단기 Lane 1 예산을 공개한다.
3. **구현과 focused 검증.** plan task의 검증 명령을 실행한다.
4. **기록.** green이면 plan checkbox를 체크하고, campaign의 `next_task`를 다음 id로 옮기고,
   `completed_tasks`에 추가한다. 이 두 파일 변경은 task 변경과 같은 commit에 넣는다.
5. **commit.** `commit_policy=local-commit-per-task`면 로컬 commit 하나를 만든다.
   `ask-per-commit`이면 여기서 멈추고 승인을 묻는다.
6. **진행 보고.** 1~3줄(task id, 결과, commit SHA)만 쓰고 턴을 끝내지 않는다.
7. **다음 checkpoint.** 새 시작 계약으로 1번으로 돌아간다. checkpoint마다 예산이 새로
   시작하며, 이것은 같은 checkpoint의 예산 소급 확장이 아니다.

### 2.3 종료 조건

다음 중 하나가 생기면 현재 상태를 보존하고 최종 보고를 한 뒤 턴을 끝낸다.

| 조건 | 동작 |
| --- | --- |
| `task_queue` 소진 | `next_step`을 완료 상태로 갱신하고 같은 commit에 넣는다 |
| `checkpoint_limit` 도달 | `next_task`를 남긴 채 멈춘다 |
| 회로 차단기 한도(시간, batch, same-failure `3`) | 해당 checkpoint 변경은 commit하지 않는다 |
| focused 검증 red가 checkpoint 안에서 해결되지 않음 | commit하지 않고 working tree 상태를 보고한다 |
| 승인 밖 작업 필요(push/PR, host mutation, current-evidence, Lane 2/3) | 멈추고 `next_approval_required`를 보고한다 |
| `new-design-required`, `Add-Type`/`P/Invoke`, installer handoff | 멈추고 보고한다 |
| 권한 거부(auto mode classifier 포함) | 우회하지 않고 멈춘다 |
| run 시작 시 working tree가 dirty | 시작하지 않고 보고한다 |

범위 밖 발견은 종료 조건이 아니다. `report-only`로 최종 보고에 모으고 다음 task를 계속한다.

### 2.4 세션 연결

- 에이전트 세션 지침(`CLAUDE.md` 등)은 열린 campaign이 있을 때 `재개`/`계속`/`다음 단계`를
  이 러너로 보낸다.
- Claude Code는 `pcv-campaign` skill이 이 루프를 실행한다. 여러 턴에 걸친 무인 실행이
  필요하면 `/loop /pcv-campaign`으로 턴마다 run을 다시 시작할 수 있다. campaign 파일이
  상태를 소유하므로 컨텍스트 요약이나 새 세션에서도 `next_task`부터 이어진다.

### 2.5 최종 보고

- 완료 task 목록과 commit SHA
- 종료 조건과 `next_task`
- `report-only` 발견 목록
- `lane=`, `working_authority=`, `current_evidence_written=false`

## 3. 유지하는 금지

- campaign은 없는 승인을 만들지 않는다. push/PR, host mutation, current-evidence 쓰기는
  기존 승인 표를 따른다.
- 한 checkpoint는 한 차선만 소유한다. 러너는 Lane 0/1만 연쇄한다.
- 회로 차단기 한도 변경 금지, 같은 checkpoint 예산 소급 확장 금지.
- FAIL은 current를 쓰지 못한다.

## 4. 파일

| 파일 | 역할 |
| --- | --- |
| `docs/ga-ready/active-campaign.json` | campaign 상태와 task 큐 |
| `docs/DEVELOPMENT_PROCEDURE.md` §1.4 | 실행 라우터에서 러너로 연결 |
| `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` | checkpoint 연쇄 규칙 |
| plan 문서(`plan` 필드) | task 정의, 허용 파일, 검증 명령, checkbox |

## 5. 비목표

- push/PR 자동화
- Lane 2/3 자동 연쇄
- 회로 차단기 한도 변경
- 실패 checkpoint의 자동 되돌리기
