using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private bool TryInvokeVmResourceMutation(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        var memoryMb = (int?)null;
        var cpu = (int?)null;
        var diskGb = (int?)null;
        var diskPath = (string?)null;
        var qosDisk = (string?)null;
        var qosAdapter = (string?)null;
        var maximumIops = (int?)null;
        var minimumIops = (int?)null;
        var maximumKbps = (int?)null;
        var minimumKbps = (int?)null;
        var action = operation switch
        {
            "vm.set-memory" => "set-memory",
            "vm.set-vcpu" => "set-vcpu",
            "vm.disk-resize" => "disk-resize",
            "vm.limit" => "limit",
            "vm.qos.storage.set" => "storage-qos",
            "vm.qos.network.set" => "network-qos",
            _ => string.Empty
        };

        var requestedMemoryMb = 0;
        if (operation == "vm.set-memory" && !TryGetInt32Property(parameters, "memory_mb", out requestedMemoryMb))
        {
            result = ResourceMutationParamFailure(operation, "PCV_MEMORY_REQUIRED", "VM memory is required.", "Provide numeric params.memory_mb.");
            return true;
        }
        if (operation == "vm.set-memory")
        {
            memoryMb = requestedMemoryMb;
        }

        var requestedCpu = 0;
        if (operation == "vm.set-vcpu" && !TryGetInt32Property(parameters, "cpu", out requestedCpu))
        {
            result = ResourceMutationParamFailure(operation, "PCV_CPU_REQUIRED", "VM CPU count is required.", "Provide numeric params.cpu.");
            return true;
        }
        if (operation == "vm.set-vcpu")
        {
            cpu = requestedCpu;
        }

        var requestedLimitMemoryMb = 0;
        var requestedLimitCpu = 0;
        if (operation == "vm.limit")
        {
            var hasMemoryLimit = TryGetInt32Property(parameters, "memory_mb", out requestedLimitMemoryMb);
            var hasCpuLimit = TryGetInt32Property(parameters, "cpu", out requestedLimitCpu);
            if (!hasMemoryLimit && !hasCpuLimit)
            {
                result = ResourceMutationParamFailure(
                    operation,
                    "PCV_LIMIT_REQUIRED",
                    "VM limit requires at least one CPU or memory value.",
                    "Provide numeric params.cpu and/or params.memory_mb.");
                return true;
            }

            memoryMb = hasMemoryLimit ? requestedLimitMemoryMb : null;
            cpu = hasCpuLimit ? requestedLimitCpu : null;
        }

        var requestedDiskGb = 0;
        if (operation == "vm.disk-resize" && !TryGetInt32Property(parameters, "disk_gb", out requestedDiskGb))
        {
            result = ResourceMutationParamFailure(operation, "PCV_DISK_SIZE_REQUIRED", "VM disk size is required.", "Provide numeric params.disk_gb.");
            return true;
        }
        if (operation == "vm.disk-resize")
        {
            diskGb = requestedDiskGb;
        }

        if (operation == "vm.qos.storage.set")
        {
            qosDisk = GetStringProperty(parameters, "disk");
            if (string.IsNullOrWhiteSpace(qosDisk) || !TryGetInt32Property(parameters, "maximum_iops", out var requestedMaximumIops))
            {
                result = ResourceMutationParamFailure(
                    operation,
                    "PCV_VM_QOS_STORAGE_VALUE_REQUIRED",
                    "VM storage QoS requires disk and maximum_iops.",
                    "Provide params.disk and numeric params.maximum_iops.");
                return true;
            }

            maximumIops = requestedMaximumIops;
            if (TryGetInt32Property(parameters, "minimum_iops", out var requestedMinimumIops))
            {
                minimumIops = requestedMinimumIops;
            }
        }

        if (operation == "vm.qos.network.set")
        {
            qosAdapter = GetStringProperty(parameters, "adapter");
            if (string.IsNullOrWhiteSpace(qosAdapter) || !TryGetInt32Property(parameters, "maximum_kbps", out var requestedMaximumKbps))
            {
                result = ResourceMutationParamFailure(
                    operation,
                    "PCV_VM_QOS_NETWORK_VALUE_REQUIRED",
                    "VM network QoS requires adapter and maximum_kbps.",
                    "Provide params.adapter and numeric params.maximum_kbps.");
                return true;
            }

            maximumKbps = requestedMaximumKbps;
            if (TryGetInt32Property(parameters, "minimum_kbps", out var requestedMinimumKbps))
            {
                minimumKbps = requestedMinimumKbps;
            }
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_OPERATION_NOT_ALLOWED",
                $"Operation '{operation}' is not a VM resource mutation operation.",
                "Use vm.limit, vm.set-memory, vm.set-vcpu, vm.disk-resize, vm.qos.storage.set, or vm.qos.network.set.",
                false);
            return true;
        }

        if (memoryMb is < 512 or > 262144)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_MEMORY_OUT_OF_RANGE",
                $"Memory '{memoryMb}' MB is outside the supported range.",
                "Use memory from 512 MB through 262144 MB.",
                false);
            return true;
        }

        if (cpu is < 1 or > 32)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_CPU_OUT_OF_RANGE",
                $"CPU count '{cpu}' is outside the supported range.",
                "Use a CPU count from 1 through 32.",
                false);
            return true;
        }

        if (diskGb is < 8 or > 4096)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_DISK_OUT_OF_RANGE",
                $"Disk '{diskGb}' GB is outside the supported range.",
                "Use disk size from 8 GB through 4096 GB.",
                false);
            return true;
        }

        if (maximumIops is < 0 or > 1_000_000_000 ||
            minimumIops is < 0 ||
            (maximumIops is not null && minimumIops is not null && minimumIops > maximumIops))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_QOS_STORAGE_RANGE_INVALID",
                "VM storage QoS values are outside the supported range.",
                "Use non-negative IOPS values and keep minimum_iops less than or equal to maximum_iops.",
                false);
            return true;
        }

        if (maximumKbps is < 0 or > 1_000_000_000 ||
            minimumKbps is < 0 ||
            (maximumKbps is not null && minimumKbps is not null && minimumKbps > maximumKbps))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_VM_QOS_NETWORK_RANGE_INVALID",
                "VM network QoS values are outside the supported range.",
                "Use non-negative Kbps values and keep minimum_kbps less than or equal to maximum_kbps.",
                false);
            return true;
        }

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            if (operation == "vm.disk-resize")
            {
                var vm = FindVm(vmProvider.GetVms(cancellationToken), vmName);
                if (vm is null)
                {
                    result = DesktopNodeHyperVOperationResult.Failure(
                        operation,
                        "PCV_VM_NOT_FOUND",
                        $"VM '{vmName}' was not found.",
                        "The VM was not present in the native Hyper-V VM inventory response.",
                        false);
                    return true;
                }

                var disk = vm.Storage.FirstOrDefault(item =>
                    item.Attached &&
                    !string.IsNullOrWhiteSpace(item.Path) &&
                    (item.Path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase) ||
                        item.Path.EndsWith(".vhd", StringComparison.OrdinalIgnoreCase)));
                if (disk is null)
                {
                    result = DesktopNodeHyperVOperationResult.Failure(
                        operation,
                        "PCV_VM_DISK_NOT_FOUND",
                        $"VM '{vmName}' has no attached VHD/VHDX disk in inventory.",
                        "Attach a virtual hard disk or refresh VM inventory before resizing.",
                        false);
                    return true;
                }

                if (disk.SizeGb.HasValue && diskGb < disk.SizeGb.Value)
                {
                    result = DesktopNodeHyperVOperationResult.Failure(
                        operation,
                        "PCV_DISK_RESIZE_SHRINK_NOT_SUPPORTED",
                        $"Disk resize target '{diskGb}' GB is smaller than the current disk size '{disk.SizeGb}' GB.",
                        "Hyper-V online disk resize supports expansion in this product route; choose a size equal to or larger than the current disk.",
                        false);
                    return true;
                }

                diskPath = disk.Path;
            }

            var data = vmResourceMutationProvider.Invoke(
                new DesktopNodeHyperVVmResourceMutationRequest(
                    operation,
                    vmName,
                    memoryMb,
                    cpu,
                    diskGb,
                    diskPath,
                    qosDisk,
                    qosAdapter,
                    maximumIops,
                    minimumIops,
                    maximumKbps,
                    minimumKbps,
                    GetStringProperty(parameters, "request_id")),
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
                "PCV_VM_RESOURCE_MUTATION_FAILED",
                $"VM resource mutation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private static DesktopNodeHyperVOperationResult ResourceMutationParamFailure(
        string operation,
        string code,
        string message,
        string detail)
    {
        return DesktopNodeHyperVOperationResult.Failure(
            operation,
            code,
            message,
            detail,
            false);
    }

    // The QoS readbacks report whether the matching queued set operation exists. They used to return a constant false
    // from before vm.qos.storage.set and vm.qos.network.set were added (backlog BL-0002).
    private static bool IsCatalogMutation(string operation) =>
        DesktopNodeHyperVAdapterDispatchCatalog.TryGetEntry(operation, out var entry) &&
        entry.Kind == DesktopNodeHyperVOperationKind.Mutation;
}
