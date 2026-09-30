using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiVmMutationRouteHandler
{
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
                        reconciliationHandler.BuildCheckpointDeleteParameters(routeId.Value!, checkpointId.Value!, cancellationToken),
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
                        DesktopNodeApiJobReconciliationHandler.ExpectedPowerState(lifecycleOperation) is not null
                            ? reconciliationHandler.BuildVmPowerStateParameters(lifecycleOperation, routeId.Value!, cancellationToken)
                            : DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
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
                    "Pass a JSON body with numeric memory_mb.",
                    cancellationToken);

            case "QueueSetVmVcpu":
                return QueueVmResourceMutation(
                    request,
                    routeMatch,
                    "vm.set-vcpu",
                    "cpu",
                    "PCV_VM_CPU_VALUE_REQUIRED",
                    "VM vCPU value is required.",
                    "Pass a JSON body with numeric cpu.",
                    cancellationToken);

            case "QueueResizeVmDisk":
                return QueueVmResourceMutation(
                    request,
                    routeMatch,
                    "vm.disk-resize",
                    "disk_gb",
                    "PCV_VM_DISK_SIZE_VALUE_REQUIRED",
                    "VM disk resize value is required.",
                    "Pass a JSON body with numeric disk_gb.",
                    cancellationToken);

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
}
