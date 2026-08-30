using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

internal sealed class DevelopmentPolicyContractVerifier
{
    internal const string SpecPath = "config/pcv-development-policy-contract-spec-v1.json";

    private const string ExpectedSpecSha256 =
        "f19b1b91dd4ec47a0031c870ec3538e27084869009eb752a2823e83a396a78ab";

    private static readonly string[] ExpectedKeys =
    [
        "agent-circuit-breaker",
        "architecture-gap",
        "gate-workflow",
        "verification-policy",
        "verification-execution",
        "quality-tools",
        "module-ratchet",
        "strict-collection",
    ];

    private static readonly int[] ExpectedCounts = [3, 10, 1, 9, 3, 20, 3, 2];

    private static readonly string[] ExpectedSuiteIds =
    [
        "dotnet",
        "web-typecheck",
        "web-parity",
        "delivery-contracts",
        "installer-contracts",
        "evidence-check",
        "policy-boundaries",
    ];

    private static readonly string[] ExpectedAllowedExecutables =
    [
        "dotnet",
        "dotnet.exe",
        "node",
        "node.exe",
        "npm",
        "npm.cmd",
        "git",
        "git.exe",
    ];

    private static readonly HashSet<string> StructuredTransitionSources = new(
    [
        ".github/workflows/development-gates.yml",
        "config/development-verification-suites.json",
        "config/development-verification-suites.schema.json",
        "src/DesktopNode.Verification/VerificationCatalog.cs",
    ],
    StringComparer.Ordinal);

    private static readonly Lazy<DevelopmentPolicyContractVerifier> Default =
        new(() => new DevelopmentPolicyContractVerifier(),
            LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly RepositoryContractContext repository;
    private readonly DevelopmentPolicySpec spec;
    private readonly Dictionary<string, string> sourceTexts;
    private readonly string combinedSource;
    private readonly Lazy<bool> binding;

    private DevelopmentPolicyContractVerifier()
    {
        repository = RepositoryContractContext.Find();
        spec = LoadSpec();
        ValidateSpec();
        sourceTexts = spec.SourceFiles.ToDictionary(
            source => source.Path,
            source => repository.ReadUtf8Text(source.Path),
            StringComparer.Ordinal);
        combinedSource = string.Join("\n", sourceTexts.Values);
        binding = new Lazy<bool>(
            ValidateBinding,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal static void Verify(string key, int ordinal) =>
        Default.Value.VerifyContract(key, ordinal);

    internal void VerifyContract(string key, int ordinal)
    {
        _ = binding.Value;
        var contract = spec.Contracts.SingleOrDefault(candidate =>
            candidate.Key == key && candidate.Ordinal == ordinal);
        if (contract is null)
        {
            throw Invalid("ordinal");
        }

        foreach (var literal in contract.RequiredLiterals)
        {
            if (!combinedSource.Contains(literal, StringComparison.Ordinal))
            {
                throw Invalid($"literal-{key}-{ordinal:D3}");
            }
        }
    }

    internal static void ValidateWorkflowText(string workflow)
    {
        RequireTokens(
            workflow,
            "workflow-required",
            "name: Development Gates",
            "pull_request:",
            "push:",
            "workflow_dispatch:",
            "contents: read",
            "cancel-in-progress: true",
            "dotnet-version: 10.0.x",
            "node-version: 24");

        var active = workflow.Contains("  dotnet:\n", StringComparison.Ordinal) ||
            workflow.Contains("  dotnet:\r\n", StringComparison.Ordinal);
        string[] jobNames;
        if (active)
        {
            RequireTokens(
                workflow,
                "workflow-active",
                "dotnet:",
                "web:",
                "delivery:",
                "installer-policy:",
                "Run dotnet shard",
                "Run web shard",
                "Run delivery shard",
                "Run installer and policy shard");
            if (Regex.IsMatch(workflow, "(?i)pwsh|powershell|Invoke-Pester") ||
                Count(workflow, "RequiredVersion 5.7.1") != 0)
            {
                throw Invalid("workflow-active-shell");
            }
            jobNames =
            [
                "  dotnet:",
                "  web:",
                "  delivery:",
                "  installer-policy:",
            ];
        }
        else
        {
            RequireTokens(
                workflow,
                "workflow-legacy-shadow",
                "dotnet-tests:",
                "web-tests:",
                "packaging-pester:",
                "installer-web-pester:",
                "dotnet restore src/DesktopNode.sln",
                "dotnet test src/DesktopNode.sln -c Release --no-restore",
                "npm run verify:parity --prefix web",
                "Invoke-PcvDevelopmentVerification.ps1");
            if (Count(workflow, "RequiredVersion 5.7.1") != 2)
            {
                throw Invalid("workflow-cardinality");
            }
            jobNames =
            [
                "  dotnet-tests:",
                "  web-tests:",
                "  packaging-pester:",
                "  installer-web-pester:",
            ];
        }

        if (Count(workflow, "runs-on: windows-latest") != 3 ||
            Count(workflow, "runs-on: ubuntu-latest") != 1 ||
            Regex.Matches(workflow, "timeout-minutes:\\s*\\d+").Count != 4)
        {
            throw Invalid("workflow-cardinality");
        }

        RequireOrdered(workflow, "workflow-job-order", jobNames);

        if (Regex.IsMatch(
                workflow,
                "(?i)AllowHostMutation|msiexec|Start-VM|New-VM|" +
                "(?:New|Set|Start|Stop|Restart|Remove)-Service|" +
                "SignTool|Create-Release|gh\\s+release|deploy"))
        {
            throw Invalid("workflow-mutation");
        }
    }

    internal static void ValidateSuiteCatalog(
        IReadOnlyList<DevelopmentSuiteContract> suites,
        IReadOnlyList<string> allowedExecutables)
    {
        if (!allowedExecutables.SequenceEqual(
                ExpectedAllowedExecutables,
                StringComparer.Ordinal) ||
            allowedExecutables.Distinct(StringComparer.OrdinalIgnoreCase).Count() !=
                allowedExecutables.Count)
        {
            throw Invalid("suite-executables");
        }

        if (suites.Count != ExpectedSuiteIds.Length ||
            suites.Select(suite => suite.Id).Distinct(StringComparer.Ordinal).Count() !=
                suites.Count ||
            !suites.Select(suite => suite.Id).SequenceEqual(
                ExpectedSuiteIds,
                StringComparer.Ordinal))
        {
            throw Invalid("suite-identity");
        }

        foreach (var suite in suites)
        {
            if (suite.Kind is not ("process" or "managed") ||
                suite.TimeoutSeconds is < 1 or > 900)
            {
                throw Invalid("suite-shape");
            }

            if (suite.Kind == "process" &&
                (string.IsNullOrWhiteSpace(suite.FileName) ||
                 !allowedExecutables.Contains(suite.FileName, StringComparer.OrdinalIgnoreCase)))
            {
                throw Invalid("suite-executable-forbidden");
            }

            if (suite.Kind == "managed" && !string.IsNullOrWhiteSpace(suite.FileName))
            {
                throw Invalid("suite-managed-process");
            }
        }
    }

    internal static void ValidateModuleRatchet(
        int actualLines,
        int proposedMaximum,
        int recordedMaximum,
        int slackLines)
    {
        if (actualLines < 1 || proposedMaximum < 1 || recordedMaximum < 1 || slackLines < 1)
        {
            throw Invalid("module-ratchet-shape");
        }

        if (proposedMaximum > recordedMaximum)
        {
            throw Invalid("module-ratchet-widened");
        }

        if (actualLines > proposedMaximum)
        {
            throw Invalid("module-ratchet-exceeded");
        }

        if (proposedMaximum - actualLines >= slackLines)
        {
            throw Invalid("module-ratchet-stale");
        }
    }

    internal static void ValidateToolVersions(
        IReadOnlyDictionary<string, string> versions)
    {
        string[] expected = ["dotnet_sdk_version", "coverage_collector_version"];
        if (!versions.Keys.Order(StringComparer.Ordinal).SequenceEqual(
                expected.Order(StringComparer.Ordinal),
                StringComparer.Ordinal) ||
            versions.Values.Any(string.IsNullOrWhiteSpace) ||
            versions.Values.Any(value =>
                !Regex.IsMatch(value, "^[0-9]+(?:\\.[0-9]+){1,3}(?:[-+][0-9A-Za-z.-]+)?$")))
        {
            throw Invalid("quality-tool-version");
        }
    }

    private DevelopmentPolicySpec LoadSpec()
    {
        var text = repository.ReadUtf8Text(SpecPath);
        if (Hash(text) != ExpectedSpecSha256)
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var json = JsonContract.Parse(SpecPath, text);
            return JsonSerializer.Deserialize<DevelopmentPolicySpec>(
                    json.Root.GetRawText(),
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = false,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    })
                ?? throw Invalid("spec-json");
        }
        catch (JsonException error)
        {
            throw Invalid("spec-json", error);
        }
        catch (NotSupportedException error)
        {
            throw Invalid("spec-json", error);
        }
    }

    private void ValidateSpec()
    {
        if (spec.Contract != "pcv-development-policy-contract-spec-v1" ||
            spec.LegacyContractCount != 51 ||
            spec.LegacyShouldSiteCount != 371 ||
            spec.RequiredLiteralCount != 652 ||
            spec.SourceFiles.Count != 17 ||
            spec.LegacyFiles.Count != 8 ||
            spec.Contracts.Count != 51 ||
            spec.SourceFiles.Select(source => source.Path)
                .Distinct(StringComparer.Ordinal).Count() != 17)
        {
            throw Invalid("spec-contract");
        }

        var contractIndex = 0;
        var literalCount = 0;
        for (var fileIndex = 0; fileIndex < spec.LegacyFiles.Count; fileIndex++)
        {
            var file = spec.LegacyFiles[fileIndex];
            if (file.Key != ExpectedKeys[fileIndex] ||
                file.ContractCount != ExpectedCounts[fileIndex] ||
                file.ShouldSiteCount < 1 ||
                file.Sha256.Length != 64)
            {
                throw Invalid("spec-file-order");
            }

            for (var ordinal = 1; ordinal <= file.ContractCount; ordinal++)
            {
                var contract = spec.Contracts[contractIndex++];
                if (contract.Key != file.Key ||
                    contract.Ordinal != ordinal ||
                    string.IsNullOrWhiteSpace(contract.Name) ||
                    contract.RequiredLiterals.Count == 0 ||
                    contract.RequiredLiterals.Any(string.IsNullOrWhiteSpace) ||
                    contract.RequiredLiterals.Distinct(StringComparer.Ordinal).Count() !=
                        contract.RequiredLiterals.Count)
                {
                    throw Invalid("spec-contract-order");
                }

                literalCount += contract.RequiredLiterals.Count;
            }
        }

        if (contractIndex != spec.Contracts.Count ||
            literalCount != spec.RequiredLiteralCount ||
            spec.LegacyFiles.Sum(file => file.ShouldSiteCount) !=
                spec.LegacyShouldSiteCount)
        {
            throw Invalid("spec-count");
        }
    }

    private bool ValidateBinding()
    {
        foreach (var source in spec.SourceFiles)
        {
            if (!sourceTexts.TryGetValue(source.Path, out var text) ||
                (Hash(text) != source.Sha256 &&
                 !StructuredTransitionSources.Contains(source.Path)))
            {
                throw Invalid("source-sha");
            }
        }

        foreach (var file in spec.LegacyFiles)
        {
            var legacy = repository.ReadUtf8Text(file.Path);
            if (Hash(legacy) != file.Sha256)
            {
                throw Invalid("legacy-sha");
            }

            var parsed = LegacyPesterContractParser.Parse(file.Path, legacy);
            var expected = spec.Contracts.Where(contract => contract.Key == file.Key).ToArray();
            if (parsed.Count != expected.Length)
            {
                throw Invalid("legacy-count");
            }

            for (var index = 0; index < parsed.Count; index++)
            {
                if (parsed[index].Ordinal != expected[index].Ordinal ||
                    parsed[index].Name != expected[index].Name)
                {
                    throw Invalid("legacy-order");
                }
            }
        }

        ValidateCircuitBreaker();
        ValidateArchitectureRegistry();
        ValidateWorkflowText(Source(".github/workflows/development-gates.yml"));
        ValidateDevelopmentVerificationSources();
        ValidateManagedSuiteCatalog();
        ValidateQualitySources();
        ValidateModuleSizeFixture();
        ValidateStrictCollectionSource();
        return true;
    }

    private void ValidateCircuitBreaker()
    {
        using var json = JsonContract.Parse(
            "config/agent-execution-circuit-breaker.json",
            Source("config/agent-execution-circuit-breaker.json"));
        var root = json.Root;
        if (root.EnumerateObject().Count() != 10 ||
            root.GetProperty("schema_version").GetInt32() != 1 ||
            root.GetProperty("contract").GetString() !=
                "pcv-agent-execution-circuit-breaker-v1" ||
            root.GetProperty("default_checkpoint_count").GetInt32() != 1 ||
            root.GetProperty("elapsed_minutes_limit").GetInt32() != 30 ||
            root.GetProperty("tool_batch_limit").GetInt32() != 18 ||
            root.GetProperty("review_pass_limit").GetInt32() != 1 ||
            root.GetProperty("narrow_rereview_pass_limit").GetInt32() != 2 ||
            root.GetProperty("same_failure_limit").GetInt32() != 3 ||
            root.GetProperty("progress_warning_percent").GetInt32() != 70)
        {
            throw Invalid("circuit-breaker-contract");
        }

        var lanes = root.GetProperty("lanes");
        if (lanes.EnumerateObject().Count() != 4 ||
            lanes.GetProperty("0").GetProperty("elapsed_minutes_limit").GetInt32() != 10 ||
            lanes.GetProperty("0").GetProperty("tool_batch_limit").GetInt32() != 6 ||
            lanes.GetProperty("0").GetProperty("review_pass_limit").GetInt32() != 0 ||
            lanes.GetProperty("0").GetProperty("narrow_rereview_pass_limit").GetInt32() != 0 ||
            lanes.GetProperty("1").GetProperty("elapsed_minutes_limit").GetInt32() != 30 ||
            lanes.GetProperty("1").GetProperty("tool_batch_limit").GetInt32() != 18 ||
            lanes.GetProperty("1").GetProperty("review_pass_limit").GetInt32() != 1 ||
            lanes.GetProperty("1").GetProperty("narrow_rereview_pass_limit").GetInt32() != 2 ||
            lanes.GetProperty("2").GetProperty("elapsed_minutes_limit").GetInt32() != 45 ||
            lanes.GetProperty("2").GetProperty("tool_batch_limit").GetInt32() != 12 ||
            lanes.GetProperty("2").GetProperty("review_pass_limit").GetInt32() != 1 ||
            lanes.GetProperty("2").GetProperty("narrow_rereview_pass_limit").GetInt32() != 0 ||
            lanes.GetProperty("3").GetProperty("elapsed_minutes_limit").GetInt32() != 30 ||
            lanes.GetProperty("3").GetProperty("tool_batch_limit").GetInt32() != 12 ||
            lanes.GetProperty("3").GetProperty("review_pass_limit").GetInt32() != 1 ||
            lanes.GetProperty("3").GetProperty("narrow_rereview_pass_limit").GetInt32() != 2)
        {
            throw Invalid("circuit-breaker-lanes");
        }

        var policy = Source("docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md");
        var agents = Source("AGENTS.md");
        RequireTokens(
            policy,
            "circuit-breaker-policy",
            "30분",
            "18회",
            "21분",
            "13번째",
            "Lane 0",
            "Lane 1",
            "Lane 2",
            "Lane 3",
            "45분",
            "current_evidence_written",
            "추가 patch는 금지",
            "새 테스트도 금지",
            "Add-Type",
            "P/Invoke",
            "사용자의 명시적 승인");
        RequireTokens(
            agents,
            "agents-policy-link",
            "docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md",
            "config/agent-execution-circuit-breaker.json",
            "`vague_resume_policy`: `one-bounded-checkpoint`",
            "`out_of_scope_findings`: `report-only`",
            "Lane 0",
            "FAIL 프로브는 current를 못 쓴다",
            "canonical operator id");
    }

    private void ValidateArchitectureRegistry()
    {
        const string path =
            "packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json";
        using var json = JsonContract.Parse(path, Source(path));
        var root = json.Root;
        if (root.GetProperty("host_mutation_performed").GetBoolean() ||
            root.GetProperty("public_trusted_signing").GetBoolean() ||
            root.GetProperty("external_stable_publication").GetBoolean())
        {
            throw Invalid("architecture-mutation-claim");
        }

        var required = root.GetProperty("required_fault_gap_ids").EnumerateArray()
            .Select(value => value.GetString() ?? string.Empty).ToArray();
        var faults = root.GetProperty("fault_scenarios").EnumerateArray().ToArray();
        string[] expectedFaults =
        [
            "W0-FI-01",
            "W0-FI-02",
            "W0-FI-03",
            "W0-FI-04",
            "W0-FI-05",
            "W0-FI-06",
        ];
        if (!required.SequenceEqual(expectedFaults, StringComparer.Ordinal) ||
            faults.Length != 6 ||
            !faults.Select(fault => fault.GetProperty("gap_id").GetString())
                .SequenceEqual(expectedFaults, StringComparer.Ordinal))
        {
            throw Invalid("architecture-faults");
        }

        var migrationIds = root.GetProperty("migrations").EnumerateArray()
            .Select(migration => migration.GetProperty("migration_id").GetString() ?? string.Empty)
            .ToArray();
        if (migrationIds.Length != 14 ||
            migrationIds.Distinct(StringComparer.Ordinal).Count() != migrationIds.Length)
        {
            throw Invalid("architecture-migrations");
        }

        var registry = Source(
            "docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md");
        RequireTokens(registry, "architecture-registry-links", expectedFaults);
    }

    private void ValidateDevelopmentVerificationSources()
    {
        var selector = Source(
            "packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1");
        var runner = Source(
            "packaging/windows-desktop-node/tools/PcvDevelopmentVerificationRunner.psm1");
        RequireTokens(
            selector,
            "verification-selector",
            "Resolve-PcvDevelopmentChangeTier",
            "Resolve-PcvDevelopmentVerificationSelection",
            "tier-l-requires-release",
            "tier-m-requires-full",
            "unknown-change-scope",
            "current-evidence-check");
        string[] legacySuites =
        [
            "'dotnet'",
            "'web-npm'",
            "'packaging-pester'",
            "'installer-pester'",
            "'web-pester'",
            "'git-diff-check'",
            "'current-evidence-check'",
        ];
        RequireOrdered(selector, "verification-suite-order", legacySuites);
        RequireTokens(
            runner,
            "verification-runner",
            "Get-PcvDevelopmentVerificationSuiteCatalog",
            "not-selected-by-scope",
            "not-run-after-failure",
            "status = 'planned'",
            "status = $status",
            "failed_suite = $failedSuite");
        foreach (var mutation in new[]
        {
            "msiexec",
            "Start-VM",
            "New-VM",
            "Invoke-PcvFullAdminHostMutationGate",
        })
        {
            if (runner.Contains(mutation, StringComparison.OrdinalIgnoreCase))
            {
                throw Invalid("verification-runner-mutation");
            }
        }
    }

    private void ValidateManagedSuiteCatalog()
    {
        const string path = "config/development-verification-suites.json";
        using var json = JsonContract.Parse(path, Source(path));
        var root = json.Root;
        var activationState = root.GetProperty("activation_state").GetString();
        if (root.GetProperty("schema_version").GetInt32() != 1 ||
            root.GetProperty("contract").GetString() !=
                "pcv-development-verification-suite-catalog-v1" ||
            activationState is not ("plan-only-foundation" or "shadow-ready" or "active") ||
            root.GetProperty("max_parallelism").GetInt32() != 4 ||
            root.GetProperty("overall_timeout_seconds").GetInt32() != 1200)
        {
            throw Invalid("suite-catalog-contract");
        }

        var allowed = root.GetProperty("allowed_executables").EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty).ToArray();
        var suites = root.GetProperty("suites").EnumerateArray().Select(item =>
        {
            var executor = item.GetProperty("executor");
            var kind = executor.GetProperty("kind").GetString() ?? string.Empty;
            return new DevelopmentSuiteContract(
                item.GetProperty("id").GetString() ?? string.Empty,
                kind,
                kind == "process" ? executor.GetProperty("file_name").GetString() : null,
                item.GetProperty("timeout_seconds").GetInt32());
        }).ToArray();
        ValidateSuiteCatalog(suites, allowed);
    }

    private void ValidateQualitySources()
    {
        var capture = Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvDotNetQualityCapture.ps1");
        var ratchet = Source(
            "packaging/windows-desktop-node/tools/Test-PcvDotNetQualityRatchet.ps1");
        foreach (var source in new[] { capture, ratchet })
        {
            RequireTokens(
                source,
                "quality-safety",
                "DtdProcessing]::Prohibit",
                "XmlResolver = $null",
                "ReparsePoint",
                "GetRelativePath",
                "PCV_DOTNET_QUALITY_PATH_OUTSIDE_REPO");
        }

        RequireTokens(
            capture,
            "quality-capture",
            "coverlet.collector",
            "--version",
            "dotnet_sdk_version",
            "coverage_collector_version",
            "pcv-dotnet-quality-capture/v1");
        RequireTokens(
            ratchet,
            "quality-ratchet",
            "pcv-dotnet-quality-baseline/v1",
            "pcv-dotnet-source-snapshot/v1",
            "line_coverage",
            "branch_coverage",
            "removed test",
            "replacement_test_id");
        ValidateToolVersions(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["dotnet_sdk_version"] = "10.0.100",
            ["coverage_collector_version"] = "6.0.4",
        });
    }

    private void ValidateModuleSizeFixture()
    {
        const string path =
            "packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json";
        using var json = JsonContract.Parse(path, Source(path));
        var root = json.Root;
        var slack = root.GetProperty("slack_lines").GetInt32();
        var modules = root.GetProperty("modules").EnumerateArray().ToArray();
        if (root.GetProperty("contract").GetString() != "pcv-module-size-ratchet-v1" ||
            slack != 50 ||
            modules.Length != 14)
        {
            throw Invalid("module-ratchet-contract");
        }

        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            var modulePath = module.GetProperty("path").GetString() ?? string.Empty;
            var maximum = module.GetProperty("max_lines").GetInt32();
            if (!paths.Add(modulePath))
            {
                throw Invalid("module-ratchet-duplicate");
            }

            var text = repository.ReadUtf8Text(modulePath);
            var actual = CountLines(text);
            ValidateModuleRatchet(actual, maximum, maximum, slack);
        }
    }

    private void ValidateStrictCollectionSource()
    {
        var source = Source(
            "packaging/windows-desktop-node/tools/PcvStrictCollection.psm1");
        RequireOrdered(
            source,
            "strict-collection",
            "Set-StrictMode -Version Latest",
            "function Get-PcvChildItemArray",
            "return ,[object[]]@()",
            "$items = @(Get-ChildItem",
            "return ,$items",
            "Export-ModuleMember -Function Get-PcvChildItemArray");
    }

    private string Source(string path) =>
        sourceTexts.TryGetValue(path, out var text)
            ? text
            : throw Invalid("source-not-declared");

    private static int CountLines(string text) =>
        text.Length == 0
            ? 0
            : Regex.Matches(text, "\\n").Count + (text.EndsWith('\n') ? 0 : 1);

    private static int Count(string text, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }

    private static void RequireTokens(string source, string detail, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (!source.Contains(token, StringComparison.Ordinal))
            {
                throw Invalid(detail);
            }
        }
    }

    private static void RequireOrdered(string source, string detail, params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            if (index < 0)
            {
                throw Invalid(detail);
            }

            offset = index + token.Length;
        }
    }

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false).GetBytes(text)))
            .ToLowerInvariant();

    private static InvalidDataException Invalid(string detail, Exception? inner = null) =>
        new($"PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|{detail}", inner);

    private sealed record DevelopmentPolicySpec(
        string Contract,
        int LegacyContractCount,
        int LegacyShouldSiteCount,
        int RequiredLiteralCount,
        IReadOnlyList<DevelopmentPolicySourceFile> SourceFiles,
        IReadOnlyList<DevelopmentPolicyLegacyFile> LegacyFiles,
        IReadOnlyList<DevelopmentPolicySpecContract> Contracts);

    private sealed record DevelopmentPolicySourceFile(string Path, string Sha256);

    private sealed record DevelopmentPolicyLegacyFile(
        string Key,
        string Path,
        string Sha256,
        int ContractCount,
        int ShouldSiteCount);

    private sealed record DevelopmentPolicySpecContract(
        string Key,
        int Ordinal,
        string Name,
        IReadOnlyList<string> RequiredLiterals);
}

internal sealed record DevelopmentSuiteContract(
    string Id,
    string Kind,
    string? FileName,
    int TimeoutSeconds);
