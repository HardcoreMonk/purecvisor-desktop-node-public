using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class Pcv04278PromotionEvidenceContractTests
{
    [Fact]
    public void PinsCanonicalCurrentTo04278()
    {
        D2EvidenceContractVerifier.Verify04278Current();
    }
}
