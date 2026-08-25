# Read Route Helper Fallback Removal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tier 1 read-only product routes stop falling back to the PowerShell Hyper-V helper when native C# read parity is incomplete.

**Architecture:** `DesktopNodeApiRequestProcessor` keeps native adapter dispatch for `host.status`, `network.inventory`, `vm.list`, and `checkpoint.list`, but returns the native adapter's structured success or failure directly. Known native read operations become authoritative product route handlers; the PowerShell helper remains only for queued mutation job execution in this slice.

**Tech Stack:** C#/.NET 10, xUnit, Markdown route matrix, Pester documentation guards.

---

## Status

- Date: 2026-05-03
- Implementation status: completed
- Admin opt-in: not required for the original no-helper read-route slice. 2026-05-03 사용자 승인 후 별도 설치본 service/MSI/Hyper-V mutation smoke를 실행했다.
- GA state: Tier 1 read routes can move to `current-native`/`fallback_policy = none`; Tier 2/Tier 3 mutation routes remain `transition-helper`.

## Files

- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Replace fallback expectations for read routes with no-helper structured native failure expectations.
  - Keep queued mutation worker helper-dispatch tests unchanged.
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - Return native read operation results directly even when `TryInvoke` reports `handled = false`.
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
  - Return `handled = true` for known read operation parity failures.
  - Keep `handled = false` only for unsupported operations.
- Modify docs:
  - `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
  - `docs/DEVELOPER_INDEX.md`
  - `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`
  - `packaging/windows-desktop-node/README.md`
  - `spikes/purecvisor-desktop-node/README.md`
  - `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

## Tasks

### Task 1: RED - read routes must not invoke helper fallback

- [x] Change xUnit tests so incomplete native read results return native structured errors and leave helper calls empty for:
  - `GET /api/v1/host/status`
  - `GET /api/v1/network/inventory`
  - `GET /api/v1/vms`
  - `GET /api/v1/vms/{id}`
  - `GET /api/v1/vms/{id}/checkpoints`
- [x] Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "HostStatusRouteReturnsNativeFailureWithoutPowerShellFallback|NetworkInventoryRouteReturnsNativeFailureWithoutPowerShellFallback|VmListRouteReturnsNativeFailureWithoutPowerShellFallback|VmDetailRouteReturnsNativeFailureWithoutPowerShellFallback|CheckpointListRouteReturnsNativeFailureWithoutPowerShellFallback"
```

Expected before implementation: FAIL because current request processor still falls back to `IDesktopNodeHyperVHelper`.

### Task 2: GREEN - make native read routes authoritative

- [x] Update `InvokeHyperVOperation` so read candidates return `nativeAdapter.TryInvoke(...)` result directly and never call `helper.Invoke(...)`.
- [x] Update native adapter parity guard methods to return `true` with structured failure for known read operation failures.
- [x] Run the focused xUnit filter from Task 1 and confirm PASS.

### Task 3: Docs and verification

- [x] Update route matrix Tier 1 read rows to `fallback_policy = none`, `promotion_state = current-native` where product fallback is removed.
- [x] Keep mutation rows as `dotnet-request-processor-powershell-helper` with `fallback_policy = transition-helper`.
- [x] Run:

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj
dotnet test src\DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: all pass. Actual evidence is appended after execution.

## Completion Evidence

- RED: focused xUnit filter failed as expected before implementation. Failures showed read routes still returned helper success responses and native parity guards still returned `handled=false`.
- GREEN: focused xUnit filter passed after removing read-route helper fallback and making known native read parity failures structured handled failures. Result: 8 passed, 0 failed.
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj`: 59 passed.
- `dotnet test src\DesktopNode.sln`: 104 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 17 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 100 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 35 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`: 97 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"`: 21 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"`: 13 passed.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"`: 50 passed, 1 not run.
- `npm test --prefix web`: passed, including `check:served`.
- `npm run verify:parity --prefix web`: passed, including browser fixture.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`: 20 passed.
- `node --check web/app.js`: passed.
- `node --check web/scripts/build-served-asset.mjs`: passed.
- `git diff --check`: passed with line-ending normalization warnings only.

## Post-Plan Admin Opt-In Evidence

- 2026-05-03 사용자 승인 후 실제 service/MSI/Hyper-V mutation smoke를 실행했다.
- 실패 원인으로 드러난 native WMI parity 결함 4개를 TDD로 고정하고 수정했다:
  - `Default Switch`의 `allow_management_os` parity 필드 누락
  - 지역화된 `Msvm_ComputerSystem.Caption` 필터 의존
  - projected WMI object로 association traversal을 수행하던 VM/checkpoint lookup
  - `byte * 2^20`처럼 공백을 포함한 Hyper-V memory unit 처리 누락
- 최종 성공 evidence:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1' -Version '0.28.1-admin-smoke' -IsoPath 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso' -ArtifactRoot 'artifacts\routeparity-service-msi-hyperv-mutation-20260503-152658-0281'
```

- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-152658-0281/summary.json`: `ok=true`, `boot_time_unchanged=true`, `remaining_pcv_vms=[]`.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260503-152658-0281/hyperv-api-route-smoke.json`: `host.status`, `network.inventory`, `vm.create`, `vm.list`, `vm.get`, `vm.start`, `checkpoint.create`, `checkpoint.list`, `checkpoint.delete`, `vm.poweroff`, cleanup 통과.
- 최종 service 상태: `PureCVisorDesktopNode` Running/Auto, `DesktopNode.Host.exe listen ...`.
- 최종 검증:
  - `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj`: 63 passed.
  - `dotnet test src\DesktopNode.sln`: 108 passed.
  - `git diff --check`: passed with line-ending normalization warnings only.
