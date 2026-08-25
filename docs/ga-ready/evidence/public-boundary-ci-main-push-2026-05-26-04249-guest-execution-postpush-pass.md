# 0.42.49 Guest Execution main push public-boundary CI 증거

evidence_id: `public-boundary-ci-main-push-2026-05-26-04249-guest-execution-postpush-pass`
result: `PASS`
scope: `post-04249-guest-execution-main-push-public-boundary-ci`
source_version_anchor: `0.42.49-admin-smoke`
source_commit_title: `docs: record 0.42.49 guest execution boundary gate`
base_branch: `main`
head_sha: `d09ecfc425f6050a2c182cbcb3090ad2f9fa4827`
run_id: `26449795425`
job_id: `77866996627`
workflow: `Public Boundary Contract`
workflow_file: `.github/workflows/public-boundary.yml`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-26T13:07:07Z`
started_at: `2026-05-26T13:07:11Z`
completed_at: `2026-05-26T13:07:32Z`
completed_at_kst: `2026-05-26T22:07:32+09:00`
conclusion: `success`
checkout_action_version: `actions/checkout@v6.0.2`
fallback_required_guard: `public-boundary-ci-required`
local_pester_command: `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed`
local_pester_result: `76 passed, 0 failed`
local_solution_command: `dotnet test src/DesktopNode.sln --no-restore`
local_solution_result: `700 passed, 0 failed`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

`0.42.49-admin-smoke` Guest Execution policy/API preview disabled boundary,
full admin host mutation gate, installed Web/TUI/CLI current-card evidence, 그리고
manual-admin `0.42.48 -> 0.42.49` readiness blocker 문서를 main에 반영한 뒤
GitHub Actions `Public Boundary Contract`를 확인했다.

## 확인

| 항목 | 값 |
| --- | --- |
| commit | `d09ecfc425f6050a2c182cbcb3090ad2f9fa4827` |
| run | `26449795425`, event `push`, conclusion `success` |
| job | `77866996627`, `public-boundary-ci-required`, conclusion `success` |
| run URL | `[private-archive-repository]/actions/runs/26449795425` |
| job URL | `[private-archive-repository]/actions/runs/26449795425/job/77866996627` |
| predecessor | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-26-04248-manual-admin-postpush-pass.md` |

## 경계

이 evidence는 public-boundary CI PASS와 문서 guard 정합성만 주장한다. Public trusted
signing, public stable installer URL, winget submission, 외부 stable publication,
clean-host public signed install/update/rollback smoke는 계속 ADR-0006 범위 밖이다.
