using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    private const string ResourceDiskPath = @"D:\\PureCVisor\\lab-vm\\lab-vm.vhdx";

    [Theory]
    [InlineData("set-memory", "vm.set-memory", "memory_mb", 8192, 4096)]
    [InlineData("set-vcpu", "vm.set-vcpu", "cpu", 4, 2)]
    [InlineData("disk-resize", "vm.disk-resize", "disk_gb", 128, 64)]
    public void VmResourceQueueCapturesRequestedAndBeforeValuesWithoutMutatingProvider(
        string routeAction,
        string operation,
        string valueProperty,
        int requested,
        int before)
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = ResourceVmList("vm-id", 4096, 2, 64),
                ["vm.disk.inspect"] = DiskInspectResponse(64)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            $"/api/v1/vms/lab-vm/{routeAction}",
            $"{{\"{valueProperty}\":{requested}}}"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(ExpectedResourceCalls(operation), nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var parameters = document.RootElement.GetProperty("data").GetProperty("params");
        Assert.Equal(requested, parameters.GetProperty(valueProperty).GetInt32());
        var reconciliation = parameters.GetProperty("reconciliation");
        Assert.Equal("pcv-vm-resource-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal(operation, reconciliation.GetProperty("operation").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal(operation == "vm.disk-resize" ? before * ResourceGiB : before, reconciliation.GetProperty("before_value").GetInt64());
        Assert.Equal(requested, reconciliation.GetProperty("expected_after").GetProperty(valueProperty).GetInt32());
        if (operation == "vm.disk-resize")
        {
            Assert.Equal(@"D:\PureCVisor\lab-vm\lab-vm.vhdx", reconciliation.GetProperty("expected_after").GetProperty("disk_path").GetString());
        }
    }

    [Theory]
    [InlineData("vm.set-memory", "memory_mb", 8192, 4096, "vm-id", 8192, 2, 64, 200, "postcondition-confirmed")]
    [InlineData("vm.set-memory", "memory_mb", 8192, 4096, "vm-id", 4096, 2, 64, 409, "not-applied")]
    [InlineData("vm.set-memory", "memory_mb", 8192, 4096, "vm-id", 6144, 2, 64, 409, "incomplete-resource-value")]
    [InlineData("vm.set-vcpu", "cpu", 4, 2, "vm-id", 4096, 4, 64, 200, "postcondition-confirmed")]
    [InlineData("vm.set-vcpu", "cpu", 4, 2, "vm-id", 4096, 2, 64, 409, "not-applied")]
    [InlineData("vm.disk-resize", "disk_gb", 128, 64, "vm-id", 4096, 2, 128, 200, "postcondition-confirmed")]
    [InlineData("vm.disk-resize", "disk_gb", 128, 64, "vm-id", 4096, 2, 64, 409, "not-applied")]
    [InlineData("vm.set-vcpu", "cpu", 4, 2, "other-vm-id", 4096, 4, 64, 409, "identity-mismatch")]
    public void VmResourceReconcileJudgesReadbackValueWithoutCallingResourceProvider(
        string operation,
        string valueProperty,
        int requested,
        int before,
        string observedId,
        int observedMemory,
        int observedCpu,
        int observedDisk,
        int expectedStatusCode,
        string expectedClassification)
    {
        using var store = new ResourceJobStore(operation, valueProperty, requested, before, captured: true);
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = ResourceVmList(observedId, observedMemory, observedCpu, observedDisk),
                ["vm.disk.inspect"] = DiskInspectResponse(observedDisk)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-resource/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(observedId == "vm-id" ? ExpectedResourceCalls(operation) : ["vm.list"], nativeCalls);
        Assert.Contains(expectedClassification, response.Body, StringComparison.Ordinal);
        if (expectedStatusCode == 200)
        {
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("succeeded", data.GetProperty("status").GetString());
            Assert.Equal(operation, data.GetProperty("result").GetProperty("operation").GetString());
        }
        else
        {
            Assert.Contains("PCV_JOB_RECONCILIATION_REQUIRED", response.Body, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void VmResourceReconcileRequiresCapturedBaseline()
    {
        using var store = new ResourceJobStore("vm.set-memory", "memory_mb", 8192, 4096, captured: false);
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = ResourceVmList("vm-id", 8192, 2, 64)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-resource/reconcile"));

        Assert.Equal(409, response.StatusCode);
        Assert.Contains("baseline-unavailable", response.Body, StringComparison.Ordinal);
    }

    private const long ResourceGiB = 1024L * 1024 * 1024;

    private static string[] ExpectedResourceCalls(string operation) =>
        operation == "vm.disk-resize" ? ["vm.list", "vm.disk.inspect"] : ["vm.list"];

    private static string DiskInspectResponse(int gb)
    {
        return "{\"ok\":true,\"operation\":\"vm.disk.inspect\",\"data\":{\"name\":\"lab-vm\",\"max_internal_size_bytes\":" +
            (gb * ResourceGiB) + "},\"error\":null}";
    }

    private static string ResourceVmItem(string id, int memory, int cpu, int disk)
    {
        return "{\"id\":\"" + id + "\",\"name\":\"lab-vm\",\"platform\":\"hyperv\",\"guest_family\":\"windows\",\"state\":\"off\"," +
            "\"cpu\":{\"count\":" + cpu + "},\"memory\":{\"startup_mb\":" + memory + "},\"generation\":2,\"managed_by_purecvisor\":true," +
            "\"storage\":[{\"kind\":\"iso\",\"path\":\"D:\\\\iso\\\\setup.iso\",\"size_gb\":null,\"attached\":true}," +
            "{\"kind\":\"vhdx\",\"path\":\"" + ResourceDiskPath + "\",\"size_gb\":" + disk + ",\"attached\":true}]}";
    }

    private static string ResourceVmList(string id, int memory, int cpu, int disk)
    {
        return "{\"ok\":true,\"operation\":\"vm.list\",\"data\":[" + ResourceVmItem(id, memory, cpu, disk) + "],\"error\":null}";
    }

    private sealed class ResourceJobStore : IDisposable
    {
        public ResourceJobStore(string operation, string valueProperty, int requested, int before, bool captured)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pcv-dotnet-api-resource-" + Guid.NewGuid().ToString("N") + ".json");
            var beforeVm = operation switch
            {
                "vm.set-memory" => ResourceVmItem("vm-id", before, 2, 64),
                "vm.set-vcpu" => ResourceVmItem("vm-id", 4096, before, 64),
                _ => ResourceVmItem("vm-id", 4096, 2, before)
            };
            var diskPath = operation == "vm.disk-resize" ? ", \"disk_path\": \"" + ResourceDiskPath + "\"" : string.Empty;
            File.WriteAllText(Path, $$"""
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-resource",
                  "operation": "{{operation}}",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "{{valueProperty}}": {{requested}},
                    "reconciliation": {
                      "schema": "pcv-vm-resource-reconciliation/v1",
                      "operation": "{{operation}}",
                      "capture_status": "{{(captured ? "captured" : "unavailable")}}",
                      "before": {{(captured ? beforeVm : "null")}},
                      "before_fingerprint": {{(captured ? "{ \"id\": \"vm-id\", \"platform\": \"hyperv\", \"guest_family\": \"windows\", \"generation\": 2, \"managed_by_purecvisor\": true }" : "null")}},
                      "before_value": {{(operation == "vm.disk-resize" ? before * ResourceGiB : before)}},
                      "expected_after": { "name": "lab-vm", "{{valueProperty}}": {{requested}}{{diskPath}} }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-resource",
                  "correlation_id": "corr-resource",
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
