using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

// The Burn and MSIX package-pair buckets ran from untracked scripts until 2026-09-29. Their
// behavior suites live in packaging/windows-desktop-node/manual-admin-tests; required CI does not
// run PowerShell, so this class pins the source contracts those suites and the pair evidence rely on.
[Trait("Category", "Delivery")]
public sealed class PcvPairLifecycleRunnersContractTests
{
    private const string BurnRunner =
        "packaging/windows-desktop-node/tools/Invoke-PcvBurnBootstrapperLifecycle.ps1";
    private const string MsixRunner =
        "packaging/windows-desktop-node/tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1";
    private const string MsixTemplateRoot = "packaging/windows-desktop-node/msix/template-layout";

    [Fact]
    public void BurnRunnerChecksTheTargetPrestateAndRestoresTheTargetMsiInFinally()
    {
        var source = Source(BurnRunner);

        RequireTokens(
            source,
            "[switch]$PlanOnly",
            "[switch]$Execute",
            "PCV_BURN_LIFECYCLE_MODE_REQUIRED",
            "'pcv-burn-bootstrapper-lifecycle-v1'",
            "{8F455BB4-640E-47A2-A982-338C7A6318B5}",
            "WixToolset.BootstrapperApplications.wixext",
            "PCV_BURN_LIFECYCLE_PRESTATE_INVALID",
            "'REBOOT=ReallySuppress', 'MSIRESTARTMANAGERCONTROL=Disable'",
            "-Action RepairInstalled",
            "restoration_status");
        AssertOrdered(
            source,
            "$prestate = Get-PcvProductState 'prestate' $true",
            "Invoke-PcvProcess 'install' $bundle",
            "Invoke-PcvProcess 'repair' $bundle",
            "Invoke-PcvProcess 'remove' $bundle",
            "Get-PcvProductState 'remove-absence' $false",
            "finally",
            "Invoke-PcvProcess 'restore-target-msi' 'msiexec.exe'");
        Assert.DoesNotContain("New-SelfSignedCertificate", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("REMOVE_DATA", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MsixRunnerUsesAnExistingCertificateAndRemovesTheSmokePackageInFinally()
    {
        var source = Source(MsixRunner);

        RequireTokens(
            source,
            "[switch]$PlanOnly",
            "[switch]$Execute",
            "PCV_MSIX_LIFECYCLE_MODE_REQUIRED",
            "'pcv-msix-package-lifecycle-v1'",
            "certificate_generation = 'forbidden'",
            "PCV_MSIX_LIFECYCLE_CERTIFICATE_INVALID",
            "'1.3.6.1.5.5.7.3.3'",
            "preexisting-smoke-identity",
            "PCV_MSIX_LIFECYCLE_TEMPLATE_CONTRACT_INVALID",
            "PCV_MSIX_LIFECYCLE_VERSION_ORDER_INVALID",
            "/sha1 <redacted>",
            "target_manifest_unchanged");
        AssertOrdered(
            source,
            "Assert-PcvManifestContract",
            "Add-AppxPackage -Path $packages[0]",
            "Add-AppxPackage -Path $packages[1] -ForceUpdateFromAnyVersion",
            "finally",
            "Remove-AppxPackage -Package $package.PackageFullName");
        Assert.DoesNotContain("New-SelfSignedCertificate", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("msiexec", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MsixTemplateLayoutCarriesThePlaceholdersAndAssetsTheRunnerExpects()
    {
        var repository = RepositoryContractContext.Find();
        var template = repository.ReadUtf8Text($"{MsixTemplateRoot}/AppxManifest.xml.template");

        RequireTokens(
            template,
            "Name=\"{{PackageName}}\"",
            "Publisher=\"{{Publisher}}\"",
            "Version=\"{{Version}}\"",
            "ProcessorArchitecture=\"x64\"",
            "Name=\"{{ServiceName}}\" StartupType=\"manual\" StartAccount=\"localSystem\"",
            "--prefix http://127.0.0.1:7789/",
            "<rescap:Capability Name=\"runFullTrust\" />",
            "<rescap:Capability Name=\"packagedServices\" />",
            "<rescap:Capability Name=\"localSystemServices\" />");
        Assert.Equal(
            [
                $"{MsixTemplateRoot}/Assets/Square150x150Logo.png",
                $"{MsixTemplateRoot}/Assets/Square44x44Logo.png",
                $"{MsixTemplateRoot}/Assets/StoreLogo.png",
            ],
            repository.EnumerateRegularFiles($"{MsixTemplateRoot}/Assets", ".png").Order(StringComparer.Ordinal));
    }

    private static string Source(string path) =>
        RepositoryContractContext.Find().ReadUtf8Text(path);

    private static void RequireTokens(string source, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            Assert.Contains(token, source, StringComparison.Ordinal);
        }
    }

    private static void AssertOrdered(string source, params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            Assert.True(index >= 0, $"Missing or out-of-order source token: {token}");
            offset = index + token.Length;
        }
    }
}
