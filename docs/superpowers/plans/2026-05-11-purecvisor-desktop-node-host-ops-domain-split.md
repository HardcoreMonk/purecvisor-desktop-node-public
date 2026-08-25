# Host Ops Domain Split Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split `DesktopNodeHostServiceAction.cs` into focused host operation families while preserving every existing service-action contract.

**Architecture:** Keep `DesktopNodeHostServiceAction.CreatePlan` and `ExecuteAsync` as the public entrypoints during the first wave. Move operation-family code into internal static classes under `src/DesktopNode.Host/Ops/` so tests can keep calling the same public API.

**Tech Stack:** C# net10.0-windows, xUnit, existing Windows controller abstractions, packaging Pester for service plan contract.

---

## File Structure

- Create: `src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeEventLogOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeFirewallOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeTrustStoreOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs`
- Create: `docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md`
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

## Task 1: Baseline Service-Action Families

**Files:**
- Create: `docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [ ] **Step 1: Write baseline document**

Create `docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md`:

```markdown
# Host Ops Boundary Baseline - 2026-05-11

entrypoint: `DesktopNodeHostServiceAction.CreatePlan` and `DesktopNodeHostServiceAction.ExecuteAsync`
host_mutation_default: explicit-admin-opt-in-only
behavior_change_allowed: false

## Operation families

- service lifecycle: `status`, `start`, `stop`, `configure-installed`, `repair-installed`, `remove-installed`
- data root: `data-root-remove`
- migration apply: `config-migration-apply`, `job-store-migration-apply`
- token: `service-token-rotation-revoke`
- Credential Manager: `credential-manager-system-proof`, `credential-manager-default-transition`
- Event Log: `eventlog-register`, `eventlog-remove`, `eventlog-repair`, `eventlog-write-test`, `eventlog-volume-guard`, `eventlog-default-transition`
- firewall: `firewall-enable`, `firewall-remove`
- trust store: `trust-store-install`, `trust-store-remove`

## Invariants

- No operation family may introduce PowerShell command fallback.
- Firewall LAN exposure requires explicit LAN approval.
- Trust store install requires release approval.
- Data-root delete requires service absent and explicit remove-data.
```

- [ ] **Step 2: Add operation family plan guard**

Append this test to `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`:

```csharp
[Fact]
public void ServiceActionPlansDeclareStableOperationFamilies()
{
    var cases = new[]
    {
        ("status", "service-lifecycle"),
        ("start", "service-lifecycle"),
        ("stop", "service-lifecycle"),
        ("configure-installed", "service-lifecycle"),
        ("repair-installed", "service-lifecycle"),
        ("remove-installed", "service-lifecycle"),
        ("data-root-remove", "data-root"),
        ("config-migration-apply", "migration"),
        ("job-store-migration-apply", "migration"),
        ("service-token-rotation-revoke", "token"),
        ("credential-manager-system-proof", "credential-manager"),
        ("credential-manager-default-transition", "credential-manager"),
        ("eventlog-register", "event-log"),
        ("eventlog-remove", "event-log"),
        ("eventlog-repair", "event-log"),
        ("eventlog-write-test", "event-log"),
        ("eventlog-volume-guard", "event-log"),
        ("eventlog-default-transition", "event-log"),
        ("firewall-enable", "firewall"),
        ("firewall-remove", "firewall"),
        ("trust-store-install", "trust-store"),
        ("trust-store-remove", "trust-store")
    };

    foreach (var (action, family) in cases)
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = action,
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            DataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            RemoveData = action == "data-root-remove",
            AllowLan = action.StartsWith("firewall-", StringComparison.Ordinal),
            AllowReleaseMutation = action.StartsWith("trust-store-", StringComparison.Ordinal)
        });

        Assert.Equal(family, plan.OperationFamily);
        Assert.Empty(plan.Commands);
    }
}
```

- [ ] **Step 3: Run the guard test**

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter ServiceActionPlansDeclareStableOperationFamilies --no-restore
```

Expected: FAIL with a compile error because `DesktopNodeHostServiceActionPlan.OperationFamily` does not exist.

## Task 2: Add Operation Family Metadata

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [ ] **Step 1: Add property to plan record**

Add `string OperationFamily` to `DesktopNodeHostServiceActionPlan` after `string Operation`.

- [ ] **Step 2: Add family resolver**

Add this private helper to `DesktopNodeHostServiceAction`:

```csharp
private static string ResolveOperationFamily(string action)
{
    if (action is "status" or "start" or "stop" or "configure-installed" or "repair-installed" or "remove-installed")
    {
        return "service-lifecycle";
    }

    if (action == "data-root-remove")
    {
        return "data-root";
    }

    if (action is "config-migration-apply" or "job-store-migration-apply")
    {
        return "migration";
    }

    if (action == "service-token-rotation-revoke")
    {
        return "token";
    }

    if (action is "credential-manager-system-proof" or "credential-manager-default-transition")
    {
        return "credential-manager";
    }

    if (action.StartsWith("eventlog-", StringComparison.OrdinalIgnoreCase))
    {
        return "event-log";
    }

    if (action.StartsWith("firewall-", StringComparison.OrdinalIgnoreCase))
    {
        return "firewall";
    }

    if (action.StartsWith("trust-store-", StringComparison.OrdinalIgnoreCase))
    {
        return "trust-store";
    }

    return "unknown";
}
```

- [ ] **Step 3: Set family in `CreatePlan`**

When constructing `DesktopNodeHostServiceActionPlan`, pass:

```csharp
OperationFamily: ResolveOperationFamily(action),
```

- [ ] **Step 4: Run focused tests**

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "ServiceActionPlansDeclareStableOperationFamilies|ConfigureInstalledPlanUsesNativeServiceActionWithoutScmCommands|FirewallEnableRequiresLanApprovalBeforeMutation|TrustStoreInstallRequiresReleaseApprovalBeforeMutation" --no-restore
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Host/DesktopNodeHostServiceAction.cs src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md
git commit -m "test: lock host ops operation families"
```

## Task 3: Extract Event Log Ops First

**Files:**
- Create: `src/DesktopNode.Host/Ops/DesktopNodeEventLogOps.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [ ] **Step 1: Create Event Log ops class**

Create `src/DesktopNode.Host/Ops/DesktopNodeEventLogOps.cs`:

```csharp
namespace DesktopNode.Host.Ops;

internal static class DesktopNodeEventLogOps
{
    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsEventLogController eventLogController)
    {
        return DesktopNodeHostServiceAction.ExecuteNativeEventLogActionForOps(options, plan, eventLogController);
    }
}
```

- [ ] **Step 2: Expose internal bridge**

In `DesktopNodeHostServiceAction`, change `ExecuteNativeEventLogAction` from `private static` to:

```csharp
internal static DesktopNodeHostServiceActionResult ExecuteNativeEventLogActionForOps(
    DesktopNodeHostOptions options,
    DesktopNodeHostServiceActionPlan plan,
    IDesktopNodeWindowsEventLogController eventLogController)
```

Keep the method body unchanged.

- [ ] **Step 3: Route event-log actions through ops class**

In `ExecuteAsync`, replace the Event Log branch with:

```csharp
return Ops.DesktopNodeEventLogOps.Execute(
    options,
    plan,
    eventLogController is null
        ? new DesktopNodeWindowsEventLogController()
        : eventLogController);
```

- [ ] **Step 4: Run Event Log tests**

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "EventLog" --no-restore
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Host src/DesktopNode.Host.Tests
git commit -m "refactor: extract host event log ops"
```

## Task 4: Repeat Operation Family Extraction

**Files:**
- Create: `src/DesktopNode.Host/Ops/DesktopNodeFirewallOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeTrustStoreOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs`
- Create: `src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`

- [ ] **Step 1: Extract firewall branch**

Use the same bridge pattern as Event Log:

```csharp
internal static DesktopNodeHostServiceActionResult ExecuteNativeFirewallActionForOps(
    DesktopNodeHostOptions options,
    DesktopNodeHostServiceActionPlan plan,
    IDesktopNodeWindowsFirewallController firewallController)
```

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "Firewall" --no-restore
```

Expected: PASS.

- [ ] **Step 2: Extract trust-store branch**

Use:

```csharp
internal static DesktopNodeHostServiceActionResult ExecuteNativeTrustStoreActionForOps(
    DesktopNodeHostOptions options,
    DesktopNodeHostServiceActionPlan plan,
    IDesktopNodeWindowsTrustStoreController trustStoreController)
```

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "TrustStore" --no-restore
```

Expected: PASS.

- [ ] **Step 3: Extract credential-manager branch**

Use:

```csharp
internal static DesktopNodeHostServiceActionResult ExecuteNativeCredentialManagerActionForOps(
    DesktopNodeHostOptions options,
    DesktopNodeHostServiceActionPlan plan,
    IDesktopNodeWindowsCredentialManagerController credentialManagerController)
```

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "CredentialManager" --no-restore
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add src/DesktopNode.Host
git commit -m "refactor: split host ops families"
```

## Task 5: Full Host Ops Verification

**Files:**
- No source edits

- [ ] **Step 1: Run host and packaging guards**

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: PASS and no whitespace errors.
