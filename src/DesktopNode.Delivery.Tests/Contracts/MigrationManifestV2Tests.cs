using DesktopNode.Delivery.Tests.Infrastructure;
using System.Text.Json.Nodes;

namespace DesktopNode.Delivery.Tests.Contracts;

[Trait("Category", "VerificationInfrastructure")]
public sealed class MigrationManifestV2Tests
{
    [Fact]
    public void PublishedLedgerHasExactStrictV2Inventory()
    {
        var summary = MigrationManifestV2.ReadAndValidate(RepositoryContractContext.Find());

        Assert.Equal(62, summary.FilesTotal);
        Assert.Equal(627, summary.ContractsTotal);
        Assert.Equal(528, summary.PackagingContracts);
        Assert.Equal(49, summary.InstallerContracts);
        Assert.Equal(50, summary.WebContracts);
        Assert.Equal(0, summary.Missing);
        Assert.Equal(0, summary.Duplicate);
        Assert.Equal(0, summary.OrderDrift);
    }

    [Theory]
    [InlineData("missing-contract")]
    [InlineData("duplicate-key")]
    [InlineData("duplicate-id")]
    [InlineData("legacy-order")]
    [InlineData("wrong-owner")]
    [InlineData("mapped-null")]
    [InlineData("unmapped-replacement")]
    [InlineData("pass-without-evidence")]
    [InlineData("unknown-prefix")]
    [InlineData("extra-property")]
    public void RejectsDeterministicNegativeLedgerMutations(string mutation)
    {
        var repository = RepositoryContractContext.Find();
        var root = JsonNode.Parse(
            repository.ReadUtf8Text("config/development-verification-migration-manifest.json"))!.AsObject();
        Mutate(root, mutation);

        var error = Assert.Throws<InvalidDataException>(
            () => MigrationManifestV2.ValidateJson(root.ToJsonString(), repository));

        Assert.StartsWith(MigrationManifestV2.ErrorCode + "|", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(FindRepositoryRoot(), error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReflectionInventoryDoesNotTrustManifestReplacementRows()
    {
        var metadata = typeof(PcvLegacyContractAttribute).Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods())
            .Select(method => method.GetCustomAttributes(typeof(PcvLegacyContractAttribute), false)
                .Cast<PcvLegacyContractAttribute>()
                .SingleOrDefault())
            .Where(attribute => attribute is not null)
            .Cast<PcvLegacyContractAttribute>()
            .ToArray();

        Assert.Equal(metadata.Length, metadata.Select(item => item.ContractId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            metadata.Length,
            metadata.Select(item => $"{item.LegacyPath}\0{item.LegacyOrdinal}").Distinct(StringComparer.Ordinal).Count());
    }

    private static void Mutate(JsonObject root, string mutation)
    {
        var contracts = root["contracts"]!.AsArray();
        var mapped = contracts
            .Select(node => node!.AsObject())
            .Where(row => row["replacement_contract_id"] is not null)
            .ToArray();
        switch (mutation)
        {
            case "missing-contract":
                contracts.RemoveAt(contracts.Count - 1);
                break;
            case "duplicate-key":
                contracts[1]!["legacy_path"] = contracts[0]!["legacy_path"]!.GetValue<string>();
                contracts[1]!["legacy_ordinal"] = contracts[0]!["legacy_ordinal"]!.GetValue<int>();
                break;
            case "duplicate-id":
                mapped[1]["replacement_contract_id"] = mapped[0]["replacement_contract_id"]!.GetValue<string>();
                break;
            case "legacy-order":
                (contracts[0]!["legacy_name"], contracts[1]!["legacy_name"]) =
                    (contracts[1]!["legacy_name"]!.GetValue<string>(), contracts[0]!["legacy_name"]!.GetValue<string>());
                break;
            case "wrong-owner":
                mapped[0]["replacement_owner"] = "wrong/owner";
                break;
            case "mapped-null":
                mapped[0]["replacement_owner"] = null;
                mapped[0]["replacement_contract_id"] = null;
                break;
            case "unmapped-replacement":
                mapped[0]["parity_status"] = "unmapped";
                break;
            case "pass-without-evidence":
                mapped[0]["local_parity"] = new JsonObject { ["status"] = "pass", ["evidence"] = null };
                break;
            case "unknown-prefix":
                mapped[0]["replacement_contract_id"] = "unknown.prefix.contract";
                break;
            case "extra-property":
                contracts[0]!["extra"] = true;
                break;
            default:
                throw new InvalidOperationException("PCV_DELIVERY_TEST_INVALID|unknown-mutation");
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("PCV_DELIVERY_CONFIG_INVALID|repository-root-not-found");
    }
}
