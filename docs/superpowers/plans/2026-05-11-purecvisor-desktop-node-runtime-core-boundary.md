# Runtime Core Boundary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split `DesktopNode.Api` runtime route ownership into explicit route-family boundaries without changing served API behavior.

**Architecture:** Keep `DesktopNodeApiRequestProcessor` as the request processor while adding route-family metadata and moving auth, diagnostics, jobs, and ops summary logic behind focused runtime helpers. The first wave is behavior-preserving: tests lock the current route contract before any code is moved.

**Tech Stack:** C# net10.0, xUnit, `System.Text.Json`, existing `DesktopNode.Api`, `DesktopNode.Runtime`, and `DesktopNode.Contracts` projects.

---

## File Structure

- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` to add route-family metadata.
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` to lock route family ownership.
- Create: `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs` for route-family constants and matching helpers.
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` to call the new helper while preserving existing response bodies.
- Create: `docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md` as the baseline snapshot.

## Task 1: Baseline Route Family Snapshot

**Files:**
- Create: `docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md`
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

- [ ] **Step 1: Write the baseline document**

Create `docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md` with this content:

```markdown
# Runtime/Core Boundary Baseline - 2026-05-11

source_decision: ga-ready-product-runtime
distribution_boundary: internal-private-network-only
host_mutation_default: explicit-admin-opt-in-only

## Runtime-owned route families

- runtime policy: `GET /api/v1/runtime/policy`
- jobs: `GET /api/v1/jobs`, `GET /api/v1/jobs/{jobId}`, `POST /api/v1/jobs/{jobId}/cancel`, `POST /api/v1/jobs/{jobId}/retry`
- diagnostics: `GET /api/v1/diagnostics/bundles`, `POST /api/v1/diagnostics/bundles`, `GET /api/v1/diagnostics/bundles/{bundleId}/download`
- auth/session/RBAC: `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`, `GET /api/v1/auth/session`, `GET /api/v1/auth/rbac`
- console handoff metadata: `GET /api/v1/console/capabilities`, `GET /api/v1/vms/{vmId}/console`
- ops summary: `GET /api/v1/ops/summary`

## Native adapter-owned route families

- host status: `GET /api/v1/host/status`
- network inventory: `GET /api/v1/network/inventory`
- VM read: `GET /api/v1/vms`, `GET /api/v1/vms/{vmId}`
- checkpoint read: `GET /api/v1/vms/{vmId}/checkpoints`
- VM/checkpoint queued mutations: `POST /api/v1/vms`, VM lifecycle routes, checkpoint create/restore/delete routes, `DELETE /api/v1/vms/{vmId}`

## Non-goals

- No Hyper-V WMI provider movement in this workstream.
- No Windows service/firewall/Event Log/trust store mutation movement in this workstream.
- No public distribution reopening.
```

- [ ] **Step 2: Add a route-family guard test**

Append this test to `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` before `AssertRoute`:

```csharp
[Fact]
public void DefaultContractGroupsRuntimeCoreRoutesByFamily()
{
    var contract = ApiHandlerAdapterContract.CreateDefault();
    var routes = contract.Routes.ToDictionary(route => (route.Method, route.RouteTemplate));

    Assert.Equal("runtime-policy", routes[("GET", "/api/v1/runtime/policy")].RouteFamily);
    Assert.Equal("jobs", routes[("GET", "/api/v1/jobs")].RouteFamily);
    Assert.Equal("ops-summary", routes[("GET", "/api/v1/ops/summary")].RouteFamily);
    Assert.Equal("diagnostics", routes[("GET", "/api/v1/diagnostics/bundles")].RouteFamily);
    Assert.Equal("diagnostics", routes[("POST", "/api/v1/diagnostics/bundles")].RouteFamily);
    Assert.Equal("diagnostics", routes[("GET", "/api/v1/diagnostics/bundles/{bundleId}/download")].RouteFamily);
    Assert.Equal("auth", routes[("POST", "/api/v1/auth/login")].RouteFamily);
    Assert.Equal("auth", routes[("POST", "/api/v1/auth/refresh")].RouteFamily);
    Assert.Equal("auth", routes[("POST", "/api/v1/auth/logout")].RouteFamily);
    Assert.Equal("auth", routes[("GET", "/api/v1/auth/session")].RouteFamily);
    Assert.Equal("auth", routes[("GET", "/api/v1/auth/rbac")].RouteFamily);
    Assert.Equal("console", routes[("GET", "/api/v1/console/capabilities")].RouteFamily);
    Assert.Equal("console", routes[("GET", "/api/v1/vms/{vmId}/console")].RouteFamily);

    Assert.Equal("hyperv-host", routes[("GET", "/api/v1/host/status")].RouteFamily);
    Assert.Equal("hyperv-network", routes[("GET", "/api/v1/network/inventory")].RouteFamily);
    Assert.Equal("hyperv-vm", routes[("GET", "/api/v1/vms")].RouteFamily);
    Assert.Equal("hyperv-vm", routes[("DELETE", "/api/v1/vms/{vmId}")].RouteFamily);
    Assert.Equal("hyperv-checkpoint", routes[("GET", "/api/v1/vms/{vmId}/checkpoints")].RouteFamily);
}
```

- [ ] **Step 3: Run the new test to verify it fails**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter DefaultContractGroupsRuntimeCoreRoutesByFamily --no-restore
```

Expected: FAIL with a compile error because `ApiHandlerRouteContract.RouteFamily` does not exist.

## Task 2: Add Route Family Metadata

**Files:**
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
- Test: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

- [ ] **Step 1: Add `RouteFamily` to the route contract**

Update the record signature in `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`:

```csharp
public sealed record ApiHandlerRouteContract(
    string RouteTemplate,
    string Method,
    string AuthPolicy,
    MutationStance MutationStance,
    string OperationName,
    string DefaultOwner,
    string RouteFamily);
```

- [ ] **Step 2: Update route builder helpers**

Replace the four helper signatures and constructors with:

```csharp
private static ApiHandlerRouteContract RuntimeReadOnly(string routeTemplate, string operationName, string routeFamily)
{
    return new ApiHandlerRouteContract(
        RouteTemplate: routeTemplate,
        Method: "GET",
        AuthPolicy: "ReadTokenRequired",
        MutationStance: MutationStance.ReadOnly,
        OperationName: operationName,
        DefaultOwner: DotNetRuntimeOwner,
        RouteFamily: routeFamily);
}

private static ApiHandlerRouteContract NativeReadOnly(string routeTemplate, string operationName, string routeFamily)
{
    return new ApiHandlerRouteContract(
        RouteTemplate: routeTemplate,
        Method: "GET",
        AuthPolicy: "ReadTokenRequired",
        MutationStance: MutationStance.ReadOnly,
        OperationName: operationName,
        DefaultOwner: DotNetNativeAdapterOwner,
        RouteFamily: routeFamily);
}

private static ApiHandlerRouteContract RuntimeProductOperation(string routeTemplate, string operationName, string routeFamily, string authPolicy = "TokenRequired")
{
    return new ApiHandlerRouteContract(
        RouteTemplate: routeTemplate,
        Method: "POST",
        AuthPolicy: authPolicy,
        MutationStance: MutationStance.ProductOperation,
        OperationName: operationName,
        DefaultOwner: DotNetRuntimeOwner,
        RouteFamily: routeFamily);
}

private static ApiHandlerRouteContract NativeQueuedMutation(string routeTemplate, string operationName, string routeFamily, string method = "POST")
{
    return new ApiHandlerRouteContract(
        RouteTemplate: routeTemplate,
        Method: method,
        AuthPolicy: "TokenRequired",
        MutationStance: MutationStance.QueuedMutation,
        OperationName: operationName,
        DefaultOwner: DotNetNativeAdapterOwner,
        RouteFamily: routeFamily);
}
```

- [ ] **Step 3: Update `CreateDefault` route calls**

Use these route-family values in `CreateDefault`:

```csharp
RuntimeReadOnly("/api/v1/runtime/policy", "RuntimePolicy", "runtime-policy"),
NativeReadOnly("/api/v1/host/status", "HostStatus", "hyperv-host"),
NativeReadOnly("/api/v1/vms", "ListVms", "hyperv-vm"),
RuntimeReadOnly("/api/v1/jobs", "ListJobs", "jobs"),
RuntimeReadOnly("/api/v1/ops/summary", "OpsSummary", "ops-summary"),
RuntimeReadOnly("/api/v1/diagnostics/bundles", "ListDiagnosticBundles", "diagnostics"),
RuntimeProductOperation("/api/v1/diagnostics/bundles", "CreateDiagnosticBundle", "diagnostics"),
RuntimeReadOnly("/api/v1/diagnostics/bundles/{bundleId}/download", "DownloadDiagnosticBundle", "diagnostics"),
RuntimeProductOperation("/api/v1/auth/login", "LoginAccount", "auth", "NoBearerTokenRequired"),
RuntimeProductOperation("/api/v1/auth/refresh", "RefreshAccount", "auth", "NoBearerTokenRequired"),
RuntimeProductOperation("/api/v1/auth/logout", "LogoutAccount", "auth", "NoBearerTokenRequired"),
RuntimeReadOnly("/api/v1/auth/session", "GetAccountSession", "auth"),
RuntimeReadOnly("/api/v1/auth/rbac", "GetAccountRbac", "auth"),
RuntimeReadOnly("/api/v1/console/capabilities", "GetConsoleCapabilities", "console"),
NativeReadOnly("/api/v1/network/inventory", "NetworkInventory", "hyperv-network"),
NativeReadOnly("/api/v1/vms/{vmId}", "GetVm", "hyperv-vm"),
RuntimeReadOnly("/api/v1/vms/{vmId}/console", "GetVmConsoleSession", "console"),
NativeReadOnly("/api/v1/vms/{vmId}/checkpoints", "ListVmCheckpoints", "hyperv-checkpoint"),
NativeQueuedMutation("/api/v1/vms", "QueueCreateVm", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}/checkpoints", "QueueCreateVmCheckpoint", "hyperv-checkpoint"),
NativeQueuedMutation("/api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore", "QueueRestoreVmCheckpoint", "hyperv-checkpoint"),
NativeQueuedMutation("/api/v1/vms/{vmId}/checkpoints/{checkpointId}", "QueueDeleteVmCheckpoint", "hyperv-checkpoint", "DELETE"),
NativeQueuedMutation("/api/v1/vms/{vmId}/start", "QueueStartVm", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}/shutdown", "QueueShutdownVm", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}/poweroff", "QueuePowerOffVm", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}/restart", "QueueRestartVm", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}", "QueueDeleteVm", "hyperv-vm", "DELETE")
```

- [ ] **Step 4: Run route contract tests**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter ApiHandlerAdapterContractTests --no-restore
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Api/ApiHandlerAdapterContract.cs src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md
git commit -m "test: lock runtime core route families"
```

## Task 3: Create Runtime Route Helper

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Test: `src/DesktopNode.Api.Tests/ApiAccountAuthRequestProcessorTests.cs`, `src/DesktopNode.Api.Tests/ApiDiagnosticBundleRequestProcessorTests.cs`

- [ ] **Step 1: Add runtime route helper**

Create `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs`:

```csharp
using System.Text.RegularExpressions;

namespace DesktopNode.Api;

internal static class DesktopNodeApiRuntimeRoutes
{
    public static bool IsAccountAuthRoute(string method, string path)
    {
        return path.StartsWith("/api/v1/auth/", StringComparison.OrdinalIgnoreCase) &&
            (method == "GET" || method == "POST");
    }

    public static bool IsDiagnosticsRoute(string method, string path)
    {
        return (method == "GET" || method == "POST") &&
            (path == "/api/v1/diagnostics/bundles" ||
             Regex.IsMatch(path, "^/api/v1/diagnostics/bundles/([^/]+)/download$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));
    }

    public static bool IsJobRoute(string method, string path)
    {
        return (method == "GET" || method == "POST") &&
            (path == "/api/v1/jobs" ||
             Regex.IsMatch(path, "^/api/v1/jobs/([^/]+)(/(cancel|retry))?$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));
    }
}
```

- [ ] **Step 2: Replace inline auth route prefix check**

In `DesktopNodeApiRequestProcessor.HandleAccountAuthRoute`, replace:

```csharp
if (!path.StartsWith("/api/v1/auth/", StringComparison.OrdinalIgnoreCase))
```

with:

```csharp
if (!DesktopNodeApiRuntimeRoutes.IsAccountAuthRoute(method, path))
```

- [ ] **Step 3: Run focused tests**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "ApiAccountAuthRequestProcessorTests|ApiDiagnosticBundleRequestProcessorTests|ApiHandlerAdapterContractTests" --no-restore
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests
git commit -m "refactor: add runtime route boundary helper"
```

## Task 4: Full Runtime/Core Verification

**Files:**
- No source edits

- [ ] **Step 1: Run runtime/core tests**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore
dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --no-restore
dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --no-restore
git diff --check
```

Expected: all tests PASS and `git diff --check` prints no errors.

- [ ] **Step 2: Commit verification note if docs changed**

If only source/test changes exist, skip this step. If the baseline document was corrected after review:

```powershell
git add docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md
git commit -m "docs: update runtime core boundary baseline"
```
