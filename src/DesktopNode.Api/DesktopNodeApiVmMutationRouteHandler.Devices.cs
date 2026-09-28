using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiVmMutationRouteHandler
{
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
        // Native inventory lists VHDs only in storage and reports DVD drives in dvd_drives.count.
        if (vm.TryGetProperty("dvd_drives", out var dvdDrives) &&
            dvdDrives.ValueKind == JsonValueKind.Object &&
            dvdDrives.TryGetProperty("count", out var dvdCount) &&
            dvdCount.ValueKind == JsonValueKind.Number &&
            dvdCount.TryGetInt32(out var reported))
        {
            return reported;
        }

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
}
