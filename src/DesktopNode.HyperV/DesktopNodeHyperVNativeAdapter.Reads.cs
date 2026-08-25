using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter
{
    private bool TryInvokeNetworkInventory(string operation, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var switches = switchProvider.GetSwitches(cancellationToken);
            if (!HasCompleteSwitchTopology(switches))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE",
                    "Native Hyper-V network inventory topology is incomplete.",
                    "The native adapter could not preserve switch type or adapter fields.",
                    false);
                return true;
            }

            var data = new DesktopNodeHyperVNetworkInventoryData(
                Source: "hyperv",
                Mutating: false,
                Switches: switches);
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
                "PCV_NETWORK_INVENTORY_FAILED",
                "Hyper-V network inventory failed.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmList(string operation, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var vms = vmProvider.GetVms(cancellationToken);
            if (!HasCompleteVmIdentityState(vms))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE",
                    "Native Hyper-V VM inventory identity/state is incomplete.",
                    "The native adapter could not preserve VM id, name, and state for every row.",
                    false);
                return true;
            }

            if (!HasCompleteVmSummaryParity(vms))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE",
                    "Native Hyper-V VM inventory summary parity is incomplete.",
                    "The native adapter could not preserve VM platform, CPU, startup memory, generation, checkpoint, or console summary fields for every row.",
                    false);
                return true;
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(vms, JsonOptions),
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
                "PCV_VM_LIST_FAILED",
                "Hyper-V VM inventory failed.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmStats(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "vm_name") ?? GetStringProperty(parameters, "name");
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
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_NOT_FOUND",
                    $"VM '{vmName}' was not found.",
                    "The VM was not present in the current Hyper-V inventory response.",
                    false);
                return true;
            }

            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = vm.Name,
                ["id"] = vm.Id,
                ["state"] = vm.State
            };
            if (operation == "vm.memory-stats")
            {
                payload["memory"] = vm.Memory;
            }
            else
            {
                payload["cpu"] = vm.Cpu;
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
                "PCV_VM_STATS_FAILED",
                $"VM stats operation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmInventoryReadback(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "vm_name") ?? GetStringProperty(parameters, "name");
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
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_VM_NOT_FOUND",
                    $"VM '{vmName}' was not found.",
                    "The VM was not present in the current Hyper-V inventory response.",
                    false);
                return true;
            }

            var payload = new SortedDictionary<string, object?>
            {
                ["name"] = vm.Name,
                ["id"] = vm.Id,
                ["state"] = vm.State
            };

            switch (operation)
            {
                case "vm.blkio-get":
                    payload["storage_qos"] = new SortedDictionary<string, object?>
                    {
                        ["contract"] = "hyperv-storage-inventory-readback-v1",
                        ["linux_blkio_compatible"] = false,
                        ["mutation_supported"] = false,
                        ["disks"] = vm.Storage
                    };
                    break;
                case "vm.bandwidth":
                    payload["network_qos"] = new SortedDictionary<string, object?>
                    {
                        ["contract"] = "hyperv-network-inventory-readback-v1",
                        ["linux_bandwidth_compatible"] = false,
                        ["mutation_supported"] = false,
                        ["adapters"] = vm.Network
                    };
                    break;
                case "vm.guest-agent-status":
                    payload["guest_agent"] = new SortedDictionary<string, object?>
                    {
                        ["contract"] = "hyperv-guest-service-status-readback-v1",
                        ["agent_type"] = "hyperv-integration-services",
                        ["qemu_guest_agent"] = false,
                        ["guest_exec_supported"] = false,
                        ["credential_required_for_exec"] = true,
                        ["status"] = string.Equals(vm.State, "running", StringComparison.OrdinalIgnoreCase)
                            ? "vm-running-integration-service-readiness-unverified"
                            : "vm-not-running"
                    };
                    break;
                case "vm.guest-ping":
                    payload["guest_ping"] = new SortedDictionary<string, object?>
                    {
                        ["contract"] = "hyperv-guest-service-ping-readiness-v1",
                        ["agent_type"] = "hyperv-integration-services",
                        ["qemu_guest_agent"] = false,
                        ["guest_heartbeat_verified"] = false,
                        ["reachable"] = string.Equals(vm.State, "running", StringComparison.OrdinalIgnoreCase),
                        ["status"] = string.Equals(vm.State, "running", StringComparison.OrdinalIgnoreCase)
                            ? "vm-running-guest-credentialless-ping-not-claimed"
                            : "vm-not-running"
                    };
                    break;
                default:
                    result = DesktopNodeHyperVOperationResult.Failure(
                        operation,
                        "PCV_OPERATION_NOT_ALLOWED",
                        $"Operation '{operation}' is not a VM inventory readback operation.",
                        "Use vm.blkio-get, vm.bandwidth, vm.guest-agent-status, or vm.guest-ping.",
                        false);
                    return true;
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
                "PCV_VM_INVENTORY_READBACK_FAILED",
                $"VM inventory readback operation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeVmQosPreview(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var vmName = GetStringProperty(parameters, "vm_name") ?? GetStringProperty(parameters, "name");
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

        var isStorage = string.Equals(operation, "vm.qos.storage.preview", StringComparison.Ordinal);
        var targetProperty = isStorage ? "disk" : "adapter";
        var maximumProperty = isStorage ? "maximum_iops" : "maximum_kbps";
        var minimumProperty = isStorage ? "minimum_iops" : "minimum_kbps";
        var target = GetStringProperty(parameters, targetProperty);
        if (string.IsNullOrWhiteSpace(target) || !TryGetInt32Property(parameters, maximumProperty, out var maximum))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                isStorage ? "PCV_VM_QOS_STORAGE_VALUE_REQUIRED" : "PCV_VM_QOS_NETWORK_VALUE_REQUIRED",
                isStorage ? "VM storage QoS preview requires disk and maximum_iops." : "VM network QoS preview requires adapter and maximum_kbps.",
                isStorage ? "Provide params.disk and numeric params.maximum_iops." : "Provide params.adapter and numeric params.maximum_kbps.",
                false);
            return true;
        }

        TryGetInt32Property(parameters, minimumProperty, out var minimum);
        var hasMinimum = parameters.ValueKind == JsonValueKind.Object && parameters.TryGetProperty(minimumProperty, out _);
        if (maximum < 0 || maximum > 1_000_000_000 || (hasMinimum && (minimum < 0 || minimum > maximum)))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                isStorage ? "PCV_VM_QOS_STORAGE_RANGE_INVALID" : "PCV_VM_QOS_NETWORK_RANGE_INVALID",
                "VM QoS preview values are outside the supported range.",
                "Use non-negative integer values and keep the minimum value less than or equal to the maximum value.",
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
                    operation,
                    "PCV_VM_NOT_FOUND",
                    $"VM '{vmName}' was not found.",
                    "The VM was not present in the current Hyper-V inventory response.",
                    false);
                return true;
            }

            var policy = new SortedDictionary<string, object?>
            {
                [maximumProperty] = maximum
            };
            if (hasMinimum)
            {
                policy[minimumProperty] = minimum;
            }

            var bucket = new SortedDictionary<string, object?>
            {
                [isStorage ? "target_disk" : "adapter"] = target,
                ["proposed_policy"] = policy,
                ["supported"] = true,
                ["current_readback"] = isStorage ? vm.Storage : vm.Network
            };
            var payload = new SortedDictionary<string, object?>
            {
                ["contract"] = "hyperv-qos-mutation-preview.v1",
                ["mode"] = "dry-run",
                ["provider"] = "hyperv",
                ["request_id"] = GetStringProperty(parameters, "request_id") ?? string.Empty,
                ["actor"] = "local-api-operator",
                ["vm"] = new SortedDictionary<string, object?>
                {
                    ["id"] = vm.Id,
                    ["name"] = vm.Name,
                    ["state"] = vm.State
                },
                [isStorage ? "storage" : "network"] = bucket,
                ["validation"] = new SortedDictionary<string, object?>
                {
                    ["requires_admin"] = true,
                    ["host_mutation_performed"] = false,
                    ["range_valid"] = true
                },
                ["rollback_plan"] = new SortedDictionary<string, object?>
                {
                    ["previous_policy_captured"] = false,
                    ["rollback_operation"] = isStorage ? "vm.qos.storage.rollback" : "vm.qos.network.rollback"
                },
                ["readback_routes"] = isStorage
                    ? new[] { "vm.blkio-get" }
                    : ["vm.bandwidth"]
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
                isStorage ? "PCV_VM_QOS_STORAGE_PREVIEW_FAILED" : "PCV_VM_QOS_NETWORK_PREVIEW_FAILED",
                $"VM QoS preview operation '{operation}' failed for VM '{vmName}'.",
                ex.Message,
                true);
            return true;
        }
    }

    private bool TryInvokeCheckpointList(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
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

        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            var vms = vmProvider.GetVms(cancellationToken);
            if (vms.Count == 0)
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_CHECKPOINT_LIST_VM_INVENTORY_EMPTY",
                    "Native checkpoint list cannot verify VM existence from an empty VM inventory.",
                    "The native adapter does not treat an empty VM inventory as authoritative for checkpoint list.",
                    false);
                return true;
            }

            if (!HasCompleteVmIdentity(vms))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_CHECKPOINT_LIST_VM_IDENTITY_INCOMPLETE",
                    "Native checkpoint list VM identity is incomplete.",
                    "The native adapter could not preserve VM id and name while checking checkpoint ownership.",
                    false);
                return true;
            }

            var vm = FindVm(vms, vmName);
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

            var checkpoints = checkpointProvider.GetCheckpoints(vm.Name, cancellationToken);
            if (!HasCompleteCheckpointListParity(checkpoints))
            {
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_CHECKPOINT_LIST_PARITY_INCOMPLETE",
                    "Native checkpoint list parity is incomplete.",
                    "The native adapter could not preserve checkpoint name and VM name for every row.",
                    false);
                return true;
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(checkpoints, JsonOptions),
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

    private bool TryInvokeHostStatus(string operation, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        try
        {
            ThrowIfNativeCanceled(cancellationToken, operation);
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(hostStatusProvider.GetStatus(cancellationToken), JsonOptions),
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
                "PCV_HOST_STATUS_FAILED",
                "Hyper-V host status failed.",
                ex.Message,
                true);
            return true;
        }
    }

}
