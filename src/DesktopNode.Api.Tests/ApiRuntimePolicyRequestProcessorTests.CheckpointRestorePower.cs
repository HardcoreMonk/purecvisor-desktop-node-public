using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    [Theory]
    [InlineData("running")]
    [InlineData("Running")]
    [InlineData("paused")]
    [InlineData("stopping")]
    [InlineData("")]
    public void CheckpointRestoreRefusesBeforeQueueWhenVmIsNotOffOrSaved(string state)
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = VmListWithState("lab-vm", state),
                ["checkpoint.list"] = CheckpointRestoreListJson()
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/requested/restore"));

        Assert.Equal(409, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("checkpoint.restore", document.RootElement.GetProperty("operation").GetString());
        var error = document.RootElement.GetProperty("error");
        Assert.Equal("PCV_CHECKPOINT_RESTORE_VM_RUNNING", error.GetProperty("code").GetString());
        Assert.Contains("Power off the VM", error.GetProperty("detail").GetString(), StringComparison.Ordinal);
        Assert.False(error.GetProperty("retryable").GetBoolean());
    }

    [Theory]
    [InlineData("stopped")]
    [InlineData("Off")]
    [InlineData("saved")]
    [InlineData("Saved")]
    public void CheckpointRestoreQueuesWhenVmIsOffOrSaved(string state)
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = VmListWithState("lab-vm", state),
                ["checkpoint.list"] = CheckpointRestoreListJson()
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/requested/restore"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list", "checkpoint.list"], nativeCalls);
        Assert.DoesNotContain("checkpoint.restore", nativeCalls);
    }

    private static string VmListWithState(string name, string state)
    {
        return "{\"ok\":true,\"operation\":\"vm.list\",\"data\":[{\"name\":\"" + name + "\",\"state\":\"" + state + "\"}],\"error\":null}";
    }

    private static string CheckpointRestoreListJson()
    {
        return """
            {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":false}],"error":null}
            """;
    }
}
