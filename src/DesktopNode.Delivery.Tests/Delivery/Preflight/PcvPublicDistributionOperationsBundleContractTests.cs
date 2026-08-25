using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvPublicDistributionOperationsBundleContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.public-distribution-operations-bundle.001",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",
        1,
        "executes and collects the non-mutating public distribution operations preflight bundle")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("public-distribution-operations-bundle", 1);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-operations-bundle.002",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",
        2,
        "records all requested distribution and operations component steps")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("public-distribution-operations-bundle", 2);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-operations-bundle.003",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",
        3,
        "preserves the requested legacy follow-up branches without deletion")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("public-distribution-operations-bundle", 3);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-operations-bundle.004",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",
        4,
        "keeps real public release, credential, event log, TLS, token, and clean-host mutation blocked")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("public-distribution-operations-bundle", 4);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-operations-bundle.005",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",
        5,
        "requires an explicit local descriptor write opt-in")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("public-distribution-operations-bundle", 5);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-operations-bundle.006",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionOperationsBundle.Tests.ps1",
        6,
        "does not contain host mutation, external submission, or public publication command text")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("public-distribution-operations-bundle", 6);
}

