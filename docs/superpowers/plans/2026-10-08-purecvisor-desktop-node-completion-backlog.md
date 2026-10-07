# 완료 판정 backlog 일곱 건 처리 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 완료 판정 C7의 backlog `BL-0001`~`0007`(2026-10-08 `counts`로 분류)을 닫는다. 도구·시험 결함 네 건, 단일 PR train 설계 개정 한 건, 제품 결함 두 건이다. 제품 변경은 다음 release train 대기열에 올리고, 판정 연쇄로 train campaign을 연다. 2026-10-19 뒤 C5 확인과 완료 판정은 이관 task로 남긴다.

**Architecture:** 완료 정의 v2, 설계 `pcv-completion-autopilot-v1`. PR은 셋이다. PR 1(Task 1~5, 도구·시험)은 `lane1/completion-backlog-20261008`, PR 2(Task 6~8, 단일 PR train 개정)와 PR 3(Task 9~12, 제품 변경과 대기열 행)은 직전 PR merge 뒤 `origin/main`에서 만든 branch다. Task 13은 판정과 연쇄, Task 14·15는 `completion-20261007` Task 10·11 이관이다.

**Tech Stack:** C# xUnit(`DesktopNode.Verification`, `DesktopNode.Host.Tests`, `DesktopNode.HyperV`, `DesktopNode.Api`), 문서(`docs/superpowers/specs/`, `docs/DEVELOPMENT_PROCEDURE.md`), `pcvverify completion`

## 사용자 결정 (2026-10-08)

승인 원문: `1,2,3,4,5,6,7` (`completion-20261007` 판정 기록의 `next_approval_required` 일곱 건에 대한 답)

| 항목 | 범위 |
| --- | --- |
| 1 | `BL-0001` counts, 새 설계 승인: `vm.create` reconcile 지문에 디스크·ISO·switch 연결을 넣는 설계와 구현. Lane 1, push, PR, green CI 뒤 merge, product payload면 다음 train queue 행, host mutation 없음 |
| 2 | `BL-0002` counts: QoS readback `mutation_supported: false` 원인 조사와 수정. Lane 1, push, PR, green CI 뒤 merge, 다음 train queue 행, host mutation 없음 |
| 3 | `BL-0003` counts: `VerificationExecutorTests` 타이밍 시험 안정화. Lane 1, push, PR, green CI 뒤 merge |
| 4 | `BL-0004` counts: `train-facts` fullgate와 `train-host-inputs` current-card 순환 제거. Lane 1, push, PR, green CI 뒤 merge |
| 5 | `BL-0005` counts, 새 설계 승인: 단일 PR train path check가 Lane 3 pin을 허용하도록 `pcv-single-pr-train` 개정과 구현. Lane 1, push, PR, green CI 뒤 merge |
| 6 | `BL-0006` counts: `pcvverify completion` C2가 operational 뒤 running train을 미충족으로 보도록 수정. Lane 1, push, PR, green CI 뒤 merge |
| 7 | `BL-0007` counts: `DesktopNodeHttpTransportContractTests` raw HTTP 시한 안정화. Lane 1, push, PR, green CI 뒤 merge |
| 권한 | Lane 0/1, task마다 로컬 commit, push, PR, green CI 뒤 merge. host mutation과 Lane 3 쓰기는 없다. Task 13의 연쇄로 열리는 train campaign은 정책 `config/completion-autopilot-policy.json`(2026-10-07 승인 4, 2026-10-08 os-mutation 추가)을 따른다 |

ADR-0016 standing approval(`pcv-it-` 접두사 VM 생성·삭제)은 그대로 옮긴다. `BL-0001` 구현에서 실제 WMI 확인이 필요하면 그 단계만 쓴다.

## Global Constraints

- host, service, package, 보존 VM `pcv-guest-installed-04253-r1`을 바꾸지 않는다. operational current `0.42.92-admin-smoke`와 `current-evidence.json`을 바꾸지 않는다.
- 새 `Add-Type`/`P/Invoke`/native ACL/installer handoff가 필요해지면 멈춘다.
- product payload(`src/DesktopNode.HyperV`, `Api`, `Host`, `Runtime`, `Service`, `Cli`, `Contracts`, `web/src`)를 바꾸는 PR은 그 PR 안에서 `release-train.json` `queue`에 행을 더한다(§10).
- Web 문구·binding을 바꾸면 pin된 다섯 곳과 web Pester를 함께 바꾼다. 새 packaging `*.Tests.ps1`을 만들지 않는다.
- backlog 행을 닫을 때 `status=closed`, `closed_by`에 이 campaign task id를 적는다. 범위 밖 발견은 새 `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: BL-0006 판정기 C2

- [x] `ProjectCompletionEvaluator` C2가 `operational_current` 뒤의 train 중 `status≠promoted`인 것이 있으면 미충족과 `train-departure` 갭(`C2-train-running`, 그 version)을 내게 한다. fixture 시험을 더한다. 검증: Verification.Tests, `git diff --check`. `BL-0006` 닫기. 로컬 commit.


실행 기록(2026-10-08): `ProjectCompletionEvaluator` C2가 `operational_current` train 뒤에 `status≠promoted`인 train을 찾아 미충족과 `train-departure` 갭 `C2-train-running-<version>`(lane `2`, refs `train:<version>`)을 내고, detail에 `unfinished_trains=<n>`을 더했다. 시험 `TrainDepartedAfterTheOperationalOneKeepsC2Open`(`running`, `stopped`) `2`개, completion 시험 `27/27`. `BL-0006` 닫음.

## Task 2: BL-0004 train 도구 순환

- [x] `TrainFactsBuilder` fullgate의 current-card 검사(설치 hash, product version)를 current-card 문서 쪽으로 옮겨 fullgate facts가 current-card 없이 렌더되게 한다. 렌더 값과 golden은 바뀌지 않아야 한다. `DEVELOPMENT_PROCEDURE.md` §10의 순서 문장을 맞춘다. 검증: Verification.Tests(TrainEvidenceGolden 포함), Delivery, `git diff --check`. `BL-0004` 닫기. 로컬 commit.


실행 기록(2026-10-08): `TrainFactsBuilder` fullgate에서 current-card `summary.json` 검사(설치 Host/CLI hash, product version)를 빼고 current-card 문서로 옮겼다. current-card는 fullgate batch id의 route artifact provenance를 직접 읽어 같은 검사를 한다(실패 이유 `current-card:installed-hashes`, `current-card:installed-product-version`). 렌더 값은 바뀌지 않아 golden이 그대로다. 시험 `FullgateRendersBeforeTheCurrentCardExists`, `CurrentCardRejectsAnInstalledBuildOtherThanTheFullgateBuild`(2) `3`개, train 도구 시험 `44/44`. `DEVELOPMENT_PROCEDURE.md` §10에 복사 이름 `capture-current-card.ps1`, 렌더 순서, supervisor 결과 파일 경로를 적었다. `BL-0004` 닫음.

## Task 3: BL-0003 타이밍 시험

- [ ] `VerificationExecutorTests.NonCooperativeManagedRunnerCannotBlockPerSuiteDeadline`의 hang guard(`WaitAsync` 3초)를 runner 속도에 기대지 않게 고친다(검증 의미는 유지). 검증: Verification.Tests. `BL-0003` 닫기. 로컬 commit.

## Task 4: BL-0007 Host transport 시험

- [ ] `DesktopNodeHttpTransportContractTests.RawTargetProjectionAndRepresentative404And405ResponsesMatchFixture`의 raw HTTP 시한 초과 원인을 확인하고 시한·준비 대기를 runner 속도에 기대지 않게 고친다. 검증: Host.Tests, `git diff --check`. `BL-0007` 닫기. 로컬 commit.

## Task 5: 종료 검증과 merge (PR 1)

- [ ] clean HEAD 종료 검증(solution), push, PR, green CI 뒤 merge. 도구·시험 PR이라 queue 행은 없다.

## Task 6: BL-0005 단일 PR train 설계 개정

- [ ] `pcv-single-pr-train-v1`을 대체하는 개정 설계를 쓴다. Lane 3가 바꾸는 pin(config spec 3개, Delivery verifier spec SHA 상수 3개)을 path check 허용 목록으로 두고, `CurrentEvidenceVerifierTests`는 `current-evidence.json`에서 version을 읽어 Lane 3가 고치지 않게 한다. 허용 목록 밖 경로는 지금처럼 멈춘다. 검증: `git diff --check`, Delivery 문서 시험. 로컬 commit.

## Task 7: BL-0005 구현

- [ ] `train-path-check`에 Lane 3 pin 허용 목록(결과 계약에 허용된 경로를 따로 보고)과 시험, `CurrentEvidenceVerifierTests` 수정, `DEVELOPMENT_PROCEDURE.md` §10, private `pcv-ship`·`pcv-campaign-open` train 문장(필요하면). 검증: Verification.Tests, Delivery, `git diff --check`. `BL-0005` 닫기. 로컬 commit.

## Task 8: 종료 검증과 merge (PR 2)

- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Task 9: BL-0002 QoS readback

- [ ] `vm.blkio-get`·`vm.bandwidth` readback의 `mutation_supported`가 상수 `false`인데 제품에 `vm.blkio-set`·`vm.bandwidth-set` mutation이 있는 원인을 확인하고, readback이 실제 mutation 지원을 말하도록 고친다. CLI/Web/parity fixture와 문서의 영향 확인. product payload이므로 `queue` 행(Lane 2 probe 기능군 `vm.qos`)을 같은 PR에 더한다. 검증: HyperV.Tests, Api.Tests, 영향 범위의 web 시험, `git diff --check`. `BL-0002` 닫기. 로컬 commit.

## Task 10: BL-0001 설계

- [ ] `vm.create` reconcile 지문(`CreateFingerprintMatches`)에 디스크(`disk0.vhdx` 연결), ISO, switch 연결을 넣어 `DefineSystem` 뒤 장치 연결 전에 끊긴 create를 `postcondition-confirmed`로 판정하지 않게 하는 설계를 쓴다(판정 표, readback 필드, 시험, 다음 train Lane 2 probe). 검증: `git diff --check`. 로컬 commit.

## Task 11: BL-0001 구현

- [ ] 설계대로 Api reconcile handler와 필요한 readback을 고치고 시험을 더한다. product payload이므로 `queue` 행(Lane 2 probe 기능군 `vm.create` reconcile)을 더한다. 검증: Api.Tests, HyperV.Tests, `git diff --check`. `BL-0001` 닫기. 로컬 commit.

## Task 12: 종료 검증과 merge (PR 3)

- [ ] clean HEAD 종료 검증(solution), push, PR, green CI 뒤 merge.

## Task 13: 판정과 연쇄

- [ ] clean `main`에서 `pcvverify completion`을 돌려 기록한다. 기대 갭은 C2 `train-departure`(queue 행 두 개, `0.42.93-admin-smoke`)와 C5 `deadline-wait`다. 기록 PR을 merge한 뒤 `pcv-campaign` §6대로 정책 안 갭으로 completion 모드 campaign을 열고 Task 14·15를 carry-over한다.

## Task 14: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `completion-20261007` Task 10이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Task 15: 완료 판정

- [ ] `not_before` 2026-10-19. 이관 전 `completion-20261007` Task 11이다. clean `main`에서 `pcvverify completion`을 돌린다. exit `0`이면 결과를 인용한 감사 문서로 완료를 적고 push, PR, green CI 뒤 merge한다. exit `1`이면 결과를 기록하고 `pcv-campaign` §6 연쇄로 넘긴다.

## Nonclaims

- operational current는 `0.42.92-admin-smoke` 그대로다. 제품 변경은 다음 train에서 실제 설치본으로 확인한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
