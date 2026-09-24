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
    private readonly DesktopNodeCheckpointScheduleStore checkpointScheduleStore;

    public DesktopNodeApiVmMutationRouteHandler(
        DesktopNodeJobRuntime jobRuntime,
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        DesktopNodeApiJobReconciliationHandler reconciliationHandler,
        DesktopNodeApiAuthSessionHandler authSessionHandler,
        DesktopNodeCheckpointScheduleStore checkpointScheduleStore)
    {
        this.jobRuntime = jobRuntime;
        this.operationInvoker = operationInvoker;
        this.reconciliationHandler = reconciliationHandler;
        this.authSessionHandler = authSessionHandler;
        this.checkpointScheduleStore = checkpointScheduleStore;
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
            DesktopNodeApiRuntimeRoutes.TryMatchContract(method, normalizedPath, out var guestFilePreviewMatch) &&
            string.Equals(guestFilePreviewMatch.Route.OperationName, "PreviewVmGuestFile", StringComparison.Ordinal))
        {
            return HandleGuestFilePreviewRoute(request, guestFilePreviewMatch, cancellationToken);
        }

        if (method == "POST" &&
            DesktopNodeApiRuntimeRoutes.TryMatchContract(method, normalizedPath, out var schedulePreviewMatch) &&
            string.Equals(schedulePreviewMatch.Route.OperationName, "PreviewVmCheckpointSchedule", StringComparison.Ordinal))
        {
            return HandleCheckpointSchedulePreview(request, schedulePreviewMatch, cancellationToken);
        }

        if (method == "POST" &&
            DesktopNodeApiRuntimeRoutes.TryMatchContract(method, normalizedPath, out var exportPreviewMatch) &&
            string.Equals(exportPreviewMatch.Route.OperationName, "PreviewVmExport", StringComparison.Ordinal))
        {
            return HandleVmExportPreview(request, exportPreviewMatch, cancellationToken);
        }

        if (method == "POST" &&
            DesktopNodeApiRuntimeRoutes.TryMatchContract(method, normalizedPath, out var importPreviewMatch) &&
            string.Equals(importPreviewMatch.Route.OperationName, "PreviewVmImport", StringComparison.Ordinal))
        {
            return HandleVmImportPreview(request, cancellationToken);
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
                        : DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                            "vm.create",
                            reconciliationHandler.BuildVmCreateParameters(parsed.Value!.Value, cancellationToken),
                            request.RequestId!));
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

            case "QueueShutdownVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.shutdown");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.shutdown",
                        reconciliationHandler.BuildVmShutdownParameters(routeId.Value!, cancellationToken),
                        request.RequestId!));
                }

            case "QueueRestartVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.restart");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.restart",
                        reconciliationHandler.BuildVmRestartParameters(routeId.Value!, cancellationToken),
                        request.RequestId!));
                }

            case "QueueStartVm":
            case "QueuePowerOffVm":
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
                        "QueuePowerOffVm" => "vm.poweroff",
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

            case "QueueTemplateLockVm":
                {
                    var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], "vm.template.lock");
                    if (!routeId.Ok)
                    {
                        return routeId.Response!;
                    }

                    if (string.IsNullOrWhiteSpace(request.Body))
                    {
                        return DesktopNodeApiResponseFactory.Failure(
                            400,
                            "vm.template.lock",
                            "PCV_VM_TEMPLATE_LOCK_LOCKED_REQUIRED",
                            "Template lock requires a JSON body.",
                            "Pass confirm_name and locked.",
                            false);
                    }

                    var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, "vm.template.lock");
                    if (!parsed.Ok)
                    {
                        return parsed.Response!;
                    }

                    var confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "confirm_name");
                    if (!string.Equals(confirmName, routeId.Value, StringComparison.Ordinal))
                    {
                        return DesktopNodeApiResponseFactory.Failure(
                            400,
                            "vm.template.lock",
                            "PCV_VM_TEMPLATE_LOCK_CONFIRMATION_MISMATCH",
                            "VM template-lock confirmation does not match the target VM name.",
                            "Pass confirm_name equal to the VM display name in the route.",
                            false);
                    }

                    if (!parsed.Value.Value.TryGetProperty("locked", out var lockedElement) ||
                        (lockedElement.ValueKind != JsonValueKind.True && lockedElement.ValueKind != JsonValueKind.False))
                    {
                        return DesktopNodeApiResponseFactory.Failure(
                            400,
                            "vm.template.lock",
                            "PCV_VM_TEMPLATE_LOCK_LOCKED_REQUIRED",
                            "Template lock requires params.locked.",
                            "Pass locked=true to lock or locked=false to unlock.",
                            false);
                    }

                    return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
                        "vm.template.lock",
                        DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                        {
                            ["name"] = routeId.Value,
                            ["locked"] = lockedElement.ValueKind == JsonValueKind.True
                        }),
                        request.RequestId!));
                }

            case "QueueExportVm":
                return HandleVmExport(request, routeMatch, cancellationToken);

            case "QueueImportVm":
                return HandleVmImport(request, cancellationToken);

            case "QueueConnectVmNetwork":
                return HandleVmNetworkConnect(request, routeMatch, cancellationToken);

            case "QueueAddVmDevice":
                return HandleVmDeviceAdd(request, routeMatch, cancellationToken);

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
                    "Pass a JSON body with disk and numeric maximum_iops.",
                    cancellationToken);

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
                    "Pass a JSON body with adapter and numeric maximum_kbps.",
                    cancellationToken);

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

            case "QueueVmGuestFile":
                return QueueVmGuestFile(request, routeMatch);

            case "QueueVerifyVmGuestChannel":
                return QueueVmGuestChannelVerify(request, routeMatch);

            case "QueueEnsureVmGuestChannel":
                return QueueVmGuestChannelEnsure(request, routeMatch);

            case "QueueSetVmCheckpointSchedule":
                return HandleCheckpointScheduleSet(request, routeMatch, cancellationToken);

            case "QueueClearVmCheckpointSchedule":
                return HandleCheckpointScheduleClear(request, routeMatch);

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

    private DesktopNodeApiResponse HandleGuestFilePreviewRoute(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        var parsed = TryReadGuestFileRequest(
            request,
            routeMatch.Parameters["vmId"],
            "vm.guest.file.preview",
            out var parameters);
        if (parsed is not null)
        {
            return parsed;
        }

        return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
            "vm.guest.file.preview",
            DesktopNodeApiResponseFactory.JsonFromObject(parameters),
            cancellationToken));
    }

    private DesktopNodeApiResponse QueueVmGuestFile(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch)
    {
        var parsed = TryReadGuestFileRequest(
            request,
            routeMatch.Parameters["vmId"],
            "vm.guest.file",
            out var parameters);
        if (parsed is not null)
        {
            return parsed;
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            "vm.guest.file",
            DesktopNodeApiResponseFactory.JsonFromObject(parameters),
            request.RequestId!));
    }

    private static DesktopNodeApiResponse? TryReadGuestFileRequest(
        DesktopNodeApiRequest request,
        string encodedVmId,
        string operation,
        out SortedDictionary<string, object?> parameters)
    {
        parameters = [];
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(encodedVmId, operation);
        if (!routeId.Ok)
        {
            return routeId.Response;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response;
        }

        var hostPath = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "host_path");
        var guestPath = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "guest_path");
        var credentialRef = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "credential_ref");
        var direction = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "direction") ?? GuestFileJobContract.DirectionHostToGuest;
        var sharedFolder = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "shared_folder");
        var timeoutSeconds = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "timeout_sec") ?? 60;
        var sizeBytes = DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "size_bytes");
        if (timeoutSeconds is < 1 or > 600)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                GuestExecutionProblemCodes.Timeout,
                "Guest file timeout is outside the supported range.",
                "Pass timeout_sec between 1 and 600 seconds.",
                false);
        }

        var evaluation = GuestFileJobContract.Evaluate(new GuestFileJobRequest(
            direction,
            hostPath,
            guestPath,
            sizeBytes ?? 1,
            credentialRef,
            sharedFolder));
        if (!evaluation.Ok)
        {
            return DesktopNodeApiResponseFactory.Failure(
                400,
                operation,
                evaluation.ErrorCode ?? GuestFileJobProblemCodes.PathNotAllowed,
                "Guest file job request is outside the allowlist.",
                "Use host-to-guest, credential-ref, and allowlisted paths/size. HGFS shared folders are forbidden.",
                false);
        }

        parameters = new SortedDictionary<string, object?>
        {
            ["name"] = routeId.Value,
            ["credential_ref"] = credentialRef,
            ["direction"] = evaluation.Direction,
            ["guest_path"] = evaluation.NormalizedGuestPath,
            ["host_path"] = evaluation.NormalizedHostPath,
            ["timeout_sec"] = timeoutSeconds
        };
        if (sizeBytes is not null)
        {
            parameters["size_bytes"] = sizeBytes.Value;
        }

        return null;
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

    private DesktopNodeApiResponse HandleVmExport(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        const string operation = "vm.export";
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

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory.Data, routeId.Value!);
        if (vm is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                operation,
                "PCV_VM_NOT_FOUND",
                $"VM '{routeId.Value}' was not found.",
                "The VM was not present in the current Hyper-V inventory response.",
                false);
        }

        var confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "confirm_name") ?? routeId.Value;
        var directory = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "directory");
        var allowedRoot = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "allowed_root") ??
            VmExportImportPolicy.DefaultExportRoot;
        var evaluation = VmExportImportPolicy.EvaluateExport(new VmExportRequest(
            routeId.Value,
            confirmName,
            directory,
            allowedRoot,
            authSessionHandler.ResolveVmExportImportAuth(request),
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            Generation: DesktopNodeApiJsonReader.ReadInt(vm.Value, "generation") ?? 0,
            PowerState: DesktopNodeApiJsonReader.GetStringProperty(vm.Value, "state"),
            SecurityFeaturesPresent: DesktopNodeApiJsonReader.ReadBool(vm.Value, "security_features_present")));
        if (!evaluation.Ok)
        {
            return MapVmExportImportError(operation, evaluation.ErrorCode!);
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["allowed_root"] = allowedRoot,
                ["directory"] = evaluation.Directory,
                ["vm_name"] = evaluation.VmName
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse HandleVmImport(
        DesktopNodeApiRequest request,
        CancellationToken cancellationToken)
    {
        const string operation = "vm.import";
        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var targetName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "name") ??
            DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "target_name");
        var confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "confirm_name") ?? targetName;
        var directory = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "directory");
        var allowedRoot = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "allowed_root") ??
            VmExportImportPolicy.DefaultExportRoot;
        var packageKind = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "package_kind") ??
            VmExportImportPolicy.PackageHyperVExport;
        var generateNewId = !parsed.Value.Value.TryGetProperty("generate_new_id", out _) ||
            DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "generate_new_id");
        var evaluation = VmExportImportPolicy.EvaluateImport(new VmImportRequest(
            targetName,
            confirmName,
            directory,
            allowedRoot,
            authSessionHandler.ResolveVmExportImportAuth(request),
            PackageKind: packageKind,
            HasVmcx: DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "has_vmcx"),
            SecurityFeaturesPresent: DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "security_features_present"),
            GenerateNewId: generateNewId,
            TargetExists: targetName is not null &&
                DesktopNodeApiJsonReader.FindVm(inventory.Data, targetName) is not null));
        if (!evaluation.Ok)
        {
            return MapVmExportImportError(operation, evaluation.ErrorCode!);
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["allowed_root"] = allowedRoot,
                ["apply_managed_marker"] = evaluation.ApplyManagedMarker,
                ["directory"] = evaluation.Directory,
                ["generate_new_id"] = evaluation.GenerateNewId,
                ["has_vmcx"] = true,
                ["name"] = evaluation.VmName,
                ["package_kind"] = evaluation.PackageKind,
                ["vm_name"] = evaluation.VmName
            }),
            request.RequestId!));
    }

    private DesktopNodeApiResponse HandleVmDeviceAdd(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        const string routeOperation = "vm.device.add";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], routeOperation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, routeOperation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory.Data, routeId.Value!);
        if (vm is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                routeOperation,
                "PCV_VM_NOT_FOUND",
                $"VM '{routeId.Value}' was not found.",
                "The VM was not present in the current Hyper-V inventory response.",
                false);
        }

        var body = parsed.Value!.Value;
        var kind = DesktopNodeApiJsonReader.GetStringProperty(body, "device") ??
            DesktopNodeApiJsonReader.GetStringProperty(body, "kind");
        var switchName = DesktopNodeApiJsonReader.GetStringProperty(body, "switch") ??
            DesktopNodeApiJsonReader.GetStringProperty(body, "switch_name");
        var isoPath = DesktopNodeApiJsonReader.GetStringProperty(body, "iso_path");
        var nicRequest = string.Equals(kind?.Trim(), VmDeviceAddPolicy.KindNic, StringComparison.OrdinalIgnoreCase);
        var switchExists = false;
        if (nicRequest)
        {
            var switches = operationInvoker.Invoke(
                "network.inventory",
                DesktopNodeApiResponseFactory.EmptyObject(),
                cancellationToken);
            if (!switches.Ok)
            {
                return DesktopNodeApiResponseFactory.OperationResponse(switches);
            }

            switchExists = SwitchExists(switches.Data ?? default, switchName);
        }

        var evaluation = VmDeviceAddPolicy.EvaluateAdd(new VmDeviceAddRequest(
            routeId.Value,
            kind,
            authSessionHandler.ResolveVmDeviceAddAuth(request),
            Quantity: ReadDeviceQuantity(body),
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            TemplateLocked: DesktopNodeApiJsonReader.ReadBool(vm.Value, "template_lock"),
            PowerState: DesktopNodeApiJsonReader.GetStringProperty(vm.Value, "state"),
            Generation: ReadGeneration(vm.Value),
            ExistingNicCount: CountArray(vm.Value, "network"),
            ExistingDvdCount: CountDvdDrives(vm.Value),
            SwitchName: switchName,
            SwitchExists: switchExists,
            NatEnabled: DesktopNodeApiJsonReader.ReadBool(body, "nat"),
            DhcpEnabled: DesktopNodeApiJsonReader.ReadBool(body, "dhcp"),
            IsoRequested: !string.IsNullOrWhiteSpace(isoPath)));
        if (!evaluation.Ok)
        {
            return MapVmDeviceAddError(routeOperation, evaluation.ErrorCode!);
        }

        var operation = evaluation.DeviceKind == VmDeviceAddPolicy.KindDvd ? "vm.dvd.add" : "vm.nic.add";
        var parameters = new SortedDictionary<string, object?>
        {
            ["device"] = evaluation.DeviceKind,
            ["name"] = evaluation.VmName,
            ["vm_name"] = evaluation.VmName
        };
        if (evaluation.DeviceKind == VmDeviceAddPolicy.KindNic)
        {
            parameters["switch"] = evaluation.SwitchName;
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(parameters),
            request.RequestId!));
    }

    private static int ReadDeviceQuantity(JsonElement body)
    {
        if (!body.TryGetProperty("quantity", out var quantity) || quantity.ValueKind != JsonValueKind.Number)
        {
            return 1;
        }

        return quantity.TryGetInt32(out var value) ? value : 0;
    }

    private static int ReadGeneration(JsonElement vm)
    {
        if (!vm.TryGetProperty("generation", out var generation) || generation.ValueKind != JsonValueKind.Number)
        {
            return 0;
        }

        return generation.TryGetInt32(out var value) ? value : 0;
    }

    private static int CountArray(JsonElement vm, string name)
    {
        return vm.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;
    }

    private static int CountDvdDrives(JsonElement vm)
    {
        if (!vm.TryGetProperty("storage", out var storage) || storage.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        var count = 0;
        foreach (var item in storage.EnumerateArray())
        {
            var type = DesktopNodeApiJsonReader.GetStringProperty(item, "type") ?? string.Empty;
            var path = DesktopNodeApiJsonReader.GetStringProperty(item, "path") ?? string.Empty;
            if (type.Contains("dvd", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".iso", StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private static DesktopNodeApiResponse MapVmDeviceAddError(string operation, string code)
    {
        var forbidden = string.Equals(code, VmDeviceAddProblemCodes.Forbidden, StringComparison.Ordinal);
        var status = forbidden
            ? 403
            : string.Equals(code, VmDeviceAddProblemCodes.TemplateLocked, StringComparison.Ordinal)
                ? 409
                : 400;
        return DesktopNodeApiResponseFactory.Failure(
            status,
            operation,
            code,
            forbidden
                ? "The current account role is not allowed to add a VM device."
                : "The VM device add request was rejected.",
            forbidden
                ? "Grant operate or use the service bearer."
                : "Add one synthetic NIC or one empty DVD drive on a managed Generation 2 VM that is Off.",
            false);
    }

    private DesktopNodeApiResponse HandleVmNetworkConnect(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        const string operation = "vm.network.connect";
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

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory.Data, routeId.Value!);
        if (vm is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                operation,
                "PCV_VM_NOT_FOUND",
                $"VM '{routeId.Value}' was not found.",
                "The VM was not present in the current Hyper-V inventory response.",
                false);
        }

        var switches = operationInvoker.Invoke(
            "network.inventory",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!switches.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(switches);
        }

        var switchName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "switch") ??
            DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "switch_name");
        var evaluation = NetworkChangePolicy.EvaluateVmConnect(new VmNetworkConnectRequest(
            routeId.Value,
            switchName,
            authSessionHandler.ResolveNetworkChangeAuth(request),
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            TemplateLocked: DesktopNodeApiJsonReader.ReadBool(vm.Value, "template_lock"),
            PowerState: DesktopNodeApiJsonReader.GetStringProperty(vm.Value, "state"),
            SwitchExists: SwitchExists(switches.Data ?? default, switchName)));
        if (!evaluation.Ok)
        {
            return MapNetworkChangeError(operation, evaluation.ErrorCode!);
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["switch"] = evaluation.SwitchName,
                ["vm_name"] = evaluation.VmName
            }),
            request.RequestId!));
    }

    private static bool SwitchExists(JsonElement inventory, string? switchName)
    {
        if (string.IsNullOrWhiteSpace(switchName) ||
            inventory.ValueKind != JsonValueKind.Object ||
            !inventory.TryGetProperty("switches", out var switches) ||
            switches.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in switches.EnumerateArray())
        {
            var name = DesktopNodeApiJsonReader.GetStringProperty(item, "name");
            if (string.Equals(name, switchName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static DesktopNodeApiResponse MapNetworkChangeError(string operation, string code)
    {
        var forbidden = string.Equals(code, NetworkChangeProblemCodes.Forbidden, StringComparison.Ordinal);
        var status = forbidden
            ? 403
            : string.Equals(code, NetworkChangeProblemCodes.TemplateLocked, StringComparison.Ordinal)
                ? 409
                : 400;
        return DesktopNodeApiResponseFactory.Failure(
            status,
            operation,
            code,
            forbidden
                ? "The current account role is not allowed to change VM network attachment."
                : "The VM network connect request was rejected.",
            forbidden
                ? "Grant operate or use the service bearer."
                : "Pass a managed Off VM and an existing Hyper-V switch name.",
            false);
    }

    private DesktopNodeApiResponse HandleVmExportPreview(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        const string operation = "vm.export.preview";
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

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory.Data, routeId.Value!);
        if (vm is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                operation,
                "PCV_VM_NOT_FOUND",
                $"VM '{routeId.Value}' was not found.",
                "The VM was not present in the current Hyper-V inventory response.",
                false);
        }

        var confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "confirm_name") ?? routeId.Value;
        var directory = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "directory");
        var allowedRoot = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "allowed_root") ??
            VmExportImportPolicy.DefaultExportRoot;
        var evaluation = VmExportImportPolicy.EvaluateExportPreview(new VmExportRequest(
            routeId.Value,
            confirmName,
            directory,
            allowedRoot,
            authSessionHandler.ResolveVmExportImportAuth(request),
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            Generation: DesktopNodeApiJsonReader.ReadInt(vm.Value, "generation") ?? 0,
            PowerState: DesktopNodeApiJsonReader.GetStringProperty(vm.Value, "state"),
            SecurityFeaturesPresent: DesktopNodeApiJsonReader.ReadBool(vm.Value, "security_features_present")));
        if (!evaluation.Ok)
        {
            return MapVmExportImportError(operation, evaluation.ErrorCode!);
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(
            true,
            operation,
            new SortedDictionary<string, object?>
            {
                ["action"] = evaluation.Action,
                ["apply_managed_marker"] = evaluation.ApplyManagedMarker,
                ["directory"] = evaluation.Directory,
                ["dry_run"] = true,
                ["generate_new_id"] = evaluation.GenerateNewId,
                ["host_mutation_performed"] = false,
                ["package_kind"] = evaluation.PackageKind,
                ["schema"] = VmExportImportPolicy.Schema,
                ["vm_name"] = evaluation.VmName
            },
            null));
    }

    private DesktopNodeApiResponse HandleVmImportPreview(
        DesktopNodeApiRequest request,
        CancellationToken cancellationToken)
    {
        const string operation = "vm.import.preview";
        var parsed = DesktopNodeApiRequestParsing.TryParseBody(request.Body, operation);
        if (!parsed.Ok)
        {
            return parsed.Response!;
        }

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var targetName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value!.Value, "name") ??
            DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "target_name");
        var confirmName = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "confirm_name") ?? targetName;
        var directory = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "directory");
        var allowedRoot = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "allowed_root") ??
            VmExportImportPolicy.DefaultExportRoot;
        var packageKind = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "package_kind") ??
            VmExportImportPolicy.PackageHyperVExport;
        var generateNewId = !parsed.Value.Value.TryGetProperty("generate_new_id", out _) ||
            DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "generate_new_id");
        var evaluation = VmExportImportPolicy.EvaluateImportPreview(new VmImportRequest(
            targetName,
            confirmName,
            directory,
            allowedRoot,
            authSessionHandler.ResolveVmExportImportAuth(request),
            PackageKind: packageKind,
            HasVmcx: DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "has_vmcx"),
            SecurityFeaturesPresent: DesktopNodeApiJsonReader.ReadBool(parsed.Value.Value, "security_features_present"),
            GenerateNewId: generateNewId,
            TargetExists: targetName is not null &&
                DesktopNodeApiJsonReader.FindVm(inventory.Data, targetName) is not null));
        if (!evaluation.Ok)
        {
            return MapVmExportImportError(operation, evaluation.ErrorCode!);
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(
            true,
            operation,
            new SortedDictionary<string, object?>
            {
                ["action"] = evaluation.Action,
                ["apply_managed_marker"] = evaluation.ApplyManagedMarker,
                ["directory"] = evaluation.Directory,
                ["dry_run"] = true,
                ["generate_new_id"] = evaluation.GenerateNewId,
                ["host_mutation_performed"] = false,
                ["package_kind"] = evaluation.PackageKind,
                ["schema"] = VmExportImportPolicy.Schema,
                ["vm_name"] = evaluation.VmName
            },
            null));
    }

    private static DesktopNodeApiResponse MapVmExportImportError(string operation, string code)
    {
        var forbidden = string.Equals(code, VmExportImportProblemCodes.Forbidden, StringComparison.Ordinal);
        var conflict = string.Equals(code, VmExportImportProblemCodes.AlreadyExists, StringComparison.Ordinal);
        return DesktopNodeApiResponseFactory.Failure(
            forbidden ? 403 : conflict ? 409 : 400,
            operation,
            code,
            forbidden
                ? "The current account role is not allowed to export or import a VM."
                : "The VM export or import request was rejected.",
            forbidden
                ? "Grant operate or use the service bearer."
                : "Pass a managed Gen2 Off VM, a Hyper-V export folder under the allowlist root, and no TPM/OVF package.",
            false);
    }

    private DesktopNodeApiResponse HandleCheckpointSchedulePreview(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        const string operation = "checkpoint.schedule.preview";
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

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory.Data, routeId.Value!);
        if (vm is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                operation,
                "PCV_VM_NOT_FOUND",
                $"VM '{routeId.Value}' was not found.",
                "The VM was not present in the current Hyper-V inventory response.",
                false);
        }

        var currentCount = DesktopNodeApiJsonReader.ReadNestedElement(vm.Value, "checkpoints", "count");
        var evaluation = CheckpointSchedulePolicy.EvaluatePreview(new CheckpointScheduleRequest(
            routeId.Value,
            authSessionHandler.ResolveCheckpointScheduleAuth(request),
            Enabled: true,
            IntervalMinutes: DesktopNodeApiJsonReader.ReadInt(parsed.Value!.Value, "interval_minutes"),
            RetentionMax: DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "retention_max"),
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            TemplateLocked: DesktopNodeApiJsonReader.ReadBool(vm.Value, "template_lock"),
            CurrentCheckpointCount: currentCount is { } count &&
                count.ValueKind == JsonValueKind.Number &&
                count.TryGetInt32(out var parsedCount)
                ? parsedCount
                : 0));
        if (!evaluation.Ok)
        {
            return MapCheckpointScheduleError(operation, evaluation.ErrorCode!);
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(
            true,
            operation,
            new SortedDictionary<string, object?>
            {
                ["action"] = evaluation.Action,
                ["dry_run"] = true,
                ["host_mutation_performed"] = false,
                ["interval_minutes"] = evaluation.IntervalMinutes,
                ["retention_max"] = evaluation.RetentionMax,
                ["schema"] = CheckpointSchedulePolicy.Schema,
                ["vm_name"] = evaluation.VmName
            },
            null));
    }

    private DesktopNodeApiResponse HandleCheckpointScheduleSet(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch,
        CancellationToken cancellationToken)
    {
        const string operation = "checkpoint.schedule.set";
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

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return DesktopNodeApiResponseFactory.OperationResponse(inventory);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory.Data, routeId.Value!);
        if (vm is null)
        {
            return DesktopNodeApiResponseFactory.Failure(
                404,
                operation,
                "PCV_VM_NOT_FOUND",
                $"VM '{routeId.Value}' was not found.",
                "The VM was not present in the current Hyper-V inventory response.",
                false);
        }

        var currentCount = DesktopNodeApiJsonReader.ReadNestedElement(vm.Value, "checkpoints", "count");
        var evaluation = CheckpointSchedulePolicy.EvaluateSet(new CheckpointScheduleRequest(
            routeId.Value,
            authSessionHandler.ResolveCheckpointScheduleAuth(request),
            Enabled: true,
            IntervalMinutes: DesktopNodeApiJsonReader.ReadInt(parsed.Value!.Value, "interval_minutes"),
            RetentionMax: DesktopNodeApiJsonReader.ReadInt(parsed.Value.Value, "retention_max"),
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            TemplateLocked: DesktopNodeApiJsonReader.ReadBool(vm.Value, "template_lock"),
            CurrentCheckpointCount: currentCount is { } count &&
                count.ValueKind == JsonValueKind.Number &&
                count.TryGetInt32(out var parsedCount)
                ? parsedCount
                : 0));
        if (!evaluation.Ok)
        {
            return MapCheckpointScheduleError(operation, evaluation.ErrorCode!);
        }

        var parameters = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["interval_minutes"] = evaluation.IntervalMinutes,
            ["reconciliation"] = checkpointScheduleStore.CaptureReconciliation(operation, evaluation),
            ["retention_max"] = evaluation.RetentionMax,
            ["vm_name"] = evaluation.VmName
        });
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(operation, parameters, request.RequestId!));
    }

    private DesktopNodeApiResponse HandleCheckpointScheduleClear(
        DesktopNodeApiRequest request,
        DesktopNodeApiRouteMatch routeMatch)
    {
        const string operation = "checkpoint.schedule.clear";
        var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(routeMatch.Parameters["vmId"], operation);
        if (!routeId.Ok)
        {
            return routeId.Response!;
        }

        var evaluation = CheckpointSchedulePolicy.EvaluateClear(new CheckpointScheduleRequest(
            routeId.Value,
            authSessionHandler.ResolveCheckpointScheduleAuth(request)));
        if (!evaluation.Ok)
        {
            return MapCheckpointScheduleError(operation, evaluation.ErrorCode!);
        }

        var parameters = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["reconciliation"] = checkpointScheduleStore.CaptureReconciliation(operation, evaluation),
            ["vm_name"] = evaluation.VmName
        });
        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(operation, parameters, request.RequestId!));
    }

    private static DesktopNodeApiResponse MapCheckpointScheduleError(string operation, string code)
    {
        var forbidden = string.Equals(code, CheckpointScheduleProblemCodes.Forbidden, StringComparison.Ordinal);
        var conflict = string.Equals(code, CheckpointScheduleProblemCodes.CapacityExceeded, StringComparison.Ordinal);
        return DesktopNodeApiResponseFactory.Failure(
            forbidden ? 403 : conflict ? 409 : 400,
            operation,
            code,
            forbidden
                ? "The current account role is not allowed to configure a checkpoint schedule."
                : "The checkpoint schedule request was rejected.",
            forbidden
                ? "Grant operate or use the service bearer."
                : "Pass a managed VM, interval 60-10080 minutes, and retention 1-32.",
            false);
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
        string missingAction,
        CancellationToken cancellationToken)
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

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            reconciliationHandler.BuildVmQosParameters(
                operation,
                routeId.Value!,
                targetProperty,
                target!,
                requiredValueProperty,
                maximum.Value,
                optionalValueProperty,
                minimum,
                cancellationToken),
            request.RequestId!));
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
