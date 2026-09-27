using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private bool TryInvokeCheckpointMutation(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "vm_name");
        if (string.IsNullOrWhiteSpace(vmName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CHECKPOINT_PARAMS_INVALID",
                "Checkpoint params are missing or invalid.",
                "Provide params.vm_name for checkpoint operations.",
                false);
            return true;
        }

        if (!IsValidHyperVName(vmName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{vmName}' is invalid.",
                "Use a non-empty Hyper-V display name without leading/trailing whitespace, control characters, slash, or backslash.",
                false);
            return true;
        }

        var checkpointName = GetStringProperty(parameters, "checkpoint_name");
        if (string.IsNullOrWhiteSpace(checkpointName) || !IsValidHyperVName(checkpointName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CHECKPOINT_NAME_INVALID",
                $"Checkpoint name '{checkpointName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V checkpoint display name without leading/trailing whitespace, control characters, slash, or backslash.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = checkpointMutationProvider.Invoke(operation, vmName, checkpointName, cancellationToken);
            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = data.Name,
                ["vm_name"] = data.VmName
            };
            if (!string.IsNullOrWhiteSpace(data.Action))
            {
                payload["action"] = data.Action;
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(payload, JsonOptions),
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
                "PCV_CHECKPOINT_FAILED",
                $"Checkpoint operation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmPowerState(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = vmPowerStateProvider.Invoke(operation, vmName, cancellationToken);
            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = data.Name,
                ["action"] = data.Action
            };

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(payload, JsonOptions),
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
                "PCV_VM_POWER_STATE_FAILED",
                $"VM power-state operation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmRename(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "name");
        var newName = GetStringProperty(parameters, "new_name") ?? GetStringProperty(parameters, "target_name");
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

        if (string.IsNullOrWhiteSpace(newName) || !IsValidHyperVName(newName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_RENAME_TARGET_INVALID",
                $"VM rename target '{newName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name without leading/trailing whitespace, control characters, slash, or backslash.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = vmRenameProvider.Invoke(vmName, newName, cancellationToken);
            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = data.Name,
                ["new_name"] = data.NewName,
                ["action"] = data.Action
            };

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(payload, JsonOptions),
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
                "PCV_VM_RENAME_FAILED",
                $"VM rename operation failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmManage(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = vmManageProvider.Invoke(vmName, cancellationToken);
            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = data.Name,
                ["action"] = data.Action
            };

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(payload, JsonOptions),
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
                "PCV_VM_MANAGE_FAILED",
                $"VM manage operation failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmTemplateLock(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        if (!TryGetBooleanProperty(parameters, "locked", out var locked))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_TEMPLATE_LOCK_LOCKED_REQUIRED",
                "Template lock requires params.locked.",
                "Pass locked=true to lock or locked=false to unlock.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = vmManageProvider.InvokeTemplateLock(vmName, locked, cancellationToken);
            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = data.Name,
                ["action"] = data.Action,
                ["locked"] = locked
            };

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(payload, JsonOptions),
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
                "PCV_VM_TEMPLATE_LOCK_FAILED",
                $"VM template-lock operation failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

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
                    new DesktopNodeHyperVVmImportRequest(targetName, directory ?? string.Empty, allowedRoot, generateNewId),
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

    private bool TryInvokeVmNetworkConnect(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "vm_name") ?? GetStringProperty(parameters, "name");
        var switchName = GetStringProperty(parameters, "switch") ?? GetStringProperty(parameters, "switch_name");
        if (string.IsNullOrWhiteSpace(vmName) || !IsValidHyperVName(vmName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                DesktopNode.Contracts.NetworkChangeProblemCodes.VmRequired,
                $"VM name '{vmName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name.",
                false);
            return true;
        }

        if (string.IsNullOrWhiteSpace(switchName))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                DesktopNode.Contracts.NetworkChangeProblemCodes.SwitchRequired,
                "A switch name is required.",
                "Pass switch as the target Hyper-V switch display name.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            if (string.Equals(operation, "vm.nic.add", StringComparison.Ordinal))
            {
                var added = JsonSerializer.SerializeToElement(
                    vmNetworkConnectProvider.AddNic(new DesktopNodeHyperVVmDeviceAddRequest(vmName, switchName), cancellationToken),
                    JsonOptions);
                result = new DesktopNodeHyperVOperationResult(true, operation, added, null);
                return true;
            }

            var data = JsonSerializer.SerializeToElement(
                vmNetworkConnectProvider.Invoke(new DesktopNodeHyperVVmNetworkConnectRequest(vmName, switchName), cancellationToken),
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
                "PCV_VM_NETWORK_CONNECT_FAILED",
                $"VM network connect failed for '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmClone(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        if (!TryReadVmCloneRequest(parameters, out var request, out result, operation))
        {
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = operation == "vm.clone.preview"
                ? JsonSerializer.SerializeToElement(vmCloneProvider.Preview(request, cancellationToken), JsonOptions)
                : JsonSerializer.SerializeToElement(vmCloneProvider.Invoke(request, cancellationToken), JsonOptions);

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: data,
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
                "PCV_VM_CLONE_FAILED",
                $"VM clone operation '{operation}' failed for VM '{request.SourceName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private static bool TryReadVmCloneRequest(
        JsonElement parameters,
        out DesktopNodeHyperVVmCloneRequest request,
        out DesktopNodeHyperVOperationResult result,
        string operation)
    {
        var sourceName = GetStringProperty(parameters, "source");
        var targetName = GetStringProperty(parameters, "target");
        if (string.IsNullOrWhiteSpace(sourceName) && !string.IsNullOrWhiteSpace(targetName))
        {
            sourceName = GetStringProperty(parameters, "name");
        }
        else if (string.IsNullOrWhiteSpace(targetName))
        {
            targetName = GetStringProperty(parameters, "name");
        }

        var vmRoot = TryGetStringProperty(parameters, "vm_root", out var parsedVmRoot)
            ? parsedVmRoot
            : @"D:\PureCVisor\VMs";

        if (string.IsNullOrWhiteSpace(sourceName) || !IsValidHyperVName(sourceName))
        {
            request = null!;
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{sourceName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name without leading/trailing whitespace, control characters, slash, or backslash.",
                false);
            return false;
        }

        if (string.IsNullOrWhiteSpace(targetName) || !IsValidHyperVName(targetName))
        {
            request = null!;
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{targetName ?? string.Empty}' is invalid.",
                "Use a non-empty Hyper-V display name without leading/trailing whitespace, control characters, slash, or backslash.",
                false);
            return false;
        }

        request = new DesktopNodeHyperVVmCloneRequest(sourceName, targetName, vmRoot);
        result = null!;
        return true;
    }

    private bool TryInvokeVmMedia(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            if (string.Equals(operation, "vm.dvd.add", StringComparison.Ordinal))
            {
                var added = vmMediaProvider.AddDvd(new DesktopNodeHyperVVmDeviceAddRequest(vmName), cancellationToken);
                result = new DesktopNodeHyperVOperationResult(
                    true,
                    operation,
                    JsonSerializer.SerializeToElement(added, JsonOptions),
                    null);
                return true;
            }

            var isoPath = GetStringProperty(parameters, "iso_path");
            if (operation == "vm.attach" && string.IsNullOrWhiteSpace(isoPath))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_ATTACH_ISO_REQUIRED",
                    "VM attach requires iso_path.",
                    "Pass params.iso_path with an existing host ISO file.",
                    false);
                return true;
            }

            var data = vmMediaProvider.Invoke(
                new DesktopNodeHyperVVmMediaRequest(operation, vmName, isoPath),
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
                "PCV_VM_MEDIA_FAILED",
                $"VM media operation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmDelete(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var vm = FindVm(vmProvider.GetVms(cancellationToken), vmName);
            if (vm is null)
            {
                result = VmDeleteResult(operation, vmName, "absent");
                return true;
            }

            if (!vm.ManagedByPurecvisor)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_NOT_MANAGED_BY_PURECVISOR",
                    $"VM '{vmName}' is not managed by PureCVisor Desktop Node.",
                    $"Refusing destructive delete for a VM without the {DesktopNodeHyperVManagedNotes.Marker} marker.",
                    false);
                return true;
            }

            var data = vmDeleteProvider.Invoke(vm.Name, cancellationToken);
            result = VmDeleteResult(operation, data.Name, data.Action);
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
                "PCV_VM_DELETE_FAILED",
                $"VM delete operation failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private static DesktopNodeHyperVOperationResult VmDeleteResult(string operation, string vmName, string action)
    {
        var payload = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["action"] = action
        };
        return new DesktopNodeHyperVOperationResult(
            Ok: true,
            Operation: operation,
            Data: JsonSerializer.SerializeToElement(payload, JsonOptions),
            Error: null);
    }

    private bool TryInvokeVmCreate(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        if (!TryGetStringProperty(parameters, "name", out var name) ||
            !TryGetStringProperty(parameters, "iso_path", out var isoPath) ||
            !TryGetInt32Property(parameters, "cpu", out var cpu) ||
            !TryGetInt32Property(parameters, "memory_mb", out var memoryMb) ||
            !TryGetInt32Property(parameters, "disk_gb", out var diskGb))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CREATE_PARAMS_INVALID",
                "VM create params are missing or invalid.",
                "Provide name, iso_path, cpu, memory_mb, and disk_gb. Optional fields are vm_root and generation.",
                false);
            return true;
        }

        var generation = 2;
        if (parameters.ValueKind == JsonValueKind.Object &&
            parameters.TryGetProperty("generation", out _) &&
            !TryGetInt32Property(parameters, "generation", out generation))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CREATE_PARAMS_INVALID",
                "VM create params are missing or invalid.",
                "cpu, memory_mb, disk_gb, and generation must be numeric integer values.",
                false);
            return true;
        }

        var vmRoot = TryGetStringProperty(parameters, "vm_root", out var parsedVmRoot)
            ? parsedVmRoot
            : @"D:\PureCVisor\VMs";

        if (!IsValidVmCreateName(name))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_NAME_INVALID",
                $"VM name '{name}' is invalid.",
                "Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.",
                false);
            return true;
        }

        if (cpu is < 1 or > 32)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CPU_OUT_OF_RANGE",
                $"CPU count '{cpu}' is outside the supported spike range.",
                "Use a CPU count from 1 through 32.",
                false);
            return true;
        }

        if (memoryMb is < 512 or > 262144)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_MEMORY_OUT_OF_RANGE",
                $"Memory '{memoryMb}' MB is outside the supported spike range.",
                "Use memory from 512 MB through 262144 MB.",
                false);
            return true;
        }

        if (diskGb is < 8 or > 4096)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_DISK_OUT_OF_RANGE",
                $"Disk '{diskGb}' GB is outside the supported spike range.",
                "Use disk size from 8 GB through 4096 GB.",
                false);
            return true;
        }

        if (generation != 2)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_GENERATION_INVALID",
                $"Generation '{generation}' is invalid.",
                "Use Hyper-V generation 2 for the native VM create product path.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var data = vmCreateProvider.Invoke(new DesktopNodeHyperVVmCreateRequest(
                name,
                isoPath,
                cpu,
                memoryMb,
                diskGb,
                vmRoot,
                generation),
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
                "PCV_VM_CREATE_FAILED",
                $"VM '{name}' creation failed.",
                ex.Message,
                true);
            return true;
        }
    }

}
