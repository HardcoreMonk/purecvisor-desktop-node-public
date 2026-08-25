# Public boundary CI main push 2026-05-29 0.42.59 admin-smoke closure

evidence_id: `public-boundary-ci-main-push-2026-05-29-04259-admin-smoke-closure-postpush-pass`
result: `PASS`
scope: `post-04259-admin-smoke-closure-main-push`
workflow: `Public Boundary Contract`
run_id: `26629340294`
job_id: `78473968530`
head_sha: `b1733c1d9777d2c0828897ae2751af33a270b2fe`
head_commit_title: `Document 0.42.59 admin smoke closure`
run_url: `[private-archive-repository]/actions/runs/26629340294`
job_url: `[private-archive-repository]/actions/runs/26629340294/job/78473968530`
host_mutation_performed: `false`
current_evidence_payload_candidate: `true`
next_product_payload_package_candidate: `0.42.60-admin-smoke`
installed_account_novnc_rerun_decision: `not-run-no-account-novnc-payload-change-after-04258`
actual_vm_guest_execution_qos_smoke_decision: `not-run-no-guest-execution-or-qos-provider-payload-change-after-04259`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 검증 항목

| step | 결과 |
| --- | --- |
| Checkout repository | `success` |
| Install Pester | `success` |
| Public boundary evidence guard | `success` |
| Verify packaging regression required step | `success` |

## 경계

이 run은 `0.42.59-admin-smoke` package/fullgate/manual-admin/current-card closure evidence가
`main`에 push된 뒤 public-boundary contract가 통과했음을 기록한다. Public trusted signing,
winget public submission, public stable installer URL, external stable publication은 계속
ADR-0006 out-of-scope이며 이 evidence가 주장하지 않는다.

## 후속 제품 payload 판단

Runtime/API current evidence rollup은 public-boundary main-push evidence를
`current_evidence.public_boundary.latest_main_push`로 노출하고, CLI/TUI/Web current-card도 이 값을
읽는다. 따라서 이 evidence는 다음 installed current-card payload 후보를 여는 작은 제품화 변경으로
분류하고, 후속 package 후보를 `0.42.60-admin-smoke`로 둔다.

account/noVNC payload는 `0.42.58-admin-smoke` 이후 변경되지 않았으므로 최신 PASS
`docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md`를
carry-forward한다. Guest Execution provider/direct-control과 Hyper-V QoS provider/control payload도
`0.42.59-admin-smoke` 이후 변경하지 않았으므로 actual VM Guest Execution/QoS smoke는 이번
public-boundary evidence rollup만으로 재실행하지 않는다.
