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

추가 승인(2026-10-08): `1` (Task 8 정지 보고의 1). main push `dotnet` job을 한 번 더 재실행하고 green이면 Task 9부터 계속한다. runner 시간 의존 시험 두 개를 `BL-0008`·`BL-0009`로 등록해 `counts`로 두고 Lane 1에서 안정화한다(Task 16·17, PR 3에 포함, push, PR, green CI 뒤 merge, host mutation 없음).

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

- [x] `VerificationExecutorTests.NonCooperativeManagedRunnerCannotBlockPerSuiteDeadline`의 hang guard(`WaitAsync` 3초)를 runner 속도에 기대지 않게 고친다(검증 의미는 유지). 검증: Verification.Tests. `BL-0003` 닫기. 로컬 commit.


실행 기록(2026-10-08): `NonCooperativeManagedRunnerCannotBlockPerSuiteDeadline`(3초)와 같은 위험의 옆 시험(4초, 비협조 process runner와 overall 시한)의 `WaitAsync` hang guard를 상수 `HangGuard` `30`초로 바꿨다. 가짜 runner는 `finally`에서만 풀리므로 executor가 막히면 여전히 guard에서 실패하고, 검증하는 suite 시한(1·2초)과 판정은 그대로다. `VerificationExecutorTests` `55/55`. `BL-0003` 닫음.

## Task 4: BL-0007 Host transport 시험

- [x] `DesktopNodeHttpTransportContractTests.RawTargetProjectionAndRepresentative404And405ResponsesMatchFixture`의 raw HTTP 시한 초과 원인을 확인하고 시한·준비 대기를 runner 속도에 기대지 않게 고친다. 검증: Host.Tests, `git diff --check`. `BL-0007` 닫기. 로컬 commit.


실행 기록(2026-10-08): `DesktopNodeHttpTransportContractTests`의 요청별 `CancellationTokenSource` 10초 두 곳(raw HTTP helper, noVNC handshake)을 hang guard 상수 `RawHttpHangGuard` `60`초로 바꿨다. 이 시험들은 응답 내용을 비교하고 지연 시간을 판정하지 않는다. PR #63 attempt 1의 실패는 host 시작 직후 첫 raw 요청이 runner에서 10초를 넘긴 것이다. Host.Tests `216/216`. `BL-0007` 닫음.

## Task 5: 종료 검증과 merge (PR 1)

- [x] clean HEAD 종료 검증(solution), push, PR, green CI 뒤 merge. 도구·시험 PR이라 queue 행은 없다.


실행 기록(2026-10-08): clean HEAD `cc59c8c`에서 `dotnet test src/DesktopNode.sln -c Release` 실패 `0`(Verification `660`, Delivery `775`, Api `490`, HyperV `271`, Host `216`, Contracts `200`, Cli `183`, Runtime `129`, Service `11`), `git diff --check origin/main...HEAD` 통과, product payload 경로 변경 `0`(queue 행 없음). 이 기록 commit 뒤 push, PR, green CI 뒤 merge한다. PR 번호와 merge commit은 Task 6 기록에 적는다.

## Task 6: BL-0005 단일 PR train 설계 개정

- [x] `pcv-single-pr-train-v1`을 대체하는 개정 설계를 쓴다. Lane 3가 바꾸는 pin(config spec 3개, Delivery verifier spec SHA 상수 3개)을 path check 허용 목록으로 두고, `CurrentEvidenceVerifierTests`는 `current-evidence.json`에서 version을 읽어 Lane 3가 고치지 않게 한다. 허용 목록 밖 경로는 지금처럼 멈춘다. 검증: `git diff --check`, Delivery 문서 시험. 로컬 commit.


실행 기록(2026-10-08): Task 5 PR 1 #66(head `9637db9`, Required check 다섯 개 pass)을 head 고정으로 merge했다(`a2bdd20`, main push Development Gates `37649278790` success). `origin/main`에서 `lane1/single-pr-train-lane3-pins-20261008`을 만들어 설계 `pcv-single-pr-train-v2`를 썼다. 0.42.91 PR #55와 0.42.92 PR #63의 pin 변경이 모두 64자리 SHA 한 줄씩임을 `git diff -U0`로 확인해, 허용 목록 여섯 파일에서 바뀐 줄이 모두 SHA 값 한 줄이고 `+`·`-` 수가 같을 때만 허용하는 규칙으로 정했다. `CurrentEvidenceVerifierTests`는 version을 `current-evidence.json`에서 읽는다. v1 문서에 개정 줄, `DOCUMENTATION_INDEX`에 v2 줄을 더했다.

## Task 7: BL-0005 구현

- [x] `train-path-check`에 Lane 3 pin 허용 목록(결과 계약에 허용된 경로를 따로 보고)과 시험, `CurrentEvidenceVerifierTests` 수정, `DEVELOPMENT_PROCEDURE.md` §10, private `pcv-ship`·`pcv-campaign-open` train 문장(필요하면). 검증: Verification.Tests, Delivery, `git diff --check`. `BL-0005` 닫기. 로컬 commit.


실행 기록(2026-10-08): `TrainPathCheckCommand`에 Lane 3 pin 허용 목록 여섯 파일과 `OnlyPinLinesChanged`(파일별 `git diff -U0`의 바뀐 줄이 모두 SHA-256 한 줄이고 `+`·`-` 수가 같음)를 더하고 결과에 `allowed_pin_paths`를 넣었다. `CurrentEvidenceVerifierTests`는 기대 version을 `current-evidence.json`에서 읽는다. 시험 `PathCheckAllowsLane3PinFilesThatOnlyReplaceShaValues`, `PathCheckKeepsAPinFileWithOtherChangesAsAProductPath`(3) `4`개, path check·current-evidence 시험 `25/25`. 실제 0.42.92 Lane 3 범위 `34b2b16..4246880`에서 pin 여섯 개가 모두 허용되고 남은 product path는 이 변경으로 Lane 3가 더 고치지 않는 시험 두 개뿐임을 확인했다. `DEVELOPMENT_PROCEDURE.md` §10 문장을 고쳤다. private skill은 train 문장에 path check 예외를 적지 않으므로 바꾸지 않았다. `BL-0005` 닫음.

## Task 8: 종료 검증과 merge (PR 2)

- [x] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.


실행 기록(2026-10-08): clean HEAD `f6b5945`에서 solution 실패 `0`(Verification `664`, Delivery `775`), `git diff --check origin/main...HEAD` 통과, product payload 경로 변경 `0`. 이 기록 commit 뒤 push, PR, green CI 뒤 merge한다. PR 번호와 merge commit은 Task 9 기록에 적는다.

## Task 9: BL-0002 QoS readback

- [x] `vm.blkio-get`·`vm.bandwidth` readback의 `mutation_supported`가 상수 `false`인데 제품에 `vm.blkio-set`·`vm.bandwidth-set` mutation이 있는 원인을 확인하고, readback이 실제 mutation 지원을 말하도록 고친다. CLI/Web/parity fixture와 문서의 영향 확인. product payload이므로 `queue` 행(Lane 2 probe 기능군 `vm.qos`)을 같은 PR에 더한다. 검증: HyperV.Tests, Api.Tests, 영향 범위의 web 시험, `git diff --check`. `BL-0002` 닫기. 로컬 commit.


실행 기록(2026-10-08): Task 8 PR 2 #67(head `c24eb52`)을 merge했다(`fc7e462`). main push `dotnet` shard가 attempt 1(`ConsoleCancellationBridge` 계열 2초 대기)과 attempt 2(Chromium loopback bootstrap 시한)에서 서로 다른 runner 시간 의존 시험으로 red였고, 사용자 추가 승인 뒤 attempt 3이 green이다(run `37650610001`, `BL-0008`·`BL-0009` 등록). 원인: `vm.blkio-get`·`vm.bandwidth` readback의 `mutation_supported`가 `vm.qos.storage.set`·`vm.qos.network.set`이 생기기 전의 상수 `false`였다. Web QoS 카드는 이 값을 그대로 보여 준다. 수정: 값을 dispatch catalog에서 계산(`IsCatalogMutation`, 해당 operation이 Mutation이면 `true`)한다. 처음 commit(`24be288`)은 `mutation_operation` 필드도 더했으나 `Reads.cs`가 module-size ratchet 한도(`570`줄)를 넘어 Delivery 계약 `50`개가 실패했다. 그 실패를 Task 10 commit 뒤 Delivery 실행에서 발견해, 필드를 빼고 helper를 `ResourceMutations.cs`로 옮겨 `Reads.cs`를 값 두 줄 변경으로 되돌렸다(HyperV `273/273`, Delivery `775/775`). Web·CLI fixture와 계약 문서에 이 값의 pin은 없다. 시험 `NativeVmQosReadbacksReportTheCatalogMutation`(2), HyperV `273/273`, Api `490/490`. product payload이므로 queue 행은 Task 12에서 이 commit SHA로 더한다. `BL-0002` 닫음.

## Task 10: BL-0001 설계

- [x] `vm.create` reconcile 지문(`CreateFingerprintMatches`)에 디스크(`disk0.vhdx` 연결), ISO, switch 연결을 넣어 `DefineSystem` 뒤 장치 연결 전에 끊긴 create를 `postcondition-confirmed`로 판정하지 않게 하는 설계를 쓴다(판정 표, readback 필드, 시험, 다음 train Lane 2 probe). 검증: `git diff --check`. 로컬 commit.


실행 기록(2026-10-08): 설계 `pcv-vm-create-reconcile-devices-v1`을 썼다. 근거: create provider는 항상 `<vm_root>\<name>\disk0.vhdx` 연결, 필수 인자 `iso_path` ISO 연결, 상수 `Default Switch` 연결을 하고, 0.42.92 probe의 실제 `vm.list` 행에 `storage[].path/attached`, `dvd_media[].path`, `network[].switch`가 있다. 결정: 기대 장치를 baseline에 저장하지 않고 reconcile 때 job 인자(`vm_root`, `iso_path`)에서 계산해 업그레이드 전 job에도 적용하고 baseline 계약 v1을 유지한다. 장치가 하나라도 빠지면 `target-fingerprint-mismatch`와 `missing_devices`, 회수 hint. `DOCUMENTATION_INDEX`에 줄을 더했다.

## Task 11: BL-0001 구현

- [ ] 설계대로 Api reconcile handler와 필요한 readback을 고치고 시험을 더한다. product payload이므로 `queue` 행(Lane 2 probe 기능군 `vm.create` reconcile)을 더한다. 검증: Api.Tests, HyperV.Tests, `git diff --check`. `BL-0001` 닫기. 로컬 commit.

## Task 16: BL-0008 cancellation bridge 시험

- [ ] `CallbackCanWaitForDisposeWithoutDeadlockAndLaterSignalsAreNoOps`의 dispose 대기(2초)를 runner 속도에 기대지 않게 고친다(교착 없음 판정은 유지). 검증: Verification.Tests. `BL-0008` 닫기. 로컬 commit.

## Task 17: BL-0009 loopback bootstrap 브라우저 시험

- [ ] `ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste`의 bootstrap 대기 시한을 runner 속도에 기대지 않게 고친다(판정 조건은 유지). 검증: Host.Tests. `BL-0009` 닫기. 로컬 commit.

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
