using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "Installer")]
public sealed class PcvDesktopNodeInstallerLifecycleContractTests : IDisposable
{
    private const string ModulePath =
        "packaging/windows-desktop-node/installer/PcvDesktopNodeMsiLifecycle.psm1";

    private readonly string temporaryRoot = Path.Combine(
        Path.GetTempPath(),
        "pcv-delivery-tests",
        Guid.NewGuid().ToString("N"));
    private readonly RepositoryContractContext repository = RepositoryContractContext.Find();

    public PcvDesktopNodeInstallerLifecycleContractTests()
    {
        Directory.CreateDirectory(temporaryRoot);
        File.WriteAllText(MsiPath, "fake-msi");
    }

    private string MsiPath => Path.Combine(
        temporaryRoot,
        "PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi");

    private string LogDirectory => Path.Combine(temporaryRoot, "logs");

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-lifecycle.001",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1",
        1,
        "plans repair through explicit reinstall properties instead of /fa force-all shorthand")]
    public void Contract001()
    {
        var plan = BuildPlan();
        var repair = Assert.Single(plan.Steps, step => step.Phase == "Repair");

        Assert.Equal("msiexec.exe", repair.FilePath);
        Assert.Contains("/i", repair.Arguments);
        Assert.DoesNotContain("/fa", repair.Arguments);
        Assert.Contains("REINSTALL=ALL", repair.Arguments);
        Assert.Contains("REINSTALLMODE=vomus", repair.Arguments);
        Assert.Contains("REBOOT=ReallySuppress", repair.Arguments);
        Assert.Contains("MSIRESTARTMANAGERCONTROL=Disable", repair.Arguments);
        Assert.Contains("/norestart", repair.Arguments);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-lifecycle.002",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1",
        2,
        "suppresses reboot and Restart Manager actions for every mutating MSI step")]
    public void Contract002()
    {
        var plan = BuildPlan();

        Assert.Equal(5, plan.Steps.Count);
        foreach (var step in plan.Steps)
        {
            Assert.True(step.MutatesHost);
            Assert.Contains("REBOOT=ReallySuppress", step.Arguments);
            Assert.Contains("MSIRESTARTMANAGERCONTROL=Disable", step.Arguments);
            Assert.Contains("/norestart", step.Arguments);
            Assert.Contains("/l*vx", step.Arguments);
        }

        Assert.True(plan.NoAutoReboot.Enabled);
        Assert.Equal("Disable", plan.NoAutoReboot.RestartManagerControl);
        Assert.Equal("ReallySuppress", plan.NoAutoReboot.RebootProperty);
        Assert.Equal(1641, plan.NoAutoReboot.RebootInitiatedExitCode);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-lifecycle.003",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1",
        3,
        "classifies repair 3010 as success only after preservation assertions pass")]
    public void Contract003()
    {
        var source = ReadModule();
        var pending = MsiLifecycleContractVerifier.Classify(source, "Repair", 3010, false);
        Assert.False(pending.Ok);
        Assert.Equal("reboot_required_pending_assertions", pending.Result);
        Assert.True(pending.RebootRequired);
        Assert.False(pending.ActualRebootInitiated);

        var accepted = MsiLifecycleContractVerifier.Classify(source, "Repair", 3010, true);
        Assert.True(accepted.Ok);
        Assert.Equal("reboot_required_success", accepted.Result);
        Assert.True(accepted.RebootRequired);
        Assert.True(accepted.RequiresPostRebootVerification);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-lifecycle.004",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1",
        4,
        "keeps repair 3010 out of unconditional success exit codes")]
    public void Contract004()
    {
        var repair = Assert.Single(BuildPlan().Steps, step => step.Phase == "Repair");

        Assert.Contains(0, repair.SuccessExitCodes);
        Assert.DoesNotContain(3010, repair.SuccessExitCodes);
        Assert.Contains(3010, repair.ConditionalExitCodes);
    }

    [PcvLegacyContract(
        "pcv.installer.desktop-node-installer-lifecycle.005",
        "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1",
        5,
        "classifies 1641 as reboot-initiated failure for every lifecycle phase")]
    public void Contract005()
    {
        foreach (var phase in new[] { "Install", "Repair", "Uninstall", "InstallRemoveData", "UninstallRemoveData" })
        {
            var classification = MsiLifecycleContractVerifier.Classify(ReadModule(), phase, 1641, true);
            Assert.False(classification.Ok);
            Assert.Equal("reboot_initiated_failure", classification.Result);
            Assert.True(classification.ActualRebootInitiated);
            Assert.True(classification.RequiresPostRebootVerification);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(temporaryRoot))
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    private MsiLifecyclePlanProjection BuildPlan() =>
        MsiLifecycleContractVerifier.BuildPlan(ReadModule(), MsiPath, LogDirectory);

    private string ReadModule() => repository.ReadUtf8Text(ModulePath);
}
