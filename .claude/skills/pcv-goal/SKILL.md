---
name: pcv-goal
description: PureCVisor 작업에 쓸 Claude Code `/goal` 조건을 만든다. 열린 campaign, release train, 단일 task, PR merge 중 대상을 골라 500자 안의 조건 문장(완료 절 + 정당한 정지 절)을 만들고 사용자가 붙여 넣을 `/goal ...` 한 줄로 보여 준다. 사용자가 "goal 설정", "goal 만들어", /pcv-goal을 부르면 사용한다. goal은 승인을 만들지 않는다.
---

# PCV goal

`/goal <condition>`은 세션 goal을 건다. 턴이 끝날 때마다 별도 평가기가 **대화 내용만 보고** 조건이 충족됐는지
판정하고, 충족 전에는 Claude가 계속 일한다. `/goal clear`로 지운다.

- 조건은 `500`자 이하. 신뢰한 workspace와 hook 허용(`disableAllHooks` 아님)이 필요하다.
- 평가기는 명령을 실행하지 않는다. 대화에 적힌 사실만 본다. 확신이 없으면 미충족으로 본다.
- 평가기는 조건이 진짜로 달성 불가능할 때만 `impossible`로 끝낸다. 에이전트가 "불가능"이라고 말하는 것만으로는 끝나지 않는다.

`$pub` = 이 저장소 루트(`purecvisor-desktop-node-public`). shell cwd가 곧 `$pub`이다.

## 0. 원칙

1. goal은 승인을 만들지 않는다. 권한은 사용자 메시지와 campaign 필드(`allowed_lanes`, `mutation_allowed`,
   `current_write_allowed`, `push_allowed`, `merge_policy`)만 준다.
2. 조건에는 반드시 **정당한 정지 절**을 넣는다. 회로 차단기 한도, 같은 원인 3회, verification-red, Lane 2 FAIL,
   CI fail, 승인 밖 작업, `new-design-required`, 권한 거부에서 멈추고 원인과 필요한 승인을 보고했으면 충족이다.
   이 절이 없으면 평가기가 계속을 강제하고, 그 압력은 정지 규칙을 넘는 쪽으로 작용한다.
3. 대상을 id로 고정한다(campaign id, train version, task id, PR 번호). "백로그 전부", "품질 개선" 같은 열린 조건은 쓰지 않는다.
4. 평가기가 대화에서 확인할 수 있는 사실로 쓴다. campaign 필드 값, PR 상태, merge SHA, 검증 결과 줄이다.
   "30분 안에" 같은 시간 조건은 쓰지 않는다.
5. `500`자를 넘으면 정지 절은 그대로 두고 완료 절의 세부를 줄인다.

## 1. 대상 고르기 (Lane 0, 읽기만)

1. `git -C $pub status --short`, campaign(`id`, `status`, `intent`, `next_task`, `task_queue`, `allowed_lanes`,
   `push_allowed`, `merge_policy`), `release-train.json`의 `running` train, 열린 PR(`gh pr list --state open`)을 읽는다.
2. 상태에 맞는 틀을 고른다.

   | 상태 | 틀 |
   | --- | --- |
   | `status=open`이고 `next_task`가 있다 | A campaign 완주 |
   | 위와 같고 train campaign이다(`allowed_lanes`에 `"3"`, intent에 `train`) | C release train |
   | 사용자가 task 하나만 요청했다 | B 단일 task |
   | 승인된 PR merge만 남았다 | D PR merge |
   | 사용자가 프로젝트 완료까지 무인 실행을 원하고 `config/completion-autopilot-policy.json`이 있으며 `status`가 `paused`가 아니다 | E 프로젝트 완료 |
   | 열린 campaign이 없고 정책 파일도 없다 | goal을 만들지 않는다. 먼저 다음 승인을 받아 `pcv-campaign-open`으로 연다 |

   E는 원칙 3의 예외다. 대상은 `pcvverify completion` exit code와 정책 파일로 고정된다.

## 2. 틀

`<...>`를 실제 값으로 바꾼다.

A. campaign 완주

    public 저장소 campaign <ID>에 대해 다음 중 하나가 대화에서 확인된다. (1) active-campaign.json이 status=closed, next_task=null이고, 마지막 task가 PR을 적었다면 그 PR이 required check 모두 pass 뒤 merge되어 로컬 main이 그 merge commit이며 작업 트리가 clean이다. (2) 러너가 종료 조건(회로 차단기 한도, 같은 원인 3회, verification-red, Lane 2 FAIL, CI fail, 승인 밖 작업, new-design-required, 권한 거부)에서 멈추고 최종 보고에 원인, 남은 next_task, 필요한 승인을 적었다.

B. 단일 task

    public 저장소 campaign <ID>의 <TASK>가 completed_tasks에 들어간 로컬 commit SHA와 focused 검증 green이 대화에 보고되었다. 또는 그 task가 종료 조건(회로 차단기 한도, 같은 원인 3회, verification-red, 승인 밖 작업, new-design-required, 권한 거부)에서 멈추고 원인과 필요한 승인이 보고되었다.

C. release train

    release train <VERSION>이 release-train.json에서 status=promoted이고 current-evidence.json의 operational current가 <VERSION>이며, Lane 3 PR이 required check 모두 pass 뒤 merge되어 로컬 main이 그 merge commit이다. 또는 train이 FAIL·정차 조건이나 승인 밖 작업에서 멈추고 최종 보고에 정차 원인, 설치본 끝 상태, 필요한 승인을 적었다.

D. PR merge

    PR #<N>이 required check 다섯 개(dotnet, web, delivery, installer-policy, public-boundary-ci-required) 모두 pass 뒤 head <SHA>로 merge되어 로컬 main이 그 merge commit이다. 또는 check가 fail해 merge하지 않고 실패 check 이름과 run URL을 보고했다.

E. 프로젝트 완료 (pcv-completion-autopilot-v1)

    public 저장소에서 다음 중 하나가 대화에서 확인된다. (1) clean main HEAD에서 pcvverify completion이 complete=true met=7/7을 출력하고 exit 0이었으며, 그 결과를 인용한 감사 문서 PR이 merge되었다. (2) 러너가 정지 조건(회로 차단기 한도, 같은 원인 3회, verification-red, Lane 2 FAIL, CI fail, 정책 밖 갭 user-decision·new-design·scenario, autopilot-paused, deadline-wait, no-progress, 승인 밖 작업, 권한 거부)에서 멈추고 최종 보고에 원인, 남은 갭, 필요한 승인을 적었다.

## 3. 출력

- 고른 틀과 이유 한 줄, 조건 글자 수.
- 사용자가 붙여 넣을 한 줄: `/goal <조건>`(코드 블록).
- 끝나면 `/goal clear`로 지우거나 다음 campaign의 goal로 바꾼다는 안내 한 줄.

## 4. goal이 걸린 동안의 실행 규칙

- checkpoint 진행 보고와 최종 보고 끝에 `goal-evidence:` 줄을 쓴다(`pcv-campaign` §3, `pcv-ship` §4). 평가기는 이 줄을 근거로 판정한다.
- CI 대기는 background 대신 foreground `gh pr checks <n> --watch --interval 30`(timeout `600000`)로 한다.
  background로 턴을 끝내면 평가기가 미충족으로 보고 곧바로 다시 일을 시켜 polling이 된다.
- 정지 조건이면 우회하지 않는다. 최종 보고와 `goal-evidence:`에 `stop=<원인>`과 필요한 승인을 적으면 정지 절로 충족된다.
- 완료를 지어내지 않는다. `goal-evidence:`의 값은 직전에 실행한 명령 출력에서 옮긴다.
