namespace DesktopNode.Verification;

internal static class RepositoryLocator
{
    internal static string Find(string startDirectory)
    {
        try
        {
            var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "src", "DesktopNode.sln")) &&
                    File.Exists(Path.Combine(current.FullName, "config", "development-verification-suites.json")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }
        catch (Exception exception) when (IsExpectedPathException(exception))
        {
            throw NotFound();
        }

        throw NotFound();
    }

    private static bool IsExpectedPathException(Exception exception) =>
        exception is ArgumentException or NotSupportedException or PathTooLongException or IOException or UnauthorizedAccessException;

    private static VerificationException NotFound() =>
        new(VerificationErrorCodes.ConfigInvalid, "repository-root-not-found");
}

internal static class ArtifactRootPolicy
{
    internal static string ResolveAndValidate(
        string repositoryRoot,
        string requestedRoot,
        string? runnerTemp,
        string? userProfile)
    {
        if (string.IsNullOrWhiteSpace(requestedRoot) ||
            requestedRoot.Contains('%', StringComparison.Ordinal) ||
            requestedRoot.Contains('$', StringComparison.Ordinal) ||
            requestedRoot.StartsWith('~'))
        {
            throw Invalid("unresolved");
        }

        try
        {
            var repository = Path.GetFullPath(repositoryRoot);
            var candidate = Path.GetFullPath(requestedRoot, repository);
            var filesystemRoot = Path.GetPathRoot(candidate);
            var resolvedRunnerTemp = ResolveRunnerTemp(runnerTemp, repository);

            if (resolvedRunnerTemp is not null && PathsEqual(candidate, resolvedRunnerTemp))
            {
                throw Invalid("runner-temp-equal");
            }

            if (filesystemRoot is not null && PathsEqual(candidate, filesystemRoot))
            {
                throw Invalid("broad-root");
            }

            if (PathsEqual(candidate, repository))
            {
                throw Invalid("repository-root");
            }

            var artifacts = Path.Combine(repository, "artifacts");
            var withinArtifacts = IsStrictDescendant(candidate, artifacts);
            var withinRepository = IsStrictDescendant(candidate, repository);
            var withinRunnerTemp = resolvedRunnerTemp is not null &&
                IsStrictDescendant(candidate, resolvedRunnerTemp);

            if (!string.IsNullOrWhiteSpace(userProfile))
            {
                var profile = Path.GetFullPath(userProfile);
                var allowedProfileContainedRunnerTemp = withinRunnerTemp &&
                    resolvedRunnerTemp is not null &&
                    !PathsEqual(resolvedRunnerTemp, profile);
                if (PathsEqual(candidate, profile) ||
                    (IsStrictDescendant(candidate, profile) &&
                     !withinArtifacts &&
                     !allowedProfileContainedRunnerTemp))
                {
                    throw Invalid("user-profile");
                }
            }

            if (withinRepository)
            {
                if (!withinArtifacts)
                {
                    throw Invalid("outside-boundary");
                }

                return candidate;
            }

            if (!withinArtifacts && !withinRunnerTemp)
            {
                throw Invalid("outside-boundary");
            }

            return candidate;
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (Exception exception) when (IsExpectedPathException(exception))
        {
            throw Invalid("path");
        }
    }

    private static string? ResolveRunnerTemp(string? runnerTemp, string repository)
    {
        if (string.IsNullOrWhiteSpace(runnerTemp))
        {
            return null;
        }

        if (!Path.IsPathFullyQualified(runnerTemp))
        {
            throw Invalid("runner-temp");
        }

        var resolved = Path.GetFullPath(runnerTemp);
        var filesystemRoot = Path.GetPathRoot(resolved);
        if ((filesystemRoot is not null && PathsEqual(resolved, filesystemRoot)) ||
            PathsEqual(resolved, repository))
        {
            throw Invalid("runner-temp");
        }

        return resolved;
    }

    private static bool IsStrictDescendant(string candidate, string parent)
    {
        var relative = Path.GetRelativePath(Path.GetFullPath(parent), Path.GetFullPath(candidate));
        return relative != "." &&
            !Path.IsPathRooted(relative) &&
            relative != ".." &&
            !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);
    }

    private static bool PathsEqual(string left, string right) =>
        string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static bool IsExpectedPathException(Exception exception) =>
        exception is ArgumentException or NotSupportedException or PathTooLongException or IOException or UnauthorizedAccessException;

    private static VerificationException Invalid(string reason) =>
        new(VerificationErrorCodes.ArtifactRootInvalid, $"artifact-root-invalid:{reason}");
}
