# Product Config Migration Apply Implementation Boundary

> **현행화(2026-05-07):** 이 문서는 최초에는 plan-only 경계였지만, 후속 구현 작업에서 code-level actual apply 경로가 구현됐고 `0.38.6-admin-smoke` installed destructive admin smoke PASS로 닫혔다. 이 closure는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

**Goal:** `product config migration apply`의 ownership, schema, backup, atomic replace, rollback evidence 요구사항과 2026-05-06 code-level 구현 상태를 고정한다.

**Architecture:** `DesktopNode.Host.exe service-action config-migration-apply --migration-plan-id product-config-v1-to-v2 --migration-plan-version 1`은 owned service identity, stopped service proof, owned product manifest, source schema v1, supported plan identity를 통과할 때 product manifest를 schema v2로 백업 후 same-directory temp replace로 교체한다. Missing precondition은 config write 없이 blocked diagnostics를 반환한다.

**Tech Stack:** C#/.NET product action candidate, product manifest/config JSON, Windows service stopped precondition, same-volume backup/atomic replace, Korean Markdown evidence.

**구현 상태:** 2026-05-06 code-level actual apply 구현이 완료됐고, 2026-05-07 `artifacts/config-jobstore-migration-apply-installed-20260507-0386`으로 installed destructive admin smoke가 PASS됐다. 이 action은 Local API route, 암묵적 service stop/start, MSI, Hyper-V, firewall, trust-store, LAN, Event Log, update, rollback, reboot를 실행하지 않는다.

---

## Scope

Included:

- Product config source inventory requirements.
- Current schema owner resolution requirements.
- Owned config path and source version evidence requirements.
- Backup root, atomic replace, rollback diagnostics requirements.
- Service stopped precondition and service-start preflight descriptor requirements.
- Blocked diagnostics/no-mutation behavior when any evidence is missing.

Excluded:

- Local API route exposure for config migration apply.
- Implicit service stop/start, install/repair/remove, MSI, update/rollback, Hyper-V, firewall, trust-store, LAN, Event Log, Task Scheduler, reboot.
- 명시적 `0.38.6-admin-smoke` evidence closure 이후 범위의 추가 installed destructive admin smoke.
- Public trusted signing or external stable publication evidence.

## Required Invariants

- 명시적 `0.38.6-admin-smoke` installed destructive admin smoke PASS 이후 `promotion_state`는 `current-native`다.
- Validation and dry-run descriptors may read config metadata, but validation must not write config, data root, token, job store, service identity, or diagnostics state outside normal test artifacts.
- Config mutation must be rejected if config ownership, schema version, migration plan id, source path, target path, service stopped state, backup root, or rollback behavior is unclear.
- Partial config migration is forbidden. A failed migration must leave either the original config intact or restore it from the owned backup with diagnostics.
- The action is owned by `dotnet-config-migration-action`, not by the runtime request processor, MSI default path, PowerShell helper, or external stable release flow.

## 구현 파일 지도

- Modify: `src/DesktopNode.Host/**` or a new focused `src/DesktopNode.ProductOps/**` owner for a native product action.
- Modify: `src/DesktopNode.Api/**` only if the future route is intentionally exposed through the Local API; otherwise keep it as service-action only.
- Modify: `src/DesktopNode.*.Tests/**` for precondition, blocked diagnostics, backup/atomic replace, rollback, and no-mutation tests.
- Modify: `packaging/windows-desktop-node/README.md` and `docs/DEVELOPMENT_VERIFICATION_POLICY.md` only after code-level verification exists.
- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` only after evidence changes the row state.

This file map is not authorization to edit those files now.

## Task 1: Pre-Implementation Inventory Gate

- [x] **Step 1: Record config source inventory**

Future implementation must produce a dry-run descriptor with these fields before any apply action can exist:

```json
{
  "operation": "product.config.migration.inventory",
  "ok": true,
  "config_sources": [
    {
      "name": "product-manifest",
      "path": "C:\\Program Files\\PureCVisor\\DesktopNode\\product-manifest.json",
      "owned": true,
      "schema_version": 1,
      "version": "installed-manifest-version"
    }
  ],
  "data_root": "C:\\ProgramData\\PureCVisor\\desktop-node",
  "service_name": "PureCVisorDesktopNode",
  "mutation_planned": false
}
```

- [x] **Step 2: Block if inventory is incomplete**

If any source path, owner, schema version, or installed version cannot be proven, the future action must return blocked diagnostics and must not create backups:

```json
{
  "ok": false,
  "operation": "product.config.migration.apply",
  "error": {
    "code": "PCV_CONFIG_MIGRATION_PRECONDITION_MISSING",
    "message": "Product config migration apply is blocked because config ownership or schema evidence is incomplete.",
    "retryable": false
  },
  "mutation_performed": false
}
```

## Task 2: Migration Plan Identity Gate

- [x] **Step 1: Require a migration plan id/version**

Future apply must require a stable migration descriptor:

```json
{
  "migration_plan_id": "product-config-v1-to-v2",
  "migration_plan_version": 1,
  "source_schema_version": 1,
  "target_schema_version": 2,
  "requires_service_stopped": true,
  "partial_migration_allowed": false
}
```

- [x] **Step 2: Block unknown plan ids**

Unknown or mismatched plan ids must return `PCV_CONFIG_MIGRATION_PLAN_UNSUPPORTED` with `mutation_performed=false`.

## Task 3: Service Stopped And Backup Gate

- [x] **Step 1: Require service stopped proof**

Future apply must prove `PureCVisorDesktopNode` is stopped before config write. If the service is running, return `PCV_CONFIG_MIGRATION_SERVICE_RUNNING` and do not stop it implicitly.

- [x] **Step 2: Require owned backup root**

Backup path must be inside an owned config backup root under the data root, for example:

```text
C:\ProgramData\PureCVisor\desktop-node\backups\config\<migration_plan_id>\<timestamp>\
```

The action must reject backup paths outside that root with `PCV_CONFIG_MIGRATION_BACKUP_PATH_INVALID`.

## Task 4: Atomic Replace And Rollback Gate

- [x] **Step 1: Require same-volume atomic replace**

Future implementation must write the target config to a temp path in the same directory as the final config, then replace the final path atomically. Cross-volume moves are not allowed.

- [x] **Step 2: Require rollback diagnostics**

If atomic replace fails, diagnostics must include:

```json
{
  "rollback_attempted": true,
  "rollback_succeeded": true,
  "original_config_restored": true,
  "partial_config_present": false
}
```

If rollback cannot be proven, the route remains blocked and cannot become GA-scope evidence.

## Task 5: Verification Boundary

- [x] **Step 1: Code-level verification**

Future code-level verification must include no-mutation tests for every missing precondition and atomic replace/rollback unit tests using temp directories only.

- [x] **Step 2: Admin opt-in verification**

Installed config apply smoke는 별도 명시적 admin opt-in evidence다. `0.38.6-admin-smoke`는 host mutation performed, product manifest schema `1 -> 2`, backup/temp cleanup evidence, final service `Running`, boot time unchanged, public trusted signing/external stable publication 미주장을 확인했다.

## Current Verification

For the code-level implementation:

```powershell
git diff --check
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

## Completion Criteria

- Code-level actual apply path exists for supported `product-config-v1-to-v2` plan/version 1.
- Route matrix can point to this document as implementation boundary and code-level evidence.
- Installed destructive admin smoke PASS evidence는 `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`에 있고, route matrix promotion은 `current-native`다.
- Public trusted signing and external stable publication remain excluded.
