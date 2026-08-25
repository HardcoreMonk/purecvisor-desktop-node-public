using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmManageProvider : IDesktopNodeHyperVVmManageProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    public const string ModifySystemSettingsMethod = "ModifySystemSettings";

    public DesktopNodeHyperVVmManageInfo Invoke(string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var vm = FindVm(scope, vmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            $"VM '{vmName}' settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);

        var currentNotes = GetStringProperty(settings, "Notes");
        if (DesktopNodeHyperVManagedNotes.IsManagedNotes(currentNotes))
        {
            return new DesktopNodeHyperVVmManageInfo(vmName, "already-managed");
        }

        settings.Properties["Notes"].Value = DesktopNodeHyperVManagedNotes.AppendManagedMarker(currentNotes)
            .Split(["\r\n", "\n"], StringSplitOptions.None);

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifySystemSettingsMethod);
        inParams.Properties["SystemSettings"].Value = settings.GetText(TextFormat.CimDtd20);
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifySystemSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.manage", cancellationToken);

        return new DesktopNodeHyperVVmManageInfo(vmName, "manage");
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
}
