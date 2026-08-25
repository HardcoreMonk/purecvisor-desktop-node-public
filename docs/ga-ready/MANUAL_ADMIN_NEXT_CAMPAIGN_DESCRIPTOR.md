# MANUAL-ADMIN 캠페인 Descriptor

## 읽는 법

이 문서는 삭제 없이 덧붙이는 누적 기록이다. 필드 이름의 접두사가 시제를 결정한다.

| 접두사 | 뜻 |
| --- | --- |
| `current_*` | 현재 판. `descriptor_id`/`updated_at` 시점의 값이다 |
| `latest_*` | 현재 판 중 후보·게이트 계열 |
| `next_*` | 아직 실행하지 않은 다음 대상 |
| `previous_<버전>_*`, `historical_*` | 강등된 과거 값. 재해석하거나 삭제하지 않는다 |

새 campaign을 닫으면 기존 `current_*`/`latest_*`/`next_*` 값을
`previous_<직전버전>_<원래 필드명>`으로 강등한 뒤 그 자리에 새 값을 쓴다. 문서 하단의 dated
section과 접두사 없는 `status:` 줄은 작성 시점 기록이며 현재 판이 아니다. 현재 판단이 필요하면
위 `current_*` 블록과 `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`를 읽는다.

descriptor_id: `manual-admin-next-campaign-descriptor-2026-08-21-04274-promotion-closure`
previous_04273_descriptor_id: `manual-admin-next-campaign-descriptor-2026-08-14-04273-promotion-closure`
updated_at: `2026-08-21T00:34:57.0450674+09:00`
previous_04273_updated_at: `2026-08-20T22:42:08.9382675+09:00`
current_status: `closed-package-pair-04273-04274-and-04274-fullgate-functional-current-card-pass-with-p0-save-open-defect`
previous_04273_current_status: `closed-package-pair-04272-04273-and-04273-fullgate-functional-current-card-pass`
previous_04272_descriptor_id: `manual-admin-next-campaign-descriptor-2026-08-10-04272-r4-promotion-closure`
previous_04272_updated_at: `2026-08-10T00:40:16.3006641+09:00`
previous_04272_current_status: `closed-package-pair-04271-04272-and-04272-fullgate-functional-credential-token-r4-current-card-pass`
previous_04271_descriptor_id: `manual-admin-next-campaign-descriptor-2026-08-08-04271-package-pair-closure`
previous_04271_updated_at: `2026-08-09T00:27:00+09:00`
previous_04271_current_status: `closed-package-pair-04270-04271-pass-and-04271-fullgate-pass`
previous_04259_descriptor_id: `manual-admin-next-campaign-descriptor-2026-05-29-04259-public-boundary-docs-maintenance-postpush`
previous_04259_updated_at: `2026-05-29T21:15:00+09:00`
current_manual_admin_package_pair: `0.42.73-admin-smoke -> 0.42.74-admin-smoke`
previous_04273_current_manual_admin_package_pair: `0.42.72-admin-smoke -> 0.42.73-admin-smoke`
previous_04272_current_manual_admin_package_pair: `0.42.71-admin-smoke -> 0.42.72-admin-smoke`
previous_04271_current_manual_admin_package_pair: `0.42.70-admin-smoke -> 0.42.71-admin-smoke`
previous_04259_current_manual_admin_package_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
previous_04259_current_descriptor_batch_id: `manual-admin-campaign-descriptor-20260529-04258-04259-closed`
previous_04259_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md`
previous_04259_installed_account_novnc_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`
previous_04258_current_manual_admin_package_pair: `0.42.57-admin-smoke -> 0.42.58-admin-smoke`
previous_04258_current_descriptor_batch_id: `manual-admin-campaign-descriptor-20260529-04257-04258-closed`
previous_04258_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`
previous_04258_installed_account_novnc_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`
previous_04257_current_manual_admin_package_pair: `0.42.56-admin-smoke -> 0.42.57-admin-smoke`
previous_04257_current_descriptor_batch_id: `manual-admin-campaign-descriptor-20260528-04256-04257-closed`
previous_04257_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04257.md`
previous_04257_installed_account_novnc_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04257.md`
previous_04256_current_manual_admin_package_pair: `0.42.55-admin-smoke -> 0.42.56-admin-smoke`
previous_04256_current_descriptor_batch_id: `manual-admin-campaign-descriptor-20260528-04255-04256-closed`
previous_04256_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md`
previous_04256_installed_account_novnc_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04256.md`
current_manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md`
previous_04273_current_manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md`
current_manual_admin_campaign_root: `artifacts/manual-admin-campaign-20260820-04273-04274`
previous_04273_current_manual_admin_campaign_root: `artifacts/manual-admin-campaign-20260814-04272-04273`
current_manual_admin_target_package_root: `artifacts/admin-smoke-package-20260820-04274`
previous_04273_current_manual_admin_target_package_root: `artifacts/admin-smoke-package-20260814-04273`
current_manual_admin_target_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
previous_04273_current_manual_admin_target_msi_sha256: `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`
current_manual_admin_update_package_sha256: `cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1`
previous_04273_current_manual_admin_update_package_sha256: `1a7b17e2f1e2e3175f94c1ffce03b5d358a291f795ca34b3e0d4602e116d1b3c`
current_manual_admin_descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
current_manual_admin_descriptor_schema_version: `2`
current_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260820-04273-04274-closed`
previous_04273_current_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260814-04272-04273-closed`
current_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260820-04273-04274/manual-admin-campaign-descriptor/summary.json`
previous_04273_current_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260814-04272-04273/manual-admin-campaign-descriptor/summary.json`
current_manual_admin_current_card_descriptor_batch_id_contract: `direct-exposed`
current_manual_admin_runner_count: `6`
current_manual_admin_missing_count: `0`
current_manual_admin_not_pass_count: `0`
current_manual_admin_days_since_previous_closure: `6`
previous_04273_current_manual_admin_days_since_previous_closure: `4`
current_evidence_ledger: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
current_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md`
previous_04273_current_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md`
current_installed_operator_surface_current_card_summary: `artifacts/installed-operator-surface-current-card-20260820-04274/summary.json`
previous_04273_current_installed_operator_surface_current_card_summary: `artifacts/installed-operator-surface-current-card-20260814-04273/summary.json`
current_installed_operator_surface_current_card_summary_sha256: `531fc614da5edb0e11994b021383491ccb8830115d59fb211c6c330f5b25f8c8`
previous_04273_current_installed_operator_surface_current_card_summary_sha256: `44a91426579c6fb486e6b99cca2321ba4fd8cd547d16797017e0baa6c9d0da14`
current_functional_correctness_actual_host_evidence: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md`
previous_04273_current_functional_correctness_actual_host_evidence: `docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-14-04273.md`
current_functional_correctness_actual_host_summary: `artifacts/functional-correctness-carryforward-20260820-04274/summary.json`
previous_04273_current_functional_correctness_actual_host_summary: `artifacts/functional-correctness-carryforward-20260814-04273/summary.json`
current_functional_correctness_actual_host_summary_sha256: `5395286b74ca7dabd3edccbb63c0b006c32999a4c350559e8b90ddb1ea1fb4b8`
previous_04273_current_functional_correctness_actual_host_summary_sha256: `09a571235524b1a32c6066b7ef8c3c4ab4a425a7016ef4ccd1d284f75f9e6fac`
current_operational_credential_rebootstrap_recovery_evidence: `docs/ga-ready/evidence/operational-credential-rebootstrap-recovery-r2-2026-08-09-04272.md`
current_operational_credential_rebootstrap_recovery_summary: `artifacts/operational-credential-rebootstrap-recovery-r2-20260809-04272/summary.json`
current_operational_credential_rebootstrap_recovery_summary_sha256: `529626336fcb79696f5cf765e7f1dacbf81a96beafc30000e00fa591ec7bfacb`
current_installed_token_rotation_evidence: `docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md`
current_installed_token_rotation_summary: `artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json`
current_installed_token_rotation_summary_sha256: `285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136`
previous_04272_current_manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-09-04271-04272.md`
previous_04272_current_manual_admin_campaign_root: `artifacts/manual-admin-campaign-20260809-04271-04272`
previous_04272_current_manual_admin_target_package_root: `artifacts/admin-smoke-package-20260809-04272`
previous_04272_current_manual_admin_target_msi_sha256: `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22`
previous_04272_current_manual_admin_update_package_sha256: `f9dfa886dd5db2623ec63342538d775757b5f464e9eb9ca23a5206bcc1d65ba8`
previous_04272_current_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260809-04271-04272-closed`
previous_04272_current_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260809-04271-04272/manual-admin-campaign-descriptor/summary.json`
previous_04272_current_manual_admin_days_since_previous_closure: `1`
previous_04272_current_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-09-04272.md`
previous_04271_current_manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-08-04270-04271.md`
previous_04271_current_manual_admin_campaign_root: `artifacts/manual-admin-campaign-20260808-04270-04271`
previous_04271_current_manual_admin_target_package_root: `artifacts/admin-smoke-package-20260808-04271`
previous_04271_current_manual_admin_target_msi_sha256: `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5`
previous_04271_current_manual_admin_update_package_sha256: `836f79c2448642a05840ad4380e872b5a60c0c505c83a33e1fea07110e61ebf4`
previous_04271_current_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260808-04270-04271-closed`
previous_04271_current_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260808-04270-04271/manual-admin-campaign-descriptor/summary.json`
previous_04271_current_manual_admin_days_since_previous_closure: `69`
previous_04271_current_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-09-04271.md`
previous_04270_current_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-06-04270.md`
current_installed_account_novnc_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`
current_installed_account_novnc_evidence_status: `unchanged-since-04258-no-account-novnc-payload-change`
previous_04259_current_manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md`
previous_04259_current_manual_admin_campaign_root: `artifacts/manual-admin-campaign-20260529-04258-04259`
previous_04259_current_manual_admin_target_package_root: `artifacts/admin-smoke-package-20260529-04259`
previous_04259_current_manual_admin_target_msi_sha256: `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`
previous_04259_current_manual_admin_update_package_sha256: `05951af066f0080c9c111de7e104fc8a9418812b68ca0fb246a573d89b6e44fb`
previous_04259_current_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260529-04258-04259-closed`
previous_04259_current_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260529-04258-04259/manual-admin-campaign-descriptor/summary.json`
latest_manual_admin_candidate_package_pair: `0.42.73-admin-smoke -> 0.42.74-admin-smoke`
previous_04273_latest_manual_admin_candidate_package_pair: `0.42.72-admin-smoke -> 0.42.73-admin-smoke`
latest_manual_admin_candidate_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md`
previous_04273_latest_manual_admin_candidate_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md`
latest_manual_admin_candidate_campaign_root: `artifacts/manual-admin-campaign-20260820-04273-04274`
previous_04273_latest_manual_admin_candidate_campaign_root: `artifacts/manual-admin-campaign-20260814-04272-04273`
latest_manual_admin_candidate_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260820-04273-04274-closed`
previous_04273_latest_manual_admin_candidate_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260814-04272-04273-closed`
latest_manual_admin_candidate_descriptor_summary: `artifacts/manual-admin-campaign-20260820-04273-04274/manual-admin-campaign-descriptor/summary.json`
previous_04273_latest_manual_admin_candidate_descriptor_summary: `artifacts/manual-admin-campaign-20260814-04272-04273/manual-admin-campaign-descriptor/summary.json`
latest_manual_admin_candidate_status: `pass-closed`
latest_manual_admin_candidate_missing_count: `0`
latest_manual_admin_candidate_not_pass_count: `0`
latest_manual_admin_candidate_blocker: `none`
latest_manual_admin_candidate_target_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
previous_04273_latest_manual_admin_candidate_target_msi_sha256: `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`
latest_manual_admin_candidate_update_package_sha256: `cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1`
previous_04273_latest_manual_admin_candidate_update_package_sha256: `1a7b17e2f1e2e3175f94c1ffce03b5d358a291f795ca34b3e0d4602e116d1b3c`
latest_manual_admin_candidate_provenance_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
previous_04273_latest_manual_admin_candidate_provenance_commit: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
previous_04272_latest_manual_admin_candidate_package_pair: `0.42.71-admin-smoke -> 0.42.72-admin-smoke`
previous_04272_latest_manual_admin_candidate_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-09-04271-04272.md`
previous_04272_latest_manual_admin_candidate_campaign_root: `artifacts/manual-admin-campaign-20260809-04271-04272`
previous_04272_latest_manual_admin_candidate_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260809-04271-04272-closed`
previous_04272_latest_manual_admin_candidate_descriptor_summary: `artifacts/manual-admin-campaign-20260809-04271-04272/manual-admin-campaign-descriptor/summary.json`
previous_04272_latest_manual_admin_candidate_status: `pass-closed`
previous_04272_latest_manual_admin_candidate_target_msi_sha256: `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22`
previous_04272_latest_manual_admin_candidate_update_package_sha256: `f9dfa886dd5db2623ec63342538d775757b5f464e9eb9ca23a5206bcc1d65ba8`
previous_04272_latest_manual_admin_candidate_provenance_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
previous_04271_latest_manual_admin_candidate_package_pair: `0.42.70-admin-smoke -> 0.42.71-admin-smoke`
previous_04271_latest_manual_admin_candidate_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-08-04270-04271.md`
previous_04271_latest_manual_admin_candidate_campaign_root: `artifacts/manual-admin-campaign-20260808-04270-04271`
previous_04271_latest_manual_admin_candidate_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260808-04270-04271-closed`
previous_04271_latest_manual_admin_candidate_descriptor_summary: `artifacts/manual-admin-campaign-20260808-04270-04271/manual-admin-campaign-descriptor/summary.json`
previous_04271_latest_manual_admin_candidate_status: `pass-closed`
previous_04271_latest_manual_admin_candidate_missing_count: `0`
previous_04271_latest_manual_admin_candidate_not_pass_count: `0`
previous_04271_latest_manual_admin_candidate_blocker: `none`
previous_04271_latest_manual_admin_candidate_target_msi_sha256: `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5`
previous_04271_latest_manual_admin_candidate_update_package_sha256: `836f79c2448642a05840ad4380e872b5a60c0c505c83a33e1fea07110e61ebf4`
previous_04271_latest_manual_admin_candidate_provenance_commit: `80f69f31464ce07b2c9eca19211adf1232ea75f6`
previous_04259_latest_manual_admin_candidate_package_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
previous_04259_latest_manual_admin_candidate_target_msi_sha256: `6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`
previous_04259_latest_manual_admin_candidate_update_package_sha256: `05951af066f0080c9c111de7e104fc8a9418812b68ca0fb246a573d89b6e44fb`
previous_04259_latest_manual_admin_candidate_provenance_commit: `63d57feba605f82dabd44a96ed50a4d622f6310a`
latest_guest_execution_security_boundary_evidence: `docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253.md`
latest_guest_execution_security_boundary_status: `pass-code-installed-provider-and-actual-credentialed-smoke`
latest_guest_execution_docs_contract_predecessor_evidence: `docs/ga-ready/evidence/guest-execution-preview-code-level-2026-05-27-04250.md`
latest_guest_execution_product_payload_change: `true`
latest_guest_execution_package_gate_decision: `package-fullgate-installed-current-card-actual-guest-exec-pass-manual-admin-readiness-blocked`
latest_guest_execution_hardening_code_level_evidence: `docs/ga-ready/evidence/guest-execution-redaction-hardening-code-level-2026-05-29.md`
latest_guest_execution_hardening_status: `pass-code-level-promoted-by-04259-package-chain`
latest_guest_execution_hardening_package_gate_candidate: `0.42.59-admin-smoke`
latest_guest_execution_hardening_manual_admin_package_pair_candidate: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
latest_hyperv_qos_mutation_value_hardening_code_level_evidence: `docs/ga-ready/evidence/hyperv-qos-mutation-value-hardening-code-level-2026-05-29.md`
latest_hyperv_qos_mutation_value_hardening_status: `pass-code-level-promoted-by-04259-package-chain`
latest_hyperv_qos_mutation_value_hardening_package_gate_candidate: `0.42.59-admin-smoke`
latest_hyperv_qos_mutation_value_hardening_manual_admin_package_pair_candidate: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
latest_hyperv_qos_mutation_value_hardening_problem_codes: `PCV_VM_QOS_STORAGE_RANGE_INVALID`, `PCV_VM_QOS_NETWORK_RANGE_INVALID`
next_manual_admin_package_pair_trigger: `product-payload-change-after-04274`
previous_04274_next_manual_admin_package_pair_trigger: `product-payload-change-after-04273`
next_manual_admin_package_pair_candidate: `0.42.74-admin-smoke -> next-admin-smoke-required`
previous_04274_next_manual_admin_package_pair_candidate: `0.42.73-admin-smoke -> 0.42.74-admin-smoke`
next_manual_admin_package_pair_candidate_status: `not-opened-awaiting-next-product-payload`
previous_04274_next_manual_admin_package_pair_candidate_status: `pass-closed-awaiting-current-evidence-promotion`
next_manual_admin_package_pair_payload_change_source_commit_range: `adc04673b569ef9b587371fdb23bc11ceb14e2e2..next-admin-smoke-required`
previous_04274_next_manual_admin_package_pair_payload_change_source_commit_range: `b84441f0750a9f77fd0588a86912dbdb68b94f0c..adc04673b569ef9b587371fdb23bc11ceb14e2e2`
next_manual_admin_package_pair_payload_changed_source_file_count: `0`
previous_04274_next_manual_admin_package_pair_payload_changed_source_file_count: `33`
next_manual_admin_package_pair_target_package_root: `not-opened`
previous_04274_next_manual_admin_package_pair_target_package_root: `artifacts/admin-smoke-package-20260820-04274`
next_manual_admin_package_pair_target_package_evidence: `not-opened`
previous_04274_next_manual_admin_package_pair_target_package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md`
next_manual_admin_package_pair_target_msi_sha256: `not-opened`
previous_04274_next_manual_admin_package_pair_target_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
next_manual_admin_package_pair_target_provenance_commit: `not-opened`
previous_04274_next_manual_admin_package_pair_target_provenance_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
next_manual_admin_package_pair_update_zip_sha256: `not-opened`
previous_04274_next_manual_admin_package_pair_update_zip_sha256: `cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1`
next_manual_admin_package_pair_descriptor_batch_id: `not-opened`
previous_04274_next_manual_admin_package_pair_descriptor_batch_id: `manual-admin-campaign-descriptor-20260820-04273-04274-closed`
next_manual_admin_package_pair_campaign: `not-opened`
previous_04274_next_manual_admin_package_pair_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md`
previous_04273_next_manual_admin_package_pair_candidate: `0.42.73-admin-smoke -> next-admin-smoke-required`
previous_04273_next_manual_admin_package_pair_candidate_status: `not-opened-awaiting-next-product-payload`
previous_04272_next_manual_admin_package_pair_trigger: `product-payload-change-after-04272`
previous_04272_next_manual_admin_package_pair_candidate: `0.42.72-admin-smoke -> 0.42.73-admin-smoke`
previous_04272_next_manual_admin_package_pair_candidate_status: `pass-closed`
previous_04272_next_manual_admin_package_pair_payload_change_source_commit_range: `02428fabfe5550e0bb3e412db3da29e8ccb57d40..b84441f0750a9f77fd0588a86912dbdb68b94f0c`
previous_04272_next_manual_admin_package_pair_payload_changed_source_file_count: `11`
previous_04271_next_manual_admin_package_pair_trigger: `product-payload-change-after-04271`
previous_04271_next_manual_admin_package_pair_candidate: `0.42.71-admin-smoke -> 0.42.72-admin-smoke`
previous_04271_next_manual_admin_package_pair_candidate_status: `open-not-executed-awaiting-next-payload`
previous_04271_next_manual_admin_package_pair_payload_change_source_commit_range: `821a6a342465ee1c8e17bd8d9a9aa4b27a0a6d6d..ba918e7a`
previous_04271_next_manual_admin_package_pair_payload_changed_source_file_count: `13`
previous_04259_next_manual_admin_package_pair_trigger: `next-product-payload-target-or-dedicated-clean-host`
previous_04259_next_manual_admin_package_pair_candidate: `0.42.59-admin-smoke -> 0.42.60-admin-smoke`
post_04259_public_boundary_product_payload_change_detected: `true`
post_04259_next_product_payload_package_candidate: `0.42.60-admin-smoke`
post_04259_public_boundary_package_chain_decision: `opened-next-product-payload-candidate-current-evidence-rollup`
post_04259_public_boundary_docs_maintenance_product_payload_change_detected: `false`
post_04259_public_boundary_docs_maintenance_package_chain_decision: `no-new-package-candidate-existing-04260-current-card-payload-candidate`
post_04259_public_boundary_recursive_evidence_policy: `docs-maintenance-postpush-does-not-open-additional-package-candidate`
post_04259_installed_account_novnc_smoke_decision: `not-run-no-account-novnc-payload-change-after-04258`
post_04259_actual_vm_guest_qos_smoke_decision: `not-run-no-guest-execution-or-qos-provider-payload-change-after-04259`
post_04255_followup_evidence: `docs/ga-ready/evidence/post-04255-followup-execution-2026-05-28.md`
post_04255_installed_account_novnc_evidence: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup.md`
post_04255_rebaseline_readiness_summary: `artifacts/manual-admin-campaign-20260528-04255-next/rebaseline-readiness/summary.json`
post_04255_package_pair_decision: `not-opened-no-next-product-payload-target`
post_04255_next_package_pair_candidate: `0.42.55-admin-smoke -> next-admin-smoke-required`
post_04226_ledger_contract_followup: `docs/ga-ready/evidence/post-04226-ledger-contract-followup-2026-05-17.md`
post_04226_pre_branch_product_payload_change_detected: `false`
post_04226_branch_product_payload_change: `true`
post_04226_next_product_payload_package_build_trigger: `post-04226-ledger-contract-merge`
current_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md`
previous_04273_current_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-14-04273-hostmutation.md`
current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260820-04274`
previous_04273_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260814-04273`
latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260820-04274`
previous_04273_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260814-04273`
previous_04272_current_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-09-04272-hostmutation.md`
previous_04272_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260809-04272`
previous_04272_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260809-04272`
previous_04271_current_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-08-04271-hostmutation.md`
previous_04271_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260808-04271`
previous_04271_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260808-04271`
previous_04270_current_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-06-04270-hostmutation.md`
previous_04270_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260806-04270`
previous_04270_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260806-04270`
previous_04259_current_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md`
previous_04259_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260529-04259`
previous_04259_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260529-04259`
previous_04257_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260528-04257`
previous_04257_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260528-04257`
previous_04256_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260528-04256`
previous_04256_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260528-04256`
current_full_admin_host_mutation_current_card: `artifacts/installed-operator-surface-current-card-20260820-04274/summary.json`
previous_04273_current_full_admin_host_mutation_current_card: `artifacts/installed-operator-surface-current-card-20260814-04273/summary.json`
current_full_admin_host_mutation_payload_aggregate_sha256: `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`
previous_04273_current_full_admin_host_mutation_payload_aggregate_sha256: `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85`
current_full_admin_host_mutation_operational_msi_sha256: `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`
previous_04273_current_full_admin_host_mutation_operational_msi_sha256: `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1`
current_full_admin_host_mutation_provenance_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
previous_04273_current_full_admin_host_mutation_provenance_commit: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
current_full_admin_host_mutation_routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260820-04274`
previous_04273_current_full_admin_host_mutation_routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260814-04273`
current_full_admin_host_mutation_os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260820-04274`
previous_04273_current_full_admin_host_mutation_os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260814-04273`
previous_04272_current_full_admin_host_mutation_current_card: `artifacts/installed-operator-surface-current-card-20260809-04272/summary.json`
previous_04272_current_full_admin_host_mutation_payload_aggregate_sha256: `deb40a67c5913fd3129adcdbf5aaec29951ce1b223647f28e7df4f6b141c8933`
previous_04272_current_full_admin_host_mutation_operational_msi_sha256: `36561d9304511464378cf0f445ca9525fbdc3254bd85f76a724abba7ad4472aa`
previous_04272_current_full_admin_host_mutation_provenance_commit: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
previous_04272_current_full_admin_host_mutation_routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260809-04272`
previous_04272_current_full_admin_host_mutation_os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260809-04272`
previous_04271_current_full_admin_host_mutation_current_card: `artifacts/installed-operator-surface-current-card-20260809-04271/summary.json`
previous_04271_current_full_admin_host_mutation_payload_aggregate_sha256: `6f325c245808d5d3bb6ead60184cb9c0c2065d79552e22b673ba1be7a010ca16`
previous_04271_current_full_admin_host_mutation_operational_msi_sha256: `4748cc7453ac85178830c179533e7236ed4d3eb15ddb3f968e1dbd4934c27156`
previous_04271_current_full_admin_host_mutation_provenance_commit: `80f69f31464ce07b2c9eca19211adf1232ea75f6`
previous_04271_current_full_admin_host_mutation_routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260808-04271`
previous_04271_current_full_admin_host_mutation_os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260808-04271`
previous_04270_current_full_admin_host_mutation_current_card: `artifacts/installed-operator-surface-current-card-20260806-04270/summary.json`
previous_04270_current_full_admin_host_mutation_payload_aggregate_sha256: `625a08ce4fcc4435c2ffa9af6804dbffc9c4b87450ea4b0613b1df52cb217f99`
previous_04270_current_full_admin_host_mutation_operational_msi_sha256: `90aeda60633ec7e6d32d88f71cbea2b2d5bb54eff205cf49d51cd894b44d8165`
previous_04270_current_full_admin_host_mutation_provenance_commit: `e91389880febdfb3c1ba430f97c84c2f7e006591`
previous_04270_current_full_admin_host_mutation_routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260806-04270`
previous_04270_current_full_admin_host_mutation_os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260806-04270`
previous_04259_current_full_admin_host_mutation_current_card: `artifacts/installed-operator-surface-current-card-20260529-04259/summary.json`
previous_04259_current_full_admin_host_mutation_payload_aggregate_sha256: `3f015e7743efac3b61de81962c236a03c1bcf882053fc92fd3c525da280a1687`
current_full_admin_host_mutation_current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
current_full_admin_host_mutation_runtime_api_registry_bridge_contract: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
current_full_admin_host_mutation_runtime_api_registry_bridge_route_count: `4`
post_04227_host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
post_04227_host_ops_lifecycle_bucket_count: `6`
post_04227_host_ops_lifecycle_bucket_contract_key: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
post_04227_manual_admin_current_card_recheck: `pass`
current_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-21-04274-p0-landing-pass.md`
previous_04273_current_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md`
current_public_boundary_main_push_run_id: `32388996125`
previous_04273_current_public_boundary_main_push_run_id: `31737488576`
current_public_boundary_main_push_job_id: `96490306459`
previous_04273_current_public_boundary_main_push_job_id: `94572517694`
current_public_boundary_main_push_head_sha: `5f9cecfd5507e7e5dd726601aae3760e4e1b558c`
previous_04273_current_public_boundary_main_push_head_sha: `291435e374efef7f9639b820ac197c11e2c7e8a4`
current_public_boundary_main_push_pr: `none-direct-main-push`
current_public_boundary_main_push_evidence_gap: `none-dedicated-04274-p0-landing-evidence`
previous_04273_current_public_boundary_main_push_evidence_gap: `none-dedicated-04273-promotion-postpush-evidence`
current_public_boundary_main_push_undocumented_latest_run_id: `none`
current_public_boundary_main_push_undocumented_latest_head_sha: `none`
current_public_boundary_main_push_product_payload_change_detected: `true`
previous_04273_current_public_boundary_main_push_product_payload_change_detected: `false`
current_public_boundary_main_push_package_candidate_decision: `landed-already-validated-as-0.42.74-admin-smoke`
previous_04273_current_public_boundary_main_push_package_candidate_decision: `docs-only-followup-retains-0.42.73-admin-smoke`
previous_pr187_current_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md`
previous_pr187_current_public_boundary_main_push_run_id: `31579083573`
previous_pr187_current_public_boundary_main_push_job_id: `94057811212`
previous_pr187_current_public_boundary_main_push_head_sha: `a626a7e15d51903f2df5d83d48ffcd2c2115dfc1`
previous_pr187_current_public_boundary_main_push_pr: `[private-archive-repository]/pull/187`
previous_pr187_current_public_boundary_main_push_evidence_gap: `none-dedicated-pr187-postmerge-evidence`
previous_pr187_current_public_boundary_main_push_undocumented_latest_head_sha: `none`
previous_pr187_current_public_boundary_main_push_package_candidate_decision: `docs-only-followup-retains-0.42.72-admin-smoke`
previous_pr186_current_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-09-pr186-postmerge-pass.md`
previous_pr186_current_public_boundary_main_push_run_id: `31302773929`
previous_pr186_current_public_boundary_main_push_job_id: `93218124085`
previous_pr186_current_public_boundary_main_push_head_sha: `02428fabfe5550e0bb3e412db3da29e8ccb57d40`
previous_pr186_current_public_boundary_main_push_pr: `[private-archive-repository]/pull/186`
previous_pr186_current_public_boundary_main_push_product_payload_change_detected: `true`
previous_pr186_current_public_boundary_main_push_package_candidate_decision: `opened-and-validated-as-0.42.72-admin-smoke`
previous_04271_current_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md`
previous_04271_current_public_boundary_main_push_run_id: `29260188921`
previous_04271_current_public_boundary_main_push_job_id: `86851059567`
previous_04271_current_public_boundary_main_push_head_sha: `e08c67ce2bb80529270e258419948e3c573462c0`
previous_04271_current_public_boundary_main_push_pr: `[private-archive-repository]/pull/171`
previous_04271_current_public_boundary_main_push_evidence_gap: `later-main-pushes-passed-without-a-dedicated-evidence-document`
previous_04271_current_public_boundary_main_push_undocumented_latest_run_id: `31099392944`
previous_04271_current_public_boundary_main_push_undocumented_latest_head_sha: `ba918e7a`
previous_04259_current_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md`
previous_04259_current_public_boundary_main_push_run_id: `26636072420`
previous_04259_current_public_boundary_main_push_job_id: `78496568595`
previous_04259_current_public_boundary_main_push_head_sha: `5a2f91762a6c2a8ab6b84d334fa6cb420474671f`
previous_04259_current_public_boundary_main_push_pr: `none-post-04259-public-boundary-docs-maintenance-main-push`
previous_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-admin-smoke-closure-postpush-pass.md`
previous_public_boundary_main_push_run_id: `26629340294`
previous_public_boundary_main_push_job_id: `78473968530`
previous_public_boundary_main_push_head_sha: `b1733c1d9777d2c0828897ae2751af33a270b2fe`
previous_04257_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04257-main-push-pass.md`
previous_04257_public_boundary_main_push_run_id: `26587524245`
previous_04257_public_boundary_main_push_job_id: `78337437665`
previous_04257_public_boundary_main_push_head_sha: `96182b440b35c17183802ad323a123ff6e4b6730`
previous_04256_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md`
previous_04256_public_boundary_main_push_run_id: `26578120570`
previous_04256_public_boundary_main_push_job_id: `78303066840`
previous_04256_public_boundary_main_push_head_sha: `7a7d5de822bdb058b04149eeeef0a7eb462828b5`
previous_04254_running_cancel_public_boundary_main_push_run_id: `26556328902`
previous_04254_running_cancel_public_boundary_main_push_job_id: `78228845568`
previous_04254_running_cancel_public_boundary_main_push_head_sha: `2c11e359709c775be7a57ea9624716720c5b62d6`
previous_credentialed_windows_guest_execution_public_boundary_main_push_run_id: `26516950720`
previous_credentialed_windows_guest_execution_public_boundary_main_push_job_id: `78096741408`
previous_credentialed_windows_guest_execution_public_boundary_main_push_head_sha: `9c9a2d16ce5e0dd7d18df8c5e497eb89b343acc4`
previous_04253_evidence_closure_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md`
previous_04253_evidence_closure_public_boundary_main_push_run_id: `26511891436`
previous_04253_evidence_closure_public_boundary_main_push_job_id: `78078338831`
previous_04253_evidence_closure_public_boundary_main_push_head_sha: `153edc0c1977d1d39249846dbaeff421810c44e8`
previous_04253_evidence_closure_rollforward_public_boundary_main_push_run_id: `26510159990`
previous_04253_evidence_closure_earlier_rollforward_public_boundary_main_push_run_id: `26496046109`
previous_04253_evidence_closure_gates_rollforward_public_boundary_main_push_run_id: `26495580805`
previous_04253_evidence_closure_initial_public_boundary_main_push_run_id: `26494683032`
previous_04253_provider_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-provider-postpush-pass.md`
previous_04253_provider_public_boundary_main_push_run_id: `26494136304`
previous_04253_provider_public_boundary_main_push_job_id: `78018181426`
previous_04253_provider_public_boundary_main_push_head_sha: `824540bea237011b73b00c53ff399675b8346c7f`
previous_04250_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass.md`
previous_04250_public_boundary_main_push_run_id: `26489610881`
previous_04250_public_boundary_main_push_job_id: `78004396577`
previous_04250_public_boundary_main_push_head_sha: `baba155d6adfd4c9e2b2ba179d6727bb5035d1fc`
previous_04249_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04249-guest-execution-postpush-pass.md`
previous_04249_public_boundary_main_push_run_id: `26449795425`
previous_04249_public_boundary_main_push_job_id: `77866996627`
previous_04249_public_boundary_main_push_head_sha: `d09ecfc425f6050a2c182cbcb3090ad2f9fa4827`
previous_04248_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04248-manual-admin-postpush-pass.md`
previous_04248_public_boundary_main_push_run_id: `26445409133`
previous_04248_public_boundary_main_push_job_id: `77850326001`
previous_04248_public_boundary_main_push_head_sha: `ea1e7b85757f35feb10811dda4bbc38d94b304ac`
previous_04245_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass.md`
previous_04245_public_boundary_main_push_run_id: `26413569064`
previous_04245_public_boundary_main_push_job_id: `77753058728`
previous_04245_public_boundary_main_push_head_sha: `4f1f0bd8f7ffe9488dbb7175f65013870cf8d58f`
previous_pr169_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md`
previous_pr169_public_boundary_main_push_run_id: `26288103559`
previous_pr169_public_boundary_main_push_job_id: `77380766318`
previous_pr169_public_boundary_main_push_head_sha: `11b123311d718cf77e87ccc7b8dea7c5728dc463`
previous_pr168_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr168-postmerge-pass.md`
previous_pr168_public_boundary_main_push_run_id: `26233838385`
previous_pr168_public_boundary_main_push_job_id: `77201340972`
previous_pr168_public_boundary_main_push_head_sha: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
current_public_boundary_checkout_action_version: `actions/checkout@v6.0.2`
current_public_boundary_public_trusted_signing: `not-claimed`
current_public_boundary_external_stable_publication: `not-claimed`
previous_pr163_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr163-postmerge-pass.md`
previous_pr163_public_boundary_main_push_run_id: `26164349961`
previous_pr163_public_boundary_main_push_job_id: `76964254604`
previous_pr163_public_boundary_main_push_head_sha: `465e7b8ef79a1c05913107fa1364850e8dd387e9`
post_pr164_installed_cli_qos_guest_targeted_smoke: `docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`
post_pr164_installed_cli_qos_guest_targeted_smoke_artifact: `artifacts/installed-cli-qos-guest-smoke-20260521-04239/summary.json`
post_pr164_product_payload_change_detected: `false`
post_pr164_next_product_payload_package_candidate: `0.42.40-admin-smoke`
post_pr164_package_chain_decision: `not-opened-no-product-payload-change-docs-tools-evidence-only`
post_pr164_web_tui_qos_guest_readback_decision: `defer-direct-web-tui-control-no-product-payload-change`
post_04240_operator_surface_qos_guest_readback_code_level: `docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`
post_04240_web_tui_qos_guest_readback_decision: `implemented-readback-surface-no-direct-control`
post_04240_product_payload_change_detected: `true`
post_04240_next_product_payload_package_candidate: `0.42.40-admin-smoke`
post_04240_package_chain_status: `closed-manual-admin-package-pair-04239-04240`
post_04240_manual_admin_package_pair_candidate: `0.42.39-admin-smoke -> 0.42.40-admin-smoke`
post_04240_manual_admin_package_pair_status: `pass-closed`
post_04240_descriptor_evidence: `manual-admin-campaign-descriptor-20260521-04239-04240-closed`
post_04240_installed_operator_surface_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md`
post_04240_installed_operator_surface_current_card_artifact: `artifacts/installed-operator-surface-current-card-20260521-04240/summary.json`
post_04240_actual_vm_qos_guest_readback_evidence: `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md`
post_04240_actual_vm_qos_guest_readback_artifact: `artifacts/web-tui-qos-guest-readback-actual-vm-20260521-04240/summary.json`
post_04240_installed_tui_row_projection_status: `blocker-found-installed-0.42.40`
post_04240_source_tui_row_projection_fix_status: `pass-code-level`
post_04241_package_chain_trigger: `0.42.41-admin-smoke-required-for-installed-TUI-row-projection-fix`
post_pr169_public_boundary_followup_evidence: `docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md`
post_pr169_product_payload_change_detected: `false`
post_pr169_next_product_payload_package_candidate: `0.42.42-admin-smoke`
post_pr169_package_chain_decision: `not-run-no-product-payload-change-current-0.42.41-admin-smoke`
post_pr169_manual_admin_package_pair_decision: `deferred-until-next-product-payload-change-after-pr169`
post_pr169_installed_account_novnc_smoke_decision: `not-run-no-operator-surface-payload-change-after-pr169`
previous_pr160_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-19-pr160-postmerge-pass.md`
previous_pr160_public_boundary_main_push_run_id: `26101838192`
previous_pr160_public_boundary_main_push_job_id: `76754696421`
previous_pr160_public_boundary_main_push_head_sha: `51a21d7c8612f598b85eeb58818ad3d61136c320`
post_04232_pr156_followup_evidence: `docs/ga-ready/evidence/post-04232-pr156-public-boundary-followup-2026-05-18.md`
post_04232_pr156_product_payload_change_detected: `false`
post_04232_pr156_package_chain_decision: `historical-deferred-no-product-payload-change-after-pr156`
post_04232_pr156_manual_admin_package_pair_decision: `historical-deferred-until-followup-user-approved-04232-chain`
post_04232_04234_package_chain_status: `closed-manual-admin-package-pair-04232-04234`
post_04232_04234_manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md`
post_04232_04234_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260519-04232-04234-closed`
post_04232_04234_target_msi_sha256: `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`
post_04232_04234_update_zip_sha256: `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`
post_04232_04234_provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
post_04231_04232_package_chain_status: `historical-closed-manual-admin-package-pair-04231-04232`
post_04231_04232_manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04231-04232.md`
post_04231_04232_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260519-04231-04232-closed`
status: `04257-fullgate-manual-admin-package-pair-account-novnc-pass`
manual_admin_next_package_pair_candidate: `0.42.57-admin-smoke -> next-admin-smoke-required`
manual_admin_next_package_pair_candidate_status: `awaiting-next-product-payload-target-after-04257-closure`
previous_04255_manual_admin_next_package_pair_candidate: `0.42.55-admin-smoke -> next-admin-smoke-required`
previous_04255_manual_admin_next_package_pair_candidate_status: `opened-and-closed-by-04255-to-04256-package-pair`
previous_04250_04254_manual_admin_next_package_pair_candidate: `0.42.50-admin-smoke -> 0.42.54-admin-smoke`
previous_04250_04254_manual_admin_next_package_pair_candidate_status: `blocked-by-installed-baseline-version-mismatch`
latest_guest_execution_actual_windows_credentialed_smoke_evidence: `docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md`
latest_guest_execution_actual_windows_credentialed_smoke_status: `pass-installed-windows-vhd-credentialed-guest-exec-04255`
latest_guest_execution_persistent_windows_guest_policy_evidence: `docs/ga-ready/evidence/persistent-windows-guest-target-policy-2026-05-28-04255.md`
latest_guest_execution_persistent_windows_guest_policy_status: `keep-after-04255-fullgate`
previous_guest_execution_actual_windows_credentialed_smoke_evidence: `docs/ga-ready/evidence/guest-execution-actual-vm-web-tui-smoke-2026-05-27-04253-blocked.md`
previous_guest_execution_actual_windows_credentialed_smoke_status: `blocked-iso-boot-shell-pass-missing-installed-guest-credential`
latest_guest_execution_running_cancel_policy_evidence: `docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md`
latest_guest_execution_running_cancel_policy_status: `pass-installed-windows-guest-running-cancel`
previous_guest_execution_running_interrupt_code_evidence: `docs/ga-ready/evidence/guest-execution-running-interrupt-code-level-2026-05-28.md`
previous_guest_execution_running_interrupt_code_status: `pass-code-level-promoted-by-04254-installed-smoke`
previous_guest_execution_running_cancel_policy_evidence: `docs/ga-ready/evidence/guest-execution-running-cancel-policy-2026-05-27-04253.md`
previous_guest_execution_running_cancel_policy_status: `accepted-deferred-product-payload`
latest_guest_execution_running_interrupt_design: `docs/superpowers/specs/2026-05-27-purecvisor-desktop-node-guest-execution-running-interrupt-cancel-design.md`
latest_manual_admin_04250_04254_baseline_host_prep_evidence: `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-04255-fullgate.md`
latest_manual_admin_04250_04254_baseline_host_prep_status: `blocked-missing-dedicated-baseline-host-after-04255-fullgate`
latest_operator_surface_running_job_cancel_affordance_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md`
latest_operator_surface_running_job_cancel_affordance_status: `pass-installed-04255-current-card`
previous_manual_admin_04250_04253_baseline_host_prep_evidence: `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-27-04250-04253-blocked-post-ci-rollforward.md`
previous_manual_admin_04250_04253_baseline_host_prep_status: `blocked-missing-dedicated-baseline-host`
previous_04234_status: `closed-package-pair-04232-04234-pass-and-04234-fullgate-current-card-pass-awaiting-next-product-payload`
previous_04234_current_manual_admin_package_pair: `0.42.32-admin-smoke -> 0.42.34-admin-smoke`
previous_04234_manual_admin_next_package_pair_candidate: `pending-next-product-payload-after-04234-package-pair`
previous_04234_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260519-04234`
previous_04234_latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260519-04234`
previous_04234_full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md`
previous_04234_full_admin_host_mutation_payload_aggregate_sha256: `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`
previous_pr156_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`
previous_pr156_public_boundary_main_push_run_id: `26017721669`
previous_pr156_public_boundary_main_push_job_id: `76471545641`
previous_pr156_public_boundary_main_push_head_sha: `a4509c552c003ee0fc87b54b26529686e6dfeb84`
previous_pr155_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`
previous_pr155_public_boundary_main_push_run_id: `26013384587`
previous_pr155_public_boundary_main_push_job_id: `76458402221`
previous_pr155_public_boundary_main_push_head_sha: `2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f`
post_04231_pr155_followup_evidence: `docs/ga-ready/evidence/post-04231-pr155-public-boundary-followup-2026-05-18.md`
post_04231_pr155_product_payload_change_detected: `false`
post_04231_pr155_package_chain_decision: `deferred-no-product-payload-change-after-pr155`
post_04231_pr155_manual_admin_package_pair_decision: `deferred-until-next-product-payload-change-after-pr155`
post_04231_local_worktree_triage_evidence: `docs/ga-ready/evidence/local-worktree-triage-2026-05-18-04231.md`
post_04231_local_worktree_patch_equivalent_delete_candidate_count: `13`
previous_pr154_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md`
previous_pr154_public_boundary_main_push_run_id: `25989986761`
previous_pr154_public_boundary_main_push_job_id: `76394250912`
previous_pr154_public_boundary_main_push_head_sha: `d7f611dfc14a9fa1507f936559209513272b585a`
post_04230_pr154_followup_evidence: `docs/ga-ready/evidence/post-04230-pr154-public-boundary-followup-2026-05-18.md`
post_04230_pr154_product_payload_change_detected: `false`
post_04230_pr154_package_chain_decision: `deferred-no-product-payload-change-after-pr154`
post_04230_pr154_manual_admin_package_pair_decision: `deferred-until-next-product-payload-change-after-pr154`
previous_pr153_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`
previous_pr153_public_boundary_main_push_run_id: `25987705546`
previous_pr153_public_boundary_main_push_job_id: `76388078056`
previous_pr153_public_boundary_main_push_head_sha: `d306712ad671c8a00d5c560765b8952e24a07502`
previous_pr152_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md`
previous_pr152_public_boundary_main_push_run_id: `25985786230`
previous_pr152_public_boundary_main_push_job_id: `76382711230`
previous_pr152_public_boundary_main_push_head_sha: `ca07514097f4e9524a7f3630d321c9666593c962`
previous_pr151_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`
previous_pr151_public_boundary_main_push_run_id: `25984814303`
previous_pr151_public_boundary_main_push_job_id: `76380096421`
previous_pr151_public_boundary_main_push_head_sha: `26ae50fa7bef11b4919b441e706bde505463aded`
post_04227_public_boundary_followup_evidence: `docs/ga-ready/evidence/post-04227-pr150-public-boundary-followup-2026-05-17.md`
post_04227_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass.md`
post_04227_public_boundary_main_push_run_id: `25983307305`
post_04227_public_boundary_main_push_job_id: `76375957834`
post_04227_public_boundary_main_push_head_sha: `6d4b5d95742044bdbd8def933fbc8cdefbba71b3`
previous_04227_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr149-postmerge-pass.md`
previous_04227_public_boundary_main_push_run_id: `25974335803`
previous_04227_public_boundary_main_push_job_id: `76351743536`
previous_04227_public_boundary_main_push_head_sha: `dd895306c4b08802d262b4afb890382dd991a4d0`
post_04228_04229_package_chain_status: `closed-manual-admin-package-pair-04228-04229`
post_04227_04228_package_chain_status: `historical-closed-manual-admin-package-pair-04227-04228`
post_04227_04228_package_chain_decision: `opened-0.42.28-admin-smoke-for-host-ops-web-diagnostics-bucket-table`
post_04227_host_ops_web_diagnostics_bucket_table_review: `implemented-host-ops-web-diagnostics-bucket-table-v1`
post_04227_installed_account_novnc_smoke_decision: `completed-for-04228-operator-surface-product-payload`
post_04227_next_operator_surface_installed_account_novnc_smoke_trigger: `completed-2026-05-17-04228`
latest_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04256-04257.md`
status: `04257-fullgate-manual-admin-package-pair-account-novnc-pass`
baseline_version: `0.42.47-admin-smoke`
target_version: `0.42.48-admin-smoke`
previous_04256_latest_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04255-04256.md`
previous_04256_closed_package_pair_baseline_version: `0.42.55-admin-smoke`
previous_04256_closed_package_pair_target_version: `0.42.56-admin-smoke`
previous_04256_closed_package_pair_provenance_commit: `5594adc55b013a2bf3ade9c6ae7171ca37bdbeb0`
previous_04256_closed_package_pair_target_msi_sha256: `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`
previous_04256_closed_package_pair_update_package_sha256: `073a3d3d0a1e6ce6d4e09d2b66154ed957b42fe2bba6e30e4b101a9beac85a24`
previous_04256_closed_package_pair_burn_bundle_sha256: `f10204ab9e17a300c97b4e7e81e22a53ba5ca3db252a1bf7aff9b1bc48db729e`
previous_04256_closed_package_pair_msix_v1_sha256: `d61788ec1cdf794e02891b13ce583826f9bb09b3b87fb4684c3c9590889169bd`
previous_04256_closed_package_pair_msix_v2_sha256: `44db00ac736568b0de185711e099c2b109afddb4de97b2fcb6a5f163050c1e08`
closed_package_pair_baseline_version: `0.42.56-admin-smoke`
closed_package_pair_target_version: `0.42.57-admin-smoke`
closed_package_pair_provenance_commit: `16cc0d6b592d7f2f9ead14c41d8f4ad0e1f28b76`
closed_package_pair_target_msi_sha256: `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`
closed_package_pair_update_package_sha256: `c50e846e51a568a184cd706dc71506cdad95d8248c4e89713f2f52b690236946`
closed_package_pair_burn_bundle_sha256: `a6d6f6d2378e57feafb6ca346464c08258a8822120458204f51570a2a96d0d04`
closed_package_pair_msix_v1_sha256: `6fa8eaefa49c7f5761b4f051ed8e30c055e7dfcfd5cd9f1b515cebc6eed5fea5`
closed_package_pair_msix_v2_sha256: `c6345a59f533af24abcdce33deab0e6d0f43f6da33accab72baa1ac44e36fa3b`
closed_package_pair_clean_host_windows_update: `KB5087545`
descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260528-04256-04257-closed`
descriptor_batch_manifest_helper: `packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1`
descriptor_batch_manifest_host_mutation_performed: `false`
descriptor_summary: `artifacts/manual-admin-campaign-20260528-04256-04257/manual-admin-campaign-descriptor/summary.json`
descriptor_overall_status: `pass`
manual_admin_next_package_pair_candidate: `0.42.57-admin-smoke -> next-admin-smoke-required`
manual_admin_next_package_pair_candidate_status: `awaiting-next-product-payload-target-after-04257-closure`
manual_admin_next_package_pair_candidate_trigger: `next-product-payload-target-or-dedicated-clean-host`
manual_admin_next_package_pair_candidate_next_version_hint: `0.42.57-admin-smoke -> next-admin-smoke-required`
manual_admin_next_package_pair_descriptor_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04256-04257.md`
manual_admin_next_package_pair_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260528-04256-04257-closed`
manual_admin_next_package_pair_descriptor_summary: `artifacts/manual-admin-campaign-20260528-04256-04257/manual-admin-campaign-descriptor/summary.json`
manual_admin_next_package_pair_descriptor_overall_status: `pass`
manual_admin_next_package_pair_descriptor_missing_count: `0`
manual_admin_next_package_pair_descriptor_not_pass_count: `0`
manual_admin_next_package_pair_descriptor_host_mutation_performed: `true`
previous_04256_manual_admin_next_package_pair_readiness_status: `pass`
manual_admin_next_package_pair_readiness_status: `ready-current-baseline-target-package-pair`
manual_admin_next_package_pair_readiness_summary: `artifacts/manual-admin-campaign-20260528-04256-04257/manual-admin-rebaseline-readiness/summary.json`
manual_admin_next_package_pair_target_package_root: `artifacts/admin-smoke-package-20260528-04257`
manual_admin_next_package_pair_target_msi_sha256: `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`
manual_admin_next_package_pair_baseline_msi_sha256: `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`
previous_04255_manual_admin_next_package_pair_descriptor_evidence: `docs/ga-ready/evidence/post-04255-followup-execution-2026-05-28.md`
previous_04250_04254_manual_admin_next_package_pair_descriptor_evidence: `docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-fullgate.md`
previous_04250_04254_manual_admin_next_package_pair_readiness_summary: `artifacts/manual-admin-campaign-20260528-04250-04254/manual-admin-rebaseline-readiness-current-host/summary.json`
previous_04229_status: `closed-package-pair-04228-04229-pass-and-04229-fullgate-current-card-pass`
previous_04229_current_manual_admin_package_pair: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
previous_04229_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`
previous_04229_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
previous_04229_descriptor_summary: `artifacts/manual-admin-campaign-20260517-04228-04229/manual-admin-campaign-descriptor/summary.json`
previous_04229_target_msi_sha256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
previous_04229_update_package_sha256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
previous_04229_provenance_commit: `d306712ad671c8a00d5c560765b8952e24a07502`
previous_04228_status: `closed-package-pair-04227-04228-pass-and-04228-fullgate-current-card-pass-awaiting-next-product-payload`
previous_04228_current_manual_admin_package_pair: `0.42.27-admin-smoke -> 0.42.28-admin-smoke`
previous_04228_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md`
previous_04228_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260517-04227-04228-closed`
previous_04228_descriptor_summary: `artifacts/manual-admin-campaign-20260517-04227-04228/manual-admin-campaign-descriptor/summary.json`
previous_04228_target_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
previous_04228_update_package_sha256: `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`
previous_04228_provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
previous_04227_status: `closed-package-pair-04226-04227-pass-and-04228-fullgate-current-card-pass-awaiting-04227-04228-package-pair`
previous_04227_current_manual_admin_package_pair: `0.42.26-admin-smoke -> 0.42.27-admin-smoke`
previous_04227_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md`
previous_04227_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260517-04226-04227-closed`
previous_04227_descriptor_summary: `artifacts/manual-admin-campaign-20260517-04226-04227/manual-admin-campaign-descriptor/summary.json`
previous_04227_target_msi_sha256: `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`
previous_04227_update_package_sha256: `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`
previous_04227_provenance_commit: `69aba3eb3ff08c843f1a481818ddc86eac2f019b`
previous_04227_manual_admin_next_package_pair_candidate: `0.42.27-admin-smoke -> 0.42.28-admin-smoke`
previous_04227_manual_admin_next_package_pair_candidate_status: `closed-by-20260517-04227-04228-campaign`
post_04228_operator_surface_followup_evidence: `docs/ga-ready/evidence/post-04228-operator-surface-admin-smoke-2026-05-17.md`
post_04228_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`
post_04228_public_boundary_main_push_run_id: `25984814303`
post_04228_public_boundary_main_push_job_id: `76380096421`
post_04228_public_boundary_main_push_head_sha: `26ae50fa7bef11b4919b441e706bde505463aded`
post_04228_package_chain_status: `executed-after-operator-surface-product-payload-change`
post_04228_clean_package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-17-04228.md`
post_04228_clean_package_msi_sha256: `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`
post_04228_full_admin_host_mutation_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-17-04228-hostmutation.md`
post_04228_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260517-04228`
post_04228_full_gate_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
post_04228_provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
post_04228_host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
post_04228_installed_account_novnc_smoke: `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04228.md`
post_04228_installed_operator_surface_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04228.md`
post_04228_next_manual_admin_package_pair_candidate: `0.42.27-admin-smoke -> 0.42.28-admin-smoke`
post_04228_next_manual_admin_package_pair_candidate_status: `closed-by-manual-admin-campaign-2026-05-17-04227-04228`
previous_04226_status: `closed-package-pair-04225-04226-pass-and-04226-fullgate-current-card-pass-awaiting-next-product-payload`
previous_04226_current_manual_admin_package_pair: `0.42.25-admin-smoke -> 0.42.26-admin-smoke`
previous_04226_current_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260516-04226`
previous_04226_manual_admin_next_package_pair_candidate: `pending-next-product-payload-after-04226-package-pair`
previous_status: `closed-package-pair-04224-04225-pass-and-04226-fullgate-current-card-pass-with-04225-04226-candidate-open`
previous_04224_04225_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`
previous_04224_04225_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260516-04224-04225-closed`
previous_04224_04225_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-campaign-descriptor-supervised/summary.json`
previous_04224_04225_target_msi_sha256: `e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`
previous_04224_04225_update_package_sha256: `393a69802c55d9f1b5d34bc5ed47fe2b7b0e89b52b8102ff4bb3c0dbf59e4585`
previous_04226_initial_manual_admin_next_package_pair_candidate: `0.42.25-admin-smoke -> 0.42.26-admin-smoke`
previous_04226_initial_manual_admin_next_package_pair_candidate_status: `04225-04226-readiness-pass-descriptor-blocked-by-missing-evidence`
previous_04226_initial_manual_admin_next_package_pair_candidate_trigger: `lifecycle-evidence-fill`
previous_04226_initial_manual_admin_next_package_pair_candidate_next_version_hint: `0.42.25-admin-smoke -> 0.42.26-admin-smoke`
previous_04226_initial_manual_admin_next_package_pair_descriptor_evidence: `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md`
previous_04226_initial_manual_admin_next_package_pair_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260516-04225-04226`
previous_04226_initial_manual_admin_next_package_pair_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04225-04226/manual-admin-campaign-descriptor/summary.json`
previous_04226_initial_manual_admin_next_package_pair_descriptor_overall_status: `blocked-by-missing-evidence`
previous_04226_initial_manual_admin_next_package_pair_descriptor_missing_count: `4`
previous_04226_initial_manual_admin_next_package_pair_descriptor_not_pass_count: `1`
previous_04226_initial_manual_admin_next_package_pair_readiness_status: `pass`
previous_04226_initial_manual_admin_next_package_pair_readiness_summary: `artifacts/manual-admin-campaign-20260516-04225-04226/manual-admin-rebaseline-readiness/summary.json`
post_04224_manual_admin_descriptor_evidence: `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04223-04224.md`
post_04224_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260516-04223-04224`
post_04224_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04223-04224/manual-admin-campaign-descriptor/summary.json`
post_04224_manual_admin_descriptor_overall_status: `blocked-by-missing-evidence`
post_04224_manual_admin_descriptor_missing_count: `5`
post_04224_manual_admin_descriptor_not_pass_count: `1`
post_04222_package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`
post_04222_package_artifact_root: `artifacts/admin-smoke-package-20260516-04222`
post_04222_package_msi_sha256: `68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`
post_04222_full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`
post_04222_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260516-04222`
post_04222_full_admin_host_mutation_msi_sha256: `35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`
post_04222_provenance_commit: `8a38995cc25a888f64473e9a2869740949ad6b24`
post_04222_installed_operator_surface_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md`
post_04222_installed_operator_surface_artifact_root: `artifacts/installed-operator-surface-current-card-20260516-04222`
post_04222_manual_admin_descriptor_evidence: `docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04221-04222.md`
post_04222_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260516-04221-04222`
post_04222_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04221-04222/manual-admin-campaign-descriptor-supervised/summary.json`
post_04222_manual_admin_descriptor_overall_status: `historical-descriptor-generated-then-burn-blocked`
post_04222_manual_admin_descriptor_missing_count: `4`
post_04222_manual_admin_descriptor_not_pass_count: `1`
post_04222_public_boundary_postmerge_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass.md`
post_04222_public_boundary_postmerge_run_id: `25952150476`
post_04222_public_boundary_postmerge_job_id: `76291983316`
post_04222_package_host_mutation_current_card_evidence: `docs/ga-ready/evidence/post-04222-package-host-mutation-current-card-2026-05-16.md`
post_04221_04222_burn_blocker_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`
post_04221_04222_burn_blocker_status: `blocked-by-burn-credential-manager-idempotence`
post_04223_package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04223.md`
post_04223_package_artifact_root: `artifacts/admin-smoke-package-20260516-04223`
post_04223_package_msi_sha256: `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`
post_04223_update_package_sha256: `6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb`
post_04223_provenance_commit: `676b4177b10dc80209969066857bab6008ff2473`
post_04223_manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`
post_04223_manual_admin_campaign_root: `artifacts/manual-admin-campaign-20260516-04222-04223`
post_04223_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260516-04222-04223-closed`
post_04223_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04222-04223/manual-admin-campaign-descriptor-supervised/summary.json`
post_04223_manual_admin_descriptor_overall_status: `pass`
post_04223_manual_admin_descriptor_missing_count: `0`
post_04223_manual_admin_descriptor_not_pass_count: `0`
post_04223_full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`
post_04223_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260516-04223`
post_04223_full_admin_host_mutation_msi_sha256: `ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932`
post_04223_full_admin_host_mutation_provenance_commit: `d11a096086326004f27facd9612c2296ded15a4b`
post_04223_installed_operator_surface_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md`
post_04223_installed_operator_surface_artifact_root: `artifacts/installed-operator-surface-current-card-20260516-04223`
post_04223_public_boundary_postmerge_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md`
post_04223_public_boundary_postmerge_run_id: `25954744127`
post_04223_public_boundary_postmerge_job_id: `76299282407`
post_04223_aggregate_evidence: `docs/ga-ready/evidence/post-04223-full-host-mutation-current-card-2026-05-16.md`
post_04223_stale_local_codex_branch_cleanup_deleted_count: `12`
post_04223_next_product_payload_candidate: `0.42.24-admin-smoke`
post_04223_next_slice_runtime_api: `current-evidence-rollup`
post_04224_package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`
post_04224_package_artifact_root: `artifacts/admin-smoke-package-20260516-04224`
post_04224_package_msi_sha256: `d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`
post_04224_provenance_commit: `b974d6b541423f2e4160f726f96155b16f105e9d`
post_04224_full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`
post_04224_full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260516-04224`
post_04224_full_admin_host_mutation_msi_sha256: `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`
post_04224_full_admin_host_mutation_provenance_commit: `b974d6b541423f2e4160f726f96155b16f105e9d`
post_04224_installed_operator_surface_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`
post_04224_installed_operator_surface_artifact_root: `artifacts/installed-operator-surface-current-card-20260516-04224`
post_04224_current_evidence_contract: `runtime-api-current-evidence-rollup-v1`
post_04224_public_boundary_scope_lock_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04224-scope-lock-pass.md`
post_04224_public_boundary_scope_lock_run_id: `25958514394`
post_04224_public_boundary_scope_lock_job_id: `76309528498`
post_0425_package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md`
post_0425_package_artifact_root: `artifacts/admin-smoke-package-20260516-04225`
post_0425_package_msi_sha256: `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`
post_0425_provenance_commit: `403d4474c4b88136774600cc81ca2d941c0b5e4b`
post_0425_manual_admin_descriptor_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`
post_0425_manual_admin_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260516-04224-04225-closed`
post_0425_manual_admin_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-campaign-descriptor-supervised/summary.json`
post_0425_manual_admin_readiness_summary: `artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-rebaseline-readiness/summary.json`
post_0425_manual_admin_descriptor_overall_status: `pass`
post_0425_manual_admin_descriptor_missing_count: `0`
post_0425_manual_admin_descriptor_not_pass_count: `0`
post_04221_successor_operator_surface_evidence: `docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md`
post_04221_successor_operator_surface_status: `code-level-and-operator-surface-pass`
post_04221_installed_operator_surface_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04221.md`
post_04221_installed_operator_surface_artifact_root: `artifacts/installed-operator-surface-current-card-20260516-04221`
post_04221_public_boundary_successor_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md`
post_04221_public_boundary_successor_run_id: `25938745434`
post_04221_web_console_diagnostics_registry_bridge_direct_expose: `code-level-applied`
post_04221_next_product_payload_candidate: `0.42.22-admin-smoke`
post_04221_next_package_build_decision: `deferred-open-candidate-after-04221-web-diagnostics-direct-expose`
previous_04219_04220_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04219-04220.md`
previous_04219_04220_status: `closed-package-pair-04219-04220-pass`
previous_04219_04220_descriptor_summary: `artifacts/manual-admin-campaign-20260516-04219-04220/manual-admin-campaign-descriptor-supervised/summary.json`
previous_04219_04220_target_msi_sha256: `794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f`
previous_04219_04220_update_package_sha256: `8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b`
previous_04220_manual_admin_next_package_pair_candidate: `pending-next-product-payload-after-04220-fullgate`
post_04220_dev_slices_evidence: `docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md`
post_04220_dev_slices_status: `code-level-pass`
post_04220_runtime_diagnostics_ops_summary_contract: `runtime-api-diagnostics-ops-summary-contract-v1`
post_04220_hyperv_wmi_common_helper_contract: `hyperv-wmi-common-helper-contract-v1`
post_04220_host_ops_mutation_boundary_contract: `service-eventlog-firewall-truststore-credential-manager-data-root`
post_04220_packaging_release_next_trigger: `product-payload-change-after-04220-fullgate`
post_04220_public_boundary_evidence: `docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md`
post_04220_public_boundary_workflow_run_id: `25933428239`
post_04220_public_boundary_check_run_id: `76232707240`
post_04220_public_boundary_status: `pass`
post_04220_public_boundary_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md`
post_04220_public_boundary_main_push_run_id: `25934411998`
post_04220_public_boundary_main_push_job_id: `76236050409`
post_04220_public_boundary_main_push_head_sha: `3933231e6e2abf3a398dfcc3fdc999b3df38dac6`
post_04220_public_boundary_main_push_checkout_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md`
post_04220_public_boundary_previous_main_push_run_id: `25933861585`
post_04220_public_boundary_previous_main_push_job_id: `76234195716`
post_04220_public_boundary_previous_main_push_head_sha: `686e4201f823295dc65cde302f613a982ab8cade`
post_04220_public_boundary_previous_main_push_evidence: `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md`
post_04220_public_boundary_checkout_action_version: `actions/checkout@v6.0.2`
post_04220_public_boundary_fallback_required_guard: `public-boundary-ci-required`
post_04220_package_build_decision: `deferred-no-product-payload-change-after-04220`
post_04220_public_boundary_public_trusted_signing: `not-claimed`
post_04220_public_boundary_external_stable_publication: `not-claimed`
post_04220_public_boundary_previous_blocker_run_id: `25931297085`
post_04220_public_boundary_previous_blocker: `GitHub billing/spending-limit`
post_ci_maintenance_dev_slices_evidence: `docs/ga-ready/evidence/post-ci-maintenance-dev-slices-2026-05-16.md`
post_ci_maintenance_dev_slices_status: `code-level-pass`
post_ci_maintenance_next_product_payload_candidate: `0.42.21-admin-smoke`
post_ci_maintenance_runtime_api_registry_bridge_contract: `runtime-api-diagnostics-ops-summary-registry-bridge-v2`
post_ci_maintenance_hyperv_provider_callsite_guard: `hyperv-wmi-provider-callsite-drift-guard-v1`
post_ci_maintenance_host_ops_reason_code_contract: `host-ops-dryrun-mutation-reason-code-v1`
post_ci_maintenance_manual_admin_descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
post_ci_maintenance_host_mutation_performed: `false`
previous_status: `closed-package-pair-04216-04218-pass`
previous_descriptor_id: `manual-admin-next-campaign-descriptor-2026-05-15-04216-04218-closed`
previous_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md`
previous_closed_package_pair_baseline_version: `0.42.16-admin-smoke`
previous_closed_package_pair_target_version: `0.42.18-admin-smoke`
previous_closed_package_pair_target_msi_sha256: `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af`
previous_closed_package_pair_update_package_sha256: `8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd`
previous_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260515-04216-04218`
previous_descriptor_summary: `artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised/summary.json`
previous_manual_admin_next_package_pair_candidate: `pending-next-product-payload-after-04218-fullgate`
previous_manual_admin_next_package_pair_candidate_status: `04218-fullgate-closed-awaiting-next-product-payload`
previous_manual_admin_next_package_pair_candidate_trigger: `next-product-payload-change`
previous_manual_admin_next_package_pair_candidate_next_version_hint: `0.42.18-admin-smoke-to-next`
historical_04215_04216_status: `closed-package-pair-04215-04216-pass`
historical_04214_04215_status: `closed-package-pair-04214-04215-pass`
historical_04214_04215_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04214-04215.md`
historical_04214_04215_target_msi_sha256: `80440d55ec99f8fdd738f1b5a3c917226e4b9b604fe58b2944156721e86200c7`
historical_04214_04215_update_package_sha256: `06f5879431bac90da6da09f243c1e91c6bb875358779e4cedc98a9a3860dad6b`
historical_04212_04213_status: `closed-package-pair-04212-04213-pass`
historical_04212_04213_descriptor_id: `manual-admin-next-campaign-descriptor-2026-05-14-04212-04213-closed`
historical_04212_04213_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md`
historical_04212_04213_target_msi_sha256: `414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a`
historical_04212_04213_update_package_sha256: `638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9f700b83059a8c9`
historical_04212_04213_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260514-04212-04213`
historical_04212_04213_descriptor_summary: `artifacts/manual-admin-campaign-20260514-04212-04213/manual-admin-campaign-descriptor-supervised/summary.json`
historical_post_04212_descriptor_id: `manual-admin-next-campaign-descriptor-2026-05-14-post-04212-followup-triage`
historical_post_04212_manual_admin_next_package_pair_candidate_status: `not-opened-no-new-product-payload`
historical_post_04212_manual_admin_next_package_pair_candidate_next_version_hint: `0.42.13-admin-smoke`
historical_04211_04212_status: `closed-package-pair-04211-04212-pass`
historical_04211_04212_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`
historical_04211_04212_target_msi_sha256: `c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e`
historical_04211_04212_update_package_sha256: `91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04`
historical_04211_04212_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260514-04211-04212`
historical_04211_04212_descriptor_summary: `artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised/summary.json`
post_04212_clean_host_runner_guard_evidence: `docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`
post_04212_clean_host_runner_guard_status: `code-level-pass`
post_04212_clean_host_runner_guard_host_mutation_performed: `false`
post_04212_followup_execution_evidence: `docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`
post_04212_followup_main_commit_checked: `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea`
post_04212_followup_product_payload_change_detected: `false`
post_04212_followup_package_build_decision: `deferred-until-next-product-payload-change`
post_04212_followup_full_admin_host_mutation_decision: `not-run-no-product-payload`
post_04212_followup_manual_admin_package_pair_decision: `deferred-until-next-product-payload-change`
post_04212_followup_clean_host_recovery_guard_decision: `ready-for-next-clean-host-run-not-executed`
post_04212_followup_1_2_3_4_5_current_card_evidence: `docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`
post_04212_followup_1_2_3_4_5_status: `pass-dashboard-current-card-smoke-deferred-product-chain`
post_04212_followup_1_2_3_4_5_main_commit_checked: `8224af81c00482145b6c08dcde8c92a039b2aa26`
post_04212_followup_1_2_3_4_5_product_payload_change_detected: `false`
post_04212_followup_1_2_3_4_5_package_build_decision: `deferred-until-next-product-payload-change`
post_04212_followup_1_2_3_4_5_full_admin_host_mutation_decision: `not-run-no-product-payload`
post_04212_followup_1_2_3_4_5_manual_admin_package_pair_decision: `deferred-until-next-product-payload-change`
post_04212_followup_1_2_3_4_5_clean_host_recovery_summary_key_decision: `not-executed-no-package-pair-campaign`
post_04212_followup_1_2_3_4_5_dashboard_current_card_smoke: `pass`
post_04212_followup_1_2_3_4_5_current_card_artifact_root: `artifacts/web-console-current-card-20260514-04212-rerun-followup`
post_04212_followup_1_2_3_4_5_host_mutation_performed: `false`
post_04212_host_mutation_rerun_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`
post_04212_host_mutation_rerun_batch: `full-admin-host-mutation-gate-20260514-04212-rerun`
post_04212_host_mutation_rerun_status: `pass`
post_04212_host_mutation_rerun_host_mutation_performed: `true`
post_04212_host_mutation_explicit_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation.md`
post_04212_host_mutation_explicit_batch: `full-admin-host-mutation-gate-20260514-140126-04212-explicit`
post_04212_host_mutation_explicit_status: `pass`
post_04212_host_mutation_explicit_host_mutation_performed: `true`
post_04212_host_mutation_explicit_msi_sha256: `269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db`
post_04212_host_mutation_explicit_provenance_commit: `d338b8a99f3e1e3839ac89a6de0da034ff3da148`
post_04212_host_mutation_explicit_web_console_current_card_smoke: `pass`
post_04212_host_mutation_explicit_web_console_current_card_artifact_root: `artifacts/web-console-current-card-20260514-140126-04212-explicit`
latest_full_admin_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-22-04241-hostmutation.md`
latest_full_admin_gate_batch: `full-admin-host-mutation-gate-20260528-04257`
latest_full_admin_gate_msi_sha256: `e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb`
latest_full_admin_gate_payload_aggregate_sha256: `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`
latest_full_admin_gate_provenance_commit: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
latest_admin_smoke_package_build: `artifacts/admin-smoke-package-20260522-04241`
latest_admin_smoke_package_build_msi_sha256: `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`
latest_admin_smoke_package_build_payload_aggregate_sha256: `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`
latest_admin_smoke_provenance_commit: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
latest_operational_package_root: `artifacts/admin-smoke-package-20260522-04241`
latest_operational_package_msi_sha256: `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`
latest_operational_package_provenance_commit: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
latest_selectorfix_package_evidence: `docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`
latest_selectorfix_package_root: `artifacts/admin-smoke-package-20260514-04214-selectorfix`
latest_selectorfix_package_version: `0.42.14-admin-smoke`
latest_selectorfix_package_msi_sha256: `dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb`
latest_selectorfix_web_console_current_card_artifact_root: `artifacts/installed-operator-surface-current-card-20260516-04225`
latest_selectorfix_installed_manifest_version: `0.42.25-admin-smoke`
previous_04221_full_admin_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md`
previous_04221_full_admin_gate_batch: `full-admin-host-mutation-gate-20260516-04221`
previous_04221_full_admin_gate_msi_sha256: `f39bbcbba4932ed9ea57abaf3f77c03222ead371febe48ed5ee475eae6cb8551`
previous_04221_full_admin_gate_provenance_commit: `3b8c48deb4c31675f6fce46c320703f23c27c131`
previous_04220_full_admin_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md`
previous_04220_full_admin_gate_batch: `full-admin-host-mutation-gate-20260516-04220`
previous_04220_full_admin_gate_msi_sha256: `12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c`
previous_04220_full_admin_gate_provenance_commit: `0895d018935298721b25b5d9ce1ae083a6690c25`
previous_04220_admin_smoke_package_build: `artifacts/admin-smoke-package-20260516-04220`
previous_04220_selectorfix_web_console_current_card_artifact_root: `artifacts/installed-current-card-20260516-04220-fullgate`
previous_04220_selectorfix_installed_manifest_version: `0.42.20-admin-smoke`
installed_listener_batch_evidence_status: `available`
framework_dependent_regression_04217_status: `superseded-by-04218-self-contained-package`
framework_dependent_regression_04217_clean_host_summary: `artifacts/manual-admin-campaign-20260515-04216-04217/clean-host-updated-os/summary.json`
framework_dependent_regression_04217_error: `PCV_PRODUCT_UPDATE_START_FAILED`
post_04218_contract_alignment_evidence: `docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md`
post_04218_contract_alignment_status: `pass`
post_04218_contract_alignment_host_mutation_performed: `false`
post_04218_runtime_api_diagnostics_bridge: `route-family-evidence-linked`
post_04218_hyperv_dispatch_catalog_contract: `vm-checkpoint-network-fixed`
post_04218_host_ops_lifecycle_buckets: `service-eventlog-firewall-truststore-data-root-separated`
post_04218_packaging_release_next_trigger: `pending-next-product-payload-after-04218-fullgate`
post_04218_operator_surface_journey_alignment: `web-console-tui-cli-current-card`
post_04218_public_boundary_preserved: `adr-0005-closed-adr-0006-internal-only`
previous_04212_full_admin_gate: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation.md`
historical_0429_04211_closed_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`
historical_0429_04211_closed_campaign_descriptor: `artifacts/manual-admin-campaign-20260513-0429-04211/manual-admin-campaign-descriptor-supervised/summary.json`
historical_0429_04211_descriptor_batch_manifest: `manual-admin-campaign-descriptor-20260513-0429-04211`
previous_full_admin_gate_batch: `full-admin-host-mutation-gate-20260513-0429-04211`
historical_0429_04211_target_version: `0.42.11-admin-smoke`
historical_0429_04211_target_msi_sha256: `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`
historical_0429_04211_update_package_sha256: `734114e0ea7c9d486a1d329cd551a6abc34d20f3801a944bd5dbcb8c1c4a9991`
historical_0429_04211_provenance_commit: `987beb51025a5aa926df7d9a905019b4d6d29705`
skipped_target_version: `0.42.10-admin-smoke`
skipped_target_rca_evidence: `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`
skipped_target_rca_status: `historical-duplicate-outer-start-closed-by-04211`
post_merge_main_commit: `14f56fd7348572e1757413657a68cd17c0aeca52`
post_merge_package_provenance_decision: `deferred-no-new-product-payload-after-04211`
post_04210_followup_execution_evidence: `docs/ga-ready/evidence/post-04210-followup-execution-2026-05-13.md`
post_04210_followup_main_commit_checked: `371e05055c7488f923c0038f87f1a1288054c271`
post_04210_followup_product_payload_change_detected: `false`
post_04210_followup_package_build_decision: `deferred-until-next-product-payload-change`
post_04210_followup_full_admin_host_mutation_decision: `not-run-no-new-product-payload`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 0.42.26 Manual-admin Package-pair Closure

`0.42.25-admin-smoke -> 0.42.26-admin-smoke` package-pair는 2026-05-17
manual-admin campaign에서 닫혔다. Evidence는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`이며
campaign root는 `artifacts/manual-admin-campaign-20260517-04225-04226`다.

PASS bucket은 installed update/rollback, Windows Update 포함 dedicated clean-host,
Burn package-pair lifecycle, MSIX package-pair lifecycle, installed runtime ops summary,
descriptor generation v2다. Descriptor batch는
`manual-admin-campaign-descriptor-20260517-04225-04226-closed`, summary는
`artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json`이며
`overall_status=pass`, `missing_count=0`, `not_pass_count=0`이다.

Target operational MSI SHA-256은
`f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`, update ZIP
SHA-256은 `4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4`,
provenance commit은 `d6500c01c972cbc7ca1e290e51120181ceea1501`다. 설치본 runtime
ops summary recheck는 manifest `0.42.26-admin-smoke`, service `Running`, Web `200`,
`/pcv-config.js` `200`, unauthenticated runtime policy `401`/`PCV_AUTH_REQUIRED`,
latest batch `full-admin-host-mutation-gate-20260516-04226`,
`runtime-api-current-evidence-rollup-v1`, registry bridge
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를
확인했다.

2026-05-16 `manual-admin-campaign-descriptor-2026-05-16-04225-04226`는
readiness PASS 이후 lifecycle evidence가 비어 있던 initial blocked descriptor로
보존한다. 최신 open package-pair candidate는 아직 열지 않았고 다음 product payload
변경 시 `pending-next-product-payload-after-04226-package-pair`에서 시작한다.

## 0.42.25 Full Gate / Package-pair Closure

`0.42.25-admin-smoke` previous operational evidence는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04225-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04225.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md`가 소유한다.
Full-gate 및 package-pair target MSI SHA-256은
`e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b`, provenance
commit은 `4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1`다.

`0.42.24-admin-smoke -> 0.42.25-admin-smoke` package-pair는 readiness, installed
update/rollback, Windows Update 포함 clean-host, Burn, MSIX, installed runtime ops
summary, descriptor generation v2를 모두 PASS로 닫았다. Descriptor batch는
`manual-admin-campaign-descriptor-20260516-04224-04225-closed`, descriptor summary는
`artifacts/manual-admin-campaign-20260516-04224-04225/manual-admin-campaign-descriptor-supervised/summary.json`이며
`missing_count=0`, `not_pass_count=0`이다. PR #144 post-merge public-boundary는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md`,
run `25959505688`, job `76312299500`에서 PASS했다.

Earlier package build record `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md`
는 MSI SHA-256 `5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06`,
provenance commit `403d4474c4b88136774600cc81ca2d941c0b5e4b`로 historical package
candidate record로 보존한다.

## 0.42.24 Runtime/API Current Evidence Rollup

`0.42.24-admin-smoke`는 Runtime/API `current_evidence` rollup product payload다.
Package build는 `docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md`가
소유하고, MSI SHA-256은
`d2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e`, provenance
commit은 `b974d6b541423f2e4160f726f96155b16f105e9d`다.

`0.42.24-admin-smoke` full admin host mutation은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md`
및 `full-admin-host-mutation-gate-20260516-04224`가 소유한다. Full-gate MSI
SHA-256은 `0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826`이다.
설치본 Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md`와
`artifacts/installed-operator-surface-current-card-20260516-04224`에서
`batch_evidence.status=available`, latest batch
`full-admin-host-mutation-gate-20260516-04224`, Runtime/API current evidence
`runtime-api-current-evidence-rollup-v1`, registry bridge
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를
확인했다.

`0.42.23-admin-smoke -> 0.42.24-admin-smoke` descriptor는
`docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04223-04224.md`에서
생성됐지만 `missing_count=5`, `not_pass_count=1`로 blocked다. 이 04224 fullgate와
blocked descriptor는 0.42.25 closure 이후 historical predecessor로 보존한다. Current
closed package-pair는 `0.42.25-admin-smoke -> 0.42.26-admin-smoke` PASS evidence가
소유한다.

## 0.42.23 Package-pair Closure

`0.42.23-admin-smoke`는 Credential Manager default transition idempotence fix를
포함한 product payload다. Package build는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04223.md`가 소유하고, MSI
SHA-256은 `2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406`,
provenance commit은 `676b4177b10dc80209969066857bab6008ff2473`이다.

`0.42.22-admin-smoke -> 0.42.23-admin-smoke` package-pair campaign은
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md`가 소유한다.
Readiness, installed update/rollback, Windows Update 포함 clean-host, Burn, MSIX,
installed runtime ops summary, descriptor generation이 모두 PASS이고 descriptor는
`missing_count=0`, `not_pass_count=0`이다.

`0.42.21-admin-smoke -> 0.42.22-admin-smoke` package-pair는 Burn bootstrapper
`CredentialManagerDefaultTransition` idempotence blocker로 PASS claim하지 않는다. 이
이력은 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`
에 보존하며, 0.42.23 campaign이 closure를 소유한다.

## 0.42.23 Full Gate / Next Candidate

`0.42.23-admin-smoke` full admin host mutation은
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md`
및 `full-admin-host-mutation-gate-20260516-04223`가 소유한다. Full-gate MSI SHA-256은
`ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932`, full-gate
provenance commit은 `d11a096086326004f27facd9612c2296ded15a4b`다. 설치본 Web/TUI/CLI
current-card는 `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md`와
`artifacts/installed-operator-surface-current-card-20260516-04223`에서
`batch_evidence.status=available`, latest batch
`full-admin-host-mutation-gate-20260516-04223`, Runtime/API registry bridge
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를
확인했다.

다음 package-pair candidate였던 `0.42.23-admin-smoke -> 0.42.24-admin-smoke`는
descriptor 생성까지 완료됐지만 missing lifecycle evidence 때문에 historical blocked
candidate로 보존한다. `0.42.24-admin-smoke -> 0.42.25-admin-smoke`는 historical
closed package-pair PASS로 보존하고, 최신 closed package-pair는
`0.42.25-admin-smoke -> 0.42.26-admin-smoke`다.

## 0.42.22 Predecessor Current Card / Descriptor Candidate

`0.42.22-admin-smoke`는 Web Console diagnostics panel의 Runtime/API registry bridge
route detail 노출을 포함한 product payload다. Package build는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md`가 소유하고, clean
package MSI SHA-256은
`68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3`이다.

이전 full admin host mutation anchor는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md`
및 `full-admin-host-mutation-gate-20260516-04222`다. Full-gate MSI SHA-256은
`35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c`, provenance commit은
`8a38995cc25a888f64473e9a2869740949ad6b24`다. 설치본 Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md`와
`artifacts/installed-operator-surface-current-card-20260516-04222`에서
`batch_evidence.status=available`, latest batch
`full-admin-host-mutation-gate-20260516-04222`, Runtime/API registry bridge
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`, route detail count `4`를
확인했다.

`0.42.21-admin-smoke -> 0.42.22-admin-smoke` descriptor는
`docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04221-04222.md`가
소유한다. Descriptor batch `manual-admin-campaign-descriptor-20260516-04221-04222`는
non-mutating으로 실행됐고, 후속 실제 campaign은 Burn `CredentialManagerDefaultTransition`
idempotence blocker로 닫히지 않았다. 따라서 이 package-pair는 PASS claim이 아니며
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md`에
historical blocker로 보존한다.

## 0.42.21 Current Card

`0.42.20-admin-smoke -> 0.42.21-admin-smoke` package-pair campaign은
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04220-04221.md`가
소유한다. `0.42.21-admin-smoke` package build,
manual-admin descriptor generation v2, installed update/rollback, Windows Update
포함 clean-host, Burn, MSIX, installed runtime ops summary, full admin host
mutation gate가 모두 PASS다.

이전 installed current-card anchor는
`full-admin-host-mutation-gate-20260516-04221`이며
`runtime-api-diagnostics-ops-summary-registry-bridge-v2`가 ops summary에 표시된다.
Public boundary successor는
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md`
run `25938745434` / job `76250726268`에서 PASS했다. 설치본 Web/TUI/CLI current-card는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04221.md`와
`artifacts/installed-operator-surface-current-card-20260516-04221`에 기록했다. 이 current-card는 internal
admin-smoke evidence이며 public trusted signing 또는 외부 stable publication
evidence가 아니다.

이 descriptor는 현재 닫힌 MANUAL-ADMIN package-pair campaign과 다음 실행 판단을
고정한다. Current closed package-pair claim은
`0.42.25-admin-smoke -> 0.42.26-admin-smoke` PASS evidence가 소유한다.
`0.42.21-admin-smoke`와 이후 이전 package-pair closure들은 historical predecessor로
보존한다. Descriptor batch는 운영 최신 후보에서 계속 제외된다.

## 현재 Package-pair 기준

| 항목 | 값 |
| --- | --- |
| current campaign evidence | `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md` |
| baseline version | `0.42.26-admin-smoke` |
| target version | `0.42.27-admin-smoke` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227` |
| target MSI SHA-256 | `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9` |
| update package SHA-256 | `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997` |
| descriptor summary | `artifacts/manual-admin-campaign-20260517-04226-04227/manual-admin-campaign-descriptor/summary.json` |
| descriptor overall status | `pass` |

## Package-pair PASS Bucket

| Bucket | 상태 | Artifact |
| --- | --- | --- |
| readiness | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/lifecycle/product-update-rollback/summary.json` |
| clean-host install/update/rollback | `pass-with-windows-update` | `artifacts/manual-admin-campaign-20260517-04225-04226/clean-host-updated-os/summary.json` |
| Burn install/repair/remove | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `pass` | `artifacts/msix-package-lifecycle-smoke-20260517-04225-04226/summary.json` |
| installed runtime ops summary | `pass` | `artifacts/manual-admin-campaign-20260517-04225-04226/installed-runtime-ops-summary/summary.json` |

## 다음 Package-pair Candidate

| 항목 | 값 |
| --- | --- |
| candidate | `pending-next-product-payload-after-04227-package-pair` |
| status | `04227-package-pair-closed-awaiting-next-product-payload` |
| next version hint | `0.42.27-admin-smoke -> next` |
| trigger | next Runtime/Core, Hyper-V, Host Ops, Packaging, Operator Surface product payload change |

`docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md`는
`0.42.21-admin-smoke` 이후 trigger를 code/test/operator-surface 계약으로 고정한다.
이번 Web Console diagnostics direct expose는 product payload 변경이므로
`0.42.22-admin-smoke` package와 full admin host mutation gate를 열었고, 후속
Credential Manager idempotence fix는 `0.42.23-admin-smoke` package-pair로 닫혔다.
다음 package-pair는 `0.42.23-admin-smoke` 이후 새 payload 또는 0.42.23 full admin
host mutation 결과를 기준으로 판단한다.

## Post-04212 Runner Guard

`docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md`는
04211→04212 clean-host Windows Update reboot 중 관찰된 heartbeat `NoContact` +
CPU idle 수동 복구를 runner contract로 승격한다. 다음 clean-host package-pair 실행은
`WindowsUpdateNoContactRecoverySeconds` threshold와 `recovery_actions` summary를
기본으로 남긴다. 이 guard 자체는 code-level 변경이며 host mutation을 실행하지 않았다.

## Post-04212 Follow-up Triage

`docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md`는 사용자 승인
`1-2-3-4-5` 이후 새 product payload 변경이 없음을 확인한 triage evidence다.
기준 package provenance commit `8f694dc2494314a6ddd7223f46ec0ba0ca8523e3`부터
`main` `0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea`까지의 변경은 문서, 테스트,
clean-host runner guard에 한정됐다. 따라서 다음 package build와 package-pair
campaign은 `next-product-payload-change` trigger까지 보류한다. 2026-05-14 host
mutation rerun은 같은 `0.42.12-admin-smoke` payload로 current full gate evidence만
갱신했으며, `0.42.13-admin-smoke` 후보를 열지 않는다. Clean-host recovery summary key는 다음 실제 clean-host run에서
`recovery_actions`와 `automatic_recovery_performed`로 판정한다.

## Post-04212 Current-card Follow-up

`docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md`는
사용자 재승인 `1-2-3-4-5` 실행 기록이다. `main`
`8224af81c00482145b6c08dcde8c92a039b2aa26` 기준 `src`, `web`, product wrapper,
installer payload 변경이 없어서 `0.42.13-admin-smoke` package build,
package-pair, clean-host campaign, full admin host mutation gate는 보류했다.

설치본 Web Console smoke artifact
`artifacts/web-console-current-card-20260514-04212-rerun-followup`는 Dashboard와
Evidence view가 `full-admin-host-mutation-gate-20260514-04212-rerun`,
`0.42.12-admin-smoke`를 표시함을 확인했다. Token value는 UI text에 노출되지
않았다. 이 follow-up 자체는 host mutation을 실행하지 않는다.

## 04215 Descriptor Selector Guard Follow-up

`docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md`는
`manual-admin-campaign-descriptor-*` batch가 최신 `batch-runs` 항목일 때도 Web Console
current-card가 descriptor가 아니라 최신 full-admin operational evidence를 선택하는지
확인한다. `0.42.14-admin-smoke` selector fix package 설치 후 canonical
`artifacts` root에서 `batch_evidence.status=available`,
`latest.batch_id=full-admin-host-mutation-gate-20260514-140126-04212-explicit`,
`errors=[]`를 확인했고, Web Console Dashboard/Evidence current-card smoke를
PASS로 닫았다.

이후 `0.42.16 -> 0.42.18` descriptor batch와 04218 full admin host mutation gate를
실행한 뒤 `artifacts/installed-current-card-20260515-04218-fullgate`에서
`latest.batch_id=full-admin-host-mutation-gate-20260515-163107-04218`,
`latest.release.version=0.42.18-admin-smoke`,
`descriptor_batch_id=manual-admin-campaign-descriptor-20260515-04216-04218`,
`descriptor_excluded_from_operational_latest=true`를 다시 확인했다.

## Batch Supervisor Manifest 연결

`PcvBatchSupervisor.psm1`의 `ManualAdminCampaignDescriptor` profile은
`New-PcvManualAdminCampaignDescriptor.ps1 -PlanOnly`를 하나의 non-mutating step으로
만든다. Manifest는 runner summary path를 인자로 받아 descriptor summary를 생성한다.

필수 profile option:

```text
descriptor_artifact_root
campaign_artifact_root
baseline_version
target_version
readiness_summary_path
product_update_summary_path
product_rollback_summary_path
clean_host_summary_path
burn_lifecycle_summary_path
msix_lifecycle_summary_path
installed_runtime_ops_summary_path
```

이 profile은 `requires_admin=false`, `mutates_host=false`이며 host mutation runner를
대체하지 않는다. 실제 install/update/rollback, clean-host VM, Burn/MSIX lifecycle은
계속 `MANUAL-ADMIN`으로 실행한다.

## 실행된 Manifest

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1 -BatchId manual-admin-campaign-descriptor-20260515-04216-04218 -CampaignArtifactRoot artifacts/manual-admin-campaign-20260515-04216-04218 -DescriptorArtifactRoot artifacts/manual-admin-campaign-20260515-04216-04218/manual-admin-campaign-descriptor-supervised -PassThru
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/manual-admin-campaign-descriptor-20260515-04216-04218/manifest.json
```

생성된 manifest:

- batch id: `manual-admin-campaign-descriptor-20260515-04216-04218`
- path: `artifacts/batch-runs/manual-admin-campaign-descriptor-20260515-04216-04218/manifest.json`
- profile: `ManualAdminCampaignDescriptor`
- step: `manual-admin-campaign-descriptor`
- command: `New-PcvManualAdminCampaignDescriptor.ps1 -PlanOnly`
- host mutation: `requires_admin=false`, `mutates_host=false`
- summary: `ok=true`, `status=completed`, `executed_steps=1`

## 이전 실행 Manifest

이전 `0.42.14-admin-smoke -> 0.42.15-admin-smoke` descriptor command는
historical regression fixture로 보존한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1 -BatchId manual-admin-campaign-descriptor-20260514-04214-04215 -CampaignArtifactRoot artifacts/manual-admin-campaign-20260514-04214-04215 -DescriptorArtifactRoot artifacts/manual-admin-campaign-20260514-04214-04215/manual-admin-campaign-descriptor-supervised -PassThru
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04214-04215/manifest.json
```

이전 이전 `0.42.12-admin-smoke -> 0.42.13-admin-smoke` descriptor command도
historical regression fixture로 보존한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1 -BatchId manual-admin-campaign-descriptor-20260514-04212-04213 -CampaignArtifactRoot artifacts/manual-admin-campaign-20260514-04212-04213 -DescriptorArtifactRoot artifacts/manual-admin-campaign-20260514-04212-04213/manual-admin-campaign-descriptor-supervised -PassThru
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04212-04213/manifest.json
```

이전 이전 이전 `0.42.11-admin-smoke -> 0.42.12-admin-smoke` descriptor command도
historical regression fixture로 보존한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1 -BatchId manual-admin-campaign-descriptor-20260514-04211-04212 -CampaignArtifactRoot artifacts/manual-admin-campaign-20260514-04211-04212 -DescriptorArtifactRoot artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised -PassThru
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/manual-admin-campaign-descriptor-20260514-04211-04212/manifest.json
```

## 최신 Full Admin Gate 판단

`0.42.18-admin-smoke` full admin host mutation PASS는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md`가
소유한다.

| 항목 | 값 |
| --- | --- |
| package root | `artifacts/admin-smoke-package-20260515-04218` |
| package build MSI SHA-256 | `459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af` |
| batch id | `full-admin-host-mutation-gate-20260515-163107-04218` |
| route artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260515-163107-04218` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260515-163107-04218` |
| full-gate MSI SHA-256 | `0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09` |
| full-gate provenance commit | `9121d1f5e7fa83d803c484a44698d4fc8e825c19` |
| package provenance commit | `9121d1f5e7fa83d803c484a44698d4fc8e825c19` |
| installed listener current card | `batch_evidence.status=available`, `latest.batch_id=full-admin-host-mutation-gate-20260515-163107-04218`, `descriptor_excluded_from_operational_latest=true` |
| Web Console/current-card smoke | `artifacts/installed-current-card-20260515-04218-fullgate` |

## Historical-only 보존

- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md`는 이전
  package-pair PASS로 보존한다. Target MSI SHA-256은
  `750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1`, update ZIP
  SHA-256은 `734114e0ea7c9d486a1d329cd551a6abc34d20f3801a944bd5dbcb8c1c4a9991`,
  provenance commit은 `987beb51025a5aa926df7d9a905019b4d6d29705`다. 이전 manifest id는
  `manual-admin-campaign-descriptor-20260513-0429-04211`이다.
- `docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md`는
  `0.42.10-admin-smoke` duplicate outer `sc.exe start` 1056 RCA로 보존한다.
- `docs/ga-ready/evidence/manual-admin-campaign-candidate-2026-05-13-0428-0429.md`는
  installed update/rollback-only candidate로 보존한다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`와
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md`는 이전
  package-pair PASS로 보존한다.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`는
  `historical-partial-pass-clean-host-blocked` record로 보존한다.

## 실행 금지 조건

- baseline/target version이 섞이면 실행하지 않는다.
- installed baseline과 runner payload baseline이 다르면 blocker로 기록한다.
- shared developer workstation에서 unattended recurring host mutation으로 실행하지 않는다.
- public signing material, public timestamp URL, external upload credential, public stable
  installer URL 부재를 우회하지 않는다.
