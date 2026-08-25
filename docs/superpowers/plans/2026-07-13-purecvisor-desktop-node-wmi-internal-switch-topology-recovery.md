# WMI Internal Switch Topology Recovery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Classify arbitrary-name Hyper-V internal switches from WMI topology and close the recovery with a passing `0.42.61-admin-smoke` full admin host mutation gate.

**Architecture:** Keep `DesktopNodeHyperVWmiSwitchProvider` as the authoritative native provider. It will derive two facts from WMI associations—an internal management-port match and an external adapter binding—and pass those facts to a pure mapping boundary; only positive internal evidence without an external binding produces `Type=internal`. The existing structured incomplete-topology failure remains the conservative result for external, private, or unproven topology.

**Tech Stack:** .NET 10, C#, `System.Management`, xUnit, PowerShell 7, Pester, WiX/MSI packaging, Hyper-V WMI v2.

---

## Execution amendment (2026-07-13)

The task steps below preserve the original `0.42.61-admin-smoke` target and commands as execution
history. That package installed and passed service/MSI lifecycle work, but full-gate step 1 failed with
`PCV_NETWORK_INVENTORY_FAILED` because partial WMI switch projection returned empty object paths and
the first `GetRelated` call raised `System.InvalidOperationException`; OS mutation was not executed.
Elevated `SELECT *` diagnostics restored object paths and both association traversals. The amended final
target is `0.42.62-admin-smoke`: provenance `7f71f0a518c5b592f233373522d36b5401c3f1df`, batch
`full-admin-host-mutation-gate-20260713-04262` PASS with 2/2 steps, and installed current-card PASS.
Task 9 evidence therefore uses combined `0.42.60`/`0.42.61` predecessor RCA and `0.42.62` closure
documents; it does not rewrite the historical steps below.

## File map

- Modify `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs`: collect WMI topology facts and map them conservatively.
- Modify `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`: add focused mapping regression tests.
- Create `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04261/manifest.json`: define the new two-step privileged batch.
- Create `artifacts/run-logs/full-admin-host-mutation-gate-20260713-04261/Invoke-ElevatedGate.ps1`: run dry-run and execution under one elevated PowerShell process.
- Create `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04260-wsl-topology-rca.md`: preserve the failed predecessor and explain why OS mutation did not run.
- Create `docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04261.md`: record clean package provenance and hashes.
- Create `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04261-hostmutation.md`: record the recovered full gate.
- Create `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04261.md`: record installed Web/TUI/CLI verification.
- Modify `AGENTS.md`, `packaging/windows-desktop-node/README.md`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, `docs/ga-ready/EVIDENCE_INDEX.md`, and `docs/ga-ready/CONTROL_PLANE_INDEX.md`: promote `0.42.61` and retain `0.42.60` as a failed predecessor.
- Modify `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`: lock the promoted evidence links and internal-only release boundary.

### Task 1: RED/GREEN for arbitrary-name internal mapping

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs:855`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs:34`

- [ ] **Step 1: Add the failing internal-switch mapping test**

Insert after `WmiSwitchProviderMapsDefaultSwitchAsCompleteInternalTopology`:

```csharp
[Fact]
public void WmiSwitchProviderMapsProvenInternalSwitchWithoutNameHeuristic()
{
    var info = DesktopNodeHyperVWmiSwitchProvider.MapSwitch(
        "WSL (Hyper-V firewall)",
        hasInternalManagementPort: true);

    Assert.Equal("WSL (Hyper-V firewall)", info.Name);
    Assert.Equal("internal", info.Type);
    Assert.False(info.IsDefault);
    Assert.True(info.AllowManagementOs);
    Assert.Null(info.NetAdapterInterfaceDescription);
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~WmiSwitchProviderMapsProvenInternalSwitchWithoutNameHeuristic"
```

Expected: FAIL at compile time because the current `MapSwitch` method has no `hasInternalManagementPort` parameter. This is the expected missing-contract failure, not a typo.

- [ ] **Step 3: Add the minimum positive internal mapping**

Change the mapping method to:

```csharp
public static DesktopNodeHyperVSwitchInfo MapSwitch(
    string? name,
    bool hasInternalManagementPort = false)
{
    var switchName = string.IsNullOrWhiteSpace(name) ? "unknown" : name;
    var isDefault = string.Equals(switchName, "Default Switch", StringComparison.OrdinalIgnoreCase);
    var isInternal = isDefault || hasInternalManagementPort;
    return new DesktopNodeHyperVSwitchInfo(
        Name: switchName,
        Type: isInternal ? "internal" : "unknown",
        IsDefault: isDefault,
        AllowManagementOs: isInternal ? true : null,
        NetAdapterInterfaceDescription: null);
}
```

- [ ] **Step 4: Run the focused and existing Default Switch tests**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~WmiSwitchProviderMaps"
```

Expected: PASS for the existing Default Switch case and the new arbitrary-name internal case.

- [ ] **Step 5: Commit the first mapping slice**

```powershell
git add -- src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs
git commit -m "test: classify proven Hyper-V internal switches"
```

### Task 2: RED/GREEN for external-binding guard

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs`

- [ ] **Step 1: Add the failing external-binding guard test**

Insert after the arbitrary-name internal test:

```csharp
[Fact]
public void WmiSwitchProviderDoesNotClassifyExternallyBoundSwitchAsInternal()
{
    var info = DesktopNodeHyperVWmiSwitchProvider.MapSwitch(
        "corp-uplink",
        hasInternalManagementPort: true,
        hasExternalBinding: true);

    Assert.Equal("corp-uplink", info.Name);
    Assert.Equal("unknown", info.Type);
    Assert.False(info.IsDefault);
    Assert.Null(info.AllowManagementOs);
    Assert.Null(info.NetAdapterInterfaceDescription);
}
```

- [ ] **Step 2: Run the new guard test and verify RED**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~WmiSwitchProviderDoesNotClassifyExternallyBoundSwitchAsInternal"
```

Expected: FAIL at compile time because `MapSwitch` does not yet accept `hasExternalBinding`.

- [ ] **Step 3: Add the external-binding fact to the mapping contract**

Replace the mapping signature and `isInternal` assignment with:

```csharp
public static DesktopNodeHyperVSwitchInfo MapSwitch(
    string? name,
    bool hasInternalManagementPort = false,
    bool hasExternalBinding = false)
{
    var switchName = string.IsNullOrWhiteSpace(name) ? "unknown" : name;
    var isDefault = string.Equals(switchName, "Default Switch", StringComparison.OrdinalIgnoreCase);
    var isInternal = isDefault || (hasInternalManagementPort && !hasExternalBinding);
    return new DesktopNodeHyperVSwitchInfo(
        Name: switchName,
        Type: isInternal ? "internal" : "unknown",
        IsDefault: isDefault,
        AllowManagementOs: isInternal ? true : null,
        NetAdapterInterfaceDescription: null);
}
```

- [ ] **Step 4: Run all WMI switch mapping tests**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~WmiSwitchProvider"
```

Expected: PASS for Default Switch, arbitrary-name internal, and external-binding guard.

- [ ] **Step 5: Commit the guard slice**

```powershell
git add -- src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs
git commit -m "test: guard external Hyper-V switch topology"
```

### Task 3: Wire WMI topology evidence into the mapper

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs:1-45`
- Read-only RED evidence: `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04260/summary.json`
- Read-only topology evidence: `artifacts/run-logs/full-admin-host-mutation-gate-20260713-04260/switch-topology.json`

- [ ] **Step 1: Confirm the existing integration RED without rerunning mutation**

Run:

```powershell
$failed = Get-Content -Raw artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04260/summary.json | ConvertFrom-Json
if ($failed.status -ne 'failed' -or $failed.failed_step_id -ne 'service-msi-hyperv-admin-smoke') { throw 'unexpected 0.42.60 predecessor state' }
rg -n "PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE" artifacts/routeparity-service-msi-hyperv-batch-profile-20260713-04260
```

Expected: the batch is failed in step 1 and the route artifact contains `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`. This preserved host-level failure is the integration RED for the WMI association wiring.

- [ ] **Step 2: Replace the name-only provider with topology collection**

Use only `System.Management` plus the existing static WMI helpers. The completed provider body is:

```csharp
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiSwitchProvider : IDesktopNodeHyperVSwitchProvider
{
    public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        var internalPortNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var internalPortSearcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery("SELECT Name FROM Msvm_InternalEthernetPort")))
        {
            foreach (ManagementObject internalPort in internalPortSearcher.Get())
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (internalPort)
                {
                    var internalPortName = GetStringProperty(internalPort, "Name");
                    if (!string.IsNullOrWhiteSpace(internalPortName))
                    {
                        internalPortNames.Add(internalPortName);
                    }
                }
            }
        }

        using var switchSearcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery("SELECT ElementName, Name FROM Msvm_VirtualEthernetSwitch"));
        var switches = new List<DesktopNodeHyperVSwitchInfo>();
        foreach (ManagementObject item in switchSearcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (item)
            {
                var name = GetStringProperty(item, "ElementName") ??
                    GetStringProperty(item, "Name") ??
                    "unknown";
                var hasInternalManagementPort = false;
                var hasExternalBinding = false;
                using var relatedPorts = item.GetRelated("Msvm_EthernetSwitchPort");
                foreach (ManagementObject relatedPort in relatedPorts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using (relatedPort)
                    {
                        var relatedPortName = GetStringProperty(relatedPort, "Name");
                        hasInternalManagementPort |=
                            !string.IsNullOrWhiteSpace(relatedPortName) &&
                            internalPortNames.Contains(relatedPortName);

                        using var allocations = relatedPort.GetRelated("Msvm_EthernetPortAllocationSettingData");
                        foreach (ManagementObject allocation in allocations)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            using (allocation)
                            {
                                var hostResources = allocation.Properties["HostResource"]?.Value switch
                                {
                                    string value => [value],
                                    string[] values => values,
                                    _ => Array.Empty<string>()
                                };
                                hasExternalBinding |= hostResources.Any(resource =>
                                    resource.Contains("Msvm_ExternalEthernetPort", StringComparison.OrdinalIgnoreCase));
                            }
                        }
                    }
                }

                switches.Add(MapSwitch(name, hasInternalManagementPort, hasExternalBinding));
            }
        }

        return switches;
    }

    public static DesktopNodeHyperVSwitchInfo MapSwitch(
        string? name,
        bool hasInternalManagementPort = false,
        bool hasExternalBinding = false)
    {
        var switchName = string.IsNullOrWhiteSpace(name) ? "unknown" : name;
        var isDefault = string.Equals(switchName, "Default Switch", StringComparison.OrdinalIgnoreCase);
        var isInternal = isDefault || (hasInternalManagementPort && !hasExternalBinding);
        return new DesktopNodeHyperVSwitchInfo(
            Name: switchName,
            Type: isInternal ? "internal" : "unknown",
            IsDefault: isDefault,
            AllowManagementOs: isInternal ? true : null,
            NetAdapterInterfaceDescription: null);
    }
}
```

- [ ] **Step 3: Run the focused tests after WMI wiring**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~WmiSwitchProvider"
```

Expected: PASS with no new warning or analyzer error.

- [ ] **Step 4: Commit the WMI provider wiring**

```powershell
git add -- src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs
git commit -m "fix: derive internal Hyper-V switch topology from WMI"
```

### Task 4: Run code-level verification once

**Files:**
- Verify: `src/DesktopNode.sln`

- [ ] **Step 1: Run the full .NET solution tests**

```powershell
dotnet test src/DesktopNode.sln -c Release
```

Expected: exit code `0`; all solution test projects pass.

- [ ] **Step 2: Check formatting and repository state**

```powershell
dotnet format src/DesktopNode.sln --verify-no-changes
git diff --check
git status --short --branch
```

Expected: format and whitespace checks pass; tracked worktree is clean; `main` is ahead of `origin/main` and is not pushed by this plan.

### Task 5: Build and validate `0.42.61-admin-smoke`

**Files:**
- Generate: `artifacts/admin-smoke-package-20260713-04261/**`
- Generate: `artifacts/run-logs/admin-smoke-package-20260713-04261/**`

- [ ] **Step 1: Build the unsigned internal package**

```powershell
New-Item -ItemType Directory -Force artifacts/run-logs/admin-smoke-package-20260713-04261 | Out-Null
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.61-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260713-04261 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest *> artifacts/run-logs/admin-smoke-package-20260713-04261/build.log
```

Expected: exit code `0`; MSI, WiX symbols, payload, checksum, provenance, and publication descriptors exist.

- [ ] **Step 2: Validate version, provenance, and hashes**

```powershell
$root = Resolve-Path artifacts/admin-smoke-package-20260713-04261
$provenance = Get-Content -Raw (Get-ChildItem $root -Filter '*.provenance.json' | Select-Object -First 1).FullName | ConvertFrom-Json
$head = (git rev-parse HEAD).Trim()
if ($provenance.product.version -ne '0.42.61-admin-smoke') { throw 'package version mismatch' }
if ($provenance.git_commit -ne $head) { throw 'package provenance mismatch' }
Get-FileHash (Get-ChildItem $root -Filter '*.msi' | Select-Object -First 1).FullName -Algorithm SHA256
$provenance | Select-Object git_commit,signing_mode,signing_trust_model,@{Name='msi_sha256';Expression={$_.msi.sha256}},@{Name='payload_aggregate_sha256';Expression={$_.payload.aggregate_sha256}} | Format-List
```

Expected: version is `0.42.61-admin-smoke`; provenance equals the current fix commit; computed and recorded MSI hashes agree; provenance exposes the payload aggregate hash.

### Task 6: Prepare the privileged `0.42.61` batch

**Files:**
- Create: `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04261/manifest.json`
- Create: `artifacts/run-logs/full-admin-host-mutation-gate-20260713-04261/Invoke-ElevatedGate.ps1`

- [ ] **Step 1: Create the batch manifest with `apply_patch`**

Use the preserved `0.42.60` manifest as the structural baseline and make only these exact substitutions:

```text
full-admin-host-mutation-gate-20260713-04260 -> full-admin-host-mutation-gate-20260713-04261
0.42.60-admin-smoke -> 0.42.61-admin-smoke
routeparity-service-msi-hyperv-batch-profile-20260713-04260 -> routeparity-service-msi-hyperv-batch-profile-20260713-04261
os-mutation-gates-batch-profile-20260713-04260 -> os-mutation-gates-batch-profile-20260713-04261
```

Keep the route-parity step first with `retry_count=1`, the OS mutation step second with `retry_count=0`, ISO `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`, LAN prefix `http://[redacted-private-endpoint]:7777/`, and `requires_admin=true`/`mutates_host=true` for both steps.

- [ ] **Step 2: Create the elevated wrapper with `apply_patch`**

Use the preserved `0.42.60` wrapper body unchanged except for the manifest path and log directory changing from `04260` to `04261`. The wrapper must accept `DryRun`, `Execute`, or `Full`; `Full` must stop after a failing dry-run and otherwise execute the real batch, writing `dryrun-status.json`, `execute-status.json`, and `full-status.json`.

- [ ] **Step 3: Run a non-mutating manifest validation**

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04261/manifest.json -AllowHostMutation -DryRun
```

Expected: exit code `0`; both steps validate as privileged host mutations without executing them.

### Task 7: Execute and monitor the full admin host mutation gate

**Files:**
- Generate: `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04261/**`
- Generate: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260713-04261/**`
- Generate: `artifacts/os-mutation-gates-batch-profile-20260713-04261/**`
- Generate: `artifacts/run-logs/full-admin-host-mutation-gate-20260713-04261/**`

- [ ] **Step 1: Launch one elevated Full run**

```powershell
Start-Process -FilePath 'C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.3.0_x64__8wekyb3d8bbwe\pwsh.exe' -Verb RunAs -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-File',(Resolve-Path 'artifacts/run-logs/full-admin-host-mutation-gate-20260713-04261/Invoke-ElevatedGate.ps1'),'-Mode','Full') -Wait
```

Expected: the operator accepts UAC once. The wrapper writes `full-status.json` with `exit_code=0`.

- [ ] **Step 2: Monitor summaries, not full logs**

```powershell
$batch = Get-Content -Raw artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04261/summary.json | ConvertFrom-Json
$route = Get-Content -Raw artifacts/routeparity-service-msi-hyperv-batch-profile-20260713-04261/summary.json | ConvertFrom-Json
$os = Get-Content -Raw artifacts/os-mutation-gates-batch-profile-20260713-04261/summary.json | ConvertFrom-Json
$batch | Select-Object batch_id,status,ok,executed_steps,total_steps
$route | Select-Object ok,version,boot_time_unchanged,final_service
$os | Select-Object ok,version,boot_time_unchanged,final_service
```

Expected: batch `status=passed`, `ok=true`, `executed_steps=2`, `total_steps=2`; route and OS summaries both have `ok=true`. Inspect a narrow failure log only if a summary is not passing; do not repeat the same gate automatically.

- [ ] **Step 3: Verify installed topology and cleanup**

```powershell
$cli = 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe'
& $cli --json network inventory
Get-Service PureCVisorDesktopNode | Select-Object Status,StartType
Get-Content -Raw 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json'
Get-VM -Name 'pcv-*' -ErrorAction SilentlyContinue | Select-Object Name,State
```

Expected: `Default Switch` and `WSL (Hyper-V firewall)` are both `internal` with `allow_management_os=true`; service is Running/Automatic; installed version is `0.42.61-admin-smoke`; no temporary `pcv-*` VM remains.

### Task 8: Capture the installed operator current-card

**Files:**
- Generate: `artifacts/installed-operator-surface-current-card-20260713-04261/**`

- [ ] **Step 1: Run installed CLI/TUI/Web read-only checks**

Capture stdout, stderr, exit codes, and HTTP status under the new artifact root for these commands:

```powershell
$root = New-Item -ItemType Directory -Force artifacts/installed-operator-surface-current-card-20260713-04261
$product = 'C:\Program Files\PureCVisor\DesktopNode'
& "$product\pcvcli.exe" --json ops summary *> "$root\pcvcli-ops-summary-json.stdout.txt"
& "$product\pcvcli.exe" ops summary *> "$root\pcvcli-ops-summary-table.stdout.txt"
& "$product\pcvcli.exe" --json host status *> "$root\pcvcli-host-status-json.stdout.txt"
& "$product\pcvcli.exe" --json vm list *> "$root\pcvcli-vm-list-json.stdout.txt"
& "$product\pcvtui.exe" --smoke-once runtime *> "$root\pcvtui-smoke-runtime.stdout.txt"
& "$product\pcvtui.exe" --smoke-once job *> "$root\pcvtui-smoke-job.stdout.txt"
Invoke-WebRequest http://127.0.0.1/ -UseBasicParsing | Select-Object -ExpandProperty Content | Set-Content "$root\web-root.txt"
Invoke-WebRequest http://127.0.0.1/pcv-config.js -UseBasicParsing | Select-Object -ExpandProperty Content | Set-Content "$root\web-config.txt"
Invoke-WebRequest http://127.0.0.1/app.js -UseBasicParsing | Select-Object -ExpandProperty Content | Set-Content "$root\web-app.txt"
```

Expected: every command exits `0`; Web resources return HTTP `200`; captures contain the current full-gate batch and version; no token or password value is present.

- [ ] **Step 2: Write `summary.json` with measured values**

Create the summary with `schema_version=1`, `status=pass`, version and installed manifest `0.42.61-admin-smoke`, the exact clean/full-gate MSI hashes, full-gate batch ID, per-command exit codes, HTTP status codes, current-card identifiers, `token_value_observed=false`, `password_value_observed=false`, `host_mutation_performed=false-smoke-after-fullgate`, `public_trusted_signing=excluded`, and `external_stable_publication=not-claimed`.

### Task 9: Promote evidence and preserve the failed predecessor

**Files:**
- Create/modify all evidence and index files listed in the file map.

- [ ] **Step 1: Add a failing evidence-contract test**

Add one `It` block to `PcvAdminSmokeEvidenceDocs.Tests.ps1` that loads the new RCA, package, full-gate, and current-card documents plus the five current-anchor files. Assert:

```powershell
$package | Should -Match 'version:\s*`0\.42\.61-admin-smoke`'
$fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260713-04261`'
$currentCard | Should -Match 'result:\s*`PASS`'
$rca | Should -Match 'PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE'
$rca | Should -Match 'os_mutation_executed:\s*`false`'
$currentAnchors | Should -Match '0\.42\.61-admin-smoke'
$currentAnchors | Should -Match '0\.42\.60-admin-smoke.*failed predecessor'
$fullgate | Should -Match 'public trusted signing.*not'
```

- [ ] **Step 2: Run the new evidence test and verify RED**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -FullNameFilter '*0.42.61*' -Output Detailed
```

Expected: FAIL because the `0.42.61` evidence documents and promoted anchors do not exist yet.

- [ ] **Step 3: Write evidence from machine-readable summaries**

Use exact values from the package provenance/summary, full-gate/route/OS summaries, installed current-card summary, and the preserved `0.42.60` summaries. The documents must explicitly distinguish:

```text
0.42.60-admin-smoke: package and installed lifecycle succeeded; network.inventory failed; VM creation and OS mutation were not executed.
0.42.61-admin-smoke: WMI topology recovery package; both full-gate steps and installed current-card passed.
Boundary: internal AllowUnsignedDev admin-smoke only; no public trusted signing or external stable publication claim.
```

- [ ] **Step 4: Promote the current anchors**

Update only the current sections of `AGENTS.md`, the packaging README, current evidence ledger, evidence index, and control-plane index. Keep the `0.42.59` closed manual-admin package-pair as the latest manual-admin closure unless a separate approved manual-admin campaign is actually executed. Record `0.42.60` as the failed immediate predecessor rather than rewriting its artifact state.

- [ ] **Step 5: Run evidence and repository verification**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
git diff --check
git status --short --branch
```

Expected: Pester passes; no whitespace errors; only intended evidence/index/test files are modified.

- [ ] **Step 6: Commit the evidence closure**

```powershell
git add -- AGENTS.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/CONTROL_PLANE_INDEX.md docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04260-wsl-topology-rca.md docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04261.md docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04261-hostmutation.md docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04261.md
git commit -m "docs: promote 0.42.61 WMI topology recovery evidence"
```

### Task 10: Final verification and handoff

**Files:**
- Verify: repository and installed artifacts only.

- [ ] **Step 1: Invoke verification-before-completion**

Follow the repository skill and re-check the exact claims that will be reported: focused/full .NET tests, evidence Pester, package provenance, batch/route/OS summaries, installed version/service/topology, current-card summary, cleanup, and git state.

- [ ] **Step 2: Report without pushing**

Report the fix commits, package and operational MSI hashes, payload aggregate hash, full-gate batch, installed version, current-card result, preserved `0.42.60` failure boundary, and local branch state. Do not push `main` unless the user separately authorizes that new push.
