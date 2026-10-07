# 완료 판정 갭: release train `0.42.92`와 C5 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** completion 판정(`main` `ad8b5c2`, `met=4/7`)의 정책 안 갭 두 개를 닫는다. `C2-queue`는 PR #59(`vm.create` 잔여물 회수)를 실은 `0.42.92-admin-smoke` train을 package부터 Lane 3까지 돌려 operational current로 승격해 닫는다. `C5-risk-ubuntu-26-runner`는 2026-10-19 뒤 확인한다. 정책 밖 갭(`C7-BL-0001`~`0003` 분류)은 `next_approval_required`에 둔다.

**Architecture:** completion 모드 campaign(설계 `pcv-completion-autopilot-v1` §2.6). train은 `docs/DEVELOPMENT_PROCEDURE.md` §10 순서와 단일 PR train(`pcv-single-pr-train-v1`)이다. branch `train/04292-20261007`(`origin/main` `ad8b5c2` 기준) 하나에 출발부터 Lane 3까지 쌓고 PR 하나로 merge한다. payload commit은 `ad8b5c2`이고 그 main push run은 Development Gates `37634988986`, Public Boundary `37634988899`(둘 다 success)다. task 템플릿은 직전 train plan `docs/superpowers/plans/2026-10-06-purecvisor-desktop-node-train-04291.md`를 따른다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `pcvverify train-host-inputs`/`train-facts`/`train-evidence`/`lane3-spec`/`train-path-check`/`completion`, `Invoke-PcvBatchSupervisor.ps1`, current-card capture, 설치본 `pcvcli`, `Invoke-PcvLane3PromotionDocs.ps1`

## 사용자 결정 (2026-10-07)

승인 원문(정책 `config/completion-autopilot-policy.json` `approval_locator`): `User-Approval: 2026-10-07 "1,2,3,4,5" item 4, approve completion-autopilot-policy, clarified by the user as Lane 2/3 fully automatic: campaigns generated under the policy may run the release train standard host mutation scope (MSI install, repair, uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, probe VM create and delete), Lane 3 current-evidence write, push, PR and merge after green CI, and stop on FAIL, new-design, undecided backlog rows, new Add-Type/P/Invoke/native ACL/installer handoff and permanent out-of-scope items`

판정 결과: `artifacts/completion/20261007-r3/result.json`(head `ad8b5c2488568581505b8e1d2f9cdd39db23b2ff`).

| 갭 | 종류 | 정책 행 |
| --- | --- | --- |
| `C2-queue` | `train-departure` | Lane 0~3. train `0.42.92-admin-smoke` 출발, queue 행 PR #59 고정, package build, pair host mutation(여섯 bucket: 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX. bucket을 하나씩 돌던 때와 같은 범위), fullgate(MSI 설치·repair·제거·`REMOVE_DATA`, service, route parity VM), installed current-card, Lane 2 probe 기능군 `vm.create`(probe VM 생성·삭제, service 중지·시작), Lane 3 `current-evidence.json` 쓰기, push, PR, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR |
| `C5-risk-ubuntu-26-runner` | `deadline-wait` | Lane 0/1. `not_before` 2026-10-19. push, PR, green CI 뒤 merge |
| `C7-BL-0001`, `C7-BL-0002`, `C7-BL-0003` | `user-decision` | 정책 밖. `next_approval_required` |

출발은 정기 출발일(직전 출발 2026-10-06의 7일 뒤) 전이다. 2026-10-07 승인 4를 고를 때 사용자는 "이번 run에서 0.42.92 train까지 이어 갈 수 있다"는 설명을 받고 그 선택지를 골랐다. 그래서 이 출발은 사용자 요청에 의한 조기 출발로 본다. 정차하면 이 campaign의 train 권한은 끝난다.

## 적재 변경

| PR | 변경 commit | 요약 | Lane 2 probe |
| --- | --- | --- | --- |
| #59 | `ee90474` | VM 폴더 안의 끊긴 `vm.create` 잔여 `disk0.vhdx`를 표식 확인 뒤 회수하고, reconcile not-applied 안내에 위치와 회수 경로를 붙임 | `vm.create` (Task 6) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner `pcv-cleanhost-*`, fullgate route smoke VM, probe VM `pcv-probe-c2-*`뿐이고 끝나면 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. guest 인증 정보는 실행 경계에서 만들어 `-GuestCredential`로 넘긴다.
- evidence와 artifact는 새 이름(`-04292`, `-04291-04292`)으로 쓴다. 이전 train의 evidence, facts, artifact를 바꾸지 않는다.
- baseline package는 `artifacts/admin-smoke-package-20261006-04291`(MSI, payload, update ZIP, catalog)다. orchestrator 입력은 train `0.42.91`과 같다. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다. FAIL은 current에 쓰지 않는다.
- 남은 task가 `not_before` 미래 task(Task 10, Task 11)뿐이면, 판정의 정책 안 갭이 그 task들이 이미 덮는 `deadline-wait`뿐일 때 새 campaign을 열지 않고 `deadline-wait`로 멈춘다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 0: 출발

- [x] `release-train.json`의 queue 행(PR #59)을 train `0.42.92-admin-smoke`의 `carriages`로 옮기고 `status=running`, payload commit `ad8b5c2`와 main push run id 두 개를 출발 기록에 적는다. 검증: `PcvReleaseTrainContractTests`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): 출발 `2026-10-07T23:19:00+09:00`, source(payload) `ad8b5c2488568581505b8e1d2f9cdd39db23b2ff`, carriages `[59]`, queue 비움, train `status=running`. payload commit의 main push run: Development Gates `37634988986` success, Public Boundary Contract `37634988899` success.

## Task 1: package

- [ ] clean HEAD에서 `build.ps1 -Version 0.42.92-admin-smoke -MsiProductVersion 0.42.92 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261007-04292`, `New-PcvAdminSmokeUpdatePackage.ps1`. train-facts 입력과 package 문서. 로컬 commit.

## Task 2: pair 실행

- [ ] orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261007-04291-04292`). 여섯 bucket PASS, closed descriptor, `observation_error` 없음. 로컬 commit.

## Task 3: pair 문서

- [ ] `train-facts`에 pair 문서 `6`개를 더해 렌더한다. 로컬 commit.

## Task 4: fullgate

- [ ] clean `0.42.92` 위에서 `full-admin-host-mutation-gate-20261007-04292`(manifest는 `pcvverify train-host-inputs`). 사후 검사(build commit, 같은 version ARP 1개, firewall 규칙 0). 로컬 commit.

## Task 5: installed current-card

- [ ] train facts에서 렌더한 capture 스크립트를 root 밖에서 실행한다. `status=pass`, `not-promoted`. 로컬 commit.

## Task 6: probe `vm.create` 잔여물 회수 (PR #59)

- [ ] 설치본 `0.42.92`에서 probe VM `pcv-probe-c2-*`로 (1) `vm.create` 진행 중 서비스 프로세스를 끊어 잔여 `disk0.vhdx`와 표식 `.pcv-create-pending.json`을 남기고 `job reconcile` not-applied hint에 위치·회수 경로가 나오는지, (2) 같은 이름 재 `vm.create`가 잔여물을 회수(`Remove interrupted create residue`, `recovered_residue`)하는지, (3) 표식 없는 폴더는 `PCV_VHD_ALREADY_EXISTS` detail `no-create-marker`로 거절하고 지우지 않는지 확인한다. probe VM·폴더 삭제, service Running 확인. evidence `lane2-vm-create-residue-actual-vm-2026-10-07-04292`. 로컬 commit.

## Task 7: 문서

- [ ] `train-facts`에 fullgate, current-card 문서를 더해 렌더한다. probe evidence와 `FEATURE_IMPLEMENTATION_LEDGER` 비후보 관측 행을 쓴다. 로컬 commit.

## Task 8: Lane 3

- [ ] functional carry-forward, consume(`…-closed` descriptor와 consume manifest), payload main push 문서(`main-push-payload`, `train-path-check --payload ad8b5c2 --head HEAD`), current-card `promoted-current`. `current-evidence.json`, `lane3-spec` 입력과 spec `04292`, 문서 도구 dry-run·`-Apply`·`-Check`, `release-train.json` 승격(`operational_current`, train `status=promoted`, queue 빈 목록). 로컬 commit.

## Task 9: 종료와 train PR merge

- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge(merge 직전 base가 `origin/main`인지 확인). merge 뒤 main push run(Development Gates, Public Boundary)을 기다린다. red면 revert PR을 열고 멈춘다.

## Task 10: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `completion-autopilot-20261007` Task 9다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Task 11: 완료 판정

- [ ] `not_before` 2026-10-19. clean `main`에서 `pcvverify completion`을 돌린다. exit `0`이면 결과를 인용한 감사 문서 `docs/project-status-audit-<date>.md`로 완료를 적고 push, PR, green CI 뒤 merge한다. exit `1`이면 결과를 기록하고 `pcv-campaign` §6 연쇄로 넘긴다.

## Nonclaims

- train `0.42.92`가 Lane 3까지 PASS해야 operational current가 바뀐다. FAIL이면 `0.42.91-admin-smoke` 그대로다.
- probe 결과는 `vm.create` 기능군 관측이다. feature 승격 후보를 늘리지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
