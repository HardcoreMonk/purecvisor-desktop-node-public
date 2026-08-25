# Service Token Rotation Revoke Preflight Evidence - 2026-05-08

evidence_id: service-token-rotation-revoke-preflight-2026-05-08
scope: service-token-rotation-revoke-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
service_token_rotation_revoke: blocked-by-no-mutation-preflight
service_token_mutation: not-run
service_token_value_observed: false
new_token_value_created: false
service_reload_status: not-run
old_token_rejection_status: not-run
token_rotation_audit_status: not-run

## 요약

이 slice는 ADR-0005의 service token rotation/revoke mutation API row를 실제 service token mutation 전 plan-only preflight로 고정한다. `New-PcvServiceTokenRotationRevokePreflight.ps1`는 서비스명, protected token path, 현재 DPAPI LocalMachine protected token file storage, rotation mode, rotation check 목록을 `summary.json`과 Service token rotation revoke plan preview에 기록한다.

이 도구는 current service token 값을 읽지 않고, 새 token 값을 생성하지 않으며, protected token file write, service token policy reload, old-token rejection verification, audit record write, service/MSI/firewall/trust-store/LAN/update mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. 실제 token rotation/revoke implementation과 rollback diagnostics가 닫히기 전까지 `service_token_rotation_revoke: blocked-by-no-mutation-preflight`, `service_token_mutation: not-run`, `service_token_value_observed: false`, `new_token_value_created: false`, `service_reload_status: not-run`, `old_token_rejection_status: not-run`, `token_rotation_audit_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1 -ArtifactRoot 'artifacts/service-token-rotation-revoke-preflight-20260508-dryrun' -ServiceName 'PureCVisorDesktopNode' -ProtectedTokenPath '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json' -CurrentTokenStorage 'dpapi-local-machine-protected-file' -RotationMode 'rotate-and-revoke-old-token' -PlanOnly
```

## Contract

```text
scope: service-token-rotation-revoke-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
service_token_rotation_revoke: blocked-by-no-mutation-preflight
service_token_mutation: not-run
service_token_value_observed: false
new_token_value_created: false
service_reload_status: not-run
old_token_rejection_status: not-run
token_rotation_audit_status: not-run
rotation_checks:
  service-name-present
  current-token-storage-recorded
  protected-token-path-recorded
  rotation-mode-recorded
  token-value-not-read
  new-token-not-generated
  protected-token-write-not-executed
  service-reload-not-executed
  old-token-rejection-not-executed
  audit-record-not-written
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1`는 `New-PcvServiceTokenRotationRevokePreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 service token rotation revoke preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- Dry-run artifact root: `artifacts/service-token-rotation-revoke-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=service-token-rotation-revoke-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `service_token_rotation_revoke=blocked-by-no-mutation-preflight`, `service_token_mutation=not-run`, `service_token_value_observed=false`, `new_token_value_created=false`, `service_reload_status=not-run`, `old_token_rejection_status=not-run`, `token_rotation_audit_status=not-run`.

이 GREEN은 rotation/revoke plan preview와 blocker descriptor만 확인한다. Token value read, new token value creation, protected token file write, service token policy reload, old-token rejection verification, audit record write, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
