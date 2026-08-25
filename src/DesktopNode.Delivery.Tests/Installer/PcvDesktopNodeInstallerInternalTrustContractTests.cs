using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "Installer")]
public sealed class PcvDesktopNodeInstallerInternalTrustContractTests
{
    private const string TrustScriptPath =
        "packaging/windows-desktop-node/installer/New-PcvInternalCodeSigningTrust.ps1";

    private readonly RepositoryContractContext repository = RepositoryContractContext.Find();

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-internal-trust.001",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",
        1,
        "exposes a dry-run plan for CurrentUser signing and LocalMachine trust")]
    public void Contract001()
    {
        var projection = InternalTrustContractVerifier.ProjectDryRun(
            repository.ReadUtf8Text(TrustScriptPath),
            "CurrentUser",
            "LocalMachine");

        Assert.Equal(0, projection.ExitCode);
        Assert.True(projection.Ok);
        Assert.True(projection.DryRun);
        Assert.Equal("CurrentUser", projection.SigningStoreScope);
        Assert.Equal("LocalMachine", projection.TrustStoreScope);
        Assert.True(projection.LocalMachineAdminRequired);
        Assert.False(projection.AdminGateEvaluated);
        Assert.Equal("InternalEnterprise", projection.SigningTrustModel);
        Assert.False(projection.SecretsRecorded);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-internal-trust.002",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",
        2,
        "does not require administrator privileges for LocalMachine dry-run planning")]
    public void Contract002()
    {
        var projection = InternalTrustContractVerifier.ProjectDryRun(
            repository.ReadUtf8Text(TrustScriptPath),
            "LocalMachine",
            "LocalMachine");

        Assert.Equal(0, projection.ExitCode);
        Assert.True(projection.Ok);
        Assert.True(projection.DryRun);
        Assert.True(projection.LocalMachineAdminRequired);
        Assert.False(projection.AdminGateEvaluated);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-internal-trust.003",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",
        3,
        "records only public certificate output paths and build arguments")]
    public void Contract003()
    {
        var boundary = InternalTrustContractVerifier.InspectCertificateBoundary(
            repository.ReadUtf8Text(TrustScriptPath));

        Assert.Equal(["Export-Certificate", "Import-Certificate"], boundary.PublicCertificateCommands);
        Assert.Equal(
            ["PureCVisor-Internal-CodeSigning-Root.cer", "PureCVisor-DesktopNode-Internal-CodeSigning.cer"],
            boundary.PublicCertificateFileNames);
        Assert.Equal("TrustedPublisher", boundary.TrustedPublisherStore);
        Assert.Equal("RequireSigned", boundary.SigningMode);
        Assert.Equal("InternalEnterprise", boundary.SigningTrustModel);
        Assert.Equal("CertificateThumbprint", boundary.CertificateReference);
        Assert.False(boundary.ExportsPrivateKey);
        Assert.False(boundary.RecordsPfxPassword);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-internal-trust.004",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",
        4,
        "documents dry-run and admin opt-in boundaries for internal RequireSigned release gates")]
    public void Contract004()
    {
        var boundary = InternalTrustContractVerifier.InspectDocumentation(
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/README.md"),
            repository.ReadUtf8Text("docs/adr/0003-internal-trusted-signing-policy.md"),
            repository.ReadUtf8Text("docs/DEVELOPMENT_VERIFICATION_POLICY.md"));

        Assert.Equal("Internal RequireSigned gate runbook", boundary.RunbookHeading);
        Assert.True(boundary.HasDryRunExample);
        Assert.True(boundary.StatesNoLocalMachineDryRunImport);
        Assert.True(boundary.StatesRequireSignedMode);
        Assert.True(boundary.StatesInternalEnterpriseModel);
        Assert.True(boundary.StatesAdministratorOptIn);
        Assert.True(boundary.StatesSecretBoundary);
        Assert.True(boundary.StatesNonPublicationBoundary);
    }
}
