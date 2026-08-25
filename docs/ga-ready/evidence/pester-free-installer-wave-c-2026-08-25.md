# Installer verification Wave C local parity evidence

## Current verdict

Installer Wave C local parity is PASS against the fixed clean code input below. All six legacy
Installer files and 49 contracts have one C# replacement owner and one strict-v2 manifest row;
replacement and one-time Pester 5.7.1 reference runs both passed `49/49` with failed, skipped, and
not-run counts of `0`. Required CI parity, branch-protection cutover, and Packaging Wave D remain
pending and are not claimed by this evidence.

evidence_input_head=0ab1bda71f3398aed302d53e7d6715987ce87b19
input_dirty_state=clean

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
| `PcvDesktopNodeInstaller.Plan.Tests.ps1` | 21 | `PcvDesktopNodeInstallerPlanContractTests.cs` | 21/21, failed 0, skipped 0, 687 ms | 21/21, failed 0, skipped 0, not-run 0, Pester 5734 ms | mapped / local pass / CI pending |
| `PcvDesktopNodeInstaller.Signing.Tests.ps1` | 6 | `PcvDesktopNodeInstallerSigningContractTests.cs` | 6/6, failed 0, skipped 0, 181 ms | 6/6, failed 0, skipped 0, not-run 0, Pester 2418 ms | mapped / local pass / CI pending |
| `PcvDesktopNodeInstaller.WixSource.Tests.ps1` | 10 | `PcvDesktopNodeInstallerWixSourceContractTests.cs` | 10/10, failed 0, skipped 0, 76 ms | 10/10, failed 0, skipped 0, not-run 0, Pester 1198 ms | mapped / local pass / CI pending |
| `PcvDesktopNodeInstaller.Wrapper.Tests.ps1` | 3 | `PcvDesktopNodeInstallerWrapperContractTests.cs` | 3/3, failed 0, skipped 0, 106 ms | 3/3, failed 0, skipped 0, not-run 0, Pester 4157 ms | mapped / local pass / CI pending |

## Verification commands

| Owner | Command | Result |
| --- | --- | --- |
| Ledger generator/verifier | `npm run check:verification-migration-manifest --prefix web` | PASS, files 62, contracts 627, missing/duplicate/order drift 0 |
| Ledger Node tests | `node --test web/node-tests/verification-migration-manifest.test.mjs` | PASS, 10/10, skipped 0 |
| Ledger .NET tests | `dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj --filter FullyQualifiedName~MigrationManifestV2Tests --nologo` | PASS, 12/12, skipped 0 |
| Installer replacement suite | `dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter Category=Installer --no-restore --nologo` | PASS, 49/49, skipped 0 |
| Delivery assembly | `dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --no-restore --nologo` | PASS, 96/96, skipped 0 |
| Installer legacy reference | Pester 5.7.1 over the six fixed Installer files | PASS, 49/49, failed/skipped/not-run 0, 13143 ms |
| Full solution | `dotnet test src/DesktopNode.sln -c Release --no-restore --nologo` | PASS, 1547/1547, skipped 0 |
| Web baseline and parity | `npm test --prefix web`; `npm run verify:parity --prefix web`; `npm run test:web-contracts --prefix web` | PASS; Web contracts 50/50, skipped 0 |
| Public source safety | `npm run test:public-source-safety --prefix web`; `npm run verify:public-source-safety --prefix web` | PASS, 20/20; finding count 0 |

## Completion verification

The replacement summary digest is over the exact UTF-8 compact JSON shown below. The legacy
digest is over the corresponding compact JSON emitted by the one-time Pester 5.7.1 reference
runner. Duration is evidence metadata and does not participate in any product-performance claim.

replacement_summary={"runner":"dotnet-test/xunit","total":49,"executed":49,"passed":49,"failed":0,"skipped":0,"duration_ms":1215.0,"result":"Completed"}
replacement_summary_sha256=1313465ec313aa7aae3d664ef7995cf70451bdcfdc4d0efe6dfa70ff6dccf7dd
legacy_summary={"pester_version":"5.7.1","total":49,"passed":49,"failed":0,"skipped":0,"not_run":0,"duration_ms":13143.0,"result":"Passed"}
legacy_summary_sha256=00a5c2cdcb21f54c0292f8db0b711e8f9bb15a48b14f16ad32d843f5d3a67eb2

manifest_files_total=62
manifest_contracts_total=627
manifest_web_mapped=50
manifest_web_local_pass=50
manifest_web_ci_pending=50
manifest_installer_mapped=49
manifest_installer_local_pass=49
manifest_installer_ci_pending=49
manifest_packaging_unmapped=528
manifest_missing=0
manifest_duplicate=0
manifest_order_drift=0

The fixed diff from the sanitized public seed through the evidence input contains 39 paths. Review
found no missing or duplicate Installer mapping, behavioral omission, P0/P1 defect, required
workflow edit, or product `ProjectReference`. Risk-token matches were enforcement blocklists,
negative tests, source locators, or non-executed lifecycle plan data. New replacement executable
invocation of PowerShell, Pester, MSI/service/VM mutation tools, or a shell is `0`.

fixed_diff_base=c76a831be168a6b5aa122a91df3588a0c5e67f0d
fixed_diff_head=0ab1bda71f3398aed302d53e7d6715987ce87b19
fixed_diff_path_count=39
fixed_diff_p0_count=0
fixed_diff_p1_count=0
installer_behavioral_omission_count=0
required_workflow_changed=false
new_replacement_executable_invocation_count=0
public_source_safety_finding_count=0
public_source_safety_report_sha256=603f64030f501eeb60d58859f377cd7ee6668f2ce1bb73ec1b95c4906d9eeebd

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

### Plan fixed-file run

The replacement fixture executed 21 contracts with exit `0`. A focused run including the three
required-property, WiX-ordering, and escaping-payload-root negative fixtures executed `24/24`.
The Pester 5.7.1 reference run executed only
`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`
and reported total `21`, passed `21`, failed `0`, skipped `0`, not run `0`, duration `5734 ms`, and
exit `0`. Both paths wrote only disposable fixture outputs; no MSI installation, service mutation,
administrator elevation, or VM operation occurred.

plan_replacement_result=pass
plan_replacement_total=21
plan_replacement_failed=0
plan_replacement_skipped=0
plan_negative_total=3
plan_negative_failed=0
plan_legacy_result=pass
plan_legacy_total=21
plan_legacy_failed=0
plan_legacy_skipped=0
plan_legacy_not_run=0

### Signing fixed-file run

The replacement fixture executed six contracts with exit `0`, and the SHA-256 downgrade negative
fixture was rejected. The Pester 5.7.1 reference run executed only
`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1`
and reported total `6`, passed `6`, failed `0`, skipped `0`, not run `0`, duration `2418 ms`, and
exit `0`. Certificate material was synthetic, secret values were redacted, and no certificate
store, real SignTool, administrator, service, MSI lifecycle, or VM operation was used.

signing_replacement_result=pass
signing_replacement_total=6
signing_replacement_failed=0
signing_replacement_skipped=0
signing_negative_total=1
signing_negative_failed=0
signing_legacy_result=pass
signing_legacy_total=6
signing_legacy_failed=0
signing_legacy_skipped=0
signing_legacy_not_run=0

### WixSource fixed-file run

The replacement fixture executed ten contracts with exit `0`. The missing-element,
duplicate-element, and wrong-namespace negative fixtures were all rejected. The Pester 5.7.1
reference run executed only
`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`
and reported total `10`, passed `10`, failed `0`, skipped `0`, not run `0`, duration `1198 ms`, and
exit `0`. Both paths parsed source files only; no WiX build, MSI, service, administrator, or VM
operation occurred.

wix_source_replacement_result=pass
wix_source_replacement_total=10
wix_source_replacement_failed=0
wix_source_replacement_skipped=0
wix_source_negative_total=3
wix_source_negative_failed=0
wix_source_legacy_result=pass
wix_source_legacy_total=10
wix_source_legacy_failed=0
wix_source_legacy_skipped=0
wix_source_legacy_not_run=0

### Wrapper fixed-file run

The replacement fixture executed three contracts with exit `0`. The exit-code-collapse and
administrator-elevation negative fixtures were both rejected. The Pester 5.7.1 reference run
executed only
`packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1`
and reported total `3`, passed `3`, failed `0`, skipped `0`, not run `0`, duration `4157 ms`, and
exit `0`. The replacement path validated source and projected independent argument-array values;
it did not start PowerShell, request elevation, build an MSI, mutate a service, or operate a VM.

wrapper_replacement_result=pass
wrapper_replacement_total=3
wrapper_replacement_failed=0
wrapper_replacement_skipped=0
wrapper_negative_total=2
wrapper_negative_failed=0
wrapper_legacy_result=pass
wrapper_legacy_total=3
wrapper_legacy_failed=0
wrapper_legacy_skipped=0
wrapper_legacy_not_run=0

## Claim boundary

installer_local_parity=true
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
