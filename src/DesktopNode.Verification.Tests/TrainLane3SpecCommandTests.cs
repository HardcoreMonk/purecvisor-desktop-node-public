using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class TrainLane3SpecCommandTests
{
    private const string InputPath = "docs/ga-ready/trains/0.42.89-admin-smoke.lane3-spec-input.json";

    [Fact]
    public void LedgerRowsMatchTheCommitted04289SpecApartFromRuleTexts()
    {
        var builder = new TrainLane3SpecBuilder(VerificationCatalogFixture.RepositoryRoot, TrainLane3SpecBuilderTests.Input04289());
        var expected = TrainLane3SpecBuilderTests.Committed04289()["ledger_rows"]!.DeepClone().AsObject();
        foreach (var section in new[] { "supersede", "replace" })
        {
            foreach (var row in expected[section]!.AsArray().Select(item => item!.AsObject()))
            {
                var id = row["id"]!.GetValue<string>();
                if (TrainLane3SpecInput.RuleIds.Contains(id))
                {
                    row["rule"] = "rule " + id;
                }
            }
        }

        Assert.Equal(expected.ToJsonString(), builder.LedgerRows().ToJsonString());
    }

    [Fact]
    public void IndexSectionsMatchTheCommitted04289SpecApartFromTheFunctionalNote()
    {
        var builder = new TrainLane3SpecBuilder(VerificationCatalogFixture.RepositoryRoot, TrainLane3SpecBuilderTests.Input04289());
        var expected = TrainLane3SpecBuilderTests.Committed04289()["index_sections"]!.DeepClone().AsObject();
        expected["functional_note"] = "functional note";

        Assert.Equal(expected.ToJsonString(), builder.IndexSections().ToJsonString());
    }

    [Fact]
    public void WritesThenChecksTheSpecInATemporaryRepository()
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-lane3-spec-" + Guid.NewGuid().ToString("N"));
        try
        {
            Write(root, "src/DesktopNode.sln", string.Empty);
            Write(root, "config/development-verification-suites.json", "{}");
            foreach (var path in new[]
            {
                "packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04288.json",
                "docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json",
                "docs/ga-ready/release-train.json"
            })
            {
                Write(root, path, File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, path)));
            }

            Write(root, InputPath, TrainLane3SpecBuilderTests.InputJson04289().ToJsonString());

            var missing = Run(root, "--check");
            var written = Run(root, "--write");
            var current = Run(root, "--check");

            Assert.Equal((1, "missing"), (missing.ExitCode, missing.Result.GetProperty("status").GetString()));
            Assert.Equal((0, "written"), (written.ExitCode, written.Result.GetProperty("status").GetString()));
            Assert.Equal((0, "current"), (current.ExitCode, current.Result.GetProperty("status").GetString()));
            var text = File.ReadAllText(Full(root, TrainLane3SpecBuilderTests.Spec04289Path));
            Assert.DoesNotContain('\r', text);
            Assert.EndsWith("}\n", text, StringComparison.Ordinal);
            Assert.Contains("\"previous_tag\": \"04288\"", text, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void RejectsAnIncompleteCommandLine()
    {
        var (exitCode, result) = Run(VerificationCatalogFixture.RepositoryRoot, null);

        Assert.Equal(2, exitCode);
        Assert.Equal("lane3-spec:cli-invalid:usage", result.GetProperty("error_detail").GetString());
    }

    private static (int ExitCode, JsonElement Result) Run(string root, string? mode)
    {
        using var output = new StringWriter();
        string[] args = mode is null ? ["lane3-spec", "--input", InputPath] : ["lane3-spec", "--input", InputPath, mode];
        var exitCode = TrainLane3SpecCommand.Run(args, root, output);
        return (exitCode, JsonDocument.Parse(output.ToString()).RootElement.Clone());
    }

    private static void Write(string root, string relativePath, string text)
    {
        var full = Full(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, text);
    }

    private static string Full(string root, string relativePath) =>
        Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
}
