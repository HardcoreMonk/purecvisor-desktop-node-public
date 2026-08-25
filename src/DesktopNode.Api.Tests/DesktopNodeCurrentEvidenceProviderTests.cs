using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeCurrentEvidenceProviderTests
{
    [Fact]
    public void LoadPreservesBlockedQualificationAndBlockerOrder()
    {
        var path = WriteEvidence(BlockedEvidence("pcv.vm.saved-lifecycle", "pcv.vm.media-attach"));
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);

            Assert.Equal(1, snapshot.SchemaVersion);
            Assert.Equal("pcv-feature-promotion-decision-v1", snapshot.Contract);
            Assert.Equal("blocked", snapshot.Status);
            Assert.False(snapshot.PromotionEligible);
            Assert.Null(snapshot.ErrorCode);
            Assert.Equal(
                ["pcv.vm.saved-lifecycle", "pcv.vm.media-attach"],
                snapshot.Blockers.Select(blocker => blocker.FeatureId).ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadReturnsUnavailableForMissingAssetWithoutDisclosingPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-missing-" + Guid.NewGuid().ToString("N") + ".json");

        var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);
        var serialized = JsonSerializer.Serialize(snapshot);

        Assert.Equal("unavailable", snapshot.Status);
        Assert.False(snapshot.PromotionEligible);
        Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
        Assert.Empty(snapshot.Blockers);
        Assert.DoesNotContain(path, serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadReturnsUnavailableForMalformedAssetWithoutDisclosingParserInput()
    {
        var path = WriteEvidence("{broken-current-evidence");
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);
            var serialized = JsonSerializer.Serialize(snapshot);

            Assert.Equal("unavailable", snapshot.Status);
            Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
            Assert.DoesNotContain(path, serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("broken-current-evidence", serialized, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadReturnsUnavailableForAnInvalidCurrentEvidenceContract()
    {
        var path = WriteEvidence(BlockedEvidence("pcv.vm.saved-lifecycle")
            .Replace("pcv-current-evidence-v1", "pcv-current-evidence-v99", StringComparison.Ordinal));
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);

            Assert.Equal("unavailable", snapshot.Status);
            Assert.False(snapshot.PromotionEligible);
            Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
            Assert.Empty(snapshot.Blockers);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadReturnsUnavailableForNonNumericSchemaVersions()
    {
        var path = WriteEvidence(BlockedEvidence("pcv.vm.saved-lifecycle")
            .Replace("\"schema_version\":1", "\"schema_version\":\"1\"", StringComparison.Ordinal));
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);

            Assert.Equal("unavailable", snapshot.Status);
            Assert.False(snapshot.PromotionEligible);
            Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
            Assert.Empty(snapshot.Blockers);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WriteEvidence(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, content);
        return path;
    }

    private static string BlockedEvidence(params string[] featureIds)
    {
        var blockers = featureIds.Select(featureId => new
        {
            feature_id = featureId,
            stage = "actual_vm_tested",
            verdict = "fail"
        });
        return JsonSerializer.Serialize(new
        {
            schema_version = 1,
            contract = "pcv-current-evidence-v1",
            current = new { version = "0.42.74-admin-smoke" },
            feature_qualification = new
            {
                schema_version = 1,
                contract = "pcv-feature-promotion-decision-v1",
                promotion_eligible = false,
                blockers
            }
        });
    }
}
