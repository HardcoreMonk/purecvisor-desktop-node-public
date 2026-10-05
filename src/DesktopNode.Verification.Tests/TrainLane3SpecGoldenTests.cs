using System.Text.Json;

namespace DesktopNode.Verification.Tests;

// Every committed Lane 3 spec input must rebuild exactly the committed lane3-promotion-docs spec.
public sealed class TrainLane3SpecGoldenTests
{
    private static readonly string TrainsRoot = Path.Combine(
        VerificationCatalogFixture.RepositoryRoot, "docs", "ga-ready", "trains");

    public static TheoryData<string> InputFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in InputPaths())
        {
            data.Add(path);
        }

        return data;
    }

    [Fact]
    public void FirstTrainInputIsPresent()
    {
        Assert.Contains("docs/ga-ready/trains/0.42.89-admin-smoke.lane3-spec-input.json", InputPaths());
    }

    [Theory]
    [MemberData(nameof(InputFiles))]
    public void InputRebuildsTheCommittedSpec(string inputPath)
    {
        using var output = new StringWriter();

        var exitCode = TrainLane3SpecCommand.Run(
            ["lane3-spec", "--input", inputPath, "--check"],
            VerificationCatalogFixture.RepositoryRoot,
            output);

        var result = JsonDocument.Parse(output.ToString()).RootElement;
        Assert.True(exitCode == 0, result.ToString());
        Assert.Equal("current", result.GetProperty("status").GetString());
    }

    private static IReadOnlyList<string> InputPaths() =>
        Directory.GetFiles(TrainsRoot, "*.lane3-spec-input.json")
            .Select(path => "docs/ga-ready/trains/" + Path.GetFileName(path))
            .Order(StringComparer.Ordinal)
            .ToList();
}
