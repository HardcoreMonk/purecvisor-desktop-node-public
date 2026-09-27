namespace DesktopNode.Contracts;

public static class VmPowerStates
{
    public const string Off = "Off";
    public const string InventoryStopped = "stopped";

    // Hyper-V inventory reports an Off VM as "stopped" (EnabledState 3); API callers also send "Off".
    public static bool IsOff(string? powerState)
    {
        var power = powerState?.Trim();
        return string.Equals(power, Off, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(power, InventoryStopped, StringComparison.OrdinalIgnoreCase);
    }
}
