# HyperV Domain Split Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract the Hyper-V domain from `DesktopNode.Api` into a focused `DesktopNode.HyperV` project while preserving all API route behavior.

**Architecture:** Introduce a `DesktopNode.HyperV` class library that owns Hyper-V interfaces, records, WMI providers, and structured operation exceptions. `DesktopNode.Api` should depend on this domain project and continue using `IDesktopNodeHyperVNativeAdapter` through the same public contract.

**Tech Stack:** C# net10.0-windows, `System.Management`, xUnit, existing `DesktopNode.Api.Tests` adapter coverage.

---

## File Structure

- Create: `src/DesktopNode.HyperV/DesktopNode.HyperV.csproj`
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs`
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs`
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviders.cs`
- Modify: `src/DesktopNode.Api/DesktopNode.Api.csproj`
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
- Modify: `src/DesktopNode.sln`
- Create: `docs/ga-ready/hyperv-domain-baseline-2026-05-11.md`

## Task 1: Baseline Hyper-V Boundary

**Files:**
- Create: `docs/ga-ready/hyperv-domain-baseline-2026-05-11.md`
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

- [ ] **Step 1: Write baseline document**

Create `docs/ga-ready/hyperv-domain-baseline-2026-05-11.md`:

```markdown
# Hyper-V Domain Baseline - 2026-05-11

source_file: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
target_project: `src/DesktopNode.HyperV`
behavior_change_allowed: false
host_mutation_default: explicit-admin-opt-in-only

## Current route ownership

- `GET /api/v1/host/status`: native Hyper-V host status provider
- `GET /api/v1/network/inventory`: native WMI switch provider
- `GET /api/v1/vms`: native WMI VM provider
- `GET /api/v1/vms/{vmId}`: native VM inventory result
- `GET /api/v1/vms/{vmId}/checkpoints`: native checkpoint provider
- VM lifecycle queued mutations: native WMI lifecycle providers
- Checkpoint lifecycle queued mutations: native WMI snapshot providers

## Split order

1. Move interfaces and records.
2. Move native adapter orchestration class.
3. Move WMI provider implementations.
4. Keep API response contract unchanged.
```

- [ ] **Step 2: Add a guard that Hyper-V routes remain native-owned**

Append this test to `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`:

```csharp
[Fact]
public void HyperVRouteFamiliesStayNativeOwnedDuringDomainSplit()
{
    var contract = ApiHandlerAdapterContract.CreateDefault();
    var hyperVRoutes = contract.Routes
        .Where(route => route.RouteFamily.StartsWith("hyperv-", StringComparison.Ordinal))
        .ToArray();

    Assert.NotEmpty(hyperVRoutes);
    Assert.All(hyperVRoutes, route => Assert.Equal("dotnet-native-adapter", route.DefaultOwner));
    Assert.All(hyperVRoutes, route => Assert.DoesNotContain("powershell", route.DefaultOwner, StringComparison.OrdinalIgnoreCase));
}
```

- [ ] **Step 3: Run guard test**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter HyperVRouteFamiliesStayNativeOwnedDuringDomainSplit --no-restore
```

Prerequisite: complete `runtime-core-boundary-plan` Task 2 before this step. Expected: PASS.

## Task 2: Create HyperV Project Shell

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNode.HyperV.csproj`
- Modify: `src/DesktopNode.Api/DesktopNode.Api.csproj`
- Modify: `src/DesktopNode.sln`

- [ ] **Step 1: Create project file**

Create `src/DesktopNode.HyperV/DesktopNode.HyperV.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Management" Version="10.0.0" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add project reference**

In `src/DesktopNode.Api/DesktopNode.Api.csproj`, add:

```xml
<ProjectReference Include="..\DesktopNode.HyperV\DesktopNode.HyperV.csproj" />
```

inside the existing project reference item group.

- [ ] **Step 3: Add project to solution**

Run:

```powershell
dotnet sln src/DesktopNode.sln add src/DesktopNode.HyperV/DesktopNode.HyperV.csproj
```

Expected: command reports the project was added.

- [ ] **Step 4: Build solution**

```powershell
dotnet build src/DesktopNode.sln --no-restore
```

Expected: PASS.

## Task 3: Move Hyper-V Interfaces and Models

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`

- [ ] **Step 1: Move declarations**

Move these declarations unchanged from `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs` to `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` and change the namespace to `DesktopNode.HyperV`:

```csharp
public interface IDesktopNodeHyperVNativeAdapter
public interface IDesktopNodeHyperVSwitchProvider
public interface IDesktopNodeHyperVHostStatusProvider
public interface IDesktopNodeHyperVVmProvider
public interface IDesktopNodeHyperVCheckpointProvider
public interface IDesktopNodeHyperVCheckpointMutationProvider
public interface IDesktopNodeHyperVVmPowerStateProvider
public interface IDesktopNodeHyperVVmCreateProvider
public interface IDesktopNodeHyperVVmDeleteProvider
public sealed record DesktopNodeHyperVSwitchInfo
public sealed record DesktopNodeHyperVVmInfo
public sealed record DesktopNodeHyperVVmCpuInfo
public sealed record DesktopNodeHyperVVmMemoryInfo
public sealed record DesktopNodeHyperVVmDiskInfo
public sealed record DesktopNodeHyperVVmNetworkInfo
public sealed record DesktopNodeHyperVVmCheckpointInfo
public sealed record DesktopNodeHyperVVmConsoleInfo
public sealed record DesktopNodeHyperVCheckpointInfo
public sealed record DesktopNodeHyperVCheckpointMutationInfo
public sealed record DesktopNodeHyperVVmPowerStateInfo
public sealed record DesktopNodeHyperVVmCreateRequest
public sealed record DesktopNodeHyperVVmCreateInfo
public sealed record DesktopNodeHyperVVmDeleteInfo
public sealed record DesktopNodeHyperVWmiVmSummary
public sealed record DesktopNodeHyperVWmiVmStorageSummary
public sealed record DesktopNodeHyperVWmiVmNetworkSummary
public sealed record DesktopNodeHyperVHostStatusData
public sealed record DesktopNodeHyperVHostWindowsInfo
public sealed record DesktopNodeHyperVHostAdminInfo
public sealed record DesktopNodeHyperVHostHyperVInfo
public sealed class DesktopNodeHyperVNativeOperationException
```

- [ ] **Step 2: Add using in API file**

At the top of `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`, add:

```csharp
using DesktopNode.HyperV;
```

- [ ] **Step 3: Update tests using namespace**

Add this using to API test files that reference moved Hyper-V types directly:

```csharp
using DesktopNode.HyperV;
```

to that test file.

- [ ] **Step 4: Build**

```powershell
dotnet build src/DesktopNode.sln --no-restore
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.HyperV src/DesktopNode.Api src/DesktopNode.sln docs/ga-ready/hyperv-domain-baseline-2026-05-11.md
git commit -m "refactor: introduce hyperv domain project"
```

## Task 4: Move Adapter Orchestration

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`

- [ ] **Step 1: Move adapter class**

Move `public sealed class DesktopNodeHyperVNativeAdapter : IDesktopNodeHyperVNativeAdapter` unchanged into `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs` with namespace `DesktopNode.HyperV`.

- [ ] **Step 2: Keep API creation behavior**

In `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`, add:

```csharp
using DesktopNode.HyperV;
```

Keep the default adapter creation behavior identical:

```csharp
nativeAdapter is null
    ? DesktopNodeHyperVNativeAdapter.CreateDefault()
    : nativeAdapter
```

- [ ] **Step 3: Build and run API tests**

```powershell
dotnet build src/DesktopNode.sln --no-restore
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add src/DesktopNode.HyperV src/DesktopNode.Api
git commit -m "refactor: move hyperv adapter orchestration"
```

## Task 5: Move WMI Providers

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviders.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`

- [ ] **Step 1: Move provider classes unchanged**

Move these provider classes to `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviders.cs` with namespace `DesktopNode.HyperV`:

```csharp
public sealed class DesktopNodeHyperVNativeHostStatusProvider
public sealed class DesktopNodeHyperVWmiSwitchProvider
public sealed class DesktopNodeHyperVWmiVmProvider
public sealed class DesktopNodeHyperVWmiCheckpointProvider
public sealed class DesktopNodeHyperVWmiCheckpointMutationProvider
public sealed class DesktopNodeHyperVWmiVmPowerStateProvider
public sealed class DesktopNodeHyperVWmiVmDeleteProvider
public sealed class DesktopNodeHyperVWmiVmCreateProvider
```

- [ ] **Step 2: Delete old API source file after the move**

After all declarations have moved, delete `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`. Do not leave duplicate type definitions. If a compile error shows a namespace import is missing, add this using to the file that references the moved type:

```csharp
using DesktopNode.HyperV;

namespace DesktopNode.Api;
```

Do not duplicate type definitions.

- [ ] **Step 3: Run full dotnet solution tests**

```powershell
dotnet test src/DesktopNode.sln --no-restore
git diff --check
```

Expected: PASS and no whitespace errors.

- [ ] **Step 4: Commit**

```powershell
git add src/DesktopNode.HyperV src/DesktopNode.Api src/DesktopNode.sln
git commit -m "refactor: move hyperv wmi providers"
```
