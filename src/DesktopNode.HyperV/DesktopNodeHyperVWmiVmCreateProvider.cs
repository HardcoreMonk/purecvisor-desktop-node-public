using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmCreateProvider : IDesktopNodeHyperVVmCreateProvider
{
    private const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    private const string ImageManagementServiceClass = "Msvm_ImageManagementService";
    private const string ResourcePoolClass = "Msvm_ResourcePool";
    private const string AllocationCapabilitiesClass = "Msvm_AllocationCapabilities";
    private const string SettingsDefineCapabilitiesClass = "Msvm_SettingsDefineCapabilities";
    private const string VirtualSystemSettingClass = "Msvm_VirtualSystemSettingData";
    private const string ResourceAllocationSettingClass = "Msvm_ResourceAllocationSettingData";
    private const string StorageAllocationSettingClass = "Msvm_StorageAllocationSettingData";
    private const string VirtualHardDiskSettingClass = "Msvm_VirtualHardDiskSettingData";
    public const string SyntheticEthernetPortSettingClass = "Msvm_SyntheticEthernetPortSettingData";
    public const string EthernetPortAllocationSettingClass = "Msvm_EthernetPortAllocationSettingData";
    private const string DefineSystemMethod = "DefineSystem";
    private const string DestroySystemMethod = "DestroySystem";
    private const string CreateVirtualHardDiskMethod = "CreateVirtualHardDisk";
    private const string AddResourceSettingsMethod = "AddResourceSettings";
    private const string ModifyResourceSettingsMethod = "ModifyResourceSettings";
    private const string ModifySystemSettingsMethod = "ModifySystemSettings";
    // Get-VMHost SecureBootTemplates on the validation host reported this id for
    // MicrosoftUEFICertificateAuthority; MicrosoftWindows is 1734c6e8-3154-4dda-ba5f-a874cc483422.
    private const string UefiCertificateAuthorityTemplateId = "272e7447-90a4-4563-a4b9-8e4ab00526ce";
    private const string DefaultSwitchName = "Default Switch";
    private const string ManagedByNote = DesktopNodeHyperVManagedNotes.Marker;
    public const string SyntheticScsiControllerSubtype = "Microsoft:Hyper-V:Synthetic SCSI Controller";
    public const string SyntheticEthernetPortSubtype = "Microsoft:Hyper-V:Synthetic Ethernet Port";
    public const string EthernetConnectionSubtype = "Microsoft:Hyper-V:Ethernet Connection";
    public const int SyntheticScsiControllerResourceType = 6;
    public const int SyntheticEthernetPortResourceType = 10;
    public const int EthernetConnectionResourceType = 33;

    public DesktopNodeHyperVVmCreateInfo Invoke(DesktopNodeHyperVVmCreateRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(request.IsoPath))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_ISO_NOT_FOUND",
                $"ISO '{request.IsoPath}' was not found.",
                "Provide a local ISO file path visible to the Hyper-V host.",
                false);
        }

        var vmDirectory = Path.Combine(request.VmRoot, request.Name);
        var vhdPath = Path.Combine(vmDirectory, "disk0.vhdx");
        var steps = new List<string>();
        var vmDirectoryPreExisting = Directory.Exists(vmDirectory);
        var vhdPreExisting = File.Exists(vhdPath);
        var vmCreated = false;
        var vmDirectoryCreated = false;
        var vhdCreated = false;

        var scope = CreateScope(connect: true);

        if (FindVm(scope, request.Name, cancellationToken) is not null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_ALREADY_EXISTS",
                $"VM '{request.Name}' already exists.",
                "Choose a different VM name or remove the existing Hyper-V VM.",
                false);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(vmDirectory);
            cancellationToken.ThrowIfCancellationRequested();
            vmDirectoryCreated = true;
            steps.Add("Create VM folder");

            CreateVirtualHardDisk(scope, vhdPath, request.DiskGb, cancellationToken);
            vhdCreated = true;
            steps.Add("Create VHDX");

            DefineVirtualMachine(scope, request, vmDirectory, cancellationToken);
            vmCreated = true;
            steps.Add("Create Hyper-V VM");

            using var vm = FindVm(scope, request.Name, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_LOOKUP_FAILED",
                $"VM '{request.Name}' lookup failed.",
                "The VM was not visible after DefineSystem completed.",
                true);

            using var settings = GetRealizedSettings(vm, cancellationToken);
            EnsureCoreDevices(scope, settings, cancellationToken);
            steps.Add("Create Gen2 controllers");

            ModifyMemoryAndProcessor(scope, settings, request.MemoryMb, request.Cpu, cancellationToken);
            steps.Add("Set resources");

            AttachDiskAndDvd(scope, settings, vhdPath, request.IsoPath, cancellationToken);
            steps.Add("Attach ISO");

            AttachDefaultSwitch(scope, settings, cancellationToken);
            steps.Add("Attach Default Switch");

            if (request.Generation == 2)
            {
                ConfigureGen2Firmware(scope, request.Name, cancellationToken);
                steps.Add("Configure Gen2 firmware");
            }

            return new DesktopNodeHyperVVmCreateInfo(
                request.Name,
                vmDirectory,
                vhdPath,
                request.IsoPath,
                DefaultSwitchName,
                request.Generation,
                steps);
        }
        catch (DesktopNodeHyperVNativeOperationException)
        {
            Cleanup(scope, request.Name, vmCreated, vmDirectoryCreated, vmDirectoryPreExisting, vmDirectory, vhdCreated, vhdPreExisting, vhdPath, CancellationToken.None);
            throw;
        }
        catch (OperationCanceledException)
        {
            Cleanup(scope, request.Name, vmCreated, vmDirectoryCreated, vmDirectoryPreExisting, vmDirectory, vhdCreated, vhdPreExisting, vhdPath, CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            Cleanup(scope, request.Name, vmCreated, vmDirectoryCreated, vmDirectoryPreExisting, vmDirectory, vhdCreated, vhdPreExisting, vhdPath, CancellationToken.None);
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_CREATE_FAILED",
                $"VM '{request.Name}' creation failed.",
                ex.Message,
                true);
        }
    }

    private static void CreateVirtualHardDisk(ManagementScope scope, string vhdPath, int diskGb, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (File.Exists(vhdPath))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VHD_ALREADY_EXISTS",
                $"VHD '{vhdPath}' already exists.",
                "Choose an empty VM root or remove the existing disk file.",
                false);
        }

        using var diskClass = new ManagementClass(scope, new ManagementPath(VirtualHardDiskSettingClass), null);
        using var diskSettings = diskClass.CreateInstance();
        diskSettings["Path"] = vhdPath;
        diskSettings["Format"] = 3;
        diskSettings["Type"] = 3;
        diskSettings["MaxInternalSize"] = Convert.ToUInt64(diskGb, CultureInfo.InvariantCulture) * 1024UL * 1024UL * 1024UL;

        using var service = GetService(scope, ImageManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(CreateVirtualHardDiskMethod);
        inParams["VirtualDiskSettingData"] = diskSettings.GetText(TextFormat.WmiDtd20);
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(CreateVirtualHardDiskMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.create.vhd", cancellationToken);
    }

    private static void DefineVirtualMachine(ManagementScope scope, DesktopNodeHyperVVmCreateRequest request, string vmDirectory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var settingsClass = new ManagementClass(scope, new ManagementPath(VirtualSystemSettingClass), null);
        using var settings = settingsClass.CreateInstance();
        settings["ElementName"] = request.Name;
        settings["VirtualSystemType"] = "Microsoft:Hyper-V:System:Realized";
        settings["VirtualSystemSubType"] = $"Microsoft:Hyper-V:SubType:{request.Generation}";
        settings["ConfigurationDataRoot"] = vmDirectory;
        settings["SnapshotDataRoot"] = vmDirectory;
        settings["SuspendDataRoot"] = vmDirectory;
        settings["SwapFileDataRoot"] = vmDirectory;
        settings["Notes"] = new[] { ManagedByNote };

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(DefineSystemMethod);
        inParams["SystemSettings"] = settings.GetText(TextFormat.WmiDtd20);
        inParams["ResourceSettings"] = Array.Empty<string>();
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(DefineSystemMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.create.define", cancellationToken);
    }

    private static void ModifyMemoryAndProcessor(ManagementScope scope, ManagementObject settings, int memoryMb, int cpu, CancellationToken cancellationToken)
    {
        var modifications = new List<string>();
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (item)
            {
                var subtype = GetStringProperty(item, "ResourceSubType");
                if (string.Equals(subtype, "Microsoft:Hyper-V:Memory", StringComparison.OrdinalIgnoreCase))
                {
                    item["VirtualQuantity"] = Convert.ToUInt64(memoryMb, CultureInfo.InvariantCulture);
                    item["Reservation"] = Convert.ToUInt64(memoryMb, CultureInfo.InvariantCulture);
                    item["Limit"] = Convert.ToUInt64(memoryMb, CultureInfo.InvariantCulture);
                    modifications.Add(item.GetText(TextFormat.WmiDtd20));
                }
                else if (string.Equals(subtype, "Microsoft:Hyper-V:Processor", StringComparison.OrdinalIgnoreCase))
                {
                    item["VirtualQuantity"] = Convert.ToUInt64(cpu, CultureInfo.InvariantCulture);
                    item["Reservation"] = 0UL;
                    item["Limit"] = 100000UL;
                    modifications.Add(item.GetText(TextFormat.WmiDtd20));
                }
            }
        }

        if (modifications.Count == 0)
        {
            return;
        }

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifyResourceSettingsMethod);
        inParams["ResourceSettings"] = modifications.ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifyResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.create.resources", cancellationToken);
    }

    private static void EnsureCoreDevices(ManagementScope scope, ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scsiController = FindResource(settings, SyntheticScsiControllerSubtype, cancellationToken);
        if (scsiController is null)
        {
            AddScsiController(scope, settings, cancellationToken);
        }

        using var ethernetPort = FindResource(settings, SyntheticEthernetPortSubtype, cancellationToken);
        if (ethernetPort is null)
        {
            AddSyntheticEthernetPort(scope, settings, cancellationToken);
        }
    }

    private static void AddScsiController(ManagementScope scope, ManagementObject settings, CancellationToken cancellationToken)
    {
        using var controllerClass = new ManagementClass(scope, new ManagementPath(ResourceAllocationSettingClass), null);
        using var controller = controllerClass.CreateInstance();
        controller["ResourceType"] = SyntheticScsiControllerResourceType;
        controller["ResourceSubType"] = SyntheticScsiControllerSubtype;
        controller["VirtualQuantity"] = 1UL;
        controller["VirtualQuantityUnits"] = "count";
        controller["AllocationUnits"] = "count";
        AddSingleResourceSetting(scope, settings, controller, "vm.create.scsi-controller", cancellationToken);
    }

    private static void AddSyntheticEthernetPort(ManagementScope scope, ManagementObject settings, CancellationToken cancellationToken)
    {
        using var port = GetDefaultResourceSetting(scope, SyntheticEthernetPortResourceType, SyntheticEthernetPortSubtype, cancellationToken);
        port["ElementName"] = "Network Adapter";
        port["StaticMacAddress"] = false;
        port["VirtualSystemIdentifiers"] = new[] { $"{{{Guid.NewGuid()}}}" };
        AddSingleResourceSetting(scope, settings, port, "vm.create.ethernet-port", cancellationToken);
    }

    private static void AttachDiskAndDvd(ManagementScope scope, ManagementObject settings, string vhdPath, string isoPath, CancellationToken cancellationToken)
    {
        using var scsiController = FindResource(settings, SyntheticScsiControllerSubtype, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CREATE_STORAGE_CONTROLLER_MISSING",
            "VM storage controller was not found after creation.",
            "The native create adapter could not find a synthetic SCSI controller.",
            true);

        var diskDrivePath = AddDrive(scope, settings, scsiController.Path.Path, "Microsoft:Hyper-V:Synthetic Disk Drive", 17, "0", cancellationToken);
        AddStorage(scope, settings, diskDrivePath, "Microsoft:Hyper-V:Virtual Hard Disk", vhdPath, cancellationToken);

        var dvdDrivePath = AddDrive(scope, settings, scsiController.Path.Path, "Microsoft:Hyper-V:Synthetic DVD Drive", 16, "1", cancellationToken);
        AddStorage(scope, settings, dvdDrivePath, "Microsoft:Hyper-V:Virtual CD/DVD Disk", isoPath, cancellationToken);
    }

    private static string AddDrive(ManagementScope scope, ManagementObject settings, string parentPath, string resourceSubType, ushort resourceType, string addressOnParent, CancellationToken cancellationToken)
    {
        using var driveClass = new ManagementClass(scope, new ManagementPath(ResourceAllocationSettingClass), null);
        using var drive = driveClass.CreateInstance();
        drive["ResourceType"] = resourceType;
        drive["ResourceSubType"] = resourceSubType;
        drive["Parent"] = parentPath;
        drive["AddressOnParent"] = addressOnParent;
        drive["VirtualQuantity"] = 1UL;
        drive["VirtualQuantityUnits"] = "count";
        drive["AllocationUnits"] = "count";

        return AddSingleResourceSetting(scope, settings, drive, $"vm.create.{resourceSubType}", cancellationToken);
    }

    private static void AddStorage(ManagementScope scope, ManagementObject settings, string parentPath, string resourceSubType, string hostResource, CancellationToken cancellationToken)
    {
        using var storageClass = new ManagementClass(scope, new ManagementPath(StorageAllocationSettingClass), null);
        using var storage = storageClass.CreateInstance();
        storage["ResourceType"] = 31;
        storage["ResourceSubType"] = resourceSubType;
        storage["Parent"] = parentPath;
        storage["HostResource"] = new[] { hostResource };
        AddSingleResourceSetting(scope, settings, storage, $"vm.create.{resourceSubType}", cancellationToken);
    }

    private static void AttachDefaultSwitch(ManagementScope scope, ManagementObject settings, CancellationToken cancellationToken)
    {
        using var ethernetPort = FindResource(settings, SyntheticEthernetPortSubtype, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CREATE_NETWORK_PORT_MISSING",
            "VM network adapter was not found after creation.",
            "The native create adapter could not find a synthetic ethernet port.",
            true);
        using var defaultSwitch = FindDefaultSwitch(scope, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_DEFAULT_SWITCH_NOT_FOUND",
            "Default Switch was not found.",
            "Create or enable the Hyper-V Default Switch before creating a VM.",
            false);
        using var connection = FindEthernetConnection(ethernetPort, cancellationToken);
        if (connection is null)
        {
            AddEthernetConnection(scope, settings, ethernetPort.Path.Path, defaultSwitch.Path.Path, cancellationToken);
            return;
        }

        SetEthernetConnectionTarget(connection, defaultSwitch.Path.Path);
        ModifySingleResourceSetting(scope, connection, "vm.create.network", cancellationToken);
    }

    private static void AddEthernetConnection(ManagementScope scope, ManagementObject settings, string portPath, string switchPath, CancellationToken cancellationToken)
    {
        using var connection = GetDefaultResourceSetting(scope, EthernetConnectionResourceType, EthernetConnectionSubtype, cancellationToken);
        connection["Parent"] = portPath;
        SetEthernetConnectionTarget(connection, switchPath);
        AddSingleResourceSetting(scope, settings, connection, "vm.create.network", cancellationToken);
    }

    private static void SetEthernetConnectionTarget(ManagementObject connection, string switchPath)
    {
        connection["HostResource"] = new[] { switchPath };
        connection["LastKnownSwitchName"] = DefaultSwitchName;
        connection["EnabledState"] = 2;
    }

    private static string AddSingleResourceSetting(ManagementScope scope, ManagementObject settings, ManagementObject resource, string operation, CancellationToken cancellationToken)
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

    private static void ModifySingleResourceSetting(ManagementScope scope, ManagementObject resource, string operation, CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifyResourceSettingsMethod);
        inParams["ResourceSettings"] = new[] { resource.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifyResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
    }

    private static ManagementObject GetRealizedSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ManagementObject item in vm.GetRelated("Msvm_VirtualSystemSettingData", "Msvm_SettingsDefineState", null, null, "SettingData", "ManagedElement", false, null))
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
            "VM settings were not found after creation.",
            "The native create adapter could not find realized settings for the VM.",
            true);
    }

    private static ManagementObject? FindResource(ManagementObject settings, string resourceSubType, CancellationToken cancellationToken)
    {
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
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

    // Hyper-V assigns a Generation 2 boot order of network, then disk, then DVD. A VM created
    // with an ISO therefore attempts PXE and an empty VHD before the media the caller supplied,
    // which is the opposite of why an ISO was supplied. Measured on a freshly created VM:
    // BootSourceOrder = [ type 2 MAC(...), type 1 Scsi(0,0), type 1 Scsi(0,1) ].
    // The product owns this ordering rather than depending on an unspecified default.
    private static void ConfigureGen2Firmware(ManagementScope scope, string vmName, CancellationToken cancellationToken)
    {
        using var searcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(
                $"SELECT * FROM {VirtualSystemSettingClass} WHERE ElementName = '{vmName.Replace("'", "''")}'"));
        using var results = searcher.Get();
        foreach (ManagementObject settings in results)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (settings)
            {
                var changed = false;

                if (settings["BootSourceOrder"] is string[] order && order.Length > 0)
                {
                    var dvd = order.FirstOrDefault(entry => IsDvdBootSource(scope, entry, cancellationToken));
                    var reordered = OrderBootSourcesDvdFirst(order, dvd);
                    if (reordered is not null)
                    {
                        settings["BootSourceOrder"] = reordered;
                        changed = true;
                    }
                }

                // Hyper-V defaults a Generation 2 VM to the MicrosoftWindows template, which does
                // not trust the Microsoft third-party UEFI CA. Any Linux shim is then rejected with
                // "The signed image's hash is not allowed (DB)", so the ISO path the caller supplied
                // can only ever boot Windows media. The UEFI CA template keeps Secure Boot enabled
                // and only widens the trusted signers, which matches a create route that accepts an
                // arbitrary ISO. Measured: with this template the same Ubuntu ISO reaches its
                // installer instead of failing signature validation.
                var currentTemplate = settings["SecureBootTemplateId"] as string;
                if (!string.Equals(currentTemplate, UefiCertificateAuthorityTemplateId, StringComparison.OrdinalIgnoreCase))
                {
                    settings["SecureBootTemplateId"] = UefiCertificateAuthorityTemplateId;
                    changed = true;
                }

                if (!changed)
                {
                    return;
                }

                using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
                using var inParams = service.GetMethodParameters(ModifySystemSettingsMethod);
                inParams["SystemSettings"] = settings.GetText(TextFormat.WmiDtd20);
                using var outParams = service.InvokeMethod(ModifySystemSettingsMethod, inParams, null);
                WaitForMethodResult(outParams, "vm.create.gen2-firmware", cancellationToken);
                return;
            }
        }
    }

    // Split out from ConfigureGen2Firmware so the ordering contract is reachable without a WMI
    // scope. Returns null when no write is needed: no DVD entry, or the DVD already boots first.
    internal static string[]? OrderBootSourcesDvdFirst(string[] order, string? dvd)
    {
        if (order.Length == 0 || dvd is null)
        {
            return null;
        }

        if (string.Equals(dvd, order[0], StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // OrderBy is a stable sort, so every non-DVD entry keeps its relative position and the
        // firmware order stays otherwise untouched.
        return order
            .OrderBy(entry => string.Equals(entry, dvd, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ToArray();
    }

    private static bool IsDvdBootSource(ManagementScope scope, string bootSourcePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var source = new ManagementObject(scope, new ManagementPath(bootSourcePath), null);
            source.Get();
            // The DVD drive is the only SCSI boot source the create path puts at address 1.
            return Convert.ToUInt16(source["BootSourceType"], CultureInfo.InvariantCulture) == 1 &&
                (source["FirmwareDevicePath"] as string)?.Contains("Scsi(0,1)", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch (ManagementException)
        {
            return false;
        }
    }

    private static IEnumerable<ManagementObject> EnumerateSettingResources(ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var resources = settings.GetRelated(null, "Msvm_VirtualSystemSettingDataComponent", null, null, "PartComponent", "GroupComponent", false, null);
        foreach (ManagementObject item in resources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private static ManagementObject? FindDefaultSwitch(ManagementScope scope, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM Msvm_VirtualEthernetSwitch"));
        foreach (ManagementObject item in searcher.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var elementName = GetStringProperty(item, "ElementName");
            var name = GetStringProperty(item, "Name");
            if (string.Equals(elementName, DefaultSwitchName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, DefaultSwitchName, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    private static ManagementObject GetDefaultResourceSetting(ManagementScope scope, int resourceType, string resourceSubType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var pools = new ManagementObjectSearcher(
            scope,
            new ObjectQuery($"SELECT * FROM {ResourcePoolClass} WHERE ResourceType = {resourceType} AND ResourceSubType = '{EscapeWqlString(resourceSubType)}'"));

        foreach (ManagementObject pool in pools.Get())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (pool)
            {
                foreach (ManagementObject capability in pool.GetRelated(AllocationCapabilitiesClass))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using (capability)
                    {
                        foreach (ManagementObject relationship in capability.GetRelationships(SettingsDefineCapabilitiesClass))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
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
                                if (string.Equals(subtype, resourceSubType, StringComparison.OrdinalIgnoreCase))
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
            $"Default Hyper-V resource setting '{resourceSubType}' was not found.",
            "The native create adapter could not find the default RASD in Msvm_ResourcePool.",
            true);
    }

    private static string EscapeWqlString(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static void Cleanup(
        ManagementScope scope,
        string vmName,
        bool vmCreated,
        bool vmDirectoryCreated,
        bool vmDirectoryPreExisting,
        string vmDirectory,
        bool vhdCreated,
        bool vhdPreExisting,
        string vhdPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (vmCreated)
        {
            try
            {
                using var vm = FindVm(scope, vmName, cancellationToken);
                if (vm is not null)
                {
                    using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
                    using var inParams = service.GetMethodParameters(DestroySystemMethod);
                    inParams["AffectedSystem"] = vm.Path.Path;
                    cancellationToken.ThrowIfCancellationRequested();
                    using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
                    WaitForMethodResult(outParams, "vm.create.cleanup", cancellationToken);
                }
            }
            catch
            {
                // Best-effort cleanup preserves the primary create failure detail.
            }
        }

        if (vmDirectoryCreated && !vmDirectoryPreExisting && Directory.Exists(vmDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { Directory.Delete(vmDirectory, recursive: true); } catch { }
        }
        else if (vhdCreated && !vhdPreExisting && File.Exists(vhdPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { File.Delete(vhdPath); } catch { }
        }
    }

}
