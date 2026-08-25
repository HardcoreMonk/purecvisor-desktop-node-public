# Public boundary CI main push 2026-05-19 PR #160

evidence_id: `public-boundary-ci-main-push-2026-05-19-pr160-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `160`
pr_url: `[private-archive-repository]/pull/160`
head_sha: `51a21d7c8612f598b85eeb58818ad3d61136c320`
merge_commit: `51a21d7c8612f598b85eeb58818ad3d61136c320`
run_id: `26101838192`
job_id: `76754696421`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-19T13:54:25Z`
completed_at: `2026-05-19T13:54:50Z`
conclusion: `success`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #160 `add pcvcli vm stats and lifecycle backend routes` merge 후 `main` push에서
public-boundary CI가 성공했다. 이 evidence는 public release claim이 아니라
public-boundary guard가 통과했음을 기록하는 문서다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#160`, merged `2026-05-19T13:54:21Z` |
| branch | `codex/vm-stats-lifecycle-backend-slice` -> `main` |
| merge commit | `51a21d7c8612f598b85eeb58818ad3d61136c320` |
| run | `26101838192`, event `push`, conclusion `success` |
| job | `76754696421`, `public-boundary-ci-required`, conclusion `success` |
| guard steps | checkout, Pester install, public boundary evidence guard, packaging regression required step |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
