using System.Text.Json;

namespace DesktopNode.Verification.Tests;

public sealed class VerificationApplicationTests
{
    private static readonly DateTimeOffset StartedAt = DateTimeOffset.Parse("2026-08-24T01:02:03Z");
    private static readonly DateTimeOffset CompletedAt = StartedAt.AddSeconds(2);

    [Theory]
    [InlineData("null")]
    [InlineData("throw")]
    public async Task ClockBoundaryFailureBeforeSafeRootUsesCompactApplicationFailure(string scenario)
    {
        const string dangerous = "TOP_SECRET_CLOCK_MESSAGE_112358";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        IVerificationClock? clock = scenario == "null"
            ? null
            : new ThrowingVerificationClock(1, new InvalidOperationException(dangerous), StartedAt);
        var application = new VerificationApplication(
            process,
            managed,
            new PhysicalVerificationFileSystem(),
            clock!,
            () => repository.Root,
            () => null,
            () => repository.UserProfile);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", $"clock-{scenario}", "--plan-only"));

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        AssertCompactBoundary(
            outcome.StandardError,
            VerificationErrorCodes.ConfigInvalid,
            "application-failure");
        Assert.DoesNotContain(dangerous, outcome.StandardError, StringComparison.Ordinal);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task ClockBoundaryFailureAfterSafeRootUsesStartedTimestampForFailureSummary()
    {
        const string dangerous = "TOP_SECRET_LATE_CLOCK_MESSAGE_223606";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var clock = new ThrowingVerificationClock(
            2,
            new InvalidOperationException(dangerous),
            StartedAt);
        var application = repository.CreateApplication(process, managed, clock);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "clock-late", "--plan-only"));

        var summaryPath = repository.ArtifactPath("clock-late") + Path.DirectorySeparatorChar + "summary.json";
        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(summaryPath, outcome.StandardOutput.Trim());
        Assert.Equal(string.Empty, outcome.StandardError);
        var summary = ReadSummary(summaryPath);
        Assert.False(summary.Ok);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
        Assert.Equal(StartedAt, summary.StartedAt);
        Assert.Equal(StartedAt, summary.CompletedAt);
        Assert.Empty(summary.Results);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task FullPlanOnlyWritesPassingSevenSuiteSummaryWithoutStartingRunners()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(
            process,
            managed,
            FixedVerificationClock.At(StartedAt, CompletedAt));
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        var exitCode = await application.RunAsync(
            Arguments("Full", "M", ".github/workflows/development-gates.yml", "full", "--plan-only"),
            stdout,
            stderr,
            CancellationToken.None);

        var summaryPath = repository.ArtifactPath("full") + Path.DirectorySeparatorChar + "summary.json";
        var summary = ReadSummary(summaryPath);
        Assert.Equal(0, exitCode);
        Assert.Equal(summaryPath, stdout.ToString().Trim());
        Assert.Equal(string.Empty, stderr.ToString());
        Assert.Equal(2, summary.SchemaVersion);
        Assert.Equal("pcv-development-verification-summary-v2", summary.Contract);
        Assert.True(summary.Ok);
        Assert.True(summary.PlanOnly);
        Assert.Equal("Full", summary.RequestedLane);
        Assert.Equal("Full", summary.EffectiveLane);
        Assert.Equal("lane", summary.ExecutionScope);
        Assert.Equal("plan-only-foundation", summary.CatalogActivationState);
        Assert.Equal(7, summary.Results.Count);
        Assert.All(summary.Results, result => Assert.Equal("planned", result.Status));
        Assert.Equal(StartedAt, summary.StartedAt);
        Assert.Equal(CompletedAt, summary.CompletedAt);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task ActualExecutionIsLockedBeforeExecutorAndWritesFailureSummary()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        var exitCode = await application.RunAsync(
            Arguments("Full", "M", ".github/workflows/development-gates.yml", "actual"),
            stdout,
            stderr,
            CancellationToken.None);

        var summary = ReadSummary(repository.ArtifactPath("actual") + Path.DirectorySeparatorChar + "summary.json");
        Assert.Equal(2, exitCode);
        Assert.False(summary.Ok);
        Assert.False(summary.PlanOnly);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
        Assert.Equal("plan-only-foundation", summary.CatalogActivationState);
        Assert.Empty(summary.Results);
        Assert.NotEmpty(stdout.ToString());
        Assert.Equal(string.Empty, stderr.ToString());
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task FastSourcePlanSelectsOnlyDotnetWithoutStartingRunners()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);

        var outcome = await RunAsync(
            application,
            Arguments("Fast", "S", "src/DesktopNode.Runtime/Internal.cs", "fast", "--plan-only"));
        var summary = ReadSummary(repository.ArtifactPath("fast") + Path.DirectorySeparatorChar + "summary.json");

        Assert.Equal(0, outcome.ExitCode);
        Assert.Equal("Fast", summary.RequestedLane);
        Assert.Equal("Fast", summary.EffectiveLane);
        Assert.Equal("lane", summary.ExecutionScope);
        Assert.Equal(["dotnet"], summary.Results.Select(result => result.SuiteId));
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Theory]
    [InlineData("partial", null, "dotnet,policy-boundaries", "--suite", "policy-boundaries", "--suite", "dotnet")]
    [InlineData("shard", "web", "web-typecheck,web-parity", "--shard", "web", null, null)]
    public async Task SelectedPlanPreservesExactScopeMetadataAndResults(
        string expectedScope,
        string? expectedShard,
        string expectedSuites,
        string selectorOne,
        string valueOne,
        string? selectorTwo,
        string? valueTwo)
    {
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        var extra = new List<string> { selectorOne, valueOne };
        if (selectorTwo is not null && valueTwo is not null)
        {
            extra.Add(selectorTwo);
            extra.Add(valueTwo);
        }
        extra.Add("--plan-only");

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", ".github/workflows/development-gates.yml", expectedScope, extra.ToArray()));
        var summary = ReadSummary(repository.ArtifactPath(expectedScope) + Path.DirectorySeparatorChar + "summary.json");

        Assert.Equal(0, outcome.ExitCode);
        Assert.Equal(expectedScope, summary.ExecutionScope);
        Assert.NotEqual("lane", summary.ExecutionScope);
        Assert.Equal(expectedShard, summary.ShardId);
        Assert.Equal(expectedSuites.Split(','), summary.Results.Select(result => result.SuiteId));
        Assert.All(summary.Results, result => Assert.Equal("planned", result.Status));
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task InvalidArtifactRootUsesDeterministicCompactStderrWithoutCreatingDirectory()
    {
        const string dangerous = "TOP_SECRET_CREDENTIAL_TOKEN_314159";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var unsafeDirectory = Path.Combine(repository.Root, $"%{dangerous}%", "wave-a");

        var args = Arguments("Full", "M", "src/a.cs", "unused", "--plan-only");
        args[Array.IndexOf(args, "artifacts/unused")] = $"%{dangerous}%/wave-a";
        var exitCode = await application.RunAsync(args, stdout, stderr, CancellationToken.None);

        Assert.Equal(2, exitCode);
        Assert.Equal(string.Empty, stdout.ToString());
        Assert.Equal(
            "{\"schema_version\":2,\"contract\":\"pcv-development-verification-summary-v2\",\"ok\":false,\"error_code\":\"PCV_VERIFY_ARTIFACT_ROOT_INVALID\",\"error_detail\":\"artifact-root-invalid:unresolved\"}" + Environment.NewLine,
            stderr.ToString());
        Assert.DoesNotContain(dangerous, stderr.ToString(), StringComparison.Ordinal);
        Assert.False(Directory.Exists(unsafeDirectory));
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Theory]
    [InlineData("invalid-lane", "cli:invalid-lane")]
    [InlineData("invalid-shard", "cli:invalid-shard")]
    [InlineData("unknown-option", "cli:unknown-option")]
    [InlineData("duplicate-suite", "cli:duplicate-suite")]
    public async Task UserControlledCliDetailIsProjectedToFixedCategory(
        string scenario,
        string expectedDetail)
    {
        const string dangerous = "TOP_SECRET_AUTHORIZATION_TOKEN_577215";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        var args = Arguments("Full", "M", "src/a.cs", scenario, "--plan-only").ToList();
        switch (scenario)
        {
            case "invalid-lane":
                args[args.IndexOf("Full")] = dangerous;
                break;
            case "invalid-shard":
                args.AddRange(["--shard", dangerous]);
                break;
            case "unknown-option":
                args.Add($"--{dangerous}");
                break;
            case "duplicate-suite":
                args.AddRange(["--suite", dangerous, "--suite", dangerous]);
                break;
        }

        var outcome = await RunAsync(application, args);

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        Assert.Equal(
            $"{{\"schema_version\":2,\"contract\":\"pcv-development-verification-summary-v2\",\"ok\":false,\"error_code\":\"PCV_VERIFY_CONFIG_INVALID\",\"error_detail\":\"{expectedDetail}\"}}" + Environment.NewLine,
            outcome.StandardError);
        Assert.DoesNotContain(dangerous, outcome.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("authorization", outcome.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", outcome.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(repository.ArtifactPath(scenario) + Path.DirectorySeparatorChar + "summary.json"));
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("TOP_SECRET_UNKNOWN_CODE_314159")]
    [InlineData("PCV_VERIFY_CONFIG_INVALID\r\nTOP_SECRET_CONTROL")]
    [InlineData("PCV_VERIFY_CONFIG_INVALID\0TOP_SECRET_NUL")]
    [InlineData("PCV_VERIFY_CONFIG_INVALID한글TOP_SECRET_UNICODE")]
    [InlineData("\"PCV_VERIFY_CONFIG_INVALID\":\"TOP_SECRET_JSON\"")]
    public async Task ClosedErrorBoundaryNormalizesMaliciousCodeAndDetailBeforeSafeRoot(string? maliciousCode)
    {
        const string maliciousDetail = "summary-write-failed=TOP_SECRET_DETAIL\r\n{\"token\":\"credential\"}\0한글";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(
            process,
            managed,
            currentDirectory: () => throw new VerificationException(maliciousCode!, maliciousDetail));

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "malicious-code", "--plan-only"));

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        AssertCompactBoundary(
            outcome.StandardError,
            VerificationErrorCodes.ConfigInvalid,
            "application-failure");
        Assert.DoesNotContain("TOP_SECRET", outcome.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("credential", outcome.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("한글", outcome.StandardError, StringComparison.Ordinal);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task ClosedErrorBoundaryNormalizesMaliciousFilesystemVerificationExceptionAfterSafeRoot()
    {
        const string dangerous = "TOP_SECRET_FILESYSTEM_CODE_DETAIL_173205";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var fileSystem = new FirstReadFailureVerificationFileSystem(
            new VerificationException($"{dangerous}\r\n한글", $"{dangerous}=\0{{}}"));
        var application = repository.CreateApplication(process, managed, fileSystem: fileSystem);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "malicious-filesystem", "--plan-only"));

        var summaryPath = repository.ArtifactPath("malicious-filesystem") + Path.DirectorySeparatorChar + "summary.json";
        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(summaryPath, outcome.StandardOutput.Trim());
        Assert.Equal(string.Empty, outcome.StandardError);
        var summary = ReadSummary(summaryPath);
        Assert.False(summary.Ok);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
        Assert.Empty(summary.Results);
        Assert.DoesNotContain(dangerous, File.ReadAllText(summaryPath), StringComparison.Ordinal);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task MalformedCatalogAfterSafeRootWritesFailureSummary()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        File.WriteAllText(repository.CatalogPath, "{");
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "malformed", "--plan-only"));
        var summary = ReadSummary(repository.ArtifactPath("malformed") + Path.DirectorySeparatorChar + "summary.json");

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardError);
        Assert.False(summary.Ok);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
        Assert.Equal("unavailable", summary.CatalogActivationState);
        Assert.Empty(summary.Results);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Theory]
    [InlineData("cli")]
    [InlineData("repository")]
    [InlineData("schema")]
    public async Task CliRepositoryAndSchemaFailuresStayAtTheCorrectSafeBoundary(string scenario)
    {
        const string dangerous = "TOP_SECRET_ENV_TOKEN_CREDENTIAL_271828";
        using var repository = ApplicationRepositoryFixture.Create();
        if (scenario == "repository")
        {
            File.Delete(repository.SolutionPath);
        }
        else if (scenario == "schema")
        {
            File.Delete(repository.SchemaPath);
        }
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var args = scenario == "cli"
            ? Arguments("Full", "M", "src/a.cs", scenario, "--unknown", dangerous, "--plan-only")
            : Arguments("Full", "M", "src/a.cs", scenario, "--plan-only");

        var exitCode = await application.RunAsync(args, stdout, stderr, CancellationToken.None);

        Assert.Equal(2, exitCode);
        Assert.DoesNotContain(dangerous, stdout.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(dangerous, stderr.ToString(), StringComparison.Ordinal);
        if (scenario == "schema")
        {
            var summary = ReadSummary(repository.ArtifactPath(scenario) + Path.DirectorySeparatorChar + "summary.json");
            Assert.Equal(string.Empty, stderr.ToString());
            Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
            Assert.Empty(summary.Results);
        }
        else
        {
            Assert.Equal(string.Empty, stdout.ToString());
            using var document = JsonDocument.Parse(stderr.ToString());
            Assert.Equal(VerificationErrorCodes.ConfigInvalid, document.RootElement.GetProperty("error_code").GetString());
            Assert.True(document.RootElement.TryGetProperty("error_detail", out _));
            Assert.False(File.Exists(repository.ArtifactPath(scenario) + Path.DirectorySeparatorChar + "summary.json"));
        }
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task PreCancelledRequestDoesNotStartWorkAndUsesStableBoundaryError()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "cancelled", "--plan-only"),
            cancellation.Token);

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        using var document = JsonDocument.Parse(outcome.StandardError);
        Assert.Equal(VerificationErrorCodes.Cancelled, document.RootElement.GetProperty("error_code").GetString());
        Assert.Equal("operation=cancelled", document.RootElement.GetProperty("error_detail").GetString());
        Assert.False(File.Exists(repository.ArtifactPath("cancelled") + Path.DirectorySeparatorChar + "summary.json"));
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task WriterFailureIsClassifiedOnceWithoutLeakingExceptionMessage()
    {
        const string dangerous = "TOP_SECRET_WRITER_EXCEPTION_161803";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var fileSystem = new FaultInjectingVerificationFileSystem(new IOException(dangerous));
        var application = repository.CreateApplication(process, managed, fileSystem: fileSystem);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "writer-failure", "--plan-only"));

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        Assert.Equal(1, fileSystem.WriteCallCount);
        Assert.DoesNotContain(dangerous, outcome.StandardError, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(outcome.StandardError);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, document.RootElement.GetProperty("error_code").GetString());
        Assert.Equal("summary-write-failed", document.RootElement.GetProperty("error_detail").GetString());
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task StdoutFailureAfterCommittedSuccessRewritesTargetAsFailureWithoutRetryingStdout()
    {
        const string dangerous = "TOP_SECRET_STDOUT_FAILURE_244949";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(process, managed);
        using var stdout = new ThrowingLineTextWriter(new IOException(dangerous));
        using var stderr = new StringWriter();

        var exitCode = await application.RunAsync(
            Arguments("Full", "M", "src/a.cs", "stdout-failure", "--plan-only"),
            stdout,
            stderr,
            CancellationToken.None);

        var summaryPath = repository.ArtifactPath("stdout-failure") + Path.DirectorySeparatorChar + "summary.json";
        Assert.Equal(2, exitCode);
        Assert.Equal(1, stdout.CallCount);
        AssertCompactBoundary(
            stderr.ToString(),
            VerificationErrorCodes.ConfigInvalid,
            "standard-output-write-failed");
        Assert.DoesNotContain(dangerous, stderr.ToString(), StringComparison.Ordinal);
        var summary = ReadSummary(summaryPath);
        Assert.False(summary.Ok);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
        Assert.Empty(summary.Results);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task StdoutRecoveryMoveFailureDeletesCommittedSuccessAndEmitsOneFixedError()
    {
        const string dangerous = "TOP_SECRET_RECOVERY_MOVE_FAILURE_264575";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var fileSystem = new RecoveryMoveFailureVerificationFileSystem(new IOException(dangerous));
        var application = repository.CreateApplication(process, managed, fileSystem: fileSystem);
        using var stdout = new ThrowingLineTextWriter(new IOException(dangerous));
        using var stderr = new StringWriter();

        var exitCode = await application.RunAsync(
            Arguments("Full", "M", "src/a.cs", "stdout-recovery-failure", "--plan-only"),
            stdout,
            stderr,
            CancellationToken.None);

        var summaryPath = repository.ArtifactPath("stdout-recovery-failure") + Path.DirectorySeparatorChar + "summary.json";
        Assert.Equal(2, exitCode);
        Assert.Equal(1, stdout.CallCount);
        Assert.Equal(2, fileSystem.MoveCallCount);
        Assert.True(fileSystem.DeleteCallCount >= 1);
        Assert.False(File.Exists(summaryPath));
        AssertCompactBoundary(
            stderr.ToString(),
            VerificationErrorCodes.ConfigInvalid,
            "standard-output-write-failed");
        Assert.DoesNotContain(dangerous, stderr.ToString(), StringComparison.Ordinal);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task InitialSummaryWriterFailurePreservesPreExistingTarget()
    {
        const string existing = "PRE_EXISTING_SUMMARY_NOT_FROM_THIS_RUN";
        using var repository = ApplicationRepositoryFixture.Create();
        var artifactRoot = repository.ArtifactPath("existing-target");
        Directory.CreateDirectory(artifactRoot);
        var summaryPath = Path.Combine(artifactRoot, "summary.json");
        File.WriteAllText(summaryPath, existing);
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var fileSystem = new FaultInjectingVerificationFileSystem(new IOException("TOP_SECRET_INITIAL_WRITE"));
        var application = repository.CreateApplication(process, managed, fileSystem: fileSystem);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "existing-target", "--plan-only"));

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        Assert.Equal(existing, File.ReadAllText(summaryPath));
        AssertCompactBoundary(
            outcome.StandardError,
            VerificationErrorCodes.ConfigInvalid,
            "summary-write-failed");
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task CallerWriterCancellationWritesCancelledFailureSummaryWithUncancelledRecovery()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        using var cancellation = new CancellationTokenSource();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var fileSystem = new FirstWriteCancellationVerificationFileSystem(
            cancellation.Token,
            cancellation);
        var application = repository.CreateApplication(process, managed, fileSystem: fileSystem);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "writer-cancelled", "--plan-only"),
            cancellation.Token);

        var summaryPath = repository.ArtifactPath("writer-cancelled") + Path.DirectorySeparatorChar + "summary.json";
        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(summaryPath, outcome.StandardOutput.Trim());
        Assert.Equal(string.Empty, outcome.StandardError);
        Assert.Equal(2, fileSystem.WriteCallCount);
        Assert.Equal(cancellation.Token, fileSystem.WriteTokens[0]);
        Assert.Equal(CancellationToken.None, fileSystem.WriteTokens[1]);
        var summary = ReadSummary(summaryPath);
        Assert.False(summary.Ok);
        Assert.Equal(VerificationErrorCodes.Cancelled, summary.ErrorCode);
        Assert.Empty(summary.Results);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task UnrelatedWriterCancellationIsNotClassifiedAsCallerCancellation()
    {
        using var repository = ApplicationRepositoryFixture.Create();
        using var callerCancellation = new CancellationTokenSource();
        using var unrelatedCancellation = new CancellationTokenSource();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var fileSystem = new FirstWriteCancellationVerificationFileSystem(unrelatedCancellation.Token);
        var application = repository.CreateApplication(process, managed, fileSystem: fileSystem);

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "writer-unrelated-cancel", "--plan-only"),
            callerCancellation.Token);

        var summaryPath = repository.ArtifactPath("writer-unrelated-cancel") + Path.DirectorySeparatorChar + "summary.json";
        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(summaryPath, outcome.StandardOutput.Trim());
        Assert.Equal(string.Empty, outcome.StandardError);
        Assert.Equal(2, fileSystem.WriteCallCount);
        var summary = ReadSummary(summaryPath);
        Assert.False(summary.Ok);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, summary.ErrorCode);
        Assert.Empty(summary.Results);
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    [Fact]
    public async Task UnexpectedApplicationFailureDoesNotExposeExceptionMessage()
    {
        const string dangerous = "TOP_SECRET_APPLICATION_EXCEPTION_141421";
        using var repository = ApplicationRepositoryFixture.Create();
        var process = new RecordingProcessRunner(failIfCalled: true);
        var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
        var application = repository.CreateApplication(
            process,
            managed,
            currentDirectory: () => throw new InvalidOperationException(dangerous));

        var outcome = await RunAsync(
            application,
            Arguments("Full", "M", "src/a.cs", "unexpected", "--plan-only"));

        Assert.Equal(2, outcome.ExitCode);
        Assert.Equal(string.Empty, outcome.StandardOutput);
        Assert.DoesNotContain(dangerous, outcome.StandardError, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(outcome.StandardError);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, document.RootElement.GetProperty("error_code").GetString());
        Assert.Equal("application-failure", document.RootElement.GetProperty("error_detail").GetString());
        Assert.Equal(0, process.CallCount);
        Assert.Equal(0, managed.CallCount);
    }

    private static string[] Arguments(
        string lane,
        string tier,
        string changedPath,
        string artifactName,
        params string[] extra) =>
        [
            "verify",
            "--lane", lane,
            "--change-tier", tier,
            "--changed-path", changedPath,
            "--artifact-root", $"artifacts/{artifactName}",
            .. extra
        ];

    private static VerificationSummary ReadSummary(string path) =>
        JsonSerializer.Deserialize<VerificationSummary>(File.ReadAllText(path), VerificationJson.Options)!;

    private static void AssertCompactBoundary(string json, string errorCode, string errorDetail)
    {
        Assert.Equal(
            $"{{\"schema_version\":2,\"contract\":\"pcv-development-verification-summary-v2\",\"ok\":false,\"error_code\":\"{errorCode}\",\"error_detail\":\"{errorDetail}\"}}" + Environment.NewLine,
            json);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(5, document.RootElement.EnumerateObject().Count());
    }

    private static async Task<ApplicationOutcome> RunAsync(
        VerificationApplication application,
        IReadOnlyList<string> args,
        CancellationToken cancellationToken = default)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var exitCode = await application.RunAsync(args, stdout, stderr, cancellationToken);
        return new ApplicationOutcome(exitCode, stdout.ToString(), stderr.ToString());
    }

    private sealed record ApplicationOutcome(int ExitCode, string StandardOutput, string StandardError);
}
