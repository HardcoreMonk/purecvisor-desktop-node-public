using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 큐에 올리는 VM 변경과 QoS 경로.
// 예약 시점에 조정 baseline 을 캡처해야
// 하므로 reconciliation 소유자를 소비한다.
internal sealed partial class DesktopNodeApiVmMutationRouteHandler
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

        var requestedValue = DesktopNodeApiJsonReader.ReadInt(parsed.Value!.Value, valueProperty);
        if (requestedValue is null)
        {
            return DesktopNodeApiResponseFactory.Failure(400, operation, missingCode, missingMessage, missingAction, false);
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            reconciliationHandler.BuildVmResourceParameters(operation, routeId.Value!, requestedValue.Value, cancellationToken),
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
