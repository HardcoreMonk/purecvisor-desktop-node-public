using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Contracts;

internal sealed class LegacyBatchContractVerifier
{
    private readonly RepositoryContractContext repository;
    private readonly LegacyBatchSpec spec;
    private readonly string expectedSpecSha256;
    private readonly string expectedContract;
    private readonly string errorCode;
    private readonly IReadOnlyList<string> expectedKeys;
    private readonly IReadOnlyList<int> expectedCounts;
    private readonly int expectedContractCount;
    private readonly int expectedShouldSiteCount;
    private readonly int expectedRequiredLiteralCount;
    private readonly int expectedSourceCount;
    private readonly Dictionary<string, string> sources;
    private readonly Lazy<bool> binding;

    internal LegacyBatchContractVerifier(
        string specPath,
        string expectedSpecSha256,
        string expectedContract,
        string errorCode,
        IReadOnlyList<string> expectedKeys,
        IReadOnlyList<int> expectedCounts,
        int expectedContractCount,
        int expectedShouldSiteCount,
        int expectedRequiredLiteralCount,
        int expectedSourceCount)
    {
        repository = RepositoryContractContext.Find();
        this.expectedSpecSha256 = expectedSpecSha256;
        this.expectedContract = expectedContract;
        this.errorCode = errorCode;
        this.expectedKeys = expectedKeys;
        this.expectedCounts = expectedCounts;
        this.expectedContractCount = expectedContractCount;
        this.expectedShouldSiteCount = expectedShouldSiteCount;
        this.expectedRequiredLiteralCount = expectedRequiredLiteralCount;
        this.expectedSourceCount = expectedSourceCount;
        spec = LoadSpec(specPath);
        ValidateSpec();
        sources = spec.SourceFiles.ToDictionary(
            source => source.Path,
            source => repository.ReadUtf8Text(source.Path),
            StringComparer.Ordinal);
        binding = new Lazy<bool>(ValidateBinding, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal void Verify(string key, int ordinal)
    {
        _ = binding.Value;
        var contract = spec.Contracts.SingleOrDefault(candidate =>
            candidate.Key == key && candidate.Ordinal == ordinal);
        if (contract is null)
        {
            throw Invalid("ordinal");
        }

        var source = CombinedSource(key);
        foreach (var literal in contract.RequiredLiterals)
        {
            if (!source.Contains(literal, StringComparison.Ordinal))
            {
                throw Invalid($"literal-{key}-{ordinal:D3}");
            }
        }
    }

    internal string Source(string path) =>
        sources.TryGetValue(path, out var source)
            ? source
            : throw Invalid("source-not-declared");

    internal string CombinedSource(string key)
    {
        var file = spec.LegacyFiles.SingleOrDefault(candidate => candidate.Key == key)
            ?? throw Invalid("key");
        return string.Join("\n", file.SourcePaths.Select(Source));
    }

    private LegacyBatchSpec LoadSpec(string specPath)
    {
        var text = repository.ReadUtf8Text(specPath);
        if (Hash(text) != expectedSpecSha256)
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var json = JsonContract.Parse(specPath, text);
            return JsonSerializer.Deserialize<LegacyBatchSpec>(
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
        if (spec.Contract != expectedContract ||
            spec.LegacyContractCount != expectedContractCount ||
            spec.LegacyShouldSiteCount != expectedShouldSiteCount ||
            spec.RequiredLiteralCount != expectedRequiredLiteralCount ||
            spec.SourceFiles.Count != expectedSourceCount ||
            spec.LegacyFiles.Count != expectedKeys.Count ||
            spec.Contracts.Count != expectedContractCount ||
            expectedCounts.Count != expectedKeys.Count ||
            spec.SourceFiles.Select(source => source.Path)
                .Distinct(StringComparer.Ordinal).Count() != spec.SourceFiles.Count)
        {
            throw Invalid("spec-contract");
        }

        var declaredSources = spec.SourceFiles.Select(source => source.Path)
            .ToHashSet(StringComparer.Ordinal);
        var contractIndex = 0;
        var literalCount = 0;
        for (var fileIndex = 0; fileIndex < spec.LegacyFiles.Count; fileIndex++)
        {
            var file = spec.LegacyFiles[fileIndex];
            if (file.Key != expectedKeys[fileIndex] ||
                file.ContractCount != expectedCounts[fileIndex] ||
                file.ShouldSiteCount < 1 ||
                file.Sha256.Length != 64 ||
                file.SourcePaths.Count == 0 ||
                file.SourcePaths.Any(path => !declaredSources.Contains(path)) ||
                file.SourcePaths.Distinct(StringComparer.Ordinal).Count() != file.SourcePaths.Count)
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
            spec.LegacyFiles.Sum(file => file.ShouldSiteCount) != spec.LegacyShouldSiteCount ||
            spec.SourceFiles.Any(source => source.Sha256.Length != 64))
        {
            throw Invalid("spec-count");
        }
    }

    private bool ValidateBinding()
    {
        foreach (var source in spec.SourceFiles)
        {
            if (!sources.TryGetValue(source.Path, out var text) || Hash(text) != source.Sha256)
            {
                throw Invalid("source-sha");
            }
        }

        foreach (var file in spec.LegacyFiles)
        {
            var legacy = repository.ReadUtf8Text(file.Path);
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

        return true;
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false).GetBytes(text)))
            .ToLowerInvariant();

    private InvalidDataException Invalid(string detail, Exception? inner = null) =>
        new($"{errorCode}|{detail}", inner);

    private sealed record LegacyBatchSpec(
        string Contract,
        int LegacyContractCount,
        int LegacyShouldSiteCount,
        int RequiredLiteralCount,
        IReadOnlyList<LegacyBatchSourceFile> SourceFiles,
        IReadOnlyList<LegacyBatchLegacyFile> LegacyFiles,
        IReadOnlyList<LegacyBatchSpecContract> Contracts);

    private sealed record LegacyBatchSourceFile(string Path, string Sha256);

    private sealed record LegacyBatchLegacyFile(
        string Key,
        string Path,
        string Sha256,
        int ContractCount,
        int ShouldSiteCount,
        IReadOnlyList<string> SourcePaths);

    private sealed record LegacyBatchSpecContract(
        string Key,
        int Ordinal,
        string Name,
        IReadOnlyList<string> RequiredLiterals);
}
