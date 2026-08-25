# AR-002 Feature ID Surface Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Subagents are not authorized for this checkpoint.

**Goal:** Assign every Desktop Node API route one stable Feature ID and mechanically prove each Web/CLI surface is either present with a real binding or explicitly excluded with a reason.

**Architecture:** A schema-validated surface ledger is the cross-language source of truth for Feature IDs, route ownership, permissions, surface decisions, and bindings. The static C# registry remains the runtime route source; .NET and Node tests compare it with the ledger. The existing four-feature promotion evidence ledger remains unchanged and is validated as a subset.

**Tech Stack:** .NET 10/C#/xUnit, TypeScript 5/Node.js, JSON Schema 2020-12, existing Pester 5 static tests, Markdown.

---

## Execution boundary

- Worktree: `.worktrees/ar002-feature-id-surface-parity`
- Branch: `codex/ar002-feature-id-surface-parity`
- Design: `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-feature-id-surface-parity-design.md`
- Baseline: API 299/299 PASS; CLI 143/143 PASS.
- No MSI build, service install, Hyper-V mutation, actual-VM action, trusted-signing claim, external publication, or generic PowerShell runtime fallback.
- Preserve `config/desktop-node-feature-evidence-ledger.json` at four candidates and preserve `pcv.vm.saved-lifecycle/actual_vm_tested/fail`.
- `surface_bindings.web.coverage_id` and `surface_bindings.cli.command` are the implementation-level realization of the approved present/excluded model.

## File structure

### Create

- `config/desktop-node-feature-surface-ledger.schema.json` — all-feature catalog schema.
- `config/desktop-node-feature-surface-ledger.json` — canonical route/surface/binding data.
- `web/scripts/verify-feature-surface-parity.mjs` — executable Web-to-ledger comparison.
- `docs/FEATURE_IMPLEMENTATION_LEDGER.md` — human route/stage/blocker projection.

### Modify

- `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` — add stable operation and feature fields.
- `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` — API/ledger and evidence-subset tests.
- `web/src/served/types.ts` — add `featureId` to coverage items.
- `web/src/served/routes.ts` — retain local IDs and add Feature IDs.
- `web/app.js` — regenerate the served asset from the changed TypeScript source.
- `web/package.json` — wire Node verifier into `npm test`.
- `web/tests/PcvDesktopWeb.Static.Tests.ps1` — schema/wiring assertions only.
- `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs` — execute ledger CLI bindings.
- `src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs` — verify documentation projection.
- `docs/USER_FEATURE_USAGE_SPEC.md` — link user features to stable IDs.

## Exclusion reasons

- `CLI_AUTH_SESSION`: `Account and JWT session lifecycle is Web/API-only; PCVCLI uses protected bearer-token resolution.`
- `CLI_CONSOLE_CAPABILITY`: `Global console capability discovery is API/Web-only; PCVCLI exposes VM-specific console handoff.`
- `WEB_RAW_TELEMETRY`: `Web Console uses inventory and ops-summary projections instead of raw per-VM telemetry endpoints.`
- `WEB_DIRECT_PREVIEW`: `Web Console exposes explicit direct control; this preview route remains API/CLI-only.`
- `WEB_TRANSIENT_LIFECYCLE`: `Web Console intentionally omits transient pause/resume controls; this route remains API/CLI-only.`
- `WEB_RENAME`: `Web Console does not expose rename in the current operator flow; this route remains API/CLI-only.`
- `WEB_COMBINED_LIMIT`: `Web Console exposes explicit QoS controls instead of the combined limit command; this route remains API/CLI-only.`

## Authoritative 60-route mapping

`Web` is the existing coverage ID or `excluded:<reason-key>`. `CLI` is the exact command token sequence or an exclusion.

| # | Feature ID | Operation ID | Method and route | Permission | Web | CLI |
|---:|---|---|---|---|---|---|
| 1 | `pcv.runtime.policy` | `runtime.policy` | `GET /api/v1/runtime/policy` | null | `runtime.policy` | `runtime policy` |
| 2 | `pcv.host.status` | `host.status` | `GET /api/v1/host/status` | `read` | `host.status` | `host status` |
| 3 | `pcv.vm.inventory` | `vm.list` | `GET /api/v1/vms` | `read` | `vm.list` | `vm list` |
| 4 | `pcv.job.lifecycle` | `job.list` | `GET /api/v1/jobs` | `read` | `job.list` | `job list` |
| 5 | `pcv.job.lifecycle` | `job.detail` | `GET /api/v1/jobs/{jobId}` | `read` | `job.detail` | `job get job-01` |
| 6 | `pcv.job.lifecycle` | `job.cancel` | `POST /api/v1/jobs/{jobId}/cancel` | `operate` | `job.cancel` | `job cancel job-01` |
| 7 | `pcv.job.lifecycle` | `job.retry` | `POST /api/v1/jobs/{jobId}/retry` | `operate` | `job.retry` | `job retry job-01` |
| 8 | `pcv.job.lifecycle` | `job.reconcile` | `POST /api/v1/jobs/{jobId}/reconcile` | `operate` | `job.reconcile` | `job reconcile job-01` |
| 9 | `pcv.ops.summary` | `ops.summary` | `GET /api/v1/ops/summary` | `read` | `ops.summary` | `ops summary` |
| 10 | `pcv.diagnostics.bundle` | `diagnostic.bundle.list` | `GET /api/v1/diagnostics/bundles` | `diagnostics.read` | `diagnostic.bundle.list` | `diagnostics bundle list` |
| 11 | `pcv.diagnostics.bundle` | `diagnostic.bundle.create` | `POST /api/v1/diagnostics/bundles` | `diagnostics.create` | `diagnostic.bundle.create` | `diagnostics bundle create` |
| 12 | `pcv.diagnostics.bundle` | `diagnostic.bundle.download` | `GET /api/v1/diagnostics/bundles/{bundleId}/download` | `diagnostics.read` | `diagnostic.bundle.download` | `diagnostics bundle download bundle-01 --output D:\\evidence\\bundle.json` |
| 13 | `pcv.account.session` | `auth.login` | `POST /api/v1/auth/login` | null | `auth.login` | `excluded:CLI_AUTH_SESSION` |
| 14 | `pcv.account.session` | `auth.loopback-session` | `POST /api/v1/auth/loopback-session` | null | `auth.loopback-session` | `excluded:CLI_AUTH_SESSION` |
| 15 | `pcv.account.session` | `auth.refresh` | `POST /api/v1/auth/refresh` | null | `auth.refresh` | `excluded:CLI_AUTH_SESSION` |
| 16 | `pcv.account.session` | `auth.logout` | `POST /api/v1/auth/logout` | null | `auth.logout` | `excluded:CLI_AUTH_SESSION` |
| 17 | `pcv.account.session` | `auth.session` | `GET /api/v1/auth/session` | `read` | `auth.session` | `excluded:CLI_AUTH_SESSION` |
| 18 | `pcv.account.session` | `auth.rbac` | `GET /api/v1/auth/rbac` | `read` | `auth.rbac` | `excluded:CLI_AUTH_SESSION` |
| 19 | `pcv.console.capabilities` | `console.capabilities` | `GET /api/v1/console/capabilities` | `read` | `console.capabilities` | `excluded:CLI_CONSOLE_CAPABILITY` |
| 20 | `pcv.network.inventory` | `network.inventory` | `GET /api/v1/network/inventory` | `read` | `network.inventory` | `network list` |
| 21 | `pcv.vm.delete` | `vm.delete-status` | `GET /api/v1/vms/{vmId}/delete-status` | `read` | `vm.delete-status` | `vm delete-status vm-01` |
| 22 | `pcv.vm.inventory` | `vm.detail` | `GET /api/v1/vms/{vmId}` | `read` | `vm.detail` | `vm get vm-01` |
| 23 | `pcv.vm.console-handoff` | `console.session` | `GET /api/v1/vms/{vmId}/console` | `console.view` | `console.session` | `vm console vm-01` |
| 24 | `pcv.vm.telemetry` | `vm.memory-stats` | `GET /api/v1/vms/{vmId}/memory-stats` | `read` | `excluded:WEB_RAW_TELEMETRY` | `vm memory-stats vm-01` |
| 25 | `pcv.vm.telemetry` | `vm.cpu-stats` | `GET /api/v1/vms/{vmId}/cpu-stats` | `read` | `excluded:WEB_RAW_TELEMETRY` | `vm cpu-stats vm-01` |
| 26 | `pcv.vm.qos` | `vm.blkio-get` | `GET /api/v1/vms/{vmId}/blkio` | `read` | `vm.blkio-get` | `vm blkio-get vm-01` |
| 27 | `pcv.vm.qos` | `vm.bandwidth` | `GET /api/v1/vms/{vmId}/bandwidth` | `read` | `vm.bandwidth` | `vm bandwidth vm-01` |
| 28 | `pcv.vm.guest-service-readback` | `vm.guest-agent-status` | `GET /api/v1/vms/{vmId}/guest-agent/status` | `read` | `vm.guest-agent-status` | `vm guest-agent-status vm-01` |
| 29 | `pcv.vm.guest-service-readback` | `vm.guest-ping` | `GET /api/v1/vms/{vmId}/guest-agent/ping` | `read` | `vm.guest-ping` | `vm guest-ping vm-01` |
| 30 | `pcv.vm.guest-execution` | `vm.guest.exec.preview` | `POST /api/v1/vms/{vmId}/guest/exec/preview` | `guest.exec` | `excluded:WEB_DIRECT_PREVIEW` | `vm guest-exec vm-01 --dry-run --credential-ref wincred:PureCVisor/guest/admin -- hostname` |
| 31 | `pcv.vm.guest-channel` | `vm.guest.channel.preview` | `POST /api/v1/vms/{vmId}/guest/channel/preview` | `guest.channel.configure` | `excluded:WEB_DIRECT_PREVIEW` | `vm guest-agent-ensure-channel vm-01 --dry-run` |
| 32 | `pcv.vm.guest-execution` | `vm.guest.exec` | `POST /api/v1/vms/{vmId}/guest/exec` | `guest.exec` | `vm.guest.exec` | `vm guest-exec vm-01 --credential-ref wincred:PureCVisor/guest/admin -- hostname` |
| 33 | `pcv.vm.guest-channel` | `vm.guest.channel.verify` | `POST /api/v1/vms/{vmId}/guest/channel/verify` | `guest.channel.configure` | `vm.guest.channel.verify` | `vm guest-agent-ensure-channel vm-01 --verify --credential-ref wincred:PureCVisor/guest/admin` |
| 34 | `pcv.vm.guest-channel` | `vm.guest.channel.ensure` | `POST /api/v1/vms/{vmId}/guest/channel` | `guest.channel.configure` | `vm.guest.channel.ensure` | `vm guest-agent-ensure-channel vm-01 --repair --yes` |
| 35 | `pcv.checkpoint.lifecycle` | `checkpoint.list` | `GET /api/v1/vms/{vmId}/checkpoints` | `read` | `checkpoint.list` | `vm checkpoint list vm-01` |
| 36 | `pcv.vm.qos` | `vm.qos.storage.preview` | `POST /api/v1/vms/{vmId}/qos/storage/preview` | `operate` | `vm.qos.storage.preview` | `vm blkio-set vm-01 --disk disk0 --maximum-iops 1200 --dry-run` |
| 37 | `pcv.vm.qos` | `vm.qos.network.preview` | `POST /api/v1/vms/{vmId}/qos/network/preview` | `operate` | `vm.qos.network.preview` | `vm bandwidth-set vm-01 --adapter adapter0 --maximum-kbps 2048 --dry-run` |
| 38 | `pcv.vm.create` | `vm.create` | `POST /api/v1/vms` | `operate` | `vm.create` | `vm create vm-01 --vcpu 2 --memory_mb 4096 --disk_size_gb 40 --iso_path D:\\isos\\windows.iso` |
| 39 | `pcv.checkpoint.lifecycle` | `checkpoint.create` | `POST /api/v1/vms/{vmId}/checkpoints` | `operate` | `checkpoint.create` | `vm checkpoint create vm-01 --name before-upgrade` |
| 40 | `pcv.checkpoint.restore` | `checkpoint.restore` | `POST /api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore` | `operate` | `checkpoint.restore` | `vm checkpoint restore vm-01 before-upgrade` |
| 41 | `pcv.checkpoint.lifecycle` | `checkpoint.delete` | `DELETE /api/v1/vms/{vmId}/checkpoints/{checkpointId}` | `operate` | `checkpoint.delete` | `vm checkpoint delete vm-01 before-upgrade` |
| 42 | `pcv.vm.power-lifecycle` | `vm.start` | `POST /api/v1/vms/{vmId}/start` | `operate` | `vm.lifecycle` | `vm start vm-01` |
| 43 | `pcv.vm.power-lifecycle` | `vm.shutdown` | `POST /api/v1/vms/{vmId}/shutdown` | `operate` | `vm.lifecycle` | `vm guest-shutdown vm-01` |
| 44 | `pcv.vm.power-lifecycle` | `vm.poweroff` | `POST /api/v1/vms/{vmId}/poweroff` | `operate` | `vm.lifecycle` | `vm stop vm-01` |
| 45 | `pcv.vm.power-lifecycle` | `vm.restart` | `POST /api/v1/vms/{vmId}/restart` | `operate` | `vm.lifecycle` | `vm restart vm-01` |
| 46 | `pcv.vm.pause-lifecycle` | `vm.pause` | `POST /api/v1/vms/{vmId}/pause` | `operate` | `excluded:WEB_TRANSIENT_LIFECYCLE` | `vm pause vm-01` |
| 47 | `pcv.vm.pause-lifecycle` | `vm.resume` | `POST /api/v1/vms/{vmId}/resume` | `operate` | `excluded:WEB_TRANSIENT_LIFECYCLE` | `vm resume vm-01` |
| 48 | `pcv.vm.saved-lifecycle` | `vm.save` | `POST /api/v1/vms/{vmId}/save` | `operate` | `vm.save` | `vm save vm-01` |
| 49 | `pcv.vm.saved-lifecycle` | `vm.resume-saved` | `POST /api/v1/vms/{vmId}/resume-saved` | `operate` | `vm.resume-saved` | `vm resume-saved vm-01` |
| 50 | `pcv.vm.rename` | `vm.rename` | `POST /api/v1/vms/{vmId}/rename` | `operate` | `excluded:WEB_RENAME` | `vm rename vm-01 vm-02` |
| 51 | `pcv.vm.managed-import` | `vm.manage` | `POST /api/v1/vms/{vmId}/manage` | `operate` | `vm.manage` | `vm manage vm-01 --yes` |
| 52 | `pcv.vm.media-eject` | `vm.eject` | `POST /api/v1/vms/{vmId}/eject` | `operate` | `vm.media` | `vm eject vm-01` |
| 53 | `pcv.vm.media-attach` | `vm.attach` | `POST /api/v1/vms/{vmId}/attach` | `operate` | `vm.media.attach` | `vm attach vm-01 --iso D:\\isos\\windows.iso` |
| 54 | `pcv.vm.resource-limits` | `vm.limit` | `POST /api/v1/vms/{vmId}/limit` | `operate` | `excluded:WEB_COMBINED_LIMIT` | `vm limit vm-01 --cpu 4 --memory-mb 4096` |
| 55 | `pcv.vm.qos` | `vm.qos.storage.set` | `POST /api/v1/vms/{vmId}/qos/storage` | `operate` | `vm.qos.storage.set` | `vm blkio-set vm-01 --disk disk0 --maximum-iops 1200 --yes` |
| 56 | `pcv.vm.qos` | `vm.qos.network.set` | `POST /api/v1/vms/{vmId}/qos/network` | `operate` | `vm.qos.network.set` | `vm bandwidth-set vm-01 --adapter adapter0 --maximum-kbps 2048 --yes` |
| 57 | `pcv.vm.resource-limits` | `vm.set-memory` | `POST /api/v1/vms/{vmId}/set-memory` | `operate` | `vm.resource-mutation` | `vm set-memory vm-01 4096` |
| 58 | `pcv.vm.resource-limits` | `vm.set-vcpu` | `POST /api/v1/vms/{vmId}/set-vcpu` | `operate` | `vm.resource-mutation` | `vm set-vcpu vm-01 4` |
| 59 | `pcv.vm.resource-limits` | `vm.disk-resize` | `POST /api/v1/vms/{vmId}/disk-resize` | `operate` | `vm.resource-mutation` | `vm disk-resize vm-01 80` |
| 60 | `pcv.vm.delete` | `vm.delete` | `DELETE /api/v1/vms/{vmId}` | `operate` | `vm.delete` | `vm delete vm-01 --yes` |

## Task 1: Machine ledger and API route ownership

**Files:**
- Create: `config/desktop-node-feature-surface-ledger.schema.json`
- Create: `config/desktop-node-feature-surface-ledger.json`
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs:9-210`
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs:1-570`

- [ ] **Step 1: Write API/ledger failing tests**

Add `DefaultContractMatchesFeatureSurfaceLedger` and `EvidenceCandidateFeaturesAreKnownSurfaceFeatures`. Load repository files by walking parents from `AppContext.BaseDirectory`. Flatten the JSON routes and assert:

```csharp
Assert.Equal(60, ledgerRoutes.Count);
Assert.Equal(60, ledgerRoutes.Keys.Distinct().Count());
Assert.Equal(27, surfaceFeatureIds.Count);
Assert.All(contract.Routes, route => Assert.Matches("^pcv\\.[a-z0-9._-]+$", route.FeatureId));
Assert.All(contract.Routes, route => Assert.Matches("^[a-z0-9.-]+$", route.OperationId));

foreach (var route in contract.Routes)
{
    var key = (route.Method, route.RouteTemplate);
    Assert.True(ledgerRoutes.TryGetValue(key, out var ledgerRoute), $"Missing route: {key}");
    Assert.Equal(route.FeatureId, ledgerRoute.FeatureId);
    Assert.Equal(route.OperationId, ledgerRoute.OperationId);
    Assert.Equal(route.RequiredPermission, ledgerRoute.RequiredPermission);
}
```

Also assert target surfaces are exactly API/CLI/Web, API is always present, CLI and Web are each exactly one of present/excluded, exclusion reasons are nonblank, and Feature IDs/titles/routes are unique and nonempty. For evidence IDs use `Assert.Subset(surfaceFeatureIds, evidenceFeatureIds)`, then assert the evidence set count is four and contains `pcv.vm.saved-lifecycle`.

- [ ] **Step 2: Run the API test and verify RED**

Run:

```text
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHandlerAdapterContractTests" --nologo
```

Expected: FAIL because `FeatureId`, `OperationId`, and the surface files are absent.

- [ ] **Step 3: Create the schema and unbound ledger**

The top level requires `$schema`, `schema_version: 1`, `contract: pcv-feature-surface-ledger-v1`, exact unique `target_surfaces`, and nonempty `features`. Each feature requires `feature_id`, `title`, and nonempty `routes`. Every object uses `additionalProperties: false`.

Each route requires this complete shape:

```json
{
  "operation_id": "runtime.policy",
  "method": "GET",
  "route_template": "/api/v1/runtime/policy",
  "required_permission": null,
  "present_surfaces": ["api", "cli", "web"],
  "excluded_surfaces": []
}
```

Schema constraints: operation pattern `^[a-z0-9.-]+$`; methods `GET|POST|DELETE`; route prefix `/api/v1/`; permission string-or-null; surface enum `api|cli|web`; excluded item requires `surface` (`cli|web`) and nonblank `reason`. Permit optional `surface_bindings.web.coverage_id` and `surface_bindings.cli.command` (string array, minimum two tokens) for Tasks 2 and 3.

Translate every mapping row. Use exact exclusion reason text, not keys. The result is 27 features and 60 unique routes. Do not add evidence verdicts.

- [ ] **Step 4: Add IDs to the C# route contract**

Use this exact record shape:

```csharp
public sealed record ApiHandlerRouteContract(
    string RouteTemplate,
    string Method,
    string AuthPolicy,
    string? RequiredPermission,
    MutationStance MutationStance,
    string OperationId,
    string OperationName,
    string FeatureId,
    string DefaultOwner,
    string RouteFamily);
```

Add `operationId` and `featureId` parameters to all five factory helpers and pass named arguments. Update all 60 calls from the table. Keep every existing route, handler name, method, permission, owner, stance, and family unchanged.

- [ ] **Step 5: Run GREEN tests**

Run the filtered test, then the complete API project. Expected: 0 failures, route count 60, feature count 27, evidence candidate count four.

- [ ] **Step 6: Commit Task 1**

```text
git add config/desktop-node-feature-surface-ledger.schema.json config/desktop-node-feature-surface-ledger.json src/DesktopNode.Api/ApiHandlerAdapterContract.cs src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs
git commit -m "feat: map API routes to stable feature IDs"
```

## Task 2: Web coverage binding parity

**Files:**
- Create: `web/scripts/verify-feature-surface-parity.mjs`
- Modify: `config/desktop-node-feature-surface-ledger.json`
- Modify: `web/package.json`
- Modify: `web/src/served/types.ts:61-68`
- Modify: `web/src/served/routes.ts:42-89`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1:1-48,230-270`

- [ ] **Step 1: Add the verifier and npm hook**

The Node verifier loads the ledger, transpiles `routes.ts` with the installed `typescript`, executes it in `node:vm`, and appends `globalThis.__pcvCoverage = DESKTOP_NODE_ROUTE_COVERAGE` in the same script. Stub `normalizeError`. Normalize snake-case parameters to `{vmId}`, `{jobId}`, `{checkpointId}`, `{bundleId}`. Expand final-segment alternatives such as `start|shutdown|poweroff|restart` and `set-memory|set-vcpu|disk-resize`.

Compare all Web-present ledger tuples `(method, route, featureId, coverage_id)` with the executed coverage array. Reject missing, extra, duplicate, or unbound tuples with:

```javascript
function fail(message) {
  throw new Error(`PCV_FEATURE_SURFACE_PARITY_FAILED|surface=web|${message}`);
}
```

Add:

```json
"check:feature-surfaces": "node scripts/verify-feature-surface-parity.mjs",
"test": "npm run check:feature-surfaces && tsc --noEmit -p tsconfig.json && npm run check:served && npm run check:frontend-batches"
```

- [ ] **Step 2: Run RED Web verification**

Run `npm run check:feature-surfaces --prefix web`. Expected: `PCV_FEATURE_SURFACE_PARITY_FAILED` because Feature IDs and Web bindings are absent.

- [ ] **Step 3: Add Web Feature IDs and bindings**

Add `featureId: string` after `id` in `PcvRouteCoverageItem`; add each table Feature ID while preserving local `id`. Use these exact aggregate entries:

```typescript
{ id: 'vm.lifecycle', featureId: 'pcv.vm.power-lifecycle', method: 'POST', route: '/api/v1/vms/{vm_id}/start|shutdown|poweroff|restart', view: 'vms', mutating: true, tokenRequired: true },
{ id: 'vm.resource-mutation', featureId: 'pcv.vm.resource-limits', method: 'POST', route: '/api/v1/vms/{vm_id}/set-memory|set-vcpu|disk-resize', view: 'vms', mutating: true, tokenRequired: true },
```

For every Web-present ledger route add `"web": { "coverage_id": "<Web table cell>" }` under `surface_bindings`. Multiple exact routes may share either aggregate coverage ID.

- [ ] **Step 4: Add narrow Pester assertions**

Define ledger/schema/verifier paths in `BeforeAll`. Add one test using `Test-Json -SchemaFile`, checking verifier existence, checking `package.scripts.test` starts with `npm run check:feature-surfaces`, and checking served source contains `featureId`. Do not duplicate Node comparison logic in PowerShell.

- [ ] **Step 5: Run GREEN Web verification**

```text
npm run build:served --prefix web
npm test --prefix web
npm run verify:parity --prefix web
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
```

Expected: generation exits 0, all verification commands PASS, and generated `web/app.js` matches TypeScript.

- [ ] **Step 6: Commit Task 2**

```text
git add config/desktop-node-feature-surface-ledger.json web/package.json web/scripts/verify-feature-surface-parity.mjs web/src/served/types.ts web/src/served/routes.ts web/tests/PcvDesktopWeb.Static.Tests.ps1 web/app.js
git commit -m "test: enforce Web feature surface parity"
```

## Task 3: CLI binding parity

**Files:**
- Modify: `config/desktop-node-feature-surface-ledger.json`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs:583-709`

- [ ] **Step 1: Write the ledger-driven CLI failing test**

Replace `RoutesEveryDesktopNodeHyperVRuntimeOperationThroughPcvCli` with `RoutesEveryDeclaredCliSurfaceBindingThroughPcvCli`. Load every route whose `present_surfaces` contains `cli`, require `surface_bindings.cli.command`, pass the JSON string array to `DesktopNodeCliCommandCatalog.CreateRequest`, remove the returned query, and match the ledger template with:

```csharp
private static bool MatchesRouteTemplate(string template, string actualPath)
{
    var expected = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
    var actual = actualPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return expected.Length == actual.Length && expected.Zip(actual).All(pair =>
        (pair.First.StartsWith('{') && pair.First.EndsWith('}')) ||
        string.Equals(pair.First, pair.Second, StringComparison.Ordinal));
}
```

Assert 53 CLI-present bindings and seven CLI exclusions. Include operation ID and Feature ID in assertion messages.

- [ ] **Step 2: Run RED CLI verification**

```text
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --filter "FullyQualifiedName~RoutesEveryDeclaredCliSurfaceBindingThroughPcvCli" --nologo
```

Expected: FAIL because CLI-present routes have no `surface_bindings.cli.command`.

- [ ] **Step 3: Add all 53 CLI bindings**

Split each non-excluded CLI table cell into the exact JSON token array. Preserve Windows paths as `D:\\evidence\\bundle.json` and `D:\\isos\\windows.iso`. Merge with any Web binding:

```json
"surface_bindings": {
  "web": { "coverage_id": "runtime.policy" },
  "cli": { "command": ["runtime", "policy"] }
}
```

Do not add CLI bindings to the six auth routes or global console capability route.

- [ ] **Step 4: Run GREEN CLI/API verification**

```text
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --nologo
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --nologo
```

Expected: 0 failures; semantic counts are 60 routes, 27 features, 53 CLI-present, and seven CLI-excluded.

- [ ] **Step 5: Commit Task 3**

```text
git add config/desktop-node-feature-surface-ledger.json src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs
git commit -m "test: enforce CLI feature surface parity"
```

## Task 4: Human ledger and user-spec projection

**Files:**
- Create: `docs/FEATURE_IMPLEMENTATION_LEDGER.md`
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md:49-70,375-385`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs:70-124`

- [ ] **Step 1: Write documentation projection failing tests**

Add `DocumentsEveryStableFeatureIdAndSurfaceLedgerLink`. Load the surface JSON, implementation ledger, and user usage spec. For each feature compute `anchor = featureId.Replace('.', '-')` and assert:

```csharp
Assert.Contains($"<a id=\"{anchor}\"></a>", implementationLedger, StringComparison.Ordinal);
Assert.Contains($"[ `{featureId}` ](FEATURE_IMPLEMENTATION_LEDGER.md#{anchor})", featureUsage, StringComparison.Ordinal);
```

For every route assert the human ledger contains its operation ID and canonical `METHOD route`. Assert stage labels `code_tested`, `packaged`, `installed_tested`, `actual_vm_tested`, `manual_admin_tested`, `not-assessed`, and blocker `pcv.vm.saved-lifecycle/actual_vm_tested/fail`. Update the three existing exact feature-matrix row assertions to include the added `Feature ID` column; do not weaken them.

- [ ] **Step 2: Run RED documentation verification**

```text
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --filter "FullyQualifiedName~DesktopNodeCliProjectContractTests" --nologo
```

Expected: FAIL because the implementation ledger and links do not exist.

- [ ] **Step 3: Create the human implementation ledger**

Create a boundary statement, 27-feature anchor summary, 60-route projection, evidence-stage projection, blocker section, and non-claims section. Use these exact P0 stages from `04274-p0-fail.json`:

| Feature ID | code | packaged | installed | actual VM | manual admin | blocker |
|---|---|---|---|---|---|---|
| `pcv.checkpoint.restore` | pass | pass | pass | pass | pass | none |
| `pcv.vm.managed-import` | pass | pass | pass | pass | pass | none |
| `pcv.vm.media-attach` | pass | pass | pass | pass | pass | none |
| `pcv.vm.saved-lifecycle` | pass | pass | pass | fail | pass | `pcv.vm.saved-lifecycle/actual_vm_tested/fail` |

Set all five feature-scoped stages for the other 23 IDs to `not-assessed`. State operational current `0.42.74-admin-smoke` separately from feature promotion ineligible/one blocker.

- [ ] **Step 4: Link the user feature usage spec**

Add a `Feature ID` column to the 18-row matrix. Add `Feature ID 추적` with all 27 links in this exact form:

```markdown
| [ `pcv.vm.saved-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-saved-lifecycle) | 가상 머신 전원 작업 — Saved/Resume saved |
```

Update the media row to cover attach/eject and link both IDs. Preserve all commands, routes, roles, error codes, and safety language.

- [ ] **Step 5: Run GREEN documentation/cross-surface verification**

```text
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --nologo
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --nologo
npm test --prefix web
```

Expected: all PASS and every machine Feature ID has both human-ledger and user-spec links.

- [ ] **Step 6: Commit Task 4**

```text
git add docs/FEATURE_IMPLEMENTATION_LEDGER.md docs/USER_FEATURE_USAGE_SPEC.md src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs
git commit -m "docs: publish feature implementation ledger"
```

## Task 5: Full verification and bounded review

**Files:**
- Verify only; modify an already listed file only for a directly caused defect.

- [ ] **Step 1: Run complete targeted verification**

```text
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --nologo
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --nologo
npm test --prefix web
npm run verify:parity --prefix web
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
git diff --check
```

Expected: every command exits 0; zero failed and zero skipped tests in the targeted suites.

- [ ] **Step 2: Verify scope and immutable boundaries**

```text
git diff 54869e5..HEAD --name-only
git diff 54869e5..HEAD -- config/desktop-node-feature-evidence-ledger.json
git status --short --branch
```

Expected: only planned files changed; evidence-ledger diff empty; worktree clean after commits.

- [ ] **Step 3: Perform one regular review**

Review the diff for 60 routes, 27 features, Web 52 present/eight excluded routes, CLI 53 present/seven excluded routes, duplicate ownership, permission drift, route normalization, raw secrets, runtime PowerShell additions, and release/host-mutation overclaims.

- [ ] **Step 4: Perform at most one narrow re-review if needed**

For one directly caused defect, run its smallest failing test, fix only that defect, rerun the complete targeted verification once, and commit specifically. On the same cause twice or a circuit limit, stop and report.

- [ ] **Step 5: Hand off without release claims**

Report branch, commits, exact test counts, Web/CLI exclusion counts, unchanged P0 promotion status, and out-of-scope findings. Do not package, create release evidence, merge, or mutate the host without the next explicit approval.
