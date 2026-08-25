using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVResourceMutationPolicyTests
{
    [Theory]
    [InlineData(0, 0UL)]
    [InlineData(1, 1_000UL)]
    [InlineData(2_048, 2_048_000UL)]
    public void KbpsToBitsPerSecondUsesDecimalKilobits(int kbps, ulong expected)
    {
        Assert.Equal(expected, DesktopNodeHyperVResourceMutationPolicy.KbpsToBitsPerSecond(kbps));
    }

    [Theory]
    [InlineData(0UL, 0UL)]
    [InlineData(2_048_000UL, 2_048UL)]
    public void BitsPerSecondToKbpsReturnsEvidenceUnits(ulong bps, ulong expected)
    {
        Assert.Equal(expected, DesktopNodeHyperVResourceMutationPolicy.BitsPerSecondToKbps(bps));
    }
}
