using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Api;

internal sealed record DesktopNodeFeatureQualificationBlocker(
    string FeatureId,
    string Stage,
    string Verdict);

internal sealed record DesktopNodeFeatureQualificationSnapshot(
    int SchemaVersion,
    string Contract,
    string Status,
    bool PromotionEligible,
    IReadOnlyList<DesktopNodeFeatureQualificationBlocker> Blockers,
    string? ErrorCode)
{
    public static DesktopNodeFeatureQualificationSnapshot Unavailable() => new(
        1,
        "pcv-feature-promotion-decision-v1",
        "unavailable",
        false,
        Array.Empty<DesktopNodeFeatureQualificationBlocker>(),
        "PCV_CURRENT_EVIDENCE_UNAVAILABLE");
}

internal static partial class DesktopNodeCurrentEvidenceProvider
{
    private static readonly HashSet<string> Stages =
    [
        "code_tested",
        "packaged",
        "installed_tested",
        "actual_vm_tested",
        "manual_admin_tested"
    ];

    private static readonly HashSet<string> BlockerVerdicts = ["fail", "blocked", "missing"];

    public static DesktopNodeFeatureQualificationSnapshot Load(string? path = null)
    {
        var resolvedPath = path ?? Path.Combine(
            AppContext.BaseDirectory,
            "evidence",
            "current-evidence.json");
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(resolvedPath));
            return Parse(document.RootElement);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            return DesktopNodeFeatureQualificationSnapshot.Unavailable();
        }
    }

    private static DesktopNodeFeatureQualificationSnapshot Parse(JsonElement root)
    {
        if (RequireInt32(root, "schema_version") != 1)
        {
            throw new InvalidDataException("schema_version");
        }
        RequireString(root, "contract", "pcv-current-evidence-v1");
        var current = RequireObject(root, "current");
        var version = RequireString(current, "version");
        if (!AdminSmokeVersion().IsMatch(version))
        {
            throw new InvalidDataException("current.version");
        }

        var qualification = RequireObject(root, "feature_qualification");
        var schemaVersion = RequireInt32(qualification, "schema_version");
        if (schemaVersion != 1)
        {
            throw new InvalidDataException("feature_qualification.schema_version");
        }
        var contract = RequireString(qualification, "contract", "pcv-feature-promotion-decision-v1");
        var promotionEligible = RequireBoolean(qualification, "promotion_eligible");
        var blockerRows = RequireArray(qualification, "blockers");
        var blockers = new List<DesktopNodeFeatureQualificationBlocker>();
        foreach (var row in blockerRows.EnumerateArray())
        {
            var featureId = RequireString(row, "feature_id");
            var stage = RequireString(row, "stage");
            var verdict = RequireString(row, "verdict");
            if (!FeatureId().IsMatch(featureId) || !Stages.Contains(stage) || !BlockerVerdicts.Contains(verdict))
            {
                throw new InvalidDataException("feature_qualification.blockers");
            }
            blockers.Add(new DesktopNodeFeatureQualificationBlocker(featureId, stage, verdict));
        }
        if (promotionEligible != (blockers.Count == 0))
        {
            throw new InvalidDataException("feature_qualification.invariant");
        }

        ReadOnlyCollection<DesktopNodeFeatureQualificationBlocker> immutable = blockers.AsReadOnly();
        return new DesktopNodeFeatureQualificationSnapshot(
            schemaVersion,
            contract,
            promotionEligible ? "eligible" : "blocked",
            promotionEligible,
            immutable,
            null);
    }

    private static JsonElement RequireObject(JsonElement element, string name)
    {
        var value = RequireProperty(element, name);
        return value.ValueKind == JsonValueKind.Object ? value : throw new InvalidDataException(name);
    }

    private static JsonElement RequireArray(JsonElement element, string name)
    {
        var value = RequireProperty(element, name);
        return value.ValueKind == JsonValueKind.Array ? value : throw new InvalidDataException(name);
    }

    private static string RequireString(JsonElement element, string name, string? expected = null)
    {
        var value = RequireProperty(element, name);
        var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (string.IsNullOrWhiteSpace(text) || (expected is not null && !string.Equals(text, expected, StringComparison.Ordinal)))
        {
            throw new InvalidDataException(name);
        }
        return text;
    }

    private static int RequireInt32(JsonElement element, string name)
    {
        var value = RequireProperty(element, name);
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidDataException(name);
        }
        return value.TryGetInt32(out var result) ? result : throw new InvalidDataException(name);
    }

    private static bool RequireBoolean(JsonElement element, string name)
    {
        return RequireProperty(element, name).ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new InvalidDataException(name)
        };
    }

    private static JsonElement RequireProperty(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value
            : throw new InvalidDataException(name);
    }

    [GeneratedRegex("^pcv\\.[a-z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex FeatureId();

    [GeneratedRegex("^0\\.\\d+\\.\\d+-admin-smoke$", RegexOptions.CultureInvariant)]
    private static partial Regex AdminSmokeVersion();
}
