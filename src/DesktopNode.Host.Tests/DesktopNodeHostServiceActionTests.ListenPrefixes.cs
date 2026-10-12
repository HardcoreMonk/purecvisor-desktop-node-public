using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed partial class DesktopNodeHostServiceActionTests
{
    // public-safety: synthetic-rfc1918
    private const string SyntheticLanHost = "192.168.1.17";

    private static DesktopNodeHostOptions ConfigurePrefixOptions(
        string action = "configure-installed",
        string? prefix = null,
        string? webPrefix = null,
        bool allowLan = false,
        string? dataRoot = "C:\\ProgramData\\PureCVisor\\desktop-node")
    {
        return new DesktopNodeHostOptions
        {
            Mode = DesktopNodeHostMode.ServiceAction,
            ServiceAction = action,
            ProductRoot = "C:\\Program Files\\PureCVisor\\DesktopNode",
            ServiceExecutablePath = "C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe",
            DataRoot = dataRoot,
            Prefix = prefix ?? "http://127.0.0.1:7777/",
            WebPrefix = webPrefix,
            AllowLan = allowLan
        };
    }

    [Theory]
    [InlineData("configure-installed")]
    [InlineData("repair-installed")]
    public void ConfigureAndRepairDefaultPrefixesMatchAndOmitAllowLan(string action)
    {
        var options = ConfigurePrefixOptions(action);
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);
        var configuration = DesktopNodeHostServiceAction.CreateServiceConfiguration(plan, options);

        Assert.Equal(plan.ServiceBinaryPathName, configuration.BinaryPathName);
        Assert.Contains("--prefix \"http://127.0.0.1:7777/\"", plan.ServiceBinaryPathName);
        Assert.Contains("--web-prefix \"http://127.0.0.1:80/\"", plan.ServiceBinaryPathName);
        Assert.DoesNotContain("--allow-lan", plan.ServiceBinaryPathName);
    }

    [Fact]
    public void ConfigureInstalledLanPrefixesMatchOnPlanAndNativeConfiguration()
    {
        var prefix = $"http://{SyntheticLanHost}:7777/";
        var webPrefix = $"http://{SyntheticLanHost}:80/";
        var options = ConfigurePrefixOptions(prefix: prefix, webPrefix: webPrefix, allowLan: true);
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);
        var configuration = DesktopNodeHostServiceAction.CreateServiceConfiguration(plan, options);

        Assert.Equal(plan.ServiceBinaryPathName, configuration.BinaryPathName);
        Assert.Contains($"--prefix \"{prefix}\"", plan.ServiceBinaryPathName);
        Assert.Contains($"--web-prefix \"{webPrefix}\"", plan.ServiceBinaryPathName);
        var allowLanAt = plan.ServiceBinaryPathName.IndexOf("--allow-lan", StringComparison.Ordinal);
        var webRootAt = plan.ServiceBinaryPathName.IndexOf("--web-root", StringComparison.Ordinal);
        Assert.True(allowLanAt >= 0 && allowLanAt < webRootAt);
    }

    [Fact]
    public void ConfigureInstalledLoopbackPrefixesDoNotEmitAllowLan()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(ConfigurePrefixOptions(allowLan: true));

        Assert.Contains("--prefix \"http://127.0.0.1:7777/\"", plan.ServiceBinaryPathName);
        Assert.DoesNotContain("--allow-lan", plan.ServiceBinaryPathName);
    }

    [Fact]
    public void ConfigureInstalledRejectsLanPrefixWithoutAllowLan()
    {
        var prefix = $"http://{SyntheticLanHost}:7777/";
        var webPrefix = $"http://{SyntheticLanHost}:80/";
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostServiceAction.CreatePlan(ConfigurePrefixOptions(prefix: prefix, webPrefix: webPrefix)));

        Assert.Contains("PCV_HOST_PREFIX_NOT_LOOPBACK", error.Message);
    }

    [Theory]
    [InlineData("http://+:7777/", "http://+:80/")]
    [InlineData("http://*:7777/", "http://*:80/")]
    [InlineData("http://0.0.0.0:7777/", "http://0.0.0.0:80/")]
    [InlineData("http://[::]:7777/", "http://[::]:80/")]
    public void ConfigureInstalledRejectsWildcardPrefixes(string prefix, string webPrefix)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostServiceAction.CreatePlan(
                ConfigurePrefixOptions(prefix: prefix, webPrefix: webPrefix, allowLan: true)));

        Assert.Contains("PCV_HOST_PREFIX_WILDCARD", error.Message);
    }

    [Fact]
    public void ConfigureInstalledRejectsMismatchedPrefixHosts()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostServiceAction.CreatePlan(ConfigurePrefixOptions(
                prefix: $"http://{SyntheticLanHost}:7777/",
                webPrefix: "http://127.0.0.1:80/",
                allowLan: true)));

        Assert.Contains("PCV_HOST_PREFIX_HOST_MISMATCH", error.Message);
    }

    [Fact]
    public void ConfigureInstalledRejectsLanApiPortOtherThan7777()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostServiceAction.CreatePlan(ConfigurePrefixOptions(
                prefix: $"http://{SyntheticLanHost}:7778/",
                webPrefix: $"http://{SyntheticLanHost}:80/",
                allowLan: true)));

        Assert.Contains("PCV_HOST_PREFIX_API_PORT", error.Message);
    }

    [Fact]
    public void ConfigureInstalledRejectsNonHttpScheme()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostServiceAction.CreatePlan(ConfigurePrefixOptions(
                prefix: "https://127.0.0.1:7777/",
                webPrefix: "https://127.0.0.1:80/")));

        Assert.Contains("PCV_HOST_PREFIX_SCHEME", error.Message);
    }

    [Fact]
    public void FirewallEnableIgnoresListenPrefixOptions()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(ConfigurePrefixOptions(
            action: "firewall-enable",
            prefix: $"http://{SyntheticLanHost}:7777/",
            webPrefix: $"http://{SyntheticLanHost}:80/",
            allowLan: true));

        Assert.DoesNotContain("--allow-lan", plan.ServiceBinaryPathName);
        Assert.DoesNotContain(SyntheticLanHost, plan.ServiceBinaryPathName);
    }

    [Fact]
    public void ConfigureInstalledLanNativeConfigurationPreservesCredentialTargetAndBatchEvidenceRoot()
    {
        var prefix = $"http://{SyntheticLanHost}:7777/";
        var webPrefix = $"http://{SyntheticLanHost}:80/";
        var options = ConfigurePrefixOptions(prefix: prefix, webPrefix: webPrefix, allowLan: true);
        var plan = DesktopNodeHostServiceAction.CreatePlan(options);
        var configuration = DesktopNodeHostServiceAction.CreateServiceConfiguration(
            plan,
            options,
            useCredentialManagerToken: true,
            credentialTargetOverride: "PureCVisor/Custom/api-token",
            batchEvidenceRootOverride: "D:\\PureCVisorEvidence\\batch-runs");

        Assert.Contains($"--prefix \"{prefix}\"", configuration.BinaryPathName);
        Assert.Contains($"--web-prefix \"{webPrefix}\"", configuration.BinaryPathName);
        Assert.Contains("--allow-lan", configuration.BinaryPathName);
        Assert.Contains("--api-token-credential-target", configuration.BinaryPathName);
        Assert.Contains("\"PureCVisor/Custom/api-token\"", configuration.BinaryPathName);
        Assert.DoesNotContain("--api-token-protected-file", configuration.BinaryPathName);
        Assert.Contains("--batch-evidence-root", configuration.BinaryPathName);
        Assert.Contains("\"D:\\PureCVisorEvidence\\batch-runs\"", configuration.BinaryPathName);
        Assert.Contains($"--prefix \"{prefix}\"", plan.ServiceBinaryPathName);
        Assert.Contains("--allow-lan", plan.ServiceBinaryPathName);
    }

    [Fact]
    public async Task RepairInstalledLanPrefixesPreserveCredentialTargetAndBatchEvidenceRoot()
    {
        var dataRoot = Path.Combine(Path.GetTempPath(), "pcv-host-service-repair-lan-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        try
        {
            var prefix = $"http://{SyntheticLanHost}:7777/";
            var webPrefix = $"http://{SyntheticLanHost}:80/";
            var batchEvidenceRoot = "D:\\PureCVisorEvidence\\batch-runs";
            var currentBinaryPath =
                "\"C:\\Program Files\\PureCVisor\\DesktopNode\\DesktopNode.Host.exe\" listen " +
                "--api-token-credential-target \"PureCVisor/Custom/api-token\" " +
                $"--batch-evidence-root \"{batchEvidenceRoot}\"";
            var controller = new FakeWindowsServiceController
            {
                Snapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StoppedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "stopped",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0),
                StartedSnapshot = new DesktopNodeWindowsServiceSnapshot(
                    ServiceName: "PureCVisorDesktopNode",
                    Exists: true,
                    Status: "running",
                    BinaryPathName: currentBinaryPath,
                    Win32ExitCode: 0)
            };

            var result = await ExecuteWithHardenerAsync(
                ConfigurePrefixOptions(
                    action: "repair-installed",
                    prefix: prefix,
                    webPrefix: webPrefix,
                    allowLan: true,
                    dataRoot: dataRoot),
                controller,
                new RecordingFileAclHardener());

            Assert.True(result.Ok);
            Assert.Equal(["query", "stop", "configure", "start"], controller.Calls);
            Assert.Single(controller.Configurations);
            var binaryPath = controller.Configurations[0].BinaryPathName;
            Assert.Contains($"--prefix \"{prefix}\"", binaryPath);
            Assert.Contains($"--web-prefix \"{webPrefix}\"", binaryPath);
            Assert.Contains("--allow-lan", binaryPath);
            Assert.Contains("--api-token-credential-target", binaryPath);
            Assert.Contains("\"PureCVisor/Custom/api-token\"", binaryPath);
            Assert.DoesNotContain("--api-token-protected-file", binaryPath);
            Assert.Contains($"\"{batchEvidenceRoot}\"", binaryPath);
            Assert.Contains($"--prefix \"{prefix}\"", result.Plan.ServiceBinaryPathName);
            Assert.Contains("--allow-lan", result.Plan.ServiceBinaryPathName);
        }
        finally
        {
            if (Directory.Exists(dataRoot))
            {
                Directory.Delete(dataRoot, recursive: true);
            }
        }
    }
}
