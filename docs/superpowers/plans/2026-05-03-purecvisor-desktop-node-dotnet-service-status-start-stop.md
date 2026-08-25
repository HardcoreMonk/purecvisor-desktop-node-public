# PureCVisor Desktop Node .NET Service Status Start Stop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `DesktopNode.Host.exe service-action status|start|stop`을 C# 제품 operation으로 추가해 service status/start/stop을 외부 process 없이 실행한다.

**Architecture:** 이 slice 자체는 MSI install/repair/uninstall lifecycle을 바꾸지 않았다. 최초 구현 시점에는 `configure-installed`, `repair-installed`, `remove-installed`는 기존 command plan을 유지하고, 새 `status`, `start`, `stop` action만 native Windows service controller abstraction으로 처리했다. 후속 2026-05-04 service/data-root product ops slice에서 `configure-installed|repair-installed|remove-installed`도 native SCM controller로 전환됐고, remove-data는 `remove-installed` handoff와 별도 `data-root-remove` action으로 분리됐다. 후속 상세 문서는 `docs/superpowers/plans/2026-05-04-purecvisor-desktop-node-data-root-remove-handoff.md`를 따른다.

**Tech Stack:** .NET 10, C#, xUnit, Windows SCM API boundary, existing DesktopNode.Host service-action JSON contract.

---

## 범위

- 포함:
  - `DesktopNode.Host.exe service-action status`
  - `DesktopNode.Host.exe service-action start`
  - `DesktopNode.Host.exe service-action stop`
  - owned service identity check before mutating start/stop
  - fake controller 기반 unit tests
- 제외:
  - MSI custom action 변경
  - service install/create/configure/delete native 전환
  - product wrapper PowerShell plan 제거
  - service install/create/configure/delete native 전환 smoke

## Task 1: service-action parse와 native service operation contract

**Files:**

- Modify: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [x] Add failing parse test for `service-action status`.
- [x] Add failing fake-controller tests for `status`, `start`, `stop`.
- [x] Verify targeted tests fail for missing support.
- [x] Implement minimal native operation branch and controller abstraction.
- [x] Verify targeted tests pass.

## Task 2: native Windows SCM controller

**Files:**

- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [x] Add tests that service start/stop rejects missing or foreign service snapshots before mutation.
- [x] Implement SCM controller behind `IDesktopNodeWindowsServiceController` without shell execution.
- [x] Keep tests injected so local verification does not mutate the host.
- [x] Verify `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter DesktopNodeHostServiceActionTests` passes.

## Task 3: docs/matrix state update

**Files:**

- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- Modify: `follower.md`
- Modify: `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`

- [x] Mark service status/start/stop rows as `dotnet-native` or slice-specific current owner only after code-level tests pass.
- [x] Keep install/repair/uninstall rows blocked.
- [x] Record code-level/unit-test evidence first, then update rows to `current-native` after installed service-action status/start/stop smoke evidence exists.

## Verification

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

## Verification Result

- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostServiceActionTests"`: PASS, 14 passed.
- `dotnet test src/DesktopNode.Service.Tests/DesktopNode.Service.Tests.csproj --filter ServiceLifecycleAdapterContractTests`: PASS, 6 passed.
- `dotnet test src/DesktopNode.sln`: PASS, 140 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: PASS, 17 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: PASS, 100 passed.
- `git diff --check`: PASS, exit 0.

이 slice의 최초 완료 시점은 code-level/unit-test evidence였다. 후속 explicit admin opt-in smoke `artifacts/service-action-status-start-stop-20260504-002359`에서 installed `DesktopNode.Host.exe service-action status/stop/start/status`가 모두 exit `0`으로 통과했고, service owner verified, stopped/running state observation, restart 후 runtime policy health `200`, final service `Running`, boot time unchanged를 확인했다. 이 evidence 이후 route matrix의 `service status`, `service start`, `service stop` row는 `current-native`로 승격됐다.
