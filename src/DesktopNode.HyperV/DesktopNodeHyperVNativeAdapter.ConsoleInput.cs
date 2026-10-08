using System.Text.Json;

namespace DesktopNode.HyperV;

// vm.console.input 은 S1 브라우저 콘솔의 키보드 입력이다(설계 pcv-s1-browser-console-v1 §3). Msvm_Keyboard 의
// TypeKey/PressKey/ReleaseKey/TypeText/TypeCtrlAltDel 중 하나를 부른다. 입력 값은 결과에 되돌려 주지 않는다.
public sealed partial class DesktopNodeHyperVNativeAdapter
{
    public const int ConsoleTextMaxLength = 256;

    private bool TryInvokeVmConsoleInput(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "name") ?? GetStringProperty(parameters, "vm_name");
        var plan = PlanConsoleInput(
            GetStringProperty(parameters, "kind"),
            GetStringProperty(parameters, "action"),
            GetIntProperty(parameters, "key_code"),
            GetStringProperty(parameters, "text"));
        if (string.IsNullOrWhiteSpace(vmName) || plan is null)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CONSOLE_INPUT_INVALID",
                "Console input is not valid.",
                "Send kind=key with action type|press|release and key_code 1-254, kind=text with 1-256 printable ASCII characters, or kind=ctrl-alt-del.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var vm = FindVm(vmProvider.GetVms(cancellationToken), vmName);
            if (vm is null)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation, "PCV_VM_NOT_FOUND", $"VM '{vmName}' was not found.", "The VM was not present in the native Hyper-V VM inventory response.", false);
                return true;
            }

            if (!string.Equals(vm.State, "running", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation, "PCV_CONSOLE_VM_NOT_RUNNING", $"VM '{vm.Name}' is {vm.State}, not running.", "Start the VM before sending console input.", false);
                return true;
            }

            var returnValue = vmProvider.SendConsoleKeyboard(vm.Id, plan.Value.Method, plan.Value.Argument, cancellationToken);
            if (returnValue != 0)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_CONSOLE_INPUT_FAILED",
                    "The console input was not delivered.",
                    returnValue is null
                        ? "The VM provider has no keyboard for this VM."
                        : $"Msvm_Keyboard.{plan.Value.Method} returned {returnValue}.",
                    true);
                return true;
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(
                    new SortedDictionary<string, object?>
                    {
                        ["name"] = vm.Name,
                        ["kind"] = plan.Value.Kind,
                        ["action"] = plan.Value.Action,
                        ["accepted"] = true
                    },
                    JsonOptions),
                Error: null);
            return true;
        }
        catch (DesktopNodeHyperVNativeOperationException ex)
        {
            result = DesktopNodeHyperVOperationResult.Failure(operation, ex.Code, ex.Message, ex.Detail, ex.Retryable);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result = CanceledResult(operation);
            return true;
        }
        catch (Exception ex)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation, "PCV_CONSOLE_INPUT_FAILED", "The console input was not delivered.", ex.Message, true);
            return true;
        }
    }

    public static (string Kind, string Action, string Method, object? Argument)? PlanConsoleInput(
        string? kind,
        string? action,
        int? keyCode,
        string? text)
    {
        switch (kind)
        {
            case "key" when keyCode is >= 1 and <= 254:
                return action switch
                {
                    "type" => ("key", "type", "TypeKey", (uint)keyCode.Value),
                    "press" => ("key", "press", "PressKey", (uint)keyCode.Value),
                    "release" => ("key", "release", "ReleaseKey", (uint)keyCode.Value),
                    _ => null
                };
            case "text" when text is { Length: >= 1 and <= ConsoleTextMaxLength } && text.All(character => character is >= ' ' and <= '~'):
                return ("text", "type", "TypeText", text);
            case "ctrl-alt-del":
                return ("ctrl-alt-del", "type", "TypeCtrlAltDel", null);
            default:
                return null;
        }
    }
}
