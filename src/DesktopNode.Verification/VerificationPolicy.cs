using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

internal sealed record TierResolution(
    ChangeTier RequestedTier,
    ChangeTier EffectiveTier,
    IReadOnlyList<string> Reasons);

internal sealed record FastSuiteResolution(
    IReadOnlyList<SuiteDefinition> Suites,
    bool HasUnknownPath);

internal static class ChangeTierPolicy
{
    internal static TierResolution Resolve(ChangeTier requestedTier, IReadOnlyList<string> changedPaths)
    {
        var effectiveTier = requestedTier;
        var reasons = new List<string>();
        var seenReasons = new HashSet<string>(StringComparer.Ordinal);
        var domains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddReason(string reason, ChangeTier minimumTier)
        {
            if (seenReasons.Add(reason))
            {
                reasons.Add(reason);
            }

            if (Rank(minimumTier) > Rank(effectiveTier))
            {
                effectiveTier = minimumTier;
            }
        }

        foreach (var path in changedPaths)
        {
            var normalized = Normalize(path);
            AddDomain(normalized, domains);

            if (PolicyPatterns.Installer.IsMatch(normalized))
            {
                AddReason("installer-lifecycle", ChangeTier.L);
            }

            if (PolicyPatterns.HostMutation.IsMatch(normalized))
            {
                AddReason("host-mutation-boundary", ChangeTier.L);
            }

            if (PolicyPatterns.SecurityPolicy.IsMatch(normalized))
            {
                AddReason("security-policy-boundary", ChangeTier.L);
            }

            if (PolicyPatterns.CurrentEvidenceAnchor.IsMatch(normalized))
            {
                AddReason("current-evidence-anchor", ChangeTier.L);
            }

            if (PolicyPatterns.PublicReleaseBoundary.IsMatch(normalized))
            {
                AddReason("public-release-boundary", ChangeTier.L);
            }

            if (PolicyPatterns.SigningPublicationToken.IsMatch(normalized))
            {
                AddReason("signing-publication-boundary", ChangeTier.L);
            }

            if (PolicyPatterns.DesktopNodeApiCli.IsMatch(normalized) ||
                (PolicyPatterns.WebSourceTests.IsMatch(normalized) &&
                 PolicyPatterns.WebContractToken.IsMatch(normalized)))
            {
                AddReason("api-cli-web-contract", ChangeTier.M);
            }

            if (PolicyPatterns.Packaging.IsMatch(normalized))
            {
                AddReason("packaging-contract", ChangeTier.M);
            }

            if (PolicyPatterns.DevelopmentVerificationBoundary.IsMatch(normalized))
            {
                AddReason("development-verification-boundary", ChangeTier.M);
            }
        }

        if (domains.Count > 1)
        {
            AddReason("cross-module-change", ChangeTier.M);
        }

        return new TierResolution(
            requestedTier,
            effectiveTier,
            Array.AsReadOnly(reasons.ToArray()));
    }

    private static int Rank(ChangeTier tier) => tier switch
    {
        ChangeTier.S => 1,
        ChangeTier.M => 2,
        ChangeTier.L => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    private static void AddDomain(string path, ISet<string> domains)
    {
        var sourceMatch = PolicyPatterns.SourceDomain.Match(path);
        if (sourceMatch.Success)
        {
            domains.Add($"src:{sourceMatch.Groups[1].Value}");
        }
        else if (PolicyPatterns.WebDomain.IsMatch(path))
        {
            domains.Add("web");
        }
        else if (PolicyPatterns.Packaging.IsMatch(path))
        {
            domains.Add("packaging");
        }
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}

internal static class VerificationPlanner
{
    internal static VerificationPlan Create(VerificationRequest request, VerificationCatalog catalog)
    {
        var requestSnapshot = SnapshotRequest(request);
        var catalogSnapshot = SnapshotCatalog(catalog);

        var tier = ChangeTierPolicy.Resolve(requestSnapshot.RequestedChangeTier, requestSnapshot.ChangedPaths);
        var effectiveLane = requestSnapshot.RequestedLane;
        string? promotionReason = null;

        if (tier.EffectiveTier == ChangeTier.L && effectiveLane != VerificationLane.Release)
        {
            effectiveLane = VerificationLane.Release;
            promotionReason = "tier-l-requires-release";
        }
        else if (tier.EffectiveTier == ChangeTier.M && effectiveLane == VerificationLane.Fast)
        {
            effectiveLane = VerificationLane.Full;
            promotionReason = "tier-m-requires-full";
        }

        if (requestSnapshot.SuiteIds.Count > 0)
        {
            var requestedIds = requestSnapshot.SuiteIds.ToHashSet(StringComparer.Ordinal);
            foreach (var suiteId in requestSnapshot.SuiteIds)
            {
                if (!catalogSnapshot.Suites.Any(suite => string.Equals(suite.Id, suiteId, StringComparison.Ordinal)))
                {
                    throw new VerificationException(VerificationErrorCodes.UnknownSuite, $"suite={suiteId}");
                }
            }

            return CreatePlan(
                requestSnapshot,
                effectiveLane,
                tier,
                promotionReason,
                ExecutionScope.Partial,
                null,
                catalogSnapshot.Suites.Where(suite => requestedIds.Contains(suite.Id)));
        }

        if (requestSnapshot.ShardId is not null)
        {
            var shard = catalogSnapshot.Shards.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, requestSnapshot.ShardId, StringComparison.Ordinal));
            if (shard is null)
            {
                throw Invalid($"shard={requestSnapshot.ShardId}");
            }

            var shardSuites = new List<SuiteDefinition>();
            foreach (var suiteId in shard.SuiteIds)
            {
                var suite = catalogSnapshot.Suites.FirstOrDefault(candidate =>
                    string.Equals(candidate.Id, suiteId, StringComparison.Ordinal));
                if (suite is null)
                {
                    throw Invalid("catalog-shard-member=unknown");
                }

                shardSuites.Add(suite);
            }

            return CreatePlan(
                requestSnapshot,
                effectiveLane,
                tier,
                promotionReason,
                ExecutionScope.Shard,
                shard.Id,
                shardSuites);
        }

        if (effectiveLane is VerificationLane.Full or VerificationLane.Release)
        {
            return CreatePlan(
                requestSnapshot,
                effectiveLane,
                tier,
                promotionReason,
                ExecutionScope.Lane,
                null,
                catalogSnapshot.Suites);
        }

        var fast = FastSuitePolicy.Resolve(requestSnapshot.ChangedPaths, catalogSnapshot);
        if (fast.HasUnknownPath || fast.Suites.Count == 0)
        {
            return CreatePlan(
                requestSnapshot,
                VerificationLane.Full,
                tier,
                "unknown-change-scope",
                ExecutionScope.Lane,
                null,
                catalogSnapshot.Suites);
        }

        return CreatePlan(
            requestSnapshot,
            effectiveLane,
            tier,
            promotionReason,
            ExecutionScope.Lane,
            null,
            fast.Suites);
    }

    private static VerificationRequest SnapshotRequest(VerificationRequest request)
    {
        if (request is null)
        {
            throw Invalid("request=missing");
        }

        if (request.ChangedPaths is null)
        {
            throw Invalid("changed-paths=missing");
        }

        var changedPaths = request.ChangedPaths.ToArray();
        if (changedPaths.Any(string.IsNullOrWhiteSpace))
        {
            throw Invalid("changed-path=empty");
        }

        if (request.SuiteIds is null)
        {
            throw Invalid("suite-ids=missing");
        }

        var suiteIds = request.SuiteIds.ToArray();
        if (suiteIds.Any(string.IsNullOrWhiteSpace))
        {
            throw Invalid("suite-id=empty");
        }

        if (request.ShardId is not null && string.IsNullOrWhiteSpace(request.ShardId))
        {
            throw Invalid("shard-id=empty");
        }

        if (suiteIds.Length > 0 && request.ShardId is not null)
        {
            throw Invalid("selection=suite-and-shard");
        }

        if (suiteIds.Length != suiteIds.Distinct(StringComparer.Ordinal).Count())
        {
            throw Invalid("selection=duplicate-suite");
        }

        return request with
        {
            ChangedPaths = Array.AsReadOnly(changedPaths),
            SuiteIds = Array.AsReadOnly(suiteIds)
        };
    }

    private static VerificationCatalog SnapshotCatalog(VerificationCatalog catalog)
    {
        if (catalog is null)
        {
            throw Invalid("catalog=missing");
        }

        if (catalog.Suites is null)
        {
            throw Invalid("catalog-suites=missing");
        }

        if (catalog.Suites.Count == 0)
        {
            throw Invalid("catalog-suites=empty");
        }

        var suiteIds = new HashSet<string>(StringComparer.Ordinal);
        var suites = new List<SuiteDefinition>(catalog.Suites.Count);
        foreach (var suite in catalog.Suites)
        {
            if (suite is null)
            {
                throw Invalid("catalog-suite=missing");
            }

            if (string.IsNullOrWhiteSpace(suite.Id))
            {
                throw Invalid("catalog-suite-id=empty");
            }

            if (!suiteIds.Add(suite.Id))
            {
                throw Invalid("catalog-suite-id=duplicate");
            }

            if (suite.Arguments is null)
            {
                throw Invalid("catalog-suite-arguments=missing");
            }

            suites.Add(suite with
            {
                Arguments = Array.AsReadOnly(suite.Arguments.ToArray())
            });
        }

        if (catalog.Shards is null)
        {
            throw Invalid("catalog-shards=missing");
        }

        if (catalog.Shards.Count == 0)
        {
            throw Invalid("catalog-shards=empty");
        }

        var shardIds = new HashSet<string>(StringComparer.Ordinal);
        var shards = new List<ShardDefinition>(catalog.Shards.Count);
        foreach (var shard in catalog.Shards)
        {
            if (shard is null)
            {
                throw Invalid("catalog-shard=missing");
            }

            if (string.IsNullOrWhiteSpace(shard.Id))
            {
                throw Invalid("catalog-shard-id=empty");
            }

            if (!shardIds.Add(shard.Id))
            {
                throw Invalid("catalog-shard-id=duplicate");
            }

            if (shard.SuiteIds is null)
            {
                throw Invalid("catalog-shard-members=missing");
            }

            if (shard.SuiteIds.Count == 0)
            {
                throw Invalid("catalog-shard-members=empty");
            }

            var memberIds = new HashSet<string>(StringComparer.Ordinal);
            var members = shard.SuiteIds.ToArray();
            foreach (var member in members)
            {
                if (string.IsNullOrWhiteSpace(member))
                {
                    throw Invalid("catalog-shard-member=empty");
                }

                if (!memberIds.Add(member))
                {
                    throw Invalid("catalog-shard-member=duplicate");
                }

                if (!suiteIds.Contains(member))
                {
                    throw Invalid("catalog-shard-member=unknown");
                }
            }

            shards.Add(shard with { SuiteIds = Array.AsReadOnly(members) });
        }

        return catalog with
        {
            Suites = Array.AsReadOnly(suites.ToArray()),
            Shards = Array.AsReadOnly(shards.ToArray())
        };
    }

    private static VerificationPlan CreatePlan(
        VerificationRequest request,
        VerificationLane effectiveLane,
        TierResolution tier,
        string? promotionReason,
        ExecutionScope executionScope,
        string? shardId,
        IEnumerable<SuiteDefinition> suites) =>
        new(
            request,
            effectiveLane,
            tier.EffectiveTier,
            tier.Reasons,
            promotionReason,
            executionScope,
            shardId,
            effectiveLane == VerificationLane.Release,
            Array.AsReadOnly(suites.ToArray()));

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);
}

internal static class FastSuitePolicy
{
    internal static FastSuiteResolution Resolve(IReadOnlyList<string> changedPaths, VerificationCatalog catalog)
    {
        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
        var hasUnknownPath = false;

        foreach (var path in changedPaths)
        {
            var normalized = path.Replace('\\', '/');
            var mapped = false;

            if (PolicyPatterns.SourceOrProject.IsMatch(normalized))
            {
                selectedIds.Add("dotnet");
                mapped = true;
            }

            if (PolicyPatterns.WebDomain.IsMatch(normalized))
            {
                selectedIds.Add("web-typecheck");
                selectedIds.Add("web-parity");
                mapped = true;
            }

            if (PolicyPatterns.Installer.IsMatch(normalized))
            {
                selectedIds.Add("installer-contracts");
                mapped = true;
            }

            if (PolicyPatterns.Packaging.IsMatch(normalized))
            {
                selectedIds.Add("delivery-contracts");
                mapped = true;
            }

            var isCurrentEvidenceAnchor = PolicyPatterns.CurrentEvidenceAnchor.IsMatch(normalized);
            if (isCurrentEvidenceAnchor)
            {
                selectedIds.Add("evidence-check");
                mapped = true;
            }

            if (!isCurrentEvidenceAnchor && PolicyPatterns.Docs.IsMatch(normalized))
            {
                selectedIds.Add("policy-boundaries");
                mapped = true;
            }

            if (!mapped)
            {
                hasUnknownPath = true;
            }
        }

        var suites = catalog.Suites.Where(suite => selectedIds.Contains(suite.Id)).ToArray();
        return new FastSuiteResolution(Array.AsReadOnly(suites), hasUnknownPath);
    }
}

internal static class PolicyPatterns
{
    private const RegexOptions Options =
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

    internal static Regex Installer { get; } = new(
        @"^packaging/windows-desktop-node/installer(?:/|$)", Options);

    internal static Regex HostMutation { get; } = new(
        @"^packaging/windows-desktop-node/(?:tools|tests)/.*(?:HostMutation|OsMutation|FullAdminHostMutation)", Options);

    internal static Regex SecurityPolicy { get; } = new(
        @"^docs/(?:adr/(?:0003|0009|0010)-|.*(?:security|credential|token|tls|trust).*policy)", Options);

    internal static Regex CurrentEvidenceAnchor { get; } = new(
        @"^(?:AGENTS\.md|docs/ga-ready/(?:current-evidence(?:\.schema)?\.json|EVIDENCE_INDEX\.md|CURRENT_EVIDENCE_LEDGER\.md|CONTROL_PLANE_INDEX\.md)|docs/DEVELOPMENT_VERIFICATION_POLICY\.md|packaging/windows-desktop-node/README\.md)$",
        Options);

    internal static Regex PublicReleaseBoundary { get; } = new(
        @"^(?:docs/PUBLIC_RELEASE_BOUNDARY\.md|docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX\.md|docs/adr/0005-[^/]*|\.github/workflows/[^/]*(?:release|publish)[^/]*\.ya?ml)$",
        Options);

    internal static Regex SigningPublicationToken { get; } = new(
        @"(?:^|[-_./]|(?-i:(?<=[a-z0-9])(?=[A-Z])))(?:signing|signed|sign|publication|publish)(?=$|[-_./]|(?-i:(?<=[a-z])(?=[A-Z])))",
        Options);

    internal static Regex DesktopNodeApiCli { get; } = new(
        @"^src/DesktopNode\.(?:Api|Cli)(?:/|$)", Options);

    internal static Regex WebSourceTests { get; } = new(@"^web/(?:src|tests)(?:/|$)", Options);

    internal static Regex WebContractToken { get; } = new(
        @"(?:^|[-_./]|(?-i:(?<=[a-z0-9])(?=[A-Z])))(?:api|contract|client|auth)(?=$|[-_./]|(?-i:(?<=[a-z])(?=[A-Z])))",
        Options);

    internal static Regex Packaging { get; } = new(
        @"^packaging/windows-desktop-node(?:/|$)", Options);

    internal static Regex DevelopmentVerificationBoundary { get; } = new(
        @"^(?:src/DesktopNode\.Verification(?:\.Tests)?(?:/.*)?|config/development-verification-suites(?:\.schema)?\.json|docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design\.md|docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-wave-a\.md|\.github/workflows/development-gates\.yml)$",
        Options);

    internal static Regex SourceDomain { get; } = new(@"^src/([^/]+)(?:/|$)", Options);
    internal static Regex WebDomain { get; } = new(@"^web(?:/|$)", Options);
    internal static Regex SourceOrProject { get; } = new(@"^(?:src/|.*\.(?:sln|csproj)$)", Options);
    internal static Regex Docs { get; } = new(@"^docs(?:/|$)", Options);
}
