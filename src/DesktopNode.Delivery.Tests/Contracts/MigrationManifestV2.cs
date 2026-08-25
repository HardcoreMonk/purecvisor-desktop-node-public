using DesktopNode.Delivery.Tests.Infrastructure;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Delivery.Tests.Contracts;

internal sealed record MigrationManifestSummary(
    int FilesTotal,
    int ContractsTotal,
    int PackagingContracts,
    int InstallerContracts,
    int WebContracts,
    int Missing,
    int Duplicate,
    int OrderDrift);

internal static class MigrationManifestV2
{
    internal const string ErrorCode = "PCV_DELIVERY_MANIFEST_INVALID";
    private const string ManifestPath = "config/development-verification-migration-manifest.json";
    private const string SchemaPath = "config/development-verification-migration-manifest.schema.json";
    private const string WebLegacyPath = "web/tests/PcvDesktopWeb.Static.Tests.ps1";
    private const string WebOwner = "web/node-tests/web-static-contracts.test.mjs";
    private static readonly string[] RootKeys =
        ["contract", "schema_version", "inventory", "entries", "contracts"];
    private static readonly string[] CutoverRootKeys =
        ["contract", "schema_version", "inventory", "cutover_locator", "entries", "contracts"];
    private static readonly string[] CutoverLocatorKeys =
        ["shadow_sha", "shadow_run_id", "shadow_run_url", "parity_status"];
    private static readonly string[] EntryKeys =
        ["legacy_path", "domain", "legacy_contract_count", "parity_status", "local_parity", "ci_parity"];
    private static readonly string[] ContractKeys =
        [
            "legacy_path", "legacy_ordinal", "legacy_name", "domain", "replacement_owner",
            "replacement_contract_id", "parity_status", "local_parity", "ci_parity",
        ];
    private static readonly string[] ParityKeys = ["status", "evidence"];
    private static readonly string[] States = ["unmapped", "mapped", "dual-run-pass", "cutover"];
    private static readonly string[] Domains = ["packaging", "installer", "web"];

    internal static MigrationManifestSummary ReadAndValidate(RepositoryContractContext repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        using var schema = repository.LoadJson(SchemaPath);
        ValidatePublishedSchema(schema.RootElement);
        return ValidateJson(repository.ReadUtf8Text(ManifestPath), repository);
    }

    internal static MigrationManifestSummary ValidateJson(
        string json,
        RepositoryContractContext repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        try
        {
            using var document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                });
            return ValidateDocument(document.RootElement, repository);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"{ErrorCode}|json", error);
        }
    }

    private static MigrationManifestSummary ValidateDocument(
        JsonElement root,
        RepositoryContractContext repository)
    {
        var hasCutoverLocator = root.TryGetProperty("cutover_locator", out var cutoverLocator);
        RequireObject(root, hasCutoverLocator ? CutoverRootKeys : RootKeys, "manifest=shape");
        RequireString(root, "contract", "pcv-development-verification-migration-manifest-v2", "manifest=shape");
        RequireInteger(root, "schema_version", 2, "manifest=shape");
        ValidateInventory(root.GetProperty("inventory"));
        if (hasCutoverLocator)
        {
            ValidateCutoverLocator(cutoverLocator);
        }

        var entryElements = RequireArray(root, "entries", "entries=shape");
        var contractElements = RequireArray(root, "contracts", "contracts=shape");
        if (entryElements.Count != 62)
        {
            throw Invalid("entries=count");
        }

        if (contractElements.Count < 627)
        {
            throw Invalid("contracts=missing");
        }

        if (contractElements.Count != 627)
        {
            throw Invalid("contracts=count");
        }

        var legacy = DiscoverLegacy(repository);
        var replacements = DiscoverReplacements(repository);
        var replacementByKey = replacements.ToDictionary(
            row => row.Key,
            row => row,
            StringComparer.Ordinal);
        var parsedContracts = new List<ManifestContract>(627);
        var contractKeys = new HashSet<string>(StringComparer.Ordinal);
        var replacementIds = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < contractElements.Count; index++)
        {
            var element = contractElements[index];
            RequireObject(element, ContractKeys, "contract=shape");
            var row = ParseContract(element, repository);
            if (!contractKeys.Add(row.Key))
            {
                throw Invalid("contracts=duplicate-key");
            }

            if (row.ReplacementContractId is not null)
            {
                if (!IdPrefixValid(row.Domain, row.ReplacementContractId))
                {
                    throw Invalid("replacement=id");
                }

                if (!replacementIds.Add(row.ReplacementContractId))
                {
                    throw Invalid("contracts=duplicate-replacement");
                }
            }

            var expectedLegacy = legacy.Contracts[index];
            if (row.LegacyPath != expectedLegacy.LegacyPath ||
                row.LegacyOrdinal != expectedLegacy.LegacyOrdinal ||
                row.LegacyName != expectedLegacy.LegacyName ||
                row.Domain != expectedLegacy.Domain)
            {
                throw Invalid("contracts=legacy-order");
            }

            replacementByKey.TryGetValue(row.Key, out var expectedReplacement);
            if (expectedReplacement is null)
            {
                if (row.ReplacementOwner is not null || row.ReplacementContractId is not null)
                {
                    throw Invalid("replacement=orphan");
                }
            }
            else
            {
                if (row.ReplacementOwner != expectedReplacement.ReplacementOwner)
                {
                    throw Invalid("replacement=owner");
                }

                if (row.ReplacementContractId != expectedReplacement.ReplacementContractId)
                {
                    throw Invalid("replacement=id");
                }

                if (row.LegacyName != expectedReplacement.LegacyName || row.Domain != expectedReplacement.Domain)
                {
                    throw Invalid("replacement=legacy");
                }
            }

            parsedContracts.Add(row);
        }

        var entryPaths = new HashSet<string>(StringComparer.Ordinal);
        var allEntriesCutover = true;
        for (var index = 0; index < entryElements.Count; index++)
        {
            var element = entryElements[index];
            RequireObject(element, EntryKeys, "entry=shape");
            var entry = ParseEntry(element, repository);
            allEntriesCutover &= entry.ParityStatus == "cutover";
            if (!entryPaths.Add(entry.LegacyPath))
            {
                throw Invalid("entries=duplicate");
            }

            var expected = legacy.Entries[index];
            if (entry.LegacyPath != expected.LegacyPath ||
                entry.Domain != expected.Domain ||
                entry.LegacyContractCount != expected.LegacyContractCount)
            {
                throw Invalid("entries=legacy-order");
            }

            var children = parsedContracts.Where(row => row.LegacyPath == entry.LegacyPath).ToArray();
            if (children.Length != entry.LegacyContractCount)
            {
                throw Invalid("entry=contract-count");
            }

            ValidateEntryState(entry, children);
        }

        var allContractsCutover = parsedContracts.All(row => row.ParityStatus == "cutover");
        if (hasCutoverLocator != (allEntriesCutover && allContractsCutover))
        {
            throw Invalid("cutover-locator=state");
        }

        return new MigrationManifestSummary(
            62,
            627,
            parsedContracts.Count(row => row.Domain == "packaging"),
            parsedContracts.Count(row => row.Domain == "installer"),
            parsedContracts.Count(row => row.Domain == "web"),
            0,
            0,
            0);
    }

    private static LegacyInventory DiscoverLegacy(RepositoryContractContext repository)
    {
        var roots = new[]
        {
            (Domain: "packaging", Directory: "packaging/windows-desktop-node/tests"),
            (Domain: "installer", Directory: "packaging/windows-desktop-node/installer/tests"),
            (Domain: "web", Directory: "web/tests"),
        };
        var entries = new List<LegacyEntry>();
        var contracts = new List<LegacyContract>();
        foreach (var item in roots)
        {
            foreach (var legacyPath in repository.EnumerateRegularFiles(item.Directory, ".Tests.ps1"))
            {
                var parsed = LegacyPesterContractParser.Parse(
                    legacyPath,
                    repository.ReadUtf8Text(legacyPath));
                entries.Add(new LegacyEntry(legacyPath, item.Domain, parsed.Count));
                contracts.AddRange(parsed.Select(contract => new LegacyContract(
                    legacyPath,
                    contract.Ordinal,
                    contract.Name,
                    item.Domain)));
            }
        }

        if (entries.Count != 62 || contracts.Count != 627 ||
            entries.Count(row => row.Domain == "packaging") != 55 ||
            entries.Count(row => row.Domain == "installer") != 6 ||
            entries.Count(row => row.Domain == "web") != 1 ||
            contracts.Count(row => row.Domain == "packaging") != 528 ||
            contracts.Count(row => row.Domain == "installer") != 49 ||
            contracts.Count(row => row.Domain == "web") != 50)
        {
            throw Invalid("inventory=count");
        }

        return new LegacyInventory(entries, contracts);
    }

    private static IReadOnlyList<ReplacementContract> DiscoverReplacements(
        RepositoryContractContext repository)
    {
        var replacements = new List<ReplacementContract>();
        var webSource = repository.ReadUtf8Text("web/contracts/web-static-contracts.mjs");
        var webPattern = new Regex(
            """\[\s*"(?<id>web\.static\.[a-z0-9]+(?:-[a-z0-9]+)*)"\s*,\s*"(?<name>(?:\\.|[^"\\])*)"\s*,""",
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
        var webMatches = webPattern.Matches(webSource);
        if (webMatches.Count != 50)
        {
            throw Invalid("replacement=web-registry");
        }

        for (var index = 0; index < webMatches.Count; index++)
        {
            var match = webMatches[index];
            var legacyName = JsonSerializer.Deserialize<string>(
                $"\"{match.Groups["name"].Value}\"") ?? throw Invalid("replacement=web-string");
            replacements.Add(new ReplacementContract(
                WebLegacyPath,
                index + 1,
                legacyName,
                "web",
                WebOwner,
                match.Groups["id"].Value));
        }

        foreach (var type in typeof(PcvLegacyContractAttribute).Assembly.GetTypes())
        {
            var attributes = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.GetCustomAttribute<PcvLegacyContractAttribute>())
                .Where(attribute => attribute is not null)
                .Cast<PcvLegacyContractAttribute>()
                .ToArray();
            if (attributes.Length == 0)
            {
                continue;
            }

            var category = GetCategory(type);
            var domain = category == "Installer" ? "installer" : "packaging";
            const string namespacePrefix = "DesktopNode.Delivery.Tests.";
            if (type.Namespace is null ||
                !type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal))
            {
                throw Invalid("replacement=owner-namespace");
            }

            var ownerDirectory = type.Namespace[namespacePrefix.Length..].Replace('.', '/');
            if (ownerDirectory.Split('/').Any(segment => segment is "" or "." or ".."))
            {
                throw Invalid("replacement=owner-namespace");
            }

            var owner = $"src/DesktopNode.Delivery.Tests/{ownerDirectory}/{type.Name}.cs";
            var ownerSource = repository.ReadUtf8Text(owner);
            foreach (var attribute in attributes)
            {
                if (!ownerSource.Contains(attribute.ContractId, StringComparison.Ordinal))
                {
                    throw Invalid("replacement=owner-source");
                }

                replacements.Add(new ReplacementContract(
                    attribute.LegacyPath,
                    attribute.LegacyOrdinal,
                    attribute.LegacyName,
                    domain,
                    owner,
                    attribute.ContractId));
            }
        }

        if (replacements.Select(row => row.Key).Distinct(StringComparer.Ordinal).Count() != replacements.Count)
        {
            throw Invalid("replacement=duplicate-key");
        }

        if (replacements.Select(row => row.ReplacementContractId).Distinct(StringComparer.Ordinal).Count() != replacements.Count)
        {
            throw Invalid("replacement=duplicate-id");
        }

        return replacements;
    }

    private static string GetCategory(Type type)
    {
        var traits = type.GetCustomAttributesData()
            .Where(data => data.AttributeType == typeof(TraitAttribute))
            .Select(data => new
            {
                Name = data.ConstructorArguments[0].Value as string,
                Value = data.ConstructorArguments[1].Value as string,
            })
            .Where(trait => trait.Name == "Category" && trait.Value is "Installer" or "Delivery")
            .ToArray();
        var trait = traits.Length == 1 ? traits[0] : throw Invalid("replacement=category");
        return trait.Value!;
    }

    private static ManifestContract ParseContract(
        JsonElement element,
        RepositoryContractContext repository)
    {
        var legacyPath = RequireString(element, "legacy_path", "contract=shape");
        var legacyOrdinal = RequirePositiveInteger(element, "legacy_ordinal", "contract=shape");
        var legacyName = RequireString(element, "legacy_name", "contract=shape");
        var domain = RequireEnumString(element, "domain", Domains, "contract=shape");
        var replacementOwner = RequireNullableString(element, "replacement_owner", "contract=shape");
        var replacementId = RequireNullableString(element, "replacement_contract_id", "contract=shape");
        var state = RequireEnumString(element, "parity_status", States, "contract=shape");
        var local = ParseParity(element.GetProperty("local_parity"), repository);
        var ci = ParseParity(element.GetProperty("ci_parity"), repository);
        var row = new ManifestContract(
            legacyPath,
            legacyOrdinal,
            legacyName,
            domain,
            replacementOwner,
            replacementId,
            state,
            local,
            ci);
        ValidateState(row);
        return row;
    }

    private static ManifestEntry ParseEntry(
        JsonElement element,
        RepositoryContractContext repository)
    {
        var legacyPath = RequireString(element, "legacy_path", "entry=shape");
        var domain = RequireEnumString(element, "domain", Domains, "entry=shape");
        var legacyCount = RequireNonNegativeInteger(element, "legacy_contract_count", "entry=shape");
        var state = RequireEnumString(element, "parity_status", States, "entry=shape");
        var local = ParseParity(element.GetProperty("local_parity"), repository);
        var ci = ParseParity(element.GetProperty("ci_parity"), repository);
        return new ManifestEntry(legacyPath, domain, legacyCount, state, local, ci);
    }

    private static Parity ParseParity(
        JsonElement element,
        RepositoryContractContext repository)
    {
        RequireObject(element, ParityKeys, "parity=shape");
        var status = RequireEnumString(
            element,
            "status",
            ["pending", "pass", "fail"],
            "parity=shape");
        var evidence = RequireNullableString(element, "evidence", "parity=shape");
        if (status == "pending")
        {
            if (evidence is not null)
            {
                throw Invalid("parity=pending-evidence");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(evidence))
            {
                throw Invalid("parity=evidence");
            }

            _ = repository.ReadUtf8Text(evidence);
        }

        return new Parity(status, evidence);
    }

    private static void ValidateState(ManifestContract row)
    {
        var mapped = row.ReplacementOwner is not null || row.ReplacementContractId is not null;
        if (row.ParityStatus == "unmapped")
        {
            if (mapped)
            {
                throw Invalid("state=unmapped-replacement");
            }

            if (row.LocalParity.Status != "pending" || row.CiParity.Status != "pending")
            {
                throw Invalid("state=unmapped-parity");
            }

            return;
        }

        if (!mapped || row.ReplacementOwner is null || row.ReplacementContractId is null)
        {
            throw Invalid("state=mapped-null");
        }

        if (row.ParityStatus == "mapped")
        {
            if (row.LocalParity.Status is not ("pending" or "pass") || row.CiParity.Status != "pending")
            {
                throw Invalid("state=mapped-parity");
            }
        }
        else if (row.LocalParity.Status != "pass" || row.CiParity.Status != "pass")
        {
            throw Invalid("state=advanced-parity");
        }
    }

    private static void ValidateEntryState(
        ManifestEntry entry,
        IReadOnlyCollection<ManifestContract> children)
    {
        var mapped = children.Count(row => row.ReplacementContractId is not null);
        if (mapped != 0 && mapped != children.Count)
        {
            throw Invalid("entry=partial-mapping");
        }

        if (entry.ParityStatus == "unmapped")
        {
            if (mapped != 0 || entry.LocalParity.Status != "pending" || entry.CiParity.Status != "pending")
            {
                throw Invalid("entry=unmapped-state");
            }
        }
        else if (mapped != children.Count)
        {
            throw Invalid("entry=mapped-state");
        }

        if (children.Any(row =>
                row.ParityStatus != entry.ParityStatus ||
                row.LocalParity != entry.LocalParity ||
                row.CiParity != entry.CiParity))
        {
            throw Invalid("entry=contract-coherence");
        }
    }

    private static bool IdPrefixValid(string domain, string id) => domain switch
    {
        "web" => Regex.IsMatch(id, "^web\\.static\\.[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant),
        "installer" => Regex.IsMatch(id, "^pcv\\.installer\\.[a-z0-9]+(?:-[a-z0-9]+)*\\.[0-9]{3}$", RegexOptions.CultureInvariant),
        _ => Regex.IsMatch(id, "^pcv\\.delivery\\.[a-z0-9]+(?:-[a-z0-9]+)*\\.[0-9]{3}$", RegexOptions.CultureInvariant),
    };

    private static void ValidateInventory(JsonElement inventory)
    {
        RequireObject(inventory, ["files", "contracts"], "inventory=shape");
        ValidateCountObject(inventory.GetProperty("files"), 62, 55, 6, 1, "inventory=files");
        ValidateCountObject(inventory.GetProperty("contracts"), 627, 528, 49, 50, "inventory=contracts");
    }

    private static void ValidateCountObject(
        JsonElement element,
        int total,
        int packaging,
        int installer,
        int web,
        string detail)
    {
        RequireObject(element, ["total", "packaging", "installer", "web"], detail);
        RequireInteger(element, "total", total, detail);
        RequireInteger(element, "packaging", packaging, detail);
        RequireInteger(element, "installer", installer, detail);
        RequireInteger(element, "web", web, detail);
    }

    private static void ValidatePublishedSchema(JsonElement schema)
    {
        if (schema.ValueKind != JsonValueKind.Object ||
            !schema.TryGetProperty("$id", out var id) ||
            id.GetString() != "pcv-development-verification-migration-manifest-schema-v2" ||
            !schema.TryGetProperty("additionalProperties", out var additional) ||
            additional.ValueKind != JsonValueKind.False ||
            !schema.TryGetProperty("required", out var required) ||
            required.ValueKind != JsonValueKind.Array ||
            required.GetArrayLength() != 5 ||
            !schema.TryGetProperty("$defs", out var definitions) ||
            definitions.ValueKind != JsonValueKind.Object ||
            !definitions.TryGetProperty("cutoverLocator", out var cutoverDefinition) ||
            !cutoverDefinition.TryGetProperty("additionalProperties", out var cutoverAdditional) ||
            cutoverAdditional.ValueKind != JsonValueKind.False ||
            !definitions.TryGetProperty("contract", out var contract) ||
            !contract.TryGetProperty("additionalProperties", out var contractAdditional) ||
            contractAdditional.ValueKind != JsonValueKind.False)
        {
            throw Invalid("schema=invalid");
        }
    }

    private static void ValidateCutoverLocator(JsonElement locator)
    {
        RequireObject(locator, CutoverLocatorKeys, "cutover-locator=shape");
        var sha = RequireString(locator, "shadow_sha", "cutover-locator=shape");
        var runId = RequirePositiveInteger(locator, "shadow_run_id", "cutover-locator=shape");
        var runUrl = RequireString(locator, "shadow_run_url", "cutover-locator=shape");
        var status = RequireString(locator, "parity_status", "cutover-locator=shape");
        if (!Regex.IsMatch(sha, "^[0-9a-f]{40}$", RegexOptions.CultureInvariant) ||
            runId < 1 ||
            !Uri.TryCreate(runUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.Contains("/actions/runs/", StringComparison.Ordinal) ||
            status != "dual-run-pass")
        {
            throw Invalid("cutover-locator=invalid");
        }
    }

    private static List<JsonElement> RequireArray(
        JsonElement owner,
        string propertyName,
        string detail)
    {
        if (!owner.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(detail);
        }

        return property.EnumerateArray().ToList();
    }

    private static void RequireObject(
        JsonElement element,
        IReadOnlyCollection<string> expected,
        string detail)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(detail);
        }

        var names = element.EnumerateObject().Select(property => property.Name).ToArray();
        if (names.Length != expected.Count || expected.Any(name => !names.Contains(name, StringComparer.Ordinal)))
        {
            throw Invalid(detail);
        }
    }

    private static string RequireString(JsonElement owner, string name, string detail)
    {
        if (!owner.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw Invalid(detail);
        }

        var value = property.GetString();
        return !string.IsNullOrEmpty(value) ? value : throw Invalid(detail);
    }

    private static void RequireString(
        JsonElement owner,
        string name,
        string expected,
        string detail)
    {
        if (RequireString(owner, name, detail) != expected)
        {
            throw Invalid(detail);
        }
    }

    private static string? RequireNullableString(JsonElement owner, string name, string detail)
    {
        if (!owner.TryGetProperty(name, out var property))
        {
            throw Invalid(detail);
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw Invalid(detail);
        }

        var value = property.GetString();
        return !string.IsNullOrEmpty(value) ? value : throw Invalid(detail);
    }

    private static string RequireEnumString(
        JsonElement owner,
        string name,
        IReadOnlyCollection<string> allowed,
        string detail)
    {
        var value = RequireString(owner, name, detail);
        return allowed.Contains(value, StringComparer.Ordinal) ? value : throw Invalid(detail);
    }

    private static int RequirePositiveInteger(JsonElement owner, string name, string detail)
    {
        var value = RequireNonNegativeInteger(owner, name, detail);
        return value > 0 ? value : throw Invalid(detail);
    }

    private static int RequireNonNegativeInteger(JsonElement owner, string name, string detail)
    {
        if (!owner.TryGetProperty(name, out var property) ||
            property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var value) ||
            value < 0)
        {
            throw Invalid(detail);
        }

        return value;
    }

    private static void RequireInteger(
        JsonElement owner,
        string name,
        int expected,
        string detail)
    {
        if (RequireNonNegativeInteger(owner, name, detail) != expected)
        {
            throw Invalid(detail);
        }
    }

    private static InvalidDataException Invalid(string detail) =>
        new($"{ErrorCode}|{detail}");

    private sealed record LegacyInventory(
        IReadOnlyList<LegacyEntry> Entries,
        IReadOnlyList<LegacyContract> Contracts);

    private sealed record LegacyEntry(
        string LegacyPath,
        string Domain,
        int LegacyContractCount);

    private sealed record LegacyContract(
        string LegacyPath,
        int LegacyOrdinal,
        string LegacyName,
        string Domain);

    private sealed record ReplacementContract(
        string LegacyPath,
        int LegacyOrdinal,
        string LegacyName,
        string Domain,
        string ReplacementOwner,
        string ReplacementContractId)
    {
        internal string Key => $"{LegacyPath}\0{LegacyOrdinal}";
    }

    private sealed record Parity(string Status, string? Evidence);

    private sealed record ManifestEntry(
        string LegacyPath,
        string Domain,
        int LegacyContractCount,
        string ParityStatus,
        Parity LocalParity,
        Parity CiParity);

    private sealed record ManifestContract(
        string LegacyPath,
        int LegacyOrdinal,
        string LegacyName,
        string Domain,
        string? ReplacementOwner,
        string? ReplacementContractId,
        string ParityStatus,
        Parity LocalParity,
        Parity CiParity)
    {
        internal string Key => $"{LegacyPath}\0{LegacyOrdinal}";
    }
}
