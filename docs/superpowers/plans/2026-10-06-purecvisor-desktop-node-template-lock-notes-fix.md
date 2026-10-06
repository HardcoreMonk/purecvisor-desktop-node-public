# Hyper-V Notes 다중 원소 쓰기 결함 수정 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `vm.template.lock`이 성공을 보고하면서 `template-lock=true` marker를 저장하지 못하는 결함과, 같은 원인으로 메모가 있는 VM에 `vm.manage`가 managed marker를 잃는 잠재 결함을 고친다. product payload 변경이므로 release train 대기열에 한 행을 더한다.

**Architecture:** 근거는 campaign `completion-probes-20261006` Task 1 정차 기록이다. 설치본 `0.42.90`에서 lock job은 `succeeded`였지만 Notes에는 `managed-by=purecvisor-desktop-node`만 남았다. Hyper-V `Msvm_VirtualSystemSettingData.Notes`는 여러 줄 메모를 원소 하나 안의 `\n`으로 저장한다(보존 VM은 원소 `1`개, `\n` `3`개). `DesktopNodeHyperVWmiVmManageProvider`의 `vm.manage`와 `vm.template.lock`만 메모를 줄 단위로 나눈 여러 원소 배열로 써서 첫 원소만 남는다. 다른 쓰기 경로(create, clone, import, switch)는 원소 하나를 쓴다. branch `lane1/template-lock-notes-fix-20261006` 하나, PR 하나. 설치본 확인은 승인 2의 release train `0.42.91`과 그 뒤 완료 probe가 맡는다.

**Tech Stack:** `src/DesktopNode.HyperV`, `src/DesktopNode.HyperV.Tests`, Delivery 계약 시험, `docs/ga-ready/release-train.json`

## 사용자 결정 (2026-10-06)

승인 원문: `1,2` (campaign `completion-probes-20261006` 정차 보고의 다음 승인).

| 항목 | 범위 |
| --- | --- |
| 1 | template lock 결함 수정 campaign. 멈춘 probe campaign을 교체한다. 원인 확정, 재현 시험, 수정. Lane 1, host mutation 없음. release-train `queue` 행. push, PR, green CI 뒤 merge |
| 2 | 1의 merge 뒤 release train `0.42.91` 출발(0.42.90과 같은 범위: package, pair host mutation, fullgate, current-card, Lane 3 `current-evidence.json` 쓰기, push, PR, green CI 뒤 merge)과 0.42.91에서 완료 probe 재실행. 이 계획 밖이며 다음 campaign이 연다 |

## Global Constraints

- host, 설치본, service, VM을 바꾸지 않는다. 원인 확인은 CIM 읽기만 했다.
- Notes 형식을 바꾸지 않는다. 저장은 지금처럼 줄을 `\n` 또는 `Environment.NewLine`으로 잇고, 원소 하나에 담는다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 원소 하나로 쓰기

- [ ] `DesktopNodeHyperVManagedNotes`에 Notes 쓰기 값(원소 하나 배열)을 만드는 helper를 두고, `vm.manage`와 `vm.template.lock`이 그것을 쓰게 한다. 시험: helper가 lock·unlock·manage 결과를 원소 하나로 만들고 그 원소에 marker 줄이 모두 들어 있다. provider source에 Notes를 `Split`해서 쓰는 형태가 없다. 검증 `dotnet build src/DesktopNode.sln -c Release`(경고 `0`), `dotnet test src/DesktopNode.HyperV.Tests -c Release`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

## Task 2: PR과 대기열 행

- [ ] branch를 push하고 PR을 먼저 연 뒤 그 번호로 `release-train.json` `queue`에 Task 1 변경 한 행을 더한다(`lane2_probe`는 P1-7 template lock 완료 probe). 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit, push.

## Task 3: 종료 검증과 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. campaign을 닫고 로컬 commit, push, green CI 뒤 merge. 승인 2로 다음 campaign(train `0.42.91`과 완료 probe)을 연다.

## Nonclaims

- operational current는 `0.42.90-admin-smoke` 그대로다. 수정의 설치본 확인은 train `0.42.91` 뒤 probe가 한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
