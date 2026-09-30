using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    private const string PowerStateLabVmFingerprint = """{ "platform": "hyperv", "guest_family": "windows", "cpu_count": 2, "startup_memory_mb": 4096, "generation": 2, "managed_by_purecvisor": true }""";

    [Theory]
    [InlineData("start", "vm.start", "off", "running")]
    [InlineData("poweroff", "vm.poweroff", "running", "off")]
    [InlineData("pause", "vm.pause", "running", "paused")]
    [InlineData("resume", "vm.resume", "paused", "running")]
    [InlineData("save", "vm.save", "running", "saved")]
    [InlineData("resume-saved", "vm.resume-saved", "saved", "running")]
    public void VmPowerStateQueueCapturesReadbackBaselineWithoutMutatingProvider(
        string routeAction,
        string operation,
        string beforeState,
        string expectedState)
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = PowerStateVmList(beforeState)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", $"/api/v1/vms/lab-vm/{routeAction}"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(operation, data.GetProperty("operation").GetString());
        var reconciliation = data.GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-vm-power-state-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal(operation, reconciliation.GetProperty("operation").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal(beforeState, reconciliation.GetProperty("before").GetProperty("state").GetString());
        Assert.Equal(expectedState, reconciliation.GetProperty("expected_after").GetProperty("state").GetString());
        Assert.Equal("lab-vm", data.GetProperty("params").GetProperty("name").GetString());
    }

    [Fact]
    public void VmPowerStateQueueRecordsUnavailableBaselineWhenVmIsMissing()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/lab-vm/start"));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("unavailable", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("PCV_VM_NOT_FOUND", reconciliation.GetProperty("capture_error_code").GetString());
    }

    [Theory]
    [InlineData("vm.start", "off", "running", 200, "postcondition-confirmed")]
    [InlineData("vm.poweroff", "running", "off", 200, "postcondition-confirmed")]
    [InlineData("vm.start", "off", "off", 409, "not-applied")]
    [InlineData("vm.poweroff", "running", "running", 409, "not-applied")]
    [InlineData("vm.start", "off", "saved", 409, "incomplete-power-state")]
    [InlineData("vm.pause", "running", "paused", 200, "postcondition-confirmed")]
    [InlineData("vm.pause", "running", "running", 409, "not-applied")]
    [InlineData("vm.resume", "paused", "running", 200, "postcondition-confirmed")]
    [InlineData("vm.resume", "paused", "paused", 409, "not-applied")]
    [InlineData("vm.save", "running", "saved", 200, "postcondition-confirmed")]
    [InlineData("vm.save", "running", "saving", 409, "incomplete-power-state")]
    [InlineData("vm.resume-saved", "saved", "running", 200, "postcondition-confirmed")]
    [InlineData("vm.resume-saved", "saved", "saved", 409, "not-applied")]
    public void VmPowerStateReconcileJudgesObservedStateWithoutCallingPowerProvider(
        string operation,
        string beforeState,
        string observedState,
        int expectedStatusCode,
        string expectedClassification)
    {
        using var store = new PowerStateJobStore(operation, beforeState, PowerStateLabVmFingerprint);
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = PowerStateVmList(observedState)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-power-state/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
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
    public void VmPowerStateReconcileRejectsIdentityMismatchAndDuplicates()
    {
        using var store = new PowerStateJobStore("vm.start", "off", PowerStateLabVmFingerprint);
        var mismatched = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = PowerStateVmList("running").Replace("\"generation\":2", "\"generation\":1", StringComparison.Ordinal)
            }));

        var mismatch = mismatched.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-power-state/reconcile"));

        Assert.Equal(409, mismatch.StatusCode);
        Assert.Contains("identity-mismatch", mismatch.Body, StringComparison.Ordinal);

        using var duplicateStore = new PowerStateJobStore("vm.start", "off", PowerStateLabVmFingerprint);
        var duplicateVm = """{"id":"vm-id-2","name":"LAB-VM","platform":"hyperv","guest_family":"windows","state":"running","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"managed_by_purecvisor":true}""";
        var duplicated = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: duplicateStore.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = PowerStateVmList("running").Replace("}],\"error\"", "}," + duplicateVm + "],\"error\"", StringComparison.Ordinal)
            }));

        var duplicate = duplicated.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-power-state/reconcile"));

        Assert.Equal(409, duplicate.StatusCode);
        Assert.Contains("ambiguous-duplicate-names", duplicate.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void VmPowerStateReconcileRequiresBaselineAndReadback()
    {
        using var missingBaseline = new PowerStateJobStore("vm.poweroff", "running", PowerStateLabVmFingerprint, captured: false);
        var baselineProcessor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: missingBaseline.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = PowerStateVmList("off")
            }));

        var baseline = baselineProcessor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-power-state/reconcile"));

        Assert.Equal(409, baseline.StatusCode);
        Assert.Contains("baseline-unavailable", baseline.Body, StringComparison.Ordinal);

        using var readbackStore = new PowerStateJobStore("vm.poweroff", "running", PowerStateLabVmFingerprint);
        var readbackProcessor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: readbackStore.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = """{"ok":false,"operation":"vm.list","data":null,"error":{"code":"PCV_HYPERV_UNAVAILABLE","message":"Unavailable.","detail":"","retryable":true,"recommended_action":"Retry."}}"""
            }));

        var readback = readbackProcessor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-power-state/reconcile"));

        Assert.Equal(409, readback.StatusCode);
        Assert.Contains("readback-unavailable", readback.Body, StringComparison.Ordinal);
    }

    private static string PowerStateVmList(string state)
    {
        return "{\"ok\":true,\"operation\":\"vm.list\",\"data\":[{\"id\":\"vm-id\",\"name\":\"lab-vm\",\"platform\":\"hyperv\",\"guest_family\":\"windows\",\"state\":\"" +
            state +
            "\",\"cpu\":{\"count\":2},\"memory\":{\"startup_mb\":4096},\"generation\":2,\"managed_by_purecvisor\":true}],\"error\":null}";
    }

    private sealed class PowerStateJobStore : IDisposable
    {
        public PowerStateJobStore(string operation, string beforeState, string fingerprint, bool captured = true)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pcv-dotnet-api-power-state-" + Guid.NewGuid().ToString("N") + ".json");
            var expectedState = operation switch
            {
                "vm.poweroff" => "off",
                "vm.pause" => "paused",
                "vm.save" => "saved",
                _ => "running"
            };
            var captureStatus = captured ? "captured" : "unavailable";
            var before = captured
                ? "{ \"id\": \"vm-id\", \"name\": \"lab-vm\", \"platform\": \"hyperv\", \"guest_family\": \"windows\", \"state\": \"" + beforeState + "\", \"cpu\": { \"count\": 2 }, \"memory\": { \"startup_mb\": 4096 }, \"generation\": 2, \"managed_by_purecvisor\": true }"
                : "null";
            File.WriteAllText(Path, $$"""
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-power-state",
                  "operation": "{{operation}}",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "reconciliation": {
                      "schema": "pcv-vm-power-state-reconciliation/v1",
                      "operation": "{{operation}}",
                      "capture_status": "{{captureStatus}}",
                      "before": {{before}},
                      "before_fingerprint": {{(captured ? fingerprint : "null")}},
                      "expected_after": { "name": "lab-vm", "state": "{{expectedState}}" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-power-state",
                  "correlation_id": "corr-power-state",
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
