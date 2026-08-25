using DesktopNode.Api;
using DesktopNode.Runtime;

namespace DesktopNode.Api.Tests;

internal sealed class RecordingDesktopNodeApiJobStore : IDesktopNodeJobStore
{
    private readonly IList<string> trace;
    private string? durableSnapshot;
    private int writeAttempt;

    public RecordingDesktopNodeApiJobStore(
        string? initialSnapshot = null,
        IList<string>? trace = null,
        string location = "recording://desktop-node/jobs.json")
    {
        durableSnapshot = initialSnapshot;
        this.trace = trace ?? new List<string>();
        Location = location;
    }

    public string Location { get; }

    public int? FailOnWriteAttempt { get; set; }

    public int? IndeterminateOnWriteAttempt { get; set; }

    public string? DurableSnapshot => durableSnapshot;

    public List<string> AttemptedSnapshots { get; } = [];

    public List<string> QuarantineSuffixes { get; } = [];

    public bool Exists()
    {
        trace.Add("store.exists");
        return durableSnapshot is not null;
    }

    public string ReadSnapshot()
    {
        trace.Add("store.read");
        return durableSnapshot ?? throw new FileNotFoundException("The recording job store has no snapshot.", Location);
    }

    public DesktopNodeJobStoreWriteResult WriteSnapshot(string json)
    {
        writeAttempt++;
        trace.Add($"store.write:{writeAttempt}");
        AttemptedSnapshots.Add(json);
        if (IndeterminateOnWriteAttempt == writeAttempt)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(
                new IOException($"Injected indeterminate job store write at attempt {writeAttempt}."));
        }

        if (FailOnWriteAttempt == writeAttempt)
        {
            return DesktopNodeJobStoreWriteResult.NotCommitted(
                new IOException($"Injected job store write failure at attempt {writeAttempt}."));
        }

        durableSnapshot = json;
        return DesktopNodeJobStoreWriteResult.Committed;
    }

    public void Quarantine(string suffix)
    {
        trace.Add("store.quarantine");
        QuarantineSuffixes.Add(suffix);
        durableSnapshot = null;
    }
}

internal sealed class SequenceDesktopNodeApiClock : IDesktopNodeJobClock
{
    private readonly Queue<DateTimeOffset> values;
    private readonly IList<string>? trace;
    private DateTimeOffset last;
    private int readCount;

    public SequenceDesktopNodeApiClock(IEnumerable<DateTimeOffset> values, IList<string>? trace = null)
    {
        this.values = new Queue<DateTimeOffset>(values);
        if (this.values.Count == 0)
        {
            throw new ArgumentException("At least one clock value is required.", nameof(values));
        }

        last = this.values.Peek();
        this.trace = trace;
    }

    public DateTimeOffset UtcNow
    {
        get
        {
            readCount++;
            trace?.Add($"clock:{readCount}");
            if (values.Count > 0)
            {
                last = values.Dequeue();
            }

            return last;
        }
    }
}

internal sealed class RecordingDesktopNodeApiCancellationScopeFactory : IDesktopNodeApiCancellationScopeFactory
{
    private readonly IList<string> trace;

    public RecordingDesktopNodeApiCancellationScopeFactory(IList<string>? trace = null)
    {
        this.trace = trace ?? new List<string>();
    }

    public List<RecordingDesktopNodeApiCancellationScope> RouteScopes { get; } = [];

    public List<RecordingDesktopNodeApiCancellationScope> LinkedJobScopes { get; } = [];

    public IDesktopNodeApiCancellationScope CreateRouteTimeoutScope()
    {
        trace.Add("cancellation.route.create");
        var scope = new RecordingDesktopNodeApiCancellationScope(
            new CancellationTokenSource(),
            "cancellation.route",
            trace);
        RouteScopes.Add(scope);
        return scope;
    }

    public IDesktopNodeApiCancellationScope CreateLinkedJobScope(CancellationToken parent)
    {
        trace.Add("cancellation.job.create");
        var scope = new RecordingDesktopNodeApiCancellationScope(
            CancellationTokenSource.CreateLinkedTokenSource(parent),
            "cancellation.job",
            trace);
        LinkedJobScopes.Add(scope);
        return scope;
    }
}

internal sealed class RecordingDesktopNodeApiCancellationScope(
    CancellationTokenSource source,
    string traceName,
    IList<string> trace) : IDesktopNodeApiCancellationScope
{
    private readonly CancellationToken token = source.Token;
    private int disposed;

    public CancellationToken Token => token;

    public bool IsCancellationRequested => token.IsCancellationRequested;

    public bool Disposed => Volatile.Read(ref disposed) != 0;

    public void Cancel()
    {
        trace.Add(traceName + ".cancel");
        source.Cancel();
    }

    public void Dispose()
    {
        trace.Add(traceName + ".dispose");
        source.Dispose();
        Volatile.Write(ref disposed, 1);
    }
}
