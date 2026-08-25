# Packaging Release Control Plane Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Recenter packaging, release, and evidence docs around internal distribution control plane entrypoints without reopening public distribution scope.

**Architecture:** Keep existing packaging scripts and evidence files. Add an evidence index and shorten README/AGENTS references so they point to canonical control plane docs instead of duplicating every evidence record.

**Tech Stack:** Markdown, PowerShell Pester, existing `packaging/windows-desktop-node/**`, ADR docs, `docs/ga-ready/**`.

---

## File Structure

- Create: `docs/ga-ready/EVIDENCE_INDEX.md`
- Create: `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

## Task 1: Create Evidence Index

**Files:**
- Create: `docs/ga-ready/EVIDENCE_INDEX.md`

- [ ] **Step 1: Write evidence index**

Create `docs/ga-ready/EVIDENCE_INDEX.md`:

```markdown
# Desktop Node Evidence Index

이 문서는 Desktop Node evidence entrypoint다. 개별 evidence 파일은
`docs/ga-ready/evidence/`에 보존하고, README/AGENTS는 이 파일을 통해 최신
분류만 안내한다.

## Current

- Full admin host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md`
- Manual admin hardening: `docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md`
- Lifecycle packaging rebaseline: `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`
- Internal private network boundary: `docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md`

## Supporting

- Web/API port split: `docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md`, `docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md`
- Account/RBAC/JWT/console: `docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md`
- Installed account login/noVNC: `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`, `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`

## Historical

- Earlier admin-smoke and route parity evidence remains in `docs/ga-ready/evidence/`.
- Historical records are not deleted during Stabilize Then Split.

## Closed Not Adopted

- Public distribution candidate: `docs/adr/0005-public-distribution-operations-expansion-candidate.md`
- Public distribution gate matrix: `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`

## Rule

Public trusted signing, winget public submission, external stable publication, and
public clean-host signed smoke remain out of scope unless a new ADR changes the
distribution boundary.
```

- [ ] **Step 2: Run markdown hygiene**

```powershell
git diff --check -- docs/ga-ready/EVIDENCE_INDEX.md
```

Expected: no output.

## Task 2: Create Control Plane Index

**Files:**
- Create: `docs/ga-ready/CONTROL_PLANE_INDEX.md`

- [ ] **Step 1: Write control plane index**

Create `docs/ga-ready/CONTROL_PLANE_INDEX.md`:

````markdown
# Desktop Node Control Plane Index

## Current Decisions

- ADR index: `docs/ADR_INDEX.md`
- Internal private network distribution: `docs/adr/0006-internal-private-network-distribution.md`
- Release/version policy: `docs/adr/0002-release-version-policy.md`
- Internal signing policy: `docs/adr/0003-internal-trusted-signing-policy.md`

## Matrices

- Route promotion: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- Internal distribution: `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- Public closed-not-adopted history: `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md`
- Repo migration: `docs/ga-ready/REPO_MIGRATION_MAP.md`
- Verification ownership: `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
- Evidence index: `docs/ga-ready/EVIDENCE_INDEX.md`

## Packaging Entrypoints

- Product wrapper: `packaging/windows-desktop-node/README.md`
- Installer: `packaging/windows-desktop-node/installer/README.md`
- Product module: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Installer build: `packaging/windows-desktop-node/installer/build.ps1`

## Verification

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
git diff --check
```
````

- [ ] **Step 2: Verify code fence rendering**

Run:

```powershell
rg -n "```" docs/ga-ready/CONTROL_PLANE_INDEX.md
```

Expected: exactly two lines containing triple backticks.

## Task 3: Shorten README/AGENTS Evidence Entry

**Files:**
- Modify: `README.md`
- Modify: `AGENTS.md`

- [ ] **Step 1: Add control plane references near document entrypoints**

Add this short block near the existing document entrypoint sections:

```markdown
- GA-ready control plane index: `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- Evidence index: `docs/ga-ready/EVIDENCE_INDEX.md`
```

- [ ] **Step 2: Preserve detailed historical lists**

Do not delete historical evidence paragraphs in this first pass. Only add the entrypoints and mark future shortening as a separate implementation task.

- [ ] **Step 3: Run documentation guard**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/CONTROL_PLANE_INDEX.md README.md AGENTS.md
git commit -m "docs: add packaging release control plane indexes"
```

## Task 4: Full Packaging Verification

**Files:**
- No source edits

- [ ] **Step 1: Run packaging suites**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
git diff --check
```

Expected: PASS and no whitespace errors.
