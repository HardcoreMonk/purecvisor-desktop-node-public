using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvManualAdminCampaignDescriptorContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.manual-admin-campaign-descriptor.001",
        "packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1",
        1,
        "writes a plan-only descriptor that ties manual-admin runner evidence together")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("manual-admin-campaign-descriptor", 1);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-campaign-descriptor.002",
        "packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1",
        2,
        "blocks the descriptor when a required evidence summary is missing")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("manual-admin-campaign-descriptor", 2);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-campaign-descriptor.003",
        "packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1",
        3,
        "records the post-04218 next product payload trigger for the 0.42.19 descriptor candidate")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("manual-admin-campaign-descriptor", 3);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-campaign-descriptor.004",
        "packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1",
        4,
        "records the post-04220 next product payload trigger for the next descriptor candidate")]
    public void Contract004() =>
        ManualAdminContractVerifier.Verify("manual-admin-campaign-descriptor", 4);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-campaign-descriptor.005",
        "packaging/windows-desktop-node/tests/PcvManualAdminCampaignDescriptor.Tests.ps1",
        5,
        "requires plan-only mode and contains no host mutation commands")]
    public void Contract005() =>
        ManualAdminContractVerifier.Verify("manual-admin-campaign-descriptor", 5);
}
