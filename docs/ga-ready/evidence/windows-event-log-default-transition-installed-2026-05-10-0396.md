# Windows Event Log Default Transition Installed Evidence - 2026-05-10 0.39.6

evidence_id: windows-event-log-default-transition-installed-2026-05-10-0396
scope: windows-event-log-default-transition-installed
version: 0.39.6-admin-smoke
artifact_root: artifacts/windows-event-log-default-transition-installed-20260510-0396
summary: artifacts/windows-event-log-default-transition-installed-20260510-0396/summary.json
actual_execution: installed-msi-local-system-custom-action
host_mutation_performed: true
event_log_default_transition: installed-admin-smoke-pass
event_log_hardening: installed-default-writer-repair-remove-volume-schema-pass
event_log_provider_transition: installed-provider-default-writer-pass
event_log_provider_mutation: registered
event_log_write_status: write-query-pass
event_log_default_writer: installed-admin-smoke-pass
event_log_repair_status: installed-provider-repair-pass
event_log_volume_guard_status: installed-volume-guard-pass
event_log_remove_repair_status: installed-provider-remove-restore-pass
event_log_schema_version: 1
event_id: 39101
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1 -ArtifactRoot artifacts/windows-event-log-default-transition-installed-20260510-0396 -Version 0.39.6-admin-smoke
```

## Result

`Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1` built the current AllowUnsignedDev MSI, installed it through `msiexec`, and exercised the MSI deferred `EventLogDefaultTransition` custom action with `Impersonate="no"`. The action ran the native `DesktopNode.Host.exe service-action eventlog-default-transition` path, repaired the owned provider binding, wrote a schema v1 default-writer event, checked the Application log volume policy, removed the provider, restored it, and wrote a redacted `eventlog-default-transition.json` descriptor under ProgramData.

Key summary fields:

| Field | Value |
|---|---|
| MSI SHA-256 | `180e3a6185bfcc47681f1e7a62afae8998efd05a7334df3f7b1dbf98f6f052fe` |
| Provenance commit | `8c661b864ab64b5df1596625a58b0bd9583f477f` |
| Signing mode | `AllowUnsignedDev` |
| Default writer status | `default-writer-pass` |
| Provider repair status | `provider-repair-pass` |
| Event write status | `write-query-pass` |
| Volume guard status | `volume-guard-pass` |
| Provider remove/restore | `provider-remove-pass`, final `provider-present` |
| Event id | `39101` |
| Event records found | `1` |
| Final service | `PureCVisorDesktopNode` `Running`, `LocalSystem` |
| Service Event Log args | `--event-log-provider-source`, `--event-log-writer`, `--event-log-schema-version` present |
| Runtime policy health | HTTP `200` |
| Token value observed | `false` |

Copied evidence file:

- `artifacts/windows-event-log-default-transition-installed-20260510-0396/eventlog-default-transition.json`

## Boundary

This is internal installed admin-smoke evidence. It does not claim public trusted signing, timestamp evidence, external stable publication, winget submission, or clean-host public signed install/update/rollback.
