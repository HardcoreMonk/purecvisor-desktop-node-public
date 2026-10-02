using System.IO;
using System.Management;
using DesktopNode.Contracts;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmMediaProvider : IDesktopNodeHyperVVmMediaProvider
{
    private const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    private const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    private const string SettingDataComponentAssociationClass = "Msvm_VirtualSystemSettingDataComponent";
    private const string StorageAllocationSettingClass = "Msvm_StorageAllocationSettingData";
    private const string ResourceAllocationSettingClass = "Msvm_ResourceAllocationSettingData";
    private const string ModifyResourceSettingsMethod = "ModifyResourceSettings";
    private const string AddResourceSettingsMethod = "AddResourceSettings";
    private const string RemoveResourceSettingsMethod = "RemoveResourceSettings";
    private const string SyntheticDvdDriveSubtype = "Microsoft:Hyper-V:Synthetic DVD Drive";
    private const string VirtualDvdDiskSubtype = "Microsoft:Hyper-V:Virtual CD/DVD Disk";

    public DesktopNodeHyperVVmMediaInfo Invoke(
        DesktopNodeHyperVVmMediaRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.Operation is not ("vm.eject" or "vm.attach"))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_OPERATION_NOT_ALLOWED",
                $"Operation '{request.Operation}' is not a native VM media operation.",
                "Use vm.eject or vm.attach for this native media mutation slice.",
                false);
        }

        if (request.Operation == "vm.attach" && string.IsNullOrWhiteSpace(request.IsoPath))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_ATTACH_ISO_REQUIRED",
                "VM attach requires iso_path.",
                "Pass a JSON body with iso_path set to an existing host ISO file.",
                false);
        }

        if (request.Operation == "vm.attach" && !File.Exists(request.IsoPath!))
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_ISO_NOT_FOUND",
                $"ISO '{request.IsoPath}' was not found.",
                "Use an absolute path to an ISO that exists on this Hyper-V host.",
                false);
        }

        var scope = CreateScope();
        using var vm = FindVm(scope, request.VmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{request.VmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            $"VM '{request.VmName}' settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);

        using var drive = FindDvdDriveRasd(settings, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_DVD_DRIVE_NOT_FOUND",
            $"VM '{request.VmName}' has no virtual DVD drive to {(request.Operation == "vm.attach" ? "attach" : "eject")}.",
            "Attach a virtual DVD drive before using vm.eject or vm.attach. This slice does not create DVD devices.",
            false);
        using var media = FindDvdMedia(settings, drive.Path.Path, cancellationToken);
        switch (DesktopNodeHyperVDvdMediaPlan.Decide(media is not null, request.Operation))
        {
            case DesktopNodeHyperVDvdMediaPlan.Action.EjectAlreadyEmpty:
                return new DesktopNodeHyperVVmMediaInfo(request.VmName, "eject");
            case DesktopNodeHyperVDvdMediaPlan.Action.EjectRemoveMedia:
                RemoveMedia(scope, media!, cancellationToken);
                return new DesktopNodeHyperVVmMediaInfo(request.VmName, "eject");
            case DesktopNodeHyperVDvdMediaPlan.Action.AttachModifyMedia:
                media!["HostResource"] = new[] { request.IsoPath! };
                ModifyMedia(scope, media, request.Operation, cancellationToken);
                return new DesktopNodeHyperVVmMediaInfo(request.VmName, "attach", request.IsoPath);
            case DesktopNodeHyperVDvdMediaPlan.Action.AttachAddMedia:
                AddMedia(scope, settings, drive.Path.Path, request.IsoPath!, cancellationToken);
                return new DesktopNodeHyperVVmMediaInfo(request.VmName, "attach", request.IsoPath);
            default:
                throw new DesktopNodeHyperVNativeOperationException(
                    "PCV_OPERATION_NOT_ALLOWED",
                    $"Operation '{request.Operation}' is not a native VM media operation.",
                    "Use vm.eject or vm.attach for this native media mutation slice.",
                    false);
        }
    }

    public DesktopNodeHyperVVmDeviceAddInfo AddDvd(
        DesktopNodeHyperVVmDeviceAddRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var vm = FindVm(scope, request.VmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{request.VmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);
        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            $"VM '{request.VmName}' settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);
        using var existing = FindDvdDriveRasd(settings, cancellationToken);
        if (existing is not null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                VmDeviceAddProblemCodes.AlreadyPresent,
                $"VM '{request.VmName}' already has a virtual DVD drive.",
                "Use vm.attach to change ISO media. P2-15 does not add a second DVD drive.",
                false);
        }

        using var controller = FindScsiController(settings, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_HYPERV_WMI_RESOURCE_MISSING",
            $"VM '{request.VmName}' has no synthetic SCSI controller.",
            "A Generation 2 SCSI controller is required before adding a DVD drive.",
            true);
        using var driveClass = new ManagementClass(scope, new ManagementPath(ResourceAllocationSettingClass), null);
        using var drive = driveClass.CreateInstance();
        drive["ResourceType"] = 16;
        drive["ResourceSubType"] = SyntheticDvdDriveSubtype;
        drive["Parent"] = controller.Path.Path;
        drive["AddressOnParent"] = "1";
        drive["ElementName"] = "DVD Drive";
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(AddResourceSettingsMethod);
        inParams["AffectedConfiguration"] = settings.Path.Path;
        inParams["ResourceSettings"] = new[] { drive.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(AddResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.dvd.add", cancellationToken);
        return new DesktopNodeHyperVVmDeviceAddInfo("dvd-add", request.VmName, VmDeviceAddPolicy.KindDvd, null);
    }

    private static void RemoveMedia(ManagementScope scope, ManagementObject media, CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(RemoveResourceSettingsMethod);
        inParams["ResourceSettings"] = new[] { media.Path.Path };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(RemoveResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.eject", cancellationToken);
    }

    private static void ModifyMedia(ManagementScope scope, ManagementObject media, string operation, CancellationToken cancellationToken)
    {
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifyResourceSettingsMethod);
        inParams["ResourceSettings"] = new[] { media.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifyResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);
    }

    private static void AddMedia(
        ManagementScope scope,
        ManagementObject settings,
        string drivePath,
        string isoPath,
        CancellationToken cancellationToken)
    {
        using var storageClass = new ManagementClass(scope, new ManagementPath(StorageAllocationSettingClass), null);
        using var storage = storageClass.CreateInstance();
        storage["ResourceType"] = 31;
        storage["ResourceSubType"] = VirtualDvdDiskSubtype;
        storage["Parent"] = drivePath;
        storage["HostResource"] = new[] { isoPath };
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(AddResourceSettingsMethod);
        inParams["AffectedConfiguration"] = settings.Path.Path;
        inParams["ResourceSettings"] = new[] { storage.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(AddResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.attach", cancellationToken);
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

    private static ManagementObject? FindDvdDriveRasd(ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = settings.GetRelated(
            ResourceAllocationSettingClass,
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
            var subtype = GetStringProperty(item, "ResourceSubType");
            if (string.Equals(subtype, SyntheticDvdDriveSubtype, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }

    private static ManagementObject? FindDvdMedia(ManagementObject settings, string drivePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = settings.GetRelated(
            StorageAllocationSettingClass,
            SettingDataComponentAssociationClass,
            relationshipQualifier: null,
            relatedQualifier: null,
            relatedRole: "PartComponent",
            thisRole: "GroupComponent",
            classDefinitionsOnly: false,
            options: null);

        ManagementObject? sole = null;
        var dvdCount = 0;
        foreach (ManagementObject item in related)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subtype = GetStringProperty(item, "ResourceSubType");
            if (!string.Equals(subtype, VirtualDvdDiskSubtype, StringComparison.OrdinalIgnoreCase))
            {
                item.Dispose();
                continue;
            }

            var parent = GetStringProperty(item, "Parent");
            if (string.Equals(parent, drivePath, StringComparison.OrdinalIgnoreCase))
            {
                sole?.Dispose();
                return item;
            }

            dvdCount++;
            if (sole is null)
            {
                sole = item;
                continue;
            }

            item.Dispose();
        }

        if (dvdCount == 1)
        {
            return sole;
        }

        sole?.Dispose();
        if (dvdCount > 1)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_HYPERV_WMI_RESOURCE_MISSING",
                "More than one virtual DVD disk was found, and none is parented to the DVD drive.",
                "Keep a single DVD disk on the drive, then retry vm.eject or vm.attach.",
                true);
        }

        return null;
    }

    private static ManagementObject? FindScsiController(ManagementObject settings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var related = settings.GetRelated(
            ResourceAllocationSettingClass,
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
            var subtype = GetStringProperty(item, "ResourceSubType");
            if (subtype?.Contains("SCSI Controller", StringComparison.OrdinalIgnoreCase) == true)
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }
}
