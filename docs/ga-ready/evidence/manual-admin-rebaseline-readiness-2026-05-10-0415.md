# Manual Admin Rebaseline Readiness - 2026-05-10 0.41.5

evidence_id: manual-admin-rebaseline-readiness-2026-05-10-0415
status: pass
scope: manual-admin-rebaseline-readiness
actual_execution: local-readiness-descriptor-written
host_mutation_performed: false
version: 0.41.5-admin-smoke
installed_version: 0.41.5-admin-smoke
requested_0412_status: blocked-by-installed-version-mismatch
artifact_root: artifacts/manual-admin-rebaseline-readiness-20260510-0415
requested_0412_artifact_root: artifacts/manual-admin-rebaseline-readiness-20260510-0412
current_package_artifact_root: artifacts/manual-admin-campaign-20260510-192701-0413/windows-event-log-default-transition-installed
current_msi_sha256: 3458c95cc67b8a8540cd10029e8b88f2d618159225fb6b8d76748bd06d922ae5
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Scope

This evidence records a non-mutating readiness descriptor for the manual-admin campaign buckets that should not reuse historical `0.39.x` or `0.38.x` lifecycle defaults on the current installed node.

The first requested `0.41.2-admin-smoke` readiness descriptor was intentionally marked `blocked-by-installed-version-mismatch` because the installed product manifest had already advanced to `0.41.5-admin-smoke`.

## Verified Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 -ArtifactRoot artifacts/manual-admin-rebaseline-readiness-20260510-0412 -Version 0.41.2-admin-smoke -InstalledManifestPath 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json' -RouteParityArtifactRoot artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412 -PlanOnly

pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 -ArtifactRoot artifacts/manual-admin-rebaseline-readiness-20260510-0415 -Version 0.41.5-admin-smoke -InstalledManifestPath 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json' -RouteParityArtifactRoot artifacts/manual-admin-campaign-20260510-192701-0413/windows-event-log-default-transition-installed -PlanOnly
```

## Observed Result

- `0.41.2-admin-smoke` requested readiness: `ok=true`, `installed_version=0.41.5-admin-smoke`, `installed_version_matches_requested=false`, `requested_version_status=blocked-by-installed-version-mismatch`.
- `0.41.5-admin-smoke` current readiness: `ok=true`, `installed_version_matches_requested=true`, `requested_version_status=matches-installed-version`.
- Current package inputs exist for `0.41.5-admin-smoke`: MSI, MSI SHA-256 sidecar, publication descriptor, and payload root.
- Credential Manager and Event Log buckets are ready only with explicit `-Version 0.41.5-admin-smoke` and manual operator opt-in.
- Burn/MSIX/MSI remains `requires-current-lifecycle-runner-generation`.
- update/rollback remains `requires-current-baseline-target-package-pair`.
- clean-host remains `requires-dedicated-host-current-package-pair`.
- Historical direct defaults remain blocked until current-version rebaseline or dedicated clean-host package selection.

## Boundary

This is local readiness evidence only. It did not install, update, roll back, repair, remove, restart the service, mutate Credential Manager, mutate Event Log, bind TLS, rotate service tokens, build Burn/MSIX packages, run a clean host, or perform host mutation.

It is not public trusted signing, timestamp, external stable publication, winget submission, public stable installer URL, or public signed clean-host evidence.
