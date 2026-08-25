using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DesktopNode.Verification.Tests;

[Collection("Console contract tests")]
public sealed class VerificationProcessTests
{
    private static readonly string[] CanonicalAllowed =
        ["dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"];

    [Fact]
    public void BuildsArgumentListWithoutACommandShell()
    {
        var invocation = Invocation(
            "dotnet",
            ["test", @"path with spaces\project.csproj", "--filter", "Name=a;b"]);

        var startInfo = ProcessStartInfoFactory.Create(invocation);

        Assert.Equal(ProcessExecutableResolver.Resolve("dotnet"), startInfo.FileName);
        Assert.Equal(invocation.WorkingDirectory, startInfo.WorkingDirectory);
        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.RedirectStandardOutput);
        Assert.True(startInfo.RedirectStandardError);
        Assert.False(startInfo.RedirectStandardInput);
        Assert.True(startInfo.CreateNoWindow);
        Assert.Equal(invocation.Arguments, startInfo.ArgumentList);
        Assert.Empty(startInfo.Arguments);
        Assert.False(startInfo.Environment.TryGetValue("PCV_TOKEN", out var sensitiveValue));
        Assert.Null(sensitiveValue);
        Assert.True(startInfo.Environment.ContainsKey("PATH"));
        Assert.True(string.IsNullOrEmpty(startInfo.Verb));
    }

    [Fact]
    public void ResolvesNpmCmdOnlyOnWindows()
    {
        Assert.Equal(OperatingSystem.IsWindows() ? "npm.cmd" : "npm", ProcessExecutableResolver.Resolve("npm"));
        Assert.Equal("npm.cmd", ProcessExecutableResolver.Resolve("npm.cmd"));
    }

    [Fact]
    public void BuildsNpmLaunchWithNodeAndNpmCliWithoutACommandShell()
    {
        var startInfo = ProcessStartInfoFactory.Create(Invocation("npm", ["--version"]));

        if (OperatingSystem.IsWindows())
        {
            Assert.Equal("node.exe", Path.GetFileName(startInfo.FileName), ignoreCase: true);
            Assert.EndsWith("npm-cli.js", startInfo.ArgumentList[0], StringComparison.OrdinalIgnoreCase);
            Assert.Equal("--version", startInfo.ArgumentList[1]);
            var npmCliDirectory = Path.GetDirectoryName(Path.GetFullPath(startInfo.ArgumentList[0]))!;
            var npmInstallRoot = Directory.GetParent(npmCliDirectory)!.Parent!.Parent!.FullName;
            Assert.Equal(
                npmInstallRoot,
                Path.GetDirectoryName(Path.GetFullPath(startInfo.FileName)),
                ignoreCase: true);
        }
        else
        {
            Assert.Equal("npm", startInfo.FileName);
            Assert.Equal(["--version"], startInfo.ArgumentList);
        }

        Assert.False(startInfo.UseShellExecute);
        Assert.DoesNotContain("cmd", Path.GetFileName(startInfo.FileName), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunsNpmVersionWithoutACommandShell()
    {
        var result = await new SystemProcessRunner().RunAsync(
            Invocation("npm", ["--version"]),
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(result.Cancelled);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput));
    }

    [Fact]
    public void RejectsNpmInstallWhenNodeIsOnlyAvailableFromAnotherPathRoot()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var originalPath = Environment.GetEnvironmentVariable("PATH")!;
        var nodeDirectory = FindExecutableDirectoryOnPath(originalPath, "node.exe");
        Assert.NotNull(nodeDirectory);
        var isolatedNpmRoot = Path.Combine(Path.GetTempPath(), $"pcv-npm-root-{Guid.NewGuid():N}");
        var npmCliDirectory = Path.Combine(isolatedNpmRoot, "node_modules", "npm", "bin");

        try
        {
            Directory.CreateDirectory(npmCliDirectory);
            File.WriteAllText(Path.Combine(isolatedNpmRoot, "npm.cmd"), "untrusted-test-shim");
            File.WriteAllText(Path.Combine(npmCliDirectory, "npm-cli.js"), "untrusted-test-cli");
            Environment.SetEnvironmentVariable(
                "PATH",
                string.Join(Path.PathSeparator, isolatedNpmRoot, nodeDirectory));

            var exception = Assert.Throws<VerificationException>(() =>
                ProcessStartInfoFactory.Create(Invocation("npm", ["--version"])));

            Assert.Equal(VerificationErrorCodes.ProcessFailed, exception.Code);
            Assert.Equal("suite=verification-process-tests;reason=NpmDiscoveryFailed", exception.Detail);
            Assert.DoesNotContain(isolatedNpmRoot, exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nodeDirectory!, exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            if (Directory.Exists(isolatedNpmRoot) &&
                Path.GetFullPath(isolatedNpmRoot).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(isolatedNpmRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void NpmDiscoveryFailureIsStableAndDoesNotLeakPathState()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var originalPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", string.Empty);

            var exception = Assert.Throws<VerificationException>(() =>
                ProcessStartInfoFactory.Create(Invocation("npm", ["--version"])));

            Assert.Equal(VerificationErrorCodes.ProcessFailed, exception.Code);
            Assert.Equal("suite=verification-process-tests;reason=NpmDiscoveryFailed", exception.Detail);
            Assert.DoesNotContain(originalPath ?? string.Empty, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
        }
    }

    [Theory]
    [InlineData("pwsh", "-NoProfile", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
    [InlineData("powershell.exe", "-File", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
    [InlineData("dotnet", "Invoke-Pester", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
    [InlineData("cmd.exe", "/c", "PCV_VERIFY_CONFIG_INVALID")]
    [InlineData("msiexec.exe", "/i", "PCV_VERIFY_CONFIG_INVALID")]
    public void RejectsForbiddenCommandsWithStableCode(string fileName, string argument, string expectedCode)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate(fileName, [argument], [fileName]));

        Assert.Equal(expectedCode, exception.Code);
        Assert.DoesNotContain(argument, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AllowsBenignSubstringsButRejectsDelimitedForbiddenTokens()
    {
        ProcessCommandGuard.Validate("dotnet", ["Invoke-Pesterer", "renew-vmware", "power-shell-safe"], CanonicalAllowed);

        var exception = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate("dotnet", ["--filter=Invoke-Pester"], CanonicalAllowed));

        Assert.Equal(VerificationErrorCodes.NonAdminPowerShellForbidden, exception.Code);
    }

    [Theory]
    [InlineData("--new-vm", "PCV_VERIFY_CONFIG_INVALID")]
    [InlineData("--allowhostmutation", "PCV_VERIFY_CONFIG_INVALID")]
    [InlineData("--powershell", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
    public void RejectsForbiddenTokensDelimitedByHyphens(string argument, string expectedCode)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate("dotnet", [argument], CanonicalAllowed));

        Assert.Equal(expectedCode, exception.Code);
    }

    [Fact]
    public void RejectsCallerAuthoredExecutableAllowlist()
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate("calc.exe", [], ["calc.exe"]));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
    }

    [Fact]
    public void RequiresTheCompleteCanonicalExecutableAllowlist()
    {
        var missing = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate("dotnet", [], ["dotnet", "dotnet.exe"]));
        var duplicate = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate("dotnet", [], CanonicalAllowed.Append("dotnet").ToArray()));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, missing.Code);
        Assert.Equal(VerificationErrorCodes.ConfigInvalid, duplicate.Code);
        ProcessCommandGuard.Validate("dotnet", [], CanonicalAllowed.Reverse().ToArray());
    }

    [Fact]
    public void FactoryValidatesCommandsWithoutRunner()
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ProcessStartInfoFactory.Create(Invocation("dotnet", ["--powershell"])));

        Assert.Equal(VerificationErrorCodes.NonAdminPowerShellForbidden, exception.Code);
    }

    [Fact]
    public void PathQualifiedPowerShellUsesPowerShellSpecificCode()
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate(@"C:\private\pwsh.exe", [], CanonicalAllowed));

        Assert.Equal(VerificationErrorCodes.NonAdminPowerShellForbidden, exception.Code);
    }

    [Theory]
    [InlineData(@"C:\private\dotnet.exe")]
    [InlineData("../dotnet")]
    [InlineData("tools/dotnet")]
    public void RejectsPathQualifiedExecutable(string fileName)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ProcessCommandGuard.Validate(fileName, [], CanonicalAllowed));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.DoesNotContain(fileName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RemovesExpandedSensitiveEnvironmentKeysAndPreservesUnrelatedKeys()
    {
        var sentinels = new Dictionary<string, string?>
        {
            ["PCV_TEST_API_KEY"] = "api-key-value",
            ["PCV_TEST_ACCESS_KEY"] = "access-key-value",
            ["PCV_TEST_PRIVATE_KEY"] = "private-key-value",
            ["PCV_TEST_CONNECTIONSTRING"] = "connection-value",
            ["PCV_TEST_DATABASE_URL"] = "database-value",
            ["PCV_TEST_COOKIE"] = "cookie-value",
            ["PCV_TEST_SESSION"] = "session-value",
            ["PCV_TEST_PAT"] = "pat-value",
            ["PCV_TEST_BEARER"] = "bearer-value",
            ["PCV_TEST_UNRELATED"] = "keep-value"
        };
        var originals = sentinels.Keys.ToDictionary(key => key, Environment.GetEnvironmentVariable);

        try
        {
            foreach (var sentinel in sentinels)
            {
                Environment.SetEnvironmentVariable(sentinel.Key, sentinel.Value);
                Assert.Equal(sentinel.Value, Environment.GetEnvironmentVariable(sentinel.Key));
            }

            var startInfo = ProcessStartInfoFactory.Create(Invocation("dotnet", ["--version"]));

            foreach (var sensitiveKey in sentinels.Keys.Where(key => key != "PCV_TEST_UNRELATED"))
            {
                Assert.False(startInfo.Environment.ContainsKey(sensitiveKey));
            }

            Assert.Equal("keep-value", startInfo.Environment["PCV_TEST_UNRELATED"]);
            Assert.True(startInfo.Environment.ContainsKey("PATH"));
            Assert.True(startInfo.Environment.ContainsKey("SystemRoot"));
        }
        finally
        {
            foreach (var original in originals)
            {
                Environment.SetEnvironmentVariable(original.Key, original.Value);
            }
        }
    }

    [Fact]
    public void RedactsSecretsAndCapsEachStream()
    {
        var text = "D:\\repo\\private / d:/repo/private Authorization: Bearer top-secret " +
            "--token raw-token --password=raw-password --secret raw-secret " +
            "{\"databasePassword\":\"pw\",\"credential_name\":\"cred\"}" + new string('x', 9000);

        var sanitized = ProcessOutputSanitizer.Sanitize(text, @"D:\repo", 8192);

        Assert.DoesNotContain("top-secret", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-token", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-password", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-secret", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("\"pw\"", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("\"cred\"", sanitized, StringComparison.Ordinal);
        Assert.Contains("[REPO_ROOT]", sanitized, StringComparison.Ordinal);
        Assert.Contains("[REDACTED]", sanitized, StringComparison.Ordinal);
        Assert.Equal(8192, sanitized.Length);
        Assert.EndsWith("...[truncated]", sanitized, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1, ".")]
    [InlineData(5, "...[t")]
    [InlineData(14, "...[truncated]")]
    public void KeepsTruncationSuffixInsideSmallCaps(int limit, string expected)
    {
        Assert.Equal(expected, ProcessOutputSanitizer.Sanitize("longer than the limit", "", limit));
    }

    [Fact]
    public void RedactsQuotedEscapedAndStructuredSecretBypasses()
    {
        const string bearerSecret = "bearer secret tail";
        const string cliToken = "token escaped tail";
        const string cliPassword = "password spaced tail";
        const string nestedSecret = "nested-json-secret";
        var text = "benign-prefix Authorization: Bearer \"bearer secret \\\"tail\" " +
            "--token = \"token escaped \\\"tail\" --password=  'password spaced tail' " +
            "{\"to\\u006ben\":{\"nested\":[\"nested-json-secret\"]}," +
            "\"safe\":\"benign-json\",\"path\":\"D:\\\\repo\\\\private\"}";

        var sanitized = ProcessOutputSanitizer.Sanitize(text, @"D:\repo", 8192);

        Assert.Contains("benign-prefix", sanitized, StringComparison.Ordinal);
        Assert.Contains("benign-json", sanitized, StringComparison.Ordinal);
        Assert.Contains("[REPO_ROOT]", sanitized, StringComparison.Ordinal);
        Assert.Contains("[REDACTED]", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain(bearerSecret, sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain(cliToken, sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain(cliPassword, sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain(nestedSecret, sanitized, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("prefix {\"token\":\"string-secret-tail", "string-secret-tail")]
    [InlineData("prefix {\"password\":{\"nested\":\"object-secret-tail\"", "object-secret-tail")]
    [InlineData("prefix {\"credential\":[\"array-secret-tail\"", "array-secret-tail")]
    public void FailsClosedForIncompleteSensitiveJsonValues(string text, string secret)
    {
        var sanitized = ProcessOutputSanitizer.Sanitize(text, RepositoryRoot(), 8192);

        Assert.Equal("prefix " + text["prefix ".Length..(text.IndexOf(':') + 1)] + "[REDACTED]", sanitized);
        Assert.DoesNotContain(secret, sanitized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnterminatedSensitiveJsonBeyondRetentionBoundaryNeverLeaksOrDeadlocks()
    {
        const string secretFragment = "raw-retention-secret";
        var script = "process.stdout.write('{\\\"token\\\":\\\"raw-retention-secret-' + 'x'.repeat(70000));";

        var result = await new SystemProcessRunner().RunAsync(
            Invocation("node", ["-e", script]),
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(result.Cancelled);
        Assert.DoesNotContain(secretFragment, result.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("{\"token\":[REDACTED]", result.StandardOutput);
        Assert.True(result.StandardOutput.Length <= 8192);
        Assert.Equal(ExpectedHash(result.StandardOutput, result.StandardError), result.OutputSha256);
    }

    [Fact]
    public async Task CanonicalizesWorkingDirectoryForLaunchAndOutputRedaction()
    {
        var nonCanonicalRoot = Path.Combine(RepositoryRoot(), "src", "..");
        var canonicalRoot = Path.GetFullPath(nonCanonicalRoot);
        var invocation = Invocation(
            "node",
            ["-e", "process.stdout.write(process.cwd())"],
            workingDirectory: nonCanonicalRoot);

        var startInfo = ProcessStartInfoFactory.Create(invocation);
        var result = await new SystemProcessRunner().RunAsync(invocation, CancellationToken.None);

        Assert.Equal(canonicalRoot, startInfo.WorkingDirectory);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("[REPO_ROOT]", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(canonicalRoot, result.StandardOutput, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunsSafeDotnetProcessAndPreservesTerminalResult()
    {
        var result = await new SystemProcessRunner().RunAsync(
            Invocation("dotnet", ["--version"], timeout: TimeSpan.FromSeconds(30)),
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(result.Cancelled);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput));
        Assert.Equal(ExpectedHash(result.StandardOutput, result.StandardError), result.OutputSha256);
    }

    [Fact]
    public async Task TimeoutKillsTheEntireProcessTreeAndReturnsTimedOut()
    {
        var result = await new SystemProcessRunner().RunAsync(
            Invocation("dotnet", ["--info"], timeout: TimeSpan.FromMilliseconds(1)),
            CancellationToken.None);

        Assert.Null(result.ExitCode);
        Assert.True(result.TimedOut);
        Assert.False(result.Cancelled);
    }

    [Fact]
    public void UsesBoundedTreeTerminationWithoutShellFallbackOrUnboundedWaits()
    {
        var source = File.ReadAllText(Path.Combine(
            RepositoryRoot(), "src", "DesktopNode.Verification", "VerificationProcess.cs"));

        Assert.Contains("private static async Task TerminateAsync", source, StringComparison.Ordinal);
        Assert.Contains("process.Kill(entireProcessTree: true)", source, StringComparison.Ordinal);
        Assert.Contains("new CancellationTokenSource(CleanupTimeout)", source, StringComparison.Ordinal);
        Assert.Contains("WaitAsync(CleanupTimeout)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WaitForExitAsync(CancellationToken.None)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ComSpec", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Thread.Sleep", source, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CallerCancellationKillsReadyNodeProcessTree()
    {
        var markerRoot = Path.Combine(Path.GetTempPath(), $"pcv-cancel-tree-{Guid.NewGuid():N}");
        var markerPath = Path.Combine(markerRoot, "ready.marker");
        Directory.CreateDirectory(markerRoot);
        using var watcher = new FileSystemWatcher(markerRoot, Path.GetFileName(markerPath))
        {
            NotifyFilter = NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        FileSystemEventHandler signalReady = (_, _) => ready.TrySetResult();
        RenamedEventHandler signalRenamed = (_, _) => ready.TrySetResult();
        watcher.Created += signalReady;
        watcher.Renamed += signalRenamed;
        using var cancellation = new CancellationTokenSource();
        int? childPid = null;
        var script = "const fs=require('fs'),cp=require('child_process');" +
            "const child=cp.spawn(process.execPath,['-e','setInterval(()=>{},1000)'],{stdio:'ignore'});" +
            "fs.writeFileSync(process.argv[1]+'.tmp',String(child.pid));" +
            "fs.renameSync(process.argv[1]+'.tmp',process.argv[1]);setInterval(()=>{},1000);";
        var runTask = new SystemProcessRunner().RunAsync(
            Invocation("node", ["-e", script, markerPath]),
            cancellation.Token);

        try
        {
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(10));
            childPid = int.Parse(File.ReadAllText(markerPath), System.Globalization.CultureInfo.InvariantCulture);
            cancellation.Cancel();

            var result = await runTask.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Null(result.ExitCode);
            Assert.False(result.TimedOut);
            Assert.True(result.Cancelled);
            Assert.False(IsProcessAlive(childPid.Value));
        }
        finally
        {
            cancellation.Cancel();
            try
            {
                await runTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception exception) when (exception is TimeoutException or VerificationException)
            {
                // The assertions retain the primary failure; targeted child cleanup follows.
            }

            if (childPid is int pid && TryGetRunningProcess(pid, out var child))
            {
                using (child)
                {
                    try
                    {
                        child.Kill(entireProcessTree: true);
                        await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
                    }
                    catch (InvalidOperationException) when (child.HasExited)
                    {
                        // The targeted child exited between the liveness check and cleanup.
                    }
                }
            }

            if (Directory.Exists(markerRoot) &&
                Path.GetFullPath(markerRoot).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(markerRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task AlreadyCancelledReturnsWithoutValidatingOrStarting()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var invocation = Invocation(@"C:\private\does-not-exist.exe", ["Invoke-Pester"]);

        var result = await new SystemProcessRunner().RunAsync(invocation, cancellation.Token);

        Assert.Null(result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.True(result.Cancelled);
        Assert.Empty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        Assert.Equal(ExpectedHash("", ""), result.OutputSha256);
    }

    [Fact]
    public async Task PreservesOrdinaryNonzeroExitCode()
    {
        var result = await new SystemProcessRunner().RunAsync(
            Invocation("node", ["-e", "process.exit(7)"]),
            CancellationToken.None);

        Assert.Equal(7, result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(result.Cancelled);
    }

    [Fact]
    public async Task DrainsMoreThanInternalLimitFromBothStreamsWithoutDeadlock()
    {
        var result = await new SystemProcessRunner().RunAsync(
            Invocation("node", ["-e", "process.stdout.write('x'.repeat(70000));process.stderr.write('y'.repeat(70000));"]),
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(8192, result.StandardOutput.Length);
        Assert.Equal(8192, result.StandardError.Length);
        Assert.EndsWith("...[truncated]", result.StandardOutput, StringComparison.Ordinal);
        Assert.EndsWith("...[truncated]", result.StandardError, StringComparison.Ordinal);
        Assert.Equal(ExpectedHash(result.StandardOutput, result.StandardError), result.OutputSha256);
    }

    [Theory]
    [InlineData("", @"D:\repo", 30)]
    [InlineData("bad suite;secret", @"D:\repo", 30)]
    [InlineData("verification-process-tests", "relative", 30)]
    [InlineData("verification-process-tests", @"D:\repo", 3601)]
    public async Task RejectsInvalidInvocationShape(string suiteId, string workingDirectory, int timeoutSeconds)
    {
        var invocation = new ProcessInvocation(
            suiteId,
            "dotnet",
            ["--version"],
            workingDirectory,
            TimeSpan.FromSeconds(timeoutSeconds),
            CanonicalAllowed);

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new SystemProcessRunner().RunAsync(invocation, CancellationToken.None));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
    }

    [Fact]
    public async Task RejectsInvalidTimeoutWithoutStarting()
    {
        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new SystemProcessRunner().RunAsync(
                Invocation("dotnet", [], CanonicalAllowed, TimeSpan.Zero),
                CancellationToken.None));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
    }

    private static ProcessInvocation Invocation(
        string fileName,
        IReadOnlyList<string> arguments,
        IReadOnlyList<string>? allowedExecutables = null,
        TimeSpan? timeout = null,
        string? workingDirectory = null) =>
        new(
            "verification-process-tests",
            fileName,
            arguments,
            workingDirectory ?? RepositoryRoot(),
            timeout ?? TimeSpan.FromSeconds(30),
            allowedExecutables ?? CanonicalAllowed);

    private static string RepositoryRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string? FindExecutableDirectoryOnPath(string pathValue, string fileName) =>
        pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(entry => entry.Trim().Trim('"'))
            .FirstOrDefault(entry => File.Exists(Path.Combine(entry, fileName)));

    private static bool IsProcessAlive(int processId) => TryGetRunningProcess(processId, out var process)
        ? DisposeAndReturnAlive(process)
        : false;

    private static bool DisposeAndReturnAlive(Process process)
    {
        using (process)
        {
            return !process.HasExited;
        }
    }

    private static bool TryGetRunningProcess(int processId, out Process process)
    {
        try
        {
            process = Process.GetProcessById(processId);
            if (process.HasExited)
            {
                process.Dispose();
                process = null!;
                return false;
            }
            return true;
        }
        catch (ArgumentException)
        {
            process = null!;
            return false;
        }
    }

    private static string ExpectedHash(string standardOutput, string standardError) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(standardOutput + "\n" + standardError)))
            .ToLowerInvariant();
}
