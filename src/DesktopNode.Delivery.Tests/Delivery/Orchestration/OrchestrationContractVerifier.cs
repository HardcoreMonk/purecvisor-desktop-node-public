using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

internal sealed class OrchestrationContractVerifier
{
    internal const string SpecPath = "config/pcv-orchestration-contract-spec-v1.json";

    private const string ExpectedSpecSha256 =
        "a9dc4702728706ec8bdab6327b74a28c6ba98f9f764d3063428125a385c86c1e";

    private static readonly string[] ExpectedKeys =
    [
        "batch-supervisor",
        "ci-trigger",
        "job-store-migration",
        "artifact-root",
        "timeout-rate-limit",
    ];

    private static readonly int[] ExpectedCounts = [28, 2, 5, 3, 6];

    private static readonly Lazy<OrchestrationContractVerifier> Default =
        new(() => new OrchestrationContractVerifier(),
            LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly RepositoryContractContext repository;
    private readonly OrchestrationSpec spec;
    private readonly Dictionary<string, string> sources;
    private readonly string combinedSource;
    private readonly Lazy<bool> binding;

    private OrchestrationContractVerifier()
    {
        repository = RepositoryContractContext.Find();
        spec = LoadSpec();
        ValidateSpec();
        sources = spec.SourceFiles.ToDictionary(
            source => source.Path,
            source => repository.ReadUtf8Text(source.Path),
            StringComparer.Ordinal);
        combinedSource = string.Join("\n", sources.Values);
        binding = new Lazy<bool>(ValidateBinding, LazyThreadSafetyMode.ExecutionAndPublication);
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

    internal static void ValidateTerminalRows(IReadOnlyList<StepEventContract> events)
    {
        if (events.Count == 0 || events.Any(item =>
                string.IsNullOrWhiteSpace(item.StepId) ||
                string.IsNullOrWhiteSpace(item.Status)))
        {
            throw Invalid("terminal-events");
        }

        string[] terminal = ["completed", "failed", "timed-out", "skipped"];
        foreach (var group in events.GroupBy(item => item.StepId, StringComparer.Ordinal))
        {
            var items = group.ToArray();
            var terminalIndexes = items.Select((item, index) => (item, index))
                .Where(pair => terminal.Contains(pair.item.Status, StringComparer.Ordinal))
                .Select(pair => pair.index)
                .ToArray();
            if (terminalIndexes.Length != 1 || terminalIndexes[0] != items.Length - 1)
            {
                throw Invalid("terminal-cardinality");
            }
        }
    }

    internal static void ValidateTimeoutPolicy(
        int timeoutSeconds,
        int retryCount,
        int maximumTimeoutSeconds)
    {
        if (timeoutSeconds < 1 ||
            maximumTimeoutSeconds < 1 ||
            timeoutSeconds > maximumTimeoutSeconds ||
            retryCount is < 0 or > 10)
        {
            throw Invalid("timeout-policy");
        }
    }

    internal static void ValidateArtifactPath(string artifactRoot, string candidate)
    {
        if (string.IsNullOrWhiteSpace(artifactRoot) ||
            string.IsNullOrWhiteSpace(candidate) ||
            !Path.IsPathFullyQualified(artifactRoot) ||
            !Path.IsPathFullyQualified(candidate))
        {
            throw Invalid("artifact-path");
        }

        var root = Path.GetFullPath(artifactRoot).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        var path = Path.GetFullPath(candidate);
        if (!path.Equals(root, StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid("artifact-path");
        }
    }

    internal static void ValidateCiTriggers(string workflow)
    {
        RequireTokens(
            workflow,
            "ci-trigger",
            "on:",
            "pull_request:",
            "push:",
            "branches:",
            "- main",
            "workflow_dispatch:");
        if (Count(workflow, "pull_request:") != 1 ||
            Count(workflow, "push:") != 1 ||
            Count(workflow, "workflow_dispatch:") != 1 ||
            Regex.IsMatch(workflow, "(?m)^\\s*-\\s*['\"]?codex/\\*\\*['\"]?\\s*$"))
        {
            throw Invalid("ci-trigger-cardinality");
        }
    }

    internal static void ValidatePlanDescriptor(OrchestrationPlanContract plan)
    {
        if (!plan.PlanOnly || plan.HostMutationPerformed || plan.MutatesHost ||
            plan.Operations.Count == 0 ||
            plan.Operations.Any(string.IsNullOrWhiteSpace) ||
            plan.Operations.Distinct(StringComparer.Ordinal).Count() != plan.Operations.Count)
        {
            throw Invalid("plan-boundary");
        }
    }

    private OrchestrationSpec LoadSpec()
    {
        var text = repository.ReadUtf8Text(SpecPath);
        if (Hash(text) != ExpectedSpecSha256)
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var json = JsonContract.Parse(SpecPath, text);
            return JsonSerializer.Deserialize<OrchestrationSpec>(
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
        if (spec.Contract != "pcv-orchestration-contract-spec-v1" ||
            spec.LegacyContractCount != 44 ||
            spec.LegacyShouldSiteCount != 262 ||
            spec.RequiredLiteralCount != 607 ||
            spec.ArtifactRootRunnerCount != 36 ||
            spec.SourceFiles.Count != 40 ||
            spec.LegacyFiles.Count != 5 ||
            spec.Contracts.Count != 44 ||
            spec.SourceFiles.Select(source => source.Path)
                .Distinct(StringComparer.Ordinal).Count() != spec.SourceFiles.Count)
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
            spec.LegacyFiles.Sum(file => file.ShouldSiteCount) != spec.LegacyShouldSiteCount)
        {
            throw Invalid("spec-count");
        }
    }

    private bool ValidateBinding()
    {
        foreach (var source in spec.SourceFiles)
        {
            if (!sources.TryGetValue(source.Path, out var text) || Hash(text) != source.Sha256)
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

        ValidateBatchSupervisorSource();
        ValidateCiTriggers(Source(".github/workflows/development-gates.yml"));
        ValidateCiTriggers(Source(".github/workflows/public-boundary.yml"));
        ValidateMigrationSource();
        ValidateArtifactRootRunners();
        ValidateTimeoutRateLimitSource();
        return true;
    }

    private void ValidateBatchSupervisorSource()
    {
        var module = Source("packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1");
        RequireTokens(
            module,
            "batch-supervisor-source",
            "New-PcvBatchSupervisorManifest",
            "Assert-PcvBatchManifestValid",
            "Assert-PcvBatchExecutionAllowed",
            "Test-PcvBatchRebootForbiddenCommand",
            "Get-PcvBatchCommandFingerprint",
            "Add-PcvBatchHeartbeat",
            "Invoke-PcvBatchStepAttemptProcess",
            "Invoke-PcvBatchStepProcess",
            "Get-PcvBatchPriorSuccessfulResult",
            "Invoke-PcvBatchSupervisor",
            "summary.dry-run.json",
            "current-step.dry-run.json",
            "batch-manifest.resolved.dry-run.json",
            "PCV_BATCH_REBOOT_COMMAND_FORBIDDEN",
            "PCV_BATCH_HOST_MUTATION_APPROVAL_REQUIRED",
            "PCV_BATCH_ADMIN_REQUIRED",
            "retry_count",
            "next_resume_step_id",
            "$finalHeartbeatStatus");

        RequireOrdered(
            module,
            "batch-profile-order",
            "'PackagingRegression'",
            "'WebRegression'",
            "'ServiceMsiHyperVAdminSmoke'",
            "'OsMutationGate'",
            "'ManualAdminCampaignDescriptor'",
            "'FullAdminHostMutationGate'");
        SourceContract.RequireNoExecutableToken(
            "packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1",
            module,
            "Invoke-Expression");

        ValidateTerminalRows(
        [
            new("example", "started"),
            new("example", "running"),
            new("example", "completed"),
        ]);
        ValidateTimeoutPolicy(900, 1, 3600);
    }

    private void ValidateMigrationSource()
    {
        var source = Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvConfigJobStoreMigrationApplySmoke.ps1");
        string[] steps =
        [
            "'preflight'",
            "'build-current-admin-smoke-msi'",
            "'install-current-msi'",
            "'stop-installed-service-for-migration'",
            "'seed-installed-job-store-v1'",
            "'config-migration-apply-installed'",
            "'job-store-migration-apply-installed'",
            "'start-installed-service-after-migration'",
            "'post-migration-api-read'",
            "'final-state'",
        ];
        RequireOrdered(source, "migration-step-order", steps);
        RequireTokens(
            source,
            "migration-plan",
            "product-config-v1-to-v2",
            "job-store-v1-to-v2",
            "--migration-plan-id",
            "--migration-plan-version",
            "if ($PlanOnly)",
            "actual_execution = 'not-run'",
            "mutates_host = $false",
            "host_mutation_performed = $false",
            "public_trusted_signing = 'excluded'",
            "external_stable_publication = 'not-claimed'");
        ValidatePlanDescriptor(new OrchestrationPlanContract(
            PlanOnly: true,
            HostMutationPerformed: false,
            MutatesHost: false,
            Operations: steps));
    }

    private void ValidateArtifactRootRunners()
    {
        var runners = sources.Where(pair =>
                Regex.IsMatch(pair.Value, "(?m)^\\s*\\[string\\]\\$ArtifactRoot"))
            .ToArray();
        if (runners.Length != spec.ArtifactRootRunnerCount)
        {
            throw Invalid("artifact-runner-count");
        }

        foreach (var runner in runners)
        {
            var hasRelativeJoin = Regex.IsMatch(
                runner.Value,
                "Join-Path\\s*\\(\\s*Get-Location\\s*\\)\\s*\\$ArtifactRoot");
            if (hasRelativeJoin &&
                !Regex.IsMatch(runner.Value, "IsPathRooted\\(\\s*\\$ArtifactRoot\\s*\\)"))
            {
                throw Invalid("artifact-root-unguarded");
            }
        }

        ValidateArtifactPath(
            @"C:\repo\artifacts",
            @"C:\repo\artifacts\batch\summary.json");
    }

    private void ValidateTimeoutRateLimitSource()
    {
        var source = Source(
            "packaging/windows-desktop-node/tools/New-PcvTimeoutRateLimitHardeningPreflight.ps1");
        RequireTokens(
            source,
            "timeout-rate-limit-source",
            "PCV_TIMEOUT_RATE_LIMIT_PREFLIGHT_PLAN_ONLY_REQUIRED",
            "PCV_TIMEOUT_RATE_LIMIT_PREFLIGHT_API_ROUTE_PREFIX_INVALID",
            "[ValidateRange(1, 3600)]",
            "[ValidateRange(1, 100000)]",
            "[ValidateRange(1, 10000)]",
            "'/api/v1/'",
            "middleware_status = 'not-enabled'",
            "load_test_status = 'not-run'",
            "server_config_status = 'not-mutated'",
            "host_mutation_performed = $false",
            "blocked-by-no-mutation-preflight");
        string[] checks =
        [
            "'service-name-present'",
            "'api-route-prefix-recorded'",
            "'timeout-policy-recorded'",
            "'request-limit-policy-recorded'",
            "'retry-semantics-recorded'",
            "'ui-api-error-contract-recorded'",
            "'server-config-not-mutated'",
            "'middleware-not-enabled'",
            "'load-test-not-executed'",
            "'host-mutation-not-executed'",
        ];
        RequireOrdered(source, "timeout-check-order", checks);
        if (Regex.IsMatch(
                source,
                "(?i)Restart-Computer|shutdown\\.exe|msiexec|sc\\.exe|" +
                "Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|" +
                "Invoke-WebRequest|Invoke-RestMethod|Start-Process"))
        {
            throw Invalid("timeout-mutation");
        }
    }

    private string Source(string path) =>
        sources.TryGetValue(path, out var text)
            ? text
            : throw Invalid("source-not-declared");

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
        new($"PCV_DELIVERY_ORCHESTRATION_INVALID|{detail}", inner);

    private sealed record OrchestrationSpec(
        string Contract,
        int LegacyContractCount,
        int LegacyShouldSiteCount,
        int RequiredLiteralCount,
        int ArtifactRootRunnerCount,
        IReadOnlyList<OrchestrationSourceFile> SourceFiles,
        IReadOnlyList<OrchestrationLegacyFile> LegacyFiles,
        IReadOnlyList<OrchestrationSpecContract> Contracts);

    private sealed record OrchestrationSourceFile(string Path, string Sha256);

    private sealed record OrchestrationLegacyFile(
        string Key,
        string Path,
        string Sha256,
        int ContractCount,
        int ShouldSiteCount);

    private sealed record OrchestrationSpecContract(
        string Key,
        int Ordinal,
        string Name,
        IReadOnlyList<string> RequiredLiterals);
}

internal sealed record StepEventContract(string StepId, string Status);

internal sealed record OrchestrationPlanContract(
    bool PlanOnly,
    bool HostMutationPerformed,
    bool MutatesHost,
    IReadOnlyList<string> Operations);
