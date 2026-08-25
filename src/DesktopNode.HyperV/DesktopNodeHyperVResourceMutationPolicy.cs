namespace DesktopNode.HyperV;

internal static class DesktopNodeHyperVResourceMutationPolicy
{
    internal static ulong KbpsToBitsPerSecond(int value)
    {
        return checked((ulong)value * 1_000UL);
    }

    internal static ulong BitsPerSecondToKbps(ulong value)
    {
        return value / 1_000UL;
    }
}
