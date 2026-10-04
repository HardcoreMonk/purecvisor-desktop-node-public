using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// Input of pcvverify train-facts (design pcv-train-pair-orchestrator-v1 3a): which pair documents to build,
// where the orchestrated pair, package, fullgate, and current-card artifacts are, and the human narrative values.
internal sealed record TrainFactsInput(
    string Version,
    string BaselineVersion,
    string CanonicalCurrent,
    string Date,
    IReadOnlyDictionary<string, string> Documents,
    IReadOnlyDictionary<string, string> Sources,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Narrative)
{
    internal const string Contract = "pcv-train-facts-input-v1";

    internal static readonly IReadOnlyList<string> BuildableTemplates =
    [
        "package", "ops-summary", "update-rollback", "clean-host", "burn", "msix",
        "pair-descriptor", "fullgate", "current-card"
    ];

    private static readonly HashSet<string> ArtifactSources = new(StringComparer.Ordinal)
    {
        "package_root", "baseline_package_root", "campaign_root", "target_update_catalog", "iso_path", "current_card_root"
    };

    private static readonly HashSet<string> ValueSources = new(StringComparer.Ordinal)
    {
        "fullgate_batch_id", "signer_thumbprint", "fullgate_final_firewall_rule_count"
    };

    private static readonly Regex VersionPattern = new("^[0-9]+\\.[0-9]+\\.[0-9]+-admin-smoke$", RegexOptions.CultureInvariant);
    private static readonly Regex DatePattern = new("^[0-9]{4}-[0-9]{2}-[0-9]{2}$", RegexOptions.CultureInvariant);
    private static readonly Regex ArtifactPattern = new("^artifacts/[A-Za-z0-9._-]+(/[A-Za-z0-9._-]+)*$", RegexOptions.CultureInvariant);
    private static readonly Regex DocumentPattern = new("^docs/ga-ready/evidence/[A-Za-z0-9._-]+\\.md$", RegexOptions.CultureInvariant);

    internal static TrainFactsInput Parse(string json)
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

        string[] properties = ["schema_version", "contract", "version", "baseline_version", "canonical_current_evidence", "date", "documents", "sources", "narrative"];
        if (root.Count != properties.Length || properties.Any(name => !root.ContainsKey(name)) ||
            root["schema_version"]?.ToJsonString() != "1" || Text(root["contract"]) != Contract)
        {
            throw Invalid("input-invalid", "contract");
        }

        var version = Text(root["version"]);
        var baseline = Text(root["baseline_version"]);
        var canonical = Text(root["canonical_current_evidence"]);
        var date = Text(root["date"]);
        if (!VersionPattern.IsMatch(version) || !VersionPattern.IsMatch(baseline) || !VersionPattern.IsMatch(canonical) || !DatePattern.IsMatch(date))
        {
            throw Invalid("input-invalid", "version-or-date");
        }

        var documents = Strings(root["documents"], "documents");
        foreach (var (template, path) in documents)
        {
            if (!BuildableTemplates.Contains(template, StringComparer.Ordinal))
            {
                throw Invalid("template-unknown", template);
            }

            if (!DocumentPattern.IsMatch(path))
            {
                throw Invalid("path-invalid", path);
            }
        }

        var sources = Strings(root["sources"], "sources");
        foreach (var (key, value) in sources)
        {
            if (ArtifactSources.Contains(key))
            {
                if (!ArtifactPattern.IsMatch(value) || value.Split('/').Contains(".."))
                {
                    throw Invalid("source-invalid", key);
                }
            }
            else if (!ValueSources.Contains(key))
            {
                throw Invalid("source-unknown", key);
            }
        }

        var narrative = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var (template, values) in (root["narrative"] as JsonObject ?? throw Invalid("input-invalid", "narrative")))
        {
            if (!documents.ContainsKey(template))
            {
                throw Invalid("narrative-unknown", template);
            }

            var parsed = Strings(values, "narrative." + template);
            foreach (var (key, value) in parsed)
            {
                if (!TrainEvidenceFactsReader.KeyPattern.IsMatch(key) || value.Contains('\r') || value.Contains('\n') ||
                    value.Contains("{{", StringComparison.Ordinal))
                {
                    throw Invalid("narrative-invalid", template + ":" + key);
                }
            }

            narrative[template] = parsed;
        }

        return new TrainFactsInput(version, baseline, canonical, date, documents, sources, narrative);
    }

    private static Dictionary<string, string> Strings(JsonNode? node, string subject)
    {
        if (node is not JsonObject values)
        {
            throw Invalid("input-invalid", subject);
        }

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in values)
        {
            result[key] = Text(value);
        }

        return result;
    }

    private static string Text(JsonNode? node)
    {
        if (node is JsonValue value && value.GetValueKind() == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(value.GetValue<string>()))
        {
            return value.GetValue<string>();
        }

        throw Invalid("input-invalid", "string");
    }

    internal static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"train-facts:{reason}:{subject}");
}

// Read-only JSON view over one artifact file with path-style accessors that fail closed.
internal sealed class TrainFactsJson(string relativePath, JsonNode root)
{
    internal string RelativePath { get; } = relativePath;

    internal static TrainFactsJson Load(string repositoryRoot, string relativePath)
    {
        var full = TrainFactsFiles.Full(repositoryRoot, relativePath);
        if (!File.Exists(full))
        {
            throw TrainFactsInput.Invalid("artifact-missing", relativePath);
        }

        try
        {
            return new TrainFactsJson(relativePath, JsonNode.Parse(File.ReadAllText(full)) ?? throw new JsonException());
        }
        catch (JsonException)
        {
            throw TrainFactsInput.Invalid("artifact-invalid", relativePath);
        }
    }

    internal JsonNode? Get(params string[] path)
    {
        JsonNode? current = root;
        foreach (var name in path)
        {
            current = current is JsonObject item && item.TryGetPropertyValue(name, out var next) ? next : null;
        }

        return current;
    }

    internal string Str(params string[] path) =>
        TrainFactsFiles.Scalar(Get(path)) ?? throw TrainFactsInput.Invalid("artifact-field-missing", RelativePath + ":" + string.Join('.', path));

    internal string? OptStr(params string[] path) => TrainFactsFiles.Scalar(Get(path));

    internal long Long(params string[] path) =>
        long.TryParse(Str(path), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw TrainFactsInput.Invalid("artifact-field-invalid", RelativePath + ":" + string.Join('.', path));

    internal bool Bool(params string[] path) =>
        Get(path) is JsonValue value && value.GetValueKind() is JsonValueKind.True or JsonValueKind.False
            ? value.GetValue<bool>()
            : throw TrainFactsInput.Invalid("artifact-field-invalid", RelativePath + ":" + string.Join('.', path));

    internal IReadOnlyList<JsonNode?> Array(params string[] path) => Get(path) switch
    {
        JsonArray items => items.ToList(),
        null => [],
        JsonNode single => [single]
    };
}

internal static class TrainFactsFiles
{
    internal static string Full(string repositoryRoot, string relativePath) =>
        Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

    internal static string Sha256(string repositoryRoot, string relativePath)
    {
        var full = Full(repositoryRoot, relativePath);
        if (!File.Exists(full))
        {
            throw TrainFactsInput.Invalid("artifact-missing", relativePath);
        }

        using var stream = File.OpenRead(full);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal static long Length(string repositoryRoot, string relativePath)
    {
        var full = Full(repositoryRoot, relativePath);
        return File.Exists(full) ? new FileInfo(full).Length : throw TrainFactsInput.Invalid("artifact-missing", relativePath);
    }

    internal static string? Scalar(JsonNode? node) => node is JsonValue value
        ? value.GetValueKind() switch
        {
            JsonValueKind.String => value.GetValue<string>(),
            JsonValueKind.Number => value.ToJsonString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        }
        : null;
}
