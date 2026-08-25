# ADR-0009: Guest Execution Security Boundary 후보

상태: 후보
일자: 2026-05-26

## 결정 마커

```text
DESKTOP_NODE_GUEST_EXECUTION_SECURITY_BOUNDARY_DECISION: security-boundary-deferred
phase: Phase 4 Guest Execution / Guest Channel
implementation_status: not-implemented
cli_guest_channel_candidate: pcvcli vm guest-agent-ensure-channel
cli_guest_exec_candidate: pcvcli vm guest-exec
default_transport_decision: undecided
credential_policy: required-before-implementation
audit_log_schema: required-before-implementation
secret_redaction_policy: required-before-implementation
timeout_cancel_policy: required-before-implementation
rbac_capability: guest.exec
web_tui_guest_exec_control: prohibited-until-security-boundary-applied
host_mutation_performed: false
package_build_performed: false
public_release: not-claimed
```

## 맥락

Linux `pcvctl vm guest-exec`는 qemu guest agent/libvirt 계층을 전제로 한다. Desktop Node는
Hyper-V backend이므로 동일 이름을 그대로 열면 credential, audit, secret redaction,
timeout/cancel, 권한 모델이 모두 불분명해진다. 따라서 Guest Execution은 신규 기능이며
ADR-0007 parity 범위 밖에 남아 있다.

## 보류 결정

`pcvcli vm guest-agent-ensure-channel <vm>`과
`pcvcli vm guest-exec <vm> -- <command>`는 아래 보안 경계가 닫힐 때까지 구현하지 않는다.

| 경계 | 필요한 결정 |
| --- | --- |
| credential | guest credential 입력/보관 금지 범위, Credential Manager 사용 여부, one-shot credential 처리 |
| audit | actor, VM, command hash, redacted args, result summary, request id, job id, exit code, timeout 기록 |
| redaction | command line, env, stdout/stderr, file path, secret-like token masking |
| timeout/cancel | default timeout, max timeout, cancel 가능 상태, terminal state |
| RBAC | `guest.exec`, `guest.channel.configure`, `job.cancel` capability 분리 |
| transport | PowerShell Direct, Hyper-V Guest Service Interface, explicit in-guest agent 중 선택 |
| output limit | stdout/stderr byte limit, binary output 금지, artifact retention |
| concurrency | VM별 동시 실행 제한, host-wide 실행 제한 |

## Transport 후보

| 후보 | 장점 | 위험 |
| --- | --- | --- |
| PowerShell Direct | Hyper-V host에서 Windows guest에 직접 접근 가능 | Windows guest 한정, credential/RBAC/audit 강함 필요 |
| Hyper-V Guest Service Interface | Hyper-V Integration Services와 의미가 맞음 | 파일 복사 중심, command exec semantics가 제한적 |
| Explicit in-guest agent | Linux/Windows 확장 가능 | agent 배포, update, trust, compromise boundary가 큼 |

Transport 선택은 별도 implementation ADR에서만 확정한다.

## 최소 Contract 후보

```text
pcvcli vm guest-exec <vm> --timeout-sec 60 -- <command>
pcvcli vm guest-exec <vm> --job <job-id> --cancel
pcvcli vm guest-agent-ensure-channel <vm> --dry-run
```

초기 구현은 반드시 queued job이어야 하며, synchronous command output stream은 열지 않는다.
CLI 출력은 job id, redacted command hash, timeout, status route만 표시한다.

## Web/TUI 경계

Web/TUI guest command 실행 버튼, input panel, history panel은 이 ADR이 적용되기 전까지 금지한다.
Phase 3 Direct Control에서도 Guest Execution은 제외한다. Guest Execution을 Web/TUI에 열려면
별도 UI confirmation, command redaction preview, audit trail link, cancel affordance가 필요하다.

## 검증 Gate

1. Threat model과 abuse case review.
2. Credential policy와 redaction unit tests.
3. Audit schema tests.
4. Timeout/cancel job lifecycle tests.
5. Negative tests: secret-like command, long output, binary output, unauthorized actor.
6. Actual VM smoke는 credential value가 artifact에 남지 않는 것을 먼저 증명해야 한다.
7. Full admin host mutation gate와 manual-admin package-pair closure.

## 경계

이 ADR 후보는 보안 경계를 정의하기 위한 시작점이다. 현재 제품은 guest command execution을
지원하지 않는다. Public trusted signing, external stable publication도 주장하지 않는다.
