# Service Token Rotation/Revoke Installed Evidence - 2026-05-09

evidence_id: service-token-rotation-revoke-installed-20260509
scope: ADR-0005 service token rotate/revoke mutation
status: PASS
artifact_root: artifacts/service-token-rotation-revoke-installed-20260509-150334
host_mutation_performed: true
service_name: PureCVisorDesktopNode
final_service_status: Running
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

`DesktopNode.Host.exe service-action service-token-rotation-revoke` now performs an actual installed service token mutation path:

- reads only token metadata/hash from the existing DPAPI LocalMachine protected token file;
- generates a new protected token without returning token values;
- writes a backup under `%ProgramData%\PureCVisor\desktop-node\backups\service-token-rotation`;
- atomically replaces `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`;
- restarts `PureCVisorDesktopNode`;
- verifies old bearer rejection and new bearer acceptance through `GET /api/v1/runtime/policy`;
- appends a redacted audit record to `%ProgramData%\PureCVisor\desktop-node\service-token-rotation.audit.jsonl`.

The installed elevated smoke result:

- `ok=true`
- `service_token_mutation=performed`
- `token_value_observed=false`
- `new_token_value_created=true`
- `service_reload_status=restarted`
- `old_token_rejection_status=old-token-rejected-after-reload`
- `token_rotation_audit_status=written`
- `old_token_status_before=200`
- `old_token_status_after=403`
- `new_token_status_after=200`
- `token_hash_changed=true`

## Artifact

Canonical artifact root:

```text
artifacts/service-token-rotation-revoke-installed-20260509-150334
```

Important files:

- `summary.json`
- `service-token-rotation-revoke.stdout.json`
- `service-token-rotation-revoke.stderr.txt`

Artifact scan found no `Bearer`, `Authorization`, `protected_token`, or raw token output in the captured files.

## Verification

- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostServiceActionTests.ServiceTokenRotationRevoke"` PASS after RED failure.
- `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostServiceActionTests"` PASS.

## Boundary

This is internal installed-admin operational mutation evidence for the current local service. It is not public trusted signing evidence, not external stable publication evidence, and not clean-host public signed update/rollback evidence.
