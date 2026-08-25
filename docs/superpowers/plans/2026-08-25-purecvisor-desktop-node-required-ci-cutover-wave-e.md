# Required CI Shadow, Pester-free Cutover, and Merge Wave E Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development, superpowers:executing-plans, superpowers:requesting-code-review, superpowers:verification-before-completion, and superpowers:ship in that order at the applicable checkpoints. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 같은 commit에서 legacy 627 Pester contracts와 replacement 627 contracts를 dual-run하여 CI parity를 고정한 뒤, 단일 후속 commit으로 Required CI 네 job을 Pester/비관리자 PowerShell invocation 0의 C#/Node shards로 cut over하고, GitHub main protection을 새 check identities로 원자적으로 전환하여 PR을 병합·push하고 final remote-main CI를 증명한다.

**Architecture:** `RequiredCiPolicy`는 YamlDotNet AST로 Development Gates workflow를 구조적으로 읽고 catalog activation에 따라 `shadow-ready` 또는 `active` 계약을 적용한다. Shadow commit은 기존 protected job names와 legacy steps를 유지하면서 replacement runner를 같은 SHA에서 추가한다. 바로 다음 cutover commit은 workflow/catalog/manifest/evidence allowlist만 변경한다. Provider required checks는 cutover SHA의 새 네 checks가 PASS한 뒤 one-request compare/readback 방식으로 교체한다.

**Tech Stack:** .NET 10, xUnit 2.9.3, YamlDotNet 18.1.0, Node.js 24, Git/GitHub CLI/API, GitHub Actions immutable action SHAs, Pester 5.7.1 shadow reference only, Visual Studio 2026 또는 `dotnet` CLI.

---

## Preconditions and exact completion claim

- Active branch: `codex/pester-free-verification-cutover`; draft PR targets protected public `main` in `HardcoreMonk/purecvisor-desktop-node-public`.
- All provider reads, pushes, protection changes, PR operations, and merge commands target that new authority. The original private archive remains unchanged.
- Remote main SHA and branch-protection current contexts are captured and unchanged since Plan 1.
- Wave C/D local ledger: files `62/62`, contracts `627/627`, local PASS `627`, unmapped/missing/duplicate/order drift `0`, CI pending `627`.
- Legacy reference totals: Packaging `528`, Installer `49`, Web `50`, total `627`; failed/skipped/not-run `0` at local checkpoints.
- Final Required CI job IDs and check names are exactly `dotnet`, `web`, `delivery`, `installer-policy`.
- Final Required CI executable/shell/run tokens contain Pester `0`, `pwsh` `0`, `powershell` `0`, and host mutation tools `0`.
- Legacy Pester source files remain tracked for rollback/manual reference. Non-required workflows are not silently represented as part of the four-job Required CI union.
- `.github/workflows/public-boundary.yml` is not protected by this program and is not modified by the single cutover commit. Its legacy PowerShell/Pester use must be disclosed as non-required residue, not counted as Required CI zero.
- Final required wall-clock is measured from earliest start to latest completion across the four parallel jobs and must be at most `214` seconds (3:34). If exceeded, optimize and rerun; do not waive.
- No version bump, root `VERSION`, `CHANGELOG`, package candidate, package build, trusted signing, stable binary publication, host/service/MSI/VM mutation, direct main push, or force-push.

## Immutable third-party action pins

Use only these reviewed pins in `.github/workflows/development-gates.yml`:

```text
actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd        # v6.0.2
actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d   # v5.0.0
actions/cache@a7833574556fa59680c1b7cb190c1735db73ebf0          # v5.0.0
actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903     # v6.0.0
actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f # v6.0.0
```

YamlDotNet is pinned to `18.1.0`; the official NuGet entry declares .NET 10 compatibility: `https://www.nuget.org/packages/YamlDotNet/18.1.0`.

## File map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Verification/RequiredCiPolicy.cs` | Structured shadow/active workflow contract and forbidden executable scan. |
| `src/DesktopNode.Verification/CutoverGitBoundary.cs` | Shell-free `git` argument-array parent/diff verification. |
| `src/DesktopNode.Verification/ManagedSuiteRunner.cs` | Activates `policy-boundaries`. |
| `src/DesktopNode.Verification/DesktopNode.Verification.csproj` | Pinned YamlDotNet dependency. |
| `src/DesktopNode.Verification.Tests/RequiredCiPolicyTests.cs` | YAML AST positive/negative fixtures. |
| `src/DesktopNode.Verification.Tests/CutoverGitBoundaryTests.cs` | Parent/allowlist/wrong-SHA fixtures with temporary Git repos. |
| `src/DesktopNode.Delivery.Tests/Delivery/Verification/PcvDevelopmentGateWorkflowContractTests.cs` | Legacy mapping updated from shadow to final contract at cutover. |
| `web/package.json` | Pester-free required Web command composition. |
| `.github/workflows/development-gates.yml` | Shadow first, then final required four-job workflow. |
| `config/development-verification-suites.json` | `shadow-ready` then `active`; shell-free suite argument arrays. |
| `config/development-verification-suites.schema.json` | Strict state/executable schema. |
| `config/development-verification-migration-manifest.json` | Immutable shadow locator and 627 cutover parity rows. |
| `config/development-verification-migration-manifest.schema.json` | Strict cutover locator and state rules. |
| `docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md` | Shadow/cutover/protection evidence. |
| `docs/DEVELOPMENT_VERIFICATION_POLICY.md` | Final four required commands and legacy residue boundary. |
| `docs/ga-ready/EVIDENCE_INDEX.md` | Cutover evidence entrypoint. |

## Task 1: Implement the structured Required CI policy guard

**Files:** `RequiredCiPolicy.cs`, its tests, Verification project file, architecture/package tests, managed runner tests

- [ ] **Step 1: Write YAML AST RED tests before adding the package**

Fixtures must reject duplicate jobs, anchors/aliases that change executable semantics, missing/extra required job, skipped/conditional required job, wrong runner, missing setup, mutable action tag, `shell: pwsh`, `powershell`, `Invoke-Pester`, `Install-Module`, command hidden in folded/literal YAML, forbidden mutation token, duplicate shard, missing artifact, wrong artifact path, catalog/workflow mismatch, and active workflow with a shadow legacy step. Add a valid shadow fixture and a valid final fixture.

- [ ] **Step 2: Add the sole new production dependency**

Add exactly:

```xml
<PackageReference Include="YamlDotNet" Version="18.1.0" />
```

Update project/package contract tests to allow this one production package and no transitive direct declaration. Run the focused test and confirm it now compiles but fails because `RequiredCiPolicy` does not exist.

- [ ] **Step 3: Implement the fixed policy API**

```csharp
internal enum RequiredCiMode
{
    Shadow,
    Active
}

internal sealed record RequiredCiPolicyResult(
    RequiredCiMode Mode,
    IReadOnlyList<string> JobIds,
    IReadOnlyList<string> Shards,
    int PesterInvocationCount,
    int NonAdminPowerShellInvocationCount,
    int HostMutationInvocationCount);

internal static class RequiredCiPolicy
{
    internal static RequiredCiPolicyResult Validate(
        string workflowYaml,
        VerificationCatalog catalog);
}
```

Traverse `YamlStream` mapping/sequence/scalar nodes, reject duplicate semantic keys, and inspect only executable fields (`shell`, `run`, local/docker action, executor file/arguments). Documentation text and legacy-path evidence locators are not executable invocations. In active mode require exact ordered job set and exact shard union once each.

- [ ] **Step 4: Implement shadow vs active rules**

`shadow-ready` permits Pester/PowerShell only in the four explicitly named legacy steps and requires a replacement shard plus separate legacy/replacement artifacts in the same job. `active` requires all three invocation counts zero and no legacy step. Both modes reject admin/mutation commands.

- [ ] **Step 5: Wire `policy-boundaries` managed execution**

`ManagedSuiteRunner` reads workflow/catalog/manifest through contained paths, calls the policy and ledger validators, and returns PASS only when the relevant mode is coherent. Cancellation and malformed YAML return stable failure without outputting source content.

- [ ] **Step 6: Run focused and full Verification tests, then commit**

```powershell
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --filter "FullyQualifiedName~RequiredCiPolicy|FullyQualifiedName~ManagedSuiteRunner" --nologo
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
git diff --check
```

Commit message: `test: enforce structured Required CI policy`.

## Task 2: Implement the cutover parent and diff allowlist guard

**Files:** `CutoverGitBoundary.cs`, its tests, policy managed runner, manifest/schema tests

- [ ] **Step 1: Write temporary-Git RED tests**

Create contained repositories with exact commits and assert PASS only when the history reachable from the supplied verification HEAD contains exactly one direct child of manifest `shadow_sha`, that child is a non-merge cutover commit, and its `shadow_sha..cutover_sha` paths are within the cutover allowlist. This works for a branch head, GitHub's synthetic PR merge commit, the real main merge commit, and later documentation commits. Reject missing/shallow history, zero/multiple direct children, wrong parent, dirty index/worktree, rename/copy escape, submodule/gitlink, extra path, absolute output, and non-40-hex SHA.

- [ ] **Step 2: Use argument-array Git invocations only**

Call the already guarded process boundary with `git rev-parse`, `git status --porcelain=v1`, `git rev-list --parents $shadowSha..HEAD`, `git merge-base --is-ancestor`, and `git diff --name-status --no-renames $shadowSha..$cutoverSha`. `UseShellExecute=false`; no command string, pipe, redirection, PowerShell, or shell.

- [ ] **Step 3: Freeze the active cutover allowlist**

```text
.github/workflows/development-gates.yml
config/development-verification-suites.json
config/development-verification-suites.schema.json
config/development-verification-migration-manifest.json
config/development-verification-migration-manifest.schema.json
src/DesktopNode.Delivery.Tests/Delivery/Verification/PcvDevelopmentGateWorkflowContractTests.cs
docs/DEVELOPMENT_VERIFICATION_POLICY.md
docs/ga-ready/EVIDENCE_INDEX.md
docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md
```

All guard implementation, Web script composition, package reference, and shadow support must be committed before the shadow workflow commit so the cutover commit needs no path outside this list.

- [ ] **Step 4: Run tests and commit**

Commit message: `test: guard cutover parent and diff scope`.

## Task 3: Prepare Pester-free Web and executable catalog commands before shadow

**Files:** `web/package.json`, Web architecture tests, catalog/schema/loader/summary tests, application activation tests

- [ ] **Step 1: Add a RED Web required-command contract**

Require `test:required` to compose `npm test`, strict v2 manifest/Web positive/negative Node tests, and static/browser parity without `verify:web-contract-negative-parity` because that transitional command invokes Pester.

- [ ] **Step 2: Add the exact script composition**

Set `test:web-contracts` to execute all Web contract, negative, manifest, and architecture Node test files; set:

```json
"test:required": "npm test && npm run test:web-contracts && npm run verify:parity"
```

Keep the Pester-backed negative-parity command available for shadow evidence only; no final catalog suite may reference it.

- [ ] **Step 3: Make process suite commands prebuilt/no-restore**

Add `--no-build` and `--no-restore` to .NET test suite argument arrays. Set `web-typecheck` to `npm run test:required --prefix web` and remove duplicate work from `web-parity` by making it a small read-only manifest/parity check that does not rerun the whole suite. Keep all executable names inside the existing allowlist.

- [ ] **Step 4: Permit actual execution only for `shadow-ready` or `active`**

Update application tests first. `plan-only-foundation` remains blocked before a process call. `shadow-ready` and `active` may execute the safe catalog. Unknown activation is rejected.

- [ ] **Step 5: Verify and commit before the shadow commit**

```powershell
npm ci --prefix web
npm run test:required --prefix web
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
git diff --check
```

Commit message: `test: prepare shell-free required verification commands`.

## Task 4: Create the same-SHA shadow workflow commit

**Files:** `.github/workflows/development-gates.yml`, suite catalog/schema, workflow owner test, shadow evidence skeleton

- [ ] **Step 1: Freeze remote-main freshness immediately before shadow**

Fetch and require the draft PR branch to contain current `origin/main`, with no other open merge expected during the shadow/cutover window. Record remote-main SHA. If main already moved, merge it now, rerun local gates, and only then create the shadow commit.

- [ ] **Step 2: Convert the catalog to `shadow-ready`**

All seven suites have mapped/native states; all four shards are executable; `current-evidence-check` and `policy-boundaries` are available. Manifest remains local PASS/CI pending `627`.

- [ ] **Step 3: Preserve four existing protected job identities and append replacement paths**

| Job ID | Legacy path | Replacement path | Required artifacts |
| --- | --- | --- | --- |
| `dotnet-tests` | existing solution test | runner `--shard dotnet` | `legacy-dotnet`, `replacement-dotnet` |
| `web-tests` | existing npm test/parity | `npm run test:required` + runner `--shard web` | `legacy-web`, `replacement-web` |
| `packaging-pester` | Pester 55 files / 528 | runner `--shard delivery` | `legacy-packaging`, `replacement-delivery` |
| `installer-web-pester` | Pester 6+1 files / 99 | runner `--shard installer-policy` plus Node Web contracts | `legacy-installer-web`, `replacement-installer-policy` |

Each legacy summary contains passed/failed/skipped/not-run/duration and SHA-256 only; no raw secrets or absolute paths. Each replacement artifact contains the runner `summary.json`, manifest summary, and test-result count. Use immutable upload-artifact pin.

- [ ] **Step 4: Run shadow policy locally before commit**

Expected mode `Shadow`, exact existing four jobs, exact replacement shards, and Pester/PowerShell only in allowed legacy steps. Host mutation invocation count must be zero.

- [ ] **Step 5: Create exactly one shadow workflow commit**

```powershell
git add .github/workflows/development-gates.yml config/development-verification-suites.json config/development-verification-suites.schema.json src/DesktopNode.Delivery.Tests/Delivery/Verification/PcvDevelopmentGateWorkflowContractTests.cs docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md
git diff --cached --check
git commit -m "ci: dual-run legacy and replacement verification"
git rev-parse HEAD
git push origin codex/pester-free-verification-cutover
```

Record this HEAD as `shadow_sha`. Do not commit anything else before the cutover commit.

## Task 5: Validate immutable shadow CI evidence

**Files:** no tracked edit; external GitHub run/artifacts only

- [ ] **Step 1: Find the exact run by `headSha=shadow_sha`**

Use Actions API, not merely the latest run. Require event is the draft PR, workflow path is `.github/workflows/development-gates.yml`, and all four existing job IDs report success.

- [ ] **Step 2: Download every shadow artifact into a contained OS-temp directory**

Validate artifact names, producing job ID, embedded commit SHA, summary schema, count, duration, and SHA-256. Require no expired/missing artifact.

- [ ] **Step 3: Require exact parity totals**

```text
legacy Pester: 627 passed, 0 failed, 0 skipped, 0 not-run
replacement ledger: 627 mapped/local-pass contracts
replacement Web: 50 passed, deterministic negative suite passed
replacement Installer: 49 passed
replacement Delivery: 528 passed
full .NET Release: all assemblies passed, skipped 0
host mutation: false
```

- [ ] **Step 4: Run fixed shadow review**

Confirm legacy and replacement came from the exact same commit, no retry used a different SHA, artifact digests match API downloads, and CI parity evidence is immutable. A rerun is permitted only for diagnosed infrastructure flake and must retain the same SHA; application/test failure requires a new shadow commit and restart from Task 4.

## Task 6: Create the single Wave E cutover commit

**Files:** only the fixed cutover allowlist

- [ ] **Step 1: Populate the immutable predecessor evidence**

Update the evidence document with shadow SHA/run URL/run ID/job IDs/artifact IDs/digests/counts/durations. Update manifest/schema with one cutover locator containing shadow SHA/run ID/URL and set all 627 contract/file rows to `cutover`, local PASS, CI PASS. This is the only permitted persisted `mapped -> cutover` transition: the guard must prove its parent is that shadow SHA and its immutable run satisfies `dual-run-pass`; therefore the logical intermediate is evidenced but no self-referential intermediate commit is inserted. Do not store the future cutover SHA in its own contents.

- [ ] **Step 2: Set catalog activation to `active`**

Require all suite states `cutover`; exact four shards; safe executable arrays; current evidence and policy handlers active; no Pester/PowerShell/mutation executable token.

- [ ] **Step 3: Replace Development Gates with this exact four-job structure**

The implemented YAML uses the following complete job bodies; preserve the existing trigger, read-only permissions, and concurrency header from the design:

```yaml
jobs:
  dotnet:
    name: dotnet
    runs-on: windows-latest
    timeout-minutes: 15
    steps:
      - uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
        with:
          fetch-depth: 0
      - uses: actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d # v5.0.0
        with:
          dotnet-version: 10.0.x
      - uses: actions/cache@a7833574556fa59680c1b7cb190c1735db73ebf0 # v5.0.0
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('src/**/*.csproj') }}
      - name: Restore and build
        shell: cmd
        run: dotnet restore src\DesktopNode.sln && dotnet build src\DesktopNode.sln -c Release --no-restore
      - name: Run dotnet shard
        shell: cmd
        run: dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-gates-dotnet --shard dotnet
      - if: always()
        uses: actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f # v6.0.0
        with:
          name: development-gates-dotnet-${{ github.run_id }}
          path: artifacts/development-gates-dotnet
          if-no-files-found: error

  web:
    name: web
    runs-on: ubuntu-latest
    timeout-minutes: 15
    steps:
      - uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
        with:
          fetch-depth: 0
      - uses: actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d # v5.0.0
        with:
          dotnet-version: 10.0.x
      - uses: actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903 # v6.0.0
        with:
          node-version: 24
          cache: npm
          cache-dependency-path: web/package-lock.json
      - name: Restore and build verifier
        run: dotnet restore src/DesktopNode.Verification/DesktopNode.Verification.csproj && dotnet build src/DesktopNode.Verification/DesktopNode.Verification.csproj -c Release --no-restore
      - name: Install Web dependencies
        run: npm ci --prefix web
      - name: Run web shard
        run: dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/development-gates-web --shard web
      - if: always()
        uses: actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f # v6.0.0
        with:
          name: development-gates-web-${{ github.run_id }}
          path: artifacts/development-gates-web
          if-no-files-found: error

  delivery:
    name: delivery
    runs-on: windows-latest
    timeout-minutes: 15
    steps:
      - uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
        with:
          fetch-depth: 0
      - uses: actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d # v5.0.0
        with:
          dotnet-version: 10.0.x
      - name: Restore and build verification projects
        shell: cmd
        run: dotnet restore src\DesktopNode.Verification\DesktopNode.Verification.csproj && dotnet restore src\DesktopNode.Delivery.Tests\DesktopNode.Delivery.Tests.csproj && dotnet build src\DesktopNode.Verification\DesktopNode.Verification.csproj -c Release --no-restore && dotnet build src\DesktopNode.Delivery.Tests\DesktopNode.Delivery.Tests.csproj -c Release --no-restore
      - name: Run delivery shard
        shell: cmd
        run: dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/development-gates-delivery --shard delivery
      - if: always()
        uses: actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f # v6.0.0
        with:
          name: development-gates-delivery-${{ github.run_id }}
          path: artifacts/development-gates-delivery
          if-no-files-found: error

  installer-policy:
    name: installer-policy
    runs-on: windows-latest
    timeout-minutes: 15
    steps:
      - uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
        with:
          fetch-depth: 0
      - uses: actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d # v5.0.0
        with:
          dotnet-version: 10.0.x
      - name: Restore and build verification projects
        shell: cmd
        run: dotnet restore src\DesktopNode.Verification\DesktopNode.Verification.csproj && dotnet restore src\DesktopNode.Delivery.Tests\DesktopNode.Delivery.Tests.csproj && dotnet build src\DesktopNode.Verification\DesktopNode.Verification.csproj -c Release --no-restore && dotnet build src\DesktopNode.Delivery.Tests\DesktopNode.Delivery.Tests.csproj -c Release --no-restore
      - name: Run installer and policy shard
        shell: cmd
        run: dotnet run --project src\DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/development-gates-installer-policy --shard installer-policy
      - if: always()
        uses: actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f # v6.0.0
        with:
          name: development-gates-installer-policy-${{ github.run_id }}
          path: artifacts/development-gates-installer-policy
          if-no-files-found: error
```

On Windows every executable `run` step explicitly uses `shell: cmd`; on Ubuntu the default shell is bash. The policy must count non-admin PowerShell invocations as zero.

- [ ] **Step 4: Update final workflow owner/policy documentation**

Update only the cutover allowlist files. State that legacy Pester remains rollback/manual reference and `.github/workflows/public-boundary.yml` is non-required residue. Do not claim repository-wide script deletion.

- [ ] **Step 5: Commit and prove parent/diff locally**

```powershell
git diff --cached --check
git commit -m "ci: cut over required verification from Pester"
git rev-parse HEAD^
git diff --name-only HEAD^..HEAD
```

Expected: parent is exact `shadow_sha`; changed paths are a subset of the fixed allowlist; commit is not a merge commit. Run the active policy locally and require exact four jobs, Pester `0`, non-admin PowerShell `0`, host mutation `0`, manifest `627/627 cutover`.

- [ ] **Step 6: Push without force**

Push `codex/pester-free-verification-cutover`. Do not change provider required contexts yet.

## Task 7: Validate new checks, then atomically switch main protection

**Files:** no tracked change; external branch protection state

- [ ] **Step 1: Wait for cutover-SHA checks by exact SHA**

Require successful check runs `dotnet`, `web`, `delivery`, `installer-policy`; no old required job is expected from the final workflow. Download and validate all four summary artifacts.

- [ ] **Step 2: Verify final performance and static counts**

Require union suite missing/duplicate/skip `0`, Pester/PowerShell/mutation invocation `0`, all summaries bound to cutover SHA, and wall-clock `<=214000 ms` based on provider job timestamps.

- [ ] **Step 3: Capture current protection and ETag**

GET main protection. Require exact old contexts `dotnet-tests`, `web-tests`, `packaging-pester`, `installer-web-pester`, strict true, admin enforcement true, force-push/deletion false. Canonicalize and hash the response; any drift stops the mutation.

- [ ] **Step 4: Replace only required check contexts in one request**

Send the same protection body as Plan 1 with this exact context array:

```json
["dotnet", "web", "delivery", "installer-policy"]
```

Preserve every other protection field exactly. If the API supports conditional `If-Match`, use the captured ETag; otherwise perform immediate compare-before-PUT and compare-after-GET.

- [ ] **Step 5: Independently read back or restore**

Require exact new four contexts and unchanged safety fields. Hash before/after responses. On mismatch, restore the exact old four contexts and block merge; never leave a partial/mixed set.

## Task 8: Final review, ready transition, and cutover PR merge

**Files:** PR description/checklist only; no new feature commit

- [ ] **Step 1: Reconfirm main has not moved since shadow**

If `origin/main` differs from the frozen main SHA, do not merge or append a merge commit after cutover. Abandon this remote branch without force, create a successor from latest main, cherry-pick the reviewed Wave C/D/pre-shadow work, and repeat shadow plus a new single cutover pair.

- [ ] **Step 2: Run the full local final gate**

```powershell
dotnet restore src/DesktopNode.sln
dotnet build src/DesktopNode.sln -c Release --no-restore
dotnet test src/DesktopNode.sln -c Release --no-build --no-restore --nologo
npm ci --prefix web
npm run test:required --prefix web
npm run check:verification-migration-manifest --prefix web
git diff --check origin/main...HEAD
```

Run `pcvverify` for each of the four shards with separate contained artifact roots and require PASS.

- [ ] **Step 3: Perform pre-landing fixed-diff review**

Review architecture, SQL/data safety if present, trust boundaries, workflow/action pins, ledger mapping, diff allowlist, public claims, and docs. Require P0/P1/P2 open findings `0` before merge.

- [ ] **Step 4: Update the draft PR body and mark ready**

Include parentless-root bootstrap/publication evidence, Wave C/D counts, shadow SHA/run/artifacts, cutover SHA, protection before/after digests, final four checks, performance, rollback commit, residue boundary, and non-claims. Mark ready only after body review.

- [ ] **Step 5: Require protected checks and merge**

Use `gh pr checks` and branch-protection readback again. Merge with repository-permitted non-force merge method:

```text
gh pr view --repo HardcoreMonk/purecvisor-desktop-node-public --json number,url,headRefName,baseRefName
gh pr merge <cutover-pr-number> --repo HardcoreMonk/purecvisor-desktop-node-public --merge --delete-branch
git fetch origin --prune
```

Expected: PR `MERGED`; remote main includes the cutover commit; no direct main push and no force update.

## Task 9: Verify final remote main and publish post-merge evidence

**Files:**
- Modify on a documentation-only follow-up branch: cutover evidence, Evidence Index, Development Verification Policy if observed facts require it

- [ ] **Step 1: Verify the exact cutover merge on remote main**

Read PR merge commit and `origin/main`. Require cutover commit ancestry. Do not pull/reset the dirty local main worktree.

- [ ] **Step 2: Wait for the remote-main Development Gates run**

Find by exact merge `headSha`. Require new four jobs successful, artifacts valid, Pester/PowerShell/mutation invocation `0`, wall-clock `<=214s`, and branch protection exact.

- [ ] **Step 3: Create a documentation-only evidence branch from remote main**

Create an isolated branch `codex/pester-free-cutover-evidence-20260825` from `origin/main`. Update the evidence with cutover PR/merge SHA, main run/job/artifact IDs and URLs, exact counts/digests/durations, protection before/after, residue disclosure, rollback command, and non-claims. Do not modify current operational version or package evidence.

- [ ] **Step 4: Push, PR, verify and merge the evidence update**

Push without force, open a documentation-only PR, require the new protected four jobs, fixed-diff review, then merge with a merge commit. Record its final main run in the handoff; do not create an infinite self-referential evidence update loop.

- [ ] **Step 5: Final provider and dirty-main readbacks**

Require repository PUBLIC, main protection exact new four, no open cutover PR, final main CI success, and local dirty main status unchanged from Plan 1.

## Completion conditions

1. Parentless public-root/provider audit unresolved P0/P1 `0`, Gitleaks finding `0`, repository visibility `PUBLIC` readback exact, and original private archive unchanged.
2. Legacy same-SHA shadow: Pester `627/627`, failed/skipped/not-run `0`.
3. Replacement: Web `50/50`, Installer `49/49`, Delivery `528/528`, manifest `627/627`, missing/duplicate/unmapped `0`.
4. Required CI exact four-job union: `dotnet`, `web`, `delivery`, `installer-policy`.
5. Required CI Pester invocation `0`, non-admin PowerShell invocation `0`, host mutation invocation `0`.
6. Required wall-clock `<=3:34` and all four artifacts bound to the exact SHA.
7. Branch protection strict/base-fresh with exact new contexts; force-push/deletion disabled; before/after/rollback material complete.
8. Cutover PR and evidence PR merged without force; final remote-main CI PASS.
9. Legacy Pester retained; non-required `.github/workflows/public-boundary.yml` residue disclosed.
10. Operational version remains `0.42.74-admin-smoke`; feature promotion blocker remains; public trusted signing and external stable binary publication remain false.

## Rollback and failure handling

- Before branch-protection change: revert the single cutover commit to restore shadow workflow/catalog/manifest, push the revert normally, and keep old required contexts.
- After protection change but before merge: restore old four contexts from the captured body, verify readback, then revert the cutover commit.
- After merge: create a normal revert PR for the cutover commit and switch protection back only after the restored shadow/current jobs pass. Never force-reset main.
- Main drift after shadow invalidates the shadow/cutover pair; restart on a successor branch rather than appending a merge to the cutover commit.
- Visibility exposure cannot be rolled back by making the repository private. Treat any post-public P0/P1 as an exposure incident.
- Any missing artifact, wrong SHA, skipped test, wall-clock breach, protection mismatch, open review finding, or dirty-main mutation blocks completion.
