using System.Globalization;
using System.Management;
using System.Xml;
using System.Xml.Linq;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmCloneProvider : IDesktopNodeHyperVVmCloneProvider
{
    private const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    private const string ImageManagementServiceClass = "Msvm_ImageManagementService";
    private const string ResourcePoolClass = "Msvm_ResourcePool";
    private const string AllocationCapabilitiesClass = "Msvm_AllocationCapabilities";
    private const string SettingsDefineCapabilitiesClass = "Msvm_SettingsDefineCapabilities";
    private const string VirtualSystemSettingClass = "Msvm_VirtualSystemSettingData";
    private const string ResourceAllocationSettingClass = "Msvm_ResourceAllocationSettingData";
    private const string StorageAllocationSettingClass = "Msvm_StorageAllocationSettingData";
    private const string EthernetPortAllocationSettingClass = "Msvm_EthernetPortAllocationSettingData";
    private const string SecuritySettingClass = "Msvm_SecuritySettingData";
    private const string SnapshotAssociationClass = "Msvm_SnapshotOfVirtualSystem";
    private const string GetVirtualHardDiskSettingDataMethod = "GetVirtualHardDiskSettingData";
    private const string DefineSystemMethod = "DefineSystem";
    private const string DestroySystemMethod = "DestroySystem";
    private const string AddResourceSettingsMethod = "AddResourceSettings";
    private const string ModifyResourceSettingsMethod = "ModifyResourceSettings";
    private const string ModifySystemSettingsMethod = "ModifySystemSettings";
    private const string UefiCertificateAuthorityTemplateId = "272e7447-90a4-4563-a4b9-8e4ab00526ce";
    private const string DefaultSwitchName = "Default Switch";
    private const string SyntheticScsiControllerSubtype = "Microsoft:Hyper-V:Synthetic SCSI Controller";
    private const string SyntheticEthernetPortSubtype = "Microsoft:Hyper-V:Synthetic Ethernet Port";
    private const string EthernetConnectionSubtype = "Microsoft:Hyper-V:Ethernet Connection";
    private const string VirtualHardDiskSubtype = "Microsoft:Hyper-V:Virtual Hard Disk";
    private const string VirtualDvdDiskSubtype = "Microsoft:Hyper-V:Virtual CD/DVD Disk";
    private const string SyntheticDiskDriveSubtype = "Microsoft:Hyper-V:Synthetic Disk Drive";
    private const string SyntheticDvdDriveSubtype = "Microsoft:Hyper-V:Synthetic DVD Drive";
    private const int SyntheticScsiControllerResourceType = 6;
    private const int SyntheticEthernetPortResourceType = 10;
    private const int EthernetConnectionResourceType = 33;
    private const int DifferencingVirtualHardDiskType = 4;

    public DesktopNodeHyperVVmClonePlan Preview(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lookup = LookupFromWmi(request, cancellationToken);
        if (lookup.Source is null)
        {
            throw VmNotFound(request.SourceName);
        }

        return PreviewFromSnapshot(lookup.Source, request, lookup.TargetExists);
    }

    public DesktopNodeHyperVVmCloneInfo Invoke(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lookup = LookupFromWmi(request, cancellationToken);
        if (lookup.Source is null)
        {
            throw VmNotFound(request.SourceName);
        }

        return InvokeCopyFromSnapshot(
            lookup.Source,
            request,
            lookup.TargetExists,
            cancellationToken,
            (plan, copied, token) => DefineClonedSystem(request, plan, copied, token));
    }

    internal static DesktopNodeHyperVVmClonePlan PreviewFromSnapshot(
        DesktopNodeHyperVVmCloneSourceSnapshot source,
        DesktopNodeHyperVVmCloneRequest request,
        bool targetExists)
    {
        if (!DesktopNodeHyperVVmCloneGuard.TryPlan(source, request, targetExists, out var plan, out var error))
        {
            throw error!;
        }

        EnsureCloneTargetIsSafe(request.VmRoot, request.TargetName, plan.Directory);
        return plan;
    }

    internal static DesktopNodeHyperVVmCloneInfo InvokeCopyFromSnapshot(
        DesktopNodeHyperVVmCloneSourceSnapshot source,
        DesktopNodeHyperVVmCloneRequest request,
        bool targetExists,
        CancellationToken cancellationToken,
        Action<DesktopNodeHyperVVmClonePlan, IReadOnlyList<string>, CancellationToken>? afterCopy)
    {
        var plan = PreviewFromSnapshot(source, request, targetExists);
        var directoryPreExisting = Directory.Exists(plan.Directory);
        IReadOnlyList<string> copied = [];
        try
        {
            copied = CopyPlannedDisks(plan, request.VmRoot, cancellationToken);
            afterCopy?.Invoke(plan, copied, cancellationToken);
            return new DesktopNodeHyperVVmCloneInfo(
                request.SourceName,
                request.TargetName,
                "clone",
                plan.Directory,
                copied);
        }
        catch
        {
            TryRollbackCloneArtifacts(request.VmRoot, plan.Directory, directoryPreExisting, copied);
            throw;
        }
    }

    internal static IReadOnlyList<string> CopyPlannedDisks(
        DesktopNodeHyperVVmClonePlan plan,
        string vmRoot,
        CancellationToken cancellationToken)
    {
        EnsureCloneTargetIsSafe(vmRoot, plan.Name, plan.Directory);
        var directoryPreExisting = Directory.Exists(plan.Directory);
        var createdFiles = new List<string>();
        try
        {
            Directory.CreateDirectory(plan.Directory);
            var copied = new List<string>(plan.Disks.Count);
            foreach (var disk in plan.Disks)
            {
                if (!File.Exists(disk.Target))
                {
                    createdFiles.Add(disk.Target);
                }

                CopyVhdx(disk.Source, disk.Target, cancellationToken);
                copied.Add(disk.Target);
            }

            return copied;
        }
        catch
        {
            TryRollbackCloneArtifacts(vmRoot, plan.Directory, directoryPreExisting, createdFiles);
            throw;
        }
    }

    internal static void CopyVhdx(string source, string target, CancellationToken cancellationToken)
    {
        using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        var buffer = new byte[1024 * 1024];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            output.Write(buffer, 0, read);
        }
    }

    internal static void EnsureCloneTargetIsSafe(string vmRoot, string targetName, string directory)
    {
        if (string.IsNullOrWhiteSpace(targetName) ||
            DesktopNodeHyperVVmCloneGuard.IsReservedCloneTargetName(targetName) ||
            !DesktopNodeHyperVVmCloneGuard.IsContainedCloneDirectory(vmRoot, directory))
        {
            throw DesktopNodeHyperVVmCloneGuard.InvalidCloneTargetName(targetName);
        }
    }

    internal static void TryRollbackCloneArtifacts(
        string vmRoot,
        string directory,
        bool directoryPreExisting,
        IReadOnlyList<string> createdFiles)
    {
        foreach (var file in createdFiles)
        {
            TryDeleteCreatedFile(vmRoot, file);
        }

        if (directoryPreExisting ||
            string.IsNullOrWhiteSpace(directory) ||
            !DesktopNodeHyperVVmCloneGuard.IsContainedCloneDirectory(vmRoot, directory) ||
            !Directory.Exists(directory))
        {
            return;
        }

        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch
        {
        }
    }

    internal static DesktopNodeHyperVNativeOperationException SecurityFeaturesInspectFailed()
    {
        return new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED",
            "Clone security feature inspect failed.",
            "Hyper-V security settings could not be read. This clone path does not copy security key material.",
            false);
    }

    internal static DesktopNodeHyperVNativeOperationException CheckpointsInspectFailed()
    {
        return new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CLONE_CHECKPOINTS_PRESENT",
            "Clone checkpoint inspect failed.",
            "Hyper-V checkpoints could not be read. Flatten is not supported.",
            false);
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    internal static void ThrowIfSecurityInspectFailed()
    {
        throw SecurityFeaturesInspectFailed();
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    internal static void ThrowIfCheckpointInspectFailed()
    {
        throw CheckpointsInspectFailed();
    }

    private static void TryDeleteCreatedFile(string vmRoot, string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return;
        }

        try
        {
            var fullFile = Path.GetFullPath(file);
            if (!File.Exists(fullFile) || !DesktopNodeHyperVVmCloneGuard.IsContainedCloneDirectory(vmRoot, fullFile))
            {
                return;
            }

            File.Delete(fullFile);
        }
        catch
        {
        }
    }

    internal static string MapClonePowerState(object? enabledState)
    {
        var mapped = MapEnabledState(enabledState);
        return string.Equals(mapped, "stopped", StringComparison.Ordinal) ? "Off" : mapped;
    }

    internal static bool IsIndependentVhdx(string path, string? parentPath, int? diskType)
    {
        return !string.IsNullOrWhiteSpace(path) &&
            string.Equals(Path.GetExtension(path), ".vhdx", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(parentPath) &&
            diskType != DifferencingVirtualHardDiskType;
    }

    private readonly record struct CloneLookup(
        DesktopNodeHyperVVmCloneSourceSnapshot? Source,
        bool TargetExists);

    private static CloneLookup LookupFromWmi(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope(connect: true);
        using var sourceVm = FindVm(scope, request.SourceName, cancellationToken);
        using var targetVm = FindVm(scope, request.TargetName, cancellationToken);
        var targetExists = targetVm is not null;
        if (sourceVm is null)
        {
            return new CloneLookup(null, targetExists);
        }

        return new CloneLookup(
            ReadSourceSnapshot(scope, sourceVm, request.SourceName, cancellationToken),
            targetExists);
    }

    private static DesktopNodeHyperVVmCloneSourceSnapshot ReadSourceSnapshot(
        ManagementScope scope,
        ManagementObject sourceVm,
        string sourceName,
        CancellationToken cancellationToken)
    {
        using var settings = GetRealizedSettings(sourceVm, cancellationToken);
        var disks = ReadDiskSnapshots(scope, settings, cancellationToken);
        return new DesktopNodeHyperVVmCloneSourceSnapshot(
            sourceName,
            DesktopNodeHyperVManagedNotes.IsManagedNotes(GetStringProperty(settings, "Notes")),
            MapCloneGeneration(GetStringProperty(settings, "VirtualSystemSubType")),
            MapClonePowerState(sourceVm.Properties["EnabledState"]?.Value),
            CountCheckpoints(sourceVm, cancellationToken),
            disks,
            SecurityFeaturesPresent(settings, cancellationToken));
    }

    private static IReadOnlyList<DesktopNodeHyperVVmCloneDiskSnapshot> ReadDiskSnapshots(
        ManagementScope scope,
        ManagementObject settings,
        CancellationToken cancellationToken)
    {
        var disks = new List<DesktopNodeHyperVVmCloneDiskSnapshot>();
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
            using (item)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.Equals(GetStringProperty(item, "ResourceSubType"), VirtualHardDiskSubtype, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var path = GetFirstHostResource(item);
                if (string.IsNullOrWhiteSpace(path))
                {
                    disks.Add(new DesktopNodeHyperVVmCloneDiskSnapshot(string.Empty, 0, false));
                    continue;
                }

                var fileLength = File.Exists(path) ? new FileInfo(path).Length : 0L;
                disks.Add(new DesktopNodeHyperVVmCloneDiskSnapshot(
                    path,
                    fileLength,
                    TryReadIndependentVhdx(scope, path, cancellationToken)));
            }
        }

        return disks;
    }

    private static bool TryReadIndependentVhdx(ManagementScope scope, string path, CancellationToken cancellationToken)
    {
        if (!IsIndependentVhdx(path, parentPath: null, diskType: null))
        {
            return false;
        }

        try
        {
            using var service = GetService(scope, ImageManagementServiceClass, cancellationToken);
            using var inParams = service.GetMethodParameters(GetVirtualHardDiskSettingDataMethod);
            inParams["Path"] = path;
            cancellationToken.ThrowIfCancellationRequested();
            using var outParams = service.InvokeMethod(GetVirtualHardDiskSettingDataMethod, inParams, null);
            WaitForMethodResult(outParams, "vm.clone.disk-inspect", cancellationToken);
            var settingData = Convert.ToString(outParams["SettingData"], CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(settingData))
            {
                return false;
            }

            var (parentPath, diskType) = ReadVirtualHardDiskIdentity(settingData);
            return IsIndependentVhdx(path, parentPath, diskType);
        }
        catch (DesktopNodeHyperVNativeOperationException)
        {
            return false;
        }
        catch (ManagementException)
        {
            return false;
        }
    }

    private static (string? ParentPath, int? Type) ReadVirtualHardDiskIdentity(string settingData)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        using var textReader = new StringReader(settingData);
        using var xmlReader = XmlReader.Create(textReader, settings);
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        var parentPath = ReadCimProperty(document, "ParentPath");
        var typeText = ReadCimProperty(document, "Type");
        int? diskType = int.TryParse(typeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
        return (string.IsNullOrWhiteSpace(parentPath) ? null : parentPath, diskType);
    }

    private static string? ReadCimProperty(XDocument document, string propertyName)
    {
        var property = document
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName.Equals("PROPERTY", StringComparison.OrdinalIgnoreCase) &&
                string.Equals((string?)element.Attribute("NAME"), propertyName, StringComparison.OrdinalIgnoreCase));
        return property?
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName.Equals("VALUE", StringComparison.OrdinalIgnoreCase))?
            .Value;
    }

    private static bool SecurityFeaturesPresent(ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var related = settings.GetRelated(SecuritySettingClass);
            foreach (ManagementObject item in related)
            {
                using (item)
                {
                    if (IsEnabledFlag(item, "TpmEnabled") ||
                        IsEnabledFlag(item, "ShieldingRequested") ||
                        IsEnabledFlag(item, "EncryptStateAndVmMigrationTraffic") ||
                        IsEnabledFlag(item, "DataProtectionRequested") ||
                        !string.IsNullOrWhiteSpace(GetStringProperty(item, "KeyProtector")))
                    {
                        return true;
                    }
                }
            }
        }
        catch (ManagementException)
        {
            throw SecurityFeaturesInspectFailed();
        }
        catch (UnauthorizedAccessException)
        {
            throw SecurityFeaturesInspectFailed();
        }

        return false;
    }

    private static bool IsEnabledFlag(ManagementBaseObject item, string propertyName)
    {
        try
        {
            var value = item.Properties[propertyName]?.Value;
            return value is not null && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }
        catch (ManagementException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    private static int CountCheckpoints(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var snapshots = vm.GetRelated(
                VirtualSystemSettingClass,
                SnapshotAssociationClass,
                relationshipQualifier: null,
                relatedQualifier: null,
                relatedRole: "Dependent",
                thisRole: "Antecedent",
                classDefinitionsOnly: false,
                options: null);

            var count = 0;
            foreach (ManagementObject snapshot in snapshots)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (snapshot)
                {
                    count++;
                }
            }

            return count;
        }
        catch (ManagementException)
        {
            throw CheckpointsInspectFailed();
        }
        catch (UnauthorizedAccessException)
        {
            throw CheckpointsInspectFailed();
        }
    }

    private static int MapCloneGeneration(string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype))
        {
            return 0;
        }

        var trimmed = subtype.Trim();
        if (trimmed.EndsWith(":2", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (trimmed.EndsWith(":1", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    private static void DefineClonedSystem(
        DesktopNodeHyperVVmCloneRequest request,
        DesktopNodeHyperVVmClonePlan plan,
        IReadOnlyList<string> copiedDisks,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope(connect: true);
        using var sourceVm = FindVm(scope, request.SourceName, cancellationToken) ?? throw VmNotFound(request.SourceName);
        using var sourceSettings = GetRealizedSettings(sourceVm, cancellationToken);
        var memoryMb = ReadMemoryMb(sourceSettings, cancellationToken);
        var cpu = ReadCpu(sourceSettings, cancellationToken);
        var switchName = ReadSwitchName(sourceSettings, cancellationToken) ?? DefaultSwitchName;
        var isoPath = ReadExistingIsoPath(sourceSettings, cancellationToken);
        var vmCreated = false;

        try
        {
            DefineVirtualMachine(scope, request.TargetName, plan.Directory, cancellationToken);
            vmCreated = true;

            using var vm = FindVm(scope, request.TargetName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_LOOKUP_FAILED",
                $"VM '{request.TargetName}' lookup failed.",
                "The VM was not visible after DefineSystem completed.",
                true);
            using var settings = GetRealizedSettings(vm, cancellationToken);
            EnsureCoreDevices(scope, settings, cancellationToken);
            ModifyMemoryAndProcessor(scope, settings, memoryMb, cpu, cancellationToken);
            AttachDisksAndDvd(scope, settings, copiedDisks, isoPath, cancellationToken);
            AttachSwitch(scope, settings, switchName, cancellationToken);
            ConfigureGen2Firmware(scope, request.TargetName, cancellationToken);
        }
        catch
        {
            if (vmCreated)
            {
                TryDestroyTargetVm(scope, request.TargetName, CancellationToken.None);
            }

            throw;
        }
    }

    private static void DefineVirtualMachine(
        ManagementScope scope,
        string vmName,
        string vmDirectory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var settingsClass = new ManagementClass(scope, new ManagementPath(VirtualSystemSettingClass), null);
        using var settings = settingsClass.CreateInstance();
        settings["ElementName"] = vmName;
        settings["VirtualSystemType"] = "Microsoft:Hyper-V:System:Realized";
        settings["VirtualSystemSubType"] = "Microsoft:Hyper-V:SubType:2";
        settings["ConfigurationDataRoot"] = vmDirectory;
        settings["SnapshotDataRoot"] = vmDirectory;
        settings["SuspendDataRoot"] = vmDirectory;
        settings["SwapFileDataRoot"] = vmDirectory;
        settings["Notes"] = new[] { DesktopNodeHyperVManagedNotes.Marker };

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(DefineSystemMethod);
        inParams["SystemSettings"] = settings.GetText(TextFormat.WmiDtd20);
        inParams["ResourceSettings"] = Array.Empty<string>();
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(DefineSystemMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.clone.define", cancellationToken);
    }

    private static void ModifyMemoryAndProcessor(
        ManagementScope scope,
        ManagementObject settings,
        int memoryMb,
        int cpu,
        CancellationToken cancellationToken)
    {
        var modifications = new List<string>();
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
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
        WaitForMethodResult(outParams, "vm.clone.resources", cancellationToken);
    }

    private static void EnsureCoreDevices(ManagementScope scope, ManagementObject settings, CancellationToken cancellationToken)
    {
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
        AddSingleResourceSetting(scope, settings, controller, "vm.clone.scsi-controller", cancellationToken);
    }

    private static void AddSyntheticEthernetPort(ManagementScope scope, ManagementObject settings, CancellationToken cancellationToken)
    {
        using var port = GetDefaultResourceSetting(scope, SyntheticEthernetPortResourceType, SyntheticEthernetPortSubtype, cancellationToken);
        port["ElementName"] = "Network Adapter";
        port["StaticMacAddress"] = false;
        port["VirtualSystemIdentifiers"] = new[] { $"{{{Guid.NewGuid()}}}" };
        AddSingleResourceSetting(scope, settings, port, "vm.clone.ethernet-port", cancellationToken);
    }

    private static void AttachDisksAndDvd(
        ManagementScope scope,
        ManagementObject settings,
        IReadOnlyList<string> copiedDisks,
        string? isoPath,
        CancellationToken cancellationToken)
    {
        using var scsiController = FindResource(settings, SyntheticScsiControllerSubtype, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CREATE_STORAGE_CONTROLLER_MISSING",
            "VM storage controller was not found after creation.",
            "The native clone adapter could not find a synthetic SCSI controller.",
            true);

        for (var index = 0; index < copiedDisks.Count; index++)
        {
            var diskDrivePath = AddDrive(
                scope,
                settings,
                scsiController.Path.Path,
                SyntheticDiskDriveSubtype,
                17,
                index.ToString(CultureInfo.InvariantCulture),
                cancellationToken);
            AddStorage(scope, settings, diskDrivePath, VirtualHardDiskSubtype, copiedDisks[index], cancellationToken);
        }

        var dvdAddress = copiedDisks.Count.ToString(CultureInfo.InvariantCulture);
        var dvdDrivePath = AddDrive(
            scope,
            settings,
            scsiController.Path.Path,
            SyntheticDvdDriveSubtype,
            16,
            dvdAddress,
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(isoPath))
        {
            AddStorage(scope, settings, dvdDrivePath, VirtualDvdDiskSubtype, isoPath, cancellationToken);
        }
    }

    private static string AddDrive(
        ManagementScope scope,
        ManagementObject settings,
        string parentPath,
        string resourceSubType,
        ushort resourceType,
        string addressOnParent,
        CancellationToken cancellationToken)
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
        return AddSingleResourceSetting(scope, settings, drive, $"vm.clone.{resourceSubType}", cancellationToken);
    }

    private static void AddStorage(
        ManagementScope scope,
        ManagementObject settings,
        string parentPath,
        string resourceSubType,
        string hostResource,
        CancellationToken cancellationToken)
    {
        using var storageClass = new ManagementClass(scope, new ManagementPath(StorageAllocationSettingClass), null);
        using var storage = storageClass.CreateInstance();
        storage["ResourceType"] = 31;
        storage["ResourceSubType"] = resourceSubType;
        storage["Parent"] = parentPath;
        storage["HostResource"] = new[] { hostResource };
        AddSingleResourceSetting(scope, settings, storage, $"vm.clone.{resourceSubType}", cancellationToken);
    }

    private static void AttachSwitch(
        ManagementScope scope,
        ManagementObject settings,
        string switchName,
        CancellationToken cancellationToken)
    {
        using var ethernetPort = FindResource(settings, SyntheticEthernetPortSubtype, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CREATE_NETWORK_PORT_MISSING",
            "VM network adapter was not found after creation.",
            "The native clone adapter could not find a synthetic ethernet port.",
            true);
        using var virtualSwitch = FindSwitch(scope, switchName, cancellationToken) ??
            FindSwitch(scope, DefaultSwitchName, cancellationToken) ??
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_DEFAULT_SWITCH_NOT_FOUND",
                "Default Switch was not found.",
                "Create or enable the Hyper-V Default Switch before cloning a VM.",
                false);

        using var connection = FindEthernetConnection(ethernetPort, cancellationToken);
        var targetSwitchPath = virtualSwitch.Path.Path;
        var targetSwitchName = GetStringProperty(virtualSwitch, "ElementName") ?? DefaultSwitchName;
        if (connection is null)
        {
            AddEthernetConnection(scope, settings, ethernetPort.Path.Path, targetSwitchPath, targetSwitchName, cancellationToken);
            return;
        }

        SetEthernetConnectionTarget(connection, targetSwitchPath, targetSwitchName);
        ModifySingleResourceSetting(scope, connection, "vm.clone.network", cancellationToken);
    }

    private static void AddEthernetConnection(
        ManagementScope scope,
        ManagementObject settings,
        string portPath,
        string switchPath,
        string switchName,
        CancellationToken cancellationToken)
    {
        using var connection = GetDefaultResourceSetting(scope, EthernetConnectionResourceType, EthernetConnectionSubtype, cancellationToken);
        connection["Parent"] = portPath;
        SetEthernetConnectionTarget(connection, switchPath, switchName);
        AddSingleResourceSetting(scope, settings, connection, "vm.clone.network", cancellationToken);
    }

    private static void SetEthernetConnectionTarget(ManagementObject connection, string switchPath, string switchName)
    {
        connection["HostResource"] = new[] { switchPath };
        connection["LastKnownSwitchName"] = switchName;
        connection["EnabledState"] = 2;
    }

    private static void ConfigureGen2Firmware(ManagementScope scope, string vmName, CancellationToken cancellationToken)
    {
        using var searcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(
                $"SELECT * FROM {VirtualSystemSettingClass} WHERE ElementName = '{EscapeWqlString(vmName)}'"));
        using var results = searcher.Get();
        foreach (ManagementObject settings in results)
        {
            using (settings)
            {
                var currentTemplate = settings["SecureBootTemplateId"] as string;
                if (string.Equals(currentTemplate, UefiCertificateAuthorityTemplateId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                settings["SecureBootTemplateId"] = UefiCertificateAuthorityTemplateId;
                using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
                using var inParams = service.GetMethodParameters(ModifySystemSettingsMethod);
                inParams["SystemSettings"] = settings.GetText(TextFormat.WmiDtd20);
                using var outParams = service.InvokeMethod(ModifySystemSettingsMethod, inParams, null);
                WaitForMethodResult(outParams, "vm.clone.gen2-firmware", cancellationToken);
                return;
            }
        }
    }

    private static string AddSingleResourceSetting(
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

    private static ManagementObject GetRealizedSettings(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ManagementObject item in vm.GetRelated(
            VirtualSystemSettingClass,
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
            "The native clone adapter could not find realized settings for the VM.",
            true);
    }

    private static ManagementObject? FindResource(ManagementObject settings, string resourceSubType, CancellationToken cancellationToken)
    {
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
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

    private static IEnumerable<ManagementObject> EnumerateSettingResources(ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var resources = settings.GetRelated(
            null,
            "Msvm_VirtualSystemSettingDataComponent",
            null,
            null,
            "PartComponent",
            "GroupComponent",
            false,
            null);
        foreach (ManagementObject item in resources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    private static int ReadMemoryMb(ManagementObject settings, CancellationToken cancellationToken)
    {
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
            using (item)
            {
                if (string.Equals(GetStringProperty(item, "ResourceSubType"), "Microsoft:Hyper-V:Memory", StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToInt32(item["VirtualQuantity"], CultureInfo.InvariantCulture);
                }
            }
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_MEMORY_SETTINGS_NOT_FOUND",
            "Source VM memory settings were not found.",
            "Msvm_MemorySettingData was not available for the source VM.",
            true);
    }

    private static int ReadCpu(ManagementObject settings, CancellationToken cancellationToken)
    {
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
            using (item)
            {
                if (string.Equals(GetStringProperty(item, "ResourceSubType"), "Microsoft:Hyper-V:Processor", StringComparison.OrdinalIgnoreCase))
                {
                    return Convert.ToInt32(item["VirtualQuantity"], CultureInfo.InvariantCulture);
                }
            }
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_CPU_SETTINGS_NOT_FOUND",
            "Source VM processor settings were not found.",
            "Msvm_ProcessorSettingData was not available for the source VM.",
            true);
    }

    private static string? ReadSwitchName(ManagementObject settings, CancellationToken cancellationToken)
    {
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
            using (item)
            {
                if (string.Equals(GetStringProperty(item, "ResourceSubType"), EthernetConnectionSubtype, StringComparison.OrdinalIgnoreCase))
                {
                    var name = GetStringProperty(item, "LastKnownSwitchName");
                    return string.IsNullOrWhiteSpace(name) ? null : name;
                }
            }
        }

        return null;
    }

    private static string? ReadExistingIsoPath(ManagementObject settings, CancellationToken cancellationToken)
    {
        foreach (var item in EnumerateSettingResources(settings, cancellationToken))
        {
            using (item)
            {
                if (!string.Equals(GetStringProperty(item, "ResourceSubType"), VirtualDvdDiskSubtype, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var path = GetFirstHostResource(item);
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    return path;
                }
            }
        }

        return null;
    }

    private static string? GetFirstHostResource(ManagementBaseObject item)
    {
        try
        {
            var value = item.Properties["HostResource"]?.Value;
            if (value is string[] values)
            {
                return values.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));
            }

            return value as string;
        }
        catch (ManagementException)
        {
            return null;
        }
    }

    private static ManagementObject? FindSwitch(ManagementScope scope, string switchName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM Msvm_VirtualEthernetSwitch"));
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

    private static ManagementObject GetDefaultResourceSetting(
        ManagementScope scope,
        int resourceType,
        string resourceSubType,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var pools = new ManagementObjectSearcher(
            scope,
            new ObjectQuery($"SELECT * FROM {ResourcePoolClass} WHERE ResourceType = {resourceType} AND ResourceSubType = '{EscapeWqlString(resourceSubType)}'"));

        foreach (ManagementObject pool in pools.Get())
        {
            using (pool)
            {
                foreach (ManagementObject capability in pool.GetRelated(AllocationCapabilitiesClass))
                {
                    using (capability)
                    {
                        foreach (ManagementObject relationship in capability.GetRelationships(SettingsDefineCapabilitiesClass))
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
            "The native clone adapter could not find the default RASD in Msvm_ResourcePool.",
            true);
    }

    private static void TryDestroyTargetVm(ManagementScope scope, string vmName, CancellationToken cancellationToken)
    {
        try
        {
            using var vm = FindVm(scope, vmName, cancellationToken);
            if (vm is null)
            {
                return;
            }

            using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
            using var inParams = service.GetMethodParameters(DestroySystemMethod);
            inParams["AffectedSystem"] = vm.Path.Path;
            using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
            WaitForMethodResult(outParams, "vm.clone.cleanup", cancellationToken);
        }
        catch
        {
        }
    }

    private static string EscapeWqlString(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static DesktopNodeHyperVNativeOperationException VmNotFound(string vmName)
    {
        return new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);
    }
}
