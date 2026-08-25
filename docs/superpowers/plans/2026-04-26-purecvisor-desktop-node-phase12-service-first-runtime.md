# PureCVisor Desktop Node Phase 12 Service-first Runtime Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Windows Desktop Node Local API와 Web Console을 Service-first 제품 런타임으로 설치, 업데이트, 롤백, 제거, 진단 수집할 수 있는 product wrapper를 만든다.

**Architecture:** 기존 `spikes/purecvisor-desktop-node/{api,web,hyperv,service}` 구현은 즉시 이동하지 않고 `packaging/windows-desktop-node/` wrapper가 제품 설치 루트로 필요한 자산을 복사한다. wrapper는 PowerShell module과 thin entrypoint로 구성하고, 기본 검증은 관리자 권한 없이 Pester로 plan, manifest, command builder, redaction을 검증한다.

**Tech Stack:** PowerShell 7, Pester 5, Windows `sc.exe` command contract, JSON manifest, static Web Console assets.

---

## 파일 구조

- Create: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
  - 제품 경로 기본값, action plan, asset manifest, asset copy, install/update/rollback/uninstall orchestration, diagnostic redaction을 제공한다.
- Create: `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
  - CLI entrypoint. `Plan`, `Install`, `Update`, `Rollback`, `Uninstall`, `Status`, `CollectDiagnostics` action을 JSON으로 실행한다.
- Create: `packaging/windows-desktop-node/README.md`
  - Service-first 제품 wrapper 사용법, dry-run, 관리자 smoke, 데이터 보존 정책을 문서화한다.
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
  - 기본 경로, install/update/uninstall/rollback plan, service command, token file 계약을 검증한다.
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
  - asset manifest와 product manifest, 제품 루트 복사 계약을 검증한다.
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
  - entrypoint JSON 출력, dry-run action, mutating action guard를 검증한다.
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
  - diagnostic bundle과 token/Authorization redaction을 검증한다.
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
  - Phase 12 service-first runtime 결정과 plan/spec drift를 검증한다.
- Modify: `README.md`
  - Desktop Node 제품 wrapper 진입점과 검증 명령을 추가한다.
- Modify: `AGENTS.md`
  - Phase 12 문서 진입점, wrapper 경계, 검증 명령을 추가한다.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Phase 11 `keep-spike`와 Phase 12 service-first 승격 시작 상태가 충돌하지 않도록 갱신한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - packaging wrapper suite와 관리자 opt-in smoke 기준을 추가한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Phase 12 설계/계획과 packaging README 진입점을 추가한다.
- Modify: `spikes/purecvisor-desktop-node/README.md`
  - root boundary에서 Phase 12 wrapper를 제품 후보 배포 단위로 참조한다.
- Modify: `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
  - Phase 11 결정이 Phase 12 service-first runtime 시작으로 부분 해소되었음을 기록한다.
- Modify: `follower.md`
  - Phase 12 진행 상태와 검증 값을 최신화한다.

## Task 1: Product wrapper plan contract red test

**Files:**
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`

- [ ] **Step 1: Create the failing plan contract test**

Create `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1` with this content:

```powershell
Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct plan contract' {
    BeforeAll {
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'declares stable service-first product defaults' {
        $defaults = Get-PcvDesktopNodeProductDefaults

        $defaults.service_name | Should -Be 'PureCVisorDesktopNode'
        $defaults.display_name | Should -Be 'PureCVisor Desktop Node'
        $defaults.prefix | Should -Be 'http://127.0.0.1:7777/'
        $defaults.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        $defaults.data_root | Should -Be (Join-Path $env:ProgramData 'PureCVisor\desktop-node')
        $defaults.token_file | Should -Be (Join-Path $defaults.data_root 'api-token.txt')
        $defaults.job_store | Should -Be (Join-Path $defaults.data_root 'jobs.json')
        $defaults.event_log | Should -Be (Join-Path $defaults.data_root 'events.jsonl')
        $defaults.install_log | Should -Be (Join-Path $defaults.data_root 'install.jsonl')
        $defaults.diagnostics_root | Should -Be (Join-Path $defaults.data_root 'diagnostics')
    }

    It 'builds an install plan that serves the Web Console from the product root' {
        $productRoot = 'C:\Program Files\PureCVisor\DesktopNode'
        $dataRoot = 'C:\ProgramData\PureCVisor\desktop-node'

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot

        $plan.action | Should -Be 'Install'
        $plan.requires_elevation | Should -BeTrue
        $plan.product_root | Should -Be $productRoot
        $plan.data_root | Should -Be $dataRoot
        $plan.auth.api_token_source | Should -Be 'file'
        $plan.auth.api_token_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\api-token.txt'
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('C:\Program Files\PureCVisor\DesktopNode\api\Invoke-PcvDesktopApi.ps1'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('-WebRootPath "C:\Program Files\PureCVisor\DesktopNode\web"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('-JobStorePath "C:\ProgramData\PureCVisor\desktop-node\jobs.json"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('-EventLogPath "C:\ProgramData\PureCVisor\desktop-node\events.jsonl"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('-ApiTokenFile "C:\ProgramData\PureCVisor\desktop-node\api-token.txt"'))
        $plan.service.config.binary_path | Should -Not -Match '-ApiToken "'
        @($plan.assets.name) | Should -Be @('api', 'web', 'hyperv', 'service')
        @($plan.service.commands.arguments -join ' ') | Should -Match 'failure PureCVisorDesktopNode'
    }

    It 'builds a remove-data uninstall plan with explicit destructive paths' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -RemoveData

        $plan.action | Should -Be 'Uninstall'
        $plan.remove_data | Should -BeTrue
        $plan.delete_paths | Should -Contain 'C:\Program Files\PureCVisor\DesktopNode'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\api-token.txt'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\jobs.json'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\events.jsonl'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\install.jsonl'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\diagnostics'
    }

    It 'rejects inline API tokens for product service plans' {
        { New-PcvDesktopNodeProductPlan `
                -Action Install `
                -SourceRoot $script:RepoRoot `
                -ApiToken 'inline-secret' } |
            Should -Throw -ExpectedMessage '*PCV_PRODUCT_INLINE_TOKEN_FORBIDDEN*'
    }
}
```

- [ ] **Step 2: Run the red test**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` does not exist yet.

## Task 2: Product wrapper plan module

**Files:**
- Create: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Test: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`

- [ ] **Step 1: Create the initial product wrapper module**

Create `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` with these functions:

```powershell
Set-StrictMode -Version Latest

function Join-PcvProductPath {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][string]$Child
    )
    Join-Path $Root $Child
}

function Get-PcvDesktopNodeProductDefaults {
    $dataRoot = Join-Path $env:ProgramData 'PureCVisor\desktop-node'

    [ordered]@{
        service_name = 'PureCVisorDesktopNode'
        display_name = 'PureCVisor Desktop Node'
        prefix = 'http://127.0.0.1:7777/'
        product_root = 'C:\Program Files\PureCVisor\DesktopNode'
        data_root = $dataRoot
        token_file = Join-Path $dataRoot 'api-token.txt'
        job_store = Join-Path $dataRoot 'jobs.json'
        event_log = Join-Path $dataRoot 'events.jsonl'
        install_log = Join-Path $dataRoot 'install.jsonl'
        diagnostics_root = Join-Path $dataRoot 'diagnostics'
        service_account = 'LocalSystem'
        worker_count = 1
        timeout_sec = 30
        schema_version = 1
    }
}

function Resolve-PcvDesktopNodeProductPaths {
    param(
        [AllowNull()][string]$ProductRoot,
        [AllowNull()][string]$DataRoot
    )

    $defaults = Get-PcvDesktopNodeProductDefaults
    $resolvedProductRoot = if ([string]::IsNullOrWhiteSpace($ProductRoot)) { $defaults.product_root } else { $ProductRoot }
    $resolvedDataRoot = if ([string]::IsNullOrWhiteSpace($DataRoot)) { $defaults.data_root } else { $DataRoot }

    [ordered]@{
        product_root = $resolvedProductRoot
        data_root = $resolvedDataRoot
        api_script = Join-Path (Join-Path $resolvedProductRoot 'api') 'Invoke-PcvDesktopApi.ps1'
        web_root = Join-Path $resolvedProductRoot 'web'
        helper_script = Join-Path (Join-Path $resolvedProductRoot 'hyperv') 'Invoke-PcvHyperV.ps1'
        service_module = Join-Path (Join-Path $resolvedProductRoot 'service') 'PcvDesktopService.psm1'
        token_file = Join-Path $resolvedDataRoot 'api-token.txt'
        job_store = Join-Path $resolvedDataRoot 'jobs.json'
        event_log = Join-Path $resolvedDataRoot 'events.jsonl'
        install_log = Join-Path $resolvedDataRoot 'install.jsonl'
        diagnostics_root = Join-Path $resolvedDataRoot 'diagnostics'
        manifest_path = Join-Path $resolvedProductRoot 'product-manifest.json'
    }
}

function Get-PcvDesktopNodeProductAssets {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)][string]$ProductRoot
    )

    $desktopRoot = Join-Path $SourceRoot 'spikes/purecvisor-desktop-node'
    @(
        [ordered]@{ name = 'api'; source = Join-Path $desktopRoot 'api'; destination = Join-Path $ProductRoot 'api' },
        [ordered]@{ name = 'web'; source = Join-Path $desktopRoot 'web'; destination = Join-Path $ProductRoot 'web' },
        [ordered]@{ name = 'hyperv'; source = Join-Path $desktopRoot 'hyperv'; destination = Join-Path $ProductRoot 'hyperv' },
        [ordered]@{ name = 'service'; source = Join-Path $desktopRoot 'service'; destination = Join-Path $ProductRoot 'service' }
    )
}

function Import-PcvDesktopServiceSupport {
    param([Parameter(Mandatory)][string]$SourceRoot)

    $serviceModule = Join-Path $SourceRoot 'spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1'
    Import-Module $serviceModule -Force
}

function New-PcvDesktopNodeServicePlan {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)]$Paths,
        [Parameter(Mandatory)][string]$ServiceName,
        [Parameter(Mandatory)][string]$DisplayName,
        [Parameter(Mandatory)][string]$Prefix,
        [Parameter(Mandatory)][string]$ServiceAccount,
        [Parameter(Mandatory)][int]$WorkerCount,
        [Parameter(Mandatory)][int]$TimeoutSec
    )

    Import-PcvDesktopServiceSupport -SourceRoot $SourceRoot

    $config = New-PcvDesktopServiceConfig `
        -ServiceName $ServiceName `
        -DisplayName $DisplayName `
        -Description 'PureCVisor Desktop Node service-first product runtime.' `
        -ApiScriptPath $Paths.api_script `
        -HelperScriptPath $Paths.helper_script `
        -Prefix $Prefix `
        -JobStorePath $Paths.job_store `
        -WebRootPath $Paths.web_root `
        -ApiTokenFile $Paths.token_file `
        -EventLogPath $Paths.event_log `
        -WorkerCount $WorkerCount `
        -TimeoutSec $TimeoutSec `
        -ServiceAccount $ServiceAccount

    [ordered]@{
        config = $config
        commands = @(New-PcvDesktopServiceCommand -Config $config -Action Install)
        start_commands = @(New-PcvDesktopServiceCommand -Config $config -Action Start)
        stop_commands = @(New-PcvDesktopServiceCommand -Config $config -Action Stop)
        uninstall_commands = @(New-PcvDesktopServiceCommand -Config $config -Action Uninstall)
        status_commands = @(New-PcvDesktopServiceCommand -Config $config -Action Status)
    }
}

function New-PcvDesktopNodeProductPlan {
    param(
        [ValidateSet('Plan', 'Install', 'Update', 'Rollback', 'Uninstall', 'Status', 'CollectDiagnostics')][string]$Action = 'Plan',
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
        [string]$ProductRoot,
        [string]$DataRoot,
        [string]$ServiceName,
        [string]$DisplayName,
        [string]$Prefix,
        [string]$ServiceAccount,
        [int]$WorkerCount = 0,
        [int]$TimeoutSec = 0,
        [string]$ApiToken,
        [switch]$RemoveData
    )

    if (-not [string]::IsNullOrWhiteSpace($ApiToken)) {
        throw 'PCV_PRODUCT_INLINE_TOKEN_FORBIDDEN|Product service installs must not expose inline bearer tokens.|Use the generated ApiTokenFile path.'
    }

    $defaults = Get-PcvDesktopNodeProductDefaults
    $paths = Resolve-PcvDesktopNodeProductPaths -ProductRoot $ProductRoot -DataRoot $DataRoot
    $resolvedServiceName = if ([string]::IsNullOrWhiteSpace($ServiceName)) { $defaults.service_name } else { $ServiceName }
    $resolvedDisplayName = if ([string]::IsNullOrWhiteSpace($DisplayName)) { $defaults.display_name } else { $DisplayName }
    $resolvedPrefix = if ([string]::IsNullOrWhiteSpace($Prefix)) { $defaults.prefix } else { $Prefix }
    $resolvedServiceAccount = if ([string]::IsNullOrWhiteSpace($ServiceAccount)) { $defaults.service_account } else { $ServiceAccount }
    $resolvedWorkerCount = if ($WorkerCount -gt 0) { $WorkerCount } else { $defaults.worker_count }
    $resolvedTimeoutSec = if ($TimeoutSec -gt 0) { $TimeoutSec } else { $defaults.timeout_sec }
    $assets = @(Get-PcvDesktopNodeProductAssets -SourceRoot $SourceRoot -ProductRoot $paths.product_root)
    $service = New-PcvDesktopNodeServicePlan `
        -SourceRoot $SourceRoot `
        -Paths $paths `
        -ServiceName $resolvedServiceName `
        -DisplayName $resolvedDisplayName `
        -Prefix $resolvedPrefix `
        -ServiceAccount $resolvedServiceAccount `
        -WorkerCount $resolvedWorkerCount `
        -TimeoutSec $resolvedTimeoutSec

    $deletePaths = @($paths.product_root)
    if ($RemoveData) {
        $deletePaths += @(
            $paths.token_file,
            $paths.job_store,
            $paths.event_log,
            $paths.install_log,
            $paths.diagnostics_root
        )
    }

    [ordered]@{
        schema_version = $defaults.schema_version
        action = $Action
        requires_elevation = ($Action -in @('Install', 'Update', 'Rollback', 'Uninstall'))
        product_root = $paths.product_root
        data_root = $paths.data_root
        paths = $paths
        assets = $assets
        auth = [ordered]@{
            api_token_source = 'file'
            api_token_file = $paths.token_file
        }
        service = $service
        remove_data = [bool]$RemoveData
        delete_paths = $deletePaths
    }
}

function ConvertTo-PcvProductJson {
    param([Parameter(Mandatory, ValueFromPipeline)]$Value)
    process {
        $Value | ConvertTo-Json -Depth 32
    }
}

Export-ModuleMember -Function `
    ConvertTo-PcvProductJson, `
    Get-PcvDesktopNodeProductAssets, `
    Get-PcvDesktopNodeProductDefaults, `
    New-PcvDesktopNodeProductPlan, `
    Resolve-PcvDesktopNodeProductPaths
```

- [ ] **Step 2: Run the plan contract test**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: 4 passed, 0 failed.

- [ ] **Step 3: Commit the green plan contract**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1
git commit -m "Add Desktop Node product plan contract"
```

Expected: commit succeeds.

## Task 3: Entrypoint command surface

**Files:**
- Create: `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: Write the failing entrypoint tests**

Create `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`:

```powershell
Set-StrictMode -Version Latest

Describe 'Invoke-PcvDesktopNodeProduct entrypoint' {
    BeforeAll {
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
        $script:Entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1'
    }

    It 'prints a JSON plan for the Plan action' {
        $json = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:Entrypoint `
            -Action Plan `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node'

        $LASTEXITCODE | Should -Be 0
        $plan = $json | ConvertFrom-Json
        $plan.action | Should -Be 'Plan'
        $plan.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        $plan.auth.api_token_source | Should -Be 'file'
    }

    It 'treats Install -WhatIf as a dry-run plan' {
        $json = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:Entrypoint `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WhatIf

        $LASTEXITCODE | Should -Be 0
        $result = $json | ConvertFrom-Json
        $result.action | Should -Be 'Install'
        $result.dry_run | Should -BeTrue
        $result.execution_skipped | Should -BeTrue
    }

    It 'rejects mutating actions without -WhatIf or -DryRun until orchestration is implemented' {
        $json = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:Entrypoint `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' 2>$null

        $LASTEXITCODE | Should -Be 1
        $result = $json | ConvertFrom-Json
        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_MUTATION_NOT_IMPLEMENTED'
    }
}
```

- [ ] **Step 2: Run the red entrypoint test**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because `Invoke-PcvDesktopNodeProduct.ps1` does not exist.

- [ ] **Step 3: Add entrypoint helper functions to the module**

Append this function before `Export-ModuleMember` in `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`:

```powershell
function New-PcvProductError {
    param(
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [string]$Detail = ''
    )

    [ordered]@{
        ok = $false
        error = [ordered]@{
            code = $Code
            message = $Message
            detail = $Detail
        }
    }
}

function New-PcvDesktopNodeDryRunResult {
    param([Parameter(Mandatory)]$Plan)

    $result = [ordered]@{}
    foreach ($key in $Plan.Keys) {
        $result[$key] = $Plan[$key]
    }
    $result.ok = $true
    $result.dry_run = $true
    $result.execution_skipped = $true
    $result
}
```

Add `New-PcvDesktopNodeDryRunResult` and `New-PcvProductError` to `Export-ModuleMember`.

- [ ] **Step 4: Create the entrypoint**

Create `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`:

```powershell
[CmdletBinding()]
param(
    [ValidateSet('Plan', 'Install', 'Update', 'Rollback', 'Uninstall', 'Status', 'CollectDiagnostics')][string]$Action = 'Plan',
    [string]$SourceRoot,
    [string]$ProductRoot,
    [string]$DataRoot,
    [string]$ServiceName,
    [string]$DisplayName,
    [string]$Prefix,
    [string]$ServiceAccount,
    [int]$WorkerCount = 0,
    [int]$TimeoutSec = 0,
    [switch]$RemoveData,
    [switch]$DryRun,
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvDesktopNodeProduct.psm1'
Import-Module $modulePath -Force

try {
    if ([string]::IsNullOrWhiteSpace($SourceRoot)) {
        $SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
    }

    $plan = New-PcvDesktopNodeProductPlan `
        -Action $Action `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot `
        -ServiceName $ServiceName `
        -DisplayName $DisplayName `
        -Prefix $Prefix `
        -ServiceAccount $ServiceAccount `
        -WorkerCount $WorkerCount `
        -TimeoutSec $TimeoutSec `
        -RemoveData:$RemoveData

    if ($Action -eq 'Plan') {
        $plan | ConvertTo-PcvProductJson
        exit 0
    }

    if ($DryRun -or $WhatIf) {
        New-PcvDesktopNodeDryRunResult -Plan $plan | ConvertTo-PcvProductJson
        exit 0
    }

    New-PcvProductError `
        -Code 'PCV_PRODUCT_MUTATION_NOT_IMPLEMENTED' `
        -Message 'Mutating product actions require the Phase 12 orchestration task.' `
        -Detail 'Run with -WhatIf for a dry-run plan until Task 5 is complete.' |
        ConvertTo-PcvProductJson
    exit 1
}
catch {
    New-PcvProductError `
        -Code 'PCV_PRODUCT_COMMAND_FAILED' `
        -Message 'The Desktop Node product command failed.' `
        -Detail $_.Exception.Message |
        ConvertTo-PcvProductJson
    exit 1
}
```

- [ ] **Step 5: Run entrypoint tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: 3 passed, 0 failed.

- [ ] **Step 6: Commit the entrypoint**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "Add Desktop Node product entrypoint"
```

Expected: commit succeeds.

## Task 4: Asset manifest and product file copy

**Files:**
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: Write the failing manifest tests**

Create `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`:

```powershell
Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct manifest and asset copy' {
    BeforeAll {
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'creates a manifest containing required product assets' {
        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -Version '0.12.0-test'

        $manifest.schema_version | Should -Be 1
        $manifest.product | Should -Be 'PureCVisor Desktop Node'
        $manifest.version | Should -Be '0.12.0-test'
        $manifest.paths.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        @($manifest.assets.relative_path) | Should -Contain 'api\Invoke-PcvDesktopApi.ps1'
        @($manifest.assets.relative_path) | Should -Contain 'web\index.html'
        @($manifest.assets.relative_path) | Should -Contain 'hyperv\Invoke-PcvHyperV.ps1'
        @($manifest.assets.relative_path) | Should -Contain 'service\PcvDesktopService.psm1'
    }

    It 'copies required assets and writes product-manifest.json' {
        $productRoot = Join-Path $TestDrive 'DesktopNode'

        $result = Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data') `
            -Version '0.12.0-test'

        $result.ok | Should -BeTrue
        Test-Path (Join-Path $productRoot 'api\Invoke-PcvDesktopApi.ps1') | Should -BeTrue
        Test-Path (Join-Path $productRoot 'web\index.html') | Should -BeTrue
        Test-Path (Join-Path $productRoot 'hyperv\Invoke-PcvHyperV.ps1') | Should -BeTrue
        Test-Path (Join-Path $productRoot 'service\PcvDesktopService.psm1') | Should -BeTrue
        Test-Path (Join-Path $productRoot 'product-manifest.json') | Should -BeTrue

        $manifest = Get-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Raw | ConvertFrom-Json
        $manifest.version | Should -Be '0.12.0-test'
    }
}
```

- [ ] **Step 2: Run the red manifest tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because manifest/copy functions are missing.

- [ ] **Step 3: Add manifest and copy functions**

Add these functions to `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`:

```powershell
function Get-PcvDesktopNodeAssetFileList {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)][string]$ProductRoot
    )

    $assets = @(Get-PcvDesktopNodeProductAssets -SourceRoot $SourceRoot -ProductRoot $ProductRoot)
    $files = @()
    foreach ($asset in $assets) {
        if (-not (Test-Path -LiteralPath $asset.source -PathType Container)) {
            throw "PCV_PRODUCT_ASSET_SOURCE_MISSING|The product asset source is missing.|Missing path: $($asset.source)"
        }
        $assetFiles = Get-ChildItem -LiteralPath $asset.source -Recurse -File |
            Where-Object { $_.FullName -notmatch '\\tests\\' }
        foreach ($file in $assetFiles) {
            $relativeInsideAsset = [System.IO.Path]::GetRelativePath($asset.source, $file.FullName)
            $relativePath = Join-Path $asset.name $relativeInsideAsset
            $files += [ordered]@{
                asset = $asset.name
                source = $file.FullName
                destination = Join-Path $ProductRoot $relativePath
                relative_path = $relativePath
                length = $file.Length
            }
        }
    }
    Write-Output -NoEnumerate $files
}

function New-PcvDesktopNodeProductManifest {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)][string]$ProductRoot,
        [Parameter(Mandatory)][string]$DataRoot,
        [string]$Version = '0.12.0'
    )

    $paths = Resolve-PcvDesktopNodeProductPaths -ProductRoot $ProductRoot -DataRoot $DataRoot
    $assets = @(Get-PcvDesktopNodeAssetFileList -SourceRoot $SourceRoot -ProductRoot $ProductRoot)

    [ordered]@{
        schema_version = 1
        product = 'PureCVisor Desktop Node'
        version = $Version
        source_root = $SourceRoot
        generated_at = (Get-Date).ToUniversalTime().ToString('o')
        paths = $paths
        assets = $assets
    }
}

function Copy-PcvDesktopNodeProductAssets {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)][string]$ProductRoot,
        [Parameter(Mandatory)][string]$DataRoot,
        [string]$Version = '0.12.0'
    )

    $manifest = New-PcvDesktopNodeProductManifest `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot `
        -Version $Version

    foreach ($file in $manifest.assets) {
        $parent = Split-Path -Parent $file.destination
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
        Copy-Item -LiteralPath $file.source -Destination $file.destination -Force
    }

    New-Item -ItemType Directory -Path $ProductRoot -Force | Out-Null
    $manifestPath = Join-Path $ProductRoot 'product-manifest.json'
    $manifest | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

    [ordered]@{
        ok = $true
        product_root = $ProductRoot
        manifest_path = $manifestPath
        asset_count = @($manifest.assets).Count
    }
}
```

Add `Copy-PcvDesktopNodeProductAssets`, `Get-PcvDesktopNodeAssetFileList`, and `New-PcvDesktopNodeProductManifest` to `Export-ModuleMember`.

- [ ] **Step 4: Run manifest tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: 2 passed, 0 failed.

- [ ] **Step 5: Commit the manifest work**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1
git commit -m "Add Desktop Node product manifest"
```

Expected: commit succeeds.

## Task 5: Product action orchestration

**Files:**
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`

- [ ] **Step 1: Add orchestration tests to the entrypoint suite**

Append these tests inside the `Describe` block in `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`:

```powershell
    It 'executes install orchestration through injectable runners' {
        Import-Module (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1') -Force
        $script:Steps = @()
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $script:Steps += "$FileName $($Arguments -join ' ')"
            [ordered]@{ exit_code = 0; stdout = 'ok'; stderr = '' }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$Version)
            $script:Steps += "copy $ProductRoot"
            [ordered]@{ ok = $true; product_root = $ProductRoot; manifest_path = (Join-Path $ProductRoot 'product-manifest.json'); asset_count = 4 }
        }
        $token = {
            param([string]$Path, [string]$ServiceAccount)
            $script:Steps += "token $Path"
            [ordered]@{ ok = $true; path = $Path; token_length = 43 }
        }
        $health = {
            param([string]$Prefix)
            $script:Steps += "health $Prefix"
            [ordered]@{ ok = $true; prefix = $Prefix }
        }

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -CopyAssets $copy `
            -PrepareTokenFile $token `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'Install'
        $script:Steps | Should -Contain "copy $($plan.product_root)"
        $script:Steps | Should -Contain "token $($plan.auth.api_token_file)"
        ($script:Steps -join "`n") | Should -Match 'sc.exe create PureCVisorDesktopNode'
        ($script:Steps -join "`n") | Should -Match 'sc.exe start PureCVisorDesktopNode'
        $script:Steps | Should -Contain 'health http://127.0.0.1:7777/'
    }

    It 'executes rollback using stop, restore, service install, start, and health check steps' {
        Import-Module (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1') -Force
        $script:Steps = @()
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $script:Steps += "$FileName $($Arguments -join ' ')"
            [ordered]@{ exit_code = 0; stdout = 'ok'; stderr = '' }
        }
        $restore = {
            param([string]$ProductRoot)
            $script:Steps += "restore $ProductRoot"
            [ordered]@{ ok = $true; product_root = $ProductRoot }
        }
        $health = {
            param([string]$Prefix)
            $script:Steps += "health $Prefix"
            [ordered]@{ ok = $true; prefix = $Prefix }
        }

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RestorePreviousProductRoot $restore `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'Rollback'
        ($script:Steps -join "`n") | Should -Match 'sc.exe stop PureCVisorDesktopNode'
        $script:Steps | Should -Contain "restore $($plan.product_root)"
        ($script:Steps -join "`n") | Should -Match 'sc.exe start PureCVisorDesktopNode'
        $script:Steps | Should -Contain 'health http://127.0.0.1:7777/'
    }
```

- [ ] **Step 2: Run red orchestration tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because `Invoke-PcvDesktopNodeProductAction` is missing.

- [ ] **Step 3: Add orchestration functions**

Add these functions to `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`:

```powershell
function Invoke-PcvProductProcessCommand {
    param(
        [Parameter(Mandatory)][object[]]$Commands,
        [Parameter(Mandatory)][scriptblock]$InvokeProcess
    )

    $results = @()
    foreach ($command in $Commands) {
        $result = & $InvokeProcess -FileName $command.file_name -Arguments $command.arguments
        $results += [ordered]@{
            command = $command
            result = $result
        }
        if ([int]$result.exit_code -ne 0) {
            throw "PCV_PRODUCT_COMMAND_FAILED|A product command failed.|$($command.file_name) $($command.arguments -join ' ')"
        }
    }
    Write-Output -NoEnumerate $results
}

function Invoke-PcvDesktopNodeProductAction {
    param(
        [Parameter(Mandatory)]$Plan,
        [scriptblock]$CopyAssets,
        [scriptblock]$PrepareTokenFile,
        [scriptblock]$InvokeProcess,
        [scriptblock]$TestHealth,
        [scriptblock]$RestorePreviousProductRoot
    )

    if ($null -eq $InvokeProcess) {
        $InvokeProcess = {
            param([string]$FileName, [string[]]$Arguments)
            $process = [System.Diagnostics.Process]::new()
            $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
            $process.StartInfo.FileName = $FileName
            foreach ($argument in $Arguments) {
                [void]$process.StartInfo.ArgumentList.Add($argument)
            }
            $process.StartInfo.RedirectStandardOutput = $true
            $process.StartInfo.RedirectStandardError = $true
            [void]$process.Start()
            $stdout = $process.StandardOutput.ReadToEnd()
            $stderr = $process.StandardError.ReadToEnd()
            $process.WaitForExit()
            [ordered]@{ exit_code = $process.ExitCode; stdout = $stdout; stderr = $stderr }
        }
    }

    if ($null -eq $CopyAssets) {
        $CopyAssets = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$Version)
            Copy-PcvDesktopNodeProductAssets -SourceRoot $SourceRoot -ProductRoot $ProductRoot -DataRoot $DataRoot -Version $Version
        }
    }

    if ($null -eq $PrepareTokenFile) {
        $PrepareTokenFile = {
            param([string]$Path, [string]$ServiceAccount)
            Import-PcvDesktopServiceSupport -SourceRoot (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
            New-PcvDesktopServiceTokenFile -Path $Path -ServiceAccount $ServiceAccount
        }
    }

    if ($null -eq $TestHealth) {
        $TestHealth = {
            param([string]$Prefix)
            $uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy')
            $response = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 10
            [ordered]@{ ok = ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300); status_code = $response.StatusCode; uri = $uri }
        }
    }

    if ($null -eq $RestorePreviousProductRoot) {
        $RestorePreviousProductRoot = {
            param([string]$ProductRoot)
            [ordered]@{ ok = $true; product_root = $ProductRoot; restored = $false }
        }
    }

    $executed = @()
    try {
        if ($Plan.action -eq 'Install') {
            $executed += (& $CopyAssets -SourceRoot $Plan.source_root -ProductRoot $Plan.product_root -DataRoot $Plan.data_root -Version '0.12.0')
            $executed += (& $PrepareTokenFile -Path $Plan.auth.api_token_file -ServiceAccount $Plan.service.config.service_account)
            $executed += @(Invoke-PcvProductProcessCommand -Commands $Plan.service.commands -InvokeProcess $InvokeProcess)
            $executed += @(Invoke-PcvProductProcessCommand -Commands $Plan.service.start_commands -InvokeProcess $InvokeProcess)
            $executed += (& $TestHealth -Prefix $Plan.service.config.prefix)
        }
        elseif ($Plan.action -eq 'Rollback') {
            $executed += @(Invoke-PcvProductProcessCommand -Commands $Plan.service.stop_commands -InvokeProcess $InvokeProcess)
            $executed += (& $RestorePreviousProductRoot -ProductRoot $Plan.product_root)
            $executed += @(Invoke-PcvProductProcessCommand -Commands $Plan.service.commands -InvokeProcess $InvokeProcess)
            $executed += @(Invoke-PcvProductProcessCommand -Commands $Plan.service.start_commands -InvokeProcess $InvokeProcess)
            $executed += (& $TestHealth -Prefix $Plan.service.config.prefix)
        }
        else {
            throw "PCV_PRODUCT_ACTION_UNSUPPORTED|The product action is not wired yet.|Action: $($Plan.action)"
        }

        [ordered]@{
            ok = $true
            action = $Plan.action
            executed = $executed
        }
    }
    catch {
        [ordered]@{
            ok = $false
            action = $Plan.action
            error = [ordered]@{
                code = 'PCV_PRODUCT_ACTION_FAILED'
                message = 'The product action failed.'
                detail = $_.Exception.Message
            }
            executed = $executed
        }
    }
}
```

Add `Invoke-PcvDesktopNodeProductAction` and `Invoke-PcvProductProcessCommand` to `Export-ModuleMember`.

Also add `source_root = $SourceRoot` to the object returned by `New-PcvDesktopNodeProductPlan`.

- [ ] **Step 4: Wire mutating entrypoint actions**

In `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`, replace the `PCV_PRODUCT_MUTATION_NOT_IMPLEMENTED` block with:

```powershell
$result = Invoke-PcvDesktopNodeProductAction -Plan $plan
$result | ConvertTo-PcvProductJson
if ($result.ok) { exit 0 }
exit 1
```

- [ ] **Step 5: Run orchestration tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: all entrypoint/orchestration tests pass.

- [ ] **Step 6: Commit orchestration**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "Add Desktop Node product orchestration"
```

Expected: commit succeeds.

## Task 6: Diagnostic bundle and redaction

**Files:**
- Create: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: Write failing diagnostic tests**

Create `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`:

```powershell
Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct diagnostics' {
    BeforeAll {
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'redacts tokens and Authorization headers from diagnostic objects' {
        $inputObject = [ordered]@{
            token = 'secret-token'
            api_token_file = 'C:\ProgramData\PureCVisor\desktop-node\api-token.txt'
            headers = [ordered]@{
                Authorization = 'Bearer secret-token'
                Accept = 'application/json'
            }
            nested = [ordered]@{
                access_token = 'nested-secret'
                message = 'safe'
            }
        }

        $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $inputObject
        $json = $redacted | ConvertTo-Json -Depth 16

        $json | Should -Not -Match 'secret-token'
        $json | Should -Not -Match 'nested-secret'
        $redacted.token | Should -Be '[REDACTED]'
        $redacted.headers.Authorization | Should -Be '[REDACTED]'
        $redacted.headers.Accept | Should -Be 'application/json'
        $redacted.nested.access_token | Should -Be '[REDACTED]'
    }

    It 'writes a diagnostic bundle without token file content' {
        $outRoot = Join-Path $TestDrive 'diagnostics'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
        Set-Content -LiteralPath $plan.auth.api_token_file -Value 'super-secret-token' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath $plan.paths.event_log -Value '{"Authorization":"Bearer super-secret-token","message":"boot"}' -Encoding UTF8
        Set-Content -LiteralPath $plan.paths.job_store -Value '{"jobs":[{"id":"job-1","status":"queued"}]}' -Encoding UTF8

        $bundle = New-PcvDesktopNodeDiagnosticBundle -Plan $plan -OutputRoot $outRoot

        $bundle.ok | Should -BeTrue
        Test-Path $bundle.path | Should -BeTrue
        $combined = Get-ChildItem -LiteralPath $bundle.path -File | ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } | Out-String
        $combined | Should -Not -Match 'super-secret-token'
        $combined | Should -Match '\[REDACTED\]'
        Test-Path (Join-Path $bundle.path 'token-file.txt') | Should -BeFalse
    }
}
```

- [ ] **Step 2: Run red diagnostic tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because diagnostic functions are missing.

- [ ] **Step 3: Add redaction and diagnostic bundle functions**

Add these functions to `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`:

```powershell
function Test-PcvDiagnosticSensitiveKey {
    param([Parameter(Mandatory)][string]$Key)
    $Key -match '(?i)(^token$|api_token|access_token|authorization|password|secret)'
}

function ConvertTo-PcvDesktopNodeDiagnosticRedactedObject {
    param([Parameter(Mandatory)]$InputObject)

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [System.Collections.IDictionary]) {
        $out = [ordered]@{}
        foreach ($key in $InputObject.Keys) {
            if (Test-PcvDiagnosticSensitiveKey -Key ([string]$key)) {
                $out[$key] = '[REDACTED]'
            }
            else {
                $out[$key] = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $InputObject[$key]
            }
        }
        return $out
    }

    if ($InputObject -is [pscustomobject]) {
        $out = [ordered]@{}
        foreach ($property in $InputObject.PSObject.Properties) {
            if (Test-PcvDiagnosticSensitiveKey -Key $property.Name) {
                $out[$property.Name] = '[REDACTED]'
            }
            else {
                $out[$property.Name] = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $property.Value
            }
        }
        return $out
    }

    if ($InputObject -is [System.Collections.IEnumerable] -and $InputObject -isnot [string]) {
        $items = @()
        foreach ($item in $InputObject) {
            $items += ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $item
        }
        return $items
    }

    if ($InputObject -is [string] -and $InputObject -match '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+') {
        return ($InputObject -replace '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+', 'Bearer [REDACTED]')
    }

    return $InputObject
}

function New-PcvDesktopNodeDiagnosticBundle {
    param(
        [Parameter(Mandatory)]$Plan,
        [string]$OutputRoot
    )

    if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
        $OutputRoot = $Plan.paths.diagnostics_root
    }

    $bundlePath = Join-Path $OutputRoot ("bundle-" + (Get-Date).ToUniversalTime().ToString('yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Path $bundlePath -Force | Out-Null

    $summary = [ordered]@{
        generated_at = (Get-Date).ToUniversalTime().ToString('o')
        product_root = $Plan.product_root
        data_root = $Plan.data_root
        service_name = $Plan.service.config.service_name
        prefix = $Plan.service.config.prefix
    }
    $summary | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath (Join-Path $bundlePath 'summary.json') -Encoding UTF8

    if (Test-Path -LiteralPath $Plan.paths.event_log -PathType Leaf) {
        $eventText = Get-Content -LiteralPath $Plan.paths.event_log -Raw
        $eventText = $eventText -replace '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+', 'Bearer [REDACTED]'
        $eventText | Set-Content -LiteralPath (Join-Path $bundlePath 'events-redacted.jsonl') -Encoding UTF8
    }

    if (Test-Path -LiteralPath $Plan.paths.job_store -PathType Leaf) {
        $jobText = Get-Content -LiteralPath $Plan.paths.job_store -Raw
        try {
            $jobObject = $jobText | ConvertFrom-Json
            $redactedJobs = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $jobObject
            $redactedJobs | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath (Join-Path $bundlePath 'jobs-redacted.json') -Encoding UTF8
        }
        catch {
            '[UNREADABLE JOB STORE]' | Set-Content -LiteralPath (Join-Path $bundlePath 'jobs-redacted.json') -Encoding UTF8
        }
    }

    [ordered]@{
        ok = $true
        path = $bundlePath
    }
}
```

Add `ConvertTo-PcvDesktopNodeDiagnosticRedactedObject`, `New-PcvDesktopNodeDiagnosticBundle`, and `Test-PcvDiagnosticSensitiveKey` to `Export-ModuleMember`.

- [ ] **Step 4: Run diagnostic tests**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: 2 passed, 0 failed.

- [ ] **Step 5: Commit diagnostics**

Run:

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1
git commit -m "Add Desktop Node product diagnostics"
```

Expected: commit succeeds.

## Task 7: Packaging docs and boundary updates

**Files:**
- Create: `packaging/windows-desktop-node/README.md`
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`
- Modify: `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
- Modify: `follower.md`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [ ] **Step 1: Add boundary test for Phase 12 docs**

Append this test to `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`:

```powershell
It 'documents the Phase 12 service-first product wrapper boundary' {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
    $phase12Spec = Get-Content -Path (Join-Path $repoRoot 'docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime-design.md') -Raw
    $phase12Plan = Get-Content -Path (Join-Path $repoRoot 'docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime.md') -Raw
    $rootReadme = Get-Content -Path (Join-Path $repoRoot 'spikes/purecvisor-desktop-node/README.md') -Raw
    $releaseBoundary = Get-Content -Path (Join-Path $repoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw

    $phase12Spec | Should -Match 'DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper'
    $phase12Plan | Should -Match 'packaging/windows-desktop-node'
    $rootReadme | Should -Match 'Phase 12'
    $rootReadme | Should -Match 'service-first'
    $releaseBoundary | Should -Match 'Phase 12'
    $releaseBoundary | Should -Match 'Service-first'
}
```

- [ ] **Step 2: Run red boundary test**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -eq 0) { exit 1 }"
```

Expected: fails because README/release boundary do not yet mention Phase 12.

- [ ] **Step 3: Create packaging README**

Create `packaging/windows-desktop-node/README.md` with:

```markdown
# PureCVisor Desktop Node Service-first Product Wrapper

이 디렉터리는 Desktop Node Phase 12 Service-first 제품 런타임 wrapper다.

## 기본 경로

- 제품 루트: `C:\Program Files\PureCVisor\DesktopNode`
- 데이터 루트: `%ProgramData%\PureCVisor\desktop-node`
- 서비스명: `PureCVisorDesktopNode`
- 기본 URL: `http://127.0.0.1:7777/`

## Dry-run plan

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall -RemoveData -WhatIf
```

## 관리자 smoke

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
Invoke-WebRequest http://127.0.0.1:7777/api/v1/runtime/policy
Invoke-WebRequest http://127.0.0.1:7777/
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall
```

## 데이터 보존

기본 uninstall은 `%ProgramData%\PureCVisor\desktop-node` 데이터를 보존한다. token, job store, event log, install log, diagnostics를 제거하려면 `-RemoveData`를 명시한다.

## 제외 범위

Phase 14에서 WiX MSI-first installer source와 unsigned dev install/uninstall smoke를 추가했다. 이후 Phase 15-18에서 DPAPI protected token, JSONL first diagnostics, LAN security policy, manifest-first update/rollback을 추가했고, 2026-04-30에는 local test certificate 기준 signed RC MSI lifecycle과 Event Log source lifecycle evidence를 기록했다. Full transactional rollback, public trusted/stable signing, 내장 LAN TLS는 후속 판단으로 남는다.
```

- [ ] **Step 4: Update active docs**

Apply these doc changes:

- `README.md`: add Phase 12 product wrapper to the Desktop Node paragraph and add the packaging suite command under verification.
- `AGENTS.md`: add Phase 12 spec/plan/packaging README to document entrypoints and add `Invoke-Pester -Path 'packaging/windows-desktop-node/tests'`.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`: add that Phase 12 starts service-first product wrapper promotion while signed release build/full updater remain separate.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: add a row for `packaging/windows-desktop-node/**` changes requiring packaging suite plus service/API/web suites.
- `docs/DEVELOPER_INDEX.md`: add Phase 12 spec, plan, and packaging README entries.
- `spikes/purecvisor-desktop-node/README.md`: mention that Phase 12 wrapper consumes the spike assets as product-candidate payload without moving the spike implementation.
- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`: add an "후속 결정" note that Phase 12 partially supersedes the keep-spike gate for service-first packaging.
- `follower.md`: add Phase 12 as current P0 and record that service-first product runtime design/plan are approved.

- [ ] **Step 5: Run boundary and packaging doc checks**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
rg -n "Phase 12|service-first|packaging/windows-desktop-node|DESKTOP_NODE_PHASE12_RUNTIME_DECISION" README.md AGENTS.md docs spikes/purecvisor-desktop-node packaging/windows-desktop-node
git diff --check
```

Expected: root boundary suite passes and grep output shows Phase 12 references in active docs.

- [ ] **Step 6: Commit docs**

Run:

```powershell
git add README.md AGENTS.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md follower.md spikes/purecvisor-desktop-node/README.md spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1 packaging/windows-desktop-node/README.md
git commit -m "Document Desktop Node phase 12 wrapper"
```

Expected: commit succeeds.

## Task 8: Full Phase 12 verification

**Files:**
- Modify: `docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime.md`
- Modify: `follower.md`

- [ ] **Step 1: Run packaging suite**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
```

Expected: all packaging tests pass.

- [ ] **Step 2: Run Desktop Node component suites**

Run:

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
node --check spikes/purecvisor-desktop-node/web/app.js
```

Expected: existing component suites pass with current expected counts.

- [ ] **Step 3: Run product dry-run smoke**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall -RemoveData -WhatIf
```

Expected: each command exits 0 and returns JSON.

- [ ] **Step 4: Run diff hygiene**

Run:

```powershell
git diff --check
git status -sb
```

Expected: `git diff --check` exits 0. `git status -sb` shows only intended files before the final commit.

- [ ] **Step 5: Record verification evidence**

Update the `완료 증거` section at the end of this plan with the exact pass counts and command outcomes from Steps 1-4. Update `follower.md` with the current Phase 12 status and remaining administrator smoke items.

- [ ] **Step 6: Commit final verification notes**

Run:

```powershell
git add docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime.md follower.md
git commit -m "Record Desktop Node phase 12 verification"
```

Expected: commit succeeds.

## 최종 검증 명령

Task 8에서 아래 명령을 실행하고, 관측한 pass count와 exit code를 이 계획의 완료 증거로 갱신한다.

```powershell
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$r = Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed -PassThru; if ($r.FailedCount -gt 0) { exit 1 }"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Administrator opt-in smoke remains separate:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
Invoke-WebRequest http://127.0.0.1:7777/api/v1/runtime/policy
Invoke-WebRequest http://127.0.0.1:7777/
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall
```

## 완료 증거

실행 일자: 2026-04-26

Phase 12 기본 검증 결과:

| 검증 | 결과 |
|------|------|
| `Invoke-Pester -Path 'packaging/windows-desktop-node/tests'` | 32 passed, 0 failed |
| `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests'` | 85 passed, 0 failed |
| `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests'` | 13 passed, 0 failed |
| `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests'` | 11 passed, 0 failed |
| `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration` | 41 passed, 0 failed, 1 NotRun |
| `node --check spikes/purecvisor-desktop-node/web/app.js` | exit 0 |
| `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests'` | 11 passed, 0 failed |
| `Invoke-PcvDesktopNodeProduct.ps1 -Action Plan` | exit 0, JSON plan 출력 |
| `Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WhatIf` | exit 0, JSON dry-run 출력 |
| `Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall -RemoveData -WhatIf` | exit 0, JSON dry-run 출력 |
| `Invoke-PcvDesktopNodeProduct.ps1 -Action Status` | exit 0, 서비스 미설치 상태를 JSON으로 보고 |
| `git diff --check` | exit 0 |

관리자 권한 opt-in으로 남은 검증:

- 2026-04-26 실제 `Install`을 관리자 권한으로 실행했다. 제품 자산 복사, token file 준비/ACL, `sc.exe create`는 성공했다.
- 첫 실행은 service `binPath`가 `"pwsh.exe"` 상대 경로로 등록되어 `sc.exe start` 오류 2로 실패했다. 이후 service config는 `pwsh.exe`를 절대 경로로 resolve하도록 보강했다.
- 절대 경로 보강 후 재실행한 `Install`은 `sc.exe start` 오류 1053으로 실패했다. Windows SCM이 직접 `pwsh.exe -File Invoke-PcvDesktopApi.ps1` listener를 native service process로 관리할 수 없는 것이 남은 제품 차단점이다.
- 직접 PowerShell listener 실행은 `GET /api/v1/runtime/policy`가 token 포함 요청에서 200을 반환했다. 따라서 API listener 자체와 SCM service host 문제를 분리해 다룬다.
- 제품 service의 `-ApiTokenFile` 기본값에서는 Web Console root 무인증 요청이 `401`을 반환한다. Phase 13에서 static asset 인증 예외 또는 token 전달 UX를 결정한다.
- 실제 `Status`, `CollectDiagnostics`, 기본 `Uninstall`은 실행됐다. 진단 번들은 `%ProgramData%\PureCVisor\desktop-node\diagnostics\bundle-20260426-072517-54f21988`에 생성됐고, 기본 uninstall은 service 등록과 제품 루트를 제거하고 데이터 루트는 보존했다.
- 실제 `Rollback` 이전 제품 루트 복원 확인
- 실제 `Uninstall -RemoveData` 데이터 삭제 확인
- 실제 Hyper-V VM create/start/poweroff/checkpoint lifecycle integration
