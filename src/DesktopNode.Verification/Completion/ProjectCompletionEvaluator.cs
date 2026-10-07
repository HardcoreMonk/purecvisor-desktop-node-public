using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

internal sealed record CompletionCondition(string Id, bool Met, string Detail);

internal sealed record CompletionGap(
    string Id,
    string Condition,
    string Kind,
    string Lane,
    string Summary,
    IReadOnlyList<string> Refs,
    string? NotBefore);

internal sealed record CompletionCiRun(
    string WorkflowName,
    string Status,
    string Conclusion,
    string HeadSha,
    DateTimeOffset CreatedAt);

internal sealed record ProjectCompletionInputs(
    JsonObject Criteria,
    JsonObject CurrentEvidence,
    JsonObject ReleaseTrain,
    JsonObject Ledger,
    JsonObject Backlog,
    IReadOnlyList<CompletionCiRun> Runs,
    string HeadSha,
    DateOnly Today,
    Func<string, bool> RepositoryFileExists);

internal sealed record ProjectCompletionResult(
    IReadOnlyList<CompletionCondition> Conditions,
    IReadOnlyList<CompletionGap> Gaps)
{
    internal bool Complete => Conditions.All(condition => condition.Met);

    internal int MetCount => Conditions.Count(condition => condition.Met);
}

// Judges the completion definition pcv-project-completion-definition-v2 from repository files and a CI run list
// (design pcv-completion-autopilot-v1 §2.2). Pure: no process, network or clock access.
internal static class ProjectCompletionEvaluator
{
    internal const string CriteriaContract = "pcv-project-completion-criteria-v1";
    internal const string BacklogContract = "pcv-backlog-v1";
    internal const int ServicePlanItemCount = 15;

    internal static readonly IReadOnlyList<string> PermanentOutOfScopeIds =
    [
        "public-trusted-signing", "external-stable-publication", "workstation-parity-100",
        "hyperv-exactly-once-reconcile-completeness", "linux-kvm-stack",
    ];

    private static readonly Regex TrainVersion = new(@"^0\.(\d+)\.(\d+)-admin-smoke$", RegexOptions.CultureInvariant);

    internal static ProjectCompletionResult Evaluate(ProjectCompletionInputs inputs)
    {
        RequireContract(inputs.Criteria, CriteriaContract, "criteria");
        RequireContract(inputs.Backlog, BacklogContract, "backlog");

        var conditions = new List<CompletionCondition>();
        var gaps = new List<CompletionGap>();
        var workflows = Object(inputs.Criteria, "workflows", "criteria");
        var c1Workflows = Strings(workflows, "c1", "criteria.workflows");
        var c5Workflows = Strings(workflows, "c5", "criteria.workflows");

        conditions.Add(EvaluateWorkflows("C1", c1Workflows, [], inputs, gaps));
        conditions.Add(EvaluateC2(inputs, gaps));
        conditions.Add(EvaluateC3(inputs, gaps));
        conditions.Add(EvaluateC4(inputs, gaps));
        conditions.Add(EvaluateC5(c5Workflows, c1Workflows, inputs, gaps));
        conditions.Add(EvaluateC6(inputs));
        conditions.Add(EvaluateC7(inputs, gaps));
        return new ProjectCompletionResult(conditions, gaps);
    }

    internal static string NextTrainVersion(string version)
    {
        var match = TrainVersion.Match(version);
        if (!match.Success)
        {
            throw Invalid("version-invalid", version);
        }

        var patch = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) + 1;
        return $"0.{match.Groups[1].Value}.{patch}-admin-smoke";
    }

    private static CompletionCondition EvaluateWorkflows(
        string condition,
        IReadOnlyList<string> workflows,
        IReadOnlyList<string> reportedElsewhere,
        ProjectCompletionInputs inputs,
        List<CompletionGap> gaps)
    {
        var details = new List<string>();
        var met = true;
        foreach (var workflow in workflows)
        {
            var run = inputs.Runs
                .Where(candidate => candidate.WorkflowName == workflow && SameCommit(candidate.HeadSha, inputs.HeadSha))
                .OrderByDescending(candidate => candidate.CreatedAt)
                .FirstOrDefault();
            var state = run is null ? "missing" : run.Status != "completed" ? run.Status : run.Conclusion;
            details.Add($"{workflow}={state}");
            if (state == "success")
            {
                continue;
            }

            met = false;
            if (reportedElsewhere.Contains(workflow, StringComparer.Ordinal))
            {
                continue;
            }

            var waiting = run is null || run.Status != "completed";
            gaps.Add(new CompletionGap(
                $"{condition}-ci-{Slug(workflow)}",
                condition,
                waiting ? "ci-wait" : "lane1-fix",
                waiting ? "0" : "1",
                waiting
                    ? $"{workflow} push run for {Short(inputs.HeadSha)} is {state}; wait and judge again"
                    : $"{workflow} push run for {Short(inputs.HeadSha)} concluded {state}; fix main in Lane 1",
                [$"workflow:{workflow}"],
                null));
        }

        return new CompletionCondition(condition, met, string.Join(", ", details));
    }

    private static CompletionCondition EvaluateC2(ProjectCompletionInputs inputs, List<CompletionGap> gaps)
    {
        var current = String(Object(inputs.CurrentEvidence, "current", "current-evidence"), "version", "current-evidence.current");
        var operational = String(inputs.ReleaseTrain, "operational_current", "release-train");
        var queue = Array(inputs.ReleaseTrain, "queue", "release-train");
        var train = Array(inputs.ReleaseTrain, "trains", "release-train")
            .OfType<JsonObject>()
            .LastOrDefault(row => row["version"]?.GetValue<string>() == operational);
        var trainStatus = train?["status"]?.GetValue<string>() ?? "missing";

        if (current != operational)
        {
            gaps.Add(new CompletionGap(
                "C2-version", "C2", "user-decision", "0",
                $"current-evidence current {current} differs from release-train operational_current {operational}",
                [], null));
        }

        if (trainStatus != "promoted")
        {
            gaps.Add(new CompletionGap(
                "C2-train-status", "C2", "train-departure", "2",
                $"train {operational} is {trainStatus}, not promoted",
                [$"train:{operational}"], null));
        }

        if (queue.Count > 0)
        {
            var rows = queue.OfType<JsonObject>().ToList();
            var families = rows
                .Select(row => row["lane2_probe"]?["family"]?.GetValue<string>())
                .Where(family => !string.IsNullOrEmpty(family))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            gaps.Add(new CompletionGap(
                "C2-queue", "C2", "train-departure", "2",
                $"queue has {rows.Count} row(s); depart train {NextTrainVersion(operational)} with Lane 2 probe families " +
                (families.Count == 0 ? "none" : string.Join(", ", families)),
                rows.Select(row => $"pr:{row["pr"]?.ToJsonString()}").ToList(),
                null));
        }

        var met = current == operational && trainStatus == "promoted" && queue.Count == 0;
        return new CompletionCondition(
            "C2", met, $"current={current}, operational_current={operational}, train={trainStatus}, queue={queue.Count}");
    }

    private static CompletionCondition EvaluateC3(ProjectCompletionInputs inputs, List<CompletionGap> gaps)
    {
        var candidates = Array(inputs.Ledger, "features", "ledger")
            .OfType<JsonObject>()
            .Where(feature => feature["candidate_required"]?.GetValue<bool>() == true)
            .ToList();
        var failing = 0;
        foreach (var feature in candidates)
        {
            var id = String(feature, "feature_id", "ledger.features");
            var verdict = feature["current"]?["verdict"]?.GetValue<string>() ?? "missing";
            if (verdict == "pass")
            {
                continue;
            }

            failing++;
            gaps.Add(new CompletionGap(
                $"C3-{id}", "C3", "lane2-probe", "2",
                $"candidate feature {id} current verdict is {verdict}", [$"feature:{id}"], null));
        }

        var qualification = Object(inputs.CurrentEvidence, "feature_qualification", "current-evidence");
        var eligible = qualification["promotion_eligible"]?.GetValue<bool>() == true;
        var blockers = Array(qualification, "blockers", "current-evidence.feature_qualification").Count;
        if (!eligible || blockers > 0)
        {
            gaps.Add(new CompletionGap(
                "C3-promotion", "C3", "lane2-probe", "2",
                $"feature promotion_eligible={eligible.ToString().ToLowerInvariant()} with {blockers} blocker(s)",
                [], null));
        }

        return new CompletionCondition(
            "C3", failing == 0 && eligible && blockers == 0,
            $"candidates={candidates.Count}, pass={candidates.Count - failing}, promotion_eligible={eligible.ToString().ToLowerInvariant()}");
    }

    private static CompletionCondition EvaluateC4(ProjectCompletionInputs inputs, List<CompletionGap> gaps)
    {
        var items = Array(inputs.Criteria, "service_plan_items", "criteria").OfType<JsonObject>().ToList();
        if (items.Count != ServicePlanItemCount)
        {
            throw Invalid("service-plan-item-count", items.Count.ToString(CultureInfo.InvariantCulture));
        }

        var closed = 0;
        foreach (var item in items)
        {
            var id = String(item, "id", "criteria.service_plan_items");
            var evidence = Strings(item, "evidence_ids", $"criteria.{id}");
            var waiver = item["waiver"]?.GetValue<string>();
            var missing = evidence.Where(evidenceId => !inputs.RepositoryFileExists($"docs/ga-ready/evidence/{evidenceId}.md")).ToList();
            var waiverOk = waiver is not null && inputs.RepositoryFileExists(waiver);
            if ((evidence.Count > 0 && missing.Count == 0) || (evidence.Count == 0 && waiverOk))
            {
                closed++;
                continue;
            }

            gaps.Add(new CompletionGap(
                $"C4-{id}", "C4", "lane2-probe", "2",
                missing.Count > 0
                    ? $"{id} evidence not found: {string.Join(", ", missing)}"
                    : $"{id} has no installed actual-VM evidence or waiver",
                [$"service-plan:{id}"], null));
        }

        return new CompletionCondition("C4", closed == items.Count, $"items={items.Count}, closed={closed}");
    }

    private static CompletionCondition EvaluateC5(
        IReadOnlyList<string> workflows,
        IReadOnlyList<string> c1Workflows,
        ProjectCompletionInputs inputs,
        List<CompletionGap> gaps)
    {
        var ci = EvaluateWorkflows("C5", workflows, c1Workflows, inputs, gaps);
        var open = 0;
        foreach (var risk in Array(inputs.Criteria, "deadline_risks", "criteria").OfType<JsonObject>())
        {
            var id = String(risk, "id", "criteria.deadline_risks");
            if (String(risk, "status", $"criteria.{id}") != "open")
            {
                continue;
            }

            open++;
            var deadline = String(risk, "deadline", $"criteria.{id}");
            if (!DateOnly.TryParseExact(deadline, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var due))
            {
                throw Invalid("deadline-invalid", id);
            }

            var waiting = inputs.Today < due;
            gaps.Add(new CompletionGap(
                $"C5-risk-{id}", "C5", waiting ? "deadline-wait" : "lane1-fix", "1",
                waiting
                    ? $"deadline risk {id} stays open until {deadline}"
                    : $"deadline risk {id} reached {deadline}; check it and close the risk row or respond",
                [$"risk:{id}"], waiting ? deadline : null));
        }

        return new CompletionCondition("C5", ci.Met && open == 0, $"{ci.Detail}, open_deadline_risks={open}");
    }

    private static CompletionCondition EvaluateC6(ProjectCompletionInputs inputs)
    {
        var ids = Array(inputs.Criteria, "permanent_out_of_scope", "criteria")
            .OfType<JsonObject>()
            .Select(row => String(row, "id", "criteria.permanent_out_of_scope"))
            .ToHashSet(StringComparer.Ordinal);
        var missing = PermanentOutOfScopeIds.Where(id => !ids.Contains(id)).ToList();
        if (missing.Count > 0)
        {
            throw Invalid("permanent-out-of-scope-missing", string.Join(",", missing));
        }

        return new CompletionCondition("C6", true, $"permanent_out_of_scope={ids.Count}");
    }

    private static CompletionCondition EvaluateC7(ProjectCompletionInputs inputs, List<CompletionGap> gaps)
    {
        var scopeIds = Array(inputs.Criteria, "permanent_out_of_scope", "criteria")
            .OfType<JsonObject>()
            .Select(row => String(row, "id", "criteria.permanent_out_of_scope"))
            .ToHashSet(StringComparer.Ordinal);
        var counts = 0;
        var undecided = 0;
        foreach (var row in Array(inputs.Backlog, "rows", "backlog").OfType<JsonObject>())
        {
            var id = String(row, "id", "backlog.rows");
            if (String(row, "status", $"backlog.{id}") != "open")
            {
                continue;
            }

            var classification = String(row, "classification", $"backlog.{id}");
            var summary = String(row, "summary", $"backlog.{id}");
            if (classification == "out-of-scope" && scopeIds.Contains(row["out_of_scope_ref"]?.GetValue<string>() ?? string.Empty))
            {
                continue;
            }

            if (classification == "counts")
            {
                counts++;
                var design = row["needs_design"]?.GetValue<bool>() == true;
                gaps.Add(new CompletionGap(
                    $"C7-{id}", "C7", design ? "new-design" : "lane1-fix", "1", summary, [$"backlog:{id}"], null));
                continue;
            }

            undecided++;
            gaps.Add(new CompletionGap(
                $"C7-{id}", "C7", "user-decision", "0",
                $"classify {id} as counts or out-of-scope: {summary}", [$"backlog:{id}"], null));
        }

        return new CompletionCondition("C7", counts + undecided == 0, $"open_counts={counts}, undecided={undecided}");
    }

    private static bool SameCommit(string runSha, string head) =>
        head.Length >= 7 && runSha.StartsWith(head, StringComparison.OrdinalIgnoreCase);

    private static string Short(string sha) => sha.Length > 7 ? sha[..7] : sha;

    private static string Slug(string value) =>
        Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-", RegexOptions.CultureInvariant).Trim('-');

    private static void RequireContract(JsonObject root, string contract, string subject)
    {
        if (root["contract"]?.GetValue<string>() != contract)
        {
            throw Invalid("contract-mismatch", subject);
        }
    }

    private static JsonObject Object(JsonObject parent, string name, string subject) =>
        parent[name] as JsonObject ?? throw Invalid("object-missing", $"{subject}.{name}");

    private static JsonArray Array(JsonObject parent, string name, string subject) =>
        parent[name] as JsonArray ?? throw Invalid("array-missing", $"{subject}.{name}");

    private static string String(JsonObject parent, string name, string subject) =>
        parent[name] is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text)
            ? text
            : throw Invalid("string-missing", $"{subject}.{name}");

    private static List<string> Strings(JsonObject parent, string name, string subject) =>
        Array(parent, name, subject)
            .Select(value => value is JsonValue text && text.TryGetValue<string>(out var item) ? item : throw Invalid("string-missing", $"{subject}.{name}"))
            .ToList();

    internal static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"completion:{reason}:{subject}");
}
