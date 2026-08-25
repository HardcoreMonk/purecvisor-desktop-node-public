namespace DesktopNode.Host;

/// <summary>
/// Bounds request work before a request body is read. The legacy listener path does not
/// instantiate this owner; it is enabled only by the explicit tracked async lifetime mode.
/// </summary>
internal sealed class DesktopNodeRequestAdmission : IDisposable
{
    private readonly object sync = new();
    private readonly SemaphoreSlim activeSlots;
    private readonly int waitingLimit;
    private int activeCount;
    private int waitingCount;
    private int disposed;

    public DesktopNodeRequestAdmission(int activeLimit, int waitingLimit)
    {
        if (activeLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(activeLimit));
        }

        if (waitingLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(waitingLimit));
        }

        ActiveLimit = activeLimit;
        this.waitingLimit = waitingLimit;
        activeSlots = new SemaphoreSlim(activeLimit, activeLimit);
    }

    public int ActiveLimit { get; }

    public int WaitingLimit => waitingLimit;

    public int ActiveCount
    {
        get
        {
            lock (sync)
            {
                return activeCount;
            }
        }
    }

    public int WaitingCount
    {
        get
        {
            lock (sync)
            {
                return waitingCount;
            }
        }
    }

    public async ValueTask<Lease?> TryEnterAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(disposed != 0, this);

        if (activeSlots.Wait(0))
        {
            lock (sync)
            {
                activeCount++;
            }

            return new Lease(this);
        }

        lock (sync)
        {
            if (waitingCount >= waitingLimit)
            {
                return null;
            }

            waitingCount++;
        }

        try
        {
            await activeSlots.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            lock (sync)
            {
                waitingCount--;
            }

            throw;
        }

        lock (sync)
        {
            waitingCount--;
            activeCount++;
        }

        return new Lease(this);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        activeSlots.Dispose();
    }

    private void Release()
    {
        lock (sync)
        {
            if (activeCount > 0)
            {
                activeCount--;
            }
        }

        try
        {
            activeSlots.Release();
        }
        catch (ObjectDisposedException)
        {
            // A bounded shutdown may finish the owner after the final request snapshot.
            // The lease has already been removed from the active count, so no slot can leak.
        }
    }

    internal sealed class Lease : IDisposable
    {
        private DesktopNodeRequestAdmission? owner;

        internal Lease(DesktopNodeRequestAdmission owner)
        {
            this.owner = owner;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.Release();
        }
    }
}
