using System.Globalization;
using System.Management;
using static DesktopNode.HyperV.DesktopNodeHyperVWmiCommon;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVWmiVmProvider
{
    public const string KeyboardClass = "Msvm_Keyboard";

    private readonly DesktopNodeHyperVConsoleKeyboardSession consoleKeyboard = new();

    // vm.console.input: 같은 VM은 세션이 순서대로 처리한다. 키보드 경로는 한 번 찾고, 실패하면 한 번만 다시 찾는다.
    public uint? SendConsoleKeyboard(string vmId, string method, object? argument, CancellationToken cancellationToken)
    {
        return consoleKeyboard.Send(
            vmId,
            method,
            argument,
            () => ResolveKeyboardPath(vmId, cancellationToken),
            (path, keyboardMethod, keyboardArgument) => InvokeKeyboardPath(path, keyboardMethod, keyboardArgument, cancellationToken),
            cancellationToken);
    }

    private static string? ResolveKeyboardPath(string vmId, CancellationToken cancellationToken)
    {
        var scope = CreateScope();
        using var vm = FindVm(scope, vmId, cancellationToken);
        if (vm is null)
        {
            return null;
        }

        using var related = vm.GetRelated(KeyboardClass);
        using var keyboard = related.Cast<ManagementObject>().FirstOrDefault();
        var path = keyboard?.Path?.Path;
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    private static uint? InvokeKeyboardPath(string path, string method, object? argument, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var keyboard = new ManagementObject(CreateScope(), new ManagementPath(path), null);
        using var inParams = argument is null ? null : keyboard.GetMethodParameters(method);
        if (inParams is not null)
        {
            inParams[argument is string ? "asciiText" : "keyCode"] = argument;
        }

        using var outParams = keyboard.InvokeMethod(method, inParams, null);
        return Convert.ToUInt32(outParams["ReturnValue"], CultureInfo.InvariantCulture);
    }
}
