# Windows Credential Manager Transition Preflight Evidence - 2026-05-08

evidence_id: windows-credential-manager-transition-preflight-2026-05-08
scope: windows-credential-manager-transition-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
credential_manager_transition: blocked-by-no-mutation-preflight
credential_manager_mutation: not-run
token_value_observed: false

## 요약

이 slice는 ADR-0005의 Windows Credential Manager transition row를 실제 credential mutation 전 plan-only preflight로 고정한다. `New-PcvWindowsCredentialManagerTransitionPreflight.ps1`는 서비스명, credential target, 현재 DPAPI LocalMachine protected token file storage, 목표 Windows Credential Manager storage, 전환 check 목록을 `summary.json`과 Windows Credential Manager transition plan preview에 기록한다.

이 도구는 token 값을 읽지 않고, credential target write/delete, service reload, service/MSI/firewall/trust-store/LAN/update mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. 실제 Windows Credential Manager 전환 구현과 rollback diagnostics가 닫히기 전까지 `credential_manager_transition: blocked-by-no-mutation-preflight`, `credential_manager_mutation: not-run`, `token_value_observed: false`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1 -ArtifactRoot 'artifacts/windows-credential-manager-transition-preflight-20260508-dryrun' -ServiceName 'PureCVisorDesktopNode' -CredentialTarget 'PureCVisor/DesktopNode/LocalApiToken' -CurrentTokenStorage 'dpapi-local-machine-protected-file' -PlanOnly
```

## Contract

```text
scope: windows-credential-manager-transition-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
credential_manager_transition: blocked-by-no-mutation-preflight
credential_manager_mutation: not-run
token_value_observed: false
transition_checks:
  service-name-present
  credential-target-present
  current-token-storage-recorded
  target-token-storage-recorded
  token-value-not-read
  credential-write-not-executed
  credential-delete-not-executed
  rollback-diagnostics-required
  service-reload-required
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1`는 `New-PcvWindowsCredentialManagerTransitionPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 Windows Credential Manager transition preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- Dry-run artifact root: `artifacts/windows-credential-manager-transition-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=windows-credential-manager-transition-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `credential_manager_transition=blocked-by-no-mutation-preflight`, `credential_manager_mutation=not-run`, `token_value_observed=false`.

이 GREEN은 전환 plan preview와 blocker descriptor만 확인한다. Token value read, credential target write/delete, service reload, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
