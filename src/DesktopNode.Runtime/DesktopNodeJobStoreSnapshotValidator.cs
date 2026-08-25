using System.Globalization;
using System.Text.Json;

namespace DesktopNode.Runtime;

internal enum DesktopNodeJobStoreSnapshotValidationKind
{
    Valid,
    UnsupportedFuture,
    Corrupt
}

internal sealed record DesktopNodeJobStoreSnapshotValidationResult(
    DesktopNodeJobStoreSnapshotValidationKind Kind,
    int? SchemaVersion,
    JsonElement? Root,
    string? ErrorCode,
    string? Detail)
{
    public static DesktopNodeJobStoreSnapshotValidationResult Valid(
        int schemaVersion,
        JsonElement root) =>
        new(
            DesktopNodeJobStoreSnapshotValidationKind.Valid,
            schemaVersion,
            root.Clone(),
            null,
            null);

    public static DesktopNodeJobStoreSnapshotValidationResult UnsupportedFuture(int schemaVersion) =>
        new(
            DesktopNodeJobStoreSnapshotValidationKind.UnsupportedFuture,
            schemaVersion,
            null,
            "PCV_JOB_STORE_SCHEMA_UNSUPPORTED",
            $"Job store schema version {schemaVersion} is newer than the supported v1/v2 contract.");

    public static DesktopNodeJobStoreSnapshotValidationResult Corrupt(string detail) =>
        new(
            DesktopNodeJobStoreSnapshotValidationKind.Corrupt,
            null,
            null,
            "PCV_JOB_STORE_CORRUPT",
            detail);
}

internal static class DesktopNodeJobStoreSnapshotValidator
{
    private static readonly HashSet<string> SupportedStatuses = new(StringComparer.Ordinal)
    {
        "queued",
        "running",
        "succeeded",
        "failed",
        "canceled"
    };

    public static DesktopNodeJobStoreSnapshotValidationResult Validate(string snapshotJson)
    {
        if (snapshotJson is null)
        {
            return DesktopNodeJobStoreSnapshotValidationResult.Corrupt(
                "The job store snapshot is missing.");
        }

        try
        {
            using var document = JsonDocument.Parse(snapshotJson);
            return Validate(document.RootElement);
        }
        catch (JsonException)
        {
            return DesktopNodeJobStoreSnapshotValidationResult.Corrupt(
                "The job store snapshot is not valid JSON.");
        }
    }

    internal static DesktopNodeJobStoreSnapshotValidationResult Validate(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return DesktopNodeJobStoreSnapshotValidationResult.Corrupt(
                "The job store root must be a JSON object.");
        }

        var versionResult = ReadSchemaVersion(root);
        if (versionResult.Result is not null)
        {
            return versionResult.Result;
        }

        var schemaVersion = versionResult.Version;
        if (!root.TryGetProperty("jobs", out var jobsElement) ||
            jobsElement.ValueKind != JsonValueKind.Array)
        {
            return Corrupt("The job store 'jobs' field must be an array.");
        }

        if (!root.TryGetProperty("queue", out var queueElement) ||
            queueElement.ValueKind != JsonValueKind.Array)
        {
            return Corrupt("The job store 'queue' field must be an array.");
        }

        var jobs = new Dictionary<string, ValidatedJob>(StringComparer.Ordinal);
        var jobIndex = 0;
        foreach (var jobElement in jobsElement.EnumerateArray())
        {
            var jobResult = ValidateJob(jobElement, jobIndex);
            if (jobResult.Result is not null)
            {
                return jobResult.Result;
            }

            var job = jobResult.Job!;
            if (!jobs.TryAdd(job.JobId, job))
            {
                return Corrupt($"Job id '{job.JobId}' is duplicated.");
            }

            jobIndex++;
        }

        var queueReferenceCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var queueIndex = 0;
        foreach (var queueItem in queueElement.EnumerateArray())
        {
            if (queueItem.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(queueItem.GetString()))
            {
                return Corrupt($"Queue entry {queueIndex} must be a non-empty job id string.");
            }

            var jobId = queueItem.GetString()!;
            if (!jobs.TryGetValue(jobId, out var referencedJob))
            {
                return Corrupt($"Queue entry '{jobId}' does not reference an existing job.");
            }

            if (referencedJob.Status is not ("queued" or "running"))
            {
                return Corrupt(
                    $"Queue entry '{jobId}' references terminal job status '{referencedJob.Status}'.");
            }

            var referenceCount = queueReferenceCounts.GetValueOrDefault(jobId) + 1;
            if (referenceCount > 1)
            {
                return Corrupt($"Queue entry '{jobId}' is duplicated.");
            }

            queueReferenceCounts[jobId] = referenceCount;
            queueIndex++;
        }

        foreach (var job in jobs.Values)
        {
            var referenceCount = queueReferenceCounts.GetValueOrDefault(job.JobId);
            if (string.Equals(job.Status, "queued", StringComparison.Ordinal) && referenceCount != 1)
            {
                return Corrupt($"Queued job '{job.JobId}' must appear in the queue exactly once.");
            }

            if (job.Status is not ("queued" or "running") && referenceCount != 0)
            {
                return Corrupt($"Terminal job '{job.JobId}' must not appear in the queue.");
            }
        }

        return DesktopNodeJobStoreSnapshotValidationResult.Valid(schemaVersion, root);
    }

    private static (int Version, DesktopNodeJobStoreSnapshotValidationResult? Result) ReadSchemaVersion(
        JsonElement root)
    {
        if (!root.TryGetProperty("version", out var versionElement))
        {
            return (1, null);
        }

        if (versionElement.ValueKind != JsonValueKind.Number ||
            !versionElement.TryGetInt32(out var version) ||
            version < 1)
        {
            return (0, Corrupt("The job store schema version must be a positive integer."));
        }

        if (version > 2)
        {
            return (version, DesktopNodeJobStoreSnapshotValidationResult.UnsupportedFuture(version));
        }

        return (version, null);
    }

    private static (ValidatedJob? Job, DesktopNodeJobStoreSnapshotValidationResult? Result) ValidateJob(
        JsonElement jobElement,
        int index)
    {
        if (jobElement.ValueKind != JsonValueKind.Object)
        {
            return (null, Corrupt($"Job entry {index} must be a JSON object."));
        }

        if (!TryReadRequiredString(jobElement, "job_id", out var jobId))
        {
            return (null, Corrupt($"Job entry {index} must have a non-empty string 'job_id'."));
        }

        if (!TryReadRequiredString(jobElement, "operation", out _))
        {
            return (null, Corrupt($"Job '{jobId}' must have a non-empty string 'operation'."));
        }

        if (!jobElement.TryGetProperty("params", out var parameters) ||
            parameters.ValueKind != JsonValueKind.Object)
        {
            return (null, Corrupt($"Job '{jobId}' params must be a required JSON object."));
        }

        if (!TryReadRequiredString(jobElement, "status", out var status) ||
            !SupportedStatuses.Contains(status))
        {
            return (null, Corrupt($"Job '{jobId}' has an unsupported status."));
        }

        var createdAtResult = ReadRequiredTimestamp(jobElement, "created_at", jobId);
        if (createdAtResult.Result is not null)
        {
            return (null, createdAtResult.Result);
        }

        var updatedAtResult = ReadRequiredTimestamp(jobElement, "updated_at", jobId);
        if (updatedAtResult.Result is not null)
        {
            return (null, updatedAtResult.Result);
        }

        var attempt = 1;
        if (jobElement.TryGetProperty("attempt", out var attemptElement) &&
            (attemptElement.ValueKind != JsonValueKind.Number ||
             !attemptElement.TryGetInt32(out attempt) ||
             attempt < 1))
        {
            return (null, Corrupt($"Job '{jobId}' attempt must be an integer greater than or equal to 1."));
        }

        var resultPresent = TryReadNonNull(jobElement, "result", out _);
        var errorResult = ValidateError(jobElement, jobId);
        if (errorResult.Result is not null)
        {
            return (null, errorResult.Result);
        }

        var errorPresent = errorResult.ErrorCode is not null;
        var canceledAtResult = ReadOptionalTimestamp(jobElement, "canceled_at", jobId);
        if (canceledAtResult.Result is not null)
        {
            return (null, canceledAtResult.Result);
        }

        var canceledAtPresent = canceledAtResult.Present;
        DesktopNodeJobStoreSnapshotValidationResult? combinationFailure = status switch
        {
            "queued" when resultPresent || errorPresent || canceledAtPresent =>
                Corrupt($"Queued job '{jobId}' must not contain result, error, or canceled_at state."),
            "running" when resultPresent =>
                Corrupt($"Running job '{jobId}' must not contain a result."),
            "running" when errorPresent &&
                (!string.Equals(errorResult.ErrorCode, "PCV_JOB_CANCEL_REQUESTED", StringComparison.Ordinal) ||
                 !canceledAtPresent) =>
                Corrupt($"Running job '{jobId}' may contain only a persisted cancellation request error."),
            "running" when !errorPresent && canceledAtPresent =>
                Corrupt($"Running job '{jobId}' has canceled_at without a cancellation request error."),
            "succeeded" when errorPresent =>
                Corrupt($"Succeeded job '{jobId}' must not contain an error."),
            "failed" when resultPresent || !errorPresent =>
                Corrupt($"Failed job '{jobId}' must contain an error and must not contain a result."),
            "canceled" when !errorPresent ||
                !string.Equals(errorResult.ErrorCode, "PCV_JOB_CANCELED", StringComparison.Ordinal) ||
                !canceledAtPresent =>
                Corrupt($"Canceled job '{jobId}' must contain PCV_JOB_CANCELED and canceled_at."),
            _ => null
        };

        return combinationFailure is null
            ? (new ValidatedJob(jobId, status, attempt), null)
            : (null, combinationFailure);
    }

    private static (string? ErrorCode, DesktopNodeJobStoreSnapshotValidationResult? Result) ValidateError(
        JsonElement jobElement,
        string jobId)
    {
        if (!TryReadNonNull(jobElement, "error", out var errorElement))
        {
            return (null, null);
        }

        if (errorElement.ValueKind != JsonValueKind.Object ||
            !TryReadRequiredString(errorElement, "code", out var errorCode) ||
            !TryReadRequiredString(errorElement, "message", out _) ||
            !TryReadRequiredString(errorElement, "detail", out _) ||
            !errorElement.TryGetProperty("retryable", out var retryableElement) ||
            retryableElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return (null, Corrupt($"Job '{jobId}' error does not match the persisted error contract."));
        }

        if (errorElement.TryGetProperty("recommended_action", out var actionElement) &&
            actionElement.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
        {
            return (null, Corrupt($"Job '{jobId}' error recommended_action must be a string or null."));
        }

        return (errorCode, null);
    }

    private static (bool Present, DesktopNodeJobStoreSnapshotValidationResult? Result) ReadOptionalTimestamp(
        JsonElement element,
        string propertyName,
        string jobId)
    {
        if (!TryReadNonNull(element, propertyName, out var value))
        {
            return (false, null);
        }

        if (value.ValueKind != JsonValueKind.String ||
            !DateTimeOffset.TryParse(
                value.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out _))
        {
            return (false, Corrupt($"Job '{jobId}' {propertyName} must be a timestamp or null."));
        }

        return (true, null);
    }

    private static (DateTimeOffset Value, DesktopNodeJobStoreSnapshotValidationResult? Result) ReadRequiredTimestamp(
        JsonElement element,
        string propertyName,
        string jobId)
    {
        if (!element.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.String ||
            !DateTimeOffset.TryParse(
                value.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            return (default, Corrupt($"Job '{jobId}' {propertyName} must be a required timestamp string."));
        }

        return (parsed, null);
    }

    private static bool TryReadRequiredString(
        JsonElement element,
        string propertyName,
        out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            return false;
        }

        value = property.GetString()!;
        return true;
    }

    private static bool TryReadNonNull(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value) && value.ValueKind != JsonValueKind.Null)
        {
            return true;
        }

        value = default;
        return false;
    }

    private static DesktopNodeJobStoreSnapshotValidationResult Corrupt(string detail) =>
        DesktopNodeJobStoreSnapshotValidationResult.Corrupt(detail);

    private sealed record ValidatedJob(string JobId, string Status, int Attempt);
}
