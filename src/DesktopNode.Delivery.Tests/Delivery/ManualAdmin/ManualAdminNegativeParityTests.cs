namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "VerificationInfrastructure")]
public sealed class ManualAdminNegativeParityTests
{
    private static readonly DateTimeOffset Now =
        DateTimeOffset.Parse("2026-08-26T12:00:00Z");

    [Fact]
    public void RejectsStaleDescriptor()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ManualAdminContractVerifier.ValidateReadiness(
                Baseline() with { ExpiresAt = Now },
                Now));

        Assert.Equal("PCV_DELIVERY_MANUAL_ADMIN_INVALID|stale-descriptor", error.Message);
    }

    [Fact]
    public void RejectsMismatchedPackagePair()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ManualAdminContractVerifier.ValidateReadiness(
                Baseline() with { InstalledVersion = "0.42.72-admin-smoke" },
                Now));

        Assert.Equal("PCV_DELIVERY_MANUAL_ADMIN_INVALID|package-pair", error.Message);
    }

    [Fact]
    public void RejectsMissingBlocker()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ManualAdminContractVerifier.ValidateReadiness(
                Baseline() with { Blockers = [] },
                Now));

        Assert.Equal("PCV_DELIVERY_MANUAL_ADMIN_INVALID|blockers", error.Message);
    }

    [Fact]
    public void RejectsSecretValuedField()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ManualAdminContractVerifier.ValidateReadiness(
                Baseline() with
                {
                    CredentialFields = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["upload_secret"] = "synthetic-value",
                    },
                },
                Now));

        Assert.Equal("PCV_DELIVERY_MANUAL_ADMIN_INVALID|credential-field", error.Message);
    }

    [Fact]
    public void RejectsFalseExternalPublicationClaim()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ManualAdminContractVerifier.ValidateReadiness(
                Baseline() with { ExternalStablePublication = "published" },
                Now));

        Assert.Equal("PCV_DELIVERY_MANUAL_ADMIN_INVALID|claim-boundary", error.Message);
    }

    private static ManualAdminReadinessContract Baseline() =>
        new(
            GeneratedAt: Now.AddHours(-1),
            ExpiresAt: Now.AddHours(1),
            BaselineVersion: "0.42.73-admin-smoke",
            TargetVersion: "0.42.74-admin-smoke",
            InstalledVersion: "0.42.73-admin-smoke",
            Blockers: ["public-signing-required"],
            HostMutationPerformed: false,
            PublicTrustedSigning: "not-claimed",
            ExternalStablePublication: "not-claimed",
            CredentialFields: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["credential_manager_mutation"] = "not-run",
            });
}
