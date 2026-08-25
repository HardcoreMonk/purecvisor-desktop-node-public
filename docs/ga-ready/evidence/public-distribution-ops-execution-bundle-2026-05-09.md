# Public Distribution Ops Execution Bundle - 2026-05-09

evidence_id: public-distribution-ops-execution-bundle-2026-05-09
artifact_root: artifacts/public-distribution-ops-execution-bundle-20260509-0391
runner: packaging/windows-desktop-node/tools/New-PcvPublicDistributionOperationsBundle.ps1
version: 0.39.1
scope: public-distribution-ops-execution-bundle
public_distribution_ops_execution_bundle: code-level-nonmutating-bundle-pass
actual_execution: local-preflight-bundle-executed
host_mutation_performed: false
mutates_host: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Summary

`New-PcvPublicDistributionOperationsBundle.ps1` executed the ADR-0005 public distribution and operations preflight generators into one local artifact tree. The bundle wrote:

- `summary.json`
- `execution-bundle.json`
- `follow-up-work-items.json`
- per-component `components/*/summary.json` descriptors

This is not public trusted signing evidence, external stable publication evidence, clean-host public update/rollback evidence, or host mutation evidence.

## Component Steps

The bundle collected these non-mutating component summaries:

- `public-distribution-descriptor`
- `public-distribution-readiness`
- `burn-bootstrapper-preflight`
- `msix-packaging-feasibility-preflight`
- `winget-manifest-compliance-preflight`
- `updater-catalog-publication-preflight`
- `public-signed-update-rollback-smoke-preflight`
- `windows-credential-manager-transition-preflight`
- `windows-event-log-provider-transition-preflight`
- `builtin-tls-certificate-lifecycle-preflight`
- `service-token-rotation-revoke-preflight`
- `timeout-rate-limit-hardening-preflight`
- `diagnostic-bundle-server-preflight`

## Preserved Branches

The following older follow-up branches were intentionally preserved, not deleted:

- `codex/diagnostic-bundle-api-action`
- `codex/diagnostic-bundle-listener-evidence`
- `codex/diagnostic-bundle-product-wrapper-evidence`
- `codex/full-admin-host-mutation-0389-evidence`

## Boundary

The bundle keeps the real public release and host-mutating statuses unchanged:

- `catalog_publication: not-published`
- `winget_submission: not-submitted`
- `public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication`
- `clean_host_smoke_status: not-run`
- `credential_manager_mutation: not-run`
- `event_log_provider_mutation: not-run`
- `tls_certificate_mutation: not-run`
- `service_token_mutation: not-run`
- `timeout_rate_limit_hardening: blocked-by-no-mutation-preflight`

## Exact Command

```powershell
.\packaging\windows-desktop-node\tools\New-PcvPublicDistributionOperationsBundle.ps1 `
  -ArtifactRoot artifacts/public-distribution-ops-execution-bundle-20260509-0391 `
  -Version '0.39.1' `
  -InstallerUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.1-windows-x64.msi' `
  -UpdatePackageUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.1-windows-x64.update.zip' `
  -PublicCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' `
  -MsiSha256 '19b93e72f567e1d5598c7998da2385edde574732284c3ff82a1a5954857f915d' `
  -UpdatePackageSha256 'd1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5' `
  -ServiceName 'PureCVisorDesktopNode' `
  -ProtectedTokenPath '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json' `
  -DiagnosticsRoot '%ProgramData%\PureCVisor\desktop-node\diagnostics' `
  -PreserveBranch 'codex/diagnostic-bundle-api-action,codex/diagnostic-bundle-listener-evidence,codex/diagnostic-bundle-product-wrapper-evidence,codex/full-admin-host-mutation-0389-evidence' `
  -AllowLocalDescriptorWrite
```

## Verification

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1 -Output Detailed
```

Result: PASS, 6/6.
