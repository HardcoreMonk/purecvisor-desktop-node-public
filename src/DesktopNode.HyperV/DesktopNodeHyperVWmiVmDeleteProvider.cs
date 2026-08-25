using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmDeleteProvider : IDesktopNodeHyperVVmDeleteProvider
{
    public const string VirtualSystemManagementServiceClass = "Msvm_VirtualSystemManagementService";
    public const string DestroySystemMethod = "DestroySystem";

    public DesktopNodeHyperVVmDeleteInfo Invoke(string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scope = CreateScope(connect: true);

        using var vm = FindVm(scope, vmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        using var service = GetService(scope, VirtualSystemManagementServiceClass, cancellationToken);
        using var inParams = service.GetMethodParameters(DestroySystemMethod);
        inParams["AffectedSystem"] = vm.Path.Path;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = service.InvokeMethod(DestroySystemMethod, inParams, null);
        WaitForMethodResult(outParams, "vm.delete", cancellationToken);

        return new DesktopNodeHyperVVmDeleteInfo(vmName, "delete");
    }

}
