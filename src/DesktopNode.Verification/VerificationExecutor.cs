using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopNode.Verification;

internal sealed class UnavailableManagedSuiteRunner : IManagedSuiteRunner
{
    public Task<SuiteExecutionRecord> RunAsync(SuiteDefinition suite, string repositoryRoot, CancellationToken cancellationToken) =>
        Task.FromResult(new SuiteExecutionRecord(
            suite.Id, SuiteStatus.Missing, suite.MigrationState, null, 0, false, false,
            null, null, null, VerificationErrorCodes.ParityUnmapped));
}

internal sealed class VerificationExecutor(IProcessRunner processRunner, IManagedSuiteRunner managedSuiteRunner)
{
    private static readonly HashSet<string> ApprovedErrorCodes = new(StringComparer.Ordinal)
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

    private static readonly HashSet<string> GenuineFailureErrorCodes = new(StringComparer.Ordinal)
    {
        VerificationErrorCodes.ConfigInvalid,
        VerificationErrorCodes.UnknownSuite,
        VerificationErrorCodes.ProcessFailed,
        VerificationErrorCodes.NonAdminPowerShellForbidden,
        VerificationErrorCodes.ArtifactRootInvalid
    };

    internal Task<VerificationExecutionReport> ExecuteAsync(
        VerificationPlan plan,
        VerificationCatalog catalog,
        string repositoryRoot,
        CancellationToken token)
    {
        if (plan is null || plan.Request is null)
        {
            return Task.FromException<VerificationExecutionReport>(Invalid("plan=missing"));
        }

        if (plan.Request.PlanOnly)
        {
            try
            {
                var suites = SnapshotSuites(plan.Suites, "plan");
                var rows = suites.Select(suite => new SuiteExecutionRecord(
                    suite.Id, SuiteStatus.Planned, suite.MigrationState, null, 0, false, false,
                    null, null, null, null)).ToArray();
                return Task.FromResult(new VerificationExecutionReport(0, Array.AsReadOnly(rows)));
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                return Task.FromException<VerificationExecutionReport>(AsStableException(exception));
            }
        }

        return ExecuteCoreAsync(plan, catalog, repositoryRoot, token);
    }

    private async Task<VerificationExecutionReport> ExecuteCoreAsync(
        VerificationPlan plan,
        VerificationCatalog catalog,
        string repositoryRoot,
        CancellationToken callerToken)
    {
        var planSuites = SnapshotSuites(plan.Suites, "plan");
        var execution = SnapshotExecution(catalog, planSuites, repositoryRoot);
        var stopwatch = Stopwatch.StartNew();

        using var overallTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(execution.OverallTimeoutSeconds));
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(callerToken, overallTimeout.Token);
        var semaphore = new SemaphoreSlim(execution.MaxParallelism, execution.MaxParallelism);
        var tasks = execution.Suites.Select(suite => ExecuteSuiteAsync(
            suite, execution, semaphore, callerToken, overallTimeout.Token, linkedCancellation.Token)).ToArray();
        var rows = await Task.WhenAll(tasks);

        stopwatch.Stop();
        return new VerificationExecutionReport(
            Math.Max(0, stopwatch.ElapsedMilliseconds),
            Array.AsReadOnly(rows.ToArray()));
    }

    private async Task<SuiteExecutionRecord> ExecuteSuiteAsync(
        SuiteDefinition suite,
        ExecutionSnapshot execution,
        SemaphoreSlim semaphore,
        CancellationToken callerToken,
        CancellationToken overallTimeoutToken,
        CancellationToken linkedToken)
    {
        var acquired = false;
        Stopwatch? active = null;
        CancellationTokenSource? suiteDeadline = null;
        try
        {
            await semaphore.WaitAsync(linkedToken);
            acquired = true;
            active = Stopwatch.StartNew();
            suiteDeadline = CancellationTokenSource.CreateLinkedTokenSource(linkedToken);
            suiteDeadline.CancelAfter(TimeSpan.FromSeconds(suite.TimeoutSeconds));

            if (string.Equals(suite.ExecutorKind, "process", StringComparison.Ordinal))
            {
                if (!IsValidProcessSuite(suite))
                {
                    return Failure(suite, VerificationErrorCodes.ConfigInvalid, Elapsed(active));
                }

                var invocation = new ProcessInvocation(
                    suite.Id, suite.FileName!, suite.Arguments, execution.RepositoryRoot,
                    TimeSpan.FromSeconds(suite.TimeoutSeconds), execution.AllowedExecutables);
                var result = await AwaitRunnerAsync(
                    processRunner.RunAsync(invocation, suiteDeadline.Token),
                    suiteDeadline.Token,
                    semaphore,
                    () => acquired = false);

                if (result is null || !IsValidProcessResult(result))
                {
                    return Failure(suite, VerificationErrorCodes.ProcessFailed, Elapsed(active));
                }

                return MapProcessResult(suite, result);
            }

            if (string.Equals(suite.ExecutorKind, "managed", StringComparison.Ordinal))
            {
                if (!IsValidManagedSuite(suite))
                {
                    return Failure(suite, VerificationErrorCodes.ConfigInvalid, Elapsed(active));
                }

                var result = await AwaitRunnerAsync(
                    managedSuiteRunner.RunAsync(suite, execution.RepositoryRoot, suiteDeadline.Token),
                    suiteDeadline.Token,
                    semaphore,
                    () => acquired = false);

                if (result is null || !IsValidManagedResult(result))
                {
                    return Failure(suite, VerificationErrorCodes.ProcessFailed, Elapsed(active));
                }

                return result with { SuiteId = suite.Id, MigrationState = suite.MigrationState };
            }

            return Failure(suite, VerificationErrorCodes.ConfigInvalid, Elapsed(active));
        }
        catch (OperationCanceledException exception)
        {
            var executorToken = suiteDeadline is not null
                ? suiteDeadline.Token
                : linkedToken;
            var relevantSourceRequested = callerToken.IsCancellationRequested ||
                overallTimeoutToken.IsCancellationRequested ||
                suiteDeadline?.IsCancellationRequested == true;
            if (exception.CancellationToken != executorToken || !relevantSourceRequested)
            {
                return Failure(suite, VerificationErrorCodes.ProcessFailed, Elapsed(active));
            }

            if (callerToken.IsCancellationRequested)
            {
                return Cancellation(suite, SuiteStatus.Cancelled, Elapsed(active));
            }

            if (overallTimeoutToken.IsCancellationRequested || suiteDeadline?.IsCancellationRequested == true)
            {
                return Cancellation(suite, SuiteStatus.TimedOut, Elapsed(active));
            }

            return Failure(suite, VerificationErrorCodes.ProcessFailed, Elapsed(active));
        }
        catch (VerificationException exception)
        {
            var code = GenuineFailureErrorCodes.Contains(exception.Code)
                ? exception.Code
                : VerificationErrorCodes.ProcessFailed;
            return Failure(suite, code, Elapsed(active));
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return Failure(suite, VerificationErrorCodes.ProcessFailed, Elapsed(active));
        }
        finally
        {
            active?.Stop();
            suiteDeadline?.Dispose();
            if (acquired)
            {
                semaphore.Release();
            }
        }
    }

    private static async Task<T> AwaitRunnerAsync<T>(
        Task<T> runnerTask,
        CancellationToken token,
        SemaphoreSlim semaphore,
        Action relinquishPermit)
    {
        try
        {
            return await runnerTask.WaitAsync(token);
        }
        catch (OperationCanceledException) when (!runnerTask.IsCompleted)
        {
            relinquishPermit();
            TransferPermit(runnerTask, semaphore);
            throw;
        }
    }

    private static void TransferPermit(Task task, SemaphoreSlim semaphore) =>
        _ = task.ContinueWith(
            completed =>
            {
                if (completed.IsFaulted)
                {
                    _ = completed.Exception;
                }

                semaphore.Release();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

    private static SuiteExecutionRecord MapProcessResult(SuiteDefinition suite, ProcessExecutionResult result)
    {
        if (result.TimedOut)
        {
            return Cancellation(suite, SuiteStatus.TimedOut, result.DurationMs,
                result.StandardOutput, result.StandardError, result.OutputSha256);
        }

        if (result.Cancelled)
        {
            return Cancellation(suite, SuiteStatus.Cancelled, result.DurationMs,
                result.StandardOutput, result.StandardError, result.OutputSha256);
        }

        if (result.ExitCode == 0)
        {
            return new SuiteExecutionRecord(
                suite.Id, SuiteStatus.Passed, suite.MigrationState, 0, result.DurationMs,
                false, false, result.StandardOutput, result.StandardError, result.OutputSha256, null);
        }

        return new SuiteExecutionRecord(
            suite.Id, SuiteStatus.Failed, suite.MigrationState, result.ExitCode, result.DurationMs,
            false, false, result.StandardOutput, result.StandardError, result.OutputSha256,
            VerificationErrorCodes.ProcessFailed);
    }

    private static bool IsValidProcessResult(ProcessExecutionResult result)
    {
        var exitCoherent = result.TimedOut || result.Cancelled
            ? result.ExitCode is null
            : result.ExitCode is not null;
        return !(result.TimedOut && result.Cancelled) &&
            result.DurationMs >= 0 &&
            result.StandardOutput is not null &&
            result.StandardError is not null &&
            IsLowerHexSha256(result.OutputSha256) &&
            exitCoherent;
    }

    private static bool IsLowerHexSha256(string? value) =>
        value is { Length: 64 } &&
        value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsValidManagedResult(SuiteExecutionRecord result)
    {
        if (result.DurationMs < 0 || result.StandardOutput is not null ||
            result.StandardError is not null || result.OutputSha256 is not null ||
            result.ExitCode is not null || (result.TimedOut && result.Cancelled))
        {
            return false;
        }

        return result.Status switch
        {
            SuiteStatus.Passed => !result.TimedOut && !result.Cancelled && result.ErrorCode is null,
            SuiteStatus.Missing => !result.TimedOut && !result.Cancelled &&
                string.Equals(result.ErrorCode, VerificationErrorCodes.ParityUnmapped, StringComparison.Ordinal),
            SuiteStatus.Failed => !result.TimedOut && !result.Cancelled &&
                result.ErrorCode is not null && GenuineFailureErrorCodes.Contains(result.ErrorCode),
            SuiteStatus.TimedOut => result.TimedOut && !result.Cancelled &&
                string.Equals(result.ErrorCode, VerificationErrorCodes.Timeout, StringComparison.Ordinal),
            SuiteStatus.Cancelled => !result.TimedOut && result.Cancelled &&
                string.Equals(result.ErrorCode, VerificationErrorCodes.Cancelled, StringComparison.Ordinal),
            _ => false
        };
    }

    private static SuiteExecutionRecord Cancellation(
        SuiteDefinition suite,
        SuiteStatus status,
        long durationMs = 0,
        string? standardOutput = null,
        string? standardError = null,
        string? outputSha256 = null) =>
        new(
            suite.Id, status, suite.MigrationState, null, Math.Max(0, durationMs),
            status == SuiteStatus.TimedOut, status == SuiteStatus.Cancelled,
            standardOutput, standardError, outputSha256,
            status == SuiteStatus.TimedOut ? VerificationErrorCodes.Timeout : VerificationErrorCodes.Cancelled);

    private static SuiteExecutionRecord Failure(SuiteDefinition suite, string code, long durationMs = 0) =>
        new(
            suite.Id, SuiteStatus.Failed, suite.MigrationState, null, Math.Max(0, durationMs),
            false, false, null, null, null, code);

    private static bool IsValidProcessSuite(SuiteDefinition suite) =>
        !string.IsNullOrWhiteSpace(suite.Id) &&
        !string.IsNullOrWhiteSpace(suite.MigrationState) &&
        !string.IsNullOrWhiteSpace(suite.FileName) &&
        suite.Arguments is not null && suite.Arguments.All(argument => argument is not null) &&
        suite.ManagedHandler is null && suite.TimeoutSeconds is > 0 and <= 3600;

    private static bool IsValidManagedSuite(SuiteDefinition suite) =>
        !string.IsNullOrWhiteSpace(suite.Id) &&
        !string.IsNullOrWhiteSpace(suite.MigrationState) &&
        suite.FileName is null && suite.Arguments is { Count: 0 } &&
        !string.IsNullOrWhiteSpace(suite.ManagedHandler) && suite.TimeoutSeconds is > 0 and <= 3600;

    private static SuiteDefinition[] SnapshotSuites(IReadOnlyList<SuiteDefinition> source, string owner)
    {
        if (source is null)
        {
            throw Invalid($"{owner}-suites=missing");
        }

        var suites = source.ToArray();
        if (suites.Any(suite => suite is null))
        {
            throw Invalid($"{owner}-suite=missing");
        }

        return suites.Select(SnapshotSuite).ToArray();
    }

    private static SuiteDefinition SnapshotSuite(SuiteDefinition suite) => suite with
    {
        Arguments = suite.Arguments is null ? null! : Array.AsReadOnly(suite.Arguments.ToArray())
    };

    private static ExecutionSnapshot SnapshotExecution(
        VerificationCatalog catalog,
        IReadOnlyList<SuiteDefinition> planSuites,
        string repositoryRoot)
    {
        if (catalog is null || catalog.AllowedExecutables is null || catalog.Suites is null)
        {
            throw Invalid("catalog=missing");
        }

        if (catalog.MaxParallelism <= 0 || catalog.OverallTimeoutSeconds is < 1 or > 3600)
        {
            throw Invalid("catalog-execution-limits=invalid");
        }

        var allowedExecutables = catalog.AllowedExecutables.ToArray();
        if (allowedExecutables.Any(string.IsNullOrWhiteSpace))
        {
            throw Invalid("catalog-allowlist=invalid");
        }

        var catalogSuites = SnapshotSuites(catalog.Suites, "catalog");
        if (catalogSuites.Any(suite => string.IsNullOrWhiteSpace(suite.Id)) ||
            catalogSuites.Select(suite => suite.Id).Distinct(StringComparer.Ordinal).Count() != catalogSuites.Length)
        {
            throw Invalid("catalog-suites=invalid");
        }

        if (planSuites.Any(suite => string.IsNullOrWhiteSpace(suite.Id)) ||
            planSuites.Select(suite => suite.Id).Distinct(StringComparer.Ordinal).Count() != planSuites.Count)
        {
            throw Invalid("plan-suites=invalid");
        }

        var canonicalById = catalogSuites.ToDictionary(suite => suite.Id, StringComparer.Ordinal);
        var selected = new SuiteDefinition[planSuites.Count];
        for (var index = 0; index < planSuites.Count; index++)
        {
            var planned = planSuites[index];
            if (!canonicalById.TryGetValue(planned.Id, out var canonical) || !DefinitionsEqual(planned, canonical))
            {
                throw Invalid("plan-suite-binding=invalid");
            }

            selected[index] = canonical;
        }

        string canonicalRoot;
        try
        {
            if (string.IsNullOrWhiteSpace(repositoryRoot) || !Path.IsPathFullyQualified(repositoryRoot))
            {
                throw Invalid("repository-root=invalid");
            }

            canonicalRoot = Path.GetFullPath(repositoryRoot);
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception) &&
            exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw Invalid("repository-root=invalid");
        }

        return new ExecutionSnapshot(
            Math.Min(catalog.MaxParallelism, 4), catalog.OverallTimeoutSeconds, canonicalRoot,
            Array.AsReadOnly(allowedExecutables), Array.AsReadOnly(selected));
    }

    private static bool DefinitionsEqual(SuiteDefinition left, SuiteDefinition right) =>
        string.Equals(left.Id, right.Id, StringComparison.Ordinal) &&
        string.Equals(left.Owner, right.Owner, StringComparison.Ordinal) &&
        string.Equals(left.MigrationState, right.MigrationState, StringComparison.Ordinal) &&
        string.Equals(left.ExecutorKind, right.ExecutorKind, StringComparison.Ordinal) &&
        string.Equals(left.FileName, right.FileName, StringComparison.Ordinal) &&
        ArgumentsEqual(left.Arguments, right.Arguments) &&
        string.Equals(left.ManagedHandler, right.ManagedHandler, StringComparison.Ordinal) &&
        left.TimeoutSeconds == right.TimeoutSeconds;

    private static bool ArgumentsEqual(IReadOnlyList<string>? left, IReadOnlyList<string>? right) =>
        left is null || right is null
            ? left is null && right is null
            : left.SequenceEqual(right, StringComparer.Ordinal);

    private static long Elapsed(Stopwatch? stopwatch) =>
        stopwatch is null ? 0 : Math.Max(0, stopwatch.ElapsedMilliseconds);

    private static bool IsFatal(Exception exception) =>
        exception is OutOfMemoryException or StackOverflowException or AccessViolationException or
        AppDomainUnloadedException or SEHException;

    private static VerificationException AsStableException(Exception exception) =>
        exception is VerificationException verificationException && ApprovedErrorCodes.Contains(verificationException.Code)
            ? verificationException
            : Invalid("plan=invalid");

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);

    private sealed record ExecutionSnapshot(
        int MaxParallelism,
        int OverallTimeoutSeconds,
        string RepositoryRoot,
        IReadOnlyList<string> AllowedExecutables,
        IReadOnlyList<SuiteDefinition> Suites);
}
