using System.Text.Json;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private bool TryInvokeVmExport(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "vm_name") ?? GetStringProperty(parameters, "name");
        var directory = GetStringProperty(parameters, "directory");
        var allowedRoot = TryGetStringProperty(parameters, "allowed_root", out var parsedRoot)
            ? parsedRoot
            : DesktopNode.Contracts.VmExportImportPolicy.DefaultExportRoot;
        if (string.IsNullOrWhiteSpace(vmName) || !IsValidHyperVName(vmName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{vmName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = JsonSerializer.SerializeToElement(
                vmExportProvider.Invoke(new DesktopNodeHyperVVmExportRequest(vmName, directory ?? string.Empty, allowedRoot), cancellationToken),
                JsonOptions);
            result = new DesktopNodeHyperVOperationResult(true, operation, data, null);
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
                "PCV_VM_EXPORT_FAILED",
                $"VM export failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmImport(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var targetName = GetStringProperty(parameters, "name") ??
            GetStringProperty(parameters, "vm_name") ??
            GetStringProperty(parameters, "target_name");
        var directory = GetStringProperty(parameters, "directory");
        var allowedRoot = TryGetStringProperty(parameters, "allowed_root", out var parsedRoot)
            ? parsedRoot
            : DesktopNode.Contracts.VmExportImportPolicy.DefaultExportRoot;
        var generateNewId = !TryGetBooleanProperty(parameters, "generate_new_id", out var parsedGenerateNewId) ||
            parsedGenerateNewId;
        var vmRoot = TryGetStringProperty(parameters, "vm_root", out var parsedVmRoot)
            ? parsedVmRoot
            : DesktopNodeHyperVVmImportRequest.DefaultVmRoot;
        if (string.IsNullOrWhiteSpace(targetName) || !IsValidHyperVName(targetName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{targetName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = JsonSerializer.SerializeToElement(
                vmImportProvider.Invoke(
                    new DesktopNodeHyperVVmImportRequest(targetName, directory ?? string.Empty, allowedRoot, generateNewId, vmRoot),
                    cancellationToken),
                JsonOptions);
            result = new DesktopNodeHyperVOperationResult(true, operation, data, null);
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
                "PCV_VM_IMPORT_FAILED",
                $"VM import failed for '{targetName}'.",
                ex.Message,
                true);
            return true;
        }
    }
}
