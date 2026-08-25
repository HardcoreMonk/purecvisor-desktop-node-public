# PureCVisor Desktop Node Post-0.42.62 Follow-up Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close the eight approved post-`0.42.62` engineering, evidence, CI-efficiency, operational-validation, and worktree-cleanup actions without changing the verified product payload.

**Architecture:** Extend PR #171 with test-first ledger, WMI query/path, and CI-trigger hardening, then land it only after local and remote gates pass. After merge, use one docs/evidence follow-up branch to record the main-push boundary plus honest PASS-or-blocked operational outcomes; perform worktree cleanup only after a fresh per-path audit.

**Tech Stack:** .NET 10/C#, xUnit, PowerShell 7, Pester 5.7.1, System.Management WMI, GitHub Actions YAML, Git/GitHub CLI, Hyper-V/PowerShell Direct, WiX/MSI/MSIX packaging scripts.

---

## File map

- Modify `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`: align current metadata and current-anchor table.
- Modify `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`: guard ledger and follow-up evidence consistency.
- Modify `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs`: expose and enforce the WMI association traversal query/path contract.
- Modify `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`: cover WMI full projection and empty-path rejection.
- Create `packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1`: prevent duplicate feature-push/PR runs.
- Modify `packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1`: align the existing Development Gates trigger contract.
- Modify `.github/workflows/development-gates.yml`: keep PR, main push, manual triggers; remove `codex/**` push.
- Modify `.github/workflows/public-boundary.yml`: keep PR, main push, manual triggers; remove `codex/**` push.
- Modify `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`: describe the deduplicated trigger contract.
- Create `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md`: own the merged main run.
- Create `docs/ga-ready/evidence/manual-admin-campaign-2026-07-13-04259-04262.md`: record PASS or exact readiness blocker.
- Create `docs/ga-ready/evidence/secondary-hyperv-wmi-topology-smoke-2026-07-13-04262.md`: record PASS or missing-host blocker.
- Create `docs/ga-ready/evidence/post-04262-worktree-cleanup-2026-07-13.md`: record removed and preserved worktrees.
- Modify `AGENTS.md`, `docs/ga-ready/EVIDENCE_INDEX.md`, `docs/ga-ready/CONTROL_PLANE_INDEX.md`, and `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`: publish the post-merge rollup without opening a package candidate.

### Task 1: Add a failing current-ledger consistency contract

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1:5124-5196`
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: Add assertions for every current row that became stale**

Append these assertions inside `It 'records 0.42.62 WMI topology recovery current evidence'` after the existing `$ledger` assertions:

```powershell
$ledger | Should -Match '(?m)^\| `full-admin-host-mutation-current` \| `pass`, `0\.42\.62-admin-smoke`'
$ledger | Should -Match '(?m)^\| `manual-admin-package-pair-current` \| `pass`, `0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke`'
$ledger | Should -Match '(?m)^\| `package-build-current` \| `package-build-pass`, `0\.42\.62-admin-smoke`'
$ledger | Should -Match '(?m)^\| `latest-product-payload-smoke` \| `pass`, package `0\.42\.62-admin-smoke`'
$ledger | Should -Match '(?m)^\| `installed-operator-surface-smoke-latest` \| `pass`, installed `0\.42\.62-admin-smoke`'
$ledger | Should -Match '(?m)^\| `manual-admin-package-pair-latest-candidate` \| `pass-closed`, `0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke`'
$ledger | Should -Match '(?m)^\| `operator-surface-account-novnc-current` \| `pass`, installed `0\.42\.58-admin-smoke`'
$ledger | Should -Match 'current_manual_admin_update_zip_owner:\s*`0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke`'
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -FullName '*0.42.62 WMI topology recovery current evidence*' -Output Detailed
```

Expected: FAIL because the current table still contains `0.42.57` rows and the update ZIP owner field is absent.

### Task 2: Align the current evidence table and make the ledger contract green

**Files:**
- Modify: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md:14-45,643-659`
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: Declare the manual-admin update ZIP owner**

Add this metadata immediately after `current_manual_admin_update_zip_sha256`:

```markdown
current_manual_admin_update_zip_owner: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
```

- [ ] **Step 2: Replace stale current rows with the verified anchors**

Use these exact values in the corresponding table rows:

```markdown
| `full-admin-host-mutation-current` | `pass`, `0.42.62-admin-smoke` | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04262-hostmutation.md`; `artifacts/batch-runs/full-admin-host-mutation-gate-20260713-04262` | Elevated full gate completed route and OS mutation steps `2/2`; public trusted signing and external stable publication remain excluded/not-claimed. |
| `manual-admin-package-pair-current` | `pass`, `0.42.58-admin-smoke -> 0.42.59-admin-smoke` | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`; descriptor `manual-admin-campaign-descriptor-20260529-04258-04259-closed`; update ZIP SHA-256 `05951af066f0080c9c111de7e104fc8a9418812b68ca0fb246a573d89b6e44fb` | This row intentionally trails the `0.42.62` package/full-gate anchor until a dedicated baseline campaign closes or records a blocker. |
| `package-build-current` | `package-build-pass`, `0.42.62-admin-smoke` | `docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md`; `artifacts/admin-smoke-package-20260713-04262`; MSI SHA-256 `ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533` | WMI internal-switch topology recovery payload; operational full-gate MSI SHA-256 is `c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`. |
| `latest-product-payload-smoke` | `pass`, package `0.42.62-admin-smoke` | `docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md`; `artifacts/admin-smoke-package-20260713-04262`; payload aggregate SHA-256 `0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e` | The package closes the `0.42.60` topology-incomplete and `0.42.61` missing-WMI-path predecessors. |
| `installed-operator-surface-smoke-latest` | `pass`, installed `0.42.62-admin-smoke` | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04262.md`; `artifacts/installed-operator-surface-current-card-20260713-04262/summary.json` | CLI `5/5`, TUI `2/2`, Web `3/3`; Default and WSL firewall switches are internal with management OS enabled. |
| `manual-admin-package-pair-latest-candidate` | `pass-closed`, `0.42.58-admin-smoke -> 0.42.59-admin-smoke` | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`; descriptor `manual-admin-campaign-descriptor-20260529-04258-04259-closed` | Readiness, update/rollback, Windows Update clean-host, Burn, MSIX, runtime ops all passed with `missing_count=0`, `not_pass_count=0`. |
| `operator-surface-account-novnc-current` | `pass`, installed `0.42.58-admin-smoke` | `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`; `artifacts/installed-account-login-smoke-20260529-04258/summary.json`; `artifacts/target-backed-novnc-installed-streaming-smoke-20260529-04258-r2/summary.json` | Carry-forward is valid because no account/noVNC payload changed after `0.42.58`. |
```

- [ ] **Step 3: Run the focused test and verify GREEN**

Run the Task 1 command again.

Expected: PASS, one test passed and zero failed.

- [ ] **Step 4: Commit the ledger correction**

```powershell
git add -- docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "docs: align 0.42.62 current evidence ledger"
```

### Task 3: Add WMI full-projection and path regression tests

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs:854-945`
- Test: `src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj`

- [ ] **Step 1: Add failing query and object-path tests**

Add these tests next to the existing WMI switch provider tests:

```csharp
[Fact]
public void WmiSwitchProviderUsesFullProjectionForAssociationTraversal()
{
    Assert.Equal(
        "SELECT * FROM Msvm_VirtualEthernetSwitch",
        DesktopNodeHyperVWmiSwitchProvider.SwitchQuery);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void WmiSwitchProviderRejectsMissingAssociationTraversalPath(string? objectPath)
{
    var error = Assert.Throws<InvalidOperationException>(() =>
        DesktopNodeHyperVWmiSwitchProvider.EnsureAssociationTraversalPath(objectPath));

    Assert.Contains("association traversal", error.Message, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void WmiSwitchProviderAcceptsCompleteAssociationTraversalPath()
{
    DesktopNodeHyperVWmiSwitchProvider.EnsureAssociationTraversalPath(
        @"\\HOST\root\virtualization\v2:Msvm_VirtualEthernetSwitch.Name=""switch-id""");
}
```

- [ ] **Step 2: Run the focused tests and verify RED**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~WmiSwitchProviderUsesFullProjectionForAssociationTraversal|FullyQualifiedName~WmiSwitchProviderRejectsMissingAssociationTraversalPath|FullyQualifiedName~WmiSwitchProviderAcceptsCompleteAssociationTraversalPath"
```

Expected: compilation FAIL because `SwitchQuery` and `EnsureAssociationTraversalPath` do not exist.

### Task 4: Enforce the association traversal contract

**Files:**
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs:6-100`
- Test: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

- [ ] **Step 1: Add the query constant and guard**

Add the constant at class scope and the guard before `MapSwitch`:

```csharp
public const string SwitchQuery = "SELECT * FROM Msvm_VirtualEthernetSwitch";

public static void EnsureAssociationTraversalPath(string? objectPath)
{
    if (string.IsNullOrWhiteSpace(objectPath))
    {
        throw new InvalidOperationException(
            "WMI switch object path is required for association traversal.");
    }
}
```

- [ ] **Step 2: Use the constant and validate each switch object before `GetRelated`**

Replace the inline query and add the guard immediately after the switch name is read:

```csharp
new ObjectQuery(SwitchQuery)
```

```csharp
EnsureAssociationTraversalPath(item.Path?.Path);
using var relatedPorts = item.GetRelated("Msvm_EthernetSwitchPort");
```

The existing adapter catch block remains the single owner of the structured
`PCV_NETWORK_INVENTORY_FAILED` response.

- [ ] **Step 3: Run the focused tests and verify GREEN**

Run the Task 3 command again.

Expected: all five theory/fact cases pass.

- [ ] **Step 4: Run the whole API test project**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release
```

Expected: zero failures, including the existing external-binding, incomplete-topology, and provider-failure cases.

- [ ] **Step 5: Commit the WMI regression guard**

```powershell
git add -- src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs
git commit -m "test: guard WMI switch association traversal"
```

### Task 5: Add a failing CI trigger deduplication contract

**Files:**
- Create: `packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1:14-18`
- Test: `packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1`

- [ ] **Step 1: Create the cross-workflow trigger contract**

```powershell
Set-StrictMode -Version Latest

Describe 'CI trigger deduplication contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:WorkflowPaths = @(
            (Join-Path $script:RepoRoot '.github\workflows\development-gates.yml'),
            (Join-Path $script:RepoRoot '.github\workflows\public-boundary.yml')
        )
    }

    It 'runs on pull requests, main pushes, and manual dispatch without feature branch push duplication' {
        foreach ($path in $script:WorkflowPaths) {
            $path | Should -Exist
            $workflow = Get-Content -Raw -LiteralPath $path
            $workflow | Should -Match '(?m)^\s*pull_request:\s*$'
            $workflow | Should -Match '(?m)^\s*push:\s*$'
            $workflow | Should -Match '(?m)^\s*-\s*main\s*$'
            $workflow | Should -Match '(?m)^\s*workflow_dispatch:\s*$'
            $workflow | Should -Not -Match "(?m)^\s*-\s*'codex/\*\*'\s*$"
        }
    }
}
```

Change the existing Development Gates assertion from `Should -Match` to
`Should -Not -Match` for the `codex/**` branch pattern.

- [ ] **Step 2: Run the new test and verify RED**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1 -Output Detailed
```

Expected: FAIL for both workflows because both still listen to `push: codex/**`.

### Task 6: Remove duplicate feature-push workflow runs

**Files:**
- Modify: `.github/workflows/development-gates.yml:3-9`
- Modify: `.github/workflows/public-boundary.yml:3-9`
- Modify: `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md:100-111`
- Test: `packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1`
- Test: `packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1`

- [ ] **Step 1: Replace both trigger headers with the deduplicated form**

```yaml
on:
  pull_request:
  push:
    branches:
      - main
  workflow_dispatch:
```

Do not rename workflow or job names.

- [ ] **Step 2: Update the development-gate evidence trigger sentence**

Replace the old `pull request, main, codex/**, manual dispatch` statement with:

```markdown
workflow는 pull request, `main` push, manual dispatch에서 실행한다. 기능 브랜치 push는
같은 head의 pull request run과 중복되므로 제외하며 required job 이름과 비변경 gate matrix는
유지한다.
```

- [ ] **Step 3: Run the trigger contracts and verify GREEN**

```powershell
Invoke-Pester -Path @(
  'packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1',
  'packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1'
) -Output Detailed
```

Expected: both files pass with zero failures.

- [ ] **Step 4: Commit the workflow change**

```powershell
git add -- .github/workflows/development-gates.yml .github/workflows/public-boundary.yml packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1 packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1 docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md
git commit -m "ci: deduplicate feature branch validation"
```

### Task 7: Verify and update PR #171

**Files:**
- Verify only: all changed files in Tasks 1-6

- [ ] **Step 1: Run the full local validation set**

```powershell
dotnet test src/DesktopNode.sln -c Release
Invoke-Pester -Path packaging/windows-desktop-node/tests -Output Detailed
Invoke-Pester -Path @('packaging/windows-desktop-node/installer/tests', 'web/tests') -Output Detailed
npm test --prefix web
npm run verify:parity --prefix web
git diff --check origin/main...HEAD
```

Expected: every command exits `0`; .NET total is at least `739`; Pester has zero failures; Web tests and parity pass; diff check is empty.

- [ ] **Step 2: Push the PR branch without force**

```powershell
git status --short --branch
git push origin codex/wmi-internal-switch-topology-recovery
```

Expected: clean worktree and a normal fast-forward push.

- [ ] **Step 3: Update the PR description and wait for checks**

Use `gh pr edit 171` to add the ledger consistency, WMI query/path guard, and CI deduplication bullets to the existing summary and validation sections. Then run:

```powershell
gh pr checks 171 --watch --interval 10
```

Expected: one Development Gates set and one Public Boundary Contract set for the `pull_request` event; all checks SUCCESS.

- [ ] **Step 4: Mark ready and merge only after re-reading PR state**

```powershell
gh pr ready 171
gh pr view 171 --json state,isDraft,mergeable,reviewDecision,statusCheckRollup,headRefOid,baseRefName
gh pr merge 171 --merge --delete-branch=false
```

Expected before merge: `OPEN`, `isDraft=false`, `MERGEABLE`, base `main`, all required conclusions SUCCESS. Expected after merge: PR state `MERGED`.

- [ ] **Step 5: Fast-forward local main**

```powershell
git switch main
git pull --ff-only origin main
git status --short --branch
```

Expected: clean `main` at the merged remote head.

### Task 8: Capture the merged main public-boundary run

**Files:**
- Create: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md`

- [ ] **Step 1: Wait for both main-push workflows**

```powershell
$head = git rev-parse HEAD
$publicRun = gh run list --commit $head --event push --workflow public-boundary.yml --limit 1 --json databaseId,status,conclusion,url,headSha | ConvertFrom-Json | Select-Object -First 1
$developmentRun = gh run list --commit $head --event push --workflow development-gates.yml --limit 1 --json databaseId,status,conclusion,url,headSha | ConvertFrom-Json | Select-Object -First 1
gh run watch $publicRun.databaseId --exit-status
gh run watch $developmentRun.databaseId --exit-status
```

Expected: both exit `0` and both `headSha` values equal `$head`.

- [ ] **Step 2: Capture exact job IDs and create the follow-up branch**

```powershell
$public = gh run view $publicRun.databaseId --json databaseId,headSha,url,conclusion,jobs | ConvertFrom-Json
$development = gh run view $developmentRun.databaseId --json databaseId,headSha,url,conclusion,jobs | ConvertFrom-Json
$publicJob = $public.jobs | Where-Object name -eq 'public-boundary-ci-required' | Select-Object -First 1
git switch -c codex/post-04262-evidence-followup
```

Expected: `$public.conclusion` and `$development.conclusion` are `success`; `$publicJob.databaseId` is non-null.

- [ ] **Step 3: Write the post-merge evidence with the captured values**

Create `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md` with:

```markdown
# Public boundary CI main push 2026-07-13 PR #171 post-merge

result: `PASS`
scope: `post-04262-pr171-main-push`
workflow: `Public Boundary Contract`
job_name: `public-boundary-ci-required`
host_mutation_performed: `false`
product_payload_change_detected: `false`
source_version_anchor: `0.42.62-admin-smoke`
additional_package_candidate_opened: `false`
package_candidate_decision: `docs-only-followup-does-not-open-0.42.63`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
```

Add the actual `$public.databaseId`, `$publicJob.databaseId`, `$public.headSha`, `$public.url`, and merge commit title as literal fields before committing. Do not write a PASS document if either workflow conclusion is not `success`.

### Task 9: Resolve the manual-admin package-pair honestly

**Files:**
- Create: `docs/ga-ready/evidence/manual-admin-campaign-2026-07-13-04259-04262.md`
- Artifact: `artifacts/manual-admin-campaign-20260713-04259-04262/**`

- [ ] **Step 1: Verify immutable inputs and prepare update packages mechanically**

```powershell
$campaign = 'artifacts/manual-admin-campaign-20260713-04259-04262'
$baselineRoot = 'artifacts/admin-smoke-package-20260529-04259'
$targetRoot = 'artifacts/admin-smoke-package-20260713-04262'
$baselineMsi = Join-Path $baselineRoot 'PureCVisorDesktopNode-0.42.59-admin-smoke-windows-x64.msi'
$targetMsi = Join-Path $targetRoot 'PureCVisorDesktopNode-0.42.62-admin-smoke-windows-x64.msi'
$baseVhd = 'artifacts/image-cache/windows-server-2022-eval-vhd/20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd'
@($baselineMsi,$targetMsi,$baseVhd) | ForEach-Object { if (-not (Test-Path -LiteralPath $_ -PathType Leaf)) { throw "Missing campaign input: $_" } }
New-Item -ItemType Directory -Force -Path (Join-Path $campaign 'lifecycle') | Out-Null
Compress-Archive -Path (Join-Path $baselineRoot 'payload\*') -DestinationPath (Join-Path $campaign 'lifecycle\PureCVisorDesktopNode-0.42.59-admin-smoke-baseline-align.zip') -CompressionLevel Optimal
Compress-Archive -Path (Join-Path $targetRoot 'payload\*') -DestinationPath (Join-Path $campaign 'lifecycle\PureCVisorDesktopNode-0.42.62-admin-smoke-update.zip') -CompressionLevel Optimal
```

Expected: both ZIPs contain the nine payload entries and their SHA-256 values are recorded in the campaign summary.

- [ ] **Step 2: Run non-mutating readiness first**

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 `
  -ArtifactRoot "$campaign/manual-admin-rebaseline-readiness" `
  -BaselineVersion '0.42.59-admin-smoke' `
  -TargetVersion '0.42.62-admin-smoke' `
  -RouteParityArtifactRoot 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260529-04259' `
  -TargetPackageArtifactRoot $targetRoot `
  -PlanOnly
```

Expected on the current `0.42.62` host: `blocked-by-installed-baseline-version-mismatch`. This is evidence that the current host must not be downgraded.

- [ ] **Step 3: Discover an explicitly configured dedicated baseline host**

```powershell
$baselineHost = [Environment]::GetEnvironmentVariable('PCV_MANUAL_ADMIN_BASELINE_HOST','Process')
$secondaryCredentialRef = [Environment]::GetEnvironmentVariable('PCV_MANUAL_ADMIN_CREDENTIAL_REF','Process')
$dedicatedReady = -not [string]::IsNullOrWhiteSpace($baselineHost) -and -not [string]::IsNullOrWhiteSpace($secondaryCredentialRef)
```

If `$dedicatedReady` is false, stop all mutation buckets and write result
`BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED`; list present MSI/VHD/ZIP inputs, readiness summary path, current installed version, and `host_mutation_performed=false`.

If `$dedicatedReady` is true, use only that host and the configured credential reference to run installed update/rollback, clean-host Windows Update, Burn, MSIX, runtime ops, and descriptor v2. Do not put credential values in command lines or artifacts. The descriptor must end with `missing_count=0`, `not_pass_count=0`; otherwise record `BLOCKED_OR_FAILED`, never PASS.

- [ ] **Step 4: Create the canonical manual-admin evidence**

The evidence must contain exactly one of these outcomes:

```markdown
result: `PASS`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
host_mutation_performed: `true-dedicated-baseline-host`
```

or:

```markdown
result: `BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED`
blocker: `blocked-by-installed-baseline-version-mismatch-and-no-configured-dedicated-host`
host_mutation_performed: `false`
```

Both outcomes must keep `public_trusted_signing: not-claimed` and
`external_stable_publication: not-claimed`.

### Task 10: Run or block the secondary Hyper-V topology smoke

**Files:**
- Create: `docs/ga-ready/evidence/secondary-hyperv-wmi-topology-smoke-2026-07-13-04262.md`

- [ ] **Step 1: Discover a configured secondary host without mutating it**

```powershell
$secondaryHost = [Environment]::GetEnvironmentVariable('PCV_SECONDARY_HYPERV_HOST','Process')
$secondaryCredentialRef = [Environment]::GetEnvironmentVariable('PCV_SECONDARY_HYPERV_CREDENTIAL_REF','Process')
$secondaryReady = -not [string]::IsNullOrWhiteSpace($secondaryHost) -and -not [string]::IsNullOrWhiteSpace($secondaryCredentialRef)
```

- [ ] **Step 2: Execute only read-only inventory when configured**

When `$secondaryReady` is true, resolve the credential through the existing protected reference mechanism and run a read-only remote command containing only:

```powershell
Get-CimInstance -Namespace root/virtualization/v2 -ClassName Msvm_VirtualEthernetSwitch |
  Select-Object ElementName,Name,__PATH
Get-VMSwitch | Select-Object Name,SwitchType,AllowManagementOS
```

Then run the installed network inventory/current-card client against that host's already configured listener. Do not create/delete switches or VMs and do not alter service, firewall, trust store, Event Log, or listener settings.

Run these commands inside the same read-only remote session:

```powershell
& 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe' --json network inventory
& 'C:\Program Files\PureCVisor\DesktopNode\pcvtui.exe' --smoke-once net
Invoke-WebRequest -UseBasicParsing -Uri 'http://127.0.0.1/' -TimeoutSec 10 |
  Select-Object StatusCode
```

Expected: both executables exit `0`, the Web Console returns `200`, and the JSON inventory contains no switch with an internally claimed topology unless WMI association evidence supports it.

When `$secondaryReady` is false, create the evidence with:

```markdown
result: `BLOCKED_NO_SECONDARY_HYPERV_HOST`
blocker: `blocked-no-secondary-hyperv-host-or-protected-credential-reference`
host_mutation_performed: `false`
single_host_pass_promoted_to_multi_host: `false`
```

- [ ] **Step 3: Assert secret-free evidence**

```powershell
$evidence = Get-Content -Raw docs/ga-ready/evidence/secondary-hyperv-wmi-topology-smoke-2026-07-13-04262.md
$evidence | Should -Not -Match '(?i)password\s*[:=]\s*[^`\s]+'
$evidence | Should -Not -Match '(?i)bearer\s+[A-Za-z0-9._-]+'
```

Expected: both assertions pass.

### Task 11: Audit and safely clean historical worktrees

**Files:**
- Create: `docs/ga-ready/evidence/post-04262-worktree-cleanup-2026-07-13.md`

- [ ] **Step 1: Produce a fresh read-only audit**

For each entry from `git worktree list --porcelain`, capture resolved path, branch, `git status --porcelain`, `git merge-base --is-ancestor $branch main`, and `git cherry main $branch`. Preserve the repository root, the active follow-up branch, any path outside the repository `.worktrees` directory, any dirty worktree, and any branch with `git cherry` `+` entries.

- [ ] **Step 2: Verify every removal path before mutation**

```powershell
$repo = (Resolve-Path '.').Path
$boundary = [IO.Path]::GetFullPath((Join-Path $repo '.worktrees'))
$candidate = [IO.Path]::GetFullPath($candidatePath)
if (-not $candidate.StartsWith($boundary + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) {
    throw "Worktree is outside cleanup boundary: $candidate"
}
```

- [ ] **Step 3: Remove only clean merged or cherry-equivalent candidates**

Run one candidate at a time:

```powershell
git worktree remove $candidate
git branch -d $candidateBranch
```

If `git branch -d` refuses a cherry-equivalent branch, preserve the branch and record
`worktree-removed-branch-preserved-non-ancestor`; do not use `-D`.

- [ ] **Step 4: Record removed and preserved entries**

The evidence must include counts and one row per audited worktree with category, dirty count, ancestor result, cherry `+/-` counts, action, and reason. It must state `host_mutation_performed=false` and must not claim deletion for preserved paths.

### Task 12: Add follow-up evidence guards and publish the docs-only branch

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
- Modify: `AGENTS.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Modify: `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- Modify: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: Add a failing evidence-presence contract before index edits**

Add this Pester test:

```powershell
It 'records the post 0.42.62 PR171 operational follow-up without opening a package' {
    $relativePaths = @(
        'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md',
        'docs/ga-ready/evidence/manual-admin-campaign-2026-07-13-04259-04262.md',
        'docs/ga-ready/evidence/secondary-hyperv-wmi-topology-smoke-2026-07-13-04262.md',
        'docs/ga-ready/evidence/post-04262-worktree-cleanup-2026-07-13.md'
    )
    $contents = foreach ($relativePath in $relativePaths) {
        $path = Join-Path $script:RepoRoot $relativePath
        $path | Should -Exist
        Get-Content -Raw -LiteralPath $path
    }

    $contents[0] | Should -Match 'result:\s*`PASS`'
    $contents[0] | Should -Match 'run_id:\s*`\d+`'
    $contents[0] | Should -Match 'job_id:\s*`\d+`'
    $contents[0] | Should -Match 'head_sha:\s*`[a-f0-9]{40}`'
    $contents[0] | Should -Match 'additional_package_candidate_opened:\s*`false`'
    $contents[1] | Should -Match 'result:\s*`(PASS|BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED)`'
    $contents[2] | Should -Match 'result:\s*`(PASS|BLOCKED_NO_SECONDARY_HYPERV_HOST)`'
    $contents[3] | Should -Match 'removed_worktree_count:\s*`\d+`'
    $contents[3] | Should -Match 'preserved_worktree_count:\s*`\d+`'

    foreach ($content in $contents) {
        $content | Should -Match 'public_trusted_signing:\s*`(excluded|not-claimed)`'
        $content | Should -Match 'external_stable_publication:\s*`not-claimed`'
    }

    foreach ($index in @(
        (Get-RepoText -RelativePath 'AGENTS.md'),
        (Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'),
        (Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'),
        (Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md')
    )) {
        foreach ($relativePath in $relativePaths) {
            $index | Should -Match ([regex]::Escape((Split-Path -Leaf $relativePath)))
        }
    }
}
```

- [ ] **Step 2: Run the focused contract and verify RED**

Expected: FAIL because indexes and ledger do not yet reference the four files.

- [ ] **Step 3: Update current indexes and ledger with actual outcomes**

Promote the PR #171 main-push head/run/job as current public-boundary. Keep package/full-gate/current-card at `0.42.62`. Promote manual-admin only when its result is PASS; when blocked, keep `0.42.58 -> 0.42.59` current and link the blocker as a follow-up. Record secondary-host and cleanup outcomes without opening `0.42.63`.

- [ ] **Step 4: Run documentation and full verification**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
dotnet test src/DesktopNode.sln -c Release
git diff --check origin/main...HEAD
git status --short --branch
```

Expected: zero failures, empty diff-check output, and only intended tracked changes.

- [ ] **Step 5: Commit, push, and create the docs/evidence PR**

```powershell
git add -- AGENTS.md docs/ga-ready packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "docs: record post-0.42.62 operational follow-up"
git push -u origin codex/post-04262-evidence-followup
```

Create a draft PR targeting `main` with the four outcome documents, validation results, and explicit statement that no new product package, public trusted signing, or external stable publication is claimed.

## Final verification checklist

- [ ] PR #171 merged only after one deduplicated PR check set passed.
- [ ] Main push Development Gates and Public Boundary Contract passed on the merge head.
- [ ] Ledger table and metadata agree on package/full-gate/current-card/manual-admin owners.
- [ ] WMI query/path tests failed before implementation and passed after it.
- [ ] Manual-admin is PASS only with all six buckets and descriptor zero counts; otherwise blocker is explicit.
- [ ] Secondary-host result is not inferred from the local host.
- [ ] No dirty, unique, active, external-path, or Codex-owned worktree was removed.
- [ ] Follow-up branch is pushed and its draft PR URL is reported.
