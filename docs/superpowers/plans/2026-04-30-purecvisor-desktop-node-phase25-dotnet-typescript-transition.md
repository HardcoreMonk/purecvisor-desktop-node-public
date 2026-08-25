# PureCVisor Desktop Node Phase 25 .NET/TypeScript Transition Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node를 PowerShell-only spike에서 C#/.NET runtime core, TypeScript Web Console, PowerShell Windows adapter 조합으로 점진 전환할 수 있는 첫 contract slice를 만든다.

**Architecture:** Phase 25는 side-by-side 전환으로 시작했고, evidence가 충분한 service host 경로만 제품 기본값으로 전환했다.

- 첫 구현은 .NET contract mirror를 추가해 Phase 24 `job_runtime` public contract를 C# 타입과 테스트로 고정했다.
- 2026-05-01 replacement slice는 `DesktopNode.Host.exe`를 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner로 승격했다.
- Route parity 시작 slice는 .NET request processor가 helper-backed routes, queued job runtime, job store save/load/recovery를 처리하게 했다.
- 2026-05-02 native adapter slice는 `host.status`에서 C# registry/WMI/service/admin read route를 사용한다.
- 2026-05-03 read-route fallback removal slice 이후 `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native read route가 직접 처리하며 helper fallback 없이 native structured success/failure를 반환한다.
- 2026-05-03 checkpoint mutation native adapter slices 이후 checkpoint create/restore/delete는 C# WMI snapshot service adapter가 직접 실행한다.
- 2026-05-03 VM power-state/checkpoint/native lifecycle/delete adapter slices 이후 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete는 C# WMI adapter가 직접 실행한다. Native VM create product path는 Hyper-V Generation 2만 지원하고 native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다.
- 2026-05-03 Web Console served asset/root migration slice 이후 TypeScript Web Console source와 npm/package verification은 repo-root `web/**`가 소유한다.
- PowerShell Local API와 Hyper-V helper는 component/adapter 경계로 유지한다.

**Tech Stack:** .NET 10, C#, xUnit, PowerShell 7, Pester 5, existing Local API JSON contracts, future TypeScript Web Console.

---

## 상태

- 작성 기준: 2026-04-30
- 현재 상태: 초기 .NET contract/runtime validator, TypeScript Web Console static parity/build verification/regeneration/user-visible fixture/browser fixture flow, .NET API/service host 및 handler/lifecycle adapter candidate slices 구현/검증 완료.
- 2026-05-01: `.NET Windows Service Host replacement` slice에서 기본 제품 service host와 MSI installed action runner를 `DesktopNode.Host.exe`로 교체했고 관리자 opt-in smoke를 완료했다.
- Route parity 시작 slice: helper-backed route parity, queued VM/checkpoint lifecycle routes, job get/cancel/retry, JSON job store save/load/recovery, Host request body/helper/job-store forwarding을 구현했다.
- 초기 Mutation route owner contract slice는 served VM/checkpoint lifecycle mutation routes를 aggregate 후보가 아니라 실제 served route 단위로 기록하고, 당시 owner를 .NET request processor queue + PowerShell helper execution boundary인 `dotnet-request-processor-powershell-helper`로 고정했다. 후속 native mutation slices 이후 current served VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete owner는 `dotnet-native`로 갱신됐다.
- Mutation dispatch helper boundary slice: queued mutation worker가 native read adapter를 probe하지 않고 PowerShell Hyper-V helper process로 직접 dispatch하도록 제한했다.
- Runtime policy dispatch boundary contract slice: `job_runtime.dispatch.native_probe_operations`와 `job_runtime.dispatch.mutation_dispatch`로 native read probe 범위와 queued mutation helper-direct dispatch를 machine-readable하게 노출했다.
- 2026-05-02 `0.26.6-admin-smoke`: tracked route parity mutation runner로 설치본 service-action/MSI lifecycle/Hyper-V API route smoke를 완료했다.
- 2026-05-02 native adapter slice: `host.status`를 registry/WMI/service/admin 기반 native 경로로 전환하고 `network.inventory`를 WMI 기반 native-first 경로로 전환했다.
- `0.26.8-admin-smoke`: `network.inventory` 포함 설치본 service/MSI/Hyper-V route smoke 완료.
- `0.26.9-admin-smoke`: switch topology parity fallback, MSI repair missing-service 재생성, shared request processor 직렬화 포함 설치본 service/MSI/Hyper-V route smoke 완료.
- `0.27.1-admin-smoke`: installed `host.status`와 `network.inventory` 설치본 smoke 완료.
- `0.27.6-admin-smoke`: runtime policy dispatch boundary contract 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke 완료. Final service `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.
- `0.28.3-admin-smoke`: checkpoint create/delete native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke 완료. Installed runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.delete]`와 `mutation_dispatch=native-checkpoint-mutation-plus-helper-process-remainder`를 보고했다.
- `0.28.6-admin-smoke`: checkpoint restore native mutation adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke 완료. Installed restore smoke는 `vm.poweroff-before-restore` 최소 안정 조건에서 `{ vm_name, name, action=restore }` payload를 확인했고 runtime policy는 `native_mutation_operations=[checkpoint.create,checkpoint.restore,checkpoint.delete]`를 보고했다.
- `0.28.8-admin-smoke`: VM start/poweroff native power-state adapter 포함 service-action, MSI lifecycle, installed Hyper-V API route smoke 완료. Installed start/poweroff smoke는 `{ name, action=start }`, `{ name, action=poweroff }` payload를 확인했고 runtime policy는 `native_mutation_operations=[vm.start,vm.poweroff,checkpoint.create,checkpoint.restore,checkpoint.delete]`와 `mutation_dispatch=native-vm-power-state-checkpoint-mutation-plus-helper-process-remainder`를 보고했다.
- VM create/shutdown/restart/delete도 후속 native lifecycle/delete slice에서 native adapter product path로 이동했다. `0.30.1-admin-smoke` 설치본 mutation smoke는 VM create/start/restart/poweroff/delete와 checkpoint create/restore/delete 성공, managed delete `action=delete`, repeat delete `action=absent`, unmanaged guard block, installer-ISO shutdown unavailable structured failure를 확인했다. Successful guest shutdown installed smoke는 `artifacts/guest-shutdown-windows-smoke-20260503-222750`에서 Microsoft Windows Server 2022 Evaluation VHD guest 기준 installed Local API `vm.shutdown` job `succeeded`, final VM `Off`, cleanup 완료로 확인했다.
- 2026-05-05 현행화: 아래 초기 .NET contract mirror의 `unsupported_future_version = quarantine-and-start-empty` snippet은 Phase 24 baseline mirror다. 현재 .NET 제품 경로의 unsupported future job-store schema는 blocked diagnostics/no-mutation/no quarantine이다. 2026-05-06 후속에서 schema v2 migration store load와 `job-store-migration-apply` code-level actual path가 추가됐다.
- 관련 설계: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`
- TypeScript Web Console 경계 설계: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md`
- .NET Windows Service Host replacement 설계: `docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md`
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- Phase 25 후보 마커: `DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first`
- Route parity 시작 마커: `DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start`

## 파일 구조

첫 구현 slice:

- Create: `src/DesktopNode.sln`
  - .NET solution root.
- Create: `src/DesktopNode.Contracts/DesktopNode.Contracts.csproj`
  - Phase 24/25 public contract model library.
- Create: `src/DesktopNode.Contracts/RuntimePolicy.cs`
  - `runtime.policy`와 `job_runtime` contract DTO, contract factory. 기존 PowerShell runtime policy와 같은 public meaning을 가진다.
- Create: `src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj`
  - xUnit test project.
- Create: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`
  - Phase 24 `job_runtime` shape와 Phase 25 managed-core candidate stance를 검증한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Phase 25 .NET/TypeScript 변경 검증 기준을 추가한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Phase 25 설계/계획 진입점을 추가한다.
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
  - Phase 25 후보 row를 추가한다.
- Modify: `follower.md`
  - 다음 작업 대기열에 Phase 25 contract mirror slice를 추가한다.

두 번째 구현 slice:

- Create: `src/DesktopNode.Runtime/DesktopNode.Runtime.csproj`
- Create: `src/DesktopNode.Runtime/JobStateTransitionPolicy.cs`
- Create: `src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj`
- Create: `src/DesktopNode.Runtime.Tests/JobStateTransitionPolicyTests.cs`
  - PowerShell Local API의 job 상태 계약을 side-by-side pure .NET library로 mirror한다.
  - 허용 전이는 `queued -> running`, `queued -> canceled`, `running -> succeeded`, `running -> failed`이다.
  - cancel은 queued-only, retry는 failed + retryable error + max attempt 3 계약만 허용한다.
  - persisted running job은 reboot/process restart 이후 `PCV_JOB_INTERRUPTED` retryable failed 상태로 복구한다.

세 번째 구현 slice:

- Create: `spikes/purecvisor-desktop-node/web/package.json`
- Create: `spikes/purecvisor-desktop-node/web/package-lock.json`
- Create: `spikes/purecvisor-desktop-node/web/tsconfig.json`
- Create: `spikes/purecvisor-desktop-node/web/src/api-types.ts`
- Create: `spikes/purecvisor-desktop-node/web/src/view-model.ts`
- Create: `spikes/purecvisor-desktop-node/web/src/app.ts`
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - TypeScript Web Console 후보는 static asset parity-first design note를 따른다.
  - 기존 `app.js`와 Local API static serving route를 즉시 대체하지 않는다.
  - TypeScript source는 Local API response type mirror와 pure view-model helper까지만 포함한다.

병렬 후속 slice:

- Create: `spikes/purecvisor-desktop-node/web/src/generate-parity-manifest.ts`
- Create: `spikes/purecvisor-desktop-node/web/generated/parity/static-asset-parity.manifest.json`
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - generated parity manifest는 기존 `app.js` served asset과 `/app.js` script source를 유지한다.
  - manifest는 route marker와 `static-asset-parity-scaffold-first` decision marker를 기록하되 token 값과 host mutation command를 포함하지 않는다.
- Create: `src/DesktopNode.Api/DesktopNode.Api.csproj`
- Create: `src/DesktopNode.Api/ApiHostCandidateContract.cs`
- Create: `src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj`
- Create: `src/DesktopNode.Api.Tests/ApiHostCandidateContractTests.cs`
- Modify: `src/DesktopNode.sln`
  - .NET API host 후보는 route/ownership/replacement stance contract만 추가한다.
  - ASP.NET server, port bind, service host replacement는 구현하지 않는다.

두 번째 병렬 후속 slice:

- Create: `spikes/purecvisor-desktop-node/web/scripts/verify-static-parity.mjs`
- Modify: `spikes/purecvisor-desktop-node/web/package.json`
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - `verify:parity` npm script는 committed generated parity manifest, TypeScript source/static serving contract, browser fixture smoke를 검증한다.
  - 실제 bundler, generated asset replacement, `index.html` script replacement는 도입하지 않는다.
- Create: `src/DesktopNode.Service/DesktopNode.Service.csproj`
- Create: `src/DesktopNode.Service/ServiceHostCandidateContract.cs`
- Create: `src/DesktopNode.Service.Tests/DesktopNode.Service.Tests.csproj`
- Create: `src/DesktopNode.Service.Tests/ServiceHostCandidateContractTests.cs`
- Modify: `src/DesktopNode.sln`
  - 초기 .NET Service host 후보는 side-by-side stance를 기록했다.
  - 2026-05-01 replacement slice에서 이 stance는 `dotnet-windows-service-host` default owner와 `replaces-winsw` product replacement stance로 갱신됐다.

세 번째 병렬 후속 slice:

- Create: `spikes/purecvisor-desktop-node/web/scripts/regenerate-static-parity.mjs`
- Modify: `spikes/purecvisor-desktop-node/web/package.json`
- Modify: `spikes/purecvisor-desktop-node/web/src/generate-parity-manifest.ts`
- Modify: `spikes/purecvisor-desktop-node/web/generated/parity/static-asset-parity.manifest.json`
- Modify: `spikes/purecvisor-desktop-node/web/scripts/verify-static-parity.mjs`
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - `generate:parity` npm script는 TypeScript AST 기반으로 side-by-side generated parity manifest를 재생성한다.
  - `verify:parity` npm script는 manifest stale 여부를 먼저 확인한 뒤 static serving contract와 Node `vm` browser fixture 검증을 실행한다.
  - 기존 `index.html`과 served `/app.js`는 변경하지 않는다.

네 번째 병렬 후속 slice:

- Create: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
- Create: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
- Create: `src/DesktopNode.Service/ServiceLifecycleAdapterContract.cs`
- Create: `src/DesktopNode.Service.Tests/ServiceLifecycleAdapterContractTests.cs`
- Create: `spikes/purecvisor-desktop-node/web/src/user-visible-fixtures.ts`
- Modify: `spikes/purecvisor-desktop-node/web/src/generate-parity-manifest.ts`
- Modify: `spikes/purecvisor-desktop-node/web/generated/parity/static-asset-parity.manifest.json`
- Modify: `spikes/purecvisor-desktop-node/web/scripts/regenerate-static-parity.mjs`
- Modify: `spikes/purecvisor-desktop-node/web/scripts/verify-static-parity.mjs`
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - .NET API handler adapter 후보는 route/method/auth/mutation stance/helper operation mapping만 기록한다.
  - .NET service lifecycle adapter 후보는 product wrapper delegation, explicit remove-data opt-in, protected token file stance만 기록한다.
  - TypeScript user-visible fixture parity는 empty/running/unsupported dashboard snapshot을 source/test scaffold로만 고정한다.
  - ASP.NET server, port bind, SCM 호출, service install/start/stop/delete, MSI, firewall, Event Log, Hyper-V, reboot mutation은 구현하지 않는다.

후속 slice 후보:

- Expand .NET API native route parity beyond `host.status` and the guarded `network.inventory` read adapter only after keeping automatic reboot disabled and preserving the PowerShell adapter fallback baseline.
- Extend TypeScript Web Console parity beyond the completed Node `vm` DOM/browser fixture only after preserving existing static serving and token redaction guarantees. Playwright and real browser/dev server evidence remain follow-up candidates.
- 제품 실행 경로 교체는 2026-05-01 `.NET Windows Service Host replacement` slice에서 `DesktopNode.Host.exe listen`과 `service-action` runner로 진행됐다.
- 이 slice는 `HttpListener`, default SCM service executable, MSI installed custom action, port bind를 교체했다.
- 후속 route parity 시작 slice는 helper-backed routes와 queued job runtime을 .NET request processor에 추가했다.
- 2026-05-02 `host.status` native adapter slice는 registry/WMI/service/admin read-only 조회만 수행한다.
- `network.inventory` native adapter slice는 read-only WMI 조회만 수행하며 host mutation을 추가하지 않는다.
- Topology parity가 불완전하면 helper fallback을 사용한다.
- service mutation 시작 slice는 product wrapper command guard가 `Restart-Computer`, `shutdown.exe`, reboot-forcing `msiexec.exe` argument를 process 실행 전에 차단하는 계약으로 제한한다.
- MSI lifecycle 시작 slice는 `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/norestart`, `1641` reboot-initiated failure classification을 `no_auto_reboot` plan metadata로 노출하는 계약으로 제한한다.
- 사용자 관리자 opt-in smoke는 `0.26.0-admin-smoke` unsigned MSI로 service mutation과 MSI lifecycle을 실제 실행하되, 자동 reboot 금지와 `AllowUnsignedDev` 범위를 유지한다. 이 evidence는 public trusted/stable signing evidence가 아니다.

## Task 1: .NET contract mirror scaffold

**Files:**

- Create: `src/DesktopNode.sln`
- Create: `src/DesktopNode.Contracts/DesktopNode.Contracts.csproj`
- Create: `src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj`

- [x] **Step 1: Confirm .NET SDK**

Run:

```powershell
dotnet --info
```

Expected:

```text
.NET SDK:
 Version: 10.x
```

If .NET 10 SDK is not installed, stop and record the blocker. Do not downgrade silently.

- [x] **Step 2: Create solution and projects**

Run:

```powershell
dotnet new sln -n DesktopNode -o src -f sln
dotnet new classlib -n DesktopNode.Contracts -o src/DesktopNode.Contracts -f net10.0
dotnet new xunit -n DesktopNode.Contracts.Tests -o src/DesktopNode.Contracts.Tests -f net10.0
dotnet sln src/DesktopNode.sln add src/DesktopNode.Contracts/DesktopNode.Contracts.csproj
dotnet sln src/DesktopNode.sln add src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj
dotnet add src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj reference src/DesktopNode.Contracts/DesktopNode.Contracts.csproj
```

Expected:

```text
Project `DesktopNode.Contracts\DesktopNode.Contracts.csproj` added to the solution.
Project `DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj` added to the solution.
Reference `..\DesktopNode.Contracts\DesktopNode.Contracts.csproj` added to the project.
```

- [x] **Step 3: Remove template class**

Delete:

```text
src/DesktopNode.Contracts/Class1.cs
src/DesktopNode.Contracts.Tests/UnitTest1.cs
```

Use a non-destructive path check before deletion. Only remove these exact template files inside `src/`.

## Task 2: Runtime policy DTO contract

**Files:**

- Create: `src/DesktopNode.Contracts/RuntimePolicy.cs`
- Create: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`

- [x] **Step 1: Write failing DTO serialization test**

Create `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs` with:

```csharp
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class RuntimePolicyContractTests
{
    [Fact]
    public void RuntimePolicySerializesPhase24JobRuntimeContract()
    {
        var policy = RuntimePolicyContract.CreateDefault();

        var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var jobRuntime = root.GetProperty("data").GetProperty("job_runtime");

        Assert.Equal("runtime.policy", root.GetProperty("operation").GetString());
        Assert.Equal(1, jobRuntime.GetProperty("contract_version").GetInt32());
        Assert.Equal("local-api", jobRuntime.GetProperty("owner").GetString());
        Assert.Equal("json-file-snapshot", jobRuntime.GetProperty("state_store").GetProperty("persistence").GetString());
        Assert.Equal("hyperv-helper-process", jobRuntime.GetProperty("dispatch").GetProperty("helper_boundary").GetString());
        Assert.True(jobRuntime.GetProperty("control").GetProperty("retry").GetProperty("failed_error_retryable_only").GetBoolean());
        Assert.Equal("helper-process-only", jobRuntime.GetProperty("host_mutation").GetString());
    }
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
dotnet test src/DesktopNode.sln --filter RuntimePolicySerializesPhase24JobRuntimeContract
```

Expected:

```text
error CS0246: The type or namespace name 'RuntimePolicyContract' could not be found
```

- [x] **Step 3: Add minimal DTO implementation**

Create `src/DesktopNode.Contracts/RuntimePolicy.cs` with:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.Contracts;

public sealed record RuntimePolicyResponse(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("data")] RuntimePolicyData Data,
    [property: JsonPropertyName("error")] object? Error);

public sealed record RuntimePolicyData(
    [property: JsonPropertyName("job_runtime")] JobRuntimePolicy JobRuntime);

public sealed record JobRuntimePolicy(
    [property: JsonPropertyName("contract_version")] int ContractVersion,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("state_store")] JobRuntimeStateStorePolicy StateStore,
    [property: JsonPropertyName("dispatch")] JobRuntimeDispatchPolicy Dispatch,
    [property: JsonPropertyName("control")] JobRuntimeControlPolicy Control,
    [property: JsonPropertyName("host_mutation")] string HostMutation,
    [property: JsonPropertyName("orchestration")] JobRuntimeOrchestrationPolicy Orchestration,
    [property: JsonPropertyName("native_core")] JobRuntimeNativeCorePolicy NativeCore,
    [property: JsonPropertyName("managed_core")] JobRuntimeManagedCorePolicy ManagedCore);

public sealed record JobRuntimeStateStorePolicy(
    [property: JsonPropertyName("backend")] string Backend,
    [property: JsonPropertyName("persistence")] string Persistence,
    [property: JsonPropertyName("corrupt_store")] string CorruptStore,
    [property: JsonPropertyName("unsupported_future_version")] string UnsupportedFutureVersion);

public sealed record JobRuntimeDispatchPolicy(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("helper_boundary")] string HelperBoundary);

public sealed record JobRuntimeControlPolicy(
    [property: JsonPropertyName("cancel")] JobRuntimeCancelPolicy Cancel,
    [property: JsonPropertyName("retry")] JobRuntimeRetryPolicy Retry);

public sealed record JobRuntimeCancelPolicy(
    [property: JsonPropertyName("queued_only")] bool QueuedOnly,
    [property: JsonPropertyName("running_interrupt")] bool RunningInterrupt);

public sealed record JobRuntimeRetryPolicy(
    [property: JsonPropertyName("manual_only")] bool ManualOnly,
    [property: JsonPropertyName("failed_error_retryable_only")] bool FailedErrorRetryableOnly,
    [property: JsonPropertyName("max_attempts")] int MaxAttempts,
    [property: JsonPropertyName("creates_new_job")] bool CreatesNewJob);

public sealed record JobRuntimeOrchestrationPolicy(
    [property: JsonPropertyName("primary")] string Primary,
    [property: JsonPropertyName("contract")] string Contract);

public sealed record JobRuntimeNativeCorePolicy(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("revisit_when")] string RevisitWhen);

public sealed record JobRuntimeManagedCorePolicy(
    [property: JsonPropertyName("candidate")] string Candidate,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("host_replacement")] string HostReplacement);

public static class RuntimePolicyContract
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public static RuntimePolicyResponse CreateDefault()
    {
        return new RuntimePolicyResponse(
            Ok: true,
            Operation: "runtime.policy",
            Data: new RuntimePolicyData(
                JobRuntime: new JobRuntimePolicy(
                    ContractVersion: 1,
                    Owner: "local-api",
                    StateStore: new JobRuntimeStateStorePolicy(
                        Backend: "script-scope-memory",
                        Persistence: "json-file-snapshot",
                        CorruptStore: "quarantine-and-start-empty",
                        UnsupportedFutureVersion: "quarantine-and-start-empty"),
                    Dispatch: new JobRuntimeDispatchPolicy(
                        Mode: "bounded-synchronous-worker-tick",
                        HelperBoundary: "hyperv-helper-process"),
                    Control: new JobRuntimeControlPolicy(
                        Cancel: new JobRuntimeCancelPolicy(
                            QueuedOnly: true,
                            RunningInterrupt: false),
                        Retry: new JobRuntimeRetryPolicy(
                            ManualOnly: true,
                            FailedErrorRetryableOnly: true,
                            MaxAttempts: 3,
                            CreatesNewJob: true)),
                    HostMutation: "helper-process-only",
                    Orchestration: new JobRuntimeOrchestrationPolicy(
                        Primary: "powershell",
                        Contract: "plan-contract-injectable-runner-diagnostics"),
                    NativeCore: new JobRuntimeNativeCorePolicy(
                        Status: "not-planned-unless-runtime-boundary-deepens",
                        Reason: "windows-hyperv-orchestration-not-dataplane",
                        RevisitWhen: "state-machine-or-supervision-outgrows-powershell"),
                    ManagedCore: new JobRuntimeManagedCorePolicy(
                        Candidate: "dotnet",
                        Status: "service-host-default",
                        HostReplacement: "dotnet-windows-service-host"))),
            Error: null);
    }
}
```

- [x] **Step 4: Run GREEN verification**

Run:

```powershell
dotnet test src/DesktopNode.sln --filter RuntimePolicySerializesPhase24JobRuntimeContract
```

Expected:

```text
Passed!  - Failed: 0
```

## Task 3: Managed-core candidate stance test

**Files:**

- Modify: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`

- [x] **Step 1: Write failing managed-core stance test**

Append this test to `RuntimePolicyContractTests`:

```csharp
[Fact]
public void RuntimePolicyDeclaresDotNetAsDefaultServiceHost()
{
    var policy = RuntimePolicyContract.CreateDefault();

    var json = JsonSerializer.Serialize(policy, RuntimePolicyContract.JsonOptions);
    using var document = JsonDocument.Parse(json);
    var managedCore = document.RootElement
        .GetProperty("data")
        .GetProperty("job_runtime")
        .GetProperty("managed_core");

    Assert.Equal("dotnet", managedCore.GetProperty("candidate").GetString());
    Assert.Equal("service-host-default", managedCore.GetProperty("status").GetString());
    Assert.Equal("dotnet-windows-service-host", managedCore.GetProperty("host_replacement").GetString());
}
```

- [x] **Step 2: Run RED verification**

Temporarily run before `ManagedCore` exists if Task 2 implementation did not include it:

```powershell
dotnet test src/DesktopNode.sln --filter RuntimePolicyDeclaresDotNetAsDefaultServiceHost
```

Expected when not implemented:

```text
The given key was not present in the dictionary.
```

If Task 2 already included `ManagedCore`, this test may pass immediately. In that case, record that Task 2 intentionally covered the Phase 25 managed-core stance and proceed.

- [x] **Step 3: Keep implementation minimal**

Do not add a .NET API host in this task. The only required behavior is the contract model and serialization.

- [x] **Step 4: Run all .NET contract tests**

Run:

```powershell
dotnet test src/DesktopNode.sln
```

Expected:

```text
Passed!  - Failed: 0
```

## Task 4: Documentation synchronization

**Files:**

- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `follower.md`

- [x] **Step 1: Add verification policy row**

Add a row for Phase 25:

```markdown
| Desktop Node Phase 25 .NET/TypeScript 전환 변경 | .NET contract/runtime/API/service/host 변경은 `dotnet test src/DesktopNode.sln` + 관련 Pester suite 필수 | TypeScript Web Console 변경 시 `npm test --prefix web`, `npm run verify:parity --prefix web`, `npm run browser:fixture --prefix web`, `node --check`, Web Pester suite 추가. Product wrapper/MSI 경로 변경 시 packaging + installer suite 추가 | .NET service host replacement는 기본 제품 service/MSI path지만, Hyper-V/service/MSI/firewall/Event Log mutation은 계속 관리자 opt-in gate |
```

- [x] **Step 2: Add developer index entry**

Add:

```markdown
| Phase 25 .NET/TypeScript 전환 후보 | `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md`, `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md` |
```

- [x] **Step 3: Add roadmap row**

Add Phase 25 candidate row:

```markdown
| Phase 25 | 후보/현재 .NET 및 TypeScript parity/API/service scaffold, .NET service host replacement, native read routes, VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete native mutation adapter 완료 | .NET contract/runtime core, TypeScript Web Console, PowerShell adapter 전환 경계를 고정하고 기본 제품 service host를 .NET으로 이동한다. | Phase 25 spec/plan, .NET Host replacement spec/plan, native adapter plans, .NET contract/runtime/API/service/host tests, TypeScript `tsc --noEmit` and `verify:parity` scaffold | 기본 제품 service host, listener owner, SCM binary path, MSI installed custom action runner는 `DesktopNode.Host.exe`로 교체됐다. `src/DesktopNode.Api/**`는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation routes, checkpoint create/restore/delete native mutation routes, queued job runtime, job store save/load/recovery를 처리한다. Web Console served `app.js`는 repo-root `web/src/served-app.ts` build output이다. Public trusted/stable signing, GA 승격은 아직 별도 후속 판단이다. |
```

- [x] **Step 4: Update follower**

Add Phase 25 to the next priority queue as a non-admin follow-up:

```markdown
1. .NET/TypeScript mixed runtime transition candidate
   - 첫 slice인 `.NET contract mirror`, 순수 job state validator, TypeScript static parity scaffold/manifest/verification flow, .NET API/service host candidate contract는 side-by-side로 구현됐다.
   - 2026-05-01 replacement slice에서 기본 제품 service host와 MSI installed action runner는 `DesktopNode.Host.exe`로 교체됐다.
   - PowerShell Hyper-V helper와 packaging 운영 runbook은 유지한다.
   - public trusted/stable signing과 GA 승격은 수행하지 않는다.
```

## Task 5: Verification

**Files:**

- Test: `src/DesktopNode.sln`
- Test: `spikes/purecvisor-desktop-node/tests`
- Test: `spikes/purecvisor-desktop-node/api/tests`

- [x] **Step 1: Run .NET tests**

Run:

```powershell
dotnet test src/DesktopNode.sln
```

Expected:

```text
Passed!  - Failed: 0
```

- [x] **Step 2: Run Local API Pester suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected:

```text
Failed: 0
```

- [x] **Step 3: Run root docs suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected:

```text
Failed: 0
```

- [x] **Step 4: Run diff hygiene**

Run:

```powershell
git diff --check
```

Expected:

```text
exit 0
```

## 완료 증거

첫 .NET contract mirror slice를 실행했다. 이어서 두 번째 순수 .NET job state transition validator slice, 세 번째 TypeScript Web Console static parity scaffold slice, 병렬 후속 TypeScript generated parity manifest와 .NET API host candidate contract slice, 두 번째 병렬 후속 TypeScript parity verification flow와 .NET Service host candidate contract slice, 세 번째 병렬 후속 TypeScript generated parity manifest regeneration flow, 네 번째 병렬 후속 .NET API handler adapter contract, .NET service lifecycle adapter contract, TypeScript user-visible fixture parity flow, Web Console browser fixture parity flow를 실행했다. 이후 2026-05-03 served asset/root migration slice가 Web Console source와 package 검증 owner를 repo-root `web/**`로 이동했고, served `web/app.js`는 `web/src/served-app.ts` build output이 됐다. Native adapter 후속 slices는 read-route helper fallback을 제거하고 checkpoint create/restore/delete를 C# WMI snapshot service adapter로 전환했다.

완료한 증거:

- `dotnet --info`: .NET SDK `10.0.203`
- `dotnet test src/DesktopNode.sln --filter RuntimePolicySerializesPhase24JobRuntimeContract`: RED에서 `RuntimePolicyContract` 미정의 컴파일 오류 확인
- `dotnet test src/DesktopNode.sln --filter RuntimePolicySerializesPhase24JobRuntimeContract`: GREEN, 1 passed
- 초기 side-by-side managed-core stance test: RED/GREEN 확인
- `dotnet test src/DesktopNode.sln --filter RuntimePolicyDeclaresDotNetAsDefaultServiceHost`: replacement slice에서 `service-host-default`/`dotnet-windows-service-host` stance 확인
- `dotnet test src/DesktopNode.sln`: 2 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`: 97 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 15 passed, 0 failed
- `git diff --check`: exit 0, CRLF warnings only
- `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --filter JobStateTransitionPolicy`: RED에서 `DesktopNode.Runtime`/`JobStatus` 미정의 컴파일 오류 확인
- `dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --filter JobStateTransitionPolicy`: GREEN, 15 passed, 0 failed
- `dotnet test src/DesktopNode.sln`: 17 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: RED에서 `package.json`, `tsconfig.json`, `src` TypeScript source 누락 확인
- `npm install --prefix spikes/purecvisor-desktop-node/web`: TypeScript `5.9.3` dev dependency와 lockfile 생성
- `npm test --prefix spikes/purecvisor-desktop-node/web`: `tsc --noEmit -p tsconfig.json` 통과
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: GREEN, 14 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: generated parity manifest 누락 RED 확인
- `npm test --prefix spikes/purecvisor-desktop-node/web`: generated parity manifest builder 포함 `tsc --noEmit -p tsconfig.json` 통과
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: GREEN, 15 passed, 0 failed
- `node --check spikes/purecvisor-desktop-node/web/app.js`: exit 0
- `dotnet test src/DesktopNode.sln`: .NET API host candidate contract 누락 RED 확인
- `dotnet test src/DesktopNode.sln`: GREEN, 22 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: parity verification script 누락 RED 확인
- `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web`: committed manifest/source/static serving parity 검증 통과
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: GREEN, 18 passed, 0 failed
- `node --check spikes/purecvisor-desktop-node/web/scripts/verify-static-parity.mjs`: exit 0
- `dotnet test src\DesktopNode.Service.Tests\DesktopNode.Service.Tests.csproj`: Service host candidate contract 누락 RED 확인
- `dotnet test src/DesktopNode.sln`: GREEN, 27 passed, 0 failed
- `npm run generate:parity --prefix spikes/purecvisor-desktop-node/web`: TypeScript AST 기반 generated parity manifest 재생성
- `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web`: manifest stale check와 static serving parity 검증 통과
- `node --check spikes/purecvisor-desktop-node/web/scripts/regenerate-static-parity.mjs`: exit 0
- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj`: API handler adapter contract 검증 통과
- `dotnet test src/DesktopNode.Service.Tests/DesktopNode.Service.Tests.csproj`: service lifecycle adapter contract 검증 통과
- `npm test --prefix spikes/purecvisor-desktop-node/web`: user-visible fixture parity source 포함 TypeScript 검증 통과
- `npm run verify:parity --prefix spikes/purecvisor-desktop-node/web`: user-visible fixture entry 포함 parity 검증 통과
- `dotnet test src/DesktopNode.sln --filter ApiRuntimePolicyRequestProcessorTests`: RED에서 `DesktopNodeApiRequestProcessor` 미정의 컴파일 오류 확인 후 GREEN 통과
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: product wrapper `no_auto_reboot` plan/process guard 검증 통과
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: MSI lifecycle `no_auto_reboot` metadata 검증 통과
- `artifacts/product-path-service-msi-start-20260501-194840/msi-build/PureCVisorDesktopNode-0.26.0-admin-smoke-windows-x64.msi`: `AllowUnsignedDev` unsigned MSI build, SHA-256 `b9bfff35195f88bd1b9e4c4f35f3d883e39e5a721a32bb2f15023f5fe60446f8`
- `artifacts/product-path-service-msi-start-20260501-194840/lifecycle-sequential-20260501-200211/msi-lifecycle-sequential-result.json`: install, repair, 기본 uninstall, install-remove-data, uninstall-remove-data, restore install 모두 exit `0`; service `Running`; Web root `200`; automatic reboot, reboot required, post-reboot verification required 모두 false
- `artifacts/product-path-service-msi-start-20260501-194840/lifecycle`: 초기 lifecycle runner는 `msiexec` 대기 방식 오류로 1618 경합을 만들었으므로 유효 lifecycle evidence로 사용하지 않는다.

남은 작업:

- TypeScript Web Console scaffold, generated parity manifest, parity verification/regeneration flow, user-visible fixture parity, Node `vm` browser fixture parity, .NET API host/handler adapter candidate contract, .NET Service host/lifecycle adapter candidate contract는 side-by-side 범위로 구현됐다.
- 제품 실행 경로 교체는 `GET /api/v1/runtime/policy` pure request processor로 시작했고, 2026-05-01 replacement slice에서 listener, service executable, MSI installed custom action, product host default owner를 `DesktopNode.Host.exe`로 교체했다.
- service mutation과 MSI lifecycle 시작은 no-auto-reboot contract 보강 및 `0.26.0-admin-smoke` 사용자 관리자 opt-in smoke로 제한됐다. 실제 smoke는 자동 reboot 미사용/미관측으로 통과했지만 unsigned admin-smoke evidence라서 public trusted/stable signing evidence가 아니다.

후속 slice 후보:

- 다음 구현 slice는 installed non-mutating rerun 또는 .NET adapter injectable runner tests 중 하나로 제한한다.
- API host route parity 시작 slice는 .NET request processor와 Host forwarding에 반영됐다. PowerShell Local API adapter 경계는 component baseline으로 유지한다. Service host 기본 실행 경로는 `DesktopNode.Host.exe`로 교체됐지만, 이 replacement와 route parity code evidence는 GA 승격을 의미하지 않는다.
- TypeScript Web Console 후속은 기존 static asset serving과 user-visible behavior parity를 먼저 검증하며, token 값은 source/fixture/log/generated artifact에 노출하지 않는다. Real browser/Playwright evidence는 후속 도구 후보로 둔다.
- 후속 route parity evidence hardening은 Hyper-V helper, firewall, Event Log mutation을 기본 비파괴 검증에서 건드리지 않는다. `0.26.6-admin-smoke` 설치본 service/MSI/Hyper-V route smoke는 `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729`에서 통과했으며, 이후 실제 service/MSI/Hyper-V mutation은 계속 관리자 opt-in gate로 분리한다.
- 현재 기본 검증은 `dotnet test src/DesktopNode.sln`, `npm test --prefix web`, `npm run generate:parity --prefix web`, `npm run verify:parity --prefix web`, `npm run browser:fixture --prefix web`, `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`, `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`, `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`, `git diff --check`다.
