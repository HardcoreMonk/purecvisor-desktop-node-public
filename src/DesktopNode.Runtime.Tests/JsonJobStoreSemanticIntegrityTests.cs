using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class JsonJobStoreSemanticIntegrityTests
{
    [Theory]
    [InlineData("{not-json")]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("\"text\"")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("false")]
    public void MalformedOrNonObjectRootReturnsTypedCorruptResult(string snapshot)
    {
        var result = DesktopNodeJobStoreSnapshotValidator.Validate(snapshot);

        Assert.Equal(DesktopNodeJobStoreSnapshotValidationKind.Corrupt, result.Kind);
        Assert.Equal("PCV_JOB_STORE_CORRUPT", result.ErrorCode);
        Assert.Null(result.SchemaVersion);
        Assert.Null(result.Root);
        Assert.False(string.IsNullOrWhiteSpace(result.Detail));
    }

    [Theory]
    [MemberData(nameof(CorruptSemanticCases))]
    public void SemanticIntegrityViolationReturnsTypedCorruptResult(string scenario, string snapshot)
    {
        var result = DesktopNodeJobStoreSnapshotValidator.Validate(snapshot);

        Assert.Equal(DesktopNodeJobStoreSnapshotValidationKind.Corrupt, result.Kind);
        Assert.Equal("PCV_JOB_STORE_CORRUPT", result.ErrorCode);
        Assert.Null(result.Root);
        Assert.False(string.IsNullOrWhiteSpace(result.Detail), scenario);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public void ValidV1AndV2SnapshotsPreserveLegacyMissingVersionCompatibility(
        int? version,
        int expectedVersion)
    {
        var queued = Job("job-queued", "queued");
        var running = Job("job-running", "running");
        var cancelRequested = Job("job-cancel-requested", "running");
        cancelRequested["error"] = Error("PCV_JOB_CANCEL_REQUESTED");
        cancelRequested["canceled_at"] = Timestamp;
        var succeeded = Job("job-succeeded", "succeeded");
        succeeded["result"] = new SortedDictionary<string, object?> { ["ok"] = true };
        var failed = Job("job-failed", "failed");
        failed["error"] = Error("PCV_TEST_FAILURE");
        var canceled = Job("job-canceled", "canceled");
        canceled["error"] = Error("PCV_JOB_CANCELED");
        canceled["canceled_at"] = Timestamp;
        canceled["result"] = new SortedDictionary<string, object?> { ["provider_acknowledged"] = true };

        var snapshot = Store(
            [queued, running, cancelRequested, succeeded, failed, canceled],
            ["job-queued", "job-running"],
            version,
            includeMigrationMetadata: version == 2);

        var result = DesktopNodeJobStoreSnapshotValidator.Validate(snapshot);

        Assert.Equal(DesktopNodeJobStoreSnapshotValidationKind.Valid, result.Kind);
        Assert.Equal(expectedVersion, result.SchemaVersion);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.Detail);
        Assert.NotNull(result.Root);
        Assert.Equal(JsonValueKind.Object, result.Root.Value.ValueKind);
        Assert.Equal(6, result.Root.Value.GetProperty("jobs").GetArrayLength());
        Assert.Equal("job-queued", result.Root.Value.GetProperty("queue")[0].GetString());
        Assert.Equal("job-running", result.Root.Value.GetProperty("queue")[1].GetString());
    }

    [Theory]
    [InlineData(3)]
    [InlineData(99)]
    public void UnsupportedFutureVersionReturnsTypedNoProjectionResult(int version)
    {
        var snapshot = Store([], [], version);

        var result = DesktopNodeJobStoreSnapshotValidator.Validate(snapshot);

        Assert.Equal(DesktopNodeJobStoreSnapshotValidationKind.UnsupportedFuture, result.Kind);
        Assert.Equal(version, result.SchemaVersion);
        Assert.Equal("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", result.ErrorCode);
        Assert.Null(result.Root);
        Assert.Contains($"version {version}", result.Detail, StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> CorruptSemanticCases()
    {
        yield return Case(
            "missing jobs array",
            JsonSerializer.Serialize(new { version = 1, queue = Array.Empty<string>() }));
        yield return Case(
            "missing queue array",
            JsonSerializer.Serialize(new { version = 1, jobs = Array.Empty<object>() }));
        yield return Case(
            "invalid schema version type",
            """{"version":"1","jobs":[],"queue":[]}""");
        yield return Case(
            "invalid schema version range",
            """{"version":0,"jobs":[],"queue":[]}""");

        var duplicateA = Job("job-duplicate", "succeeded");
        var duplicateB = Job("job-duplicate", "failed");
        duplicateB["error"] = Error("PCV_TEST_FAILURE");
        yield return Case(
            "duplicate job id",
            Store([duplicateA, duplicateB], [], 1));

        var queued = Job("job-queued", "queued");
        yield return Case(
            "queued job missing queue reference",
            Store([queued], [], 1));
        yield return Case(
            "duplicate queue reference",
            Store([queued], ["job-queued", "job-queued"], 1));
        yield return Case(
            "unknown queue reference",
            Store([queued], ["job-unknown"], 1));

        var succeededInQueue = Job("job-succeeded", "succeeded");
        yield return Case(
            "non-queued job referenced by queue",
            Store([succeededInQueue], ["job-succeeded"], 1));

        var invalidStatus = Job("job-invalid-status", "mystery");
        yield return Case(
            "invalid status",
            Store([invalidStatus], [], 1));

        var missingParameters = Job("job-missing-params", "queued");
        missingParameters.Remove("params");
        yield return Case(
            "missing params object",
            Store([missingParameters], ["job-missing-params"], 1));

        var nonObjectParameters = Job("job-array-params", "queued");
        nonObjectParameters["params"] = Array.Empty<string>();
        yield return Case(
            "non-object params",
            Store([nonObjectParameters], ["job-array-params"], 1));

        var invalidAttempt = Job("job-invalid-attempt", "succeeded");
        invalidAttempt["attempt"] = 0;
        yield return Case(
            "attempt below one",
            Store([invalidAttempt], [], 1));

        var invalidAttemptType = Job("job-invalid-attempt-type", "succeeded");
        invalidAttemptType["attempt"] = "1";
        yield return Case(
            "invalid attempt type",
            Store([invalidAttemptType], [], 1));

        var missingCreatedAt = Job("job-missing-created-at", "succeeded");
        missingCreatedAt.Remove("created_at");
        yield return Case(
            "missing created timestamp",
            Store([missingCreatedAt], [], 1));

        var invalidUpdatedAt = Job("job-invalid-updated-at", "succeeded");
        invalidUpdatedAt["updated_at"] = "not-a-timestamp";
        yield return Case(
            "invalid updated timestamp",
            Store([invalidUpdatedAt], [], 1));

        var queuedWithResult = Job("job-queued-result", "queued");
        queuedWithResult["result"] = new { ok = true };
        yield return Case(
            "queued result combination",
            Store([queuedWithResult], ["job-queued-result"], 1));

        var runningWithWrongError = Job("job-running-error", "running");
        runningWithWrongError["error"] = Error("PCV_TEST_FAILURE");
        runningWithWrongError["canceled_at"] = Timestamp;
        yield return Case(
            "running non-cancel error combination",
            Store([runningWithWrongError], [], 1));

        var succeededWithError = Job("job-succeeded-error", "succeeded");
        succeededWithError["error"] = Error("PCV_TEST_FAILURE");
        yield return Case(
            "succeeded error combination",
            Store([succeededWithError], [], 1));

        var failedWithoutError = Job("job-failed-no-error", "failed");
        yield return Case(
            "failed without error",
            Store([failedWithoutError], [], 1));

        var canceledWithoutTimestamp = Job("job-canceled-no-time", "canceled");
        canceledWithoutTimestamp["error"] = Error("PCV_JOB_CANCELED");
        yield return Case(
            "canceled without timestamp",
            Store([canceledWithoutTimestamp], [], 1));

        var malformedError = Job("job-malformed-error", "failed");
        malformedError["error"] = new SortedDictionary<string, object?>
        {
            ["code"] = "PCV_TEST_FAILURE",
            ["message"] = "Failure.",
            ["detail"] = "Missing retryable."
        };
        yield return Case(
            "malformed error contract",
            Store([malformedError], [], 1));
    }

    private const string Timestamp = "2026-08-02T00:00:00.0000000+00:00";

    private static object[] Case(string scenario, string snapshot) => [scenario, snapshot];

    private static SortedDictionary<string, object?> Job(string jobId, string status)
    {
        return new SortedDictionary<string, object?>
        {
            ["attempt"] = 1,
            ["canceled_at"] = null,
            ["correlation_id"] = jobId,
            ["created_at"] = Timestamp,
            ["error"] = null,
            ["job_id"] = jobId,
            ["operation"] = "vm.start",
            ["params"] = new SortedDictionary<string, object?> { ["name"] = "alpha" },
            ["request_id"] = "req-" + jobId,
            ["result"] = null,
            ["retry_of"] = null,
            ["status"] = status,
            ["updated_at"] = Timestamp
        };
    }

    private static SortedDictionary<string, object?> Error(string code)
    {
        return new SortedDictionary<string, object?>
        {
            ["code"] = code,
            ["detail"] = "Structured test detail.",
            ["message"] = "Structured test message.",
            ["retryable"] = false
        };
    }

    private static string Store(
        IReadOnlyList<SortedDictionary<string, object?>> jobs,
        IReadOnlyList<string> queue,
        int? version,
        bool includeMigrationMetadata = false)
    {
        var root = new SortedDictionary<string, object?>
        {
            ["jobs"] = jobs,
            ["queue"] = queue,
            ["saved_at"] = Timestamp
        };
        if (version.HasValue)
        {
            root["version"] = version.Value;
        }

        if (includeMigrationMetadata)
        {
            root["migration"] = new SortedDictionary<string, object?>
            {
                ["plan_id"] = "job-store-v1-to-v2",
                ["source_schema_version"] = 1,
                ["target_schema_version"] = 2
            };
        }

        return JsonSerializer.Serialize(root);
    }
}
