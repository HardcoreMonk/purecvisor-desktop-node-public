# PureCVisor Assurance Authority and Contract Integration Implementation Plan

> **Status:** proposed child plan 1; no task may execute until the program plan is separately reviewed
> and approved.
>
> **Execution class:** every tracked change in this plan is trust-root work: `L`, `gpt-5.6-sol`,
> `ultra`, actual `Release`, with a different trust-domain Sol reviewer.

**Goal:** Make the approved no-human-code-review design a stable authority, publish successor v4 and
derived guidance without activating it, and prepare a fresh-main digest-bound Plan 2 trust-root
bootstrap request.

**Architecture:** An empty exact-plan approval commit, four ordered merge/PR boundaries (A01–A04), and
one external bootstrap request (A05) preserve the approved design commit as an ancestor, amend the Luna
stable owner contract, replace incompatible successor v3 card rules, regenerate human-facing policy,
and prepare Plan 2 without materializing execution state. Product code, workflows, current evidence and
host state remain unchanged.

**Prerequisite:** A00 requires authenticated approval of the exact planning commit/tree/seven-plan
path-blob manifest and the frozen bootstrap trust profile; it creates the durable A00 record and therefore
does not require that record as its own prerequisite. A01 and every later task require the completed,
validated durable A00 record. Commits `4031c50687414ebc0706441d441d93f6c6f06863` and
`dcae9b0d0050397fc1e5145e12bdd99414bfe654` must be present in the reviewed planning branch.

**Source design:**
`docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`

---

## File map

**A00 — durable program execution approval**

- No tracked file. Create an empty Git approval commit after the user approves the exact plan
  commit/tree/path-blob manifest digest.

**PR A01 — approved design becomes an ancestor**

- No new implementation file. Merge the reviewed design/planning branch with commit-preserving
  history and capture exact post-merge facts.

**PR A02 — stable authority integration**

- Modify `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-luna-completion-control-design.md`.
- Modify `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`
  only to change authority status from pending to locator-bound integrated.

**PR A03 — successor v4**

- Modify `docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md`.

**PR A04 — derived projections**

- Modify `docs/CODING_GUIDE.md`.
- Modify `docs/DEVELOPMENT_VERIFICATION_POLICY.md` outside its generated current-evidence block.
- Modify `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-weekly-service-development-spec.md`.
- Modify `docs/DEVELOPER_INDEX.md` without changing its effective-current pointer.

**A05 — Plan 2 trust-root bootstrap request, after A04 is merged**

- Produce an immutable external bootstrap artifact at logical URI
  `assurance-bootstrap://trust-root/<PACKET-ID>/<REQUEST-DIGEST>`.
- Do not create `execution-state.json`, task cards, product files or host artifacts.

## Bootstrap-grade verification used by A01–A05

The canonical Plan 2 runner/notary does not exist yet. Every A work item therefore uses the Program
Plan §7 bootstrap envelope and two independent clean actors. For each immutable candidate commit,
freeze `<base>`, `<candidate>`, `<target-tree>`, `<work-id>`, `<run-id>` and artifact root before running:

```powershell
if ((git rev-parse HEAD) -ne '<candidate>') { throw 'candidate HEAD mismatch' }
git cat-file -e '<base>^{commit}'
git cat-file -e '<candidate>^{commit}'
if ((git rev-parse '<candidate>^{tree}') -ne '<target-tree>') { throw 'candidate tree mismatch' }
git merge-base --is-ancestor '<base>' '<candidate>'
if (git status --porcelain=v1) { throw 'bootstrap checkout is not clean' }
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L -BaseRef <base> `
  -ArtifactRoot <bootstrap-artifact-root>
dotnet restore src/DesktopNode.sln
dotnet test src/DesktopNode.sln -c Release --no-restore
npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web
git diff --check <base>...<candidate>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check
```

The actor also imports exact Pester 5.7.1 and invokes `Invoke-Pester -CI -PassThru -Output Detailed`
separately for these three roots: `packaging/windows-desktop-node/tests`,
`packaging/windows-desktop-node/installer/tests`, and `web/tests`. For each result,
`Result != Passed`, `FailedCount > 0`, `FailedContainersCount > 0`, `TotalCount = 0` or unexpected
`SkippedCount|NotRunCount > 0` is nonzero. The command manifest contains each argv array, executable
digest, clean cwd, target objects and complete stdout/stderr artifact URI/hash/size; the current
runner's 8 KiB summary preview is not raw proof. A second actor repeats from a different fresh checkout,
rehashes the target and signs the envelope. Because server policy, canonical notary and hardened runner
are not yet present, this is a shadow/bootstrap Release result and never assurance GREEN.

Metavariables above are replaced with exact immutable values in the Packet and command manifest; no
literal angle-bracket value is executable.

Before A00 approval is requested, freeze the Program §5 interim authenticated decision-channel profile:
immutable provider/channel and user-principal IDs; provider-signed export or OIDC issuer/subject/audience
and verification key; nonce/expiry/revocation; exact response grammar; and create-only request/approval/
consume locators. The channel must export independently verifiable authentication evidence. Repository
comments, display names, editable transcripts and model assertions are invalid. This profile authenticates
A00 and every later bootstrap/shadow decision until the atomic L04 cutover; inability to prove it blocks
A00 rather than falling back to an ordinary conversation locator.

Every A02-through-T02 execution or landing request is a Program §6.1
`pcv-assurance-bootstrap-decision-request-v1` surrogate. Both A00-frozen external validators must accept
the exact bytes and both canonicalizers must produce the stated digest before the channel can approve;
the future Plan 2 schema/tool is not presumed. A05's artifact-only authority and T01 handoff are separate;
T01 execution uses the handoff, T01 landing and both T02 phases use their own requests. The A00-pinned
external publisher and §6.2 receipt contract are the only publication mechanism through T02. T03
candidate verification cross-checks the new repository tools; only after T03 merges do T04+ require the
repository publisher together with the independent external verifier. T08 preserves the original bytes
and E06 imports them with their bootstrap limitation.
Every surrogate prebinds its exact finalizer argv, output prefix, A00 publisher/client digest, provider,
retention and separate-principal readback receipt. Therefore only its mandatory raw exit and explicitly
named non-independent candidate-verification output may be emitted as part of that consumed transaction;
any other independent publication needs a new artifact-only request.
For A02–A04, the execution request also prebinds that task's exact `candidate-verification` prefix and
publisher finalizer as a non-independent output of the same consumed execution transaction; its landing
request separately prebinds only the final `exits` prefix. A05's T01 handoff applies the same split to T01.
Before any such request is presented, Dispatch Authority signs and locally validates the complete task
manifest/range, preallocates its bootstrap URI and the planned-command descriptor is generated from those
fixed records by the A00-pinned external generator and both validators, with a preallocated content-
addressed URI; the execution request binds all three digests. Immediately after the matching decision is
consumed, its prebound finalizer conditionally publishes/readbacks the signed root dispatch, selected range
and descriptor with three separate §6.2 receipts before RED or any other range action. A merely valid but unconsumed dispatch or
local-only/mutable descriptor is never executable.
For multi-range A03, the execution consume alone creates the root-dispatch receipt. Its spec-revision and
landing ranges must reopen that byte-identical root/original receipt. The spec-revision root range creates
only its selected-range receipt; the resolved landing child creates its child-range receipt plus signed
resolution record/receipt and no descriptor. A02/A04 shorthand-expanded verified landing children follow the
same child-range+resolution rule. None recreates or amends the root.

## Normative task-dispatch matrix

For every row, run the Program §5.1 canonical Test, Red and Final invocations with
`-ExpectedWorkId`/Packet work ID equal to the row ID. The signed row expands every referenced File-map
entry to ordinal exact paths and complete argv before consume; an unresolved alias is invalid. `Final`
always includes actual Release plus an independent different-trust-domain Sol verifier unless the row is
explicitly no-implementation. The task section supplies the detailed oracle and finalizer inputs.

| Work ID | Ordered path/range closure | RED contract | Implementation/finalizer boundary | Rollback | Commit/PR boundary |
|---|---|---|---|---|---|
| NHA-A00 | signed `program_approval_empty_commit` deferred parent resolves after the approval event to one `approval_empty_commit` child with exact planning parent/same-tree/message/trailers/ref; finalizer publishes A00/A01 root+parent objects and A00 child+resolution with six distinct receipts | tampered manifest/profile/event/resolver output, root recreation, parent drift, child collision or missing resolution receipt must reject | create only durable approval empty commit/event and raw A00 exit | invalidate approval; no file mutation | resolved child `approval_empty_commit` |
| NHA-A01 | reopen byte-identical A01 root+parent receipts; signed `program_approved_candidate_landing` deferred parent resolves only after verified A00 commit/exit to one `approved_candidate_landing` child plus resolution receipt binding full design-commit lineage, seven-plan change-set, merge method and ref CAS | wrong A00 output slot, root/parent/child/receipt, parent/tree/consume/merge method rejects | unchanged commit-preserving landing of approved design commits and seven plans, plus raw exit only | stop before merge or revert whole merge | resolved child `tracked_pr`, no new file operation |
| NHA-A02 | `exact_paths`, exactly the two design files in A02 Files | six-outcome ambiguity matrix RED | only stable Luna authority design and assurance design amendments plus prebound finalizers | whole-commit revert | `tracked_pr` |
| NHA-A03 | ordered `exact_paths` one-successor-document execution range, deferred post-verification `artifact_only` `spec_revision` decision range, then deferred `verified_candidate_landing` range; all three bind the same immutable candidate | invalid v4 transition/owner/card cases RED | only inactive successor Plan-Revision v4 document; exactly one execution, one spec-revision and one landing consume in that order | whole-commit revert | candidate `candidate_commit_no_merge`; spec decision `artifact_only_no_commit`; landing `tracked_pr` |
| NHA-A04 | `exact_paths`, exactly CODING_GUIDE, verification policy, weekly spec and DEVELOPER_INDEX | v3/v4 route and lifecycle conflict corpus RED | only four derived guidance/projection files | whole-commit revert | `tracked_pr` |
| NHA-A05 | `artifact_only`, exact A05 root-dispatch/selected-range/descriptor receipts, reviews/command/request/decision/consume/exit prefixes | missing/duplicate/stale A00–A04 input rejects | publish A05 root dispatch/range/descriptor first, two clean review runs, freeze unconsumed T01 handoff and raw A05 exit; no tracked write | invalidate publication; preserve failure | `artifact_only_no_commit` |

## Task NHA-A00: Record exact program-plan execution approval

**Files:** no tracked file.

- [ ] Commit these seven planning documents as one candidate planning commit.
- [ ] Build an RFC 8785 manifest of exact repo-relative path, Git blob ID and mode for all seven files;
      record candidate commit/tree and manifest SHA-256.
- [ ] Build the bootstrap trust profile, derive deterministic Packet surrogate
      `NHA-A00:<approved-plan-commit>:<approved-plan-manifest-sha256>`, and hash the RFC 8785 approval
      request containing the exact tuple, trust-profile digest, separately named Program §6.1 decision-
      request schema SHA-256
      `b719c93bebef0fe5028e551069304b0a12d7d894c41c93be89df52c60ed47a0e`, §6.2 publication-receipt
      schema SHA-256 `a4f7e6bf835c0b91f1bf2e642fb037d2e6b912df29900ffe84d04ae140319ea0`, §7 exit schema
      SHA-256 `c398ac5f2d13df77a579697f052df259e12aed147212b817b16f0e6c081de115`, two exact external
      validator/canonicalizer binary digests, two exact external repository-identity verifier binary
      digests, their closed procedure digest, the external task-dispatch schema and canonical-generator
      digests, two exact external task-dispatch validator/argv-runner binary digests, dispatch signer/
      key/algorithm/revocation/namespace, the Program §5.1 dispatch procedure digest, external planned-
      command-descriptor schema/canonical-generator/two validator binary digests and descriptor procedure
      digest, and the signed A00/
      A01 deferred parent manifests with resolver digests, typed event/commit/exit/candidate output slots
      and ordered range IDs. The approval request includes those exact two parent-manifest digests and statement “Plan execution approval
      opens A01 only and does not
      approve A02/A03/A04 trust-root candidates, materialization, activation or mutation.”
- [ ] In two isolated environments, hash the external planned-command-descriptor schema, canonical
      generator, both validators and closed procedure; require identical component-manifest bytes/digest.
      Run one valid dispatch→range→descriptor case plus wrong-range, future-result and argv-drift negatives
      through both validators. Bind the exact five component digests and their aggregate manifest digest in
      the approval request/trust profile; disagreement blocks A00.
- [ ] After authenticated user approval, create an empty commit with `Program-Approval`,
      `Approved-Plan-Commit`, `Approved-Plan-Tree`, `Approved-Plan-Manifest-SHA256`, unique
      `Approved-Bootstrap-Trust-Profile-SHA256`, `Approved-Bootstrap-Decision-Request-Schema-SHA256`,
      `Approved-Bootstrap-Artifact-Receipt-Schema-SHA256`, `Approved-Bootstrap-Exit-Schema-SHA256`,
      `Approved-Bootstrap-Validator-1-SHA256`, `Approved-Bootstrap-Validator-2-SHA256` and
      `Approved-Bootstrap-Repository-Identity-Verifier-1-SHA256`,
      `Approved-Bootstrap-Repository-Identity-Verifier-2-SHA256`,
      `Approved-Bootstrap-Repository-Identity-Procedure-SHA256` and
      `Approved-Bootstrap-Task-Dispatch-Validator-1-SHA256`,
      `Approved-Bootstrap-Task-Dispatch-Validator-2-SHA256`,
      `Approved-Bootstrap-Task-Dispatch-Schema-SHA256`,
      `Approved-Bootstrap-Task-Dispatch-Generator-SHA256`,
      `Approved-Bootstrap-Task-Dispatch-Signer-Profile-SHA256`,
      `Approved-Bootstrap-Task-Dispatch-Procedure-SHA256` and
      `Approved-Bootstrap-Planned-Command-Descriptor-Schema-SHA256`,
      `Approved-Bootstrap-Planned-Command-Descriptor-Generator-SHA256`,
      `Approved-Bootstrap-Planned-Command-Descriptor-Validator-1-SHA256`,
      `Approved-Bootstrap-Planned-Command-Descriptor-Validator-2-SHA256`,
      `Approved-Bootstrap-Planned-Command-Descriptor-Procedure-SHA256` and
      `Approved-A00-Task-Dispatch-Parent-SHA256`, `Approved-A01-Task-Dispatch-Parent-SHA256`,
      `User-Approval` trailers. The three schema trailers equal the literals above. The trust-profile
      digest binds the Program §§6.1/6.2/7 embedded contract bytes, both external validator/canonicalizer
      binary digests, both repository-identity verifier digests and their exact provider-event/readback/
      signature/freshness/Git-object procedure digest, task-dispatch schema/generator and both validator/
      runner digests, independent dispatch signer/key/algorithm/revocation/namespace, external planned-
      descriptor schema/generator/two validators/procedure digests, exact A00/A01
      deferred parent bytes/locators/digests/range IDs/resolver/output slots and the exact §5.1 path/RED/final/actor/rollback/boundary
      procedure digest, two signer algorithms/public identities and
      revocation locators, plus the interim
      channel profile, approval-event namespace/template and a preallocated nonce. It additionally binds
      the exact external bootstrap publisher/client binary digest, two exit-replica provider IDs,
      distinct write/read credential roles, allowed prefix families, minimum retention and closed
      conditional-create/readback receipt rules used by A02–T02, plus exact A00/A01 raw-exit finalizer
      argv/digests, prefixes, providers and retention/readback rules. It does not bind the future approval-
      event locator. Its parent must be the exact planning commit.
- [ ] Before user approval, validate the signed A00/A01 deferred parent manifests locally with both
      pinned external validators and include their exact bytes/digests/range IDs/resolver digests/output
      slots in the approval request; do not publish them yet. After the approval event, Dispatch Authority
      deterministically resolves/signs the A00 child only, and A00's prebound finalizer conditionally
      creates/readbacks exactly these separate objects under `assurance-bootstrap://task-dispatch/...`:
      the A00 root and A00 deferred-parent range, the A01 root and A01 deferred-parent range, the A00
      resolved-child range and the signed A00 resolution record. It validates exactly one §6.2 receipt for
      each object—six receipts total—before commit creation. The empty commit message binds the two parent
      digests but deliberately omits the resolved child digest so the child's authorization of that exact
      message is not self-referential; the publication receipt and A00 exit bind the measured child digest.
      Only after that commit/exit is
      immutable may Dispatch Authority reopen the byte-identical A01 root/parent objects and original two
      receipts, resolve/sign/publish only the A01 child range and signed A01 resolution record with one new
      §6.2 receipt each, and perform its typed consume. A01 cannot recreate the root, replace the parent or
      child, or treat publication alone as executable authority. Root/parent digest drift, an existing
      current child, cross-parent/root substitution or a missing/wrong resolution receipt rejects.
- [ ] After approval, validate that the actual provider-signed event uses the prebound namespace/template
      and nonce, then record its locator in the empty approval commit/exit as a measured external fact
      outside the already-hashed trust profile/request. A missing, reused or substituted locator blocks.
      The deterministically resolved A00 child-range digest is likewise a measured post-approval fact
      recorded in the exit/readback only—never in a commit message or trailer—and is not an input to the
      preapproval trust-profile digest. The commit contains only the preapproved parent digests and
      measured approval locator among task-dispatch facts.
- [ ] Verify the empty approval commit changes zero paths and publish
      `assurance-bootstrap://exits/NHA-A00/<approved-tree>/<payload-digest>`. Its authorization object
      uses the Program §6 A00 mapping with `state=approved`; its `approval_commit` lineage names the
      planning parent/commit/tree and proves the approval commit reuses the exact parent tree. The exit's
      publication inventory binds exactly the A00 root receipt, A00 parent-range receipt, A01 root receipt,
      A01 parent-range receipt, A00 child-range receipt and A00 resolution receipt; a missing, duplicate or
      additional member blocks the exit.

A01 is blocked if the plan content changes after approval; revise, recompute and obtain a new A00
approval rather than reusing the locator.

## Task NHA-A01: Merge the approved design and planning branch

**Files:** no additional source edit.

- [ ] **Step 1: Prove the intended tracked set**

Run:

```powershell
git diff --name-status origin/main...HEAD
git status --short
git log --format='%H%n%B' origin/main..HEAD
```

Expected tracked content is the approved design commits plus these seven planning documents only; the
A00 approval is an empty commit. The user-owned
`docs/functional-correctness-verification-2026-07-15-results.md` and
`docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` remain untracked and
unstaged. The unclassified/generated `testResults.xml` also remains untracked, unmodified and unstaged.
Any product, workflow, current-evidence, GA-evidence or state path is `blocked/scope-drift`.

- [ ] **Step 2: Run planning verification**

```powershell
git diff --check origin/main...HEAD
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 -Check
```

Expected: both exit 0. This is document consistency, not assurance GREEN.

- [ ] **Step 3: Obtain independent plan review and merge**

The reviewer checks work-ID uniqueness, plan DAG, stale approval handling, exact file lists, rollback
and all NHR mappings. Merge without squash/rebase so both design approval commits remain discoverable
ancestors. A01 is the sole Packet-before-contract exception and is authorized only by A00's exact plan
approval; it cannot be reused. Record PR head, merge commit, merge method and both workflow conclusions.
After the verified A00 commit/exit exists, Dispatch Authority resolves the preapproved A01 deferred
parent once after reopening the byte-identical A01 root/parent and their original receipts. It signs,
publishes and reads back only the `approved_candidate_landing` child range and signed resolution record,
with one new §6.2 receipt for each, and proves its candidate, base/head/tree/change-set/merge method/CAS
equal this unchanged PR. Root recreation/digest drift, a current-child collision, cross-parent/root
substitution, or a missing/substituted resolver, output slot, child signature, child-range receipt or
resolution receipt blocks.
Immediately before the unchanged merge, append one create-only, two-domain-signed consume event at
`assurance-bootstrap://consumes/NHA-A01/<A00-PACKET-SURROGATE>/<consume-id>`, binding A00 request/
decision IDs, preapproved A01 root/parent receipts and resolver locator+digest, resolved child range and
resolution locators/digests/receipts, exact
PR head/tree and expected base. A01's exit records the Program §6 authorization as
`state=consumed` and binds this event; a stale, failed or uncertain merge blocks reuse and requires a
fresh A00 approval rather than another consume.

- [ ] **Step 4: Verify the post-merge ancestor**

From a fresh `main` checkout:

```powershell
git merge-base --is-ancestor 4031c50687414ebc0706441d441d93f6c6f06863 HEAD
git merge-base --is-ancestor dcae9b0d0050397fc1e5145e12bdd99414bfe654 HEAD
```

Expected: both exit 0. Failure blocks A02; do not recreate duplicate approval trailers.

**Exit artifact:**
`assurance-bootstrap://exits/NHA-A01/<target-tree>/<payload-digest>` with contract
`pcv-assurance-bootstrap-exit-v1`, exact main commit/tree, commands, raw hashes, actor signatures and CI
URLs. Its inventory requires exactly the reopened A01 root and parent-range receipts, newly created A01
child-range and resolution receipts, and the typed consume; publication without consume is never an
executable or completed A01.

## Task NHA-A02: Integrate the assurance amendment into stable design authority

**Files:**

- Modify: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-luna-completion-control-design.md`
- Modify: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`

- [ ] **Step 1: Freeze the authority delta**

The change must add one unique locator:

```text
Authority-Integration: purecvisor-luna-no-human-review-assurance-v1
```

It references the original `Design-Amendment` and written-spec approval commits by ancestry. It must
not copy either original approval trailer into the new commit.

Generate the six-outcome RED matrix locally and read-only first, then hash it. Before the first external
write or tracked edit, generate/approve/consume a Packet with `packet_type=trust_root`,
`phase=execution_authorization`, exact A01 post-merge start commit/tree, the two modify operations, local
matrix digest, commands, capabilities, exact conditional-create prefix
`assurance-bootstrap://reviews/NHA-A02/<A01-BASE-TREE>/` and exact
`assurance-bootstrap://candidate-verification/NHA-A02/` prefix, A00-pinned publisher/client digest, provider/
retention/readback receipt policy, risk and whole-task revert. Result commit/tree/gates are absent. After
consume, publish the unchanged local matrix, read back and validate its §6.2 receipt before the first
tracked edit. This decision cannot authorize landing.

- [ ] **Step 2: Add RED authority assertions before prose edits**

Verify the exact externally read-back RED artifact
`assurance-bootstrap://reviews/NHA-A02/<A01-BASE-TREE>/<red-run-digest>/authority-red-matrix.json` that currently fails
each item:

- stable owner table has no assurance design/canonical JSON/manifest/Packet/decision owner;
- stable activation formula permits activation without NHR environment completion;
- card contract permits directory `allowed_paths`, free-form acceptance or mixed result/evidence state;
- S/M may omit an independent verifier;
- schema/validator/workflow paths may remain M/Luna;
- PASS is not bound to exact actor, target, argv, environment and raw artifact digest.

Expected RED count: six.

- [ ] **Step 3: Amend the stable owner and card contracts**

Make these exact semantic changes:

1. Link the assurance design as the sole owner of NHR rules and name canonical
   requirements/acceptance/traceability JSON, immutable manifests, Packet and decision owners.
2. Add `spec_lock_sha256`, requirement/case refs, exact `path+create|modify|delete`, protected symbols,
   capability declarations, implementation/verification actor policy and expected artifact locators to
   ready-card requirements.
3. Separate implementation result commit, verification artifact, review attestation and state
   transition. A single result commit cannot self-assert final verification.
4. Force every trust-root path to `L/Sol/Release`; every required card gets an independent verifier;
   L/trust-root additionally gets independent Sol review and enforced CI.
5. Map assurance case/control/failure enums without merging them into existing Luna state fields.
6. Preserve exactly the five mutation categories and require category/host/artifact/command child
   approval and separate consume events. Aggregate campaigns remain non-executable.
7. Insert all seven hard gates, false-green controls, Packet invalidation and NHR-001..030 environment
   completion ahead of product activation/completion.

- [ ] **Step 4: Mark the amendment integrated without self-reference**

Change its authority status to `integrated` and cite the integration locator, not the integration
commit's own hash. Do not change the normative NHR text or claim materialization.

- [ ] **Step 5: Create and verify the immutable A02 candidate**

Run exact-path diff inspection, link validation, current-evidence `-Check` and `git diff --check`.
Independent Sol review must report all six RED assertions GREEN and predecessor-current unchanged.

Commit message: `docs: integrate assurance into Luna authority` with trailer
`Authority-Integration: purecvisor-luna-no-human-review-assurance-v1`. The commit is an immutable
candidate, not yet approved or mergeable. Run the bootstrap-grade actual Release protocol and publish
candidate-only verification at
`assurance-bootstrap://candidate-verification/NHA-A02/<candidate-tree>/<payload-digest>`.

- [ ] **Step 6: Approve and merge only the exact A02 candidate**

Generate a `trust_root` Packet in phase `landing_authorization` binding candidate commit/tree,
canonical change-set digest, six review results, raw Release artifacts, risks and rollback. Obtain an authenticated exact digest decision and
append a separate external consume event immediately before the approved merge action. Do not amend,
rebase or add an approval locator to the candidate after approval; any byte change requires a new Packet.

Open a separate PR from fresh main and preserve history. A03 starts only after post-merge Release and
ancestor checks pass. Publish the final
`assurance-bootstrap://exits/NHA-A02/<merged-tree>/<payload-digest>` with the execution and landing
decision/consume IDs, candidate/merge lineage, CI and ancestry evidence.

## Task NHA-A03: Revise the inactive successor to Plan-Revision v4

**Files:**

- Modify: `docs/superpowers/plans/2026-08-03-purecvisor-desktop-node-csharp-architecture-improvement-successor.md`

- [ ] **Step 1: Create the v4 review matrix**

Before editing, derive every v3 card ID, dependency, tier/model/lane, allowed path and acceptance owner as
a read-only local `v3-card-audit.json`. Hash it, but do not publish an external object yet.
Mark each item whose path or output is schema, oracle, validator, classifier, state guard, approval,
evidence/current projection, workflow, quality baseline, landing or completion logic as trust root.

RED fixtures must show at least:

- `LC-009` resolves as `M/Luna/Full`;
- `LC-001..005` can self-bootstrap/schema-validate under Luna;
- `LC-022..026` can land/activate without the new environment formula;
- directory paths and future artifact references are accepted;
- old v3/dbac approvals can be interpreted as active.

Before editing the successor file, generate/approve/consume a fresh Program §6.1 bootstrap request with
`packet_type=trust_root`, `phase=execution_authorization`, exact A02 post-merge start commit/tree, one
modify operation, v3 audit digest, exact conditional-create prefix
`assurance-bootstrap://reviews/NHA-A03/<A02-BASE-TREE>/` and exact
`assurance-bootstrap://candidate-verification/NHA-A03/` prefix, A00-pinned publisher/client digest, provider/
retention/readback receipt policy, planned v4 outcomes, commands, exact finalizers, risk and revert.
It has no future candidate or PASS fields and cannot authorize landing. Immediately after consume,
publish the unchanged local audit through the A00-pinned bootstrap writer, read it back and bind its
receipt before the first tracked edit.

- [ ] **Step 2: Publish one unique v4 locator**

Use:

```text
Plan-Revision: purecvisor-desktop-node-luna-successor-weekly-delivery-v4
```

Retain v3 locators as historical. Explicitly label the old materialization approval stale/unused.
Require A02's authority-integration locator to be a unique `main` ancestor.

- [ ] **Step 3: Replace the control-card contract and DAG**

Successor v4 must machine-specify:

- mapping from `NHA-*` work IDs to canonical card IDs;
- frozen spec/oracle ancestor before any implementation lease;
- exact path operation objects instead of directory strings;
- actor/capability/timeout/rollback and state-conditional artifact refs;
- separate result, verification, review and ledger transitions;
- independent verifier for S/M and separate Sol review for L/trust-root;
- trust-root auto-promotion to L/Sol/Release, including `LC-009`;
- fresh-main Packet digest approval before control-only materialization;
- NHR environment and live server enforcement before activation.

Re-audit and, where necessary, split all schema/validator/approval/evidence/workflow/current-state/
completion cards. Missing, duplicate, extra or cyclic canonical cards must be zero.

- [ ] **Step 4: Define the one-time pre-bootstrap Packet rule**

To avoid Packet-before-schema circularity, v4 defines exactly one exception:

- an external immutable artifact generated from the v4 request shape using two independently pinned
  RFC 8785 implementations;
- both canonical byte streams and SHA-256 results must agree;
- the request includes exact main commit/tree, design/integration/v4 refs, exact control-only paths,
  model-resolution evidence, `host_mutation=false`, exclusions, risk, expiry and rollback;
- only the A00-frozen interim authenticated channel may be the bootstrap decision source; its signed
  event must bind exact Packet ID/digest and later be imported as an immutable approval event after the
  Plan 2 validator exists;
- any drift makes it stale; this exception cannot authorize activation, product or mutation work.

- [ ] **Step 5: Commit and independently verify the immutable v4 candidate**

Commit the proposed v4 without an approval trailer. Run the bootstrap actual Release protocol and a
second clean Sol review. Generate a semantic diff artifact containing changed card IDs/DAG, all
trust-root promotions, removed permissions, completion formula and bootstrap exception. Publish
candidate-only verification at
`assurance-bootstrap://candidate-verification/NHA-A03/<candidate-tree>/<payload-digest>`.

- [ ] **Step 6: Obtain an exact v4 decision and merge unchanged**

Generate two distinct Packets over the same immutable candidate: `spec_revision` authorizes the exact v4
authority revision, and `trust_root` in phase `landing_authorization` authorizes only landing that reviewed candidate.
Each binds commit/tree, change-set digest and evidence and receives its own approval event. Validate and
consume the `spec_revision` once in A03's zero-operation, decision-consume-only artifact range and publish/
read back its exact consume receipt; that range cannot merge. Then the separately resolved
`verified_candidate_landing` range opens that receipt, proves candidate/tree/change-set equality and
consumes only the `trust_root` landing decision immediately before the single merge action. Neither
authorizes materialization. Candidate amendment/rebase invalidates both decisions; a Packet can never
carry two `packet_type` values or one range's consumer.

- [ ] **Step 7: Verify and merge A03**

Run an independent static validator over card/DAG/path/model/approval rules, plus current-evidence
`-Check` and `git diff --check`. Expected:

- trust-root misroutes: 0;
- missing independent verifier policies: 0;
- open-ended directory operations: 0;
- active references to v3/dbac approval: 0;
- predecessor remains current; state/materialization files remain absent.

Commit message is `docs: revise Luna successor for assurance v4`; approval remains a separate immutable
event rather than a mutable locator added to the approved candidate. Merge as its own PR and capture
exact post-merge main. Publish
`assurance-bootstrap://exits/NHA-A03/<merged-tree>/<payload-digest>` with all three decision/consume
events (execution, `spec_revision`, landing), candidate/merge lineage, CI and ancestry.

## Task NHA-A04: Align derived guidance and weekly projections

**Files:**

- Modify: `docs/CODING_GUIDE.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-weekly-service-development-spec.md`
- Modify: `docs/DEVELOPER_INDEX.md`

- [ ] **Step 1: Add cross-document RED assertions**

Generate a read-only local `derived-alignment-red.json` and assert that all
four derived documents currently reference v4 integration/revision, independent S/M verification,
trust-root Sol routing, five child approval categories, false-green gaps and current RED/blocked
activation. Expected: failures before edits.

Before Step 2 changes a tracked file, generate/approve/consume a Program §6.1 bootstrap request with
`packet_type=trust_root`, `phase=execution_authorization`, exact A03 post-merge start commit/tree, four
modify operations, local RED-matrix digest, exact conditional-create prefix
`assurance-bootstrap://reviews/NHA-A04/<A03-BASE-TREE>/` and exact
`assurance-bootstrap://candidate-verification/NHA-A04/` prefix, A00-pinned publisher/client digest, provider/
retention/readback receipt policy, deterministic projection commands, exact finalizers, risk and whole-
commit revert. It cannot authorize candidate landing. Immediately after consume, publish/read back
the unchanged RED matrix through the A00-pinned bootstrap writer before the first tracked edit.

- [ ] **Step 2: Regenerate derived policy**

- `CODING_GUIDE.md` teaches frozen-oracle TDD, exact scope, no self-verification, model routing and
  Packet/mutation boundaries.
- `DEVELOPMENT_VERIFICATION_POLICY.md` distinguishes planned selection from actual Full/Release PASS,
  documents the current public-boundary/Pester/server gaps as RED, and does not edit the generated
  current-evidence block.
- The weekly spec binds v4 source/approval. Recompute its row/DAG digest from v4; never copy v3's
  digest. Product-week rows may remain only when the recomputed graph proves them unchanged.
- `DEVELOPER_INDEX.md` adds v4 navigation and NHR current RED status while continuing to point to the
  predecessor as effective current.

- [ ] **Step 3: Verify and commit the deterministic A04 candidate**

Cross-check v4 locator, source refs, card counts, weekly rows/digest and approval categories in all four
files. Run current-evidence `-Check` twice and `git diff --check`.

Commit: `docs: align development guidance with assurance v4`. Run bootstrap actual Release and a second
clean Sol review. Product, workflow, state and evidence diffs must be zero. Publish candidate-only
verification at
`assurance-bootstrap://candidate-verification/NHA-A04/<candidate-tree>/<payload-digest>`.

- [ ] **Step 4: Approve and merge only the exact A04 candidate**

Generate a `trust_root` Packet in phase `landing_authorization` over the candidate commit/tree, four-file
change-set digest, recomputed weekly digest, evidence and rollback. Obtain/consume the exact decision
immediately before merge. Merge the unchanged derived-only candidate and publish
`assurance-bootstrap://exits/NHA-A04/<merged-tree>/<payload-digest>` with both decision/consume events,
candidate/merge lineage, CI and ancestry.

## Task NHA-A05: Prepare and decide the exact fresh-main Plan 2 trust-root bootstrap Packet

**Files:** no repository implementation or state edit in this task.

- [ ] **Step 1: Freeze fresh main and model resolution**

Record exact A04 post-merge main commit/tree. Prove the UI selector aliases resolve to canonical
`gpt-5.6-luna` with reasoning `max` for later S/M work and canonical `gpt-5.6-sol` with reasoning
`ultra` for Plan 2 trust-root work; otherwise stop before artifact creation.

- [ ] **Step 2: Generate the one-time request envelope independently**

First generate locally, validate and obtain through the interim channel a distinct A05 artifact-only
Program §6.1 request with `work_id=target_work_id=NHA-A05`,
`purpose=bootstrap_artifact_publication`, `packet_type=trust_root`,
`phase=execution_authorization`, `operations=[]`, exact A04 main, read-only validation commands, finalizer
argv and only the A05 task-dispatch/range, planned descriptor, their three §6.2 receipts, plus request/decision/
consume/exit output prefixes.
It binds the signed A05 dispatch/range and its pre-request planned-command descriptor, A00-pinned publisher,
provider, retention, separate readback principal, conditional create and abort/reconciliation. Consume it
once, then publish/read back the A05 root dispatch, selected range and descriptor with three separate receipts before the first
other publisher side effect. Its provider-signed request/approval/consume
events and §6.2 readback receipts are inventory members; it cannot authorize T01 execution.

Two separate Sol actors use the two v4-approved RFC 8785 tools to canonicalize the same `trust_root`
Packet `request_payload` with `work_id=NHA-A05`, `target_work_id=NHA-T01`,
`purpose=successor_execution_handoff` and phase `execution_authorization`. Both outputs must be byte-identical and
hash-identical. The request's exact include set is T01 only: `global.json`, `.node-version`, toolchain
schema/record, fourteen named product `packages.lock.json` files, the two workflows, and the exact T01
toolchain fixtures/test plus current workflow-pin assertion test files named by Plan 2. It records
`workflow_change=true` and `current_evidence_control_change=false` while preserving the exact
operational tuple. Exclusions include T02–T08, all `src/**` product behavior, `web/src/**`, installer/
package output, effective-current pointer, GA/current-evidence changes and every host mutation
capability. T02–T08 each require a fresh-main Packet and decision after the preceding task's final exit.
The handoff prebinds T01's exact candidate-verification publisher finalizer/prefix/provider/retention/
readback; its separately generated T01 landing request prebinds the final exit publication.
Before that handoff request is presented, independently sign/validate T01's exact dispatch/range, allocate
its immutable bootstrap URI and use the A00-pinned descriptor generator/two validators to generate the T01
planned-command descriptor at a preallocated content-addressed URI; bind all three digests in the
handoff. Do not publish the T01 dispatch or consume the T01 decision in A05. Plan 2 T01 consumes that
decision and its first mandatory finalizer conditionally publishes/readbacks the T01 root dispatch, selected
range and descriptor with three separate receipts before RED or the first tracked write.

- [ ] **Step 3: Independently inspect the Packet and request a decision**

Present Packet ID, request digest, every exact Plan 2 path/operation, includes/excludes, risks, proof,
expiry and rollback through the A00-frozen interim channel. Accept only the exact three-command grammar
from its immutable allowlisted principal and verify the provider signature/OIDC, nonce, expiry and
revocation. Store that authenticated decision event and immutable request in the external bootstrap
artifact through the consumed A05 artifact-only authority; do not consume the T01 decision and do not
modify the request to add approval state.

- [ ] **Step 4: Close Plan 1 without materializing**

Exit only when A01–A04 commits are unique main ancestors, post-merge CI refs exist, the predecessor is
still current, v3/dbac decisions are stale, the bootstrap envelope has two matching digests, and the
fresh decision is unconsumed. Plan 2 consumes it before T01's first write; T08 later emits only an
external import-candidate for the original event, Plan 4 E05 finalizes the complete inventory, and E06
alone performs the separately authorized canonical import after storage/notary implementation. Publish
`assurance-bootstrap://exits/NHA-A05/<A04-main-tree>/<payload-digest>` binding the unconsumed T01
request/decision, exact T01 operation manifest, A05 artifact-only request/approval/consume and two
validator/canonicalizer results. T01 landing uses a separate `work_id=NHA-T01` bootstrap request; T02
execution and landing each use a separate `work_id=NHA-T02` bootstrap request. Before T03 execution, only
the T02-merged canonical schema plus A00-pinned external validators apply; T03 candidate verification
cross-checks the new locked tool, and T03 post-merge is the mandatory repository-tool cutoff for T04+.

## Rollback and stop rules

- Before activation, each document PR can be reverted as a whole; do not partially restore v3 active
  authority. A revert returns authority integration to pending and invalidates all later Packets.
- A failed or ambiguous authority review is `blocked/spec-defect`, not an editing opportunity inside an
  implementation PR.
- Any change to design, v4, derived policy, exact main, risk or T01 bootstrap scope invalidates A05.
- No package, service, binding, TLS, VM, installer or Hyper-V command appears in this plan's execution.

## Plan 1 exit gate

- [ ] A00 exact seven-plan approval commit exists and A01 approved design/approval commits are exact
      `main` ancestors.
- [ ] A02 stable owner integration is merged and independently reviewed.
- [ ] A03 v4 is separately approved, merged, acyclic and trust-root-correct.
- [ ] A04 derived documents deterministically match v4.
- [ ] A02 and A04 each used distinct execution and landing `trust_root` Packets/decisions; A03 used a
      pre-execution `trust_root` plus distinct post-verification `spec_revision` and landing
      `trust_root` Packets/decisions over the same unchanged candidate.
- [ ] A05 fresh-main Plan 2 trust-root request has two agreeing canonical digests, the exact T01 path
      set and a valid unconsumed decision; its separate zero-operation artifact-only authority was consumed
      exactly once and T02–T08 are explicitly excluded pending fresh decisions.
- [ ] Predecessor remains current; materialization/state/product/workflow/current-evidence/host changes
      are zero.
- [ ] `required_enforced=false` and `overall_readiness=red` remain visible until Plan 5 proves otherwise.
