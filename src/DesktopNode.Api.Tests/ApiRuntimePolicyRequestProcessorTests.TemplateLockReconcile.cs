using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    [Fact]
    public void VmTemplateLockQueueCapturesBeforeLockWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = TemplateLockVmList("vm-id", locked: false)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/template-lock",
            """{"confirm_name":"lab-vm","locked":true}"""));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var parameters = document.RootElement.GetProperty("data").GetProperty("params");
        Assert.True(parameters.GetProperty("locked").GetBoolean());
        var reconciliation = parameters.GetProperty("reconciliation");
        Assert.Equal("pcv-vm-template-lock-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.False(reconciliation.GetProperty("before_template_lock").GetBoolean());
        Assert.True(reconciliation.GetProperty("expected_after").GetProperty("template_lock").GetBoolean());
    }

    [Theory]
    [InlineData(true, "vm-id", true, 200, "postcondition-confirmed")]
    [InlineData(true, "vm-id", false, 409, "not-applied")]
    [InlineData(false, "vm-id", false, 200, "postcondition-confirmed")]
    [InlineData(false, "vm-id", true, 409, "not-applied")]
    [InlineData(true, "other-vm-id", true, 409, "identity-mismatch")]
    public void VmTemplateLockReconcileJudgesReadbackWithoutCallingLockProvider(
        bool requested,
        string observedId,
        bool observedLocked,
        int expectedStatusCode,
        string expectedClassification)
    {
        using var store = new TemplateLockJobStore(requested, captured: true);
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = TemplateLockVmList(observedId, observedLocked)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-template-lock/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        Assert.Contains(expectedClassification, response.Body, StringComparison.Ordinal);
        if (expectedStatusCode == 200)
        {
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("succeeded", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        }
    }

    [Fact]
    public void VmTemplateLockReconcileRequiresCapturedBaseline()
    {
        using var store = new TemplateLockJobStore(requested: true, captured: false);
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = TemplateLockVmList("vm-id", locked: true)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-template-lock/reconcile"));

        Assert.Equal(409, response.StatusCode);
        Assert.Contains("baseline-unavailable", response.Body, StringComparison.Ordinal);
    }

    private static string TemplateLockVmItem(string id, bool locked)
    {
        return "{\"id\":\"" + id + "\",\"name\":\"lab-vm\",\"platform\":\"hyperv\",\"guest_family\":\"windows\",\"state\":\"off\"," +
            "\"cpu\":{\"count\":2},\"memory\":{\"startup_mb\":4096},\"generation\":2,\"managed_by_purecvisor\":true" +
            (locked ? ",\"template_lock\":true" : string.Empty) + "}";
    }

    private static string TemplateLockVmList(string id, bool locked)
    {
        return "{\"ok\":true,\"operation\":\"vm.list\",\"data\":[" + TemplateLockVmItem(id, locked) + "],\"error\":null}";
    }

    private sealed class TemplateLockJobStore : IDisposable
    {
        public TemplateLockJobStore(bool requested, bool captured)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pcv-dotnet-api-template-lock-" + Guid.NewGuid().ToString("N") + ".json");
            var requestedJson = requested ? "true" : "false";
            File.WriteAllText(Path, $$"""
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-template-lock",
                  "operation": "vm.template.lock",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "locked": {{requestedJson}},
                    "reconciliation": {
                      "schema": "pcv-vm-template-lock-reconciliation/v1",
                      "capture_status": "{{(captured ? "captured" : "unavailable")}}",
                      "before": {{(captured ? TemplateLockVmItem("vm-id", !requested) : "null")}},
                      "before_fingerprint": {{(captured ? "{ \"id\": \"vm-id\", \"platform\": \"hyperv\", \"guest_family\": \"windows\", \"generation\": 2, \"managed_by_purecvisor\": true }" : "null")}},
                      "before_template_lock": {{(requested ? "false" : "true")}},
                      "expected_after": { "name": "lab-vm", "template_lock": {{requestedJson}} }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-template-lock",
                  "correlation_id": "corr-template-lock",
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
