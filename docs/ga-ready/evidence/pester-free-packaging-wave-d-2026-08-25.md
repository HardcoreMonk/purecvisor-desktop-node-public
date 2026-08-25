# Packaging verification Wave D local parity evidence

## Current verdict

Packaging Wave D is in progress. D1 and D2 local parity are PASS. The managed replacements
executed all `132/132` mapped Packaging contracts without skips. The one-time Pester 5.7.1
references also passed D1 `90/90` and D2 `42/42` without skips. D3-D10, aggregate Packaging local
parity, Required CI parity, branch-protection cutover, and managed current-evidence catalog
activation remain pending and are not claimed by this checkpoint.

wave_c_checkpoint_head=9bc382650059fe3ea759e27bcee69cce6823fe38
evidence_input_head=b86eddd37d0ed5af44c8b33f426c0eef8024eb44
d2_input_head=a38607b14ac1d2dd5904dc408d4d0cb757f1f936
input_dirty_state=clean

## Fixed ten-batch inventory

| Batch | Responsibility | Legacy files | Legacy contracts | Replacement | Legacy reference | Ledger state |
| --- | --- | ---: | ---: | --- | --- | --- |
| D1 | canonical admin-smoke evidence | 1 | 90 | 90/90 pass | 90/90 pass | mapped / local pass / CI pending |
| D2 | current/promotion/package evidence | 5 | 42 | 42/42 pass | 42/42 pass | mapped / local pass / CI pending |
| D3 | product invocation | 1 | 61 | not-run | not-run | unmapped / local pending / CI pending |
| D4 | product descriptors | 3 | 58 | not-run | not-run | unmapped / local pending / CI pending |
| D5 | development verification policy | 8 | 51 | not-run | not-run | unmapped / local pending / CI pending |
| D6 | orchestration and timeout boundaries | 5 | 44 | not-run | not-run | unmapped / local pending / CI pending |
| D7 | package/public-distribution preflight | 8 | 52 | not-run | not-run | unmapped / local pending / CI pending |
| D8 | manual-admin/public-ops readiness | 8 | 45 | not-run | not-run | unmapped / local pending / CI pending |
| D9 | installed-smoke descriptors | 8 | 31 | not-run | not-run | unmapped / local pending / CI pending |
| D10 | reconciliation/lifecycle policy | 8 | 54 | not-run | not-run | unmapped / local pending / CI pending |
| **Total** |  | **55** | **528** | **132 pass / 396 not-run** | **132 pass / 396 not-run** | **132 mapped local pass / 396 unmapped / CI pending** |

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
