using System.Collections.Concurrent;
using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class JobRuntimeDurabilityTests
{
    [Fact]
    public void CreateSaveFailureDoesNotPublishMemoryOrQueueGhost()
    {
        var store = new SaveFailingJobStore
        {
            FailOnWriteAttempt = 1
        };
        DesktopNodeJobRuntimeSnapshot? stateObservedDuringWrite = null;
        DesktopNodeJobRuntime? runtimeObservedDuringWrite = null;
        store.BeforeWrite = () => stateObservedDuringWrite = runtimeObservedDuringWrite!.Snapshot();
        var runtime = new DesktopNodeJobRuntime(store);
        runtimeObservedDuringWrite = runtime;

        var exception = Assert.Throws<DesktopNodeJobStoreWriteException>(() =>
            CreateJob(runtime, "job-create-failure"));

        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", exception.Error.Code);
        Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, exception.CommitOutcome);
        Assert.False(exception.Error.Retryable);
        Assert.IsType<IOException>(exception.InnerException);
        Assert.Contains("attempt 1", exception.InnerException.Message, StringComparison.Ordinal);
        Assert.Null(store.DurableSnapshot);
        Assert.Single(store.AttemptedSnapshots);
        Assert.NotNull(stateObservedDuringWrite);
        Assert.Empty(stateObservedDuringWrite.Jobs);
        Assert.Empty(stateObservedDuringWrite.Queue);
        using (var candidate = JsonDocument.Parse(store.AttemptedSnapshots[0]))
        {
            Assert.Equal(
                "job-create-failure",
                Assert.Single(candidate.RootElement.GetProperty("jobs").EnumerateArray())
                    .GetProperty("job_id")
                    .GetString());
            Assert.Equal(
                "job-create-failure",
                Assert.Single(candidate.RootElement.GetProperty("queue").EnumerateArray())
                    .GetString());
        }

        Assert.Empty(runtime.Snapshot().Jobs);
        Assert.Empty(runtime.Snapshot().Queue);

        store.FailOnWriteAttempt = null;
        Assert.Null(runtime.TryStartNext(() => { }));

        var restarted = new DesktopNodeJobRuntime(store);
        Assert.Empty(restarted.Snapshot().Jobs);
        Assert.Empty(restarted.Snapshot().Queue);
        Assert.Null(restarted.TryStartNext(() => { }));

        var establishedStore = new SaveFailingJobStore();
        var establishedRuntime = new DesktopNodeJobRuntime(establishedStore);
        CreateJob(establishedRuntime, "job-existing");
        var durableBeforeFailure = establishedStore.DurableSnapshot;
        establishedStore.FailOnWriteAttempt = 2;

        Assert.Throws<DesktopNodeJobStoreWriteException>(() =>
            CreateJob(establishedRuntime, "job-rejected-candidate"));

        Assert.Equal(durableBeforeFailure, establishedStore.DurableSnapshot);
        var preservedState = establishedRuntime.Snapshot();
        Assert.Equal("job-existing", Assert.Single(preservedState.Jobs).JobId);
        Assert.Equal(["job-existing"], preservedState.Queue);

        establishedStore.FailOnWriteAttempt = null;
        var preservedRestart = new DesktopNodeJobRuntime(establishedStore);
        Assert.Equal("job-existing", Assert.Single(preservedRestart.Snapshot().Jobs).JobId);
        Assert.Equal(["job-existing"], preservedRestart.Snapshot().Queue);
        Assert.Equal("job-existing", preservedRestart.TryStartNext(() => { })!.JobId);
    }

    [Fact]
    public void StartSaveFailureKeepsRecoverableMeaning()
    {
        var store = new SaveFailingJobStore();
        var runtime = new DesktopNodeJobRuntime(store);
        CreateJob(runtime, "job-start-failure");
        DesktopNodeJobRuntimeSnapshot? stateObservedDuringWrite = null;
        store.BeforeWrite = () => stateObservedDuringWrite = runtime.Snapshot();
        store.FailOnWriteAttempt = 2;
        var cancellationSignalCount = 0;

        var started = runtime.TryStartNext(() => cancellationSignalCount++);

        Assert.Null(started);
        Assert.Equal(0, cancellationSignalCount);
        Assert.NotNull(stateObservedDuringWrite);
        Assert.Equal("queued", Assert.Single(stateObservedDuringWrite.Jobs).Status);
        Assert.Equal(["job-start-failure"], stateObservedDuringWrite.Queue);
        var live = runtime.Snapshot();
        Assert.Equal("queued", Assert.Single(live.Jobs).Status);
        Assert.Equal(["job-start-failure"], live.Queue);
        AssertPersistedState(store.DurableSnapshot!, "job-start-failure", "queued", ["job-start-failure"]);

        store.BeforeWrite = null;
        store.FailOnWriteAttempt = null;
        var restarted = new DesktopNodeJobRuntime(store);
        Assert.Equal("queued", Assert.Single(restarted.Snapshot().Jobs).Status);
        Assert.Equal(["job-start-failure"], restarted.Snapshot().Queue);
    }

    [Fact]
    public void TransitionSaveFailureKeepsRecoverableMeaning()
    {
        var cancelStore = new SaveFailingJobStore();
        var cancelRuntime = new DesktopNodeJobRuntime(cancelStore);
        CreateJob(cancelRuntime, "job-cancel-failure");
        DesktopNodeJobRuntimeSnapshot? cancelStateObservedDuringWrite = null;
        cancelStore.BeforeWrite = () => cancelStateObservedDuringWrite = cancelRuntime.Snapshot();
        cancelStore.FailOnWriteAttempt = 2;

        var cancelException = Assert.Throws<DesktopNodeJobStoreCommitException>(() =>
            cancelRuntime.Cancel("job-cancel-failure"));

        Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, cancelException.Outcome);
        Assert.NotNull(cancelStateObservedDuringWrite);
        Assert.Equal("queued", Assert.Single(cancelStateObservedDuringWrite.Jobs).Status);
        Assert.Equal(["job-cancel-failure"], cancelStateObservedDuringWrite.Queue);
        var cancelLive = cancelRuntime.Snapshot();
        Assert.Equal("queued", Assert.Single(cancelLive.Jobs).Status);
        Assert.Equal(["job-cancel-failure"], cancelLive.Queue);
        AssertPersistedState(cancelStore.DurableSnapshot!, "job-cancel-failure", "queued", ["job-cancel-failure"]);
        cancelStore.BeforeWrite = null;
        cancelStore.FailOnWriteAttempt = null;
        var cancelRestart = new DesktopNodeJobRuntime(cancelStore);
        Assert.Equal("queued", Assert.Single(cancelRestart.Snapshot().Jobs).Status);
        Assert.Equal(["job-cancel-failure"], cancelRestart.Snapshot().Queue);

        var completeStore = new SaveFailingJobStore();
        var completeRuntime = new DesktopNodeJobRuntime(completeStore);
        CreateJob(completeRuntime, "job-complete-failure");
        var completeStarted = completeRuntime.TryStartNext(() => { });
        Assert.NotNull(completeStarted);
        completeRuntime.DetachRunningCancellation(completeStarted.JobId);
        DesktopNodeJobRuntimeSnapshot? completeStateObservedDuringWrite = null;
        completeStore.BeforeWrite = () => completeStateObservedDuringWrite = completeRuntime.Snapshot();
        completeStore.FailOnWriteAttempt = 3;

        var completeException = Assert.Throws<DesktopNodeJobStoreCommitException>(() =>
            completeRuntime.Complete(completeStarted, SuccessfulOutcome("vm.start")));

        Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, completeException.Outcome);
        Assert.NotNull(completeStateObservedDuringWrite);
        Assert.Equal("running", Assert.Single(completeStateObservedDuringWrite.Jobs).Status);
        var completeLive = completeRuntime.Snapshot();
        Assert.Equal("running", Assert.Single(completeLive.Jobs).Status);
        Assert.Empty(completeLive.Queue);
        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", completeLive.LoadBlock!.Code);
        Assert.False(completeLive.LoadBlock.Retryable);
        Assert.Contains("confirmed previous durable job remain running", completeLive.LoadBlock.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("authoritative durable state is unknown", completeLive.LoadBlock.Detail, StringComparison.Ordinal);
        Assert.Contains("Do not retry", completeLive.LoadBlock.RecommendedAction, StringComparison.Ordinal);
        Assert.Equal("blocked", completeLive.StoreHealth.Status);
        Assert.Contains(
            completeLive.StoreHealth.RecentEvents,
            observation => observation.Event == "completion-persistence-failed" &&
                observation.CommitOutcome == "notcommitted");
        AssertPersistedState(completeStore.DurableSnapshot!, "job-complete-failure", "running", []);
        Assert.Null(completeRuntime.TryStartNext(() =>
            throw new InvalidOperationException("A terminal save failure must not retry the provider in-process.")));

        completeStore.BeforeWrite = null;
        completeStore.FailOnWriteAttempt = null;
        var completeRestart = new DesktopNodeJobRuntime(completeStore);
        var recovered = Assert.Single(completeRestart.Snapshot().Jobs);
        Assert.Equal("failed", recovered.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", recovered.Error!.Code);
        Assert.False(recovered.Error.Retryable);
        Assert.Contains("reconcile", recovered.Error.RecommendedAction, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(completeRestart.Snapshot().Queue);
        var rejectedRetry = completeRestart.Retry(
            recovered.JobId,
            new DesktopNodeJobRequestContext("req-interrupted-retry"));
        Assert.Equal(DesktopNodeJobCommandOutcome.Rejected, rejectedRetry.Outcome);
        Assert.Equal("PCV_JOB_NOT_RETRYABLE", rejectedRetry.Error!.Code);
        Assert.Null(completeRestart.TryStartNext(() =>
            throw new InvalidOperationException("A terminal save failure must not re-dispatch the provider.")));

        var indeterminateCompleteStore = new SaveFailingJobStore();
        var indeterminateCompleteRuntime = new DesktopNodeJobRuntime(indeterminateCompleteStore);
        CreateJob(indeterminateCompleteRuntime, "job-complete-indeterminate");
        var indeterminateCompleteStarted = indeterminateCompleteRuntime.TryStartNext(() => { })!;
        indeterminateCompleteRuntime.DetachRunningCancellation(indeterminateCompleteStarted.JobId);
        indeterminateCompleteStore.IndeterminateOnWriteAttempt = 3;

        var indeterminateCompleteException = Assert.Throws<DesktopNodeJobStoreCommitException>(() =>
            indeterminateCompleteRuntime.Complete(
                indeterminateCompleteStarted,
                SuccessfulOutcome("vm.start")));

        Assert.Equal(DesktopNodeJobStoreCommitOutcome.Indeterminate, indeterminateCompleteException.Outcome);
        Assert.Contains(
            "authoritative durable state is unknown",
            indeterminateCompleteRuntime.LoadBlock!.Detail,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "confirmed previous durable job",
            indeterminateCompleteRuntime.LoadBlock.Detail,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RunningCancelPersistsRequestBeforeProviderSignalOutsideStateLock()
    {
        var trace = new List<string>();
        var store = new SaveFailingJobStore();
        var runtime = new DesktopNodeJobRuntime(store);
        CreateGuestJob(runtime, "job-running-cancel");
        var callbackObservedPublishedState = false;
        var started = runtime.TryStartNext(() =>
        {
            trace.Add("cancel.signal");
            var observation = Task.Run(runtime.Snapshot);
            callbackObservedPublishedState = observation.Wait(TimeSpan.FromSeconds(5)) &&
                Assert.Single(observation.Result.Jobs).Error?.Code == "PCV_JOB_CANCEL_REQUESTED";
        });
        Assert.NotNull(started);
        DesktopNodeJobRuntimeSnapshot? stateObservedDuringWrite = null;
        store.BeforeWrite = () =>
        {
            trace.Add("store.write");
            stateObservedDuringWrite = runtime.Snapshot();
        };

        var cancel = runtime.Cancel(started.JobId);

        Assert.Equal(DesktopNodeJobCommandOutcome.CancellationRequested, cancel.Outcome);
        Assert.Equal(["store.write", "cancel.signal"], trace);
        Assert.True(callbackObservedPublishedState);
        Assert.NotNull(stateObservedDuringWrite);
        var preCommit = Assert.Single(stateObservedDuringWrite.Jobs);
        Assert.Equal("running", preCommit.Status);
        Assert.Null(preCommit.CanceledAt);
        Assert.Null(preCommit.Error);
        Assert.Equal("PCV_JOB_CANCEL_REQUESTED", cancel.Job!.Error!.Code);
        AssertPersistedState(store.DurableSnapshot!, started.JobId, "running", []);
        using (var durableCancel = JsonDocument.Parse(store.DurableSnapshot!))
        {
            var persistedJob = Assert.Single(durableCancel.RootElement.GetProperty("jobs").EnumerateArray());
            Assert.False(string.IsNullOrWhiteSpace(persistedJob.GetProperty("canceled_at").GetString()));
            Assert.Equal(
                "PCV_JOB_CANCEL_REQUESTED",
                persistedJob.GetProperty("error").GetProperty("code").GetString());
        }

        var writeCountAfterFirstCancel = store.WriteCount;
        var repeatedCancel = runtime.Cancel(started.JobId);
        Assert.Equal(DesktopNodeJobCommandOutcome.CancellationRequested, repeatedCancel.Outcome);
        Assert.Null(repeatedCancel.Error);
        Assert.Equal(writeCountAfterFirstCancel, store.WriteCount);
        Assert.Equal(1, trace.Count(item => item == "cancel.signal"));

        var signalFailureStore = new SaveFailingJobStore();
        var signalFailureRuntime = new DesktopNodeJobRuntime(signalFailureStore);
        CreateGuestJob(signalFailureRuntime, "job-running-cancel-signal-failure");
        var signalFailureStarted = signalFailureRuntime.TryStartNext(
            () => throw new InvalidOperationException("Injected provider cancellation callback failure."));
        Assert.NotNull(signalFailureStarted);

        var signalFailure = signalFailureRuntime.Cancel(signalFailureStarted.JobId);

        Assert.Equal(DesktopNodeJobCommandOutcome.CancellationRequested, signalFailure.Outcome);
        Assert.Equal("PCV_JOB_CANCEL_SIGNAL_FAILED", signalFailure.Error!.Code);
        Assert.False(signalFailure.Error.Retryable);
        Assert.Equal(
            "PCV_JOB_CANCEL_REQUESTED",
            Assert.Single(signalFailureRuntime.Snapshot().Jobs).Error!.Code);
        Assert.Equal("attention-required", signalFailureRuntime.Snapshot().StoreHealth.Status);
        AssertPersistedState(signalFailureStore.DurableSnapshot!, signalFailureStarted.JobId, "running", []);

        var terminalAfterSignalFailure = signalFailureRuntime.Complete(
            signalFailureStarted,
            SuccessfulOutcome("vm.guest.exec"));

        Assert.True(terminalAfterSignalFailure.Processed);
        Assert.Equal("healthy", signalFailureRuntime.Snapshot().StoreHealth.Status);
        Assert.Contains(
            signalFailureRuntime.Snapshot().StoreHealth.RecentEvents,
            item => item.Event == "cancel-signal-failed");

        var failedStore = new SaveFailingJobStore();
        var failedRuntime = new DesktopNodeJobRuntime(failedStore);
        CreateGuestJob(failedRuntime, "job-running-cancel-failure");
        var failedSignalCount = 0;
        var failedStarted = failedRuntime.TryStartNext(() => failedSignalCount++);
        Assert.NotNull(failedStarted);
        failedStore.FailOnWriteAttempt = 3;

        var failedException = Assert.Throws<DesktopNodeJobStoreCommitException>(() =>
            failedRuntime.Cancel(failedStarted.JobId));

        Assert.Equal(DesktopNodeJobStoreCommitOutcome.NotCommitted, failedException.Outcome);
        Assert.Equal(0, failedSignalCount);
        var failedLive = Assert.Single(failedRuntime.Snapshot().Jobs);
        Assert.Equal("running", failedLive.Status);
        Assert.Null(failedLive.CanceledAt);
        Assert.Null(failedLive.Error);
        AssertPersistedState(failedStore.DurableSnapshot!, failedStarted.JobId, "running", []);

        var indeterminateStore = new SaveFailingJobStore();
        var indeterminateRuntime = new DesktopNodeJobRuntime(indeterminateStore);
        CreateGuestJob(indeterminateRuntime, "job-running-cancel-indeterminate");
        var indeterminateSignalCount = 0;
        var indeterminateStarted = indeterminateRuntime.TryStartNext(() => indeterminateSignalCount++);
        Assert.NotNull(indeterminateStarted);
        indeterminateStore.IndeterminateOnWriteAttempt = 3;

        var indeterminateException = Assert.Throws<DesktopNodeJobStoreCommitException>(() =>
            indeterminateRuntime.Cancel(indeterminateStarted.JobId));

        Assert.Equal(DesktopNodeJobStoreCommitOutcome.Indeterminate, indeterminateException.Outcome);
        Assert.Equal(0, indeterminateSignalCount);
        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", indeterminateRuntime.LoadBlock!.Code);
        var indeterminateLive = Assert.Single(indeterminateRuntime.Snapshot().Jobs);
        Assert.Equal("running", indeterminateLive.Status);
        Assert.Null(indeterminateLive.CanceledAt);
        Assert.Null(indeterminateLive.Error);
    }

    [Fact]
    public void RestartRecoveryPublishesOnlyAfterDurableRewriteAndEmitsRedactedObservations()
    {
        var store = new SaveFailingJobStore();
        var initial = new DesktopNodeJobRuntime(store);
        CreateJob(initial, "job-recovery-save-failure");
        Assert.NotNull(initial.TryStartNext(() => { }));
        store.FailOnWriteAttempt = 3;
        var sink = new RecordingEventSink();

        var blocked = new DesktopNodeJobRuntime(store, eventSink: sink);

        var blockedSnapshot = blocked.Snapshot();
        Assert.True(sink.Wait(() => sink.Observations.Count >= 2));
        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", blockedSnapshot.LoadBlock!.Code);
        Assert.Equal("running", Assert.Single(blockedSnapshot.Jobs).Status);
        Assert.True(blockedSnapshot.StoreHealth.MutationBlocked);
        Assert.Contains(
            blockedSnapshot.StoreHealth.RecentEvents,
            observation => observation.Event == "save-not-committed");
        Assert.Contains(
            blockedSnapshot.StoreHealth.RecentEvents,
            observation => observation.Event == "running-recovery-persistence-failed");
        Assert.Contains(
            "confirmed previous persisted running state remains authoritative",
            blockedSnapshot.LoadBlock.Detail,
            StringComparison.Ordinal);
        var observationJson = JsonSerializer.Serialize(
            blockedSnapshot.StoreHealth.RecentEvents,
            RuntimePolicyContract.JsonOptions);
        Assert.DoesNotContain(store.Location, observationJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Injected", observationJson, StringComparison.OrdinalIgnoreCase);
        Assert.All(sink.Observations, observation => Assert.Null(
            observation.GetType().GetProperty("Parameters")));

        store.FailOnWriteAttempt = null;
        var recovered = new DesktopNodeJobRuntime(store, eventSink: sink);

        var recoveredSnapshot = recovered.Snapshot();
        Assert.True(sink.Wait(
            () => sink.Observations.Any(observation => observation.Event == "running-recovered")));
        var recoveredJob = Assert.Single(recoveredSnapshot.Jobs);
        Assert.Equal("failed", recoveredJob.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", recoveredJob.Error!.Code);
        Assert.False(recoveredJob.Error.Retryable);
        Assert.Contains(
            recoveredSnapshot.StoreHealth.RecentEvents,
            observation => observation.Event == "running-recovered" &&
                observation.CommitOutcome == "committed");
        AssertPersistedState(
            store.DurableSnapshot!,
            "job-recovery-save-failure",
            "failed",
            []);

        var indeterminateStore = new SaveFailingJobStore();
        var indeterminateInitial = new DesktopNodeJobRuntime(indeterminateStore);
        CreateJob(indeterminateInitial, "job-recovery-indeterminate");
        Assert.NotNull(indeterminateInitial.TryStartNext(() => { }));
        indeterminateStore.IndeterminateOnWriteAttempt = 3;

        var indeterminateBlocked = new DesktopNodeJobRuntime(indeterminateStore);

        Assert.Contains(
            "authoritative durable state is unknown",
            indeterminateBlocked.LoadBlock!.Detail,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "remains authoritative",
            indeterminateBlocked.LoadBlock.Detail,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EventSinkFailureDoesNotChangeLoadBlockOutcome()
    {
        var store = new SaveFailingJobStore();
        Assert.Equal(
            DesktopNodeJobStoreCommitOutcome.Committed,
            store.WriteSnapshot("{not-json").Outcome);

        var runtime = new DesktopNodeJobRuntime(store, eventSink: new ThrowingEventSink());

        Assert.Equal("PCV_JOB_STORE_CORRUPT", runtime.LoadBlock!.Code);
        Assert.Equal("load-blocked", Assert.Single(runtime.Snapshot().StoreHealth.RecentEvents).Event);
    }

    [Fact]
    public async Task BlockingEventSinkDoesNotHoldRuntimeStateLockOrDelayLoadOutcome()
    {
        var store = new SaveFailingJobStore();
        Assert.Equal(
            DesktopNodeJobStoreCommitOutcome.Committed,
            store.WriteSnapshot("{not-json").Outcome);
        using var sink = new BlockingEventSink();

        var runtime = new DesktopNodeJobRuntime(store, eventSink: sink);

        // Both properties are asserted by observing that the sink is STILL inside
        // Write, not by timing the caller. If the runtime published synchronously and
        // waited, the constructor could only return after Write completed, so Exited
        // would already be true. The timeouts below are hang guards, not thresholds.
        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(30)));
        Assert.False(sink.Exited);

        Assert.Equal("PCV_JOB_STORE_CORRUPT", runtime.LoadBlock!.Code);

        var snapshot = await Task.Run(runtime.Snapshot).WaitAsync(TimeSpan.FromSeconds(30));
        Assert.False(sink.Exited);
        Assert.Equal("blocked", snapshot.StoreHealth.Status);
        sink.Release.Set();
    }

    [Fact]
    public void SuccessfulCommitClearsTransientWriteAttentionButKeepsHistory()
    {
        var store = new SaveFailingJobStore();
        var runtime = new DesktopNodeJobRuntime(store);
        CreateJob(runtime, "job-transient-attention");
        store.FailOnWriteAttempt = 2;

        Assert.Null(runtime.TryStartNext(() => { }));
        var attention = runtime.Snapshot();
        Assert.Equal("attention-required", attention.StoreHealth.Status);
        Assert.Contains(attention.StoreHealth.RecentEvents, item => item.Event == "save-not-committed");

        store.FailOnWriteAttempt = null;
        var started = runtime.TryStartNext(() => { });

        Assert.NotNull(started);
        var healthy = runtime.Snapshot();
        Assert.Equal("healthy", healthy.StoreHealth.Status);
        Assert.Contains(healthy.StoreHealth.RecentEvents, item => item.Event == "save-not-committed");
    }

    [Fact]
    public async Task LateCancelSignalFailureDoesNotReopenAttentionAfterTerminalCommit()
    {
        var runtime = new DesktopNodeJobRuntime();
        CreateGuestJob(runtime, "job-late-cancel-signal-failure");
        using var signalEntered = new ManualResetEventSlim(false);
        using var releaseSignalFailure = new ManualResetEventSlim(false);
        var started = runtime.TryStartNext(() =>
        {
            signalEntered.Set();
            releaseSignalFailure.Wait(TimeSpan.FromSeconds(10));
            throw new IOException("Injected late cancellation signal failure.");
        });
        Assert.NotNull(started);

        var cancelTask = Task.Run(() => runtime.Cancel(started.JobId));
        Assert.True(signalEntered.Wait(TimeSpan.FromSeconds(5)));

        var completion = runtime.Complete(
            started,
            SuccessfulOutcome("vm.guest.exec"));
        releaseSignalFailure.Set();
        var cancel = await cancelTask.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(completion.Processed);
        Assert.Equal("succeeded", completion.Job!.Status);
        Assert.Equal("PCV_JOB_CANCEL_SIGNAL_FAILED", cancel.Error!.Code);
        var snapshot = runtime.Snapshot();
        Assert.Equal("healthy", snapshot.StoreHealth.Status);
        Assert.Contains(snapshot.StoreHealth.RecentEvents, item => item.Event == "cancel-signal-failed");
    }

    private static DesktopNodeJobSnapshot CreateJob(
        DesktopNodeJobRuntime runtime,
        string jobId)
    {
        return runtime.Create(
            new DesktopNodeJobCreateCommand(
                "vm.start",
                JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                {
                    ["name"] = "alpha"
                }),
                JobId: jobId),
            new DesktopNodeJobRequestContext("req-" + jobId));
    }

    private static DesktopNodeJobSnapshot CreateGuestJob(
        DesktopNodeJobRuntime runtime,
        string jobId)
    {
        return runtime.Create(
            new DesktopNodeJobCreateCommand(
                "vm.guest.exec",
                JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                {
                    ["command"] = new[] { "cmd", "/c", "ver" },
                    ["credential_ref"] = "wincred:PureCVisor/guest/admin",
                    ["name"] = "alpha"
                }),
                JobId: jobId),
            new DesktopNodeJobRequestContext("req-" + jobId));
    }

    private static DesktopNodeJobExecutionOutcome SuccessfulOutcome(string operation)
    {
        return new DesktopNodeJobExecutionOutcome(
            true,
            JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
            {
                ["data"] = new SortedDictionary<string, object?> { ["name"] = "alpha" },
                ["error"] = null,
                ["ok"] = true,
                ["operation"] = operation
            }),
            null);
    }

    private static void AssertPersistedState(
        string json,
        string jobId,
        string status,
        string[] queue)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var job = Assert.Single(root.GetProperty("jobs").EnumerateArray());
        Assert.Equal(jobId, job.GetProperty("job_id").GetString());
        Assert.Equal(status, job.GetProperty("status").GetString());
        Assert.Equal(queue, root.GetProperty("queue").EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    private sealed class SaveFailingJobStore : IDesktopNodeJobStore
    {
        public string Location { get; } = "recording://durability/jobs.json";

        public string? DurableSnapshot { get; private set; }

        public int? FailOnWriteAttempt { get; set; }

        public int? IndeterminateOnWriteAttempt { get; set; }

        public int WriteCount { get; private set; }

        public List<string> AttemptedSnapshots { get; } = [];

        public Action? BeforeWrite { get; set; }

        public bool Exists() => DurableSnapshot is not null;

        public string ReadSnapshot() => DurableSnapshot
            ?? throw new InvalidOperationException("No durable recording snapshot exists.");

        public DesktopNodeJobStoreWriteResult WriteSnapshot(string json)
        {
            BeforeWrite?.Invoke();
            WriteCount++;
            AttemptedSnapshots.Add(json);
            if (IndeterminateOnWriteAttempt == WriteCount)
            {
                return DesktopNodeJobStoreWriteResult.Indeterminate(
                    new IOException($"Injected indeterminate store write at attempt {WriteCount}."));
            }

            if (FailOnWriteAttempt == WriteCount)
            {
                return DesktopNodeJobStoreWriteResult.NotCommitted(
                    new IOException($"Injected store write failure at attempt {WriteCount}."));
            }

            DurableSnapshot = json;
            return DesktopNodeJobStoreWriteResult.Committed;
        }

        public void Quarantine(string suffix)
        {
            DurableSnapshot = null;
        }
    }

    private sealed class RecordingEventSink : IDesktopNodeJobRuntimeEventSink
    {
        private readonly object gate = new();

        public ConcurrentQueue<DesktopNodeJobRuntimeObservation> Observations { get; } = new();

        public void Write(DesktopNodeJobRuntimeObservation observation)
        {
            lock (gate)
            {
                Observations.Enqueue(observation);
                Monitor.PulseAll(gate);
            }
        }

        // Observations are published asynchronously, so a test has to wait for them. It must
        // not do that by spinning: SpinWait burns the very CPU the publisher needs, so under a
        // loaded runner the wait competes with the work it is waiting for and a short deadline
        // expires. Blocking on a pulse costs nothing while idle, and the timeout below is a
        // hang guard rather than a performance threshold.
        public bool Wait(Func<bool> condition)
        {
            var deadline = Environment.TickCount64 + (long)TimeSpan.FromSeconds(60).TotalMilliseconds;
            lock (gate)
            {
                while (!condition())
                {
                    var remaining = deadline - Environment.TickCount64;
                    if (remaining <= 0)
                    {
                        return false;
                    }

                    Monitor.Wait(gate, (int)Math.Min(remaining, int.MaxValue));
                }

                return true;
            }
        }
    }

    private sealed class BlockingEventSink : IDesktopNodeJobRuntimeEventSink, IDisposable
    {
        private volatile bool exited;

        public ManualResetEventSlim Entered { get; } = new(false);

        public ManualResetEventSlim Release { get; } = new(false);

        // Lets a caller assert it ran while Write was still blocked, without timing
        // how long the caller took. A wall-clock threshold turns a concurrency
        // property into a performance one and fails under parallel-suite load.
        public bool Exited => exited;

        public void Write(DesktopNodeJobRuntimeObservation observation)
        {
            Entered.Set();
            Release.Wait(TimeSpan.FromSeconds(10));
            exited = true;
        }

        public void Dispose()
        {
            Release.Set();
            Entered.Dispose();
            Release.Dispose();
        }
    }

    private sealed class ThrowingEventSink : IDesktopNodeJobRuntimeEventSink
    {
        public void Write(DesktopNodeJobRuntimeObservation observation)
        {
            throw new IOException("Injected event sink failure with a sensitive path.");
        }
    }
}
