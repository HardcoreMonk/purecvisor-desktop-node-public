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
| `C2-queue` | `train-departure` | Lane 0~3. train `0.42.92-admin-smoke` 출발, queue 행 PR #59 고정, package build, pair host mutation(여섯 bucket: 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX. bucket을 하나씩 돌던 때와 같은 범위), fullgate(MSI 설치·repair·제거·`REMOVE_DATA`, service, route parity VM, 추가 승인 뒤 `os-mutation-gate`의 firewall 규칙·Event Log source·LAN listener), installed current-card, Lane 2 probe 기능군 `vm.create`(probe VM 생성·삭제, service 중지·시작), Lane 3 `current-evidence.json` 쓰기, push, PR, green CI 뒤 merge, merge 뒤 main push가 red면 revert PR |
| `C5-risk-ubuntu-26-runner` | `deadline-wait` | Lane 0/1. `not_before` 2026-10-19. push, PR, green CI 뒤 merge |
| `C7-BL-0001`, `C7-BL-0002`, `C7-BL-0003` | `user-decision` | 정책 밖. `next_approval_required` |

추가 승인(2026-10-07): `1` (Task 4 정지 보고의 다음 승인 1). 정책 `train-departure` 범위에 fullgate `os-mutation-gate`를 더하고 Task 4부터 다시 진행한다. 이 단계는 PureCVisor firewall 규칙 추가·삭제, Event Log source 등록, LAN listener를 다룬다. LAN prefix는 직전 manifest(`artifacts/batch-manifests/full-admin-host-mutation-gate-20261006-04291.json`)의 값을 실행 때 환경 변수로만 쓰고 저장소에 남기지 않는다. 끝 상태는 PureCVisor firewall 규칙 `0`이다. 정책 파일은 `config/` 아래라 단일 PR train의 `train-path-check` 범위에 걸리므로 train merge 뒤 Task 12에서 고친다.

추가 승인 2(2026-10-08): `1` (Task 8 정지 보고의 다음 승인 1). 단일 PR train의 path check가 Lane 3의 `config/`·`src/` pin 변경과 충돌해(`new-design-required`, backlog `BL-0005`) 이 train은 두 PR 방식(0.42.89~0.42.91과 같음)으로 마친다. PR A는 Task 0~7 branch(evidence, docs)를 push, PR, green CI 뒤 merge한다. PR B는 `origin/main`에서 만든 Lane 3 branch로, PR A merge의 main push run을 `main-push` 템플릿으로 인용하고 pin 갱신을 포함해 push, PR, green CI 뒤 merge한다. host mutation은 없다.

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

- [x] clean HEAD에서 `build.ps1 -Version 0.42.92-admin-smoke -MsiProductVersion 0.42.92 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261007-04292`, `New-PcvAdminSmokeUpdatePackage.ps1`. train-facts 입력과 package 문서. 로컬 commit.

실행 기록(2026-10-07): clean HEAD `e250950`에서 `packaging/windows-desktop-node/installer/build.ps1` build `36`초, MSI `dc79fdd1…`, payload `efbbbb16…`(파일 `8`), Host `ede0af77…`, CLI `052993a3…`, `build_utc` `2026-10-07T14:21:03.0191697Z`(ISO 8601). `New-PcvAdminSmokeUpdatePackage.ps1 -BuildSeconds 36`으로 update ZIP `fcaf13dc…`, catalog `7f170c8d…`, `package-facts.json`(host mutation 없음). train-facts 입력 `docs/ga-ready/trains/0.42.92-admin-smoke.train-facts-input.json`(documents는 package만, 서술 `3`개)으로 `train-facts`(생성 값 `20`), `train-evidence --write` → `admin-smoke-package-2026-10-07-04292` `written`, `--check` `current`. TrainEvidenceGolden `7/7`, Delivery `775/775`.

## Task 2: pair 실행

- [x] orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261007-04291-04292`). 여섯 bucket PASS, closed descriptor, `observation_error` 없음. 로컬 commit.

실행 기록(2026-10-07): 시작 상태 설치본 `0.42.91`, service Running/Automatic, VM은 보존 VM(Off)뿐. 0.42.91 실행 스크립트와 같은 형식(guest 인증 정보는 clean-host runner 소스에서 실행 경계에 만들어 `-GuestCredential`로 넘김)으로 `-PlanOnly` `ok=true`(host mutation 없음) 뒤 `-Execute`(`2026-10-07T14:24:09Z`부터 `238`초). 여섯 bucket 모두 PASS, `descriptor_eligible`, closed descriptor, restoration 불필요. `observations.json` `15`개 모두 `observation_error` 없음. 끝 상태: 설치본 `0.42.92`(`0.42.92-admin-smoke+e25095029463943ad75b166f95718a77f7661467`), ARP `{0FDC43BE-E4C0-4D10-A574-781A9F060D4C}` `0.42.92` 1개, service Running/Automatic, Web `200`, 보존 VM Off. artifact `artifacts/manual-admin-campaign-20261007-04291-04292`.

## Task 3: pair 문서

- [x] `train-facts`에 pair 문서 `6`개를 더해 렌더한다. 로컬 commit.

실행 기록(2026-10-07): train-facts 입력 documents에 pair 문서 `6`개(ops-summary, update-rollback, clean-host, burn, msix, pair-descriptor)를 더했다. `train-facts` 생성 값 ops-summary `10`, update-rollback `23`, clean-host `21`, burn `16`, msix `17`, pair-descriptor `17`(서술 `0`). `train-evidence --write` 여섯 문서 `written`, package `current`, `--check` 일곱 문서 `current`. TrainEvidenceGolden `7/7`, Delivery `775/775`.

## Task 4: fullgate

- [x] clean `0.42.92` 위에서 `full-admin-host-mutation-gate-20261007-04292`(manifest는 `pcvverify train-host-inputs`). 사후 검사(build commit, 같은 version ARP 1개, firewall 규칙 0). 로컬 commit.

실행 기록(2026-10-07~08): 첫 시도는 `os-mutation-gate`(firewall 규칙, Event Log source, LAN listener)가 승인 범위 밖이라 실행 전에 멈췄고(`approval-required-mutation`), 사용자 추가 승인 `1`(`c2a842f`) 뒤 다시 시작했다. host-inputs 입력을 commit(`b51b8cf`)하고 `train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들었다(`sha256 de6e0002…`, LAN prefix는 직전 manifest artifact에서 읽어 실행 환경 변수로만 넘김). pair가 남긴 clean `0.42.92`(`+e250950`) 위에서 supervisor `-DryRun -AllowHostMutation`(두 단계 planned) 뒤 `2026-10-07T15:01:01Z`~`15:04:57Z` 실행, `ok=true`, `status=completed`. step: service-msi-hyperv exit `0`(시도 `1`, `224.1`초), os-mutation exit `0`(`11.1`초). 사후 검사: 설치본 Host/CLI `+b51b8cf` == gate build(clean HEAD `b51b8cf`, product source는 `ad8b5c2`와 같고 그 뒤는 문서뿐), 같은 version ARP `{7D82F2ED-575E-41B2-89E8-BE2B51976E4C}` `0.42.92` 1개, PureCVisor firewall 규칙 `0`, service Running/Automatic, Web `200`, 보존 VM Off. artifact `artifacts/batch-runs/full-admin-host-mutation-gate-20261007-04292`.

## Task 5: installed current-card

- [x] train facts에서 렌더한 capture 스크립트를 root 밖에서 실행한다. `status=pass`, `not-promoted`. 로컬 commit.

실행 기록(2026-10-08): 절차 순서(fullgate 문서 렌더 → capture 스크립트)가 도구에서 순환했다. `train-facts` fullgate는 current-card `summary.json`을 요구하고(`artifact-missing`), `train-host-inputs --kind current-card`는 facts fullgate 값을 요구한다(`fact-missing:fullgate`). supervisor stdout을 `artifacts/batch-manifests/full-admin-host-mutation-gate-20261007-04292.result.json`(빈 `.stderr.txt`)에 두고, capture 스크립트가 읽는 fullgate 값 다섯 개(`batch_id`, `routeparity_artifact_root`, MSI `67b257a3…`, payload `dda8eb7f…`, `provenance_commit` `b51b8cf`)를 route artifact provenance에서 계산해 facts에 잠시 넣고 스크립트를 만든 뒤 facts를 되돌렸다(commit 없음). capture 스크립트(`sha256 c1285063…`)를 root 밖에서 실행해 `5`초, `status=pass`, `promotion_ledger_status=not-promoted`, 설치본 `0.42.92-admin-smoke+b51b8cf`, ARP `1`, CLI/Host payload 일치, 남은 시험 VM `0`. 스크립트는 root에 복사했다. 그 뒤 train-facts 입력에 fullgate 문서와 서술 `2`개를 더해 렌더(생성 값 `35`, `written`)했고, 렌더된 fullgate 값 다섯 개가 임시 값과 모두 같음을 확인했다. 이 순환은 backlog `BL-0004`(`undecided`)로 등록했다. Task 7의 fullgate 렌더는 여기서 끝났다.

## Task 6: probe `vm.create` 잔여물 회수 (PR #59)

- [x] 설치본 `0.42.92`에서 probe VM `pcv-probe-c2-*`로 (1) `vm.create` 진행 중 서비스 프로세스를 끊어 잔여 `disk0.vhdx`와 표식 `.pcv-create-pending.json`을 남기고 `job reconcile` not-applied hint에 위치·회수 경로가 나오는지, (2) 같은 이름 재 `vm.create`가 잔여물을 회수(`Remove interrupted create residue`, `recovered_residue`)하는지, (3) 표식 없는 폴더는 `PCV_VHD_ALREADY_EXISTS` detail `no-create-marker`로 거절하고 지우지 않는지 확인한다. probe VM·폴더 삭제, service Running 확인. evidence `lane2-vm-create-residue-actual-vm-2026-10-07-04292`. 로컬 commit.

실행 기록(2026-10-08): 설치본 `0.42.92-admin-smoke+b51b8cf`. r1은 판정과 정리를 모두 돌았으나 스크립트 결함(판정 아닌 항목을 summary 판정에 넣음)으로 `summary.json`을 쓰지 못했다(r1 artifact 보존, 같은 판정 결과). 고친 r2(`18`초)가 PASS다. 끊김: running 중 표식과 `disk0.vhdx`가 생긴 순간 서비스 프로세스를 종료해 첫 시도에 `PCV_JOB_INTERRUPTED`, VM 행 `0`, 잔여물 `disk0.vhdx` `4194304` B와 `.pcv-create-pending.json` `283` B(필드 `7`개, `pcv-vm-create-pending/v1`, `directory_created=true`). reconcile: `PCV_JOB_RECONCILIATION_REQUIRED` not-applied, hint에 위치와 회수 경로(`can leave a disk in`), 파일은 그대로. 같은 이름 create: `succeeded`, 단계 `Remove interrupted create residue`, `recovered_residue`, managed, 표식 삭제. 표식 없는 폴더(빈 `disk0.vhdx`): `PCV_VHD_ALREADY_EXISTS` detail `no-create-marker`, 파일 유지, VM 없음. managed delete 뒤 폴더 없음. 끝 상태: probe VM·폴더 `0`, 보존 VM stopped(시작과 같음), service Running/Automatic, Web `200`. artifact `artifacts/lane2-vm-create-residue-20261007-04292-r2`(스크립트 사본 포함).

## Task 7: 문서

- [x] `train-facts`에 fullgate, current-card 문서를 더해 렌더한다. probe evidence와 `FEATURE_IMPLEMENTATION_LEDGER` 비후보 관측 행을 쓴다. 로컬 commit.

실행 기록(2026-10-08): fullgate 문서는 Task 5에서 렌더했다. current-card는 card root에 `capture-current-card.ps1`(생성 스크립트 사본)이 있어야 해서 그 이름으로 복사한 뒤 렌더했다(생성 값 `31`, 서술 `1`, `written`). facts 문서 `9`개 모두 `--check` `current`. probe evidence `lane2-vm-create-residue-actual-vm-2026-10-07-04292`를 손으로 쓰고 `FEATURE_IMPLEMENTATION_LEDGER` 비후보 관측 절에 `pcv.vm.create` 행을 더했다. TrainEvidenceGolden `7/7`, Delivery `775/775`.

## Task 8: Lane 3

- [ ] (추가 승인 2) 먼저 PR A(이 branch)를 clean HEAD 종료 검증, push, PR, green CI 뒤 merge하고 merge의 main push run을 기다린다. 그 뒤 `origin/main`에서 `lane3/04292-promotion-20261008`을 만들어 functional carry-forward, consume(`…-closed` descriptor와 consume manifest), main push 문서(`main-push`, PR A merge의 run), current-card `promoted-current`. `current-evidence.json`, `lane3-spec` 입력과 spec `04292`, 문서 도구 dry-run·`-Apply`·`-Check`, `release-train.json` 승격(`operational_current`, train `status=promoted`, queue 빈 목록). 로컬 commit.

## Task 9: 종료와 Lane 3 PR(PR B) merge

- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge. merge 뒤 main push run(Development Gates, Public Boundary)을 기다린다. red면 revert PR을 열고 멈춘다.

## Task 10: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `completion-autopilot-20261007` Task 9다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Task 11: 완료 판정

- [ ] `not_before` 2026-10-19. clean `main`에서 `pcvverify completion`을 돌린다. exit `0`이면 결과를 인용한 감사 문서 `docs/project-status-audit-<date>.md`로 완료를 적고 push, PR, green CI 뒤 merge한다. exit `1`이면 결과를 기록하고 `pcv-campaign` §6 연쇄로 넘긴다.

## Task 12: 정책 파일 갱신

- [ ] Task 9 merge 뒤 `origin/main`에서 branch를 만들어 `config/completion-autopilot-policy.json` `train-departure` `mutation_scope`에 `fullgate os-mutation-gate (firewall rule, Event Log source, LAN listener)`를 더하고 `approval_locator`에 추가 승인 문장을 붙인다. 설계 `pcv-completion-autopilot-v1` §2.5 표에 같은 줄을 더한다. 검증: `PcvProjectCompletionInputsContractTests`, Delivery, `git diff --check`. push, PR, green CI 뒤 merge.

## Nonclaims

- train `0.42.92`가 Lane 3까지 PASS해야 operational current가 바뀐다. FAIL이면 `0.42.91-admin-smoke` 그대로다.
- probe 결과는 `vm.create` 기능군 관측이다. feature 승격 후보를 늘리지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
