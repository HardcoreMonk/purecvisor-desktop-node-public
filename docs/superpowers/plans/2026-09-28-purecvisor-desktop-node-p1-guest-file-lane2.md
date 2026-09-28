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

- [x] 착수 시 제품 전원 명령(`vm start`/`vm stop` 계열)과 `guest-exec`/`guest-agent-ensure-channel` CLI 인자, job 결과의 stdout digest 필드를 확인한다.
- [x] 설계 §4 slice를 구현한다. guest 접두사를 만들지 않는다.
- [x] DryRun, RuntimeAdapter Pester(PASS, 접두사 부재로 copy 실패, 정지 실패), C# 계약 테스트.

실행 기록(2026-09-28): 전원은 `vm start`, 복귀는 `vm shutdown` 뒤 `Off`가 안 되면 `vm poweroff`(둘 다 제품 명령). guest 확인·정리는 `vm guest-exec -- powershell.exe -NoProfile -NonInteractive -Command <script>`이고 판정은 job 결과 `stdout_digest`(stdout UTF-8 SHA-256)를 `<값>`+CRLF/LF/무종결의 digest와 비교한다. transport가 `Out-String`으로 출력을 묶기 때문이다. Hyper-V 전원·heartbeat는 WMI(`EnabledState`, `Msvm_HeartbeatComponent`)로 읽는다. `preflight`는 읽기만 하고 host staging은 별도 `host_staging` slice로 뺐다(설계 표 갱신). 읽기 전용 확인: 설치본 `vm guest-exec --dry-run`이 이 argv와 credential-ref를 받는다(`host_mutation_performed=false`, VM `Off` 유지). Pester `6/6`(PASS, 접두사 부재 copy 실패, 비보존 VM 거절, 이미 Running, shutdown 실패 시 poweroff), Delivery `715/715`.

## Task 3: Lane 2 run (별도 승인)

- [x] 설치본에서 한 run. evidence 문서 새 파일. copy 실패가 제품 결함이면 Lane 1 수정 task를 더한다.

실행 기록(2026-09-28): 승인 `guest file run 승인, host mutation 승인`. copy 전 slice는 모두 PASS(시작·heartbeat·channel·거절 셋·preview). guest 접두사가 없어 copy job이 `PCV_GUEST_FILE_COPY_FAILED`(`RemotePathNotFound`)로 FAIL. cleanup PASS, VM `Off` 복귀. `docs/ga-ready/evidence/service-plan-p1-guest-file-actual-vm-2026-09-28-04281.md`.

## Task 4: guest 부모 디렉터리 생성 (Lane 1)

- [ ] 제품 copier가 copy 전에 guest 경로의 부모 디렉터리를 세션 안에서 만든다. 부모는 allowlist 접두사 안이어야 하고 C#에서 확인한다.
- [ ] 단위 테스트(부모 계산·접두사 경계, bridge script 순서).

## Task 5: probe-vehicle package와 설치 (별도 승인)

- [ ] Task 4가 든 HEAD에서 다음 admin-smoke version을 빌드하고 설치한다. MSI 설치 승인 없이는 시작하지 않는다.

## Task 6: Lane 2 재실행

- [ ] 한 run, evidence 문서 새 파일.
