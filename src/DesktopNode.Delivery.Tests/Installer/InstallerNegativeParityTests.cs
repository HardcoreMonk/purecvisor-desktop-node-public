using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "VerificationInfrastructure")]
public sealed class InstallerNegativeParityTests
{
    private const string LifecycleModulePath =
        "packaging/windows-desktop-node/installer/PcvDesktopNodeMsiLifecycle.psm1";

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

    [Fact]
    public void LifecycleVerifierRejectsMissingRestartManagerSuppression()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(LifecycleModulePath);
        var mutated = source.Replace(
            "MSIRESTARTMANAGERCONTROL=Disable",
            "MSIRESTARTMANAGERCONTROL=Enable",
            StringComparison.Ordinal);
        Assert.NotEqual(source, mutated);

        var error = Assert.Throws<InvalidDataException>(() =>
            MsiLifecycleContractVerifier.BuildPlan(mutated, "fixture.msi", "logs"));
        Assert.Equal(
            "PCV_INSTALLER_MSI_LIFECYCLE_SOURCE_INVALID|common-arguments",
            error.Message);
    }

    [Fact]
    public void LifecycleVerifierRejectsUnconditionalRepair3010Success()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(LifecycleModulePath);
        var mutated = source.Replace(
            "'reboot_required_success'",
            "'unconditional_success'",
            StringComparison.Ordinal);
        Assert.NotEqual(source, mutated);

        var error = Assert.Throws<InvalidDataException>(() =>
            MsiLifecycleContractVerifier.Classify(mutated, "Repair", 3010, true));
        Assert.Equal(
            "PCV_INSTALLER_MSI_LIFECYCLE_SOURCE_INVALID|classification:repair-3010",
            error.Message);
    }
}
