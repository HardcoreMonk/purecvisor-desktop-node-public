# Release train Lane 3 spec 생성기 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train이 끝날 때 `lane3-promotion-docs-spec-<tag>.json`을 직전 spec과 train 산출물(train facts, `docs/ga-ready/release-train.json`, `docs/ga-ready/current-evidence.json`, Lane 3 main push 결과)에서 만든다. train마다 사람이 판단하는 값(ledger 행 문구, functional note 등)만 별도 값으로 받는다.

**Architecture:** 3a facts 생성기(`pcvverify train-facts`) 선례를 따라 C# `DesktopNode.Verification`에 둔다(Required CI PowerShell 0 원칙). golden은 04288 spec과 0.42.89 입력으로 커밋된 04289 fixture를 byte 단위로 다시 만드는 것이다. 설계는 Task 1에서 쓴다. 연관 설계: release train `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-release-train-design.md` §9, pair orchestrator `docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-train-pair-orchestrator-design.md` §3c. branch `lane1/train-lane3-spec-20261005` 하나, PR 하나.

**Tech Stack:** `DesktopNode.Verification`(`pcvverify`), `DesktopNode.Verification.Tests`, `packaging/windows-desktop-node/tools/Invoke-PcvLane3PromotionDocs.ps1`(소비자, 바꾸지 않는다), `packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-<tag>.json`

## 사용자 결정 (2026-10-05)

승인 원문: `1,2` (2026-10-05 "후속 작업 개시" checkpoint 보고의 다음 승인 1, 2에 대한 답).

| 항목 | 범위 |
| --- | --- |
| 1 | train 3단계 상태 문서 현행화 commit, push, PR, green CI 뒤 merge. PR #46, merge `2013eb4`로 끝났다 |
| 2 | Lane 3 spec 생성 campaign. Lane 1, host mutation 없음. `lane3-promotion-docs-spec-<tag>.json`을 train facts와 `release-train.json`에서 만들도록 설계·구현하고, train마다 사람이 판단하는 값은 따로 받는다. task마다 로컬 commit, 마지막에 push, PR, green CI 뒤 merge |

## Global Constraints

- 기존 Lane 3 spec fixture(`04278`~`04289`), `current-evidence.json`, evidence 문서, train facts 파일을 바꾸지 않는다. golden 입력은 새 파일로 더한다.
- `Invoke-PcvLane3PromotionDocs.ps1`과 그 spec 계약 `pcv-lane3-promotion-docs-v1`은 바꾸지 않는다. 생성기는 그 계약의 spec을 출력한다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다. 시험은 C#(`DesktopNode.Verification.Tests`, 필요하면 Delivery 계약)으로 쓴다.
- host, 설치본, service, VM을 바꾸지 않는다. `current_evidence_written=false`.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 값 출처 조사와 설계

- [ ] 04289 spec의 값마다 출처를 분류한다(직전 spec에서 회전, train facts, `release-train.json`/`current-evidence.json`, Lane 3 main push 결과, 사람 판단). 04288→04289 diff로 분류를 확인하고, 설계 문서 `docs/superpowers/specs/2026-10-05-purecvisor-desktop-node-train-lane3-spec-generator-design.md`에 입력 계약, 명령 형태, golden 방법, 남는 사람 값 목록을 적는다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 2: 입력 계약과 descriptor chain 생성

- [ ] 설계의 입력 계약 파서와 descriptor chain·ledger head 값 생성을 구현하고 단위 시험을 더한다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

## Task 3: ledger 행, index 절, 명령

- [ ] ledger 행(`replace`, `supersede`)과 index 절 값 생성, `pcvverify` 명령(쓰기와 확인)을 구현하고 시험을 더한다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

## Task 4: 04289 golden

- [ ] 04288 spec과 0.42.89 입력(새 입력 파일)으로 커밋된 `lane3-promotion-docs-spec-04289.json`을 byte 단위로 다시 만드는 시험을 더한다. 맞지 않는 값은 출처 분류를 고치거나 사람 값으로 옮기고 설계에 적는다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

## Task 5: 절차 반영

- [ ] `docs/DEVELOPMENT_PROCEDURE.md` §6 문서 반영 순서와 §10 train task 7, release train 설계 §9 3단계 행, pair orchestrator 설계 §3c의 Lane 3 spec 줄을 생성기 기준으로 바꾼다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 6: 종료 검증과 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. campaign을 닫고(`next_approval_required` 포함) 로컬 commit한 뒤 push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.89-admin-smoke` 그대로다. 이 campaign은 Lane 3 승격을 하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
