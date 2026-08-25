# Guest Execution Security Boundary Design

date: `2026-05-26`
status: `approved-boundary-contract`
scope: `phase4-guest-execution-security`
adr: `docs/adr/0009-guest-execution-security-boundary.md`
predecessor: `docs/adr/0009-guest-execution-security-boundary-candidate.md`
product_payload_change: `false`
host_mutation_performed: `false`
package_build_performed: `false`

## 목표

Phase 4의 목적은 `pcvcli vm guest-exec <vm> -- <command>`와
`pcvcli vm guest-agent-ensure-channel <vm>`를 제품 payload로 열기 전에 보안 경계를
먼저 닫는 것이다. 이 설계는 구현 직전 plan으로 쪼갤 수 있도록 credential, audit,
redaction, timeout/cancel, permission, channel lifecycle, Operator Surface 노출 단계를
명확히 분리한다.

이번 slice는 docs-contract다. API route, CLI command 실행, Web/TUI command panel을
추가하지 않으므로 package/fullgate/manual-admin gate는 다음 product payload로 이월한다.

## Domain Architecture

| Bounded context | 책임 | 금지 |
| --- | --- | --- |
| Runtime/Core | policy flag, capability check, problem details, queued job lifecycle | provider secret value 보관 |
| Guest Execution Security | credential reference, redaction, audit schema, timeout/cancel contract | raw credential logging, synchronous streaming |
| Hyper-V Domain | transport preview/execute/repair provider | Web/TUI permission 판단 |
| Host Ops | audit artifact retention, diagnostics redaction, Event Log bridge | VM product semantics 소유 |
| Operator Surfaces | disabled/status/preview UX, explicit confirmation | ADR 없이 direct command input 노출 |

## Credential Model

초기 product slice는 `credential_ref`만 받는다. `credential_ref`는 Windows Credential
Manager 또는 DPAPI LocalMachine protected reference를 가리킨다.

| 입력 | 허용 여부 | 설명 |
| --- | --- | --- |
| `--credential-ref <name>` | 허용 | reference 이름만 CLI/API에 전달한다. |
| `--username/--password` | 금지 | command history와 process list에 남을 수 있다. |
| `--env SECRET=value` | 금지 | env raw secret은 diagnostics와 audit에 유출될 수 있다. |
| `--stdin-secret-ref <name>` | 후속 후보 | stdin secret이 필요한 경우 redaction proof 뒤에 연다. |
| Web/TUI password input | 금지 | 첫 product slice에서는 secret entry UI를 만들지 않는다. |

Credential reference 관리 자체는 `admin` 또는 별도 `credential.manage` capability로 분리한다.
Guest execution 요청자는 `operate + guest.exec`를 가져야 한다.

## Audit Contract

Audit schema는 `guest-execution-audit-v1`이다. Audit은 command 재현 자료가 아니라 운영
추적 자료다.

필수 필드:

- `request_id`, `job_id`, `operation`
- `actor_id`, `session_id`, `capabilities`
- `vm_id`, `vm_name`
- `command_hash`, `redacted_argv`, `redacted_env_keys`
- `credential_ref_hash`
- `timeout_sec`, `started_at`, `completed_at`
- `terminal_state`, `exit_code`
- `stdout_byte_count`, `stderr_byte_count`, `stdout_digest`, `stderr_digest`

stdout/stderr 원문은 기본 저장하지 않는다. 원문 capture가 필요하면 별도 retention policy,
byte limit, binary reject, redaction test를 구현한 뒤 연다.

## Secret Redaction Contract

Redaction contract version은 `guest-execution-redaction-v1`이며, 네 단계로 적용한다.

1. Key-based redaction: `password`, `token`, `secret`, `key`, `authorization`, `cookie`.
2. Shape-based redaction: JWT, bearer token, SSH/private-key, high-entropy string.
3. Context redaction: command separator 뒤 raw credential처럼 보이는 argv/env/stdin.
4. Artifact redaction: diagnostics bundle, audit log, job summary, Event Log payload.

Redaction engine이 secret-like token을 안전하게 분류하지 못하면 execution preview가
`PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED`를 반환하고 execute route는 열지 않는다.

## Timeout / Cancel Contract

모든 execution은 queued job이다.

| 항목 | 값 |
| --- | --- |
| default timeout | 60 seconds |
| max timeout | 600 seconds |
| output byte limit | stdout 64 KiB, stderr 64 KiB 후보. 원문 저장은 기본 off |
| cancel owner | request actor 또는 `job.cancel` capability actor |
| cancel result | `cancelled` 또는 `cancel-requested-provider-incomplete` |

Timeout은 provider process kill 성공 여부와 별개로 job terminal state에 남긴다. Provider가
guest process를 정리하지 못하면 audit에 cleanup incomplete reason을 남긴다.

## Permission Model

| Capability | 필요한 route |
| --- | --- |
| `operate` | VM status/channel status read |
| `guest.exec` | guest exec preview/execute |
| `guest.channel.configure` | channel repair/apply |
| `job.cancel` | guest exec job cancel |
| `admin` | credential reference 관리, policy override |

Policy가 disabled이면 권한과 무관하게 `PCV_GUEST_EXEC_DISABLED`를 반환한다.

## Channel Contract

`guest-agent-ensure-channel`은 Linux qemu guest agent channel을 복제하지 않는다. Desktop Node의
channel은 Hyper-V transport prerequisite를 의미한다.

| CLI shape | API 후보 | 의미 |
| --- | --- | --- |
| `pcvcli vm guest-agent-ensure-channel <vm> --dry-run` | `POST /api/v1/vms/{vm}/guest/channel/preview` | host-only prerequisite preview |
| `pcvcli vm guest-agent-ensure-channel <vm> --verify --credential-ref <ref>` | `POST /api/v1/vms/{vm}/guest/channel/verify` | queued minimal session verification |
| `pcvcli vm guest-agent-ensure-channel <vm> --repair --yes` | `POST /api/v1/vms/{vm}/guest/channel` | host-side prerequisite repair job |

초기 transport 후보는 Windows guest PowerShell Direct다. Linux guest와 in-guest agent 배포는
새 runtime object 정의가 필요하므로 out-of-product-scope로 유지한다.

## Guest Exec API 후보

CLI execution candidate는 `pcvcli vm guest-exec <vm> --credential-ref <ref> --timeout-sec 60 -- <command>`다.

| Route | Mutation | 설명 |
| --- | --- | --- |
| `GET /api/v1/runtime/policy` | 없음 | `guest_execution.enabled=false`와 capability requirement 노출 |
| `POST /api/v1/vms/{vm}/guest/exec/preview` | 없음 | command hash, redaction result, timeout, capability requirement 확인 |
| `POST /api/v1/vms/{vm}/guest/exec` | guest mutation | queued execution job 생성 |
| `POST /api/v1/jobs/{job}/cancel` | guest mutation | cancel request |
| `GET /api/v1/jobs/{job}` | 없음 | terminal state와 redacted summary 확인 |

첫 product payload는 preview/disabled problem details를 먼저 열고, execute는 provider
redaction/audit/timeout tests가 닫힌 뒤 연다.

## CLI / Web / TUI Slice Split

| Slice | CLI | API | Web | TUI | Gate |
| --- | --- | --- | --- | --- | --- |
| A. Boundary status | unsupported reason을 ADR-0009 적용 문구로 정리 | policy disabled problem details | disabled/status copy | help/status copy | docs + unit/static |
| B. Preview | `guest-exec --dry-run` 후보 | preview route | redaction preview read-only | selected VM preview read-only | package smoke |
| C. Channel ensure | `guest-agent-ensure-channel --dry-run/--repair --yes` | channel preview/repair queued job | channel status card | channel status line | fullgate |
| D. Execute/cancel | `guest-exec --credential-ref ... -- <command>` | queued execute/cancel | confirmation, audit link, cancel | confirmation, audit link, cancel | actual VM + manual-admin |

Web/TUI direct command input은 Slice D 전까지 금지한다.

## Evidence / Release Gate

현재 slice의 증거는 `docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md`가
소유한다. 다음 product payload가 열리면 아래 순서로 gate를 반복한다.

1. Code-level tests: redaction, audit, permission, timeout/cancel, disabled problem details.
2. Package build: 다음 `0.42.x-admin-smoke`.
3. Full admin host mutation gate.
4. Installed Web/TUI/CLI current-card smoke.
5. Manual-admin package-pair descriptor/readiness/campaign.
6. Public-boundary CI guard.

## Out Of Scope

- qemu guest agent channel compatibility claim.
- Linux guest runtime object 계열.
- Web/TUI secret input UI.
- Raw stdout/stderr retention.
- Public trusted signing, winget submission, external stable publication.
