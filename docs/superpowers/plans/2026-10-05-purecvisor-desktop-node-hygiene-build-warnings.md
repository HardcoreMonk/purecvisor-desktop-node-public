# 로컬 branch 확인과 빌드 경고 정리 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** merge되지 않은 로컬 branch 2개와 별도 clone 1개의 상태를 확인해 보고한다. Release build 경고 2개(CS8622, CS8600)를 동작 변경 없이 없애고 release train 대기열에 한 행을 더한다.

**Architecture:** 확인은 읽기만 한다. 삭제는 이 campaign 밖이며 `next_approval_required`로 넘긴다. 경고 수정은 nullable 주석·흐름만 바꾸고 실행 경로는 그대로 둔다. product payload를 바꾸므로 `docs/DEVELOPMENT_PROCEDURE.md` §10에 따라 같은 PR에서 `docs/ga-ready/release-train.json` `queue`에 한 행을 더한다. branch `lane1/build-warnings-20261005` 하나, PR 하나.

**Tech Stack:** `git`, `dotnet build`/`dotnet test`, `PcvReleaseTrainContractTests`

## 사용자 결정 (2026-10-05)

승인 원문: `3` (2026-10-05 후보 보고의 1 "빌드 경고 정리", 2 "로컬 branch 정리"를 둘 다 하라는 답. 2를 먼저 확인한다).

| 항목 | 범위 |
| --- | --- |
| 1 | 빌드 경고 정리. CS8622 `src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.Media.cs`, CS8600 `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmProvider.cs`. 동작 변경 없음, release-train `queue` 행 1개. Lane 1, host mutation 없음. task마다 로컬 commit, 마지막에 push, PR, green CI 뒤 merge |
| 2 | 로컬 branch 정리. merge되지 않은 로컬 branch `codex/04275-stage1-immutable-preflight`, `docs/train-phase3c-procedure-20261004`와 별도 clone `D:\data\projects\codex-zone\pcv-04286-fullshard`를 확인해 보고한다. 삭제는 항목마다 별도 확인 |

## Global Constraints

- branch, clone, worktree를 지우거나 바꾸지 않는다. 확인 명령은 읽기만 한다(`git log`, `git status`, `git diff --stat`, `git branch -vv`).
- 경고 수정은 동작을 바꾸지 않는다. null 처리 경로가 바뀌어야만 경고가 없어지면 멈추고 보고한다(`new-design-required`).
- host, 설치본, service, VM을 바꾸지 않는다. `current_evidence_written=false`.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 로컬 branch와 별도 clone 확인

- [x] 두 branch의 고유 commit, 그 내용이 다른 경로로 `main`에 들어갔는지(같은 patch, 같은 파일 결과), 원격 branch 유무를 확인한다. 별도 clone은 `git status`, 현재 branch, 원격에 없는 commit을 확인한다. 항목마다 "지워도 됨 / 보존 / 판단 필요"와 근거를 실행 기록에 적고, 지우는 일은 `next_approval_required`로 넘긴다. 검증 `git diff --check`. 로컬 commit.

실행 기록(2026-10-05, `git fetch --prune` 뒤):

| 항목 | 상태 | 판정 |
| --- | --- | --- |
| `docs/train-phase3c-procedure-20261004` | 로컬과 원격에 있다. 고유 commit `bdd3f5b` 1개(base `48f7b3b`, 문서 `6`개). **열린 PR #43**(2026-10-04T11:58Z) | 지워도 됨. 같은 3c 변경이 PR #44(`2c0a4c2`)와 상태 현행화 PR #46(`2013eb4`)으로 이미 `main`에 있다. PR #43 close와 원격 branch 삭제는 외부 변경이라 별도 승인 |
| `codex/04275-stage1-immutable-preflight` | 로컬만 있다(원격 없음). 2026-08-27 고유 commit `3`개: dual-hash preflight 설계·계획과 `CampaignToolingIntegrity.cs`(`508`줄)·시험(`341`줄), 합계 `1305`줄 | 판단 필요. 08-27 lane 분리 설계가 campaign 상태기계와 dual-hash Stage 1을 완료하지 않는다고 적었고 04275는 다른 경로로 승격됐다. 다른 곳에 없는 작업이므로 지운다면 `git bundle`로 보존한 뒤 지운다 |
| 별도 clone `D:\data\projects\codex-zone\pcv-04286-fullshard` | 경로가 없다 | 조치 없음. 이미 정리됐다 |

삭제는 이 campaign 밖이며 campaign `next_approval_required`에 적었다.

## Task 2: 빌드 경고 정리

- [ ] CS8622와 CS8600을 동작 변경 없이 없앤다. 검증 `dotnet build src/DesktopNode.sln -c Release --no-incremental`에서 경고 `0`, `dotnet test src/DesktopNode.Api.Tests -c Release`, `dotnet test src/DesktopNode.HyperV.Tests -c Release`. 로컬 commit.

## Task 3: release train 대기열 행

- [ ] `docs/ga-ready/release-train.json` `queue`에 Task 2 변경 한 행을 더한다(`merge_commit`은 Task 2 commit SHA). 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`(`PcvReleaseTrainContractTests` 포함). 로컬 commit.

## Task 4: 종료 검증과 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. campaign을 닫고(`next_approval_required`에 Task 1 삭제 후보) 로컬 commit한 뒤 push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.89-admin-smoke` 그대로다. 이 campaign은 train을 출발시키지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
