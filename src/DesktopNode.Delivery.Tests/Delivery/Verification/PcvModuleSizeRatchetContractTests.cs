using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvModuleSizeRatchetContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.module-size-ratchet.001",
        "packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1",
        1,
        "declares a well-formed ratchet contract")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("module-ratchet", 1);

    [PcvLegacyContract(
        "pcv.delivery.module-size-ratchet.002",
        "packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1",
        2,
        "keeps every ratcheted module at or below its recorded ceiling")]
    public void Contract002() =>
        DevelopmentPolicyContractVerifier.Verify("module-ratchet", 2);

    [PcvLegacyContract(
        "pcv.delivery.module-size-ratchet.003",
        "packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1",
        3,
        "requires the ceiling to be tightened once a module actually shrinks")]
    public void Contract003() =>
        DevelopmentPolicyContractVerifier.Verify("module-ratchet", 3);
}
