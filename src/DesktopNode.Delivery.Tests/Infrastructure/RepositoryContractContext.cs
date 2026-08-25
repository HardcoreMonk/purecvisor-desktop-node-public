using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace DesktopNode.Delivery.Tests.Infrastructure;

internal sealed class RepositoryContractContext
{
    private const string PathErrorCode = "PCV_DELIVERY_PATH_INVALID";
    private readonly string rootPath;
    private readonly StringComparison pathComparison;

    internal string RootPath => rootPath;

    private RepositoryContractContext(string rootPath)
    {
        this.rootPath = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }

    internal static RepositoryContractContext Find()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln")))
                {
                    return new RepositoryContractContext(directory.FullName);
                }

                directory = directory.Parent;
            }
        }

        throw new InvalidOperationException("PCV_DELIVERY_CONFIG_INVALID|repository-root-not-found");
    }

    internal string ReadUtf8Text(string repositoryRelativePath)
    {
        var path = ResolveRegularFile(repositoryRelativePath);
        try
        {
            return File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }
        catch (DecoderFallbackException error)
        {
            throw new InvalidDataException("PCV_DELIVERY_FILE_INVALID|utf8", error);
        }
    }

    internal JsonDocument LoadJson(string repositoryRelativePath)
    {
        try
        {
            return JsonDocument.Parse(
                ReadUtf8Text(repositoryRelativePath),
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                });
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("PCV_DELIVERY_JSON_INVALID|parse", error);
        }
    }

    internal XDocument LoadXml(string repositoryRelativePath)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };
        try
        {
            using var text = new StringReader(ReadUtf8Text(repositoryRelativePath));
            using var reader = XmlReader.Create(text, settings);
            return XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }
        catch (XmlException error)
        {
            throw new InvalidDataException("PCV_DELIVERY_XML_INVALID|parse", error);
        }
    }

    internal IReadOnlyList<string> EnumerateRegularFiles(
        string repositoryRelativeDirectory,
        string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
        {
            throw InvalidPath("suffix");
        }

        var directory = ResolveDirectory(repositoryRelativeDirectory);
        var results = new List<string>();
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            if (!file.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
            {
                throw InvalidPath("reparse-point");
            }

            results.Add(Path.GetRelativePath(rootPath, file).Replace('\\', '/'));
        }

        results.Sort(StringComparer.Ordinal);
        return results;
    }

    private string ResolveRegularFile(string repositoryRelativePath)
    {
        if (string.IsNullOrWhiteSpace(repositoryRelativePath) ||
            repositoryRelativePath.Contains('\\') ||
            repositoryRelativePath.Contains('\0') ||
            Path.IsPathRooted(repositoryRelativePath))
        {
            throw InvalidPath("format");
        }

        var segments = repositoryRelativePath.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".."))
        {
            throw InvalidPath("segment");
        }

        var combined = segments.Aggregate(rootPath, Path.Combine);
        var resolved = Path.GetFullPath(combined);
        var boundary = rootPath + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(boundary, pathComparison))
        {
            throw InvalidPath("containment");
        }

        var cursor = rootPath;
        foreach (var segment in segments)
        {
            cursor = Path.Combine(cursor, segment);
            if (!File.Exists(cursor) && !Directory.Exists(cursor))
            {
                throw InvalidPath("missing");
            }

            if ((File.GetAttributes(cursor) & FileAttributes.ReparsePoint) != 0)
            {
                throw InvalidPath("reparse-point");
            }
        }

        if (!File.Exists(resolved))
        {
            throw InvalidPath("not-file");
        }

        return resolved;
    }

    private string ResolveDirectory(string repositoryRelativePath)
    {
        if (string.IsNullOrWhiteSpace(repositoryRelativePath) ||
            repositoryRelativePath.Contains('\\') ||
            repositoryRelativePath.Contains('\0') ||
            Path.IsPathRooted(repositoryRelativePath))
        {
            throw InvalidPath("format");
        }

        var segments = repositoryRelativePath.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".."))
        {
            throw InvalidPath("segment");
        }

        var resolved = Path.GetFullPath(segments.Aggregate(rootPath, Path.Combine));
        var boundary = rootPath + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(boundary, pathComparison))
        {
            throw InvalidPath("containment");
        }

        var cursor = rootPath;
        foreach (var segment in segments)
        {
            cursor = Path.Combine(cursor, segment);
            if (!Directory.Exists(cursor))
            {
                throw InvalidPath("missing");
            }

            if ((File.GetAttributes(cursor) & FileAttributes.ReparsePoint) != 0)
            {
                throw InvalidPath("reparse-point");
            }
        }

        return resolved;
    }

    private static InvalidDataException InvalidPath(string detail) =>
        new($"{PathErrorCode}|{detail}");
}
