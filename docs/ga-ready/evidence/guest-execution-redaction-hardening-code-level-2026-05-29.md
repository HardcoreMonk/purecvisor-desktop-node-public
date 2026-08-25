# Guest Execution redaction hardening code-level 2026-05-29

evidence_id: `guest-execution-redaction-hardening-code-level-2026-05-29`
result: `PASS_CODE_LEVEL`
scope: `guest-exec-preview-and-queued-input-secret-redaction-hardening`
status: `pass-code-level-next-package-required`
product_payload_change: `true`
host_mutation_performed: `false`
package_build_performed: `false`
next_package_gate_candidate: `0.42.59-admin-smoke`
next_manual_admin_package_pair_candidate: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
adr: `docs/adr/0009-guest-execution-security-boundary.md`
plan: `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md`
redaction_policy: `guest-execution-redaction-v1`
problem_code: `PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED`
secret_value_observed: `false`
credential_ref_value_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 변경

- `GuestExecutionRedactor`가 AWS access-key shape와 공백 없는 고엔트로피 token shape를
  secret-like material로 분류한다.
- `--password=value` 같은 assignment는 기존 계약처럼 key를 남기고 value만 `[REDACTED]`로
  보존한다.
- `POST /api/v1/vms/{vm}/guest/exec/preview`는 redaction이 필요한 command/environment를
  preview success로 반환하지 않고 `PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED` `400`으로
  차단한다.
- queued `POST /api/v1/vms/{vm}/guest/exec`의 기존 secret-like command 차단 계약은 유지한다.
- failure body는 raw command secret, high-entropy token value, protected credential reference
  원문을 포함하지 않는다.

## 검증

```powershell
dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --nologo -p:UseSharedCompilation=false
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --nologo --filter GuestExecution -p:UseSharedCompilation=false
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --nologo -p:UseSharedCompilation=false
```

## 경계

이 evidence는 code-level hardening이다. 설치본 package build, full admin host mutation gate,
manual-admin package-pair closure, installed Web/TUI/CLI current-card smoke는 아직 실행하지
않았다. 다음 제품화 gate는 `0.42.59-admin-smoke` package chain에서 닫는다.
