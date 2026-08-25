# Guest Execution Running Interrupt Code-Level 2026-05-28

evidence_id: `guest-execution-running-interrupt-code-level-2026-05-28`
result: `PASS_CODE_LEVEL_RUNNING_GUEST_EXEC_CANCEL_TOKEN_PATH`
scope: `runtime-api-running-guest-execution-cancel`
status: `pass-code-level-promoted-by-04254-installed-smoke`
successor_installed_evidence: `docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md`
policy_cancel_contract: `queued-and-running-guest-execution-cancel-with-provider-token-interrupt`
job_cancel_running_response: `202-cancel-requested`
terminal_state: `canceled`
provider_signal: `CancellationTokenSource.Cancel`
transport_interrupt: `PowerShell Direct bridge process kill on cancellation token`
audit_redaction_policy: `guest-execution-redaction-v1`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 변경

- Runtime/API는 running `vm.guest.exec`와 `vm.guest.channel.verify` job에 대해 provider
  cancellation token을 보관한다.
- `POST /api/v1/jobs/{jobId}/cancel`은 running guest execution job에 대해 `202`와
  `PCV_JOB_CANCEL_REQUESTED` 상태를 반환하고 provider token을 signal한다.
- Provider가 `PCV_NATIVE_OPERATION_CANCELED`를 반환하면 job은 `canceled` terminal state와
  `PCV_JOB_CANCELED` error code로 닫힌다.
- PowerShell Direct transport는 cancellation token이 signal되면 bridge process tree를 종료한다.
- Runtime policy는 `running_interrupt=true`, `queued_only=false`를 보고한다.

## 검증

```powershell
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~RunningGuestExecutionJobCancelRequestsProviderCancellationAndFinishesCanceled"
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --filter "FullyQualifiedName~RuntimePolicySerializesPhase24JobRuntimeContract|FullyQualifiedName~RuntimePolicyDeclaresGuestExecutionProviderBoundary"
```

## 후속 gate

- `0.42.54-admin-smoke` package build, installed current-card smoke, 실제 Windows guest
  long-running command cancel smoke는 successor installed evidence에서 PASS했다.
- CLI/TUI/Web에서 running job cancel affordance가 같은 state contract를 표시하는지 제품 UI로 확장한다.
- Public trusted signing, winget, external stable publication은 계속 범위 밖이다.
