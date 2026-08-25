using System.IO;
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmMediaProvider : IDesktopNodeHyperVVmMediaProvider
{
    private const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    private const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    private const string SettingDataComponentAssociationClass = "Msvm_VirtualSystemSettingDataComponent";
    private const string StorageAllocationSettingClass = "Msvm_StorageAllocationSettingData";
    private const string ModifyResourceSettingsMethod = "ModifyResourceSettings";

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

        using var dvdDrive = FindDvdDrive(settings, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_DVD_DRIVE_NOT_FOUND",
            $"VM '{request.VmName}' has no virtual DVD drive to {(request.Operation == "vm.attach" ? "attach" : "eject")}.",
            "Attach a virtual DVD drive before using vm.eject or vm.attach. This slice does not create DVD devices.",
            false);

        dvdDrive["HostResource"] = request.Operation == "vm.attach"
            ? new[] { request.IsoPath! }
            : Array.Empty<string>();
        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifyResourceSettingsMethod);
        inParams["ResourceSettings"] = new[] { dvdDrive.GetText(TextFormat.WmiDtd20) };
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifyResourceSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, request.Operation, cancellationToken);

        return request.Operation == "vm.attach"
            ? new DesktopNodeHyperVVmMediaInfo(request.VmName, "attach", request.IsoPath)
            : new DesktopNodeHyperVVmMediaInfo(request.VmName, "eject");
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

    private static ManagementObject? FindDvdDrive(ManagementObject settings, CancellationToken cancellationToken)
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

        foreach (ManagementObject item in related)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subtype = GetStringProperty(item, "ResourceSubType");
            var elementName = GetStringProperty(item, "ElementName");
            if ((subtype?.Contains("DVD", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (elementName?.Contains("DVD", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return item;
            }

            item.Dispose();
        }

        return null;
    }
}
