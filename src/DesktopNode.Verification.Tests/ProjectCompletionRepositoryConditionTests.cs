using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

// design pcv-completion-autopilot-v1 §2.2: C1 workflow list, C3 feature ledger and C4 SERVICE_PLAN evidence verdicts.
public sealed class ProjectCompletionRepositoryConditionTests
{
    [Fact]
    public void C1FollowsTheCriteriaWorkflowList()
    {
        var inputs = ProjectCompletionEvaluatorTests.Cleared();
        inputs.Criteria["workflows"]!["c1"]!.AsArray().Add("Release Gates");

        var result = ProjectCompletionEvaluator.Evaluate(inputs);

        Assert.False(ProjectCompletionEvaluatorTests.Condition(result, "C1").Met);
        Assert.True(ProjectCompletionEvaluatorTests.Condition(result, "C5").Met);
        var gap = Assert.Single(result.Gaps);
        Assert.Equal(("C1-ci-release-gates", "ci-wait"), (gap.Id, gap.Kind));
    }

    [Fact]
    public void C3NeedsEveryCandidateFeatureToPassAndPromotionToBeEligible()
    {
        var inputs = ProjectCompletionEvaluatorTests.Cleared();
        var features = inputs.Ledger["features"]!.AsArray().OfType<JsonObject>().ToList();
        features[0]["current"]!["verdict"] = "fail";
        features[1]["candidate_required"] = false;
        features[1]["current"]!["verdict"] = "fail";
        inputs.CurrentEvidence["feature_qualification"]!["promotion_eligible"] = false;

        var result = ProjectCompletionEvaluator.Evaluate(inputs);

        Assert.False(ProjectCompletionEvaluatorTests.Condition(result, "C3").Met);
        Assert.Equal(
            [($"C3-{features[0]["feature_id"]!.GetValue<string>()}", "lane2-probe"), ("C3-promotion", "lane2-probe")],
            result.Gaps.Select(gap => (gap.Id, gap.Kind)));
    }

    [Fact]
    public void C4NeedsResolvableEvidenceOrAnExistingWaiver()
    {
        var inputs = ProjectCompletionEvaluatorTests.Cleared();
        var items = inputs.Criteria["service_plan_items"]!.AsArray().OfType<JsonObject>().ToList();
        items.Single(item => item["id"]!.GetValue<string>() == "P0-2")["evidence_ids"] = new JsonArray("no-such-evidence-2026-10-07");
        items.Single(item => item["id"]!.GetValue<string>() == "P1-9")["waiver"] = "docs/no-such-decision.md";
        var p26 = items.Single(item => item["id"]!.GetValue<string>() == "P1-6");
        p26["evidence_ids"] = new JsonArray();
        p26["waiver"] = null;

        var result = ProjectCompletionEvaluator.Evaluate(inputs);

        Assert.False(ProjectCompletionEvaluatorTests.Condition(result, "C4").Met);
        Assert.Equal("items=15, closed=12", ProjectCompletionEvaluatorTests.Condition(result, "C4").Detail);
        Assert.Equal(["C4-P0-2", "C4-P1-6", "C4-P1-9"], result.Gaps.Select(gap => gap.Id));
        Assert.All(result.Gaps, gap => Assert.Equal(("lane2-probe", "2"), (gap.Kind, gap.Lane)));
        Assert.Contains("no-such-evidence-2026-10-07", result.Gaps[0].Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void C4RejectsAnItemListThatIsNotFifteenLong()
    {
        var inputs = ProjectCompletionEvaluatorTests.Cleared();
        inputs.Criteria["service_plan_items"]!.AsArray().RemoveAt(14);

        Assert.Equal(
            "completion:service-plan-item-count:14",
            Assert.Throws<VerificationException>(() => ProjectCompletionEvaluator.Evaluate(inputs)).Detail);
    }

    [Fact]
    public void CommandWithoutOptionsReportsAnErrorInTheResultContract()
    {
        using var output = new StringWriter();

        var code = ProjectCompletionCommand.Run(["completion"], VerificationCatalogFixture.RepositoryRoot, output);

        Assert.Equal(2, code);
        Assert.Contains("\"contract\":\"pcv-project-completion-result-v1\"", output.ToString(), StringComparison.Ordinal);
    }
}
