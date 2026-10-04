using System.Text;
using System.Text.Json;

namespace DesktopNode.Verification.Tests;

public sealed class TrainEvidenceRendererTests
{
    private const string DocumentPath = "docs/ga-ready/evidence/sample-2026-10-04-04289.md";

    [Fact]
    public void RendersValuesAndKeepsOptionalLineWithValue()
    {
        var rendered = TrainEvidenceTemplate.Render(
            "id: `{{id}}`\n{{?note}}note: {{note}}\nend {{id}}\n",
            Values(("id", "a-1"), ("note", "kept")),
            DocumentPath);

        Assert.Equal("id: `a-1`\nnote: kept\nend a-1\n", rendered);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DropsOptionalLineWhenValueIsEmptyOrAbsent(bool present)
    {
        var values = present ? Values(("id", "a-1"), ("note", "")) : Values(("id", "a-1"));

        var rendered = TrainEvidenceTemplate.Render("{{id}}\n{{?note}}note: x\nend\n", values, DocumentPath);

        Assert.Equal("a-1\nend\n", rendered);
    }

    [Theory]
    [InlineData("{{id}} {{other}}\n", "missing-value")]
    [InlineData("{{id}}\n", "unused-value")]
    [InlineData("{{id}} {{Bad}}\n", "template-invalid")]
    [InlineData("{{?}}x\n{{id}}\n", "template-invalid")]
    public void RejectsTemplateAndValueMismatch(string template, string reason)
    {
        var values = reason == "unused-value"
            ? Values(("id", "a-1"), ("extra", "x"))
            : Values(("id", "a-1"));

        var exception = Assert.Throws<VerificationException>(() =>
            TrainEvidenceTemplate.Render(template, values, DocumentPath));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.StartsWith($"train-evidence:{reason}:", exception.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("""{"schema_version":1,"contract":"other","version":"0.42.89-admin-smoke","documents":[]}""", "facts-invalid")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89","documents":[{"template":"package","path":"docs/ga-ready/evidence/a.md","values":{}}]}""", "facts-invalid")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"nope","path":"docs/ga-ready/evidence/a.md","values":{}}]}""", "template-unknown")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"package","path":"docs/other/a.md","values":{}}]}""", "path-invalid")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"package","path":"docs/ga-ready/evidence/a.md","values":{}},{"template":"msix","path":"docs/ga-ready/evidence/a.md","values":{}}]}""", "path-duplicate")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"package","path":"docs/ga-ready/evidence/a.md","values":{"k":"a\nb"}}]}""", "value-invalid")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"package","path":"docs/ga-ready/evidence/a.md","values":{"k":"{{x}}"}}]}""", "value-invalid")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"package","path":"docs/ga-ready/evidence/a.md","values":{"K":"x"}}]}""", "value-invalid")]
    [InlineData("""{"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[{"template":"package","path":"docs/ga-ready/evidence/a.md","values":{}}],"extra":1}""", "facts-invalid")]
    public void RejectsInvalidFacts(string json, string reason)
    {
        var exception = Assert.Throws<VerificationException>(() => TrainEvidenceFactsReader.Parse(json));

        Assert.StartsWith($"train-evidence:{reason}:", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void CheckReportsMissingStaleAndCurrentWithoutWriting()
    {
        using var repository = TrainEvidenceRepository.Create();
        repository.WriteEvidence("b.md", "B old\n");
        repository.WriteEvidence("c.md", "C c-1\n");

        var (exitCode, result) = repository.Run("--check");

        Assert.Equal(1, exitCode);
        Assert.False(result.GetProperty("ok").GetBoolean());
        Assert.Equal(["missing", "stale", "current"], Statuses(result));
        Assert.False(File.Exists(repository.EvidencePath("a.md")));
        Assert.Equal("B old\n", File.ReadAllText(repository.EvidencePath("b.md")));
    }

    [Fact]
    public void WriteRefusesChangedEvidenceAndWritesNothing()
    {
        using var repository = TrainEvidenceRepository.Create();
        repository.WriteEvidence("b.md", "B old\n");

        var (exitCode, result) = repository.Run("--write");

        Assert.Equal(2, exitCode);
        Assert.Equal("train-evidence:existing-differs:docs/ga-ready/evidence/b.md", result.GetProperty("error_detail").GetString());
        Assert.False(File.Exists(repository.EvidencePath("a.md")));
    }

    [Fact]
    public void WriteCreatesMissingAndUpdatesOnlyAllowedEvidence()
    {
        using var repository = TrainEvidenceRepository.Create();
        repository.WriteEvidence("b.md", "B old\n");

        var (exitCode, result) = repository.Run("--write", "--allow-update", "docs/ga-ready/evidence/b.md");

        Assert.Equal(0, exitCode);
        Assert.Equal(["written", "updated", "written"], Statuses(result));
        Assert.Equal("A a-1\n", File.ReadAllText(repository.EvidencePath("a.md")));
        Assert.Equal("B b-1\n", File.ReadAllText(repository.EvidencePath("b.md")));
        Assert.Equal(0, repository.Run("--check").ExitCode);
    }

    [Theory]
    [InlineData("--check", "--write")]
    [InlineData("--check", "--allow-update", "docs/ga-ready/evidence/a.md")]
    [InlineData("--write", "--allow-update", "docs/ga-ready/evidence/unknown.md")]
    [InlineData("--verbose")]
    public void RejectsInvalidCommandLines(params string[] extra)
    {
        using var repository = TrainEvidenceRepository.Create();

        var (exitCode, result) = repository.Run(extra);

        Assert.Equal(2, exitCode);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, result.GetProperty("error_code").GetString());
        Assert.StartsWith("train-evidence:", result.GetProperty("error_detail").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplicationRoutesTrainEvidenceBeforeVerifyGrammar()
    {
        using var repository = TrainEvidenceRepository.Create();
        var application = new VerificationApplication(
            new RecordingProcessRunner(),
            new RecordingManagedSuiteRunner(),
            new RecordingVerificationFileSystem(),
            FixedVerificationClock.At(DateTimeOffset.UnixEpoch),
            () => repository.Root,
            () => null,
            () => repository.Root);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await application.RunAsync(
            ["train-evidence", "--facts", TrainEvidenceRepository.FactsRelativePath, "--check"],
            output,
            error,
            CancellationToken.None);

        Assert.Equal(1, exitCode);
        Assert.Equal(TrainEvidenceCommand.ResultContract, JsonDocument.Parse(output.ToString()).RootElement.GetProperty("contract").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    private static IReadOnlyDictionary<string, string> Values(params (string Key, string Value)[] pairs) =>
        pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    private static string[] Statuses(JsonElement result) =>
        result.GetProperty("documents").EnumerateArray().Select(item => item.GetProperty("status").GetString()!).ToArray();

    private sealed class TrainEvidenceRepository : IDisposable
    {
        internal const string FactsRelativePath = "docs/ga-ready/trains/sample.evidence-facts.json";

        private TrainEvidenceRepository(string root) => Root = root;

        internal string Root { get; }

        internal static TrainEvidenceRepository Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "pcv-train-evidence-" + Guid.NewGuid().ToString("N"));
            var repository = new TrainEvidenceRepository(root);
            repository.Write("src/DesktopNode.sln", string.Empty);
            repository.Write("config/development-verification-suites.json", "{}");
            repository.Write("docs/ga-ready/trains/templates/package.md.tmpl", "A {{v}}\n");
            repository.Write("docs/ga-ready/trains/templates/msix.md.tmpl", "B {{v}}\n");
            repository.Write("docs/ga-ready/trains/templates/burn.md.tmpl", "C {{v}}\n");
            repository.Write(FactsRelativePath, """
                {"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.89-admin-smoke","documents":[
                {"template":"package","path":"docs/ga-ready/evidence/a.md","values":{"v":"a-1"}},
                {"template":"msix","path":"docs/ga-ready/evidence/b.md","values":{"v":"b-1"}},
                {"template":"burn","path":"docs/ga-ready/evidence/c.md","values":{"v":"c-1"}}]}
                """);
            Directory.CreateDirectory(Path.Combine(root, "docs", "ga-ready", "evidence"));
            return repository;
        }

        internal string EvidencePath(string name) => Path.Combine(Root, "docs", "ga-ready", "evidence", name);

        internal void WriteEvidence(string name, string content) => File.WriteAllText(EvidencePath(name), content);

        internal (int ExitCode, JsonElement Result) Run(params string[] extra)
        {
            using var output = new StringWriter();
            var args = new[] { "train-evidence", "--facts", FactsRelativePath }.Concat(extra).ToArray();
            var exitCode = TrainEvidenceCommand.Run(args, Root, output);
            return (exitCode, JsonDocument.Parse(output.ToString()).RootElement.Clone());
        }

        private void Write(string relativePath, string content)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}
