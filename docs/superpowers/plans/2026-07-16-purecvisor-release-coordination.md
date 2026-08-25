# PureCVisor Release Coordination Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reserve an N-1 manual-admin baseline before host mutation and merge an explicitly approved PR only after its approved SHA passes both named workflows.

**Architecture:** An immutable reservation descriptor binds campaign, versions, hashed host identity, and dedicated-host/checkpoint reference before existing readiness opens mutation runners. A local operator watcher polls GitHub through an injected command runner and invokes `gh pr merge --match-head-commit` once; it adds no GitHub Actions write token or unattended bot.

**Tech Stack:** PowerShell 7.6, Pester 5.7.1, GitHub CLI, JSON Schema, existing manual-admin runners.

---

Prerequisites: Slice A Full/Release summary and Slice B change-tier/evidence checks.
Source design: `docs/superpowers/specs/2026-07-16-purecvisor-desktop-node-development-throughput-automation-design.md`.

## File map

- Create `packaging/windows-desktop-node/schemas/manual-admin-baseline-reservation.schema.json`.
- Create `packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1`.
- Create `packaging/windows-desktop-node/tools/New-PcvManualAdminBaselineReservation.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1`.
- Modify `New-PcvManualAdminRebaselineReadiness.ps1` and its tests to require a reservation for execution.
- Create `packaging/windows-desktop-node/tools/PcvPullRequestMergeWatcher.psm1`.
- Create `packaging/windows-desktop-node/tools/Wait-PcvPullRequestGreenAndMerge.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvPullRequestMergeWatcher.Tests.ps1`.
- Update operations/verification docs and add code-level evidence.

### Task 1: Define and create baseline reservations

**Files:**
- Create: `packaging/windows-desktop-node/schemas/manual-admin-baseline-reservation.schema.json`
- Create: `packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1`
- Create: `packaging/windows-desktop-node/tools/New-PcvManualAdminBaselineReservation.ps1`
- Test: `packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1`

- [ ] **Step 1: Write failing reservation tests**

```powershell
BeforeAll { Import-Module (Join-Path $PSScriptRoot '../tools/PcvManualAdminBaselineReservation.psm1') -Force }
Describe 'New-PcvManualAdminBaselineReservationRecord' {
    It 'binds the campaign to the installed N-1 version without exposing host identity' {
        $r=New-PcvManualAdminBaselineReservationRecord -CampaignId c-04264-04265 `
          -BaselineVersion 0.42.64-admin-smoke -TargetVersion 0.42.65-admin-smoke `
          -InstalledVersion 0.42.64-admin-smoke -HostIdentity 'host-secret' `
          -ReservationKind dedicated-host -ResourceReference 'lab-host-a' `
          -Now ([datetimeoffset]'2026-07-16T00:00:00Z') -ExpiresAt ([datetimeoffset]'2026-07-18T00:00:00Z')
        $r.status | Should -Be reserved
        $r.host_fingerprint_sha256 | Should -Match '^[0-9a-f]{64}$'
        ($r|ConvertTo-Json -Depth 8) | Should -Not -Match 'host-secret'
    }
    It 'rejects an installed version that is not the requested baseline' {
        { New-PcvManualAdminBaselineReservationRecord -CampaignId c -BaselineVersion 0.42.62-admin-smoke `
          -TargetVersion 0.42.63-admin-smoke -InstalledVersion 0.42.63-admin-smoke `
          -HostIdentity h -ReservationKind dedicated-host -ResourceReference lab `
          -Now ([datetimeoffset]'2026-07-16T00:00:00Z') -ExpiresAt ([datetimeoffset]'2026-07-18T00:00:00Z') } |
          Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_VERSION_MISMATCH*'
    }
}
```

- [ ] **Step 2: Run focused Pester and verify RED**

Expected: module/functions missing.

- [ ] **Step 3: Add schema and module**

The schema requires `schema_version=1`, contract `pcv-manual-admin-baseline-reservation-v1`, UUID reservation ID, campaign ID, distinct baseline/target, 64-char host fingerprint, kind enum `dedicated-host|hyperv-checkpoint`, non-empty resource reference, installed version, ISO timestamps, and state enum `reserved|consumed|released`. It disallows raw computer name, MachineGuid and credentials.

Implement SHA-256 with UTF-8 bytes and return this record:

```powershell
[ordered]@{schema_version=1;contract='pcv-manual-admin-baseline-reservation-v1';
reservation_id=[guid]::NewGuid().ToString();campaign_id=$CampaignId;baseline_version=$BaselineVersion;
target_version=$TargetVersion;host_fingerprint_sha256=(Get-PcvSha256 $HostIdentity);
reservation_kind=$ReservationKind;resource_reference=$ResourceReference;
installed_version_at_reservation=$InstalledVersion;created_at=$Now.ToUniversalTime().ToString('o');
expires_at=$ExpiresAt.ToUniversalTime().ToString('o');status='reserved'}
```

Throw stable codes for version mismatch, invalid version order, expiry not after creation, invalid resource, and invalid schema.

- [ ] **Step 4: Add the CLI wrapper**

Parameters include campaign/baseline/target, kind/reference, installed manifest path override, artifact root, expiry hours, and PlanOnly. Default host identity is SHA input composed from Windows MachineGuid and computer name but neither raw value is serialized. PlanOnly returns the intended descriptor without writing. Normal mode writes `reservation.json` atomically and refuses overwrite.

- [ ] **Step 5: Verify GREEN and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1 -Output Detailed
git add packaging/windows-desktop-node/schemas/manual-admin-baseline-reservation.schema.json packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1 packaging/windows-desktop-node/tools/New-PcvManualAdminBaselineReservation.ps1 packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1
git commit -m "feat: reserve manual admin baseline resources"
```

### Task 2: Guard manual-admin readiness with the reservation

**Files:**
- Modify: `packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1`

- [ ] **Step 1: Add failing readiness tests**

Test missing reservation under actual execution, campaign mismatch, baseline/target mismatch, host fingerprint mismatch, expired, consumed, and a valid reserved descriptor. PlanOnly without host mutation may report `reservation-required-before-actual-execution` but must not synthesize PASS.

- [ ] **Step 2: Verify RED**

Expected: readiness currently accepts no reservation parameter.

- [ ] **Step 3: Implement reservation validation**

Add `Test-PcvManualAdminBaselineReservation` with expected campaign/baseline/target/fingerprint and Now. It returns the parsed record or throws:

- `PCV_MANUAL_ADMIN_BASELINE_RESERVATION_REQUIRED`
- `PCV_MANUAL_ADMIN_BASELINE_RESERVATION_MISMATCH`
- `PCV_MANUAL_ADMIN_BASELINE_RESERVATION_EXPIRED`
- `PCV_MANUAL_ADMIN_BASELINE_RESERVATION_CONSUMED`

Add `-BaselineReservationPath` to readiness. Validate before generating or invoking any clean-host, Burn, MSIX, update or rollback runner command.

- [ ] **Step 4: Add consumed transition**

`Set-PcvManualAdminBaselineReservationState` creates a new `reservation-consumed.json` next to the immutable reservation after the campaign runner accepts target installation. It never edits the original reservation. Readiness treats a matching consumed sidecar as consumed.

- [ ] **Step 5: Verify no mutation on failure and commit**

Focused tests must assert runner call count zero for every mismatch. Run PlanOnly against the current 0.42.64 installed manifest with a synthetic 0.42.64->0.42.65 reservation artifact; do not create a VM/checkpoint or run host mutation.

```powershell
git add packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1 packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1
git commit -m "feat: require a baseline reservation before manual admin mutation"
```

### Task 3: Implement the safe PR state evaluator

**Files:**
- Create: `packaging/windows-desktop-node/tools/PcvPullRequestMergeWatcher.psm1`
- Test: `packaging/windows-desktop-node/tests/PcvPullRequestMergeWatcher.Tests.ps1`

- [ ] **Step 1: Write failing evaluator tests**

Use an injected `GhRunner` that returns JSON for `gh pr view` and `gh run list`. Test open/non-draft/main/same-owner, approved SHA, workflow success, failed/missing/in-progress workflows, merge conflict, and SHA drift. Do not invoke real GitHub.

```powershell
It 'rejects a changed head SHA before workflow evaluation' {
    $state=Test-PcvApprovedPullRequestState -PullRequest $script:Pr -WorkflowRuns @() `
      -Repository [private-archive-repository] -ApprovedHeadSha ('a'*40) `
      -RequiredWorkflow @('Development Gates','Public Boundary Contract')
    $state.code | Should -Be 'PCV_PR_HEAD_CHANGED'
    $state.ready_to_merge | Should -BeFalse
}
```

- [ ] **Step 2: Verify RED**

Expected: module/evaluator missing.

- [ ] **Step 3: Implement pure state evaluation**

Return `[ordered]@{ready_to_merge;code;detail;observed_head_sha;workflow_status}`. The accepted PR is open, non-draft, base main, head owner equal to repository owner, head SHA exact, mergeable `MERGEABLE`, and the latest run for each required workflow on that SHA has `status=completed` and `conclusion=success`. `UNKNOWN` mergeability is waitable; conflict is terminal.

- [ ] **Step 4: Verify GREEN and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvPullRequestMergeWatcher.Tests.ps1 -Output Detailed
git add packaging/windows-desktop-node/tools/PcvPullRequestMergeWatcher.psm1 packaging/windows-desktop-node/tests/PcvPullRequestMergeWatcher.Tests.ps1
git commit -m "feat: evaluate approved pull request merge state"
```

### Task 4: Poll and merge exactly the approved SHA

**Files:**
- Modify: `packaging/windows-desktop-node/tools/PcvPullRequestMergeWatcher.psm1`
- Create: `packaging/windows-desktop-node/tools/Wait-PcvPullRequestGreenAndMerge.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvPullRequestMergeWatcher.Tests.ps1`

- [ ] **Step 1: Add failing polling tests**

Test in-progress then success, SHA change during polling, workflow failure, timeout, PlanOnly, and exactly one merge call. Inject `NowProvider` and `WaitAction` so tests do not sleep.

- [ ] **Step 2: Implement GitHub queries**

Default runner executes:

```text
gh pr view <n> --repo <repo> --json number,state,isDraft,baseRefName,headRefOid,headRepositoryOwner,mergeable,url
gh run list --repo <repo> --commit <approved-sha> --json name,status,conclusion,event,headSha,createdAt,url
```

Parse JSON only when exit code is zero; redact stderr and return `PCV_GITHUB_QUERY_FAILED` otherwise.

- [ ] **Step 3: Implement condition polling**

`Wait-PcvPullRequestGreenAndMerge` accepts repository, PR, approved SHA, required workflows, poll seconds, timeout minutes, PlanOnly, runner, now provider and wait action. Terminal failures return immediately. Waitable states are incomplete workflows and mergeability UNKNOWN. Timeout returns `PCV_PR_MERGE_WATCH_TIMEOUT`.

- [ ] **Step 4: Implement one guarded merge call**

When ready and not PlanOnly, invoke exactly:

```text
gh pr merge <n> --repo <repo> --merge --match-head-commit <approved-sha>
```

Do not use `--admin`, `--auto`, `--delete-branch`, a write-enabled workflow, or a stored token. Re-query PR once and require state `MERGED`; write a summary containing PR URL, approved SHA, merge result and observed workflows.

- [ ] **Step 5: Add the thin wrapper and verify GREEN**

The script imports the module, defaults required workflows to `Development Gates` and `Public Boundary Contract`, writes `summary.json` under ArtifactRoot, and exits nonzero for every non-merged terminal result.

- [ ] **Step 6: Commit watcher**

```powershell
git add packaging/windows-desktop-node/tools/PcvPullRequestMergeWatcher.psm1 packaging/windows-desktop-node/tools/Wait-PcvPullRequestGreenAndMerge.ps1 packaging/windows-desktop-node/tests/PcvPullRequestMergeWatcher.Tests.ps1
git commit -m "feat: merge an approved green pull request SHA"
```

### Task 5: Document and verify Slice C

**Files:**
- Modify: `docs/OPERATIONS_GUIDE.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Create: `docs/ga-ready/evidence/release-coordination-code-level-2026-07-16.md`

- [ ] **Step 1: Document operator flows**

Show reservation creation, readiness PlanOnly with reservation, consumed sidecar ownership, watcher PlanOnly, and watcher actual invocation. State that reservation does not provision a host/checkpoint and watcher invocation itself is the one-time operator approval.

- [ ] **Step 2: Run focused safety tests**

Run reservation/readiness/watcher suites. Assert no host mutation command on invalid reservation and no merge command on changed SHA or missing/failed workflows.

- [ ] **Step 3: Run the Full/Release non-mutating gates**

Run Full and Release PlanOnly, all Pester, .NET, Web parity, current evidence Check and `git diff --check`. No new admin-smoke, install, checkpoint or host mutation is allowed.

- [ ] **Step 4: Record code-level evidence and commit**

Evidence records schema IDs, safety cases, exact workflow names, GitHub private Free constraint, `host_mutation_performed=false`, `pull_request_merge_performed=false` for tests, and no public claims.

```powershell
git add docs/OPERATIONS_GUIDE.md docs/DEVELOPMENT_VERIFICATION_POLICY.md packaging/windows-desktop-node/README.md docs/ga-ready/evidence/release-coordination-code-level-2026-07-16.md
git commit -m "docs: record release coordination verification"
```

### Task 6: Publish the branch and use the watcher on its own PR

**Files:**
- No product file changes; generated runtime artifact only.

- [ ] **Step 1: Run final full verification and review the diff**

Require every Slice A/B/C acceptance criterion, no uncommitted tracked files, no TUI reintroduction, and no host mutation.

- [ ] **Step 2: Push and create a ready PR**

Push `codex/development-throughput-automation`, create a PR against main, and record the exact head SHA after the final push.

- [ ] **Step 3: Invoke watcher PlanOnly**

Use the actual PR number and approved head SHA. Expected: it names both workflows and produces no merge call.

- [ ] **Step 4: Invoke watcher actual mode after operator approval**

Poll until both workflows succeed; merge only with `--match-head-commit`; preserve the watcher summary artifact. If the head changes, stop and request a new approval instead of merging.

- [ ] **Step 5: Verify main and post-merge CI**

Fetch origin/main, confirm it contains the approved head ancestry/merge commit, and confirm post-merge Development Gates and Public Boundary Contract pass.
