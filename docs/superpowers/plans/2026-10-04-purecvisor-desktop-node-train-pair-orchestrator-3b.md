# Release train 3단계 3b 리허설 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 이미 있는 `0.42.88`과 `0.42.89` package로 pair orchestrator를 실제 호스트에서 한 번 돌리고, 그 출력으로 `pcvverify train-facts`가 pair 문서를 만들 수 있는지 확인한다. 결과는 승격 근거가 아니다.

**Architecture:** 설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3b. branch `lane2/train-3b-rehearsal-20261004`(`origin/main` `c9f2d5b` 기준), 리허설 evidence PR 하나로 merge한다.

**Tech Stack:** `Invoke-PcvManualAdminPackagePairCampaign.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1` 출력, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`, `pcvverify train-evidence`

## 사용자 결정 (2026-10-04)

승인 원문: `3b 리허설 승인`. 설계 §6 승인 단위대로 Lane 2 host mutation(제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn install/repair/remove, MSIX install/update/remove, fullgate)과 리허설 evidence PR, merge까지다. 3c 절차 변경은 승인 밖이다.

## Global Constraints

- 보존 VM의 전원, Notes, 디스크를 바꾸지 않는다.
- guest 인증 정보는 clean-host runner 기본값을 실행 경계에서 `PSCredential`로 만들고 명령줄, artifact, evidence에 남기지 않는다.
- 0.42.89 evidence, facts 파일, current-card artifact를 바꾸지 않는다. 리허설 산출물은 `-r3b` 이름을 쓴다.
- orchestrator가 실패하면 restoration 결과를 확인하고 멈춘다. orchestrator 수정이 필요하면 이 campaign 밖(Lane 1)이다.
- 한도: checkpoint마다 Lane 2 45분·tool batch 12회.

## Task 0: 계획과 campaign

- [x] 이 계획, campaign `train-pair-orchestrator-3b-20261004`.

## Task 1: orchestrator 리허설

- [x] 3a 도구가 만든 update ZIP과 catalog(`artifacts/update-package-check-20261004/`)로 `-PlanOnly`, 그 뒤 `-Execute`. campaign `manual-admin-campaign-20261004-04288-04289-r3b`.

실행 기록(2026-10-04): `-PlanOnly` `ok=true` 뒤 `-Execute`(`06:26:34Z`부터 `252`초). 설치본이 target이라 orchestrator가 baseline catalog로 `0.42.88`에 먼저 맞췄다. 여섯 bucket 모두 PASS, closed descriptor 생성, reservation 소비, restoration 불필요. 끝난 뒤 설치본은 clean package `0.42.89`(Host `+b463903`), ARP `{0E27390F-…}` 1개, service Running/Auto, 보존 VM Off. 발견: `observations.json` `16`개 항목 모두 `observation_error=PropertyNotFoundException`. orchestrator의 `Set-StrictMode -Version Latest` 아래에서 `DisplayName`이 없는 Uninstall 키를 읽어 ARP 수집만 실패했다(VM, boot time, firewall은 기록됨). 수정은 계획대로 이 campaign 밖(Lane 1)이다.

## Task 2: fullgate 복구

- [x] `full-admin-host-mutation-gate-20261004-04289-r3b`로 0.42.89 fullgate build를 다시 설치한다.

실행 기록(2026-10-04): orchestrator가 남긴 clean `0.42.89`(`{0E27390F-…}`) 위에서 `06:32:05Z`부터 약 `457`초, 두 step 모두 exit `0`(service-msi-hyperv `444.490s`). 사후 build commit 검사 `c67d22b` 일치, 같은 version ARP `{FACF6D5F-1041-4B77-92B6-7B476447ECB7}` 1개, MSI log `another client exists`/`Won't Overwrite` `0`/`0`, managed delete `storage_cleanup` 관측. fullgate MSI `dcec4e92`, payload `11228c3f`. 설치본 `0.42.89-admin-smoke+c67d22b`, service Running/Auto, Web `200`, firewall rule `0`, 보존 VM Off.

## Task 3: current-card

- [x] `installed-operator-surface-current-card-20261004-04289-r3b`(읽기 전용).

실행 기록(2026-10-04): 0.42.89 current-card 스크립트에서 root, evidence id, fullgate batch, 기대 SHA 상수, `canonical_current_evidence`(`0.42.89-admin-smoke`)를 바꿔 새 root `artifacts/installed-operator-surface-current-card-20261004-04289-r3b`에 캡처했다. `status=pass`, CLI `3/3`, Web `2/2`, 설치본 Host/CLI가 r3b fullgate payload와 같음, 테스트 VM `0`, secret 없음, `not-promoted`. summary SHA `d42d97fc…`. 0.42.89 card artifact는 바꾸지 않았다.

## Task 4: facts 생성 확인

- [x] 리허설 출력으로 `pcvverify train-facts`와 `train-evidence --write`를 돌려 pair 문서 `9`개를 만든다. 결과는 `artifacts/train-facts-rehearsal-20261004-r3b/`로 옮기고, 0.42.89 facts 파일과 evidence 폴더를 원래대로 둔다.

실행 기록(2026-10-04): 0.42.89 facts를 백업하고 리허설 출력으로 네 번 돌렸다. A(9개)는 관측 결함으로 `fact-mismatch:observations:after:installed-runtime-ops-summary`, B(package, pair-descriptor, current-card)는 pair-descriptor가 bucket 문서 id를 요구해 `document-missing:update-rollback`, B2(package, current-card)와 C(fullgate)는 생성과 렌더 통과. 결과는 `artifacts/train-facts-rehearsal-20261004-r3b/`로 옮기고 0.42.89 facts를 되돌렸다(golden `ok`, 작업 트리 clean).

## Task 5: 리허설 evidence

- [x] `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289`: bucket 판정을 0.42.89 수동 pair와 비교하고, facts 생성 결과와 발견을 적는다. `DOCUMENTATION_INDEX` 호스트 설치본 줄을 맞춘다.

실행 기록(2026-10-04): evidence `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289`(`PARTIAL_PASS_WITH_OBSERVATION_DEFECT`). `DOCUMENTATION_INDEX` 호스트 설치본 줄을 리허설 fullgate build로 맞췄다.

## Task 6: 종료와 merge

- [x] clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

실행 기록(2026-10-04, clean HEAD `992aaa8`): `dotnet test src/DesktopNode.sln` 실패 `0`(Delivery `763`, Verification `593`, Api `488`, HyperV `255`, Host `216`, Contracts `200`, Cli `183`, Runtime `129`, Service `11`). Pester 네 종 `151/151`. `npm run test:required --prefix web` exit `0`. Required CI 네 shard 모두 `ok=true`, `plan_only=false`. Task 4와 5는 commit 하나(`992aaa8`)로 묶였다. 이 기록 commit 뒤 push, PR, green CI 뒤 merge한다.

## Nonclaims

- 리허설은 승격 근거가 아니다. operational current는 `0.42.89-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
