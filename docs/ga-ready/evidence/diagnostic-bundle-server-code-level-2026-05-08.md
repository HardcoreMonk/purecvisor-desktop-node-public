# Diagnostic Bundle Server Code-Level Evidence - 2026-05-08

evidence_id: diagnostic-bundle-server-code-level-2026-05-08
scope: diagnostic-bundle-server-code-level
adr: ADR-0005
matrix: docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md
artifact_or_package_version: src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs, src/DesktopNode.Host/DesktopNodeHostOptions.cs, packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1
runner_version: ApiDiagnosticBundleRequestProcessorTests.cs, ApiHandlerAdapterContractTests.cs, DesktopNodeHostOptionsTests.cs, PcvDesktopNodeProduct.Plan.Tests.ps1
actual_execution: code-level-tests
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
diagnostic_bundle_server_generation: partial-code-level-api-action
diagnostic_bundle_api_action: code-level-applied
diagnostic_bundle_archive_created: code-level-created
diagnostic_bundle_download_served: code-level-download-served
diagnostic_bundle_redaction_status: code-level-applied
diagnostic_bundle_authz_status: token-required-route-contract
diagnostic_bundle_retention_status: code-level-applied

## Summary

`DesktopNodeApiRequestProcessor` now exposes a code-level Local API action for diagnostic bundle server-side generation. `POST /api/v1/diagnostics/bundles` writes a redacted `.bundle.json` artifact under the configured diagnostics root and returns bundle metadata, while `GET /api/v1/diagnostics/bundles/{bundle_id}/download` serves the saved artifact through the download route.

The product service plan wires the diagnostics root into `DesktopNode.Host.exe listen` with `--diagnostics-root "C:\ProgramData\PureCVisor\desktop-node\diagnostics"`. The route contract remains token-required, and the code-level test verifies that request body secrets such as `super-secret` are replaced with `[REDACTED]`, that `X-PCV-Diagnostic-Bundle-Id` is emitted on download, and that max-count retention deletes older bundle files.

## Evidence Contract

- `POST /api/v1/diagnostics/bundles` returns `actual_execution=code-level-api-action`, `archive_status=created`, `download_status=served-by-download-route`, `redaction_status=applied`, `authz_status=token-required-route-contract`, and `retention_status=applied`.
- `GET /api/v1/diagnostics/bundles/{bundle_id}/download` returns `application/vnd.purecvisor.diagnostic-bundle+json`, `Content-Disposition`, and `X-PCV-Diagnostic-Bundle-Id`.
- `ApiHandlerAdapterContract` lists `POST /api/v1/diagnostics/bundles` as `ProductOperation` with `TokenRequired` and `dotnet-runtime` ownership.
- `ApiHandlerAdapterContract` lists `GET /api/v1/diagnostics/bundles/{bundleId}/download` as `ReadOnly` with `dotnet-runtime` ownership.
- `PcvDesktopNodeProduct.psm1` includes `--diagnostics-root` in the generated product service binary path.

## Exclusions

This evidence does not start an installed listener, execute product wrapper `CollectDiagnostics` delegation, run elevated service/MSI/firewall/trust-store/LAN/update mutation, build a public signed artifact, publish an external stable package, or claim public trusted signing. Installed listener evidence, product diagnostics runner delegation, binary archive packaging, external download benchmarking, and public distribution publication remain separate follow-up gates.

## Verification

- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~DiagnosticBundleCreateWritesRedactedDownloadableArtifactAndAppliesRetention|FullyQualifiedName~DefaultContractMapsPhase25RouteCandidates|FullyQualifiedName~DefaultContractKeepsDotNetProductOwners|FullyQualifiedName~DefaultContractSeparatesReadOnlyAndQueuedMutationRoutes"`
- `dotnet test src\DesktopNode.Host.Tests\DesktopNode.Host.Tests.csproj --no-restore --filter "FullyQualifiedName~ListenOptionsParseLoopbackPrefixAndProtectedTokenFile"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"`

This is code-level product API evidence only. Public trusted signing and external stable publication remain `not-claimed`.
