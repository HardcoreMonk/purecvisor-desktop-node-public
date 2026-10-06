using System.Text.Json;

namespace DesktopNode.Verification.Tests;

public sealed class TrainSinglePrTests
{
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
