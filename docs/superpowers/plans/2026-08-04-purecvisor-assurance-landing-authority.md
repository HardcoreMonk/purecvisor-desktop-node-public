# PureCVisor Server Landing Authority Implementation Plan

> **Status:** proposed child plan 5; implementation can run in shadow mode after Plan 4, but live exit
> requires a separately approved hosting/enforcement choice.
>
> **Execution class:** policy, provider, decision, equivalence and landing components are trust-root `L`,
> `gpt-5.6-sol`, `ultra`, actual `Release`, with a different trust-domain Sol verifier.

**Goal:** Make protected-branch landing possible only through server-enforced required gates, latest-base
serialization, digest-bound decisions and independently attested candidate/final equivalence.

**Architecture:** A provider-neutral landing policy is projected to GitHub rules. A trusted Landing
Authority reads live provider state, computes `required_enforced`, validates the immutable Packet and
decision, rechecks the exact latest-base candidate and consumes the decision once. A final independent
job attests the merged tree. No PR-supplied boolean, bot summary or ordinary green workflow can grant
landing eligibility.

**Prerequisite:** Plan 4's artifact/notary/Packet/Dashboard exit attestation is valid and fresh. The exact
fresh-main digests for Plan 2's authority-reservation/horizon/pair-state schemas and client tools plus Plan
4 E05's canonical Decision-Plane/terminal tools are immutable inputs to L06 and L08. Missing, stale or
mismatched digests block those tasks; neither may substitute a local state engine.

**Source design:** `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`
§14 and NHR-019..023, NHR-028..029.

---

## Current provider fact and required user choice

At planning time the repository is private and owned by the personal account `HardcoreMonk`; the
ruleset API returns HTTP 403 with an upgrade message. Therefore live enforcement is unavailable now.

Before L02, the user must approve one exact provider/identity selection Packet. It selects requirements
only and never authorizes repository or provider mutation:

- **Native queue mode:** transfer to an organization on GitHub Enterprise Cloud and use GitHub's native
  merge queue. GitHub documents private merge queues as available to organizations using Enterprise
  Cloud: [Managing a merge queue](https://docs.github.com/en/enterprise-cloud@latest/repositories/configuring-branches-and-merges-in-your-repository/configuring-pull-request-merges/managing-a-merge-queue).
- **Equivalent serialized mode:** enable private-repository rulesets on an eligible plan and operate an
  independently hosted Landing Authority that serializes one latest-base candidate at a time. The live
  canaries must prove direct push/bypass is blocked and the external lease is exclusive. GitHub's
  ruleset availability is documented in [About rulesets](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets).

Public visibility is outside this plan. A plan purchase, repository transfer, collaborator invitation,
GitHub App installation or ruleset mutation is an external administrative change. Each actual change
later requires a new exact-main Packet with `packet_type=trust_root` and
`phase=execution_authorization` after L03–L06 tools are
merged. If neither mode is chosen and proven, remain shadow/RED and do not start Plan 6 activation.
The only pre-L06 administrative exception is L04's own authenticated decision-channel setup and atomic
cutover after the L04 adapter is already merged and shadow-verified. That exception may provision only
the selected decision App/environment/credential and replace the A00 interim input authenticator; it
cannot transfer the repository, purchase/enable enforcement, grant CODEOWNER access, install the Landing
App, change rules/checks/queue/lease policy or make `required_enforced=true`. All such landing/provider
enforcement changes remain forbidden until merged L03–L06 tools and L07 authority.

## File map

**Provider-neutral policy**

- Create `.github/assurance/landing-policy.schema.json`.
- Create `.github/assurance/landing-policy.json` only after L02 freezes real provider/App/owner
  identities; L01 creates no concrete policy record.
- Create `.github/assurance/main-ruleset.schema.json`.
- Create `.github/assurance/main-ruleset.json` only after the provider decision fixes exact identities.
- Create `.github/CODEOWNERS` only after a real independent owner identity is verified.
- Create `.github/pull_request_template.md` as informational UX; it is never enforcement.

**Live provider and policy tools**

- Create `packaging/windows-desktop-node/tools/PcvGitHubLandingAuthority.psm1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvGitHubLandingAuthority.ps1`.
- Create `packaging/windows-desktop-node/tools/Set-PcvGitHubLandingPolicy.ps1` with mandatory PlanOnly
  default and digest-bound `-ApplyDecisionPath` for actual changes.
- Create `packaging/windows-desktop-node/tests/PcvGitHubLandingAuthority.Tests.ps1`.
- Provider API fixtures are deterministic in-memory cases inside
  `packaging/windows-desktop-node/tests/PcvGitHubLandingAuthority.Tests.ps1`; no fixture file or directory
  is created.

**Authenticated decision and candidate equivalence**

- Modify Plan 4's `PcvAssuranceDecisionRecord.psm1`, add/consume wrappers and exact
  `packaging/windows-desktop-node/tests/PcvAssuranceDecisionRecord.Tests.ps1` for live provider authentication.
- Create `.github/workflows/assurance-decision.yml` and
  `packaging/windows-desktop-node/tests/PcvAssuranceDecisionWorkflow.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvLandingCandidateEquivalence.psm1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvLandingCandidateEquivalence.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvLandingCandidateEquivalence.Tests.ps1`.

**Landing workflow and post-merge proof**

- Create `.github/workflows/assurance-landing.yml` and
  `packaging/windows-desktop-node/tests/PcvAssuranceLandingWorkflow.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceGitHubPairState.psm1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceGitHubPairState.Tests.ps1` as the GitHub provider
  CAS/readback adapter behind Plan 2/E05; these files contain no second decision or pair-state engine.
- Modify Plan 2's two existing workflows only where live queue/provider facts require exact triggers.
- Create `packaging/windows-desktop-node/tools/New-PcvLandingEnforcementAttestation.ps1` and
  `packaging/windows-desktop-node/tests/PcvLandingEnforcementAttestation.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvPostMergeAttestation.ps1` and
  `packaging/windows-desktop-node/tests/PcvPostMergeAttestation.Tests.ps1`.

All these paths are protected before implementation.

## Normative task-dispatch matrix

Each row uses the Program §5.1 canonical Test, Red and Final argv with its exact work ID. The signed
dispatch expands every static File-map/fixture entry to exact paths and complete argv; provider-only rows
bind exact APIs/resources/ETags and `operations=[]`. Final is actual Release plus a different-trust-domain
Sol verifier, or the same independent actor/readback discipline for no-tracked-change rows.

| Work ID | Ordered path/range closure | RED or allowed N/A | Implementation/finalizer boundary | Rollback | Commit/PR boundary |
|---|---|---|---|---|---|
| NHA-L01 | `exact_paths`, policy/ruleset schemas and exact abstract provider fixtures | 403/404/bypass/check/queue/owner corpus RED | schemas and fixture oracle only; no concrete identity record | whole-commit revert | `tracked_pr` |
| NHA-L02 | `artifact_only`, exact selection request/readback/projection prefixes | `red_not_applicable`: no implementation; DENY/no decision/placeholder identity is named fail-closed oracle | authenticated choice and immutable projection only; no provider/tracked mutation | invalidate selection; preserve events | `artifact_only_no_commit` |
| NHA-L03 | `exact_paths`, live provider module/wrapper/test and exact fixture entries | partial/spoofed/drifted provider responses RED | read-only evaluation and PlanOnly projection only | whole-commit revert | `tracked_pr` |
| NHA-L04 | ordered `exact_paths` decision adapter/workflow range, conditional separate `provider_administration` channel-setup/readback range, then a distinct fresh-decision `provider_administration` cutover/revoke range | forged/replay/overlap/fallback/revoked-channel corpus RED | merged adapter first; setup must close before cutover; neither external decision substitutes for the other | conditional setup rollback before cutover; after cutover fail closed | `tracked_pr`; both external ranges `artifact_only_no_commit` |
| NHA-L05 | `exact_paths`, equivalence module/wrapper/test entries | base/head/tree/change-set/lease/fencing/CAS one-field mutation RED | candidate equivalence proof only; no merge/provider change | whole-commit revert | `tracked_pr` |
| NHA-L06 | `exact_paths`, exact landing workflow/tests, GitHub pair-state CAS/readback adapter/test, two trigger edits, policy/ruleset/CODEOWNERS/template and attestation tools/tests | PR-controlled decision, wrong candidate, stale owner/epoch, two-winner race, missing check and shadow-as-enforce RED | shadow-only workflow, E05-backed provider adapter and desired immutable policy records | whole-commit revert | `tracked_pr` |
| NHA-L07 | ordered one or more `provider_administration` apply/grant/App/ruleset ranges, all `operations=[]` | merged-main PlanOnly plus stale/wrong decision/before-state/ETag fixtures must reject before apply | exact CAS apply, audit/readback/final attestation only; no tracked edit | separately approved exact old-state restore or recovery blocked | `artifact_only_no_commit` |
| NHA-L08 | initial `artifact_only` preflight/root publication; static setup rollback→fresh reservation→fresh setup forward; each candidate is a distinct deferred `exact_paths` child; each probe resolves actual plan→four-output bundle, approves rollback, consumes fresh reservation, resolves stage-two attempt and consumes fresh attempt claim; expected-reject/pre-attempt/verified-landing use fresh release claims, pending cancel uses rollback claim; valid expendable landings and conditional recovery are separate; setup cleanup→fresh setup release→artifact-only exit | wrong lineage/receipt/horizon/owner/epoch, race loser, unexpected transition and live forbidden attempts reject; rejection candidates have zero landing authority | E05 is sole state engine; L06 adapter supplies GitHub CAS/readback only; provider ranges cannot create commits or target main | paired rollback, winner-only release/cancel, reconciliation on uncertainty | provider/preflight/exit ranges `artifact_only_no_commit`; candidates `candidate_commit_no_merge`; valid expendable landings `tracked_pr` |
| NHA-L09 | `artifact_only`, exact provider/decision/cutover reads and Dashboard/exit prefixes | replay, identity drift, stale ETag, old-channel event and split-brain evidence reject | read-only requery/projection and L09 exit only | invalidate attestation; no mutation | `artifact_only_no_commit` |

## Task NHA-L01: Freeze provider-neutral policy and adversarial API corpus

**Files:** policy/ruleset schemas and abstract API fixtures. No concrete policy/ruleset/CODEOWNER record
is created before L02.

- [ ] **Step 1: Define the seven check identities**

The required assurance check names are exactly:

```text
spec-contract
scope-integrity
product-verification
independent-verifier
quality-ratchet
security
artifact-attestation
```

Both native and equivalent modes also require the provider-control check `landing-authorization` from
the exact Landing Authority App/integration. It is not an eighth assurance gate and cannot affect
assurance color; it remains pending/failed until an exact candidate-bound authenticated decision is
valid and all seven gates are green. Native queue cannot merge merely because the seven gates passed.

The policy schema requires existing product checks, expected GitHub App/integration IDs, trusted
workflow content digests, target `main`, PR-only, force/delete prohibition, stale-dismissal, latest-base
mode, review requirements, covered roles and zero bypass. L01 fixtures use explicit synthetic identities
marked as fixture-only; L02 supplies every real identity. Unknown identity/digest is invalid, and no
placeholder or fixture identity may enter `landing-policy.json`.

Native mode is deliberately single-PR only. The policy/schema fixes
`max_entries_to_build=1`, `min_entries_to_merge=1`, `max_entries_to_merge=1`,
`grouping_strategy=ALLGREEN` and `merge_method=MERGE`, plus explicit approved integer
`check_response_timeout_minutes` and `min_entries_to_merge_wait_minutes` values; defaults and caller
overrides are invalid. A multi-PR constituent manifest is not implemented by this program, so any
provider setting/event with zero or more than one constituent is invalid and keeps
`required_enforced=false`. Equivalent mode already serializes exactly one PR candidate per lease and
also freezes `merge_method=MERGE`: its least-privileged merge call must preserve the approved result
commit as an ancestor. Squash, rebase, caller-selected merge methods and a provider default are invalid
in both policy and runtime projection.

- [ ] **Step 2: Freeze RED provider fixtures**

Create fixtures for HTTP 403/404, missing rule, missing check, wrong integration, spoofed same-name check,
admin/user/team/App bypass, force/delete allowed, stale review retained, latest-base off, queue absent,
any wrong/missing native queue parameter above, multi-PR group, ruleset changed after attestation,
CODEOWNER absent/unresolvable, personal-owner ambiguity and partial API response.

- [ ] **Step 3: Validate externally and commit the oracle**

Every invalid fixture has one expected stable code; one synthetic valid fixture passes. Commit
`test: freeze Landing Authority provider policy` before implementation.

## Task NHA-L02: Obtain provider, mode and independent-owner selection approval

**Files:** immutable Decision Packet/decision events only; no tracked repository or provider mutation.

- [ ] **Step 1: Present the deployment Packet**

Include current 403 evidence, native/equivalent options, recurring cost, repository ownership/visibility,
required identities, App permissions, independent CODEOWNER candidate, failure recovery and rollback.
The Packet must distinguish ruleset availability from private merge-queue availability.

- [ ] **Step 2: Verify real identities**

Do not insert placeholder users/teams. Verify the chosen independent reviewer's immutable account/team
identity, owner and eligibility and prove it is not the implementation actor. Query current repository
access and record `access_status=present|deferred` plus the exact required permission. `deferred` is
allowed only because L07 owns the separately authorized grant and readback; L02 performs no grant and
cannot treat deferred access as enforcement. Verify the Landing App's owner, intended installation,
key/OIDC subject and least privileges. Executor/verifier credentials are never reused.

- [ ] **Step 3: Freeze exact provider projection**

Generate a local, unpublished projection candidate with repository ID, branch/ruleset target, required
check integration IDs (including `landing-authorization`), review/CODEOWNER settings, bypass set
(empty), force/delete restriction, latest-base/queue configuration, independent-owner access status and
the exact deferred grant when applicable. `access_status` is observed mutable provider state, not a
tracked policy field: L06 materializes only the desired immutable identity and required permission. For
native mode, freeze all seven named queue parameters above, including exact timeout/wait integers. For
equivalent mode, freeze the external lease/store endpoint, exclusive-lock semantics, exact merge API and
`merge_method=MERGE`. Hash it locally; Step 3 performs no external write. L06 later materializes
repository files from the stable selected fields.

- [ ] **Step 4: Consume the selection decision without applying**

Use `packet_type=requirements_approval`, `phase=planning_authorization`,
`purpose=landing_provider_mode_owner_selection`. The decision binds repository
ID/visibility/owner, selected plan/mode/provider, App, CODEOWNER, canary boundary, cost and rollback.
Consume it once to freeze the immutable selection record; it cannot be reused for an App install,
repository transfer, ruleset apply or landing.
After that consume, create/approve/consume a distinct artifact-only
`trust_root/execution_authorization` Packet binding the exact selected local projection digest, project
main, publisher/tool argv, output prefix/provider/retention, conditional-create/readback/notary, expiry and
abort/reconciliation with zero tracked/provider-admin/host mutation. Publish and independently read back
the unchanged projection only under this authority. L03 requires both the selection consume and
publication authority/receipt; neither can substitute for the other.

## Task NHA-L03: Implement live provider state evaluation

**Files:** GitHub Landing Authority module, test wrapper and fixtures/tests.

- [ ] **Step 1: Run the L01 corpus as RED**

Existing tools cannot calculate the complete provider truth. Preserve failures.

- [ ] **Step 2: Query authoritative APIs**

With read-only admin metadata permission, collect repository ID/owner type/visibility/default branch,
rulesets/branch protection, required checks and expected source integration, bypass actors, merge methods,
all native queue parameters, latest-base settings, installed App and CODEOWNERS resolution. Pin API
version and record response digests, ETags and collection time.

- [ ] **Step 3: Derive `required_enforced`**

The evaluator accepts provider response objects, never caller `required_enforced`. Any 403, missing/
unknown/partial field, bypass, wrong App/workflow digest, stale ruleset or unresolvable CODEOWNER yields
false. Any native queue parameter differing from the exact frozen values also yields false. Return per-
For equivalent mode, any merge API/method other than the frozen commit-preserving `MERGE`, or inability
to prove the approved result commit remains an ancestor, also yields false. Return per-rule facts and
stable failure codes.

- [ ] **Step 4: Verify and commit L03**

All L01 invalid fixtures return false; synthetic valid returns true. A live read on the current baseline
returns false/403 and Dashboard remains RED. Commit `feat: attest live GitHub landing policy`.

## Task NHA-L04: Authenticate decision events through a trusted channel

**Files:** decision module/wrappers/tests and `assurance-decision.yml`.

- [ ] **Step 1: Add live authentication RED cases**

Reject PR comments, edited/deleted comments, unallowlisted actor, actor name without immutable ID,
fork event, missing signed event payload, wrong repository/environment, nonce replay, expiry and
category/scope/target drift.

- [ ] **Step 2: Implement authenticated dispatch**

Recommended GitHub adapter uses `workflow_dispatch` from an allowlisted immutable user ID in a protected
environment and accepts the exact decision command plus nonce. The workflow on trusted default-branch
content validates the Packet, signs/notarizes the provider event and appends it to Plan 4's immutable
store. An equivalent adapter must prove the same identity/channel/signature properties.

- [ ] **Step 3: Keep decision and consume separate**

This workflow never merges or mutates a host. The Landing Authority consumes a valid decision later and
appends its own target-bound event. Editing repository JSON cannot forge either event.

- [ ] **Step 4: Verify and shadow-land L04**

All unauthenticated/replay plus pre/post-cutover wrong-channel, overlap, fallback and revoked-interim
fixtures reject. Commit `feat: authenticate assurance decisions`, use the still-current interim channel
for L04's execution/landing decisions, merge unchanged and post-merge shadow-test the protected adapter.
It accepts no authoritative event before the next step's durable cutover.

- [ ] **Step 5: Authorize and perform an atomic channel cutover**

Only from the exact L04 post-merge main, through E05 and the still-current A00 interim channel, create/
approve/consume a provider-setup `packet_type=trust_root`, `phase=execution_authorization` Packet when
setup is needed. It binds only the new provider/App/environment/principal/issuer/key provisioning,
credential custody/rotation, cost, before-state, independent readback and conditional rollback. Complete
that transaction and seal its immutable readback/revocation facts first; neither tracked L04 authority
nor L02 selection authorizes it.

Then create/approve/consume a new cutover-only `trust_root/execution_authorization` Packet binding the
exact old profile, new immutable setup/readback digest, no-op canary, cutover sequence, old-permission
revocation, failure oracle and reconciliation. It forbids App/environment/credential provisioning and
any unrelated provider mutation. A setup decision cannot be consumed as cutover authority, and a
cutover decision cannot repair or complete setup.

Append a create-only cutover record only after both an independent read of the old profile and an
authenticated no-op event through the new profile pass. The record fixes one cutover instant/sequence:
events before it validate only under the interim profile and events after it validate only under the new
profile. Revoke the interim channel's decision permission immediately after durable cutover readback. No
overlap, dual-active window, fallback or silent re-enable is allowed. Failure before the durable cutover
keeps the old channel current; uncertainty after it blocks decisions until a separately approved
reconciliation, never automatic rollback. The cutover transaction enumerates every interim approval that
has no consume event in the immutable cutover record; it does not append a new decision event or edit those
approvals. The verifier rejects any old-profile consume at or after the cutover sequence even when original
expiry has not elapsed. Required work receives a fresh Packet/approval through the new channel. Historical
interim events remain signature-verifiable. Fixtures race an unconsumed old approval against the cutover CAS
and require exactly one outcome: a fully read-back pre-cutover consume, or cutover readback listing that
still-unconsumed approval and permanently rejecting its consume. There is no pair-scoped
`terminal_cancellation` for channel cutover.

## Task NHA-L05: Prove latest-base candidate equivalence

**Files:** equivalence module/wrapper/tests.

- [ ] **Step 1: Freeze temporary Git DAG cases**

Cover unchanged base, unrelated base advancement, conflict resolution, approved blob changed, path/mode/
operation changed, risk/capability/rollback changed, missing rerun check, wrong queue entry, multiple PR
composition and final merged tree not descended from attested candidate.

- [ ] **Step 2: Recompute the approved change set**

Using trusted Git objects, compare approved implementation head/change-set to the queue or serialized
candidate relative to its new base. Preserve exact path/blob/mode/operation without conflict resolution.
Re-evaluate requirement, user-visible behavior, capability, risk and rollback digests.

- [ ] **Step 3: Require latest candidate checks and lineage**

Every required check must rerun on the exact candidate through its expected integration/workflow digest.
Bind queue entry or external lease, candidate commit/tree and eventual merged commit/tree in a signed
landing-equivalence attestation. Temporary and final SHAs need not equal, but approved content and
lineage must be proven.

- [ ] **Step 4: Invalidate decisions correctly**

Pure base advancement may preserve the earlier implementation approval only when every equivalence field
passes, but the final landing authorization is a separate post-verification `landing_attestation` Packet
bound to the newly built exact candidate tree. Any implementation/change-set/spec/evidence/risk/workflow/oracle/capability/rollback
change makes the relevant decision stale and requires a new Packet. Commit
`feat: attest latest-base landing equivalence`.

## Task NHA-L06: Implement and shadow-land the sole Landing Authority workflow

**Files:** `assurance-landing.yml`, workflow tests, exact changes to existing workflow triggers,
`PcvAssuranceGitHubPairState.psm1`, its exact test, `Set-PcvGitHubLandingPolicy.ps1`,
`landing-policy.json`, `main-ruleset.json`, `.github/CODEOWNERS` and `.github/pull_request_template.md`.
All are implemented, PlanOnly-tested, trust-root-landed and
post-merge attested before any provider mutation.

- [ ] **Step 1: Add RED workflow trust cases**

Reject PR-writable workflow, caller-supplied check booleans, wrong event, non-latest target, same actor as
executor/notary, missing Packet/decision/access proof, valid authenticated APPROVE while any assurance
gate is RED, `--admin`, direct push, absent post-merge job and same-name spoofed checks. The adapter corpus
also rejects stale owner/epoch/fencing token, sibling-key substitution, ETag drift, duplicate claim and both
branches claiming the same pair.

- [ ] **Step 2: Implement evaluate-only stages**

Fetch immutable Packet/decision/attestations; query live provider; derive Dashboard axes; verify exact
candidate, seven gates, product checks, actor separation, artifact freshness, rollback and decision.
Any unknown is blocked. No stage accepts a PR-authored success summary. A correctly authenticated
APPROVE decision is necessary but never sufficient: if `assurance_verdict != green` or any required
check is failed/missing/stale, landing remains blocked. This is the explicit NHR-021 oracle.

`PcvAssuranceGitHubPairState.psm1` implements only GitHub provider CAS/ETag/readback operations behind
Plan 2's contracts and E05's canonical `consume_and_claim`; it cannot append decisions, define transitions
or return authority on its own. L06 uses deterministic in-memory/fake adapters only and requires E05 to
select exactly one winner while preserving the loser with zero provider side effect. The first live
GitHub CAS/readback exercise is L08 under its separate provider decisions; L08 may use only this merged
adapter.

Every required-check producer workflow, including existing product checks and Plan 2–4 assurance
checks, implements both closed provider-neutral target entries. Native mode handles `merge_group` with
type `checks_requested`, derives base/head from the server event, requires exactly one constituent PR
whose immutable number/head match the approved input, and checks out only that queue candidate.
Equivalent mode accepts only the signed `serialized_landing_candidate` target issued after Landing
Authority acquires its external CAS lease/fencing token, constructs one latest-base candidate and
Verification Authority independently re-resolves provider objects. A PR-head result cannot be reused
for either candidate. The serialized record also binds the exact commit-preserving merge API and
`merge_method=MERGE`; squash/rebase or a method omitted for provider defaulting rejects. Static fixtures
enumerate every producer and fail when a selected mode lacks its exact target behavior or accepts the
other mode's fields.

For the independent verifier producer, the protected Plan 2 target issuer first creates the signed
`issuer_class=provider_event_oidc` merge-group target. Landing Authority then invokes Plan 3's
`New-PcvVerificationDispatch.ps1 -ProviderEventTargetPath` branch, followed by
`Submit-PcvVerificationAuthorityDispatch.ps1` and
`Receive-PcvVerificationAuthorityRun.ps1`. This branch derives the candidate only from the authenticated
server event and deliberately has no model-result/execution-manifest/result-transport input. The
provider delivery receipt, exact workflow/tooling commit, remote run receipt and artifact manifest must
all bind the merge-group commit/tree before the independent-verifier required check can succeed.
This paragraph is native-mode only. In equivalent mode, Landing Authority instead submits the exact
lease/latest-base candidate record to Verification Authority, which produces
`issuer_class=verification_authority`, `authority_source=serialized_landing_candidate`; Plan 3's
`New-PcvVerificationDispatch.ps1 -SerializedLandingCandidatePath` and the same Submit/Receive chain run
all checks against that commit/tree. Missing/stale lease, fencing token, CAS receipt, provider readback
or latest-base equality blocks before dispatch.

- [ ] **Step 3: Serialize and consume**

For both modes, the expected Landing App first publishes `landing-authorization=pending` for the exact
candidate/check suite; only this App can later publish success. Native mode adds only the exact approved
PR to merge queue, then waits for the server-produced candidate before generating/consuming its
candidate-bound `landing_attestation` authorization. The queue cannot merge while the provider-control
check is pending, including when an ordinary actor enqueues. Equivalent mode first acquires a
provider-external exclusive lease using compare-and-set and a monotonically increasing fencing token,
builds one latest-base candidate, reruns checks, and generates the exact candidate
`landing_attestation` Packet. It may
proceed only after an authenticated, unexpired, unconsumed candidate decision and an expected-source
required `landing-authorization` check both bind that candidate tree and fencing token. Lease expiry,
process crash, stale holder, split brain, token rollback or compare-and-set conflict blocks and requires
reconciliation/new candidate; a stale holder cannot publish or merge. Revalidate everything after lease
acquisition and immediately before consume. Append decision consume and landing-start events before the
least-privileged merge API. In equivalent mode that API invocation explicitly requests only
`merge_method=MERGE`, rejects a provider response showing squash/rebase/default substitution and later
proves the approved result commit is an ancestor of the merged commit. Ordinary users/bots cannot make
an enqueue or merge complete because they cannot mint the expected-source candidate authorization check.
No direct push is permitted.

Immediately after successful consume/revalidation, the Landing App changes only that candidate's
`landing-authorization` check from pending to success. Decision stale/replay, candidate drift, gate RED
or actor mismatch leaves it pending/failed; no user or ordinary bot can substitute a same-name check.

- [ ] **Step 4: Attest final merge**

After merge, a separate clean job verifies merged commit/tree, candidate lineage, required post-merge
checks and current policy. If attestation fails, set `automatic_landing=false` and
`recovery_status=assurance_recovery_blocked`, preserve safe evidence, and reject every subsequent
otherwise-valid landing until a separate recovery Packet is approved and consumed. Never rewrite the
failed event as PASS.

- [ ] **Step 5: Verify and shadow-land L06**

Static workflow corpus, merge-group target tests, fencing-token/crash/split-brain corpus and synthetic
provider/Git DAG tests pass. Commit `ci: enforce sole assurance landing authority`. Because live server
enforcement is not yet enabled, land L03–L06 trust-root candidates only through the program's bounded
pre-enforcement shadow exception with Plan 4's canonical exact Packet/decision/WORM/notary chain,
independent Sol review, current ordinary CI and canonical post-merge landing-equivalence attestations.
Do not emit a `pcv-assurance-bootstrap-exit-v1` envelope for an L work ID. The workflow remains evaluate/
shadow-only until L07 readback passes.

Before this commit, run the merged apply tool in PlanOnly against captured current provider state and
prove its exact old/new/API/rollback projection is deterministic. The PlanOnly output is evidence input
for L07 but never an apply decision or PASS.

## Task NHA-L07: Apply and verify server rules under exact approval

**Prerequisite:** L03–L06 implementation commits are exact `main` ancestors and their canonical shadow
post-merge attestations are independently reproduced. Applying policy before this condition is
forbidden.

**Files:** no tracked repository change. Use only the exact merged-main apply script, policy projection,
CODEOWNERS and workflow/App digests; this task changes approved external administrative/provider state.

- [ ] **Step 1: Re-run merged-main PlanOnly and create a fresh apply Packet**

From a fresh exact L06 post-merge `main`, default output lists exact create/update API calls, old/new
rule digests, required check/App identities, expected canary and rollback. It performs no mutation and
is never PASS evidence. Generate a new Packet with `packet_type=trust_root`,
`phase=execution_authorization` binding this exact main/tool/projection/workflow/App/before-state/API
command digest, `execution_scope=provider_administration`, expiry and rollback. It also prebinds the
mandatory provider-audit/readback/final-attestation tool argv/digests, exact output prefixes, providers,
retention, separate read credential, conditional create/readback/notary and abort/reconciliation; obtain
an authenticated approval. Those raw outputs are finalizers of this one apply transaction, not an
independent aggregate publication. Any workflow or provider drift invalidates it.

- [ ] **Step 2: Guard actual apply**

If plan purchase, repository transfer, collaborator/CODEOWNER grant or App install remains necessary,
perform each through its own exact administrative Packet/decision/readback and regenerate the final
ruleset Packet against the new repository ID/state. A deferred L02 owner access must be granted and
read back here before CODEOWNERS/ruleset enforcement; only the external live apply projection and final
Packet are regenerated after that readback. Tracked `landing-policy.json`/`main-ruleset.json` contain the
desired identity/permission, never mutable `access_status`, and remain unchanged. If identity or desired
permission changes, stop and land a separately authorized L06 trust-root revision before retrying L07.
`-ApplyDecisionPath` validates and consumes only the
fresh final L07 decision, exact repository/projection/command/current before-state/workflow/App digests
and expiry. Apply with compare-and-swap/ETag where available. Append provider audit receipt separately.
In native mode the applied/read-back queue projection must set and prove all seven named queue
parameters exactly; inability to configure or read any value blocks instead of falling back to defaults
or multi-PR groups. In equivalent mode the applied/read-back projection must prove the exact merge API
accepts and is fixed to commit-preserving `MERGE`; any available or selected squash/rebase/default path
keeps enforcement false.
Do not use `--admin` or an owner bypass.
The provider audit receipt is emitted only by the consumed apply Packet's prebound finalizer; it is not
authorized by a prior selection or tracked-code decision.

- [ ] **Step 3: Read back through independent credentials**

A separate Sol verifier re-queries APIs and computes `required_enforced`. It proves every required
check's expected App/integration, all required workflows' merge-group behavior, zero covered-role bypass
and the both-mode `landing-authorization` identity. Mismatch triggers approved rollback
only if exact old-state restore is still safe; otherwise automatic landing remains off and recovery is
blocked.

- [ ] **Step 4: Attest readback and switch from shadow without a repository commit**

Provider state receipt is immutable external evidence, not hand-written Markdown. A separate clean
readback attestation is the only input that changes the already-merged workflow from shadow to enforce;
code presence alone cannot do so. The workflow derives mode from signed live policy/readback, so L07
creates no follow-up commit. Any required tracked correction restarts at L06 under a new Packet rather
than being mixed into provider apply.
The independent readback and final attestation use the exact prebound read-only principal/finalizer and
must complete before this execution transaction closes; any later aggregate projection needs a new
artifact-only Packet.

## Task NHA-L08: Run non-destructive live enforcement canaries

**Files:** no tracked file. Exact external artifacts are the Packet-bound canary branch/ruleset/App
before-state, rejected-attempt receipts, bounded no-op landing/final attestation, NHR-029 recovery events,
cleanup/readback records and `NHA-L08-live-enforcement-canaries-v1`. It creates no main-tracked file or
main-targeting PR. Only separately approved commits/PRs targeting the exact Packet-named expendable canary
ref are permitted; they are cleaned up after readback and can never land on main.

**External state:** approved canary branch/ruleset/App operations only.

- [ ] **Preflight and publish the immutable root.** From fresh exact L07 state, consume a fresh
      `artifact_only` trust-root decision whose only effects are provider read APIs and create-only evidence.
      Publish/read back L08's signed root dispatch, this selected range and descriptor with separate receipts;
      freeze exact ETags, provider identities, setup lineage/templates, probe-pair parent ordinals, output
      prefixes and rollback-horizon inputs. It performs no provider mutation. Every later L08 range reopens
      this byte-identical root/receipt; root recreation or a preexisting current range rejects.
- [ ] **Escrow setup rollback before setup authority.** Seal the static `canary_setup` lineage—setup rollback
      range/descriptor, reservation template/stable slot, setup-forward template and unused-release
      template—as non-executable intrinsic attachments. There is no probe plan or stage-two resolver in this
      setup branch. Approve the setup rollback over exact initial ETags/inverse/readback and a future
      reservation-receipt slot; approval writes nothing and remains unconsumed.
- [ ] **Reserve, claim and execute setup.** Consume a fresh `canary_setup_reservation` decision to CAS/read
      back `reserved`, its owner/fencing/epoch receipt and a rollback-capability horizon covering setup,
      canaries, cleanup/cancel and safety margin. Its consumed finalizer first promotes the reservation
      template/attachment receipt byte-equal, then performs the CAS. Locally validate the sealed setup-
      forward template before its request; the distinct decision's winning E05 `consume_and_claim`
      atomically moves `reserved -> setup_claimed`, and its prebound finalizer then publishes the active
      range/descriptor/attachment-promotion receipts before the first branch/ruleset/App side effect. Exact
      readback alone moves it to `setup_active`. On partial started failure, setup rollback first wins atomic
      consume-and-claim `setup_claimed|setup_active -> rollback_claimed`; its prebound finalizer then promotes
      the active rollback range/descriptor/attachment receipt before inverse and terminalizes its incompatible
      setup-unused-release subject as `superseded_by_rollback_consume`. Exact inverse/readback reaches
      `restored`, fails L08 and starts no canary. For pre-effect abandonment, a fresh
      setup-unused-release decision first wins one E05 `consume_and_claim` that both consumes it and moves
      either `reserved -> release_claimed` when setup-forward was denied/expired/unconsumed, or
      `setup_claimed -> release_claimed` after fencing a claimed runner; both require setup side-effect count
      zero. Its finalizer
      then promotes the active release records. The `reserved` path terminalizes the most-materialized open
      setup-forward subject as `abort_release` and setup-rollback subject as `unused_release`; the
      `setup_claimed` zero-effect path targets only the still-open rollback as `unused_release`. It then
      releases/readbacks the reservation and reaches `released`. A losing/partial/uncertain claim writes no
      terminal/effect and remains fenced for reconciliation. Setup authority
      contains no future candidate/PR/merge.
- [ ] **Create isolated candidates.** For each actual canary commit/PR, resolve one dedicated deferred
      `exact_paths`, `trust_root/tracked_change`, `candidate_commit_no_merge` child targeting only the
      Packet-named expendable ref, and consume its fresh execution decision. Candidate execution cannot
      target main or inherit setup/probe authority. The immutable candidate bytes may be handed off by exact
      lineage to a later landing resolution, but no range, decision, nonce or consume is reused. A rejection-
      only candidate has no landing range.
- [ ] **Materialize every probe pair noncircularly.** For each actual candidate/operation, resolve its
      predeclared `probe_pair_plan_resolution` slots to one signed `probe_attempt_plan` containing exact
      candidate/ref/PR/queue, ETags, actor/permission digest, argv/nonce, expected `reject|pending` and inverse,
      but no future child/decision/receipt. The protected first resolver emits exactly, in order, rollback
      child, reservation template, `probe_attempt_resolution` stage-two parent and unused-release template,
      plus the signed four-output envelope. Seal/read back that bundle, approve the rollback child unconsumed,
      then consume a fresh `canary_enforcement_probe/reservation` decision. Its consumed finalizer publishes/
      readbacks the active reservation range/descriptor, resolution and attachment-promotion receipts before
      the provider CAS/readback yields the measured reservation. Require that receipt and sufficient horizon
      before the stage-two resolver emits the one sealed attempt child. Locally
      validate it before request; after a fresh attempt decision wins
      `consume_and_claim reserved -> attempt_claimed`, its prebound finalizer publishes the active range/
      descriptor, resolution and attachment-promotion receipts before the provider API. Reservation,
      rollback and release templates follow the same consume-then-byte-equal-promotion order.
- [ ] **Classify probe outcomes and close only the winning branch.** Exact rejection moves to
      `eligible_reject_release`; one E05 `consume_and_claim` atomically consumes a fresh unused-release
      decision and moves `eligible_reject_release -> release_claimed`; its prebound finalizer publishes/
      readbacks the active release range/descriptor, resolution and attachment-promotion receipts, appends one
      winner-owned terminal cancellation for the most-materialized rollback subject with reason
      `unused_release`, releases/readbacks the reservation and then marks `released`. Pre-attempt abort uses a
      fresh release decision whose one atomic transaction moves `reserved -> release_claimed`; its prebound
      finalizer then publishes/readbacks the active release range/descriptor, resolution and attachment-
      promotion receipts before it closes two still-open targets one time each: the most-materialized stage-
      two/attempt subject with reason `abort_release`, and the paired rollback subject with reason
      `unused_release`; only then may it release/read back the provider reservation and reach `released`.
      Expected rejection or verified landing never targets the already-consumed attempt and closes only
      rollback. Expected pending moves to `pending` and retains rollback. Unexpected transition moves to
      `rollback_required`; only its paired rollback may win E05 `consume_and_claim`. The rollback finalizer
      publishes/readbacks the active rollback range/descriptor, resolution and attachment-promotion receipts,
      terminalizes the incompatible unused-release/landing subject as `superseded_by_rollback_consume`, then
      performs the exact inverse and provider readback to reach `restored` before L08 fails. A losing/partial/
      uncertain release or rollback writes no terminal event/effect and requires
      reconciliation. Cross-pair lineage, insufficient horizon or owner/epoch drift rejects.
- [ ] **Run the enforcement corpus.** With ordinary non-bypass credentials, prove direct/force push,
      delete, missing-check merge, stale/replayed decision, wrong-App check, ordinary merge and bypass are
      rejected. Native mode reads all seven queue parameters and proves each of two isolated merge groups has
      one constituent and remains pending without the Landing App. Equivalent mode proves squash, rebase and
      omitted/default method reject. A valid APPROVE with one RED assurance gate remains blocked. No failed
      destructive probe targets main.
- [ ] **Land only designated valid canaries.** Resolve each actual immutable valid candidate through its own
      `candidate_landing_resolution` and submit a fresh landing decision targeting only the expendable ref.
      For an associated pending pair, E05 performs one indivisible `consume_and_claim` transaction that
      consumes that decision and moves `pending -> landing_claimed`; its finalizer then
      publishes/readbacks the derived landing range and resolution receipt—no descriptor is permitted for
      this landing-only range—before merge. Exact
      post-landing ancestry/provider readback moves only that pair to `eligible_landing_release`; landing claim
      alone never cancels rollback. Then one E05 `consume_and_claim` atomically consumes a fresh unused-
      release decision and moves `eligible_landing_release -> release_claimed`; its finalizer publishes/
      promotes the active release range/descriptor, resolution and attachment-promotion receipts before
      terminalizing the incompatible rollback subject with reason `unused_release`, releasing/readback and
      marking `released`.
      Deterministic landing failure moves `landing_claimed -> rollback_required` and permits only its paired
      rollback; uncertain landing is reconciliation-only and writes no release/cancellation.
- [ ] **Exercise bounded recovery.** Simulate the specified post-merge attestation failure, prove automatic
      landing blocks, and—only if this drill runs—consume a fresh NHR-029 provider-administration recovery
      decision; its finalizer publishes/readbacks the active range/descriptor before its exact current-state
      recovery side effect. It has no candidate, landing
      or cleanup capability; independent readback must restore the canary state.
- [ ] **Cancel pending pairs, clean setup and release escrow.** For every non-landed pending pair, atomically
      consume-and-claim `pending -> rollback_claimed`, publish/promote its active rollback records before its
      exact cancel/restore, terminalize the incompatible
      unused-release/landing subject as `superseded_by_rollback_consume`, and let readback yield `restored`.
      Then E05 performs one indivisible `consume_and_claim` that consumes a fresh cleanup decision and moves
      `setup_active -> cleanup_claimed`; its
      finalizer publishes/readbacks the active range/descriptor before removing canary branch/PR/queue/
      ruleset/App state. Exact initial-state/no-main-change readback yields
      `eligible_setup_release`. One E05 `consume_and_claim` atomically consumes a separate fresh
      `canary_setup_unused_release` decision and moves `eligible_setup_release -> release_claimed`, then its
      finalizer promotes the active release range/descriptor/attachment
      receipt, append the winner-owned terminal event with reason `unused_release` for the most-materialized
      still-open setup-rollback subject, release/read back the setup reservation and mark `released`. Cleanup
      authority cannot release it itself. Any competing/uncertain result remains
      fenced and reconciliation-only.
- [ ] **Publish the terminal result separately.** Consume a final fresh `artifact_only` decision and
      publish/read back/notarize `NHA-L08-live-enforcement-canaries-v1`, binding the root/range/descriptor and
      derived resolution/promotion receipts, every consume/claim/outcome/terminal event, valid expendable
      landing, NHR-029 recovery, cleanup, release and final provider readback. It authorizes no new provider,
      repository or host operation.

Failed destructive attempts must target only the canary branch; never probe force/delete against main.

## Task NHA-L09: Publish live server enforcement attestation

**Files:** no tracked file. Exact external artifacts are the Packet-bound provider/ETag/decision-channel/
cutover/revocation reads, replay/fencing/recovery results, external Dashboard fact set and
`NHA-L09-live-landing-authority-v1`. This task creates no commit or PR.

- [ ] From exact post-L08 provider state, create/approve/consume a fresh artifact-only
      `trust_root/execution_authorization` Packet before the first read/publish side effect. Bind exact
      main, L04 cutover/profile digest and revocation state, provider read APIs/ETags, protected tool argv,
      output prefix/provider/retention, create-only/readback/notary, expiry and abort/reconciliation with
      zero tracked/provider-admin/host mutation.
- [ ] Independent actor re-queries policy after canaries and records provider response/ETag digests.
- [ ] Confirm required checks and source identities, no covered-role bypass, stale dismissal,
      serialization, direct-push prohibition, exact commit-preserving `MERGE` in the selected mode and
      final ancestry/lineage.
- [ ] Run decision replay/double-consume and candidate mutation tests once more.
- [ ] Re-read the active decision provider/App/environment/principal/issuer/key/workflow digest and the
      atomic L04 cutover record; prove interim permission revoked, every unconsumed interim approval has
      its immutable ID listed in that record and no post-cutover event or consume authenticates under the old
      profile. No synthetic decision-closure event is expected.
- [ ] Reproduce fencing-token lease expiry/crash/split-brain rejection and the NHR-021 red-gate plus
      NHR-029 recovery canaries from immutable evidence.
- [ ] Generate and publish an external immutable Dashboard fact set;
      `required_enforced=true` may now be set only by derivation. Do not edit Plan 4's tracked RED
      Dashboard snapshot in this artifact-only task; Plan 6 P09 regenerates/lands the canonical tracked
      JSON/Markdown only after every remaining completion fact is actual.
- [ ] Publish `NHA-L09-live-landing-authority-v1` binding the consumed L09 artifact authority, with expiry
      and stale predicates tied to the full active decision provider/App/environment/principal/issuer/key/
      workflow profile, L04 cutover digest, revocation state, ruleset and CODEOWNER digests.

Any subsequent policy/App/CODEOWNER/workflow, active identity/profile/key, cutover or revocation-state
change immediately stales this attestation.

## Plan 5 exit gate

- [ ] A user-approved native or equivalent server mode is live; current 403 is resolved.
- [ ] Protected main is PR-only, force/delete/direct push blocked and covered-role bypass set empty.
- [ ] Seven assurance gates plus product checks are bound to exact trusted source identities.
- [ ] Independent trust-root review is enforceable through a real CODEOWNER identity.
- [ ] Latest-base serialization and all equivalence/lineage fixtures pass.
- [ ] Authenticated decisions reject replay/staleness and consume exactly once; the active channel and
      atomic cutover readback are current, interim authority is revoked, unconsumed old approvals are
      stale and no old-channel post-cutover event exists.
- [ ] In native mode every required producer runs on the exact `merge_group/checks_requested` candidate;
      all seven queue parameters are attested and every group has one constituent;
      in equivalent mode every producer runs on the exact Authority-signed lease/fenced latest-base
      candidate and only the explicit commit-preserving `MERGE` API can land it. Squash/rebase/default
      substitution is rejected, and PR-head results are never reused in either mode.
- [ ] Live canaries show invalid landing attempts rejected by the server.
- [ ] Valid APPROVE plus RED assurance remains blocked, and post-merge failure blocks later landing until
      a separate recovery decision is consumed.
- [ ] Final merge attestation is independently verifiable.
- [ ] If any item is missing, `required_enforced=false`, `overall_readiness=red`, and Plan 6 activation is
      blocked.
