using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// Release train evidence = fixed template + per-train facts (design pcv-train-evidence-render-v1).
internal sealed record TrainEvidenceDocument(
    string Template,
    string Path,
    IReadOnlyDictionary<string, string> Values);

internal sealed record TrainEvidenceFacts(string Version, IReadOnlyList<TrainEvidenceDocument> Documents);

internal static class TrainEvidenceFactsReader
{
    internal const string Contract = "pcv-train-evidence-facts-v1";

    internal static readonly IReadOnlyList<string> Templates =
    [
        "package", "ops-summary", "update-rollback", "clean-host", "burn", "msix",
        "pair-descriptor", "fullgate", "current-card",
        "functional-carryforward", "pair-consume", "main-push",
        // Single-PR trains (pcv-single-pr-train-v1) cite the payload commit main push run instead of a post-merge one.
        "main-push-payload"
    ];

    private static readonly Regex VersionPattern = new(
        "^[0-9]+\\.[0-9]+\\.[0-9]+-admin-smoke$", RegexOptions.CultureInvariant);

    private static readonly Regex PathPattern = new(
        "^docs/ga-ready/evidence/[A-Za-z0-9._-]+\\.md$", RegexOptions.CultureInvariant);

    internal static readonly Regex KeyPattern = new("^[a-z0-9_]+$", RegexOptions.CultureInvariant);

    internal static TrainEvidenceFacts Parse(string json)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw Invalid("facts-invalid", "json");
        }

        using (document)
        {
            var root = document.RootElement;
            RequireProperties(root, "facts", "schema_version", "contract", "version", "documents");
            if (root.GetProperty("schema_version").ValueKind != JsonValueKind.Number ||
                root.GetProperty("schema_version").GetRawText() != "1" ||
                ReadString(root, "contract", "facts") != Contract)
            {
                throw Invalid("facts-invalid", "contract");
            }

            var version = ReadString(root, "version", "facts");
            if (!VersionPattern.IsMatch(version))
            {
                throw Invalid("facts-invalid", "version");
            }

            var documents = root.GetProperty("documents");
            if (documents.ValueKind != JsonValueKind.Array || documents.GetArrayLength() == 0)
            {
                throw Invalid("facts-invalid", "documents");
            }

            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parsed = new List<TrainEvidenceDocument>();
            foreach (var item in documents.EnumerateArray())
            {
                RequireProperties(item, "document", "template", "path", "values");
                var template = ReadString(item, "template", "document");
                if (!Templates.Contains(template, StringComparer.Ordinal))
                {
                    throw Invalid("template-unknown", template);
                }

                var path = ReadString(item, "path", "document");
                if (!PathPattern.IsMatch(path))
                {
                    throw Invalid("path-invalid", path);
                }

                if (!paths.Add(path))
                {
                    throw Invalid("path-duplicate", path);
                }

                parsed.Add(new TrainEvidenceDocument(template, path, ReadValues(item.GetProperty("values"), path)));
            }

            return new TrainEvidenceFacts(version, parsed);
        }
    }

    private static Dictionary<string, string> ReadValues(JsonElement values, string path)
    {
        if (values.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("facts-invalid", path + ":values");
        }

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in values.EnumerateObject())
        {
            if (!KeyPattern.IsMatch(property.Name) || property.Value.ValueKind != JsonValueKind.String)
            {
                throw Invalid("value-invalid", path + ":" + property.Name);
            }

            var value = property.Value.GetString()!;
            if (value.Contains('\r') || value.Contains('\n') || value.Contains("{{", StringComparison.Ordinal))
            {
                throw Invalid("value-invalid", path + ":" + property.Name);
            }

            if (!result.TryAdd(property.Name, value))
            {
                throw Invalid("value-invalid", path + ":" + property.Name);
            }
        }

        return result;
    }

    private static void RequireProperties(JsonElement element, string subject, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("facts-invalid", subject);
        }

        var seen = element.EnumerateObject().Select(property => property.Name).ToList();
        if (seen.Count != names.Length || names.Any(name => !seen.Contains(name, StringComparer.Ordinal)))
        {
            throw Invalid("facts-invalid", subject + ":properties");
        }
    }

    private static string ReadString(JsonElement element, string name, string subject)
    {
        var value = element.GetProperty(name);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw Invalid("facts-invalid", subject + ":" + name);
        }

        return value.GetString()!;
    }

    internal static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"train-evidence:{reason}:{subject}");
}

internal static class TrainEvidenceTemplate
{
    private const string OptionalPrefix = "{{?";

    private static readonly Regex Placeholder = new("\\{\\{([a-z0-9_]+)\\}\\}", RegexOptions.CultureInvariant);

    // {{key}} is replaced; a line that starts with {{?key}} is kept only when key has a non-empty value.
    internal static string Render(string template, IReadOnlyDictionary<string, string> values, string documentPath)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        var output = new StringBuilder(template.Length);
        foreach (var line in SplitKeepingNewlines(template))
        {
            var current = line;
            if (current.StartsWith(OptionalPrefix, StringComparison.Ordinal))
            {
                var end = current.IndexOf("}}", OptionalPrefix.Length, StringComparison.Ordinal);
                var key = end < 0 ? string.Empty : current[OptionalPrefix.Length..end];
                if (!TrainEvidenceFactsReader.KeyPattern.IsMatch(key))
                {
                    throw TrainEvidenceFactsReader.Invalid("template-invalid", documentPath);
                }

                used.Add(key);
                if (!values.TryGetValue(key, out var flag) || flag.Length == 0)
                {
                    continue;
                }

                current = current[(end + 2)..];
            }

            var rendered = Placeholder.Replace(current, match =>
            {
                var key = match.Groups[1].Value;
                if (!values.TryGetValue(key, out var value) || value.Length == 0)
                {
                    throw TrainEvidenceFactsReader.Invalid("missing-value", documentPath + ":" + key);
                }

                used.Add(key);
                return value;
            });

            if (rendered.Contains("{{", StringComparison.Ordinal))
            {
                throw TrainEvidenceFactsReader.Invalid("template-invalid", documentPath);
            }

            output.Append(rendered);
        }

        var unused = values.Keys.Where(key => !used.Contains(key)).Order(StringComparer.Ordinal).FirstOrDefault();
        if (unused is not null)
        {
            throw TrainEvidenceFactsReader.Invalid("unused-value", documentPath + ":" + unused);
        }

        return output.ToString();
    }

    private static IEnumerable<string> SplitKeepingNewlines(string text)
    {
        var start = 0;
        while (start < text.Length)
        {
            var newline = text.IndexOf('\n', start);
            var end = newline < 0 ? text.Length : newline + 1;
            yield return text[start..end];
            start = end;
        }
    }
}
