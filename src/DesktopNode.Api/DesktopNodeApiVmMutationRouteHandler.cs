using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 큐에 올리는 VM 변경과 QoS 경로.
// 예약 시점에 조정 baseline 을 캡처해야
// 하므로 reconciliation 소유자를 소비한다.
internal sealed class DesktopNodeApiVmMutationRouteHandler
{
    private const int MaxQosPolicyValue = 1_000_000_000;

    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;
    private readonly DesktopNodeApiJobReconciliationHandler reconciliationHandler;
    private readonly DesktopNodeApiAuthSessionHandler authSessionHandler;

    public DesktopNodeApiVmMutationRouteHandler(
        DesktopNodeJobRuntime jobRuntime,
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        DesktopNodeApiJobReconciliationHandler reconciliationHandler,
        DesktopNodeApiAuthSessionHandler authSessionHandler)
    {
        this.jobRuntime = jobRuntime;
        this.operationInvoker = operationInvoker;
        this.reconciliationHandler = reconciliationHandler;
        this.authSessionHandler = authSessionHandler;
    }

    public DesktopNodeApiResponse? TryHandleQosPreview(
        DesktopNodeApiRequest request,
        string method,
        string normalizedPath,
        CancellationToken cancellationToken)
    {
        // HandleCore는 ProductOperation preview를 TryHandleQosPreview로만 보낸다.
        if (method == "POST" &&
            DesktopNodeApiRuntimeRoutes.TryMatchContract(method, normalizedPath, out var clonePreviewMatch) &&
            string.Equals(clonePreviewMatch.Route.OperationName, "PreviewCloneVm", StringComparison.Ordinal))
        {
            return HandleClonePreviewRoute(request, clonePreviewMatch, cancellationToken);
        }

        if (method == "POST" &&
            DesktopNodeApiRequestParsing.TryMatch(normalizedPath, "^/api/v1/vms/([^/]*)/qos/(storage|network)/preview$", out var qosPreviewMatch))
        {
            return HandleQosPreviewRoute(request, qosPreviewMatch, cancellationToken);
        }

        return null;
    }

    public DesktopNodeApiResponse HandleQueuedMutationRoute(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        switch (routeMatch.Route.OperationName)
        {
            case "QueueCreateVm":
                {
                    var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.create");
                    return !parsed.Ok
                        ? parsed.Response!
                        : DesktopNodeApiResponseFactory.JobCreated(CreateJob("vm.create", parsed.Value!.Value, request.RequestId!));
                }

            case "QueueCreateVmCheckpoint":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "checkpoint.create");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "checkpoint.create");
                    if (!parsed.Ok)
                    {
                        return parsed.Response!;
                    }

                    var checkpointName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "checkpoint_name") ??
                        DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "name");
                    if (string.IsNullOrWhiteSpace(checkpointName))
                    {
                        return DesktopNodeApiResponseFactory.Failure(400, "checkpoint.create", "PCV_CHECKPOINT_NAME_REQUIRED", "Checkpoint name is required.", "Pass a JSON body with name or checkpoint_name.", false);
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "checkpoint.create",
                        reconciliationHandler.BuildCheckpointCreateParameters(routeId.Value!, checkpointName, cancellationToken),
                        request.RequestId!));
                }

            case "QueueRestoreVmCheckpoint":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "checkpoint.restore");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    var checkpointId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["checkpointId"], "checkpoint.restore");
                    if (!checkpointId.Ok)
                    {
                        return checkpointId.Response!;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "checkpoint.restore",
                        reconciliationHandler.BuildCheckpointRestoreParameters(routeId.Value!, checkpointId.Value!, cancellationToken),
                        request.RequestId!));
                }

            case "QueueDeleteVmCheckpoint":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "checkpoint.delete");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    var checkpointId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["checkpointId"], "checkpoint.delete");
                    if (!checkpointId.Ok)
                    {
                        return checkpointId.Response!;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "checkpoint.delete",
                        DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                        {
                            ["checkpoint_name"] = checkpointId.Value,
                            ["vm_name"] = routeId.Value
                        }),
                        request.RequestId!));
                }

            case "QueueDeleteVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.delete");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.delete",
                        reconciliationHandler.BuildVmDeleteParameters(routeId.Value!, cancellationToken),
                        request.RequestId!));
                }

            case "QueueStartVm":
            case "QueueShutdownVm":
            case "QueuePowerOffVm":
            case "QueueRestartVm":
            case "QueuePauseVm":
            case "QueueResumeVm":
            case "QueueSaveVm":
            case "QueueResumeSavedVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "job.create");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    var lifecycleOperation = routeMatch.Route.OperationName switch
                    {
                        "QueueStartVm" => "vm.start",
                        "QueueShutdownVm" => "vm.shutdown",
                        "QueuePowerOffVm" => "vm.poweroff",
                        "QueueRestartVm" => "vm.restart",
                        "QueuePauseVm" => "vm.pause",
                        "QueueResumeVm" => "vm.resume",
                        "QueueSaveVm" => "vm.save",
                        "QueueResumeSavedVm" => "vm.resume-saved",
                        _ => throw new InvalidOperationException("Unexpected lifecycle route.")
                    };

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        lifecycleOperation,
                        DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                        {
                            ["name"] = routeId.Value
                        }),
                        request.RequestId!));
                }

            case "QueueRenameVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.rename");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.rename");
                    if (!parsed.Ok)
                    {
                        return parsed.Response!;
                    }

                    var newName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "new_name") ??
                        DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "name");
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        return DesktopNodeApiResponseFactory.Failure(400, "vm.rename", "PCV_VM_RENAME_TARGET_REQUIRED", "VM rename target is required.", "Pass a JSON body with new_name.", false);
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.rename",
                        reconciliationHandler.BuildVmRenameParameters(routeId.Value!, newName, cancellationToken),
                        request.RequestId!));
                }

            case "QueueManageVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.manage");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    string? confirmName = null;
                    if (!string.IsNullOrWhiteSpace(request.Body))
                    {
                        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.manage");
                        if (!parsed.Ok)
                        {
                            return parsed.Response!;
                        }

                        confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "confirm_name");
                    }

                    if (!string.Equals(confirmName, routeId.Value, StringComparison.Ordinal))
                    {
                        return DesktopNodeApiResponseFactory.Failure(
                            400,
                            "vm.manage",
                            "PCV_VM_MANAGE_CONFIRMATION_MISMATCH",
                            "VM manage confirmation does not match the target VM name.",
                            "Pass confirm_name equal to the VM display name in the route.",
                            false);
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.manage",
                        DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                        {
                            ["name"] = routeId.Value
                        }),
                        request.RequestId!));
                }

            case "QueueCloneVm":
                {
                    var parsed = TryReadCloneRequest(
                        request,
                        routeMatch.Parameters["vmId"],
                        "vm.clone",
                        out var sourceName,
                        out var targetName,
                        out var vmRoot);
                    if (parsed is not null)
                    {
                        return parsed;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.clone",
                        DesktopNodeApiResponseFactory.JsonFromObject(CloneParameters(sourceName, targetName, vmRoot)),
                        request.RequestId!));
                }

            case "QueueEjectVmMedia":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.eject");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.eject",
                        DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                        {
                            ["name"] = routeId.Value
                        }),
                        request.RequestId!));
                }

            case "QueueAttachVmMedia":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.attach");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.attach");
                    if (!parsed.Ok)
                    {
                        return parsed.Response!;
                    }

                    var isoPath = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "iso_path");
                    if (string.IsNullOrWhiteSpace(isoPath))
                    {
                        return DesktopNodeApiResponseFactory.Failure(
                            400,
                            "vm.attach",
                            "PCV_VM_ATTACH_ISO_REQUIRED",
                            "VM attach requires iso_path.",
                            "Pass a JSON body with iso_path set to an existing host ISO file.",
                            false);
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.attach",
                        DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                        {
                            ["name"] = routeId.Value,
                            ["iso_path"] = isoPath
                        }),
                        request.RequestId!));
                }

            case "QueueSetVmLimit":
                return QueueVmLimit(request, routeMatch);

            case "QueueSetVmStorageQos":
                return QueueVmQosMutation(
                    request,
                    routeMatch,
                    "vm.qos.storage.set",
                    "disk",
                    "maximum_iops",
                    "minimum_iops",
                    "PCV_VM_QOS_STORAGE_VALUE_REQUIRED",
                    "VM storage QoS requires disk and maximum_iops.",
                    "Pass a JSON body with disk and numeric maximum_iops.");

            case "QueueSetVmNetworkQos":
                return QueueVmQosMutation(
                    request,
                    routeMatch,
                    "vm.qos.network.set",
                    "adapter",
                    "maximum_kbps",
                    "minimum_kbps",
                    "PCV_VM_QOS_NETWORK_VALUE_REQUIRED",
                    "VM network QoS requires adapter and maximum_kbps.",
                    "Pass a JSON body with adapter and numeric maximum_kbps.");

            case "QueueSetVmMemory":
                return QueueVmResourceMutation(
                    request,
                    routeMatch,
                    "vm.set-memory",
                    "memory_mb",
                    "PCV_VM_MEMORY_VALUE_REQUIRED",
                    "VM memory value is required.",
                    "Pass a JSON body with numeric memory_mb.");

            case "QueueSetVmVcpu":
                return QueueVmResourceMutation(
                    request,
                    routeMatch,
                    "vm.set-vcpu",
                    "cpu",
                    "PCV_VM_CPU_VALUE_REQUIRED",
                    "VM vCPU value is required.",
                    "Pass a JSON body with numeric cpu.");

            case "QueueResizeVmDisk":
                return QueueVmResourceMutation(
                    request,
                    routeMatch,
                    "vm.disk-resize",
                    "disk_gb",
                    "PCV_VM_DISK_SIZE_VALUE_REQUIRED",
                    "VM disk resize value is required.",
                    "Pass a JSON body with numeric disk_gb.");

            case "QueueVmGuestExec":
                return QueueVmGuestExec(request, routeMatch);

            case "QueueVerifyVmGuestChannel":
                return QueueVmGuestChannelVerify(request, routeMatch);

            case "QueueEnsureVmGuestChannel":
                return QueueVmGuestChannelEnsure(request, routeMatch);

            default:
                return DesktopNodeApiResponseFactory.Failure(404, "api.route", "PCV_ROUTE_NOT_FOUND", $"No queued mutation route matches '{request.Path}'.", "The requested route is not part of the queued mutation API contract.", false);
        }
    }

    private DesktopNodeApiResponse QueueVmLimit(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch)
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.limit");
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.limit");
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var memoryMb = DesktopNodeApiJsonReader.ReadInt(parsed.Value!.Value, "memory_mb");
        var cpu = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "cpu");
        if (memoryMb is null && cpu is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                "vm.limit",
                "PCV_VM_LIMIT_VALUE_REQUIRED",
                "VM limit requires at least one CPU or memory value.",
                "Pass numeric cpu and/or memory_mb in the JSON body.",
                false);
        }

        var parameters = new SortedDictionary<string, object?>
        {
            ["name"] = routeId.Value
        };
        if (memoryMb is not null)
        {
            parameters["memory_mb"] = memoryMb.Value;
        }

        if (cpu is not null)
        {
            parameters["cpu"] = cpu.Value;
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob("vm.limit", DesktopNodeApiResponseFactory.JsonFromObject(parameters), request.RequestId!));
    }

    private DesktopNodeApiResponse HandleClonePreviewRoute(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        var parsed = TryReadCloneRequest(
            request,
            routeMatch.Parameters["vmId"],
            "vm.clone.preview",
            out var sourceName,
            out var targetName,
            out var vmRoot);
        if (parsed is not null)
        {
            return parsed;
        }

        return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
            "vm.clone.preview",
            DesktopNodeApiResponseFactory.JsonFromObject(CloneParameters(sourceName, targetName, vmRoot)),
            cancellationToken));
    }

    private static SortedDictionary<string, object?> CloneParameters(
        string sourceName,
        string targetName,
        string? vmRoot)
    {
        var parameters = new SortedDictionary<string, object?>
        {
            ["name"] = targetName,
            ["source"] = sourceName
        };
        if (!string.IsNullOrWhiteSpace(vmRoot))
        {
            parameters["vm_root"] = vmRoot;
        }

        return parameters;
    }

    private static DesktopNodeApiResponse? TryReadCloneRequest(
        DesktopNodeApiRequest request,
        string encodedVmId,
        string operation,
        out string sourceName,
        out string targetName,
        out string? vmRoot)
    {
        sourceName = null!;
        targetName = null!;
        vmRoot = null;
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(encodedVmId, operation);
        if (!routeId.Ok)
        {
            return routeId.Response;
        }

        string? confirmName = null;
        string? name = null;
        if (!string.IsNullOrWhiteSpace(request.Body))
        {
            var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
            if (!parsed.Ok)
            {
                return parsed.Response;
            }

            confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "confirm_name");
            name = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "name");
            vmRoot = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "vm_root");
            if (string.IsNullOrWhiteSpace(vmRoot))
            {
                vmRoot = null;
            }
        }

        if (!string.Equals(confirmName, routeId.Value, StringComparison.Ordinal))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                "PCV_VM_CLONE_CONFIRMATION_MISMATCH",
                "VM clone confirmation does not match the source VM name.",
                "Pass confirm_name equal to the VM display name in the route.",
                false);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                "PCV_VM_CLONE_NAME_REQUIRED",
                "VM clone target name is required.",
                "Pass a JSON body with name set to the new VM display name.",
                false);
        }

        if (string.Equals(name, routeId.Value, StringComparison.Ordinal))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                "PCV_VM_CLONE_NAME_CONFLICT",
                "VM clone target name matches the source VM name.",
                "Pass a different display name for the cloned VM.",
                false);
        }

        sourceName = routeId.Value!;
        targetName = name;
        return null;
    }

    private DesktopNodeApiResponse HandleQosPreviewRoute(
        DesktopNodeApiRequest request,
        Match qosPreviewMatch,
        CancellationToken cancellationToken)
    {
        var targetKind = qosPreviewMatch.Groups[2].Value;
        var operation = string.Equals(targetKind, "storage", StringComparison.OrdinalIgnoreCase)
            ? "vm.qos.storage.preview"
            : "vm.qos.network.preview";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(qosPreviewMatch.Groups[1].Value, operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var targetProperty = operation == "vm.qos.storage.preview" ? "disk" : "adapter";
        var requiredValueProperty = operation == "vm.qos.storage.preview" ? "maximum_iops" : "maximum_kbps";
        var optionalValueProperty = operation == "vm.qos.storage.preview" ? "minimum_iops" : "minimum_kbps";
        var target = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, targetProperty);
        var maximum = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, requiredValueProperty);
        if (string.IsNullOrWhiteSpace(target) || maximum is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                operation == "vm.qos.storage.preview" ? "PCV_VM_QOS_STORAGE_VALUE_REQUIRED" : "PCV_VM_QOS_NETWORK_VALUE_REQUIRED",
                operation == "vm.qos.storage.preview" ? "VM storage QoS preview requires disk and maximum_iops." : "VM network QoS preview requires adapter and maximum_kbps.",
                operation == "vm.qos.storage.preview" ? "Pass a JSON body with disk and numeric maximum_iops." : "Pass a JSON body with adapter and numeric maximum_kbps.",
                false);
        }

        var minimum = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, optionalValueProperty);
        var rangeFailure = ValidateQosRange(
            operation,
            maximum.Value,
            minimum,
            isStorage: operation == "vm.qos.storage.preview");
        if (rangeFailure is not null)
        {
            return rangeFailure;
        }

        var parameters = BuildQosParameters(
            routeId.Value!,
            targetProperty,
            target!,
            requiredValueProperty,
            maximum.Value,
            optionalValueProperty,
            minimum);
        parameters["dry_run"] = true;
        parameters["request_id"] = request.RequestId!;

        return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(operation, DesktopNodeApiResponseFactory.JsonFromObject(parameters), cancellationToken));
    }

    private DesktopNodeApiResponse QueueVmQosMutation(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        string operation,
        string targetProperty,
        string requiredValueProperty,
        string optionalValueProperty,
        string missingCode,
        string missingMessage,
        string missingAction)
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var target = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, targetProperty);
        var maximum = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, requiredValueProperty);
        if (string.IsNullOrWhiteSpace(target) || maximum is null)
        {
            return DesktopNodeApiResponseFactory.Failure(400, operation, missingCode, missingMessage, missingAction, false);
        }

        var minimum = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, optionalValueProperty);
        var rangeFailure = ValidateQosRange(
            operation,
            maximum.Value,
            minimum,
            isStorage: operation == "vm.qos.storage.set");
        if (rangeFailure is not null)
        {
            return rangeFailure;
        }

        var parameters = BuildQosParameters(
            routeId.Value!,
            targetProperty,
            target!,
            requiredValueProperty,
            maximum.Value,
            optionalValueProperty,
            minimum);
        parameters["rollback_descriptor_required"] = true;
        parameters["readback_after_apply_required"] = true;

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(operation, DesktopNodeApiResponseFactory.JsonFromObject(parameters), request.RequestId!));
    }

    private static DesktopNodeApiResponse? ValidateQosRange(
        string operation,
        int maximum,
        int? minimum,
        bool isStorage)
    {
        if (maximum >= 0 &&
            maximum <= MaxQosPolicyValue &&
            minimum is null or >= 0 &&
            (minimum is null || minimum <= maximum))
        {
            return null;
        }

        return DesktopNodeApiResponseFactory.Failure(
            400,
            operation,
            isStorage ? "PCV_VM_QOS_STORAGE_RANGE_INVALID" : "PCV_VM_QOS_NETWORK_RANGE_INVALID",
            isStorage
                ? "VM storage QoS values are outside the supported range."
                : "VM network QoS values are outside the supported range.",
            isStorage
                ? "Use non-negative IOPS values through 1000000000 and keep minimum_iops less than or equal to maximum_iops."
                : "Use non-negative Kbps values through 1000000000 and keep minimum_kbps less than or equal to maximum_kbps.",
            false);
    }

    private static SortedDictionary<string, object?> BuildQosParameters(
        string vmName,
        string targetProperty,
        string targetValue,
        string requiredValueProperty,
        int requiredValue,
        string optionalValueProperty,
        int? optionalValue)
    {
        var parameters = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            [targetProperty] = targetValue,
            [requiredValueProperty] = requiredValue
        };
        if (optionalValue is not null)
        {
            parameters[optionalValueProperty] = optionalValue.Value;
        }

        return parameters;
    }

    private DesktopNodeApiResponse QueueVmResourceMutation(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        string operation,
        string valueProperty,
        string missingCode,
        string missingMessage,
        string missingAction)
    {
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var requestedValue = DesktopNodeApiJsonReader.ReadInt(parsed.Value!.Value, valueProperty);
        if (requestedValue is null)
        {
            return DesktopNodeApiResponseFactory.Failure(400, operation, missingCode, missingMessage, missingAction, false);
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["name"] = routeId.Value,
                [valueProperty] = requestedValue.Value
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse QueueVmGuestExec(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "vm.guest.exec";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var command = DesktopNodeApiJsonReader.ReadStringList(parsed.Value!.Value, "command");
        if (command.Count == 0)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CommandRequired,
                "Guest execution requires a command array.",
                "Pass command as a non-empty JSON string array.",
                false);
        }

        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "credential_ref");
        var credential = GuestExecutionCredentialReferenceResolver.Resolve(credentialRef);
        if (string.IsNullOrWhiteSpace(credentialRef) || !credential.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CredentialRefRequired,
                "Guest execution requires a protected credential reference.",
                "Use wincred:<target>, credential-manager:<target>, or dpapi:<path>; do not pass raw secrets.",
                false);
        }

        var environment = DesktopNodeApiJsonReader.ReadStringDictionary(parsed.Value.Value, "environment");
        var timeoutSeconds = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "timeout_sec") ?? 60;
        if (timeoutSeconds is < 1 or > 600)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.Timeout,
                "Guest execution timeout is outside the supported range.",
                "Pass timeout_sec between 1 and 600 seconds.",
                false);
        }

        var redaction = GuestExecutionRedactor.Redact(command, environment);
        if (redaction.RedactionApplied)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.SecretRedactionRequired,
                "Guest execution command contains secret-like material.",
                "Move secrets into a protected credential reference before queueing guest execution.",
                false);
        }

        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef,
            redaction,
            "queued");
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["actor"] = authSessionHandler.ResolveActor(request),
                ["audit_preview"] = audit,
                ["command"] = command,
                ["credential_ref"] = credentialRef,
                ["environment"] = environment,
                ["name"] = routeId.Value,
                ["request_id"] = request.RequestId!,
                ["timeout_sec"] = timeoutSeconds
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse QueueVmGuestChannelVerify(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "vm.guest.channel.verify";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "credential_ref");
        var credential = GuestExecutionCredentialReferenceResolver.Resolve(credentialRef);
        if (string.IsNullOrWhiteSpace(credentialRef) || !credential.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.CredentialRefRequired,
                "Guest channel verification requires a protected credential reference.",
                "Use wincred:<target>, credential-manager:<target>, or dpapi:<path>; do not pass raw secrets.",
                false);
        }

        var timeoutSeconds = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "timeout_sec") ?? 60;
        if (timeoutSeconds is < 1 or > 600)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.Timeout,
                "Guest channel verification timeout is outside the supported range.",
                "Pass timeout_sec between 1 and 600 seconds.",
                false);
        }

        var redaction = GuestExecutionRedactor.Redact(["guest-agent-ensure-channel", "--verify"], new Dictionary<string, string>());
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef,
            redaction,
            "queued");
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["actor"] = authSessionHandler.ResolveActor(request),
                ["audit_preview"] = audit,
                ["credential_ref"] = credentialRef,
                ["mode"] = "verify",
                ["name"] = routeId.Value,
                ["request_id"] = request.RequestId!,
                ["timeout_sec"] = timeoutSeconds
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse QueueVmGuestChannelEnsure(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "vm.guest.channel.ensure";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        if (!DesktopNodeApiJsonReader.ReadBool(parsed.Value!.Value, "yes"))
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                "PCV_GUEST_CHANNEL_REPAIR_CONFIRMATION_REQUIRED",
                "Guest channel repair requires explicit confirmation.",
                "Pass yes=true or use pcvcli vm guest-agent-ensure-channel <vm> --repair --yes.",
                false);
        }

        var redaction = GuestExecutionRedactor.Redact(["guest-agent-ensure-channel", "--repair", "--yes"], new Dictionary<string, string>());
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation,
            request.RequestId!,
            authSessionHandler.ResolveActor(request),
            routeId.Value!,
            credentialRef: null,
            redaction,
            "queued");
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["actor"] = authSessionHandler.ResolveActor(request),
                ["audit_preview"] = audit,
                ["mode"] = "repair",
                ["name"] = routeId.Value,
                ["request_id"] = request.RequestId!,
                ["timeout_sec"] = 60,
                ["yes"] = true
            }),
            request.RequestId!));
    }

    private DesktopNodeJobSnapshot CreateJob(
        string operation,
        JsonElement parameters,
        string requestId,
        string? retryOf = null,
        int attempt = 1,
        string? correlationId = null,
        string? jobId = null)
    {
        return jobRuntime.Create(
            new DesktopNodeJobCreateCommand(
                operation,
                parameters,
                retryOf,
                attempt,
                jobId),
            new DesktopNodeJobRequestContext(requestId, correlationId));
    }
}
