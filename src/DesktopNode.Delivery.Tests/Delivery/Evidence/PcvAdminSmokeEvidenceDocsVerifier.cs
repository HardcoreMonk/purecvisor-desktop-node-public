using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

internal sealed class PcvAdminSmokeEvidenceDocsVerifier
{
    internal const string SpecPath =
        "config/pcv-admin-smoke-evidence-docs-contract-spec-v1.json";
    internal const string LegacyPath =
        "packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1";

    private const string ExpectedSpecSha256 =
        "8156454e45733c91f08ed8071b2bad1effac8ffb6edc36375e10af8e54512010";
    private const string ExpectedLegacySha256 =
        "91c580d11875c79a28ff86c7daceba275a231d0cb31483500d25181d325b63c9";
    private const string CurrentEvidencePath = "docs/ga-ready/current-evidence.json";
    private const string GeneratedBlockPattern =
        "(?s)<!-- BEGIN GENERATED CURRENT EVIDENCE -->.*?<!-- END GENERATED CURRENT EVIDENCE -->";

    private static readonly string[] CurrentEvidenceTargets =
    [
        "README.md",
        "AGENTS.md",
        "docs/ga-ready/EVIDENCE_INDEX.md",
        "docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md",
        "docs/ga-ready/CONTROL_PLANE_INDEX.md",
        "docs/DEVELOPMENT_VERIFICATION_POLICY.md",
        "packaging/windows-desktop-node/README.md",
    ];

    private readonly EvidenceDocsSource source;
    private readonly SpecModel spec;
    private readonly Regex[] patterns;
    private readonly Lazy<bool> legacyBinding;

    private PcvAdminSmokeEvidenceDocsVerifier(
        RepositoryContractContext repository,
        IReadOnlyDictionary<string, string?>? overrides)
    {
        source = new EvidenceDocsSource(repository, overrides);
        spec = LoadSpec();
        ValidateSpec(spec);
        patterns = CompilePatterns(spec.Patterns);
        ValidateMetadataBinding(spec);
        legacyBinding = new Lazy<bool>(
            ValidateLegacyBinding,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal static PcvAdminSmokeEvidenceDocsVerifier Create(
        IReadOnlyDictionary<string, string?>? overrides = null) =>
        new(RepositoryContractContext.Find(), overrides);

    internal void Verify(int ordinal)
    {
        _ = legacyBinding.Value;
        if (ordinal < 1 || ordinal > spec.Contracts.Count)
        {
            throw Invalid("ordinal");
        }

        var contract = spec.Contracts[ordinal - 1];
        if (contract.Ordinal != ordinal)
        {
            throw Invalid("contract-order");
        }

        if (ordinal == 1)
        {
            VerifyCanonicalShaFields();
        }

        foreach (var site in contract.RegexSites)
        {
            foreach (var regexCase in site.Cases)
            {
                var text = BuildSubject(regexCase, ordinal, site.Line);
                var matched = patterns[regexCase.Pattern].IsMatch(text);
                if (matched == site.Negated)
                {
                    throw Invalid($"regex-{ordinal}-{site.Line}");
                }
            }
        }

        foreach (var site in contract.ExistenceSites)
        {
            foreach (var pathIndex in site.Paths)
            {
                var exists = source.Exists(spec.Paths[pathIndex]);
                if (exists == site.Negated)
                {
                    throw Invalid($"existence-{ordinal}-{site.Line}");
                }
            }
        }

        if (ordinal == 88)
        {
            VerifyCurrentEvidenceProjection();
        }
        else if (ordinal == 89)
        {
            VerifyAdrCount();
        }
    }

    private SpecModel LoadSpec()
    {
        var text = source.ReadText(SpecPath);
        if (!Hash(text).Equals(ExpectedSpecSha256, StringComparison.Ordinal))
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var strictJson = JsonContract.Parse(SpecPath, text);
            return JsonSerializer.Deserialize<SpecModel>(
                    strictJson.Root.GetRawText(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = false,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    })
                ?? throw Invalid("json-type");
        }
        catch (JsonException error)
        {
            throw Invalid("json-type", error);
        }
        catch (NotSupportedException error)
        {
            throw Invalid("json-type", error);
        }
    }

    private static void ValidateSpec(SpecModel value)
    {
        if (value.Contract != "pcv-admin-smoke-evidence-docs-contract-spec-v1" ||
            value.LegacyPath != LegacyPath ||
            value.LegacySha256 != ExpectedLegacySha256 ||
            value.LegacyContractCount != 90 ||
            value.LegacyShouldSiteCount != 2935 ||
            value.CapturedEvaluationCount != 6687 ||
            value.RegexSiteCount != 2802 ||
            value.ExistenceSiteCount != 119 ||
            value.ManualSiteCount != 14 ||
            value.Paths is null ||
            value.Paths.Count != 307 ||
            value.Patterns is null ||
            value.Patterns.Count != 2080 ||
            value.Contracts is null ||
            value.Contracts.Count != 90)
        {
            throw Invalid("spec-contract");
        }

        if (value.Paths.Any(path => !IsRepositoryPath(path)) ||
            value.Paths.Distinct(StringComparer.Ordinal).Count() != value.Paths.Count ||
            value.Patterns.Any(string.IsNullOrEmpty) ||
            value.Patterns.Distinct(StringComparer.Ordinal).Count() != value.Patterns.Count)
        {
            throw Invalid("spec-table");
        }

        var regexSites = 0;
        var existenceSites = 0;
        var manualSites = 0;
        var evaluations = 0;
        var manualFingerprint = new StringBuilder();

        for (var index = 0; index < value.Contracts.Count; index++)
        {
            var contract = value.Contracts[index];
            if (contract.Ordinal != index + 1 ||
                string.IsNullOrEmpty(contract.Name) ||
                contract.RegexSites is null ||
                contract.ExistenceSites is null ||
                contract.ManualSites is null)
            {
                throw Invalid("spec-contract-order");
            }

            foreach (var site in contract.RegexSites)
            {
                if (site.Line <= 0 || site.Cases is null || site.Cases.Count == 0)
                {
                    throw Invalid("spec-regex-site");
                }

                foreach (var regexCase in site.Cases)
                {
                    if (regexCase.Pattern < 0 ||
                        regexCase.Pattern >= value.Patterns.Count ||
                        regexCase.Segments is null ||
                        regexCase.Segments.Count == 0)
                    {
                        throw Invalid("spec-regex-case");
                    }

                    foreach (var segment in regexCase.Segments)
                    {
                        if (segment.Path < 0 ||
                            segment.Path >= value.Paths.Count ||
                            segment.Before is not ("" or "\n"))
                        {
                            throw Invalid("spec-segment");
                        }
                    }
                }

                regexSites++;
                evaluations += site.Cases.Count;
            }

            foreach (var site in contract.ExistenceSites)
            {
                if (site.Line <= 0 ||
                    site.Paths is null ||
                    site.Paths.Count == 0 ||
                    site.Paths.Any(path => path < 0 || path >= value.Paths.Count))
                {
                    throw Invalid("spec-existence-site");
                }

                existenceSites++;
                evaluations += site.Paths.Count;
            }

            foreach (var site in contract.ManualSites)
            {
                if (site.Line <= 0 ||
                    string.IsNullOrEmpty(site.Operator) ||
                    site.Evaluations <= 0)
                {
                    throw Invalid("spec-manual-site");
                }

                if (manualFingerprint.Length > 0)
                {
                    manualFingerprint.Append(';');
                }

                manualFingerprint
                    .Append(contract.Ordinal)
                    .Append(':')
                    .Append(site.Line)
                    .Append(':')
                    .Append(site.Operator)
                    .Append(':')
                    .Append(site.Negated)
                    .Append(':')
                    .Append(site.Evaluations);
                manualSites++;
                evaluations += site.Evaluations;
            }
        }

        const string expectedManualFingerprint =
            "88:5367:Be:False:1;88:5368:Be:False:1;88:5369:BeFalse:False:1;" +
            "88:5370:BeFalse:False:1;88:5371:BeFalse:False:1;" +
            "88:5378:BeNullOrEmpty:True:7;88:5379:Match:False:7;" +
            "88:5380:Match:False:7;88:5381:Match:False:7;" +
            "88:5382:Match:True:7;88:5383:Match:False:7;" +
            "88:5385:Match:False:1;88:5386:Match:False:1;" +
            "89:5396:BeGreaterThan:False:1";

        if (regexSites != value.RegexSiteCount ||
            existenceSites != value.ExistenceSiteCount ||
            manualSites != value.ManualSiteCount ||
            evaluations != value.CapturedEvaluationCount ||
            manualFingerprint.ToString() != expectedManualFingerprint)
        {
            throw Invalid("spec-count");
        }
    }

    private static Regex[] CompilePatterns(IReadOnlyList<string> sourcePatterns)
    {
        try
        {
            return sourcePatterns
                .Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase))
                .ToArray();
        }
        catch (ArgumentException error)
        {
            throw Invalid("spec-regex", error);
        }
    }

    private static void ValidateMetadataBinding(SpecModel value)
    {
        var attributes = typeof(PcvAdminSmokeEvidenceDocsContractTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Select(method => method.GetCustomAttribute<PcvLegacyContractAttribute>())
            .Where(attribute => attribute is not null)
            .Cast<PcvLegacyContractAttribute>()
            .OrderBy(attribute => attribute.LegacyOrdinal)
            .ToArray();

        if (attributes.Length != value.Contracts.Count)
        {
            throw Invalid("metadata-count");
        }

        for (var index = 0; index < attributes.Length; index++)
        {
            var attribute = attributes[index];
            var contract = value.Contracts[index];
            var ordinal = index + 1;
            if (attribute.ContractId != $"pcv.delivery.admin-smoke-evidence-docs.{ordinal:000}" ||
                attribute.LegacyPath != LegacyPath ||
                attribute.LegacyOrdinal != ordinal ||
                attribute.LegacyName != contract.Name)
            {
                throw Invalid("metadata-binding");
            }
        }
    }

    private bool ValidateLegacyBinding()
    {
        string legacy;
        try
        {
            legacy = source.ReadText(LegacyPath);
        }
        catch (Exception error) when (error is IOException or InvalidDataException)
        {
            throw Invalid("legacy-source", error);
        }

        if (!Hash(legacy).Equals(spec.LegacySha256, StringComparison.Ordinal))
        {
            throw Invalid("legacy-sha");
        }

        return true;
    }

    private string BuildSubject(RegexCaseModel regexCase, int ordinal, int line)
    {
        var builder = new StringBuilder();
        try
        {
            foreach (var segment in regexCase.Segments)
            {
                builder.Append(segment.Before);
                builder.Append(source.ReadText(spec.Paths[segment.Path]));
            }
        }
        catch (Exception error) when (error is IOException or InvalidDataException)
        {
            throw Invalid($"source-{ordinal}-{line}", error);
        }

        return builder.ToString();
    }

    private void VerifyCurrentEvidenceProjection()
    {
        using var json = JsonContract.Parse(CurrentEvidencePath, source.ReadText(CurrentEvidencePath));
        var root = json.Root;
        if (json.RequireString(root, "contract") != "pcv-current-evidence-v1")
        {
            throw Invalid("current-contract");
        }

        var current = json.RequireObject(root, "current");
        var surfaces = json.RequireArray(current, "operator_surfaces")
            .EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String
                ? item.GetString()
                : null)
            .ToArray();
        if (!surfaces.SequenceEqual(["web", "cli"], StringComparer.Ordinal))
        {
            throw Invalid("current-surfaces");
        }

        if (json.RequireBoolean(current, "tui_present"))
        {
            throw Invalid("current-tui");
        }

        var claims = json.RequireObject(root, "claims");
        if (json.RequireBoolean(claims, "public_trusted_signing"))
        {
            throw Invalid("current-public-signing");
        }

        if (json.RequireBoolean(claims, "external_stable_publication"))
        {
            throw Invalid("current-external-publication");
        }

        var version = json.RequireString(current, "version");
        var fullgate = json.RequireString(current, "fullgate_batch");
        var provenance = json.RequireString(current, "provenance_commit");
        var generator = source.ReadText(
            "packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1");

        foreach (var target in CurrentEvidenceTargets)
        {
            var content = source.ReadText(target);
            var block = Regex.Match(content, GeneratedBlockPattern).Value;
            if (string.IsNullOrEmpty(block))
            {
                throw Invalid($"current-block-{target}");
            }

            RequireRegex(block, Regex.Escape(version), false, $"current-version-{target}");
            RequireRegex(block, Regex.Escape(fullgate), false, $"current-fullgate-{target}");
            RequireRegex(block, Regex.Escape(provenance), false, $"current-provenance-{target}");
            RequireRegex(block, "Web/TUI/CLI current-card", true, $"current-tui-label-{target}");
            RequireRegex(
                generator,
                Regex.Escape($"'{target}'"),
                false,
                $"current-generator-target-{target}");
        }

        RequireRegex(generator, @"\[switch\]\$Check", false, "current-generator-check");
        RequireRegex(generator, "PCV_CURRENT_EVIDENCE_STALE", false, "current-generator-stale");
    }

    private void VerifyCanonicalShaFields()
    {
        const string evidencePath =
            "docs/ga-ready/evidence/" +
            "internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md";
        string evidence;
        try
        {
            evidence = source.ReadText(evidencePath);
        }
        catch (Exception error) when (error is IOException or InvalidDataException)
        {
            throw Invalid("sha-source", error);
        }

        foreach (var field in new[]
        {
            "baseline_msi_sha256",
            "target_msi_sha256",
            "update_package_sha256",
        })
        {
            var match = Regex.Match(
                evidence,
                $"^{Regex.Escape(field)}:\\s*(\\S+)\\s*$",
                RegexOptions.Multiline);
            if (!match.Success ||
                !Regex.IsMatch(
                    match.Groups[1].Value,
                    "^[a-f0-9]{64}$",
                    RegexOptions.CultureInvariant))
            {
                throw Invalid($"sha-format-{field}");
            }
        }
    }

    private void VerifyAdrCount()
    {
        if (source.EnumerateRegularFiles("docs/adr", ".md").Count == 0)
        {
            throw Invalid("adr-count");
        }
    }

    private static void RequireRegex(
        string text,
        string pattern,
        bool negated,
        string detail)
    {
        var matched = Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        if (matched == negated)
        {
            throw Invalid(detail);
        }
    }

    private static bool IsRepositoryPath(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !Path.IsPathRooted(path) &&
        !path.Contains('\\') &&
        !path.Contains('\0') &&
        path.Split('/').All(segment => segment is not ("" or "." or ".."));

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static InvalidDataException Invalid(string detail, Exception? inner = null) =>
        DeliveryContractError.Invalid(SpecPath, detail, inner);

    private sealed class EvidenceDocsSource
    {
        private readonly RepositoryContractContext repository;
        private readonly IReadOnlyDictionary<string, string?> overrides;
        private readonly ConcurrentDictionary<string, string> cache =
            new(StringComparer.Ordinal);

        internal EvidenceDocsSource(
            RepositoryContractContext repository,
            IReadOnlyDictionary<string, string?>? overrides)
        {
            this.repository = repository;
            this.overrides = overrides ?? new Dictionary<string, string?>(StringComparer.Ordinal);
        }

        internal string ReadText(string path)
        {
            if (overrides.TryGetValue(path, out var overridden))
            {
                return overridden ?? throw Invalid($"source-missing-{path}");
            }

            return cache.GetOrAdd(path, repository.ReadUtf8Text);
        }

        internal bool Exists(string path)
        {
            if (overrides.TryGetValue(path, out var overridden))
            {
                return overridden is not null;
            }

            try
            {
                _ = ReadText(path);
                return true;
            }
            catch (InvalidDataException error)
                when (error.Message is "PCV_DELIVERY_PATH_INVALID|missing" or
                    "PCV_DELIVERY_PATH_INVALID|not-file")
            {
                return false;
            }
        }

        internal IReadOnlyList<string> EnumerateRegularFiles(string directory, string suffix)
        {
            var paths = repository.EnumerateRegularFiles(directory, suffix)
                .Where(path => !overrides.TryGetValue(path, out var value) || value is not null)
                .ToHashSet(StringComparer.Ordinal);

            var prefix = directory + "/";
            foreach (var entry in overrides)
            {
                if (entry.Value is not null &&
                    entry.Key.StartsWith(prefix, StringComparison.Ordinal) &&
                    entry.Key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                    !entry.Key[prefix.Length..].Contains('/'))
                {
                    paths.Add(entry.Key);
                }
            }

            return paths.Order(StringComparer.Ordinal).ToArray();
        }
    }

    private sealed class SpecModel
    {
        [JsonPropertyName("contract")]
        public required string Contract { get; init; }

        [JsonPropertyName("legacy_path")]
        public required string LegacyPath { get; init; }

        [JsonPropertyName("legacy_sha256")]
        public required string LegacySha256 { get; init; }

        [JsonPropertyName("legacy_contract_count")]
        public required int LegacyContractCount { get; init; }

        [JsonPropertyName("legacy_should_site_count")]
        public required int LegacyShouldSiteCount { get; init; }

        [JsonPropertyName("captured_evaluation_count")]
        public required int CapturedEvaluationCount { get; init; }

        [JsonPropertyName("regex_site_count")]
        public required int RegexSiteCount { get; init; }

        [JsonPropertyName("existence_site_count")]
        public required int ExistenceSiteCount { get; init; }

        [JsonPropertyName("manual_site_count")]
        public required int ManualSiteCount { get; init; }

        [JsonPropertyName("paths")]
        public required List<string> Paths { get; init; }

        [JsonPropertyName("patterns")]
        public required List<string> Patterns { get; init; }

        [JsonPropertyName("contracts")]
        public required List<ContractModel> Contracts { get; init; }
    }

    private sealed class ContractModel
    {
        [JsonPropertyName("o")]
        public required int Ordinal { get; init; }

        [JsonPropertyName("n")]
        public required string Name { get; init; }

        [JsonPropertyName("r")]
        public required List<RegexSiteModel> RegexSites { get; init; }

        [JsonPropertyName("x")]
        public required List<ExistenceSiteModel> ExistenceSites { get; init; }

        [JsonPropertyName("m")]
        public required List<ManualSiteModel> ManualSites { get; init; }
    }

    private sealed class RegexSiteModel
    {
        [JsonPropertyName("l")]
        public required int Line { get; init; }

        [JsonPropertyName("n")]
        public required bool Negated { get; init; }

        [JsonPropertyName("c")]
        public required List<RegexCaseModel> Cases { get; init; }
    }

    private sealed class RegexCaseModel
    {
        [JsonPropertyName("p")]
        public required int Pattern { get; init; }

        [JsonPropertyName("s")]
        public required List<SegmentModel> Segments { get; init; }
    }

    private sealed class SegmentModel
    {
        [JsonPropertyName("p")]
        public required int Path { get; init; }

        [JsonPropertyName("b")]
        public required string Before { get; init; }
    }

    private sealed class ExistenceSiteModel
    {
        [JsonPropertyName("l")]
        public required int Line { get; init; }

        [JsonPropertyName("n")]
        public required bool Negated { get; init; }

        [JsonPropertyName("p")]
        public required List<int> Paths { get; init; }
    }

    private sealed class ManualSiteModel
    {
        [JsonPropertyName("l")]
        public required int Line { get; init; }

        [JsonPropertyName("o")]
        public required string Operator { get; init; }

        [JsonPropertyName("n")]
        public required bool Negated { get; init; }

        [JsonPropertyName("e")]
        public required int Evaluations { get; init; }
    }
}
