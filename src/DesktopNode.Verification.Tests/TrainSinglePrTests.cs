using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class TrainSinglePrTests
{
    private const string Facts04289 = "docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json";
    private const string PayloadEvidence = "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-10-04-04289-payload-pass.md";

    [Fact]
    public void Lane3SpecUsesThePayloadMainPushDocument()
    {
        var root = Lane3Repository(documents =>
        {
            var mainPush = documents.Single(document => document!["template"]!.GetValue<string>() == "main-push")!.AsObject();
            mainPush["template"] = "main-push-payload";
            mainPush["path"] = PayloadEvidence;
        });
        try
        {
            var builder = new TrainLane3SpecBuilder(root, TrainLane3SpecBuilderTests.Input04289());
            var chain = builder.DescriptorChain()["values"]!.AsObject();

            Assert.Equal(
                "04289-payload-main-push-not-provider-required-authority",
                chain["current_public_boundary_main_push_compatibility_alias_semantics"]!.GetValue<string>());
            Assert.Equal(PayloadEvidence, chain["current_public_boundary_main_push_evidence"]!.GetValue<string>());
            Assert.Equal(PayloadEvidence, builder.IndexSections()["main_push_evidence"]!.GetValue<string>());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Lane3SpecRejectsTwoMainPushDocuments()
    {
        var root = Lane3Repository(documents =>
        {
            var copy = documents.Single(document => document!["template"]!.GetValue<string>() == "main-push")!.DeepClone().AsObject();
            copy["template"] = "main-push-payload";
            copy["path"] = PayloadEvidence;
            documents.Add(copy);
        });
        try
        {
            var exception = Assert.Throws<VerificationException>(
                () => new TrainLane3SpecBuilder(root, TrainLane3SpecBuilderTests.Input04289()));

            Assert.Equal("lane3-spec:facts-main-push-ambiguous:" + Facts04289, exception.Detail);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string Lane3Repository(Action<JsonArray> editDocuments)
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-single-pr-" + Guid.NewGuid().ToString("N"));
        foreach (var path in new[]
        {
            "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04288.json",
            "docs/ga-ready/release-train.json"
        })
        {
            Write(root, path, File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, path)));
        }

        var facts = JsonNode.Parse(File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, Facts04289)))!.AsObject();
        editDocuments(facts["documents"]!.AsArray());
        Write(root, Facts04289, facts.ToJsonString());
        return root;
    }

    private static void Write(string root, string relativePath, string text)
    {
        var path = Full(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private static string Full(string root, string relativePath) =>
        Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    [Fact]
    public void PayloadMainPushTemplateRendersEveryValue()
    {
        var template = File.ReadAllText(Path.Combine(
            VerificationCatalogFixture.RepositoryRoot, "docs", "ga-ready", "trains", "templates", "main-push-payload.md.tmpl"));
        var keys = System.Text.RegularExpressions.Regex.Matches(template, "\\{\\{([a-z0-9_]+)\\}\\}")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal);
        var values = keys.ToDictionary(key => key, key => "v-" + key, StringComparer.Ordinal);

        var rendered = TrainEvidenceTemplate.Render(template, values, "docs/ga-ready/evidence/x.md");

        Assert.DoesNotContain("{{", rendered, StringComparison.Ordinal);
        Assert.Contains("head_sha: `v-head_sha`", rendered, StringComparison.Ordinal);
        Assert.Contains("path_check_contract: `pcv-train-path-check-result-v1`", rendered, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("docs/a.md\ndocs/ga-ready/trains/x.json\n", 0, 0)]
    [InlineData("docs/a.md\nsrc/DesktopNode.Api/X.cs\n.github/workflows/y.yml\n", 1, 2)]
    public void PathCheckFlagsOnlyProductPaths(string diff, int exitCode, int productPaths)
    {
        using var output = new StringWriter();

        var code = TrainPathCheckCommand.Run(
            ["train-path-check", "--payload", "05f42a2", "--head", "HEAD"],
            VerificationCatalogFixture.RepositoryRoot,
            output,
            new FixedRunner(diff));

        var result = JsonDocument.Parse(output.ToString()).RootElement;
        Assert.Equal(exitCode, code);
        Assert.Equal(productPaths, result.GetProperty("product_paths").GetArrayLength());
    }

    [Theory]
    [InlineData("a35ea05", "a35ea05", 0)]
    [InlineData("a35ea05", "dde42d8", 1)]
    public void PathCheckRunsGitThroughTheProcessPolicy(string payload, string head, int exitCode)
    {
        using var output = new StringWriter();

        var code = TrainPathCheckCommand.Run(
            ["train-path-check", "--payload", payload, "--head", head],
            VerificationCatalogFixture.RepositoryRoot,
            output);

        var result = JsonDocument.Parse(output.ToString()).RootElement;
        Assert.True(code == exitCode, result.ToString());
        Assert.Equal(exitCode == 0, result.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public void PathCheckRejectsAnInvalidPayload()
    {
        using var output = new StringWriter();

        var code = TrainPathCheckCommand.Run(
            ["train-path-check", "--payload", "main;rm", "--head", "HEAD"],
            VerificationCatalogFixture.RepositoryRoot,
            output,
            new FixedRunner(string.Empty));

        Assert.Equal(2, code);
    }

    private sealed class FixedRunner(string standardOutput) : IProcessRunner
    {
        public Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken) =>
            Task.FromResult(new ProcessExecutionResult(0, 1, false, false, standardOutput, string.Empty, string.Empty));
    }
}
