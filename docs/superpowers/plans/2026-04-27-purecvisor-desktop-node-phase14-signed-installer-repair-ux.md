# PureCVisor Desktop Node Phase 14 Signed Installer and Repair UX Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Phase 13 WinSW product wrapper를 WiX MSI-first signed installer 산출물로 감싸고, repair/uninstall/remove-data 경로가 service lifecycle과 data 보존 정책을 우회하지 않게 만든다.

**Architecture:** MSI는 `%ProgramFiles%\PureCVisor\DesktopNode` 파일 설치, repair, 제거를 소유한다. Product wrapper는 MSI 전용 `ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled` action으로 service/data configuration만 수행하고, 기존 standalone `Install`/`Uninstall` action은 관리자 smoke와 개발자 CLI용으로 유지한다. Installer build script는 WiX source, signing input, WinSW provenance, installer provenance manifest를 한 경계에서 검증한다.

**Tech Stack:** PowerShell 7, Pester 5, WiX Toolset CLI, Windows Installer MSI, WinSW, JSON Schema draft 2020-12 형식의 provenance contract.

---

## 설계 기준

- 설계 문서: `docs/superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md`
- 결정 토큰: `DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first`
- 유지 결정: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 경계: `packaging/windows-desktop-node/**`와 `spikes/purecvisor-desktop-node/**` 바깥의 Linux `purecvisorsd`, libvirt/KVM, Single Edge 공개 UI/API 표면은 변경하지 않는다.

## 현행화 상태

2026-04-28 기준 Phase 14 기본 구현, 문서 연결, Pester 검증, static check, 실제 unsigned dev MSI build가 완료됐다. `build.ps1`의 제품 build 경로는 `.wixproj`가 아니라 `Product.wxs`와 `ProductActions.wxs`를 `wix build`에 전달한다. `.wixproj`는 source include와 IDE/MSBuild 보조 경계로 유지한다.

남은 항목은 signing secret이 필요한 signed release build와 elevated PowerShell이 필요한 `msiexec` install/repair/uninstall/remove-data smoke다. 이 둘은 조건부 관리자 opt-in 검증으로 유지하고, 준비되지 않은 경우 Phase 15 secure token storage spec으로 넘어갈 수 있다.

## 파일 구조

생성:

- `packaging/windows-desktop-node/installer/README.md`: installer build, signing, repair/uninstall/remove-data smoke 사용법.
- `packaging/windows-desktop-node/installer/PureCVisorDesktopNode.wixproj`: WiX build project와 source include.
- `packaging/windows-desktop-node/installer/Product.wxs`: product identity, per-machine install scope, install directory, core payload component group.
- `packaging/windows-desktop-node/installer/ProductActions.wxs`: MSI custom action property, install/repair/uninstall sequencing, `REMOVE_DATA` mapping.
- `packaging/windows-desktop-node/installer/build.ps1`: payload staging, WiX CLI detection, MSI build, optional signing, provenance manifest 생성.
- `packaging/windows-desktop-node/installer/installer-provenance.schema.json`: provenance manifest 최소 schema.
- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`: build script parameter, dry-run, structured error 검증.
- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`: WiX source의 product/action 경계 검증.
- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1`: signing/provenance policy 검증.

수정:

- `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`: `ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled` action validate set 추가.
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: MSI 전용 action plan과 invoke flow 추가.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`: new action plan, elevation, delete path 검증.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`: new action invoke order, no-copy/no-product-root-delete, remove-data 검증.
- `packaging/windows-desktop-node/README.md`: Phase 14 installer와 MSI 전용 product action 사용법.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: Phase 14 installer suite와 관리자 smoke 기준.
- `docs/DEVELOPER_INDEX.md`: Phase 14 구현 계획 진입점.
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`: Phase 14 상태를 구현 준비 상태로 갱신.
- `follower.md`: 다음 실행 권장 순서를 Phase 14 구현으로 갱신.

## 작업 원칙

- 각 task는 테스트를 먼저 추가하고 실패를 확인한 뒤 구현한다.
- MSI custom action은 raw token을 MSI property, command line, log에 넣지 않는다.
- `RemoveInstalled` 기본 경로는 ProgramData를 보존한다.
- `RemoveInstalled -RemoveData`만 token/job/event/install/diagnostics 경로를 삭제한다.
- MSI 전용 action은 product root 파일 복사나 product root 삭제를 하지 않는다.
- WiX가 설치되지 않은 환경에서도 Pester suite는 static/dry-run 검증으로 통과해야 한다.

---

### Task 1: Product Wrapper Plan Contract

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: MSI 전용 action plan 실패 테스트를 추가한다**

`packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`에 다음 `Describe` 블록을 추가한다.

```powershell
Describe 'New-PcvDesktopNodeProductPlan MSI installed actions' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:ProductRoot = Join-Path $TestDrive 'product-root'
        $script:DataRoot = Join-Path $TestDrive 'data-root'
        $script:WinSwPath = Join-Path $TestDrive 'winsw.exe'
        New-Item -ItemType Directory -Path $script:ProductRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $script:DataRoot -Force | Out-Null
        Set-Content -LiteralPath $script:WinSwPath -Value 'fake-winsw' -NoNewline
    }

    It 'marks ConfigureInstalled as elevated service configuration without file copy delete paths' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action ConfigureInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath

        $plan.action | Should -Be 'ConfigureInstalled'
        $plan.requires_elevation | Should -BeTrue
        $plan.delete_paths | Should -BeNullOrEmpty
        $plan.release.product_root | Should -Be $script:ProductRoot
        $plan.service.executable_path | Should -Be (Join-Path $script:ProductRoot 'PureCVisorDesktopNode.exe')
    }

    It 'marks RepairInstalled as elevated and preserves token, jobs, events, and diagnostics' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RepairInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath

        $plan.action | Should -Be 'RepairInstalled'
        $plan.requires_elevation | Should -BeTrue
        $plan.delete_paths | Should -BeNullOrEmpty
        $plan.token_file | Should -Be (Join-Path $script:DataRoot 'api-token.txt')
        $plan.job_store | Should -Be (Join-Path $script:DataRoot 'jobs.json')
        $plan.event_log | Should -Be (Join-Path $script:DataRoot 'events.jsonl')
        $plan.diagnostics_root | Should -Be (Join-Path $script:DataRoot 'diagnostics')
    }

    It 'keeps RemoveInstalled default uninstall data-preserving and product-root neutral' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath

        $plan.action | Should -Be 'RemoveInstalled'
        $plan.requires_elevation | Should -BeTrue
        $plan.remove_data | Should -BeFalse
        $plan.delete_paths | Should -BeNullOrEmpty
    }

    It 'lists only ProgramData paths for RemoveInstalled -RemoveData' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath `
            -RemoveData

        $expected = @(
            (Join-Path $script:DataRoot 'api-token.txt'),
            (Join-Path $script:DataRoot 'jobs.json'),
            (Join-Path $script:DataRoot 'events.jsonl'),
            (Join-Path $script:DataRoot 'install.jsonl'),
            (Join-Path $script:DataRoot 'diagnostics')
        )

        $plan.action | Should -Be 'RemoveInstalled'
        $plan.remove_data | Should -BeTrue
        $plan.delete_paths | Should -Be $expected
        $plan.delete_paths | Should -Not -Contain $script:ProductRoot
    }
}
```

- [ ] **Step 2: 실패를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected:

```text
Failed
ParameterBindingValidationException
Cannot validate argument on parameter 'Action'
```

- [ ] **Step 3: entrypoint action validate set을 확장한다**

`packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`의 `Action` validate set을 다음 값으로 교체한다.

```powershell
[ValidateSet(
    'Plan',
    'Install',
    'Update',
    'Rollback',
    'Uninstall',
    'Status',
    'CollectDiagnostics',
    'ConfigureInstalled',
    'RepairInstalled',
    'RemoveInstalled'
)]
[string]$Action = 'Plan',
```

- [ ] **Step 4: module action validate set과 elevation/delete-path 정책을 구현한다**

`packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`의 `New-PcvDesktopNodeProductPlan` `Action` validate set을 같은 값으로 교체한다.

```powershell
[ValidateSet(
    'Plan',
    'Install',
    'Update',
    'Rollback',
    'Uninstall',
    'Status',
    'CollectDiagnostics',
    'ConfigureInstalled',
    'RepairInstalled',
    'RemoveInstalled'
)]
[string]$Action,
```

`$requiresElevation` 계산을 다음 코드로 교체한다.

```powershell
$requiresElevation = $Action -in @(
    'Install',
    'Update',
    'Rollback',
    'Uninstall',
    'ConfigureInstalled',
    'RepairInstalled',
    'RemoveInstalled'
)
```

`$deletePaths` 계산을 다음 코드로 교체한다.

```powershell
$deletePaths = @()
if ($RemoveData) {
    $deletePaths = @(
        $tokenFile,
        $jobStore,
        $eventLog,
        $installLog,
        $diagnosticsRoot
    )

    if ($Action -eq 'Uninstall') {
        $deletePaths = @($productRoot) + $deletePaths
    }
}
```

- [ ] **Step 5: plan contract 테스트 통과를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 6: Task 1을 커밋한다**

```powershell
git add packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1
git commit -m "Add Desktop Node MSI action plan contracts"
```

---

### Task 2: Product Wrapper MSI Invoke Flow

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: ConfigureInstalled invoke 실패 테스트를 추가한다**

`packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`에 다음 테스트를 추가한다.

```powershell
It 'configures an already installed MSI payload without copying assets' {
    $calls = [System.Collections.Generic.List[object]]::new()
    $plan = New-PcvDesktopNodeProductPlan `
        -Action ConfigureInstalled `
        -SourceRoot $repoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -WinSwPath $winSwPath

    $result = Invoke-PcvDesktopNodeProductAction `
        -Plan $plan `
        -InvokeProcess {
            param($FileName, $Arguments)
            $calls.Add([ordered]@{ file = $FileName; args = $Arguments })
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        } `
        -CopyAssets {
            throw 'ConfigureInstalled must not copy assets'
        } `
        -PrepareTokenFile {
            param($Path)
            $calls.Add([ordered]@{ op = 'prepare-token'; path = $Path })
            [ordered]@{ ok = $true; path = $Path }
        } `
        -TestHealth {
            param($Prefix)
            $calls.Add([ordered]@{ op = 'health'; prefix = $Prefix })
            [ordered]@{ ok = $true; status_code = 200 }
        }

    $result.action | Should -Be 'ConfigureInstalled'
    $result.ok | Should -BeTrue
    ($calls | Where-Object { $_.op -eq 'prepare-token' }).Count | Should -Be 1
    ($calls | Where-Object { $_.args -contains 'install' }).Count | Should -Be 1
    ($calls | Where-Object { $_.args -contains 'start' }).Count | Should -Be 1
    ($calls | Where-Object { $_.op -eq 'health' }).Count | Should -Be 1
}
```

- [ ] **Step 2: RepairInstalled invoke 실패 테스트를 추가한다**

같은 파일에 다음 테스트를 추가한다.

```powershell
It 'repairs service configuration without copying assets or deleting product root' {
    $calls = [System.Collections.Generic.List[object]]::new()
    $plan = New-PcvDesktopNodeProductPlan `
        -Action RepairInstalled `
        -SourceRoot $repoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -WinSwPath $winSwPath

    $result = Invoke-PcvDesktopNodeProductAction `
        -Plan $plan `
        -InvokeProcess {
            param($FileName, $Arguments)
            $calls.Add([ordered]@{ file = $FileName; args = $Arguments })
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        } `
        -CopyAssets {
            throw 'RepairInstalled must not copy assets'
        } `
        -PrepareTokenFile {
            param($Path)
            $calls.Add([ordered]@{ op = 'prepare-token'; path = $Path })
            [ordered]@{ ok = $true; path = $Path }
        } `
        -TestHealth {
            param($Prefix)
            $calls.Add([ordered]@{ op = 'health'; prefix = $Prefix })
            [ordered]@{ ok = $true; status_code = 200 }
        } `
        -RemovePath {
            param($Path)
            throw "RepairInstalled must not remove paths: $Path"
        }

    $result.action | Should -Be 'RepairInstalled'
    $result.ok | Should -BeTrue
    ($calls | Where-Object { $_.args -contains 'stop' }).Count | Should -Be 1
    ($calls | Where-Object { $_.op -eq 'service-stop-wait' }).Count | Should -Be 1
    ($calls | Where-Object { $_.args -contains 'install' }).Count | Should -Be 1
    ($calls | Where-Object { $_.args -contains 'start' }).Count | Should -Be 1
    ($calls | Where-Object { $_.op -eq 'health' }).Count | Should -Be 1
}
```

- [ ] **Step 3: RemoveInstalled invoke 실패 테스트를 추가한다**

같은 파일에 다음 테스트를 추가한다.

```powershell
It 'removes MSI-installed service while preserving data by default' {
    $calls = [System.Collections.Generic.List[object]]::new()
    $plan = New-PcvDesktopNodeProductPlan `
        -Action RemoveInstalled `
        -SourceRoot $repoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -WinSwPath $winSwPath

    $result = Invoke-PcvDesktopNodeProductAction `
        -Plan $plan `
        -InvokeProcess {
            param($FileName, $Arguments)
            $calls.Add([ordered]@{ file = $FileName; args = $Arguments })
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        } `
        -RemovePath {
            param($Path)
            throw "RemoveInstalled without RemoveData must not remove paths: $Path"
        }

    $result.action | Should -Be 'RemoveInstalled'
    $result.ok | Should -BeTrue
    $result.removed_paths | Should -BeNullOrEmpty
    ($calls | Where-Object { $_.args -contains 'stop' }).Count | Should -Be 1
    ($calls | Where-Object { $_.op -eq 'service-stop-wait' }).Count | Should -Be 1
    ($calls | Where-Object { $_.args -contains 'uninstall' }).Count | Should -Be 1
}
```

- [ ] **Step 4: RemoveInstalled -RemoveData invoke 실패 테스트를 추가한다**

같은 파일에 다음 테스트를 추가한다.

```powershell
It 'removes only ProgramData paths for MSI RemoveInstalled -RemoveData' {
    $removed = [System.Collections.Generic.List[string]]::new()
    $plan = New-PcvDesktopNodeProductPlan `
        -Action RemoveInstalled `
        -SourceRoot $repoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -WinSwPath $winSwPath `
        -RemoveData

    $result = Invoke-PcvDesktopNodeProductAction `
        -Plan $plan `
        -InvokeProcess {
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        } `
        -RemovePath {
            param($Path)
            $removed.Add($Path)
            [ordered]@{ path = $Path; removed = $true }
        } `
        -GrantAdministratorsFullControl {
            param($Path)
            [ordered]@{ ok = $true; path = $Path }
        }

    $result.action | Should -Be 'RemoveInstalled'
    $result.ok | Should -BeTrue
    $removed | Should -Contain (Join-Path $dataRoot 'api-token.txt')
    $removed | Should -Contain (Join-Path $dataRoot 'jobs.json')
    $removed | Should -Contain (Join-Path $dataRoot 'events.jsonl')
    $removed | Should -Contain (Join-Path $dataRoot 'install.jsonl')
    $removed | Should -Contain (Join-Path $dataRoot 'diagnostics')
    $removed | Should -Not -Contain $productRoot
}
```

- [ ] **Step 5: 실패를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected:

```text
Failed
Unsupported product action: ConfigureInstalled
```

- [ ] **Step 6: MSI installed service configure helper를 구현한다**

`packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`에 `Invoke-PcvDesktopNodeProductAction`보다 앞쪽에 다음 helper를 추가한다.

```powershell
function Invoke-PcvInstalledServiceConfigure {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [hashtable]$Plan,

        [Parameter(Mandatory)]
        [scriptblock]$InvokeProcess,

        [Parameter(Mandatory)]
        [scriptblock]$PrepareTokenFile,

        [Parameter(Mandatory)]
        [scriptblock]$TestHealth,

        [switch]$Repair
    )

    $steps = [System.Collections.Generic.List[object]]::new()

    if ($Repair) {
        $stop = & $InvokeProcess -FileName $Plan.service.executable_path -Arguments @('stop')
        $steps.Add([ordered]@{ op = 'service.stop'; result = $stop })
        if ($stop.exit_code -ne 0 -and $stop.exit_code -notin @(1060, 1062)) {
            throw "WinSW stop failed with exit code $($stop.exit_code): $($stop.stderr)"
        }

        $wait = Wait-PcvWinSwServiceStopped -WinSwPath $Plan.service.executable_path -InvokeProcess $InvokeProcess
        $steps.Add([ordered]@{ op = 'service-stop-wait'; result = $wait })
    }

    $token = & $PrepareTokenFile -Path $Plan.token_file
    $steps.Add([ordered]@{ op = 'prepare-token'; result = $token })

    $install = & $InvokeProcess -FileName $Plan.service.executable_path -Arguments @('install')
    $steps.Add([ordered]@{ op = 'service.install'; result = $install })
    if ($install.exit_code -ne 0 -and $install.exit_code -ne 1073) {
        throw "WinSW install failed with exit code $($install.exit_code): $($install.stderr)"
    }

    $start = & $InvokeProcess -FileName $Plan.service.executable_path -Arguments @('start')
    $steps.Add([ordered]@{ op = 'service.start'; result = $start })
    if ($start.exit_code -ne 0 -and $start.exit_code -ne 1056) {
        throw "WinSW start failed with exit code $($start.exit_code): $($start.stderr)"
    }

    $health = & $TestHealth -Prefix $Plan.local_api.prefix
    $steps.Add([ordered]@{ op = 'health'; result = $health })
    if (-not $health.ok) {
        throw 'Desktop Node health check failed after MSI installed service configuration'
    }

    return $steps.ToArray()
}
```

- [ ] **Step 7: MSI installed service remove helper를 구현한다**

같은 module에 다음 helper를 추가한다.

```powershell
function Invoke-PcvInstalledServiceRemove {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [hashtable]$Plan,

        [Parameter(Mandatory)]
        [scriptblock]$InvokeProcess,

        [Parameter(Mandatory)]
        [scriptblock]$RemovePath,

        [Parameter(Mandatory)]
        [scriptblock]$GrantAdministratorsFullControl
    )

    $steps = [System.Collections.Generic.List[object]]::new()
    $removed = [System.Collections.Generic.List[object]]::new()

    $stop = & $InvokeProcess -FileName $Plan.service.executable_path -Arguments @('stop')
    $steps.Add([ordered]@{ op = 'service.stop'; result = $stop })
    if ($stop.exit_code -ne 0 -and $stop.exit_code -notin @(1060, 1062)) {
        throw "WinSW stop failed with exit code $($stop.exit_code): $($stop.stderr)"
    }

    $wait = Wait-PcvWinSwServiceStopped -WinSwPath $Plan.service.executable_path -InvokeProcess $InvokeProcess
    $steps.Add([ordered]@{ op = 'service-stop-wait'; result = $wait })

    $uninstall = & $InvokeProcess -FileName $Plan.service.executable_path -Arguments @('uninstall')
    $steps.Add([ordered]@{ op = 'service.uninstall'; result = $uninstall })
    if ($uninstall.exit_code -ne 0 -and $uninstall.exit_code -ne 1060) {
        throw "WinSW uninstall failed with exit code $($uninstall.exit_code): $($uninstall.stderr)"
    }

    foreach ($path in $Plan.delete_paths) {
        if ($path -eq $Plan.release.product_root) {
            throw "RemoveInstalled must not remove MSI-owned product root: $path"
        }

        if ($path -eq $Plan.token_file) {
            $grant = & $GrantAdministratorsFullControl -Path $path
            $steps.Add([ordered]@{ op = 'token-acl-repair'; result = $grant })
        }

        $removed.Add((& $RemovePath -Path $path))
    }

    return [ordered]@{
        steps = $steps.ToArray()
        removed_paths = $removed.ToArray()
    }
}
```

- [ ] **Step 8: Invoke-PcvDesktopNodeProductAction 분기를 연결한다**

`Invoke-PcvDesktopNodeProductAction`의 action 분기에서 unsupported branch보다 앞에 다음 분기를 추가한다.

```powershell
if ($Plan.action -eq 'ConfigureInstalled') {
    $steps = Invoke-PcvInstalledServiceConfigure `
        -Plan $Plan `
        -InvokeProcess $InvokeProcess `
        -PrepareTokenFile $PrepareTokenFile `
        -TestHealth $TestHealth

    return [ordered]@{
        ok = $true
        action = $Plan.action
        steps = $steps
        service = $Plan.service
        local_api = $Plan.local_api
    }
}

if ($Plan.action -eq 'RepairInstalled') {
    $steps = Invoke-PcvInstalledServiceConfigure `
        -Plan $Plan `
        -InvokeProcess $InvokeProcess `
        -PrepareTokenFile $PrepareTokenFile `
        -TestHealth $TestHealth `
        -Repair

    return [ordered]@{
        ok = $true
        action = $Plan.action
        steps = $steps
        service = $Plan.service
        local_api = $Plan.local_api
    }
}

if ($Plan.action -eq 'RemoveInstalled') {
    $removed = Invoke-PcvInstalledServiceRemove `
        -Plan $Plan `
        -InvokeProcess $InvokeProcess `
        -RemovePath $RemovePath `
        -GrantAdministratorsFullControl $GrantAdministratorsFullControl

    return [ordered]@{
        ok = $true
        action = $Plan.action
        steps = $removed.steps
        removed_paths = $removed.removed_paths
        remove_data = $Plan.remove_data
    }
}
```

- [ ] **Step 9: invoke tests 통과를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 10: packaging suite 통과를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 11: Task 2를 커밋한다**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "Add Desktop Node MSI installed action flows"
```

---

### Task 3: Installer Static Source Contract

**Files:**

- Create: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`
- Create: `packaging/windows-desktop-node/installer/PureCVisorDesktopNode.wixproj`
- Create: `packaging/windows-desktop-node/installer/Product.wxs`
- Create: `packaging/windows-desktop-node/installer/ProductActions.wxs`

- [ ] **Step 1: WiX source 실패 테스트를 추가한다**

`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`를 생성한다.

```powershell
BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

Describe 'Desktop Node WiX source contract' {
    It 'defines a per-machine MSI product with a fixed UpgradeCode' {
        $product = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $product | Should -Match '<Package'
        $product | Should -Match 'Name="PureCVisor Desktop Node"'
        $product | Should -Match 'Manufacturer="PureCVisor"'
        $product | Should -Match 'UpgradeCode="\{[0-9A-Fa-f-]{36}\}"'
        $product | Should -Match 'Scope="perMachine"'
        $product | Should -Match 'ProgramFilesFolder'
        $product | Should -Match 'PureCVisor'
        $product | Should -Match 'DesktopNode'
    }

    It 'keeps MSI file ownership separate from service configuration actions' {
        $product = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $product | Should -Match 'ComponentGroupRef Id="DesktopNodePayloadComponents"'
        $product | Should -Match 'ComponentGroupRef Id="DesktopNodeProductWrapperComponents"'
        $product | Should -Match 'CustomActionRef Id="ConfigureInstalled"'
        $product | Should -Match 'CustomActionRef Id="RepairInstalled"'
        $product | Should -Match 'CustomActionRef Id="RemoveInstalled"'
    }

    It 'maps install repair uninstall and remove-data custom actions without raw token properties' {
        $actions = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'ProductActions.wxs')

        $actions | Should -Match 'Id="ConfigureInstalled"'
        $actions | Should -Match 'Id="RepairInstalled"'
        $actions | Should -Match 'Id="RemoveInstalled"'
        $actions | Should -Match 'REMOVE_DATA'
        $actions | Should -Match 'ConfigureInstalledData'
        $actions | Should -Match 'RepairInstalledData'
        $actions | Should -Match 'RemoveInstalledData'
        $actions | Should -Not -Match 'ApiToken='
        $actions | Should -Not -Match 'API_TOKEN'
    }

    It 'includes all WiX source files in the project' {
        $project = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'PureCVisorDesktopNode.wixproj')

        $project | Should -Match '<Project'
        $project | Should -Match 'Product.wxs'
        $project | Should -Match 'ProductActions.wxs'
        $project | Should -Match 'WixToolset.Sdk'
    }
}
```

- [ ] **Step 2: 실패를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1' -Output Detailed"
```

Expected:

```text
Failed
Could not find file
```

- [ ] **Step 3: WiX project file을 생성한다**

`packaging/windows-desktop-node/installer/PureCVisorDesktopNode.wixproj`를 생성한다.

```xml
<Project Sdk="WixToolset.Sdk/5.0.2">
  <PropertyGroup>
    <OutputName>PureCVisorDesktopNode</OutputName>
    <OutputType>Package</OutputType>
    <Platform>x64</Platform>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Product.wxs" />
    <Compile Include="ProductActions.wxs" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Product.wxs를 생성한다**

`packaging/windows-desktop-node/installer/Product.wxs`를 생성한다.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package
    Name="PureCVisor Desktop Node"
    Manufacturer="PureCVisor"
    Version="$(var.ProductVersion)"
    UpgradeCode="{D85593B0-1A5A-4A96-B28B-AB3FE4B96E43}"
    Scope="perMachine">

    <MajorUpgrade DowngradeErrorMessage="A newer PureCVisor Desktop Node version is already installed." />
    <MediaTemplate EmbedCab="yes" />

    <StandardDirectory Id="ProgramFilesFolder">
      <Directory Id="PURECVISORFOLDER" Name="PureCVisor">
        <Directory Id="INSTALLFOLDER" Name="DesktopNode" />
      </Directory>
    </StandardDirectory>

    <Feature Id="MainFeature" Title="PureCVisor Desktop Node" Level="1">
      <ComponentGroupRef Id="DesktopNodePayloadComponents" />
      <ComponentGroupRef Id="DesktopNodeProductWrapperComponents" />
    </Feature>

    <CustomActionRef Id="ConfigureInstalled" />
    <CustomActionRef Id="RepairInstalled" />
    <CustomActionRef Id="RemoveInstalled" />
  </Package>

  <Fragment>
    <ComponentGroup Id="DesktopNodePayloadComponents">
      <Component Id="DesktopNodeManifestComponent" Directory="INSTALLFOLDER" Guid="{C6C8C802-844D-4DF2-9E1A-F9983E74DE6A}">
        <File Id="DesktopNodeManifest" Source="$(var.PayloadRoot)\product-manifest.json" KeyPath="yes" />
      </Component>
      <Component Id="DesktopNodeWinSwComponent" Directory="INSTALLFOLDER" Guid="{96B0EE9D-3515-4C4E-B29C-2A8754AFA09C}">
        <File Id="DesktopNodeWinSw" Source="$(var.PayloadRoot)\PureCVisorDesktopNode.exe" KeyPath="yes" />
      </Component>
    </ComponentGroup>

    <ComponentGroup Id="DesktopNodeProductWrapperComponents">
      <Component Id="DesktopNodeProductEntryPointComponent" Directory="INSTALLFOLDER" Guid="{9D5F782B-09FB-4F9E-AFE5-B6D9D6E69C67}">
        <File Id="DesktopNodeProductEntryPoint" Source="$(var.PayloadRoot)\Invoke-PcvDesktopNodeProduct.ps1" KeyPath="yes" />
      </Component>
      <Component Id="DesktopNodeProductModuleComponent" Directory="INSTALLFOLDER" Guid="{E935B139-28C5-4E40-9245-C136E0F0DA67}">
        <File Id="DesktopNodeProductModule" Source="$(var.PayloadRoot)\PcvDesktopNodeProduct.psm1" KeyPath="yes" />
      </Component>
    </ComponentGroup>
  </Fragment>
</Wix>
```

- [ ] **Step 5: ProductActions.wxs를 생성한다**

`packaging/windows-desktop-node/installer/ProductActions.wxs`를 생성한다.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Fragment>
    <Property Id="POWERSHELLEXE" Value="powershell.exe" />
    <Property Id="DESKTOP_NODE_DATA_ROOT" Value="[CommonAppDataFolder]PureCVisor\desktop-node" />
    <Property Id="REMOVE_DATA" Secure="yes" />

    <SetProperty
      Id="ConfigureInstalledData"
      Value="Action=ConfigureInstalled;ProductRoot=[INSTALLFOLDER];DataRoot=[DESKTOP_NODE_DATA_ROOT];WinSwPath=[INSTALLFOLDER]PureCVisorDesktopNode.exe;LogPath=[DESKTOP_NODE_DATA_ROOT]\install.jsonl"
      Before="ConfigureInstalled"
      Sequence="execute"
      Condition="NOT Installed" />

    <CustomAction
      Id="ConfigureInstalled"
      Property="ConfigureInstalledData"
      Execute="deferred"
      Impersonate="no"
      Return="check"
      ExeCommand="&quot;[POWERSHELLEXE]&quot; -NoProfile -ExecutionPolicy Bypass -File &quot;[INSTALLFOLDER]Invoke-PcvDesktopNodeProduct.ps1&quot; -Action ConfigureInstalled -ProductRoot &quot;[INSTALLFOLDER]&quot; -DataRoot &quot;[DESKTOP_NODE_DATA_ROOT]&quot; -WinSwPath &quot;[INSTALLFOLDER]PureCVisorDesktopNode.exe&quot;" />

    <SetProperty
      Id="RepairInstalledData"
      Value="Action=RepairInstalled;ProductRoot=[INSTALLFOLDER];DataRoot=[DESKTOP_NODE_DATA_ROOT];WinSwPath=[INSTALLFOLDER]PureCVisorDesktopNode.exe;LogPath=[DESKTOP_NODE_DATA_ROOT]\install.jsonl"
      Before="RepairInstalled"
      Sequence="execute"
      Condition="Installed AND NOT REMOVE~=&quot;ALL&quot;" />

    <CustomAction
      Id="RepairInstalled"
      Property="RepairInstalledData"
      Execute="deferred"
      Impersonate="no"
      Return="check"
      ExeCommand="&quot;[POWERSHELLEXE]&quot; -NoProfile -ExecutionPolicy Bypass -File &quot;[INSTALLFOLDER]Invoke-PcvDesktopNodeProduct.ps1&quot; -Action RepairInstalled -ProductRoot &quot;[INSTALLFOLDER]&quot; -DataRoot &quot;[DESKTOP_NODE_DATA_ROOT]&quot; -WinSwPath &quot;[INSTALLFOLDER]PureCVisorDesktopNode.exe&quot;" />

    <SetProperty
      Id="RemoveInstalledData"
      Value="Action=RemoveInstalled;ProductRoot=[INSTALLFOLDER];DataRoot=[DESKTOP_NODE_DATA_ROOT];WinSwPath=[INSTALLFOLDER]PureCVisorDesktopNode.exe;RemoveData=[REMOVE_DATA];LogPath=[DESKTOP_NODE_DATA_ROOT]\install.jsonl"
      Before="RemoveInstalled"
      Sequence="execute"
      Condition="REMOVE~=&quot;ALL&quot;" />

    <CustomAction
      Id="RemoveInstalled"
      Property="RemoveInstalledData"
      Execute="deferred"
      Impersonate="no"
      Return="check"
      ExeCommand="&quot;[POWERSHELLEXE]&quot; -NoProfile -ExecutionPolicy Bypass -File &quot;[INSTALLFOLDER]Invoke-PcvDesktopNodeProduct.ps1&quot; -Action RemoveInstalled -ProductRoot &quot;[INSTALLFOLDER]&quot; -DataRoot &quot;[DESKTOP_NODE_DATA_ROOT]&quot; -WinSwPath &quot;[INSTALLFOLDER]PureCVisorDesktopNode.exe&quot; [REMOVE_DATA]" />

    <InstallExecuteSequence>
      <Custom Action="ConfigureInstalled" After="InstallFiles" Condition="NOT Installed" />
      <Custom Action="RepairInstalled" After="InstallFiles" Condition="Installed AND NOT REMOVE~=&quot;ALL&quot;" />
      <Custom Action="RemoveInstalled" Before="RemoveFiles" Condition="REMOVE~=&quot;ALL&quot;" />
    </InstallExecuteSequence>
  </Fragment>
</Wix>
```

- [ ] **Step 6: WiX source static tests 통과를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 7: Task 3을 커밋한다**

```powershell
git add packaging/windows-desktop-node/installer/PureCVisorDesktopNode.wixproj packaging/windows-desktop-node/installer/Product.wxs packaging/windows-desktop-node/installer/ProductActions.wxs packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1
git commit -m "Add Desktop Node WiX installer source contract"
```

---

### Task 4: Installer Build, Signing, and Provenance Contract

**Files:**

- Create: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`
- Create: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1`
- Create: `packaging/windows-desktop-node/installer/build.ps1`
- Create: `packaging/windows-desktop-node/installer/installer-provenance.schema.json`

- [ ] **Step 1: build plan 실패 테스트를 추가한다**

`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`를 생성한다.

```powershell
BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:BuildScript = Join-Path $script:InstallerRoot 'build.ps1'
}

Describe 'Desktop Node installer build plan' {
    It 'exposes explicit version winsw output signing and dry-run parameters' {
        $scriptText = Get-Content -Raw -LiteralPath $script:BuildScript

        $scriptText | Should -Match '\[string\]\$Version'
        $scriptText | Should -Match '\[string\]\$WinSwPath'
        $scriptText | Should -Match '\[string\]\$OutputRoot'
        $scriptText | Should -Match 'RequireSigned'
        $scriptText | Should -Match 'AllowUnsignedDev'
        $scriptText | Should -Match 'SigningTrustModel'
        $scriptText | Should -Match '\[switch\]\$DryRun'
    }

    It 'returns structured JSON when release signing input is missing' {
        $winsw = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winsw -Value 'fake-winsw' -NoNewline

        $output = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
            -Version '0.14.0' `
            -WinSwPath $winsw `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode RequireSigned `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 1
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_SIGNING_REQUIRED'
    }

    It 'emits a dry-run plan without requiring WiX for unsigned developer builds' {
        $winsw = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winsw -Value 'fake-winsw' -NoNewline

        $output = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
            -Version '0.14.0-dev' `
            -WinSwPath $winsw `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $output.plan.product_name | Should -Be 'PureCVisor Desktop Node'
        $output.plan.version | Should -Be '0.14.0-dev'
        $output.plan.signing_mode | Should -Be 'AllowUnsignedDev'
        $output.plan.winsw_sha256 | Should -Match '^[0-9A-Fa-f]{64}$'
    }
}
```

- [ ] **Step 2: signing/provenance 실패 테스트를 추가한다**

`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1`를 생성한다.

```powershell
BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:BuildScript = Join-Path $script:InstallerRoot 'build.ps1'
    $script:SchemaPath = Join-Path $script:InstallerRoot 'installer-provenance.schema.json'
}

Describe 'Desktop Node installer signing and provenance contract' {
    It 'defines required provenance fields' {
        $schema = Get-Content -Raw -LiteralPath $script:SchemaPath | ConvertFrom-Json

        $schema.'$schema' | Should -Be 'https://json-schema.org/draft/2020-12/schema'
        $schema.required | Should -Contain 'schema_version'
        $schema.required | Should -Contain 'product'
        $schema.required | Should -Contain 'git_commit'
        $schema.required | Should -Contain 'build_utc'
        $schema.required | Should -Contain 'wix'
        $schema.required | Should -Contain 'msi'
        $schema.required | Should -Contain 'payload'
        $schema.required | Should -Contain 'winsw'
        $schema.required | Should -Contain 'signing_mode'
        $schema.required | Should -Contain 'host'
    }

    It 'accepts release signing input without writing certificate secrets into dry-run output' {
        $winsw = Join-Path $TestDrive 'winsw.exe'
        $signtool = Join-Path $TestDrive 'signtool.exe'
        Set-Content -LiteralPath $winsw -Value 'fake-winsw' -NoNewline
        Set-Content -LiteralPath $signtool -Value 'fake-signtool' -NoNewline

        $json = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
            -Version '0.14.0' `
            -WinSwPath $winsw `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode RequireSigned `
            -SigningTrustModel LocalTest `
            -SignToolPath $signtool `
            -CertificateThumbprint '00112233445566778899AABBCCDDEEFF00112233' `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -DryRun

        $LASTEXITCODE | Should -Be 0
        $json | Should -Not -Match '00112233445566778899AABBCCDDEEFF00112233'
        $json | Should -Not -Match 'pfx'

        $output = $json | ConvertFrom-Json
        $output.ok | Should -BeTrue
        $output.plan.signing_mode | Should -Be 'RequireSigned'
        $output.plan.signing_inputs.has_signtool | Should -BeTrue
        $output.plan.signing_inputs.has_certificate | Should -BeTrue
        $output.plan.signing_inputs.has_timestamp | Should -BeTrue
    }
}
```

- [ ] **Step 3: 실패를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected:

```text
Failed
Could not find file
```

- [ ] **Step 4: provenance schema를 생성한다**

`packaging/windows-desktop-node/installer/installer-provenance.schema.json`를 생성한다.

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://[redacted-private-endpoint]/schemas/desktop-node-installer-provenance.schema.json",
  "title": "PureCVisor Desktop Node Installer Provenance",
  "type": "object",
  "required": [
    "schema_version",
    "product",
    "git_commit",
    "build_utc",
    "wix",
    "msi",
    "payload",
    "winsw",
    "signing_mode",
    "host"
  ],
  "properties": {
    "schema_version": { "type": "string", "const": "1" },
    "product": {
      "type": "object",
      "required": ["name", "version"],
      "properties": {
        "name": { "type": "string" },
        "version": { "type": "string" }
      }
    },
    "git_commit": { "type": "string" },
    "build_utc": { "type": "string" },
    "wix": {
      "type": "object",
      "required": ["version", "source_project"],
      "properties": {
        "version": { "type": "string" },
        "source_project": { "type": "string" }
      }
    },
    "msi": {
      "type": "object",
      "required": ["path", "sha256", "signed"],
      "properties": {
        "path": { "type": "string" },
        "sha256": { "type": "string" },
        "signed": { "type": "boolean" }
      }
    },
    "payload": {
      "type": "object",
      "required": ["root", "file_count", "aggregate_sha256", "product_wrapper_sha256"],
      "properties": {
        "root": { "type": "string" },
        "file_count": { "type": "integer" },
        "aggregate_sha256": { "type": "string" },
        "product_wrapper_sha256": { "type": "string" }
      }
    },
    "winsw": {
      "type": "object",
      "required": ["source_path", "release_label", "sha256", "signature_status"],
      "properties": {
        "source_path": { "type": "string" },
        "release_label": { "type": "string" },
        "sha256": { "type": "string" },
        "signature_status": { "type": "string" }
      }
    },
    "signing_mode": { "type": "string", "enum": ["RequireSigned", "AllowUnsignedDev"] },
    "host": {
      "type": "object",
      "required": ["os", "powershell"],
      "properties": {
        "os": { "type": "string" },
        "powershell": { "type": "string" }
      }
    }
  }
}
```

- [ ] **Step 5: build.ps1을 생성한다**

`packaging/windows-desktop-node/installer/build.ps1`를 생성한다.

```powershell
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Version,

    [Parameter(Mandatory)]
    [string]$WinSwPath,

    [Parameter(Mandatory)]
    [string]$OutputRoot,

    [ValidateSet('RequireSigned', 'AllowUnsignedDev')]
    [string]$SigningMode = 'RequireSigned',

    [string]$SignToolPath,
    [string]$CertificateThumbprint,
    [string]$CertificatePath,
    [string]$TimestampUrl,
    [string]$WixPath = 'wix',
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-PcvJsonAndExit {
    param(
        [Parameter(Mandatory)]
        [object]$Payload,

        [Parameter(Mandatory)]
        [int]$ExitCode
    )

    $Payload | ConvertTo-Json -Depth 8 -Compress
    exit $ExitCode
}

function New-PcvInstallerError {
    param(
        [Parameter(Mandatory)]
        [string]$Code,

        [Parameter(Mandatory)]
        [string]$Message
    )

    [ordered]@{
        ok = $false
        error = [ordered]@{
            code = $Code
            message = $Message
        }
    }
}

function Get-PcvFileSha256 {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-PcvGitCommit {
    try {
        $value = (& git rev-parse HEAD 2>$null)
        if ($LASTEXITCODE -eq 0 -and $value) {
            return ($value | Select-Object -First 1)
        }
    } catch {
    }

    return 'unknown'
}

function Get-PcvWixVersion {
    param(
        [Parameter(Mandatory)]
        [string]$Command
    )

    try {
        $value = (& $Command --version 2>$null)
        if ($LASTEXITCODE -eq 0 -and $value) {
            return ($value | Select-Object -First 1)
        }
    } catch {
    }

    return 'not-detected'
}

function Test-PcvHasSigningCertificateInput {
    param(
        [string]$Thumbprint,
        [string]$Path
    )

    -not [string]::IsNullOrWhiteSpace($Thumbprint) -or -not [string]::IsNullOrWhiteSpace($Path)
}

$resolvedWinSw = Resolve-Path -LiteralPath $WinSwPath -ErrorAction Stop
$resolvedWinSwPath = $resolvedWinSw.Path

if ($SigningMode -eq 'RequireSigned') {
    $hasSignTool = -not [string]::IsNullOrWhiteSpace($SignToolPath)
    $hasCertificate = Test-PcvHasSigningCertificateInput -Thumbprint $CertificateThumbprint -Path $CertificatePath
    $hasTimestamp = -not [string]::IsNullOrWhiteSpace($TimestampUrl)

    if (-not ($hasSignTool -and $hasCertificate -and $hasTimestamp)) {
        Write-PcvJsonAndExit `
            -ExitCode 1 `
            -Payload (New-PcvInstallerError `
                -Code 'PCV_INSTALLER_SIGNING_REQUIRED' `
                -Message 'RequireSigned builds require SignToolPath, certificate input, and TimestampUrl.')
    }
}

$installerRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $installerRoot '..\..\..')).Path
$outputRootFull = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputRoot))
$payloadRoot = Join-Path $outputRootFull 'payload'
$msiPath = Join-Path $outputRootFull "PureCVisorDesktopNode-$Version.msi"
$projectPath = Join-Path $installerRoot 'PureCVisorDesktopNode.wixproj'
$winswHash = Get-PcvFileSha256 -Path $resolvedWinSwPath
$modulePath = Join-Path $repoRoot 'packaging\windows-desktop-node\PcvDesktopNodeProduct.psm1'
$moduleHash = Get-PcvFileSha256 -Path $modulePath

$plan = [ordered]@{
    product_name = 'PureCVisor Desktop Node'
    version = $Version
    output_root = $outputRootFull
    payload_root = $payloadRoot
    msi_path = $msiPath
    wix_project = $projectPath
    wix_version = Get-PcvWixVersion -Command $WixPath
    signing_mode = $SigningMode
    signing_inputs = [ordered]@{
        has_signtool = -not [string]::IsNullOrWhiteSpace($SignToolPath)
        has_certificate = Test-PcvHasSigningCertificateInput -Thumbprint $CertificateThumbprint -Path $CertificatePath
        has_timestamp = -not [string]::IsNullOrWhiteSpace($TimestampUrl)
    }
    winsw_path = $resolvedWinSwPath
    winsw_sha256 = $winswHash
    product_wrapper_sha256 = $moduleHash
}

if ($DryRun) {
    Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
        ok = $true
        dry_run = $true
        plan = $plan
    })
}

$wixCommand = Get-Command -Name $WixPath -ErrorAction SilentlyContinue
if (-not $wixCommand) {
    Write-PcvJsonAndExit `
        -ExitCode 1 `
        -Payload (New-PcvInstallerError -Code 'PCV_INSTALLER_WIX_NOT_FOUND' -Message 'WiX CLI was not found. Install WiX or pass -WixPath.')
}

$wixExecutable = $wixCommand.Source
if ([string]::IsNullOrWhiteSpace($wixExecutable)) {
    $wixExecutable = $wixCommand.Path
}

New-Item -ItemType Directory -Path $payloadRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repoRoot 'packaging\windows-desktop-node\Invoke-PcvDesktopNodeProduct.ps1') -Destination (Join-Path $payloadRoot 'Invoke-PcvDesktopNodeProduct.ps1') -Force
Copy-Item -LiteralPath $modulePath -Destination (Join-Path $payloadRoot 'PcvDesktopNodeProduct.psm1') -Force
Copy-Item -LiteralPath $resolvedWinSwPath -Destination (Join-Path $payloadRoot 'PureCVisorDesktopNode.exe') -Force
Set-Content -LiteralPath (Join-Path $payloadRoot 'product-manifest.json') -Encoding UTF8 -Value (@{
    product = 'PureCVisor Desktop Node'
    version = $Version
    built_utc = (Get-Date).ToUniversalTime().ToString('o')
} | ConvertTo-Json -Depth 4)

$wixSourceFiles = @(
    (Join-Path $installerRoot 'Product.wxs'),
    (Join-Path $installerRoot 'ProductActions.wxs')
)

& $wixExecutable build @wixSourceFiles `
    -define "ProductVersion=$Version" `
    -define "PayloadRoot=$payloadRoot" `
    -out $msiPath

if ($LASTEXITCODE -ne 0) {
    Write-PcvJsonAndExit `
        -ExitCode $LASTEXITCODE `
        -Payload (New-PcvInstallerError -Code 'PCV_INSTALLER_WIX_BUILD_FAILED' -Message 'WiX build failed.')
}

if ($SigningMode -eq 'RequireSigned') {
    $signArgs = @('sign', '/fd', 'SHA256', '/tr', $TimestampUrl, '/td', 'SHA256')
    if (-not [string]::IsNullOrWhiteSpace($CertificateThumbprint)) {
        $signArgs += @('/sha1', $CertificateThumbprint)
    } else {
        $signArgs += @('/f', $CertificatePath)
    }
    $signArgs += $msiPath
    & $SignToolPath @signArgs

    if ($LASTEXITCODE -ne 0) {
        Write-PcvJsonAndExit `
            -ExitCode $LASTEXITCODE `
            -Payload (New-PcvInstallerError -Code 'PCV_INSTALLER_SIGNING_FAILED' -Message 'MSI signing failed.')
    }
}

$payloadFiles = Get-ChildItem -LiteralPath $payloadRoot -File -Recurse
$aggregateInput = ($payloadFiles | Sort-Object FullName | ForEach-Object {
    "$(Get-PcvFileSha256 -Path $_.FullName)  $($_.FullName.Substring($payloadRoot.Length).TrimStart('\'))"
}) -join "`n"
$aggregateHash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($aggregateInput))).Replace('-', '').ToLowerInvariant()

$provenance = [ordered]@{
    schema_version = '1'
    product = [ordered]@{
        name = 'PureCVisor Desktop Node'
        version = $Version
    }
    git_commit = Get-PcvGitCommit
    build_utc = (Get-Date).ToUniversalTime().ToString('o')
    wix = [ordered]@{
        version = Get-PcvWixVersion -Command $WixPath
        source_project = $projectPath
    }
    msi = [ordered]@{
        path = $msiPath
        sha256 = Get-PcvFileSha256 -Path $msiPath
        signed = ($SigningMode -eq 'RequireSigned')
    }
    payload = [ordered]@{
        root = $payloadRoot
        file_count = $payloadFiles.Count
        aggregate_sha256 = $aggregateHash
        product_wrapper_sha256 = $moduleHash
    }
    winsw = [ordered]@{
        source_path = $resolvedWinSwPath
        release_label = 'external'
        sha256 = $winswHash
        signature_status = 'not-verified-by-phase14-script'
    }
    signing_mode = $SigningMode
    host = [ordered]@{
        os = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        powershell = $PSVersionTable.PSVersion.ToString()
    }
}

$provenancePath = Join-Path $outputRootFull 'purecvisor-desktop-node-installer-provenance.json'
$provenance | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $provenancePath -Encoding UTF8

Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
    ok = $true
    msi_path = $msiPath
    provenance_path = $provenancePath
    provenance = $provenance
})
```

- [ ] **Step 6: installer plan/signing tests 통과를 확인한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 7: WiX가 있는 환경에서 unsigned dev build를 검증한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.14.0-dev -WinSwPath '<winsw.exe>' -OutputRoot artifacts/windows-desktop-node -SigningMode AllowUnsignedDev
```

Expected when WiX exists:

```text
{"ok":true,...,"provenance_path":"...purecvisor-desktop-node-installer-provenance.json",...}
```

Expected when WiX is not installed:

```text
PCV_INSTALLER_WIX_NOT_FOUND
```

If WiX is not installed, record that the Pester static/dry-run suite passed and the actual MSI build was not executed in this environment.

- [ ] **Step 8: Task 4를 커밋한다**

```powershell
git add packaging/windows-desktop-node/installer/build.ps1 packaging/windows-desktop-node/installer/installer-provenance.schema.json packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1
git commit -m "Add Desktop Node installer build provenance contract"
```

---

### Task 5: Installer Documentation and Verification Policy

**Files:**

- Create: `packaging/windows-desktop-node/installer/README.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `follower.md`

- [ ] **Step 1: installer README를 작성한다**

`packaging/windows-desktop-node/installer/README.md`를 생성한다.

```markdown
# PureCVisor Desktop Node Installer

이 디렉터리는 Desktop Node Phase 14 WiX MSI-first installer 경계를 담는다.

## 책임 경계

- MSI는 `%ProgramFiles%\PureCVisor\DesktopNode` 파일 설치, repair, 제거를 소유한다.
- Product wrapper는 service install/start/stop/uninstall, token file 준비, health check, diagnostics를 소유한다.
- 기본 uninstall은 `%ProgramData%\PureCVisor\desktop-node`를 보존한다.
- `REMOVE_DATA=1` uninstall만 token, job store, event log, install log, diagnostics를 삭제한다.

## 개발자 dry-run

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.14.0-dev `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode AllowUnsignedDev `
  -DryRun
```

## 개발자 unsigned MSI build

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.14.0-dev `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode AllowUnsignedDev
```

## Release signing 입력

Release build는 `RequireSigned`가 기본이다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.14.0 `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node `
  -SigningMode RequireSigned `
  -SigningTrustModel InternalEnterprise `
  -SignToolPath '<signtool.exe>' `
  -CertificateThumbprint '<thumbprint>' `
  -TimestampUrl '<timestamp-url>'
```

인증서 private key, PFX password, API token 값은 repo와 provenance manifest에 기록하지 않는다.

## 관리자 smoke

```powershell
$msi = 'artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi'
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install.log
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
msiexec /i $msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log
msiexec /x $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall.log
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install-remove-data.log
msiexec /x $msi REMOVE_DATA=1 REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall-remove-data.log
```

관리자 smoke는 service started, runtime policy HTTP 200, loopback root HTTP 200, repair 후 data 보존, 기본 uninstall 후 data 보존, `REMOVE_DATA=1` 후 token/job/event/install/diagnostics 제거를 확인한다.

## 기본 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```
```

- [ ] **Step 2: product wrapper README에 Phase 14 entrypoint를 추가한다**

`packaging/windows-desktop-node/README.md`에 다음 섹션을 추가한다.

```markdown
## Phase 14 WiX MSI installer

Phase 14는 `packaging/windows-desktop-node/installer/` 아래 WiX MSI-first installer 산출물을 추가한다.

- MSI는 Program Files 제품 파일 설치, repair, 제거를 소유한다.
- Product wrapper는 service/data configuration만 소유한다.
- MSI custom action은 `ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled` action을 호출한다.
- 기존 `Install`/`Uninstall` action은 standalone 관리자 smoke와 개발자 CLI용으로 유지한다.

기본 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```
```

- [ ] **Step 3: verification policy에 Phase 14 행을 추가한다**

`docs/DEVELOPMENT_VERIFICATION_POLICY.md`의 Desktop Node 검증 표에 다음 행을 추가한다.

```markdown
| Desktop Node Phase 14 signed installer와 repair/uninstall UX 변경 | `packaging/windows-desktop-node/installer/tests`, `packaging/windows-desktop-node/tests`, root boundary Pester suite 필수 | .NET SDK와 WiX CLI가 있으면 `build.ps1 -SigningMode AllowUnsignedDev` dev build 필수 | 실제 `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` smoke와 signed release build는 관리자 권한 및 signing secret 환경에서 조건부 | Single Edge 릴리스 게이트와 분리 |
```

같은 문서의 Desktop Node 설명 문단에 다음 문장을 추가한다.

```markdown
Phase 14는 WiX MSI-first installer source, signing/provenance dry-run, MSI 전용 product wrapper action을 검증한다. 기본 Pester suite는 WiX 설치 없이 통과해야 하며, 실제 MSI build와 `msiexec` smoke는 별도 관리자 opt-in 검증으로 분리한다. WiX CLI가 있는 개발 환경에서는 unsigned dev MSI build를 확인한다.
```

- [ ] **Step 4: developer index에 구현 계획 링크를 추가한다**

`docs/DEVELOPER_INDEX.md`의 먼저 볼 문서 표에서 Phase 14 설계 행 다음에 다음 행을 추가한다.

```markdown
| Desktop Node Phase 14 signed installer와 repair/uninstall UX 상태를 확인할 때 | [2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md](superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md) | WiX source/build script, MSI 전용 product wrapper action, signing/provenance, 완료 증거 |
```

문서별 용도 섹션의 Phase 14 설계 문서 아래에 다음 섹션을 추가한다.

```markdown
### [Desktop Node Phase 14 Signed Installer와 Repair/Uninstall UX 구현 계획](superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md)

- 언제 보는지: WiX source/build script, MSI 전용 product wrapper action, installer Pester suite, unsigned dev MSI build, 관리자 smoke 보류 상태를 확인할 때
- 왜 보는지: Phase 14 구현 task, 수정 파일, 테스트 명령, 완료 증거를 기록하기 때문
- 같이 봐야 하는 문서: [Desktop Node Phase 14 Signed Installer와 Repair/Uninstall UX 설계](superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md), [Desktop Node Phase 13 WinSW service wrapper 구현 계획](superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md), [Desktop Node Phase 12 제품 wrapper README](../packaging/windows-desktop-node/README.md), [DEVELOPMENT_VERIFICATION_POLICY.md](DEVELOPMENT_VERIFICATION_POLICY.md)
```

- [ ] **Step 5: roadmap과 follower 상태를 갱신한다**

`docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`의 Phase 14 표 행을 다음 행으로 교체한다.

```markdown
| Phase 14 | 기본 구현/검증 완료 | signed installer와 repair/uninstall UX를 구현한다. | WiX MSI-first source/build, MSI 전용 product wrapper action, signing/provenance, installer 문서와 검증 정책, unsigned dev MSI build | signed release build와 elevated `msiexec` smoke는 보류하고, Phase 15 secure token storage spec으로 넘어갈 수 있다. |
```

같은 파일의 Phase 14 섹션에서 `설계 문서:` 아래에 다음 `구현 계획:`을 추가한다.

```markdown
구현 계획:

- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md`
```

`follower.md`의 다음 실행 권장 순서 첫 항목을 다음으로 교체한다.

```markdown
1. Phase 14 signed release/admin smoke는 signing secret과 elevated PowerShell이 준비된 경우에만 진행한다.
   - 설계 문서: `docs/superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md`
   - 구현 계획: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md`
   - 결정: `DESKTOP_NODE_PHASE14_INSTALLER_DECISION: wix-msi-first`
   - 기본 구현, 문서 연결, Pester/static check, unsigned dev MSI build는 완료됐다.
   - signed release build와 관리자 `msiexec` smoke는 signing secret과 elevated PowerShell이 준비된 경우에만 기록한다.
   - 그 환경이 없으면 Phase 16 Event Log와 long-term diagnostics spec으로 넘어간다.
```

- [ ] **Step 6: 문서 링크와 Markdown 검증을 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected:

```text
Passed
Failed: 0
```

```text
git diff --check exits with code 0
```

- [ ] **Step 7: Task 5를 커밋한다**

```powershell
git add packaging/windows-desktop-node/installer/README.md packaging/windows-desktop-node/README.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md follower.md
git commit -m "Document Desktop Node phase 14 installer workflow"
```

---

### Task 6: End-to-End Verification and Push

**Files:**

- Verify only: no planned source changes.

- [ ] **Step 1: installer suite를 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 2: product wrapper suite를 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 3: root Desktop Node boundary suite를 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected:

```text
Passed
Failed: 0
```

- [ ] **Step 4: 영향 범위 static checks를 실행한다**

Run:

```powershell
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Expected:

```text
node --check exits with code 0
git diff --check exits with code 0
```

- [ ] **Step 5: WiX 환경이 있으면 dev MSI build를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.14.0-dev -WinSwPath '<winsw.exe>' -OutputRoot artifacts/windows-desktop-node -SigningMode AllowUnsignedDev
```

Expected when WiX exists:

```text
{"ok":true,...}
```

Expected when WiX is not installed:

```text
record "WiX CLI unavailable; Pester static/dry-run verification passed"
```

- [ ] **Step 6: 관리자 권한 smoke가 가능한 환경이면 MSI install/repair/uninstall을 실행한다**

Run from elevated PowerShell:

```powershell
$msi = 'artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi'
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install.log
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
msiexec /i $msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log
msiexec /x $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall.log
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install-remove-data.log
msiexec /x $msi REMOVE_DATA=1 REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall-remove-data.log
```

Expected:

```text
install service started
status ok true
repair preserves token jobs events diagnostics
default uninstall preserves ProgramData
REMOVE_DATA=1 removes token jobs events install log diagnostics
service and listener are absent after uninstall
```

- [ ] **Step 7: 최종 상태를 확인하고 push한다**

Run:

```powershell
git status --short
git log --oneline -6
git push
```

Expected:

```text
working tree clean
origin branch updated
```

## 완료 증거

2026-04-28 기본 검증:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'`: 17 passed, 0 failed.
- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests'`: 53 passed, 0 failed.
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests'`: 9 passed, 0 failed.
- `node --check spikes/purecvisor-desktop-node/web/app.js`: exit code 0.
- `git diff --check`: exit code 0.
- `dotnet --list-sdks`: `10.0.203 [C:\Program Files\dotnet\sdk]`.
- `wix.exe --version`: `5.0.2+aa65968c`.
- `build.ps1 -Version 0.14.0-dev -WinSwPath <fake-winsw> -SigningMode AllowUnsignedDev -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: JSON `ok=true`, unsigned MSI와 provenance manifest 생성 확인.

조건부 보류:

- Signed release build는 signing secret, timestamp URL, `signtool.exe` 입력이 필요해 실행하지 않았다.
- Elevated `msiexec` install/repair/uninstall/remove-data smoke는 관리자 실행 컨텍스트에서만 실행한다.

## Self-Review Checklist

- Spec coverage:
  - MSI-first 결정: Task 3, Task 4, Task 5.
  - MSI/product wrapper 책임 분리: Task 1, Task 2, Task 3.
  - Repair UX: Task 2, Task 5, Task 6.
  - 기본 uninstall data preservation: Task 1, Task 2, Task 5, Task 6.
  - `REMOVE_DATA=1`: Task 1, Task 2, Task 3, Task 5, Task 6.
  - Signing policy: Task 4, Task 5.
  - Provenance manifest: Task 4.
  - WiX 없는 기본 검증: Task 4, Task 6.
  - 관리자 opt-in smoke: Task 5, Task 6.
- Placeholder scan:
  - 빈 구현 지시, 미정 상태 표기, 나중에 채우라는 지시는 계획 본문에 두지 않는다.
  - WiX 미설치와 관리자 권한 부재는 실패가 아니라 조건부 검증 결과로 기록한다.
- Type consistency:
  - Product wrapper action 이름은 모든 task에서 `ConfigureInstalled`, `RepairInstalled`, `RemoveInstalled`로 통일한다.
  - MSI destructive property는 `REMOVE_DATA`로 통일한다.
  - Product wrapper CLI flag는 기존 PowerShell switch `-RemoveData`를 유지한다.
  - Provenance output 파일명은 `purecvisor-desktop-node-installer-provenance.json`으로 통일한다.
