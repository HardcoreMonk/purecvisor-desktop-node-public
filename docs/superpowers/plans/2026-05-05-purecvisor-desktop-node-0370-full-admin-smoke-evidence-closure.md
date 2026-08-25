# 0.37.0 Full Admin Smoke Evidence Closure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `0.37.0-admin-smoke` batch-supervised full admin host mutation gate PASS evidence를 active docs와 GA evidence ledger에 최신 기준점으로 고정한다.

**Architecture:** 실행 artifact는 immutable evidence로 두고, git-tracked 문서는 evidence pointer, 최신 gate 해석, transient failure disposition, public boundary 분류만 갱신한다. 이번 evidence는 Batch Supervisor 아래에서 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 통과한 full admin host mutation gate이므로 `0.35.7-admin-smoke` OS gate와 `0.36.1-admin-smoke` batch-supervised Service/MSI/Hyper-V evidence를 최신 기준점에서 대체한다. Product runtime/source code와 OS state는 변경하지 않는다.

**Tech Stack:** Markdown, Pester 5 documentation guard, PowerShell 7, existing Batch Supervisor/admin smoke artifacts.

**구현 상태:** `0.37.0-admin-smoke` closure evidence와 문서 guard는 과거에 완료됐고, 최신 full admin host mutation evidence 기준점은 이후 `0.38.x` gate로 대체됐다. 2026-05-07에는 실제 host mutation 없이 문서 상태 정리로 checkbox closure만 반영했다.

---

## Evidence Facts

- Batch artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`
- Route parity artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`
- OS mutation artifact: `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`
- Version: `0.37.0-admin-smoke`
- Source/MSI provenance commit: `485b1a7338fb2b682c3964c858ccc13c322950d7`
- MSI SHA-256: `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`
- Signing mode: `AllowUnsignedDev`
- Public trusted signing: `excluded`
- External stable publication: `not-claimed`
- Batch result: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, `failed_step_id=null`, `next_resume_step_id=null`
- Step 1 result: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `duration_ms=60127`
- Step 2 result: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `duration_ms=10029`
- Service/MSI/Hyper-V final state: service `PureCVisorDesktopNode` `Running`, startup `Automatic`, boot time unchanged, `remaining_pcv_vms=[]`
- MSI lifecycle: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`
- Hyper-V route smoke: host status, network inventory, VM create/start/restart/poweroff/delete, checkpoint create/restore/delete pass
- Expected structured failure: installer ISO `vm.shutdown` returned `PCV_VM_SHUTDOWN_NOT_AVAILABLE`
- Delete guard: managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete blocked with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`
- OS gate final state: firewall final rule count `0`, Event Log source absent, internal Root and TrustedPublisher present, boot time unchanged
- LAN smoke prefix: `http://[redacted-private-endpoint]:7777/`
- Final trust-store thumbprints:
  - Root: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
  - TrustedPublisher: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- Transient disposition:
  - First batch attempt failed at MSI repair with `PCV_SMOKE_MSI_STEP_FAILED|repair exited 1603.`
  - Direct `DesktopNode.Host.exe service-action repair-installed` returned exit `0`.
  - Manual MSI repair after recovery returned exit `0`.
  - Batch Supervisor `-Resume` completed both steps with `ok=true`.
  - The failed attempt is recorded as transient/recovered evidence, not as the final gate result.

## File Structure

- Create: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md`
  - Canonical human-readable evidence record for the `0.37.0-admin-smoke` full admin host mutation gate.
- Create: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
  - Documentation guard that ensures the new evidence document and high-level docs reference `0.37.0-admin-smoke` consistently.
- Modify: `README.md`
  - Add latest full admin host mutation gate bullet after the existing `0.36.1-admin-smoke` bullet.
- Modify: `AGENTS.md`
  - Update current latest OS gate/admin smoke summary from `0.35.6`/older artifacts to `0.37.0-admin-smoke`.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Replace latest OS mutation evidence and latest batch-supervised evidence rows with the unified `0.37.0` full admin gate row.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Update latest gate wording and Batch Supervisor stale warning from `0.35.7` to `0.37.0`.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Add public-boundary classification for `0.37.0-admin-smoke`.
- Modify: `docs/ADR_INDEX.md`
  - Add the new evidence pointer under the current ADR-0004/internal-only product runtime support notes.
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
  - Append post-closure `0.37.0` full admin gate evidence.
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
  - Add ledger row or section for the latest full admin gate.
- Modify: `packaging/windows-desktop-node/README.md`
  - Add installer/product wrapper evidence bullet and final installed state.
- Modify: `packaging/windows-desktop-node/installer/README.md`
  - Add latest MSI lifecycle evidence bullet and transient repair disposition.

## Task 1: Add Failing Documentation Guard

**Files:**
- Create: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [x] **Step 1: Add the guard test**

Create `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1` with this content:

```powershell
Set-StrictMode -Version Latest

Describe 'Admin smoke evidence documentation' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path

        function Get-RepoText {
            param([Parameter(Mandatory)] [string] $RelativePath)

            Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot $RelativePath)
        }
    }

    It 'records the 0.37.0 full admin host mutation gate as canonical evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md'

        $evidencePath | Should -Exist
        $content = Get-Content -Raw -LiteralPath $evidencePath

        $content | Should -Match '0\.37\.0-admin-smoke'
        $content | Should -Match 'full-admin-host-mutation-gate-20260505-231654-0370'
        $content | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370'
        $content | Should -Match 'os-mutation-gates-batch-profile-20260505-231654-0370'
        $content | Should -Match '485b1a7338fb2b682c3964c858ccc13c322950d7'
        $content | Should -Match 'f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95'
        $content | Should -Match 'PCV_SMOKE_MSI_STEP_FAILED\|repair exited 1603'
        $content | Should -Match 'Batch Supervisor `-Resume`'
        $content | Should -Match 'public trusted signing.*excluded'
        $content | Should -Match 'external stable publication.*not-claimed'
    }

    It 'updates high-level docs to point at the 0.37.0 full admin gate' {
        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.37\.0-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260505-231654-0370|routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370|os-mutation-gates-batch-profile-20260505-231654-0370'
        }
    }

    It 'does not leave stale latest OS gate wording behind' {
        $paths = @(
            'AGENTS.md',
            'README.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Not -Match '최신 OS gate.*0\.35\.7-admin-smoke'
            $content | Should -Not -Match 'latest OS gate.*0\.35\.7-admin-smoke'
            $content | Should -Not -Match '최신 OS mutation gate.*0\.35\.7-admin-smoke'
            $content | Should -Not -Match '향후 별도 승인된 OS gate rerun이 성공하기 전까지 최신 OS gate evidence는 `0\.35\.7-admin-smoke`'
        }
    }
}
```

- [x] **Step 2: Run the focused guard and confirm it fails**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

Expected: fail because `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md` does not exist yet and high-level docs do not all reference `0.37.0-admin-smoke`.

## Task 2: Create Canonical Evidence Record

**Files:**
- Create: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md`

- [x] **Step 1: Add the evidence document**

Create `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md` with this content:

```markdown
# Full Admin Host Mutation Gate Evidence - 2026-05-05 0.37.0

evidence_id: full-admin-host-mutation-gate-2026-05-05-0370
created_at: 2026-05-05T23:26:05+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260505-231654-0370
version: 0.37.0-admin-smoke
source_commit_sha: 485b1a7338fb2b682c3964c858ccc13c322950d7
msi_sha256: f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95
signing_mode: AllowUnsignedDev
trust_model: AllowUnsignedDev plus ADR-0003 internal trust restore
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass
transient_recovery_status: pass-with-resume

## 범위

사용자 opt-in 범위에서 Batch Supervisor full admin host mutation gate를 실행했다. 이 gate는 Service/MSI/Hyper-V route parity와 firewall, LAN, Event Log, ADR-0003 internal trust-store OS mutation gate를 같은 batch manifest에서 실행했다.

이 evidence는 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal trust-store restore 범위다. Public trusted signing, public/stable signing claim, 외부 stable publication claim은 제외한다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `duration_ms=60127`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `duration_ms=10029`

## Service, MSI, Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`
- MSI: `PureCVisorDesktopNode-0.37.0-admin-smoke-windows-x64.msi`
- MSI SHA-256: `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`
- MSI signing mode: `AllowUnsignedDev`
- MSI lifecycle: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`
- Service-action smoke: pass
- Installed Hyper-V API route smoke: pass
- Host status route: pass
- Network inventory route: pass
- VM lifecycle routes: create, start, restart, poweroff, delete pass
- Checkpoint routes: create, restore, delete pass
- Expected structured failure: installer ISO `vm.shutdown` returned `PCV_VM_SHUTDOWN_NOT_AVAILABLE`
- Delete guard: managed delete `action=delete`, repeat delete `action=absent`, unmanaged delete blocked with `PCV_VM_NOT_MANAGED_BY_PURECVISOR`
- Final proof: service `Running`, startup `Automatic`, boot time unchanged, `remaining_pcv_vms=[]`

## Firewall, LAN, Event Log, Trust Store 결과

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`
- Event Log: `eventlog-register` pass 후 `eventlog-remove` pass, final source absent
- Firewall: owned rule enable pass 후 remove pass, final rule count `0`
- LAN: `http://[redacted-private-endpoint]:7777/` smoke pass
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`와 TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`
- Final service: `PureCVisorDesktopNode` `Running`, loopback `http://127.0.0.1:7777/`
- Boot time unchanged: pass

## Transient MSI Repair Disposition

첫 batch 실행은 `service-msi-hyperv-admin-smoke`의 MSI repair 단계에서 `PCV_SMOKE_MSI_STEP_FAILED|repair exited 1603.`으로 실패했다. 실패 직후 서비스는 수동 복구로 `Running` 상태와 loopback Web root `HTTP 200`을 회복했다.

동일한 `DesktopNode.Host.exe service-action repair-installed` 직접 실행은 exit `0`이었다. 같은 MSI의 manual repair도 exit `0`이었다. 이후 Batch Supervisor `-Resume`으로 동일 manifest를 재개했고 Service/MSI/Hyper-V step과 OS mutation gate step 모두 exit `0`으로 완료했다.

따라서 최종 `0.37.0-admin-smoke` gate 판정은 pass이며, 최초 repair `1603`은 recovered transient evidence로 기록한다. 이 transient는 다음 hardening batch에서 retry/backoff와 partial evidence persistence 대상으로 다룬다.

## 판정

`0.37.0-admin-smoke` full admin host mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
```

- [x] **Step 2: Check exact evidence strings**

Run:

```powershell
rg -n "0\.37\.0-admin-smoke|full-admin-host-mutation-gate-20260505-231654-0370|f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95|PCV_SMOKE_MSI_STEP_FAILED" docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md
```

Expected: output includes version, batch artifact, MSI SHA-256, and transient MSI repair failure disposition.

## Task 3: Update High-Level Evidence Pointers

**Files:**
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/ADR_INDEX.md`
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`

- [x] **Step 1: Add canonical latest evidence bullets**

Use this exact Korean evidence bullet in `README.md` and `packaging/windows-desktop-node/README.md` near the latest admin-smoke bullets:

```markdown
- `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370` evidence는 사용자 관리자 opt-in으로 `0.37.0-admin-smoke` full admin host mutation gate를 Batch Supervisor 아래에서 완료했음을 기록한다. MSI provenance commit은 `485b1a7338fb2b682c3964c858ccc13c322950d7`, MSI SHA-256은 `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`, signing mode는 `AllowUnsignedDev`다. Batch summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, timeout false였고, Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 PASS였다. MSI lifecycle install/repair/uninstall preserve/install-remove-data/uninstall-remove-data/final restore는 모두 exit `0`였고 VM create/start/restart/poweroff/delete, checkpoint create/restore/delete, unmanaged delete guard, firewall enable/remove, LAN listener IP smoke, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore가 PASS였다. 첫 batch attempt의 MSI repair `1603`은 direct `repair-installed`와 manual MSI repair exit `0`, 이후 Batch Supervisor `-Resume` PASS로 recovered transient evidence로 분류한다. Final service는 loopback-only `Running`, installed DisplayVersion은 `0.37.0`, firewall final count는 `0`, Event Log source는 absent, internal trust cert는 present, boot time unchanged, `remaining_pcv_vms=[]`다. 이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
```

Use this shorter pointer in `docs/DEVELOPER_INDEX.md`:

```markdown
| 최신 full admin host mutation gate evidence 확인 | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370/summary.json`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370/summary.json`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370/summary.json` |
```

Use this policy sentence in `docs/DEVELOPMENT_VERIFICATION_POLICY.md` and `AGENTS.md`:

```markdown
- `0.37.0-admin-smoke`는 사용자 관리자 opt-in으로 Batch Supervisor full admin host mutation gate를 통과한 최신 Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store evidence다. First-attempt MSI repair `1603`은 recovered transient로 기록하고, 최종 `-Resume` run은 `ok=true`, timeout false, boot time unchanged, final service `Running`, firewall final count `0`, Event Log source absent, internal trust cert present, `remaining_pcv_vms=[]`로 완료했다.
```

Use this public boundary sentence in `docs/PUBLIC_RELEASE_BOUNDARY.md`:

```markdown
- `0.37.0-admin-smoke` full admin host mutation gate는 내부 전용 `AllowUnsignedDev` 관리자 opt-in evidence다. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store mutation pass를 기록하지만 public trusted signing, public stable channel, external stable publication evidence는 아니다.
```

Use this ADR support note in `docs/ADR_INDEX.md`:

```markdown
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md`는 ADR-0004 내부 전용 제품 런타임 판단 이후 최신 full admin host mutation gate evidence다. 이 evidence는 `AllowUnsignedDev`와 ADR-0003 internal trust-store restore 범위이며 public trusted signing 또는 외부 stable publication을 주장하지 않는다.
```

- [x] **Step 2: Update aggregate and ledger records**

Append this section to `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`:

```markdown
## Post-closure 2026-05-05 0.37.0 Full Admin Host Mutation Gate

`0.37.0-admin-smoke` full admin host mutation gate는 `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370`에 기록됐다. MSI provenance commit은 `485b1a7338fb2b682c3964c858ccc13c322950d7`, MSI SHA-256은 `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`다.

Batch Supervisor summary는 `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, timeout false다. Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 PASS였고 final state는 installed DisplayVersion `0.37.0`, loopback-only service `Running`, firewall final count `0`, Event Log source absent, internal trust Root/TrustedPublisher present, boot time unchanged, `remaining_pcv_vms=[]`다.

첫 batch attempt의 MSI repair `1603`은 direct `repair-installed` exit `0`, manual MSI repair exit `0`, Batch Supervisor `-Resume` PASS로 recovered transient evidence로 분류한다. Closure math는 계속 closed이고 public trusted signing과 외부 stable publication은 제외 상태다.
```

Add this ledger row to `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md` in the admin-smoke evidence table or latest evidence section:

```markdown
| 2026-05-05 | `0.37.0-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370` | PASS | Batch Supervisor full gate. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass. First-attempt MSI repair `1603` recovered by direct repair/manual repair and `-Resume`. Public trusted signing/external stable publication excluded. |
```

Add this installer note to `packaging/windows-desktop-node/installer/README.md` near MSI lifecycle evidence:

```markdown
- `0.37.0-admin-smoke` MSI lifecycle evidence is `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`. The final resumed run recorded install, repair, uninstall preserve, install-remove-data, uninstall-remove-data, and final restore exit `0`; MSI SHA-256 is `f7fc56ab9ca83ba863008c864894d1ae8d14079616e8d2c0dd4a961895a43d95`. A first-attempt repair `1603` is retained as recovered transient evidence because direct `repair-installed`, manual MSI repair, and Batch Supervisor `-Resume` all completed successfully.
```

- [x] **Step 3: Remove stale latest OS gate wording**

Run:

```powershell
rg -n '최신 OS gate.*0\.35\.7-admin-smoke|latest OS gate.*0\.35\.7-admin-smoke|최신 OS mutation gate.*0\.35\.7-admin-smoke|향후 별도 승인된 OS gate rerun이 성공하기 전까지 최신 OS gate evidence는 `0\.35\.7-admin-smoke`' AGENTS.md README.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md
```

Expected: no output. Historical evidence files such as `docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0357.md` may still describe their own historical status and should not be edited just to erase history.

## Task 4: Verify, Commit, and Push

**Files:**
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
- Verify: all files changed in Tasks 1-3

- [x] **Step 1: Run focused documentation guard**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [x] **Step 2: Run affected packaging docs suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: PASS. This is non-mutating Pester coverage; it must not run Hyper-V, MSI, firewall, LAN, Event Log, trust-store, or service mutation.

- [x] **Step 3: Run markdown whitespace check**

Run:

```powershell
git diff --check
```

Expected: no output and exit `0`.

- [x] **Step 4: Review changed files**

Run:

```powershell
git status -sb
git diff --stat
git diff -- docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md
```

Expected: changed files are limited to the evidence doc, documentation guard, and high-level docs listed in this plan.

- [x] **Step 5: Commit**

Run:

```powershell
git add AGENTS.md README.md docs/ADR_INDEX.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "Document 0.37.0 full admin smoke evidence"
```

Expected: commit created.

- [x] **Step 6: Push**

Run:

```powershell
git push
```

Expected: push succeeds. If the branch is still ahead because prior commits are unpushed, this push publishes the accumulated main branch commits as requested by the user.

## Rollback

- Documentation-only rollback:

```powershell
git revert --no-edit HEAD
```

- Artifact cleanup is not part of rollback. The `artifacts/**` evidence roots are execution records and should remain available for audit unless the user explicitly requests artifact deletion.

## Completion Criteria

- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-05-0370.md` exists and records `0.37.0-admin-smoke`.
- High-level docs point at `0.37.0-admin-smoke` as latest full admin host mutation gate evidence.
- Stale latest OS gate wording for `0.35.7-admin-smoke` is removed from active high-level docs.
- Focused Pester documentation guard passes.
- `packaging/windows-desktop-node/tests` passes.
- `git diff --check` passes.
- Changes are committed and pushed.

## Self-Review

- Spec coverage: selected batch option `1` is covered by a canonical evidence doc, high-level docs, public boundary classification, transient repair disposition, verification guard, commit, and push.
- Placeholder scan: no `TBD`, `TODO`, `fill in`, or unspecified implementation steps remain.
- Type/path consistency: evidence paths, version, commit SHA, MSI SHA-256, and artifact roots match the completed admin smoke run.
