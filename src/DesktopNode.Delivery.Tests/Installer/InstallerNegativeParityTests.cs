using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "VerificationInfrastructure")]
public sealed class InstallerNegativeParityTests
{
    [Fact]
    public void InternalTrustVerifierRejectsChangedTrustModel()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(
            "packaging/windows-desktop-node/installer/New-PcvInternalCodeSigningTrust.ps1");
        var mutated = source.Replace(
            "signing_trust_model = 'InternalEnterprise'",
            "signing_trust_model = 'PublicTrusted'",
            StringComparison.Ordinal);
        Assert.NotEqual(source, mutated);

        var error = Assert.Throws<InvalidDataException>(() =>
            InternalTrustContractVerifier.ProjectDryRun(mutated, "CurrentUser", "LocalMachine"));
        Assert.Equal(
            "PCV_INSTALLER_INTERNAL_TRUST_SOURCE_INVALID|assignment:signing_trust_model",
            error.Message);
    }
}
