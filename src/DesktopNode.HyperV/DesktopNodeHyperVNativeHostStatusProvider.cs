using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVNativeHostStatusProvider(IDesktopNodeHyperVSwitchProvider switchProvider) : IDesktopNodeHyperVHostStatusProvider
{
    public DesktopNodeHyperVHostStatusData GetStatus(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reasons = new List<string>();
        var windows = GetWindowsInfo(reasons);
        var isSupportedEdition = windows.Edition is "Pro" or "Enterprise" or "Education";
        if (!isSupportedEdition)
        {
            reasons.Add("PCV_WINDOWS_EDITION_UNSUPPORTED");
        }

        var featureEnabled = GetHyperVFeatureEnabled(reasons);
        var vmmsRunning = GetVmmsRunning(reasons);
        var defaultSwitchPresent = GetDefaultSwitchPresent(reasons, cancellationToken);
        var isAdmin = GetAdminElevated(reasons);

        return new DesktopNodeHyperVHostStatusData(
            Supported: isSupportedEdition && featureEnabled && vmmsRunning && defaultSwitchPresent && isAdmin,
            Reasons: reasons,
            Windows: windows,
            Admin: new DesktopNodeHyperVHostAdminInfo(isAdmin),
            HyperV: new DesktopNodeHyperVHostHyperVInfo(featureEnabled, vmmsRunning, defaultSwitchPresent));
    }

    private static DesktopNodeHyperVHostWindowsInfo GetWindowsInfo(ICollection<string> reasons)
    {
        try
        {
            var caption = GetRegistryString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName") ??
                GetOperatingSystemProperty("Caption");
            var version = GetRegistryString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion") ??
                GetRegistryString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId") ??
                GetOperatingSystemProperty("Version");

            return new DesktopNodeHyperVHostWindowsInfo(
                Caption: caption,
                Version: version,
                Edition: GetWindowsEdition(caption));
        }
        catch
        {
            reasons.Add("PCV_WINDOWS_INFO_UNKNOWN");
            return new DesktopNodeHyperVHostWindowsInfo(null, null, "Unknown");
        }
    }

    private static string GetWindowsEdition(string? productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return "Unknown";
        }

        if (productName.Contains("Enterprise", StringComparison.OrdinalIgnoreCase))
        {
            return "Enterprise";
        }

        if (productName.Contains("Professional", StringComparison.OrdinalIgnoreCase) ||
            productName.Contains("Pro", StringComparison.OrdinalIgnoreCase))
        {
            return "Pro";
        }

        if (productName.Contains("Education", StringComparison.OrdinalIgnoreCase))
        {
            return "Education";
        }

        if (productName.Contains("Home", StringComparison.OrdinalIgnoreCase))
        {
            return "Home";
        }

        return "Unknown";
    }

    private static bool GetHyperVFeatureEnabled(ICollection<string> reasons)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                new ObjectQuery("SELECT Name, InstallState FROM Win32_OptionalFeature WHERE Name = 'Microsoft-Hyper-V'"));
            foreach (ManagementObject item in searcher.Get())
            {
                using (item)
                {
                    var state = Convert.ToInt32(item.Properties["InstallState"]?.Value, System.Globalization.CultureInfo.InvariantCulture);
                    if (state == 1)
                    {
                        return true;
                    }
                }
            }

            reasons.Add("PCV_HYPERV_NOT_ENABLED");
            return false;
        }
        catch
        {
            reasons.Add("PCV_HYPERV_FEATURE_UNKNOWN");
            return false;
        }
    }

    private static bool GetVmmsRunning(ICollection<string> reasons)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                new ObjectQuery("SELECT State FROM Win32_Service WHERE Name = 'vmms'"));
            foreach (ManagementObject item in searcher.Get())
            {
                using (item)
                {
                    var running = string.Equals(GetStringProperty(item, "State"), "Running", StringComparison.OrdinalIgnoreCase);
                    if (!running)
                    {
                        reasons.Add("PCV_VMMS_NOT_RUNNING");
                    }

                    return running;
                }
            }

            reasons.Add("PCV_VMMS_UNKNOWN");
            return false;
        }
        catch
        {
            reasons.Add("PCV_VMMS_UNKNOWN");
            return false;
        }
    }

    private bool GetDefaultSwitchPresent(ICollection<string> reasons, CancellationToken cancellationToken)
    {
        try
        {
            var present = switchProvider
                .GetSwitches(cancellationToken)
                .Any(item => string.Equals(item.Name, "Default Switch", StringComparison.OrdinalIgnoreCase));
            if (!present)
            {
                reasons.Add("PCV_DEFAULT_SWITCH_MISSING");
            }

            return present;
        }
        catch
        {
            reasons.Add("PCV_DEFAULT_SWITCH_UNKNOWN");
            return false;
        }
    }

    private static bool GetAdminElevated(ICollection<string> reasons)
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var elevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!elevated)
            {
                reasons.Add("PCV_ADMIN_REQUIRED");
            }

            return elevated;
        }
        catch
        {
            reasons.Add("PCV_ADMIN_UNKNOWN");
            return false;
        }
    }

    private static string? GetRegistryString(string subKey, string valueName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(subKey);
        return key?.GetValue(valueName) is { } value
            ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
            : null;
    }

    private static string? GetOperatingSystemProperty(string propertyName)
    {
        using var searcher = new ManagementObjectSearcher(
            new ObjectQuery($"SELECT {propertyName} FROM Win32_OperatingSystem"));
        foreach (ManagementObject item in searcher.Get())
        {
            using (item)
            {
                return GetStringProperty(item, propertyName);
            }
        }

        return null;
    }
}
