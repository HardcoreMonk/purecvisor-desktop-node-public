using System.Text.Json;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVConsoleKeyboardTests
{
    // The ordering tests block worker threads on purpose. On a small CI runner the shared thread pool can be
    // starved by parallel tests, so each worker gets its own thread and the waits allow a slow machine.
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(30);

    private static Task<T> RunDedicated<T>(Func<T> work) =>
        Task.Factory.StartNew(work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private static Task RunDedicated(Action work) =>
        Task.Factory.StartNew(work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    [Fact]
    public void KeyboardPathIsResolvedOnceAndLookedUpAgainOnlyAfterTheCachedPathFails()
    {
        var session = new DesktopNodeHyperVConsoleKeyboardSession(static (_, _) => { });
        var resolves = 0;
        var invoked = new List<string>();
        var failStale = false;

        uint? Invoke(string path, string method, object? argument)
        {
            invoked.Add($"{path}:{method}:{argument}");
            if (failStale && path == "path-a")
            {
                throw new InvalidOperationException("stale");
            }

            return 0;
        }

        string? Resolve()
        {
            resolves += 1;
            return resolves == 1 ? "path-a" : "path-b";
        }

        Assert.Equal(0u, session.Send("vm-1", "PressKey", 1u, Resolve, Invoke, CancellationToken.None));
        Assert.Equal(0u, session.Send("vm-1", "PressKey", 2u, Resolve, Invoke, CancellationToken.None));
        Assert.Equal(0u, session.Send("vm-1", "ReleaseKey", 2u, Resolve, Invoke, CancellationToken.None));
        Assert.Equal(1, resolves);
        Assert.Equal(
            new[] { "path-a:PressKey:1", "path-a:PressKey:2", "path-a:ReleaseKey:2" },
            invoked);

        failStale = true;
        Assert.Equal(0u, session.Send("vm-1", "PressKey", 3u, Resolve, Invoke, CancellationToken.None));
        Assert.Equal(2, resolves);
        Assert.Equal("path-b:PressKey:3", invoked[^1]);

        var retryResolves = 0;
        var failed = Assert.Throws<InvalidOperationException>(() => session.Send(
            "vm-1",
            "PressKey",
            4u,
            () =>
            {
                retryResolves += 1;
                return "path-c";
            },
            (_, _, _) => throw new InvalidOperationException("dead"),
            CancellationToken.None));
        Assert.Equal("dead", failed.Message);
        Assert.Equal(1, retryResolves);
    }

    [Fact]
    public void SameVmInputsRunInArrivalOrderAndOtherVmsDoNotWait()
    {
        var session = new DesktopNodeHyperVConsoleKeyboardSession(static (_, _) => { });
        var order = new List<string>();
        var firstEntered = new ManualResetEventSlim(false);
        var releaseFirst = new ManualResetEventSlim(false);
        var otherDone = new ManualResetEventSlim(false);

        var first = RunDedicated(() => session.Send(
            "vm-a",
            "PressKey",
            13u,
            () => "path-a",
            (_, method, _) =>
            {
                lock (order) order.Add(method);
                firstEntered.Set();
                releaseFirst.Wait();
                return 0u;
            },
            CancellationToken.None));

        Assert.True(firstEntered.Wait(WaitLimit));
        var secondStarted = new ManualResetEventSlim(false);
        var second = RunDedicated(() =>
        {
            secondStarted.Set();
            return session.Send(
                "vm-a",
                "ReleaseKey",
                13u,
                () => "path-a",
                (_, method, _) =>
                {
                    lock (order) order.Add(method);
                    return 0u;
                },
                CancellationToken.None);
        });
        Assert.True(secondStarted.Wait(WaitLimit));
        Thread.Sleep(100);
        lock (order)
        {
            Assert.Equal(new[] { "PressKey" }, order);
        }

        var other = RunDedicated(() => session.Send(
            "vm-b",
            "PressKey",
            1u,
            () => "path-b",
            (_, _, _) =>
            {
                otherDone.Set();
                return 0u;
            },
            CancellationToken.None));
        Assert.True(otherDone.Wait(WaitLimit));
        releaseFirst.Set();
        Assert.True(Task.WaitAll([first, second, other], WaitLimit));
        lock (order)
        {
            Assert.Equal(new[] { "PressKey", "ReleaseKey" }, order);
        }
    }

    [Fact]
    public void TypeTextIsOneCharacterAtATimeWithTheGuestInterval()
    {
        var delays = new List<int>();
        var session = new DesktopNodeHyperVConsoleKeyboardSession((milliseconds, _) => delays.Add(milliseconds));
        var characters = new List<string>();
        var resolves = 0;

        var sent = session.Send(
            "vm-1",
            "TypeText",
            "ab",
            () =>
            {
                resolves += 1;
                return "path";
            },
            (_, method, argument) =>
            {
                Assert.Equal("TypeText", method);
                characters.Add(Assert.IsType<string>(argument));
                return 0u;
            },
            CancellationToken.None);

        Assert.Equal(0u, sent);
        Assert.Equal(50, DesktopNodeHyperVConsoleKeyboardSession.TextCharacterIntervalMilliseconds);
        Assert.Equal(new[] { "a", "b" }, characters);
        Assert.Equal(new[] { 50 }, delays);
        Assert.Equal(1, resolves);

        characters.Clear();
        delays.Clear();
        var stopped = session.Send(
            "vm-1",
            "TypeText",
            "xyz",
            () => "path",
            (_, _, argument) =>
            {
                var text = Assert.IsType<string>(argument);
                characters.Add(text);
                return text == "y" ? 5u : 0u;
            },
            CancellationToken.None);
        Assert.Equal(5u, stopped);
        Assert.Equal(new[] { "x", "y" }, characters);
        Assert.Equal(new[] { 50 }, delays);
    }

    [Fact]
    public void AdapterKeepsSameVmConsoleInputInArrivalOrder()
    {
        var provider = new OrderingKeyboardProvider(RunningVm("lane-a"), RunningVm("lane-b"));
        var adapter = new DesktopNodeHyperVNativeAdapter(new NoSwitches(), provider);
        using var press = JsonDocument.Parse("""{"name":"lane-a","kind":"key","action":"press","key_code":13}""");
        using var release = JsonDocument.Parse("""{"name":"lane-a","kind":"key","action":"release","key_code":13}""");
        using var other = JsonDocument.Parse("""{"name":"lane-b","kind":"key","action":"press","key_code":1}""");

        var first = RunDedicated(() => adapter.TryInvoke("vm.console.input", press.RootElement, CancellationToken.None, out _));
        Assert.True(provider.FirstEntered.Wait(WaitLimit));
        var second = RunDedicated(() => adapter.TryInvoke("vm.console.input", release.RootElement, CancellationToken.None, out _));
        var side = RunDedicated(() => adapter.TryInvoke("vm.console.input", other.RootElement, CancellationToken.None, out _));
        Assert.True(side.Wait(WaitLimit));
        Thread.Sleep(100);
        lock (provider.Order)
        {
            Assert.Contains("lane-b:PressKey", provider.Order);
            Assert.DoesNotContain("lane-a:ReleaseKey", provider.Order);
        }

        provider.ReleaseFirst.Set();
        Assert.True(Task.WaitAll([first, second], WaitLimit));
        lock (provider.Order)
        {
            Assert.Equal(new[] { "lane-a:PressKey", "lane-b:PressKey", "lane-a:ReleaseKey" }, provider.Order);
        }
    }

    [Fact]
    public void AdapterConsoleInputReadsOneVmRunStateInsteadOfTheFullList()
    {
        var provider = new RunStateCountingProvider(new DesktopNodeHyperVVmRunState("lane-a", "lane-a", "running"));
        var adapter = new DesktopNodeHyperVNativeAdapter(new NoSwitches(), provider);
        using var press = JsonDocument.Parse("""{"name":"lane-a","kind":"key","action":"type","key_code":13}""");
        for (var index = 0; index < 4; index++)
        {
            Assert.True(adapter.TryInvoke("vm.console.input", press.RootElement, CancellationToken.None, out var result));
            Assert.True(result.Ok);
        }

        Assert.Equal(0, provider.GetVmsCalls);
        Assert.Equal(4, provider.FindCalls);

        var stopped = new RunStateCountingProvider(new DesktopNodeHyperVVmRunState("lane-a", "Lane-A", "stopped"));
        var stoppedAdapter = new DesktopNodeHyperVNativeAdapter(new NoSwitches(), stopped);
        Assert.True(stoppedAdapter.TryInvoke("vm.console.input", press.RootElement, CancellationToken.None, out var refused));
        Assert.Equal("PCV_CONSOLE_VM_NOT_RUNNING", refused.Error!.Code);
        Assert.Equal(0, stopped.GetVmsCalls);

        var missing = new RunStateCountingProvider(null);
        var missingAdapter = new DesktopNodeHyperVNativeAdapter(new NoSwitches(), missing);
        Assert.True(missingAdapter.TryInvoke("vm.console.input", press.RootElement, CancellationToken.None, out var absent));
        Assert.Equal("PCV_VM_NOT_FOUND", absent.Error!.Code);
        Assert.Equal(0, missing.GetVmsCalls);
    }

    private static DesktopNodeHyperVVmInfo RunningVm(string name)
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

    private sealed class NoSwitches : IDesktopNodeHyperVSwitchProvider
    {
        public IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken) => [];
    }

    private sealed class OrderingKeyboardProvider(params DesktopNodeHyperVVmInfo[] vms) : IDesktopNodeHyperVVmProvider
    {
        public List<string> Order { get; } = [];

        public ManualResetEventSlim FirstEntered { get; } = new(false);

        public ManualResetEventSlim ReleaseFirst { get; } = new(false);

        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken) => vms;

        public uint? SendConsoleKeyboard(string vmId, string method, object? argument, CancellationToken cancellationToken)
        {
            var label = $"{vmId}:{method}";
            lock (Order) Order.Add(label);
            if (label == "lane-a:PressKey")
            {
                FirstEntered.Set();
                ReleaseFirst.Wait();
            }

            return 0;
        }
    }

    private sealed class RunStateCountingProvider(DesktopNodeHyperVVmRunState? state) : IDesktopNodeHyperVVmProvider, IDesktopNodeHyperVVmRunStateProvider
    {
        public int GetVmsCalls { get; private set; }

        public int FindCalls { get; private set; }

        public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
        {
            GetVmsCalls++;
            return [];
        }

        public DesktopNodeHyperVVmRunState? FindVmRunState(string vmName, CancellationToken cancellationToken)
        {
            FindCalls++;
            if (state is null)
            {
                return null;
            }

            return string.Equals(vmName, state.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(vmName, state.Id, StringComparison.OrdinalIgnoreCase)
                ? state
                : null;
        }

        public uint? SendConsoleKeyboard(string vmId, string method, object? argument, CancellationToken cancellationToken) => 0;
    }
}
