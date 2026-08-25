using System.Collections.Frozen;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

internal static class ProcessCommandGuard
{
    private static readonly FrozenSet<string> CanonicalExecutables = new[]
    {
        "dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"
    }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly string[] PowerShellTokens =
        ["pwsh", "pwsh.exe", "powershell", "powershell.exe", "invoke-pester"];

    private static readonly string[] ForbiddenTokens =
    [
        "msiexec", "msiexec.exe", "sc.exe", "new-vm", "start-vm", "stop-vm",
        "start-service", "stop-service", "install-module", "allowhostmutation"
    ];

    internal static void Validate(
        string fileName,
        IReadOnlyList<string> arguments,
        IReadOnlyList<string> allowedExecutables)
    {
        if (string.IsNullOrWhiteSpace(fileName) || arguments is null || allowedExecutables is null)
        {
            throw Invalid("<invalid>");
        }

        var argumentSnapshot = arguments.ToArray();
        var allowlistSnapshot = allowedExecutables.ToArray();
        if (argumentSnapshot.Any(argument => argument is null) ||
            allowlistSnapshot.Any(executable => string.IsNullOrWhiteSpace(executable)))
        {
            throw Invalid("<invalid>");
        }

        var normalizedName = NormalizeBaseName(fileName);
        var commandParts = new[] { fileName }.Concat(argumentSnapshot);
        if (commandParts.Any(part => PowerShellTokens.Any(token => ContainsToken(part, token))))
        {
            throw new VerificationException(
                VerificationErrorCodes.NonAdminPowerShellForbidden,
                $"command={normalizedName}");
        }

        if (IsPathQualified(fileName) || allowlistSnapshot.Any(IsPathQualified))
        {
            throw Invalid(normalizedName);
        }

        var normalizedAllowlist = allowlistSnapshot.Select(NormalizeBaseName).ToArray();
        if (normalizedAllowlist.Length != CanonicalExecutables.Count ||
            normalizedAllowlist.Distinct(StringComparer.Ordinal).Count() != CanonicalExecutables.Count ||
            !normalizedAllowlist.ToFrozenSet(StringComparer.Ordinal).SetEquals(CanonicalExecutables) ||
            !CanonicalExecutables.Contains(normalizedName) ||
            commandParts.Any(part => ForbiddenTokens.Any(token => ContainsToken(part, token))))
        {
            throw Invalid(normalizedName);
        }
    }

    private static bool ContainsToken(string value, string token) =>
        string.Equals(value, token, StringComparison.OrdinalIgnoreCase) ||
        Regex.IsMatch(
            value,
            $@"(?<![A-Za-z0-9_]){Regex.Escape(token)}(?![A-Za-z0-9_])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string NormalizeBaseName(string value) =>
        value.Replace('\\', '/').Split('/')[^1].ToLowerInvariant();

    private static bool IsPathQualified(string value) =>
        Path.IsPathRooted(value) || value.IndexOfAny(['\\', '/']) >= 0;

    private static VerificationException Invalid(string normalizedName) =>
        new(VerificationErrorCodes.ConfigInvalid, $"process-command-forbidden={normalizedName}");
}

internal static class ProcessExecutableResolver
{
    internal static string Resolve(string catalogName) =>
        OperatingSystem.IsWindows() && string.Equals(catalogName, "npm", StringComparison.OrdinalIgnoreCase)
            ? "npm.cmd"
            : catalogName;
}

internal static class ProcessStartInfoFactory
{
    internal static ProcessStartInfo Create(ProcessInvocation invocation)
    {
        var snapshot = ProcessInvocationValidator.SnapshotAndValidate(invocation);
        ProcessCommandGuard.Validate(snapshot.FileName, snapshot.Arguments, snapshot.AllowedExecutables);
        var resolved = ProcessExecutableResolver.Resolve(snapshot.FileName);
        ProcessCommandGuard.Validate(resolved, snapshot.Arguments, snapshot.AllowedExecutables);

        string fileName = resolved;
        var arguments = snapshot.Arguments.ToList();
        if (OperatingSystem.IsWindows() && string.Equals(resolved, "npm.cmd", StringComparison.OrdinalIgnoreCase))
        {
            var npmLaunch = DiscoverWindowsNpm(snapshot.SuiteId);
            ProcessCommandGuard.Validate(Path.GetFileName(npmLaunch.NodePath), snapshot.Arguments, snapshot.AllowedExecutables);
            fileName = npmLaunch.NodePath;
            arguments.Insert(0, npmLaunch.NpmCliPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = snapshot.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        foreach (var key in startInfo.Environment.Keys.Where(IsSensitiveEnvironmentKey).ToArray())
        {
            startInfo.Environment.Remove(key);
        }

        return startInfo;
    }

    private static NpmLaunch DiscoverWindowsNpm(string suiteId)
    {
        var npmPath = FindOnPath("npm.cmd");
        if (npmPath is null)
        {
            throw DiscoveryFailed(suiteId);
        }

        var npmDirectory = Path.GetDirectoryName(npmPath)!;
        var adjacentNode = Path.Combine(npmDirectory, "node.exe");
        var npmCliPath = Path.Combine(npmDirectory, "node_modules", "npm", "bin", "npm-cli.js");
        if (!File.Exists(adjacentNode) || !File.Exists(npmCliPath))
        {
            throw DiscoveryFailed(suiteId);
        }

        return new NpmLaunch(Path.GetFullPath(adjacentNode), Path.GetFullPath(npmCliPath));
    }

    private static string? FindOnPath(string fileName)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (var entry in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var directory = entry.Trim().Trim('"');
            if (directory.Length == 0)
            {
                continue;
            }

            try
            {
                var candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
            {
                // Ignore malformed PATH entries without disclosing them.
            }
        }

        return null;
    }

    private static bool IsSensitiveEnvironmentKey(string key) =>
        Contains(key, "TOKEN") || Contains(key, "PASSWORD") || Contains(key, "SECRET") ||
        Contains(key, "CREDENTIAL") || Contains(key, "API_KEY") || Contains(key, "ACCESS_KEY") ||
        Contains(key, "PRIVATE_KEY") || Contains(key, "CONNECTION_STRING") ||
        Contains(key, "CONNECTIONSTRING") || Contains(key, "DATABASE_URL") ||
        Contains(key, "COOKIE") || Contains(key, "SESSION") || Contains(key, "BEARER") ||
        string.Equals(key, "PAT", StringComparison.OrdinalIgnoreCase) ||
        key.EndsWith("_PAT", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "AUTHORIZATION", StringComparison.OrdinalIgnoreCase);

    private static bool Contains(string value, string fragment) =>
        value.Contains(fragment, StringComparison.OrdinalIgnoreCase);

    private static VerificationException DiscoveryFailed(string suiteId) =>
        new(VerificationErrorCodes.ProcessFailed, $"suite={suiteId};reason=NpmDiscoveryFailed");

    private sealed record NpmLaunch(string NodePath, string NpmCliPath);
}

internal static class ProcessInvocationValidator
{
    private const int MaximumTimeoutSeconds = 3600;

    internal static ProcessInvocation SnapshotAndValidate(ProcessInvocation invocation)
    {
        if (invocation is null || invocation.Arguments is null || invocation.AllowedExecutables is null)
        {
            throw Invalid();
        }

        var arguments = invocation.Arguments.ToArray();
        var allowedExecutables = invocation.AllowedExecutables.ToArray();
        if (string.IsNullOrWhiteSpace(invocation.SuiteId) ||
            !Regex.IsMatch(invocation.SuiteId, @"^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$", RegexOptions.CultureInvariant) ||
            string.IsNullOrWhiteSpace(invocation.WorkingDirectory) ||
            !Path.IsPathFullyQualified(invocation.WorkingDirectory) ||
            invocation.Timeout <= TimeSpan.Zero ||
            invocation.Timeout > TimeSpan.FromSeconds(MaximumTimeoutSeconds) ||
            invocation.OutputLimitCharacters <= 0 ||
            arguments.Any(argument => argument is null) ||
            allowedExecutables.Any(executable => executable is null))
        {
            throw Invalid();
        }

        string canonicalWorkingDirectory;
        try
        {
            canonicalWorkingDirectory = Path.GetFullPath(invocation.WorkingDirectory);
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException or SecurityException)
        {
            throw Invalid();
        }

        return invocation with
        {
            Arguments = Array.AsReadOnly(arguments),
            AllowedExecutables = Array.AsReadOnly(allowedExecutables),
            WorkingDirectory = canonicalWorkingDirectory
        };
    }

    private static VerificationException Invalid() =>
        new(VerificationErrorCodes.ConfigInvalid, "process-invocation=invalid");
}

internal static class ProcessOutputSanitizer
{
    private const string Redacted = "[REDACTED]";
    private const string Truncated = "...[truncated]";
    private static readonly Regex AuthorizationBearerRegex = new(
        "(?i)(\\bauthorization\\s*[:=]\\s*bearer\\s+)(?:\"(?:\\\\.|[^\"])*\"|'(?:\\\\.|[^'])*'|[^\\s,;]+)",
        RegexOptions.CultureInvariant);
    private static readonly Regex CommandSecretRegex = new(
        "(?i)((?:^|\\s)--(?:token|password|secret)(?:\\s*=\\s*|\\s+))(?:\"(?:\\\\.|[^\"])*\"|'(?:\\\\.|[^'])*'|[^\\s,;]+)",
        RegexOptions.CultureInvariant);

    internal static string Sanitize(string text, string repositoryRoot, int maxCharacters)
    {
        if (maxCharacters <= 0)
        {
            throw new VerificationException(VerificationErrorCodes.ConfigInvalid, "output-limit=invalid");
        }

        var sanitized = RedactRepositoryRoot(text ?? string.Empty, repositoryRoot);
        sanitized = AuthorizationBearerRegex.Replace(sanitized, match => match.Groups[1].Value + Redacted);
        sanitized = CommandSecretRegex.Replace(sanitized, match => match.Groups[1].Value + Redacted);
        sanitized = RedactSensitiveJsonValues(sanitized);

        if (sanitized.Length <= maxCharacters)
        {
            return sanitized;
        }

        return maxCharacters <= Truncated.Length
            ? Truncated[..maxCharacters]
            : sanitized[..(maxCharacters - Truncated.Length)] + Truncated;
    }

    private static string RedactRepositoryRoot(string text, string repositoryRoot)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot))
        {
            return text;
        }

        var trimmed = repositoryRoot.TrimEnd('\\', '/');
        var rawVariants = new[] { trimmed, trimmed.Replace('\\', '/'), trimmed.Replace('/', '\\') };
        var variants = rawVariants
            .Concat(rawVariants.Select(value => JsonSerializer.Serialize(value)[1..^1]))
            .Where(value => value.Length > 0)
            .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
            .OrderByDescending(value => value.Length);
        var options = RegexOptions.CultureInvariant |
            (OperatingSystem.IsWindows() ? RegexOptions.IgnoreCase : RegexOptions.None);
        foreach (var variant in variants)
        {
            text = Regex.Replace(text, Regex.Escape(variant), "[REPO_ROOT]", options);
        }

        return text;
    }

    private static string RedactSensitiveJsonValues(string text)
    {
        var output = new StringBuilder(text.Length);
        var copyFrom = 0;
        var index = 0;
        while (index < text.Length)
        {
            if (text[index] != '"' || !TryFindJsonStringEnd(text, index, out var keyEnd))
            {
                index++;
                continue;
            }

            string? decodedKey;
            try
            {
                decodedKey = JsonSerializer.Deserialize<string>(text[index..(keyEnd + 1)]);
            }
            catch (JsonException)
            {
                index = keyEnd + 1;
                continue;
            }

            var colon = SkipWhitespace(text, keyEnd + 1);
            if (colon >= text.Length || text[colon] != ':' || !IsSensitiveJsonKey(decodedKey))
            {
                index = keyEnd + 1;
                continue;
            }

            var valueStart = SkipWhitespace(text, colon + 1);
            if (!TryFindJsonValueEnd(text, valueStart, out var valueEnd))
            {
                output.Append(text, copyFrom, valueStart - copyFrom);
                output.Append(Redacted);
                copyFrom = text.Length;
                index = text.Length;
                break;
            }

            output.Append(text, copyFrom, valueStart - copyFrom);
            output.Append(Redacted);
            copyFrom = valueEnd;
            index = valueEnd;
        }

        output.Append(text, copyFrom, text.Length - copyFrom);
        return output.ToString();
    }

    private static bool IsSensitiveJsonKey(string? key) =>
        key is not null && (key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("credential", StringComparison.OrdinalIgnoreCase));

    private static bool TryFindJsonValueEnd(string text, int start, out int end)
    {
        end = start;
        if (start >= text.Length)
        {
            return false;
        }

        if (text[start] == '"')
        {
            if (!TryFindJsonStringEnd(text, start, out var stringEnd))
            {
                return false;
            }

            end = stringEnd + 1;
            return true;
        }

        if (text[start] is '{' or '[')
        {
            var stack = new Stack<char>();
            stack.Push(text[start] == '{' ? '}' : ']');
            for (var index = start + 1; index < text.Length; index++)
            {
                if (text[index] == '"')
                {
                    if (!TryFindJsonStringEnd(text, index, out index))
                    {
                        return false;
                    }
                    continue;
                }

                if (text[index] is '{' or '[')
                {
                    stack.Push(text[index] == '{' ? '}' : ']');
                }
                else if (stack.Count > 0 && text[index] == stack.Peek())
                {
                    stack.Pop();
                    if (stack.Count == 0)
                    {
                        end = index + 1;
                        return true;
                    }
                }
            }

            return false;
        }

        while (end < text.Length && text[end] != ',' && text[end] != '}' && !char.IsWhiteSpace(text[end]))
        {
            end++;
        }
        return end > start;
    }

    private static bool TryFindJsonStringEnd(string text, int start, out int end)
    {
        var escaped = false;
        for (var index = start + 1; index < text.Length; index++)
        {
            if (!escaped && text[index] == '"')
            {
                end = index;
                return true;
            }

            if (!escaped && text[index] == '\\')
            {
                escaped = true;
            }
            else
            {
                escaped = false;
            }
        }

        end = text.Length;
        return false;
    }

    private static int SkipWhitespace(string text, int start)
    {
        while (start < text.Length && char.IsWhiteSpace(text[start]))
        {
            start++;
        }
        return start;
    }
}

internal sealed class SystemProcessRunner : IProcessRunner
{
    private const int MaximumRetainedCharacters = 65_536;
    private const int MaximumOutputCharacters = 8_192;
    private static readonly TimeSpan CleanupTimeout = TimeSpan.FromSeconds(5);

    public async Task<ProcessExecutionResult> RunAsync(
        ProcessInvocation invocation,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result(null, 0, false, true, string.Empty, string.Empty);
        }

        var snapshot = ProcessInvocationValidator.SnapshotAndValidate(invocation);
        using var process = new Process { StartInfo = ProcessStartInfoFactory.Create(snapshot) };
        var stopwatch = new Stopwatch();
        var started = false;
        var terminationAttempted = false;

        try
        {
            stopwatch.Start();
            started = process.Start();
            if (!started)
            {
                throw new InvalidOperationException();
            }

            var outputTask = DrainBoundedAsync(process.StandardOutput);
            var errorTask = DrainBoundedAsync(process.StandardError);
            var timedOut = false;
            var cancelled = false;
            using var timeout = new CancellationTokenSource(snapshot.Timeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
            try
            {
                await process.WaitForExitAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                cancelled = cancellationToken.IsCancellationRequested;
                timedOut = !cancelled;
                terminationAttempted = true;
                await TerminateAsync(process, snapshot.SuiteId).ConfigureAwait(false);
            }

            await AwaitDrainsAsync(outputTask, errorTask, snapshot.SuiteId).ConfigureAwait(false);
            stopwatch.Stop();
            int? exitCode = timedOut || cancelled ? null : process.ExitCode;
            return Result(
                exitCode,
                stopwatch.ElapsedMilliseconds,
                timedOut,
                cancelled,
                outputTask.Result,
                errorTask.Result,
                snapshot);
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (Exception exception) when (IsExpectedProcessFailure(exception))
        {
            throw Failure(snapshot.SuiteId, exception.GetType().Name);
        }
        finally
        {
            if (started && !terminationAttempted && !HasExited(process))
            {
                terminationAttempted = true;
                await TerminateAsync(process, snapshot.SuiteId).ConfigureAwait(false);
            }
        }
    }

    private static async Task TerminateAsync(Process process, string suiteId)
    {
        if (HasExited(process))
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
        {
            if (!HasExited(process))
            {
                throw Failure(suiteId, "TerminationFailed");
            }
            return;
        }

        using var cleanup = new CancellationTokenSource(CleanupTimeout);
        try
        {
            await process.WaitForExitAsync(cleanup.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw Failure(suiteId, "TerminationTimeout");
        }

        if (!HasExited(process))
        {
            throw Failure(suiteId, "TerminationFailed");
        }
    }

    private static async Task AwaitDrainsAsync(Task<string> output, Task<string> error, string suiteId)
    {
        try
        {
            await Task.WhenAll(output, error).WaitAsync(CleanupTimeout).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            throw Failure(suiteId, "DrainTimeout");
        }
    }

    private static bool HasExited(Process process)
    {
        try
        {
            return process.HasExited;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static async Task<string> DrainBoundedAsync(StreamReader reader)
    {
        var retained = new StringBuilder(MaximumRetainedCharacters);
        var buffer = new char[4_096];
        int read;
        while ((read = await reader.ReadAsync(buffer.AsMemory()).ConfigureAwait(false)) > 0)
        {
            var retain = Math.Min(read, MaximumRetainedCharacters - retained.Length);
            if (retain > 0)
            {
                retained.Append(buffer, 0, retain);
            }
        }
        return retained.ToString();
    }

    private static ProcessExecutionResult Result(
        int? exitCode,
        long durationMs,
        bool timedOut,
        bool cancelled,
        string standardOutput,
        string standardError,
        ProcessInvocation? invocation = null)
    {
        var root = invocation?.WorkingDirectory ?? string.Empty;
        var limit = Math.Min(invocation?.OutputLimitCharacters ?? MaximumOutputCharacters, MaximumOutputCharacters);
        var output = ProcessOutputSanitizer.Sanitize(standardOutput, root, limit);
        var error = ProcessOutputSanitizer.Sanitize(standardError, root, limit);
        var hash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(output + "\n" + error))).ToLowerInvariant();
        return new ProcessExecutionResult(exitCode, durationMs, timedOut, cancelled, output, error, hash);
    }

    private static VerificationException Failure(string suiteId, string reason) =>
        new(VerificationErrorCodes.ProcessFailed, $"suite={suiteId};reason={reason}");

    private static bool IsExpectedProcessFailure(Exception exception) =>
        exception is Win32Exception or InvalidOperationException or IOException or UnauthorizedAccessException or SecurityException;
}
