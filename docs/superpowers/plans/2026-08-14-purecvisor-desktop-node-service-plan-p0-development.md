# Service Plan P0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `docs/SERVICE_PLAN.md` P0 네 항목(미디어 재장착, checkpoint restore reconcile, Hyper-V Saved, managed 승격)을 기존 Hyper-V 제어 평면 계약으로 닫아 lab 운영 공백을 줄인다.

**Architecture:** 네 슬라이스를 순서대로 연다. 각 mutation은 native WMI adapter + queued job이며 helper fallback이 없다. Web와 PCVCLI는 같은 route를 쓴다. P0-1은 eject의 짝이라 설계가 이 계획과 `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-media-attach-design.md`에 고정되어 있다. P0-2/P0-3/P0-4는 각 슬라이스 첫 작업에서 설계 문서를 쓰고 사용자 승인 후에만 product payload를 연다.

**Tech Stack:** C# / .NET, WMI (`Msvm_*`), xUnit, TypeScript served concatenation (`build-served-asset.mjs`), Node `vm` browser fixture, Pester 5, PCVCLI.

## Global Constraints

- Source planning: `docs/SERVICE_PLAN.md` Document-ID `purecvisor-desktop-node-service-plan-v1`
- P0-1 spec: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-media-attach-design.md`
- Operational current remains `0.42.73-admin-smoke`. Do not edit `docs/ga-ready/current-evidence.json` or generated current blocks.
- Do not open `0.42.74` or `0.42.73 -> next` package-pair. Trigger stays `product-payload-change-after-04273` until an approved installed campaign.
- Change tier `M`, verification lane `Full` for each slice. No MSI, service, Hyper-V host, firewall, or other host mutation in code-level tasks.
- Web and `pcvcli` use the same route. Mutations are queued jobs. Failures use `PCV_*` plus a next operator action. Native structured failure only.
- Keep Gen2-only, no disk shrink, managed delete guard, credential-ref, loopback default, `no-default-account`, TUI absent (ADR-0011).
- Do not implement P1 (clone/notes/template/guest-file/account CRUD/remaining reconcile) or P2 (noVNC target, periodic checkpoint, export/import, network editor, NIC/DVD add).
- Do not add USB/3D/HGFS/Type-2 engine/Linux runtime/public signing/external publication.
- New documents use Korean body; keep identifiers, routes, and problem codes in the original form.
- Every implementation task is RED then GREEN. Do not Skip expectation tests.
- Do not start Slice B/C/D implementation until that slice's design spec is approved.

## Decomposition

```text
P0-1 media attach          Tasks 1-8     (this plan + P0-1 spec)
  -> P0-2 restore reconcile Tasks 10-13  (design gate, then implement)
  -> P0-3 Saved suspend     Tasks 16-18  (design gate, then implement)
  -> P0-4 managed import    Tasks 21-23  (design gate, then implement)
```

각 슬라이스는 혼자 설치·테스트 가능한 제품 경로를 남긴다. P0-2는 새 route가 없고
`checkpoint.restore` family에 reconcile만 더한다.

## File map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` | Route catalog. P0-1 `POST .../attach`. P0-3 `.../save`, `.../resume-saved`. P0-4 `.../manage`. |
| `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs` | Enqueue attach/save/resume-saved/manage. |
| `src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.cs` | P0-2 restore reconcile + restore baseline capture. |
| `src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs` | Allowlist new operations. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` | Media request record, checkpoint `is_current`, manage types. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmMediaProvider.cs` | `vm.attach` HostResource set. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiCheckpointProvider.cs` | `is_current` via `Msvm_MostCurrentSnapshotInBranch`. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmPowerStateProvider.cs` | `vm.save` RequestedState `32769`, `vm.resume-saved` Enabled `2`. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmManageProvider.cs` | Create. Notes marker append. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Mutations.cs` | Pass `iso_path` / manage confirm / save. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs` | Domain catalog rows. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs` | Handler map. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs` | Provider operation lists. |
| `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs` | Wire manage provider. |
| `src/DesktopNode.Contracts/RuntimePolicy.cs` | Native mutation operation list / reason string. |
| `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs` | `vm attach`, later `vm save`, `vm resume-saved`, `vm manage`. |
| `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs` | Help/completion. |
| `web/src/served/routes.ts` | Allowlisted actions + coverage rows. |
| `web/src/served/api-client.ts` | `queueVmAttach` and later save/manage. |
| `web/src/served/mutate.ts` | Confirmation + queue. |
| `web/src/served/render-vm-detail.ts` | Attach form; later save/manage buttons. |
| `web/src/served-app.ts` | Click/submit wiring. |
| `web/app.js` | Only via `node scripts/build-served-asset.mjs --write`. |
| `docs/USER_FEATURE_USAGE_SPEC.md`, `docs/CLI_COMMAND_USAGE.md`, `docs/USER_GUIDE.md` | Operator copy. |
| `docs/ga-ready/evidence/service-plan-p0-*-code-level-2026-08-14.md` | Per-slice evidence. Do not promote current. |

---

# Slice A — P0-1 media attach

P0-1 spec approval locator: `User-Approval: service-plan-p0-media-attach-20260814`.
Do not start Task 1 until the user approves the spec and this plan.

Approved catalog increment for this slice: `56` → `57`.

### Task 1: Register `POST /api/v1/vms/{vmId}/attach`

**Files:**
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` (after the eject row)
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
- Test: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

**Interfaces:**
- Consumes: `NativeQueuedMutation(...)`.
- Produces: catalog row `POST /api/v1/vms/{vmId}/attach`, operation `QueueAttachVmMedia`, family `hyperv-vm`, stance `QueuedMutation`. Route count `57`. QueuedMutation count `23`.

- [ ] **Step 1: Write the failing catalog assertions**

In `DefaultContractMapsPhase25RouteCandidates` change the count and add the row next to eject:

```csharp
Assert.Equal(57, contract.Routes.Count);
Assert.Equal(57, routes.Count);
AssertRoute(routes[("POST", "/api/v1/vms/{vmId}/attach")], "POST", "QueueAttachVmMedia", MutationStance.QueuedMutation);
```

In `DefaultContractPinsCompleteRoutePermissionAndMutationSnapshot` change:

```csharp
Assert.Equal(23, routes.Count(route => route.MutationStance == MutationStance.QueuedMutation));
```

Leave the SHA-256 digest assertion as-is for this step so the test fails on count or missing route.

- [ ] **Step 2: Run the catalog test and confirm RED**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests --nologo`

Expected: FAIL because the attach route is missing and count is still 56/22.

- [ ] **Step 3: Add the catalog row**

Insert immediately after the eject line in `ApiHandlerAdapterContract.CreateDefault()`:

```csharp
NativeQueuedMutation("/api/v1/vms/{vmId}/eject", "QueueEjectVmMedia", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}/attach", "QueueAttachVmMedia", "hyperv-vm"),
```

- [ ] **Step 4: Refresh the snapshot digest and re-run**

Run the same test. Copy the actual SHA-256 from the digest assertion failure into:

```csharp
Assert.Equal("<actual-sha256-from-failure>", digest);
```

Re-run. Expected: PASS. Family count stays `13`. ReadOnly `22`, ProductOperation `12`.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Api/ApiHandlerAdapterContract.cs src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs
git commit -m "feat(api): register vm.attach queued media route"
```

---

### Task 2: Media request record and WMI attach

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` (`IDesktopNodeHyperVVmMediaProvider`, `DesktopNodeHyperVVmMediaInfo`)
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmMediaProvider.cs`
- Modify: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVNativeAdapterTests.cs` (`RecordingHyperVVmMediaProvider`)
- Test: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVNativeAdapterTests.cs`

**Interfaces:**
- Consumes: existing `FindDvdDrive`, `ModifyResourceSettings`, `PCV_VM_DVD_DRIVE_NOT_FOUND`.
- Produces:
  - `DesktopNodeHyperVVmMediaRequest(string Operation, string VmName, string? IsoPath = null)`
  - `IDesktopNodeHyperVVmMediaProvider.Invoke(DesktopNodeHyperVVmMediaRequest request, CancellationToken cancellationToken)`
  - `DesktopNodeHyperVVmMediaInfo` additive optional `iso_path`
  - WMI `vm.attach` sets `HostResource` to `new[] { isoPath }`
  - missing ISO → `PCV_ISO_NOT_FOUND` (same code as create)
  - missing body path → `PCV_VM_ATTACH_ISO_REQUIRED`
  - unknown operation → `PCV_OPERATION_NOT_ALLOWED`

- [ ] **Step 1: Write the failing recording-provider compile/tests**

Change `RecordingHyperVVmMediaProvider` to the new signature and add:

```csharp
[Fact]
public void NativeAdapterDispatchesVmAttachWithIsoPath()
{
    var media = new RecordingHyperVVmMediaProvider();
    var adapter = CreateAdapter(vmMediaProvider: media);
    using var parameters = JsonDocument.Parse(
        """{"name":"lab-vm","iso_path":"D:\\\\isos\\\\ubuntu.iso"}""");

    Assert.True(adapter.TryInvoke("vm.attach", parameters.RootElement, CancellationToken.None, out var result));
    Assert.True(result.Ok);
    Assert.Equal("attach", result.Data!.Value.GetProperty("action").GetString());
    Assert.Equal(@"D:\isos\ubuntu.iso", result.Data.Value.GetProperty("iso_path").GetString());
    Assert.Equal("vm.attach", media.LastRequest!.Operation);
    Assert.Equal(@"D:\isos\ubuntu.iso", media.LastRequest.IsoPath);
}

private sealed class RecordingHyperVVmMediaProvider : IDesktopNodeHyperVVmMediaProvider
{
    public DesktopNodeHyperVVmMediaRequest? LastRequest { get; private set; }

    public DesktopNodeHyperVVmMediaInfo Invoke(
        DesktopNodeHyperVVmMediaRequest request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        var action = request.Operation == "vm.attach" ? "attach" : "eject";
        return new DesktopNodeHyperVVmMediaInfo(request.VmName, action, request.IsoPath);
    }
}
```

Keep `CreateAdapter` helper's existing media-provider argument. If the current helper still constructs `new RecordingHyperVVmMediaProvider()` with the old `Invoke(operation, vmName, token)` signature, the project will not compile — that is the RED.

- [ ] **Step 2: Confirm RED**

Run: `dotnet test src/DesktopNode.HyperV.Tests/DesktopNodeHyperV.Tests.csproj --filter FullyQualifiedName~NativeAdapterDispatchesVmAttachWithIsoPath --nologo`

Expected: FAIL or compile error (`Invoke` signature / `vm.attach` not handled).

- [ ] **Step 3: Implement the request record and WMI provider**

Replace the media interface and info in `DesktopNodeHyperVModels.cs`:

```csharp
public interface IDesktopNodeHyperVVmMediaProvider
{
    DesktopNodeHyperVVmMediaInfo Invoke(
        DesktopNodeHyperVVmMediaRequest request,
        CancellationToken cancellationToken);
}

public sealed record DesktopNodeHyperVVmMediaRequest(
    string Operation,
    string VmName,
    string? IsoPath = null);

public sealed record DesktopNodeHyperVVmMediaInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("iso_path")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? IsoPath = null);
```

Replace `DesktopNodeHyperVWmiVmMediaProvider.Invoke` with:

```csharp
public DesktopNodeHyperVVmMediaInfo Invoke(
    DesktopNodeHyperVVmMediaRequest request,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();
    if (request.Operation is not ("vm.eject" or "vm.attach"))
    {
        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_OPERATION_NOT_ALLOWED",
            $"Operation '{request.Operation}' is not a native VM media operation.",
            "Use vm.eject or vm.attach for this native media mutation slice.",
            false);
    }

    if (request.Operation == "vm.attach" && string.IsNullOrWhiteSpace(request.IsoPath))
    {
        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_ATTACH_ISO_REQUIRED",
            "VM attach requires iso_path.",
            "Pass a JSON body with iso_path set to an existing host ISO file.",
            false);
    }

    if (request.Operation == "vm.attach" && !File.Exists(request.IsoPath!))
    {
        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_ISO_NOT_FOUND",
            $"ISO '{request.IsoPath}' was not found.",
            "Use an absolute path to an ISO that exists on this Hyper-V host.",
            false);
    }

    var scope = CreateScope();
    using var vm = FindVm(scope, request.VmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
        "PCV_VM_NOT_FOUND",
        $"VM '{request.VmName}' was not found.",
        "The VM was not present in the native Hyper-V VM inventory response.",
        false);

    using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
        "PCV_VM_SETTINGS_NOT_FOUND",
        $"VM '{request.VmName}' settings were not found.",
        "Msvm_VirtualSystemSettingData was not available for the VM.",
        true);

    using var dvdDrive = FindDvdDrive(settings, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
        "PCV_VM_DVD_DRIVE_NOT_FOUND",
        $"VM '{request.VmName}' has no virtual DVD drive to {(request.Operation == "vm.attach" ? "attach" : "eject")}.",
        "Attach a virtual DVD drive before using vm.eject or vm.attach. This slice does not create DVD devices.",
        false);

    dvdDrive["HostResource"] = request.Operation == "vm.attach"
        ? new[] { request.IsoPath! }
        : Array.Empty<string>();
    using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
    using var inParams = service.GetMethodParameters(ModifyResourceSettingsMethod);
    inParams["ResourceSettings"] = new[] { dvdDrive.GetText(TextFormat.WmiDtd20) };
    cancellationToken.ThrowIfCancellationRequested();
    using var outParams = service.InvokeMethod(ModifyResourceSettingsMethod, inParams, null);
    WaitForMethodResult(outParams, request.Operation, cancellationToken);

    return request.Operation == "vm.attach"
        ? new DesktopNodeHyperVVmMediaInfo(request.VmName, "attach", request.IsoPath)
        : new DesktopNodeHyperVVmMediaInfo(request.VmName, "eject");
}
```

Add `using System.IO;` if missing. Do not add a DVD device when `FindDvdDrive` returns null.

- [ ] **Step 4: Update `TryInvokeVmMedia` to build the request**

In `DesktopNodeHyperVNativeAdapter.Mutations.cs` replace the provider call:

```csharp
var isoPath = GetStringProperty(parameters, "iso_path");
if (operation == "vm.attach" && string.IsNullOrWhiteSpace(isoPath))
{
    result = DesktopNodeHyperVOperationResult.Failure(
        operation,
        "PCV_VM_ATTACH_ISO_REQUIRED",
        "VM attach requires iso_path.",
        "Pass params.iso_path with an existing host ISO file.",
        false);
    return true;
}

var data = vmMediaProvider.Invoke(
    new DesktopNodeHyperVVmMediaRequest(operation, vmName, isoPath),
    cancellationToken);
```

Update every other `IDesktopNodeHyperVVmMediaProvider` fake in this test project the same way.

- [ ] **Step 5: Re-run HyperV tests**

Run: `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --nologo`

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmMediaProvider.cs src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Mutations.cs src/DesktopNode.HyperV.Tests/DesktopNodeHyperVNativeAdapterTests.cs
git commit -m "feat(hyperv): attach ISO to existing virtual DVD"
```

---

### Task 3: Domain, dispatch, provider catalog, runtime policy

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs` (after `vm.eject`)
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs` (`["vm.eject"]` → include `vm.attach`)
- Modify: `src/DesktopNode.HyperV.Tests/HyperVDomainContractTests.cs`
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs` (`vm.eject` lists)
- Modify: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs`
- Test: `src/DesktopNode.HyperV.Tests/HyperVDomainContractTests.cs`, `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`

**Interfaces:**
- Consumes: Task 2 `vm.attach` operation name.
- Produces: domain row `vm.attach` / `VmLifecycle` / `vm-media-provider` / `VmMedia`. Runtime policy `NativeMutationOperations` and `NativeCore.Reason` include `vm.attach` immediately after `vm.eject`. Invoker allowlist includes `vm.attach`.

- [ ] **Step 1: Write failing domain/policy assertions**

`HyperVDomainContractTests`:

```csharp
[InlineData("vm.attach", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider")]
```

Add `"vm.attach"` next to `"vm.eject"` in the handler-operations collection assertion.

`RuntimePolicyContractTests` — insert `vm.attach` immediately after `vm.eject` in both the reason string and the `NativeMutationOperations` array.

- [ ] **Step 2: Run tests to confirm RED**

Run: `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --filter FullyQualifiedName~HyperVDomainContractTests --nologo`

Run: `dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter FullyQualifiedName~RuntimePolicyContractTests --nologo`

Expected: FAIL (`vm.attach` missing).

- [ ] **Step 3: Add catalog rows**

Domain (after eject):

```csharp
new("vm.eject", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider"),
new("vm.attach", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider"),
```

Dispatch catalog:

```csharp
new("vm.attach", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider", DesktopNodeHyperVAdapterDispatchHandler.VmMedia),
```

Wmi provider catalog operations: `["vm.eject", "vm.attach"]`.

Invoker: add `"vm.attach" or` next to `"vm.eject" or`.

RuntimePolicy: insert `"vm.attach"` after `"vm.eject"` in `NativeMutationOperations` and in the `Reason` comma-separated string.

- [ ] **Step 4: Re-run**

Run the two test commands from Step 2. Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs src/DesktopNode.HyperV.Tests/HyperVDomainContractTests.cs src/DesktopNode.Contracts/RuntimePolicy.cs src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs
git commit -m "feat(hyperv): catalog vm.attach as media-provider mutation"
```

---

### Task 4: API enqueue `QueueAttachVmMedia`

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.Fakes.cs` if the media fake still uses the old `Invoke`
- Test: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

**Interfaces:**
- Consumes: Task 1 `QueueAttachVmMedia`, Task 2 request params `name` + `iso_path`.
- Produces: `202` job `operation=vm.attach` with those params. Missing `iso_path` → HTTP 400 `PCV_VM_ATTACH_ISO_REQUIRED`. Worker dispatches to native adapter without helper fallback.

- [ ] **Step 1: Write failing processor tests**

Add next to `QueuedVmEjectWorkerDispatchesToNativeAdapterWithoutExternalFallback`:

```csharp
[Fact]
public void QueuedVmAttachWorkerDispatchesToNativeAdapterWithoutExternalFallback()
{
    var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
    var processor = CreateProcessor(
        nativeAdapter: new RecordingNativeHyperVVmMediaAdapter(nativeCalls));

    var create = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/vms/lab%20vm/attach",
        """{"iso_path":"D:\\\\isos\\\\ubuntu.iso"}"""));

    Assert.Equal(202, create.StatusCode);
    Assert.Equal("vm.attach", ReadJobOperation(create));
    DrainWorker(processor);

    var nativeCall = Assert.Single(nativeCalls);
    Assert.Equal("vm.attach", nativeCall.Operation);
    Assert.Equal("lab vm", nativeCall.Parameters.GetProperty("name").GetString());
    Assert.Equal(@"D:\isos\ubuntu.iso", nativeCall.Parameters.GetProperty("iso_path").GetString());
}

[Fact]
public void QueueAttachVmMediaRejectsMissingIsoPath()
{
    var processor = CreateProcessor();
    var response = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/vms/lab%20vm/attach",
        "{}"));

    Assert.Equal(400, response.StatusCode);
    Assert.Equal("PCV_VM_ATTACH_ISO_REQUIRED", ReadErrorCode(response));
}
```

Add InlineData next to the eject row:

```csharp
[InlineData("POST", "/api/v1/vms/lab%20vm/attach", """{"iso_path":"D:\\\\isos\\\\ubuntu.iso"}""", "vm.attach", "name", "lab vm")]
```

Use the same `ReadJobOperation` / `DrainWorker` / `ReadErrorCode` helpers the eject tests already use. If a helper name differs, copy the eject test locally and rename — do not invent a new helper.

- [ ] **Step 2: Run and confirm RED**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~QueuedVmAttachWorkerDispatchesToNativeAdapterWithoutExternalFallback --nologo`

Expected: FAIL (unhandled `QueueAttachVmMedia` or 404).

- [ ] **Step 3: Handle the route**

In `DesktopNodeApiVmMutationRouteHandler` add after `QueueEjectVmMedia`:

```csharp
case "QueueAttachVmMedia":
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.attach");
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.attach");
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var isoPath = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "iso_path");
        if (string.IsNullOrWhiteSpace(isoPath))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                "vm.attach",
                "PCV_VM_ATTACH_ISO_REQUIRED",
                "VM attach requires iso_path.",
                "Pass a JSON body with iso_path set to an existing host ISO file.",
                false);
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            "vm.attach",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["name"] = routeId.Value,
                ["iso_path"] = isoPath
            }),
            request.RequestId!));
    }
```

Update `RecordingNativeHyperVVmMediaAdapter` (and any other media fake) to the Task 2 interface if the API test project still fails to compile.

- [ ] **Step 4: Re-run API tests**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests --nologo`

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.Fakes.cs
git commit -m "feat(api): queue vm.attach jobs from ISO path body"
```

---

### Task 5: PCVCLI `vm attach`

**Files:**
- Modify: `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs`
- Modify: `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs`
- Modify: `src/DesktopNode.Cli/README.md`
- Test: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`

**Interfaces:**
- Consumes: Task 4 `POST /api/v1/vms/{vm}/attach` body `{ iso_path }`.
- Produces: `pcvcli vm attach <vm> --iso <path>` and `--iso_path` alias. No `--yes`.

- [ ] **Step 1: Write failing CLI catalog tests**

```csharp
[InlineData("vm attach ubuntu-lab-01 --iso D:\\isos\\ubuntu.iso", "POST", "/api/v1/vms/ubuntu-lab-01/attach")]
```

In the dictionary that maps operations to argv, add:

```csharp
["vm.attach"] = ["vm", "attach", "ubuntu-lab-01", "--iso", @"D:\isos\ubuntu.iso"],
```

Add a dedicated body assertion next to the create ISO test:

```csharp
[Fact]
public void VmAttachSendsIsoPathBody()
{
    var request = DesktopNodeCliCommandCatalog.Parse(["vm", "attach", "ubuntu-lab-01", "--iso", @"D:\isos\ubuntu.iso"]);
    using var document = JsonDocument.Parse(request.Body!);
    Assert.Equal(@"D:\isos\ubuntu.iso", document.RootElement.GetProperty("iso_path").GetString());
}
```

Interactive help test — add:

```csharp
Assert.Contains(lines, line => string.Equals(line.Trim(), "vm attach | Attach ISO media to the virtual DVD", StringComparison.Ordinal));
```

- [ ] **Step 2: Run and confirm RED**

Run: `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --filter FullyQualifiedName~DesktopNodeCliCommandCatalogTests --nologo`

Expected: FAIL (`attach` usage).

- [ ] **Step 3: Implement the command**

In the `vm` switch add `"attach" => VmAttach(args),` next to eject.

```csharp
private static DesktopNodeCliRequest VmAttach(IReadOnlyList<string> args)
{
    if (args.Count < 3)
    {
        throw Usage("Use: vm attach <vm> --iso <path>");
    }

    var parsed = ParseOptions(args.Skip(3).ToArray());
    var isoPath = Required(parsed.Options, "--iso", "--iso_path");
    var body = new SortedDictionary<string, object?>
    {
        ["iso_path"] = isoPath
    };

    return new DesktopNodeCliRequest(
        "POST",
        $"/api/v1/vms/{Segment(args[2])}/attach",
        JsonSerializer.Serialize(body, JsonOptions));
}
```

Use the existing `ParseOptions` / `Required` helpers that `vm create` uses. Update the usage string that currently ends with `pause|resume|rename|delete|checkpoint.` to include `attach`. Change help line `pcvcli vm eject|delete-status <vm>` to:

```text
  pcvcli vm attach <vm> --iso <path>
  pcvcli vm eject|delete-status <vm>
```

Interactive shell: add `"vm attach "` to `CompletionCandidates` (near eject if present; otherwise after `"vm resume "`). Add `new("vm attach", "Attach ISO media to the virtual DVD")` to `AvailableCommands`. Also add missing `vm eject` completion/help in this task so attach/eject stay paired.

README command list: insert `attach` next to `eject`.

- [ ] **Step 4: Re-run CLI tests**

Run: `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --nologo`

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs src/DesktopNode.Cli.Tests src/DesktopNode.Cli/README.md
git commit -m "feat(cli): add pcvcli vm attach --iso"
```

---

### Task 6: Web Console attach form

**Files:**
- Modify: `web/src/served/routes.ts`
- Modify: `web/src/served/api-client.ts`
- Modify: `web/src/served/types.ts`
- Modify: `web/src/served/errors.ts`
- Modify: `web/src/served/mutate.ts`
- Modify: `web/src/served/render-vm-detail.ts`
- Modify: `web/src/served-app.ts`
- Modify: `web/app.js` only via `node scripts/build-served-asset.mjs --write`
- Test: `web/tests/PcvDesktopWeb.Static.Tests.ps1` if it pins route strings; otherwise `npm test --prefix web` and `npm run verify:parity --prefix web`

**Interfaces:**
- Consumes: Task 4 `POST /api/v1/vms/{id}/attach` + `{ iso_path }`.
- Produces: `desktopApi.queueVmAttach(vmId, isoPath)`, confirmation showing VM display name and ISO path, RBAC `operate`.

- [ ] **Step 1: Write the failing Web assertions**

Add coverage row (keep existing `vm.media` eject row):

```ts
{ id: 'vm.media.attach', method: 'POST', route: '/api/v1/vms/{vm_id}/attach', view: 'vms', mutating: true, tokenRequired: true },
```

Change `requireRouteAction` allowlist:

```ts
['start', 'shutdown', 'poweroff', 'restart', 'eject', 'attach', 'delete-status', 'set-memory', 'set-vcpu', 'disk-resize']
```

If a static Pester test matches `vms/{vm_id}/eject`, add a sibling assertion for `/attach`. If none exists, add one `Should -Match '/api/v1/vms/\{vm_id\}/attach'` near other VM route assertions in `web/tests/PcvDesktopWeb.Static.Tests.ps1`.

- [ ] **Step 2: Run Web static/parity and confirm RED**

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"`

Expected: FAIL on the new attach route string until `web/app.js` is regenerated after implementation.

- [ ] **Step 3: Implement the surface**

`api-client.ts`:

```ts
queueVmAttach: (vmId: string, isoPath: string) => apiFetch(DESKTOP_NODE_API_ROUTES.vmAction(vmId, 'attach'), {
  method: 'POST',
  body: JSON.stringify({ iso_path: isoPath })
}),
```

Add the same method to `types.ts`.

`errors.ts`:

```ts
function buildVmAttachConfirmation(vmId: string, isoPath: string): string {
  const vm = state.selectedVm || findCachedVm(vmId);
  const vmName = getVmName(vm);
  return [
    `Attach ISO to VM ${vmName}?`,
    `VM id: ${vmId}`,
    `ISO: ${isoPath}`,
    'This queues a Hyper-V DVD media mutation.',
    'The existing virtual DVD HostResource is replaced. No USB or new DVD device is created.',
    'The result will appear in Tracked Jobs.'
  ].join('\n');
}
```

`mutate.ts`:

```ts
async function queueVmAttach(vmId, isoPath) {
  requireRbac('operate', 'VM attach');
  const path = String(isoPath || '').trim();
  if (!path) {
    throw normalizeError({
      code: 'PCV_VM_ATTACH_ISO_REQUIRED',
      message: 'Enter an ISO path.',
      detail: 'iso_path is required before queueing vm.attach.'
    });
  }
  if (!window.confirm(buildVmAttachConfirmation(vmId, path))) {
    return;
  }

  state.actionPending = true;
  setVmActionPending(vmId, 'attach');
  state.error = null;
  render();
  try {
    const job = await desktopApi.queueVmAttach(vmId, path);
    trackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    clearVmActionPending(vmId);
    render();
  }
}
```

`render-vm-detail.ts` — after the eject button, add a form in `vm-resource-grid`:

```html
<form class="vm-resource-form" data-action="vm-attach" data-vm-id="${escapeHtml(vmId)}">
  <input name="iso_path" type="text" placeholder="ISO path" aria-label="ISO path"${actionDisabled}>
  <button type="submit"${actionDisabled}>Attach media</button>
</form>
```

`served-app.ts` form handler, next to disk-resize:

```ts
} else if (form.dataset.action === 'vm-attach') {
  await queueVmAttach(form.dataset.vmId, data.get('iso_path'));
  form.reset();
}
```

Regenerate:

```powershell
node scripts/build-served-asset.mjs --write
```

working directory `web`.

- [ ] **Step 4: Re-run Web verification**

```powershell
npm test --prefix web
npm run verify:parity --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add web/src web/app.js web/tests
git commit -m "feat(web): attach ISO from VM detail"
```

---

### Task 7: Operator docs and P0-1 code-level evidence

**Files:**
- Modify: `docs/CLI_COMMAND_USAGE.md` (media section)
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md` (matrix row)
- Modify: `docs/USER_GUIDE.md` (VM media paragraph; add one if absent)
- Create: `docs/ga-ready/evidence/service-plan-p0-media-attach-code-level-2026-08-14.md`
- Modify: `docs/SERVICE_PLAN.md` §11 related-docs row
- Modify: `docs/DEVELOPER_INDEX.md` 2026-08-14 section
- Test: `git diff --check`

**Interfaces:**
- Consumes: Tasks 1-6 contracts.
- Produces: Korean operator copy. Evidence claims code-level only. `host_mutation_performed=false`. current not promoted.

- [ ] **Step 1: Add the CLI/usage sentences**

`docs/CLI_COMMAND_USAGE.md` table, after eject:

```markdown
| `pcvcli vm attach <vm> --iso <path>` | `POST /api/v1/vms/{vm}/attach` | Virtual DVD drive에 ISO를 연결하는 job queue. `--iso_path` alias |
```

Media section example:

```powershell
pcvcli --json vm attach ubuntu-lab-01 --iso D:\isos\ubuntu.iso
```

`docs/USER_FEATURE_USAGE_SPEC.md` matrix: add `VM media attach` row (Web form, `pcvcli vm attach`, `POST /vms/{id}/attach`).

- [ ] **Step 2: Write evidence**

Create `docs/ga-ready/evidence/service-plan-p0-media-attach-code-level-2026-08-14.md` with:

- slice id `service-plan-p0-media-attach-code-level-2026-08-14`
- source spec Design-ID `purecvisor-desktop-node-p0-media-attach-v1`
- route `POST /api/v1/vms/{vmId}/attach`, operation `vm.attach`
- catalog `57`, QueuedMutation `23`
- tests run (dotnet HyperV/Api/Cli/Contracts, npm web, Pester web)
- `host_mutation_performed: false`
- `package_candidate_created: false`
- `public_trusted_signing: false`
- `external_stable_publication: false`
- installed smoke `not-run`

Do not edit `docs/ga-ready/current-evidence.json`.

- [ ] **Step 3: Point indexes at the plan/evidence**

`docs/SERVICE_PLAN.md` §11 add:

```markdown
| `docs/superpowers/plans/2026-08-14-purecvisor-desktop-node-service-plan-p0-development.md` | P0 개발 계획 |
| `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-media-attach-design.md` | P0-1 attach 설계 |
```

`docs/DEVELOPER_INDEX.md` 2026-08-14 section: add that P0 execution plan is the file above and that this docs-only index edit does not open `0.42.74`.

- [ ] **Step 4: `git diff --check` then commit**

```powershell
git add docs/CLI_COMMAND_USAGE.md docs/USER_FEATURE_USAGE_SPEC.md docs/USER_GUIDE.md docs/ga-ready/evidence/service-plan-p0-media-attach-code-level-2026-08-14.md docs/SERVICE_PLAN.md docs/DEVELOPER_INDEX.md
git commit -m "docs: record P0-1 media attach code-level evidence"
```

---

### Task 8: P0-1 verification gate

**Files:** none new.

**Interfaces:** Consumes Tasks 1-7. Produces a local green suite. No host mutation.

- [ ] **Step 1: Run the required suite**

```powershell
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

Expected: all PASS. Do not run MSI, service, or Hyper-V mutation.

- [ ] **Step 2: Stop**

P0-1 code-level is closed. Do not start Slice B until the P0-2 design spec is written and approved.

---

# Slice B — P0-2 checkpoint restore reconcile

Stop here until the user approves `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-checkpoint-restore-reconciliation-design.md`.

Locked contracts (write them into that spec; do not weaken them):

- No new HTTP route. `POST /api/v1/jobs/{jobId}/reconcile` stays the only reconcile endpoint.
- Schema `pcv-checkpoint-restore-reconciliation/v1`.
- Enqueue of `POST /api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore` captures a read-only `checkpoint.list` baseline.
- `DesktopNodeHyperVCheckpointInfo` gains `is_current` (`bool?`). WMI source is `Msvm_MostCurrentSnapshotInBranch` (or equivalent association already used by Hyper-V). If current snapshot cannot be read, `capture_status=unavailable`.
- Reconcile runs only when job is `failed`, operation `checkpoint.restore`, error `PCV_JOB_INTERRUPTED`, baseline `captured`.
- `succeeded` only when exactly one checkpoint row has the requested name **and** `is_current=true`.
- Presence in the list without `is_current=true` is `not-applied` → HTTP 409 `PCV_JOB_RECONCILIATION_REQUIRED`, job stays `failed`.
- Duplicate names or missing current flag → 409, no mutation.
- Reconcile never calls `checkpoint.restore`.
- Web: `Reconcile restore` on interrupted restore rows (RBAC `operate`). CLI `job reconcile` already posts the same route.
- Do not auto-succeed because “the checkpoint still exists”.

### Task 10: Write the P0-2 design spec

**Files:**
- Create: `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-checkpoint-restore-reconciliation-design.md`

**Interfaces:**
- Consumes: locked contracts above; Wave 2C create spec `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-checkpoint-create-reconciliation.md`.
- Produces: approved-design document. Wait for `User-Approval: service-plan-p0-restore-reconcile-20260814`.

- [ ] **Step 1: Write the spec**

Copy the Wave 2C create spec structure. Replace create-specific tables with the locked restore table. State that `checkpoint.list` without `is_current` is insufficient. Host mutation `false`. Do not open a package.

- [ ] **Step 2: Ask the user to approve the spec**

Do not implement Task 11 until approval.

- [ ] **Step 3: Commit the spec after approval**

```powershell
git add docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-checkpoint-restore-reconciliation-design.md
git commit -m "docs: specify checkpoint restore reconciliation"
```

---

### Task 11: Add `is_current` to checkpoint.list

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` (`DesktopNodeHyperVCheckpointInfo`)
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiCheckpointProvider.cs`
- Modify: checkpoint list tests under `src/DesktopNode.HyperV.Tests` and API checkpoint list tests
- Test: existing checkpoint list test class (extend; do not create a second list API)

**Interfaces:**
- Consumes: `Msvm_SnapshotOfVirtualSystem` list already in `GetCheckpoints`.
- Produces: JSON field `is_current`. Exactly one row may be true. Read failure → all `is_current` null, no exception that hides the list.

- [ ] **Step 1: Write a failing mapping test**

Add a unit test that a fake/current-snapshot InstanceID marks one `DesktopNodeHyperVCheckpointInfo.IsCurrent == true` and others false. If the provider is sealed to WMI, test a new internal helper `MarkCurrentCheckpoint(IReadOnlyList<...>, string? currentInstanceId)` extracted in the same file.

```csharp
[Fact]
public void CheckpointListMarksSingleCurrentSnapshot()
{
    var marked = DesktopNodeHyperVWmiCheckpointProvider.MarkCurrent(
        [
            new DesktopNodeHyperVCheckpointInfo("before", "lab-vm", "2026-08-01T00:00:00Z", InstanceId: "snap-1"),
            new DesktopNodeHyperVCheckpointInfo("after", "lab-vm", "2026-08-02T00:00:00Z", InstanceId: "snap-2")
        ],
        currentInstanceId: "snap-2");

    Assert.False(marked[0].IsCurrent);
    Assert.True(marked[1].IsCurrent);
}
```

If adding `InstanceId` to the public record would leak WMI into the API, keep InstanceId internal to the provider and only serialize `is_current`. Then the helper takes `(name, isCurrent)` pairs instead. Prefer **not** adding `instance_id` to the public JSON.

- [ ] **Step 2: Confirm RED, then implement MarkCurrent + WMI current lookup**

Query `vm.GetRelated("Msvm_VirtualSystemSettingData", "Msvm_MostCurrentSnapshotInBranch", ...)`. Compare ElementName (or InstanceID internally) to list rows. Serialize `is_current` as bool.

- [ ] **Step 3: Run HyperV + API checkpoint list tests**

`dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --nologo`

`dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~Checkpoint --nologo`

Expected: PASS. Additive field only.

- [ ] **Step 4: Commit**

```powershell
git add src/DesktopNode.HyperV src/DesktopNode.HyperV.Tests src/DesktopNode.Api.Tests
git commit -m "feat(hyperv): expose checkpoint is_current on list"
```

---

### Task 12: Capture restore baseline and reconcile

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs` (restore enqueue must call `BuildCheckpointRestoreParameters`)
- Modify: `src/DesktopNode.Runtime.Tests/JobRuntimeReconciliationTests.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs` (restore reconcile cases, mirror create)
- Create if the packaging pattern is required: `packaging/windows-desktop-node/tests/fixtures/service-plan-p0-checkpoint-restore-reconciliation.json` and a Pester file matching Wave 2C create
- Test: `src/DesktopNode.Api.Tests` restore reconcile facts

**Interfaces:**
- Consumes: Task 11 `is_current`; schema `pcv-checkpoint-restore-reconciliation/v1`.
- Produces: `BuildCheckpointRestoreParameters(vmName, checkpointName, token)` stores:

```json
{
  "reconciliation": {
    "schema": "pcv-checkpoint-restore-reconciliation/v1",
    "capture_status": "captured",
    "before": { "current_name": "old", "vm_name": "lab-vm" },
    "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true }
  }
}
```

`HandleJobReconcile` treats `checkpoint.restore` like create. Update the “only rename/delete/create” error string to include restore.

Classification:

| readback | outcome |
| --- | --- |
| exactly one requested name with `is_current=true` | 200, job `succeeded`, action `reconciled` |
| requested name present, `is_current` false/null | 409 `not-applied` |
| zero rows with that name | 409 `not-applied` |
| multiple rows with that name | 409 `ambiguous-duplicate-checkpoint-names` |
| list failed / baseline unavailable | 409 `baseline-unavailable` |

- [ ] **Step 1: Write failing API tests**

Add facts beside the existing `checkpoint.create` reconcile tests in `ApiRuntimePolicyRequestProcessorTests.cs`. The interrupted restore job must include:

```json
"reconciliation": {
  "schema": "pcv-checkpoint-restore-reconciliation/v1",
  "capture_status": "captured",
  "before": { "current_name": "old", "vm_name": "lab-vm" },
  "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true }
}
```

Assert:

1. `checkpoint.list` readback `{ "name": "requested", "is_current": true }` → HTTP 200, job `succeeded`.
2. same name with `is_current: false` → HTTP 409 `PCV_JOB_RECONCILIATION_REQUIRED`, job stays `failed`.
3. missing name → 409 `not-applied`.
4. two rows with the same name → 409 `ambiguous-duplicate-checkpoint-names`.

- [ ] **Step 2: Confirm RED, implement handler + enqueue wiring, re-run**

Extend `ReconciliationRequiredError` mutation label: `"checkpoint.restore" => "checkpoint restore"`.

- [ ] **Step 3: Commit**

```powershell
git add src/DesktopNode.Api src/DesktopNode.Api.Tests src/DesktopNode.Runtime.Tests packaging/windows-desktop-node/tests
git commit -m "feat(api): reconcile interrupted checkpoint.restore"
```

---

### Task 13: Web restore reconcile affordance and P0-2 evidence

**Files:**
- Modify: `web/src/served/render-jobs.ts` (`canReconcileVmMutation`, `renderJobReconcileButton`)
- Modify: `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs` help text
- Create: `docs/ga-ready/evidence/service-plan-p0-checkpoint-restore-reconciliation-code-level-2026-08-14.md`
- Test: `web/tests` if a job-reconcile fixture exists; otherwise `npm test --prefix web` plus a static string assert

**Interfaces:**
- Consumes: Task 12 `checkpoint.restore` reconcile.
- Produces: button label `Reconcile restore` when `job.operation === 'checkpoint.restore'`.

- [ ] **Step 1: Write the failing allowlist assertion**

Change the intended production code in tests first by adding a fixture or static match for `Reconcile restore`. Then implement:

```ts
function canReconcileVmMutation(job) {
  const operation = String(job?.operation || '').toLowerCase();
  return String(job?.status || '').toLowerCase() === 'failed' &&
    ['vm.rename', 'vm.delete', 'checkpoint.create', 'checkpoint.restore'].includes(operation) &&
    String(job?.error?.code || '').toUpperCase() === 'PCV_JOB_INTERRUPTED';
}

function renderJobReconcileButton(job, canOperate) {
  if (!canReconcileVmMutation(job)) return '';
  const operation = String(job?.operation || '').toLowerCase();
  const label = operation === 'vm.delete'
    ? 'Reconcile delete'
    : operation === 'checkpoint.create'
      ? 'Reconcile checkpoint'
      : operation === 'checkpoint.restore'
        ? 'Reconcile restore'
        : 'Reconcile rename';
  return `<button data-action="reconcile-job" data-job-id="${escapeHtml(job.job_id)}"${canOperate ? '' : ' disabled'}>${label}</button>`;
}
```

CLI help:

```csharp
new("job reconcile", "Reconcile an interrupted rename, delete, checkpoint create, or restore"),
```

- [ ] **Step 2: Regenerate `web/app.js` and run web + CLI tests**

Expected: PASS.

- [ ] **Step 3: Evidence file, `host_mutation_performed=false`, current not promoted**
- [ ] **Step 4: Commit**

```powershell
git add web/src web/app.js src/DesktopNode.Cli docs/ga-ready/evidence/service-plan-p0-checkpoint-restore-reconciliation-code-level-2026-08-14.md
git commit -m "feat: operator restore reconcile affordance"
```

---

# Slice C — P0-3 Hyper-V Saved

Stop until `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-hyperv-saved-design.md` is approved (`User-Approval: service-plan-p0-saved-20260814`).

Locked contracts:

- Do **not** rename `pause`/`resume`.
- `POST /api/v1/vms/{vmId}/save` → job `vm.save` → `RequestStateChange` RequestedState `32769` (`SavedState`). Inventory already maps `32769` → `"saved"`.
- `POST /api/v1/vms/{vmId}/resume-saved` → job `vm.resume-saved` → RequestedState `2`, allowed only when current mapped state is `saved`. Otherwise `PCV_VM_NOT_SAVED`.
- `vm.pause` stays RequestedState `9`. `vm.resume` stays RequestedState `2` for paused VMs. Do not change those constants.
- Catalog increment from the then-current count (57 after P0-1) by **+2**. Family stays `hyperv-vm`, stance `QueuedMutation`.
- CLI: `pcvcli vm save <vm>`, `pcvcli vm resume-saved <vm>`.
- Web: `Save` and `Resume saved` buttons with confirmation showing VM name and current state.
- Required later: actual-VM evidence. Code-level tasks must not perform that mutation.

### Task 16: Write the P0-3 design spec

**Files:** Create the spec path above. Same gate as Task 10.

- [ ] **Step 1: Write spec from locked contracts**
- [ ] **Step 2: Wait for approval**
- [ ] **Step 3: Commit spec**

### Task 17: Power-state provider + catalog + API

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmPowerStateProvider.cs` (`public const ushort SavedState = 32769;`)
- Modify: Domain, dispatch (`vm-power-state-provider`), WmiProviderCatalog power-state operations list, RuntimePolicy lists, Invoker allowlist, `ApiHandlerAdapterContract`
- Modify: `DesktopNodeApiVmMutationRouteHandler` cases `QueueSaveVm`, `QueueResumeSavedVm`
- Test: `DesktopNodeHyperVWmiProviderTests.WmiVmPowerStateProviderUsesRequestStateChangeConstants` (assert `SavedState == 32769`), native adapter InlineData, API enqueue tests, catalog count + digest

**Interfaces:**
- `vm.save` / `vm.resume-saved` return `DesktopNodeHyperVVmPowerStateInfo(name, "save"|"resume-saved")`.
- Resume-saved reads current VM via existing `FindVm` EnabledState mapping; if not saved, throw `PCV_VM_NOT_SAVED`.

- [ ] **Step 1: Failing constant + catalog + enqueue tests**
- [ ] **Step 2: Implement, update snapshot digest, PASS**
- [ ] **Step 3: Commit** `feat(hyperv): add vm.save and vm.resume-saved`

### Task 18: CLI/Web/docs/evidence for Saved

**Files:** CLI catalog + interactive shell, Web routes allowlist `save`/`resume-saved`, detail buttons, `docs/CLI_COMMAND_USAGE.md`, `docs/USER_FEATURE_USAGE_SPEC.md`, evidence `docs/ga-ready/evidence/service-plan-p0-hyperv-saved-code-level-2026-08-14.md`

- [ ] **Step 1: Failing CLI/Web tests**
- [ ] **Step 2: Implement + regenerate `web/app.js`**
- [ ] **Step 3: Evidence, no current promotion, note actual-VM still required for SERVICE_PLAN 완료 조건**
- [ ] **Step 4: Commit** `feat: operator Hyper-V Saved save/resume-saved`

---

# Slice D — P0-4 managed import

Stop until `docs/superpowers/specs/2026-08-14-purecvisor-desktop-node-p0-managed-import-design.md` is approved (`User-Approval: service-plan-p0-managed-import-20260814`).

Locked contracts:

- This is **marker promotion**, not OVF import and not VHDX copy (P1 clone / P2 export stay closed).
- `POST /api/v1/vms/{vmId}/manage` → job `vm.manage`.
- Body `{ "confirm_name": "<exact Hyper-V display name>" }`. Mismatch → HTTP 400 `PCV_VM_MANAGE_CONFIRMATION_MISMATCH`.
- Provider `IDesktopNodeHyperVVmManageProvider` uses `ModifySystemSettings` like rename. Append `managed-by=purecvisor-desktop-node` to Notes; do not wipe other notes.
- Already managed → job success `action=already-managed` (idempotent). No second marker line.
- After success, `managed_by_purecvisor=true` and `DELETE` guard passes. Unmanaged delete still `PCV_VM_NOT_MANAGED_BY_PURECVISOR`.
- CLI: `pcvcli vm manage <vm> --yes`. Missing `--yes` → usage error (same pattern as delete).
- Web: confirmation shows VM name and that delete will become allowed.
- Catalog +1 queued mutation. Family `hyperv-vm`.

### Task 21: Write the P0-4 design spec

- [ ] **Step 1: Write spec from locked contracts**
- [ ] **Step 2: Wait for approval**
- [ ] **Step 3: Commit spec**

### Task 22: Manage provider + API + delete-guard proof

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmManageProvider.cs`
- Modify: Models, ProviderSet, Domain, dispatch, Wmi catalog, RuntimePolicy, Invoker, route catalog, `DesktopNodeApiVmMutationRouteHandler`
- Test: native adapter manage + already-managed; API confirm mismatch; delete still blocked for unmanaged; managed-after-manage fake allows delete enqueue

**Interfaces:**

```csharp
public interface IDesktopNodeHyperVVmManageProvider
{
    DesktopNodeHyperVVmManageInfo Invoke(string vmName, CancellationToken cancellationToken);
}

public sealed record DesktopNodeHyperVVmManageInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action);
```

`action` is `manage` or `already-managed`. Notes detection reuses `IsManagedVm` / `managed-by=purecvisor-desktop-node`.

- [ ] **Step 1: Failing tests for confirm mismatch, manage dispatch, unmanaged delete still blocked**
- [ ] **Step 2: Implement provider (ModifySystemSettings Notes append) + enqueue**
- [ ] **Step 3: Commit** `feat(hyperv): opt-in managed marker promotion`

### Task 23: CLI/Web/docs/evidence for manage

**Files:** CLI `--yes`, Web confirm, usage specs, evidence `docs/ga-ready/evidence/service-plan-p0-managed-import-code-level-2026-08-14.md`

- [ ] **Step 1: Failing CLI `--yes` and Web confirmation tests**
- [ ] **Step 2: Implement + regenerate `web/app.js`**
- [ ] **Step 3: Evidence, current not promoted**
- [ ] **Step 4: Commit** `feat: operator managed import opt-in`

---

# After P0 code-level

P0 네 항목이 각각 code-level evidence를 가지면 SERVICE_PLAN §9의 “설계→code-level”만 닫힌다. “설치본 evidence”는 별도 사용자 승인 캠페인이다. 그 캠페인을 이 계획이 시작하지 않는다.

P1 `managed full clone`은 P0-4 managed 정의가 제품에 있는 뒤에만 별도 설계를 연다.

---

## Self-review

### Spec coverage (`docs/SERVICE_PLAN.md`)

| 기획 항목 | 계획 위치 |
| --- | --- |
| P0-1 attach + CLI/Web same route | Slice A Tasks 1-8 |
| P0-2 restore reconcile, not list-implies-success | Slice B Tasks 10-13 |
| P0-3 Saved ≠ pause rename | Slice C Tasks 16-18 |
| P0-4 managed marker opt-in, reject path kept | Slice D Tasks 21-23 |
| Common contracts (queued, PCV_*, native, no current bump) | Global Constraints |
| P1/P2, Workstation rejects | Explicitly out of scope |
| No 0.42.74 / no public signing | Global Constraints + each evidence task |

### Placeholder scan

No TBD/TODO. P0-2/3/4 begin with a concrete spec path and locked contracts copied from this plan.

### Type consistency

- `vm.attach` / `QueueAttachVmMedia` / `iso_path` used from Task 1 through Task 7.
- Restore schema name `pcv-checkpoint-restore-reconciliation/v1` is the only restore schema.
- Saved operations are `vm.save` and `vm.resume-saved`, never a renamed `pause`.
- Manage operation is `vm.manage` with `confirm_name` / `--yes`.
