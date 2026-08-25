using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "Installer")]
public sealed class PcvDesktopNodeInstallerPlanContractTests : IDisposable
{
    private readonly string temporaryRoot = Path.Combine(
        Path.GetTempPath(),
        "pcv-delivery-tests",
        Guid.NewGuid().ToString("N"));
    private readonly InstallerBuildContractHarness harness;

    public PcvDesktopNodeInstallerPlanContractTests()
    {
        Directory.CreateDirectory(temporaryRoot);
        harness = new InstallerBuildContractHarness(RepositoryContractContext.Find(), temporaryRoot);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.001", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 1, "exposes explicit version service host output signing and dry-run parameters")]
    public void Contract001()
    {
        var boundary = harness.SourceBoundary;
        foreach (var parameter in new[]
        {
            "Version", "DesktopNodeHostPath", "DesktopNodeCliPath", "OutputRoot",
            "MsiProductVersion", "SigningTrustModel", "DryRun",
        })
        {
            Assert.Contains(parameter, boundary.Parameters);
        }
        Assert.True(boundary.SelfContained);
        Assert.True(boundary.SingleFile);
        Assert.True(boundary.HasMonotonicFileVersion);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.002", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 2, "keeps the active installer boundary CLI and Web only without TUI references")]
    public void Contract002()
    {
        var boundary = harness.SourceBoundary;
        Assert.True(boundary.HasCliPayload);
        Assert.True(boundary.HasWebPayload);
        Assert.False(boundary.HasTuiReference);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.003", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 3, "returns structured JSON when release signing input is missing")]
    public void Contract003()
    {
        var result = harness.Execute(new InstallerBuildInput("0.14.0", Output("c003"))
        {
            DesktopNodeHostPath = Fixture("DesktopNode.Host.exe", "fake-host"),
            SigningMode = "RequireSigned",
            DryRun = true,
        });
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.Equal("PCV_INSTALLER_SIGNING_REQUIRED", result.Error?.Code);
        Assert.StartsWith("{\"ok\":false", result.Json, StringComparison.Ordinal);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.004", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 4, "returns structured JSON when service host input is missing")]
    public void Contract004()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            Output("c004"),
            Path.Combine(temporaryRoot, "missing-DesktopNode.Host.exe")));
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.Equal("PCV_INSTALLER_SERVICE_HOST_NOT_FOUND", result.Error?.Code);
        Assert.False(string.IsNullOrWhiteSpace(result.Json));
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.005", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 5, "returns structured JSON when CLI input is missing")]
    public void Contract005()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            Output("c005"),
            Fixture("DesktopNode.Host.exe", "fake-host")) with
        {
            DesktopNodeCliPath = Path.Combine(temporaryRoot, "missing-DesktopNode.Cli.exe"),
        });
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.Equal("PCV_INSTALLER_CLI_NOT_FOUND", result.Error?.Code);
        Assert.False(string.IsNullOrWhiteSpace(result.Json));
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.006", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 6, "rejects unsigned release-candidate builds")]
    public void Contract006()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.22.0-rc.1",
            Output("c006"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.Equal("PCV_INSTALLER_RELEASE_SIGNING_REQUIRED", result.Error?.Code);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.007", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 7, "rejects unsigned stable builds")]
    public void Contract007()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.22.0",
            Output("c007"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        Assert.Equal(1, result.ExitCode);
        Assert.False(result.Ok);
        Assert.Equal("PCV_INSTALLER_RELEASE_SIGNING_REQUIRED", result.Error?.Code);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.008", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 8, "emits a dry-run plan without requiring WiX for unsigned developer builds")]
    public void Contract008()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            Output("c008"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        Assert.Equal(0, result.ExitCode);
        Assert.True(result.Ok);
        Assert.Equal("PureCVisor Desktop Node", plan.ProductName);
        Assert.Equal("0.14.0-dev", plan.Version);
        Assert.Equal("dev", plan.ReleaseChannel);
        Assert.Equal("0.14.0", plan.MsiProductVersion);
        Assert.Equal("AllowUnsignedDev", plan.SigningMode);
        Assert.Equal("Unspecified", plan.SigningTrustModel);
        Assert.Matches("^[0-9a-f]{64}$", plan.ServiceHostSha256!);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.009", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 9, "records an explicit CLI payload in dry-run output")]
    public void Contract009()
    {
        var cli = Fixture("DesktopNode.Cli.exe", "fake-cli");
        var result = harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            Output("c009"),
            Fixture("DesktopNode.Host.exe", "fake-host")) with { DesktopNodeCliPath = cli });
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        Assert.True(result.Ok);
        Assert.Equal("explicit-path", plan.CliSource);
        Assert.Equal(Path.GetFullPath(cli), plan.CliPath);
        Assert.Matches("^[0-9a-f]{64}$", plan.CliSha256!);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.010", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 10, "describes internal publication boundaries in dry-run output")]
    public void Contract010()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.39.0-dev",
            Output("c010"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        Assert.Equal("PureCVisorDesktopNode-0.39.0-dev-windows-x64.publication.json", Path.GetFileName(plan.PublicationPath));
        Assert.Equal("1", plan.Publication.SchemaVersion);
        Assert.Equal("internal-artifact-descriptor-only", plan.Publication.Mode);
        Assert.Equal("not-claimed", plan.Publication.PublicTrustedSigning);
        Assert.Equal("not-claimed", plan.Publication.ExternalStablePublication);
        Assert.Equal("not-built", plan.Publication.BurnBootstrapper);
        Assert.Equal("not-built", plan.Publication.Msix);
        Assert.Equal("not-generated", plan.Publication.WingetManifest);
        Assert.Equal("not-published", plan.Publication.CatalogPublication);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.011", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 11, "accepts an explicit MSI product version override in the build plan")]
    public void Contract011()
    {
        var result = harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            Output("c011"),
            Fixture("DesktopNode.Host.exe", "fake-host")) with { MsiProductVersion = "0.14.7" });
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        Assert.True(result.Ok);
        Assert.Equal("0.14.0-dev", plan.Version);
        Assert.Equal("0.14.7", plan.MsiProductVersion);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.012", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 12, "keeps absolute output roots absolute in the build plan")]
    public void Contract012()
    {
        var output = Output("c012");
        var result = harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            output,
            Fixture("DesktopNode.Host.exe", "fake-host")));
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        Assert.Equal(Path.GetFullPath(output), plan.OutputRoot);
        Assert.Equal(Path.Combine(Path.GetFullPath(output), "payload"), plan.PayloadRoot);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.013", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 13, "records the actual WiX source files used for build input")]
    public void Contract013()
    {
        var plan = Assert.IsType<InstallerBuildPlanProjection>(harness.Execute(UnsignedDryRun(
            "0.14.0-dev",
            Output("c013"),
            Fixture("DesktopNode.Host.exe", "fake-host"))).Plan);
        Assert.Contains(plan.WixSourceFiles, path => path.EndsWith("Product.wxs", StringComparison.Ordinal));
        Assert.Contains(plan.WixSourceFiles, path => path.EndsWith("ProductActions.wxs", StringComparison.Ordinal));
        Assert.DoesNotContain(plan.WixSourceFiles, path => path.EndsWith(".wixproj", StringComparison.OrdinalIgnoreCase));
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.014", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 14, "invokes WiX CLI with WiX source files instead of the project file")]
    public void Contract014()
    {
        var result = harness.Execute(UnsignedBuild(
            "0.14.0-dev",
            Output("c014"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        Assert.True(result.Ok);
        Assert.Equal("build", result.WixArguments[0]);
        Assert.DoesNotContain(result.WixArguments, value => value.EndsWith(".wixproj", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.WixArguments, value => value.EndsWith("Product.wxs", StringComparison.Ordinal));
        Assert.Contains(result.WixArguments, value => value.EndsWith("ProductActions.wxs", StringComparison.Ordinal));
        var archIndex = result.WixArguments.ToList().IndexOf("-arch");
        Assert.True(archIndex > -1);
        Assert.Equal("x64", result.WixArguments[archIndex + 1]);
        Assert.Contains("MsiProductVersion=0.14.0", result.WixArguments);
        Assert.Equal(harness.CurrentGitCommit, result.Provenance?.GitCommit);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.015", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 15, "emits Phase 22 windows-x64 artifact names for MSI provenance and hash sidecar")]
    public void Contract015()
    {
        var result = harness.Execute(UnsignedBuild(
            "0.22.0-dev.1",
            Output("c015"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        var provenance = Assert.IsType<InstallerBuildProvenanceProjection>(result.Provenance);
        var expectedBase = "PureCVisorDesktopNode-0.22.0-dev.1-windows-x64";
        Assert.Equal($"{expectedBase}.msi", Path.GetFileName(plan.MsiPath));
        Assert.Equal($"{expectedBase}.provenance.json", Path.GetFileName(plan.ProvenancePath));
        Assert.Equal($"{expectedBase}.msi.sha256", Path.GetFileName(plan.MsiSha256Path));
        Assert.True(File.Exists(plan.MsiPath));
        Assert.True(File.Exists(plan.ProvenancePath));
        Assert.True(File.Exists(plan.MsiSha256Path));
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(plan.MsiPath))).ToLowerInvariant();
        var sidecar = File.ReadAllText(plan.MsiSha256Path);
        Assert.Contains(hash, sidecar, StringComparison.Ordinal);
        Assert.Contains($"{expectedBase}.msi", sidecar, StringComparison.Ordinal);
        Assert.Equal($"{expectedBase}.msi", Path.GetFileName(provenance.MsiPath));
        Assert.Equal("dev", provenance.ReleaseChannel);
        Assert.Equal("Unspecified", provenance.SigningTrustModel);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.016", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 16, "writes an internal publication descriptor sidecar for built MSI artifacts")]
    public void Contract016()
    {
        var result = harness.Execute(UnsignedBuild(
            "0.39.0-dev",
            Output("c016"),
            Fixture("DesktopNode.Host.exe", "fake-host")));
        var plan = Assert.IsType<InstallerBuildPlanProjection>(result.Plan);
        var descriptor = Assert.IsType<InstallerPublicationDescriptorProjection>(result.PublicationDescriptor);
        var provenance = Assert.IsType<InstallerBuildProvenanceProjection>(result.Provenance);
        var expectedBase = "PureCVisorDesktopNode-0.39.0-dev-windows-x64";
        Assert.Equal($"{expectedBase}.publication.json", Path.GetFileName(plan.PublicationPath));
        Assert.True(File.Exists(plan.PublicationPath));
        using var json = JsonDocument.Parse(File.ReadAllText(plan.PublicationPath));
        Assert.Equal("1", json.RootElement.GetProperty("schema_version").GetString());
        Assert.Equal("0.39.0-dev", descriptor.ProductVersion);
        Assert.Equal(expectedBase, descriptor.ArtifactBaseName);
        Assert.Equal(provenance.MsiSha256, descriptor.MsiSha256);
        Assert.Equal(plan.ProvenancePath, descriptor.ProvenancePath);
        Assert.Equal("internal-artifact-descriptor-only", descriptor.Publication.Mode);
        Assert.Equal("not-claimed", descriptor.Publication.PublicTrustedSigning);
        Assert.Equal("not-claimed", descriptor.Publication.ExternalStablePublication);
        Assert.Equal("not-built", descriptor.Publication.BurnBootstrapper);
        Assert.Equal("not-built", descriptor.Publication.Msix);
        Assert.Equal("not-generated", descriptor.Publication.WingetManifest);
        Assert.Equal("not-published", descriptor.Publication.CatalogPublication);
        Assert.Equal(provenance.Publication.PublicTrustedSigning, descriptor.Publication.PublicTrustedSigning);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.017", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 17, "returns parseable JSON with captured WiX output when WiX build fails")]
    public void Contract017()
    {
        var input = UnsignedBuild(
            "0.14.0-dev",
            Output("c017"),
            Fixture("DesktopNode.Host.exe", "fake-host")) with
        {
            WixExitCode = 42,
            WixStdout = "fake wix stdout progress",
            WixStderr = "fake wix stderr detail",
        };
        var result = harness.Execute(input);
        Assert.Equal(42, result.ExitCode);
        Assert.StartsWith("{\"ok\":false", result.Json, StringComparison.Ordinal);
        using var json = JsonDocument.Parse(result.Json);
        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("PCV_INSTALLER_WIX_BUILD_FAILED", result.Error?.Code);
        Assert.Contains("fake wix stdout progress", result.WixOutput?.Stdout, StringComparison.Ordinal);
        Assert.Contains("fake wix stderr detail", result.WixOutput?.Stderr, StringComparison.Ordinal);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.018", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 18, "builds explicit Host and CLI payloads without requiring dotnet")]
    public void Contract018()
    {
        var output = Output("c018");
        var result = harness.Execute(UnsignedBuild(
            "0.14.0-dev",
            output,
            Fixture("DesktopNode.Host.exe", "fake-host")) with
        {
            DesktopNodeCliPath = Fixture("pcvcli.exe", "fake-cli"),
        });
        Assert.True(result.Ok);
        Assert.False(result.DotnetPublishRequired);
        var payload = Path.Combine(output, "payload");
        Assert.True(File.Exists(Path.Combine(payload, "DesktopNode.Host.exe")));
        Assert.True(File.Exists(Path.Combine(payload, "pcvcli.exe")));
        Assert.False(File.Exists(Path.Combine(payload, "pcvtui.exe")));
        Assert.Equal("explicit-path", result.Provenance?.ServiceHostSource);
        Assert.Equal("explicit-path", result.Provenance?.CliSource);
        Assert.False(result.Provenance?.HasTuiProperty);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.019", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 19, "cleans an existing payload directory before staging build files")]
    public void Contract019()
    {
        var output = Output("c019");
        var payload = Path.Combine(output, "payload");
        Directory.CreateDirectory(payload);
        var stale = Path.Combine(payload, "stale.txt");
        File.WriteAllText(stale, "stale");

        var result = harness.Execute(UnsignedBuild(
            "0.14.0-dev",
            output,
            Fixture("DesktopNode.Host.exe", "fake-host")));
        Assert.True(result.Ok);
        Assert.False(File.Exists(stale));
        Assert.True(File.Exists(Path.Combine(payload, "pcvcli.exe")));
        Assert.False(File.Exists(Path.Combine(payload, "pcvtui.exe")));
        Assert.Equal(8, result.Provenance?.PayloadFileCount);
        Assert.Matches("^[0-9a-f]{64}$", result.Provenance?.CliSha256!);
        Assert.False(result.Provenance?.HasTuiProperty);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.020", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 20, "stages only product-owned MSI runtime files without active spike payload sources")]
    public void Contract020()
    {
        Assert.False(harness.SourceBoundary.HasSpikePayloadSource);
        var output = Output("c020");
        var result = harness.Execute(UnsignedBuild(
            "0.14.0-dev",
            output,
            Fixture("DesktopNode.Host.exe", "fake-host")));
        var payload = Path.Combine(output, "payload");
        Assert.True(result.Ok);
        foreach (var relative in new[]
        {
            "DesktopNode.Host.exe", "pcvcli.exe", "Invoke-PcvDesktopNodeProduct.ps1",
            "PcvDesktopNodeProduct.psm1", Path.Combine("web", "index.html"),
        })
        {
            Assert.True(File.Exists(Path.Combine(payload, relative)), relative);
        }
        Assert.False(File.Exists(Path.Combine(payload, "pcvtui.exe")));
        Assert.False(Directory.Exists(Path.Combine(payload, "api")));
        Assert.False(Directory.Exists(Path.Combine(payload, "hyperv")));
        Assert.False(Directory.Exists(Path.Combine(payload, "service")));
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(payload, "product-manifest.json")));
        Assert.Equal(2, manifest.RootElement.GetProperty("schema_version").GetInt32());
        Assert.Equal("0.14.0-dev", manifest.RootElement.GetProperty("version").GetString());
        Assert.Equal("dotnet-windows-service", manifest.RootElement.GetProperty("service_host").GetProperty("mode").GetString());
        Assert.True(manifest.RootElement.GetProperty("update").GetProperty("installed_manifest_is_source_of_truth").GetBoolean());
        Assert.Empty(Directory.EnumerateDirectories(payload, "tests", SearchOption.AllDirectories));
        Assert.Equal(8, result.Provenance?.PayloadFileCount);
        Assert.EndsWith("pcvcli.exe", result.Provenance?.CliPath, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.Provenance?.HasTuiProperty);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-plan.021", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1", 21, "removes legacy WinSW root files during current MSI install")]
    public void Contract021()
    {
        var document = XDocument.Parse(harness.ProductWxs, LoadOptions.None);
        XNamespace ns = "http://wixtoolset.org/schemas/v4/wxs";
        var removals = document.Descendants(ns + "RemoveFile").ToArray();
        var executable = Assert.Single(removals, element => (string?)element.Attribute("Id") == "RemoveLegacyWinSwRootExe");
        Assert.Equal("PureCVisorDesktopNode.exe", (string?)executable.Attribute("Name"));
        Assert.Equal("install", (string?)executable.Attribute("On"));
        var xml = Assert.Single(removals, element => (string?)element.Attribute("Id") == "RemoveLegacyWinSwRootXml");
        Assert.Equal("PureCVisorDesktopNode.xml", (string?)xml.Attribute("Name"));
        Assert.Equal("install", (string?)xml.Attribute("On"));
    }

    public void Dispose()
    {
        var baseRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pcv-delivery-tests"));
        var target = Path.GetFullPath(temporaryRoot);
        Assert.StartsWith(baseRoot + Path.DirectorySeparatorChar, target, StringComparison.OrdinalIgnoreCase);
        if (Directory.Exists(target))
        {
            Directory.Delete(target, recursive: true);
        }
    }

    private InstallerBuildInput UnsignedDryRun(string version, string output, string host) =>
        new(version, output)
        {
            DesktopNodeHostPath = host,
            SigningMode = "AllowUnsignedDev",
            DryRun = true,
        };

    private InstallerBuildInput UnsignedBuild(string version, string output, string host) =>
        new(version, output)
        {
            DesktopNodeHostPath = host,
            SigningMode = "AllowUnsignedDev",
        };

    private string Output(string name) => Path.Combine(temporaryRoot, name);

    private string Fixture(string name, string content)
    {
        var path = Path.Combine(temporaryRoot, name);
        File.WriteAllText(path, content);
        return path;
    }
}
