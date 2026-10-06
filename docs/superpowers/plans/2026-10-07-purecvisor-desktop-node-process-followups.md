# 개발 공정 최적화 후속 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `process-optimization-20261006`의 다음 승인 다섯 개를 실행한다. Edge 누수 정리 효과를 fullgate 재실행으로 확인하고, 단일 PR train과 Hyper-V 어댑터 integration 단계(ADR-0016)를 구현하며, 끊긴 `vm.create` 잔여물 처리를 구현해 다음 train 대기열에 올린다. 2026-10-19 뒤 C5 runner 확인으로 끝낸다.

**Architecture:** 설계 `pcv-single-pr-train-v1`, `pcv-hyperv-adapter-integration-tier-v1`, `pcv-interrupted-create-residue-v1`(모두 2026-10-06), 점검 기준값(fullgate `msi-lifecycle-smoke` uninstall `340`초·`342`초, 2026-10-06 0.42.91). branch는 둘이다. Task 1~10은 `lane2/process-followups-20261007`, Task 11~12는 그 PR이 merge된 뒤 `origin/main`에서 만든 `lane1/interrupted-create-residue-20261007`이다. Task 13은 2026-10-19 뒤 별도 branch다.

**Tech Stack:** `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-host-inputs`, msiexec, C# xUnit(`DesktopNode.Verification`, `DesktopNode.HyperV`, 새 `DesktopNode.HyperV.IntegrationTests`), 문서(`docs/DEVELOPMENT_PROCEDURE.md`, `docs/adr/`), private `.claude/skills/pcv-ship/SKILL.md`

## 사용자 결정 (2026-10-07)

승인 원문: `1,2,3,4,5` (`process-optimization-20261006` 최종 보고의 다음 승인 후보에 대한 답). 2번과 4번은 두 가지로 읽혀 다시 물었고, 답은 `지금 fullgate 재실행`과 `채택으로 보고 구현`이다.

| 항목 | 범위 |
| --- | --- |
| 1 | `train-04291-20261006` Task 12(2026-10-19 뒤 C5 runner 확인)를 2026-10-06 `1,2,3` 항목 3 승인 그대로 다시 연다(Task 13) |
| 2 | 0.42.91 fullgate를 지금 다시 돌려 Edge 정리 효과를 확인한다. Lane 2 host mutation: MSI 설치·repair·제거·`REMOVE_DATA`, service, route parity VM. 끝에 0.42.91 운영 MSI를 다시 설치해 설치본을 operational current build로 되돌린다 |
| 3 | `pcv-single-pr-train-v1` 구현(Lane 1), push, PR, green CI 뒤 merge |
| 4 | ADR-0016 채택, Hyper-V 어댑터 integration 단계 2 구현. standing approval은 `pcv-it-` 접두사 VM의 생성·삭제로 한정한다(service, MSI, switch 변경 없음) |
| 5 | `pcv-interrupted-create-residue-v1` 구현(Lane 1)과 다음 release train 대기열 행. 대기열 규칙(§10)대로 PR을 green CI 뒤 merge한 다음 행을 남긴다 |
| 권한 | Lane 0/1/2, task마다 로컬 commit, push, PR, green CI 뒤 merge. Lane 3와 `current-evidence.json` 쓰기는 없다 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 fullgate route parity VM과 `pcv-it-` 접두사 VM뿐이고 끝나면 지운다.
- fullgate 재실행은 Task 11의 제품 변경보다 먼저, 제품 payload가 0.42.91과 같은 tree에서 돈다. 새 batch와 artifact는 `-20261007-04291` 이름으로 쓰고 2026-10-06 artifact와 evidence를 고치지 않는다.
- 사설 LAN prefix와 사용자 이름은 실행 값으로만 쓴다. token, credential, password는 command line, summary, evidence에 남기지 않는다.
- 이 campaign은 operational current(`0.42.91-admin-smoke`)를 바꾸지 않는다. fullgate 재실행 결과는 관측이며 승격 근거가 아니다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: F1 확인 fullgate 실행 (Lane 2)

- [x] 사전 상태(누수 Edge `0`, `Default Switch`, ARP `0.42.91` 1개, service Running/Automatic)를 확인하고, 추적하지 않는 host-inputs(date `2026-10-07`)로 `train-host-inputs --kind fullgate-manifest`가 manifest를 만들게 한다. supervisor `-DryRun` 뒤 실행하고, 사후 검사(같은 version ARP 1개, build commit, firewall 규칙 `0`)와 `msi-lifecycle-smoke` 단계별 시간을 기록한다. 로컬 commit.

실행 기록(2026-10-07): 사전 상태는 누수 Edge `0`, 전체 프로세스 `539`(2026-10-06 점검 때 `1059`), `Default Switch`, ARP `{ED64B13A-…}` `0.42.91` 1개, service Running/Auto, 설치본 `+990a4b2`, PureCVisor firewall 규칙 `0`, VM은 보존 VM Off 하나였다. `train-host-inputs --kind fullgate-manifest`가 manifest를 처음 실제로 만들었다(`artifacts/batch-manifests/full-admin-host-mutation-gate-20261007-04291.json`, `written`). os-mutation 단계는 firewall 규칙·Event Log source·LAN listener를 바꿔 승인 2 범위 밖이고 F1 확인에 필요 없으므로 manifest에서 빼고 `service-msi-hyperv-admin-smoke` 단계만 돌렸다. 그래서 LAN prefix는 문서용 주소를 넣었다. supervisor `-DryRun` 통과 뒤 실행(`00:55:01`~`01:00:23` KST): `ok=true`, `status=completed`, 시도 `1`, 단계 `321.3`초(2026-10-06 `802.8`초). `msi-lifecycle-smoke` `232`초(2026-10-06 `710`초). msiexec 로그 기준 uninstall-preserve `340`→`91`초, uninstall-remove-data `342`→`90`초다. 두 번 모두 아직 `DesktopNode.Host.exe` 1개를 files-in-use로 잡는다. 사후 검사: 같은 version ARP `{8D07CE80-…}` 1개, 설치본 `+2e05cc1` == gate build(`2e05cc1`, 제품 payload는 0.42.91과 같은 tree), firewall 규칙 `0`, VM 보존 VM Off 하나, service Running/Auto, PathName batch root는 새 batch다. artifact `artifacts/batch-runs/full-admin-host-mutation-gate-20261007-04291`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20261007-04291`.

## Task 2: 운영 build 복원 (Lane 2)

- [x] 0.42.91 운영 MSI(`46dddccb…`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20261006-04291`)를 설치해 설치본을 operational current build(`+990a4b2`)로 되돌린다. 설치본 Host·CLI SHA-256이 0.42.91 fullgate payload와 같은지, ARP 1개, service Running/Automatic, Web `200`을 확인한다. 로컬 commit.

실행 기록(2026-10-07): 2026-10-06 fullgate `final-restore-install` 로그의 명령줄과 같은 속성으로 설치했다(`BATCH_EVIDENCE_ROOT`=2026-10-06 batch root, `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/qn /norestart`). 설치 전 MSI SHA-256 `46dddccb…` 일치. msiexec exit `0`, `10`초(같은 version upgrade로 `{8D07CE80-…}` 제거). 끝 상태: ARP `{ED64B13A-742B-422C-9142-DED650CB856B}` `0.42.91` 1개(Task 1 전과 같은 ProductCode), 설치본 Host `a935701e…`·CLI `2c249f0d…`는 0.42.91 fullgate payload와 같다. ProductVersion `+990a4b2`, service Running/Auto, PathName batch root는 `full-admin-host-mutation-gate-20261006-04291`, Web `200`, VM은 보존 VM Off 하나, PureCVisor firewall 규칙 `0`. 설치본이 operational current와 다시 같다. 로그 `artifacts/f1-check-20261007/restore-operational-04291.log`.

## Task 3: F1 확인 기록

- [x] Task 1·2 결과를 새 evidence `docs/ga-ready/evidence/fullgate-msi-uninstall-f1-check-2026-10-07-04291.md`에 적는다(2026-10-06 기준값과 비교, 누수 Edge 수, 결론). `EVIDENCE_INDEX`에 관측 줄을 더한다. 검증: Delivery, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): evidence `fullgate-msi-uninstall-f1-check-2026-10-07-04291`(`PASS`, `observation-only`)를 썼다. 단계·uninstall 시간 비교, 사전 상태, 사후 검사와 복원, 남은 files-in-use(service process)는 report-only로 적었다. `EVIDENCE_INDEX`는 Lane 3 도구가 만드는 승격 절로만 이루어져 있고 관측 evidence(예: `default-switch-recovery-2026-09-20-04277`)는 올리지 않으므로 계획과 달리 줄을 더하지 않았다. public boundary evidence guard Pester `90/90`, Delivery `769/769`, `git diff --check` 통과.

## Task 4: 단일 PR train 1 — 절차

- [x] `docs/DEVELOPMENT_PROCEDURE.md` §10 task 순서를 PR 하나로 바꾸고, 출발 조건에 고정할 `main` HEAD의 push run green을, 승인 문구에 post-merge red면 revert PR을 더한다. train 설계 §4.7과 `pcv-single-pr-train-v1` 상태를 맞추고 private `pcv-ship`에 train branch base 일치 확인을 더한다(private 로컬 commit). 검증: Delivery, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): §10에 단일 PR 규칙 두 항목을 더했다. 하나는 출발 때 payload commit main push run green과 run id 기록, Lane 3 main push evidence의 인용 run과 경로 확인이다. 다른 하나는 merge 직전 base 일치, merge 뒤 main push 대기, red면 revert PR이다. 승인 문구 항목에 revert PR을 더했다. task 순서 표를 `0`~`7`로 바꿨다(pair evidence PR task 삭제, Lane 3와 종료 검증·PR 하나). 0.42.89~0.42.91의 두 PR 기록과 golden은 그대로 둔다는 줄을 붙였다. train 설계 §4.7에 2026-10-07 단일 PR 문구를 더했다. `pcv-single-pr-train-v1` 상태를 `accepted`로 바꿨다. private `pcv-ship` §3에 train PR base 확인(`merge-base --is-ancestor origin/main`)과 merge 뒤 main push 대기·revert를 더했다(private 로컬 commit). 도구 변경은 Task 5·6이다. contract spec pin `current`, Delivery `769/769`, Verification.Tests `619/620`(dirty tree의 PolicyBoundary), `git diff --check` 통과.

## Task 5: 단일 PR train 2 — main push evidence

- [x] `train-facts`·`train-evidence`의 `main-push` 문서가 payload commit의 main push run을 인용하고 `payload commit..PR head` 제품 경로 확인 결과를 담게 한다. 기존 train(0.42.89~0.42.91) golden은 그대로 통과해야 한다. 검증: Verification.Tests, Delivery, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): 기존 `main-push` 템플릿은 0.42.89~0.42.91 golden이 쓰므로 그대로 두고 새 템플릿 `main-push-payload`(`head_sha`=payload commit, `train_pr_head`, `path_check_line`)를 카탈로그에 더했다. `FirstTrainFactsCoverEveryTemplateOnce`는 새 템플릿을 제외한다. 경로 확인은 `pcvverify train-path-check --payload <commit> --head <commit|HEAD>`(`pcv-train-path-check-result-v1`)가 기존 `IProcessRunner`로 `git diff --name-only`를 돌려 `src/`·`web/src/`·`config/`·`.github/` 변경이 없으면 exit `0`, 있으면 `1`이다. `TrainSinglePrTests` `4`개(템플릿 렌더, 경로 판정 두 경우, 잘못된 payload 거부). Release build 경고 `0`, Verification.Tests `623/624`(dirty tree의 PolicyBoundary), Delivery `769/769`.

## Task 6: 단일 PR train 3 — Lane 3 spec

- [x] `lane3-spec`이 새 main push evidence id와 `DOCUMENTATION_INDEX` 공개 소스 권위 줄(payload commit)을 만들게 한다. 기존 golden 유지. 검증: Verification.Tests, Delivery, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): `TrainLane3SpecBuilder`가 facts의 `main-push`와 `main-push-payload` 가운데 있는 쪽을 쓴다. 없으면 `facts-document-missing`, 둘 다 있으면 `facts-main-push-ambiguous`로 멈춘다. payload 쪽 alias는 `<tag>-payload-main-push-not-provider-required-authority`이고, main push evidence·run id·head SHA·package candidate 값과 `index_sections.main_push_evidence`를 그 문서에서 읽는다. 기존 `lane3-spec` golden(0.42.89~0.42.91)은 그대로 통과한다. `DOCUMENTATION_INDEX` 공개 소스 권위 줄은 spec 값이 아니라 Lane 3가 손으로 고치는 줄이라 §10에 "HEAD는 payload commit" 규칙으로 적었고, §10에 `train-path-check` 명령과 `main-push-payload` 사용법을 더했다. 실제 실행에서 Task 5의 `train-path-check`가 `process-command-forbidden=git`으로 거부되는 결함을 찾았다(process 정책은 정식 실행 파일 목록 8개 전체를 allowlist로 요구한다). `CutoverGitBoundary`와 같은 목록으로 고치고 실제 git으로 도는 시험 2개(같은 commit은 exit `0`, PR #57 구간 `a35ea05..dde42d8`는 exit `1`)를 더했다. 이 branch에서 `--payload dde42d8 --head HEAD`는 제품 경로 `5`개로 exit `1`이다(도구 branch라 기대한 결과). `TrainSinglePrTests` `8/8`, Verification.Tests `627/628`(dirty tree의 PolicyBoundary), Delivery `769/769`, `git diff --check` 통과.

## Task 7: ADR-0016

- [x] `docs/adr/0016-hyperv-adapter-integration-tier.md`(accepted, 2026-10-07 승인 4)와 `docs/ADR_INDEX.md` 현재 기준 절을 쓴다. 허용 범위, 금지, 승인 방식, 실패 처리는 설계 §4대로다. 설계 상태를 `accepted`로 바꾼다. 검증: Delivery, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): ADR-0016(`채택 / 단계 2 구현 중`)을 썼다. 결정 마커 `4`개, 허용 범위(`pcv-it-<run id>-` VM 생성·설정·삭제, 저장 위치 `artifacts/hyperv-integration/<run id>/`), 금지, 승인 방식(`PCV_HYPERV_INTEGRATION_APPROVAL`이 campaign `approval_locator` 안의 문자열), 실행 가드, 정리, 결과의 지위(관측)를 정했다. `ADR_INDEX`에 2026-10-07 현재 기준 절을 더했다(구조를 고정하는 계약은 없음). 설계 상태를 `accepted`로 바꿨다. Delivery `769/769`, `git diff --check` 통과.

## Task 8: integration 단계 구현

- [x] `src/DesktopNode.HyperV.IntegrationTests`(solution·Required CI 밖), 실행 가드, 정리, 첫 시험(Off VM의 inventory `stopped`와 export·network connect 정책 수용), `InternalsVisibleTo`, 그리고 project가 solution과 Required CI 밖에 있음을 고정하는 Delivery 계약을 만든다. VM은 만들지 않는다(가드 없이 돌리면 전부 실패하는지 확인). 검증: 새 project build, Delivery, Verification.Tests, `git diff --check`. 로컬 commit.

실행 기록(2026-10-07): `src/DesktopNode.HyperV.IntegrationTests`를 만들었다(solution, `development-verification-suites.json`, workflow 밖). `HyperVIntegrationFixture`가 ADR-0016 가드를 맡는다. 가드는 `PCV_HYPERV_INTEGRATION_APPROVAL`(20자 이상, campaign `approval_locator` 안의 문자열), 관리자 또는 `Hyper-V Administrators`(SID `S-1-5-32-578`), `Msvm_VirtualEthernetSwitch`의 `Default Switch`, ISO 존재, 이전 run이 남긴 `pcv-it-` VM 없음이다. VM 이름은 `pcv-it-<UTC run id>-`, 저장 위치는 `artifacts/hyperv-integration/<run id>/`다. 정리는 이 run 접두사 VM을 어댑터 `vm.delete`로 지우고 저장 폴더를 지운 뒤 시작·끝 VM 이름 목록을 비교한다. 결과는 `artifacts/hyperv-integration/<run id>.summary.json`(`promotion_evidence=false`)에 쓴다. 첫 시험 `CreatedOffVmReadsAsStoppedAndPassesTheSharedOffCheck`는 제품 어댑터 `vm.create`(Gen2, 1 vCPU, 1024 MB, 8 GB, `vm_root`)로 만든 VM의 inventory 상태가 `stopped`이고 `VmPowerStates.IsOff`(export·network connect·device add 정책이 함께 씀)가 참이며 managed인지 본다(`9402774` 결함 종류). 어댑터와 `VmPowerStates`가 public이라 설계의 `InternalsVisibleTo`는 필요 없었다. Delivery 계약 `HyperVIntegrationTierContractTests` `2`개(solution·suite·workflow 밖, 가드 문자열 유지). 확인: 승인 없이 실행하면 가드 예외로 `1/1` 실패하고 VM 목록(`pcv-guest-installed-04253-r1`만)과 `artifacts/hyperv-integration` 폴더 수(`0`)가 그대로다. 새 project build 경고 `0`, solution Release build 경고 `0`, Delivery `771/771`, Verification.Tests `627/628`(dirty tree의 PolicyBoundary), contract spec pin `current`. VM은 만들지 않았다.

## Task 9: integration 단계 첫 실행 (Lane 2)

- [ ] standing approval 범위(`pcv-it-` 접두사 VM 생성·삭제) 안에서 integration 단계를 이 호스트에서 한 번 돌린다. 시작·끝 VM 이름 목록이 같고 `artifacts/hyperv-integration/` 아래에 남은 것이 없음을 확인하고 결과를 계획에 `hyperv-integration` 관측으로 적는다. 로컬 commit.

## Task 10: 종료 검증과 merge (1차)

- [ ] clean HEAD 종료 검증(build, 네 shard, Pester 네 종) 뒤 push, PR, green CI 뒤 merge.

## Task 11: 끊긴 create 잔여물 처리 구현

- [ ] Task 10 merge 뒤 `origin/main`에서 branch를 만들고 `pcv-interrupted-create-residue-v1`을 구현한다(소유 표식, 회수 조건, `PCV_VHD_ALREADY_EXISTS` detail, reconcile hint, managed delete 정리). 설계의 report-only(`DefineSystem` 직후 끊긴 create의 reconcile 판정)는 같은 checkpoint에서 다룰 수 있으면 함께 고친다. 검증: HyperV.Tests, Api.Tests, `git diff --check`. 로컬 commit.

## Task 12: 대기열과 merge (2차)

- [ ] 종료 검증, push, PR, green CI 뒤 merge, 그리고 `docs/ga-ready/release-train.json` `queue`에 이 PR 행을 더한다(§10 대기열 규칙과 같은 방식).

## Task 13: C5 runner 확인 (2026-10-19 이후)

- [ ] 2026-10-19 이후에만 실행한다. 그 전에 오면 기한 대기로 멈춘다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 완료 정의 §4에 C5 충족과 프로젝트 완료 판정을 적고 campaign을 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.91-admin-smoke` 그대로다. Lane 3와 `current-evidence.json` 쓰기는 없다.
- fullgate 재실행과 integration 단계 결과는 관측이다. 승격 근거가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
