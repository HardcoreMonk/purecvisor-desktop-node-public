using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    [Fact]
    public void VmManageQueueCapturesReadbackBaselineWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = ReadbackVmList("vm-id", managed: false, "Default Switch")
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/manage",
            """{"confirm_name":"lab-vm"}"""));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-vm-manage-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("vm-id", reconciliation.GetProperty("before").GetProperty("id").GetString());
    }

    [Theory]
    [InlineData("vm-id", true, 200, "postcondition-confirmed")]
    [InlineData("vm-id", false, 409, "not-applied")]
    [InlineData("other-vm-id", true, 409, "identity-mismatch")]
    public void VmManageReconcileJudgesManagedMarker(string observedId, bool observedManaged, int expectedStatusCode, string expectedClassification)
    {
        using var store = new ReadbackJobStore(
            "vm.manage",
            "\"name\": \"lab-vm\"",
            "pcv-vm-manage-reconciliation/v1",
            ReadbackVmItem("vm-id", managed: false),
            "\"managed_by_purecvisor\": true");
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = ReadbackVmList(observedId, observedManaged)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-readback/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Contains(expectedClassification, response.Body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(new[] { "Default Switch" }, new[] { "lab-switch" }, 200, "postcondition-confirmed")]
    [InlineData(new string[0], new[] { "lab-switch" }, 200, "postcondition-confirmed")]
    [InlineData(new[] { "Default Switch" }, new[] { "Default Switch" }, 409, "not-applied")]
    [InlineData(new[] { "Default Switch" }, new[] { "lab-switch", "Default Switch" }, 409, "ambiguous-adapter-state")]
    [InlineData(new[] { "lab-switch" }, new[] { "lab-switch" }, 409, "not-applied")]
    public void VmNetworkConnectReconcileRequiresASingleNewlyConnectedSwitch(
        string[] beforeSwitches,
        string[] observedSwitches,
        int expectedStatusCode,
        string expectedClassification)
    {
        using var store = new ReadbackJobStore(
            "vm.network.connect",
            "\"vm_name\": \"lab-vm\", \"switch\": \"lab-switch\"",
            "pcv-vm-network-connect-reconciliation/v1",
            ReadbackVmItem("vm-id", managed: true, beforeSwitches),
            "\"switch\": \"lab-switch\"");
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = ReadbackVmList("vm-id", true, observedSwitches)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-readback/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        Assert.Contains(expectedClassification, response.Body, StringComparison.Ordinal);
    }

    private static string ReadbackVmItem(string id, bool managed, params string[] switches)
    {
        var network = string.Join(",", switches.Select(name => "{\"switch\":\"" + name + "\",\"mode\":\"hyperv-switch\"}"));
        return "{\"id\":\"" + id + "\",\"name\":\"lab-vm\",\"platform\":\"hyperv\",\"guest_family\":\"windows\",\"state\":\"off\"," +
            "\"cpu\":{\"count\":2},\"memory\":{\"startup_mb\":4096},\"generation\":2,\"managed_by_purecvisor\":" + (managed ? "true" : "false") +
            ",\"network\":[" + network + "]}";
    }

    private static string ReadbackVmList(string id, bool managed, params string[] switches)
    {
        return "{\"ok\":true,\"operation\":\"vm.list\",\"data\":[" + ReadbackVmItem(id, managed, switches) + "],\"error\":null}";
    }

    private sealed class ReadbackJobStore : IDisposable
    {
        public ReadbackJobStore(string operation, string parameters, string schema, string before, string expectedAfter)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pcv-dotnet-api-readback-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(Path, $$"""
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-readback",
                  "operation": "{{operation}}",
                  "status": "failed",
                  "params": {
                    {{parameters}},
                    "reconciliation": {
                      "schema": "{{schema}}",
                      "capture_status": "captured",
                      "before": {{before}},
                      "expected_after": { "name": "lab-vm", {{expectedAfter}} }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-readback",
                  "correlation_id": "corr-readback",
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
