using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiVmMutationRouteHandler
{
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

        var jobParameters = new SortedDictionary<string, object?>
        {
            ["allowed_root"] = allowedRoot,
            ["apply_managed_marker"] = evaluation.ApplyManagedMarker,
            ["directory"] = evaluation.Directory,
            ["generate_new_id"] = evaluation.GenerateNewId,
            ["has_vmcx"] = true,
            ["name"] = evaluation.VmName,
            ["package_kind"] = evaluation.PackageKind,
            ["vm_name"] = evaluation.VmName
        };
        var vmRoot = DesktopNodeApiJsonReader.GetStringProperty(parsed.Value.Value, "vm_root");
        if (!string.IsNullOrWhiteSpace(vmRoot))
        {
            jobParameters["vm_root"] = vmRoot;
        }

        return DesktopNodeApiResponseFactory.JobCreated(CreateJob(
            operation,
            DesktopNodeApiResponseFactory.JsonFromObject(jobParameters),
            request.RequestId!));
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
}
