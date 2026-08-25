using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvPublicDistributionReadinessContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.public-distribution-readiness.001",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",
        1,
        "creates a non-mutating public distribution readiness summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("public-distribution-readiness", 1);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-readiness.002",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",
        2,
        "records the exact readiness gate names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("public-distribution-readiness", 2);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-readiness.003",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",
        3,
        "writes a winget singleton manifest preview with required package fields")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("public-distribution-readiness", 3);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-readiness.004",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",
        4,
        "keeps winget validation and submission as explicit manual follow-up")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("public-distribution-readiness", 4);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-readiness.005",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",
        5,
        "requires plan-only mode")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("public-distribution-readiness", 5);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-readiness.006",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionReadiness.Tests.ps1",
        6,
        "does not contain host mutation or publication submission command text")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("public-distribution-readiness", 6);
}

