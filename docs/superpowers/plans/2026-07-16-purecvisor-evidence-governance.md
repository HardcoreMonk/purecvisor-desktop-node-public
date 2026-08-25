# PureCVisor Evidence Governance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make current operational evidence machine-owned from one JSON source and apply proportional S/M/L documentation rules.

**Architecture:** A schema-validated `current-evidence.json` owns only current pointers and facts. A PowerShell generator replaces bounded marker blocks in six documents and supports a read-only `-Check`; historical evidence remains immutable. The change-tier policy connects S/M/L to Fast/Full/Release lanes from Slice A.

**Tech Stack:** JSON Schema, PowerShell 7.6, Pester 5.7.1, Markdown.

---

Prerequisite: `2026-07-16-purecvisor-development-feedback-loop.md` Task 2.
Source design: `docs/superpowers/specs/2026-07-16-purecvisor-desktop-node-development-throughput-automation-design.md`.

## File map

- Create `docs/ga-ready/current-evidence.schema.json`: canonical current-fact schema.
- Create `docs/ga-ready/current-evidence.json`: 0.42.64 current operational values.
- Create `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1`: validate, render, update/check.
- Create `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`: schema, reference, marker and stale-output tests.
- Modify six current-facing Markdown documents to contain generated markers.
- Modify `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`: assert JSON/generator ownership instead of hand-copied values.
- Create `docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`: S/M/L policy.
- Modify verification module/tests and policy to enforce tier minimum lanes.
- Create code-level evidence for the migration.

### Task 1: Define the current evidence schema and initial record

**Files:**
- Create: `docs/ga-ready/current-evidence.schema.json`
- Create: `docs/ga-ready/current-evidence.json`
- Test: `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`

- [ ] **Step 1: Write failing canonical-record tests**

```powershell
Describe 'current evidence canonical record' {
    It 'contains the CLI Web only 0.42.64 anchor' {
        $j=Get-Content -Raw (Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.json')|ConvertFrom-Json
        $j.current.version | Should -Be '0.42.64-admin-smoke'
        $j.current.operator_surfaces | Should -Be @('web','cli')
        $j.current.tui_present | Should -BeFalse
        $j.current.provenance_commit | Should -Be 'a0491e39992093b9ad506619cfacb1675939d6a3'
    }
    It 'rejects malformed SHA and missing evidence references' {
        { Test-PcvCurrentEvidenceRecord -Record ([pscustomobject]@{schema_version=1}) -RepoRoot $script:RepoRoot } |
            Should -Throw '*PCV_CURRENT_EVIDENCE_INVALID*'
    }
}
```

- [ ] **Step 2: Run focused Pester and verify RED**

Expected: files and validation function are missing.

- [ ] **Step 3: Add the JSON schema**

The schema is Draft 2020-12, disallows additional top-level properties, and requires:

```json
{
  "schema_version": 1,
  "contract": "pcv-current-evidence-v1",
  "current": {
    "version": "0.42.64-admin-smoke",
    "operator_surfaces": ["web", "cli"],
    "tui_present": false,
    "package_evidence": "docs/ga-ready/evidence/admin-smoke-package-2026-07-15-04264.md",
    "fullgate_batch": "full-admin-host-mutation-gate-20260715-04264",
    "fullgate_evidence": "docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-15-04264-hostmutation.md",
    "functional_evidence": "docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-15-04264.md",
    "installed_evidence": "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-15-04264.md",
    "clean_msi_sha256": "8ba9714995d153e97a84c90afcf01b3ab1a612a166089e764b7046aae46c1cb7",
    "operational_msi_sha256": "540f5c5fc8bc78a7c07f950cf9c39002491e69308dc264112a42ad0b510f50bf",
    "payload_sha256": "d02aec33be7d8f12348e242336604bb453b63b5b0d2cc139f6ced1ef15287cc0",
    "provenance_commit": "a0491e39992093b9ad506619cfacb1675939d6a3"
  },
  "manual_admin": {
    "latest_closed_baseline": "0.42.58-admin-smoke",
    "latest_closed_target": "0.42.59-admin-smoke",
    "latest_closed_descriptor": "manual-admin-campaign-descriptor-20260529-04258-04259-closed",
    "blocked_baseline": "0.42.62-admin-smoke",
    "blocked_target": "0.42.63-admin-smoke",
    "blocked_reason": "blocked-by-installed-baseline-version-mismatch"
  },
  "claims": {"public_trusted_signing": false, "external_stable_publication": false}
}
```

SHA properties use `^[0-9a-f]{64}$`; commit uses `^[0-9a-f]{40}$`; surfaces have unique enum values `web|cli`; `tui_present` is constrained to false.

- [ ] **Step 4: Implement record validation inside the generator script**

`Test-PcvCurrentEvidenceRecord` checks required properties, exact hashes, CLI/Web-only surfaces, referenced Markdown existence, and version agreement across referenced evidence headers. It returns the record or throws `PCV_CURRENT_EVIDENCE_INVALID|<field>|<detail>`.

- [ ] **Step 5: Verify GREEN and commit**

```powershell
git add docs/ga-ready/current-evidence.schema.json docs/ga-ready/current-evidence.json packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1
git commit -m "feat: define canonical current evidence record"
```

### Task 2: Generate and check bounded Markdown blocks

**Files:**
- Modify: `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1`

- [ ] **Step 1: Add failing render/check tests**

```powershell
It 'renders one bounded CLI Web current block' {
    $block=ConvertTo-PcvCurrentEvidenceMarkdown -Record $script:Record
    $block | Should -Match '<!-- BEGIN GENERATED CURRENT EVIDENCE -->'
    $block | Should -Match '0\.42\.64-admin-smoke'
    $block | Should -Match 'Web Console.*PCVCLI'
    $block | Should -Match 'tui_present.*false'
    $block | Should -Not -Match 'Web/TUI/CLI current-card'
}
It 'fails Check when a target block is stale without writing' {
    $before=Get-Content -Raw $target
    { Update-PcvCurrentEvidenceDocument -Path $target -Block $script:Block -Check } |
        Should -Throw '*PCV_CURRENT_EVIDENCE_STALE*'
    (Get-Content -Raw $target) | Should -BeExactly $before
}
```

- [ ] **Step 2: Verify RED**

Expected: render/update functions missing.

- [ ] **Step 3: Implement deterministic rendering**

Use constants:

```powershell
$begin='<!-- BEGIN GENERATED CURRENT EVIDENCE -->'
$end='<!-- END GENERATED CURRENT EVIDENCE -->'
```

The block contains version, four evidence links, hashes, provenance, CLI/Web-only installed result, actual-VM QoS/disk result, manual-admin closed pair, blocked pair, and non-public claims. Normalize to LF while rendering and preserve the target file's original newline style when writing.

- [ ] **Step 4: Implement marker replacement and atomic write**

Require exactly one begin/end pair. Build complete output in memory, validate markers again, write `<path>.tmp`, then `Move-Item -Force` to the target. Under `-Check`, compare normalized expected/actual and throw without creating a temp file.

- [ ] **Step 5: Add the CLI contract**

Parameters: `EvidencePath` defaulting to the canonical JSON, `RepoRoot`, and `Check`. Targets are the six approved documents only. Output JSON contains schema version, `ok`, `check`, source path, and per-target `current|updated|stale` status. Exit 1 on validation/stale failure.

- [ ] **Step 6: Add the evidence suite to the verification selector**

Add `current-evidence-check` to Full/Release with command `pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check`. Map the canonical JSON, generator/tests, `AGENTS.md`, and all six marker targets to that suite in Fast lane. Add failing selector/catalog tests before editing the module.


- [ ] **Step 7: Verify GREEN and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1 -Output Detailed
git add packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1 packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1 packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1
git commit -m "feat: generate current evidence document blocks"
```

### Task 3: Migrate six current-facing documents

**Files:**
- Modify: `AGENTS.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Modify: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- Modify: `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: Add a failing ownership test**

For every target assert exactly one marker pair, generator `-Check` success, 0.42.64 values coming from JSON, and unchanged historical anchor text after the end marker. Confirm RED before migration.

- [ ] **Step 2: Replace only active-current sections with markers**

In `AGENTS.md`, replace the 2026-07-15 current section up to the 2026-07-13 historical heading. In each index/ledger/policy/README replace only its top current-summary area; keep all historical sections and ownership tables outside markers.

- [ ] **Step 3: Run generator update then Check twice**

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check
```

Expected: first update succeeds; both checks are idempotent and write nothing.

- [ ] **Step 4: Verify historical immutability**

Use `git diff --word-diff` to confirm changes outside active-current sections are absent. Run focused evidence Pester and TUI-removal boundary tests.

- [ ] **Step 5: Commit migration**

```powershell
git add AGENTS.md docs/ga-ready docs/DEVELOPMENT_VERIFICATION_POLICY.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "docs: generate current operational evidence blocks"
```

### Task 4: Apply S/M/L change classification

**Files:**
- Create: `docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`
- Modify: `packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [ ] **Step 1: Add failing tier compatibility tests**

Assert S permits Fast only when no public/installer/host boundary path exists, M promotes Fast to Full, L promotes any lower lane to Release, and host mutation/security/current-anchor paths force L regardless of requested tier.

- [ ] **Step 2: Implement path risk classification**

Add `Resolve-PcvDevelopmentChangeTier` returning `requested_tier`, `effective_tier`, and reasons. Force L for installer lifecycle, host mutation runner, security ADR/policy, current evidence anchor, public boundary, signing and publication paths. Force at least M for API/CLI/Web contract and general packaging changes. Unknown paths force Full verification but not an unsupported L claim.

- [ ] **Step 3: Write the policy**

Define S as one module/no public contract/no host mutation; M as cross-module or non-mutating public/package contract; L as security/installer lifecycle/current anchor/host mutation/public release. Specify required docs and lanes exactly as the design.

- [ ] **Step 4: Verify and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1 -Output Detailed
git add docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md docs/DEVELOPMENT_VERIFICATION_POLICY.md packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1 packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1
git commit -m "docs: classify development changes by risk"
```

### Task 5: Close Slice B with generation evidence

**Files:**
- Create: `docs/ga-ready/evidence/current-evidence-generation-code-level-2026-07-16.md`
- Modify: `.github/workflows/development-gates.yml`

- [ ] **Step 1: Add generator Check to non-mutating CI**

Run it after checkout in the packaging owner job. It must never invoke package build or host mutation.

- [ ] **Step 2: Execute focused and full checks**

Run generation tests, evidence docs tests, the Full lane, and `git diff --check`. Re-run generator Check after all documentation changes.

- [ ] **Step 3: Record evidence**

Record canonical record ID, six targets, idempotence result, historical immutability, current 0.42.64 unchanged, TUI absent, `host_mutation_performed=false`, and public claims false.

- [ ] **Step 4: Commit Slice B closure**

```powershell
git add .github/workflows/development-gates.yml docs/ga-ready/evidence/current-evidence-generation-code-level-2026-07-16.md
git commit -m "docs: record current evidence generation verification"
```
