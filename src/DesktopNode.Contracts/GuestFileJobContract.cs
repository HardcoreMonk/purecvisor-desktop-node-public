namespace DesktopNode.Contracts;

public static class GuestFileJobProblemCodes
{
    public const string PathNotAllowed = "PCV_GUEST_FILE_PATH_NOT_ALLOWED";
    public const string SizeLimit = "PCV_GUEST_FILE_SIZE_LIMIT";
    public const string DirectionInvalid = "PCV_GUEST_FILE_DIRECTION_INVALID";
    public const string CredentialRefRequired = "PCV_GUEST_FILE_CREDENTIAL_REF_REQUIRED";
    public const string HgfsForbidden = "PCV_GUEST_FILE_HGFS_FORBIDDEN";
}

public sealed record GuestFileJobRequest(
    string? Direction,
    string? HostPath,
    string? GuestPath,
    long? SizeBytes,
    string? CredentialRef,
    string? SharedFolder = null);

public sealed record GuestFileJobEvaluation(
    bool Ok,
    string? ErrorCode,
    string? NormalizedHostPath,
    string? NormalizedGuestPath,
    long SizeBytes,
    string Direction);

public static class GuestFileJobContract
{
    public const string DirectionHostToGuest = "host-to-guest";
    public const long MaxSizeBytes = 64L * 1024L * 1024L;
    public const string GuestPrefix = @"C:\Users\Public\PureCVisor\";

    public static string HostRoot => Path.GetFullPath(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PureCVisor",
            "desktop-node",
            "guest-files"));

    public static GuestFileJobEvaluation Evaluate(GuestFileJobRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SharedFolder) ||
            ContainsHgfsToken(request.HostPath) ||
            ContainsHgfsToken(request.GuestPath))
        {
            return Reject(GuestFileJobProblemCodes.HgfsForbidden);
        }

        var direction = (request.Direction ?? string.Empty).Trim();
        if (!string.Equals(direction, DirectionHostToGuest, StringComparison.OrdinalIgnoreCase))
        {
            return Reject(GuestFileJobProblemCodes.DirectionInvalid);
        }

        var credential = GuestExecutionCredentialReferenceResolver.Resolve(request.CredentialRef);
        if (!credential.Ok)
        {
            return Reject(GuestFileJobProblemCodes.CredentialRefRequired);
        }

        if (request.SizeBytes is null or < 1 or > MaxSizeBytes)
        {
            return Reject(GuestFileJobProblemCodes.SizeLimit);
        }

        if (!TryNormalizeAllowedFile(request.HostPath, HostRoot, out var hostPath))
        {
            return Reject(GuestFileJobProblemCodes.PathNotAllowed);
        }

        if (!TryNormalizeAllowedFile(request.GuestPath, GuestPrefix, out var guestPath))
        {
            return Reject(GuestFileJobProblemCodes.PathNotAllowed);
        }

        return new GuestFileJobEvaluation(
            true,
            null,
            hostPath,
            guestPath,
            request.SizeBytes.Value,
            DirectionHostToGuest);
    }

    private static GuestFileJobEvaluation Reject(string code)
    {
        return new GuestFileJobEvaluation(false, code, null, null, 0, string.Empty);
    }

    private static bool ContainsHgfsToken(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return path.Contains(".hgfs", StringComparison.OrdinalIgnoreCase) ||
            path.Contains(@"\hgfs\", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/hgfs/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryNormalizeAllowedFile(string? path, string allowedPrefix, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var trimmed = path.Trim();
        if (trimmed.IndexOfAny(['*', '?', '"', '<', '>', '|']) >= 0)
        {
            return false;
        }

        if (trimmed.StartsWith(@"\\", StringComparison.Ordinal) ||
            trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        if (!Path.IsPathRooted(trimmed))
        {
            return false;
        }

        if (trimmed.IndexOf(':', 2) >= 0)
        {
            return false;
        }

        string full;
        try
        {
            full = Path.GetFullPath(trimmed);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }

        if (string.Equals(full, Path.TrimEndingDirectorySeparator(full) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(Path.GetFileName(full)))
        {
            return false;
        }

        var prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(allowedPrefix)) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        normalized = full;
        return true;
    }
}
