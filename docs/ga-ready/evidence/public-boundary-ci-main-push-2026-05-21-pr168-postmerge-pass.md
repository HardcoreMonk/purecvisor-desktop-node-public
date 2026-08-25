# Public boundary CI main push 2026-05-21 PR #168

evidence_id: `public-boundary-ci-main-push-2026-05-21-pr168-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `168`
pr_url: `[private-archive-repository]/pull/168`
source_pr_title: `docs: record 04240 current-card and TUI row trigger`
head_branch: `codex/04240-current-card-ux-evidence`
base_branch: `main`
head_sha: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
merge_commit: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
run_id: `26233838385`
job_id: `77201340972`
workflow: `Public Boundary Contract`
workflow_file: `.github/workflows/public-boundary.yml`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-21T14:54:32Z`
started_at: `2026-05-21T14:54:37Z`
completed_at: `2026-05-21T14:55:02Z`
completed_at_kst: `2026-05-21T23:55:02+09:00`
conclusion: `success`
fallback_required_guard: `public-boundary-ci-required`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #168 merge 후 `main` push에서 public-boundary CI가 성공했다. 이 evidence는
`0.42.40-admin-smoke` current-card와 설치본 TUI row projection blocker trigger 문서가
main에 반영된 뒤 required guard가 통과했음을 기록한다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#168`, merged `2026-05-21T14:54:25Z` |
| branch | `codex/04240-current-card-ux-evidence` -> `main` |
| merge commit | `2f41da1073df6e65113ae8ddaeb183e9b55874f4` |
| run | `26233838385`, event `push`, conclusion `success` |
| job | `77201340972`, `public-boundary-ci-required`, conclusion `success` |
| run URL | `[private-archive-repository]/actions/runs/26233838385` |
| job URL | `[private-archive-repository]/actions/runs/26233838385/job/77201340972` |
| previous PR #167 evidence | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md` |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
