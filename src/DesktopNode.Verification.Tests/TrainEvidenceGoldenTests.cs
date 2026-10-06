using System.Text.Json;

namespace DesktopNode.Verification.Tests;

// Every committed train facts file must render exactly the evidence documents that are in the repository.
public sealed class TrainEvidenceGoldenTests
{
    private static readonly string TrainsRoot = Path.Combine(
        VerificationCatalogFixture.RepositoryRoot, "docs", "ga-ready", "trains");

    public static TheoryData<string> FactsFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in FactsPaths())
        {
            data.Add(path);
        }

        return data;
    }

    [Fact]
    public void FirstTrainFactsFileIsPresent()
    {
        Assert.Contains("docs/ga-ready/trains/0.42.89-admin-smoke.evidence-facts.json", FactsPaths());
    }

    private static IReadOnlyList<string> FactsPaths() =>
        Directory.GetFiles(TrainsRoot, "*.evidence-facts.json")
            .Select(path => "docs/ga-ready/trains/" + Path.GetFileName(path))
            .Order(StringComparer.Ordinal)
            .ToList();

    [Theory]
    [MemberData(nameof(FactsFiles))]
    public void FactsRenderTheCommittedEvidence(string factsPath)
    {
        using var output = new StringWriter();

        var exitCode = TrainEvidenceCommand.Run(
            ["train-evidence", "--facts", factsPath, "--check"],
            VerificationCatalogFixture.RepositoryRoot,
            output);

        var result = JsonDocument.Parse(output.ToString()).RootElement;
        var stale = result.TryGetProperty("documents", out var documents)
            ? documents.EnumerateArray()
                .Where(item => item.GetProperty("status").GetString() != "current")
                .Select(item => item.GetProperty("path").GetString())
                .ToList()
            : [result.GetProperty("error_detail").GetString()];
        Assert.True(exitCode == 0, string.Join(", ", stale));
        Assert.Empty(stale);
    }

    [Fact]
    public void TemplateDirectoryMatchesTheTemplateCatalog()
    {
        var files = Directory.GetFiles(Path.Combine(TrainsRoot, "templates"))
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            TrainEvidenceFactsReader.Templates.Select(name => name + ".md.tmpl").Order(StringComparer.Ordinal),
            files);
    }

    [Fact]
    public void FirstTrainFactsCoverEveryTemplateOnce()
    {
        var facts = TrainEvidenceFactsReader.Parse(File.ReadAllText(
            Path.Combine(TrainsRoot, "0.42.89-admin-smoke.evidence-facts.json")));

        Assert.Equal("0.42.89-admin-smoke", facts.Version);
        Assert.Equal(
            TrainEvidenceFactsReader.Templates.Where(template => template != "main-push-payload").Order(StringComparer.Ordinal),
            facts.Documents.Select(document => document.Template).Order(StringComparer.Ordinal));
    }
}
