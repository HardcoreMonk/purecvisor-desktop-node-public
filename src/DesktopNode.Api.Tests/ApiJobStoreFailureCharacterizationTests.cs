using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

[CollectionDefinition("API job-store timing isolation", DisableParallelization = true)]
public sealed class ApiJobStoreTimingIsolationCollection
{
}

[Collection("API job-store timing isolation")]
public sealed class ApiJobStoreFailureCharacterizationTests
{
    [Fact]
    public void IndeterminateCreateCommitReturns503AndBlocksJobRoutesAndWorker()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            IndeterminateOnWriteAttempt = 1
        };
        var adapter = new RecordingSuccessAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/start",
            RequestId: "req-indeterminate-create"));

        Assert.Equal(503, response.StatusCode);
        using (var responseDocument = JsonDocument.Parse(response.Body))
        {
            var error = responseDocument.RootElement.GetProperty("error");
            Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", error.GetProperty("code").GetString());
            Assert.Equal("The job store commit outcome is indeterminate.", error.GetProperty("message").GetString());
            Assert.Contains(
                "job-store mutation and dispatch are blocked",
                error.GetProperty("detail").GetString(),
                StringComparison.Ordinal);
            Assert.Contains(
                "restart Desktop Node",
                error.GetProperty("recommended_action").GetString(),
                StringComparison.Ordinal);
            Assert.Contains(
                "Operations Guide pending-commit recovery procedure",
                error.GetProperty("recommended_action").GetString(),
                StringComparison.Ordinal);
            Assert.DoesNotContain("Injected indeterminate", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("recording://", response.Body, StringComparison.Ordinal);
        }

        var list = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        var tick = processor.ProcessOneQueuedJob();

        Assert.Equal(409, list.StatusCode);
        Assert.False(tick.Processed);
        Assert.Equal(0, adapter.InvokeCount);
        Assert.Null(store.DurableSnapshot);
    }

    [Fact]
    public void CreateSaveFailureDoesNotReturn202OrInvokeNativeMutation()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            FailOnWriteAttempt = 1
        };
        var adapter = new RecordingSuccessAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/start",
            RequestId: "req-create-save-failure"));

        Assert.Equal(503, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);
        using (var responseDocument = JsonDocument.Parse(response.Body))
        {
            var root = responseDocument.RootElement;
            Assert.False(root.GetProperty("ok").GetBoolean());
            Assert.Equal("req-create-save-failure", root.GetProperty("request_id").GetString());
            Assert.Equal(
                "PCV_JOB_STORE_SAVE_FAILED",
                root.GetProperty("error").GetProperty("code").GetString());
            Assert.Equal(
                "The job store did not acknowledge the queued job snapshot.",
                root.GetProperty("error").GetProperty("message").GetString());
            Assert.Equal(
                "The candidate snapshot was not published to this process. The on-disk commit state requires recovery if the store failed after replacement.",
                root.GetProperty("error").GetProperty("detail").GetString());
            Assert.False(root.GetProperty("error").GetProperty("retryable").GetBoolean());
            Assert.Contains(
                "confirm that no matching job exists before retrying",
                root.GetProperty("error").GetProperty("recommended_action").GetString(),
                StringComparison.Ordinal);
            Assert.DoesNotContain("attempt 1", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("Injected job store write failure at attempt 1.", response.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("recording://", response.Body, StringComparison.Ordinal);
        }
        Assert.Null(store.DurableSnapshot);
        Assert.Single(store.AttemptedSnapshots);
        using (var attempted = JsonDocument.Parse(store.AttemptedSnapshots[0]))
        {
            var root = attempted.RootElement;
            var job = Assert.Single(root.GetProperty("jobs").EnumerateArray());
            var queuedJobId = Assert.Single(root.GetProperty("queue").EnumerateArray()).GetString();
            Assert.Equal(job.GetProperty("job_id").GetString(), queuedJobId);
            Assert.Equal("queued", job.GetProperty("status").GetString());
        }

        var list = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        using (var listDocument = JsonDocument.Parse(list.Body))
        {
            var data = listDocument.RootElement.GetProperty("data");
            Assert.Equal(0, data.GetProperty("count").GetInt32());
            Assert.Empty(data.GetProperty("jobs").EnumerateArray());
        }

        store.FailOnWriteAttempt = null;
        var tick = processor.ProcessOneQueuedJob();

        Assert.False(tick.Processed);
        Assert.Equal(0, adapter.InvokeCount);
    }

    [Fact]
    public void JobClockAndStoreSnapshotsPreserveCurrentCreateStartCompleteOrder()
    {
        var trace = new List<string>();
        var timestamps = Enumerable.Range(1, 6)
            .Select(second => new DateTimeOffset(2026, 8, 2, 0, 0, second, TimeSpan.Zero))
            .ToArray();
        var store = new RecordingDesktopNodeApiJobStore(trace: trace);
        var clock = new SequenceDesktopNodeApiClock(timestamps, trace);
        var cancellationScopes = new RecordingDesktopNodeApiCancellationScopeFactory(trace);
        var adapter = new RecordingSuccessAdapter(trace);
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(store, clock, cancellationScopes),
            nativeAdapter: adapter);
        trace.Clear();

        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        Assert.Equal(202, create.StatusCode);
        using var createDocument = JsonDocument.Parse(create.Body);
        var jobId = createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString();

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Equal(
            [
                "clock:1",
                "clock:2",
                "store.write:1",
                "cancellation.job.create",
                "clock:3",
                "clock:4",
                "store.write:2",
                "provider.invoke",
                "clock:5",
                "clock:6",
                "store.write:3",
                "cancellation.job.dispose"
            ],
            trace);

        Assert.Equal(3, store.AttemptedSnapshots.Count);
        using var queuedSnapshot = JsonDocument.Parse(store.AttemptedSnapshots[0]);
        using var runningSnapshot = JsonDocument.Parse(store.AttemptedSnapshots[1]);
        using var completedSnapshot = JsonDocument.Parse(store.AttemptedSnapshots[2]);
        AssertSnapshot(queuedSnapshot, jobId!, "queued", [jobId!], timestamps[0], timestamps[0], timestamps[1]);
        AssertSnapshot(runningSnapshot, jobId!, "running", [], timestamps[0], timestamps[2], timestamps[3]);
        AssertSnapshot(completedSnapshot, jobId!, "succeeded", [], timestamps[0], timestamps[4], timestamps[5]);
    }

    [Fact]
    public async Task RunningCancelPersistsBeforeSignalAndUsesLinkedToken()
    {
        var trace = new List<string>();
        var store = new RecordingDesktopNodeApiJobStore(trace: trace);
        var cancellationScopes = new RecordingDesktopNodeApiCancellationScopeFactory(trace);
        var adapter = new BlockingCancellationAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(
                JobStore: store,
                CancellationScopes: cancellationScopes),
            nativeAdapter: adapter);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec",
            """{"command":["cmd","/c","ver"],"credential_ref":"wincred:PureCVisor/guest/admin","timeout_sec":60}"""));
        using var queuedDocument = JsonDocument.Parse(queued.Body);
        var jobId = queuedDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString();

        var tickTask = Task.Run(() => processor.ProcessOneQueuedJob());
        await adapter.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var cancel = processor.Handle(new DesktopNodeApiRequest("POST", $"/api/v1/jobs/{jobId}/cancel"));

        Assert.Equal(202, cancel.StatusCode);
        var cancelSignalIndex = trace.IndexOf("cancellation.job.cancel");
        var cancelSaveIndex = trace.IndexOf("store.write:3");
        Assert.True(cancelSignalIndex >= 0, "The running cancellation scope was not signaled.");
        Assert.True(cancelSaveIndex >= 0, "The running cancellation request was not saved.");
        Assert.True(cancelSignalIndex > cancelSaveIndex, "The durable cancellation request must be saved before provider signal.");
        Assert.Single(cancellationScopes.LinkedJobScopes);
        Assert.Equal(cancellationScopes.LinkedJobScopes[0].Token, adapter.ReceivedToken);

        var tick = await tickTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(tick.Processed);
        Assert.Equal(4, store.AttemptedSnapshots.Count);
        using var cancelRequestedSnapshot = JsonDocument.Parse(store.AttemptedSnapshots[2]);
        using var canceledSnapshot = JsonDocument.Parse(store.AttemptedSnapshots[3]);
        Assert.Equal("running", cancelRequestedSnapshot.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
        Assert.Equal("PCV_JOB_CANCEL_REQUESTED", cancelRequestedSnapshot.RootElement.GetProperty("jobs")[0].GetProperty("error").GetProperty("code").GetString());
        Assert.Equal("canceled", canceledSnapshot.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
    }

    [Fact]
    public void QueuedCancelSaveFailureReturnsStructured503AndPreservesQueuedJob()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            FailOnWriteAttempt = 2
        };
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store));
        var create = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/start",
            RequestId: "req-cancel-store-create"));
        using var createDocument = JsonDocument.Parse(create.Body);
        var jobId = createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        var cancel = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            $"/api/v1/jobs/{jobId}/cancel",
            RequestId: "req-cancel-store-failure"));

        Assert.Equal(503, cancel.StatusCode);
        using (var cancelDocument = JsonDocument.Parse(cancel.Body))
        {
            var root = cancelDocument.RootElement;
            Assert.False(root.GetProperty("ok").GetBoolean());
            Assert.Equal("req-cancel-store-failure", root.GetProperty("request_id").GetString());
            Assert.Equal("job.store", root.GetProperty("operation").GetString());
            var error = root.GetProperty("error");
            Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", error.GetProperty("code").GetString());
            Assert.False(error.GetProperty("retryable").GetBoolean());
            Assert.DoesNotContain("attempt 2", cancel.Body, StringComparison.Ordinal);
            Assert.DoesNotContain("recording://", cancel.Body, StringComparison.Ordinal);
        }

        var get = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{jobId}"));
        using var getDocument = JsonDocument.Parse(get.Body);
        Assert.Equal("queued", getDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
        using var durable = JsonDocument.Parse(store.DurableSnapshot!);
        Assert.Equal("queued", durable.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
        Assert.Equal(jobId, durable.RootElement.GetProperty("queue")[0].GetString());
    }

    [Fact]
    public void CompletionSaveFailureReturnsStructuredWorkerErrorWithoutProviderReplay()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            FailOnWriteAttempt = 3
        };
        var adapter = new RecordingSuccessAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);
        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        using var createDocument = JsonDocument.Parse(create.Body);
        var jobId = createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        var tick = processor.ProcessOneQueuedJob();

        Assert.False(tick.Processed);
        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", tick.Error!.Code);
        Assert.DoesNotContain("attempt 3", tick.Error.Detail, StringComparison.Ordinal);
        Assert.Equal("running", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal(1, adapter.InvokeCount);
        using (var durable = JsonDocument.Parse(store.DurableSnapshot!))
        {
            Assert.Equal("running", durable.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
            Assert.Empty(durable.RootElement.GetProperty("queue").EnumerateArray());
        }

        store.FailOnWriteAttempt = null;
        var nextTick = processor.ProcessOneQueuedJob();
        Assert.False(nextTick.Processed);
        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", nextTick.Error!.Code);
        Assert.Contains("external side effect is unresolved", nextTick.Error.Detail, StringComparison.Ordinal);
        Assert.Equal(1, adapter.InvokeCount);
        var get = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{jobId}"));
        using var getDocument = JsonDocument.Parse(get.Body);
        Assert.Equal(409, get.StatusCode);
        Assert.Equal(
            "PCV_JOB_STORE_SAVE_FAILED",
            getDocument.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task WorkerLoopSurvivesTransientStartCommitErrorAndInvokesProviderOnce()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            FailOnWriteAttempt = 2
        };
        var adapter = new RecordingSuccessAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);
        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        Assert.Equal(202, create.StatusCode);

        using var cancellation = new CancellationTokenSource();
        var worker = processor.RunWorkerLoopAsync(
            cancellation.Token,
            workerCount: 1,
            idleDelay: TimeSpan.FromMilliseconds(1));

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        DesktopNodeApiResponse? get = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            get = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs?limit=50&offset=0"));
            using var document = JsonDocument.Parse(get.Body);
            var jobs = document.RootElement.GetProperty("data").GetProperty("jobs").EnumerateArray().ToArray();
            if (jobs.Length == 1 && string.Equals(
                    jobs[0].GetProperty("status").GetString(),
                    "succeeded",
                    StringComparison.Ordinal))
            {
                break;
            }

            await Task.Delay(10);
        }

        Assert.NotNull(get);
        using (var document = JsonDocument.Parse(get.Body))
        {
            var job = Assert.Single(document.RootElement.GetProperty("data").GetProperty("jobs").EnumerateArray());
            Assert.Equal("succeeded", job.GetProperty("status").GetString());
        }

        Assert.Equal(1, adapter.InvokeCount);
        Assert.False(worker.IsCompleted);
        await cancellation.CancelAsync();
        await worker.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task WorkerLoopStaysAliveWithoutProviderReplayAfterCompletionCommitError()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            FailOnWriteAttempt = 3
        };
        var adapter = new RecordingSuccessAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);
        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        Assert.Equal(202, create.StatusCode);
        using var createDocument = JsonDocument.Parse(create.Body);
        var jobId = createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        using var cancellation = new CancellationTokenSource();
        var worker = processor.RunWorkerLoopAsync(
            cancellation.Token,
            workerCount: 1,
            idleDelay: TimeSpan.FromMilliseconds(1));

        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        DesktopNodeApiResponse? get = null;
        while (DateTimeOffset.UtcNow < deadline)
        {
            get = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{jobId}"));
            if (get.StatusCode == 409)
            {
                break;
            }

            await Task.Delay(10);
        }

        Assert.NotNull(get);
        Assert.Equal(409, get.StatusCode);
        Assert.Equal(1, adapter.InvokeCount);
        await Task.Delay(25);
        Assert.Equal(1, adapter.InvokeCount);
        Assert.False(worker.IsCompleted);
        using (var durable = JsonDocument.Parse(store.DurableSnapshot!))
        {
            Assert.Equal("running", durable.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
            Assert.Empty(durable.RootElement.GetProperty("queue").EnumerateArray());
        }

        await cancellation.CancelAsync();
        await worker.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ProviderResultFinalizationKeepsRunningCancellationAttachedUntilSerializedComplete()
    {
        using var finalizationReached = new ManualResetEventSlim(false);
        using var releaseFinalization = new ManualResetEventSlim(false);
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(),
            nativeAdapter: new RecordingSuccessAdapter());
        processor.BeforeJobFinalization = () =>
        {
            finalizationReached.Set();
            Assert.True(releaseFinalization.Wait(TimeSpan.FromSeconds(5)));
        };
        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec",
            """{"command":["cmd","/c","ver"],"credential_ref":"wincred:PureCVisor/guest/admin","timeout_sec":60}"""));
        using var queuedDocument = JsonDocument.Parse(queued.Body);
        var jobId = queuedDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        var tickTask = Task.Run(processor.ProcessOneQueuedJob);
        Assert.True(finalizationReached.Wait(TimeSpan.FromSeconds(5)));

        var cancel = processor.Handle(new DesktopNodeApiRequest("POST", $"/api/v1/jobs/{jobId}/cancel"));

        Assert.Equal(202, cancel.StatusCode);
        using (var cancelDocument = JsonDocument.Parse(cancel.Body))
        {
            Assert.Equal(
                "PCV_JOB_CANCEL_REQUESTED",
                cancelDocument.RootElement.GetProperty("data").GetProperty("error").GetProperty("code").GetString());
        }
        releaseFinalization.Set();
        var tick = await tickTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(tick.Processed);
        Assert.Null(tick.Error);
    }

    [Fact]
    public async Task WorkerOwnerCancellationReachesLinkedJobToken()
    {
        var trace = new List<string>();
        var cancellationScopes = new RecordingDesktopNodeApiCancellationScopeFactory(trace);
        var adapter = new BlockingCancellationAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(CancellationScopes: cancellationScopes),
            nativeAdapter: adapter);
        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec",
            """{"command":["cmd","/c","ver"],"credential_ref":"wincred:PureCVisor/guest/admin","timeout_sec":60}"""));
        Assert.Equal(202, queued.StatusCode);

        using var ownerCancellation = new CancellationTokenSource();
        var worker = processor.RunWorkerLoopAsync(
            ownerCancellation.Token,
            workerCount: 1,
            idleDelay: TimeSpan.FromMilliseconds(10));
        await adapter.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await ownerCancellation.CancelAsync();
        await worker.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Single(cancellationScopes.LinkedJobScopes);
        Assert.Equal(cancellationScopes.LinkedJobScopes[0].Token, adapter.ReceivedToken);
        Assert.True(cancellationScopes.LinkedJobScopes[0].IsCancellationRequested);
        await adapter.Canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(cancellationScopes.LinkedJobScopes[0].Disposed);
    }

    [Fact]
    public void DurableEnqueueSurvivesDroppedResponseAndIsDiscoverableByRequestCorrelation()
    {
        var store = new RecordingDesktopNodeApiJobStore();
        var adapter = new RecordingSuccessAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);

        var deliveredButDiscarded = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/start",
            RequestId: "req-response-not-observed"));
        Assert.Equal(202, deliveredButDiscarded.StatusCode);
        deliveredButDiscarded = null!;

        var restarted = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);
        var list = restarted.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/jobs?limit=50&offset=0"));

        Assert.Equal(200, list.StatusCode);
        using var document = JsonDocument.Parse(list.Body);
        var recovered = Assert.Single(document.RootElement
            .GetProperty("data")
            .GetProperty("jobs")
            .EnumerateArray());
        Assert.Equal("queued", recovered.GetProperty("status").GetString());
        Assert.Equal("req-response-not-observed", recovered.GetProperty("request_id").GetString());
        Assert.False(string.IsNullOrWhiteSpace(recovered.GetProperty("correlation_id").GetString()));
        Assert.Equal(0, adapter.InvokeCount);
    }

    private static void AssertSnapshot(
        JsonDocument document,
        string jobId,
        string status,
        string[] queue,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset savedAt)
    {
        var root = document.RootElement;
        var job = Assert.Single(root.GetProperty("jobs").EnumerateArray());
        Assert.Equal(jobId, job.GetProperty("job_id").GetString());
        Assert.Equal(status, job.GetProperty("status").GetString());
        Assert.Equal(createdAt.ToString("o"), job.GetProperty("created_at").GetString());
        Assert.Equal(updatedAt.ToString("o"), job.GetProperty("updated_at").GetString());
        Assert.Equal(savedAt.ToString("o"), root.GetProperty("saved_at").GetString());
        Assert.Equal(queue, root.GetProperty("queue").EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    private sealed class RecordingSuccessAdapter(IList<string>? trace = null) : IDesktopNodeHyperVNativeAdapter
    {
        private int invokeCount;

        public int InvokeCount => invokeCount;

        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            Interlocked.Increment(ref invokeCount);
            trace?.Add("provider.invoke");
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                {
                    ["name"] = parameters.TryGetProperty("name", out var name) ? name.GetString() : "alpha"
                }),
                Error: null);
            return true;
        }
    }

    private sealed class BlockingCancellationAdapter : IDesktopNodeHyperVNativeAdapter
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Canceled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CancellationToken ReceivedToken { get; private set; }

        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            ReceivedToken = cancellationToken;
            Started.TrySetResult();
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
            if (cancellationToken.IsCancellationRequested)
            {
                Canceled.TrySetResult();
                throw new OperationCanceledException(cancellationToken);
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_TEST_CANCEL_NOT_REQUESTED",
                "The recording adapter did not observe cancellation.",
                "The linked job token was not canceled before the test timeout.",
                false);
            return true;
        }
    }
}
