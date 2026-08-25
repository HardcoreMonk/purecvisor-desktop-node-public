# Guest Execution Running Interrupt/Cancel Design

status: `installed-windows-guest-long-running-cancel-pass`
date: `2026-05-27`
owner: `Desktop Node Runtime/API`
decision_evidence: `docs/ga-ready/evidence/guest-execution-running-cancel-policy-2026-05-27-04253.md`
code_level_evidence: `docs/ga-ready/evidence/guest-execution-running-interrupt-code-level-2026-05-28.md`
installed_evidence: `docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md`
current_runtime_policy: `queued-and-running-guest-execution-cancel-with-provider-token-interrupt`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결정

0.42.53 설치본 payload는 queued job cancel, provider timeout, terminal job cancel rejection까지였다.
2026-05-28 code-level payload는 running guest execution job cancel token path를 열었다. 이어
`0.42.54-admin-smoke` 설치본에서 actual Windows guest long-running command cancel smoke를 실행했고,
API는 running `vm.guest.exec` job에 대해 cancellation token을 provider에 전달하며 PowerShell
Direct bridge process tree를 종료했다. 최종 job state는 `canceled`, error code는
`PCV_JOB_CANCELED`, native error code는 `PCV_NATIVE_OPERATION_CANCELED`다.

## 현재 유지 계약

- `job.cancel`은 queued job과 running guest execution job에 성공한다.
- running provider는 `timeout_sec` 안에서 완료되거나 provider timeout으로 terminal state가 된다.
- terminal job cancel은 `PCV_JOB_NOT_CANCELABLE`로 차단한다.
- Web/TUI/CLI는 공통 `job cancel` route를 사용한다. Installed current-card는 runtime policy와 actual
  running cancel result를 같은 evidence set에 기록했다.
- Audit schema는 `guest-execution-audit-v1`, redaction은 `guest-execution-redaction-v1`을 유지한다.

## 향후 payload 범위

Running interrupt 설치본 smoke 이후 남은 확장 contract는 다음과 같다.

1. Provider session handle registry: job id, VM id, process/session id, timeout, actor를 연결한다.
2. Cancellation requested state: `cancel_requested_at`, `cancel_requested_by`, provider ack 여부를 기록한다.
3. Terminal state 확장: `cancelled`, `cancel-requested-provider-incomplete`, `cancel-timeout`을 분리한다.
4. Guest credential 재사용 금지: cancel 요청은 기존 credential secret을 다시 요구하지 않는다.
5. Audit redaction: command, env, credential ref, provider detail은 계속 redaction한다.
6. RBAC: `job.cancel`과 `guest.exec` scope를 모두 검증하거나 policy로 owner-only cancel을 명시한다.
7. UI affordance: running job에만 cancel action을 노출하고 terminal/queued 상태별 메시지를 분리한다.

## 수용 기준

- Long-running Windows guest command actual VM smoke가 있어야 한다. 0.42.54에서 PASS.
- cancel 요청이 guest process 종료, provider incomplete, provider timeout 중 하나로 terminal evidence를 남겨야 한다. 0.42.54에서 `canceled` PASS.
- secret echo guard와 audit log redaction이 cancel path에서도 통과해야 한다. 0.42.54 installed evidence에서 token/password value 미관측.
- CLI, TUI, Web 모두 같은 job state contract를 표시해야 한다. 0.42.54 current-card에서 runtime/job contract 확인.
- Public trusted signing이나 external stable publication claim과 연결하지 않는다.

## 제외

이번 결정은 Linux guest agent, SSH transport, raw password CLI input, cross-host cancel,
unattended destructive host mutation을 포함하지 않는다. 0.42.54 installed smoke는 internal
admin-smoke evidence이며 public release evidence가 아니다.
