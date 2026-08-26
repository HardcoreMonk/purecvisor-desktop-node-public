# Public Boundary CI Contract

```text
PUBLIC_BOUNDARY_CI_CONTRACT
status: active-non-required-residue
classification: public-boundary-ci-non-required-residue
provider_required: false
residue_workflow: .github/workflows/public-boundary.yml
residue_job_name: public-boundary-ci-required
residue_execution: pester-and-powershell
required_ci_authority_workflow: .github/workflows/development-gates.yml
required_ci_provider_required: true
required_ci_contexts: dotnet,web,delivery,installer-policy
required_ci_final_main_sha: 6e2bdb93ce308b632c929e2c17f5550ac3845401
required_ci_final_main_run_id: 32904006595
required_ci_evidence: docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md
public_boundary_residue_run_id: 32904006619
public_boundary_residue_job_id: 97983888524
public_boundary_residue_head_sha: 6e2bdb93ce308b632c929e2c17f5550ac3845401
ADR-0005: closed-not-adopted
ADR-0006: internal-private-network-only
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
winget_public_submission: out-of-scope
public_stable_installer_url: out-of-scope
public_clean_host_signed_install_update_rollback_smoke: out-of-scope
current_operational_version_anchor: 0.42.74-admin-smoke
current_operational_package_pair: 0.42.73-admin-smoke -> 0.42.74-admin-smoke
current_promotion_eligible: false
current_promotion_blocker: pcv.vm.saved-lifecycle/actual_vm_tested/fail
additional_package_candidate_opened: false
package_candidate_decision: docs-only-required-ci-closure-retains-0.42.74-admin-smoke
historical_04259_current_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md
historical_04259_source_version_anchor: 0.42.59-admin-smoke
historical_04259_postmerge_package_anchor: 0.42.59-admin-smoke-fullgate-manual-admin-closed
historical_04259_main_push_run_id: 26636072420
historical_04259_main_push_job_id: 78496568595
historical_04259_main_push_head_sha: 5a2f91762a6c2a8ab6b84d334fa6cb420474671f
historical_04259_main_push_pr: none-post-04259-public-boundary-docs-maintenance-main-push
historical_04259_next_product_payload_package_candidate: 0.42.60-admin-smoke
historical_04259_recursive_evidence_policy: docs-maintenance-postpush-does-not-open-additional-package-candidate
historical_04259_installed_account_novnc_rerun_decision: not-run-no-account-novnc-payload-change-after-04258
historical_04259_actual_vm_guest_execution_qos_smoke_decision: not-run-no-guest-execution-or-qos-provider-payload-change-after-04259
historical_04259_compatibility_alias_semantics: historical-predecessor-not-current-required-ci-authority
historical_04259_required_verification: packaging-pester-public-boundary-guard
historical_04259_latest_main_push_run_id: 26636072420
historical_04259_latest_main_push_job_id: 78496568595
historical_04259_latest_main_push_pr: none-post-04259-public-boundary-docs-maintenance-main-push
historical_04259_previous_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-admin-smoke-closure-postpush-pass.md
historical_04259_previous_main_push_run_id: 26629340294
historical_04259_previous_main_push_job_id: 78473968530
historical_04259_previous_main_push_head_sha: b1733c1d9777d2c0828897ae2751af33a270b2fe
previous_04257_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04257-main-push-pass.md
previous_04257_main_push_run_id: 26587524245
previous_04257_main_push_job_id: 78337437665
previous_04257_main_push_head_sha: 96182b440b35c17183802ad323a123ff6e4b6730
previous_04256_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md
previous_04256_main_push_run_id: 26578120570
previous_04256_main_push_job_id: 78303066840
previous_04256_main_push_head_sha: 7a7d5de822bdb058b04149eeeef0a7eb462828b5
previous_04254_fullgate_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04254-fullgate-evidence-rollforward-postpush-pass.md
previous_04254_fullgate_main_push_run_id: 26558089193
previous_04254_fullgate_main_push_job_id: 78234262641
previous_04254_fullgate_main_push_head_sha: 958052181012f7d1be6ccff535316bfaeeef07df
previous_04254_running_cancel_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04254-running-cancel-evidence-rollforward-postpush-pass.md
previous_04254_running_cancel_main_push_run_id: 26556328902
previous_04254_running_cancel_main_push_job_id: 78228845568
previous_04254_running_cancel_main_push_head_sha: 2c11e359709c775be7a57ea9624716720c5b62d6
previous_04253_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md
previous_04253_main_push_run_id: 26518952796
previous_04253_main_push_job_id: 78104102372
previous_04253_main_push_head_sha: 12bc72e856ea9ac7c6d54c4094873b2d8db9f672
previous_credentialed_windows_guest_execution_main_push_run_id: 26516950720
previous_credentialed_windows_guest_execution_main_push_job_id: 78096741408
previous_credentialed_windows_guest_execution_main_push_head_sha: 9c9a2d16ce5e0dd7d18df8c5e497eb89b343acc4
previous_04253_evidence_closure_latest_main_push_run_id: 26511891436
previous_04253_evidence_closure_latest_main_push_job_id: 78078338831
previous_04253_evidence_closure_latest_main_push_head_sha: 153edc0c1977d1d39249846dbaeff421810c44e8
previous_04253_evidence_closure_rollforward_main_push_run_id: 26510159990
previous_04253_evidence_closure_rollforward_main_push_job_id: 78072405936
previous_04253_evidence_closure_rollforward_main_push_head_sha: 89132b24035cca661cef035730f8d6970d1e6726
previous_04253_evidence_closure_earlier_rollforward_main_push_run_id: 26496046109
previous_04253_evidence_closure_earlier_rollforward_main_push_job_id: 78024393463
previous_04253_evidence_closure_earlier_rollforward_main_push_head_sha: 332e534cdb31f43341efcd6aa3e3e1bf75b34def
previous_04253_evidence_closure_gates_rollforward_main_push_run_id: 26495580805
previous_04253_evidence_closure_gates_rollforward_main_push_job_id: 78022903624
previous_04253_evidence_closure_gates_rollforward_main_push_head_sha: f78b0fd01f48e4babe200bd8dea112d1c5cb797f
previous_04253_evidence_closure_initial_main_push_run_id: 26494683032
previous_04253_evidence_closure_initial_main_push_job_id: 78019985036
previous_04253_evidence_closure_initial_main_push_head_sha: a7ecfdb950a07a27f5a527c5e890d4f183ce5c47
previous_04253_provider_latest_main_push_run_id: 26494136304
previous_04253_provider_latest_main_push_job_id: 78018181426
previous_04253_provider_latest_main_push_head_sha: 824540bea237011b73b00c53ff399675b8346c7f
previous_04250_latest_main_push_run_id: 26489610881
previous_04250_latest_main_push_job_id: 78004396577
previous_04250_latest_main_push_head_sha: baba155d6adfd4c9e2b2ba179d6727bb5035d1fc
previous_04249_latest_main_push_run_id: 26449795425
previous_04249_latest_main_push_job_id: 77866996627
previous_04249_latest_main_push_head_sha: d09ecfc425f6050a2c182cbcb3090ad2f9fa4827
previous_04248_latest_main_push_run_id: 26445409133
previous_04248_latest_main_push_job_id: 77850326001
previous_04248_latest_main_push_head_sha: ea1e7b85757f35feb10811dda4bbc38d94b304ac
previous_04245_latest_main_push_run_id: 26413569064
previous_04245_latest_main_push_job_id: 77753058728
previous_04245_latest_main_push_head_sha: 4f1f0bd8f7ffe9488dbb7175f65013870cf8d58f
previous_pr169_latest_main_push_run_id: 26288103559
previous_pr169_latest_main_push_job_id: 77380766318
previous_pr169_latest_main_push_head_sha: 11b123311d718cf77e87ccc7b8dea7c5728dc463
previous_pr168_latest_main_push_run_id: 26233838385
previous_pr168_latest_main_push_job_id: 77201340972
previous_pr168_latest_main_push_head_sha: 2f41da1073df6e65113ae8ddaeb183e9b55874f4
previous_pr167_latest_main_push_run_id: 26228675428
previous_pr167_latest_main_push_job_id: 77182631331
previous_pr167_latest_main_push_head_sha: f173f9857089de61ca1fb2b7a2da7839a3dd73a8
previous_pr164_latest_main_push_run_id: 26170972989
previous_pr164_latest_main_push_job_id: 76988240617
previous_pr164_latest_main_push_head_sha: 03402f1607b735f2d92291ae6109d7986d9a57b8
previous_pr163_latest_main_push_run_id: 26164349961
previous_pr163_latest_main_push_job_id: 76964254604
previous_pr163_latest_main_push_head_sha: 465e7b8ef79a1c05913107fa1364850e8dd387e9
previous_pr162_latest_main_push_run_id: 26156660639
previous_pr162_latest_main_push_job_id: 76937705571
previous_pr162_latest_main_push_head_sha: 39087469b2ed1752927cbf5a24c7410d5f96f22b
previous_pr160_latest_main_push_run_id: 26101838192
previous_pr160_latest_main_push_job_id: 76754696421
previous_pr160_latest_main_push_head_sha: 51a21d7c8612f598b85eeb58818ad3d61136c320
previous_pr156_latest_main_push_run_id: 26017721669
previous_pr156_latest_main_push_job_id: 76471545641
previous_pr156_latest_main_push_head_sha: a4509c552c003ee0fc87b54b26529686e6dfeb84
previous_pr155_latest_main_push_run_id: 26013384587
previous_pr155_latest_main_push_job_id: 76458402221
previous_pr155_latest_main_push_head_sha: 2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f
previous_pr154_latest_main_push_run_id: 25989986761
previous_pr154_latest_main_push_job_id: 76394250912
previous_pr154_latest_main_push_head_sha: d7f611dfc14a9fa1507f936559209513272b585a
previous_pr153_latest_main_push_run_id: 25987705546
previous_pr153_latest_main_push_job_id: 76388078056
previous_pr153_latest_main_push_head_sha: d306712ad671c8a00d5c560765b8952e24a07502
previous_pr152_latest_main_push_run_id: 25985786230
previous_pr152_latest_main_push_job_id: 76382711230
previous_pr152_latest_main_push_head_sha: ca07514097f4e9524a7f3630d321c9666593c962
previous_pr151_latest_main_push_run_id: 25984814303
previous_pr151_latest_main_push_job_id: 76380096421
previous_pr151_latest_main_push_head_sha: 26ae50fa7bef11b4919b441e706bde505463aded
historical_pr150_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass.md
historical_pr150_main_push_run_id: 25983307305
historical_pr150_main_push_job_id: 76375957834
historical_pr150_main_push_head_sha: 6d4b5d95742044bdbd8def933fbc8cdefbba71b3
historical_pr150_previous_main_push_run_id: 25983307305
historical_pr150_previous_main_push_job_id: 76375957834
historical_pr150_previous_main_push_head_sha: 6d4b5d95742044bdbd8def933fbc8cdefbba71b3
historical_pr149_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr149-postmerge-pass.md
historical_pr149_main_push_run_id: 25974335803
historical_pr149_main_push_job_id: 76351743536
historical_pr149_main_push_head_sha: dd895306c4b08802d262b4afb890382dd991a4d0
historical_pr145_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md
historical_pr145_main_push_run_id: 25961834812
historical_pr145_main_push_job_id: 76318357776
historical_pr145_main_push_head_sha: d6500c01c972cbc7ca1e290e51120181ceea1501
historical_pr144_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md
historical_pr144_main_push_run_id: 25959505688
historical_pr144_main_push_job_id: 76312299500
historical_pr144_main_push_head_sha: 4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1
historical_scope_lock_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04224-scope-lock-pass.md
historical_scope_lock_main_push_run_id: 25958514394
historical_scope_lock_main_push_job_id: 76309528498
historical_scope_lock_main_push_head_sha: ef903f114829eb0e1dc6e42bcd429685d1783d30
historical_successor_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md
historical_successor_main_push_run_id: 25938745434
historical_successor_main_push_job_id: 76250726268
historical_successor_main_push_head_sha: d0b12bd41e1104f68e5684aa797b8050286e6a69
historical_successor_main_push_version: 0.42.21-admin-smoke
historical_successor_main_push_package_pair: 0.42.20-admin-smoke -> 0.42.21-admin-smoke
historical_checkout_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md
historical_checkout_main_push_run_id: 25934411998
historical_checkout_main_push_job_id: 76236050409
historical_checkout_main_push_head_sha: 3933231e6e2abf3a398dfcc3fdc999b3df38dac6
historical_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md
historical_main_push_run_id: 25933861585
historical_main_push_job_id: 76234195716
historical_main_push_head_sha: 686e4201f823295dc65cde302f613a982ab8cade
checkout_action_version: actions/checkout@v6.0.2
branch_protection_ruleset_status: public-repository-enabled-exact-four-required-contexts
historical_private_plan_branch_protection_ruleset_status: unavailable-private-repo-plan
historical_private_plan_fallback_required_guard: public-boundary-ci-required
branch_protection_strict: true
branch_protection_enforce_admins: true
branch_protection_required_contexts: dotnet,web,delivery,installer-policy
historical_private_plan_ruleset_status: unavailable-private-repo-plan
historical_private_plan_fallback_guard: public-boundary-ci-required
package_build_decision: closed-0.42.54-admin-smoke-after-running-guest-cancel-policy
post_04245_public_boundary_guard_status: pass
post_04245_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass.md
post_04248_public_boundary_guard_status: pass
post_04248_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04248-manual-admin-postpush-pass.md
post_04249_public_boundary_guard_status: pass
post_04249_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04249-guest-execution-postpush-pass.md
post_04250_public_boundary_guard_status: pass
post_04250_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass.md
post_04253_public_boundary_guard_status: pass
post_04253_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-provider-postpush-pass.md
post_04253_evidence_closure_public_boundary_guard_status: pass
post_04253_evidence_closure_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md
post_04254_running_cancel_public_boundary_guard_status: pass
post_04254_running_cancel_public_boundary_guard_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04254-running-cancel-postpush-pass.md
post_pr167_product_payload_change_detected: true
post_pr167_admin_smoke_package_chain_decision: triggered-0.42.41-admin-smoke-for-installed-tui-row-projection-fix
post_pr167_manual_admin_package_pair_decision: closed-manual-admin-package-pair-04240-04241
post_pr168_product_payload_change_detected: false
post_pr168_admin_smoke_package_chain_decision: not-run-no-product-payload-change-current-0.42.41-admin-smoke
post_pr168_manual_admin_package_pair_decision: deferred-until-next-product-payload-change-after-pr168
post_pr169_product_payload_change_detected: false
post_pr169_admin_smoke_package_chain_decision: not-run-no-product-payload-change-current-0.42.41-admin-smoke
post_pr169_manual_admin_package_pair_decision: deferred-until-next-product-payload-change-after-pr169
post_pr169_next_product_payload_package_candidate: 0.42.42-admin-smoke
post_pr169_installed_account_novnc_smoke_decision: not-run-no-operator-surface-payload-change-after-pr169
post_pr164_product_payload_change_detected: false
post_pr164_admin_smoke_package_chain_decision: not-run-no-product-payload-change-next-candidate-0.42.40-admin-smoke
post_pr164_manual_admin_package_pair_decision: deferred-until-next-product-payload-change-after-pr164
post_pr164_installed_cli_targeted_smoke: docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md
post_pr164_web_tui_qos_guest_readback_decision: defer-direct-web-tui-control-no-product-payload-change
previous_package_build_decision: executed-0.42.29-admin-smoke-after-selector-package-chain-payload-change
previous_pr155_package_build_decision: deferred-no-product-payload-change-after-pr155
previous_pr154_package_build_decision: deferred-no-product-payload-change-after-pr154
post_pr162_product_payload_change_detected: true
post_pr162_admin_smoke_package_chain_decision: executed-0.42.38-admin-smoke-before-main-merge
post_pr162_manual_admin_package_pair_decision: candidate-partial-cleanhost-blocked
post_pr156_product_payload_change_detected: false
post_pr156_admin_smoke_package_chain_decision: not-run-no-product-payload-change-after-pr156
post_pr156_manual_admin_package_pair_decision: deferred-until-next-product-payload-change-after-pr156
post_pr155_product_payload_change_detected: false
post_pr155_admin_smoke_package_chain_decision: not-run-no-product-payload-change-after-pr155
post_pr155_manual_admin_package_pair_decision: deferred-until-next-product-payload-change-after-pr155
post_pr154_product_payload_change_detected: false
post_pr154_admin_smoke_package_chain_decision: not-run-no-product-payload-change-after-pr154
post_pr154_manual_admin_package_pair_decision: deferred-until-next-product-payload-change-after-pr154
historical_04224_internal_admin_smoke: 0.42.24-admin-smoke
historical_04224_04225_manual_admin_package_pair: 0.42.24-admin-smoke -> 0.42.25-admin-smoke
historical_04225_04226_manual_admin_package_pair: 0.42.25-admin-smoke -> 0.42.26-admin-smoke
historical_04226_04227_manual_admin_package_pair: 0.42.26-admin-smoke -> 0.42.27-admin-smoke
historical_04227_04228_manual_admin_package_pair: 0.42.27-admin-smoke -> 0.42.28-admin-smoke
historical_04228_04229_update_zip_sha256: 3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542
```

이 contract는 Desktop Node의 Public Boundary Pester/PowerShell workflow를 현재
`provider_required=false`인 residue로 분류한다. Provider가 실제로 요구하는 Required CI는
`.github/workflows/development-gates.yml`의 `dotnet`, `web`, `delivery`, `installer-policy`
네 context다. ADR-0005 public distribution expansion candidate는 `closed-not-adopted` 상태이고,
ADR-0006 `internal-private-network-only`가 현재 적용 배포 경계다.

0.42.49 후속은 Guest Execution policy/API preview disabled boundary package/fullgate/current-card,
manual-admin `0.42.48 -> 0.42.49` readiness blocker 기록이 main에 반영된 뒤의
public-boundary PASS다. 0.42.48, 0.42.45와 PR #169 public-boundary PASS는 predecessor로 보존한다. Public
trusted signing, trusted timestamp, winget public submission,
external stable publication/catalog upload, public stable installer URL, clean-host public
signed install/update/rollback smoke는 이 contract에서 계속 claim하지 않는다.

현재 provider-required guard:

- Main protection은 `strict=true`, admin enforcement enabled이고 GitHub Actions app ID
  `15368`의 exact contexts `dotnet`, `web`, `delivery`, `installer-policy`만 요구한다.
- Final-main SHA `6e2bdb93ce308b632c929e2c17f5550ac3845401`의 Development Gates run
  [`32904006595`](https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/32904006595)은
  네 job과 네 contract-v2 artifact를 PASS했다.
- `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`와
  `.github/workflows/public-boundary.yml`은 historical/local compatibility 및 non-required residue로
  남는다. 실제 residue run `32904006619`, job `97983888524`는 PASS했지만 보호 context가 아니다.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`와
  `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`는 이 provider-required/residue 분리를
  참조해야 한다.

## Provider protection current와 historical private-plan fallback

현재 공개 저장소는 branch protection API를 사용할 수 있고 exact four Required CI를 강제한다.
과거 private archive 플랜에서 branch protection/ruleset API는
`Upgrade to GitHub Pro or make this repository public to enable this feature` 403을
반환했다. 당시 PR 생성/check 확인/merge 직후 `main` push의 `public-boundary-ci-required`
PASS 확인을 fallback으로 사용한 기록은 historical snapshot이며 현재 provider authority가 아니다.

현재 Required CI evidence는
`docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md`, final-main run
`32904006595`, head `6e2bdb93ce308b632c929e2c17f5550ac3845401`이다. Public Boundary residue는 run
`32904006619`, job `97983888524`, 같은 head에서 PASS했으며 `provider_required=false`다.
이 docs/CI closure는 product payload를 바꾸지 않아 추가 package 후보를 열지 않는다.
Operational authority는 `0.42.74-admin-smoke`, closed pair는
`0.42.73-admin-smoke -> 0.42.74-admin-smoke`, promotion은 blocker 1건 때문에 `false`다.

아래 0.42.59 이하 값은 historical predecessor chain이다. 당시 직전 product payload
public-boundary main push는 run `26629340294`,
job `78473968530`, head `b1733c1d9777d2c0828897ae2751af33a270b2fe`이다. 그 이전 0.42.57 main push는 run `26587524245`, job `78337437665`, head
`96182b440b35c17183802ad323a123ff6e4b6730`이고, 그 이전 0.42.56 manual-admin closure main
push는 run `26578120570`, job `78303066840`, head
`7a7d5de822bdb058b04149eeeef0a7eb462828b5`이다. 0.42.54 fullgate evidence roll-forward main
push는 run `26558089193`, job `78234262641`, head
`958052181012f7d1be6ccff535316bfaeeef07df`이고, 그 이전 running cancel evidence
roll-forward main push는 run `26556328902`, job `78228845568`, head
`2c11e359709c775be7a57ea9624716720c5b62d6`이고, 그 이전 running cancel code-level main push는
run `26526151668`, job `78130197561`, head `5a1058f55fcd42d28c7075514e1924c5ccdfb525`이다.
그 이전 0.42.53 evidence closure main push는
run `26518952796`, job `78104102372`, head `12bc72e856ea9ac7c6d54c4094873b2d8db9f672`이다. 그 이전 0.42.53 credentialed Windows guest execution
smoke main push run은 `26516950720`, job `78096741408`, head
`9c9a2d16ce5e0dd7d18df8c5e497eb89b343acc4`, 그 이전 0.42.53 ISO evidence roll-forward main push
run `26512890221`, job `78081757583`, head `5985d547f87f91fceed067da4a0803a6096d8c29`,
그 이전 0.42.53 evidence closure main push
run `26511891436`, job `78078338831`, head `153edc0c1977d1d39249846dbaeff421810c44e8`,
이전 0.42.53 evidence closure roll-forward main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-evidence-closure-postpush-pass.md`
run `26510159990`, job `78072405936`, 그 이전 roll-forward run `26496046109`,
evidence gates roll-forward run `26495580805`,
최초 0.42.53 evidence closure run `26494683032`,
이전 0.42.53 provider main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04253-guest-execution-provider-postpush-pass.md`
run `26494136304`, job `78018181426`, 이전 0.42.50 main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass.md`
run `26489610881`, job `78004396577`, 이전 0.42.49 main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04249-guest-execution-postpush-pass.md`
run `26449795425`, job `77866996627`, 이전 0.42.48 main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04248-manual-admin-postpush-pass.md`
run `26445409133`, job `77850326001`, 이전 0.42.45 main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass.md`
run `26413569064`, job `77753058728`, 이전 PR #169 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md`
run `26288103559`, job `77380766318`, 이전 PR #168 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr168-postmerge-pass.md`
run `26233838385`, job `77201340972`, 이전 PR #167 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md`
run `26228675428`, job `77182631331`, 이전 PR #164 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md`
run `26170972989`, job `76988240617`, 이전 PR #163 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr163-postmerge-pass.md`
run `26164349961`, job `76964254604`, 이전 PR #162 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr162-postmerge-pass.md`
run `26156660639`, job `76937705571`, 이전 PR #160 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-19-pr160-postmerge-pass.md`
run `26101838192`, job `76754696421`, 이전 PR #156 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md`
run `26017721669`, job `76471545641`, 이전 PR #155 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md`
run `26013384587`, job `76458402221`, 이전 PR #154 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md`
run `25989986761`, job `76394250912`, 이전 PR #153 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md`
run `25987705546`, job `76388078056`, 이전 PR #152 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md`
run `25985786230`, job `76382711230`, 이전 PR #151 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md`
run `25984814303`, job `76380096421`, 이전 PR #150 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass.md`
run `25983307305`, 이전 PR #149 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr149-postmerge-pass.md`
run `25974335803`, 이전 PR #145 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md`
run `25961834812`, 이전 PR #144 post-merge main push
evidence `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md`
run `25959505688`, job `76312299500`, 이전 scope-lock main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04224-scope-lock-pass.md`
run `25958514394`, 이전 post-merge main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md`
run `25954744127`, job `76299282407`, predecessor main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass.md`
run `25952150476`, successor main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md`
run `25938745434`, 이전 04221 main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-pass.md`
run `25935332346`, checkout v6.0.2 maintenance main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md`
run `25934411998`와 04220 main push evidence
`docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md` run
`25933861585`는 historical CI maintenance anchor로 보존한다. 이 evidence는 public
trusted signing 또는 external stable publication evidence가 아니다.

## CI Maintenance

GitHub Actions Node.js 20 deprecation warning을 줄이기 위해 checkout step은 공식
`actions/checkout` latest release `v6.0.2` 기준 `actions/checkout@v6.0.2`로 pin한다.
이 maintenance는 product payload 변경이 아니므로 next product payload package build는
04220 기준에서는 `deferred-no-product-payload-change-after-04220`로 보존하고,
이후 Credential Manager idempotence closure payload가 `0.42.23-admin-smoke`로
빌드되고 `0.42.23` full admin host mutation까지 PASS하면서
`package_build_decision=executed-0.42.23-admin-smoke`로 승격됐다.
