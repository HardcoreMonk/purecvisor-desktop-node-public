# PureCVisor Assurance Evidence, Decision Packet and Trust Dashboard Implementation Plan

> **Status:** proposed child plan 4; starts only after Plan 3 proves bounded execution and independent
> verification.
>
> **Execution class:** artifact, notary, Packet, decision and Dashboard code is trust-root `L`,
> `gpt-5.6-sol`, `ultra`, actual `Release`, with a different trust-domain Sol verifier. This plan does not
> authorize product or host mutation.

**Goal:** Preserve complete execution/verification evidence outside the working tree, attest it with an
independent notary, generate immutable decision requests and show a fail-closed Trust Dashboard that a
user can rely on without inspecting source.

**Architecture:** Raw files are encrypted and content-addressed before publication to a user-selected
WORM/object-lock provider. A separate Evidence Notary verifies bytes, actor/run/target and retention,
then signs an append-only attestation. Decision Packets reference only verified proofs. Approval and
consumption are separate immutable events. Authoritative JSON deterministically generates Packet and
Dashboard Markdown.

**Prerequisite:** `NHA-X09-bounded-executor-verifier-v1` and Plan 2 contract authority are fresh,
accessible and exact-main-bound.

**Source design:** `docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-no-human-code-review-assurance-design.md`
§§8, 11.6, 12, 13, 15 and NHR-001, NHR-014..021, NHR-028..029.

---

## External selection gate

Before implementation task E02, present a Decision Packet with
`packet_type=requirements_approval`, `phase=planning_authorization` and
`purpose=artifact_store_notary_selection`, then obtain an exact decision for:

1. one immutable storage backend with versioning, retention lock and read-after-write verification;
2. one independent signing/notary mode with a verifiable identity and revocation story;
3. one retention schedule for execution, verification, review, decision, landing and secret-tombstone
   classes;
4. one credential custody boundary that excludes executor and product code.

Acceptable storage capability examples are S3-compatible Object Lock compliance mode or Azure immutable
blob time-based/legal-hold policy. GitHub Actions artifacts may transport data but are not, alone, WORM
or notary evidence. A local writable directory is not acceptable production evidence. The selected
provider, region/endpoint, API version, object-lock policy, encryption mode and tool/image digest are
frozen in the Packet; this plan does not guess or purchase a provider.

This selection authority is consumed exactly once immediately before E02 freezes the chosen policy and
integration inputs. It authorizes no account/KMS/bucket/container/notary/credential provisioning and no
object write. If any selected resource does not already exist, or the sandbox proof will create an
external object, obtain and consume a distinct external `packet_type=trust_root`,
`phase=execution_authorization` decision immediately before the first exact side effect; no undefined
purpose extension is permitted. If account/KMS/bucket/container/notary policy, key, credential or role
administration is required, the decision has `execution_scope=provider_administration`, `operations=[]`
and selects one closed provider-resource branch. `existing_resource` binds exact assigned resource/key/
principal IDs plus signed current readback. `create_resource_intent` instead binds desired stable locators/
names, immutable inputs, exact create argv/capabilities and typed assigned-ID/key/credential output slots;
future assigned values are forbidden. Both bind provider operations, cost ceiling, before-state, readback,
cleanup/conditional rollback, revocation and expiry. Create forward/readback receipts supply measured IDs,
and E02's protected generator writes them into the policy candidate without altering or backfilling the
consumed request. If every resource/policy/credential already exists and the only side effect is the
prebound conditional creation of immutable nonsecret sandbox evidence objects, the mutually exclusive
decision has `execution_scope=artifact_only`, `operations=[]`
and binds only exact object prefixes, publisher/readback/notary principals, provider/retention,
conditional-create/readback and abort/reconciliation. Either branch cannot authorize E02 tracked edits,
landing or the other branch's side effects. E02's bootstrap code decision remains separate.

E04's generator does not exist yet at this gate. Requirement Authority therefore creates the request
envelope directly from Plan 2's already trusted `decision-packet.schema.json`, and two independent Sol
actors validate/canonicalize it with the locked contract tool. The authenticated approval and one-time
E02 consumption are preserved as external immutable events. E05 later publishes their complete import-
candidate inventory; E06 alone performs separately authorized canonical import. The same preservation
rule applies to any separate provider-setup request/approval/consume/outcome. This process cannot change
a request after approval and does not reuse Plan 1's already consumed bootstrap decision.

## Early Plan 4 bootstrap bridge

E01–E05 are the final users of the Program's bounded bootstrap/shadow protocol because E03 notary, E04
Packet generator and E05 decision plane do not all exist on `main` before them. Each starts from the
preceding verified post-merge `main`, consumes a fresh external `packet_type=trust_root`,
`phase=execution_authorization` decision over exact path operations before its first tracked edit, then
after actual Release and independent Sol review consumes a distinct `phase=landing_authorization`
decision over the immutable candidate. It lands unchanged through ordinary PR/CI and publishes a
`pcv-assurance-bootstrap-exit-v1` envelope whose E work ID is permitted by the Program schema. External
request/approval/consume events use Plan 2's locked schema/validator and two signer domains; they are not
canonical decisions or assurance GREEN. E02's storage-provider selection is an additional planning
decision and never substitutes for E02 task execution/landing authority.

After E05 merges, its consumed landing transaction first emits only the mandatory raw landing-equivalence
attestation. Then a
fresh canonical artifact-only `trust_root/execution_authorization` Packet authorizes a complete early-E
import-candidate inventory covering the external
storage/notary selection request/approval/E02 consume, every applicable provider-setup request/approval/
consume plus signed forward/readback/revocation/selected-rollback outcome; the distinct interim-channel-
continuation request/approval/consume and its no-mutation readback or provider-setup/outcome set; E01–E04
execution/landing events and exits; plus E05 execution/landing events. The manifest excludes E05's
terminal exit. The now-merged E02/E03/E05 code makes canonical authorization possible, but E05's consumed
landing authority cannot authorize the post-merge manifest/publication. The new publication Packet binds
the exact Plan 2 generator/validator, expected set, prefix/provider/retention/readback/notary and is consumed
before generation/publication. Its request/approval/consume are already canonical prerequisites and are
not wrapped as bootstrap import candidates or imported again. E06's separately authorized artifact-only
pre-step imports prior history only. After E06's tracked candidate lands, its terminal artifact-only range
imports E06 self-history and atomically closes the task-dispatch namespace transition. All E06 ranges retain
one bootstrap-grade dispatch manifest; only E07–E09 use `assurance-control://task-dispatch/...`, and
neither a mixed namespace nor a later bootstrap exception is permitted.

## File map

**Additional contracts**

- Verify Plan 2's `docs/superpowers/plans/luna-completion/contracts/decision-packet.schema.json`
  already implements the exhaustive tuple table below. Any mismatch is a blocked Plan 2 defect and a
  separate trust-root revision, not an opportunistic E01 schema edit.
- Reuse Plan 2's protected `bootstrap-import-candidate-manifest.schema.json`, New/Test manifest tools and
  focused tests unchanged for E05's post-merge early-E inventory.
- Create `docs/superpowers/plans/luna-completion/contracts/artifact-manifest.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/artifact-store-receipt.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/notary-attestation.schema.json`.
- Create `docs/superpowers/plans/luna-completion/contracts/artifact-tombstone.schema.json`.
- Create `docs/superpowers/plans/luna-completion/artifact-policy.schema.json`.
- Create `docs/superpowers/plans/luna-completion/artifact-policy.json` after provider selection.

**Artifact publication and access**

- Create `packaging/windows-desktop-node/tools/PcvAssuranceArtifactManifest.psm1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceArtifactManifest.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceArtifactStore.psm1`.
- Create `packaging/windows-desktop-node/tools/Publish-PcvAssuranceArtifacts.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceArtifactAccess.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceArtifactManifest.Tests.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceArtifactStore.Tests.ps1`. Fake-provider cases are
  in-memory instances inside those two files; no unlisted fixture path is created.

**Notary, Packet, decision and projection**

- Create `packaging/windows-desktop-node/tools/PcvAssuranceNotary.psm1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceAttestation.ps1`.
- Create `packaging/windows-desktop-node/tools/Test-PcvAssuranceAttestation.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceDecisionPacket.psm1`.
- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceDecisionPacket.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceDecisionRecord.psm1`.
- Create `packaging/windows-desktop-node/tools/Add-PcvAssuranceDecisionEvent.ps1`.
- Create `packaging/windows-desktop-node/tools/Consume-PcvAssuranceDecisionEvent.ps1`.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceTerminalCancellation.ps1`.
- Modify Plan 2's `packaging/windows-desktop-node/tools/Invoke-PcvAssuranceConsumeAndClaim.ps1` and
  `packaging/windows-desktop-node/tools/Test-PcvAssurancePairState.ps1` only to bind the canonical E05
  Decision-Plane store adapter; their Plan 2 schemas and client contract remain unchanged.
- Create `packaging/windows-desktop-node/tools/Import-PcvAssuranceBootstrapHistory.ps1`.
- Create `packaging/windows-desktop-node/tools/PcvAssuranceProjection.psm1`.
- Create `packaging/windows-desktop-node/tools/Update-PcvAssuranceTrustDashboard.ps1`.
- Create `packaging/windows-desktop-node/tests/PcvAssuranceNotary.Tests.ps1`,
  `packaging/windows-desktop-node/tests/PcvAssuranceDecisionPacket.Tests.ps1`,
  `packaging/windows-desktop-node/tests/PcvAssuranceDecisionRecord.Tests.ps1`,
  `packaging/windows-desktop-node/tests/PcvAssuranceDecisionPlanePairState.Tests.ps1`, and
  `packaging/windows-desktop-node/tests/PcvAssuranceProjection.Tests.ps1`.

Plan 2 T02 materializes and owns the `decision-record.schema.json` file contract; Plan 1 remains the
normative semantic owner of authority/transition policy. E05 may add only a separately approved Plan 2
contract revision when WORM receipt/notary fields cannot be expressed by that version.

**Secret incident and workflow**

- Create `packaging/windows-desktop-node/tools/New-PcvAssuranceArtifactTombstone.ps1` and
  `packaging/windows-desktop-node/tests/PcvAssuranceArtifactTombstone.Tests.ps1`.
- Create `.github/workflows/assurance-evidence.yml` and
  `packaging/windows-desktop-node/tests/PcvAssuranceEvidenceWorkflow.Tests.ps1`.
- Materialize generated JSON/Markdown only under the exact
  `docs/superpowers/plans/luna-completion/decision-packets/` and Dashboard paths defined by the design.

Add all files to the protected-path manifest before their implementation PR.

## Normative task-dispatch matrix

Each row uses the Program §5.1 canonical Test, Red and Final argv with the row ID as exact work ID. The
signed dispatch expands the File-map entries and named fixtures to exact paths and complete argv before
consume. Final is actual Release plus a different-trust-domain Sol verifier. Future provider-assigned
facts remain a deferred child range until protected readback resolves them; no caller may complete it.

| Work ID | Ordered path/range closure | RED or allowed N/A | Implementation/finalizer boundary | Rollback | Commit/PR boundary |
|---|---|---|---|---|---|
| NHA-E01 | `exact_paths`, Additional-contract/policy/fixture entries | unknown tuple, mutable store, same notary or mixed resource branch rejects | schemas, exact negative oracle and policy candidate only | whole-commit revert; conditional setup rollback | `tracked_pr` |
| NHA-E02 | ordered conditional `provider_administration` resource child, `exact_paths` artifact-store entries and `artifact_only` sandbox objects | overwrite/retention/readback/credential escape RED | selected store policy, publisher/access tools and sandbox evidence only | revoke/conditional resource rollback; whole tracked revert | provider child/sandbox `artifact_only_no_commit`; tracked store range `tracked_pr` |
| NHA-E03 | `exact_paths`, exact notary module/wrappers/test entries | same actor/key, tamper, missing raw bytes and revocation RED | independent notary attestation only | whole-commit revert | `tracked_pr` |
| NHA-E04 | `exact_paths`, exact Packet module/wrapper/test entries; all tuple cases are in-memory in the exact named test and no tracked fixture path exists | unknown tuple, future fact, wrong consumer and self-approval RED | authoritative JSON Packet generation only | whole-commit revert | `tracked_pr` |
| NHA-E05 | ordered `exact_paths` decision-record/add-consume-terminal wrappers, canonical `consume_and_claim` store binding and exhaustive pair-state tests with Plan2 schemas/import assets read-only; post-merge `provider_administration` sandbox one-winner/terminal-readback canary; then `artifact_only` early-E inventory/exit range | forged/replay/double-consume, losing/partial claim terminal append, owner/epoch/reason drift and incomplete import RED | append-only decisions plus sole atomic Decision Plane; isolated canary; final range owns only import candidates/exit | whole tracked revert; revoke/reconcile canary; invalidate artifact range | tracked decision range `tracked_pr`; later ranges `artifact_only_no_commit` |
| NHA-E06 | ordered `artifact_only` prior-history canonical import, `exact_paths` projection/Dashboard candidate plus deferred verified landing, then post-landing `artifact_only` E06 self-history import/dispatch-store cutover | missing/extra/reordered source, false GREEN, Markdown drift, missing self-dispatch or mixed namespace RED | import prior candidates, deterministic JSON/Markdown projection, then one terminal bootstrap→control cutover attestation | whole tracked revert before cutover; preserve imported history; disable later dispatch on uncertain cutover | import/cutover `artifact_only_no_commit`; projection `tracked_pr` |
| NHA-E07 | ordered `exact_paths` tombstone module/wrapper/test range then post-merge `provider_administration` sandbox-KMS drill | deletion, locator leak, secret replay and unnotarized tombstone RED | append-only redaction/tombstone code, then isolated KMS revoke/readback drill | whole tracked revert; revoke/reconcile sandbox | `tracked_pr`; drill `artifact_only_no_commit` |
| NHA-E08 | ordered `exact_paths` evidence workflow/static-test range then post-merge `provider_administration` remote canary | PR-controlled policy, success-with-missing-artifact and mutable ref RED | fail-closed workflow, then one-run remote delivery/readback/revoke canary | whole tracked revert; revoke/reconcile canary | `tracked_pr`; canary `artifact_only_no_commit` |
| NHA-E09 | `provider_administration`, `operations=[]`, exact verifier-read/credential/revoke plus WORM/notary/Dashboard/exit prefixes | inaccessible byte, stale notary, branch-history or inventory mismatch rejects | existing-target read-only verifier canary and final E09 exit only; no tracked edit | revoke/reconcile; preserve failure | `artifact_only_no_commit` |

## Task NHA-E01: Freeze artifact/notary policy and negative oracle

**Files:** four new contract schemas, decision-packet conformance fixtures, artifact policy schema, and fixtures under
`packaging/windows-desktop-node/tests/fixtures/assurance-evidence/`.

- [ ] **Step 1: Obtain the external selection decision**

Create two separate planning transactions through the A00-frozen interim authenticated channel. The
first Packet has purpose `artifact_store_notary_selection` and includes cost/retention implications,
exact provider/tool versions, credential roles, object-lock behavior, encryption, deletion/secret
procedure, availability dependency and fallback. Its request/approval/consume event is owned by E02's
selection boundary. The second has purpose `interim_decision_channel_continuation` and binds the exact
A00 provider/channel/principal/authentication/key-or-OIDC/nonce/expiry/revocation profile that E05 will
accept through the durable L04 cutover. It receives and consumes its own request/digest decision; it cannot be combined with
or consumed as the storage decision.

Both use the closed `requirements_approval/planning_authorization` tuple and authorize no side effect.
Do not create credentials in the repository. DENY or no decision blocks its consumer without reducing
policy. Before storage/notary resources are created, and separately before any new interim-channel App/
environment/credential adapter is provisioned, consume a distinct `trust_root/execution_authorization`
provider-setup Packet with `execution_scope=provider_administration`, `operations=[]`, binding exact
provider operations and one closed resource branch. `existing_resource` binds exact assigned resource/
key/principal IDs and signed current readback. `create_resource_intent` binds desired stable locators/
names, immutable inputs, exact create argv/capabilities and typed assigned-ID/key/credential output slots,
and forbids future assigned values. Both bind before-state, least privileges, credential custody/rotation,
readback oracle, cost and one conditional rollback; signed forward/readback events provide the measured
IDs for E02's protected policy generator without backfilling the request. The existing A00 channel
authenticates both setup decisions. Publish signed forward/readback/revocation and selected rollback
events. If the existing channel already satisfies the selected continuation, record signed no-mutation
readback and create no setup authority; before publishing that readback, instead consume a distinct
`trust_root/execution_authorization` Packet with `execution_scope=artifact_only`, `operations=[]`, binding
the exact profile/read tool and argv digests, output prefix/provider/retention, conditional create,
separate readback/notary principals, expiry and abort/reconciliation. Its finalizer owns only that no-
mutation readback. Selection approval never substitutes for provider setup or an object write.

- [ ] **Step 2: Freeze RED fixtures**

Create one fixture and stable expected error per case:

```text
missing-object.json
http-403.json
http-404.json
hash-mismatch.json
size-mismatch.json
expired.json
retention-too-short.json
mutable-receipt.json
wrong-producer.json
wrong-target.json
truncated-log.json
unsigned-manifest.json
revoked-notary.json
secret-detected.json
duplicate-role.json
```

Expected RED: no repository component can verify these cases end to end.

- [ ] **Step 3: Define exact schemas and policy**

Artifact manifest requires logical URI, content SHA-256, byte size, MIME, role/class, producer actor,
run/card/commit/tree/spec/oracle, created/expiry UTC, retention class, encryption descriptor, secret-scan
result and local source. Store receipt requires immutable provider object/version, lock mode/until,
server checksum, readback checksum/time and provider identity. Notary attestation binds manifest and
receipt digests, signer identity/key or OIDC subject, signing tool digest, timestamp/transparency receipt
and revocation status.

Plan 2's `decision-packet.schema.json` encodes this exhaustive closed tuple table; no unlisted tuple is
valid:

| `packet_type` | exact `phase` | Required payload branch | Prohibited payload branch | Consumable authority |
|---|---|---|---|---|
| `requirements_approval` | `planning_authorization` | start commit/tree, requirement inputs, exact allowed operations/outcomes, oracle/risk/rollback | result/candidate/change-set, actual gates, GREEN | once before the exact planning write/action |
| `requirements_approval` | `landing_authorization` | exact result commit/tree/change-set and deterministic requirement validation | future queue/final SHA, planned-as-PASS | once before that requirement/spec candidate lands |
| `spec_revision` | `planning_authorization` | start commit/tree, exact spec operations/outcomes and review oracle | result/candidate/change-set, actual gates, GREEN | once before the exact spec work |
| `spec_revision` | `landing_authorization` | exact result commit/tree/change-set and actual spec-review evidence | future queue/final SHA, planned-as-PASS | once before that spec candidate lands |
| `residual_risk` | `landing_authorization` | exact result candidate, actual gates, P2/P3 finding IDs, mitigation/expiry | P0/P1 acceptance, hard-gate override, assurance GREEN | once as risk acknowledgement only |
| `trust_root` | `execution_authorization` | start commit/tree and exactly one closed `execution_scope`: nonempty `tracked_change`, zero-operation `artifact_only`, or zero-repo-operation `provider_administration`; exact commands/capabilities/preconditions/oracles/finalizers, risk/rollback | mixed scope, result/candidate/change-set, actual gates, PASS/GREEN | once before the first exact scoped action |
| `trust_root` | `landing_authorization` | exact result commit/tree/change-set and actual required gate/review artifacts | future queue/final SHA, planned-as-PASS | once before unchanged candidate landing |
| `mutation_authorization` | `execution_authorization` | closed `mutation_kind`: `brokered_code_change` requires start commit/tree, card/spec/oracle/scope, exact path operations/broker lease/commands/capabilities/revert; `host_or_artifact_mutation` selects a closed category branch—`package_service/build` requires exact source/recipe/output artifact and forbids host, while install/TLS/Hyper-V/lifecycle rollback requires exact host plus category/artifact/commands/capabilities/rollback oracle | either branch's result/candidate/change-set, executed trace, PASS/GREEN; all fields from the other branch | once immediately before the first exact side effect |
| `release_change` | `landing_authorization` | exact result candidate/change-set, actual gates/artifacts and release boundary | future queue/final SHA, planned-as-PASS | once before exact release pointer/state landing |
| `promotion` | `landing_authorization` | exact product artifact/provenance/current-state candidate and actual release gates | unproved signing/publication, future final SHA | once before exact promotion landing/action |
| `landing_attestation` | `landing_authorization` | exact implementation/result candidate, change-set, actual gates and provider facts | future queue/final SHA or caller success boolean | once before exact candidate enqueue/merge |
| `campaign_summary` | `projection_non_executable` | exact immutable child Packet/decision/result locators and aggregate status | commands, credentials, consume target, execution authority | never |

The design enum has nine unique values; the table has multiple rows only where a type intentionally
supports planning/execution and later landing phases. Queue candidate/final merge facts remain separate
append-only landing-equivalence attestations as required by the source design. `campaign_summary` is
non-consumable and `residual_risk` never upgrades assurance color.
The two `mutation_authorization` shapes are JSON Schema conditional branches under the same listed tuple,
not an open union. Inside `host_or_artifact_mutation`, category conditionals also reject a host on the
unprivileged `package_service/build` branch and require an exact host on install, TLS, Hyper-V and
lifecycle-rollback branches. Missing/unknown kind/category, mixed code/host fields or aggregate
multi-card scope rejects.

The `requirements_approval` row is further closed by Plan 2 T02; no new purpose may be introduced here:

| exact `purpose` | exact phase/consumer | Required specialization | Forbidden/cardinality |
|---|---|---|---|
| `verification_target_issuer_selection` | planning / stage-bound T06 consumer | closed `selection_stage`; existing exact identity/readback, create intent, or post-create measured identity/receipts | no mixed/backfilled/future ID or side effect/result; existing has one Packet/consume, create has intent then post-create Packets each consumed once |
| `dependency_selection` | planning / stage-bound X02/dependency consumer | closed `selection_stage`; existing exact dependency/readback, create intent, or post-create measured dependency/receipts | no mixed/backfilled/future ID or setup/result; existing has one Packet/consume, create has intent then post-create Packets each consumed once |
| `artifact_store_notary_selection` | planning / E02 policy freeze | exact store/notary options, retention/cost and outcome | no provider write/result; one consume |
| `interim_decision_channel_continuation` | planning / E01 channel-profile freeze | exact current channel/readback or separately authorized setup outcome | no cutover/provider mutation; one consume |
| `landing_provider_mode_owner_selection` | planning / L02 selection-record freeze | exact repository/provider mode/App/owner/CODEOWNER outcome | no App/ruleset change; one consume |
| `tls_rehearsal_not_applicable` | planning / exact campaign planner | observed no-impact oracle and campaign/host/artifact scope | no operations/commands/capabilities or TLS decision/reservation; one planning consume |
| `packet_only_user_exercise` | planning / P06 exercise-terminal consumer | synthetic ID/digest/grammar and no execution target | cannot match product/host/tracked/landing/mutation consumer; APPROVE one terminal consume, DENY/REQUEST-CHANGES zero |
| `pilot_selection_approval` | landing / exact P03 selection candidate | immutable selection result/tree/change-set and actual review evidence | no product/host mutation; one selection-landing consume |

Every purpose binds exact start/subject/oracle/risk/expiry, rejects a cross-consumer replay and follows the
DENY/REQUEST-CHANGES zero-consume rule. The table is a semantic projection of the T02 schema, not an E01
extension point.

For the first two purposes, each Packet also has one closed `selection_subject_id`:
`verification_target_issuer` for issuer selection, or exactly one of `oci_executor`, `windows_verifier`,
`verification_authority`, `authenticated_model_boundary`, `result_transport` for dependency selection.
Mixed stages are allowed only across distinct subject chains, never inside one Packet. `existing_identity`
consumes only into the named issuer/dependency-record
freeze. `create_identity_intent` consumes only into the exact provider-setup-request freeze and contains
desired locators/inputs/argv/output slots but no assigned ID/key. After the separate provider-
administration consume and signed readback, `post_create_identity_freeze` binds that unique intent and
provider event chain, measured IDs/key/revocation and consumes only into the originally named record
freeze. A post-create Packet without its intent/receipts, a second intent/post-create Packet, reordering,
cross-purpose/cross-subject chaining or mutation of the first request rejects.

Every consumable execution/landing row in the exhaustive table also requires Plan 2's closed
`finalizer_policy`: exact protected tool/argv digests, nonempty allowed output prefixes, provider/
retention, conditional-create/readback/notary and abort/reconciliation. It can emit only the request's
mandatory raw result/candidate-verification/landing/child-exit outputs. Planning-only Decision-Plane event
append is intrinsic and cannot carry an arbitrary publisher; `campaign_summary` forbids the policy.

- [ ] **Step 4: Commit the oracle separately**

External schema validation and independent Sol review must pass valid fixtures and reject all invalid
fixtures for the intended reason. Commit `test: freeze assurance evidence policy and failures` before
implementation.

## Task NHA-E02: Build content-addressed artifact manifests and storage adapter

**Files:** artifact manifest/store modules, three wrappers and focused tests.

- [ ] **Step 1: Verify E01 RED through missing commands**

Before any code edit, complete E02's first ordered range. If E01 selected resource creation, revalidate and
consume its distinct provider-administration setup decision, create only the exact store/key resource,
independently read it back and publish the signed setup/rollback receipts. If an existing resource was
selected, use only E01's separately authorized signed no-mutation readback and create nothing. This range
cannot edit the repository, publish a sandbox object or authorize the later candidate. Then run every
frozen fixture through local fake providers and preserve expected missing-validator/storage failures.

- [ ] **Step 2: Create manifests from bytes, never claims**

Open each source with no-follow semantics, reject reparse/symlink and size changes during read, scan for
secrets, compute SHA-256 and size from the same byte stream, then write a canonical manifest envelope.
No caller-supplied result/PASS field overrides measured facts. Logical URI format is:

```text
assurance://runs/<CARD-ID>/<TARGET-TREE>/<RUN-ID>/<ROLE>/<SHA256>
```

- [ ] **Step 3: Encrypt before external publication**

Generate a per-run data-encryption key through the approved key service. Store only key reference and
algorithm metadata, never plaintext key. This enables cryptographic destruction if immutable ciphertext
later proves secret-bearing. Executor cannot call key or store APIs.

- [ ] **Step 4: Publish and read back immutably**

The adapter uses create-only/version-conditional writes, sets retention before success, reads the object
back through a separate read credential, recomputes checksum/size and records the provider's object-lock
receipt. Existing key/version or insufficient retention is failure. Retry is allowed only before a
provider acknowledges object creation; ambiguous acknowledgement requires reconciliation, not blind
write.

- [ ] **Step 5: Implement access verification**

`Test-PcvAssuranceArtifactAccess.ps1` rechecks object/version, lock, expiry, decryption authorization,
hash and size without trusting the original uploader summary. It emits `accessible|unavailable|expired|
hash_invalid|retention_invalid` and never upgrades an invalid status.

- [ ] **Step 6: Verify, land, then run the separately authorized sandbox range**

Fake providers cover all E01 failures. The candidate executor has no live provider credential. Run actual
Release gates and independent Sol review, commit `feat: publish immutable assurance artifacts`, and land
only that immutable candidate through its fresh verified-candidate decision/consume. From exact post-merge
main, create/approve/consume a fresh artifact-only execution Packet that binds the merged tool digests,
one non-secret fixture, exact sandbox prefix/provider/retention, separate write/read credentials,
conditional create, readback/object-lock/notary oracle, abort/reconciliation and E02 exit. Only this later
range publishes the sandbox object, reads it back, proves object-lock and emits the terminal attestation.
It cannot edit the repository or reuse the earlier provider-setup/candidate authorities.

E02 may stage measured manifests and store/readback receipts for non-secret bootstrap fixtures, but it
does not notarize or canonically import any Plan 1–3 source. E03 notary and E05 decision-history logic do
not exist yet at this point, so a staged object has no canonical-import status and Dashboard remains RED.

## Task NHA-E03: Implement the independent Evidence Notary

**Files:** notary module, new/test wrappers and focused tests.

- [ ] **Step 1: Add identity and signature RED cases**

Reject executor signer, same trust domain as producer, wrong workflow/check identity, stale/revoked key,
untrusted OIDC issuer/subject/audience, missing timestamp/transparency receipt, changed manifest/store
receipt and duplicate attestation ID.

- [ ] **Step 2: Verify before signing**

Notary receives only immutable locators. It downloads bytes with read-only credential, validates all
schemas, recomputes hashes/sizes, checks actor separation, exact target/spec/oracle and retention/freshness,
then signs the canonical subject envelope. It cannot write product, execution state or raw artifacts.

- [ ] **Step 3: Verify signatures independently**

`Test-PcvAssuranceAttestation.ps1` resolves the frozen trust roots, revocation and timestamp/transparency
receipts and rejects any mismatch. The verifier does not call the producer's code path. Two independent
notary/verifier runs must agree on subject digest and semantic result.

- [ ] **Step 4: Commit E03**

Commit `feat: notarize assurance evidence independently` after all signature/identity negatives reject.

## Task NHA-E04: Generate immutable Decision Packet requests

**Files:** exactly
`packaging/windows-desktop-node/tools/PcvAssuranceDecisionPacket.psm1`,
`packaging/windows-desktop-node/tools/New-PcvAssuranceDecisionPacket.ps1` and
`packaging/windows-desktop-node/tests/PcvAssuranceDecisionPacket.Tests.ps1`.

- [ ] **Step 1: Freeze Packet RED cases**

Construct at least one valid, one missing-required and one forbidden-field case for every tuple in E01's
closed table as in-memory JSON inside the exact named test file. Generated JSON/Markdown bytes use only
that test's temporary artifact slots and never create an unlisted tracked fixture path. Reject inaccessible required artifact, stale spec/target, unsupported tuple, hardcoded
recommendation, request-digest self-reference, post-generation mutation and aggregate campaign claiming
execution authority. Execution/planning authorization rejects result/candidate fields and PASS/GREEN;
landing authorization rejects missing actual candidate/gates/proof while keeping future queue/final SHA
outside the request. Prove `residual_risk` cannot waive P0/P1 or a failed gate and
`campaign_summary` cannot be consumed for work.

- [ ] **Step 2: Build `request_payload` only from verified inputs**

The generator first selects the closed schema branch for `packet_type` and `phase`. Every branch binds
the observed start commit/tree, spec/oracle, requirements/cases/traceability, current evidence,
risk/rollback, workflow/oracle/capability digests, server facts and approval categories.
`planning_authorization|execution_authorization` requires exact allowed operations/commands and frozen
preconditions but **forbids** result head/tree/change-set and actual gate claims. `landing_authorization`
requires exact implementation head, implementation tree, approved change-set and actual phase-specific
gate/artifact evidence. `projection_non_executable` contains child locators only. It calculates RFC 8785
SHA-256 over `request_payload` only and returns the immutable envelope. No mutable approval field appears
inside the payload.

- [ ] **Step 3: Generate Markdown deterministically**

Packet Markdown contains scope/outcome/exclusions, risk/blast radius/mitigation/rollback/expiry, case and
gate results, raw-proof links, limitations, server state, blockers/alternatives/recommendation and exact
requested response. Every sentence maps to a typed field. Manual Markdown edit or unsupported sentence
fails projection check.

- [ ] **Step 4: Verify and commit E04**

Regenerate JSON/Markdown twice, compare bytes and mutate implementation head, implementation tree,
change-set, spec, evidence, risk, rollback, workflow, oracle and capability one field at a time to prove
digest change/staleness.
Commit `feat: generate immutable assurance decision packets`.

## Task NHA-E05: Implement append-only decision and consumption records

**Files:** decision-record module, add/consume wrappers and focused tests; reuse Plan 2's import-candidate
schema/New/Test wrappers without modifying them.

- [ ] **Step 1: Add RED event cases**

Cover unknown Packet/digest, invalid grammar, wrong approver, missing authentication, nonce replay,
expired decision, wrong category/scope/target, changed implementation head/implementation tree/
change-set/spec/evidence/risk/
rollback/workflow/oracle/capability, double consume, repository-only forged event, mutable overwrite and
attempted in-place `consumed=true` mutation. Add exhaustive state-table and race cases for every Plan 2
allowed/disallowed transition, stale owner/fencing token/epoch, sibling substitution, consume-without-claim,
claim-without-consume, crash before effect and duplicate winner. Exercise all three
`terminal_cancellation` variants and all four reasons; reject missing/wrong winning transaction/claim
receipt, approval fields on a template, absent approval fields on an authorization, a losing/uncertain claim,
duplicate terminalization and later consume.

- [ ] **Step 2: Append approval events**

Implement parsing for only the three exact commands. Until L04's atomic cutover, validate the A00-frozen interim channel's
immutable provider/channel/principal, provider-signed export or OIDC issuer/subject/audience/key,
revocation, nonce, issued/expiry and Packet digest; an editable transcript, repository comment, display
name, unsigned copy or caller identity field rejects. Create the approval event through E02's
create-only WORM adapter, read it back through a separate credential, obtain a store receipt and have E03
notarize the event/receipt before returning a decision ID. Do not edit Packet or an earlier event.
Repository decision JSON/Markdown is a deterministic projection and has no authority without the exact
WORM object/version, receipt and notary attestation. Before merge, exercise this path only against
deterministic fake adapters and signed exported interim-event fixtures; candidate code receives no WORM,
notary, KMS or provider credential and creates no canonical event.

- [ ] **Step 3: Implement the sole atomic consume-and-claim and terminal path**

Validate one-time use and exact landing/mutation target. Reuse Plan 2's reservation/horizon/pair-state
schemas and `Invoke-PcvAssuranceConsumeAndClaim.ps1`; E05 implements their canonical provider-neutral
Decision-Plane store adapter and no later plan may create a second decision/state engine. In one serializable
transaction append the consume event, CAS the exact owner/fencing/epoch pair key and independently read back
the winning claim before returning a receipt. If one durable store cannot couple both writes, persist the
single fenced claim intent and complete/read back consume before any external effect; partial or uncertain
completion fences every branch and is reconciliation-only. Fake adapters prove exactly one winner for every
forward/release, attempt/release, landing/rollback and guard-close/add-child race.

`Invoke-PcvAssuranceTerminalCancellation.ps1` is callable only inside or after that winning transaction and
requires its consume/claim receipt. It conditionally appends exactly one event for the most-materialized
still-open losing subject, binding variant, closed reason, immutable owner/epoch/pair/bundle and terminal
receipt; it never edits an approval. Losing, stale, partial, uncertain or duplicate callers append nothing.
Plan 5 L04 later replaces only the authenticated input channel, Plan 5 L06 adds only GitHub CAS/readback
operations, and Plan 6 P07 adds only host root/surface operations behind this adapter. Bootstrap sources are
imported only by the next step's complete union/bijection procedure.

- [ ] **Step 4: Verify and commit the E05 candidate**

All one-field mutation/replay/stale/repository-forgery cases reject and original request/approval bytes
remain unchanged. Run actual Release and independent Sol review, then commit
`feat: record immutable assurance decisions`. No canonical import, provider write or live decision canary
is attempted from candidate code.

- [ ] **Step 5: Land E05 and finalize the early-E inventory**

Consume E05's exact bootstrap landing decision, merge the unchanged candidate, verify the merged tree
from a fresh checkout, and emit only its mandatory raw landing-equivalence attestation under the prebound
landing finalizer. The terminal E05 bootstrap exit is generated later as described below.

Next create/approve/consume a fresh canonical `trust_root/execution_authorization` Packet with
`execution_scope=provider_administration` and `operations=[]` for one nonsecret decision-plane sandbox
canary. It binds the exact merged tool digest, an explicitly non-executable sandbox namespace/prefix and
IDs, one-run credential mint/revoke, WORM/readback/notary operations, provider/retention, expiry and
abort/reconciliation. Its `finalizer_policy` prebinds the protected tool/argv digests and raw authority,
run/readback/notary/revocation receipts only; it does not own the later terminal E05 exit. Prove schemas
and the canonical resolver reject every sandbox event as a real decision. Race two exact branches through
the merged E05 adapter, prove one consume/claim winner, prove only that winner can append the expected
terminal event, and independently read back the pair state, event, store receipt and notary attestation.
Revoke credentials and preserve its canonical authority, run and readback/notary receipts as prerequisite
evidence, never import candidates.

Then create/approve/consume a second fresh canonical artifact-only
`trust_root/execution_authorization` Packet for early-E inventory publication. It binds Plan 2's exact
`bootstrap-import-candidate-manifest` schema/New/Test tool digests and argv, fresh project target,
expected-inventory descriptor, import and terminal-E05-exit output prefixes, provider/retention,
conditional create/readback/notary,
expiry and abort/reconciliation, with no tracked/provider-admin/host operation. Use those tools to reread,
validate and deterministically produce a one-to-one manifest for: the storage/notary selection request/approval/
consume; its distinct provider-setup request/approval/consume/readback/revocation/selected-rollback
events; the interim-channel continuation selection request/approval/consume; its separate no-mutation
readback or provider-setup request/approval/consume/readback/revocation/selected-rollback events; E01–E04
  execution/landing events and exits; and E05 execution/landing events. Bootstrap terminal-cancellation and
  pair-state history through this boundary has expected count zero; the later canonical sandbox history is
  prerequisite-only and excluded. For each E01–E05 work item, include exactly one signed bootstrap root dispatch and its
  receipt, every selected/deferred/resolved range and its own receipt, each execution descriptor and its own
  receipt, every derived child's signed resolution record/receipt and, only where sealed pair/setup
  attachments were promoted, its attachment-promotion record/receipt. A00/A01 are outside the early-E
  manifest and remain solely in T08's inventory; E06 validates their root/parent/child/resolution cardinality
  from that source without duplicate import. Require one-to-one Packet/range/
  descriptor/resolution/promotion/consume bindings and no unresolved or reused range. Independently validate and publish
that exact manifest create-only, then read back/notarize it. As the same Packet's prebound mandatory
finalizer, publish the terminal signed E05 bootstrap exit binding the landing-equivalence attestation,
sandbox-canary canonical authority/run, inventory-publication canonical authority, exact manifest and
readback/notary receipt. E05's terminal exit is not a manifest member. Both
post-merge Packet/approval/consume chains are already canonical and must not appear in the bootstrap
manifest or be canonically imported again.

## Task NHA-E06: Build the fail-closed Trust Dashboard

**Files:** projection module, Dashboard wrapper/tests, generated
`docs/superpowers/plans/luna-completion/trust-dashboard.json` and `TRUST_DASHBOARD.md`.

- [ ] **Step 0: Authorize and canonically import the bounded bootstrap history**

From fresh exact E05 post-merge `main`, use the now-live E05 Decision Authority to create/approve/
consume a fresh `packet_type=trust_root`, `phase=execution_authorization` decision authorizing only the
exact create-only WORM/store-readback/E03-notary/import operations and expected set below. It permits no
tracked edit and is distinct from E06's later Dashboard-code execution/landing decisions.
The user input is authenticated by the still-current A00 interim channel, while E05 supplies canonical
Packet validation, WORM approval/consume events and one-time semantics; neither role may impersonate the
other.

Construct the expected set as the exact union of: T08's complete Plan 1 plus T01–T07 decision/exit
inventory—including A05's artifact-publication request/approval/consume, A05's T01 execution request/
approval, T01 execution consume and T01's separate landing request/approval/consume, plus T06's exact
branch-conditioned issuer history. `existing_identity` includes one planning chain and its preexisting
signed readback, with zero setup/post-create event. `create_identity_intent` includes exactly the intent
planning chain, signing-provider setup authority/consume, forward/readback/credential-revocation and any
selected rollback, followed by one post-create-freeze planning chain. Include the
T08-execution import-candidate manifest plus publication receipt; the terminal signed T08 exit; and X09's
complete branch-conditioned dependency-selection histories. For each OCI/Windows/Verification-Authority/
model-boundary/result-transport dependency, `existing_identity` includes one planning chain/readback and
zero setup/post-create event, while `create_identity_intent` includes exactly intent, provider-setup
authority/consume/forward/readback/revocation/selected-rollback and one post-create-freeze planning chain.
Include X00–X08 decision/exit and X09-execution import-
candidate manifest plus publication receipt, including X06's separate post-merge remote-canary authority/consume/delivery/run/
credential-revoke events; the terminal signed X09 exit; E05's complete external storage/notary and interim-
decision history consisting of two non-combinable selection transactions—storage/notary request/
approval/consume and interim-channel-continuation request/approval/consume—plus each transaction's own
no-mutation readback or applicable provider-setup request/approval/consume/readback/revocation/selected-
rollback inventory; E01–E04 decision/exit and E05-decision import-candidate manifest/publication receipt;
and the terminal signed E05 exit. The already-canonical E05 post-merge sandbox-canary and inventory-
publication Packet/approval/consume plus their run/readback/notary receipts, and this E06 import Packet,
are prerequisite authority/evidence references only; they are not members to import again.
  The union also requires exactly one bootstrap root dispatch and receipt for each A00–E05, T01–T08 and
  X00–X09 work item represented by its owning inventory; every selected/deferred/resolved/executable range
  has its own receipt, every execution descriptor has its own receipt, every derived child has a resolution
  record/receipt, and every pair/setup attachment promotion has its additional record/receipt. A00/A01
  cardinality is the separate root, deferred-parent, child and resolution chain defined by Plan 1. Import
  only terminal-cancellation/pair-state records actually present in those bootstrap import-candidate
  histories; the expected count through the E05 bootstrap exit is exactly zero. The canonical E05 sandbox
  terminal/pair history remains the prerequisite-only reference in the preceding sentence and is not imported.
  Their source IDs and Packet/range/descriptor/resolution/promotion/consume bindings must be bijective. At
this initial step E06 imports only A00–E05 history; E06's own bootstrap dispatch/ranges are intentionally
reserved for the terminal self-history cutover step below, not silently omitted or imported early.
Require one-to-one source ID/hash coverage with no gap, duplicate or extra. Import candidates are hints
only. The E06 expected-set descriptor records every selected identity branch; a missing/reordered create
stage, an extra stage in an existing branch, repeat or backfill is a cardinality failure. For every member,
reread original immutable bytes, validate source schema/signatures/actors/target/
history, rehash raw command/artifact bytes and independently reproduce only their validation result. Do
not rerun any referenced bootstrap command, provider operation, host operation or side effect. Publish
the already-validated original bytes through E02's create-only store/readback receipt, have E03 notarize
them, and append exactly one canonical import event. Import never consumes again or upgrades
bootstrap grade. Publish one canonical import receipt; do not begin E06 tracked edits unless it verifies
and the Dashboard remains honestly RED.

- [ ] **Step 1: Freeze full truth-table fixtures**

Cover every valid/invalid combination of `assurance_verdict`, `landing_eligibility`,
`overall_readiness`, `required_enforced`, residual P0..P3, missing/stale proof and decisions. Unlisted
combinations must be invalid/RED. Include no-supersession, complete one-to-one current-evidence
supersession, partial/cyclic/duplicate/inaccessible replacement and historical-fact-rewrite cases.

- [ ] **Step 2: Derive all three axes**

- Green assurance requires seven gates PASS, no residual finding, fresh accessible proof, actor
  separation and rollback ready.
- Amber assurance permits only P2/P3 after all gates PASS.
- Red includes any failed/not-run/planned gate, P0/P1, drift, stale/inaccessible proof, actor collision or
  missing rollback.
- Landing values follow the source-design table and use provider-attested `required_enforced`, never a
  caller boolean.
- Overall readiness uses only the enumerated fail-closed combinations.
- Current-evidence completion considers only the one schema-selected `completion_required=true`,
  `evidence_status=current` role set. A historical inaccessible entry remains visibly RED historical
  evidence but does not poison the current formula only when Plan 2's signed/notarized one-to-one
  supersession contract validates against fresh accessible replacements. Missing, partial, cyclic,
  mutable or caller-declared supersession keeps overall readiness RED.

- [ ] **Step 3: Project machine JSON to Markdown**

Show gate/case counts, raw artifact access/freshness, actor separation, open risks, decisions,
server/queue status, blockers and exact next action. Initial repository Dashboard is RED because Plan 5
server enforcement and Plan 6 pilots/rehearsal are absent; never color it green for code-level success.

- [ ] **Step 4: Verify and commit E06**

Byte-identical regeneration, manual edit detection and all truth-table fixtures must pass. Commit
`feat: project assurance trust dashboard`, obtain the distinct verified-candidate landing decision, land
unchanged and verify the fresh-main tree. This landing does not enable the control dispatch namespace.

- [ ] **Step 5: Import E06 self-history and atomically cut over task dispatch**

From fresh E06 post-merge main, resolve the predeclared final `artifact_only` range and create/approve/
consume its fresh trust-root decision. Reread E06's immutable one bootstrap task manifest—never amend or
replace it—and require exactly one root-dispatch receipt; one receipt for each ordered range—including
initial import, projection candidate, resolved landing and this terminal range; one receipt for every
execution descriptor; and each applicable derived resolution and pair/setup attachment-promotion receipt.
Require exact Packet/decision/consume bindings plus all E06 terminal-cancellation and pair-state events in
one-to-one history. Canonically import those dispatch objects exactly once; E06's already-canonical decision
events remain prerequisite references and are not imported again. Then conditionally publish/read back/
notarize one `NHA-E06-task-dispatch-store-cutover-v1` attestation and terminal E06 exit binding the complete
prior-history import receipt, E06 self-import receipt, exact bootstrap/control namespaces, cutoff main tree
and no overlap. Only that immutable exit enables E07. A missing self-range, terminal self-exit inclusion,
mixed namespace, duplicate import or uncertain publication leaves the bootstrap namespace closed and the
control namespace disabled; there is no fallback writer.

## Task NHA-E07: Add secret quarantine and cryptographic tombstones

**Files:** tombstone wrapper/module additions and tests.

- [ ] **Step 1: Add RED secret cases**

Test token/password/private-key patterns, high-entropy candidates, false-positive review boundary,
immutable ciphertext already stored, missing key-destruction receipt and attempted silent deletion.

- [ ] **Step 2: Implement incident handling**

Implement the path that immediately marks an artifact quarantined, revokes read grants, destroys/disables the per-run DEK through the
approved key service, retain immutable ciphertext, and append a signed tombstone containing artifact
digest, detection class, quarantine/revocation/destruction receipts, incident ID and safe redacted
metadata. Never copy the secret into logs, Packet or tombstone.

- [ ] **Step 3: Verify recovery behavior and commit the candidate**

With fake/static adapters only, prove quarantined proof becomes inaccessible and cannot support PASS;
affected Packet/decision becomes stale; Dashboard RED and landing blocked. Preserve non-secret
attestations/tombstone. Candidate code receives no KMS/store grant. Commit
`feat: quarantine secret-bearing assurance artifacts`, run Release/Sol review and land unchanged.

- [ ] **Step 4: Run a post-merge nonsecret provider drill under separate authority**

From fresh E07 post-merge main, create/approve/consume a fresh provider-administration
`trust_root/execution_authorization` Packet with `execution_scope=provider_administration`, binding the
exact merged tool digest, sandbox nonsecret marker
object/key/grant/prefix, provider/KMS identities, one-run credential, revoke/destroy/readback/tombstone
operations, provider/retention, expiry/cost ceiling and abort/reconciliation. Only after consume create the
marker and grant, then revoke the grant, destroy the sandbox DEK, read back the immutable ciphertext and
publish/read back/notarize a redacted tombstone. Revoke the credential. Any actual secret input is
forbidden. Seal the authority/consume and all forward/revoke/destroy/readback/tombstone receipts into the
E07 exit; candidate execution authority is never reused.
The Packet's `finalizer_policy` prebinds that exit and every raw provider/KMS receipt publication.

## Task NHA-E08: Wire the independent evidence workflow

**Files:** `.github/workflows/assurance-evidence.yml` and static tests.

- [ ] **Step 1: Add RED workflow permissions tests**

Require exact action/tool digests, trusted invocation, no PR-controlled provider endpoint/role, separate
publisher/notary credentials, OIDC minimum permission only in notary job, no source write, failure
artifact transport and final attestation enforcement.

- [ ] **Step 2: Implement separated jobs**

`collect` validates manifests; `publish` encrypts/stores with create-only credential; `notarize` uses a
separate identity; `project` creates candidate Packet/Dashboard artifacts; `enforce` revalidates outputs.
GitHub artifact upload is staging transport and labeled non-authoritative.

The workflow implements both closed provider-neutral exact-target entries before Plan 5: native
`merge_group` with `types: [checks_requested]` derives base/candidate only from the authenticated server
event, while equivalent mode accepts only Plan 3's signed
`issuer_class=verification_authority`, `authority_source=serialized_landing_candidate` envelope. It
checks out the exact candidate, binds every manifest/notary receipt to that commit/tree and publishes
`artifact-attestation` only from the protected workflow identity. PR head reuse, raw caller SHA,
cross-mode fields, missing lease/fencing facts or a same-name foreign check rejects. Static tests freeze
the trigger, source identity, target mapping and final-enforcement DAG so Plan 5 need not rewrite or
repin this notary workflow merely to open native queue mode.

- [ ] **Step 3: Verify static/fake canaries and commit E08**

Wrong role, 403, hash drift, expired proof, notary identity mismatch and secret fixture all fail while
fake adapters still produce safe diagnostics/tombstones. Candidate workflows receive no publisher,
notary or provider credential and cannot remotely dispatch. Commit
`ci: notarize assurance evidence independently`, run Release/Sol review and land unchanged.

- [ ] **Step 4: Run the merged workflow canary under fresh artifact authority**

From fresh E08 post-merge main, create/approve/consume a fresh provider-administration
`trust_root/execution_authorization` Packet with `execution_scope=provider_administration`, binding exact
merged workflow/blob/tool digests, trusted
dispatch event, synthetic authorization fixture, publisher/notary identities, one-run authorization mint/revoke, output
prefixes/provider/retention/readback/notary, cost/expiry, failure/abort/reconciliation and zero tracked or
host mutation. After consume, dispatch exactly one remote canary, validate delivery/run/artifacts and
failure finalizer, revoke credentials and seal the authority/consume/delivery/run/readback/notary/revoke
events into the E08 exit. A candidate-branch workflow or untrusted dispatch can never receive credentials.
Its `finalizer_policy` prebinds the raw delivery/run/readback/revoke outputs and E08 exit.

## Task NHA-E09: Publish the Plan 4 exit attestation

**Files:** no tracked file. Exact external artifacts are the signed existing X08/X09 target/read inputs,
one-run verifier delivery/run/readback/revocation records, Packet mutation/parser results, two Dashboard
projections and `NHA-E09-evidence-decision-dashboard-v1` under Packet-bound create-only prefixes. This
task creates no commit or PR.

- [ ] From fresh exact E08-complete main, create/approve/consume a fresh
      `trust_root/execution_authorization` Packet with `execution_scope=provider_administration` and
      `operations=[]` before the first side effect. Bind the exact run/read/projection tools and inputs,
      independent verifier dispatch, one-run credential mint/revoke, actors, E07/E08 post-merge drill
      locators, output prefix/provider/retention, create-only/readback/notary, expiry and abort/
      reconciliation; permit no tracked/product/host change or persistent provider setup/policy mutation
      beyond the exact ephemeral control-run transaction. Its `finalizer_policy` prebinds protected tool/
      argv digests and every raw delivery/run/readback/notary/revocation/Plan-4-exit output. This is
      distinct from all E01–E08 execution/landing/drill decisions.
- [ ] Read only the existing immutable X08/X09 bounded-executor execution/result/scope manifests and
      signed fixture target, revalidate their hashes and publish/read back one complete control-run
      projection; do not invoke the bounded executor, write broker or create a new fixture Git result.
      Dispatch only the separately authorized independent-verifier read of that exact existing target.
- [ ] Verify raw logs/manifests/receipts/attestations through a second clean actor.
- [ ] Generate pre-execution and post-verification Packets and exercise implementation head,
      implementation tree, change-set, spec, evidence, risk, rollback, workflow, oracle and capability
      one-field mutations; every mutation invalidates its decision everywhere it is verified or
      consumed.
- [ ] Run approve, deny and request-changes parser tests plus nonce replay/double consume.
- [ ] Generate Dashboard twice; expect `overall_readiness=red` because server/pilots/rehearsal are pending.
- [ ] Publish `NHA-E09-evidence-decision-dashboard-v1` with storage/notary policy and retention expiry,
      binding its consumed provider-administration authority plus E07 sandbox-KMS drill and E08 merged-workflow
      canary authority/run/revocation events.

## Plan 4 exit gate

- [ ] Selected store proves immutable lock, readback, hash, size, retention and independent access.
- [ ] Notary identity is separate and every attestation independently verifies.
- [ ] Packet request/Markdown are deterministic and approval-sensitive mutation makes decisions stale.
- [ ] Pre-execution Packets authorize only exact frozen future work and never claim PASS/GREEN;
      post-execution Packets require actual seven-gate/artifact/lineage proof.
- [ ] Approval and consumption are distinct WORM/readback/notarized events; repository files are only
      projections and replay/double use reject.
- [ ] Dashboard truth table is complete, deterministic and honestly RED.
- [ ] Secret incident cryptoshred/quarantine/tombstone drill passes without secret leakage.
- [ ] Product, package, service, TLS and Hyper-V mutation count is zero.
