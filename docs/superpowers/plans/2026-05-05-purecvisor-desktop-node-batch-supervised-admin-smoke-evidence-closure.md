# Batch-Supervised Admin Smoke Evidence Closure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity evidence를 active docs와 GA evidence ledger에 분리 기록한다.

**Architecture:** 실행 artifact는 immutable evidence로 두고, git-tracked 문서는 evidence pointer, 분류, 최신 gate 해석만 갱신한다. `0.36.1-admin-smoke`는 Batch Supervisor가 감싼 Service/MSI/Hyper-V route parity evidence이며, firewall/LAN/Event Log/trust-store OS gate가 아니므로 최신 OS gate는 계속 `0.35.7-admin-smoke`로 보존한다. Product runtime/source code와 OS state는 변경하지 않는다.

**Tech Stack:** Markdown docs, Pester 5 documentation guard, PowerShell 7, ripgrep, git diff hygiene.

**구현 상태:** `0.36.1-admin-smoke` batch-supervised evidence는 Service/MSI/Hyper-V 범위 evidence로 과거에 기록됐고, 최신 admin smoke 기준점은 이후 full gate로 대체됐다. 2026-05-07에는 실제 host mutation 없이 문서 상태 정리로 checkbox closure만 반영했다.

---

## State

- 작성 기준: 2026-05-05
- 구현 상태: 승인 대기
- 실행 방식: 승인 후 documentation-only batch로 진행
- Host mutation: 이번 계획에서는 실행하지 않는다.
- Public trusted signing: excluded
- External stable publication: not-claimed

## Evidence Facts

- Batch Supervisor artifact: `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`
- Route parity artifact: `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`
- Version: `0.36.1-admin-smoke`
- Batch Supervisor summary: `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`
- Step summary: `ok=true`, `timed_out=false`, `exit_code=0`, `duration_ms=115258`, heartbeat lines `25`
- MSI provenance commit: `2a080d80a3394218aee6e1f68fc64cf9f347bf86`
- MSI SHA-256: `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`
- Signing mode: `AllowUnsignedDev`
- MSI payload file count: `7`
- Installed DisplayVersion after smoke: `0.36.1`
- Final service: `PureCVisorDesktopNode` `Running`, startup `Automatic`
- Boot time: unchanged
- Remaining PureCVisor smoke VMs: `[]`
- Hyper-V route results: `host.status` pass, `network.inventory` pass, VM create/start/restart/poweroff/delete pass, checkpoint create/restore/delete pass
- Expected structured failure: installer ISO `vm.shutdown` returned `PCV_VM_SHUTDOWN_NOT_AVAILABLE`
- Delete guard: managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete blocked with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`
- Scope exclusion: firewall/LAN/Event Log/trust-store OS gate was not rerun; latest OS gate remains `0.35.7-admin-smoke` at `artifacts/os-mutation-gates-20260505-180434-0357-rerun`

## File Structure

- Create: `docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md`
  - Dedicated evidence record for the batch-supervised `0.36.1-admin-smoke`.
- Modify: `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
  - Add a guard that high-level docs mention `0.36.1-admin-smoke` while preserving `0.35.7-admin-smoke` as latest OS gate.
- Modify: `AGENTS.md`
  - Add latest batch-supervised Service/MSI/Hyper-V evidence bullet after the existing `0.36.0-admin-smoke` bullet.
- Modify: `README.md`
  - Add product status evidence bullet and GA evidence pointer.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Add discovery link for the new evidence record and preserve latest OS gate link.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Add evidence bullet under admin opt-in evidence history.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Add public-boundary classification for `0.36.1-admin-smoke`.
- Modify: `packaging/windows-desktop-node/README.md`
  - Add product wrapper evidence bullet and Batch Supervisor artifact pointer.
- Modify: `packaging/windows-desktop-node/installer/README.md`
  - Add installer evidence bullet and final installed DisplayVersion `0.36.1` note.
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
  - Add post-closure note. Closure math remains closed.
- Modify: `docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md`
  - Add follow-up note under later admin opt-in executions.
- Read-only check: `docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0357.md`
  - Do not convert this file to `0.36.1`; it remains the latest OS mutation gate evidence.
- Read-only check: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
  - Do not change route semantics in this batch. If the worktree already contains matrix edits before execution, leave them untouched and report them as pre-existing.

## Task 1: RED - Documentation Guard

**Files:**

- Modify: `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`

- [x] **Step 1: Add failing guard for `0.36.1` classification**

Append this `It` block inside `Describe 'Desktop Node documentation synchronization guard'` after the existing `keeps installed product usage and latest OS mutation evidence discoverable` test:

```powershell
    It 'keeps batch-supervised admin smoke evidence separate from the latest OS mutation gate' {
        $docsRequiringBatchEvidence = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md',
            'docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md'
        )

        foreach ($relativePath in $docsRequiringBatchEvidence) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.36\.1-admin-smoke'
            $content | Should -Match 'batch-supervisor-host-mutating-admin-smoke-20260505-201026'
            $content | Should -Match 'routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361'
        }

        $docsPreservingLatestOsGate = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md'
        )

        foreach ($relativePath in $docsPreservingLatestOsGate) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.35\.7-admin-smoke'
            $content | Should -Match 'os-mutation-gates-20260505-180434-0357-rerun'
            $content | Should -Not -Match '0\.36\.1-admin-smoke` 현재 HEAD OS gate'
            $content | Should -Not -Match '최신 OS gate는 `0\.36\.1-admin-smoke`'
            $content | Should -Not -Match '0\.36\.1-admin-smoke`.*Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate'
        }
    }
```

- [x] **Step 2: Run the guard and confirm it fails before docs are updated**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

Expected: fail because `docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md` does not exist yet and active docs do not all reference `0.36.1-admin-smoke`.

## Task 2: Create Dedicated Evidence Record

**Files:**

- Create: `docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md`

- [x] **Step 1: Create the evidence record**

Create `docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md` with this content:

```markdown
# Batch-Supervised Admin Smoke Evidence - 2026-05-05 0.36.1

evidence_id: batch-supervised-admin-smoke-2026-05-05-0361
created_at: 2026-05-05T20:12:18+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361
version: 0.36.1-admin-smoke
msi_provenance_commit_sha: 2a080d80a3394218aee6e1f68fc64cf9f347bf86
msi_sha256: 6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307
signing_mode: AllowUnsignedDev
public_trusted_signing: excluded
external_stable_publication: not-claimed
latest_os_mutation_gate: 0.35.7-admin-smoke
latest_os_mutation_gate_artifact_root: artifacts/os-mutation-gates-20260505-180434-0357-rerun
machine_readable_json_created: no

## 범위

이 evidence는 Batch Supervisor가 감싼 Service/MSI/Hyper-V route parity admin smoke다. Firewall, LAN bearer exposure, Event Log source register/remove, ADR-0003 internal trust-store install/remove/restore OS gate는 이 실행에서 rerun하지 않았다. 최신 OS mutation gate는 계속 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다.

MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`이다. Batch Supervisor tooling과 docs 변경은 repo-local development runner/evidence closure 변경이며 MSI payload provenance로 해석하지 않는다.

## Batch Supervisor 결과

- Summary: `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`
- Step: `ok=true`, `timed_out=false`, `exit_code=0`, `duration_ms=115258`
- Heartbeat lines: `25`
- Artifact: `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`

## Service/MSI/Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`
- Installed DisplayVersion: `0.36.1`
- Final service: `PureCVisorDesktopNode` `Running`, startup `Automatic`
- Boot time: unchanged
- Remaining PureCVisor smoke VMs: `[]`
- MSI payload file count: `7`
- Host status route: pass
- Network inventory route: pass
- VM lifecycle routes: create/start/restart/poweroff/delete pass
- Checkpoint routes: create/restore/delete pass
- Expected structured failure: installer ISO `vm.shutdown` returned `PCV_VM_SHUTDOWN_NOT_AVAILABLE`
- Delete guard: managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete blocked with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`

## 판정

`0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity evidence는 pass다. 이 pass는 `AllowUnsignedDev` 내부 관리자 opt-in smoke evidence이며 public trusted signing, external stable publication, firewall/LAN/Event Log/trust-store OS mutation gate evidence가 아니다.
```

- [x] **Step 2: Confirm the evidence record contains the required anchors**

Run:

```powershell
rg -n "0\.36\.1-admin-smoke|batch-supervisor-host-mutating-admin-smoke-20260505-201026|routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361|os-mutation-gates-20260505-180434-0357-rerun" docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md
```

Expected: all four anchors are printed.

## Task 3: Update Active Documentation Pointers

**Files:**

- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`

- [x] **Step 1: Add the canonical `0.36.1` evidence bullet to high-level docs**

Use this canonical Korean paragraph in the evidence/status sections of `AGENTS.md`, `README.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `packaging/windows-desktop-node/README.md`, and `packaging/windows-desktop-node/installer/README.md`:

```markdown
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity rerun PASS. Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`, step `timed_out=false`, `exit_code=0`, heartbeat lines `25`다. MSI provenance commit은 `2a080d80a3394218aee6e1f68fc64cf9f347bf86`, MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`, signing mode는 `AllowUnsignedDev`다. Service-action, MSI lifecycle, installed Hyper-V API route smoke가 PASS였고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이번 rerun 범위가 아니며 최신 OS gate는 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
```

- [x] **Step 2: Add a Developer Index discovery row**

In `docs/DEVELOPER_INDEX.md`, add this row near the existing 최신 OS mutation evidence row:

```markdown
| 최신 batch-supervised Service/MSI/Hyper-V evidence 확인 | `docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md`, `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026/summary.json`, `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361/summary.json` |
```

Also add this bullet near the existing `0.36.0-admin-smoke` note:

```markdown
- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`는 `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V route parity evidence다. Batch Supervisor는 timeout 없이 완료됐고 final service는 loopback-only `Running`, installed DisplayVersion은 `0.36.1`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/trust-store/LAN/Event Log OS gate는 이번 rerun 범위가 아니며 최신 OS gate는 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다.
```

- [x] **Step 3: Keep OS gate wording pinned to `0.35.7`**

Run:

```powershell
rg -n '최신 OS gate는 `0\.36\.1-admin-smoke`|0\.36\.1-admin-smoke` 현재 HEAD OS gate|0\.36\.1-admin-smoke`.*Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate' AGENTS.md README.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/ga-ready/evidence packaging/windows-desktop-node -g "*.md"
```

Expected: no output.

## Task 4: Update GA Evidence Follow-Up Notes

**Files:**

- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
- Modify: `docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md`

- [x] **Step 1: Add aggregate closure post-closure note**

Append this bullet after the existing `0.35.7-admin-smoke` post-closure bullet in `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`:

```markdown
- Post-closure 2026-05-05 `0.36.1-admin-smoke` batch-supervised rerun `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`와 `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`은 MSI provenance commit `2a080d80a3394218aee6e1f68fc64cf9f347bf86` 기준으로 Service/MSI/Hyper-V route parity를 Batch Supervisor 아래에서 다시 확인했다. Supervisor는 `ok=true`, `status=completed`, timeout false였고 final state는 installed DisplayVersion `0.36.1`, loopback `Running`, boot time unchanged, `remaining_pcv_vms=[]`다. Firewall/LAN/Event Log/trust-store OS mutation gate는 이번 실행 범위가 아니며 최신 OS gate는 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다. Closure math는 계속 closed이고 public trusted signing은 제외 상태다.
```

- [x] **Step 2: Add release/LAN/OS preapproval follow-up note**

Append this section before `## 판정` in `docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md`:

```markdown
## 2026-05-05 0.36.1 Batch-Supervised Service/MSI/Hyper-V 재실행

사용자 승인 후 `0.36.1-admin-smoke` Service/MSI/Hyper-V route parity를 Batch Supervisor로 감싸 실행했다. 이 실행은 firewall/LAN/Event Log/trust-store OS gate rerun이 아니며 public trusted signing 또는 외부 stable publication 승인이 아니다.

- `artifacts/batch-runs/batch-supervisor-host-mutating-admin-smoke-20260505-201026`: Batch Supervisor summary `ok=true`, `status=completed`, `timed_out=false`, heartbeat lines `25`.
- `artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361`: Service/MSI/Hyper-V route parity `0.36.1-admin-smoke`.
- MSI SHA-256은 `6518ae19a36f00f3dde33db81b49f7cd7fd6f7d0936dc3c9e82a6413497ab307`이고 signing mode는 `AllowUnsignedDev`다.
- Final state는 installed DisplayVersion `0.36.1`, loopback-only service `Running`, boot time unchanged, `remaining_pcv_vms=[]`다.
- 최신 OS mutation gate는 계속 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다.
```

- [x] **Step 3: Leave the OS mutation evidence file unchanged**

Run:

```powershell
git diff -- docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0357.md
```

Expected: no diff for the OS mutation evidence file. If `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` has pre-existing diff, do not edit or revert it in this batch.

## Task 5: Verification

**Files:**

- Test: `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`
- Test: `packaging/windows-desktop-node/tests`

- [x] **Step 1: Run documentation sync guard**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

Expected: pass.

- [x] **Step 2: Run packaging documentation/tool guard**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: pass.

- [x] **Step 3: Verify discoverability anchors**

Run:

```powershell
rg -n "0\.36\.1-admin-smoke|batch-supervisor-host-mutating-admin-smoke-20260505-201026|routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361" AGENTS.md README.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md docs/ga-ready/evidence
```

Expected: output includes every file listed in Task 3 plus `docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md`.

- [x] **Step 4: Verify OS gate separation**

Run:

```powershell
rg -n '최신 OS gate는 `0\.36\.1-admin-smoke`|0\.36\.1-admin-smoke` 현재 HEAD OS gate|0\.36\.1-admin-smoke`.*Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate' AGENTS.md README.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/ga-ready/evidence packaging/windows-desktop-node -g "*.md"
```

Expected: no output.

- [x] **Step 5: Run diff hygiene**

Run:

```powershell
git diff --check
```

Expected: exit `0`. Existing line-ending warnings may print, but no whitespace error should be reported.

## Task 6: Final Review

**Files:**

- Read: `git diff --stat`
- Read: `git diff -- AGENTS.md README.md docs packaging/windows-desktop-node archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`

- [x] **Step 1: Confirm the diff is documentation-only plus docs guard**

Run:

```powershell
git diff --stat
```

Expected: changed files are Markdown docs and `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`.

- [x] **Step 2: Confirm no product runtime/source files changed**

Run:

```powershell
git diff --name-only | rg -n "^(src|web|packaging/windows-desktop-node/tools)/"
```

Expected: no output.

- [x] **Step 3: Report execution summary without committing**

Report:

```text
0.36.1 batch-supervised evidence closure complete.
Docs updated: <list changed markdown files>
Tests: DocumentationSync PASS, packaging Pester PASS, git diff --check PASS
No host mutation rerun in this batch.
Latest OS gate remains 0.35.7-admin-smoke.
Commit: not created in this batch.
```
