# Public boundary CI main push 2026-05-20 PR #163

evidence_id: `public-boundary-ci-main-push-2026-05-20-pr163-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `163`
pr_url: `[private-archive-repository]/pull/163`
head_sha: `465e7b8ef79a1c05913107fa1364850e8dd387e9`
merge_commit: `465e7b8ef79a1c05913107fa1364850e8dd387e9`
run_id: `26164349961`
job_id: `76964254604`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-20T13:03:40Z`
started_at: `2026-05-20T13:03:45Z`
completed_at: `2026-05-20T13:04:07Z`
conclusion: `success`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #163 merge 후 `main` push에서 public-boundary CI가 성공했다. 이 evidence는 public
release claim이 아니라 public-boundary guard가 통과했음을 기록하는 문서다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#163`, merged `2026-05-20T13:03:40Z` |
| branch | `codex/04238-fullgate-manual-admin-closure` -> `main` |
| merge commit | `465e7b8ef79a1c05913107fa1364850e8dd387e9` |
| run | `26164349961`, event `push`, conclusion `success` |
| job | `76964254604`, `public-boundary-ci-required`, conclusion `success` |
| guard steps | checkout, Pester install, public boundary evidence guard, packaging regression required step |
| run URL | `[private-archive-repository]/actions/runs/26164349961` |
| job URL | `[private-archive-repository]/actions/runs/26164349961/job/76964254604` |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
