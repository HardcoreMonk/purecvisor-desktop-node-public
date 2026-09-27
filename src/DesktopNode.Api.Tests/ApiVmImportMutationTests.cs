using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiVmImportMutationTests
{
    [Fact]
    public void ImportQueuesJobAndInvokesNativeImportWithManagedMarker()
    {
        using var root = new TempExportRoot();
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root, nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm-restored",
                confirm_name = "lab-vm-restored",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "hyperv-export",
                has_vmcx = true
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            Assert.Equal("queued", queuedDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.Equal("vm.import", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Equal(["vm.list", "vm.import"], nativeCalls);
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        var result = tick.Job.Value.GetProperty("result").GetProperty("data");
        Assert.Equal("import", result.GetProperty("action").GetString());
        Assert.True(result.GetProperty("apply_managed_marker").GetBoolean());
        Assert.True(result.GetProperty("generate_new_id").GetBoolean());
        Assert.Equal("lab-vm-restored", result.GetProperty("vm_name").GetString());
    }

    [Fact]
    public void ImportRejectsExistingTargetAndOvfWithoutQueuing()
    {
        using var root = new TempExportRoot();
        var processor = CreateProcessor(root, []);
        var existing = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm",
                confirm_name = "lab-vm",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "hyperv-export",
                has_vmcx = true
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(409, existing.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.AlreadyExists, existing.Body, StringComparison.Ordinal);

        var ovf = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm-restored",
                confirm_name = "lab-vm-restored",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "ovf",
                has_vmcx = true
            }),
            ServiceBearerAccepted: true));
        Assert.Equal(400, ovf.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.OvfForbidden, ovf.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    [Fact]
    public void ImportRejectsInPlaceIdentityWithoutQueuing()
    {
        using var root = new TempExportRoot();
        var processor = CreateProcessor(root, []);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/import",
            JsonSerializer.Serialize(new
            {
                name = "lab-vm-restored",
                confirm_name = "lab-vm-restored",
                directory = Path.Combine(root.Directory, "lab-vm"),
                allowed_root = root.Directory,
                package_kind = "hyperv-export",
                has_vmcx = true,
                generate_new_id = false
            }),
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(VmExportImportProblemCodes.InPlaceForbidden, response.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(TempExportRoot root, List<string> nativeCalls)
    {
        var vmJson = """
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":0},"managed_by_purecvisor":true,"security_features_present":false}],"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingImportNativeAdapter(nativeCalls, vmJson, Path.Combine(root.Directory, "lab-vm")));
    }

    private sealed class RecordingImportNativeAdapter(List<string> calls, string vmListJson, string importDirectory) : IDesktopNodeHyperVNativeAdapter
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

            if (string.Equals(operation, "vm.import", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(
                    $$"""{"ok":true,"operation":"vm.import","data":{"action":"import","apply_managed_marker":true,"directory":{{JsonSerializer.Serialize(importDirectory)}},"generate_new_id":true,"package_kind":"hyperv-export","vm_name":"lab-vm-restored"},"error":null}""");
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Unexpected native operation in import tests.",
                false);
            return false;
        }
    }

    private sealed class TempExportRoot : IDisposable
    {
        public string Directory { get; }

        public TempExportRoot()
        {
            Directory = Path.Combine(Path.GetTempPath(), "pcv-import-queue-" + Guid.NewGuid().ToString("N"));
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
