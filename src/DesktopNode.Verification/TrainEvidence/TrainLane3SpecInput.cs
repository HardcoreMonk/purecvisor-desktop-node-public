using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// Input of pcvverify lane3-spec (design pcv-train-lane3-spec-generator-v1): the previous Lane 3 promotion spec, the
// train facts file, the spec to write, and the human-judged values that the facts cannot carry.
internal sealed record TrainLane3SpecInput(
    string Version,
    string PreviousSpec,
    string Facts,
    string Output,
    string UpdatedAt,
    string FunctionalNote,
    IReadOnlyDictionary<string, string> Rules)
{
    internal const string Contract = "pcv-train-lane3-spec-input-v1";

    internal static readonly IReadOnlyList<string> RuleIds =
    [
        "full-admin-host-mutation-current", "manual-admin-package-pair-current", "package-build-current",
        "installed-operator-surface-smoke-latest", "latest-product-payload-smoke",
        "functional-correctness-actual-host-latest", "manual-admin-package-pair-next"
    ];

    private static readonly Regex VersionPattern = new(
        "^([0-9]+)\\.([0-9]+)\\.([0-9]+)-admin-smoke$", RegexOptions.CultureInvariant);

    private static readonly Regex SpecPattern = new(
        "^packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-([0-9]{5})\\.json$",
        RegexOptions.CultureInvariant);

    private static readonly Regex TimestampPattern = new(
        "^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}[+-][0-9]{2}:[0-9]{2}$", RegexOptions.CultureInvariant);

    internal string Tag => TagOf(Version);

    internal string PreviousTag => SpecPattern.Match(PreviousSpec).Groups[1].Value;

    // 0.42.89-admin-smoke -> 04289: major, then minor and patch as two digits.
    internal static string TagOf(string version)
    {
        var match = VersionPattern.Match(version);
        if (!match.Success)
        {
            throw Invalid("version-invalid", version);
        }

        return match.Groups[1].Value +
            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture).ToString("00", CultureInfo.InvariantCulture) +
            int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture).ToString("00", CultureInfo.InvariantCulture);
    }

    internal static TrainLane3SpecInput Parse(string json)
    {
        JsonObject root;
        try
        {
            root = JsonNode.Parse(json) as JsonObject ?? throw Invalid("input-invalid", "root");
        }
        catch (JsonException)
        {
            throw Invalid("input-invalid", "json");
        }

        RequireExactly(root, "input", "schema_version", "contract", "version", "previous_spec", "facts", "output", "narrative");
        if (root["schema_version"]?.ToJsonString() != "1" || Text(root["contract"], "contract") != Contract)
        {
            throw Invalid("input-invalid", "contract");
        }

        var version = Text(root["version"], "version");
        var tag = TagOf(version);
        var previousSpec = Text(root["previous_spec"], "previous_spec");
        var output = Text(root["output"], "output");
        var facts = Text(root["facts"], "facts");
        var previousMatch = SpecPattern.Match(previousSpec);
        var outputMatch = SpecPattern.Match(output);
        if (!previousMatch.Success || !outputMatch.Success)
        {
            throw Invalid("path-invalid", previousMatch.Success ? output : previousSpec);
        }

        if (outputMatch.Groups[1].Value != tag || string.CompareOrdinal(previousMatch.Groups[1].Value, tag) >= 0)
        {
            throw Invalid("tag-mismatch", output);
        }

        if (facts != $"docs/ga-ready/trains/{version}.evidence-facts.json")
        {
            throw Invalid("path-invalid", facts);
        }

        var narrative = root["narrative"] as JsonObject ?? throw Invalid("input-invalid", "narrative");
        RequireExactly(narrative, "narrative", "updated_at", "functional_note", "rules");
        var updatedAt = Line(narrative["updated_at"], "updated_at");
        if (!TimestampPattern.IsMatch(updatedAt))
        {
            throw Invalid("narrative-invalid", "updated_at");
        }

        var rulesNode = narrative["rules"] as JsonObject ?? throw Invalid("narrative-invalid", "rules");
        foreach (var (id, _) in rulesNode)
        {
            if (!RuleIds.Contains(id, StringComparer.Ordinal))
            {
                throw Invalid("narrative-conflict", "rules." + id);
            }
        }

        var rules = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var id in RuleIds)
        {
            rules[id] = rulesNode.ContainsKey(id)
                ? Line(rulesNode[id], "rules." + id)
                : throw Invalid("narrative-missing", "rules." + id);
        }

        return new TrainLane3SpecInput(
            version, previousSpec, facts, output, updatedAt, Line(narrative["functional_note"], "functional_note"), rules);
    }

    internal static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"lane3-spec:{reason}:{subject}");

    private static void RequireExactly(JsonObject node, string subject, params string[] properties)
    {
        if (node.Count != properties.Length || properties.Any(name => !node.ContainsKey(name)))
        {
            throw Invalid("input-invalid", subject);
        }
    }

    private static string Text(JsonNode? node, string subject) =>
        node is JsonValue value && value.GetValueKind() == JsonValueKind.String && value.GetValue<string>().Length > 0
            ? value.GetValue<string>()
            : throw Invalid("input-invalid", subject);

    private static string Line(JsonNode? node, string subject)
    {
        var text = node is JsonValue value && value.GetValueKind() == JsonValueKind.String
            ? value.GetValue<string>()
            : throw Invalid("narrative-missing", subject);
        if (text.Trim().Length == 0 || text.Contains('\r') || text.Contains('\n'))
        {
            throw Invalid("narrative-invalid", subject);
        }

        return text;
    }
}
