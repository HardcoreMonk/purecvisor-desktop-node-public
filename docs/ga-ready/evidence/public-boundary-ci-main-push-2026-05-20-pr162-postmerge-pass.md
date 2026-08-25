# Public boundary CI main push 2026-05-20 PR #162

evidence_id: `public-boundary-ci-main-push-2026-05-20-pr162-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `162`
pr_url: `[private-archive-repository]/pull/162`
head_sha: `39087469b2ed1752927cbf5a24c7410d5f96f22b`
merge_commit: `39087469b2ed1752927cbf5a24c7410d5f96f22b`
run_id: `26156660639`
job_id: `76937705571`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-20T10:27:11Z`
started_at: `2026-05-20T10:27:15Z`
completed_at: `2026-05-20T10:27:37Z`
conclusion: `success`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #162 `add vm media and resource mutation parity` merge 후 `main` push에서
public-boundary CI가 성공했다. 이 evidence는 public release claim이 아니라
public-boundary guard가 통과했음을 기록하는 문서다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#162`, merged `2026-05-20T10:27:11Z` |
| branch | `codex/vm-media-resource-mutation-parity` -> `main` |
| merge commit | `39087469b2ed1752927cbf5a24c7410d5f96f22b` |
| run | `26156660639`, event `push`, conclusion `success` |
| job | `76937705571`, `public-boundary-ci-required`, conclusion `success` |
| guard steps | checkout, Pester install, public boundary evidence guard, packaging regression required step |
| run URL | `[private-archive-repository]/actions/runs/26156660639` |
| job URL | `[private-archive-repository]/actions/runs/26156660639/job/76937705571` |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
