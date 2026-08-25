# Public Boundary CI Main Push PASS Evidence

evidence_id: public-boundary-ci-main-push-2026-05-16-04220-pass
result: PASS
actual_execution: github-actions-main-push
workflow: public-boundary.yml
run_id: 25933861585
run_url: [private-archive-repository]/actions/runs/25933861585
job_name: public-boundary-ci-required
job_id: 76234195716
head_branch: main
head_sha: 686e4201f823295dc65cde302f613a982ab8cade
source_pr: [private-archive-repository]/pull/134
source_version_anchor: 0.42.20-admin-smoke
source_full_admin_gate: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md
source_full_admin_gate_batch: full-admin-host-mutation-gate-20260516-04220
source_post_04220_dev_slices: docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md
source_public_boundary_rerun: docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md
public_boundary_guard_executed: true
checkout_action_version: actions/checkout@v4
checkout_maintenance_target: actions/checkout@v6.0.2
node20_deprecation_warning_observed: true
branch_protection_ruleset_status: unavailable-private-repo-plan
fallback_required_guard: public-boundary-ci-required
package_build_decision: deferred-no-product-payload-change-after-04220
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

이 evidence는 PR #134 merge 이후 `main` push에서 `public-boundary-ci-required`가
PASS한 사실을 기록한다. 결제 수단 등록 이후 GitHub-hosted runner가 정상 시작했고,
`Run public boundary evidence guard`와 `Verify packaging regression required step`이
모두 PASS했다.

## 실행 결과

| 항목 | 값 |
| --- | --- |
| run id | `25933861585` |
| run URL | `[private-archive-repository]/actions/runs/25933861585` |
| ref | `main` |
| head SHA | `686e4201f823295dc65cde302f613a982ab8cade` |
| job | `public-boundary-ci-required` |
| job id | `76234195716` |
| conclusion | `success` |
| public boundary guard executed | `true` |

## CI Maintenance 결정

이 run 자체는 기존 `.github/workflows/public-boundary.yml`의
`actions/checkout@v4`로 실행되어 Node.js 20 deprecation warning을 남겼다. 후속
maintenance slice는 공식 `actions/checkout` latest release `v6.0.2`에 맞춰
workflow checkout step을 `actions/checkout@v6.0.2`로 pin한다.

Branch protection/ruleset API는 private repo 현재 플랜에서
`Upgrade to GitHub Pro or make this repository public to enable this feature` 403을
반환한다. 따라서 현재 대체 guard는 repository ruleset이 아니라 PR/merge 운영에서
`public-boundary-ci-required` PASS를 확인하는 방식이다.

## Package-build 결정

이 slice는 CI workflow와 문서/evidence 계약만 바꾼다. `src`, product wrapper,
installer payload, web runtime asset 변경이 없으므로 `0.42.20-admin-smoke -> next`
product payload package build는 `deferred-no-product-payload-change-after-04220`로
보류한다.

## 경계

Host mutation, clean-host VM, MSI install/update/rollback, firewall, Event Log,
trust store, Credential Manager, public trusted signing, trusted timestamp, external
stable publication, winget submission, public stable installer URL은 실행하거나
주장하지 않는다.
