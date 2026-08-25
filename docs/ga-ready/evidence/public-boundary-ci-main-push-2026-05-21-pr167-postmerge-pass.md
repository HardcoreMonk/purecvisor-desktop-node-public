# Public boundary CI main push 2026-05-21 PR #167

evidence_id: `public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `167`
pr_url: `[private-archive-repository]/pull/167`
source_pr_title: `docs: close 0.42.40 admin smoke evidence`
head_branch: `codex/04240-package-host-mutation-campaign`
base_branch: `main`
head_sha: `f173f9857089de61ca1fb2b7a2da7839a3dd73a8`
merge_commit: `f173f9857089de61ca1fb2b7a2da7839a3dd73a8`
run_id: `26228675428`
job_id: `77182631331`
workflow: `Public Boundary Contract`
workflow_file: `.github/workflows/public-boundary.yml`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-21T13:22:37Z`
started_at: `2026-05-21T13:22:41Z`
completed_at: `2026-05-21T13:23:02Z`
completed_at_kst: `2026-05-21T22:23:02+09:00`
conclusion: `success`
fallback_required_guard: `public-boundary-ci-required`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #167 merge 후 `main` push에서 public-boundary CI가 성공했다. 이 evidence는
`0.42.40-admin-smoke` package/full admin host mutation/manual-admin package-pair closure가
main에 반영된 뒤 required guard가 통과했음을 기록한다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#167`, merged `2026-05-21T13:22:32Z` |
| branch | `codex/04240-package-host-mutation-campaign` -> `main` |
| merge commit | `f173f9857089de61ca1fb2b7a2da7839a3dd73a8` |
| run | `26228675428`, event `push`, conclusion `success` |
| job | `77182631331`, `public-boundary-ci-required`, conclusion `success` |
| run URL | `[private-archive-repository]/actions/runs/26228675428` |
| job URL | `[private-archive-repository]/actions/runs/26228675428/job/77182631331` |
| previous PR #164 evidence | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md` |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
