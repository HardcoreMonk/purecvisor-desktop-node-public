# PureCVisor Desktop Node Phase 18 Update/Rollback/Config Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node product wrapper의 update/rollback/config migration을 manifest-first safe update 정책으로 고정하고 검증한다.

**Architecture:** `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`가 update policy v1, manifest validation, config migration plan, safe update orchestration, rollback validation, diagnostics artifact를 소유한다. 기존 `spikes/purecvisor-desktop-node/**` 런타임은 이동하지 않고, product wrapper가 설치 루트와 데이터 루트의 버전 전환만 조율한다.

**Tech Stack:** PowerShell 7, Pester 5, WinSW product wrapper, JSON product manifest, JSONL install diagnostics, Windows filesystem operations with injectable dependencies.

---

## 설계 기준

- 설계 문서: `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration-design.md`
- 결정 토큰: `DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration`
- 유지 결정: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 경계: `packaging/windows-desktop-node/**` 중심으로 구현한다. 필요한 검증 문서만 `spikes/purecvisor-desktop-node/**`에 반영한다. Linux `purecvisorsd`, Single Edge `ui/**`, Single Edge API 공개 표면은 변경하지 않는다.

## 파일 구조

수정:

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: update policy helper, manifest validation helper, config migration plan helper, update orchestration, rollback validation, diagnostics artifact를 추가한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`: product plan의 update policy v1과 rollback/config migration 기본 정책을 검증한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`: product manifest의 update policy v1과 version source contract를 검증한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`: `Update`와 `Rollback` orchestration의 실패 지점별 behavior를 검증한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`: diagnostic bundle이 update policy, migration plan, rollback state artifact를 포함하는지 검증한다.
- `packaging/windows-desktop-node/README.md`: Phase 18 update/rollback/config migration 정책과 관리자 smoke 경계를 설명한다.
- `docs/DEVELOPER_INDEX.md`: Phase 18 spec/plan 진입점을 추가한다.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: Phase 18 검증 mapping과 기대 결과를 구현 후 갱신한다.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`: Phase 18이 Single Edge 공개 표면과 분리된 packaging-only 정책임을 명시한다.
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`: Phase 18 상태를 갱신한다.
- `spikes/purecvisor-desktop-node/README.md`: Desktop Node root 경계와 keep-spike gate를 Phase 18까지 갱신한다.
- `spikes/purecvisor-desktop-node/api/README.md`: update/rollback/config migration이 Local API 공개 표면이 아니라 product wrapper 경계임을 명시한다.
- `AGENTS.md`, `README.md`, `follower.md`: Phase 18 문서 링크와 후속 실행 순서를 갱신한다.

## 작업 원칙

- 네트워크 다운로드형 updater는 구현하지 않는다.
- 기본 rollback slot은 `DesktopNode.previous` 하나만 유지한다.
- `product-manifest.json` top-level `schema_version = 1`은 유지하고, `update.schema_version = 1` object를 추가한다.
- config migration은 service start 전에 dry-run/validation을 통과해야 한다.
- job store는 파괴적으로 rewrite하지 않는다.
- 실제 product root/data root를 변경하는 관리자 smoke는 기본 검증과 분리한다.
- diagnostics는 token 값, protected token blob/hash, Authorization header, full host path를 남기지 않는다.

---

### Task 1: Product Manifest Update Policy

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: product plan 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Plan.Tests.ps1`의 기본 install plan test에 다음 assertions를 추가한다.

```powershell
$plan.update.schema_version | Should -Be 1
$plan.update.decision | Should -Be 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
$plan.update.version_source | Should -Be 'product-wrapper-version-parameter'
$plan.update.installed_manifest_is_source_of_truth | Should -BeTrue
$plan.update.payload_version_must_match_manifest | Should -BeTrue
$plan.update.rollback.previous_root | Should -Be $plan.paths.previous_product_root
$plan.update.rollback.retained_previous_roots | Should -Be 1
$plan.update.rollback.rollback_requires_health_check | Should -BeTrue
$plan.update.config_migration.mode | Should -Be 'validate-before-service-start'
$plan.update.config_migration.dry_run_required | Should -BeTrue
$plan.update.config_migration.block_service_start_on_failure | Should -BeTrue
$plan.update.config_migration.data_backup_required_before_mutation | Should -BeTrue
$plan.update.job_store.destructive_rewrite_by_default | Should -BeFalse
$plan.update.job_store.schema_mismatch_mode | Should -Be 'read-only-or-blocked-with-diagnostics'
$plan.update.provenance.unsigned_dev_allowed_for_dev_channel | Should -BeTrue
```

- [x] **Step 2: product manifest 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Manifest.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'records update policy v1 in product-manifest.json' {
    $manifest = New-PcvDesktopNodeProductManifest `
        -SourceRoot $script:RepoRoot `
        -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
        -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
        -Version '0.18.0-dev'

    $manifest.schema_version | Should -Be 1
    $manifest.version | Should -Be '0.18.0-dev'
    $manifest.update.schema_version | Should -Be 1
    $manifest.update.decision | Should -Be 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
    $manifest.update.version_source | Should -Be 'product-wrapper-version-parameter'
    $manifest.update.rollback.retained_previous_roots | Should -Be 1
    $manifest.update.config_migration.mode | Should -Be 'validate-before-service-start'
    $manifest.update.job_store.destructive_rewrite_by_default | Should -BeFalse
    $manifest.update.provenance.signed_release_required_for_release_channel | Should -BeTrue
}
```

- [x] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Expected: `update` field가 없어서 관련 assertions가 실패한다.

- [x] **Step 4: update policy helper를 구현한다**

`PcvDesktopNodeProduct.psm1`에 다음 helper를 추가한다.

```powershell
function Get-PcvDesktopNodeUpdatePolicy {
    param(
        [Parameter(Mandatory)]
        [hashtable]$Paths
    )

    [ordered]@{
        schema_version = 1
        decision = 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
        version_source = 'product-wrapper-version-parameter'
        installed_manifest_is_source_of_truth = $true
        payload_version_must_match_manifest = $true
        rollback = [ordered]@{
            previous_root = $Paths.previous_product_root
            failed_root_suffix = '.failed'
            retained_previous_roots = 1
            rollback_requires_health_check = $true
        }
        config_migration = [ordered]@{
            mode = 'validate-before-service-start'
            dry_run_required = $true
            block_service_start_on_failure = $true
            data_backup_required_before_mutation = $true
        }
        job_store = [ordered]@{
            destructive_rewrite_by_default = $false
            schema_mismatch_mode = 'read-only-or-blocked-with-diagnostics'
        }
        provenance = [ordered]@{
            signed_release_required_for_release_channel = $true
            unsigned_dev_allowed_for_dev_channel = $true
        }
    }
}
```

`New-PcvDesktopNodeProductManifest`와 `New-PcvDesktopNodeProductPlan`의 returned object에 다음 field를 추가한다.

```powershell
update = Get-PcvDesktopNodeUpdatePolicy -Paths $paths
```

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Expected: targeted product wrapper tests가 `Failed: 0`으로 통과한다.

- [x] **Step 6: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1
git commit -m "feat: add Desktop Node update policy"
```

---

### Task 2: Manifest Validation and Migration Plan Contract

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: manifest validation 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Invoke.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'rejects rollback when previous manifest is missing or invalid' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeInvalidRollback'
    $previousRoot = "$productRoot.previous"
    New-Item -ItemType Directory -Path $previousRoot -Force | Out-Null

    $result = Invoke-PcvDesktopNodeProductAction `
        -Action Rollback `
        -ProductRoot $productRoot `
        -DataRoot (Join-Path $TestDrive 'data-invalid-rollback') `
        -WhatIf `
        -ErrorAction SilentlyContinue

    $result.ok | Should -BeFalse
    $result.error.code | Should -Be 'PCV_PRODUCT_PREVIOUS_MANIFEST_INVALID'
}
```

- [x] **Step 2: migration plan dry-run 실패 테스트를 추가한다**

같은 파일에 다음 behavior를 추가한다.

```powershell
It 'blocks service start when config migration validation fails during Update' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeMigrationFailure'
    $dataRoot = Join-Path $TestDrive 'data-migration-failure'
    New-Item -ItemType Directory -Path $productRoot,$dataRoot -Force | Out-Null
    [ordered]@{
        schema_version = 1
        product = 'PureCVisor Desktop Node'
        version = '0.17.0-dev'
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8

    $dependencies = New-PcvDesktopNodeProductActionDependencies
    $dependencies.StartService = { throw 'service start must be blocked before migration succeeds' }
    $dependencies.NewConfigMigrationPlan = {
        [ordered]@{
            schema_version = 1
            from_version = '0.17.0-dev'
            to_version = '0.18.0-dev'
            dry_run = $true
            service_start_allowed = $false
            steps = @([ordered]@{ name = 'validate-job-store-schema'; mutation = $false; required = $true; status = 'failed' })
            error = [ordered]@{ code = 'PCV_PRODUCT_CONFIG_MIGRATION_BLOCKED'; message = 'Config migration validation failed.' }
        }
    }

    $result = Invoke-PcvDesktopNodeProductAction `
        -Action Update `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -Version '0.18.0-dev' `
        -Dependencies $dependencies `
        -WhatIf

    $result.ok | Should -BeFalse
    $result.error.code | Should -Be 'PCV_PRODUCT_CONFIG_MIGRATION_BLOCKED'
    @($result.executed.step) | Should -Not -Contain 'start-service'
}
```

- [x] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected: previous manifest validation 또는 migration dependency hook이 없어서 새 tests가 실패한다.

- [x] **Step 4: manifest validation helper를 구현한다**

`PcvDesktopNodeProduct.psm1`에 다음 helper를 추가한다.

```powershell
function Read-PcvDesktopNodeProductManifest {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw (New-PcvDesktopNodeProductError `
            -Code 'PCV_PRODUCT_MANIFEST_MISSING' `
            -Message 'Product manifest is missing.' `
            -Detail $Path)
    }

    try {
        $manifest = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw (New-PcvDesktopNodeProductError `
            -Code 'PCV_PRODUCT_MANIFEST_INVALID_JSON' `
            -Message 'Product manifest is not valid JSON.' `
            -Detail $Path)
    }

    if ($manifest.schema_version -ne 1 -or $manifest.product -ne 'PureCVisor Desktop Node' -or [string]::IsNullOrWhiteSpace([string]$manifest.version)) {
        throw (New-PcvDesktopNodeProductError `
            -Code 'PCV_PRODUCT_MANIFEST_INVALID' `
            -Message 'Product manifest schema, product, or version is invalid.' `
            -Detail $Path)
    }

    $manifest
}
```

Rollback의 previous manifest validation에서는 위 helper의 예외를 `PCV_PRODUCT_PREVIOUS_MANIFEST_INVALID`로 감싼다.

- [x] **Step 5: config migration plan helper를 구현한다**

`PcvDesktopNodeProduct.psm1`에 다음 helper를 추가한다.

```powershell
function New-PcvDesktopNodeConfigMigrationPlan {
    param(
        [Parameter(Mandatory)]
        [string]$FromVersion,
        [Parameter(Mandatory)]
        [string]$ToVersion,
        [Parameter(Mandatory)]
        [hashtable]$Paths,
        [switch]$DryRun
    )

    [ordered]@{
        schema_version = 1
        from_version = $FromVersion
        to_version = $ToVersion
        dry_run = [bool]$DryRun
        service_start_allowed = $true
        steps = @(
            [ordered]@{
                name = 'validate-protected-token-source'
                mutation = $false
                required = $true
                status = 'planned'
            },
            [ordered]@{
                name = 'validate-job-store-compatibility'
                mutation = $false
                required = $true
                status = 'planned'
            }
        )
        backups = @(
            [ordered]@{
                source = 'jobs.json'
                artifact = ('jobs.json.pre-{0}.bak' -f $ToVersion)
                required_before_mutation = $true
            }
        )
    }
}
```

`New-PcvDesktopNodeProductActionDependencies`에 injectable `NewConfigMigrationPlan` delegate를 추가한다.

- [x] **Step 6: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected: 새 manifest validation/config migration focused tests가 `Failed: 0`으로 통과한다.

- [x] **Step 7: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "feat: validate Desktop Node update manifests"
```

---

### Task 3: Safe Update Orchestration

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: update orchestration order 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Invoke.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'orchestrates Update with manifest validation, migration dry-run, service start, and health check' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeUpdate'
    $dataRoot = Join-Path $TestDrive 'data-update'
    New-Item -ItemType Directory -Path $productRoot,$dataRoot -Force | Out-Null
    [ordered]@{
        schema_version = 1
        product = 'PureCVisor Desktop Node'
        version = '0.17.0-dev'
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8

    $calls = [System.Collections.Generic.List[string]]::new()
    $dependencies = New-PcvDesktopNodeProductActionDependencies
    $dependencies.StopService = { $calls.Add('stop-service'); [ordered]@{ exit_code = 0 } }
    $dependencies.BackupProductRoot = { $calls.Add('backup-product-root'); [ordered]@{ backed_up = $true } }
    $dependencies.CopyProductAssets = { $calls.Add('copy-assets'); [ordered]@{ copied = $true } }
    $dependencies.NewConfigMigrationPlan = { $calls.Add('migration-plan'); [ordered]@{ service_start_allowed = $true; dry_run = $true; steps = @(); backups = @() } }
    $dependencies.StartService = { $calls.Add('start-service'); [ordered]@{ exit_code = 0 } }
    $dependencies.TestProductHealth = { $calls.Add('health-check'); [ordered]@{ ok = $true } }

    $result = Invoke-PcvDesktopNodeProductAction `
        -Action Update `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -Version '0.18.0-dev' `
        -Dependencies $dependencies `
        -WhatIf

    $result.ok | Should -BeTrue
    $calls -join ',' | Should -Be 'stop-service,backup-product-root,copy-assets,migration-plan,start-service,health-check'
    $result.update.from_version | Should -Be '0.17.0-dev'
    $result.update.to_version | Should -Be '0.18.0-dev'
}
```

- [x] **Step 2: service health failure rollback 테스트를 추가한다**

같은 파일에 다음 behavior를 추가한다.

```powershell
It 'restores previous product root when Update health check fails' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateHealthFail'
    $dataRoot = Join-Path $TestDrive 'data-update-health-fail'
    New-Item -ItemType Directory -Path $productRoot,$dataRoot -Force | Out-Null
    [ordered]@{
        schema_version = 1
        product = 'PureCVisor Desktop Node'
        version = '0.17.0-dev'
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8

    $calls = [System.Collections.Generic.List[string]]::new()
    $dependencies = New-PcvDesktopNodeProductActionDependencies
    $dependencies.StopService = { $calls.Add('stop-service'); [ordered]@{ exit_code = 0 } }
    $dependencies.BackupProductRoot = { $calls.Add('backup-product-root'); [ordered]@{ backed_up = $true } }
    $dependencies.CopyProductAssets = { $calls.Add('copy-assets'); [ordered]@{ copied = $true } }
    $dependencies.NewConfigMigrationPlan = { $calls.Add('migration-plan'); [ordered]@{ service_start_allowed = $true; dry_run = $true; steps = @(); backups = @() } }
    $dependencies.StartService = { $calls.Add('start-service'); [ordered]@{ exit_code = 0 } }
    $dependencies.TestProductHealth = { $calls.Add('health-check'); [ordered]@{ ok = $false; status_code = 503 } }
    $dependencies.RestorePreviousProductRoot = { $calls.Add('restore-previous-root'); [ordered]@{ restored = $true } }

    $result = Invoke-PcvDesktopNodeProductAction `
        -Action Update `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -Version '0.18.0-dev' `
        -Dependencies $dependencies `
        -WhatIf

    $result.ok | Should -BeFalse
    $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_HEALTH_CHECK_FAILED'
    @($calls) | Should -Contain 'restore-previous-root'
    $result.update.rollback_attempted | Should -BeTrue
}
```

- [x] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected: `Update`가 current behavior보다 엄격한 update flow와 rollback result를 반환하지 않아 실패한다.

- [x] **Step 4: safe update orchestration을 구현한다**

`Invoke-PcvDesktopNodeProductAction`의 `Update` branch가 다음 ordered steps를 실행하도록 구현한다.

```powershell
$currentManifest = Read-PcvDesktopNodeProductManifest -Path $Plan.paths.manifest_path
$fromVersion = [string]$currentManifest.version
$toVersion = [string]$Plan.version
$executed += [ordered]@{ step = 'current-manifest'; result = [ordered]@{ version = $fromVersion } }

$stopResult = & $Dependencies.StopService -Plan $Plan
$executed += [ordered]@{ step = 'stop-service'; result = $stopResult }
if (($stopResult.exit_code -as [int]) -ne 0) {
    throw (New-PcvDesktopNodeProductError -Code 'PCV_PRODUCT_UPDATE_STOP_FAILED' -Message 'Update cannot continue after service stop failed.')
}

$backupResult = & $Dependencies.BackupProductRoot -Plan $Plan
$executed += [ordered]@{ step = 'backup-product-root'; result = $backupResult }
if (-not $backupResult.backed_up) {
    throw (New-PcvDesktopNodeProductError -Code 'PCV_PRODUCT_UPDATE_BACKUP_FAILED' -Message 'Update cannot continue without previous product root backup.')
}

$copyResult = & $Dependencies.CopyProductAssets -Plan $Plan
$executed += [ordered]@{ step = 'copy-assets'; result = $copyResult }

$migrationPlan = & $Dependencies.NewConfigMigrationPlan -FromVersion $fromVersion -ToVersion $toVersion -Paths $Plan.paths -DryRun
$executed += [ordered]@{ step = 'migration-plan'; result = $migrationPlan }
if ($migrationPlan.service_start_allowed -ne $true) {
    throw (New-PcvDesktopNodeProductError -Code 'PCV_PRODUCT_CONFIG_MIGRATION_BLOCKED' -Message 'Config migration validation blocked service start.')
}

$startResult = & $Dependencies.StartService -Plan $Plan
$executed += [ordered]@{ step = 'start-service'; result = $startResult }
$healthResult = & $Dependencies.TestProductHealth -Plan $Plan
$executed += [ordered]@{ step = 'health-check'; result = $healthResult }
if ($healthResult.ok -ne $true) {
    $restoreResult = & $Dependencies.RestorePreviousProductRoot -Plan $Plan
    $executed += [ordered]@{ step = 'restore-previous-root'; result = $restoreResult }
    throw (New-PcvDesktopNodeProductError -Code 'PCV_PRODUCT_UPDATE_HEALTH_CHECK_FAILED' -Message 'Update health check failed and rollback was attempted.')
}
```

Use existing dependency names where the module already has equivalent helpers. If names differ, keep the observable `executed.step` values exactly as the tests assert.

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected: update orchestration tests가 `Failed: 0`으로 통과한다.

- [x] **Step 6: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "feat: orchestrate Desktop Node safe update"
```

---

### Task 4: Rollback Validation Hardening

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: rollback previous manifest validation test를 추가한다**

`PcvDesktopNodeProduct.Invoke.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'validates previous product manifest before restoring Rollback target' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeRollbackManifest'
    $previousRoot = "$productRoot.previous"
    New-Item -ItemType Directory -Path $productRoot,$previousRoot -Force | Out-Null
    [ordered]@{
        schema_version = 1
        product = 'PureCVisor Desktop Node'
        version = '0.17.0-dev'
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $previousRoot 'product-manifest.json') -Encoding UTF8

    $result = Invoke-PcvDesktopNodeProductAction `
        -Action Rollback `
        -ProductRoot $productRoot `
        -DataRoot (Join-Path $TestDrive 'data-rollback-manifest') `
        -WhatIf

    $result.action | Should -Be 'Rollback'
    $result.rollback.previous_version | Should -Be '0.17.0-dev'
    $result.ok | Should -BeTrue
}
```

- [x] **Step 2: failed root preservation test를 추가한다**

같은 파일에 다음 assertion을 기존 rollback restore test에 추가한다.

```powershell
$restoreStep = @($result.executed | Where-Object { $_.step -eq 'restore-product-root' })[0]
$restoreStep.result.failed_root | Should -Match '\.failed$'
$restoreStep.result.failed_root_preserved_for_diagnostics | Should -BeTrue
```

- [x] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected: previous manifest version and failed root preservation metadata가 없어서 실패한다.

- [x] **Step 4: rollback validation과 result metadata를 구현한다**

Rollback branch에서 previous manifest를 restore 전 읽고, 결과 object에 version을 포함한다.

```powershell
$previousManifestPath = Join-Path $Plan.paths.previous_product_root 'product-manifest.json'
try {
    $previousManifest = Read-PcvDesktopNodeProductManifest -Path $previousManifestPath
}
catch {
    throw (New-PcvDesktopNodeProductError `
        -Code 'PCV_PRODUCT_PREVIOUS_MANIFEST_INVALID' `
        -Message 'Rollback previous product manifest is missing or invalid.' `
        -Detail $previousManifestPath)
}

$rollbackState = [ordered]@{
    previous_version = [string]$previousManifest.version
    failed_root = ($Plan.paths.product_root + '.failed')
    failed_root_preserved_for_diagnostics = $true
}
```

`restore-product-root` result에도 `failed_root`와 `failed_root_preserved_for_diagnostics`를 포함한다.

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected: rollback hardening tests가 `Failed: 0`으로 통과한다.

- [x] **Step 6: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "feat: harden Desktop Node rollback validation"
```

---

### Task 5: Update Diagnostics Artifacts

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: diagnostic bundle 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Diagnostics.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'includes update policy and migration artifacts in diagnostic bundle manifest' {
    $outRoot = Join-Path $TestDrive 'diagnostics-update'
    $productRoot = Join-Path $TestDrive 'product-update-diag'
    $dataRoot = Join-Path $TestDrive 'data-update-diag'
    New-Item -ItemType Directory -Path $outRoot,$productRoot,$dataRoot -Force | Out-Null

    $plan = New-PcvDesktopNodeProductPlan `
        -Action CollectDiagnostics `
        -SourceRoot $script:RepoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -Version '0.18.0-dev'

    $bundle = New-PcvDesktopNodeDiagnosticBundle -Plan $plan -OutputRoot $outRoot
    $manifest = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-manifest.json') -Raw | ConvertFrom-Json

    $manifest.policy.update.decision | Should -Be 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
    @($manifest.sources.name) | Should -Contain 'update_policy'
    @($manifest.sources.name) | Should -Contain 'migration_plan'
    @($manifest.sources.name) | Should -Contain 'rollback_state'

    Test-Path -LiteralPath (Join-Path $bundle.path 'update-policy-redacted.json') | Should -BeTrue
    Test-Path -LiteralPath (Join-Path $bundle.path 'migration-plan-redacted.json') | Should -BeTrue
    Test-Path -LiteralPath (Join-Path $bundle.path 'rollback-state-redacted.json') | Should -BeTrue
}
```

- [x] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: update diagnostic sources/artifacts가 없어서 실패한다.

- [x] **Step 3: diagnostic source list를 구현한다**

`New-PcvDesktopNodeDiagnosticBundleSourceList`에 다음 sources를 추가한다.

```powershell
[ordered]@{ name = 'update_policy'; artifact = 'update-policy-redacted.json'; required = $true; redacted = $true }
[ordered]@{ name = 'migration_plan'; artifact = 'migration-plan-redacted.json'; required = $true; redacted = $true }
[ordered]@{ name = 'rollback_state'; artifact = 'rollback-state-redacted.json'; required = $true; redacted = $true }
```

Diagnostic manifest `policy` object에 기존 diagnostics/network policy와 함께 update policy를 추가한다.

```powershell
$policy['update'] = Get-PcvDesktopNodeUpdatePolicy -Paths $Plan.paths
```

- [x] **Step 4: diagnostic artifacts를 구현한다**

`New-PcvDesktopNodeDiagnosticBundle`에서 다음 files를 쓴다.

```powershell
Write-PcvDesktopNodeDiagnosticJson `
    -InputObject (Get-PcvDesktopNodeUpdatePolicy -Paths $Plan.paths) `
    -Path (Join-Path $bundlePath 'update-policy-redacted.json') `
    -Plan $Plan

Write-PcvDesktopNodeDiagnosticJson `
    -InputObject (New-PcvDesktopNodeConfigMigrationPlan -FromVersion $Plan.version -ToVersion $Plan.version -Paths $Plan.paths -DryRun) `
    -Path (Join-Path $bundlePath 'migration-plan-redacted.json') `
    -Plan $Plan

Write-PcvDesktopNodeDiagnosticJson `
    -InputObject ([ordered]@{
        schema_version = 1
        previous_root_exists = Test-Path -LiteralPath $Plan.paths.previous_product_root -PathType Container
        failed_root_preserved_for_diagnostics = $true
    }) `
    -Path (Join-Path $bundlePath 'rollback-state-redacted.json') `
    -Plan $Plan
```

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: diagnostics suite가 `Failed: 0`으로 통과한다.

- [x] **Step 6: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1
git commit -m "feat: include Desktop Node update diagnostics"
```

---

### Task 6: Documentation Synchronization

**Files:**

- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `follower.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`

- [x] **Step 1: active docs를 Phase 18 상태로 갱신한다**

다음 문구를 기준으로 문서 상태를 통일한다.

```text
Desktop Node Phase 18 update/rollback/config migration은 installed product manifest를 버전의 단일 진실로 두고, local payload 기반 safe update, 단일 previous root rollback slot, service-start-before migration validation, job store non-destructive compatibility를 제품 wrapper 경계에 고정한다.
```

High-level 문서에는 suite pass count를 복제하지 않는다. Pass count는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`와 이 plan의 `완료 증거`에만 기록한다.

- [x] **Step 2: 문서 동기화 guard를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: root boundary/documentation sync suite가 `Failed: 0`으로 통과한다.

- [x] **Step 3: Commit**

```powershell
git add AGENTS.md README.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md follower.md packaging/windows-desktop-node/README.md spikes/purecvisor-desktop-node/README.md spikes/purecvisor-desktop-node/api/README.md
git commit -m "docs: document Desktop Node phase 18 update policy"
```

---

### Task 7: Full Verification and PR Handoff

**Files:**

- Modify: `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration.md`

- [x] **Step 1: full Phase 18 verification을 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Expected: 모든 명령이 exit 0이고 Pester output에 `Failed: 0`이 표시된다.

- [x] **Step 2: product dry-run smoke를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

Expected:

- `Plan` output에서 `update.schema_version = 1`과 Phase 18 decision marker를 확인한다.
- `Update -WhatIf`, `Rollback -WhatIf`, `CollectDiagnostics -WhatIf` output에서 `ok = true`, `dry_run = true`, `execution_skipped = true`를 확인한다.
- `-WhatIf` entrypoint는 product root/data root를 건드리지 않는 plan-level dry-run envelope만 반환한다. 상세 update orchestration, rollback manifest validation, diagnostics artifact 계약은 Pester suite에서 검증한다.

- [x] **Step 3: 완료 증거를 기록한다**

이 plan의 `완료 증거` 섹션에 실행 명령, 결과, 관리자 smoke 보류 사유를 기록한다. 실제 mutating update/rollback smoke를 실행하지 않았다면 그 이유를 명시한다.

- [x] **Step 4: 최종 커밋을 만든다**

```powershell
git add docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration.md
git commit -m "docs: record Desktop Node phase 18 verification"
```

- [x] **Step 5: PR을 준비한다**

```powershell
git status --short --branch
git push -u origin codex/desktop-node-phase18-update-rollback
```

PR base는 Phase 17 branch인 `codex/desktop-node-phase17-lan-security`로 둔다. Phase 14-17 canonical line 정리가 끝나기 전에는 draft PR로 유지한다.

## 완료 증거

Phase 18 구현은 `codex/desktop-node-phase18-update-rollback` 브랜치에서 완료했다.

구현 범위:

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`에 update policy v1, product manifest validation, config migration plan, safe update orchestration, rollback validation metadata, update diagnostics artifact를 추가했다.
- `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1` action surface가 `Update`와 version parameter를 전달하도록 정리했다.
- `packaging/windows-desktop-node/tests/`에 Phase 18 product plan/manifest/invoke/diagnostics Pester coverage를 추가했다.
- active 문서와 Desktop Node 경계 문서를 Phase 18 상태로 동기화했다.

계획 적용 중 조정:

- 계획 초안에는 `New-PcvDesktopNodeProductActionDependencies` helper 추가가 적혀 있었지만, 현재 module은 별도 dependency factory가 아니라 `Invoke-PcvDesktopNodeProductAction`의 scriptblock injection parameter를 직접 받는 구조였다. 구현은 기존 구조를 유지하고 `-BackupProductRoot`, `-RestorePreviousProductRoot`, `-CopyProductAssets`, `-NewConfigMigrationPlan`, `-StartService`, `-StopService`, `-TestProductHealth` injection surface로 테스트 가능성을 확보했다.
- product entrypoint `-WhatIf`는 실제 update/rollback 절차를 부분 실행하지 않고 `dry_run/execution_skipped` envelope를 반환한다. service-start-before migration validation, failed root preservation, diagnostics artifacts는 unit/orchestration tests에서 검증했다.

검증 결과:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
# Tests Passed: 69, Failed: 0

pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
# Tests Passed: 92, Failed: 0

pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
# Tests Passed: 19, Failed: 0

pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
# Tests Passed: 10, Failed: 0

node --check spikes/purecvisor-desktop-node/web/app.js
# exit 0

git diff --check
# exit 0
```

Product dry-run smoke:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
# exit 0, update.schema_version = 1, decision = DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -WhatIf
# exit 0, ok = true, dry_run = true, execution_skipped = true

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -WhatIf
# exit 0, ok = true, dry_run = true, execution_skipped = true

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
# exit 0, ok = true, dry_run = true, execution_skipped = true
```

관리자 mutating update/rollback smoke:

```powershell
# 관리자 PowerShell에서 실행.
# 기존 설치: C:\Program Files\PureCVisor\DesktopNode, C:\ProgramData\PureCVisor\desktop-node
# WinSW source는 update 중 product root 이동의 영향을 피하기 위해 TEMP에 복사한 staged WinSW를 사용했다.

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
# exit 0, 기존 manifest version = 0.14.0-dev, runtime policy HTTP 200, Web Console root HTTP 200

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -Version 0.18.0-admin-smoke -WinSwPath <TEMP\PureCVisorDesktopNode.exe>
# exit 0
# from_version = 0.14.0-dev
# to_version = 0.18.0-admin-smoke
# rollback_attempted = false
# executed steps = current-manifest, service.stop, service.stop.wait, backup-product-root, copy, config-migration, service.start, health
# post-update manifest version = 0.18.0-admin-smoke
# runtime policy HTTP 200, Web Console root HTTP 200

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback
# exit 0
# previous_version = 0.14.0-dev
# failed_root_preserved_for_diagnostics = true
# failed root exists = true
# executed steps = service.stop, service.stop.wait, restore, service.start, health
# post-rollback manifest version = 0.14.0-dev
# runtime policy HTTP 200, Web Console root HTTP 200

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
# exit 0
# bundle path = C:\ProgramData\PureCVisor\desktop-node\diagnostics\bundle-20260428-125501-32bd4a7b
# diagnostics manifest includes update_policy, migration_plan, rollback_state
```

관리자 smoke 후 의도된 상태:

- `C:\Program Files\PureCVisor\DesktopNode`는 rollback으로 기존 `0.14.0-dev` manifest 상태로 복원됐다.
- `C:\Program Files\PureCVisor\DesktopNode.failed`는 실패/교체 root diagnostic 보존 정책 확인을 위해 남아 있다.
- `PureCVisorDesktopNode` Windows service는 rollback 후 다시 started/healthy 상태다.
- Protected token 값은 출력하거나 문서에 기록하지 않았다.

2026-05-01 current-head update/rollback/config migration 재확인:

- evidence root: `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`
- baseline install: `Invoke-PcvDesktopNodeProduct.ps1 -Action Install -Version 0.23.9-admin-smoke-baseline -WinSwPath <external WinSW>` exit `0`
- update/config migration: `-Action Update -Version 0.23.9-admin-smoke-update -WinSwPath <external WinSW>` exit `0`
- migration result: dry-run validation allowed service start, rollback attempted false
- rollback: `-Action Rollback` exit `0`, runtime healthy
- diagnostics: `-Action CollectDiagnostics` exit `0`
- cleanup: product-wrapper uninstall and `.failed` product root cleanup passed
- final state: `0.23.9-rc.1` MSI restore install completed, `PureCVisorDesktopNode` service Running with MSI root WinSW path
- secret handling: protected token 값, PFX password, private key material은 evidence에 기록하지 않았다.
