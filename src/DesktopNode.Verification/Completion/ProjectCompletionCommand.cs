using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// pcvverify completion --ci-runs <gh run list JSON> [--head <commit>] [--today <yyyy-mm-dd>] [--output <artifacts/...json>]
// Read-only judgment of the completion definition the criteria file names (v3 since ADR-0017, design
// pcv-completion-autopilot-v1 §2.1). The CI run list
// comes from `gh run list --branch main --event push --json databaseId,workflowName,status,conclusion,headSha,createdAt`.
// Exit 0 when every gating condition is met, 1 when gaps remain, 2 on an input error. v3 hygiene lines never gate.
internal static class ProjectCompletionCommand
{
    internal const string Name = "completion";
    internal const string ResultContract = "pcv-project-completion-result-v1";
    internal const string Definition = ProjectCompletionEvaluator.DefinitionV3;

    internal const string CriteriaPath = "config/project-completion-criteria.json";
    internal const string CurrentEvidencePath = "docs/ga-ready/current-evidence.json";
    internal const string ReleaseTrainPath = "docs/ga-ready/release-train.json";
    internal const string LedgerPath = "config/desktop-node-feature-evidence-ledger.json";
    internal const string BacklogPath = "docs/ga-ready/backlog.json";

    private static readonly Regex CommitPattern = new("^[0-9a-f]{7,40}$", RegexOptions.CultureInvariant);

    private static readonly IReadOnlyList<string> AllowedExecutables = Array.AsReadOnly([
        "dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"
    ]);

    internal static int Run(
        IReadOnlyList<string> args,
        string currentDirectory,
        TextWriter standardOutput,
        IProcessRunner? processRunner = null)
    {
        try
        {
            var options = Parse(args);
            var repositoryRoot = RepositoryLocator.Find(currentDirectory);
            var head = options.Head ?? ResolveHead(repositoryRoot, processRunner ?? new SystemProcessRunner());
            var today = options.Today ?? DateOnly.FromDateTime(DateTime.Now);
            var inputs = new ProjectCompletionInputs(
                LoadObject(repositoryRoot, CriteriaPath),
                LoadObject(repositoryRoot, CurrentEvidencePath),
                LoadObject(repositoryRoot, ReleaseTrainPath),
                LoadObject(repositoryRoot, LedgerPath),
                LoadObject(repositoryRoot, BacklogPath),
                ParseRuns(ReadText(repositoryRoot, options.CiRuns, "ci-runs")),
                head,
                today,
                path => File.Exists(Path.Combine(repositoryRoot, path.Replace('/', Path.DirectorySeparatorChar))));
            var result = ProjectCompletionEvaluator.Evaluate(inputs);

            if (options.Output is not null)
            {
                var output = ArtifactPath(repositoryRoot, options.Output);
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                File.WriteAllText(output, Serialize(result, head, today).ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
            }

            foreach (var condition in result.Conditions)
            {
                standardOutput.WriteLine($"{condition.Id} met={Bool(condition.Met)} {condition.Detail}");
            }

            foreach (var gap in result.Gaps)
            {
                var notBefore = gap.NotBefore is null ? string.Empty : $" not_before={gap.NotBefore}";
                standardOutput.WriteLine($"gap {gap.Id} kind={gap.Kind} lane={gap.Lane}{notBefore} {gap.Summary}");
            }

            foreach (var condition in result.Hygiene)
            {
                standardOutput.WriteLine($"hygiene {condition.Id} met={Bool(condition.Met)} {condition.Detail}");
            }

            foreach (var gap in result.HygieneGaps)
            {
                standardOutput.WriteLine($"hygiene-gap {gap.Id} kind={gap.Kind} lane={gap.Lane} {gap.Summary}");
            }

            standardOutput.WriteLine(
                $"completion: complete={Bool(result.Complete)} met={result.MetCount}/{result.Conditions.Count} gaps={result.Gaps.Count} head={head}");
            return result.Complete ? 0 : 1;
        }
        catch (VerificationException exception)
        {
            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = false,
                ["error_code"] = exception.Code,
                ["error_detail"] = exception.Detail
            }));
            return 2;
        }
    }

    internal static JsonObject Serialize(ProjectCompletionResult result, string head, DateOnly today) => new()
    {
        ["schema_version"] = 1,
        ["contract"] = ResultContract,
        ["definition"] = result.Definition,
        ["head_sha"] = head,
        ["evaluated_on"] = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        ["complete"] = result.Complete,
        ["met_count"] = result.MetCount,
        ["condition_count"] = result.Conditions.Count,
        ["conditions"] = new JsonArray(result.Conditions.Select(condition => (JsonNode)new JsonObject
        {
            ["id"] = condition.Id,
            ["met"] = condition.Met,
            ["detail"] = condition.Detail
        }).ToArray()),
        ["gaps"] = new JsonArray(result.Gaps.Select(gap => (JsonNode)new JsonObject
        {
            ["id"] = gap.Id,
            ["condition"] = gap.Condition,
            ["kind"] = gap.Kind,
            ["lane"] = gap.Lane,
            ["summary"] = gap.Summary,
            ["refs"] = new JsonArray(gap.Refs.Select(reference => (JsonNode)JsonValue.Create(reference)!).ToArray()),
            ["not_before"] = gap.NotBefore
        }).ToArray()),
        ["hygiene"] = new JsonArray(result.Hygiene.Select(condition => (JsonNode)new JsonObject
        {
            ["id"] = condition.Id,
            ["met"] = condition.Met,
            ["detail"] = condition.Detail
        }).ToArray()),
        ["hygiene_gaps"] = new JsonArray(result.HygieneGaps.Select(gap => (JsonNode)new JsonObject
        {
            ["id"] = gap.Id,
            ["condition"] = gap.Condition,
            ["kind"] = gap.Kind,
            ["summary"] = gap.Summary
        }).ToArray())
    };

    internal static IReadOnlyList<CompletionCiRun> ParseRuns(string text)
    {
        JsonArray runs;
        try
        {
            runs = JsonNode.Parse(text) as JsonArray ?? throw ProjectCompletionEvaluator.Invalid("ci-runs-not-array", "ci-runs");
        }
        catch (JsonException)
        {
            throw ProjectCompletionEvaluator.Invalid("ci-runs-json", "ci-runs");
        }

        var parsed = new List<CompletionCiRun>();
        foreach (var run in runs.OfType<JsonObject>())
        {
            var runEvent = run["event"]?.GetValue<string>();
            if (runEvent is not null && runEvent != "push")
            {
                continue;
            }

            var created = run["createdAt"]?.GetValue<string>();
            if (run["workflowName"]?.GetValue<string>() is not { } workflow ||
                run["headSha"]?.GetValue<string>() is not { } sha ||
                !DateTimeOffset.TryParse(created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var createdAt))
            {
                throw ProjectCompletionEvaluator.Invalid("ci-run-invalid", "ci-runs");
            }

            parsed.Add(new CompletionCiRun(
                workflow,
                run["status"]?.GetValue<string>() ?? string.Empty,
                run["conclusion"]?.GetValue<string>() ?? string.Empty,
                sha,
                createdAt));
        }

        return parsed;
    }

    private static Options Parse(IReadOnlyList<string> args)
    {
        if (args.Count < 3 || args[0] != Name || (args.Count - 1) % 2 != 0)
        {
            throw ProjectCompletionEvaluator.Invalid("cli-invalid", "usage");
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 1; index < args.Count; index += 2)
        {
            if (args[index] is not ("--ci-runs" or "--head" or "--today" or "--output") || !values.TryAdd(args[index], args[index + 1]))
            {
                throw ProjectCompletionEvaluator.Invalid("cli-invalid", args[index]);
            }
        }

        if (!values.TryGetValue("--ci-runs", out var ciRuns))
        {
            throw ProjectCompletionEvaluator.Invalid("cli-invalid", "--ci-runs");
        }

        string? head = null;
        if (values.TryGetValue("--head", out var headValue))
        {
            head = CommitPattern.IsMatch(headValue) ? headValue : throw ProjectCompletionEvaluator.Invalid("cli-invalid", "--head");
        }

        DateOnly? today = null;
        if (values.TryGetValue("--today", out var todayValue))
        {
            today = DateOnly.TryParseExact(todayValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : throw ProjectCompletionEvaluator.Invalid("cli-invalid", "--today");
        }

        return new Options(ciRuns, head, today, values.GetValueOrDefault("--output"));
    }

    private static string ResolveHead(string repositoryRoot, IProcessRunner processRunner)
    {
        var result = processRunner.RunAsync(
            new ProcessInvocation(
                "completion-git-head",
                "git",
                ["rev-parse", "HEAD"],
                repositoryRoot,
                TimeSpan.FromSeconds(30),
                AllowedExecutables,
                OutputLimitCharacters: 1_000),
            CancellationToken.None).GetAwaiter().GetResult();
        var head = result.StandardOutput.Trim();
        if (result.TimedOut || result.Cancelled || result.ExitCode != 0 || !CommitPattern.IsMatch(head))
        {
            throw ProjectCompletionEvaluator.Invalid("git-head-failed", "HEAD");
        }

        return head;
    }

    private static JsonObject LoadObject(string repositoryRoot, string path)
    {
        try
        {
            return JsonNode.Parse(ReadText(repositoryRoot, path, path)) as JsonObject
                ?? throw ProjectCompletionEvaluator.Invalid("json-not-object", path);
        }
        catch (JsonException)
        {
            throw ProjectCompletionEvaluator.Invalid("json-invalid", path);
        }
    }

    private static string ReadText(string repositoryRoot, string path, string subject)
    {
        var full = Path.GetFullPath(Path.Combine(repositoryRoot, path));
        if (!File.Exists(full))
        {
            throw ProjectCompletionEvaluator.Invalid("file-missing", subject);
        }

        return File.ReadAllText(full);
    }

    private static string ArtifactPath(string repositoryRoot, string path)
    {
        var artifacts = Path.GetFullPath(Path.Combine(repositoryRoot, "artifacts")) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(repositoryRoot, path));
        return full.StartsWith(artifacts, StringComparison.OrdinalIgnoreCase) && full.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? full
            : throw ProjectCompletionEvaluator.Invalid("output-outside-artifacts", path);
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private sealed record Options(string CiRuns, string? Head, DateOnly? Today, string? Output);
}
