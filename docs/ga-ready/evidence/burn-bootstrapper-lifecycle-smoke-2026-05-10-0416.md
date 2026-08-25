# Burn Bootstrapper Lifecycle Smoke Evidence - 2026-05-10 0.41.6

evidence_id: burn-bootstrapper-lifecycle-smoke-2026-05-10-0416
scope: burn-bootstrapper-actual-lifecycle-smoke
status: PASS
artifact_root: artifacts/burn-bootstrapper-lifecycle-20260510-0416
summary: artifacts/burn-bootstrapper-lifecycle-20260510-0416/summary.json
actual_execution: burn-build-install-repair-remove-executed
host_mutation_performed: true
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
burn_bootstrapper: build-install-repair-remove-pass-internal-smoke
signing_mode: AllowUnsignedDev

## Summary

The WiX Burn bootstrapper was rebuilt for the current `0.41.6-admin-smoke` package pair and exercised on the installed host.

- Bundle: `artifacts/burn-bootstrapper-lifecycle-20260510-0416/PureCVisorDesktopNode-0.41.6-admin-smoke-bootstrapper.exe`
- Bundle SHA-256: `5e67bd3a1fed7262447531000328825180fd678b252170793cf88e50fc41535d`
- Chained target MSI: `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416/target-0416/PureCVisorDesktopNode-0.41.6-admin-smoke-windows-x64.msi`
- Target MSI SHA-256: `967ac29bf2928f1fec3a0bb72425d15d2eda65a2466b1cb29dd9183bb18928a3`
- Restore baseline MSI: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415/PureCVisorDesktopNode-0.41.5-admin-smoke-windows-x64.msi`
- Restore baseline MSI SHA-256: `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`

Observed lifecycle:

- Build with WiX `5.0.2+aa65968c` and `WixToolset.BootstrapperApplications.wixext/5.0.2`: exit `0`
- `/quiet /norestart` install: exit `0`, service `Running`, product manifest `0.41.6-admin-smoke`
- `/repair /quiet /norestart`: exit `0`, service `Running`, product manifest `0.41.6-admin-smoke`
- `/uninstall /quiet /norestart`: exit `0`, service absent after remove
- Direct MSI restore to `0.41.5-admin-smoke`: exit `0`, final service `Running`, product manifest `0.41.5-admin-smoke`

## Boundary

This closes only the internal Burn build/install/repair/remove lifecycle smoke for the current `0.41.5-admin-smoke` to `0.41.6-admin-smoke` package pair. It does not claim public trusted signing, trusted timestamping, external stable publication, winget submission, or clean-host public signed update/rollback evidence.
