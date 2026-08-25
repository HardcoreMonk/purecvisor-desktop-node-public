using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "VerificationInfrastructure")]
public sealed class InstallerNegativeParityTests
{
    private const string LifecycleModulePath =
        "packaging/windows-desktop-node/installer/PcvDesktopNodeMsiLifecycle.psm1";
    private const string BuildScriptPath = "packaging/windows-desktop-node/installer/build.ps1";
    private const string BuildModulePath =
        "packaging/windows-desktop-node/installer/PcvDesktopNodeInstaller.Build.psm1";
    private const string ProductWxsPath = "packaging/windows-desktop-node/installer/Product.wxs";

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

    [Fact]
    public void PlanVerifierRejectsMissingRequiredPlanProperty()
    {
        var repository = RepositoryContractContext.Find();
        var module = repository.ReadUtf8Text(BuildModulePath);
        var mutated = module.Replace(
            "product_name = 'PureCVisor Desktop Node'",
            "product_title = 'PureCVisor Desktop Node'",
            StringComparison.Ordinal);
        Assert.NotEqual(module, mutated);

        var error = Assert.Throws<InvalidDataException>(() => InstallerBuildSourcePolicy.Validate(
            repository.ReadUtf8Text(BuildScriptPath),
            mutated,
            repository.ReadUtf8Text(ProductWxsPath)));
        Assert.Equal("PCV_INSTALLER_BUILD_SOURCE_INVALID|plan-product-name", error.Message);
    }

    [Fact]
    public void PlanVerifierRejectsWixArgumentOrderingDrift()
    {
        var repository = RepositoryContractContext.Find();
        var module = repository.ReadUtf8Text(BuildModulePath);
        var mutated = module.Replace(
            "$wixArgs = @(",
            "$wixArgs = $wixSourcePaths + @(",
            StringComparison.Ordinal);
        Assert.NotEqual(module, mutated);

        var error = Assert.Throws<InvalidDataException>(() => InstallerBuildSourcePolicy.Validate(
            repository.ReadUtf8Text(BuildScriptPath),
            mutated,
            repository.ReadUtf8Text(ProductWxsPath)));
        Assert.Equal("PCV_INSTALLER_BUILD_SOURCE_INVALID|wix-argument-order", error.Message);
    }

    [Fact]
    public void PlanVerifierRejectsEscapingPayloadRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-delivery-tests", "containment-root");
        var escaping = Path.Combine(root, "..", "escape", "payload");

        var error = Assert.Throws<InvalidDataException>(() =>
            InstallerBuildContractHarness.EnsurePayloadRootContained(root, escaping));
        Assert.Equal("PCV_INSTALLER_PLAN_INVALID|payload-root", error.Message);
    }
}
