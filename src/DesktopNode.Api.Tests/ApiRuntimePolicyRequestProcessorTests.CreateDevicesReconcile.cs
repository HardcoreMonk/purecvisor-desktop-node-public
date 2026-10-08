using System.Text.Json;

namespace DesktopNode.Api.Tests;

// Design pcv-vm-create-reconcile-devices-v1 end to end: a managed VM of the right generation without the create devices
// keeps the interrupted vm.create failed as target-fingerprint-mismatch.
public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    [Fact]
    public void ReconcileKeepsAManagedVmWithoutDevicesFailed()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-vm-create-devices-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-vm-create-devices",
                  "operation": "vm.create",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "iso_path": "D:\\isos\\lab.iso",
                    "generation": 2,
                    "reconciliation": {
                      "schema": "pcv-vm-create-reconciliation/v1",
                      "capture_status": "captured",
                      "before": null,
                      "expected_before": { "state": "absent", "name": "lab-vm" },
                      "expected_after": { "state": "present", "name": "lab-vm", "generation": 2, "managed_by_purecvisor": true }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-vm-create-devices",
                  "correlation_id": "corr-vm-create-devices",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-10-08T00:00:00.0000000Z",
                  "updated_at": "2026-10-08T00:00:01.0000000Z"
                }
              ],
              "queue": []
            }
            """);
            var nativeCalls = new List<string>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath,
                nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
                {
                    ["vm.list"] = """
                    {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","state":"off","generation":2,"managed_by_purecvisor":true,"storage":[],"dvd_media":[],"network":[]}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-vm-create-devices/reconcile",
                RequestId: "req-reconcile"));

            Assert.Equal(["vm.list"], nativeCalls);
            Assert.Contains("PCV_JOB_RECONCILIATION_REQUIRED", response.Body, StringComparison.Ordinal);
            Assert.Contains("target-fingerprint-mismatch", response.Body, StringComparison.Ordinal);
            Assert.Contains("missing_devices=disk,iso,switch", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("postcondition-confirmed", response.Body, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }
}
