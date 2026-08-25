namespace DesktopNode.Verification.Tests;

public sealed class VerificationPolicyTests
{
    private static readonly string[] AllSuiteIds =
    [
        "dotnet",
        "web-typecheck",
        "web-parity",
        "delivery-contracts",
        "installer-contracts",
        "evidence-check",
        "policy-boundaries"
    ];

    [Theory]
    [InlineData("Fast", "S", "src/DesktopNode.Runtime/Internal.cs", "Fast", "S", "dotnet")]
    [InlineData("Fast", "S", "src/DesktopNode.Api/Program.cs", "Full", "M", "dotnet,web-typecheck,web-parity,delivery-contracts,installer-contracts,evidence-check,policy-boundaries")]
    [InlineData("Fast", "S", "packaging/windows-desktop-node/installer/Product.wxs", "Release", "L", "dotnet,web-typecheck,web-parity,delivery-contracts,installer-contracts,evidence-check,policy-boundaries")]
    [InlineData("Fast", "S", "unclassified/new.txt", "Full", "S", "dotnet,web-typecheck,web-parity,delivery-contracts,installer-contracts,evidence-check,policy-boundaries")]
    public void ResolvesLaneTierAndOrderedSuites(
        string lane,
        string tier,
        string path,
        string effectiveLane,
        string effectiveTier,
        string suiteIds)
    {
        var plan = Plan(VerificationRequestFixture.Create(lane, tier, [path]));

        Assert.Equal(Enum.Parse<VerificationLane>(effectiveLane), plan.EffectiveLane);
        Assert.Equal(Enum.Parse<ChangeTier>(effectiveTier), plan.EffectiveChangeTier);
        Assert.Equal(suiteIds.Split(','), Ids(plan));
    }

    [Fact]
    public void SuiteSelectionIsPartialAndSortedByCatalogOrder()
    {
        var request = VerificationRequestFixture.Create("Full", "M", ["src/a.cs"]) with
        {
            SuiteIds = Array.AsReadOnly(["policy-boundaries", "dotnet"])
        };

        var plan = Plan(request);

        Assert.Equal(ExecutionScope.Partial, plan.ExecutionScope);
        Assert.Null(plan.ShardId);
        Assert.Equal(["dotnet", "policy-boundaries"], Ids(plan));
    }

    [Theory]
    [InlineData("dotnet", "dotnet")]
    [InlineData("web", "web-typecheck,web-parity")]
    [InlineData("delivery", "delivery-contracts,evidence-check")]
    [InlineData("installer-policy", "installer-contracts,policy-boundaries")]
    public void UsesOnlyCatalogDefinedShards(string shardId, string suiteIds)
    {
        var request = VerificationRequestFixture.Create(
            "Full",
            "M",
            [".github/workflows/development-gates.yml"]) with
        {
            ShardId = shardId
        };

        var plan = Plan(request);

        Assert.Equal(ExecutionScope.Shard, plan.ExecutionScope);
        Assert.Equal(shardId, plan.ShardId);
        Assert.Equal(suiteIds.Split(','), Ids(plan));
    }

    [Theory]
    [InlineData("Full", false)]
    [InlineData("Release", true)]
    public void FullAndReleaseLanesUseAllSuites(string lane, bool releasePreflight)
    {
        var plan = Plan(VerificationRequestFixture.Create(lane, "S", ["src/a.cs"]));

        Assert.Equal(AllSuiteIds, Ids(plan));
        Assert.Equal(ExecutionScope.Lane, plan.ExecutionScope);
        Assert.Equal(releasePreflight, plan.ReleasePreflight);
    }

    [Fact]
    public void UnknownSuiteHasExactCodeAndStableDetail()
    {
        var request = VerificationRequestFixture.Create("Full", "M", ["src/a.cs"]) with
        {
            SuiteIds = Array.AsReadOnly(["private-suite"])
        };

        var exception = Assert.Throws<VerificationException>(() => Plan(request));

        Assert.Equal(VerificationErrorCodes.UnknownSuite, exception.Code);
        Assert.Equal("suite=private-suite", exception.Detail);
    }

    [Theory]
    [InlineData("duplicate", "selection=duplicate-suite")]
    [InlineData("suite-and-shard", "selection=suite-and-shard")]
    [InlineData("unknown-shard", "shard=private-shard")]
    public void InvalidSelectionHasStableNonsecretDetail(string scenario, string detail)
    {
        var request = VerificationRequestFixture.Create("Full", "M", ["src/a.cs"]);
        request = scenario switch
        {
            "duplicate" => request with { SuiteIds = Array.AsReadOnly(["dotnet", "dotnet"]) },
            "suite-and-shard" => request with
            {
                SuiteIds = Array.AsReadOnly(["dotnet"]),
                ShardId = "web"
            },
            _ => request with { ShardId = "private-shard" }
        };

        var exception = Assert.Throws<VerificationException>(() => Plan(request));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(detail, exception.Detail);
    }

    public static TheoryData<string, string> LargeTierPaths => new()
    {
        { "packaging/windows-desktop-node/installer/Product.wxs", "installer-lifecycle" },
        { "PACKAGING\\WINDOWS-DESKTOP-NODE\\tests\\Invoke-PcvFullAdminHostMutation.Tests.ps1", "host-mutation-boundary" },
        { "docs/adr/0003-internal-trusted-signing-policy.md", "security-policy-boundary" },
        { "docs/adr/0009-guest-execution.md", "security-policy-boundary" },
        { "docs/adr/0010-token-boundary.md", "security-policy-boundary" },
        { "docs/security/credential-rotation-policy.md", "security-policy-boundary" },
        { "AGENTS.md", "current-evidence-anchor" },
        { "docs/ga-ready/current-evidence.json", "current-evidence-anchor" },
        { "docs/ga-ready/current-evidence.schema.json", "current-evidence-anchor" },
        { "docs/ga-ready/EVIDENCE_INDEX.md", "current-evidence-anchor" },
        { "docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md", "current-evidence-anchor" },
        { "docs/ga-ready/CONTROL_PLANE_INDEX.md", "current-evidence-anchor" },
        { "docs/DEVELOPMENT_VERIFICATION_POLICY.md", "current-evidence-anchor" },
        { "packaging/windows-desktop-node/README.md", "current-evidence-anchor" },
        { "docs/PUBLIC_RELEASE_BOUNDARY.md", "public-release-boundary" },
        { "docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md", "public-release-boundary" },
        { "docs/adr/0005-release-policy.md", "public-release-boundary" },
        { ".github/workflows/publish-release.yml", "public-release-boundary" },
        { "docs/signing/rotation.md", "signing-publication-boundary" },
        { "packaging/windows-desktop-node/tools/New-PcvPublicSignedPreflight.ps1", "signing-publication-boundary" }
    };

    [Theory]
    [MemberData(nameof(LargeTierPaths))]
    public void LargeTierRulesMatchCaseInsensitivelyAndNormalizeSeparators(string path, string reason)
    {
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, [path]);

        Assert.Equal(ChangeTier.L, resolution.EffectiveTier);
        Assert.Contains(reason, resolution.Reasons);
    }

    public static TheoryData<string, string> MediumTierPaths => new()
    {
        { "src/DesktopNode.Api/Program.cs", "api-cli-web-contract" },
        { "src/DesktopNode.Cli/Program.cs", "api-cli-web-contract" },
        { "web/tests/client/session.test.ts", "api-cli-web-contract" },
        { "packaging/windows-desktop-node/tools/Build.ps1", "packaging-contract" },
        { "src/DesktopNode.Verification/VerificationPolicy.cs", "development-verification-boundary" },
        { "src/DesktopNode.Verification.Tests/VerificationPolicyTests.cs", "development-verification-boundary" },
        { "config/development-verification-suites.json", "development-verification-boundary" },
        { "config/development-verification-suites.schema.json", "development-verification-boundary" },
        { "docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design.md", "development-verification-boundary" },
        { "docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-wave-a.md", "development-verification-boundary" },
        { ".github/workflows/development-gates.yml", "development-verification-boundary" }
    };

    [Theory]
    [MemberData(nameof(MediumTierPaths))]
    public void MediumTierRulesStayMediumWithoutARealLargeRule(string path, string reason)
    {
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, [path]);

        Assert.Equal(ChangeTier.M, resolution.EffectiveTier);
        Assert.Contains(reason, resolution.Reasons);
        Assert.DoesNotContain("signing-publication-boundary", resolution.Reasons);
    }

    [Fact]
    public void CrossModuleChangeRaisesTierAfterPathRules()
    {
        var resolution = ChangeTierPolicy.Resolve(
            ChangeTier.S,
            ["src/DesktopNode.Runtime/Internal.cs", "web/styles/site.css"]);

        Assert.Equal(ChangeTier.M, resolution.EffectiveTier);
        Assert.Equal(["cross-module-change"], resolution.Reasons);
    }

    [Fact]
    public void ReasonsAreUniqueInFirstEncounterOrderAndRequestedHigherTierIsPreserved()
    {
        var resolution = ChangeTierPolicy.Resolve(
            ChangeTier.L,
            [
                "src/DesktopNode.Api/Program.cs",
                "src/DesktopNode.Api/Other.cs",
                "packaging/windows-desktop-node/installer/Product.wxs"
            ]);

        Assert.Equal(ChangeTier.L, resolution.RequestedTier);
        Assert.Equal(ChangeTier.L, resolution.EffectiveTier);
        Assert.Equal(
            ["api-cli-web-contract", "installer-lifecycle", "packaging-contract", "cross-module-change"],
            resolution.Reasons);
    }

    [Theory]
    [InlineData("Release", "M", "unclassified/new.txt", "Release", null)]
    [InlineData("Full", "L", "src/a.cs", "Release", "tier-l-requires-release")]
    [InlineData("Full", "M", "unclassified/new.txt", "Full", null)]
    [InlineData("Fast", "M", "unclassified/new.txt", "Full", "tier-m-requires-full")]
    [InlineData("Fast", "L", "unclassified/new.txt", "Release", "tier-l-requires-release")]
    [InlineData("Fast", "S", "unclassified/new.txt", "Full", "unknown-change-scope")]
    public void PromotionPrecedenceAndNullSemanticsAreExact(
        string lane,
        string tier,
        string path,
        string effectiveLane,
        string? promotionReason)
    {
        var plan = Plan(VerificationRequestFixture.Create(lane, tier, [path]));

        Assert.Equal(Enum.Parse<VerificationLane>(effectiveLane), plan.EffectiveLane);
        Assert.Equal(promotionReason, plan.PromotionReason);
    }

    [Fact]
    public void FastMappingUsesCatalogOrderAndUnknownMixturePromotesAllSuites()
    {
        var mapped = Plan(VerificationRequestFixture.Create(
            "Fast",
            "S",
            ["web/src/app.ts", "docs/notes.md"]));
        var unknown = Plan(VerificationRequestFixture.Create(
            "Fast",
            "S",
            ["src/DesktopNode.Runtime/a.cs", "unclassified/new.txt"]));

        Assert.Equal(["web-typecheck", "web-parity", "policy-boundaries"], Ids(mapped));
        Assert.Equal(VerificationLane.Full, unknown.EffectiveLane);
        Assert.Equal("unknown-change-scope", unknown.PromotionReason);
        Assert.Equal(AllSuiteIds, Ids(unknown));
    }

    [Fact]
    public void EmptyFastChangeSetFallsBackToFullWithoutChangingTier()
    {
        var plan = Plan(VerificationRequestFixture.Create("Fast", "S", []));

        Assert.Empty(plan.Request.ChangedPaths);
        Assert.Equal(VerificationLane.Full, plan.EffectiveLane);
        Assert.Equal(ChangeTier.S, plan.EffectiveChangeTier);
        Assert.Equal("unknown-change-scope", plan.PromotionReason);
        Assert.Equal(ExecutionScope.Lane, plan.ExecutionScope);
        Assert.Equal(AllSuiteIds, Ids(plan));
    }

    [Fact]
    public void InstallerFastMappingUnionsDeliveryAndInstallerSuites()
    {
        var request = VerificationRequestFixture.Create(
            "Fast",
            "S",
            ["packaging/windows-desktop-node/installer/Product.wxs"]);
        var selected = FastSuitePolicy.Resolve(request.ChangedPaths, VerificationCatalogFixture.LoadCanonical());

        Assert.Equal(["delivery-contracts", "installer-contracts"], selected.Suites.Select(suite => suite.Id));
        Assert.False(selected.HasUnknownPath);
    }

    [Fact]
    public void ExplicitAllSuitesRemainPartial()
    {
        var request = VerificationRequestFixture.Create("Full", "M", ["src/a.cs"]) with
        {
            SuiteIds = Array.AsReadOnly(AllSuiteIds.ToArray())
        };

        var plan = Plan(request);

        Assert.Equal(ExecutionScope.Partial, plan.ExecutionScope);
        Assert.Null(plan.ShardId);
    }

    [Fact]
    public void ReturnedCollectionsAreImmutableSnapshots()
    {
        var paths = new List<string> { "src/DesktopNode.Api/Program.cs" };
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, paths);
        var plan = Plan(VerificationRequestFixture.Create("Fast", "S", ["src/a.cs"]));
        paths.Add("packaging/windows-desktop-node/installer/Product.wxs");

        Assert.Equal(["api-cli-web-contract"], resolution.Reasons);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)resolution.Reasons).Add("changed"));
        Assert.Throws<NotSupportedException>(() => ((IList<SuiteDefinition>)plan.Suites).Clear());
    }

    [Fact]
    public void PlanDeeplySnapshotsCallerOwnedRequestAndSuiteArguments()
    {
        var changedPaths = new List<string> { "src/DesktopNode.Runtime/Internal.cs" };
        var suiteIds = new List<string> { "dotnet" };
        var arguments = new List<string> { "test", "DesktopNode.sln" };
        var canonical = VerificationCatalogFixture.LoadCanonical();
        var originalSuite = canonical.Suites[0] with { Arguments = arguments };
        var callerSuites = canonical.Suites.ToList();
        callerSuites[0] = originalSuite;
        var catalog = canonical with { Suites = callerSuites };
        var request = new VerificationRequest(
            VerificationLane.Fast,
            ChangeTier.S,
            changedPaths,
            "artifacts/test-run",
            suiteIds,
            null,
            PlanOnly: true);

        var plan = VerificationPlanner.Create(request, catalog);
        changedPaths.Clear();
        changedPaths.Add("packaging/windows-desktop-node/installer/Product.wxs");
        suiteIds.Clear();
        suiteIds.Add("policy-boundaries");
        arguments.Clear();
        arguments.Add("changed");
        callerSuites[0] = originalSuite with { Owner = "changed" };

        Assert.Equal(["src/DesktopNode.Runtime/Internal.cs"], plan.Request.ChangedPaths);
        Assert.Equal(["dotnet"], plan.Request.SuiteIds);
        Assert.Equal(VerificationLane.Fast, plan.EffectiveLane);
        Assert.Equal(ChangeTier.S, plan.EffectiveChangeTier);
        var selected = Assert.Single(plan.Suites);
        Assert.Equal(originalSuite.Id, selected.Id);
        Assert.Equal(originalSuite.Owner, selected.Owner);
        Assert.Equal(originalSuite.MigrationState, selected.MigrationState);
        Assert.Equal(originalSuite.ExecutorKind, selected.ExecutorKind);
        Assert.Equal(originalSuite.FileName, selected.FileName);
        Assert.Equal(["test", "DesktopNode.sln"], selected.Arguments);
        Assert.Equal(originalSuite.ManagedHandler, selected.ManagedHandler);
        Assert.Equal(originalSuite.TimeoutSeconds, selected.TimeoutSeconds);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)plan.Request.ChangedPaths).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)plan.Request.SuiteIds).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)selected.Arguments).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<SuiteDefinition>)plan.Suites).Clear());
    }

    [Theory]
    [InlineData("request", "request=missing")]
    [InlineData("paths-null", "changed-paths=missing")]
    [InlineData("path-empty", "changed-path=empty")]
    [InlineData("suites-null", "suite-ids=missing")]
    [InlineData("suite-empty", "suite-id=empty")]
    [InlineData("shard-empty", "shard-id=empty")]
    public void InvalidRequestBoundaryUsesStableConfigDetail(string scenario, string detail)
    {
        var request = VerificationRequestFixture.Create("Fast", "S", ["src/a.cs"]);
        request = scenario switch
        {
            "paths-null" => request with { ChangedPaths = null! },
            "path-empty" => request with { ChangedPaths = Array.AsReadOnly(["  "]) },
            "suites-null" => request with { SuiteIds = null! },
            "suite-empty" => request with { SuiteIds = Array.AsReadOnly(["  "]) },
            "shard-empty" => request with { ShardId = "  " },
            _ => request
        };

        var exception = Assert.Throws<VerificationException>(() =>
            VerificationPlanner.Create(scenario == "request" ? null! : request, VerificationCatalogFixture.LoadCanonical()));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(detail, exception.Detail);
    }

    [Theory]
    [InlineData("catalog", "catalog=missing")]
    [InlineData("suites-null", "catalog-suites=missing")]
    [InlineData("suite-id-empty", "catalog-suite-id=empty")]
    [InlineData("suite-id-duplicate", "catalog-suite-id=duplicate")]
    [InlineData("suite-arguments-null", "catalog-suite-arguments=missing")]
    [InlineData("shards-null", "catalog-shards=missing")]
    [InlineData("shard-id-empty", "catalog-shard-id=empty")]
    [InlineData("shard-id-duplicate", "catalog-shard-id=duplicate")]
    [InlineData("members-null", "catalog-shard-members=missing")]
    [InlineData("members-empty", "catalog-shard-members=empty")]
    [InlineData("member-empty", "catalog-shard-member=empty")]
    [InlineData("member-duplicate", "catalog-shard-member=duplicate")]
    [InlineData("member-unknown", "catalog-shard-member=unknown")]
    public void MalformedCatalogUsesStableConfigDetail(string scenario, string detail)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        catalog = MalformedCatalog(catalog, scenario);

        var exception = Assert.Throws<VerificationException>(() =>
            VerificationPlanner.Create(
                VerificationRequestFixture.Create("Full", "S", ["src/a.cs"]),
                scenario == "catalog" ? null! : catalog));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(detail, exception.Detail);
    }

    [Theory]
    [InlineData(".github/workflows/development-gates.yml", "development-verification-boundary")]
    [InlineData(".github/workflows/product-release.yml", "public-release-boundary")]
    [InlineData(".github/workflows/publish-product.yaml", "public-release-boundary")]
    [InlineData(".github/workflows/signing-rotation.md", "signing-publication-boundary")]
    [InlineData(".github/workflows/publication-notes.txt", "signing-publication-boundary")]
    public void WorkflowAndTokenRulesRespectTheirIndependentBoundaries(string path, string reason)
    {
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, [path]);

        Assert.Contains(reason, resolution.Reasons);
    }

    [Theory]
    [InlineData(".github/workflows/development-gates.txt")]
    [InlineData(".github/workflows/development-gates.yaml")]
    [InlineData(".github/workflows/product-release.md")]
    public void NonYamlWorkflowFilesWithoutSigningTokensRemainUnclassified(string path)
    {
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, [path]);

        Assert.Equal(ChangeTier.S, resolution.EffectiveTier);
        Assert.Empty(resolution.Reasons);
    }

    [Fact]
    public void NonYamlPublishWorkflowUsesOnlyExtensionIndependentSigningRule()
    {
        var resolution = ChangeTierPolicy.Resolve(
            ChangeTier.S,
            [".github/workflows/publish-product.txt"]);

        Assert.Equal(ChangeTier.L, resolution.EffectiveTier);
        Assert.Equal(["signing-publication-boundary"], resolution.Reasons);
        Assert.DoesNotContain("public-release-boundary", resolution.Reasons);
    }

    [Theory]
    [InlineData("web/src/api-client.ts")]
    [InlineData("web/tests/services/ApiClient.test.ts")]
    [InlineData("WEB/SRC/Auth/Contract.ts")]
    public void WebContractTokensMatchSeparatorAndCamelCaseBoundaries(string path)
    {
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, [path]);

        Assert.Equal(ChangeTier.M, resolution.EffectiveTier);
        Assert.Equal(["api-cli-web-contract"], resolution.Reasons);
    }

    [Theory]
    [InlineData("web/src/rapid.ts")]
    [InlineData("web/src/capitalization.ts")]
    [InlineData("web/tests/authoring.test.ts")]
    [InlineData("web/src/design.ts")]
    public void WebContractTokensRejectSubstrings(string path)
    {
        var resolution = ChangeTierPolicy.Resolve(ChangeTier.S, [path]);

        Assert.Equal(ChangeTier.S, resolution.EffectiveTier);
        Assert.Empty(resolution.Reasons);
    }

    private static VerificationCatalog MalformedCatalog(VerificationCatalog catalog, string scenario)
    {
        var suites = catalog.Suites.ToArray();
        var shards = catalog.Shards.ToArray();
        return scenario switch
        {
            "suites-null" => catalog with { Suites = null! },
            "suite-id-empty" => catalog with
            {
                Suites = Array.AsReadOnly(suites.Select((suite, index) =>
                    index == 0 ? suite with { Id = " " } : suite).ToArray())
            },
            "suite-id-duplicate" => catalog with
            {
                Suites = Array.AsReadOnly(suites.Select((suite, index) =>
                    index == 1 ? suite with { Id = suites[0].Id } : suite).ToArray())
            },
            "suite-arguments-null" => catalog with
            {
                Suites = Array.AsReadOnly(suites.Select((suite, index) =>
                    index == 0 ? suite with { Arguments = null! } : suite).ToArray())
            },
            "shards-null" => catalog with { Shards = null! },
            "shard-id-empty" => catalog with
            {
                Shards = Array.AsReadOnly(shards.Select((shard, index) =>
                    index == 0 ? shard with { Id = " " } : shard).ToArray())
            },
            "shard-id-duplicate" => catalog with
            {
                Shards = Array.AsReadOnly(shards.Select((shard, index) =>
                    index == 1 ? shard with { Id = shards[0].Id } : shard).ToArray())
            },
            "members-null" => catalog with
            {
                Shards = Array.AsReadOnly(shards.Select((shard, index) =>
                    index == 0 ? shard with { SuiteIds = null! } : shard).ToArray())
            },
            "members-empty" => catalog with
            {
                Shards = Array.AsReadOnly(shards.Select((shard, index) =>
                    index == 0 ? shard with { SuiteIds = Array.Empty<string>() } : shard).ToArray())
            },
            "member-empty" => ReplaceFirstShardMembers(catalog, shards, [" "]),
            "member-duplicate" => ReplaceFirstShardMembers(catalog, shards, ["dotnet", "dotnet"]),
            "member-unknown" => ReplaceFirstShardMembers(catalog, shards, ["private-suite"]),
            _ => catalog
        };
    }

    private static VerificationCatalog ReplaceFirstShardMembers(
        VerificationCatalog catalog,
        IReadOnlyList<ShardDefinition> shards,
        string[] members) =>
        catalog with
        {
            Shards = Array.AsReadOnly(shards.Select((shard, index) =>
                index == 0 ? shard with { SuiteIds = Array.AsReadOnly(members) } : shard).ToArray())
        };

    private static VerificationPlan Plan(VerificationRequest request) =>
        VerificationPlanner.Create(request, VerificationCatalogFixture.LoadCanonical());

    private static string[] Ids(VerificationPlan plan) =>
        plan.Suites.Select(suite => suite.Id).ToArray();
}
