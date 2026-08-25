using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopNode.Verification;

internal sealed class ManagedSuiteRunner : IManagedSuiteRunner
{
    public Task<SuiteExecutionRecord> RunAsync(
        SuiteDefinition suite,
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (suite.ManagedHandler == "current-evidence-check")
            {
                _ = CurrentEvidenceVerifier.Verify(repositoryRoot, cancellationToken);
                return Task.FromResult(Result(suite, SuiteStatus.Passed, null, stopwatch));
            }

            if (suite.ManagedHandler == "policy-boundaries")
            {
                return Task.FromResult(Result(
                    suite,
                    SuiteStatus.Missing,
                    VerificationErrorCodes.ParityUnmapped,
                    stopwatch));
            }

            return Task.FromResult(Result(
                suite,
                SuiteStatus.Failed,
                VerificationErrorCodes.ConfigInvalid,
                stopwatch));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (CurrentEvidenceException)
        {
            return Task.FromResult(Result(
                suite,
                SuiteStatus.Failed,
                VerificationErrorCodes.ConfigInvalid,
                stopwatch));
        }
        catch (Exception error) when (!IsFatal(error))
        {
            return Task.FromResult(Result(
                suite,
                SuiteStatus.Failed,
                VerificationErrorCodes.ConfigInvalid,
                stopwatch));
        }
    }

    private static SuiteExecutionRecord Result(
        SuiteDefinition suite,
        SuiteStatus status,
        string? errorCode,
        Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new SuiteExecutionRecord(
            suite.Id,
            status,
            suite.MigrationState,
            null,
            Math.Max(0, stopwatch.ElapsedMilliseconds),
            false,
            false,
            null,
            null,
            null,
            errorCode);
    }

    private static bool IsFatal(Exception error) =>
        error is OutOfMemoryException or StackOverflowException or AccessViolationException or
            AppDomainUnloadedException or SEHException;
}
