# Guest Execution policy/API preview code-level 2026-05-26 0.42.49

evidence_id: `guest-execution-policy-api-preview-code-level-2026-05-26-04249`
result: `PASS_CODE_AND_INSTALLED_DISABLED_BOUNDARY`
scope: `phase4-guest-execution-policy-api-preview-disabled-boundary`
adr: `docs/adr/0009-guest-execution-security-boundary.md`
plan: `docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`
installed_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04249.md`
provenance_commit: `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`
product_payload_change_detected: `true`
host_mutation_performed: `true`
package_build_performed: `0.42.49-admin-smoke`
manual_admin_package_pair_performed: `readiness-blocked-installed-baseline-mismatch`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

ADR-0009의 첫 구현 slice는 실제 guest command execution을 열지 않고, runtime policy와
disabled API boundary를 product payload로 올렸다. 목적은 후속 provider/execute 구현 전에
credential, audit, redaction, RBAC, timeout/cancel 문제 코드를 설치본에서 먼저 고정하는 것이다.

## 구현 범위

| 영역 | 상태 |
| --- | --- |
| Runtime policy | `guest_execution.enabled=false`, ADR path, credential policy, audit/redaction policy, timeout/cancel, required capabilities, problem code catalog 노출 |
| API routes | `POST /api/v1/vms/{vmId}/guest/exec/preview`, `POST /api/v1/vms/{vmId}/guest/channel/preview`, `POST /api/v1/vms/{vmId}/guest/channel/verify`, `POST /api/v1/vms/{vmId}/guest/channel` route 등록 |
| Disabled boundary | route가 native adapter/job runtime을 호출하지 않고 `PCV_GUEST_EXEC_DISABLED` problem details 반환 |
| RBAC boundary | capability 부족 경로는 `PCV_GUEST_EXEC_PERMISSION_DENIED` catalog로 분리 |
| Credential resolver | `wincred:`, `credential-manager:`, `dpapi:` reference만 허용하고 raw secret value를 거부 |
| Redaction engine | secret-like args/env/stdin/stdout/stderr 후보를 `guest-execution-redaction-v1` 규칙으로 masking |
| Audit writer skeleton | `guest-execution-audit-v1` record, command hash, credential ref hash, redacted projection 생성 |

## 검증

- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore`: `PASS`, 13 tests
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore`: `PASS`, 250 tests
- `dotnet test src\DesktopNode.sln --no-restore`: `PASS`
  - Contracts `13`
  - Runtime `17`
  - Service `11`
  - TUI `160`
  - CLI `100`
  - Host `149`
  - API `250`
- 설치본 `pcvcli --json runtime policy`: exit `0`, `guest_execution.enabled=false`
- 설치본 direct API preview POST: HTTP `403`, `operation=vm.guest.exec.preview`,
  `error=PCV_GUEST_EXEC_DISABLED`
- Secret echo guard: request body의 `super-secret-value`와 credential ref
  `PureCVisor/guest/admin`은 response에 나타나지 않았다.

## 남은 범위

Guest command execution provider, PowerShell Direct transport, queued execute job, timeout/cancel
terminal state, Web/TUI command panel, CLI `vm guest-exec` 실행 UX는 아직 열지 않는다. 다음
slice는 disabled boundary 위에서 `guest-agent-ensure-channel` dry-run/verify/repair provider
또는 CLI preview UX 중 하나를 선택한다.
