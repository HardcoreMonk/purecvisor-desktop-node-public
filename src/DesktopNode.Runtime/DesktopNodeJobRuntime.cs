using System.Collections.Concurrent;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Runtime;

public sealed partial class DesktopNodeJobRuntime
{
    public const int MaxRetainedTerminalJobs = 500;
    private const int MaxRecentObservations = 32;

    private readonly object stateSync = new();
    private readonly IDesktopNodeJobStore? store;
    private readonly IDesktopNodeJobClock clock;
    private readonly IDesktopNodeJobRuntimeEventSink? eventSink;
    private readonly List<DesktopNodeJobRuntimeObservation> recentObservations = [];
    private readonly ConcurrentQueue<DesktopNodeJobRuntimeObservation> pendingSinkObservations = new();
    private readonly HashSet<string> cancelSignalAttentionJobIds = new(StringComparer.Ordinal);
    private Dictionary<string, MutableJob> jobs = new(StringComparer.Ordinal);
    private Queue<string> queue = new();
    private readonly Dictionary<string, Action> runningCancellations = new(StringComparer.Ordinal);
    private DesktopNodeJobRuntimeError? loadBlock;
    private int schemaVersion = 1;
    private int prunedTerminalJobs;
    private int pendingSinkObservationCount;
    private int sinkDrainScheduled;
    private bool storeWriteAttentionRequired;
    private bool storeRecovered;

    public DesktopNodeJobRuntime(
        IDesktopNodeJobStore? store = null,
        IDesktopNodeJobClock? clock = null,
        IDesktopNodeJobRuntimeEventSink? eventSink = null)
    {
        this.store = store;
        this.clock = clock ?? SystemDesktopNodeJobClock.Instance;
        this.eventSink = eventSink;
        LoadUnsafe();
    }

    public static DesktopNodeJobRuntime CreateDefault(
        string? jobStorePath = null,
        IDesktopNodeJobRuntimeEventSink? eventSink = null)
    {
        return new DesktopNodeJobRuntime(
            string.IsNullOrWhiteSpace(jobStorePath)
                ? null
                : new JsonFileDesktopNodeJobStore(jobStorePath),
            eventSink: eventSink);
    }

    public DesktopNodeJobRuntimeError? LoadBlock
    {
        get
        {
            lock (stateSync)
            {
                return loadBlock;
            }
        }
    }

    public DesktopNodeJobSnapshot Create(
        DesktopNodeJobCreateCommand command,
        DesktopNodeJobRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        lock (stateSync)
        {
            return CreateUnsafe(command, context);
        }
    }

    public DesktopNodeJobCommandResult Get(string jobId)
    {
        lock (stateSync)
        {
            return jobs.TryGetValue(jobId, out var job)
                ? new DesktopNodeJobCommandResult(
                    DesktopNodeJobCommandOutcome.Found,
                    Project(job),
                    null)
                : NotFound(jobId);
        }
    }

    public DesktopNodeJobRuntimeSnapshot Snapshot()
    {
        lock (stateSync)
        {
            var observations = recentObservations.ToArray();
            var storeHealthStatus = loadBlock is not null
                ? "blocked"
                : storeWriteAttentionRequired || cancelSignalAttentionJobIds.Count > 0
                    ? "attention-required"
                    : storeRecovered
                        ? "recovered"
                        : "healthy";
            return new DesktopNodeJobRuntimeSnapshot(
                jobs.Values.Select(Project).ToArray(),
                queue.ToArray(),
                schemaVersion,
                prunedTerminalJobs,
                MaxRetainedTerminalJobs,
                loadBlock,
                new DesktopNodeJobStoreHealthSnapshot(
                    storeHealthStatus,
                    loadBlock is not null,
                    loadBlock?.Code,
                    observations));
        }
    }

}
