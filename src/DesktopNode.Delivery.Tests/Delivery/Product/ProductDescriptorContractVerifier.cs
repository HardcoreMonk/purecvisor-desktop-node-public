using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Product;

internal sealed class ProductDescriptorContractVerifier
{
    internal const string SpecPath =
        "config/pcv-desktop-node-product-descriptor-contract-spec-v1.json";
    internal const string ModulePath =
        "packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1";

    private const string ExpectedSpecSha256 =
        "04abecd51ece223175bc0324d949b61f976fb31b5cf33327103f6b1beeee19da";
    private const string ExpectedModuleSha256 =
        "8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3";

    private static readonly string[] ExpectedKeys =
    [
        "diagnostics",
        "manifest",
        "plan",
    ];

    private static readonly int[] ExpectedCounts = [16, 16, 26];

    private static readonly string[] ExpectedActions =
    [
        "Plan",
        "Install",
        "Update",
        "Rollback",
        "Uninstall",
        "Status",
        "CollectDiagnostics",
        "ConfigureInstalled",
        "RepairInstalled",
        "RemoveInstalled",
    ];

    private static readonly ProductManifestEntryContract[] CanonicalManifestEntries =
    [
        new("schema_version", "2"),
        new("product", "PureCVisor Desktop Node"),
        new("paths", "resolved-product-paths"),
        new("assets", "web"),
        new("service_host", "dotnet-windows-service-host"),
        new("cli", "dotnet-local-api-client"),
        new("auth", "protected_file"),
        new("data_acl", "product-wrapper"),
        new("network", "loopback"),
        new("update", "update-policy-v1"),
        new("diagnostics", "diagnostics-policy-v1"),
    ];

    private readonly RepositoryContractContext repository;
    private readonly IReadOnlyDictionary<string, string?> overrides;
    private readonly bool enforceSourceHashes;
    private readonly ProductDescriptorSpec spec;
    private readonly Lazy<bool> binding;

    private ProductDescriptorContractVerifier(
        IReadOnlyDictionary<string, string?>? overrides,
        bool enforceSourceHashes)
    {
        repository = RepositoryContractContext.Find();
        this.overrides = overrides ??
            new Dictionary<string, string?>(StringComparer.Ordinal);
        this.enforceSourceHashes = enforceSourceHashes;
        spec = LoadSpec();
        ValidateSpec();
        binding = new Lazy<bool>(
            ValidateBinding,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal static ProductDescriptorContractVerifier Create(
        IReadOnlyDictionary<string, string?>? overrides = null,
        bool enforceSourceHashes = true) =>
        new(overrides, enforceSourceHashes);

    internal static void Verify(string key, int ordinal) =>
        Create().VerifyContract(key, ordinal);

    internal void VerifyContract(string key, int ordinal)
    {
        _ = binding.Value;
        var contract = spec.Contracts.SingleOrDefault(candidate =>
            candidate.Key == key && candidate.Ordinal == ordinal);
        if (contract is null)
        {
            throw Invalid("ordinal");
        }

        var module = Read(ModulePath);
        foreach (var literal in contract.RequiredLiterals)
        {
            if (!module.Contains(literal, StringComparison.Ordinal))
            {
                throw Invalid($"literal-{key}-{ordinal:D3}");
            }
        }
    }

    internal static IReadOnlyList<ProductManifestEntryContract> CanonicalManifest() =>
        CanonicalManifestEntries;

    internal static void ValidateManifestEntries(
        IReadOnlyList<ProductManifestEntryContract> entries)
    {
        if (entries.Count != CanonicalManifestEntries.Length ||
            entries.Any(entry =>
                string.IsNullOrWhiteSpace(entry.Name) ||
                string.IsNullOrWhiteSpace(entry.Value)) ||
            entries.Select(entry => entry.Name).Distinct(StringComparer.Ordinal).Count() !=
                entries.Count)
        {
            throw Invalid("manifest-cardinality");
        }

        for (var index = 0; index < CanonicalManifestEntries.Length; index++)
        {
            if (entries[index] != CanonicalManifestEntries[index])
            {
                throw Invalid("manifest-order");
            }
        }
    }

    internal static void ValidatePlanPath(string root, string candidate)
    {
        if (string.IsNullOrWhiteSpace(root) ||
            string.IsNullOrWhiteSpace(candidate) ||
            !Path.IsPathFullyQualified(root) ||
            !Path.IsPathFullyQualified(candidate))
        {
            throw Invalid("plan-path");
        }

        var fullRoot = Path.GetFullPath(root).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        var fullCandidate = Path.GetFullPath(candidate);
        if (!fullCandidate.Equals(fullRoot, StringComparison.OrdinalIgnoreCase) &&
            !fullCandidate.StartsWith(
                fullRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid("plan-path");
        }
    }

    internal static void ValidateDiagnosticProjection(
        IReadOnlyDictionary<string, string?> projection)
    {
        string[] sensitiveNames =
        [
            "api_token",
            "access_token",
            "authorization",
            "password",
            "secret",
            "protected_token",
            "jwt_signing_key",
        ];

        foreach (var pair in projection)
        {
            if (sensitiveNames.Any(name =>
                    pair.Key.Contains(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw Invalid("diagnostics-sensitive-key");
            }

            if (pair.Value is not null &&
                Regex.IsMatch(pair.Value, "(?i)\\bBearer\\s+(?!\\[REDACTED\\])\\S+"))
            {
                throw Invalid("diagnostics-leakage");
            }
        }
    }

    private ProductDescriptorSpec LoadSpec()
    {
        var text = Read(SpecPath);
        if (Hash(text) != ExpectedSpecSha256)
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var json = JsonContract.Parse(SpecPath, text);
            return JsonSerializer.Deserialize<ProductDescriptorSpec>(
                    json.Root.GetRawText(),
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = false,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    })
                ?? throw Invalid("spec-json");
        }
        catch (JsonException error)
        {
            throw Invalid("spec-json", error);
        }
        catch (NotSupportedException error)
        {
            throw Invalid("spec-json", error);
        }
    }

    private void ValidateSpec()
    {
        if (spec.Contract != "pcv-desktop-node-product-descriptor-contract-spec-v1" ||
            spec.ModulePath != ModulePath ||
            spec.ModuleSha256 != ExpectedModuleSha256 ||
            spec.LegacyContractCount != 58 ||
            spec.LegacyShouldSiteCount != 521 ||
            spec.RequiredLiteralCount != 1195 ||
            spec.LegacyFiles.Count != 3 ||
            spec.Contracts.Count != 58)
        {
            throw Invalid("spec-contract");
        }

        var literalCount = 0;
        var contractIndex = 0;
        for (var fileIndex = 0; fileIndex < spec.LegacyFiles.Count; fileIndex++)
        {
            var file = spec.LegacyFiles[fileIndex];
            if (file.Key != ExpectedKeys[fileIndex] ||
                file.ContractCount != ExpectedCounts[fileIndex] ||
                string.IsNullOrWhiteSpace(file.Path) ||
                file.Sha256.Length != 64 ||
                file.ShouldSiteCount <= 0)
            {
                throw Invalid("spec-file-order");
            }

            for (var ordinal = 1; ordinal <= file.ContractCount; ordinal++)
            {
                var contract = spec.Contracts[contractIndex++];
                if (contract.Key != file.Key ||
                    contract.Ordinal != ordinal ||
                    string.IsNullOrWhiteSpace(contract.Name) ||
                    contract.RequiredLiterals.Count == 0 ||
                    contract.RequiredLiterals.Any(string.IsNullOrWhiteSpace) ||
                    contract.RequiredLiterals.Distinct(StringComparer.Ordinal).Count() !=
                        contract.RequiredLiterals.Count)
                {
                    throw Invalid("spec-contract-order");
                }

                literalCount += contract.RequiredLiterals.Count;
            }
        }

        if (contractIndex != spec.Contracts.Count ||
            literalCount != spec.RequiredLiteralCount ||
            spec.LegacyFiles.Sum(file => file.ShouldSiteCount) !=
                spec.LegacyShouldSiteCount)
        {
            throw Invalid("spec-count");
        }
    }

    private bool ValidateBinding()
    {
        var module = Read(ModulePath);
        if (enforceSourceHashes && Hash(module) != ExpectedModuleSha256)
        {
            throw Invalid("module-sha");
        }

        foreach (var file in spec.LegacyFiles)
        {
            var legacy = Read(file.Path);
            if (Hash(legacy) != file.Sha256)
            {
                throw Invalid("legacy-sha");
            }

            var parsed = LegacyPesterContractParser.Parse(file.Path, legacy);
            var expected = spec.Contracts.Where(contract => contract.Key == file.Key).ToArray();
            if (parsed.Count != expected.Length)
            {
                throw Invalid("legacy-count");
            }

            for (var index = 0; index < parsed.Count; index++)
            {
                if (parsed[index].Ordinal != expected[index].Ordinal ||
                    parsed[index].Name != expected[index].Name)
                {
                    throw Invalid("legacy-order");
                }
            }
        }

        ValidateDiagnosticsSource(module);
        ValidateManifestSource(module);
        ValidatePlanSource(module);
        SourceContract.RequireNoExecutableToken(ModulePath, module, "Invoke-Expression");
        return true;
    }

    private static void ValidateDiagnosticsSource(string module)
    {
        var policy = ExtractFunction(module, "Get-PcvDesktopNodeDiagnosticsPolicy");
        RequireOrdered(
            policy,
            "diagnostics-policy-order",
            "schema_version = 1",
            "mode = 'windows-event-log-default-jsonl-retained'",
            "diagnostic_bundle_schema_version = 1",
            "redaction_version = 1",
            "event_log = [ordered]@{",
            "install_log = [ordered]@{",
            "service_logs = [ordered]@{",
            "windows_event_log = [ordered]@{");

        var sourceList = ExtractFunction(module, "New-PcvDesktopNodeDiagnosticBundleSourceList");
        var names = Regex.Matches(
                sourceList,
                "\\[ordered\\]@\\{ name = '(?<name>[^']+)'; artifact =")
            .Select(match => match.Groups["name"].Value)
            .ToArray();
        string[] expectedNames =
        [
            "summary",
            "service_status",
            "service_host_status",
            "runtime_policy",
            "lan_security_policy",
            "update_policy",
            "migration_plan",
            "rollback_state",
            "diagnostics_self_audit",
            "operational_evidence",
            "service_host_metadata",
            "product_manifest",
            "events",
            "install_log",
            "jobs",
            "update_transaction_journal",
            "service_host_config",
        ];
        if (!names.SequenceEqual(expectedNames, StringComparer.Ordinal) ||
            !sourceList.Contains("redacted = $true", StringComparison.Ordinal))
        {
            throw Invalid("diagnostics-source-cardinality");
        }

        var eventLogPlan = ExtractFunction(
            module,
            "New-PcvDesktopNodeEventLogRegistrationPlan");
        RequireOrdered(
            eventLogPlan,
            "eventlog-plan-order",
            "default_transition = [ordered]@{",
            "'eventlog-default-transition'",
            "register = [ordered]@{",
            "'eventlog-register'",
            "unregister = [ordered]@{",
            "'eventlog-remove'");
        foreach (var mutation in new[] { "New-EventLog", "Remove-EventLog", "Limit-EventLog" })
        {
            if (eventLogPlan.Contains(mutation, StringComparison.Ordinal))
            {
                throw Invalid("eventlog-direct-mutation");
            }
        }
    }

    private static void ValidateManifestSource(string module)
    {
        var assets = ExtractFunction(module, "Get-PcvDesktopNodeProductAssets");
        var names = Regex.Matches(assets, "(?m)^\\s*name = '(?<name>[^']+)'")
            .Select(match => match.Groups["name"].Value)
            .ToArray();
        if (!names.SequenceEqual(["web"], StringComparer.Ordinal))
        {
            throw Invalid("manifest-asset-cardinality");
        }

        var fileList = ExtractFunction(module, "Get-PcvDesktopNodeAssetFileList");
        if (!fileList.Contains("-notmatch '[\\\\/]tests[\\\\/]'", StringComparison.Ordinal) ||
            !fileList.Contains("-ErrorAction Stop", StringComparison.Ordinal))
        {
            throw Invalid("manifest-asset-boundary");
        }

        var manifest = ExtractFunction(module, "New-PcvDesktopNodeProductManifest");
        RequireOrdered(
            manifest,
            "manifest-order",
            "schema_version = 2",
            "product = 'PureCVisor Desktop Node'",
            "version = $Version",
            "source_root = $SourceRoot",
            "generated_at =",
            "paths = $paths",
            "assets = $assets",
            "service_host = [ordered]@{",
            "cli = [ordered]@{",
            "auth = [ordered]@{",
            "data_acl =",
            "network =",
            "update =",
            "diagnostics =");
        if (manifest.Contains("tui", StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid("manifest-retired-surface");
        }

        var runtimePayload = ExtractFunction(
            module,
            "Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths");
        var payloadNames = Regex.Matches(runtimePayload, "'(?<name>[^']+)'")
            .Select(match => match.Groups["name"].Value)
            .ToArray();
        string[] expectedPayload =
        [
            "DesktopNode.Host.exe",
            "pcvcli.exe",
            "Invoke-PcvDesktopNodeProduct.ps1",
            "PcvDesktopNodeProduct.psm1",
        ];
        if (!payloadNames.SequenceEqual(expectedPayload, StringComparer.Ordinal))
        {
            throw Invalid("manifest-runtime-payload");
        }

        ValidateManifestEntries(CanonicalManifestEntries);
    }

    private static void ValidatePlanSource(string module)
    {
        var plan = ExtractFunction(module, "New-PcvDesktopNodeProductPlan");
        var actionMatch = Regex.Match(
            plan,
            "(?s)\\[ValidateSet\\((?<values>.*?)\\)\\]\\s*\\[string\\]\\$Action");
        var actions = Regex.Matches(actionMatch.Groups["values"].Value, "'(?<value>[^']+)'")
            .Select(match => match.Groups["value"].Value)
            .ToArray();
        if (!actionMatch.Success ||
            !actions.SequenceEqual(ExpectedActions, StringComparer.Ordinal) ||
            actions.Distinct(StringComparer.Ordinal).Count() != actions.Length)
        {
            throw Invalid("plan-actions");
        }

        RequireOrdered(
            plan,
            "plan-default-order",
            "[string]$Action = 'Plan'",
            "[ValidateRange(1, 64)][int]$WorkerCount = 1",
            "[ValidateRange(1, 600)][int]$TimeoutSec = 30",
            "PCV_PRODUCT_INLINE_TOKEN_FORBIDDEN",
            "$requiresElevation = $Action -in @(",
            "$deletePaths = @()",
            "$deletePathPatterns = @()",
            "schema_version = 1",
            "action = $Action",
            "requires_elevation = $requiresElevation",
            "no_auto_reboot = [ordered]@{",
            "enabled = $true",
            "enforcement = 'product-process-command-guard'",
            "allowed_schemes = @('file', 'https')",
            "expected_sha256_required = $true",
            "resolution_stage = 'before-service-stop'",
            "mutates_host = $false");

        foreach (var required in new[]
        {
            "$paths.token_protected_file",
            "$paths.token_file",
            "$paths.account_file",
            "$paths.jwt_signing_key_file",
            "$paths.job_store",
            "$paths.event_log",
            "$paths.install_log",
            "$paths.diagnostics_root",
        })
        {
            if (!plan.Contains(required, StringComparison.Ordinal))
            {
                throw Invalid("plan-remove-data");
            }
        }
    }

    private string Read(string path)
    {
        if (overrides.TryGetValue(path, out var value))
        {
            return value ?? throw Invalid("missing-source");
        }

        return repository.ReadUtf8Text(path);
    }

    private static void RequireOrdered(
        string source,
        string detail,
        params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            if (index < 0)
            {
                throw Invalid(detail);
            }

            offset = index + token.Length;
        }
    }

    private static string ExtractFunction(string sourceText, string name)
    {
        var start = sourceText.IndexOf($"function {name} {{", StringComparison.Ordinal);
        if (start < 0)
        {
            throw Invalid("missing-function");
        }

        var next = sourceText.IndexOf("\nfunction ", start + name.Length + 10, StringComparison.Ordinal);
        return next < 0 ? sourceText[start..] : sourceText[start..next];
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false).GetBytes(text)))
            .ToLowerInvariant();

    private static InvalidDataException Invalid(string detail, Exception? inner = null) =>
        new($"PCV_DELIVERY_PRODUCT_DESCRIPTOR_INVALID|{detail}", inner);

    private sealed record ProductDescriptorSpec(
        string Contract,
        string ModulePath,
        string ModuleSha256,
        int LegacyContractCount,
        int LegacyShouldSiteCount,
        int RequiredLiteralCount,
        IReadOnlyList<ProductDescriptorLegacyFile> LegacyFiles,
        IReadOnlyList<ProductDescriptorSpecContract> Contracts);

    private sealed record ProductDescriptorLegacyFile(
        string Key,
        string Path,
        string Sha256,
        int ContractCount,
        int ShouldSiteCount);

    private sealed record ProductDescriptorSpecContract(
        string Key,
        int Ordinal,
        string Name,
        IReadOnlyList<string> RequiredLiterals);
}

internal sealed record ProductManifestEntryContract(string Name, string Value);
