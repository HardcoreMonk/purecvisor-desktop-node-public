using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Product;

internal sealed class ProductInvokeContractVerifier
{
    internal const string SpecPath =
        "config/pcv-desktop-node-product-invoke-contract-spec-v1.json";
    internal const string LegacyPath =
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1";
    internal const string EntrypointPath =
        "packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1";
    internal const string ModulePath =
        "packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1";

    private const string ExpectedSpecSha256 =
        "9b4269d5820840f0f1b94795c7b2b97cca8bf0abfac2b15026fc5fc74e80b0f6";
    private const string ExpectedLegacySha256 =
        "0fff10664f5e65b72eb1cc86b668717b4caaeac15b4612e0d94c524ffc777955";
    private const string ExpectedEntrypointSha256 =
        "086d491283f170558899cbce5e640c17e774186ed83b86d39a791ce4a7f4c1d5";
    private const string ExpectedModuleSha256 =
        "8c0bf982097881f56e60354f53961e54ea4d7a49e566a1e1eee861cd309403c3";

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

    private readonly SourceSnapshot source;
    private readonly ProductInvokeSpec spec;
    private readonly bool enforceSourceHashes;
    private readonly Lazy<bool> binding;

    private ProductInvokeContractVerifier(
        IReadOnlyDictionary<string, string?>? overrides,
        bool enforceSourceHashes)
    {
        var repository = RepositoryContractContext.Find();
        source = new SourceSnapshot(repository, overrides);
        this.enforceSourceHashes = enforceSourceHashes;
        spec = LoadSpec();
        ValidateSpec();
        binding = new Lazy<bool>(
            ValidateBinding,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal static ProductInvokeContractVerifier Create(
        IReadOnlyDictionary<string, string?>? overrides = null,
        bool enforceSourceHashes = true) =>
        new(overrides, enforceSourceHashes);

    internal static void Verify(int ordinal) => Create().VerifyContract(ordinal);

    internal void VerifyContract(int ordinal)
    {
        _ = binding.Value;
        if (ordinal is < 1 or > 61)
        {
            throw Invalid("ordinal");
        }

        var contract = spec.Contracts[ordinal - 1];
        if (contract.Ordinal != ordinal)
        {
            throw Invalid("contract-order");
        }

        var combined = source.Entrypoint + "\n" + source.Module;
        foreach (var literal in contract.RequiredLiterals)
        {
            if (!combined.Contains(literal, StringComparison.Ordinal))
            {
                throw Invalid($"literal-{ordinal:D3}");
            }
        }
    }

    internal static void ValidateCommandPlan(IReadOnlyList<ProductCommandContract> commands)
    {
        if (commands.Count == 0)
        {
            throw Invalid("command-plan-empty");
        }

        foreach (var command in commands)
        {
            if (string.IsNullOrWhiteSpace(command.FileName) ||
                HasControl(command.FileName) ||
                command.Arguments.Any(argument => argument is null || HasControl(argument)))
            {
                throw Invalid("argument-injection");
            }

            var leaf = Path.GetFileName(command.FileName);
            if (leaf.Equals("Restart-Computer", StringComparison.OrdinalIgnoreCase) ||
                leaf.Equals("shutdown", StringComparison.OrdinalIgnoreCase) ||
                leaf.Equals("shutdown.exe", StringComparison.OrdinalIgnoreCase) ||
                leaf.Equals("Invoke-Expression", StringComparison.OrdinalIgnoreCase) ||
                leaf.Equals("iex", StringComparison.OrdinalIgnoreCase) ||
                leaf is "msiexec" or "msiexec.exe" && command.Arguments.Any(argument =>
                    argument.Equals("/forcerestart", StringComparison.OrdinalIgnoreCase) ||
                    argument.Equals("REBOOT=Force", StringComparison.OrdinalIgnoreCase) ||
                    argument.Equals("REBOOT=ForceRestart", StringComparison.OrdinalIgnoreCase)))
            {
                throw Invalid("mutation-command");
            }
        }
    }

    private ProductInvokeSpec LoadSpec()
    {
        var text = source.Read(SpecPath);
        if (Hash(text) != ExpectedSpecSha256)
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var json = JsonContract.Parse(SpecPath, text);
            return JsonSerializer.Deserialize<ProductInvokeSpec>(
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
        if (spec.Contract != "pcv-desktop-node-product-invoke-contract-spec-v1" ||
            spec.LegacyPath != LegacyPath ||
            spec.LegacySha256 != ExpectedLegacySha256 ||
            spec.EntrypointPath != EntrypointPath ||
            spec.EntrypointSha256 != ExpectedEntrypointSha256 ||
            spec.ModulePath != ModulePath ||
            spec.ModuleSha256 != ExpectedModuleSha256 ||
            spec.LegacyContractCount != 61 ||
            spec.LegacyShouldSiteCount != 418 ||
            spec.RequiredLiteralCount != 844 ||
            spec.Contracts.Count != 61)
        {
            throw Invalid("spec-contract");
        }

        var literalCount = 0;
        for (var index = 0; index < spec.Contracts.Count; index++)
        {
            var contract = spec.Contracts[index];
            if (contract.Ordinal != index + 1 ||
                string.IsNullOrWhiteSpace(contract.Name) ||
                contract.RequiredLiterals.Count == 0 ||
                contract.RequiredLiterals.Any(string.IsNullOrEmpty) ||
                contract.RequiredLiterals.Distinct(StringComparer.Ordinal).Count() !=
                    contract.RequiredLiterals.Count)
            {
                throw Invalid("spec-order");
            }

            literalCount += contract.RequiredLiterals.Count;
        }

        if (literalCount != spec.RequiredLiteralCount)
        {
            throw Invalid("spec-count");
        }
    }

    private bool ValidateBinding()
    {
        var legacy = source.Read(LegacyPath);
        var entrypoint = source.Entrypoint;
        var module = source.Module;
        if (Hash(legacy) != ExpectedLegacySha256)
        {
            throw Invalid("legacy-sha");
        }

        if (enforceSourceHashes &&
            (Hash(entrypoint) != ExpectedEntrypointSha256 ||
             Hash(module) != ExpectedModuleSha256))
        {
            throw Invalid("product-source-sha");
        }

        var contracts = LegacyPesterContractParser.Parse(LegacyPath, legacy);
        if (contracts.Count != spec.Contracts.Count)
        {
            throw Invalid("legacy-count");
        }

        for (var index = 0; index < contracts.Count; index++)
        {
            if (contracts[index].Ordinal != spec.Contracts[index].Ordinal ||
                contracts[index].Name != spec.Contracts[index].Name)
            {
                throw Invalid("legacy-order");
            }
        }

        ValidateActionSurface(entrypoint, "entrypoint-actions");
        ValidateActionSurface(ExtractFunction(module, "New-PcvDesktopNodeProductPlan"), "plan-actions");
        _ = ExtractFunction(module, "Invoke-PcvDesktopNodeProductAction");
        ValidateCommandBoundary(module);
        ValidateRedactionBoundary(module);
        SourceContract.RequireExecutableToken(
            ModulePath,
            module,
            "Invoke-PcvDesktopNodeProductAction");
        SourceContract.RequireExecutableToken(
            ModulePath,
            module,
            "Test-PcvDesktopNodeJobStorePendingCommitGuard");
        SourceContract.RequireNoExecutableToken(ModulePath, module, "Invoke-Expression");
        return true;
    }

    private static void ValidateActionSurface(string sourceText, string detail)
    {
        var match = Regex.Match(
            sourceText,
            "(?s)\\[ValidateSet\\((?<values>.*?)\\)\\]\\s*\\[string\\]\\$Action");
        if (!match.Success)
        {
            throw Invalid(detail);
        }

        var actions = Regex.Matches(match.Groups["values"].Value, "'(?<value>[^']+)'")
            .Select(candidate => candidate.Groups["value"].Value)
            .ToArray();
        if (!actions.SequenceEqual(ExpectedActions, StringComparer.Ordinal) ||
            actions.Distinct(StringComparer.Ordinal).Count() != actions.Length)
        {
            throw Invalid(detail);
        }
    }

    private static void ValidateCommandBoundary(string module)
    {
        var command = ExtractFunction(module, "Invoke-PcvProductProcessCommand");
        var guard = command.IndexOf("Test-PcvProductAutoRebootCommand", StringComparison.Ordinal);
        var invoke = command.IndexOf("$processResult = & $InvokeProcess", StringComparison.Ordinal);
        if (guard < 0 || invoke < 0 || guard >= invoke || module.Contains(".ArgumentList", StringComparison.Ordinal))
        {
            throw Invalid("command-boundary");
        }

        var reboot = ExtractFunction(module, "Test-PcvProductAutoRebootCommand");
        foreach (var literal in new[]
        {
            "^(Restart-Computer|shutdown|shutdown\\.exe)$",
            "^(msiexec|msiexec\\.exe)$",
            "^(?i)(/forcerestart|REBOOT=Force|REBOOT=ForceRestart)$",
            "PCV_PRODUCT_AUTO_REBOOT_FORBIDDEN",
        })
        {
            if (!reboot.Contains(literal, StringComparison.Ordinal))
            {
                throw Invalid("mutation-command");
            }
        }
    }

    private static void ValidateRedactionBoundary(string module)
    {
        var sensitive = ExtractFunction(module, "Test-PcvDiagnosticSensitiveKey");
        foreach (var key in new[]
        {
            "api_token",
            "access_token",
            "authorization",
            "password",
            "secret",
            "protected_token",
            "jwt_signing_key",
        })
        {
            if (!sensitive.Contains(key, StringComparison.Ordinal))
            {
                throw Invalid("redaction-key");
            }
        }

        var redaction = ExtractFunction(module, "ConvertTo-PcvDesktopNodeDiagnosticRedactedString");
        if (!redaction.Contains("Bearer [REDACTED]", StringComparison.Ordinal))
        {
            throw Invalid("redaction-bearer");
        }
    }

    private static string ExtractFunction(string sourceText, string name)
    {
        var start = sourceText.IndexOf($"function {name} {{", StringComparison.Ordinal);
        if (start < 0)
        {
            throw Invalid("missing-route");
        }

        var next = sourceText.IndexOf("\nfunction ", start + name.Length + 10, StringComparison.Ordinal);
        return next < 0 ? sourceText[start..] : sourceText[start..next];
    }

    private static bool HasControl(string value) =>
        value.IndexOfAny(['\0', '\r', '\n']) >= 0;

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static InvalidDataException Invalid(string detail, Exception? inner = null) =>
        new($"PCV_DELIVERY_PRODUCT_INVOKE_INVALID|{detail}", inner);

    private sealed class SourceSnapshot
    {
        private readonly RepositoryContractContext repository;
        private readonly IReadOnlyDictionary<string, string?> overrides;

        internal SourceSnapshot(
            RepositoryContractContext repository,
            IReadOnlyDictionary<string, string?>? overrides)
        {
            this.repository = repository;
            this.overrides = overrides ?? new Dictionary<string, string?>(StringComparer.Ordinal);
        }

        internal string Entrypoint => Read(EntrypointPath);
        internal string Module => Read(ModulePath);

        internal string Read(string path)
        {
            if (overrides.TryGetValue(path, out var value))
            {
                return value ?? throw Invalid("override-missing");
            }

            return repository.ReadUtf8Text(path);
        }
    }

    private sealed record ProductInvokeSpec(
        string Contract,
        string LegacyPath,
        string LegacySha256,
        string EntrypointPath,
        string EntrypointSha256,
        string ModulePath,
        string ModuleSha256,
        int LegacyContractCount,
        int LegacyShouldSiteCount,
        int RequiredLiteralCount,
        IReadOnlyList<ProductInvokeSpecContract> Contracts);

    private sealed record ProductInvokeSpecContract(
        int Ordinal,
        string Name,
        IReadOnlyList<string> RequiredLiterals);
}

internal sealed record ProductCommandContract(
    string FileName,
    IReadOnlyList<string> Arguments);
