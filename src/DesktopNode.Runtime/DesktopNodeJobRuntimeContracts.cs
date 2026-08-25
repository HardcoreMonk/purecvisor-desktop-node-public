using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.Runtime;

public interface IDesktopNodeJobStore
{
    string Location { get; }

    bool Exists();

    string ReadSnapshot();

    /// <remarks>
    /// Implementations must not invoke mutating <see cref="DesktopNodeJobRuntime"/> commands while
    /// the write is in progress. Read-only observation for test instrumentation is permitted.
    /// Implementations must return <see cref="DesktopNodeJobStoreCommitOutcome.Indeterminate"/>
    /// when they cannot prove whether the candidate became the authoritative snapshot.
    /// Persistent implementations must retain a restart-readable guard for that outcome and make
    /// <see cref="Exists"/> or <see cref="ReadSnapshot"/> fail closed until the authoritative
    /// candidate or previous snapshot has been reconciled.
    /// </remarks>
    DesktopNodeJobStoreWriteResult WriteSnapshot(string json);

    void Quarantine(string suffix);
}

public enum DesktopNodeJobStoreCommitOutcome
{
    Committed,
    NotCommitted,
    Indeterminate
}

public sealed record DesktopNodeJobStoreWriteResult
{
    private DesktopNodeJobStoreWriteResult(
        DesktopNodeJobStoreCommitOutcome outcome,
        Exception? failure = null)
    {
        Outcome = outcome;
        Failure = failure;
    }

    public DesktopNodeJobStoreCommitOutcome Outcome { get; }

    public Exception? Failure { get; }

    public static DesktopNodeJobStoreWriteResult Committed { get; } = new(
        DesktopNodeJobStoreCommitOutcome.Committed);

    public static DesktopNodeJobStoreWriteResult NotCommitted(Exception failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new DesktopNodeJobStoreWriteResult(
            DesktopNodeJobStoreCommitOutcome.NotCommitted,
            failure);
    }

    public static DesktopNodeJobStoreWriteResult Indeterminate(Exception failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new DesktopNodeJobStoreWriteResult(
            DesktopNodeJobStoreCommitOutcome.Indeterminate,
            failure);
    }
}

public sealed class DesktopNodeJobStoreCommitException : IOException
{
    public DesktopNodeJobStoreCommitException(
        DesktopNodeJobStoreCommitOutcome outcome,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        if (outcome == DesktopNodeJobStoreCommitOutcome.Committed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "A committed write is not an exception.");
        }

        Outcome = outcome;
    }

    public DesktopNodeJobStoreCommitOutcome Outcome { get; }
}

public interface IDesktopNodeJobClock
{
    DateTimeOffset UtcNow { get; }
}

internal sealed class SystemDesktopNodeJobClock : IDesktopNodeJobClock
{
    public static SystemDesktopNodeJobClock Instance { get; } = new();

    private SystemDesktopNodeJobClock()
    {
    }

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed record DesktopNodeJobRequestContext(
    string RequestId,
    string? CorrelationId = null);

public sealed record DesktopNodeJobCreateCommand(
    string Operation,
    JsonElement Parameters,
    string? RetryOf = null,
    int Attempt = 1,
    string? JobId = null);

public sealed record DesktopNodeJobRuntimeError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("retryable")] bool Retryable,
    [property: JsonPropertyName("recommended_action")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RecommendedAction = null);

public sealed record DesktopNodeJobRuntimeObservation(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("commit_outcome")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? CommitOutcome,
    [property: JsonPropertyName("occurred_at")] string OccurredAt,
    [property: JsonPropertyName("recommended_action")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RecommendedAction);

public interface IDesktopNodeJobRuntimeEventSink
{
    void Write(DesktopNodeJobRuntimeObservation observation);
}

public sealed record DesktopNodeJobStoreHealthSnapshot(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("mutation_blocked")] bool MutationBlocked,
    [property: JsonPropertyName("error_code")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? ErrorCode,
    [property: JsonPropertyName("recent_events")] IReadOnlyList<DesktopNodeJobRuntimeObservation> RecentEvents);

public sealed class DesktopNodeJobStoreWriteException : Exception
{
    internal DesktopNodeJobStoreWriteException(
        DesktopNodeJobRuntimeError error,
        DesktopNodeJobStoreCommitOutcome commitOutcome,
        Exception innerException)
        : base(error.Message, innerException)
    {
        Error = error;
        CommitOutcome = commitOutcome;
    }

    public DesktopNodeJobRuntimeError Error { get; }

    public DesktopNodeJobStoreCommitOutcome CommitOutcome { get; }
}

public sealed record DesktopNodeJobSnapshot(
    [property: JsonPropertyName("attempt")] int Attempt,
    [property: JsonPropertyName("canceled_at")] string? CanceledAt,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("error")] DesktopNodeJobRuntimeError? Error,
    [property: JsonPropertyName("job_id")] string JobId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("params")] JsonElement Parameters,
    [property: JsonPropertyName("request_id")] string? RequestId,
    [property: JsonPropertyName("result")] JsonElement? Result,
    [property: JsonPropertyName("retry_of")] string? RetryOf,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("updated_at")] string UpdatedAt);

public sealed record DesktopNodeJobRuntimeSnapshot(
    IReadOnlyList<DesktopNodeJobSnapshot> Jobs,
    IReadOnlyList<string> Queue,
    int SchemaVersion,
    int PrunedTerminalJobs,
    int MaxRetainedTerminalJobs,
    DesktopNodeJobRuntimeError? LoadBlock,
    DesktopNodeJobStoreHealthSnapshot StoreHealth);

public enum DesktopNodeJobCommandOutcome
{
    Found,
    Canceled,
    CancellationRequested,
    Retried,
    NotFound,
    Rejected
}

public sealed record DesktopNodeJobCommandResult(
    DesktopNodeJobCommandOutcome Outcome,
    DesktopNodeJobSnapshot? Job,
    DesktopNodeJobRuntimeError? Error);

public enum DesktopNodeJobReconciliationOutcome
{
    Reconciled,
    Required,
    NotFound,
    Rejected
}

public sealed record DesktopNodeJobReconciliationAssessment(
    bool PostconditionConfirmed,
    string Classification,
    JsonElement? Result,
    DesktopNodeJobRuntimeError? Error = null);

public sealed record DesktopNodeJobReconciliationResult(
    DesktopNodeJobReconciliationOutcome Outcome,
    DesktopNodeJobSnapshot? Job,
    DesktopNodeJobRuntimeError? Error);

public sealed record DesktopNodeStartedJob(
    string JobId,
    string Operation,
    JsonElement Parameters);

public sealed record DesktopNodeJobExecutionOutcome(
    bool Ok,
    JsonElement ProviderResult,
    JobError? Error,
    bool CancellationAcknowledged = false);

public sealed record DesktopNodeJobCompletionResult(
    bool Processed,
    DesktopNodeJobSnapshot? Job);
