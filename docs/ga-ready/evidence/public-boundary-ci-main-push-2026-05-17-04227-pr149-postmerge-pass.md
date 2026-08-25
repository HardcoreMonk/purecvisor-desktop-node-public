# Public Boundary CI Main Push 2026-05-17 PR #149 Post-merge PASS

```text
evidence_id: public-boundary-ci-main-push-2026-05-17-04227-pr149-postmerge-pass
result: PASS
scope: public-boundary-ci-required-main-push
workflow: Public Boundary Contract
run_id: 25974335803
job_id: 76351743536
head_sha: dd895306c4b08802d262b4afb890382dd991a4d0
merge_commit: dd895306c4b08802d262b4afb890382dd991a4d0
completed_at: 2026-05-16T22:14:35Z
completed_at_kst: 2026-05-17T07:14:35+09:00
source_pr: [private-archive-repository]/pull/149
public_boundary_guard_executed: true
checkout_action_version: actions/checkout@v6.0.2
branch_protection_ruleset_status: unavailable-private-repo-plan
fallback_required_guard: public-boundary-ci-required
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

PR #149 merge commit `dd895306c4b08802d262b4afb890382dd991a4d0` 기준 `main` push에서
`public-boundary-ci-required` guard가 PASS했다. 이 guard는 ADR-0006
`internal-private-network-only` 경계를 확인하는 문서/Pester contract이며 public trusted
signing, public timestamp, winget public submission, external stable publication/catalog
upload, public stable installer URL, clean-host public signed install/update/rollback smoke를
claim하지 않는다.

| 항목 | 값 |
| --- | --- |
| workflow run | `[private-archive-repository]/actions/runs/25974335803` |
| job | `[private-archive-repository]/actions/runs/25974335803/job/76351743536` |
| source PR | `[private-archive-repository]/pull/149` |
| previous PR #145 evidence | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-pr145-postmerge-pass.md` |
| previous run/job | `25961834812` / `76318357776` |

## 확인된 Step

| step | conclusion |
| --- | --- |
| Checkout | `success` |
| Install Pester | `success` |
| Run public boundary evidence guard | `success` |
| Verify packaging regression required step | `success` |

이 evidence는 CI boundary proof일 뿐 public distribution evidence가 아니다.
