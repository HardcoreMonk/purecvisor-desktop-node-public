# Public Boundary CI Main Push 2026-05-17 PR #152 Post-merge PASS

```text
evidence_id: public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass
result: PASS
scope: public-boundary-ci-required-main-push
workflow: Public Boundary Contract
run_id: 25985786230
job_id: 76382711230
head_sha: ca07514097f4e9524a7f3630d321c9666593c962
merge_commit: ca07514097f4e9524a7f3630d321c9666593c962
completed_at: 2026-05-17T08:24:39Z
completed_at_kst: 2026-05-17T17:24:39+09:00
source_pr: [private-archive-repository]/pull/152
public_boundary_guard_executed: true
checkout_action_version: actions/checkout@v6.0.2
branch_protection_ruleset_status: unavailable-private-repo-plan
fallback_required_guard: public-boundary-ci-required
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

PR #152 merge commit `ca07514097f4e9524a7f3630d321c9666593c962` 기준 `main`
push에서 `public-boundary-ci-required` guard가 PASS했다. 이 guard는 ADR-0006
`internal-private-network-only` 경계를 확인하는 문서/Pester contract이며 public trusted
signing, trusted timestamp, winget public submission, external stable publication/catalog
upload, public stable installer URL, clean-host public signed install/update/rollback smoke를
claim하지 않는다.

| 항목 | 값 |
| --- | --- |
| workflow run | `[private-archive-repository]/actions/runs/25985786230` |
| job | `[private-archive-repository]/actions/runs/25985786230/job/76382711230` |
| source PR | `[private-archive-repository]/pull/152` |
| previous PR #151 evidence | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md` |
| previous run/job | `25984814303` / `76380096421` |

## 확인된 Step

| step | conclusion |
| --- | --- |
| Checkout | `success` |
| Install Pester | `success` |
| Run public boundary evidence guard | `success` |
| Verify packaging regression required step | `success` |

이 evidence는 CI boundary proof일 뿐 public distribution evidence가 아니다. PR #152는
`0.42.28-admin-smoke` Operator Surface current-card와 Host Ops diagnostics bucket table을
문서화한 PR이며, post-merge main push PASS는 internal admin-smoke chain의 public boundary
guard가 깨지지 않았음을 기록한다.
