namespace DesktopNode.Runtime;

public enum JobStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Canceled
}

public sealed record JobError(
    string Code,
    string Message,
    string Detail,
    bool Retryable,
    string? RecommendedAction = null);

public sealed record DesktopNodeJob(
    string JobId,
    string Operation,
    JobStatus Status,
    object? Parameters,
    object? Result,
    JobError? Error,
    string? RetryOf,
    int Attempt)
{
    public static DesktopNodeJob CreateQueued(
        string jobId,
        string operation,
        object? parameters = null,
        string? retryOf = null,
        int attempt = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentOutOfRangeException.ThrowIfLessThan(attempt, 1);

        return new DesktopNodeJob(
            JobId: jobId,
            Operation: operation,
            Status: JobStatus.Queued,
            Parameters: parameters,
            Result: null,
            Error: null,
            RetryOf: retryOf,
            Attempt: attempt);
    }
}

public sealed record HelperExecutionResult(
    bool Ok,
    object? Result,
    JobError? Error)
{
    public static HelperExecutionResult Success(object? result) =>
        new(Ok: true, Result: result, Error: null);

    public static HelperExecutionResult Failure(JobError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new HelperExecutionResult(Ok: false, Result: null, Error: error);
    }
}

public sealed record JobTransitionDecision(
    bool Ok,
    JobError? Error)
{
    public static JobTransitionDecision Allowed { get; } =
        new(Ok: true, Error: null);

    public static JobTransitionDecision Rejected(JobError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new JobTransitionDecision(Ok: false, Error: error);
    }
}

public sealed record JobTransitionResult(
    bool Ok,
    DesktopNodeJob? Job,
    JobError? Error)
{
    public static JobTransitionResult Allowed(DesktopNodeJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        return new JobTransitionResult(Ok: true, Job: job, Error: null);
    }

    public static JobTransitionResult Rejected(JobError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new JobTransitionResult(Ok: false, Job: null, Error: error);
    }
}

public static class JobStateTransitionPolicy
{
    public const int DefaultMaxAttempts = 3;

    public static bool IsTerminal(JobStatus status) =>
        status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Canceled;

    public static JobTransitionDecision CanDirectlyTransition(
        JobStatus from,
        JobStatus to)
    {
        if (IsTerminal(from))
        {
            return JobTransitionDecision.Rejected(NewError(
                "PCV_JOB_TRANSITION_TERMINAL",
                "Terminal jobs cannot transition.",
                $"Job status '{from}' is terminal.",
                retryable: false));
        }

        if ((from, to) is
            (JobStatus.Queued, JobStatus.Running) or
            (JobStatus.Queued, JobStatus.Canceled) or
            (JobStatus.Running, JobStatus.Succeeded) or
            (JobStatus.Running, JobStatus.Failed))
        {
            return JobTransitionDecision.Allowed;
        }

        return JobTransitionDecision.Rejected(NewError(
            "PCV_JOB_TRANSITION_UNSUPPORTED",
            "The requested job transition is not supported.",
            $"Transition from '{from}' to '{to}' is not part of the Desktop Node runtime contract.",
            retryable: false));
    }

    public static JobTransitionResult Start(DesktopNodeJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        var decision = CanDirectlyTransition(job.Status, JobStatus.Running);
        if (!decision.Ok)
        {
            return JobTransitionResult.Rejected(decision.Error!);
        }

        return JobTransitionResult.Allowed(job with
        {
            Status = JobStatus.Running,
            Result = null,
            Error = null
        });
    }

    public static JobTransitionResult Complete(
        DesktopNodeJob job,
        HelperExecutionResult helperResult)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(helperResult);

        var targetStatus = helperResult.Ok ? JobStatus.Succeeded : JobStatus.Failed;
        var decision = CanDirectlyTransition(job.Status, targetStatus);
        if (!decision.Ok)
        {
            return JobTransitionResult.Rejected(decision.Error!);
        }

        if (helperResult.Ok)
        {
            return JobTransitionResult.Allowed(job with
            {
                Status = JobStatus.Succeeded,
                Result = helperResult.Result,
                Error = null
            });
        }

        return JobTransitionResult.Allowed(job with
        {
            Status = JobStatus.Failed,
            Result = null,
            Error = helperResult.Error ?? NewError(
                "PCV_HELPER_EXIT_FAILED",
                "The helper process failed.",
                "The helper returned a failed result without an error payload.",
                retryable: true)
        });
    }

    public static JobTransitionResult Cancel(DesktopNodeJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (job.Status != JobStatus.Queued)
        {
            return JobTransitionResult.Rejected(NewError(
                "PCV_JOB_NOT_CANCELABLE",
                $"Job '{job.JobId}' cannot be canceled.",
                $"Only queued jobs can be canceled. Current status is '{job.Status}'.",
                retryable: false));
        }

        return JobTransitionResult.Allowed(job with
        {
            Status = JobStatus.Canceled,
            Result = null,
            Error = NewError(
                "PCV_JOB_CANCELED",
                "The job was canceled before it started.",
                "Queued jobs can be canceled before a worker begins processing.",
                retryable: false)
        });
    }

    public static DesktopNodeJob RecoverPersistedRunningJob(DesktopNodeJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        if (job.Status != JobStatus.Running)
        {
            return job;
        }

        return job with
        {
            Status = JobStatus.Failed,
            Result = null,
            Error = NewError(
                "PCV_JOB_INTERRUPTED",
                "The job was interrupted before the API process stopped.",
                "A persisted running job cannot be resumed automatically after restart, and the external side effect may have completed before terminal state was persisted.",
                retryable: false,
                recommendedAction: "Inspect provider and Hyper-V readback for this operation, then reconcile the job manually. Do not retry until the prior side effect is known to be absent or safely idempotent.")
        };
    }

    public static JobTransitionResult Retry(
        DesktopNodeJob job,
        string newJobId,
        int maxAttempts = DefaultMaxAttempts)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentException.ThrowIfNullOrWhiteSpace(newJobId);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        if (job.Status != JobStatus.Failed)
        {
            return JobTransitionResult.Rejected(NewError(
                "PCV_JOB_NOT_RETRYABLE",
                $"Job '{job.JobId}' cannot be retried.",
                $"Only failed jobs can be retried. Current status is '{job.Status}'.",
                retryable: false));
        }

        if (job.Error is not { Retryable: true })
        {
            return JobTransitionResult.Rejected(NewError(
                "PCV_JOB_NOT_RETRYABLE",
                $"Job '{job.JobId}' cannot be retried.",
                "Only failed jobs with retryable errors can be retried manually.",
                retryable: false));
        }

        if (job.Attempt < 1)
        {
            return JobTransitionResult.Rejected(NewError(
                "PCV_JOB_INVALID_ATTEMPT",
                $"Job '{job.JobId}' has an invalid retry attempt value.",
                $"Attempt must be at least 1. Current attempt is {job.Attempt}.",
                retryable: false));
        }

        if (job.Attempt >= maxAttempts)
        {
            return JobTransitionResult.Rejected(NewError(
                "PCV_JOB_RETRY_LIMIT_REACHED",
                $"Job '{job.JobId}' reached the manual retry attempt limit.",
                $"Maximum attempts is {maxAttempts}. Current attempt is {job.Attempt}.",
                retryable: false));
        }

        var nextAttempt = job.Attempt + 1;

        return JobTransitionResult.Allowed(DesktopNodeJob.CreateQueued(
            jobId: newJobId,
            operation: job.Operation,
            parameters: job.Parameters,
            retryOf: job.JobId,
            attempt: nextAttempt));
    }

    private static JobError NewError(
        string code,
        string message,
        string detail,
        bool retryable,
        string? recommendedAction = null) =>
        new(
            Code: code,
            Message: message,
            Detail: detail,
            Retryable: retryable,
            RecommendedAction: recommendedAction);
}
