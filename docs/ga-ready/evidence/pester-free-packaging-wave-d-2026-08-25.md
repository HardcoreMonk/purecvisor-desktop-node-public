# Packaging verification Wave D local parity evidence

## Current verdict

Packaging Wave D is in progress. D1 through D6 local parity are PASS. The managed replacements
executed all `346/346` mapped Packaging contracts without skips. The one-time Pester 5.7.1
references also passed D1 `90/90`, D2 `42/42`, D3 `61/61`, D4 `58/58`, D5 `51/51`, and D6 `44/44` without skips. D7-D10, aggregate Packaging local
parity, Required CI parity, branch-protection cutover, and managed current-evidence catalog
activation remain pending and are not claimed by this checkpoint.

wave_c_checkpoint_head=9bc382650059fe3ea759e27bcee69cce6823fe38
evidence_input_head=b86eddd37d0ed5af44c8b33f426c0eef8024eb44
d2_input_head=a38607b14ac1d2dd5904dc408d4d0cb757f1f936
d3_input_head=dea5f6c9f11b51abab53bc321bcf5acbebff8d51
d4_input_head=03316a7f8db38dfa325d69a16f8607c167b1ff11
d5_input_head=373a07f360a6d1f78b307dc06692a634e3f1f523
d6_input_head=8e3bac7b4a54d7e62cf95ad708f53ca1f7a4156f
input_dirty_state=clean

## Fixed ten-batch inventory

| Batch | Responsibility | Legacy files | Legacy contracts | Replacement | Legacy reference | Ledger state |
| --- | --- | ---: | ---: | --- | --- | --- |
| D1 | canonical admin-smoke evidence | 1 | 90 | 90/90 pass | 90/90 pass | mapped / local pass / CI pending |
| D2 | current/promotion/package evidence | 5 | 42 | 42/42 pass | 42/42 pass | mapped / local pass / CI pending |
| D3 | product invocation | 1 | 61 | 61/61 pass | 61/61 pass | mapped / local pass / CI pending |
| D4 | product descriptors | 3 | 58 | 58/58 pass | 58/58 pass | mapped / local pass / CI pending |
| D5 | development verification policy | 8 | 51 | 51/51 pass | 51/51 pass | mapped / local pass / CI pending |
| D6 | orchestration and timeout boundaries | 5 | 44 | 44/44 pass | 44/44 pass | mapped / local pass / CI pending |
| D7 | package/public-distribution preflight | 8 | 52 | not-run | not-run | unmapped / local pending / CI pending |
| D8 | manual-admin/public-ops readiness | 8 | 45 | not-run | not-run | unmapped / local pending / CI pending |
| D9 | installed-smoke descriptors | 8 | 31 | not-run | not-run | unmapped / local pending / CI pending |
| D10 | reconciliation/lifecycle policy | 8 | 54 | not-run | not-run | unmapped / local pending / CI pending |
| **Total** |  | **55** | **528** | **346 pass / 182 not-run** | **346 pass / 182 not-run** | **346 mapped local pass / 182 unmapped / CI pending** |

batch_d1_files=1
batch_d1_contracts=90
batch_d2_files=5
batch_d2_contracts=42
batch_d3_files=1
batch_d3_contracts=61
batch_d4_files=3
batch_d4_contracts=58
batch_d5_files=8
batch_d5_contracts=51
batch_d6_files=5
batch_d6_contracts=44
batch_d7_files=8
batch_d7_contracts=52
batch_d8_files=8
batch_d8_contracts=45
batch_d9_files=8
batch_d9_contracts=31
batch_d10_files=8
batch_d10_contracts=54
packaging_files_total=55
packaging_contracts_total=528

## Starting ledger state

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped | pass | pending |
| Installer | 6 | 49 | mapped | pass | pending |
| Packaging | 55 | 528 | unmapped | pending | pending |
| Total | 62 | 627 | partial | partial | pending |

ledger_files_total=62
ledger_contracts_total=627
ledger_web_mapped_local_pass=50
ledger_installer_mapped_local_pass=49
ledger_packaging_unmapped=528
ledger_ci_pending=627
ledger_missing=0
ledger_duplicate=0
ledger_order_drift=0

## D1 canonical admin-smoke evidence

The replacement is a read-only C# verifier over a generated static contract spec. The spec is
bound to the exact legacy source SHA-256, 90 names in ordinal order, 2,935 legacy assertion sites,
and 6,687 evaluated cases. Its 307 repository paths, 2,080 regular-expression patterns, partial
C# source concatenation order, file-existence assertions, current-evidence generated blocks, and
ADR inventory boundary are validated without starting PowerShell or another process.

| Check | Result |
| --- | --- |
| Replacement legacy contracts | PASS, 90/90, failed 0, skipped 0 |
| Deterministic negative tests | PASS, 6/6 |
| Pester 5.7.1 reference | PASS, 90/90, failed 0, skipped 0, not-run 0 |
| Delivery assembly | PASS, 204/204, failed 0, skipped 0 |
| Node strict-v2 ledger | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| .NET strict-v2 ledger | PASS, 12/12 |

d1_legacy_path=packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
d1_replacement_owner=src/DesktopNode.Delivery.Tests/Delivery/Evidence/PcvAdminSmokeEvidenceDocsContractTests.cs
d1_contract_spec=config/pcv-admin-smoke-evidence-docs-contract-spec-v1.json
d1_contract_spec_sha256=ee146ec5398766d9fe1a4c366a6af7adda404fb8a727697f35c10f987762b5f2
d1_legacy_source_sha256=91c580d11875c79a28ff86c7daceba275a231d0cb31483500d25181d325b63c9
d1_legacy_should_site_count=2935
d1_captured_evaluation_count=6687
d1_spec_path_count=307
d1_spec_pattern_count=2080
d1_replacement_summary={"runner":"dotnet-test/xunit","total":90,"executed":90,"passed":90,"failed":0,"skipped":0,"duration_ms":1000.0,"result":"Completed"}
d1_replacement_summary_sha256=342f3672f21196116095dac5c2504c39555a1f319ec28e536ca72ca31e1fe29a
d1_legacy_summary={"pester_version":"5.7.1","total":90,"passed":90,"failed":0,"skipped":0,"not_run":0,"duration_ms":11823.514,"result":"Passed"}
d1_legacy_summary_sha256=09374edbbe6fe68b40b571e4401e674f252f118d2bf2c7bde50c80588b2e3574
d1_negative_stale_current_anchor=pass
d1_negative_wrong_sha_length_case=pass
d1_negative_missing_predecessor_label=pass
d1_negative_false_public_signing_claim=pass
d1_replacement_child_process_count=0
d1_host_mutation_performed=false

## Ledger state after D1

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped 50 | pass 50 | pending 50 |
| Installer | 6 | 49 | mapped 49 | pass 49 | pending 49 |
| Packaging | 55 | 528 | mapped 90 / unmapped 438 | pass 90 / pending 438 | pending 528 |
| Total | 62 | 627 | mapped 189 / unmapped 438 | pass 189 / pending 438 | pending 627 |

ledger_after_d1_packaging_mapped=90
ledger_after_d1_packaging_local_pass=90
ledger_after_d1_packaging_unmapped=438
ledger_after_d1_missing=0
ledger_after_d1_duplicate=0
ledger_after_d1_order_drift=0

## D2 current, promotion, and package evidence

Five legacy files own 42 contracts: the 0.42.73 promotion record, 0.42.74 package/current
record, canonical current-evidence generation, feature-evidence promotion, and the frozen
0.42.65 JobStore reader compatibility boundary. Their C# replacements read repository state
directly, validate strict JSON/Markdown/source contracts, and do not start PowerShell, Pester,
an installer, a service command, or a VM command. The production managed current-evidence
verifier separately validates the canonical record, referenced evidence, eight generated blocks,
rendering, stale targets, cancellation, and the feature-promotion fail-closed gate without any
write API.

| Check | Result |
| --- | --- |
| Replacement legacy contracts | PASS, 42/42, failed 0, skipped 0 |
| Invalid qualification variants | PASS, 12/12 rejected |
| Managed current-evidence focused tests | PASS, 17/17, failed 0, skipped 0 |
| Pester 5.7.1 reference | PASS, 42/42, failed 0, skipped 0, not-run 0 |
| Delivery assembly | PASS, 246/246, failed 0, skipped 0 |
| Verification assembly | PASS, 501/501, failed 0, skipped 0 |
| Release solution build | PASS, warnings 0, errors 0 |
| Node strict-v2 ledger | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| Public source safety | PASS, 20/20, finding count 0 |

d2_legacy_files=5
d2_legacy_contracts=42
d2_replacement_summary={"runner":"dotnet-test/xunit","total":42,"executed":42,"passed":42,"failed":0,"skipped":0,"duration_ms":106.0,"result":"Completed"}
d2_replacement_summary_sha256=b8dbf64ee2995818840722e44b15fc94f4bee6cb22d34a82dab5826e1fdda472
d2_legacy_summary={"pester_version":"5.7.1","total":42,"passed":42,"failed":0,"skipped":0,"not_run":0,"duration_ms":34250.534,"result":"Passed"}
d2_legacy_summary_sha256=325b3220479b43ccc9e0a13b56717efce32300ac530eeb84a27009c30dd2f0af
d2_frozen_reference_sha256=95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83
d2_frozen_reference_product_version=0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876
d2_frozen_reference_source=read-only-archive-temporary-local-copy
d2_frozen_reference_removed_after_run=true
d2_invalid_qualification_case_count=12
d2_release_build_warnings=0
d2_release_build_errors=0
d2_public_source_safety_tests=20
d2_public_source_safety_finding_count=0
d2_public_source_safety_report_sha256=603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd
d2_replacement_child_process_count=0
d2_replacement_write_api_count=0
d2_host_mutation_performed=false

## Ledger state after D2

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped 50 | pass 50 | pending 50 |
| Installer | 6 | 49 | mapped 49 | pass 49 | pending 49 |
| Packaging | 55 | 528 | mapped 132 / unmapped 396 | pass 132 / pending 396 | pending 528 |
| Total | 62 | 627 | mapped 231 / unmapped 396 | pass 231 / pending 396 | pending 627 |

ledger_after_d2_packaging_mapped=132
ledger_after_d2_packaging_local_pass=132
ledger_after_d2_packaging_unmapped=396
ledger_after_d2_ci_pending=528
ledger_after_d2_missing=0
ledger_after_d2_duplicate=0
ledger_after_d2_order_drift=0

## D3 product invocation

The single product-invocation fixture owns 61 contracts. Its managed replacement binds the exact
legacy file, product entrypoint, module, contract names/order, 418 assertion sites, and 844
source literals by SHA-256. It validates the ten-action surface, route presence, argument-array
command boundary, pre-execution automatic-reboot guard, diagnostic redaction, and required
dispatch/pending-commit tokens without invoking PowerShell, the product entrypoint, or a host
mutation executable. Five deterministic negative fixtures reject argument injection, a missing
route, a duplicate action, an unredacted bearer projection, and an executable mutation command.

| Check | Result |
| --- | --- |
| Replacement legacy contracts | PASS, 61/61, failed 0, skipped 0 |
| Deterministic negative tests | PASS, 5/5 |
| Pester 5.7.1 reference | PASS, 61/61, failed 0, skipped 0, not-run 0 |
| Delivery assembly | PASS, 312/312, failed 0, skipped 0 |
| Release solution build | PASS, warnings 0, errors 0 |
| Node strict-v2 ledger | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| Public source safety | PASS, 20/20, finding count 0 |

d3_legacy_path=packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
d3_replacement_owner=src/DesktopNode.Delivery.Tests/Delivery/Product/PcvDesktopNodeProductInvokeContractTests.cs
d3_contract_spec=config/pcv-desktop-node-product-invoke-contract-spec-v1.json
d3_contract_spec_sha256=9b4269d5820840f0f1b94795c7b2b97cca8bf0abfac2b15026fc5fc74e80b0f6
d3_legacy_source_sha256=0fff10664f5e65b72eb1cc86b668717b4caaeac15b4612e0d94c524ffc777955
d3_entrypoint_source_sha256=086d491283f170558899cbce5e640c17e774186ed83b86d39a791ce4a7f4c1d5
d3_module_source_sha256=8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3
d3_legacy_should_site_count=418
d3_required_literal_count=844
d3_replacement_summary={"runner":"dotnet-test/xunit","total":61,"executed":61,"passed":61,"failed":0,"skipped":0,"duration_ms":1000.0,"result":"Completed"}
d3_replacement_summary_sha256=cd7b67a61273ad8ee28acd41e9727041ca83652a3be78042dde261e2cdfc8f6a
d3_legacy_summary={"pester_version":"5.7.1","total":61,"passed":61,"failed":0,"skipped":0,"not_run":0,"duration_ms":16943.779,"result":"Passed"}
d3_legacy_summary_sha256=d2cde82b79f188d993e26af0031d26b16d4472d655ae1fa856c67a8b39671db6
d3_negative_argument_injection=pass
d3_negative_missing_route=pass
d3_negative_duplicate_action=pass
d3_negative_unredacted_bearer=pass
d3_negative_executable_mutation_command=pass
d3_release_build_warnings=0
d3_release_build_errors=0
d3_public_source_safety_tests=20
d3_public_source_safety_finding_count=0
d3_public_source_safety_report_sha256=603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd
d3_replacement_child_process_count=0
d3_replacement_write_api_count=0
d3_host_mutation_performed=false

## Ledger state after D3

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped 50 | pass 50 | pending 50 |
| Installer | 6 | 49 | mapped 49 | pass 49 | pending 49 |
| Packaging | 55 | 528 | mapped 193 / unmapped 335 | pass 193 / pending 335 | pending 528 |
| Total | 62 | 627 | mapped 292 / unmapped 335 | pass 292 / pending 335 | pending 627 |

ledger_after_d3_packaging_mapped=193
ledger_after_d3_packaging_local_pass=193
ledger_after_d3_packaging_unmapped=335
ledger_after_d3_ci_pending=528
ledger_after_d3_missing=0
ledger_after_d3_duplicate=0
ledger_after_d3_order_drift=0

## D4 product descriptors

Three legacy files own 58 diagnostics, product-manifest, and product-plan contracts. Their managed
replacement binds the exact legacy files, product module, names/order, 521 assertion sites, and
1,195 source literals by SHA-256. Structural checks fix the diagnostics policy and 17-source order,
Event Log service-action boundary, Web-only asset and four-file runtime payload cardinality,
schema-v2 manifest order, ten-action plan/defaults, owned paths, and no-auto-reboot/update-source
policy without invoking the product module or any host mutation executable.

| Check | Result |
| --- | --- |
| Replacement legacy contracts | PASS, 58/58, failed 0, skipped 0 |
| Deterministic structure fixtures | PASS, 6/6 |
| Pester 5.7.1 reference | PASS, 58/58, failed 0, skipped 0, not-run 0 |
| Delivery assembly | PASS, 376/376, failed 0, skipped 0 |
| Release solution build | PASS, warnings 0, errors 0 |
| Node strict-v2 ledger | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| Public source safety | PASS, 20/20, finding count 0 |

d4_legacy_files=3
d4_legacy_contracts=58
d4_contract_spec=config/pcv-desktop-node-product-descriptor-contract-spec-v1.json
d4_contract_spec_sha256=04abecd51ece223175bc0324d949b61f976fb31b5cf33327103f6b1beeee19da
d4_module_source_sha256=8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3
d4_legacy_should_site_count=521
d4_required_literal_count=1195
d4_replacement_summary={"runner":"dotnet-test/xunit","total":58,"executed":58,"passed":58,"failed":0,"skipped":0,"duration_ms":420.0,"result":"Completed"}
d4_replacement_summary_sha256=9c000af6a87d318a88ffb84cb05114cb67ca749a9e2e0b40e76ae5b1b31b578b
d4_legacy_summary={"pester_version":"5.7.1","total":58,"passed":58,"failed":0,"skipped":0,"not_run":0,"duration_ms":13977.631,"result":"Passed"}
d4_legacy_summary_sha256=2c1bf6e3d6af32fa04480f8f1d912f88236da420101a3e55c507d1ce892e9b4f
d4_negative_missing_manifest_entry=pass
d4_negative_duplicate_manifest_entry=pass
d4_negative_invalid_plan_path=pass
d4_negative_diagnostics_sensitive_key=pass
d4_negative_diagnostics_bearer_leakage=pass
d4_release_build_warnings=0
d4_release_build_errors=0
d4_public_source_safety_tests=20
d4_public_source_safety_finding_count=0
d4_public_source_safety_report_sha256=603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd
d4_replacement_child_process_count=0
d4_replacement_write_api_count=0
d4_host_mutation_performed=false

## Ledger state after D4

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped 50 | pass 50 | pending 50 |
| Installer | 6 | 49 | mapped 49 | pass 49 | pending 49 |
| Packaging | 55 | 528 | mapped 251 / unmapped 277 | pass 251 / pending 277 | pending 528 |
| Total | 62 | 627 | mapped 350 / unmapped 277 | pass 350 / pending 277 | pending 627 |

ledger_after_d4_packaging_mapped=251
ledger_after_d4_packaging_local_pass=251
ledger_after_d4_packaging_unmapped=277
ledger_after_d4_ci_pending=528
ledger_after_d4_missing=0
ledger_after_d4_duplicate=0
ledger_after_d4_order_drift=0

## D5 development verification policy

Eight legacy files own 51 circuit-breaker, architecture registry, current shadow workflow,
verification selector/runner/catalog, quality-tool, module-size ratchet, and strict-collection
contracts. Their managed replacement binds 17 policy sources, all eight legacy files, 356
assertion sites, 652 source literals, and exact names/order by SHA-256. It parses JSON policies,
keeps the current workflow as a Wave E cutover guard, restricts the managed catalog executable
allowlist to dotnet/Node/npm/git, verifies architecture faults/migrations and module ceilings, and
does not invoke a suite, PowerShell, Pester, a quality tool, or a host mutation command.

The first combined Pester reference found one real cross-contract regression: the Wave D helper
test had introduced one `BindingFlags.NonPublic` occurrence while the architecture inventory
requires zero. The helper test was converted to source-surface inspection; the inventory then
returned to zero, the helper test passed `19/19`, the focused architecture reference passed
`10/10`, and the full D5 reference passed `51/51`. The architecture registry was not weakened.

| Check | Result |
| --- | --- |
| Replacement legacy contracts | PASS, 51/51, failed 0, skipped 0 |
| Deterministic policy fixtures | PASS, 5/5 |
| Helper regression verification | PASS, 19/19; private-reflection occurrence 0 |
| Pester 5.7.1 reference | PASS, 51/51, failed 0, skipped 0, not-run 0 |
| Delivery assembly | PASS, 432/432, failed 0, skipped 0 |
| Release solution build | PASS, warnings 0, errors 0 |
| Node strict-v2 ledger | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| Public source safety | PASS, 20/20, finding count 0 |

d5_legacy_files=8
d5_legacy_contracts=51
d5_contract_spec=config/pcv-development-policy-contract-spec-v1.json
d5_contract_spec_sha256=a5df1c06b99077a6b8135b9bb5c9bc19caa3a765afa45b9ee5adb5815835b316
d5_source_file_count=17
d5_legacy_should_site_count=356
d5_required_literal_count=652
d5_replacement_summary={"runner":"dotnet-test/xunit","total":51,"executed":51,"passed":51,"failed":0,"skipped":0,"duration_ms":127.0,"result":"Completed"}
d5_replacement_summary_sha256=76df21c7303c665fc6a66d17a9a1feee1e93e945e6c14ed9b889708dcbb9c588
d5_legacy_initial_summary={"pester_version":"5.7.1","total":51,"passed":50,"failed":1,"skipped":0,"not_run":0,"duration_ms":19226.566,"result":"Failed"}
d5_legacy_initial_summary_sha256=676b601997b299d54e1066d3e982ffa13a351b2b2651fe9d53ebe66adb89c376
d5_legacy_initial_failure=private-reflection-occurrence-inventory-expected-0-actual-1
d5_legacy_summary={"pester_version":"5.7.1","total":51,"passed":51,"failed":0,"skipped":0,"not_run":0,"duration_ms":48945.464,"result":"Passed"}
d5_legacy_summary_sha256=85f51137648269996c63e7801feaaea4827aa3e74a5ea368fd0869dfefad1578
d5_negative_weakened_workflow=pass
d5_negative_forbidden_executable=pass
d5_negative_widened_module_threshold=pass
d5_negative_unknown_tool_version=pass
d5_negative_duplicate_suite=pass
d5_release_build_warnings=0
d5_release_build_errors=0
d5_public_source_safety_tests=20
d5_public_source_safety_finding_count=0
d5_public_source_safety_report_sha256=603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd
d5_replacement_child_process_count=0
d5_replacement_write_api_count=0
d5_host_mutation_performed=false

## Ledger state after D5

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped 50 | pass 50 | pending 50 |
| Installer | 6 | 49 | mapped 49 | pass 49 | pending 49 |
| Packaging | 55 | 528 | mapped 302 / unmapped 226 | pass 302 / pending 226 | pending 528 |
| Total | 62 | 627 | mapped 401 / unmapped 226 | pass 401 / pending 226 | pending 627 |

ledger_after_d5_packaging_mapped=302
ledger_after_d5_packaging_local_pass=302
ledger_after_d5_packaging_unmapped=226
ledger_after_d5_ci_pending=528
ledger_after_d5_missing=0
ledger_after_d5_duplicate=0
ledger_after_d5_order_drift=0

## D6 orchestration and timeout boundaries

Five legacy files own 44 BatchSupervisor, CI trigger, migration plan-only, ArtifactRoot, and
timeout/rate-limit preflight contracts. Their managed replacement binds 40 source files, all 36
current `-ArtifactRoot` runners, all five legacy files, 262 assertion sites, 607 source literals,
and exact names/order by SHA-256. It verifies batch state/retry/resume/reboot guards, one terminal
row, both workflow triggers, ten migration steps, rooted artifact resolution, and plan-only
timeout/rate-limit policy without starting a batch, process, service, VM, or network request.

| Check | Result |
| --- | --- |
| Replacement legacy contracts | PASS, 44/44, failed 0, skipped 0 |
| Deterministic orchestration fixtures | PASS, 5/5 |
| Pester 5.7.1 reference | PASS, 44/44, failed 0, skipped 0, not-run 0 |
| Delivery assembly | PASS, 481/481, failed 0, skipped 0 |
| Release solution build | PASS, warnings 0, errors 0 |
| Node strict-v2 ledger | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| Public source safety | PASS, 20/20, finding count 0 |

d6_legacy_files=5
d6_legacy_contracts=44
d6_contract_spec=config/pcv-orchestration-contract-spec-v1.json
d6_contract_spec_sha256=a9dc4702728706ec8bdab6327b74a28c6ba98f9f764d3063428125a385c86c1e
d6_source_file_count=40
d6_artifact_root_runner_count=36
d6_legacy_should_site_count=262
d6_required_literal_count=607
d6_replacement_summary={"runner":"dotnet-test/xunit","total":44,"executed":44,"passed":44,"failed":0,"skipped":0,"duration_ms":146.0,"result":"Completed"}
d6_replacement_summary_sha256=9632364a3c8e7f3f76c7007ad29ed86076ea9cbf28e6c6baa2030acc59b2eaaa
d6_legacy_summary={"pester_version":"5.7.1","total":44,"passed":44,"failed":0,"skipped":0,"not_run":0,"duration_ms":38321.128,"result":"Passed"}
d6_legacy_summary_sha256=e6febe9db2cf03e3c942ae26675fd52431cac38372f9de497a7e7e479390074b
d6_negative_double_terminal=pass
d6_negative_timeout_overflow=pass
d6_negative_artifact_escape=pass
d6_negative_duplicate_ci_trigger=pass
d6_negative_mutation_enabled_plan=pass
d6_release_build_warnings=0
d6_release_build_errors=0
d6_public_source_safety_tests=20
d6_public_source_safety_finding_count=0
d6_public_source_safety_report_sha256=603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd
d6_replacement_child_process_count=0
d6_replacement_write_api_count=0
d6_host_mutation_performed=false

## Ledger state after D6

| Domain | Files | Contracts | Mapping | Local parity | CI parity |
| --- | ---: | ---: | --- | --- | --- |
| Web | 1 | 50 | mapped 50 | pass 50 | pending 50 |
| Installer | 6 | 49 | mapped 49 | pass 49 | pending 49 |
| Packaging | 55 | 528 | mapped 346 / unmapped 182 | pass 346 / pending 182 | pending 528 |
| Total | 62 | 627 | mapped 445 / unmapped 182 | pass 445 / pending 182 | pending 627 |

ledger_after_d6_packaging_mapped=346
ledger_after_d6_packaging_local_pass=346
ledger_after_d6_packaging_unmapped=182
ledger_after_d6_ci_pending=528
ledger_after_d6_missing=0
ledger_after_d6_duplicate=0
ledger_after_d6_order_drift=0

## Shared fixture boundary

The Wave D helpers are read-only parsers over repository files and contained disposable fixtures.
They do not start PowerShell/Pester, an installer, service command, host mutation tool, or VM tool.
Helper verification and every batch result remain separate; helper PASS does not promote any
Packaging contract.

shared_helper_result=pass
shared_helper_total=12
shared_helper_failed=0
shared_helper_skipped=0
shared_helper_duration_ms=71
shared_helper_delivery_total=108
shared_helper_delivery_failed=0
shared_helper_delivery_skipped=0
shared_helper_changed_file_format_check=pass
preexisting_wave_c_whitespace_diagnostic_count=6
preexisting_wave_c_whitespace_diagnostic_disposition=review-only-not-mixed-into-wave-d-task1
managed_current_evidence_result=pass-read-only
managed_current_evidence_focused_tests=17
managed_current_evidence_writes=0
managed_current_evidence_child_processes=0
managed_current_evidence_catalog_activation=plan-only-foundation

## Claim boundary

packaging_local_parity=false
packaging_ci_parity=false
required_ci_pester_zero=false
required_ci_nonadmin_powershell_zero=false
cutover_completed=false
host_mutation_performed=false
msi_or_service_mutation=false
actual_vm_tested=false
package_candidate_created=false
public_trusted_signing=false
external_stable_publication=false
operational_current=0.42.74-admin-smoke
