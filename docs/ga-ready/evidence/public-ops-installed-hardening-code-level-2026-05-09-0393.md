# Public Ops Installed Hardening Code-Level Evidence - 2026-05-09 0.39.3

evidence_id: public-ops-installed-hardening-code-level-2026-05-09-0393
scope: public-ops-installed-hardening-code-level
status: PARTIAL_CODE_LEVEL_PASS_WITH_EXTERNAL_AND_INSTALLED_SMOKE_BLOCKERS
version: 0.39.3
actual_execution: code-level-tests-executed
host_mutation_performed: false
public_release: not-claimed
public_trusted_signing: not-claimed
external_stable_publication: blocked-by-missing-upload-endpoint-and-credential
catalog_publication: not-uploaded
winget_submission: blocked-by-missing-public-installer-url-or-submission-token
clean_host_public_signed_install_update_rollback_smoke: blocked-by-missing-clean-host-runner-or-public-publication
credential_manager_system_context_proof_runner: code-level-native-service-action
credential_manager_system_context_proof: system-context-proof-runner-code-level-applied-system-execution-pending
service_credential_manager_default_transition: system-proof-runner-code-level-applied-service-default-transition-pending
event_log_hardening: partial-code-level-repair-write-volume-guard-default-writer-pending
event_log_repair_status: code-level-native-service-action
event_log_write_status: code-level-native-service-action
event_log_volume_guard_status: code-level-native-service-action
event_log_default_writer: pending
tls_certificate_lifecycle: partial-code-level-cert-generate-rotate-delete-pass
tls_binding: not-run
tls_trust_boundary: pending

## Summary

This slice adds code-level native service actions for the remaining installed-ops hardening path that can proceed without public release credentials:

- `DesktopNode.Host.exe service-action credential-manager-system-proof` runs a Windows Credential Manager generic credential write/read/delete proof through Advapi32 and only passes when the executing identity is `NT AUTHORITY\SYSTEM`. It records token redaction, target name, lifecycle statuses, and `public_trusted_signing`/`external_stable_publication` as `not-claimed`.
- `DesktopNode.Host.exe service-action eventlog-repair` rewrites the owned `PureCVisor Desktop Node` provider binding to the installed `DesktopNode.Host.exe`.
- `DesktopNode.Host.exe service-action eventlog-write-test` writes the provider hardening event id `39100` only after the provider exists and is owned.
- `DesktopNode.Host.exe service-action eventlog-volume-guard` reads the Windows Event Log retention/max-size policy and reports whether the Application log has a bounded overwrite policy.

The focused and full Host tests passed. This evidence does not run an elevated installed smoke, does not change the service default token source to Windows Credential Manager, does not enable a default Windows Event Log writer, and does not bind HTTPS.

## Current Scope Note

This is a historical code-level evidence snapshot. Later internal-only installed evidence closes the Windows Credential Manager service default transition in `docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md`, the Windows Event Log default writer/provider hardening in `docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md`, and the internal HTTPS/TLS lifecycle in `docs/ga-ready/evidence/internal-https-tls-lifecycle-installed-2026-05-10-0397.md`. Public trusted signing, external stable publication, winget submission, and public clean-host signed install/update/rollback remain out-of-scope.

## Verification

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj
```

Result: PASS, 73 tests.

## Remaining Gates

| gate | current state | next required evidence |
|------|---------------|------------------------|
| external-stable-publication-catalog-upload | blocked-by-missing-upload-endpoint-and-credential | upload endpoint, credential, externally reachable catalog/package URL, immutable SHA-256 binding, upload audit |
| winget-submission | blocked-by-missing-public-installer-url-or-submission-token | public signed stable installer URL, manifest validation, submission reference |
| clean-host-public-signed-install-update-rollback | blocked-by-missing-clean-host-runner-or-public-publication | clean-host runner, public signed installer, public catalog, update, rollback, final service health |
| Windows Credential Manager service default transition | superseded-by-installed-pass | closed by `windows-credential-manager-default-transition-installed-2026-05-10-0395` |
| built-in TLS lifecycle | superseded-by-internal-https-pass | closed by `internal-https-tls-lifecycle-installed-2026-05-10-0397` |
| Windows Event Log provider hardening | superseded-by-installed-pass | closed by `windows-event-log-default-transition-installed-2026-05-10-0396` |

## Boundary

Public trusted signing, timestamp evidence, external stable publication/catalog upload, winget submission, and clean-host public signed install/update/rollback are still blocked by external release inputs. This code-level evidence keeps `host_mutation_performed: false` and does not supersede the installed admin-smoke evidence rows.
