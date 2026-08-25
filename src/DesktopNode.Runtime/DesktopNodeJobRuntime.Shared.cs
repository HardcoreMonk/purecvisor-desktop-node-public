using System.Collections.Concurrent;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Runtime;

public sealed partial class DesktopNodeJobRuntime
{
    private static PolicyAdapterResult ToPolicyJob(MutableJob job)
    {
        if (!TryParseStatus(job.Status, out var status))
        {
            return PolicyAdapterResult.Fail(new DesktopNodeJobRuntimeError(
                "PCV_JOB_STATUS_INVALID",
                $"Job '{job.JobId}' has an invalid status.",
                $"Status '{job.Status}' is not part of the Desktop Node job status contract.",
                false));
        }

        return PolicyAdapterResult.Success(new DesktopNodeJob(
            job.JobId,
            job.Operation,
            status,
            job.Parameters.Clone(),
            job.Result?.Clone(),
            ToPolicyError(job.Error),
            job.RetryOf,
            job.Attempt));
    }

    private static void ApplyPolicyJob(MutableJob target, DesktopNodeJob source)
    {
        target.Status = ToPersistedStatus(source.Status);
        target.Result = source.Result switch
        {
            null => null,
            JsonElement element => element.Clone(),
            _ => JsonSerializer.SerializeToElement(source.Result, RuntimePolicyContract.JsonOptions)
        };
        target.Error = FromPolicyError(source.Error);
    }

    private static DesktopNodeJobRuntimeError? FromPolicyError(JobError? error)
    {
        return error is null
            ? null
            : new DesktopNodeJobRuntimeError(
                error.Code,
                error.Message,
                error.Detail,
                error.Retryable,
                error.RecommendedAction);
    }

    private static JobError? ToPolicyError(DesktopNodeJobRuntimeError? error)
    {
        return error is null
            ? null
            : new JobError(
                error.Code,
                error.Message,
                error.Detail,
                error.Retryable,
                error.RecommendedAction);
    }

    private static DesktopNodeJobCommandResult NotFound(string jobId)
    {
        return new DesktopNodeJobCommandResult(
            DesktopNodeJobCommandOutcome.NotFound,
            null,
            new DesktopNodeJobRuntimeError(
                "PCV_JOB_NOT_FOUND",
                $"Job '{jobId}' was not found.",
                "The job was not found in the current memory store.",
                false));
    }

    private static DesktopNodeJobCommandResult Rejected(DesktopNodeJobRuntimeError error)
    {
        return new DesktopNodeJobCommandResult(
            DesktopNodeJobCommandOutcome.Rejected,
            null,
            error);
    }

    private static DesktopNodeJobSnapshot Project(MutableJob job)
    {
        return new DesktopNodeJobSnapshot(
            job.Attempt,
            job.CanceledAt,
            job.CorrelationId,
            job.CreatedAt,
            job.Error,
            job.JobId,
            job.Operation,
            SanitizeParameters(job.Operation, job.Parameters),
            job.RequestId,
            job.Result?.Clone(),
            job.RetryOf,
            job.Status,
            job.UpdatedAt);
    }

    private static JsonElement SanitizeParameters(string operation, JsonElement parameters)
    {
        if (!operation.StartsWith("vm.guest.", StringComparison.Ordinal) ||
            parameters.ValueKind != JsonValueKind.Object)
        {
            return parameters.Clone();
        }

        var sanitized = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in parameters.EnumerateObject())
        {
            if (string.Equals(property.Name, "credential_ref", StringComparison.Ordinal))
            {
                var value = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : null;
                sanitized["credential_ref_hash"] = string.IsNullOrWhiteSpace(value)
                    ? null
                    : GuestExecutionContractHasher.Hash(value.Trim());
                sanitized["credential_ref"] = "[redacted-ref]";
                continue;
            }

            sanitized[property.Name] = property.Value.Clone();
        }

        return JsonSerializer.SerializeToElement(sanitized, RuntimePolicyContract.JsonOptions);
    }

    private static string? ReadString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ToString();
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.TryGetInt32(out var parsed)
                ? parsed
                : null;
    }

    private static JsonElement? ReadElement(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind != JsonValueKind.Null
                ? value.Clone()
                : null;
    }

    private static DesktopNodeJobRuntimeError? ReadError(JsonElement element, string name)
    {
        var value = ReadElement(element, name);
        return value is null
            ? null
            : value.Value.Deserialize<DesktopNodeJobRuntimeError>(RuntimePolicyContract.JsonOptions);
    }

    private static JsonElement EmptyObject()
    {
        return JsonSerializer.SerializeToElement(
            new SortedDictionary<string, object?>(),
            RuntimePolicyContract.JsonOptions);
    }

    private string Now()
    {
        return clock.UtcNow.ToString("o");
    }

    private static bool TryParseStatus(string status, out JobStatus parsed)
    {
        parsed = status switch
        {
            "queued" => JobStatus.Queued,
            "running" => JobStatus.Running,
            "succeeded" => JobStatus.Succeeded,
            "failed" => JobStatus.Failed,
            "canceled" => JobStatus.Canceled,
            _ => default
        };

        return status is "queued" or "running" or "succeeded" or "failed" or "canceled";
    }

    private static string ToPersistedStatus(JobStatus status)
    {
        return status switch
        {
            JobStatus.Queued => "queued",
            JobStatus.Running => "running",
            JobStatus.Succeeded => "succeeded",
            JobStatus.Failed => "failed",
            JobStatus.Canceled => "canceled",
            _ => "failed"
        };
    }

    private static bool IsTerminalStatus(string status)
    {
        return status is "succeeded" or "failed" or "canceled";
    }

    private static bool IsRunningInterruptEligible(string operation)
    {
        return RuntimePolicyContract.IsRunningInterruptOperation(operation);
    }

    private sealed record PolicyAdapterResult(
        bool Ok,
        DesktopNodeJob? Job,
        DesktopNodeJobRuntimeError? Error)
    {
        public static PolicyAdapterResult Success(DesktopNodeJob job) => new(true, job, null);

        public static PolicyAdapterResult Fail(DesktopNodeJobRuntimeError error) => new(false, null, error);
    }

    private sealed class MutableJob(
        string jobId,
        string operation,
        string status,
        JsonElement parameters,
        JsonElement? result,
        DesktopNodeJobRuntimeError? error,
        string? retryOf,
        string? requestId,
        string correlationId,
        int attempt,
        string? canceledAt,
        string createdAt,
        string updatedAt)
    {
        public string JobId { get; } = jobId;
        public string Operation { get; } = operation;
        public string Status { get; set; } = status;
        public JsonElement Parameters { get; } = parameters.Clone();
        public JsonElement? Result { get; set; } = result?.Clone();
        public DesktopNodeJobRuntimeError? Error { get; set; } = error;
        public string? RetryOf { get; } = retryOf;
        public string? RequestId { get; } = requestId;
        public string CorrelationId { get; } = correlationId;
        public int Attempt { get; } = attempt;
        public string? CanceledAt { get; set; } = canceledAt;
        public string CreatedAt { get; } = createdAt;
        public string UpdatedAt { get; set; } = updatedAt;
    }
}
