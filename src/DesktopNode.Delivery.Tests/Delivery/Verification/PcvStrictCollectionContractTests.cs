using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvStrictCollectionContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.strict-collection.001",
        "packaging/windows-desktop-node/tests/PcvStrictCollection.Tests.ps1",
        1,
        "reproduces the if-assignment unwrap that broke clean-host residue Count readback")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("strict-collection", 1);

    [PcvLegacyContract(
        "pcv.delivery.strict-collection.002",
        "packaging/windows-desktop-node/tests/PcvStrictCollection.Tests.ps1",
        2,
        "returns Count 0/1/2 under StrictMode for missing, one-child, and two-child directories")]
    public void Contract002() =>
        DevelopmentPolicyContractVerifier.Verify("strict-collection", 2);
}
