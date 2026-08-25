# Public Boundary CI Main Push 2026-05-27 0.42.50 Guest Execution Preview PASS

evidence_id: `public-boundary-ci-main-push-2026-05-27-04250-guest-execution-preview-postpush-pass`
result: `PASS`
scope: `post-04250-guest-execution-preview-main-push-public-boundary-ci`
workflow: `Public Boundary Contract`
source_version_anchor: `0.42.50-admin-smoke`
postmerge_package_anchor: `0.42.50-admin-smoke`
run_id: `26489610881`
job_id: `78004396577`
head_sha: `baba155d6adfd4c9e2b2ba179d6727bb5035d1fc`
head_commit_title: `docs: align 0.42.50 public boundary guard`
branch: `main`
event: `push`
guard_job: `public-boundary-ci-required`
checkout_action_version: `actions/checkout@v6.0.2`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 확인

`0.42.50-admin-smoke` Guest Execution API/CLI preview package/fullgate/current-card
closure를 main에 반영한 뒤 GitHub Actions `Public Boundary Contract`를 확인했다.
`Run public boundary evidence guard`와 `Verify packaging regression required step`이
PASS했다.

## 경계

이 evidence는 public-boundary guard 실행 결과만 기록한다. Public trusted signing,
external stable publication, winget public submission, public stable installer URL,
public signed clean-host smoke는 계속 ADR-0006 범위 밖이다.
