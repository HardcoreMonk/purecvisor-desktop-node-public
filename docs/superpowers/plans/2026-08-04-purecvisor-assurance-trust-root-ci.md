# PureCVisor Assurance Trust Root and False-Green CI Implementation Plan

> **Status:** proposed child plan 2; starts only after Plan 1's unconsumed fresh-main trust-root
> bootstrap decision passes the v4 one-time external validation and is consumed for this exact scope.
>
> **Execution class:** all tasks are trust-root `L`, `gpt-5.6-sol`, `ultra`, actual `Release`, plus a
> different trust-domain Sol verifier. No task authorizes package, service, binding, TLS or Hyper-V
> mutation.

**Goal:** Bootstrap a locked, independently validated assurance trust root; make current evidence typed;
and eliminate known PlanOnly, Pester, runner and workflow false-green paths.

**Architecture:** A locked .NET 10 contract tool performs Draft 2020-12 schema validation and RFC 8785
canonical hashing. PowerShell adapters integrate it with existing verification. Negative corpora are
written before validators. Current evidence becomes a typed v2 projection while truthfully staying RED
when raw historical artifacts are unavailable. Required Pester and development runners preserve raw
logs and fail closed. Both workflows run an actual lane and publish evidence even on failure.

**Prerequisite:** Plan 1 exit artifact, authority-integration locator and successor v4 revision are exact
`main` ancestors; the bootstrap Packet decision is valid, unconsumed and exact-main-bound before the
pre-execution consumption gate below.

**Source design:**
`docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`

---

## Pre-execution bootstrap consumption gate

Before T01 changes a tracked file, two independent Sol actors repeat Plan 1's externally pinned RFC 8785
canonicalization and validate the exact Packet/main/spec/v4/scope/expiry. The authenticated Decision
Authority then appends the one-time external consume event for this Plan 2 trust-root bootstrap. The
event must name the exact target main tree and T01 include set. Any mismatch, expiry or prior consume
blocks T01. The request, approval and consume artifacts stay immutable; T08 validates them and emits
external create-only import candidates without consuming them again. Plan 4 alone performs canonical
import into its repository-backed/WORM contract plane.

## Per-task authorization, merge and exit protocol

T01 execution uses A05's unconsumed `successor_execution_handoff`; T01 landing uses its own Program §6.1
`work_id=NHA-T01` request. T02 execution and landing each use a separate Program §6.1
`work_id=NHA-T02` request. After each task's final post-merge exit, T03–T07 start from that fresh exact
`main` and independently generate/approve/consume a new Packet with
`packet_type=trust_root`, `phase=execution_authorization`, the task's complete path-operation manifest,
commands, oracle, capabilities, risk and revert. Each execution request also binds the exact
`assurance-bootstrap://candidate-verification/NHA-TNN/` prefix and A00-pinned or post-T03 repository
publisher finalizer/tool/provider/retention/readback policy as a non-independent output of that consumed
transaction. It contains the observed start commit/tree but no
future result/candidate commit/tree/change-set or PASS claim.

After each T01–T07 candidate commit passes the task's actual Release, negative corpus and independent
Sol review, generate a second Packet with `packet_type=trust_root`,
`phase=landing_authorization`, exact candidate commit/tree/change-set and actual artifacts. Consume it
immediately before the unchanged bootstrap/shadow PR merge; it separately binds the exact final-exit
prefix and finalizer policy. From a fresh checkout of merged `main`,
repeat target/tree/ancestry/current-evidence checks and publish
`assurance-bootstrap://exits/NHA-TNN/<merged-tree>/<payload-digest>` containing both decision/consume
IDs, PR/CI lineage and raw replicas. Candidate-verification artifacts use
`assurance-bootstrap://candidate-verification/NHA-TNN/<candidate-tree>/<payload-digest>` and never count
as the final exit. Any unrelated main advance or candidate amendment stales both decisions.

T08 is the sole artifact-only exception in this plan. It starts only after T07's verified post-merge
exit, creates no candidate/PR and needs no landing Packet, but its create-only external writes still
require a fresh `packet_type=trust_root`, `phase=execution_authorization` Packet. That Packet binds the
exact T07 main commit/tree, read-only input locators, a closed planned-command descriptor digest, two
output URI prefixes,
create-only capability, provider/retention, actor separation, oracle and abort procedure; consume it
immediately before the first authorized measured command. The same Packet separately binds the protected
finalization publisher's tool digest/argv, exact two prefix families and replica providers but does not
place finalizers inside their own measured manifest. T08 may publish only import-candidate and exit
artifacts. Any
need to alter a repository path, provider policy, credential/environment, mutable object, product or
host state cancels this exception and requires a separately revised tracked task and landing flow.

T01/T02 validate/sign both Program §§6.1/6.2 and §7 embedded schemas and publish through the A00-pinned
external writer because repository bootstrap tools do not yet exist. T03 pre-execution uses the exact
T02-merged canonical decision schema plus A00's two external validators; it cannot require its own missing
tool. T03 candidate/post-verification proves its new locked generator, validator, publisher and receipt
checker byte/semantically compatible with those external tools and rejects the frozen negatives. Only
after T03 merges do T04–T07 and artifact-only T08 require both the locked tool and an independent external
verifier; the A00-pinned writer is then read-only historical fallback evidence, never an alternate writer.

## Baseline defects this plan must reproduce as RED

1. `.github/workflows/development-gates.yml` validates Full orchestration with `-PlanOnly`.
2. `.github/workflows/public-boundary.yml` does not pin Pester 5.7.1 and does not use a fail-closed
   `-PassThru` result check.
3. `PlanOnly` returns `ok=true`; runner output is truncated at 8 KiB; there is no timeout/process-tree
   kill; caller-provided scope can influence selection.
4. `git diff --check` is not bound to trusted exact base/head Git objects.
5. `current-evidence.schema.json` is not executed by an authoritative schema engine; the generator
   hardcodes functional and installed PASS prose not represented as typed input.
6. Referenced historical `artifacts/**` are not accessible immutable proof.
7. Trust-root and workflow paths are not unconditionally promoted to L/Sol/Release.
8. GitHub Actions use tags; the repository has no exact SDK file or NuGet lock files.
9. Current private personal repository ruleset API returns HTTP 403, so shadow CI cannot claim landing
   enforcement.

## File map

**Toolchain lock (T01 only)**

- Create `global.json` with SDK `10.0.302`, `rollForward=disable`.
- Create `.node-version` with `24.18.0`.
- Create `docs/superpowers/plans/luna-completion/toolchain-lock.schema.json`.
- Create `docs/superpowers/plans/luna-completion/toolchain-lock.json`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceToolchainLock.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/fixtures/assurance-toolchain/valid/toolchain-lock.json`.
- Create these exact invalid/expected pairs under
  `packaging/windows-desktop-node/tests/fixtures/assurance-toolchain/invalid/`:
  `floating-action.json`/`floating-action.expected.json`,
  `sdk-rollforward.json`/`sdk-rollforward.expected.json`, and
  `unhashed-runtime.json`/`unhashed-runtime.expected.json`.
- Create lock files for all fourteen existing `src/**.csproj` projects.
- Modify both workflows only to provision exact runtimes, pin actions and use locked restore.
- Modify current action-pin assertions in `PcvAdminSmokeEvidenceDocs.Tests.ps1`,
  `PcvDevelopmentGateWorkflow.Tests.ps1` and `PcvCiTriggerContract.Tests.ps1`; T07 later owns only
  false-green/job-DAG semantics in those files.

**Validator implementation (T03 only)**

- Create `tools/assurance/Pcv.Assurance.Contracts/Pcv.Assurance.Contracts.csproj`.
- Create `tools/assurance/Pcv.Assurance.Contracts/Program.cs`, `ContractCommandRouter.cs`,
  `Draft202012Validator.cs`, `Rfc8785Canonicalizer.cs`, `TreeManifestValidator.cs`, and
  `AssuranceError.cs`.
- Create `tools/assurance/Pcv.Assurance.Contracts.Tests/Pcv.Assurance.Contracts.Tests.csproj`,
  `ContractCommandRouterTests.cs`, `Draft202012ValidatorTests.cs`,
  `Rfc8785CanonicalizerTests.cs`, and `TreeManifestValidatorTests.cs`.
- Create lock files for both assurance projects.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceContracts.psm1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceContracts.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceContracts.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceBootstrapExit.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapExit.ps1`.
- Create `packaging/windows-desktop-node/tools/Publish-PcvAssuranceBootstrapArtifact.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapArtifact.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceBootstrapImportCandidateManifest.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapImportCandidateManifest.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssurancePlannedCommandDescriptor.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceCommandManifest.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceRepositoryIdentity.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceRepositoryIdentity.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceTaskDispatch.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceTaskDispatch.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceProbeAttemptPlan.ps1` and
  `Test-PcvAssuranceProbeAttemptPlan.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceReversibleMutationPlan.ps1` and
  `Test-PcvAssuranceReversibleMutationPlan.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceAuthorityReservation.ps1`,
  `Test-PcvAssuranceAuthorityReservation.ps1`, `Invoke-PcvAssuranceConsumeAndClaim.ps1`, and
  `Test-PcvAssurancePairState.ps1` as provider-neutral Decision-Plane contracts and client tools. T03 does
  not create a competing durable state engine, GitHub adapter or host mutation adapter. Plan 4 E05 owns the
  canonical append-only/atomic Decision-Plane implementation, Plan 5 L06 supplies only the GitHub provider
  CAS/readback adapter behind it, and Plan 6 P07 supplies only host root/surface and category-runner adapters.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceTaskVerification.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceBootstrapExit.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceBootstrapArtifact.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceBootstrapImportCandidateManifest.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssurancePlannedCommandDescriptor.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceBootstrapCommandRunner.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceCommandManifest.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceRepositoryIdentity.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceTaskDispatch.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssurancePairPlans.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceAuthorityReservation.Tests.ps1` and
  `PcvAssurancePairState.Tests.ps1`.

**Contracts and corpora**

- Create all schema files in
  `docs/superpowers/plans/luna-completion/contracts/` listed by the source design §8.1.
- Create `docs/superpowers/plans/luna-completion/contracts/bootstrap-exit.schema.json` byte-equivalent
  to the Program §7 embedded schema payload.
- Create `docs/superpowers/plans/luna-completion/contracts/bootstrap-decision-request.schema.json` byte-
  equivalent to the Program §6.1 embedded schema payload with expected SHA-256
  `b719c93bebef0fe5028e551069304b0a12d7d894c41c93be89df52c60ed47a0e`.
- Create
  `docs/superpowers/plans/luna-completion/contracts/bootstrap-artifact-publication-receipt.schema.json`
  byte-equivalent to Program §6.2 with expected SHA-256
  `a4f7e6bf835c0b91f1bf2e642fb037d2e6b912df29900ffe84d04ae140319ea0`.
- Create
  `docs/superpowers/plans/luna-completion/contracts/bootstrap-import-candidate-manifest.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/planned-command-descriptor.schema.json` as the
  repository semantic successor to A00's externally pinned descriptor contract; do not claim byte identity.
- Create `docs/superpowers/plans/luna-completion/contracts/bootstrap-command-run.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/bootstrap-command-manifest.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/repository-identity.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/task-dispatch.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/canary-probe-attempt-plan.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/reversible-mutation-plan.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/authority-reservation-receipt.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/rollback-capability-horizon.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/pair-state-transition.schema.json`.
- Create `docs/superpowers/plans/luna-completion/protected-paths.schema.json`.
- Create `docs/superpowers/plans/luna-completion/protected-paths.json`.
- Create exact valid/invalid fixtures under
  `packaging/windows-desktop-node/tests/fixtures/assurance-contracts/`.
- Create `packaging/windows-desktop-node/tests/fixtures/assurance-bootstrap/valid/exit.json` and exact
  invalid/expected pairs `same-domain-signatures`, `tampered-raw-log`, `wrong-target-tree`,
  `expired-envelope`, and `replica-collision` under `assurance-bootstrap/invalid/`.

**Current evidence**

- Modify `docs/ga-ready/current-evidence.schema.json` and `docs/ga-ready/current-evidence.json`.
- Create four exact `*.evidence.json` sidecars for the 0.42.65 package/fullgate/functional/installed
  Markdown records.
- Modify `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1`.
- Modify `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`.
- Regenerate only the bounded blocks in these six exact files: `AGENTS.md`,
  `docs/ga-ready/EVIDENCE_INDEX.md`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`,
  `docs/ga-ready/CONTROL_PLANE_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, and
  `packaging/windows-desktop-node/README.md`.

**Fail-closed execution and CI**

- Create `packaging/windows-desktop-node/tools/Invoke-PcvRequiredPester.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvRequiredPester.Tests.ps1`.
- Create five non-discoverable Pester fixtures under
  `packaging/windows-desktop-node/tests/fixtures/pester-canaries/`.
- Modify `PcvDevelopmentVerification.psm1`, `PcvDevelopmentVerificationRunner.psm1`,
  `Invoke-PcvDevelopmentVerification.ps1` and their existing tests.
- The exact existing focused tests are `PcvDevelopmentVerification.Tests.ps1` and
  `PcvDevelopmentVerificationExecution.Tests.ps1`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-target.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-target-issuer.schema.json`.
- Create `docs/superpowers/plans/luna-completion/verification-target-issuer.json` after the issuer
  identity is approved.
- Create `packaging/windows-desktop-node/tools/New-PcvVerificationTargetManifest.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvVerificationTargetManifest.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceTargetWorkflow.Tests.ps1`.
- Modify `.github/workflows/development-gates.yml` and `.github/workflows/public-boundary.yml`.
- Create `.github/workflows/assurance-target.yml` as a pinned protected-main reusable issuer workflow.
- Modify `PcvDevelopmentGateWorkflow.Tests.ps1`, `PcvCiTriggerContract.Tests.ps1`, and only the current
  workflow assertions in `PcvAdminSmokeEvidenceDocs.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvPublicBoundaryWorkflow.Tests.ps1`.

## Normative task-dispatch matrix

Each row invokes the Program §5.1 canonical Test, Red and Final argv with the row ID as exact authorized
target `-ExpectedWorkId`. For T01 execution only, the owning handoff request remains NHA-A05 while
`target_work_id`, dispatch/range/descriptor work IDs and consume consumer are all NHA-T01; all other
branches require owner=target=row. Before consume, the signed dispatch expands the named File-map block and every listed
fixture into exact create/modify/delete operations; no directory alias or wildcard remains. Final phase
is actual Release plus a different-trust-domain Sol verifier. Detailed case IDs and finalizer prefixes
come from the owning task section and are copied byte-for-byte into the dispatch.

| Work ID | Ordered path/range closure | RED contract | Implementation/finalizer boundary | Rollback | Commit/PR boundary |
|---|---|---|---|---|---|
| NHA-T01 | `exact_paths`, Toolchain/file-map list | floating SDK/action/package and unlocked restore reject | only exact locks, pins, workflow supply-chain edits and raw task finalizers | whole-commit revert; remain RED if input unavailable | `tracked_pr` |
| NHA-T02 | `exact_paths`, Contracts/corpora list | valid/invalid corpus runs before validators | schemas and exact fixtures only; no validator code | whole-commit revert | `tracked_pr` |
| NHA-T03 | `exact_paths`, Validator/file-map list | missing CLI/classification plus external-disagreement RED | validator, repository-identity/task-dispatch/bootstrap tools and protected map only | whole-commit revert | `tracked_pr` |
| NHA-T04 | `exact_paths`, Current-evidence list | stale/contradictory/missing typed evidence rejects | sidecars, generator/schema and bounded projections only | whole-commit revert | `tracked_pr` |
| NHA-T05 | `exact_paths`, Required-Pester list | assertion/BeforeAll/parse/zero-test/PlanOnly canaries RED | adapter, exact fixtures and tests only | whole-commit revert | `tracked_pr` |
| NHA-T06 | ordered parent `exact_paths` tracked range, then deferred issuer-selection resolver: existing branch has no create child, create branch emits one exact `provider_administration` child; parent resumes only singleton issuer-record op with the same consume | issuer/OIDC/future-digest/final-selection negatives RED | reviewed anchor blobs plus sole issuer-record op; setup child has `operations=[]` | conditional provider rollback; whole tracked revert | `tracked_pr`; setup child `artifact_only_no_commit` |
| NHA-T07 | `exact_paths`, exactly `.github/workflows/development-gates.yml`, `.github/workflows/public-boundary.yml`, `PcvDevelopmentGateWorkflow.Tests.ps1`, `PcvCiTriggerContract.Tests.ps1`, `PcvAdminSmokeEvidenceDocs.Tests.ps1`, `PcvPublicBoundaryWorkflow.Tests.ps1`; T06-owned `assurance-target.yml` is read-only | stale PlanOnly/no-upload/DAG/trigger assertions RED | actual fail-closed workflow semantics only | whole-commit revert; post-activation fix forward | `tracked_pr` |
| NHA-T08 | `artifact_only`, exact two-clean-run/import-candidate/exit prefixes | missing source, branch history, raw proof or invariant mismatch rejects | protected reproduction/import-candidate/final-exit chain only | invalidate publication; preserve failure | `artifact_only_no_commit` |

## Task NHA-T01: Lock the toolchain and action supply chain

**Files:** `global.json`, `.node-version`, toolchain lock schema/record, exact toolchain fixtures/test,
both workflows, the three current action-pin assertion tests named in the file map, and these lock files
only:

```text
src/DesktopNode.Api/packages.lock.json
src/DesktopNode.Api.Tests/packages.lock.json
src/DesktopNode.Cli/packages.lock.json
src/DesktopNode.Cli.Tests/packages.lock.json
src/DesktopNode.Contracts/packages.lock.json
src/DesktopNode.Contracts.Tests/packages.lock.json
src/DesktopNode.Host/packages.lock.json
src/DesktopNode.Host.Tests/packages.lock.json
src/DesktopNode.HyperV/packages.lock.json
src/DesktopNode.HyperV.Tests/packages.lock.json
src/DesktopNode.Runtime/packages.lock.json
src/DesktopNode.Runtime.Tests/packages.lock.json
src/DesktopNode.Service/packages.lock.json
src/DesktopNode.Service.Tests/packages.lock.json
```

Before Step 1, validate the A05-owned handoff mapping exactly: request owner NHA-A05 and target/dispatch/
range/external planned-descriptor/consumer NHA-T01. Revalidate and consume that decision once, then its
  first prebound finalizer conditionally publishes/readbacks the signed T01 root dispatch, selected range and
  descriptor with three separate §6.2 receipts. Only after all three validate may RED or a tracked write run. T01's later
landing uses a fresh NHA-T01 landing request and cannot reuse the handoff.

- [ ] **Step 1: Add RED toolchain assertions**

Tests reject `10.0.x`, Node major-only values, an action tag, missing lock files, restore without
`--locked-mode`, or a package outside its lock. Before the repository validator exists, A05 revalidates,
uses and binds the two external Draft 2020-12 validator/canonicalizer binaries, SHA-256, public provenance
and exact argv already pinned by A00; it cannot substitute or refreeze them. Both must accept the valid fixture, reject each invalid fixture at its expected schema
location and produce identical canonical bytes/digest. Expected RED: current workflows and repository
fail.

- [ ] **Step 2: Pin exact tools and packages**

- .NET SDK: `10.0.302`; Pester: `5.7.1`; Node: `24.18.0`.
- Windows PowerShell archive: `PowerShell-7.6.4-win-x64.zip`, SHA-256
  `80832551c52809301e6071c8bac977beb5a2f1ec953eb4db9f94deb953333793`.
- Linux PowerShell archive: `powershell-7.6.4-linux-x64.tar.gz`, SHA-256
  `4471b5a36bfe86ec7af8525d36bb1cacba0128e7aac22d05cc064bc00e604721`.
- T03 contract package decisions are frozen now in `toolchain-lock.json` but their project/lock files are
  created only in T03: `JsonSchema.Net` `9.4.0`, `jsoncanonicalizer` `1.0.0`,
  `Microsoft.NET.Test.Sdk` `17.14.1`, `xunit` `2.9.3`, `xunit.runner.visualstudio` `3.1.4`, and
  `coverlet.collector` `6.0.4`.
- Generate the fourteen existing product-project NuGet locks in T01 with locked direct and transitive
  versions; their subsequent restores use `dotnet restore --locked-mode`. The two not-yet-existing
  assurance-project locks follow T03's separately specified deterministic first-generation procedure.

The implementer must archive package metadata, license and package SHA-512 from the lock. Package
selection is frozen by this plan; a newer version requires a trust-root revision, not opportunistic
upgrade.

- [ ] **Step 3: Pin GitHub Actions to immutable commits**

Use exact SHAs with version comments:

```text
actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd        # v6.0.2
actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d   # v5.0.0
actions/cache@a7833574556fa59680c1b7cb190c1735db73ebf0          # v5.0.0
actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903     # v6.0.0
actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f # v6.0.0
```

Each workflow downloads the approved PowerShell archive from the official release URL, verifies the
exact SHA-256 before extraction, and invokes that extracted absolute `pwsh` path. Runner-preinstalled
rolling PowerShell is not a trust input. Save Pester 5.7.1 to an exact cache directory, record its module
manifest/package digest in `toolchain-lock.json`, and import it with `-RequiredVersion 5.7.1`.

Do not change workflow semantics yet beyond action/runtime pinning and locked restore; T07 owns
false-green rewiring.

- [ ] **Step 4: Verify and commit T01**

Run locked restores for the fourteen product projects, npm ci/test/parity, toolchain fixture validation,
all three updated workflow assertion tests and `git diff --check`. Independent verifier checks every
action SHA against the expected upstream tag and records the lookup result. Commit
`build: lock assurance toolchain inputs`.

Rollback is a whole-commit revert before activation. If an exact tool becomes unavailable, assurance
remains RED; do not loosen versions.

## Task NHA-T02: Define canonical schemas and known-bad fixtures

**Files:**

- Create the twelve source-design contract schemas:
  `spec-lock`, `requirements`, `card-blueprints`, `traceability`, `acceptance`, `execution-manifest`,
  `verification-result`, `review-attestation`, `landing-equivalence-attestation`, `decision-packet`,
  `decision-record`, and `trust-dashboard`.
- Create the additional Program-owned `bootstrap-exit.schema.json` by extracting and byte-comparing the
  exact Program §7 Git-blob payload with its stated delimiter algorithm and expected SHA-256
  `c398ac5f2d13df77a579697f052df259e12aed147212b817b16f0e6c081de115`; semantic additions,
  omissions, CRLF conversion, BOM and terminal LF are forbidden.
- Create the Program-owned `bootstrap-decision-request.schema.json` by the same unique-marker Git-blob
  extraction and byte-comparison algorithm using the §6.1 marker and expected SHA-256
  `b719c93bebef0fe5028e551069304b0a12d7d894c41c93be89df52c60ed47a0e`; it is the closed historical A02-through-T02 request
  contract, not a permissive substitute for the canonical decision schema.
- Create the Program-owned `bootstrap-artifact-publication-receipt.schema.json` by byte-extracting §6.2
  with expected SHA-256 `a4f7e6bf835c0b91f1bf2e642fb037d2e6b912df29900ffe84d04ae140319ea0`.
  It closes provider/object/version, Packet/prefix/source binding, conditional create, remote checksum/
  size/retention and distinct readback principal/time for every pre-T03 publication.
- Create `bootstrap-import-candidate-manifest.schema.json` as the closed deterministic inventory contract
  reused by T08, X09 and E05. It contains exact project lineage and authorization, expected-inventory
  descriptor digest, and a nonempty sorted one-to-one list of source event/exit IDs, original immutable
  URI/hash/size/schema/signature/actor/target/consume-cardinality facts and planned candidate URI. It has
  no publication receipt, canonical-import status, caller verdict or terminal owning exit.
- Create `bootstrap-command-manifest.schema.json` as the closed measured input contract for the Program
  schema's ordered commands and supporting source-manifest references.
- Create `planned-command-descriptor.schema.json` as a distinct pre-execution contract containing exact
  ordered argv arrays, executable digests, cwd, bounded non-secret environment, input digests, declared
  output URI prefixes, timeout, capability and actor role, but no start/end/exit/raw-output/result/PASS
  fields. It also requires `work_id`, `task_dispatch_uri`, `task_dispatch_sha256`,
  `task_dispatch_range_ids`, `authorized_range_id`, `authorized_range_sha256`, range-record URI/digest and
  boundary mode. Before a request exists, the generator opens only the signed dispatch/range and requires
  authorized range → descriptor equality for work/range, operations, command IDs/argv, capabilities,
  actor/lane, output prefixes and boundary. The later request binds both descriptor and dispatch/range
  digests; only the post-consume runner/finalizers prove Packet → authorized range → descriptor equality.
  Each input is a closed mutually exclusive branch: `frozen_input` requires an already-existing
  path/locator plus digest, while `prior_output_slot` requires only an earlier producer command ID,
  schema/role, exact predeclared path or URI prefix and slot name and forbids a future digest/size. The
  producer's measured run manifest supplies and binds the later digest/size before the consumer may run.
  The third `authority_output_slot` branch is permitted only for an L08/P08/P09 rollback, reservation,
  setup-forward or conditional-abort descriptor and requires exactly one lineage: signed `pair_plan`, or L08-only immutable
  `canary_setup` root-dispatch/setup-range/template digest set. It also requires stable slot, expected
  authority/receipt schema and role, exact producer work/range/subbranch and create-only prefix while
  forbidding future digest/size. `slot_kind` is closed: `future_reservation_receipt` is rollback-only,
  unresolved at approval and resolves from exactly one consumed reserve chain at rollback;
  `approved_unconsumed_rollback_decision` is reservation-only and requires APPROVE, consume count zero and
  active/unexpired rollback child/request; `consumed_reservation_receipt` is abort/release-only and opens the
  exact reserve request/decision/consume/one receipt;
  `consumed_reservation_receipt_for_setup_forward` is L08 setup-forward-only and opens the exact consumed
  setup-reservation chain before request and consume; `verified_terminal_outcome` is abort/release-only and opens
  pair-scoped zero-count/reject/authorized-landing proof. A rollback request is the sole branch allowed to
  remain unresolved at approval; it binds the stable future reservation slot and resolves it only at rollback
  consume. Reservation, setup-forward and conditional-abort requests dereference every required predecessor
  slot into exact request facts and revalidate them at consume; unresolved setup-forward is non-executable.
  Every resolution opens the matching immutable ledger/
  publication chain and requires exactly one measured value. A consumed
  execution Packet may bind only this descriptor, never the measured manifest.
  The T02 corpus is shared with the A00-pinned external descriptor schema/generator/two validators and
  proves canonical payload/digest plus valid/wrong-range/future-result/argv-drift behavior agrees. T03's
  own authorization uses the external descriptor; the repository implementation is comparison-only until
  its unchanged candidate lands.
- Create `bootstrap-command-run.schema.json` as the closed one-command measured record and keep it
  distinct from the aggregate command manifest.
- Create `repository-identity.schema.json` as a closed signed envelope. Its payload requires
  `kind=project|typed_control_fixture`, immutable provider/repository IDs, descriptor SHA-256, exact start
  commit/tree, issued/expiry UTC, signer role/key ID, signature/attestation locator and one mutually
  exclusive source: `project` requires independently signed provider/server repository readback;
  `typed_control_fixture` requires a signed test-controller allocation record. It accepts no caller path
  as identity and no unsigned/free-form tuple.
- Create `canary-probe-attempt-plan.schema.json` as a closed signed payload with one immutable probe-pair
  ID, exact candidate/ref/PR/queue identity and pre-probe ETags, ordinary actor class and credential-policy
  digest, attempt operation/argv/nonce/expected transition, inverse/cancel/restore argv and readback oracle,
  stable rollback output slot, expected `probe_attempt_resolution` kind/schema and stable resolver slot ID,
  and the already-existing `probe_pair_plan_resolution` initial-parent URI/digest. It contains no rollback
  child, first-resolution bundle, stage-two parent, Packet, decision, reservation, receipt, provider result
  or verdict value. The T03 pair-plan tools generate its
  canonical unsigned signing request, verify the returned Dispatch-Authority envelope and never hold a
  signing key.
- Create `reversible-mutation-plan.schema.json` as a closed signed payload with one immutable mutation-pair
  ID, exact artifact/host/surface/before-state, one forward category and argv/capability/oracle, its exact
  inverse lifecycle category and argv/capability/oracle, stable reservation output slot, expected
  `reversible_mutation_forward_resolution` kind/schema and stable resolver slot ID, and the already-existing
  `reversible_mutation_pair_plan_resolution` initial-parent URI/digest. It contains no rollback child, first-
  resolution bundle, stage-two parent, Packet, decision, reservation, receipt, measured result or verdict
  value. The same T03 separation applies: the local pair-plan tools
  create only a canonical signing request and validate the externally signed envelope.
- Create `authority-reservation-receipt.schema.json` as the immutable result of a consumed reservation range:
  exact lineage kind/digest, work/pair/setup ordinal, stable slot, owner/fencing token/epoch, state key,
  acquisition mode, root and owned guard set, rollback decision, capability-horizon digest, CAS/readback/
  WORM locators and terminal deadline. It contains no caller verdict and cannot grant forward authority.
- Create `rollback-capability-horizon.schema.json` with exact decision, credential/lease, inverse artifact/
  checkpoint, sealed attachment, signer/key and monitor expiry/retention values, bounded forward/queue,
  readback, restore/cancel and safety durations, computed minimum and terminal deadline. Any unavailable,
  unverified or shorter component rejects.
- Create `pair-state-transition.schema.json` for the Program's closed serializable `consume_and_claim` and
  outcome transition table. It binds the same pair key/owner/fencing/epoch, from/to states, decision/consume,
  claim intent/receipt, actor, WORM/readback and exactly-one-winner oracle. It has no free-form state or
  non-atomic success mode; a partial distributed operation is only `uncertain/reconciliation_required`.
- Create `task-dispatch.schema.json` as the closed canonical Program §5.1 contract: one ordered nonempty
  range manifest with `exact_paths` (including `candidate_commit_no_merge` boundary), artifact-only,
  provider-administration, mutation-authorization, all eight closed deferred-resolver kinds,
  verified-candidate-landing, and work-ID-locked A00 approval-empty-commit/A01 approved-candidate-landing
  shapes. `exact_paths` requires exactly one authority mapping:
  `trust_root/tracked_change` or `mutation_authorization/brokered_code_change`; the latter additionally
  requires broker lease/revert and false provider/host mutation flags. The mutation-authorization range
  shape requires `mutation_authorization/host_or_artifact_mutation`, `operations=[]` and one closed
  category. The fifth and sixth initial resolvers are `probe_pair_plan_resolution` (NHA-L08-only) and
  `reversible_mutation_pair_plan_resolution` (NHA-P08/P09-only). Each immutable task-manifest parent fixes
  work/pair ordinal and kind/category, typed actual-input/plan producer slots, plan schema and protected
  generator/resolver digests, stable rollback/reservation slots and exact four-output arity/order/boundaries.
  After the actual signed plan binds that initial parent, first resolution emits exactly the rollback child,
  reservation template, matching stage-two parent and conditional abort/unused-release template, plus one
  envelope binding all four. The seventh `probe_attempt_resolution` and eighth
  `reversible_mutation_forward_resolution` stage-two parents bind plan/rollback child and empty rollback-
  approval/reservation-receipt slots; only the separately consumed reservation receipt resolves either once
  to exactly one executable attempt or mutation-forward child. Each initial pair resolver emits the exact
  four-output bundle with exactly one deferred stage-two member; each stage-two resolver emits exactly one
  executable child and no deferred child. Other resolver kinds retain their own closed arity. Maximum pair
  nesting depth is two and every output slot is one-shot/single-assignment. It requires exact RED/final argv,
  actor/lane, rollback, per-range boundary and required decision cardinality, plus signed-envelope payload
  digest, Dispatch Authority signer/trust-domain/key/algorithm/signature/attestation/issued/expiry/nonce/
  revocation fields. It forbids wildcard, future result, range-decision reuse, caller command and
  implementation-actor signing. This repository schema is semantic successor to the A00-pinned external
  dispatch schema; T03 must prove the shared fixture corpus and canonical payload/digest behavior agree,
  not falsely claim byte identity to a schema that is not embedded here. A03 is one exact ordered
  three-range manifest—tracked execution, zero-operation `spec_revision_approval`, then verified landing—
  with three distinct decision consumers over the same candidate and exact cardinality one each.
- Create fixture files listed below.

- [ ] **Step 1: Add exact fixture corpus before validators**

Valid fixtures:

```text
valid/envelope.json
valid/requirements.json
valid/acceptance.json
valid/card-ready.json
valid/traceability-planned.json
valid/decision-packet.json
valid/decision-events.json
valid/dashboard-red.json
valid/bootstrap-exit.json
valid/bootstrap-decision-request-execution.json
valid/bootstrap-decision-request-landing.json
valid/bootstrap-artifact-publication-receipt.json
valid/bootstrap-import-candidate-manifest.json
valid/planned-command-descriptor.json
valid/planned-command-descriptor-authority-output-slot-l08.json
valid/planned-command-descriptor-authority-output-slot-p08.json
valid/bootstrap-command-run.json
valid/bootstrap-command-manifest.json
valid/requirements-purpose-verification-target-issuer-existing.json
valid/requirements-purpose-verification-target-issuer-create-intent.json
valid/requirements-purpose-verification-target-issuer-post-create.json
valid/requirements-purpose-dependency-existing.json
valid/requirements-purpose-dependency-create-intent.json
valid/requirements-purpose-dependency-post-create.json
valid/requirements-purpose-artifact-store-notary-selection.json
valid/requirements-purpose-interim-channel-continuation.json
valid/requirements-purpose-landing-provider-mode-owner-selection.json
valid/requirements-purpose-tls-not-applicable.json
valid/requirements-purpose-packet-only-exercise.json
valid/requirements-purpose-pilot-selection-landing.json
valid/trust-root-tracked-project.json
valid/trust-root-tracked-typed-control-fixture.json
valid/trust-root-provider-admin-canary-enforcement-probe.json
valid/trust-root-provider-admin-canary-probe-rollback.json
valid/trust-root-provider-admin-canary-probe-reservation.json
valid/trust-root-provider-admin-canary-probe-unused-release.json
valid/trust-root-provider-admin-canary-setup-reservation.json
valid/trust-root-provider-admin-canary-setup-forward.json
valid/trust-root-provider-admin-canary-setup-rollback.json
valid/trust-root-provider-admin-canary-setup-unused-release.json
valid/canary-probe-attempt-plan.json
valid/trust-root-provider-admin-mutation-guard-close.json
valid/trust-root-provider-admin-mutation-guard-reserve.json
valid/trust-root-provider-admin-mutation-guard-abort-release.json
valid/mutation-build-no-host-no-rollback-reservation.json
valid/mutation-hyperv-forward-with-unconsumed-rollback-reservation.json
valid/mutation-lifecycle-rollback-hyperv.json
valid/reversible-mutation-plan.json
valid/authority-reservation-receipt.json
valid/rollback-capability-horizon.json
valid/pair-state-transition-forward-wins.json
valid/pair-state-transition-release-wins.json
valid/decision-events-terminal-cancellation-authorization.json
valid/decision-events-terminal-cancellation-template.json
valid/decision-events-terminal-cancellation-resolved-lineage.json
valid/repository-identity-project.json
valid/repository-identity-typed-control-fixture.json
valid/task-dispatch-exact-paths-trust-root-tracked.json
valid/task-dispatch-exact-paths-brokered-code-change.json
valid/task-dispatch-artifact-only.json
valid/task-dispatch-provider-administration.json
valid/task-dispatch-mutation-authorization-host-or-artifact.json
valid/task-dispatch-deferred-not-ready.json
valid/task-dispatch-candidate-landing-deferred.json
valid/task-dispatch-program-approval-deferred-parent-a00.json
valid/task-dispatch-program-approved-landing-deferred-parent-a01.json
valid/task-dispatch-approval-empty-commit-a00.json
valid/task-dispatch-approved-candidate-landing-a01.json
valid/task-dispatch-verified-candidate-landing.json
valid/task-dispatch-multi-range-x09.json
valid/task-dispatch-a03-three-decision.json
valid/task-dispatch-prelanding-consume-a03-spec-revision.json
valid/task-dispatch-prelanding-consume-p03-requirements.json
valid/task-dispatch-prelanding-consume-p03-spec-revision.json
valid/task-dispatch-probe-attempt-deferred-l08.json
valid/task-dispatch-probe-attempt-resolved-l08.json
valid/task-dispatch-probe-pair-first-resolution-l08.json
valid/task-dispatch-reversible-mutation-deferred-p08.json
valid/task-dispatch-reversible-mutation-resolved-p08.json
valid/task-dispatch-reversible-pair-first-resolution-p08.json
valid/task-dispatch-two-root-range-publication.json
```

Schema-invalid fixtures, each with one expected schema location in a sidecar `.expected.json`:

```text
schema-invalid/digest-field-shape.json
schema-invalid/additional-property.json
schema-invalid/unknown-status.json
schema-invalid/ready-future-artifact-ref.json
schema-invalid/unknown-failure-code.json
schema-invalid/zero-or-empty-hash.json
schema-invalid/bootstrap-exit-additional-property.json
schema-invalid/bootstrap-decision-request-branch-mismatch.json
schema-invalid/bootstrap-decision-request-future-result.json
schema-invalid/bootstrap-decision-request-wrong-target.json
schema-invalid/bootstrap-decision-request-empty-operations.json
schema-invalid/bootstrap-decision-request-empty-output-prefixes.json
schema-invalid/bootstrap-decision-request-path-traversal.json
schema-invalid/bootstrap-decision-request-path-backslash.json
schema-invalid/bootstrap-decision-request-path-control.json
schema-invalid/bootstrap-decision-request-missing-planned-descriptor.json
schema-invalid/bootstrap-artifact-publication-receipt-additional-property.json
schema-invalid/bootstrap-import-candidate-manifest-additional-property.json
schema-invalid/bootstrap-import-candidate-manifest-empty-inventory.json
schema-invalid/planned-command-descriptor-additional-property.json
schema-invalid/planned-command-descriptor-result-field.json
schema-invalid/planned-command-descriptor-future-slot-digest.json
schema-invalid/planned-command-descriptor-authority-slot-wrong-role.json
schema-invalid/planned-command-descriptor-authority-slot-wrong-work.json
schema-invalid/planned-command-descriptor-authority-slot-future-size.json
schema-invalid/planned-command-descriptor-setup-forward-unresolved-receipt.json
schema-invalid/bootstrap-command-run-additional-property.json
schema-invalid/bootstrap-command-manifest-additional-property.json
schema-invalid/requirements-purpose-unknown.json
schema-invalid/requirements-purpose-wrong-phase.json
schema-invalid/requirements-purpose-forbidden-side-effect.json
schema-invalid/requirements-purpose-wrong-consumer.json
schema-invalid/requirements-purpose-selection-stage-unknown.json
schema-invalid/requirements-purpose-selection-stage-mixed.json
schema-invalid/requirements-purpose-create-intent-assigned-id.json
schema-invalid/requirements-purpose-post-create-missing-receipt.json
schema-invalid/requirements-purpose-selection-subject-unknown.json
schema-invalid/trust-root-execution-scope-mixed.json
schema-invalid/trust-root-artifact-with-operation.json
schema-invalid/trust-root-provider-admin-with-tracked-path.json
schema-invalid/trust-root-tracked-missing-repository-identity.json
schema-invalid/trust-root-provider-canary-probe-main-target.json
schema-invalid/trust-root-provider-canary-probe-candidate-create.json
schema-invalid/trust-root-provider-canary-probe-bypass-credential.json
schema-invalid/trust-root-provider-canary-probe-success-outcome.json
schema-invalid/trust-root-provider-canary-probe-missing-paired-rollback.json
schema-invalid/trust-root-provider-canary-probe-rollback-missing-probe-plan.json
schema-invalid/trust-root-provider-canary-probe-rollback-with-forward-attempt.json
schema-invalid/trust-root-provider-canary-probe-reservation-unconsumed-authority.json
schema-invalid/trust-root-provider-canary-probe-unused-release-transitioned.json
schema-invalid/trust-root-provider-canary-probe-unused-release-missing-terminal-cancellation.json
schema-invalid/trust-root-provider-canary-probe-insufficient-rollback-horizon.json
schema-invalid/trust-root-provider-canary-setup-forward-missing-reservation.json
schema-invalid/trust-root-provider-canary-setup-rollback-missing-future-slot.json
schema-invalid/trust-root-provider-canary-setup-unused-release-wrong-state.json
schema-invalid/canary-probe-attempt-plan-future-rollback-field.json
schema-invalid/canary-probe-attempt-plan-future-decision-field.json
schema-invalid/canary-probe-attempt-plan-future-receipt-field.json
schema-invalid/canary-probe-attempt-plan-missing-inverse.json
schema-invalid/canary-probe-attempt-plan-future-stage-two-parent.json
schema-invalid/task-dispatch-probe-attempt-future-rollback-reference.json
schema-invalid/task-dispatch-probe-attempt-wrong-work-id.json
schema-invalid/trust-root-provider-guard-close-wrong-work-id.json
schema-invalid/trust-root-provider-guard-close-missing-rollback-receipt.json
schema-invalid/trust-root-provider-guard-close-host-operation.json
schema-invalid/trust-root-provider-guard-reserve-unconsumed-authority.json
schema-invalid/trust-root-provider-guard-reserve-missing-stable-slot.json
schema-invalid/trust-root-provider-guard-abort-release-forward-consumed.json
schema-invalid/trust-root-provider-guard-abort-release-host-mutated.json
schema-invalid/trust-root-provider-guard-abort-release-missing-terminal-cancellation.json
schema-invalid/trust-root-provider-guard-abort-release-root-overrelease.json
schema-invalid/trust-root-provider-guard-reserve-insufficient-rollback-horizon.json
schema-invalid/mutation-build-with-rollback-reservation.json
schema-invalid/mutation-hyperv-forward-missing-rollback-reservation.json
schema-invalid/mutation-lifecycle-rollback-nested-reservation.json
schema-invalid/mutation-lifecycle-rollback-unknown-kind.json
schema-invalid/reversible-mutation-plan-future-packet-id.json
schema-invalid/reversible-mutation-plan-missing-inverse.json
schema-invalid/reversible-mutation-plan-future-stage-two-parent.json
schema-invalid/authority-reservation-receipt-missing-fencing-epoch.json
schema-invalid/rollback-capability-horizon-short-component.json
schema-invalid/pair-state-transition-free-form-state.json
schema-invalid/pair-state-transition-nonatomic-success.json
schema-invalid/decision-terminal-cancellation-missing-owner-consume.json
schema-invalid/decision-terminal-cancellation-template-with-approval.json
schema-invalid/repository-identity-unsigned.json
schema-invalid/task-dispatch-unresolved-alias.json
schema-invalid/task-dispatch-free-form-command.json
schema-invalid/task-dispatch-missing-signature.json
schema-invalid/task-dispatch-brokered-code-change-mutation-range.json
schema-invalid/task-dispatch-host-or-artifact-exact-paths.json
schema-invalid/task-dispatch-exact-paths-mixed-authority-mapping.json
schema-invalid/task-dispatch-prelanding-descriptor-present.json
schema-invalid/task-dispatch-prelanding-wrong-work-id.json
schema-invalid/task-dispatch-reversible-mutation-future-receipt.json
schema-invalid/task-dispatch-reversible-mutation-wrong-work-id.json
schema-invalid/task-dispatch-pair-first-resolution-wrong-arity.json
schema-invalid/task-dispatch-pair-first-resolution-wrong-order.json
schema-invalid/task-dispatch-pair-depth-three.json
schema-invalid/task-dispatch-pair-template-caller-backfill.json
schema-invalid/task-dispatch-root-recreation.json
schema-invalid/external-output-missing-finalizer.json
```

Semantic-invalid fixtures are intentionally schema-valid and carry the stable T03 semantic error code:

```text
semantic-invalid/digest-mismatch.json
semantic-invalid/orphan-traceability.json
semantic-invalid/planned-as-pass.json
semantic-invalid/same-actor.json
semantic-invalid/decision-replay.json
semantic-invalid/planned-command-descriptor-argv-drift.json
semantic-invalid/planned-command-descriptor-slot-producer-order.json
semantic-invalid/planned-command-descriptor-authority-slot-cross-pair.json
semantic-invalid/planned-command-descriptor-authority-slot-producer-substitution.json
semantic-invalid/planned-command-descriptor-authority-slot-zero-receipt.json
semantic-invalid/planned-command-descriptor-authority-slot-multiple-receipts.json
semantic-invalid/planned-command-descriptor-authority-slot-cross-setup.json
semantic-invalid/bootstrap-command-run-raw-hash-drift.json
semantic-invalid/bootstrap-command-manifest-source-reorder.json
semantic-invalid/bootstrap-command-manifest-source-lineage.json
semantic-invalid/bootstrap-decision-request-digest-mismatch.json
semantic-invalid/bootstrap-decision-request-path-non-nfc.json
semantic-invalid/bootstrap-decision-request-path-casefold-duplicate.json
semantic-invalid/bootstrap-decision-request-path-reserved-device.json
semantic-invalid/bootstrap-decision-request-path-trailing-dot-space.json
semantic-invalid/bootstrap-import-candidate-manifest-duplicate-source.json
semantic-invalid/bootstrap-import-candidate-manifest-source-lineage.json
semantic-invalid/bootstrap-exit-replica-provider-collision.json
semantic-invalid/bootstrap-exit-lineage-mismatch.json
semantic-invalid/requirements-purpose-consume-cardinality.json
semantic-invalid/requirements-purpose-cross-consumer-replay.json
semantic-invalid/requirements-purpose-post-create-without-intent.json
semantic-invalid/requirements-purpose-post-create-backfill.json
semantic-invalid/requirements-purpose-selection-stage-repeated.json
semantic-invalid/requirements-purpose-selection-subject-replay.json
semantic-invalid/requirements-purpose-post-create-subject-mismatch.json
semantic-invalid/trust-root-tracked-wrong-repository-id.json
semantic-invalid/trust-root-tracked-allowance-repository-mismatch.json
semantic-invalid/trust-root-tracked-cross-kind-substitution.json
semantic-invalid/repository-identity-forged-signature.json
semantic-invalid/repository-identity-stale.json
semantic-invalid/repository-identity-cross-repository.json
semantic-invalid/task-dispatch-packet-mismatch.json
semantic-invalid/task-dispatch-deferred-ready.json
semantic-invalid/task-dispatch-wrong-special-work-id.json
semantic-invalid/task-dispatch-future-candidate.json
semantic-invalid/task-dispatch-reused-decision.json
semantic-invalid/task-dispatch-cross-range-authority.json
semantic-invalid/task-dispatch-range-reorder.json
semantic-invalid/task-dispatch-implementation-self-signed.json
semantic-invalid/task-dispatch-revoked-signer.json
semantic-invalid/task-dispatch-a03-missing-spec-revision.json
semantic-invalid/task-dispatch-final-landing-prior-consume-missing.json
semantic-invalid/task-dispatch-final-landing-prior-consume-reordered.json
semantic-invalid/task-dispatch-final-landing-prior-consume-candidate-mismatch.json
semantic-invalid/canary-pending-landed-rollback-not-released.json
semantic-invalid/canary-probe-rollback-attempt-descriptor-mismatch.json
semantic-invalid/canary-probe-cross-paired-rollback.json
semantic-invalid/canary-probe-wrong-inverse-operation.json
semantic-invalid/canary-probe-resolved-child-plan-mismatch.json
semantic-invalid/canary-probe-resolver-without-consumed-reservation.json
semantic-invalid/canary-probe-unused-release-after-transition.json
semantic-invalid/canary-pending-authorized-landing-release.json
semantic-invalid/canary-pending-probe-transition-release.json
semantic-invalid/canary-pending-extra-transition-release.json
semantic-invalid/canary-probe-rollback-deadline-race.json
semantic-invalid/reversible-mutation-cross-plan-pair.json
semantic-invalid/reversible-mutation-wrong-inverse.json
semantic-invalid/reversible-mutation-reservation-reuse.json
semantic-invalid/reversible-mutation-resolved-child-receipt-mismatch.json
semantic-invalid/reversible-mutation-first-resolution-back-reference-cycle.json
semantic-invalid/reversible-mutation-reserve-before-rollback-approval.json
semantic-invalid/reversible-mutation-forward-before-reservation-consume.json
semantic-invalid/reversible-mutation-expired-rollback-fence-retained.json
semantic-invalid/pair-state-forward-vs-abort-double-winner.json
semantic-invalid/pair-state-attempt-vs-unused-release-double-winner.json
semantic-invalid/pair-state-landing-vs-rollback-double-winner.json
semantic-invalid/pair-state-guard-close-vs-add-child-double-winner.json
semantic-invalid/pair-state-stale-epoch.json
semantic-invalid/pair-state-sibling-key-substitution.json
semantic-invalid/pair-state-consume-persisted-claim-uncertain.json
semantic-invalid/pair-state-claim-persisted-consume-uncertain.json
semantic-invalid/pair-state-crash-before-side-effect.json
semantic-invalid/pair-state-duplicate-claim-replay.json
semantic-invalid/decision-terminal-cancellation-duplicate-target.json
semantic-invalid/task-dispatch-handoff-owner-target-consumer-mismatch.json
semantic-invalid/planned-command-descriptor-task-dispatch-mismatch.json
semantic-invalid/planned-command-descriptor-range-mismatch.json
semantic-invalid/bootstrap-command-run-authorization-mismatch.json
semantic-invalid/bootstrap-command-manifest-authorization-replay.json
semantic-invalid/bootstrap-exit-task-dispatch-mismatch.json
semantic-invalid/task-dispatch-publication-receipt-missing.json
semantic-invalid/task-dispatch-later-range-missing-root-receipt.json
semantic-invalid/task-dispatch-later-range-root-digest-drift.json
semantic-invalid/task-dispatch-current-range-collision.json
semantic-invalid/task-dispatch-a00-resolution-receipt-missing.json
semantic-invalid/task-dispatch-a00-resolution-receipt-wrong.json
semantic-invalid/task-dispatch-a01-root-recreation.json
semantic-invalid/task-dispatch-a01-root-digest-drift.json
semantic-invalid/task-dispatch-a01-child-range-collision.json
semantic-invalid/task-dispatch-a01-cross-parent-root-receipt.json
semantic-invalid/task-dispatch-a01-child-published-not-consumed.json
semantic-invalid/task-dispatch-namespace-before-cutover.json
semantic-invalid/task-dispatch-namespace-after-cutover.json
semantic-invalid/task-dispatch-mixed-namespace.json
```

Expected RED: there is no executable schema authority and invalid fixtures are not deterministically
rejected.

- [ ] **Step 2: Write Draft 2020-12 schemas**

Every schema has `additionalProperties=false`, fixed schema/contract versions, exact enums, strict
envelope shape and state-conditional fields. Encode:

- `{payload,payload_sha256}`, `{lock_payload,lock_payload_sha256}` and
  `{request_payload,request_payload_sha256}` without digest self-reference;
- no future actual manifest refs on ready cards;
- exact path-operation objects and capability booleans;
- planned/materialized traceability roles;
- independent actor identity tuple;
- canonical case/control/failure enums;
- Packet/decision separation and one-time consume records;
- the exhaustive nine-value `packet_type` plus closed `planning_authorization`,
  `execution_authorization`, `landing_authorization`, and `projection_non_executable` phase branches
  specified by child Plan 4's tuple table, so Plans 2–3 never rely on a future schema;
- for `requirements_approval`, one closed purpose enum with no caller extension:
  `verification_target_issuer_selection`, `dependency_selection`,
  `artifact_store_notary_selection`, `interim_decision_channel_continuation`,
  `landing_provider_mode_owner_selection`, `tls_rehearsal_not_applicable`,
  `packet_only_user_exercise`, and `pilot_selection_approval`. The first five require
  `planning_authorization`, exact start/subject/options/outcome/oracle/risk/expiry and one named consumer;
  they forbid result/candidate/PASS, executable commands and provider/host mutation. For
  `verification_target_issuer_selection` and `dependency_selection`, a required closed
  `selection_stage` is exactly `existing_identity`, `create_identity_intent` or
  `post_create_identity_freeze`, and required `selection_subject_id` is conditional on purpose:
  `verification_target_issuer_selection` permits only `verification_target_issuer`; dependency selection
  permits exactly `oci_executor`, `windows_verifier`, `verification_authority`,
  `authenticated_model_boundary` or `result_transport`. One Packet names one subject. `existing_identity` requires assigned IDs/key/revocation plus signed
  current provider readback, forbids create inputs/output slots and consumes once only into the named
  issuer/dependency-record freeze. `create_identity_intent` requires desired stable locators/names,
  immutable inputs, exact create argv/capabilities and typed assigned-ID/key/credential output slots,
  forbids assigned values and consumes once only into its exact provider-setup-request freeze.
  `post_create_identity_freeze` requires the unique prior intent decision plus provider-administration
  request/decision/consume and signed forward/readback/revocation/selected-rollback receipts, requires the
  measured assigned IDs/key/revocation, forbids desired/future/backfilled fields and consumes once only
  into the named issuer/dependency-record freeze. Existing flow has exactly one planning Packet; create
  flow has exactly intent then post-create Packets, each distinct and consumed once. Mixed, repeated,
  reordered, cross-purpose or cross-subject chains reject; post-create must repeat the exact intent
  subject ID and parent decision digest.
  `tls_rehearsal_not_applicable` and `packet_only_user_exercise` also require
  `planning_authorization`, operations/commands/capabilities empty or absent and respectively only the
  campaign-planner or exercise-terminal consumer; each has consume cardinality one and cannot match any
  landing/product/host/mutation consumer. `pilot_selection_approval` requires
  `landing_authorization`, exact immutable selection candidate/change-set/evidence and only the selection-
  landing consumer. DENY/REQUEST-CHANGES events have consume cardinality zero in every purpose.
- `decision-record.schema.json` keeps verdict and approval immutable and adds one closed append-only
  `terminal_cancellation` branch. `subject_kind=authorization_cancellation` requires an existing immutable
  approval and consume count zero; `template_terminalization` requires a sealed unmaterialized range/template
  ID and forbids approval fields; `resolved_lineage_cancellation` binds the exact resolver lineage. Every
  branch requires the winning `consume_and_claim` receipt/owning consumed transaction, terminal receipt and
  one reason `unused_release|abort_release|superseded_by_forward_consume|superseded_by_rollback_consume`.
  Reason/subject compatibility is closed: a forward winner closes its abort subject with
  `superseded_by_forward_consume`; zero-effect abort closes its forward subject with `abort_release`; an
  eligible release closes rollback with `unused_release`; and a rollback winner closes the most-materialized
  still-open incompatible landing, forward, probe-unused-release or setup-unused-release subject with
  `superseded_by_rollback_consume`.
  Exactly one terminal event per subject is allowed; pair state is authoritative, a losing/uncertain claim
  cannot append it, and any later consume/replay rejects.
- `trust_root/execution_authorization` requires closed `execution_scope`:
  `tracked_change` has at least one exact repo operation, `repository_write=true` and no provider-admin/
  host capability. It also requires a closed `repository_identity` object with
  signed envelope URI/SHA-256 plus repeated `kind=project|typed_control_fixture`, immutable `provider_id`,
  immutable `repository_id`, descriptor SHA-256 and exact start commit/tree. The validator must open the
  independently signed `repository-identity` envelope, verify its source/readback, freshness and Git
  objects, and require the repeated tuple to match. Every allowance, descriptor, broker lease, command/
  run/scope manifest and result binds the identical envelope digest and tuple. Missing/unsigned/stale
  identity, caller path as identity, forged readback, project/fixture substitution or tuple drift rejects.
  `artifact_only` has zero repo operations, `repository_write=false`,
  `provider_admin=false`, exact immutable reads/tools/output prefixes/store/notary facts and no host
  capability; `provider_administration` has zero repo operations, `provider_admin=true`, exact resource/
  before-state/API/credential/cost/readback/rollback/revocation facts and no host or repository file/
  content/candidate-construction capability. Closed NHA-L08-only `canary_setup_reservation`,
  `canary_setup_forward`, `canary_setup_rollback` and `canary_setup_unused_release` share the immutable root-
  dispatch/setup-range/template lineage and stable reservation slot. Setup rollback binds exact inverse/
  before-state/readback and a future-reservation-receipt slot, is approved before reservation and remains
  unconsumed. Setup reservation binds that approval, exact ETags/before-state, sufficient rollback-capability
  horizon and CAS/reservation/readback/WORM argv; its fresh decision consume alone creates/readbacks
  `reserved`, and approval alone writes nothing. Setup forward binds the measured receipt and exact setup/
  readback operations and uses atomic consume-and-claim. `canary_setup_unused_release` is a fresh consumed decision allowed either
  before setup start with zero pair effect, or after separately authorized cleanup proves exact initial state;
  it releases that reservation and appends terminal event/receipt. Started partial failure atomically claims
  and consumes setup rollback,
  and uncertainty is reconciliation-only. Its closed NHA-L08-only `canary_enforcement_probe` subbranch
  selects `probe_role=attempt|rollback|reservation|unused_release`. All roles require a preexisting immutable
  candidate digest, Packet-named non-main ref/PR/queue IDs and exact pre-probe ETags. `rollback` additionally
  binds a signed closed `probe_attempt_plan` URI/digest with operation ID/argv/actor class/nonce/expected
  transition/inverse requirements and a `probe_attempt_resolution` deferred parent containing only typed
  rollback and reservation-receipt output slots; it permits only inverse/cancel/restore/readback argv and
  stays unconsumed until an inverse is required. `reservation` binds that same plan/parent, rollback Packet/
  approval, stable slot, ETags and CAS/reservation/readback/WORM argv. Its own fresh consumed provider
  decision performs only that reservation and emits the immutable measured receipt; rollback approval alone
  writes nothing. After receipt readback, the protected resolver emits one signed attempt child range/
  descriptor containing the measured rollback Packet/decision/reservation receipt. `attempt` requires that
  exact resolved child and repeats the shared plan facts
  bidirectionally, plus Git/
  provider attempt argv, exact ordinary non-bypass actor/credential permission digest and
  `expected_outcome=reject|pending`; the ordinary credential may attempt the API but must have zero
  successful transition under enforced policy. It forbids main, commit creation, bypass/admin/unenforced
  credentials and success/merge outcome. Each attempt has exactly one fresh paired rollback approval and
  separately consumed reservation first. Validators enforce one rollback-to-one-reservation-to-attempt
  cardinality and inverse compatibility; cross-probe pairing rejects. `unused_release` requires a fresh
  consumed provider decision over the exact plan/receipt/root/slot/outcome and release/readback/WORM argv.
  Every branch proves the probe attempt caused zero successful transition. Before an attempt and after an
  expected rejection it also proves zero target-ref transition; the pre-attempt branch proves consume count
  zero and appends/readbacks `terminal_cancellation` for the lineage while leaving every approval immutable.
  After pending, it is allowed only
  when exactly one successful transition equals the separately authorized landing decision/head/tree/ref/
  CAS, no extra transition exists and exact landing readback moves the pair to
  `eligible_landing_release`. A fresh release decision then claims `release_claimed`, records terminal reason
  `unused_release`, releases/readbacks the unused rollback reservation and reaches `released`. Pending
  without that landing is retained for terminal rollback cancel; uncertain
  landing never releases it; unexpected transition
  consumes it immediately and fails, and uncertainty requires reconciliation. Its
  `mutation_guard_reserve` special subbranch is permitted only for NHA-P08/P09. It requires a signed
  `reversible_mutation_plan`, rollback Packet/approval, stable reservation output slot, exact root/fencing/
  current guard state, `mode=acquire_root|add_child_guards`, CAS/reservation/readback/WORM argv and a typed
  receipt slot; a fresh consumed provider decision is mandatory and the measured receipt is immutable. It
  may reserve only and cannot itself run a forward, rollback, release, host, repository or credential
  operation. Its `mutation_guard_abort_release` subbranch is permitted only after such a receipt and before
  that pair's forward consume or pair-attributable host side effect. It requires exact plan/resolver/rollback
  approval/reservation, root/guards and immutable signed proofs that this pair's forward consume and
  delegated-surface side-effect counts are both zero; if a resolved child or unconsumed forward approval
  exists, it appends/readbacks `terminal_cancellation` while leaving approval immutable. A fresh provider
  decision then permits exactly one release mode.
  `release_acquired_root` matches an `acquire_root` receipt, requires zero other active/delegated child and
  releases that reservation, all its guards and the empty root. `remove_unused_child_guards` matches an
  `add_child_guards` receipt and releases only that pair's reservation/new disjoint guards while proving the
  shared root/fencing token and every other guard unchanged. Both require owner/epoch CAS equality and WORM
  receipt. A started or uncertain forward for that pair rejects this
  branch and remains rollback/reconciliation-only. Its `mutation_guard_close` subbranch is also permitted only for
  NHA-P08/P09 after all required rollback/reservation and credential readbacks. It requires one exact
  current campaign root/fencing token, the complete guard set, every terminal receipt and atomic close/
  readback/WORM argv; it forbids host/repository/credential/other provider mutation and cannot restore
  state. Missing, failed or uncertain receipt rejects without releasing a guard. Mixed branches reject.
  Every reservation and immediate pre-forward/pre-attempt check requires a closed
  `rollback_capability_horizon` receipt whose minimum decision expiry, inverse credential/lease expiry,
  baseline/checkpoint/artifact and sealed-attachment retention, signer/key verification window and monitor
  availability covers bounded forward-or-queue/readback/restore-or-cancel plus safety margin. The receipt
  fixes the terminal deadline; insufficient horizon or drift blocks forward, while post-side-effect expiry/
  uncertainty preserves the fence and permits reconciliation only.
  Every pair uses one owner/fencing/epoch-bound state key. The Decision Plane performs a closed atomic
  `consume_and_claim` transaction: append the matching consume and CAS/readback
  `reserved -> forward_claimed|attempt_claimed|setup_claimed|release_claimed`,
  `pending -> landing_claimed|rollback_claimed`,
  `eligible_reject_release|eligible_landing_release|eligible_setup_release -> release_claimed`,
  `rollback_required|forward_claimed|setup_claimed|setup_active -> rollback_claimed`, or the normal cleanup
  `setup_active -> cleanup_claimed`, before any external target side effect. A zero-effect abandoned setup
  may claim `setup_claimed -> release_claimed` only after fencing the old runner. A distributed partial claim/
  consume is `uncertain` and blocks all competing branches. Outcome transitions permit only
  `attempt_claimed -> eligible_reject_release|pending|rollback_required`,
  `landing_claimed -> eligible_landing_release|rollback_required`,
  `setup_claimed -> setup_active`, `cleanup_claimed -> eligible_setup_release`,
  `release_claimed -> released`, and `rollback_claimed -> restored|uncertain` after exact readback.
  Release/forward losers, stale epoch, duplicate claim, guard-close versus add-child race and sibling-key
  substitution reject; every transition is WORM/readback. Every phase that emits an external raw finalizer requires exact `finalizer_commands`,
  output prefixes and provider/retention/conditional-create/readback/notary policy prebound before consume;
  aggregate or later projection is never silently covered by that finalizer.
  `PcvAssurancePairState.Tests.ps1` is an exhaustive table-driven oracle, not a sample corpus: it executes
  every allowed Program transition exactly once through claim and required readback, generates the full
  Cartesian product of closed states and rejects every non-enumerated edge, and covers both winners for each
  declared race. It also mutates subject, owner, fencing token, epoch, consume ID, terminal variant/reason and
  outcome prerequisite one field at a time. The three valid `terminal_cancellation` fixtures exercise
  authorization, template and resolved-lineage subjects; the table enforces approval fields only for the
  authorization variant and all four reason/owning-transaction compatibility rules. Discovered case and
  assertion counts must both be greater than zero and equal the generated matrix manifest.
- every consumable `execution_authorization` or `landing_authorization` branch requires one closed
  `finalizer_policy` object containing exact protected tool/argv digests, nonempty allowed output prefixes,
  provider/retention, conditional-create/readback/notary and abort/reconciliation. It covers only mandatory
  raw result/candidate-verification/landing/child-exit outputs enumerated by that request. Planning-only
  request/approval/consume append events are intrinsic Decision-Plane operations and do not smuggle an
  arbitrary publication finalizer; `projection_non_executable` forbids `finalizer_policy`.
- L08 probe rollback and P08/P09 lifecycle-rollback requests require closed
  `intrinsic_request_attachments`. Their initial pair parent already exists in the root task-dispatch receipt;
  Decision-Plane request construction atomically content-addresses, seals and independently reads back the
  signed actual plan, signed first-resolution envelope and exact four-output bundle—rollback child,
  reservation template, stage-two parent and conditional release template. This escrow is non-executable.
  Reservation consume promotes the reservation and first-resolution/stage-two lineage; conditional release
  and rollback are promoted only by their own consumes, and the later attempt/forward child is resolved and
  published only after the measured reservation receipt under its own consume.
- L08 setup-failure rollback uses a distinct static setup lineage and has no pair plan, first resolver or
  stage-two resolver. Its request construction seals the exact signed setup rollback range/descriptor,
  reservation template/stable slot, setup-forward template with its late reservation-receipt authority slot,
  and setup-unused-release template. Setup reservation, forward, rollback and unused-release are promoted
  byte-equal only by their own matching consumes. Both escrow forms grant no repository, provider or host
  mutation; every consumer reopens the root receipt and sealed attachments, never caller-local bytes.
  Retention covers the rollback-capability terminal deadline plus reconciliation margin. Backfill,
  substitution, premature sibling activation or byte drift rejects; unused release records terminal state
  without pretending rollback consumption.
- the `mutation_authorization/execution_authorization` tuple's mutually exclusive
  `brokered_code_change` and `host_or_artifact_mutation` conditional payload branches.
  `brokered_code_change` requires nonempty exact repo operations, the same signed repository identity used
  downstream, exact broker lease/revert, `repository_write=true`, `provider_admin=false` and
  `host_mutation=false`; it maps only to Program `path_resolution=exact_paths` with
  `authority_mapping=mutation_authorization/brokered_code_change`. `host_or_artifact_mutation` requires
  `operations=[]` and maps only to Program `path_resolution=mutation_authorization`, including nested
  category rules that forbid host and rollback reservation for `package_service/build`. A reversible
  `package_service/install`, `http_binding_tls` or `hyperv_actual_vm` Packet is executable only as the one
  resolved child of `reversible_mutation_forward_resolution`; it must bind the unchanged signed pair-plan
  digest, exact rollback Packet/approval, measured active `mutation_guard_reserve` receipt and current
  root/guards. The matching `lifecycle_rollback` requires
  `rollback_kind=installer_lifecycle|tls_binding|hyperv_actual_vm`, binds the same pair plan and stable
  receipt slot without a future forward Packet/decision/receipt value, validates the measured reservation
  receipt at consume, consumes that reservation and forbids a nested reservation. Future-value, cross-plan,
  wrong-inverse, receipt-substitution, unresolved-parent, inactive/resolved-twice child and direct-forward
  forms reject. Cross-mapping, mixed branch fields and use of `trust_root/tracked_change` as a mutation
  Packet reject;
- the three verdict axes and fail-closed truth table inputs.
- bootstrap exit envelope shape, while leaving two-distinct-domain/signature/Git/expiry semantics to
  T03.
- byte-exact bootstrap decision-request branches for A02 through T02, including digest separation,
  nonempty canonical tracked paths/output prefixes, the A05 zero-operation artifact-publication branch,
  the distinct A05-to-T01 execution handoff, T01 landing and T02 execution/landing combinations; this
  historical contract cannot authorize T03 or any new P task.
- bootstrap path semantics require NFC, ordinal-ignore-case operation uniqueness and reject Windows
  reserved-device or trailing-dot/space segments in both A00 external and T03 repository validators.
- byte-exact bootstrap publication receipts with Packet/prefix/source/provider/version/conditional-create/
  checksum/size/retention/readback binding, zero-byte artifact support, semantic source/remote hash+size
  equality and rejection of identical write/read principals.
- bootstrap import-candidate manifests with deterministic sorted one-to-one original-source coverage,
  project lineage, authorization and expected-inventory binding; duplicate/extra/missing/reordered source,
  terminal self-inclusion, mutable/canonical-import claims or receipt/result self-reference rejects.
- planned-command descriptor with exact immutable pre-run command/input/output-prefix/capability facts
  and no measured result, output artifact, verdict or PASS field; it opens the signed task dispatch and
  selected range record and requires their URI/digest/work/range/operation/command/actor/capability/
  boundary facts to equal the descriptor. The later request binds all three digests; only the post-consume
  runner/manifest prove request/decision/consume → range → descriptor equality. Frozen-input and prior-
  output-slot branches reject mixed fields, future slot hashes and non-earlier producers.
- bootstrap command-run record with one planned command ID, measured executable/argv/cwd/environment,
  actor, start/end/exit and complete raw stdout/stderr URI/hash/size, but no verdict/PASS field. It repeats
  the planned descriptor, task-dispatch and selected-range digests and the runner reopens all three before
  execution.
- bootstrap command manifest with work/target IDs, ordered argv/executable digest/cwd/bounded
  environment, actor/trust domain, start/end/exit, raw stdout/stderr artifact URI/hash/size/provider/
  version/retention, and schema-validated source-manifest URI/digest entries; its optional ordered source
  command manifests bind source work/target/descriptor/manifest digests and exact prerequisite lineage;
  it has no caller verdict or PASS field. It reopens the dispatch/range/descriptor chain, proves every run
  repeats the same range authority and carries that chain transitively into result and bootstrap-exit
  validation; a result or exit with no chain, a different range or a descriptor-only caller assertion
  rejects.
- every consumable canonical Decision Packet has required `dispatch_grade=bootstrap_external|canonical`,
  `task_dispatch_uri`, `task_dispatch_sha256`, ordered `task_dispatch_range_ids`,
  `authorized_range_id` and `authorized_range_sha256`. Executable branches require three distinct publication
  receipts for root dispatch, selected/derived range and planned descriptor; landing/prelanding branches
  require root dispatch plus selected/derived range receipts. The validator opens the signed envelope and exact
  immutable range, checks Dispatch Authority signature/key/revocation/expiry and all applicable receipts,
  then requires Packet work/task, range order/digest, operations, commands/finalizers, capabilities,
  actor/lane, output prefixes, decision consumer and boundary to match. Deferred ranges are
  non-consumable until a new signed child exists. Except for A00's approval-event-bound recording finalizer,
  each executable child has one fresh decision and consume; A01 specifically requires its typed consume.
  Cross-range authority or range reorder rejects. `bootstrap_external` is mandatory through E06's
  terminal cutover and remains historically imported as that grade; `canonical` is permitted only for
  E07 and later under `assurance-control://task-dispatch/...`. T03 may bind the imported external T03
  dispatch while cross-validating the candidate implementation; T04 onward uses the repository validator,
  but the bootstrap namespace remains in force until E06. A03 additionally requires exactly the ordered
  execution/spec-revision/landing decision set, three distinct consumers, the same candidate and
  cardinality one for each. P03 requires candidate execution, requirements consume-only, optional spec-
  revision consume-only, final landing and closure in that order. Only A03's spec-revision and P03's named
  requirements/spec-revision ranges may use the closed decision-consume-only prelanding shape: no
  descriptor, implementation command, repository/provider/host/merge capability, and only a mandatory
  root-dispatch receipt, selected-range receipt and consume receipt. The following verified landing opens the ordered receipts
  and requires exact candidate/tree/change-set equality. Every canonical `execution_authorization` additionally requires
  `planned_command_descriptor_uri` and `planned_command_descriptor_sha256`, opens it and requires exact
  authorized-range equality; a landing branch with no planned implementation command omits those fields.
  The sole handoff mapping requires request owner NHA-A05 but target/dispatch/range/descriptor/consumer
  NHA-T01; general branches require all work IDs equal. The dedicated cross-substitution fixture proves
  no validator normalizes the owner into an executable A05 range or accepts the wrong T01 consumer.
  For a multi-range task, only the first consumed root range creates/readbacks the root dispatch and receipt.
  Every later root range requires that byte-identical root/receipt and creates only its own range/descriptor/
  receipts; root recreation, digest drift, missing first receipt or an existing current-range record rejects.
  Any deferred child creates only its child range/descriptor and signed resolution record/receipt linked to
  the original root, never amends the root. A pair/setup child that promotes sealed intrinsic attachments
  additionally requires the attachment-promotion record/receipt. The A00 approval-empty child is the sole
  no-consume exception and may run only as its exact authenticated approval event's prebound recording
  finalizer. A01 and every other executable child require their typed fresh decision/consume. Landing and
  prelanding reopen the same root receipt.

- [ ] **Step 3: Independently validate schema quality**

Before the new repository validator is trusted, a separate Sol verifier invokes an externally pinned
Draft 2020-12 validator. Every valid and semantic-invalid fixture must pass schema; every schema-invalid
fixture must fail at its intended schema location, not from a parse accident. Cross-record/history,
actor separation, digest comparison, Git object and traceability semantics are explicitly pending T03;
ordinary JSON Schema is never claimed to perform them.

The two valid tracked-change fixtures separately prove that both closed `repository_identity.kind`
branches are reachable: one exact project Packet and one exact typed-control-fixture Packet. T03 must
validate each Packet through the complete Packet-to-allowance-to-planned-descriptor semantic chain and
must reject the schema-valid cross-kind substitution fixture with its stable repository-identity code.
Passing only a generic Packet or only one kind cannot close T02/T03.

T02 owns the byte-exact schema and proves `valid/bootstrap-exit.json` plus both bootstrap semantic-
invalid fixtures are schema-valid, while `bootstrap-exit-additional-property.json` rejects at its exact
sidecar location. T03 owns semantic rejection of provider collision and lineage mismatch; neither case
may be relabelled as a T02 schema failure.

- [ ] **Step 4: Commit schema and fixture corpus**

Commit `test: freeze assurance contracts and negative corpus`. This commit is an oracle ancestor of
T03. It must not include validator code.

## Task NHA-T03: Implement the locked validator and protected-path classifier

**Files:** contract projects, PowerShell contract/bootstrap adapters/tests, protected-path schema/record,
`PcvDevelopmentVerification.psm1`, `PcvDevelopmentVerification.Tests.ps1`, and
`docs/DEVELOPMENT_CHANGE_CLASSIFICATION.md`.

- [ ] **Step 1: Verify RED with the frozen T02 corpus**

Run the missing contract CLI and request S/Full classification for a schema, each workflow, current
evidence generator and the validator itself. Expected: CLI missing and at least one path not promoted to
L/Release.

Before requesting T03's tracked execution Packet, use both A00-pinned external repository-identity
verifiers on the authenticated provider event and signed server readback for the exact T02 `main`
commit/tree. They must produce/accept identical signed project-identity envelope bytes and digest. Bind
that external envelope in T03's Packet; the not-yet-trusted T03 identity tool is forbidden from creating
its own authority. Preserve forged, stale and cross-repository external negatives as RED.
Use the A00-pinned external planned-command-descriptor generator and both validators on T03's already
signed dispatch/range, preallocate the content-addressed URI and bind that external descriptor digest into
T03's request. The candidate `New-PcvAssurancePlannedCommandDescriptor.ps1` may reproduce and compare the
bytes during verification but cannot create or validate the authority that permits its own implementation.

- [ ] **Step 2: Implement canonical validation and hashing**

The .NET tool provides only frozen subcommands:

```text
validate --schema <exact-file> --instance <exact-file> --result <exact-file>
canonicalize --input <exact-file> --output <exact-file> --digest-output <exact-file>
validate-tree --repo-root <exact-root> --manifest <exact-file> --result <exact-file>
```

It rejects duplicate JSON properties, invalid Unicode/I-JSON, extra properties, noncanonical digest,
all-zero digests, traversal and symlink escapes. It emits stable `PCV_ASSURANCE_*` codes and never
rewrites an input. The PowerShell wrapper propagates process exit code and parses only a validated
result file.

Create both assurance project files from the exact T01-frozen package metadata. Before a lock exists,
two isolated clean worktrees with separate package caches and the same pinned NuGet source/snapshot run
`dotnet restore <project> --use-lock-file --force-evaluate` once for each project. Compare the two
generated `packages.lock.json` files byte-for-byte and by SHA-256, verify every direct/transitive
version, package SHA-512, source, license and metadata against `toolchain-lock.json`, and fail on any
difference. Commit only the matching locks. From the first verified lock onward, every restore,
including all T03 verification and publication, uses `dotnet restore <project> --locked-mode`; deleting
or regenerating a lock is forbidden without a fresh trust-root revision. Publish self-contained Release
binaries for the locked RID set. Schema validation and canonicalization run in this .NET tool;
PowerShell's rolling `Test-Json`/embedded JsonSchema assembly is never an authority.

`New-PcvAssuranceBootstrapExit.ps1` accepts only an exact command manifest, artifact root and signed
project target manifest plus the exact task-dispatch/range/request/decision/consume chain, measures Git/
actor/command/artifact facts, requires root-dispatch, selected-range and descriptor publication receipts, reopens that chain
through the command manifest, and conditionally requires the derived-child resolution receipt plus pair/setup
attachment-promotion receipt, canonicalizes the embedded payload and
obtains two detached signatures;
it cannot accept a caller verdict override. `Test-PcvAssuranceBootstrapExit.ps1` validates the T02
schema, payload digest, two distinct trust domains, key/revocation, target commit/tree/ancestry,
created/expiry order, command exit/raw hashes and two different accessible replica providers. Frozen
fixtures cover same-domain signatures, tampered raw log, wrong target tree, expired envelope and replica
collision.
For an artifact-only control fixture, optional `-TypedControlTargetManifestPath` is a prerequisite artifact
only; base/result must still equal `-ExpectedTargetManifestPath`. T08/X09 additionally require explicit
`-ImportCandidateManifestPath` and `-ImportCandidatePublicationReceiptPath`; generator and validator prove
every source locator and remote receipt one-to-one rather than assuming a command manifest can include
post-finalization facts.

The validator also extracts the Program §6.1 bootstrap decision-request bytes from the approved Git
blob, requires the expected digest, validates every preserved A02-through-T02 request/approval/consume against
that exact historical schema and recomputes `request_payload_sha256`. It does not reinterpret a bootstrap
request as a new canonical Packet or broaden its target; T08/E06 may import only the original authority
and limitation.

`Publish-PcvAssuranceBootstrapArtifact.ps1` is the only repository-implemented pre-Plan-4 external writer
after T03 is merged; A02 through T03 candidate verification use only the A00-pinned external writer and
both implementations must agree before the cutoff. The repository writer accepts a validated
local input, a consumed Packet, one exact Packet-bound logical prefix/provider/retention branch and a
predeclared receipt path; it performs conditional create, rejects existing/ambiguous keys, reads remote
bytes through a separate credential and records provider/version/checksum/size/retention. Its test wrapper
rehashes the remote bytes and receipt. For exit creation, the generator first canonicalizes the payload,
uses this adapter to publish identical payload bytes to the two A00-frozen distinct replica providers,
then records their readback receipts in `replicas`, signs the envelope and writes it locally. The adapter
then conditionally publishes the complete envelope to its exact exit prefix. No receipt or final envelope
is inserted into the payload it authenticates, so there is no digest self-reference. Import candidates
are published first under their separate prefix. The exit validator opens both payload replicas and the
remote full-envelope receipt/bytes; missing, same-provider, mutable, hash/size/retention mismatch or an
unapproved prefix rejects.
For every execution range, the consumed request's first finalizer uses the then-authoritative adapter—
the A00-pinned external adapter through T03 candidate verification, and the merged repository adapter only
for T04 through E06—to conditionally publish/read back the root dispatch, selected range and planned
descriptor at preallocated URIs, producing three §6.2 receipts before RED or any other command. The first
consumed root range creates all three; a later root range requires the existing byte-identical root and its
original receipt and creates only its own range/descriptor. The pre-request state is local validation only.
Missing bytes/receipt, range/descriptor mismatch, existing first-range key, root recreation, later root drift
or existing current range/descriptor blocks execution. A derived child creates its resolution record/receipt;
a pair/setup promotion creates the additional attachment-promotion record/receipt, both before RED or any
other command. Through T03 the descriptor was produced/validated by the A00-pinned external tools; T03
candidate parity is read-only, and the merged repository generator owns T04 onward.

`New-PcvAssuranceBootstrapImportCandidateManifest.ps1` consumes an exact closed expected-inventory
descriptor plus original immutable source locators, never a caller-created summary. It rereads and hashes
every original byte, validates schema/signature/actor/target/history, proves consume cardinality and a
sorted one-to-one source-ID bijection, excludes the owning terminal exit, and writes the deterministic
`bootstrap-import-candidate-manifest`. `Test-PcvAssuranceBootstrapImportCandidateManifest.ps1` repeats
those checks independently and verifies the consumed artifact-publication decision, project lineage and
planned candidate prefix. It does not publish, consume again or create canonical import status. T08, X09
and E05 reuse these exact schema/tool digests; each supplies a task-specific expected-inventory descriptor.

`New-PcvAssuranceCommandManifest.ps1` accepts a frozen work/target descriptor plus an ordered list of
schema-validated execution/run/artifact manifest paths, the exact planned-command descriptor and required
task-dispatch/range/request/decision/consume paths plus root/range/descriptor publication receipts and the
same conditional resolution/attachment-promotion receipts. It
opens and rehashes every referenced raw
stdout/stderr and actor record, proves command order/exit/target agreement, and writes the closed
bootstrap command manifest deterministically. Every measured executable/argv/cwd/environment/input/
output-prefix/timeout/capability must equal the plan; start/end/exit and raw URI/hash/size come only from
execution. It cannot accept argv results, actor IDs, digests or a PASS boolean as free-form overrides.
Missing, duplicate, reordered or schema-invalid source input blocks bootstrap exit generation.
Its optional `-SourceCommandManifestPath` accepts only an order already frozen by the work descriptor.
For each source it opens and rehashes the complete manifest, proves unique source work/target/descriptor
IDs and that target's required ancestor/exit lineage, and records locator/digest rather than copying a
caller summary. A duplicate, reorder, self-reference, wrong target or non-prerequisite source rejects.

`New-PcvAssurancePlannedCommandDescriptor.ps1` runs before an execution decision is requested. It derives
only from the frozen card/work descriptor, immutable input digests and required
`-TaskDispatchPath`/`-RangeRecordPath`. It validates both signed records, requires their selected range to
match the work descriptor and writes the descriptor whose digest and complete dispatch/range chain are
bound into that Packet. It has no access to a future workspace, result commit, exit code or raw output.
After consume and execution, the measured command-manifest generator requires
`-PlannedCommandDescriptorPath`, `-TaskDispatchPath` and `-RangeRecordPath`, rehashes all three and rejects
every range/argv/input/output-prefix/capability drift rather than updating the plan to match the run. For a `prior_output_slot`, the
runner opens only the named earlier producer record, resolves the exact predeclared output and derives
hash/size itself; a caller-supplied future digest, missing producer, reorder, role/schema mismatch or
alternate path rejects.
For T03 itself this paragraph describes the candidate parity output only: the actual T03 request binds the
A00-pinned external descriptor. After T03 post-merge, the repository generator/validator plus independent
external check become the only allowed T04+ path.

`New-PcvAssuranceRepositoryIdentity.ps1` has no free-form identity fields. Its `project` branch accepts
only an authenticated provider event plus an independently signed provider/server repository readback;
its `typed_control_fixture` branch accepts only a signed allocation record from the protected test
controller. It resolves and verifies provider/repository IDs and exact start commit/tree, signs the
closed envelope and writes its digest. `Test-PcvAssuranceRepositoryIdentity.ps1` independently verifies
schema, signature/key/revocation, source readback/allocation, expiry and Git objects before any scope
allowance or Packet request. Packet, allowance, planned descriptor, broker lease and every downstream
run/scope/result open that same envelope and require both its digest and normalized tuple. The focused
tests run complete valid project and typed-fixture Packet-to-allowance-to-descriptor chains and reject
forged, unsigned, stale, cross-repository, cross-kind and tuple-drift cases with stable codes.
For T03 candidate verification only, the new generator/tester must byte- and digest-match the
A00-pinned external project-identity result; external-versus-candidate disagreement blocks landing.
Only the exact T03 post-merge tools may generate authority for T04 and later.

`New-PcvAssuranceTaskDispatch.ps1` resolves only the Program §5.1 closed branch selected by the approved
plan/card and emits a canonical **unsigned payload** plus digest; it never possesses a Dispatch Authority
key and never writes a signed envelope. A deferred row always remains `ready=false` until the protected
resolver produces exact child operations. The independently controlled Dispatch Authority signs and
conditionally publishes that payload under the namespace valid for the current cutoff. Only the first
consumed root range creates the root envelope/receipt; a later root range must reopen the identical root and
original receipt and creates only its selected range/descriptor receipts. Existing identical root is required
after range one, while an existing current range/descriptor blocks. Resolved children add only child/range
and resolution receipts and never replace the root; pair/setup promotion adds the separate attachment-
promotion receipt. Before publication,
`Test-PcvAssuranceTaskDispatch.ps1 -ValidationPhase PreRequest` verifies the candidate payload against the
source plan blob/section and cannot execute it. After publication/consume, `-ValidationPhase PostConsume`
always requires the exact signed range, distinct root-dispatch and selected-range publication receipts and
request/decision/consume paths. For an execution range it additionally requires the planned descriptor and
its separate publication receipt; for a landing or named decision-consume-only prelanding range those descriptor
inputs are forbidden. Any derived child additionally requires `-ResolutionReceiptPath`; only a pair/setup
child that promotes sealed attachments additionally requires `-AttachmentPromotionReceiptPath`. Supplying
either to a root/unresolved range, omitting resolution for a derived child, or substituting promotion across
pairs rejects. It opens the signed envelope, key/
revocation/expiry, immutable range and receipt and requires Packet/work ID, path or artifact closure,
complete RED/final argv, actor/lane, rollback, boundary and consume cardinality to match.
For an `exact_paths` range it also requires exactly one authority mapping: ordinary tracked work opens a
`trust_root/tracked_change` Packet, while a brokered pilot opens a
`mutation_authorization/brokered_code_change` Packet and matches its lease/revert and false provider/host
flags. The `mutation_authorization` range branch opens only
`mutation_authorization/host_or_artifact_mutation` with `operations=[]`. Any cross-mapping or mixed branch
rejects before consume.
`Invoke-PcvAssuranceTaskVerification.ps1` is execution-range only, requires dispatch/range, the root/range/
descriptor publication receipts and request/decision/consume paths, plus the same conditional resolution and
attachment-promotion receipt arguments, and accepts no command override: it
executes only the record's ordered `Red` or `Final` entries with argv preservation and
complete raw logs. T03 candidate payload/canonicalization/validation/argv-runner outputs must match both
A00-pinned external task-dispatch validators/runners; the candidate generator is never allowed to
self-sign or authorize its own merge. Only after T03 merges may the repository tools own T04+ payload
generation and cross-validation. Missing phase, free-form command, unresolved alias, deferred-ready lie,
Packet mismatch, unsigned/self-signed envelope or unexpected commit/PR/external write rejects.

`Invoke-PcvAssuranceBootstrapCommand.ps1` accepts only a validated planned descriptor, one command ID and
 a predeclared run-manifest/raw-artifact output root plus exact task-dispatch/range/request/decision/consume
paths, root/range/descriptor publication receipts and the same conditional derived-child receipts. It
reopens the descriptor's task dispatch and selected range, requires the consumed Packet digest/
consumer and all records to agree, then selects the closed executable/argv/cwd/environment/
timeout/capability entry from the descriptor, uses an argv-preserving process primitive, kills the full
process tree on timeout, preserves complete stdout/stderr, measures every field and writes one
`bootstrap-command-run` record. It has no free-form command/argument override and cannot execute the
descriptor generator, command-manifest generator or bootstrap-exit generator/validator. Those protected
tools form the non-self-referential pre-authorization/finalization control plane. Direct shell execution
of a descriptor command produces no eligible run manifest and therefore cannot enter a bootstrap exit.

- [ ] **Step 3: Freeze the protected transitive set**

`protected-paths.json` explicitly enumerates contracts, requirements/acceptance/traceability/oracles,
validator and negative corpus, both workflows, classifier/runner, current-evidence schema/generator/
tests, notary/Packet/Dashboard/decision/landing code, quality baselines, stable design and effective-plan
authority. Imports from a protected module are protected transitively. Directory glob is not a write
allowance.

- [ ] **Step 4: Enforce trust-root classification**

Any protected or transitive gate-affecting path resolves to reason `assurance-trust-root`, tier L and
Release. Caller-requested S/M or Fast/Full cannot lower it. Unknown gate-affecting paths fail closed to
L/Release. Add negative cases for traversal, symlink, case variance and hidden transitive imports.

- [ ] **Step 5: Verify and commit T03**

Run .NET tests, focused Pester and the full T02 corpus. Expected: valid fixtures pass; schema-invalid
fixtures reject by schema; semantic-invalid fixtures pass schema and reject by their exact T03 semantic
code; classification downgrade count is zero. Independent Sol reruns from a clean checkout. Commit
`feat: enforce assurance contracts and trust root`.

## Task NHA-T04: Lift current evidence to typed v2 without inventing proof

**Files:** current evidence schema/record/generator/test, six bounded projections, and these sidecars:

```text
docs/ga-ready/evidence/admin-smoke-package-2026-07-16-04265.evidence.json
docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-16-04265-hostmutation.evidence.json
docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.evidence.json
docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-16-04265.evidence.json
```

- [ ] **Step 1: Add semantic-invalid RED fixtures**

Cover additional/duplicate property, empty batch, all-zero hash, nonexistent Git object/tree, traversal,
missing/wrong/reused role, version/result/batch/document hash mismatch, inaccessible artifact, hidden
documented limitation and an untyped projection PASS literal. Supersession fixtures also reject
`completion_required=false` without a signed/notarized record, missing/duplicate old or new roles,
partial mappings, cycles, reuse of one replacement, more than one current required role set, inaccessible
replacement and any attempted rewrite of a historical result/accessibility/limitation. Freeze valid
baseline-without-supersession and synthetic complete-supersession fixtures as well.

- [ ] **Step 2: Define typed v2 roles**

Each reference carries role, evidence ID, result, version, batch, document digest, content facts,
artifact locator/hash/size/producer/expiry/accessibility and limitations. References must use distinct
documents and roles. Resolve provenance with `git cat-file`; format-only commit strings are invalid.

The v2 schema also freezes append-only supersession semantics for later fresh operational proof. Every
entry has `evidence_status=current|historical` and `completion_required`; exactly one complete current
required role set is allowed. `completion_required=false` is valid only for `historical` evidence with a
schema-valid, signed/notarized supersession record that binds every old evidence ID/digest to a distinct
new accessible current evidence ID/digest and exact reason. Supersession never deletes an old entry,
changes its original result/accessibility, or turns unavailable proof into PASS. At T04 there is no
supersession: all four 0.42.65 roles remain current/completion-required and therefore honestly RED.

Schema conditionality is exact: when `artifact_accessibility=accessible`, immutable locator, nonzero
hash, positive size, producer and expiry are all required; when `unavailable`, locator/hash/size are
absent, `unavailable_reason` is required and the assurance verdict must be RED. A zero/fabricated hash
is never used as a null substitute.

Preserve the exact 0.42.65 version/MSI/payload/provenance tuple. Do not rewrite historical Markdown.
Where raw artifacts are unavailable, record `artifact_accessibility=unavailable`; the functional
host limitation projects AMBER before unavailable proof forces overall RED. Never fabricate an artifact
digest.

- [ ] **Step 3: Remove hardcoded PASS prose**

QoS/disk, CLI/Web/service and every limitation sentence must be generated from typed sidecar fields.
Machine JSON is schema- and semantic-validated by T03 before projection. Add
`-RequireAssuranceGreen`; ordinary `-Check` may accept an internally consistent RED record, while the
green switch must fail it.

- [ ] **Step 4: Regenerate atomically and verify**

Run update, then two `-Check` passes. Hash all six outputs and prove byte-identical regeneration.
Expected current operational tuple unchanged and `overall_readiness=red` due inaccessible historical
raw proof. Commit `feat: type current evidence without overstating readiness`.

Rollback reverts v2 schema/record/sidecars and all six generated blocks atomically; it never deletes
historical evidence or changes installed state.

## Task NHA-T05: Add a fail-closed required-Pester adapter

**Files:** `Invoke-PcvRequiredPester.ps1`, its tests, and these non-discoverable fixtures:

```text
pass.ps1.fixture
assertion-failure.ps1.fixture
beforeall-failure.ps1.fixture
parse-failure.ps1.fixture
zero-tests.ps1.fixture
```

- [ ] **Step 1: Prove current false-green cases**

Spawn child PowerShell processes so expected nonzero results do not stop the parent. Capture how the
current public-boundary command treats assertion, BeforeAll/container, parse and zero-test fixtures.
Preserve RED results.

The harness verifies each frozen `.ps1.fixture` digest, byte-copies exactly one case to an isolated
temporary `<case>.Tests.ps1`, executes only that path, and deletes the temp directory afterward. This
ensures assertion/BeforeAll/parse fixtures are actually discovered; a parse fixture misreported merely
as zero tests is a harness failure.

- [ ] **Step 2: Implement the adapter**

Invoke exact Pester 5.7.1 using configuration/`-CI` semantics and `-PassThru`. Always write raw log,
NUnit/JUnit XML and a JSON summary. Exit nonzero unless all are true:

- overall result Passed;
- failed assertions/errors/failed containers are zero;
- discovered and executed tests are greater than zero;
- required skipped/not-run count is zero;
- XML/log exist, are nonempty and match recorded hash/size.

Never transform an expected failing canary into a required-job success. The harness job succeeds only
when it proves each known-bad child returned nonzero.

- [ ] **Step 3: Verify and commit T05**

Expected pass fixture exit 0; four bad fixtures exact nonzero/stable code; missing artifact also nonzero.
Independent clean run repeats all five. Commit `feat: fail closed on required Pester results`.

## Task NHA-T06: Harden development verification evidence

**Files:** verification-target/issuer schemas and issuer record, protected-main issuer workflow and
test, target-manifest generator/test, the three development-verification tools and two existing focused
test files.

- [ ] **Step 1: Add RED execution cases**

Cover PlanOnly `ok=true`, output over 8192 bytes, hung child, caller-hidden protected path, dirty
worktree versus trusted tree, fake base/head, unsigned/wrong-issuer/replayed event envelope, path/mode
mismatch, process-tree leakage, future workflow commit/digest in a selection request, anchor/final workflow
blob drift, provider-readback/workflow-policy mismatch, wrong final selection decision, cross-subject
selection replay and existing/provider-setup readback drift.

- [ ] **Step 2: Implement exact target and raw evidence**

T06's tracked execution Packet preauthorizes the exact schema/tool/test/workflow paths and one final
generator-owned `verification-target-issuer.json` record operation. First create every implementation
blob except that record, then freeze an immutable `issuer_workflow_input_anchor_commit/tree`. Run actual
Release and independent Sol static/security review on that anchor, including OIDC claim, workflow path/
ref/blob and provider-policy negatives. Any anchor amendment stales all later selection/setup authority.
Only this reviewed anchor's actual workflow blob digest—not a future T06 final/merge commit—is eligible for
the issuer-selection flow below.

- `verification-target.schema.json` requires repository ID, event name/delivery ID, event-payload
  SHA-256, issuer and signature/attestation ref, base commit/tree, implementation head/tree, candidate
  head/tree, target branch, generated/expiry UTC and nonce.
- The final generated `verification-target-issuer.json` freezes GitHub OIDC issuer/JWKS, exact audience, repository ID,
  immutable App/environment/principal IDs, allowed event names, the protected default-branch workflow's
  repo-relative path and approved Git-blob/content digest, external signing-service public key/revocation
  and maximum expiry. It also requires `selection_subject_id=verification_target_issuer`, the applicable
  final selection request/decision/consume locators and digests, and one closed provenance branch:
  `existing_identity` binds the signed provider-readback digest; `post_create_identity_freeze` binds the
  original intent decision, provider-administration request/decision/consume, ordered forward/readback/
  credential-revocation/selected-rollback receipts and final-freeze decision as one chain digest. The
  protected generator opens and validates every referenced byte; copied matching IDs without the exact
  chain are invalid. The record deliberately contains no future self-commit literal. At runtime the signing
  service validates both signed OIDC claims: `job_workflow_ref` must name the exact reusable-workflow
  repository/path/ref and `job_workflow_sha` must equal the immutable called-workflow commit. It resolves
  that SHA through the provider API, proves it is a current protected-main ancestor, opens that commit's
  workflow blob and requires its digest to equal the approved record. The resolved commit/blob and
  provider receipt are measured into the signed target; PR code cannot hold the key, choose claims or
  substitute candidate workflow content.
- `assurance-target.yml` runs from the pinned protected-main reusable-workflow commit, captures the raw
  provider event before any candidate checkout, obtains the bounded OIDC token, resolves immutable Git
  objects/PR facts through read-only provider API, and asks the external issuer to sign.
- Before provisioning, Requirement Authority approves and consumes a distinct
  `packet_type=requirements_approval`, `phase=planning_authorization` Packet whose purpose is
  `verification_target_issuer_selection`, whose `selection_subject_id=verification_target_issuer`, and
  selects exactly one mutually exclusive branch. The
  `selection_stage=existing_identity` branch freezes already assigned provider/service/App/environment/principal IDs,
  issuer/JWKS/audience, public key/revocation service and independently signed current provider readback;
  it authorizes no create. The `selection_stage=create_identity_intent` branch freezes only provider/region, desired stable
  resource locators/names, immutable base inputs, capability/owner policy, exact create argv, bounded
  cost/credential lifetime/retention and predeclared output slots for provider-assigned IDs, issuer/JWKS,
  public key and revocation facts. It forbids guessed/future assigned values and does not authorize the
  provider mutation. Both branches bind the reviewed anchor commit/tree only as provenance, the exact
  workflow repo-relative path/ref and actual anchor blob/content digest; a future final/merge workflow
  commit or digest is forbidden. Existing provider policy/readback must already match that path/ref/blob.
- Creating the external signing service/App/environment/credential is a separate trust-root provider
  mutation available only to `create_identity_intent`. Before it, an administrator-specific
  `packet_type=trust_root`, `phase=execution_authorization`,
  `execution_scope=provider_administration` Packet with `operations=[]` must bind the exact desired
  locators/inputs/output slots and provider operations, capability set, owner, credential delivery/
  rotation, cost ceiling, before-state, readback oracle and one conditional rollback branch. It cannot
  bind a not-yet-assigned provider ID or key. Before Plan 4/6 reservation tooling exists, this same one-
  time Packet is the whole bootstrap provider transaction: it permits the rollback argv only after the
  forward operation returns failure or its readback mismatches, records which branch ran, and cannot be
  consumed again. There is no separate rollback child or CAS claim at this phase. Consume it immediately
  before provisioning, perform no unrelated operation, and publish signed forward/readback/rollback/
  credential-revocation evidence. Missing authority or unexpected provider state blocks T06.
- After a `create_identity_intent` readback, Requirement Authority must approve and consume a new
  `verification_target_issuer_selection` planning Packet with
  `selection_stage=post_create_identity_freeze` that freezes only the measured provider-assigned
  IDs/JWKS/public key/revocation facts and exact creation/readback receipts. It cannot amend or backfill
  the first request. The `existing_identity` branch uses its original exact-ID selection and requires no
  post-create decision. `verification-target-issuer.json` is generated only from the applicable final
  exact-ID decision.
- The same already-consumed T06 tracked-file execution Packet remains the single bounded task
  transaction while execution pauses for issuer selection and any provider setup. After the applicable
  exact-ID freeze, the protected generator may resume only its preauthorized singleton
  `verification-target-issuer.json` operation; no second T06 execution Packet or consume is permitted.
  The controller then creates the final candidate commit and proves the anchor-to-final diff is exactly
  that issuer-record operation and that every reviewed workflow/tool/schema blob is byte-identical to
  the anchor. Run final Release and independent Sol verification before requesting a separate landing
  Packet and landing the candidate unchanged. Selection and provider-administration decisions authorize
  no tracked write, cannot substitute for the T06 execution or landing decisions and cannot backfill a
  future workflow digest; any selected-ID, policy, readback, path or anchor drift stales the paused
  transaction.
- `New-PcvVerificationTargetManifest.ps1` accepts only the captured provider event plus issuer receipt;
  it validates JWT signature/JWKS, issuer/audience/subject/repository/workflow-ref claims, external
  signature/revocation, event bytes/delivery ID and policy, reopens the issuer record's final selection
  decision/consume and branch provenance, requires the closed subject and chain digest to match, and
  writes that digest into the signed canonical envelope. A wrong-final, intent-only, cross-subject or
  readback-drift chain rejects. It
  resolves every Git object and ancestry before success.
- Add `-AssuranceMode -TargetManifestPath <exact-file>` to the entrypoint. In assurance mode,
  `-BaseRef` and `-ChangedPath` are rejected rather than ignored; base/head/candidate come only from the
  validated target manifest.
- Canonical diff contains normalized path, create/modify/delete, old/new blob and mode in ordinal order.
- Run `git diff --check <base-commit>...<candidate-head>` against exact objects and keep implementation
  head distinct from a PR synthetic/merge-group candidate.
- Record exact argv array, cwd, bounded non-secret environment, actor, timeout, commit/tree and
  toolchain.
- Write complete stdout/stderr as separate files with hash/size; summary may contain a bounded preview.
- Timeout kills the full process tree and reports failure.
- PlanOnly emits `assurance_case_status=planned`, `eligible_for_pass=false`, `ok=false`; it can never
  satisfy a required gate.
- Required Pester suites call T05's adapter. Preserve the seven-suite catalog unless a separately frozen
  contract explicitly adds a suite.

- [ ] **Step 3: Verify and commit T06**

Run the focused selection/execution tests plus trusted Git temp-repository fixtures. Rehash every raw
artifact. Test wrong issuer/repository, expired/replayed delivery, invalid object/ancestry and caller
scope parameters. Also prove that the issuer record and workflow can share one candidate without a
future-commit reference: exactly one T06 execution request/approval/consume spans anchor construction,
the external-selection pause and the singleton record operation; a second consume or any non-record
anchor-to-final diff rejects. Only the later protected-main runtime commit whose workflow blob matches
the approved digest can issue, while the PR/candidate copy and a protected-main blob mismatch both
reject.
Commit `feat: bind verification to exact target evidence`.

## Task NHA-T07: Close workflow false-green paths

**Files:** both workflows and workflow contract tests listed in the file map.

- [ ] **Step 1: Invert stale workflow tests as RED**

`PcvDevelopmentGateWorkflow.Tests.ps1` currently requires PlanOnly and forbids artifact upload; reverse
those assertions. Update fixed job-count expectations intentionally. Add `merge_group` trigger tests to
both workflows. T01 already changed current action-pin assertions to full SHA plus version comment; T07
must preserve them and owns only false-green/job-DAG semantics. Do not rewrite historical evidence
assertions.

- [ ] **Step 2: Wire actual fail-closed runs**

Both workflows use `pull_request`, `push main`, `workflow_dispatch` and
`merge_group: { types: [checks_requested] }`, full Git history, exact action SHAs and exact Pester.
Development gates execute an actual lane determined from trusted diff; this trust-root PR runs actual
Release, never PlanOnly. Public boundary uses T05.

After T06 is merged, every T07 caller uses the exact reusable-workflow reference
`owner/repository/.github/workflows/assurance-target.yml@<T06-MERGED-COMMIT>`. A branch, tag, PR head,
candidate SHA or later caller-selected SHA is invalid. The issuer requires that value to agree with both
OIDC `job_workflow_ref` and `job_workflow_sha`, the protected-main ancestry proof and the approved blob
digest before signing.

Target mapping is fixed and tested:

- pull request: provider event `pull_request.base.sha` is base, `pull_request.head.sha` is implementation
  head, and the provider-created test merge commit is a separate candidate field;
- merge group: `merge_group.base_sha` is base and `merge_group.head_sha` is candidate; the approved PR
  head remains a separately resolved implementation head. Plan 2 accepts exactly one constituent PR
  whose immutable number/head are signed into the issuer receipt; zero or multiple constituents reject
  `PCV_ASSURANCE_MULTI_PR_GROUP_UNSUPPORTED`. This program never replaces the rule; multi-PR support
  requires a separately approved future Plan-Revision with a closed constituent manifest, never an
  inferred head;
- push: `before` is base and `after` is candidate/head, with the all-zero creation/deletion cases
  rejected for required main verification;
- workflow dispatch: no raw SHA input is trusted; it requires a previously signed verification-target
  envelope and fails when absent/expired.

Checkout uses exact candidate SHA with `fetch-depth: 0`; the GitHub event payload bytes/digest, delivery
ID and mapping are sealed into the target manifest.

The Development Gates DAG is exact: `target-manifest` first; parallel `dotnet-tests`, `web-tests`,
`required-pester` and `negative-canaries` consume it; `actual-release` consumes their raw summaries and
runs current-evidence/exact-diff enforcement; `final-enforcement` has `if: always()` and fails if any
dependency/result/artifact is missing, planned or failed. Each execution job performs its own locked
setup (`setup-dotnet`/`setup-node`, npm ci, locked restore) before tests. Public Boundary has
`target-manifest -> required-public-boundary-pester -> final-enforcement`. Static tests own exact job
names/needs/counts; a lower model may not retain a parallel legacy green job.

Upload raw result/log/XML/manifests with `if: always()`. A later upload success must not mask an earlier
failure. A final enforcement step reads validated summaries and exits nonzero on missing/failed/
planned/zero-test evidence. Source-controlled PR code never receives admin, secret or host capability.

- [ ] **Step 3: Add the negative CI harness**

The harness runs all known-bad Pester and contract fixtures and fails if any bad child returns zero.
Missing XML/log/hash, PlanOnly, required skip/not-run and parse/container failures must make required
jobs fail.

- [ ] **Step 4: Run actual Release and commit T07**

Execute locked restore/test, npm ci/test/parity, all required Pester, contract corpus, current-evidence
consistency and exact diff. Preserve/upload raw artifacts. Commit `ci: eliminate assurance false green
paths` and get independent Sol review.

Rollback before activation is a whole verified revert. After activation, never restore old false-green
commands; set `automatic_landing=false`, `recovery_status=assurance_recovery_blocked`, then fix forward.

## Task NHA-T08: Produce the shadow trust-root exit attestation

**Files:** no product or host files. Produce immutable run artifacts only.

Before requesting T08's execution decision, run
`New-PcvAssurancePlannedCommandDescriptor.ps1` from the frozen T08 card/work descriptor and already signed
T08 dispatch/range, validate it and bind its exact schema, payload digest and dispatch/range digests into
the execution Packet. The descriptor contains no Packet, decision, consume, run or
result fields. Only after the decision is authenticated and consumed may Step 1 run; the later measured
command manifest must match that same descriptor byte-for-byte by digest and field-for-field by semantic
validation.

- [ ] **Step 1: Execute two clean Release reproductions**

The canonical Verification Authority is not implemented until Plan 3. Under the Program §7 bootstrap
protocol, two externally authenticated Sol actors in distinct trust domains/credentials independently
start fresh exact-target checkouts and run the same frozen schemas/cases/toolchain. Compare
artifact-class invariants, not runner-local timestamps/IDs, and preserve both transcript/CI replicas.

- [ ] **Step 2: Produce import candidates for already consumed bootstrap decisions**

Validate A00's approval/A01 consume, every A02–A04 execution/landing event, A05's separate artifact-only
request/approval/consume, A05's T01 execution request/approval, T01's execution consume, T01's separate
landing request/approval/consume, every T02–T07
execution/landing event, and T06's branch-conditioned issuer identity history. `existing_identity`
requires exactly one issuer-selection request/approval/consume plus its preexisting signed readback and
zero setup/post-create chain. `create_identity_intent` requires exactly the intent request/approval/
consume, signing-provider setup trust-root request/approval/consume plus forward/readback and any selected
rollback/credential-revocation events, then exactly one `post_create_identity_freeze` request/approval/
consume. In either branch, T06 has exactly one tracked execution request/approval/consume whose unchanged
decision ID spans the anchor pause and sole issuer-record operation, followed by one separate landing
request/approval/consume; a second T06 tracked execution chain rejects. Include all signed A00–A05 and
T01–T07 exits, and T08's own execution
request/approval/consume with the now-trusted schemas. Generate one deterministic manifest with exactly
one candidate entry per original source, preserving original IDs and binding only its exact task target,
and have the expected-inventory descriptor encode the selected branch and exact one-versus-two planning-
chain cardinality. An extra create/post-create event in the existing branch or a missing/reordered intent,
provider receipt or post-create freeze in the create branch rejects. T08 reopens the final issuer record
and signed target and requires their `selection_subject_id`, final decision/consume digests and closed
existing-readback or intent-to-post-freeze chain digest to equal this inventory byte-for-byte; copied IDs,
wrong-final or cross-subject provenance rejects. Publish that manifest create-only at
`assurance-bootstrap://import-candidates/NHA-T08/<manifest-payload-digest>/manifest.json`. These are external import
candidates, not repository-backed/canonical import events and never consume again. Prove each external
consume occurred exactly once. T08's final exit is terminal and therefore not a member of its own
inventory. Any missing/duplicate/mismatched source invalidates the owning task and stops later plans.
Plan 4 alone imports these candidates into its canonical store.

The same expected inventory also enumerates every bootstrap task-dispatch authority object exactly once:
the separate A00/A01 root and deferred-parent range objects/receipts, their resolved child ranges and signed
resolution records/receipts;
every A02–A05 and T01–T07 signed task manifest, ordered executable/deferred/resolved range records,
signatures and receipts; every derived child's signed resolution record/receipt; and, only for a pair/setup
child that actually promotes sealed attachments, its attachment-promotion record/receipt, forbidden
otherwise; every
execution range's planned-command descriptor URI/digest and distinct §6.2
publication/readback receipt; and T08's own signed manifest, selected range, descriptor and receipts published immediately
after its consume. For A03 it requires the same ordered execution/spec-revision/landing range set and three
distinct Packet/consume bindings; for every expanded tracked shorthand it requires both candidate and
resolved landing range records. Source IDs, range order, Packet-to-range digest equality and one consume
per executable range are closed cardinalities. Missing/extra/reordered/range-reused objects, an unresolved
deferred range, an unreceipted envelope/descriptor or inclusion of T08's terminal exit rejects.

- [ ] **Step 3: Assert the shadow exit truth**

Required outcomes:

- valid contract corpus 100% accepted and known-bad corpus 100% rejected;
- trust-root path classification cannot be lowered;
- Pester assertion/BeforeAll/parse/zero-test cases all reject;
- PlanOnly cannot PASS;
- raw new-run bootstrap replicas are accessible and hash-valid, without claiming Plan 4 WORM/notary;
- two independent invariant sets agree;
- current evidence is internally valid but RED because historical raw proof is unavailable;
- server `required_enforced=false`, automatic landing disabled, activation blocked.

Record `NHA-T08-shadow-trust-root-v1` at
`assurance-bootstrap://exits/NHA-T08/<T07-MERGED-MAIN-TREE>/<payload-digest>`. Its lineage kind is
`artifact_only`, its base/result commit and tree are the same exact T07 post-merge `main`, and it has no
candidate/merged/PR fields. It binds T08's consumed execution decision plus every import-candidate
locator, the exact import-candidate manifest and its publication receipt one-to-one. It is prerequisite
evidence for Plan 3, not environment GREEN.

## Required verification commands

The `required-pester` descriptor entry freezes these eighteen paths exactly:

```text
packaging/windows-desktop-node/tests/PcvAssuranceContracts.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceBootstrapExit.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceBootstrapArtifact.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceBootstrapImportCandidateManifest.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssurancePlannedCommandDescriptor.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceBootstrapCommandRunner.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceCommandManifest.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceRepositoryIdentity.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceTaskDispatch.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceToolchainLock.Tests.ps1
packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1
packaging/windows-desktop-node/tests/PcvRequiredPester.Tests.ps1
packaging/windows-desktop-node/tests/PcvDevelopmentVerificationExecution.Tests.ps1
packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1
packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1
packaging/windows-desktop-node/tests/PcvAssuranceTargetWorkflow.Tests.ps1
packaging/windows-desktop-node/tests/PcvVerificationTargetManifest.Tests.ps1
packaging/windows-desktop-node/tests/PcvPublicBoundaryWorkflow.Tests.ps1
```

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssurancePlannedCommandDescriptor.ps1 `
  -WorkDescriptorPath <exact-work-descriptor> -CardPath <exact-frozen-card> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -OutputPath <exact-planned-command-descriptor>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId contracts-project-restore -OutputRunManifestPath <run:contracts-project-restore>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId contracts-tests-restore -OutputRunManifestPath <run:contracts-tests-restore>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId contracts-dotnet-test -OutputRunManifestPath <run:contracts-dotnet-test>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId required-pester -OutputRunManifestPath <run:required-pester>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId contract-result-validation -OutputRunManifestPath <run:contract-result-validation>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId current-evidence-check -OutputRunManifestPath <run:current-evidence-check>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId development-verification-release -OutputRunManifestPath <run:development-verification-release>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceBootstrapCommand.ps1 `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -CommandId trusted-git-diff-check -OutputRunManifestPath <run:trusted-git-diff-check>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceCommandManifest.ps1 `
  -WorkDescriptorPath <exact-work-descriptor> `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -ValidatedInputPath @(
    <run:contracts-project-restore>,
    <run:contracts-tests-restore>,
    <run:contracts-dotnet-test>,
    <run:required-pester>,
    <run:contract-result-validation>,
    <run:current-evidence-check>,
    <run:development-verification-release>,
    <run:trusted-git-diff-check>,
    <exact-pester-summary>,
    <exact-development-verification-summary>
  ) `
  -OutputPath <exact-command-manifest>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceBootstrapImportCandidateManifest.ps1 `
  -ExpectedInventoryDescriptorPath <exact-t08-expected-inventory-descriptor> `
  -SourceLocatorManifestPath <exact-original-source-locator-manifest> `
  -AuthorizationDecisionPath <exact-consumed-t08-execution-decision> `
  -ProjectTargetManifestPath <exact-signed-target-manifest> `
  -OutputPath <exact-import-candidate-manifest>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapImportCandidateManifest.ps1 `
  -InputPath <exact-import-candidate-manifest> `
  -ExpectedInventoryDescriptorPath <exact-t08-expected-inventory-descriptor> `
  -AuthorizationDecisionPath <exact-consumed-t08-execution-decision> `
  -ProjectTargetManifestPath <exact-signed-target-manifest>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Publish-PcvAssuranceBootstrapArtifact.ps1 `
  -InputManifestPath <exact-import-candidate-manifest> `
  -AuthorizationDecisionPath <exact-consumed-t08-execution-decision> `
  -LogicalPrefix assurance-bootstrap://import-candidates/NHA-T08/ `
  -OutputReceiptPath <exact-import-candidate-publication-receipt>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceBootstrapExit.ps1 `
  -CommandManifestPath <exact-command-manifest> -ArtifactRoot <exact-bootstrap-artifact-root> `
  -TaskDispatchPath <exact-t08-task-dispatch> -RangeRecordPath <exact-t08-range-record> `
  -PlannedCommandDescriptorPath <exact-planned-command-descriptor> `
  -TaskDispatchPublicationReceiptPath <exact-t08-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <exact-t08-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <exact-t08-planned-descriptor-receipt> `
  -AuthorizationRequestPath <exact-t08-authorization-request> `
  -DecisionEventPath <exact-t08-approval-event> -ConsumeEventPath <exact-t08-consume-event> `
  -ExpectedTargetManifestPath <exact-signed-target-manifest> `
  -ReplicaProviderPolicyPath <exact-a00-replica-provider-policy> `
  -ReplicaReceiptRoot <exact-exit-payload-replica-receipt-root> `
  -ImportCandidateManifestPath <exact-import-candidate-manifest> `
  -ImportCandidatePublicationReceiptPath <exact-import-candidate-publication-receipt> `
  -OutputPath <exact-bootstrap-exit>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Publish-PcvAssuranceBootstrapArtifact.ps1 `
  -InputPath <exact-bootstrap-exit> `
  -AuthorizationDecisionPath <exact-consumed-t08-execution-decision> `
  -LogicalPrefix assurance-bootstrap://exits/NHA-T08/ `
  -OutputReceiptPath <exact-bootstrap-exit-publication-receipt>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapExit.ps1 `
  -InputPath <exact-bootstrap-exit> -ExpectedTargetManifestPath <exact-signed-target-manifest> `
  -ImportCandidateManifestPath <exact-import-candidate-manifest> `
  -ImportCandidatePublicationReceiptPath <exact-import-candidate-publication-receipt> `
  -PublicationReceiptPath <exact-bootstrap-exit-publication-receipt> `
  -ReplicaReceiptRoot <exact-exit-payload-replica-receipt-root>
```

The exact descriptor entry for `required-pester` contains the eighteen focused paths listed above and in the T08
card, including `PcvAssuranceBootstrapCommandRunner.Tests.ps1`; the restore/test, contract validation,
current-evidence, Release and Git-diff entries contain the exact argv shown by their owning steps above.
The descriptor generator is a non-mutating pre-authorization control protected by the already-landed
T03 tool digest and is not recursively listed inside its own descriptor. After the T08 Packet binds its
output and is consumed, only the eight named descriptor commands run through the protected wrapper; each
produces a schema-validated run record and complete raw logs. `New-PcvAssuranceCommandManifest`, import-
candidate manifest generation/independent validation and publication, `New-PcvAssuranceBootstrapExit`, full-envelope publication and
`Test-PcvAssuranceBootstrapExit` are the protected finalization chain outside the measured range and are
never self-listed. Their exact tool/argv/provider/prefix facts are nevertheless prebound by the consumed
T08 Packet, and conditional-create/readback receipts plus remote bytes must validate. The two clean replicas use distinct command IDs,
actors, credentials, workspaces and output roots expanded from these eight templates; one replica's run
record cannot satisfy the other.

The frozen card replaces angle-bracket metavariables with exact values before execution. They are not
accepted as runtime literals.

## Plan 2 exit gate

- [ ] T01–T07 commits/PRs are exact `main` ancestors and separately reviewed; T08 is artifact-only and
      has no commit/PR.
- [ ] T01–T07 each have distinct execution/landing decisions and a verified post-merge
      `assurance-bootstrap://exits/NHA-TNN/...` envelope; candidate-only artifacts are not exits. T08
      has one execution decision, no landing decision and only its explicitly typed artifact-only exit;
      canonical import remains deferred to Plan 4.
- [ ] All trust-root files resolve L/Sol/Release with no caller downgrade.
- [ ] Known-bad contract and Pester corpora reject 100%.
- [ ] Current evidence v2 is typed, deterministic and honestly RED.
- [ ] Required runner evidence is exact-target, complete, accessible and non-PlanOnly.
- [ ] Two independent clean Release invariant sets agree.
- [ ] Package/service/TLS/Hyper-V mutation count is zero.
- [ ] Landing enforcement and automatic activation remain RED/blocked pending Plan 5.
