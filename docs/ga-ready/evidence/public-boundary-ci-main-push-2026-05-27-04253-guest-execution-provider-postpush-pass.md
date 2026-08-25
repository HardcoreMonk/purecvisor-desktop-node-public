# Public Boundary CI Main Push 2026-05-27 0.42.53 Guest Execution Provider PASS

evidence_id: `public-boundary-ci-main-push-2026-05-27-04253-guest-execution-provider-postpush-pass`
result: `PASS`
scope: `post-04253-guest-execution-provider-main-push-public-boundary-ci`
workflow: `Public Boundary Contract`
source_version_anchor: `0.42.53-admin-smoke`
postmerge_package_anchor: `0.42.53-admin-smoke`
run_id: `26494136304`
job_id: `78018181426`
head_sha: `824540bea237011b73b00c53ff399675b8346c7f`
head_commit_title: `Align guest execution evidence guard with provider gate`
branch: `main`
event: `push`
guard_job: `public-boundary-ci-required`
checkout_action_version: `actions/checkout@v6.0.2`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 확인

`0.42.53-admin-smoke` Guest Execution provider/direct-control package/fullgate/current-card
closure를 main에 반영한 뒤 GitHub Actions `Public Boundary Contract`를 확인했다.
`Run public boundary evidence guard`와 `Verify packaging regression required step`이 PASS했다.

## 경계

이 evidence는 public-boundary guard 실행 결과만 기록한다. Public trusted signing,
external stable publication, winget public submission, public stable installer URL,
public signed clean-host smoke는 계속 ADR-0006 범위 밖이다.
