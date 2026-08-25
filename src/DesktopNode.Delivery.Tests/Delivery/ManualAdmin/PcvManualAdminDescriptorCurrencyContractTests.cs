using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvManualAdminDescriptorCurrencyContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.manual-admin-descriptor-currency.001",
        "packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",
        1,
        "declares each current field exactly once at line start")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("manual-admin-descriptor-currency", 1);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-descriptor-currency.002",
        "packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",
        2,
        "agrees with the canonical manual-admin closure")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("manual-admin-descriptor-currency", 2);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-descriptor-currency.003",
        "packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",
        3,
        "agrees with the canonical anchor gate and installed evidence")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("manual-admin-descriptor-currency", 3);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-descriptor-currency.004",
        "packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",
        4,
        "agrees with the canonical anchor hashes and provenance")]
    public void Contract004() =>
        ManualAdminContractVerifier.Verify("manual-admin-descriptor-currency", 4);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-descriptor-currency.005",
        "packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",
        5,
        "keeps every referenced evidence document on disk")]
    public void Contract005() =>
        ManualAdminContractVerifier.Verify("manual-admin-descriptor-currency", 5);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-descriptor-currency.006",
        "packaging/windows-desktop-node/tests/PcvManualAdminDescriptorCurrency.Tests.ps1",
        6,
        "demotes rather than deletes the superseded 04259 values")]
    public void Contract006() =>
        ManualAdminContractVerifier.Verify("manual-admin-descriptor-currency", 6);
}

