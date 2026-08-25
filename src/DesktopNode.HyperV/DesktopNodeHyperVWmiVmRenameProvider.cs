using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmRenameProvider : IDesktopNodeHyperVVmRenameProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string VirtualSystemSettingDataClass = "Msvm_VirtualSystemSettingData";
    public const string ModifySystemSettingsMethod = "ModifySystemSettings";

    public DesktopNodeHyperVVmRenameInfo Invoke(string vmName, string newName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope();
        using var vm = FindVm(scope, vmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        using var existingTarget = FindVm(scope, newName, cancellationToken);
        if (existingTarget is not null)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_RENAME_TARGET_EXISTS",
                $"VM rename target '{newName}' already exists.",
                "Choose a Hyper-V VM display name that is not already in use.",
                false);
        }

        using var settings = FindCurrentSettings(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SETTINGS_NOT_FOUND",
            $"VM '{vmName}' settings were not found.",
            "Msvm_VirtualSystemSettingData was not available for the VM.",
            true);
        settings.Properties["ElementName"].Value = newName;

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(ModifySystemSettingsMethod);
        inParams.Properties["SystemSettings"].Value = settings.GetText(TextFormat.CimDtd20);
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(ModifySystemSettingsMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.rename", cancellationToken);

        using var renamed = FindVm(scope, newName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_RENAME_VERIFY_FAILED",
            $"VM '{vmName}' was not visible as '{newName}' after rename.",
            "Hyper-V accepted the settings mutation but the renamed VM was not visible in inventory.",
            true);

        return new DesktopNodeHyperVVmRenameInfo(vmName, newName, "rename");
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
