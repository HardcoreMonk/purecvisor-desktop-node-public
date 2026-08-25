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
    private const string ProductActionsWxsPath =
        "packaging/windows-desktop-node/installer/ProductActions.wxs";
    private const string WixProjectPath =
        "packaging/windows-desktop-node/installer/PureCVisorDesktopNode.wixproj";

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

    [Fact]
    public void SigningVerifierRejectsDigestDowngrade()
    {
        var repository = RepositoryContractContext.Find();
        var module = repository.ReadUtf8Text(BuildModulePath);
        var mutated = module.Replace(
            "@('sign', '/fd', 'SHA256', '/tr', $TimestampUrl, '/td', 'SHA256')",
            "@('sign', '/fd', 'SHA1', '/tr', $TimestampUrl, '/td', 'SHA1')",
            StringComparison.Ordinal);
        Assert.NotEqual(module, mutated);

        var error = Assert.Throws<InvalidDataException>(() => InstallerBuildSourcePolicy.Validate(
            repository.ReadUtf8Text(BuildScriptPath),
            mutated,
            repository.ReadUtf8Text(ProductWxsPath)));
        Assert.Equal("PCV_INSTALLER_BUILD_SOURCE_INVALID|signing-digest", error.Message);
    }

    [Fact]
    public void WixSourceVerifierRejectsMissingRequiredElement()
    {
        var repository = RepositoryContractContext.Find();
        var product = repository.ReadUtf8Text(ProductWxsPath);
        const string cliFile = "<File Id=\"DesktopNodeCli\" Source=\"$(var.PayloadRoot)\\pcvcli.exe\" KeyPath=\"yes\" />";
        var mutated = product.Replace(cliFile, string.Empty, StringComparison.Ordinal);
        Assert.NotEqual(product, mutated);

        var error = Assert.Throws<InvalidDataException>(() => NewWixVerifier(repository, mutated));
        Assert.Equal("PCV_INSTALLER_WIX_SOURCE_INVALID|product-file:DesktopNodeCli", error.Message);
    }

    [Fact]
    public void WixSourceVerifierRejectsDuplicateElement()
    {
        var repository = RepositoryContractContext.Find();
        var product = repository.ReadUtf8Text(ProductWxsPath);
        const string cliFile = "<File Id=\"DesktopNodeCli\" Source=\"$(var.PayloadRoot)\\pcvcli.exe\" KeyPath=\"yes\" />";
        var mutated = product.Replace(cliFile, $"{cliFile}{Environment.NewLine}        {cliFile}", StringComparison.Ordinal);
        Assert.NotEqual(product, mutated);

        var error = Assert.Throws<InvalidDataException>(() => NewWixVerifier(repository, mutated));
        Assert.Equal("PCV_INSTALLER_WIX_SOURCE_INVALID|duplicate:product-file:DesktopNodeCli", error.Message);
    }

    [Fact]
    public void WixSourceVerifierRejectsWrongNamespace()
    {
        var repository = RepositoryContractContext.Find();
        var product = repository.ReadUtf8Text(ProductWxsPath);
        var mutated = product.Replace(
            WixSourceContractVerifier.NamespaceUri,
            "https://example.invalid/wrong-wix-namespace",
            StringComparison.Ordinal);
        Assert.NotEqual(product, mutated);

        var error = Assert.Throws<InvalidDataException>(() => NewWixVerifier(repository, mutated));
        Assert.Equal("PCV_INSTALLER_WIX_SOURCE_INVALID|namespace:product", error.Message);
    }

    private static WixSourceContractVerifier NewWixVerifier(
        RepositoryContractContext repository,
        string productSource) =>
        new(
            productSource,
            repository.ReadUtf8Text(ProductActionsWxsPath),
            repository.ReadUtf8Text(WixProjectPath));

    [Fact]
    public void WrapperVerifierRejectsExitCodeCollapse()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(BuildScriptPath);
        var mutated = source.Replace(
            "$exitCode = [int]$payload.exit_code",
            "$exitCode = 1",
            StringComparison.Ordinal);
        Assert.NotEqual(source, mutated);

        var error = Assert.Throws<InvalidDataException>(() =>
            InstallerWrapperContractVerifier.Inspect(mutated));
        Assert.Equal("PCV_INSTALLER_WRAPPER_SOURCE_INVALID|exit-propagation", error.Message);
    }

    [Fact]
    public void WrapperVerifierRejectsElevationRequest()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(BuildScriptPath);
        var mutated = $"#requires -RunAsAdministrator{Environment.NewLine}{source}";

        var error = Assert.Throws<InvalidDataException>(() =>
            InstallerWrapperContractVerifier.Inspect(mutated));
        Assert.Equal("PCV_INSTALLER_WRAPPER_SOURCE_INVALID|elevation", error.Message);
    }
}
