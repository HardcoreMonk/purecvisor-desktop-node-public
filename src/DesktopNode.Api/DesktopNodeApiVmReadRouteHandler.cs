using DesktopNode.HyperV;

namespace DesktopNode.Api;

// GET 조회 경로는 전부 같은 모양이다: route id 를 디코딩하고, native operation 을 하나 호출하고,
// 그 결과를 그대로 응답으로 옮긴다. HandleCore 에 남겨 두면 dispatcher 가 아니라 여전히 구현이
// 되므로 함께 떼어낸다. 이 소유자는 GET 경로의 종단이라 null 을 돌려주지 않는다 - 아무 것도
// 맞지 않으면 404 를 내는 것까지가 이 소유자의 계약이다.
internal sealed class DesktopNodeApiVmReadRouteHandler
{
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;

    // vm.delete-status 는 provider 를 읽지 않고 job 상태를 읽는다. GET 경로 종단에 함께 있던
    // 분기라 여기로 따라왔고, HandleCore 의 평가 순서를 바꾸지 않으려고 job route 소유자를
    // 옮기는 대신 의존성으로 받는다.
    private readonly DesktopNodeApiJobRouteHandler jobRouteHandler;

    public DesktopNodeApiVmReadRouteHandler(
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        DesktopNodeApiJobRouteHandler jobRouteHandler)
    {
        this.operationInvoker = operationInvoker;
        this.jobRouteHandler = jobRouteHandler;
    }

    public DesktopNodeApiResponse Handle(string method, string path, CancellationToken cancellationToken)
    {
        if (method == "GET" && DesktopNodeApiRequestParsing.TryMatch(path, "^/api/v1/vms/([^/]*)/checkpoints$", out var checkpointListMatch))
        {
            var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(checkpointListMatch.Groups[1].Value, "checkpoint.list");
            if (!routeId.Ok)
            {
                return routeId.Response!;
            }

            return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
                "checkpoint.list",
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["vm_name"] = routeId.Value
                }),
                cancellationToken));
        }

        if (method == "GET" && DesktopNodeApiRequestParsing.TryMatch(path, "^/api/v1/vms/([^/]*)/(memory-stats|cpu-stats)$", out var vmStatsMatch))
        {
            var statsOperation = string.Equals(vmStatsMatch.Groups[2].Value, "memory-stats", StringComparison.OrdinalIgnoreCase)
                ? "vm.memory-stats"
                : "vm.cpu-stats";
            var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(vmStatsMatch.Groups[1].Value, statsOperation);
            if (!routeId.Ok)
            {
                return routeId.Response!;
            }

            return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
                statsOperation,
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["vm_name"] = routeId.Value
                }),
                cancellationToken));
        }

        if (method == "GET" && DesktopNodeApiRequestParsing.TryMatch(path, "^/api/v1/vms/([^/]*)/(blkio|bandwidth)$", out var vmQosReadbackMatch))
        {
            var qosOperation = string.Equals(vmQosReadbackMatch.Groups[2].Value, "blkio", StringComparison.OrdinalIgnoreCase)
                ? "vm.blkio-get"
                : "vm.bandwidth";
            var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(vmQosReadbackMatch.Groups[1].Value, qosOperation);
            if (!routeId.Ok)
            {
                return routeId.Response!;
            }

            return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
                qosOperation,
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["vm_name"] = routeId.Value
                }),
                cancellationToken));
        }

        if (method == "GET" && DesktopNodeApiRequestParsing.TryMatch(path, "^/api/v1/vms/([^/]*)/guest-agent/(status|ping)$", out var vmGuestAgentMatch))
        {
            var guestOperation = string.Equals(vmGuestAgentMatch.Groups[2].Value, "status", StringComparison.OrdinalIgnoreCase)
                ? "vm.guest-agent-status"
                : "vm.guest-ping";
            var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(vmGuestAgentMatch.Groups[1].Value, guestOperation);
            if (!routeId.Ok)
            {
                return routeId.Response!;
            }

            return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(
                guestOperation,
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["vm_name"] = routeId.Value
                }),
                cancellationToken));
        }

        if (method == "GET" && DesktopNodeApiRequestParsing.TryMatch(path, "^/api/v1/vms/([^/]*)/delete-status$", out var vmDeleteStatusMatch))
        {
            var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(vmDeleteStatusMatch.Groups[1].Value, "vm.delete-status");
            if (!routeId.Ok)
            {
                return routeId.Response!;
            }

            return jobRouteHandler.HandleVmDeleteStatus(routeId.Value!);
        }

        if (method == "GET" && DesktopNodeApiRequestParsing.TryMatch(path, "^/api/v1/vms/([^/]+)$", out var vmDetailMatch))
        {
            var routeId = DesktopNodeApiRequestParsing.DecodeRouteId(vmDetailMatch.Groups[1].Value, "vm.get");
            if (!routeId.Ok)
            {
                return routeId.Response!;
            }

            var operationResult = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), cancellationToken);
            if (!operationResult.Ok)
            {
                return DesktopNodeApiResponseFactory.OperationResponse(operationResult);
            }

            var vm = DesktopNodeApiJsonReader.FindVm(operationResult.Data, routeId.Value!);
            if (vm is null)
            {
                return DesktopNodeApiResponseFactory.Failure(404, "vm.get", "PCV_VM_NOT_FOUND", $"VM '{routeId.Value}' was not found.", "The VM was not present in the current Hyper-V inventory response.", false);
            }

            return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "vm.get", vm.Value, null));
        }

        var operation = path switch
        {
            "/api/v1/host/status" => "host.status",
            "/api/v1/network/inventory" => "network.inventory",
            "/api/v1/vms" => "vm.list",
            _ => null
        };

        if (operation is null)
        {
            return DesktopNodeApiResponseFactory.Failure(404, "api.route", "PCV_ROUTE_NOT_FOUND", $"No route matches '{path}'.", "The requested route is not part of the Desktop Node API contract.", false);
        }

        return DesktopNodeApiResponseFactory.OperationResponse(operationInvoker.Invoke(operation, DesktopNodeApiResponseFactory.EmptyObject(), cancellationToken));
    }
}
