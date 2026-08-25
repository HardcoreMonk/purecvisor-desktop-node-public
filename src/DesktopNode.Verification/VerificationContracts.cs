using System.Text.Json.Serialization;

namespace DesktopNode.Verification;

internal enum VerificationLane { Fast, Full, Release }
internal enum ChangeTier { S, M, L }
internal enum ExecutionScope { Lane, Shard, Partial }
internal enum SuiteStatus { Planned, Passed, Failed, Missing, TimedOut, Cancelled }

internal sealed record VerificationRequest(
    VerificationLane RequestedLane,
    ChangeTier RequestedChangeTier,
    IReadOnlyList<string> ChangedPaths,
    string ArtifactRoot,
    IReadOnlyList<string> SuiteIds,
    string? ShardId,
    bool PlanOnly);

internal sealed record VerificationPlan(
    VerificationRequest Request,
    VerificationLane EffectiveLane,
    ChangeTier EffectiveChangeTier,
    IReadOnlyList<string> TierReasons,
    string? PromotionReason,
    ExecutionScope ExecutionScope,
    string? ShardId,
    bool ReleasePreflight,
    IReadOnlyList<SuiteDefinition> Suites);

internal sealed record ProcessInvocation(
    string SuiteId,
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    TimeSpan Timeout,
    IReadOnlyList<string> AllowedExecutables,
    int OutputLimitCharacters = 8192);

internal sealed record ProcessExecutionResult(
    int? ExitCode,
    long DurationMs,
    bool TimedOut,
    bool Cancelled,
    string StandardOutput,
    string StandardError,
    string OutputSha256);

internal sealed record SuiteExecutionRecord(
    string SuiteId,
    SuiteStatus Status,
    string MigrationState,
    int? ExitCode,
    long DurationMs,
    bool TimedOut,
    bool Cancelled,
    string? StandardOutput,
    string? StandardError,
    string? OutputSha256,
    string? ErrorCode);

internal sealed record VerificationExecutionReport(
    long DurationMs,
    IReadOnlyList<SuiteExecutionRecord> Results);

internal sealed record VerificationSummary(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("contract")] string Contract,
    [property: JsonPropertyName("requested_lane")] string RequestedLane,
    [property: JsonPropertyName("effective_lane")] string EffectiveLane,
    [property: JsonPropertyName("requested_change_tier")] string RequestedChangeTier,
    [property: JsonPropertyName("change_tier")] string ChangeTier,
    [property: JsonPropertyName("tier_reasons")] IReadOnlyList<string> TierReasons,
    [property: JsonPropertyName("promotion_reason")] string? PromotionReason,
    [property: JsonPropertyName("execution_scope")] string ExecutionScope,
    [property: JsonPropertyName("shard_id")] string? ShardId,
    [property: JsonPropertyName("plan_only")] bool PlanOnly,
    [property: JsonPropertyName("catalog_activation_state")] string CatalogActivationState,
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("error_code")] string? ErrorCode,
    [property: JsonPropertyName("started_at")] DateTimeOffset StartedAt,
    [property: JsonPropertyName("completed_at")] DateTimeOffset CompletedAt,
    [property: JsonPropertyName("duration_ms")] long DurationMs,
    [property: JsonPropertyName("results")] IReadOnlyList<VerificationSuiteSummary> Results);

internal sealed record VerificationSuiteSummary(
    [property: JsonPropertyName("suite_id")] string SuiteId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("migration_state")] string MigrationState,
    [property: JsonPropertyName("exit_code")] int? ExitCode,
    [property: JsonPropertyName("duration_ms")] long DurationMs,
    [property: JsonPropertyName("timed_out")] bool TimedOut,
    [property: JsonPropertyName("cancelled")] bool Cancelled,
    [property: JsonPropertyName("standard_output")] string? StandardOutput,
    [property: JsonPropertyName("standard_error")] string? StandardError,
    [property: JsonPropertyName("output_sha256")] string? OutputSha256,
    [property: JsonPropertyName("error_code")] string? ErrorCode);

internal interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken);
}

internal interface IManagedSuiteRunner
{
    Task<SuiteExecutionRecord> RunAsync(
        SuiteDefinition suite,
        string repositoryRoot,
        CancellationToken cancellationToken);
}

internal interface IVerificationClock
{
    DateTimeOffset UtcNow { get; }
}

internal interface IVerificationFileSystem
{
    string ReadAllText(string path);
    void CreateDirectory(string path);
    Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken);
    bool FileExists(string path);
    void MoveFile(string source, string destination, bool overwrite);
    void DeleteFile(string path);
}

internal static class VerificationErrorCodes
{
    internal const string ConfigInvalid = "PCV_VERIFY_CONFIG_INVALID";
    internal const string UnknownSuite = "PCV_VERIFY_UNKNOWN_SUITE";
    internal const string ProcessFailed = "PCV_VERIFY_PROCESS_FAILED";
    internal const string Timeout = "PCV_VERIFY_TIMEOUT";
    internal const string Cancelled = "PCV_VERIFY_CANCELLED";
    internal const string ParityUnmapped = "PCV_VERIFY_PARITY_UNMAPPED";
    internal const string NonAdminPowerShellForbidden = "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN";
    internal const string ArtifactRootInvalid = "PCV_VERIFY_ARTIFACT_ROOT_INVALID";
}

internal sealed class VerificationException(string code, string detail)
    : Exception($"{code}|{detail}")
{
    internal string Code { get; } = code;
    internal string Detail { get; } = detail;
}
