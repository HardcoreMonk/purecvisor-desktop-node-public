# Job Store Migration Apply Implementation Boundary

> **현행화(2026-05-07):** 이 문서는 최초에는 plan-only 경계였지만, 후속 구현 작업에서 code-level actual apply 경로가 구현됐고 `0.38.6-admin-smoke` installed destructive admin smoke PASS로 닫혔다. 이 closure는 internal `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

**Goal:** `job store migration apply`의 job store ownership, schema, runtime writer stopped, backup, atomic replace, rollback/recovery evidence 요구사항과 2026-05-06 code-level 구현 상태를 고정한다.

**Architecture:** `DesktopNode.Host.exe service-action job-store-migration-apply --migration-plan-id job-store-v1-to-v2 --migration-plan-version 1`은 owned service identity, stopped service/runtime writer proof, owned `jobs.json`, source schema v1, supported plan identity를 통과할 때 job store를 schema v2로 백업 후 same-directory temp replace로 교체한다. Missing precondition은 job store write 없이 blocked diagnostics를 반환한다. .NET request processor는 schema v2 migration store를 load하고, v99 같은 더 새로운 schema는 계속 blocked/no-mutation으로 처리한다.

**Tech Stack:** C#/.NET product action candidate, `jobs.json` schema v1, Windows service stopped/runtime writer stopped proof, same-directory temp write, same-volume atomic replace, Korean Markdown evidence.

**구현 상태:** 2026-05-06 code-level actual apply 구현이 완료됐고, 2026-05-07 `artifacts/config-jobstore-migration-apply-installed-20260507-0386`으로 installed destructive admin smoke가 PASS됐다. 이 action은 Local API route, 암묵적 service stop/start, MSI, Hyper-V, firewall, trust-store, LAN, Event Log, update, rollback, reboot를 실행하지 않는다.

---

## Scope

Included:

- Job store path inventory requirements.
- Current job schema owner and schema version evidence requirements.
- Runtime writer stopped proof requirements.
- Backup root, atomic replace, rollback diagnostics, and recovery evidence requirements.
- Blocked diagnostics/no-mutation behavior when any evidence is missing.

Excluded:

- Local API route exposure for job store migration apply.
- Implicit service stop/start, install/repair/remove, MSI, update/rollback, Hyper-V, firewall, trust-store, LAN, Event Log, Task Scheduler, reboot.
- 명시적 `0.38.6-admin-smoke` evidence closure 이후 범위의 추가 installed destructive admin smoke.
- Product config migration, token migration, service identity mutation, public trusted signing, external stable publication.

## Required Invariants

- 명시적 `0.38.6-admin-smoke` installed destructive admin smoke PASS 이후 `promotion_state`는 `current-native`다.
- Schema v2 migration store loads as a supported target. Newer unsupported future job store schema continues to return blocked diagnostics/no-mutation and must not quarantine or start empty as a product path.
- Runtime job enqueue/save remains the only current product job store write. That write is not migration apply evidence.
- Migration apply must be rejected if job store ownership, schema version, migration plan id, runtime writer stopped state, backup root, or rollback/recovery behavior is unclear.
- Partial job store migration is forbidden. A failed migration must leave the original job store intact or restore it from the owned backup with recovery diagnostics.
- The action is owned by `dotnet-job-store-migration-action`, not by normal queued job runtime, MSI default path, PowerShell helper, or update/rollback flow.

## 구현 파일 지도

- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` only for schema read/blocking compatibility if needed.
- Create or modify: a focused product ops owner under `src/DesktopNode.Host/**` or `src/DesktopNode.ProductOps/**` for migration apply.
- Modify: `src/DesktopNode.Api.Tests/**` or product ops tests for precondition, blocked diagnostics, atomic replace, rollback, and recovery.
- Modify: `packaging/windows-desktop-node/README.md` and `docs/DEVELOPMENT_VERIFICATION_POLICY.md` only after code-level verification exists.
- Modify: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` only after evidence changes the row state.

This file map is not authorization to edit those files now.

## Task 1: Job Store Inventory Gate

- [x] **Step 1: Record job store inventory**

Future implementation must produce a dry-run descriptor with these fields before apply can exist:

```json
{
  "operation": "job.store.migration.inventory",
  "ok": true,
  "job_store_path": "C:\\ProgramData\\PureCVisor\\desktop-node\\jobs.json",
  "owned": true,
  "current_schema_version": 1,
  "job_count": 0,
  "queue_count": 0,
  "runtime_writer": "DesktopNode.Host",
  "mutation_planned": false
}
```

- [x] **Step 2: Block if inventory is incomplete**

If path, owner, schema version, or runtime writer owner cannot be proven, future apply must return blocked diagnostics and must not create backups:

```json
{
  "ok": false,
  "operation": "job.store.migration.apply",
  "error": {
    "code": "PCV_JOB_STORE_MIGRATION_PRECONDITION_MISSING",
    "message": "Job store migration apply is blocked because ownership, schema, or runtime writer evidence is incomplete.",
    "retryable": false
  },
  "mutation_performed": false
}
```

## Task 2: Runtime Writer Stopped Gate

- [x] **Step 1: Require stopped writer proof**

Future apply must prove the installed runtime writer is stopped before any job store write. Acceptable proof is an explicit service stopped observation for `PureCVisorDesktopNode` plus no live writer process holding the configured job store path.

- [x] **Step 2: Block running writer**

If the service or writer is running, return `PCV_JOB_STORE_WRITER_RUNNING` and do not stop the service implicitly.

## Task 3: Migration Plan Identity Gate

- [x] **Step 1: Require source and target schema versions**

Future apply must require a stable descriptor:

```json
{
  "migration_plan_id": "job-store-v1-to-v2",
  "migration_plan_version": 1,
  "source_schema_version": 1,
  "target_schema_version": 2,
  "partial_migration_allowed": false,
  "destructive_rewrite_default": false
}
```

- [x] **Step 2: Block unknown plan ids**

Unknown or mismatched plan ids must return `PCV_JOB_STORE_MIGRATION_PLAN_UNSUPPORTED` with `mutation_performed=false`.

## Task 4: Backup, Atomic Replace, And Recovery Gate

- [x] **Step 1: Require owned backup root**

Backup path must be inside an owned job-store backup root under the data root, for example:

```text
C:\ProgramData\PureCVisor\desktop-node\backups\jobs\<migration_plan_id>\<timestamp>\
```

The action must reject backup paths outside that root with `PCV_JOB_STORE_BACKUP_PATH_INVALID`.

- [x] **Step 2: Require same-directory temp write**

Future implementation must write migrated JSON to a temp path in the same directory as `jobs.json`, validate JSON, then replace the final path atomically. Cross-volume moves are not allowed.

- [x] **Step 3: Require recovery diagnostics**

If replace fails, diagnostics must include:

```json
{
  "rollback_attempted": true,
  "rollback_succeeded": true,
  "original_job_store_restored": true,
  "partial_job_store_present": false,
  "recovery_required": false
}
```

If recovery cannot be proven, the route remains blocked and cannot become GA-scope evidence.

## Task 5: Verification Boundary

- [x] **Step 1: Code-level verification**

Future code-level verification must include no-mutation tests for every missing precondition, unknown migration plan id, writer running state, invalid backup root, partial migration rejection, atomic replace failure, rollback success, and recovery diagnostics.

- [x] **Step 2: Admin opt-in verification**

Installed job store apply smoke는 별도 명시적 admin opt-in evidence다. `0.38.6-admin-smoke`는 host mutation performed, job store schema `1 -> 2`, seeded job preserved/listed, backup/temp cleanup evidence, final service `Running`, boot time unchanged, post-migration job list read 유효성을 확인했다.

## Current Verification

For the code-level implementation:

```powershell
git diff --check
dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobStoreVersion2MigrationStoreLoadsWithoutBlockedDiagnostics"
```

## Completion Criteria

- Code-level actual apply path exists for supported `job-store-v1-to-v2` plan/version 1.
- Route matrix can point to this document as implementation boundary and code-level evidence.
- Installed destructive admin smoke PASS evidence는 `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`에 있고, route matrix promotion은 `current-native`다.
- Public trusted signing and external stable publication remain excluded.
