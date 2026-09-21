using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiCheckpointSchedulePreviewTests
{
    [Fact]
    public void PreviewAcceptsManagedScheduleWithoutCreatingCheckpoint()
    {
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(nativeCalls, managed: true, templateLock: false);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule/preview",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("checkpoint.schedule.preview", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("preview", data.GetProperty("action").GetString());
        Assert.True(data.GetProperty("dry_run").GetBoolean());
        Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
        Assert.Equal("lab-vm", data.GetProperty("vm_name").GetString());
        Assert.Equal(1440, data.GetProperty("interval_minutes").GetInt32());
        Assert.Equal(8, data.GetProperty("retention_max").GetInt32());
        Assert.Equal(CheckpointSchedulePolicy.Schema, data.GetProperty("schema").GetString());
    }

    [Fact]
    public void PreviewRejectsUnmanagedVm()
    {
        var processor = CreateProcessor([], managed: false, templateLock: false);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule/preview",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(CheckpointScheduleProblemCodes.NotManaged, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewRejectsTemplateLockedVm()
    {
        var processor = CreateProcessor([], managed: true, templateLock: true);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule/preview",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(CheckpointScheduleProblemCodes.TemplateLocked, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewRejectsMissingRetentionAsInfiniteAutoProtect()
    {
        var processor = CreateProcessor([], managed: true, templateLock: false);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule/preview",
            """{"interval_minutes":1440}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(CheckpointScheduleProblemCodes.RetentionRequired, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewRejectsMissingVm()
    {
        var processor = CreateProcessor([], managed: true, templateLock: false, vmName: "other-vm");
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule/preview",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));

        Assert.Equal(404, response.StatusCode);
        Assert.Contains("PCV_VM_NOT_FOUND", response.Body, StringComparison.Ordinal);
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        List<string> nativeCalls,
        bool managed,
        bool templateLock,
        string vmName = "lab-vm")
    {
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"{{vmName}}","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":2},"managed_by_purecvisor":{{managed.ToString().ToLowerInvariant()}},"template_lock":{{templateLock.ToString().ToLowerInvariant()}}}],"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingScheduleNativeAdapter(nativeCalls, vmJson));
    }

    private sealed class RecordingScheduleNativeAdapter(List<string> calls, string vmListJson) : IDesktopNodeHyperVNativeAdapter
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

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Schedule preview must not create a checkpoint.",
                false);
            return false;
        }
    }
}
