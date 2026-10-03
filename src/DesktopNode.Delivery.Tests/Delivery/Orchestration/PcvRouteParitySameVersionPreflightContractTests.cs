using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

// Option C of docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md:
// the route parity admin smoke stops before a same-version residual ProductCode and after an installed
// build that is not the gate build. Required CI runs no PowerShell, so the function behavior is exercised
// by the smoke's -SelfTest mode and this class pins the wiring.
[Trait("Category", "Delivery")]
public sealed class PcvRouteParitySameVersionPreflightContractTests
{
    private const string SmokePath = "packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1";

    private static readonly string Smoke = RepositoryContractContext.Find().ReadUtf8Text(SmokePath);

    [Fact]
    public void ChecksTheArpForASameVersionResidualBeforeTheGateBuild()
    {
        var preflight = IndexOf("Start-Step -Name 'same-version-preflight' -Path $sameVersionPath");
        var build = IndexOf("Start-Step -Name 'build-current-admin-smoke-msi'");

        Assert.True(preflight < build, "same-version-preflight must run before the gate MSI build.");
        Assert.Contains("-ArpEntries (Get-SmokeArpProductEntries)", Smoke, StringComparison.Ordinal);
        Assert.Contains("throw \"PCV_SMOKE_SAME_VERSION_RESIDUAL|", Smoke, StringComparison.Ordinal);
        Assert.Contains("remove-the-same-version-product-without-REMOVE_DATA-then-rerun", Smoke, StringComparison.Ordinal);
    }

    [Fact]
    public void ComparesTheInstalledHostBuildCommitWithTheGateBuildAfterFinalRestoreInstall()
    {
        var finalRestore = IndexOf("@{ name = 'final-restore-install'; phase = 'Install';");
        var check = IndexOf("$lifecycle.installed_build = Test-SmokeInstalledBuildCommit -InstalledProductVersion $installedHostProductVersion -GateCommit $gateCommit");
        var lifecycleOk = Smoke.IndexOf("$lifecycle.ok = $true", check, StringComparison.Ordinal);

        Assert.True(finalRestore < check, "the installed build check must follow the MSI lifecycle steps.");
        Assert.True(check < lifecycleOk, "the installed build check must gate msi-lifecycle-smoke ok.");
        Assert.Contains("$gateCommit = [string](Get-ObjectPropertyValue -InputObject $buildOutput.provenance -Name 'git_commit')", Smoke, StringComparison.Ordinal);
        Assert.Contains("throw \"PCV_SMOKE_INSTALLED_BUILD_MISMATCH|", Smoke, StringComparison.Ordinal);
    }

    [Fact]
    public void KeepsTheArpReaderReadOnly()
    {
        var reader = FunctionBody("Get-SmokeArpProductEntries");

        Assert.Contains("HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", reader, StringComparison.Ordinal);
        Assert.Contains("HKLM:\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall", reader, StringComparison.Ordinal);
        foreach (var mutation in new[] { "Remove-Item", "Set-ItemProperty", "New-ItemProperty", "msiexec", "Start-Process" })
        {
            Assert.DoesNotContain(mutation, reader, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void FailsClosedWhenTheBuildCommitCannotBeRead()
    {
        var check = FunctionBody("Test-SmokeInstalledBuildCommit");

        Assert.Contains("-not [string]::IsNullOrWhiteSpace($installedCommit) -and", check, StringComparison.Ordinal);
        Assert.Contains("-not [string]::IsNullOrWhiteSpace($expectedCommit) -and", check, StringComparison.Ordinal);
    }

    [Fact]
    public void CoversBothChecksInTheSelfTest()
    {
        Assert.Contains("Start-Step -Name 'same-version-preflight-self-test'", Smoke, StringComparison.Ordinal);
        Assert.Contains("$buildUnknownCase = Test-SmokeInstalledBuildCommit -InstalledProductVersion '0.42.86-admin-smoke' -GateCommit 'b807803f'", Smoke, StringComparison.Ordinal);
        Assert.Contains("$ok = [bool]($captureOk -and $protectedTokenSelfTestOk -and $msiClassifierOk -and $sameVersionSelfTestOk)", Smoke, StringComparison.Ordinal);
    }

    private static int IndexOf(string literal)
    {
        var index = Smoke.IndexOf(literal, StringComparison.Ordinal);
        Assert.True(index >= 0, $"missing literal: {literal}");
        return index;
    }

    private static string FunctionBody(string name)
    {
        var start = IndexOf($"function {name} {{");
        var next = Smoke.IndexOf("\nfunction ", start + 1, StringComparison.Ordinal);
        return next < 0 ? Smoke[start..] : Smoke[start..next];
    }
}
