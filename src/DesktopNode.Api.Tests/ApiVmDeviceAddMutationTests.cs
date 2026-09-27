using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiVmDeviceAddMutationTests
{
    [Fact]
    public void NicAddQueuesJobAndInvokesNativeAdd()
    {
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/devices",
            JsonSerializer.Serialize(new { device = "nic", @switch = "Default Switch" }),
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.Equal(["vm.list", "network.inventory"], nativeCalls);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            Assert.Equal("queued", queuedDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.Equal("vm.nic.add", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Equal(["vm.list", "network.inventory", "vm.nic.add"], nativeCalls);
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("nic-add", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void DvdAddQueuesEmptyDriveWithoutIso()
    {
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(nativeCalls);
        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/devices",
            JsonSerializer.Serialize(new { device = "dvd" }),
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        using var queuedDocument = JsonDocument.Parse(queued.Body);
        Assert.Equal("vm.dvd.add", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Equal("dvd-add", tick.Job!.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void DvdAddRejectsIsoWithoutQueuing()
    {
        var processor = CreateProcessor([]);
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/devices",
            JsonSerializer.Serialize(new { device = "dvd", iso_path = @"D:\isos\lab.iso" }),
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(VmDeviceAddProblemCodes.IsoForbidden, response.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    [Fact]
    public void NicAddRejectsNatAndAThirdNicWithoutQueuing()
    {
        var nat = CreateProcessor([]);
        var natResponse = nat.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/devices",
            JsonSerializer.Serialize(new { device = "nic", @switch = "Default Switch", nat = true }),
            ServiceBearerAccepted: true));
        Assert.Equal(400, natResponse.StatusCode);
        Assert.Contains(VmDeviceAddProblemCodes.NatForbidden, natResponse.Body, StringComparison.Ordinal);

        var limited = CreateProcessor([], nicCount: 2);
        var limitedResponse = limited.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/devices",
            JsonSerializer.Serialize(new { device = "nic", @switch = "Default Switch" }),
            ServiceBearerAccepted: true));
        Assert.Equal(400, limitedResponse.StatusCode);
        Assert.Contains(VmDeviceAddProblemCodes.Limit, limitedResponse.Body, StringComparison.Ordinal);
        Assert.False(limited.ProcessOneQueuedJob().Processed);
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        List<string> nativeCalls,
        int nicCount = 1,
        bool dvdPresent = false)
    {
        var nics = string.Join(",", Enumerable.Range(0, nicCount).Select(index =>
            $$"""{"name":"nic-{{index}}","switch":"Default Switch"}"""));
        var storage = dvdPresent
            ? """[{"type":"dvd","path":""},{"type":"vhdx","path":"D:\\\\lab.vhdx"}]"""
            : """[{"type":"vhdx","path":"D:\\\\lab.vhdx"}]""";
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":0},"managed_by_purecvisor":true,"network":[{{nics}}],"storage":{{storage}}}],"error":null}
        """;
        var inventoryJson = """
        {"ok":true,"operation":"network.inventory","data":{"source":"native-csharp","mutating":false,"switches":[{"name":"Default Switch"}]},"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingDeviceAddNativeAdapter(nativeCalls, vmJson, inventoryJson));
    }

    private sealed class RecordingDeviceAddNativeAdapter(
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

            if (string.Equals(operation, "vm.nic.add", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(
                    """{"ok":true,"operation":"vm.nic.add","data":{"action":"nic-add","vm_name":"lab-vm","device":"nic","switch":"Default Switch"},"error":null}""");
                return true;
            }

            if (string.Equals(operation, "vm.dvd.add", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(
                    """{"ok":true,"operation":"vm.dvd.add","data":{"action":"dvd-add","vm_name":"lab-vm","device":"dvd","switch":null},"error":null}""");
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Unexpected native operation in device add tests.",
                false);
            return false;
        }
    }
}
