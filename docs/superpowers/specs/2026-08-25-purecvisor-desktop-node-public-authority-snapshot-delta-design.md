# Public Authority Sanitized Snapshot Delta Design

State: approved
Implementation status: `completed`
Completion: PUBLIC parentless authority, protected main, PR #1/#2 merged; current main
`6e2bdb93ce308b632c929e2c17f5550ac3845401`
Approval: `2026-08-25 user-approved`
Parent design: `docs/superpowers/specs/2026-08-25-purecvisor-desktop-node-pester-free-required-ci-cutover-design.md`

## 1. Trigger and decision

The parent design intended to convert the existing `[private-archive-repository]` from private to public.
The pre-mutation audit stopped that path before any push, PR, or visibility change because existing remote
history contains actual public-safety P1 data:

- origin/main: user identifier in 26 files / 28 occurrences
- origin/main: absolute user-profile path in 23 files / 25 occurrences
- origin/main: private-network address in 79 files / 122 occurrences
- remote history: user identifier in 24 commits
- remote history: absolute user-profile path in 22 commits
- remote history: private-network address changes in 94 commits

The user selected a new public repository and approved it as the authoritative landing remote. The exact
target is:

```text
HardcoreMonk/purecvisor-desktop-node-public
```

The original repository remains private and is not renamed, archived, force-pushed, or used as the public
provider target. Its current history, provenance hashes, branches, issues, Actions data, packages, releases,
and tags are not copied. Public copies also replace textual private provider identifiers and source-history
locators with explicit archive placeholders.

## 2. Authority boundary

After the new repository seed is published and protected:

- `purecvisor-desktop-node-public` owns new source integration, pull requests, Required CI checks, and the
  Wave C/D/E cutover merge.
- The original repository remains a private historical archive. This delta does not modify its GitHub
  description, archived flag, name, visibility, default branch, or refs.
- The original local dirty main remains user-owned and untouched. Its uncommitted files are not exported.
- Public source is rights-reserved/inspection-only. Public source authority is not a trusted binary release
  authority and does not change `public_trusted_signing=false` or `external_stable_publication=false`.

The new repository name is intentionally distinct. GitHub redirects and accidental clone retargeting are
not used.

## 3. Seed source and history isolation

The seed tree combines only committed source:

1. local main committed HEAD `[private-source-commit]`
2. Wave B/design/plan branch committed HEAD and successors
3. an isolated merge of those two committed histories
4. public-safety sanitization commits on an unpushed private source branch

The merge and sanitization happen in a clean worktree. The dirty main worktree is never switched, staged,
stashed, cleaned, reset, pulled, or merged.

History isolation uses a filesystem snapshot, not a push of the old object database:

```text
private clean seed-source branch
  -> git archive of one sanitized tree
  -> extract into a separate empty directory
  -> git init --initial-branch=main
  -> one root commit
  -> new GitHub repository
```

The separate local repository path is:

```text
D:/data/projects/codex-zone/purecvisor-desktop-node-public
```

It starts with no old object, reflog, alternates file, submodule Git directory, remote, tag, branch, or PR
ref. The root commit uses repository-local identity only:

```text
user.name=HardcoreMonk
user.email=254846378+HardcoreMonk@users.noreply.github.com
```

No global Git identity is changed.

## 4. Sanitization contract

### 4.1 Personal and host identity

Every real user identifier and absolute profile path is replaced with stable public-safe terms. The design
does not repeat the private value; the private audit report supplies it to the scoped sanitizer:

```text
[private-user-id] -> Operator
[private-profile-path] -> C:/Users/Operator
```

The replacement is case-aware and applies to tracked source/docs/tests only. It does not edit the private
worktree or OS paths. A case-insensitive post-scan must find zero old identifiers.

Machine names, private share names, and any other user/host-specific identifiers found during the same scan
are treated as P1 and replaced before export.

### 4.2 Network endpoints

Operational evidence that recorded an actually observed RFC1918 address or private hostname is sanitized.
Documentation-safe endpoint examples use RFC 5737 TEST-NET addresses or explicit `[redacted-private-endpoint]`
when an address would misrepresent private-network semantics.

RFC1918 values may remain only when all of these are true:

1. the owning file is source, schema, or a deterministic test fixture rather than observed evidence;
2. the value is required to exercise private-network validation;
3. the nearby identifier says `synthetic`, `fixture`, `example`, or `test`;
4. no real host/user/batch relationship is retained;
5. a negative fixture proves the public/private network branch intentionally.

Schema IDs under `.invalid`, `.example`, or an explicitly synthetic `.local` fixture are P2, not internal
provider endpoints. Actual provider/host URLs are P1.

### 4.3 Secret-scanner fixtures

The private audit found 25 historical and 14 current Gitleaks matches. Investigation classified them as
P2 non-secret fixtures: repeated test signing material, synthetic invalid PEM/JWT redactor inputs,
certificate thumbprint fixtures, descriptor IDs, and digest-only evidence.

The public root nevertheless targets Gitleaks findings `0`:

- high-entropy test inputs are built at runtime from reviewed fragments or generated in contained temp
  fixtures;
- labels such as `SigningKey`, `Token`, and `secretThumbprint` do not have a static credential-shaped value;
- digest evidence is explicitly labeled a non-reversible SHA-256 fingerprint;
- invalid PEM/JWT fixtures remain nonfunctional and are generated inside tests;
- no broad `.gitleaksignore` or fingerprint allowlist hides findings.

If a scanner finding cannot be eliminated without weakening a security test, it requires a narrowly scoped
documented fixture design review before the seed can pass. Scanner exit-code suppression is forbidden.

### 4.4 Evidence and claims

Historical evidence remains semantically historical but public copies must not retain personal paths or
observed private endpoints. Redaction is stated explicitly rather than replacing an observed value with a
fabricated PASS value. Package hashes, non-secret provenance identifiers, public API route names, and test
contract IDs may remain.

Current operational version and claims remain:

```text
version=0.42.74-admin-smoke
promotion_eligible=false
public_trusted_signing=false
external_stable_publication=false
package_candidate_created=false
host_mutation_performed=false
```

## 5. Local verification gate

Before any new GitHub repository is created, the separate root-commit repository must pass:

1. `git fsck --full --no-reflogs` with zero corruption and no parent commit for root `main`.
2. Exactly one local branch (`main`), zero tags, zero remotes, zero alternates, zero submodules carrying a
   private Git directory.
3. Gitleaks official pinned binary current-tree scan: findings `0`.
4. Case-insensitive old username/profile path scan: `0`.
5. Observed private endpoint scan in operational evidence: `0`.
6. Email/private key/live token/certificate-secret scan: unresolved P0/P1 `0`.
7. Full .NET Release, Web required-equivalent commands, legacy Pester reference, and documentation/source
   contract tests PASS with skipped/not-run `0`.
8. Rights-reserved `LICENSE`, `SECURITY.md`, public authority boundary, and no binary publication claim.
9. Fixed root-tree review with P0/P1 `0` and no dirty-main content.

Raw reports stay under a validated OS-temp audit root. The public repository receives only redacted counts,
tool versions, report digests, and exact commands.

## 6. Provider bootstrap and one-way public gate

After local PASS:

1. Create `HardcoreMonk/purecvisor-desktop-node-public` as **private**, with issues enabled, wiki disabled,
   no README/license/gitignore initialization, and default branch established by the explicit seed push.
2. Add only that exact repository as the new local repository's `origin`.
3. Push only `refs/heads/main:refs/heads/main`. Do not use `--all`, `--mirror`, `--tags`, or wildcard refspecs.
4. Read back one branch, zero tags/releases/issues/artifacts/packages, exact root SHA, visibility private, and
   expected Actions runs.
5. Require the seed Development Gates/current public-boundary workflows to PASS by exact SHA. These are
   temporary legacy checks before Wave E.
6. Scan new provider logs/metadata and the remote main tree again; unresolved P0/P1 must be zero.
7. Immediately before visibility mutation, present exact target, private-to-public consequence, root SHA,
   and fresh audit result for a separate one-way user confirmation.
8. Convert only the new repository to public, independently read it back, enable private vulnerability
   reporting, and install main protection with the current four Development Gates checks.

The single seed push is the empty-repository bootstrap exception. After protection is installed, every
source change lands through a protected PR.

## 7. Required CI migration on the new authority

Create `codex/pester-free-verification-cutover` from the protected public root and execute:

1. Wave C Installer 49 contracts
2. Wave D Packaging 528 contracts and current-evidence handler
3. same-SHA shadow under the current four protected check identities
4. one direct-child Wave E cutover commit
5. cutover SHA's new checks: `dotnet`, `web`, `delivery`, `installer-policy`
6. atomic main-protection context replacement with before/after readback
7. cutover PR merge and exact main post-merge CI
8. documentation-only evidence PR and final main CI

The legacy source files stay tracked. Required CI Pester and non-admin PowerShell invocation become zero;
non-required residue is disclosed separately.

## 8. Failure and rollback

- Any local sanitization or test failure: do not create the GitHub repository.
- New remote creation succeeds but seed/audit fails: keep it private and empty or private with the failed
  seed; do not delete or publicize it without a separate destructive decision.
- P0/P1 after seed push: keep private, rotate/remediate as applicable, create a new root commit/repository
  design rather than rewriting the new public history after exposure.
- Public visibility after fresh approval is irreversible exposure; returning private cannot recall copies.
- Provider protection mismatch: restore captured before state and block feature PRs.
- Original private repository is never used as rollback target for a force-push. Its state stays unchanged.

## 9. Completion conditions

1. Original repository remains private and remote-ref/metadata state unchanged.
2. Dirty main's eight user-owned files retain their recorded SHA-256 values.
3. New repository root has no private parent/history/provider data.
4. Public-root Gitleaks findings `0`; unresolved P0/P1 `0`.
5. Exact new target is public and protected after fresh one-way approval.
6. Full ledger `62/62` files and `627/627` contracts reaches same-SHA dual-run PASS.
7. Required CI exact four-job union passes with Pester/PowerShell/mutation invocation `0` and wall-clock
   at most 3:34.
8. Cutover and evidence PRs merge through the protected new authority; final main CI passes.
9. Rights-reserved source/public-security boundary is present.
10. Trusted signing, package candidate, host mutation, and external stable binary publication remain false.
