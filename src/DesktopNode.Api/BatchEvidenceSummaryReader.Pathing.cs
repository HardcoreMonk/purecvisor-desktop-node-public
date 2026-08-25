using System.Text.RegularExpressions;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

public sealed partial class BatchEvidenceSummaryReader
{
    private JsonElement? TryReadArtifactSummary(string path)
    {
        if (!IsReadableEvidencePath(path))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(fileAccess.ReadAllText(path));
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private IEnumerable<string> EnumerateSummaryFiles()
    {
        if (root is null || !IsUsableEvidenceDirectory(root))
        {
            yield break;
        }

        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            if (!IsUsableEvidenceDirectory(directory))
            {
                continue;
            }

            string[] files;
            try
            {
                files = fileAccess.GetFiles(directory, "summary.json");
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var file in files)
            {
                if (IsReadableEvidencePath(file))
                {
                    yield return file;
                }
            }

            string[] childDirectories;
            try
            {
                childDirectories = fileAccess.GetDirectories(directory);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in childDirectories)
            {
                if (IsUsableEvidenceDirectory(childDirectory))
                {
                    pending.Push(childDirectory);
                }
            }
        }
    }

    private string ToArtifactEvidencePath(string path)
    {
        if (root is null)
        {
            return Redact(path).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }

        var fullPath = Path.GetFullPath(path);
        var relativePath = Path.GetRelativePath(root, fullPath);
        if (relativePath.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relativePath))
        {
            return Redact(fullPath).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
        }

        return "artifacts/" + relativePath.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static string? InferDescriptorBatchId(string path)
    {
        var directory = Path.GetDirectoryName(path);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            var name = Path.GetFileName(directory);
            if (name.StartsWith("manual-admin-campaign-descriptor-", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }

            directory = Path.GetDirectoryName(directory);
        }

        return null;
    }

    private static string? InferDescriptorBatchIdFromCampaignPath(string path)
    {
        var directory = Path.GetDirectoryName(path);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            var name = Path.GetFileName(directory);
            if (name.StartsWith("manual-admin-campaign-", StringComparison.OrdinalIgnoreCase))
            {
                return $"manual-admin-campaign-descriptor-{name["manual-admin-campaign-".Length..]}-closed";
            }

            directory = Path.GetDirectoryName(directory);
        }

        return null;
    }

    private string? ResolveChildArtifactRoot(JsonElement batch, string stepId)
    {
        if (!batch.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var result in results.EnumerateArray())
        {
            if (!string.Equals(ReadString(result, "step_id"), stepId, StringComparison.Ordinal))
            {
                continue;
            }

            var fromArguments = ResolveArtifactRootFromArguments(result);
            if (!string.IsNullOrWhiteSpace(fromArguments))
            {
                return fromArguments;
            }

            var fromStdout = ResolveArtifactRootFromStdout(ReadString(result, "stdout"));
            if (!string.IsNullOrWhiteSpace(fromStdout))
            {
                return fromStdout;
            }
        }

        return null;
    }

    private string? ResolveArtifactRootFromArguments(JsonElement result)
    {
        if (!result.TryGetProperty("arguments", out var arguments) || arguments.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var values = arguments.EnumerateArray()
            .Where(argument => argument.ValueKind == JsonValueKind.String)
            .Select(argument => argument.GetString() ?? string.Empty)
            .ToArray();

        for (var index = 0; index < values.Length - 1; index++)
        {
            if (string.Equals(values[index], "-ArtifactRoot", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(values[index], "--artifact-root", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeChildRoot(values[index + 1]);
            }
        }

        return null;
    }

    private string? ResolveArtifactRootFromStdout(string? stdout)
    {
        if (string.IsNullOrWhiteSpace(stdout))
        {
            return null;
        }

        foreach (var line in stdout.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            const string key = "ArtifactRoot=";
            var index = line.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return NormalizeChildRoot(line[(index + key.Length)..].Trim().Trim('"'));
            }
        }

        return null;
    }

    private string? NormalizeChildRoot(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || root is null)
        {
            return null;
        }

        var fullPath = Path.GetFullPath(ResolveRepoRootRedactedPath(path) ?? path);
        var rootWithSeparator = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
            IsUsableEvidenceDirectory(fullPath)
            ? fullPath
            : null;
    }

    private JsonElement WithError(string status, string code, string message, string detail)
    {
        return JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["configured"] = true,
            ["status"] = status,
            ["artifact_root"] = Redact(root),
            ["latest"] = null,
            ["errors"] = new[]
            {
                new SortedDictionary<string, object?>
                {
                    ["code"] = code,
                    ["message"] = message,
                    ["detail"] = detail,
                    ["retryable"] = false
                }
            }
        });
    }

    private string? ResolveLatestRunRoot(string root)
    {
        var directSummary = Path.Combine(root, "summary.json");
        if (fileAccess.FileExists(directSummary))
        {
            return root;
        }

        foreach (var candidate in EnumerateRunRootCandidates(root)
            .Where(directory => IsUsableEvidenceDirectory(directory) && fileAccess.FileExists(Path.Combine(directory, "summary.json")))
            .OrderByDescending(GetEvidenceSummarySortTime)
            .ToArray())
        {
            if (IsBatchSupervisorRunRootOrMalformed(candidate) && !IsManualAdminCampaignDescriptorRun(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private IEnumerable<string> EnumerateRunRootCandidates(string root)
    {
        foreach (var directory in fileAccess.GetDirectories(root))
        {
            yield return directory;
        }

        var batchRunsRoot = Path.Combine(root, "batch-runs");
        if (!IsUsableEvidenceDirectory(batchRunsRoot))
        {
            yield break;
        }

        foreach (var directory in fileAccess.GetDirectories(batchRunsRoot))
        {
            yield return directory;
        }
    }

    internal DateTime GetEvidenceSummarySortTime(string directory)
    {
        var summaryPath = Path.Combine(directory, "summary.json");
        return IsReadableEvidencePath(summaryPath)
            ? fileAccess.GetLastWriteTimeUtc(summaryPath)
            : DateTime.MinValue;
    }

    private bool IsBatchSupervisorRunRootOrMalformed(string directory)
    {
        var summaryPath = Path.Combine(directory, "summary.json");
        if (!IsReadableEvidencePath(summaryPath))
        {
            return true;
        }

        try
        {
            using var document = JsonDocument.Parse(fileAccess.ReadAllText(summaryPath));
            var summary = document.RootElement;
            if (summary.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var hasBatchSupervisorShape = summary.TryGetProperty("results", out _) ||
                summary.TryGetProperty("total_steps", out _) ||
                summary.TryGetProperty("executed_steps", out _);

            return summary.TryGetProperty("batch_id", out _) && hasBatchSupervisorShape;
        }
        catch (JsonException) when (IsKnownChildArtifactRootName(directory))
        {
            return false;
        }
    }

    private bool IsManualAdminCampaignDescriptorRun(string directory)
    {
        var summaryPath = Path.Combine(directory, "summary.json");
        if (!IsReadableEvidencePath(summaryPath))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(fileAccess.ReadAllText(summaryPath));
            var summary = document.RootElement;
            var batchId = ReadString(summary, "batch_id");
            if (batchId is not null &&
                batchId.StartsWith("manual-admin-campaign-descriptor-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!summary.TryGetProperty("results", out var results))
            {
                return false;
            }

            if (results.ValueKind == JsonValueKind.Object)
            {
                return string.Equals(
                    ReadString(results, "step_id"),
                    "manual-admin-campaign-descriptor",
                    StringComparison.Ordinal);
            }

            return results.ValueKind == JsonValueKind.Array &&
                results.EnumerateArray().Any(result => string.Equals(
                    ReadString(result, "step_id"),
                    "manual-admin-campaign-descriptor",
                    StringComparison.Ordinal));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsKnownChildArtifactRootName(string directory)
    {
        var name = Path.GetFileName(directory);
        return name.StartsWith("routeparity-service-msi-hyperv", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("os-mutation-gates", StringComparison.OrdinalIgnoreCase);
    }

    private EvidenceJsonReadResult ReadChildJson(
        string? directory,
        string fileName,
        string missingCode,
        string parseCode,
        string description)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return ChildMissing(missingCode, $"{description} evidence path was not discovered.");
        }

        return ReadChildJsonPath(Path.Combine(directory, fileName), missingCode, parseCode, description);
    }

    private EvidenceJsonReadResult ReadFirstChildJson(
        string? directory,
        string pattern,
        string missingCode,
        string parseCode,
        string description)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return ChildMissing(missingCode, $"{description} evidence path was not discovered.");
        }

        if (!IsUsableEvidenceDirectory(directory))
        {
            return ChildUnavailable(
                parseCode,
                $"{description} evidence directory was rejected.",
                $"{description} evidence directory was rejected at {Redact(directory)}.");
        }

        var path = fileAccess.GetFiles(directory, pattern)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        return path is null
            ? ChildMissing(missingCode, $"{description} evidence file was not found.")
            : ReadChildJsonPath(path, missingCode, parseCode, description);
    }

    private EvidenceJsonReadResult ReadChildJsonPath(
        string path,
        string missingCode,
        string parseCode,
        string description)
    {
        var fullPath = Path.GetFullPath(path);
        if (!fileAccess.FileExists(fullPath))
        {
            return ChildMissing(
                missingCode,
                $"{description} evidence is missing.",
                $"{description} evidence is missing at {Redact(fullPath)}.");
        }

        if (!IsReadableEvidencePath(fullPath))
        {
            return ChildUnavailable(
                parseCode,
                $"{description} evidence path was rejected.",
                $"{description} evidence path was rejected at {Redact(fullPath)}.");
        }

        try
        {
            using var document = JsonDocument.Parse(fileAccess.ReadAllText(fullPath));
            return new EvidenceJsonReadResult(document.RootElement.Clone(), StatusAvailable, null);
        }
        catch (JsonException error)
        {
            return ChildUnavailable(
                parseCode,
                $"{description} evidence JSON could not be parsed.",
                Redact(error.Message));
        }
        catch (IOException error)
        {
            return ChildUnavailable(
                parseCode,
                $"{description} evidence could not be read.",
                Redact(error.Message));
        }
        catch (UnauthorizedAccessException error)
        {
            return ChildUnavailable(
                parseCode,
                $"{description} evidence could not be read.",
                Redact(error.Message));
        }
    }

    private static EvidenceJsonReadResult ChildMissing(string code, string message, string? detail = null)
    {
        return new EvidenceJsonReadResult(
            null,
            StatusMissing,
            new BatchEvidenceIssue(code, message, detail ?? message));
    }

    private static EvidenceJsonReadResult ChildUnavailable(string code, string message, string detail)
    {
        return new EvidenceJsonReadResult(
            null,
            StatusUnavailable,
            new BatchEvidenceIssue(code, message, detail));
    }

    private static void AddIssue(List<BatchEvidenceIssue> issues, EvidenceJsonReadResult result)
    {
        if (result.Issue is not null)
        {
            issues.Add(result.Issue);
        }
    }

    private object[] BuildIssueObjects(IReadOnlyList<BatchEvidenceIssue> issues)
    {
        return issues
            .Select(issue => new SortedDictionary<string, object?>
            {
                ["code"] = issue.Code,
                ["message"] = issue.Message,
                ["detail"] = Redact(issue.Detail),
                ["retryable"] = false
            })
            .ToArray();
    }

    private bool IsReadableEvidencePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(path);
        return IsUnderConfiguredRoot(fullPath) &&
            fileAccess.FileExists(fullPath) &&
            IsPathWithinConfiguredRootWithoutReparsePoints(root!, fullPath, fileAccess.GetAttributes);
    }

    private bool IsUsableEvidenceDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var fullPath = Path.GetFullPath(path);
        return IsUnderConfiguredRoot(fullPath) &&
            fileAccess.DirectoryExists(fullPath) &&
            IsPathWithinConfiguredRootWithoutReparsePoints(root!, fullPath, fileAccess.GetAttributes);
    }

    private bool IsUnderConfiguredRoot(string path)
    {
        if (root is null)
        {
            return false;
        }

        var rootWithSeparator = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, root, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsPathWithinConfiguredRootWithoutReparsePoints(
        string configuredRoot,
        string path,
        Func<string, FileAttributes> getAttributes)
    {
        var rootFullPath = Path.GetFullPath(configuredRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var rootWithSeparator = rootFullPath + Path.DirectorySeparatorChar;
        if (!string.Equals(fullPath, rootFullPath, StringComparison.OrdinalIgnoreCase) &&
            !fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var current = rootFullPath;
        while (true)
        {
            if (HasReparsePoint(current, getAttributes))
            {
                return false;
            }

            if (string.Equals(current, fullPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var relative = Path.GetRelativePath(current, fullPath);
            var nextSegment = relative.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(nextSegment))
            {
                return true;
            }

            current = Path.Combine(current, nextSegment);
        }
    }

    private static bool HasReparsePoint(string path, Func<string, FileAttributes> getAttributes)
    {
        try
        {
            return (getAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return true;
        }
    }

    private string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var redacted = value;
        if (!string.IsNullOrWhiteSpace(root))
        {
            redacted = redacted.Replace(root, "[BATCH_EVIDENCE_ROOT]", StringComparison.OrdinalIgnoreCase);
        }

        var repoRoot = FindRepoRootFromConfiguredEvidenceRoot() ?? FindRepoRoot();
        if (!string.IsNullOrWhiteSpace(repoRoot))
        {
            redacted = redacted.Replace(repoRoot, "[REPO_ROOT]", StringComparison.OrdinalIgnoreCase);
        }

        redacted = Regex.Replace(
            redacted,
            @"(?i)\bBearer\s+[A-Za-z0-9._~+/=-]+",
            "Bearer [REDACTED_TOKEN]");
        redacted = Regex.Replace(
            redacted,
            @"(?i)\b(?:api[_-]?token|pcv_api_token|apiToken|ApiTokenProtectedFile|token[_-]?file)\b\s*[:=]\s*[^,\s;]+",
            "[REDACTED_TOKEN]");
        redacted = Regex.Replace(
            redacted,
            @"(?i)--?(?:api-token|api-token-protected-file)\s+[^,\s;]+",
            "[REDACTED_TOKEN]");

        return redacted;
    }

    private string? ResolveRepoRootRedactedPath(string path)
    {
        const string token = "[REPO_ROOT]";
        if (!path.StartsWith(token, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var repoRoot = FindRepoRootFromConfiguredEvidenceRoot() ?? FindRepoRoot();
        if (string.IsNullOrWhiteSpace(repoRoot))
        {
            return null;
        }

        var remainder = path[token.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.Combine(repoRoot, remainder);
    }

    private string? FindRepoRootFromConfiguredEvidenceRoot()
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            return null;
        }

        var directory = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (string.Equals(Path.GetFileName(directory), "artifacts", StringComparison.OrdinalIgnoreCase))
            {
                return Directory.GetParent(directory)?.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }

    private string? FindRepoRoot()
    {
        var directory = AppContext.BaseDirectory;
        string? solutionDirectoryFallback = null;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (fileAccess.FileExists(Path.Combine(directory, "src", "DesktopNode.sln")) ||
                fileAccess.DirectoryExists(Path.Combine(directory, ".git")))
            {
                return directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            if (solutionDirectoryFallback is null && fileAccess.FileExists(Path.Combine(directory, "DesktopNode.sln")))
            {
                solutionDirectoryFallback = directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        return solutionDirectoryFallback?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

}
