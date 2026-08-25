using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeApiOpsSummaryHandlerTests
{
    [Fact]
    public void TryHandleReturnsNullWithoutReadingQueryForNonOpsRoute()
    {
        var query = new StubOpsSummaryQuery(CreateSnapshot());
        var handler = new DesktopNodeApiOpsSummaryHandler(query);

        var response = handler.TryHandle("GET", "/api/v1/runtime/policy");

        Assert.Null(response);
        Assert.Equal(0, query.ReadCount);
    }

    [Fact]
    public void TryHandleOwnsOpsEnvelopeAndLeavesRequestIdAttachmentToFacade()
    {
        var query = new StubOpsSummaryQuery(CreateSnapshot());
        var handler = new DesktopNodeApiOpsSummaryHandler(query);
        using var cancellation = new CancellationTokenSource();

        var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
            "GET",
            "/api/v1/ops/summary",
            cancellation.Token));

        Assert.Equal(1, query.ReadCount);
        Assert.Equal(cancellation.Token, query.LastCancellationToken);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);
        using var document = JsonDocument.Parse(response.Body);
        Assert.True(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("ops.summary", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("error").ValueKind);
        Assert.False(document.RootElement.TryGetProperty("request_id", out _));
        Assert.Equal(
            1,
            document.RootElement.GetProperty("data").GetProperty("vm_counts").GetProperty("total").GetInt32());
    }

    [Fact]
    public void QueryOwnsOrderedSnapshotPolicyNativeAndEvidenceReads()
    {
        var runtime = new DesktopNodeJobRuntime();
        runtime.Create(
            new DesktopNodeJobCreateCommand(
                "vm.create",
                JsonSerializer.SerializeToElement(new { name = "queued-vm" })),
            new DesktopNodeJobRequestContext("req-queued"));
        var native = new RecordingOpsSummaryNativeAdapter();
        var query = new DesktopNodeApiOpsSummaryQuery(
            native,
            runtime,
            new DesktopNodeApiAuthSessionHandler(DesktopNodeAccountAuthOptions.Disabled),
            "credential-manager",
            "loopback",
            new DesktopNodeConsoleOptions(Enabled: true, NoVncEnabled: true),
            batchEvidenceRoot: null,
            diagnosticsRoot: "D:\\ProgramData\\PureCVisor\\diagnostics");
        using var cancellation = new CancellationTokenSource();

        var snapshot = query.Read(cancellation.Token);

        Assert.Equal(["host.status", "vm.list"], native.Calls.Select(call => call.Operation).ToArray());
        Assert.All(native.Calls, call => Assert.Equal("{}", call.Parameters.GetRawText()));
        Assert.All(native.Calls, call => Assert.Equal(cancellation.Token, call.CancellationToken));
        var job = Assert.Single(snapshot.JobRows);
        Assert.Equal("queued", job.GetProperty("status").GetString());
        Assert.Equal("req-queued", job.GetProperty("request_id").GetString());
        Assert.Equal("credential-manager", snapshot.RuntimePolicy.Auth.TokenStorage);
        Assert.Equal("available", snapshot.RuntimePolicy.Console.NoVnc);
        Assert.Equal("not_configured", snapshot.BatchEvidence.GetProperty("status").GetString());
        Assert.Equal("D:\\ProgramData\\PureCVisor\\diagnostics", snapshot.DiagnosticsRoot);
    }

    [Fact]
    public void NativeReadFailuresRemainSuccessfulDegradedOpsResponse()
    {
        var snapshot = CreateSnapshot(
            DesktopNodeHyperVOperationResult.Failure(
                "host.status",
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                "Host status was unavailable.",
                "The test query intentionally returned a native failure.",
                retryable: false),
            DesktopNodeHyperVOperationResult.Failure(
                "vm.list",
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                "VM inventory was unavailable.",
                "The test query intentionally returned a native failure.",
                retryable: false));
        var handler = new DesktopNodeApiOpsSummaryHandler(new StubOpsSummaryQuery(snapshot));

        var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
            "GET",
            "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(2, data.GetProperty("errors").GetArrayLength());
        Assert.Equal(0, data.GetProperty("vm_counts").GetProperty("total").GetInt32());
        Assert.Contains(
            data.GetProperty("signals").EnumerateArray(),
            signal => signal.GetProperty("key").GetString() == "host-readiness" &&
                signal.GetProperty("tone").GetString() == "warn");
    }

    [Theory]
    [InlineData("not_configured", "ok")]
    [InlineData("available", "ok")]
    [InlineData("degraded", "warn")]
    [InlineData("missing", "warn")]
    [InlineData("unavailable", "warn")]
    public void EvidenceStatusPreservesCurrentRollupAndSignalTone(
        string status,
        string expectedTone)
    {
        using var evidenceDocument = JsonDocument.Parse($$"""
            {"configured":true,"errors":[],"latest":null,"schema_version":1,"status":"{{status}}"}
            """);
        var baseline = CreateSnapshot();
        var snapshot = baseline with { BatchEvidence = evidenceDocument.RootElement.Clone() };
        var handler = new DesktopNodeApiOpsSummaryHandler(new StubOpsSummaryQuery(snapshot));

        var response = Assert.IsType<DesktopNodeApiResponse>(handler.TryHandle(
            "GET",
            "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var responseDocument = JsonDocument.Parse(response.Body);
        var data = responseDocument.RootElement.GetProperty("data");
        Assert.Equal(status, data.GetProperty("batch_evidence").GetProperty("status").GetString());
        var latest = data
            .GetProperty("current_evidence")
            .GetProperty("full_admin_host_mutation")
            .GetProperty("latest");
        Assert.Equal(status, latest.GetProperty("evidence_status").GetString());
        Assert.Equal(status, latest.GetProperty("status").GetString());
        Assert.Contains(
            data.GetProperty("signals").EnumerateArray(),
            signal => signal.GetProperty("key").GetString() == "batch-evidence" &&
                signal.GetProperty("tone").GetString() == expectedTone &&
                signal.GetProperty("value").GetString() == status);
    }

    private static DesktopNodeApiOpsSummarySnapshot CreateSnapshot(
        DesktopNodeHyperVOperationResult? hostResult = null,
        DesktopNodeHyperVOperationResult? vmResult = null)
    {
        return new DesktopNodeApiOpsSummarySnapshot(
            hostResult ?? DesktopNodeHyperVOperationResult.FromJson(
                """
                {"ok":true,"operation":"host.status","data":{"supported":true},"error":null}
                """),
            vmResult ?? DesktopNodeHyperVOperationResult.FromJson(
                """
                {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running","checkpoints":{"count":0}}],"error":null}
                """),
            [],
            RuntimePolicyContract.CreateDefault().Data,
            new BatchEvidenceSummaryReader(root: null).Read(),
            DiagnosticsRoot: null);
    }

    private sealed class StubOpsSummaryQuery(DesktopNodeApiOpsSummarySnapshot snapshot)
        : IDesktopNodeApiOpsSummaryQuery
    {
        public int ReadCount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public DesktopNodeApiOpsSummarySnapshot Read(CancellationToken cancellationToken)
        {
            ReadCount++;
            LastCancellationToken = cancellationToken;
            return snapshot;
        }
    }

    private sealed class RecordingOpsSummaryNativeAdapter : IDesktopNodeHyperVNativeAdapter
    {
        public List<RecordedCall> Calls { get; } = [];

        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            Calls.Add(new RecordedCall(operation, parameters.Clone(), cancellationToken));
            result = operation switch
            {
                "host.status" => DesktopNodeHyperVOperationResult.FromJson(
                    """
                    {"ok":true,"operation":"host.status","data":{"supported":true},"error":null}
                    """),
                "vm.list" => DesktopNodeHyperVOperationResult.FromJson(
                    """
                    {"ok":true,"operation":"vm.list","data":[],"error":null}
                    """),
                _ => DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_ROUTE_NOT_HANDLED",
                    "The test adapter did not handle the operation.",
                    "Only ops-summary reads are expected.",
                    retryable: false)
            };
            return result.Ok;
        }
    }

    private sealed record RecordedCall(
        string Operation,
        JsonElement Parameters,
        CancellationToken CancellationToken);
}
