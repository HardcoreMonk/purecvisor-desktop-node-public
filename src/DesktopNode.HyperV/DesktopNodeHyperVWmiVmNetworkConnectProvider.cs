using System.Globalization;
using System.Management;
using DesktopNode.Contracts;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmNetworkConnectProvider : IDesktopNodeHyperVVmNetworkConnectProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string EthernetPortAllocationSettingClass = "Msvm_EthernetPortAllocationSettingData";
    public const string VirtualEthernetSwitchClass = "Msvm_VirtualEthernetSwitch";
    public const string AddResourceSettingsMethod = "AddResourceSettings";
    public const string ModifyResourceSettingsMethod = "ModifyResourceSettings";
    public const string SyntheticEthernetPortSubtype = "Microsoft:Hyper-V:Synthetic Ethernet Port";
    public const string EthernetConnectionSubtype = "Microsoft:Hyper-V:Ethernet Connection";
    public const int EthernetConnectionResourceType = 33;

    public DesktopNodeHyperVVmNetworkConnectInfo Invoke(
        DesktopNodeHyperVVmNetworkConnectRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var vm = FindVm(scope, request.VmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{request.VmName}' was not found.",
            "The VM was not present in the native Hyper-V inventory.",
            false);

        using var settings = GetRealizedSettings(vm, cancellationToken);
        var notes = GetStringProperty(settings, "Notes");
        if (!DesktopNodeHyperVManagedNotes.IsManagedNotes(notes))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                NetworkChangeProblemCodes.NotManaged,
                $"VM '{request.VmName}' is not managed by PureCVisor.",
                "Only managed VMs can change switch attachment.",
                false);
        }

        if (DesktopNodeHyperVManagedNotes.IsTemplateLocked(notes))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                NetworkChangeProblemCodes.TemplateLocked,
                $"VM '{request.VmName}' is template-locked.",
                "Remove template-lock before changing the VM network.",
                false);
        }

        var power = MapEnabledState(vm.Properties["EnabledState"]?.Value);
        if (!string.Equals(power, "stopped", StringComparison.OrdinalIgnoreCase))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                NetworkChangeProblemCodes.SourceNotOff,
                $"VM '{request.VmName}' must be Off before changing the switch.",
                "Shut down or power off the VM, then retry.",
                false);
        }

        using var virtualSwitch = FindSwitch(scope, request.SwitchName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            NetworkChangeProblemCodes.SwitchNotFound,
            $"Switch '{request.SwitchName}' was not found.",
            "Use a Hyper-V switch present in network inventory.",
            false);

        using var ethernetPort = FindResource(settings, SyntheticEthernetPortSubtype, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NETWORK_ADAPTER_MISSING",
            $"VM '{request.VmName}' has no network adapter.",
            "P2-15 adds NICs. This connect path only retargets an existing adapter.",
            false);

        var switchPath = virtualSwitch.Path.Path;
        var switchName = GetStringProperty(virtualSwitch, "ElementName") ?? request.SwitchName;
        using var connection = FindEthernetConnection(ethernetPort, cancellationToken);
        if (connection is null)
        {
            AddEthernetConnection(scope, settings, ethernetPort.Path.Path, switchPath, switchName, cancellationToken);
        }
        else
        {
            SetEthernetConnectionTarget(connection, switchPath, switchName);
            ModifySingleResourceSetting(scope, connection, "vm.network.connect", cancellationToken);
        }

        return new DesktopNodeHyperVVmNetworkConnectInfo("connect", request.VmName, switchName);
    }

    public DesktopNodeHyperVVmDeviceAddInfo AddNic(
        DesktopNodeHyperVVmDeviceAddRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var vm = FindVm(scope, request.VmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{request.VmName}' was not found.",
            "The VM was not present in the native Hyper-V inventory.",
            false);
        using var settings = GetRealizedSettings(vm, cancellationToken);
        RejectUnpreparedVm(vm, settings, request.VmName);
        if (CountSubtype(settings, SyntheticEthernetPortSubtype, cancellationToken) >= VmDeviceAddPolicy.MaxNicCount)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmDeviceAddProblemCodes.Limit,
                $"VM '{request.VmName}' already has {VmDeviceAddPolicy.MaxNicCount.ToString(CultureInfo.InvariantCulture)} synthetic NICs.",
                "P2-15 adds one extra NIC. It does not open a device shop.",
                false);
        }

        using var virtualSwitch = FindSwitch(scope, request.SwitchName ?? string.Empty, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            VmDeviceAddProblemCodes.SwitchNotFound,
            $"Switch '{request.SwitchName}' was not found.",
            "Use a Hyper-V switch present in network inventory.",
            false);
        using var port = GetDefaultResourceSetting(scope, DesktopNodeHyperVWmiVmCreateProvider.SyntheticEthernetPortResourceType, SyntheticEthernetPortSubtype, cancellationToken);
        port["ElementName"] = "Network Adapter";
        port["StaticMacAddress"] = false;
        port["VirtualSystemIdentifiers"] = new[] { $"{{{Guid.NewGuid()}}}" };
        var portPath = AddResourceSetting(scope, settings, port, "vm.nic.add", cancellationToken);
        var switchName = GetStringProperty(virtualSwitch, "ElementName") ?? request.SwitchName ?? string.Empty;
        AddEthernetConnection(scope, settings, portPath, virtualSwitch.Path.Path, switchName, cancellationToken);
        return new DesktopNodeHyperVVmDeviceAddInfo("nic-add", request.VmName, VmDeviceAddPolicy.KindNic, switchName);
    }

    private static void RejectUnpreparedVm(ManagementObject vm, ManagementObject settings, string vmName)
    {
        var notes = GetStringProperty(settings, "Notes");
        if (!DesktopNodeHyperVManagedNotes.IsManagedNotes(notes))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmDeviceAddProblemCodes.NotManaged,
                $"VM '{vmName}' is not managed by PureCVisor.",
                "Only managed VMs can add a NIC.",
                false);
        }

        if (DesktopNodeHyperVManagedNotes.IsTemplateLocked(notes))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmDeviceAddProblemCodes.TemplateLocked,
                $"VM '{vmName}' is template-locked.",
                "Remove template-lock before adding a NIC.",
                false);
        }

        var power = MapEnabledState(vm.Properties["EnabledState"]?.Value);
        if (!string.Equals(power, "stopped", StringComparison.OrdinalIgnoreCase))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmDeviceAddProblemCodes.SourceNotOff,
                $"VM '{vmName}' must be Off before adding a NIC.",
                "Shut down or power off the VM, then retry.",
                false);
        }

        var subtype = GetStringProperty(settings, "VirtualSystemSubType");
        if (subtype is null || !subtype.Trim().EndsWith(":2", StringComparison.OrdinalIgnoreCase))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmDeviceAddProblemCodes.GenerationUnsupported,
                $"VM '{vmName}' is not Generation 2.",
                "P2-15 adds devices on Generation 2 VMs only.",
                false);
        }
    }

    private static int CountSubtype(ManagementObject settings, string resourceSubType, CancellationToken cancellationToken)
    {
        var count = 0;
        using var resources = settings.GetRelated(null, "Msvm_VirtualSystemSettingDataComponent", null, null, "PartComponent", "GroupComponent", false, null);
        foreach (ManagementObject item in resources)
        {
            using (item)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.Equals(GetStringProperty(item, "ResourceSubType"), resourceSubType, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static string AddResourceSetting(
        ManagementScope scope,
        ManagementObject settings,
        ManagementObject resource,
        string operation,
        CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(AddResourceSettingsMethod);
        inParams["AffectedConfiguration"] = settings.Path.Path;
        inParams["ResourceSettings"] = new[] { resource.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(AddResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
        var resultingSettings = outParams["ResultingResourceSettings"] as string[];
        if (resultingSettings is null || resultingSettings.Length == 0 || string.IsNullOrWhiteSpace(resultingSettings[0]))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_HYPERV_WMI_RESOURCE_MISSING",
                $"Native Hyper-V WMI operation '{operation}' did not return a resource path.",
                "AddResourceSettings completed without ResultingResourceSettings.",
                true);
        }

        return resultingSettings[0];
    }

    private static ManagementObject GetDefaultResourceSetting(ManagementScope scope, int resourceType, string resourceSubType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var pools = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(
                $"SELECT * FROM Msvm_ResourcePool WHERE ResourceType = {resourceType.ToString(CultureInfo.InvariantCulture)} AND ResourceSubType = '{resourceSubType.Replace("'", "''", StringComparison.Ordinal)}'"));
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

                                return new ManagementObject(scope, new ManagementPath(partComponent), null);
                            }
                        }
                    }
                }
            }
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_HYPERV_WMI_RESOURCE_MISSING",
            $"Hyper-V resource pool '{resourceSubType}' was not found.",
            "The host Hyper-V resource pool did not expose a default setting.",
            true);
    }

    private static ManagementObject GetRealizedSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ManagementObject item in vm.GetRelated(
            "Msvm_VirtualSystemSettingData",
            "Msvm_SettingsDefineState",
            null,
            null,
            "SettingData",
            "ManagedElement",
            false,
            null))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var type = GetStringProperty(item, "VirtualSystemType");
            if (string.Equals(type, "Microsoft:Hyper-V:System:Realized", StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            "VM settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);
    }

    private static ManagementObject? FindSwitch(ManagementScope scope, string switchName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery($"SELECT * FROM {VirtualEthernetSwitchClass}"));
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

    private static ManagementObject? FindResource(ManagementObject settings, string resourceSubType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var resources = settings.GetRelated(null, "Msvm_VirtualSystemSettingDataComponent", null, null, "PartComponent", "GroupComponent", false, null);
        foreach (ManagementObject item in resources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subtype = GetStringProperty(item, "ResourceSubType");
            if (string.Equals(subtype, resourceSubType, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    private static ManagementObject? FindEthernetConnection(ManagementObject ethernetPort, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ManagementObject item in ethernetPort.GetRelated(EthernetPortAllocationSettingClass))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subtype = GetStringProperty(item, "ResourceSubType");
            if (string.Equals(subtype, EthernetConnectionSubtype, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    private static void AddEthernetConnection(
        ManagementScope scope,
        ManagementObject settings,
        string portPath,
        string switchPath,
        string switchName,
        CancellationToken cancellationToken)
    {
        using var connection = GetDefaultResourceSetting(scope, cancellationToken);
        connection["Parent"] = portPath;
        SetEthernetConnectionTarget(connection, switchPath, switchName);
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(AddResourceSettingsMethod);
        inParams["AffectedConfiguration"] = settings.Path.Path;
        inParams["ResourceSettings"] = new[] { connection.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(AddResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.network.connect", cancellationToken);
    }

    private static void SetEthernetConnectionTarget(ManagementObject connection, string switchPath, string switchName)
    {
        connection["HostResource"] = new[] { switchPath };
        connection["LastKnownSwitchName"] = switchName;
        connection["EnabledState"] = 2;
    }

    private static void ModifySingleResourceSetting(
        ManagementScope scope,
        ManagementObject resource,
        string operation,
        CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifyResourceSettingsMethod);
        inParams["ResourceSettings"] = new[] { resource.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifyResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
    }

    private static ManagementObject GetDefaultResourceSetting(ManagementScope scope, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var pools = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(
                $"SELECT * FROM Msvm_ResourcePool WHERE ResourceType = {EthernetConnectionResourceType.ToString(CultureInfo.InvariantCulture)} AND ResourceSubType = '{EthernetConnectionSubtype.Replace("'", "''", StringComparison.Ordinal)}'"));
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
                                var subtype = GetStringProperty(setting, "ResourceSubType");
                                if (string.Equals(subtype, EthernetConnectionSubtype, StringComparison.OrdinalIgnoreCase))
                                {
                                    return setting;
                                }

                                setting.Dispose();
                            }
                        }
                    }
                }
            }
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_HYPERV_DEFAULT_RESOURCE_MISSING",
            "Default Ethernet connection settings were not found.",
            "The native connect adapter could not find the default RASD in Msvm_ResourcePool.",
            true);
    }
}
