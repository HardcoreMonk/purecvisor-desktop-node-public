using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

internal sealed record CutoverGitBoundaryResult(
    string ShadowSha,
    string CutoverSha,
    IReadOnlyList<string> ChangedPaths);

internal sealed partial class CutoverGitBoundary(IProcessRunner processRunner)
{
    internal static IReadOnlyList<string> AllowedCutoverPaths { get; } = Array.AsReadOnly([
        ".github/workflows/development-gates.yml",
        "config/development-verification-suites.json",
        "config/development-verification-suites.schema.json",
        "config/development-verification-migration-manifest.json",
        "config/development-verification-migration-manifest.schema.json",
        "src/DesktopNode.Delivery.Tests/Delivery/Verification/PcvDevelopmentGateWorkflowContractTests.cs",
        "docs/DEVELOPMENT_VERIFICATION_POLICY.md",
        "docs/ga-ready/EVIDENCE_INDEX.md",
        "docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md"
    ]);

    private static readonly IReadOnlyList<string> AllowedExecutables = Array.AsReadOnly([
        "dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"
    ]);
    private static readonly HashSet<string> AllowedPathSet = new(AllowedCutoverPaths, StringComparer.Ordinal);

    internal async Task<CutoverGitBoundaryResult> ValidateAsync(
        string repositoryRoot,
        string verificationHead,
        string shadowSha,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsSha(verificationHead))
        {
            throw Invalid("cutover-head=invalid");
        }

        if (!IsSha(shadowSha))
        {
            throw Invalid("cutover-shadow=invalid");
        }

        var root = CanonicalRoot(repositoryRoot);
        var head = verificationHead.ToLowerInvariant();
        var shadow = shadowSha.ToLowerInvariant();

        var resolvedHead = await RunRequiredAsync(
            root,
            "head",
            ["rev-parse", "--verify", $"{head}^{{commit}}"],
            cancellationToken);
        if (!string.Equals(SingleSha(resolvedHead, "cutover-head=missing"), head, StringComparison.Ordinal))
        {
            throw Invalid("cutover-head=mismatch");
        }

        var status = await RunRequiredAsync(
            root,
            "status",
            ["status", "--porcelain=v1"],
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(status))
        {
            throw Invalid("cutover-worktree=dirty");
        }

        var shadowResult = await RunAsync(
            root,
            "shadow",
            ["rev-parse", "--verify", $"{shadow}^{{commit}}"],
            cancellationToken);
        if (shadowResult.ExitCode != 0 ||
            !string.Equals(SingleShaOrNull(shadowResult.StandardOutput), shadow, StringComparison.Ordinal))
        {
            throw Invalid("cutover-history=shadow-missing");
        }

        var ancestor = await RunAsync(
            root,
            "shadow-ancestor",
            ["merge-base", "--is-ancestor", shadow, head],
            cancellationToken);
        if (ancestor.ExitCode != 0)
        {
            throw Invalid("cutover-history=shadow-not-ancestor");
        }

        var history = await RunRequiredAsync(
            root,
            "history",
            ["rev-list", "--parents", $"{shadow}..{head}"],
            cancellationToken);
        var commits = ParseHistory(history);
        var directChildren = commits
            .Where(commit => commit.Parents.Contains(shadow, StringComparer.Ordinal))
            .ToArray();
        if (directChildren.Length != 1)
        {
            throw Invalid("cutover-history=direct-child-count");
        }

        var cutover = directChildren[0];
        if (cutover.Parents.Length != 1 || !string.Equals(cutover.Parents[0], shadow, StringComparison.Ordinal))
        {
            throw Invalid("cutover-history=cutover-merge");
        }

        var cutoverAncestor = await RunAsync(
            root,
            "cutover-ancestor",
            ["merge-base", "--is-ancestor", cutover.Sha, head],
            cancellationToken);
        if (cutoverAncestor.ExitCode != 0)
        {
            throw Invalid("cutover-history=cutover-not-ancestor");
        }

        var range = $"{shadow}..{cutover.Sha}";
        var primaryDiff = await RunRequiredAsync(
            root,
            "diff",
            ["-c", "core.quotepath=false", "diff", "--name-status", "--no-renames", range],
            cancellationToken);
        var changes = ParsePrimaryDiff(primaryDiff);

        var detectedDiff = await RunRequiredAsync(
            root,
            "rename-copy",
            [
                "-c", "core.quotepath=false", "diff", "--name-status",
                "--find-renames=50%", "--find-copies=50%", "--find-copies-harder", range
            ],
            cancellationToken);
        if (HasRenameOrCopy(detectedDiff))
        {
            throw Invalid("cutover-diff=rename-copy");
        }

        var rawDiff = await RunRequiredAsync(
            root,
            "raw-diff",
            ["diff", "--raw", "--no-abbrev", "--no-renames", range],
            cancellationToken);
        if (HasGitlink(rawDiff))
        {
            throw Invalid("cutover-diff=gitlink");
        }

        if (changes.Length == 0 || changes.Any(change => change.Status == "D"))
        {
            throw Invalid("cutover-diff=status");
        }

        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var change in changes)
        {
            ValidateRelativePath(change.Path);
            if (!AllowedPathSet.Contains(change.Path) || !paths.Add(change.Path))
            {
                throw Invalid("cutover-diff=path");
            }
        }

        return new CutoverGitBoundaryResult(
            shadow,
            cutover.Sha,
            Array.AsReadOnly(paths.Order(StringComparer.Ordinal).ToArray()));
    }

    internal async Task<string> ResolveHeadAsync(
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        var root = CanonicalRoot(repositoryRoot);
        var output = await RunRequiredAsync(
            root,
            "resolve-head",
            ["rev-parse", "--verify", "HEAD^{commit}"],
            cancellationToken);
        return SingleSha(output, "cutover-head=missing");
    }

    private async Task<string> RunRequiredAsync(
        string root,
        string operation,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var result = await RunAsync(root, operation, arguments, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw Invalid($"cutover-git:{operation}=failed");
        }

        return result.StandardOutput;
    }

    private async Task<ProcessExecutionResult> RunAsync(
        string root,
        string operation,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var invocation = new ProcessInvocation(
            $"cutover-git-{operation}",
            "git",
            Array.AsReadOnly(arguments.ToArray()),
            root,
            TimeSpan.FromSeconds(30),
            AllowedExecutables);
        ProcessExecutionResult result;
        try
        {
            result = await processRunner.RunAsync(invocation, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw Invalid($"cutover-git:{operation}=failed");
        }

        if (result.Cancelled)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw Invalid($"cutover-git:{operation}=cancelled");
        }

        if (result.TimedOut)
        {
            throw Invalid($"cutover-git:{operation}=timeout");
        }

        return result;
    }

    private static string CanonicalRoot(string repositoryRoot)
    {
        try
        {
            var root = Path.GetFullPath(repositoryRoot);
            if (!Path.IsPathFullyQualified(root) || !Directory.Exists(root))
            {
                throw Invalid("cutover-root=invalid");
            }

            return root;
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (Exception exception) when (!IsFatal(exception))
        {
            throw Invalid("cutover-root=invalid");
        }
    }

    private static CommitRow[] ParseHistory(string output)
    {
        var rows = new List<CommitRow>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in Lines(output))
        {
            var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length == 0 || fields.Any(field => !IsSha(field)))
            {
                throw Invalid("cutover-history=invalid-output");
            }

            var sha = fields[0].ToLowerInvariant();
            if (!seen.Add(sha))
            {
                throw Invalid("cutover-history=duplicate-output");
            }

            rows.Add(new CommitRow(
                sha,
                fields.Skip(1).Select(parent => parent.ToLowerInvariant()).ToArray()));
        }

        return rows.ToArray();
    }

    private static DiffRow[] ParsePrimaryDiff(string output)
    {
        var rows = new List<DiffRow>();
        foreach (var line in Lines(output))
        {
            var separator = line.IndexOf('\t');
            if (separator <= 0 || separator == line.Length - 1)
            {
                throw Invalid("cutover-diff=invalid-output");
            }

            var status = line[..separator];
            var path = line[(separator + 1)..];
            if (status is not ("A" or "M" or "D" or "T"))
            {
                throw Invalid("cutover-diff=status");
            }

            ValidateRelativePath(path);
            rows.Add(new DiffRow(status, path));
        }

        return rows.ToArray();
    }

    private static bool HasRenameOrCopy(string output)
    {
        foreach (var line in Lines(output))
        {
            var separator = line.IndexOf('\t');
            if (separator <= 0)
            {
                throw Invalid("cutover-diff=invalid-output");
            }

            var status = line[..separator];
            if (status.StartsWith('R') || status.StartsWith('C'))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasGitlink(string output)
    {
        foreach (var line in Lines(output))
        {
            if (!line.StartsWith(':') || line.IndexOf('\t') < 0)
            {
                throw Invalid("cutover-diff=invalid-raw-output");
            }

            var metadata = line[1..line.IndexOf('\t')].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (metadata.Length < 5)
            {
                throw Invalid("cutover-diff=invalid-raw-output");
            }

            if (metadata[0] == "160000" || metadata[1] == "160000")
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            Path.IsPathRooted(path) ||
            WindowsAbsolutePathRegex().IsMatch(path) ||
            path.StartsWith('/') ||
            path.Contains('\\') ||
            path.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw Invalid("cutover-diff=absolute-path");
        }
    }

    private static string SingleSha(string output, string detail) =>
        SingleShaOrNull(output) ?? throw Invalid(detail);

    private static string? SingleShaOrNull(string output)
    {
        var values = Lines(output).ToArray();
        return values.Length == 1 && IsSha(values[0]) ? values[0].ToLowerInvariant() : null;
    }

    private static IEnumerable<string> Lines(string output) =>
        output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool IsSha(string value) => ShaRegex().IsMatch(value);

    private static bool IsFatal(Exception exception) =>
        exception is OutOfMemoryException or StackOverflowException or AccessViolationException or
            AppDomainUnloadedException;

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);

    private sealed record CommitRow(string Sha, string[] Parents);
    private sealed record DiffRow(string Status, string Path);

    [GeneratedRegex("^[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant)]
    private static partial Regex ShaRegex();

    [GeneratedRegex("^[A-Za-z]:[/\\\\]", RegexOptions.CultureInvariant)]
    private static partial Regex WindowsAbsolutePathRegex();
}
