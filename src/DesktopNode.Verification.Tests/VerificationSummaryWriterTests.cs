using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Verification.Tests;

public sealed class VerificationSummaryWriterTests
{
    private static readonly DateTimeOffset StartedAt =
        DateTimeOffset.Parse("2026-08-24T09:00:00+09:00");

    [Fact]
    public void PlanOnlySummaryHasDeterministicContractAndOrdering()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: true);
        var report = VerificationReportFixture.Planned(plan.Suites);

        var summary = VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt.AddMilliseconds(1250));
        var json = VerificationJson.Serialize(summary);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-development-verification-summary-v2", root.GetProperty("contract").GetString());
        Assert.Equal("Full", root.GetProperty("requested_lane").GetString());
        Assert.Equal("Full", root.GetProperty("effective_lane").GetString());
        Assert.Equal("M", root.GetProperty("requested_change_tier").GetString());
        Assert.Equal("M", root.GetProperty("change_tier").GetString());
        Assert.Equal("lane", root.GetProperty("execution_scope").GetString());
        Assert.True(root.GetProperty("plan_only").GetBoolean());
        Assert.True(root.GetProperty("ok").GetBoolean());
        Assert.Equal(catalog.ActivationState, root.GetProperty("catalog_activation_state").GetString());
        Assert.Equal(1250, root.GetProperty("duration_ms").GetInt64());
        Assert.Equal(
            [
                "schema_version", "contract", "requested_lane", "effective_lane",
                "requested_change_tier", "change_tier", "tier_reasons", "execution_scope",
                "plan_only", "catalog_activation_state", "ok", "started_at", "completed_at",
                "duration_ms", "results"
            ],
            root.EnumerateObject().Select(property => property.Name));

        var results = root.GetProperty("results").EnumerateArray().ToArray();
        Assert.Equal(7, results.Length);
        Assert.Equal(catalog.Suites.Select(suite => suite.Id),
            results.Select(row => row.GetProperty("suite_id").GetString()));
        Assert.All(results, row =>
        {
            Assert.Equal("planned", row.GetProperty("status").GetString());
            Assert.Equal(
                ["suite_id", "status", "migration_state", "duration_ms", "timed_out", "cancelled"],
                row.EnumerateObject().Select(property => property.Name));
        });
        Assert.False(root.TryGetProperty("promotion_reason", out _));
        Assert.False(root.TryGetProperty("shard_id", out _));
        Assert.False(root.TryGetProperty("error_code", out _));
        Assert.Equal(TimeSpan.Zero, root.GetProperty("started_at").GetDateTimeOffset().Offset);
        Assert.Equal(TimeSpan.Zero, root.GetProperty("completed_at").GetDateTimeOffset().Offset);
        Assert.DoesNotContain("Authorization", json, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("shadow-ready")]
    [InlineData("active")]
    public void SummaryAcceptsSupportedExecutionActivationState(string activationState)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical() with { ActivationState = activationState };
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: true);
        var report = VerificationReportFixture.Planned(plan.Suites);

        var summary = VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt);

        Assert.Equal(activationState, summary.CatalogActivationState);
    }

    [Fact]
    public void IdenticalPlanOnlyInputsAndTimestampsSerializeByteForByte()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: true);
        var report = VerificationReportFixture.Planned(plan.Suites);

        var first = VerificationJson.Serialize(VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt.AddSeconds(2)));
        var second = VerificationJson.Serialize(VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt.AddSeconds(2)));

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData("partial")]
    [InlineData("shard")]
    public void SelectedExecutionScopeCannotSerializeAsLane(string requestedScope)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var baseline = VerificationPlanFixture.ForCatalog(catalog, planOnly: true);
        var request = requestedScope == "partial"
            ? baseline.Request with
            {
                SuiteIds = Array.AsReadOnly(new[] { catalog.Suites[0].Id }),
                ShardId = null
            }
            : baseline.Request with
            {
                SuiteIds = Array.Empty<string>(),
                ShardId = catalog.Shards[0].Id
            };
        var plan = VerificationPlanner.Create(request, catalog);

        var summary = VerificationSummaryFactory.Create(
            plan, catalog, VerificationReportFixture.Planned(plan.Suites), StartedAt, StartedAt);
        using var document = JsonDocument.Parse(VerificationJson.Serialize(summary));
        var serializedScope = document.RootElement.GetProperty("execution_scope").GetString();

        Assert.Equal(requestedScope, serializedScope);
        Assert.NotEqual("lane", serializedScope);
    }

    [Theory]
    [InlineData("Missing", VerificationErrorCodes.ParityUnmapped)]
    [InlineData("TimedOut", VerificationErrorCodes.Timeout)]
    [InlineData("Cancelled", VerificationErrorCodes.Cancelled)]
    public void TerminalRowsFailAndChooseFirstCatalogOrderErrorCode(
        string statusName,
        string expectedErrorCode)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: false);
        var rows = catalog.Suites.Select(Passed).ToArray();
        var targetIndex = statusName == nameof(SuiteStatus.Missing) ? 5 : 1;
        var laterIndex = statusName == nameof(SuiteStatus.Missing) ? 6 : 4;
        rows[targetIndex] = Terminal(
            catalog.Suites[targetIndex], Enum.Parse<SuiteStatus>(statusName), expectedErrorCode);
        rows[laterIndex] = Terminal(
            catalog.Suites[laterIndex], SuiteStatus.Failed, VerificationErrorCodes.ProcessFailed);
        var report = new VerificationExecutionReport(4321, Array.AsReadOnly(rows));

        var summary = VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt.AddMinutes(9));
        using var document = JsonDocument.Parse(VerificationJson.Serialize(summary));
        var root = document.RootElement;

        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal(expectedErrorCode, root.GetProperty("error_code").GetString());
        Assert.Equal(4321, root.GetProperty("duration_ms").GetInt64());
        Assert.Equal(catalog.Suites.Select(suite => suite.Id),
            root.GetProperty("results").EnumerateArray()
                .Select(row => row.GetProperty("suite_id").GetString()));
    }

    [Fact]
    public void MapsEveryStatusAndRequiresEveryActualRowToPass()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: false);
        var statuses = new[]
        {
            SuiteStatus.Planned, SuiteStatus.Passed, SuiteStatus.Failed, SuiteStatus.TimedOut,
            SuiteStatus.Cancelled, SuiteStatus.Missing, SuiteStatus.Passed
        };
        var codes = new string?[]
        {
            null, null, VerificationErrorCodes.ProcessFailed, VerificationErrorCodes.Timeout,
            VerificationErrorCodes.Cancelled, VerificationErrorCodes.ParityUnmapped, null
        };
        var rows = catalog.Suites.Select((suite, index) =>
            statuses[index] is SuiteStatus.Planned
                ? Planned(suite)
                : statuses[index] is SuiteStatus.Passed
                    ? Passed(suite)
                    : Terminal(suite, statuses[index], codes[index]!)).ToArray();

        var summary = VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(99, Array.AsReadOnly(rows)),
            StartedAt,
            StartedAt.AddSeconds(1));
        using var document = JsonDocument.Parse(VerificationJson.Serialize(summary));

        Assert.False(document.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal(
            ["planned", "passed", "failed", "timed_out", "cancelled", "missing", "passed"],
            document.RootElement.GetProperty("results").EnumerateArray()
                .Select(row => row.GetProperty("status").GetString()));

        var passing = rows.Select((row, index) => Passed(catalog.Suites[index])).ToArray();
        var passingSummary = VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(101, Array.AsReadOnly(passing)),
            StartedAt,
            StartedAt.AddSeconds(1));
        Assert.True(passingSummary.Ok);
    }

    [Fact]
    public void OptionalPropertiesUseExactRootPositionsWhenPresent()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var baseline = VerificationPlanFixture.ForCatalog(catalog, planOnly: false);
        var request = baseline.Request with
        {
            RequestedLane = VerificationLane.Fast,
            ShardId = catalog.Shards[0].Id
        };
        var plan = VerificationPlanner.Create(request, catalog);
        var rows = plan.Suites.Select(Passed).ToArray();
        rows[0] = Terminal(plan.Suites[0], SuiteStatus.Failed, VerificationErrorCodes.ProcessFailed);

        var summary = VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(8, Array.AsReadOnly(rows)),
            StartedAt,
            StartedAt.AddSeconds(1));
        using var document = JsonDocument.Parse(VerificationJson.Serialize(summary));

        Assert.Equal(
            [
                "schema_version", "contract", "requested_lane", "effective_lane",
                "requested_change_tier", "change_tier", "tier_reasons", "promotion_reason",
                "execution_scope", "shard_id", "plan_only", "catalog_activation_state", "ok",
                "error_code", "started_at", "completed_at", "duration_ms", "results"
            ],
            document.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("tier-m-requires-full", document.RootElement.GetProperty("promotion_reason").GetString());
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("contract")]
    [InlineData("activation")]
    [InlineData("parallelism")]
    [InlineData("overall-timeout")]
    [InlineData("allowlist")]
    [InlineData("suite-truncated")]
    [InlineData("suite-extra")]
    [InlineData("suite-reordered")]
    [InlineData("owner")]
    [InlineData("migration")]
    [InlineData("suite-timeout")]
    [InlineData("managed-handler")]
    [InlineData("executor-union")]
    [InlineData("process-path")]
    [InlineData("process-not-normalized")]
    [InlineData("powershell")]
    [InlineData("host-mutation")]
    [InlineData("shard-order")]
    [InlineData("shard-overlap")]
    [InlineData("shard-incomplete")]
    public void CreateRejectsCatalogOutsideLoaderInMemoryContract(string scenario)
    {
        var canonical = VerificationCatalogFixture.LoadCanonical();
        var catalog = MutateCatalog(canonical, scenario);
        var request = VerificationPlanFixture.ForCatalog(canonical, planOnly: true).Request;
        var plan = VerificationPlanner.Create(request, catalog);
        var report = VerificationReportFixture.Planned(plan.Suites);

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt));
    }

    [Fact]
    public void CreateAcceptsLoaderValidSafeArgumentAndAllowlistOrderVariants()
    {
        var canonical = VerificationCatalogFixture.LoadCanonical();
        var processIndex = Array.FindIndex(
            canonical.Suites.ToArray(), suite => suite.ExecutorKind == "process");
        var suites = canonical.Suites.ToArray();
        suites[processIndex] = suites[processIndex] with
        {
            Arguments = Array.AsReadOnly(suites[processIndex].Arguments.Append("--safe-variant").ToArray())
        };
        var catalog = canonical with
        {
            AllowedExecutables = Array.AsReadOnly(canonical.AllowedExecutables.Reverse().ToArray()),
            Suites = Array.AsReadOnly(suites)
        };
        var request = VerificationPlanFixture.ForCatalog(canonical, planOnly: true).Request;
        var plan = VerificationPlanner.Create(request, catalog);

        var summary = VerificationSummaryFactory.Create(
            plan,
            catalog,
            VerificationReportFixture.Planned(plan.Suites),
            StartedAt,
            StartedAt);

        Assert.True(summary.Ok);
        Assert.Equal(7, summary.Results.Count);
    }

    [Theory]
    [InlineData("empty-suites")]
    [InlineData("shrunken-suites")]
    [InlineData("extra-suite")]
    [InlineData("reordered-suites")]
    [InlineData("request-selection")]
    [InlineData("request-unknown-suite")]
    [InlineData("effective-lane")]
    [InlineData("effective-tier")]
    [InlineData("tier-reasons")]
    [InlineData("promotion")]
    [InlineData("scope")]
    [InlineData("shard")]
    [InlineData("release-preflight")]
    [InlineData("suite-definition")]
    public void CreateRejectsEveryNonCanonicalPlanProjection(string scenario)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var canonical = VerificationPlanner.Create(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: true).Request,
            catalog);
        var extraSuite = canonical.Suites[0] with { Id = "extra-suite" };
        var plan = scenario switch
        {
            "empty-suites" => canonical with { Suites = Array.Empty<SuiteDefinition>() },
            "shrunken-suites" => canonical with
            {
                Suites = Array.AsReadOnly(canonical.Suites.Skip(1).ToArray())
            },
            "extra-suite" => canonical with
            {
                Suites = Array.AsReadOnly(canonical.Suites.Append(extraSuite).ToArray())
            },
            "reordered-suites" => canonical with
            {
                Suites = Array.AsReadOnly(canonical.Suites.Reverse().ToArray())
            },
            "request-selection" => canonical with
            {
                Request = canonical.Request with
                {
                    SuiteIds = Array.AsReadOnly(new[] { canonical.Suites[0].Id })
                }
            },
            "request-unknown-suite" => canonical with
            {
                Request = canonical.Request with
                {
                    SuiteIds = Array.AsReadOnly(new[] { "unknown-suite" })
                }
            },
            "effective-lane" => canonical with { EffectiveLane = VerificationLane.Release },
            "effective-tier" => canonical with { EffectiveChangeTier = ChangeTier.L },
            "tier-reasons" => canonical with { TierReasons = Array.AsReadOnly(new[] { "tampered" }) },
            "promotion" => canonical with { PromotionReason = "tier-l-requires-release" },
            "scope" => canonical with { ExecutionScope = ExecutionScope.Partial },
            "shard" => canonical with { ShardId = catalog.Shards[0].Id },
            "release-preflight" => canonical with { ReleasePreflight = true },
            "suite-definition" => canonical with
            {
                Suites = Array.AsReadOnly(canonical.Suites.Select((suite, index) =>
                    index == 0 ? suite with { TimeoutSeconds = suite.TimeoutSeconds + 1 } : suite).ToArray())
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var report = VerificationReportFixture.Planned(plan.Suites);

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan, catalog, report, StartedAt, StartedAt));
    }

    [Fact]
    public void CreateRejectsEmptyCatalogPlanAndReportInsteadOfVacuousPass()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical() with
        {
            Suites = Array.Empty<SuiteDefinition>()
        };
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: true) with
        {
            TierReasons = Array.Empty<string>()
        };

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(0, Array.Empty<SuiteExecutionRecord>()),
            StartedAt,
            StartedAt));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("reordered")]
    [InlineData("migration")]
    public void CreateRejectsReportThatIsNotExactlyBoundToCanonicalPlan(string scenario)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanner.Create(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: true).Request,
            catalog);
        var canonicalRows = VerificationReportFixture.Planned(plan.Suites).Results.ToArray();
        IReadOnlyList<SuiteExecutionRecord> rows = scenario switch
        {
            "empty" => Array.Empty<SuiteExecutionRecord>(),
            "reordered" => Array.AsReadOnly(canonicalRows.Reverse().ToArray()),
            "migration" => Array.AsReadOnly(canonicalRows.Select((row, index) =>
                index == 0 ? row with { MigrationState = "tampered" } : row).ToArray()),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(0, rows),
            StartedAt,
            StartedAt));
    }

    [Theory]
    [InlineData("planned-duration")]
    [InlineData("planned-output")]
    [InlineData("passed-process-exit")]
    [InlineData("passed-managed-output")]
    [InlineData("failed-process-zero")]
    [InlineData("failed-process-partial-output")]
    [InlineData("failed-process-unknown-code")]
    [InlineData("failed-managed-execution")]
    [InlineData("missing-process")]
    [InlineData("missing-wrong-code")]
    [InlineData("timeout-flag")]
    [InlineData("timeout-exit")]
    [InlineData("cancel-code")]
    [InlineData("unknown-status")]
    [InlineData("negative-duration")]
    public void CreateRejectsIncoherentSuiteTerminalMatrix(string scenario)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var processSuite = catalog.Suites.First(suite => suite.ExecutorKind == "process");
        var managedSuite = catalog.Suites.First(suite => suite.ExecutorKind == "managed");
        var suite = scenario is "passed-managed-output" or "failed-managed-execution" or "missing-wrong-code"
            ? managedSuite
            : processSuite;
        var row = scenario switch
        {
            "planned-duration" => Planned(suite) with { DurationMs = 1 },
            "planned-output" => Planned(suite) with { StandardOutput = "output" },
            "passed-process-exit" => Passed(suite) with { ExitCode = 2 },
            "passed-managed-output" => Passed(suite) with
            {
                StandardOutput = "output",
                StandardError = string.Empty,
                OutputSha256 = Hash("output", string.Empty)
            },
            "failed-process-zero" => Terminal(suite, SuiteStatus.Failed, VerificationErrorCodes.ProcessFailed) with
            {
                ExitCode = 0
            },
            "failed-process-partial-output" => Terminal(suite, SuiteStatus.Failed, VerificationErrorCodes.ProcessFailed) with
            {
                ExitCode = 1,
                StandardOutput = "output"
            },
            "failed-process-unknown-code" => Terminal(suite, SuiteStatus.Failed, "PCV_VERIFY_UNKNOWN_DETAIL"),
            "failed-managed-execution" => Terminal(suite, SuiteStatus.Failed, VerificationErrorCodes.ProcessFailed) with
            {
                ExitCode = 1
            },
            "missing-process" => Terminal(suite, SuiteStatus.Missing, VerificationErrorCodes.ParityUnmapped),
            "missing-wrong-code" => Terminal(suite, SuiteStatus.Missing, VerificationErrorCodes.ProcessFailed),
            "timeout-flag" => Terminal(suite, SuiteStatus.TimedOut, VerificationErrorCodes.Timeout) with
            {
                TimedOut = false
            },
            "timeout-exit" => Terminal(suite, SuiteStatus.TimedOut, VerificationErrorCodes.Timeout) with
            {
                ExitCode = 1
            },
            "cancel-code" => Terminal(suite, SuiteStatus.Cancelled, VerificationErrorCodes.ProcessFailed),
            "unknown-status" => Planned(suite) with { Status = (SuiteStatus)999 },
            "negative-duration" => Passed(suite) with { DurationMs = -1 },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var request = VerificationPlanFixture.ForCatalog(catalog, planOnly: false).Request with
        {
            SuiteIds = Array.AsReadOnly(new[] { suite.Id })
        };
        var plan = VerificationPlanner.Create(request, catalog);

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(1, Array.AsReadOnly(new[] { row })),
            StartedAt,
            StartedAt.AddMilliseconds(1)));
    }

    [Fact]
    public void PlanOnlyCoherentFailureSerializesFailureWithoutThrowing()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var plan = VerificationPlanner.Create(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: true).Request,
            catalog);
        var rows = VerificationReportFixture.Planned(plan.Suites).Results.ToArray();
        rows[0] = Terminal(plan.Suites[0], SuiteStatus.Failed, VerificationErrorCodes.ProcessFailed);

        var summary = VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(4, Array.AsReadOnly(rows)),
            StartedAt,
            StartedAt.AddMilliseconds(4));

        Assert.False(summary.Ok);
        Assert.Equal("failed", summary.Results[0].Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, summary.ErrorCode);
    }

    [Theory]
    [InlineData("authorization")]
    [InlineData("oversize")]
    [InlineData("hash-mismatch")]
    public void CreateRejectsUnsafeOrMismatchedProcessOutput(string scenario)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var suite = catalog.Suites.First(candidate => candidate.ExecutorKind == "process");
        var row = Passed(suite);
        row = scenario switch
        {
            "authorization" => WithOutput(row, "Authorization: Bearer actual-secret", string.Empty),
            "oversize" => WithOutput(row, new string('x', 8193), string.Empty),
            "hash-mismatch" => row with { OutputSha256 = new string('f', 64) },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var request = VerificationPlanFixture.ForCatalog(catalog, planOnly: false).Request with
        {
            SuiteIds = Array.AsReadOnly(new[] { suite.Id })
        };
        var plan = VerificationPlanner.Create(request, catalog);

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(1, Array.AsReadOnly(new[] { row })),
            StartedAt,
            StartedAt.AddMilliseconds(1)));
    }

    [Theory]
    [InlineData(VerificationErrorCodes.ConfigInvalid)]
    [InlineData(VerificationErrorCodes.UnknownSuite)]
    [InlineData(VerificationErrorCodes.NonAdminPowerShellForbidden)]
    [InlineData(VerificationErrorCodes.ArtifactRootInvalid)]
    public void ProcessFailureWithExecutionPayloadRejectsBoundaryOnlyErrorCode(string errorCode)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var suite = catalog.Suites.First(candidate => candidate.ExecutorKind == "process");
        var row = WithOutput(
            Terminal(suite, SuiteStatus.Failed, errorCode) with { ExitCode = 7 },
            "failed output",
            "failed error");
        var request = VerificationPlanFixture.ForCatalog(catalog, planOnly: false).Request with
        {
            SuiteIds = Array.AsReadOnly(new[] { suite.Id })
        };
        var plan = VerificationPlanner.Create(request, catalog);

        AssertConfigInvalid(() => VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(1, Array.AsReadOnly(new[] { row })),
            StartedAt,
            StartedAt.AddMilliseconds(1)));
    }

    [Theory]
    [InlineData(VerificationErrorCodes.ConfigInvalid)]
    [InlineData(VerificationErrorCodes.UnknownSuite)]
    [InlineData(VerificationErrorCodes.ProcessFailed)]
    [InlineData(VerificationErrorCodes.NonAdminPowerShellForbidden)]
    [InlineData(VerificationErrorCodes.ArtifactRootInvalid)]
    public void ProcessFailureWithoutExecutionPayloadAcceptsGenuineBoundaryCode(string errorCode)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var suite = catalog.Suites.First(candidate => candidate.ExecutorKind == "process");
        var row = Terminal(suite, SuiteStatus.Failed, errorCode);
        var request = VerificationPlanFixture.ForCatalog(catalog, planOnly: false).Request with
        {
            SuiteIds = Array.AsReadOnly(new[] { suite.Id })
        };
        var plan = VerificationPlanner.Create(request, catalog);

        var summary = VerificationSummaryFactory.Create(
            plan,
            catalog,
            new VerificationExecutionReport(1, Array.AsReadOnly(new[] { row })),
            StartedAt,
            StartedAt.AddMilliseconds(1));

        Assert.False(summary.Ok);
        Assert.Equal(errorCode, summary.ErrorCode);
    }

    [Fact]
    public void FailureWithoutPlanUsesRequestAndOmitsExceptionDetail()
    {
        var request = VerificationPlanFixture.Full(planOnly: true).Request with
        {
            RequestedLane = VerificationLane.Release,
            RequestedChangeTier = ChangeTier.L,
            SuiteIds = Array.AsReadOnly(new[] { "dotnet" })
        };

        var summary = VerificationSummaryFactory.CreateFailure(
            request,
            null,
            "plan-only-foundation",
            VerificationErrorCodes.ConfigInvalid,
            StartedAt.AddSeconds(1),
            StartedAt);
        var json = VerificationJson.Serialize(summary);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Release", root.GetProperty("requested_lane").GetString());
        Assert.Equal("Release", root.GetProperty("effective_lane").GetString());
        Assert.Equal("L", root.GetProperty("requested_change_tier").GetString());
        Assert.Equal("L", root.GetProperty("change_tier").GetString());
        Assert.Equal("partial", root.GetProperty("execution_scope").GetString());
        Assert.False(root.GetProperty("ok").GetBoolean());
        Assert.Equal(0, root.GetProperty("duration_ms").GetInt64());
        Assert.Equal(0, root.GetProperty("results").GetArrayLength());
        Assert.DoesNotContain("exception", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("detail", json, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("PCV_VERIFY_CONFIG_INVALID|secret-detail")]
    [InlineData("Authorization: Bearer actual-secret")]
    [InlineData("PCV_VERIFY_NOT_STABLE")]
    public void FailureRejectsAnyNonStableErrorCodeOrSensitiveDetail(string errorCode)
    {
        var request = VerificationPlanFixture.Full(planOnly: true).Request;

        AssertConfigInvalid(() => VerificationSummaryFactory.CreateFailure(
            request,
            null,
            "plan-only-foundation",
            errorCode,
            StartedAt,
            StartedAt));
    }

    [Theory]
    [InlineData(VerificationErrorCodes.ConfigInvalid)]
    [InlineData(VerificationErrorCodes.UnknownSuite)]
    [InlineData(VerificationErrorCodes.ProcessFailed)]
    [InlineData(VerificationErrorCodes.Timeout)]
    [InlineData(VerificationErrorCodes.Cancelled)]
    [InlineData(VerificationErrorCodes.ParityUnmapped)]
    [InlineData(VerificationErrorCodes.NonAdminPowerShellForbidden)]
    [InlineData(VerificationErrorCodes.ArtifactRootInvalid)]
    public void FailureAcceptsEachExactStableErrorCode(string errorCode)
    {
        var request = VerificationPlanFixture.Full(planOnly: true).Request;

        var summary = VerificationSummaryFactory.CreateFailure(
            request,
            null,
            "plan-only-foundation",
            errorCode,
            StartedAt,
            StartedAt);

        Assert.Equal(errorCode, summary.ErrorCode);
    }

    [Theory]
    [InlineData("lane")]
    [InlineData("tier")]
    [InlineData("changed-paths")]
    [InlineData("artifact-root")]
    [InlineData("suite-ids")]
    [InlineData("shard")]
    [InlineData("plan-only")]
    public void FailureRejectsRequestThatDoesNotDeepMatchSuppliedPlan(string scenario)
    {
        var plan = VerificationPlanFixture.Full(planOnly: true);
        var request = scenario switch
        {
            "lane" => plan.Request with { RequestedLane = VerificationLane.Release },
            "tier" => plan.Request with { RequestedChangeTier = ChangeTier.L },
            "changed-paths" => plan.Request with
            {
                ChangedPaths = Array.AsReadOnly(plan.Request.ChangedPaths.Append("other-path").ToArray())
            },
            "artifact-root" => plan.Request with { ArtifactRoot = "artifacts/other" },
            "suite-ids" => plan.Request with
            {
                SuiteIds = Array.AsReadOnly(new[] { plan.Suites[0].Id })
            },
            "shard" => plan.Request with { ShardId = "dotnet" },
            "plan-only" => plan.Request with { PlanOnly = false },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        AssertConfigInvalid(() => VerificationSummaryFactory.CreateFailure(
            request,
            plan,
            "plan-only-foundation",
            VerificationErrorCodes.ConfigInvalid,
            StartedAt,
            StartedAt));
    }

    [Theory]
    [InlineData("empty-suites")]
    [InlineData("duplicate-suite")]
    [InlineData("scope")]
    [InlineData("release-preflight")]
    [InlineData("suite-definition")]
    public void FailureRejectsInvalidSuppliedPlanBinding(string scenario)
    {
        var plan = VerificationPlanFixture.Full(planOnly: true);
        plan = scenario switch
        {
            "empty-suites" => plan with { Suites = Array.Empty<SuiteDefinition>() },
            "duplicate-suite" => plan with
            {
                Suites = Array.AsReadOnly(plan.Suites.Append(plan.Suites[0]).ToArray())
            },
            "scope" => plan with { ExecutionScope = ExecutionScope.Partial },
            "release-preflight" => plan with { ReleasePreflight = true },
            "suite-definition" => plan with
            {
                Suites = Array.AsReadOnly(plan.Suites.Select((suite, index) =>
                    index == 0 ? suite with { ExecutorKind = "unknown" } : suite).ToArray())
            },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        AssertConfigInvalid(() => VerificationSummaryFactory.CreateFailure(
            plan.Request,
            plan,
            "plan-only-foundation",
            VerificationErrorCodes.ConfigInvalid,
            StartedAt,
            StartedAt));
    }

    [Fact]
    public void FailureRejectsFakeUnknownPromotionForKnownFastPath()
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var baseline = VerificationPlanFixture.ForCatalog(catalog, planOnly: true).Request;
        var request = baseline with
        {
            RequestedLane = VerificationLane.Fast,
            RequestedChangeTier = ChangeTier.S,
            ChangedPaths = Array.AsReadOnly(new[] { "src/DesktopNode.Runtime/RuntimeThing.cs" })
        };
        var plan = VerificationPlanner.Create(request, catalog);
        var tampered = plan with
        {
            EffectiveLane = VerificationLane.Full,
            PromotionReason = "unknown-change-scope"
        };

        AssertConfigInvalid(() => VerificationSummaryFactory.CreateFailure(
            request,
            tampered,
            "plan-only-foundation",
            VerificationErrorCodes.ConfigInvalid,
            StartedAt,
            StartedAt));
    }

    [Theory]
    [InlineData("unknown/path.txt")]
    [InlineData(null)]
    public void FailureRejectsRemovedUnknownPromotionForUnknownOrEmptyFastPath(string? changedPath)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var baseline = VerificationPlanFixture.ForCatalog(catalog, planOnly: true).Request;
        var request = baseline with
        {
            RequestedLane = VerificationLane.Fast,
            RequestedChangeTier = ChangeTier.S,
            ChangedPaths = changedPath is null
                ? Array.Empty<string>()
                : Array.AsReadOnly(new[] { changedPath })
        };
        var plan = VerificationPlanner.Create(request, catalog);
        var tampered = plan with
        {
            EffectiveLane = VerificationLane.Fast,
            PromotionReason = null
        };

        AssertConfigInvalid(() => VerificationSummaryFactory.CreateFailure(
            request,
            tampered,
            "plan-only-foundation",
            VerificationErrorCodes.ConfigInvalid,
            StartedAt,
            StartedAt));
    }

    [Theory]
    [InlineData("known")]
    [InlineData("unknown")]
    [InlineData("empty")]
    public void FailureAcceptsPlannerDerivedFastScopePromotion(string scenario)
    {
        var catalog = VerificationCatalogFixture.LoadCanonical();
        var baseline = VerificationPlanFixture.ForCatalog(catalog, planOnly: true).Request;
        var paths = scenario switch
        {
            "known" => new[] { "src/DesktopNode.Runtime/RuntimeThing.cs" },
            "unknown" => new[] { "unknown/path.txt" },
            "empty" => Array.Empty<string>(),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var request = baseline with
        {
            RequestedLane = VerificationLane.Fast,
            RequestedChangeTier = ChangeTier.S,
            ChangedPaths = Array.AsReadOnly(paths)
        };
        var plan = VerificationPlanner.Create(request, catalog);

        var summary = VerificationSummaryFactory.CreateFailure(
            request,
            plan,
            "plan-only-foundation",
            VerificationErrorCodes.ConfigInvalid,
            StartedAt,
            StartedAt);

        Assert.Equal(plan.PromotionReason, summary.PromotionReason);
        Assert.Equal(plan.EffectiveLane.ToString(), summary.EffectiveLane);
    }

    [Fact]
    public async Task WriterUsesSameDirectoryTemporaryFileThenAtomicMove()
    {
        var fileSystem = new RecordingVerificationFileSystem();
        var writer = new AtomicVerificationSummaryWriter(fileSystem);
        var root = Path.Combine("D:\\repo", "artifacts", "wave-a");

        var path = await writer.WriteAsync(
            root, VerificationSummaryFixture.Success(), CancellationToken.None);

        Assert.Equal(Path.Combine(root, "summary.json"), path);
        Assert.Equal(["create-directory", "write-temp", "move-overwrite"], fileSystem.Operations);
        Assert.Equal(root, Path.GetDirectoryName(fileSystem.TempPath), ignoreCase: true);
        Assert.StartsWith(
            Path.Combine(root, "summary.json."),
            fileSystem.TempPath,
            StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(".tmp", fileSystem.TempPath, StringComparison.OrdinalIgnoreCase);
        Assert.False(fileSystem.FileExists(fileSystem.TempPath));
        Assert.True(fileSystem.FileExists(path));
        Assert.DoesNotContain(Path.GetFileName(fileSystem.TempPath), fileSystem.ReadAllText(path), StringComparison.Ordinal);
    }

    [Fact]
    public async Task WriterDeletesTemporaryFileWhenMoveFails()
    {
        var fileSystem = new RecordingVerificationFileSystem(failMove: true);
        var writer = new AtomicVerificationSummaryWriter(fileSystem);

        await Assert.ThrowsAsync<IOException>(() => writer.WriteAsync(
            Path.Combine("D:\\repo", "artifacts", "wave-a"),
            VerificationSummaryFixture.Success(),
            CancellationToken.None));

        Assert.Equal(
            ["create-directory", "write-temp", "move-overwrite", "delete-temp"],
            fileSystem.Operations);
        Assert.False(fileSystem.FileExists(fileSystem.TempPath));
    }

    [Theory]
    [InlineData("write")]
    [InlineData("move")]
    public async Task WriterPreservesPrimaryIoFailureWhenDeleteAlsoFails(string stage)
    {
        var primary = new IOException($"primary-{stage}");
        var cleanup = new UnauthorizedAccessException("cleanup-sensitive-path");
        var fileSystem = new RecordingVerificationFileSystem(
            writeException: stage == "write" ? primary : null,
            moveException: stage == "move" ? primary : null,
            deleteException: cleanup);
        var writer = new AtomicVerificationSummaryWriter(fileSystem);

        var caught = await Assert.ThrowsAsync<IOException>(() => writer.WriteAsync(
            Path.Combine("D:\\repo", "artifacts", "wave-a"),
            VerificationSummaryFixture.Success(),
            CancellationToken.None));

        Assert.Same(primary, caught);
        Assert.Contains(caught.Data.Values.Cast<object?>(), value =>
            string.Equals(value as string, cleanup.GetType().FullName, StringComparison.Ordinal));
        Assert.DoesNotContain(caught.Data.Values.Cast<object?>(), value =>
            string.Equals(value as string, cleanup.Message, StringComparison.Ordinal));
    }

    [Fact]
    public async Task WriterPreservesCancellationReferenceAndTokenWhenDeleteAlsoFails()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var primary = new OperationCanceledException("primary-cancel", null, source.Token);
        var cleanup = new IOException("cleanup-sensitive-path");
        var fileSystem = new RecordingVerificationFileSystem(
            writeException: primary,
            deleteException: cleanup);
        var writer = new AtomicVerificationSummaryWriter(fileSystem);

        var caught = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WriteAsync(
            Path.Combine("D:\\repo", "artifacts", "wave-a"),
            VerificationSummaryFixture.Success(),
            source.Token));

        Assert.Same(primary, caught);
        Assert.Equal(source.Token, caught.CancellationToken);
        Assert.Contains(caught.Data.Values.Cast<object?>(), value =>
            string.Equals(value as string, cleanup.GetType().FullName, StringComparison.Ordinal));
    }

    [Fact]
    public async Task WriterThrowsCleanupFailureWhenThereIsNoPrimaryFailure()
    {
        var cleanup = new IOException("cleanup-failure");
        var fileSystem = new RecordingVerificationFileSystem(fileExistsException: cleanup);
        var writer = new AtomicVerificationSummaryWriter(fileSystem);

        var caught = await Assert.ThrowsAsync<IOException>(() => writer.WriteAsync(
            Path.Combine("D:\\repo", "artifacts", "wave-a"),
            VerificationSummaryFixture.Success(),
            CancellationToken.None));

        Assert.Same(cleanup, caught);
    }

    [Fact]
    public async Task PhysicalWriterOverwritesWithExactBomlessUtf8AndLeavesNoTemporaryFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"pcv-summary-writer-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var destination = Path.Combine(root, "summary.json");
            await File.WriteAllTextAsync(destination, "pre-existing");
            var summary = VerificationSummaryFixture.Success();
            var expected = Encoding.UTF8.GetBytes(VerificationJson.Serialize(summary));

            var path = await new AtomicVerificationSummaryWriter(
                new PhysicalVerificationFileSystem()).WriteAsync(root, summary, CancellationToken.None);
            var actual = await File.ReadAllBytesAsync(path);

            Assert.Equal(expected, actual);
            Assert.False(actual.AsSpan().StartsWith(Encoding.UTF8.Preamble));
            Assert.Empty(Directory.EnumerateFiles(root, "summary.json.*.tmp"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static SuiteExecutionRecord Planned(SuiteDefinition suite) =>
        new(suite.Id, SuiteStatus.Planned, suite.MigrationState, null, 0, false, false,
            null, null, null, null);

    private static SuiteExecutionRecord Passed(SuiteDefinition suite) =>
        suite.ExecutorKind == "managed"
            ? new SuiteExecutionRecord(
                suite.Id, SuiteStatus.Passed, suite.MigrationState, null, 7, false, false,
                null, null, null, null)
            : WithOutput(
                new SuiteExecutionRecord(
                    suite.Id, SuiteStatus.Passed, suite.MigrationState, 0, 7, false, false,
                    null, null, null, null),
                "safe output",
                string.Empty);

    private static SuiteExecutionRecord Terminal(
        SuiteDefinition suite,
        SuiteStatus status,
        string errorCode) =>
        new(
            suite.Id,
            status,
            suite.MigrationState,
            null,
            11,
            status == SuiteStatus.TimedOut,
            status == SuiteStatus.Cancelled,
            null,
            null,
            null,
            errorCode);

    private static SuiteExecutionRecord WithOutput(
        SuiteExecutionRecord row,
        string standardOutput,
        string standardError) =>
        row with
        {
            StandardOutput = standardOutput,
            StandardError = standardError,
            OutputSha256 = Hash(standardOutput, standardError)
        };

    private static VerificationCatalog MutateCatalog(
        VerificationCatalog canonical,
        string scenario)
    {
        var suites = canonical.Suites.ToArray();
        var shards = canonical.Shards.Select(shard => shard with
        {
            SuiteIds = Array.AsReadOnly(shard.SuiteIds.ToArray())
        }).ToArray();
        var processIndex = Array.FindIndex(suites, suite => suite.ExecutorKind == "process");
        var managedIndex = Array.FindIndex(suites, suite => suite.ExecutorKind == "managed");

        switch (scenario)
        {
            case "schema":
                return canonical with { SchemaVersion = 2 };
            case "contract":
                return canonical with { Contract = "wrong-contract" };
            case "activation":
                return canonical with { ActivationState = "actual-enabled" };
            case "parallelism":
                return canonical with { MaxParallelism = 3 };
            case "overall-timeout":
                return canonical with { OverallTimeoutSeconds = 0 };
            case "allowlist":
                return canonical with
                {
                    AllowedExecutables = Array.AsReadOnly(canonical.AllowedExecutables
                        .Select((value, index) => index == 0 ? "evil.exe" : value).ToArray())
                };
            case "suite-truncated":
            {
                var removed = suites[^1].Id;
                return canonical with
                {
                    Suites = Array.AsReadOnly(suites[..^1]),
                    Shards = Array.AsReadOnly(shards.Select(shard => shard with
                    {
                        SuiteIds = Array.AsReadOnly(shard.SuiteIds
                            .Where(suiteId => suiteId != removed).ToArray())
                    }).ToArray())
                };
            }
            case "suite-extra":
                return canonical with
                {
                    Suites = Array.AsReadOnly(suites.Append(suites[^1] with { Id = "extra-suite" }).ToArray())
                };
            case "suite-reordered":
                return canonical with { Suites = Array.AsReadOnly(suites.Reverse().ToArray()) };
            case "owner":
                suites[0] = suites[0] with { Owner = "unknown-owner" };
                break;
            case "migration":
                suites[0] = suites[0] with { MigrationState = "unknown-state" };
                break;
            case "suite-timeout":
                suites[0] = suites[0] with { TimeoutSeconds = canonical.OverallTimeoutSeconds + 1 };
                break;
            case "managed-handler":
                suites[managedIndex] = suites[managedIndex] with { ManagedHandler = "unknown-handler" };
                break;
            case "executor-union":
                suites[managedIndex] = suites[managedIndex] with
                {
                    Arguments = Array.AsReadOnly(new[] { "unexpected" })
                };
                break;
            case "process-path":
                suites[processIndex] = suites[processIndex] with { FileName = "tools/dotnet" };
                break;
            case "process-not-normalized":
                suites[processIndex] = suites[processIndex] with
                {
                    FileName = suites[processIndex].FileName!.ToUpperInvariant()
                };
                break;
            case "powershell":
                suites[processIndex] = suites[processIndex] with
                {
                    Arguments = Array.AsReadOnly(suites[processIndex].Arguments.Append("Invoke-Pester").ToArray())
                };
                break;
            case "host-mutation":
                suites[processIndex] = suites[processIndex] with
                {
                    Arguments = Array.AsReadOnly(suites[processIndex].Arguments.Append("Start-VM").ToArray())
                };
                break;
            case "shard-order":
                return canonical with { Shards = Array.AsReadOnly(shards.Reverse().ToArray()) };
            case "shard-overlap":
                shards[1] = shards[1] with
                {
                    SuiteIds = Array.AsReadOnly(shards[1].SuiteIds.Append(shards[0].SuiteIds[0]).ToArray())
                };
                return canonical with { Shards = Array.AsReadOnly(shards) };
            case "shard-incomplete":
            {
                var shardIndex = Array.FindIndex(shards, shard => shard.SuiteIds.Count > 1);
                shards[shardIndex] = shards[shardIndex] with
                {
                    SuiteIds = Array.AsReadOnly(shards[shardIndex].SuiteIds.Skip(1).ToArray())
                };
                return canonical with { Shards = Array.AsReadOnly(shards) };
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        return canonical with { Suites = Array.AsReadOnly(suites) };
    }

    private static string Hash(string standardOutput, string standardError) =>
        Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(standardOutput + "\n" + standardError))).ToLowerInvariant();

    private static void AssertConfigInvalid(Action action)
    {
        var exception = Assert.Throws<VerificationException>(action);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
    }
}
