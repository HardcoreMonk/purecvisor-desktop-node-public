# PureCVisor Bounded Executor and Independent Verifier Implementation Plan

> **Status:** proposed child plan 3; Plan 2 shadow trust-root attestation is mandatory input.
>
> **Execution class:** controller, scope, confinement, broker, runner and verifier changes are trust-root
> `L`, `gpt-5.6-sol`, `ultra`, actual `Release`, with a different trust-domain Sol verifier. Product
> execution is not opened by this plan.

**Goal:** Ensure an implementation model can read the frozen repository but can change only exact
approved files/operations, cannot alter the trust root or oracle, and cannot control its independent
verification target or result.

**Architecture:** A trusted Git scope calculator seals exact object-level diff intent. An authenticated
model-dispatch controller outside the egress-denied runtime sends only frozen read-only context and
mediates every model tool request. The executor sees a read-only repository and submits exact
create/modify/delete operations to a separate write broker. After each accepted batch the controller
publishes a new immutable candidate-tree snapshot so later model/test turns see cumulative work without
receiving a writable checkout. A disposable non-root OCI executor and a separately provisioned,
non-admin Windows verifier record operating-system telemetry. Verification Authority independently
selects and signs the exact target, mints one-run verifier credentials, and dispatches the exact closed
Fast/Full/Release lane from a different provider/App/environment trust domain. Plan 3's own trust-root
changes use Release.

The closed canonical work-class route used everywhere in this plan is exactly:
`S -> Fast / gpt-5.6-luna / max`, `M -> Full / gpt-5.6-luna / max`, and
`L|trust_root|release -> Release / gpt-5.6-sol / <card-frozen high|ultra>`. These case-sensitive enum
values and routes admit no aliases, omitted class, caller override or downgrade.

**Prerequisite:** `NHA-T08-shadow-trust-root-v1` is valid and all Plan 2 commits are exact `main`
ancestors. The Plan 2 protected-path mechanism is operational before X00. Current server enforcement
may still be RED; this plan does not land automatically.

**Source design:** `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`
§§7, 8.4, 10, 11, 12 and NHR-005..010, NHR-013..014.

---

## File map

**Trusted scope**

- Create `packaging/windows-desktop-node/tools/PcvAssuranceScope.psm1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceScopeAllowance.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceScopeManifest.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceScope.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceScope.Tests.ps1`.
- Create the exact scope fixture files listed in the fixture manifest below.
- Create `docs/superpowers/plans/luna-completion/contracts/scope-allowance.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/scope-manifest.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/task-authority-chain.schema.json` in X03 and
  modify Plan 2's `docs/superpowers/plans/luna-completion/contracts/execution-manifest.schema.json` to
  require it.
- Create `docs/superpowers/plans/luna-completion/contracts/model-dispatch.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/model-result.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/dependency-selection-binding.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/model-provider-policy.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/result-transport-policy.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-authority.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-dispatch.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/result-publication.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/result-transport-receipt.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/verified-result.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/telemetry-event.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/telemetry-summary.schema.json`.
- Create `docs/superpowers/plans/luna-completion/verification-authority.json` only after the external
  authority choice is approved.
- Create `docs/superpowers/plans/luna-completion/dependencies/authenticated-model-boundary.json` in X04
  only from its applicable final selection chain.
- Create `docs/superpowers/plans/luna-completion/dependencies/result-transport.json` in X06 only from its
  applicable final selection chain.

**Confinement and write broker**

- Create `docs/superpowers/plans/luna-completion/runner-images.schema.json`.
- Create `docs/superpowers/plans/luna-completion/runner-images.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/runner-build-command-manifest.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/runner-build-attestation.schema.json`.
- Create `tools/assurance/runner/Dockerfile.executor` only after its base image digest is frozen in a
  trust-root Packet.
- Create `tools/assurance/runner/windows-verifier-image.pkr.hcl` and
  `tools/assurance/runner/Build-PcvWindowsVerifierImage.ps1` only after the Windows image/provider
  inputs are frozen.
- Create `tools/assurance/runner/Build-PcvAssuranceRunnerImages.ps1` as the protected two-build
  OCI/Windows orchestrator.
- Create `tools/assurance/runner/New-PcvAssuranceRunnerBuildManifest.ps1`.
- Create `tools/assurance/runner/New-PcvAssuranceRunnerImages.ps1` as the deterministic measured-
  attestation-to-record generator.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceConfinement.psm1`.
- Create `packaging/windows-desktop-node/tools/Initialize-PcvAssuranceWorkspace.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceConfinement.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceWriteBroker.psm1`.
- Create `packaging/windows-desktop-node/tools/Start-PcvAssuranceWriteBroker.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceConfinement.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceWriteBroker.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceTelemetry.psm1`.
- Create `packaging/windows-desktop-node/tools/Start-PcvAssuranceTelemetry.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceTelemetry.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceRunnerImages.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceRunnerImages.Tests.ps1`.

**Process execution and independent verification**

- Reuse Plan 2's protected `planned-command-descriptor.schema.json`,
  `bootstrap-command-run.schema.json`, `bootstrap-command-manifest.schema.json`,
  `New-PcvAssurancePlannedCommandDescriptor.ps1`, `Invoke-PcvAssuranceBootstrapCommand.ps1`,
  `New-PcvAssuranceCommandManifest.ps1` and the exact
  `PcvAssurancePlannedCommandDescriptor.Tests.ps1`,
  `PcvAssuranceBootstrapCommandRunner.Tests.ps1` and `PcvAssuranceCommandManifest.Tests.ps1`; Plan 3
  does not redefine their contracts.
- Reuse Plan 2's protected `bootstrap-exit.schema.json`,
  `bootstrap-artifact-publication-receipt.schema.json`, bootstrap exit generator/validator, conditional-
  create bootstrap artifact publisher/readback validator and their exact focused tests.
- Reuse Plan 2's protected `bootstrap-import-candidate-manifest.schema.json`,
  `New-PcvAssuranceBootstrapImportCandidateManifest.ps1`,
  `Test-PcvAssuranceBootstrapImportCandidateManifest.ps1` and focused tests without redefining them.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceModelDispatch.psm1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceModelDispatch.ps1` as the protected
  internal adapter invoked only by the live bounded-executor session; direct pre-session use rejects.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceModelDispatch.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceDependencyBinding.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceDependencyBinding.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceDependencyBinding.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceVerifierModel.psm1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceVerifierModel.ps1` as the remote
  Authority-only read-only verifier-model adapter.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceVerifierModel.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceProcessRunner.psm1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvBoundedExecutor.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceProcessRunner.Tests.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvBoundedExecutor.Tests.ps1`.
- Create `docs/superpowers/plans/luna-completion/contracts/recovery-drill.schema.json`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceRecoveryDrill.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceRecoveryDrill.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvIndependentVerifier.psm1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvIndependentVerifier.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvIndependentVerifier.Tests.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvVerificationAuthority.psm1`.
- Create `packaging/windows-desktop-node/tools/New-PcvVerificationDispatch.ps1`.
- Create `packaging/windows-desktop-node/tools/Submit-PcvVerificationAuthorityDispatch.ps1`.
- Create `packaging/windows-desktop-node/tools/Receive-PcvVerificationAuthorityRun.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvVerificationAuthority.Tests.ps1`.
- Modify Plan 2's `verification-target.schema.json`, `verification-target-issuer.schema.json`,
  `verification-target-issuer.json`, `New-PcvVerificationTargetManifest.ps1`,
  `PcvVerificationTargetManifest.Tests.ps1`, and `PcvDevelopmentVerificationExecution.Tests.ps1` in
  X06 to add the separately approved `verification_authority` issuer class.
- Create `packaging/windows-desktop-node/tools/Publish-PcvAssuranceResultObject.ps1`.
- Create `packaging/windows-desktop-node/tools/Import-PcvAssuranceResultObject.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceResultTransport.Tests.ps1`.
- Create `.github/workflows/assurance-verifier.yml`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceVerifierWorkflow.Tests.ps1`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-delivery-receipt.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-run-receipt.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/verification-artifact-manifest.schema.json`.
- Create the exact confinement fixture files listed below.

**Exact fixture manifest**

Under `packaging/windows-desktop-node/tests/fixtures/assurance-scope/`, create exactly:

```text
valid-single-modify.json
add.json
modify.json
delete.json
mode-change.json
rename.json
false-base.json
omitted-path.json
missing-object.json
case-collision.json
traversal.json
absolute-path.json
ntfs-ads.json
symlink-escape.json
protected-path.json
protected-symbol.json
dirty-untracked.json
allowance-result-field.json
allowance-new-blob.json
allowance-caller-path.json
```

Under `packaging/windows-desktop-node/tests/fixtures/assurance-confinement/`, create exactly the
seventeen X07 negative files plus `positive-single-write.json`. Every negative file has a same-base-name
`.expected.json` beside it. No directory entry or future wildcard is an approved create operation.

Under `packaging/windows-desktop-node/tests/fixtures/assurance-runner-build/`, create exact valid
`command-manifest.json` and `two-clean-attestation.json` plus invalid/expected pairs
`floating-command`, `provider-reuse`, `workspace-reuse`, `output-digest-mismatch`,
`missing-raw-artifact` and `unsigned-attestation`. The first two invalid cases target the command schema;
the remaining four target the attestation schema or its semantic verifier.

Under `packaging/windows-desktop-node/tests/fixtures/assurance-verifier-model/`, create a valid read-only
review plus invalid/expected pairs `same-implementation-actor`, `target-override`, `write-tool`,
`broker-request`, `lane-downgrade`, `model-mismatch`, `unsigned-result` and `verdict-injection`.

All new module, script, workflow, contract, image and fixture paths are added to Plan 2's protected-path
manifest by X00 before their implementation PR. That manifest update is itself an L/Release trust-root
result.

## External infrastructure and authority selection gate

Requirement Authority generates the exact closed dependency Packets at the first eligible boundary:
before X02 it selects `oci_executor`, `windows_verifier`, `authenticated_model_boundary` and
`result_transport`; only after X05 is an immutable protected-main ancestor does it select
`verification_authority`, because that subject must bind X05's actual workflow ref/blob digest. Each uses
`packet_type=requirements_approval`, `phase=planning_authorization`, purpose
`dependency_selection`: one Packet per named dependency below, never one mixed multi-dependency Packet.
Each Packet has the Plan 2 `selection_stage` appropriate to that dependency and one immutable
`selection_subject_id` from `oci_executor`, `windows_verifier`, `verification_authority`,
`authenticated_model_boundary`, `result_transport`. The telemetry capabilities in item 3 are bound by
their owning OCI/Windows subjects and are not an open sixth subject. Each Packet freezes:

1. a non-root OCI executor provider/runtime, digest-pinned base image, package snapshot, recipe
   specification, exact image build argv/capability policy and predeclared output slots;
2. a disposable non-admin Windows verifier image/provider able to run the repository's actual current
   `Release` lane without Hyper-V, installer, service or host mutation privileges;
3. operating-system telemetry collectors and policies: seccomp/cgroup plus fanotify/eBPF-equivalent
   evidence for OCI, and an approved process/file/network collector for Windows;
4. a Verification Authority App/service/environment in a trust domain distinct from implementation:
   either existing immutable provider/App/environment IDs, issuer public key and signed provider readback,
   or desired stable locators/names/create inputs/argv plus predeclared provider-assigned ID/key output
   slots; in both cases bind the protected default-branch workflow reference digest, allowed caller/ref
   and one-run credential mint/revoke policy; and
5. the authenticated model provider/controller boundary, allowlisted endpoints, provider identity,
   request/result schemas, retention, and either a provider-issued signed result receipt or a protected
   adapter signature over authenticated raw provider request/response bytes; and
6. a create-only content-addressed Git result transport with separate publisher/Authority-read
   credentials, conditional-create/readback receipt and retention long enough for Plan 4 import.

Because X02 must materialize two new clean runner outputs per platform, the `oci_executor` and
`windows_verifier` subjects use `selection_stage=create_identity_intent` even when their provider/runtime
and base inputs already exist. Their measured image/SBOM/filesystem/output identities are frozen only by
their later `post_create_identity_freeze` Packets. An `existing_identity` runner subject is valid only for
a truly preexisting immutable image and therefore cannot satisfy X02's mandatory two-clean-build proof.

Every selected external service/App/environment/key uses exactly one branch. `selection_stage=existing_identity` requires
the exact assigned IDs/key/revocation facts and current signed provider readback before approval and
authorizes no create. `selection_stage=create_identity_intent` contains only desired stable locators, immutable inputs, exact
create argv/capabilities and typed output slots; future provider-assigned IDs or keys are forbidden. After
an authorized create/readback, Requirement Authority approves and consumes a new
`requirements_approval/planning_authorization`, `purpose=dependency_selection`,
`selection_stage=post_create_identity_freeze` Packet that freezes the
measured IDs/key/revocation and creation/readback receipts for the named downstream consumer. It does not
amend or backfill the first request. No X task may consume a newly created identity until this second
freeze exists, and intent/post-create subject IDs plus parent decision digest match exactly. One subject's
decision cannot satisfy another.

Before any first runtime consume, the protected dependency-record generator opens the applicable final
selection request/decision/consume and emits a signed subject record. `existing_identity` binds its
provider readback; a created identity binds the intent, provider-administration request/decision/consume,
ordered forward/readback/revocation/selected-rollback receipts and `post_create_identity_freeze` as one
chain digest. Every provider/transport/Authority policy record embeds that final decision digest,
`selection_subject_id`, provider/key/readback digests and chain digest. Runtime entrypoints require
`-DependencySelectionDecisionPath` and independently prove the supplied final decision is either
`existing_identity` or `post_create_identity_freeze`, never `create_identity_intent`, and equals the
policy record. Dispatch/result, publication/receipt, Authority target/delivery/run and X09 inventory all
carry the same subject and chain digest. Wrong subject, intent-as-final, copied IDs, readback drift or
cross-dependency replay rejects.

Verification-Authority provider setup, if its selection branch is `create_identity_intent`, likewise
occurs only after X05 and is followed by its post-create freeze before X06 creates the Authority record.
Each Packet records its dependency ID, one downstream consumer, provider, region/endpoint, immutable
base/package/tool input digests, recipe
specification, exact build/provision argv, capability policy, predeclared output slots, credential
custodians, cost/expiry and rollback. It contains no future recipe blob, measured image/SBOM/filesystem/
output digest or PASS field. GitHub-hosted Windows administrative jobs do not
satisfy the non-admin verifier requirement. If any selected capability cannot be independently tested,
stop `blocked/infrastructure`; do not emulate independence on the developer workstation.

Obtain an authenticated exact APPROVE decision for every required dependency and consume each once only
at its schema-selected existing-record or create-intent consumer before freezing any provisioning
request. DENY/REQUEST-CHANGES/no decision blocks its dependent task. These planning selections do not authorize an
external side effect. Before each independently reversible provider resource set is provisioned or each
runner image is built, consume a separate `packet_type=trust_root`,
`phase=execution_authorization`, `execution_scope=provider_administration` decision with `operations=[]`
binding exact provider operations and either existing resource IDs or approved desired locators/input/
output slots, identities,
capabilities, the now-immutable X02 provider-input anchor commit/tree and exact recipe/tool blob digests, build argv,
predeclared output slots, cost ceiling, credential issuance/rotation, before-state, readback oracle and
approved-conditional rollback argv. Before Plan 4/6 reservation tooling exists, the same one-time decision is an
atomic bootstrap provider transaction: rollback is permitted only after forward failure/readback
mismatch, the selected branch is recorded, and the decision cannot be consumed again; no separate child
or CAS reservation is claimed. Publish signed provider readback and revoke build credentials after
sealing. The selection and provider-mutation events do not authorize tracked trust-root edits; each X
task still uses the Program's separate execution and landing `trust_root` Packets.

For runner construction, OCI executor and Windows verifier are two independently reversible resource
sets. Each has its own consumed provider-mutation decision, and each decision authorizes exactly that
target's two clean build slots; neither decision can authorize the other target or any third run.

## Normative task-dispatch matrix

Each row uses the Program §5.1 canonical Test, Red and Final argv with its exact work ID. The signed
dispatch expands the owning File-map entries and fixture manifest to exact paths/argv before consume;
unresolved shorthand rejects. Final is actual Release plus a different-trust-domain Sol verifier. An
ordered range receives its own fresh decision, and a future-dependent range remains deferred/non-
executable until its protected resolver emits the signed child-range record.

| Work ID | Ordered path/range closure | RED contract | Implementation/finalizer boundary | Rollback | Commit/PR boundary |
|---|---|---|---|---|---|
| NHA-X00 | `exact_paths`, three explicitly named protection files | synthetic route downgrade RED | protect only Plan 3 paths and raw exit | whole-commit revert | `tracked_pr` |
| NHA-X01 | `exact_paths`, exactly scope allowance/manifest schemas, `PcvAssuranceScope.psm1`, New allowance/manifest, Test scope, scope test and enumerated scope fixtures | Git object/path/identity corpus RED | scope allowance/manifest and tests only; Plan 2 identity tools are read-only | whole-commit revert | `tracked_pr` |
| NHA-X02 | parent `exact_paths` runner schema/recipe/build-tool/test range creates reviewed provider-input anchor then pauses; two ordered `provider_administration` OCI/Windows build children run; after two post-create freezes the same parent resumes only generator-owned `runner-images.json` op | build reuse/drift/unsigned attestation RED | two clean OCI/Windows builds; no child can edit tracked files or authorize the other | child conditional rollback; whole tracked revert | `tracked_pr`; build children `artifact_only_no_commit` |
| NHA-X03 | `exact_paths`, exactly task-authority-chain/execution-manifest schemas, confinement module/init/test wrapper, write-broker module/start wrapper, their two tests and enumerated confinement/broker fixtures; runner and telemetry paths excluded | escape/replay/crash/identity/authority-chain cases RED | read-only workspace, broker, signed event chain and non-replayable execution authority contract only | whole-commit revert | `tracked_pr` |
| NHA-X04 | parent `exact_paths`, exactly model-dispatch/result/dependency-policy schemas, model/dependency tools/records/tests, telemetry module/start/test, process runner and bounded-executor wrappers/tests; conditional deferred authenticated-model setup child resolves existing/no-create or one `provider_administration` create/readback/post-freeze range before the parent writes only its generated policy record | injection/identity/timeout/telemetry cases RED | authenticated model boundary and bounded executor only; Authority/transport/workflow paths excluded | conditional setup rollback; whole tracked revert | `tracked_pr`; setup child `artifact_only_no_commit` |
| NHA-X05 | `exact_paths`, exact workflow plus exact workflow test | unbound/mutable workflow input RED | fail-closed protected-main workflow; no dispatch | whole-commit revert | `tracked_pr` |
| NHA-X06 | parent `exact_paths` range creates Authority/transport/receipt/target-branch schemas/tools/tests and reviewed anchor but defers generated Authority/transport records; result-transport and verification-Authority subjects each resolve existing/no-create or their own ordered `provider_administration` setup/readback/post-freeze child; parent resumes only generated records/target branch, then post-merge distinct `provider_administration` remote-canary/exit range | independence/transport/dependency corpus RED | tracked Authority/transport code first; setup children cannot dispatch; exact one-run post-merge canary solely owns exit | each setup conditional rollback; whole tracked revert; revoke/abort canary | `tracked_pr`; external children `artifact_only_no_commit` |
| NHA-X07 | `exact_paths`, exactly the seventeen negative JSON/expected pairs and `positive-single-write.json`; existing tests are read-only verification inputs | each fixture must fail at intended layer | fixtures only; no production logic or test edit | whole-commit revert | `tracked_pr` |
| NHA-X08 | ordered fixture `exact_paths` range then deferred `provider_administration` target/two-verifier/exit range | unauthorized fixture/provider action rejects | typed fixture only; no project/product path | abort/revoke and delete disposable fixture after preserved evidence | fixture result `candidate_commit_no_merge`; provider range `artifact_only_no_commit` in project repository |
| NHA-X09 | ordered fixture `exact_paths` result range then deferred zero-operation `provider_administration` recovery/import/dispatch/exit range | recovery/replay/inventory/orphan corpus RED | sealed first manifest; second range inherits it and owns remote outputs | revoke/kill/reconcile; preserve failure | fixture result `candidate_commit_no_merge`; provider range `artifact_only_no_commit` in project repository |

## Task NHA-X00: Protect every Plan 3 trust-root path before implementation

**Files:** only
`docs/superpowers/plans/luna-completion/protected-paths.json`,
`packaging/windows-desktop-node/tests/PcvAssuranceContracts.Tests.ps1`, and
`packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1`.

- [ ] Generate a pre-execution Packet with `packet_type=trust_root`,
      `phase=execution_authorization` whose exact modify set is the three X00 files above; it declares
      every individual Plan 3 file-map and exact fixture-manifest path
      protected, while product paths, package output and host mutation are false. Consume its exact
      decision immediately before the first tracked edit under the bootstrap/shadow protocol; it claims
      no PASS.
- [ ] Run actual `Release`, Plan 2's trust-root negative corpus and an independent Sol review on the
      immutable candidate commit.
- [ ] Obtain and consume a fresh exact decision with `packet_type=trust_root` and
      `phase=landing_authorization`, then land the unchanged candidate under the
      program's bounded pre-Plan-5 bootstrap exception, and independently attest its merged tree.
- [ ] Prove a synthetic modification to each newly protected path is routed L/Sol/Release and cannot be
      accepted by an S/M card.
- [ ] Publish bootstrap-grade
      `assurance-bootstrap://exits/NHA-X00/<TARGET-TREE>/<PAYLOAD-SHA256>` using
      Plan 2's materialized `bootstrap-exit.schema.json` plus locked generator/validator; it is never
      assurance GREEN.

For X01–X05 and X07, the Program's per-task protocol is mandatory: a fresh-main Packet with
`packet_type=trust_root` and `phase=execution_authorization` before the first tracked edit, immutable
candidate Release plus independent review, and a separate Packet with `packet_type=trust_root` and
`phase=landing_authorization` immediately before unchanged merge, followed by
post-merge `assurance-bootstrap://exits/<WORK-ID>/<MERGED-TREE>/<PAYLOAD-SHA256>`. X06 uses the same
tracked execution/review/landing flow, but its landing Packet finalizer owns only the raw merge/landing-
equivalence receipt and explicitly excludes the final X06 exit prefix. The post-merge provider-
administration canary in X06 Step 6 solely owns and publishes that exit. X08/X09 produce control/exit
artifacts only and have no landing Packet/PR; their exact split execution decisions are defined in their
tasks. No earlier selection, X00 or X08 decision is reusable.

## Task NHA-X01: Seal scope from trusted Git objects

**Files:** scope module, allowance/manifest/test wrappers, tests and fixture descriptors.

- [ ] **Step 1: Add RED Git-object cases**

Create isolated temporary Git repositories for:

- add, modify, delete, executable-mode change and rename-as-delete-plus-create;
- caller omitting a changed path or supplying a false base;
- nonexistent base/head/tree/blob;
- case-variant path, `..` traversal, absolute path, NTFS alternate data stream;
- symlink/reparse target escaping the repository;
- protected-path and protected-symbol modification;
- dirty/untracked working-tree content that differs from Git objects;
- unsigned, forged or stale repository-identity envelope, project/typed-fixture substitution and
  allowance/descriptor/lease envelope-digest drift.

Expected RED: existing `ChangedPath`/`BaseRef` flow can influence selection and no closed pre-run
allowance/post-run manifest pair exists.

- [ ] **Step 2: Implement the pre-run scope allowance**

`New-PcvAssuranceScopeAllowance.ps1` exposes `New-PcvAssuranceScopeAllowance`, accepts only a Plan 2
`Test-PcvAssuranceRepositoryIdentity.ps1`-validated signed repository-identity envelope path and digest
(kind, immutable provider/repository IDs, descriptor digest and exact start objects), immutable start
commit/tree plus frozen spec lock and card blueprint, and writes only its exact output slot. It reopens
the signature/readback/allocation, proves the start objects belong to that identity, resolves every expected old blob/mode
with Git plumbing, then
emits UTF-8 ordinal-sorted entries such as:

```json
{
  "path": "repo/relative/file",
  "operation": "create|modify|delete",
  "expected_old_blob": "40-or-64-hex-or-null",
  "expected_old_mode": "100644-or-null",
  "allowed_new_mode": "100644-or-null"
}
```

The canonical allowance envelope records the complete repository identity, start commit/tree, card/spec-lock/blueprint digests and RFC
8785 payload digest. It has no result/head commit/tree, new blob, content digest or PASS field. Caller
path lists, `git status`, untracked files and the executor workspace are never inputs. This is the only
scope artifact the broker/executor may consume before implementation.

- [ ] **Step 3: Materialize and match the post-run canonical scope manifest**

Only after the broker closes and the trusted controller creates/reopens the deterministic result commit,
`New-PcvAssuranceScopeManifest.ps1` exposes `New-PcvAssuranceScopeManifest` and accepts the validated
allowance, its identical repository-identity descriptor and immutable start/result commit IDs. It proves
both objects belong to that identity, independently resolves both trees and emits measured
entries:

```json
{
  "path": "repo/relative/file",
  "operation": "create|modify|delete",
  "old_blob": "40-or-64-hex-or-null",
  "new_blob": "40-or-64-hex-or-null",
  "old_mode": "100644-or-null",
  "new_mode": "100644-or-null"
}
```

Rename is two entries. The canonical manifest envelope records the identical repository identity, start/result/merge-base commits and trees,
allowance/card/spec-lock digests and RFC 8785 payload digest. Every actual manifest entry must match
exactly one allowance path+operation/old blob/old mode/allowed new mode; every allowance entry must have
exactly one actual entry. Missing, extra, duplicate, case-colliding, symlink/reparse, protected or
measured-old-state mismatch rejects with a stable `PCV_ASSURANCE_SCOPE_*` code. A supplied or pre-run
manifest, new-blob guess, result override, wrong repository ID/descriptor or post-run allowance mutation
rejects. Focused fixtures cover missing identity, project-for-fixture substitution and identical Git
object IDs presented under a different repository identity.

Both phases enforce frozen intent. Directory paths and wildcard write permissions are invalid. Git
object diffs cannot prove symbol-level
safety: in this Plan-Revision, if a blueprint names `protected_symbols`, the entire containing file is
protected and rejected with no exception. A future semantic-parser design would require its own
pre-protected parser/source/binary digests, adversarial corpus and fresh trust-root design/Packet; an L
card or text/regex heuristic cannot relax whole-file protection.

- [ ] **Step 4: Verify and commit X01**

Run focused Pester twice against temp repositories, then independent clean Release. Commit
`feat: seal assurance scope from Git objects`.

## Task NHA-X02: Freeze disposable runner images and capabilities

**Files:** runner-image schema/record, OCI/Windows image recipes and confinement/telemetry fixtures.

- [ ] **Step 1: Decide and approve exact runner inputs**

The approved OCI and Windows `dependency_selection/create_identity_intent` Packets from the external selection gate freeze their
respective executor/verifier base/package/tool inputs, package repository snapshots, .NET/Node/PowerShell/
Pester versions, OS packages, recipe specification, exact build/provision argv, capability policy and
predeclared output slots. It does not freeze a recipe blob or predict an SBOM/filesystem/image/output
digest. A tag, floating `latest`, or locally substituted base is invalid. The executor has no Docker
socket, sudo, SSH, credential helper, cloud metadata route or host mount. The Windows verifier runs as a
standard user with no service-control, installer, Hyper-V, certificate-store or firewall privilege.

Each Packet is a `requirements_approval` dependency-selection decision, not product/host mutation. If no
provider can supply an
OCI runtime with non-root user, read-only root filesystem, dropped capabilities, seccomp/no-new-
privileges and network-none, stop `blocked/infrastructure`.

The closed `runner-build-command-manifest.schema.json` binds schema/version, both consumed OCI/Windows
create-intent dependency selections, immutable
X02 provider-input anchor commit/tree and exact recipe/tool blob digests, plus an exact two-item
`provider_mutation_decisions` array keyed uniquely to `oci_executor` and `windows_verifier`. Each entry
contains its request/decision/consume ID and digest, target class, provider/base/package/input digests and
exactly two clean build slots. Each slot has distinct provider/workspace/cache IDs, argv array,
executable digest, cwd, bounded non-secret environment, network/package snapshot, predeclared raw-log/
SBOM/filesystem/output artifact slots, nonce and expiry. It forbids a shell string, tag, caller-supplied or
predicted measured digest and undeclared credential. The manifest generator reads the selection, the two
consumed decisions and exact anchor Git blobs; `runner-images.json` is a later output and cannot be an
input.

- [ ] **Step 2: Write RED capability probes**

Probes attempt admin elevation, network/DNS/cloud metadata, environment secret discovery, raw device,
process escape, sibling-workspace access, repository write and trust-root/oracle write. The test fails
unless the operating-system layer denies each attempt before post-diff validation and the approved OS
collector records the attempted process/file/network action with actor/run correlation. The collector
runs outside the executor's trust boundary under a separately controlled read-only policy; the executor
cannot disable, filter or rewrite it.

Both platform adapters emit the same `telemetry-event` chain: collector/provider/image identity,
run/actor/process IDs, monotonic sequence, previous-event digest, monotonic and UTC timestamps,
process/file/network operation, target, OS disposition and signature. `telemetry-summary` binds start/
stop fences, first/last sequence, event count, dropped-event counter, chained digest and collector
health. Missing fence, sequence gap, `dropped_count != 0`, bad signature/clock order or activity outside
the run fence is failure. Fixtures inject collector stop, buffer overflow, event deletion/reorder and
executor tampering.

- [ ] **Step 3: Build images reproducibly**

The initial X02 tracked-change Packet preauthorizes the exact code/recipe/test paths and the single later
generator-owned `runner-images.json` record operation. First use it to create every code/recipe/tool blob
except that measured record and freeze a distinct immutable `provider_input_anchor_commit/tree`; do not
build during discovery. Run actual Release plus independent Sol static/security review on that exact
anchor, including provider-command injection and credential-boundary negatives. Any correction creates a
new anchor and stales later authority. Only then obtain and consume the two separate provider-
administration decisions described above, each binding that target's reviewed anchor recipe/tool blobs
and two clean output slots. Build from approved input digests with fixed identities, read-only toolchains
and preloaded locked dependencies. Run each decision's exact build/provision argv twice from clean
infrastructure and compare measured normalized filesystem/SBOM/output digests. Runtime starts with egress
disabled; dependency fetching occurs only in the separately attested image build stage. Windows image
sealing removes build credentials and proves the runtime standard-user token.

`Build-PcvAssuranceRunnerImages.ps1` is the protected orchestrator for this proof. It accepts only the
two consumed OCI/Windows create-intent dependency decisions, the exact OCI and Windows consumed provider-
mutation decisions and exact argv-array command manifest, invokes
the selected OCI build and Windows build/provision commands without shell interpolation on two distinct
clean provider workspaces/caches, then records provider/build IDs, recipes, inputs, raw logs, SBOMs,
normalized filesystem/output digests and credential-removal/readback facts. It fails unless both OCI
outputs match each other, both Windows outputs match each other, all actual input/recipe/tool digests
match the consumed decisions and every raw artifact is accessible. It never compares against a predicted
output digest. Its signed two-clean-build attestation is mandatory input to
the two new OCI/Windows `dependency_selection/post_create_identity_freeze` requests. Each post-create
Packet binds its exact intent, provider-administration decision/consume, build/readback/credential-
revocation receipts and measured attestation fields and is consumed once into the runner-record freeze.
Only then may `New-PcvAssuranceRunnerImages.ps1` deterministically fill `runner-images.json` from the two
post-create decisions and measured attestation fields under X02's still-bounded preauthorized record
operation and produce the final
immutable candidate commit `build: pin disposable assurance runner images`. The controller proves the
anchor-to-final diff contains exactly that one generator-owned record and no recipe/tool/code change.
`Test-PcvAssuranceRunnerImages.ps1` recomputes that
mapping; a locally pre-existing image or caller-filled digest cannot satisfy the task.

The closed `runner-build-attestation.schema.json` records both target classes and all four run records,
decision/manifest digests, distinct provider/workspace/cache identities, exact recipes/inputs/argv,
raw stdout/stderr/SBOM/filesystem/output artifact URI/hash/size/provider/version/retention, normalized
digests, credential removal, standard-user/confinement readback, terminal verdict and an independent
signer/key/revocation tuple. The build wrapper creates it; the test wrapper validates schema, signature,
decision consumption, distinct clean domains, exact approved input/recipe equality and two-run measured
output equality. The X02 landing Packet binds the final candidate plus actual attestation/runner-record
digests; no measured value is backfilled into selection or either already-consumed build request. The
frozen runner-build fixtures must reject missing, wrong-subject, repeated or reordered intent/provider-
mutation/attestation/post-create chains and reach 100% rejection before X02 can land.

- [ ] **Step 4: Verify runner suitability**

Plan 3 uses the approved non-root OCI runtime for model/code control and the approved disposable
non-admin Windows image for actual current `Release` verification. Both collectors must reproduce the
same process/file/network event classes. Windows-only admin/Hyper-V/install/service mutation remains
closed until Plan 6's separately approved sacrificial-host stage.

Run actual Release and a fresh independent Sol review again on the exact final candidate, binding the
measured attestation/record digests and anchor-to-final singleton diff. Then obtain the distinct landing
Packet over that actual candidate/evidence and land it unchanged. Provider execution before anchor
review, a measured-record update outside the
preauthorized generator operation, or any candidate amendment after final verification restarts X02.

## Task NHA-X03: Implement read-only workspace and exact write broker

**Files:** task-authority-chain and execution-manifest schemas plus confinement and broker modules/wrappers/tests.

- [ ] **Step 1: Add RED filesystem cases**

Test direct repository modification, arbitrary sibling creation, approved-path wrong operation, second
write after lease expiry, content hash mismatch, path normalization collision, broker replay, broker
credential reuse, large payload, trust-root/oracle path, unauthorized deletion, operation-count/order
drift, partial-batch validation failure and injected crash before/after every requested/prepared/terminal/
reference-switch boundary. Add schema-valid missing task-authority, cross-range descriptor, wrong consumer,
decision replay and consume-cardinality-two execution-manifest cases as in-memory instances inside the
already named `PcvAssuranceWriteBroker.Tests.ps1`, with stable rejection codes and no new fixture file.
Also reject missing/wrong/cross-range root-dispatch, selected-range or descriptor publication receipts; a
derived non-pair child missing/wrong resolution receipt; a pair/setup promotion missing/wrong attachment-
promotion receipt; a root range carrying a forbidden resolution receipt; and a root/generic child carrying
a forbidden promotion receipt.

- [ ] **Step 2: Initialize a constrained workspace**

Trusted controller mounts the exact repository identity/tree, pre-run scope allowance and frozen oracle read-only. The executor receives a
private temporary directory for compiler/temp output but no writable repository checkout. It sees only a
Unix-domain broker endpoint scoped to one repository identity, card, target tree, actor and expiry. Network remains disabled.
The controller authenticates the endpoint using peer identity plus the lease; the model never receives
the broker credential or endpoint outside its mediated tool protocol.

- [ ] **Step 3: Implement brokered writes**

The executor submits one canonical batch over a length-prefixed canonical-JSON Unix-domain protocol
with fixed maximum message/content/batch sizes. The batch envelope binds `batch_id`, prior candidate
tree, repository identity/descriptor digest, ordered operation count, ordered operation-manifest SHA-256,
lease/actor/card/expiry and nonce;
each operation binds:

```text
batch_id, sequence, repository_id, repository_descriptor_sha256, lease_id, actor_id, card_id, prior_candidate_tree,
operation, repo_path, expected_old_blob, content_sha256, content_length, nonce
```

For create/modify, content follows over the authenticated local channel; delete has no content. The
broker validates **all** peer/repository-identity/lease/allowance/path/operation/old-blob/mode/size/count/order/nonces before mutation and
appends a signed `requested` event. It stages the full batch in a private same-filesystem directory,
flushes files/directories, builds and rehashes the proposed Git tree, then appends a signed `prepared`
event binding old/new trees and the operation-manifest digest. No staged object is mounted or eligible.
It atomically installs the content-addressed snapshot, appends exactly one signed terminal
`committed|aborted|reconciled_aborted` event, and only after a durable `committed` event atomically
switches the controller's read-only candidate reference. A crash at every boundary is reconciled from
requested/prepared/terminal events: orphan snapshots remain unreferenced and are quarantined, while a
durable committed event is idempotently mounted. Partial-file or partial-batch visibility is impossible,
and the same batch/operation nonce is never retried. The broker cannot change protected paths and never
reveals its signing credential.

- [ ] **Step 4: Materialize a result tree safely**

After each terminal committed batch, the trusted controller rehashes every blob/mode/path, proves the
new tree and cumulative canonical diff equal the committed manifests, and restarts/remounts the runtime
on that immutable tree read-only. Thus later model and test turns see cumulative edits but never a
writable repository. Frozen commands declare exact ephemeral writable build-output mounts separately
from source/broker state: per-project `obj`/`bin`, `web/node_modules`, test results, package cache and
temp roots. Unknown or overlapping output roots reject; telemetry and post-run diff must show source
writes zero, and all ephemeral mounts are discarded before the next candidate snapshot. After lease
close the controller repeats the proof and **must** create exactly one deterministic result commit with
the approved base tree, controller-measured post-broker result tree, author policy and card/spec/oracle/
scope-allowance locators but no future verification claim. The result tree derives only from signed
terminal committed broker events and their rehashed canonical diff; a caller, Packet or allowance cannot
supply it. The controller reopens that commit through Git plumbing, proves parent/tree/message fields, then
generates the post-run canonical scope manifest from the actual start/result objects and requires exact
one-to-one allowance agreement. It seals allowance/manifest digests, result commit/tree and canonical
diff digest plus the identical repository-identity tuple into the validated execution manifest. The
  manifest also embeds one closed `task_authority` object and its digest: task-dispatch URI/SHA-256, selected
  range URI/SHA-256, planned-descriptor URI/SHA-256, three distinct root-dispatch/selected-range/descriptor
  publication-receipt URI/SHA-256 values, a required exact resolution receipt URI/SHA-256 for every derived
  child, an additional required attachment-promotion receipt only for a pair/setup child that promotes sealed
  intrinsic attachments, with resolution forbidden on a root range and promotion forbidden otherwise, and
  authorization-request/decision-event/consume-event URI/
SHA-256, consumer work/range IDs and `consume_cardinality=1`. Its validator opens every immutable source,
proves request → dispatch/range → descriptor equality and rejects a missing, replayed or cross-range
consumer before a result can be transport-eligible. A missing,
extra or caller-substituted result commit
or a pre-supplied/mismatched scope manifest blocks publication.

- [ ] **Step 5: Verify and commit X03**

Every direct forbidden OS write and broker negative case rejects; every injected crash recovers to
exactly the old or full new tree, never a partial tree. Commit
`feat: broker exact assurance file operations`.

## Task NHA-X04: Add authenticated model dispatch, exact process execution and telemetry

**Files:** model-dispatch module/wrapper/contracts, telemetry and process runner, bounded executor and
focused tests.

- [ ] **Step 1: Add RED process cases**

Cover forged model/provider identity, prompt/result digest drift, direct model tool/network call, broker
credential request, extra argument injection, shell metacharacter expansion, wrong cwd, secret
environment, child/grandchild timeout, output over 8 KiB, missing log, telemetry gap, cancellation and
nonzero child exit.

- [ ] **Step 2: Dispatch models through closed implementation and verifier roles**

For `actor_role=implementation`, the adapter is an internal operation of one already-live bounded-
executor session; no model request is
sent before the OS collector, egress-denied runtime and broker lease are active. The controller, outside
that runtime, authenticates to only the frozen provider endpoint. It opens the applicable final
`authenticated_model_boundary` dependency-selection decision and requires its subject/final-decision/
provider-key/readback/chain digests to equal the signed provider policy.
It signs a `model-dispatch` envelope binding those dependency digests, provider/model/reasoning, card/spec/oracle/scope-allowance/current
candidate tree, read-only context digests, allowed typed tools, token/turn/deadline limits and nonce.
The model receives no provider, broker, verifier or repository credential. Model tool requests return to
the controller, which permits only read, exact broker operation, and frozen command execution; free-form
shell/network tools reject. The `model-result` binds the original envelope, provider request ID, all
turns/tool events and terminal status. Its authority is either the allowlisted provider's signed receipt
or the selected protected adapter's signature over hashes of authenticated raw request/response bytes
plus provider TLS/account/request receipt. The schema binds signer/key/algorithm, trust domain,
timestamp and revocation locator; the controller's unsigned projection is never identity proof.
Unknown/revoked signer, raw-byte digest mismatch or provider/model mismatch returns
`PCV_ASSURANCE_MODEL_RESULT_UNVERIFIED` and blocks.

X04 implements the closed dependency-binding schema plus protected New/Test tools. For
`selection_subject_id=authenticated_model_boundary`, the generator accepts only the applicable final
selection chain and its signed provider readback, then emits
`dependencies/authenticated-model-boundary.json` under `model-provider-policy.schema.json`. The tester
reopens every referenced event and validates the final stage, subject, endpoint/key, provider readback
and chain digest. Neither tool accepts free-form provider policy fields.

The same closed `model-dispatch`/`model-result` contracts have a mutually exclusive
`actor_role=independent_verifier` branch, invoked only by
`Invoke-PcvAssuranceVerifierModel.ps1` inside the remote Authority workflow. It binds exact target/spec/
oracle/post-run-scope-manifest/diff and deterministic raw test artifacts, a principal/credential/workspace distinct from
implementation, and only read-file/read-artifact tools. Broker, write, shell, network, credential and
target-selection tools are forbidden. Model/lane selection must equal the closed canonical work-class
route above. Its signed result is a review
attestation with findings/coverage and cannot contain or override a gate PASS. Missing, same-actor,
wrong-model, write request, target drift or unsigned result blocks the independent-verifier gate.

- [ ] **Step 3: Execute without shell interpolation**

Use `ProcessStartInfo.ArgumentList` or an equivalent argv-preserving primitive. Match exact executable
digest, argv array, cwd, allowlisted non-secret environment, timeout and capability from the card.
Start a process group/job; timeout or cancellation terminates the whole tree and is failure.

- [ ] **Step 4: Preserve complete raw execution evidence and OS telemetry**

Write stdout/stderr as immutable files without truncation and record hash, size, media type, start/end,
exit, timeout, actor, image digest and OS collector process/file/network locators. Correlate collector
events to run/process identity and validate the normalized signed event/summary schemas, start/stop
fences, chained digest, monotonic sequence and zero drop counter. Fail on loss, reorder, bad signature,
tamper or unexplained activity.
Process-runner self-report is not confinement proof. Summary previews are never raw proof. Validate the
execution-manifest schema from Plan 2.

- [ ] **Step 5: Compose the bounded executor**

`Invoke-PcvBoundedExecutor.ps1` requires the applicable final `authenticated_model_boundary`
`-DependencySelectionDecisionPath` plus exact `-TaskDispatchPath`, `-RangeRecordPath`,
`-PlannedCommandDescriptorPath`, `-TaskDispatchPublicationReceiptPath`,
`-RangeRecordPublicationReceiptPath`, `-PlannedCommandDescriptorPublicationReceiptPath`, required
`-ResolutionReceiptPath` for every derived child, and required `-AttachmentPromotionReceiptPath` only when
that pair/setup child promotes sealed attachments; resolution is forbidden for a root range and promotion is
forbidden for every other range,
`-AuthorizationRequestPath`, `-DecisionEventPath` and
`-ConsumeEventPath`. It opens the authority chain, rejects replay/cross-range/cardinality drift, validates
the model dependency against provider policy, then validates spec/card/scope-allowance/actor/image and the selected
collector policy, starts the signed telemetry fence and egress-denied runtime, and only then opens the
broker lease. Within that live session it creates/signs the model-dispatch envelope, invokes
`Invoke-PcvAssuranceModelDispatch.ps1`, mediates every Luna/Sol tool turn, snapshots every accepted
batch, and obtains the authenticated terminal model-result. It then closes the broker lease/runtime and
telemetry fence, validates the dispatch/result/tool/telemetry chains as one transaction, builds the
result tree, generates/validates the post-run scope manifest and writes exact model-dispatch, model-
result, scope-manifest and execution-manifest output files. A missing
or unverified model-result, a model call outside the live fence, any mismatch or nonterminal session
discards the result from landing eligibility but preserves signed failure evidence.

- [ ] **Step 6: Verify and commit X04**

Expected: all injection/escape/timeout/secret/network cases reject, complete long logs rehash, and
PlanOnly remains planned/not eligible. Commit `feat: execute frozen cards in bounded runtime`.

## Task NHA-X05: Land the protected verifier workflow before Authority binding

**Files:** only `.github/workflows/assurance-verifier.yml` and
`packaging/windows-desktop-node/tests/PcvAssuranceVerifierWorkflow.Tests.ps1`.

- [ ] **Step 1: Add RED workflow assertions**

Require exact action SHAs, protected-default-branch workflow identity, read-only source permission,
OIDC only for the one-run verifier credential, no repository write/admin/secret/host token, signed
content-addressed dispatch input, clean checkout, artifact upload on failure, actual lane, final schema
enforcement and no merge step. Reject PR-defined workflow content, target/card/oracle input, mutable ref,
raw caller SHA, unknown App/environment, missing Authority record and reused delivery/nonce.

- [ ] **Step 2: Land the final fail-closed remote entry workflow**

The workflow accepts only an external Authority App delivery containing a signed dispatch locator and
digest. The provider must resolve the workflow from protected default branch; the first job records its
provider workflow ref/blob and proves content equality to X05's approved digest before candidate code is
checked out. It then loads `verification-authority.json` and all verifier entrypoints from the signed
dispatch's exact
`verifier_tooling_commit/tree`. That tooling commit must be the approved X06-or-later protected `main`
ancestor, its projection blob must match the signed live Authority record, and it is independent from
the product result target.
Until X06 lands that record and code, every dispatch fails closed; no stub success exists. Separate
accept, provision, execute, revoke and publish jobs preserve provider delivery/run identities. Only the
remote execute job may invoke `Invoke-PcvIndependentVerifier.ps1` and
`Invoke-PcvDevelopmentVerification.ps1`; neither script is a local dispatch substitute. The executor
job cannot access the verifier credential, the verifier cannot write source, and terminal publication
runs on failure. Upload is remote transport evidence, not Plan 4 notary proof.

The remote execute job derives Authority, dispatch, target, runner-image, base/result and artifact-root
values only from the validated delivery plus protected tooling tree. It validates the dispatch's closed
tier/lane/model/reasoning mapping from the closed canonical work-class route; unknown pairs or caller
downgrade/override reject. In order it runs the independent deterministic verifier entrypoint,
`Invoke-PcvDevelopmentVerification.ps1` with those exact signed tier/lane values, `git diff --check` on
the target manifest's exact base/result objects, and the Authority-only read-only verifier-model branch.
S/M require a different-trust-domain Luna Max result; `L|trust_root|release` requires the card-frozen
separate Sol actor/reasoning.
A prior nonzero exit prevents PASS but not revoke/publish; the final artifact manifest records every
argv, raw output and signed verifier-model attestation.

- [ ] **Step 3: Verify and commit X05**

Static negative fixtures prove every unbound/missing input fails and that no job can report PASS while
the Authority record is absent. Commit `ci: add fail-closed assurance verifier entry workflow`, land it
under the global per-task protocol, and record the exact merged default-branch workflow commit/content
digest. Do not dispatch the workflow yet; X06 alone may bind an Authority to this already-landed digest.

## Task NHA-X06: Implement the independent Verification Authority and remote dispatch

**Files:** verification authority/dispatch/receipt contracts and record; target/issuer branch changes;
result transport; Authority dispatch/receive and independent-verifier modules, wrappers and focused
tests. X05's workflow is read-only input and is not modified.

- [ ] **Step 1: Add RED independence and transport cases**

Reject unapproved provider/App/environment, unsigned/wrong-issuer dispatch, non-X05 workflow digest,
same trust domain/principal/credential/workspace, controller-supplied target override, nonce replay,
credential reuse, dirty checkout, non-ancestor oracle, missing spec lock, stale target, local untracked
oracle, wrong lane, executor-provided result summary, mutable/overwritten result object, wrong transport
receipt, truncated bundle, unadvertised Git object, base/result ancestry mismatch, forged provider
delivery/run receipt, wrong run attempt and incomplete remote artifact manifest.
Also reject intent-only dependency decisions, wrong final decision, wrong subject, copied endpoint/key IDs
with a different chain, result-transport readback drift and cross-dependency replay. Add schema-valid
missing-task-authority, publication/range mismatch, descriptor substitution, decision/consume replay,
wrong consumer and receipt/verified-result/target/dispatch authority-digest drift as in-memory cases inside
the already named `PcvAssuranceResultTransport.Tests.ps1` and `PcvVerificationAuthority.Tests.ps1`; create
no additional fixture file. Each must fail before remote eligibility with a stable code.

- [ ] **Step 2: Materialize a separately controlled authority and target branch**

After X05 is an exact `main` ancestor and the applicable final `verification_authority` selection is
consumed, `verification-authority.json` binds that final request/decision/consume and subject/chain
digests plus immutable provider/App/
environment IDs, issuer and public key, allowed repository/caller/ref, that exact X05 protected-main
workflow commit/content digest, verifier image/SBOM digest, credential mint/revoke endpoint and expiry.
A protected external environment owned by neither executor nor implementation controller signs the
envelope. Repository JSON is only a projection; live identity, signature and revocation are
authoritative. The record generator opens either the signed existing-provider readback or the complete
intent/provider-administration/post-create chain and rejects copied IDs or drift. If this separate trust
domain is unavailable, block.

X06 reuses the protected dependency-binding tools to emit
`dependencies/result-transport.json` under `result-transport-policy.schema.json` and to bind the final
`verification_authority` chain into `verification-authority.json`. The transport and Authority records
are generated solely from applicable final selection chains and signed readbacks; a caller-authored
policy, wrong subject or intent-only decision is invalid.
The record does not predict X06's future merge hash. After X06 merges, the live Authority selects and
signs the exact protected-main `verifier_tooling_commit/tree` in each dispatch; policy requires X05's
workflow blob at that tree to retain the approved content digest and requires all verifier entrypoints
to match the protected X06 result blobs.

Under X06's fresh `trust_root/execution_authorization` Packet, extend Plan 2's protected target contract
without weakening its original branch. `verification-target.schema.json` gains required `issuer_class`
with exactly two conditional shapes:

- `provider_event_oidc` retains every Plan 2 repository/event/delivery/payload/JWT claim and forbids
  Verification Authority result fields;
- `verification_authority` requires the approved Authority manifest/signing key, protected workflow and
  verifier-tooling digests, exact base/result commit/tree, scope/spec/oracle and nonce/expiry, and forbids
  raw GitHub event claims. Its closed `authority_source` is either `verified_result`, which additionally
  requires the signed verified-result/publication/transport chain, or
  `serialized_landing_candidate`, which instead requires Landing Authority App identity, exact
  PR/latest-base candidate, provider readback, exclusive lease ID/fencing token/CAS receipt and signed
  candidate-construction attestation while forbidding every result-transport/model-execution field.

Modify `verification-target-issuer.schema.json`, `verification-target-issuer.json`, the target generator
and both focused test suites in the same protected candidate. The issuer record adds the separately
approved Authority identity/public key/workflow digest but cannot replace or broaden Plan 2's issuer.
The generator always validates the applicable signer/revocation, allowlisted provider/App/environment
and repository/ref/default-branch workflow digest, then validates only the selected branch's complete
chain: `verified_result` requires signed publication, transport receipt and imported verified-result;
`provider_event_oidc` requires signed provider event/delivery/payload/JWT and its exact constituent
mapping; `serialized_landing_candidate` requires Landing Authority identity plus exact lease/fencing/
CAS/provider-readback/candidate-construction chain. Cross-branch fields, unknown issuer classes,
unsigned projections and caller target overrides reject. The selection decision authorizes the
Authority choice; X06 execution/landing decisions separately authorize and attest these tracked trust-
root changes.

- [ ] **Step 3: Publish and independently import the result object**

After bounded-executor finalization, the trusted controller creates a deterministic Git bundle
containing only the approved base and manifest-sealed result closure and a canonical signed
`result-publication` envelope. It recomputes and binds base/result commit/tree, scope-allowance/scope-
manifest/card/spec/oracle,
execution-manifest/model-result digests, bundle SHA-256/size and publisher identity from validated input
files. It also repeats the complete X03 `task_authority` object and digest. The publisher requires the
exact task-dispatch/range/planned-descriptor/request/decision/consume paths, opens each and requires them
and the execution manifest to agree before the first object write; `scope` here is the matched allowance plus post-run manifest pair, never a pre-run new-blob
claim. A credential unavailable to executor/model writes it create-only at
`assurance-result://objects/<RESULT-TREE>/<BUNDLE-SHA256>` and reads it back to produce a conditional-
create/version/checksum receipt. The publisher derives the result commit from the execution manifest;
the signed receipt repeats the same complete `task_authority` object/digest and binds the publication.
It accepts no caller result or authority override. This transport is target delivery, not Plan 4 evidence/notary.
The publisher and importer both require the applicable final `result_transport` dependency-selection
decision; the signed transport policy, publication and receipt repeat its subject/final-decision/
provider-readback/chain digests. Intent-only, wrong-subject or policy/decision mismatch rejects before
any object write or read.

The external Authority receives the signed publication plus receipt through its protected queue,
downloads with a separate read credential into a new bare repository, verifies provider identity/
version, publication schema/signature, receipt, bundle hash/size and `git bundle verify`, imports
objects, and independently resolves base/result trees, ancestry and scope. The closed
`verified-result.schema.json` binds Authority manifest/key/workflow, publication/receipt digests, bundle
locator/hash/size, base/result commit/tree, scope-allowance/scope-manifest/card/spec/oracle, nonce/expiry
and Authority signature plus that identical `task_authority` object/digest. The importer opens the six
authority sources through the signed publication and receipt, recomputes consume cardinality and rejects
any substitution before signing.
It emits that signed record only after every field recomputes; controller paths/local refs/worktrees are
never target authority.

- [ ] **Step 4: Select, deliver and receive only through the remote workflow**

From the signed verified-result record, Authority opens the applicable final `verification_authority`,
`windows_verifier` and `authenticated_model_boundary` dependency-selection decisions, matches them to
the signed Authority/image/model policies, selects the exact result/spec/oracle/lane and signs
verification-target and verification-dispatch envelopes. `Submit-PcvVerificationAuthorityDispatch.ps1`
validates those envelopes, uses the selected Authority App to deliver only the content-addressed
dispatch to the already-landed X05 workflow, and records a signed provider delivery receipt.
`Receive-PcvVerificationAuthorityRun.ps1` waits by immutable delivery ID, rejects a different workflow
commit/content digest, App/environment/ref/run attempt or replay, verifies credential revocation, then
downloads and validates the signed provider run receipt and remote artifact manifest. Direct local
invocation of `Invoke-PcvIndependentVerifier.ps1` as a dispatch path rejects.

For the `verified_result` branch, target and dispatch each repeat the same complete `task_authority`
object/digest and the Authority reopens publication, receipt and verified-result before signing. Submit,
provider delivery/run receipts and Receive preserve/recompute that digest; the remote workflow opens the
immutable dispatch/range/descriptor/request/decision/consume chain before checkout or verifier execution.
A missing source, cross-range digest, wrong consumer, replay or cardinality other than one is ineligible and
cannot be repaired by a later command manifest or exit.

The closed delivery-receipt schema binds provider/repository/delivery ID, dispatch digest, Authority
App/environment, X05 workflow path/content digest, accepted UTC and provider signature. The run-receipt
schema binds that delivery, run ID/attempt, exact workflow and verifier-tooling commit/tree, verifier
actor/credential/image, start/end/conclusion, revoke receipt and artifact-manifest digest. The artifact
manifest enumerates verification result, execution comparison, raw stdout/stderr/test/telemetry objects
and verifier-model dispatch/result with URI/hash/size/provider/version/retention and terminal signature.
Target, dispatch, delivery, run and verifier-model records repeat all applicable final subject/decision/
readback/chain digests; the receiver reopens each final selection and rejects caller-substituted endpoints.
Unknown fields, duplicate delivery/run, missing failure artifacts, non-success deterministic verification
verdict, missing required model-role attestation or unrevoked credential reject.

`verification-dispatch.schema.json` has three mutually exclusive Authority source branches. The
`verified_result` branch used by Plan 3 requires the signed result-publication/transport/verified-result
chain. The `provider_event_target` branch used by Plan 5 accepts only a Plan 2
`issuer_class=provider_event_oidc` target for a server-produced `merge_group/checks_requested` candidate,
requires its exact provider event/delivery/base/head/tree/workflow signature plus frozen spec/oracle/lane,
and forbids model-result, execution-manifest and result-transport fields. In that branch
`New-PcvVerificationDispatch.ps1 -ProviderEventTargetPath <validated-target>` has no
`-VerifiedResultPath`; Authority signs the remote dispatch over the exact merge-group candidate and the
same Submit/Receive receipt chain runs it. The `serialized_landing_candidate` branch accepts only a
Verification Authority target with the exact Landing Authority lease/fencing/CAS/latest-base/provider-
readback chain; `-SerializedLandingCandidatePath` replaces both other source parameters and is used only
by Plan 5 equivalent mode. It independently re-resolves the provider candidate before signing. The
three branches cannot satisfy or weaken each other.
Every branch also binds exact `change_tier`, `lane`, verifier model/reasoning and independent actor policy
under the closed canonical work-class route above. No dispatch client or workflow input may lower or
replace those values.

The remote workflow mints a non-delegable one-run credential for a different trust domain, creates a
fresh checkout with clean submodules/dependencies, independently recomputes the post-run scope manifest
from start/result objects and matches it to the signed allowance, then runs
`Invoke-PcvIndependentVerifier.ps1` and the exact signed Fast/Full/Release lane from the closed table,
publishes all raw artifacts even on failure, and revokes the credential at terminal status. It consumes
no executor PASS fields.

- [ ] **Step 5: Close pre-landing verification without remote dispatch**

Run the frozen Authority/transport/workflow positive, negative, boundary/property/mutation/rollback
fixtures locally and statically, plus the existing Plan 2 actual Release path and independent Sol review
on the immutable X06 candidate. Do not dispatch X05, mint a remote verifier credential or claim an actual
remote lane before X06 is a protected-main ancestor; the X05 contract must reject the candidate copy.
Validate discovered/executed counts and Plan 2's Pester adapter. Failed/not-run/planned can never become
PASS.

- [ ] **Step 6: Land X06, then prove the remote post-merge canary**

After the exact landing decision and unchanged merge, start from fresh X06 `main` and generate/approve/
consume a new `packet_type=trust_root`, `phase=execution_authorization`,
`execution_scope=provider_administration` Packet. It binds the
exact X06 main commit/tree, protected X05 workflow and Authority/tooling/spec/oracle digests, signed target,
closed work-class route, one-run credential mint/revoke, dispatch/receive commands, create-only output
prefixes, cost/expiry, failure publication and abort/reconciliation; tracked edits, provider setup,
product and host mutation are forbidden. Only then run the X05 workflow against merged X06 `main` and
execute the frozen actual Fast/Full/Release cases in the approved disposable non-admin Windows verifier
image. Exact target/spec/oracle/scope/toolchain invariants must agree while actor/credential/workspace
differ; provider delivery/run receipts, revoke receipt and raw remote artifacts must verify. OCI may
duplicate portable suites but cannot substitute for current actual Release.
Its `finalizer_policy` prebinds every delivery/run/revoke/raw-artifact receipt and the NHA-X06 exit.
The exit generator/validator requires and revalidates this post-merge canary decision/consume, provider
delivery/run, credential-revocation and independent readback artifacts; omission, substitution, stale
target or an X06 landing-decision locator in place of the canary authority rejects. No other X06 Packet
may name or publish the final exit prefix.

Commit `feat: dispatch independent assurance verification` for the candidate before that post-merge
authorization. No X07 work starts until the remote canary authority/consume, run, credential revocation,
create-only outputs and `NHA-X06` bootstrap exit pass. Automatic landing remains disabled.

## Task NHA-X07: Run the confinement negative corpus

**Files:** exact fixtures in `assurance-confinement/`:

```text
hidden-path.json
wrong-operation.json
trust-root-edit.json
same-actor.json
dirty-tree.json
network-attempt.json
admin-attempt.json
secret-attempt.json
symlink-escape.json
timeout-child-tree.json
zero-test.json
plan-only.json
broker-replay.json
model-direct-tool.json
telemetry-gap.json
authority-spoof.json
credential-reuse.json
```

- [ ] **Step 1: Freeze expected error for each fixture**

Each case has one expected failing layer and code. A case that fails earlier for an unrelated parse
error does not count.

- [ ] **Step 2: Run corpus in two clean environments**

All seventeen known-bad cases must reject in both environments. Positive fixture produces one exact
approved blob change and no unexplained write/network/process event.

- [ ] **Step 3: Commit only the exact fixture files**

Existing tests are read-only verification inputs. No test or production-logic edit is permitted by this
range.

No production logic is changed in this task. Commit `test: prove bounded executor escape rejection`.

## Task NHA-X08: Reproduce an end-to-end non-product card

**Files:** no product path. Use a frozen temporary fixture repository owned by the test suite.

- [ ] From an immutable provider event/issuer receipt for exact X07 project `main`, use
      `New-PcvVerificationTargetManifest.ps1` to create and validate the signed project-lineage target
      before requesting X08 approval. It names only the unchanged project commit/tree and no fixture
      result.
- [ ] From that immutable project target plus the frozen fixture-repository identity/start commit/tree,
      spec and card, generate and validate a new X08 scope allowance. The allowance targets the fixture
      repository—not the project—and binds the closed `repository_identity` tuple
      (`kind=typed_control_fixture`, immutable provider/repository IDs, descriptor digest, start commit/
      tree) plus its exactly one create/modify path operation. Then generate
      an X08-only planned-command descriptor binding that allowance, the signed project-target digest,
      exact model/verifier commands, actors/capabilities and output slots. Neither pre-run artifact
      contains a result commit, new blob, exit, raw output or PASS field.
- [ ] Generate, approve and consume a fresh `packet_type=trust_root`,
      `phase=execution_authorization`, `execution_scope=tracked_change` Packet whose repository target is
      the frozen fixture repository. It binds that exact `repository_identity`, its start commit/tree and
      an `operations` array byte-
      for-byte equal to the allowance's singleton path operation, both pre-run allowance/descriptor
      digests, signed unchanged-project target digest,
      temporary path, already-provisioned bounded-model adapter command, actors/capabilities, expiry,
      oracle, abort and cleanup. It authorizes no project/product/host operation and no provider resource,
      policy, credential or remote-verifier administration. Its `finalizer_policy` prebinds only protected
      raw bounded-execution/result/scope-manifest/unsigned-fixture-target outputs; it cannot publish the
      later verifier run or X08 exit.
- [ ] Require the X08 descriptor, broker lease, execution/run/command/scope manifests and fixture result
      to repeat the identical repository-identity tuple; a project ID, path-derived ID or tuple drift is a
      hard failure.
- [ ] Freeze `change_tier=trust_root`, `lane=Release`, the required Sol implementation/verifier model
      separation, requirement, oracle, exact file operation, actor policies and expected artifacts.
- [ ] Run the bounded implementation actor, create the exact fixture result commit and deterministically
      generate the unsigned fixture-target payload naming only its start/result objects.
- [ ] Only after that immutable result exists, generate, approve and consume a second distinct
      `packet_type=trust_root`, `phase=execution_authorization`,
      `execution_scope=provider_administration` Packet with `operations=[]`. It binds the exact fixture
      result and unsigned target-payload digests, Verification Authority signing operation, exactly two
      clean verifier dispatches, one-run credential mint/revoke, actors, protected commands, output/
      replica prefixes and providers, retention, expiry, cost, readback, abort and reconciliation. It
      permits no repository/product/host change or persistent provider setup/policy mutation. Its
      `finalizer_policy` prebinds only the signed fixture target and raw delivery/run/readback/revocation/
      X08-exit outputs.
- [ ] Under that second consumed decision, have the separate Verification Authority sign the exact
      fixture target and independently dispatch clean verification twice. The target is never backfilled
      into the first decision or either pre-run artifact. X08 `New/Test-PcvAssuranceBootstrapExit` receives
      the pre-run project manifest as `-ExpectedTargetManifestPath` and this fixture target only as
      `-TypedControlTargetManifestPath`; a fixture object in project base/result is a hard failure.
- [ ] Confirm executor/verifier actor separation and identical artifact-class invariants.
- [ ] Confirm unauthorized write, network, admin, secret and host mutation counts are zero.
- [ ] Publish bootstrap-grade
      `assurance-bootstrap://exits/NHA-X08/<TARGET-TREE>/<PAYLOAD-SHA256>` with schema
      `pcv-assurance-bootstrap-exit-v1`, two signatures and raw transcript/CI replica locators. Its
      project lineage is `artifact_only`: base/result are the same exact X07 post-merge main commit/tree
      and candidate/merged/PR fields are absent. The fixture repository's start/result commit/tree are
      typed control artifacts only and cannot replace project lineage. Use Plan 2's protected payload-
      replica generator and conditional-create full-envelope publisher/readback validator under X08's
      second exact Packet; local-only or mutable publication is not an exit. The exit binds both distinct
      decision/consume chains and neither can be reused or substitute for the other.

This is a control canary, not one of Plan 6's required product S/M pilots. X09 must derive a new
allowance/descriptor from its own immutable start/card and may reference but never reuse X08's Packet,
allowance, descriptor, lease, output slots or measured run records.

## Task NHA-X09: Publish the Plan 3 exit attestation and recovery drill

**Files:** no project tracked file. Exact external artifacts are the typed fixture result plus the
Packet-bound recovery/import/dispatch/readback/revocation/reconciliation/exit objects. Neither range
creates a project commit or PR.

- [ ] From an immutable provider event/issuer receipt for the exact X08-complete project `main`, generate
      and validate a new signed project-lineage target manifest before either X09 Packet. The later
      fixture Verification-Authority target remains a typed control artifact and cannot satisfy project
      lineage.
- [ ] For the valid fixture run, generate and validate a fresh fixture-targeted pre-run scope allowance
      from its frozen start/spec/card, then generate a first Plan 2 planned-command descriptor binding the
      allowance and unchanged-project target digests. Neither may contain a result commit, new blob, exit
      code, raw output or PASS field; any drift before consume requires new artifacts and authority.
- [ ] Generate, approve and consume a first distinct `packet_type=trust_root`,
      `phase=execution_authorization`, `execution_scope=tracked_change` Packet whose repository target is
      the X09 fixture. It binds its closed `kind=typed_control_fixture` repository-identity tuple, exact
      start commit/tree, the allowance's complete nonempty operation
      set, first descriptor, already-provisioned bounded-model adapter, oracle, actors, result-transport
      prefixes/provider/retention and abort/cleanup. It authorizes no project/product/host or provider-
      administration operation. Its `finalizer_policy` owns only raw bounded-execution/result/scope/
      signed-result-publication/transport-receipt outputs, never remote dispatch, import inventory or exit.
- [ ] Require the first X09 allowance/descriptor, broker lease, execution/run/command/scope manifests and
      fixture result to bind that same repository identity; mismatch with the unchanged project identity
      or any path-derived alias rejects.
- [ ] Under that first consumed decision, produce one exact fixture result/scope manifest and conditionally
      publish/read back the authenticated result object. Freeze its immutable publication and receipt.
- [ ] From that actual result, freeze a distinct closed recovery-fixture `repository_identity` with
      `kind=typed_control_fixture`, immutable provider/repository IDs, descriptor digest and exact recovery
      start commit/tree. Generate a separate zero-operation recovery allowance and second planned-command
      descriptor that bind that identical tuple. Then generate, approve and consume a second distinct `packet_type=trust_root`,
      `phase=execution_authorization`, `execution_scope=provider_administration` Packet binding the exact
      result/receipt, both recovery-artifact digests and unchanged X08-complete project target,
      `operations=[]`,
      Authority import/target-signing, lease/recovery and exactly bounded remote dispatch/revoke commands,
      failure oracle, exact import-candidate/exit prefix families, two distinct payload-replica providers,
      cost/expiry and protected finalizer tool/argv digests. It cannot authorize a product/host/repository
      mutation, provider setup or persistent provider resource/policy mutation beyond the exact ephemeral
      credential/lease/dispatch/revoke transaction, nor reuse X08 or the first X09 authority. Its
      `finalizer_policy` prebinds the raw verified-result/target/lease-revoke/process-kill/blocked-first-
      run/evidence-survival/fresh-dispatch-attempt/delivery/run/credential-revoke/reconciliation/import-publication/
      readback/exit outputs.
- [ ] The recovery-fixture identity is a read-only causal subject, not repository authority. The allowance,
      second descriptor, drill record and final command manifest must repeat the same tuple while every
      operation list remains empty. The provider-administration Packet binds the allowance/descriptor
      digests transitively but must not expose the tracked branch's `repository_identity` field or gain a
      repository capability. Missing identity inside either recovery artifact, project/first-fixture
      substitution, mixed-branch Packet fields, cross-range tuple injection, a nonempty operation or any
      broker write rejects.
- [ ] Under only the second decision, first import/verify the frozen result and create the immutable signed
      fixture target/dispatch without submitting it. Then run the protected
      `Invoke-PcvAssuranceRecoveryDrill.ps1` with that exact dispatch, the zero-operation allowance and
      prebound fault schedule. The wrapper is the sole caller of the protected Submit/Receive callbacks;
      no later standalone submit is permitted. Revoke its fresh lease before any broker
      `requested` event, prove the complete process tree dies and emit schema-validated signed lease-
      revoke, process-kill, blocked-first-run and evidence-survival records. Then inject verifier
      infrastructure loss, emit the one allowed fresh-dispatch eligibility/attempt record and finish with
      a signed uncertain-result reconciliation record. The closed `recovery-drill.schema.json` binds the
      decision, allowance/descriptor, actor/process/lease/dispatch IDs, ordered timestamps, zero repository
      operations, raw artifact locators and terminal state. Its fresh-dispatch attempt ID/digest must equal
      the actual provider delivery/run/credential-revoke receipts and the original immutable dispatch;
      a second delivery, mismatch, missing slot or any write makes the second range ineligible.
- [ ] Simulate verifier infrastructure loss before any mutation; allow at most one fresh dispatch and
      never turn the first blocked record into PASS.
- [ ] Prove an uncertain result cannot be retried or landed without reconciliation/new run.
- [ ] Validate each OCI executor, Windows verifier, Verification Authority, authenticated-model boundary
      and result-transport dependency's branch-conditioned selection history. `existing_identity` has
      exactly one dependency-selection request/approval/consume plus preexisting signed readback and zero
      create/post-create chain. `create_identity_intent` has exactly its intent request/approval/consume,
      provider-setup trust-root request/approval/consume, signed forward/readback/credential-revocation/
      selected-rollback events and one `post_create_identity_freeze` request/approval/consume. Validate
      that the applicable final decision/subject/provider-key/readback/chain digest equals its policy/
      record and every model dispatch/result, transport publication/receipt, Authority target/delivery/
      run artifact that consumed it; intent-as-final, wrong-subject and cross-dependency reuse reject.
      Validate
      every X00–X08 execution/landing
      decision and signed exit; X06's distinct post-merge remote-canary request/approval/consume,
      credential-mint/revoke, delivery/run and create-only artifact events; plus both X09 execution
      request/approval/consume chains. The expected-inventory descriptor freezes each selected branch and
      its one-versus-two planning-chain cardinality; missing, extra, reordered or cross-dependency events
      reject. Use Plan 2's exact import-candidate schema and New/Test tools to build and
      independently validate a deterministic one-to-one manifest against the X09 expected-inventory
      descriptor and signed project-lineage target. Then publish that exact manifest create-only under
      `assurance-bootstrap://import-candidates/NHA-X09/<manifest-payload-digest>/manifest.json`.
      X09's final exit is terminal and excluded from its own inventory; candidates never consume or
      claim canonical import.
- [ ] X09 has one immutable `x09-root-task-dispatch` and one root receipt. Fixture-change consume publishes
      that root plus its own range/descriptor receipts; provider-recovery consume must reopen the identical
      root/original receipt and publish only its second range/descriptor receipts. Duplicate roots, different
      root digest/receipt or a missing/cross-range selected-range receipt rejects.
- [ ] Include exactly one signed bootstrap root task manifest and root receipt per X00–X09 work item, every
       ordered/deferred/resolved selected-range record with its distinct §6.2 receipt, Dispatch Authority
       signature, plus every execution range's planned-command descriptor URI/digest and third §6.2 receipt.
       Every derived child additionally binds its resolution receipt; only a pair/setup child that promotes
       sealed intrinsic attachments additionally binds an attachment-promotion receipt. The latter is
       forbidden for root/generic children. Bind
      every executable range one-to-one to its request/decision/consume and preserve multi-range order,
      including X02/X04/X06 conditional setup branches and X08/X09 fixture/provider splits. Missing,
      extra, reordered, unresolved, cross-range, unreceipted or Packet-digest-mismatched dispatch history
      invalidates the X09 import candidate; the terminal X09 exit remains excluded.
- [ ] Publish `NHA-X09-bounded-executor-verifier-v1` at
      `assurance-bootstrap://exits/NHA-X09/<TARGET-TREE>/<PAYLOAD-SHA256>` using
      `pcv-assurance-bootstrap-exit-v1`, two trust-domain signatures, exact command/actor/image/tool
      digests, every recovery-drill record and raw transcript/CI replica locators. It remains `bootstrap_grade=true` and
      `assurance_green=false` until Plan 4 imports and notarizes it. Its project lineage is
      `artifact_only`: base/result are the same frozen X08-complete project main commit/tree and no
      candidate/merged/PR field exists; recovery-run temporary/fixture objects remain typed artifacts.
      The exit validator receives the signed project-lineage target as its expected target and the fixture
      target through the distinct typed-control parameter. This exit `lineage.kind=artifact_only` describes
      unchanged project Git lineage; the exit binds both X09 decision/consume chains and does not collapse
      their distinct `tracked_change` and `provider_administration` execution scopes.

## Required verification commands

This is an ordered cross-task evidence manifest, not one rerunnable shell batch. X02 alone executes the
runner-manifest/build/test subset immediately after consuming the distinct OCI and Windows provider-
mutation decisions; those decisions are never reused. X04/X06/X08 later consume X02's immutable signed
two-clean-build attestation and may rerun only the non-mutating image test, never rebuild or reprovision
an image. X09 executes the end-to-end subset after all owning tools are merged, then assembles the final
command manifest from each task's already schema-validated immutable outputs. Every owning phase has its
own pre-run planned-command descriptor and Packet; the protected Plan 2 bootstrap-command runner emits a
measured run manifest plus raw stdout/stderr for each top-level command. X09 has two measured ranges: its
fixture-change descriptor/Packet covers only the valid bounded result transaction, and its later
provider-recovery descriptor/Packet covers only the zero-operation recovery, Authority import, target
signing and remote dispatch/finalization transaction. Neither authorizes or reruns X02 provisioning/
builds, X04 implementation or an earlier Authority run. The final aggregator accepts the first X09 range
and each prior owning task's immutable command manifest as source references. Phase boundaries, decision
IDs and owning work IDs are themselves entries in that manifest.

```powershell
# X09 pre-authorization controls
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceRepositoryIdentity.ps1 `
  -InputPath <frozen:x09-fixture-repository-identity> `
  -ExpectedKind typed_control_fixture -ExpectedStartCommit <frozen:start-commit> `
  -ExpectedStartTree <frozen:start-tree>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceScopeAllowance.ps1 `
  -StartCommit <frozen:start-commit> -StartTree <frozen:start-tree> `
  -RepositoryIdentityPath <frozen:x09-fixture-repository-identity> `
  -SpecLockPath <frozen:spec-lock> -CardPath <frozen:card> `
  -OutputPath <slot:x09-fixture-change-scope-allowance>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceScope.ps1 `
  -AllowancePath <slot:x09-fixture-change-scope-allowance>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvVerificationTargetManifest.ps1 `
  -ProviderEventPath <frozen:x08-project-main-provider-event> `
  -IssuerReceiptPath <frozen:x08-project-main-issuer-receipt> `
  -OutputPath <slot:x09-project-lineage-target>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssurancePlannedCommandDescriptor.ps1 `
  -WorkDescriptorPath <frozen:work-descriptor> -CardPath <frozen:card> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-fixture-change-range-record> `
  -InputManifestPath @(<slot:x09-fixture-change-scope-allowance>,<slot:x09-project-lineage-target>) `
  -OutputPath <slot:x09-fixture-change-planned-command-descriptor>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvRequiredPester.ps1 `
  -Path @(
    'packaging/windows-desktop-node/tests/PcvAssurancePlannedCommandDescriptor.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceBootstrapCommandRunner.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceCommandManifest.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceRepositoryIdentity.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceTaskDispatch.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceBootstrapImportCandidateManifest.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceScope.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceConfinement.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceWriteBroker.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceTelemetry.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceRunnerImages.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceModelDispatch.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceDependencyBinding.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceVerifierModel.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceProcessRunner.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvBoundedExecutor.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceRecoveryDrill.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvIndependentVerifier.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvVerificationAuthority.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceResultTransport.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvAssuranceVerifierWorkflow.Tests.ps1',
    'packaging/windows-desktop-node/tests/PcvVerificationTargetManifest.Tests.ps1'
  ) -ArtifactRoot <frozen:fresh-artifact-root> `
  -OutputSummaryPath <slot:x09-focused-pester-summary>
# X02 owning phase only; X09 imports its command manifest and never invokes these three commands
pwsh -NoProfile -File tools/assurance/runner/New-PcvAssuranceRunnerBuildManifest.ps1 `
  -DependencySelectionDecisionPath @(
    <frozen:consumed-oci-create-intent-decision>,
    <frozen:consumed-windows-create-intent-decision>
  ) `
  -ProviderMutationDecisionPath @(
    <frozen:consumed-oci-runner-build-provider-mutation-decision>,
    <frozen:consumed-windows-runner-build-provider-mutation-decision>
  ) `
  -CandidateRecipeManifestPath <frozen:x02-provider-input-anchor-manifest> `
  -PredeclaredOutputSlotsPath <frozen:x02-runner-build-output-slots> `
  -OutputPath <slot:x02-runner-build-command-manifest>
pwsh -NoProfile -File tools/assurance/runner/Build-PcvAssuranceRunnerImages.ps1 `
  -BuildCommandManifestPath <slot:x02-runner-build-command-manifest> `
  -DependencySelectionDecisionPath @(
    <frozen:consumed-oci-create-intent-decision>,
    <frozen:consumed-windows-create-intent-decision>
  ) `
  -ProviderMutationDecisionPath @(
    <frozen:consumed-oci-runner-build-provider-mutation-decision>,
    <frozen:consumed-windows-runner-build-provider-mutation-decision>
  ) `
  -CandidateRecipeManifestPath <frozen:x02-provider-input-anchor-manifest> `
  -OutputAttestationPath <slot:x02-signed-two-clean-build-attestation> `
  -ArtifactRoot <frozen:fresh-artifact-root>
# After validating the attestation/readbacks, obtain and consume the two subject-matched
# post_create_identity_freeze decisions; neither command below accepts the earlier intent decision.
pwsh -NoProfile -File tools/assurance/runner/New-PcvAssuranceRunnerImages.ps1 `
  -DependencySelectionDecisionPath @(
    <frozen:consumed-oci-post-create-freeze-decision>,
    <frozen:consumed-windows-post-create-freeze-decision>
  ) `
  -CandidateRecipeManifestPath <frozen:x02-provider-input-anchor-manifest> `
  -BuildAttestationPath <slot:x02-signed-two-clean-build-attestation> `
  -OutputPath <slot:x02-measured-runner-images>
# X09 non-mutating readback of the frozen X02 output
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceRunnerImages.ps1 `
  -RunnerImagesPath <frozen:x02-measured-runner-images> `
  -DependencySelectionDecisionPath @(
    <frozen:consumed-oci-post-create-freeze-decision>,
    <frozen:consumed-windows-post-create-freeze-decision>
  ) `
  -BuildAttestationPath <frozen:x02-signed-two-clean-build-attestation> `
  -ArtifactRoot <frozen:fresh-artifact-root> `
  -OutputSummaryPath <slot:x09-runner-image-validation-summary>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvBoundedExecutor.ps1 `
  -AuthorizationDecisionPath <frozen:consumed-x09-fixture-change-decision> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-fixture-change-range-record> `
  -PlannedCommandDescriptorPath <slot:x09-fixture-change-planned-command-descriptor> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-fixture-change-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-fixture-change-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-fixture-change-authorization-request> `
  -DecisionEventPath <frozen:x09-fixture-change-approval-event> `
  -ConsumeEventPath <frozen:x09-fixture-change-consume-event> `
  -DependencySelectionDecisionPath <frozen:consumed-authenticated-model-boundary-final-decision> `
  -RepositoryIdentityPath <frozen:x09-fixture-repository-identity> `
  -ProviderPolicyPath <frozen:authenticated-model-boundary-policy-record> `
  -SpecLockPath <frozen:spec-lock> -CardPath <frozen:card> `
  -ScopeAllowancePath <slot:x09-fixture-change-scope-allowance> `
  -RunnerImagesPath <frozen:x02-measured-runner-images> -CandidateTree <frozen:fixture-start-tree> `
  -ModelDispatchOutputPath <slot:signed-model-dispatch> `
  -ModelResultOutputPath <slot:authenticated-model-result> `
  -ScopeManifestOutputPath <slot:scope-manifest> `
  -ExecutionManifestOutputPath <slot:execution-manifest> `
  -ArtifactRoot <frozen:fresh-artifact-root>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Publish-PcvAssuranceResultObject.ps1 `
  -AuthorizationDecisionPath <frozen:consumed-x09-fixture-change-decision> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-fixture-change-range-record> `
  -PlannedCommandDescriptorPath <slot:x09-fixture-change-planned-command-descriptor> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-fixture-change-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-fixture-change-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-fixture-change-authorization-request> `
  -DecisionEventPath <frozen:x09-fixture-change-approval-event> `
  -ConsumeEventPath <frozen:x09-fixture-change-consume-event> `
  -DependencySelectionDecisionPath <frozen:consumed-result-transport-final-decision> `
  -SpecLockPath <frozen:spec-lock> -CardPath <frozen:card> `
  -OracleManifestPath <frozen:oracle-manifest> `
  -ScopeAllowancePath <slot:x09-fixture-change-scope-allowance> `
  -ScopeManifestPath <slot:scope-manifest> `
  -ExecutionManifestPath <slot:execution-manifest> `
  -ModelResultPath <slot:authenticated-model-result> `
  -TransportPolicyPath <frozen:result-transport-policy-record> `
  -OutputPublicationPath <slot:signed-result-publication> `
  -OutputReceiptPath <slot:result-transport-receipt>
# First X09 transaction finalizer; it cannot include later provider-run outputs
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceCommandManifest.ps1 `
  -WorkDescriptorPath <frozen:work-descriptor> `
  -PlannedCommandDescriptorPath <slot:x09-fixture-change-planned-command-descriptor> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-fixture-change-range-record> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-fixture-change-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-fixture-change-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-fixture-change-authorization-request> `
  -DecisionEventPath <frozen:x09-fixture-change-approval-event> `
  -ConsumeEventPath <frozen:x09-fixture-change-consume-event> `
  -ValidatedInputPath @(
    <frozen:x09-fixture-repository-identity>,
    <frozen:consumed-authenticated-model-boundary-final-decision>,
    <frozen:consumed-result-transport-final-decision>,
    <slot:x09-fixture-change-scope-allowance>,
    <slot:x09-focused-pester-summary>,
    <slot:x09-runner-image-validation-summary>,
    <slot:signed-model-dispatch>,
    <slot:authenticated-model-result>,
    <slot:scope-manifest>,
    <slot:execution-manifest>,
    <slot:signed-result-publication>,
    <slot:result-transport-receipt>
  ) `
  -OutputPath <slot:x09-fixture-change-command-manifest>
# Derive the second range before its distinct provider-administration approval/consume
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceRepositoryIdentity.ps1 `
  -InputPath <frozen:x09-recovery-fixture-repository-identity> `
  -ExpectedKind typed_control_fixture -ExpectedStartCommit <frozen:recovery-fixture-start-commit> `
  -ExpectedStartTree <frozen:recovery-fixture-start-tree>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceScopeAllowance.ps1 `
  -StartCommit <frozen:recovery-fixture-start-commit> `
  -StartTree <frozen:recovery-fixture-start-tree> `
  -RepositoryIdentityPath <frozen:x09-recovery-fixture-repository-identity> `
  -SpecLockPath <frozen:zero-operation-recovery-spec-lock> `
  -CardPath <frozen:zero-operation-recovery-card> `
  -OutputPath <slot:x09-zero-operation-recovery-allowance>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceScope.ps1 `
  -AllowancePath <slot:x09-zero-operation-recovery-allowance>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssurancePlannedCommandDescriptor.ps1 `
  -WorkDescriptorPath <frozen:provider-recovery-work-descriptor> `
  -CardPath <frozen:zero-operation-recovery-card> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-provider-recovery-range-record> `
  -InputManifestPath @(
    <frozen:x09-recovery-fixture-repository-identity>,
    <slot:x09-zero-operation-recovery-allowance>,
    <slot:x09-project-lineage-target>,
    <slot:signed-result-publication>,
    <slot:result-transport-receipt>
  ) `
  -OutputPath <slot:x09-provider-recovery-planned-command-descriptor>
# Consume the second provider-administration decision before this next command
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Import-PcvAssuranceResultObject.ps1 `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-provider-recovery-range-record> `
  -PlannedCommandDescriptorPath <slot:x09-provider-recovery-planned-command-descriptor> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-provider-recovery-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-provider-recovery-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-provider-recovery-authorization-request> `
  -DecisionEventPath <frozen:x09-provider-recovery-approval-event> `
  -ConsumeEventPath <frozen:x09-provider-recovery-consume-event> `
  -SourceTaskDispatchPath <frozen:x09-root-task-dispatch> `
  -SourceRangeRecordPath <frozen:x09-fixture-change-range-record> `
  -SourcePlannedCommandDescriptorPath <slot:x09-fixture-change-planned-command-descriptor> `
  -SourceTaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -SourceRangeRecordPublicationReceiptPath <frozen:x09-fixture-change-range-record-receipt> `
  -SourcePlannedCommandDescriptorPublicationReceiptPath <frozen:x09-fixture-change-planned-descriptor-receipt> `
  -SourceAuthorizationRequestPath <frozen:x09-fixture-change-authorization-request> `
  -SourceDecisionEventPath <frozen:x09-fixture-change-approval-event> `
  -SourceConsumeEventPath <frozen:x09-fixture-change-consume-event> `
  -DependencySelectionDecisionPath <frozen:consumed-result-transport-final-decision> `
  -PublicationPath <slot:signed-result-publication> `
  -ReceiptPath <slot:result-transport-receipt> `
  -TransportPolicyPath <frozen:result-transport-policy-record> `
  -AuthorityPath <frozen:authority-manifest> `
  -OutputVerifiedResultPath <slot:signed-verified-result>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvVerificationDispatch.ps1 `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-provider-recovery-range-record> `
  -PlannedCommandDescriptorPath <slot:x09-provider-recovery-planned-command-descriptor> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-provider-recovery-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-provider-recovery-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-provider-recovery-authorization-request> `
  -DecisionEventPath <frozen:x09-provider-recovery-approval-event> `
  -ConsumeEventPath <frozen:x09-provider-recovery-consume-event> `
  -SourceTaskDispatchPath <frozen:x09-root-task-dispatch> `
  -SourceRangeRecordPath <frozen:x09-fixture-change-range-record> `
  -SourcePlannedCommandDescriptorPath <slot:x09-fixture-change-planned-command-descriptor> `
  -SourceTaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -SourceRangeRecordPublicationReceiptPath <frozen:x09-fixture-change-range-record-receipt> `
  -SourcePlannedCommandDescriptorPublicationReceiptPath <frozen:x09-fixture-change-planned-descriptor-receipt> `
  -SourceAuthorizationRequestPath <frozen:x09-fixture-change-authorization-request> `
  -SourceDecisionEventPath <frozen:x09-fixture-change-approval-event> `
  -SourceConsumeEventPath <frozen:x09-fixture-change-consume-event> `
  -DependencySelectionDecisionPath @(
    <frozen:consumed-verification-authority-final-decision>,
    <frozen:consumed-windows-verifier-final-decision>,
    <frozen:consumed-authenticated-model-boundary-final-decision>
  ) `
  -VerificationAuthorityPath <frozen:authority-manifest> `
  -VerifiedResultPath <slot:signed-verified-result> -SpecLockPath <frozen:spec-lock> `
  -TargetOutputPath <slot:signed-verification-target> `
  -DispatchOutputPath <slot:signed-verification-dispatch>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvAssuranceRecoveryDrill.ps1 `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-provider-recovery-range-record> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-provider-recovery-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-provider-recovery-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-provider-recovery-authorization-request> `
  -DecisionEventPath <frozen:x09-provider-recovery-approval-event> `
  -ConsumeEventPath <frozen:x09-provider-recovery-consume-event> `
  -DependencySelectionDecisionPath @(
    <frozen:consumed-verification-authority-final-decision>,
    <frozen:consumed-windows-verifier-final-decision>,
    <frozen:consumed-authenticated-model-boundary-final-decision>
  ) `
  -PlannedCommandDescriptorPath <slot:x09-provider-recovery-planned-command-descriptor> `
  -RepositoryIdentityPath <frozen:x09-recovery-fixture-repository-identity> `
  -ScopeAllowancePath <slot:x09-zero-operation-recovery-allowance> `
  -FaultSchedulePath <frozen:x09-lease-revoke-and-infrastructure-loss-schedule> `
  -VerificationAuthorityPath <frozen:authority-manifest> `
  -TargetManifestPath <slot:signed-verification-target> `
  -DispatchEnvelopePath <slot:signed-verification-dispatch> `
  -ExpectedWorkflowCommit <frozen:x05-main-workflow-commit> `
  -OutputLeaseRevokeReceiptPath <slot:x09-lease-revoke-receipt> `
  -OutputProcessKillReceiptPath <slot:x09-process-kill-receipt> `
  -OutputBlockedFirstRunPath <slot:x09-blocked-first-run> `
  -OutputEvidenceSurvivalPath <slot:x09-evidence-survival> `
  -OutputFreshDispatchAttemptPath <slot:x09-fresh-dispatch-attempt> `
  -OutputDeliveryReceiptPath <slot:signed-provider-delivery-receipt> `
  -OutputRunReceiptPath <slot:signed-provider-run-receipt> `
  -OutputCredentialRevokeReceiptPath <slot:signed-provider-credential-revoke-receipt> `
  -OutputArtifactManifestPath <slot:signed-remote-artifact-manifest> `
  -OutputReconciliationPath <slot:x09-uncertain-result-reconciliation> `
  -LocalArtifactRoot <frozen:fresh-artifact-root> `
  -RemoteArtifactRoot <frozen:fresh-remote-artifact-root>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceCommandManifest.ps1 `
  -WorkDescriptorPath <frozen:provider-recovery-work-descriptor> `
  -PlannedCommandDescriptorPath <slot:x09-provider-recovery-planned-command-descriptor> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath <frozen:x09-provider-recovery-range-record> `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath <frozen:x09-provider-recovery-range-record-receipt> `
  -PlannedCommandDescriptorPublicationReceiptPath <frozen:x09-provider-recovery-planned-descriptor-receipt> `
  -AuthorizationRequestPath <frozen:x09-provider-recovery-authorization-request> `
  -DecisionEventPath <frozen:x09-provider-recovery-approval-event> `
  -ConsumeEventPath <frozen:x09-provider-recovery-consume-event> `
  -SourceCommandManifestPath @(
    <frozen:x02-command-manifest>,
    <frozen:x04-command-manifest>,
    <frozen:x06-command-manifest>,
    <frozen:x08-command-manifest>,
    <slot:x09-fixture-change-command-manifest>
  ) `
  -ValidatedInputPath @(
    <frozen:x09-recovery-fixture-repository-identity>,
    <frozen:consumed-result-transport-final-decision>,
    <frozen:consumed-verification-authority-final-decision>,
    <frozen:consumed-windows-verifier-final-decision>,
    <frozen:consumed-authenticated-model-boundary-final-decision>,
    <slot:x09-zero-operation-recovery-allowance>,
    <slot:x09-project-lineage-target>,
    <slot:x09-lease-revoke-receipt>,
    <slot:x09-process-kill-receipt>,
    <slot:x09-blocked-first-run>,
    <slot:x09-evidence-survival>,
    <slot:x09-fresh-dispatch-attempt>,
    <slot:x09-uncertain-result-reconciliation>,
    <slot:signed-verified-result>,
    <slot:signed-verification-target>,
    <slot:signed-verification-dispatch>,
    <slot:signed-provider-delivery-receipt>,
    <slot:signed-provider-run-receipt>,
    <slot:signed-provider-credential-revoke-receipt>,
    <slot:signed-remote-artifact-manifest>
  ) `
  -OutputPath <slot:command-manifest>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceBootstrapImportCandidateManifest.ps1 `
  -ExpectedInventoryDescriptorPath <frozen:x09-expected-inventory-descriptor> `
  -SourceLocatorManifestPath <frozen:x09-original-source-locator-manifest> `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -ProjectTargetManifestPath <slot:x09-project-lineage-target> `
  -OutputPath <slot:x09-import-candidate-manifest>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapImportCandidateManifest.ps1 `
  -InputPath <slot:x09-import-candidate-manifest> `
  -ExpectedInventoryDescriptorPath <frozen:x09-expected-inventory-descriptor> `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -ProjectTargetManifestPath <slot:x09-project-lineage-target>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Publish-PcvAssuranceBootstrapArtifact.ps1 `
  -InputManifestPath <slot:x09-import-candidate-manifest> `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -LogicalPrefix assurance-bootstrap://import-candidates/NHA-X09/ `
  -OutputReceiptPath <slot:x09-import-publication-receipt>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvAssuranceBootstrapExit.ps1 `
  -CommandManifestPath <slot:command-manifest> -ArtifactRoot <frozen:fresh-artifact-root> `
  -TaskDispatchPath <frozen:x09-root-task-dispatch> `
  -RangeRecordPath @(
    <frozen:x09-fixture-change-range-record>,
    <frozen:x09-provider-recovery-range-record>
  ) `
  -PlannedCommandDescriptorPath @(
    <slot:x09-fixture-change-planned-command-descriptor>,
    <slot:x09-provider-recovery-planned-command-descriptor>
  ) `
  -TaskDispatchPublicationReceiptPath <frozen:x09-root-task-dispatch-receipt> `
  -RangeRecordPublicationReceiptPath @(
    <frozen:x09-fixture-change-range-record-receipt>,
    <frozen:x09-provider-recovery-range-record-receipt>
  ) `
  -PlannedCommandDescriptorPublicationReceiptPath @(
    <frozen:x09-fixture-change-planned-descriptor-receipt>,
    <frozen:x09-provider-recovery-planned-descriptor-receipt>
  ) `
  -AuthorizationRequestPath @(
    <frozen:x09-fixture-change-authorization-request>,
    <frozen:x09-provider-recovery-authorization-request>
  ) `
  -DecisionEventPath @(
    <frozen:x09-fixture-change-approval-event>,
    <frozen:x09-provider-recovery-approval-event>
  ) `
  -ConsumeEventPath @(
    <frozen:x09-fixture-change-consume-event>,
    <frozen:x09-provider-recovery-consume-event>
  ) `
  -ReplicaProviderPolicyPath <frozen:a00-replica-provider-policy> `
  -ReplicaReceiptRoot <slot:x09-payload-replica-receipt-root> `
  -ExpectedTargetManifestPath <slot:x09-project-lineage-target> `
  -TypedControlTargetManifestPath <slot:signed-verification-target> `
  -ImportCandidateManifestPath <slot:x09-import-candidate-manifest> `
  -ImportCandidatePublicationReceiptPath <slot:x09-import-publication-receipt> `
  -OutputPath <slot:bootstrap-exit>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Publish-PcvAssuranceBootstrapArtifact.ps1 `
  -InputPath <slot:bootstrap-exit> `
  -AuthorizationDecisionPath <frozen:consumed-x09-provider-administration-decision> `
  -LogicalPrefix assurance-bootstrap://exits/NHA-X09/ `
  -OutputReceiptPath <slot:x09-exit-publication-receipt>
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Test-PcvAssuranceBootstrapExit.ps1 `
  -InputPath <slot:bootstrap-exit> `
  -ExpectedTargetManifestPath <slot:x09-project-lineage-target> `
  -TypedControlTargetManifestPath <slot:signed-verification-target> `
  -ImportCandidateManifestPath <slot:x09-import-candidate-manifest> `
  -ImportCandidatePublicationReceiptPath <slot:x09-import-publication-receipt> `
  -PublicationReceiptPath <slot:x09-exit-publication-receipt> `
  -ReplicaReceiptRoot <slot:x09-payload-replica-receipt-root>
```

The first five shown commands are X09's non-mutating controls before the first decision. The fixture
allowance and signed project-lineage target are derived first; the fixture-change descriptor then binds
only the first measured range. After its tracked-change decision is consumed, only that range executes
and its finalizer seals `x09-fixture-change-command-manifest`, including the required-Pester, runner-image
readback, model dispatch/result, scope/execution and publication/receipt outputs. The zero-operation
recovery allowance and provider-recovery descriptor are derived from the now-immutable first result and
the closed recovery-fixture repository identity; they contain no repository operation. Only after the second provider-administration decision is consumed may the protected
zero-operation lease-revocation drill, Authority import/target signing and remote dispatch range execute.
Every top-level executable is launched by Plan 2's exact bootstrap-command runner, which emits its
measured run manifest and raw logs; commands shown for X02/X04/X06/X08 denote already completed owning
phases and immutable source manifests, not X09 reruns. The final measured X09 command manifest compares
only the second range to its descriptor and incorporates every first-range output only through the sealed
`x09-fixture-change-command-manifest`; none may also appear as a direct second-range validated-input slot.
Missing source ownership, an orphan output, direct cross-range injection, duplicate ownership or recovery-
fixture identity drift rejects.
Import-candidate conditional publication, payload-replica/exit generation, full-envelope conditional
publication and remote exit validation are the second Packet's protected finalization chain and are never
self-listed. Their exact tool/argv/provider/prefix facts remain bound by that consumed decision; every
conditional-create/readback receipt and remote byte stream must validate. No descriptor or Packet hides a
fixture repository operation inside the provider-administration range.

`<frozen:...>` denotes an immutable input value or path resolved into the signed card/command manifest
before execution. `<slot:...>` denotes a predeclared output path whose content, commit, digest, locator
or provider run ID can exist only after the preceding command succeeds. Slots are never substituted as
pre-run claims: each consumer opens the immediately preceding schema-validated output and derives every
runtime content-addressed value from it. In particular, result commit comes only from the execution
manifest, post-run scope manifest comes only from the sealed start/result Git objects and frozen scope
allowance, result locator only from publication/receipt, and workflow run/artifact identities only from
provider delivery/run receipts. Literal angle-bracket values are never executable.

## Plan 3 exit gate

- [ ] Trusted Git pre-run allowance rejects future-result/new-blob claims; post-run scope manifest is
      derived only from exact start/result Git objects and matches the allowance bijectively.
- [ ] OS layer denies direct repository/trust-root/network/admin/secret access.
- [ ] Exact write broker accepts only frozen path+operation and replay is impossible.
- [ ] Model calls are authenticated/typed, tool-mediated and never expose broker/verifier credentials.
- [ ] Every accepted batch yields a rehashed immutable read-only candidate snapshot.
- [ ] Full raw process evidence, timeout and process-tree termination are verified.
- [ ] OS telemetry independently accounts for process/file/network attempts without unexplained gaps.
- [ ] Independent Authority controls exact clean target and differs by provider/App/environment,
      trust domain, credential and workspace.
- [ ] The signed publication/receipt/bundle/verified-result chain validates end to end, and the
      `verification_authority` issuer branch cannot satisfy or weaken `provider_event_oidc`.
- [ ] Negative corpus reject rate is 100%; positive canary produces exactly one approved change.
- [ ] Product path changes and host mutation count are zero.
- [ ] Landing and activation remain disabled/RED pending Plans 4–6.
