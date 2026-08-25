using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.Verification;

internal static class VerificationJson
{
    internal static JsonSerializerOptions Options { get; } = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    internal static string Serialize(VerificationSummary summary) =>
        JsonSerializer.Serialize(summary, Options);
}

internal static class VerificationSummaryFactory
{
    private const string SummaryContract = "pcv-development-verification-summary-v2";
    private const string CatalogContract = "pcv-development-verification-suite-catalog-v1";
    private const string CatalogActivationState = "plan-only-foundation";
    private const int MaximumOutputCharacters = 8192;

    private static readonly string[] ExpectedExecutableNames =
        ["dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"];

    private static readonly string[] ExpectedSuiteIds =
        ["dotnet", "web-typecheck", "web-parity", "delivery-contracts", "installer-contracts", "evidence-check", "policy-boundaries"];

    private static readonly string[] ExpectedShardIds = ["dotnet", "web", "delivery", "installer-policy"];

    private static readonly HashSet<string> AllowedOwners =
        new(["csharp", "node"], StringComparer.Ordinal);

    private static readonly HashSet<string> AllowedMigrationStates = new(
        ["native-existing", "wave-a-foundation", "wave-b-pending", "wave-c-pending", "wave-d-pending"],
        StringComparer.Ordinal);

    private static readonly HashSet<string> AllowedManagedHandlers =
        new(["current-evidence-check", "policy-boundaries"], StringComparer.Ordinal);

    private static readonly string[] PowerShellTokens = ["pwsh", "powershell", "Invoke-Pester"];

    private static readonly string[] ForbiddenCommandTokens =
        ["msiexec", "sc.exe", "New-VM", "Start-VM", "Stop-VM", "Start-Service", "Stop-Service", "Install-Module", "AllowHostMutation"];

    private static readonly VerificationCatalog FastClassificationCatalog = CreateFastClassificationCatalog();

    private static readonly HashSet<string> StableErrorCodes = new(StringComparer.Ordinal)
    {
        VerificationErrorCodes.ConfigInvalid,
        VerificationErrorCodes.UnknownSuite,
        VerificationErrorCodes.ProcessFailed,
        VerificationErrorCodes.Timeout,
        VerificationErrorCodes.Cancelled,
        VerificationErrorCodes.ParityUnmapped,
        VerificationErrorCodes.NonAdminPowerShellForbidden,
        VerificationErrorCodes.ArtifactRootInvalid
    };

    private static readonly HashSet<string> GenuineFailureErrorCodes = new(StringComparer.Ordinal)
    {
        VerificationErrorCodes.ConfigInvalid,
        VerificationErrorCodes.UnknownSuite,
        VerificationErrorCodes.ProcessFailed,
        VerificationErrorCodes.NonAdminPowerShellForbidden,
        VerificationErrorCodes.ArtifactRootInvalid
    };

    internal static VerificationSummary Create(
        VerificationPlan plan,
        VerificationCatalog catalog,
        VerificationExecutionReport report,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        if (plan is null || plan.Request is null || catalog is null || report is null ||
            plan.TierReasons is null || plan.Suites is null || report.Results is null ||
            !IsBasicRequest(plan.Request))
        {
            throw Invalid("summary-input=invalid");
        }

        bool catalogValid;
        try
        {
            catalogValid = IsValidCatalog(catalog);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            catalogValid = false;
        }

        if (!catalogValid)
        {
            throw Invalid("summary-catalog=invalid");
        }

        VerificationPlan canonicalPlan;
        try
        {
            canonicalPlan = VerificationPlanner.Create(plan.Request, catalog);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw Invalid("summary-canonical-plan=invalid");
        }

        if (!PlansEqual(plan, canonicalPlan) || report.DurationMs < 0)
        {
            throw Invalid("summary-plan-binding=invalid");
        }

        var rows = report.Results.ToArray();
        if (rows.Length != canonicalPlan.Suites.Count || rows.Any(row => row is null))
        {
            throw Invalid("summary-report-binding=invalid");
        }

        for (var index = 0; index < rows.Length; index++)
        {
            var suite = canonicalPlan.Suites[index];
            var row = rows[index];
            if (!string.Equals(row.SuiteId, suite.Id, StringComparison.Ordinal) ||
                !string.Equals(row.MigrationState, suite.MigrationState, StringComparison.Ordinal) ||
                !IsCoherentRow(suite, row))
            {
                throw Invalid("summary-report-row=invalid");
            }
        }

        var planOnly = canonicalPlan.Request.PlanOnly;
        var ok = rows.All(row => row.Status ==
            (planOnly ? SuiteStatus.Planned : SuiteStatus.Passed));
        var errorCode = rows.FirstOrDefault(row =>
            row.Status is not SuiteStatus.Planned and not SuiteStatus.Passed)?.ErrorCode;
        var results = rows.Select(ToSummary).ToArray();

        return new VerificationSummary(
            2,
            SummaryContract,
            Lane(canonicalPlan.Request.RequestedLane),
            Lane(canonicalPlan.EffectiveLane),
            Tier(canonicalPlan.Request.RequestedChangeTier),
            Tier(canonicalPlan.EffectiveChangeTier),
            Array.AsReadOnly(canonicalPlan.TierReasons.ToArray()),
            canonicalPlan.PromotionReason,
            Scope(canonicalPlan.ExecutionScope),
            canonicalPlan.ShardId,
            planOnly,
            catalog.ActivationState,
            ok,
            errorCode,
            startedAt.ToUniversalTime(),
            completedAt.ToUniversalTime(),
            planOnly ? TimestampDuration(startedAt, completedAt) : report.DurationMs,
            Array.AsReadOnly(results));
    }

    internal static VerificationSummary CreateFailure(
        VerificationRequest request,
        VerificationPlan? plan,
        string catalogActivationState,
        string errorCode,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        if (request is null || string.IsNullOrWhiteSpace(catalogActivationState) ||
            !StableErrorCodes.Contains(errorCode))
        {
            throw Invalid("failure-summary-input=invalid");
        }

        var sourceRequest = request;
        if (plan is not null)
        {
            bool valid;
            try
            {
                valid = plan.Request is not null &&
                    IsBasicRequest(request) &&
                    IsBasicRequest(plan.Request) &&
                    RequestsEqual(request, plan.Request) &&
                    IsValidFailurePlan(plan);
            }
            catch (Exception exception) when (!IsFatal(exception))
            {
                valid = false;
            }

            if (!valid)
            {
                throw Invalid("failure-summary-plan=invalid");
            }

            sourceRequest = plan.Request!;
        }

        var executionScope = plan is null ? Scope(sourceRequest) : Scope(plan.ExecutionScope);
        var shardId = plan is null
            ? executionScope == "shard" ? sourceRequest.ShardId : null
            : plan.ShardId;

        return new VerificationSummary(
            2,
            SummaryContract,
            Lane(sourceRequest.RequestedLane),
            Lane(plan?.EffectiveLane ?? sourceRequest.RequestedLane),
            Tier(sourceRequest.RequestedChangeTier),
            Tier(plan?.EffectiveChangeTier ?? sourceRequest.RequestedChangeTier),
            plan is null
                ? Array.Empty<string>()
                : Array.AsReadOnly(plan.TierReasons!.ToArray()),
            plan?.PromotionReason,
            executionScope,
            shardId,
            sourceRequest.PlanOnly,
            catalogActivationState,
            false,
            errorCode,
            startedAt.ToUniversalTime(),
            completedAt.ToUniversalTime(),
            TimestampDuration(startedAt, completedAt),
            Array.Empty<VerificationSuiteSummary>());
    }

    private static bool IsCoherentRow(SuiteDefinition suite, SuiteExecutionRecord row)
    {
        if (!IsBasicSuiteDefinition(suite) || row.DurationMs < 0 || !Enum.IsDefined(row.Status))
        {
            return false;
        }

        if (row.Status == SuiteStatus.Planned)
        {
            return row.DurationMs == 0 && !row.TimedOut && !row.Cancelled &&
                ExecutionFieldsAreNull(row) && row.ErrorCode is null;
        }

        return suite.ExecutorKind switch
        {
            "process" => IsCoherentProcessRow(row),
            "managed" => IsCoherentManagedRow(row),
            _ => false
        };
    }

    private static bool IsCoherentProcessRow(SuiteExecutionRecord row) =>
        row.Status switch
        {
            SuiteStatus.Passed => row.ExitCode == 0 && !row.TimedOut && !row.Cancelled &&
                row.ErrorCode is null && HasValidProcessOutput(row),
            SuiteStatus.Failed => !row.TimedOut && !row.Cancelled &&
                ((ExecutionFieldsAreNull(row) &&
                  GenuineFailureErrorCodes.Contains(row.ErrorCode ?? string.Empty)) ||
                 (row.ExitCode is not null and not 0 &&
                  string.Equals(row.ErrorCode, VerificationErrorCodes.ProcessFailed, StringComparison.Ordinal) &&
                  HasValidProcessOutput(row))),
            SuiteStatus.Missing => false,
            SuiteStatus.TimedOut => row.ExitCode is null && row.TimedOut && !row.Cancelled &&
                string.Equals(row.ErrorCode, VerificationErrorCodes.Timeout, StringComparison.Ordinal) &&
                (OutputFieldsAreNull(row) || HasValidProcessOutput(row)),
            SuiteStatus.Cancelled => row.ExitCode is null && !row.TimedOut && row.Cancelled &&
                string.Equals(row.ErrorCode, VerificationErrorCodes.Cancelled, StringComparison.Ordinal) &&
                (OutputFieldsAreNull(row) || HasValidProcessOutput(row)),
            _ => false
        };

    private static bool IsCoherentManagedRow(SuiteExecutionRecord row) =>
        row.Status switch
        {
            SuiteStatus.Passed => !row.TimedOut && !row.Cancelled &&
                row.ErrorCode is null && ExecutionFieldsAreNull(row),
            SuiteStatus.Failed => !row.TimedOut && !row.Cancelled &&
                GenuineFailureErrorCodes.Contains(row.ErrorCode ?? string.Empty) &&
                ExecutionFieldsAreNull(row),
            SuiteStatus.Missing => !row.TimedOut && !row.Cancelled &&
                string.Equals(row.ErrorCode, VerificationErrorCodes.ParityUnmapped, StringComparison.Ordinal) &&
                ExecutionFieldsAreNull(row),
            SuiteStatus.TimedOut => row.TimedOut && !row.Cancelled &&
                string.Equals(row.ErrorCode, VerificationErrorCodes.Timeout, StringComparison.Ordinal) &&
                ExecutionFieldsAreNull(row),
            SuiteStatus.Cancelled => !row.TimedOut && row.Cancelled &&
                string.Equals(row.ErrorCode, VerificationErrorCodes.Cancelled, StringComparison.Ordinal) &&
                ExecutionFieldsAreNull(row),
            _ => false
        };

    private static bool HasValidProcessOutput(SuiteExecutionRecord row)
    {
        if (row.StandardOutput is null || row.StandardError is null || row.OutputSha256 is null ||
            row.StandardOutput.Length > MaximumOutputCharacters ||
            row.StandardError.Length > MaximumOutputCharacters)
        {
            return false;
        }

        try
        {
            if (!string.Equals(
                    row.StandardOutput,
                    ProcessOutputSanitizer.Sanitize(row.StandardOutput, string.Empty, MaximumOutputCharacters),
                    StringComparison.Ordinal) ||
                !string.Equals(
                    row.StandardError,
                    ProcessOutputSanitizer.Sanitize(row.StandardError, string.Empty, MaximumOutputCharacters),
                    StringComparison.Ordinal))
            {
                return false;
            }

            var expectedHash = Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(row.StandardOutput + "\n" + row.StandardError))).ToLowerInvariant();
            return string.Equals(row.OutputSha256, expectedHash, StringComparison.Ordinal);
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            return false;
        }
    }

    private static bool ExecutionFieldsAreNull(SuiteExecutionRecord row) =>
        row.ExitCode is null && OutputFieldsAreNull(row);

    private static bool OutputFieldsAreNull(SuiteExecutionRecord row) =>
        row.StandardOutput is null && row.StandardError is null && row.OutputSha256 is null;

    private static bool PlansEqual(VerificationPlan actual, VerificationPlan expected)
    {
        if (actual.Request is null || actual.TierReasons is null || actual.Suites is null ||
            expected.Request is null || expected.TierReasons is null || expected.Suites is null ||
            !RequestsEqual(actual.Request, expected.Request) ||
            actual.EffectiveLane != expected.EffectiveLane ||
            actual.EffectiveChangeTier != expected.EffectiveChangeTier ||
            !actual.TierReasons.SequenceEqual(expected.TierReasons, StringComparer.Ordinal) ||
            !string.Equals(actual.PromotionReason, expected.PromotionReason, StringComparison.Ordinal) ||
            actual.ExecutionScope != expected.ExecutionScope ||
            !string.Equals(actual.ShardId, expected.ShardId, StringComparison.Ordinal) ||
            actual.ReleasePreflight != expected.ReleasePreflight ||
            actual.Suites.Count != expected.Suites.Count)
        {
            return false;
        }

        for (var index = 0; index < actual.Suites.Count; index++)
        {
            if (!DefinitionsEqual(actual.Suites[index], expected.Suites[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool RequestsEqual(VerificationRequest left, VerificationRequest right) =>
        left.RequestedLane == right.RequestedLane &&
        left.RequestedChangeTier == right.RequestedChangeTier &&
        left.ChangedPaths is not null && right.ChangedPaths is not null &&
        left.ChangedPaths.SequenceEqual(right.ChangedPaths, StringComparer.Ordinal) &&
        string.Equals(left.ArtifactRoot, right.ArtifactRoot, StringComparison.Ordinal) &&
        left.SuiteIds is not null && right.SuiteIds is not null &&
        left.SuiteIds.SequenceEqual(right.SuiteIds, StringComparer.Ordinal) &&
        string.Equals(left.ShardId, right.ShardId, StringComparison.Ordinal) &&
        left.PlanOnly == right.PlanOnly;

    private static bool IsBasicRequest(VerificationRequest request)
    {
        if (!Enum.IsDefined(request.RequestedLane) || !Enum.IsDefined(request.RequestedChangeTier) ||
            request.ChangedPaths is null || request.SuiteIds is null ||
            string.IsNullOrWhiteSpace(request.ArtifactRoot) ||
            request.ChangedPaths.Any(string.IsNullOrWhiteSpace) ||
            request.SuiteIds.Any(string.IsNullOrWhiteSpace) ||
            request.SuiteIds.Count != request.SuiteIds.Distinct(StringComparer.Ordinal).Count() ||
            (request.ShardId is not null && string.IsNullOrWhiteSpace(request.ShardId)) ||
            (request.SuiteIds.Count > 0 && request.ShardId is not null))
        {
            return false;
        }

        return true;
    }

    private static bool IsValidCatalog(VerificationCatalog catalog)
    {
        if (catalog.SchemaVersion != 1 ||
            !string.Equals(catalog.Contract, CatalogContract, StringComparison.Ordinal) ||
            !string.Equals(catalog.ActivationState, CatalogActivationState, StringComparison.Ordinal) ||
            catalog.MaxParallelism != 4 ||
            catalog.OverallTimeoutSeconds is < 1 or > 3600 ||
            catalog.AllowedExecutables is null ||
            catalog.Suites is null ||
            catalog.Shards is null)
        {
            return false;
        }

        if (catalog.AllowedExecutables.Count != ExpectedExecutableNames.Length)
        {
            return false;
        }

        var executableSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var executable in catalog.AllowedExecutables)
        {
            if (string.IsNullOrWhiteSpace(executable) ||
                Path.IsPathRooted(executable) ||
                executable.IndexOfAny(['\\', '/']) >= 0 ||
                !string.Equals(
                    executable,
                    Path.GetFileName(executable).ToLowerInvariant(),
                    StringComparison.Ordinal) ||
                !executableSet.Add(executable))
            {
                return false;
            }
        }

        if (!executableSet.SetEquals(ExpectedExecutableNames) ||
            catalog.Suites.Count != ExpectedSuiteIds.Length)
        {
            return false;
        }

        var suiteIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < catalog.Suites.Count; index++)
        {
            var suite = catalog.Suites[index];
            if (suite is null ||
                !string.Equals(suite.Id, ExpectedSuiteIds[index], StringComparison.Ordinal) ||
                !suiteIds.Add(suite.Id) ||
                !AllowedOwners.Contains(suite.Owner) ||
                !AllowedMigrationStates.Contains(suite.MigrationState) ||
                suite.TimeoutSeconds < 1 ||
                suite.TimeoutSeconds > catalog.OverallTimeoutSeconds ||
                suite.Arguments is null ||
                suite.Arguments.Any(argument => argument is null))
            {
                return false;
            }

            if (string.Equals(suite.ExecutorKind, "managed", StringComparison.Ordinal))
            {
                if (suite.FileName is not null || suite.Arguments.Count != 0 ||
                    suite.ManagedHandler is null || !AllowedManagedHandlers.Contains(suite.ManagedHandler))
                {
                    return false;
                }

                continue;
            }

            if (!string.Equals(suite.ExecutorKind, "process", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(suite.FileName) || suite.ManagedHandler is not null ||
                Path.IsPathRooted(suite.FileName) || suite.FileName.IndexOfAny(['\\', '/']) >= 0 ||
                !string.Equals(
                    suite.FileName,
                    Path.GetFileName(suite.FileName).ToLowerInvariant(),
                    StringComparison.Ordinal) ||
                !executableSet.Contains(suite.FileName))
            {
                return false;
            }

            var commandParts = new[] { suite.FileName }.Concat(suite.Arguments);
            if (commandParts.Any(part =>
                    PowerShellTokens.Any(token => part.Contains(token, StringComparison.OrdinalIgnoreCase)) ||
                    ForbiddenCommandTokens.Any(token => part.Contains(token, StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }
        }

        if (catalog.Shards.Count != ExpectedShardIds.Length)
        {
            return false;
        }

        var seenShardIds = new HashSet<string>(StringComparer.Ordinal);
        var shardUnion = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < catalog.Shards.Count; index++)
        {
            var shard = catalog.Shards[index];
            if (shard is null ||
                !string.Equals(shard.Id, ExpectedShardIds[index], StringComparison.Ordinal) ||
                !seenShardIds.Add(shard.Id) ||
                shard.SuiteIds is null || shard.SuiteIds.Count == 0)
            {
                return false;
            }

            var shardMembers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var suiteId in shard.SuiteIds)
            {
                if (string.IsNullOrWhiteSpace(suiteId) ||
                    !suiteIds.Contains(suiteId) ||
                    !shardMembers.Add(suiteId) ||
                    !shardUnion.Add(suiteId))
                {
                    return false;
                }
            }
        }

        return shardUnion.Count == suiteIds.Count && shardUnion.SetEquals(suiteIds);
    }

    private static bool IsValidFailurePlan(VerificationPlan plan)
    {
        if (plan.Request is null || plan.TierReasons is null || plan.Suites is null ||
            plan.Suites.Count == 0 || !Enum.IsDefined(plan.EffectiveLane) ||
            !Enum.IsDefined(plan.EffectiveChangeTier) || !Enum.IsDefined(plan.ExecutionScope) ||
            plan.TierReasons.Any(string.IsNullOrWhiteSpace) ||
            plan.Suites.Any(suite => suite is null || !IsBasicSuiteDefinition(suite)) ||
            plan.Suites.Select(suite => suite.Id).Distinct(StringComparer.Ordinal).Count() != plan.Suites.Count ||
            plan.ReleasePreflight != (plan.EffectiveLane == VerificationLane.Release))
        {
            return false;
        }

        var expectedScope = plan.Request.SuiteIds.Count > 0
            ? ExecutionScope.Partial
            : plan.Request.ShardId is not null
                ? ExecutionScope.Shard
                : ExecutionScope.Lane;
        if (plan.ExecutionScope != expectedScope ||
            (expectedScope == ExecutionScope.Shard
                ? !string.Equals(plan.ShardId, plan.Request.ShardId, StringComparison.Ordinal)
                : plan.ShardId is not null))
        {
            return false;
        }

        var tier = ChangeTierPolicy.Resolve(
            plan.Request.RequestedChangeTier,
            plan.Request.ChangedPaths);
        if (plan.EffectiveChangeTier != tier.EffectiveTier ||
            !plan.TierReasons.SequenceEqual(tier.Reasons, StringComparer.Ordinal))
        {
            return false;
        }

        var expectedLane = plan.Request.RequestedLane;
        string? expectedPromotion = null;
        if (tier.EffectiveTier == ChangeTier.L && expectedLane != VerificationLane.Release)
        {
            expectedLane = VerificationLane.Release;
            expectedPromotion = "tier-l-requires-release";
        }
        else if (tier.EffectiveTier == ChangeTier.M && expectedLane == VerificationLane.Fast)
        {
            expectedLane = VerificationLane.Full;
            expectedPromotion = "tier-m-requires-full";
        }

        FastSuiteResolution? fast = null;
        if (expectedPromotion is null && expectedLane == VerificationLane.Fast &&
            expectedScope == ExecutionScope.Lane)
        {
            fast = FastSuitePolicy.Resolve(plan.Request.ChangedPaths, FastClassificationCatalog);
            if (fast.HasUnknownPath || fast.Suites.Count == 0)
            {
                expectedLane = VerificationLane.Full;
                expectedPromotion = "unknown-change-scope";
            }
        }

        if (plan.EffectiveLane != expectedLane ||
            !string.Equals(plan.PromotionReason, expectedPromotion, StringComparison.Ordinal))
        {
            return false;
        }

        var planSuiteIds = plan.Suites.Select(suite => suite.Id).ToArray();
        if (expectedScope == ExecutionScope.Partial)
        {
            var requestedIds = plan.Request.SuiteIds.ToHashSet(StringComparer.Ordinal);
            var expectedIds = ExpectedSuiteIds.Where(requestedIds.Contains).ToArray();
            return expectedIds.Length == plan.Request.SuiteIds.Count &&
                planSuiteIds.SequenceEqual(expectedIds, StringComparer.Ordinal);
        }

        if (expectedScope == ExecutionScope.Shard)
        {
            return ExpectedShardIds.Contains(plan.ShardId, StringComparer.Ordinal) &&
                planSuiteIds.All(suiteId => ExpectedSuiteIds.Contains(suiteId, StringComparer.Ordinal));
        }

        var expectedLaneSuiteIds = expectedLane is VerificationLane.Full or VerificationLane.Release
            ? ExpectedSuiteIds
            : fast!.Suites.Select(suite => suite.Id).ToArray();
        return planSuiteIds.SequenceEqual(expectedLaneSuiteIds, StringComparer.Ordinal);
    }

    private static VerificationCatalog CreateFastClassificationCatalog()
    {
        var suites = ExpectedSuiteIds.Select(id => new SuiteDefinition(
            id,
            "csharp",
            "native-existing",
            "process",
            "dotnet",
            Array.Empty<string>(),
            null,
            1)).ToArray();

        return new VerificationCatalog(
            1,
            CatalogContract,
            CatalogActivationState,
            4,
            1,
            Array.Empty<string>(),
            Array.AsReadOnly(suites),
            Array.Empty<ShardDefinition>());
    }

    private static bool IsBasicSuiteDefinition(SuiteDefinition suite)
    {
        if (string.IsNullOrWhiteSpace(suite.Id) || string.IsNullOrWhiteSpace(suite.Owner) ||
            string.IsNullOrWhiteSpace(suite.MigrationState) || suite.Arguments is null ||
            suite.Arguments.Any(argument => argument is null) ||
            suite.TimeoutSeconds is < 1 or > 3600)
        {
            return false;
        }

        return suite.ExecutorKind switch
        {
            "process" => !string.IsNullOrWhiteSpace(suite.FileName) && suite.ManagedHandler is null,
            "managed" => suite.FileName is null && suite.Arguments.Count == 0 &&
                !string.IsNullOrWhiteSpace(suite.ManagedHandler),
            _ => false
        };
    }

    private static bool DefinitionsEqual(SuiteDefinition left, SuiteDefinition right) =>
        left is not null && right is not null &&
        string.Equals(left.Id, right.Id, StringComparison.Ordinal) &&
        string.Equals(left.Owner, right.Owner, StringComparison.Ordinal) &&
        string.Equals(left.MigrationState, right.MigrationState, StringComparison.Ordinal) &&
        string.Equals(left.ExecutorKind, right.ExecutorKind, StringComparison.Ordinal) &&
        string.Equals(left.FileName, right.FileName, StringComparison.Ordinal) &&
        ArgumentsEqual(left.Arguments, right.Arguments) &&
        string.Equals(left.ManagedHandler, right.ManagedHandler, StringComparison.Ordinal) &&
        left.TimeoutSeconds == right.TimeoutSeconds;

    private static bool ArgumentsEqual(IReadOnlyList<string>? left, IReadOnlyList<string>? right) =>
        left is null || right is null
            ? left is null && right is null
            : left.SequenceEqual(right, StringComparer.Ordinal);

    private static VerificationSuiteSummary ToSummary(SuiteExecutionRecord row) =>
        new(
            row.SuiteId,
            Status(row.Status),
            row.MigrationState,
            row.ExitCode,
            row.DurationMs,
            row.TimedOut,
            row.Cancelled,
            row.StandardOutput,
            row.StandardError,
            row.OutputSha256,
            row.ErrorCode);

    private static string Status(SuiteStatus status) => status switch
    {
        SuiteStatus.Planned => "planned",
        SuiteStatus.Passed => "passed",
        SuiteStatus.Failed => "failed",
        SuiteStatus.Missing => "missing",
        SuiteStatus.TimedOut => "timed_out",
        SuiteStatus.Cancelled => "cancelled",
        _ => throw Invalid("summary-status=invalid")
    };

    private static string Scope(ExecutionScope scope) => scope switch
    {
        ExecutionScope.Lane => "lane",
        ExecutionScope.Shard => "shard",
        ExecutionScope.Partial => "partial",
        _ => throw Invalid("summary-scope=invalid")
    };

    private static string Scope(VerificationRequest request) =>
        request.SuiteIds is { Count: > 0 }
            ? "partial"
            : !string.IsNullOrWhiteSpace(request.ShardId)
                ? "shard"
                : "lane";

    private static string Lane(VerificationLane lane) => lane switch
    {
        VerificationLane.Fast => "Fast",
        VerificationLane.Full => "Full",
        VerificationLane.Release => "Release",
        _ => throw Invalid("summary-lane=invalid")
    };

    private static string Tier(ChangeTier tier) => tier switch
    {
        ChangeTier.S => "S",
        ChangeTier.M => "M",
        ChangeTier.L => "L",
        _ => throw Invalid("summary-tier=invalid")
    };

    private static long TimestampDuration(DateTimeOffset startedAt, DateTimeOffset completedAt) =>
        Math.Max(0, (long)(completedAt - startedAt).TotalMilliseconds);

    private static bool IsFatal(Exception exception) =>
        exception is OutOfMemoryException or StackOverflowException or AccessViolationException or
        AppDomainUnloadedException or SEHException;

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);
}

internal sealed class AtomicVerificationSummaryWriter(IVerificationFileSystem fileSystem)
{
    private const string CleanupExceptionTypeKey = "verification_summary_cleanup_exception_type";

    internal async Task<string> WriteAsync(
        string artifactRoot,
        VerificationSummary summary,
        CancellationToken cancellationToken)
    {
        fileSystem.CreateDirectory(artifactRoot);
        var contents = VerificationJson.Serialize(summary);
        var destination = Path.Combine(artifactRoot, "summary.json");
        var temporary = Path.Combine(artifactRoot, $"summary.json.{Guid.NewGuid():N}.tmp");
        Exception? primaryException = null;

        try
        {
            await fileSystem.WriteAllTextAsync(temporary, contents, cancellationToken);
            fileSystem.MoveFile(temporary, destination, overwrite: true);
            return destination;
        }
        catch (Exception exception)
        {
            primaryException = exception;
            throw;
        }
        finally
        {
            try
            {
                if (fileSystem.FileExists(temporary))
                {
                    fileSystem.DeleteFile(temporary);
                }
            }
            catch (Exception cleanupException) when (
                primaryException is not null && !IsFatal(cleanupException))
            {
                AttachCleanupType(primaryException, cleanupException);
            }
        }
    }

    private static void AttachCleanupType(Exception primaryException, Exception cleanupException)
    {
        try
        {
            primaryException.Data[CleanupExceptionTypeKey] =
                cleanupException.GetType().FullName ?? cleanupException.GetType().Name;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
        }
    }

    private static bool IsFatal(Exception exception) =>
        exception is OutOfMemoryException or StackOverflowException or AccessViolationException or
        AppDomainUnloadedException or SEHException;
}
