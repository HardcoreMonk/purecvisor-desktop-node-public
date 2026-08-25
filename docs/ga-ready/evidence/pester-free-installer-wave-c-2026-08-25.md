# Installer verification Wave C local parity evidence

## Current verdict

This document is the Wave C evidence skeleton. It does not yet claim Installer local parity or CI
parity. Evidence-capture command and result fields remain `not-run` until the corresponding
command has completed against the final fixed clean input. Required CI and branch-protection
cutover remain unchanged.

evidence_input_head=not-run
input_dirty_state=not-run

## Fixed migration inventory

| Domain | Legacy files | Legacy contracts | Starting state |
| --- | ---: | ---: | --- |
| Packaging | 55 | 528 | unmapped / local pending / CI pending |
| Installer | 6 | 49 | unmapped / local pending / CI pending |
| Web | 1 | 50 | mapped / local pass / CI pending |
| Total | 62 | 627 | mixed, non-cutover |

inventory_files_total=62
inventory_files_packaging=55
inventory_files_installer=6
inventory_files_web=1
inventory_contracts_total=627
inventory_contracts_packaging=528
inventory_contracts_installer=49
inventory_contracts_web=50

## Installer file ledger

| Legacy file | Contracts | Replacement owner | Replacement result | Legacy reference result | Local evidence state |
| --- | ---: | --- | --- | --- | --- |
| `PcvDesktopNodeInstaller.InternalTrust.Tests.ps1` | 4 | `PcvDesktopNodeInstallerInternalTrustContractTests.cs` | 4/4, failed 0, skipped 0, 47 ms | 4/4, failed 0, skipped 0, not-run 0, Pester 2428 ms | mapped / local pass / CI pending |
| `PcvDesktopNodeInstaller.Lifecycle.Tests.ps1` | 5 | `PcvDesktopNodeInstallerLifecycleContractTests.cs` | 5/5, failed 0, skipped 0, 49 ms | 5/5, failed 0, skipped 0, not-run 0, Pester 1100 ms | mapped / local pass / CI pending |
| `PcvDesktopNodeInstaller.Plan.Tests.ps1` | 21 | `PcvDesktopNodeInstallerPlanContractTests.cs` | not-run | not-run | pending |
| `PcvDesktopNodeInstaller.Signing.Tests.ps1` | 6 | `PcvDesktopNodeInstallerSigningContractTests.cs` | not-run | not-run | pending |
| `PcvDesktopNodeInstaller.WixSource.Tests.ps1` | 10 | `PcvDesktopNodeInstallerWixSourceContractTests.cs` | not-run | not-run | pending |
| `PcvDesktopNodeInstaller.Wrapper.Tests.ps1` | 3 | `PcvDesktopNodeInstallerWrapperContractTests.cs` | not-run | not-run | pending |

## Verification commands

| Owner | Command | Result |
| --- | --- | --- |
| Ledger generator/verifier | `npm run check:verification-migration-manifest --prefix web` | not-run |
| Ledger Node tests | `node --test web/node-tests/verification-migration-manifest.test.mjs` | not-run |
| Ledger .NET tests | `dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj --filter FullyQualifiedName~MigrationManifestV2Tests --nologo` | not-run |
| Installer replacement suite | `dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter Category=Installer --no-restore --nologo` | not-run |
| Installer legacy reference | Pester 5.7.1 over the six fixed Installer files | not-run |
| Full solution | `dotnet test src/DesktopNode.sln -c Release --no-restore --nologo` | not-run |

### InternalTrust fixed-file run

The replacement `Category=Installer` run executed four contracts with exit `0`. The Pester 5.7.1
reference run executed only
`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1`
and reported total `4`, passed `4`, failed `0`, skipped `0`, not run `0`, duration `2428 ms`, and
exit `0`. The replacement source verifier did not start PowerShell; the reference script's two
PowerShell child calls both used `-DryRun` and performed no certificate-store or host mutation.

internal_trust_replacement_result=pass
internal_trust_replacement_total=4
internal_trust_replacement_failed=0
internal_trust_replacement_skipped=0
internal_trust_legacy_result=pass
internal_trust_legacy_total=4
internal_trust_legacy_failed=0
internal_trust_legacy_skipped=0
internal_trust_legacy_not_run=0

### Lifecycle fixed-file run

The replacement fixture executed five contracts with exit `0`. Its two negative fixtures rejected
a missing Restart Manager suppression marker and an unconditional Repair-3010 success marker. The
Pester 5.7.1 reference run executed only
`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1`
and reported total `5`, passed `5`, failed `0`, skipped `0`, not run `0`, duration `1100 ms`, and
exit `0`. Both paths created only disposable test files; neither invoked MSI nor mutated the host.

lifecycle_replacement_result=pass
lifecycle_replacement_total=5
lifecycle_replacement_failed=0
lifecycle_replacement_skipped=0
lifecycle_legacy_result=pass
lifecycle_legacy_total=5
lifecycle_legacy_failed=0
lifecycle_legacy_skipped=0
lifecycle_legacy_not_run=0

## Claim boundary

installer_local_parity=false
installer_ci_parity=false
required_ci_pester_zero=false
required_ci_nonadmin_powershell_zero=false
cutover_completed=false
host_mutation_performed=false
msi_or_service_mutation=false
actual_vm_tested=false
public_trusted_signing=false
external_stable_publication=false
operational_current=0.42.74-admin-smoke
