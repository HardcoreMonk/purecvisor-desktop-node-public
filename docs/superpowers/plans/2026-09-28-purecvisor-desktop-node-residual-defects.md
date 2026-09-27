# P2 Off VM 후속 결함 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** P2 Off VM Lane 2 campaign(`p2-offvm-lane2-20260927`)이 report-only로 남긴 결함을 host 변경 없이 Lane 1에서 고친다.

**Architecture:** 한 task가 한 checkpoint(Lane 1, 30분, tool batch 18회)다. 설치본, Hyper-V, current-evidence는 바꾸지 않는다.

**Tech Stack:** C# / .NET 10 xUnit, PowerShell 7 + Pester 5

## 사용자 결정 (2026-09-28)

| 항목 | 결정 |
| --- | --- |
| 다음 작업 | 남은 결함 Lane 1 campaign(route DVD guard, External switch 분류, 기존 packaging Pester 실패 2건) |

## Global Constraints

- host mutation, MSI, Lane 2/3, current-evidence는 이 계획 밖이다.
- `config/pcv-development-policy-contract-spec-v1.json`의 pin 파일을 바꾸면 spec SHA와 `ExpectedSpecSha256`을 같은 task에서 갱신한다.
- 바뀐 파일이 module size ratchet 상한을 넘으면 상한을 올리지 않고 나눈다.
- commit은 campaign `commit_policy`를 따른다. push/PR/merge는 사용자가 부를 때만 한다.

## Task 1: 기존 packaging Pester 실패 2건

**수정:** `packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1`, `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`

- [x] workflow step 기대값을 제품 계약(`RequiredCiPolicy.cs`, C# 계약 테스트, workflow)의 `Run installer and policy shard`에 맞춘다. `RequiredCiPolicyTests`는 `Run installer-policy shard`를 decoy로만 쓴다.
- [x] case-only blocked 테스트가 live `current-evidence.json`(`promotion_eligible=true`)에 기대지 않게 candidate에 `promotion_eligible=false`와 blocker를 넣고, case-only version을 live version의 대문자로 만든다.
- [x] 두 Pester 파일 통과, `packaging/windows-desktop-node/tests` 전체 실패 `0`.

실행 기록(2026-09-28): workflow step 이름은 2026-08-26 cut-over(`68756f1`)에서 바뀌었고 Pester 기대값만 남아 있었다. case-only 테스트는 live record의 `promotion_eligible`에 의존해 promotion 검사를 건너뛰고 record 검증(`PCV_CURRENT_EVIDENCE_INVALID|current.version`)에서 멈췄다. 이제 candidate가 자기 blocker를 가져 case-sensitive promotion 검사가 `PCV_FEATURE_PROMOTION_BLOCKED`로 거절한다. 나머지 기대 token은 workflow에 모두 있다. `packaging/windows-desktop-node/tests` `528/528`. `PcvDevelopmentGateWorkflow.Tests.ps1`은 spec `legacy_files` pin이라 SHA `46084abb` → `e204a79b`, `ExpectedSpecSha256` `4ec4eec3` → `38af10eb`(contract·Should 수는 그대로). Delivery.Tests `712/712`.

## Task 2: External switch 분류

**수정:** `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs`, `src/DesktopNode.HyperV.Tests`

- [ ] external binding이 있는 switch는 `external`, `AllowManagementOs`는 management port 유무, `NetAdapterInterfaceDescription`은 `Msvm_ExternalEthernetPort` 이름으로 채운다. 이름을 못 읽으면 지금처럼 `unknown`(topology 불완전)으로 둔다.
- [ ] 단위 테스트. 이 호스트에 External switch를 만들지 않는다(host network 변경 금지, nonclaim).

## Task 3: route DVD guard

**수정:** inventory VM 정보(HyperV provider/models), `DesktopNodeApiVmMutationRouteHandler.Devices.cs`, 테스트

- [ ] 착수 시 `vm.list` 응답 필드를 고정하는 계약·fixture·Web parity를 확인한다.
- [ ] inventory가 DVD drive 수를 내고 route guard가 그 값을 쓴다. 제품 VM의 DVD 추가 요청이 job 대신 route에서 `PCV_VM_DEVICE_ALREADY_PRESENT`로 거절된다.
- [ ] Api/HyperV 테스트.

## Task 4: 종료 검증

- [ ] `dotnet test src/DesktopNode.sln` 실패 `0`(clean HEAD).
- [ ] Pester `packaging/windows-desktop-node/tests`, `manual-admin-tests`.
- [ ] `git diff --check`.
