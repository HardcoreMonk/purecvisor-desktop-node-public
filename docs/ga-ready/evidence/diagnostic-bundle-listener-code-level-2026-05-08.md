# Diagnostic Bundle Host Listener Code-Level Evidence - 2026-05-08

evidence_id: diagnostic-bundle-listener-code-level-2026-05-08
scope: diagnostic-bundle-host-listener-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: src/DesktopNode.Host/DesktopNodeHostApplication.cs, src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs
runner_version: DesktopNodeHostApplicationTests.cs
actual_execution: code-level-host-listener-test
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
diagnostic_bundle_server_generation: partial-code-level-api-action
diagnostic_bundle_host_listener_execution: code-level-host-listener
diagnostic_bundle_installed_listener_execution: not-run
diagnostic_bundle_product_wrapper_delegation: not-run
diagnostic_bundle_api_action: code-level-applied
diagnostic_bundle_archive_created: code-level-created
diagnostic_bundle_download_served: code-level-download-served
diagnostic_bundle_redaction_status: code-level-applied
diagnostic_bundle_authz_status: token-required-route-contract
diagnostic_bundle_retention_status: code-level-applied
diagnostic_bundle_request_id_propagation: code-level-host-header

## Summary

`DesktopNodeHostApplication` now forwards `X-PCV-Request-Id` and `X-Request-Id` into `DesktopNodeApiRequestProcessor`. The diagnostic bundle create/download path was verified through the in-process `HttpListener` host with a configured bearer token file and diagnostics root.

The focused test starts `DesktopNodeHostApplication` on `http://127.0.0.1:0/`, configures `ApiTokenFile` and `DiagnosticsRootPath`, confirms unauthenticated `POST /api/v1/diagnostics/bundles` is rejected, then sends an authenticated create request with `X-PCV-Request-Id: listener-diag-create`. The response and saved `.bundle.json` preserve the listener request id, redact `super-secret` as `[REDACTED]`, and the authenticated download route returns `application/vnd.purecvisor.diagnostic-bundle+json` with `X-PCV-Diagnostic-Bundle-Id`.

## Evidence Contract

- `POST /api/v1/diagnostics/bundles` over `DesktopNodeHostApplication` requires bearer authorization when `ApiTokenFile` is configured.
- `X-PCV-Request-Id` is propagated from the host listener into the Local API response and saved diagnostic bundle.
- Diagnostic bundle archive output remains redacted and does not contain the bearer token or request body secret.
- `GET /api/v1/diagnostics/bundles/{bundle_id}/download` over the host listener serves the saved `.bundle.json`.

## Exclusions

This evidence does not install or restart the Windows service, run an MSI lifecycle, mutate firewall/trust-store/LAN state, execute product wrapper `CollectDiagnostics` delegation, publish an external artifact, or claim public trusted signing. Product diagnostics delegation remains a separate code-level gate. The later `0.39.0-admin-smoke` MSI/service rerun in `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md` owns the installed listener PASS evidence.

## Verification

- RED: `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~DiagnosticBundleRoutesWorkThroughTokenProtectedHostListener"` failed because the host listener generated a random `req-*` id instead of propagating `listener-diag-create`.
- GREEN: `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~DiagnosticBundleRoutesWorkThroughTokenProtectedHostListener"`

This is code-level host listener evidence only. Public trusted signing and external stable publication remain `not-claimed`.
