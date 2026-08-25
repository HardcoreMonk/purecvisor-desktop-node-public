# PureCVisor Assurance Pilot, L/Release Rehearsal and Activation Implementation Plan

> **Status:** proposed child plan 6; no control materialization, product card, package build/install,
> service action, Hyper-V action, rollback or activation may start from this document alone.
>
> **Execution class:** control/rehearsal/activation tooling is trust-root L/Sol/Release with independent
> Sol review. Frozen S/M product pilots use Luna Max and a separately dispatched verifier. Every host
> operation requires its own exact child Packet and prior decision.

**Goal:** Prove the assurance environment on real bounded product work and a fully authorized
install/service/Hyper-V/rollback campaign, exercise packet-only user decisions, and activate successor
v4 only after the complete NHR formula and live Landing Authority pass.

**Architecture:** Fresh-main control-only materialization remains inactive. A deterministic selector
freezes three representative product pilots. Each card uses a frozen oracle, bounded executor, clean
independent verifier, notary and Landing Authority. Separate category child Packets then drive a
prebuilt MSI through sacrificial-host service/install, targeted Hyper-V and rollback stages. Independent
reproduction precedes a two-PR activation: first an attested `pending_activation` record, then a new
exact `release_change` decision for the pointer-only effective-current transition.

**Prerequisite:** `NHA-L09-live-landing-authority-v1` is fresh and `required_enforced=true`. Plans 1–5
are exact `main` ancestors and their artifacts remain accessible.

**Source design:** `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`
§§13.4, 15, 16, 18 and NHR-001..030.

---

## File map

**Control materialization outputs**

- Create `docs/superpowers/plans/luna-completion/execution-state.json` and its schema if not already
  materialized by the v4 bootstrap.
- Create exact v4 task-card Markdown files, actual `requirements.json`, `spec-lock.json`,
  `card-blueprints.json`, `traceability.json` and per-card `acceptance/<CARD-ID>.json`.
- Create the first generated Trust Dashboard/Packet records required for the materialization request.
- Do not modify product, workflow, current-evidence, GA-evidence or effective-current pointer in the
  materialization PR.

**Pilot selection and result files**

- Create `docs/superpowers/plans/luna-completion/pilot-selection.schema.json`.
- Create `docs/superpowers/plans/luna-completion/pilot-selection.json` only after exact candidates are
  selected from v4.
- Product/test files are not pre-guessed in this plan. Their exact create/modify/delete paths come only
  from the approved v4 card blueprint and pilot-selection Packet.

**Rehearsal tooling**

- Reuse without revision Plan 2's `authority-reservation-receipt`, `rollback-capability-horizon` and
  `pair-state-transition` schemas/tools and E05's canonical Decision Plane.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceHostReservationAdapter.psm1` and
  `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceHostReservation.ps1` only as host root/surface
  CAS/readback operations behind that engine.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceHostReservationAdapter.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssurancePackageBuildChild.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssurancePackageBuildChild.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssurancePackageInstallServiceChild.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssurancePackageInstallServiceChild.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceHttpBindingTlsChild.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceHttpBindingTlsChild.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceHyperVChild.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceHyperVChild.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceLifecycleRollbackChild.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceLifecycleRollbackChild.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceRehearsalCampaign.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceRehearsalCampaign.Tests.ps1`.
- Modify existing package/route/clean-host/TLS scripts only through separately frozen L cards to accept
  prebuilt artifacts, separate categories and remove embedded credentials/retry ambiguity.

Every new control, selection, runner, test, campaign and activation path is protected before code work.

## Canonical state-ledger closure for P02–P10

P02 materialization marks every A00–P01 card from immutable prior exits as historical `completed`, marks
P02 as the sole current card, and leaves P03–P10 not started. Thereafter no P task is complete, and its
successor cannot start, until its terminal result/verification/decision/final-landing or artifact-only
attestation exists and a separate state-ledger candidate closes it.

For each P02–P10 closure, start from fresh main; create/consume a new
`packet_type=trust_root`, `phase=execution_authorization` decision whose sole tracked operation is the
exact expected transition in canonical `execution-state.json`. Bind prior state digest, card/work ID,
allowed transition, terminal immutable locators and no future state-PR merge fact. Broker that one file,
run actual Release, independent Sol and all seven gates, then create/consume a distinct
`trust_root/landing_authorization`, land unchanged and verify the transition from fresh post-merge main.
This state-only PR is part of the owning card and does not create a recursive card. Product, workflow,
oracle, current-evidence, Dashboard, pointer and host paths are forbidden. P10's pointer validator
requires P02–P09 completed and permits only P10 itself to be the current `landing_pending` card; P10 uses
this same closure after its final activation attestation.

P04/P05 contain two ledger namespaces that must not be collapsed. Each selected S/M product card gets
its own state-only closure after that card's product landing. Separately, the `NHA-P04` or `NHA-P05`
owner/control card gets another state-only closure only after all selected cards owned by that task are
completed and their pilot attestations exist. The owner candidate binds those already-immutable outputs
but no future fact about its own state PR; its fresh post-merge attestation is the task exit. P06 cannot
start from selected-card completion alone.

## Normative task-dispatch matrix

Each row uses the Program §5.1 canonical Test, Red and Final argv with its exact work ID. Static ranges
expand the File map to exact paths; selected product/rehearsal ranges remain
`deferred_resolver/ready=false` until an immutable approved v4 selection/card or campaign/child-plan
descriptor plus protected resolver emits the complete signed child range. Only then may a fresh Packet
bind that fixed child digest; a Packet never resolves the operations it authorizes. Final is the card's actual lane and separately dispatched verifier, with Sol/Release
for every trust-root, state, L/release or activation range. Every state closure is a distinct one-file
execution range plus deferred verified landing range.

| Work ID | Ordered path/range closure | RED or allowed N/A | Implementation/finalizer boundary | Rollback | Commit/PR boundary |
|---|---|---|---|---|---|
| NHA-P01 | `artifact_only`, exact Plan1–5/live-policy/corpora reads and preflight exit prefix | `red_not_applicable`: read-only audit; any missing/stale/drifted prerequisite is named fail-closed oracle | revalidation and P01 exit only | invalidate preflight; no retry/mutation | `artifact_only_no_commit` |
| NHA-P02 | deferred v4 resolver to exact control-materialization `exact_paths` plus verified landing, post-merge `artifact_only` inactive-state attestation, then deferred one-file state closure | missing/duplicate/cycle/unresolved-card/control-path cases run RED before generation | control records and inactive attestation only; product/workflow/current evidence/pointer/host excluded | whole materialization/state revert; invalidate attestation | materialization `tracked_pr`; attestation `artifact_only_no_commit`; closure `state_only_pr` |
| NHA-P03 | one `selection_resolution` deferred parent resolves the complete exact selector schema/record and selected requirements/oracles/card blueprints to one `exact_paths` candidate with `candidate_commit_no_merge`; after verification, a requirements-approval decision-consume-only `artifact_only` range, optional distinct spec-revision decision-consume-only `artifact_only` range, then one trust-root `verified_candidate_landing` range requiring all prior receipts; state closure follows | no-eligible, overlap, docs-only, tier/capability and nondeterminism fixtures RED | deterministic whole selection/spec/oracle/card set only; no partial candidate; every prelanding range binds the identical candidate but cannot merge | whole selection/state revert | candidate `candidate_commit_no_merge`; prelanding consumes `artifact_only_no_commit`; final landing `tracked_pr`; closure `state_only_pr` |
| NHA-P04 | deferred selected S-card `exact_paths` product/test candidate plus verified landing, per-card state closure, distinct `artifact_only` S-result publication, then owner-card state closure | card positive/negative/boundary/property/rollback oracle must be RED before code | only P03-selected S paths; Luna Max/Fast plus independent verifier | card rollback; invalidate result; state reverts | product `tracked_pr`; result `artifact_only_no_commit`; closures `state_only_pr` |
| NHA-P05 | ordered deferred selected M-card `exact_paths` candidate/landing, per-card state closure and distinct `artifact_only` result for each card, then owner-card closure | each card's frozen oracle must be RED before code | only P03-selected M paths; Luna Max/Full plus independent verifier | per-card rollback; invalidate result; state reverts | products `tracked_pr`; results `artifact_only_no_commit`; closures `state_only_pr` |
| NHA-P06 | `artifact_only`, exact Packet-only exercise request/decision/consume/result prefixes, then state closure | `red_not_applicable`: no implementation; malformed grammar, no decision, replay and REQUEST-CHANGES/DENY are fail-closed oracle | user-facing decision exercise only; no code/provider/host mutation | invalidate exercise; state revert | exercise `artifact_only_no_commit`; closure `state_only_pr` |
| NHA-P07 | `exact_paths` host root/surface adapter plus category rehearsal tool/test candidate and verified landing; each selected existing-script L-card gets its own deferred exact candidate/landing; post-merge `artifact_only` tooling attestation, then state closure | child replay, mixed category, missing horizon/reservation, two-engine drift, secret/retry/rollback ambiguity corpus RED | adapters only behind Plan2/E05; no actual package/install/service/TLS/Hyper-V mutation | whole tooling/L-card/state revert; invalidate attestation | each tooling/L range `tracked_pr`; attestation `artifact_only_no_commit`; closure `state_only_pr` |
| NHA-P08 | initial artifact-only root/preflight; actual build; installer/TLS/Hyper-V each resolve signed plan→four-output bundle→rollback approval→fresh guard reserve→stage-two forward→fresh consume-and-claim; Hyper-V/TLS/installer restore; credential transaction; atomic guard close; aggregate/evidence and state closure | tampered artifact, wrong baseline/host/category, future child, missing horizon/resolution/promotion, race loser and incomplete restore RED | dedicated reimageable VM, exact prebuilt artifact and E05/P07-backed category children only; no all-decisions-upfront | winner-only abort/rollback; root remains fenced and reconciliation-only on uncertainty | mutation/provider/guard-close/campaign ranges `artifact_only_no_commit`; closure `state_only_pr` |
| NHA-P09 | one immutable task-dispatch root: outer preflight first range; conditional fallback outer host-reservation root/pair; independently signed inner preflight range reopening that task root; distinct inner host-reservation root and installer/TLS/Hyper-V pairs; close inner first, then outer rollback/close; second aggregate and three tracked candidates | missing role/raw proof, false GREEN, future child, task-root recreation, cross-host-root/campaign reuse, false inner restore, stale live policy and incomplete restoration RED | independent reproduction with disjoint outer/inner host-reservation owners/fences; afterward only resolved evidence/projection/control paths | winner-only child rollback; inner uncertainty may outer-reimage only as failed reconciliation; whole tracked/state reverts | mutation/provider/guard-close/aggregate/attestation `artifact_only_no_commit`; three tracked ranges `tracked_pr`; closure `state_only_pr` |
| NHA-P10 | ordered exact pending-activation `exact_paths` candidate with explicit deferred `verified_candidate_landing`, then fresh pointer-only `exact_paths` candidate with its own deferred verified landing, external activation-lineage `artifact_only` projection and final state closure | stale prerequisite, extra pointer path, premature effective-current, wrong target and missing post-merge attestation RED | pending record first; only after its attestation may the separate pointer-only release change run; no product/package/host | revert pending before activation; after activation fail closed/fix forward | pending/pointer `tracked_pr`; projection `artifact_only_no_commit`; closure `state_only_pr` |

## Task NHA-P01: Re-audit all environment prerequisites on fresh main

**Files:** no product or host edit.

- [ ] From exact fresh main, create/approve/consume a fresh artifact-only
      `trust_root/execution_authorization` Packet before the first measured run or publication. Bind every
      Plan 1–5 input locator, known-bad command/tool digest, exact main, output prefix/provider/retention,
      create-only/readback/notary, expiry and abort/reconciliation, with zero tracked/provider-admin/host
      mutation.
- [ ] Resolve every Plan 1–5 exit artifact and revalidate signature, exact subject, accessibility,
      retention/freshness and unique main ancestry.
- [ ] Re-query live Landing Authority; any 403, bypass, ruleset/App/workflow/CODEOWNER drift or missing
      check returns `required_enforced=false` and stops.
- [ ] Re-read the full active decision provider/App/environment/principal/issuer/key/workflow profile,
      L04 cutover digest and current revocation state. Any difference from L09, or a stale L09 predicate,
      stops before a pilot rather than accepting the old attestation.
- [ ] Re-run known-bad contract, Pester, confinement, evidence, decision and landing corpora. Rejection
      rate must remain 100%.
- [ ] Confirm current operational/public claims still match typed current evidence and no plan has
      silently promoted 0.42.68 over the 0.42.65 operational anchor.
- [ ] Publish `NHA-P01-fresh-environment-preflight-v1` with exact main commit/tree and expiry.

This task has no automatic retry after drift; fix the owning earlier plan through a new trust-root
revision.

## Task NHA-P02: Materialize successor v4 control-only state on exact fresh main

**Files:** only the exact control materialization outputs frozen by v4.

- [ ] **Step 1: Generate a new materialization Packet**

All earlier materialization decisions are stale because Plans 2–5 changed main and the trust root.
Generate a new Packet with `packet_type=trust_root`, `phase=execution_authorization` binding exact fresh
main, v4/integration/spec locks, canonical card/DAG set, exact control paths and exclusions. It must
state:

```text
product_change=false
workflow_change=false
current_evidence_change=false
effective_current_change=false
host_mutation=false
activation=false
```

Obtain and preserve a new unconsumed decision. The Plan 1 bootstrap approval cannot be reused.

- [ ] **Step 2: Materialize canonical control records**

Generate schemas/state/cards/requirements/acceptance/traceability from v4, not manual prose. Validate:

- card/DAG missing, duplicate, extra and cycle counts are zero;
- every ready card has resolved ambiguity, exact operations, spec/oracle, actors, capabilities,
  rollback and state-conditional artifact locators;
- every protected path routes L/Sol/Release and every required card has an independent verifier;
- NHR-001..030 have planned bidirectional traceability;
- A00–P01 terminal exits materialize as completed, P02 alone is current, and P03–P10 are not started;
- predecessor remains current and successor state is `materialized_inactive` only after merge proof.

- [ ] **Step 3: Execute one control card at a time**

Immediately before the first brokered control write, revalidate and consume the P02 decision once
against the exact aggregate materialization target. Use bounded executor/verifier/notary for each
control card. After the exact aggregate candidate passes actual Release and all seven gates, generate a
separate post-verification Packet with `packet_type=trust_root` and
`phase=landing_authorization`, obtain/consume its decision through
the Landing Authority, and use commit-preserving history. Product, workflow, current evidence and host
mutation are forbidden by the pre-run scope allowance, post-run scope manifest and OS confinement.

- [ ] **Step 4: Post-merge attest inactive state**

From fresh main, resolve original card commits, materialization merge candidate/final tree, required
checks and exact generated state. Append `materialized_inactive`; do not activate. Before this independent
post-merge projection, create/approve/consume a fresh artifact-only `trust_root/execution_authorization`
binding exact read/tool/prefix/provider/retention/readback/notary/expiry/abort facts and zero tracked/host
mutation; the materialization landing decision is not reusable. Publish
`NHA-P02-v4-control-materialized-inactive-v1` under that authority, then execute the canonical state-
ledger closure above before P03.

## Task NHA-P03: Select and freeze three representative S/M pilots

**Files:** pilot-selection schema/record and exact selected requirements/oracles/card blueprints.

- [ ] **Step 1: Run the deterministic selector**

Input is the materialized v4 DAG and latest main. Candidate cards must:

- have all dependencies completed and fresh;
- be S or M, non-trust-root and non-release;
- require no admin, secret, network, package, service, TLS or Hyper-V capability;
- have resolved frozen positive/negative/boundary/property cases and rollback oracle;
- fit exact bounded executor and clean verifier environments;
- avoid overlapping writable paths across simultaneous candidates.

Run the selector as a read-only protected command and freeze its signed candidate set. Select exactly
three: exactly one S and exactly two M. At least two must
change actual `src/**` or `web/src/**` product code. Three docs-only cards are invalid. Do not nominate a
v3 card by intuition; if v4 yields no eligible set, stop and revise requirements through the authority
flow.

Before any tracked write, a Sol reviewer validates the complete selector output and the protected
`selection_resolution` resolver emits one signed exact child range containing the selector schema/record
and every selected requirement/oracle/card-blueprint operation together. Only then generate and consume a
Packet with `packet_type=trust_root`, `phase=execution_authorization`, exact fresh-main/child-range digest
and selector/review oracle. It authorizes only construction of that whole candidate, claims no PASS and
cannot land a partial selection.

- [ ] **Step 2: Independently review pilot adequacy**

The post-candidate Sol verifier rechecks representativeness, no hidden trust-root impact, acceptance
completeness, exact equality to the pre-write resolved range and quality/security obligations. After the
immutable candidate is verified, generate two separate Packets
for the same exact candidate: (1) `packet_type=requirements_approval`,
`phase=landing_authorization`, `purpose=pilot_selection_approval` binds card selection, outcomes, risks
and sequence; (2)
`packet_type=trust_root`, `phase=landing_authorization` binds the pilot-selection schema/record,
requirements/oracles/card blueprints and exact path operations. Obtain both exact decisions because the
assurance environment is not active yet. The first freezes selection semantics; the second alone
authorizes landing the trust-root files. Neither authorizes pilot execution-result acceptance or
activation. If selection changes a frozen spec, use a third separate `spec_revision` Packet rather than
combining packet types. Materialize the requirements Packet as its own decision-consume-only
`artifact_only` range with a fresh consumer, exact candidate/tree/change-set, no repository operation, no
merge capability and a prebound immutable consume receipt. Materialize the optional spec-revision Packet
the same way with a different consumer/range. The final trust-root landing range is separately resolved to
`verified_candidate_landing` and prebinds the exact prior receipt locators/digests; it rejects a missing,
reordered or candidate-mismatched receipt.

- [ ] **Step 3: Consume the selection decision and freeze pilot inputs as ancestors**

First consume the exact requirements decision in its artifact-only prelanding range and publish/read back
its receipt; if required, do the same for the distinct spec-revision range. Neither range has repository
write, enqueue or merge capability. Then Landing Authority opens those receipts, proves their candidate/
tree/change-set equality to the unchanged selection candidate, and consumes only the trust-root landing
decision in the final `verified_candidate_landing` range immediately before merge. Missing either
mandatory receipt, a required spec receipt, or any order/candidate mismatch blocks landing. Merge those
commits before any product lease and post-merge attest them. Any later oracle/expected-result/risk/rollback
change invalidates selection and all pilot results. Execute P03's separate canonical state-ledger closure
before P04.

## Task NHA-P04: Execute and land the S pilot

**Files:** product candidate: only exact S-card product/test paths from P03. Later state-ledger candidate:
only canonical `docs/superpowers/plans/luna-completion/execution-state.json`.

- [ ] Generate a pre-execution `mutation_authorization` Packet with
      `mutation_kind=brokered_code_change`, binding exact start commit/tree, card/spec/oracle/scope, path
      operations, broker lease, commands/capability/risk/revert and planned trace edges; obtain and
      consume its decision before the first brokered product write. Host/category/artifact fields are
      forbidden. It cannot claim PASS or authorize later landing.
- [ ] Dispatch `gpt-5.6-luna`, reasoning `max`, in the bounded no-network/non-admin environment.
- [ ] Require RED frozen cases before implementation; executor-authored tests are supplemental only.
- [ ] Broker exactly the approved path operations and create a result commit with no trust-root change.
- [ ] Through Plan 3's Authority-only read-only verifier-model branch, dispatch a different Luna Max
      trust domain to a clean exact target; run focused + actual Fast,
      changed-code obligations and the first six gates: `spec-contract`, `scope-integrity`,
      `product-verification`, `independent-verifier`, `quality-ratchet`, `security`.
- [ ] Publish/read back/notarize all raw execution/verification artifacts; only then evaluate the seventh
      `artifact-attestation` gate. After all seven actual gates, produce the pilot Packet/Dashboard row.
- [ ] Because automatic approval is not active yet, generate a new post-verification
      `landing_attestation` Packet bound to the exact result/candidate, obtain an authenticated user
      decision, and consume it exactly once.
- [ ] Land only through the live Landing Authority; attest latest-base candidate and final merged tree.
- [ ] After that final product landing attestation exists, start from fresh main and generate/consume a
      new `trust_root/execution_authorization` Packet for only the exact canonical state transition from
      the selected S card's prior state to `completed`, binding result/verification/decision/final-
      landing locators.
      Broker only `execution-state.json`; product/test/oracle paths are forbidden and no future state-PR
      landing fact is claimed. Run actual Release, independent Sol and all seven gates, then obtain/
      consume a separate `trust_root/landing_authorization` and land the unchanged state-only candidate.
      Fresh-main readback must prove the selected-card transition and locators.
- [ ] Before independently aggregating/publishing the immutable S-pilot result, create/approve/consume a
      fresh artifact-only `trust_root/execution_authorization` binding exact product/state landing reads,
      projection tool, `NHA-P04-s-pilot-v1` prefix/provider/retention/readback/notary and abort, with zero
      tracked/host mutation. Publish under that authority, then execute a second, separately
      approved state-only closure that changes only the `NHA-P04` owner/control card to `completed` and
      binds the selected-card readback plus pilot result. Its post-merge attestation is the P04 task exit.
      P05 cannot start before that owner readback. Any waiver or user-accepted red result disqualifies
      this pilot.

## Task NHA-P05: Execute and land both M pilots sequentially

**Files:** each product candidate: only that exact M-card product/test paths from P03. After each product
landing, a separate state-ledger candidate may modify only canonical `execution-state.json`; one branch/
PR at a time.

For each pilot repeat P04 with `gpt-5.6-luna/max`, a fresh pre-execution `mutation_authorization` using
the closed `mutation_kind=brokered_code_change` branch, a
separate verifier, focused + actual Full, the same six-gates → immutable publish/readback/notary →
seventh artifact-attestation order, and a fresh post-verification
`landing_attestation` Packet/decision/consume. The selection decision is never reused as execution or
landing authority. After each product final attestation, repeat P04's separate exact trust-root state-
ledger execution/Release/Sol/landing/post-merge flow and mark only that card completed. The second M
starts from fresh main only after the first M state transition is merged and attested. Recompute scope,
spec freshness and selection eligibility; do not reuse worktrees, credentials, decisions or artifacts.

Required evidence:

- two independent result/verification pairs;
- no oracle/trust-root edits;
- changed-line coverage at least 90%, changed-branch at least 85%, targeted mutation at least 90%,
  critical survivors zero, plus existing baseline ratchet;
- no required skip/not-run/planned case;
- exact candidate/final landing lineage;
- waiver count zero.

For each result, create/approve/consume a distinct artifact-only `trust_root/execution_authorization`
binding that pilot's exact landing/read/tool/prefix/provider/retention/readback/notary and zero tracked/
host mutation; then publish `NHA-P05-m-pilot-1-v1` and `NHA-P05-m-pilot-2-v1` separately. Neither result
authority is reused for the other pilot or a state closure.
After both selected M-card state closures and pilot artifacts are immutable, execute one additional fresh-
main, separately approved state-only closure for the `NHA-P05` owner/control card. It binds both selected-
card completion readbacks and both pilot artifacts, changes only that owner entry to `completed`, and
produces P05's task-exit post-merge attestation. P06 remains blocked until this owner readback passes.

## Task NHA-P06: Run packet-only user and invalidation exercises

**Files:** immutable Packet/decision/event artifacts only.

- [ ] Create/approve/consume one outer artifact-only `trust_root/execution_authorization` Packet binding
      exact exercise IDs/request digests/grammar, parser and mutation-test tools, authenticated channel,
      output prefixes/provider/retention/readback/notary, expiry and abort/reconciliation. It authorizes
      only the three synthetic event/publication chains and no tracked/product/host/landing operation.
- [ ] Present one synthetic valid low-risk Packet and ask the user to APPROVE it without source review.
- [ ] Present a second Packet with a visible blocker/risk and ask the user to DENY it.
- [ ] Present a third Packet with an incomplete acceptance/rollback item and ask for REQUEST-CHANGES.
- [ ] For each, verify exact grammar, authenticated identity, immutable digest and correct append-only
      event. Do not execute a product/host action from synthetic exercise Packets.
- [ ] Encode the APPROVE exercise only as the existing closed
      `requirements_approval/planning_authorization` branch with
      `purpose=packet_only_user_exercise`, no path/command/capability or landing/mutation target, and an
      exercise-only consumer ID. Consume it once in that consumer; its immutable consume receipt/state is
      terminal, and no additional decision event is appended.
      DENY and REQUEST-CHANGES have consume count zero. Schema/consumer negative fixtures prove none of
      the three can match a product, host, tracked-change, landing, activation or mutation consumer.
- [ ] Mutate, one at a time, implementation head/tree, change-set, spec, evidence, risk, rollback,
      workflow, oracle and capability in copies; prove each old decision becomes stale at generation,
      verification and consumption.
- [ ] Attempt nonce replay and double consume; both reject.

Publish `NHA-P06-packet-only-user-exercise-v1`. The user must be able to decide from scope/outcome/risk/
proof/rollback alone; requiring source inspection fails NHR-001. Then execute P06's canonical state-
ledger closure before P07.

## Task NHA-P07: Harden and freeze category-specific rehearsal runners

**Files:** exact host root/surface adapter/wrapper/test and six rehearsal tools/tests plus separately frozen
modifications to reused runners. Plan 2 reservation/horizon/pair-state schemas and E05 decision history/
`consume_and_claim` are dependencies, not duplicated P07 files.

- [ ] **Step 1: Add RED category-boundary cases**

Show current route/full-gate and clean-host runners can rebuild internally, combine package/service/OS
mutation, use coarse host-mutation switches, retry uncertain steps or embed guest credentials. Preserve
expected failures.

- [ ] **Step 2: Implement build/install-service child boundaries**

Build child accepts exact clean source commit/tree, version, recipe and output root and emits hashes/
provenance/notary; it performs no install. Install-service child accepts only the prebuilt approved MSI
and expected MSI/payload/provenance hashes; internal rebuild is forbidden. It targets one approved
sacrificial host and exact install/repair/service/Web/CLI/readback commands.

- [ ] **Step 3: Implement optional TLS and targeted Hyper-V children**

TLS forward child owns only exact URL ACL/certificate/firewall binding. A separate
`lifecycle_rollback`, `rollback_kind=tls_binding` child binds exact before-state and restore commands/
oracle, is approved without side effect and receives its reservation only from a later fresh consumed
guard-reserve decision before TLS forward mutation. If the rehearsal has no TLS impact, an
independently approved `requirements_approval/planning_authorization` Packet with purpose
`tls_rehearsal_not_applicable` binds the observed no-impact oracle, exact campaign/host/artifact scope,
expiry and expected zero TLS forward/rollback mutation decisions/consumes. Its one-time planning consume
freezes the child set but authorizes no side effect; no fake PASS child is created. The campaign summary
must reference this request/approval/consume when TLS is N/A.
Hyper-V forward child owns only one exact disposable VM/switch/disk operation set and explicitly sets
package/service/TLS/other OS mutation false. A separate `lifecycle_rollback`,
`rollback_kind=hyperv_actual_vm` child binds the exact VM/switch/disk before-state, cleanup/recovery
commands and success oracle; it is approved first, then separately reserved by a fresh guard-reserve
decision before the Hyper-V forward child,
remains unconsumed during functional operations, and alone may perform cleanup/recovery side effects.

- [ ] **Step 4: Implement lifecycle rollback child**

For `rollback_kind=installer_lifecycle`, bind exact baseline/target MSI hashes, host reservation,
before-state, update/rollback commands, rollback oracle and cleanup. For
`rollback_kind=tls_binding`, bind the TLS forward child, exact URL ACL/certificate/firewall before-state,
restore commands and readback oracle. For `rollback_kind=hyperv_actual_vm`, bind the Hyper-V forward
child, exact VM/switch/disk before-state, cleanup/recovery commands and zero-leftover/readback oracle.
`package_service/build` has no host and no lifecycle reservation, while a lifecycle rollback child never
requires another rollback child. Refactor VM provision, guest lifecycle and cleanup so they are
independently observable. Remove hardcoded guest passwords; inject an ephemeral secret through the
approved secret channel, redact it and rotate/destroy after use.

For every reversible category, the adapter accepts only Program §8's sequence: predeclared initial pair
parent, actual signed plan, protected four-output first resolution, sealed attachments, unconsumed rollback
approval, fresh reservation consume/receipt/horizon, stage-two forward child and fresh forward
`consume_and_claim`. Rollback consumes through the same engine immediately before inverse. It implements
both `acquire_root` and `add_child_guards` abort-release modes and calls E05's terminal wrapper only after a
winning claim. P07 defines no approval store, decision event, transition table or alternate reservation
schema.

The host adapter supplies an authoritative reservation-domain root to E05 whose key is independent of any
approval or forward child:
`SHA256(canonical(host_identity, reservation_domain))`, where `reservation_domain` is a closed policy
constant for that host/campaign class rather than caller text. Approval, rollback and forward decision IDs are
bound only in the root value with host/before-state, owner, monotonic epoch/fencing token and expiry, so a
new ID cannot create a parallel root. Every canonical `restore_surface_id` (MSI product/service state,
URL ACL, certificate binding, firewall rule, or other closed surface) also has a guard key
`SHA256(canonical(host_identity, restore_surface_id))`; the surface key is deliberately independent of
root/domain/approval/forward IDs, and its value binds the owning root and fencing token. Root plus the
complete ordinal-sorted surface-key set
is acquired by one all-or-nothing CAS transaction; partial acquisition is rolled back before success and
never authorizes forward work.

Approval-specific requested/active/released/consumed/reconciled receipts are separate append-only WORM
events linked from the root value. Installer and TLS rollback may coexist only beneath the same active
root lease/fencing token through an explicit parent-to-child delegation that binds disjoint or explicitly
delegated surface IDs; TLS forward/rollback closes its child before installer rollback may consume or
close the parent. A different root/domain, kind, approval or forward ID with the same, partially
overlapping or cross-kind surface set therefore collides on at least one global host-surface guard and
fails atomically. A fresh abort-release consume-and-claim is allowed only before forward mutation;
after mutation each child reservation can close only through its rollback consume/result or an explicit
reconciliation Packet. The campaign root and complete guard set remain fenced after child closes and can
close normally only through the fresh P08/P09 `provider_administration/mutation_guard_close` range after
all rollback and credential receipts verify; uncertainty permits only reconciliation.
Expiry or uncertain owner state blocks forward work until reconciliation; it never silently frees the
root or surfaces. Tests race different approval and forward IDs over identical, partial and cross-kind
surfaces; parent/child delegation, atomic multi-key failure, stale fencing token, epoch rollback, expiry,
crash before/after forward mutation, release replay and double consume.
The exhaustive tests also require sufficient rollback horizon, every allowed/disallowed pair-state edge,
winner-only terminal callback, and zero provider/host effect for every losing or uncertain claim.

- [ ] **Step 5: Keep campaign summary non-executable**

`New-PcvAssuranceRehearsalCampaign.ps1` only validates and projects child Packet/decision/consume/result/
restore attestations. It has no runner credential and cannot synthesize a missing child.

- [ ] **Step 6: Verify tooling before host use**

All negative fixtures reject, PlanOnly remains planned, prebuilt artifact identity is enforced, uncertain
result has no automatic retry, and no actual host mutation occurs in runner-development PRs. Land each
trust-root change through its own Release + independent Sol review and verified-candidate landing. Any
selected existing-script L card is a distinct exact candidate/landing range; it cannot be folded into the
new-tooling candidate or inherit its decision. From exact final post-merge main,
create/approve/consume a fresh artifact-only `trust_root/execution_authorization` binding all tooling-
landing inputs, projection tool, output prefix/provider/retention/readback/notary and abort; publish P07's
terminal post-merge tooling attestation under it, then execute P07's canonical state-ledger closure before
P08.

## Task NHA-P08: Execute the authorized L/Release rehearsal campaign

**Files:** no tracked file during campaign execution. Exact external artifacts are the Packet-bound
prebuilt MSI/update inputs, dedicated-VM baseline/readback, all applicable category child decision/consume/result/
rollback records (TLS may be a separately proven N/A branch), campaign/reproduction evidence and P08
exit under create-only prefixes. P08 creates no implementation commit or PR; its later canonical
one-file state-ledger closure is the separately authorized `state_only_pr` range.

**External state:** a dedicated reimageable Windows verification VM with nested Hyper-V, exact baseline
and target artifacts, and category-specific decisions. A physical host is outside this plan because the
closed five-category contract has no physical-host-reset branch.

- [ ] **Step 1: Reserve and attest the sacrificial environment**

From fresh P07-complete main, first freeze a provider-signed, independently verified, read-only TLS
applicability classification. It has no publication, reservation or host effect. Build one branch-specific
task-dispatch manifest from that fact: the applicable shape contains the existing TLS reversible-pair initial
parent; the inapplicable shape contains the exact TLS-N/A planning range and fixes TLS pair-parent,
reservation and mutation counts at zero. Then consume a fresh artifact-only preflight decision before the
first external write and publish/read back P08's signed root dispatch, selected preflight range and descriptor
with distinct receipts. Record hashed host identity, Windows build/update state, Hyper-V capability, storage/network
isolation, baseline version, checkpoint/reimage reference, credential epoch, cleanup/reimage procedure,
the build slot/range and installer/applicable-TLS/Hyper-V pair-parent ordinals, typed empty actual-input
slots, stable reservation slots and resolver digests. Reject the general developer workstation and any physical host that
cannot be restored through the exact `hyperv_actual_vm` child. Verify no unrelated VM/service/listener is
in target scope. This range may read and publish evidence only; it cannot reserve or mutate the host. Every
later child reopens the byte-identical root receipt. If nested Hyper-V and independently controlled reimage
are unavailable, stop and revise the closed category design rather than performing an unclassified reset.
No new selector/resolver kind exists and no TLS parent remains unresolved. A post-build applicability mismatch
fails the campaign and requires a new classification/root/plan before any TLS mutation.

- [ ] **Step 2: Build aggregate and child Packets**

The original signed task-dispatch manifest published in Step 1—not the aggregate—freezes the build slot and
one initial `reversible_mutation_pair_plan_resolution` parent/stable slot for each actual installer,
applicable TLS and Hyper-V pair; its inapplicable shape freezes the TLS-N/A planning range instead. It contains
no future rollback/forward child, bundle, decision, reservation or receipt ID. Aggregate `campaign_summary`
only projects those immutable inputs and is non-executable.
Build the actual artifact first. Only afterward, immediately before each reversible pair, measure the exact
artifact/host/before-state and follow the same two-stage resolver protocol independently. Do not create all
child decisions up front.

When TLS is inapplicable, omit its pair and consume exactly one fresh
`tls_rehearsal_not_applicable` planning decision; TLS mutation decision/consume/reservation counts remain
zero. Reinstall is forbidden. Capture the installed current-card from the initial install before rollback,
then restore baseline/reimage. Every build, reservation, forward, rollback, abort-release, credential and
guard-close Packet has a distinct consumer. `brokered_code_change` is forbidden, and the campaign summary
is never consumable. Freeze the exact product version from the then-current version policy rather than this
plan.

- [ ] **Step 3: Consume and execute the build child without privilege**

The frozen command follows the installer build contract and exact Packet values, for example the
template below only after every metavariable is replaced:

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version <exact-approved-version> `
  -OutputRoot <exact-approved-output-root> `
  -SigningMode AllowUnsignedDev
```

Resolve the predeclared slot to one exact `package_service/build` range and create/approve a fresh build
request/decision. Immediately before creating the first output byte, consume only that decision; its
prebound finalizer publishes/readbacks the active child range, descriptor and signed resolution record/
receipt first. Then
confirm source clean, output hashes, payload/provenance, SBOM and notary. Build does not imply install
approval and does not claim public trusted signing.

- [ ] **Step 4: Resolve, reserve and execute the installer pair**

Freeze one signed installer `reversible_mutation_plan` over the actual MSI, host/baseline, complete restore
surfaces, forward/inverse argv and oracles; it binds only its initial parent/slot. The protected first
resolver emits exactly rollback child, `acquire_root` reservation template, stage-two forward parent and
abort-release template plus their signed envelope. Seal/read back the bundle, approve rollback unconsumed,
and require a sufficient capability horizon. Consume a fresh `mutation_guard_reserve` decision; its
prebound finalizer publishes/readbacks the active reservation range, descriptor, resolution and attachment-
promotion receipts, then E05/P07 atomically acquires/readbacks the stable root, installer guards and
`reserved` receipt. Approval alone writes nothing.

Only after that receipt, the stage-two resolver emits the sealed install child. A fresh forward decision
wins `consume_and_claim reserved -> forward_claimed`; its finalizer publishes the active child/descriptor,
resolution and attachment-promotion receipts, then the winning transaction terminalizes the still-open abort
template as `superseded_by_forward_consume` before the first install/service side effect. Install only the
prebuilt artifact and verify MSI lifecycle, service Running/Automatic, Web listener, PCVCLI and current-card. If
reservation succeeded but forward did not consume and effect count is zero, use a fresh abort-release
decision: one E05 `consume_and_claim` winner consumes it and moves `reserved -> release_claimed`, publishes/readbacks its active release range,
descriptor, resolution and attachment-promotion receipts, terminalizes the open forward subject as
`abort_release`, releases according to `acquire_root`, reads back and marks `released`.
Any partial/uncertain claim stays fenced for reconciliation.
Deterministic partial/started install failure immediately wins paired rollback
through the matching fresh decision's E05 `consume_and_claim`, moving
`forward_claimed|rollback_required -> rollback_claimed`, and publishes/readbacks its active rollback child range,
descriptor, resolution and attachment-promotion receipts before inverse, proves exact baseline readback and
reaches `restored`; the campaign fails and no next pair starts.
Uncertain state performs no automatic inverse and is reconciliation-only.

- [ ] **Step 5: Resolve and execute the applicable TLS pair**

If applicable, repeat Step 4 with a new initial parent and signed TLS plan. Its first resolution emits the
TLS rollback, `add_child_guards` reservation template, stage-two TLS forward parent and abort-release
template. Approve rollback first, consume a fresh guard-reserve decision to add/read back only disjoint TLS
guards beneath the installer root/fencing token only after its consumed finalizer publishes/readbacks the
active reservation range, descriptor, resolution and attachment-promotion receipts, then resolve the
forward child. A fresh forward decision wins E05 `consume_and_claim reserved -> forward_claimed`, publishes/readbacks its active
child range, descriptor, resolution and attachment-promotion receipts, terminalizes the
abort template as `superseded_by_forward_consume` and only then performs the first URL ACL/certificate/
firewall side effect. Capture exact before/after/readback. A pre-forward abort uses the winning
`remove_unused_child_guards` branch through its fresh decision's E05
      `consume_and_claim reserved -> release_claimed`, publishes/readbacks its active release range, descriptor,
      resolution and attachment-promotion receipts, terminalizes the open forward subject as `abort_release`,
      removes/readbacks only the child guards and reaches `released` while leaving the parent root/other guards
      unchanged. A losing/partial/uncertain abort claim writes no terminal/effect, leaves the fence in place and
      requires reconciliation. Same/cross-kind overlap or insufficient horizon blocks. If TLS is not applicable, preserve its one
      planning consume and prove zero TLS mutation decisions, reservations, consumes and commands.
Deterministic partial/started TLS failure immediately wins
the matching fresh rollback decision's E05 `consume_and_claim`, moving
`forward_claimed|rollback_required -> rollback_claimed`, and publishes/readbacks its active rollback child range,
descriptor, resolution and attachment-promotion receipts,
performs inverse, proves exact TLS baseline readback and reaches `restored`; the campaign fails and no next
pair starts. Uncertainty remains fenced and reconciliation-only.

- [ ] **Step 6: Resolve, execute and restore the targeted Hyper-V pair**

Repeat the same plan/four-output/rollback-approval/fresh-reservation/stage-two sequence for one exact
disposable VM/switch/disk surface set under `add_child_guards`; the consumed reservation finalizer publishes/
readbacks its active reservation range, descriptor, resolution and attachment-promotion receipts before
adding guards. A fresh Hyper-V forward decision wins
E05 `consume_and_claim reserved -> forward_claimed`, publishes/readbacks its active child range, descriptor, resolution and
attachment-promotion receipts, terminalizes the abort template as `superseded_by_forward_consume` and then
performs only the approved actual-VM operations/readbacks; package/service/TLS/unrelated
OS flags are false. After functional readback, the still-unconsumed rollback decision wins
E05 `consume_and_claim`, moving `forward_claimed|rollback_required -> rollback_claimed`, and publishes/readbacks its active rollback child range,
descriptor, resolution and attachment-promotion receipts and only then
performs cleanup. Exact zero-leftover/before-state readback yields `restored`. Failed/uncertain cleanup keeps
the campaign fenced, failed and reconciliation-only; it is not retried or reclassified as PASS.
A pre-forward Hyper-V abort uses a fresh E05 `consume_and_claim reserved -> release_claimed`, publishes the
full active release chain, terminalizes forward as `abort_release`, removes/readbacks only its child guards
and reaches `released`; losing/uncertain claim has zero effect and requires reconciliation.

- [ ] **Step 7: Restore remaining pairs and close the campaign root**

After successful Hyper-V restoration, or after any current required pair has reached a deterministic abort/
restore terminal state, start no later pair and reverse-restore every earlier successful pair. The matching
fresh TLS rollback decision wins E05 `consume_and_claim`, publishes/readbacks its active child range,
descriptor, resolution and attachment-promotion receipts before inverse, and exact URL ACL/certificate/
firewall readback yields `restored`. Do the same for installer and prove the original package/service/
listener baseline. No reinstall is permitted. Rotate external credentials only through their separate fresh
provider transaction. Every rollback decision/range is single-use; uncertainty or reimage remains failed
reconciliation. Any required pre-forward abort or deterministic started failure keeps the aggregate FAIL
even when cleanup succeeds.

Exactly one campaign reservation-domain terminal shape is valid. If the root remains active—including after
a TLS/Hyper-V child abort—consume one fresh `mutation_guard_close` decision; its finalizer publishes/readbacks
the active close range/descriptor, then E05/P07 CASes `open -> closing`, race-excludes add-child and reads back
closed. If the installer `acquire_root` pre-forward abort already released/read back the reservation, guards
and empty root, require that root-absent `released` receipt and guard-close consume count zero. If build or
pre-reservation failure means the root was never acquired, require a signed zero-host-effect/never-acquired
proof and guard-close consume count zero. The aggregate accepts exactly one of these three shapes. Missing,
mixed, partial or uncertain state leaves fences/reconciliation active and performs no synthetic close. Every
shape also binds either a zero-materialized-credential/count proof or the receipt from a separate fresh
credential rotation/revocation transaction and exact invalidation readback.

- [ ] **Step 8: Project campaign result**

Before the first independent aggregate/result/evidence-candidate publication, create/approve/consume a
fresh `trust_root/execution_authorization`, `execution_scope=artifact_only` Packet binding the exact terminal child/result/
reservation-domain-terminal-shape/restoration locators, projection tools, output prefixes/providers/retention, create-only
readback/notary, expiry and abort/reconciliation. It permits no new child, host or tracked operation; each
child's raw finalizer remains part of that child's already-consumed mutation transaction.

For every child, first run and preserve raw outputs for `spec-contract`, `scope-integrity`,
`product-verification`, `independent-verifier`, `quality-ratchet` and `security`. Publish/read back/notarize
those artifacts, then and only then evaluate the seventh `artifact-attestation` gate. After all seven
actual gates, generate the result Packet and Dashboard projection. Aggregate only verified children.
Required install+service+Hyper-V+rollback must all PASS with exact artifact/host/command and restore
proof. One blocked/failed/stale child keeps campaign failed and product promotion closed.

In addition, project a fresh four-role typed current-evidence candidate for the exact rehearsed internal
admin-smoke version: package, full admin host mutation, actual-VM functional correctness and installed
operator-surface current-card. Every role must point to accessible content-addressed WORM raw artifacts,
store readback and independent notary receipts. P08 publishes these immutable candidate bytes only; it
does not edit repository current evidence or supersede 0.42.65. Public trusted signing and external
stable publication remain false.

After publishing and independently reading back the complete P08 campaign/result/evidence candidate,
execute P08's canonical state-ledger closure before P09.

## Task NHA-P09: Independently reproduce and prove pre-activation readiness

**Files:** the fresh four-role evidence Markdown/sidecars, `docs/ga-ready/current-evidence.json`, its six
bounded generated projections and generator-owned indexes/blocks, plus canonical
`docs/superpowers/plans/luna-completion/traceability.json`, `trust-dashboard.json` and
`TRUST_DASHBOARD.md` only. The v2 schemas/generators are reused, not weakened; historical 0.42.65
documents and raw-accessibility facts are never rewritten.

Before those tracked ranges, external-only ranges own the second clean-host/VM preparation, provider
credential transactions, build/install/TLS-or-N/A/Hyper-V/rollback children, closed
root/guards and second aggregate evidence. They create no project product/package commit, but they do
perform the separately authorized external mutations listed below; the tracked Files list is not a claim
that P09 has no host or package activity.

- [ ] **Publish an outer-controller preflight and choose the environment without mutation.** Freeze provider-
      signed, independently verified read-only environment and TLS-applicability classifications as pre-request
      inputs before the task dispatch is signed; they perform no publication, provider administration or host
      effect and select exactly already-clean/fallback and TLS-applicable/TLS-N/A. Build the one branch-specific
      root manifest from those immutable facts, then consume a fresh artifact-only publication decision
      and publish/read back P09's sole immutable task-dispatch root/receipt
      and the selected outer-preflight range/descriptor receipts. Bind the physical controller, disposable
      verification VM/image/checkpoint/
      switch/disk facts, independent credential epoch, and an explicit branch: already-clean second host or
      fallback reimage. In the fallback shape the manifest contains the outer
      `reversible_mutation_pair_plan_resolution` parent; in the already-clean shape it contains the signed
      classification fact and fixes outer parent/host-root/reservation/mutation counts at zero, with no outer
      deferred parent. Independently, the TLS-applicable shape predeclares the inner TLS pair ordinal, initial
      parent, typed actual-input slot, stable reservation slot and resolver digest; the TLS-N/A shape instead
      contains the exact TLS-N/A planning range and fixes TLS pair-parent/reservation/mutation counts at zero.
      Both shapes predeclare the inner build slot and every actual installer/Hyper-V pair ordinal and leave
      future values empty. No new selector/resolver kind is introduced and no parent remains unresolved. A
      post-build TLS-applicability mismatch fails/replans before any inner mutation. This range
      has no reset/reservation capability. An unclassified
      physical-host reset is forbidden.
- [ ] **If fallback is selected, prepare it under an outer pair/root.** Use a predeclared outer preparation
      parent and the full P08 two-stage protocol: actual signed Hyper-V preparation plan, four-output bundle,
      unconsumed preparation rollback approval, fresh `acquire_root` reservation decision/receipt/horizon,
      stage-two preparation forward and fresh E05 `consume_and_claim`. The winner publishes/readbacks the
      active forward child range, descriptor, resolution and attachment-promotion receipts, terminalizes its
      abort template as `superseded_by_forward_consume` and only then starts reimage. Only successful
      preparation leaves the outer rollback unconsumed while inner work runs. Pre-forward denial uses
      winner-only abort release whose finalizer publishes/readbacks its active range, descriptor, resolution
      and attachment-promotion receipts, terminalizes forward as `abort_release`, releases/readbacks the
      outer host root/guards and reaches `released`; a losing/uncertain claim has zero effect and reconciles.
      Deterministic started preparation failure immediately lets the paired outer
      rollback win consume-and-claim, publishes/readbacks its active child range, descriptor, resolution and
      attachment-promotion receipts before inverse, proves pre-preparation readback and
      `restored`, fails P09 and never starts inner work; uncertainty remains fenced reconciliation. This is an
      outer **host-reservation** root, not a
      second task-dispatch root. Its owner/fencing/guards/credentials are never
      inputs to an inner mutation decision.
      The outer controller records exactly one mutually exclusive early terminal branch: pre-reservation
      failure proves never-acquired/zero outer host effect and has rollback/close consume counts zero;
      `acquire_root` pre-forward abort proves `released`/root absent and has rollback/close counts zero;
      the just-completed paired rollback in the deterministic started-failure path is its sole rollback consume;
      that path rotates any materialized outer credential through its separate fresh transaction and closes/readbacks the still-open
      outer root through one fresh guard-close decision, fails and never starts inner work; uncertainty remains
      fenced for reconciliation. Only successful preparation keeps the outer rollback unconsumed and may enter
      the inner campaign. Each deterministic early branch binds either a zero-materialized-credential/count
      proof or a separate fresh outer credential rotation/revocation receipt with exact invalidation readback
      into its terminal evidence. None of these early branches later enters the common outer-rollback path.
- [ ] **After boot, establish an independently signed inner campaign range and reservation domain.** A
      different Sol trust domain
      consumes a fresh artifact-only inner preflight, independently reads guest identity, clean baseline,
      credential epoch, isolation and restore surfaces. It reopens the byte-identical P09 task-dispatch root
      and original receipt, then publishes only its own inner-preflight range/descriptor and applicable
      resolution receipts; task-root recreation is forbidden. The first inner guard-reserve later acquires a
      distinct inner **host-reservation** root whose owner, fencing token, epoch, namespace and credentials
      differ from the outer preparation root and P08. A cross-root receipt, guard, nonce, decision, TLS-N/A
      authority or writable-state reuse invalidates reproduction. If no fallback was needed, the inner host-
      reservation root is still mandatory for every gate-eligible/successful inner host campaign; a failure
      before its first acquisition may produce only the `never_acquired` cleanup-only terminal shape below.
- [ ] Perform a second clean deterministic build rather than merely rehashing the first artifact.
      Resolve its predeclared build slot to a separate signed build child, create/approve its fresh exact
      `package_service/build` request/decision and consume that decision immediately before output;
      its finalizer publishes/readbacks the active child range, descriptor and signed resolution record/
      receipt before the first output byte. Build has no host
      guard or rollback reservation. Compare normalized relative path/mode/content manifests, MSI/signature class, payload aggregate,
      SBOM and provenance. Any permitted volatile field must be enumerated in the approved allowlist and
      semantically compared; unexplained byte/class drift fails NHR-016.
- [ ] Using only the second build, execute new inner installer, applicable TLS and functional Hyper-V pairs
      sequentially. Each starts from its own predeclared parent and actual signed plan, emits the exact four-
      output bundle, approves rollback unconsumed, consumes a fresh inner guard-reserve decision with horizon,
      whose finalizer publishes/readbacks its active reservation range, descriptor, resolution and attachment-
      promotion receipts before the CAS; it resolves the stage-two forward and wins a fresh E05
      `consume_and_claim`, publishes/readbacks the active forward child range, descriptor, resolution and
      attachment-promotion receipts, terminalizes the abort subject and only then starts the first side
      effect. Host pairs share only the inner root/fencing token through
      exact disjoint delegation. The first inner reversible pair uses `acquire_root`; later disjoint pairs use
      `add_child_guards`. If forward is denied, expires or fails before effect, a fresh abort-release decision
      wins one E05 `consume_and_claim`, publishes/readbacks its active release range, descriptor, resolution and attachment-
      promotion receipts, terminalizes the most-materialized open forward subject with reason
      `abort_release`, uses respectively root release or child-guard removal, verifies exact release readback
      and reaches `released`. A losing/partial/uncertain claim writes no terminal/effect and is reconciliation-
      only. Any required-pair abort fails reproduction and starts no later pair. If it was a later child,
      reverse-restore earlier successful inner pairs before closing; if it was the first `acquire_root`, its
      root-absent release receipt is the inner terminal proof. A winning forward uses
      terminal reason `superseded_by_forward_consume`; deterministic started failure invokes only the
      paired rollback, while uncertainty is reconciliation-only. TLS N/A uses one new planning consume and zero TLS mutation records.
      Capture current-card, then restore Hyper-V, TLS and installer in that order: each fresh matching
      rollback decision wins one E05 `consume_and_claim`, publishes/readbacks its active child range,
      descriptor, resolution and attachment-promotion
      receipts before inverse and proves `restored`. Reinstall is forbidden.
      A deterministic started failure instead immediately consumes and claims only the paired rollback, then
      completes its publication/inverse/
      baseline-readback path, fails reproduction and forbids the next pair; uncertainty is reconciliation-only.
      Exactly one deterministic inner reservation-domain terminal shape is valid. A root-acquired campaign
      reverse-restores every started/successful pair, rotates its inner credential through the separate fresh
      transaction, consumes a fresh `mutation_guard_close` decision, publishes/readbacks its active close
      range/descriptor, atomically CASes `open -> closing` and reads back `closed`; its outcome is `pass` only
      when all required inner build/pairs/readbacks passed, otherwise `failed_cleanup`. Before any root
      acquisition, use a signed `never_acquired`/zero-inner-host-effect proof with close count zero. After the
      first `acquire_root` abort, use its `first_abort_released`/root-absent receipt with close count zero.
      Each shape also binds either a zero-materialized-inner-credential/count proof or a separate fresh inner
      credential rotation/revocation receipt with exact invalidation readback; that proof/receipt is part of
      the terminal evidence and any eligible gate input. Missing/mixed/uncertain state keeps fences active and
      fails reproduction. Only `closed/pass` plus every
      required inner build/pair/restore PASS is gate-eligible; `closed/failed_cleanup`, `never_acquired` and
      `first_abort_released` are cleanup-only failure evidence and must never feed a gate, attestation or PASS.
      For `closed/pass` only, create/approve/consume a distinct fresh artifact-only six-gate decision binding
      the exact inner terminal receipts, protected gate argv, raw output prefixes, provider/retention/readback/
      notary and, conditionally, either the still-active fallback outer host-root/fence locator or the signed
      already-clean classification/readback with outer host-root count zero. Run the first six gates and
      immediately publish/readback/notarize their raw outputs under that authority before any outer restore;
      do not evaluate artifact-attestation yet.
- [ ] If fallback preparation completed successfully, consume its separate outer rollback exactly once only
      after the inner terminal result is independently read back. Publish/readback its active child range,
      descriptor, resolution and attachment-promotion receipts and restore the pre-preparation image/VM state.
      Complete the separate outer credential rotation/readback first, then consume one outer
      `mutation_guard_close` decision, publish/readback its active close range/descriptor, atomically CAS
      `open -> closing` and close/readback only the outer root/guards with that credential receipt as an input.
      When the inner result is `closed/pass`, this happens after the six raw gates and may continue toward the
      seventh gate. Any deterministic inner failure terminal shape permits this outer cleanup and failed-result
      evidence only, starts no six/seventh gate and then stops P09. An inner uncertainty may invoke outer
      reimage only under a fresh reconciliation decision and always remains a failed campaign; it can never
      manufacture inner `restored` or PASS. The no-fallback branch proves outer parent/root/rollback/
      credential/close counts zero and performs no outer cleanup.
- [ ] Only for an inner `closed/pass` result, after every applicable host-reservation root and credential has
      its selected terminal readback, reopen the already immutable six-gate WORM/notary receipts and validate
      them as inputs. Do not evaluate or publish artifact-attestation yet. Missing selected outer terminal
      proof keeps the seventh gate ineligible; raw gate evidence is never deferred or regenerated to fit the
      result.
- [ ] Before producing the second four-role aggregate/result set, create/approve/consume a fresh
      `trust_root/execution_authorization`, `execution_scope=artifact_only` Packet binding the exact second-
      campaign child/result/restoration/root-and-guard-close locators, the six immutable gate receipts,
      artifact-attestation/projection tools, four-role output
      prefixes/providers/retention, conditional-create/readback/notary, expiry and abort/reconciliation.
      It permits no additional host/provider/tracked mutation, and its `finalizer_policy` prebinds only
      these seventh-gate/aggregate/evidence outputs. Under this consumed authority, evaluate
      artifact-attestation first; only PASS may feed the aggregate.
      The aggregate preserves the selected inner terminal shape, TLS-applicable/TLS-N/A shape, fallback/no-
      fallback branch and exact outer-root terminal shape/count; it cannot normalize a zero-count branch into
      an omitted unresolved parent. Only inner `closed/pass` with the successful-preparation-restored-and-
      closed outer shape, or inner `closed/pass` with the no-fallback zero-count shape, is eligible.
- [ ] Produce a second accessible WORM/notarized four-role evidence set and compare it with P08's set on
      exact source/version/recipe, normalized payload/provenance, functional outcomes, installed
      current-card contract and permitted volatile-field allowlist. Any unexplained mismatch blocks; do
      not select whichever run looks greener.
- [ ] Only after that immutable comparison/result exists, run the protected evidence-candidate resolver.
      It emits the new signed exact child range for the fresh evidence documents/sidecars, current-evidence
      record and generator-owned projections; no earlier dispatch may guess or widen those operations.
      From fresh `main`, generate and consume a separate `packet_type=trust_root`,
      `phase=execution_authorization`, `execution_scope=tracked_change` decision before the broker creates
      the current-evidence candidate.
      The exact path set is limited to the new evidence documents/sidecars, current-evidence record and
      generator-owned bounded projections. Run the existing current-evidence generator/validator—no
      hand editing—to add the fresh
      accessible role set as `evidence_status=current`, `completion_required=true` and a signed/notarized
      one-to-one supersession record. It links every old 0.42.65 role ID/digest to its exact replacement
      and changes the old entries only to `historical`, `completion_required=false`; their original
      result, limitation and unavailable-artifact facts remain unchanged and visible. Traceability and
      Dashboard paths are forbidden in this release candidate.
- [ ] Run actual Release, independent Sol verification, all seven gates, byte-identical regeneration and
      the complete supersession-negative corpus on that exact candidate. Then create/consume a distinct
      `packet_type=promotion`, `phase=landing_authorization` decision binding the exact evidence artifact/
      provenance/current-state candidate, change-set, actual Release/independent-Sol/seven-gate evidence,
      supersession proof and the unchanged public-claim boundary; land unchanged through the live Landing
      Authority. Fresh-main post-merge attestation must prove exactly one complete current required role
      set, all its raw proof accessible, and every old role still present as historical. This promotion
      advances only the internal admin-smoke operational evidence anchor; it does not claim public trusted
      signing or external stable publication. P10's later successor activation pointer remains a separate
      `release_change/landing_authorization` transaction and cannot reuse this promotion decision.
- [ ] Only after that release candidate is merged and attested, start a separate fresh-main
      traceability-only candidate. Resolve its one exact signed child range from the immutable evidence
      landing/readback, then generate/consume a new `trust_root/execution_authorization` bound to that
      digest, run the
      v4 traceability generator/validator and broker only canonical `traceability.json`, replacing every
      formula-required NHR planned edge with its actual case/execution/verification/Packet/decision/
      landing locator. P10 activation-lineage edges remain planned and explicitly outside the NHR
      completion set. Run actual Release, independent Sol/all seven gates and direction/orphan/planned-
      as-PASS negatives; then obtain/consume a separate `trust_root/landing_authorization`, land unchanged
      and reread fresh post-merge main. NHR-001..030 actual bidirectional coverage must be 100%, every NHR
      PASS and orphans 0 before P10.
- [ ] Re-run all known-bad/verifier-negative controls; rejection 100%.
- [ ] Re-query and bind the full active decision provider/App/environment/principal/issuer/key/workflow
      profile, L04 cutover digest and revocation state; require the L09 subject/expiry/stale predicates to
      remain current at this exact pre-activation main.
- [ ] Only after both the current-evidence and traceability PRs are merged and post-merge attested, start
      a third fresh-main Dashboard-only candidate. Generate/consume a new
      `trust_root/execution_authorization` for the newly resolved signed range containing exactly
      `trust-dashboard.json` and
      `TRUST_DASHBOARD.md`; deterministically regenerate both from canonical traceability, accessible
      current evidence, server facts, pilots, campaign/reproduction and packet-only exercise. The JSON
      is authoritative and Markdown byte-derived. It may contain no future Dashboard-PR merge SHA or
      self-attestation. Run actual Release, independent Sol, all seven gates and manual-edit/byte-
      regeneration negatives, then obtain/consume a distinct `trust_root/landing_authorization`, land
      unchanged and verify both files from fresh post-merge main.
- [ ] Evaluate every source-design §18 term. Any inaccessible/stale
      completion-required proof, invalid/partial supersession, unresolved ambiguity, waiver, server
      drift or user-exercise failure keeps `overall_readiness=red`. Historical inaccessible 0.42.65 proof
      remains RED historical and is excluded from the current completion formula only by the validated
      supersession above. `overall_readiness=green` and the full assurance-environment formula must be
      true before P10; this claims environment completion, not successor activation or product completion.
- [ ] Before the independent final preactivation aggregate publication, create/approve/consume a fresh
      artifact-only `trust_root/execution_authorization` binding exact current-evidence/traceability/
      Dashboard/server/profile inputs, projection tool, output prefix/provider/retention/readback/notary,
      expiry and abort/reconciliation, with zero tracked/provider-admin/host mutation. Publish
      `NHA-P09-assurance-environment-complete-preactivation-v1` only when every §18 term is true
      and the canonical tracked Dashboard deterministically reads `overall_readiness=green`. P10
      activation-lineage edges may remain honestly planned because they are outside that formula. Then
      execute P09's canonical state-ledger closure; P10 cannot start before its fresh-main readback.

## Task NHA-P10: Activate successor v4 through two exact release-change Packets

**Files:** only exact activation pointer/state/index paths owned by successor v4. No product, workflow,
current operational evidence, package or host path.

- [ ] **Step 1: Build, verify, land and attest `pending_activation` without changing effective current**

From latest main, bind P09 completion, live Landing Authority, exact v4/materialization inactive anchor,
all NHR proof, current operational/public claims, exact planned operations and rollback/recovery
implications. First obtain/consume a pre-execution decision with `packet_type=trust_root` and
`phase=execution_authorization` before the broker creates the
pending candidate; it claims no PASS. Add/update only v4-owned `pending_activation` state and leave the
effective-current pointer unchanged. Run actual Release plus independent Sol/seven gates, then generate
a post-verification `release_change` Packet bound to the exact candidate/proof. Obtain/consume its new
decision, land through the Landing Authority, and independently attest fresh main. Publish
`NHA-P10-pending-activation-attested-v1`. Earlier approvals cannot be reused.

- [ ] **Step 2: Create a new pointer-only activation request**

From a later fresh main, obtain/consume a new pre-execution decision with `packet_type=trust_root` and
`phase=execution_authorization` for only the exact
effective-current pointer/state/index operations, then build the pointer-only candidate. Scope guard
proves no product, workflow, GA/current evidence, package or host diff. Run actual Release, independent
Sol review and all seven gates on the exact latest-base candidate. Only then generate the second
post-verification `release_change` Packet binding the P10 pending attestation, exact candidate/diff,
actual proof, live server facts and recovery implications, and obtain a new authenticated decision.

- [ ] **Step 3: Consume, land and post-merge verify the pointer transition**

Use the Landing Authority, latest-base equivalence and one-time decision consumption. From fresh merged
main, independently verify final tree, pointer/state consistency, v4 locator ancestry, operational
claims and server policy. The pointer becomes effective only when this second PR lands; the prior pending
attestation is a required precondition, not the effective switch. A post-merge failure does not rewrite
history: it sets automatic landing off/recovery blocked and requires a separate recovery Packet.

- [ ] **Step 4: Finalize actual traceability and keep product promotion separate**

After the pointer's raw landing attestation, create/approve/consume a fresh artifact-only
`trust_root/execution_authorization` binding exact activation/cutover/Packet/decision/consume/final-
attestation reads, projection/check tools, output prefix/provider/retention/readback/notary, expiry and
abort/reconciliation with zero tracked/product/host mutation. The pointer landing decision cannot
authorize this independent publication. Under that authority, in the external immutable activation-
lineage projection, supersede only the P10 planned edges with actual
Packet/decision/consume/candidate/final-attestation locators, re-run bidirectional coverage/orphan
checks for that extension, and publish `NHA-P10-successor-v4-activation-complete-v1`. It does not alter
the already-complete NHR-001..030 environment traceability. This step writes no repository file;
any later desire to materialize those actual locators into repository `traceability.json` is a separate
third trust-root PR/Packet outside this two-PR activation. Activation does not promote a new MSI/current
operational version, public signing or external stable publication. Those require their own exact
promotion or mutation Packets later; the activation `release_change` decision cannot authorize them.

- [ ] **Step 5: Close the canonical P10 state after activation**

Only after the pointer transition's final post-merge activation attestation is immutable, execute the
canonical state-ledger closure for P10. The state-only candidate changes P10 from the exact
`landing_pending` state to `completed` and binds the activation attestation locator; it cannot change or
roll back the effective-current pointer. Plan 6 exit requires this final fresh-main readback.

If post-merge attestation fails, set automatic landing off and recovery blocked. Do not silently restore
the predecessor or lower a gate; execute a separately approved L/Sol/Release recovery card.

## Plan 6 exit gate

- [ ] Successor v4 control materialization is complete, exact and was inactive until P10.
- [ ] Three pilots include S and M, at least two real product changes, independent verification and zero
      waivers.
- [ ] Packet-only APPROVE, DENY and REQUEST-CHANGES exercise plus replay/invalidation tests pass.
- [ ] Build/install-service/Hyper-V/rollback children have separate exact decisions and restore proof;
      TLS is either separately passed or validly frozen N/A.
- [ ] Independent clean reproduction uses a second build and a second complete child Packet/decision/
      consume set.
- [ ] NHR-001..030 actual traceability is 100%, orphan 0 and every assurance-environment formula term was
      true at P09 before activation; P10 adds only separately labeled activation lineage.
- [ ] Live server enforcement remains valid and `overall_readiness=green` before activation.
- [ ] Activation uses attested pending state followed by a fresh pointer-only `release_change`,
      latest-base attested and post-merge verified.
- [ ] Product public signing/publication remains unclaimed unless separately proven.
