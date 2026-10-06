using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// Input of pcvverify train-host-inputs (design pcv-train-host-inputs-v1): the train version and date, the train facts
// file and the smoke ISO. Private run values (repository root, LAN prefix, operator) are passed at render time and are
// never committed.
internal sealed record TrainHostInputs(string Version, string Date, string Facts, string Iso)
{
    internal const string Contract = "pcv-train-host-inputs-v1";
    internal const string CurrentCardTemplate = "docs/ga-ready/trains/host-templates/capture-current-card.ps1.tmpl";

    private static readonly Regex VersionPattern = new(
        "^[0-9]+\\.[0-9]+\\.[0-9]+-admin-smoke$", RegexOptions.CultureInvariant);

    private static readonly Regex IsoPattern = new("^artifacts/[A-Za-z0-9._/-]+\\.iso$", RegexOptions.CultureInvariant);

    private static readonly Regex PlaceholderPattern = new("\\{\\{([a-z0-9_]+)\\}\\}", RegexOptions.CultureInvariant);

    private static readonly Regex Sha256Pattern = new("^[0-9a-f]{64}$", RegexOptions.CultureInvariant);

    private static readonly Regex CommitPattern = new("^[0-9a-f]{40}$", RegexOptions.CultureInvariant);

    private static readonly Regex ArtifactRootPattern = new("^artifacts/[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant);

    private static readonly Regex FullgateBatchPattern = new(
        "^full-admin-host-mutation-gate-[0-9]{8}-[0-9]{5}$", RegexOptions.CultureInvariant);

    // 0.42.91-admin-smoke -> 04291
    internal string VersionTag => TrainLane3SpecInput.TagOf(Version);

    // 2026-10-06 and 0.42.91-admin-smoke -> 20261006-04291, the suffix of every artifact root of the train.
    internal string RunTag => Date.Replace("-", "", StringComparison.Ordinal) + "-" + VersionTag;

    internal string CurrentCardOutput => $"artifacts/installed-operator-surface-current-card-{RunTag}.capture.ps1";

    internal static TrainHostInputs Parse(string json)
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

        string[] names = ["schema_version", "contract", "version", "date", "facts", "iso"];
        if (root.Count != names.Length || names.Any(name => !root.ContainsKey(name)))
        {
            throw Invalid("input-invalid", "properties");
        }

        if (root["schema_version"]?.ToJsonString() != "1" || Text(root, "contract") != Contract)
        {
            throw Invalid("input-invalid", "contract");
        }

        var version = Text(root, "version");
        if (!VersionPattern.IsMatch(version))
        {
            throw Invalid("input-invalid", "version");
        }

        var date = Text(root, "date");
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            throw Invalid("input-invalid", "date");
        }

        var facts = Text(root, "facts");
        if (facts != $"docs/ga-ready/trains/{version}.evidence-facts.json")
        {
            throw Invalid("input-invalid", "facts");
        }

        var iso = Text(root, "iso");
        if (!IsoPattern.IsMatch(iso) || iso.Contains("..", StringComparison.Ordinal))
        {
            throw Invalid("input-invalid", "iso");
        }

        return new TrainHostInputs(version, date, facts, iso);
    }

    // repositoryRoot locates the template and the facts; repositoryRootValue is what the script runs from.
    internal string RenderCurrentCard(string repositoryRoot, string repositoryRootValue)
    {
        var documents = FactDocuments(repositoryRoot);
        var package = Values(documents, "package");
        var fullgate = Values(documents, "fullgate");
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["repo_root"] = repositoryRootValue,
            ["card_root"] = WindowsPath($"artifacts/installed-operator-surface-current-card-{RunTag}"),
            ["routeparity_payload"] = WindowsPath(Fact(fullgate, "fullgate", "routeparity_artifact_root", ArtifactRootPattern) + "/payload"),
            ["package_payload"] = WindowsPath(Fact(package, "package", "artifact_root", ArtifactRootPattern) + "/payload"),
            ["evidence_id"] = $"installed-operator-surface-current-card-{Date}-{VersionTag}",
            ["version"] = Version,
            ["fullgate_batch"] = Fact(fullgate, "fullgate", "batch_id", FullgateBatchPattern),
            ["clean_package_msi_sha256"] = Fact(package, "package", "clean_package_msi_sha256", Sha256Pattern),
            ["operational_fullgate_msi_sha256"] = Fact(fullgate, "fullgate", "operational_fullgate_msi_sha256", Sha256Pattern),
            ["clean_package_payload_aggregate_sha256"] =
                Fact(package, "package", "clean_package_payload_aggregate_sha256", Sha256Pattern),
            ["operational_fullgate_payload_aggregate_sha256"] =
                Fact(fullgate, "fullgate", "operational_fullgate_payload_aggregate_sha256", Sha256Pattern),
            ["provenance_commit"] = Fact(fullgate, "fullgate", "provenance_commit", CommitPattern),
            // The package facts carry current-evidence.json as it was when the train departed.
            ["canonical_current_evidence"] = Fact(package, "package", "canonical_current_evidence", VersionPattern)
        };

        return Render(ReadTemplate(repositoryRoot, CurrentCardTemplate), values);
    }

    internal static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"train-host-inputs:{reason}:{subject}");

    private JsonArray FactDocuments(string repositoryRoot)
    {
        var path = Path.Combine(repositoryRoot, Facts.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            throw Invalid("facts-missing", Facts);
        }

        var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8)) as JsonObject ?? throw Invalid("facts-invalid", "root");
        if (!(root["version"] is JsonValue version && version.TryGetValue<string>(out var text) && text == Version))
        {
            throw Invalid("facts-invalid", "version");
        }

        return root["documents"] as JsonArray ?? throw Invalid("facts-invalid", "documents");
    }

    private static JsonObject Values(JsonArray documents, string template) =>
        documents
            .OfType<JsonObject>()
            .FirstOrDefault(document => document["template"] is JsonValue value &&
                value.TryGetValue<string>(out var name) && name == template)?["values"] as JsonObject
        ?? throw Invalid("fact-missing", template);

    private static string Fact(JsonObject values, string template, string key, Regex pattern)
    {
        var value = values[key] is JsonValue node && node.TryGetValue<string>(out var text) ? text : null;
        if (string.IsNullOrEmpty(value))
        {
            throw Invalid("fact-missing", template + "." + key);
        }

        return pattern.IsMatch(value) ? value : throw Invalid("fact-invalid", template + "." + key);
    }

    private static string ReadTemplate(string repositoryRoot, string template)
    {
        var path = Path.Combine(repositoryRoot, template.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path)
            ? File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal)
            : throw Invalid("template-missing", template);
    }

    private static string Render(string template, IReadOnlyDictionary<string, string> values)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        var text = PlaceholderPattern.Replace(template, match =>
        {
            var name = match.Groups[1].Value;
            if (!values.TryGetValue(name, out var value))
            {
                throw Invalid("template-placeholder-unknown", name);
            }

            used.Add(name);
            return value;
        });
        var unused = values.Keys.FirstOrDefault(name => !used.Contains(name));
        return unused is null ? text : throw Invalid("template-placeholder-unused", unused);
    }

    private static string WindowsPath(string path) => path.Replace('/', '\\');

    private static string Text(JsonObject root, string name) =>
        root[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : throw Invalid("input-invalid", name);
}
