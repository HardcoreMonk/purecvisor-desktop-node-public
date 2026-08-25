using System.Globalization;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed class DesktopNodeHyperVWmiVmPowerStateProvider : IDesktopNodeHyperVVmPowerStateProvider
{
    public const string ShutdownComponentClass = "Msvm_ShutdownComponent";

    public const string RequestStateChangeMethod = "RequestStateChange";
    public const string InitiateShutdownMethod = "InitiateShutdown";
    public const ushort EnabledState = 2;
    public const ushort DisabledState = 3;
    public const ushort ResetState = 11;
    public const ushort PausedState = 9;
    public const ushort SavedState = 6;
    public const ushort SavedEnabledState = 32769;
    public const uint Completed = DesktopNodeHyperVWmiCommon.Completed;
    public const uint JobStarted = DesktopNodeHyperVWmiCommon.JobStarted;
    public const uint Failed = 32768;
    public const uint NotSupported = 32770;
    public const uint InvalidState = 32775;
    public const uint SystemNotReady = 32780;

    public DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var action = operation switch
        {
            "vm.start" => "start",
            "vm.shutdown" => "shutdown",
            "vm.poweroff" => "poweroff",
            "vm.restart" => "restart",
            "vm.pause" => "pause",
            "vm.resume" => "resume",
            "vm.save" => "save",
            "vm.resume-saved" => "resume-saved",
            _ => throw new InvalidOperationException("Unexpected VM power-state operation.")
        };

        var scope = CreateScope();
        using var vm = FindVm(scope, vmName, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_FOUND",
            $"VM '{vmName}' was not found.",
            "The VM was not present in the native Hyper-V VM inventory response.",
            false);

        if (operation == "vm.shutdown")
        {
            InvokeGuestShutdown(vm, vmName, cancellationToken);
            return new DesktopNodeHyperVVmPowerStateInfo(vmName, action);
        }

        if (operation == "vm.resume-saved")
        {
            RequireSaved(vm.Properties["EnabledState"]?.Value, vmName);
        }

        var requestedState = operation switch
        {
            "vm.start" => EnabledState,
            "vm.poweroff" => DisabledState,
            "vm.restart" => ResetState,
            "vm.pause" => PausedState,
            "vm.resume" => EnabledState,
            "vm.save" => SavedState,
            "vm.resume-saved" => EnabledState,
            _ => throw new DesktopNodeHyperVNativeOperationException(
                "PCV_OPERATION_NOT_ALLOWED",
                $"Operation '{operation}' is not a native VM power-state operation.",
                "Use vm.start, vm.shutdown, vm.poweroff, vm.restart, vm.pause, vm.resume, vm.save, or vm.resume-saved for this native mutation slice.",
                false)
        };

        using var inParams = vm.GetMethodParameters(RequestStateChangeMethod);
        inParams.Properties["RequestedState"].Value = requestedState;
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = vm.InvokeMethod(RequestStateChangeMethod, inParams, null);
        WaitForMethodResult(outParams, operation, cancellationToken);

        return new DesktopNodeHyperVVmPowerStateInfo(vmName, action);
    }

    internal static void RequireSaved(object? enabledState, string vmName)
    {
        var mapped = MapEnabledState(enabledState);
        if (string.Equals(mapped, "saved", StringComparison.Ordinal))
        {
            return;
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_NOT_SAVED",
            $"VM '{vmName}' is not in Hyper-V Saved state.",
            $"Mapped EnabledState is '{mapped}'. Use vm resume-saved only when the VM is saved; paused VMs use vm resume.",
            false);
    }

    private static void InvokeGuestShutdown(ManagementObject vm, string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var shutdown = FindShutdownComponent(vm, cancellationToken) ?? throw new DesktopNodeHyperVNativeOperationException(
            "PCV_VM_SHUTDOWN_COMPONENT_MISSING",
            $"VM '{vmName}' shutdown integration component was not found.",
            "The native shutdown route requires the Hyper-V shutdown integration component.",
            false);

        using var inParams = shutdown.GetMethodParameters(InitiateShutdownMethod);
        inParams["Force"] = false;
        inParams["Reason"] = "PureCVisor Desktop Node requested VM shutdown.";
        cancellationToken.ThrowIfCancellationRequested();
        using var outParams = shutdown.InvokeMethod(InitiateShutdownMethod, inParams, null);
        WaitForShutdownResult(outParams, vmName, cancellationToken);
    }

    private static ManagementObject? FindShutdownComponent(ManagementObject vm, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ManagementObject item in vm.GetRelated(ShutdownComponentClass))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return item;
        }

        return null;
    }

    private static void WaitForShutdownResult(ManagementBaseObject outParams, string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var returnValue = Convert.ToUInt32(outParams.Properties["ReturnValue"]?.Value, CultureInfo.InvariantCulture);
        if (returnValue is Completed or JobStarted)
        {
            return;
        }

        if (returnValue is Failed or NotSupported or InvalidState or SystemNotReady)
        {
            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_SHUTDOWN_NOT_AVAILABLE",
                $"VM '{vmName}' guest shutdown is not available.",
                $"Msvm_ShutdownComponent.InitiateShutdown returned {returnValue}. The guest must be running with the Hyper-V shutdown integration service available.",
                returnValue is Failed or SystemNotReady);
        }

        throw new DesktopNodeHyperVNativeOperationException(
            "PCV_HYPERV_WMI_METHOD_FAILED",
            "Native Hyper-V WMI operation 'vm.shutdown' failed.",
            $"WMI method returned {returnValue}.",
            true);
    }

}
