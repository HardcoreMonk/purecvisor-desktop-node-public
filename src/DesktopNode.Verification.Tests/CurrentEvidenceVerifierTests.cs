using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class CurrentEvidenceVerifierTests
{
    [Fact]
    public void CanonicalRepositoryPassesWithoutWritingOwnedFiles()
    {
        var root = VerificationCatalogFixture.RepositoryRoot;
        var paths = CurrentEvidenceVerifier.OwnedRelativePaths
            .Append(CurrentEvidenceVerifier.RecordPath)
            .Append(CurrentEvidenceVerifier.SchemaPath)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var before = paths.ToDictionary(
            path => path,
            path => Hash(Path.Combine(root, path)),
            StringComparer.Ordinal);

        var result = CurrentEvidenceVerifier.Verify(root, CancellationToken.None);

        Assert.Equal("0.42.74-admin-smoke", result.Record.Current.Version);
        Assert.Equal(8, result.Targets.Count);
        Assert.All(result.Targets, target => Assert.Equal("current", target.Status));
        Assert.Equal(
            before,
            paths.ToDictionary(
                path => path,
                path => Hash(Path.Combine(root, path)),
                StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("root-extra", "root.unexpected")]
    [InlineData("contract-case", "feature_qualification.contract")]
    [InlineData("blocker-extra", "feature_qualification.blockers.unexpected")]
    [InlineData("contradictory", "feature_qualification.blockers")]
    [InlineData("version-case", "current.version")]
    [InlineData("sha-case", "current.clean_msi_sha256")]
    [InlineData("missing-reference", "current.installed_evidence")]
    [InlineData("path-escape", "current.installed_evidence")]
    public void RejectsStrictRecordMutations(string mutation, string field)
    {
        var root = VerificationCatalogFixture.RepositoryRoot;
        var json = File.ReadAllText(Path.Combine(root, CurrentEvidenceVerifier.RecordPath));
        var document = JsonNode.Parse(json)!.AsObject();
        Mutate(document, mutation);

        var error = Assert.Throws<CurrentEvidenceException>(() =>
            CurrentEvidenceVerifier.ValidateJson(
                document.ToJsonString(),
                root,
                CancellationToken.None));

        Assert.StartsWith($"PCV_CURRENT_EVIDENCE_INVALID|{field}|", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(root, error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderIsBoundedAndIndependentOfHistoricalText()
    {
        var root = VerificationCatalogFixture.RepositoryRoot;
        var record = CurrentEvidenceVerifier.ValidateJson(
            File.ReadAllText(Path.Combine(root, CurrentEvidenceVerifier.RecordPath)),
            root,
            CancellationToken.None);

        var block = CurrentEvidenceVerifier.Render(record);

        Assert.StartsWith("<!-- BEGIN GENERATED CURRENT EVIDENCE -->", block, StringComparison.Ordinal);
        Assert.EndsWith("<!-- END GENERATED CURRENT EVIDENCE -->", block, StringComparison.Ordinal);
        Assert.Contains("Feature qualification:", block, StringComparison.Ordinal);
        Assert.Contains("promotion_eligible=false", block, StringComparison.Ordinal);
        Assert.Contains("blocker_count=1", block, StringComparison.Ordinal);
        Assert.Contains("pcv.vm.saved-lifecycle/actual_vm_tested/fail", block, StringComparison.Ordinal);
        Assert.DoesNotContain("Web/TUI/CLI current-card", block, StringComparison.Ordinal);
    }

    [Fact]
    public void StaleDocumentFailsWithoutReturningMutatedContent()
    {
        const string stale =
            "# fixture\n<!-- BEGIN GENERATED CURRENT EVIDENCE -->\nstale\n" +
            "<!-- END GENERATED CURRENT EVIDENCE -->\nfixture tail";
        var root = VerificationCatalogFixture.RepositoryRoot;
        var record = CurrentEvidenceVerifier.ValidateJson(
            File.ReadAllText(Path.Combine(root, CurrentEvidenceVerifier.RecordPath)),
            root,
            CancellationToken.None);
        var block = CurrentEvidenceVerifier.Render(record);

        var error = Assert.Throws<CurrentEvidenceException>(() =>
            CurrentEvidenceVerifier.VerifyDocument("fixture.md", stale, block));

        Assert.Equal("PCV_CURRENT_EVIDENCE_STALE|fixture.md|generated-block", error.Message);
        Assert.Contains("stale", stale, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateMarkerAndMalformedJsonFailClosed()
    {
        var markerError = Assert.Throws<CurrentEvidenceException>(() =>
            CurrentEvidenceVerifier.VerifyDocument(
                "fixture.md",
                "<!-- BEGIN GENERATED CURRENT EVIDENCE --><!-- BEGIN GENERATED CURRENT EVIDENCE -->" +
                "<!-- END GENERATED CURRENT EVIDENCE -->",
                "block"));
        Assert.Equal("PCV_CURRENT_EVIDENCE_INVALID|fixture.md|marker-cardinality", markerError.Message);

        var jsonError = Assert.Throws<CurrentEvidenceException>(() =>
            CurrentEvidenceVerifier.ValidateJson(
                """{"schema_version":1,"schema_version":1}""",
                VerificationCatalogFixture.RepositoryRoot,
                CancellationToken.None));
        Assert.Equal(
            "PCV_CURRENT_EVIDENCE_INVALID|schema_version|duplicate",
            jsonError.Message);
    }

    [Fact]
    public void PreCancelledVerificationStopsBeforeReading()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            CurrentEvidenceVerifier.Verify(
                VerificationCatalogFixture.RepositoryRoot,
                cancellation.Token));
    }

    private static void Mutate(JsonObject root, string mutation)
    {
        var current = root["current"]!.AsObject();
        var qualification = root["feature_qualification"]!.AsObject();
        switch (mutation)
        {
            case "root-extra":
                root["unexpected"] = true;
                break;
            case "contract-case":
                qualification["contract"] = "PCV-FEATURE-PROMOTION-DECISION-V1";
                break;
            case "blocker-extra":
                qualification["blockers"]!.AsArray()[0]!["unexpected"] = true;
                break;
            case "contradictory":
                qualification["promotion_eligible"] = true;
                break;
            case "version-case":
                current["version"] = "0.42.74-ADMIN-SMOKE";
                break;
            case "sha-case":
                current["clean_msi_sha256"] =
                    current["clean_msi_sha256"]!.GetValue<string>().ToUpperInvariant();
                break;
            case "missing-reference":
                current["installed_evidence"] =
                    "docs/ga-ready/evidence/does-not-exist.md";
                break;
            case "path-escape":
                current["installed_evidence"] = "../outside.md";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }
    }

    private static string Hash(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
}
