using System.Text.Json;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "Installer")]
public sealed class PcvDesktopNodeInstallerSigningContractTests : IDisposable
{
    private readonly string temporaryRoot = Path.Combine(
        Path.GetTempPath(),
        "pcv-delivery-tests",
        Guid.NewGuid().ToString("N"));
    private readonly RepositoryContractContext repository;
    private readonly InstallerBuildContractHarness harness;

    public PcvDesktopNodeInstallerSigningContractTests()
    {
        repository = RepositoryContractContext.Find();
        harness = new InstallerBuildContractHarness(repository, temporaryRoot);
        Directory.CreateDirectory(temporaryRoot);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-signing.001", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1", 1, "defines required provenance fields")]
    public void Contract001()
    {
        using var schema = repository.LoadJson(
            "packaging/windows-desktop-node/installer/installer-provenance.schema.json");
        var root = schema.RootElement;
        Assert.Equal("https://json-schema.org/draft/2020-12/schema", root.GetProperty("$schema").GetString());
        var required = root.GetProperty("required").EnumerateArray().Select(value => value.GetString()).ToHashSet(StringComparer.Ordinal);
        foreach (var name in new[]
        {
            "schema_version", "product", "git_commit", "build_utc", "wix", "msi",
            "payload", "service_host", "cli", "signing_mode", "signing_trust_model", "host",
        })
        {
            Assert.Contains(name, required);
        }
        var properties = root.GetProperty("properties");
        var productRequired = properties.GetProperty("product").GetProperty("required")
            .EnumerateArray().Select(value => value.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("msi_product_version", productRequired);
        Assert.Contains("release_channel", productRequired);
        var wixRequired = properties.GetProperty("wix").GetProperty("required")
            .EnumerateArray().Select(value => value.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("source_files", wixRequired);
        Assert.DoesNotContain("source_project", wixRequired);
        var trustModels = properties.GetProperty("signing_trust_model").GetProperty("enum")
            .EnumerateArray().Select(value => value.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("LocalTest", trustModels);
        Assert.Contains("InternalEnterprise", trustModels);
        Assert.Contains("PublicTrusted", trustModels);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-signing.002", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1", 2, "accepts release signing input without writing certificate secrets into dry-run output")]
    public void Contract002()
    {
        var thumbprint = Thumbprint();
        var result = harness.Execute(SignedInput("0.14.0", "c002", "LocalTest") with
        {
            CertificateThumbprint = thumbprint,
            DryRun = true,
        });
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        Assert.Equal(0, result.ExitCode);
        Assert.DoesNotContain(thumbprint, result.Json, StringComparison.Ordinal);
        Assert.DoesNotContain("pfx", result.Json, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.Ok);
        Assert.Equal("RequireSigned", plan.SigningMode);
        Assert.Equal("LocalTest", plan.SigningTrustModel);
        Assert.True(plan.HasSignTool);
        Assert.True(plan.HasCertificate);
        Assert.True(plan.HasTimestamp);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-signing.003", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1", 3, "records InternalEnterprise provenance without writing certificate secrets")]
    public void Contract003()
    {
        var thumbprint = Thumbprint();
        var result = harness.Execute(SignedInput("0.24.0-rc.1", "c003", "InternalEnterprise") with
        {
            CertificateThumbprint = thumbprint,
            SignToolStdout = "fake signtool signed",
        });
        var provenance = Assert.IsType<InstallerBuildProvenanceProjection>(result.Provenance);
        var signingOutput = Assert.IsType<InstallerToolOutputProjection>(result.SignToolOutput);
        Assert.True(result.Ok);
        Assert.DoesNotContain(thumbprint, result.Json, StringComparison.Ordinal);
        Assert.DoesNotMatch("(?i)pfx|password|private key", result.Json);
        Assert.Equal("rc", provenance.ReleaseChannel);
        Assert.Equal("RequireSigned", provenance.SigningMode);
        Assert.Equal("InternalEnterprise", provenance.SigningTrustModel);
        Assert.True(provenance.MsiSigned);
        var arguments = string.Join(' ', signingOutput.Arguments);
        Assert.Contains("[redacted]", arguments, StringComparison.Ordinal);
        Assert.DoesNotContain(thumbprint, arguments, StringComparison.Ordinal);
        Assert.Contains("/fd SHA256", arguments, StringComparison.Ordinal);
        Assert.Contains("/td SHA256", arguments, StringComparison.Ordinal);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-signing.004", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1", 4, "requires an explicit trust model for signed release builds")]
    public void Contract004()
    {
        var result = harness.Execute(SignedInput("0.14.0", "c004", "Unspecified") with
        {
            CertificateThumbprint = Thumbprint(),
            DryRun = true,
        });
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.False(string.IsNullOrWhiteSpace(result.Json));
        Assert.Equal("PCV_INSTALLER_SIGNING_TRUST_MODEL_REQUIRED", result.Error?.Code);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-signing.005", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1", 5, "returns structured JSON when release SignTool input is missing")]
    public void Contract005()
    {
        var input = SignedInput("0.14.0", "c005", "LocalTest") with
        {
            SignToolPath = Path.Combine(temporaryRoot, "missing-signtool.exe"),
            CertificateThumbprint = Thumbprint(),
            DryRun = true,
        };
        var result = harness.Execute(input);
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.False(string.IsNullOrWhiteSpace(result.Json));
        Assert.Equal("PCV_INSTALLER_SIGNTOOL_NOT_FOUND", result.Error?.Code);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-signing.006", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1", 6, "returns parseable JSON with captured SignTool output when signing fails")]
    public void Contract006()
    {
        var thumbprint = Thumbprint();
        var result = harness.Execute(SignedInput("0.14.0", "c006", "LocalTest") with
        {
            CertificateThumbprint = thumbprint,
            SignToolExitCode = 7,
            SignToolStdout = "fake signtool stdout progress",
            SignToolStderr = "fake signtool stderr detail",
        });
        var signingOutput = Assert.IsType<InstallerToolOutputProjection>(result.SignToolOutput);
        Assert.Equal(7, result.ExitCode);
        Assert.StartsWith("{\"ok\":false", result.Json, StringComparison.Ordinal);
        using var json = JsonDocument.Parse(result.Json);
        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("PCV_INSTALLER_SIGNING_FAILED", result.Error?.Code);
        Assert.Contains("fake signtool stdout progress", signingOutput.Stdout, StringComparison.Ordinal);
        Assert.Contains("fake signtool stderr detail", signingOutput.Stderr, StringComparison.Ordinal);
        var arguments = string.Join(' ', signingOutput.Arguments);
        Assert.Contains("[redacted]", arguments, StringComparison.Ordinal);
        Assert.DoesNotContain(thumbprint, arguments, StringComparison.Ordinal);
        Assert.DoesNotContain(thumbprint, result.Json, StringComparison.Ordinal);
    }

    public void Dispose()
    {
        var baseRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pcv-delivery-tests"));
        var target = Path.GetFullPath(temporaryRoot);
        if (!target.StartsWith(baseRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("PCV_DELIVERY_TEMP_PATH_INVALID|signing");
        }
        if (Directory.Exists(target))
        {
            Directory.Delete(target, recursive: true);
        }
    }

    private InstallerBuildInput SignedInput(string version, string outputName, string trustModel) =>
        new(version, Path.Combine(temporaryRoot, outputName))
        {
            DesktopNodeHostPath = Fixture("DesktopNode.Host.exe", "fake-host"),
            SigningMode = "RequireSigned",
            SigningTrustModel = trustModel,
            SignToolPath = Fixture("signtool.exe", "fake-signtool"),
            TimestampUrl = "https://timestamp.example.invalid",
        };

    private string Fixture(string name, string content)
    {
        var path = Path.Combine(temporaryRoot, name);
        Directory.CreateDirectory(temporaryRoot);
        File.WriteAllText(path, content);
        return path;
    }

    private static string Thumbprint() => string.Concat(
        "0011223344556677",
        "8899AABBCCDDEEFF",
        "00112233");
}
