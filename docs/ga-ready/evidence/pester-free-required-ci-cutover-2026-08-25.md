# Required CI Pester-free cutover evidence 2026-08-25

evidence_id: `pester-free-required-ci-cutover-2026-08-25`
contract: `pcv-required-ci-pester-free-cutover-v1`
result: `SHADOW_PENDING`
phase: `same-sha-dual-run-pre-commit`
remote_main_sha: `c76a831be168a6b5aa122a91df3588a0c5e67f0d`
shadow_sha: `captured-after-shadow-commit`
shadow_run_id: `pending`
host_mutation_performed: `false`
package_candidate_created: `false`
public_trusted_signing: `false`
external_stable_publication: `false`

## Scope

The shadow commit preserves the four protected legacy job identities and runs the legacy and
replacement verification paths at the same Git SHA. It does not change branch protection, merge the
draft PR, create a package, execute an installer, mutate a service, or invoke Hyper-V operations.

The migration ledger remains `627/627` mapped and local-PASS with CI parity pending until immutable
GitHub Actions artifacts are downloaded and independently validated. The shadow SHA is deliberately
not self-recorded in this commit; the direct-child cutover commit records the observed SHA, run, job,
artifact, count, duration, and digest identities.

## Frozen-reader public boundary

The ignored 0.42.65 executable remains excluded from the public Git tree, archive, Actions inputs,
release, and package. On a clean public runner, the two affected Pester cases validate the checked-in
immutable compatibility and public-exclusion evidence instead of executing that binary. Shadow
summaries must report `frozen_binary_execution_count=0` and
`immutable_evidence_fallback_count=2`; this is not a claim of fresh binary execution. The historical
actual-reader result remains `8/8 PASS` under the pinned SHA-256 recorded in
`docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`.

## Pending immutable observations

- Exact shadow SHA and draft-PR Actions run identity.
- Four legacy and four replacement artifact IDs and provider digests.
- Legacy Pester `627/627`, failed/skipped/not-run `0/0/0`.
- Replacement Web `50`, Installer `49`, Delivery `528`, ledger `627/627`.
- Full .NET Release totals with skipped `0`.
- Same-SHA binding, duration, public-safety, and host-mutation `false` readback.
