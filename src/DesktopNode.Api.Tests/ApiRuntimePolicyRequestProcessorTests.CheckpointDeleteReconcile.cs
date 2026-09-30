using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    private const string CheckpointDeleteTarget = """{"name":"cp-1","vm_name":"lab-vm","created_at":"2026-09-30T01:00:00.0000000Z","is_current":false}""";
    private const string CheckpointDeleteReplacement = """{"name":"CP-1","vm_name":"lab-vm","created_at":"2026-09-30T02:00:00.0000000Z","is_current":true}""";

    [Fact]
    public void CheckpointDeleteQueueCapturesTargetIdentityWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["checkpoint.list"] = CheckpointDeleteList(CheckpointDeleteTarget)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("DELETE", "/api/v1/vms/lab-vm/checkpoints/cp-1"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["checkpoint.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var parameters = document.RootElement.GetProperty("data").GetProperty("params");
        Assert.Equal("cp-1", parameters.GetProperty("checkpoint_name").GetString());
        Assert.Equal("lab-vm", parameters.GetProperty("vm_name").GetString());
        var reconciliation = parameters.GetProperty("reconciliation");
        Assert.Equal("pcv-checkpoint-delete-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("2026-09-30T01:00:00.0000000Z", reconciliation.GetProperty("before").GetProperty("created_at").GetString());
        Assert.Equal("absent", reconciliation.GetProperty("expected_after").GetProperty("state").GetString());
    }

    [Fact]
    public void CheckpointDeleteQueueRecordsUnavailableBaselineWhenTargetIsMissing()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["checkpoint.list"] = CheckpointDeleteList()
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("DELETE", "/api/v1/vms/lab-vm/checkpoints/cp-1"));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("unavailable", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("PCV_CHECKPOINT_NOT_FOUND", reconciliation.GetProperty("capture_error_code").GetString());
    }

    [Theory]
    [InlineData("", 200, "postcondition-confirmed")]
    [InlineData(CheckpointDeleteTarget, 409, "not-applied")]
    [InlineData(CheckpointDeleteReplacement, 409, "replacement-observed")]
    [InlineData(CheckpointDeleteTarget + "," + CheckpointDeleteReplacement, 409, "ambiguous-duplicate-names")]
    public void CheckpointDeleteReconcileJudgesReadbackWithoutCallingDeleteProvider(
        string observedCheckpoints,
        int expectedStatusCode,
        string expectedClassification)
    {
        using var store = new CheckpointDeleteJobStore(captured: true);
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["checkpoint.list"] = CheckpointDeleteList(observedCheckpoints)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-checkpoint-delete/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(["checkpoint.list"], nativeCalls);
        Assert.Contains(expectedClassification, response.Body, StringComparison.Ordinal);
        if (expectedStatusCode == 200)
        {
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("succeeded", data.GetProperty("status").GetString());
            Assert.Equal("checkpoint.delete", data.GetProperty("result").GetProperty("operation").GetString());
        }
        else
        {
            Assert.Contains("PCV_JOB_RECONCILIATION_REQUIRED", response.Body, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CheckpointDeleteReconcileRequiresBaselineAndReadback()
    {
        using var missingBaseline = new CheckpointDeleteJobStore(captured: false);
        var baselineProcessor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: missingBaseline.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["checkpoint.list"] = CheckpointDeleteList()
            }));

        var baseline = baselineProcessor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-checkpoint-delete/reconcile"));

        Assert.Equal(409, baseline.StatusCode);
        Assert.Contains("baseline-unavailable", baseline.Body, StringComparison.Ordinal);

        using var readbackStore = new CheckpointDeleteJobStore(captured: true);
        var readbackProcessor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: readbackStore.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["checkpoint.list"] = """{"ok":false,"operation":"checkpoint.list","data":null,"error":{"code":"PCV_HYPERV_UNAVAILABLE","message":"Unavailable.","detail":"","retryable":true,"recommended_action":"Retry."}}"""
            }));

        var readback = readbackProcessor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-checkpoint-delete/reconcile"));

        Assert.Equal(409, readback.StatusCode);
        Assert.Contains("readback-unavailable", readback.Body, StringComparison.Ordinal);
    }

    private static string CheckpointDeleteList(string items = "")
    {
        return "{\"ok\":true,\"operation\":\"checkpoint.list\",\"data\":[" + items + "],\"error\":null}";
    }

    private sealed class CheckpointDeleteJobStore : IDisposable
    {
        public CheckpointDeleteJobStore(bool captured)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pcv-dotnet-api-checkpoint-delete-" + Guid.NewGuid().ToString("N") + ".json");
            var captureStatus = captured ? "captured" : "unavailable";
            var before = captured ? CheckpointDeleteTarget : "null";
            File.WriteAllText(Path, $$"""
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-checkpoint-delete",
                  "operation": "checkpoint.delete",
                  "status": "failed",
                  "params": {
                    "checkpoint_name": "cp-1",
                    "vm_name": "lab-vm",
                    "reconciliation": {
                      "schema": "pcv-checkpoint-delete-reconciliation/v1",
                      "capture_status": "{{captureStatus}}",
                      "before": {{before}},
                      "expected_after": { "state": "absent", "name": "cp-1", "vm_name": "lab-vm" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-checkpoint-delete",
                  "correlation_id": "corr-checkpoint-delete",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-09-30T00:00:00.0000000Z",
                  "updated_at": "2026-09-30T00:00:01.0000000Z"
                }
              ],
              "queue": []
            }
            """);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
