# Guest Execution Running Cancel Policy 2026-05-27 0.42.53

evidence_id: `guest-execution-running-cancel-policy-2026-05-27-04253`
result: `PASS_POLICY_CONFIRMED_RUNNING_INTERRUPT_NOT_SUPPORTED`
scope: `guest-execution-timeout-cancel-policy`
version: `0.42.53-admin-smoke`
runtime_policy_cancel_queued_only: `true`
runtime_policy_running_interrupt: `false`
guest_execution_timeout_contract: `queued-job-cancel-before-running-provider-timeout-during-execute-running-interrupt-false`
host_mutation_performed: `false-policy-and-terminal-job-cancel-check`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 확인

`pcvcli --json runtime policy`는 `job_runtime.control.cancel.queued_only=true`,
`job_runtime.control.cancel.running_interrupt=false`를 반환한다. Guest Execution timeout contract는
`provider-timeout`과 `queued job cancel`을 지원하되 running provider interrupt는 아직 지원하지 않는다.

추가로 actual Windows guest credentialed smoke preflight에서 생성된 terminal jobs에 cancel을 요청했다.

```powershell
pcvcli --json job cancel job-072a7d004e7f407d8dccd9a8845d8152
pcvcli --json job cancel job-cbec0b48d48c4a6dbb2add167ec40e91
```

두 요청은 `PCV_JOB_NOT_CANCELABLE`과 `Only queued jobs can be canceled`를 반환했다. 이는 terminal
job에 대한 취소 차단이며 running interrupt 지원 claim이 아니다.

## 판정

0.42.53 기준 제품 정책은 queued 상태 취소와 provider timeout까지다. Running guest process interrupt는
아직 지원하지 않으며, 실제 Windows guest credentialed long-running command가 준비되면 별도 product
payload/evidence로 다룬다.
