using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvPublicDistributionDescriptorContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.public-distribution-descriptor.001",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",
        1,
        "creates a no-mutation dry-run summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("public-distribution-descriptor", 1);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-descriptor.002",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",
        2,
        "records the exact public distribution and operations gate names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("public-distribution-descriptor", 2);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-descriptor.003",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",
        3,
        "keeps public release claims explicitly unclaimed")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("public-distribution-descriptor", 3);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-descriptor.004",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",
        4,
        "records required inputs before any public publication claim")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("public-distribution-descriptor", 4);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-descriptor.005",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",
        5,
        "records operations expansion gates separately from publication gates")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("public-distribution-descriptor", 5);

    [PcvLegacyContract(
        "pcv.delivery.public-distribution-descriptor.006",
        "packaging/windows-desktop-node/tests/PcvPublicDistributionDescriptor.Tests.ps1",
        6,
        "does not contain host mutation or publication command text")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("public-distribution-descriptor", 6);
}
