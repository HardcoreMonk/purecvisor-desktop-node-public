using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiOpsSummaryContractCharacterizationTests
{
    [Fact]
    public void OpsSummaryPinsQueryOrderCancellationAndResponseEnvelope()
    {
        var adapter = new RecordingOpsSummaryAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            tokenStorage: "dpapi-local-machine",
            currentExposure: "loopback",
            nativeAdapter: adapter);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/ops/summary",
            RequestId: "req-wave1d-characterization"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);
        Assert.Equal(["host.status", "vm.list"], adapter.Calls.Select(call => call.Operation).ToArray());
        Assert.All(adapter.Calls, call => Assert.Equal("{}", call.Parameters.GetRawText()));
        Assert.All(adapter.Calls, call => Assert.True(call.CancellationCanBeRequested));

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        Assert.Equal(
            ["data", "error", "ok", "operation", "request_id"],
            root.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal("ops.summary", root.GetProperty("operation").GetString());
        Assert.Equal("req-wave1d-characterization", root.GetProperty("request_id").GetString());

        var data = root.GetProperty("data");
        Assert.Equal(
            [
                "batch_evidence",
                "current_evidence",
                "errors",
                "host",
                "installed_runtime",
                "job_counts",
                "job_store",
                "recent_activity",
                "runtime_policy",
                "signals",
                "vm_counts"
            ],
            data.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.True(data.GetProperty("host").GetProperty("supported").GetBoolean());
        Assert.Equal(1, data.GetProperty("vm_counts").GetProperty("total").GetInt32());
        Assert.Equal(1, data.GetProperty("vm_counts").GetProperty("running").GetInt32());
        Assert.All(
            data.GetProperty("job_counts").EnumerateObject(),
            count => Assert.Equal(0, count.Value.GetInt32()));
        Assert.Equal("not_configured", data.GetProperty("batch_evidence").GetProperty("status").GetString());
        Assert.Equal(
            "dpapi-local-machine",
            data.GetProperty("runtime_policy").GetProperty("auth").GetProperty("token_storage").GetString());
    }

    [Fact]
    public void NonOpsRouteDoesNotIssueOpsSummaryQueries()
    {
        var adapter = new RecordingOpsSummaryAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: adapter);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/runtime/policy",
            RequestId: "req-wave1d-non-ops"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(adapter.Calls);
    }

    private sealed class RecordingOpsSummaryAdapter : IDesktopNodeHyperVNativeAdapter
    {
        public List<RecordedCall> Calls { get; } = [];

        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            Calls.Add(new RecordedCall(
                operation,
                parameters.Clone(),
                cancellationToken.CanBeCanceled));

            result = operation switch
            {
                "host.status" => DesktopNodeHyperVOperationResult.FromJson(
                    """
                    {"ok":true,"operation":"host.status","data":{"supported":true,"admin":{"elevated":true},"hyperv":{"vmms_running":true,"feature_enabled":true,"default_switch_present":true}},"error":null}
                    """),
                "vm.list" => DesktopNodeHyperVOperationResult.FromJson(
                    """
                    {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running","checkpoints":{"count":0}}],"error":null}
                    """),
                _ => DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_ROUTE_NOT_HANDLED",
                    "The test adapter did not handle the operation.",
                    "Only ops-summary reads are expected in this characterization fixture.",
                    retryable: false)
            };
            return result.Ok;
        }
    }

    private sealed record RecordedCall(
        string Operation,
        JsonElement Parameters,
        bool CancellationCanBeRequested);
}
