namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "VerificationInfrastructure")]
public sealed class InstalledNegativeParityTests
{
    [Fact]
    public void RejectsHostMutationTrue()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            InstalledContractVerifier.ValidateEvidence(
                Baseline() with { HostMutationPerformed = true }));

        Assert.Equal("PCV_DELIVERY_INSTALLED_INVALID|host-mutation", error.Message);
    }

    [Fact]
    public void RejectsObservedTokenValue()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            InstalledContractVerifier.ValidateEvidence(
                Baseline() with { TokenValueObserved = true }));

        Assert.Equal("PCV_DELIVERY_INSTALLED_INVALID|token-observed", error.Message);
    }

    [Fact]
    public void RejectsMissingCleanup()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            InstalledContractVerifier.ValidateEvidence(
                Baseline() with { CleanupSteps = [] }));

        Assert.Equal("PCV_DELIVERY_INSTALLED_INVALID|cleanup", error.Message);
    }

    [Fact]
    public void RejectsWrongRouteResult()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            InstalledContractVerifier.ValidateEvidence(
                Baseline() with { RouteResult = "unexpected-500" }));

        Assert.Equal("PCV_DELIVERY_INSTALLED_INVALID|route-result", error.Message);
    }

    [Fact]
    public void RejectsFabricatedInstalledPass()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            InstalledContractVerifier.ValidateEvidence(
                Baseline() with { OverallStatus = "pass" }));

        Assert.Equal("PCV_DELIVERY_INSTALLED_INVALID|fabricated-pass", error.Message);
    }

    private static InstalledSmokeEvidenceContract Baseline() =>
        new(
            HostMutationPerformed: false,
            TokenValueObserved: false,
            CleanupSteps: ["restore-original-state"],
            RouteResult: "expected-boundary",
            OverallStatus: "not-run",
            InstalledExecutionObserved: false,
            PublicTrustedSigning: "not-claimed",
            ExternalStablePublication: "not-claimed");
}
