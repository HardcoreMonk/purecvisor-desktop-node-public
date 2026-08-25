# PureCVisor Development Feedback Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add fast/full/release verification lanes and remove measured PowerShell test delays without reducing coverage.

**Architecture:** A PowerShell module selects suites from lane, tier, and changed paths; a thin entry script executes the selection and writes JSON. Batch Supervisor and installer tests gain explicit process/time seams so unit tests run in-process while representative wrapper integrations remain.

**Tech Stack:** PowerShell 7.6, Pester 5.7.1, .NET 10, Node.js 24, GitHub Actions.

---

Source design: `docs/superpowers/specs/2026-07-16-purecvisor-desktop-node-development-throughput-automation-design.md`

## File map

- Create `packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1` for selection, execution, and summaries.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1` as the CLI wrapper.
- Create `packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1` for lane and execution contracts.
- Modify `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1` and its focused test for process/time seams.
- Create `packaging/windows-desktop-node/installer/PcvDesktopNodeInstaller.Build.psm1` and reduce `build.ps1` to a wrapper.
- Move installer Plan/Signing assertions in-process and add a focused wrapper integration test.
- Update `.github/workflows/development-gates.yml`, verification policy, and code-level evidence.

### Task 1: Deterministic lane selection

**Files:**
- Create: `packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1`
- Test: `packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1`

- [ ] **Step 1: Write failing selection tests**

```powershell
BeforeAll { Import-Module (Join-Path $PSScriptRoot '../tools/PcvDevelopmentVerification.psm1') -Force }
Describe 'Resolve-PcvDevelopmentVerificationSelection' {
    It 'selects dotnet for source-only tier S' {
        $r = Resolve-PcvDevelopmentVerificationSelection -Lane Fast -ChangeTier S `
            -ChangedPath @('src/DesktopNode.Api/Program.cs')
        $r.effective_lane | Should -Be Fast
        $r.suites | Should -Be @('dotnet')
    }
    It 'selects npm and Web Pester for Web changes' {
        $r = Resolve-PcvDevelopmentVerificationSelection -Lane Fast -ChangeTier S `
            -ChangedPath @('web/src/app.ts')
        $r.suites | Should -Be @('web-npm','web-pester')
    }
    It 'promotes unknown paths to Full' {
        $r = Resolve-PcvDevelopmentVerificationSelection -Lane Fast -ChangeTier S `
            -ChangedPath @('unclassified/new.txt')
        $r.effective_lane | Should -Be Full
        $r.promotion_reason | Should -Be 'unknown-change-scope'
    }
    It 'promotes M to Full and L to Release' {
        (Resolve-PcvDevelopmentVerificationSelection -Lane Fast -ChangeTier M `
            -ChangedPath @('src/a.cs')).effective_lane | Should -Be Full
        (Resolve-PcvDevelopmentVerificationSelection -Lane Fast -ChangeTier L `
            -ChangedPath @('src/a.cs')).effective_lane | Should -Be Release
    }
}
```

- [ ] **Step 2: Verify RED**

Run `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1 -Output Detailed`.
Expected: module/function missing failures.

- [ ] **Step 3: Implement the selector**

```powershell
function Resolve-PcvDevelopmentVerificationSelection {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('Fast','Full','Release')][string]$Lane,
        [Parameter(Mandatory)][ValidateSet('S','M','L')][string]$ChangeTier,
        [Parameter(Mandatory)][string[]]$ChangedPath
    )
    $all = @('dotnet','web-npm','packaging-pester','installer-pester','web-pester','git-diff-check')
    $effective = if($ChangeTier -eq 'L'){'Release'}elseif($ChangeTier -eq 'M' -and $Lane -eq 'Fast'){'Full'}else{$Lane}
    $reason = if($effective -ne $Lane){"tier-$($ChangeTier.ToLowerInvariant())-requires-$($effective.ToLowerInvariant())"}else{''}
    if($effective -ne 'Fast'){
        return [pscustomobject]@{requested_lane=$Lane;effective_lane=$effective;change_tier=$ChangeTier;promotion_reason=$reason;suites=$all}
    }
    $selected=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase); $unknown=$false
    foreach($path in $ChangedPath){
        switch -Regex ($path.Replace('\','/')) {
            '^(src/|.*\.sln$|.*\.csproj$)' {[void]$selected.Add('dotnet');continue}
            '^web/' {[void]$selected.Add('web-npm');[void]$selected.Add('web-pester');continue}
            '^packaging/windows-desktop-node/installer/' {[void]$selected.Add('installer-pester');continue}
            '^packaging/windows-desktop-node/(tools/PcvBatchSupervisor\.psm1|tests/PcvBatchSupervisor\.Tests\.ps1)$' {[void]$selected.Add('packaging-pester');continue}
            '^docs/' {[void]$selected.Add('git-diff-check');continue}
            default {$unknown=$true}
        }
    }
    if($unknown -or $selected.Count -eq 0){
        return [pscustomobject]@{requested_lane=$Lane;effective_lane='Full';change_tier=$ChangeTier;promotion_reason='unknown-change-scope';suites=$all}
    }
    [pscustomobject]@{requested_lane=$Lane;effective_lane='Fast';change_tier=$ChangeTier;promotion_reason='';suites=@($selected|Sort-Object)}
}
Export-ModuleMember -Function Resolve-PcvDevelopmentVerificationSelection
```

- [ ] **Step 4: Verify GREEN and commit**

Run the focused Pester command. Expected: all tests pass.

```powershell
git add packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1 packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1
git commit -m "feat: classify development verification lanes"
```

### Task 2: Execute suites and write JSON summaries

**Files:**
- Modify: `packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1`
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1`

- [ ] **Step 1: Add a failing command-runner test**

```powershell
It 'records selected, skipped and failed suites' {
    $calls=[Collections.Generic.List[string]]::new()
    $runner={param($Suite,$FileName,$Arguments,$WorkingDirectory) $calls.Add($Suite);[pscustomobject]@{exit_code=$(if($Suite -eq 'web-pester'){7}else{0});duration_ms=4}}
    $r=Invoke-PcvDevelopmentVerification -Lane Fast -ChangeTier S -ChangedPath @('web/src/app.ts') -CommandRunner $runner
    $r.ok | Should -BeFalse
    $r.failed_suite | Should -Be 'web-pester'
    ($r.results|Where-Object suite -eq dotnet).status | Should -Be 'not-selected-by-scope'
    $calls | Should -Be @('web-npm','web-pester')
}
```

- [ ] **Step 2: Verify RED, then implement catalog and execution**

Use exact suite commands: `dotnet test src/DesktopNode.sln -c Release`, npm test plus parity, the three Pester owners, and `git diff --check`. Slice B adds evidence `-Check` after the generator exists. The default runner must use `ProcessStartInfo.ArgumentList`, capture bounded output, and redact the repository root. Return this schema:

```powershell
[ordered]@{schema_version=1;ok=($failedSuite -eq '');requested_lane=$selection.requested_lane;
effective_lane=$selection.effective_lane;change_tier=$selection.change_tier;
promotion_reason=$selection.promotion_reason;failed_suite=$failedSuite;results=@($results)}
```

- [ ] **Step 3: Implement the entry script**

The wrapper accepts `Lane`, `ChangeTier`, `BaseRef`, `ChangedPath`, `ArtifactRoot`, and `PlanOnly`; derives paths with `git diff --name-only <BaseRef>...HEAD` when needed; writes `summary.json`; and exits 1 only when `ok=false`. PlanOnly records commands without executing them.

- [ ] **Step 4: Verify GREEN and commit**

Run focused Pester and a Fast PlanOnly smoke. Expected: selected/skipped reasons are complete and no process runs in PlanOnly.

```powershell
git add packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1 packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1
git commit -m "feat: orchestrate scoped development verification"
```

### Task 3: Replace Batch Supervisor wall-clock unit tests

**Files:**
- Modify: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`

- [ ] **Step 1: Add failing fake process/time tests**

Test success, timeout/kill, retry, heartbeat and resume with a fake process factory, now provider and wait action. Confirm RED because dependency parameters do not exist.

- [ ] **Step 2: Add production-default seams**

```powershell
[scriptblock]$ProcessFactory={param($si) $p=[Diagnostics.Process]::new();$p.StartInfo=$si;$p},
[scriptblock]$NowProvider={Get-Date},
[scriptblock]$WaitAction={param([int]$ms) Start-Sleep -Milliseconds $ms}
```

Propagate them from `Invoke-PcvBatchSupervisor` through step/attempt functions. Replace internal `Get-Date`, process construction, and heartbeat sleep with these dependencies. Do not change result JSON.

- [ ] **Step 3: Remove slow real-time cases**

Convert the 10-second timeout, 2-second heartbeat and retry cases to fakes. Retain one actual pwsh output test and one process-start failure integration.

- [ ] **Step 4: Verify and commit**

Run the focused suite twice. Expected: 26 tests pass and duration is at most 15 seconds.

```powershell
git add packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1
git commit -m "test: remove Batch Supervisor wall-clock waits"
```

### Task 4: Move installer plan/signing tests in-process

**Files:**
- Create: `packaging/windows-desktop-node/installer/PcvDesktopNodeInstaller.Build.psm1`
- Modify: `packaging/windows-desktop-node/installer/build.ps1`
- Modify: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1`
- Create: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1`

- [ ] **Step 1: Add failing module tests**

Import the missing module and call `Invoke-PcvDesktopNodeInstallerBuild -Input $input -ToolRunner $fake`. Cover unsigned DryRun, missing host, InternalEnterprise provenance, missing trust model, signing success/failure, and thumbprint redaction. Confirm RED.

- [ ] **Step 2: Extract reusable functions unchanged**

Move validation, plan, redaction, WiX/signing, provenance, and publication helpers from `build.ps1` into the module. Add one public entry that returns the existing payload and never calls `exit`:

```powershell
function Invoke-PcvDesktopNodeInstallerBuild {
    [CmdletBinding()] param([Parameter(Mandatory)][hashtable]$Input,[scriptblock]$ToolRunner=${function:Invoke-PcvInstallerProcess})
    Invoke-PcvInstallerBuildCore -Input $Input -ToolRunner $ToolRunner
}
Export-ModuleMember -Function Invoke-PcvDesktopNodeInstallerBuild
```

- [ ] **Step 3: Reduce build.ps1 to a wrapper**

Keep its parameter block; import the module; convert bound parameters to `$input`; emit compressed JSON; exit 0 only when `payload.ok=true`. No WiX/signing plan logic remains in the wrapper.

- [ ] **Step 4: Keep three wrapper integrations**

Actual pwsh covers unsigned DryRun success, structured missing-host error, and signing failure redaction. All other Plan/Signing assertions invoke the module in-process.

- [ ] **Step 5: Verify and commit**

Run installer Pester. Expected: prior assertions remain, new wrapper tests pass, and Plan+Signing combined duration is at most 20 seconds.

```powershell
git add packaging/windows-desktop-node/installer
git commit -m "refactor: test installer build logic in process"
```

### Task 5: Full-lane CI and performance evidence

**Files:**
- Modify: `.github/workflows/development-gates.yml`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Create: `docs/ga-ready/evidence/development-feedback-loop-code-level-2026-07-16.md`

- [ ] **Step 1: Add a failing workflow contract test**

Assert the workflow includes Full-lane contract validation, preserves four independent jobs, and contains no host mutation flag/package runner. Confirm RED before YAML changes.

- [ ] **Step 2: Update CI and policy**

Keep four parallel jobs and existing commands; add the orchestrator Full PlanOnly contract or suite-specific entry use without serializing jobs. Document Fast as local feedback, Full as PR/main, and Release as non-mutating preflight.

- [ ] **Step 3: Measure and record**

Run the same stopwatch commands used in the design baseline. Evidence records old/new duration, counts, commit, `host_mutation_performed=false`, and no public-release claim. Require at least 30% aggregate Pester reduction.

- [ ] **Step 4: Run final Slice A gates**

```powershell
dotnet test src/DesktopNode.sln -c Release --no-restore
npm test --prefix web
npm run verify:parity --prefix web
Invoke-Pester -Path packaging/windows-desktop-node/tests -Output Detailed
Invoke-Pester -Path @('packaging/windows-desktop-node/installer/tests','web/tests') -Output Detailed
git diff --check
```

Expected: 0 failures, no TUI reintroduction, no host mutation.

- [ ] **Step 5: Commit evidence**

```powershell
git add .github/workflows/development-gates.yml docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/ga-ready/evidence/development-feedback-loop-code-level-2026-07-16.md
git commit -m "docs: record development feedback loop verification"
```
