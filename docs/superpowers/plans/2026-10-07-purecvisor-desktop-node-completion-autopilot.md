# 완료 기준 자동 판정과 작업 등록 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 프로젝트 완료 정의(`pcv-project-completion-definition-v1`)를 기계가 판정하게 하고, 판정이 낸 갭을 campaign task로 자동 등록해 사용자 승인 정책 안에서 연쇄 실행한다. 2026-10-07 PR #59가 C2를 다시 깨뜨렸는데 아무것도 잡지 못했고, `report-only` 발견이 저장되지 않으며, 기한 대기 task 하나가 큐 전체를 막는 문제를 닫는다.

**Architecture:** 완료 정의 v2 결정(C7 backlog 추가, 기계 판정 출처 `config/project-completion-criteria.json`, 감사 §4 위험 목록 이관), 설계 `pcv-completion-autopilot-v1`(판정 명령 `pcvverify completion`, backlog, autopilot 정책, runner v2 추가 필드). branch는 셋이다. Task 1~6은 `lane1/completion-autopilot-20261007`, Task 7~8은 그 PR이 merge된 뒤 `origin/main`에서 만든 `lane1/completion-autopilot-first-run-20261007`, Task 9는 2026-10-19 뒤 별도 branch다. Task 7의 skill 변경은 private 저장소 로컬 commit이다.

**Tech Stack:** C# xUnit(`DesktopNode.Verification`, `DesktopNode.Verification.Tests`), `gh run list`, 문서(`docs/superpowers/specs/`, `docs/DEVELOPMENT_PROCEDURE.md`), private `.claude/skills/pcv-campaign`, `pcv-campaign-open`, `pcv-goal`

## 사용자 결정 (2026-10-07)

승인 원문: `1,2,3,4,5` (`process-followups-20261007` 진행 중 "100퍼센트 완료 기준을 근거로 모든 개발 작업을 자동으로 등록 처리하는 방법" 제안의 다음 승인 후보에 대한 답). 4번은 Lane 2/3 범위가 두 가지로 읽혀 다시 물었고, 답은 `Lane 2/3 모두 자동`이다.

| 항목 | 범위 |
| --- | --- |
| 1 | Lane 0/1. 완료 정의 개정 결정 문서(C7 backlog, criteria JSON, 위험 목록 이관)와 `pcvverify completion` 설계. push, PR, green CI 뒤 merge |
| 2 | Lane 1. criteria JSON, backlog(현재 `report-only` 2건 등록), `pcvverify completion`과 시험 구현. push, PR, green CI 뒤 merge |
| 3 | private 저장소 skill `pcv-campaign`, `pcv-campaign-open`, `pcv-goal`에 completion 모드, `not_before`, backlog 쓰기를 넣는다. 로컬 commit만 |
| 4 | `completion-autopilot-policy` 승인, Lane 2/3 모두 자동. 정책으로 생성되는 campaign은 train 출발 표준 범위(MSI 설치·repair·제거·`REMOVE_DATA`, service, pair 여섯 bucket, fullgate route parity VM, probe VM 생성·삭제), Lane 3 `current-evidence.json` 쓰기, push, PR, green CI 뒤 merge를 묻지 않고 연다. FAIL(정차), `new-design`, backlog `undecided` 행, 새 `Add-Type`/`P/Invoke`/native ACL/installer handoff, 영구 범위 밖 항목에서는 멈추고 묻는다 |
| 5 | `process-followups-20261007`을 이 campaign으로 교체하고 그 Task 13(C5 runner 확인)을 `not_before` 2026-10-19 task(Task 9)로 옮긴다 |
| 권한 | 이 campaign: Lane 0/1, task마다 로컬 commit, push, PR, green CI 뒤 merge. host mutation과 Lane 3 쓰기는 없다. 4번의 Lane 2/3 자동 범위는 이 campaign이 만든 정책으로 열리는 다음 campaign에만 적용한다 |

ADR-0016 standing approval(`pcv-it-` 접두사 VM 생성·삭제)은 철회 지시가 없었으므로 `approval_locator`에 그대로 옮긴다. 이 campaign의 task는 그 단계를 쓰지 않는다.

## Global Constraints

- 이 campaign은 host, VM, service, package를 바꾸지 않는다. 보존 VM `pcv-guest-installed-04253-r1`을 건드리지 않는다.
- operational current(`0.42.91-admin-smoke`), `current-evidence.json`, `release-train.json` `queue`와 `trains`를 바꾸지 않는다. `pcvverify completion`은 이 파일들을 읽기만 한다.
- 판정기와 정책 파일은 완료를 주장하지 않는다. 프로젝트 완료 판정은 `pcvverify completion` exit `0`일 때만 적는다.
- 완료 정의 v1, 감사 문서, 기존 evidence는 덮어쓰지 않는다. v2는 새 문서이고 index가 v2를 가리킨다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다. 시험은 C# `DesktopNode.Verification.Tests`에 둔다. Web은 바꾸지 않는다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. `gh run list` 출력은 저장소 `artifacts/` 아래 입력 파일로만 쓴다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 완료 정의 v2 결정

- [x] `docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-definition-v2-design.md`(`pcv-project-completion-definition-v2`)를 쓴다. C1~C6을 유지하고, C7(backlog의 `counts`·`undecided` 행 `0`)을 더하며, 기계 판정 출처를 `config/project-completion-criteria.json`으로 정하고, 감사 2026-10-06 §4 위험 목록을 그 파일로 옮긴다는 결정과 2026-10-07 판정(C2 재미충족: queue PR #59)을 적는다. v1 문서 상단에 v2 대체 줄만 더하고 `DOCUMENTATION_INDEX.md`에 v2를 건다. 검증: `git diff --check`, Delivery 문서 시험(`EfficientDevelopmentProcedureDocumentationTests` 등 index를 읽는 시험). 로컬 commit.

실행 기록(2026-10-07): v2 결정 문서를 새로 썼다. C1~C6 뜻은 v1 그대로이고 C7(backlog `status=open`이면서 `counts`·`undecided`인 행 `0`)을 더했다. 기계 판정 출처는 `config/project-completion-criteria.json`(`pcv-project-completion-criteria-v1`), backlog 계약은 `pcv-backlog-v1`, 완료는 `pcvverify completion` exit `0`인 `main` HEAD에서만 적는다. 감사 2026-10-06 §4 위험 행은 criteria `deadline_risks`로 옮기고 감사는 고치지 않았다. 2026-10-07 판정(사람 대조, `main` `3f55831`): C1·C3·C4·C6 충족, C2(queue PR #59)·C5(기한 대기)·C7(backlog `undecided` 2행) 미충족, `4/7`. v1에는 상단 대체 줄만 더했고 `DOCUMENTATION_INDEX.md`·`DEVELOPER_INDEX.md`가 v2를 가리킨다. 검증: `git diff --check` 통과, Delivery `771/771`.

## Task 2: autopilot 설계

- [x] `docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-autopilot-design.md`(`pcv-completion-autopilot-v1`)를 쓴다. 판정 명령 계약(`pcv-project-completion-result-v1`, 입력, 조건별 판정, 갭 종류 `train-departure`·`lane2-probe`·`lane1-fix`·`new-design`·`deadline-wait`·`user-decision`, exit `0`/`1`/`2`), backlog 계약(`pcv-backlog-v1`, 분류 `counts`·`out-of-scope`·`undecided`), 정책 계약(`pcv-completion-autopilot-policy-v1`, 승인 4 범위), 갭 → task 템플릿과 순서, runner v2 추가 필드(`task_not_before`, 닫을 때 판정과 연쇄, 미래 `not_before` task의 carry-over), runner v2 §3·§5와의 관계(정책은 사용자 승인 문장이고 생성 campaign의 `approval_locator`가 그 승인을 인용한다)를 적는다. 검증: `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): 설계 `pcv-completion-autopilot-v1`을 썼다. 판정 명령은 `completion --ci-runs <gh run list JSON> [--head] [--today] [--output]`, 네트워크 없이 읽기 전용, exit `0`/`1`/`2`. 갭 종류는 plan의 여섯에 `ci-wait`(head의 run이 없거나 진행 중)를 더해 일곱이다. C3은 ledger `candidate_required` feature의 `current.verdict=pass`와 `promotion_eligible`, C4는 `docs/ga-ready/evidence/<id>.md` 존재 또는 `waiver` 문서로 판정한다. 정책 표(승인 4): `lane1-fix`·`deadline-wait`·`ci-wait`·`train-departure`(Lane 0~3, train 표준 범위)·`lane2-probe`(probe VM, service)는 자동, `new-design`·`user-decision`은 멈춤. campaign 필드 `task_not_before`·`carried_tasks`·`generated_from`, 정지 이유 `deadline-wait`·`no-progress`를 더했고 이 campaign `stop_on`에도 반영했다. plan Task 5의 "현재 저장소 입력으로 기대 갭을 내는 시험"은 저장소 상태가 바뀌면 깨지므로 설계 §3에서 구조 계약 시험과 fixture 시험으로 바꾸고, 실제 판정 결과는 Task 8 실행 기록에 적는다. 검증: `git diff --check`, Delivery 시험.

## Task 3: 기준·backlog·정책 파일

- [ ] `config/project-completion-criteria.json`(C1~C7 판정 출처, C4 `15`개 항목의 evidence id 또는 면제 결정, C5 위험 목록), `docs/ga-ready/backlog.json`(현재 `report-only` 2건: `vm.create` reconcile 지문의 장치 연결 누락, QoS readback `mutation_supported: false`, 둘 다 `undecided`), `config/completion-autopilot-policy.json`(승인 4)을 만들고 구조 계약 시험을 `DesktopNode.Verification.Tests`에 더한다. 검증: Verification.Tests, Delivery.Tests, `git diff --check`. 로컬 commit.

## Task 4: 판정기 1 (결과 계약, C2·C5·C6·C7)

- [ ] `pcvverify completion`을 더한다. 결과 계약, CI run 입력 파일(`gh run list --json` 출력), C2(`current-evidence.json`과 `release-train.json`), C5(main HEAD Required CI와 위험 목록 기한), C6, C7(backlog), 갭 목록과 exit code. fixture 시험을 더한다. 검증: Verification.Tests, `git diff --check`. 로컬 commit.

## Task 5: 판정기 2 (C1·C3·C4, 저장소 판정)

- [ ] C1(Required CI `dotnet`·`delivery` shard), C3(feature evidence ledger 후보 stage), C4(criteria 항목의 evidence id 해석과 면제 결정 문서 존재)를 더하고, 현재 저장소 입력으로 기대 갭(C2 `train-departure`, C5 `deadline-wait` 2026-10-19, C7 `user-decision` 2건)을 내는 시험을 둔다. `DEVELOPMENT_PROCEDURE.md` §1.4·§9에 판정 명령 줄을 더한다. 검증: Verification.Tests, Delivery.Tests, `git diff --check`. 로컬 commit.

## Task 6: 종료 검증과 merge (1차)

- [ ] clean HEAD 종료 검증(`pcv-ship`), push, PR, green CI 뒤 merge, 로컬 `main` 동기화. tooling·docs PR이므로 `release-train.json` `queue` 행은 더하지 않는다. merge commit은 Task 8 기록에 적는다.

## Task 7: private skill 갱신

- [ ] private 저장소 `.claude/skills/pcv-campaign`(`task_not_before`, backlog 쓰기, 닫을 때 `pcvverify completion`과 정책 안 연쇄, carry-over), `pcv-campaign-open`(completion 모드: 갭 → task 템플릿, 정책 승인 인용), `pcv-goal`(완료 goal 템플릿: `pcvverify completion` exit `0` 또는 정지 절)을 설계대로 고친다. 검증: private `git diff --check`, skill 본문이 설계의 필드 이름과 같은지 Grep. private 로컬 commit, public은 plan checkbox와 campaign 전진만 로컬 commit.

## Task 8: 첫 판정과 연쇄

- [ ] clean `main` HEAD에서 `gh run list` 입력으로 `pcvverify completion`을 돌려 결과(조건별 판정, 갭, exit)를 기록하고 push, PR, green CI 뒤 merge한다. 그 뒤 남은 큐가 미래 `not_before` task뿐이면 설계의 carry-over로 이 campaign을 닫고, 정책 안 갭(train `0.42.92` 등)은 `pcv-campaign-open` completion 모드로 다음 campaign을 연다. `user-decision`·`new-design` 갭은 `next_approval_required`에 번호로 남긴다.

## Task 9: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `process-followups-20261007` Task 13이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 criteria 위험 행을 닫고 C5 충족을 기록한다. 프로젝트 완료 판정은 `pcvverify completion` exit `0`일 때만 적는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.91-admin-smoke` 그대로다. 이 campaign에는 Lane 3와 `current-evidence.json` 쓰기가 없다.
- 판정기 결과는 관측이다. 완료 정의를 대신하지 않고, v2 결정 문서가 기준이다.
- 정책은 승인 4가 명시한 범위만 연다. 정책이 없는 승인을 만들지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
