using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal interface IDesktopNodeApiCancellationScope : IDisposable
{
    CancellationToken Token { get; }

    bool IsCancellationRequested { get; }

    void Cancel();
}

internal interface IDesktopNodeApiCancellationScopeFactory
{
    IDesktopNodeApiCancellationScope CreateRouteTimeoutScope();

    IDesktopNodeApiCancellationScope CreateLinkedJobScope(CancellationToken parent);
}

internal sealed class SystemDesktopNodeApiCancellationScopeFactory : IDesktopNodeApiCancellationScopeFactory
{
    public static SystemDesktopNodeApiCancellationScopeFactory Instance { get; } = new();

    private SystemDesktopNodeApiCancellationScopeFactory()
    {
    }

    public IDesktopNodeApiCancellationScope CreateRouteTimeoutScope()
    {
        return new SystemDesktopNodeApiCancellationScope(new CancellationTokenSource());
    }

    public IDesktopNodeApiCancellationScope CreateLinkedJobScope(CancellationToken parent)
    {
        return new SystemDesktopNodeApiCancellationScope(CancellationTokenSource.CreateLinkedTokenSource(parent));
    }
}

internal sealed class SystemDesktopNodeApiCancellationScope(CancellationTokenSource source) : IDesktopNodeApiCancellationScope
{
    public CancellationToken Token => source.Token;

    public bool IsCancellationRequested => source.IsCancellationRequested;

    public void Cancel() => source.Cancel();

    public void Dispose() => source.Dispose();
}

internal sealed record DesktopNodeApiRuntimeDependencies(
    IDesktopNodeJobStore? JobStore = null,
    IDesktopNodeJobClock? JobClock = null,
    IDesktopNodeApiCancellationScopeFactory? CancellationScopes = null,
    DesktopNodeJobRuntime? JobRuntime = null);
