using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiVmExportMutationTests
{
    [Fact]
    public void ExportQueuesJobAndInvokesNativeExportWithoutPreviewOnly()
    {
        using var root = new TempExportRoot();
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root, nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/export",
            JsonSerializer.Serialize(new
            {
                confirm_name = "lab-vm",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            Assert.Equal("queued", queuedDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.Equal("vm.export", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Equal(["vm.list", "vm.export"], nativeCalls);
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("export", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void ExportRejectsUnmanagedWithoutQueuing()
    {
        using var root = new TempExportRoot();
        var processor = CreateProcessor(root, [], managed: false);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/export",
            JsonSerializer.Serialize(new
            {
                confirm_name = "lab-vm",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.NotManaged, response.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        TempExportRoot root,
        List<string> nativeCalls,
        bool managed = true)
    {
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":0},"managed_by_purecvisor":{{managed.ToString().ToLowerInvariant()}},"security_features_present":false}],"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingExportNativeAdapter(nativeCalls, vmJson, Path.Combine(root.Directory, "lab-vm")));
    }

    private sealed class RecordingExportNativeAdapter(List<string> calls, string vmListJson, string exportDirectory) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(operation);
            if (string.Equals(operation, "vm.list", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(vmListJson);
                return true;
            }

            if (string.Equals(operation, "vm.export", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(
                    $$"""{"ok":true,"operation":"vm.export","data":{"action":"export","directory":{{JsonSerializer.Serialize(exportDirectory)}},"package_kind":"hyperv-export","vm_name":"lab-vm"},"error":null}""");
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Unexpected native operation in export tests.",
                false);
            return false;
        }
    }

    private sealed class TempExportRoot : IDisposable
    {
        public string Directory { get; }

        public TempExportRoot()
        {
            Directory = Path.Combine(Path.GetTempPath(), "pcv-export-queue-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
        }

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(Directory, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}
