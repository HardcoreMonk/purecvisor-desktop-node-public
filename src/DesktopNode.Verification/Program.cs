namespace DesktopNode.Verification;

internal sealed class ConsoleCancellationBridge : IDisposable
{
    private readonly object gate = new();
    private readonly CancellationTokenSource source = new();
    private readonly CancellationToken token;
    private int activeSignals;
    private bool disposeRequested;
    private bool sourceDisposed;

    internal ConsoleCancellationBridge()
    {
        token = source.Token;
        Handler = HandleCancel;
    }

    internal CancellationToken Token => token;
    internal ConsoleCancelEventHandler Handler { get; }

    internal void Signal()
    {
        lock (gate)
        {
            if (disposeRequested)
            {
                return;
            }

            activeSignals++;
        }

        try
        {
            try
            {
                source.Cancel();
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
            }
        }
        finally
        {
            var disposeSource = false;
            lock (gate)
            {
                activeSignals--;
                if (disposeRequested && activeSignals == 0 && !sourceDisposed)
                {
                    sourceDisposed = true;
                    disposeSource = true;
                }
            }

            if (disposeSource)
            {
                DisposeSource();
            }
        }
    }

    public void Dispose()
    {
        var disposeSource = false;
        lock (gate)
        {
            if (disposeRequested)
            {
                return;
            }

            disposeRequested = true;
            if (activeSignals == 0 && !sourceDisposed)
            {
                sourceDisposed = true;
                disposeSource = true;
            }
        }

        if (disposeSource)
        {
            DisposeSource();
        }
    }

    private void DisposeSource()
    {
        try
        {
            source.Dispose();
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
        }
    }

    private void HandleCancel(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        try
        {
            eventArgs.Cancel = true;
            Signal();
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
        }
    }

    private static bool IsFatal(Exception exception)
    {
        if (exception is OutOfMemoryException or StackOverflowException or AccessViolationException or
            AppDomainUnloadedException or System.Runtime.InteropServices.SEHException)
        {
            return true;
        }

        return exception is AggregateException aggregate &&
            aggregate.Flatten().InnerExceptions.Any(IsFatal);
    }
}

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new ConsoleCancellationBridge();
        Console.CancelKeyPress += cancellation.Handler;

        try
        {
            var fileSystem = new PhysicalVerificationFileSystem();
            var application = new VerificationApplication(
                new SystemProcessRunner(),
                new UnavailableManagedSuiteRunner(),
                fileSystem,
                new SystemVerificationClock(),
                Directory.GetCurrentDirectory,
                () => Environment.GetEnvironmentVariable("RUNNER_TEMP"),
                () => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            return await application.RunAsync(
                args,
                Console.Out,
                Console.Error,
                cancellation.Token);
        }
        finally
        {
            Console.CancelKeyPress -= cancellation.Handler;
        }
    }
}
