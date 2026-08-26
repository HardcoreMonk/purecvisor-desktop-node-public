# Required CI Pester-free cutover evidence 2026-08-25

evidence_id: `pester-free-required-ci-cutover-2026-08-25`
contract: `pcv-required-ci-pester-free-cutover-v1`
result: `PASS_WITH_DISCLOSED_DEVIATION`
phase: `post-merge-required-ci-cutover-closure`
plan_deviation: `pre-change-etag-and-provider-before-response-hash-not-retained`
pre_cutover_main_sha: `c76a831be168a6b5aa122a91df3588a0c5e67f0d`
shadow_sha: `f8208f076cb9db69022b4dc060e65f13d23fae8c`
shadow_run_id: `32898937784`
shadow_run_url: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32898937784`
shadow_run_attempt: `1`
shadow_ci_parity_pass: `true`
cutover_sha: `68756f1f2f609951aaf54d76963b10f96409011b`
cutover_run_id: `32900785756`
cutover_run_url: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32900785756`
cutover_pr: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/pull/1`
pr1_cutover_merge_sha: `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`
pr1_cutover_post_merge_main_sha: `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`
pr1_cutover_post_merge_run_id: `32901477892`
pr1_cutover_post_merge_run_url: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32901477892`
pr1_historical_metadata_compatibility_alias_semantics: `historical-pr1-cutover-closure-not-final-main-authority`
merge_sha: `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`
post_merge_main_sha: `d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`
post_merge_run_id: `32901477892`
post_merge_run_url: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32901477892`
final_closure_pr: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/pull/2`
final_closure_pr_head_sha: `110c8d998c1f830132eb08ed241afc40b4541879`
final_main_sha: `6e2bdb93ce308b632c929e2c17f5550ac3845401`
final_main_run_id: `32904006595`
final_main_run_url: `https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595`
required_contexts: `dotnet,web,delivery,installer-policy`
public_boundary_residue_run_id: `32904006619`
public_boundary_residue_job_id: `97983888524`
public_boundary_provider_required: `false`
cutover_ci_status: `pass`
branch_protection_switch: `pass`
ci_parity_pass: `true`
required_ci_pester_zero: `true`
required_ci_nonadmin_powershell_zero: `true`
cutover_completed: `true`
host_mutation_performed: `false`
msi_or_service_mutation: `false`
actual_vm_tested: `false`
package_candidate_created: `false`
public_trusted_signing: `false`
external_stable_publication: `false`

## Scope and claim boundary

The shadow commit preserved the four protected legacy job identities and ran the legacy and
replacement verification paths at the same Git SHA. Run `32898937784`, attempt `1`, passed all
four jobs and produced eight artifacts. Each artifact ZIP was downloaded again through the provider
API and its SHA-256 was equal to the provider digest.

Direct-child cutover commit `68756f1f2f609951aaf54d76963b10f96409011b` has exact parent
`f8208f076cb9db69022b4dc060e65f13d23fae8c`. It moved the ledger to `62/62` files
and `627/627` contracts at `cutover / local pass / CI pass` and activated the replacement
four-job workflow. Exact-SHA CI passed, main protection was switched in one request, PR #1 was
merged without force, and the exact merge SHA passed remote-main CI. Required CI cutover is
therefore operationally complete. The protection transition used the permitted immediate
compare-before/PATCH/readback route, but its pre-change ETag and original provider-before response
hash were not retained; that evidence-capture deviation is disclosed below. This is not a
repository-wide PowerShell deletion claim or literal full-compliance claim for every plan step.

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

## Exact cutover-SHA CI

Development Gates run `32900785756`, attempt `1`, was a pull-request run bound to exact SHA
`68756f1f2f609951aaf54d76963b10f96409011b`. It completed successfully in `120000 ms`; the
provider job envelope was `116000 ms`, below the `214000 ms` ceiling.

| Job | Job ID / URL | Started UTC | Completed UTC | Result |
| --- | --- | --- | --- | --- |
| `dotnet` | [`97973909141`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32900785756/job/97973909141) | `2026-08-25T21:23:59Z` | `2026-08-25T21:25:54Z` | `success` |
| `web` | [`97973909441`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32900785756/job/97973909441) | `2026-08-25T21:23:58Z` | `2026-08-25T21:24:41Z` | `success` |
| `delivery` | [`97973909359`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32900785756/job/97973909359) | `2026-08-25T21:23:58Z` | `2026-08-25T21:24:43Z` | `success` |
| `installer-policy` | [`97973909537`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32900785756/job/97973909537) | `2026-08-25T21:24:02Z` | `2026-08-25T21:25:03Z` | `success` |

| Artifact | Artifact ID / provider URL | Provider/API ZIP SHA-256 | Extracted summary SHA-256 |
| --- | --- | --- | --- |
| `development-gates-dotnet-32900785756` | [`9583021635`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9583021635) | `9ae3fa525e7c918df2c08225c4927163135c6e3e52b0a757a3c61c18eca122d9` | `6d32bcf46f5ef9dad16a3d7dc08a051d31eaf24abc32e88cad7f003dc9766412` |
| `development-gates-web-32900785756` | [`9582983097`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9582983097) | `eba635a5f64ef0517917fb8d896c1318b35e128d770449a0e11e0066197b91ab` | `e8029b12046dc9909f817a576e305e7150afedf7b89879209d093447c189c2c5` |
| `development-gates-delivery-32900785756` | [`9582983092`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9582983092) | `bca4cdc6f7d1d50d22e53964a4f1569c2f1121eda619edecb1948746be0f40af` | `05710270322d4c90dac702e9b70a54bf3554502f8ea80f0fd3c74ee88bae4007` |
| `development-gates-installer-policy-32900785756` | [`9582994239`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9582994239) | `9330e8a971d1a47540d0a8b592c98c83dafbdeb1e4eed5220d308e419093ec8a` | `375ea58d71e97a874ee52d9ff88564706da7ef673c9a97a51603abbd19aed4de` |

Every provider digest matched an independent API re-download. The v2 summaries were all
`catalog_activation_state=active` and `ok=true`: .NET `2210/2210` in `51530 ms`, Web registry
`50/50` with manifest `62 files / 627 contracts` in `20108 ms`, Delivery `528/528` in `4725 ms`,
and Installer `49/49` in `3570 ms`. All seven suites reported `cutover/passed`; timeout, cancel,
missing, duplicate, skipped, and not-run counts were `0`. Static workflow validation reported
Pester, non-admin PowerShell, and host/service/MSI/VM mutation invocation `0`.

## Branch-protection transition

The immediate pre-mutation readback required `strict=true`, admin enforcement enabled,
force-push/deletion disabled, and exact old checks `dotnet-tests`, `web-tests`,
`packaging-pester`, and `installer-web-pester`, each bound to GitHub Actions app ID `15368`.
One PATCH replaced only that check set with `dotnet`, `web`, `delivery`, and
`installer-policy`, also bound to app ID `15368`.

The newline-terminated canonical compact SHA-256 of the exact old required-status rollback payload is
`7b2ae4962bea6779aaf4408e2cc7b0b8ddfa6f4a45a13cd4850d486e79197292`; the new payload is
`a13b0626b38e46fec320608b07a5f9fec88d22219d8e0bfef06d91336399fd0d`. Immediate and final
readbacks found exact new checks, `strict=true`, admin enforcement enabled, signatures and linear
history disabled, `allow_force_pushes=false`, `allow_deletions=false`, `block_creations=false`,
`lock_branch=false`, `required_conversation_resolution=false`, and `allow_fork_syncing=false`.
The final newline-terminated full canonical response SHA-256 is
`2b4315655acb12ddb67af778be6845ca4073f7e021acddaa487a988796bfb82b`; its provider ETag is
`W/"0499d9c3474f350a0da954236fca03ee3e71604a13de1ec929c6cc015c4933bc"`.

The pre-change ETag was not retained in the closure material, so no before-ETag value is invented
here. The permitted compare-before/PATCH/readback path was used instead: the immediate
before readback established the old contexts and safety fields above, and the one-request body was:

```json
{"strict":true,"checks":[{"context":"dotnet","app_id":15368},{"context":"web","app_id":15368},{"context":"delivery","app_id":15368},{"context":"installer-policy","app_id":15368}]}
```

The rollback body is fully reconstructable and has the old-payload digest stated above:

```json
{"strict":true,"checks":[{"context":"dotnet-tests","app_id":15368},{"context":"web-tests","app_id":15368},{"context":"packaging-pester","app_id":15368},{"context":"installer-web-pester","app_id":15368}]}
```

Replacing only the current response's contexts/checks with that captured old set while preserving
all unchanged safety fields yields reconstructed pre-state full canonical SHA-256
`290a93d2ce606c7bdfe12fddd53a57cb49b36e9fefadd0fcd08fd7a3c162f190`. This is explicitly a
post-readback reconstruction, not a fabricated provider-before response hash.

## Merge and remote-main CI

PR #1 was marked ready only after exact-SHA checks passed and was merged at
`2026-08-25T21:31:39Z` with normal, non-force merge commit
`d4a952b8e5ab11f7e3a9ae92b41c61b12828bfab`. Its parents are exactly frozen pre-cutover main
`c76a831be168a6b5aa122a91df3588a0c5e67f0d` and reviewed cutover
`68756f1f2f609951aaf54d76963b10f96409011b`; GitHub reports a verified signature.

Remote-main Development Gates run `32901477892`, attempt `1`, event `push`, is bound to that exact
merge SHA and completed successfully in `204000 ms`; the job envelope was `200000 ms`, below the
`214000 ms` ceiling.

| Job | Job ID / URL | Started UTC | Completed UTC | Result |
| --- | --- | --- | --- | --- |
| `dotnet` | [`97976095792`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32901477892/job/97976095792) | `2026-08-25T21:31:44Z` | `2026-08-25T21:33:54Z` | `success` |
| `web` | [`97976095798`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32901477892/job/97976095798) | `2026-08-25T21:31:44Z` | `2026-08-25T21:32:28Z` | `success` |
| `delivery` | [`97976095779`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32901477892/job/97976095779) | `2026-08-25T21:31:44Z` | `2026-08-25T21:35:04Z` | `success` |
| `installer-policy` | [`97976095661`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32901477892/job/97976095661) | `2026-08-25T21:31:45Z` | `2026-08-25T21:32:37Z` | `success` |

| Artifact | Artifact ID / provider URL | Provider/API ZIP SHA-256 | Extracted summary SHA-256 |
| --- | --- | --- | --- |
| `development-gates-dotnet-32901477892` | [`9583287720`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9583287720) | `f8f0b47b64829a154e5ab4afb5324ce85292993112c89192599c8e93b4952b78` | `3280f89467445a6ff7baa30f62caa6ef97ad20da7ec7afa8a2c3d61b1d122c2f` |
| `development-gates-web-32901477892` | [`9583242169`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9583242169) | `829fea5683ea4daa4e4ef40b9d4a9dfe28ca6d567cb2d0f4591948b6b4b50d63` | `fc4482a0f7639f1b565ec02b65e5303b7112c7c9be94f14808cfdf34c5e8f1c0` |
| `development-gates-delivery-32901477892` | [`9583328387`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9583328387) | `cbd223202ecc852d52ee2ba841d81c059b997af8b947d85928bb550e2169f155` | `2f2d9797f2f3bd2c23072142ead3652739e78a5f53cba63c6a7ba54313dbc913` |
| `development-gates-installer-policy-32901477892` | [`9583245735`](https://api.github.com/repos/HardcoreMonk/purecvisor-desktop-node-public/actions/artifacts/9583245735) | `7f7eaab6c1ee0e565f87719de795ae388c7fb3504c6919c633d966a0801e12f2` | `4bd3475090dcc16b35d5ed887a89c6e30a225ed94ac1c4d79029ba082518a6dd` |

The remote-main v2 summaries again reported .NET `2210/2210` in `54566 ms`, Web registry
`50/50` and manifest `62/627` in `19976 ms`, Delivery `528/528` in `5292 ms`, and Installer
`49/49` in `4048 ms`. All seven suites were `cutover/passed`; timeout, cancel, missing, duplicate,
skipped, and not-run counts were `0`. The four ZIP hashes above were independently recomputed from
provider API downloads. Public Boundary run `32901477914` also passed, but it is deliberately not
one of the four required contexts and still uses Pester/PowerShell.

## Final documentation-closure main authority

The preceding PR #1 merge and run `32901477892` are immutable cutover predecessor evidence. PR #2
head `110c8d998c1f830132eb08ed241afc40b4541879` merged normally at
`2026-08-25T22:00:31Z`, producing final `main` SHA
`6e2bdb93ce308b632c929e2c17f5550ac3845401`. Development Gates run
[`32904006595`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595),
attempt `1`, event `push`, is bound to that exact SHA and completed successfully. Its workflow
wall-clock was `120000 ms`; the provider job envelope was `117000 ms`.

| Required job | Job ID / URL | Started UTC | Completed UTC | Result |
| --- | --- | --- | --- | --- |
| `dotnet` | [`97983889723`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595/job/97983889723) | `2026-08-25T22:00:37Z` | `2026-08-25T22:02:34Z` | `success` |
| `web` | [`97983889620`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595/job/97983889620) | `2026-08-25T22:00:38Z` | `2026-08-25T22:01:28Z` | `success` |
| `delivery` | [`97983889739`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595/job/97983889739) | `2026-08-25T22:00:37Z` | `2026-08-25T22:01:26Z` | `success` |
| `installer-policy` | [`97983889504`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595/job/97983889504) | `2026-08-25T22:00:37Z` | `2026-08-25T22:01:35Z` | `success` |

| Artifact | Artifact ID | Provider/API ZIP SHA-256 | Extracted summary SHA-256 |
| --- | ---: | --- | --- |
| `development-gates-dotnet-32904006595` | `9584155808` | `0287f854903e39b5971567d805355b9f0d03b11ed88928bf9442b212a417573d` | `2e753a370d776acf1a72ff31e6c30bbe978d7cf3fcb4923145c69145b4dc02d3` |
| `development-gates-web-32904006595` | `9584126782` | `d3a812201483578b4a9e601a0b4bead8c5dc937515f9a7c8ecfa9df844419514` | `6f144a03e81b9e591f704ceface3bc2156c0f23bcb4374cab4145c04eacfcd85` |
| `development-gates-delivery-32904006595` | `9584125168` | `47916ff4ad227d3452e6c02baefd2f4c337c80b912a4963002eb9ff4a59adae8` | `2bc02d6d2f176f500e187a87b0a162ae0ffe15a181954e016d7fc48c3e19932c` |
| `development-gates-installer-policy-32904006595` | `9584129949` | `22d94e5f679aa498df634199a9aae79b20ca399ef167053159218de07d6f194b` | `d3256323a6a65146cac2a93da436d634bbca33ea8f08d914ea3326fbcc3ebb2d` |

All four `pcv-development-verification-summary-v2` summaries report
`catalog_activation_state=active`, `ok=true`, `plan_only=false`, and all suites
`cutover/passed`. Measured results are .NET `2210/2210` in `55913 ms`, Web registry `50/50` in
`19352 ms`, Delivery `528/528` in `5092 ms`, and Installer Policy `49/49` in `4082 ms`;
failure, skip, not-run, timeout, cancel, and host-mutation counts are all `0`.

Main branch protection currently requires exactly `dotnet`, `web`, `delivery`, and
`installer-policy`, each bound to GitHub Actions app ID `15368`, with `strict=true` and admin
enforcement enabled. The separate Public Boundary run
[`32904006619`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006619),
job `97983888524` (`public-boundary-ci-required`), also passed at the same SHA, but it remains a
Pester/PowerShell residue workflow with `provider_required=false`.

## Frozen-reader public boundary

The ignored 0.42.65 executable remains excluded from the public Git tree, archive, Actions inputs,
release, and package. On a clean public runner, the two affected Pester cases validate the checked-in
immutable compatibility and public-exclusion evidence instead of executing that binary. Shadow
summaries must report `frozen_binary_execution_count=0` and
`immutable_evidence_fallback_count=2`; this is not a claim of fresh binary execution. The historical
actual-reader result remains `8/8 PASS` under the pinned SHA-256 recorded in
`docs/ga-ready/evidence/csharp-architecture-wave2a-job-durability-completion-2026-08-02.md`.

## Completed gates, residue, and rollback

- Exact cutover-SHA CI, the one-request protection transition, fixed-diff review, non-force PR
  merge, and exact remote-main CI all passed.
- The repository is PUBLIC, main protection requires exact new checks with strict/admin enforcement,
  cutover PR #1 and documentation-closure PR #2 are merged, and final-main run `32904006595` at
  `6e2bdb93ce308b632c929e2c17f5550ac3845401` is the current Required CI authority.
- The 62 legacy Pester source files remain historical parity/rollback material. Non-required
  `.github/workflows/public-boundary.yml`, local/manual Pester instructions, and administrator
  scripts remain residue. Required CI alone has Pester and non-admin PowerShell invocation `0`.
- Post-merge rollback must use a new branch and normal PR: run
  `git revert 68756f1f2f609951aaf54d76963b10f96409011b`, validate the restored shadow/current jobs, restore
  the exact old four protection contexts from the payload identified above, verify readback, and
  merge without force. Never reset or force-push `main`.

The original private archive was independently re-read after cutover: it remains branch `main` at
`7f3ce04afda0284556d57bb8ba66614f70e70cb0`, its GitHub repository remains PRIVATE, its pre-existing
two modified and six untracked paths remain unstaged, and neither the shadow nor cutover public
object is present in its object database.

The operational version remains `0.42.74-admin-smoke`. The
`pcv.vm.saved-lifecycle/actual_vm_tested/fail` feature-promotion blocker remains open. No package,
installer, service, Hyper-V, guest, or actual-VM mutation was performed, and public trusted signing
and external stable binary publication remain false.
