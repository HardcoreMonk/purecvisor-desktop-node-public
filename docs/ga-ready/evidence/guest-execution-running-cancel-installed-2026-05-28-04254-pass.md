# Guest Execution Running Cancel Installed 2026-05-28 0.42.54 PASS

evidence_id: `guest-execution-running-cancel-installed-2026-05-28-04254-pass`
result: `PASS_INSTALLED_WINDOWS_GUEST_RUNNING_CANCEL`
scope: `installed-windows-guest-execution-running-cancel-smoke`
version: `0.42.54-admin-smoke`
artifact_root: `artifacts/guest-execution-running-cancel-installed-20260528-04254`
summary: `artifacts/guest-execution-running-cancel-installed-20260528-04254/summary.json`
vm_name: `pcv-guest-installed-04253-r1`
credential_ref_type: `dpapi-local-machine`
credential_ref: `dpapi:<protected-file>`
create_job_id: `job-b06eb90e549a481bbf4003399b5604f8`
final_status: `canceled`
final_error_code: `PCV_JOB_CANCELED`
native_error_code: `PCV_NATIVE_OPERATION_CANCELED`
running_interrupt_observed: `true`
token_value_observed: `false`
password_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 확인

설치본 `0.42.54-admin-smoke`에서 persistent Windows VHD target
`pcv-guest-installed-04253-r1`에 `Start-Sleep -Seconds 120` guest command를 queue했다.
worker가 job을 `running`으로 전환한 뒤 `pcvcli --json job cancel`을 호출했고,
provider cancellation token이 PowerShell Direct bridge process tree를 종료했다.

| 항목 | 결과 |
| --- | --- |
| create route | `pcvcli --json vm guest-exec ... -- Start-Sleep -Seconds 120` |
| running state observed | `true` |
| cancel response state | `running` with cancel request accepted |
| terminal state | `canceled` |
| API error code | `PCV_JOB_CANCELED` |
| native error code | `PCV_NATIVE_OPERATION_CANCELED` |
| secret echo guard | token/password value not observed |

## 경계

이 evidence는 internal installed admin-smoke와 실제 Windows guest target 기반의 running
cancel smoke다. Public trusted signing, winget public submission, external stable publication은
계속 범위 밖이다.
