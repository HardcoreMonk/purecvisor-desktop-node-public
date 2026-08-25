using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvDevelopmentVerificationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.development-verification.001",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        1,
        "selects dotnet for a source-only tier S change")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 1);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.002",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        2,
        "selects npm and Web Pester for a Web change")]
    public void Contract002() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 2);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.003",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        3,
        "selects the current evidence check for canonical evidence changes")]
    public void Contract003() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 3);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.004",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        4,
        "promotes an unknown path to Full")]
    public void Contract004() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 4);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.005",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        5,
        "promotes tier M to Full and tier L to Release")]
    public void Contract005() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 5);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.006",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        6,
        "keeps an internal single-module source change at S")]
    public void Contract006() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 6);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.007",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        7,
        "promotes API contract and general packaging changes to at least M")]
    public void Contract007() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 7);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.008",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        8,
        "forces L for installer host mutation security current evidence public and signing boundaries")]
    public void Contract008() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 8);

    [PcvLegacyContract(
        "pcv.delivery.development-verification.009",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerification.Tests.ps1",
        9,
        "uses the path-derived tier for lane promotion while unknown scope only forces Full")]
    public void Contract009() =>
        DevelopmentPolicyContractVerifier.Verify("verification-policy", 9);
}
