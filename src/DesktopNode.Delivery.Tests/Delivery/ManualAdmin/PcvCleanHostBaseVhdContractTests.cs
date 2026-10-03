using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

// New-PcvCleanHostBaseVhd.ps1 services a copy of the clean-host evaluation VHD offline. Its behavior suite lives
// in packaging/windows-desktop-node/manual-admin-tests; required CI does not run PowerShell, so this class pins
// the source preservation, plan-only default, no-download, and discard-on-failure contract.
[Trait("Category", "Delivery")]
public sealed class PcvCleanHostBaseVhdContractTests
{
    private const string Tool = "packaging/windows-desktop-node/tools/New-PcvCleanHostBaseVhd.ps1";

    [Fact]
    public void ValidatesInputsAndPlansBeforeAnyWrite()
    {
        var source = Source();

        RequireTokens(
            source,
            "[switch]$Execute",
            "PCV_BASE_VHD_KB_INVALID",
            "PCV_BASE_VHD_PACKAGE_KB_MISMATCH",
            "PCV_BASE_VHD_PACKAGE_HASH_MISMATCH",
            "PCV_BASE_VHD_TARGET_IS_SOURCE",
            "PCV_BASE_VHD_TARGET_EXISTS",
            "download_performed = $false",
            "source_modified = $false",
            "current_base_changed = $false");
        AssertOrdered(
            source,
            "$packageSha256 = Get-PcvSha256 $packageFullPath",
            "if (Test-Path -LiteralPath $existing) { throw \"PCV_BASE_VHD_TARGET_EXISTS|$existing\" }",
            "if (-not $Execute) { return [pscustomobject]$result }",
            "PCV_BASE_VHD_ELEVATION_REQUIRED",
            "PCV_BASE_VHD_PACKAGED_HOST_UNSUPPORTED",
            "Copy-Item -LiteralPath $sourcePath -Destination $basePath");
        Assert.DoesNotContain("Invoke-WebRequest", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-BitsTransfer", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mount-WindowsImage -ImagePath $sourcePath", source, StringComparison.Ordinal);
        Assert.DoesNotContain("StartComponentCleanup", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChecksTheOfflineUbrBeforeSavingAndDiscardsTheCopyOnFailure()
    {
        var source = Source();

        // The combined .msu fails offline on the 20348.169 image (0x800f0823): the SSU cab goes in first.
        AssertOrdered(
            source,
            "Expand-PcvMsuPackage -PackagePath $packageFullPath -Destination $packageExtractPath",
            "-Filter 'SSU-*.cab'",
            "PCV_BASE_VHD_MSU_LAYOUT_INVALID",
            "$applyPackages = @($servicingStack) + @($cumulative)",
            "Copy-Item -LiteralPath $sourcePath -Destination $basePath",
            "Mount-WindowsImage -ImagePath $basePath -Index 1 -Path $mountPath",
            "foreach ($package in $applyPackages) {",
            "Add-WindowsPackage -Path $mountPath -PackagePath $package.FullName",
            "$offline = Get-PcvOfflineImageBuild -MountPath $mountPath",
            "PCV_BASE_VHD_UBR_MISMATCH",
            "Dismount-WindowsImage -Path $mountPath -Save",
            "catch {",
            "Dismount-WindowsImage -Path $mountPath -Discard",
            "Remove-Item -LiteralPath $basePath -Force",
            "Write-PcvJson $sidecarPath $sidecar");
        RequireTokens(
            source,
            "'Windows\\System32\\config\\SOFTWARE'",
            "& reg.exe load \"HKLM\\$key\" $hive",
            "& reg.exe unload \"HKLM\\$key\"",
            "$PSHOME -like '*\\WindowsApps\\*'",
            "& (Join-Path $env:SystemRoot 'System32\\expand.exe') \"-F:*.cab\" $PackagePath $Destination",
            "applied_packages = $appliedPackages",
            "Remove-Item -LiteralPath $packageExtractPath -Recurse -Force");
    }

    [Fact]
    public void PinsTheSidecarAndCurrentBaseSchemasAndLeavesPruningToTheOperator()
    {
        var source = Source();

        RequireTokens(
            source,
            "$script:BaseSchema = 'pcv-clean-host-base-vhd-v1'",
            "$script:CurrentSchema = 'pcv-clean-host-current-base-v1'",
            "$script:CurrentFileName = 'current-base.json'",
            "$script:RetainedPreviousBases = 2",
            "$baseFile = \"$($script:BaseBuild).$ExpectedUbr-$BuildDate.vhd\"",
            "$sidecarPath = \"$basePath.base.json\"",
            "component_cleanup = $false",
            "PCV_BASE_VHD_SIDECAR_MISSING",
            "PCV_BASE_VHD_SIDECAR_MISMATCH|sha256=",
            "prune_performed = $false");
        AssertOrdered(
            source,
            "$baseSha256 = Get-PcvSha256 $basePath",
            "if ($baseSha256 -ne $sidecar.value.base_sha256)",
            "if ($Execute) { Write-PcvJson $currentPath $current }");
        Assert.DoesNotContain("Remove-Item -LiteralPath $sourcePath", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CleanHostRunnerPrefersExplicitThenCurrentBaseThenTheEvaluationVhd()
    {
        var runner = RepositoryContractContext.Find().ReadUtf8Text(Runner);

        RequireTokens(
            runner,
            "[string]$BaseVhdPath = '',",
            "'pcv-clean-host-current-base-v1'",
            "PCV_CLEAN_HOST_CURRENT_BASE_INVALID",
            "[System.IO.Path]::GetFileName($baseFile) -ne $baseFile",
            "$sidecarPath = \"$full.base.json\"");
        AssertOrdered(
            runner,
            "if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {",
            "$source = 'explicit'",
            "'current-base.json'",
            "$source = 'current-base'",
            "$path = $DefaultSourcePath",
            "$source = 'default-source'");
        AssertOrdered(
            runner,
            "$baseVhd = Resolve-PcvCleanHostBaseVhd -ExplicitPath $BaseVhdPath -DefaultSourcePath $script:DefaultSourceBaseVhdPath",
            "base_vhd_source = $baseVhd.source",
            "base_vhd_ubr = $baseVhd.ubr",
            "$summary.base_vhd_source = $baseVhd.source",
            "$summary.base_vhd_ubr = $baseVhd.ubr",
            "$summary.base_vhd_kb = $baseVhd.kb",
            "New-VHD -Path $diffVhdPath -ParentPath $baseVhdFull -Differencing");
    }

    private const string Runner =
        "packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1";

    private static string Source() =>
        RepositoryContractContext.Find().ReadUtf8Text(Tool);

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
