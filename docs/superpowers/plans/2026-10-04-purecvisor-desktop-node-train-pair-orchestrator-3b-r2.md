# Release train 3단계 3b 재리허설과 3c 검토 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 3b 리허설에서 나온 관측 ARP 결함을 고치고, 같은 package로 리허설을 한 번 더 돌려 pair 문서 `9`개 전부를 실제 데이터로 생성한다. 통과하면 3c 절차 변경을 PR로 연다.

**Architecture:** 설계 `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3b, §3c. 직전 리허설 evidence `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289`. 수정은 branch `lane1/orchestrator-arp-strict-20261004`, 재리허설은 그 merge 위의 Lane 2 branch, 3c는 그 뒤 branch에서 PR을 연다.

**Tech Stack:** `Invoke-PcvManualAdminPackagePairCampaign.ps1`, Delivery C# 계약 시험, `Invoke-PcvBatchSupervisor.ps1`, `pcvverify train-facts`, `pcvverify train-evidence`

## 사용자 결정 (2026-10-04)

승인 원문: `1,2,3` (3b 리허설 최종 보고의 다음 승인 1 "관측 ARP 결함 수정(Lane 1 한 줄과 strict mode 계약 시험)", 2 "고친 orchestrator로 리허설 재실행(3b와 같은 host mutation 범위)", 3 "그 뒤 3c 절차 변경 검토"에 대한 답).

| 항목 | 범위 |
| --- | --- |
| 1 | orchestrator 관측 함수 수정, 계약 시험, commit, PR, green CI 뒤 merge |
| 2 | 제품 Update/Rollback, clean-host VM 생성·Windows Update·삭제, Burn, MSIX, fullgate. 리허설 evidence PR과 merge |
| 3 | `DEVELOPMENT_PROCEDURE.md` §10 train task를 orchestrator 기준으로 바꾸는 PR을 연다. merge는 검토 뒤 별도 확인 |

## Global Constraints

- 보존 VM과 0.42.89 evidence, facts, card artifact를 바꾸지 않는다. 재리허설 산출물은 `-r3b2` 이름을 쓴다.
- guest 인증 정보는 실행 경계에서 만들고 기록하지 않는다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: 관측 ARP 수정

- [x] `Add-PcvObservation`의 Uninstall 키 읽기를 `PSObject.Properties`로 먼저 확인하고, Delivery 계약 시험에 strict mode 조건을 더한다. strict mode를 켠 harness로 실제 호스트에서 확인한다. PR, green CI 뒤 merge.

실행 기록(2026-10-04): ARP 수집의 `Where-Object`가 `$_.PSObject.Properties['DisplayName']`를 먼저 보고, `DisplayVersion`도 있을 때만 읽는다. Delivery 계약 시험이 strict mode 선언 위치와 두 확인 token, 옛 `Where-Object { [string]$_.DisplayName` 형태의 부재를 본다(Delivery `763`). `Set-StrictMode -Version Latest`를 켠 harness로 실제 호스트에서 관측 함수를 돌려 `observation_error` 없음, ARP `{FACF6D5F-…}` `0.42.89` 1개를 확인했다. 이 commit 뒤 PR, green CI 뒤 merge한다.

## Task 2: orchestrator 재리허설

- [x] campaign `manual-admin-campaign-20261004-04288-04289-r3b2`로 `-Execute`. `observations.json`에 `observation_error`가 없어야 한다.

실행 기록(2026-10-04): `-PlanOnly` `ok=true` 뒤 `-Execute`(`11:32:22Z`부터 `256`초). baseline 정렬 뒤 여섯 bucket 모두 PASS, closed descriptor, reservation 소비, restoration 불필요. `observations.json` `16`개 항목 모두 `observation_error` 없음, ARP 목록 있음(fullgate MSI `{FACF6D5F-…}`가 Burn 뒤 clean MSI `{0E27390F-…}`로 바뀐다). 끝난 뒤 설치본은 clean package `0.42.89`(Host `+b463903`), service Running/Auto, 보존 VM Off.

## Task 3: fullgate 복구

- [ ] `full-admin-host-mutation-gate-20261004-04289-r3b2`.

## Task 4: current-card

- [ ] `installed-operator-surface-current-card-20261004-04289-r3b2`.

## Task 5: facts 생성

- [ ] pair 문서 `9`개를 `pcvverify train-facts`와 `train-evidence --write`로 만든다. 결과는 `artifacts/train-facts-rehearsal-20261004-r3b2/`로 옮기고 0.42.89 facts를 되돌린다.

## Task 6: 재리허설 evidence와 merge

- [ ] `train-pair-orchestrator-rehearsal-2026-10-04-04288-04289-r2`, clean HEAD 종료 검증, push, PR, green CI 뒤 merge.

## Task 7: 3c 절차 변경 PR

- [ ] Task 5가 `9/9`이면 `DEVELOPMENT_PROCEDURE.md` §10 train task 2a~2g를 orchestrator 실행, facts 생성, 렌더 task 하나로 바꾸는 PR을 연다. merge하지 않는다.

## Nonclaims

- 리허설은 승격 근거가 아니다. operational current는 `0.42.89-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
