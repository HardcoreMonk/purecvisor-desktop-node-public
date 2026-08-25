# Public Boundary CI Main Push 2026-05-19 PR #158 Post-merge PASS

evidence_id: public-boundary-ci-main-push-2026-05-19-pr158-postmerge-pass
result: PASS
scope: public-boundary-ci-required-main-push
workflow: Public Boundary Contract
workflow_file: .github/workflows/public-boundary.yml
run_id: 26094982269
job_id: 76730240480
head_sha: 63df3e42a4e42b4e21646e356968399c1458d89b
merge_commit: 63df3e42a4e42b4e21646e356968399c1458d89b
source_pr: [private-archive-repository]/pull/158
source_pr_title: "[codex] pcvcli parity and 0.42.34 evidence"
merged_at_utc: 2026-05-19T11:44:12Z
run_created_at_utc: 2026-05-19T11:44:16Z
run_completed_at_utc: 2026-05-19T11:44:43Z
fallback_required_guard: public-boundary-ci-required
checkout_action_version: actions/checkout@v6.0.2
source_version_anchor: 0.42.34-admin-smoke
postmerge_package_anchor: 0.42.34-admin-smoke
product_payload_change_detected: true
package_build_decision: closed-by-0.42.34-admin-smoke-before-pr158-merge
admin_smoke_package_chain_decision: closed-full-admin-host-mutation-gate-20260519-04234
manual_admin_package_pair_decision: closed-manual-admin-campaign-descriptor-20260519-04232-04234
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

PR #158 merge commit `63df3e42a4e42b4e21646e356968399c1458d89b` 기준 `main`
push에서 `public-boundary-ci-required` guard가 PASS했다. PR #158은 PCVCLI Linux parity
구현과 `0.42.34-admin-smoke` package/fullgate/package-pair/current-card evidence를
main에 반영했다.

`0.42.34-admin-smoke` package chain은 PR merge 전에 이미
`full-admin-host-mutation-gate-20260519-04234`와
`manual-admin-campaign-descriptor-20260519-04232-04234-closed`로 닫혔으므로, 이 CI
evidence 자체는 추가 host mutation 또는 public distribution evidence를 주장하지
않는다.

## Run

| 항목 | 값 |
| --- | --- |
| workflow run | `[private-archive-repository]/actions/runs/26094982269` |
| job | `[private-archive-repository]/actions/runs/26094982269/job/76730240480` |
| source PR | `[private-archive-repository]/pull/158` |
| previous PR #156 evidence | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md` |
| result | `success` |

## Checked Steps

| Step | 결과 |
| --- | --- |
| Set up job | `success` |
| Checkout | `success` |
| Install Pester | `success` |
| Run public boundary evidence guard | `success` |
| Verify packaging regression required step | `success` |
| Post Checkout | `success` |
| Complete job | `success` |

이 evidence는 CI boundary proof일 뿐 public trusted signing 또는 external stable
publication evidence가 아니다.
