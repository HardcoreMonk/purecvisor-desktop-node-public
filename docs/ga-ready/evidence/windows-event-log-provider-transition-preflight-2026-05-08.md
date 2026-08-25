# Windows Event Log Provider Transition Preflight Evidence - 2026-05-08

evidence_id: windows-event-log-provider-transition-preflight-2026-05-08
scope: windows-event-log-provider-transition-preflight
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
tool: packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
event_log_provider_transition: blocked-by-no-mutation-preflight
event_log_provider_mutation: not-run
event_log_write_status: not-run

## 요약

이 slice는 ADR-0005의 default Windows Event Log writer/provider transition row를 실제 provider mutation 전 plan-only preflight로 고정한다. `New-PcvWindowsEventLogProviderTransitionPreflight.ps1`는 서비스명, provider name, log name, 현재 JSONL-first/Event Log opt-in writer policy, 목표 default Windows Event Log provider writer, 전환 check 목록을 `summary.json`과 Windows Event Log provider transition plan preview에 기록한다.

이 도구는 provider registration/removal, event write/query, default writer switch, service/MSI/firewall/trust-store/LAN/update mutation, public trusted signing, external stable publication을 실행하거나 주장하지 않는다. 실제 provider 전환 구현, removal/repair policy, log volume guard evidence가 닫히기 전까지 `event_log_provider_transition: blocked-by-no-mutation-preflight`, `event_log_provider_mutation: not-run`, `event_log_write_status: not-run`, `actual_execution: not-run`, `host_mutation_performed: false`를 유지한다.

## Dry-run Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1 -ArtifactRoot 'artifacts/windows-event-log-provider-transition-preflight-20260508-dryrun' -ServiceName 'PureCVisorDesktopNode' -ProviderName 'PureCVisor Desktop Node' -LogName 'Application' -CurrentWriter 'jsonl-first-eventlog-opt-in' -PlanOnly
```

## Contract

```text
scope: windows-event-log-provider-transition-preflight
actual_execution: not-run
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
event_log_provider_transition: blocked-by-no-mutation-preflight
event_log_provider_mutation: not-run
event_log_write_status: not-run
transition_checks:
  service-name-present
  provider-name-present
  log-name-present
  current-writer-recorded
  target-writer-recorded
  provider-registration-not-executed
  provider-removal-not-executed
  event-write-not-executed
  retention-volume-guard-required
  host-mutation-not-executed
```

## 검증

RED:

- `packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1`는 `New-PcvWindowsEventLogProviderTransitionPreflight.ps1` 부재로 실패했다.
- `archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1`는 Windows Event Log provider transition preflight evidence와 matrix linkage 부재로 실패했다.

GREEN:

- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1' -Output Detailed`
- Result: PASS, 6 tests.
- Dry-run artifact root: `artifacts/windows-event-log-provider-transition-preflight-20260508-dryrun`
- Dry-run summary: `ok=true`, `scope=windows-event-log-provider-transition-preflight`, `actual_execution=not-run`, `host_mutation_performed=false`, `event_log_provider_transition=blocked-by-no-mutation-preflight`, `event_log_provider_mutation=not-run`, `event_log_write_status=not-run`.

이 GREEN은 provider transition plan preview와 blocker descriptor만 확인한다. Provider registration/removal, event write/query, default writer switch, host mutation, public trusted signing, external stable publication은 수행하지 않았다.
