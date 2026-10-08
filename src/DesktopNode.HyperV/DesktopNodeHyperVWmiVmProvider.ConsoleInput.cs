using System.Globalization;
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVWmiVmProvider
{
    public const string KeyboardClass = "Msvm_Keyboard";

    // vm.console.input: VM 의 Msvm_Keyboard method 를 부르고 ReturnValue 를 돌려준다. VM 이나 keyboard 가 없으면 null.
    public uint? SendConsoleKeyboard(string vmId, string method, object? argument, CancellationToken cancellationToken)
    {
        var scope = CreateScope();
        using var vm = FindVm(scope, vmId, cancellationToken);
        if (vm is null)
        {
            return null;
        }

        using var related = vm.GetRelated(KeyboardClass);
        using var keyboard = related.Cast<ManagementObject>().FirstOrDefault();
        if (keyboard is null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var inParams = argument is null ? null : keyboard.GetMethodParameters(method);
        if (inParams is not null)
        {
            inParams[argument is string ? "asciiText" : "keyCode"] = argument;
        }

        using var outParams = keyboard.InvokeMethod(method, inParams, null);
        return Convert.ToUInt32(outParams["ReturnValue"], CultureInfo.InvariantCulture);
    }
}
