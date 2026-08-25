# ADR-0009: Guest Execution Security Boundary

상태: 적용 중
일자: 2026-05-26

## 결정 마커

```text
DESKTOP_NODE_GUEST_EXECUTION_SECURITY_BOUNDARY_DECISION: accepted-boundary-contract
phase: Phase 4 Guest Execution / Guest Channel
supersedes: docs/adr/0009-guest-execution-security-boundary-candidate.md
implementation_status: provider-direct-control-applied
product_payload_change: true
cli_guest_channel_candidate: pcvcli vm guest-agent-ensure-channel
cli_guest_exec_candidate: pcvcli vm guest-exec
guest_exec_surface_status: dry-run-preview-and-queued-provider-enabled
default_transport_decision: windows-powershell-direct-first-candidate
credential_policy: protected-secret-reference-only-no-raw-cli-args
credential_storage_policy: windows-credential-manager-or-dpapi-protected-reference
audit_log_schema: guest-execution-audit-v1-required
secret_redaction_policy: guest-execution-redaction-v1-required
argv_fidelity_policy: guest-boundary-argv-as-data-no-guest-side-reparse
argv_fidelity_evidence: docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md
timeout_cancel_policy: provider-timeout-queued-and-running-guest-execution-cancel
running_interrupt_decision: installed-windows-guest-long-running-cancel-pass
running_interrupt_design: docs/superpowers/specs/2026-05-27-purecvisor-desktop-node-guest-execution-running-interrupt-cancel-design.md
running_interrupt_code_evidence: docs/ga-ready/evidence/guest-execution-running-interrupt-code-level-2026-05-28.md
running_interrupt_installed_evidence: docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md
installed_affordance_evidence: docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04255.md
latest_actual_guest_exec_evidence: docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md
secret_redaction_hardening_decision: preview-and-execute-block-secret-like-material
secret_redaction_hardening_evidence: docs/ga-ready/evidence/guest-execution-redaction-hardening-code-level-2026-05-29.md
rbac_capabilities: operate, guest.exec, guest.channel.configure, job.cancel
web_tui_guest_exec_control: enabled-with-confirmed-queued-provider-route
host_mutation_performed: 0.42.55-admin-smoke-full-admin-gate
package_build_performed: 0.42.55-admin-smoke
public_release: not-claimed
next_product_payload_gate: 0.42.59-admin-smoke-package-fullgate-manual-admin
```

## 결정

Guest Execution / Guest Channel은 Desktop Node의 보안 bounded context다. 이 ADR은
`pcvcli vm guest-exec <vm> -- <command>`와
`pcvcli vm guest-agent-ensure-channel <vm>`가 반드시 지켜야 하는 credential, audit,
secret redaction, timeout/cancel, RBAC, channel lifecycle 경계를 제품 계약으로 고정한다.

2026-05-27 `0.42.50-admin-smoke`는 host mutation 없는 preview route와 PCVCLI dry-run
command까지만 포함한 predecessor다. 같은 날 `0.42.53-admin-smoke`에서 provider route,
channel verify/repair, Web/TUI direct-control surface를 제품 payload로 열었다. Runtime
policy는 `guest_execution.enabled=true`, `execute_enabled=true`, channel verify/repair
true를 보고한다. 실제 Windows guest credentialed execution smoke와 Web/TUI actual VM smoke는
`pcv-guest-installed-04253-r1` persistent Windows VHD target과 DPAPI LocalMachine credential
reference 기준으로 PASS했다. 2026-05-28 `0.42.54-admin-smoke` 설치본과 full admin gate는 running
guest execution cancel token path를 포함하며, actual long-running Windows guest smoke에서
`PCV_JOB_CANCELED` / `PCV_NATIVE_OPERATION_CANCELED`로 PASS했다. 같은 날
`0.42.55-admin-smoke`는 Web/TUI running cancel affordance를 설치본 current-card로 승격하고,
actual credentialed guest-exec를 persistent Windows VHD target에서 재확인했다.

## Credential 경계

Guest credential은 CLI 인자, API body의 raw secret, job summary, audit log, diagnostics
bundle, Web/TUI state에 평문으로 남기지 않는다.

| 항목 | 결정 |
| --- | --- |
| credential source | `credential_ref`만 허용한다. 참조는 Windows Credential Manager 또는 DPAPI LocalMachine protected reference를 가리킨다. |
| one-shot secret | 초기 product slice에서는 금지한다. 필요하면 별도 ADR/plan에서 no-echo prompt와 redaction proof를 먼저 추가한다. |
| CLI args | `--username`, `--password`, `--token`, `--env SECRET=...` 형태의 raw secret 입력을 금지한다. |
| API request | secret value 대신 `credential_ref`, `stdin_secret_ref`, `env_secret_refs`만 받을 수 있다. |
| diagnostics | secret reference name은 남길 수 있지만 secret value와 reversible material은 남기지 않는다. |

## Argv Fidelity 경계

**Guest 실행 경계를 넘는 인자는 데이터로 전달하며, guest 측에서 코드로 재해석하지 않는다.**

이 조항은 2026-08-08에 추가됐다. FC-12(b)의 근본 원인이 인코딩이 아니라 argv 전달이었고
(`docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`), 그때까지 이 ADR에는
대응 조항이 없었다. PCVCLI 계약 문서가 `pcvcli vm guest-exec <vm> -- <command>`로 argv 전달을
문서화하고 있었을 뿐이므로, **구현이 그 계약을 어겨도 위반되는 ADR이 없었다.** 조항을 두는 목적은
그 구조를 닫는 것이다.

| 항목 | 결정 |
| --- | --- |
| 전달 형태 | argv는 배열로 transport에 넘긴다. 원소 `0`이 명령이고 나머지는 splat된 인자다. |
| 금지 | argv 원소를 구분자로 이어붙여 guest 측 shell/PowerShell이 다시 파싱하게 하는 것을 금지한다. |
| 재해석 금지 대상 | 공백, 따옴표, `$(...)` subexpression, `;` 등 문장 구분자, 파이프, 리다이렉션. 호출자가 리터럴로 넘긴 것은 리터럴로 도착해야 한다. |
| 인코딩 | UTF-8 왕복을 유지한다. 비 ASCII 인자의 byte 길이가 보존돼야 한다. |
| 적용 범위 | guest 실행 경계를 넘는 모든 transport. 현재 구현은 PowerShell Direct다. |

이 결함은 **권한 상승이 아니었다.** 이 endpoint의 목적 자체가 인가된 호출자의 guest 명령 실행이기
때문이다. 실질 위험은 두 가지이며, 조항은 둘 다를 대상으로 한다.

- 운영자가 넘긴 리터럴 인자가 조용히 재해석된다 (정합성 결함).
- 자동화가 신뢰할 수 없는 데이터를 argv 원소로 넘기면 그 데이터가 실행 가능해진다 (호출자 측 위험).

계약은 `GuestExecutionArgvFidelityTests` `6`건이 잠근다. 조항은 그 테스트를 뒤따라온 문서이지
새 요구사항이 아니다 — 구현과 테스트는 2026-08-06에 이미 이 형태다.

## Audit 경계

모든 guest-exec/channel mutation은 `guest-execution-audit-v1` 이벤트를 남긴다. Audit record는
명령 실행을 재현할 수 있는 secret value를 담지 않고, 운영자가 문제를 추적할 수 있는 식별자와
요약만 보존한다.

| 필드 | 필수 여부 | 설명 |
| --- | --- | --- |
| `schema_version` | 필수 | `guest-execution-audit-v1` |
| `request_id`, `job_id` | 필수 | API request와 queued job 상관관계 |
| `actor_id`, `session_id`, `capabilities` | 필수 | 호출 주체와 승인된 capability |
| `vm_id`, `vm_name` | 필수 | 대상 VM 식별자 |
| `operation` | 필수 | `vm.guest.exec`, `vm.guest.channel.preview`, `vm.guest.channel.ensure`, `job.cancel` |
| `command_hash` | guest-exec 필수 | redacted argv canonical form의 SHA-256 |
| `redacted_argv`, `redacted_env_keys` | guest-exec 필수 | secret-like value 제거 후 표시 가능한 형태 |
| `credential_ref_hash` | credential 사용 시 필수 | credential reference value가 아니라 reference id hash |
| `timeout_sec`, `started_at`, `completed_at` | 필수 | timeout/cancel 분석 |
| `exit_code`, `terminal_state` | 완료 시 필수 | `succeeded`, `failed`, `timed_out`, `cancelled`, `channel_unavailable` |
| `stdout_digest`, `stderr_digest` | output capture 시 필수 | 원문 output 대신 digest와 byte count |

## Secret Redaction 경계

Redaction은 command line, environment, stdin, stdout/stderr, diagnostics artifact에 같은 규칙을
적용한다.

1. `password`, `token`, `secret`, `key`, `credential`, `authorization`, `cookie` 계열 key는
   값 전체를 masking한다.
2. 고엔트로피 문자열, bearer/JWT/SSH/private-key 형태, Windows credential blob 형태는
   key 이름과 무관하게 masking한다.
3. stdout/stderr 원문 capture는 기본 비활성화한다. 필요하면 byte limit, binary reject,
   redaction proof test를 먼저 통과해야 한다.
4. Redaction 실패 또는 미분류 secret-like token 감지 시 preview와 command execution은
   `PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED`로 거부한다. 2026-05-29 code-level hardening은
   AWS access-key shape와 공백 없는 고엔트로피 token shape를 이 gate에 포함했다.

## Timeout / Cancel 경계

Guest Execution은 반드시 queued job으로 실행한다. CLI synchronous streaming은 초기 product
slice에서 금지한다.

| 항목 | 결정 |
| --- | --- |
| default timeout | 60초 |
| max timeout | 600초. 더 긴 작업은 별도 long-running policy가 필요하다. |
| cancel | `job.cancel` capability가 있는 actor만 요청한다. |
| cancel semantics | 0.42.53에서는 queued job cancel만 성공한다. Running provider interrupt는 별도 product payload까지 defer한다. |
| terminal states | `succeeded`, `failed`, `timed_out`, `cancelled`, `channel_unavailable`, `permission_denied`, `redaction_required` |

## Permission 경계

Guest Execution은 기존 `operate` 권한만으로 열지 않는다.

| Capability | 의미 |
| --- | --- |
| `operate` | VM operator session 기본 권한. 단독으로 guest-exec 불가 |
| `guest.exec` | guest command execution 요청 권한 |
| `guest.channel.configure` | guest channel ensure/repair 요청 권한 |
| `job.cancel` | 본인 또는 policy가 허용한 guest-exec job cancel 권한 |
| `admin` | policy override와 credential reference 관리 권한 |

권한이 없으면 API는 `PCV_GUEST_EXEC_PERMISSION_DENIED` problem details를 반환한다. 기능이
policy상 닫혀 있으면 `PCV_GUEST_EXEC_DISABLED`를 반환한다.

## Channel Lifecycle 경계

`guest-agent-ensure-channel`은 qemu guest agent channel 생성 명령이 아니다. Desktop Node에서는
Hyper-V/Windows guest 실행 채널의 준비 상태를 확인하고, 허용된 경우에만 host-side channel
prerequisite를 복구하는 명령으로 정의한다.

| 모드 | Host mutation | Guest command | 결정 |
| --- | --- | --- | --- |
| `--dry-run` | 없음 | 없음 | VM 존재, power state, 지원 transport 후보, 필요한 credential policy를 preview한다. |
| `--verify --credential-ref <ref>` | 없음 | 최소 세션 검증 가능 | queued verification job으로 실행하고 audit/redaction/timeout을 적용한다. |
| `--repair --yes` | 가능 | 없음 | Hyper-V Integration Service 등 host-side prerequisite만 복구한다. in-guest agent 설치는 하지 않는다. |

초기 transport 후보는 Windows guest 대상 PowerShell Direct다. Hyper-V Guest Service Interface는
파일 전달/복구 보조 후보로만 다루며, explicit in-guest agent 배포는 이 ADR 범위 밖이다.

## API / CLI / Web / TUI 경계

현재 구현 slice는 API policy/preview, queued provider route, channel verify/repair,
Operator Surface direct control을 열었다. 실제 Windows guest credentialed execution smoke는
persistent Windows VHD target과 protected credential reference 기준으로 닫았다.
Running interrupt/cancel semantics는 code-level로 시작됐고
`docs/superpowers/specs/2026-05-27-purecvisor-desktop-node-guest-execution-running-interrupt-cancel-design.md`에
따라 설치본/actual long-running guest smoke로 승격해야 한다.

| Surface | 첫 product slice | 이후 slice |
| --- | --- | --- |
| Local API | policy, preview, queued execute, channel verify/repair, running guest execution cancel token path | installed actual long-running guest cancel smoke, credential lifecycle |
| PCVCLI | dry-run, queued `guest-exec`, channel verify/repair, `job cancel` route | installed running job cancel affordance smoke |
| Web Console | redaction/audit preview, credential-ref input, confirmation, queued action | credential lifecycle, running job cancel affordance |
| TUI | runtime status와 Guest Execution affordance | full interactive command panel/running job cancel |

Web/TUI는 `0.42.53-admin-smoke`부터 confirmation이 필요한 queued provider route로
Guest Execution direct-control surface를 노출한다. 실행 버튼과 command input panel은 raw
secret을 받지 않고 protected credential reference만 허용한다. 실제 운영 실행을 더 넓히려면
다음 evidence가 필요하다.

1. Credential reference lifecycle/rotation evidence.
2. Running interrupt/cancel installed package와 actual long-running guest smoke.
3. Manual-admin package-pair closure evidence.
4. Security review evidence.

## Problem Details

초기 route 구현은 아래 problem code를 안정 계약으로 사용한다.

```text
PCV_GUEST_EXEC_DISABLED
PCV_GUEST_EXEC_PERMISSION_DENIED
PCV_GUEST_EXEC_CHANNEL_UNAVAILABLE
PCV_GUEST_EXEC_TIMEOUT
PCV_GUEST_EXEC_CANCELLED
PCV_GUEST_EXEC_OUTPUT_LIMIT_EXCEEDED
PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED
PCV_GUEST_EXEC_TRANSPORT_UNSUPPORTED
PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED
PCV_GUEST_EXEC_COMMAND_REQUIRED
```

## 검증 Gate

1. Credential source와 secret redaction unit tests.
2. Audit schema snapshot tests.
3. Unauthorized actor, missing capability, disabled policy negative tests.
4. Timeout/cancel job lifecycle tests.
5. Channel dry-run/verify/repair contract tests.
6. CLI no-secret-echo tests.
7. Web/TUI confirmation/direct-control UX tests.
8. Actual VM smoke는 credential value가 artifact에 남지 않는 것을 먼저 증명한다.
9. Product payload가 열리면 package build, full admin host mutation, manual-admin package-pair,
   public-boundary guard를 반복한다.
10. Argv fidelity 계약 tests. 공백, subexpression, 문장 구분자, 비 ASCII 인자가 guest에서
    리터럴로 도착하는 것을 확인한다. 현재 소유자는 `GuestExecutionArgvFidelityTests` `6`건이다.

## 경계

이 ADR은 Guest Execution 보안 경계를 확정한다. 현재 제품은 provider-backed queued guest
execution route와 실제 Windows guest credentialed execution smoke를 지원하지만, public release
claim은 아직 별도 evidence가 필요하다. Public trusted signing, external stable publication,
winget public submission, public clean-host signed smoke도 주장하지 않는다.
