using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

// Invoke-PcvManualAdminPackagePairCampaign.ps1 ran from an untracked worktree until 2026-09-29.
// Its behavior suite lives in packaging/windows-desktop-node/manual-admin-tests; required CI does
// not run PowerShell, so this class pins the orchestration contract the pair evidence relies on.
[Trait("Category", "Delivery")]
public sealed class PcvManualAdminPackagePairCampaignContractTests
{
    private const string Orchestrator =
        "packaging/windows-desktop-node/tools/Invoke-PcvManualAdminPackagePairCampaign.ps1";

    [Fact]
    public void RequiresOneModeAndValidatesThePackagePairBeforeWritingOutput()
    {
        var source = Source();

        RequireTokens(
            source,
            "[switch]$PlanOnly",
            "[switch]$Execute",
            "PCV_MANUAL_ADMIN_CAMPAIGN_MODE_REQUIRED",
            "PCV_MANUAL_ADMIN_CAMPAIGN_MODE_CONFLICT",
            "'pcv-manual-admin-package-pair-campaign-v1'",
            "'pcv-manual-admin-package-pair-campaign-summary-v1'",
            "PCV_MANUAL_ADMIN_CAMPAIGN_VERSION_INVALID|baseline must be less than target",
            "PCV_MANUAL_ADMIN_CAMPAIGN_MSI_HASH_MISMATCH",
            "PCV_MANUAL_ADMIN_CAMPAIGN_CATALOG_INVALID",
            "PCV_MANUAL_ADMIN_CAMPAIGN_PUBLICATION_INVALID",
            "PCV_MANUAL_ADMIN_CAMPAIGN_INSTALLED_VERSION_INVALID");
        AssertOrdered(
            source,
            "$baseline = Assert-PcvPackage $baselineRoot $BaselineVersion",
            "$targetCatalogUri = Assert-PcvCatalog $targetCatalog $targetZip $TargetVersion",
            "New-Item -ItemType Directory -Force -Path $root",
            "if ($PlanOnly) { return [pscustomobject]$summary }");
    }

    [Fact]
    public void RunsTheSixBucketsInPairOrderThroughTheCanonicalRunners()
    {
        var source = Source();

        AssertOrdered(
            source,
            "id = 'manual-admin-rebaseline-readiness'",
            "id = 'lifecycle-product-update-rollback'",
            "id = 'clean-host-windows-update'",
            "id = 'burn-bootstrapper-lifecycle'",
            "id = 'msix-package-lifecycle-smoke'",
            "id = 'installed-runtime-ops-summary'");
        RequireTokens(
            source,
            "'New-PcvManualAdminRebaselineReadiness.ps1'",
            "'Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'",
            "'New-PcvManualAdminCampaignDescriptor.ps1'",
            "'PcvManualAdminBaselineReservation.psm1'",
            "'Invoke-PcvBurnBootstrapperLifecycle.ps1'",
            "'Invoke-PcvMsixPackageLifecycleSmoke.ps1'",
            "Assert-PcvFile $entry.Value \"canonical-$($entry.Key)\"",
            "RemoveVmOnSuccess = $true; RemoveVmOnFailure = $true");
    }

    [Fact]
    public void KeepsCredentialsOutOfArtifactsAndClosesFailClosed()
    {
        var source = Source();

        RequireTokens(
            source,
            "credential_handling = '<guest-credential-at-execution-boundary>'",
            "signing_certificate = 'format-validated-thumbprint-redacted'",
            "certificate_generation = 'forbidden'",
            "PCV_MANUAL_ADMIN_CAMPAIGN_ELEVATION_REQUIRED",
            "PCV_MANUAL_ADMIN_CAMPAIGN_DESCRIPTOR_NOT_VALIDATED",
            "PCV_MANUAL_ADMIN_CAMPAIGN_ARTIFACT_ESCAPE",
            "PCV_MANUAL_ADMIN_CAMPAIGN_DESCRIPTOR_NOT_CLOSED",
            "-State consumed");
        AssertOrdered(
            source,
            "} finally {",
            "if ($mutationStarted -and $currentVersion -ne $TargetVersion) {",
            "$summary.restoration_attempted = $true");
        Assert.DoesNotContain("New-SelfSignedCertificate", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("REMOVE_DATA", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string Source() =>
        RepositoryContractContext.Find().ReadUtf8Text(Orchestrator);

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
