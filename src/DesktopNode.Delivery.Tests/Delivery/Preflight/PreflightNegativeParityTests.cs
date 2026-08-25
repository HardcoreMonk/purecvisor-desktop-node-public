namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "VerificationInfrastructure")]
public sealed class PreflightNegativeParityTests
{
    [Fact]
    public void RejectsMissingPublicationBlocker()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            PreflightContractVerifier.ValidateDescriptor(Baseline() with { Blockers = [] }));

        Assert.Equal("PCV_DELIVERY_PREFLIGHT_INVALID|blockers", error.Message);
    }

    [Fact]
    public void RejectsPublishedCatalogState()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            PreflightContractVerifier.ValidateDescriptor(
                Baseline() with { CatalogPublication = "published" }));

        Assert.Equal("PCV_DELIVERY_PREFLIGHT_INVALID|publication-boundary", error.Message);
    }

    [Fact]
    public void RejectsUnsafeCredentialField()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            PreflightContractVerifier.ValidateDescriptor(
                Baseline() with
                {
                    CredentialFields = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["password"] = "clear-text",
                    },
                }));

        Assert.Equal("PCV_DELIVERY_PREFLIGHT_INVALID|credential-field", error.Message);
    }

    [Fact]
    public void RejectsWrongPublicPackageChannel()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            PreflightContractVerifier.ValidateDescriptor(
                Baseline() with { PackageChannel = "nightly" }));

        Assert.Equal("PCV_DELIVERY_PREFLIGHT_INVALID|package-channel", error.Message);
    }

    [Fact]
    public void RejectsExecutableHostMutation()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            PreflightContractVerifier.ValidateSourceSafety("Start-Service PureCVisorDesktopNode"));

        Assert.Equal("PCV_DELIVERY_PREFLIGHT_INVALID|source-mutation", error.Message);
    }

    private static PreflightDescriptorContract Baseline() =>
        new(
            PlanOnly: true,
            HostMutationPerformed: false,
            PublicTrustedSigning: "not-claimed",
            ExternalStablePublication: "not-claimed",
            CatalogPublication: "not-published",
            PackageChannel: "stable",
            Blockers: ["public-signing-required"],
            CredentialFields: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["credential_manager_mutation"] = "not-run",
            });
}
