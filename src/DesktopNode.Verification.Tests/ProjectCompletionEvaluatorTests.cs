using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

// design pcv-completion-autopilot-v1 §2.2: each condition's verdict and gap kind, built from the committed input files
// and edited per case so that no case depends on today's repository state.
public sealed class ProjectCompletionEvaluatorTests
{
    private const string Head = "3f55831c2aa51400823fec295a51eed5954dcb4f";
    private static readonly DateOnly Today = new(2026, 10, 7);

    [Fact]
    public void ClearedInputsMeetAllSevenConditions()
    {
        var result = ProjectCompletionEvaluator.Evaluate(Cleared());

        Assert.True(result.Complete);
        Assert.Equal(7, result.MetCount);
        Assert.Equal(["C1", "C2", "C3", "C4", "C5", "C6", "C7"], result.Conditions.Select(condition => condition.Id));
        Assert.Empty(result.Gaps);
    }

    [Fact]
    public void QueueRowsBecomeATrainDepartureForTheNextPatchVersion()
    {
        var inputs = Cleared();
        inputs.ReleaseTrain["queue"] = new JsonArray(new JsonObject
        {
            ["pr"] = 59,
            ["merge_commit"] = "ee90474",
            ["lane2_probe"] = new JsonObject { ["family"] = "vm.create" }
        });

        var operational = inputs.ReleaseTrain["operational_current"]!.GetValue<string>();

        var gap = Assert.Single(ProjectCompletionEvaluator.Evaluate(inputs).Gaps);

        Assert.Equal(("C2-queue", "train-departure", "2"), (gap.Id, gap.Kind, gap.Lane));
        Assert.Equal("0.42.92-admin-smoke", ProjectCompletionEvaluator.NextTrainVersion("0.42.91-admin-smoke"));
        Assert.Contains(ProjectCompletionEvaluator.NextTrainVersion(operational), gap.Summary, StringComparison.Ordinal);
        Assert.Contains("vm.create", gap.Summary, StringComparison.Ordinal);
        Assert.Equal(["pr:59"], gap.Refs);
    }

    [Fact]
    public void VersionMismatchIsAUserDecisionAndAnUnpromotedTrainDeparts()
    {
        var inputs = Cleared();
        inputs.CurrentEvidence["current"]!["version"] = "0.42.90-admin-smoke";
        var operational = inputs.ReleaseTrain["operational_current"]!.GetValue<string>();
        inputs.ReleaseTrain["trains"]!.AsArray().OfType<JsonObject>()
            .Last(train => train["version"]!.GetValue<string>() == operational)["status"] = "departed";

        var result = ProjectCompletionEvaluator.Evaluate(inputs);

        Assert.False(Condition(result, "C2").Met);
        Assert.Equal(
            [("C2-version", "user-decision"), ("C2-train-status", "train-departure")],
            result.Gaps.Select(gap => (gap.Id, gap.Kind)));
    }

    [Theory]
    [InlineData(null, null, "ci-wait", "0")]
    [InlineData("in_progress", "", "ci-wait", "0")]
    [InlineData("completed", "failure", "lane1-fix", "1")]
    public void DevelopmentGatesRunStateDecidesC1AndIsReportedOnce(
        string? status, string? conclusion, string kind, string lane)
    {
        var runs = new List<CompletionCiRun> { Run("Public Boundary Contract", "completed", "success") };
        if (status is not null)
        {
            runs.Add(Run("Development Gates", status, conclusion!));
        }

        var result = ProjectCompletionEvaluator.Evaluate(Cleared() with { Runs = runs });

        Assert.False(Condition(result, "C1").Met);
        Assert.False(Condition(result, "C5").Met);
        var gap = Assert.Single(result.Gaps);
        Assert.Equal(("C1-ci-development-gates", kind, lane), (gap.Id, gap.Kind, gap.Lane));
    }

    [Fact]
    public void LatestRunForTheHeadWinsAndOtherCommitsAreIgnored()
    {
        var runs = new List<CompletionCiRun>
        {
            Run("Development Gates", "completed", "failure", createdAt: "2026-10-06T18:00:00Z"),
            Run("Development Gates", "completed", "success", createdAt: "2026-10-06T19:00:00Z"),
            Run("Public Boundary Contract", "completed", "success"),
            Run("Public Boundary Contract", "completed", "failure", sha: "0000000000000000000000000000000000000000"),
        };

        var result = ProjectCompletionEvaluator.Evaluate(Cleared() with { Runs = runs, HeadSha = Head[..7] });

        Assert.True(result.Complete);
    }

    [Fact]
    public void PublicBoundaryFailureIsAC5Gap()
    {
        var runs = new List<CompletionCiRun>
        {
            Run("Development Gates", "completed", "success"),
            Run("Public Boundary Contract", "completed", "cancelled"),
        };

        var result = ProjectCompletionEvaluator.Evaluate(Cleared() with { Runs = runs });

        Assert.True(Condition(result, "C1").Met);
        var gap = Assert.Single(result.Gaps);
        Assert.Equal(("C5-ci-public-boundary-contract", "C5", "lane1-fix"), (gap.Id, gap.Condition, gap.Kind));
    }

    [Theory]
    [InlineData("2026-10-18", "deadline-wait", "2026-10-19")]
    [InlineData("2026-10-19", "lane1-fix", null)]
    [InlineData("2026-10-25", "lane1-fix", null)]
    public void OpenDeadlineRiskWaitsUntilItsDateThenNeedsACheck(string today, string kind, string? notBefore)
    {
        var inputs = Cleared() with { Today = DateOnly.Parse(today, System.Globalization.CultureInfo.InvariantCulture) };
        var risk = Risk(inputs);
        risk["status"] = "open";
        risk["closed_by"] = null;

        var result = ProjectCompletionEvaluator.Evaluate(inputs);

        Assert.False(Condition(result, "C5").Met);
        var gap = Assert.Single(result.Gaps);
        Assert.Equal(("C5-risk-ubuntu-26-runner", kind, notBefore), (gap.Id, gap.Kind, gap.NotBefore));
    }

    [Fact]
    public void BacklogClassificationsMapToDecisionDesignAndFixGaps()
    {
        var inputs = Cleared();
        inputs.Backlog["rows"] = new JsonArray(
            Row("BL-0001", "undecided", needsDesign: true),
            Row("BL-0002", "counts", needsDesign: true),
            Row("BL-0003", "counts", needsDesign: false),
            Row("BL-0004", "out-of-scope", needsDesign: false, outOfScopeRef: "hyperv-exactly-once-reconcile-completeness"),
            Row("BL-0005", "out-of-scope", needsDesign: false, outOfScopeRef: "not-a-scope-id"),
            Row("BL-0006", "counts", needsDesign: false, status: "closed"));

        var result = ProjectCompletionEvaluator.Evaluate(inputs);

        Assert.False(Condition(result, "C7").Met);
        Assert.Equal(
            [
                ("C7-BL-0001", "user-decision"), ("C7-BL-0002", "new-design"), ("C7-BL-0003", "lane1-fix"),
                ("C7-BL-0005", "user-decision"),
            ],
            result.Gaps.Select(gap => (gap.Id, gap.Kind)));
    }

    [Fact]
    public void MissingPermanentScopeOrWrongContractIsAnInputError()
    {
        var scope = Cleared();
        scope.Criteria["permanent_out_of_scope"]!.AsArray().RemoveAt(0);
        var contract = Cleared();
        contract.Backlog["contract"] = "pcv-backlog-v0";

        Assert.Equal(
            "completion:permanent-out-of-scope-missing:public-trusted-signing",
            Assert.Throws<VerificationException>(() => ProjectCompletionEvaluator.Evaluate(scope)).Detail);
        Assert.Equal(
            "completion:contract-mismatch:backlog",
            Assert.Throws<VerificationException>(() => ProjectCompletionEvaluator.Evaluate(contract)).Detail);
    }

    [Fact]
    public void RunParsingSkipsNonPushEventsAndRejectsIncompleteRows()
    {
        var runs = ProjectCompletionCommand.ParseRuns(
            """
            [
              {"workflowName":"Development Gates","status":"completed","conclusion":"success","headSha":"abc1234","createdAt":"2026-10-06T18:26:59Z","event":"push"},
              {"workflowName":"Development Gates","status":"completed","conclusion":"failure","headSha":"abc1234","createdAt":"2026-10-06T19:26:59Z","event":"pull_request"},
              {"workflowName":"Public Boundary Contract","status":"in_progress","conclusion":"","headSha":"abc1234","createdAt":"2026-10-06T18:26:59Z"}
            ]
            """);

        Assert.Equal(["success", ""], runs.Select(run => run.Conclusion));
        Assert.Equal(
            "completion:ci-run-invalid:ci-runs",
            Assert.Throws<VerificationException>(() => ProjectCompletionCommand.ParseRuns("""[{"workflowName":"x"}]""")).Detail);
    }

    [Fact]
    public void CommandWritesTheResultUnderArtifactsAndRefusesOtherOutputs()
    {
        var directory = Path.Combine(VerificationCatalogFixture.RepositoryRoot, "artifacts", "completion-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var runs = Path.Combine(directory, "runs.json");
            File.WriteAllText(runs, "[]");
            var relativeRuns = Path.GetRelativePath(VerificationCatalogFixture.RepositoryRoot, runs);
            var relativeOutput = Path.GetRelativePath(VerificationCatalogFixture.RepositoryRoot, Path.Combine(directory, "result.json"));
            using var output = new StringWriter();

            var code = ProjectCompletionCommand.Run(
                ["completion", "--ci-runs", relativeRuns, "--head", Head, "--today", "2026-10-07", "--output", relativeOutput],
                VerificationCatalogFixture.RepositoryRoot,
                output);

            Assert.Equal(1, code);
            var result = JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "result.json")))!.AsObject();
            Assert.Equal("pcv-project-completion-result-v1", result["contract"]!.GetValue<string>());
            Assert.Equal(7, result["condition_count"]!.GetValue<int>());
            Assert.Contains(result["gaps"]!.AsArray(), gap => gap!["id"]!.GetValue<string>() == "C1-ci-development-gates");
            Assert.StartsWith(
                $"completion: complete=false met={result["met_count"]!.GetValue<int>()}/7 gaps=",
                output.ToString().TrimEnd().Split('\n')[^1],
                StringComparison.Ordinal);

            using var refused = new StringWriter();
            Assert.Equal(2, ProjectCompletionCommand.Run(
                ["completion", "--ci-runs", relativeRuns, "--head", Head, "--output", "docs/result.json"],
                VerificationCatalogFixture.RepositoryRoot,
                refused));
            Assert.Contains("completion:output-outside-artifacts:docs/result.json", refused.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(new[] { "completion" }, "completion:cli-invalid:usage")]
    [InlineData(new[] { "completion", "--head", Head }, "completion:cli-invalid:--ci-runs")]
    [InlineData(new[] { "completion", "--ci-runs", "x.json", "--today", "10/07/2026" }, "completion:cli-invalid:--today")]
    [InlineData(new[] { "completion", "--ci-runs", "x.json", "--write", "y" }, "completion:cli-invalid:--write")]
    [InlineData(new[] { "completion", "--ci-runs", "artifacts/missing-runs.json", "--head", Head }, "completion:file-missing:ci-runs")]
    public void CommandRejectsInvalidArgumentsWithExitTwo(string[] args, string detail)
    {
        using var output = new StringWriter();

        Assert.Equal(2, ProjectCompletionCommand.Run(args, VerificationCatalogFixture.RepositoryRoot, output));
        Assert.Contains(detail, output.ToString(), StringComparison.Ordinal);
    }

    internal static ProjectCompletionInputs Cleared()
    {
        var inputs = new ProjectCompletionInputs(
            Load(ProjectCompletionCommand.CriteriaPath),
            Load(ProjectCompletionCommand.CurrentEvidencePath),
            Load(ProjectCompletionCommand.ReleaseTrainPath),
            Load(ProjectCompletionCommand.LedgerPath),
            Load(ProjectCompletionCommand.BacklogPath),
            [Run("Development Gates", "completed", "success"), Run("Public Boundary Contract", "completed", "success")],
            Head,
            Today,
            path => File.Exists(Path.Combine(VerificationCatalogFixture.RepositoryRoot, path)));
        inputs.ReleaseTrain["queue"] = new JsonArray();
        inputs.Backlog["rows"] = new JsonArray();
        var risk = Risk(inputs);
        risk["status"] = "closed";
        risk["closed_by"] = "run:1";
        return inputs;
    }

    internal static CompletionCondition Condition(ProjectCompletionResult result, string id) =>
        result.Conditions.Single(condition => condition.Id == id);

    private static JsonObject Risk(ProjectCompletionInputs inputs) =>
        inputs.Criteria["deadline_risks"]!.AsArray().OfType<JsonObject>().Single(risk => risk["id"]!.GetValue<string>() == "ubuntu-26-runner");

    private static JsonObject Row(
        string id, string classification, bool needsDesign, string? outOfScopeRef = null, string status = "open") => new()
    {
        ["id"] = id,
        ["found_on"] = "2026-10-07",
        ["source"] = "test",
        ["summary"] = $"summary {id}",
        ["classification"] = classification,
        ["basis"] = classification == "undecided" ? null : "test decision",
        ["out_of_scope_ref"] = outOfScopeRef,
        ["needs_design"] = needsDesign,
        ["status"] = status,
        ["closed_by"] = status == "closed" ? "pr:1" : null
    };

    private static CompletionCiRun Run(
        string workflow, string status, string conclusion, string sha = Head, string createdAt = "2026-10-06T18:26:59Z") =>
        new(workflow, status, conclusion, sha, DateTimeOffset.Parse(createdAt, System.Globalization.CultureInfo.InvariantCulture));

    private static JsonObject Load(string path) =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(VerificationCatalogFixture.RepositoryRoot, path)))!.AsObject();
}
