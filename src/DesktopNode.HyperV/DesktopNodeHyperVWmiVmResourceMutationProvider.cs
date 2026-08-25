using System.Globalization;
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmResourceMutationProvider : IDesktopNodeHyperVVmResourceMutationProvider
{
    private const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    private const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    private const string SettingDataComponentAssociationClass = "Msvm_VirtualSystemSettingDataComponent";
    private const string MemorySettingClass = "Msvm_MemorySettingData";
    private const string ProcessorSettingClass = "Msvm_ProcessorSettingData";
    private const string StorageSettingClass = "Msvm_StorageAllocationSettingData";
    private const string EthernetPortAllocationSettingClass = "Msvm_EthernetPortAllocationSettingData";
    private const string EthernetPortSettingDataComponentAssociationClass = "Msvm_EthernetPortSettingDataComponent";
    private const string EthernetSwitchPortBandwidthSettingClass = "Msvm_EthernetSwitchPortBandwidthSettingData";
    private const string AddFeatureSettingsMethod = "AddFeatureSettings";
    private const string ModifyResourceSettingsMethod = "ModifyResourceSettings";
    private const string ModifyFeatureSettingsMethod = "ModifyFeatureSettings";
    private readonly IDesktopNodeVirtualDiskOperations virtualDiskOperations;

    public DesktopNodeHyperVWmiVmResourceMutationProvider()
        : this(new DesktopNodeWmiVirtualDiskOperations())
    {
    }

    internal DesktopNodeHyperVWmiVmResourceMutationProvider(IDesktopNodeVirtualDiskOperations virtualDiskOperations)
    {
        ArgumentNullException.ThrowIfNull(virtualDiskOperations);
        this.virtualDiskOperations = virtualDiskOperations;
    }

    public DesktopNodeHyperVVmResourceMutationInfo Invoke(DesktopNodeHyperVVmResourceMutationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return request.Operation switch
        {
            "vm.set-memory" => SetMemory(request, cancellationToken),
            "vm.set-vcpu" => SetVcpu(request, cancellationToken),
            "vm.disk-resize" => ResizeDisk(request, cancellationToken),
            "vm.limit" => SetLimit(request, cancellationToken),
            "vm.qos.storage.set" => SetStorageQos(request, cancellationToken),
            "vm.qos.network.set" => SetNetworkQos(request, cancellationToken),
            _ => throw new DesktopNodeHyperVNativeOperationException(
                "PCV_OPERATION_NOT_ALLOWED",
                $"Operation '{request.Operation}' is not a native VM resource mutation operation.",
                "Use vm.limit, vm.set-memory, vm.set-vcpu, vm.disk-resize, vm.qos.storage.set, or vm.qos.network.set for this native resource mutation slice.",
                false)
        };
    }

    private static DesktopNodeHyperVVmResourceMutationInfo SetMemory(
        DesktopNodeHyperVVmResourceMutationRequest request,
        CancellationToken cancellationToken)
    {
        var memoryMb = request.MemoryMb ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_MEMORY_REQUIRED",
            "VM memory is required.",
            "Provide memory_mb before invoking vm.set-memory.",
            false);

        var scope = CreateScope();
        using var vm = FindVm(scope, request.Name, cancellationToken) ?? throw VmNotFound(request.Name);
        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw VmSettingsNotFound(request.Name);
        using var memory = FindResource(settings, MemorySettingClass, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_MEMORY_SETTINGS_NOT_FOUND",
            $"VM '{request.Name}' memory settings were not found.",
            "Msvm_MemorySettingData was not available for the VM.",
            true);

        var quantity = Convert.ToUInt64(memoryMb, CultureInfo.InvariantCulture);
        memory["VirtualQuantity"] = quantity;
        memory["Reservation"] = quantity;
        memory["Limit"] = quantity;
        ModifyResource(scope, memory, "vm.set-memory", cancellationToken);
        return new DesktopNodeHyperVVmResourceMutationInfo(request.Name, "set-memory", MemoryMb: memoryMb);
    }

    private static DesktopNodeHyperVVmResourceMutationInfo SetVcpu(
        DesktopNodeHyperVVmResourceMutationRequest request,
        CancellationToken cancellationToken)
    {
        var cpu = request.Cpu ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_CPU_REQUIRED",
            "VM CPU count is required.",
            "Provide cpu before invoking vm.set-vcpu.",
            false);

        var scope = CreateScope();
        using var vm = FindVm(scope, request.Name, cancellationToken) ?? throw VmNotFound(request.Name);
        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw VmSettingsNotFound(request.Name);
        using var processor = FindResource(settings, ProcessorSettingClass, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_PROCESSOR_SETTINGS_NOT_FOUND",
            $"VM '{request.Name}' processor settings were not found.",
            "Msvm_ProcessorSettingData was not available for the VM.",
            true);

        processor["VirtualQuantity"] = Convert.ToUInt64(cpu, CultureInfo.InvariantCulture);
        processor["Reservation"] = 0UL;
        processor["Limit"] = 100000UL;
        ModifyResource(scope, processor, "vm.set-vcpu", cancellationToken);
        return new DesktopNodeHyperVVmResourceMutationInfo(request.Name, "set-vcpu", Cpu: cpu);
    }

    private static DesktopNodeHyperVVmResourceMutationInfo SetLimit(
        DesktopNodeHyperVVmResourceMutationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MemoryMb is null && request.Cpu is null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_LIMIT_REQUIRED",
                "VM limit requires at least one CPU or memory value.",
                "Provide memory_mb and/or cpu before invoking vm.limit.",
                false);
        }

        if (request.MemoryMb is not null)
        {
            SetMemory(request, cancellationToken);
        }

        if (request.Cpu is not null)
        {
            SetVcpu(request, cancellationToken);
        }

        return new DesktopNodeHyperVVmResourceMutationInfo(
            request.Name,
            "limit",
            MemoryMb: request.MemoryMb,
            Cpu: request.Cpu);
    }

    private DesktopNodeHyperVVmResourceMutationInfo ResizeDisk(
        DesktopNodeHyperVVmResourceMutationRequest request,
        CancellationToken cancellationToken)
    {
        var diskGb = request.DiskGb ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_DISK_SIZE_REQUIRED",
            "VM disk size is required.",
            "Provide disk_gb before invoking vm.disk-resize.",
            false);
        if (string.IsNullOrWhiteSpace(request.DiskPath))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_DISK_PATH_REQUIRED",
                $"VM '{request.Name}' disk path is required for resize.",
                "Resolve the attached VHD/VHDX path before invoking vm.disk-resize.",
                false);
        }

        var requestedBytes = checked(Convert.ToUInt64(diskGb, CultureInfo.InvariantCulture) * 1024UL * 1024UL * 1024UL);
        var currentBytes = virtualDiskOperations.GetMaxInternalSize(request.DiskPath, cancellationToken);
        if (requestedBytes < currentBytes)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_DISK_SHRINK_NOT_SUPPORTED",
                $"VM '{request.Name}' disk resize cannot shrink the virtual disk.",
                "Provide disk_gb greater than or equal to the current virtual disk size.",
                false);
        }

        virtualDiskOperations.Resize(request.DiskPath, requestedBytes, cancellationToken);

        return new DesktopNodeHyperVVmResourceMutationInfo(
            request.Name,
            "disk-resize",
            DiskGb: diskGb,
            DiskPath: request.DiskPath);
    }

    private static DesktopNodeHyperVVmResourceMutationInfo SetStorageQos(
        DesktopNodeHyperVVmResourceMutationRequest request,
        CancellationToken cancellationToken)
    {
        var target = RequireText(request.QosDisk, "PCV_VM_QOS_STORAGE_TARGET_REQUIRED", "VM storage QoS target disk is required.", "Provide disk before invoking vm.qos.storage.set.");
        var maximumIops = request.MaximumIops ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_QOS_STORAGE_MAXIMUM_IOPS_REQUIRED",
            "VM storage QoS maximum IOPS is required.",
            "Provide maximum_iops before invoking vm.qos.storage.set.",
            false);
        var minimumIops = request.MinimumIops ?? 0;

        var scope = CreateScope();
        using var vm = FindVm(scope, request.Name, cancellationToken) ?? throw VmNotFound(request.Name);
        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw VmSettingsNotFound(request.Name);
        using var storage = FindStorageResource(settings, target, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_STORAGE_QOS_TARGET_NOT_FOUND",
            $"VM '{request.Name}' storage QoS target '{target}' was not found.",
            "Use disk0, an attached VHD/VHDX path, or a VHD/VHDX file name present in VM inventory.",
            false);

        var previousMaximumIops = GetUInt64Property(storage, "IOPSLimit");
        var previousMinimumIops = GetUInt64Property(storage, "IOPSReservation");
        storage["IOPSLimit"] = Convert.ToUInt64(maximumIops, CultureInfo.InvariantCulture);
        storage["IOPSReservation"] = Convert.ToUInt64(minimumIops, CultureInfo.InvariantCulture);
        ModifyResource(scope, storage, "vm.qos.storage.set", cancellationToken);

        return new DesktopNodeHyperVVmResourceMutationInfo(
            request.Name,
            "storage-qos",
            Evidence: BuildStorageQosEvidence(
                request,
                target,
                previousMaximumIops,
                previousMinimumIops,
                maximumIops,
                minimumIops,
                hostMutationPerformed: true));
    }

    private static DesktopNodeHyperVVmResourceMutationInfo SetNetworkQos(
        DesktopNodeHyperVVmResourceMutationRequest request,
        CancellationToken cancellationToken)
    {
        var target = RequireText(request.QosAdapter, "PCV_VM_QOS_NETWORK_TARGET_REQUIRED", "VM network QoS target adapter is required.", "Provide adapter before invoking vm.qos.network.set.");
        var maximumKbps = request.MaximumKbps ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_QOS_NETWORK_MAXIMUM_KBPS_REQUIRED",
            "VM network QoS maximum Kbps is required.",
            "Provide maximum_kbps before invoking vm.qos.network.set.",
            false);
        var minimumKbps = request.MinimumKbps ?? 0;

        var scope = CreateScope();
        using var vm = FindVm(scope, request.Name, cancellationToken) ?? throw VmNotFound(request.Name);
        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw VmSettingsNotFound(request.Name);
        using var port = FindNetworkPort(settings, target, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NETWORK_QOS_TARGET_NOT_FOUND",
            $"VM '{request.Name}' network QoS target '{target}' was not found.",
            "Use adapter0, nic0, eth0, a switch name, or a Hyper-V adapter identifier present in VM network inventory.",
            false);

        using var bandwidth = FindBandwidthFeature(port, cancellationToken);
        var previousMaximumKbps = bandwidth is null
            ? 0UL
            : DesktopNodeHyperVResourceMutationPolicy.BitsPerSecondToKbps(GetUInt64Property(bandwidth, "Limit") ?? 0UL);
        var previousMinimumKbps = bandwidth is null
            ? 0UL
            : DesktopNodeHyperVResourceMutationPolicy.BitsPerSecondToKbps(GetUInt64Property(bandwidth, "Reservation") ?? 0UL);
        var maximumBitsPerSecond = DesktopNodeHyperVResourceMutationPolicy.KbpsToBitsPerSecond(maximumKbps);
        var minimumBitsPerSecond = DesktopNodeHyperVResourceMutationPolicy.KbpsToBitsPerSecond(minimumKbps);
        if (bandwidth is null)
        {
            using var newBandwidth = CreateBandwidthFeature(scope, maximumBitsPerSecond, minimumBitsPerSecond);
            AddFeature(scope, port, newBandwidth, "vm.qos.network.set", cancellationToken);
        }
        else
        {
            bandwidth["Limit"] = maximumBitsPerSecond;
            bandwidth["Reservation"] = minimumBitsPerSecond;
            ModifyFeature(scope, bandwidth, "vm.qos.network.set", cancellationToken);
        }

        return new DesktopNodeHyperVVmResourceMutationInfo(
            request.Name,
            "network-qos",
            Evidence: BuildNetworkQosEvidence(
                request,
                target,
                previousMaximumKbps,
                previousMinimumKbps,
                maximumKbps,
                minimumKbps,
                hostMutationPerformed: true));
    }

    private static ManagementObject? FindCurrentSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ManagementObject setting in vm.GetRelated(VirtualSystemSettingDataClass))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var instanceId = GetStringProperty(setting, "InstanceID");
            if (instanceId is null || !instanceId.Contains(@"\Realized", StringComparison.OrdinalIgnoreCase))
            {
                return setting;
            }

            setting.Dispose();
        }

        return null;
    }

    private static ManagementObject? FindResource(
        ManagementObject settings,
        string resourceClass,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = settings.GetRelated(
            resourceClass,
            SettingDataComponentAssociationClass,
            relationshipQualifier: null,
            relatedQualifier: null,
            relatedRole: "PartComponent",
            thisRole: "GroupComponent",
            classDefinitionsOnly: false,
            options: null);

        foreach (ManagementObject item in related)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return item;
        }

        return null;
    }

    private static ManagementObject? FindStorageResource(
        ManagementObject settings,
        string target,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = settings.GetRelated(
            StorageSettingClass,
            SettingDataComponentAssociationClass,
            relationshipQualifier: null,
            relatedQualifier: null,
            relatedRole: "PartComponent",
            thisRole: "GroupComponent",
            classDefinitionsOnly: false,
            options: null);

        var index = 0;
        foreach (ManagementObject item in related)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (StorageTargetMatches(item, target, index))
            {
                return item;
            }

            index += 1;
            item.Dispose();
        }

        return null;
    }

    private static ManagementObject? FindNetworkPort(
        ManagementObject settings,
        string target,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var ports = settings.GetRelated(
            EthernetPortAllocationSettingClass,
            SettingDataComponentAssociationClass,
            relationshipQualifier: null,
            relatedQualifier: null,
            relatedRole: "PartComponent",
            thisRole: "GroupComponent",
            classDefinitionsOnly: false,
            options: null);

        var index = 0;
        foreach (ManagementObject port in ports)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!NetworkTargetMatches(port, target, index))
            {
                index += 1;
                port.Dispose();
                continue;
            }

            return port;
        }

        return null;
    }

    private static ManagementObject CreateBandwidthFeature(ManagementScope scope, ulong maximumBitsPerSecond, ulong minimumBitsPerSecond)
    {
        using var featureClass = new ManagementClass(scope, new ManagementPath(EthernetSwitchPortBandwidthSettingClass), null);
        var feature = featureClass.CreateInstance() ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NETWORK_QOS_FEATURE_CREATE_FAILED",
            "Hyper-V network QoS bandwidth feature could not be created.",
            "Msvm_EthernetSwitchPortBandwidthSettingData.CreateInstance returned null.",
            true);
        feature["Limit"] = maximumBitsPerSecond;
        feature["Reservation"] = minimumBitsPerSecond;
        feature["Weight"] = 0UL;
        feature["BurstLimit"] = 0UL;
        feature["BurstSize"] = 0UL;
        return feature;
    }

    private static ManagementObject? FindBandwidthFeature(ManagementObject port, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var features = port.GetRelated(
            EthernetSwitchPortBandwidthSettingClass,
            EthernetPortSettingDataComponentAssociationClass,
            relationshipQualifier: null,
            relatedQualifier: null,
            relatedRole: "PartComponent",
            thisRole: "GroupComponent",
            classDefinitionsOnly: false,
            options: null);

        foreach (ManagementObject feature in features)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return feature;
        }

        return null;
    }

    private static void ModifyResource(
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

    private static void ModifyFeature(
        ManagementScope scope,
        ManagementObject feature,
        string operation,
        CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifyFeatureSettingsMethod);
        inParams["FeatureSettings"] = new[] { feature.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifyFeatureSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
    }

    private static void AddFeature(
        ManagementScope scope,
        ManagementObject port,
        ManagementObject feature,
        string operation,
        CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(AddFeatureSettingsMethod);
        inParams["AffectedConfiguration"] = port.Path.Path;
        inParams["FeatureSettings"] = new[] { feature.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(AddFeatureSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
    }

    private static bool StorageTargetMatches(ManagementBaseObject storage, string target, int index)
    {
        if (MatchesOrdinalTarget(target, "disk", index))
        {
            return true;
        }

        var hostResources = GetStringArrayProperty(storage, "HostResource");
        return hostResources.Any(value => MatchesTargetText(value, target) || MatchesTargetText(Path.GetFileName(value), target));
    }

    private static bool NetworkTargetMatches(ManagementBaseObject port, string target, int index)
    {
        if (MatchesOrdinalTarget(target, "adapter", index) ||
            MatchesOrdinalTarget(target, "nic", index) ||
            MatchesOrdinalTarget(target, "eth", index))
        {
            return true;
        }

        var candidates = new[]
        {
            GetStringProperty(port, "ElementName"),
            GetStringProperty(port, "InstanceID"),
            GetStringProperty(port, "LastKnownSwitchName")
        };
        if (candidates.Any(value => MatchesTargetText(value, target)))
        {
            return true;
        }

        return GetStringArrayProperty(port, "Connection").Any(value => MatchesTargetText(value, target));
    }

    private static bool MatchesOrdinalTarget(string target, string prefix, int index)
    {
        return string.Equals(target, $"{prefix}{index}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesTargetText(string? value, string target)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            string.Equals(value.Trim(), target.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string RequireText(string? value, string code, string message, string detail)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        throw new DesktopNodeHyperVNativeOperationException(code, message, detail, false);
    }

    private static ulong? GetUInt64Property(ManagementBaseObject item, string propertyName)
    {
        try
        {
            var value = item.Properties[propertyName]?.Value;
            return value is null
                ? null
                : Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidCastException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> GetStringArrayProperty(ManagementBaseObject item, string propertyName)
    {
        try
        {
            var value = item.Properties[propertyName]?.Value;
            return value switch
            {
                string[] values => values,
                string text when !string.IsNullOrWhiteSpace(text) => [text],
                _ => []
            };
        }
        catch (ManagementException)
        {
            return [];
        }
    }

    private static IReadOnlyDictionary<string, object?> BuildStorageQosEvidence(
        DesktopNodeHyperVVmResourceMutationRequest request,
        string target,
        ulong? previousMaximumIops,
        ulong? previousMinimumIops,
        int maximumIops,
        int minimumIops,
        bool hostMutationPerformed)
    {
        return BuildQosEvidence(
            "vm.qos.storage.set",
            "vm.qos.storage.rollback",
            request.Name,
            target,
            previousPolicy: new SortedDictionary<string, object?>
            {
                ["maximum_iops"] = previousMaximumIops,
                ["minimum_iops"] = previousMinimumIops
            },
            appliedPolicy: new SortedDictionary<string, object?>
            {
                ["maximum_iops"] = maximumIops,
                ["minimum_iops"] = minimumIops
            },
            request.RequestId,
            hostMutationPerformed);
    }

    private static IReadOnlyDictionary<string, object?> BuildNetworkQosEvidence(
        DesktopNodeHyperVVmResourceMutationRequest request,
        string target,
        ulong? previousMaximumKbps,
        ulong? previousMinimumKbps,
        int maximumKbps,
        int minimumKbps,
        bool hostMutationPerformed)
    {
        return BuildQosEvidence(
            "vm.qos.network.set",
            "vm.qos.network.rollback",
            request.Name,
            target,
            previousPolicy: new SortedDictionary<string, object?>
            {
                ["maximum_kbps"] = previousMaximumKbps,
                ["minimum_kbps"] = previousMinimumKbps
            },
            appliedPolicy: new SortedDictionary<string, object?>
            {
                ["maximum_kbps"] = maximumKbps,
                ["minimum_kbps"] = minimumKbps
            },
            request.RequestId,
            hostMutationPerformed);
    }

    private static IReadOnlyDictionary<string, object?> BuildQosEvidence(
        string operation,
        string rollbackOperation,
        string vmName,
        string target,
        IReadOnlyDictionary<string, object?> previousPolicy,
        IReadOnlyDictionary<string, object?> appliedPolicy,
        string? requestId,
        bool hostMutationPerformed)
    {
        return new SortedDictionary<string, object?>
        {
            ["contract"] = "hyperv-qos-mutation-apply-evidence.v1",
            ["operation"] = operation,
            ["vm"] = new SortedDictionary<string, object?> { ["name"] = vmName },
            ["target"] = target,
            ["previous_policy"] = previousPolicy,
            ["applied_policy"] = appliedPolicy,
            ["rollback_plan"] = new SortedDictionary<string, object?>
            {
                ["previous_policy_captured"] = true,
                ["rollback_operation"] = rollbackOperation
            },
            ["audit"] = new SortedDictionary<string, object?>
            {
                ["actor"] = "local-api-operator",
                ["request_id"] = requestId ?? string.Empty,
                ["args_redacted"] = true
            },
            ["host_mutation_performed"] = hostMutationPerformed
        };
    }

    private static DesktopNodeHyperVNativeOperationException VmNotFound(string vmName)
    {
        return new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);
    }

    private static DesktopNodeHyperVNativeOperationException VmSettingsNotFound(string vmName)
    {
        return new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            $"VM '{vmName}' settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);
    }
}
