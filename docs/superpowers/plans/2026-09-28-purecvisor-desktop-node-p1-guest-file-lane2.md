# P1-8 guest file Lane 2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** P1-8 guest file을 보존 Windows guest(`pcv-guest-installed-04253-r1`)에서 설치본 actual-VM으로 검증한다.

**Architecture:** 설계 `docs/superpowers/specs/2026-09-28-purecvisor-desktop-node-p1-guest-file-lane2-probe-design.md`. 한 task가 한 checkpoint다. Lane 1은 30분/tool batch 18회, Lane 2는 45분/tool batch 12회.

**Tech Stack:** PowerShell 7 + Pester 5, C# / .NET 10 xUnit, PCVCLI, Hyper-V PowerShell Direct

## 사용자 결정 (2026-09-28)

| 항목 | 결정 |
| --- | --- |
| 다음 작업 | P1-8 guest file actual-VM 설계 먼저 |

## Global Constraints

- VM 생성·삭제, Notes·credential 변경, MSI 설치를 하지 않는다.
- Lane 2 run은 별도 승인(보존 VM 시작·정지, guest 파일 쓰기·삭제, host staging 파일) 없이 시작하지 않는다.
- credential·token 값은 command line, summary, evidence에 싣지 않는다.
- commit은 campaign `commit_policy`를 따른다. push/PR/merge는 사용자가 부를 때만 한다.

## Task 1: 설계

- [x] 설계 문서(대상 VM, credential-ref, slice, 예상 위험, 승인 범위).

실행 기록(2026-09-28): Lane 0 읽기 전용 관측(설치본 `0.42.81`, 보존 VM `Off`, credential 파일·host root 있음, CLI `vm guest-file` 있음). 제품 copy가 guest 부모 디렉터리를 만들지 않는 점을 예상 위험으로 적었다.

## Task 2: runner (Lane 1)

**생성:** `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1GuestFileActualVmSmoke.ps1`, `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP1GuestFileActualVmSmoke.Tests.ps1`, `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP1GuestFileActualVmSmokeContractTests.cs`

- [ ] 착수 시 제품 전원 명령(`vm start`/`vm stop` 계열)과 `guest-exec`/`guest-agent-ensure-channel` CLI 인자, job 결과의 stdout digest 필드를 확인한다.
- [ ] 설계 §4 slice를 구현한다. guest 접두사를 만들지 않는다.
- [ ] DryRun, RuntimeAdapter Pester(PASS, 접두사 부재로 copy 실패, 정지 실패), C# 계약 테스트.

## Task 3: Lane 2 run (별도 승인)

- [ ] 설치본에서 한 run. evidence 문서 새 파일. copy 실패가 제품 결함이면 Lane 1 수정 task를 더한다.
