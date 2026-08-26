# Sanitized Public Authority Bootstrap and Protection Implementation Plan

> **Completion status (2026-08-25): completed.** Parentless public authority가 생성되어 PUBLIC로
> 전환됐고 main protection이 설치됐다. PR #1 cutover와 PR #2 documentation closure가 일반
> merge로 완료됐으며 현재 main은 `6e2bdb93ce308b632c929e2c17f5550ac3845401`이다. 아래
> bootstrap/current-four 문구는 seed 당시 pre-cutover snapshot이다.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development,
> superpowers:executing-plans, superpowers:verification-before-completion, and
> superpowers:requesting-code-review at the applicable checkpoints. Steps use checkbox (`- [ ]`) syntax.

**Goal:** 기존 비공개 archive와 사용자 소유 dirty main을 변경하지 않고, committed source를
public-safe tree로 정제해 별도 parentless Git root를 만든다. 이 root만 새 authoritative repository
`HardcoreMonk/purecvisor-desktop-node-public`에 private seed하고, local/provider P0/P1 `0`과 seed CI를
확인한 뒤 별도 one-way 승인으로 public 전환 및 current-four main protection을 설치한다.

**Architecture:** 기존 object database를 push하거나 rewrite하지 않는다. Clean private source
worktree에서 committed main과 Wave B/design을 병합하고 정제한 뒤 `git archive` tree를 별도 디렉터리에
extract한다. 그 디렉터리에서 새 Git repository를 초기화하여 parent 없는 root commit 하나만 만든다.
Repository-owned Node safety verifier와 official pinned Gitleaks scan이 export 전후 동일한 policy를
검증한다. Empty-repository seed가 유일한 direct-main 예외이며, protection 뒤 모든 변경은 PR로 간다.

**Tech Stack:** Git, GitHub CLI/API, Node.js 24 built-in test runner, .NET 10, Visual Studio 2026 또는
`dotnet` CLI, official Gitleaks v8.30.1. 새 구현·Required CI에 PowerShell 의존성을 추가하지 않는다.
Legacy Pester는 Wave E shadow까지 reference oracle로만 보존한다.

---

## Preconditions and immutable boundaries

- Parent design:
  `docs/superpowers/specs/2026-08-25-purecvisor-desktop-node-pester-free-required-ci-cutover-design.md`.
- Approved delta:
  `docs/superpowers/specs/2026-08-25-purecvisor-desktop-node-public-authority-snapshot-delta-design.md`.
- Original private archive remote, refs, provider metadata, visibility, description, package, issue, release,
  and Actions state are read-only throughout this plan.
- Original local main worktree is never switched, staged, stashed, cleaned, reset, pulled, merged, or pushed.
- The recorded eight dirty-main file digests are an external immutability oracle. Verify before source work,
  after export, after provider bootstrap, and at completion; mismatch is a hard stop.
- Existing remote audit findings are evidence that exact-repository publication is prohibited. They are not
  an allowlist for the new root.
- No force-push, `--mirror`, `--all`, wildcard refspec, tag push, history rewrite, destructive clean, host
  mutation, installer execution, package build, version bump, trusted-signing claim, or stable publication.
- New repository creation occurs only after the local parentless root passes every safety and test gate.
- Visibility remains `PRIVATE` until a fresh, immediate, exact-target one-way approval is received. Earlier
  general execution approval does not satisfy that final visibility gate.
- Operator commands may run in Visual Studio 2026 Terminal, cmd, or another terminal. No task requires a
  PowerShell-language script; repository automation added here is Node/C#.

## Fixed identities and paths

| Item | Exact value |
| --- | --- |
| Private source branch | `codex/public-authority-seed-source-20260825` |
| Private source worktree | `.worktrees/public-authority-seed-source-20260825` |
| New local repository | `D:/data/projects/codex-zone/purecvisor-desktop-node-public` |
| New GitHub repository | `HardcoreMonk/purecvisor-desktop-node-public` |
| New default branch | `main` |
| Root author name | `HardcoreMonk` |
| Root author email | `254846378+HardcoreMonk@users.noreply.github.com` |
| Cutover branch | `codex/pester-free-verification-cutover` |
| Current protected checks | `dotnet-tests`, `web-tests`, `packaging-pester`, `installer-web-pester` |

The new local repository path must be absent before export. If it already exists, stop and report its exact
state; do not delete or reuse it automatically.

## Public-source safety contract

`web/scripts/verify-public-source-safety.mjs` owns deterministic repository-tree policy:

1. enumerate only tracked regular files and reject symlink/submodule escape;
2. reject absolute personal profile paths and real operator/host identifiers;
3. reject observed RFC1918/private host endpoints in docs, evidence, artifacts, or configuration;
4. permit an RFC1918 literal only in source/schema/test fixtures with a nearby explicit
   `public-safety: synthetic-rfc1918` marker and no operational identity;
5. reject credential-bearing URLs, private keys, certificate secrets, live-token shapes, non-no-reply
   personal email, and the private archive provider identifier;
6. reject nested `.git`, alternates, unexpected binary archives, or provider-export data;
7. require rights-reserved `LICENSE`, `SECURITY.md`, public authority boundary, and explicit non-claims;
8. emit only relative paths, rule IDs, counts, and a canonical report digest—never raw candidate values.

`web/node-tests/public-source-safety.test.mjs` must prove accepted placeholders and synthetic fixtures plus
rejection of every category above. Each implementation change follows RED → observed expected failure →
minimal GREEN → full regression. Gitleaks is an independent second oracle; its result cannot be waived by
the Node verifier.

## Task 1: Freeze the private archive and source inputs

**Files:**
- Create on the clean source branch:
  `docs/ga-ready/evidence/public-authority-bootstrap-2026-08-25.md`

- [ ] **Step 1: Capture read-only private archive/provider inventory**

Record private visibility, default branch, remote branch/tag/release/issue/Actions/package counts, local main
committed SHA, Wave B/design SHA, and the official Gitleaks tool/archive hashes. Raw provider output remains
under a validated OS-temp audit root and is not exported.

- [ ] **Step 2: Verify the dirty-main digest oracle**

Recompute all eight previously recorded SHA-256 values without staging or editing those paths. Require exact
match. Store only the canonical aggregate digest in public evidence; keep the path/value table in the
private temp audit report.

- [ ] **Step 3: Create the isolated source worktree**

Create `codex/public-authority-seed-source-20260825` from the committed Wave B/design checkpoint. Add a clean
worktree under the fixed private-source path. Verify branch, clean status, common directory, and resolved
path containment before any merge.

- [ ] **Step 4: Merge only committed local main**

Merge the local `main` ref in the isolated source branch with a normal merge commit. Resolve conflicts only
inside that worktree. Do not copy from the dirty main filesystem. Run `git diff --check`, targeted docs/Web
tests, and fixed-diff review before committing any resolution.

## Task 2: Add the repository-owned public-safety verifier with TDD

**Files:**
- Create: `web/scripts/verify-public-source-safety.mjs`
- Create: `web/node-tests/public-source-safety.test.mjs`
- Modify: `web/package.json`
- Modify as needed: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [ ] **Step 1: Add one RED test per policy category**

Use contained temporary repositories and literal fixtures. Watch each test fail for the missing rule—not a
test setup error. Include forward/backslash profile paths, case variants, docs/evidence private endpoints,
marked and unmarked synthetic RFC1918, nested Git metadata, credential URL, key block, email, symlink escape,
and missing boundary documents.

- [ ] **Step 2: Implement the minimum tracked-tree scanner**

Use Node standard libraries and argument arrays only. Do not spawn a shell or PowerShell. Normalize paths,
apply file-class-aware rules, redact candidate values in output, sort findings, and return stable nonzero
exit on any finding.

- [ ] **Step 3: Add package commands and negative-fixture coverage**

Add `test:public-source-safety` and `verify:public-source-safety`. Require malformed marker, out-of-scope
marker, binary, symlink, nested `.git`, and report-redaction negative tests.

- [ ] **Step 4: Run targeted and Web regressions**

Run the Node test file, then the full Web test/parity suite. Record RED and GREEN command/result summaries in
the bootstrap evidence; do not record sensitive literal fixture values.

## Task 3: Sanitize the committed source tree

**Files:** all tracked text files identified by the two independent scanners; no user dirty-main file

- [ ] **Step 1: Build a private replacement inventory**

Generate a read-only candidate list in the validated OS-temp audit root. Classify every match as personal or
host identity, observed endpoint, legitimate synthetic network fixture, secret-shaped test fixture, provider
metadata, or false positive. Unclassified matches block editing.

- [ ] **Step 2: Apply deterministic personal/host redaction**

Replace real identifiers with `Operator`, absolute profiles with `C:/Users/Operator` or environment-neutral
paths, and private host/share names with explicit redaction terms. A bulk mechanical rewrite is permitted
only through a temporary Node script created with `apply_patch` under the audit root, reviewed before use,
scoped to the clean source worktree, and never committed.

- [ ] **Step 3: Sanitize operational network evidence**

Replace observed private endpoints with `[redacted-private-endpoint]` or documentation-safe TEST-NET values.
Preserve the fact that redaction occurred; never fabricate a measured PASS. Mark only genuinely synthetic
source/test values with the exact public-safety marker and add negative verifier coverage.

- [ ] **Step 4: Remove static Gitleaks-shaped fixtures**

Construct high-entropy signing, thumbprint, invalid PEM, and synthetic JWT inputs at test runtime from
reviewed fragments. Keep the same test behavior. Do not introduce `.gitleaksignore`, fingerprint allowlists,
exit suppression, or weaker assertions.

- [ ] **Step 5: Review and commit bounded sanitization batches**

For each category, run the targeted tests first, then Node safety and Gitleaks current-tree scans. Require
findings to monotonically decrease without behavior loss. Commit identity, network/evidence, and scanner
fixture changes separately so each is reviewable and revertable.

## Task 4: Establish the public rights and security boundary

**Files:**
- Create or replace: `LICENSE`
- Create or update: `SECURITY.md`
- Create: `docs/PUBLIC_SOURCE_AUTHORITY.md`
- Modify: `README.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`

- [ ] **Step 1: Add RED boundary-document contract tests**

Require the exact authoritative repository, rights-reserved/inspection-only terms, security reporting route,
no trusted-binary implication, no external stable publication claim, and the private archive non-authority
boundary. Tests must reject permissive-license language and missing non-claims.

- [ ] **Step 2: Add the minimum boundary documents**

State that public source inspection does not grant reproduction, modification, redistribution, sublicense,
or sale rights without written permission. Keep `public_trusted_signing=false`,
`external_stable_publication=false`, `promotion_eligible=false`, and current operational version unchanged.

- [ ] **Step 3: Run document/source safety tests and commit**

Do not call the repository open source and do not create a release/package candidate.

## Task 5: Close the clean source-tree gate

- [ ] **Step 1: Run the complete managed/Web/reference test matrix**

Run restore, Release build, full .NET tests, Web install/tests/parity, migration-manifest checks, public-source
safety tests, and the complete legacy Pester reference suite. Pester is executed only as a temporary local
oracle; administrator/host mutation runners are excluded. Require failed/skipped/not-run `0`.

- [ ] **Step 2: Run independent public-safety scans**

Run official pinned Gitleaks against the current tree and require findings `0`. Run the Node verifier and
independent fixed-pattern scans; require personal path/identifier `0`, observed private endpoint `0`, private
archive provider identifier `0`, nested Git/provider export `0`, and unresolved P0/P1 `0`.

- [ ] **Step 3: Perform fixed-diff review**

Compare the clean source merge base through HEAD. Review every redaction, runtime-built security fixture,
workflow, license/security boundary, test result, and non-claim. Require open P0/P1/P2 `0` and dirty-main
digest match.

- [ ] **Step 4: Finalize source evidence and commit**

Record tool versions, canonical command/result summaries, counts/digests, source commit, host mutation false,
package candidate false, trusted signing false, and stable binary publication false. Keep raw candidate data
outside the repository.

## Task 6: Export and initialize the parentless local authority

- [ ] **Step 1: Validate exact export paths**

Resolve the clean source worktree, new sibling parent, OS-temp archive, and new repository target. Require the
source inside the private repository worktree root, the target exactly the fixed absent sibling, and no path
overlap. Stop if the target exists.

- [ ] **Step 2: Export committed tree only**

Create a tar archive with `git archive` from the exact sanitized source commit and extract it into the newly
created empty target. Do not copy `.git`, untracked files, worktree metadata, audit reports, or filesystem
timestamps as evidence.

- [ ] **Step 3: Initialize one-root Git history**

Run `git init --initial-branch=main`, set repository-local no-reply identity, stage the exported tree, and
create one root commit. Do not change global Git config. Record source-tree hash and new root-tree hash and
require equality.

- [ ] **Step 4: Verify Git isolation**

Require root parent count `0`, commit count `1`, branch set exactly `main`, tags/remotes/alternates/submodule
Git directories `0`, and `git fsck --full --no-reflogs` success. Confirm no old commit/ref/object is reachable.

- [ ] **Step 5: Re-run the full local gate in the new root**

Run all Task 5 tests/scans again from the new repository. Require clean status, Gitleaks `0`, public-safety
verifier `0`, unresolved P0/P1 `0`, and root fixed-diff review PASS before any provider mutation.

## Task 7: Bootstrap the new provider repository as private

- [ ] **Step 1: Reconfirm target absence and actor**

Read back `HardcoreMonk/purecvisor-desktop-node-public` as absent, authenticated actor exactly
`HardcoreMonk`, original archive still private/unchanged, local root clean, and dirty-main digest match.

- [ ] **Step 2: Create an uninitialized private repository**

Create only the exact target with visibility private, issues enabled, wiki disabled, and no provider-created
README, license, or gitignore. If creation returns an ambiguous result, read back before retrying.

- [ ] **Step 3: Push one explicit ref**

Add the exact HTTPS target as `origin` in the new local repository and push only
`refs/heads/main:refs/heads/main`. Never push all refs, tags, mirror refs, notes, or the private source branch.

- [ ] **Step 4: Verify private provider identity and inventory**

Require visibility `PRIVATE`, default branch/main SHA exact, one branch, zero tags/releases/issues/artifacts,
expected zero-or-current workflow runs only, no packages, and no imported branch/ruleset/protection/provider
history. Any unexpected object or metadata blocks publication.

- [ ] **Step 5: Require seed CI and provider safety audit PASS**

Wait for Development Gates and public-boundary workflows for the exact root SHA. Validate job identities,
conclusions, logs, and artifacts. Re-scan remote tree and provider metadata; require unresolved P0/P1 `0`.

## Task 8: Fresh one-way visibility checkpoint

- [ ] **Step 1: Present the exact irreversible action**

Report exact repository, root SHA, current `PRIVATE` visibility, local/provider audit digests, CI run/job IDs,
protection to be installed, rights boundary, and consequence that later returning private cannot recall clones,
caches, or forks.

- [ ] **Step 2: Request a fresh exact-target user approval and stop**

Do not infer approval from this plan or earlier broad approvals. The next provider mutation is allowed only
after the user explicitly confirms making `HardcoreMonk/purecvisor-desktop-node-public` public at the stated
root SHA.

## Task 9: Publish, protect main, and open the cutover PR

**Files:** no source mutation until the cutover branch is created

- [ ] **Step 1: Change only the approved repository visibility**

Use GitHub's explicit visibility-change acknowledgement. Immediately read back `PUBLIC`, owner, name, root
SHA, default branch, and rights/security documents. Enable private vulnerability reporting and verify it.

- [ ] **Step 2: Install current-four main protection**

Create a full protection body with strict status checks and exact contexts `dotnet-tests`, `web-tests`,
`packaging-pester`, `installer-web-pester`; admin enforcement enabled; force-push/deletion disabled. Preserve
the approved review/conversation settings from the design. Apply once and independently read back/canonicalize
the result. A mismatch triggers restoration/blocking, not a second blind mutation.

- [ ] **Step 3: Verify the public baseline**

Clone/read the repository anonymously, verify the exact root SHA/tree, run the public-source verifier against
the remote main tree, and confirm current four checks/protection. Record provider URLs and canonical digests.

- [ ] **Step 4: Create the protected cutover branch and draft PR**

Create `codex/pester-free-verification-cutover` from the new public `origin/main`, push without force, and open
a draft PR targeting `main`. Its body links Plan 2/3/4 and states that current required CI remains legacy until
same-SHA shadow and Wave E cutover.

- [ ] **Step 5: Finalize and commit bootstrap evidence through the draft PR**

Update the bootstrap evidence with measured public/protection/PR facts on the cutover branch. Do not merge a
documentation-only bootstrap change separately; it will land with the reviewed cutover PR.

## Completion checkpoint

1. Original private archive/provider state is unchanged and the dirty-main digest oracle matches.
2. New local/remote authority has exactly one parentless seed root and no old object/ref/tag/provider data.
3. Local and provider Gitleaks findings `0`; unresolved public-safety P0/P1 `0`.
4. Exact new target is `PUBLIC` only after fresh one-way approval.
5. Main protection has exact current four checks, strict freshness, admin enforcement, and disabled
   force-push/deletion with exact readback.
6. Seed CI passes on the exact root SHA; public baseline evidence contains measured IDs/digests.
7. Draft `codex/pester-free-verification-cutover` PR exists on the new authority.
8. Host/service/MSI/VM mutation `0`, package candidate `false`, public trusted signing `false`, and external
   stable binary publication `false`.

Stop on this checkpoint and begin the Wave C Installer plan. Do not change required-check identities yet.

## Failure and rollback

- Sanitization/test failure: keep all work local; do not create the provider repository.
- Private seed/provider failure: keep the new repository private and report exact state. Do not delete it
  without a separate destructive approval.
- P0/P1 after private seed: do not publish or rewrite exposed history. Remediate locally and design a new root
  or replacement private target as required.
- Visibility mutation without exact readback: block all pushes/PR merges and report exposure state.
- Protection mismatch: restore captured pre-protection body if one exists; otherwise remove only the newly
  created mismatched protection after exact-target verification and block Wave C.
- Any dirty-main digest mismatch or original archive mutation: stop immediately; do not attempt automatic
  restoration.
