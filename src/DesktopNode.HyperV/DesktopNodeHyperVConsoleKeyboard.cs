using System.Collections.Concurrent;

namespace DesktopNode.HyperV;

// 같은 VM의 콘솔 입력은 도착 순서대로 한 번에 하나다. 다른 VM은 다른 문을 쓴다.
internal sealed class DesktopNodeHyperVConsoleInputLane
{
    public static DesktopNodeHyperVConsoleInputLane Shared { get; } = new();

    private readonly ConcurrentDictionary<string, SemaphoreSlim> gates = new(StringComparer.OrdinalIgnoreCase);

    public Lease Enter(string vmKey, CancellationToken cancellationToken)
    {
        var gate = gates.GetOrAdd(vmKey, static _ => new SemaphoreSlim(1, 1));
        gate.Wait(cancellationToken);
        return new Lease(gate);
    }

    internal sealed class Lease(SemaphoreSlim gate) : IDisposable
    {
        private int released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref released, 1) == 0)
            {
                gate.Release();
            }
        }
    }
}

// Msvm_Keyboard 경로는 VM id로 기억한다. TypeText는 게스트 TUI가 받을 수 있게 글자마다 보낸다.
internal sealed class DesktopNodeHyperVConsoleKeyboardSession
{
    public const int TextCharacterIntervalMilliseconds = 50;

    private readonly ConcurrentDictionary<string, SemaphoreSlim> gates = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> paths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Action<int, CancellationToken> delay;

    public DesktopNodeHyperVConsoleKeyboardSession(Action<int, CancellationToken>? delay = null)
    {
        this.delay = delay ?? DefaultDelay;
    }

    public uint? Send(
        string vmId,
        string method,
        object? argument,
        Func<string?> resolvePath,
        Func<string, string, object?, uint?> invoke,
        CancellationToken cancellationToken)
    {
        var gate = gates.GetOrAdd(vmId, static _ => new SemaphoreSlim(1, 1));
        gate.Wait(cancellationToken);
        try
        {
            if (string.Equals(method, "TypeText", StringComparison.Ordinal) && argument is string text)
            {
                uint? last = 0;
                for (var index = 0; index < text.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (index > 0)
                    {
                        delay(TextCharacterIntervalMilliseconds, cancellationToken);
                    }

                    last = SendOnce(vmId, method, text[index].ToString(), resolvePath, invoke);
                    if (last is null or not 0)
                    {
                        return last;
                    }
                }

                return last;
            }

            return SendOnce(vmId, method, argument, resolvePath, invoke);
        }
        finally
        {
            gate.Release();
        }
    }

    private uint? SendOnce(
        string vmId,
        string method,
        object? argument,
        Func<string?> resolvePath,
        Func<string, string, object?, uint?> invoke)
    {
        if (paths.TryGetValue(vmId, out var cached))
        {
            var cachedAttempt = TryInvoke(cached, method, argument, invoke);
            if (cachedAttempt.Delivered)
            {
                return cachedAttempt.Value;
            }

            paths.TryRemove(vmId, out _);
        }

        var resolved = resolvePath();
        if (string.IsNullOrWhiteSpace(resolved))
        {
            return null;
        }

        paths[vmId] = resolved;
        var fresh = TryInvoke(resolved, method, argument, invoke);
        if (fresh.Delivered)
        {
            return fresh.Value;
        }

        paths.TryRemove(vmId, out _);
        if (fresh.Error is not null)
        {
            throw fresh.Error;
        }

        return null;
    }

    private static Attempt TryInvoke(
        string path,
        string method,
        object? argument,
        Func<string, string, object?, uint?> invoke)
    {
        try
        {
            var value = invoke(path, method, argument);
            return value is null ? new Attempt(false, null, null) : new Attempt(true, value, null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new Attempt(false, null, ex);
        }
    }

    private static void DefaultDelay(int milliseconds, CancellationToken cancellationToken)
    {
        if (milliseconds <= 0)
        {
            return;
        }

        if (cancellationToken.WaitHandle.WaitOne(milliseconds))
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private readonly record struct Attempt(bool Delivered, uint? Value, Exception? Error);
}
