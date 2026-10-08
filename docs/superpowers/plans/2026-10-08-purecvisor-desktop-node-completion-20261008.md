# 완료 판정 연쇄: train `0.42.93` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 판정 head `56e7cd0`의 갭 `C2-queue`로 release train `0.42.93-admin-smoke`를 package부터 Lane 3까지 한 PR로 돌려 operational current로 승격한다. 적재 두 행의 Lane 2 probe를 하고, 2026-10-19 뒤 C5 확인과 완료 판정은 이관 task로 남긴다.

**Architecture:** `docs/DEVELOPMENT_PROCEDURE.md` §10 단일 PR train(`pcv-single-pr-train-v2`). 직전 train plan은 `docs/superpowers/plans/2026-10-07-purecvisor-desktop-node-completion-20261007.md`다. 판정 결과 `artifacts/completion/20261008/result.json`, 정책 `config/completion-autopilot-policy.json`. branch는 `lane1/completion-20261008`이고 payload commit은 이 opening의 `origin/main`이다.

**Tech Stack:** `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`/`train-evidence`/`train-host-inputs`/`lane3-spec`/`train-path-check`/`completion`

## 사용자 결정 (2026-10-08)

승인 원문: `config/completion-autopilot-policy.json` `approval_locator` (판정 exit `1`, 갭 `C2-queue`, `C5-risk-ubuntu-26-runner`). 직전 campaign의 ADR-0016 standing approval을 그대로 옮긴다.

| 항목 | 범위 |
| --- | --- |
| `C2-queue` | `train-departure`. Lane 0/1/2/3. mutation scope: MSI install, MSI repair, MSI uninstall, REMOVE_DATA, service, pair six buckets, fullgate route parity VM, fullgate os-mutation-gate (firewall rule, Event Log source, LAN listener), probe VM create and delete. Lane 3 `current-evidence.json` 쓰기. push, PR, green CI 뒤 merge |
| `C5-risk-ubuntu-26-runner` | `deadline-wait`. Lane 0/1. `not_before` 2026-10-19. host mutation 없음. push, PR, green CI 뒤 merge. 이관 Task 10이 이 갭을 덮는다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·삭제로 한정 |

## 적재 변경

| PR | 변경 commit | 요약 | Lane 2 probe |
| --- | --- | --- | --- |
| #68 | `92146da` | QoS readback `mutation_supported`를 dispatch catalog로 계산 | `vm.qos` (Task 5) |
| #68 | `65c376d` | `vm.create` reconcile이 디스크·ISO·Default Switch까지 맞을 때만 확정 | `vm.create reconcile` (Task 6) |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 clean-host runner, fullgate route smoke VM, probe VM뿐이고 끝나면 지운다.
- token, credential, password는 command line, summary, evidence에 남기지 않는다. guest 인증 정보는 호출 직전에 만들어 `-GuestCredential`로 넘긴다.
- evidence와 artifact는 새 이름(`-04293`, `-04292-04293`)으로 쓴다. 이전 train의 evidence를 바꾸지 않는다.
- baseline package는 `artifacts/admin-smoke-package-20261007-04292`다. ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`.
- 환경 원인 일시 실패만 같은 단계를 한 번 다시 돌린다. 그 밖의 FAIL은 정차한다. FAIL은 current에 쓰지 않는다.
- 판정 뒤 남은 갭이 Task 10·11이 덮는 `deadline-wait`뿐이면 새 campaign을 열지 않고 `deadline-wait`로 멈춘다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 0: 출발

- [x] payload commit의 main push Development Gates와 Public Boundary가 green인지 확인하고 run id를 적는다. `release-train.json` `queue` 두 행을 train `0.42.93-admin-smoke`의 `carriages` `[68]`로 옮기고 `status=running`, `queue`는 빈 목록. 검증: `PcvReleaseTrainContractTests`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-08): 출발 `2026-10-08T14:33:00+09:00`, payload `56e7cd0ff0df3a4ac2688ff2f4030fa10e72936c`, carriages `[68]`, queue 비움, train `status=running`. payload main push Development Gates `37732628393` success, Public Boundary `37732628401` success. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 1: package

- [x] clean HEAD에서 `build.ps1 -Version 0.42.93-admin-smoke -MsiProductVersion 0.42.93 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest -OutputRoot artifacts/admin-smoke-package-20261008-04293`, `New-PcvAdminSmokeUpdatePackage.ps1`. `train-facts`로 package 문서. 로컬 commit.

실행 기록(2026-10-08): build `35`초, `build_utc` `2026-10-08T05:42:52.3292771Z`, provenance `41421d8bf3272dbdbb8dcf384bcf1ff3f329726a`. MSI SHA-256 `13d7f0d476828f865b0d4aca7331a2dcd1a10a8dbe157b9d6a93dc2217f9fb49`, payload SHA-256 `2a071cd2c6298e289b940a37ef124a1c3d3502fd9a476033174251884a0f2cf7`, payload `8`, WiX `5.0.2+aa65968c`, update ZIP SHA-256 `97e4ec6a6376fea970cd2f084ec4352d062bb00cb42e2fdfc0f08d1cb7dfd181`. `train-facts` package `generated=20` `narrative=3`, `train-evidence --check` `current`. `TrainEvidenceGoldenTests`와 `TrainFactsBuilderTests` `19/19`. host mutation 없음. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 2: pair

- [x] orchestrator `-PlanOnly` 뒤 `-Execute`(campaign `manual-admin-campaign-20261008-04292-04293`). 여섯 bucket PASS, closed descriptor, `observation_error` 없음. `train-facts`로 pair 문서 6개. 로컬 commit.

실행 기록(2026-10-08): `-PlanOnly` `ok=true`, host mutation 없음. `-Execute` `2026-10-08T15:02:56+09:00`부터 `238`초. 여섯 bucket PASS, closed descriptor `manual-admin-campaign-20261008-04292-04293-closed`, observations `15`개 `observation_error` 없음, restoration 없음. 끝 상태: 설치본 `0.42.93-admin-smoke`(`0.42.93-admin-smoke+41421d8bf3272dbdbb8dcf384bcf1ff3f329726a`), ARP `{867BA47E-0553-4B74-8E46-8C803008F61C}` `0.42.93` 1개, service Running/Automatic, Web `200`, 보존 VM Off. `train-evidence --check` pair 문서 6개 `current`. `TrainEvidenceGoldenTests`와 `TrainFactsBuilderTests` `19/19`. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 3: fullgate

- [x] `pcvverify train-host-inputs --kind fullgate-manifest`로 manifest를 만들고 clean `0.42.93` 위에서 `full-admin-host-mutation-gate-20261008-04293`을 돌린다. 사후 검사(build commit, 같은 version ARP 1개, PureCVisor firewall 규칙 0). `train-facts`로 fullgate 문서. 로컬 commit.

실행 기록(2026-10-08): `train-host-inputs --kind fullgate-manifest --write`로 manifest를 만들었다(sha256 `e0b8b4df8a014c038eef9e4b25ff9b9f7cb97af67c3d1478e3b4664e038496d0`, LAN prefix는 직전 manifest에서 읽어 환경 변수로만 넘김). supervisor `-DryRun -AllowHostMutation`은 두 단계 planned, `ok=true`. `2026-10-08T06:20:25Z`부터 `268`초 실행, `ok=true`, `status=completed`. step: service-msi-hyperv exit `0`(시도 `1`, `255.5`초), os-mutation exit `0`(`11.1`초). 사후 검사: 설치본 Host/CLI `+818d00f` == gate build, ARP `{70A3822D-89D0-4E0D-A60B-892CE8CDC776}` `0.42.93` 1개, PureCVisor firewall 규칙 `0`, service Running/Automatic, Web `200`, 보존 VM Off. `train-facts` fullgate `generated=35` `narrative=2`, `--check` `current`. 시험 `24/24`. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 4: installed current-card

- [x] fullgate 문서를 렌더한 뒤 `train-host-inputs --kind current-card` 스크립트를 root 밖에서 실행한다. `status=pass`, `not-promoted`. 복사 이름은 `capture-current-card.ps1`. `train-facts`로 current-card 문서. 로컬 commit.

실행 기록(2026-10-08): capture 스크립트 sha256 `730b2e5c0c29556fc0325a17a02d0eba760d8c86a4c76e4769f2ce07a572261f`를 root 밖에서 `6`초 실행했다. `status=pass`, `promotion_ledger_status=not-promoted`, 설치본 `0.42.93-admin-smoke+818d00f`, ARP `1`, CLI `3`/`3`, Web `2`, service Running/Auto LocalSystem, 남은 시험 VM `0`, secret 없음, host mutation 없음. 스크립트를 root에 `capture-current-card.ps1`로 복사했다. `train-facts` current-card `generated=31` `narrative=1`, `--check` `current`. 시험 `24/24`. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 5: probe `vm.qos`

- [x] 설치본 `0.42.93`에서 `vm.blkio-get`·`vm.bandwidth` readback의 `mutation_supported`가 catalog mutation과 같은지 확인한다. probe VM은 끝나면 지운다. evidence `lane2-vm-qos-actual-vm-2026-10-08-04293`. 보존 VM은 그대로 둔다. 로컬 commit.

실행 기록(2026-10-08): 설치본 `0.42.93-admin-smoke+818d00f`에서 probe VM `pcv-probe-qos-1008`을 Generation 2, CPU 2, memory 2048 MB, disk 20 GB로 만들고 켜지 않았다. `2026-10-08T15:57:34+09:00`부터 `10`초. create job `succeeded`(`3`초). `vm blkio-get` `storage_qos.mutation_supported=true`, `vm bandwidth` `network_qos.mutation_supported=true`. 설치 빌드 catalog의 `vm.qos.storage.set`와 `vm.qos.network.set`는 `Mutation`이라 readback과 같다. delete job `succeeded`(`3`초) 뒤 VM과 `D:\PureCVisor\VMs\pcv-probe-qos-1008`은 없었다. 보존 VM은 Off 그대로다. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 6: probe `vm.create reconcile`

- [x] 설치본 `0.42.93`에서 설계 `pcv-vm-create-reconcile-devices-v1`대로, 디스크·ISO·Default Switch가 빠진 create는 `postcondition-confirmed`가 아니고 `missing_devices`가 보이는지 확인한다. probe VM·폴더는 끝나면 지운다. evidence `lane2-vm-create-reconcile-devices-actual-vm-2026-10-08-04293`. 로컬 commit.

실행 기록(2026-10-08): 설치본 `0.42.93-admin-smoke+818d00f`에서 probe VM `pcv-probe-reconcile-1008` create를 큐에 넣고, VM이 보인 직후(1561 ms, 디스크·ISO·Default Switch 0) `DesktopNode.Host`를 종료했다. 서비스는 1초 만에 Running. job `job-a25d2b4676714642a16e5440f193cce5`는 `PCV_JOB_INTERRUPTED`. reconcile은 `target-fingerprint-mismatch`, `missing_devices=disk,iso,switch`이고 `postcondition-confirmed`는 없었다. managed delete 뒤 같은 이름 create가 `succeeded`로 세 장치를 붙였고, 최종 delete 뒤 VM과 폴더는 없었다. `2026-10-08T16:10:54+09:00`부터 `16`초. 보존 VM은 Off 그대로다. operational current는 `0.42.92-admin-smoke` 그대로다.

## Task 7: Lane 3

- [x] functional carry-forward, consume(`…-closed` descriptor와 consume manifest), payload commit의 main push를 `train-path-check`로 확인한 `main-push-payload` 문서, current-card `promoted-current`. `current-evidence.json`, `lane3-spec`, 문서 도구 dry-run·`-Apply`·`-Check`, `release-train.json` 승격(`operational_current`, train `status=promoted`). 로컬 commit.

실행 기록(2026-10-08): payload `56e7cd0`에서 train head `b6ecb7f`까지 `train-path-check`가 exit 0이다(제품 경로 없음). functional carry-forward, consume manifest(요약 7개, 복사 없음), `main-push-payload`(Development Gates `37732628393`, Public Boundary `37732628401`)를 렌더하고 current-card를 `promoted-current`로 고쳤다. `current-evidence.json`과 `release-train.json`의 operational current는 `0.42.93-admin-smoke`이고 train `status`는 `promoted`다. `lane3-spec` 생성 뒤 문서 도구 dry-run, `-Apply`, `-Check`가 ok다. 호스트 mutation은 하지 않았다.

## Task 8: 종료와 train PR merge

- [x] clean HEAD 종료 검증, `train-path-check` exit `0`, push, PR 하나, green CI 뒤 merge. merge 직전 base는 `origin/main`과 같다. merge 뒤 main push run을 기다리고, red면 revert PR을 열고 멈춘다.

실행 기록(2026-10-08): clean HEAD `87918a2`에서 Release build 경고 0. shard `dotnet` Full/M `50615ms` ok, `web` Full/M `31921ms` ok, `delivery` Full/M `3494ms` ok, `installer-policy`는 요청 Full/M에 대해 effective Release/L(`tier-l-requires-release`) `2201ms` ok. 모두 `plan_only=false`. dotnet assembly는 Service 11, Contracts 200, Cli 183, Runtime 129, HyperV 273, Delivery 775, Host 216, Api 503, Verification 666, 실패 0. Pester는 wrapper 528, installer 49, web 50, manual-admin 129, 실패 0. `train-path-check`는 payload `56e7cd0`에서 exit 0이고 제품 경로는 pin 여섯 파일의 SHA-256 한 줄뿐이다. `git diff --check origin/main...HEAD` 통과. 이 기록 commit 뒤 push, PR, green CI merge와 main push 확인을 잇는다.

## Task 9: 판정

- [x] clean `main`에서 `pcvverify completion`을 돌린다. exit `0`이면 감사 문서로 완료를 적고 push, PR, green CI 뒤 merge한다. exit `1`이고 남은 갭이 Task 10이 덮는 `deadline-wait`뿐이면 기록하고 `deadline-wait`로 멈춘다. 그 밖이면 `pcv-campaign` §6 연쇄로 넘긴다.

실행 기록(2026-10-08): clean main `8de4384`에서 `pcvverify completion` exit `1`. `complete=false met=6/7 gaps=1` head `8de4384e1a11f260133d7447db7f9c502d67fbf9`. C2는 `current=0.42.93-admin-smoke`, `operational_current=0.42.93-admin-smoke`, `train=promoted`, `queue=0`, `unfinished_trains=0`으로 충족이다. 남은 갭은 `C5-risk-ubuntu-26-runner`(`deadline-wait`, `not_before=2026-10-19`)뿐이고 Task 10이 덮는다. 새 campaign을 열지 않고 `deadline-wait`로 멈춘다. host mutation 없음.

## Task 10: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `completion-backlog-20261008` Task 14다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Task 11: 완료 판정

- [ ] `not_before` 2026-10-19. 이관 전 `completion-backlog-20261008` Task 15다. clean `main`에서 `pcvverify completion`을 돌린다. exit `0`이면 결과를 인용한 감사 문서로 완료를 적고 push, PR, green CI 뒤 merge한다. exit `1`이면 결과를 기록하고 `pcv-campaign` §6 연쇄로 넘긴다.

## Nonclaims

- 이 opening은 operational current를 바꾸지 않는다. `0.42.93-admin-smoke`는 Task 7 Lane 3가 쓰기 전까지 `0.42.92-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
