using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class TrainLane3SpecBuilderTests
{
    private const string Spec04289 = "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04289.json";

    [Fact]
    public void DescriptorChainMatchesTheCommitted04289Spec()
    {
        var builder = new TrainLane3SpecBuilder(VerificationCatalogFixture.RepositoryRoot, Input04289());

        AssertSameJson(Committed04289()["descriptor_chain"], builder.DescriptorChain());
    }

    [Fact]
    public void LedgerHeadMatchesTheCommitted04289Spec()
    {
        var builder = new TrainLane3SpecBuilder(VerificationCatalogFixture.RepositoryRoot, Input04289());

        AssertSameJson(Committed04289()["ledger_head"], builder.LedgerHead());
    }

    [Theory]
    [InlineData("0.42.89-admin-smoke", "04289")]
    [InlineData("0.42.90-admin-smoke", "04290")]
    [InlineData("0.43.1-admin-smoke", "04301")]
    public void TagJoinsMajorMinorAndPatch(string version, string tag)
    {
        Assert.Equal(tag, TrainLane3SpecInput.TagOf(version));
    }

    [Theory]
    [InlineData("contract", "\"pcv-other\"", "input-invalid")]
    [InlineData("output", "\"packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04288.json\"", "tag-mismatch")]
    [InlineData("previous_spec", "\"packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04289.json\"", "tag-mismatch")]
    [InlineData("output", "\"packaging/windows-desktop-node/tests/fixtures/../lane3-promotion-docs-spec-04289.json\"", "path-invalid")]
    [InlineData("facts", "\"docs/ga-ready/trains/0.42.88-admin-smoke.evidence-facts.json\"", "path-invalid")]
    public void InputRejectsContractAndPathErrors(string property, string value, string reason)
    {
        var input = InputJson04289();
        input[property] = JsonNode.Parse(value);

        var exception = Assert.Throws<VerificationException>(() => TrainLane3SpecInput.Parse(input.ToJsonString()));

        Assert.StartsWith($"lane3-spec:{reason}:", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void InputRejectsMissingExtraAndMultilineNarrative()
    {
        var missing = InputJson04289();
        missing["narrative"]!["rules"]!.AsObject().Remove("package-build-current");
        var extra = InputJson04289();
        extra["narrative"]!["rules"]!["current_status"] = "x";
        var multiline = InputJson04289();
        multiline["narrative"]!["functional_note"] = "first\nsecond";

        Assert.StartsWith("lane3-spec:narrative-missing:rules.package-build-current", Detail(missing), StringComparison.Ordinal);
        Assert.StartsWith("lane3-spec:narrative-conflict:rules.current_status", Detail(extra), StringComparison.Ordinal);
        Assert.StartsWith("lane3-spec:narrative-invalid:functional_note", Detail(multiline), StringComparison.Ordinal);
    }

    [Fact]
    public void BuilderRejectsFactsOfAnotherVersion()
    {
        var input = Input04289() with
        {
            Version = "0.42.90-admin-smoke",
            Facts = "docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json"
        };

        var exception = Assert.Throws<VerificationException>(() =>
            new TrainLane3SpecBuilder(VerificationCatalogFixture.RepositoryRoot, input));

        Assert.Equal("lane3-spec:facts-version-mismatch:docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json", exception.Detail);
    }

    private static string Detail(JsonObject input) =>
        Assert.Throws<VerificationException>(() => TrainLane3SpecInput.Parse(input.ToJsonString())).Detail;

    private static TrainLane3SpecInput Input04289() => TrainLane3SpecInput.Parse(InputJson04289().ToJsonString());

    private static JsonObject InputJson04289()
    {
        var rules = new JsonObject();
        foreach (var id in TrainLane3SpecInput.RuleIds)
        {
            rules[id] = "rule " + id;
        }

        return new JsonObject
        {
            ["schema_version"] = 1,
            ["contract"] = TrainLane3SpecInput.Contract,
            ["version"] = "0.42.89-admin-smoke",
            ["previous_spec"] = "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04288.json",
            ["facts"] = "docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json",
            ["output"] = Spec04289,
            ["narrative"] = new JsonObject
            {
                ["updated_at"] = "2026-10-04T11:42:00+09:00",
                ["functional_note"] = "functional note",
                ["rules"] = rules
            }
        };
    }

    private static JsonObject Committed04289() =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(
            VerificationCatalogFixture.RepositoryRoot, Spec04289.Replace('/', Path.DirectorySeparatorChar))))!.AsObject();

    private static void AssertSameJson(JsonNode? expected, JsonNode actual) =>
        Assert.Equal(expected!.ToJsonString(), actual.ToJsonString());
}
