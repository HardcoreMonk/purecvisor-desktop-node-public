# Public boundary CI main push 2026-05-20 PR #164

evidence_id: `public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass`
result: `PASS`
scope: `post-merge-main-push-public-boundary-ci`
pr: `164`
pr_url: `[private-archive-repository]/pull/164`
head_sha: `03402f1607b735f2d92291ae6109d7986d9a57b8`
merge_commit: `03402f1607b735f2d92291ae6109d7986d9a57b8`
run_id: `26170972989`
job_id: `76988240617`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-20T14:59:40Z`
started_at: `2026-05-20T14:59:45Z`
completed_at: `2026-05-20T15:00:07Z`
completed_at_kst: `2026-05-21T00:00:07+09:00`
conclusion: `success`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

PR #164 merge 후 `main` push에서 public-boundary CI가 성공했다. 이 evidence는 public
release claim이 아니라 public-boundary guard가 통과했음을 기록하는 문서다.

## 확인

| 항목 | 값 |
| --- | --- |
| PR | `#164`, merged `2026-05-20T14:59:40Z` |
| branch | `codex/04239-pcvcli-linux-parity-qos-guest` -> `main` |
| merge commit | `03402f1607b735f2d92291ae6109d7986d9a57b8` |
| run | `26170972989`, event `push`, conclusion `success` |
| job | `76988240617`, `public-boundary-ci-required`, conclusion `success` |
| guard steps | checkout, Pester install, public boundary evidence guard, packaging regression required step |
| run URL | `[private-archive-repository]/actions/runs/26170972989` |
| job URL | `[private-archive-repository]/actions/runs/26170972989/job/76988240617` |

## 경계

이 evidence는 public-boundary CI PASS만 주장한다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
