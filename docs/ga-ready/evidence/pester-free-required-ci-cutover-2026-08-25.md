# Required CI Pester-free cutover evidence 2026-08-25

evidence_id: `pester-free-required-ci-cutover-2026-08-25`
contract: `pcv-required-ci-pester-free-cutover-v1`
result: `SHADOW_DUAL_RUN_PASS_CUTOVER_CI_PENDING`
phase: `single-cutover-commit-pre-provider-switch`
remote_main_sha: `c76a831be168a6b5aa122a91df3588a0c5e67f0d`
shadow_sha: `f8208f076cb9db69022b4dc060e65f13d23fae8c`
shadow_run_id: `32898937784`
shadow_run_url: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32898937784`
shadow_run_attempt: `1`
shadow_ci_parity_pass: `true`
cutover_ci_status: `pending`
branch_protection_switch: `pending`
cutover_completed: `false`
host_mutation_performed: `false`
package_candidate_created: `false`
public_trusted_signing: `false`
external_stable_publication: `false`

## Scope and claim boundary

The shadow commit preserved the four protected legacy job identities and ran the legacy and
replacement verification paths at the same Git SHA. Run `32898937784`, attempt `1`, passed all
four jobs and produced eight artifacts. Each artifact ZIP was downloaded again through the provider
API and its SHA-256 was equal to the provider digest.

This direct-child cutover commit records that immutable predecessor, moves the ledger to `62/62`
files and `627/627` contracts at `cutover / local pass / CI pass`, and activates the replacement
four-job workflow. The new cutover-SHA jobs, provider protection switch, PR merge, and remote-main
run are still pending. Therefore `cutover_completed=false`; this document does not yet make the
final Required CI completion claim.

## Immutable shadow run

| Job | Job ID | Started UTC | Completed UTC | Result |
| --- | ---: | --- | --- | --- |
| `dotnet-tests` | `97967948584` | `2026-08-25T21:04:00Z` | `2026-08-25T21:06:53Z` | `success` |
| `installer-web-pester` | `97967948587` | `2026-08-25T21:04:00Z` | `2026-08-25T21:05:52Z` | `success` |
| `packaging-pester` | `97967948557` | `2026-08-25T21:04:00Z` | `2026-08-25T21:07:02Z` | `success` |
| `web-tests` | `97967948273` | `2026-08-25T21:04:00Z` | `2026-08-25T21:05:27Z` | `success` |

The provider job envelope was `182000 ms` and the full workflow wall-clock was `186000 ms`;
both were below the `214000 ms` ceiling.

## Artifact identities

| Artifact | Artifact ID | Provider/API ZIP SHA-256 |
| --- | ---: | --- |
| `replacement-delivery` | `9582376452` | `2b2188a2888ff442dd8e5c8e52eeafc948ce72f4343bcace6988db3fa70e15c4` |
| `legacy-packaging` | `9582375763` | `25ecbab1da902997ade48a9fc58ef5050114a59454081a809d7d8e90b2749469` |
| `replacement-dotnet` | `9582371090` | `325b1bf4c6c8c0d543a9c8ccdec0934fa8657bd941cf0302e3543f48ee71795b` |
| `legacy-dotnet` | `9582370551` | `b8c601d97628698b77cb6f1c3cc988fb56a092558b3df7475083a6f4d62d5af7` |
| `replacement-installer-policy` | `9582337434` | `8cf146f27800462033ece318c7bdc866f360e6db6309a12325dd671374bc5b67` |
| `legacy-installer-web` | `9582336855` | `065d9b830d9c147df6f2c60c1e14784caaf4ca9314204136fbecc4a340185978` |
| `replacement-web` | `9582323969` | `8bcf4180464441d9342e6ecb87e54e1d1bf4be4b80404308923f45d9c588b39c` |
| `legacy-web` | `9582323494` | `a40f182c705fdbbb70387fb12bf882d975be0bacc2ef1e1bec7eaf0d866d8041` |

## Measured parity

| Path | Passed/total | Failed | Skipped | Not run | Duration |
| --- | ---: | ---: | ---: | ---: | ---: |
| Legacy .NET, 9 assemblies | `2210/2210` | `0` | `0` | `0` | `60736 ms` |
| Legacy Web command pair | `2/2` | `0` | `0` | `0` | `4062 ms` |
| Legacy Packaging Pester | `528/528` | `0` | `0` | `0` | `98547 ms` |
| Legacy Installer/Web Pester | `99/99` | `0` | `0` | `0` | `9533 ms` |
| Replacement .NET, 9 assemblies | `2210/2210` | `0` | `0` | `0` | `44424 ms` |
| Replacement Web contracts | `50/50` | `0` | `0` | `0` | `20006 ms` shard |
| Replacement Delivery contracts | `528/528` | `0` | `0` | `0` | `4678 ms` shard |
| Replacement Installer contracts | `49/49` | `0` | `0` | `0` | `2953 ms` shard |

The four replacement manifest summaries were bound to the exact shadow SHA and each reported
`contracts_total=627`, `mapped=627`, `local_pass=627`, and `ci_pending=627` at the shadow
checkpoint. The independent aggregate validation reported missing/duplicate/order drift `0/0/0`,
`frozen_binary_execution_count=0`, `immutable_evidence_fallback_count=2`, and
`host_mutation_performed=false`.

Legacy result-summary SHA-256 values were:

- .NET: `6e86b021f1422e1a0af4d5e289d7d90d216f120e7a6a4e72ca3af89a26554bf8`
- Web: `3e55b085fad48b77b32d4affd8364496c4e6dc6f5ba65ef6b8a5d3f0b61d21d0`
- Packaging: `81e6dca9ed6027e3f322556bc8bb6726eb5e37cdd2c20ba9f680d98c0d3ea32c`
- Installer/Web: `b1b40aa0ad4dca0b03a2ae440be4014894c8816c6e50aaff17bc3190d855f639`

Replacement executable-output SHA-256 values independently recomputed from the extracted summaries
were `e6bf3d3bf03a7349287496c04c5c061319144a91d9e0e45805dda50e88a4b3c6`
(.NET), `e5acd9e3f1416465fa354141297df74737856ddf1e05867d1228c8e00bea98fe`
(Web typecheck), `1a989313ebee306b2d0aacfbd99320003c82042bf7243a0adb4f76eb10f7bb44`
(Web parity), `29c8b9f85965617de39f23119087de27c8fa952693c4adf1070ac3d2e304c645`
(Delivery), and `6129c78c669ae5039cc4d267732b3ec206a1a141d52ad14975a368cc98e83ee1`
(Installer).

## Cutover commit state

The tracked Required CI workflow now contains exactly `dotnet`, `web`, `delivery`, and
`installer-policy`. Its executable nodes contain Pester invocation `0`, non-admin
PowerShell invocation `0`, and host/service/MSI/VM mutation invocation `0`. Windows executable
steps explicitly use `cmd`; Ubuntu uses its default Bash shell. The catalog is `active`, all
seven suite migration states are `cutover`, and the manifest locator points to the immutable
shadow run above.

Legacy Pester sources remain checked-in as non-required historical parity and rollback sources, not
as the active workflow oracle. Separate administrator/manual scripts also remain available.
`.github/workflows/public-boundary.yml` remains a non-required residue workflow. This is not a
repository-wide PowerShell deletion claim.

## Frozen-reader public boundary

The ignored 0.42.65 executable remains excluded from the public Git tree, archive, Actions inputs,
release, and package. On a clean public runner, the two affected Pester cases validate the checked-in
immutable compatibility and public-exclusion evidence instead of executing that binary. Shadow
summaries must report `frozen_binary_execution_count=0` and
`immutable_evidence_fallback_count=2`; this is not a claim of fresh binary execution. The historical
actual-reader result remains `8/8 PASS` under the pinned SHA-256 recorded in
`docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`.

## Remaining gates

- Run and independently validate the four replacement jobs on the exact cutover SHA.
- Atomically replace only the four required branch-protection contexts and verify readback.
- Complete fixed-diff review, merge the cutover PR without force, and validate remote-main CI.
- Publish the documentation-only post-merge evidence update.

The operational version remains `0.42.74-admin-smoke`. The
`pcv.vm.saved-lifecycle/actual_vm_tested/fail` feature-promotion blocker remains open. No package,
installer, service, Hyper-V, guest, or actual-VM mutation was performed, and public trusted signing
and external stable binary publication remain false.
