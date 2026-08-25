# PR #169 main push public-boundary CI 증거

evidence_id: `public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `169`
pr_url: `[private-archive-repository]/pull/169`
source_pr_title: `docs: close 04241 tui row projection package chain`
head_branch: `codex/04241-tui-row-projection-package-chain`
base_branch: `main`
head_sha: `11b123311d718cf77e87ccc7b8dea7c5728dc463`
merge_commit: `11b123311d718cf77e87ccc7b8dea7c5728dc463`
run_id: `26288103559`
job_id: `77380766318`
workflow: `Public Boundary Contract`
workflow_file: `.github/workflows/public-boundary.yml`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-22T12:35:41Z`
started_at: `2026-05-22T12:35:45Z`
completed_at: `2026-05-22T12:36:08Z`
completed_at_kst: `2026-05-22T21:36:08+09:00`
conclusion: `success`
fallback_required_guard: `public-boundary-ci-required`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #169 merge 후 `main` push에서 public-boundary CI가 성공했다. 이 evidence는
`0.42.41-admin-smoke` package chain closure, full admin host mutation gate,
manual-admin package-pair closure, 설치본 current-card, 실제 VM TUI row projection evidence가
main에 반영된 뒤 required guard가 통과했음을 기록한다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#169`, merged `2026-05-22T12:35:38Z` |
| branch | `codex/04241-tui-row-projection-package-chain` -> `main` |
| merge commit | `11b123311d718cf77e87ccc7b8dea7c5728dc463` |
| run | `26288103559`, event `push`, conclusion `success` |
| job | `77380766318`, `public-boundary-ci-required`, conclusion `success` |
| run URL | `[private-archive-repository]/actions/runs/26288103559` |
| job URL | `[private-archive-repository]/actions/runs/26288103559/job/77380766318` |
| previous PR #168 evidence | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr168-postmerge-pass.md` |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
