# 0.38.2 Canonical Evidence Closure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `0.38.2-admin-smoke` the only canonical standalone latest full admin host mutation evidence baseline while keeping `0.38.1` only as historical ledger/reference material.

**Architecture:** This is a documentation, fixture, and guard closure batch. It does not change product runtime behavior and does not run host mutation. Canonical values flow from the actual `0.38.2` Batch Supervisor artifacts into docs, Pester guards, API unit fixtures, and Web Console parity fixtures.

**Tech Stack:** Markdown docs, PowerShell 7, Pester 5, C# xUnit tests, TypeScript/Web fixture scripts, Node verification scripts.

## 실행 종료 정리

- 상태: 완료.
- 구현 commits: `ce71077`, `0734caa`
- 후속 병합 commits: Batch 2 PR `#4` merge `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940`, closure PR `#6` merge `6b375486c0c1aadd9f1cb52b2ee118f4f8a2945f`
- 종료 evidence: `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`

이 plan의 세부 commit step은 작업 중 분리 commit 대신 후속 main history와 closure PR로 정리됐다. 결과 기준으로 `0.38.2-admin-smoke`는 최신 canonical standalone full admin host mutation evidence이고, `0.38.1`은 ledger/history reference로만 남는다. 실제 host mutation, signed MSI build, LocalMachine trust import, public trusted signing, 외부 stable publication은 이 closure에서 실행하거나 주장하지 않는다.

---

## File Structure

- Delete: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md`
  - Responsibility: remove the non-canonical standalone `0.38.1` evidence file from the worktree.
- Create or keep: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md`
  - Responsibility: canonical standalone latest full admin host mutation evidence document.
- Modify: `AGENTS.md`
  - Responsibility: repo-local agent guidance latest evidence summary.
- Modify: `README.md`
  - Responsibility: top-level latest evidence summary.
- Modify: `docs/ADR_INDEX.md`
  - Responsibility: ADR-0004 supporting latest evidence pointer.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Responsibility: developer entry point latest evidence table and summary.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Responsibility: latest verification evidence policy.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Responsibility: clarify that `0.38.2` is not public trusted signing or external stable publication evidence.
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
  - Responsibility: post-closure latest evidence note.
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
  - Responsibility: keep `0.38.1` only as historical row and mark `0.38.2` as latest post-snapshot row.
- Modify: `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-next-batch-sequence-0380-evidence-to-release.md`
  - Responsibility: mark the old `0.38.0` sequence plan as superseded by the approved `0.38.2` sequence design.
- Modify: `packaging/windows-desktop-node/README.md`
  - Responsibility: product wrapper evidence summary and Batch Supervisor section.
- Modify: `packaging/windows-desktop-node/installer/README.md`
  - Responsibility: installer MSI lifecycle latest evidence summary.
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
  - Responsibility: Pester guard for canonical evidence and high-level doc references.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Responsibility: API batch evidence fixture values.
- Modify: `web/src/user-visible-fixtures.ts`
  - Responsibility: Web Console user-visible fixture values.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - Responsibility: Web fixture verification expectations.

## Canonical Values

Use these values exactly:

```text
version: 0.38.2-admin-smoke
batch_id: full-admin-host-mutation-gate-20260506-145506-0382
batch_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382
route_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382
os_root: artifacts/os-mutation-gates-batch-profile-20260506-145506-0382
provenance_commit: d05d395e96d5d8d83b4cc4310c2b8ef11253041c
msi_sha256: 4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252
signing_mode: AllowUnsignedDev
public_trusted_signing: excluded
external_stable_publication: not-claimed
service_final_state: Running
firewall_final_count: 0
eventlog_final_source: absent
trust_store_final_state: present
boot_time_status: unchanged
pcv_spike_vm_count: 0
service_msi_hyperv_duration_ms: 108824
os_mutation_duration_ms: 11071
gpu_snapshot_count: 18
gpu_peak_adapter_mib: 3357.76
gpu_peak_process_mib: 16951.19
```

---

### Task 1: Canonical Evidence File Boundary

**Files:**
- Delete: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md`
- Create or keep: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md`

- [x] **Step 1: Inspect evidence files**

Run:

```powershell
Get-ChildItem -LiteralPath 'docs/ga-ready/evidence' -Filter 'full-admin-host-mutation-gate-2026-05-06-038*.md' |
  Select-Object Name,Length,LastWriteTime
```

Expected: `0382.md` exists. `0381.md` may exist before this task and should be removed by this task.

- [x] **Step 2: Delete the standalone 0.38.1 evidence file**

Use `apply_patch`:

```patch
*** Begin Patch
*** Delete File: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md
*** End Patch
```

Expected: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md` no longer exists.

- [x] **Step 3: Verify the 0.38.2 evidence document content**

Run:

```powershell
rg -n "0\.38\.2-admin-smoke|full-admin-host-mutation-gate-20260506-145506-0382|routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382|os-mutation-gates-batch-profile-20260506-145506-0382|4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252" docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md
```

Expected: matches for version, all three artifact roots, and MSI hash.

- [x] **Step 4: Commit Task 1**

Run:

```powershell
git add -- docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md
git commit -m "docs: canonicalize 0.38.2 host mutation evidence"
```

Expected: commit succeeds. If `0382.md` was already tracked in an earlier commit, only the `0381.md` deletion may be staged.

---

### Task 2: High-Level Documentation Pointers

**Files:**
- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `docs/ADR_INDEX.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- Modify: `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-next-batch-sequence-0380-evidence-to-release.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`

- [x] **Step 1: Find stale latest wording**

Run:

```powershell
rg -n "최신[^.\r\n]*(0\.38\.0|0\.38\.1)|Latest[^.\r\n]*(0\.38\.0|0\.38\.1)|current latest[^.\r\n]*(0\.38\.0|0\.38\.1)|현재 최신[^.\r\n]*(0\.38\.0|0\.38\.1)" AGENTS.md README.md docs packaging
```

Expected before edits: stale latest references may appear in `docs/DEVELOPER_INDEX.md` and the old `next-batch-sequence-0380` plan.

- [x] **Step 2: Update current latest evidence summaries**

Use `apply_patch` to replace latest evidence prose in high-level docs with this canonical summary shape:

```markdown
`artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382`, `artifacts/os-mutation-gates-batch-profile-20260506-145506-0382` evidence는 사용자 관리자 opt-in으로 `0.38.2-admin-smoke` full admin host mutation gate를 Batch Supervisor 아래에서 완료한 최신 evidence다. MSI provenance commit은 `d05d395e96d5d8d83b4cc4310c2b8ef11253041c`, MSI SHA-256은 `4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, Service/MSI/Hyper-V step `attempt_count=1`, OS mutation gate `attempt_count=1`, timeout false였고, Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 PASS였다. Final service는 loopback-only `Running`, product manifest version은 `0.38.2-admin-smoke`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `pcv-spike-*` VM count `0`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
```

Apply the same values to `AGENTS.md`, `README.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `packaging/windows-desktop-node/README.md`, and `packaging/windows-desktop-node/installer/README.md`. Keep each file's existing tone and do not remove older historical evidence bullets unless they incorrectly claim to be latest.

- [x] **Step 3: Fix the developer index table entry**

In `docs/DEVELOPER_INDEX.md`, replace the stale table row with:

```markdown
| 최신 full admin host mutation gate evidence 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382/summary.json`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382/summary.json`, `artifacts/os-mutation-gates-batch-profile-20260506-145506-0382/summary.json` |
```

Expected: `docs/DEVELOPER_INDEX.md` no longer points the latest evidence table at `0380`.

- [x] **Step 4: Keep 0.38.1 only as historical ledger material**

In `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`, keep both rows below:

```markdown
| 2026-05-06 | `0.38.1-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-142310-0381`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-142310-0381`, `artifacts/os-mutation-gates-batch-profile-20260506-142310-0381` | PASS | Historical Batch Supervisor full gate before `0.38.2`. Commit `d05d395e96d5d8d83b4cc4310c2b8ef11253041c`, MSI SHA-256 `69e1439f5e12adf6f72ad0e8612e2cef327e1c3e0a800f04934ddb79a136e36e`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, `pcv-spike-*` VM count `0`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.2-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382`, `artifacts/os-mutation-gates-batch-profile-20260506-145506-0382` | PASS | Latest Batch Supervisor full gate. Commit `d05d395e96d5d8d83b4cc4310c2b8ef11253041c`, MSI SHA-256 `4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM count `0`. Public trusted signing/external stable publication excluded. |
```

Expected: `0.38.1` remains visible only as historical evidence in the ledger, not as latest guidance.

- [x] **Step 5: Mark the old 0.38.0 next-batch sequence plan as superseded**

In `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-next-batch-sequence-0380-evidence-to-release.md`, add this note near the top after the header:

```markdown
> Superseded note: this plan captured the next-batch sequence when `0.38.0-admin-smoke` was the latest full admin host mutation evidence. The current canonical sequence baseline is `0.38.2-admin-smoke` and is specified in `docs/superpowers/specs/2026-05-06-purecvisor-desktop-node-next-batch-0382-canonical-evidence-and-evidence-api-hardening-design.md`.
```

Replace any prose that says `0.38.0-admin-smoke` is current/latest with:

```markdown
`0.38.0-admin-smoke` was the latest Batch Supervisor full admin host mutation gate evidence when this historical plan was written. It has been superseded by `0.38.2-admin-smoke`.
```

Expected: the file may still mention `0.38.0` historically, but it must not claim `0.38.0` is current/latest.

- [x] **Step 6: Verify stale latest wording is gone**

Run:

```powershell
rg -n "최신[^.\r\n]*(0\.38\.0|0\.38\.1)|Latest[^.\r\n]*(0\.38\.0|0\.38\.1)|current latest[^.\r\n]*(0\.38\.0|0\.38\.1)|현재 최신[^.\r\n]*(0\.38\.0|0\.38\.1)" AGENTS.md README.md docs packaging
```

Expected: no output. Historical rows that mention `0.38.1` should use wording such as `Historical` or `이전`.

- [x] **Step 7: Commit Task 2**

Run:

```powershell
git add -- AGENTS.md README.md docs/ADR_INDEX.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-next-batch-sequence-0380-evidence-to-release.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md
git commit -m "docs: point latest evidence guidance at 0.38.2"
```

Expected: commit succeeds.

---

### Task 3: Documentation Guard

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [x] **Step 1: Add canonical 0.38.2 assertions**

Ensure the first test uses this exact evidence path and assertions:

```powershell
It 'records the 0.38.2 full admin host mutation gate as canonical evidence' {
    $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0382.md'

    $evidencePath | Should -Exist
    $content = Get-Content -Raw -LiteralPath $evidencePath

    $content | Should -Match '0\.38\.2-admin-smoke'
    $content | Should -Match 'full-admin-host-mutation-gate-20260506-145506-0382'
    $content | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382'
    $content | Should -Match 'os-mutation-gates-batch-profile-20260506-145506-0382'
    $content | Should -Match 'd05d395e96d5d8d83b4cc4310c2b8ef11253041c'
    $content | Should -Match '4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252'
    $content | Should -Match 'AllowUnsignedDev'
    $content | Should -Match 'Service/MSI/Hyper-V'
    $content | Should -Match 'firewall.*Event Log.*trust-store'
    $content | Should -Match 'public trusted signing.*외부 stable publication'
}
```

- [x] **Step 2: Add absence guard for standalone 0.38.1 canonical evidence**

Add this test after the canonical evidence test:

```powershell
It 'does not keep 0.38.1 as a standalone canonical evidence document' {
    $legacyEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md'

    $legacyEvidencePath | Should -Not -Exist
}
```

- [x] **Step 3: Keep high-level doc guard on 0.38.2**

Ensure the high-level docs test contains:

```powershell
$content | Should -Match '0\.38\.2-admin-smoke'
$content | Should -Match 'full-admin-host-mutation-gate-20260506-145506-0382|routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382|os-mutation-gates-batch-profile-20260506-145506-0382'
$content | Should -Match 'd05d395e96d5d8d83b4cc4310c2b8ef11253041c|4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252|AllowUnsignedDev'
```

- [x] **Step 4: Run the focused Pester guard**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

Expected: all tests in `PcvAdminSmokeEvidenceDocs.Tests.ps1` pass.

- [x] **Step 5: Commit Task 3**

Run:

```powershell
git add -- packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "test: guard canonical 0.38.2 evidence docs"
```

Expected: commit succeeds.

---

### Task 4: API and Web Fixture Values

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `web/src/user-visible-fixtures.ts`
- Modify: `web/scripts/verify-browser-fixture.mjs`

- [x] **Step 1: Update API fixture paths and release values**

In `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`, ensure both batch evidence tests use these values:

```csharp
var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-145506-0382");
var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382");
var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-145506-0382");
```

For the redacted-root test, keep the `batch-runs` child:

```csharp
var batchRun = Path.Combine(root, "batch-runs", "full-admin-host-mutation-gate-20260506-145506-0382");
```

Use this provenance JSON in both tests:

```csharp
{"schema_version":"1","product":{"version":"0.38.2-admin-smoke"},"git_commit":"d05d395e96d5d8d83b4cc4310c2b8ef11253041c","msi":{"sha256":"4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252"},"signing_mode":"AllowUnsignedDev"}
```

Use this OS summary JSON in both tests:

```csharp
{"schema_version":1,"ok":true,"version":"0.38.2-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
```

- [x] **Step 2: Update API assertions**

Ensure assertions expect:

```csharp
Assert.Equal("full-admin-host-mutation-gate-20260506-145506-0382", latest.GetProperty("batch_id").GetString());
Assert.Equal("0.38.2-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
Assert.Equal("AllowUnsignedDev", latest.GetProperty("release").GetProperty("signing_mode").GetString());
Assert.Equal("excluded", latest.GetProperty("release").GetProperty("public_trusted_signing").GetString());
```

- [x] **Step 3: Update Web user-visible fixture values**

In `web/src/user-visible-fixtures.ts`, set:

```typescript
batch_id: "full-admin-host-mutation-gate-20260506-145506-0382",
duration_ms: 108824,
duration_ms: 11071,
count: 18,
status_counts: { collected: 18 },
peak_adapter_mib: 3357.76,
peak_process_mib: 16951.19,
version: "0.38.2-admin-smoke",
```

Expected: the first `duration_ms` belongs to `service-msi-hyperv-admin-smoke`; the second belongs to `os-mutation-gate`.

- [x] **Step 4: Update browser fixture verification values**

In `web/scripts/verify-browser-fixture.mjs`, set:

```javascript
batch_id: "full-admin-host-mutation-gate-20260506-145506-0382",
gpu_snapshots: { present: true, count: 18, peak_adapter_mib: 3357.76, peak_process_mib: 16951.19 },
version: "0.38.2-admin-smoke",
```

Ensure the evidence panel expectation includes:

```javascript
"full-admin-host-mutation-gate-20260506-145506-0382",
"0.38.2-admin-smoke",
"18",
```

- [x] **Step 5: Run focused API and Web checks**

Run:

```powershell
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
```

Expected: all commands exit `0`.

- [x] **Step 6: Commit Task 4**

Run:

```powershell
git add -- src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs web/src/user-visible-fixtures.ts web/scripts/verify-browser-fixture.mjs
git commit -m "test: update evidence fixtures to 0.38.2"
```

Expected: commit succeeds.

---

### Task 5: Full Non-Mutating Verification and Closure

**Files:**
- Read: all files touched in Tasks 1-4

- [x] **Step 1: Run required non-mutating verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

Expected:

```text
packaging/windows-desktop-node/tests: 142 passed
packaging/windows-desktop-node/installer/tests: 36 passed
web/tests: 26 passed
dotnet test: all test projects pass
npm test --prefix web: served app.js is current
npm run verify:parity --prefix web: browser fixture verification passed
node --check web/app.js: exit 0
git diff --check: exit 0; line-ending warnings are acceptable if no whitespace errors are reported
```

- [x] **Step 2: Confirm canonical/historical search results**

Run:

```powershell
rg -n "full-admin-host-mutation-gate-2026-05-06-0381.md|0\.38\.1-admin-smoke.*최신|0\.38\.1-admin-smoke.*Latest|0\.38\.0-admin-smoke.*최신|0\.38\.0-admin-smoke.*Latest" .
```

Expected: no output for standalone `0381.md` and no latest claims for `0.38.0` or `0.38.1`. Historical ledger mentions of `0.38.1-admin-smoke` are acceptable only when the same line says `Historical` or clearly references an earlier run.

- [x] **Step 3: Check working tree**

Run:

```powershell
git status --short
```

Expected: no unstaged implementation files from Batch 1 remain. If the only remaining changes are unrelated user changes, leave them untouched and document them in the final response.

- [x] **Step 4: Prepare Batch 2 handoff**

Record in the final response that Batch 2 should be planned from:

```text
docs/superpowers/specs/2026-05-06-purecvisor-desktop-node-next-batch-0382-canonical-evidence-and-evidence-api-hardening-design.md
```

Batch 2 implementation should start with API unit tests for degraded/malformed/partial evidence and redaction. It should not perform host mutation.

---

## Self-Review

Spec coverage:

- Batch 1 canonical `0.38.2` evidence: Tasks 1-4.
- `0.38.1` historical-only treatment: Tasks 1 and 2.
- High-level docs and fixtures: Tasks 2-4.
- Required non-mutating verification: Task 5.
- Batch 2 handoff: Task 5, Step 4.

Unresolved marker scan:

- No unresolved marker tokens are intentionally present.
- Every code or document change step includes exact values to apply.

Type and property consistency:

- API fixture property names match existing `ops.summary.batch_evidence` tests: `batch_id`, `release.version`, `release.signing_mode`, `release.public_trusted_signing`, `route_msi_hyperv`, `os_mutation`, and `host_final_state`.
- Web fixture property names match existing fixture structure: `gpu_snapshots.count`, `status_counts.collected`, `peak_adapter_mib`, and `peak_process_mib`.

## Execution Options

Plan execution should not run actual host mutation. Use one of:

1. Subagent-Driven: dispatch a fresh worker per task, review between tasks, and keep commits scoped.
2. Inline Execution: execute tasks in this session with checkpoints after each task.
