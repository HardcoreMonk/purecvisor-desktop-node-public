# 0.42.48 manual-admin main push public-boundary CI 증거

evidence_id: `public-boundary-ci-main-push-2026-05-26-04248-manual-admin-postpush-pass`
result: `PASS`
scope: `post-04248-manual-admin-main-push-public-boundary-ci`
source_version_anchor: `0.42.48-admin-smoke`
source_commit_title: `docs: close 0.42.48 manual admin evidence`
base_branch: `main`
head_sha: `ea1e7b85757f35feb10811dda4bbc38d94b304ac`
run_id: `26445409133`
job_id: `77850326001`
workflow: `Public Boundary Contract`
workflow_file: `.github/workflows/public-boundary.yml`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-26T09:56:13Z`
started_at: `2026-05-26T09:56:17Z`
completed_at: `2026-05-26T09:56:41Z`
completed_at_kst: `2026-05-26T18:56:41+09:00`
conclusion: `success`
checkout_action_version: `actions/checkout@v6.0.2`
fallback_required_guard: `public-boundary-ci-required`
local_pester_command: `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed`
local_pester_result: `75 passed, 0 failed`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

`0.42.48-admin-smoke` Phase 3 Web/TUI QoS direct control package/fullgate/current-card와
`0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair closure를
main에 반영한 뒤 GitHub Actions `Public Boundary Contract`를 확인했다.

## 확인

| 항목 | 값 |
| --- | --- |
| commit | `ea1e7b85757f35feb10811dda4bbc38d94b304ac` |
| run | `26445409133`, event `push`, conclusion `success` |
| job | `77850326001`, `public-boundary-ci-required`, conclusion `success` |
| run URL | `[private-archive-repository]/actions/runs/26445409133` |
| job URL | `[private-archive-repository]/actions/runs/26445409133/job/77850326001` |
| predecessor | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass.md` |

## 경계

이 evidence는 public-boundary CI PASS와 문서 guard 정합성만 주장한다. Public trusted
signing, public stable installer URL, winget submission, 외부 stable publication,
clean-host public signed install/update/rollback smoke는 계속 ADR-0006 범위 밖이다.
