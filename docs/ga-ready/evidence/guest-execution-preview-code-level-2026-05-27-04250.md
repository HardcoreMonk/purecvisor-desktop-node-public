# Guest Execution preview code-level 2026-05-27 0.42.50

evidence_id: `guest-execution-preview-code-level-2026-05-27-04250`
result: `PASS_CODE_LEVEL_PREVIEW`
scope: `phase4-guest-execution-api-cli-preview-execute-disabled`
adr: `docs/adr/0009-guest-execution-security-boundary.md`
plan: `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md`
provenance_commit: `d42ff7fddc67cbcebbfcbbec3342278511edafb3`
product_payload_change_detected: `true`
host_mutation_performed: `false-code-level`
package_build_performed: `0.42.50-admin-smoke`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

ADR-0009의 두 번째 구현 slice는 실제 guest command execution을 열지 않고,
host mutation 없는 API/CLI preview를 제품 payload로 올렸다. 실행 route, channel
verify/repair, Web/TUI command panel은 계속 닫혀 있다.

## 구현 범위

| 영역 | 상태 |
| --- | --- |
| Runtime policy | `preview_enabled=true`, `execute_enabled=false`, `channel_preview_enabled=true`, `channel_verify_enabled=false`, `channel_repair_enabled=false` |
| API preview | `POST /api/v1/vms/{vmId}/guest/exec/preview`는 `guest-execution-preview.v1` 반환 |
| Channel preview | `POST /api/v1/vms/{vmId}/guest/channel/preview`는 `guest-channel-preview.v1` 반환 |
| CLI preview | `pcvcli vm guest-exec <vm> --dry-run ... -- <command>`와 `pcvcli vm guest-agent-ensure-channel <vm> --dry-run` 연결 |
| Audit preview | `guest-execution-audit-v1` schema에 맞춘 actor/request/command hash/redacted argv projection 반환 |
| Redaction preview | `guest-execution-redaction-v1` policy에 맞춰 secret-like argv/env/credential ref 원문을 반환하지 않음 |
| Secret guard | raw credential option은 CLI에서 거부하고, preview 응답은 redacted argv/hash만 반환 |
| Disabled boundary | `guest/channel/verify`, `guest/channel`과 실제 execution은 `PCV_GUEST_EXEC_DISABLED` 상태 유지 |

## 검증

- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore`: `PASS`, 14 tests
- `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore`: `PASS`, 103 tests
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore`: `PASS`, 251 tests
- `dotnet test src\DesktopNode.sln --no-restore`: `PASS`, 705 tests

## 남은 범위

`IGuestExecutionProvider`, PowerShell Direct execution, queued execute job, timeout/cancel
terminal state, channel verify/repair, Web/TUI direct control은 아직 열지 않는다. 다음
slice는 provider/audit sink/timeout-cancel을 먼저 닫아야 한다.
