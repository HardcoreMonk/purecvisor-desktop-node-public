# GA-ready Evidence Ledger - 2026-05-04

ledger_id: ga-ready-evidence-ledger-2026-05-04
created_at: 2026-05-04T00:54:00+09:00
source_commit_sha: f5a7539972199afd285f94ee622d59d409a411e7
route_matrix_commit_sha: f5a7539972199afd285f94ee622d59d409a411e7
machine_readable_json_created: no

## Evidence Group: Full Admin Host Mutation Gate 2026-05-16 0.42.26 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-16-04226-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate 0.42.26
host_capability_snapshot: 0.42.26-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS, PR #145 public-boundary PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-16T22:27:09+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, product wrapper service-action repair contract, current-card UI contract, manual-admin lifecycle evidence, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: 0.42.26 package/full admin host mutation/current-card

evidence_id: full-admin-host-mutation-gate-record-20260516-04226
route_or_operation: package build, manual-admin descriptor candidate, full admin host mutation gate, installed current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary-current-card
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: d6500c01c972cbc7ca1e290e51120181ceea1501
artifact_or_package_version: 0.42.26-admin-smoke; artifacts/admin-smoke-package-20260516-04226; artifacts/manual-admin-campaign-20260516-04225-04226; artifacts/batch-runs/full-admin-host-mutation-gate-20260516-04226; artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226; artifacts/os-mutation-gates-batch-profile-20260516-04226; artifacts/installed-operator-surface-current-card-20260516-04226
target_owner: windows-desktop-node
implementation_basis: selector guard included package build, 0.42.25 to 0.42.26 descriptor/readiness, Batch Supervisor FullAdminHostMutationGate run, installed Web/TUI/CLI current-card smoke, PR #145 public-boundary main push PASS
fallback_policy: 0.42.25 full gate/current-card and 0.42.24 to 0.42.25 closed package-pair remain historical/current closed package-pair references; latest full gate claim belongs to 0.42.26
identity_requirement: protected token file used for installed ops summary and TUI smoke; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.26-admin-smoke package and elevated Batch Supervisor run
host_capability_snapshot: package MSI SHA-256 aa596c785fdd2a941fa8d88ece9c177b47d56a4f762666f31c1efaffdbc30685; full-gate MSI SHA-256 f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7; provenance commit d6500c01c972cbc7ca1e290e51120181ceea1501; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.26-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: package build, non-mutating descriptor batch, Batch Supervisor actual host mutation run, installed product wrapper RepairInstalled -BatchEvidenceRoot, installed pcvcli ops summary, installed pcvtui smoke-once runtime
result: pass
observed_result: full-gate batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, latest_batch_id=full-admin-host-mutation-gate-20260516-04226, current_evidence_contract=runtime-api-current-evidence-rollup-v1, registry_bridge_route_count=4, descriptor 04225-to-04226 readiness pass but overall_status=blocked-by-missing-evidence, public-boundary run=25961834812 job=76318357776
created_at: 2026-05-16T22:27:09+09:00
stale_triggers: package payload, service path, native service-action wrapper behavior, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, manual-admin lifecycle evidence, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-14 0.42.15 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-14-04215-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04215-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate 0.42.15
host_capability_snapshot: 0.42.15-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS, descriptor selector guard retained
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-14T23:41:58+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, product wrapper service-action repair contract, current-card UI contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.15 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-record-20260514-04215
route_or_operation: full admin host mutation gate and installed current-card selector guard smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary-current-card
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: 8ddf4b9715dd50cd4aa94c4fa77eb17ba8beaaff
artifact_or_package_version: 0.42.15-admin-smoke; artifacts/admin-smoke-package-20260514-04215-clean; artifacts/batch-runs/full-admin-host-mutation-gate-20260514-234158-04215; artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-234158-04215; artifacts/os-mutation-gates-batch-profile-20260514-234158-04215; artifacts/installed-current-card-20260514-04215-fullgate
target_owner: windows-desktop-node
implementation_basis: clean 0.42.15 package build, 0.42.14 to 0.42.15 manual-admin package-pair closure, Batch Supervisor FullAdminHostMutationGate run, installed ops.summary batch_evidence reader
fallback_policy: 2026-05-14 0.42.12 explicit/rerun, 2026-05-13 0.42.12, 0.42.11-admin-smoke, 0.42.9-admin-smoke, 0.42.8-admin-smoke, 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; latest full gate claim belongs to this 0.42.15 run
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.15-admin-smoke full-gate package and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 a00f07e3a86b5e62569c9ddaa17052d74f881f48d9ec6c9043be9815762e690d; clean package MSI SHA-256 80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7; provenance commit 8ddf4b9715dd50cd4aa94c4fa77eb17ba8beaaff; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.15-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor actual host mutation run, installed product wrapper RepairInstalled -BatchEvidenceRoot, installed pcvcli ops summary against canonical artifacts root
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, latest_batch_id=full-admin-host-mutation-gate-20260514-234158-04215, descriptor_batch_id=manual-admin-campaign-descriptor-20260514-04214-04215, descriptor_excluded_from_operational_latest=true, errors_count=0
created_at: 2026-05-14T23:41:58+09:00
stale_triggers: package payload, service path, native service-action wrapper behavior, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Manual Admin Package-Pair 2026-05-14 0.42.14 to 0.42.15

evidence_id: manual-admin-campaign-2026-05-14-04214-04215
artifact_or_package_version: docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04214-04215.md
runner_version: manual-admin lifecycle campaign and Batch Supervisor descriptor
host_capability_snapshot: 0.42.14-admin-smoke baseline to 0.42.15-admin-smoke target, clean-host Windows Update NoContact recovery, Burn/MSIX lifecycle, installed runtime ops summary, descriptor generation PASS
exact_command_mode: manual-admin package-pair campaign; public trusted signing and external stable publication not claimed
created_at: 2026-05-14T23:35:00+09:00
stale_triggers: package payload, update package, clean-host lifecycle runner, Burn/MSIX lifecycle, descriptor summary contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.14 to 0.42.15 package-pair closure

evidence_id: manual-admin-campaign-record-20260514-04214-04215
route_or_operation: installed update/rollback, clean-host install/update/rollback, Burn, MSIX, installed runtime ops summary, descriptor generation
route_surface: packaging-release-host-ops-current-card
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: 8ddf4b9715dd50cd4aa94c4fa77eb17ba8beaaff
artifact_or_package_version: baseline 0.42.14-admin-smoke; target 0.42.15-admin-smoke; artifacts/admin-smoke-package-20260514-04215-clean; artifacts/manual-admin-campaign-20260514-04214-04215; artifacts/msix-package-lifecycle-smoke-20260514-04214-04215; artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04214-04215
target_owner: windows-desktop-node
implementation_basis: clean provenance package build plus installed and clean-host package-pair campaign
fallback_policy: 0.42.12 to 0.42.13 package-pair remains historical predecessor
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: loopback/internal admin-smoke only; public stable publication not claimed
runner_version: 0.42.14 baseline MSI, 0.42.15 target MSI/update package, descriptor runner
host_capability_snapshot: target MSI SHA-256 80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7; update ZIP SHA-256 06f5879431bac90da6da09f243c1e91c6bb875358779e4cedc98a9a3860dad6b; automatic_recovery_performed=true; recovery_actions=1
exact_command_mode: manual-admin installed and clean-host lifecycle campaign plus non-mutating descriptor batch manifest
result: pass
observed_result: readiness pass, installed update/rollback pass, clean-host pass-with-windows-update-nocontact-recovery, Burn pass, MSIX pass, installed runtime ops summary pass, descriptor overall_status=pass
created_at: 2026-05-14T23:35:00+09:00
stale_triggers: package payload, clean-host runner recovery contract, update/rollback behavior, Burn/MSIX lifecycle, descriptor contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-14 0.42.12 Explicit Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate explicit rerun
host_capability_snapshot: 0.42.12-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS, Web Console current-card smoke PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-14T14:01:26+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, product wrapper native service-action repair contract, current-card UI contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.12 full admin host mutation gate explicit rerun

evidence_id: full-admin-host-mutation-gate-explicit-record-20260514-04212
route_or_operation: full admin host mutation gate explicit rerun and installed/Web Console current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary-web-console-current-card
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: d338b8a99f3e1e3839ac89a6de0da034ff3da148
artifact_or_package_version: 0.42.12-admin-smoke; artifacts/admin-smoke-package-20260513-04212; artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit; artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit; artifacts/os-mutation-gates-batch-profile-20260514-140126-04212-explicit; artifacts/installed-batch-evidence-current-card-20260514-140126-04212-explicit; artifacts/web-console-current-card-20260514-140126-04212-explicit
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate explicit rerun, unchanged 0.42.12 product payload, product wrapper native service-action repair, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader, Web Console current-card browser smoke
fallback_policy: 2026-05-14 rerun, 2026-05-13 0.42.12, 0.42.11-admin-smoke, 0.42.9-admin-smoke, 0.42.8-admin-smoke, 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; latest full gate claim belongs to this explicit rerun
identity_requirement: protected token file used for installed ops summary and browser smoke; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.12-admin-smoke full-gate rebuild from current main and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db; package MSI SHA-256 c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e; full-gate commit d338b8a99f3e1e3839ac89a6de0da034ff3da148; package provenance commit 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.12-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor actual host mutation run, installed product wrapper RepairInstalled -BatchEvidenceRoot, installed pcvcli ops summary against `--batch-evidence-root`, Web Console current-card browser smoke
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, latest_batch_id=full-admin-host-mutation-gate-20260514-140126-04212-explicit, errors_count=0, wrapper_repair_used_native_service_action=true, wrapper_repair_skipped_outer_start=true, token_value_observed_in_ui_text=false
created_at: 2026-05-14T14:01:26+09:00
stale_triggers: package payload, service path, native service-action wrapper behavior, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-14 0.42.12 Rerun Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate rerun
host_capability_snapshot: 0.42.12-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-14T12:30:53+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, product wrapper native service-action repair contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.12 full admin host mutation gate rerun

evidence_id: full-admin-host-mutation-gate-rerun-record-20260514-04212
route_or_operation: full admin host mutation gate rerun and installed listener current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: b9c2c25b2ea88f67a0b0ffa5e7e03240eb0ce2fe
artifact_or_package_version: 0.42.12-admin-smoke; artifacts/admin-smoke-package-20260513-04212; artifacts/batch-runs/full-admin-host-mutation-gate-20260514-04212-rerun; artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-04212-rerun; artifacts/os-mutation-gates-batch-profile-20260514-04212-rerun; artifacts/installed-batch-evidence-current-card-20260514-04212-rerun
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate rerun, unchanged 0.42.12 product payload, product wrapper native service-action repair, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader
fallback_policy: 2026-05-13 0.42.12, 0.42.11-admin-smoke, 0.42.9-admin-smoke, 0.42.8-admin-smoke, 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; latest full gate claim belongs to this rerun
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.12-admin-smoke full-gate rebuild from current main and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 b18d86c197a568ed9b5f6bb38580e568de7a989dda8d730e585684d1c5131b7a; package MSI SHA-256 c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e; full-gate commit b9c2c25b2ea88f67a0b0ffa5e7e03240eb0ce2fe; package provenance commit 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.12-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor actual host mutation run, installed product wrapper RepairInstalled -BatchEvidenceRoot, installed pcvcli ops summary against `--batch-evidence-root`
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, latest_batch_id=full-admin-host-mutation-gate-20260514-04212-rerun, errors_count=0, wrapper_repair_used_native_service_action=true, wrapper_repair_skipped_outer_start=true
created_at: 2026-05-14T12:30:53+09:00
stale_triggers: package payload, service path, native service-action wrapper behavior, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Post-04212 1-2-3-4-5 Current-card Follow-up 2026-05-14

evidence_id: post-04212-followup-1-2-3-4-5-current-card-2026-05-14
artifact_or_package_version: docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md
runner_version: repo product-payload triage plus installed Web Console current-card browser smoke
host_capability_snapshot: no product payload change after 0.42.12 package provenance; Dashboard and Evidence view show latest 0.42.12 rerun batch
exact_command_mode: non-mutating git product-payload diff review; installed listener browser current-card smoke; host mutation not performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-14T13:47:30+09:00
stale_triggers: product payload changes, ops.summary batch_evidence contract changes, Web Console current-card rendering changes, or public release boundary changes
waiver_status: triage/browser evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: post-04212 1-2-3-4-5 current-card follow-up

evidence_id: post-04212-followup-current-card-record-20260514
route_or_operation: post-04212 product payload triage and Web Console current-card smoke
route_surface: Dashboard current evidence card and Evidence view
risk_tier: tier1-installed-readonly-browser-smoke
current_owner: desktop-node-host-packaging
commit_sha: 8224af81c00482145b6c08dcde8c92a039b2aa26
artifact_or_package_version: latest product payload package 0.42.12-admin-smoke; next version hint 0.42.13-admin-smoke deferred; artifacts/web-console-current-card-20260514-04212-rerun-followup
target_owner: windows-desktop-node
implementation_basis: product payload provenance check from 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3 to main 8224af81c00482145b6c08dcde8c92a039b2aa26 plus headless browser current-card smoke against installed listener
fallback_policy: keep 0.42.12 package build, 0.42.12 full admin host mutation rerun, and 0.42.11 to 0.42.12 manual-admin package-pair as current claims
identity_requirement: protected token file used for browser smoke; token value not recorded
network_exposure_gate: loopback installed Web Console only; public stable publication not claimed
runner_version: current-card-smoke.mjs artifact helper using Chrome DevTools Protocol
host_capability_snapshot: product_payload_change_detected=false; package_build_decision=deferred-until-next-product-payload-change; full_admin_host_mutation_campaign_decision=not-run-no-product-payload; manual_admin_package_pair_campaign_decision=deferred-until-next-product-payload-change; dashboard_current_card_smoke=pass; evidence_view_current_card_smoke=pass
exact_command_mode: non-mutating repository inspection and installed listener browser smoke
result: pass-dashboard-current-card-smoke-deferred-product-chain
observed_result: Dashboard and Evidence view mention full-admin-host-mutation-gate-20260514-04212-rerun and 0.42.12-admin-smoke; token_value_observed_in_ui_text=false; host_mutation_performed=false
created_at: 2026-05-14T13:47:30+09:00
stale_triggers: any product payload commit after 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3, batch evidence summary shape changes, current-card UI changes, or a new manual-admin clean-host summary with recovery_actions
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Post-04212 Follow-up Execution 2026-05-14

evidence_id: post-04212-followup-execution-2026-05-14
artifact_or_package_version: docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md
runner_version: repo triage only
host_capability_snapshot: no new product payload after 0.42.12 package provenance; 0.42.13 package/full gate/package-pair deferred
exact_command_mode: git product-payload diff review; host mutation not performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-14T11:57:46+09:00
stale_triggers: Runtime/Core, Hyper-V, Host Ops, Packaging, Operator Surface product payload changes, or clean-host recovery summary contract changes
waiver_status: triage evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: post-04212 follow-up deferred package execution

evidence_id: post-04212-followup-execution-record-20260514
route_or_operation: post-04212 product payload/package-pair triage
route_surface: manual-admin descriptor and GA-ready evidence indexes
risk_tier: tier0-nonmutating-doc-triage
current_owner: desktop-node-host-packaging
commit_sha: 0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea
artifact_or_package_version: latest product payload package 0.42.12-admin-smoke; next version hint 0.42.13-admin-smoke deferred
target_owner: windows-desktop-node
implementation_basis: product payload provenance check from 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3 to main 0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea
fallback_policy: keep 0.42.12 package build, 0.42.12 full admin host mutation, and 0.42.11 to 0.42.12 manual-admin package-pair as current claims
identity_requirement: none
network_exposure_gate: no network or host mutation executed
runner_version: repository diff and documentation update
host_capability_snapshot: product_payload_change_detected=false; package_build_decision=deferred-until-next-product-payload-change; full_admin_host_mutation_campaign_decision=not-run-no-product-payload; manual_admin_package_pair_campaign_decision=deferred-until-next-product-payload-change; clean_host_recovery_guard_decision=ready-for-next-clean-host-run-not-executed
exact_command_mode: non-mutating repository inspection and documentation tests
result: pass-triage-deferred-package-and-host-mutation
observed_result: changed paths since 0.42.12 package provenance were docs, tests, and clean-host runner guard only
created_at: 2026-05-14T11:57:46+09:00
stale_triggers: any product payload commit after 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3 or a new manual-admin clean-host summary with recovery_actions
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-13 0.42.12 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-13-04212-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate
host_capability_snapshot: 0.42.12-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-13T23:53:00+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, product wrapper native service-action repair contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.12 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-record-20260513-04212
route_or_operation: full admin host mutation gate and installed listener current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3
artifact_or_package_version: 0.42.12-admin-smoke; artifacts/admin-smoke-package-20260513-04212; artifacts/batch-runs/full-admin-host-mutation-gate-20260513-04212; artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-04212; artifacts/os-mutation-gates-batch-profile-20260513-04212; artifacts/installed-batch-evidence-current-card-20260513-04212
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate, ops summary data builder split, product wrapper native service-action repair, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader
fallback_policy: this 2026-05-13 0.42.12 full gate is historical after the 2026-05-14 rerun; 0.42.11-admin-smoke, 0.42.9-admin-smoke, 0.42.8-admin-smoke, 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are also historical
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.12-admin-smoke admin package build and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 74735f98bb7afbaa46127eddb200a3de6e5a954b240d7a65578072960368e233; package MSI SHA-256 c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.12-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: package build, Batch Supervisor actual host mutation run, installed product wrapper RepairInstalled -BatchEvidenceRoot, installed pcvcli ops summary against `--batch-evidence-root`
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, errors_count=0, wrapper_repair_used_native_service_action=true, wrapper_repair_skipped_outer_start=true
created_at: 2026-05-13T23:53:00+09:00
stale_triggers: package payload, service path, native service-action wrapper behavior, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-13 0.42.11 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-13-04211-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate
host_capability_snapshot: 0.42.11-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-13T14:29:00+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, product wrapper native service-action repair contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.11 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-record-20260513-04211
route_or_operation: full admin host mutation gate and installed listener current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: 987beb51025a5aa926df7d9a905019b4d6d29705
artifact_or_package_version: 0.42.11-admin-smoke; artifacts/admin-smoke-package-20260513-04211; artifacts/batch-runs/full-admin-host-mutation-gate-20260513-0429-04211; artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-0429-04211; artifacts/os-mutation-gates-batch-profile-20260513-0429-04211; artifacts/installed-batch-evidence-current-card-20260513-04211
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate, product wrapper native service-action repair, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader
fallback_policy: 0.42.9-admin-smoke, 0.42.8-admin-smoke, 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; latest claim belongs to 0.42.11
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.11-admin-smoke admin package build and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 902e175cd6354843da2c928e2b6772f04d40240f02783e4edfed460ba0f9fce2; package MSI SHA-256 750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1; update ZIP SHA-256 734114e0ea7c9d486a1d329cd551a6abc34d20f3801a944bd5dbcb8c1c4a9991; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.11-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: package build, Batch Supervisor actual host mutation run, installed product wrapper RepairInstalled -BatchEvidenceRoot, installed pcvcli ops summary against `--batch-evidence-root`
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, errors_count=0, wrapper_repair_used_native_service_action=true, wrapper_repair_skipped_outer_start=true
created_at: 2026-05-13T14:29:00+09:00
stale_triggers: package payload, service path, native service-action wrapper behavior, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-13 0.42.9 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-13-0429-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate
host_capability_snapshot: 0.42.9-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-13T04:02:13+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.9 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-record-20260513-0429
route_or_operation: full admin host mutation gate and installed listener current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: f0620f2e18ae25de8751333684cb74b5051dcdc6
artifact_or_package_version: 0.42.9-admin-smoke; artifacts/admin-smoke-package-20260513-0429; artifacts/batch-runs/full-admin-host-mutation-gate-20260513-040213-0429; artifacts/routeparity-service-msi-hyperv-batch-profile-20260513-040213-0429; artifacts/os-mutation-gates-batch-profile-20260513-040213-0429; artifacts/installed-batch-evidence-current-card-20260513-0429
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader
fallback_policy: 0.42.8-admin-smoke, 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; this 0.42.9 claim is preserved as predecessor after 0.42.11
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.9-admin-smoke admin package build and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9; package MSI SHA-256 a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.9-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: package build, Batch Supervisor actual host mutation run, installed pcvcli ops summary against `--batch-evidence-root`
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, errors_count=0
created_at: 2026-05-13T04:02:13+09:00
stale_triggers: package payload, service path, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-12 0.42.8 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-12-0428-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate
host_capability_snapshot: 0.42.8-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-12T23:36:50+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.8 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-record-20260512-0428
route_or_operation: full admin host mutation gate and installed listener current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: 5397e580c98a34e8b7beb5b9773d1d857025315b
artifact_or_package_version: 0.42.8-admin-smoke; artifacts/admin-smoke-package-20260512-0428-postmerge; artifacts/batch-runs/full-admin-host-mutation-gate-20260512-233650-0428-r2; artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2; artifacts/os-mutation-gates-batch-profile-20260512-233650-0428-r2; artifacts/installed-batch-evidence-current-card-20260512-0428-post-gate-r2
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader
fallback_policy: 0.42.7-admin-smoke, 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; this 0.42.8 record was later superseded by 0.42.9 and 0.42.11
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.8-admin-smoke admin package build and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 01762ee3fd103981ac6fce121b6749e832dfabc7420123a6363f7fbe0e0f8f99; post-merge package MSI SHA-256 e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.8-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: package build, Batch Supervisor actual host mutation run, installed pcvcli ops summary against `--batch-evidence-root`
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, errors_count=0
created_at: 2026-05-12T23:36:50+09:00
stale_triggers: package payload, service path, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-12 0.42.7 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-12-0427-hostmutation
artifact_or_package_version: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0427-hostmutation.md
runner_version: Batch Supervisor FullAdminHostMutationGate
host_capability_snapshot: 0.42.7-admin-smoke Service/MSI/Hyper-V route smoke, firewall/LAN/Event Log/internal trust-store OS mutation gate, installed listener batch_evidence current card PASS
exact_command_mode: elevated Batch Supervisor `-AllowHostMutation`; host mutation performed; public trusted signing and external stable publication not claimed
created_at: 2026-05-12T18:13:09+09:00
stale_triggers: DesktopNode.Host payload, Web Console installed payload, WiX/MSIX/Burn package output, Batch Supervisor summary contract, ops.summary batch_evidence contract, or public release boundary changes
waiver_status: internal/private admin-smoke evidence only
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.7 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-record-20260512-0427
route_or_operation: full admin host mutation gate and installed listener current-card smoke
route_surface: installed-service-msi-hyperv-os-mutation-ops-summary
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-packaging
commit_sha: 8d6aea7bac30ce279093ec61406c62428f69e79c
artifact_or_package_version: 0.42.7-admin-smoke; artifacts/admin-smoke-package-20260512-0427; artifacts/batch-runs/full-admin-host-mutation-gate-20260512-181309-0427; artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-181309-0427; artifacts/os-mutation-gates-batch-profile-20260512-181309-0427; artifacts/installed-batch-evidence-current-card-20260512-0427
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor FullAdminHostMutationGate, service/MSI/Hyper-V route smoke, OS mutation gate, installed ops.summary batch_evidence reader
fallback_policy: 0.42.3-admin-smoke and 0.42.2-admin-smoke full gates are historical; 0.42.9 supersedes this 0.42.7 record as latest claim
identity_requirement: protected token file used for installed ops summary; token value not recorded
network_exposure_gate: LAN listener smoke used internal address only; public stable publication not claimed
runner_version: 0.42.7-admin-smoke admin package build and elevated Batch Supervisor run
host_capability_snapshot: full-gate MSI SHA-256 9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4; package build MSI SHA-256 256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9; signing mode AllowUnsignedDev; final service Running; installed manifest 0.42.7-admin-smoke; Web Console http://127.0.0.1/ 200; /pcv-config.js 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall final count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: package build, Batch Supervisor actual host mutation run, installed pcvcli ops summary against `--batch-evidence-root`
result: pass
observed_result: batch ok=true, total_steps=2, executed_steps=2, route_msi_hyperv.status=available, os_mutation.status=available, batch_evidence.status=available, errors_count=0
created_at: 2026-05-12T18:13:09+09:00
stale_triggers: package payload, service path, Hyper-V adapter behavior, OS mutation runner cleanup, batch evidence summary shape, current-card UI contract, or public release boundary changes
waiver_status: public trusted signing and external stable publication not claimed

## Evidence Group: Installed noVNC And TUI Operator Smoke 2026-05-10 0411

evidence_id: installed-novnc-tui-operator-smoke-2026-05-10-0411
artifact_or_package_version: docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md
runner_version: installed target-backed noVNC streaming runner and installed pcvtui smoke-once runner
host_capability_snapshot: 0.41.1-admin-smoke installed product update, target-backed noVNC WebSocket-to-loopback TCP streaming PASS, installed pcvtui runtime smoke PASS, service final Running
exact_command_mode: installed product update, temporary noVNC service PathName mutation and restore, installed pcvtui --smoke-once runtime; no public trusted signing, external stable publication, winget submission, public URL, or public clean-host evidence
created_at: 2026-05-10T15:45:53+09:00
stale_triggers: noVNC WebSocket bridge path/auth policy, service PathName restoration, protected token source policy, TUI --smoke-once output contract, installed payload hashes, or public release boundary changes
waiver_status: installed noVNC and TUI operator smoke closed; installed account login smoke passed separately in `artifacts/installed-account-login-smoke-20260510-0410-final`
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: target-backed noVNC streaming and installed TUI operator smoke

evidence_id: installed-novnc-tui-operator-smoke-record-20260510-0411
route_or_operation: target-backed noVNC streaming bridge and installed TUI runtime route
route_surface: installed-local-api-websocket-and-tui-client
risk_tier: tier3-installed-service-mutation
current_owner: desktop-node-host-tui-packaging
commit_sha: a3226ef637ea895d2f2a9956599e0d5e79d00410
artifact_or_package_version: 0.41.1-admin-smoke; artifacts/installed-novnc-tui-operator-smoke-20260510-0411; artifacts/target-backed-novnc-installed-streaming-smoke-20260510-0411; artifacts/installed-tui-operator-smoke-20260510-0411
target_owner: windows-desktop-node
implementation_basis: DesktopNode.Host noVNC WebSocket bridge, DesktopNode.Tui --smoke-once, Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1, Invoke-PcvInstalledTuiOperatorSmoke.ps1
fallback_policy: noVNC remains disabled unless explicit target host/port is configured; Web Console and pcvcli.exe remain independent Local API clients
identity_requirement: bearer token is read from protected token file or installed service source and never recorded as a value; TUI output is redacted
network_exposure_gate: noVNC target loopback 127.0.0.1 only; no firewall or LAN exposure added by this smoke
runner_version: dotnet publish 0.41.1-admin-smoke, product update, installed noVNC runner, installed TUI runner, focused xUnit and Pester guard
host_capability_snapshot: update 0.41.0-admin-smoke to 0.41.1-admin-smoke succeeded; MSI SHA-256 0583f71c4fcc1ed0da886e55f2fbac6713d8bc731fad7d33d6c189c214fcea6e; target frame SHA-256 c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106; echoed frame SHA-256 matched; path_name_restored=true; pcvtui exit_code=0; final_service=Running
exact_command_mode: Invoke-PcvDesktopNodeProduct.ps1 -Action Update; Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1; Invoke-PcvInstalledTuiOperatorSmoke.ps1
result: installed-smoke-pass
observed_result: target_backed_novnc_installed_streaming_smoke=pass; installed_tui_operator_smoke=pass; host_mutation_performed=true for product update/noVNC temporary service config; TUI runner host_mutation_performed=false; token_value_observed=false
created_at: 2026-05-10T15:45:53+09:00
stale_triggers: installed product version, service PathName, noVNC target guard, WebSocket bridge behavior, TUI output contract, token redaction, or public release boundary changes
waiver_status: public trusted signing and external stable publication not-claimed; installed account login smoke passed in `artifacts/installed-account-login-smoke-20260510-0410-final`

## Evidence Group: Product TUI Service Plan Closure 2026-05-10

evidence_id: product-tui-service-plan-closure-2026-05-10
artifact_or_package_version: docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md
runner_version: focused xUnit TUI verification plus documentation closure sync
host_capability_snapshot: pcvtui.exe product TUI client, Local API route registry, token source parsing, renderer, poller, mutation confirmation guard, installer/product payload contract
exact_command_mode: local focused TUI tests and documentation sync; no installed service, MSI lifecycle, firewall, trust-store, LAN, Event Log, update/rollback, Hyper-V, public upload, or winget submission
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: TUI command-line options, token source policy, TUI tab route registry, renderer text contract, installer payload files, product manifest TUI metadata, update payload validation, or TUI documentation changes
waiver_status: plan closure only; installed interactive TUI operator smoke is recorded separately by installed-novnc-tui-operator-smoke-2026-05-10-0411
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: product TUI service implementation plan closure

evidence_id: product-tui-service-plan-closure-record-20260510
route_or_operation: product TUI service implementation plan closure
route_surface: local-api-tui-client
risk_tier: tier0-doc-closure
current_owner: desktop-node-tui
commit_sha: codex/installed-account-novnc-evidence-clean
artifact_or_package_version: docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-product-tui-service.md; docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md
target_owner: windows-desktop-node
implementation_basis: src/DesktopNode.Tui, src/DesktopNode.Tui.Tests, installer payload wiring, product manifest TUI metadata, user/operator documentation
fallback_policy: Web Console and pcvcli.exe remain independent Local API clients; TUI is not a Windows SCM service
identity_requirement: TUI token values remain redacted; supported sources are inline token, token file, environment token, and protected token file
network_exposure_gate: loopback Local API client by default; no LAN/firewall mutation executed by this closure sync
runner_version: dotnet/xUnit
host_capability_snapshot: TUI tests cover options, token resolution, route registry, bearer transport, state/poller, renderer, and interactive mutation guards
exact_command_mode: dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore
result: plan-closure-synchronized
observed_result: TUI focused test suite passed with 115 tests; plan checkboxes synchronized to closed state; host_mutation_performed=false
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: TUI feature, route, payload, manifest, docs, or packaging contract changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Account/RBAC/JWT Console Code-Level 2026-05-10

evidence_id: account-rbac-jwt-console-code-level-2026-05-10
artifact_or_package_version: docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md
runner_version: code-level API/Host/Web/packaging verification
host_capability_snapshot: account auth routes, JWT access/refresh, RBAC route gating, Web Console session UX, Windows vmconnect console handoff, noVNC default disabled boundary, installed service account/JWT file path contract; follow-up noVNC bridge evidence recorded below
exact_command_mode: local code-level tests and packaging contract tests; no installed listener account login execution, target-backed noVNC streaming on an installed service, MSI lifecycle, firewall, trust-store, LAN, Event Log, Hyper-V, HTTPS/443, public upload, or winget submission
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: auth route contract, JWT signing policy, account file schema, RBAC permissions, service binary path, Web Console auth/session UX, console capability policy, or noVNC bridge implementation changes
waiver_status: code-level applied; installed account login smoke follow-up passed in `artifacts/installed-account-login-smoke-20260510-0410-final`; noVNC bridge follow-up recorded separately
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: account RBAC JWT and console capability

evidence_id: account-rbac-jwt-console-code-level-record-20260510
route_or_operation: account auth, RBAC, JWT refresh, and console capability
route_surface: local-api-web-console
risk_tier: tier1-code-level-auth-console
current_owner: desktop-node-api-host-web-console
commit_sha: codex/account-rbac-jwt-console
artifact_or_package_version: docs/ga-ready/evidence/account-rbac-jwt-console-code-level-2026-05-10.md
target_owner: windows-desktop-node
implementation_basis: DesktopNodeAccountAuthService, DesktopNodeApiRequestProcessor auth/console routes, DesktopNode.Host service binary path, Web Console account session and console panels
fallback_policy: existing protected bearer-token route gate remains authoritative until account auth is fully configured
identity_requirement: account auth requires local accounts.json plus jwt-signing-key.txt; no default account/password is created
network_exposure_gate: loopback code-level default; no LAN/firewall/TLS mutation executed
runner_version: dotnet/xUnit, Pester, npm TypeScript/static parity
host_capability_snapshot: auth.login/auth.refresh/auth.logout/auth.session/auth.rbac routes present; viewer/operator/admin RBAC present; service path includes account-file and jwt-signing-key-file; noVNC default disabled for the account slice; Windows console transport=vmconnect-handoff; noVNC bridge follow-up evidence is separate
exact_command_mode: code-level tests; no service restart, MSI repair/install, URL ACL mutation, firewall mutation, trust-store mutation, Event Log mutation, or Hyper-V mutation
result: code-level-pass-installed-account-smoke-closed
observed_result: API account auth tests 6 passed, Host targeted tests 72 passed, packaging plan/manifest/invoke Pester 90 passed, Web static Pester 44 passed, admin smoke evidence docs Pester 22 passed, Web npm/parity passed, follow-up reruns include DesktopNode.Api.Tests 139 passed and DesktopNode.Tui.Tests 115 passed, installed account smoke artifact `artifacts/installed-account-login-smoke-20260510-0410-final` passed, target-backed noVNC installed streaming and installed TUI operator smoke passed, git diff --check exit 0 with LF/CRLF warnings only
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: service PathName, account bootstrap file schema, JWT validation, RBAC permission matrix, Web session storage, console capability response, or noVNC bridge status changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Installed Account Login And noVNC Bridge Code-Level 2026-05-10

evidence_id: installed-account-login-novnc-bridge-code-level-2026-05-10
artifact_or_package_version: docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md
runner_version: code-level API/Host tests plus installed account login smoke runner static guard and installed admin smoke
host_capability_snapshot: installed account login smoke PASS; noVNC WebSocket-to-VNC TCP bridge code-level PASS; bridge disabled until explicit target host/port configuration
exact_command_mode: local xUnit/Pester tests plus installed admin account login smoke; service restart and temporary account/JWT file replacement were restored; no VNC server mutation, no firewall/trust-store/LAN/Event Log/Hyper-V mutation
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: account auth file schema, JWT login/session/RBAC contract, host listener WebSocket handling, noVNC target configuration, console capability/session route shape, or installed smoke runner redaction changes
waiver_status: installed account login smoke passed; noVNC bridge is opt-in and code-level verified; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: installed account login smoke runner and noVNC bridge

evidence_id: installed-account-login-novnc-bridge-code-level-record-20260510
route_or_operation: installed account login smoke runner and noVNC WebSocket bridge
route_surface: local-api-web-console-installed-service
risk_tier: tier2-code-level-listener-bridge-and-installed-runner
current_owner: desktop-node-api-host-packaging
commit_sha: codex/installed-account-novnc-evidence
artifact_or_package_version: docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md
target_owner: windows-desktop-node
implementation_basis: DesktopNodeConsoleOptions, DesktopNodeHostOptions noVNC target options, DesktopNodeHostApplication WebSocket bridge, Invoke-PcvInstalledAccountLoginSmoke.ps1
fallback_policy: keep noVNC disabled unless target host/port is explicitly configured; keep installed account login execution as administrator opt-in smoke
identity_requirement: installed account login smoke requires elevated operator and installed LocalSystem service; noVNC bridge accepts service bearer or account JWT with console.view
network_exposure_gate: noVNC target loopback-only unless --allow-lan is explicit; public release not claimed
runner_version: dotnet test focused API/Host suites, Pester smoke runner guard, installed account login smoke
host_capability_snapshot: noVNC bridge code-level websocket-to-vnc-tcp-pass; installed_account_login_smoke_execution=installed-admin-smoke-pass; runtime_auth_mode=account_rbac_jwt; token_value_observed=false; password_value_observed=false
exact_command_mode: code-level tests plus installed admin smoke with temporary account/JWT mutation, service restart, restore
result: installed-admin-smoke-pass-and-code-level-novnc-pass
observed_result: API account/console tests pass; Host options/listener tests pass; WebSocket frame proxied through loopback TCP echo target; smoke runner guard passes; installed smoke artifact `artifacts/installed-account-login-smoke-20260510-0410-final` recorded login/session/RBAC/console 200, restore_status=restored, acl_restore_status=restored
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: noVNC WebSocket path template, bridge auth policy, account login smoke route set, token redaction fields, or service account/JWT bootstrap behavior changes
waiver_status: installed account login smoke passed; public trusted signing and external stable publication not-claimed

## Evidence Group: Frontend/Backend Auth Console Live Smoke 2026-05-10 235543

evidence_id: frontend-backend-auth-console-live-smoke-2026-05-10-235543
artifact_or_package_version: docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md
runner_version: Invoke-PcvInstalledAccountLoginSmoke.ps1 with -RunBrowserQa plus installed Web Console QA
host_capability_snapshot: installed Web Console real account login form, auth/session/RBAC/console route contract, diagnostic create/download, responsive screenshots, installed web/app.js refresh PASS
exact_command_mode: installed web/app.js refresh, temporary account/JWT replacement, service restart/restore, browser QA against http://127.0.0.1/; no public trusted signing, external stable publication, winget submission, public URL, or public clean-host evidence
created_at: 2026-05-10T23:55:43+09:00
stale_triggers: auth route registry, ApiHandlerAdapterContract auth/console route list, Web Console account login/session UX, diagnostic bundle action UI, installed Web asset copy, browser QA redaction, or public release boundary changes
waiver_status: installed listener browser live smoke closed; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: installed auth console browser live smoke

evidence_id: frontend-backend-auth-console-live-smoke-record-20260510-235543
route_or_operation: installed Web Console account login, session/RBAC/console, diagnostic action, responsive browser QA
route_surface: installed-local-api-web-console-auth-console
risk_tier: tier3-installed-service-mutation
current_owner: desktop-node-api-host-web-console-packaging
commit_sha: d0d30ca00b37fd92269eb24cc7cb85b5f582d34e
artifact_or_package_version: artifacts/installed-account-login-browser-live-smoke-20260510-235543; artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543; artifacts/installed-web-asset-refresh-20260510-235258
target_owner: windows-desktop-node
implementation_basis: DesktopNode API auth/session/RBAC/console routes, ApiHandlerAdapterContract auth-console route contract, Web Console account login form/session state, installed listener browser QA runner
fallback_policy: service bearer token remains separate from account JWT; account auth is used only after local account/JWT bootstrap is prepared
identity_requirement: temporary operator account and JWT signing key are restored after smoke; token/password values are not recorded
network_exposure_gate: loopback Web Console http://127.0.0.1/ and API http://127.0.0.1:7777 only; public publication not claimed
runner_version: dotnet ApiHandlerAdapterContractTests, Web static Pester, installed account login smoke, installed listener browser QA
host_capability_snapshot: route_coverage_metadata=auth_logout-added; api_handler_adapter_contract=auth-console-routes-added; login/session/RBAC/console status 200/200/200/200; runtime_auth_mode=account_rbac_jwt; browser_qa_status=pass; screenshots=8; diagnostic_create_clicked=true; diagnostic_download_clicked=true; missing_button_labels=0; unlabeled_inputs=0
exact_command_mode: Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-browser-live-smoke-20260510-235543 -RunBrowserQa -BrowserQaUrl http://127.0.0.1/ -BrowserQaArtifactRoot artifacts/web-console-installed-listener-browser-live-smoke-20260510-235543
result: installed-listener-browser-live-smoke-pass
observed_result: installed web asset refresh changed app.js from SHA-256 065b724b1a5e75bc87a491c6c0ca0d349a35cb2b8a90eb90ab9563d5edecf9e4 to 53c2cd53248cb57d586c50092ead1791ced3089912005f4f525be0b4d8c82bc9; dashboard-wide screenshot SHA-256 7073e8b67d87f77987b7d776f8528e5a9e65d041240711a4f13b5cd4744e05de; dashboard-mobile screenshot SHA-256 da2d25577f7058116f4e410592e6bd59bacefd1090cc3b661ca588481c45f2fa; token_value_observed=false; password_value_observed=false
created_at: 2026-05-10T23:55:43+09:00
stale_triggers: auth logout route coverage, account JWT session bootstrap, RBAC state, console capabilities, Web diagnostic action controls, installed Web assets, screenshot/a11y QA, or redaction fields change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Web/API Port Split Installed Listener 2026-05-10

evidence_id: web-api-port-split-installed-listener-2026-05-10-0392
artifact_or_package_version: docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md; artifacts/installed-port-split-20260510-010714-0392; artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80
runner_version: installed DesktopNode.Host service-action stop/repair-installed plus HTTP smoke and headless Chrome CDP browser QA
host_capability_snapshot: installed service PathName includes Web Console prefix http://127.0.0.1:80/ and Web API prefix http://127.0.0.1:7777/; /pcv-config.js injects API origin; Web listener rejects /api/* with PCV_API_ROUTE_ON_WEB_PORT; browser QA against http://127.0.0.1/ passed
exact_command_mode: admin installed service stop, current payload copy, native repair-installed, service start, loopback HTTP verification, browser tab/filter/diagnostic/responsive QA; no MSI lifecycle, firewall, trust-store, LAN, Event Log, Hyper-V, HTTPS/443, public upload, winget submission, or clean-host execution
created_at: 2026-05-10T01:07:14+09:00
stale_triggers: listener prefix defaults, installed service binPath, Web static payload, protected token source, port 80 binding, CORS origin policy, TLS/443 binding policy, or public release claim changes
waiver_status: installed listener follow-up closed; HTTPS/443 not-run
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: installed Web/API listener port split

evidence_id: web-api-port-split-installed-listener-record-20260510-0392
route_or_operation: installed Web Console and Local API listener split
route_surface: installed-web-console-local-api
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-installed-service
commit_sha: codex/public-ops-transitions
artifact_or_package_version: artifacts/installed-port-split-20260510-010714-0392/installed-port-split-smoke.json; artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80/summary.json
target_owner: windows-desktop-node
implementation_basis: DesktopNode.Host --web-prefix, native service-action repair-installed, installed Web payload /pcv-config.js
fallback_policy: keep loopback-only Web/API split; HTTPS/443 remains separate TLS follow-up
identity_requirement: Local API route uses bearer token from protected token file; token value not recorded
network_exposure_gate: loopback-only Web 80 and API 7777; no LAN or firewall mutation executed
runner_version: dotnet publish payload, installed service stop/repair-installed, Invoke-WebRequest HTTP smoke, Chrome CDP installed listener QA
host_capability_snapshot: before_ports=127.0.0.1:7777; after_ports=127.0.0.1:80,127.0.0.1:7777; final_service=Running; host_mutation_performed=true; browser_qa_url=http://127.0.0.1/
exact_command_mode: service stop/copy/repair-installed/start; Web 200, /pcv-config.js 200, Web-port API 404 PCV_API_ROUTE_ON_WEB_PORT, API runtime policy 200 with bearer token, CORS preflight 204, browser diagnostics create/download and responsive screenshot capture
result: installed-listener-pass
observed_result: service binPath now includes --web-prefix "http://127.0.0.1:80/" and --api-token-protected-file; browser QA token_value_observed=false, diagnostic create/download true, missing button labels 0, unlabeled inputs 0; public trusted signing/external stable publication not claimed
created_at: 2026-05-10T01:07:14+09:00
stale_triggers: installed payload hash, service PathName, listener HTTP result, token storage policy, port binding, CORS policy, or public release boundary changes
waiver_status: HTTPS/443 not-run; public trusted signing and external stable publication not-claimed

## Evidence Group: Web/API Port Split Code-Level 2026-05-10

evidence_id: web-api-port-split-code-level-2026-05-10
artifact_or_package_version: docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md
runner_version: code-level host/product/web verification
host_capability_snapshot: Web Console prefix http://127.0.0.1:80/; Web API prefix http://127.0.0.1:7777/; /pcv-config.js API origin injection; Web listener /api/* rejected
exact_command_mode: local code-level tests and documentation update; no installed service reconfiguration, port 80 URL reservation smoke, HTTPS/443 binding, service restart, MSI install/repair, or host mutation
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: listener prefix defaults, service binPath generation, static config script contract, CORS origin policy, Web/API route ownership, port 80 binding policy, TLS/443 binding policy, or installed listener evidence changes
waiver_status: code-level applied; installed listener follow-up closed by web-api-port-split-installed-listener-2026-05-10-0392
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Web/API listener port split

evidence_id: web-api-port-split-code-level-record-20260510
route_or_operation: Web Console and Local API listener split
route_surface: web-console-local-api
risk_tier: tier1-code-level-listener-config
current_owner: desktop-node-host-and-product-wrapper
commit_sha: codex/public-ops-transitions
artifact_or_package_version: docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md
target_owner: windows-desktop-node
implementation_basis: DesktopNode.Host --web-prefix, product wrapper web_prefix default, served /pcv-config.js, Web static client API base resolution
fallback_policy: browser client falls back to current Web origin only when listener config is absent
identity_requirement: bearer token remains required on Local API routes
network_exposure_gate: loopback-only code-level default; no LAN or HTTPS binding executed
runner_version: dotnet/xUnit, Pester, npm TypeScript/static parity
host_capability_snapshot: web_console_url=http://127.0.0.1/, api_route_prefix=http://127.0.0.1:7777/api/v1/..., web_api_same_port=false, installed_listener_execution=installed-listener-pass, host_mutation_performed=true for installed service repair smoke
exact_command_mode: code-level tests; no service restart, MSI repair/install, URL ACL mutation, firewall mutation, trust-store mutation, or TLS binding
result: code-level-pass-installed-listener-closed
observed_result: Web listener serves static/config, API listener owns /api/v1, Web listener rejects /api/* with PCV_API_ROUTE_ON_WEB_PORT, CORS allows configured Web origin, installed listener artifact `artifacts/installed-port-split-20260510-010714-0392` passed, HTTPS/443 remains not-run
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: port default, listener route ownership, config script, CORS, service plan, URL reservation, or installed listener evidence changes
waiver_status: HTTPS/443 not-run; public trusted signing and external stable publication not-claimed

## Evidence Group: Internal Private Network Boundary 2026-05-10

evidence_id: internal-private-network-boundary-2026-05-10
artifact_or_package_version: docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md; docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md
runner_version: docs-boundary-reclassification
host_capability_snapshot: ADR-0005 public distribution candidate closed; ADR-0006 internal private network distribution adopted; public signing/winget/external upload/public clean-host smoke out-of-scope; internal HTTPS/TLS installed lifecycle PASS; internal clean-host install/update/rollback PASS
exact_command_mode: docs-only boundary reclassification; no host mutation
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: product distribution scope, public release claim, internal catalog/channel policy, HTTPS/TLS installed lifecycle status, clean-host runner availability, or internal signed MSI evidence changes
waiver_status: public trusted signing, trusted timestamp, external stable publication, winget submission, and clean-host public signed smoke out-of-scope
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
winget_submission: out-of-scope
internal_distribution_matrix: docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md

### Record: internal private network boundary decision

evidence_id: internal-private-network-boundary-record-20260510
route_or_operation: distribution boundary decision
route_surface: internal-private-network-distribution
risk_tier: tier0-docs-boundary
current_owner: docs-adr-index
commit_sha: 092c781
artifact_or_package_version: docs/adr/0006-internal-private-network-distribution.md
target_owner: windows-desktop-node
implementation_basis: ADR-0006 and internal distribution matrix
fallback_policy: retain ADR-0005 public candidate evidence as historical closed-not-adopted record
identity_requirement: not applicable
network_exposure_gate: private LAN smoke remains internal gate; public stable publication out-of-scope
runner_version: PcvAdminSmokeEvidenceDocs.Tests.ps1 documentation guard
host_capability_snapshot: internal_signed_msi_status=pass, internal_updater_catalog_channel=code-level-pass, private_lan_smoke=pass, internal_https_tls_lifecycle_installed_smoke=pass, internal_clean_host_install_update_rollback_smoke=pass
exact_command_mode: documentation guard plus git diff --check
result: docs-boundary-reclassification
observed_result: public distribution candidate closed-not-adopted; internal private network matrix added
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: ADR-0006 state, matrix classification, public out-of-scope classification, or internal smoke status changes
waiver_status: host mutation not performed by this docs evidence

## Evidence Group: Internal HTTPS/TLS Lifecycle Installed 2026-05-10

evidence_id: internal-https-tls-lifecycle-installed-2026-05-10-0397
artifact_or_package_version: docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md; artifacts/internal-https-tls-lifecycle-installed-20260510-0397
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1
host_capability_snapshot: installed service HTTPS binding, certificate generate/bind/rotate/remove, original HTTP service restore PASS
exact_command_mode: temporary LocalMachine self-signed certs, HTTP.sys 127.0.0.1:7443 SSL binding, service PathName prefix mutation and restore, bearer runtime policy over HTTPS
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: service listener prefix handling, HTTP.sys SSL binding policy, certificate lifecycle policy, token resolver, service restore behavior, or internal HTTPS exposure changes
waiver_status: internal private-network smoke; public trusted signing, external stable publication, winget submission, public clean-host smoke out-of-scope
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope

### Record: internal HTTPS/TLS lifecycle installed smoke

evidence_id: internal-https-tls-lifecycle-installed-record-20260510-0397
route_or_operation: internal HTTPS/TLS lifecycle installed smoke
route_surface: internal-private-network-distribution
risk_tier: tier3-installed-service-host-mutation
current_owner: windows-desktop-node-tools
commit_sha: dff6456
artifact_or_package_version: artifacts/internal-https-tls-lifecycle-installed-20260510-0397/summary.json
target_owner: windows-desktop-node
implementation_basis: Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1
fallback_policy: always restore original service PathName, remove SSL binding, remove temporary certificates, and verify restored HTTP runtime policy
identity_requirement: elevated shell; installed service runs as LocalSystem
network_exposure_gate: loopback-only HTTPS 127.0.0.1:7443 temporary binding
runner_version: Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1
host_capability_snapshot: certificate_lifecycle=generate-bind-rotate-remove-pass, https_initial_status=200, https_rotated_status=200, final_http_restore_status=200, final_service=Running, path_name_restored=true, token_value_observed=false
exact_command_mode: pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1 -ArtifactRoot artifacts/internal-https-tls-lifecycle-installed-20260510-0397
result: pass
observed_result: HTTPS runtime policy returned 200 before and after certificate rotation; original HTTP listener restored and returned 200; HTTP.sys SSL binding and temporary certs removed
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: TLS binding, certificate store, service prefix, or cleanup behavior changes
waiver_status: no public release claim

## Evidence Group: Internal Clean-Host Install Update Rollback Readiness 2026-05-10

evidence_id: internal-clean-host-install-update-rollback-readiness-2026-05-10
artifact_or_package_version: docs/ga-ready/evidence/internal-clean-host-install-update-rollback-readiness-2026-05-10.md; artifacts/internal-clean-host-install-update-rollback-readiness-20260510
runner_version: packaging/windows-desktop-node/tools/New-PcvInternalCleanHostInstallUpdateRollbackReadiness.ps1
host_capability_snapshot: historical internal clean-host readiness snapshot; runner was unavailable at that time and install/update/rollback was not executed; superseded by internal-clean-host-install-update-rollback-smoke-2026-05-10-0417 PASS
exact_command_mode: local prerequisite scan only; no host mutation
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: clean-host runner availability, internal catalog path, target MSI/update package, baseline/target versions, or public release claim changes
waiver_status: blocked by missing clean-host runner
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope

### Record: internal clean-host install/update/rollback readiness

evidence_id: internal-clean-host-install-update-rollback-readiness-record-20260510
route_or_operation: internal clean-host install/update/rollback readiness
route_surface: internal-private-network-distribution
risk_tier: tier1-readiness-scan
current_owner: windows-desktop-node-tools
commit_sha: 092c781
artifact_or_package_version: artifacts/internal-clean-host-install-update-rollback-readiness-20260510/summary.json
target_owner: windows-desktop-node
implementation_basis: New-PcvInternalCleanHostInstallUpdateRollbackReadiness.ps1
fallback_policy: do not substitute current-host update/rollback smoke for clean-host evidence
identity_requirement: clean Windows host or VM runner required
network_exposure_gate: internal catalog/channel only; public upload and winget out-of-scope
runner_version: New-PcvInternalCleanHostInstallUpdateRollbackReadiness.ps1
host_capability_snapshot: internal_clean_host_install_update_rollback_smoke=blocked-by-missing-clean-host-runner, clean_host_runner_present=false, host_mutation_performed=false
exact_command_mode: pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvInternalCleanHostInstallUpdateRollbackReadiness.ps1 -ArtifactRoot artifacts/internal-clean-host-install-update-rollback-readiness-20260510
result: blocked-by-missing-clean-host-runner
observed_result: readiness descriptor written; no clean-host install/update/rollback execution
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: clean-host runner, internal catalog, installer payload, or update/rollback policy changes
waiver_status: no public release claim

## Evidence Group: Internal Clean-Host Install Update Rollback Smoke 2026-05-10

evidence_id: internal-clean-host-install-update-rollback-smoke-2026-05-10-0417
artifact_or_package_version: docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md; artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417; artifacts/internal-clean-host-packages-20260510-0414
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1
host_capability_snapshot: dedicated Hyper-V clean-host VM install/update/rollback PASS; baseline=0.39.6-admin-smoke; target=0.39.7-admin-smoke; final_service=Running; final_web_status_code=200; failed_root_manifest=0.39.7-admin-smoke; token_value_observed=false
exact_command_mode: dedicated Windows Server 2022 Eval VHD on Default Switch; install KB5082142 cumulative update; import internal root certificate; install internal signed MSI; update via guest-local internal file catalog; rollback to baseline; remove VM on success
created_at: 2026-05-10T04:17:00+09:00
stale_triggers: clean-host base image, Windows Update requirement, internal root/leaf signing policy, updater catalog schema, MSI/update package hashes, rollback diagnostics policy, or public release claim changes
waiver_status: internal private-network smoke; public trusted signing, trusted timestamp, external stable publication, winget submission, and public clean-host smoke out-of-scope
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
winget_submission: out-of-scope

### Record: internal clean-host install/update/rollback smoke

evidence_id: internal-clean-host-install-update-rollback-smoke-record-20260510-0417
route_or_operation: internal clean-host install/update/rollback smoke
route_surface: internal-private-network-distribution
risk_tier: tier4-dedicated-clean-host-hyper-v-mutation
current_owner: windows-desktop-node-tools
commit_sha: 092c781
artifact_or_package_version: artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417/summary.json
target_owner: windows-desktop-node
implementation_basis: Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1
fallback_policy: do not substitute current-host update/rollback smoke for clean-host evidence; require dedicated VM or clean host
identity_requirement: elevated Hyper-V host; guest Administrator via PowerShell Direct; installed service runs as LocalSystem
network_exposure_gate: internal file catalog only; public upload, winget, and public installer URL out-of-scope
runner_version: Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1
host_capability_snapshot: internal_clean_host_install_update_rollback_smoke=pass, install_exit_code=0, update_exit_code=0, rollback_exit_code=0, baseline_manifest_version=0.39.6-admin-smoke, updated_manifest_version=0.39.7-admin-smoke, final_manifest_version=0.39.6-admin-smoke, final_service_state=Running, final_web_status_code=200, failed_root_exists_after_rollback=true
exact_command_mode: pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1 -ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417 -BaseVhdPath D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\image-cache\windows-server-2022-eval-vhd\20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd -BaselineMsiPath artifacts/internal-clean-host-packages-20260510-0414/baseline/PureCVisorDesktopNode-0.39.6-admin-smoke-windows-x64.msi -TargetUpdatePackagePath artifacts/internal-clean-host-packages-20260510-0414/target/PureCVisorDesktopNode-0.39.7-admin-smoke-update.zip -InternalRootCertificatePath artifacts/internal-clean-host-packages-20260510-0414/PureCVisorInternalCodeSigningRoot.cer -VmName pcv-cleanhost-0417 -VmSwitchName "Default Switch" -InstallWindowsUpdates -RemoveVmOnSuccess
result: pass
observed_result: baseline MSI signature became Valid after internal root import; install 0.39.6-admin-smoke exit 0; internal catalog update to 0.39.7-admin-smoke exit 0; rollback exit 0 restored 0.39.6-admin-smoke; service Running and Web Console HTTP 200
created_at: 2026-05-10T04:17:00+09:00
stale_triggers: base VHD, OS cumulative update policy, internal root certificate, MSI/update package hash, update catalog, rollback behavior, or clean-host runner changes
waiver_status: no public release claim

## Evidence Group: Windows Credential Manager Default Transition Installed 2026-05-10

evidence_id: windows-credential-manager-default-transition-installed-2026-05-10-0395
artifact_or_package_version: docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md; artifacts/windows-credential-manager-default-transition-installed-20260510-0395
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1
host_capability_snapshot: installed MSI deferred LocalSystem custom action pass; service token source migrated to Windows Credential Manager; rollback diagnostics written
exact_command_mode: AllowUnsignedDev MSI build and install; `credential-manager-default-transition` custom action with `Impersonate=no`; final runtime policy read over loopback
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: Credential Manager target policy, service token source arguments, MSI ProductActions sequence, rollback diagnostics schema, token resolver storage metadata, or public release claim changes
waiver_status: internal admin-smoke only; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Windows Credential Manager default transition installed smoke

evidence_id: windows-credential-manager-default-transition-installed-record-20260510-0395
route_or_operation: Windows Credential Manager service default token-source transition
route_surface: product-operation
risk_tier: tier2-installed-service-credential-mutation
current_owner: desktop-node-host-service-action
commit_sha: 039e24086292e394c0061593c91e5768fb810450
artifact_or_package_version: artifacts/windows-credential-manager-default-transition-installed-20260510-0395/summary.json
target_owner: windows-desktop-node
implementation_basis: `DesktopNode.Host.exe service-action credential-manager-default-transition` plus MSI deferred `CredentialManagerDefaultTransition` custom action
fallback_policy: retain DPAPI protected token file as rollback fallback while service default reads Windows Credential Manager target
identity_requirement: `NT AUTHORITY\SYSTEM` installed custom action proof pass
network_exposure_gate: loopback runtime policy health only; no LAN/firewall/trust-store/public upload mutation
runner_version: Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1
host_capability_snapshot: credential_manager_transition=installed-local-system-default-transition-pass, service_credential_manager_default_transition=installed-admin-smoke-pass, token_source_migration=protected-file-to-credential-manager, service_reload_status=restarted, old_source_rejection_status=protected-file-source-rejected-after-reload, rollback_diagnostics_status=written, token_value_observed=false
exact_command_mode: `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1 -ArtifactRoot artifacts/windows-credential-manager-default-transition-installed-20260510-0395 -Version 0.39.5-admin-smoke`
result: pass
observed_result: MSI exit 0, identity `NT AUTHORITY\SYSTEM`, final service `Running` as `LocalSystem`, final SCM PathName uses `--api-token-credential-target` and not `--api-token-protected-file`, runtime policy returned HTTP 200 with `token_storage=windows-credential-manager`
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: credential target, token source migration behavior, service reload policy, old source rejection, rollback diagnostics, or runtime policy token storage changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Public Ops Installed Hardening Code-Level 2026-05-09

evidence_id: public-ops-installed-hardening-code-level-2026-05-09-0393
artifact_or_package_version: docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md
runner_version: DesktopNode.Host.exe native service-action code path
host_capability_snapshot: Credential Manager SYSTEM proof runner and Event Log repair/write/volume guard actions code-level pass; external upload/winget/public signing absent; later installed SYSTEM proof, Event Log default writer, internal HTTPS, and internal clean-host PASS recorded in later evidence groups
exact_command_mode: code-level xUnit host tests; no public upload, winget submission, clean-host smoke, installed host mutation, TLS binding, trust-store mutation, or LAN binding
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: Credential Manager service token source migration, SYSTEM-context installed proof, Event Log default writer, Event Log repair/remove installed smoke, volume guard policy, HTTPS binding, trust boundary, external publication, or public release claim changes
waiver_status: partial code-level hardening; public release not claimed
public_trusted_signing: not-claimed
external_stable_publication: blocked-by-missing-upload-endpoint-and-credential

### Record: public ops installed hardening code-level

evidence_id: public-ops-installed-hardening-code-level-record-20260509-0393
route_or_operation: Credential Manager SYSTEM proof runner and Event Log provider hardening service actions
route_surface: public-distribution-ops
risk_tier: tier1-code-level-native-service-action
current_owner: desktop-node-host-service-action
commit_sha: codex/public-ops-installed-hardening
artifact_or_package_version: docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md
target_owner: windows-desktop-node
implementation_basis: native `credential-manager-system-proof`, `eventlog-repair`, `eventlog-write-test`, and `eventlog-volume-guard` service actions
fallback_policy: keep DPAPI protected token file default and JSONL-first writer until installed transition evidence exists
identity_requirement: installed SYSTEM execution pending
network_exposure_gate: no upload, submission, public installer fetch, HTTPS binding, or LAN binding executed
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj
host_capability_snapshot: credential_manager_system_context_proof_runner=code-level-native-service-action, service_credential_manager_default_transition=system-proof-runner-code-level-applied-service-default-transition-pending, event_log_hardening=partial-code-level-repair-write-volume-guard-default-writer-pending, tls_binding=not-run
exact_command_mode: code-level tests only; host_mutation_performed=false
result: partial-code-level-pass-with-external-and-installed-smoke-blockers
observed_result: Host tests passed; external stable publication/catalog upload, winget submission, and clean-host public signed install/update/rollback remain out-of-scope/blocked for public release; installed SYSTEM Credential Manager proof, default Event Log writer, internal HTTPS binding, and trust boundary evidence are superseded by later internal PASS evidence
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: any installed hardening state, blocker, next evidence contract, TLS lifecycle status, Event Log hardening status, or public release claim changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Public Ops Gate Execution Readiness 2026-05-09

evidence_id: public-ops-gate-execution-readiness-2026-05-09-0392
artifact_or_package_version: docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md; artifacts/public-ops-gate-execution-readiness-20260509-0392
runner_version: packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1
host_capability_snapshot: historical local execution-readiness descriptor; TLS certificate generation/rotation/delete code-level slice pass; public external upload/winget/public clean-host inputs absent; later internal SYSTEM credential proof, Event Log hardening, HTTPS, and clean-host PASS recorded in later evidence groups
exact_command_mode: local descriptor write with `-AllowLocalEvidenceWrite -RunLocalTlsLifecycle`; no public upload, winget submission, clean-host smoke, host mutation, TLS binding, trust-store mutation, or LAN binding
created_at: 2026-05-09T00:00:00+09:00
regenerated_at: 2026-05-10T00:00:00+09:00
regeneration_snapshot: ok=true, host_mutation_performed=false, public_release=not-claimed, tls_certificate_lifecycle=partial-code-level-cert-generate-rotate-delete-pass
stale_triggers: upload endpoint/credential, public installer URL, winget submission token, clean-host runner, SYSTEM credential proof, TLS binding/trust-store policy, Event Log default writer/repair/remove/volume guard, or public release claim changes
waiver_status: partial code-level readiness; public release not claimed
public_trusted_signing: not-claimed
external_stable_publication: blocked-by-missing-upload-endpoint-and-credential

### Record: public ops gate execution readiness

evidence_id: public-ops-gate-execution-readiness-record-20260509-0392
route_or_operation: six remaining public operations gate execution-readiness descriptor
route_surface: public-distribution-ops
risk_tier: tier0-non-mutating
current_owner: packaging-windows-desktop-node
commit_sha: codex/public-ops-gates-implementation
artifact_or_package_version: artifacts/public-ops-gate-execution-readiness-20260509-0392/summary.json; artifacts/public-ops-gate-execution-readiness-20260509-0392/gates.json; artifacts/public-ops-gate-execution-readiness-20260509-0392/tls-certificate-lifecycle.json
target_owner: windows-desktop-node
implementation_basis: local evidence descriptor and code-level ephemeral TLS certificate generation/rotation/delete
fallback_policy: keep ADR-0004 internal-only-service boundary
identity_requirement: public release credentials absent; later internal SYSTEM proof exists in installed evidence
network_exposure_gate: no public upload, submission, or public installer fetch executed
runner_version: New-PcvPublicOpsGateExecutionReadiness.ps1
host_capability_snapshot: actual_execution=local-execution-readiness-descriptor-written, host_mutation_performed=false, public_release=not-claimed, tls_certificate_lifecycle=partial-code-level-cert-generate-rotate-delete-pass, tls_private_key_material_written=false, tls_binding=not-run
exact_command_mode: local readiness descriptor; no public submission, clean-host execution, service mutation, trust-store mutation, or host mutation
result: partial-code-level-with-external-blockers
observed_result: external stable publication/catalog upload, winget submission, and clean-host public signed install/update/rollback remain blocked/out-of-scope; SYSTEM-context Credential Manager proof, internal HTTPS/TLS, Event Log hardening, and internal clean-host evidence are superseded by later internal PASS evidence
created_at: 2026-05-09T00:00:00+09:00
regenerated_at: 2026-05-10T00:00:00+09:00
regeneration_snapshot: ok=true, host_mutation_performed=false, public_release=not-claimed, public_trusted_signing=not-claimed, tls_private_key_material_written=false, tls_binding=not-run
stale_triggers: any six-gate state, blocker, next evidence contract, TLS lifecycle status, Event Log hardening status, or public release claim changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Public Ops Final Follow-up Attempt 2026-05-09

evidence_id: public-ops-final-followup-attempt-2026-05-09-0391
artifact_or_package_version: docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md; artifacts/public-ops-final-followup-attempt-20260509-0391
runner_version: packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1
host_capability_snapshot: historical local prerequisite scan only; public signing/timestamp/upload/public installer/public clean-host inputs absent; later internal SYSTEM credential, TLS, Event Log, and clean-host PASS recorded in later evidence groups
exact_command_mode: local descriptor write with `-AllowLocalEvidenceWrite`; no signing, upload, winget submission, clean-host smoke, TLS binding, service Credential Manager mutation, or host mutation
created_at: 2026-05-09T00:00:00+09:00
regenerated_at: 2026-05-10T00:00:00+09:00
regeneration_snapshot: ok=true, remaining_follow_up_count=7, host_mutation_performed=false, public_release=not-claimed
stale_triggers: public signing material, timestamp URL, publication endpoint/credential, public installer URL, winget submission path, public clean-host target, or ADR-0005 status changes
waiver_status: 1-7 final public operations follow-up prerequisite scan; public release not claimed
public_trusted_signing: blocked-by-missing-public-signing-material
external_stable_publication: blocked-by-missing-upload-endpoint-and-credentials

### Record: public ops final follow-up attempt

evidence_id: public-ops-final-followup-attempt-record-20260509-0391
route_or_operation: 1-7 final public operations follow-up prerequisite scan
route_surface: public-distribution-ops
risk_tier: tier0-non-mutating
current_owner: packaging-windows-desktop-node
commit_sha: codex/public-ops-final-seven
artifact_or_package_version: artifacts/public-ops-final-followup-attempt-20260509-0391/summary.json; artifacts/public-ops-final-followup-attempt-20260509-0391/remaining-follow-up-items.json
target_owner: windows-desktop-node
implementation_basis: local evidence descriptor for remaining follow-up states
fallback_policy: keep ADR-0004 internal-only-service boundary
identity_requirement: release credentials absent
network_exposure_gate: no upload or submission executed
runner_version: New-PcvPublicOpsFinalFollowupAttempt.ps1
host_capability_snapshot: remaining_follow_up_count=7, actual_execution=local-final-followup-prerequisite-scan-executed, host_mutation_performed=false, public_release=not-claimed
exact_command_mode: local scan; no public signing, upload, winget submission, clean-host execution, TLS binding, or service Credential Manager mutation
result: blocked-with-next-evidence
observed_result: public trusted signing/timestamp, external stable publication/catalog upload, winget submission, and clean-host public signed install/update/rollback remain blocked/out-of-scope; Windows Credential Manager service default transition, built-in TLS lifecycle, Event Log provider hardening, and internal clean-host smoke are superseded by later internal PASS evidence
created_at: 2026-05-09T00:00:00+09:00
regenerated_at: 2026-05-10T00:00:00+09:00
regeneration_snapshot: ok=true, remaining_follow_up_count=7, mutates_host=false, host_mutation_performed=false, public_release=not-claimed
stale_triggers: any 1-7 follow-up state, blocker, next evidence contract, or public release claim changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Burn Bootstrapper Lifecycle Smoke 2026-05-10

evidence_id: burn-bootstrapper-lifecycle-smoke-2026-05-10-0416
artifact_or_package_version: docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md; artifacts/burn-bootstrapper-lifecycle-20260510-0416
runner_version: WiX CLI 5.0.2 Burn bootstrapper; msiexec restore
host_capability_snapshot: WiX Burn bundle build plus install/repair/remove lifecycle executed; direct MSI restore completed; final PureCVisorDesktopNode service Running
exact_command_mode: Burn `/install`, `/repair`, `/uninstall` with quiet norestart, followed by direct MSI restore
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: Burn authoring, MSI chain payload, installer lifecycle contract, service restore contract, or public signing claim changes
waiver_status: internal AllowUnsignedDev smoke; public trusted signing, timestamp evidence, external stable publication, winget submission, and clean-host public signed update/rollback not claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Burn bootstrapper lifecycle smoke

evidence_id: burn-bootstrapper-lifecycle-smoke-pass-20260510-0416
route_or_operation: Burn bootstrapper build/install/repair/remove lifecycle
route_surface: packaging-distribution
risk_tier: tier3-host-mutation
current_owner: packaging-windows-desktop-node
commit_sha: codex/public-ops-actual
artifact_or_package_version: artifacts/burn-bootstrapper-lifecycle-20260510-0416/summary.json
target_owner: windows-desktop-node
implementation_basis: WiX Burn chain over 0.41.6-admin-smoke MSI payload
fallback_policy: direct MSI restore after Burn remove
identity_requirement: elevated administrator
network_exposure_gate: none
runner_version: wix.exe; msiexec.exe
host_capability_snapshot: summary ok=true, actual_execution=burn-build-install-repair-remove-executed, host_mutation_performed=true, final service Running
exact_command_mode: Burn install/repair/remove smoke; no public upload or winget submit
result: pass
observed_result: bundle SHA-256 5e67bd3a1fed7262447531000328825180fd678b252170793cf88e50fc41535d; install, repair, remove, and restore exit code 0
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: Burn authoring, MSI payload, installer restore path, or public signing boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Windows Event Log Provider Default Transition 2026-05-09

evidence_id: windows-event-log-provider-default-transition-2026-05-09-0391
artifact_or_package_version: docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md; artifacts/windows-event-log-provider-default-transition-20260509-0391
runner_version: DesktopNode.Host.exe service-action eventlog-register; EventLog write/query
host_capability_snapshot: installed provider registration completed; Application log event id 39100 write/query passed; provider final state present; service Running
exact_command_mode: corrected native eventlog-register command with installed product root and service exe
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: Event Log source name, provider registration path, default writer policy, repair/remove policy, or log volume guard changes
waiver_status: internal installed host mutation; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Windows Event Log provider default transition

evidence_id: windows-event-log-provider-default-transition-pass-20260509-0391
route_or_operation: Windows Event Log provider registration and write/query
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-service-action
commit_sha: codex/public-ops-actual
artifact_or_package_version: artifacts/windows-event-log-provider-default-transition-20260509-0391/summary.json
target_owner: windows-desktop-node
implementation_basis: native service-action eventlog-register and EventLog write/query smoke
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: none
runner_version: DesktopNode.Host.exe
host_capability_snapshot: event_log_provider_transition=installed-provider-register-write-pass, event_log_provider_mutation=registered, event_log_write_status=write-query-pass
exact_command_mode: host mutation provider registration and Application log write/query
result: pass
observed_result: source PureCVisor Desktop Node, EventMessageFile C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe, event id 39100, provider final state present
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: provider registration, default writer policy, repair/remove behavior, or EventMessageFile path changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Windows Event Log Default Transition Installed 2026-05-10

evidence_id: windows-event-log-default-transition-installed-2026-05-10-0396
artifact_or_package_version: docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md; artifacts/windows-event-log-default-transition-installed-20260510-0396
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1
host_capability_snapshot: installed MSI deferred LocalSystem custom action completed provider repair, schema v1 event id 39101 write/query, volume guard, provider remove/restore, final provider present, service Event Log writer args present
exact_command_mode: `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1 -ArtifactRoot artifacts/windows-event-log-default-transition-installed-20260510-0396 -Version 0.39.6-admin-smoke`
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: MSI custom action sequencing, eventlog-default-transition action, Event Log source name, event schema, EventMessageFile path, service Event Log writer args, or Application log volume guard policy changes
waiver_status: internal installed admin-smoke; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Windows Event Log default transition installed smoke

evidence_id: windows-event-log-default-transition-installed-pass-20260510-0396
route_or_operation: Windows Event Log default writer/provider hardening
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: desktop-node-host-service-action
commit_sha: 8c661b864ab64b5df1596625a58b0bd9583f477f
artifact_or_package_version: artifacts/windows-event-log-default-transition-installed-20260510-0396/summary.json
target_owner: windows-desktop-node
implementation_basis: MSI deferred LocalSystem EventLogDefaultTransition custom action and native service-action eventlog-default-transition
fallback_policy: none
identity_requirement: elevated administrator/MSI deferred LocalSystem
network_exposure_gate: none
runner_version: Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1
host_capability_snapshot: event_log_default_transition=installed-admin-smoke-pass, event_log_hardening=installed-default-writer-repair-remove-volume-schema-pass, event_log_default_writer=installed-admin-smoke-pass, event_log_schema_version=1
exact_command_mode: build current AllowUnsignedDev MSI, install through msiexec, copy eventlog-default-transition.json, query Application event id 39101, verify service PathName Event Log writer args
result: pass
observed_result: MSI SHA-256 180e3a6185bfcc47681f1e7a62afae8998efd05a7334df3f7b1dbf98f6f052fe; provider repair/write/volume/remove/restore passed; final service Running; runtime policy HTTP 200; token_value_observed=false
created_at: 2026-05-10T00:00:00+09:00
stale_triggers: provider registration, default writer policy, repair/remove behavior, EventMessageFile path, service binary path, or event schema changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Windows Credential Manager Transition 2026-05-09

evidence_id: windows-credential-manager-transition-2026-05-09-0391
artifact_or_package_version: docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md; artifacts/windows-credential-manager-transition-20260509-0391
runner_version: Advapi32 CredWriteW/CredReadW/CredDeleteW current-user capability smoke
host_capability_snapshot: current elevated user credential write/read/delete passed; LocalSystem service default transition is superseded by `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`
exact_command_mode: temporary Generic credential write/read/delete, token digest only
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: service account, credential target option, SYSTEM-context migration runner, token storage policy, or rollback diagnostics changes
waiver_status: historical partial record; superseded by installed LocalSystem default transition; token value not observed; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Windows Credential Manager transition capability smoke

evidence_id: windows-credential-manager-transition-partial-20260509-0391
route_or_operation: Windows Credential Manager write/read/delete capability
route_surface: product-operation
risk_tier: tier2-local-credential-capability
current_owner: desktop-node-host-service-action
commit_sha: codex/public-ops-actual
artifact_or_package_version: artifacts/windows-credential-manager-transition-20260509-0391/summary.json
target_owner: windows-desktop-node
implementation_basis: current-user Credential Manager capability smoke plus service account blocker
fallback_policy: keep DPAPI protected token file default
identity_requirement: elevated user; future service proof requires LocalSystem context
network_exposure_gate: none
runner_version: PowerShell Add-Type Advapi32 P/Invoke
host_capability_snapshot: credential_manager_transition=capability-pass-service-transition-blocked, credential_manager_mutation=current-user-smoke-write-read-delete, service_account=LocalSystem, token_value_observed=false
exact_command_mode: current-user credential write/read/delete only; no service reload or product token migration
result: partial-blocked
observed_result: CredWrite, CredRead, and CredDelete passed; service default transition blocked by service account context
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: service identity, credential target support, SYSTEM migration runner, or token source default changes
waiver_status: historical blocker superseded by `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`

## Evidence Group: Public External Gates Blocked Scan 2026-05-09

evidence_id: public-external-gates-blocked-2026-05-09-0391
artifact_or_package_version: docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md; artifacts/public-external-gates-blocked-20260509-0391
runner_version: local prerequisite scan for SignTool, winget, gh, and release credential environment
host_capability_snapshot: local tools present, but public signing material, timestamp URL, upload endpoint/credential, public stable installer URL, and public clean-host publication input absent; internal clean-host PASS recorded later under ADR-0006
exact_command_mode: local prerequisite scan only
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: public signing provider, timestamp URL, upload endpoint/credential, winget submission path, public installer URL, or public clean-host publication input changes
waiver_status: public trusted signing, timestamp evidence, external stable publication, catalog upload, winget submission, and clean-host public signed smoke blocked
public_trusted_signing: blocked-by-missing-public-signing-material
external_stable_publication: blocked-by-missing-upload-endpoint-and-credentials

### Record: public external gates blocked scan

evidence_id: public-external-gates-blocked-scan-20260509-0391
route_or_operation: public external release prerequisite scan
route_surface: public-distribution-ops
risk_tier: tier0-non-mutating
current_owner: packaging-windows-desktop-node
commit_sha: codex/public-ops-actual
artifact_or_package_version: artifacts/public-external-gates-blocked-20260509-0391/summary.json
target_owner: windows-desktop-node
implementation_basis: environment/tool prerequisite scan
fallback_policy: keep ADR-0004 internal-only-service boundary
identity_requirement: release credentials absent
network_exposure_gate: no upload or submission executed
runner_version: PowerShell prerequisite scan
host_capability_snapshot: signtool_x64_found=true, winget_cli_present=true, github_cli_authenticated=true, timestamp_evidence blocked, catalog_publication not-uploaded, winget_submission blocked, clean-host smoke blocked
exact_command_mode: local scan; no public signing, upload, winget submit, or clean-host execution
result: blocked
observed_result: required public release credentials and endpoints absent
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: public signing material, timestamp URL, upload endpoint/credential, public installer URL, winget submission token, or clean host target availability
waiver_status: external/public gates remain blocked

## Evidence Group: Web Console Beta Follow-up Status 2026-05-09

evidence_id: web-console-beta-followup-status-2026-05-09
artifact_or_package_version: docs/ga-ready/evidence/web-console-beta-followup-status-2026-05-09.md
runner_version: web/tests/PcvDesktopWeb.Static.Tests.ps1; web/scripts/verify-browser-fixture.mjs
host_capability_snapshot: Web Console troubleshooting view includes beta follow-up status surface for installed listener QA automation, service token revoke handoff, diagnostic retention pagination, guarded VM delete, ops cockpit P0/P1/P2, public distribution bundle, and browser host boundary
exact_command_mode: static Web Console tests and browser fixture verification only
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: Web Console troubleshooting DOM, served app renderer, browser fixture contract, beta follow-up copy, or host mutation boundary changes
waiver_status: host mutation not executed; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: Web Console beta follow-up status surface

evidence_id: web-console-beta-followup-status-pass-20260509
route_or_operation: Web Console beta follow-up status surface
route_surface: web-console
risk_tier: tier1-read-only
current_owner: typescript-web-console
commit_sha: codex/public-ops-beta-followups
artifact_or_package_version: docs/ga-ready/evidence/web-console-beta-followup-status-2026-05-09.md
target_owner: windows-desktop-node-web-console
implementation_basis: troubleshooting beta status panel, static Pester guard, generated app.js, browser fixture guard
fallback_policy: none
identity_requirement: none
network_exposure_gate: none
runner_version: PcvDesktopWeb.Static.Tests.ps1; verify-browser-fixture.mjs
host_capability_snapshot: browser panel renders follow-up state and explicitly states host mutation is not started from browser
exact_command_mode: Invoke-Pester web static, npm test, npm run verify:parity
result: pass
observed_result: installed listener QA automation, service token revoke handoff, diagnostic retention pagination, VM delete guarded, ops cockpit P0/P1/P2, public distribution bundle, and host mutation boundary strings are present without token values or host mutation command strings
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: beta panel renderer, diagnostic/token/VM delete UI, public distribution bundle wording, or Web fixture changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Public Distribution Ops Execution Bundle 2026-05-09

evidence_id: public-distribution-ops-execution-bundle-2026-05-09
artifact_or_package_version: docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md; artifacts/public-distribution-ops-execution-bundle-20260509-0391
runner_version: packaging/windows-desktop-node/tools/New-PcvPublicDistributionOperationsBundle.ps1
host_capability_snapshot: local non-mutating ADR-0005 public distribution and operations preflight bundle executed; 13 component summaries collected; preserved legacy follow-up branches recorded; no host mutation, public signing, external publication, winget submission, catalog upload, clean-host public smoke, credential, event log, TLS, or service token mutation executed
exact_command_mode: New-PcvPublicDistributionOperationsBundle.ps1 -AllowLocalDescriptorWrite with HTTPS placeholder publication inputs and preserved branch list
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: ADR-0005 matrix state, public distribution preflight component contract, branch preservation decision, or public trusted signing/external publication claim changes
waiver_status: public trusted signing and external stable publication not-claimed; host mutation not executed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: public distribution ops execution bundle

evidence_id: public-distribution-ops-execution-bundle-pass-20260509
route_or_operation: public distribution operations non-mutating preflight bundle
route_surface: public-distribution-ops
risk_tier: tier0-non-mutating
current_owner: packaging-windows-desktop-node
commit_sha: codex/public-ops-beta-followups
artifact_or_package_version: artifacts/public-distribution-ops-execution-bundle-20260509-0391/summary.json
target_owner: windows-desktop-node
implementation_basis: local bundle runner invokes descriptor/readiness/Burn/MSIX/winget/catalog/public-signed-rollback/Credential Manager/Event Log/TLS/service-token/timeout/diagnostic preflight generators
fallback_policy: none
identity_requirement: none
network_exposure_gate: none
runner_version: New-PcvPublicDistributionOperationsBundle.ps1
host_capability_snapshot: summary ok=true, scope public-distribution-ops-execution-bundle, actual_execution local-preflight-bundle-executed, host_mutation_performed=false, public_trusted_signing=not-claimed, external_stable_publication=not-claimed
exact_command_mode: local descriptor bundle only; no Burn build, winget submit, catalog upload, clean-host public rollback smoke, Credential Manager mutation, Event Log provider mutation, TLS certificate/binding mutation, or service token mutation
result: pass
observed_result: Pester contract passed 6/6 and artifact root recorded component summaries plus follow-up work items; `codex/diagnostic-bundle-api-action`, `codex/diagnostic-bundle-listener-evidence`, `codex/diagnostic-bundle-product-wrapper-evidence`, and `codex/full-admin-host-mutation-0389-evidence` were preserved
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: bundle script, component preflight scripts, PUBLIC_DISTRIBUTION_GATE_MATRIX fields, or branch preservation policy changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Winget CLI Validate 2026-05-09

evidence_id: winget-cli-validate-2026-05-09
artifact_or_package_version: docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md; artifacts/winget-cli-validate-20260509-0391
runner_version: Windows Package Manager `winget validate`; packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1
host_capability_snapshot: real winget CLI validation passed for generated singleton manifest preview; repository submission not executed
exact_command_mode: `winget validate --manifest artifacts/winget-cli-validate-20260509-0391/winget/PureCVisor.DesktopNode.yaml --disable-interactivity`
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: winget manifest generator, manifest schema version, installer URL/SHA contract, winget CLI validation output, or public submission policy changes
waiver_status: winget submission not executed; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: winget CLI validation

evidence_id: winget-cli-validate-pass-20260509
route_or_operation: winget manifest validation
route_surface: public-distribution-ops
risk_tier: tier0-non-mutating
current_owner: packaging-windows-desktop-node
commit_sha: codex/ops-mutation-hardening
artifact_or_package_version: artifacts/winget-cli-validate-20260509-0391/summary.json
target_owner: windows-desktop-node
implementation_basis: readiness preflight singleton manifest preview with winget schema header
fallback_policy: none
identity_requirement: none
network_exposure_gate: none
runner_version: winget validate
host_capability_snapshot: summary ok=true, actual_execution=winget-validate-executed, winget_validation_status=winget-cli-validate-pass, winget_submission=not-submitted
exact_command_mode: local `winget validate`; no winget-pkgs submission, installer upload, public trusted signing, or external stable publication
result: pass
observed_result: winget validate exit code 0 for PureCVisor.DesktopNode 0.39.1 singleton MSI manifest
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: manifest generator, manifest schema header, package metadata, installer URL/SHA, or winget CLI behavior changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Auto Non-Mutating Regression Batch 2026-05-09

evidence_id: auto-nonmutating-regression-batch-2026-05-09
artifact_or_package_version: artifacts/batch-runs/auto-nonmutating-regression-20260509-005232
runner_version: packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1
host_capability_snapshot: non-mutating regression batch completed; packaging Pester 248/248, installer Pester 41/41, web Pester 31/31, npm web verification, dotnet solution tests, and git diff check passed
exact_command_mode: Batch Supervisor non-mutating regression steps only; no MSI/Hyper-V/firewall/trust-store/LAN/Event Log/service mutation
created_at: 2026-05-09T00:55:39+09:00
stale_triggers: packaging tests, installer tests, Web Console fixture/parity contract, .NET API/Host/Runtime/Contracts tests, Batch Supervisor contract, or generated static asset contract changes
waiver_status: host mutation not executed; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: automatic non-mutating regression batch

evidence_id: auto-nonmutating-regression-batch-pass-20260509
route_or_operation: repository regression verification
route_surface: verification
risk_tier: tier0-non-mutating
current_owner: batch-supervisor
commit_sha: fb78483
artifact_or_package_version: artifacts/batch-runs/auto-nonmutating-regression-20260509-005232
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor five-step non-mutating regression manifest
fallback_policy: none
identity_requirement: none
network_exposure_gate: none
runner_version: PcvBatchSupervisor.psm1
host_capability_snapshot: summary ok=true, status=completed, total_steps=5, executed_steps=5, failed_step_id=null
exact_command_mode: packaging regression, installer regression, web regression, dotnet solution tests, git diff check
result: pass
observed_result: all five Batch Supervisor steps exited 0 without timeout; packaging Pester 248/248, installer Pester 41/41, web Pester 31/31, npm test, npm verify:parity, node syntax check, dotnet solution tests, and git diff check passed
created_at: 2026-05-09T00:55:39+09:00
stale_triggers: packaging tests, installer tests, Web Console fixture/parity contract, .NET solution tests, or Batch Supervisor execution contract changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-12 0.42.3 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-12-0423-hostmutation
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed after manual host mutation opt-in; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running with Web 80/API 7777 split; installed manifest 0.42.3-admin-smoke; Web Console HTTP 200; pcv-config.js HTTP 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation using generated input manifest artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423/batch-manifest.input.json
created_at: 2026-05-12T02:13:37+09:00
stale_triggers: service/MSI/Hyper-V route contract, Web/API port split health check, firewall/LAN/Event Log/trust-store mutation contract, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.3 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-pass-20260512-0423-hostmutation
route_or_operation: Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: 61a015a56a71a8a3194d18f0882b39d620ddf896
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423; artifacts/routeparity-service-msi-hyperv-batch-profile-20260512-021337-0423; artifacts/os-mutation-gates-batch-profile-20260512-021337-0423
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, installed manifest 0.42.3-admin-smoke, web prefix 80, API prefix 7777, Web Console HTTP 200, pcv-config.js HTTP 200, API auth boundary HTTP 401 PCV_AUTH_REQUIRED, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[]
exact_command_mode: generated FullAdminHostMutationGate manifest with version `0.42.3-admin-smoke`, then invoked `Invoke-PcvBatchSupervisor -AllowHostMutation`
result: pass
observed_result: MSI SHA-256 31ea6df1ff11cbaa9a9681b083cb5d1f61bc87ecd49db52c4e60e7a141cb229d, provenance commit 61a015a56a71a8a3194d18f0882b39d620ddf896, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP smoke PASS, public trusted signing excluded, external stable publication not-claimed
created_at: 2026-05-12T02:13:37+09:00
stale_triggers: route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-11 0.42.2 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-11-0422-hostmutation
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed after manual host mutation opt-in; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running with Web 80/API 7777 split; installed manifest 0.42.2-admin-smoke; Web Console HTTP 200; pcv-config.js HTTP 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation using persisted input manifest artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422/batch-manifest.input.json
created_at: 2026-05-11T23:26:59+09:00
stale_triggers: service/MSI/Hyper-V route contract, Web/API port split health check, firewall/LAN/Event Log/trust-store mutation contract, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.42.2 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-pass-20260511-0422-hostmutation
route_or_operation: Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: 1d68a3b6c2ac1d9202d0ec53d0ccb35858d84ee6
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260511-232659-0422; artifacts/routeparity-service-msi-hyperv-batch-profile-20260511-232659-0422; artifacts/os-mutation-gates-batch-profile-20260511-232659-0422
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, installed manifest 0.42.2-admin-smoke, web prefix 80, API prefix 7777, Web Console HTTP 200, pcv-config.js HTTP 200, API auth boundary HTTP 401 PCV_AUTH_REQUIRED, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[]
exact_command_mode: generated FullAdminHostMutationGate manifest with version `0.42.2-admin-smoke`, then invoked `Invoke-PcvBatchSupervisor -AllowHostMutation`
result: pass
observed_result: MSI SHA-256 e4d66d006cd14355b57507fea3c9a41b6c17a002f9ff824bec35830ce029fc29, provenance commit 1d68a3b6c2ac1d9202d0ec53d0ccb35858d84ee6, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP smoke PASS, public trusted signing excluded, external stable publication not-claimed
created_at: 2026-05-11T23:26:59+09:00
stale_triggers: route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-10 0.41.5 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-10-0415-hostmutation
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed after manual host mutation opt-in; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running with Web 80/API 7777 split; installed manifest 0.41.5-admin-smoke; Web Console HTTP 200; pcv-config.js HTTP 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation using persisted input manifest artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415/batch-manifest.input.json
created_at: 2026-05-10T20:00:31+09:00
stale_triggers: service/MSI/Hyper-V route contract, Web/API port split health check, firewall/LAN/Event Log/trust-store mutation contract, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.41.5 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-pass-20260510-0415-hostmutation
route_or_operation: Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: c9efe852db0e3fb4d120bc5058c56a38c7cb30db
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415; artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415; artifacts/os-mutation-gates-batch-profile-20260510-195837-0415
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, installed manifest 0.41.5-admin-smoke, web prefix 80, API prefix 7777, Web Console HTTP 200, pcv-config.js HTTP 200, API auth boundary HTTP 401, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[]
exact_command_mode: `pwsh` generated `New-PcvBatchSupervisorManifest -Profile FullAdminHostMutationGate` with version `0.41.5-admin-smoke`, ISO `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`, LAN prefix `http://[redacted-private-endpoint]:7777/`, then invoked `Invoke-PcvBatchSupervisor -AllowHostMutation`
result: pass
observed_result: MSI SHA-256 add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6, provenance commit c9efe852db0e3fb4d120bc5058c56a38c7cb30db, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP smoke PASS, public trusted signing excluded, external stable publication not-claimed
created_at: 2026-05-10T20:00:31+09:00
stale_triggers: route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Manual Admin Operator/Hardening Follow-up 2026-05-10 0.41.5

evidence_id: manual-admin-operator-hardening-followup-2026-05-10-0415
artifact_or_package_version: artifacts/manual-admin-followup-20260510-0415
runner_version: Invoke-PcvInstalledAccountLoginSmoke.ps1; Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1; DesktopNode.Host.exe service-action service-token-rotation-revoke; Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1; Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1; Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1; lifecycle-packaging-rebaseline-0415-to-0416 package pair
host_capability_snapshot: elevated administrator follow-up completed with Operator Access PASS, Internal Service Hardening PASS, and Lifecycle/Packaging current rebaseline PASS; installed account login 200/session/RBAC/console 200; target-backed noVNC echo PASS and PathName restored; service token rotation/revoke PASS with backup/atomic replace/reload/audit; Credential Manager default transition PASS under LocalSystem with runtime policy 200; HTTPS/TLS generate-bind-rotate-remove PASS; Event Log default writer/provider PASS; package pair 0.41.5-admin-smoke to 0.41.6-admin-smoke update/rollback PASS; clean-host current rebaseline PASS; final service Running with installed manifest 0.41.5-admin-smoke
exact_command_mode: individual installed MANUAL-ADMIN runners under artifacts/manual-admin-followup-20260510-0415; service token rotation followed by Credential Manager default transition to synchronize the rotated protected token into the installed service credential target
created_at: 2026-05-10T20:20:00+09:00
stale_triggers: account/JWT route contract, noVNC bridge contract, Credential Manager token target contract, service token rotation semantics, HTTPS binding lifecycle, Event Log provider/default writer, current lifecycle package pair, or public claim boundary changes
waiver_status: Burn/MSIX current package regeneration remains historical preserved; public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: 0.41.5 manual admin operator access, hardening, and lifecycle packaging

evidence_id: manual-admin-operator-hardening-followup-pass-20260510-0415
route_or_operation: installed account login, target-backed noVNC, service token rotation/revoke, Credential Manager, HTTPS/TLS lifecycle, Event Log default transition, lifecycle packaging rebaseline
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: installed-admin-smoke-runners
commit_sha: 484ed04a28fbb8dd07f513463a2a5bf77ecfa61e
artifact_or_package_version: artifacts/manual-admin-followup-20260510-0415; artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416
target_owner: windows-desktop-node
implementation_basis: installed Local API/account/noVNC runners plus native Host service-actions and MSI deferred LocalSystem hardening custom actions
fallback_policy: none
identity_requirement: elevated administrator and LocalSystem custom action where applicable
network_exposure_gate: loopback installed service only except temporary internal HTTPS loopback binding
runner_version: 0.41.5-admin-smoke installed payload plus 0.41.6-admin-smoke target package
host_capability_snapshot: operator_access_ok=true, internal_service_hardening_ok=true, lifecycle_packaging_ok=true, final service Running, installed manifest 0.41.5-admin-smoke, token values not observed
exact_command_mode: `Invoke-PcvInstalledAccountLoginSmoke.ps1`; `Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`; `DesktopNode.Host.exe service-action service-token-rotation-revoke`; `Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1 -Version 0.41.5-admin-smoke`; `Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1`; `Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1 -Version 0.41.5-admin-smoke`; 0.41.5 to 0.41.6 installed product update/rollback and internal clean-host rebaseline
result: pass
observed_result: account login/session/RBAC/console HTTP 200; noVNC target frame echoed with SHA-256 c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106 and PathName restored; service token mutation performed with backup written, atomic replace completed, service reload restarted, old token rejected after reload, audit written; Credential Manager MSI SHA-256 6684061dd248ff2a9567bc251bf45b73ba1ef8174ed92e3f6cd24b2de3dfa615; Event Log MSI SHA-256 b191c45c66a57f987e262d491eeb6d22ea7af5745c93c120d02e41f18592e4ab; TLS initial/rotated/restored HTTP 200; Lifecycle/Packaging current rebaseline passed with installed update/rollback and clean-host install/update/rollback for 0.41.5-admin-smoke to 0.41.6-admin-smoke
created_at: 2026-05-10T20:20:00+09:00
stale_triggers: installed operator access route contract, hardening service-action contract, current package pair availability, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Lifecycle/Packaging Rebaseline 2026-05-10 0415 to 0416

evidence_id: lifecycle-packaging-rebaseline-2026-05-10-0415-0416
artifact_or_package_version: docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md; artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416
runner_version: installer build.ps1; Invoke-PcvDesktopNodeProduct.ps1 update/rollback; Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1 Web/API split health check update
host_capability_snapshot: package pair 0.41.5-admin-smoke to 0.41.6-admin-smoke generated; baseline MSI SHA-256 add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6; target MSI SHA-256 967ac29bf2928f1fec3a0bb72425d15d2eda65a2466b1cb29dd9183bb18928a3; update ZIP SHA-256 4e54c19ca6e6a9beec506613d66220c8b0bbbb579d0926d1d840f2cde7592161; installed product update/rollback PASS; internal clean-host install/update/rollback PASS; final manifest 0.41.5-admin-smoke; failed root manifest 0.41.6-admin-smoke; Web Console HTTP 200; API unauthenticated runtime policy HTTP 401
exact_command_mode: elevated installed product update/rollback on host; Hyper-V clean-host package-pair smoke with Windows Update to UBR 5020 and manual resume after stale runner port check; no public signing, public publication, winget submission, or public clean-host smoke
created_at: 2026-05-10T21:05:00+09:00
stale_triggers: package pair, update wrapper, clean-host runner, Web/API port split, .NET payload CET requirements, MSI custom actions, or public claim boundary changes
waiver_status: Burn/MSIX current package regeneration remains historical preserved; public trusted signing and external stable publication out-of-scope
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope

### Record: Lifecycle/Packaging current rebaseline

evidence_id: lifecycle-packaging-rebaseline-pass-20260510-0415-0416
route_or_operation: internal installed update/rollback and clean-host install/update/rollback
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: packaging-lifecycle-rebaseline
commit_sha: codex/lifecycle-packaging-rebaseline-0416
artifact_or_package_version: artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/summary.json
target_owner: windows-desktop-node
implementation_basis: current baseline/target MSI package pair, update ZIP/catalog, product wrapper update/rollback, Hyper-V clean-host runner
fallback_policy: none
identity_requirement: elevated administrator and dedicated Hyper-V clean-host guest
network_exposure_gate: loopback Web Console and protected Web API only
runner_version: 0.41.5-admin-smoke baseline, 0.41.6-admin-smoke target
host_capability_snapshot: installed_product_update_rollback=true, internal_clean_host_install_update_rollback=true, final_service=Running, final_manifest=0.41.5-admin-smoke, failed_root_manifest=0.41.6-admin-smoke
exact_command_mode: generated package pair, ran installed update/rollback, ran clean-host smoke with Windows Update and resumed current Web/API split check
result: pass
observed_result: update exit 0, rollback exit 0, Web Console 200, API unauthenticated boundary 401, clean-host update and rollback exit 0, guest UBR 5020, dedicated VM removed after evidence
created_at: 2026-05-10T21:05:00+09:00
stale_triggers: package pair version, update catalog schema, product health check, clean-host VM image baseline, Web/API listener split, or public claim boundary changes
waiver_status: public trusted signing and external stable publication out-of-scope

## Evidence Group: Full Admin Host Mutation Gate 2026-05-10 0.41.2 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-10-0412-hostmutation
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed after manual host mutation opt-in; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running with Web 80/API 7777 split; Web Console HTTP 200; pcv-config.js HTTP 200; API unauthenticated boundary 401 PCV_AUTH_REQUIRED; firewall count 0; Event Log source absent; internal trust cert present; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation using persisted input manifest artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412/batch-manifest.input.json
created_at: 2026-05-10T16:15:59+09:00
stale_triggers: service/MSI/Hyper-V route contract, Web/API port split health check, firewall/LAN/Event Log/trust-store mutation contract, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: 0.41.2 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-pass-20260510-0412-hostmutation
route_or_operation: Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: d098f0fc631ff1799d7dd238a84e896fe8616230
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412; artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412; artifacts/os-mutation-gates-batch-profile-20260510-161416-0412
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, web prefix 80, API prefix 7777, Web Console HTTP 200, pcv-config.js HTTP 200, API auth boundary HTTP 401, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[]
exact_command_mode: `pwsh` generated `New-PcvBatchSupervisorManifest -Profile FullAdminHostMutationGate` with version `0.41.2-admin-smoke`, ISO `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`, LAN prefix `http://[redacted-private-endpoint]:7777/`, then invoked `Invoke-PcvBatchSupervisor -AllowHostMutation`
result: pass
observed_result: MSI SHA-256 ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0, provenance commit d098f0fc631ff1799d7dd238a84e896fe8616230, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP smoke PASS, public trusted signing not-claimed, external stable publication not-claimed
created_at: 2026-05-10T16:15:59+09:00
stale_triggers: route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-10 0.41.0 Account Rerun

evidence_id: full-admin-host-mutation-gate-2026-05-10-0410-account-rerun
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed after Web/API split health check correction; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running with Web 80/API 7777 split; firewall count 0; Event Log source absent; boot time unchanged; remaining_pcv_vms=[]; installed account login smoke passed afterwards
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation, then installed account login smoke against final installed service
created_at: 2026-05-10T15:50:47+09:00
stale_triggers: service/MSI/Hyper-V route contract, Web/API port split health check, firewall/LAN/Event Log/trust-store mutation contract, account auth smoke runner ACL handling, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: 0.41.0 full admin host mutation gate and installed account login smoke

evidence_id: full-admin-host-mutation-gate-pass-20260510-0410-account-rerun
route_or_operation: Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate plus installed account login smoke
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: a3226ef637ea895d2f2a9956599e0d5e79d00410
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun; artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun; artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun; artifacts/installed-account-login-smoke-20260510-0410-final
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile; Invoke-PcvInstalledAccountLoginSmoke.ps1 protected account/JWT temporary ACL mutation and restore
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1; Invoke-PcvInstalledAccountLoginSmoke.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, web prefix 80, API prefix 7777, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[], installed account login/session/RBAC/console HTTP 200, runtime_auth_mode=account_rbac_jwt, account/JWT content hash and ACL restored
exact_command_mode: `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun/manifest.json -AllowHostMutation`; `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-smoke-20260510-0410-final -ServiceName PureCVisorDesktopNode -ApiBaseUri http://127.0.0.1:7777 -DataRoot C:\ProgramData\PureCVisor\desktop-node`
result: pass
observed_result: MSI SHA-256 cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP smoke PASS, installed account login smoke PASS with token/password/refresh token values not recorded, public trusted signing not-claimed, external stable publication not-claimed
created_at: 2026-05-10T15:50:47+09:00
stale_triggers: route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, account/JWT protected ACL restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Full Admin Host Mutation Gate 2026-05-09 0.39.1

evidence_id: full-admin-host-mutation-gate-2026-05-09-0391
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-032525-0391-rerun
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running; firewall count 0; Event Log source absent; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation
created_at: 2026-05-09T03:27:20+09:00
stale_triggers: service/MSI/Hyper-V route contract, firewall/LAN/Event Log/trust-store mutation contract, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.39.1 full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-pass-20260509-0391
route_or_operation: Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: 0815a6281bcb98b5b1795e8d054073e1c9fb4892
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-032525-0391-rerun; artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-032525-0391-rerun; artifacts/os-mutation-gates-batch-profile-20260509-032525-0391-rerun
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[]
exact_command_mode: `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/full-admin-host-mutation-gate-20260509-032525-0391-rerun/manifest.json -AllowHostMutation`
result: pass
observed_result: MSI SHA-256 25a88e41ed926a6bccaf3eba1fdd44d0976091aca9fd6ef77f52eea2bddf3c37, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP 200, public trusted signing excluded, external stable publication not-claimed
created_at: 2026-05-09T03:27:20+09:00
stale_triggers: route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Frontend Full Admin Host Mutation Gate 2026-05-09 0.39.1

evidence_id: full-admin-host-mutation-gate-2026-05-09-0391-frontend
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-122028-0391-frontend
runner_version: packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 FullAdminHostMutationGate
host_capability_snapshot: elevated administrator Batch Supervisor full gate completed for the frontend payload line; Service/MSI/Hyper-V route smoke and firewall/LAN/Event Log/internal trust-store OS mutation gate passed; final service Running; firewall count 0; Event Log source absent; boot time unchanged; remaining_pcv_vms=[]
exact_command_mode: Batch Supervisor full admin host mutation gate with -AllowHostMutation
created_at: 2026-05-09T12:22:31+09:00
stale_triggers: Web Console payload, service/MSI/Hyper-V route contract, firewall/LAN/Event Log/trust-store mutation contract, installer payload, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: 0.39.1 frontend full admin host mutation gate

evidence_id: full-admin-host-mutation-gate-pass-20260509-0391-frontend
route_or_operation: frontend payload Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store full admin gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: batch-supervisor
commit_sha: d8e7e162a13817dc869f30712d77c5c036981786
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-122028-0391-frontend; artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-122028-0391-frontend; artifacts/os-mutation-gates-batch-profile-20260509-122028-0391-frontend
target_owner: windows-desktop-node
implementation_basis: Batch Supervisor ServiceMsiHyperVAdminSmoke plus OsMutationGate profile
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: explicit LAN smoke only
runner_version: Invoke-PcvBatchSupervisor.ps1
host_capability_snapshot: summary ok=true, status=completed, total_steps=2, executed_steps=2, final service Running, firewall final count 0, Event Log source absent, internal trust cert present, boot time unchanged, remaining_pcv_vms=[]
exact_command_mode: `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/full-admin-host-mutation-gate-20260509-122028-0391-frontend/manifest.json -AllowHostMutation`
result: pass
observed_result: MSI SHA-256 f5086e64a58bdb43a8196574dacf383d600c5cccca0f60aeb99ed3f95b65bd73, signing mode AllowUnsignedDev, Service/MSI/Hyper-V route smoke PASS, firewall/LAN/Event Log/internal trust-store OS mutation PASS, LAN listener http://[redacted-private-endpoint]:7777/ HTTP 200, public trusted signing excluded, external stable publication not-claimed
created_at: 2026-05-09T12:22:31+09:00
stale_triggers: frontend payload, route parity smoke, MSI lifecycle, OS mutation gate, internal trust-store restore, or public claim boundary changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Web Console Installed Listener QA 2026-05-09

evidence_id: web-console-installed-listener-qa-2026-05-09
artifact_or_package_version: docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md; artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b
runner_version: web/scripts/capture-installed-listener-qa.mjs
host_capability_snapshot: installed listener Web Console QA completed against http://127.0.0.1:7777/; dashboard, VM detail, jobs, network, troubleshooting, diagnostic create/download, responsive screenshots, and basic accessibility probes passed; browser QA did not mutate host state
exact_command_mode: Chrome CDP installed-listener browser QA with token passed through process environment only
created_at: 2026-05-09T13:08:49+09:00
stale_triggers: Web Console served asset, installed listener route contract, diagnostic bundle UI, token UX, jobs/activity edge states, network inventory UX, responsive CSS, or accessibility labels change
waiver_status: public trusted signing and external stable publication not-claimed
token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: installed Web Console browser QA

evidence_id: web-console-installed-listener-qa-pass-20260509
route_or_operation: installed Web Console browser QA and screenshot evidence
route_surface: web-console
risk_tier: tier1-read-only
current_owner: typescript-web-console
commit_sha: 38e31b3ca0b84cb0cdd417b75011100a4de8ad8b
artifact_or_package_version: docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md; artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b; artifacts/batch-runs/full-admin-host-mutation-gate-20260509-130105-0391-frontend-final2
target_owner: windows-desktop-node-web-console
implementation_basis: installed listener Chrome CDP smoke plus static/fixture edge-state guards
fallback_policy: none
identity_requirement: browser token supplied without evidence capture
network_exposure_gate: loopback-default
runner_version: capture-installed-listener-qa.mjs
host_capability_snapshot: token supplied true, token value observed false, diagnostic create clicked true, diagnostic download clicked true, missing button labels 0, unlabeled inputs 0, screenshots generated for dashboard/vm-detail/jobs/network/troubleshooting and 1366/tablet/mobile responsive states
exact_command_mode: `node web/scripts/capture-installed-listener-qa.mjs --url=http://127.0.0.1:7777/ --out=artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b`
result: pass
observed_result: installed listener HTTP 200, real view navigation, VM filter/sort, jobs, network, troubleshooting, diagnostic bundle create/download; no selectable VM remained after smoke cleanup, so no VM create/select destructive action was triggered by browser QA
created_at: 2026-05-09T13:08:49+09:00
stale_triggers: installed listener asset payload, VM workbench, diagnostic create/download UI, token clear flow, jobs pagination/retention UI, network structured failure copy, problem-details mapping, responsive CSS, or a11y labels change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Web Console Destructive Lifecycle UI 2026-05-09

evidence_id: web-console-destructive-lifecycle-ui-2026-05-09
artifact_or_package_version: docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md; artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391
runner_version: web/scripts/capture-destructive-lifecycle-ui-qa.mjs
host_capability_snapshot: installed listener Web Console destructive lifecycle UI QA completed against http://127.0.0.1:7777/; managed VM create/start/restart/poweroff/delete and checkpoint create/restore/delete passed; cleanup verified no pcv-spike-ui-* VM remained
exact_command_mode: Chrome CDP installed-listener browser QA with token passed through process environment only; all lifecycle mutations queued by Web Console UI controls
created_at: 2026-05-09T15:05:08+09:00
stale_triggers: Web Console VM workbench controls, checkpoint controls, lifecycle confirmation copy, installed listener VM/checkpoint route contract, job polling contract, or Hyper-V managed VM cleanup behavior changes
waiver_status: public trusted signing and external stable publication not-claimed
token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: installed Web Console destructive lifecycle UI QA

evidence_id: web-console-destructive-lifecycle-ui-pass-20260509
route_or_operation: installed Web Console destructive VM and checkpoint lifecycle UI
route_surface: web-console
risk_tier: tier3-destructive-or-persistent
current_owner: typescript-web-console
commit_sha: working-tree-destructive-ui-evidence-20260509
artifact_or_package_version: docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md; artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391/summary.json
target_owner: windows-desktop-node-web-console
implementation_basis: installed listener Chrome CDP runner, Web Console create form, VM lifecycle buttons, checkpoint form/buttons, and Local API job completion polling
fallback_policy: cleanup-only fallback allowed after failed QA attempt; PASS path did not use fallback
identity_requirement: browser token supplied without evidence capture
network_exposure_gate: loopback-default
runner_version: capture-destructive-lifecycle-ui-qa.mjs
host_capability_snapshot: summary ok=true, host_mutation_performed=true, mutation_source=installed-listener-web-console-ui, action_count=8, screenshot_count=10, confirm_count=5, cleanup.vm_absent_after_delete=true
exact_command_mode: `node web/scripts/capture-destructive-lifecycle-ui-qa.mjs --url=http://127.0.0.1:7777/ --out=artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391 --vm-name=pcv-spike-ui-20260509-150353 --iso=D:\Downloads\Rocky-10.1-x86_64-minimal.iso --checkpoint-name=ui-before-restore`
result: pass
observed_result: UI-queued jobs succeeded for vm.create, vm.start, vm.restart, checkpoint.create, vm.poweroff, checkpoint.restore, checkpoint.delete, and vm.delete; destructive confirmations were captured for restart, poweroff, checkpoint restore, checkpoint delete, and VM delete; final `Get-VM -Name 'pcv-spike-ui-*'` returned no VM
created_at: 2026-05-09T15:05:08+09:00
stale_triggers: installed listener asset payload, VM create form, lifecycle button selectors, checkpoint controls, confirmation copy, job result contract, or managed VM cleanup behavior changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: MSI Update Package Apply 2026-05-09 0.39.1

evidence_id: msi-update-package-apply-2026-05-09-0391
artifact_or_package_version: artifacts/msi-update-package-20260509-0391
runner_version: packaging/windows-desktop-node/installer/build.ps1 plus artifacts/msi-update-package-20260509-0391/Apply-0.39.1-admin-smoke.ps1
host_capability_snapshot: elevated administrator MSI apply completed; installed PureCVisorDesktopNode final service Running; installed product manifest 0.39.1-admin-smoke; loopback Web Console HTTP 200
exact_command_mode: AllowUnsignedDev MSI build, update ZIP/catalog hash validation, elevated MSI install apply, non-mutating installed status/Web verification
created_at: 2026-05-09T00:30:00+09:00
stale_triggers: MSI payload contract, Web Console asset payload, update ZIP/catalog resolver contract, service-action configure-installed behavior, installer ProductCode/UpgradeCode behavior, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: MSI/update package apply

evidence_id: msi-update-package-apply-pass-20260509-0391
route_or_operation: MSI/update package apply
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: installer-build
commit_sha: 8f0c4b6fbac8787932d0e966437fcc62d86e6068
artifact_or_package_version: artifacts/msi-update-package-20260509-0391
target_owner: windows-msi-installer-and-product-wrapper-update-source
implementation_basis: WiX MSI build plus update package/catalog validation plus elevated MSI apply
fallback_policy: none
identity_requirement: elevated administrator
network_exposure_gate: loopback-default
runner_version: build.ps1 0.39.1-admin-smoke and Apply-0.39.1-admin-smoke.ps1
host_capability_snapshot: installed manifest version 0.39.1-admin-smoke, service PureCVisorDesktopNode Running, loopback Web Console HTTP 200
exact_command_mode: `pwsh -NoProfile -ExecutionPolicy Bypass -File artifacts/msi-update-package-20260509-0391/Apply-0.39.1-admin-smoke.ps1`
result: pass
observed_result: MSI exit code 0, MSI SHA-256 9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914, update ZIP SHA-256 d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5, provenance commit 8f0c4b6fbac8787932d0e966437fcc62d86e6068, installed manifest 0.39.1-admin-smoke, service Running, loopback Web Console HTTP 200
created_at: 2026-05-09T00:30:00+09:00
stale_triggers: MSI payload contract, installed service wrapper, Web Console asset payload, update package/catalog validation, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Installed Listener External Load Rate Limit 2026-05-09

evidence_id: installed-listener-external-load-rate-limit-2026-05-09
artifact_or_package_version: docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md; artifacts/installed-listener-external-load-rate-limit-20260509-0391
runner_version: installed listener external HTTP load smoke
host_capability_snapshot: installed PureCVisorDesktopNode listener served 180 real HTTP requests with expected rate-limit distribution and problem-details contract
exact_command_mode: external HTTP loop to `http://127.0.0.1:7777/api/v1/runtime/policy` using DPAPI protected service token resolved in memory only
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: installed listener hardening args, request limit defaults, problem-details contract, service token storage, or Local API runtime policy route changes
waiver_status: host mutation not executed; public trusted signing and external stable publication not-claimed
token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: installed listener external load/rate-limit smoke

evidence_id: installed-listener-external-load-rate-limit-pass-20260509
route_or_operation: installed listener external load/rate-limit smoke
route_surface: local-api
risk_tier: tier1-read-only-load
current_owner: dotnet-host-api-hardening
commit_sha: codex/ops-mutation-hardening
artifact_or_package_version: artifacts/installed-listener-external-load-rate-limit-20260509-0391/summary.json
target_owner: windows-desktop-node
implementation_basis: installed listener HTTP request budget and retry problem-details contract
fallback_policy: none
identity_requirement: DPAPI protected service token resolved in memory only
network_exposure_gate: loopback-default
runner_version: external HTTP load smoke
host_capability_snapshot: summary ok=true, request_count=180, HTTP 200=140, HTTP 429=40, unexpected=0, retry-after 429 count=40, rate-limit problem-details count=40
exact_command_mode: real HTTP requests to installed listener; no host mutation, no token value capture
result: pass
observed_result: installed listener returned expected rate-limit distribution with `Retry-After` and `PCV_RATE_LIMIT_EXCEEDED` on all 429 responses
created_at: 2026-05-09T00:00:00+09:00
stale_triggers: hardening defaults, Local API problem-details contract, installed service config, or listener auth behavior changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Timeout Rate Limit Hardening Load Test Code-Level 2026-05-08

evidence_id: timeout-rate-limit-hardening-load-test-code-level-2026-05-08
artifact_or_package_version: ApiHardeningRequestProcessorTests.cs
runner_version: ApiHardeningRequestProcessorTests.cs
host_capability_snapshot: code-level in-process load evidence over 64 same-client Local API requests; 20 HTTP 200, 44 HTTP 429 problem-details, no installed listener, no external load generator, no host mutation
exact_command_mode: focused dotnet API test
created_at: 2026-05-08T13:05:00+09:00
stale_triggers: request limit budget, burst policy, retry-after problem-details shape, client identity mapping, route timeout/request limit ordering, installed listener load evidence, or public claim status changes
waiver_status: installed listener external load was not part of this 2026-05-08 code-level evidence; covered later by installed-listener-external-load-rate-limit-2026-05-09
timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load
route_timeout_policy: code-level-applied
request_limit_policy: code-level-applied
retry_semantics_status: retry-after-problem-details-code-level
ui_api_error_contract_status: problem-details-json-code-level
load_test_status: code-level-inprocess-pass
server_config_mutation: code-level-product-and-native-service-plan-applied

### Record: timeout/rate-limit in-process load evidence

evidence_id: timeout-rate-limit-hardening-load-test-code-level-pass-20260508
route_or_operation: Local API timeout/rate-limit load evidence
route_surface: local-api
risk_tier: tier1-read-only
current_owner: dotnet-api
commit_sha: codex/hardening-load-evidence
artifact_or_package_version: docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-inprocess-load-test
fallback_policy: none
identity_requirement: same-client-identity-window
network_exposure_gate: none
runner_version: dotnet API tests
host_capability_snapshot: 64 same-client `/api/v1/runtime/policy` requests against `DesktopNodeApiRequestProcessor` with limit 16, burst 4, retry-after 9 return 20 successes and 44 HTTP 429 `PCV_RATE_LIMIT_EXCEEDED` problem-details responses
exact_command_mode: request processor in-process load test, no listener or network load generator
result: pass
observed_result: RED guard first showed missing `RunInProcessHardeningLoad` helper. GREEN focused verification passed the in-process load evidence test. At this evidence point the matrix was internal/public-candidate scoped with `load_test_status=code-level-inprocess-pass`; installed listener external load was covered later by `installed-listener-external-load-rate-limit-2026-05-09`, while external public publication remains open.
created_at: 2026-05-08T13:05:00+09:00
stale_triggers: hardening options, response body/header contract, request limit window accounting, concurrency behavior, installed listener load, external load generator evidence, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Timeout Rate Limit Hardening Server Config Code-Level 2026-05-08

evidence_id: timeout-rate-limit-hardening-server-config-code-level-2026-05-08
artifact_or_package_version: PcvDesktopNodeProduct.psm1, PcvDesktopNodeProduct.Plan.Tests.ps1
runner_version: PcvDesktopNodeProduct.Plan.Tests.ps1
host_capability_snapshot: code-level product service plan binary path includes route timeout, request limit, burst, and retry-after arguments; no load test, no installed service mutation, no host mutation
exact_command_mode: focused Pester product plan test
created_at: 2026-05-08T12:24:00+09:00
stale_triggers: hardening default values, service binary path argument contract, product plan descriptor shape, load test evidence, installed service config mutation, or public claim status changes
waiver_status: load-test-installed-service-mutation-not-run
timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config
route_timeout_policy: code-level-applied
request_limit_policy: code-level-applied
retry_semantics_status: retry-after-problem-details-code-level
ui_api_error_contract_status: problem-details-json-code-level
load_test_status: not-run
server_config_mutation: code-level-product-and-native-service-plan-applied

### Record: timeout/rate-limit service config plan

evidence_id: timeout-rate-limit-hardening-server-config-code-level-pass-20260508
route_or_operation: Local API timeout/rate-limit service config plan
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: product-wrapper
commit_sha: codex/server-config-hardening-plan
artifact_or_package_version: docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-service-plan
fallback_policy: none
identity_requirement: none
network_exposure_gate: none
runner_version: Pester product plan tests
host_capability_snapshot: `DesktopNode.Host.exe listen` product plan includes `--route-timeout-seconds 30`, `--request-limit-per-minute 120`, `--request-burst-limit 20`, and `--retry-after-seconds 15`; `service.hardening` records the same values
exact_command_mode: product plan contract test, no `sc.exe` execution
result: pass
observed_result: RED guard first showed missing `service.hardening` and missing hardening command-line arguments. GREEN focused verification passed the install product plan test. The later native service-action config guard applies the same arguments to `DesktopNodeWindowsServiceConfiguration.BinaryPathName`; `0.39.0-admin-smoke` later confirmed those args on the installed service, and `installed-listener-external-load-rate-limit-2026-05-09` later covered listener-level external load.
created_at: 2026-05-08T12:24:00+09:00
stale_triggers: hardening options, service plan binary path, product manifest/service descriptor shape, load test, installed service mutation, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Timeout Rate Limit Hardening Route Timeout Code-Level 2026-05-08

evidence_id: timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08
artifact_or_package_version: DesktopNodeApiRequestProcessor.cs
runner_version: ApiHardeningRequestProcessorTests.cs
host_capability_snapshot: code-level Local API GET/read route response deadline, HTTP 504 Retry-After problem-details JSON, request limit evidence preserved, API/Host suites pass, no load test, no host mutation
exact_command_mode: dotnet focused API test, full API/Host tests, plus documentation guard
created_at: 2026-05-08T03:10:00+09:00
stale_triggers: route timeout seconds, timeout response body/header shape, request id propagation, native adapter timeout/cancellation semantics, load test evidence, server config mutation, or public claim status changes
waiver_status: load-test-server-config-mutation-not-run

### Record: timeout/rate-limit code-level route timeout

evidence_id: timeout-rate-limit-hardening-route-timeout-code-level-pass-20260508
route_or_operation: Local API route timeout hardening
route_surface: local-api
risk_tier: tier1-read-only
current_owner: dotnet-api
commit_sha: codex/route-timeout-actual-contract
artifact_or_package_version: docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-actual
fallback_policy: none
identity_requirement: request-id-propagation
network_exposure_gate: loopback-default-lan-token-required
runner_version: dotnet API tests
host_capability_snapshot: `/api/v1/` GET/read routes return HTTP 504, Retry-After, application/problem+json, and PCV_ROUTE_TIMEOUT when the configured route deadline is exceeded
exact_command_mode: request processor unit test with delayed native adapter
result: pass
observed_result: RED guard first showed a delayed native route returning HTTP 200 after the deadline. GREEN focused verification passed the route timeout test, API 129 tests, and Host 63 tests. Matrix is partial: `route_timeout_policy=code-level-applied`, `request_limit_policy=code-level-applied`, `retry_semantics_status=retry-after-problem-details-code-level`, and `ui_api_error_contract_status=problem-details-json-code-level`, while `load_test_status=not-run` and `server_config_mutation=not-run` remain open.
created_at: 2026-05-08T03:10:00+09:00
stale_triggers: hardening options, HTTP 504 body/header, request id mapping, request limit preservation, load test, server config mutation, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Timeout Rate Limit Hardening Code-Level 2026-05-08

evidence_id: timeout-rate-limit-hardening-code-level-2026-05-08
artifact_or_package_version: DesktopNodeApiRequestProcessor.cs, DesktopNodeHostApplication.cs, DesktopNodeHostOptions.cs
runner_version: ApiHardeningRequestProcessorTests.cs, DesktopNodeHostApplicationTests.cs, DesktopNodeHostOptionsTests.cs
host_capability_snapshot: code-level Local API request limit enforcement, Retry-After header, problem-details JSON contract, no route timeout enforcement, no load test, no host mutation
exact_command_mode: dotnet focused API/Host tests
created_at: 2026-05-08T02:30:00+09:00
stale_triggers: rate limit window, client identity source, Retry-After semantics, problem-details shape, host option parsing, route timeout enforcement, load test evidence, or public claim status changes
waiver_status: route-timeout-load-test-server-config-mutation-not-run

### Record: timeout/rate-limit code-level request limit

evidence_id: timeout-rate-limit-hardening-code-level-pass-20260508
route_or_operation: Local API request rate-limit hardening
route_surface: local-api
risk_tier: tier1-read-only
current_owner: dotnet-api-host
commit_sha: codex/timeout-rate-limit-actual-contract
artifact_or_package_version: docs/ga-ready/evidence/timeout-rate-limit-hardening-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-actual
fallback_policy: none
identity_requirement: client-identity-window
network_exposure_gate: loopback-default-lan-token-required
runner_version: dotnet API/Host tests
host_capability_snapshot: `/api/v1/` per-client request window returns HTTP 429, Retry-After, application/problem+json, and PCV_RATE_LIMIT_EXCEEDED when exceeded
exact_command_mode: request processor unit tests plus HttpListener host test
result: pass
observed_result: RED guard first showed missing DesktopNodeApiHardeningOptions, response headers, host option parsing, and HTTP Retry-After propagation. GREEN focused verification passed API 128 tests and Host 63 tests. Matrix is partial: `request_limit_policy=code-level-applied`, `retry_semantics_status=retry-after-problem-details-code-level`, `ui_api_error_contract_status=problem-details-json-code-level`, while `route_timeout_policy=not-applied`, `load_test_status=not-run`, and `server_config_mutation=not-run` remain open.
created_at: 2026-05-08T02:30:00+09:00
stale_triggers: hardening options, response header contract, HTTP 429 body, host identity mapping, route timeout, load test, server config mutation, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Timeout Rate Limit Hardening Preflight 2026-05-08

evidence_id: timeout-rate-limit-hardening-preflight-2026-05-08
artifact_or_package_version: New-PcvTimeoutRateLimitHardeningPreflight.ps1
runner_version: PcvTimeoutRateLimitHardeningPreflight.Tests.ps1
host_capability_snapshot: non-mutating timeout and rate-limit hardening plan preview, no server config mutation, no middleware enablement, no load test execution, no host mutation
exact_command_mode: timeout/rate-limit hardening preflight `-PlanOnly`
created_at: 2026-05-08T02:05:00+09:00
stale_triggers: route timeout target, request limit target, retry-after target, UI/API error contract, middleware policy, load test evidence, server config mutation policy, or public claim status changes
waiver_status: timeout-rate-limit-mutation-not-run

### Record: timeout/rate-limit hardening preflight

evidence_id: timeout-rate-limit-hardening-preflight-pass-20260508
route_or_operation: timeout/rate-limit hardening preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/timeout-rate-limit-preflight
artifact_or_package_version: docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
identity_requirement: bearer-token-required-before-pass
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: timeout/rate-limit hardening plan preview only, no server config mutation, no middleware enablement, no load test execution, no host mutation
exact_command_mode: descriptor records service name, Local API route prefix, route timeout target, request limit target, retry-after target, UI/API error contract, hardening checks, and plan preview
result: pass
observed_result: RED guard first showed missing New-PcvTimeoutRateLimitHardeningPreflight.ps1 and missing timeout/rate-limit hardening evidence linkage. GREEN focused verification passed 6 tool tests. Dry-run artifact `artifacts/timeout-rate-limit-hardening-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `timeout_rate_limit_hardening=blocked-by-no-mutation-preflight`, `route_timeout_policy=not-applied`, `request_limit_policy=not-applied`, `retry_semantics_status=not-run`, `ui_api_error_contract_status=not-run`, `load_test_status=not-run`, and `server_config_mutation=not-run`.
created_at: 2026-05-08T02:05:00+09:00
stale_triggers: hardening check names, plan preview shape, timeout/rate-limit blocker state, UI/API error contract, load test boundary, server config mutation boundary, or not-claimed boundary values change
waiver_status: timeout/rate-limit hardening not applied; public trusted signing and external stable publication not-claimed

## Evidence Group: Diagnostic Bundle Server Preflight 2026-05-08

evidence_id: diagnostic-bundle-server-preflight-2026-05-08
artifact_or_package_version: New-PcvDiagnosticBundleServerPreflight.ps1
runner_version: PcvDiagnosticBundleServerPreflight.Tests.ps1
host_capability_snapshot: non-mutating diagnostic bundle server-side plan preview, no Local API action execution, no archive creation, no download serving, no host mutation
exact_command_mode: diagnostic bundle server-side preflight `-PlanOnly`
created_at: 2026-05-08T03:45:00+09:00
stale_triggers: Local API route, download route, archive output policy, redaction policy, authorization policy, retention policy, product diagnostics delegation, or public claim status changes
waiver_status: diagnostic-bundle-server-mutation-not-run

### Record: diagnostic bundle server-side preflight

evidence_id: diagnostic-bundle-server-preflight-pass-20260508
route_or_operation: diagnostic bundle server-side generation/download preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/diagnostic-bundle-server-preflight
artifact_or_package_version: docs/ga-ready/evidence/diagnostic-bundle-server-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
identity_requirement: bearer-token-required-before-pass
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: diagnostic bundle server-side plan preview only, no Local API action execution, no archive creation, no download serving, no host mutation
exact_command_mode: descriptor records service name, diagnostics root, Local API generation route, download route template, authz policy, redaction policy, retention policy, diagnostic checks, and plan preview
result: pass
observed_result: RED guard first showed missing New-PcvDiagnosticBundleServerPreflight.ps1 and missing diagnostic bundle server-side evidence linkage. GREEN focused verification passed 6 tool tests. Dry-run artifact `artifacts/diagnostic-bundle-server-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `diagnostic_bundle_server_generation=blocked-by-no-mutation-preflight`, `diagnostic_bundle_api_action=not-run`, `diagnostic_bundle_archive_created=false`, `diagnostic_bundle_download_served=false`, `diagnostic_bundle_redaction_status=not-run`, `diagnostic_bundle_authz_status=not-run`, and `diagnostic_bundle_retention_status=not-run`.
created_at: 2026-05-08T03:45:00+09:00
stale_triggers: diagnostic check names, plan preview shape, API/action/download blocker state, redaction/authz/retention boundary, or not-claimed boundary values change
waiver_status: diagnostic bundle server-side action not executed; public trusted signing and external stable publication not-claimed

## Evidence Group: Diagnostic Bundle Server Code-Level 2026-05-08

evidence_id: diagnostic-bundle-server-code-level-2026-05-08
artifact_or_package_version: DesktopNodeApiRequestProcessor.cs, DesktopNodeHostOptions.cs, PcvDesktopNodeProduct.psm1
runner_version: ApiDiagnosticBundleRequestProcessorTests.cs, ApiHandlerAdapterContractTests.cs, DesktopNodeHostOptionsTests.cs, PcvDesktopNodeProduct.Plan.Tests.ps1
host_capability_snapshot: code-level Local API POST creates a redacted server-side `.bundle.json`; GET download route serves it; retention max-count applies; service plan wires diagnostics root; no installed listener, no product wrapper delegation, no host mutation, no public claim
exact_command_mode: focused dotnet API/Host tests and focused Pester product plan test
created_at: 2026-05-08T14:30:00+09:00
stale_triggers: diagnostic bundle route shape, diagnostics root option, redaction policy, authorization route contract, retention policy, product diagnostics delegation, installed listener evidence, or public claim status changes
waiver_status: installed-listener-product-wrapper-delegation-not-run
diagnostic_bundle_server_generation: partial-code-level-api-action
diagnostic_bundle_api_action: code-level-applied
diagnostic_bundle_archive_created: code-level-created
diagnostic_bundle_download_served: code-level-download-served
diagnostic_bundle_redaction_status: code-level-applied
diagnostic_bundle_authz_status: token-required-route-contract
diagnostic_bundle_retention_status: code-level-applied

### Record: diagnostic bundle server API action

evidence_id: diagnostic-bundle-server-code-level-pass-20260508
route_or_operation: POST /api/v1/diagnostics/bundles and GET /api/v1/diagnostics/bundles/{bundle_id}/download
route_surface: product-operation
risk_tier: tier1-code-level-file-artifact
current_owner: dotnet-runtime
commit_sha: codex/diagnostic-bundle-api-action
artifact_or_package_version: docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-api-action
fallback_policy: none
identity_requirement: token-required-route-contract
network_exposure_gate: loopback-default-lan-token-required
runner_version: dotnet API/Host tests and Pester product plan test
host_capability_snapshot: `POST /api/v1/diagnostics/bundles` writes a redacted `.bundle.json`; `GET /api/v1/diagnostics/bundles/{bundle_id}/download` returns the saved bundle with `X-PCV-Diagnostic-Bundle-Id`; product service plan includes `--diagnostics-root`
exact_command_mode: request processor in-process API test plus product plan contract test, no installed listener or service mutation
result: pass
observed_result: RED guards first showed missing `DesktopNodeDiagnosticBundleOptions`, missing route contracts, missing Host `--diagnostics-root` option, and missing product service plan wiring. GREEN focused verification passed diagnostic bundle API creation/download/redaction/retention, adapter route ownership, Host option parsing, and product plan diagnostics-root tests. Matrix now records `diagnostic_bundle_server_generation=partial-code-level-api-action`, `diagnostic_bundle_api_action=code-level-applied`, `diagnostic_bundle_archive_created=code-level-created`, `diagnostic_bundle_download_served=code-level-download-served`, `diagnostic_bundle_redaction_status=code-level-applied`, `diagnostic_bundle_authz_status=token-required-route-contract`, and `diagnostic_bundle_retention_status=code-level-applied`; installed listener execution was closed later by `0.39.0-admin-smoke`, while host mutation, public trusted signing, and external stable publication remain separate boundaries.
created_at: 2026-05-08T14:30:00+09:00
stale_triggers: route contract, bundle id format, file path containment, redaction token names, retention count/age, Host option parsing, product plan service binary path, installed listener evidence, or public claim status changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Diagnostic Bundle Host Listener Code-Level 2026-05-08

evidence_id: diagnostic-bundle-listener-code-level-2026-05-08
artifact_or_package_version: DesktopNodeHostApplication.cs
runner_version: DesktopNodeHostApplicationTests.cs
host_capability_snapshot: code-level in-process HttpListener evidence; bearer-required diagnostic bundle POST creates a redacted `.bundle.json`; authenticated download serves it with `X-PCV-Diagnostic-Bundle-Id`; host request id header propagates; no installed service listener, no host mutation
exact_command_mode: focused dotnet Host listener test
created_at: 2026-05-08T19:45:00+09:00
stale_triggers: Host listener request id header mapping, diagnostic bundle route auth, download headers, diagnostics root wiring, installed listener evidence, product diagnostics delegation, or public claim status changes
waiver_status: installed-listener-not-run
diagnostic_bundle_server_generation: partial-code-level-api-action
diagnostic_bundle_host_listener_execution: code-level-host-listener
diagnostic_bundle_installed_listener_execution: not-run
diagnostic_bundle_request_id_propagation: code-level-host-header
diagnostic_bundle_api_action: code-level-applied
diagnostic_bundle_archive_created: code-level-created
diagnostic_bundle_download_served: code-level-download-served
diagnostic_bundle_redaction_status: code-level-applied
diagnostic_bundle_authz_status: token-required-route-contract
diagnostic_bundle_retention_status: code-level-applied

### Record: diagnostic bundle Host listener path

evidence_id: diagnostic-bundle-listener-code-level-pass-20260508
route_or_operation: Host listener POST /api/v1/diagnostics/bundles and GET /api/v1/diagnostics/bundles/{bundle_id}/download
route_surface: local-api-listener
risk_tier: tier1-code-level-listener
current_owner: dotnet-host
commit_sha: codex/diagnostic-bundle-listener-evidence
artifact_or_package_version: docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-host-listener-test
fallback_policy: none
identity_requirement: bearer-token-required-listener
network_exposure_gate: loopback-default-lan-token-required
runner_version: dotnet Host tests
host_capability_snapshot: `DesktopNodeHostApplication` forwards `X-PCV-Request-Id` into `DesktopNodeApiRequestProcessor`; bearer-protected create/download over HttpListener preserves request id, redacts `super-secret`, writes `.bundle.json`, and returns `X-PCV-Diagnostic-Bundle-Id`
exact_command_mode: in-process host listener on `http://127.0.0.1:0/`, no installed service or MSI mutation
result: pass
observed_result: RED focused Host test first showed the listener generated a random `req-*` id instead of propagating `listener-diag-create`. GREEN focused verification passed after adding Host request id header forwarding. This closes code-level host listener evidence only; installed service listener execution was closed later by `0.39.0-admin-smoke`, while public trusted signing and external stable publication remain separate boundaries.
created_at: 2026-05-08T19:45:00+09:00
stale_triggers: Host request id headers, API auth enforcement, diagnostic bundle response body/header contract, listener content type handling, diagnostics root option parsing, installed listener smoke, product diagnostics delegation, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Diagnostic Bundle Product Wrapper Code-Level 2026-05-08

evidence_id: diagnostic-bundle-product-wrapper-code-level-2026-05-08
artifact_or_package_version: PcvDesktopNodeProduct.psm1
runner_version: PcvDesktopNodeProduct.Diagnostics.Tests.ps1
host_capability_snapshot: code-level product wrapper evidence; `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics` delegates to `New-PcvDesktopNodeDiagnosticBundle`, returns product wrapper status fields, and writes `product-wrapper-delegation-redacted.json`; no installed service listener, no host mutation
exact_command_mode: focused Pester product diagnostics test
created_at: 2026-05-08T20:05:00+09:00
stale_triggers: product wrapper `CollectDiagnostics` action, diagnostic bundle return fields, `product-wrapper-delegation-redacted.json`, installed listener evidence, or public claim status changes
waiver_status: installed-listener-not-run-non-elevated-shell
actual_execution: code-level-product-wrapper
diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator
diagnostic_bundle_installed_listener_execution: not-run
diagnostic_bundle_installed_listener_blocker: non-elevated-shell
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: diagnostic bundle product wrapper delegation

evidence_id: diagnostic-bundle-product-wrapper-code-level-pass-20260508
route_or_operation: product wrapper CollectDiagnostics action
route_surface: product-operation
risk_tier: tier1-code-level-wrapper
current_owner: packaging
commit_sha: codex/diagnostic-bundle-product-wrapper-evidence
artifact_or_package_version: docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md
target_owner: windows-native-package
implementation_basis: code-level-product-wrapper-test
fallback_policy: none
identity_requirement: local product action
network_exposure_gate: none
runner_version: Pester product diagnostics tests
host_capability_snapshot: `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics` delegates to `New-PcvDesktopNodeDiagnosticBundle`; bundle directory includes `product-wrapper-delegation-redacted.json`; action result records `actual_execution=code-level-product-wrapper` and `diagnostic_bundle_product_wrapper_delegation=code-level-product-action-orchestrator`
exact_command_mode: TestDrive product/data roots, injectable process/runtime-policy runners, no installed service or MSI mutation
result: pass
observed_result: RED focused Pester first showed `actual_execution` was null. GREEN focused verification passed after adding product wrapper delegation status artifact/result fields. Current automation shell reported `IsElevated=false`, so installed service listener evidence remains not-run.
created_at: 2026-05-08T20:05:00+09:00
stale_triggers: CollectDiagnostics orchestration, diagnostic bundle writer, delegation artifact schema, installed listener smoke, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Diagnostic Bundle MSI Service Installed Listener Rerun 2026-05-08 0390

evidence_id: msi-service-installed-listener-rerun-2026-05-08-0390
artifact_or_package_version: 0.39.0-admin-smoke
runner_version: Invoke-PcvBatchSupervisor.ps1 ServiceMsiHyperVAdminSmoke
host_capability_snapshot: elevated installed MSI/service listener evidence; final SCM PathName includes diagnostics root, protected token file, route timeout, request limit, burst, retry-after; protected-token diagnostic bundle POST creates a redacted server-side bundle and authenticated GET downloads it
exact_command_mode: Batch Supervisor ServiceMsiHyperVAdminSmoke plus direct installed listener protected-token POST/GET smoke
created_at: 2026-05-08T21:33:30+09:00
stale_triggers: service-action configure/repair binary path, diagnostics root, protected token file handling, route timeout/request limit args, diagnostic bundle route auth/redaction/download headers, installed MSI lifecycle, or public claim status changes
waiver_status: firewall-trust-store-lan-eventlog-os-gate-not-run-in-this-rerun
diagnostic_bundle_installed_listener_execution: installed-listener-pass
diagnostic_bundle_installed_listener_blocker: none
diagnostic_bundle_service_action_config: code-level-applied
diagnostic_bundle_redaction_status: installed-listener-applied
diagnostic_bundle_authz_status: token-required-route-contract
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

### Record: diagnostic bundle installed listener smoke

evidence_id: diagnostic-bundle-installed-listener-pass-20260508-0390
route_or_operation: installed listener POST /api/v1/diagnostics/bundles and GET /api/v1/diagnostics/bundles/{bundle_id}/download
route_surface: local-api-listener
risk_tier: tier2-installed-service
current_owner: dotnet-host
commit_sha: 8d21654045ed75e81344556fa6444f118c62276a
artifact_or_package_version: docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md
target_owner: windows-native-package
implementation_basis: elevated-installed-listener-smoke
fallback_policy: none
identity_requirement: protected-token-bearer-required
network_exposure_gate: loopback-default
runner_version: Batch Supervisor and protected-token Invoke-WebRequest smoke
host_capability_snapshot: `PureCVisorDesktopNode` final service is `Running`, product manifest version `0.39.0-admin-smoke`, SCM `PathName` includes diagnostic bundle and hardening args, diagnostic bundle create returns HTTP 201, download returns HTTP 200 with matching `X-PCV-Diagnostic-Bundle-Id`, downloaded content contains `[REDACTED]` and not the test secret
exact_command_mode: elevated admin-smoke rerun followed by direct loopback listener POST/GET using `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`; no token value printed
result: pass
observed_result: Batch Supervisor artifact `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390` completed with `ok=true`, `total_steps=1`, `executed_steps=1`, timeout false. Route artifact `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390` records MSI SHA-256 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, final service `Running`, boot time unchanged, `remaining_pcv_vms=[]`, installed listener service evidence JSON, and installed diagnostic bundle listener smoke JSON. Firewall/trust-store/LAN/Event Log OS gate was out of scope for this rerun.
created_at: 2026-05-08T21:33:30+09:00
stale_triggers: installed service binary path, product manifest version/source commit, protected token route auth, diagnostic bundle redaction policy, route timeout/rate-limit args, MSI lifecycle, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Installed Listener OS Mutation Gate 2026-05-08 0390

evidence_id: os-mutation-gate-installed-listener-rerun-2026-05-08-0390
artifact_or_package_version: 0.39.0-admin-smoke
runner_version: Invoke-PcvBatchSupervisor.ps1 OsMutationGate
host_capability_snapshot: focused installed listener follow-up OS mutation gate; firewall enable/remove, LAN listener IP smoke, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore
exact_command_mode: Batch Supervisor OsMutationGate using routeparity input artifact `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`
created_at: 2026-05-08T22:09:00+09:00
stale_triggers: firewall action owner, LAN listener policy, Event Log source action owner, internal trust-store action owner, installed listener binary path, or public claim status changes
waiver_status: public trusted signing excluded and external stable publication not-claimed
host_mutation_performed: true
public_trusted_signing: excluded
external_stable_publication: not-claimed

### Record: installed listener OS mutation gate

evidence_id: os-mutation-gate-installed-listener-pass-20260508-0390
route_or_operation: firewall/trust-store/LAN/Event Log OS gate
route_surface: product-operation
risk_tier: tier3-host-mutation
current_owner: dotnet-host-service-action
commit_sha: 8d21654045ed75e81344556fa6444f118c62276a
artifact_or_package_version: docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md
target_owner: windows-native-package
implementation_basis: elevated-installed-listener-os-mutation-gate
fallback_policy: none
identity_requirement: administrator-opt-in
network_exposure_gate: explicit LAN listener smoke with bearer token
runner_version: Batch Supervisor OsMutationGate
host_capability_snapshot: final service `Running`; LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy and Web assets HTTP 200; firewall final count 0; Event Log source absent; internal Root/TrustedPublisher present; boot time unchanged
exact_command_mode: elevated admin-smoke OS mutation gate; no public publication or public signed update/rollback execution
result: pass
observed_result: Batch Supervisor artifact `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390` completed with `ok=true`, `total_steps=1`, `executed_steps=1`, timeout false. OS artifact `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390` records `host_mutation_performed=true`, `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`, LAN smoke HTTP 200 checks, final firewall count `0`, Event Log source absent, and internal trust certs present.
created_at: 2026-05-08T22:09:00+09:00
stale_triggers: installed listener routeparity input, firewall rule lifecycle, LAN listener policy, Event Log source lifecycle, trust-store lifecycle, or not-claimed boundary values change
waiver_status: public trusted signing excluded and external stable publication not-claimed

## Evidence Group: Service Token Rotation Revoke Preflight 2026-05-08

evidence_id: service-token-rotation-revoke-preflight-2026-05-08
artifact_or_package_version: New-PcvServiceTokenRotationRevokePreflight.ps1
runner_version: PcvServiceTokenRotationRevokePreflight.Tests.ps1
host_capability_snapshot: non-mutating service token rotation revoke plan preview, no token value read, no new token generation, no protected token write, no service reload, no host mutation
exact_command_mode: service token rotation revoke preflight `-PlanOnly`
created_at: 2026-05-08T03:10:00+09:00
stale_triggers: rotation mode, protected token storage boundary, rotation check names, protected token write policy, service reload policy, old-token rejection evidence, audit record policy, or public claim status changes
waiver_status: service-token-mutation-not-run

### Record: service token rotation revoke preflight

evidence_id: service-token-rotation-revoke-preflight-pass-20260508
route_or_operation: service token rotation/revoke preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/service-token-rotation-revoke-preflight
artifact_or_package_version: docs/ga-ready/evidence/service-token-rotation-revoke-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
identity_requirement: none
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: service token rotation/revoke plan preview only, no token value read, no new token generation, no protected token write, no service reload, no host mutation
exact_command_mode: descriptor records service name, protected token path, current token storage, rotation mode, rotation checks, and plan preview
result: pass
observed_result: RED guard first showed missing New-PcvServiceTokenRotationRevokePreflight.ps1 and missing service token rotation revoke evidence linkage. GREEN focused verification passed 6 tool tests. Dry-run artifact `artifacts/service-token-rotation-revoke-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `service_token_rotation_revoke=blocked-by-no-mutation-preflight`, `service_token_mutation=not-run`, `service_token_value_observed=false`, `new_token_value_created=false`, `service_reload_status=not-run`, `old_token_rejection_status=not-run`, and `token_rotation_audit_status=not-run`.
created_at: 2026-05-08T03:10:00+09:00
stale_triggers: rotation check names, plan preview shape, token mutation blocker state, service reload/revoke/audit boundary, or not-claimed boundary values change
waiver_status: service token mutation not executed; public trusted signing and external stable publication not-claimed

## Evidence Group: Built-in TLS Certificate Lifecycle Preflight 2026-05-08

evidence_id: builtin-tls-certificate-lifecycle-preflight-2026-05-08
artifact_or_package_version: New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1
runner_version: PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1
host_capability_snapshot: non-mutating built-in TLS certificate lifecycle plan preview, no private key material, no trust-store mutation, no LAN binding
exact_command_mode: built-in TLS certificate lifecycle preflight `-PlanOnly`
created_at: 2026-05-08T02:20:00+09:00
stale_triggers: certificate subject/bind prefix policy, private key storage policy, trust boundary, rotation/removal contract, LAN binding policy, or public claim status changes
waiver_status: tls-certificate-mutation-not-run

### Record: built-in TLS certificate lifecycle preflight

evidence_id: builtin-tls-certificate-lifecycle-preflight-pass-20260508
route_or_operation: built-in TLS certificate lifecycle preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/builtin-tls-lifecycle-preflight
artifact_or_package_version: docs/ga-ready/evidence/builtin-tls-certificate-lifecycle-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: TLS lifecycle plan preview only, no private key creation, no certificate import/export, no trust-store mutation, no HTTPS/LAN binding, no host mutation
exact_command_mode: descriptor records service name, certificate subject, HTTPS bind prefix, current TLS mode, target TLS mode, lifecycle checks, and plan preview
result: pass
observed_result: RED guard first showed missing New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1 and missing built-in TLS certificate lifecycle evidence linkage. GREEN focused verification passed 6 tool tests. Dry-run artifact `artifacts/builtin-tls-certificate-lifecycle-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `tls_certificate_lifecycle=blocked-by-no-mutation-preflight`, `tls_certificate_mutation=not-run`, `private_key_material_created=false`, `trust_store_mutation=not-run`, and `lan_binding_mutation=not-run`.
created_at: 2026-05-08T02:20:00+09:00
stale_triggers: lifecycle check names, plan preview shape, certificate/trust/binding blocker state, private key material boundary, or not-claimed boundary values change
waiver_status: TLS certificate mutation not executed; public trusted signing and external stable publication not-claimed

## Evidence Group: Windows Event Log Provider Transition Preflight 2026-05-08

evidence_id: windows-event-log-provider-transition-preflight-2026-05-08
artifact_or_package_version: New-PcvWindowsEventLogProviderTransitionPreflight.ps1
runner_version: PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1
host_capability_snapshot: non-mutating Windows Event Log provider transition plan preview, no provider registration, no event write, no host mutation
exact_command_mode: Windows Event Log provider transition preflight `-PlanOnly`
created_at: 2026-05-08T01:50:00+09:00
stale_triggers: provider name/log policy, writer default policy, provider registration/removal contract, event write/query evidence, log volume guard, or public claim status changes
waiver_status: event-log-provider-mutation-not-run

### Record: Windows Event Log provider transition preflight

evidence_id: windows-event-log-provider-transition-preflight-pass-20260508
route_or_operation: Windows Event Log provider transition preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/windows-event-log-provider-preflight
artifact_or_package_version: docs/ga-ready/evidence/windows-event-log-provider-transition-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: provider transition plan preview only, no provider registration/removal, no event write/query, no default writer switch, no host mutation
exact_command_mode: descriptor records service name, provider name, log name, current writer, target writer, transition checks, and plan preview
result: pass
observed_result: RED guard first showed missing New-PcvWindowsEventLogProviderTransitionPreflight.ps1 and missing Windows Event Log provider transition evidence linkage. GREEN focused verification passed 6 tool tests. Dry-run artifact `artifacts/windows-event-log-provider-transition-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `event_log_provider_transition=blocked-by-no-mutation-preflight`, `event_log_provider_mutation=not-run`, and `event_log_write_status=not-run`.
created_at: 2026-05-08T01:50:00+09:00
stale_triggers: transition check names, plan preview shape, provider mutation blocker state, event write/query boundary, or not-claimed boundary values change
waiver_status: provider mutation not executed; public trusted signing and external stable publication not-claimed

## Evidence Group: Windows Credential Manager Transition Preflight 2026-05-08

evidence_id: windows-credential-manager-transition-preflight-2026-05-08
artifact_or_package_version: New-PcvWindowsCredentialManagerTransitionPreflight.ps1
runner_version: PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1
host_capability_snapshot: non-mutating Windows Credential Manager transition plan preview, no token value read, no host mutation
exact_command_mode: Windows Credential Manager transition preflight `-PlanOnly`
created_at: 2026-05-08T01:20:00+09:00
stale_triggers: credential target policy, token storage boundary, transition check names, rollback diagnostics requirement, service reload policy, or public claim status changes
waiver_status: credential-manager-mutation-not-run

### Record: Windows Credential Manager transition preflight

evidence_id: windows-credential-manager-transition-preflight-pass-20260508
route_or_operation: Windows Credential Manager transition preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/windows-credential-manager-preflight
artifact_or_package_version: docs/ga-ready/evidence/windows-credential-manager-transition-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: transition plan preview only, no token value read, no credential write/delete, no service reload, no host mutation
exact_command_mode: descriptor records service name, credential target, current token storage, target token storage, transition checks, and plan preview
result: pass
observed_result: RED guard first showed missing New-PcvWindowsCredentialManagerTransitionPreflight.ps1 and missing Windows Credential Manager transition evidence linkage. GREEN focused verification passed 6 tool tests. Dry-run artifact `artifacts/windows-credential-manager-transition-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `credential_manager_transition=blocked-by-no-mutation-preflight`, `credential_manager_mutation=not-run`, and `token_value_observed=false`.
created_at: 2026-05-08T01:20:00+09:00
stale_triggers: transition check names, plan preview shape, credential mutation blocker state, token value redaction boundary, or not-claimed boundary values change
waiver_status: credential mutation not executed; public trusted signing and external stable publication not-claimed

## Evidence Group: Public Signed Update/Rollback Smoke Preflight 2026-05-08

evidence_id: public-signed-update-rollback-smoke-preflight-2026-05-08
artifact_or_package_version: New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1
runner_version: PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1
host_capability_snapshot: non-mutating clean-host public signed update/rollback smoke plan preview, no host mutation
exact_command_mode: public signed update/rollback smoke preflight `-PlanOnly`
created_at: 2026-05-08T00:55:00+09:00
stale_triggers: catalog channel schema, package URI/SHA-256 policy, baseline version, clean host profile, public signing/publication claim status, or required evidence list changes
waiver_status: public-smoke-blocked

### Record: public signed update/rollback smoke preflight

evidence_id: public-signed-update-rollback-smoke-preflight-pass-20260508
route_or_operation: public signed update/rollback smoke preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/public-signed-update-rollback-preflight
artifact_or_package_version: docs/ga-ready/evidence/public-signed-update-rollback-smoke-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: clean-host smoke plan preview only, no install/update/rollback execution, no public signing, no host mutation
exact_command_mode: descriptor reads catalog schema v1 selected channel; writes summary.json and clean-host smoke plan preview
result: pass
observed_result: RED guard first showed missing New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1 and missing public signed update/rollback preflight evidence linkage. GREEN focused verification passed 7 tool tests. Dry-run artifact `artifacts/public-signed-update-rollback-smoke-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `public_signed_update_rollback_smoke=blocked-by-public-signing-and-publication`, and `clean_host_smoke_status=not-run`.
created_at: 2026-05-08T00:55:00+09:00
stale_triggers: preflight check names, smoke plan shape, public signing/publication blocker state, or not-claimed boundary values change
waiver_status: public trusted signing and external stable publication not-claimed; clean-host smoke not-run

## Evidence Group: Winget Manifest Compliance Preflight 2026-05-08

evidence_id: winget-manifest-compliance-preflight-2026-05-08
artifact_or_package_version: New-PcvWingetManifestCompliancePreflight.ps1
runner_version: PcvWingetManifestCompliancePreflight.Tests.ps1
host_capability_snapshot: non-mutating winget manifest offline compliance, no host mutation
exact_command_mode: winget manifest compliance preflight `-PlanOnly`
created_at: 2026-05-08T00:20:00+09:00
stale_triggers: winget manifest field policy, manifest version, installer URL/SHA-256 policy, installer type, CLI validation/submission status, or public claim status changes
waiver_status: winget-submission-not-submitted

### Record: winget manifest compliance preflight

evidence_id: winget-manifest-compliance-preflight-pass-20260508
route_or_operation: winget manifest compliance preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/winget-manifest-compliance-preflight
artifact_or_package_version: docs/ga-ready/evidence/winget-manifest-compliance-preflight-2026-05-08.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: winget singleton manifest offline compliance only, no winget CLI validation, no submission, no public signing, no host mutation
exact_command_mode: descriptor reads generated winget preview; writes summary.json and normalized manifest metadata
result: pass
observed_result: RED guard first showed missing New-PcvWingetManifestCompliancePreflight.ps1 and missing winget compliance evidence linkage. GREEN focused verification passed 7 tool tests. Dry-run artifact `artifacts/winget-manifest-compliance-preflight-20260508-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `winget_submission=not-submitted`, and `validation_status=offline-compliance-pass`.
created_at: 2026-05-08T00:20:00+09:00
stale_triggers: compliance check names, normalized manifest shape, HTTPS/SHA policy, installer type policy, or not-claimed boundary values change
waiver_status: winget CLI validation and repository submission not executed; public trusted signing and external stable publication not-claimed

## Evidence Group: MSIX Packaging Feasibility Preflight 2026-05-07

evidence_id: msix-packaging-feasibility-preflight-2026-05-07
artifact_or_package_version: New-PcvMsixPackagingFeasibilityPreflight.ps1
runner_version: PcvMsixPackagingFeasibilityPreflight.Tests.ps1
host_capability_snapshot: non-mutating MSIX package manifest preview, no host mutation
exact_command_mode: MSIX packaging feasibility preflight `-PlanOnly`
created_at: 2026-05-07T23:00:00+09:00
stale_triggers: package manifest preview shape, service packaging design state, install/update/remove evidence, capability boundary, or public claim status changes
waiver_status: msix-feasibility-blocked

### Record: MSIX packaging feasibility preflight

evidence_id: msix-packaging-feasibility-preflight-pass-20260507
route_or_operation: MSIX packaging feasibility preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/msix-feasibility-preflight
artifact_or_package_version: docs/ga-ready/evidence/msix-packaging-feasibility-preflight-2026-05-07.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: MSIX package manifest preview only, no package build, no install/update/remove smoke, no public signing, no host mutation
exact_command_mode: descriptor reads `.publication.json`; writes summary.json and `AppxManifest.preview.xml`
result: pass
observed_result: RED guard first showed missing New-PcvMsixPackagingFeasibilityPreflight.ps1 and missing MSIX feasibility evidence linkage. GREEN focused verification passed 6 tool tests and 29 tool+documentation guard tests. Dry-run artifact `artifacts/msix-packaging-feasibility-preflight-20260507-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `msix=feasibility-blocked-by-service-packaging-design`, package identity `PureCVisor.DesktopNode`, and MSIX version `0.39.0.0`.
created_at: 2026-05-07T23:00:00+09:00
stale_triggers: MSIX check names, package identity preview, service packaging blocker list, or not-claimed boundary values change
waiver_status: MSIX build blocked by service packaging design; public trusted signing and external stable publication not-claimed

## Evidence Group: Burn Bootstrapper Preflight 2026-05-07

evidence_id: burn-bootstrapper-preflight-2026-05-07
artifact_or_package_version: New-PcvBurnBootstrapperPreflight.ps1
runner_version: PcvBurnBootstrapperPreflight.Tests.ps1
host_capability_snapshot: non-mutating WiX Burn authoring preview, no host mutation
exact_command_mode: Burn bootstrapper preflight `-PlanOnly`
created_at: 2026-05-07T22:35:00+09:00
stale_triggers: publication descriptor schema, MSI URL/SHA-256 policy, bundle authoring preview shape, chained lifecycle status, or public claim status changes
waiver_status: burn-bootstrapper-not-built

### Record: Burn bootstrapper preflight

evidence_id: burn-bootstrapper-preflight-pass-20260507
route_or_operation: Burn bootstrapper preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/burn-bootstrapper-preflight
artifact_or_package_version: docs/ga-ready/evidence/burn-bootstrapper-preflight-2026-05-07.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: WiX Burn authoring preview only, no bundle build, no chained lifecycle smoke, no public signing, no host mutation
exact_command_mode: descriptor reads `.publication.json` and HTTPS MSI URL; writes summary.json and `PureCVisorDesktopNode.Bundle.preview.wxs`
result: pass
observed_result: RED guard first showed missing New-PcvBurnBootstrapperPreflight.ps1 and missing Burn bootstrapper evidence linkage. GREEN focused verification passed 7 tool tests and 29 tool+documentation guard tests. Dry-run artifact `artifacts/burn-bootstrapper-preflight-20260507-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `burn_bootstrapper=not-built`, bundle upgrade code `{8F455BB4-640E-47A2-A982-338C7A6318B5}`, and WiX Burn authoring preview output only.
created_at: 2026-05-07T22:35:00+09:00
stale_triggers: Burn check names, authoring preview shape, MSI chain hash binding, or not-claimed boundary values change
waiver_status: Burn bundle not-built; public trusted signing and external stable publication not-claimed

## Evidence Group: Updater Catalog Publication Preflight 2026-05-07

evidence_id: updater-catalog-publication-preflight-2026-05-07
artifact_or_package_version: New-PcvUpdaterCatalogPublicationPreflight.ps1
runner_version: PcvUpdaterCatalogPublicationPreflight.Tests.ps1
host_capability_snapshot: non-mutating catalog publication preview, no host mutation
exact_command_mode: updater catalog publication preflight `-PlanOnly`
created_at: 2026-05-07T22:00:00+09:00
stale_triggers: catalog schema, selected channel validation, public catalog URI policy, package URI/SHA-256 policy, catalog publication state, or public claim status changes
waiver_status: public-catalog-publication-not-run

### Record: updater catalog publication preflight

evidence_id: updater-catalog-publication-preflight-pass-20260507
route_or_operation: updater catalog publication preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/catalog-publication-preflight
artifact_or_package_version: docs/ga-ready/evidence/updater-catalog-publication-preflight-2026-05-07.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: catalog publication preview only, no external endpoint validation, no public signing, no host mutation
exact_command_mode: descriptor reads catalog schema v1, selected HTTPS channel, public HTTPS catalog URI; writes summary.json, preview catalog, and SHA-256 sidecar
result: pass
observed_result: RED guard first showed missing New-PcvUpdaterCatalogPublicationPreflight.ps1 and missing updater catalog publication preflight evidence linkage. GREEN focused verification passed 8 tool tests and 29 tool+documentation guard tests. Dry-run artifact `artifacts/updater-catalog-publication-preflight-20260507-dryrun` keeps `actual_execution=not-run`, `host_mutation_performed=false`, `catalog_publication=not-published`, selected channel `stable`, and preview SHA-256 `ef222145302846806565317b43ac8f5a311e516a58bb99020e38da515561ec73`.
created_at: 2026-05-07T22:00:00+09:00
stale_triggers: catalog publication preview shape, publication checks, HTTPS-only publication policy, or not-claimed boundary values change
waiver_status: catalog publication not-published; public trusted signing and external stable publication not-claimed

## Evidence Group: Public Distribution Readiness Preflight 2026-05-07

evidence_id: public-distribution-readiness-preflight-2026-05-07
artifact_or_package_version: New-PcvPublicDistributionReadiness.ps1
runner_version: PcvPublicDistributionReadiness.Tests.ps1
host_capability_snapshot: non-mutating winget manifest preview and signing/publication input descriptor
exact_command_mode: readiness preflight `-PlanOnly`
created_at: 2026-05-07T21:20:00+09:00
stale_triggers: winget manifest preview fields, installer URL/SHA-256 handling, signing provider inputs, submission status, or public claim status changes
waiver_status: public-distribution-not-run

### Record: public distribution readiness preflight

evidence_id: public-distribution-readiness-preflight-pass-20260507
route_or_operation: public distribution readiness preflight
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/public-distribution-readiness
artifact_or_package_version: docs/ga-ready/evidence/public-distribution-readiness-preflight-2026-05-07.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: winget manifest preview only, no validation execution, no submission, no host mutation
exact_command_mode: descriptor reads `.publication.json`, writes summary.json and winget manifest preview, keeps `winget validate` manual and submission not-submitted
result: pass
observed_result: RED guard first showed missing New-PcvPublicDistributionReadiness.ps1 and missing readiness evidence linkage. GREEN focused verification passed 26 tests and dry-run artifact `artifacts/public-distribution-readiness-preflight-20260507-dryrun` recorded `actual_execution=not-run`, `host_mutation_performed=false`, and `winget_submission=not-submitted`.
created_at: 2026-05-07T21:20:00+09:00
stale_triggers: readiness schema, winget manifest fields, MSIX feasibility blocker, public trusted signing boundary, or external stable publication boundary changes
waiver_status: winget submission not-submitted; public trusted signing and external stable publication not-claimed

## Evidence Group: Public Distribution Operations Expansion Phase 1 2026-05-07

evidence_id: public-distribution-operations-expansion-phase1-2026-05-07
artifact_or_package_version: ADR-0005 public-distribution-operations-expansion-candidate
runner_version: PcvPublicDistributionDescriptor.Tests.ps1
host_capability_snapshot: non-mutating dry-run descriptor, no host mutation
exact_command_mode: New-PcvPublicDistributionDescriptor.ps1 -PlanOnly
created_at: 2026-05-07T18:00:00+09:00
stale_triggers: ADR-0005 status, PUBLIC_DISTRIBUTION_GATE_MATRIX row state, descriptor schema, public trusted signing claim, or external stable publication claim changes
waiver_status: public-distribution-not-run

### Record: ADR-0005 candidate gate descriptor

evidence_id: public-distribution-operations-expansion-phase1-descriptor-20260507
route_or_operation: public distribution and operations expansion gate descriptor
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: packaging
commit_sha: codex/public-distribution-phase1
artifact_or_package_version: docs/ga-ready/evidence/public-distribution-operations-expansion-phase1-2026-05-07.md
target_owner: windows-native-package
implementation_basis: dry-run-descriptor
fallback_policy: none
promotion_state: proposed-candidate
admin_smoke_required: none
release_gate: release-approval-required-before-public-claim
network_exposure_gate: none
runner_version: Pester descriptor and documentation guard
host_capability_snapshot: descriptor-only, actual_execution not-run, host_mutation_performed false
exact_command_mode: descriptor lists public trusted signing, Burn bootstrapper, MSIX, winget manifest, updater catalog publication, public signed update/rollback smoke, Windows Credential Manager, Event Log provider, TLS, token rotation, diagnostics, timeout/rate-limit gates
result: pass
observed_result: RED guard first showed missing New-PcvPublicDistributionDescriptor.ps1 and missing ADR-0005/PUBLIC_DISTRIBUTION_GATE_MATRIX docs. GREEN focused verification passed 25 tests for descriptor contract and documentation synchronization.
created_at: 2026-05-07T18:00:00+09:00
stale_triggers: descriptor gate names, command_plan inputs, not-claimed boundary values, or ADR adoption status changes
waiver_status: public trusted signing and external stable publication not-claimed

## Evidence Group: Packaging Publication Descriptor 2026-05-07

evidence_id: packaging-publication-descriptor-2026-05-07
artifact_or_package_version: packaging/windows-desktop-node/installer/build.ps1
runner_version: Pester installer build plan tests
host_capability_snapshot: non-mutating installer artifact descriptor validation, no host mutation
exact_command_mode: installer dry-run and fake WiX build tests
created_at: 2026-05-07T17:15:00+09:00
stale_triggers: publication descriptor schema, sidecar naming, provenance publication object, artifact SHA fields, or not-claimed/not-built boundary values change
waiver_status: none

### Record: packaging publication descriptor sidecar

evidence_id: packaging-publication-descriptor-pass-20260507
route_or_operation: packaging publication descriptor
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: product-wrapper
commit_sha: codex/packaging-publication-descriptor
artifact_or_package_version: docs/ga-ready/evidence/packaging-publication-descriptor-2026-05-07.md
target_owner: windows-native-package
implementation_basis: package-contract
fallback_policy: none
promotion_state: current-native
admin_smoke_required: none
release_gate: release-approval-required
network_exposure_gate: none
runner_version: Pester installer build plan tests
host_capability_snapshot: dry-run and fake WiX build only, no service/MSI/firewall/trust-store/LAN/update mutation
exact_command_mode: dry-run plan exposes publication_path/publication boundary; fake WiX build writes .publication.json and links descriptor MSI SHA-256 to provenance MSI SHA-256
result: pass
observed_result: RED guard first showed missing publication_path and null publication_path. GREEN installer plan tests passed 17 tests and verified internal-artifact-descriptor-only boundary, public/external publication not-claimed, Burn/MSIX not-built, winget not-generated, catalog publication not-published.
created_at: 2026-05-07T17:15:00+09:00
stale_triggers: publication descriptor schema, sidecar output path, artifact hash linkage, provenance publication object, or future publication claim status changes
waiver_status: public-publication-not-run

## Evidence Group: Full Transactional Filesystem Rollback 2026-05-07

evidence_id: full-transactional-filesystem-rollback-2026-05-07
artifact_or_package_version: packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1
runner_version: Pester product wrapper plan/invoke tests
host_capability_snapshot: non-mutating code-level update rollback validation, no host mutation
exact_command_mode: product wrapper plan and invoke injectable tests
created_at: 2026-05-07T16:45:00+09:00
stale_triggers: transaction journal full_transactional_filesystem flag, backup-completed failure catch, rollback restore result shape, failed-rollback-failed status, or update executed step names change
waiver_status: none

### Record: update filesystem transactional rollback

evidence_id: full-transactional-filesystem-rollback-pass-20260507
route_or_operation: Invoke-PcvDesktopNodeProduct.ps1 -Action Update
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: codex/full-transactional-rollback
artifact_or_package_version: docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md
target_owner: windows-native-package
implementation_basis: package-contract
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: Pester product wrapper plan/invoke tests
host_capability_snapshot: injectable runner only, no service/MSI/firewall/trust-store/LAN/update mutation
exact_command_mode: plan exposes full_transactional_filesystem true; backup-completed copy failure triggers rollback.restore; journal records failed-rolled-back or failed-rollback-failed with rollback_result
result: pass
observed_result: RED guard first showed full_transactional_filesystem false and no rollback.restore after copy failure. GREEN product wrapper plan/invoke tests passed 71 tests and verified copy-stage filesystem rollback after backup.
created_at: 2026-05-07T16:45:00+09:00
stale_triggers: update stage ordering, backup completion marker, rollback restore behavior, journal status names, or product root restore result fields
waiver_status: destructive-installed-smoke-not-run

## Evidence Group: Full Updater Catalog Channel 2026-05-07

evidence_id: full-updater-catalog-channel-2026-05-07
artifact_or_package_version: packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1, packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1
runner_version: Pester product wrapper plan/invoke tests
host_capability_snapshot: non-mutating code-level catalog/channel resolver validation, no host mutation
exact_command_mode: product wrapper plan and invoke injectable tests
created_at: 2026-05-07T16:10:00+09:00
stale_triggers: update catalog schema, channel entry shape, publication claim fields, package SHA-256 requirement, source/catalog conflict handling, or update transaction journal catalog field changes
waiver_status: none

### Record: updater catalog channel resolver

evidence_id: full-updater-catalog-channel-pass-20260507
route_or_operation: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri -UpdateChannel
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: codex/full-updater-catalog
artifact_or_package_version: docs/ga-ready/evidence/full-updater-catalog-channel-2026-05-07.md
target_owner: windows-native-package
implementation_basis: package-contract
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: Pester product wrapper plan/invoke tests
host_capability_snapshot: injectable runner only, no service/MSI/firewall/trust-store/LAN/update mutation
exact_command_mode: catalog URI/channel plan metadata, file/HTTPS JSON catalog channel resolution, package URI/SHA-256 handoff to source gate, missing channel/schema structured block before service stop, update journal update_catalog field
result: pass
observed_result: RED guard first exposed unsupported schema escaping as PCV_PRODUCT_ACTION_FAILED. GREEN product wrapper plan/invoke tests passed 70 tests and verified catalog-channel resolution before update-source preflight/service stop.
created_at: 2026-05-07T16:10:00+09:00
stale_triggers: catalog policy, catalog schema version, selected channel validation, SHA-256 validation, source/catalog conflict policy, or transaction journal shape changes
waiver_status: destructive-installed-smoke-not-run

## Evidence Group: Web Console Token Rotation UX 2026-05-07

evidence_id: web-console-token-rotation-ux-2026-05-07
artifact_or_package_version: web/index.html, web/src/served-app.ts, web/app.js, web/scripts/verify-browser-fixture.mjs
runner_version: TypeScript served asset build, Pester static guard, browser fixture parity
host_capability_snapshot: non-mutating Web Console read-only/operator handoff fixture validation, no host mutation
exact_command_mode: web static Pester plus npm/Web Console fixture verification
created_at: 2026-05-07T15:20:00+09:00
stale_triggers: token-rotation-panel id, renderTokenRotation, protected token file path, browser token clear behavior, or redaction wording changes
waiver_status: none

### Record: token rotation operator UX

evidence_id: web-console-token-rotation-ux-pass-20260507
route_or_operation: Troubleshooting Token Rotation operator UX
route_surface: web-console
risk_tier: tier1-read-only
current_owner: typescript-web-console
commit_sha: codex/token-rotation-ux
artifact_or_package_version: docs/ga-ready/evidence/web-console-token-rotation-ux-2026-05-07.md
target_owner: web-dashboard-troubleshooting
implementation_basis: read-only/operator handoff panel plus browser token clear action
fallback_policy: product-wrapper/operator-owned-service-token-replacement
promotion_state: beta-readiness-pass
admin_smoke_required: none
release_gate: none
network_exposure_gate: loopback/static-fixture-only
runner_version: web static Pester and Node browser fixture
host_capability_snapshot: browser fixture only, no service/MSI/firewall/trust-store/LAN/update mutation
exact_command_mode: rendered Troubleshooting panel includes Token Rotation, rotation handoff, protected token file root, Clear browser token, no service token mutation, no host mutation, token value and Authorization header redaction boundary
result: pass
observed_result: RED guard failed before implementation on missing token-rotation-panel; GREEN guard passed 29 web static tests. Browser fixture verifies Token Rotation, rotation handoff, Clear browser token, no service token mutation, browser token empty, and protected token file root in rendered output.
created_at: 2026-05-07T15:20:00+09:00
stale_triggers: token rotation panel, browser token clearing, protected token file path, runtime policy token storage field, or browser fixture required ids
waiver_status: destructive-beta-not-run

## Evidence Group: API/Web Retention Pagination Hardening 2026-05-07

evidence_id: api-web-retention-pagination-hardening-2026-05-07
artifact_or_package_version: src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs, src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs, web/src/served-app.ts, web/app.js
runner_version: dotnet API focused tests, TypeScript served asset build, Pester static guard, browser fixture parity
host_capability_snapshot: non-mutating API/Web fixture validation, no host mutation
exact_command_mode: API request processor tests plus Web Console static/browser fixture verification
created_at: 2026-05-07T14:55:00+09:00
stale_triggers: GET /api/v1/jobs response shape, job retention policy, Web Console Activity loader, TypeScript API type, browser fixture, or job store terminal status changes
waiver_status: none

### Record: job list pagination and retention

evidence_id: api-web-retention-pagination-hardening-pass-20260507
route_or_operation: GET /api/v1/jobs
route_surface: local-api, web-console-activity
risk_tier: tier1-read-only
current_owner: dotnet-native-api-and-typescript-web-console
commit_sha: codex/retention-pagination-hardening
artifact_or_package_version: docs/ga-ready/evidence/api-web-retention-pagination-hardening-2026-05-07.md
target_owner: local-api-job-runtime
implementation_basis: bounded limit/offset pagination with additive metadata and terminal job retention
fallback_policy: none
promotion_state: beta-readiness-pass
admin_smoke_required: none
release_gate: none
network_exposure_gate: loopback/static-fixture-only
runner_version: dotnet focused API tests and Web static/browser fixture
host_capability_snapshot: in-memory request processor and browser fixture only, no service/MSI/firewall/trust-store/LAN/update mutation
exact_command_mode: focused API tests verify limit/offset metadata, invalid query rejection, terminal retention cap 500, active job preservation, persisted store pruning on load; Web fixture verifies Activity renders pagination/retention metadata
result: pass
observed_result: RED guards failed before implementation on missing pagination metadata and Web Activity unpaged call; GREEN focused API tests passed 9 tests and Web static tests passed 28 tests. Browser fixture verifies Pagination, next_offset, max_terminal_jobs in rendered output.
created_at: 2026-05-07T14:55:00+09:00
stale_triggers: pagination query parser, max limit 200, terminal retention cap 500, active job preservation, Activity page summary, or browser fixture metadata changes
waiver_status: destructive-beta-not-run

## Evidence Group: Web Console Diagnostic Bundle UI 2026-05-07

evidence_id: web-console-diagnostic-bundle-ui-2026-05-07
artifact_or_package_version: web/index.html, web/src/served-app.ts, web/app.js, web/scripts/verify-browser-fixture.mjs
runner_version: TypeScript served asset build, Pester static guard, browser fixture parity
host_capability_snapshot: non-mutating Web Console read-only fixture validation, no host mutation
exact_command_mode: web static Pester plus npm/Web Console parity verification
created_at: 2026-05-07T04:20:00+09:00
stale_triggers: Diagnostic Bundle panel copy, CollectDiagnostics operator handoff, TypeScript served asset, browser fixture, or redaction boundary changes
waiver_status: none

### Record: diagnostic bundle operator handoff UI

evidence_id: web-console-diagnostic-bundle-ui-pass-20260507
route_or_operation: Troubleshooting Diagnostic Bundle operator handoff
route_surface: web-console
risk_tier: tier1-read-only
current_owner: typescript-web-console
commit_sha: codex/web-diagnostic-bundle-ui
artifact_or_package_version: docs/ga-ready/evidence/web-console-diagnostic-bundle-ui-2026-05-07.md
target_owner: web-dashboard-troubleshooting
implementation_basis: read-only Web Console panel for existing product wrapper CollectDiagnostics action
fallback_policy: product-wrapper-manual-collection
promotion_state: beta-readiness-pass
admin_smoke_required: none
release_gate: none
network_exposure_gate: loopback/static-fixture-only
runner_version: web static Pester and Node browser fixture
host_capability_snapshot: browser fixture only, no service/MSI/firewall/trust-store/LAN/update mutation
exact_command_mode: rendered Troubleshooting panel includes diagnostics root, CollectDiagnostics action name, operator handoff, no host mutation, token value and Authorization header redaction boundary
result: pass
observed_result: RED guard failed before implementation on missing diagnostics-panel; GREEN guard passed 28 web static tests. Browser fixture verifies Diagnostic Bundle, CollectDiagnostics, operator handoff, no host mutation, token values, Authorization headers, and diagnostics root in rendered output.
created_at: 2026-05-07T04:20:00+09:00
stale_triggers: diagnostics-panel id, renderDiagnosticsBundle, product wrapper action naming, redaction wording, or browser fixture required ids
waiver_status: destructive-beta-not-run

## Evidence Group: Web Console Network Inventory View 2026-05-07

evidence_id: web-console-network-inventory-view-2026-05-07
artifact_or_package_version: web/index.html, web/src/served-app.ts, web/app.js, web/generated/parity/static-asset-parity.manifest.json
runner_version: TypeScript served asset build, Pester static guard, browser fixture parity
host_capability_snapshot: non-mutating Web Console read-only fixture validation, no host mutation
exact_command_mode: web static Pester plus npm/Web Console parity verification
created_at: 2026-05-07T03:30:00+09:00
stale_triggers: network inventory route shape, TypeScript served asset, user-visible fixture, browser fixture, parity manifest, or Web Console navigation changes
waiver_status: none

### Record: network inventory web view

evidence_id: web-console-network-inventory-view-pass-20260507
route_or_operation: GET /api/v1/network/inventory Web Console view
route_surface: web-console
risk_tier: tier1-read-only
current_owner: typescript-web-console
commit_sha: codex/web-network-inventory-page
artifact_or_package_version: docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md
target_owner: web-dashboard-network-inventory
implementation_basis: read-only Web Console view over existing C# native network.inventory route
fallback_policy: none
promotion_state: beta-readiness-pass
admin_smoke_required: none
release_gate: none
network_exposure_gate: loopback/static-fixture-only
runner_version: web static guard, served app build, generated parity manifest, Node vm browser fixture
host_capability_snapshot: switch inventory fixture includes Default Switch and fixture-ethernet; mutation mode renders read-only
exact_command_mode: npm run build:served --prefix web; npm run generate:parity --prefix web; npm test --prefix web; npm run verify:parity --prefix web; npm run browser:fixture --prefix web; Invoke-Pester -Path web/tests; documentation sync Pester; git diff --check
result: pass
observed_result: RED guard failed before implementation on missing network nav; GREEN guard passed 27 web tests, npm test passed, verify:parity passed with browser fixture, standalone browser fixture passed, documentation sync passed 18 tests, git diff --check passed
created_at: 2026-05-07T03:30:00+09:00
stale_triggers: network inventory field names, fixture values, sidebar view list, browser fixture required ids, or served asset regeneration changes
waiver_status: destructive-beta-not-run

## Evidence Group: Beta Web Dashboard Smoke 2026-05-07

evidence_id: beta-web-dashboard-smoke-2026-05-07
artifact_or_package_version: artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743
runner_version: Batch Supervisor WebRegression profile
host_capability_snapshot: non-mutating Web Console static/read-only fixture validation, no host mutation
exact_command_mode: web Pester, npm test, npm verify:parity, node --check under Batch Supervisor
created_at: 2026-05-07T02:58:00+09:00
stale_triggers: WebRegression profile, TypeScript served asset, browser fixture, web static route contract, or Batch Supervisor Windows npm command resolution changes
waiver_status: none

### Record: beta web dashboard smoke

evidence_id: beta-web-dashboard-smoke-pass-20260507
route_or_operation: Web Dashboard Beta-0 read-only smoke
route_surface: web-console
risk_tier: tier1-read-only
current_owner: typescript-web-console
commit_sha: fd4f854646fc159d54f7578230f00c51f80e201f
artifact_or_package_version: artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743
target_owner: web-dashboard-ops-cockpit
implementation_basis: batch-supervised-web-regression
fallback_policy: none
promotion_state: beta-readiness-pass
admin_smoke_required: none
release_gate: none
network_exposure_gate: loopback/static-fixture-only
runner_version: WebRegression profile after absolute npm.cmd fix
host_capability_snapshot: total_steps 4, executed_steps 4, failed_step_id null
exact_command_mode: Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743/manifest.json
result: pass
observed_result: web Pester 26 tests PASS, npm test PASS, verify:parity PASS, node --check PASS
created_at: 2026-05-07T02:58:00+09:00
stale_triggers: dashboard fixture coverage, parity manifest, browser fixture, or web bundle changes
waiver_status: destructive-beta-not-run

## Evidence Group: Product Update/Rollback 0.38.8

evidence_id: product-update-rollback-mutation-2026-05-07-0388
artifact_or_package_version: artifacts/product-update-rollback-mutation-20260507-0388, artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass
runner_version: installer build.ps1 0.38.8-admin-smoke AllowUnsignedDev, Invoke-PcvDesktopNodeProduct.ps1 Update/Rollback
host_capability_snapshot: elevated shell rerun, installed PureCVisorDesktopNode final service Running, product root manifest restored to 0.38.6-admin-smoke
exact_command_mode: Build completed; first non-elevated attempt blocked; elevated update to 0.38.8-admin-smoke and rollback to 0.38.6-admin-smoke completed
created_at: 2026-05-07T03:00:00+09:00
stale_triggers: installer build, product update/rollback service stop, update transaction journal, or admin preflight behavior changes
waiver_status: none

### Record: 0.38.8 update rollback mutation elevated pass

evidence_id: update-rollback-mutation-pass-20260507-0388
route_or_operation: product update and rollback mutation
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: fd4f854646fc159d54f7578230f00c51f80e201f
artifact_or_package_version: artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass
target_owner: product-wrapper-update-rollback
implementation_basis: manifest-first update/rollback action with update transaction journal
fallback_policy: none
promotion_state: installed-destructive-pass
admin_smoke_required: explicit-admin-opt-in-plus-elevated-shell
release_gate: none
network_exposure_gate: none
runner_version: Invoke-PcvDesktopNodeProduct.ps1 Update/Rollback, version 0.38.8-admin-smoke payload
host_capability_snapshot: is_admin true, final service Running, final product root manifest 0.38.6-admin-smoke, failed root manifest 0.38.8-admin-smoke
exact_command_mode: Update from artifacts/product-update-rollback-mutation-20260507-0388/payload to 0.38.8-admin-smoke with -TimeoutSec 60, then Rollback -TimeoutSec 60
result: pass
observed_result: Update exit 0 with service stop/wait, backup-product-root, copy, config-migration dry-run, service start, health 200, transaction journal succeeded/health. Rollback exit 0 restored 0.38.6-admin-smoke, preserved 0.38.8-admin-smoke as DesktopNode.failed, final service Running, boot time unchanged, host_mutation_performed=true
created_at: 2026-05-07T03:00:00+09:00
stale_triggers: product update/rollback service stop, backup/restore, update journal, health check, or failed root diagnostics behavior changes
waiver_status: none

### Record: 0.38.8 update rollback mutation attempt blocked

evidence_id: update-rollback-mutation-blocked-20260507-0388
route_or_operation: product update and rollback mutation
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: fd4f854646fc159d54f7578230f00c51f80e201f
artifact_or_package_version: artifacts/product-update-rollback-mutation-20260507-0388
target_owner: product-wrapper-update-rollback
implementation_basis: manifest-first update/rollback action with update transaction journal
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in-plus-elevated-shell
release_gate: none
network_exposure_gate: none
runner_version: Invoke-PcvDesktopNodeProduct.ps1 Update/Rollback, version 0.38.8-admin-smoke payload
host_capability_snapshot: is_admin false, final service Running, product root manifest 0.38.6-admin-smoke
exact_command_mode: Update from artifacts/product-update-rollback-mutation-20260507-0388/payload to 0.38.8-admin-smoke, then Rollback -TimeoutSec 5
result: blocked
observed_result: Build PASS, update failed at sc.exe stop exit 5 after update-transaction.begin, rollback failed with PCV_PRODUCT_SERVICE_STOP_TIMEOUT, host_mutation_performed=false
created_at: 2026-05-07T02:59:00+09:00
stale_triggers: product update/rollback service stop, backup/restore, update journal, or elevation preflight changes
waiver_status: superseded-by-elevated-pass

## Evidence Group: Update Transaction Journal Diagnostics

evidence_id: update-transaction-journal-diagnostics-2026-05-07
artifact_or_package_version: docs/ga-ready/evidence/update-transaction-journal-diagnostics-2026-05-07.md
runner_version: Invoke-PcvDesktopNodeProduct.ps1 Update, PcvDesktopNodeProduct.psm1 update transaction journal
host_capability_snapshot: code-level Pester fixtures only, no service/MSI/firewall/trust-store/LAN/Event Log mutation
exact_command_mode: product plan/update/diagnostics Pester tests with injectable service runner and TestDrive data roots
created_at: 2026-05-07T02:15:00+09:00
stale_triggers: update plan path contract, update action ordering, rollback diagnostics, diagnostic bundle source manifest, or transaction journal schema changes
waiver_status: code-level-only

### Record: update transaction journal diagnostics

evidence_id: update-transaction-journal-diagnostics-code-level-20260507
route_or_operation: product update transaction journal diagnostics
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: codex/update-transaction-journal
artifact_or_package_version: docs/ga-ready/evidence/update-transaction-journal-diagnostics-2026-05-07.md
target_owner: product-wrapper-update-rollback
implementation_basis: code-level transaction journal diagnostics
fallback_policy: none
promotion_state: transaction-journal-diagnostics-code-level-partial
admin_smoke_required: installed-destructive-update-rollback-future
release_gate: none
network_exposure_gate: none
runner_version: PcvDesktopNodeProduct.Plan/Invoke/Diagnostics Pester
host_capability_snapshot: no host mutation, no service stop/start, no MSI or OS mutation
exact_command_mode: Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1, packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1, packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1
result: pass
observed_result: Tests Passed 82, update-transaction.begin before service.stop, success status succeeded, rollback failure status failed-rolled-back, diagnostic bundle source update_transaction_journal
created_at: 2026-05-07T02:15:00+09:00
stale_triggers: journal path, journal status/stage values, rollback_result shape, diagnostic source name, or full transactional rollback future boundary changes
waiver_status: full-transactional-rollback-not-claimed

## Evidence Group: Signed Build and Blocked Host Mutation Attempt 0.38.7

evidence_id: host-mutation-signed-build-attempt-2026-05-07-0387
artifact_or_package_version: artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387, artifacts/batch-runs/full-admin-host-mutation-gate-20260507-0387, artifacts/product-update-rollback-mutation-20260507-0387
runner_version: installer build.ps1 0.38.7-rc.1 RequireSigned, Batch Supervisor FullAdminHostMutationGate 0.38.7-admin-smoke, Invoke-PcvDesktopNodeProduct.ps1 Update/Rollback
host_capability_snapshot: non-elevated Codex shell, installed PureCVisorDesktopNode final service Running, product root manifest 0.38.6-admin-smoke, previous product root absent
exact_command_mode: InternalEnterprise RequireSigned MSI build and SignTool verify completed; full admin host mutation, update, rollback actual actions attempted from non-elevated shell and blocked before mutation
created_at: 2026-05-07T01:35:00+09:00
stale_triggers: signing trust model, build provenance, admin preflight guard, Batch Supervisor admin gate, product update/rollback service stop behavior, or full admin mutation gate text changes
waiver_status: blocked-non-elevated-shell

### Record: 0.38.7 internal RequireSigned build

evidence_id: internal-requiresigned-build-20260507-0387
route_or_operation: internal enterprise RequireSigned MSI build
route_surface: release-evidence
risk_tier: signed-build
current_owner: installer-build
commit_sha: dd4e7379c515b05eb82038404519c9e63f54bf51
artifact_or_package_version: artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387
target_owner: installer-build
implementation_basis: WiX MSI build plus SignTool verify
fallback_policy: none
promotion_state: signed-build-pass
admin_smoke_required: signing-material-opt-in
release_gate: internal-requiresigned
network_exposure_gate: none
runner_version: packaging/windows-desktop-node/installer/build.ps1, version 0.38.7-rc.1
host_capability_snapshot: CurrentUser internal signer/trust available, Authenticode Valid, SignTool verify exit 0
exact_command_mode: build.ps1 -SigningMode RequireSigned -SigningTrustModel InternalEnterprise -CertificateThumbprint 8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6
result: pass
observed_result: MSI SHA-256 c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602, provenance commit dd4e7379c515b05eb82038404519c9e63f54bf51, public trusted signing excluded, external stable publication not-claimed
created_at: 2026-05-07T01:26:30+09:00
stale_triggers: signing mode, trust model, SignTool verification, MSI payload contract, or provenance schema changes
waiver_status: none

### Record: 0.38.7 full admin host mutation attempt blocked

evidence_id: full-admin-host-mutation-gate-blocked-20260507-0387
route_or_operation: full admin host mutation gate
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: batch-supervisor-admin-gate
commit_sha: dd4e7379c515b05eb82038404519c9e63f54bf51
artifact_or_package_version: artifacts/batch-runs/full-admin-host-mutation-gate-20260507-0387
target_owner: dotnet-native-service-msi-hyperv-os-mutation
implementation_basis: Batch Supervisor FullAdminHostMutationGate preflight
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in-plus-elevated-shell
release_gate: none
network_exposure_gate: LAN opt-in requested but not executed
runner_version: Invoke-PcvBatchSupervisor.ps1, version 0.38.7-admin-smoke
host_capability_snapshot: -AllowHostMutation true, is_admin false
exact_command_mode: FullAdminHostMutationGate manifest with Service/MSI/Hyper-V and OS mutation steps
result: blocked
observed_result: PCV_BATCH_ADMIN_REQUIRED before service-msi-hyperv-admin-smoke, steps_started 0, host_mutation_performed=false
created_at: 2026-05-07T01:27:31+09:00
stale_triggers: Batch Supervisor admin preflight or host mutation gate step contract changes
waiver_status: non-elevated-shell

### Record: 0.38.7 update rollback mutation attempt blocked

evidence_id: update-rollback-mutation-blocked-20260507-0387
route_or_operation: product update and rollback mutation
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: dd4e7379c515b05eb82038404519c9e63f54bf51
artifact_or_package_version: artifacts/product-update-rollback-mutation-20260507-0387
target_owner: product-wrapper-update-rollback
implementation_basis: manifest-first update/rollback action
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in-plus-elevated-shell
release_gate: internal-requiresigned-payload
network_exposure_gate: none
runner_version: Invoke-PcvDesktopNodeProduct.ps1 Update/Rollback
host_capability_snapshot: is_admin false, requires_elevation true, final service Running, product root manifest 0.38.6-admin-smoke, previous root absent
exact_command_mode: Update from artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387/payload to 0.38.7-rc.1, then Rollback -TimeoutSec 5
result: blocked
observed_result: Update failed at sc.exe stop exit 5; Rollback failed with PCV_PRODUCT_SERVICE_STOP_TIMEOUT; host_mutation_performed=false
created_at: 2026-05-07T01:35:00+09:00
stale_triggers: product update/rollback service stop, backup/restore, or elevation preflight changes
waiver_status: non-elevated-shell

## Evidence Group: Service Status/Stop/Start

evidence_id: service-action-status-start-stop-20260504
artifact_or_package_version: artifacts/service-action-status-start-stop-20260504-002359
runner_version: DesktopNode.Host.exe service-action installed smoke, schema_version 1
host_capability_snapshot: installed PureCVisorDesktopNode service existed, StartMode Auto, product host path under C:\Program Files\PureCVisor\DesktopNode, boot_time_unchanged true
exact_command_mode: installed DesktopNode.Host.exe service-action status, stop, status, start, status with runtime policy health check after restart
created_at: 2026-05-04T00:23:59+09:00
stale_triggers: route matrix row identity, current_owner, target_owner, implementation_basis, fallback_policy, promotion_state, admin_smoke_required, release_gate, network_exposure_gate, package contract, service host, installer custom action, or GA-ready gate text changes
waiver_status: none

### Record: service status

evidence_id: service-action-status-start-stop-20260504-service-status
route_or_operation: service status
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: dotnet-native
commit_sha: f5a7539972199afd285f94ee622d59d409a411e7
artifact_or_package_version: artifacts/service-action-status-start-stop-20260504-002359
target_owner: dotnet-service-action
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: installed-non-mutating
release_gate: none
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action installed smoke, schema_version 1
host_capability_snapshot: service exists, owner verified, status running/stopped/running observed, boot time unchanged
exact_command_mode: installed service-action status before stop, status while stopped, status after restart
result: pass
created_at: 2026-05-04T00:23:59+09:00
stale_triggers: route matrix row identity or service-action native SCM controller contract changes
waiver_status: none

### Record: service stop

evidence_id: service-action-status-start-stop-20260504-service-stop
route_or_operation: service stop
route_surface: product-operation
risk_tier: tier2-reversible-mutation
current_owner: dotnet-native
commit_sha: f5a7539972199afd285f94ee622d59d409a411e7
artifact_or_package_version: artifacts/service-action-status-start-stop-20260504-002359
target_owner: dotnet-service-action
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action installed smoke, schema_version 1
host_capability_snapshot: service owner verified, stop returned ok true, stopped observed true, boot time unchanged
exact_command_mode: installed service-action stop, followed by non-mutating status check
result: pass
created_at: 2026-05-04T00:23:59+09:00
stale_triggers: route matrix row identity or service-action native SCM controller contract changes
waiver_status: none

### Record: service start

evidence_id: service-action-status-start-stop-20260504-service-start
route_or_operation: service start
route_surface: product-operation
risk_tier: tier2-reversible-mutation
current_owner: dotnet-native
commit_sha: f5a7539972199afd285f94ee622d59d409a411e7
artifact_or_package_version: artifacts/service-action-status-start-stop-20260504-002359
target_owner: dotnet-service-action
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action installed smoke, schema_version 1
host_capability_snapshot: service owner verified, start returned ok true, running observed true, runtime policy health status_code 200, boot time unchanged
exact_command_mode: installed service-action start, followed by status and runtime policy health check
result: pass
created_at: 2026-05-04T00:23:59+09:00
stale_triggers: route matrix row identity or service-action native SCM controller contract changes
waiver_status: none

## Ledger Status

### Record: job store schema mismatch detection

evidence_id: job-store-schema-mismatch-blocked-diagnostics-20260504
route_or_operation: job store schema mismatch detection
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: dotnet-runtime
commit_sha: f5a7539972199afd285f94ee622d59d409a411e7
artifact_or_package_version: focused xUnit tests JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine and RuntimePolicySerializesPhase24JobRuntimeContract
target_owner: dotnet-runtime
implementation_basis: dotnet-runtime
fallback_policy: none
promotion_state: current-native
admin_smoke_required: none
release_gate: none
network_exposure_gate: none
runner_version: dotnet test focused xUnit
host_capability_snapshot: no host mutation required, temp job store with unsupported version 99 remains in place, no quarantine file produced
exact_command_mode: dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine; dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter RuntimePolicySerializesPhase24JobRuntimeContract
result: pass
created_at: 2026-05-04T00:40:00+09:00
stale_triggers: job store schema version contract, unsupported future version behavior, route matrix row identity, or runtime policy state_store contract changes
waiver_status: none

## Evidence Group: Service/Data Root Lifecycle

evidence_id: service-data-root-lifecycle-20260504
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.30.3-admin-smoke
host_capability_snapshot: Windows 10 Pro for Workstations 25H2, elevated admin, Hyper-V enabled, installed PureCVisorDesktopNode final service Running, boot_time_unchanged true, remaining_pcv_vms empty
exact_command_mode: AllowUnsignedDev MSI build, direct installed payload service-action smoke, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, installed Hyper-V API route smoke
created_at: 2026-05-04T03:28:52+09:00
stale_triggers: service-action native SCM controller, data-root remove allowlist, MSI ProductActions sequence, route matrix row identity, or service/data-root lifecycle gate text changes
waiver_status: none

### Record: service uninstall remove-data request

evidence_id: service-data-root-lifecycle-20260504-remove-data-request
route_or_operation: service uninstall remove-data request
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: mixed-history
commit_sha: local working tree after 0.30.3-admin-smoke
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303
target_owner: dotnet-service-action
implementation_basis: windows-native-api
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.30.3-admin-smoke
host_capability_snapshot: direct service-action smoke configured owned service, service existed/running before remove, service absent after remove, boot time unchanged
exact_command_mode: DesktopNode.Host.exe service-action remove-installed --remove-data from installed payload copy
result: pass
observed_result: RemoveDataHandoff.Operation=data-root-remove, RemovedPaths empty, data root still existed after handoff, service absent after remove
created_at: 2026-05-04T03:28:52+09:00
stale_triggers: remove-installed --remove-data contract, service delete wait behavior, handoff schema, or MSI REMOVE_DATA sequencing changes
waiver_status: none

### Record: data root remove

evidence_id: service-data-root-lifecycle-20260504-data-root-remove
route_or_operation: data root remove
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: product-wrapper
commit_sha: local working tree after 0.30.3-admin-smoke
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303
target_owner: dotnet-data-root-action
implementation_basis: data-root-lifecycle-plan
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.30.3-admin-smoke
host_capability_snapshot: service exists guard returned PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS, service absent after remove-installed, cleanup data root absent, final service restored Running
exact_command_mode: DesktopNode.Host.exe service-action data-root-remove --remove-data before and after service deletion
result: pass
observed_result: installed service blocked deletion while present, service absent data-root-remove removed allowlisted api-token.txt/api-token.dpapi.json/jobs.json/events.jsonl/install.jsonl/diagnostics, preserved non-allowlist service-host.log, no reboot, no leftover smoke data root
created_at: 2026-05-04T03:28:52+09:00
stale_triggers: data-root remove allowlist, ACL repair, service absent precondition, or route matrix data-root lifecycle gate changes
waiver_status: none

## Evidence Group: Admin Host Mutation Rerun After Repo Migration

evidence_id: admin-host-mutation-rerun-20260504-0310
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1412-0310
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.31.0-admin-smoke
host_capability_snapshot: Windows 10 Pro for Workstations 25H2, elevated admin, Hyper-V enabled, installed PureCVisorDesktopNode final service Running, boot_time_unchanged true, remaining_pcv_vms empty
exact_command_mode: AllowUnsignedDev MSI build after repo migration active path removal, direct installed payload service-action smoke, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, installed Hyper-V API route smoke
created_at: 2026-05-04T14:58:49+09:00
stale_triggers: installer payload staging, route parity mutation runner dependency boundary, service-action native SCM controller, MSI ProductActions sequence, Hyper-V route matrix row identity, or admin host mutation gate text changes
waiver_status: none

### Record: service MSI Hyper-V host mutation rerun

evidence_id: admin-host-mutation-rerun-20260504-service-msi-hyperv
route_or_operation: installed service, MSI lifecycle, Hyper-V route mutation smoke
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: ac26cf9a8b355de4984536b3bb5492979719f6b7
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1412-0310
target_owner: dotnet-service-action-and-dotnet-native-hyperv
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: loopback-only
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.31.0-admin-smoke
host_capability_snapshot: product-owned MSI payload built from commit ac26cf9a8b355de4984536b3bb5492979719f6b7, final service path DesktopNode.Host.exe, service Running/Auto, boot time unchanged, no pcv-spike-* VM residue
exact_command_mode: full route parity mutation smoke with service-action, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, VM create/start/restart/poweroff/delete, checkpoint create/restore/delete, unmanaged VM delete guard
result: pass
observed_result: managed VM delete action=delete, repeat delete action=absent, unmanaged delete blocked with PCV_VM_NOT_MANAGED_BY_PURECVISOR, no automatic reboot, final service restored Running
created_at: 2026-05-04T14:58:49+09:00
stale_triggers: product-owned payload staging, route parity runner dependency removal, native mutation adapter contract, MSI lifecycle sequence, service owner/path contract, or Hyper-V managed marker guard changes
waiver_status: none

## Evidence Group: Admin Host Mutation Rerun After Standalone Asset Boundary

evidence_id: admin-host-mutation-rerun-20260504-0320
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1515-0320
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.32.0-admin-smoke
host_capability_snapshot: Windows 10 Pro for Workstations 25H2, elevated admin, Hyper-V enabled, installed PureCVisorDesktopNode final service Running, boot_time_unchanged true, remaining_pcv_vms empty
exact_command_mode: AllowUnsignedDev MSI build after standalone product asset boundary, direct installed payload service-action smoke, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, installed Hyper-V API route smoke
created_at: 2026-05-04T19:29:13+09:00
stale_triggers: standalone product asset staging, installer payload staging, route parity mutation runner dependency boundary, service-action native SCM controller, MSI ProductActions sequence, Hyper-V route matrix row identity, or admin host mutation gate text changes
waiver_status: none

### Record: service MSI Hyper-V host mutation rerun after asset boundary

evidence_id: admin-host-mutation-rerun-20260504-0320-service-msi-hyperv
route_or_operation: installed service, MSI lifecycle, Hyper-V route mutation smoke
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: d852ff54bafb403e16e86057b3cecec2813bf0b6
artifact_or_package_version: artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260504-1515-0320
target_owner: dotnet-service-action-and-dotnet-native-hyperv
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: loopback-only
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.32.0-admin-smoke
host_capability_snapshot: standalone product asset payload built from commit d852ff54bafb403e16e86057b3cecec2813bf0b6, MSI SHA-256 f3e4456e94d5ee16a8e0bd6d02d17ac04d682be5bd58c77098072f97711d25f5, payload file count 7, final service path DesktopNode.Host.exe, service Running/Auto, boot time unchanged, no pcv-spike-* VM residue
exact_command_mode: full route parity mutation smoke with service-action, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, VM create/start/restart/poweroff/delete, checkpoint create/restore/delete, unmanaged VM delete guard
result: pass
observed_result: managed VM delete action=delete, repeat delete action=absent, unmanaged delete blocked with PCV_VM_NOT_MANAGED_BY_PURECVISOR, no automatic reboot, final service restored Running
created_at: 2026-05-04T19:29:13+09:00
stale_triggers: standalone product asset staging, product-owned payload staging, native mutation adapter contract, MSI lifecycle sequence, service owner/path contract, or Hyper-V managed marker guard changes
waiver_status: none

## Evidence Group: Event Log Source Registration Native Owner

evidence_id: eventlog-source-registration-native-owner-20260504
artifact_or_package_version: focused xUnit tests EventLogRegisterPlanUsesNativeEventLogActionWithoutPowerShellCommands, EventLogRegisterUsesNativeRegistryControllerWithoutExternalCommands, EventLogRegisterRejectsForeignExistingSourceBeforeMutation
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter EventLogRegister
host_capability_snapshot: no host mutation required, fake Windows Event Log controller, no registry write, no service mutation, no firewall mutation, no trust store mutation
exact_command_mode: code-level DesktopNode.Host.exe service-action eventlog-register plan and injected controller execution
created_at: 2026-05-04T19:56:00+09:00
stale_triggers: Event Log source registration route matrix row identity, event source name, log name, controller ownership contract, service-action eventlog-register command surface, or OS mutation gate text changes
waiver_status: none

### Record: Event Log source registration owner migration

evidence_id: eventlog-source-registration-native-owner-20260504-registration
route_or_operation: Event Log source registration
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 44ea407688bd4badcf53ade3554584121aed77f6
artifact_or_package_version: focused xUnit tests EventLogRegister*
target_owner: windows-eventlog-action
implementation_basis: eventlog-registration-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter EventLogRegister
host_capability_snapshot: fake controller only, missing source registration path calls native controller, foreign source conflict blocks before mutation, no default MSI/diagnostics execution
exact_command_mode: DesktopNode.Host.exe service-action eventlog-register code-level plan and injected controller execution
result: pass
observed_result: service-action eventlog-register has no PowerShell command plan, missing source calls registry-backed controller, foreign existing source returns PCV_HOST_EVENTLOG_SOURCE_OWNERSHIP_MISMATCH, actual Event Log registry mutation not executed
created_at: 2026-05-04T19:56:00+09:00
stale_triggers: event source name, Application log binding, registry-backed Event Log controller behavior, foreign source conflict policy, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

## Evidence Group: Event Log Source Removal Native Owner

evidence_id: eventlog-source-removal-native-owner-20260504
artifact_or_package_version: focused xUnit tests EventLogRemovePlanUsesNativeEventLogActionWithoutPowerShellCommands, EventLogRemoveDeletesOwnedSourceWithoutExternalCommands, EventLogRemoveTreatsMissingSourceAsIdempotentSuccess
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~EventLogRemove
host_capability_snapshot: no host mutation required, fake Windows Event Log controller, no registry write/delete, no service mutation, no firewall mutation, no trust store mutation
exact_command_mode: code-level DesktopNode.Host.exe service-action eventlog-remove plan and injected controller execution
created_at: 2026-05-04T22:49:15+09:00
stale_triggers: Event Log source removal route matrix row identity, event source name, log name, controller ownership contract, service-action eventlog-remove command surface, or OS mutation gate text changes
waiver_status: none

### Record: Event Log source removal owner migration

evidence_id: eventlog-source-removal-native-owner-20260504-removal
route_or_operation: Event Log source removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: local working tree after eventlog-remove-native-owner
artifact_or_package_version: focused xUnit tests EventLogRemove*
target_owner: windows-eventlog-action
implementation_basis: eventlog-registration-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~EventLogRemove
host_capability_snapshot: fake controller only, owned source removal path calls native registry controller, missing source returns idempotent success, no default MSI/diagnostics execution
exact_command_mode: DesktopNode.Host.exe service-action eventlog-remove code-level plan and injected controller execution
result: pass
observed_result: service-action eventlog-remove has no PowerShell command plan, owned existing source calls registry-backed controller remove, missing source succeeds without mutation, actual Event Log registry removal not executed
created_at: 2026-05-04T22:49:15+09:00
stale_triggers: event source name, Application log binding, registry-backed Event Log controller behavior, foreign source conflict policy, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

## Evidence Group: Firewall Native Owner

evidence_id: firewall-native-owner-20260504
artifact_or_package_version: focused xUnit tests FirewallEnablePlanUsesNativeFirewallActionWithoutPowerShellCommands, FirewallEnableRequiresLanApprovalBeforeMutation, FirewallEnableCreatesOwnedAllowRuleWithoutExternalCommands, FirewallEnableRejectsForeignExistingRuleBeforeMutation, FirewallRemovePlanUsesNativeFirewallActionWithoutPowerShellCommands, FirewallRemoveDeletesOwnedRuleWithoutExternalCommands, FirewallRemoveTreatsMissingRuleAsIdempotentSuccess
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~Firewall
host_capability_snapshot: no host mutation required, fake Windows Firewall controller, no firewall write/delete, no service mutation, no Event Log mutation, no trust store mutation
exact_command_mode: code-level DesktopNode.Host.exe service-action firewall-enable/firewall-remove plan and injected controller execution
created_at: 2026-05-04T23:18:00+09:00
stale_triggers: firewall rule name, direction, protocol, local port, profile, remote address scope, LAN approval gate, controller ownership contract, service-action firewall command surface, or OS mutation gate text changes
waiver_status: none

### Record: firewall rule enable LAN exposure owner migration

evidence_id: firewall-native-owner-20260504-enable
route_or_operation: firewall rule enable LAN exposure
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: local working tree after firewall-truststore-native-owner
artifact_or_package_version: focused xUnit tests FirewallEnable*
target_owner: windows-firewall-action
implementation_basis: firewall-rule-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: lan-exposure-approval-required
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~Firewall
host_capability_snapshot: fake controller only, LAN approval gate blocks before query/mutation, missing owned rule path calls native firewall controller, foreign rule blocks before mutation, no default MSI/diagnostics execution
exact_command_mode: DesktopNode.Host.exe service-action firewall-enable --allow-lan code-level plan and injected controller execution
result: pass
observed_result: service-action firewall-enable has no PowerShell command plan, no --allow-lan returns PCV_HOST_FIREWALL_LAN_APPROVAL_REQUIRED before controller query, owned/missing path calls firewall controller enable, foreign existing rule returns PCV_HOST_FIREWALL_RULE_OWNERSHIP_MISMATCH, actual firewall mutation not executed
created_at: 2026-05-04T23:18:00+09:00
stale_triggers: firewall rule tuple, LAN approval gate, COM-backed firewall controller behavior, foreign rule conflict policy, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

### Record: firewall rule removal owner migration

evidence_id: firewall-native-owner-20260504-removal
route_or_operation: firewall rule removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: local working tree after firewall-truststore-native-owner
artifact_or_package_version: focused xUnit tests FirewallRemove*
target_owner: windows-firewall-action
implementation_basis: firewall-rule-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~Firewall
host_capability_snapshot: fake controller only, owned rule removal path calls native firewall controller, missing rule returns idempotent success, no default MSI/diagnostics execution
exact_command_mode: DesktopNode.Host.exe service-action firewall-remove code-level plan and injected controller execution
result: pass
observed_result: service-action firewall-remove has no PowerShell command plan, owned existing rule calls firewall controller remove, missing rule succeeds without mutation, actual firewall mutation not executed
created_at: 2026-05-04T23:18:00+09:00
stale_triggers: firewall rule tuple, COM-backed firewall controller behavior, foreign rule conflict policy, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

## Evidence Group: Trust Store Native Owner

evidence_id: trust-store-native-owner-20260504
artifact_or_package_version: focused xUnit tests TrustStoreInstallPlanUsesNativeCertificateStoreActionWithoutPowerShellCommands, TrustStoreInstallRequiresReleaseApprovalBeforeMutation, TrustStoreInstallImportsApprovedCertificatesWithoutExternalCommands, TrustStoreInstallRejectsForeignCertificateBeforeMutation, TrustStoreRemovePlanUsesNativeCertificateStoreActionWithoutPowerShellCommands, TrustStoreRemoveDeletesOwnedCertificatesWithoutExternalCommands, TrustStoreRemoveTreatsMissingCertificatesAsIdempotentSuccess
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~TrustStore
host_capability_snapshot: no host mutation required, fake Windows trust store controller, no certificate store write/delete, no service mutation, no firewall mutation, no Event Log mutation
exact_command_mode: code-level DesktopNode.Host.exe service-action trust-store-install/trust-store-remove plan and injected controller execution
created_at: 2026-05-04T23:18:00+09:00
stale_triggers: Root/TrustedPublisher store binding, internal root/leaf subject, certificate thumbprint identity, release approval gate, controller ownership contract, service-action trust-store command surface, or OS mutation gate text changes
waiver_status: none

### Record: trust store install owner migration

evidence_id: trust-store-native-owner-20260504-install
route_or_operation: trust store install
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: local working tree after firewall-truststore-native-owner
artifact_or_package_version: focused xUnit tests TrustStoreInstall*
target_owner: windows-trust-store-action
implementation_basis: windows-certificate-store-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~TrustStore
host_capability_snapshot: fake controller only, release approval gate blocks before query/mutation, missing approved Root/TrustedPublisher cert path calls native trust store controller, foreign certificate blocks before mutation, no default MSI/diagnostics execution
exact_command_mode: DesktopNode.Host.exe service-action trust-store-install --release-approved code-level plan and injected controller execution
result: pass
observed_result: service-action trust-store-install has no PowerShell command plan, missing --release-approved returns PCV_HOST_TRUST_STORE_RELEASE_APPROVAL_REQUIRED before controller query, approved path calls trust store controller install, foreign existing certificate returns PCV_HOST_TRUST_STORE_CERTIFICATE_OWNERSHIP_MISMATCH, actual certificate store mutation not executed
created_at: 2026-05-04T23:18:00+09:00
stale_triggers: certificate identity, Root/TrustedPublisher store binding, X509Store-backed controller behavior, release approval gate, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

### Record: trust store removal owner migration

evidence_id: trust-store-native-owner-20260504-removal
route_or_operation: trust store removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: local working tree after firewall-truststore-native-owner
artifact_or_package_version: focused xUnit tests TrustStoreRemove*
target_owner: windows-trust-store-action
implementation_basis: windows-certificate-store-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~TrustStore
host_capability_snapshot: fake controller only, release approval gate blocks before mutation, owned Root/TrustedPublisher certificate removal path calls native trust store controller, missing certificates return idempotent success, no default MSI/diagnostics execution
exact_command_mode: DesktopNode.Host.exe service-action trust-store-remove --release-approved code-level plan and injected controller execution
result: pass
observed_result: service-action trust-store-remove has no PowerShell command plan, owned existing certificates call trust store controller remove, missing certificates succeed without mutation, actual certificate store mutation not executed
created_at: 2026-05-04T23:18:00+09:00
stale_triggers: certificate identity, Root/TrustedPublisher store binding, X509Store-backed controller behavior, release approval gate, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

## Evidence Group: Event Log Source Registration Actual Registry Mutation

evidence_id: eventlog-source-registration-actual-registry-20260504
artifact_or_package_version: artifacts/eventlog-source-registration-20260504-actual-registry
runner_version: DesktopNode.Host.exe service-action eventlog-register from commit 44ea407688bd4badcf53ade3554584121aed77f6
host_capability_snapshot: elevated admin, source missing before run, final service Running/Auto, boot time 2026-05-01T15:52:04.5000000+09:00, remaining_pcv_vms empty
exact_command_mode: DesktopNode.Host.exe service-action eventlog-register --product-root installed product root --service-exe installed DesktopNode.Host.exe
created_at: 2026-05-04T20:04:27+09:00
stale_triggers: event source name, Application log binding, EventMessageFile path, TypesSupported value, service-action eventlog-register command surface, route matrix owner fields, or OS mutation gate text changes
waiver_status: none

### Record: Event Log source registration actual registry mutation

evidence_id: eventlog-source-registration-actual-registry-20260504-registration
route_or_operation: Event Log source registration
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 44ea407688bd4badcf53ade3554584121aed77f6
artifact_or_package_version: artifacts/eventlog-source-registration-20260504-actual-registry
target_owner: windows-eventlog-action
implementation_basis: eventlog-registration-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action eventlog-register from commit 44ea407688bd4badcf53ade3554584121aed77f6
host_capability_snapshot: elevated admin, source missing before run, final registry key exists, final service Running/Auto, no pcv-spike-* VM residue
exact_command_mode: DesktopNode.Host.exe service-action eventlog-register --product-root installed product root --service-exe installed DesktopNode.Host.exe
result: pass
observed_result: Application log source PureCVisor Desktop Node exists, EventMessageFile points to C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe, TypesSupported=7, service/MSI/Hyper-V/firewall/trust-store mutation not executed
created_at: 2026-05-04T20:04:27+09:00
stale_triggers: event source name, Application log binding, EventMessageFile path, TypesSupported value, registry-backed Event Log controller behavior, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

## Evidence Group: Service/MSI/Hyper-V/Firewall/Trust Store Actual Mutation

evidence_id: admin-host-mutation-service-msi-hyperv-firewall-truststore-20260504-0330
artifact_or_package_version: artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.33.0-admin-smoke, plus scoped firewall-only/trust-store-only admin smoke JSON
host_capability_snapshot: elevated admin, final PureCVisorDesktopNode service Running/Auto, boot_time_unchanged true, remaining_pcv_vms empty, firewall smoke rule final_absent true, trust-store smoke cert final_absent in LocalMachine Root/TrustedPublisher and CurrentUser My
exact_command_mode: AllowUnsignedDev MSI build, direct installed payload service-action smoke, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, installed Hyper-V API route smoke, firewall-only owned rule create/enable/remove, trust-store-only self-signed test cert import/remove
created_at: 2026-05-04T20:21:52+09:00
stale_triggers: service-action native SCM controller, MSI ProductActions sequence, Hyper-V native mutation adapter contract, firewall rule tuple/scope, certificate store location/identity policy, route matrix row identity, or OS mutation gate text changes
waiver_status: none

### Record: service MSI Hyper-V host mutation with OS mutation milestone

evidence_id: admin-host-mutation-service-msi-hyperv-firewall-truststore-20260504-0330-service-msi-hyperv
route_or_operation: installed service, MSI lifecycle, Hyper-V route mutation smoke
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: dca492c67c0cb3843832d5f6e1e76c8d686c3cdf
artifact_or_package_version: artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330
target_owner: dotnet-service-action-and-dotnet-native-hyperv
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: loopback-only
runner_version: Invoke-PcvRouteParityMutationSmoke.ps1, version 0.33.0-admin-smoke
host_capability_snapshot: product-owned MSI payload built from commit dca492c67c0cb3843832d5f6e1e76c8d686c3cdf, MSI SHA-256 e6522114963be755beab1f54e183eef212a9f32979751e1fe67159a20cd2a4ff, payload file count 7, final service path DesktopNode.Host.exe, service Running/Auto, boot time unchanged, no pcv-spike-* VM residue
exact_command_mode: full route parity mutation smoke with service-action, MSI install/repair/uninstall/REMOVE_DATA=1/final restore, VM create/start/restart/poweroff/delete, checkpoint create/restore/delete, unmanaged VM delete guard
result: pass
observed_result: managed VM delete action=delete, repeat delete action=absent, unmanaged delete blocked with PCV_VM_NOT_MANAGED_BY_PURECVISOR, shutdown without guest integration returned PCV_VM_SHUTDOWN_NOT_AVAILABLE, no automatic reboot, final service restored Running
created_at: 2026-05-04T20:14:25+09:00
stale_triggers: product-owned payload staging, native mutation adapter contract, MSI lifecycle sequence, service owner/path contract, or Hyper-V managed marker guard changes
waiver_status: none

### Record: firewall rule enable LAN exposure scoped smoke

evidence_id: admin-host-mutation-service-msi-hyperv-firewall-truststore-20260504-firewall-enable
route_or_operation: firewall rule enable LAN exposure
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: powershell-helper
commit_sha: dca492c67c0cb3843832d5f6e1e76c8d686c3cdf
artifact_or_package_version: artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330/firewall-only-smoke.json
target_owner: windows-firewall-action
implementation_basis: firewall-rule-plan
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: lan-exposure-approval-required
runner_version: scoped firewall-only admin smoke, schema_version 1
host_capability_snapshot: exact owned rule name PureCVisorDesktopNode-Smoke-20260504-FirewallOnly-20260504202105-03c22cf9, direction Inbound, action Allow, protocol TCP, local_port 47778, profile Private, remote_address LocalSubnet, no service/Event Log/trust-store mutation in this row-isolated smoke, boot time unchanged
exact_command_mode: New-NetFirewallRule disabled allow rule, Set-NetFirewallRule enabled true, Remove-NetFirewallRule exact name
result: pass
freshness_status: historical-pre-firewall-native-owner-migration
observed_result: rule created with expected tuple, updated to Enabled=True, removed by exact rule name, final rule count 0, final service Running, no pcv-spike-* VM residue; this evidence predates DesktopNode.Host.exe service-action firewall-enable/firewall-remove owner migration and is not fresh evidence for the current native owner contract by itself
created_at: 2026-05-04T20:21:05+09:00
stale_triggers: firewall rule name/scope tuple, LAN exposure approval policy, firewall ownership precondition, route matrix row identity, or OS mutation gate text changes
waiver_status: none

### Record: firewall rule removal scoped smoke

evidence_id: admin-host-mutation-service-msi-hyperv-firewall-truststore-20260504-firewall-removal
route_or_operation: firewall rule removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: powershell-helper
commit_sha: dca492c67c0cb3843832d5f6e1e76c8d686c3cdf
artifact_or_package_version: artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330/firewall-only-smoke.json
target_owner: windows-firewall-action
implementation_basis: firewall-rule-plan
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: scoped firewall-only admin smoke, schema_version 1
host_capability_snapshot: exact owned rule evidence captured before removal, no service/Event Log/trust-store mutation in this row-isolated smoke, boot time unchanged
exact_command_mode: Remove-NetFirewallRule exact owned rule name after tuple verification
result: pass
freshness_status: historical-pre-firewall-native-owner-migration
observed_result: final rule count 0, missing after removal verified, final service Running, no pcv-spike-* VM residue; this evidence predates DesktopNode.Host.exe service-action firewall-enable/firewall-remove owner migration and is not fresh evidence for the current native owner contract by itself
created_at: 2026-05-04T20:21:05+09:00
stale_triggers: firewall rule removal ownership policy, rule tuple, route matrix row identity, or OS mutation gate text changes
waiver_status: none

### Record: trust store install scoped smoke

evidence_id: admin-host-mutation-service-msi-hyperv-firewall-truststore-20260504-trust-store-install
route_or_operation: trust store install
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: powershell-helper
commit_sha: dca492c67c0cb3843832d5f6e1e76c8d686c3cdf
artifact_or_package_version: artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330/trust-store-only-smoke.json
target_owner: windows-trust-store-action
implementation_basis: windows-certificate-store-api
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: scoped trust-store-only admin smoke, schema_version 1
host_capability_snapshot: public trusted signing excluded, test certificate subject CN=PureCVisor Desktop Node Trust Store Smoke Only 20260504 20260504202151-5336654c, thumbprint 18FFB486CB56EBF6AD0C8B841ACF932FE482CACF, cert SHA-256 bd343068fe86a12453743baf5473c99fedb13a112e5fd6e9c8489d08a1d8f57c, LocalMachine Root and TrustedPublisher import observed, no service/firewall/Event Log mutation in this row-isolated smoke
exact_command_mode: New-SelfSignedCertificate CurrentUser My, Export-Certificate, Import-Certificate to LocalMachine Root and LocalMachine TrustedPublisher
result: pass
freshness_status: historical-pre-trust-store-native-owner-migration
observed_result: certificate was present in LocalMachine Root and TrustedPublisher by exact thumbprint before removal, boot time unchanged; this evidence predates DesktopNode.Host.exe service-action trust-store-install/trust-store-remove owner migration and is not fresh evidence for the current native owner contract by itself
created_at: 2026-05-04T20:21:52+09:00
stale_triggers: certificate identity/hash, store location, ADR-0003 internal/public trust model separation, release approval policy, route matrix row identity, or OS mutation gate text changes
waiver_status: none

### Record: trust store removal scoped smoke

evidence_id: admin-host-mutation-service-msi-hyperv-firewall-truststore-20260504-trust-store-removal
route_or_operation: trust store removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: powershell-helper
commit_sha: dca492c67c0cb3843832d5f6e1e76c8d686c3cdf
artifact_or_package_version: artifacts/service-msi-hyperv-firewall-truststore-admin-mutation-20260504-2035-0330/trust-store-only-smoke.json
target_owner: windows-trust-store-action
implementation_basis: windows-certificate-store-api
fallback_policy: blocked
promotion_state: blocked
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: scoped trust-store-only admin smoke, schema_version 1
host_capability_snapshot: owned certificate evidence captured by thumbprint before removal, no service/firewall/Event Log mutation in this row-isolated smoke, boot time unchanged
exact_command_mode: Remove-Item exact thumbprint from LocalMachine Root, LocalMachine TrustedPublisher, and CurrentUser My
result: pass
freshness_status: historical-pre-trust-store-native-owner-migration
observed_result: final_root_absent true, final_trustedpublisher_absent true, final_current_user_my_absent true, final service Running, no pcv-spike-* VM residue; this evidence predates DesktopNode.Host.exe service-action trust-store-install/trust-store-remove owner migration and is not fresh evidence for the current native owner contract by itself
created_at: 2026-05-04T20:21:52+09:00
stale_triggers: certificate identity/hash, store removal ownership policy, release approval policy, route matrix row identity, or OS mutation gate text changes
waiver_status: none

## Evidence Group: Current Native OS Mutation and LAN Gate

evidence_id: current-native-os-mutation-lan-gate-20260505-0341
artifact_or_package_version: artifacts/os-mutation-gates-20260505-003459-0341
runner_version: DesktopNode.Host.exe service-action current native OS gate, version 0.34.1-admin-smoke, MSI provenance commit 6f97a24aa2bdfacf33d7bd987559eb85e363e119, follow-up commit 49a06acd3493066a10ec26fe541d5d8be1005c2b
host_capability_snapshot: elevated admin, final PureCVisorDesktopNode service Running/Auto on loopback, boot_time_unchanged true, firewall rule final_absent true, internal Root/TrustedPublisher final_present true
exact_command_mode: AllowUnsignedDev MSI lifecycle, DesktopNode.Host.exe service-action firewall-enable/firewall-remove, LAN IP listener runtime policy smoke, DesktopNode.Host.exe service-action trust-store-install/trust-store-remove/trust-store-install restore with --release-approved
created_at: 2026-05-05T00:44:00+09:00
stale_triggers: MSI payload/provenance, firewall rule tuple/scope, LAN listener prefix policy, protected token source, internal Root/leaf thumbprints, release approval policy, route matrix row identity, or OS mutation gate text changes
waiver_status: none

### Record: MSI lifecycle current native OS gate

evidence_id: current-native-os-mutation-lan-gate-20260505-msi-lifecycle
route_or_operation: installed service and MSI lifecycle
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 6f97a24aa2bdfacf33d7bd987559eb85e363e119
artifact_or_package_version: artifacts/os-mutation-gates-20260505-003459-0341/msi-build/PureCVisorDesktopNode-0.34.1-admin-smoke-windows-x64.msi
target_owner: dotnet-service-action-and-windows-native-package
implementation_basis: windows-native-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: loopback-only
runner_version: MSI lifecycle smoke, version 0.34.1-admin-smoke
host_capability_snapshot: MSI SHA-256 550f9b03f023a580cd073884dd72e55fbc0cf70cd014dd9c1892fb1df5a22c2c, payload file count 7, service path DesktopNode.Host.exe, final loopback prefix http://127.0.0.1:7777/
exact_command_mode: install, repair, uninstall preserve, reinstall for remove data, REMOVE_DATA=1 uninstall, final restore install with REBOOT=ReallySuppress and MSIRESTARTMANAGERCONTROL=Disable
result: pass
observed_result: all MSI lifecycle steps exited 0, runtime policy health returned HTTP 200 after final restore, final service Running, no automatic reboot
created_at: 2026-05-05T00:35:17+09:00
stale_triggers: MSI ProductActions sequence, service owner/path contract, payload file list/hash, repair/remove-data behavior, or no-auto-reboot policy changes
waiver_status: none

### Record: firewall rule enable LAN exposure current native smoke

evidence_id: current-native-os-mutation-lan-gate-20260505-firewall-enable
route_or_operation: firewall rule enable LAN exposure
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 6f97a24aa2bdfacf33d7bd987559eb85e363e119
artifact_or_package_version: artifacts/os-mutation-gates-20260505-003459-0341/firewall-enable.json; artifacts/os-mutation-gates-20260505-003459-0341/lan-listener-ip-verdict.json
target_owner: windows-firewall-action
implementation_basis: firewall-rule-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: lan-exposure-approval-required
runner_version: DesktopNode.Host.exe service-action firewall-enable plus LAN IP smoke
host_capability_snapshot: exact owned rule PureCVisor Desktop Node Local API LAN, direction inbound, protocol TCP, local_port 7777, profile Private, remote_address LocalSubnet, final service restored loopback-only
exact_command_mode: DesktopNode.Host.exe service-action firewall-enable --allow-lan; stop installed loopback service; start LAN IP listener; GET /api/v1/runtime/policy with bearer token
result: pass
observed_result: firewall rule exists Enabled true Owned true with expected tuple; LAN IP prefix http://[redacted-private-endpoint]:7777/ returned HTTP 200; 0.0.0.0 prefix was rejected by Windows HttpListener as unsupported and not used as product evidence; follow-up commit 49a06acd3493066a10ec26fe541d5d8be1005c2b added FileNotFoundException missing-rule lookup hardening and xUnit coverage
created_at: 2026-05-05T00:35:54+09:00
stale_triggers: firewall rule tuple, LAN approval gate, COM-backed firewall controller behavior, HttpListener prefix policy, protected token source, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

### Record: firewall rule removal current native smoke

evidence_id: current-native-os-mutation-lan-gate-20260505-firewall-removal
route_or_operation: firewall rule removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 6f97a24aa2bdfacf33d7bd987559eb85e363e119
artifact_or_package_version: artifacts/os-mutation-gates-20260505-003459-0341/firewall-remove.json
target_owner: windows-firewall-action
implementation_basis: firewall-rule-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: none
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action firewall-remove
host_capability_snapshot: exact owned rule evidence captured before removal, final rule Exists false, no service/Event Log/trust-store mutation in this action
exact_command_mode: DesktopNode.Host.exe service-action firewall-remove with exact rule name
result: pass
observed_result: final rule Exists false for PureCVisor Desktop Node Local API LAN; follow-up commit 49a06acd3493066a10ec26fe541d5d8be1005c2b treats COM FileNotFoundException missing-rule lookup as idempotent missing rule
created_at: 2026-05-05T00:38:42+09:00
stale_triggers: firewall rule tuple, COM-backed firewall controller behavior, foreign rule conflict policy, missing-rule idempotency, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

### Record: trust store install current native smoke

evidence_id: current-native-os-mutation-lan-gate-20260505-trust-store-install
route_or_operation: trust store install
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 6f97a24aa2bdfacf33d7bd987559eb85e363e119
artifact_or_package_version: artifacts/os-mutation-gates-20260505-003459-0341/trust-store-install-existing.json; artifacts/os-mutation-gates-20260505-003459-0341/trust-store-restore-existing.json; artifacts/os-mutation-gates-20260505-003459-0341/existing-trust-certs.json
target_owner: windows-trust-store-action
implementation_basis: windows-certificate-store-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action trust-store-install --release-approved
host_capability_snapshot: public trusted signing excluded; ADR-0003 internal Root E49CD75AF53CCF7FA73C97E47443096A4507FB7E and TrustedPublisher leaf 8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6; final present true
exact_command_mode: export existing internal trust certs, DesktopNode.Host.exe service-action trust-store-install --release-approved, final restore install after removal
result: pass
observed_result: Root and TrustedPublisher certificates present by exact thumbprint, owned true, no subject collision; new test cert install attempt was blocked by subject collision with existing internal trust certs before mutation and cleaned from CurrentUser test store
created_at: 2026-05-05T00:40:41+09:00
stale_triggers: internal Root/leaf certificate identity, store location, ADR-0003 internal/public trust model separation, release approval policy, route matrix row identity, or OS mutation gate text changes
waiver_status: none

### Record: trust store removal current native smoke

evidence_id: current-native-os-mutation-lan-gate-20260505-trust-store-removal
route_or_operation: trust store removal
route_surface: product-operation
risk_tier: tier3-destructive-or-persistent
current_owner: dotnet-native
commit_sha: 6f97a24aa2bdfacf33d7bd987559eb85e363e119
artifact_or_package_version: artifacts/os-mutation-gates-20260505-003459-0341/trust-store-remove-existing.json; artifacts/os-mutation-gates-20260505-003459-0341/trust-store-restore-existing.json
target_owner: windows-trust-store-action
implementation_basis: windows-certificate-store-api
fallback_policy: none
promotion_state: current-native
admin_smoke_required: explicit-admin-opt-in
release_gate: release-approval-required
network_exposure_gate: none
runner_version: DesktopNode.Host.exe service-action trust-store-remove --release-approved plus final restore install
host_capability_snapshot: owned Root/TrustedPublisher certificates present before removal; absent after removal; restored present after final install; no service/firewall/Event Log mutation in this action
exact_command_mode: DesktopNode.Host.exe service-action trust-store-remove --release-approved with exact thumbprints, then trust-store-install --release-approved restore
result: pass
observed_result: remove action returned Exists false for Root and TrustedPublisher by exact thumbprint; final restore returned Exists true Owned true for both stores
created_at: 2026-05-05T00:41:00+09:00
stale_triggers: certificate identity, Root/TrustedPublisher store binding, X509Store-backed controller behavior, release approval gate, restore policy, route matrix owner fields, or admin-smoke gate changes
waiver_status: none

## Evidence Group: Active Spikes Reference Reclassification

evidence_id: active-spikes-reference-reclassification-20260504
artifact_or_package_version: docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json
runner_version: product wrapper protected-token helper tests, post-reboot product profile tests, root documentation guard
host_capability_snapshot: non-mutating local verification only; no Hyper-V/service/MSI/firewall/Event Log/trust-store mutation
exact_command_mode: product wrapper DPAPI protected-token xUnit/Pester path replacement, post-reboot repo boundary marker replacement, source/target/hash inventory generation
created_at: 2026-05-04T23:37:43+09:00
stale_triggers: product wrapper token schema/entropy, post-reboot repo boundary markers, archive target layout, verification ownership map, or active product path classification rule changes
waiver_status: none

### Record: product wrapper protected token owner replacement

evidence_id: product-wrapper-protected-token-owner-replacement-20260504
route_or_operation: protected token preparation and health auth
route_surface: product-operation
risk_tier: tier2-reversible-mutation
current_owner: product-wrapper
commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
artifact_or_package_version: packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
target_owner: dotnet-token-storage-action
implementation_basis: dpapi-local-machine-token-plan
fallback_policy: none
promotion_state: current-native
admin_smoke_required: none
release_gate: none
network_exposure_gate: none
runner_version: Invoke-Pester focused product wrapper tests
host_capability_snapshot: product wrapper uses DPAPI LocalMachine protected token helper internally; no spike service module import; no raw token on command line
exact_command_mode: Invoke-PcvDesktopNodeProductAction default token preparation/health check with product-owned protected-token helper
result: pass
observed_result: protected token file migrated from legacy token file, health check used Bearer token from product-owned protected token reader, module text has no PcvDesktopService.psm1 import path
created_at: 2026-05-04T23:37:43+09:00
stale_triggers: protected token schema, entropy string, product wrapper token preparation, health auth source, or service-action token bootstrap contract changes
waiver_status: none

### Record: active product spikes path closure

evidence_id: active-product-spikes-path-closure-20260504
route_or_operation: repo migration active product path inventory
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: product-wrapper
commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
artifact_or_package_version: docs/ga-ready/evidence/repo-migration-preflight-2026-05-04.md
target_owner: windows-native-package
implementation_basis: package-contract
fallback_policy: none
promotion_state: current-native
admin_smoke_required: none
release_gate: none
network_exposure_gate: none
runner_version: rg direct reference inventory plus package/post-reboot tests
host_capability_snapshot: active product path count 0; component/archive direct reference count 22; physical spikes file count 46
exact_command_mode: direct reference classification across packaging, root docs, AGENTS, follower, developer index, verification policy, public release boundary, ADR index
result: pass
observed_result: packaging/post-reboot/test active spike references removed; remaining direct references are component/archive documentation entry points only
created_at: 2026-05-04T23:37:43+09:00
stale_triggers: any new `spikes/**` runtime/packaging/required verification/post-reboot command reference or component/archive classification rule change
waiver_status: none

### Record: archive source and hash inventory

evidence_id: archive-spikes-inventory-20260504
route_or_operation: archive/read-only rollback proof
route_surface: product-operation
risk_tier: tier1-read-only
current_owner: product-wrapper
commit_sha: 53b5068544f37efea823f601ff4fdb2557ce8ba1
artifact_or_package_version: docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json
target_owner: windows-native-package
implementation_basis: package-contract
fallback_policy: none
promotion_state: current-native
admin_smoke_required: none
release_gate: none
network_exposure_gate: none
runner_version: Get-ChildItem/Get-FileHash inventory
host_capability_snapshot: file_move_execution not-run, archive_write_execution not-run, physical file count 46
exact_command_mode: source path, planned archive target, file length, SHA-256 inventory generation
result: pass
observed_result: inventory records all 46 physical spike files with planned archive target and SHA-256 without moving files
created_at: 2026-05-04T23:37:43+09:00
stale_triggers: any physical spike file content/path change before archive move, archive target layout change, or rollback restore criteria change
waiver_status: none

## Ledger Status

이 ledger는 통과한 service status/stop/start evidence, service/data-root lifecycle evidence, repo migration 이후 admin host mutation rerun evidence, standalone asset boundary 이후 admin host mutation rerun evidence, Event Log source registration native owner code-level evidence와 actual registry mutation evidence, Event Log source removal native owner code-level evidence, Firewall/trust-store native owner code-level evidence, Service/MSI/Hyper-V/firewall/trust-store actual mutation historical evidence, `0.34.1-admin-smoke` current native MSI/firewall/LAN/internal trust-store evidence, product wrapper protected-token owner replacement, active product `spikes/**` path closure, archive inventory/hash proof, job store schema mismatch blocked diagnostics evidence를 aggregate gate에 긍정 근거로 사용한다. 이 파일은 2026-05-04 ledger snapshot이며, 당시 repo migration preflight는 physical 파일 이동 미실행 때문에 별도 blocker evidence였다. 2026-05-05 physical archive move와 `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`가 이 blocker를 해소했고 ADR-0004 current decision의 최종 closure source다. `0.30.3-admin-smoke`, `0.31.0-admin-smoke`, `0.32.0-admin-smoke`, `0.33.0-admin-smoke`, `0.34.1-admin-smoke`는 unsigned `AllowUnsignedDev` evidence이므로 public trusted signing 또는 외부 stable publication evidence가 아니다. Event Log removal native owner evidence는 이 snapshot 작성 시점에는 실제 registry removal을 실행하지 않은 code-level evidence였다. `0.33.0-admin-smoke`의 row-isolated firewall/trust-store mutation은 current owner migration 이전 historical evidence로만 남기고, `0.34.1-admin-smoke`는 2026-05-04 snapshot 이후 첫 current native firewall/trust-store/LAN scoped execution evidence로 분리한다. 최신 post-snapshot full admin host mutation gate는 위 `0.39.1-admin-smoke` update를 따르고, 최신 MSI/service installed listener PASS와 후속 focused firewall/trust-store/LAN/Event Log OS gate는 `0.39.0-admin-smoke` historical update와 `0.39.1-admin-smoke` full gate를 따른다. 최신 config/job store migration apply installed PASS는 `0.38.6-admin-smoke` update를 따른다. 최신 internal enterprise `RequireSigned` build는 `0.38.7-rc.1` update를 따른다. Internal Root/leaf trust-store restore evidence는 ADR-0003 internal trust 범위이며 public trusted signing evidence가 아니다.

## Post-Snapshot Update - 2026-05-05 0.35.5

`docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0355.md`는 이 ledger snapshot 이후 사용자 재승인으로 실행한 `0.35.5-admin-smoke` evidence다. `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-094809-0355`와 `artifacts/os-mutation-gates-20260505-101659-0355-final`은 Hyper-V/MSI/service/data-root, Event Log register/remove, firewall enable/remove, LAN exposure, ADR-0003 internal trust-store install/remove/restore를 실행 당시 HEAD 기준으로 다시 확인했다. Final state는 service loopback `Running`, installed DisplayVersion `0.35.5`, firewall rule count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. `product config migration apply`와 `job store migration apply`는 `future-route/not-implemented/blocked`라 실행하지 않았다.

## Post-Snapshot Update - 2026-05-05 0.35.6

`docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0356.md`는 이 ledger snapshot 이후 실행 당시 code HEAD `cc723e28ed62f6f1c5e49c74ca68b87d0f1b8b3a` 기준으로 재실행한 `0.35.6-admin-smoke` evidence다. `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-170221-0356-rerun`와 `artifacts/os-mutation-gates-20260505-170454-0356-rerun`은 Hyper-V/MSI/service/data-root, Event Log register/remove, firewall enable/remove, LAN exposure, ADR-0003 internal trust-store install/remove/restore를 다시 확인했다. Final state는 service loopback `Running`, installed DisplayVersion `0.35.6`, firewall rule count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. LAN smoke는 `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets `HTTP 200`으로 확인했다. `product config migration apply`와 `job store migration apply`는 `future-route/not-implemented/blocked`라 실행하지 않았다.

`docs/ga-ready/evidence/os-mutation-gates-2026-05-05-0357.md`는 이 ledger snapshot 이후 현재 HEAD `2ec9e71d45b702e106824c86500cd6152b18fab7` 기준으로 재실행한 `0.35.7-admin-smoke` evidence다. `artifacts/routeparity-service-msi-hyperv-admin-host-mutation-20260505-174902-0357`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`은 Hyper-V/MSI/service/data-root, Event Log register/remove, firewall enable/remove, LAN exposure, ADR-0003 internal trust-store install/remove/restore를 다시 확인했다. Final state는 service loopback `Running`, installed DisplayVersion `0.35.7`, firewall rule count `0`, Event Log source absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM 잔여물 없음이다. LAN smoke는 `http://[redacted-private-endpoint]:7777/` bearer runtime policy/Web assets `HTTP 200`으로 확인했다. `product config migration apply`는 blocked/no-mutation descriptor로 확인했고 `job store migration apply`는 `future-route/not-implemented/blocked`라 실행하지 않았다.

## Post-Snapshot Update - 2026-05-05/06 Batch Supervisor and RequireSigned evidence

| date | evidence | artifact roots | result | notes |
|------|----------|----------------|--------|-------|
| 2026-05-05 | `0.37.0-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260505-231654-0370`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260505-231654-0370`, `artifacts/os-mutation-gates-batch-profile-20260505-231654-0370` | PASS | Batch Supervisor full gate. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass. First-attempt MSI repair `1603` recovered by direct repair/manual repair and `-Resume`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.0-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-001432-0380`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380`, `artifacts/os-mutation-gates-batch-profile-20260506-001432-0380` | PASS | Batch Supervisor full gate. Commit `267fe6afa0480ebc3b03431490bc37fa251261ae`, MSI SHA-256 `b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, `pcv-spike-*` VM count `0`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.1-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-142310-0381`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-142310-0381`, `artifacts/os-mutation-gates-batch-profile-20260506-142310-0381` | PASS | Historical Batch Supervisor full gate before `0.38.2`. Commit `d05d395e96d5d8d83b4cc4310c2b8ef11253041c`, MSI SHA-256 `69e1439f5e12adf6f72ad0e8612e2cef327e1c3e0a800f04934ddb79a136e36e`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, `pcv-spike-*` VM count `0`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.2-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382`, `artifacts/os-mutation-gates-batch-profile-20260506-145506-0382` | PASS | Historical Batch Supervisor full gate before `0.38.3`. Commit `d05d395e96d5d8d83b4cc4310c2b8ef11253041c`, MSI SHA-256 `4d93dc982d5be7fd7e592d9133e54e56540eb0f417b2ca371c4e686f0af97252`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM count `0`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.4-rc.1` InternalEnterprise `RequireSigned` MSI build | `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-212433-0384` | PASS | Historical internal signed build before `0.38.7-rc.1`. Commit `6bbb39f0a3a271e4a1187ce7de2014e009977425`, MSI SHA-256 `0b4c60d60098f89bd0adea4d183a5224d32b862e9bf69bd6dbaa41077377e8b9`, signing mode `RequireSigned`, signing trust model `InternalEnterprise`, Authenticode `Valid`, SignTool verify exit `0`, signer thumbprint `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.4-admin-smoke` full admin host mutation gate | `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-212527-0384`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384`, `artifacts/os-mutation-gates-batch-profile-20260506-212527-0384` | PASS | Historical Batch Supervisor full gate before `0.38.9-admin-smoke`. Commit `6bbb39f0a3a271e4a1187ce7de2014e009977425`, MSI SHA-256 `7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, boot time unchanged, `pcv-spike-*` VM count `0`. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | config/job store migration apply code-level actual path | `docs/ga-ready/evidence/config-jobstore-migration-apply-code-level-2026-05-06.md` | PASS | `config-migration-apply` and `job-store-migration-apply` moved from future implementation exclusion to code-level product-operation candidate. Evidence is xUnit/Pester temp-directory verification only; installed destructive admin smoke was pending for this row and was closed by the `0.38.6-admin-smoke` installed smoke below. Public trusted signing/external stable publication excluded. |
| 2026-05-06 | `0.38.5-admin-smoke` config/job store migration apply installed smoke preflight | `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-preflight-blocked-2026-05-06.md`, `artifacts/config-jobstore-migration-apply-installed-20260506-231702-0385` | BLOCKED | Focused installed smoke runner and plan-only Pester contract were added. Actual attempt stopped at preflight because current Codex shell was not elevated (`admin=false`, Administrators deny-only, medium integrity). `host_mutation_performed=false`, final service stayed `Running`, boot time unchanged. This is not PASS evidence and does not promote the config/job store migration apply rows beyond `ga-ready-candidate`. |
| 2026-05-07 | `0.38.6-admin-smoke` config/job store migration apply installed smoke | `docs/ga-ready/evidence/config-jobstore-migration-apply-installed-2026-05-07.md`, `artifacts/config-jobstore-migration-apply-installed-20260507-0386` | PASS | Focused installed destructive admin smoke PASS. Commit `d4259670e0aa90dae869bbd0e35c8910033fb59e`, MSI SHA-256 `d252110bee12e8c5c129b97474e2e08a51941d79d81d460fd6fe45932b290593`, signing mode `AllowUnsignedDev`. Product manifest schema `1 -> 2`, job store schema `1 -> 2`, backup/temp replace evidence, final service `Running`, boot time unchanged, post-migration API read ok. This promotes both migration apply rows to `current-native`. Public trusted signing/external stable publication excluded. |
| 2026-05-07 | `0.38.7-rc.1` InternalEnterprise `RequireSigned` MSI build | `docs/ga-ready/evidence/host-mutation-signed-build-attempt-2026-05-07-0387.md`, `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387` | PASS | Latest internal signed build. Commit `dd4e7379c515b05eb82038404519c9e63f54bf51`, MSI SHA-256 `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`, signing mode `RequireSigned`, signing trust model `InternalEnterprise`, Authenticode `Valid`, SignTool verify exit `0`, signer thumbprint `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`. Same `0.38.7-admin-smoke` full admin host mutation and update/rollback attempts were non-elevated blocked history, not PASS evidence. Public trusted signing/external stable publication excluded. |
| 2026-05-07 | Beta-0 Web Dashboard smoke | `docs/ga-ready/evidence/beta-web-dashboard-smoke-2026-05-07.md`, `artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743` | PASS | Batch Supervisor `WebRegression` read-only/static fixture smoke. `web/tests` Pester 26 tests, `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web/app.js` pass. Summary `ok=true`, `status=completed`, `total_steps=4`, `executed_steps=4`. Hyper-V/service/MSI/firewall/trust-store/LAN/update mutation evidence excluded. |
| 2026-05-07 | Web Console Network Inventory view | `docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md`, `web/src/served-app.ts`, `web/scripts/verify-browser-fixture.mjs` | PASS | Read-only `Network` view over `GET /api/v1/network/inventory`. Static guard passed 27 tests after RED failure, user-visible/browser fixture includes `Default Switch` and `fixture-ethernet`, generated parity manifest includes `networkInventory`. Hyper-V switch/IP/firewall/service/MSI/trust-store/LAN/update mutation evidence excluded. |
| 2026-05-07 | Web Console Diagnostic Bundle operator handoff UI | `docs/ga-ready/evidence/web-console-diagnostic-bundle-ui-2026-05-07.md`, `web/src/served-app.ts`, `web/scripts/verify-browser-fixture.mjs` | PASS | Read-only `Troubleshooting` panel for existing product wrapper `CollectDiagnostics`. Static guard passed 28 tests after RED failure, browser fixture includes `Diagnostic Bundle`, `CollectDiagnostics`, `operator handoff`, `no host mutation`, token value/Authorization header redaction boundary, and diagnostics root. Web API bundle generation/download and Hyper-V/service/MSI/firewall/trust-store/LAN/update mutation evidence excluded. |
| 2026-05-09 | Web Console Single UI clone and staged frontend service batches | `docs/ga-ready/evidence/web-console-single-ui-clone-2026-05-09.md`, `web/index.html`, `web/styles.css`, `web/src/served/*.ts`, `output/playwright/single-console-clone-20260509` | PASS | Read-only/static UI evidence. Active Web Console clones the Single console workbench frame and now binds service-core staged source parts, partial refresh degraded handling, VM/checkpoint scoped pending state, job polling backoff/next-page loading, VM asset explorer, workspace tabbar, menu/rail/quick action command routing. Static guard passed 38 tests and active Web Console source scan has no Linux runtime/auth/websocket route imports. Hyper-V/service/MSI/firewall/trust-store/LAN/update mutation evidence excluded; public trusted signing/external stable publication excluded. |
| 2026-05-07 | `0.38.8-admin-smoke` update/rollback mutation | `docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md`, `artifacts/product-update-rollback-mutation-20260507-0388`, `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass` | PASS | AllowUnsignedDev build PASS. MSI SHA-256 `163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564`, provenance commit `fd4f854646fc159d54f7578230f00c51f80e201f`. First non-elevated attempt was blocked and preserved as history. Elevated update `0.38.6-admin-smoke -> 0.38.8-admin-smoke` exited `0`, health `200`, update journal `succeeded/health`; rollback exited `0`, restored current manifest `0.38.6-admin-smoke`, preserved `0.38.8-admin-smoke` as `DesktopNode.failed`, final service `Running`, boot time unchanged, `host_mutation_performed=true`. |
| 2026-05-08 | `0.38.9-admin-smoke` full admin host mutation gate | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-08-0389.md`, `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`, `artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389`, `artifacts/os-mutation-gates-batch-profile-20260508-202255-0389` | PASS | Historical Batch Supervisor full gate before `0.39.1-admin-smoke`. Commit `159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce`, MSI SHA-256 `86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3`, signing mode `AllowUnsignedDev`. Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store pass, retry not needed, final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, boot time unchanged, `remaining_pcv_vms=[]`. Public trusted signing/external stable publication excluded. |
| 2026-05-08 | Diagnostic bundle server API action code-level | `docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md`, `src/DesktopNode.Api.Tests/ApiDiagnosticBundleRequestProcessorTests.cs`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` | PASS | Code-level Local API action. `POST /api/v1/diagnostics/bundles` writes a redacted `.bundle.json`; `GET /api/v1/diagnostics/bundles/{bundle_id}/download` serves it with `X-PCV-Diagnostic-Bundle-Id`; product service plan includes `--diagnostics-root`. Installed listener, host mutation, public trusted signing, and external stable publication excluded. |
| 2026-05-08 | Diagnostic bundle Host listener code-level | `docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md`, `src/DesktopNode.Host/DesktopNodeHostApplication.cs`, `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs` | PASS | Code-level in-process `DesktopNodeHostApplication` listener evidence. Bearer-required POST creates a redacted `.bundle.json`; authenticated GET download serves it with `X-PCV-Diagnostic-Bundle-Id`; `X-PCV-Request-Id` propagates into the response and saved bundle. Installed service listener, host mutation, public trusted signing, and external stable publication excluded. |
| 2026-05-08 | Diagnostic bundle product wrapper code-level | `docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md`, `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`, `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1` | PASS | Code-level product wrapper delegation evidence. `Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics` delegates to `New-PcvDesktopNodeDiagnosticBundle`, returns `actual_execution=code-level-product-wrapper`, and writes `product-wrapper-delegation-redacted.json` with `diagnostic_bundle_product_wrapper_delegation=code-level-product-action-orchestrator`. This code-level record did not mutate the installed listener; installed listener PASS is recorded by the later `0.39.0-admin-smoke` rerun. |
| 2026-05-08 | Diagnostic bundle native service-action config code-level | `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`, `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`, `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs` | PASS | Code-level native SCM config evidence. `service-action configure-installed|repair-installed` now writes `--diagnostics-root`, protected token file, route timeout, request limit, burst, and retry-after arguments into `DesktopNodeWindowsServiceConfiguration.BinaryPathName`. Post-0.38.9 artifact inspection showed the installed final `PathName` did not yet contain these arguments; the later `0.39.0-admin-smoke` installed listener rerun closes that blocker. Host mutation in this code-level record, public trusted signing, and external stable publication excluded. |
| 2026-05-08 | `0.39.0-admin-smoke` MSI/service installed listener rerun | `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`, `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`, `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390` | PASS | Elevated MSI/service installed listener PASS. Commit `8d21654045ed75e81344556fa6444f118c62276a`, MSI SHA-256 `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`, signing mode `AllowUnsignedDev`. Batch `ok=true`, final service `Running`, product manifest version `0.39.0-admin-smoke`, SCM `PathName` includes diagnostics root/protected token/hardening args, diagnostic bundle POST `201`, download `200`, redaction PASS, boot time unchanged, `remaining_pcv_vms=[]`. Firewall/trust-store/LAN/Event Log OS gate excluded from this rerun; public trusted signing/external stable publication excluded. |
| 2026-05-08 | `0.39.0-admin-smoke` installed listener OS mutation gate | `docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md`, `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`, `artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390` | PASS | Focused OS mutation gate using the installed listener rerun artifact as input. Batch `ok=true`, one step, timeout false. Firewall enable/remove, LAN listener `http://[redacted-private-endpoint]:7777/` runtime policy/Web assets HTTP `200`, Event Log register/remove, ADR-0003 internal Root/TrustedPublisher install/remove/restore PASS. Final service `Running`, firewall count `0`, Event Log absent, internal trust cert present, boot time unchanged. Public trusted signing `excluded`; external stable publication `not-claimed`. |
| 2026-05-10 | internal MSIX package lifecycle smoke | `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`, `artifacts/msix-package-lifecycle-smoke-20260510-0416` | PASS | `PureCVisor.DesktopNode.MsixSmoke` package identity and `PureCVisorDesktopNodeMsixSmoke` packaged service. `makeappx` package creation, `signtool sign`, `signtool verify`, install `0.41.5.0`, update `0.41.6.0`, remove, final package/service absence all PASS. Existing MSI service stayed `Running`. Public trusted signing `excluded`; external stable publication `not-claimed`. |
| 2026-05-09 | winget CLI validate | `docs/ga-ready/evidence/winget-cli-validate-2026-05-09.md`, `artifacts/winget-cli-validate-20260509-0391` | PASS | Actual `winget validate --manifest` ran against the generated singleton MSI manifest preview and exited `0`. `winget_validation_status=winget-cli-validate-pass`, `winget_submission=not-submitted`, `host_mutation_performed=false`. Public trusted signing/external stable publication not claimed. |
| 2026-05-09 | diagnostic bundle list pagination/retention | `docs/ga-ready/evidence/diagnostic-bundle-list-pagination-retention-2026-05-09.md`, `src/DesktopNode.Api.Tests/ApiDiagnosticBundleRequestProcessorTests.cs`, `web/tests/PcvDesktopWeb.Static.Tests.ps1` | PASS | Code-level read-only API/Web hardening. `GET /api/v1/diagnostics/bundles?limit=&offset=` applies retention before listing latest-first bundles with `next_offset`, and Web Console Troubleshooting renders retained bundles plus `Load more bundles`. Host mutation/public trusted signing/external stable publication not claimed. |
| 2026-05-09 | service token rotation/revoke installed admin smoke | `docs/ga-ready/evidence/service-token-rotation-revoke-installed-2026-05-09.md`, `artifacts/service-token-rotation-revoke-installed-20260509-150334` | PASS | Native `DesktopNode.Host.exe service-action service-token-rotation-revoke` backed up/replaced the DPAPI protected token file, restarted `PureCVisorDesktopNode`, verified old bearer `403`, new bearer `200`, and wrote redacted audit. Final service `Running`, `service_token_mutation=performed`, `token_value_observed=false`, `host_mutation_performed=true`. Public trusted signing/external stable publication not claimed. |
| 2026-05-09 | installed listener external load/rate-limit | `docs/ga-ready/evidence/installed-listener-external-load-rate-limit-2026-05-09.md`, `artifacts/installed-listener-external-load-rate-limit-20260509-0391` | PASS | Installed listener handled 180 real HTTP requests with 200 `140`, 429 `40`, unexpected `0`; all 429 responses carried `Retry-After` and `PCV_RATE_LIMIT_EXCEEDED` problem details. Token value was not captured, host mutation was not performed, and public trusted signing/external stable publication are not claimed. |
| 2026-05-10 | Lifecycle/Packaging current rebaseline | `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md`, `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416` | PASS | Current package pair `0.41.5-admin-smoke -> 0.41.6-admin-smoke` generated. Installed product update/rollback PASS, internal clean-host install/update/rollback PASS on Windows-updated UBR `5020` guest, final manifest `0.41.5-admin-smoke`, failed root `0.41.6-admin-smoke`, Web Console `200`, API unauthenticated boundary `401`. Public trusted signing/external stable publication out-of-scope. |
