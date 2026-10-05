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

- [x] 04289 spec의 값마다 출처를 분류한다(직전 spec에서 회전, train facts, `release-train.json`/`current-evidence.json`, Lane 3 main push 결과, 사람 판단). 04288→04289 diff로 분류를 확인하고, 설계 문서 `docs/superpowers/specs/2026-10-05-purecvisor-desktop-node-train-lane3-spec-generator-design.md`에 입력 계약, 명령 형태, golden 방법, 남는 사람 값 목록을 적는다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-05): 04289 spec 말단 값 `103`개를 04288과 비교했다(same `21`, rotated `4`, new `78`). facts 파일이 Lane 3 문서 세 개(functional carry-forward, pair consume, main push)까지 문서 `12`개를 가지므로 경로, batch id, SHA-256, run/job id 대부분이 facts에서 나온다. 사람 판단 값은 ledger 행 `rule` `7`개, `functional_note`, `updated_at`의 `9`개로 남는다. `release-train.json`은 값 출처가 아니라 train 행과 상태 확인에 쓴다. 설계 `docs/superpowers/specs/2026-10-05-purecvisor-desktop-node-train-lane3-spec-generator-design.md`(`pcv-train-lane3-spec-generator-v1`)에 입력 계약 `pcv-train-lane3-spec-input-v1`, 명령 `pcvverify lane3-spec --input (--write|--check)`, golden 방법을 적었다.

## Task 2: 입력 계약과 descriptor chain 생성

- [x] 설계의 입력 계약 파서와 descriptor chain·ledger head 값 생성을 구현하고 단위 시험을 더한다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

실행 기록(2026-10-05): `TrainLane3SpecInput.cs`(입력 계약 `pcv-train-lane3-spec-input-v1`, tag 규칙 `0.42.89` → `04289`, 경로·tag·사람 값 검사)와 `TrainLane3SpecBuilder.cs`(descriptor chain 값 `45`개와 ledger head, `release-train.json` train 행 확인, facts 일관성 검사)를 더했다. 시험 `12`개가 04288 spec과 0.42.89 facts로 만든 descriptor chain과 ledger head가 커밋된 04289 spec과 같음을 본다. Verification `605` 중 dirty tree 전용 `PolicyBoundaryMatchesCanonicalActivationState` `1`개만 실패했고 commit 뒤 clean HEAD에서 다시 본다. Delivery `763` 통과(모듈 크기 라쳇 포함).

## Task 3: ledger 행, index 절, 명령

- [x] ledger 행(`replace`, `supersede`)과 index 절 값 생성, `pcvverify` 명령(쓰기와 확인)을 구현하고 시험을 더한다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

실행 기록(2026-10-05): `TrainLane3SpecBuilder.Rows.cs`에 ledger 행 `8`개(status·evidence는 facts로 채우는 문장 틀, closed pair candidate `rule`은 상수), index 절, 직렬화(들여쓰기 2칸, escape 없는 UTF-8, LF, 끝 줄바꿈)를 더했다. functional carry-forward predecessor(04275 evidence, summary, SHA-256)는 facts에 없어 직전 spec 행의 `predecessor` 이후를 이어 받는다. `TrainLane3SpecCommand.cs`가 `pcvverify lane3-spec --input (--write|--check)`(결과 계약 `pcv-train-lane3-spec-result-v1`)이고 `VerificationApplication`에 등록했다. 시험 `16`개(ledger 행·index 절 04289 대조, 임시 저장소 missing→written→current, usage 오류) 통과.

## Task 4: 04289 golden

- [x] 04288 spec과 0.42.89 입력(새 입력 파일)으로 커밋된 `lane3-promotion-docs-spec-04289.json`을 byte 단위로 다시 만드는 시험을 더한다. 맞지 않는 값은 출처 분류를 고치거나 사람 값으로 옮기고 설계에 적는다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`. 로컬 commit.

실행 기록(2026-10-05): 입력 `docs/ga-ready/trains/0.42.89-admin-smoke.lane3-spec-input.json`(사람 값 `9`개는 커밋된 04289 spec에서 옮겼다)으로 `lane3-spec --check`가 첫 실행에서 `status=current`였다. 04289 fixture를 byte 단위로 다시 만든다. `TrainLane3SpecGoldenTests`가 커밋된 `*.lane3-spec-input.json`마다 같은 확인을 한다. 설계 §2에 functional predecessor를 직전 spec 회전으로 고친 보정을 적었다.

## Task 5: 절차 반영

- [ ] `docs/DEVELOPMENT_PROCEDURE.md` §6 문서 반영 순서와 §10 train task 7, release train 설계 §9 3단계 행, pair orchestrator 설계 §3c의 Lane 3 spec 줄을 생성기 기준으로 바꾼다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 6: 종료 검증과 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. campaign을 닫고(`next_approval_required` 포함) 로컬 commit한 뒤 push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.89-admin-smoke` 그대로다. 이 campaign은 Lane 3 승격을 하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
