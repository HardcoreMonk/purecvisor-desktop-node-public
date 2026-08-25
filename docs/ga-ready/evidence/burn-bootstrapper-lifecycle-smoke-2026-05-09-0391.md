# Burn Bootstrapper Lifecycle Smoke Evidence - 2026-05-09 0.39.1

evidence_id: burn-bootstrapper-lifecycle-smoke-2026-05-09-0391
scope: burn-bootstrapper-actual-lifecycle-smoke
status: PASS
artifact_root: artifacts/burn-bootstrapper-lifecycle-20260509-0391
summary: artifacts/burn-bootstrapper-lifecycle-20260509-0391/summary.json
actual_execution: burn-build-install-repair-remove-executed
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
burn_bootstrapper: build-install-repair-remove-pass-internal-smoke
signing_mode: AllowUnsignedDev

## Summary

The WiX Burn bootstrapper was built from the `0.39.1-admin-smoke` MSI payload and exercised on the installed host.

- Bundle: `artifacts/burn-bootstrapper-lifecycle-20260509-0391/PureCVisorDesktopNode-0.39.1-admin-smoke-bootstrapper.exe`
- Bundle SHA-256: `62df47314c659858c08f4cfe057a3323ab162b5cc36514af0db2dd82a0666946`
- Chained MSI source: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-130105-0391-frontend-final2/PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.msi`

Observed lifecycle:

- `/install /quiet /norestart`: exit `0`, service `Running`
- `/repair /quiet /norestart`: exit `0`, service `Running`
- `/uninstall /quiet /norestart`: exit `0`, service missing after remove
- direct MSI restore after Burn remove: exit `0`, final service `Running`

## Boundary

This closes only the internal Burn build/install/repair/remove lifecycle smoke for the current admin-smoke payload. It does not claim public trusted signing, trusted timestamping, external stable publication, winget submission, or clean-host public signed update/rollback evidence.
