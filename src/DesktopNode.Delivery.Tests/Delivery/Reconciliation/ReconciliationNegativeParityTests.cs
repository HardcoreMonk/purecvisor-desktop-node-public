namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "VerificationInfrastructure")]
public sealed class ReconciliationNegativeParityTests
{
    [Fact]
    public void RejectsIllegalTerminalTransition()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ReconciliationContractVerifier.ValidateTransition("completed", "running"));

        Assert.Equal("PCV_DELIVERY_RECONCILIATION_INVALID|illegal-transition", error.Message);
    }

    [Fact]
    public void RejectsStaleCheckpoint()
    {
        var now = DateTimeOffset.Parse("2026-08-26T12:00:00Z");
        var error = Assert.Throws<InvalidDataException>(() =>
            ReconciliationContractVerifier.ValidateCheckpoint(
                now.AddMinutes(-11),
                now,
                TimeSpan.FromMinutes(10)));

        Assert.Equal("PCV_DELIVERY_RECONCILIATION_INVALID|stale-checkpoint", error.Message);
    }

    [Fact]
    public void RejectsMissingScheduledTaskCleanup()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ReconciliationContractVerifier.ValidateCleanup(["write-evidence"]));

        Assert.Equal("PCV_DELIVERY_RECONCILIATION_INVALID|missing-cleanup", error.Message);
    }

    [Fact]
    public void RejectsReorderedLifecycle()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ReconciliationContractVerifier.ValidateLifecycle(
                ["unregister-task", "write-evidence", "persist-completion"]));

        Assert.Equal("PCV_DELIVERY_RECONCILIATION_INVALID|lifecycle-order", error.Message);
    }

    [Fact]
    public void RejectsEventLogMutationInPreflight()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ReconciliationContractVerifier.ValidateEventLogPreflight(
                hostMutationPerformed: true));

        Assert.Equal("PCV_DELIVERY_RECONCILIATION_INVALID|event-log-mutation", error.Message);
    }

    [Fact]
    public void RejectsInvalidWingetManifestField()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ReconciliationContractVerifier.ValidateWinget(
                BaselineWinget() with { InstallerType = "exe" }));

        Assert.Equal("PCV_DELIVERY_RECONCILIATION_INVALID|invalid-winget-field", error.Message);
    }

    private static WingetManifestContract BaselineWinget() =>
        new(
            PackageIdentifier: "PureCVisor.DesktopNode",
            InstallerUrl: "https://downloads.example.invalid/PureCVisorDesktopNode.msi",
            InstallerSha256: new string('D', 64),
            InstallerType: "msi",
            ManifestType: "singleton",
            ManifestVersion: "1.12.0");
}
