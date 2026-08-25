using System.Text;

namespace DesktopNode.Delivery.Tests.Infrastructure;

internal sealed class TemporaryContractTree : IDisposable
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly string parentPath;
    private readonly StringComparison pathComparison;
    private bool disposed;

    internal TemporaryContractTree()
    {
        parentPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pcv-delivery-contracts"))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        RootPath = Path.Combine(parentPath, Guid.NewGuid().ToString("N"));
        pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        Directory.CreateDirectory(RootPath);
    }

    internal string RootPath { get; }

    internal string WriteUtf8(string relativePath, string contents) =>
        WriteBytes(relativePath, StrictUtf8.GetBytes(contents));

    internal string WriteBytes(string relativePath, ReadOnlySpan<byte> contents)
    {
        var target = ResolveForWrite(relativePath);
        var parent = Path.GetDirectoryName(target)
            ?? throw DeliveryContractError.Invalid("fixtures/temp-tree", "path-containment");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(target, contents.ToArray());
        return target;
    }

    internal string ReadUtf8(string relativePath, string owner)
    {
        var normalizedOwner = DeliveryContractError.RequireOwner(owner);
        var target = ResolveRegularFile(relativePath, normalizedOwner);
        string result;
        try
        {
            result = StrictUtf8.GetString(File.ReadAllBytes(target));
        }
        catch (DecoderFallbackException error)
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "utf8", error);
        }

        var hasCrLf = result.Contains("\r\n", StringComparison.Ordinal);
        var withoutCrLf = result.Replace("\r\n", string.Empty, StringComparison.Ordinal);
        var hasBareLf = withoutCrLf.Contains('\n');
        var hasBareCr = withoutCrLf.Contains('\r');
        if ((hasCrLf && hasBareLf) || hasBareCr)
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "newline-policy");
        }

        return result;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        var resolvedParent = Path.GetFullPath(parentPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var resolvedRoot = Path.GetFullPath(RootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!resolvedRoot.StartsWith(resolvedParent + Path.DirectorySeparatorChar, pathComparison) ||
            Path.GetDirectoryName(resolvedRoot) != resolvedParent)
        {
            throw DeliveryContractError.Invalid("fixtures/temp-tree", "path-containment");
        }

        if (Directory.Exists(resolvedRoot))
        {
            Directory.Delete(resolvedRoot, recursive: true);
        }
    }

    private string ResolveForWrite(string relativePath)
    {
        var segments = ValidateSegments(relativePath, "fixtures/temp-tree");
        var target = Path.GetFullPath(segments.Aggregate(RootPath, Path.Combine));
        EnsureContained(target, "fixtures/temp-tree");
        return target;
    }

    private string ResolveRegularFile(string relativePath, string owner)
    {
        var segments = ValidateSegments(relativePath, owner);
        var cursor = RootPath;
        foreach (var segment in segments)
        {
            cursor = Path.Combine(cursor, segment);
            if (!File.Exists(cursor) && !Directory.Exists(cursor))
            {
                throw DeliveryContractError.Invalid(owner, "path-containment");
            }

            if ((File.GetAttributes(cursor) & FileAttributes.ReparsePoint) != 0)
            {
                throw DeliveryContractError.Invalid(owner, "symlink");
            }
        }

        var target = Path.GetFullPath(cursor);
        EnsureContained(target, owner);
        if (!File.Exists(target))
        {
            throw DeliveryContractError.Invalid(owner, "path-containment");
        }

        return target;
    }

    private string[] ValidateSegments(string relativePath, string owner)
    {
        if (string.IsNullOrWhiteSpace(relativePath) ||
            Path.IsPathRooted(relativePath) ||
            relativePath.Contains('\\') ||
            relativePath.Contains('\0'))
        {
            throw DeliveryContractError.Invalid(owner, "path-containment");
        }

        var segments = relativePath.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".."))
        {
            throw DeliveryContractError.Invalid(owner, "path-containment");
        }

        return segments;
    }

    private void EnsureContained(string target, string owner)
    {
        var boundary = RootPath + Path.DirectorySeparatorChar;
        if (!target.StartsWith(boundary, pathComparison))
        {
            throw DeliveryContractError.Invalid(owner, "path-containment");
        }
    }
}
