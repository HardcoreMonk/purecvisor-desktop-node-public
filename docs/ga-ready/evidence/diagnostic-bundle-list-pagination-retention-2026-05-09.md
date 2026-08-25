# Diagnostic Bundle List Pagination/Retention Evidence - 2026-05-09

evidence_id: diagnostic-bundle-list-pagination-retention-20260509
scope: ADR-0005 diagnostic bundle list pagination and retention hardening
status: PASS
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

`DesktopNodeApiRequestProcessor` now exposes read-only `GET /api/v1/diagnostics/bundles?limit=&offset=` for retained diagnostic bundles. The route applies the configured retention policy before listing, returns latest-first rows, and includes `count`, `returned`, `limit`, `offset`, `next_offset`, `max_limit`, and retention metadata.

The Web Console `Troubleshooting` diagnostic bundle panel now lists retained bundles, shows `max_bundle_count`/`retention_days`, and exposes a disabled/enabled `Load more bundles` control from `next_offset`. This is a read-only UX/API hardening slice; it does not create bundles, mutate service config, rotate tokens, or change host state.

## Evidence

- API tests: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiDiagnosticBundleRequestProcessorTests|FullyQualifiedName~ApiHandlerAdapterContractTests"` PASS.
- Web static guard: `Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed` PASS.
- Browser fixture: `npm run browser:fixture` under `web/` PASS.

## Boundary

This evidence updates diagnostic bundle pagination/retention UI and API behavior only. Installed listener create/download evidence remains owned by `msi-service-installed-listener-rerun-2026-05-08-0390`; public trusted signing and external stable publication are not claimed.
