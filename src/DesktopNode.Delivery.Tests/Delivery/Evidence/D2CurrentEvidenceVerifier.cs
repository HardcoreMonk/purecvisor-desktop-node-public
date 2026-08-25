using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

internal sealed record D2CurrentEvidenceRecord(
    int SchemaVersion,
    string Contract,
    D2CurrentOperationalEvidence Current,
    D2FeatureQualification FeatureQualification,
    D2ManualAdminEvidence ManualAdmin,
    D2EvidenceClaims Claims);

internal sealed record D2CurrentOperationalEvidence(
    string Version,
    IReadOnlyList<string> OperatorSurfaces,
    bool TuiPresent,
    string PackageEvidence,
    string FullgateBatch,
    string FullgateEvidence,
    string FunctionalEvidence,
    string InstalledEvidence,
    string CleanMsiSha256,
    string OperationalMsiSha256,
    string PayloadSha256,
    string ProvenanceCommit);

internal sealed record D2FeatureQualification(
    int SchemaVersion,
    string Contract,
    bool PromotionEligible,
    IReadOnlyList<D2FeatureBlocker> Blockers);

internal sealed record D2FeatureBlocker(string FeatureId, string Stage, string Verdict);

internal sealed record D2ManualAdminEvidence(
    string LatestClosedBaseline,
    string LatestClosedTarget,
    string LatestClosedDescriptor,
    string? BlockedBaseline,
    string? BlockedTarget,
    string? BlockedReason);

internal sealed record D2EvidenceClaims(
    bool PublicTrustedSigning,
    bool ExternalStablePublication);

internal static class D2CurrentEvidenceVerifier
{
    internal const string RecordPath = "docs/ga-ready/current-evidence.json";

    private const string Owner = "d2-current-evidence";
    private const string BeginMarker = "<!-- BEGIN GENERATED CURRENT EVIDENCE -->";
    private const string EndMarker = "<!-- END GENERATED CURRENT EVIDENCE -->";

    private static readonly Regex VersionPattern =
        new("^0\\.\\d+\\.\\d+-admin-smoke$", RegexOptions.CultureInvariant);

    private static readonly Regex Sha256Pattern =
        new("^[0-9a-f]{64}$", RegexOptions.CultureInvariant);

    private static readonly Regex CommitPattern =
        new("^[0-9a-f]{40}$", RegexOptions.CultureInvariant);

    private static readonly Regex EvidencePathPattern =
        new("^docs/ga-ready/evidence/.+\\.md$", RegexOptions.CultureInvariant);

    private static readonly Regex FeatureIdPattern =
        new("^pcv\\.[a-z0-9._-]+$", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> FeatureStages = new(StringComparer.Ordinal)
    {
        "code_tested",
        "packaged",
        "installed_tested",
        "actual_vm_tested",
        "manual_admin_tested",
    };

    private static readonly HashSet<string> BlockerVerdicts = new(StringComparer.Ordinal)
    {
        "fail",
        "blocked",
        "missing",
    };

    internal static IReadOnlyList<string> OwnedRelativePaths { get; } = Array.AsReadOnly([
        "README.md",
        "AGENTS.md",
        "docs/DEVELOPER_INDEX.md",
        "docs/ga-ready/EVIDENCE_INDEX.md",
        "docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md",
        "docs/ga-ready/CONTROL_PLANE_INDEX.md",
        "docs/DEVELOPMENT_VERIFICATION_POLICY.md",
        "packaging/windows-desktop-node/README.md",
    ]);

    internal static D2CurrentEvidenceRecord Validate(
        string source,
        RepositoryContractContext repository)
    {
        try
        {
            using var json = JsonContract.Parse(Owner, source);
            var root = json.Root;
            RequireExactProperties(
                root,
                "schema_version",
                "contract",
                "current",
                "feature_qualification",
                "manual_admin",
                "claims");

            var schemaVersion = json.RequireInteger(root, "schema_version");
            Require(schemaVersion == 1, "schema-version");
            var contract = json.RequireString(root, "contract");
            Require(contract == "pcv-current-evidence-v1", "contract");

            var current = ParseCurrent(json, root, repository);
            var qualification = ParseQualification(json, root);
            var manualAdmin = ParseManualAdmin(json, root);
            var claims = ParseClaims(json, root);
            return new D2CurrentEvidenceRecord(
                schemaVersion,
                contract,
                current,
                qualification,
                manualAdmin,
                claims);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception error) when (error is InvalidOperationException or FormatException)
        {
            throw DeliveryContractError.Invalid(Owner, "json-type", error);
        }
    }

    internal static void AssertPromotionAllowed(
        D2CurrentEvidenceRecord proposed,
        D2CurrentEvidenceRecord canonical)
    {
        if (proposed.Current.Version != canonical.Current.Version &&
            !proposed.FeatureQualification.PromotionEligible)
        {
            throw new InvalidDataException(
                "PCV_FEATURE_PROMOTION_BLOCKED|" + proposed.Current.Version +
                $"|blockers={proposed.FeatureQualification.Blockers.Count}");
        }
    }

    internal static string Render(D2CurrentEvidenceRecord record)
    {
        var blockers = record.FeatureQualification.Blockers.Count == 0
            ? "none"
            : string.Join(
                ',',
                record.FeatureQualification.Blockers.Select(blocker =>
                    $"{blocker.FeatureId}/{blocker.Stage}/{blocker.Verdict}"));
        var lines = new List<string>
        {
            BeginMarker,
            "## Current operational evidence (generated)",
            string.Empty,
            $"- Version: `{record.Current.Version}`",
            "- Active operator surfaces: Web Console and PCVCLI; " +
                $"`tui_present={Lower(record.Current.TuiPresent)}`.",
            $"- Package evidence: `{record.Current.PackageEvidence}`.",
            "- Full admin host mutation: " +
                $"`{record.Current.FullgateBatch}` / `{record.Current.FullgateEvidence}`.",
            $"- Actual-VM functional evidence: `{record.Current.FunctionalEvidence}`.",
            "- Feature qualification: " +
                $"`contract={record.FeatureQualification.Contract}`; " +
                $"`promotion_eligible={Lower(record.FeatureQualification.PromotionEligible)}`; " +
                $"`blocker_count={record.FeatureQualification.Blockers.Count}`; " +
                $"`blockers={blockers}`.",
            "- Installed CLI/Web current-card: " +
                $"`{record.Current.InstalledEvidence}`; CLI exit 0, Web HTTP 200, " +
                "service Running/Automatic, TUI absent.",
            $"- Clean MSI SHA-256: `{record.Current.CleanMsiSha256}`.",
            $"- Operational MSI SHA-256: `{record.Current.OperationalMsiSha256}`.",
            "- Operational payload aggregate SHA-256: " +
                $"`{record.Current.PayloadSha256}`.",
            $"- Provenance commit: `{record.Current.ProvenanceCommit}`.",
            "- Latest closed manual-admin pair: " +
                $"`{record.ManualAdmin.LatestClosedBaseline} -> " +
                $"{record.ManualAdmin.LatestClosedTarget}` / " +
                $"`{record.ManualAdmin.LatestClosedDescriptor}`.",
        };
        if (record.ManualAdmin.BlockedReason is not null)
        {
            lines.Add(
                "- Blocked follow-up: " +
                $"`{record.ManualAdmin.BlockedBaseline} -> {record.ManualAdmin.BlockedTarget}` / " +
                $"`{record.ManualAdmin.BlockedReason}`.");
        }

        lines.Add(
            "- Claims: " +
            $"`public_trusted_signing={Lower(record.Claims.PublicTrustedSigning)}`; " +
            $"`external_stable_publication={Lower(record.Claims.ExternalStablePublication)}`.");
        lines.Add(EndMarker);
        return string.Join('\n', lines);
    }

    internal static void VerifyDocument(string owner, string source, string expectedBlock)
    {
        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal);
        Require(Count(normalized, BeginMarker) == 1, $"{owner}-begin-marker");
        Require(Count(normalized, EndMarker) == 1, $"{owner}-end-marker");
        var begin = normalized.IndexOf(BeginMarker, StringComparison.Ordinal);
        var end = normalized.IndexOf(EndMarker, StringComparison.Ordinal);
        Require(begin >= 0 && end > begin, $"{owner}-marker-order");
        var actual = normalized.Substring(begin, end + EndMarker.Length - begin);
        Require(actual == expectedBlock, $"{owner}-stale");
    }

    internal static int VerifyOwnedDocuments(
        D2CurrentEvidenceRecord record,
        RepositoryContractContext repository)
    {
        var block = Render(record);
        foreach (var path in OwnedRelativePaths)
        {
            VerifyDocument(path, repository.ReadUtf8Text(path), block);
        }

        return OwnedRelativePaths.Count;
    }

    private static D2CurrentOperationalEvidence ParseCurrent(
        JsonContract json,
        JsonElement root,
        RepositoryContractContext repository)
    {
        var current = json.RequireObject(root, "current");
        RequireExactProperties(
            current,
            "version",
            "operator_surfaces",
            "tui_present",
            "package_evidence",
            "fullgate_batch",
            "fullgate_evidence",
            "functional_evidence",
            "installed_evidence",
            "clean_msi_sha256",
            "operational_msi_sha256",
            "payload_sha256",
            "provenance_commit");
        var version = json.RequireString(current, "version");
        Require(VersionPattern.IsMatch(version), "version");
        var surfaces = json.RequireArray(current, "operator_surfaces")
            .EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String
                ? item.GetString()!
                : throw DeliveryContractError.Invalid(Owner, "operator-surfaces"))
            .ToArray();
        Require(surfaces.SequenceEqual(["web", "cli"], StringComparer.Ordinal), "operator-surfaces");
        var tui = json.RequireBoolean(current, "tui_present");
        Require(!tui, "tui-present");
        var packageEvidence = json.RequireString(current, "package_evidence");
        var fullgateEvidence = json.RequireString(current, "fullgate_evidence");
        var functionalEvidence = json.RequireString(current, "functional_evidence");
        var installedEvidence = json.RequireString(current, "installed_evidence");
        foreach (var path in new[]
        {
            packageEvidence,
            fullgateEvidence,
            functionalEvidence,
            installedEvidence,
        })
        {
            Require(EvidencePathPattern.IsMatch(path), "evidence-path");
            string evidence;
            try
            {
                evidence = repository.ReadUtf8Text(path);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                throw DeliveryContractError.Invalid(Owner, "missing-evidence", error);
            }

            Require(evidence.Contains(version, StringComparison.OrdinalIgnoreCase), "evidence-version");
        }

        var clean = json.RequireString(current, "clean_msi_sha256");
        var operational = json.RequireString(current, "operational_msi_sha256");
        var payload = json.RequireString(current, "payload_sha256");
        Require(Sha256Pattern.IsMatch(clean), "clean-sha256");
        Require(Sha256Pattern.IsMatch(operational), "operational-sha256");
        Require(Sha256Pattern.IsMatch(payload), "payload-sha256");
        var commit = json.RequireString(current, "provenance_commit");
        Require(CommitPattern.IsMatch(commit), "provenance-commit");

        return new D2CurrentOperationalEvidence(
            version,
            Array.AsReadOnly(surfaces),
            tui,
            packageEvidence,
            json.RequireString(current, "fullgate_batch"),
            fullgateEvidence,
            functionalEvidence,
            installedEvidence,
            clean,
            operational,
            payload,
            commit);
    }

    private static D2FeatureQualification ParseQualification(JsonContract json, JsonElement root)
    {
        var qualification = json.RequireObject(root, "feature_qualification");
        RequireExactProperties(
            qualification,
            "schema_version",
            "contract",
            "promotion_eligible",
            "blockers");
        var schemaVersion = json.RequireInteger(qualification, "schema_version");
        Require(schemaVersion == 1, "qualification-schema-version");
        var contract = json.RequireString(qualification, "contract");
        Require(contract == "pcv-feature-promotion-decision-v1", "qualification-contract");
        var eligible = json.RequireBoolean(qualification, "promotion_eligible");
        var blockers = new List<D2FeatureBlocker>();
        foreach (var item in json.RequireArray(qualification, "blockers").EnumerateArray())
        {
            RequireExactProperties(item, "feature_id", "stage", "verdict");
            var featureId = json.RequireString(item, "feature_id");
            var stage = json.RequireString(item, "stage");
            var verdict = json.RequireString(item, "verdict");
            Require(FeatureIdPattern.IsMatch(featureId), "feature-id");
            Require(FeatureStages.Contains(stage), "feature-stage");
            Require(BlockerVerdicts.Contains(verdict), "feature-verdict");
            blockers.Add(new D2FeatureBlocker(featureId, stage, verdict));
        }

        Require(eligible ? blockers.Count == 0 : blockers.Count > 0, "qualification-contradiction");
        return new D2FeatureQualification(
            schemaVersion,
            contract,
            eligible,
            Array.AsReadOnly(blockers.ToArray()));
    }

    private static D2ManualAdminEvidence ParseManualAdmin(JsonContract json, JsonElement root)
    {
        var manual = json.RequireObject(root, "manual_admin");
        var names = manual.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        var required = new[]
        {
            "latest_closed_baseline",
            "latest_closed_target",
            "latest_closed_descriptor",
        };
        var blocked = new[] { "blocked_baseline", "blocked_target", "blocked_reason" };
        Require(required.All(names.Contains), "manual-required");
        Require(names.Count == required.Length ||
            names.Count == required.Length + blocked.Length && blocked.All(names.Contains), "manual-properties");
        var baseline = RequireNonEmpty(json.RequireString(manual, required[0]), required[0]);
        var target = RequireNonEmpty(json.RequireString(manual, required[1]), required[1]);
        var descriptor = RequireNonEmpty(json.RequireString(manual, required[2]), required[2]);
        return new D2ManualAdminEvidence(
            baseline,
            target,
            descriptor,
            names.Contains(blocked[0]) ? RequireNonEmpty(json.RequireString(manual, blocked[0]), blocked[0]) : null,
            names.Contains(blocked[1]) ? RequireNonEmpty(json.RequireString(manual, blocked[1]), blocked[1]) : null,
            names.Contains(blocked[2]) ? RequireNonEmpty(json.RequireString(manual, blocked[2]), blocked[2]) : null);
    }

    private static D2EvidenceClaims ParseClaims(JsonContract json, JsonElement root)
    {
        var claims = json.RequireObject(root, "claims");
        RequireExactProperties(claims, "public_trusted_signing", "external_stable_publication");
        var signing = json.RequireBoolean(claims, "public_trusted_signing");
        var publication = json.RequireBoolean(claims, "external_stable_publication");
        Require(!signing && !publication, "claims");
        return new D2EvidenceClaims(signing, publication);
    }

    private static void RequireExactProperties(JsonElement value, params string[] expected)
    {
        Require(value.ValueKind == JsonValueKind.Object, "object");
        var actual = value.EnumerateObject().Select(property => property.Name).ToArray();
        Require(actual.Length == expected.Length, "properties");
        Require(actual.ToHashSet(StringComparer.Ordinal).SetEquals(expected), "properties");
    }

    private static string RequireNonEmpty(string value, string field)
    {
        Require(!string.IsNullOrWhiteSpace(value), field);
        return value;
    }

    private static int Count(string source, string value)
    {
        var count = 0;
        for (var index = 0;
             (index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0;
             index += value.Length)
        {
            count++;
        }

        return count;
    }

    private static string Lower(bool value) => value ? "true" : "false";

    private static void Require(bool condition, string detail)
    {
        if (!condition)
        {
            throw DeliveryContractError.Invalid(Owner, detail);
        }
    }
}
