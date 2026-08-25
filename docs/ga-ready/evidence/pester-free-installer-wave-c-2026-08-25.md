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
| `PcvDesktopNodeInstaller.InternalTrust.Tests.ps1` | 4 | `PcvDesktopNodeInstallerInternalTrustContractTests.cs` | not-run | not-run | pending |
| `PcvDesktopNodeInstaller.Lifecycle.Tests.ps1` | 5 | `PcvDesktopNodeInstallerLifecycleContractTests.cs` | not-run | not-run | pending |
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
