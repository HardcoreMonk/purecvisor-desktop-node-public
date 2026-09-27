using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiVmNetworkConnectMutationTests
{
    [Fact]
    public void ConnectQueuesJobAndInvokesNativeConnect()
    {
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/network",
            JsonSerializer.Serialize(new { @switch = "pcv-lab-internal" }),
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.Equal(["vm.list", "network.inventory"], nativeCalls);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            Assert.Equal("queued", queuedDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.Equal("vm.network.connect", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Equal(["vm.list", "network.inventory", "vm.network.connect"], nativeCalls);
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("connect", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
        Assert.Equal("pcv-lab-internal", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("switch").GetString());
    }

    [Fact]
    public void ConnectRejectsUnmanagedWithoutQueuing()
    {
        var processor = CreateProcessor([], managed: false);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/network",
            JsonSerializer.Serialize(new { @switch = "pcv-lab-internal" }),
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(NetworkChangeProblemCodes.NotManaged, response.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    [Fact]
    public void ConnectRejectsMissingSwitchWithoutQueuing()
    {
        var processor = CreateProcessor([], switchName: "pcv-other");
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/network",
            JsonSerializer.Serialize(new { @switch = "pcv-lab-internal" }),
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(NetworkChangeProblemCodes.SwitchNotFound, response.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        List<string> nativeCalls,
        bool managed = true,
        string switchName = "pcv-lab-internal")
    {
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":0},"managed_by_purecvisor":{{managed.ToString().ToLowerInvariant()}},"security_features_present":false}],"error":null}
        """;
        var inventoryJson = $$"""
        {"ok":true,"operation":"network.inventory","data":{"source":"native-csharp","mutating":false,"switches":[{"name":{{JsonSerializer.Serialize(switchName)}}}]},"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNetworkConnectNativeAdapter(nativeCalls, vmJson, inventoryJson));
    }

    private sealed class RecordingNetworkConnectNativeAdapter(
        List<string> calls,
        string vmListJson,
        string inventoryJson) : IDesktopNodeHyperVNativeAdapter
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

            if (string.Equals(operation, "network.inventory", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(inventoryJson);
                return true;
            }

            if (string.Equals(operation, "vm.network.connect", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(
                    """{"ok":true,"operation":"vm.network.connect","data":{"action":"connect","vm_name":"lab-vm","switch":"pcv-lab-internal"},"error":null}""");
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Unexpected native operation in network connect tests.",
                false);
            return false;
        }
    }
}
