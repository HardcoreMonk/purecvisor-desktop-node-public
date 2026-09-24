using System.Globalization;
using System.Management;
using DesktopNode.Contracts;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiSwitchMutationProvider : IDesktopNodeHyperVSwitchMutationProvider
{
    public const string VirtualEthernetSwitchClass = "Msvm_VirtualEthernetSwitch";
    public const string VirtualEthernetSwitchSettingClass = "Msvm_VirtualEthernetSwitchSettingData";
    public const string VirtualEthernetSwitchManagementServiceClass = "Msvm_VirtualEthernetSwitchManagementService";
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string DefineSystemMethod = "DefineSystem";
    public const string DestroySystemMethod = "DestroySystem";
    public const string AddResourceSettingsMethod = "AddResourceSettings";
    public const string EthernetPortAllocationSettingClass = "Msvm_EthernetPortAllocationSettingData";
    public const string SyntheticEthernetPortSubtype = "Microsoft:Hyper-V:Synthetic Ethernet Port";
    public const int SyntheticEthernetPortResourceType = 10;

    public DesktopNodeHyperVSwitchMutationInfo Query(string switchName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var virtualSwitch = FindSwitch(scope, switchName, cancellationToken);
        if (virtualSwitch is null)
        {
            return Missing(switchName);
        }

        return ReadInfo(scope, virtualSwitch, switchName, cancellationToken);
    }

    public DesktopNodeHyperVSwitchMutationInfo Create(
        string switchName,
        string switchType,
        bool allowManagementOs,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var existing = FindSwitch(scope, switchName, cancellationToken);
        if (existing is not null)
        {
            return ReadInfo(scope, existing, switchName, cancellationToken);
        }

        using var service = GetService(scope, VirtualEthernetSwitchManagementServiceClass, cancellationToken);
        using var settingsClass = new ManagementClass(scope, new ManagementPath(VirtualEthernetSwitchSettingClass), null);
        using var settings = settingsClass.CreateInstance() ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NETWORK_SWITCH_CREATE_FAILED",
            $"Switch '{switchName}' settings could not be created.",
            "Msvm_VirtualEthernetSwitchSettingData was unavailable.",
            true);
        settings["ElementName"] = switchName;
        settings["Notes"] = new[] { DesktopNodeHyperVManagedNotes.Marker };

        using var inParams = service.GetMethodParameters(DefineSystemMethod);
        inParams["SystemSettings"] = settings.GetText(TextFormat.WmiDtd20);
        inParams["ResourceSettings"] = Array.Empty<string>();
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(DefineSystemMethod, inParams, null);
        WaitForMethodResult(outParams, "switch.create", cancellationToken);

        using var created = FindSwitch(scope, switchName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NETWORK_SWITCH_CREATE_FAILED",
            $"Switch '{switchName}' was not visible after DefineSystem.",
            "Hyper-V did not return the created virtual Ethernet switch.",
            true);

        if (string.Equals(switchType, NetworkChangePolicy.TypeInternal, StringComparison.OrdinalIgnoreCase) &&
            allowManagementOs)
        {
            try
            {
                AddManagementOsAdapter(scope, created, switchName, cancellationToken);
            }
            catch
            {
                TryDestroy(service, created, CancellationToken.None);
                throw;
            }
        }

        using var realized = FindSwitch(scope, switchName, cancellationToken) ?? created;
        return ReadInfo(scope, realized, switchName, cancellationToken);
    }

    public DesktopNodeHyperVSwitchMutationInfo Remove(string switchName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var virtualSwitch = FindSwitch(scope, switchName, cancellationToken);
        if (virtualSwitch is null)
        {
            return Missing(switchName);
        }

        using var service = GetService(scope, VirtualEthernetSwitchManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(DestroySystemMethod);
        inParams["AffectedSystem"] = virtualSwitch.Path.Path;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
        WaitForMethodResult(outParams, "switch.remove", cancellationToken);
        return Missing(switchName);
    }

    private static DesktopNodeHyperVSwitchMutationInfo ReadInfo(
        ManagementScope scope,
        ManagementObject virtualSwitch,
        string switchName,
        CancellationToken cancellationToken)
    {
        var notes = GetStringProperty(virtualSwitch, "Notes");
        var mapped = DesktopNodeHyperVWmiSwitchProvider.MapSwitch(
            switchName,
            hasInternalManagementPort: HasInternalManagementPort(virtualSwitch, cancellationToken),
            hasExternalBinding: false);
        return new DesktopNodeHyperVSwitchMutationInfo(
            Name: switchName,
            Exists: true,
            Type: mapped.Type,
            AttachedVmCount: CountAttachedVms(scope, virtualSwitch, cancellationToken),
            ProductOwned: NetworkChangePolicy.IsProductSwitchName(switchName) ||
                DesktopNodeHyperVManagedNotes.IsManagedNotes(notes),
            AllowManagementOs: mapped.AllowManagementOs == true);
    }

    private static DesktopNodeHyperVSwitchMutationInfo Missing(string switchName)
    {
        return new DesktopNodeHyperVSwitchMutationInfo(
            switchName,
            false,
            null,
            0,
            NetworkChangePolicy.IsProductSwitchName(switchName),
            false);
    }

    private static ManagementObject? FindSwitch(ManagementScope scope, string switchName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery($"SELECT * FROM {VirtualEthernetSwitchClass}"));
        foreach (ManagementObject item in searcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var elementName = GetStringProperty(item, "ElementName");
            var name = GetStringProperty(item, "Name");
            if (string.Equals(elementName, switchName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, switchName, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    private static int CountAttachedVms(
        ManagementScope scope,
        ManagementObject virtualSwitch,
        CancellationToken cancellationToken)
    {
        var switchPath = virtualSwitch.Path.Path;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(VmQuery));
        foreach (ManagementObject vm in searcher.Get())
        {
            using (vm)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var vmName = GetStringProperty(vm, "ElementName") ?? GetStringProperty(vm, "Name");
                if (string.IsNullOrWhiteSpace(vmName))
                {
                    continue;
                }

                using var related = vm.GetRelated(EthernetPortAllocationSettingClass);
                foreach (ManagementObject allocation in related)
                {
                    using (allocation)
                    {
                        if (HostResourceContains(allocation, switchPath))
                        {
                            names.Add(vmName);
                            break;
                        }
                    }
                }
            }
        }

        return names.Count;
    }

    private static bool HasInternalManagementPort(ManagementObject virtualSwitch, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = virtualSwitch.GetRelated("Msvm_InternalEthernetPort");
        foreach (ManagementObject item in related)
        {
            using (item)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HostResourceContains(ManagementBaseObject allocation, string switchPath)
    {
        var hostResource = allocation.Properties["HostResource"]?.Value;
        return hostResource switch
        {
            string resource => resource.Contains(switchPath, StringComparison.OrdinalIgnoreCase),
            string[] resources => resources.Any(resource =>
                resource.Contains(switchPath, StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    private static void AddManagementOsAdapter(
        ManagementScope scope,
        ManagementObject virtualSwitch,
        string switchName,
        CancellationToken cancellationToken)
    {
        using var host = FindHostComputerSystem(scope, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NETWORK_SWITCH_CREATE_FAILED",
            $"Host computer system was not found while creating internal switch '{switchName}'.",
            "Internal switches require a Microsoft Hosting Computer System object.",
            true);
        using var hostSettings = FindCurrentSettings(host, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NETWORK_SWITCH_CREATE_FAILED",
            $"Host settings were not found while creating internal switch '{switchName}'.",
            "Msvm_VirtualSystemSettingData was not available for the hosting computer system.",
            true);
        using var portSetting = GetDefaultSyntheticEthernetPortSetting(scope, cancellationToken);
        portSetting["ElementName"] = switchName;
        using var vsms = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = vsms.GetMethodParameters(AddResourceSettingsMethod);
        inParams["AffectedConfiguration"] = hostSettings;
        inParams["ResourceSettings"] = new[] { portSetting.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = vsms.InvokeMethod(AddResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "switch.create.host-adapter", cancellationToken);

        using var connectionClass = new ManagementClass(scope, new ManagementPath(EthernetPortAllocationSettingClass), null);
        using var connection = connectionClass.CreateInstance() ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NETWORK_SWITCH_CREATE_FAILED",
            $"Internal switch '{switchName}' host connection could not be created.",
            "Msvm_EthernetPortAllocationSettingData was unavailable.",
            true);
        connection["HostResource"] = new[] { virtualSwitch.Path.Path };
        using var connectIn = vsms.GetMethodParameters(AddResourceSettingsMethod);
        connectIn["AffectedConfiguration"] = hostSettings;
        connectIn["ResourceSettings"] = new[] { connection.GetText(TextFormat.WmiDtd20) };
        using var connectOut = vsms.InvokeMethod(AddResourceSettingsMethod, connectIn, null);
        WaitForMethodResult(connectOut, "switch.create.host-connection", cancellationToken);
    }

    private static ManagementObject? FindHostComputerSystem(ManagementScope scope, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery("SELECT * FROM Msvm_ComputerSystem"));
        foreach (ManagementObject item in searcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var caption = GetStringProperty(item, "Caption") ?? string.Empty;
            var description = GetStringProperty(item, "Description") ?? string.Empty;
            if (caption.Contains("Hosting Computer System", StringComparison.OrdinalIgnoreCase) ||
                description.Contains("Hosting Computer System", StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    private static ManagementObject? FindCurrentSettings(ManagementObject system, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = system.GetRelated("Msvm_VirtualSystemSettingData");
        foreach (ManagementObject item in related)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item.Properties["VirtualSystemType"]?.Value is string type &&
                type.Contains("Snapshot", StringComparison.OrdinalIgnoreCase))
            {
                item.Dispose();
                continue;
            }

            return item;
        }

        return null;
    }

    private static ManagementObject GetDefaultSyntheticEthernetPortSetting(
        ManagementScope scope,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var pools = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(
                $"SELECT * FROM Msvm_ResourcePool WHERE ResourceType = {SyntheticEthernetPortResourceType.ToString(CultureInfo.InvariantCulture)} AND ResourceSubType = '{SyntheticEthernetPortSubtype.Replace("'", "''", StringComparison.Ordinal)}'"));
        foreach (ManagementObject pool in pools.Get())
        {
            using (pool)
            {
                foreach (ManagementObject capability in pool.GetRelated("Msvm_AllocationCapabilities"))
                {
                    using (capability)
                    {
                        foreach (ManagementObject relationship in capability.GetRelationships("Msvm_SettingsDefineCapabilities"))
                        {
                            using (relationship)
                            {
                                var valueRole = Convert.ToUInt16(relationship.Properties["ValueRole"]?.Value, CultureInfo.InvariantCulture);
                                var valueRange = Convert.ToUInt16(relationship.Properties["ValueRange"]?.Value, CultureInfo.InvariantCulture);
                                if (valueRole != 0 || valueRange != 0)
                                {
                                    continue;
                                }

                                var partComponent = Convert.ToString(relationship.Properties["PartComponent"]?.Value, CultureInfo.InvariantCulture);
                                if (string.IsNullOrWhiteSpace(partComponent))
                                {
                                    continue;
                                }

                                var setting = new ManagementObject(partComponent);
                                setting.Get();
                                return setting;
                            }
                        }
                    }
                }
            }
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_NETWORK_SWITCH_CREATE_FAILED",
            "Default synthetic Ethernet port settings were not found.",
            "Hyper-V resource pool did not return a default NIC setting for the hosting computer system.",
            true);
    }

    private static void TryDestroy(ManagementObject service, ManagementObject virtualSwitch, CancellationToken cancellationToken)
    {
        try
        {
            using var inParams = service.GetMethodParameters(DestroySystemMethod);
            inParams["AffectedSystem"] = virtualSwitch.Path.Path;
            using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
            WaitForMethodResult(outParams, "switch.create.cleanup", cancellationToken);
        }
        catch
        {
        }
    }
}
