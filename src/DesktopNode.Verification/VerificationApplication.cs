using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.Verification;

internal sealed class SystemVerificationClock : IVerificationClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

internal sealed class VerificationApplication(
    IProcessRunner processRunner,
    IManagedSuiteRunner managedSuiteRunner,
    IVerificationFileSystem fileSystem,
    IVerificationClock clock,
    Func<string> currentDirectory,
    Func<string?> runnerTemp,
    Func<string?> userProfile)
{
    private const string SummaryContract = "pcv-development-verification-summary-v2";

    private static readonly JsonSerializerOptions CompactJsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    private static readonly HashSet<string> StableBoundaryDetails = new(StringComparer.Ordinal)
    {
        "application-failure",
        "failure-summary-invalid",
        "summary-write-failed",
        "standard-output-write-failed",
        "operation=cancelled",
        "cli:unknown-command",
        "cli:invalid-lane",
        "cli:invalid-change-tier",
        "cli:invalid-changed-path",
        "cli:invalid-shard",
        "cli:duplicate-suite",
        "cli:unknown-option",
        "cli:missing-value",
        "cli:empty-value",
        "cli:duplicate-option",
        "cli:missing-required-option",
        "cli:suite-and-shard-mutually-exclusive",
        "repository-root-not-found",
        "artifact-root-invalid:unresolved",
        "artifact-root-invalid:runner-temp-equal",
        "artifact-root-invalid:broad-root",
        "artifact-root-invalid:repository-root",
        "artifact-root-invalid:user-profile",
        "artifact-root-invalid:outside-boundary",
        "artifact-root-invalid:runner-temp",
        "artifact-root-invalid:path"
    };

    private static readonly HashSet<string> StableErrorCodes = new(StringComparer.Ordinal)
    {
        VerificationErrorCodes.ConfigInvalid,
        VerificationErrorCodes.UnknownSuite,
        VerificationErrorCodes.ProcessFailed,
        VerificationErrorCodes.Timeout,
        VerificationErrorCodes.Cancelled,
        VerificationErrorCodes.ParityUnmapped,
        VerificationErrorCodes.NonAdminPowerShellForbidden,
        VerificationErrorCodes.ArtifactRootInvalid
    };

    internal async Task<int> RunAsync(
        IReadOnlyList<string> args,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        DateTimeOffset? startedAt = null;
        VerificationRequest? request = null;
        VerificationPlan? plan = null;
        VerificationCatalog? catalog = null;
        string? artifactRoot = null;

        try
        {
            startedAt = clock.UtcNow;
            cancellationToken.ThrowIfCancellationRequested();
            request = VerificationOptions.Parse(args);

            cancellationToken.ThrowIfCancellationRequested();
            var repositoryRoot = RepositoryLocator.Find(currentDirectory());

            cancellationToken.ThrowIfCancellationRequested();
            artifactRoot = ArtifactRootPolicy.ResolveAndValidate(
                repositoryRoot,
                request.ArtifactRoot,
                runnerTemp(),
                userProfile());

            cancellationToken.ThrowIfCancellationRequested();
            catalog = new VerificationCatalogLoader(fileSystem).Load(
                Path.Combine(repositoryRoot, "config", "development-verification-suites.json"),
                Path.Combine(repositoryRoot, "config", "development-verification-suites.schema.json"));

            cancellationToken.ThrowIfCancellationRequested();
            plan = VerificationPlanner.Create(request, catalog);

            cancellationToken.ThrowIfCancellationRequested();
            if (!request.PlanOnly &&
                !string.Equals(catalog.ActivationState, "active", StringComparison.Ordinal))
            {
                throw new VerificationException(
                    VerificationErrorCodes.ConfigInvalid,
                    $"activation-state={catalog.ActivationState};actual-execution=false");
            }

            var report = await new VerificationExecutor(processRunner, managedSuiteRunner)
                .ExecuteAsync(plan, catalog, repositoryRoot, cancellationToken);
            var completedAt = clock.UtcNow;
            var summary = VerificationSummaryFactory.Create(
                plan,
                catalog,
                report,
                startedAt.Value,
                completedAt);
            var outputFailureSummary = summary.Ok
                ? VerificationSummaryFactory.CreateFailure(
                    request,
                    plan,
                    catalog.ActivationState,
                    VerificationErrorCodes.ConfigInvalid,
                    startedAt.Value,
                    completedAt)
                : null;

            return await WriteSummaryAsync(
                artifactRoot,
                summary,
                outputFailureSummary,
                summary.Ok ? 0 : 1,
                standardOutput,
                standardError,
                cancellationToken);
        }
        catch (VerificationException exception)
        {
            return await CompleteFailureAsync(
                request,
                plan,
                catalog,
                artifactRoot,
                exception,
                startedAt,
                standardOutput,
                standardError);
        }
        catch (OperationCanceledException exception) when (
            exception.CancellationToken == cancellationToken &&
            cancellationToken.IsCancellationRequested)
        {
            return await CompleteFailureAsync(
                request,
                plan,
                catalog,
                artifactRoot,
                new VerificationException(VerificationErrorCodes.Cancelled, "operation=cancelled"),
                startedAt,
                standardOutput,
                standardError);
        }
        catch (OperationCanceledException)
        {
            return await CompleteFailureAsync(
                request,
                plan,
                catalog,
                artifactRoot,
                new VerificationException(VerificationErrorCodes.ConfigInvalid, "application-failure"),
                startedAt,
                standardOutput,
                standardError);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return await CompleteFailureAsync(
                request,
                plan,
                catalog,
                artifactRoot,
                new VerificationException(VerificationErrorCodes.ConfigInvalid, "application-failure"),
                startedAt,
                standardOutput,
                standardError);
        }
    }

    private async Task<int> CompleteFailureAsync(
        VerificationRequest? request,
        VerificationPlan? plan,
        VerificationCatalog? catalog,
        string? artifactRoot,
        VerificationException exception,
        DateTimeOffset? startedAt,
        TextWriter standardOutput,
        TextWriter standardError)
    {
        var stableCode = StableErrorCode(exception.Code);
        if (request is null || artifactRoot is null || startedAt is null)
        {
            WriteBoundaryError(standardError, stableCode, StableBoundaryDetail(exception.Detail));
            return 2;
        }

        VerificationSummary summary;
        try
        {
            summary = VerificationSummaryFactory.CreateFailure(
                request,
                plan,
                catalog?.ActivationState ?? "unavailable",
                stableCode,
                startedAt.Value,
                FailureCompletedAt(startedAt.Value));
        }
        catch (Exception failureException) when (!IsFatal(failureException))
        {
            WriteBoundaryError(standardError, VerificationErrorCodes.ConfigInvalid, "failure-summary-invalid");
            return 2;
        }

        try
        {
            return await WriteSummaryAsync(
                artifactRoot,
                summary,
                null,
                2,
                standardOutput,
                standardError,
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            WriteBoundaryError(standardError, VerificationErrorCodes.ConfigInvalid, "summary-write-failed");
            return 2;
        }
    }

    private async Task<int> WriteSummaryAsync(
        string artifactRoot,
        VerificationSummary summary,
        VerificationSummary? outputFailureSummary,
        int exitCode,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        string summaryPath;
        try
        {
            summaryPath = await new AtomicVerificationSummaryWriter(fileSystem)
                .WriteAsync(artifactRoot, summary, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            WriteBoundaryError(standardError, VerificationErrorCodes.ConfigInvalid, "summary-write-failed");
            return 2;
        }

        try
        {
            await standardOutput.WriteLineAsync(summaryPath);
            return exitCode;
        }
        catch (Exception exception)
        {
            if (IsFatal(exception))
            {
                throw;
            }

            if (outputFailureSummary is not null)
            {
                try
                {
                    await new AtomicVerificationSummaryWriter(fileSystem).WriteAsync(
                        artifactRoot,
                        outputFailureSummary,
                        CancellationToken.None);
                }
                catch (Exception recoveryException)
                {
                    if (IsFatal(recoveryException))
                    {
                        throw;
                    }

                    BestEffortDelete(summaryPath);
                }
            }

            WriteBoundaryError(
                standardError,
                VerificationErrorCodes.ConfigInvalid,
                "standard-output-write-failed");
            return 2;
        }
    }

    private static void WriteBoundaryError(TextWriter standardError, string code, string stableDetail)
    {
        var error = new BoundaryError(2, SummaryContract, false, StableErrorCode(code), stableDetail);
        standardError.WriteLine(JsonSerializer.Serialize(error, CompactJsonOptions));
    }

    private DateTimeOffset FailureCompletedAt(DateTimeOffset startedAt)
    {
        try
        {
            return clock.UtcNow;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return startedAt;
        }
    }

    private void BestEffortDelete(string summaryPath)
    {
        try
        {
            if (fileSystem.FileExists(summaryPath))
            {
                fileSystem.DeleteFile(summaryPath);
            }
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
        }
    }

    private static string StableErrorCode(string? code) =>
        code is not null && StableErrorCodes.Contains(code)
            ? code
            : VerificationErrorCodes.ConfigInvalid;

    private static string StableBoundaryDetail(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return "application-failure";
        }

        if (StableBoundaryDetails.Contains(detail))
        {
            return detail;
        }

        var separator = detail.IndexOf('=');
        if (separator > 0)
        {
            var category = detail[..separator];
            if (category.StartsWith("cli:", StringComparison.Ordinal) &&
                StableBoundaryDetails.Contains(category))
            {
                return category;
            }
        }

        return "application-failure";
    }

    private static bool IsFatal(Exception exception) =>
        exception is OutOfMemoryException or StackOverflowException or AccessViolationException or
        AppDomainUnloadedException or SEHException;

    private sealed record BoundaryError(
        [property: JsonPropertyName("schema_version")] int SchemaVersion,
        [property: JsonPropertyName("contract")] string Contract,
        [property: JsonPropertyName("ok")] bool Ok,
        [property: JsonPropertyName("error_code")] string ErrorCode,
        [property: JsonPropertyName("error_detail")] string ErrorDetail);
}
