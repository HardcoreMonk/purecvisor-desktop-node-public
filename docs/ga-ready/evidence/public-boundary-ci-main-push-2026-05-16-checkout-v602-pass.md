# Public Boundary CI Main Push Checkout v6.0.2 PASS Evidence

evidence_id: public-boundary-ci-main-push-2026-05-16-checkout-v602-pass
result: PASS
actual_execution: github-actions-main-push
workflow: public-boundary.yml
run_id: 25934411998
run_url: [private-archive-repository]/actions/runs/25934411998
job_name: public-boundary-ci-required
job_id: 76236050409
head_branch: main
head_sha: 3933231e6e2abf3a398dfcc3fdc999b3df38dac6
source_pr: [private-archive-repository]/pull/135
source_version_anchor: 0.42.20-admin-smoke
previous_main_push_evidence: docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md
previous_main_push_run_id: 25933861585
public_boundary_guard_executed: true
checkout_action_version: actions/checkout@v6.0.2
node20_deprecation_warning_observed: false
branch_protection_ruleset_status: unavailable-private-repo-plan
fallback_required_guard: public-boundary-ci-required
package_build_decision: deferred-no-product-payload-change-after-04220
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

이 evidence는 PR #135 merge 이후 `main` push에서 `actions/checkout@v6.0.2`가
실제로 실행되고 `public-boundary-ci-required`가 PASS했음을 기록한다. 이전 main push
run `25933861585`는 Node.js 20 deprecation warning을 남겼지만, 이 run 로그에서는
`actions/checkout@v6.0.2` 실행만 확인했고 Node.js 20 deprecation warning 문자열은
관찰되지 않았다.

## 실행 결과

| 항목 | 값 |
| --- | --- |
| run id | `25934411998` |
| run URL | `[private-archive-repository]/actions/runs/25934411998` |
| ref | `main` |
| head SHA | `3933231e6e2abf3a398dfcc3fdc999b3df38dac6` |
| job | `public-boundary-ci-required` |
| job id | `76236050409` |
| checkout action | `actions/checkout@v6.0.2` |
| conclusion | `success` |
| public boundary guard executed | `true` |

PASS step:

- `Checkout`
- `Install Pester`
- `Run public boundary evidence guard`
- `Verify packaging regression required step`

## 경계

이 evidence는 CI guard maintenance 결과만 기록한다. Host mutation, clean-host VM,
MSI install/update/rollback, firewall, Event Log, trust store, Credential Manager,
public trusted signing, trusted timestamp, external stable publication, winget
submission, public stable installer URL은 실행하거나 주장하지 않는다.
