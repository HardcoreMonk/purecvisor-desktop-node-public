# 프로젝트 완료 정의와 SERVICE_PLAN 현황 감사 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** "프로젝트 100% 완료"를 측정할 수 있는 조건으로 정하고, `docs/SERVICE_PLAN.md` §7.1 P0~P2 `15`개 항목과 완료 층 네 개의 현재 상태를 근거와 함께 대조해 남은 일을 정확히 적는다.

**Architecture:** 완료 층은 ADR-0004(제품 런타임 GA-ready), operational current(`docs/ga-ready/current-evidence.json`), ADR-0015(기능 승격), SERVICE_PLAN §9(서비스 기획 성공 기준)다. 감사는 읽기만 하고, 결정 문서는 새 spec으로 쓴다. branch `lane1/completion-definition-20261006` 하나, PR 하나.

**Tech Stack:** 문서(`docs/SERVICE_PLAN.md`, `docs/FEATURE_IMPLEMENTATION_LEDGER.md`, `docs/ga-ready/**`, `config/desktop-node-feature-*.json`, `docs/adr/**`), Delivery 계약 시험

## 사용자 결정 (2026-10-06)

승인 원문: `1, public trusted signing, 외부 stable 배포는 없음` ("프로젝트 100퍼센트 완료의 조건" 답변의 다음 승인 1에 대한 답).

| 항목 | 범위 |
| --- | --- |
| 1 | "완료 정의" 결정 문서와 SERVICE_PLAN P0~P2 `15`개 항목별 현황 감사. Lane 1 문서, host mutation 없음. task마다 로컬 commit, push, PR, green CI 뒤 merge |
| 결정 | public trusted signing과 external stable publication은 하지 않는다. 완료 정의의 영구 범위 밖이며 남은 일로 세지 않는다(ADR-0004, ADR-0006과 같다) |

## Global Constraints

- 읽기만 하는 감사다. host, 설치본, service, VM, package를 바꾸지 않는다. `current_evidence_written=false`.
- 항목 상태는 evidence id, ADR, 코드 경로 같은 근거가 있을 때만 쓴다. 근거를 찾지 못한 항목은 `확인 못 함`으로 남긴다.
- 기존 evidence와 SERVICE_PLAN 본문은 덮어쓰지 않는다. 필요한 연결은 새 문서와 index 줄로 한다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 현황 감사

- [x] SERVICE_PLAN §7.1 P0~P2 `15`개 항목과 완료 층 네 개를 항목마다 대조해 `docs/project-status-audit-2026-10-06.md`에 쓴다. 항목마다 상태(`설치본 actual-VM PASS`, `code-level만`, `정책상 닫힘`, `미착수`, `확인 못 함`)와 근거를 적는다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-06): `docs/project-status-audit-2026-10-06.md`. 완료 층 세 개(GA-ready 런타임, operational current `0.42.90`, 기능 승격 후보 `4/4`)는 닫힘. P0~P2 `15`개는 모두 설계가 구현됐고(`implemented-slice-*`), 설치본 PASS `10`, 부분 `1`(P1-10), code-level만 `4`(P1-6, P1-7, P1-9, P2-11), 미착수 `0`. 기한 위험은 2026-10-19 Ubuntu 26 runner 확인 하나. Delivery `763` 통과.

## Task 2: 완료 정의 결정 문서

- [x] `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-completion-definition-design.md`에 측정 가능한 완료 조건, 영구 범위 밖(public trusted signing, external stable publication 등), Task 1 감사로 본 남은 일 목록을 적는다. `docs/DEVELOPER_INDEX.md`와 `docs/DOCUMENTATION_INDEX.md`에 연결한다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-06): 완료 정의 `pcv-project-completion-definition-v1`. 조건 C1(GA-ready 런타임 유지), C2(operational current가 마지막 payload, queue 비어 있음), C3(후보 승격), C4(SERVICE_PLAN `15`개 항목 설치본 evidence 또는 불필요 결정), C5(`main` Required CI green, 기한 위험 없음), C6(영구 범위 밖은 세지 않음). 2026-10-06 판정은 C1·C2·C3·C6 충족, C4·C5 미충족. 남은 일은 Lane 2 설치본 probe 두 묶음과 10-19 runner 확인이다. `DEVELOPER_INDEX`와 `DOCUMENTATION_INDEX`에 연결했다.

## Task 3: 종료 검증과 merge

- [x] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. campaign을 닫고(`next_approval_required`에 남은 일 승인 후보) 로컬 commit한 뒤 push, PR, green CI 뒤 merge.

실행 기록(2026-10-06, clean HEAD `22d195b`): Release build 경고 `0`. `dotnet test src/DesktopNode.sln -c Release` 실패 `0`(Verification `613`, Delivery `763`, Api `488`, HyperV `255`, Host `216`, Contracts `200`, Cli `183`, Runtime `129`, Service `11`). Pester 네 종 실패 `0`(`528`, `49`, `50`, `129`). `npm run test:required --prefix web` exit `0`. Required CI 네 shard 모두 `ok=true`, `plan_only=false`. 이 기록과 campaign 닫기 commit 뒤 push, PR, green CI 뒤 merge한다.

## Nonclaims

- operational current는 `0.42.90-admin-smoke` 그대로다. 이 문서들은 GA를 선언하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
