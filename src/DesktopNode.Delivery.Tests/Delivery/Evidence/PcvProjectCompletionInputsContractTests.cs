using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

// docs/superpowers/specs/2026-10-07-purecvisor-desktop-node-completion-autopilot-design.md: pcvverify completion
// reads the criteria, backlog and autopilot policy files; their structure is pinned here, not their verdict.
[Trait("Category", "Delivery")]
public sealed class PcvProjectCompletionInputsContractTests
{
    private const string CriteriaPath = "config/project-completion-criteria.json";
    private const string BacklogPath = "docs/ga-ready/backlog.json";
    private const string PolicyPath = "config/completion-autopilot-policy.json";
    private static readonly RepositoryContractContext Repository = RepositoryContractContext.Find();
    private static readonly Regex ServicePlanItem = new(@"^P[0-2]-\d{1,2}$", RegexOptions.CultureInvariant);
    private static readonly Regex BacklogId = new(@"^BL-\d{4}$", RegexOptions.CultureInvariant);
    private static readonly Regex Date = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.CultureInvariant);

    private static readonly string[] GapKinds =
    [
        "train-departure", "lane2-probe", "lane1-fix", "new-design", "deadline-wait", "user-decision", "ci-wait", "scenario",
    ];

    [Fact]
    public void CriteriaListTheFifteenServicePlanItemsWithResolvableEvidenceOrWaiver()
    {
        using var criteria = Load(CriteriaPath);
        var root = criteria.RootElement;

        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-project-completion-criteria-v1", root.GetProperty("contract").GetString());
        Assert.Equal("pcv-project-completion-definition-v3", root.GetProperty("definition").GetString());
        var scenarios = root.GetProperty("scenarios").EnumerateArray().ToList();
        Assert.Equal(["S1", "S2", "S3", "S4"], scenarios.Select(scenario => scenario.GetProperty("id").GetString()));
        foreach (var scenario in scenarios)
        {
            Assert.Contains(scenario.GetProperty("status").GetString(), new[] { "open", "passed" });
            var record = scenario.GetProperty("demo_record");
            Assert.True(record.ValueKind == JsonValueKind.Null || Exists(record.GetString()!), scenario.GetProperty("id").GetString());
        }
        Assert.True(Exists(root.GetProperty("design").GetString()!));

        var items = root.GetProperty("service_plan_items").EnumerateArray().ToList();
        Assert.Equal(15, items.Count);
        Assert.Equal(15, items.Select(item => item.GetProperty("id").GetString()).Distinct(StringComparer.Ordinal).Count());
        foreach (var item in items)
        {
            var id = item.GetProperty("id").GetString()!;
            Assert.Matches(ServicePlanItem, id);
            var evidence = item.GetProperty("evidence_ids").EnumerateArray().Select(value => value.GetString()!).ToList();
            var waiver = item.GetProperty("waiver");
            Assert.True(evidence.Count > 0 || waiver.ValueKind == JsonValueKind.String, $"{id} needs evidence or a waiver");
            Assert.All(evidence, evidenceId => Assert.True(Exists($"docs/ga-ready/evidence/{evidenceId}.md"), evidenceId));
            if (waiver.ValueKind == JsonValueKind.String)
            {
                Assert.True(Exists(waiver.GetString()!), $"{id} waiver");
            }
        }
    }

    [Fact]
    public void CriteriaNameTheRequiredWorkflowsDeadlineRisksAndPermanentScope()
    {
        using var criteria = Load(CriteriaPath);
        var root = criteria.RootElement;
        var workflows = root.GetProperty("workflows");

        Assert.Equal(new[] { "Development Gates" }, Strings(workflows.GetProperty("c1")));
        Assert.Equal(new[] { "Development Gates", "Public Boundary Contract" }, Strings(workflows.GetProperty("c5")));

        foreach (var risk in root.GetProperty("deadline_risks").EnumerateArray())
        {
            Assert.False(string.IsNullOrWhiteSpace(risk.GetProperty("id").GetString()));
            Assert.Matches(Date, risk.GetProperty("deadline").GetString()!);
            var status = risk.GetProperty("status").GetString();
            Assert.Contains(status, new[] { "open", "closed" });
            Assert.Equal(status == "closed", risk.GetProperty("closed_by").ValueKind == JsonValueKind.String);
        }

        Assert.Equal(
            new[]
            {
                "public-trusted-signing", "external-stable-publication", "workstation-parity-100",
                "hyperv-exactly-once-reconcile-completeness", "linux-kvm-stack",
            },
            root.GetProperty("permanent_out_of_scope").EnumerateArray().Select(row => row.GetProperty("id").GetString()!));
    }

    [Fact]
    public void BacklogRowsCarryAClassificationStatusAndOutOfScopeReference()
    {
        using var backlog = Load(BacklogPath);
        using var criteria = Load(CriteriaPath);
        var root = backlog.RootElement;
        var scopeIds = criteria.RootElement.GetProperty("permanent_out_of_scope").EnumerateArray()
            .Select(row => row.GetProperty("id").GetString()!).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-backlog-v1", root.GetProperty("contract").GetString());
        Assert.True(Exists(root.GetProperty("design").GetString()!));

        var rows = root.GetProperty("rows").EnumerateArray().ToList();
        Assert.Equal(rows.Count, rows.Select(row => row.GetProperty("id").GetString()).Distinct(StringComparer.Ordinal).Count());
        foreach (var row in rows)
        {
            var id = row.GetProperty("id").GetString()!;
            Assert.Matches(BacklogId, id);
            Assert.Matches(Date, row.GetProperty("found_on").GetString()!);
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("source").GetString()), id);
            Assert.False(string.IsNullOrWhiteSpace(row.GetProperty("summary").GetString()), id);
            Assert.Contains(row.GetProperty("classification").GetString(), new[] { "counts", "out-of-scope", "undecided" });
            Assert.Contains(row.GetProperty("needs_design").ValueKind, new[] { JsonValueKind.True, JsonValueKind.False });
            var status = row.GetProperty("status").GetString();
            Assert.Contains(status, new[] { "open", "closed" });
            Assert.Equal(status == "closed", row.GetProperty("closed_by").ValueKind == JsonValueKind.String);
            if (row.GetProperty("classification").GetString() == "out-of-scope")
            {
                Assert.Contains(row.GetProperty("out_of_scope_ref").GetString()!, scopeIds);
            }

            if (row.GetProperty("classification").GetString() != "undecided")
            {
                Assert.Equal(JsonValueKind.String, row.GetProperty("basis").ValueKind);
            }
        }
    }

    [Fact]
    public void PolicyCoversEveryGapKindAndStopsOnDesignAndUserDecisions()
    {
        using var policy = Load(PolicyPath);
        var root = policy.RootElement;

        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-completion-autopilot-policy-v1", root.GetProperty("contract").GetString());
        Assert.True(Exists(root.GetProperty("design").GetString()!));
        Assert.StartsWith("User-Approval: ", root.GetProperty("approval_locator").GetString(), StringComparison.Ordinal);

        var kinds = root.GetProperty("gap_kinds");
        Assert.Equal(
            GapKinds.Order(StringComparer.Ordinal),
            kinds.EnumerateObject().Select(kind => kind.Name).Order(StringComparer.Ordinal));
        Assert.False(kinds.GetProperty("new-design").GetProperty("auto").GetBoolean());
        Assert.False(kinds.GetProperty("user-decision").GetProperty("auto").GetBoolean());
        Assert.False(kinds.GetProperty("scenario").GetProperty("auto").GetBoolean());
        Assert.Contains(root.GetProperty("status").GetString(), new[] { "active", "paused" });
        if (root.GetProperty("status").GetString() == "paused")
        {
            Assert.True(Exists(root.GetProperty("paused_by").GetString()!));
        }
        foreach (var kind in kinds.EnumerateObject().Where(kind => kind.Value.GetProperty("auto").GetBoolean()))
        {
            var lanes = Strings(kind.Value.GetProperty("allowed_lanes"));
            Assert.All(lanes, lane => Assert.Contains(lane, new[] { "0", "1", "2", "3" }));
            Assert.Equal(lanes.Contains("2"), kind.Value.GetProperty("mutation_scope").GetArrayLength() > 0);
            Assert.Equal(lanes.Contains("3"), kind.Value.GetProperty("current_write_allowed").GetBoolean());
            Assert.Contains(kind.Value.GetProperty("merge_policy").GetString(), new[] { "none", "after-green-ci" });
        }

        Assert.Contains("pcv-guest-installed-04253-r1", Strings(root.GetProperty("preserved_vms")));
    }

    private static JsonDocument Load(string path) => JsonDocument.Parse(Repository.ReadUtf8Text(path));

    private static bool Exists(string repositoryRelativePath) =>
        File.Exists(Path.Combine(Repository.RootPath, repositoryRelativePath));

    private static string[] Strings(JsonElement array) =>
        array.EnumerateArray().Select(value => value.GetString()!).ToArray();
}
