# Windows Credential Manager Default Transition Installed Evidence - 2026-05-10 0.39.5

evidence_id: windows-credential-manager-default-transition-installed-2026-05-10-0395
scope: windows-credential-manager-default-transition-installed
version: 0.39.5-admin-smoke
artifact_root: artifacts/windows-credential-manager-default-transition-installed-20260510-0395
summary: artifacts/windows-credential-manager-default-transition-installed-20260510-0395/summary.json
actual_execution: installed-msi-local-system-custom-action
host_mutation_performed: true
credential_manager_transition: installed-local-system-default-transition-pass
credential_manager_system_context_proof: installed-local-system-proof-pass
service_credential_manager_default_transition: installed-admin-smoke-pass
credential_manager_mutation: local-system-write-read-delete-and-protected-file-migration
token_source_migration: protected-file-to-credential-manager
service_reload_status: restarted
old_source_rejection_status: protected-file-source-rejected-after-reload
rollback_diagnostics_status: written
token_value_observed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Command

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1 -ArtifactRoot artifacts/windows-credential-manager-default-transition-installed-20260510-0395 -Version 0.39.5-admin-smoke
```

## Result

`Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1` built the current AllowUnsignedDev MSI, installed it through `msiexec`, and exercised the MSI deferred `CredentialManagerDefaultTransition` custom action with `Impersonate="no"`. The action ran as `NT AUTHORITY\SYSTEM`, performed the native Credential Manager write/read/delete proof, migrated the service bearer token source from the retained DPAPI protected token file into the `PureCVisor/PureCVisorDesktopNode/api-token` generic credential target, reconfigured the installed service, restarted it, and wrote rollback diagnostics.

Key summary fields:

| Field | Value |
|---|---|
| MSI SHA-256 | `5c63ec3b5246673457b6c8bb23d2f484522f0f4b9c6336308151289e81c557ab` |
| Provenance commit | `039e24086292e394c0061593c91e5768fb810450` |
| Signing mode | `AllowUnsignedDev` |
| Identity | `NT AUTHORITY\SYSTEM` |
| Final service | `PureCVisorDesktopNode` `Running`, `LocalSystem` |
| Runtime token storage | `windows-credential-manager` |
| Runtime policy health | HTTP `200` |
| Service token source | `--api-token-credential-target "PureCVisor/PureCVisorDesktopNode/api-token"` |
| Old service source | `--api-token-protected-file` absent from final SCM PathName |

Copied evidence files:

- `artifacts/windows-credential-manager-default-transition-installed-20260510-0395/credential-manager-transition.json`
- `artifacts/windows-credential-manager-default-transition-installed-20260510-0395/credential-manager-transition.rollback.json`

This is internal installed admin-smoke evidence. It does not claim public trusted signing, timestamp evidence, external stable publication, winget submission, or clean-host public signed install/update/rollback.
