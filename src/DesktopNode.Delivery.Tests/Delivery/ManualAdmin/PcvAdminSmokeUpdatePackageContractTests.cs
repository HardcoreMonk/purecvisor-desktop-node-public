using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

// New-PcvAdminSmokeUpdatePackage.ps1 replaces the hand-made update ZIP of 0.42.83-0.42.89 and writes the
// update catalog the pair orchestrator needs plus package-facts.json for the train evidence facts generator
// (design pcv-train-pair-orchestrator-v1 section 3a). Required CI does not run PowerShell, so this pins its contract.
[Trait("Category", "Delivery")]
public sealed class PcvAdminSmokeUpdatePackageContractTests
{
    private const string Tool = "packaging/windows-desktop-node/tools/New-PcvAdminSmokeUpdatePackage.ps1";

    [Fact]
    public void ValidatesThePackageBeforeWritingAnyOutput()
    {
        var source = Source();

        RequireTokens(
            source,
            "PCV_UPDATE_PACKAGE_VERSION_INVALID",
            "PCV_UPDATE_PACKAGE_INPUT_MISSING",
            "PCV_UPDATE_PACKAGE_MANIFEST_MISMATCH",
            "PCV_UPDATE_PACKAGE_MSI_HASH_MISMATCH",
            "PCV_UPDATE_PACKAGE_UPGRADE_TABLE_INVALID");
        AssertOrdered(
            source,
            "PCV_UPDATE_PACKAGE_MSI_HASH_MISMATCH",
            "PCV_UPDATE_PACKAGE_OUTPUT_EXISTS",
            "PCV_UPDATE_PACKAGE_UPGRADE_TABLE_INVALID",
            "[System.IO.Compression.ZipFile]::Open($zipPath",
            "Write-PcvJson $catalogPath $catalog",
            "Write-PcvJson $factsPath $facts");
    }

    [Fact]
    public void WritesTheCatalogFieldsTheOrchestratorAsserts()
    {
        var source = Source();

        RequireTokens(
            source,
            "product = 'PureCVisor Desktop Node'",
            "name = 'admin-smoke'; channel = 'admin-smoke'; release_channel = 'admin-smoke'; version = $Version",
            "signing_mode = 'AllowUnsignedDev'; source_uri = ([uri]$zipPath).AbsoluteUri; expected_sha256 = $zipSha256",
            "public_trusted_signing = 'not-claimed'; external_stable_publication = 'not-claimed'",
            ".Replace('\\', '/')",
            "[System.IO.Compression.CompressionLevel]::Optimal");
    }

    [Fact]
    public void ReadsTheMsiReadOnlyAndRecordsPackageFacts()
    {
        var source = Source();

        RequireTokens(
            source,
            "contract = 'pcv-admin-smoke-package-facts-v1'",
            "'OpenDatabase' 'InvokeMethod' @($msiPath, 0)",
            "SELECT VersionMin, VersionMax, Attributes FROM Upgrade",
            "Action='RemoveExistingProducts'",
            "msi_upgrade_rows = $upgradeRows",
            "remove_existing_products_sequence = $removeExistingProducts",
            "host_mutation_performed = $false");
        Assert.DoesNotContain("msiexec", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Commit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Invoke-WebRequest", source, StringComparison.Ordinal);
    }

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
