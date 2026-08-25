# 0.42.45 main push public-boundary CI 증거

evidence_id: `public-boundary-ci-main-push-2026-05-26-04245-postmerge-pass`
result: `PASS`
scope: `post-04245-main-push-public-boundary-ci`
source_version_anchor: `0.42.45-admin-smoke`
source_commit_title: `docs: align 0.42.45 public boundary guard`
base_branch: `main`
head_sha: `4f1f0bd8f7ffe9488dbb7175f65013870cf8d58f`
run_id: `26413569064`
job_id: `77753058728`
workflow: `Public Boundary Contract`
workflow_file: `.github/workflows/public-boundary.yml`
workflow_job: `public-boundary-ci-required`
created_at: `2026-05-25T18:02:38Z`
started_at: `2026-05-25T18:02:42Z`
completed_at: `2026-05-25T18:03:05Z`
completed_at_kst: `2026-05-26T03:03:05+09:00`
conclusion: `success`
checkout_action_version: `actions/checkout@v6.0.2`
fallback_required_guard: `public-boundary-ci-required`
local_pester_command: `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed`
local_pester_result: `74 passed, 0 failed`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

`0.42.45-admin-smoke` full admin host mutation, manual-admin package-pair closure,
installed Web/TUI/CLI current-card, console access/account/noVNC smoke가 main에 반영된
뒤 public-boundary 문서 guard를 0.42.45 current anchor로 맞추고 GitHub Actions
`Public Boundary Contract`를 재실행했다.

## 확인

| 항목 | 값 |
| --- | --- |
| commit | `4f1f0bd8f7ffe9488dbb7175f65013870cf8d58f` |
| run | `26413569064`, event `push`, conclusion `success` |
| job | `77753058728`, `public-boundary-ci-required`, conclusion `success` |
| run URL | `[private-archive-repository]/actions/runs/26413569064` |
| job URL | `[private-archive-repository]/actions/runs/26413569064/job/77753058728` |
| predecessor | `docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md` |

## 경계

이 evidence는 public-boundary CI PASS와 문서 guard 정합성만 주장한다. Public trusted
signing, public stable installer URL, winget submission, 외부 stable publication,
clean-host public signed install/update/rollback smoke는 계속 ADR-0006 범위 밖이다.
