# PureCVisor Desktop Node Post-Reboot Verification 구현 계획

> **Agent 작업자 필수 지침:** 이 계획을 단계별로 구현할 때는 `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`를 사용한다. 진행 상태는 checkbox(`- [ ]`) 문법으로 추적한다.

**목표:** Windows reboot 이후 관리자 smoke의 후속 검증 명령과 evidence 기록을 1회성 elevated scheduled task가 자동으로 실행하게 만든다.

**구조:** `packaging/windows-desktop-node/tools/` 아래에 post-reboot verification 전용 PowerShell module과 두 entrypoint script를 둔다. 기본 mode는 `LocalSystemAtStartup`이며, 사용자 profile이나 signing material이 필요한 command plan은 `CurrentUserAtLogOn` opt-in에서만 허용한다. 기본 Pester는 state/profile/task plan/runner를 injectable dependency로 검증하고, 실제 Task Scheduler 등록과 reboot는 administrator opt-in smoke로 분리한다.

**기술 기준:** PowerShell 7, Pester 5, Windows Task Scheduler cmdlets, JSON evidence file, 기존 Desktop Node product wrapper command.

---

## 파일 구조

- Create `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`
  - command profile, state file contract, redaction, task registration plan, runner 실행, JSON/Markdown evidence 작성을 소유한다.
- Create `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1`
  - reboot 전 state 생성과 1회성 scheduled task 등록을 담당하는 관리자 entrypoint다.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1`
  - scheduled task action entrypoint다. state file을 읽고 post-reboot verification을 실행한다.
- Create `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1`
  - non-admin Pester contract test다. fake process runner와 fake task registration block을 사용하며 task 등록이나 reboot를 수행하지 않는다.
- Modify `packaging/windows-desktop-node/README.md`
  - 짧은 post-reboot verification runbook과 새 설계/계획 링크를 추가한다.
- Modify `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md`
  - 선택형 post-reboot verification handoff note를 추가한다.
- Modify `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence.md`
  - 선택형 post-reboot verification handoff note를 추가한다.
- Modify `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence.md`
  - 선택형 post-reboot verification handoff note를 추가한다.

구현은 기존 Linux runtime 경계를 수정하거나 파일을 `spikes/**`, `packaging/windows-desktop-node/**` 밖으로 이동하면 안 된다.

## 구현 메모

- 수동 수정에는 `apply_patch`를 사용한다.
- 기존 한국어 문서를 수정하는 경우 AGENTS.md에 따라 한국어로 작성한다.
- 관련 없는 기존 변경은 stage하지 않는다. commit 단계에서는 정확한 `git add` path만 사용한다.
- 기본 검증에서는 실제 `Register-ScheduledTask`, `Restart-Computer`, MSI lifecycle, service mutation, Event Log registration, Firewall mutation, Hyper-V lifecycle을 실행하지 않는다.
- Test는 `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`를 직접 import한다.

## 현재 구현 상태

2026-04-30 dry-run + runner evidence slice:

- `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`가 command profile, state file contract, redaction, scheduled task action plan, dry-run initialization을 제공한다.
- `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -DryRun`은 `post-reboot-state.json`과 task plan을 만들지만 실제 Task Scheduler 등록을 호출하지 않는다.
- `-DryRun` 없는 실행은 명시적 administrator opt-in task registration 경로이며, 자동 reboot는 수행하지 않는다.
- `-ContinuationProfiles PackagingRegression` 같은 후속 profile을 state에 포함하면 post-reboot 기본 profile 성공 후 allowlist된 후속 command를 이어 실행한다.
- `packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1`는 state file을 읽어 command를 실행하고 `post-reboot-result.json`, `post-reboot-summary.md`, command별 stdout/stderr artifact, `post-reboot-complete.json`을 작성한다.
- Command 실패와 cleanup 실패는 result JSON에 기록하며 이미 수집한 evidence artifact는 유지한다.
- `post-reboot-complete.json`이 이미 있으면 command를 재실행하지 않고 cleanup만 다시 시도해 stale scheduled task 제거를 허용한다.
- `-Reboot` 요청은 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 차단한다. Script는 `Restart-Computer`를 호출하지 않는다.

2026-04-30 current-head follow-up evidence:

- evidence root: `artifacts/p1-post-reboot-verification-current-head-20260430-191839`
- git commit: `eb57f09`
- Windows boot time: `2026-04-30T07:46:15.5000000Z`
- profile: `ProductStatus`
- commands: product status exit `0`, product collect diagnostics exit `0`
- continuation profiles: `PackagingRegression`, `HyperVNonIntegration`
- continuation commands: packaging product tests exit `0`, packaging installer tests exit `0`, `git diff --check` exit `0`, Hyper-V non-integration tests exit `0`
- result: `ok = true`
- automatic reboot: not used. 이 evidence는 이미 완료된 부팅 세션에서 runner를 수동 실행한 current-head follow-up이며, `Restart-Computer`나 Task Scheduler 등록을 실행하지 않았다.

### Task 1: Command Profiles, State Contract, And Redaction

**Files:**
- Create: `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`
- Create: `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1`

- [ ] **Step 1: Write failing Pester tests for profile/state/redaction**

Create `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1` with this initial content:

```powershell
Set-StrictMode -Version Latest

Describe 'PcvPostRebootVerification profile and state contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'builds the PackagingRegression profile without host mutation commands' {
        $profile = New-PcvPostRebootCommandProfile `
            -Profile PackagingRegression `
            -RepoRoot $script:RepoRoot

        $profile.profile | Should -Be 'PackagingRegression'
        $profile.commands.Count | Should -Be 3
        $profile.commands[0].id | Should -Be 'packaging-product-tests'
        $profile.commands[0].file_name | Should -Be 'pwsh'
        ($profile.commands[0].arguments -join ' ') | Should -Match 'Invoke-Pester'
        $profile.commands[1].id | Should -Be 'packaging-installer-tests'
        $profile.commands[2].id | Should -Be 'git-diff-check'
        ($profile.commands | ConvertTo-Json -Depth 12) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
    }

    It 'builds a LocalSystemAtStartup state file contract for repo-local commands' {
        $evidenceDir = Join-Path $TestDrive 'evidence'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile PackagingRegression `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-test' `
            -PrincipalMode LocalSystemAtStartup

        $state.schema_version | Should -Be 1
        $state.phase_id | Should -Be 'phase23'
        $state.repo_root | Should -Be $script:RepoRoot
        $state.evidence_dir | Should -Be $evidenceDir
        $state.principal.mode | Should -Be 'LocalSystemAtStartup'
        $state.commands.Count | Should -Be 3
        $state.cleanup.unregister_task | Should -BeTrue
    }

    It 'rejects LocalSystemAtStartup when user profile resources are required' {
        {
            New-PcvPostRebootVerificationState `
                -PhaseId 'phase20' `
                -RepoRoot $script:RepoRoot `
                -EvidenceDir (Join-Path $TestDrive 'evidence-user-profile') `
                -Profile ProductStatus `
                -TaskName 'PureCVisorDesktopNode-PostRebootVerification-user-profile' `
                -PrincipalMode LocalSystemAtStartup `
                -RequiresUserProfile
        } | Should -Throw -ExpectedMessage '*PCV_POST_REBOOT_PRINCIPAL_NOT_ALLOWED*'
    }

    It 'redacts bearer tokens, secret keys, and known paths from text' {
        $text = @'
Authorization: Bearer abc.def.secret
{"api_token":"raw-token","nested":{"password":"pw"},"path":"C:\ProgramData\PureCVisor\desktop-node"}
D:\data\projects\codex-zone\purecvisor-desktop-node
'@
        $redacted = ConvertTo-PcvPostRebootRedactedText `
            -Text $text `
            -PathRedactions @{
                'C:\ProgramData\PureCVisor\desktop-node' = '[DATA_ROOT]'
                'D:\data\projects\codex-zone\purecvisor-desktop-node' = '[REPO_ROOT]'
            }

        $redacted | Should -Match 'Bearer \[REDACTED\]'
        $redacted | Should -Match '"api_token":"\[REDACTED\]"'
        $redacted | Should -Match '"password":"\[REDACTED\]"'
        $redacted | Should -Match '\[DATA_ROOT\]'
        $redacted | Should -Match '\[REPO_ROOT\]'
        $redacted | Should -Not -Match 'abc\.def\.secret|raw-token|"pw"|D:\\data\\projects'
    }
}
```

- [ ] **Step 2: Run the new tests and verify they fail because the module is missing**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: FAIL during `Import-Module` because `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1` does not exist.

- [ ] **Step 3: Create the module with profile/state/redaction functions**

Create `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1` with these functions:

```powershell
Set-StrictMode -Version Latest

function Resolve-PcvPostRebootRepoRoot {
    param([Parameter(Mandatory)][string]$RepoRoot)

    $resolved = (Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop).Path
    $required = @(
        'AGENTS.md',
        'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1',
        'spikes/purecvisor-desktop-node/README.md'
    )
    foreach ($relative in $required) {
        $path = Join-Path $resolved $relative
        if (-not (Test-Path -LiteralPath $path)) {
            throw "PCV_POST_REBOOT_REPO_BOUNDARY|Repository boundary check failed.|Missing '$relative' under '$resolved'."
        }
    }
    $resolved
}

function Test-PcvPostRebootSensitiveKey {
    param([AllowNull()][string]$Key)

    if ([string]::IsNullOrWhiteSpace($Key)) {
        return $false
    }
    $Key -match '(?i)(authorization|token|api_token|api_token_file|api_token_protected_file|protected_token|token_sha256|access_token|password|secret|private_key|pfx)'
}

function Get-PcvPostRebootBootTimeUtc {
    try {
        return (Get-CimInstance Win32_OperatingSystem -ErrorAction Stop).LastBootUpTime.ToUniversalTime().ToString('o')
    }
    catch {
        return ''
    }
}

function ConvertTo-PcvPostRebootRedactedObject {
    param(
        [AllowNull()]$InputObject,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $InputObject) {
        return $null
    }
    if ($InputObject -is [System.Collections.IDictionary]) {
        $out = [ordered]@{}
        foreach ($key in $InputObject.Keys) {
            if (Test-PcvPostRebootSensitiveKey -Key ([string]$key)) {
                $out[$key] = '[REDACTED]'
            } else {
                $out[$key] = ConvertTo-PcvPostRebootRedactedObject -InputObject $InputObject[$key] -PathRedactions $PathRedactions
            }
        }
        return $out
    }
    if ($InputObject -is [pscustomobject]) {
        $out = [ordered]@{}
        foreach ($property in $InputObject.PSObject.Properties) {
            if (Test-PcvPostRebootSensitiveKey -Key $property.Name) {
                $out[$property.Name] = '[REDACTED]'
            } else {
                $out[$property.Name] = ConvertTo-PcvPostRebootRedactedObject -InputObject $property.Value -PathRedactions $PathRedactions
            }
        }
        return $out
    }
    if ($InputObject -is [System.Collections.IEnumerable] -and $InputObject -isnot [string]) {
        $items = @()
        foreach ($item in $InputObject) {
            $items += ConvertTo-PcvPostRebootRedactedObject -InputObject $item -PathRedactions $PathRedactions
        }
        return $items
    }
    if ($InputObject -is [string]) {
        $value = $InputObject -replace '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+', 'Bearer [REDACTED]'
        if ($null -ne $PathRedactions) {
            foreach ($path in $PathRedactions.Keys) {
                if (-not [string]::IsNullOrWhiteSpace([string]$path)) {
                    $value = $value.Replace([string]$path, [string]$PathRedactions[$path])
                }
            }
        }
        return $value
    }
    $InputObject
}

function ConvertTo-PcvPostRebootRedactedText {
    param(
        [AllowNull()][string]$Text,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $Text) {
        return ''
    }
    $textWithBearerRedacted = $Text -replace '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+', 'Bearer [REDACTED]'
    $lines = $textWithBearerRedacted -split "`r?`n"
    $redactedLines = @()
    foreach ($line in $lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            $redactedLines += $line
            continue
        }
        try {
            $json = $line | ConvertFrom-Json -ErrorAction Stop
            $redacted = ConvertTo-PcvPostRebootRedactedObject -InputObject $json -PathRedactions $PathRedactions
            $redactedLines += ($redacted | ConvertTo-Json -Depth 32 -Compress)
        }
        catch {
            $redactedLines += [string](ConvertTo-PcvPostRebootRedactedObject -InputObject $line -PathRedactions $PathRedactions)
        }
    }
    $redactedLines -join [Environment]::NewLine
}

function New-PcvPostRebootCommand {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 900,
        [bool]$Required = $true,
        [bool]$AllowFailure = $false
    )

    [ordered]@{
        id = $Id
        working_directory = $WorkingDirectory
        file_name = $FileName
        arguments = @($Arguments)
        timeout_seconds = $TimeoutSeconds
        required = $Required
        allow_failure = $AllowFailure
    }
}

function New-PcvPostRebootCommandProfile {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('ProductStatus', 'PackagingRegression', 'HyperVNonIntegration')]
        [string]$Profile,
        [Parameter(Mandatory)][string]$RepoRoot
    )

    $repo = Resolve-PcvPostRebootRepoRoot -RepoRoot $RepoRoot
    $commands = @()
    if ($Profile -eq 'ProductStatus') {
        $commands += New-PcvPostRebootCommand -Id 'product-status' -WorkingDirectory $repo -FileName 'pwsh' -Arguments @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1', '-Action', 'Status') -TimeoutSeconds 300
        $commands += New-PcvPostRebootCommand -Id 'product-diagnostics' -WorkingDirectory $repo -FileName 'pwsh' -Arguments @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1', '-Action', 'CollectDiagnostics') -TimeoutSeconds 600 -Required $false -AllowFailure $true
    }
    elseif ($Profile -eq 'PackagingRegression') {
        $commands += New-PcvPostRebootCommand -Id 'packaging-product-tests' -WorkingDirectory $repo -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed") -TimeoutSeconds 1800
        $commands += New-PcvPostRebootCommand -Id 'packaging-installer-tests' -WorkingDirectory $repo -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed") -TimeoutSeconds 1800
        $commands += New-PcvPostRebootCommand -Id 'git-diff-check' -WorkingDirectory $repo -FileName 'git' -Arguments @('diff', '--check') -TimeoutSeconds 300
    }
    elseif ($Profile -eq 'HyperVNonIntegration') {
        $commands += New-PcvPostRebootCommand -Id 'hyperv-non-integration-tests' -WorkingDirectory $repo -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed") -TimeoutSeconds 1800
    }

    [ordered]@{
        profile = $Profile
        commands = @($commands)
    }
}

function Assert-PcvPostRebootPrincipalAllowed {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode,
        [switch]$RequiresUserProfile,
        [switch]$RequiresNetworkDrive,
        [switch]$RequiresSigningMaterial
    )

    if ($PrincipalMode -eq 'LocalSystemAtStartup' -and ($RequiresUserProfile -or $RequiresNetworkDrive -or $RequiresSigningMaterial)) {
        throw 'PCV_POST_REBOOT_PRINCIPAL_NOT_ALLOWED|The selected principal cannot access required user resources.|Use CurrentUserAtLogOn for command plans that need user profile, network drive, or signing material.'
    }
}

function New-PcvPostRebootVerificationState {
    param(
        [Parameter(Mandatory)][string]$PhaseId,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$EvidenceDir,
        [Parameter(Mandatory)]
        [ValidateSet('ProductStatus', 'PackagingRegression', 'HyperVNonIntegration')]
        [string]$Profile,
        [Parameter(Mandatory)][string]$TaskName,
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode = 'LocalSystemAtStartup',
        [switch]$RequiresUserProfile,
        [switch]$RequiresNetworkDrive,
        [switch]$RequiresSigningMaterial
    )

    Assert-PcvPostRebootPrincipalAllowed -PrincipalMode $PrincipalMode -RequiresUserProfile:$RequiresUserProfile -RequiresNetworkDrive:$RequiresNetworkDrive -RequiresSigningMaterial:$RequiresSigningMaterial
    $repo = Resolve-PcvPostRebootRepoRoot -RepoRoot $RepoRoot
    $profileObject = New-PcvPostRebootCommandProfile -Profile $Profile -RepoRoot $repo
    [ordered]@{
        schema_version = 1
        phase_id = $PhaseId
        task_name = $TaskName
        repo_root = $repo
        evidence_dir = $EvidenceDir
        created_at_utc = [DateTime]::UtcNow.ToString('o')
        created_by_user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
        machine_name = $env:COMPUTERNAME
        pre_reboot_boot_time_utc = Get-PcvPostRebootBootTimeUtc
        profile = $Profile
        principal = [ordered]@{
            mode = $PrincipalMode
            requires_user_profile = [bool]$RequiresUserProfile
            requires_network_drive = [bool]$RequiresNetworkDrive
            requires_signing_material = [bool]$RequiresSigningMaterial
        }
        commands = @($profileObject.commands)
        redaction = [ordered]@{
            repo_root = '[REPO_ROOT]'
            evidence_dir = '[EVIDENCE_ROOT]'
        }
        cleanup = [ordered]@{
            unregister_task = $true
        }
    }
}

Export-ModuleMember -Function `
    Assert-PcvPostRebootPrincipalAllowed, `
    ConvertTo-PcvPostRebootRedactedObject, `
    ConvertTo-PcvPostRebootRedactedText, `
    Get-PcvPostRebootBootTimeUtc, `
    New-PcvPostRebootCommand, `
    New-PcvPostRebootCommandProfile, `
    New-PcvPostRebootVerificationState, `
    Resolve-PcvPostRebootRepoRoot, `
    Test-PcvPostRebootSensitiveKey
```

- [ ] **Step 4: Run tests and verify Task 1 passes**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: PASS for the profile/state/redaction tests.

- [ ] **Step 5: Commit Task 1**

```powershell
git add -- packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1 packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1
git commit -m "feat: add post-reboot verification state contract"
```

### Task 2: Scheduled Task Plan And Pre-Reboot Entrypoint

**Files:**
- Modify: `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`
- Create: `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1`

- [ ] **Step 1: Add failing tests for scheduled task plan and state file writing**

Append this `Describe` block to `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1`:

```powershell
Describe 'PcvPostRebootVerification scheduled task planning' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'builds a LocalSystem AtStartup scheduled task plan' {
        $stateFile = Join-Path $TestDrive 'post-reboot-state.json'
        $runner = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1'

        $plan = New-PcvPostRebootScheduledTaskPlan `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-test' `
            -StateFile $stateFile `
            -RunnerScript $runner `
            -PrincipalMode LocalSystemAtStartup

        $plan.task_name | Should -Be 'PureCVisorDesktopNode-PostRebootVerification-test'
        $plan.trigger | Should -Be 'AtStartup'
        $plan.principal_user_id | Should -Be 'SYSTEM'
        $plan.run_level | Should -Be 'Highest'
        $plan.action_file_name | Should -Be 'pwsh.exe'
        $plan.action_arguments | Should -Match 'Invoke-PcvPostRebootVerification\.ps1'
        $plan.action_arguments | Should -Match '-StateFile'
    }

    It 'writes a state file without registering a task in dry run mode' {
        $evidenceDir = Join-Path $TestDrive 'evidence-dry-run'
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        $registrations = [System.Collections.Generic.List[object]]::new()

        $result = Initialize-PcvPostRebootVerification `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-dry-run' `
            -PrincipalMode LocalSystemAtStartup `
            -DryRun `
            -RegisterTask {
                param($TaskPlan)
                $registrations.Add($TaskPlan)
                [ordered]@{ ok = $true }
            }

        $result.ok | Should -BeTrue
        $result.dry_run | Should -BeTrue
        $result.state_file | Should -Be $stateFile
        Test-Path -LiteralPath $stateFile | Should -BeTrue
        $registrations.Count | Should -Be 0
        $state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
        $state.phase_id | Should -Be 'phase23'
        $state.commands.Count | Should -Be 2
    }
}
```

- [ ] **Step 2: Run the scheduled task tests and verify they fail**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `New-PcvPostRebootScheduledTaskPlan` and `Initialize-PcvPostRebootVerification` are not defined.

- [ ] **Step 3: Add scheduled task plan and initialization functions**

Append these functions before `Export-ModuleMember` in `PcvPostRebootVerification.psm1`, then add the two function names to `Export-ModuleMember`:

```powershell
function Write-PcvPostRebootJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$InputObject
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $InputObject | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function New-PcvPostRebootScheduledTaskPlan {
    param(
        [Parameter(Mandatory)][string]$TaskName,
        [Parameter(Mandatory)][string]$StateFile,
        [Parameter(Mandatory)][string]$RunnerScript,
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode = 'LocalSystemAtStartup'
    )

    $runnerPath = (Resolve-Path -LiteralPath $RunnerScript -ErrorAction Stop).Path
    $arguments = @(
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        $runnerPath,
        '-StateFile',
        $StateFile
    )

    if ($PrincipalMode -eq 'LocalSystemAtStartup') {
        return [ordered]@{
            task_name = $TaskName
            trigger = 'AtStartup'
            principal_mode = $PrincipalMode
            principal_user_id = 'SYSTEM'
            run_level = 'Highest'
            action_file_name = 'pwsh.exe'
            action_arguments = ($arguments | ForEach-Object { if ($_ -match '\s') { '"' + ($_ -replace '"', '\"') + '"' } else { $_ } }) -join ' '
            state_file = $StateFile
            runner_script = $runnerPath
        }
    }

    [ordered]@{
        task_name = $TaskName
        trigger = 'AtLogOn'
        principal_mode = $PrincipalMode
        principal_user_id = [Security.Principal.WindowsIdentity]::GetCurrent().Name
        run_level = 'Highest'
        action_file_name = 'pwsh.exe'
        action_arguments = ($arguments | ForEach-Object { if ($_ -match '\s') { '"' + ($_ -replace '"', '\"') + '"' } else { $_ } }) -join ' '
        state_file = $StateFile
        runner_script = $runnerPath
    }
}

function Register-PcvPostRebootScheduledTask {
    param([Parameter(Mandatory)]$TaskPlan)

    $action = New-ScheduledTaskAction -Execute $TaskPlan.action_file_name -Argument $TaskPlan.action_arguments
    $trigger = if ($TaskPlan.trigger -eq 'AtStartup') {
        New-ScheduledTaskTrigger -AtStartup
    } else {
        New-ScheduledTaskTrigger -AtLogOn
    }
    $principal = New-ScheduledTaskPrincipal -UserId $TaskPlan.principal_user_id -RunLevel Highest
    Register-ScheduledTask -TaskName $TaskPlan.task_name -Action $action -Trigger $trigger -Principal $principal -Force | Out-Null
    [ordered]@{ ok = $true; task_name = $TaskPlan.task_name }
}

function Initialize-PcvPostRebootVerification {
    param(
        [Parameter(Mandatory)][string]$PhaseId,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$EvidenceDir,
        [Parameter(Mandatory)]
        [ValidateSet('ProductStatus', 'PackagingRegression', 'HyperVNonIntegration')]
        [string]$Profile,
        [Parameter(Mandatory)][string]$TaskName,
        [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
        [string]$PrincipalMode = 'LocalSystemAtStartup',
        [switch]$RequiresUserProfile,
        [switch]$RequiresNetworkDrive,
        [switch]$RequiresSigningMaterial,
        [switch]$DryRun,
        [scriptblock]$RegisterTask
    )

    $evidencePath = $EvidenceDir
    New-Item -ItemType Directory -Path $evidencePath -Force | Out-Null
    $state = New-PcvPostRebootVerificationState -PhaseId $PhaseId -RepoRoot $RepoRoot -EvidenceDir $evidencePath -Profile $Profile -TaskName $TaskName -PrincipalMode $PrincipalMode -RequiresUserProfile:$RequiresUserProfile -RequiresNetworkDrive:$RequiresNetworkDrive -RequiresSigningMaterial:$RequiresSigningMaterial
    $stateFile = Join-Path $evidencePath 'post-reboot-state.json'
    Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

    $runner = Join-Path $state.repo_root 'packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1'
    $taskPlan = New-PcvPostRebootScheduledTaskPlan -TaskName $TaskName -StateFile $stateFile -RunnerScript $runner -PrincipalMode $PrincipalMode

    $registration = $null
    if (-not $DryRun) {
        if ($null -eq $RegisterTask) {
            $RegisterTask = { param($TaskPlan) Register-PcvPostRebootScheduledTask -TaskPlan $TaskPlan }
        }
        $registration = & $RegisterTask -TaskPlan $taskPlan
    }

    [ordered]@{
        ok = $true
        dry_run = [bool]$DryRun
        state_file = $stateFile
        task_plan = $taskPlan
        registration = $registration
    }
}
```

Update `Export-ModuleMember` so it includes:

```powershell
    Initialize-PcvPostRebootVerification, `
    New-PcvPostRebootScheduledTaskPlan, `
    Register-PcvPostRebootScheduledTask, `
    Write-PcvPostRebootJsonFile, `
```

- [ ] **Step 4: Create the pre-reboot registration script**

Create `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1`:

```powershell
param(
    [Parameter(Mandatory)][string]$PhaseId,
    [string]$RepoRoot = '',
    [Parameter(Mandatory)][string]$EvidenceDir,
    [ValidateSet('ProductStatus', 'PackagingRegression', 'HyperVNonIntegration')]
    [string]$Profile = 'ProductStatus',
    [string]$TaskName = '',
    [ValidateSet('LocalSystemAtStartup', 'CurrentUserAtLogOn')]
    [string]$PrincipalMode = 'LocalSystemAtStartup',
    [switch]$RequiresUserProfile,
    [switch]$RequiresNetworkDrive,
    [switch]$RequiresSigningMaterial,
    [switch]$DryRun,
    [switch]$Reboot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvPostRebootVerification.psm1'
Import-Module $modulePath -Force

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
}

if ([string]::IsNullOrWhiteSpace($TaskName)) {
    $TaskName = "PureCVisorDesktopNode-PostRebootVerification-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
}

$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    throw 'PCV_POST_REBOOT_ADMIN_REQUIRED|Administrator PowerShell is required.|Run this script from an elevated PowerShell session.'
}

$result = Initialize-PcvPostRebootVerification `
    -PhaseId $PhaseId `
    -RepoRoot $RepoRoot `
    -EvidenceDir $EvidenceDir `
    -Profile $Profile `
    -TaskName $TaskName `
    -PrincipalMode $PrincipalMode `
    -RequiresUserProfile:$RequiresUserProfile `
    -RequiresNetworkDrive:$RequiresNetworkDrive `
    -RequiresSigningMaterial:$RequiresSigningMaterial `
    -DryRun:$DryRun

$result | ConvertTo-Json -Depth 32 | Write-Output

if ($Reboot) {
    throw 'PCV_POST_REBOOT_AUTO_REBOOT_DISABLED|Automatic reboot is disabled for this workflow.|Register the post-reboot verification task, then reboot manually when ready.'
}
```

- [ ] **Step 5: Run tests and verify Task 2 passes**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: PASS. No scheduled task is registered because tests use `-DryRun` and injected `RegisterTask`.

- [ ] **Step 6: Commit Task 2**

```powershell
git add -- packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1 packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1
git commit -m "feat: plan post-reboot scheduled verification"
```

### Task 3: Post-Reboot Runner And Evidence Artifacts

**Files:**
- Modify: `packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1`
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1`

- [x] **Step 1: Add failing runner tests**

Append this `Describe` block to `packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1`:

```powershell
Describe 'PcvPostRebootVerification runner evidence' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'runs commands, writes redacted artifacts, and unregisters the task' {
        $evidenceDir = Join-Path $TestDrive 'runner-evidence'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile ProductStatus `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-runner' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $calls = [System.Collections.Generic.List[object]]::new()
        $unregistered = [System.Collections.Generic.List[string]]::new()
        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            $calls.Add([pscustomobject]@{
                file = $FileName
                args = $Arguments
                cwd = $WorkingDirectory
                timeout = $TimeoutSeconds
            })
            [ordered]@{
                exit_code = 0
                stdout = '{"Authorization":"Bearer abc.def.secret","api_token":"raw-token"}'
                stderr = ''
                timed_out = $false
                duration_ms = 25
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { ' M packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1' } `
            -UnregisterTask { param([string]$TaskName) $unregistered.Add($TaskName); [ordered]@{ ok = $true } }

        $result.ok | Should -BeTrue
        $calls.Count | Should -Be 2
        $unregistered | Should -Contain 'PureCVisorDesktopNode-PostRebootVerification-runner'
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-result.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $evidenceDir 'post-reboot-summary.md') | Should -BeTrue
        $stdout = Get-Content -LiteralPath (Join-Path $evidenceDir 'post-reboot-stdout-product-status.log') -Raw
        $stdout | Should -Match '"Authorization":"\[REDACTED\]"'
        $stdout | Should -Match '"api_token":"\[REDACTED\]"'
        $stdout | Should -Not -Match 'abc\.def\.secret|raw-token'
    }

    It 'marks required command failure as overall failure and still writes evidence' {
        $evidenceDir = Join-Path $TestDrive 'runner-failure'
        $state = New-PcvPostRebootVerificationState `
            -PhaseId 'phase23' `
            -RepoRoot $script:RepoRoot `
            -EvidenceDir $evidenceDir `
            -Profile PackagingRegression `
            -TaskName 'PureCVisorDesktopNode-PostRebootVerification-failure' `
            -PrincipalMode LocalSystemAtStartup
        $stateFile = Join-Path $evidenceDir 'post-reboot-state.json'
        Write-PcvPostRebootJsonFile -Path $stateFile -InputObject $state

        $runner = {
            param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds)
            [ordered]@{
                exit_code = 1
                stdout = ''
                stderr = 'Pester failed'
                timed_out = $false
                duration_ms = 10
            }
        }

        $result = Invoke-PcvPostRebootVerification `
            -StateFile $stateFile `
            -InvokeProcess $runner `
            -GetBootTimeUtc { '2026-04-29T01:02:03.0000000Z' } `
            -GetGitCommit { 'abc1234' } `
            -GetGitStatusSummary { '' } `
            -UnregisterTask { param([string]$TaskName) [ordered]@{ ok = $true } }

        $result.ok | Should -BeFalse
        $json = Get-Content -LiteralPath (Join-Path $evidenceDir 'post-reboot-result.json') -Raw | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.commands[0].exit_code | Should -Be 1
    }
}
```

- [x] **Step 2: Run runner tests and verify they fail**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `Invoke-PcvPostRebootVerification` is not defined.

- [x] **Step 3: Add runner helper functions**

Append these functions before `Export-ModuleMember` and export them:

```powershell
function Read-PcvPostRebootState {
    param([Parameter(Mandatory)][string]$StateFile)

    if (-not (Test-Path -LiteralPath $StateFile -PathType Leaf)) {
        throw "PCV_POST_REBOOT_STATE_NOT_FOUND|Post-reboot state file was not found.|Path: '$StateFile'."
    }
    Get-Content -LiteralPath $StateFile -Raw | ConvertFrom-Json
}

function Invoke-PcvPostRebootNativeProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [int]$TimeoutSeconds = 900
    )

    $started = Get-Date
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = $FileName
    $process.StartInfo.WorkingDirectory = $WorkingDirectory
    foreach ($argument in $Arguments) {
        [void]$process.StartInfo.ArgumentList.Add($argument)
    }
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.CreateNoWindow = $true
    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
    if ($timedOut) {
        try { $process.Kill($true) } catch {}
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $finished = Get-Date
    [ordered]@{
        exit_code = $(if ($timedOut) { -1 } else { $process.ExitCode })
        stdout = $stdout
        stderr = $stderr
        timed_out = $timedOut
        duration_ms = [int]($finished - $started).TotalMilliseconds
    }
}

function Write-PcvPostRebootTextFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Text
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    Set-Content -LiteralPath $Path -Value ([string]$Text) -Encoding UTF8
}

function New-PcvPostRebootMarkdownSummary {
    param([Parameter(Mandatory)]$Result)

    $lines = @()
    $lines += "### Post-reboot verification"
    $lines += ""
    $lines += "- phase: `$($Result.phase_id)`"
    $lines += "- ok: `$($Result.ok)`"
    $lines += "- started_at_utc: `$($Result.started_at_utc)`"
    $lines += "- finished_at_utc: `$($Result.finished_at_utc)`"
    $lines += "- windows_boot_time_utc: `$($Result.windows_boot_time_utc)`"
    $lines += "- git_commit: `$($Result.git_commit)`"
    $lines += "- git_status_summary: `$($Result.git_status_summary)`"
    foreach ($command in @($Result.commands)) {
        $lines += "- command `$($command.id)`: exit `$($command.exit_code)`, ok `$($command.ok)`, timed_out `$($command.timed_out)`"
    }
    $lines -join [Environment]::NewLine
}

function Invoke-PcvPostRebootVerification {
    param(
        [Parameter(Mandatory)][string]$StateFile,
        [scriptblock]$InvokeProcess,
        [scriptblock]$GetBootTimeUtc,
        [scriptblock]$GetGitCommit,
        [scriptblock]$GetGitStatusSummary,
        [scriptblock]$UnregisterTask
    )

    $state = Read-PcvPostRebootState -StateFile $StateFile
    $repo = Resolve-PcvPostRebootRepoRoot -RepoRoot ([string]$state.repo_root)
    $evidenceDir = [string]$state.evidence_dir
    New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null
    $completeMarker = Join-Path $evidenceDir 'post-reboot-complete.json'
    if (Test-Path -LiteralPath $completeMarker -PathType Leaf) {
        $existing = Get-Content -LiteralPath $completeMarker -Raw | ConvertFrom-Json
        return [ordered]@{ ok = $true; already_completed = $true; completed_at_utc = $existing.completed_at_utc }
    }

    if ($null -eq $InvokeProcess) {
        $InvokeProcess = { param([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory, [int]$TimeoutSeconds) Invoke-PcvPostRebootNativeProcess -FileName $FileName -Arguments $Arguments -WorkingDirectory $WorkingDirectory -TimeoutSeconds $TimeoutSeconds }
    }
    if ($null -eq $GetBootTimeUtc) {
        $GetBootTimeUtc = { (Get-CimInstance Win32_OperatingSystem).LastBootUpTime.ToUniversalTime().ToString('o') }
    }
    if ($null -eq $GetGitCommit) {
        $GetGitCommit = { (& git -C $repo rev-parse --short HEAD 2>$null) -join "`n" }
    }
    if ($null -eq $GetGitStatusSummary) {
        $GetGitStatusSummary = { (& git -C $repo status --short 2>$null) -join '; ' }
    }
    if ($null -eq $UnregisterTask) {
        $UnregisterTask = { param([string]$TaskName) Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false; [ordered]@{ ok = $true } }
    }

    $pathRedactions = @{
        $repo = '[REPO_ROOT]'
        $evidenceDir = '[EVIDENCE_ROOT]'
    }
    $started = [DateTime]::UtcNow
    $commandResults = @()
    $overallOk = $true
    foreach ($command in @($state.commands)) {
        $processResult = & $InvokeProcess -FileName ([string]$command.file_name) -Arguments ([string[]]$command.arguments) -WorkingDirectory ([string]$command.working_directory) -TimeoutSeconds ([int]$command.timeout_seconds)
        $stdoutPath = Join-Path $evidenceDir "post-reboot-stdout-$($command.id).log"
        $stderrPath = Join-Path $evidenceDir "post-reboot-stderr-$($command.id).log"
        Write-PcvPostRebootTextFile -Path $stdoutPath -Text (ConvertTo-PcvPostRebootRedactedText -Text ([string]$processResult.stdout) -PathRedactions $pathRedactions)
        Write-PcvPostRebootTextFile -Path $stderrPath -Text (ConvertTo-PcvPostRebootRedactedText -Text ([string]$processResult.stderr) -PathRedactions $pathRedactions)
        $commandOk = ([int]$processResult.exit_code -eq 0 -and -not [bool]$processResult.timed_out)
        if (-not $commandOk -and [bool]$command.required -and -not [bool]$command.allow_failure) {
            $overallOk = $false
        }
        $commandResults += [ordered]@{
            id = [string]$command.id
            exit_code = [int]$processResult.exit_code
            duration_ms = [int]$processResult.duration_ms
            timed_out = [bool]$processResult.timed_out
            stdout_artifact = Split-Path -Leaf $stdoutPath
            stderr_artifact = Split-Path -Leaf $stderrPath
            ok = $commandOk
        }
    }

    $cleanup = & $UnregisterTask -TaskName ([string]$state.task_name)
    $result = [ordered]@{
        schema_version = 1
        phase_id = [string]$state.phase_id
        task_name = [string]$state.task_name
        started_at_utc = $started.ToString('o')
        finished_at_utc = [DateTime]::UtcNow.ToString('o')
        ok = $overallOk
        windows_boot_time_utc = [string](& $GetBootTimeUtc)
        powershell_version = $PSVersionTable.PSVersion.ToString()
        git_commit = [string](& $GetGitCommit)
        git_status_summary = ConvertTo-PcvPostRebootRedactedText -Text ([string](& $GetGitStatusSummary)) -PathRedactions $pathRedactions
        commands = @($commandResults)
        cleanup = $cleanup
    }
    Write-PcvPostRebootJsonFile -Path (Join-Path $evidenceDir 'post-reboot-result.json') -InputObject $result
    Write-PcvPostRebootTextFile -Path (Join-Path $evidenceDir 'post-reboot-summary.md') -Text (New-PcvPostRebootMarkdownSummary -Result $result)
    Write-PcvPostRebootJsonFile -Path $completeMarker -InputObject ([ordered]@{ completed_at_utc = [DateTime]::UtcNow.ToString('o') })
    $result
}
```

Add these names to `Export-ModuleMember`:

```powershell
    Invoke-PcvPostRebootNativeProcess, `
    Invoke-PcvPostRebootVerification, `
    New-PcvPostRebootMarkdownSummary, `
    Read-PcvPostRebootState, `
    Write-PcvPostRebootTextFile, `
```

- [x] **Step 4: Create the scheduled-task runner script**

Create `packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1`:

```powershell
param(
    [Parameter(Mandatory)][string]$StateFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvPostRebootVerification.psm1'
Import-Module $modulePath -Force

$result = Invoke-PcvPostRebootVerification -StateFile $StateFile
$result | ConvertTo-Json -Depth 32 | Write-Output
if ($result.ok) {
    exit 0
}
exit 1
```

- [x] **Step 5: Run tests and verify Task 3 passes**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: PASS. The test writes JSON and log artifacts under `$TestDrive`; it does not register scheduled tasks.

- [ ] **Step 6: Commit Task 3**

```powershell
git add -- packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1 packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1 packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1
git commit -m "feat: run post-reboot verification evidence"
```

### Task 4: Documentation And Phase Runbook Links

**Files:**
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md`
- Modify: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence.md`
- Modify: `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence.md`

- [x] **Step 1: Update the product wrapper README**

Add this section to `packaging/windows-desktop-node/README.md` after the `관리자 smoke` section and before `데이터 보존`:

```markdown
## Post-reboot verification dry-run/runner evidence

Windows reboot가 필요한 관리자 smoke는 먼저 post-reboot verification dry-run으로 상태 파일과 command plan을 확인한다. Runner entrypoint는 state file 기반 command 실행과 redacted evidence 작성을 지원한다. 현재 slice는 Codex TUI를 자동으로 재개하지 않는다. 자동화 범위는 reboot 이후 scheduled task가 검증 command와 redacted evidence 작성을 실행하고, 성공 시 명시된 continuation profile을 이어 실행하는 데 한정한다.

기본 mode는 `LocalSystemAtStartup`이다. 사용자 profile, mapped network drive, user certificate store, signing material이 필요한 command plan은 기본 mode에서 거부하고 `CurrentUserAtLogOn` opt-in으로만 다룬다. 자동 로그인, password 저장, credential persistence는 사용하지 않는다.

Dry-run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir '<evidence-dir>' -Profile ProductStatus -DryRun
```

`-DryRun` 없는 실행은 명시적 administrator opt-in task registration 경로다. `-ContinuationProfiles PackagingRegression`을 지정하면 reboot 이후 기본 profile 성공 뒤 후속 regression 검증을 자동으로 이어 실행한다. `-Reboot` 실행은 항상 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 차단하며, 실제 reboot는 사용자가 수동으로 수행한다.
```

- [x] **Step 2: Add Phase 20 post-reboot note**

In `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md`, add this paragraph near the administrator lifecycle smoke section:

```markdown
Post-reboot verification 선택지:

서명/MSI lifecycle smoke 중 Windows reboot가 필요한 경우 `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1`로 1회성 post-reboot verification task를 등록할 수 있다. 기본 profile은 `ProductStatus`이고, `-ContinuationProfiles PackagingRegression`으로 성공 후 regression 검증을 이어 붙인다. Signing material이나 user certificate store가 필요한 command plan은 `LocalSystemAtStartup`에서 실행하지 않고 `CurrentUserAtLogOn` opt-in으로만 실행한다. 자동화 결과는 external evidence directory의 `post-reboot-summary.md`를 이 plan의 `완료 증거`에 붙인다. 자동 reboot는 사용하지 않는다.
```

- [x] **Step 3: Add Phase 21 post-reboot note**

In `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence.md`, add this paragraph near the failure/interruption section:

```markdown
Post-reboot verification 선택지:

Hyper-V lifecycle 또는 service interruption smoke 중 Windows reboot가 별도 opt-in으로 승인된 경우 post-reboot verification runner를 사용할 수 있다. 기본 profile은 `HyperVNonIntegration` 또는 `ProductStatus`다. 실제 VM 생성/삭제 command는 기본 profile에 포함하지 않으며, 명시적 administrator opt-in command plan으로만 다룬다. Runner 결과는 external evidence directory의 `post-reboot-summary.md`를 이 plan의 `완료 증거`에 붙인다.
```

- [x] **Step 4: Add Phase 23 post-reboot note**

In `docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence.md`, add this paragraph near the long-running service evidence task:

```markdown
Post-reboot verification 선택지:

장기 service run, service recovery, reboot 이후 diagnostics 확인에는 post-reboot verification runner를 사용할 수 있다. 기본 mode는 `LocalSystemAtStartup`이며, reboot 이후 로그인 없이 `ProductStatus` profile을 실행해 service status와 diagnostic bundle 생성을 확인한다. 결과 summary와 redacted artifacts는 external evidence directory에 남기고, `post-reboot-summary.md`의 짧은 요약만 이 plan의 `완료 증거`에 붙인다.
```

- [ ] **Step 5: Commit Task 4**

```powershell
git add -- packaging/windows-desktop-node/README.md docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence.md docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence.md
git commit -m "docs: document post-reboot verification runbook"
```

### Task 5: Verification And Final Hygiene

**Files:**
- No source changes expected. Use this task to verify the complete implementation.

- [ ] **Step 1: Run the focused post-reboot verification tests**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1' -Output Detailed"
```

Expected: all tests pass.

- [ ] **Step 2: Run the packaging suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: all packaging tests pass. If failures come from pre-existing unrelated working tree changes, stop and inspect the failing file before modifying anything.

- [ ] **Step 3: Run installer tests if README or installer contract references changed**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected: all installer tests pass.

- [ ] **Step 4: Run whitespace hygiene**

```powershell
git diff --check
```

Expected: exit code 0.

- [ ] **Step 5: Confirm no default host mutation commands were executed**

Run:

```powershell
git diff -- packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1 packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1
```

Expected: implementation contains `Register-ScheduledTask` and `Unregister-ScheduledTask` only behind explicit script/function calls. It must not contain an executable `Restart-Computer` path. Pester tests use `-DryRun` or injected scriptblocks and do not call real Task Scheduler or reboot commands.

- [ ] **Step 6: Commit final verification note if docs changed during fixes**

If Task 5 required documentation changes, commit only those exact files:

```powershell
git add -- packaging/windows-desktop-node/README.md docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence.md docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence.md docs/superpowers/plans/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence.md
git commit -m "docs: clarify post-reboot verification evidence"
```

If no docs changed during Task 5, do not create an empty commit.

## Administrator Opt-In Smoke

현재 구현 slice에서는 dry-run, explicit task registration, runner evidence, success-only continuation contract를 지원한다. 아래 dry-run 명령은 state file과 task plan을 작성하지만 실제 Task Scheduler 등록이나 reboot를 수행하지 않는다.

```powershell
$evidence = Join-Path $env:TEMP ('pcv-post-reboot-' + [guid]::NewGuid().ToString('N'))
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir $evidence -Profile ProductStatus -DryRun
```

예상: state file과 task plan은 작성되지만 task는 등록되지 않는다.

Runner entrypoint는 scheduled task action path 계약으로 구현되어 있으며, Pester에서는 injected runner로 command 실행/evidence 작성/cleanup 실패 기록을 검증한다. 실제 task registration과 reboot smoke는 별도 administrator opt-in으로만 연다. 현재는 아래 동작이 차단된다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir $evidence -Profile ProductStatus -Reboot
```

예상: `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`.

명시적으로 task만 등록하고 사용자가 직접 reboot하려면 `-Reboot` 없이 실행한다. 후속 regression을 자동으로 이어 붙이는 예시는 다음과 같다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir $evidence -Profile ProductStatus -ContinuationProfiles PackagingRegression
```

예상: 1회성 scheduled task plan/register가 완료되고, 실제 reboot는 수행하지 않는다. 사용자가 수동 reboot를 완료하면 runner가 `ProductStatus`를 실행하고 성공할 때만 `PackagingRegression` command를 이어 실행한다. 여러 후속 검증이 필요하면 `-ContinuationProfiles PackagingRegression,HyperVNonIntegration`처럼 tool이 지원하는 profile 이름만 지정한다.
