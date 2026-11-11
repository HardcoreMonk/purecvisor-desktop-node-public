using System.Diagnostics;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiConsoleLockBypassTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("pcv-console-lock-").FullName;

    public void Dispose() => Directory.Delete(root, recursive: true);

    [Fact]
    public void ConsoleInputFinishesWhileAConsoleFrameIsStillRunning()
    {
        // 고치기 전에는 frame 처리 전체가 전역 lock 안이라 input 이 frame 을 기다렸다.
        // frame 의 느린 adapter 작업은 이제 lock 밖이다. input 은 그 작업을 기다리지 않는다.
        var frameEntered = new ManualResetEventSlim(false);
        var releaseFrame = new ManualResetEventSlim(false);
        var processor = CreateProcessor(new BlockingFrameAdapter(frameEntered, releaseFrame), null);
        var frame = Task.Factory.StartNew(
            () => processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/vms/lab-vm/console/frame/160x120",
                ServiceBearerAccepted: true)),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        Assert.True(frameEntered.Wait(TimeSpan.FromSeconds(5)));

        var watch = Stopwatch.StartNew();
        var response = Send(processor, """{"kind":"key","action":"type","key_code":13}""");
        watch.Stop();

        Assert.Equal(200, response.StatusCode);
        Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(1_000), $"console input waited {watch.Elapsed}");
        Assert.False(frame.IsCompleted);
        releaseFrame.Set();
        Assert.True(frame.Wait(TimeSpan.FromSeconds(5)));
        Assert.Equal(200, frame.Result.StatusCode);
    }

    [Fact]
    public void ConsoleInputDoesNotHoldTheGlobalLockWhileTheAdapterRuns()
    {
        var entered = new ManualResetEventSlim(false);
        var release = new ManualResetEventSlim(false);
        var processor = CreateProcessor(
            new BlockingInputAdapter(entered, release),
            new DesktopNodeApiHardeningOptions(ControlledRouteTimeoutProbeDelayMilliseconds: 300));
        var input = Task.Factory.StartNew(
            () => Send(processor, """{"kind":"key","action":"type","key_code":13}"""),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));

        var probe = Task.Factory.StartNew(
            () => processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/runtime/route-timeout-probe",
                ServiceBearerAccepted: true)),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        Assert.True(probe.Wait(TimeSpan.FromSeconds(3)));
        Assert.False(input.IsCompleted);
        Assert.Equal(200, probe.Result.StatusCode);

        release.Set();
        Assert.True(input.Wait(TimeSpan.FromSeconds(5)));
        Assert.Equal(200, input.Result.StatusCode);
    }

    [Fact]
    public void ConcurrentConsoleInputKeepsPerVmArrivalOrder()
    {
        var provider = new OrderingRunStateProvider();
        var adapter = new DesktopNodeHyperVNativeAdapter(new NoSwitches(), provider);
        var processor = CreateProcessor(adapter, null);
        var press = Task.Factory.StartNew(
            () => Send(processor, """{"kind":"key","action":"press","key_code":13}""", "lane-a"),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        Assert.True(provider.FirstEntered.Wait(TimeSpan.FromSeconds(5)));
        var release = Task.Factory.StartNew(
            () => Send(processor, """{"kind":"key","action":"release","key_code":13}""", "lane-a"),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        var side = Task.Factory.StartNew(
            () => Send(processor, """{"kind":"key","action":"press","key_code":1}""", "lane-b"),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        Assert.True(side.Wait(TimeSpan.FromSeconds(5)));
        Thread.Sleep(100);
        lock (provider.Order)
        {
            Assert.Contains("lane-b:PressKey", provider.Order);
            Assert.DoesNotContain("lane-a:ReleaseKey", provider.Order);
        }

        provider.ReleaseFirst.Set();
        Assert.True(Task.WaitAll([press, release], TimeSpan.FromSeconds(5)));
        Assert.Equal(200, press.Result.StatusCode);
        Assert.Equal(200, side.Result.StatusCode);
        Assert.Equal(200, release.Result.StatusCode);
        lock (provider.Order)
        {
            Assert.Equal(new[] { "lane-a:PressKey", "lane-b:PressKey", "lane-a:ReleaseKey" }, provider.Order);
        }

        Assert.Equal(0, provider.GetVmsCalls);
    }

    [Fact]
    public void ConsoleInputIgnoresTheGlobalRequestRateLimit()
    {
        var clock = new DateTimeOffset(2026, 11, 11, 0, 0, 0, TimeSpan.Zero);
        var processor = CreateProcessor(
            new AcceptingInputAdapter(),
            new DesktopNodeApiHardeningOptions(
                RequestLimitPerMinute: 1,
                BurstLimit: 0,
                RetryAfterSeconds: 7,
                Clock: () => clock));

        var first = Send(processor, """{"kind":"key","action":"type","key_code":13}""", clientIdentity: "console-client");
        var second = Send(processor, """{"kind":"key","action":"type","key_code":13}""", clientIdentity: "console-client");

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(200, second.StatusCode);
        Assert.DoesNotContain("PCV_RATE_LIMIT_EXCEEDED", first.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("PCV_RATE_LIMIT_EXCEEDED", second.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("PCV_CONSOLE_RATE_LIMITED", second.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ConsoleRequestsDoNotConsumeAnotherRoutesGlobalBudget()
    {
        var clock = new DateTimeOffset(2026, 11, 11, 0, 0, 0, TimeSpan.Zero);
        var processor = CreateProcessor(
            new AcceptingInputAdapter(),
            new DesktopNodeApiHardeningOptions(
                RequestLimitPerMinute: 1,
                BurstLimit: 0,
                RetryAfterSeconds: 7,
                Clock: () => clock));

        for (var i = 0; i < 5; i++)
        {
            var input = Send(processor, """{"kind":"key","action":"type","key_code":13}""", clientIdentity: "console-client");
            Assert.Equal(200, input.StatusCode);
            Assert.DoesNotContain("PCV_RATE_LIMIT_EXCEEDED", input.Body, StringComparison.Ordinal);
        }

        var summary = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/ops/summary",
            ServiceBearerAccepted: true,
            RemoteIsLoopback: true,
            ClientIdentity: "console-client"));
        Assert.Equal(200, summary.StatusCode);
        Assert.DoesNotContain("PCV_RATE_LIMIT_EXCEEDED", summary.Body, StringComparison.Ordinal);

        var policy = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/runtime/policy",
            ServiceBearerAccepted: true,
            RemoteIsLoopback: true,
            ClientIdentity: "console-client"));
        Assert.Equal(429, policy.StatusCode);
        Assert.Equal("application/problem+json", policy.ContentType);
        Assert.Contains("PCV_RATE_LIMIT_EXCEEDED", policy.Body, StringComparison.Ordinal);
    }

    private DesktopNodeApiRequestProcessor CreateProcessor(
        IDesktopNodeHyperVNativeAdapter adapter,
        DesktopNodeApiHardeningOptions? hardening) =>
        DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: adapter,
            consoleOptions: new DesktopNodeConsoleOptions(NoVncTargetFilePath: Path.Combine(root, "novnc-target.json")),
            hardeningOptions: hardening);

    private static DesktopNodeApiResponse Send(
        DesktopNodeApiRequestProcessor processor,
        string body,
        string vm = "lab-vm",
        string? clientIdentity = null) =>
        processor.Handle(new DesktopNodeApiRequest(
            "POST",
            $"/api/v1/vms/{vm}/console/input",
            body,
            ServiceBearerAccepted: true,
            RemoteIsLoopback: true,
            ClientIdentity: clientIdentity));

    private sealed class BlockingFrameAdapter(ManualResetEventSlim entered, ManualResetEventSlim release) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            if (operation == "vm.console.frame")
            {
                entered.Set();
                release.Wait(cancellationToken);
                result = new DesktopNodeHyperVOperationResult(
                    true,
                    operation,
                    JsonSerializer.SerializeToElement(new Dictionary<string, object?>
                    {
                        ["name"] = parameters.GetProperty("vm_name").GetString(),
                        ["width"] = parameters.GetProperty("width").GetInt32(),
                        ["height"] = parameters.GetProperty("height").GetInt32(),
                        ["frame_base64"] = Convert.ToBase64String([1, 2, 3, 4])
                    }),
                    null);
                return true;
            }

            if (operation == "vm.console.input")
            {
                result = Accepted(parameters);
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_NATIVE_ROUTE_NOT_HANDLED", "not handled", "test", false);
            return false;
        }
    }

    private sealed class AcceptingInputAdapter : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            result = operation == "vm.console.input"
                ? Accepted(parameters)
                : DesktopNodeHyperVOperationResult.Failure(operation, "PCV_NATIVE_ROUTE_NOT_HANDLED", "not handled", "test", false);
            return operation == "vm.console.input";
        }
    }

    private sealed class BlockingInputAdapter(ManualResetEventSlim entered, ManualResetEventSlim release) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            if (operation != "vm.console.input")
            {
                result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_NATIVE_ROUTE_NOT_HANDLED", "not handled", "test", false);
                return false;
            }

            entered.Set();
            release.Wait(cancellationToken);
            result = Accepted(parameters);
            return true;
        }
    }

    private static DesktopNodeHyperVOperationResult Accepted(JsonElement parameters) =>
        new(
            true,
            "vm.console.input",
            JsonSerializer.SerializeToElement(new Dictionary<string, object?>
            {
                ["name"] = parameters.GetProperty("vm_name").GetString(),
                ["kind"] = parameters.GetProperty("kind").GetString(),
                ["action"] = parameters.GetProperty("action").GetString(),
                ["accepted"] = true
            }),
            null);

    private sealed class NoSwitches : IDesktopNodeHyperVSwitchProvider
    {
        public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken) => [];
    }

    private sealed class OrderingRunStateProvider : IDesktopNodeHyperVVmProvider, IDesktopNodeHyperVVmRunStateProvider
    {
        public List<string> Order { get; } = [];

        public int GetVmsCalls { get; private set; }

        public ManualResetEventSlim FirstEntered { get; } = new(false);

        public ManualResetEventSlim ReleaseFirst { get; } = new(false);

        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
        {
            GetVmsCalls++;
            return [];
        }

        public DesktopNodeHyperVVmRunState? FindVmRunState(string vmName, CancellationToken cancellationToken) =>
            new(vmName, vmName, "running");

        public uint? SendConsoleKeyboard(string vmId, string method, object? argument, CancellationToken cancellationToken)
        {
            var label = $"{vmId}:{method}";
            lock (Order)
            {
                Order.Add(label);
            }

            if (label == "lane-a:PressKey")
            {
                FirstEntered.Set();
                ReleaseFirst.Wait(cancellationToken);
            }

            return 0;
        }
    }
}
