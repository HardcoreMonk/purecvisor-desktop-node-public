using System.Text.Json;
using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVNativeAdapterTests
{
    [Fact]
    public void NativeHostStatusAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHostStatusProvider(new DesktopNodeHyperVHostStatusData(
                Supported: true,
                Reasons: [],
                Windows: new DesktopNodeHyperVHostWindowsInfo("Windows 11 Pro", "23H2", "Pro"),
                Admin: new DesktopNodeHyperVHostAdminInfo(true),
                HyperV: new DesktopNodeHyperVHostHyperVInfo(true, true, true))));

        var handled = adapter.TryInvoke("host.status", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("host.status", result.Operation);
        var data = result.Data!.Value;
        Assert.True(data.GetProperty("supported").GetBoolean());
        Assert.Empty(data.GetProperty("reasons").EnumerateArray());
        Assert.Equal("Windows 11 Pro", data.GetProperty("windows").GetProperty("caption").GetString());
        Assert.Equal("23H2", data.GetProperty("windows").GetProperty("version").GetString());
        Assert.True(data.GetProperty("admin").GetProperty("elevated").GetBoolean());
        Assert.True(data.GetProperty("hyperv").GetProperty("default_switch_present").GetBoolean());
    }

    [Fact]
    public void NativeHostStatusAdapterReturnsStructuredFailureWhenProviderFails()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new ThrowingHostStatusProvider("host status unavailable"));

        var handled = adapter.TryInvoke("host.status", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("host.status", result.Operation);
        Assert.Equal("PCV_HOST_STATUS_FAILED", result.Error!.Code);
        Assert.True(result.Error.Retryable);
        Assert.Contains("host status unavailable", result.Error.Detail);
    }

    [Fact]
    public void NativeHostStatusAdapterReturnsCancellationFailureWhenTokenIsCanceledBeforeReadProvider()
    {
        using var parameters = JsonDocument.Parse("{}");
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHostStatusProvider(new DesktopNodeHyperVHostStatusData(
                Supported: true,
                Reasons: [],
                Windows: new DesktopNodeHyperVHostWindowsInfo("Windows 11 Pro", "23H2", "Pro"),
                Admin: new DesktopNodeHyperVHostAdminInfo(true),
                HyperV: new DesktopNodeHyperVHostHyperVInfo(true, true, true))));

        var handled = adapter.TryInvoke("host.status", parameters.RootElement, cts.Token, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_NATIVE_OPERATION_CANCELED", result.Error!.Code);
        Assert.True(result.Error.Retryable);
    }

    [Fact]
    public void NativeVmListAdapterReturnsStructuredFailureForIncompleteIdentityState()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider(
            [
                new DesktopNodeHyperVVmInfo(
                    Id: "",
                    Name: "alpha",
                    Platform: "hyperv",
                    GuestFamily: "linux",
                    State: "running",
                    Cpu: new DesktopNodeHyperVVmCpuInfo(null),
                    Memory: new DesktopNodeHyperVVmMemoryInfo(null, null, false),
                    Generation: null,
                    Storage: [],
                    Network: [],
                    Checkpoints: new DesktopNodeHyperVVmCheckpointInfo(null),
                    Console: new DesktopNodeHyperVVmConsoleInfo("vmconnect", true),
                    ManagedByPurecvisor: false)
            ]));

        var handled = adapter.TryInvoke("vm.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("vm.list", result.Operation);
        Assert.Equal("PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeVmListAdapterReturnsStructuredFailureForMissingSummaryParityField()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider(
            [
                new DesktopNodeHyperVVmInfo(
                    Id: "alpha",
                    Name: "alpha",
                    Platform: "hyperv",
                    GuestFamily: "linux",
                    State: "running",
                    Cpu: new DesktopNodeHyperVVmCpuInfo(null),
                    Memory: new DesktopNodeHyperVVmMemoryInfo(4096, null, false),
                    Generation: 2,
                    Storage: [],
                    Network: [],
                    Checkpoints: new DesktopNodeHyperVVmCheckpointInfo(0),
                    Console: new DesktopNodeHyperVVmConsoleInfo("vmconnect", true),
                    ManagedByPurecvisor: false)
            ]));

        var handled = adapter.TryInvoke("vm.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("vm.list", result.Operation);
        Assert.Equal("PCV_NATIVE_VM_LIST_SUMMARY_PARITY_INCOMPLETE", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeVmListAdapterReturnsEmptyInventoryWhenProviderReturnsNoRows()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([]));

        var handled = adapter.TryInvoke("vm.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.list", result.Operation);
        Assert.Null(result.Error);
        Assert.Equal(JsonValueKind.Array, result.Data!.Value.ValueKind);
        Assert.Empty(result.Data.Value.EnumerateArray());
    }

    [Fact]
    public void NativeVmListAdapterReturnsStructuredFailureWhenProviderFails()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new ThrowingHyperVVmProvider("Msvm_ComputerSystem unavailable"));

        var handled = adapter.TryInvoke("vm.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("vm.list", result.Operation);
        Assert.Equal("PCV_VM_LIST_FAILED", result.Error!.Code);
        Assert.True(result.Error.Retryable);
        Assert.Contains("Msvm_ComputerSystem unavailable", result.Error.Detail);
    }

    [Theory]
    [InlineData("vm.memory-stats", "memory", "startup_mb")]
    [InlineData("vm.cpu-stats", "cpu", "count")]
    public void NativeVmStatsAdapterMapsVmInventorySummary(string operation, string expectedBucket, string expectedField)
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]));

        var handled = adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal(operation, result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal("running", result.Data.Value.GetProperty("state").GetString());
        Assert.True(result.Data.Value.GetProperty(expectedBucket).TryGetProperty(expectedField, out _));
    }

    [Fact]
    public void NativeVmDiskInspectReportsSizeOnlyForTheVmsOwnDisk()
    {
        var diskPath = @"D:\VMs\alpha\disk0.vhdx";
        var vm = CompleteVm("alpha") with { Storage = [new DesktopNodeHyperVVmDiskInfo("vhdx", diskPath, null, true)] };
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new SizedHyperVVmProvider([vm], 64UL * 1024 * 1024 * 1024));

        using var owned = JsonDocument.Parse(JsonSerializer.Serialize(new { name = "alpha", path = diskPath }));
        Assert.True(adapter.TryInvoke("vm.disk.inspect", owned.RootElement, CancellationToken.None, out var result));
        Assert.True(result.Ok);
        Assert.Equal(64UL * 1024 * 1024 * 1024, result.Data!.Value.GetProperty("max_internal_size_bytes").GetUInt64());

        using var foreign = JsonDocument.Parse("""{"name":"alpha","path":"C:\\Windows\\system32\\config\\SAM"}""");
        Assert.True(adapter.TryInvoke("vm.disk.inspect", foreign.RootElement, CancellationToken.None, out var rejected));
        Assert.Equal("PCV_VM_DISK_NOT_FOUND", rejected.Error!.Code);

        var sizeless = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider([]), new RecordingHyperVVmProvider([vm]));
        Assert.True(sizeless.TryInvoke("vm.disk.inspect", owned.RootElement, CancellationToken.None, out var unavailable));
        Assert.Equal("PCV_VM_DISK_INSPECT_UNAVAILABLE", unavailable.Error!.Code);
    }

    [Fact]
    public void NativeVmConsoleFrameTrimsTheThumbnailAndChecksStateAndSize()
    {
        var running = CompleteVm("alpha") with { State = "running" };
        var frame = new byte[(160 * 120 * 2) + 4];
        frame[0] = 0x1F;
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new FramedHyperVVmProvider([running, CompleteVm("beta") with { State = "stopped" }], frame));

        using var ok = JsonDocument.Parse("""{"name":"alpha","width":160,"height":120}""");
        Assert.True(adapter.TryInvoke("vm.console.frame", ok.RootElement, CancellationToken.None, out var result));
        Assert.True(result.Ok);
        Assert.Equal("rgb565le", result.Data!.Value.GetProperty("format").GetString());
        var bytes = Convert.FromBase64String(result.Data.Value.GetProperty("frame_base64").GetString()!);
        Assert.Equal(160 * 120 * 2, bytes.Length);
        Assert.Equal(0x1F, bytes[0]);

        using var stopped = JsonDocument.Parse("""{"name":"beta"}""");
        Assert.True(adapter.TryInvoke("vm.console.frame", stopped.RootElement, CancellationToken.None, out var notRunning));
        Assert.Equal("PCV_CONSOLE_VM_NOT_RUNNING", notRunning.Error!.Code);

        using var huge = JsonDocument.Parse("""{"name":"alpha","width":4096,"height":120}""");
        Assert.True(adapter.TryInvoke("vm.console.frame", huge.RootElement, CancellationToken.None, out var invalid));
        Assert.Equal("PCV_CONSOLE_FRAME_SIZE_INVALID", invalid.Error!.Code);

        using var missing = JsonDocument.Parse("""{"name":"gamma"}""");
        Assert.True(adapter.TryInvoke("vm.console.frame", missing.RootElement, CancellationToken.None, out var absent));
        Assert.Equal("PCV_VM_NOT_FOUND", absent.Error!.Code);

        var blind = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider([]), new RecordingHyperVVmProvider([running]));
        Assert.True(blind.TryInvoke("vm.console.frame", ok.RootElement, CancellationToken.None, out var unavailable));
        Assert.Equal("PCV_CONSOLE_FRAME_FAILED", unavailable.Error!.Code);
    }

    [Fact]
    public void NativeVmConsoleInputPicksTheKeyboardMethodAndRejectsBadInput()
    {
        var calls = new List<(string Method, object? Argument)>();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new KeyboardHyperVVmProvider([CompleteVm("alpha") with { State = "running" }], calls, 0));

        foreach (var (body, method) in new[]
        {
            ("""{"name":"alpha","kind":"key","action":"type","key_code":13}""", "TypeKey"),
            ("""{"name":"alpha","kind":"key","action":"press","key_code":17}""", "PressKey"),
            ("""{"name":"alpha","kind":"key","action":"release","key_code":17}""", "ReleaseKey"),
            ("""{"name":"alpha","kind":"text","text":"setup"}""", "TypeText"),
            ("""{"name":"alpha","kind":"ctrl-alt-del"}""", "TypeCtrlAltDel")
        })
        {
            using var document = JsonDocument.Parse(body);
            Assert.True(adapter.TryInvoke("vm.console.input", document.RootElement, CancellationToken.None, out var result));
            Assert.True(result.Ok, body);
            Assert.Equal(method, calls[^1].Method);
        }

        Assert.Equal(13u, calls[0].Argument);
        Assert.Equal("setup", calls[3].Argument);
        Assert.Null(calls[4].Argument);

        using var bad = JsonDocument.Parse("""{"name":"alpha","kind":"key","action":"type","key_code":300}""");
        Assert.True(adapter.TryInvoke("vm.console.input", bad.RootElement, CancellationToken.None, out var invalid));
        Assert.Equal("PCV_CONSOLE_INPUT_INVALID", invalid.Error!.Code);

        var failing = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new KeyboardHyperVVmProvider([CompleteVm("alpha") with { State = "running" }], calls, 32775));
        using var enter = JsonDocument.Parse("""{"name":"alpha","kind":"key","action":"type","key_code":13}""");
        Assert.True(failing.TryInvoke("vm.console.input", enter.RootElement, CancellationToken.None, out var refused));
        Assert.Equal("PCV_CONSOLE_INPUT_FAILED", refused.Error!.Code);
        Assert.Contains("32775", refused.Error.Detail, StringComparison.Ordinal);
    }

    private sealed class KeyboardHyperVVmProvider(IReadOnlyList<DesktopNodeHyperVVmInfo> vms, List<(string Method, object? Argument)> calls, uint returnValue) : IDesktopNodeHyperVVmProvider
    {
        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken) => vms;

        public uint? SendConsoleKeyboard(string vmId, string method, object? argument, CancellationToken cancellationToken)
        {
            calls.Add((method, argument));
            return returnValue;
        }
    }

    private sealed class FramedHyperVVmProvider(IReadOnlyList<DesktopNodeHyperVVmInfo> vms, byte[] frame) : IDesktopNodeHyperVVmProvider
    {
        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken) => vms;

        public byte[]? GetConsoleFrame(string vmId, int width, int height, CancellationToken cancellationToken) => frame;
    }

    private sealed class SizedHyperVVmProvider(IReadOnlyList<DesktopNodeHyperVVmInfo> vms, ulong bytes) : IDesktopNodeHyperVVmProvider
    {
        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken) => vms;

        public ulong? GetVirtualDiskMaxInternalSize(string diskPath, CancellationToken cancellationToken) => bytes;
    }

    [Fact]
    public void NativeVmStatsAdapterReturnsNotFoundWhenVmIsAbsent()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"missing"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]));

        var handled = adapter.TryInvoke("vm.memory-stats", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("vm.memory-stats", result.Operation);
        Assert.Equal("PCV_VM_NOT_FOUND", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Theory]
    [InlineData("vm.blkio-get", "storage_qos", "linux_blkio_compatible")]
    [InlineData("vm.bandwidth", "network_qos", "linux_bandwidth_compatible")]
    [InlineData("vm.guest-agent-status", "guest_agent", "qemu_guest_agent")]
    [InlineData("vm.guest-ping", "guest_ping", "guest_heartbeat_verified")]
    public void NativeVmQosAndGuestServiceReadbacksMapVmInventorySummary(
        string operation,
        string expectedBucket,
        string expectedField)
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]));

        var handled = adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal(operation, result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal("running", result.Data.Value.GetProperty("state").GetString());
        Assert.True(result.Data.Value.GetProperty(expectedBucket).TryGetProperty(expectedField, out _));
    }

    [Theory]
    [InlineData("vm.blkio-get", "storage_qos", "vm.qos.storage.set")]
    [InlineData("vm.bandwidth", "network_qos", "vm.qos.network.set")]
    public void NativeVmQosReadbacksReportTheCatalogMutation(string operation, string bucket, string mutation)
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]));

        Assert.True(adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result));

        var qos = result.Data!.Value.GetProperty(bucket);
        Assert.True(DesktopNodeHyperVAdapterDispatchCatalog.TryGetEntry(mutation, out var entry));
        Assert.Equal(DesktopNodeHyperVOperationKind.Mutation, entry.Kind);
        Assert.True(qos.GetProperty("mutation_supported").GetBoolean());
    }

    [Theory]
    [InlineData("vm.qos.storage.preview","""{"name":"alpha","disk":"disk0","maximum_iops":1200,"minimum_iops":100,"request_id":"req-preview"}""", "storage", "target_disk", "disk0", "maximum_iops")]
    [InlineData("vm.qos.network.preview", """{"name":"alpha","adapter":"adapter0","maximum_kbps":2048,"minimum_kbps":256,"request_id":"req-preview"}""", "network", "adapter", "adapter0", "maximum_kbps")]
    public void NativeVmQosPreviewAdapterReturnsDryRunContract(
        string operation,
        string payload,
        string expectedBucket,
        string expectedTargetProperty,
        string expectedTarget,
        string expectedPolicyProperty)
    {
        using var parameters = JsonDocument.Parse(payload);
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider(
            [
                CompleteVm("alpha") with
                {
                    Storage = [new DesktopNodeHyperVVmDiskInfo("vhdx", "D:\\VMs\\alpha\\disk.vhdx", 32, true)],
                    Network = [new DesktopNodeHyperVVmNetworkInfo("Default Switch", "default-switch")]
                }
            ]));

        var handled = adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal(operation, result.Operation);
        var data = result.Data!.Value;
        Assert.Equal("hyperv-qos-mutation-preview.v1", data.GetProperty("contract").GetString());
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.False(data.GetProperty("validation").GetProperty("host_mutation_performed").GetBoolean());
        Assert.Equal(expectedTarget, data.GetProperty(expectedBucket).GetProperty(expectedTargetProperty).GetString());
        Assert.True(data.GetProperty(expectedBucket).GetProperty("proposed_policy").TryGetProperty(expectedPolicyProperty, out _));
    }

    [Theory]
    [InlineData("vm.qos.storage.set", """{"name":"alpha","disk":"disk0","maximum_iops":1200,"minimum_iops":100,"request_id":"req-apply"}""", "disk0", 1200, "storage-qos")]
    [InlineData("vm.qos.network.set", """{"name":"alpha","adapter":"adapter0","maximum_kbps":2048,"minimum_kbps":256,"request_id":"req-apply"}""", "adapter0", 2048, "network-qos")]
    public void NativeVmQosMutationAdapterPassesRequestToResourceProvider(
        string operation,
        string payload,
        string expectedTarget,
        int expectedMaximum,
        string expectedAction)
    {
        using var parameters = JsonDocument.Parse(payload);
        var provider = new RecordingHyperVVmResourceMutationProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHostStatusProvider(new DesktopNodeHyperVHostStatusData(
                Supported: true,
                Reasons: [],
                Windows: new DesktopNodeHyperVHostWindowsInfo("Windows 11", "24H2", "Pro"),
                Admin: new DesktopNodeHyperVHostAdminInfo(true),
                HyperV: new DesktopNodeHyperVHostHyperVInfo(true, true, true))),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider(),
            new RecordingHyperVVmMediaProvider(),
            provider);

        var handled = adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal(operation, provider.LastRequest!.Operation);
        Assert.Equal("alpha", provider.LastRequest.Name);
        Assert.Equal(expectedAction, result.Data!.Value.GetProperty("action").GetString());
        Assert.Equal("hyperv-qos-mutation-apply-evidence.v1", result.Data.Value.GetProperty("evidence").GetProperty("contract").GetString());
        if (operation == "vm.qos.storage.set")
        {
            Assert.Equal(expectedTarget, provider.LastRequest.QosDisk);
            Assert.Equal(expectedMaximum, provider.LastRequest.MaximumIops);
        }
        else
        {
            Assert.Equal(expectedTarget, provider.LastRequest.QosAdapter);
            Assert.Equal(expectedMaximum, provider.LastRequest.MaximumKbps);
        }
    }

    [Fact]
    public void NativeAdapterDispatchesVmAttachWithIsoPath()
    {
        var media = new RecordingHyperVVmMediaProvider();
        var adapter = CreateAdapter(vmMediaProvider: media);
        using var parameters = JsonDocument.Parse(
            """{"name":"lab-vm","iso_path":"D:\\isos\\ubuntu.iso"}""");

        Assert.True(adapter.TryInvoke("vm.attach", parameters.RootElement, CancellationToken.None, out var result));
        Assert.True(result.Ok);
        Assert.Equal("attach", result.Data!.Value.GetProperty("action").GetString());
        Assert.Equal(@"D:\isos\ubuntu.iso", result.Data.Value.GetProperty("iso_path").GetString());
        Assert.Equal("vm.attach", media.LastRequest!.Operation);
        Assert.Equal(@"D:\isos\ubuntu.iso", media.LastRequest.IsoPath);
    }

    [Fact]
    public void NativeCheckpointListAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider(
            [
                new DesktopNodeHyperVCheckpointInfo("before-upgrade", "alpha", "2026-05-03T00:00:00.0000000Z", InstanceId: "snap-hidden")
            ]));

        var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("checkpoint.list", result.Operation);
        var checkpoint = result.Data!.Value[0];
        Assert.Equal("before-upgrade", checkpoint.GetProperty("name").GetString());
        Assert.Equal("alpha", checkpoint.GetProperty("vm_name").GetString());
        Assert.Equal("2026-05-03T00:00:00.0000000Z", checkpoint.GetProperty("created_at").GetString());
        Assert.True(checkpoint.TryGetProperty("is_current", out var isCurrent));
        Assert.Equal(JsonValueKind.Null, isCurrent.ValueKind);
        Assert.False(checkpoint.TryGetProperty("instance_id", out _));
        Assert.False(checkpoint.TryGetProperty("InstanceId", out _));
    }

    [Fact]
    public void CheckpointListMarksSingleCurrentSnapshot()
    {
        var marked = DesktopNodeHyperVWmiCheckpointProvider.MarkCurrent(
            [
                new DesktopNodeHyperVCheckpointInfo("before", "lab-vm", "2026-08-01T00:00:00Z", InstanceId: "snap-1"),
                new DesktopNodeHyperVCheckpointInfo("after", "lab-vm", "2026-08-02T00:00:00Z", InstanceId: "snap-2")
            ],
            currentInstanceId: "snap-2");

        Assert.False(marked[0].IsCurrent);
        Assert.True(marked[1].IsCurrent);
    }

    [Fact]
    public void CheckpointListMarksUnknownCurrentAsNull()
    {
        DesktopNodeHyperVCheckpointInfo[] rows =
        [
            new DesktopNodeHyperVCheckpointInfo("before", "lab-vm", "2026-08-01T00:00:00Z", InstanceId: "snap-1"),
            new DesktopNodeHyperVCheckpointInfo("after", "lab-vm", "2026-08-02T00:00:00Z", InstanceId: "snap-2")
        ];

        foreach (var currentInstanceId in new string?[] { null, " ", "snap-missing" })
        {
            var marked = DesktopNodeHyperVWmiCheckpointProvider.MarkCurrent(rows, currentInstanceId);
            Assert.Null(marked[0].IsCurrent);
            Assert.Null(marked[1].IsCurrent);
        }
    }

    [Fact]
    public void CheckpointListMarksAmbiguousCurrentAsNull()
    {
        var marked = DesktopNodeHyperVWmiCheckpointProvider.MarkCurrent(
            [
                new DesktopNodeHyperVCheckpointInfo("before", "lab-vm", "2026-08-01T00:00:00Z", InstanceId: "snap-dup"),
                new DesktopNodeHyperVCheckpointInfo("after", "lab-vm", "2026-08-02T00:00:00Z", InstanceId: "snap-dup")
            ],
            currentInstanceId: "snap-dup");

        Assert.Null(marked[0].IsCurrent);
        Assert.Null(marked[1].IsCurrent);
    }

    [Fact]
    public void NativeCheckpointListAdapterAllowsTrueEmptyCheckpointList()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]));

        var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Empty(result.Data!.Value.EnumerateArray());
    }

    [Fact]
    public void NativeCheckpointListAdapterReturnsNotFoundWhenVmInventoryIsAuthoritative()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"beta"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]));

        var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("checkpoint.list", result.Operation);
        Assert.Equal("PCV_VM_NOT_FOUND", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeCheckpointListAdapterReturnsStructuredFailureForEmptyVmInventory()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([]),
            new RecordingHyperVCheckpointProvider([]));

        var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("checkpoint.list", result.Operation);
        Assert.Equal("PCV_NATIVE_CHECKPOINT_LIST_VM_INVENTORY_EMPTY", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeCheckpointListAdapterReturnsStructuredFailureForIncompleteCheckpointParity()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider(
            [
                new DesktopNodeHyperVCheckpointInfo("", "alpha", null)
            ]));

        var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("checkpoint.list", result.Operation);
        Assert.Equal("PCV_NATIVE_CHECKPOINT_LIST_PARITY_INCOMPLETE", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeCheckpointListAdapterReturnsStructuredFailureWhenProviderFails()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new ThrowingHyperVCheckpointProvider("snapshot provider unavailable"));

        var handled = adapter.TryInvoke("checkpoint.list", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("checkpoint.list", result.Operation);
        Assert.Equal("PCV_CHECKPOINT_FAILED", result.Error!.Code);
        Assert.True(result.Error.Retryable);
        Assert.Contains("snapshot provider unavailable", result.Error.Detail);
    }

    [Fact]
    public void NativeNetworkInventoryAdapterMapsSwitchProviderResult()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider(
        [
            new DesktopNodeHyperVSwitchInfo("Default Switch", "internal", true, true, null),
            new DesktopNodeHyperVSwitchInfo("lab-external", "external", false, true, "Intel(R) Ethernet")
        ]));

        var handled = adapter.TryInvoke("network.inventory", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("network.inventory", result.Operation);
        var data = result.Data!.Value;
        Assert.Equal("hyperv", data.GetProperty("source").GetString());
        Assert.False(data.GetProperty("mutating").GetBoolean());
        Assert.Equal("Default Switch", data.GetProperty("switches")[0].GetProperty("name").GetString());
        Assert.True(data.GetProperty("switches")[0].GetProperty("is_default").GetBoolean());
        Assert.Equal("lab-external", data.GetProperty("switches")[1].GetProperty("name").GetString());
        Assert.Equal("Intel(R) Ethernet", data.GetProperty("switches")[1].GetProperty("net_adapter_interface_description").GetString());
    }

    [Fact]
    public void NativeNetworkInventoryAdapterReturnsStructuredFailureForIncompleteSwitchTopology()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider(
        [
            new DesktopNodeHyperVSwitchInfo("lab-external", "unknown", false, null, null)
        ]));

        var handled = adapter.TryInvoke("network.inventory", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("network.inventory", result.Operation);
        Assert.Equal("PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeNetworkInventoryAdapterAcceptsProviderMappedExternalSwitch()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider(
        [
            DesktopNodeHyperVWmiSwitchProvider.MapSwitch("Default Switch"),
            DesktopNodeHyperVWmiSwitchProvider.MapSwitch(
                "corp-uplink",
                hasInternalManagementPort: true,
                hasExternalBinding: true,
                externalAdapterDescription: "Intel(R) Ethernet Controller")
        ]));

        var handled = adapter.TryInvoke("network.inventory", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        var external = result.Data!.Value.GetProperty("switches")[1];
        Assert.Equal("external", external.GetProperty("type").GetString());
        Assert.Equal("Intel(R) Ethernet Controller", external.GetProperty("net_adapter_interface_description").GetString());
    }

    [Fact]
    public void NativeNetworkInventoryAdapterAcceptsPrivateSwitchTopology()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider(
        [
            DesktopNodeHyperVWmiSwitchProvider.MapSwitch("Default Switch"),
            DesktopNodeHyperVWmiSwitchProvider.MapSwitch("pcv-lab-private")
        ]));

        var handled = adapter.TryInvoke("network.inventory", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        var switches = result.Data!.Value.GetProperty("switches");
        Assert.Equal("pcv-lab-private", switches[1].GetProperty("name").GetString());
        Assert.Equal("private", switches[1].GetProperty("type").GetString());
        Assert.False(switches[1].GetProperty("allow_management_os").GetBoolean());
    }

    [Fact]
    public void NativeNetworkInventoryAdapterReturnsStructuredFailureForMissingManagementOsParityField()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider(
        [
            new DesktopNodeHyperVSwitchInfo("Default Switch", "internal", true, null, null)
        ]));

        var handled = adapter.TryInvoke("network.inventory", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("network.inventory", result.Operation);
        Assert.Equal("PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeNetworkInventoryAdapterReturnsStructuredFailureWhenProviderFails()
    {
        using var parameters = JsonDocument.Parse("{}");
        var adapter = new DesktopNodeHyperVNativeAdapter(new ThrowingHyperVSwitchProvider("Get-VMSwitch unavailable"));

        var handled = adapter.TryInvoke("network.inventory", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("network.inventory", result.Operation);
        Assert.Equal("PCV_NETWORK_INVENTORY_FAILED", result.Error!.Code);
        Assert.True(result.Error.Retryable);
        Assert.Contains("Get-VMSwitch unavailable", result.Error.Detail);
    }

    [Fact]
    public void NativeCheckpointMutationAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha","checkpoint_name":"before-upgrade"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider());

        var handled = adapter.TryInvoke("checkpoint.create", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("checkpoint.create", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("vm_name").GetString());
        Assert.Equal("before-upgrade", result.Data.Value.GetProperty("name").GetString());
        Assert.False(result.Data.Value.TryGetProperty("action", out _));
    }

    [Fact]
    public void NativeCheckpointMutationAdapterRejectsMissingCheckpointName()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider());

        var handled = adapter.TryInvoke("checkpoint.create", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_CHECKPOINT_NAME_INVALID", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeCheckpointMutationAdapterAllowsHyperVNamesWithSpaces()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"lab vm","checkpoint_name":"before upgrade"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("lab vm")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider());

        var handled = adapter.TryInvoke("checkpoint.create", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("lab vm", result.Data!.Value.GetProperty("vm_name").GetString());
        Assert.Equal("before upgrade", result.Data.Value.GetProperty("name").GetString());
    }

    [Fact]
    public void NativeCheckpointMutationAdapterRestoresCheckpointWithNativeProvider()
    {
        using var parameters = JsonDocument.Parse("""{"vm_name":"alpha","checkpoint_name":"before-upgrade"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider());

        var handled = adapter.TryInvoke("checkpoint.restore", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("checkpoint.restore", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("vm_name").GetString());
        Assert.Equal("before-upgrade", result.Data.Value.GetProperty("name").GetString());
        Assert.Equal("restore", result.Data.Value.GetProperty("action").GetString());
    }

    [Theory]
    [InlineData("vm.start", "start")]
    [InlineData("vm.shutdown", "shutdown")]
    [InlineData("vm.poweroff", "poweroff")]
    [InlineData("vm.restart", "restart")]
    [InlineData("vm.pause", "pause")]
    [InlineData("vm.resume", "resume")]
    [InlineData("vm.save", "save")]
    [InlineData("vm.resume-saved", "resume-saved")]
    public void NativeVmPowerStateAdapterMapsProviderResult(string operation, string expectedAction)
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider());

        var handled = adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal(operation, result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal(expectedAction, result.Data.Value.GetProperty("action").GetString());
    }

    [Fact]
    public void NativeAdapterAllowsStartOnTemplateLockedVm()
    {
        using var parameters = JsonDocument.Parse("""{"name":"gold"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("gold") with { TemplateLock = true, State = "stopped" }]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider());

        var handled = adapter.TryInvoke("vm.start", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("start", result.Data!.Value.GetProperty("action").GetString());
    }

    [Fact]
    public void NativeAdapterRejectsDeleteOnTemplateLockedVm()
    {
        using var parameters = JsonDocument.Parse("""{"name":"gold"}""");
        var provider = new RecordingHyperVVmDeleteProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("gold") with { TemplateLock = true }]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.delete", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal(DesktopNodeHyperVVmTemplateLockGuard.LockedCode, result.Error!.Code);
        Assert.False(result.Error.Retryable);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public void NativeAdapterLocksAndUnlocksTemplateVm()
    {
        using var lockParameters = JsonDocument.Parse("""{"name":"gold","locked":true}""");
        using var unlockParameters = JsonDocument.Parse("""{"name":"gold","locked":false}""");
        var provider = new RecordingHyperVVmManageProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("gold")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider(),
            provider);

        var locked = adapter.TryInvoke("vm.template.lock", lockParameters.RootElement, CancellationToken.None, out var lockResult);
        var unlocked = adapter.TryInvoke("vm.template.lock", unlockParameters.RootElement, CancellationToken.None, out var unlockResult);

        Assert.True(locked);
        Assert.True(lockResult.Ok);
        Assert.Equal("lock", lockResult.Data!.Value.GetProperty("action").GetString());
        Assert.True(unlocked);
        Assert.True(unlockResult.Ok);
        Assert.Equal("unlock", unlockResult.Data!.Value.GetProperty("action").GetString());
        Assert.Equal(2, provider.CallCount);
        Assert.False(provider.LastLocked);
    }

    [Fact]
    public void NativeVmRenameAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha","new_name":"beta"}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider());

        var handled = adapter.TryInvoke("vm.rename", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.rename", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal("beta", result.Data.Value.GetProperty("new_name").GetString());
        Assert.Equal("rename", result.Data.Value.GetProperty("action").GetString());
    }

    [Fact]
    public void NativeVmManageAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var provider = new RecordingHyperVVmManageProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha") with { ManagedByPurecvisor = false }]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.manage", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.manage", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal("manage", result.Data.Value.GetProperty("action").GetString());
        Assert.Equal(1, provider.CallCount);
        Assert.Equal("alpha", provider.LastVmName);
    }

    [Fact]
    public void NativeVmManageAdapterMapsAlreadyManagedWithoutSecondMarker()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var provider = new RecordingHyperVVmManageProvider("already-managed");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.manage", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("already-managed", result.Data!.Value.GetProperty("action").GetString());
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public void NativeVmClonePreviewAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"source":"alpha","name":"beta"}""");
        var provider = new RecordingHyperVVmCloneProvider();
        var adapter = CreateCloneAdapter(provider);

        var handled = adapter.TryInvoke("vm.clone.preview", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.clone.preview", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("source").GetString());
        Assert.Equal("beta", result.Data.Value.GetProperty("name").GetString());
        Assert.Equal("preview", result.Data.Value.GetProperty("action").GetString());
        Assert.Equal(2, result.Data.Value.GetProperty("generation").GetInt32());
        Assert.Equal(@"D:\PureCVisor\VMs\beta", result.Data.Value.GetProperty("directory").GetString());
        Assert.Equal(1, result.Data.Value.GetProperty("disk_count").GetInt32());
        Assert.Equal(1024, result.Data.Value.GetProperty("planned_copy_bytes").GetInt64());
        var disk = Assert.Single(result.Data.Value.GetProperty("disks").EnumerateArray());
        Assert.Equal(@"D:\PureCVisor\VMs\alpha\disk0.vhdx", disk.GetProperty("source").GetString());
        Assert.Equal(@"D:\PureCVisor\VMs\beta\disk0.vhdx", disk.GetProperty("target").GetString());
        Assert.Equal(1, provider.PreviewCallCount);
        Assert.Equal(0, provider.InvokeCallCount);
        Assert.Equal("alpha", provider.LastRequest!.SourceName);
        Assert.Equal("beta", provider.LastRequest.TargetName);
        Assert.Equal(@"D:\PureCVisor\VMs", provider.LastRequest.VmRoot);
    }

    [Fact]
    public void NativeVmCloneAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"source":"alpha","name":"beta"}""");
        var provider = new RecordingHyperVVmCloneProvider();
        var adapter = CreateCloneAdapter(provider);

        var handled = adapter.TryInvoke("vm.clone", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.clone", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("source").GetString());
        Assert.Equal("beta", result.Data.Value.GetProperty("name").GetString());
        Assert.Equal("clone", result.Data.Value.GetProperty("action").GetString());
        Assert.Equal(@"D:\PureCVisor\VMs\beta", result.Data.Value.GetProperty("directory").GetString());
        Assert.Equal(@"D:\PureCVisor\VMs\beta\disk0.vhdx", Assert.Single(result.Data.Value.GetProperty("disks").EnumerateArray()).GetString());
        Assert.Equal(0, provider.PreviewCallCount);
        Assert.Equal(1, provider.InvokeCallCount);
        Assert.Equal("alpha", provider.LastRequest!.SourceName);
        Assert.Equal("beta", provider.LastRequest.TargetName);
    }

    [Fact]
    public void NativeVmClonePreviewAdapterMapsNameAndTargetParamsAsSourceAndName()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha","target":"beta"}""");
        var provider = new RecordingHyperVVmCloneProvider();
        var adapter = CreateCloneAdapter(provider);

        var handled = adapter.TryInvoke("vm.clone.preview", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("alpha", provider.LastRequest!.SourceName);
        Assert.Equal("beta", provider.LastRequest.TargetName);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("source").GetString());
        Assert.Equal("beta", result.Data.Value.GetProperty("name").GetString());
    }

    [Theory]
    [InlineData("vm.clone.preview")]
    [InlineData("vm.clone")]
    public void NativeVmCloneAdapterPassesThroughProviderNotFound(string operation)
    {
        using var parameters = JsonDocument.Parse("""{"source":"missing","name":"beta"}""");
        var provider = new RecordingHyperVVmCloneProvider(throwNotFound: true);
        var adapter = CreateCloneAdapter(provider);

        var handled = adapter.TryInvoke(operation, parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal(operation, result.Operation);
        Assert.Equal("PCV_VM_NOT_FOUND", result.Error!.Code);
        Assert.False(result.Error.Retryable);
        if (operation == "vm.clone.preview")
        {
            Assert.Equal(1, provider.PreviewCallCount);
            Assert.Equal(0, provider.InvokeCallCount);
        }
        else
        {
            Assert.Equal(0, provider.PreviewCallCount);
            Assert.Equal(1, provider.InvokeCallCount);
        }
    }

    [Fact]
    public void NativeVmDeleteAdapterProceedsAfterManagePromotesUnmanagedRow()
    {
        using var manageParameters = JsonDocument.Parse("""{"name":"foreign"}""");
        using var deleteParameters = JsonDocument.Parse("""{"name":"foreign"}""");
        var vms = new MutableHyperVVmProvider([CompleteVm("foreign") with { ManagedByPurecvisor = false }]);
        var deleteProvider = new RecordingHyperVVmDeleteProvider();
        var manageProvider = new PromotingHyperVVmManageProvider(vms);
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            vms,
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            deleteProvider,
            new RecordingHyperVVmRenameProvider(),
            manageProvider);

        var blocked = adapter.TryInvoke("vm.delete", deleteParameters.RootElement, CancellationToken.None, out var blockedResult);

        Assert.True(blocked);
        Assert.False(blockedResult.Ok);
        Assert.Equal("PCV_VM_NOT_MANAGED_BY_PURECVISOR", blockedResult.Error!.Code);
        Assert.Equal(0, deleteProvider.CallCount);

        var managed = adapter.TryInvoke("vm.manage", manageParameters.RootElement, CancellationToken.None, out var managedResult);

        Assert.True(managed);
        Assert.True(managedResult.Ok);
        Assert.Equal("manage", managedResult.Data!.Value.GetProperty("action").GetString());
        Assert.True(Assert.Single(vms.GetVms(CancellationToken.None)).ManagedByPurecvisor);

        var deleted = adapter.TryInvoke("vm.delete", deleteParameters.RootElement, CancellationToken.None, out var deletedResult);

        Assert.True(deleted);
        Assert.True(deletedResult.Ok);
        Assert.Equal("delete", deletedResult.Data!.Value.GetProperty("action").GetString());
        Assert.Equal(1, deleteProvider.CallCount);
    }

    [Fact]
    public void NativeVmDeleteAdapterReportsStorageCleanup()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var cleanup = new DesktopNodeHyperVVmStorageCleanupInfo(
            @"D:\PureCVisor\VMs\alpha",
            [@"D:\PureCVisor\VMs\alpha\disk0.vhdx"],
            [@"D:\PureCVisor\VMs\alpha"],
            [new DesktopNodeHyperVRetainedStorage(@"E:\shared\data.vhdx", DesktopNodeHyperVVmStorageCleanup.OutsideConfigurationRoot)]);
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(cleanup));

        var handled = adapter.TryInvoke("vm.delete", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        var storage = result.Data!.Value.GetProperty("storage_cleanup");
        Assert.Equal(@"D:\PureCVisor\VMs\alpha\disk0.vhdx", Assert.Single(storage.GetProperty("removed_files").EnumerateArray()).GetString());
        var retained = Assert.Single(storage.GetProperty("retained").EnumerateArray());
        Assert.Equal("outside-configuration-root", retained.GetProperty("reason").GetString());
    }

    [Fact]
    public void NativeVmPowerStateAdapterReturnsCancellationFailureWhenProviderObservesToken()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        using var cts = new CancellationTokenSource();
        var provider = new CancelingHyperVVmPowerStateProvider(cts);
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            provider,
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider());

        var handled = adapter.TryInvoke("vm.start", parameters.RootElement, cts.Token, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_NATIVE_OPERATION_CANCELED", result.Error!.Code);
        Assert.True(result.Error.Retryable);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public void NativeVmPowerStateAdapterDoesNotTreatUnrequestedProviderOperationCanceledAsCancellation()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var provider = new UnexpectedOperationCanceledHyperVVmPowerStateProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            provider,
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider());

        var handled = adapter.TryInvoke("vm.start", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_VM_POWER_STATE_FAILED", result.Error!.Code);
        Assert.NotEqual("PCV_NATIVE_OPERATION_CANCELED", result.Error.Code);
        Assert.True(result.Error.Retryable);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public void NativeVmPowerStateAdapterRejectsMissingVmName()
    {
        using var parameters = JsonDocument.Parse("""{}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider());

        var handled = adapter.TryInvoke("vm.start", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_VM_NAME_INVALID", result.Error!.Code);
        Assert.False(result.Error.Retryable);
    }

    [Fact]
    public void NativeVmCreateAdapterMapsProviderResult()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha","iso_path":"D:\\iso\\rocky.iso","cpu":2,"memory_mb":4096,"disk_gb":40,"vm_root":"D:\\VMs","generation":2}""");
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider());

        var handled = adapter.TryInvoke("vm.create", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.create", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal("D:\\VMs\\alpha", result.Data.Value.GetProperty("vm_dir").GetString());
        Assert.Equal("D:\\VMs\\alpha\\disk0.vhdx", result.Data.Value.GetProperty("vhd_path").GetString());
        Assert.Equal("D:\\iso\\rocky.iso", result.Data.Value.GetProperty("iso_path").GetString());
        Assert.Equal("Default Switch", result.Data.Value.GetProperty("switch").GetString());
        Assert.Equal(2, result.Data.Value.GetProperty("generation").GetInt32());
    }

    [Fact]
    public void NativeVmCreateAdapterRejectsInvalidCreateParamsBeforeProviderMutation()
    {
        using var parameters = JsonDocument.Parse("""{"name":"bad/name","iso_path":"D:\\iso\\rocky.iso","cpu":2,"memory_mb":4096,"disk_gb":40}""");
        var provider = new RecordingHyperVVmCreateProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.create", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_VM_NAME_INVALID", result.Error!.Code);
        Assert.False(result.Error.Retryable);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public void NativeVmCreateAdapterRejectsGenerationOneBeforeProviderMutation()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha","iso_path":"D:\\iso\\rocky.iso","cpu":2,"memory_mb":4096,"disk_gb":40,"generation":1}""");
        var provider = new RecordingHyperVVmCreateProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.create", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_GENERATION_INVALID", result.Error!.Code);
        Assert.False(result.Error.Retryable);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public void NativeVmDeleteAdapterMapsProviderResultForManagedVm()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var provider = new RecordingHyperVVmDeleteProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.delete", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("vm.delete", result.Operation);
        Assert.Equal("alpha", result.Data!.Value.GetProperty("name").GetString());
        Assert.Equal("delete", result.Data.Value.GetProperty("action").GetString());
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public void NativeVmDeleteAdapterReturnsAbsentWithoutProviderMutationWhenVmMissing()
    {
        using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
        var provider = new RecordingHyperVVmDeleteProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.delete", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.True(result.Ok);
        Assert.Equal("absent", result.Data!.Value.GetProperty("action").GetString());
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public void NativeVmDeleteAdapterRejectsUnmanagedVmBeforeProviderMutation()
    {
        using var parameters = JsonDocument.Parse("""{"name":"foreign"}""");
        var provider = new RecordingHyperVVmDeleteProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("foreign") with { ManagedByPurecvisor = false }]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.delete", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_VM_NOT_MANAGED_BY_PURECVISOR", result.Error!.Code);
        Assert.False(result.Error.Retryable);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public void NativeVmDeleteAdapterRejectsInvalidVmNameBeforeProviderMutation()
    {
        using var parameters = JsonDocument.Parse("""{"name":"bad/name"}""");
        var provider = new RecordingHyperVVmDeleteProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            provider);

        var handled = adapter.TryInvoke("vm.delete", parameters.RootElement, CancellationToken.None, out var result);

        Assert.True(handled);
        Assert.False(result.Ok);
        Assert.Equal("PCV_VM_NAME_INVALID", result.Error!.Code);
        Assert.False(result.Error.Retryable);
        Assert.Equal(0, provider.CallCount);
    }

    private sealed class RecordingHyperVSwitchProvider(IReadOnlyList<DesktopNodeHyperVSwitchInfo> switches) : IDesktopNodeHyperVSwitchProvider
    {
        public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken)
        {
            return switches;
        }
    }

    private sealed class ThrowingHyperVSwitchProvider(string message) : IDesktopNodeHyperVSwitchProvider
    {
        public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class RecordingHyperVVmProvider(IReadOnlyList<DesktopNodeHyperVVmInfo> vms) : IDesktopNodeHyperVVmProvider
    {
        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
        {
            return vms;
        }
    }

    private sealed class ThrowingHyperVVmProvider(string message) : IDesktopNodeHyperVVmProvider
    {
        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class RecordingHyperVCheckpointProvider(IReadOnlyList<DesktopNodeHyperVCheckpointInfo> checkpoints) : IDesktopNodeHyperVCheckpointProvider
    {
        public IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName, CancellationToken cancellationToken)
        {
            return checkpoints;
        }
    }

    private sealed class ThrowingHyperVCheckpointProvider(string message) : IDesktopNodeHyperVCheckpointProvider
    {
        public IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class RecordingHyperVCheckpointMutationProvider : IDesktopNodeHyperVCheckpointMutationProvider
    {
        public DesktopNodeHyperVCheckpointMutationInfo Invoke(string operation, string vmName, string checkpointName, CancellationToken cancellationToken)
        {
            var action = operation switch
            {
                "checkpoint.delete" => "delete",
                "checkpoint.restore" => "restore",
                _ => null
            };
            return new DesktopNodeHyperVCheckpointMutationInfo(vmName, checkpointName, action);
        }
    }

    private sealed class RecordingHyperVVmPowerStateProvider : IDesktopNodeHyperVVmPowerStateProvider
    {
        public DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken)
        {
            var action = operation switch
            {
                "vm.start" => "start",
                "vm.shutdown" => "shutdown",
                "vm.poweroff" => "poweroff",
                "vm.restart" => "restart",
                "vm.pause" => "pause",
                "vm.resume" => "resume",
                "vm.save" => "save",
                "vm.resume-saved" => "resume-saved",
                _ => "unsupported"
            };
            return new DesktopNodeHyperVVmPowerStateInfo(vmName, action);
        }
    }

    private sealed class RecordingHyperVVmRenameProvider : IDesktopNodeHyperVVmRenameProvider
    {
        public DesktopNodeHyperVVmRenameInfo Invoke(string vmName, string newName, CancellationToken cancellationToken)
        {
            return new DesktopNodeHyperVVmRenameInfo(vmName, newName, "rename");
        }
    }

    private sealed class RecordingHyperVVmManageProvider(string action = "manage") : IDesktopNodeHyperVVmManageProvider
    {
        public int CallCount { get; private set; }

        public string? LastVmName { get; private set; }

        public bool? LastLocked { get; private set; }

        public DesktopNodeHyperVVmManageInfo Invoke(string vmName, CancellationToken cancellationToken)
        {
            CallCount += 1;
            LastVmName = vmName;
            return new DesktopNodeHyperVVmManageInfo(vmName, action);
        }

        public DesktopNodeHyperVVmManageInfo InvokeTemplateLock(string vmName, bool locked, CancellationToken cancellationToken)
        {
            CallCount += 1;
            LastVmName = vmName;
            LastLocked = locked;
            return new DesktopNodeHyperVVmManageInfo(vmName, locked ? "lock" : "unlock");
        }
    }

    private sealed class RecordingHyperVVmCloneProvider(bool throwNotFound = false) : IDesktopNodeHyperVVmCloneProvider
    {
        public int PreviewCallCount { get; private set; }

        public int InvokeCallCount { get; private set; }

        public DesktopNodeHyperVVmCloneRequest? LastRequest { get; private set; }

        public DesktopNodeHyperVVmClonePlan Preview(DesktopNodeHyperVVmCloneRequest request, CancellationToken cancellationToken)
        {
            PreviewCallCount += 1;
            LastRequest = request;
            ThrowIfMissing(request.SourceName);
            return new DesktopNodeHyperVVmClonePlan(
                request.SourceName,
                request.TargetName,
                "preview",
                2,
                Path.Combine(request.VmRoot, request.TargetName),
                1,
                1024,
                [
                    new DesktopNodeHyperVVmCloneDiskPlan(
                        Path.Combine(request.VmRoot, request.SourceName, "disk0.vhdx"),
                        Path.Combine(request.VmRoot, request.TargetName, "disk0.vhdx"))
                ]);
        }

        public DesktopNodeHyperVVmCloneInfo Invoke(DesktopNodeHyperVVmCloneRequest request, CancellationToken cancellationToken)
        {
            InvokeCallCount += 1;
            LastRequest = request;
            ThrowIfMissing(request.SourceName);
            return new DesktopNodeHyperVVmCloneInfo(
                request.SourceName,
                request.TargetName,
                "clone",
                Path.Combine(request.VmRoot, request.TargetName),
                [Path.Combine(request.VmRoot, request.TargetName, "disk0.vhdx")]);
        }

        private void ThrowIfMissing(string sourceName)
        {
            if (!throwNotFound)
            {
                return;
            }

            throw new DesktopNodeHyperVNativeOperationException(
                "PCV_VM_NOT_FOUND",
                $"VM '{sourceName}' was not found.",
                "The VM was not present in the native Hyper-V VM inventory response.",
                false);
        }
    }

    private static DesktopNodeHyperVNativeAdapter CreateCloneAdapter(IDesktopNodeHyperVVmCloneProvider cloneProvider)
    {
        return new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHyperVVmProvider([CompleteVm("alpha")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider(),
            new RecordingHyperVVmManageProvider(),
            cloneProvider);
    }

    private sealed class MutableHyperVVmProvider(IEnumerable<DesktopNodeHyperVVmInfo> vms) : IDesktopNodeHyperVVmProvider
    {
        private readonly List<DesktopNodeHyperVVmInfo> vms = [.. vms];

        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
        {
            return vms;
        }

        public void Replace(DesktopNodeHyperVVmInfo vm)
        {
            var index = vms.FindIndex(item => string.Equals(item.Name, vm.Name, StringComparison.Ordinal));
            if (index >= 0)
            {
                vms[index] = vm;
                return;
            }

            vms.Add(vm);
        }
    }

    private sealed class PromotingHyperVVmManageProvider(MutableHyperVVmProvider vms) : IDesktopNodeHyperVVmManageProvider
    {
        public DesktopNodeHyperVVmManageInfo Invoke(string vmName, CancellationToken cancellationToken)
        {
            var current = vms.GetVms(cancellationToken)
                .First(vm => string.Equals(vm.Name, vmName, StringComparison.Ordinal));
            if (current.ManagedByPurecvisor)
            {
                return new DesktopNodeHyperVVmManageInfo(vmName, "already-managed");
            }

            vms.Replace(current with { ManagedByPurecvisor = true });
            return new DesktopNodeHyperVVmManageInfo(vmName, "manage");
        }

        public DesktopNodeHyperVVmManageInfo InvokeTemplateLock(string vmName, bool locked, CancellationToken cancellationToken)
        {
            var current = vms.GetVms(cancellationToken)
                .First(vm => string.Equals(vm.Name, vmName, StringComparison.Ordinal));
            if (locked && current.TemplateLock)
            {
                return new DesktopNodeHyperVVmManageInfo(vmName, "already-locked");
            }

            if (!locked && !current.TemplateLock)
            {
                return new DesktopNodeHyperVVmManageInfo(vmName, "already-unlocked");
            }

            vms.Replace(current with { TemplateLock = locked });
            return new DesktopNodeHyperVVmManageInfo(vmName, locked ? "lock" : "unlock");
        }
    }

    private sealed class CancelingHyperVVmPowerStateProvider(CancellationTokenSource cancellation) : IDesktopNodeHyperVVmPowerStateProvider
    {
        public int CallCount { get; private set; }

        public DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken)
        {
            CallCount += 1;
            cancellation.Cancel();
            throw new OperationCanceledException("Provider observed cancellation during native power-state operation.", cancellationToken);
        }
    }

    private sealed class UnexpectedOperationCanceledHyperVVmPowerStateProvider : IDesktopNodeHyperVVmPowerStateProvider
    {
        public int CallCount { get; private set; }

        public DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken)
        {
            CallCount += 1;
            throw new OperationCanceledException("Provider threw OperationCanceledException without the supplied token being canceled.");
        }
    }

    private sealed class RecordingHyperVVmCreateProvider : IDesktopNodeHyperVVmCreateProvider
    {
        public int CallCount { get; private set; }

        public DesktopNodeHyperVVmCreateInfo Invoke(DesktopNodeHyperVVmCreateRequest request, CancellationToken cancellationToken)
        {
            CallCount += 1;
            return new DesktopNodeHyperVVmCreateInfo(
                Name: request.Name,
                VmDirectory: Path.Combine(request.VmRoot, request.Name),
                VhdPath: Path.Combine(request.VmRoot, request.Name, "disk0.vhdx"),
                IsoPath: request.IsoPath,
                SwitchName: "Default Switch",
                Generation: request.Generation,
                Steps:
                [
                    "Create VM folder",
                    "Create VHDX",
                    "Create Hyper-V VM",
                    "Set resources",
                    "Attach ISO",
                    "Attach Default Switch"
                ]);
        }
    }

    private sealed class RecordingHyperVVmDeleteProvider(DesktopNodeHyperVVmStorageCleanupInfo? storageCleanup = null) : IDesktopNodeHyperVVmDeleteProvider
    {
        public int CallCount { get; private set; }

        public DesktopNodeHyperVVmDeleteInfo Invoke(string vmName, CancellationToken cancellationToken)
        {
            CallCount += 1;
            return new DesktopNodeHyperVVmDeleteInfo(vmName, "delete", storageCleanup);
        }
    }

    private static DesktopNodeHyperVNativeAdapter CreateAdapter(
        IDesktopNodeHyperVVmMediaProvider? vmMediaProvider = null)
    {
        return new DesktopNodeHyperVNativeAdapter(
            new RecordingHyperVSwitchProvider([]),
            new RecordingHostStatusProvider(new DesktopNodeHyperVHostStatusData(
                Supported: true,
                Reasons: [],
                Windows: new DesktopNodeHyperVHostWindowsInfo("Windows 11", "24H2", "Pro"),
                Admin: new DesktopNodeHyperVHostAdminInfo(true),
                HyperV: new DesktopNodeHyperVHostHyperVInfo(true, true, true))),
            new RecordingHyperVVmProvider([CompleteVm("lab-vm")]),
            new RecordingHyperVCheckpointProvider([]),
            new RecordingHyperVCheckpointMutationProvider(),
            new RecordingHyperVVmPowerStateProvider(),
            new RecordingHyperVVmCreateProvider(),
            new RecordingHyperVVmDeleteProvider(),
            new RecordingHyperVVmRenameProvider(),
            vmMediaProvider ?? new RecordingHyperVVmMediaProvider(),
            new RecordingHyperVVmResourceMutationProvider());
    }

    private sealed class RecordingHyperVVmMediaProvider : IDesktopNodeHyperVVmMediaProvider
    {
        public DesktopNodeHyperVVmMediaRequest? LastRequest { get; private set; }

        public DesktopNodeHyperVVmMediaInfo Invoke(
            DesktopNodeHyperVVmMediaRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            var action = request.Operation == "vm.attach" ? "attach" : "eject";
            return new DesktopNodeHyperVVmMediaInfo(request.VmName, action, request.IsoPath);
        }

        public DesktopNodeHyperVVmDeviceAddInfo AddDvd(
            DesktopNodeHyperVVmDeviceAddRequest request,
            CancellationToken cancellationToken)
        {
            return new DesktopNodeHyperVVmDeviceAddInfo("dvd-add", request.VmName, "dvd", null);
        }
    }

    private sealed class RecordingHyperVVmResourceMutationProvider : IDesktopNodeHyperVVmResourceMutationProvider
    {
        public DesktopNodeHyperVVmResourceMutationRequest? LastRequest { get; private set; }

        public DesktopNodeHyperVVmResourceMutationInfo Invoke(DesktopNodeHyperVVmResourceMutationRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var action = request.Operation == "vm.qos.storage.set" ? "storage-qos" : "network-qos";
            var target = request.Operation == "vm.qos.storage.set" ? request.QosDisk : request.QosAdapter;
            return new DesktopNodeHyperVVmResourceMutationInfo(
                request.Name,
                action,
                Evidence: new SortedDictionary<string, object?>
                {
                    ["contract"] = "hyperv-qos-mutation-apply-evidence.v1",
                    ["operation"] = request.Operation,
                    ["target"] = target,
                    ["audit"] = new SortedDictionary<string, object?> { ["args_redacted"] = true }
                });
        }
    }

    private static DesktopNodeHyperVVmInfo CompleteVm(string name)
    {
        return new DesktopNodeHyperVVmInfo(
            Id: name,
            Name: name,
            Platform: "hyperv",
            GuestFamily: "linux",
            State: "running",
            Cpu: new DesktopNodeHyperVVmCpuInfo(2),
            Memory: new DesktopNodeHyperVVmMemoryInfo(4096, 2048, false),
            Generation: 2,
            Storage: [],
            Network: [],
            Checkpoints: new DesktopNodeHyperVVmCheckpointInfo(0),
            Console: new DesktopNodeHyperVVmConsoleInfo("vmconnect", true),
            ManagedByPurecvisor: true);
    }

    private sealed class RecordingHostStatusProvider(DesktopNodeHyperVHostStatusData status) : IDesktopNodeHyperVHostStatusProvider
    {
        public DesktopNodeHyperVHostStatusData GetStatus(CancellationToken cancellationToken)
        {
            return status;
        }
    }

    private sealed class ThrowingHostStatusProvider(string message) : IDesktopNodeHyperVHostStatusProvider
    {
        public DesktopNodeHyperVHostStatusData GetStatus(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(message);
        }
    }
}
