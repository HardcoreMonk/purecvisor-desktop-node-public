using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private bool TryInvokeGuestExecution(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "name");
        if (string.IsNullOrWhiteSpace(vmName) || !IsValidHyperVName(vmName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{vmName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name without leading/trailing whitespace, control characters, slash, or backslash.",
                false);
            return true;
        }

        var timeoutSeconds = TryGetInt32Property(parameters, "timeout_sec", out var requestedTimeout)
            ? requestedTimeout
            : 60;
        if (timeoutSeconds is < 1 or > 600)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_GUEST_EXEC_TIMEOUT",
                "Guest execution timeout is outside the supported range.",
                "Use timeout_sec between 1 and 600 seconds.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = guestExecutionProvider.Invoke(
                new DesktopNodeHyperVGuestExecutionRequest(
                    operation,
                    vmName,
                    GetStringProperty(parameters, "credential_ref"),
                    ReadStringList(parameters, "command"),
                    ReadStringDictionary(parameters, "environment"),
                    timeoutSeconds,
                    GetStringProperty(parameters, "request_id"),
                    GetStringProperty(parameters, "actor"),
                    GetBooleanProperty(parameters, "yes") || string.Equals(GetStringProperty(parameters, "mode"), "repair", StringComparison.OrdinalIgnoreCase)),
                cancellationToken);
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(data, JsonOptions),
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
                operation,
                "PCV_GUEST_EXEC_PROVIDER_FAILED",
                $"Guest execution provider failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

}
