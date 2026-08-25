using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class DesktopNodeJobRuntimeTests
{
    [Fact]
    public void GetReturnsFoundSnapshotAndNotFoundDiagnostics()
    {
        var runtime = new DesktopNodeJobRuntime();
        var created = CreateStartJob(runtime, "job-get");

        var found = runtime.Get(created.JobId);
        var missing = runtime.Get("job-missing");

        Assert.Equal(DesktopNodeJobCommandOutcome.Found, found.Outcome);
        Assert.Equal(created, found.Job);
        Assert.Null(found.Error);
        Assert.Equal(DesktopNodeJobCommandOutcome.NotFound, missing.Outcome);
        Assert.Null(missing.Job);
        Assert.Equal("PCV_JOB_NOT_FOUND", missing.Error!.Code);
        Assert.Contains("job-missing", missing.Error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CommandsPreserveMissingQueuedAndDetachedRunningOutcomes()
    {
        var runtime = new DesktopNodeJobRuntime();
        var first = CreateStartJob(runtime, "job-first");
        var second = CreateStartJob(runtime, "job-second");

        var missingCancel = runtime.Cancel("job-missing");
        var canceled = runtime.Cancel(first.JobId);
        var repeatedCancel = runtime.Cancel(first.JobId);
        var missingRetry = runtime.Retry(
            "job-missing",
            new DesktopNodeJobRequestContext("req-missing-retry"));
        var canceledRetry = runtime.Retry(
            first.JobId,
            new DesktopNodeJobRequestContext("req-canceled-retry"));

        Assert.Equal(DesktopNodeJobCommandOutcome.NotFound, missingCancel.Outcome);
        Assert.Equal("PCV_JOB_NOT_FOUND", missingCancel.Error!.Code);
        Assert.Equal(DesktopNodeJobCommandOutcome.Canceled, canceled.Outcome);
        Assert.Equal("canceled", canceled.Job!.Status);
        Assert.Equal([second.JobId], runtime.Snapshot().Queue);
        Assert.Equal(DesktopNodeJobCommandOutcome.Rejected, repeatedCancel.Outcome);
        Assert.Equal("PCV_JOB_NOT_CANCELABLE", repeatedCancel.Error!.Code);
        Assert.Equal(DesktopNodeJobCommandOutcome.NotFound, missingRetry.Outcome);
        Assert.Equal(DesktopNodeJobCommandOutcome.Rejected, canceledRetry.Outcome);
        Assert.Equal("PCV_JOB_NOT_RETRYABLE", canceledRetry.Error!.Code);

        var runningRuntime = new DesktopNodeJobRuntime();
        var running = runningRuntime.Create(
            new DesktopNodeJobCreateCommand(
                "vm.guest.exec",
                JsonElementFromRaw("""{"name":"alpha","command":["cmd","/c","ver"]}"""),
                JobId: "job-detached-running"),
            new DesktopNodeJobRequestContext("req-detached-running"));
        var started = runningRuntime.TryStartNext(() => { });
        Assert.NotNull(started);
        runningRuntime.DetachRunningCancellation(started.JobId);

        var detachedCancel = runningRuntime.Cancel(running.JobId);

        Assert.Equal(DesktopNodeJobCommandOutcome.Rejected, detachedCancel.Outcome);
        Assert.Equal("PCV_JOB_CANCEL_NOT_INTERRUPTIBLE", detachedCancel.Error!.Code);
        Assert.Equal("running", runningRuntime.Get(running.JobId).Job!.Status);
    }

    [Fact]
    public void PersistedInvalidStatusBlocksStoreWithoutMutation()
    {
        var store = new RecordingJobStore(
            initialSnapshot: JobStoreJson(
                1,
                [JobRow("job-invalid", "vm.start", "mystery", "2026-08-01T00:00:00.0000000+00:00")],
                []));
        var runtime = new DesktopNodeJobRuntime(store);

        var cancel = runtime.Cancel("job-invalid");
        var retry = runtime.Retry(
            "job-invalid",
            new DesktopNodeJobRequestContext("req-invalid-retry"));

        Assert.Equal(DesktopNodeJobCommandOutcome.Rejected, cancel.Outcome);
        Assert.Equal("PCV_JOB_STORE_CORRUPT", cancel.Error!.Code);
        Assert.Equal(DesktopNodeJobCommandOutcome.Rejected, retry.Outcome);
        Assert.Equal("PCV_JOB_STORE_CORRUPT", retry.Error!.Code);
        Assert.Equal("PCV_JOB_STORE_CORRUPT", runtime.LoadBlock!.Code);
        Assert.Empty(runtime.Snapshot().Jobs);
        Assert.Equal(0, store.WriteCount);
    }

    [Fact]
    public void CompleteHandlesMissingProviderFailureAndRepeatedTerminalCompletion()
    {
        var runtime = new DesktopNodeJobRuntime();
        var created = CreateStartJob(runtime, "job-complete-matrix");
        var started = runtime.TryStartNext(() => { });
        Assert.NotNull(started);
        var missingStarted = new DesktopNodeStartedJob(
            "job-missing",
            started.Operation,
            started.Parameters);
        var providerFailure = new DesktopNodeJobExecutionOutcome(
            false,
            NativeFailure("vm.start", "PCV_NATIVE_OPERATION_FAILED"),
            null);

        var missing = runtime.Complete(missingStarted, providerFailure);
        var failed = runtime.Complete(started, providerFailure);
        var repeated = runtime.Complete(
            started,
            DesktopNodeJobExecutionOutcomeSuccess("vm.start", "alpha"));

        Assert.False(missing.Processed);
        Assert.Null(missing.Job);
        Assert.True(failed.Processed);
        Assert.Equal("failed", failed.Job!.Status);
        Assert.Equal("PCV_NATIVE_OPERATION_FAILED", failed.Job.Error!.Code);
        Assert.True(repeated.Processed);
        Assert.Equal("failed", repeated.Job!.Status);
        Assert.Equal("PCV_JOB_TRANSITION_TERMINAL", repeated.Job.Error!.Code);
        Assert.Equal(created.JobId, repeated.Job.JobId);
    }

    [Fact]
    public void RunningCancellationTerminalOutcomesRemainDistinct()
    {
        var acknowledgedRuntime = new DesktopNodeJobRuntime();
        CreateGuestJob(acknowledgedRuntime, "job-cancel-acknowledged");
        var acknowledgedStarted = acknowledgedRuntime.TryStartNext(() => { })!;
        Assert.Equal(
            DesktopNodeJobCommandOutcome.CancellationRequested,
            acknowledgedRuntime.Cancel(acknowledgedStarted.JobId).Outcome);
        acknowledgedRuntime.DetachRunningCancellation(acknowledgedStarted.JobId);
        var acknowledged = acknowledgedRuntime.Complete(
            acknowledgedStarted,
            new DesktopNodeJobExecutionOutcome(
                false,
                NativeFailure("vm.guest.exec", "PCV_NATIVE_OPERATION_CANCELED"),
                new JobError(
                    "PCV_NATIVE_OPERATION_CANCELED",
                    "Canceled.",
                    "The provider acknowledged cancellation.",
                    false),
                CancellationAcknowledged: true));
        Assert.Equal("canceled", acknowledged.Job!.Status);
        Assert.Equal("PCV_JOB_CANCELED", acknowledged.Job.Error!.Code);
        Assert.False(string.IsNullOrWhiteSpace(acknowledged.Job.CanceledAt));

        var completedRuntime = new DesktopNodeJobRuntime();
        CreateGuestJob(completedRuntime, "job-completed-before-cancel");
        var completedStarted = completedRuntime.TryStartNext(() => { })!;
        Assert.Equal(
            DesktopNodeJobCommandOutcome.CancellationRequested,
            completedRuntime.Cancel(completedStarted.JobId).Outcome);
        completedRuntime.DetachRunningCancellation(completedStarted.JobId);
        var completed = completedRuntime.Complete(
            completedStarted,
            DesktopNodeJobExecutionOutcomeSuccess("vm.guest.exec", "completed"));
        Assert.Equal("succeeded", completed.Job!.Status);
        Assert.Null(completed.Job.Error);
        Assert.False(string.IsNullOrWhiteSpace(completed.Job.CanceledAt));

        var timeoutRuntime = new DesktopNodeJobRuntime();
        CreateGuestJob(timeoutRuntime, "job-cancel-timeout");
        var timeoutStarted = timeoutRuntime.TryStartNext(() => { })!;
        Assert.Equal(
            DesktopNodeJobCommandOutcome.CancellationRequested,
            timeoutRuntime.Cancel(timeoutStarted.JobId).Outcome);
        timeoutRuntime.DetachRunningCancellation(timeoutStarted.JobId);
        var timeout = timeoutRuntime.Complete(
            timeoutStarted,
            new DesktopNodeJobExecutionOutcome(
                false,
                NativeFailure("vm.guest.exec", "PCV_NATIVE_TIMEOUT"),
                new JobError(
                    "PCV_NATIVE_TIMEOUT",
                    "Timed out.",
                    "The provider did not acknowledge cancellation before its timeout.",
                    true)));
        Assert.Equal("failed", timeout.Job!.Status);
        Assert.Equal("PCV_NATIVE_TIMEOUT", timeout.Job.Error!.Code);
        Assert.False(string.IsNullOrWhiteSpace(timeout.Job.CanceledAt));
    }

    [Fact]
    public void CreateStartCompleteSnapshotsPreserveCurrentClockAndWriteOrder()
    {
        var trace = new List<string>();
        var timestamps = Enumerable.Range(1, 6)
            .Select(second => new DateTimeOffset(2026, 8, 2, 0, 0, second, TimeSpan.Zero))
            .ToArray();
        var store = new RecordingJobStore(trace: trace);
        var clock = new SequenceJobClock(timestamps, trace);
        var runtime = new DesktopNodeJobRuntime(store, clock);

        var created = CreateStartJob(runtime, "job-order");
        var started = runtime.TryStartNext(() => { });
        Assert.NotNull(started);
        runtime.DetachRunningCancellation(started.JobId);
        var completed = runtime.Complete(
            started,
            DesktopNodeJobExecutionOutcomeSuccess("vm.start", "alpha"));

        Assert.True(completed.Processed);
        Assert.Equal("succeeded", completed.Job!.Status);
        Assert.Equal(
            [
                "clock:1",
                "clock:2",
                "store.write:1",
                "clock:3",
                "clock:4",
                "store.write:2",
                "clock:5",
                "clock:6",
                "store.write:3"
            ],
            trace);

        Assert.Equal(3, store.AttemptedSnapshots.Count);
        AssertStoreSnapshot(store.AttemptedSnapshots[0], "job-order", "queued", ["job-order"], timestamps[0], timestamps[0], timestamps[1]);
        AssertStoreSnapshot(store.AttemptedSnapshots[1], "job-order", "running", [], timestamps[0], timestamps[2], timestamps[3]);
        AssertStoreSnapshot(store.AttemptedSnapshots[2], "job-order", "succeeded", [], timestamps[0], timestamps[4], timestamps[5]);
        using (var shape = JsonDocument.Parse(store.AttemptedSnapshots[0]))
        {
            Assert.Equal(
                ["jobs", "queue", "saved_at", "version"],
                shape.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
            Assert.Equal(
                [
                    "attempt",
                    "canceled_at",
                    "correlation_id",
                    "created_at",
                    "error",
                    "job_id",
                    "operation",
                    "params",
                    "request_id",
                    "result",
                    "retry_of",
                    "status",
                    "updated_at"
                ],
                shape.RootElement.GetProperty("jobs")[0]
                    .EnumerateObject()
                    .Select(property => property.Name)
                    .ToArray());
        }

        Assert.Equal(created.CreatedAt, completed.Job.CreatedAt);
    }

    [Fact]
    public void NoStoreSaveDoesNotReadAnAdditionalSavedAtClockValue()
    {
        var timestamp = new DateTimeOffset(2026, 8, 2, 1, 2, 3, TimeSpan.Zero);
        var clock = new SequenceJobClock([timestamp]);
        var runtime = new DesktopNodeJobRuntime(clock: clock);

        var created = CreateStartJob(runtime, "job-no-store");

        Assert.Equal(1, clock.ReadCount);
        Assert.Equal(timestamp.ToString("o"), created.CreatedAt);
        Assert.Equal(timestamp.ToString("o"), created.UpdatedAt);
    }

    [Fact]
    public void GuestExecutionKeepsRawExecutionParametersButRedactsSnapshotAndStore()
    {
        const string credentialReference = "wincred:PureCVisor/guest/admin";
        var store = new RecordingJobStore();
        var runtime = new DesktopNodeJobRuntime(store);
        var rawParameters = JsonElementFromRaw(
            $$"""{"name":"alpha","credential_ref":"{{credentialReference}}","command":["cmd","/c","ver"]}""");

        var created = runtime.Create(
            new DesktopNodeJobCreateCommand("vm.guest.exec", rawParameters, JobId: "job-redaction"),
            new DesktopNodeJobRequestContext("req-redaction"));
        var started = runtime.TryStartNext(() => { });

        Assert.NotNull(started);
        Assert.Equal(credentialReference, started.Parameters.GetProperty("credential_ref").GetString());
        Assert.Equal("[redacted-ref]", created.Parameters.GetProperty("credential_ref").GetString());
        Assert.Equal(
            Sha256(credentialReference),
            created.Parameters.GetProperty("credential_ref_hash").GetString());

        Assert.Equal(2, store.AttemptedSnapshots.Count);
        foreach (var snapshot in store.AttemptedSnapshots)
        {
            Assert.DoesNotContain(credentialReference, snapshot, StringComparison.Ordinal);
            using var document = JsonDocument.Parse(snapshot);
            var persistedParameters = document.RootElement.GetProperty("jobs")[0].GetProperty("params");
            Assert.Equal("[redacted-ref]", persistedParameters.GetProperty("credential_ref").GetString());
            Assert.Equal(
                Sha256(credentialReference),
                persistedParameters.GetProperty("credential_ref_hash").GetString());
        }
    }

    [Fact]
    public void LoadedRecommendedActionSurvivesRunningRecoveryRewrite()
    {
        var store = new RecordingJobStore(
            initialSnapshot: JobStoreJson(
                version: 1,
                jobs:
                [
                    JobRow(
                        "job-failed",
                        "vm.start",
                        "failed",
                        "2026-08-01T00:00:00.0000000+00:00",
                        error: new SortedDictionary<string, object?>
                        {
                            ["code"] = "PCV_EXISTING_FAILURE",
                            ["message"] = "Existing failure.",
                            ["detail"] = "Existing detail.",
                            ["retryable"] = false,
                            ["recommended_action"] = "Keep this operator action."
                        }),
                    JobRow(
                        "job-running",
                        "vm.start",
                        "running",
                        "2026-08-01T00:01:00.0000000+00:00")
                ],
                queue: []));
        var clock = new SequenceJobClock(
            [
                new DateTimeOffset(2026, 8, 2, 2, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 8, 2, 2, 0, 1, TimeSpan.Zero)
            ]);

        var runtime = new DesktopNodeJobRuntime(store, clock);

        var failed = runtime.Snapshot().Jobs.Single(job => job.JobId == "job-failed");
        Assert.Equal("Keep this operator action.", failed.Error!.RecommendedAction);
        Assert.Equal(1, store.WriteCount);
        using var rewritten = JsonDocument.Parse(store.DurableSnapshot!);
        var persistedFailed = rewritten.RootElement.GetProperty("jobs")
            .EnumerateArray()
            .Single(job => job.GetProperty("job_id").GetString() == "job-failed");
        Assert.Equal(
            "Keep this operator action.",
            persistedFailed.GetProperty("error").GetProperty("recommended_action").GetString());
    }

    [Fact]
    public void UnsupportedFutureVersionBlocksWithoutWriteOrQuarantine()
    {
        var original = JobStoreJson(version: 3, jobs: [], queue: []);
        var store = new RecordingJobStore(initialSnapshot: original);

        var runtime = new DesktopNodeJobRuntime(store);

        Assert.Equal("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", runtime.LoadBlock!.Code);
        Assert.Contains("version 3", runtime.LoadBlock.Detail, StringComparison.Ordinal);
        Assert.Equal(0, store.WriteCount);
        Assert.Empty(store.QuarantineSuffixes);
        Assert.Equal(original, store.DurableSnapshot);
        Assert.Empty(runtime.Snapshot().Jobs);
    }

    [Fact]
    public void PersistedRunningRecoveryRewritesFailedAndKeepsQueuedFifo()
    {
        var store = new RecordingJobStore(
            initialSnapshot: JobStoreJson(
                version: 2,
                jobs:
                [
                    JobRow("job-running", "vm.start", "running", "2026-08-01T00:00:00.0000000+00:00"),
                    JobRow("job-second", "vm.start", "queued", "2026-08-01T00:01:00.0000000+00:00"),
                    JobRow("job-first", "vm.start", "queued", "2026-08-01T00:02:00.0000000+00:00")
                ],
                queue: ["job-first", "job-running", "job-second"]));
        var clock = new SequenceJobClock(
            Enumerable.Range(0, 8)
                .Select(second => new DateTimeOffset(2026, 8, 2, 3, 0, second, TimeSpan.Zero))
                .ToArray());
        var runtime = new DesktopNodeJobRuntime(store, clock);

        var state = runtime.Snapshot();
        var recovered = state.Jobs.Single(job => job.JobId == "job-running");
        Assert.Equal("failed", recovered.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", recovered.Error!.Code);
        Assert.Equal(["job-first", "job-second"], state.Queue);
        Assert.Equal(2, state.SchemaVersion);
        Assert.Equal(1, store.WriteCount);

        var first = runtime.TryStartNext(() => { });
        Assert.Equal("job-first", first!.JobId);
        runtime.DetachRunningCancellation(first.JobId);
        runtime.Complete(first, DesktopNodeJobExecutionOutcomeSuccess("vm.start", "first"));
        var second = runtime.TryStartNext(() => { });
        Assert.Equal("job-second", second!.JobId);
    }

    [Fact]
    public void LoadRetentionKeepsFiveHundredTerminalJobsAndAllActiveJobs()
    {
        var jobs = new List<SortedDictionary<string, object?>>();
        var baseTime = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        for (var index = 0; index < 503; index++)
        {
            var status = index % 3 == 0 ? "succeeded" : index % 3 == 1 ? "failed" : "canceled";
            jobs.Add(JobRow(
                $"terminal-{index:000}",
                "vm.start",
                status,
                baseTime.AddMinutes(index).ToString("o"),
                error: status == "failed"
                    ? PersistedError("PCV_TEST_FAILURE", retryable: false)
                    : status == "canceled"
                        ? PersistedError("PCV_JOB_CANCELED", retryable: false)
                        : null,
                canceledAt: status == "canceled" ? baseTime.AddMinutes(index).ToString("o") : null));
        }

        jobs.Add(JobRow("active-job", "vm.start", "queued", baseTime.AddDays(1).ToString("o")));
        var store = new RecordingJobStore(
            initialSnapshot: JobStoreJson(1, jobs, ["active-job"]));
        var clock = new SequenceJobClock([baseTime.AddDays(2)]);

        var runtime = new DesktopNodeJobRuntime(store, clock);

        var state = runtime.Snapshot();
        Assert.Equal(501, state.Jobs.Count);
        Assert.Equal(500, state.Jobs.Count(job => job.Status is "succeeded" or "failed" or "canceled"));
        Assert.Contains(state.Jobs, job => job.JobId == "active-job");
        Assert.DoesNotContain(state.Jobs, job => job.JobId == "terminal-000");
        Assert.DoesNotContain(state.Jobs, job => job.JobId == "terminal-001");
        Assert.DoesNotContain(state.Jobs, job => job.JobId == "terminal-002");
        Assert.Equal(3, state.PrunedTerminalJobs);
        Assert.Equal(500, state.MaxRetainedTerminalJobs);
        Assert.Equal(["active-job"], state.Queue);
        Assert.Equal(1, store.WriteCount);
    }

    [Fact]
    public void LoadRetentionOrdersTimestampsByInstantInsteadOfRawOffsetText()
    {
        var jobs = new List<SortedDictionary<string, object?>>();
        var fillerBase = new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero);
        for (var index = 0; index < 499; index++)
        {
            jobs.Add(JobRow(
                $"newer-{index:000}",
                "vm.start",
                "succeeded",
                fillerBase.AddMinutes(index).ToString("o")));
        }

        jobs.Add(JobRow(
            "older-offset-text",
            "vm.start",
            "succeeded",
            "2026-08-02T02:00:00.0000000+02:00"));
        jobs.Add(JobRow(
            "newer-utc-instant",
            "vm.start",
            "succeeded",
            "2026-08-02T01:00:00.0000000+00:00"));
        var store = new RecordingJobStore(initialSnapshot: JobStoreJson(1, jobs, []));
        var runtime = new DesktopNodeJobRuntime(store, new SequenceJobClock([fillerBase.AddDays(2)]));

        var state = runtime.Snapshot();

        Assert.Equal(500, state.Jobs.Count);
        Assert.DoesNotContain(state.Jobs, job => job.JobId == "older-offset-text");
        Assert.Contains(state.Jobs, job => job.JobId == "newer-utc-instant");
        Assert.Equal(1, state.PrunedTerminalJobs);
        Assert.Equal(1, store.WriteCount);
    }

    [Fact]
    public void RetryPreservesOriginalCorrelationAndUsesCurrentRequestId()
    {
        var store = new RecordingJobStore(
            initialSnapshot: JobStoreJson(
                1,
                [
                    JobRow(
                        "job-failed",
                        "vm.start",
                        "failed",
                        "2026-08-01T00:00:00.0000000+00:00",
                        requestId: "req-original",
                        correlationId: "corr-original",
                        error: new SortedDictionary<string, object?>
                        {
                            ["code"] = "PCV_RETRYABLE",
                            ["message"] = "Retryable failure.",
                            ["detail"] = "Retry this job.",
                            ["retryable"] = true
                        })
                ],
                []));
        var runtime = new DesktopNodeJobRuntime(store);

        var retry = runtime.Retry(
            "job-failed",
            new DesktopNodeJobRequestContext("req-retry"));

        Assert.Equal(DesktopNodeJobCommandOutcome.Retried, retry.Outcome);
        Assert.Equal("req-retry", retry.Job!.RequestId);
        Assert.Equal("corr-original", retry.Job.CorrelationId);
        Assert.Equal("job-failed", retry.Job.RetryOf);
        Assert.Equal(2, retry.Job.Attempt);
        Assert.Equal(retry.Job.JobId, Assert.Single(runtime.Snapshot().Queue));
    }

    [Theory]
    [InlineData("{not-json")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("\"text\"")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("false")]
    public void MalformedOrNonObjectRootStartsInStructuredBlockedState(string snapshot)
    {
        var store = new RecordingJobStore(initialSnapshot: snapshot);

        var runtime = new DesktopNodeJobRuntime(store);

        Assert.Equal("PCV_JOB_STORE_CORRUPT", runtime.LoadBlock!.Code);
        Assert.False(runtime.LoadBlock.Retryable);
        Assert.False(string.IsNullOrWhiteSpace(runtime.LoadBlock.RecommendedAction));
        Assert.Empty(runtime.Snapshot().Jobs);
        Assert.Empty(runtime.Snapshot().Queue);
        Assert.Null(runtime.TryStartNext(() =>
            throw new InvalidOperationException("A corrupt job store must not dispatch provider work.")));
        Assert.Equal(0, store.WriteCount);
        Assert.Empty(store.QuarantineSuffixes);
        Assert.Equal(snapshot, store.DurableSnapshot);
    }

    [Fact]
    public void SemanticIntegrityViolationBlocksWithoutPartialPublishOrWrite()
    {
        var original = JobStoreJson(
            version: 1,
            jobs:
            [
                JobRow("job-duplicate", "vm.start", "queued", "2026-08-01T00:00:00.0000000+00:00"),
                JobRow("job-duplicate", "vm.start", "queued", "2026-08-01T00:01:00.0000000+00:00")
            ],
            queue: ["job-duplicate", "job-duplicate"]);
        var store = new RecordingJobStore(initialSnapshot: original);

        var runtime = new DesktopNodeJobRuntime(store);

        Assert.Equal("PCV_JOB_STORE_CORRUPT", runtime.LoadBlock!.Code);
        Assert.Empty(runtime.Snapshot().Jobs);
        Assert.Empty(runtime.Snapshot().Queue);
        Assert.Equal(0, store.WriteCount);
        Assert.Empty(store.QuarantineSuffixes);
        Assert.Equal(original, store.DurableSnapshot);
    }

    private static DesktopNodeJobSnapshot CreateStartJob(
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
                    ["name"] = "alpha",
                    ["command"] = new[] { "cmd", "/c", "ver" }
                }),
                JobId: jobId),
            new DesktopNodeJobRequestContext("req-" + jobId));
    }

    private static DesktopNodeJobExecutionOutcome DesktopNodeJobExecutionOutcomeSuccess(
        string operation,
        string name)
    {
        return new DesktopNodeJobExecutionOutcome(
            true,
            JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
            {
                ["ok"] = true,
                ["operation"] = operation,
                ["data"] = new SortedDictionary<string, object?>
                {
                    ["name"] = name
                },
                ["error"] = null
            }),
            null);
    }

    private static JsonElement NativeFailure(string operation, string code)
    {
        return JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
        {
            ["ok"] = false,
            ["operation"] = operation,
            ["data"] = null,
            ["error"] = new SortedDictionary<string, object?>
            {
                ["code"] = code,
                ["message"] = "Canceled.",
                ["detail"] = "Cancellation was requested.",
                ["retryable"] = true
            }
        });
    }

    private static SortedDictionary<string, object?> PersistedError(string code, bool retryable)
    {
        return new SortedDictionary<string, object?>
        {
            ["code"] = code,
            ["message"] = "Persisted test failure.",
            ["detail"] = "Persisted test detail.",
            ["retryable"] = retryable
        };
    }

    private static JsonElement JsonElementFromRaw(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string Sha256(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static SortedDictionary<string, object?> JobRow(
        string jobId,
        string operation,
        string status,
        string timestamp,
        string? requestId = null,
        string? correlationId = null,
        SortedDictionary<string, object?>? error = null,
        object? result = null,
        string? canceledAt = null)
    {
        return new SortedDictionary<string, object?>
        {
            ["attempt"] = 1,
            ["canceled_at"] = canceledAt,
            ["correlation_id"] = correlationId ?? jobId,
            ["created_at"] = timestamp,
            ["error"] = error,
            ["job_id"] = jobId,
            ["operation"] = operation,
            ["params"] = new SortedDictionary<string, object?>
            {
                ["name"] = jobId
            },
            ["request_id"] = requestId,
            ["result"] = result,
            ["retry_of"] = null,
            ["status"] = status,
            ["updated_at"] = timestamp
        };
    }

    private static string JobStoreJson(
        int version,
        IEnumerable<SortedDictionary<string, object?>> jobs,
        IEnumerable<string> queue)
    {
        return JsonSerializer.Serialize(new SortedDictionary<string, object?>
        {
            ["jobs"] = jobs.ToArray(),
            ["queue"] = queue.ToArray(),
            ["saved_at"] = "2026-08-01T00:00:00.0000000+00:00",
            ["version"] = version
        });
    }

    private static void AssertStoreSnapshot(
        string json,
        string jobId,
        string status,
        string[] queue,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset savedAt)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var job = Assert.Single(root.GetProperty("jobs").EnumerateArray());
        Assert.Equal(jobId, job.GetProperty("job_id").GetString());
        Assert.Equal(status, job.GetProperty("status").GetString());
        Assert.Equal(createdAt.ToString("o"), job.GetProperty("created_at").GetString());
        Assert.Equal(updatedAt.ToString("o"), job.GetProperty("updated_at").GetString());
        Assert.Equal(savedAt.ToString("o"), root.GetProperty("saved_at").GetString());
        Assert.Equal(queue, root.GetProperty("queue").EnumerateArray().Select(item => item.GetString()).ToArray());
    }

    private sealed class RecordingJobStore : IDesktopNodeJobStore
    {
        private readonly IList<string>? trace;

        public RecordingJobStore(
            string? initialSnapshot = null,
            IList<string>? trace = null)
        {
            DurableSnapshot = initialSnapshot;
            ExistsValue = initialSnapshot is not null;
            this.trace = trace;
        }

        public string Location { get; } = "recording://jobs.json";

        public bool ExistsValue { get; private set; }

        public string? DurableSnapshot { get; private set; }

        public int? FailOnWriteAttempt { get; set; }

        public int WriteCount { get; private set; }

        public List<string> AttemptedSnapshots { get; } = [];

        public List<string> QuarantineSuffixes { get; } = [];

        public bool Exists() => ExistsValue;

        public string ReadSnapshot() => DurableSnapshot
            ?? throw new InvalidOperationException("No durable recording snapshot exists.");

        public DesktopNodeJobStoreWriteResult WriteSnapshot(string json)
        {
            WriteCount++;
            trace?.Add($"store.write:{WriteCount}");
            AttemptedSnapshots.Add(json);
            if (FailOnWriteAttempt == WriteCount)
            {
                return DesktopNodeJobStoreWriteResult.NotCommitted(
                    new IOException($"Injected store write failure at attempt {WriteCount}."));
            }

            DurableSnapshot = json;
            ExistsValue = true;
            return DesktopNodeJobStoreWriteResult.Committed;
        }

        public void Quarantine(string suffix)
        {
            QuarantineSuffixes.Add(suffix);
            ExistsValue = false;
        }
    }

    private sealed class SequenceJobClock(
        IReadOnlyList<DateTimeOffset> values,
        IList<string>? trace = null) : IDesktopNodeJobClock
    {
        public int ReadCount { get; private set; }

        public DateTimeOffset UtcNow
        {
            get
            {
                ReadCount++;
                trace?.Add($"clock:{ReadCount}");
                if (ReadCount > values.Count)
                {
                    throw new InvalidOperationException($"The recording clock has no value for read {ReadCount}.");
                }

                return values[ReadCount - 1];
            }
        }
    }
}
