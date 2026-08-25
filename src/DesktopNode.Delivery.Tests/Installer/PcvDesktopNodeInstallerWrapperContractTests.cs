using System.Text.Json;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "Installer")]
public sealed class PcvDesktopNodeInstallerWrapperContractTests : IDisposable
{
    private readonly string temporaryRoot = Path.Combine(
        Path.GetTempPath(),
        "pcv-delivery-tests",
        Guid.NewGuid().ToString("N"));
    private readonly InstallerBuildContractHarness harness;
    private readonly InstallerWrapperSourceBoundary boundary;

    public PcvDesktopNodeInstallerWrapperContractTests()
    {
        var repository = RepositoryContractContext.Find();
        harness = new InstallerBuildContractHarness(repository, temporaryRoot);
        boundary = InstallerWrapperContractVerifier.Inspect(
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/build.ps1"));
        Directory.CreateDirectory(temporaryRoot);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wrapper.001", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1", 1, "returns JSON and exit zero for an unsigned dry run")]
    public void Contract001()
    {
        var host = Fixture("Desktop Node.Host.exe", "fake-host");
        var cli = Fixture("pcv cli.exe", "fake-cli");
        var output = Path.Combine(temporaryRoot, "dry run out");
        var wrapper = InstallerWrapperContractVerifier.Execute(
            harness,
            boundary,
            new InstallerWrapperRequest("0.42.65-dev", output)
            {
                DesktopNodeHostPath = host,
                DesktopNodeCliPath = cli,
                SigningMode = "AllowUnsignedDev",
                DryRun = true,
            });

        Assert.False(boundary.RequestsElevation);
        Assert.False(boundary.UsesShellConcatenation);
        Assert.Equal(0, wrapper.BuildResult.ExitCode);
        using var json = JsonDocument.Parse(wrapper.BuildResult.Json);
        Assert.True(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.True(json.RootElement.GetProperty("dry_run").GetBoolean());
        Assert.Equal(1, wrapper.Arguments.Count(value => value == host));
        Assert.Equal(1, wrapper.Arguments.Count(value => value == cli));
        Assert.Equal(1, wrapper.Arguments.Count(value => value == output));
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wrapper.002", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1", 2, "returns structured JSON for a missing service host")]
    public void Contract002()
    {
        var wrapper = InstallerWrapperContractVerifier.Execute(
            harness,
            boundary,
            new InstallerWrapperRequest("0.42.65-dev", Path.Combine(temporaryRoot, "missing-host-out"))
            {
                DesktopNodeHostPath = Path.Combine(temporaryRoot, "missing-DesktopNode.Host.exe"),
                SigningMode = "AllowUnsignedDev",
                DryRun = true,
            });
        Assert.Equal(1, wrapper.BuildResult.ExitCode);
        Assert.False(wrapper.BuildResult.Ok);
        Assert.Equal("PCV_INSTALLER_SERVICE_HOST_NOT_FOUND", wrapper.BuildResult.Error?.Code);
        using var json = JsonDocument.Parse(wrapper.BuildResult.Json);
        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wrapper.003", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1", 3, "preserves signing failure exit code while redacting the certificate thumbprint")]
    public void Contract003()
    {
        var thumbprint = Thumbprint();
        var wrapper = InstallerWrapperContractVerifier.Execute(
            harness,
            boundary,
            new InstallerWrapperRequest("0.42.65", Path.Combine(temporaryRoot, "signing-failure-out"))
            {
                DesktopNodeHostPath = Fixture("DesktopNode.Host.exe", "fake-host"),
                DesktopNodeCliPath = Fixture("pcvcli.exe", "fake-cli"),
                SigningMode = "RequireSigned",
                SigningTrustModel = "LocalTest",
                SignToolPath = Fixture("signtool.cmd", "fake-signtool"),
                CertificateThumbprint = thumbprint,
                TimestampUrl = "https://timestamp.example.invalid",
                WixPath = Fixture("wix.cmd", "fake-wix"),
                SignToolExitCode = 7,
                SignToolStdout = "wrapper signtool stdout",
                SignToolStderr = "wrapper signtool stderr",
            });
        var signing = Assert.IsType<InstallerToolOutputProjection>(wrapper.BuildResult.SignToolOutput);
        Assert.Equal(7, wrapper.BuildResult.ExitCode);
        Assert.DoesNotContain(thumbprint, wrapper.BuildResult.Json, StringComparison.Ordinal);
        Assert.Equal("PCV_INSTALLER_SIGNING_FAILED", wrapper.BuildResult.Error?.Code);
        Assert.Contains("[redacted]", string.Join(' ', signing.Arguments), StringComparison.Ordinal);
        Assert.DoesNotContain(thumbprint, string.Join(' ', signing.Arguments), StringComparison.Ordinal);
    }

    public void Dispose()
    {
        var baseRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pcv-delivery-tests"));
        var target = Path.GetFullPath(temporaryRoot);
        if (!target.StartsWith(baseRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("PCV_DELIVERY_TEMP_PATH_INVALID|wrapper");
        }
        if (Directory.Exists(target))
        {
            Directory.Delete(target, recursive: true);
        }
    }

    private string Fixture(string name, string content)
    {
        Directory.CreateDirectory(temporaryRoot);
        var path = Path.Combine(temporaryRoot, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static string Thumbprint() => string.Concat(
        "0011223344556677",
        "8899AABBCCDDEEFF",
        "00112233");
}
