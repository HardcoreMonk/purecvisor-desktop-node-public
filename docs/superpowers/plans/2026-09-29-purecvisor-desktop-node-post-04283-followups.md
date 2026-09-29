# 0.42.83 승격 뒤 후속 작업 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 0.42.83 Lane 3(PR #18, merge `dd638d4`)가 남긴 후속 세 가지를 처리한다.
1. merge 뒤 문서 권위 줄 갱신
2. 미추적 pair orchestrator 반입
3. clean-host base VHD 오프라인 갱신 설계

**Architecture:** 한 task가 한 Lane 1 checkpoint다(30분, tool batch 18회). 브랜치 `docs/post-04283-followups-20260929`에서 커밋마다 push하고, 끝에 PR 하나로 올린다.

**Tech Stack:** Markdown 문서, PowerShell 7 + Pester 5, C# / .NET 10 xUnit

## 사용자 결정 (2026-09-29)

| 항목 | 결정 |
| --- | --- |
| 1. merge 뒤 문서 갱신과 campaign 종료 | 승인 |
| 2. `Invoke-PcvManualAdminPackagePairCampaign.ps1` 반입 | 승인 |
| 3. base VHD 오프라인 갱신 | 설계 승인. 구현은 설계 확인 뒤 |

## Global Constraints

- host mutation과 설치본 변경은 하지 않는다. 설계 task는 문서만 쓴다.
- 들여오는 스크립트는 끝까지 읽은 뒤 커밋한다. Pester는 `manual-admin-tests/`에 둔다(packaging Pester inventory 밖). required CI용 C# 정적 계약을 더한다.
- 기존 evidence는 덮어쓰지 않는다.
- public trusted signing과 external stable publication은 주장하지 않는다.

## Task 1: merge 뒤 문서 권위 줄과 campaign 종료

- [x] `DOCUMENTATION_INDEX.md`의 공개 소스 HEAD, Required CI run, Public Boundary run, 열린 campaign 줄을 `dd638d4` 기준으로 바꾼다.
- [x] `active-campaign.json`에 이 campaign을 연다. Lane 3 campaign(`lane3-04283-promotion-20260929`)은 `closed_predecessor`로 닫는다.

실행 기록(2026-09-29): HEAD는 `dd638d4`(PR #18)다. Development Gates run `36585730842`와 Public Boundary run `36585731022`(job `109465396607`)는 모두 success다. Lane 3 계획의 Task 6에 merge 기록을 남겼다.

## Task 2: pair orchestrator 반입

- [ ] `.worktrees/04277-manual-admin-promotion`의 `Invoke-PcvManualAdminPackagePairCampaign.ps1`와 그 Pester를 끝까지 읽는다. 저장소에 이미 들어온 runner와 어떻게 이어지는지 확인한다.
- [ ] 도구를 `packaging/windows-desktop-node/tools/`에, Pester를 `manual-admin-tests/`에 둔다. C# 정적 계약을 더한다.
- [ ] Pester와 Delivery.Tests를 돌린다.

## Task 3: clean-host base VHD 오프라인 갱신 설계

- [ ] 설계 문서 `docs/superpowers/specs/2026-09-29-purecvisor-desktop-node-clean-host-base-vhd-refresh-design.md`를 쓴다. 담을 내용은 다음과 같다.
  - 현재 대기 시간의 원인(0.42.83 clean-host 관측)
  - WSUS와 Connected Cache를 고르지 않은 이유
  - DISM 오프라인 서비싱 절차, base 파일 이름과 보존 규칙
  - runner와 evidence 변경, 검증과 위험, 승인 범위
- [ ] 구현은 하지 않는다.

## Nonclaims

- 이 계획은 operational current(`0.42.83-admin-smoke`)를 바꾸지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
