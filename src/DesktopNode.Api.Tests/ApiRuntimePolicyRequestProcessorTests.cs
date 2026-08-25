using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed partial class ApiRuntimePolicyRequestProcessorTests
{
    [Fact]
    public void HostStatusRouteReturnsNativeFailureWithoutPowerShellFallbackWhenNativeAdapterDoesNotHandleRoute()
    {
        var calls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/host/status"));

        Assert.Equal(502, response.StatusCode);
        Assert.Single(nativeCalls);
        Assert.Equal("host.status", nativeCalls[0]);
        Assert.Empty(calls);

        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("host.status", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("PCV_NATIVE_ROUTE_NOT_HANDLED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void HostStatusRouteUsesNativeAdapterWithoutPowerShellFallback()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "host.status", """
            {"ok":true,"operation":"host.status","data":{"supported":true,"source":"native-csharp","reasons":[],"windows":{"caption":"Windows 11 Pro","version":"23H2","edition":"Pro"},"admin":{"elevated":true},"hyperv":{"feature_enabled":true,"vmms_running":true,"default_switch_present":true}},"error":null}
            """));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/host/status"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Single(nativeCalls);
        Assert.Equal("host.status", nativeCalls[0]);

        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("host.status", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("native-csharp", data.GetProperty("source").GetString());
        Assert.True(data.GetProperty("supported").GetBoolean());
    }

    [Fact]
    public void NetworkInventoryRouteUsesNativeAdapterWithoutPowerShellFallback()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "network.inventory", """
            {"ok":true,"operation":"network.inventory","data":{"source":"native-csharp","mutating":false,"switches":[]},"error":null}
            """));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/network/inventory"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Single(nativeCalls);
        Assert.Equal("network.inventory", nativeCalls[0]);

        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("network.inventory", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("native-csharp", data.GetProperty("source").GetString());
        Assert.False(data.GetProperty("mutating").GetBoolean());
    }

    [Fact]
    public void NetworkInventoryRouteReturnsNativeFailureWithoutPowerShellFallbackWhenNativeTopologyIncomplete()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new DesktopNodeHyperVNativeAdapter(new RecordingHyperVSwitchProvider(
            [
                new DesktopNodeHyperVSwitchInfo("lab-external", "unknown", false, null, null)
            ])));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/network/inventory"));

        Assert.Equal(502, response.StatusCode);
        Assert.Empty(fallbackCalls);

        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("network.inventory", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void VmListRouteReturnsNativeFailureWithoutPowerShellFallbackWhenNativeAdapterDoesNotHandleRoute()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms"));

        Assert.Equal(502, response.StatusCode);
        Assert.Single(nativeCalls);
        Assert.Equal("vm.list", nativeCalls[0]);
        Assert.Empty(fallbackCalls);

        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("vm.list", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("PCV_NATIVE_ROUTE_NOT_HANDLED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void VmListRouteUsesNativeVmProviderWithoutPowerShellFallback()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new DesktopNodeHyperVNativeAdapter(
                new RecordingHyperVSwitchProvider([]),
                new RecordingHyperVVmProvider(
                [
                    new DesktopNodeHyperVVmInfo(
                        Id: "alpha",
                        Name: "alpha",
                        Platform: "hyperv",
                        GuestFamily: "linux",
                        State: "running",
                        Cpu: new DesktopNodeHyperVVmCpuInfo(2),
                        Memory: new DesktopNodeHyperVVmMemoryInfo(4096, 2048, false),
                        Generation: 2,
                        Storage:
                        [
                            new DesktopNodeHyperVVmDiskInfo("vhdx", "D:\\VMs\\alpha\\disk.vhdx", 32, true)
                        ],
                        Network:
                        [
                            new DesktopNodeHyperVVmNetworkInfo("Default Switch", "default-switch")
                        ],
                        Checkpoints: new DesktopNodeHyperVVmCheckpointInfo(1),
                        Console: new DesktopNodeHyperVVmConsoleInfo("vmconnect", true),
                        ManagedByPurecvisor: true)
                ])));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(fallbackCalls);

        using var document = JsonDocument.Parse(response.Body);
        var vm = document.RootElement.GetProperty("data")[0];
        Assert.Equal("vm.list", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("alpha", vm.GetProperty("id").GetString());
        Assert.Equal("alpha", vm.GetProperty("name").GetString());
        Assert.Equal("running", vm.GetProperty("state").GetString());
        Assert.Equal(2, vm.GetProperty("cpu").GetProperty("count").GetInt32());
        Assert.Equal(4096, vm.GetProperty("memory").GetProperty("startup_mb").GetInt32());
        Assert.True(vm.GetProperty("managed_by_purecvisor").GetBoolean());
    }

    [Fact]
    public async Task ProcessorSerializesConcurrentHandleCalls()
    {
        var nativeAdapter = new BlockingConcurrencyNativeHyperVAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: nativeAdapter);

        static Task<DesktopNodeApiResponse> handleHostStatusOnDedicatedThread(DesktopNodeApiRequestProcessor requestProcessor)
        {
            // Keep this test about processor serialization, not shared thread-pool scheduling latency.
            return Task.Factory.StartNew(
                () => requestProcessor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/host/status")),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        var first = handleHostStatusOnDedicatedThread(processor);

        try
        {
            Assert.True(nativeAdapter.WaitForFirstCall(TimeSpan.FromSeconds(10)));

            var second = handleHostStatusOnDedicatedThread(processor);
            Assert.False(
                nativeAdapter.WaitForConcurrentCall(TimeSpan.FromMilliseconds(500)),
                "DesktopNodeApiRequestProcessor allowed concurrent handler execution.");

            nativeAdapter.ReleaseFirstCall();
            var responses = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.All(responses, response => Assert.Equal(200, response.StatusCode));
            Assert.Equal(2, nativeAdapter.CallCount);
            Assert.Equal(1, nativeAdapter.MaxConcurrent);
        }
        finally
        {
            nativeAdapter.ReleaseFirstCall();
        }
    }

    [Fact]
    public void VmDetailRouteReturnsNativeFailureWithoutPowerShellFallbackWhenNativeVmListDeclinesRoute()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/lab%20vm%2001"));

        Assert.Equal(502, response.StatusCode);
        Assert.Single(nativeCalls);
        Assert.Equal("vm.list", nativeCalls[0]);
        Assert.Empty(fallbackCalls);

        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("vm.list", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("PCV_NATIVE_ROUTE_NOT_HANDLED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void CheckpointListRouteReturnsNativeFailureWithoutPowerShellFallbackWhenNativeInventoryDeclinesRoute()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new DesktopNodeHyperVNativeAdapter(
                new RecordingHyperVSwitchProvider([]),
                new RecordingHyperVVmProvider([]),
                new RecordingHyperVCheckpointProvider([])));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/alpha/checkpoints"));

        Assert.Equal(502, response.StatusCode);
        Assert.Empty(fallbackCalls);

        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("checkpoint.list", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("PCV_NATIVE_CHECKPOINT_LIST_VM_INVENTORY_EMPTY", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void CheckpointListRouteIncludesIsCurrentWithoutLeakingInstanceId()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new DesktopNodeHyperVNativeAdapter(
                new RecordingHyperVSwitchProvider([]),
                new RecordingHyperVVmProvider(
                [
                    new DesktopNodeHyperVVmInfo(
                        Id: "alpha",
                        Name: "alpha",
                        Platform: "hyperv",
                        GuestFamily: "linux",
                        State: "running",
                        Cpu: new DesktopNodeHyperVVmCpuInfo(2),
                        Memory: new DesktopNodeHyperVVmMemoryInfo(4096, 2048, false),
                        Generation: 2,
                        Storage: [],
                        Network: [],
                        Checkpoints: new DesktopNodeHyperVVmCheckpointInfo(1),
                        Console: new DesktopNodeHyperVVmConsoleInfo("vmconnect", true),
                        ManagedByPurecvisor: true)
                ]),
                new RecordingHyperVCheckpointProvider(
                [
                    new DesktopNodeHyperVCheckpointInfo("before-upgrade", "alpha", "2026-05-03T00:00:00.0000000Z", InstanceId: "snap-1")
                ])));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/alpha/checkpoints"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var checkpoint = document.RootElement.GetProperty("data")[0];
        Assert.Equal("checkpoint.list", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("before-upgrade", checkpoint.GetProperty("name").GetString());
        Assert.Equal("alpha", checkpoint.GetProperty("vm_name").GetString());
        Assert.True(checkpoint.TryGetProperty("is_current", out var isCurrent));
        Assert.Equal(JsonValueKind.Null, isCurrent.ValueKind);
        Assert.False(checkpoint.TryGetProperty("instance_id", out _));
        Assert.False(checkpoint.TryGetProperty("InstanceId", out _));
    }

    [Fact]
    public void VmDetailRouteUsesNativeVmListWithoutPowerShellFallbackWhenNativeComplete()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "vm.list", """
            {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running"}],"error":null}
            """));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/alpha"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Single(nativeCalls);
        Assert.Equal("vm.list", nativeCalls[0]);

        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("vm.get", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("alpha", document.RootElement.GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public void VmDetailRouteReturnsNotFoundFromNativeInventoryWithoutFallbackRetryWhenNativeComplete()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "vm.list", """
            {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running"}],"error":null}
            """));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/beta"));

        Assert.Equal(404, response.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Single(nativeCalls);
        Assert.Equal("vm.list", nativeCalls[0]);
        Assert.Contains("PCV_VM_NOT_FOUND", response.Body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/api/v1/vms/alpha/memory-stats", "vm.memory-stats", "memory")]
    [InlineData("/api/v1/vms/alpha/cpu-stats", "vm.cpu-stats", "cpu")]
    [InlineData("/api/v1/vms/alpha/blkio", "vm.blkio-get", "storage_qos")]
    [InlineData("/api/v1/vms/alpha/bandwidth", "vm.bandwidth", "network_qos")]
    [InlineData("/api/v1/vms/alpha/guest-agent/status", "vm.guest-agent-status", "guest_agent")]
    [InlineData("/api/v1/vms/alpha/guest-agent/ping", "vm.guest-ping", "guest_ping")]
    public void VmInventoryReadbackRoutesUseNativeAdapterWithoutExternalFallback(string path, string expectedOperation, string expectedBucket)
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmInventoryReadbackAdapter(nativeCalls));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", path));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(fallbackCalls);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal(expectedOperation, nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("alpha", parameters.RootElement.GetProperty("vm_name").GetString());

        using var document = JsonDocument.Parse(response.Body);
        Assert.True(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(expectedOperation, document.RootElement.GetProperty("operation").GetString());
        Assert.True(document.RootElement.GetProperty("data").TryGetProperty(expectedBucket, out _));
    }

    [Fact]
    public void VmCreateQueuesJobAndWorkerDispatchesToNativeAdapterWithoutExternalFallback()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVCreateAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"alpha","iso_path":"D:\\iso\\rocky.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Empty(nativeCalls);
        using var createdDocument = JsonDocument.Parse(create.Body);
        var jobId = createdDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(jobId));
        Assert.Equal("queued", createdDocument.RootElement.GetProperty("data").GetProperty("status").GetString());

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.NotNull(tick.Job);
        Assert.Equal(jobId, tick.Job.Value.GetProperty("job_id").GetString());
        Assert.Equal("succeeded", tick.Job.Value.GetProperty("status").GetString());
        Assert.Empty(fallbackCalls);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal("vm.create", nativeCall.Operation);
        using var callParams = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("alpha", callParams.RootElement.GetProperty("name").GetString());

        var get = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{jobId}"));
        using var getDocument = JsonDocument.Parse(get.Body);
        Assert.Equal("succeeded", getDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal("alpha", getDocument.RootElement.GetProperty("data").GetProperty("result").GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public void ProcessWorkerPoolProcessesAtMostOneQueuedJobPerCall()
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVCreateAdapter(nativeCalls));

        var first = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"alpha","iso_path":"D:\\iso\\alpha.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));
        var second = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"beta","iso_path":"D:\\iso\\beta.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));

        Assert.Equal(202, first.StatusCode);
        Assert.Equal(202, second.StatusCode);

        var ticks = processor.ProcessWorkerPool(workerCount: 2);

        var tick = Assert.Single(ticks);
        Assert.True(tick.Processed);
        var nativeCall = Assert.Single(nativeCalls);
        using var callParams = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("alpha", callParams.RootElement.GetProperty("name").GetString());

        using var secondDocument = JsonDocument.Parse(second.Body);
        var secondJobId = secondDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString();
        var getSecond = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{secondJobId}"));
        using var getSecondDocument = JsonDocument.Parse(getSecond.Body);
        Assert.Equal("queued", getSecondDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
    }

    [Fact]
    public void JobListReturnsReadOnlyServerSideSnapshot()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"activity-vm","iso_path":"D:\\iso\\activity.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));

        Assert.Equal(202, create.StatusCode);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal("job.list", root.GetProperty("operation").GetString());
        Assert.Equal(1, root.GetProperty("data").GetProperty("count").GetInt32());

        var job = root.GetProperty("data").GetProperty("jobs")[0];
        Assert.Equal("vm.create", job.GetProperty("operation").GetString());
        Assert.Equal("queued", job.GetProperty("status").GetString());
        Assert.Equal(1, job.GetProperty("attempt").GetInt32());
        Assert.True(job.TryGetProperty("created_at", out _));
        Assert.True(job.TryGetProperty("updated_at", out _));
    }

    [Fact]
    public void JobListSupportsBoundedPaginationMetadata()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        for (var index = 0; index < 3; index++)
        {
            var create = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/vms",
                $$"""{"name":"activity-vm-{{index}}","iso_path":"D:\\iso\\activity.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));

            Assert.Equal(202, create.StatusCode);
        }

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs?limit=2&offset=0"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(3, data.GetProperty("count").GetInt32());
        Assert.Equal(2, data.GetProperty("returned").GetInt32());
        Assert.Equal(2, data.GetProperty("limit").GetInt32());
        Assert.Equal(0, data.GetProperty("offset").GetInt32());
        Assert.Equal(2, data.GetProperty("next_offset").GetInt32());
        Assert.Equal(2, data.GetProperty("jobs").GetArrayLength());
        Assert.Equal(200, data.GetProperty("max_limit").GetInt32());
        Assert.Equal(500, data.GetProperty("retention").GetProperty("max_terminal_jobs").GetInt32());
        Assert.True(data.GetProperty("retention").GetProperty("active_jobs_preserved").GetBoolean());
    }

    [Theory]
    [InlineData("/api/v1/jobs?limit=0", "PCV_JOB_LIST_LIMIT_OUT_OF_RANGE")]
    [InlineData("/api/v1/jobs?limit=201", "PCV_JOB_LIST_LIMIT_OUT_OF_RANGE")]
    [InlineData("/api/v1/jobs?offset=-1", "PCV_JOB_LIST_OFFSET_OUT_OF_RANGE")]
    [InlineData("/api/v1/jobs?limit=two", "PCV_JOB_LIST_PAGE_INVALID")]
    public void JobListRejectsInvalidPagination(string path, string expectedCode)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("GET", path));

        Assert.Equal(400, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("job.list", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(expectedCode, document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void JobRetentionPrunesOldTerminalJobsButPreservesActiveJobs()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, "vm.create", """
            {"ok":true,"operation":"vm.create","data":{"name":"retained"},"error":null}
            """));
        string? oldestJobId = null;

        for (var index = 0; index < 503; index++)
        {
            var create = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/vms",
                $$"""{"name":"retention-vm-{{index}}","iso_path":"D:\\iso\\retention.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));
            using var createDocument = JsonDocument.Parse(create.Body);
            oldestJobId ??= createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString();

            processor.ProcessOneQueuedJob();
        }

        var activeCreate = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"retention-active","iso_path":"D:\\iso\\retention.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));
        Assert.Equal(202, activeCreate.StatusCode);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs?limit=200&offset=0"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(501, data.GetProperty("count").GetInt32());
        Assert.Equal(200, data.GetProperty("returned").GetInt32());
        Assert.Equal(3, data.GetProperty("retention").GetProperty("pruned_terminal_jobs").GetInt32());
        Assert.Contains(
            data.GetProperty("jobs").EnumerateArray(),
            job => job.GetProperty("status").GetString() == "queued" &&
                job.GetProperty("operation").GetString() == "vm.create");

        var oldestGet = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{oldestJobId}"));
        Assert.Equal(404, oldestGet.StatusCode);
    }

    [Fact]
    public void JobRetentionPrunesPersistedOldTerminalJobsOnLoad()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-retention-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var baseTime = DateTimeOffset.Parse("2026-05-07T00:00:00Z");
            var persistedJobs = Enumerable.Range(0, 503)
                .Select(index => new SortedDictionary<string, object?>
                {
                    ["attempt"] = 1,
                    ["created_at"] = baseTime.AddMinutes(index).ToString("O"),
                    ["error"] = null,
                    ["job_id"] = $"terminal-{index:000}",
                    ["operation"] = "vm.create",
                    ["params"] = new SortedDictionary<string, object?>(),
                    ["result"] = new SortedDictionary<string, object?> { ["ok"] = true },
                    ["status"] = "succeeded",
                    ["updated_at"] = baseTime.AddMinutes(index).ToString("O")
                })
                .Cast<object?>()
                .Append(new SortedDictionary<string, object?>
                {
                    ["attempt"] = 1,
                    ["created_at"] = baseTime.AddHours(10).ToString("O"),
                    ["error"] = null,
                    ["job_id"] = "active-loaded",
                    ["operation"] = "vm.restart",
                    ["params"] = new SortedDictionary<string, object?>(),
                    ["result"] = null,
                    ["status"] = "queued",
                    ["updated_at"] = baseTime.AddHours(10).ToString("O")
                })
                .ToArray();
            File.WriteAllText(
                jobStorePath,
                JsonSerializer.Serialize(new SortedDictionary<string, object?>
                {
                    ["version"] = 2,
                    ["saved_at"] = baseTime.ToString("O"),
                    ["jobs"] = persistedJobs,
                    ["queue"] = new[] { "active-loaded" }
                }));

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);
            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs?limit=200&offset=0"));

            Assert.Equal(200, response.StatusCode);
            using var responseDocument = JsonDocument.Parse(response.Body);
            var data = responseDocument.RootElement.GetProperty("data");
            Assert.Equal(501, data.GetProperty("count").GetInt32());
            Assert.Equal(3, data.GetProperty("retention").GetProperty("pruned_terminal_jobs").GetInt32());
            Assert.Contains(
                data.GetProperty("jobs").EnumerateArray(),
                job => job.GetProperty("job_id").GetString() == "active-loaded" &&
                    job.GetProperty("status").GetString() == "queued");

            using var persistedDocument = JsonDocument.Parse(File.ReadAllText(jobStorePath));
            Assert.Equal(501, persistedDocument.RootElement.GetProperty("jobs").GetArrayLength());
            Assert.DoesNotContain(
                persistedDocument.RootElement.GetProperty("jobs").EnumerateArray(),
                job => job.GetProperty("job_id").GetString() == "terminal-000");
            Assert.Contains(
                persistedDocument.RootElement.GetProperty("jobs").EnumerateArray(),
                job => job.GetProperty("job_id").GetString() == "active-loaded");
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void OpsSummaryReturnsReadOnlyAggregateSnapshot()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            tokenStorage: "dpapi-local-machine",
            currentExposure: "loopback",
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["host.status"] = """
                {"ok":true,"operation":"host.status","data":{"supported":true,"admin":{"elevated":true},"hyperv":{"vmms_running":true,"feature_enabled":true,"default_switch_present":true}},"error":null}
                """,
                ["vm.list"] = """
                {"ok":true,"operation":"vm.list","data":[{"id":"alpha","name":"alpha","state":"running","checkpoints":{"count":11}},{"id":"beta","name":"beta","state":"off","checkpoints":{"count":0}}],"error":null}
                """
            }));

        processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms", """{"name":"queued","iso_path":"D:\\iso\\queued.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}""", RequestId: "req-summary-create"));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary", RequestId: "req-summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal("ops.summary", root.GetProperty("operation").GetString());
        Assert.Equal("req-summary", root.GetProperty("request_id").GetString());

        var data = root.GetProperty("data");
        Assert.True(data.GetProperty("host").GetProperty("supported").GetBoolean());
        Assert.Equal(2, data.GetProperty("vm_counts").GetProperty("total").GetInt32());
        Assert.Equal(1, data.GetProperty("vm_counts").GetProperty("running").GetInt32());
        Assert.Equal(1, data.GetProperty("vm_counts").GetProperty("checkpoint_warnings").GetInt32());
        Assert.Equal(1, data.GetProperty("job_counts").GetProperty("queued").GetInt32());
        Assert.Equal("dpapi-local-machine", data.GetProperty("runtime_policy").GetProperty("auth").GetProperty("token_storage").GetString());
        Assert.True(data.GetProperty("signals").GetArrayLength() >= 4);
        Assert.True(data.GetProperty("recent_activity").GetArrayLength() >= 1);
        var nextPackagePair = data
            .GetProperty("current_evidence")
            .GetProperty("manual_admin")
            .GetProperty("next_package_pair");
        Assert.Equal("0.42.56-admin-smoke -> 0.42.57-admin-smoke", nextPackagePair.GetProperty("package_pair").GetString());
        Assert.Equal("opened-public-boundary-current-evidence-rollup-payload", nextPackagePair.GetProperty("decision").GetString());
        Assert.Equal("docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md", nextPackagePair.GetProperty("evidence").GetString());
        var publicBoundary = data
            .GetProperty("current_evidence")
            .GetProperty("public_boundary")
            .GetProperty("latest_main_push");
        Assert.Equal("tracked-in-documentation", publicBoundary.GetProperty("status").GetString());
        Assert.Equal("26578120570", publicBoundary.GetProperty("run_id").GetString());
        Assert.Equal("78303066840", publicBoundary.GetProperty("job_id").GetString());
        Assert.Equal("7a7d5de822bdb058b04149eeeef0a7eb462828b5", publicBoundary.GetProperty("head_sha").GetString());
        Assert.Equal("docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md", publicBoundary.GetProperty("evidence").GetString());

        Assert.Contains("host.status", nativeCalls);
        Assert.Contains("vm.list", nativeCalls);
    }

    [Fact]
    public void OpsSummaryIncludesBatchEvidenceWhenRootIsConfigured()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-batch-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-212527-0384");
            Directory.CreateDirectory(batchRun);
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-212527-0384");
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260506-212527-0384",
              "artifact_root": "{{batchRun.Replace("\\", "\\\\")}}",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "retry_count": 1,
                  "attempt_count": 1,
                  "final_attempt": 1,
                  "duration_ms": 133356,
                  "stdout": "secret-token-value",
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "retry_count": 0,
                  "attempt_count": 1,
                  "final_attempt": 1,
                  "duration_ms": 11047,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
            {"schema_version":1,"ts":"2026-05-05T15:14:38.0000000Z","status":"collected","adapter_memory":[{"mib":3912.45}],"process_memory":[{"mib":1512.12}]}
            {"schema_version":1,"ts":"2026-05-05T15:14:43.0000000Z","status":"unavailable","adapter_memory":[],"process_memory":[],"error":"counter unavailable"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.38.4-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.38.4-admin-smoke"},"git_commit":"6bbb39f0a3a271e4a1187ce7de2014e009977425","msi":{"sha256":"7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7"},"signing_mode":"AllowUnsignedDev"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
            """);
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);
            var manualAdminRoot = Path.Combine(root, "manual-admin-campaign-20260516-04223-04224", "manual-admin-campaign-descriptor");
            Directory.CreateDirectory(manualAdminRoot);
            File.WriteAllText(Path.Combine(manualAdminRoot, "summary.json"), """
            {
              "schema_version": 1,
              "descriptor_schema_version": 2,
              "ok": true,
              "scope": "manual-admin-campaign-descriptor",
              "overall_status": "pass",
              "baseline_version": "0.42.23-admin-smoke",
              "target_version": "0.42.24-admin-smoke",
              "descriptor_batch_id": "manual-admin-campaign-descriptor-20260516-04223-04224-closed",
              "descriptor_contract_key": "manual-admin-descriptor-generation-contract-v2",
              "manual_admin_descriptor_generation_contract": "manual-admin-descriptor-generation-contract-v2",
              "runner_count": 6,
              "missing_count": 0,
              "not_pass_count": 0,
              "public_trusted_signing": "not-claimed",
              "external_stable_publication": "not-claimed"
            }
            """);
            var publicBoundaryRoot = Path.Combine(root, "public-boundary-ci-main-push-20260516-04224-scope-lock");
            Directory.CreateDirectory(publicBoundaryRoot);
            File.WriteAllText(Path.Combine(publicBoundaryRoot, "summary.json"), """
            {
              "schema_version": 1,
              "ok": true,
              "result": "PASS",
              "scope": "public-boundary-ci-required-main-push",
              "run_id": "25958540101",
              "job_id": "76312000001",
              "head_sha": "ef903f114829eb0e1dc6e42bcd429685d1783d30",
              "public_trusted_signing": "not-claimed",
              "external_stable_publication": "not-claimed"
            }
            """);
            var currentEvidencePath = Path.Combine(root, "current-evidence.json");
            File.WriteAllText(currentEvidencePath, """
            {
              "schema_version": 1,
              "contract": "pcv-current-evidence-v1",
              "current": { "version": "0.42.74-admin-smoke" },
              "feature_qualification": {
                "schema_version": 1,
                "contract": "pcv-feature-promotion-decision-v1",
                "promotion_eligible": false,
                "blockers": [
                  {
                    "feature_id": "pcv.vm.saved-lifecycle",
                    "stage": "actual_vm_tested",
                    "verdict": "fail"
                  }
                ]
              }
            }
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                tokenStorage: "windows-credential-manager",
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root,
                diagnosticBundleOptions: new DesktopNodeDiagnosticBundleOptions(DiagnosticsRoot: Path.Combine(root, "diagnostics")),
                currentEvidencePath: currentEvidencePath);

            File.WriteAllText(currentEvidencePath, """
            {
              "schema_version": 1,
              "contract": "pcv-current-evidence-v1",
              "current": { "version": "0.42.74-admin-smoke" },
              "feature_qualification": {
                "schema_version": 1,
                "contract": "pcv-feature-promotion-decision-v1",
                "promotion_eligible": true,
                "blockers": []
              }
            }
            """);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            Assert.Equal(1, evidence.GetProperty("schema_version").GetInt32());
            Assert.True(evidence.GetProperty("configured").GetBoolean());
            Assert.Equal("available", evidence.GetProperty("status").GetString());
            var latest = evidence.GetProperty("latest");
            Assert.Equal("full-admin-host-mutation-gate-20260506-212527-0384", latest.GetProperty("batch_id").GetString());
            Assert.Equal(2, latest.GetProperty("total_steps").GetInt32());
            Assert.Equal(2, latest.GetProperty("gpu_snapshots").GetProperty("count").GetInt32());
            Assert.Equal(1, latest.GetProperty("gpu_snapshots").GetProperty("status_counts").GetProperty("collected").GetInt32());
            Assert.Equal("0.38.4-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
            Assert.Equal("AllowUnsignedDev", latest.GetProperty("release").GetProperty("signing_mode").GetString());
            Assert.Equal("excluded", latest.GetProperty("release").GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", latest.GetProperty("release").GetProperty("external_stable_publication").GetString());
            Assert.Equal("Running", latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
            var installedRuntime = document.RootElement.GetProperty("data").GetProperty("installed_runtime");
            Assert.Equal("0.38.4-admin-smoke", installedRuntime.GetProperty("version").GetString());
            Assert.Equal("Running", installedRuntime.GetProperty("service_state").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260506-212527-0384", installedRuntime.GetProperty("evidence_anchor").GetString());
            Assert.Equal("available", installedRuntime.GetProperty("evidence_status").GetString());
            Assert.Equal("internal-only-not-public-release", installedRuntime.GetProperty("distribution_claim").GetString());
            Assert.Equal("excluded", installedRuntime.GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", installedRuntime.GetProperty("external_stable_publication").GetString());
            Assert.Equal("windows-credential-manager", installedRuntime.GetProperty("auth_boundary").GetProperty("token_storage").GetString());
            Assert.Equal("PCV_AUTH_REQUIRED", installedRuntime.GetProperty("auth_boundary").GetProperty("unauthenticated_api_error_code").GetString());
            Assert.Equal("configured", installedRuntime.GetProperty("diagnostics").GetProperty("root_status").GetString());
            Assert.Equal("configured-diagnostics-root", installedRuntime.GetProperty("diagnostics").GetProperty("runtime_policy_bundle_root").GetString());
            var currentEvidence = document.RootElement.GetProperty("data").GetProperty("current_evidence");
            Assert.Equal(1, currentEvidence.GetProperty("schema_version").GetInt32());
            Assert.Equal("runtime-api-current-evidence-rollup-v1", currentEvidence.GetProperty("contract_key").GetString());
            Assert.Equal("ops-summary", currentEvidence.GetProperty("source").GetString());
            var qualification = currentEvidence.GetProperty("feature_qualification");
            Assert.Equal("pcv-feature-promotion-decision-v1", qualification.GetProperty("contract").GetString());
            Assert.Equal("blocked", qualification.GetProperty("status").GetString());
            Assert.False(qualification.GetProperty("promotion_eligible").GetBoolean());
            var blocker = Assert.Single(qualification.GetProperty("blockers").EnumerateArray());
            Assert.Equal("pcv.vm.saved-lifecycle", blocker.GetProperty("feature_id").GetString());
            Assert.Equal("actual_vm_tested", blocker.GetProperty("stage").GetString());
            Assert.Equal("fail", blocker.GetProperty("verdict").GetString());
            Assert.Contains(
                document.RootElement.GetProperty("data").GetProperty("signals").EnumerateArray(),
                signal => signal.GetProperty("key").GetString() == "feature-promotion" &&
                          signal.GetProperty("tone").GetString() == "error" &&
                          signal.GetProperty("value").GetInt32() == 1);
            var publicBoundary = currentEvidence.GetProperty("public_boundary").GetProperty("latest_main_push");
            Assert.Equal("artifact-discovered", publicBoundary.GetProperty("status").GetString());
            Assert.Equal("batch_evidence_artifact", publicBoundary.GetProperty("source").GetString());
            Assert.Equal("25958540101", publicBoundary.GetProperty("run_id").GetString());
            Assert.Equal("76312000001", publicBoundary.GetProperty("job_id").GetString());
            Assert.Equal("ef903f114829eb0e1dc6e42bcd429685d1783d30", publicBoundary.GetProperty("head_sha").GetString());
            Assert.Equal("artifacts/public-boundary-ci-main-push-20260516-04224-scope-lock/summary.json", publicBoundary.GetProperty("evidence").GetString());
            Assert.Equal("not-claimed", publicBoundary.GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", publicBoundary.GetProperty("external_stable_publication").GetString());
            var fullAdmin = currentEvidence.GetProperty("full_admin_host_mutation").GetProperty("latest");
            Assert.Equal("available", fullAdmin.GetProperty("status").GetString());
            Assert.Equal("batch_evidence", fullAdmin.GetProperty("source").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260506-212527-0384", fullAdmin.GetProperty("batch_id").GetString());
            Assert.Equal("0.38.4-admin-smoke", fullAdmin.GetProperty("version").GetString());
            Assert.Equal("7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7", fullAdmin.GetProperty("msi_sha256").GetString());
            Assert.Equal("6bbb39f0a3a271e4a1187ce7de2014e009977425", fullAdmin.GetProperty("git_commit").GetString());
            Assert.Equal("AllowUnsignedDev", fullAdmin.GetProperty("signing_mode").GetString());
            Assert.Equal("Running", fullAdmin.GetProperty("service_state").GetString());
            Assert.Equal("excluded", fullAdmin.GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", fullAdmin.GetProperty("external_stable_publication").GetString());
            var hostOps = currentEvidence.GetProperty("host_ops").GetProperty("lifecycle_descriptor");
            Assert.Equal("contract-linked", hostOps.GetProperty("status").GetString());
            Assert.Equal("DesktopNode.Contracts.HostOpsLifecycleDescriptor", hostOps.GetProperty("source").GetString());
            Assert.Equal("host-ops-lifecycle-descriptor-bridge-v1", hostOps.GetProperty("contract_key").GetString());
            Assert.Equal(
                "service-action-eventlog-firewall-truststore-credential-manager-data-root-separated",
                hostOps.GetProperty("lifecycle_bucket_contract_key").GetString());
            Assert.False(hostOps.GetProperty("host_mutation_performed").GetBoolean());
            var hostOpsBuckets = hostOps.GetProperty("buckets").EnumerateArray().ToArray();
            Assert.Equal(["service-action", "event-log", "firewall", "trust-store", "credential-manager", "data-root"], hostOpsBuckets.Select(bucket => bucket.GetProperty("bucket_key").GetString()!).ToArray());
            Assert.Contains(hostOpsBuckets, bucket =>
                bucket.GetProperty("bucket_key").GetString() == "service-action" &&
                bucket.GetProperty("operation_family").GetString() == "service-lifecycle" &&
                bucket.GetProperty("mutation_boundary").GetString() == "windows-service-control-manager");
            Assert.Contains(hostOpsBuckets, bucket =>
                bucket.GetProperty("bucket_key").GetString() == "credential-manager" &&
                bucket.GetProperty("operation_family").GetString() == "credential-manager" &&
                bucket.GetProperty("operations").EnumerateArray().Any(operation => operation.GetString() == "credential-manager-system-proof"));
            var packagePair = currentEvidence.GetProperty("manual_admin").GetProperty("latest_package_pair");
            Assert.Equal("artifact-discovered", packagePair.GetProperty("status").GetString());
            Assert.Equal("batch_evidence_artifact", packagePair.GetProperty("source").GetString());
            Assert.Equal("0.42.23-admin-smoke", packagePair.GetProperty("baseline_version").GetString());
            Assert.Equal("0.42.24-admin-smoke", packagePair.GetProperty("target_version").GetString());
            Assert.Equal("0.42.23-admin-smoke -> 0.42.24-admin-smoke", packagePair.GetProperty("package_pair").GetString());
            Assert.Equal("manual-admin-campaign-descriptor-20260516-04223-04224-closed", packagePair.GetProperty("current_card_descriptor_batch_id").GetString());
            Assert.Equal("manual-admin-campaign-descriptor-20260516-04223-04224-closed", packagePair.GetProperty("descriptor_batch_id").GetString());
            Assert.Equal("artifacts/manual-admin-campaign-20260516-04223-04224/manual-admin-campaign-descriptor/summary.json", packagePair.GetProperty("evidence").GetString());
            Assert.Equal("artifacts/manual-admin-campaign-20260516-04223-04224/manual-admin-campaign-descriptor/summary.json", packagePair.GetProperty("descriptor_summary").GetString());
            Assert.Equal("manual-admin-campaign-descriptor-summary", packagePair.GetProperty("descriptor_source").GetString());
            Assert.Equal("manual-admin-descriptor-generation-contract-v2", packagePair.GetProperty("descriptor_contract_key").GetString());
            Assert.Equal("manual-admin-descriptor-generation-contract-v2", packagePair.GetProperty("descriptor_generation_contract").GetString());
            Assert.Equal("pass", packagePair.GetProperty("descriptor_overall_status").GetString());
            Assert.Equal(2, packagePair.GetProperty("descriptor_schema_version").GetInt32());
            Assert.Equal(6, packagePair.GetProperty("runner_count").GetInt32());
            Assert.Equal(0, packagePair.GetProperty("missing_count").GetInt32());
            Assert.Equal(0, packagePair.GetProperty("not_pass_count").GetInt32());
            Assert.DoesNotContain("secret-token-value", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryKeepsRunningWithUnavailableCurrentEvidenceWithoutLeakingPath()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-api-missing-" + Guid.NewGuid().ToString("N"));
        var missingPath = Path.Combine(root, "secret-current-evidence.json");
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
            }),
            currentEvidencePath: missingPath);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        var qualification = data.GetProperty("current_evidence").GetProperty("feature_qualification");
        Assert.Equal("unavailable", qualification.GetProperty("status").GetString());
        Assert.False(qualification.GetProperty("promotion_eligible").GetBoolean());
        Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", qualification.GetProperty("error_code").GetString());
        Assert.Empty(qualification.GetProperty("blockers").EnumerateArray());
        Assert.Contains(
            data.GetProperty("signals").EnumerateArray(),
            signal => signal.GetProperty("key").GetString() == "feature-promotion" &&
                      signal.GetProperty("tone").GetString() == "error" &&
                      signal.GetProperty("value").GetString() == "unavailable");
        Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-current-evidence.json", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OpsSummaryProjectsEligibleCurrentEvidenceAndOrdersFeatureSignalAfterBatchEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-api-eligible-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            var currentEvidencePath = Path.Combine(root, "current-evidence.json");
            File.WriteAllText(currentEvidencePath, """
            {
              "schema_version": 1,
              "contract": "pcv-current-evidence-v1",
              "current": { "version": "0.42.74-admin-smoke" },
              "feature_qualification": {
                "schema_version": 1,
                "contract": "pcv-feature-promotion-decision-v1",
                "promotion_eligible": true,
                "blockers": []
              }
            }
            """);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                currentEvidencePath: currentEvidencePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            var qualification = data.GetProperty("current_evidence").GetProperty("feature_qualification");
            Assert.Equal(1, qualification.GetProperty("schema_version").GetInt32());
            Assert.Equal("pcv-feature-promotion-decision-v1", qualification.GetProperty("contract").GetString());
            Assert.Equal("eligible", qualification.GetProperty("status").GetString());
            Assert.True(qualification.GetProperty("promotion_eligible").GetBoolean());
            Assert.Empty(qualification.GetProperty("blockers").EnumerateArray());
            Assert.False(qualification.TryGetProperty("error_code", out _));

            var signals = data.GetProperty("signals").EnumerateArray().ToArray();
            var batchSignalIndex = Array.FindIndex(
                signals,
                signal => signal.GetProperty("key").GetString() == "batch-evidence");
            var featureSignalIndex = Array.FindIndex(
                signals,
                signal => signal.GetProperty("key").GetString() == "feature-promotion");
            Assert.True(batchSignalIndex >= 0);
            Assert.Equal(batchSignalIndex + 1, featureSignalIndex);
            var featureSignal = signals[featureSignalIndex];
            Assert.Equal("Feature promotion", featureSignal.GetProperty("label").GetString());
            Assert.Equal("ok", featureSignal.GetProperty("tone").GetString());
            Assert.Equal(JsonValueKind.Number, featureSignal.GetProperty("value").ValueKind);
            Assert.Equal(0, featureSignal.GetProperty("value").GetInt32());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryResolvesRepoRootRedactedBatchChildEvidenceWithinConfiguredRoot()
    {
        var repoRoot = Path.Combine(Path.GetTempPath(), "pcv-redacted-repo-root-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(repoRoot, "artifacts");
        try
        {
            var batchRun = Path.Combine(root, "batch-runs", "full-admin-host-mutation-gate-20260506-212527-0384");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-212527-0384");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);
            var routeRedactedPath = ToRepoRootRedactedPath(routeRoot, repoRoot);
            var osRedactedPath = ToRepoRootRedactedPath(osRoot, repoRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260506-212527-0384",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "retry_count": 0,
                  "attempt_count": 1,
                  "final_attempt": 1,
                  "duration_ms": 133356,
                  "stdout": "ArtifactRoot={{routeRedactedPath.Replace("\\", "\\\\")}}",
                  "arguments": ["-ArtifactRoot", "{{routeRedactedPath.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "retry_count": 0,
                  "attempt_count": 1,
                  "final_attempt": 1,
                  "duration_ms": 11047,
                  "stdout": "ArtifactRoot={{osRedactedPath.Replace("\\", "\\\\")}}",
                  "arguments": ["-ArtifactRoot", "{{osRedactedPath.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.38.4-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.38.4-admin-smoke"},"git_commit":"6bbb39f0a3a271e4a1187ce7de2014e009977425","msi":{"sha256":"7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7"},"signing_mode":"AllowUnsignedDev"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
            """);
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var latest = document.RootElement.GetProperty("data").GetProperty("batch_evidence").GetProperty("latest");
            Assert.Equal("full-admin-host-mutation-gate-20260506-212527-0384", latest.GetProperty("batch_id").GetString());
            Assert.Equal("0.38.4-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
            Assert.Equal("AllowUnsignedDev", latest.GetProperty("release").GetProperty("signing_mode").GetString());
            Assert.Equal("excluded", latest.GetProperty("release").GetProperty("public_trusted_signing").GetString());
            Assert.Equal("not-claimed", latest.GetProperty("release").GetProperty("external_stable_publication").GetString());
            Assert.True(latest.GetProperty("route_msi_hyperv").GetProperty("ok").GetBoolean());
            Assert.True(latest.GetProperty("route_msi_hyperv").GetProperty("msi_lifecycle_ok").GetBoolean());
            Assert.Equal(2, latest.GetProperty("route_msi_hyperv").GetProperty("msi_lifecycle_step_count").GetInt32());
            Assert.True(latest.GetProperty("os_mutation").GetProperty("ok").GetBoolean());
            Assert.Equal(0, latest.GetProperty("os_mutation").GetProperty("firewall_rule_count").GetInt32());
            Assert.Equal("Running", latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
            Assert.True(latest.GetProperty("host_final_state").GetProperty("trust_root_present").GetBoolean());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryReportsMissingBatchEvidenceRootWithoutFailing()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-missing-batch-evidence-" + Guid.NewGuid().ToString("N"));
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
            }),
            batchEvidenceRoot: root);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
        Assert.True(evidence.GetProperty("configured").GetBoolean());
        Assert.Equal("missing", evidence.GetProperty("status").GetString());
        Assert.Equal("PCV_BATCH_EVIDENCE_ROOT_MISSING", evidence.GetProperty("errors")[0].GetProperty("code").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("data").GetProperty("signals").EnumerateArray(),
            signal => signal.GetProperty("key").GetString() == "batch-evidence" &&
                      signal.GetProperty("tone").GetString() == "warn");
    }

    [Fact]
    public void OpsSummaryReportsMalformedBatchEvidenceWithoutLeakingPaths()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-malformed-batch-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "broken-run");
            Directory.CreateDirectory(batchRun);
            File.WriteAllText(Path.Combine(batchRun, "summary.json"), "{not-json");
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            Assert.Equal("unavailable", evidence.GetProperty("status").GetString());
            Assert.Equal("PCV_BATCH_EVIDENCE_PARSE_FAILED", evidence.GetProperty("errors")[0].GetProperty("code").GetString());
            Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryReportsDegradedBatchEvidenceForMissingChildArtifacts()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-missing-child-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-212527-0384");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-212527-0384");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260506-212527-0384",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            var evidence = data.GetProperty("batch_evidence");
            Assert.Equal("degraded", evidence.GetProperty("status").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260506-212527-0384", evidence.GetProperty("latest").GetProperty("batch_id").GetString());
            Assert.Equal("missing", evidence.GetProperty("latest").GetProperty("route_msi_hyperv").GetProperty("status").GetString());
            Assert.Equal("missing", evidence.GetProperty("latest").GetProperty("os_mutation").GetProperty("status").GetString());
            Assert.Contains(
                evidence.GetProperty("errors").EnumerateArray(),
                error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_MISSING");
            Assert.Contains(
                evidence.GetProperty("errors").EnumerateArray(),
                error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_OS_SUMMARY_MISSING");
            Assert.Contains(
                data.GetProperty("signals").EnumerateArray(),
                signal => signal.GetProperty("key").GetString() == "batch-evidence" &&
                          signal.GetProperty("tone").GetString() == "warn" &&
                          signal.GetProperty("value").GetString() == "degraded");
            Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryReportsDegradedBatchEvidenceForMalformedChildArtifacts()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-malformed-child-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-212527-0384");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-212527-0384");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260506-212527-0384",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "duration_ms": 133356,
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "duration_ms": 11047,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), "{not-json");
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            var latest = evidence.GetProperty("latest");
            Assert.Equal("degraded", evidence.GetProperty("status").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260506-212527-0384", latest.GetProperty("batch_id").GetString());
            Assert.Equal("unavailable", latest.GetProperty("route_msi_hyperv").GetProperty("status").GetString());
            Assert.Equal("available", latest.GetProperty("os_mutation").GetProperty("status").GetString());
            Assert.Equal("0.38.4-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
            Assert.Contains(
                evidence.GetProperty("errors").EnumerateArray(),
                error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_PARSE_FAILED");
            Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryReportsDegradedBatchEvidenceForMalformedGpuSnapshots()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-gpu-snapshots-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-212527-0384");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-212527-0384");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260506-212527-0384",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {"step_id":"service-msi-hyperv-admin-smoke","ok":true,"arguments":["-ArtifactRoot","{{routeRoot.Replace("\\", "\\\\")}}"]},
                {"step_id":"os-mutation-gate","ok":true,"arguments":["-ArtifactRoot","{{osRoot.Replace("\\", "\\\\")}}"]}
              ]
            }
            """);
            File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
            {"schema_version":1,"status":"collected","adapter_memory":[{"mib":3912.45}],"process_memory":[{"mib":1512.12}]}
            {not-json
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.38.4-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.38.4-admin-smoke"},"git_commit":"6bbb39f0a3a271e4a1187ce7de2014e009977425","msi":{"sha256":"7aa36d92d5c69448726e4141e1311be7f0cf791df9265fc1c1c887b2212114f7"},"signing_mode":"AllowUnsignedDev"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
            """);
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.38.4-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            var gpuSnapshots = evidence.GetProperty("latest").GetProperty("gpu_snapshots");
            Assert.Equal("degraded", evidence.GetProperty("status").GetString());
            Assert.Equal("unavailable", gpuSnapshots.GetProperty("status").GetString());
            Assert.Equal(1, gpuSnapshots.GetProperty("count").GetInt32());
            Assert.Contains(
                evidence.GetProperty("errors").EnumerateArray(),
                error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED");
            Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryRedactsBatchEvidenceSensitiveMaterialAcrossDegradedErrors()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-redaction-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-212527-0384");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-212527-0384");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(routeRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260506-212527-0384",
              "total_steps": 1,
              "executed_steps": 1,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["--api-token", "secret-token-value", "-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"],
                  "stdout": "ArtifactRoot={{routeRoot.Replace("\\", "\\\\")}}\nAuthorization: Bearer secret-bearer-value",
                  "stderr": "PCV_API_TOKEN=secret-env-token ApiTokenProtectedFile=C:\\\\secret\\\\token.txt"
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), "{not-json");

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            Assert.Equal("degraded", evidence.GetProperty("status").GetString());
            Assert.Contains(
                evidence.GetProperty("errors").EnumerateArray(),
                error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_PARSE_FAILED");
            Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret-token-value", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret-bearer-value", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret-env-token", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ApiTokenProtectedFile", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("stdout", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("stderr", response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("arguments", response.Body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryReportsNewestMalformedBatchEvidenceInsteadOfOlderAvailableRun()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-newest-malformed-batch-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var oldRun = Path.Combine(root, "old-run");
            var newRun = Path.Combine(root, "new-run");
            Directory.CreateDirectory(oldRun);
            Directory.CreateDirectory(newRun);
            var oldSummary = Path.Combine(oldRun, "summary.json");
            var newSummary = Path.Combine(newRun, "summary.json");
            File.WriteAllText(oldSummary, """
            {"schema_version":1,"ok":true,"status":"completed","batch_id":"old-run","total_steps":0,"executed_steps":0,"results":[]}
            """);
            File.WriteAllText(newSummary, "{not-json");
            File.SetLastWriteTimeUtc(oldSummary, DateTime.UtcNow.AddMinutes(-10));
            File.SetLastWriteTimeUtc(newSummary, DateTime.UtcNow);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
            Assert.Equal("unavailable", evidence.GetProperty("status").GetString());
            Assert.Equal("PCV_BATCH_EVIDENCE_PARSE_FAILED", evidence.GetProperty("errors")[0].GetProperty("code").GetString());
            Assert.Equal(JsonValueKind.Null, evidence.GetProperty("latest").ValueKind);
            Assert.DoesNotContain("old-run", response.Body, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummarySelectsLatestManualAdminDescriptorByPackagePairInsteadOfFileTime()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-manual-admin-latest-descriptor-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "batch-runs", "full-admin-host-mutation-gate-20260517-04228");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260517-04228");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260517-04228");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260517-04228",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
            {"schema_version":1,"status":"collected","adapter_memory":[{"mib":4501.93}],"process_memory":[{"mib":1512.12}]}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.42.28-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.42.28-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.42.28-admin-smoke"},"git_commit":"b9676f6dc37d667ae0d60367e9f4e576a27e3864","msi":{"sha256":"223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e"},"signing_mode":"AllowUnsignedDev"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
            """);
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.42.28-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);

            var latestDescriptor = Path.Combine(root, "manual-admin-campaign-20260517-04227-04228", "manual-admin-campaign-descriptor", "summary.json");
            var historicalDescriptor = Path.Combine(root, "manual-admin-campaign-20260516-04222-04223", "manual-admin-campaign-descriptor", "summary.json");
            Directory.CreateDirectory(Path.GetDirectoryName(latestDescriptor)!);
            Directory.CreateDirectory(Path.GetDirectoryName(historicalDescriptor)!);
            File.WriteAllText(latestDescriptor, """
            {
              "schema_version": 1,
              "descriptor_schema_version": 2,
              "ok": true,
              "scope": "manual-admin-campaign-descriptor",
              "overall_status": "pass",
              "baseline_version": "0.42.27-admin-smoke",
              "target_version": "0.42.28-admin-smoke",
              "descriptor_batch_id": "manual-admin-campaign-descriptor-20260517-04227-04228-closed",
              "descriptor_contract_key": "manual-admin-descriptor-generation-contract-v2",
              "manual_admin_descriptor_generation_contract": "manual-admin-descriptor-generation-contract-v2",
              "runner_count": 6,
              "missing_count": 0,
              "not_pass_count": 0,
              "public_trusted_signing": "not-claimed",
              "external_stable_publication": "not-claimed"
            }
            """);
            File.WriteAllText(historicalDescriptor, """
            {
              "schema_version": 1,
              "descriptor_schema_version": 2,
              "ok": true,
              "scope": "manual-admin-campaign-descriptor",
              "overall_status": "pass",
              "baseline_version": "0.42.22-admin-smoke",
              "target_version": "0.42.23-admin-smoke",
              "descriptor_batch_id": "manual-admin-campaign-descriptor-20260516-04222-04223-closed",
              "descriptor_contract_key": "manual-admin-descriptor-generation-contract-v2",
              "manual_admin_descriptor_generation_contract": "manual-admin-descriptor-generation-contract-v2",
              "runner_count": 6,
              "missing_count": 0,
              "not_pass_count": 0,
              "public_trusted_signing": "not-claimed",
              "external_stable_publication": "not-claimed"
            }
            """);
            File.SetLastWriteTimeUtc(latestDescriptor, DateTime.UtcNow.AddHours(-1));
            File.SetLastWriteTimeUtc(historicalDescriptor, DateTime.UtcNow);

            var evidence = new BatchEvidenceSummaryReader(root).Read();

            var packagePair = evidence.GetProperty("manual_admin").GetProperty("latest_package_pair");
            Assert.Equal("0.42.27-admin-smoke", packagePair.GetProperty("baseline_version").GetString());
            Assert.Equal("0.42.28-admin-smoke", packagePair.GetProperty("target_version").GetString());
            Assert.Equal("0.42.27-admin-smoke -> 0.42.28-admin-smoke", packagePair.GetProperty("package_pair").GetString());
            Assert.Equal("manual-admin-campaign-descriptor-20260517-04227-04228-closed", packagePair.GetProperty("current_card_descriptor_batch_id").GetString());
            Assert.Equal("artifacts/manual-admin-campaign-20260517-04227-04228/manual-admin-campaign-descriptor/summary.json", packagePair.GetProperty("evidence").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummarySkipsManualAdminDescriptorWhenSelectingLatestOperationalEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-skip-manual-admin-descriptor-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "batch-runs", "full-admin-host-mutation-gate-20260514-140126-04212-explicit");
            var descriptorRun = Path.Combine(root, "batch-runs", "manual-admin-campaign-descriptor-20260514-04212-04213");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260514-140126-04212-explicit");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(descriptorRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            var batchSummary = Path.Combine(batchRun, "summary.json");
            var descriptorSummary = Path.Combine(descriptorRun, "summary.json");
            File.WriteAllText(batchSummary, $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260514-140126-04212-explicit",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
            {"schema_version":1,"status":"collected","adapter_memory":[{"mib":4501.93}],"process_memory":[{"mib":1512.12}]}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.42.12-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.42.12-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.42.12-admin-smoke"},"git_commit":"d338b8a99f3e1e3839ac89a6de0da034ff3da148","msi":{"sha256":"269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db"},"signing_mode":"AllowUnsignedDev"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
            """);
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.42.12-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);
            File.WriteAllText(descriptorSummary, """
            {"schema_version":1,"ok":true,"status":"completed","batch_id":"manual-admin-campaign-descriptor-20260514-04212-04213","total_steps":1,"executed_steps":1,"results":{"step_id":"manual-admin-campaign-descriptor","ok":true}}
            """);
            File.SetLastWriteTimeUtc(batchSummary, DateTime.UtcNow.AddMinutes(-10));
            File.SetLastWriteTimeUtc(descriptorSummary, DateTime.UtcNow);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            var evidence = data.GetProperty("batch_evidence");
            var latest = evidence.GetProperty("latest");
            Assert.Equal("available", evidence.GetProperty("status").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260514-140126-04212-explicit", latest.GetProperty("batch_id").GetString());
            Assert.Equal("0.42.12-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
            Assert.Equal("Running", latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260514-140126-04212-explicit", data.GetProperty("installed_runtime").GetProperty("evidence_anchor").GetString());
            var runtimeRegistryBridge = data
                .GetProperty("installed_runtime")
                .GetProperty("diagnostics")
                .GetProperty("runtime_api_registry_bridge");
            Assert.Equal("runtime-api-diagnostics-ops-summary-registry-bridge-v2", runtimeRegistryBridge.GetProperty("contract_key").GetString());
            Assert.Equal("DesktopNodeApiRuntimeRoutes", runtimeRegistryBridge.GetProperty("handler_registry_source").GetString());
            Assert.Equal(
                "docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md#runtime-api-diagnostics-ops-summary",
                runtimeRegistryBridge.GetProperty("documentation_anchor").GetString());
            Assert.Contains(
                "GET /api/v1/ops/summary -> OpsSummary [runtime-api-ops-summary-current-card]",
                runtimeRegistryBridge.GetProperty("route_keys").EnumerateArray().Select(route => route.GetString()));
            Assert.DoesNotContain("manual-admin-campaign-descriptor-20260514-04212-04213", response.Body, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummarySkipsCurrentCardCaptureWhenSelectingLatestOperationalEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-skip-current-card-evidence-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "batch-runs", "full-admin-host-mutation-gate-20260516-04225");
            var currentCardRun = Path.Combine(root, "installed-operator-surface-current-card-20260516-04225");
            var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260516-04225");
            var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260516-04225");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(currentCardRun);
            Directory.CreateDirectory(routeRoot);
            Directory.CreateDirectory(osRoot);

            var batchSummary = Path.Combine(batchRun, "summary.json");
            var currentCardSummary = Path.Combine(currentCardRun, "summary.json");
            File.WriteAllText(batchSummary, $$"""
            {
              "schema_version": 1,
              "ok": true,
              "status": "completed",
              "batch_id": "full-admin-host-mutation-gate-20260516-04225",
              "total_steps": 2,
              "executed_steps": 2,
              "results": [
                {
                  "step_id": "service-msi-hyperv-admin-smoke",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
                },
                {
                  "step_id": "os-mutation-gate",
                  "ok": true,
                  "exit_code": 0,
                  "timed_out": false,
                  "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
                }
              ]
            }
            """);
            File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
            {"schema_version":1,"status":"collected","adapter_memory":[{"mib":4501.93}],"process_memory":[{"mib":1512.12}]}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.42.25-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.42.25-admin-smoke-windows-x64.provenance.json"), """
            {"schema_version":"1","product":{"version":"0.42.25-admin-smoke"},"git_commit":"4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1","msi":{"sha256":"e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b"},"signing_mode":"AllowUnsignedDev"}
            """);
            File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
            {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
            """);
            File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"0.42.25-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
            """);
            File.WriteAllText(currentCardSummary, """
            {"schema_version":1,"ok":true,"artifact_root":"artifacts/installed-operator-surface-current-card-20260516-04225","version":"0.42.25-admin-smoke","batch_id":"full-admin-host-mutation-gate-20260516-04225","batch_status":"available","web":{"ok":true},"cli":{"ok":true},"tui":{"ok":true}}
            """);
            File.SetLastWriteTimeUtc(batchSummary, DateTime.UtcNow.AddMinutes(-10));
            File.SetLastWriteTimeUtc(currentCardSummary, DateTime.UtcNow);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            var evidence = data.GetProperty("batch_evidence");
            var latest = evidence.GetProperty("latest");
            Assert.Equal("available", evidence.GetProperty("status").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260516-04225", latest.GetProperty("batch_id").GetString());
            Assert.Equal("0.42.25-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
            Assert.Equal("Running", latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
            Assert.Equal("full-admin-host-mutation-gate-20260516-04225", data.GetProperty("installed_runtime").GetProperty("evidence_anchor").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void OpsSummaryDoesNotReadChildEvidenceThroughDirectorySymlink()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-batch-evidence-symlink-root-" + Guid.NewGuid().ToString("N"));
        var outside = Path.Combine(Path.GetTempPath(), "pcv-batch-evidence-symlink-outside-" + Guid.NewGuid().ToString("N"));
        try
        {
            var batchRun = Path.Combine(root, "run");
            var linkPath = Path.Combine(root, "route-link");
            Directory.CreateDirectory(batchRun);
            Directory.CreateDirectory(outside);
            File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
            {"schema_version":1,"ok":true,"status":"completed","batch_id":"run","total_steps":1,"executed_steps":1,"results":[{"step_id":"service-msi-hyperv-admin-smoke","ok":true,"arguments":["-ArtifactRoot","{{linkPath.Replace("\\", "\\\\")}}"]}]}
            """);
            File.WriteAllText(Path.Combine(outside, "summary.json"), """
            {"schema_version":1,"ok":true,"version":"outside-version-that-must-not-be-read","final_service":{"state":"OutsideService"}}
            """);

            try
            {
                Directory.CreateSymbolicLink(linkPath, outside);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                return;
            }

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
                {
                    ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                    ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
                }),
                batchEvidenceRoot: root);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            var latest = document.RootElement.GetProperty("data").GetProperty("batch_evidence").GetProperty("latest");
            Assert.Null(latest.GetProperty("release").GetProperty("version").GetString());
            Assert.Null(latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
            Assert.DoesNotContain("outside-version-that-must-not-be-read", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("OutsideService", response.Body, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }

            if (Directory.Exists(outside))
            {
                Directory.Delete(outside, recursive: true);
            }
        }
    }

    [Fact]
    public void BatchEvidenceReadGuardRejectsReparsePointSegments()
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "pcv-batch-evidence-guard-root-" + Guid.NewGuid().ToString("N")));
        var child = Path.Combine(root, "child");
        var summary = Path.Combine(child, "summary.json");
        var fileAccess = new RecordingBatchEvidenceFileAccess
        {
            GetAttributesHandler = path =>
                string.Equals(path, child, StringComparison.OrdinalIgnoreCase)
                    ? FileAttributes.Directory | FileAttributes.ReparsePoint
                    : FileAttributes.Directory
        };

        var allowed = BatchEvidenceSummaryReader.IsPathWithinConfiguredRootWithoutReparsePoints(
            root,
            summary,
            fileAccess.GetAttributes);

        Assert.False(allowed);
        Assert.Equal(
            [
                $"GetAttributes:{root}",
                $"GetAttributes:{child}"
            ],
            fileAccess.Calls);
    }

    [Fact]
    public void BatchEvidenceSortTimePlacesUnreadableSummaryLast()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-batch-evidence-sort-" + Guid.NewGuid().ToString("N"));
        var unreadableRunRoot = Path.Combine(root, "unreadable-run");
        var summary = Path.Combine(unreadableRunRoot, "summary.json");
        var fileAccess = new RecordingBatchEvidenceFileAccess
        {
            FileExistsHandler = path => string.Equals(path, summary, StringComparison.OrdinalIgnoreCase),
            GetAttributesHandler = path => string.Equals(path, summary, StringComparison.OrdinalIgnoreCase)
                ? throw new IOException("recorded path metadata failure")
                : FileAttributes.Directory
        };
        var reader = new BatchEvidenceSummaryReader(root, fileAccess);

        var sortTime = reader.GetEvidenceSummarySortTime(unreadableRunRoot);

        Assert.Equal(DateTime.MinValue, sortTime);
        Assert.Equal(
            [
                $"FileExists:{summary}",
                $"GetAttributes:{root}",
                $"GetAttributes:{unreadableRunRoot}",
                $"GetAttributes:{summary}"
            ],
            fileAccess.Calls);
    }

    [Fact]
    public void BatchEvidenceMetadataReadFailureReturnsUnavailableWithoutReadingSummary()
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "pcv-batch-evidence-metadata-" + Guid.NewGuid().ToString("N")));
        var runRoot = Path.Combine(root, "run");
        var olderRunRoot = Path.Combine(root, "older-run");
        var summary = Path.Combine(runRoot, "summary.json");
        var olderSummary = Path.Combine(olderRunRoot, "summary.json");
        var fileAccess = new RecordingBatchEvidenceFileAccess
        {
            DirectoryExistsHandler = path =>
                string.Equals(path, root, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, runRoot, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, olderRunRoot, StringComparison.OrdinalIgnoreCase),
            FileExistsHandler = path =>
                string.Equals(path, summary, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, olderSummary, StringComparison.OrdinalIgnoreCase),
            GetAttributesHandler = path =>
                string.Equals(path, summary, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, olderSummary, StringComparison.OrdinalIgnoreCase)
                ? FileAttributes.Normal
                : FileAttributes.Directory,
            GetDirectoriesHandler = path => string.Equals(path, root, StringComparison.OrdinalIgnoreCase)
                ? [runRoot, olderRunRoot]
                : [],
            GetLastWriteTimeUtcHandler = _ => throw new IOException("recorded metadata failure")
        };
        var reader = new BatchEvidenceSummaryReader(root, fileAccess);

        var evidence = reader.Read();

        Assert.Equal("unavailable", evidence.GetProperty("status").GetString());
        Assert.Equal(
            "PCV_BATCH_EVIDENCE_READ_FAILED",
            evidence.GetProperty("errors")[0].GetProperty("code").GetString());
        Assert.Contains(
            fileAccess.Calls,
            call => call.StartsWith("GetLastWriteTimeUtc:", StringComparison.Ordinal));
        Assert.DoesNotContain(
            fileAccess.Calls,
            call => call.StartsWith("ReadAllText:", StringComparison.Ordinal));
    }

    [Fact]
    public void OpsSummaryDegradesNativeFailuresWithoutFailingTheSummaryRoute()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal("ops.summary", root.GetProperty("operation").GetString());

        var data = root.GetProperty("data");
        Assert.Equal(0, data.GetProperty("vm_counts").GetProperty("total").GetInt32());
        Assert.True(data.GetProperty("errors").GetArrayLength() >= 2);
        Assert.Contains(
            data.GetProperty("errors").EnumerateArray(),
            error => error.GetProperty("operation").GetString() == "host.status" &&
                     error.GetProperty("code").GetString() == "PCV_NATIVE_ROUTE_NOT_HANDLED");
        Assert.Contains(
            data.GetProperty("signals").EnumerateArray(),
            signal => signal.GetProperty("key").GetString() == "host-readiness" &&
                      signal.GetProperty("tone").GetString() == "warn");
    }

    [Fact]
    public void JobStoreLoadsQueuedLifecycleJobsAndWorkerDispatchesToNativeAfterRestart()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "saved_at": "2026-05-01T00:00:00.0000000Z",
              "jobs": [
                {
                  "job_id": "job-persisted",
                  "operation": "vm.shutdown",
                  "status": "queued",
                  "params": { "name": "persisted-vm" },
                  "result": null,
                  "error": null,
                  "retry_of": null,
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-05-01T00:00:00.0000000Z",
                  "updated_at": "2026-05-01T00:00:00.0000000Z"
                }
              ],
              "queue": [ "job-persisted" ]
            }
            """);
            var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
            var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVPowerStateAdapter(nativeCalls),
                jobStorePath: jobStorePath);

            var get = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs/job-persisted"));
            using var getDocument = JsonDocument.Parse(get.Body);
            Assert.Equal("queued", getDocument.RootElement.GetProperty("data").GetProperty("status").GetString());

            var tick = processor.ProcessOneQueuedJob();

            Assert.True(tick.Processed);
            Assert.Empty(fallbackCalls);
            var nativeCall = Assert.Single(nativeCalls);
            Assert.Equal("vm.shutdown", nativeCall.Operation);
            Assert.Equal("persisted-vm", JsonDocument.Parse(nativeCall.ParamsJson).RootElement.GetProperty("name").GetString());
            Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        }
        finally
        {
            File.Delete(jobStorePath);
        }
    }

    [Fact]
    public void JobStoreLoadsQueuedVmPowerStateJobsAndWorkerDispatchesToNativeAfterRestart()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-native-power-state-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "saved_at": "2026-05-03T00:00:00.0000000Z",
              "jobs": [
                {
                  "job_id": "job-persisted-native",
                  "operation": "vm.start",
                  "status": "queued",
                  "params": { "name": "persisted-vm" },
                  "result": null,
                  "error": null,
                  "retry_of": null,
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-05-03T00:00:00.0000000Z",
                  "updated_at": "2026-05-03T00:00:00.0000000Z"
                }
              ],
              "queue": [ "job-persisted-native" ]
            }
            """);
            var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
            var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVPowerStateAdapter(nativeCalls),
                jobStorePath: jobStorePath);

            var tick = processor.ProcessOneQueuedJob();

            Assert.True(tick.Processed);
            Assert.Empty(fallbackCalls);
            var nativeCall = Assert.Single(nativeCalls);
            Assert.Equal("vm.start", nativeCall.Operation);
            Assert.Equal("persisted-vm", JsonDocument.Parse(nativeCall.ParamsJson).RootElement.GetProperty("name").GetString());
            Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        }
        finally
        {
            File.Delete(jobStorePath);
        }
    }

    [Fact]
    public void JobStoreMarksPersistedRunningJobsInterrupted()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-running-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-running",
                  "operation": "vm.create",
                  "status": "running",
                  "params": { "name": "interrupted-vm" },
                  "result": null,
                  "error": null,
                  "retry_of": null,
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-05-01T00:00:00.0000000Z",
                  "updated_at": "2026-05-01T00:00:00.0000000Z"
                }
              ],
              "queue": [ "job-running" ]
            }
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs/job-running"));

            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("failed", data.GetProperty("status").GetString());
            Assert.Equal("PCV_JOB_INTERRUPTED", data.GetProperty("error").GetProperty("code").GetString());
            Assert.False(data.GetProperty("error").GetProperty("retryable").GetBoolean());
            Assert.Contains(
                "reconcile",
                data.GetProperty("error").GetProperty("recommended_action").GetString(),
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(jobStorePath);
        }
    }

    [Fact]
    public void JobCancelUsesRuntimePolicyForQueuedJobs()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        using var createDocument = JsonDocument.Parse(create.Body);
        var jobId = createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        var cancel = processor.Handle(new DesktopNodeApiRequest("POST", $"/api/v1/jobs/{jobId}/cancel"));

        Assert.Equal(200, cancel.StatusCode);
        using var cancelDocument = JsonDocument.Parse(cancel.Body);
        var data = cancelDocument.RootElement.GetProperty("data");
        Assert.Equal("canceled", data.GetProperty("status").GetString());
        Assert.Equal("PCV_JOB_CANCELED", data.GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("The job was canceled before it started.", data.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    public void JobRetryUsesRuntimePolicyAttemptLimit()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-runtime-policy-retry-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-failed",
                  "operation": "vm.start",
                  "status": "failed",
                  "params": { "name": "alpha" },
                  "result": null,
                  "error": {
                    "code": "PCV_TEST_RETRYABLE",
                    "message": "Synthetic retryable failure.",
                    "detail": "retry evidence",
                    "retryable": true
                  },
                  "retry_of": null,
                  "attempt": 3,
                  "canceled_at": null,
                  "created_at": "2026-05-11T00:00:00.0000000Z",
                  "updated_at": "2026-05-11T00:00:00.0000000Z"
                }
              ],
              "queue": []
            }
            """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

            var retry = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-failed/retry"));

            Assert.Equal(409, retry.StatusCode);
            using var document = JsonDocument.Parse(retry.Body);
            Assert.Equal("PCV_JOB_RETRY_LIMIT_REACHED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
        }
        finally
        {
            File.Delete(jobStorePath);
        }
    }

    [Fact]
    public void JobRetryBlocksSemanticallyCorruptPersistedStatusWithoutMutation()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-runtime-policy-invalid-status-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-mystery",
                  "operation": "vm.start",
                  "status": "mystery",
                  "params": { "name": "alpha" },
                  "result": null,
                  "error": {
                    "code": "PCV_TEST_RETRYABLE",
                    "message": "Synthetic retryable failure.",
                    "detail": "retry evidence",
                    "retryable": true
                  },
                  "retry_of": null,
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-05-11T00:00:00.0000000Z",
                  "updated_at": "2026-05-11T00:00:00.0000000Z"
                }
              ],
              "queue": []
            }
            """);

            var original = File.ReadAllText(jobStorePath);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

            var retry = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-mystery/retry"));

            Assert.Equal(409, retry.StatusCode);
            using var retryDocument = JsonDocument.Parse(retry.Body);
            Assert.Equal("PCV_JOB_STORE_CORRUPT", retryDocument.RootElement.GetProperty("error").GetProperty("code").GetString());

            var list = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
            using var listDocument = JsonDocument.Parse(list.Body);
            Assert.Equal(409, list.StatusCode);
            Assert.Equal(
                "PCV_JOB_STORE_CORRUPT",
                listDocument.RootElement.GetProperty("error").GetProperty("code").GetString());
            Assert.Equal(original, File.ReadAllText(jobStorePath));
        }
        finally
        {
            File.Delete(jobStorePath);
        }
    }

    [Fact]
    public void JobStoreSaveUsesAtomicTempReplaceAndCleansStaleTemp()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-atomic-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        var tempPath = jobStorePath + ".tmp";
        try
        {
            File.WriteAllText(tempPath, "partial previous write");
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));

            Assert.Equal(202, response.StatusCode);
            Assert.True(File.Exists(jobStorePath));
            Assert.False(File.Exists(tempPath));

            using var document = JsonDocument.Parse(File.ReadAllText(jobStorePath));
            Assert.Equal(1, document.RootElement.GetProperty("version").GetInt32());
            Assert.Equal("alpha", document.RootElement.GetProperty("jobs")[0].GetProperty("params").GetProperty("name").GetString());
            Assert.Equal("queued", document.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
        }
        finally
        {
            File.Delete(jobStorePath);
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-future-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """{"version":99,"jobs":[],"queue":[]}""");

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs/job-from-future"));

            Assert.Equal(409, response.StatusCode);
            Assert.True(File.Exists(jobStorePath));
            Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(jobStorePath)!, Path.GetFileName(jobStorePath) + ".unsupported.99.*"));

            using var document = JsonDocument.Parse(response.Body);
            var error = document.RootElement.GetProperty("error");
            Assert.Equal("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", error.GetProperty("code").GetString());
            Assert.Contains("version 99", error.GetProperty("detail").GetString());
        }
        finally
        {
            File.Delete(jobStorePath);
            foreach (var path in Directory.GetFiles(Path.GetDirectoryName(jobStorePath)!, Path.GetFileName(jobStorePath) + ".unsupported.99.*"))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void UnsupportedJobSubrouteDoesNotUseJobStoreBlock()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-future-unknown-job-route-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """{"version":99,"jobs":[],"queue":[]}""");

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs/job-from-future/events"));

            Assert.Equal(404, response.StatusCode);
            Assert.Contains("PCV_ROUTE_NOT_FOUND", response.Body);
            Assert.DoesNotContain("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", response.Body);
            Assert.True(File.Exists(jobStorePath));
            Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(jobStorePath)!, Path.GetFileName(jobStorePath) + ".unsupported.99.*"));
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }

            foreach (var path in Directory.GetFiles(Path.GetDirectoryName(jobStorePath)!, Path.GetFileName(jobStorePath) + ".unsupported.99.*"))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void JobStoreVersion2MigrationStoreLoadsWithoutBlockedDiagnostics()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-v2-jobs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(
                jobStorePath,
                """
                {"version":2,"jobs":[{"job_id":"job-v2","operation":"vm.start","status":"succeeded","params":{"name":"alpha"},"created_at":"2026-05-06T00:00:00Z","updated_at":"2026-05-06T00:00:00Z"}],"queue":[],"migration":{"plan_id":"job-store-v1-to-v2"}}
                """);

            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs/job-v2"));

            Assert.Equal(200, response.StatusCode);
            Assert.DoesNotContain("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", response.Body);
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("job-v2", document.RootElement.GetProperty("data").GetProperty("job_id").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void JobListBlocksUnsupportedFutureJobStoreWithoutMutation()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-future-job-list-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """{"version":99,"jobs":[],"queue":[]}""");
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));

            Assert.Equal(409, response.StatusCode);
            Assert.Contains("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", response.Body);
            Assert.Contains("No quarantine, migration, or job store write was performed", response.Body);
            Assert.True(File.Exists(jobStorePath));
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void OpsSummaryProjectsUnsupportedFutureJobStoreWithReadOnlyNativeObservations()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-future-ops-summary-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """{"version":99,"jobs":[],"queue":[]}""");
            var nativeCalls = new List<string>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, handledOperation: null, responseJson: null),
                jobStorePath: jobStorePath);

            var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

            Assert.Equal(200, response.StatusCode);
            using var document = JsonDocument.Parse(response.Body);
            Assert.True(document.RootElement.GetProperty("ok").GetBoolean());
            var jobStore = document.RootElement.GetProperty("data").GetProperty("job_store");
            Assert.Equal("blocked", jobStore.GetProperty("status").GetString());
            Assert.True(jobStore.GetProperty("mutation_blocked").GetBoolean());
            Assert.Equal(
                "PCV_JOB_STORE_SCHEMA_UNSUPPORTED",
                jobStore.GetProperty("error_code").GetString());
            Assert.Equal(
                "load-blocked",
                Assert.Single(jobStore.GetProperty("recent_events").EnumerateArray())
                    .GetProperty("event")
                    .GetString());
            Assert.DoesNotContain(jobStorePath, response.Body, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(jobStorePath));
            Assert.Equal(["host.status", "vm.list"], nativeCalls);
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void ApiAssemblyDoesNotShipPowerShellProcessHelperInProductRuntime()
    {
        var typeNames = typeof(DesktopNodeApiRequestProcessor).Assembly
            .GetTypes()
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        Assert.DoesNotContain(typeNames, name => name.Contains("PowerShell", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeNames, name => name.Contains("ProcessHelper", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("POST", "/api/v1/vms/lab%20vm/start", null, "vm.start", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/pause", null, "vm.pause", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/resume", null, "vm.resume", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/save", null, "vm.save", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/resume-saved", null, "vm.resume-saved", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/eject", null, "vm.eject", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/attach", """{"iso_path":"D:\\isos\\ubuntu.iso"}""", "vm.attach", "name", "lab vm")]
    [InlineData("DELETE", "/api/v1/vms/lab%20vm", null, "vm.delete", "name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/checkpoints", "{\"name\":\"before-upgrade\"}", "checkpoint.create", "vm_name", "lab vm")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/checkpoints/before-upgrade/restore", null, "checkpoint.restore", "checkpoint_name", "before-upgrade")]
    [InlineData("DELETE", "/api/v1/vms/lab%20vm/checkpoints/before-upgrade", null, "checkpoint.delete", "checkpoint_name", "before-upgrade")]
    public void MutationRoutesQueueJobsWithoutInvokingExternalFallback(
        string method,
        string path,
        string? body,
        string expectedOperation,
        string expectedParamName,
        string expectedParamValue)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest(method, path, body));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("job.create", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(expectedOperation, data.GetProperty("operation").GetString());
        Assert.Equal(expectedParamValue, data.GetProperty("params").GetProperty(expectedParamName).GetString());
    }

    [Theory]
    [InlineData("/api/v1/vms/lab%20vm/set-memory", "{\"memory_mb\":8192}", "vm.set-memory", "memory_mb", 8192)]
    [InlineData("/api/v1/vms/lab%20vm/set-vcpu", "{\"cpu\":4}", "vm.set-vcpu", "cpu", 4)]
    [InlineData("/api/v1/vms/lab%20vm/disk-resize", "{\"disk_gb\":96}", "vm.disk-resize", "disk_gb", 96)]
    [InlineData("/api/v1/vms/lab%20vm/limit", "{\"cpu\":4}", "vm.limit", "cpu", 4)]
    public void VmResourceMutationRoutesQueueJobsWithRequestedValue(
        string path,
        string body,
        string expectedOperation,
        string expectedParamName,
        int expectedParamValue)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("POST", path, body));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("job.create", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(expectedOperation, data.GetProperty("operation").GetString());
        Assert.Equal("lab vm", data.GetProperty("params").GetProperty("name").GetString());
        Assert.Equal(expectedParamValue, data.GetProperty("params").GetProperty(expectedParamName).GetInt32());
    }

    [Theory]
    [InlineData("/api/v1/vms/lab%20vm/qos/storage/preview", "{\"disk\":\"disk0\",\"maximum_iops\":1200,\"minimum_iops\":100}", "vm.qos.storage.preview", "storage", "target_disk", "disk0", "maximum_iops", 1200)]
    [InlineData("/api/v1/vms/lab%20vm/qos/network/preview", "{\"adapter\":\"eth0\",\"maximum_kbps\":2048,\"minimum_kbps\":256}", "vm.qos.network.preview", "network", "adapter", "eth0", "maximum_kbps", 2048)]
    public void QosPreviewRoutesReturnDryRunContractWithoutQueuingJob(
        string path,
        string body,
        string expectedOperation,
        string expectedSection,
        string expectedTargetProperty,
        string expectedTargetValue,
        string expectedPolicyProperty,
        int expectedPolicyValue)
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVQosMutationAdapter(nativeCalls));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", path, body, RequestId: "req-qos-preview"));

        Assert.Equal(200, response.StatusCode);
        Assert.Single(nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal(expectedOperation, document.RootElement.GetProperty("operation").GetString());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("hyperv-qos-mutation-preview.v1", data.GetProperty("contract").GetString());
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.Equal("lab vm", data.GetProperty("vm").GetProperty("name").GetString());
        Assert.Equal(expectedTargetValue, data.GetProperty(expectedSection).GetProperty(expectedTargetProperty).GetString());
        Assert.Equal(expectedPolicyValue, data.GetProperty(expectedSection).GetProperty("proposed_policy").GetProperty(expectedPolicyProperty).GetInt32());
        Assert.False(data.GetProperty("validation").GetProperty("host_mutation_performed").GetBoolean());
        Assert.DoesNotContain("token", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/v1/vms/lab%20vm/qos/storage/preview", "{\"disk\":\"disk0\",\"maximum_iops\":-1}", "vm.qos.storage.preview", "PCV_VM_QOS_STORAGE_RANGE_INVALID")]
    [InlineData("/api/v1/vms/lab%20vm/qos/network/preview", "{\"adapter\":\"eth0\",\"maximum_kbps\":2048,\"minimum_kbps\":4096}", "vm.qos.network.preview", "PCV_VM_QOS_NETWORK_RANGE_INVALID")]
    public void QosPreviewRoutesRejectInvalidRangesBeforeNativeAdapter(
        string path,
        string body,
        string expectedOperation,
        string expectedCode)
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVQosMutationAdapter(nativeCalls));

        var response = processor.Handle(new DesktopNodeApiRequest("POST", path, body, RequestId: "req-qos-preview-invalid"));

        Assert.Equal(400, response.StatusCode);
        Assert.Empty(nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(expectedOperation, document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(expectedCode, document.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.DoesNotContain("token", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/v1/vms/lab%20vm/qos/storage", "{\"disk\":\"disk0\",\"maximum_iops\":1200,\"minimum_iops\":100}", "vm.qos.storage.set", "disk", "disk0", "maximum_iops", 1200)]
    [InlineData("/api/v1/vms/lab%20vm/qos/network", "{\"adapter\":\"eth0\",\"maximum_kbps\":2048,\"minimum_kbps\":256}", "vm.qos.network.set", "adapter", "eth0", "maximum_kbps", 2048)]
    public void QosApplyRoutesQueueJobsWithRollbackDescriptorInputs(
        string path,
        string body,
        string expectedOperation,
        string expectedTargetProperty,
        string expectedTargetValue,
        string expectedPolicyProperty,
        int expectedPolicyValue)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("POST", path, body));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("job.create", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(expectedOperation, data.GetProperty("operation").GetString());
        Assert.Equal("lab vm", data.GetProperty("params").GetProperty("name").GetString());
        Assert.Equal(expectedTargetValue, data.GetProperty("params").GetProperty(expectedTargetProperty).GetString());
        Assert.Equal(expectedPolicyValue, data.GetProperty("params").GetProperty(expectedPolicyProperty).GetInt32());
        Assert.True(data.GetProperty("params").GetProperty("rollback_descriptor_required").GetBoolean());
        Assert.True(data.GetProperty("params").GetProperty("readback_after_apply_required").GetBoolean());
    }

    [Theory]
    [InlineData("/api/v1/vms/lab%20vm/qos/storage", "{\"disk\":\"disk0\",\"maximum_iops\":1200,\"minimum_iops\":2400}", "vm.qos.storage.set", "PCV_VM_QOS_STORAGE_RANGE_INVALID")]
    [InlineData("/api/v1/vms/lab%20vm/qos/network", "{\"adapter\":\"eth0\",\"maximum_kbps\":-1}", "vm.qos.network.set", "PCV_VM_QOS_NETWORK_RANGE_INVALID")]
    public void QosApplyRoutesRejectInvalidRangesBeforeQueuingJob(
        string path,
        string body,
        string expectedOperation,
        string expectedCode)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("POST", path, body, RequestId: "req-qos-apply-invalid"));
        var tick = processor.ProcessOneQueuedJob();

        Assert.Equal(400, response.StatusCode);
        Assert.False(tick.Processed);
        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(expectedOperation, document.RootElement.GetProperty("operation").GetString());
        Assert.Equal(expectedCode, document.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.DoesNotContain("token", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("POST", "/api/v1/vms/lab%20vm/qos/storage", "{\"disk\":\"disk0\",\"maximum_iops\":1200}", "vm.qos.storage.set", "disk", "disk0", "storage-qos")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/qos/network", "{\"adapter\":\"eth0\",\"maximum_kbps\":2048}", "vm.qos.network.set", "adapter", "eth0", "network-qos")]
    public void QueuedQosMutationWorkerDispatchesToNativeAdapterWithRollbackEvidence(
        string method,
        string path,
        string body,
        string expectedOperation,
        string expectedTargetProperty,
        string expectedTargetValue,
        string expectedAction)
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVQosMutationAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(method, path, body, RequestId: "req-qos-apply"));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(nativeCalls);

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal(expectedOperation, nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal(expectedTargetValue, parameters.RootElement.GetProperty(expectedTargetProperty).GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        var resultData = tick.Job.Value.GetProperty("result").GetProperty("data");
        Assert.Equal(expectedAction, resultData.GetProperty("action").GetString());
        Assert.Equal("hyperv-qos-mutation-apply-evidence.v1", resultData.GetProperty("evidence").GetProperty("contract").GetString());
        Assert.True(resultData.GetProperty("evidence").GetProperty("audit").GetProperty("args_redacted").GetBoolean());
    }

    [Fact]
    public void VmDeleteStatusReturnsLatestDeleteJobForVm()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var create = processor.Handle(new DesktopNodeApiRequest("DELETE", "/api/v1/vms/lab%20vm"));
        var status = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/lab%20vm/delete-status"));

        Assert.Equal(202, create.StatusCode);
        Assert.Equal(200, status.StatusCode);
        using var document = JsonDocument.Parse(status.Body);
        Assert.Equal("vm.delete-status", document.RootElement.GetProperty("operation").GetString());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("lab vm", data.GetProperty("name").GetString());
        Assert.Equal("queued", data.GetProperty("status").GetString());
        Assert.Equal("vm.delete", data.GetProperty("operation").GetString());
        Assert.StartsWith("job-", data.GetProperty("job_id").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public void VmRenameRouteQueuesJobWithOldAndNewNameWithoutExternalFallback()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/rename",
            """{"new_name":"renamed lab"}"""));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("job.create", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("vm.rename", data.GetProperty("operation").GetString());
        Assert.Equal("lab vm", data.GetProperty("params").GetProperty("name").GetString());
        Assert.Equal("renamed lab", data.GetProperty("params").GetProperty("new_name").GetString());
    }

    [Theory]
    [InlineData("""{"confirm_name":"other vm"}""")]
    [InlineData("""{"confirm_name":"Lab vm"}""")]
    [InlineData("""{"confirm_name":"lab vm "}""")]
    [InlineData("""{"confirm_name":""}""")]
    [InlineData("""{"confirm_name":"   "}""")]
    [InlineData("{}")]
    [InlineData(null)]
    public void VmManageRouteRejectsConfirmNameMismatch(string? body)
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/manage",
            body));

        Assert.Equal(400, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("vm.manage", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("PCV_VM_MANAGE_CONFIRMATION_MISMATCH", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void VmManageRouteQueuesJobWithConfirmNameMatchingDecodedVmId()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/manage",
            """{"confirm_name":"lab vm"}"""));

        Assert.Equal(202, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("job.create", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("vm.manage", data.GetProperty("operation").GetString());
        Assert.Equal("lab vm", data.GetProperty("params").GetProperty("name").GetString());
        Assert.False(data.GetProperty("params").TryGetProperty("confirm_name", out _));
    }

    [Fact]
    public void VmRenameQueueCapturesReadbackBaselineWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = """
                {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"managed_by_purecvisor":true}],"error":null}
                """
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/rename",
            """{"new_name":"renamed lab"}"""));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-vm-rename-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("lab vm", reconciliation.GetProperty("before").GetProperty("name").GetString());
        Assert.Equal("renamed lab", reconciliation.GetProperty("expected_after").GetProperty("name").GetString());
    }

    [Fact]
    public void VmDeleteQueueCapturesManagedReadbackBaselineWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["vm.list"] = """
                {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"managed_by_purecvisor":true}],"error":null}
                """
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "DELETE",
            "/api/v1/vms/lab%20vm"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-vm-delete-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("lab vm", reconciliation.GetProperty("before").GetProperty("name").GetString());
        Assert.Equal("absent", reconciliation.GetProperty("expected_after").GetProperty("state").GetString());
    }

    [Fact]
    public void VmRenameReconcileConfirmsPostconditionWithoutCallingRenameProvider()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-rename-reconcile-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-rename-reconcile",
                  "operation": "vm.rename",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "new_name": "renamed-vm",
                    "reconciliation": {
                      "schema": "pcv-vm-rename-reconciliation/v1",
                      "capture_status": "captured",
                      "before": { "id": "vm-id", "name": "lab-vm", "platform": "hyperv", "guest_family": "windows", "state": "off", "cpu": { "count": 2 }, "memory": { "startup_mb": 4096 }, "generation": 2, "managed_by_purecvisor": true },
                      "before_fingerprint": { "platform": "hyperv", "guest_family": "windows", "state": "off", "cpu_count": 2, "startup_memory_mb": 4096, "generation": 2, "managed_by_purecvisor": true },
                      "expected_after": { "name": "renamed-vm" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-rename-reconcile",
                  "correlation_id": "corr-rename-reconcile",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-08-03T00:00:00.0000000Z",
                  "updated_at": "2026-08-03T00:00:01.0000000Z"
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
                    {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"renamed-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"managed_by_purecvisor":true}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-rename-reconcile/reconcile",
                RequestId: "req-reconcile"));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(["vm.list"], nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("succeeded", data.GetProperty("status").GetString());
            Assert.Equal("postcondition-confirmed", data.GetProperty("result").GetProperty("reconciliation").GetProperty("classification").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void VmDeleteReconcileConfirmsAbsentPostconditionWithoutCallingDeleteProvider()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-delete-reconcile-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-delete-reconcile",
                  "operation": "vm.delete",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "reconciliation": {
                      "schema": "pcv-vm-delete-reconciliation/v1",
                      "capture_status": "captured",
                      "before": { "id": "vm-id", "name": "lab-vm", "platform": "hyperv", "guest_family": "windows", "state": "off", "cpu": { "count": 2 }, "memory": { "startup_mb": 4096 }, "generation": 2, "managed_by_purecvisor": true },
                      "before_fingerprint": { "platform": "hyperv", "guest_family": "windows", "state": "off", "cpu_count": 2, "startup_memory_mb": 4096, "generation": 2, "managed_by_purecvisor": true },
                      "expected_after": { "name": "lab-vm", "state": "absent" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-delete-reconcile",
                  "correlation_id": "corr-delete-reconcile",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-08-03T00:00:00.0000000Z",
                  "updated_at": "2026-08-03T00:00:01.0000000Z"
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
                    {"ok":true,"operation":"vm.list","data":[],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-delete-reconcile/reconcile",
                RequestId: "req-reconcile"));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(["vm.list"], nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("succeeded", data.GetProperty("status").GetString());
            Assert.Equal("vm.delete", data.GetProperty("result").GetProperty("operation").GetString());
            Assert.Equal("postcondition-confirmed", data.GetProperty("result").GetProperty("reconciliation").GetProperty("classification").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void VmDeleteReconcileRequiresManualActionWhenSameIdentityStillExists()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-delete-reconcile-required-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-delete-required",
                  "operation": "vm.delete",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "reconciliation": {
                      "schema": "pcv-vm-delete-reconciliation/v1",
                      "capture_status": "captured",
                      "before": { "id": "vm-id", "name": "lab-vm", "managed_by_purecvisor": true },
                      "before_fingerprint": { "managed_by_purecvisor": true },
                      "expected_after": { "name": "lab-vm", "state": "absent" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-delete-required",
                  "correlation_id": "corr-delete-required",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-08-03T00:00:00.0000000Z",
                  "updated_at": "2026-08-03T00:00:01.0000000Z"
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
                    {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","managed_by_purecvisor":true}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-delete-required/reconcile"));

            Assert.Equal(409, response.StatusCode);
            Assert.Equal(["vm.list"], nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
            Assert.Contains("not-applied", document.RootElement.GetProperty("error").GetProperty("detail").GetString(), StringComparison.Ordinal);
            Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void CheckpointCreateQueueCapturesAbsentReadbackBaselineWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["checkpoint.list"] = """
                {"ok":true,"operation":"checkpoint.list","data":[],"error":null}
                """
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints",
            """{"name":"before-upgrade"}"""));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["checkpoint.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-checkpoint-create-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("absent", reconciliation.GetProperty("expected_before").GetProperty("state").GetString());
        Assert.Equal("before-upgrade", reconciliation.GetProperty("expected_after").GetProperty("name").GetString());
    }

    [Fact]
    public void CheckpointCreateReconcileConfirmsPostconditionWithoutCallingCreateProvider()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-checkpoint-create-reconcile-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-checkpoint-create-reconcile",
                  "operation": "checkpoint.create",
                  "status": "failed",
                  "params": {
                    "vm_name": "lab-vm",
                    "checkpoint_name": "before-upgrade",
                    "reconciliation": {
                      "schema": "pcv-checkpoint-create-reconciliation/v1",
                      "capture_status": "captured",
                      "before": null,
                      "expected_before": { "state": "absent", "name": "before-upgrade", "vm_name": "lab-vm" },
                      "expected_after": { "state": "present", "name": "before-upgrade", "vm_name": "lab-vm" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-checkpoint-create-reconcile",
                  "correlation_id": "corr-checkpoint-create-reconcile",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-08-03T00:00:00.0000000Z",
                  "updated_at": "2026-08-03T00:00:01.0000000Z"
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
                    ["checkpoint.list"] = """
                    {"ok":true,"operation":"checkpoint.list","data":[{"name":"before-upgrade","vm_name":"lab-vm","created_at":"2026-08-03T00:00:02Z"}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-checkpoint-create-reconcile/reconcile",
                RequestId: "req-reconcile"));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(["checkpoint.list"], nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("succeeded", data.GetProperty("status").GetString());
            Assert.Equal("checkpoint.create", data.GetProperty("result").GetProperty("operation").GetString());
            Assert.Equal("postcondition-confirmed", data.GetProperty("result").GetProperty("reconciliation").GetProperty("classification").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void CheckpointRestoreQueueCapturesCurrentReadbackBaselineWithoutMutatingProvider()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["checkpoint.list"] = """
                {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":false}],"error":null}
                """
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/requested/restore"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["checkpoint.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("checkpoint.restore", data.GetProperty("operation").GetString());
        Assert.Equal("requested", data.GetProperty("params").GetProperty("checkpoint_name").GetString());
        Assert.Equal("lab-vm", data.GetProperty("params").GetProperty("vm_name").GetString());
        var reconciliation = data.GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-checkpoint-restore-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("captured", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("old", reconciliation.GetProperty("before").GetProperty("current_name").GetString());
        Assert.Equal("lab-vm", reconciliation.GetProperty("before").GetProperty("vm_name").GetString());
        Assert.Equal("requested", reconciliation.GetProperty("expected_after").GetProperty("current_name").GetString());
        Assert.Equal("lab-vm", reconciliation.GetProperty("expected_after").GetProperty("vm_name").GetString());
        Assert.True(reconciliation.GetProperty("expected_after").GetProperty("is_current").GetBoolean());
    }

    [Fact]
    public void CheckpointRestoreQueueMarksAlreadyCurrentBaselineUnavailable()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["checkpoint.list"] = """
                {"ok":true,"operation":"checkpoint.list","data":[{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true}],"error":null}
                """
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/requested/restore"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["checkpoint.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("pcv-checkpoint-restore-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
        Assert.Equal("unavailable", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("PCV_CHECKPOINT_ALREADY_CURRENT", reconciliation.GetProperty("capture_error_code").GetString());
    }

    [Fact]
    public void CheckpointRestoreQueueMarksMissingRequestedNameUnavailable()
    {
        var nativeCalls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
            {
                ["checkpoint.list"] = """
                {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true}],"error":null}
                """
            }));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/requested/restore"));

        Assert.Equal(202, response.StatusCode);
        Assert.Equal(["checkpoint.list"], nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        var reconciliation = document.RootElement.GetProperty("data").GetProperty("params").GetProperty("reconciliation");
        Assert.Equal("unavailable", reconciliation.GetProperty("capture_status").GetString());
        Assert.Equal("PCV_CHECKPOINT_NOT_FOUND", reconciliation.GetProperty("capture_error_code").GetString());
    }

    [Fact]
    public void CheckpointRestoreReconcileConfirmsPostconditionWithoutCallingRestoreProvider()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-checkpoint-restore-reconcile-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, InterruptedCheckpointRestoreJobStoreJson());

            var nativeCalls = new List<string>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath,
                nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
                {
                    ["checkpoint.list"] = """
                    {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":false},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":true}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-checkpoint-restore-reconcile/reconcile",
                RequestId: "req-reconcile"));

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(["checkpoint.list"], nativeCalls);
            Assert.DoesNotContain("checkpoint.restore", nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("succeeded", data.GetProperty("status").GetString());
            Assert.Equal("reconciled", data.GetProperty("result").GetProperty("action").GetString());
            Assert.Equal("checkpoint.restore", data.GetProperty("result").GetProperty("operation").GetString());
            var reconciliation = data.GetProperty("result").GetProperty("reconciliation");
            Assert.Equal("pcv-checkpoint-restore-reconciliation/v1", reconciliation.GetProperty("schema").GetString());
            Assert.Equal("postcondition-confirmed", reconciliation.GetProperty("classification").GetString());
            Assert.Equal("old", reconciliation.GetProperty("before").GetProperty("current_name").GetString());
            Assert.Equal("requested", reconciliation.GetProperty("expected_after").GetProperty("current_name").GetString());
            Assert.True(reconciliation.GetProperty("expected_after").GetProperty("is_current").GetBoolean());
            Assert.Equal("requested", reconciliation.GetProperty("observed").GetProperty("name").GetString());
            Assert.Equal("lab-vm", reconciliation.GetProperty("observed").GetProperty("vm_name").GetString());
            Assert.True(reconciliation.GetProperty("observed").GetProperty("is_current").GetBoolean());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenRequestedIsNotCurrent()
    {
        AssertCheckpointRestoreReconcileRequired(
            """
            {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":false}],"error":null}
            """,
            "not-applied");
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenRequestedNameIsMissing()
    {
        AssertCheckpointRestoreReconcileRequired(
            """
            {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true}],"error":null}
            """,
            "not-applied");
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenRequestedNameIsAmbiguous()
    {
        AssertCheckpointRestoreReconcileRequired(
            """
            {"ok":true,"operation":"checkpoint.list","data":[{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":false}],"error":null}
            """,
            "ambiguous-duplicate-checkpoint-names");
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenCurrentIsNull()
    {
        AssertCheckpointRestoreReconcileRequired(
            """
            {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":null},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":null}],"error":null}
            """,
            "current-unavailable");
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenDuplicateCurrentIncludesRequestedName()
    {
        AssertCheckpointRestoreReconcileRequired(
            """
            {"ok":true,"operation":"checkpoint.list","data":[{"name":"old","vm_name":"lab-vm","created_at":"2026-08-14T00:00:00Z","is_current":true},{"name":"requested","vm_name":"lab-vm","created_at":"2026-08-14T00:00:01Z","is_current":true}],"error":null}
            """,
            "current-unavailable");
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenListFailsWithoutCallingRestore()
    {
        AssertCheckpointRestoreReconcileRequired(
            """
            {"ok":false,"operation":"checkpoint.list","data":null,"error":{"code":"PCV_CHECKPOINT_LIST_FAILED","message":"List failed.","detail":"Provider checkpoint.list failed.","retryable":false}}
            """,
            "readback-unavailable");
    }

    [Fact]
    public void CheckpointRestoreReconcileRequiresManualActionWhenCapturedBaselineIsMissing()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-checkpoint-restore-baseline-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, InterruptedCheckpointRestoreJobStoreJson("""
            {
              "schema": "pcv-checkpoint-restore-reconciliation/v1",
              "capture_status": "unavailable",
              "capture_error_code": "PCV_CHECKPOINT_CURRENT_UNAVAILABLE",
              "before": null,
              "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true }
            }
            """));

            var nativeCalls = new List<string>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath,
                nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
                {
                    ["checkpoint.list"] = """
                    {"ok":true,"operation":"checkpoint.list","data":[{"name":"requested","vm_name":"lab-vm","is_current":true}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-checkpoint-restore-reconcile/reconcile"));

            Assert.Equal(409, response.StatusCode);
            Assert.Empty(nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
            Assert.Contains("baseline-unavailable", document.RootElement.GetProperty("error").GetProperty("detail").GetString(), StringComparison.Ordinal);
            Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void CheckpointRestoreReconcileJobNotReconcilableDetailIncludesRestoreFamily()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-checkpoint-restore-not-reconcilable-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, InterruptedCheckpointRestoreJobStoreJson().Replace(
                "\"code\": \"PCV_JOB_INTERRUPTED\"",
                "\"code\": \"PCV_CHECKPOINT_RESTORE_FAILED\"",
                StringComparison.Ordinal));

            var nativeCalls = new List<string>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath,
                nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>()));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-checkpoint-restore-reconcile/reconcile"));

            Assert.Equal(409, response.StatusCode);
            Assert.Empty(nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
            var detail = document.RootElement.GetProperty("error").GetProperty("detail").GetString();
            Assert.Contains("job-not-reconcilable", detail, StringComparison.Ordinal);
            Assert.Contains("checkpoint.restore", detail, StringComparison.Ordinal);
            Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void VmRenameReconcileRequiresManualActionWhenBeforeStateStillExists()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-rename-reconcile-required-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "jobs": [
                {
                  "job_id": "job-rename-required",
                  "operation": "vm.rename",
                  "status": "failed",
                  "params": {
                    "name": "lab-vm",
                    "new_name": "renamed-vm",
                    "reconciliation": {
                      "schema": "pcv-vm-rename-reconciliation/v1",
                      "capture_status": "captured",
                      "before": { "id": "vm-id", "name": "lab-vm", "platform": "hyperv", "guest_family": "windows", "state": "off", "cpu": { "count": 2 }, "memory": { "startup_mb": 4096 }, "generation": 2, "managed_by_purecvisor": true },
                      "before_fingerprint": { "platform": "hyperv", "guest_family": "windows", "state": "off", "cpu_count": 2, "startup_memory_mb": 4096, "generation": 2, "managed_by_purecvisor": true },
                      "expected_after": { "name": "renamed-vm" }
                    }
                  },
                  "result": null,
                  "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
                  "retry_of": null,
                  "request_id": "req-rename-required",
                  "correlation_id": "corr-rename-required",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-08-03T00:00:00.0000000Z",
                  "updated_at": "2026-08-03T00:00:01.0000000Z"
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
                    {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"managed_by_purecvisor":true}],"error":null}
                    """
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-rename-required/reconcile"));

            Assert.Equal(409, response.StatusCode);
            Assert.Equal(["vm.list"], nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
            Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void QueuedVmEjectWorkerDispatchesToNativeAdapterWithoutExternalFallback()
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmMediaAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/lab%20vm/eject"));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(nativeCalls);

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal("vm.eject", nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("eject", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void QueuedVmAttachWorkerDispatchesToNativeAdapterWithoutExternalFallback()
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmMediaAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/attach",
            """{"iso_path":"D:\\isos\\ubuntu.iso"}"""));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(nativeCalls);
        using (var document = JsonDocument.Parse(create.Body))
        {
            Assert.Equal("vm.attach", document.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal("vm.attach", nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal(@"D:\isos\ubuntu.iso", parameters.RootElement.GetProperty("iso_path").GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("attach", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void QueueAttachVmMediaRejectsMissingIsoPath()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/attach",
            "{}"));

        Assert.Equal(400, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("PCV_VM_ATTACH_ISO_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("POST", "/api/v1/vms/lab%20vm/set-memory", "{\"memory_mb\":8192}", "vm.set-memory", "memory_mb", 8192, "set-memory")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/set-vcpu", "{\"cpu\":4}", "vm.set-vcpu", "cpu", 4, "set-vcpu")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/disk-resize", "{\"disk_gb\":96}", "vm.disk-resize", "disk_gb", 96, "disk-resize")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/limit", "{\"cpu\":4}", "vm.limit", "cpu", 4, "limit")]
    public void QueuedVmResourceMutationWorkerDispatchesToNativeAdapterWithoutExternalFallback(
        string method,
        string path,
        string body,
        string expectedOperation,
        string expectedParamName,
        int expectedParamValue,
        string expectedAction)
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmResourceMutationAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(method, path, body));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(nativeCalls);

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal(expectedOperation, nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal(expectedParamValue, parameters.RootElement.GetProperty(expectedParamName).GetInt32());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal(expectedAction, tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void QueuedVmDeleteWorkerDispatchesToNativeAdapterWithoutExternalFallback()
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmDeleteAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest("DELETE", "/api/v1/vms/lab%20vm"));

        Assert.Equal(202, create.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls.Select(call => call.Operation).ToArray());

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Equal(["vm.list", "vm.delete"], nativeCalls.Select(call => call.Operation).ToArray());
        var nativeCall = nativeCalls[1];
        Assert.Equal("vm.delete", nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("vm.delete", tick.Job.Value.GetProperty("result").GetProperty("operation").GetString());
        Assert.Equal("delete", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Theory]
    [InlineData("POST", "/api/v1/vms/lab-vm/start", "vm.start", "lab-vm", "start")]
    [InlineData("POST", "/api/v1/vms/lab-vm/shutdown", "vm.shutdown", "lab-vm", "shutdown")]
    [InlineData("POST", "/api/v1/vms/lab-vm/poweroff", "vm.poweroff", "lab-vm", "poweroff")]
    [InlineData("POST", "/api/v1/vms/lab-vm/restart", "vm.restart", "lab-vm", "restart")]
    [InlineData("POST", "/api/v1/vms/lab-vm/pause", "vm.pause", "lab-vm", "pause")]
    [InlineData("POST", "/api/v1/vms/lab-vm/resume", "vm.resume", "lab-vm", "resume")]
    [InlineData("POST", "/api/v1/vms/lab-vm/save", "vm.save", "lab-vm", "save")]
    [InlineData("POST", "/api/v1/vms/lab-vm/resume-saved", "vm.resume-saved", "lab-vm", "resume-saved")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/start", "vm.start", "lab vm", "start")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/shutdown", "vm.shutdown", "lab vm", "shutdown")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/poweroff", "vm.poweroff", "lab vm", "poweroff")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/restart", "vm.restart", "lab vm", "restart")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/pause", "vm.pause", "lab vm", "pause")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/resume", "vm.resume", "lab vm", "resume")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/save", "vm.save", "lab vm", "save")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/resume-saved", "vm.resume-saved", "lab vm", "resume-saved")]
    public void QueuedVmPowerStateWorkerDispatchesToNativeAdapterWithoutExternalFallback(
        string method,
        string path,
        string expectedOperation,
        string expectedVmName,
        string expectedAction)
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVPowerStateAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(method, path));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Empty(nativeCalls);

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Empty(fallbackCalls);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal(expectedOperation, nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal(expectedVmName, parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal(expectedOperation, tick.Job.Value.GetProperty("result").GetProperty("operation").GetString());
        Assert.Equal(expectedAction, tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void QueuedVmRenameWorkerDispatchesToNativeAdapterWithoutExternalFallback()
    {
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmRenameAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/rename",
        """{"new_name":"renamed lab"}"""));

        Assert.Equal(202, create.StatusCode);
        Assert.Equal("vm.list", Assert.Single(nativeCalls).Operation);

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Equal(2, nativeCalls.Count);
        var nativeCall = nativeCalls[1];
        Assert.Equal("vm.rename", nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.Equal("renamed lab", parameters.RootElement.GetProperty("new_name").GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("vm.rename", tick.Job.Value.GetProperty("result").GetProperty("operation").GetString());
        Assert.Equal("rename", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Fact]
    public void QueuedVmManageWorkerDispatchesToNativeAdapterWithoutExternalFallback()
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVVmManageAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab%20vm/manage",
            """{"confirm_name":"lab vm"}"""));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(fallbackCalls);
        Assert.Empty(nativeCalls);

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Empty(fallbackCalls);
        var nativeCall = Assert.Single(nativeCalls);
        Assert.Equal("vm.manage", nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal("lab vm", parameters.RootElement.GetProperty("name").GetString());
        Assert.False(parameters.RootElement.TryGetProperty("confirm_name", out _));
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("vm.manage", tick.Job.Value.GetProperty("result").GetProperty("operation").GetString());
        Assert.Equal("manage", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
    }

    [Theory]
    [InlineData("POST", "/api/v1/vms/lab-vm/checkpoints", "{\"name\":\"before-upgrade\"}", "checkpoint.create", "lab-vm", "before-upgrade")]
    [InlineData("POST", "/api/v1/vms/lab-vm/checkpoints/before-upgrade/restore", null, "checkpoint.restore", "lab-vm", "before-upgrade")]
    [InlineData("DELETE", "/api/v1/vms/lab-vm/checkpoints/before-upgrade", null, "checkpoint.delete", "lab-vm", "before-upgrade")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/checkpoints", "{\"name\":\"before upgrade\"}", "checkpoint.create", "lab vm", "before upgrade")]
    [InlineData("POST", "/api/v1/vms/lab%20vm/checkpoints/before%20upgrade/restore", null, "checkpoint.restore", "lab vm", "before upgrade")]
    [InlineData("DELETE", "/api/v1/vms/lab%20vm/checkpoints/before%20upgrade", null, "checkpoint.delete", "lab vm", "before upgrade")]
    public void QueuedCheckpointMutationWorkerDispatchesToNativeAdapterWithoutExternalFallback(
        string method,
        string path,
        string? body,
        string expectedOperation,
        string expectedVmName,
        string expectedCheckpointName)
    {
        var fallbackCalls = new List<DesktopNodeHyperVOperationCall>();
        var nativeCalls = new List<DesktopNodeHyperVOperationCall>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVMutationAdapter(nativeCalls));

        var create = processor.Handle(new DesktopNodeApiRequest(method, path, body));

        Assert.Equal(202, create.StatusCode);
        Assert.Empty(fallbackCalls);
        if (expectedOperation is "checkpoint.create" or "checkpoint.restore")
        {
            Assert.Equal("checkpoint.list", Assert.Single(nativeCalls).Operation);
        }
        else
        {
            Assert.Empty(nativeCalls);
        }

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Empty(fallbackCalls);
        var nativeCall = expectedOperation is "checkpoint.create" or "checkpoint.restore"
            ? nativeCalls[1]
            : Assert.Single(nativeCalls);
        Assert.Equal(expectedOperation, nativeCall.Operation);
        using var parameters = JsonDocument.Parse(nativeCall.ParamsJson);
        Assert.Equal(expectedVmName, parameters.RootElement.GetProperty("vm_name").GetString());
        Assert.Equal(expectedCheckpointName, parameters.RootElement.GetProperty("checkpoint_name").GetString());
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal(expectedOperation, tick.Job.Value.GetProperty("result").GetProperty("operation").GetString());
    }

    [Fact]
    public void RuntimePolicyGetReturnsManagedContractBody()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy"));

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var jobRuntime = root.GetProperty("data").GetProperty("job_runtime");

        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal("runtime.policy", root.GetProperty("operation").GetString());
        Assert.Equal(1, jobRuntime.GetProperty("contract_version").GetInt32());
        Assert.Equal("dotnet", jobRuntime.GetProperty("managed_core").GetProperty("candidate").GetString());
        Assert.Equal("service-host-default", jobRuntime.GetProperty("managed_core").GetProperty("status").GetString());
        Assert.Equal("none", root.GetProperty("data").GetProperty("auth").GetProperty("token_storage").GetString());
    }

    [Fact]
    public void RuntimePolicyResponseIncludesRequestId()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", RequestId: "req-test-runtime"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("req-test-runtime", document.RootElement.GetProperty("request_id").GetString());
        Assert.Equal("runtime.policy", document.RootElement.GetProperty("operation").GetString());
    }

    [Fact]
    public void FailureResponseIncludesGeneratedRequestId()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/not-found"));

        Assert.Equal(404, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var requestId = document.RootElement.GetProperty("request_id").GetString();
        Assert.StartsWith("req-", requestId);
        Assert.Equal("PCV_ROUTE_NOT_FOUND", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public void QueuedJobStoresRequestAndCorrelationIds()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/start",
            RequestId: "req-start-alpha"));

        Assert.Equal(202, create.StatusCode);
        using var createDocument = JsonDocument.Parse(create.Body);
        var data = createDocument.RootElement.GetProperty("data");
        Assert.Equal("req-start-alpha", createDocument.RootElement.GetProperty("request_id").GetString());
        Assert.Equal("req-start-alpha", data.GetProperty("request_id").GetString());
        Assert.Equal(data.GetProperty("job_id").GetString(), data.GetProperty("correlation_id").GetString());
    }

    [Fact]
    public void GeneratedRequestIdIsCanonicalAcrossResponseJobAndStore()
    {
        var store = new RecordingDesktopNodeApiJobStore();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store));

        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/start"));

        Assert.Equal(202, create.StatusCode);
        using var createDocument = JsonDocument.Parse(create.Body);
        using var storeDocument = JsonDocument.Parse(store.DurableSnapshot!);
        var responseRoot = createDocument.RootElement;
        var responseJob = responseRoot.GetProperty("data");
        var storedJob = storeDocument.RootElement.GetProperty("jobs")[0];
        var requestId = responseRoot.GetProperty("request_id").GetString();
        var jobId = responseJob.GetProperty("job_id").GetString();

        Assert.StartsWith("req-", requestId);
        Assert.Equal(requestId, responseJob.GetProperty("request_id").GetString());
        Assert.Equal(requestId, storedJob.GetProperty("request_id").GetString());
        Assert.Equal(jobId, responseJob.GetProperty("correlation_id").GetString());
        Assert.Equal(jobId, storedJob.GetProperty("correlation_id").GetString());
    }

    [Fact]
    public void RetryJobPreservesOriginalCorrelationIdAndStoresNewRequestId()
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-retry-correlation-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, """
            {
              "version": 1,
              "saved_at": "2026-05-05T00:00:00.0000000Z",
              "jobs": [
                {
                  "job_id": "job-failed",
                  "operation": "vm.start",
                  "status": "failed",
                  "params": { "name": "persisted-vm" },
                  "result": null,
                  "error": {
                    "code": "PCV_TRANSIENT_PROVIDER_FAILURE",
                    "message": "Provider call failed transiently.",
                    "detail": "retry evidence",
                    "retryable": true
                  },
                  "retry_of": null,
                  "request_id": "req-original-start",
                  "correlation_id": "corr-installed-route",
                  "attempt": 1,
                  "canceled_at": null,
                  "created_at": "2026-05-05T00:00:00.0000000Z",
                  "updated_at": "2026-05-05T00:00:01.0000000Z"
                }
              ],
              "queue": []
            }
            """);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

            var retry = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-failed/retry",
                RequestId: "req-retry-operator"));

            Assert.Equal(202, retry.StatusCode);
            using var document = JsonDocument.Parse(retry.Body);
            Assert.Equal("req-retry-operator", document.RootElement.GetProperty("request_id").GetString());
            var data = document.RootElement.GetProperty("data");
            Assert.Equal("job-failed", data.GetProperty("retry_of").GetString());
            Assert.Equal("req-retry-operator", data.GetProperty("request_id").GetString());
            Assert.Equal("corr-installed-route", data.GetProperty("correlation_id").GetString());
            Assert.Equal(2, data.GetProperty("attempt").GetInt32());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    [Fact]
    public void RuntimePolicyProcessorCarriesHostAuthAndNetworkContext()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            tokenStorage: "dpapi-local-machine",
            currentExposure: "lan");

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy"));

        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("dpapi-local-machine", data.GetProperty("auth").GetProperty("token_storage").GetString());
        Assert.Equal("lan", data.GetProperty("network").GetProperty("current_exposure").GetString());
        Assert.Equal("bearer-required", data.GetProperty("network").GetProperty("static_asset_auth").GetProperty("non_loopback").GetString());
    }

    [Fact]
    public void RuntimePolicyProcessorRejectsMutationMethods()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault();

        var response = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/runtime/policy"));

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);
        Assert.Contains("PCV_API_METHOD_NOT_ALLOWED", response.Body);
        Assert.DoesNotContain("Restart-Computer", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/v1/vms/alpha/guest/exec/preview", "vm.guest.exec.preview", "guest-execution-preview.v1")]
    [InlineData("/api/v1/vms/alpha/guest/channel/preview", "vm.guest.channel.preview", "guest-channel-preview.v1")]
    public void GuestExecutionPreviewRoutesReturnContractsWithoutNativeInvocation(
        string path,
        string operation,
        string contract)
    {
        var calls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(calls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            path,
            """{"command":["powershell","hostname"],"credential_ref":"wincred:PureCVisor/guest/admin"}""",
            RequestId: "req-guest-boundary"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(calls);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var data = root.GetProperty("data");
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal(operation, root.GetProperty("operation").GetString());
        Assert.Equal("req-guest-boundary", root.GetProperty("request_id").GetString());
        Assert.Equal(contract, data.GetProperty("contract").GetString());
        Assert.True(data.GetProperty("preview_enabled").GetBoolean());
        if (operation == "vm.guest.exec.preview")
        {
            Assert.True(data.GetProperty("execute_enabled").GetBoolean());
            Assert.Contains("omit --dry-run", data.GetProperty("next_action").GetString(), StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(data.GetProperty("verify_enabled").GetBoolean());
            Assert.True(data.GetProperty("repair_enabled").GetBoolean());
            Assert.Contains("--verify", data.GetProperty("next_action").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        Assert.False(data.GetProperty("host_mutation_performed").GetBoolean());
        Assert.DoesNotContain("super-secret-value", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PureCVisor/guest/admin", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void GuestExecutionPreviewRouteRejectsSecretLikeCommandBeforeNativeInvocation()
    {
        var calls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(calls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec/preview",
            """{"command":["powershell","AKIAIOSFODNN7EXAMPLE"],"credential_ref":"wincred:PureCVisor/guest/admin","environment":{"SESSION_ID":"N0j4pX8wQ2sL6mR9vT1zY3cB5nH7kD4f"}}""",
            RequestId: "req-guest-secret-preview"));

        Assert.Equal(400, response.StatusCode);
        Assert.Empty(calls);
        Assert.Contains("PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED", response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("AKIAIOSFODNN7EXAMPLE", response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("N0j4pX8wQ2sL6mR9vT1zY3cB5nH7kD4f", response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("PureCVisor/guest/admin", response.Body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/api/v1/vms/alpha/guest/channel/verify", "vm.guest.channel.verify", "{}", "PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED")]
    [InlineData("/api/v1/vms/alpha/guest/channel", "vm.guest.channel.ensure", "{}", "PCV_GUEST_CHANNEL_REPAIR_CONFIRMATION_REQUIRED")]
    public void GuestExecutionMutationRoutesRejectUnsafeRequestsWithoutNativeInvocation(
        string path,
        string operation,
        string body,
        string code)
    {
        var calls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(calls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            path,
            body,
            RequestId: "req-guest-boundary"));

        Assert.Equal(400, response.StatusCode);
        Assert.Empty(calls);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var error = root.GetProperty("error");
        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal(operation, root.GetProperty("operation").GetString());
        Assert.Equal("req-guest-boundary", root.GetProperty("request_id").GetString());
        Assert.Equal(code, error.GetProperty("code").GetString());
        Assert.DoesNotContain("PureCVisor/guest/admin", response.Body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/api/v1/vms/alpha/guest/exec", "vm.guest.exec", """{"command":["powershell","hostname"],"credential_ref":"wincred:PureCVisor/guest/admin","timeout_sec":45}""")]
    [InlineData("/api/v1/vms/alpha/guest/channel/verify", "vm.guest.channel.verify", """{"credential_ref":"wincred:PureCVisor/guest/admin"}""")]
    [InlineData("/api/v1/vms/alpha/guest/channel", "vm.guest.channel.ensure", """{"mode":"repair","yes":true}""")]
    public void GuestExecutionQueuedRoutesCreateJobsAndDispatchNativeProvider(
        string path,
        string nativeOperation,
        string body)
    {
        var calls = new List<string>();
        var providerResponse = JsonSerializer.Serialize(new
        {
            ok = true,
            operation = nativeOperation,
            data = new
            {
                name = "alpha",
                terminal_state = "succeeded",
                audit = new
                {
                    schema_version = "guest-execution-audit-v1"
                }
            }
        });
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(
                calls,
                nativeOperation,
                providerResponse));

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            path,
            body,
            RequestId: "req-guest-queue",
            ClientIdentity: "operator-a"));

        Assert.Equal(202, queued.StatusCode);
        Assert.DoesNotContain("PureCVisor/guest/admin", queued.Body, StringComparison.Ordinal);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            var data = queuedDocument.RootElement.GetProperty("data");
            Assert.Equal(nativeOperation, data.GetProperty("operation").GetString());
            Assert.Equal("queued", data.GetProperty("status").GetString());
            Assert.Equal("req-guest-queue", data.GetProperty("request_id").GetString());
            var parameters = data.GetProperty("params");
            Assert.Equal("alpha", parameters.GetProperty("name").GetString());
            Assert.True(parameters.TryGetProperty("audit_preview", out var auditPreview));
            Assert.Equal("guest-execution-audit-v1", auditPreview.GetProperty("schema_version").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Equal([nativeOperation], calls);
        Assert.DoesNotContain("PureCVisor/guest/admin", tick.Job!.Value.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public void GuestExecutionQueuedRouteRejectsSecretLikeCommandBeforePersistingJob()
    {
        var calls = new List<string>();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(calls, handledOperation: null, responseJson: null));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec",
            """{"command":["powershell","--password=super-secret-value"],"credential_ref":"wincred:PureCVisor/guest/admin"}""",
            RequestId: "req-guest-secret"));

        Assert.Equal(400, response.StatusCode);
        Assert.Empty(calls);
        Assert.Contains("PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED", response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret-value", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PureCVisor/guest/admin", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunningGuestExecutionJobCancelRequestsProviderCancellationAndFinishesCanceled()
    {
        var adapter = new BlockingGuestExecutionNativeAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: adapter);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec",
            """{"command":["powershell","Start-Sleep","60"],"credential_ref":"wincred:PureCVisor/guest/admin","timeout_sec":60}""",
            RequestId: "req-running-cancel"));

        using var queuedDocument = JsonDocument.Parse(queued.Body);
        var jobId = queuedDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString();
        Assert.False(string.IsNullOrWhiteSpace(jobId));

        var tickTask = Task.Run(() => processor.ProcessOneQueuedJob());
        Assert.Same(adapter.Started.Task, await Task.WhenAny(adapter.Started.Task, Task.Delay(TimeSpan.FromSeconds(5))));

        var cancel = processor.Handle(new DesktopNodeApiRequest("POST", $"/api/v1/jobs/{jobId}/cancel"));

        Assert.Equal(202, cancel.StatusCode);
        using (var cancelDocument = JsonDocument.Parse(cancel.Body))
        {
            var data = cancelDocument.RootElement.GetProperty("data");
            Assert.Equal("running", data.GetProperty("status").GetString());
            Assert.NotEqual(JsonValueKind.Null, data.GetProperty("canceled_at").ValueKind);
        }

        Assert.Same(adapter.Canceled.Task, await Task.WhenAny(adapter.Canceled.Task, Task.Delay(TimeSpan.FromSeconds(5))));
        var completed = await Task.WhenAny(tickTask, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(tickTask, completed);
        var tick = await tickTask;

        using var tickDocument = JsonDocument.Parse(tick.Job!.Value.GetRawText());
        Assert.Equal("canceled", tickDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal("PCV_JOB_CANCELED", tickDocument.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    private static void AssertCheckpointRestoreReconcileRequired(string listJson, string classification)
    {
        var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-checkpoint-restore-required-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(jobStorePath, InterruptedCheckpointRestoreJobStoreJson());
            var nativeCalls = new List<string>();
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(
                jobStorePath: jobStorePath,
                nativeAdapter: new RecordingNativeHyperVAdapter(nativeCalls, new Dictionary<string, string>
                {
                    ["checkpoint.list"] = listJson
                }));

            var response = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/jobs/job-checkpoint-restore-reconcile/reconcile"));

            Assert.Equal(409, response.StatusCode);
            Assert.Equal(["checkpoint.list"], nativeCalls);
            Assert.DoesNotContain("checkpoint.restore", nativeCalls);
            using var document = JsonDocument.Parse(response.Body);
            Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
            Assert.Contains(classification, document.RootElement.GetProperty("error").GetProperty("detail").GetString(), StringComparison.Ordinal);
            Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.False(document.RootElement.GetProperty("error").GetProperty("retryable").GetBoolean());
        }
        finally
        {
            if (File.Exists(jobStorePath))
            {
                File.Delete(jobStorePath);
            }
        }
    }

    private static string InterruptedCheckpointRestoreJobStoreJson(string? reconciliationJson = null)
    {
        reconciliationJson ??= """
        {
          "schema": "pcv-checkpoint-restore-reconciliation/v1",
          "capture_status": "captured",
          "before": { "current_name": "old", "vm_name": "lab-vm" },
          "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true }
        }
        """;

        return $$"""
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-checkpoint-restore-reconcile",
              "operation": "checkpoint.restore",
              "status": "failed",
              "params": {
                "vm_name": "lab-vm",
                "checkpoint_name": "requested",
                "reconciliation": {{reconciliationJson}}
              },
              "result": null,
              "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
              "retry_of": null,
              "request_id": "req-checkpoint-restore-reconcile",
              "correlation_id": "corr-checkpoint-restore-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-08-14T00:00:00.0000000Z",
              "updated_at": "2026-08-14T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

}
