namespace DesktopNode.Verification.Tests;

public sealed class VerificationExecutorTests
{
    private static readonly string RepositoryRoot = VerificationCatalogFixture.RepositoryRoot;
    private static readonly string ValidHash = new('0', 64);

    [Fact]
    public async Task PlanOnlyReturnsOrderedPlannedRowsWithoutCallingAnyRunner()
    {
        var catalog = VerificationCatalogFixture.SevenProcessSuites();
        var process = new RecordingProcessRunner();
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);

        var report = await new VerificationExecutor(process, managed).ExecuteAsync(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: true),
            catalog with { MaxParallelism = 0, OverallTimeoutSeconds = 0 },
            RepositoryRoot,
            CancellationToken.None);

        Assert.Equal(0, report.DurationMs);
        Assert.Equal(catalog.Suites.Select(suite => suite.Id), report.Results.Select(row => row.SuiteId));
        Assert.All(report.Results, row =>
        {
            Assert.Equal(SuiteStatus.Planned, row.Status);
            Assert.Equal(0, row.DurationMs);
            Assert.False(row.TimedOut);
            Assert.False(row.Cancelled);
            Assert.Null(row.ExitCode);
            Assert.Null(row.StandardOutput);
            Assert.Null(row.StandardError);
            Assert.Null(row.OutputSha256);
            Assert.Null(row.ErrorCode);
        });
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task RunsAtMostFourSuitesAndRestoresCatalogOrder()
    {
        var catalog = VerificationCatalogFixture.SevenProcessSuites();
        var process = new RecordingProcessRunner(delay: TimeSpan.FromMilliseconds(40));

        var report = await ExecuteAsync(catalog, process);

        Assert.Equal(4, process.MaximumConcurrency);
        Assert.Equal(catalog.Suites.Select(suite => suite.Id), report.Results.Select(row => row.SuiteId));
        Assert.All(report.Results, row => Assert.Equal(SuiteStatus.Passed, row.Status));
    }

    [Theory]
    [InlineData(7, false, false, "Failed", VerificationErrorCodes.ProcessFailed)]
    [InlineData(null, true, false, "TimedOut", VerificationErrorCodes.Timeout)]
    [InlineData(null, false, true, "Cancelled", VerificationErrorCodes.Cancelled)]
    public async Task MapsEveryProcessTerminalResultWithoutPassCompression(
        int? exitCode,
        bool timedOut,
        bool cancelled,
        string expectedStatus,
        string expectedCode)
    {
        var catalog = VerificationCatalogFixture.OneProcessSuite();
        var process = new RecordingProcessRunner(
            new ProcessExecutionResult(exitCode, 19, timedOut, cancelled, "out", "err", ValidHash));

        var report = await ExecuteAsync(catalog, process);

        var row = Assert.Single(report.Results);
        Assert.Equal(Enum.Parse<SuiteStatus>(expectedStatus), row.Status);
        Assert.Equal(expectedCode, row.ErrorCode);
        Assert.Equal(19, row.DurationMs);
        Assert.Equal("out", row.StandardOutput);
        Assert.Equal("err", row.StandardError);
        Assert.Equal(ValidHash, row.OutputSha256);
    }

    [Fact]
    public async Task PreservesManagedMissingParityUnmappedAndNormalizesIdentity()
    {
        var suite = ManagedSuite();
        var catalog = Catalog([suite]);
        var managed = new RecordingManagedSuiteRunner(new SuiteExecutionRecord(
            "renamed", SuiteStatus.Missing, "wrong", null, 0, false, false,
            null, null, null, VerificationErrorCodes.ParityUnmapped));

        var report = await new VerificationExecutor(new RecordingProcessRunner(), managed).ExecuteAsync(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: false), catalog, RepositoryRoot, CancellationToken.None);

        var row = Assert.Single(report.Results);
        Assert.Equal(suite.Id, row.SuiteId);
        Assert.Equal(suite.MigrationState, row.MigrationState);
        Assert.Equal(SuiteStatus.Missing, row.Status);
        Assert.Equal(VerificationErrorCodes.ParityUnmapped, row.ErrorCode);
    }

    [Fact]
    public async Task UnavailableManagedRunnerReturnsMissingParityUnmapped()
    {
        var catalog = Catalog([ManagedSuite()]);

        var report = await new VerificationExecutor(
            new RecordingProcessRunner(), new UnavailableManagedSuiteRunner()).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(catalog, planOnly: false), catalog, RepositoryRoot, CancellationToken.None);

        var row = Assert.Single(report.Results);
        Assert.Equal(SuiteStatus.Missing, row.Status);
        Assert.Equal(VerificationErrorCodes.ParityUnmapped, row.ErrorCode);
    }

    [Fact]
    public async Task AlreadyCancelledCallerProducesOneCancelledRowPerSuite()
    {
        var catalog = VerificationCatalogFixture.SevenProcessSuites();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var report = await ExecuteAsync(catalog, new RecordingProcessRunner(delay: TimeSpan.FromSeconds(5)), cancellation.Token);

        Assert.Equal(7, report.Results.Count);
        Assert.All(report.Results, AssertCancelled);
    }

    [Fact]
    public async Task AbandonedRunnerRetainsPermitUntilUnderlyingTaskCompletes()
    {
        var original = VerificationCatalogFixture.SevenProcessSuites();
        var catalog = original with
        {
            MaxParallelism = 1,
            OverallTimeoutSeconds = 2,
            Suites = Array.AsReadOnly(original.Suites.Take(2)
                .Select(suite => suite with { TimeoutSeconds = 1 })
                .ToArray())
        };
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var process = new RecordingProcessRunner(asyncGate: gate.Task, ignoreCancellation: true);

        try
        {
            var report = await ExecuteAsync(catalog, process).WaitAsync(TimeSpan.FromSeconds(4));

            Assert.Equal(2, report.Results.Count);
            Assert.All(report.Results, row =>
            {
                Assert.Equal(SuiteStatus.TimedOut, row.Status);
                Assert.True(row.TimedOut);
                Assert.False(row.Cancelled);
                Assert.Equal(VerificationErrorCodes.Timeout, row.ErrorCode);
            });
            Assert.True(report.Results[0].DurationMs > 0);
            Assert.Equal(0, report.Results[1].DurationMs);
            Assert.Equal(1, process.CallCount);
            Assert.Equal(1, process.MaximumConcurrency);
        }
        finally
        {
            gate.TrySetResult();
        }
    }

    [Fact]
    public async Task NonCooperativeManagedRunnerCannotBlockPerSuiteDeadline()
    {
        var suite = ManagedSuite() with { TimeoutSeconds = 1 };
        var catalog = Catalog([suite]);
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var managed = new RecordingManagedSuiteRunner(asyncGate: gate.Task, ignoreCancellation: true);

        try
        {
            var report = await new VerificationExecutor(new RecordingProcessRunner(), managed).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
                catalog,
                RepositoryRoot,
                CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3));

            var row = Assert.Single(report.Results);
            Assert.Equal(SuiteStatus.TimedOut, row.Status);
            Assert.True(row.TimedOut);
            Assert.True(row.DurationMs > 0);
        }
        finally
        {
            gate.TrySetResult();
        }
    }

    [Fact]
    public async Task CompletedProcessResultWinsImmediatelyFollowingCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource<ProcessExecutionResult>();
        var process = new RecordingProcessRunner(handler: (_, _) =>
        {
            completion.SetResult(ProcessResult(0));
            cancellation.Cancel();
            return completion.Task;
        });

        var row = Assert.Single((await ExecuteAsync(
            VerificationCatalogFixture.OneProcessSuite(), process, cancellation.Token)).Results);

        Assert.Equal(SuiteStatus.Passed, row.Status);
        Assert.Equal(0, row.ExitCode);
    }

    [Fact]
    public async Task CompletedMalformedProcessResultFailsDespiteFollowingCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource<ProcessExecutionResult>();
        var process = new RecordingProcessRunner(handler: (_, _) =>
        {
            completion.SetResult(ProcessResult(0) with { OutputSha256 = "invalid" });
            cancellation.Cancel();
            return completion.Task;
        });

        var row = Assert.Single((await ExecuteAsync(
            VerificationCatalogFixture.OneProcessSuite(), process, cancellation.Token)).Results);

        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, row.ErrorCode);
    }

    [Fact]
    public async Task CompletedManagedResultWinsImmediatelyFollowingCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var suite = ManagedSuite();
        var catalog = Catalog([suite]);
        var completion = new TaskCompletionSource<SuiteExecutionRecord>();
        var managed = new RecordingManagedSuiteRunner(handler: (_, _) =>
        {
            completion.SetResult(ManagedResult(SuiteStatus.Passed));
            cancellation.Cancel();
            return completion.Task;
        });

        var report = await new VerificationExecutor(new RecordingProcessRunner(), managed).ExecuteAsync(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
            catalog,
            RepositoryRoot,
            cancellation.Token);

        Assert.Equal(SuiteStatus.Passed, Assert.Single(report.Results).Status);
    }

    [Theory]
    [InlineData("process")]
    [InlineData("managed")]
    public async Task SourceLessOperationCancelledExceptionFailsClosed(string executorKind)
    {
        VerificationExecutionReport report;
        if (executorKind == "process")
        {
            var process = new RecordingProcessRunner(handler: (_, _) =>
                Task.FromException<ProcessExecutionResult>(new OperationCanceledException("unrelated")));
            report = await ExecuteAsync(VerificationCatalogFixture.OneProcessSuite(), process);
        }
        else
        {
            var catalog = Catalog([ManagedSuite()]);
            var managed = new RecordingManagedSuiteRunner(handler: (_, _) =>
                Task.FromException<SuiteExecutionRecord>(new OperationCanceledException("unrelated")));
            report = await new VerificationExecutor(new RecordingProcessRunner(), managed).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
                catalog,
                RepositoryRoot,
                CancellationToken.None);
        }

        var row = Assert.Single(report.Results);
        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, row.ErrorCode);
        Assert.False(row.TimedOut);
        Assert.False(row.Cancelled);
    }

    [Theory]
    [InlineData("process")]
    [InlineData("managed")]
    public async Task SourceLessOperationCancelledExceptionWinsOverFollowingCallerCancellation(string executorKind)
    {
        using var callerCancellation = new CancellationTokenSource();
        VerificationExecutionReport report;
        if (executorKind == "process")
        {
            var process = new RecordingProcessRunner(handler: (_, _) =>
            {
                var completion = new TaskCompletionSource<ProcessExecutionResult>();
                completion.SetException(new OperationCanceledException("runner-originated", CancellationToken.None));
                callerCancellation.Cancel();
                return completion.Task;
            });
            report = await ExecuteAsync(
                VerificationCatalogFixture.OneProcessSuite(), process, callerCancellation.Token);
        }
        else
        {
            var catalog = Catalog([ManagedSuite()]);
            var managed = new RecordingManagedSuiteRunner(handler: (_, _) =>
            {
                var completion = new TaskCompletionSource<SuiteExecutionRecord>();
                completion.SetException(new OperationCanceledException("runner-originated", CancellationToken.None));
                callerCancellation.Cancel();
                return completion.Task;
            });
            report = await new VerificationExecutor(new RecordingProcessRunner(), managed).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
                catalog,
                RepositoryRoot,
                callerCancellation.Token);
        }

        var row = Assert.Single(report.Results);
        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, row.ErrorCode);
        Assert.False(row.TimedOut);
        Assert.False(row.Cancelled);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunnerExceptionsBecomeApprovedFailuresWithoutCancellingSiblings(bool knownVerificationException)
    {
        var catalog = VerificationCatalogFixture.SevenProcessSuites();
        var process = new RecordingProcessRunner(handler: (invocation, _) =>
        {
            if (invocation.SuiteId == "suite-3")
            {
                return Task.FromException<ProcessExecutionResult>(knownVerificationException
                    ? new VerificationException(VerificationErrorCodes.ConfigInvalid, "secret-detail")
                    : new InvalidOperationException("secret-message"));
            }

            return Task.FromResult(new ProcessExecutionResult(0, 1, false, false, "", "", ValidHash));
        });

        var report = await ExecuteAsync(catalog, process);

        Assert.Equal(7, process.CallCount);
        Assert.Equal(7, report.Results.Count);
        var failure = Assert.Single(report.Results, row => row.Status == SuiteStatus.Failed);
        Assert.Equal(knownVerificationException ? VerificationErrorCodes.ConfigInvalid : VerificationErrorCodes.ProcessFailed, failure.ErrorCode);
        Assert.Null(failure.StandardOutput);
        Assert.Null(failure.StandardError);
        Assert.Null(failure.OutputSha256);
        Assert.Equal(6, report.Results.Count(row => row.Status == SuiteStatus.Passed));
    }

    [Theory]
    [InlineData("process", VerificationErrorCodes.Timeout)]
    [InlineData("process", VerificationErrorCodes.Cancelled)]
    [InlineData("process", VerificationErrorCodes.ParityUnmapped)]
    [InlineData("managed", VerificationErrorCodes.Timeout)]
    [InlineData("managed", VerificationErrorCodes.Cancelled)]
    [InlineData("managed", VerificationErrorCodes.ParityUnmapped)]
    public async Task TerminalOnlyVerificationExceptionCodesFailClosed(
        string executorKind,
        string terminalCode)
    {
        VerificationExecutionReport report;
        if (executorKind == "process")
        {
            var process = new RecordingProcessRunner(handler: (_, _) =>
                Task.FromException<ProcessExecutionResult>(
                    new VerificationException(terminalCode, "must-not-leak")));
            report = await ExecuteAsync(VerificationCatalogFixture.OneProcessSuite(), process);
        }
        else
        {
            var catalog = Catalog([ManagedSuite()]);
            var managed = new RecordingManagedSuiteRunner(handler: (_, _) =>
                Task.FromException<SuiteExecutionRecord>(
                    new VerificationException(terminalCode, "must-not-leak")));
            report = await new VerificationExecutor(new RecordingProcessRunner(), managed).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
                catalog,
                RepositoryRoot,
                CancellationToken.None);
        }

        var row = Assert.Single(report.Results);
        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, row.ErrorCode);
        Assert.False(row.TimedOut);
        Assert.False(row.Cancelled);
        Assert.Null(row.StandardOutput);
        Assert.Null(row.StandardError);
        Assert.Null(row.OutputSha256);
    }

    [Fact]
    public async Task CapsFixtureParallelismAboveFour()
    {
        var catalog = VerificationCatalogFixture.SevenProcessSuites(maxParallelism: 99);
        var process = new RecordingProcessRunner(delay: TimeSpan.FromMilliseconds(40));

        await ExecuteAsync(catalog, process);

        Assert.Equal(4, process.MaximumConcurrency);
    }

    [Fact]
    public async Task PreservesValidProcessOutputHashAndExclusiveTimeoutFlag()
    {
        var catalog = VerificationCatalogFixture.OneProcessSuite();
        var process = new RecordingProcessRunner(
            new ProcessExecutionResult(null, 23, true, false, "safe-out", "safe-err", ValidHash));

        var row = Assert.Single((await ExecuteAsync(catalog, process)).Results);

        Assert.Equal(SuiteStatus.TimedOut, row.Status);
        Assert.True(row.TimedOut);
        Assert.False(row.Cancelled);
        Assert.Equal("safe-out", row.StandardOutput);
        Assert.Equal("safe-err", row.StandardError);
        Assert.Equal(ValidHash, row.OutputSha256);
    }

    [Theory]
    [InlineData("negative-duration")]
    [InlineData("dual-flags")]
    [InlineData("timeout-exit")]
    [InlineData("missing-exit")]
    [InlineData("null-output")]
    [InlineData("null-error")]
    [InlineData("null-hash")]
    [InlineData("bad-hash")]
    [InlineData("uppercase-hash")]
    public async Task MalformedProcessResultsFailClosed(string scenario)
    {
        var malformed = scenario switch
        {
            "negative-duration" => ProcessResult(0, duration: -1),
            "dual-flags" => ProcessResult(null, timedOut: true, cancelled: true),
            "timeout-exit" => ProcessResult(7, timedOut: true),
            "missing-exit" => ProcessResult(null),
            "null-output" => ProcessResult(0) with { StandardOutput = null! },
            "null-error" => ProcessResult(0) with { StandardError = null! },
            "null-hash" => ProcessResult(0) with { OutputSha256 = null! },
            "bad-hash" => ProcessResult(0) with { OutputSha256 = "ABC" },
            "uppercase-hash" => ProcessResult(0) with { OutputSha256 = new string('A', 64) },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        var row = Assert.Single((await ExecuteAsync(
            VerificationCatalogFixture.OneProcessSuite(),
            new RecordingProcessRunner(malformed))).Results);

        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, row.ErrorCode);
        Assert.False(row.TimedOut);
        Assert.False(row.Cancelled);
        Assert.Null(row.StandardOutput);
        Assert.Null(row.StandardError);
        Assert.Null(row.OutputSha256);
    }

    [Theory]
    [InlineData("passed-with-code")]
    [InlineData("timedout-without-code")]
    [InlineData("missing-wrong-code")]
    [InlineData("failed-without-code")]
    [InlineData("cancelled-with-output")]
    [InlineData("planned")]
    [InlineData("failed-timeout")]
    [InlineData("failed-cancelled")]
    [InlineData("failed-parity")]
    public async Task MalformedManagedStatusMatrixFailsClosed(string scenario)
    {
        var suite = ManagedSuite();
        var malformed = scenario switch
        {
            "passed-with-code" => ManagedResult(SuiteStatus.Passed) with { ErrorCode = VerificationErrorCodes.Timeout },
            "timedout-without-code" => ManagedResult(SuiteStatus.TimedOut) with { TimedOut = true },
            "missing-wrong-code" => ManagedResult(SuiteStatus.Missing) with { ErrorCode = VerificationErrorCodes.ProcessFailed },
            "failed-without-code" => ManagedResult(SuiteStatus.Failed),
            "cancelled-with-output" => ManagedResult(SuiteStatus.Cancelled) with
            {
                Cancelled = true,
                ErrorCode = VerificationErrorCodes.Cancelled,
                StandardOutput = "unexpected"
            },
            "planned" => ManagedResult(SuiteStatus.Planned),
            "failed-timeout" => ManagedResult(SuiteStatus.Failed) with { ErrorCode = VerificationErrorCodes.Timeout },
            "failed-cancelled" => ManagedResult(SuiteStatus.Failed) with { ErrorCode = VerificationErrorCodes.Cancelled },
            "failed-parity" => ManagedResult(SuiteStatus.Failed) with { ErrorCode = VerificationErrorCodes.ParityUnmapped },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var catalog = Catalog([suite]);

        var report = await new VerificationExecutor(
            new RecordingProcessRunner(), new RecordingManagedSuiteRunner(malformed)).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
                catalog,
                RepositoryRoot,
                CancellationToken.None);

        var row = Assert.Single(report.Results);
        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ProcessFailed, row.ErrorCode);
    }

    [Fact]
    public async Task RestoresSelectedOrderAfterOutOfOrderCompletion()
    {
        var catalog = VerificationCatalogFixture.SevenProcessSuites();
        var process = new RecordingProcessRunner(handler: async (invocation, token) =>
        {
            var index = int.Parse(invocation.SuiteId.AsSpan("suite-".Length));
            await Task.Delay(TimeSpan.FromMilliseconds((8 - index) * 10), token);
            return new ProcessExecutionResult(0, index, false, false, "", "", ValidHash);
        });

        var report = await ExecuteAsync(catalog, process);

        Assert.Equal(Enumerable.Range(1, 7).Select(index => $"suite-{index}"), report.Results.Select(row => row.SuiteId));
        Assert.Equal(Enumerable.Range(1, 7).Select(index => (long)index), report.Results.Select(row => row.DurationMs));
        Assert.False(catalog.Suites.Select(suite => suite.Id).SequenceEqual(process.CompletionIds));
    }

    [Theory]
    [InlineData("arguments")]
    [InlineData("kind")]
    [InlineData("timeout")]
    [InlineData("managed-handler")]
    public async Task RejectsForgedPlanDefinitionsBeforeCallingRunner(string scenario)
    {
        var catalog = scenario == "managed-handler"
            ? Catalog([ManagedSuite()])
            : VerificationCatalogFixture.OneProcessSuite();
        var canonical = catalog.Suites[0];
        var forged = scenario switch
        {
            "arguments" => canonical with { Arguments = Array.AsReadOnly(new[] { "--info" }) },
            "kind" => canonical with { ExecutorKind = "managed" },
            "timeout" => canonical with { TimeoutSeconds = canonical.TimeoutSeconds + 1 },
            "managed-handler" => canonical with { ManagedHandler = "policy-boundaries" },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: false) with
        {
            Suites = Array.AsReadOnly(new[] { forged })
        };
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new VerificationExecutor(process, managed).ExecuteAsync(plan, catalog, RepositoryRoot, CancellationToken.None));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("duplicate")]
    public async Task RejectsMalformedCatalogSuiteIdentityBeforeCallingRunner(string scenario)
    {
        var goodCatalog = VerificationCatalogFixture.OneProcessSuite();
        var malformedSuites = scenario == "null"
            ? new SuiteDefinition[] { null! }
            : new[] { goodCatalog.Suites[0], goodCatalog.Suites[0] };
        var catalog = goodCatalog with { Suites = Array.AsReadOnly(malformedSuites) };
        var process = new RecordingProcessRunner(failIfCalled: true);

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new VerificationExecutor(process, new RecordingManagedSuiteRunner(failIfCalled: true)).ExecuteAsync(
                VerificationPlanFixture.ForCatalog(goodCatalog, planOnly: false),
                catalog,
                RepositoryRoot,
                CancellationToken.None));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(0, process.CallCount);
    }

    [Fact]
    public async Task FatalRunnerExceptionPropagates()
    {
        var process = new RecordingProcessRunner(handler: (_, _) =>
            Task.FromException<ProcessExecutionResult>(new OutOfMemoryException("fatal")));

        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            ExecuteAsync(VerificationCatalogFixture.OneProcessSuite(), process));
    }

    [Fact]
    public async Task ResultsCollectionIsImmutable()
    {
        var report = await ExecuteAsync(VerificationCatalogFixture.OneProcessSuite(), new RecordingProcessRunner());

        Assert.Throws<NotSupportedException>(() => ((IList<SuiteExecutionRecord>)report.Results).Clear());
    }

    [Fact]
    public async Task UnknownExecutorKindFailsWithStableConfigCode()
    {
        var catalog = Catalog([VerificationCatalogFixture.OneProcessSuite().Suites[0] with { ExecutorKind = "unknown" }]);

        var row = Assert.Single((await ExecuteAsync(catalog, new RecordingProcessRunner())).Results);

        Assert.Equal(SuiteStatus.Failed, row.Status);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, row.ErrorCode);
    }

    private static Task<VerificationExecutionReport> ExecuteAsync(
        VerificationCatalog catalog,
        RecordingProcessRunner process,
        CancellationToken cancellationToken = default) =>
        new VerificationExecutor(process, new RecordingManagedSuiteRunner()).ExecuteAsync(
            VerificationPlanFixture.ForCatalog(catalog, planOnly: false),
            catalog,
            RepositoryRoot,
            cancellationToken);

    private static void AssertCancelled(SuiteExecutionRecord row)
    {
        Assert.Equal(SuiteStatus.Cancelled, row.Status);
        Assert.False(row.TimedOut);
        Assert.True(row.Cancelled);
        Assert.Equal(VerificationErrorCodes.Cancelled, row.ErrorCode);
    }

    private static SuiteDefinition ManagedSuite() => new(
        "managed-suite", "csharp", "wave-managed", "managed", null,
        Array.Empty<string>(), "current-evidence-check", 10);

    private static VerificationCatalog Catalog(IReadOnlyList<SuiteDefinition> suites) => new(
        1,
        "test-contract",
        "plan-only-foundation",
        4,
        30,
        Array.AsReadOnly(VerificationCatalogFixture.AllowedExecutables.ToArray()),
        Array.AsReadOnly(suites.ToArray()),
        Array.Empty<ShardDefinition>());

    private static ProcessExecutionResult ProcessResult(
        int? exitCode,
        long duration = 1,
        bool timedOut = false,
        bool cancelled = false) =>
        new(exitCode, duration, timedOut, cancelled, "out", "err", ValidHash);

    private static SuiteExecutionRecord ManagedResult(SuiteStatus status) => new(
        "managed-suite", status, "wave-managed", null, 1, false, false,
        null, null, null, null);
}
