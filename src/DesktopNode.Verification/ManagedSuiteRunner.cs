using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopNode.Verification;

internal sealed class ManagedSuiteRunner(IProcessRunner? configuredProcessRunner = null) : IManagedSuiteRunner
{
    private readonly IProcessRunner processRunner = configuredProcessRunner ?? new SystemProcessRunner();

    public async Task<SuiteExecutionRecord> RunAsync(
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
                return Result(suite, SuiteStatus.Passed, null, stopwatch);
            }

            if (suite.ManagedHandler == "policy-boundaries")
            {
                var root = Path.GetFullPath(repositoryRoot);
                var catalog = new VerificationCatalogLoader(new PhysicalVerificationFileSystem()).Load(
                    Path.Combine(root, "config", "development-verification-suites.json"),
                    Path.Combine(root, "config", "development-verification-suites.schema.json"));
                if (catalog.ActivationState == "plan-only-foundation")
                {
                    return Result(
                        suite,
                        SuiteStatus.Missing,
                        VerificationErrorCodes.ParityUnmapped,
                        stopwatch);
                }

                var workflow = File.ReadAllText(Path.Combine(
                    root, ".github", "workflows", "development-gates.yml"));
                var manifest = File.ReadAllText(Path.Combine(
                    root, "config", "development-verification-migration-manifest.json"));
                var policy = RequiredCiPolicy.Validate(workflow, catalog);
                var ledger = RequiredCiMigrationLedger.Validate(manifest, policy.Mode);
                if (policy.Mode == RequiredCiMode.Active)
                {
                    var boundary = new CutoverGitBoundary(processRunner);
                    var head = await boundary.ResolveHeadAsync(root, cancellationToken);
                    _ = await boundary.ValidateAsync(
                        root,
                        head,
                        ledger.ShadowSha ?? throw new VerificationException(
                            VerificationErrorCodes.ConfigInvalid,
                            "required-ci-ledger:cutover-locator=invalid"),
                        cancellationToken);
                }

                return Result(suite, SuiteStatus.Passed, null, stopwatch);
            }

            return Result(
                suite,
                SuiteStatus.Failed,
                VerificationErrorCodes.ConfigInvalid,
                stopwatch);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception error) when (error is CurrentEvidenceException or VerificationException)
        {
            return Result(
                suite,
                SuiteStatus.Failed,
                VerificationErrorCodes.ConfigInvalid,
                stopwatch);
        }
        catch (Exception error) when (!IsFatal(error))
        {
            return Result(
                suite,
                SuiteStatus.Failed,
                VerificationErrorCodes.ConfigInvalid,
                stopwatch);
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
