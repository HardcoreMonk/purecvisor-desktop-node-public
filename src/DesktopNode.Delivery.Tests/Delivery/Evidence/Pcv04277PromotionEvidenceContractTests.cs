using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class Pcv04277PromotionEvidenceContractTests
{
    [Fact]
    public void PinsCanonicalCurrentTo04277()
    {
        D2EvidenceContractVerifier.Verify04277Current();
    }
}
