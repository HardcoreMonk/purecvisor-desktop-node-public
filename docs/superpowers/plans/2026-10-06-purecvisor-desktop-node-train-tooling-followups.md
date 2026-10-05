# Release train 0.42.90 후속 도구·절차 정리 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.42.90`에서 나온 후속 두 가지를 닫는다. update 패키지 도구가 `package-facts.json`의 `build_utc`를 ISO 8601로 쓰게 하고, orchestrator train의 Lane 3 consume 방식과 current-card 스크립트에서 바꿔야 할 상수를 `docs/DEVELOPMENT_PROCEDURE.md` §10에 적는다.

**Architecture:** 근거는 `docs/superpowers/plans/2026-10-05-purecvisor-desktop-node-train-04290.md` Task 1·5·7 실행 기록과 `docs/ga-ready/evidence/manual-admin-campaign-2026-10-05-04289-04290.md`다. 도구만 바꾸므로 release train `queue`에 행을 더하지 않는다. branch `lane1/train-tooling-followups-20261006` 하나, PR 하나.

**Tech Stack:** `packaging/windows-desktop-node/tools/New-PcvAdminSmokeUpdatePackage.ps1`, Delivery C# 계약 시험, `docs/DEVELOPMENT_PROCEDURE.md`

## 사용자 결정 (2026-10-06)

승인 원문: `1,2` (campaign `train-04290-20261005` `next_approval_required`의 1, 2).

| 항목 | 범위 |
| --- | --- |
| 1 | Lane 1, host mutation 없음. `New-PcvAdminSmokeUpdatePackage.ps1`이 `build_utc`를 ISO 8601로 쓰게 고치고 Delivery 계약 시험을 더한다. 도구만이라 queue 행 없음. 로컬 commit, push, PR, green CI 뒤 merge |
| 2 | Lane 1, host mutation 없음. `DEVELOPMENT_PROCEDURE.md` §10에 orchestrator train의 Lane 3 consume 방식과 current-card 스크립트의 40자리 `provenance_commit` 상수를 적는다. push, PR, green CI 뒤 merge |

## Global Constraints

- 0.42.89·0.42.90 package artifact, facts, evidence를 바꾸지 않는다. 도구 확인 실행은 새 출력 root(`artifacts/update-package-check-20261006/`)에 쓴다.
- 새 packaging `*.Tests.ps1`을 만들지 않는다. 시험은 Delivery C# 계약으로 쓴다.
- host, 설치본, service, VM을 바꾸지 않는다. `current_evidence_written=false`.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: `build_utc` ISO 8601

- [x] `New-PcvAdminSmokeUpdatePackage.ps1`이 provenance의 build 시각을 culture와 무관한 ISO 8601(UTC, `Z`)로 `package-facts.json`에 쓰게 고친다. Delivery 계약 시험이 그 형식 token을 본다. 0.42.90 package로 새 출력 root에 실행해 값 형식을 확인한다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

실행 기록(2026-10-06): provenance 파일에는 `2026-10-05T12:07:30.9458397Z`로 있었다. pwsh 7의 `ConvertFrom-Json`이 이 문자열을 `DateTime`으로 바꾸고 `[string]`이 culture 형식으로 찍은 것이 원인이다. 도구는 이미 pwsh 전용(`-Depth`)이고 이 호스트 pwsh는 `7.6.6`이라 `Read-PcvJson`에 `-DateKind String`을 더했다. 계약 시험이 그 token과 `build_utc` 줄을 본다. 0.42.90 package를 새 출력 root `artifacts/update-package-check-20261006/20261005-04290`로 다시 돌려 `build_utc`가 `2026-10-05T12:07:30.9458397Z`이고, 다른 facts 값은 같음을 확인했다(catalog SHA는 생성 시각·경로 때문에 다름). Delivery `763` 통과.

## Task 2: §10 orchestrator Lane 3 절차

- [x] §10에 orchestrator train Lane 3 consume(닫을 때 만든 `<campaign>-closed` descriptor를 consume descriptor로 쓰고, 그 descriptor가 읽은 summary `7`개를 `consume-manifest.json`에 SHA-256과 함께 적는다. 복사 없음)과 current-card 캡처 스크립트에서 바꿀 값(root, evidence id, fullgate batch, 64자리 SHA 상수 `4`개, 40자리 `provenance_commit`, 설치 manifest version, `canonical_current_evidence`)을 적는다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-06): §10 train task 2 bullet 뒤에 두 bullet을 더했다. orchestrator train Lane 3 consume은 `<campaign>-closed` descriptor와 `consume-manifest.json`(summary `7`개, 복사 없음)으로 하고, current-card 스크립트는 40자리 `provenance_commit`을 포함한 상수를 바꿔 root 밖에서 실행한다. §10 문구를 pin한 계약 시험은 없다. Task 1 commit 뒤 작업 트리의 `.ps1` 줄바꿈을 CRLF로 맞췄다(저장소 blob은 그대로 LF 정규화).

## Task 3: 종료 검증과 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. campaign을 닫고 로컬 commit한 뒤 push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.90-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
