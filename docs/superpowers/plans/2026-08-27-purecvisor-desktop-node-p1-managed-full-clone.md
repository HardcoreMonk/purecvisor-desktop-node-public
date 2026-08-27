# P1-5 managed full clone Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** managed Gen2 Off VM의 독립 VHDX를 복사해 새 managed VM을 만드는 `vm.clone.preview`와 queued `vm.clone`을 기존 Hyper-V native 제어 평면에 연다.

**Architecture:** preview는 QoS preview처럼 즉시 `NativeProductOperation`이다. clone은 create/manage처럼 queued native mutation이다. 검사 로직은 WMI 없는 `DesktopNodeHyperVVmCloneGuard`가 소유하고, WMI provider는 스냅샷을 모아 guard 뒤에만 파일을 복사한다. 실패 rollback은 대상 디렉터리와 대상 VM만 지운다.

**Tech Stack:** C# / .NET 10, WMI `Msvm_*`, xUnit, TypeScript served concatenation (`build-served-asset.mjs`), Node feature-surface parity, PCVCLI.

**Spec:** `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-p1-managed-full-clone-design.md`

## Global Constraints

- 한국어 본문. 식별자, route, `PCV_*`, 파일 경로는 원문.
- Required CI 네 shard. 변경 중 focused `dotnet`/`web`만. `installer-policy`는 clean HEAD.
- `docs/ga-ready/current-evidence.json`과 generated current 블록을 쓰지 않는다.
- `0.42.76` package/fullgate/manual-admin pair를 열지 않는다. trigger는 `product-payload-change-after-04275`.
- Hyper-V VM, MSI, service, firewall mutation을 이 계획이 실행하지 않는다.
- canonical operator id는 `GET /api/v1/vms/{id}` 표시 이름이다. inventory `Id`를 GUID로 바꾸지 않는다.
- linked clone, export/import, TPM 복사, checkpoint flatten, Gen1, unmanaged 소스 클론을 구현하지 않는다.
- helper fallback 없음. Web와 `pcvcli`가 같은 route.
- 커밋 접두사 `feat:` / `test:` / `docs:`.
- Lane 2 clone 프로브와 Lane 3 승격은 이 계획의 체크박스가 아니다.

---

## File map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` | `POST .../clone/preview`, `POST .../clone` |
| `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` | catalog 62, ProductOperation 13, QueuedMutation 27, digest |
| `packaging/windows-desktop-node/tests/fixtures/http-transport-contract-v1.json` | `route_count` 62 |
| `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` | clone request/plan/info/provider interface |
| `src/DesktopNode.HyperV/DesktopNodeHyperVVmCloneGuard.cs` | 순수 거절/계획. WMI 없음 |
| `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVVmCloneGuardTests.cs` | unmanaged/Off/checkpoint/differencing/TPM/name conflict |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmCloneProvider.cs` | 스냅샷 수집, 스트림 복사, DefineSystem, rollback |
| `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs` | clone provider 배선 |
| `src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs` | `vm.clone.preview`, `vm.clone` |
| `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs` | domain catalog rows |
| `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs` | provider operation lists |
| `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Mutations.cs` | `TryInvoke` clone/preview |
| `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVNativeAdapterTests.cs` | Recording clone provider |
| `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs` | 400 confirm/name, 200 preview, 202 clone |
| `src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs` | allowlist `vm.clone.preview`, `vm.clone` |
| `src/DesktopNode.Contracts/RuntimePolicy.cs` | Native mutation list + reason |
| `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs` | reason 문자열 |
| `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs` | preview/clone HTTP |
| `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs` | `vm clone --yes` / `--dry-run` |
| `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs` | confirmation, dry-run, confirm_name |
| `web/src/served/routes.ts` | coverage `vm.clone.preview`, `vm.clone` |
| `web/src/served/api-client.ts` | preview/clone POST |
| `web/src/served/render-vm-detail.ts` | Clone VM 패널 |
| `web/src/served-app.ts` | click wiring |
| `web/app.js` | `node web/scripts/build-served-asset.mjs --write` only |
| `config/desktop-node-feature-surface-ledger.json` | feature `pcv.vm.clone` |
| `docs/FEATURE_IMPLEMENTATION_LEDGER.md` | 28 feature 투영. evidence 단계는 `not-assessed` |
| `docs/USER_FEATURE_USAGE_SPEC.md`, `docs/CLI_COMMAND_USAGE.md`, `docs/USER_GUIDE.md` | operator copy |
| `docs/ga-ready/evidence/service-plan-p1-managed-full-clone-code-level-2026-08-27.md` | code-level evidence. current 승격 없음 |

---

### Task 1: Register clone preview and clone routes

**Files:**
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` (after the manage row)
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/http-transport-contract-v1.json`

**Interfaces:**
- Consumes: `NativeProductOperation(...)`, `NativeQueuedMutation(...)`.
- Produces: catalog `62`. ProductOperation `13`. QueuedMutation `27`. Family count `13` 유지. ReadOnly `22` 유지.

- [ ] **Step 1: Write the failing catalog assertions**

`DefaultContractMapsPhase25RouteCandidates`와 snapshot 테스트에서:

```csharp
Assert.Equal(62, contract.Routes.Count);
Assert.Equal(62, routes.Count);
AssertRoute(
    routes[("POST", "/api/v1/vms/{vmId}/clone/preview")],
    "POST",
    "PreviewCloneVm",
    MutationStance.ProductOperation);
AssertRoute(
    routes[("POST", "/api/v1/vms/{vmId}/clone")],
    "POST",
    "QueueCloneVm",
    MutationStance.QueuedMutation);
Assert.Equal(13, routes.Count(route => route.MutationStance == MutationStance.ProductOperation));
Assert.Equal(27, routes.Count(route => route.MutationStance == MutationStance.QueuedMutation));
```

ledger route count assertion도 `60` → `62`. digest는 이 단계에서 그대로 두어 RED가 나게 한다.

- [ ] **Step 2: Run catalog tests RED**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests --nologo`

Expected: FAIL. route count still `60`.

- [ ] **Step 3: Add catalog rows**

`ApiHandlerAdapterContract.CreateDefault()` manage 행 바로 다음:

```csharp
NativeQueuedMutation("/api/v1/vms/{vmId}/manage", "vm.manage", "QueueManageVm", "pcv.vm.managed-import", "hyperv-vm"),
NativeProductOperation("/api/v1/vms/{vmId}/clone/preview", "vm.clone.preview", "PreviewCloneVm", "pcv.vm.clone", "hyperv-vm"),
NativeQueuedMutation("/api/v1/vms/{vmId}/clone", "vm.clone", "QueueCloneVm", "pcv.vm.clone", "hyperv-vm"),
```

`http-transport-contract-v1.json` `route_count`를 `62`로 바꾼다.

- [ ] **Step 4: Refresh digest and re-run**

digest 실패 메시지의 SHA-256을 `DefaultContractPinsCompleteRoutePermissionAndMutationSnapshot`에 넣는다. 재실행 PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Api/ApiHandlerAdapterContract.cs src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs packaging/windows-desktop-node/tests/fixtures/http-transport-contract-v1.json
git commit -m "feat(api): register vm.clone preview and queued clone routes"
```

---

### Task 2: Clone guard (no WMI)

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVVmCloneGuard.cs`
- Create: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVVmCloneGuardTests.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs`

**Interfaces:**
- Consumes: `DesktopNodeHyperVManagedNotes.Marker` 판정과 같은 managed bool.
- Produces: `DesktopNodeHyperVVmCloneGuard.Evaluate` — 거절 코드 또는 preview plan.

```csharp
public sealed record DesktopNodeHyperVVmCloneDiskSnapshot(
    string SourcePath,
    long FileLength,
    bool IndependentVhdx);

public sealed record DesktopNodeHyperVVmCloneSourceSnapshot(
    string Name,
    bool Managed,
    int Generation,
    string PowerState,
    int CheckpointCount,
    IReadOnlyList<DesktopNodeHyperVVmCloneDiskSnapshot> Disks,
    bool SecurityFeaturesPresent);

public sealed record DesktopNodeHyperVVmCloneRequest(
    string SourceName,
    string TargetName,
    string VmRoot);

public static class DesktopNodeHyperVVmCloneGuard
{
    public static bool TryPlan(
        DesktopNodeHyperVVmCloneSourceSnapshot source,
        DesktopNodeHyperVVmCloneRequest request,
        bool targetExists,
        out DesktopNodeHyperVVmClonePlan plan,
        out DesktopNodeHyperVNativeOperationException? error)
}
```

`DesktopNodeHyperVVmClonePlan` / `DesktopNodeHyperVVmCloneInfo` / `DesktopNodeHyperVVmCloneDiskPlan` JSON 이름은 spec §4.1과 같다. `action` preview는 `"preview"`.

거절 매핑:

| 조건 | 코드 |
| --- | --- |
| `!Managed` | `PCV_VM_NOT_MANAGED_BY_PURECVISOR` |
| `Generation != 2` | `PCV_VM_GENERATION_UNSUPPORTED` |
| `PowerState != "Off"` | `PCV_VM_CLONE_SOURCE_NOT_OFF` |
| `CheckpointCount > 0` | `PCV_VM_CLONE_CHECKPOINTS_PRESENT` |
| disk `IndependentVhdx == false` | `PCV_VM_CLONE_DISK_NOT_INDEPENDENT` |
| `SecurityFeaturesPresent` | `PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED` |
| `targetExists` | `PCV_VM_ALREADY_EXISTS` |
| `TargetName` Ordinal equals `SourceName` | `PCV_VM_CLONE_NAME_CONFLICT` |

성공 plan: `directory = Path.Combine(VmRoot, TargetName)`, disks `disk0.vhdx`…, `planned_copy_bytes = sum(FileLength)`.

- [ ] **Step 1: Write failing guard tests**

각 거절 코드와 한 개의 성공 plan. 성공 케이스 `FileLength=1024`면 `planned_copy_bytes=1024`, target `D:\\vms\\lab-vm-2\\disk0.vhdx`.

- [ ] **Step 2: Run RED**

Run: `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj --filter FullyQualifiedName~DesktopNodeHyperVVmCloneGuardTests --nologo`

Expected: FAIL, type missing.

- [ ] **Step 3: Implement guard only**

WMI/`ManagementObject`를 이 파일에 넣지 않는다.

- [ ] **Step 4: Run GREEN**

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.HyperV/DesktopNodeHyperVVmCloneGuard.cs src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs src/DesktopNode.HyperV.Tests/DesktopNodeHyperVVmCloneGuardTests.cs
git commit -m "feat(hyperv): add managed clone guard without WMI"
```

---

### Task 3: Native adapter dispatch with recording provider

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVModels.cs` (`IDesktopNodeHyperVVmCloneProvider`)
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.Mutations.cs`
- Modify: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVNativeAdapterTests.cs`

**Interfaces:**

```csharp
public interface IDesktopNodeHyperVVmCloneProvider
{
    DesktopNodeHyperVVmClonePlan Preview(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken);

    DesktopNodeHyperVVmCloneInfo Invoke(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken);
}
```

`TryInvoke("vm.clone.preview")`는 `Preview`를 호출하고 plan JSON을 data로 넣는다.
`TryInvoke("vm.clone")`는 `Invoke`를 호출하고 info JSON을 넣는다.
params: `source` 또는 `name`(소스), `target` 또는 대상 `name`. adapter는 spec enqueue params `source`/`name`을 읽는다. 대상은 `name`, 소스는 `source`.

Recording provider는 manage recording과 같이 호출 목록을 남긴다.

- [ ] **Step 1: Write failing adapter tests** for preview/clone mapping and `PCV_VM_NOT_FOUND` passthrough.
- [ ] **Step 2: Run RED**
- [ ] **Step 3: Wire provider + dispatch handler `VmClone` / `VmClonePreview`**
- [ ] **Step 4: Run GREEN**
- [ ] **Step 5: Commit** `feat(hyperv): dispatch vm.clone preview and clone`

이 단계에서 WMI clone provider는 `throw new NotImplementedException`이어도 된다. default WMI set은 컴파일만 통과하면 된다. 실제 WMI는 Task 4.

---

### Task 4: WMI clone provider copy and rollback

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmCloneProvider.cs`
- Modify: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVWmiProviderTests.cs` 또는 전용 `DesktopNodeHyperVWmiVmCloneProviderTests.cs`

**Interfaces:**
- Consumes: Task 2 guard, create provider의 `DefineSystem`/SCSI attach 패턴, `DesktopNodeHyperVManagedNotes.Marker`.
- Produces: Preview는 스냅샷+guard만. Invoke는 스트림 복사 후 DefineSystem. 실패 시 대상 `DestroySystem`+디렉터리 삭제.

복사:

```csharp
internal static void CopyVhdx(string source, string target, CancellationToken cancellationToken)
{
    using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    var buffer = new byte[1024 * 1024];
    int read;
    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
    {
        cancellationToken.ThrowIfCancellationRequested();
        output.Write(buffer, 0, read);
    }
}
```

`File.Copy`를 쓰지 않는다. cancel/`DefineSystem` 실패 테스트는 temp 디렉터리 fixture로 한다. 실제 Hyper-V `CreateScope(connect: true)`는 이 task의 required가 아니다. WMI 호출은 create 테스트가 쓰는 것과 같은 내부 seam이 없으면, 파일 복사/rollback helper를 `internal`로 테스트하고 WMI 조립은 recording adapter로 덮는다.

최소 GREEN:

1. `CopyVhdx`가 바이트를 복사한다.
2. cancel 시 대상 파일이 남지 않는다 (caller가 directory delete).
3. guard 실패면 copy 0.

전체 DefineSystem 경로는 설치본 Lane 2가 증명한다. code-level은 copy helper + guard 통합이면 충분하다고 이 계획이 고정한다. fake `IDesktopNodeHyperVVmCloneProvider`가 adapter 테스트를 이미 덮는다.

- [ ] **Step 1: RED copy helper tests**
- [ ] **Step 2: Implement `CopyVhdx` + WMI provider that calls guard then copy**
- [ ] **Step 3: GREEN**
- [ ] **Step 4: Commit** `feat(hyperv): copy independent VHDX for managed clone`

---

### Task 5: API preview 200 and clone 202

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.Fakes.cs`

**Interfaces:**
- Consumes: route `PreviewCloneVm` / `QueueCloneVm`.
- Produces: HTTP 400 `PCV_VM_CLONE_CONFIRMATION_MISMATCH` / `PCV_VM_CLONE_NAME_REQUIRED` / `PCV_VM_CLONE_NAME_CONFLICT`. preview는 invoker `vm.clone.preview`. clone은 job `vm.clone` params `{ "source", "name" }`.

`IsNativeOperationCandidate`에 `"vm.clone.preview" or "vm.clone"`를 `vm.manage` 옆에 추가한다.

확인 이름은 manage와 같이 body `confirm_name` vs decoded `{vmId}` Ordinal.

- [ ] **Step 1: Write failing API tests** (mismatch, missing name, same name, preview 200, clone 202)
- [ ] **Step 2: RED**
- [ ] **Step 3: Handler + allowlist**
- [ ] **Step 4: GREEN**
- [ ] **Step 5: Commit** `feat(api): queue vm.clone and return clone preview`

---

### Task 6: RuntimePolicy native list

**Files:**
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs`
- Modify: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`

`vm.clone.preview`와 `vm.clone`을 `vm.manage` 뒤에 넣고 reason 문자열을 같은 순서로 맞춘다.

- [ ] **Step 1: RED reason assertion**
- [ ] **Step 2: Update list + reason**
- [ ] **Step 3: GREEN**
- [ ] **Step 4: Commit** `feat(contracts): allow native vm.clone operations`

---

### Task 7: PCVCLI `vm clone`

**Files:**
- Modify: `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs`

```csharp
"clone" => VmClone(args),
```

```csharp
private static DesktopNodeCliRequest VmClone(IReadOnlyList<string> args)
{
    // pcvcli vm clone <source> --name <target> --yes
    // pcvcli vm clone <source> --name <target> --dry-run
}
```

`--dry-run` → `POST /api/v1/vms/{source}/clone/preview`. `--yes` 없이 dry-run이 아니면 `PCV_CLI_CONFIRMATION_REQUIRED|Use: vm clone <source> --name <target> --yes.`
body `confirm_name` = source 인자 그대로, `name` = `--name` 값.

- [ ] **Step 1: RED CLI tests** (no --yes, dry-run preview path, --yes clone path, confirm_name verbatim)
- [ ] **Step 2: Implement**
- [ ] **Step 3: GREEN** including interactive help `vm clone | Clone a managed VM to a new independent disk`
- [ ] **Step 4: Commit** `feat(cli): add pcvcli vm clone --yes and --dry-run`

---

### Task 8: Web Clone VM panel

**Files:**
- Modify: `web/src/served/routes.ts`
- Modify: `web/src/served/api-client.ts`
- Modify: `web/src/served/render-vm-detail.ts`
- Modify: `web/src/served/mutate.ts` (if confirmation helper lives there)
- Modify: `web/src/served-app.ts`
- Generate: `web/app.js` via `node web/scripts/build-served-asset.mjs --write`

coverage:

```ts
"vm.clone.preview": "POST /api/v1/vms/{vm_id}/clone/preview",
"vm.clone": "POST /api/v1/vms/{vm_id}/clone",
```

`requireRouteAction`에 `clone` 추가. 버튼 `data-action="vm-clone"`. confirmation에 소스 표시 이름과 대상 이름. preview로 `planned_copy_bytes`를 보여 준 뒤 clone POST. RBAC `operate`.

- [ ] **Step 1: RED** `npm run test:required --prefix web` after adding ledger? 아직 ledger가 없으면 routes만 추가해도 parity가 FAIL할 수 있다. 이 task는 routes/UI만 넣고, ledger는 Task 9와 같은 커밋으로 맞춘다. 실행자는 Task 8+9를 한 커밋으로 묶어도 된다. 분리한다면 Task 8 커밋 전에 `check:feature-surfaces`를 돌리지 말고 static route 테스트만 돌린다.
- [ ] **Step 2: Implement UI + regenerate served asset**
- [ ] **Step 3: `node web/scripts/build-served-asset.mjs --check` PASS**
- [ ] **Step 4: Commit** `feat(web): add managed clone confirmation on VM detail`

---

### Task 9: Surface ledger, operator docs, code-level evidence

**Files:**
- Modify: `config/desktop-node-feature-surface-ledger.json` — feature `pcv.vm.clone`, routes preview+clone, present api/cli/web
- Modify: `docs/FEATURE_IMPLEMENTATION_LEDGER.md` — 27→28. evidence 단계는 `not-assessed`. 승격 후보에 넣지 않는다
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md`
- Modify: `docs/CLI_COMMAND_USAGE.md`
- Modify: `docs/USER_GUIDE.md`
- Create: `docs/ga-ready/evidence/service-plan-p1-managed-full-clone-code-level-2026-08-27.md`
- Modify: `docs/DOCUMENTATION_INDEX.md` — evidence 한 줄
- `config/desktop-node-feature-evidence-ledger.json`는 쓰지 않는다. Lane 3 전 `not-assessed`

Evidence metadata:

```text
result: PASS
scope: code-level-p1-managed-full-clone
current_version_anchor: 0.42.75-admin-smoke
canonical_current_changed: false
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

- [ ] **Step 1: Add ledger feature + operator docs**
- [ ] **Step 2: `npm run test:required --prefix web` PASS** (present web=54 if 이전이 52)
- [ ] **Step 3: Write code-level evidence. current-evidence.json 금지**
- [ ] **Step 4: Commit** `docs: record P1 managed clone code-level contract`

---

### Task 10: Focused verification

- [ ] **Step 1:**

```powershell
dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~ApiHandlerAdapterContractTests|FullyQualifiedName~VmClone --nologo
dotnet test src/DesktopNode.HyperV.Tests --filter FullyQualifiedName~Clone --nologo
dotnet test src/DesktopNode.Cli.Tests --filter FullyQualifiedName~Clone --nologo
dotnet test src/DesktopNode.Contracts.Tests --filter FullyQualifiedName~RuntimePolicyContractTests --nologo
npm run test:required --prefix web
git diff --check
```

Expected: 관련 테스트 PASS. `current-evidence.json` diff 없음.

- [ ] **Step 2:** 구현 브랜치 PR. merge는 사용자 승인. Lane 2 clone 프로브는 별도 checkpoint.

---

## Self-review

| Spec 요구 | Task |
| --- | --- |
| preview NativeProductOperation | 1, 5 |
| clone queued | 1, 5 |
| catalog 62 / queued 27 | 1 |
| confirm_name Ordinal | 5, 7 |
| managed / Gen2 / Off / checkpoint / independent VHDX / TPM | 2 |
| 스트림 복사, File.Copy 금지 | 4 |
| 소스 불변, 대상 rollback | 4 |
| 새 marker only | 4 |
| CLI --yes / --dry-run | 7 |
| Web preview then clone | 8 |
| current-evidence / 0.42.76 금지 | Global, 9, 10 |
| display-name operator id | Global, 5, 7 |

Placeholder 없음. `IDesktopNodeHyperVVmCloneProvider` 서명은 Task 3이 정의하고 Task 4·5가 그대로 쓴다.
