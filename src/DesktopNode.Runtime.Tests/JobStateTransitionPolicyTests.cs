using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class JobStateTransitionPolicyTests
{
    [Fact]
    public void NewJobStartsQueued()
    {
        var parameters = new Dictionary<string, object?> { ["name"] = "alpha" };

        var job = DesktopNodeJob.CreateQueued("job-1", "vm.create", parameters);

        Assert.Equal(JobStatus.Queued, job.Status);
        Assert.Same(parameters, job.Parameters);
        Assert.Null(job.RetryOf);
        Assert.Equal(1, job.Attempt);
        Assert.Null(job.Result);
        Assert.Null(job.Error);
    }

    [Fact]
    public void QueuedJobCanTransitionToRunning()
    {
        var job = DesktopNodeJob.CreateQueued("job-1", "vm.create");

        var transition = JobStateTransitionPolicy.Start(job);

        Assert.True(transition.Ok);
        Assert.Equal(JobStatus.Running, transition.Job!.Status);
        Assert.Null(transition.Error);
    }

    [Fact]
    public void RunningJobCanTransitionToSucceeded()
    {
        var running = JobStateTransitionPolicy.Start(
            DesktopNodeJob.CreateQueued("job-1", "vm.create")).Job!;
        var helperResult = new Dictionary<string, object?> { ["ok"] = true };

        var transition = JobStateTransitionPolicy.Complete(
            running,
            HelperExecutionResult.Success(helperResult));

        Assert.True(transition.Ok);
        Assert.Equal(JobStatus.Succeeded, transition.Job!.Status);
        Assert.Same(helperResult, transition.Job.Result);
        Assert.Null(transition.Job.Error);
    }

    [Fact]
    public void RunningJobCanTransitionToFailed()
    {
        var running = JobStateTransitionPolicy.Start(
            DesktopNodeJob.CreateQueued("job-1", "vm.create")).Job!;
        var error = new JobError(
            "PCV_HELPER_EXIT_FAILED",
            "The helper failed.",
            "helper exit code was non-zero",
            Retryable: true);

        var transition = JobStateTransitionPolicy.Complete(
            running,
            HelperExecutionResult.Failure(error));

        Assert.True(transition.Ok);
        Assert.Equal(JobStatus.Failed, transition.Job!.Status);
        Assert.Null(transition.Job.Result);
        Assert.Same(error, transition.Job.Error);
    }

    [Fact]
    public void QueuedJobCanTransitionToCanceled()
    {
        var job = DesktopNodeJob.CreateQueued("job-1", "vm.create");

        var transition = JobStateTransitionPolicy.Cancel(job);

        Assert.True(transition.Ok);
        Assert.Equal(JobStatus.Canceled, transition.Job!.Status);
        Assert.Equal("PCV_JOB_CANCELED", transition.Job.Error!.Code);
        Assert.False(transition.Job.Error.Retryable);
    }

    [Fact]
    public void NonQueuedJobCannotBeCanceled()
    {
        var running = JobStateTransitionPolicy.Start(
            DesktopNodeJob.CreateQueued("job-1", "vm.create")).Job!;

        var transition = JobStateTransitionPolicy.Cancel(running);

        Assert.False(transition.Ok);
        Assert.Null(transition.Job);
        Assert.Equal("PCV_JOB_NOT_CANCELABLE", transition.Error!.Code);
        Assert.False(transition.Error.Retryable);
    }

    [Fact]
    public void CancelRejectsRunningJobWithStableOperatorMessage()
    {
        var job = DesktopNodeJob.CreateQueued("job-running", "vm.start") with
        {
            Status = JobStatus.Running
        };

        var result = JobStateTransitionPolicy.Cancel(job);

        Assert.False(result.Ok);
        Assert.Equal("PCV_JOB_NOT_CANCELABLE", result.Error!.Code);
        Assert.Contains("Only queued jobs can be canceled", result.Error.Detail);
    }

    [Fact]
    public void PersistedRunningJobRecoversAsInterruptedNonRetryableFailure()
    {
        var running = JobStateTransitionPolicy.Start(
            DesktopNodeJob.CreateQueued("job-1", "vm.create")).Job!;

        var recovered = JobStateTransitionPolicy.RecoverPersistedRunningJob(running);

        Assert.Equal(JobStatus.Failed, recovered.Status);
        Assert.Null(recovered.Result);
        Assert.Equal("PCV_JOB_INTERRUPTED", recovered.Error!.Code);
        Assert.False(recovered.Error.Retryable);
        Assert.Contains("reconcile", recovered.Error.RecommendedAction, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RetryableFailedJobCreatesNewQueuedRetryJob()
    {
        var parameters = new Dictionary<string, object?> { ["name"] = "retry-me" };
        var failed = DesktopNodeJob.CreateQueued("job-1", "vm.create", parameters) with
        {
            Status = JobStatus.Failed,
            Error = new JobError("PCV_HELPER_EXIT_FAILED", "failed", "detail", Retryable: true)
        };

        var retry = JobStateTransitionPolicy.Retry(failed, "job-2");

        Assert.True(retry.Ok);
        Assert.Equal("job-2", retry.Job!.JobId);
        Assert.Equal(JobStatus.Queued, retry.Job.Status);
        Assert.Equal("job-1", retry.Job.RetryOf);
        Assert.Equal(2, retry.Job.Attempt);
        Assert.Equal("vm.create", retry.Job.Operation);
        Assert.Same(parameters, retry.Job.Parameters);
    }

    [Fact]
    public void RetryDoesNotMutateOriginalFailedJob()
    {
        var failed = DesktopNodeJob.CreateQueued("job-1", "vm.create") with
        {
            Status = JobStatus.Failed,
            Error = new JobError("PCV_HELPER_EXIT_FAILED", "failed", "detail", Retryable: true)
        };

        _ = JobStateTransitionPolicy.Retry(failed, "job-2");

        Assert.Equal(JobStatus.Failed, failed.Status);
        Assert.Null(failed.RetryOf);
        Assert.Equal(1, failed.Attempt);
    }

    [Fact]
    public void NonFailedJobCannotBeRetried()
    {
        var queued = DesktopNodeJob.CreateQueued("job-1", "vm.create");

        var retry = JobStateTransitionPolicy.Retry(queued, "job-2");

        Assert.False(retry.Ok);
        Assert.Null(retry.Job);
        Assert.Equal("PCV_JOB_NOT_RETRYABLE", retry.Error!.Code);
    }

    [Fact]
    public void NonRetryableFailedJobCannotBeRetried()
    {
        var failed = DesktopNodeJob.CreateQueued("job-1", "vm.create") with
        {
            Status = JobStatus.Failed,
            Error = new JobError("PCV_HELPER_EXIT_FAILED", "failed", "detail", Retryable: false)
        };

        var retry = JobStateTransitionPolicy.Retry(failed, "job-2");

        Assert.False(retry.Ok);
        Assert.Null(retry.Job);
        Assert.Equal("PCV_JOB_NOT_RETRYABLE", retry.Error!.Code);
    }

    [Fact]
    public void RetryAttemptLimitIsEnforced()
    {
        var failed = DesktopNodeJob.CreateQueued("job-1", "vm.create", attempt: 3) with
        {
            Status = JobStatus.Failed,
            Error = new JobError("PCV_HELPER_EXIT_FAILED", "failed", "detail", Retryable: true)
        };

        var retry = JobStateTransitionPolicy.Retry(failed, "job-2");

        Assert.False(retry.Ok);
        Assert.Null(retry.Job);
        Assert.Equal("PCV_JOB_RETRY_LIMIT_REACHED", retry.Error!.Code);
    }

    [Fact]
    public void RetryRejectsMaxIntAttemptWithoutOverflowing()
    {
        var failed = DesktopNodeJob.CreateQueued("job-1", "vm.create", attempt: int.MaxValue) with
        {
            Status = JobStatus.Failed,
            Error = new JobError("PCV_HELPER_EXIT_FAILED", "failed", "detail", Retryable: true)
        };

        var retry = JobStateTransitionPolicy.Retry(
            failed,
            "job-2",
            maxAttempts: int.MaxValue);

        Assert.False(retry.Ok);
        Assert.Null(retry.Job);
        Assert.Equal("PCV_JOB_RETRY_LIMIT_REACHED", retry.Error!.Code);
    }

    [Theory]
    [InlineData(JobStatus.Succeeded)]
    [InlineData(JobStatus.Failed)]
    [InlineData(JobStatus.Canceled)]
    public void SucceededFailedAndCanceledAreTerminalForDirectStateTransitions(JobStatus terminalStatus)
    {
        var decision = JobStateTransitionPolicy.CanDirectlyTransition(
            terminalStatus,
            JobStatus.Running);

        Assert.False(decision.Ok);
        Assert.Equal("PCV_JOB_TRANSITION_TERMINAL", decision.Error!.Code);
    }
}
