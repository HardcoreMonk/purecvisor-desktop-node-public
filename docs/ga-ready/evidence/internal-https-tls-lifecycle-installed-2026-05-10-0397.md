# Internal HTTPS/TLS Lifecycle Installed Smoke - 2026-05-10 0397

```text
evidence_id: internal-https-tls-lifecycle-installed-2026-05-10-0397
artifact_root: artifacts/internal-https-tls-lifecycle-installed-20260510-0397
tool: packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1
scope: internal-https-tls-lifecycle-installed
actual_execution: installed-service-https-binding-cert-rotation-remove-smoke
ok: true
host_mutation_performed: true
https_prefix: https://127.0.0.1:7443/
ssl_ipport: 127.0.0.1:7443
certificate_lifecycle: generate-bind-rotate-remove-pass
https_initial_status: 200
https_rotated_status: 200
final_http_restore_status: 200
final_service: Running
path_name_restored: true
token_storage: windows-credential-manager
token_value_observed: false
private_key_material_recorded: false
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
```

## Result

Internal HTTPS/TLS lifecycle installed smoke passed against the installed `PureCVisorDesktopNode` service.

The runner created a temporary LocalMachine self-signed certificate, bound it to HTTP.sys on `127.0.0.1:7443`, temporarily reconfigured the installed service `--prefix` to `https://127.0.0.1:7443/`, and verified bearer-protected `GET /api/v1/runtime/policy` over HTTPS with HTTP `200`.

The smoke then rotated to a second certificate, replaced the HTTP.sys SSL binding, restarted the service, and verified HTTPS runtime policy again with HTTP `200`. Cleanup restored the original loopback HTTP service `PathName`, removed the SSL binding, removed both temporary certificates, and verified restored HTTP runtime policy with HTTP `200`.

## Boundary

This is internal private-network operational evidence. It does not claim public trusted signing, trusted timestamping, external stable publication, winget submission, public stable installer URL, or public clean-host signed install/update/rollback evidence.

