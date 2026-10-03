using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiSwitchProvider : IDesktopNodeHyperVSwitchProvider
{
    public const string SwitchQuery = "SELECT * FROM Msvm_VirtualEthernetSwitch";

    public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        var internalPortNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var internalPortSearcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery("SELECT Name FROM Msvm_InternalEthernetPort")))
        using (var internalPorts = internalPortSearcher.Get())
        {
            foreach (ManagementObject internalPort in internalPorts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (internalPort)
                {
                    var internalPortName = GetStringProperty(internalPort, "Name");
                    if (!string.IsNullOrWhiteSpace(internalPortName))
                    {
                        internalPortNames.Add(internalPortName);
                    }
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(SwitchQuery));
        using var switchItems = searcher.Get();

        var switches = new List<DesktopNodeHyperVSwitchInfo>();
        foreach (ManagementObject item in switchItems)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (item)
            {
                var name = GetStringProperty(item, "ElementName") ??
                    GetStringProperty(item, "Name") ??
                    "unknown";
                EnsureAssociationTraversalPath(item.Path?.Path);
                var hasInternalManagementPort = false;
                var hasExternalBinding = false;
                string? externalAdapterDescription = null;
                using var relatedPorts = item.GetRelated("Msvm_EthernetSwitchPort");
                foreach (ManagementObject relatedPort in relatedPorts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using (relatedPort)
                    {
                        var relatedPortName = GetStringProperty(relatedPort, "Name");
                        if (relatedPortName is not null && internalPortNames.Contains(relatedPortName))
                        {
                            hasInternalManagementPort = true;
                        }

                        using var allocationSettings = relatedPort.GetRelated("Msvm_EthernetPortAllocationSettingData");
                        foreach (ManagementObject allocationSetting in allocationSettings)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            using (allocationSetting)
                            {
                                foreach (var externalPortPath in ExternalPortPaths(allocationSetting.Properties["HostResource"]?.Value))
                                {
                                    hasExternalBinding = true;
                                    externalAdapterDescription ??= TryReadExternalAdapterDescription(externalPortPath);
                                }
                            }
                        }
                    }
                }

                switches.Add(MapSwitch(name, hasInternalManagementPort, hasExternalBinding, externalAdapterDescription));
            }
        }

        return switches;
    }

    public static IEnumerable<string> ExternalPortPaths(object? hostResource)
    {
        var resources = hostResource switch
        {
            string resource => [resource],
            string[] values => values,
            _ => []
        };
        return resources.Where(static resource =>
            !string.IsNullOrWhiteSpace(resource) &&
            resource.Contains("Msvm_ExternalEthernetPort", StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryReadExternalAdapterDescription(string externalPortPath)
    {
        try
        {
            using var port = new ManagementObject(externalPortPath);
            port.Get();
            var description = GetStringProperty(port, "ElementName");
            return string.IsNullOrWhiteSpace(description) ? null : description;
        }
        catch (ManagementException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static void EnsureAssociationTraversalPath(string? objectPath)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            throw new InvalidOperationException("WMI switch object path is required for association traversal.");
        }
    }

    public static DesktopNodeHyperVSwitchInfo MapSwitch(
        string? name,
        bool hasInternalManagementPort = false,
        bool hasExternalBinding = false,
        string? externalAdapterDescription = null)
    {
        var switchName = string.IsNullOrWhiteSpace(name) ? "unknown" : name;
        var isDefault = string.Equals(switchName, "Default Switch", StringComparison.OrdinalIgnoreCase);
        var isInternal = isDefault || (hasInternalManagementPort && !hasExternalBinding);
        // A switch with neither a management-OS port nor an external binding is a private switch.
        var isPrivate = !isDefault && !hasInternalManagementPort && !hasExternalBinding;
        // An externally bound switch is complete only when the bound adapter can be named.
        var isExternal = !isDefault && hasExternalBinding && !string.IsNullOrWhiteSpace(externalAdapterDescription);
        return new DesktopNodeHyperVSwitchInfo(
            Name: switchName,
            Type: isInternal ? "internal" : isPrivate ? "private" : isExternal ? "external" : "unknown",
            IsDefault: isDefault,
            AllowManagementOs: isInternal ? true : isPrivate ? false : isExternal ? hasInternalManagementPort : null,
            NetAdapterInterfaceDescription: isExternal ? externalAdapterDescription : null);
    }
}
