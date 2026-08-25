# Windows Credential Manager Transition Evidence - 2026-05-09 0.39.1

evidence_id: windows-credential-manager-transition-2026-05-09-0391
scope: windows-credential-manager-transition-capability-smoke
status: PARTIAL-BLOCKED
artifact_root: artifacts/windows-credential-manager-transition-20260509-0391
summary: artifacts/windows-credential-manager-transition-20260509-0391/summary.json
actual_execution: current-user-credential-write-read-delete-executed
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
credential_manager_transition: capability-pass-service-transition-blocked
credential_manager_mutation: current-user-smoke-write-read-delete
service_credential_manager_default_transition: blocked-by-service-account-context
token_value_observed: false

## Summary

The Windows Credential Manager OS capability was exercised through Advapi32 `CredWriteW`, `CredReadW`, and `CredDeleteW` under the current elevated user context.

- Temporary target prefix: `PureCVisor/DesktopNode/LocalApiToken-Smoke-*`
- Write status: `pass`
- Read status: `pass`
- Delete status: `pass`
- Installed service account: `LocalSystem`
- Token value captured in evidence: `false`
- Non-reversible SHA-256 fingerprint:
  `4f9ac8c26468ab075ae28e43205ceb892ae5e33d94f7e2e35e3607cfc3ef8969`

## Blocker

The installed service runs as `LocalSystem`. A current-user Credential Manager write/read/delete smoke does not prove that the service can resolve a token from the `LocalSystem` credential context. The product still needs host support for a credential target option and a SYSTEM-context migration runner before the DPAPI protected token file can be replaced as the default service token source.

## Boundary

This evidence is a capability smoke plus blocker record, not a completed product service transition. It does not claim public trusted signing, external stable publication, or service token storage migration completion.
