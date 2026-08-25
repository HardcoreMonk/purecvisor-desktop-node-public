# Windows Event Log Provider Default Transition Evidence - 2026-05-09 0.39.1

evidence_id: windows-event-log-provider-default-transition-2026-05-09-0391
scope: windows-event-log-provider-default-transition-installed-smoke
status: PASS
artifact_root: artifacts/windows-event-log-provider-default-transition-20260509-0391
summary: artifacts/windows-event-log-provider-default-transition-20260509-0391/summary.json
actual_execution: eventlog-provider-register-and-write-executed
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
service_name: PureCVisorDesktopNode
final_service_status: Running
event_log_provider_transition: installed-provider-register-write-pass
event_log_provider_mutation: registered
event_log_write_status: write-query-pass

## Summary

The installed `DesktopNode.Host.exe service-action eventlog-register` path was executed against the local host and the provider was left present as the default provider transition evidence.

- Event source: `PureCVisor Desktop Node`
- Log name: `Application`
- Event id written and queried: `39100`
- Register exit code: `0`
- EventMessageFile: `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`
- Provider final state: `present`
- Final service: `Running`

An earlier quoting mistake during the smoke created a .NET default source binding; that source was removed before the corrected native registration rerun. The canonical artifact records the corrected native provider binding.

## Boundary

This is internal installed host mutation evidence for provider registration and write/query behavior. It does not claim public trusted signing or external stable publication.
