# Guest Execution Security Boundary Implementation Plan

**Goal:** ADR-0009에서 확정한 Guest Execution / Guest Channel 보안 경계를 코드 구현 slice로
안전하게 쪼갠다. 첫 구현은 disabled/policy/preview부터 시작하고, 실제 guest command
execution은 credential/audit/redaction/timeout/cancel evidence가 닫힌 뒤 연다.

## 1. Contract Tests First

- [x] Runtime policy에 `guest_execution.enabled=false`, required capabilities, problem code를 노출하는 test를 추가한다.
- [x] Guest execution disabled route가 `PCV_GUEST_EXEC_DISABLED` problem details를 반환하는 test를 추가한다.
- [x] Unauthorized actor가 `PCV_GUEST_EXEC_PERMISSION_DENIED`를 받는 account/RBAC integration test를 추가한다.
- [x] Redaction engine snapshot test를 command line, env, stdin, stdout/stderr 후보에 대해 추가한다.
- [x] Audit schema `guest-execution-audit-v1` snapshot test를 추가한다.
- [ ] Timeout/cancel terminal state test를 job runtime에 추가한다.

## 2. Runtime/Core API Contract

- [x] `DesktopNodeRuntimePolicy`에 guest execution policy block을 추가한다.
- [x] `ProblemDetails` catalog에 `PCV_GUEST_EXEC_*` code를 추가한다.
- [x] `GuestExecutionPreviewResult`, `GuestChannelPreviewResult`, credential projection, command hash helper contract DTO를 추가한다.
- [x] Preview route는 host mutation 없이 command hash, redaction result, required capability만 반환한다.
- [x] Execute/channel verify/repair route는 policy disabled 상태에서는 job/native adapter를 생성하지 않는다.

## 3. Credential / Redaction / Audit

- [x] `IGuestCredentialReferenceResolver` 역할의 credential reference resolver를 contract layer에 추가하고 raw secret value가 API/CLI 경계 밖으로 나오지 않게 한다.
- [x] Windows Credential Manager 또는 DPAPI protected reference resolver 중 첫 provider를 선택한다.
- [x] `GuestExecutionRedactor`를 추가해 key-based, shape-based, context redaction을 같은 규칙으로 처리한다.
- [x] `GuestExecutionAuditWriter` skeleton을 추가한다.
- [x] Secret-like token이 남으면 preview/execute를 `PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED`로 차단한다.

## 4. Hyper-V Provider

- [ ] `IGuestExecutionProvider` interface를 Hyper-V domain에 추가한다.
- [ ] `PowerShellDirectGuestExecutionProvider` 후보를 Windows guest only로 구현한다.
- [ ] `guest-agent-ensure-channel --dry-run`은 VM state와 transport prerequisite만 확인한다.
- [ ] `--verify --credential-ref`는 queued verification job으로 최소 세션만 확인한다.
- [ ] `--repair --yes`는 host-side prerequisite만 복구하고 in-guest agent 설치는 하지 않는다.
- [ ] Provider output은 byte limit과 digest만 반환하고 raw stdout/stderr retention은 기본 off로 둔다.

## 5. PCVCLI

- [x] `pcvcli vm guest-agent-ensure-channel <vm> --dry-run`을 preview route에 연결한다.
- [ ] `pcvcli vm guest-agent-ensure-channel <vm> --repair --yes`를 queued channel ensure route에 연결한다.
- [x] `pcvcli vm guest-exec <vm> --dry-run -- <command>`을 preview route에 연결한다.
- [ ] `pcvcli vm guest-exec <vm> --credential-ref <ref> --timeout-sec <n> -- <command>`를 queued execute route에 연결한다.
- [ ] CLI는 command hash, job id, timeout, audit id만 출력하고 secret value/stdout/stderr raw stream을 출력하지 않는다.

## 6. Web / TUI Operator Surface

- [ ] Web VM detail에 Guest Execution disabled/status row를 추가하되 command input은 열지 않는다.
- [ ] TUI help/status에 Guest Execution disabled boundary를 표시한다.
- [ ] Preview slice가 닫힌 뒤 Web/TUI redaction preview read-only panel을 추가한다.
- [ ] Execute slice가 닫힌 뒤에만 explicit confirmation, audit reason, cancel affordance를 추가한다.
- [ ] Web/TUI는 `guest.exec` capability 없을 때 실행 affordance를 표시하지 않는다.

## 7. Evidence And Release

- [x] Code-level evidence에 redaction/audit/disabled-boundary PASS를 기록한다.
- [x] Product payload가 열리면 다음 `0.42.x-admin-smoke` package build를 실행한다.
- [x] 새 package 기준 full admin host mutation gate를 실행한다.
- [x] Installed Web/TUI/CLI current-card smoke에서 disabled/preview surface를 재확인한다.
- [ ] Manual-admin package-pair descriptor/readiness/campaign을 닫는다. 현재 04248->04249 readiness는 installed baseline mismatch로 blocked이며 dedicated 0.42.48 baseline host가 필요하다.
- [ ] Public-boundary CI guard를 main push 기준으로 확인한다.

## 8. Current Docs-Contract Closure

- [x] ADR-0009 적용 문서 작성.
- [x] Guest execution credential/audit/redaction/timeout/cancel/RBAC 설계 작성.
- [x] Guest channel dry-run/verify/repair contract 작성.
- [x] CLI/API/Web/TUI product slice split 작성.
- [x] Docs-only evidence와 current ledger/index 갱신.

## 9. 0.42.49 Product Payload Closure

- [x] Runtime policy guest execution disabled block, disabled API routes, problem code catalog, credential reference resolver, redaction engine, audit writer skeleton 구현.
- [x] `0.42.49-admin-smoke` clean package build PASS.
- [x] `full-admin-host-mutation-gate-20260526-04249` PASS.
- [x] 설치본 Web/TUI/CLI current-card에서 `guest_execution.enabled=false`, direct preview HTTP `403` / `PCV_GUEST_EXEC_DISABLED`, secret/ref echo 없음 확인.
- [x] Evidence:
  `docs/ga-ready/evidence/guest-execution-policy-api-preview-code-level-2026-05-26-04249.md`,
  `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`,
  `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04249-hostmutation.md`,
  `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04249.md`.
- [ ] `0.42.48-admin-smoke -> 0.42.49-admin-smoke` manual-admin package-pair closure. 현재 evidence:
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04248-04249-readiness-blocked.md`.

## 10. 0.42.50 Preview Product Payload Closure

- [x] API preview route가 `guest-execution-preview.v1`, `guest-channel-preview.v1` contract를 반환한다.
- [x] PCVCLI `vm guest-exec --dry-run`과 `vm guest-agent-ensure-channel --dry-run`을 preview route에 연결했다.
- [x] 실제 execution, channel verify/repair, Web/TUI direct command control은 disabled 상태를 유지한다.
- [x] Code-level tests PASS: Contracts `14`, CLI `103`, API `251`, full solution `705`.
- [x] `0.42.50-admin-smoke` package build PASS.
- [x] `full-admin-host-mutation-gate-20260527-04250` PASS.
- [x] 설치본 Web/TUI/CLI current-card에서 runtime policy preview enabled, execute disabled, preview secret echo guard PASS 확인.
- [ ] `0.42.49-admin-smoke -> 0.42.50-admin-smoke` manual-admin package-pair closure. 현재 evidence:
  `docs/ga-ready/evidence/manual-admin-campaign-2026-05-27-04249-04250-readiness-blocked.md`.

## 11. 2026-05-29 Guest Execution redaction hardening code-level

- [x] `GuestExecutionRedactor`가 AWS access-key shape와 공백 없는 고엔트로피 token shape를
  secret-like material로 분류한다.
- [x] `--password=value` assignment는 key를 보존하고 value만 `[REDACTED]`로 유지한다.
- [x] guest-exec preview route가 secret-like command/environment를 `200` preview success로
  반환하지 않고 `PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED` `400`으로 차단한다.
- [x] queued guest-exec route의 기존 secret-like command 차단 계약을 유지한다.
- [x] Code-level evidence:
  `docs/ga-ready/evidence/guest-execution-redaction-hardening-code-level-2026-05-29.md`.
- [ ] `0.42.59-admin-smoke` package build, full admin host mutation, manual-admin package-pair,
  installed Web/TUI/CLI current-card smoke로 제품 설치본 승격을 닫는다.
