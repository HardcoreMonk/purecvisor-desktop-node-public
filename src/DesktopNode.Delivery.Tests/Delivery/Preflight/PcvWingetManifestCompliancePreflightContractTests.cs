using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Delivery.Reconciliation;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvWingetManifestCompliancePreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.001",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        1,
        "creates a non-mutating winget manifest compliance summary")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.002",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        2,
        "records the exact winget compliance check names")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.003",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        3,
        "writes normalized manifest metadata without running winget validation")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.004",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.005",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        5,
        "rejects a non-HTTPS installer URL")]
    public void Contract005() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.006",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        6,
        "rejects an invalid installer SHA-256")]
    public void Contract006() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 6);

    [PcvLegacyContract(
        "pcv.delivery.winget-manifest-compliance-preflight.007",
        "packaging/windows-desktop-node/tests/PcvWingetManifestCompliancePreflight.Tests.ps1",
        7,
        "does not contain host mutation or winget CLI execution command text")]
    public void Contract007() =>
        ReconciliationContractVerifier.Verify("winget-manifest-compliance-preflight", 7);
}
