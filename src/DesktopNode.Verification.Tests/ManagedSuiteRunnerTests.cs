namespace DesktopNode.Verification.Tests;

public sealed class ManagedSuiteRunnerTests
{
    [Fact]
    public async Task CurrentEvidenceHandlerPassesCanonicalRepository()
    {
        var result = await new ManagedSuiteRunner().RunAsync(
            Suite("current-evidence-check"),
            VerificationCatalogFixture.RepositoryRoot,
            CancellationToken.None);

        Assert.Equal(SuiteStatus.Passed, result.Status);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.StandardOutput);
        Assert.Null(result.StandardError);
        Assert.Null(result.OutputSha256);
        Assert.False(result.TimedOut);
        Assert.False(result.Cancelled);
    }

    [Fact]
    public async Task PolicyBoundaryMatchesCanonicalActivationState()
    {
        var result = await new ManagedSuiteRunner().RunAsync(
            Suite("policy-boundaries"),
            VerificationCatalogFixture.RepositoryRoot,
            CancellationToken.None);

        var activationState = VerificationCatalogFixture.LoadCanonical().ActivationState;
        if (activationState == "plan-only-foundation")
        {
            Assert.Equal(SuiteStatus.Missing, result.Status);
            Assert.Equal(VerificationErrorCodes.ParityUnmapped, result.ErrorCode);
        }
        else
        {
            Assert.Equal(SuiteStatus.Passed, result.Status);
            Assert.Null(result.ErrorCode);
        }
    }

    [Fact]
    public async Task UnknownHandlerFailsClosed()
    {
        var result = await new ManagedSuiteRunner().RunAsync(
            Suite("unknown-handler"),
            VerificationCatalogFixture.RepositoryRoot,
            CancellationToken.None);

        Assert.Equal(SuiteStatus.Failed, result.Status);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task CancellationPropagates()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ManagedSuiteRunner().RunAsync(
                Suite("current-evidence-check"),
                VerificationCatalogFixture.RepositoryRoot,
                cancellation.Token));
    }

    private static SuiteDefinition Suite(string handler) =>
        new(
            "evidence-check",
            "csharp",
            "mapped",
            "managed",
            null,
            Array.Empty<string>(),
            handler,
            300);
}
