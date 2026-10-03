using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

// docs/superpowers/specs/2026-10-04-purecvisor-desktop-node-release-train-design.md: the queue and train
// history live in docs/ga-ready/release-train.json and must stay in step with current-evidence.json.
[Trait("Category", "Delivery")]
public sealed class PcvReleaseTrainContractTests
{
    private const string TrainPath = "docs/ga-ready/release-train.json";
    private static readonly RepositoryContractContext Repository = RepositoryContractContext.Find();
    private static readonly Regex Version = new(@"^0\.\d+\.\d+-admin-smoke$", RegexOptions.CultureInvariant);
    private static readonly Regex Commit = new("^[0-9a-f]{7,40}$", RegexOptions.CultureInvariant);

    [Fact]
    public void DeclaresTheReleaseTrainContractAndItsDesign()
    {
        using var train = Load();
        var root = train.RootElement;

        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-release-train-v1", root.GetProperty("contract").GetString());
        Assert.True(File.Exists(Path.Combine(Repository.RootPath, root.GetProperty("design").GetString()!)));
        Assert.Equal(JsonValueKind.Array, root.GetProperty("queue").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("trains").ValueKind);
    }

    [Fact]
    public void TracksTheOperationalCurrentOfCurrentEvidence()
    {
        using var train = Load();
        using var current = JsonDocument.Parse(Repository.ReadUtf8Text("docs/ga-ready/current-evidence.json"));

        Assert.Equal(
            current.RootElement.GetProperty("current").GetProperty("version").GetString(),
            train.RootElement.GetProperty("operational_current").GetString());
    }

    [Fact]
    public void QueueRowsNameAMergedProductChangeAndItsProbe()
    {
        using var train = Load();
        foreach (var row in train.RootElement.GetProperty("queue").EnumerateArray())
        {
            AssertQueueRow(row);
        }
    }

    [Fact]
    public void TrainsCarryOneVersionAndAtMostOneRuns()
    {
        using var train = Load();
        AssertTrains(train.RootElement.GetProperty("trains").EnumerateArray().ToArray());
    }

    [Fact]
    public void RowRulesAcceptASampleAndRejectBrokenRows()
    {
        using var sample = JsonDocument.Parse("""
            {
              "queue": [
                { "pr": 33, "merge_commit": "ed4a4fa", "area": "hyperv", "summary": "managed delete cleanup",
                  "lane2_probe": { "family": "vm.delete", "design": "docs/DEVELOPMENT_PROCEDURE.md" }, "risk_tier": "M" },
                { "pr": 34, "merge_commit": "d27086f", "area": "web", "summary": "copy fix", "lane2_probe": null, "risk_tier": "S" }
              ],
              "trains": [
                { "version": "0.42.88-admin-smoke", "source_commit": "47ff198", "carriages": [30], "status": "promoted" },
                { "version": "0.42.89-admin-smoke", "source_commit": "e055024", "carriages": [33, 34], "status": "running" }
              ],
              "bad_rows": [
                { "pr": 0, "merge_commit": "ed4a4fa", "area": "hyperv", "summary": "x", "lane2_probe": null, "risk_tier": "M" },
                { "pr": 35, "merge_commit": "not-a-sha", "area": "hyperv", "summary": "x", "lane2_probe": null, "risk_tier": "M" },
                { "pr": 36, "merge_commit": "ed4a4fa", "area": "hyperv", "summary": "x", "lane2_probe": null, "risk_tier": "XL" },
                { "pr": 37, "merge_commit": "ed4a4fa", "area": "hyperv", "summary": "x",
                  "lane2_probe": { "family": "vm.delete", "design": "docs/missing-probe-design.md" }, "risk_tier": "M" }
              ],
              "bad_trains": [
                { "version": "0.42.89-admin-smoke", "source_commit": "e055024", "carriages": [33], "status": "running" },
                { "version": "0.42.90-admin-smoke", "source_commit": "e055024", "carriages": [34], "status": "running" }
              ]
            }
            """);
        var root = sample.RootElement;

        foreach (var row in root.GetProperty("queue").EnumerateArray())
        {
            AssertQueueRow(row);
        }

        AssertTrains(root.GetProperty("trains").EnumerateArray().ToArray());
        foreach (var row in root.GetProperty("bad_rows").EnumerateArray())
        {
            Assert.ThrowsAny<Exception>(() => AssertQueueRow(row));
        }

        Assert.ThrowsAny<Exception>(() => AssertTrains(root.GetProperty("bad_trains").EnumerateArray().ToArray()));
    }

    private static void AssertQueueRow(JsonElement row)
    {
        Assert.True(row.GetProperty("pr").GetInt32() > 0);
        Assert.Matches(Commit, row.GetProperty("merge_commit").GetString()!);
        Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("area").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("summary").GetString()));
        Assert.Contains(row.GetProperty("risk_tier").GetString(), new[] { "S", "M", "L" });
        var probe = row.GetProperty("lane2_probe");
        if (probe.ValueKind != JsonValueKind.Null)
        {
            Assert.False(string.IsNullOrWhiteSpace(probe.GetProperty("family").GetString()));
            Assert.True(File.Exists(Path.Combine(Repository.RootPath, probe.GetProperty("design").GetString()!)));
        }
    }

    private static void AssertTrains(JsonElement[] trains)
    {
        foreach (var item in trains)
        {
            Assert.Matches(Version, item.GetProperty("version").GetString()!);
            Assert.Matches(Commit, item.GetProperty("source_commit").GetString()!);
            Assert.Contains(item.GetProperty("status").GetString(), new[] { "running", "stopped", "promoted" });
            Assert.All(item.GetProperty("carriages").EnumerateArray(), carriage => Assert.True(carriage.GetInt32() > 0));
        }

        Assert.True(trains.Count(item => item.GetProperty("status").GetString() == "running") <= 1);
        Assert.Equal(trains.Length, trains.Select(item => item.GetProperty("version").GetString()).Distinct().Count());
    }

    [Fact]
    public void DevelopmentProcedureOwnsTheTrainRules()
    {
        var procedure = Repository.ReadUtf8Text("docs/DEVELOPMENT_PROCEDURE.md");

        Assert.Contains("## 10. Release train", procedure, StringComparison.Ordinal);
        Assert.Contains("`docs/ga-ready/release-train.json`", procedure, StringComparison.Ordinal);
        Assert.Contains("| release train 출발 |", procedure, StringComparison.Ordinal);
    }

    private static JsonDocument Load() => JsonDocument.Parse(Repository.ReadUtf8Text(TrainPath));
}
