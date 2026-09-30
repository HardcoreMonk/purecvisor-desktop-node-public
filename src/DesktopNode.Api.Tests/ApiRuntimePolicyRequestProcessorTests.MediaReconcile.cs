using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    private const string MediaSetupIso = @"D:\iso\setup.iso";
    private const string MediaToolsIso = @"D:\iso\tools.iso";

    [Fact]
    public void VmAttachQueueCapturesBeforeMediaWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = MediaVmList("vm-id", MediaSetupIso)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/attach",
            JsonSerializer.Serialize(new { iso_path = MediaToolsIso })));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var parameters = document.RootElement.GetProperty("data").GetProperty("params");
        Assert.Equal(MediaToolsIso, parameters.GetProperty("iso_path").GetString());
        var reconciliation = parameters.GetProperty("reconciliation");
        Assert.Equal("pcv-vm-media-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Single(reconciliation.GetProperty("before_media").EnumerateArray());
    }

    [Fact]
    public void VmMediaQueueRecordsUnavailableBaselineWithoutDvdMediaReadback()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = MediaVmList("vm-id", null)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/lab-vm/eject"));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("unavailable", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("PCV_VM_MEDIA_READBACK_UNAVAILABLE", reconciliation.GetProperty("capture_error_code").GetString());
    }

    [Theory]
    [InlineData("vm.attach", new[] { MediaSetupIso }, "vm-id", new[] { MediaSetupIso, MediaToolsIso }, 200, "postcondition-confirmed")]
    [InlineData("vm.attach", new[] { MediaSetupIso }, "vm-id", new[] { MediaSetupIso }, 409, "not-applied")]
    [InlineData("vm.attach", new[] { MediaSetupIso }, "vm-id", new[] { MediaToolsIso }, 200, "postcondition-confirmed")]
    [InlineData("vm.attach", new[] { MediaToolsIso }, "vm-id", new[] { MediaToolsIso }, 409, "not-applied")]
    [InlineData("vm.attach", new[] { MediaSetupIso }, "other-vm-id", new[] { MediaToolsIso }, 409, "identity-mismatch")]
    [InlineData("vm.eject", new[] { MediaSetupIso }, "vm-id", new string[0], 200, "postcondition-confirmed")]
    [InlineData("vm.eject", new[] { MediaSetupIso }, "vm-id", new[] { MediaSetupIso }, 409, "not-applied")]
    [InlineData("vm.eject", new[] { MediaSetupIso, MediaToolsIso }, "vm-id", new[] { MediaSetupIso, @"D:\iso\other.iso" }, 409, "ambiguous-media-state")]
    public void VmMediaReconcileJudgesDvdMediaChange(
        string operation,
        string[] beforeMedia,
        string observedId,
        string[] observedMedia,
        int expectedStatusCode,
        string expectedClassification)
    {
        using var store = new MediaJobStore(operation, beforeMedia);
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = MediaVmList(observedId, observedMedia)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-media/reconcile"));

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        Assert.Contains(expectedClassification, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void VmMediaReconcileRequiresDvdMediaReadback()
    {
        using var store = new MediaJobStore("vm.eject", [MediaSetupIso]);
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            jobStorePath: store.Path,
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["vm.list"] = MediaVmList("vm-id", null)
            }));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-media/reconcile"));

        Assert.Equal(409, response.StatusCode);
        Assert.Contains("readback-value-unavailable", response.Body, StringComparison.Ordinal);
    }

    private static object MediaVmItem(string id, params string[]? media)
    {
        var vm = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["name"] = "lab-vm",
            ["platform"] = "hyperv",
            ["guest_family"] = "windows",
            ["state"] = "off",
            ["generation"] = 2,
            ["managed_by_purecvisor"] = true
        };
        if (media is not null)
        {
            vm["dvd_media"] = media.Select(path => new Dictionary<string, object?> { ["path"] = path }).ToArray();
        }

        return vm;
    }

    private static string MediaVmList(string id, params string[]? media)
    {
        return JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["operation"] = "vm.list",
            ["data"] = new[] { MediaVmItem(id, media) },
            ["error"] = null
        });
    }

    private sealed class MediaJobStore : IDisposable
    {
        public MediaJobStore(string operation, string[] beforeMedia)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pcv-dotnet-api-media-" + Guid.NewGuid().ToString("N") + ".json");
            var parameters = new Dictionary<string, object?> { ["name"] = "lab-vm" };
            var expectedAfter = new Dictionary<string, object?> { ["name"] = "lab-vm" };
            if (operation == "vm.attach")
            {
                parameters["iso_path"] = MediaToolsIso;
                expectedAfter["iso_path"] = MediaToolsIso;
            }

            parameters["reconciliation"] = new Dictionary<string, object?>
            {
                ["schema"] = "pcv-vm-media-reconciliation/v1",
                ["operation"] = operation,
                ["capture_status"] = "captured",
                ["before"] = MediaVmItem("vm-id", beforeMedia),
                ["before_media"] = beforeMedia.Select(path => System.IO.Path.GetFullPath(path).ToUpperInvariant()).ToArray(),
                ["expected_after"] = expectedAfter
            };
            var store = new Dictionary<string, object?>
            {
                ["version"] = 1,
                ["jobs"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["job_id"] = "job-media",
                        ["operation"] = operation,
                        ["status"] = "failed",
                        ["params"] = parameters,
                        ["result"] = null,
                        ["error"] = new Dictionary<string, object?>
                        {
                            ["code"] = "PCV_JOB_INTERRUPTED",
                            ["message"] = "Interrupted.",
                            ["detail"] = "Provider side effect is unresolved.",
                            ["retryable"] = false,
                            ["recommended_action"] = "Reconcile the provider state."
                        },
                        ["retry_of"] = null,
                        ["request_id"] = "req-media",
                        ["correlation_id"] = "corr-media",
                        ["attempt"] = 1,
                        ["canceled_at"] = null,
                        ["created_at"] = "2026-09-30T00:00:00.0000000Z",
                        ["updated_at"] = "2026-09-30T00:00:01.0000000Z"
                    }
                },
                ["queue"] = Array.Empty<string>()
            };
            File.WriteAllText(Path, JsonSerializer.Serialize(store));
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
